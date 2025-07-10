using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class EMCSJobDeclarationBaseOnlyTest : TestCaseWithFactory
	{
		public void TestMessageSendingConfiguration()
		{
			CombineAssertions(() =>
			{
				var configuration = declaration.MessageSendingConfiguration;
				AssertType<EMCSJobDeclarationMessageSendingConfiguration>(configuration);
				AssertSame("Cached", configuration, declaration.MessageSendingConfiguration);
			});
		}

		public void TestImportSADNumbers()
		{
			AssertType<ImportSADNumberCollection<ImportSADNumber>>(declaration.ImportSADNumbers);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var actualTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)declaration).GetCusSupportingInfoTypes();
			CombineAssertions(() =>
			{
				AssertEquals("ImportSADNumber", typeof(ImportSADNumber), actualTypes[CusSupportingInfoTypeList.Codes.ImportSad]);
				AssertEquals("EMCSDocument", typeof(EMCSDocument), actualTypes[CusSupportingInfoTypeList.Codes.Certificate]);
				AssertEquals("PreviousDocument", typeof(PreviousDocument), actualTypes[CusSupportingInfoTypeList.Codes.PreviousDocument]);
			});
		}

		public void TestChildType()
		{
			CombineAssertions(() =>
			{
				AssertType<EMCSInvoiceLineViewCollection>("Filtered Invoice Lines", declaration.FilteredInvoiceLines);
				AssertType<EMCSInvoiceHeaderActiveCollection>("Invoices", declaration.Invoices);
				AssertType<BaseJobComInvoiceGroupHeaderCollection<EMCSJobComInvoiceGroupHeader>>("Invoice Group Headers", declaration.JobComInvoiceGroupHeaders);
				AssertType<EMCSInvoiceLineCompleteCollection>("Invoice Lines", declaration.InvoiceLines);
			});
		}

		public void TestSupplierDocAddressIsNotLinkedToSupplierPickupAddress()
		{
			//EMCS does not need Supplier Pickup Address it is not displayed
			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgAddress>().PK;
			declaration.SupplierDocumentaryAddress.E2_AddressOverride = false;
			declaration.SupplierPickupAddress.Validation.ValidateE2_CompanyName();
			AssertNoNotifications("Company Name is mandatory", declaration.SupplierPickupAddress.E2_CompanyNameInfo);
		}

		public void TestImporterDocAddressIsNotLinkedToImporterPickupAddress()
		{
			//EMCS does not need Supplier Pickup Address it is not displayed
			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgAddress>().PK;
			declaration.ImporterDocumentaryAddress.E2_AddressOverride = false;
			declaration.ImporterDeliveryAddress.Validation.ValidateE2_CompanyName();
			AssertNoNotifications("Company Name is mandatory", declaration.ImporterDeliveryAddress.E2_CompanyNameInfo);
		}

		public void TestValidation()
		{
			AssertType<EMCSJobDeclarationValidation>(declaration.Validation);
		}

		public void TestLookups()
		{
			AssertType<EMCSJobDeclarationLookups>(declaration.Lookups);
		}

		public void TestGetCusCodeDataType()
		{
			AssertEquals(typeof(OfficeCode), ((Integration.Customs.ICusCodeDataTypeSupporter)declaration).GetCusCodeDataTypes()[EU.Business.CusCodeDataTypeList.Codes.OfficeCode]);
		}

		public void TestGetCustomsOffices()
		{
			var customsOffices = declaration.CustomsOffices;
			CombineAssertions(() =>
			{
				AssertType<OfficeCodeCollection<OfficeCode>>("Type", customsOffices);
				AssertEquals("Default", 1, customsOffices.Count);
				AssertEquals("DefaultPurposeCode", OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch, customsOffices.DefaultPurposeCode);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSJobDeclaration))]
	class EMCSJobDeclarationTest : EU.EMCS.Business.Testing.EMCSJobDeclarationTest
	{
		public void TestAddInfoLookups()
		{
			AssertType<EMCSAddInfoJobDeclarationLookups>(declaration.AddInfoLookups);
		}

		public void TestChildType()
		{
			AssertType<EU.EMCS.Business.EMCSInvoiceLineViewCollection>(declaration.FilteredInvoiceLines);
			AssertType<EU.EMCS.Business.EMCSInvoiceHeaderActiveCollection>(declaration.Invoices);
			AssertType<BaseJobComInvoiceGroupHeaderCollection<EU.EMCS.Business.EMCSJobComInvoiceGroupHeader>>(declaration.JobComInvoiceGroupHeaders);
			AssertType<EU.EMCS.Business.EMCSInvoiceLineCompleteCollection>(declaration.InvoiceLines);
			AssertType<EMCSAddInfoJobDeclaration>(declaration.AddInfo);
			AssertType<EMCSCusContainerCollection>(declaration.CusContainers);
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
			AssertEquals(typeof(EMCSOfficeCode), ((Integration.Customs.ICusCodeDataTypeSupporter)declaration).GetCusCodeDataTypes()[EU.Business.CusCodeDataTypeList.Codes.OfficeCode]);
		}

		public void TestGetCustomsOffices()
		{
			var customsOffices = declaration.CustomsOffices;
			CombineAssertions(() =>
			{
				AssertType<EMCSOfficeCodeCollection>("EMCSOfficeCodeCollection type", customsOffices);
				AssertEquals("CustomsOffices count", 1, customsOffices.Count);
				AssertEquals("DefaultPurposeCode", OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch, customsOffices.DefaultPurposeCode);
			});
		}

		public void TestJourneyTimeGetsDefaultedIfDeclarationIsConsolidatedDocument()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No ConsolidatedDocument: Empty JourneyTime", ZString.Empty, declaration.ZG_JourneyTime);
				declaration.SetConsolidatedDocument();
				AssertEquals("ConsolidatedDocument: JourneyTimeNumericPart = 45", 45, declaration.JourneyTimeNumericPart);
				AssertEquals("ConsolidatedDocument: JourneyTimeFormatPart = days", EU.EMCS.Business.JourneyTimeUnitList.Codes.Days, declaration.JourneyTimeFormatPart);
			});
		}

		public void TestTransportArrangementGetsDefaultedIfDeclarationIsConsolidatedDocument()
		{
			CombineAssertions(() =>
			{
				declaration.ZG_TransportArrangement = ZString.Empty;
				declaration.SetConsolidatedDocument();
				AssertEquals("ConsolidatedDocument: TransportArrangement == 1", EU.EMCS.Business.EMCSTransportArrangementList.Codes.Consignor, declaration.ZG_TransportArrangement);
			});
		}

		public void TestPiggyBackedDocAddressValidation()
		{
			AssertType<EMCSJobDeclarationJobDocAddressValidation>(declaration.PiggyBackedDocAddressValidation(Factory.New<JobDocAddress>()));
		}

		public void TestImportSADNumbers()
		{
			AssertType<ImportSADNumberCollection>(declaration.ImportSADNumbers);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var supportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)declaration).GetCusSupportingInfoTypes();
			AssertEquals("ImportSADNumber", typeof(ImportSADNumber), supportingInfoTypes[CusSupportingInfoTypeList.Codes.ImportSad]);
		}

		public void TestGetCusContainerType()
		{
			AssertEquals(typeof(EMCSCusContainer), ((ICusContainerTypeSupporter)declaration).GetCusContainerType());
		}

		public void TestSequenceNumber_Get()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default Value", ZString.Empty, declaration.SequenceNumber);
				AssertNull("No Entry Number created", CusEntryNumber.Load(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany));
				var eadNumber = CusEntryNumber.New(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
				eadNumber.CE_EntryLineReference = "234";
				AssertEquals("Ead Number Entry line Reference", "234", declaration.SequenceNumber);
			});
		}

		public void TestSequenceNumber_Set()
		{
			declaration.SequenceNumber = "498";
			var eadNumber = CusEntryNumber.Load(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			AssertEquals("498", eadNumber.CE_EntryLineReference);
		}

		public void TestZG_GuarantorType_MaxLength_DeclarantTypeIsConsignor()
		{
			declaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor;
			AssertEquals("MaxLength", 1, declaration.ZG_GuarantorTypeInfo.MaxLength);
		}

		public void TestZG_GuarantorType_MaxLength_DeclarantTypeIsConsignee()
		{
			declaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignee;
			AssertEquals("MaxLength", 4, declaration.ZG_GuarantorTypeInfo.MaxLength);
		}

		public void TestJourneyTimeFormatPartReadOnlyIfDeclarationIsConsolidatedDocument()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No ConsolidatedDocument", false, declaration.JourneyTimeFormatPartInfo.ReadOnly);
				declaration.SetConsolidatedDocument();
				AssertEquals("Is ConsolidatedDocument", true, declaration.JourneyTimeFormatPartInfo.ReadOnly);
			});
		}

		public void TestJourneyTimeNumericPartReadOnlyIfDeclarationIsConsolidatedDocument()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No ConsolidatedDocument", false, declaration.JourneyTimeNumericPartInfo.ReadOnly);
				declaration.SetConsolidatedDocument();
				AssertEquals("Is ConsolidatedDocument", true, declaration.JourneyTimeNumericPartInfo.ReadOnly);
			});
		}

		public void TestEMCSPackageType()
		{
			var package = declaration.EMCSPackages.AddNew();
			AssertType<EMCSPackage>(package);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
		}

		EMCSJobDeclaration declaration;

		protected override BusinessObject GetNewBusinessObject() => declaration;
	}
}

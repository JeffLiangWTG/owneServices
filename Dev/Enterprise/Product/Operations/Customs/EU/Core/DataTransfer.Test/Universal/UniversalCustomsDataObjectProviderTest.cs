using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	partial class UniversalCustomsDataObjectProviderTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGuarnateesCollectionEndToEnd()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IEDUB100", "DUBLIN PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeGB000001 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "GB000001", "Central Community Transit Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeGB000001.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			Factory.SaveForTesting();
			var declaration = Factory.New<JobDeclaration>();
			var customsOffice = declaration.CustomsOffices.AddNew();
			customsOffice.CY_Data = "GB000001";
			var guarantee = declaration.Guarantees.AddNew();
			guarantee.PW_ActivityCode = "123";
			guarantee.PW_BondNumber = "123";
			guarantee.PW_BondFiledPort = customsOffice.CY_Data;
			Factory.SaveForTesting();
			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			var dataObject = (Shipment)Enterprise.MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			AssertEquals("dataObject.CustomsReferenceCollection.Count", 1, dataObject.GuaranteeCollection.Count);
			var customsReference = dataObject.GuaranteeCollection[0];
			AssertEquals("customsReference.Type.Code", "GB000001", customsReference.BondFiledPort.Code);
		}

		public void TestGetNewDeclarationDataObjectWriter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var provider = new UniversalCustomsDataObjectProvider();
			AssertType<DeclarationDataObjectWriter>(provider.GetNewDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(null, declaration))));
		}

		public void TestGetNewUniversalDataObjectReaderHelper()
		{
			var provider = new UniversalCustomsDataObjectProvider();
			var readerHelper = provider.GetNewUniversalDataObjectReaderHelper(new UniversalObjectFactory(new BusinessObjectFactory()), Core.Constants.CountryCodes.Germany);
			AssertType<UniversalDataObjectReaderHelper>("helper should in right type", readerHelper);
		}

		public void TestTableSpecificCusCodeDataTypeList()
		{
			var provider = new UniversalCustomsDataObjectProvider();

			var cusCodeDataTypeList = provider.TableSpecificCusCodeDataTypeList(JobDeclarationSchema.Constants.Prefix, string.Empty);
			AssertEquals(1, cusCodeDataTypeList.Count);
			AssertEquals(true, cusCodeDataTypeList.ContainsCode(Business.CusCodeDataTypeList.Codes.OfficeCode));

			cusCodeDataTypeList = provider.TableSpecificCusCodeDataTypeList(string.Empty, string.Empty);
			AssertNull(cusCodeDataTypeList);
		}

		public void TestTableSpecificCusSupportingInfoTypeListEndToEnd()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var additionalInfo = invoice.AdditionalInfos.AddNew();
			additionalInfo.CSI_Description = "1";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var tax = Factory.New<Customs.Business.CusSupportingInfo>();
			tax.CSI_Type = "AGA";
			tax.CSI_ParentID = invoice.PK;
			tax.CSI_ParentTableCode = invoice.TablePrefix;
			Factory.SaveForTesting();
			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			var dataObject = (Shipment)Enterprise.MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			AssertEquals(1, dataObject.CommercialInfo.CommercialInvoiceCollection.Count);
			var commercialInvoice = dataObject.CommercialInfo.CommercialInvoiceCollection[0];
			AssertEquals("AGA is not a supported type for Invoice", 1, commercialInvoice.CustomsSupportingInformationCollection.Count);
			var customsSupportingInformation = commercialInvoice.CustomsSupportingInformationCollection[0];
			AssertEquals("customsSupportingInformation.Category.Code", Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, customsSupportingInformation.Category.GetCodeAsUpperCase());
			AssertEquals("customsSupportingInformation.Category.Description", Common.EU.CusSupportingInfoTypeList.Descriptions.AdditionalInfo, customsSupportingInformation.Category.Description.GetValueOrDefault());
		}

		public void TestTableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList()
		{
			AssertNull(new UniversalCustomsDataObjectProvider().TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(JobDeclarationSchema.Constants.Prefix, ""));
		}

		public void TestTableSpecificCusReferenceTypeList()
		{
			var provider = new UniversalCustomsDataObjectProvider();

			var cusReferenceDataTypeList = provider.TableSpecificCusReferenceTypeList(CusInBondCargoDescSchema.Constants.Prefix, string.Empty);
			AssertEquals(1, cusReferenceDataTypeList.Count);
			AssertEquals(true, cusReferenceDataTypeList.ContainsCode(Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor));

			cusReferenceDataTypeList = provider.TableSpecificCusReferenceTypeList(string.Empty, string.Empty);
			AssertNull(cusReferenceDataTypeList);

			AssertNull(provider.TableSpecificCusReferenceTypeList(CusEntryInstructionSchema.Constants.Prefix, string.Empty));
		}
	}
}

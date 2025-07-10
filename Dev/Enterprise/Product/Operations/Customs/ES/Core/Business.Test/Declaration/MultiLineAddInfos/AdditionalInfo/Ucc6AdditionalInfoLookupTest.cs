using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	sealed class Ucc6AdditionalInfoLookupTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList_Export()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, "Spain");

			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalInformation, "Additional Information Code Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalInformation, "AI1", "AddInfo1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument, "Transport Document Code Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument, "TD1", "TrDoc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalReference, "Additional Reference Code Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalReference, "AR1", "AddRef1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions("For Export.", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					ArrangeAndAssertAddInfo(nameof(JobComInvoiceHeader), invoiceAdditionalInfo);
					ArrangeAndAssertAddInfo(nameof(JobComInvoiceLine), invoiceLineAdditionalInfo);
					ArrangeAndAssertAddInfo(nameof(JobDeclaration), declarationAdditionalInfo);
					ArrangeAndAssertAddInfo(nameof(CusEntryInstruction), entryInstructionAdditionalInfo);
				}
			});
		}

		public void TestCodeList_Import()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, "Spain");

			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportAdditionalInformation, "Additional Information Code Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportAdditionalInformation, "AI1", "AddInfo1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument, "Transport Document Code Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument, "TD1", "TrDoc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportAdditionalReference, "Additional Reference Code Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportAdditionalReference, "AR1", "AddRef1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions("For Import.", () =>
			{
				using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
				{
					ArrangeAndAssertAddInfo(nameof(JobComInvoiceHeader), invoiceAdditionalInfo);
					ArrangeAndAssertAddInfo(nameof(JobComInvoiceLine), invoiceLineAdditionalInfo);
					ArrangeAndAssertAddInfo(nameof(JobDeclaration), declarationAdditionalInfo);
					ArrangeAndAssertAddInfo(nameof(CusEntryInstruction), entryInstructionAdditionalInfo);
				}
			});
		}

		void ArrangeAndAssertAddInfo(string parentEntityName, AdditionalInfo additionalInfo)
		{
			additionalInfo.CSI_SubType = ZString.Empty;
			AssertType<CodeDescriptionPairList>($"Empty Value Additional Info for {parentEntityName}", additionalInfo.Lookups.CodeList);

			additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertCodeListValues(EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation, "AI1", additionalInfo, parentEntityName);

			additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			AssertCodeListValues(EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument, "TD1", additionalInfo, parentEntityName);

			additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			AssertCodeListValues(EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference, "AR1", additionalInfo, parentEntityName);
		}

		void AssertCodeListValues(string subStyle, string expectedCode, AdditionalInfo additionalInfo, string parentEntityName)
		{
			var codeList = (ZZRefCusCodeListCombinedCollection)additionalInfo.Lookups.CodeList;
			codeList.Load();
			Assert($"AdditionalInfo for {parentEntityName} and values for {subStyle}.", codeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code).Contains(expectedCode));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			invoice = declaration.Invoices.AddNew();
			invoiceAdditionalInfo = invoice.AdditionalInfos.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLineAdditionalInfo = invoiceLine.AdditionalInfos.AddNew();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstructionAdditionalInfo = entryInstruction.AdditionalInfos.AddNew();
			declarationAdditionalInfo = declaration.AdditionalInfos.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		CusEntryInstruction entryInstruction;
		AdditionalInfo invoiceLineAdditionalInfo, invoiceAdditionalInfo, entryInstructionAdditionalInfo, declarationAdditionalInfo;
	}
}

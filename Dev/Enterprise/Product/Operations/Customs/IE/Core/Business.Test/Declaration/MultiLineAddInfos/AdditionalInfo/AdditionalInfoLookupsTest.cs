using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Business.MasterFiles;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class AdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList_Declaration_Export()
		{
			SetUpAdditioanlInformationCodes();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var additionalInfo = declaration.AdditionalInfos.AddNew();

			AssertCodeList(additionalInfo, false);
		}

		public void TestCodeList_Declaration_Import()
		{
			SetUpAdditioanlInformationCodes();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var additionalInfo = declaration.AdditionalInfos.AddNew();

			AssertCodeList(additionalInfo, true);
		}

		public void TestCodeList_Invoice_Export()
		{
			SetUpAdditioanlInformationCodes();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var invoice = declaration.Invoices.AddNew();
			var additionalInfo = invoice.AdditionalInfos.AddNew();

			AssertCodeList(additionalInfo, false);
		}

		public void TestCodeList_Invoice_Import()
		{
			SetUpAdditioanlInformationCodes();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			var additionalInfo = invoice.AdditionalInfos.AddNew();

			AssertCodeList(additionalInfo, true);
		}

		public void TestCodeList_InvoiceLine_Export()
		{
			SetUpAdditioanlInformationCodes();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			var additionalInfo = invoiceLine.AdditionalInfos.AddNew();

			AssertCodeList(additionalInfo, false);
		}

		public void TestCodeList_InvoiceLine_Import()
		{
			SetUpAdditioanlInformationCodes();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			var additionalInfo = invoiceLine.AdditionalInfos.AddNew();

			AssertCodeList(additionalInfo, true);
		}

		public void TestCodeList_EntryInstruction_Export()
		{
			SetUpAdditioanlInformationCodes();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var additionalInfo = declaration.CustomsEntryInstructions.AddNew().AdditionalInfos.AddNew();

			AssertCodeList(additionalInfo, false);
		}

		public void TestCodeList_EntryInstruction_Import()
		{
			SetUpAdditioanlInformationCodes();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var additionalInfo = declaration.CustomsEntryInstructions.AddNew().AdditionalInfos.AddNew();

			AssertCodeList(additionalInfo, true);
		}

		public void TestCodeList_AR44I_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
			var list = (ZZRefCusCodeListCombinedCollection)additionalInfo.Lookups.CodeList;
			var filter = list.CompleteFilter;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportAdditionalReference, "AddRefImp");
			var aR2Code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportAdditionalReference, "AR2", "AddRefImp", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			AssertEquals("AR2", true, Factory.Load<ZZRefCusCodeListCombined>(aR2Code.PK).MatchesFilter(filter));
		}

		void AssertCodeList(AdditionalInfo additionalInfo, bool isImport)
		{
			CombineAssertions(() =>
			{
				additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				var list = (ZZRefCusCodeListCombinedCollection)additionalInfo.Lookups.CodeList;
				list.Load();
				Assert("Values for INF", string.Join(",", list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code)).Equals("AI1"));

				additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
				list = (ZZRefCusCodeListCombinedCollection)additionalInfo.Lookups.CodeList;
				list.Load();
				Assert("Values for REF", string.Join(",", list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code)).Equals("AR1"));

				additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
				list = (ZZRefCusCodeListCombinedCollection)additionalInfo.Lookups.CodeList;
				list.Load();
				Assert("Values for TRA", string.Join(",", list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code)).Equals(isImport ? "TD2" : "TD1"));
			});
		}

		void SetUpAdditioanlInformationCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland");

			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalInformation, "AddInfo");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalInformation, "AI1", "AddInfo1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportAdditionalInformation, "AI1", "AddInfo1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalReference, "AddRef");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalReference, "AR1", "AddRef", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportAdditionalReference, "AR1", "AddRef", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument, "TrDoc");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument, "TD1", "TrDoc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument, "TrImp");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument, "TD2", "TrImp", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		public void TestSubTypeList_UCC5AndImport()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			{
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

				var list = new AdditionalInfoLookups(invoiceAdditionalInfo).SubTypeList;
				AssertArrayEqualsByElements("List for Invoice", new[] { "INF" }, list.GetAllCodes());

				list = new AdditionalInfoLookups(invoiceLineAdditionalInfo).SubTypeList;
				AssertArrayEqualsByElements("List for Invoice Line", new[] { "INF" }, list.GetAllCodes());

				list = new AdditionalInfoLookups(declarationAdditionalInfo).SubTypeList;
				AssertArrayEqualsByElements("List for declaration", new[] { "INF" }, list.GetAllCodes());
			}
		}

		public void TestSubTypeList_UCC6()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var list = new AdditionalInfoLookups(invoiceAdditionalInfo).SubTypeList;
				AssertArrayEqualsByElements("List for Invoice", new[] { "INF", "REF", "TRA" }, list.GetAllCodes());

				list = new AdditionalInfoLookups(invoiceLineAdditionalInfo).SubTypeList;
				AssertArrayEqualsByElements("List for Invoice Line", new[] { "INF", "REF", "TRA" }, list.GetAllCodes());

				list = new AdditionalInfoLookups(declarationAdditionalInfo).SubTypeList;
				AssertArrayEqualsByElements("List for declaration", new[] { "INF", "REF" }, list.GetAllCodes());

				list = new AdditionalInfoLookups(Factory.New<CusClassPartPivot>().AdditionalInfos.AddNew()).SubTypeList;
				AssertArrayEqualsByElements("List for CusClassPartPivot", new[] { "INF", "REF", "TRA" }, list.GetAllCodes());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declarationAdditionalInfo = declaration.AdditionalInfos.AddNew();

			invoice = declaration.Invoices.AddNew();
			invoiceAdditionalInfo = invoice.AdditionalInfos.AddNew();

			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLineAdditionalInfo = invoiceLine.AdditionalInfos.AddNew();
		}

		JobDeclaration declaration;
		AdditionalInfo declarationAdditionalInfo;
		JobComInvoiceHeader invoice;
		AdditionalInfo invoiceAdditionalInfo;
		JobComInvoiceLine invoiceLine;
		AdditionalInfo invoiceLineAdditionalInfo;
	}
}

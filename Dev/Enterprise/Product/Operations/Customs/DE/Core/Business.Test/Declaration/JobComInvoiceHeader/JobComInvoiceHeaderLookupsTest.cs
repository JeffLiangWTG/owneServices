using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class JobComInvoiceHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestJZ_IncoTerm_List_Import()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var codeList = header.Lookups.JZ_IncoTerm_List;
			CombineAssertions(() =>
			{
				AssertSame("Cached", header.Lookups.JZ_IncoTerm_List, codeList);
				AssertEquals("Codes", "CFR, CIF, CIP, CPT, DAP, DAT, DDP, DPU, EXW, FAS, FCA, FOB, XXX", codeList.CodesAsString);
			});
		}

		public void TestJZ_IncoTerm_List_Export()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var codeList = header.Lookups.JZ_IncoTerm_List;
			CombineAssertions(() =>
			{
				AssertSame("Cached", header.Lookups.JZ_IncoTerm_List, codeList);
				AssertType<IncoTermsCodeDescriptionPairList>("Type", codeList);
			});
		}

		public void TestIncoTermPlaceList()
		{
			var result = header.Lookups.IncoTermPlaceList;
			CombineAssertions(() =>
			{
				AssertType<RefUNLOCOCollection>("Type", result);
				AssertSame("Cached", result, header.Lookups.IncoTermPlaceList);
			});
		}

		public void TestValuationCodeList_Import()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "TranNature");
			helper.CreateNewOrGetExistingCusCodeType("DC000", "Invalid Type");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
				"TranNature01", "Valid list", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.A1150, RefCusCodeListAttributes.Value.Yes);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
				"TranNature02", "Invalid country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.A1150, RefCusCodeListAttributes.Value.Yes);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
				"TranNature03", "Invalid attribute value", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.A1150, RefCusCodeListAttributes.Value.No);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, "DC000",
				"TranNature04", "Invalid list type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.A1150, RefCusCodeListAttributes.Value.Yes);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
				"TranNature05", "No attribute", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
				"TranNature06", "Invalid expiry date", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-1), RefCusCodeListAttributes.Name.A1150, RefCusCodeListAttributes.Value.Yes);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
				"TranNature07", "Invalid attribute", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.C0091, RefCusCodeListAttributes.Value.Yes);
			Factory.Save();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var valuationCodeList = (CodeDescriptionPairList)header.Lookups.ValuationCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "TranNature01", valuationCodeList.CodesAsString);
				AssertSame("Cached", valuationCodeList, header.Lookups.ValuationCodeList);
			});
		}

		public void TestValuationCodeList_Export()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "TranNature");
			helper.CreateNewOrGetExistingCusCodeType("DC000", "Invalid Type");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
				"TranNature04", "Valid list 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.A1150, RefCusCodeListAttributes.Value.Yes);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
				"TranNature05", "Valid list 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.C0091, RefCusCodeListAttributes.Value.Yes);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
				"TranNature06", "Invalid country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.A1150, RefCusCodeListAttributes.Value.Yes);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
				"TranNature07", "Invalid attribute value", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.A1150, RefCusCodeListAttributes.Value.No);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, "DC000",
				"TranNature08", "Invalid list type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.A1150, RefCusCodeListAttributes.Value.Yes);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
				"TranNature09", "No attribute", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
				"TranNature10", "Invalid expiry date", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-1), RefCusCodeListAttributes.Name.A1150, RefCusCodeListAttributes.Value.Yes);
			Factory.Save();

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var valuationCodeList = (CodeDescriptionPairList)header.Lookups.ValuationCodeList;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "TranNature04", "TranNature05" }, valuationCodeList.GetAllCodes());
				AssertSame("Cached", valuationCodeList, header.Lookups.ValuationCodeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			header = Factory.New<JobComInvoiceHeader>();
			declaration.Invoices.Add(header);
		}
		JobComInvoiceHeader header;
		JobDeclaration declaration;
	}
}

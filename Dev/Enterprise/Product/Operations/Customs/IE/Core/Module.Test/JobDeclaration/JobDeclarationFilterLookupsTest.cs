using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.Module.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.Module.Testing
{
	sealed class JobDeclarationFilterLookupsTest : CommonFilterLookupsTest
	{
		public void TestMessageStatusList()
		{
			var parent = new JobDeclarationFilterBusinessObject();
			AssertSame(parent.Factory.GetCachedValue<IELogicalStatusList>(), new JobDeclarationFilterLookups(parent).MessageStatusList());
		}

		public void TestDeclarationTypeList()
		{
			var parent = new JobDeclarationFilterBusinessObject();
			var list1 = new JobDeclarationFilterLookups(parent).DeclarationTypeList;
			var list2 = new JobDeclarationFilterLookups(parent).DeclarationTypeList;
			AssertSame(list1, list2);
			AssertEquals("CodesAsString", "A1, A2, A3, B1, B2, B3, B4, C1, H1, H2, H3, H4, H5, H6, H7, I1", list2.CodesAsString);
		}

		public void TestApplicationCodeList()
		{
			CombineAssertions(() =>
			{
				var applicationCodeList = lookups.ApplicationCodeList();
				AssertEquals("Codes", "V1, V2, ITF", applicationCodeList.CodesAsString);
				AssertSame("ApplicationCodeList is cached.", applicationCodeList, filterBizObj.Factory.GetCachedValue<ImportDeclarationApplicationCodeList>());
			});
		}

		public void TestApplicationCodeSearchFilterList()
		{
			CombineAssertions(() =>
			{
				var applicationCodeList = lookups.ApplicationCodeSearchFilterList();
				AssertEquals("Codes", "V1, V2, ITF, BLT", applicationCodeList.CodesAsString);
				AssertSame("ApplicationCodeList is cached.", applicationCodeList, filterBizObj.Factory.GetCachedValue<DeclarationApplicationSearchFilterCodeList>());
			});
		}

		public void TestGetDataGroupingCodesForSupportingDocumentList()
		{
			var lookups = new JobDeclarationFilterLookups(null);
			var result = (ZString[])lookups.GetType().GetProperty("GetDataGroupingCodesForSupportingDocumentList", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(lookups);
			CombineAssertions(() =>
			{
				AssertEquals("Length", 2, result.Length);
				AssertContainsExactElementsInAnyOrder("Values", new ZString[] { Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefDataGrouping.Codes.IEUcc5 }, result);
			});
		}

		protected override FilterStripBusinessObject GetFilterStripBusinessObject() => new JobDeclarationFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			filterBizObj = new JobDeclarationFilterBusinessObject();
			lookups = new JobDeclarationFilterLookups(filterBizObj);
		}

		JobDeclarationFilterBusinessObject filterBizObj;
		JobDeclarationFilterLookups lookups;
	}
}

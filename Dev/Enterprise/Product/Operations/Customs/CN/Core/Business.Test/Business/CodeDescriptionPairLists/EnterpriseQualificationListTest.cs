using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class EnterpriseQualificationListTest : TestCaseWithFactory
	{
		public void TestGetCachedEnterpriseQualificationList()
		{
			var testList = EnterpriseQualificationListHelper.GetCachedEnterpriseQualificationList(Factory, true);
			var testList2 = EnterpriseQualificationListHelper.GetCachedEnterpriseQualificationList(Factory, true);
			Assert("Should have been cached.", testList.Equals(testList2));
			AssertEquals(34, testList.Count);
			Assert(testList.ContainsCode("100"));
			Assert(testList.ContainsCode("101"));
			Assert(testList.ContainsCode("200"));
			Assert(testList.ContainsCode("300"));
			Assert(testList.ContainsCode("303"));
			Assert(testList.ContainsCode("306"));
			Assert(testList.ContainsCode("307"));
			Assert(testList.ContainsCode("312"));
			Assert(testList.ContainsCode("317"));
			Assert(testList.ContainsCode("319"));
			Assert(testList.ContainsCode("320"));
			Assert(testList.ContainsCode("321"));
			Assert(testList.ContainsCode("322"));
			Assert(testList.ContainsCode("326"));
			Assert(testList.ContainsCode("327"));
			Assert(testList.ContainsCode("400"));
			Assert(testList.ContainsCode("413"));
			Assert(testList.ContainsCode("414"));
			Assert(testList.ContainsCode("415"));
			Assert(testList.ContainsCode("416"));
			Assert(testList.ContainsCode("418"));
			Assert(testList.ContainsCode("421"));
			Assert(testList.ContainsCode("500"));
			Assert(testList.ContainsCode("508"));
			Assert(testList.ContainsCode("509"));
			Assert(testList.ContainsCode("510"));
			Assert(testList.ContainsCode("511"));
			Assert(testList.ContainsCode("513"));
			Assert(testList.ContainsCode("515"));
			Assert(testList.ContainsCode("524"));
			Assert(testList.ContainsCode("600"));
			Assert(testList.ContainsCode("601"));
			Assert(testList.ContainsCode("603"));
			Assert(testList.ContainsCode("700"));
			testList = EnterpriseQualificationListHelper.GetCachedEnterpriseQualificationList(Factory, false);
			testList2 = EnterpriseQualificationListHelper.GetCachedEnterpriseQualificationList(Factory, false);
			Assert("Should have been cached.", testList.Equals(testList2));
			AssertEquals(39, testList.Count);
			Assert(testList.ContainsCode("100"));
			Assert(testList.ContainsCode("101"));
			Assert(testList.ContainsCode("102"));
			Assert(testList.ContainsCode("200"));
			Assert(testList.ContainsCode("300"));
			Assert(testList.ContainsCode("301"));
			Assert(testList.ContainsCode("302"));
			Assert(testList.ContainsCode("304"));
			Assert(testList.ContainsCode("305"));
			Assert(testList.ContainsCode("308"));
			Assert(testList.ContainsCode("309"));
			Assert(testList.ContainsCode("310"));
			Assert(testList.ContainsCode("311"));
			Assert(testList.ContainsCode("315"));
			Assert(testList.ContainsCode("317"));
			Assert(testList.ContainsCode("318"));
			Assert(testList.ContainsCode("323"));
			Assert(testList.ContainsCode("324"));
			Assert(testList.ContainsCode("329"));
			Assert(testList.ContainsCode("400"));
			Assert(testList.ContainsCode("415"));
			Assert(testList.ContainsCode("417"));
			Assert(testList.ContainsCode("418"));
			Assert(testList.ContainsCode("419"));
			Assert(testList.ContainsCode("421"));
			Assert(testList.ContainsCode("500"));
			Assert(testList.ContainsCode("501"));
			Assert(testList.ContainsCode("502"));
			Assert(testList.ContainsCode("503"));
			Assert(testList.ContainsCode("504"));
			Assert(testList.ContainsCode("505"));
			Assert(testList.ContainsCode("506"));
			Assert(testList.ContainsCode("507"));
			Assert(testList.ContainsCode("512"));
			Assert(testList.ContainsCode("514"));
			Assert(testList.ContainsCode("520"));
			Assert(testList.ContainsCode("602"));
			Assert(testList.ContainsCode("603"));
			Assert(testList.ContainsCode("700"));
		}
	}
}

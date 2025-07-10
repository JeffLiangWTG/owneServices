using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CNOrgImpAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageSubTypeList()
		{
			var testObj = CNOrgImpAddInfo.Get(Factory.New<OrgHeader>());
			var testList = testObj.Lookups.MessageSubTypeList;
			AssertEquals("MessageSubTypeList should have 2 items.", 2, testList.Count);
			AssertEquals("Should have CUS item with correct description.", "报关单", testList.GetDescriptionFromCode("CUS"));
			AssertEquals("Should have REC item with correct description.", "备案清单", testList.GetDescriptionFromCode("REC"));
			AssertSame("cached", testList, testObj.Lookups.MessageSubTypeList);
		}

		public void TestIntelligentDeclarationTypeList()
		{
			var testObj = CNOrgImpAddInfo.Get(Factory.New<OrgHeader>());
			var testList = testObj.Lookups.IntelligentDeclarationTypeList;
			AssertContainsExactElementsInExactOrder(new[] { "0", "1", "2" }, testList.GetAllCodes());
			AssertSame("cached", testList, testObj.Lookups.IntelligentDeclarationTypeList);
		}
	}
}

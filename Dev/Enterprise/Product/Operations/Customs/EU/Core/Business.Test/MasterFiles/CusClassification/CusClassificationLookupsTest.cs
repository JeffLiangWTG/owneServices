using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.MasterFiles.Testing
{
	class CusClassificationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestClassification()
		{
			CusClassification parent = Factory.New<CusClassification>();
			AssertEquals(parent.Lookups.Classification, parent);
		}

		public void TestCPCs()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;  // Latvia for base EU tests. 
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP");
			helper.CreateRefCusProcedure(currentCountry, "A", "22", "22", "222", "Two", "IMP");
			helper.CreateRefCusProcedure(currentCountry, "A", "33", "33", "333", "Three", "EXP");
			var op = OrgSupplierPart.New(Factory);

			var cc = Factory.New<CusClassification>();
			var cpcs = cc.Lookups.CPCs;
			Assert(cpcs.ContainsCode("1111111"));
			Assert(cpcs.ContainsCode("2222222"));
			Assert(cpcs.ContainsCode("3333333"));
		}
	}
}

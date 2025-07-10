using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(PackingGroup))]
	public class TestPackingGroup : Customs.Business.Testing.BasePackingGroupTest
	{
		public new void TestGetConvertedPackType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalNoOfPacks = 10;
			declaration.JE_TotalNoOfPacksPackType = "BAG";
			declaration.JE_HouseBill = "HB1";
			var packingGroup = declaration.PrimaryHouseBill.PackingGroups[0];
			var package = packingGroup.Packages[0];
			AssertEquals(10, package.CW_PackQty);
			AssertEquals("BG", package.CW_PackType);

			declaration.JE_TotalNoOfPacksPackType = "CS";
			packingGroup = declaration.PrimaryHouseBill.PackingGroups[0];
			package = packingGroup.Packages[0];
			AssertEquals(10, package.CW_PackQty);
			AssertEquals("CS", package.CW_PackType);

			declaration.JE_TotalNoOfPacksPackType = "G";
			packingGroup = declaration.PrimaryHouseBill.PackingGroups[0];
			package = packingGroup.Packages[0];
			AssertEquals(10, package.CW_PackQty);
			AssertEquals("PK", package.CW_PackType);
		}

		public void TestTypeDecider()
		{
			Assert("Update BasePackingGroup to include a decider for this class", Factory.New(typeof(BasePackingGroup)).GetType() == GetExpectedBusinessObjectType());
		}
	}
}

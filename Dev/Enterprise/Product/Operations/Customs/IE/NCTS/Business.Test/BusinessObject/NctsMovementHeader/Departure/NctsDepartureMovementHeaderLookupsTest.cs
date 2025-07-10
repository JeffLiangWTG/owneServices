using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class NctsDepartureMovementHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			(_, departureMovement) = NctsDepartureMovementHeaderTest.GetNewBusinessObject(Factory);
			lookups = departureMovement.Lookups;
		}
		NctsDepartureMovementHeader departureMovement;
		NctsDepartureMovementHeaderLookups lookups;

		public void TestAdditionalDeclarationTypeList()
		{
			AssertEquals("AdditionalDeclarationTypeList codes", "A, D", lookups.AdditionalDeclarationTypeList.CodesAsString);
			AssertSame("Should have cached.", lookups.AdditionalDeclarationTypeList, NctsDepartureMovementHeaderTest.GetNewBusinessObject(Factory).departureMovement.Lookups.AdditionalDeclarationTypeList);
		}
	}
}

using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class CreateDeclarationIPRLookupsTest : TestCaseWithFactory
	{
		public void TestDeclarationTypeList()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "EZA", "AZ", "VZA", "AVABR" }, lookups.DeclarationTypeList.GetAllCodes());
		}

		protected override void SetUp()
		{
			base.SetUp();

			var bizO = new CreateDeclarationIPR();
			lookups = (CreateDeclarationIPRLookups)bizO.Lookups;
		}

		CreateDeclarationIPRLookups lookups;
	}
}

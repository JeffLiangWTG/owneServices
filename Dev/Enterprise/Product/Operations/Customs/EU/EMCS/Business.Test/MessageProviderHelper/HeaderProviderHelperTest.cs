using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class HeaderProviderHelperTest : TestCaseWithFactory
	{
		public void TestAdministrativeReferenceCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty by Default", ZString.Empty, helper.AdministrativeReferenceCode);
				emcsDeclaration.EADNumber = "EADNUM1234";
				AssertEquals("Same as entered", "EADNUM1234", helper.AdministrativeReferenceCode);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			helper = new HeaderProviderHelper(emcsDeclaration);
		}
		EMCSJobDeclaration emcsDeclaration;
		HeaderProviderHelper helper;
	}
}

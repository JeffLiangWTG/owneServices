using Enterprise.Customs.CH.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

sealed class RestrictionsTabPageTest : TestCase
{
	public void TestProperties() => CombineAssertions(() =>
	{
		var restrictionsTabPage = new RestrictionsTabPage();
		AssertEquals("Caption", "Restrictions", restrictionsTabPage.Caption.Caption);
		AssertEquals("UserControlBindingMember", "Restrictions", restrictionsTabPage.UserControlBindingMember);
	});

	public void TestCreateUserControl() => CombineAssertions(() =>
	{
		var restrictionssTabPage = new RestrictionsTabPage();
		using (var userControl = restrictionssTabPage.CreateUserControl())
		{
			AssertType<RestrictionsUserControl>(userControl);
			AssertEquals("PermitOwnerDocAddressControl.BindToOrganisations", "Bills.GoodsItems.Restrictions.Lookups.PermitOwnerList", (userControl as RestrictionsUserControl)?.PermitOwnerDocAddressControl?.BindToOrganisations);
		}
	});
}

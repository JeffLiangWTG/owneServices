using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

sealed class AdditionalGoodsInformationTabPageTest : TestCase
{
	public void TestProperties() => CombineAssertions(() =>
	{
		var additionalGoodsInformationTabPage = new AdditionalGoodsInformationTabPage();
		AssertEquals("Caption", "Additional Goods Information", additionalGoodsInformationTabPage.Caption.Caption);
		AssertEquals("UserControlBindingMember", ".", additionalGoodsInformationTabPage.UserControlBindingMember);
	});

	public void TestCreateUserControl()
	{
		var additionalGoodsInformationTabPage = new AdditionalGoodsInformationTabPage();
		using (var userControl = additionalGoodsInformationTabPage.CreateUserControl())
		{
			AssertType<AdditionalGoodsInformationUserControl>(userControl);
		}
	}
}

using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

sealed class ExportDeclarationsTabPageTest : TestCase
{
	public void TestProperties() => CombineAssertions(() =>
	{
		var exportDeclarationsTabPage = new ExportDeclarationsTabPage();
		AssertEquals("Caption", "Export Declarations", exportDeclarationsTabPage.Caption.Caption);
		AssertEquals("UserControlBindingMember", ".", exportDeclarationsTabPage.UserControlBindingMember);
	});

	public void TestCreateUserControl()
	{
		var exportDeclarationsTabPage = new ExportDeclarationsTabPage();
		using (var userControl = exportDeclarationsTabPage.CreateUserControl())
		{
			AssertType<ExportDeclarationsUserControl>(userControl);
		}
	}
}

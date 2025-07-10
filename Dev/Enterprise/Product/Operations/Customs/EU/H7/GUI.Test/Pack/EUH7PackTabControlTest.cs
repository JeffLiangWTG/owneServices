using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7PackTabControl))]
	public class EUH7PackTabControlTest : TestCaseWithFactory
	{
		public void TestPackDetailsUserControl()
		{
			using (var control = new EUH7PackTabControl())
			{
				control.Show();

				var packDetailsUserControl = control.FindSingle<EUH7PackDetailsUserControl>("PackDetailsUserControl");

				AssertType<EUH7PackDetailsUserControl>(packDetailsUserControl);
			}
		}

		public void TestPackDetailsTabControl()
		{
			using (var control = new EUH7PackTabControl())
			{
				control.Show();

				var packDetailsTabControl = control.FindSingle<ZTemplateTabControl>("PackDetailsTabControl");

				AssertType<ZTemplateTabControl>(packDetailsTabControl);
			}
		}

		public void TestPackDetailsTabPage()
		{
			using (var control = new EUH7PackTabControl())
			{
				control.Show();

				var packDetailsTabPage = control.FindSingle<ZTabPage>("PackDetailsTabPage");
				var expectedCaption = new CargoWiseOne.ResourceStrings.ResourceStringData("374bb945-86a0-44a3-8aad-0084af0b16a6", "Pack Details");

				AssertType<ZTabPage>(packDetailsTabPage);
				AssertEquals(expectedCaption, packDetailsTabPage.CaptionResourceString);
			}
		}
	}
}

using NUnit.Framework;

namespace Enterprise.Customs.Common.GUI.Testing
{
	class WaitingForResponseFromBorderWiseMessageBoxTest : TestCase
	{
		public void TestCancelButtonText()
		{
			using (var mb = new WaitingForResponseFromBorderWiseMessageBox("&Cancel"))
			{
				AssertEquals("&Cancel", mb.MessageBoxButton1.Text);
			}
		}
	}
}

using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.GUI.Testing
{
	sealed class AUCusOutturnUserControlTest : TestCaseWithFactory
	{
		public void TestSetBindPrepend()
		{
			using (var control = new AUCusOutturnUserControl())
			{
				control.InitializeComponent();
				control.SetBindPrepend("XXX.");
				Assert("ResponsiblePartyIDTextBox.BindTo prepended", control.ResponsiblePartyIDTextBox.BindTo.StartsWith("XXX."));
			}
		}
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class AdditionalWagonNumbersUserControlTest : TestCaseWithFactory
	{
		public void TestInlandTransportEditButton()
		{
			using (var control = new AdditionalWagonNumbersUserControl())
			{
				AssertEquals("More..", control.AdditionalWagonNumbersButton.CaptionResourceString.Caption);
				AssertEquals("ToolTipCaption", "Additional Wagon Numbers", control.AdditionalWagonNumbersButton.ToolTipCaption.ToString());
			}
		}

		public void TestInlandTransportEditButton_Click()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			using (var form = new ZForm(declaration))
			using (var control = new AdditionalWagonNumbersUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.AdditionalWagonNumbersButton.PerformClick();
				AssertType<AdditionalWagonNumbersForm>(ZFormModaliser.LastFormShownForTest);
			}
		}
	}
}

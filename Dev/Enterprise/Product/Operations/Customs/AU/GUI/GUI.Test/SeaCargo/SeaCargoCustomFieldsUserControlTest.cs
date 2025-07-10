using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoCustomFieldsUserControlTest : TestCaseWithFactory
	{
		public void TestCustomFieldsDisplayControl()
		{
			using (var control = new SeaCargoCustomFieldsUserControl())
			{
				AssertEquals("To make use of this tab, please setup Sea Cargo custom fields in Workflow Manager", ((ProcessTemplateCustomFieldsControl)control.Controls.Find("CustomFieldsControl", true)[0]).NothingSetupMessageLabelText);
			}
		}
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class AsycudaBillCustomFieldsControlTest : TestCaseWithFactory
	{
		public void TestCustomFieldsDisplayControl()
		{
			using (var control = new AsycudaBillCustomFieldsControl())
			{
				AssertEquals("To make use of this tab, please setup Global Manifest Bill custom fields in Workflow Manager", ((ProcessTemplateCustomFieldsControl)control.Controls.Find("CustomFieldsControl", true)[0]).NothingSetupMessageLabelText);
			}
		}
	}
}

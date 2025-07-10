using System.Windows.Forms;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Client.EDI.FeatureControl.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.Module.Testing
{
	[TestedType(typeof(FeatureControlController))]
	public class FeatureControlControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.FeatureControl;
		}

		public override void TestNewForm()
		{
			var controller = new FeatureControlController();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			using (ZFormModaliser.SetTemporaryDelegateToCallBeforeShowingFormsOrDialogs(f =>
			{
				var newForm = f as NewFeatureControlForm;
				var newBizObj = newForm.BusinessEntity as NewFeatureControlBizObj;
				newBizObj.FeatureControlCodeList.AddPair("AAA", "BBB");
				newBizObj.FeatureControlCode = "AAA";
			}))
			{
				using (var featureControlForm = controller.ShowNewForm() as FeatureControlForm)
				{
					var bizObj = featureControlForm.BusinessEntity as FeatureControlHeader;
					AssertEquals(false, bizObj.IsInDatabase);
					AssertEquals(true, bizObj.HasChanges);
					AssertEquals("AAA", bizObj.FCM_FeatureControlCode);
					AssertEquals("BBB", bizObj.FCM_Description);
					featureControlForm.Close();
				}
			}
		}
	}
}

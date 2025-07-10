using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Client.EDI.FeatureControl.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.FeatureControl.Module
{
	public class FeatureControlController : ZController
	{
		public override ControllerID ID => Modules.ClientControllerRegistration.FeatureControl;

		public override ModuleIdentifier ModuleID => Modules.ClientModuleRegistration.FeatureControl;

		public override Type TypeOfTopLevelBusinessObject => typeof(FeatureControlHeader);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		#region Security checkpoints
		protected override SecurityCheckpoint CheckPointForView => EDISecurityCheckpoints.FeatureControlView;

		protected override SecurityCheckpoint CheckPointForNew => EDISecurityCheckpoints.FeatureControlNew;

		protected override SecurityCheckpoint CheckPointForEdit => EDISecurityCheckpoints.FeatureControlEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;
		#endregion

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new FeatureControlForm((FeatureControlHeader)businessEntity);
		}

		public override IZForm ShowNewForm()
		{
			if (CheckPointForNew.IsAllowed)
			{
				var newBizObj =  new NewFeatureControlBizObj();
				if (ZFormModaliser.ShowDialogAndDispose(new NewFeatureControlForm(newBizObj)) == DialogResult.OK)
				{
					var featureControl = Factory.New<FeatureControlHeader>();
					featureControl.FCM_FeatureControlCode = newBizObj.FeatureControlCode;
					featureControl.FCM_Description = newBizObj.FeatureControlCodeList.GetDescriptionFromCode(newBizObj.FeatureControlCode);

					return ShowEditForm(featureControl);
				}
			}
			else
			{
				Globals.Message.Show(CheckPointForNew.ErrorMessageForNotAllowed);
			}
			return null;
		}
	}
}

using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Client.EDI.FeatureControl.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.FeatureControl.Module
{
	public class FeatureSetController : ZController
	{
		public override ControllerID ID => Modules.ClientControllerRegistration.FeatureSet;

		public override ModuleIdentifier ModuleID => Modules.ClientModuleRegistration.FeatureSet;

		public override Type TypeOfTopLevelBusinessObject => typeof(FeatureControlSet);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		#region Security checkpoints

		protected override SecurityCheckpoint CheckPointForView => EDISecurityCheckpoints.FeatureSetView;

		protected override SecurityCheckpoint CheckPointForNew => EDISecurityCheckpoints.FeatureSetNew;

		protected override SecurityCheckpoint CheckPointForEdit => EDISecurityCheckpoints.FeatureSetEdit;

		protected override SecurityCheckpoint CheckPointForDelete => EDISecurityCheckpoints.FeatureSetEdit;

		#endregion

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new FeatureSetForm((FeatureControlSet)businessEntity);
		}

		public override IZForm ShowNewForm()
		{
			return base.ShowNewForm();
		}
	}
}

using System;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public class BMReleaseSequenceController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new NotImplementedException("BMReleaseSequenceController.GetForm is not implemented");
		}

		public override ControllerID ID => ControllerIDs.BMReleaseSequence;

		public override ModuleIdentifier ModuleID => ModuleIDs.BMReleaseSequence;

		public override Type TypeOfTopLevelBusinessObject => typeof(BMReleaseSequence);

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.BMReleaseSequenceNew;
		protected override SecurityCheckpoint CheckPointForView => Env.Security.BMReleaseSequenceView;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.BMReleaseSequenceEdit;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.BMReleaseSequenceDelete;

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			BMReleaseSequenceModule.OpenURL(new[] { sourceEntity });
			return null;
		}
	}
}

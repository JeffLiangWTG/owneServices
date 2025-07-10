using System;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public class BMBufferTimespanController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID => ModuleIDs.BMBufferTimespan;
		public override ControllerID ID => ControllerIDs.BMBufferTimespan;
		public override Type TypeOfTopLevelBusinessObject => typeof(BMBufferTimespan);

		protected override IZForm GetForm(IBusiness businessEntity) => new BMBufferTimespanForm((BMBufferTimespan)businessEntity);

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView => Env.Security.BMSystemsView;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.BMSystemsNew;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.BMSystemsEdit;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.BMSystemsView;

		#endregion
	}
}

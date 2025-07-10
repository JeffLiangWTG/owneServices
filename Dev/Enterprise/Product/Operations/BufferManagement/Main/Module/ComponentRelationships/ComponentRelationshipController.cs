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
	class ComponentRelationshipController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID => ModuleIDs.ComponentRelationship;

		public override ControllerID ID => ControllerIDs.ComponentRelationship;

		public override Type TypeOfTopLevelBusinessObject => typeof(ComponentRelationship);

		protected override IZForm GetForm(IBusiness businessEntity) => new ComponentRelationshipForm((ComponentRelationship)businessEntity);

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView => Env.Security.ComponentRelationshipView;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.ComponentRelationshipEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.ComponentRelationshipNew;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.ComponentRelationshipDelete;

		#endregion
	}
}

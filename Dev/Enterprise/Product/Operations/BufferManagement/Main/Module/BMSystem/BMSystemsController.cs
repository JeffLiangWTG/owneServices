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
	public class BMSystemsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.BMSystems; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.BMSystems; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BMSystem); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new BMSystemManagementForm((BMSystem)businessEntity);
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.BMSystemsView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.BMSystemsEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.BMSystemsNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.BMSystemsDelete; }
		}

		#endregion
	}
}

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
	public class BMControlCustomisationController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.BMControlCustomisation; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.BMControlCustomisation; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BMControlCustomisation); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new BMControlCustomisationForm((BMControlCustomisation)businessEntity);
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.BMControlCustomisationView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.BMControlCustomisationEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.BMControlCustomisationNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.BMControlCustomisationDelete; }
		}

		#endregion
	}
}

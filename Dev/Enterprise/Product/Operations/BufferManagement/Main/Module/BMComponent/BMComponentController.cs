using System;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public class BMComponentController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.BMComponent; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.BMComponent; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BMComponent); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new NotSupportedException("No form defined for a BMComponent");
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}

using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ServiceManager.Module
{
	public class ProcessControllerController : ZController
	{
		#region Standard Controller Overrides

		public override ControllerID ID
		{
			get { return ControllerIDs.ProcessController; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(StmServiceHost); }
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany { get; }

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null; // The forms clients will see all come from install/uninstall and start/stop actions
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ProcessController; }
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ProcessControllerView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ProcessControllerNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ProcessControllerEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ProcessControllerDelete; }
		}

		#endregion
	}
}

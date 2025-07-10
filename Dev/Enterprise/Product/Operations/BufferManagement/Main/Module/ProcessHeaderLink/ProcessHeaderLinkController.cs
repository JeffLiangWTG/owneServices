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
	public class ProcessHeaderLinkController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ProcessHeaderLink; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ProcessHeaderLink; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ProcessHeaderLink); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ProcessHeaderLinkForm((ProcessHeaderLink)businessEntity);
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.WorkflowDependenciesEdit; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.WorkflowDependenciesView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.WorkflowDependenciesNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.WorkflowDependenciesDelete; }
		}

		#endregion
	}
}

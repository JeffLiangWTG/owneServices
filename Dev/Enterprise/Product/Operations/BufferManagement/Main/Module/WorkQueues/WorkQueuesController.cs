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
	public class WorkQueuesController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WorkQueues; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.WorkQueues; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(WorkQueue); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new WorkQueueForm((WorkQueue)businessEntity);
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.WorkQueuesView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.WorkQueuesEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.WorkQueuesNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.WorkQueuesDelete; }
		}

		#endregion
	}
}

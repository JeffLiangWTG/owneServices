using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.GUI;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.Module
{
	/// <summary>
	/// Module Controller for PrintQueue.
	/// </summary>
	public class PrintQueueController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public PrintQueueController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get	{ return ModuleIDs.PrintQueue;	}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.PrintQueue; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(StmPrintQueue); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new PrintQueueForm((StmPrintQueue)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get	{ return Env.Security.PrintQueuesModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get	{ return Env.Security.PrintQueuesModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get	{ return Env.Security.PrintQueuesModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get	{ return Env.Security.PrintQueues; }
		}
	}
}

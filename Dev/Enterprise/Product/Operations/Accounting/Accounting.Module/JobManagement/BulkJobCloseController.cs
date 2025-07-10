using System;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobManagement;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class BulkJobCloseController : ZSingletonController
	{
		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CloseMultipleJobs; }
		}

		protected override ZArchitecture.GUI.IZForm GetForm(CargoWise.EntityFramework.IBusiness businessEntity)
		{
			var form = new BulkJobCloseForm(businessEntity as BulkJobCloseProcessor);
			return form;
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.BulkJobClose; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BulkJobCloseProcessor); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}

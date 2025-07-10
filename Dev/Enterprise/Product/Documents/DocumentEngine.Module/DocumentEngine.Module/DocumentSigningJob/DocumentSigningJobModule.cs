using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.Module
{
	internal class DocumentSigningJobModule : PrintJobModule
	{
		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.DocumentSigningJob; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.DocumentSigningJob);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new PrintJobFilterControl(GridCollection, (DocumentSigningJobFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new StmPrintJobCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new DocumentSigningJobFilterBusinessObject();
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.DocumentSigningJobs; }
		}

		#endregion
	}
}

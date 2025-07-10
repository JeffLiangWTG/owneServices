using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class CSARevenueSummaryFormModule : StatementModule
	{
		protected override IFilterControl GetNewFilterControl()
		{
			return new CSARevenueSummaryFormStatementFilterControl(GridCollection, FilterBusinessObject);
		}

		public override ModuleIdentifier ID => ModuleIDs.Customs.CA.CACSARevenueSummaryForm;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CACSARevenueSummaryForm;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.CA.CACSARevenueSummaryForm);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CSARevenueSummaryFormModuleCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CSARevenueSummaryFormFilterStripBusinessObject();
		}

		public override string WorkflowType => StatementProcessTask.StatementWorkflow.Code;

		public override bool AllowNew => true;
	}
}

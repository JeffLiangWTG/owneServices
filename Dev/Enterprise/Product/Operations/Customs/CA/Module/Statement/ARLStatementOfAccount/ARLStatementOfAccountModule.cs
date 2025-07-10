using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class ARLStatementOfAccountModule : StatementModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.CA.CAARLStatementOfAccount;
		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CAARLStatementOfAccount;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.CustomsStatement);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ARLStatementOfAccountModuleCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ARLStatementOfAccountFilterStripBusinessObject();
		}

		public override string WorkflowType => StatementProcessTask.StatementWorkflow.Code;
	}
}

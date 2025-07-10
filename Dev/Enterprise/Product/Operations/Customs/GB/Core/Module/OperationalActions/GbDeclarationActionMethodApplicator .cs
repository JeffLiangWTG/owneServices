using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.OperationalAction;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.GB.Module.OperationalActions
{
	public class GbDeclarationActionMethodApplicator : AutoGbDeclarationActionMethodApplicator
	{
		public GbDeclarationActionMethodApplicator()
			: base("GB MUCR management operational action")
		{ }

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			MucrFunctionUpdaterOperationalActionRunner runner = new MucrFunctionUpdaterOperationalActionRunner(log, targets);
			runner.PerformMucrFunctionOperationalAction(ActionAssociate, ActionDisassociate, ActionClose,
						this.MucrManual, this.Mawp, this.Mawn, this.Airport, this.Shed);
		}
	}
}

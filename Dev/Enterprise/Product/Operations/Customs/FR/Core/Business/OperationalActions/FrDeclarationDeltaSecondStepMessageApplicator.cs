using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class FrDeclarationDeltaSecondStepMessageApplicator : OperationalActionMethodApplicator
	{
		public FrDeclarationDeltaSecondStepMessageApplicator() : base((NoResString)"FR Delta G2 Second Step Message")
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var runner = new SecondStepMessageOperationalActionRunner(log, targets.OfType<JobDeclaration>());
			runner.SendSecondStepMessage();
		}
	}
}

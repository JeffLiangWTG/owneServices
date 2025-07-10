using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions
{
	public class GbDeclarationDropOffMessageActionMethodApplicator : AutoGbDeclarationDropOffMessageActionMethodApplicator
	{
		public GbDeclarationDropOffMessageActionMethodApplicator()
			: base("GB Drop Off Message")
		{ }

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var runner = new DropOffMessageOperationalActionRunner(log, targets);
			runner.PerformMakeDropOffMessages(Vehicle, ETD, ETA);
		}
	}
}

using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions
{
	public class DetachFromForwardingApplicator : OperationalActionMethodApplicator, IObsoleteValidation
	{
		public DetachFromForwardingApplicator()
			: base("GB CCSUK function: Detach AWB From Forwarding")
		{ }

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var methodInvoker = new CcsukOperationalActionApplicatorRunner(log, targets);
			methodInvoker.DetachFromForwarding();
		}
	}
}

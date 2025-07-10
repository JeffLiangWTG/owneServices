using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.CN.Business
{
	public class JobDeclarationTransportSupporter : BaseJobDeclarationTransportSupporter<JobDeclaration>
	{
		public JobDeclarationTransportSupporter(JobDeclaration parent)
			: base(parent) { }

		protected override void NotifyLoadChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyLoadChangedCore(transport, previousValue);
			Parent.DefaultLastPortBeforeEntryIfNeeded();
		}

		protected override void NotifyDischargeChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyDischargeChangedCore(transport, previousValue);
			Parent.DefaultLastPortBeforeEntryIfNeeded();
		}

		protected override void NotifyLegOrderChangedCore(Transport transport, ZByte previousValue)
		{
			base.NotifyLegOrderChangedCore(transport, previousValue);
			Parent.DefaultLastPortBeforeEntryIfNeeded();
		}
	}
}

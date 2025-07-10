using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public abstract class SystemRawUsage : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected SystemRawUsage(BillingLoadRawUsageContext context)
			: base(context.Factory)
		{
			this.context = context;
		}
		protected readonly BillingLoadRawUsageContext context;

		public ZDateTime PeriodStart
		{
			get { return context.Period; }
		}

		public ZString OrgCode
		{
			get { return context.OrgCode; }
		}

		public ZString OrgName
		{
			get { return context.OrgName; }
		}

		public ZString CompanyCode
		{
			get { return context.CompanyCode; }
		}

		public ZString ServerCode
		{
			get { return context.ServerCode; }
		}

		public abstract ZString SystemCode { get; }

		public abstract SummarySection[] GetRawUsageSummarySections();
	}
}


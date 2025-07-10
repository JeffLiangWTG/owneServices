using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class SystemCodeRawUsage : SystemRawUsage
	{
		public SystemCodeRawUsage(BillingLoadRawUsageContext context, ZString systemCode)
			: base(context)
		{
			this.systemCode = systemCode;
		}

		#region System Code

		public override ZString SystemCode
		{
			get { return systemCode; }
		}

		readonly ZString systemCode;

		#endregion

		#region Summary Section

		public SummarySection Summary
		{
			get { return summary ?? (summary = new SummarySection(Factory)); }
		}
		SummarySection summary;

		#endregion

		#region Raw Usage Summary Sections

		public override SummarySection[] GetRawUsageSummarySections()
		{
			Summary.Header.TopLevelDescription = !SummaryHeaderDescription.IsEmpty ? SummaryHeaderDescription : DefaultSummaryHeaderDescription;
			return new SummarySection[] { Summary };
		}

		public ZString SummaryHeaderDescription { get; set; }

		protected ZString DefaultSummaryHeaderDescription
		{
			get { return SystemDescription + " Usage Summary"; }
		}

		protected ZString SystemDescription
		{
			get { return BillingConstants.BillingSystemList.GetDescriptionFromCode(SystemCode); }
		}

		#endregion
	}
}


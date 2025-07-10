using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class JapanAFRUsage : EServicesSystemUsage
	{
		public JapanAFRUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: base("AFR", factory, user, periodStart)
		{
			PriceHeaderCode = BillingConstants.BillingSystem.ODM;
		}

		public override ZString SystemCode
		{
			get { return BillingConstants.BillingSystem.JapanAFR; }
		}

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections()
		{
			var result = base.GetGeneralSummarySections()[0];
			result.Header.MainDescription = "Pre Departure Sea Manifest Filing (AFR)";
			result.Lines[0].MainDescription = "Japan AFR Transaction";
			return new SummarySection[] { result };
		}

		#endregion
	}
}


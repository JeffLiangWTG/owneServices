using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class ZACustomsUsage : EServicesSystemUsage
	{
		public ZACustomsUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: this("", factory, user, periodStart)
		{
		}

		public ZACustomsUsage(ZString subCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: base(subCode, factory, user, periodStart)
		{
			PriceHeaderCode = BillingConstants.BillingSystem.ODM;
		}

		public override ZString SystemCode
		{
			get { return BillingConstants.BillingSystem.ZACustoms; }
		}

		public override ZString PriceItemCode
		{
			get { return SubCode; }
		}

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections()
		{
			var result = base.GetGeneralSummarySections()[0];
			if (SubCode == "ZX1")
			{
				result.Lines[0].MainDescription = "Outbound CUSCAR Messages [COH and HAB]";
			}
			else if (SubCode == "ZX2")
			{
				result.Lines[0].MainDescription = "Outbound CUSCAR Messages [FFM, ECL, RFM, BBB and RMA]";
			}
			else if (SubCode == "ZX3")
			{
				result.Lines[0].MainDescription = "Outbound CUSCAR Messages [COM and FWB]";
			}

			return new SummarySection[] { result };
		}

		#endregion
	}
}


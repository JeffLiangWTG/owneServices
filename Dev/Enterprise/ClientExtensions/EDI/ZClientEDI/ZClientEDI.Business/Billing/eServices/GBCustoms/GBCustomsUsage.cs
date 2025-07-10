using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class GBCustomsUsage : EServicesSystemUsage
	{
		public GBCustomsUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: this("", factory, user, periodStart)
		{
		}

		public GBCustomsUsage(ZString subCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: base(subCode, factory, user, periodStart)
		{
			PriceHeaderCode = BillingConstants.BillingSystem.ODM;
		}

		public override ZString SystemCode
		{
			get { return BillingConstants.BillingSystem.GBCustoms; }
		}

		public override ZString PriceItemCode
		{
			get { return SystemCode; }
		}

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections()
		{
			var result = base.GetGeneralSummarySections()[0];
			if (SubCode == "AWB")
			{
				result.Lines[0].MainDescription = "Air Waybills";
			}
			else if (SubCode == "GTM")
			{
				result.Lines[0].MainDescription = "General Text Messages";
			}
			else if (SubCode == "CUE")
			{
				result.Lines[0].MainDescription = "Customs Entries";
			}

			return new SummarySection[] { result };
		}

		#endregion
	}
}


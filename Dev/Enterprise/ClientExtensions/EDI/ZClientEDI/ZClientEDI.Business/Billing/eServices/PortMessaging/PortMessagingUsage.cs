using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class PortMessagingUsage : EServicesSystemUsage
	{
		public PortMessagingUsage(ZString subCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: base(subCode, factory, user, periodStart)
		{
			PriceHeaderCode = BillingConstants.BillingSystem.ODM;
		}

		public override ZString SystemCode
		{
			get { return BillingConstants.BillingSystem.PortMessaging; }
		}

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections()
		{
			var result = base.GetGeneralSummarySections()[0];
			var caption = PortMessagingBillingSystem.GetUsageCaptions().FirstOrDefault(x => string.Equals(x.UsageCode, SubCode, StringComparison.OrdinalIgnoreCase));
			var desc = caption?.UsageDescription ?? SubCode;
			result.Lines[0].MainDescription = desc;
			return new SummarySection[] { result };
		}

		#endregion
	}
}


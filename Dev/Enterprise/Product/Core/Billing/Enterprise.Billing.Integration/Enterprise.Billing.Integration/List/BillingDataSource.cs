using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Billing.Integration
{
	[Immutable]
	public class BillingDataSource
	{
		BillingDataSource(string databaseValue)
		{
			this.databaseValue = databaseValue;
		}

		readonly ZString databaseValue;

		public override string ToString()
		{
			return this.databaseValue;
		}

		public static readonly BillingDataSource DataWizard = new BillingDataSource("DWZ");
		public static readonly BillingDataSource eAdaptorInbound = new BillingDataSource("EAD");
		public static readonly BillingDataSource eAdaptorOutbound = new BillingDataSource("EAO");
		public static readonly BillingDataSource eHubInbound = new BillingDataSource("HUB");
		public static readonly BillingDataSource eHubOutbound = new BillingDataSource("HUO");
		public static readonly BillingDataSource InterfaceConnector = new BillingDataSource("ICN");
		public static readonly BillingDataSource None = new BillingDataSource("NON");
	}
}

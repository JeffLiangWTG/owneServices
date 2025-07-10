using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BillingSystemList : List<BillingSystem>
	{
		public BillingSystemList()
			: base()
		{
			Add(new Fee.FeeBillingSystem());
			Add(new ODPL.OdplBillingSystem());
			Add(new Ebacca.EbaccaBillingSystem());
			Add(new Fax.FaxBillingSystem());
			Add(new DeniedPartyScreening.DpsBillingSystem());
			Add(new IsfBillingSystem());
			Add(new ExDocs.ExDocsBillingSystem());
			Add(new DistanceCalculator.DistanceCalculatorGenericBillingSystem());
			Add(new DistanceCalculator.DistanceCalculatorPcMilerBillingSystem());
			Add(new S8CargoBillingSystem());
			Add(new Hosting.HostingStorageBillingSystem());
			Add(new Hosting.HostingRemoteDevicesBillingSystem());
			Add(new Hosting.HostingDataAccessBillingSystem());
			Add(new AirlineMessagingBillingSystem());
			Add(new NZCustomsBillingSystem());
			Add(new ABMCustomsBillingSystem());
			Add(new ClientMappingBillingSystem());
			Add(new eAdaptor.EAdaptorBillingSystem());
			Add(new E2EBillingSystem());
			Add(new JapanAFRBillingSystem());
			Add(new USCustomsBillingSystem());
			Add(new RailincByMessageBillingSystem());
			Add(new PortMessagingBillingSystem());
			Add(new GlobalContainerTrackingBillingSystem());
			Add(new OceanTracingBillingSystem());
			Add(new OceanTracingLegacyBillingSystem());
			Add(new ForwardAirBillingSystem());
			Add(new ShippingPortMessagingBillingSystem());
			Add(new GBCustomsBillingSystem());
			Add(new OceanCarrierMessagingBillingSystem());
			Add(new AsycudaBillingSystem());
			Add(new PremiumServiceBillingSystem());
			Add(new ZACustomsBillingSystem());
			Add(new BorderWise.BorderWiseBillingSystem());
			Add(new FlightStatsBillingSystem());
			Add(new Hosting.WiseCloudUserBillingSystem());

			var cptUsage = EDIDataRegistry.Instance.LicenceUsageBilledPerTransaction.Value;
			for (int i = 0; i < cptUsage.Count; ++i)
			{
				Add(new LicenceUsageTransactionBillingSystem(cptUsage[i].Code, cptUsage[i].Description));
			}

			var sorted = this.OrderBy(s => s.SystemDescription).ToArray();
			this.Clear();
			this.AddRange(sorted);
		}
	}
}


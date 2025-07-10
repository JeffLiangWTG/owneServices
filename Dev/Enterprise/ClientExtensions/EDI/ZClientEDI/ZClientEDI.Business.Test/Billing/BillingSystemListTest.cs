using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class BillingSystemListTest : TestCaseWithFactory
	{
		public void TestBillingSystemList()
		{
			CodeDescriptionPairList cptUsage = new CodeDescriptionPairList();
			cptUsage.AddPair("ACP", "ediACIReporting"); // Env.Licence.ACIReportingPerTransaction
			cptUsage.AddPair("AMS", "ediAMSReporting"); // Env.Licence.AMSReporting
			EDIDataRegistry.Instance.LicenceUsageBilledPerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cptUsage);

			BillingSystemList list = new BillingSystemList();
			AssertEquals("billing systems", 36 + cptUsage.Count, list.Count);

			var billingSystemMap = list.ToDictionary(x => x.SystemCode);

			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.Fee));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.ODM));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.eBACCA));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.Fax));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.DeniedPartyScreening));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.ImporterSecurityFiling));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.ExDocs));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.DistanceCalculatorGeneric));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.DistanceCalculatorPcMiler));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.S8Cargo));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.HostingStorage));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.HostingRemoteDevices));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.HostingDataAccess));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.AirlineMessaging));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.NZCustoms));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.ABMCustoms));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.ClientMapping));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.eAdaptor));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.E2E));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.JapanAFR));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.USCustoms));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.RailincByMessage));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.PortMessaging));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.GlobalContainerTracking));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.OceanTracing));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.OceanTracingLegacy));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.ForwardAir));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.ShippingPortMessaging));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.GBCustoms));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.OceanCarrierMessaging));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.ASYCUDA));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.Service));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.ZACustoms));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.BorderWise));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.FlightStats));
			AssertEquals(true, billingSystemMap.ContainsKey(BillingConstants.BillingSystem.WiseCloudUser));

			AssertEquals(true, billingSystemMap.ContainsKey("ACP"));
			AssertEquals(true, billingSystemMap.ContainsKey("AMS"));
		}
	}
}
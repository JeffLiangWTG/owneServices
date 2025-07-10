using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Testing
{
	public class ShippingPortsMessagingEHubIDLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				var shippingPortsMessagingEHubID = new ShippingPortsMessagingEHubID();
				AssertExceptionThrown<ArgumentNullException>("factory can not be null", () => new ShippingPortsMessagingEHubIDLookups(shippingPortsMessagingEHubID, null));
				AssertNoExceptionThrown("No error", () => new ShippingPortsMessagingEHubIDLookups(shippingPortsMessagingEHubID, Factory));
			});
		}

		public void TestPortList()
		{
			var expectedPortList = new string[] { "AUBNE", "AUMEL", "AUSYD", "ESBCN", "ESGAN", "ESPDS", "ESVLC", "NZAKL", "NZLYT", "NZNPE", "NZORR", "NZPOE", "NZTRG", "NZWLG" };
			var shippingPortsMessagingEHubID = new ShippingPortsMessagingEHubID();
			var list = shippingPortsMessagingEHubID.Lookups.PortList;

			Assert(list is IRefUNLOCOCollection);
			AssertContainsExactElementsInExactOrder(expectedPortList, list.ToArray().Cast<IRefUNLOCO>().Select(x => x.RL_Code));
		}

		public void TestModuleTypeList()
		{
			var shippingPortsMessagingEHubID = new ShippingPortsMessagingEHubID();
			var list = shippingPortsMessagingEHubID.Lookups.ModuleTypeList;

			Assert(list is CodeDescriptionPairList);
			AssertContainsExactElementsInAnyOrder(new string[] { ModuleTypes.Codes.eHub, ModuleTypes.Codes.xHub }, list.GetAllCodes());
		}
	}
}

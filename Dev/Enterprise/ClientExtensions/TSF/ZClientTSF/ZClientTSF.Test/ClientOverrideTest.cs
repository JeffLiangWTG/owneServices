using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Client.TSF.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Client.TSF.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestOverrides()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ClientOverride @override = ClientOverride.Instance;
			AssertEquals("Client", Clients.TSF, @override.Client);
			AssertEquals("Client Display Name should be the name of the organisation", "Transtar International Freight (VIC) Pty Ltd", @override.ClientDisplayName);
			ForwardingShipment shipment = factory.New<ForwardingShipment>();
			DocForwardingShipment shipmentWrapper = DocForwardingShipment.New(shipment, factory);
			AssertEquals("DocForwardingShipment override", typeof(DocTSFForwardingShipment), shipmentWrapper.GetType());
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}
	}
}

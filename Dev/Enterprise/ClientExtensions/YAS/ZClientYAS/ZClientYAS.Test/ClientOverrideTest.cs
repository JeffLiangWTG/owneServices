using System;
using System.Collections.Generic;

using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.YAS.Business;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.YAS.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class YASClientOverrideTest : ClientOverrideTest
	{
		protected override Type ClientOverrideType
		{
			get { return typeof(ClientOverride); }
		}

		public void TestClientTypeDecider()
		{
			ITypeDeciderDictionary clientTypeDeciders = ClientOverride.Instance.ClientTypeDeciders;
			AssertNotNull("ClientTypeDeciders", clientTypeDeciders);
			AssertEquals("ClientTypeDeciders.Count", 1, new List<KeyValuePair<Type, ITypeDecider>>(clientTypeDeciders).Count);
			AssertEquals("ForwardingShipment should map to YASForwardingShipment", typeof(YASForwardingShipment), ((TypeDeciderImpl)clientTypeDeciders[typeof(ForwardingShipment)]).ClientType);
		}

		public void TestClientOverrideInstance()
		{
			ClientOverride lazyLoadedClientOverride = ClientOverride.Instance;
			AssertSame("The Same ClientOverride Instance should be returned, because it should be lazy loaded", lazyLoadedClientOverride, ClientOverride.Instance);
		}

		public void TestModuleOverrides()
		{
			ClientOverride clientOverride = ClientOverride.Instance;
			AssertNotNull(clientOverride.ModuleOverrides);
		}

		public void TestInitialsedAndUnitialised()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ClientOverride clientOverride = new ClientOverrideTestClass();
			ForwardingShipment shipment = factory.New<ForwardingShipment>();
			DocForwardingShipment shipmentWrapper = DocForwardingShipment.New(shipment, factory);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var docAWB = DocAWB.New(shipment.AWBHeader, factory);

			clientOverride.Initialise();
			AssertEquals("DocForwardingShipment instance type should be DocYASForwardingShipment", typeof(DocYASForwardingShipment), shipmentWrapper.GetType());
			AssertEquals("DocYASAWB instance type should be DocYASAWB", typeof(DocYASAWB), docAWB.GetType());

			ClientOverride.Instance.Uninitialise();

			shipmentWrapper = DocForwardingShipment.New(shipment, factory);
			docAWB = DocAWB.New(shipment.AWBHeader, factory);
			AssertEquals("DocForwardingShipment instance type should be DocForwardingShipment", typeof(DocForwardingShipment), shipmentWrapper.GetType());
			AssertEquals("CargoIMPServiceProviderList instance type should be CargoIMPServiceProviderList", typeof(CargoIMPServiceProviderList), CargoIMPServiceProviderList.New().GetType());
			AssertEquals("DocYASAWB instance type should be DocAWB", typeof(DocAWB), docAWB.GetType());
		}

		public class ClientOverrideTestClass : ClientOverride
		{
		}
	}
}

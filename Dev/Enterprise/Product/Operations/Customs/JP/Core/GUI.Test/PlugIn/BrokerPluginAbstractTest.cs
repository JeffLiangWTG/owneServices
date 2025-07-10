using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.JP.GUI.Testing
{
	abstract class BrokerPluginAbstractTest<T> : TestCaseWithFactory
		where T : CustomsBrokerageUserControl
	{
		protected abstract Type CreateBrokerageUserControl { get; }

		protected override void SetUp()
		{
			base.SetUp();
			forwardingShipment = Factory.New<ForwardingShipment>();
			brokeragePlugIn = new BrokeragePlugIn(forwardingShipment);
			control = Activator.CreateInstance<T>();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
			brokeragePlugIn.Dispose();
		}

		protected BrokeragePlugIn brokeragePlugIn;
		protected ForwardingShipment forwardingShipment;
		protected CustomsBrokerageUserControl control;
	}
}

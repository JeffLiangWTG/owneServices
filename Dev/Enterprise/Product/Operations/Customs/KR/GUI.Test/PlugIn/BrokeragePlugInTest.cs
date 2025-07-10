using System;
using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class BrokeragePlugInTest : Customs.GUI.PlugIn.Testing.BaseBrokeragePlugInAbstractTest
	{
		public void TestBrokerageControlIsCorrectType()
		{
			using (var plugin = new BrokeragePlugInForTest(Factory.New<ForwardingShipment>()))
			{
				using (var control = plugin.CustomsBrokerageUserControl)
				{
					AssertEquals(typeof(CustomsBrokerageUserControl), control.GetType());
				}
			}
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest()
		{
			return new BrokeragePlugIn(Shipment);
		}

		public void TestMenuIsCorrectType()
		{
			using (var plugin = new BrokeragePlugIn(Factory.New<ForwardingShipment>()))
			{
				AssertEquals(typeof(EDIMenu), plugin.TopLevelMenu.GetType());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			setUNIPASSDeclarantID = KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");
		}

		protected override void TearDown()
		{
			base.TearDown();
			setUNIPASSDeclarantID.Dispose();
		}

		IDisposable setUNIPASSDeclarantID;
	}

	sealed class BrokeragePlugInForTest : BrokeragePlugIn
	{
		public BrokeragePlugInForTest(ForwardingShipment shipment) : base(shipment)
		{
		}

		public BaseCustomsBrokerageUserControl CustomsBrokerageUserControl => base.CreateBrokerageUserControl();
	}
}

using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Testing
{
	class GbBrokeragePlugInTest : Customs.GUI.PlugIn.Testing.BaseBrokeragePlugInAbstractTest
	{
		public void TestMenuIsCorrectType()
		{
			using (var plugin = new GbBrokeragePlugIn(Factory.New<ForwardingShipment>()))
			{
				AssertEquals(typeof(EDIMenu), plugin.TopLevelMenu.GetType());
			}
		}

		public void TestCreateCustomsBrokerageUserControl()
		{
			using (var plugin = new GbBrokeragePlugInForTest(Factory.New<ForwardingShipment>()))
			{
				using (var control = plugin.CreateBrokerageUserControlExposed())
				{
					AssertType(typeof(CustomsBrokerageUserControl), control);
				}
			}
		}

		[TestDate(1986, 3, 12, 1, 2, 3)]
		public void TestAttachConsolToShipment()
		{
			ChiefExportConsolIntegrationPlugInTests.RunShipmentAndConsolAssociateTest(Factory,
																						delegate(ForwardingConsol c, ForwardingShipment s)
																						{ return new GbBrokeragePlugIn(s); },
																						delegate(ForwardingConsol c, ForwardingShipment s)
																						{ s.Consols.Add(c); },
																						delegate(ForwardingConsol c, ForwardingShipment s)
																						{ s.Consols.Remove(c); }
																						);
		}

		public void TestPopupWarningsForCreatingDeclarationWithCcsukAwb()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = "00011111111";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "GBLHR";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "DANIEL01";
			shipment.JS_RL_NKDestination = "GBLHR";
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = consol.JK_MasterBillNum;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "Irrelevant";
			using (var plugin = new GbBrokeragePlugInForTest(shipment))
			{
				AssertNotContains("Does not complain about CCSUK, the hawb is not relevant", "CCS", plugin.CreateDeclarationQueryCoreExposed);
			}
			hawb.CS_HAWB = shipment.JS_HouseBill;
			using (var plugin = new GbBrokeragePlugInForTest(shipment))
			{
				AssertContains("Complains about no FK link to CCSUK hawb, the hawb is relevant", "CCS", plugin.CreateDeclarationQueryCoreExposed);
			}
			hawb.CS_JS = shipment.PK;
			using (var plugin = new GbBrokeragePlugInForTest(shipment))
			{
				AssertNotContains("No complaint now that the hawb is linked", "CCS", plugin.CreateDeclarationQueryCoreExposed);
			}
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new GbBrokeragePlugIn(Shipment);

		class GbBrokeragePlugInForTest : GbBrokeragePlugIn
		{
			public GbBrokeragePlugInForTest(ForwardingShipment shipment)
				: base(shipment)
			{ }

			public BaseCustomsBrokerageUserControl CreateBrokerageUserControlExposed()
			{
				return base.CreateBrokerageUserControl();
			}

			public string CreateDeclarationQueryCoreExposed
			{
				get { return GetQueryText(System.Array.Empty<string>(), Customs.Business.CreateDeclarationHelper.QuestionType.CreateDeclarationQuery); }
			}
		}
	}
}

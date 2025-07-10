using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.GUI.Testing
{
	public class BrokeragePlugInTest : Customs.GUI.PlugIn.Testing.BaseBrokeragePlugInAbstractTest
	{
		public void TestMenuIsCorrectType()
		{
			using (var brokeragePlugIn = (BrokeragePlugIn)GetPlugInToTest())
			{
				var topLevelMenu = brokeragePlugIn.TopLevelMenu;
				AssertEquals("TopLevelMenu type", ExpectedTopLevelMenuType, topLevelMenu.GetType());
				AssertSame("TopLevelMenu is cached", topLevelMenu, brokeragePlugIn.TopLevelMenu);
			}
		}

		public void TestCStatus()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var plugin = new BrokeragePlugInForTest(shipment))
			{
				plugin.OnGUIShown();
				AssertContains("Your company is not the nominated customs broker", plugin.CreateDeclarationWarningQueryExposed);
			}
			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_CommunityTransitStatus = "X";
			using (var plugin = new BrokeragePlugInForTest(shipment))
			{
				plugin.OnGUIShown();
				AssertContains("Your company is not the nominated customs broker", plugin.CreateDeclarationWarningQueryExposed);
			}
			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_CommunityTransitStatus = "C";
			using (var plugin = new BrokeragePlugInForTest(shipment))
			{
				plugin.OnGUIShown();
				AssertContains("C-Status", plugin.CreateDeclarationWarningQueryExposed);
			}
		}

		protected virtual Type ExpectedTopLevelMenuType => typeof(EDIMenu);

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new BrokeragePlugIn(Shipment);

		class BrokeragePlugInForTest : BrokeragePlugIn
		{
			public string CreateDeclarationWarningQueryExposed
			{
				get { return fCreateDeclarationWarningQueryExposed; }
			}
			string fCreateDeclarationWarningQueryExposed;

			protected override string GetQueryText(string[] messages, Customs.Business.CreateDeclarationHelper.QuestionType type)
			{
				var queryText = base.GetQueryText(messages, type);
				if (type == Customs.Business.CreateDeclarationHelper.QuestionType.CreateDeclarationWarning)
				{
					fCreateDeclarationWarningQueryExposed = queryText;
				}
				return queryText;
			}

			public BrokeragePlugInForTest(ForwardingShipment shipment)
				: base(shipment)
			{
			}
		}
	}

	class BrokerageInnerPlugInTest : TestCaseWithFactory
	{
		public void TestBrokerageControlIsCorrectType()
		{
			using (var plugin = new BrokeragePlugInForTesting(Factory.New<ForwardingShipment>()))
			{
				using (var control = plugin.CreateBrokerageUserControl_Exposed())
				{
					AssertEquals(typeof(CustomsBrokerageUserControl), control.GetType());
				}
			}
		}

		public class BrokeragePlugInForTesting : BrokeragePlugIn
		{
			public BrokeragePlugInForTesting(ForwardingShipment shipment) : base(shipment) { }
			public BaseCustomsBrokerageUserControl CreateBrokerageUserControl_Exposed() => CreateBrokerageUserControl();
		}
	}
}

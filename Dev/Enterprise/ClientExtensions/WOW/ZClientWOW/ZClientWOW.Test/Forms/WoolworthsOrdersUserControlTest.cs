using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	public class WoolworthsOrdersUserControlTest : TransactionedTestCase
	{
		[ExpectNoExceptions]
		public void TestReplaceServiceLevelWithCustomAttrib()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			WoolworthsOrder bO = (WoolworthsOrder)factory.New(typeof(Order));
			WoolworthsOrder bO2 = (WoolworthsOrder)factory.New(typeof(Order));
			using (TestOrdersForm coreForm = new TestOrdersForm(bO))
			using (TestWoolworthsOrdersForm clientForm = new TestWoolworthsOrdersForm(bO2))
			{
				coreForm.Show();
				Application.DoEvents();
				clientForm.Show();
				Application.DoEvents();
				AssertEquals("Old control hidden", false, clientForm.UserControl.JD_RSBoundFindBox.Visible);
				AssertEquals("New control is correct place", coreForm.UserControl.JD_RSBoundFindBox.Top, clientForm.UserControl.JD_PaymentTypeBoundDropEdit.Top);
				AssertEquals("New control is correct place", true, clientForm.UserControl.JD_PaymentTypeBoundDropEdit.Left >= coreForm.UserControl.JD_RXBoundCurrency.Left - 1 && clientForm.UserControl.JD_PaymentTypeBoundDropEdit.Left <= coreForm.UserControl.JD_RXBoundCurrency.Left + 1);
			}
		}

		public class TestOrdersForm : OrdersForm
		{
			public TestOrdersForm(Order bO) : base(bO)
			{
			}

			protected override OrdersUserControl NewOrdersUserControl()
			{
				return new TestOrdersUserControl();
			}

			public TestOrdersUserControl UserControl
			{
				get
				{
					return (TestOrdersUserControl)base.OrdersUserControl.Inner;
				}
			}
		}

		public class TestOrdersUserControl : OrdersUserControl
		{
			public new ZCodeFindBox JD_RSBoundFindBox
			{
				get
				{
					return base.JD_RSBoundFindBox;
				}
			}

			public new ZExchangeRateControl JD_RXBoundCurrency
			{
				get
				{
					return base.JD_RXBoundCurrency;
				}
			}
		}

		public class TestWoolworthsOrdersForm : WoolworthsOrdersForm
		{
			public TestWoolworthsOrdersForm(WoolworthsOrder bO) : base(bO)
			{
			}

			protected override OrdersUserControl NewOrdersUserControl()
			{
				return new TestWoolworthsOrdersUserControl();
			}

			public TestWoolworthsOrdersUserControl UserControl
			{
				get
				{
					return (TestWoolworthsOrdersUserControl)base.OrdersUserControl.Inner;
				}
			}
		}

		public class TestWoolworthsOrdersUserControl : WoolworthsOrdersUserControl
		{
			public new ZLabel JD_RSLabel
			{
				get
				{
					return base.JD_RSLabel;
				}
			}

			public new ZCodeFindBox JD_RSBoundFindBox
			{
				get
				{
					return base.JD_RSBoundFindBox;
				}
			}

			public new ZDropEdit JD_PaymentTypeBoundDropEdit
			{
				get
				{
					return base.JD_PaymentTypeBoundDropEdit;
				}
			}

			public new ZExchangeRateControl JD_RXBoundCurrency
			{
				get
				{
					return base.JD_RXBoundCurrency;
				}
			}
		}
	}
}

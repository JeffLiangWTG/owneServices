using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.Wow
{
	[TestedType(typeof(WoolworthsOrdersForm))]
	public class WoolworthsOrdersFormTest : ZFormBasherTest
	{
		[ExpectNoExceptions]
		public void TestLoadForm()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			WoolworthsOrder bO = (WoolworthsOrder)factory.New(typeof(Order));
			using (WoolworthsOrdersForm form = new WoolworthsOrdersForm(bO))
			{
				form.Show();
				Application.DoEvents();
			}
		}

		[ExpectNoExceptions]
		public void TestCorrectOrdersUserControl()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			WoolworthsOrder bO = (WoolworthsOrder)factory.New(typeof(Order));
			using (TestWoolworthsOrdersForm form = new TestWoolworthsOrdersForm(bO))
			{
				form.Show();
				Application.DoEvents();
				Assert("Correctly overridden user control", form.GetOrdersUserControl() is WoolworthsOrdersUserControl);
			}
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			WoolworthsOrder order = Factory.New<WoolworthsOrder>();
			WoolworthsOrdersForm result = new WoolworthsOrdersForm(order);
			result.ControllerID = ControllerIDs.Orders;
			return result;
		}
		#endregion
	}
}

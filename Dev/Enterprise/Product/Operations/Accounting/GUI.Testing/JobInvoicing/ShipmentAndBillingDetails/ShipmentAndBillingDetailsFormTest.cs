using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(ShipmentAndBillingDetailsForm))]
	internal class ShipmentAndBillingDetailsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ShipmentAndBillingDetailsForm(new ShipmentAndBillingDetails(Factory.New<ForwardingConsol>()));
		}

		public void TestNoFormVerb()
		{
			using (var form = GetFormToBashCore() as ShipmentAndBillingDetailsForm)
			{
				AssertEquals(string.Empty, form.FormVerb);
			}
		}
	}
}

using System.Windows.Forms;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.GUI.Testing
{
	[TestedType(typeof(BillingSystemChooserForm))]
	public class BillingSystemChooserFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new BillingSystemChooserForm(new BillingSystemWrapperCollection(new BillingSystemList()));
		}
	}
}

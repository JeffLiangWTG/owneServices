using System.Windows.Forms;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.GUI.Testing
{
	[TestedType(typeof(StlBillingMilestoneSetterForm))]
	public class StlBillingMilestoneSetterFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new StlBillingMilestoneSetterForm(new StlBillingMilestoneSetter());
		}
	}
}

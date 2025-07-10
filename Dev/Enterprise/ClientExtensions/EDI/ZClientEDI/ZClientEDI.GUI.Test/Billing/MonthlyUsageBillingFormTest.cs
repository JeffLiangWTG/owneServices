using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.GUI.Testing
{
	[TestedType(typeof(MonthlyUsageBillingForm))]
	public class MonthlyUsageBillingFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MonthlyUsageBillingForm(new MonthlyUsageBilling(new BusinessObjectFactory()));
		}
	}
}

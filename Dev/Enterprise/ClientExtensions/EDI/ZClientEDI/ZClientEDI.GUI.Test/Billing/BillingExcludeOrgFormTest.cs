using System.Windows.Forms;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.GUI.Testing
{
	[TestedType(typeof(BillingExcludeOrgForm))]
	internal sealed class BillingExcludeOrgFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new BillingExcludeOrgForm(new ClientLicenceBillingExcludeOrgUpdater(Factory));
		}

		#endregion
	}
}

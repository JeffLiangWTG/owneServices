using System.Windows.Forms;
using Enterprise.Client.EDI.Billing.Business.Maintenance;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.GUI.Testing
{
	[TestedType(typeof(MaintenanceBillingForm))]
	internal sealed class MaintenanceBillingFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MaintenanceBillingForm(new MaintenanceBilling(Factory));
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			if (control.Name == "perModuleCheckBox")
			{
				return true;
			}
			return base.ShouldIgnoreMissingBindingMember(control);
		}
	}
}

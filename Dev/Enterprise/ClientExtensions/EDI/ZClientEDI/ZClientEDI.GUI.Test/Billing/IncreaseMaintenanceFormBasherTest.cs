using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.GUI.Testing
{
	[TestedType(typeof(IncreaseMaintenanceForm))]
	internal sealed class IncreaseMaintenanceFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new IncreaseMaintenanceForm();
		}

		#endregion
	}
}

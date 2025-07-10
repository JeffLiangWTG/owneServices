using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.CDSCashPayments.Testing
{
	[TestedType(typeof(CDSCashPaymentsForm))]
	class CDSCashPaymentsGUITests : ZFormBasherTest
	{
		public void TestControls()
		{
			using (var form = GetFormToBashCore())
			{
				AssertNotNull("CDSCashPaymentsUserControl", CDSCashPaymentsHelper.GetSingleControlOrNull<CDSCashPaymentsUserControl>(form, "CDSCashPaymentsUserControl"));
			}
		}

		public void TestCaption()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				AssertContains("CDS Cash & PVA Payments", form.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return CDSCashPaymentsHelper.GetFormForTest(Factory);
		}
	}
}

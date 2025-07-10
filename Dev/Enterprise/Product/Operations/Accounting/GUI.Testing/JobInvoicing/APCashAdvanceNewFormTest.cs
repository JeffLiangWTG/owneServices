using System.Windows.Forms;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(APCashAdvanceNewForm))]
	public class APCashAdvanceNewFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var charge = Factory.NewWithValidTestData<ChargeWithCost>();
			Factory.Save();
			return new APCashAdvanceNewForm(charge);
		}

		public void TestFormCaption()
		{
			using (var form = (APCashAdvanceNewForm)GetFormToBash())
			{
				AssertEquals("Form caption", "AP Advance Payment", form.FormCaption);
			}
		}

		public void TestFormVerb()
		{
			var charge = Factory.NewWithValidTestData<ChargeWithCost>();
			using (var form = new APCashAdvanceNewForm(charge))
			{
				form.DisplayMode = ODisplayMode.New;
				AssertEquals("Form verb", "New", form.FormVerb);

				form.DisplayMode = ODisplayMode.ReadOnly;
				AssertEquals("Form verb", "View", form.FormVerb);
			}
		}
	}
}

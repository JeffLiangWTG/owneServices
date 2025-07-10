using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(APCashAdvanceViewForm))]
	public class APCashAdvanceViewFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var cah = Factory.NewWithValidTestData<CashAdvanceRequestHeader>();
			cah.CAH_Ledger = LedgerTypes.AccountsPayable;
			cah.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			Factory.Save();
			return new APCashAdvanceViewForm(cah);
		}

		public void TestFormCaption()
		{
			using (var form = (APCashAdvanceViewForm)GetFormToBash())
			{
				AssertEquals("Form caption", "AP Advance Payment", form.FormCaption);
			}
		}

		public void TestFormVerb()
		{
			var cah = Factory.NewWithValidTestData<CashAdvanceRequestHeader>();
			using (var form = new APCashAdvanceViewForm(cah))
			{
				form.DisplayMode = ODisplayMode.New;
				AssertEquals("Form verb", "New", form.FormVerb);

				form.DisplayMode = ODisplayMode.ReadOnly;
				AssertEquals("Form verb", "View", form.FormVerb);
			}
		}
	}
}

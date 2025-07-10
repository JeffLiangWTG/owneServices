using System.Windows.Forms;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.GUI.Testing
{
	[TestedType(typeof(FaxPricesForm))]
	internal sealed class FaxPricesFormBasherTest : ZFormBasherTest
	{
		[TestDate(2013, 7, 1)]
		public void TestBulkClientFaxPriceUpdater_HasChangesChanged()
		{
			BulkClientFaxPriceUpdater updater = new BulkClientFaxPriceUpdater(Factory);
			using (FaxPricesForm form = new FaxPricesForm(updater))
			{
				form.Show();
				AssertEquals("Text on the warning label", "Save your changes before changing the date", form.SaveChangesWarningLabel.Text);
				Application.DoEvents();

				ClientFaxPrice newPrice = updater.ClientFaxPriceList.AddNew();
				newPrice.CFP_RX_NKCurrencyCode = "USD";
				Assert("Pre-condition: list/children has changes", updater.ClientFaxPriceList.HasChanges);
				Assert("Label should be visible", form.SaveChangesWarningLabel.Visible);
				Assert("DateEdit should be read-only", form.MonthAndYearPeriodDateEdit.ReadOnly);

				Factory.Save();

				Assert("Pre-condition: list/children has no changes", !updater.ClientFaxPriceList.HasChanges);
				Assert("Label should not be visible", !form.SaveChangesWarningLabel.Visible);
				Assert("DateEdit should not be read-only", !form.MonthAndYearPeriodDateEdit.ReadOnly);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new FaxPricesForm(new BulkClientFaxPriceUpdater(Factory));
		}

		#endregion
	}
}

using System.Windows.Forms;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.GUI.EInvoicing.PenaltyTaxMessage;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing.EInvoicing.PenaltyTaxMessage
{
	[TestedType(typeof(KoreaSouthEInvoicingPenaltyTaxInfoForm))]
	public class KoreaSouthPenaltyTaxInfoFormBasherTest : ZFormBasherTest
	{
		public void TestInitializeMessageBox()
		{
			using (var koreaSouthPenaltyTaxInfoForm = GetFormToBashCore() as KoreaSouthEInvoicingPenaltyTaxInfoForm)
			{
				koreaSouthPenaltyTaxInfoForm.Show();
				TestButton_Click();

				var penaltyTaxForNonIssuedTableTitleLabel = koreaSouthPenaltyTaxInfoForm.GetControl<ZLabel>("PenaltyTaxForNonIssuedTableTitleLabel");
				var penaltyTaxForNonTransmitTableTitleLabel = koreaSouthPenaltyTaxInfoForm.GetControl<ZLabel>("PenaltyTaxForNonTransmitTableTitleLabel");
				var penaltyTaxForNonIssuedTableNotificationLabel = koreaSouthPenaltyTaxInfoForm.GetControl<ZLabel>("PenaltyTaxForNonIssuedTableNotificationLabel");
				var penaltyTaxForNonTransmitTableNotificationLabel = koreaSouthPenaltyTaxInfoForm.GetControl<ZLabel>("PenaltyTaxForNonTransmitTableNotificationLabel");
				var penaltyTaxForNonIssuedTableNotification2Label = koreaSouthPenaltyTaxInfoForm.GetControl<ZLabel>("PenaltyTaxForNonIssuedTableNotification2Label");
				var penaltyTaxForNonTransmitTableNotification2Label = koreaSouthPenaltyTaxInfoForm.GetControl<ZLabel>("PenaltyTaxForNonTransmitTableNotification2Label");

				AssertEquals("Additional Tax Information", koreaSouthPenaltyTaxInfoForm.CaptionResourceString.Caption);
				AssertEquals("Penalty tax for non-issued or delayed issued invoice", penaltyTaxForNonIssuedTableTitleLabel.CaptionResourceString.Caption);
				AssertEquals("Penalty tax for non-transmitted or delayed transmitted invoice", penaltyTaxForNonTransmitTableTitleLabel.CaptionResourceString.Caption);
				AssertEquals("An electronic tax invoice must be issued not later than tenth day of the month following the month in which the date of supply of goods or services falls.", penaltyTaxForNonIssuedTableNotificationLabel.CaptionResourceString.Caption);
				AssertEquals("If the day falls on a Saturday or a public holiday, it will be extended to the next business day.", penaltyTaxForNonIssuedTableNotification2Label.CaptionResourceString.Caption);
				AssertEquals("An electronic tax invoice must be transmitted to the NTS by the next day of the issuance(electronic signature) date.", penaltyTaxForNonTransmitTableNotificationLabel.CaptionResourceString.Caption);
				AssertEquals("If the day falls on a Saturday or a public holiday, it will be extended to the next business day.", penaltyTaxForNonTransmitTableNotification2Label.CaptionResourceString.Caption);
			}
		}

		public void TestButton_Click()
		{
			using (var koreaSouthPenaltyTaxInfoForm = GetFormToBashCore() as KoreaSouthEInvoicingPenaltyTaxInfoForm)
			{
				koreaSouthPenaltyTaxInfoForm.Show();
				var button = koreaSouthPenaltyTaxInfoForm.GetControl<ZButton>("OKButton");

				AssertEquals("Precondition", false, koreaSouthPenaltyTaxInfoForm.IsDisposed);
				button.PerformClick();
				AssertEquals(true, koreaSouthPenaltyTaxInfoForm.IsDisposed);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new KoreaSouthEInvoicingPenaltyTaxInfoForm(new KoreaSouthEInvoicingPenaltyTaxInfo());
		}

		#endregion
	}
}

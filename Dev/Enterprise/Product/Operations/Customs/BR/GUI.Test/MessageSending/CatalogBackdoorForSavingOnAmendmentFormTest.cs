using System.Windows.Forms;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(CatalogBackdoorForSavingOnAmendmentForm))]
	class CatalogBackdoorForSavingOnAmendmentFormTest : ZFormBasherTest
	{
		public void TestFormLabels()
		{
			using (var form = GetFormToBashCore() as CatalogBackdoorForSavingOnAmendmentFormForTesting)
			{
				form.Show();
				AssertEquals(" Save without sending the message. The message status will be set to \'NOT - Not Sent\' until the message is Sent.", form.SaveWithoutSendingToDeferAmendmentSendingRadioButtonForTesting.CaptionResourceString.Caption);
				AssertEquals("The system has detected an amendment is required to be sent to Customs. Changes made affect the message content of the catalog. Please select an option and \"OK\". Alternatively select \"CANCEL\" which cancels the process of amendment.", form.TextLabelForTesting.Text);
			}
		}

		public void TestControlVisibility()
		{
			using (var form = GetFormToBashCore() as CatalogBackdoorForSavingOnAmendmentFormForTesting)
			{
				form.Show();
				AssertEquals("SaveWithoutSendingToDeferAmendmentSendingRadioButton should be visible", true, form.SaveWithoutSendingToDeferAmendmentSendingRadioButtonForTesting.Visible);
				AssertEquals("SendWithoutSendingAmendmentAtAllRadioButton should not be visible", false, form.SendWithoutSendingAmendmentAtAllRadioButtonForTesting.Visible);
				AssertEquals("SendAmendmentRadioButton should not be visible", false, form.SendAmendmentRadioButtonForTesting.Visible);
				AssertEquals("SaveWithEntryChangesExplanationButton should not be visible", false, form.SaveWithEntryChangesExplanationButtonForTesting.Visible);
				AssertEquals("SaveWithoutEntryChangesExplanationButton should not be visible", false, form.SaveWithoutEntryChangesExplanationButtonForTesting.Visible);
			}
		}

		protected override Form GetFormToBashCore() => new CatalogBackdoorForSavingOnAmendmentFormForTesting(new CatalogDeferredAmendmentSavingOptions());

		class CatalogBackdoorForSavingOnAmendmentFormForTesting : CatalogBackdoorForSavingOnAmendmentForm
		{
			public CatalogBackdoorForSavingOnAmendmentFormForTesting(CatalogDeferredAmendmentSavingOptions savingOptions)
				: base(savingOptions)
			{
			}

			public ZRadioButton SaveWithoutSendingToDeferAmendmentSendingRadioButtonForTesting => SaveWithoutSendingToDeferAmendmentSendingRadioButton;

			public ZRadioButton SendWithoutSendingAmendmentAtAllRadioButtonForTesting => SendWithoutSendingAmendmentAtAllRadioButton;

			public ZRadioButton SendAmendmentRadioButtonForTesting => SendAmendmentRadioButton;

			public ZButton SaveWithEntryChangesExplanationButtonForTesting => SaveWithEntryChangesExplanationButton;

			public ZButton SaveWithoutEntryChangesExplanationButtonForTesting => SaveWithoutEntryChangesExplanationButton;

			public ZLabel TextLabelForTesting => TextLabel;

			public string ExplanationForSaveWithEntryChangesForTesting => ExplanationForSaveWithEntryChanges;
		}
	}
}

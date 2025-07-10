using System.Windows.Forms;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(BackdoorForSavingOnAmendmentForm))]
	class BackdoorForSavingOnAmendmentFormTest : ZFormBasherTest
	{
		public void TestFormLabels()
		{
			using (var form = GetFormToBashCore() as BackdoorForSavingOnAmendmentFormForTesting)
			{
				form.Show();
				AssertEquals(@"If you have made changes that affect Customs Entries and does not want to send the amendment  message now, please choose this option.
The system will take a rectification reason and lets you save without send a rectification message.
But you will be able to see the reasons entered on the Workflow> DAP event prior send the rectification message.
You can send a Rectification message later by Clicking Brokerage> Send to Customs.
Message Status will show ""Not Sent"" until you send Rectification Message.", form.ExplanationForSaveWithEntryChangesForTesting);

				AssertEquals(" Save WITHOUT sending amendment. Message Status will show \"Not Sent\" until you send Rectification Message.", form.SaveWithoutSendingToDeferAmendmentSendingRadioButtonForTesting.CaptionResourceString.Caption);
			}
		}

		protected override Form GetFormToBashCore() => new BackdoorForSavingOnAmendmentFormForTesting(new DeclarationDeferredAmendmentSavingOptions(Factory.NewWithValidTestData<JobDeclaration>()));

		class BackdoorForSavingOnAmendmentFormForTesting : BackdoorForSavingOnAmendmentForm
		{
			public BackdoorForSavingOnAmendmentFormForTesting(DeclarationDeferredAmendmentSavingOptions savingOptions)
				: base(savingOptions)
			{
			}

			public ZRadioButton SaveWithoutSendingToDeferAmendmentSendingRadioButtonForTesting => SaveWithoutSendingToDeferAmendmentSendingRadioButton;

			public string ExplanationForSaveWithEntryChangesForTesting => ExplanationForSaveWithEntryChanges;
		}
	}
}

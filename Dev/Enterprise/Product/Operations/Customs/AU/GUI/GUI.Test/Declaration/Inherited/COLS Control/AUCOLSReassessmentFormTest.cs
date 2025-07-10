using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.Testing
{
	[TestedType(typeof(AUCOLSReassessmentForm))]
	sealed class AUCOLSReassessmentFormTest : ZFormBasherTest
	{
		public void TestSendButton() => CombineAssertions(() =>
		{
			using (var form = new AUCOLSReassessmentForm(colsHeader, new COLSDeclarationAcceptance()))
			{
				AssertEquals("Caption", "Send", form.SendButton.Text);
				AssertEquals("By default Send button is disabled", false, form.SendButton.Enabled);
				form.DeclarationAgreementControl.AcceptCheckBox.Checked = true;
				form.ReassessmentReasonTextBox.Text = "A";
				AssertEquals("Send button is enabled when AcceptCheckBox is ticked and ReassessmentReason is specified", true, form.SendButton.Enabled);
				form.DeclarationAgreementControl.AcceptCheckBox.Checked = false;
				AssertEquals("Updating AcceptCheckBox updates the enabled property of SendButton", false, form.SendButton.Enabled);
				form.DeclarationAgreementControl.AcceptCheckBox.Checked = true;
				AssertEquals("Precondition: SendButton is enabled", true, form.SendButton.Enabled);
				form.ReassessmentReasonTextBox.Text = "";
				AssertEquals("Updating ReassessmentReason updates the enabled property of SendButton", false, form.SendButton.Enabled);
			}
		});

		public void TestCancelButton()
		{
			using (var form = new AUCOLSReassessmentForm(colsHeader, new COLSDeclarationAcceptance()))
			{
				AssertEquals("Caption", "Cancel", form.cancelButton.Text);
			}
		}

		public void TestReassessmentReasonGroupBox() => CombineAssertions(() =>
		{
			using (var form = new AUCOLSReassessmentForm(colsHeader, new COLSDeclarationAcceptance()))
			{
				AssertEquals("CharacterCountLabel Caption", "Character Count", form.CharacterCountLabel.Text);
				AssertEquals("Default ReassessmentReasonTextBox.Text", string.Empty, form.ReassessmentReasonTextBox.Text);
				AssertEquals("MaxLength", 1000, form.ReassessmentReasonTextBox.MaxLength);
				AssertEquals("CharacterCountLimitTextBox.Text", "1000", form.CharacterCountLimitTextBox.Text);
				AssertEquals("CharacterCountLimitTextBox.ReadOnly", true, form.CharacterCountLimitTextBox.ReadOnly);
				AssertEquals("Default CharacterCountTextBox.Text", "0", form.CharacterCountTextBox.Text);
				AssertEquals("CharacterCountTextBox.ReadOnly", true, form.CharacterCountTextBox.ReadOnly);
				form.ReassessmentReasonTextBox.Text = "ABC";
				AssertEquals("Updating ReassessmentReasonTextBox updates CharacterCountTextBox", "3", form.CharacterCountTextBox.Text);
			}
		});

		public void TestReassessmentReason()
		{
			using (var form = new AUCOLSReassessmentForm(colsHeader, new COLSDeclarationAcceptance()))
			{
				form.ReassessmentReasonTextBox.Text = "Hello World";
				AssertEquals("Hello World", form.ReassessmentReason);
			}
		}

		public void TestRequireDocumentationCheckBox() => CombineAssertions(() =>
		{
			using (var form = new AUCOLSReassessmentForm(colsHeader, new COLSDeclarationAcceptance()))
			{
				AssertEquals("Caption", "Documentation Required", form.RequireDocumentationCheckBox.Text);
				AssertEquals("By default it's unchecked", false, form.RequireDocumentationCheckBox.Checked);
			}
		});

		public void TestRequireDocumentation()
		{
			using (var form = new AUCOLSReassessmentForm(colsHeader, new COLSDeclarationAcceptance()))
			{
				AssertEquals("RequireDocumentationCheckBox unchecked", false, form.RequireDocumentation);

				form.RequireDocumentationCheckBox.Checked = true;
				AssertEquals("RequireDocumentationCheckBox checked", true, form.RequireDocumentation);
			}
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			var controlName = control.Name;
			if (controlName == "RequireDocumentationCheckBox" || controlName == "CharacterCountLimitTextBox"
				|| controlName == "CharacterCountTextBox" || controlName == "ReassessmentReasonTextBox")
			{
				return true;
			}

			return base.ShouldIgnoreMissingBindingMember(control);
		}

		protected override Form GetFormToBashCore()
		{
			return new AUCOLSReassessmentForm(colsHeader, new COLSDeclarationAcceptance());
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
		}
		QuarantineColsHeader colsHeader;
	}
}

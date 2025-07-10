using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class AccountVerificationControlForTest : AccountVerificationControl
	{
		public AccountVerificationControlForTest()
		{
			InitialiseControls();
		}

		public void InitialiseControls()
		{
			HeaderDiv = new HtmlGenericControl();
			MessageLabel = new ZTextLabel();
			ActionInstructionAccountReactivatedLabel = new ZTextLabelNoEncode();
			ActionInstructionEmailChangedLabel = new ZTextLabelNoEncode();
			ActionInstructionMultipleUIDLinkedLabel = new ZTextLabelNoEncode();
			ActionInstructionContactMovedLabel = new ZTextLabelNoEncode();
			PasswordMessageLabel = new ZTextLabel();
			PasswordTextbox = new ZTextBox();
			VerifyPasswordButton = new Button();
			EmailMessageLabel = new ZTextLabel();
			EmailLabel = new ZTextLabel();
			SendVerificationEmailButton = new LinkButton();
			ERequestMessageLabel = new ZTextLabel();
		}

		public Label MessageLabelExposed => MessageLabel;
		public Label ActionInstructionAccountReactivatedLabelExposed => ActionInstructionAccountReactivatedLabel;
		public Label ActionInstructionEmailChangedLabelExposed => ActionInstructionEmailChangedLabel;
		public Label ActionInstructionMultipleUIDLinkedLabelExposed => ActionInstructionMultipleUIDLinkedLabel;
		public Label ActionInstructionContactMovedLabelExposed => ActionInstructionContactMovedLabel;
		public Label PasswordMessageLabelExposed => PasswordMessageLabel;
		public ZTextBox PasswordTextboxExposed => PasswordTextbox;
		public Button VerifyPasswordButtonExposed => VerifyPasswordButton;
		public Label EmailMessageLabelExposed => EmailMessageLabel;
		public Label EmailLabelExposed => EmailLabel;
		public LinkButton SendVerificationEmailButtonExposed => SendVerificationEmailButton;
		public Label ERequestMessageLabelExposed => ERequestMessageLabel;
		public void OnLoad() => base.OnLoad(EventArgs.Empty);
		class GlobalForTest : Global
		{
			public void OnCustomSessionStart()
			{
				base.OnCustomSessionStart(this, EventArgs.Empty);
			}
		}
	}
}

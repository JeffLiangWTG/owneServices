using System.Linq;
using Enterprise.Customs.DE.Business;

namespace Enterprise.Customs.DE.GUI
{
	public partial class FinalizeAVABRForm : MessageSendingForm<FinalizeAVABRMessageSendingActionParent>
	{
		public FinalizeAVABRForm(FinalizeAVABRMessageSendingActionParent parent) : base(parent, Res.GetString("309CDBAE-265E-45E0-B2C6-927886A61A8D", "Finalize"))
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			UpdateSendButtonCaption();
		}

		void UpdateSendButtonCaption()
		{
			var sendButton = GetEffectiveSendButton();
			sendButton.CaptionResourceString = Res.GetData("2F684986-9ECF-437A-AAE0-4F148076AA36", "Finalize");
		}

		protected override void ChangeSendButtonAvailability()
		{
			base.ChangeSendButtonAvailability();

			var finalizeAction = (FinalizeAVABRMessageSendingActionParent)MessageSendingObjectParent;
			var canSendSelectedActions = finalizeAction.SelectedSendingObjects
				.Cast<FinalizeAVABREntryMessageSendingAction>().All(action => action.CanSend);

			var sendButton = GetEffectiveSendButton();
			sendButton.Enabled = sendButton.Enabled && canSendSelectedActions;
		}

		public override string FormHeading => Res.GetString("309CDBAE-265E-45E0-B2C6-927886A61A8D", "Finalize");
	}
}

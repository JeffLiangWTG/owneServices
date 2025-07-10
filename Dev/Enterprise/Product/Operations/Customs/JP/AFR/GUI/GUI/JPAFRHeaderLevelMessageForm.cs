using System;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.AFR.GUI
{
	public partial class JPAFRHeaderLevelMessageForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public JPAFRHeaderLevelMessageForm()
		{
			InitializeComponent();
		}

		public JPAFRHeaderLevelMessageForm(MessageSendingAction sendingAction)
			: base(sendingAction)
		{
			InitializeComponent();

			if (sendingAction.ActionCode == ActionCode.RegisterDepartureTime)
			{
				this.MessageLabel.CaptionResourceString = MessageLabelCaptionForATD;
				this.HasATDBeenSentCheckBox.CaptionResourceString = Res.GetData("3690D94F-EAD2-4C2B-913F-96269998B36E", "Send ATD Update message");
				this.CaptionResourceString = Res.GetData("B41197BE-358C-47C7-9225-923ADD48B3D2", "Do you want to register the Departure Time?");

				this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 400, true);
				BusinessEntity.OnHasATDBeenSentChanged -= UpdateMessageSendingActionForATDMessage;
				BusinessEntity.OnHasATDBeenSentChanged += UpdateMessageSendingActionForATDMessage;
			}
			else
			{
				BusinessEntity.OnHasATDBeenSentChanged -= UpdateMessageSendingActionForCompletionMessage;
				BusinessEntity.OnHasATDBeenSentChanged += UpdateMessageSendingActionForCompletionMessage;
			}
		}

		public new MessageSendingAction BusinessEntity
		{
			get { return (MessageSendingAction)base.BusinessEntity; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				UnhookUpdateMessageSendingAction();
			}
			base.Dispose(disposing);
		}

		void UnhookUpdateMessageSendingAction()
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.OnHasATDBeenSentChanged -= UpdateMessageSendingActionForCompletionMessage;
				BusinessEntity.OnHasATDBeenSentChanged -= UpdateMessageSendingActionForATDMessage;
			}
		}

		#region Button Action

		void sendButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			UnhookUpdateMessageSendingAction();
			Close();
		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			UnhookUpdateMessageSendingAction();
			Close();
		}

		#endregion

		#region ForCompletionMessage

		void UpdateMessageSendingActionForCompletionMessage(object sender, EventArgs e)
		{
			BusinessEntity.UpdateMessageSendingAction(BusinessEntity.HasATDBeenSent ? ActionCode.RegisterCompletionByAmendment : ActionCode.RegisterCompletionByRegistration);
		}

		#endregion

		#region ForATDMessage

		void UpdateMessageSendingActionForATDMessage(object sender, EventArgs e)
		{
			BusinessEntity.UpdateMessageSendingAction(BusinessEntity.HasATDBeenSent ? ActionCode.ChangeDepartureTimeAfterATD : ActionCode.RegisterDepartureTime);
		}

		static ResourceStringData MessageLabelCaptionForATD
		{
			get
			{
				return Res.GetData("6158D0F9-5643-49CD-AB02-56CB55DC1FC1", @"Do you want to register the Departure Time (ATD) for the manifest?

Once the Departure Time is successfully registered the departure time can be amended.

1. If you do need to change the Vessel Information* after a successful lodgement of Departure Time Registration, you will need to submit an amendment (‘Amend Manifest’ menu item), after which a new Departure Time Registration will need to be sent.

2. Note that once Departure Time is registered, bill details can only be amended on receipt of a customs assessment notice or by re-manifesting the bills onto a new manifest (vessel information* should change).

*(Carrier Code, Vessel Call Sign, Voyage Number, Port of Loading and Suffix)

Please ensure that all data is correct before proceeding.


If you have already registered the Departure Time (ATD) but accidentally canceled the 'Departure Time Registration' event, please tick the check box 'Send ATD update message' and send again.");
			}
		}

		#endregion
	}
}

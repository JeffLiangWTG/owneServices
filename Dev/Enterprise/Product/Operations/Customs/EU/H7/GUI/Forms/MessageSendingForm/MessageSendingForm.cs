using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public partial class MessageSendingForm : Customs.GUI.MessageSendingFormWithValidationDetails
	{
		[Obsolete("Do not call. Only for designer use.")]
		public MessageSendingForm()
		{
		}

		public MessageSendingForm(BaseMessageSendingObjectParent parent)
			: base(parent)
		{
			InitializeComponent();
			CustomizeLayout();
		}

		void CustomizeLayout()
		{
			messageSendingObjectsGroupBox.SuspendLayout();
			MessageSendingObjectsGrid.SuspendLayout();
			SuspendLayout();

			messageSendingObjectsGroupBox.ResumeLayout(false);
			messageSendingObjectsGroupBox.PerformLayout();
			MessageSendingObjectsGrid.ResumeLayout(false);
			MessageSendingObjectsGrid.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		protected override void AddUserControlToBottomSection()
		{
			base.AddUserControlToBottomSection();

			if (AllowOverrideMessageType)
			{
				AddOverrideUserControl("OverrideMessageTypeControl",
					nameof(MessageSendingObjectParent<MessageSendingObject>.OverrideDefaultAction),
					nameof(MessageSendingObjectParent<MessageSendingObject>.Action)
				);
			}

			if (AllowOverrideAmendmentReason)
			{
				AddOverrideUserControl("OverrideAmendmentReasonControl",
					nameof(MessageSendingObjectParent<MessageSendingObject>.OverrideAmendmentReason),
					nameof(MessageSendingObjectParent<MessageSendingObject>.AmendmentReason)
				);
			}

			if (AllowOverrideCancellationReason)
			{
				AddOverrideUserControl("OverrideCancellationReasonControl",
					nameof(MessageSendingObjectParent<MessageSendingObject>.OverrideCancellationReason),
					nameof(MessageSendingObjectParent<MessageSendingObject>.CancellationReason));
			}

			AddSelectAllButton();
		}

		void AddOverrideUserControl(string controlName, string bindToOverride, string bindToCode)
		{
			var control = new OverrideUserControl();
			control.Name = controlName;
			control.BindToOverride = bindToOverride;
			control.BindToCode = bindToCode;
			control.Dock = DockStyle.Bottom;
			WarningSplitContainer.Panel2.Controls.Add(control);
		}

		protected void AddSelectAllButton()
		{
			var selectAllButton = new ZButton();
			selectAllButton.IsCaptionOverridden = true;
			selectAllButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			selectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 433, true);
			selectAllButton.Name = "selectAllButton";
			selectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 21, true);
			selectAllButton.TabIndex = 8;
			selectAllButton.Text = Res.GetString("64833fe7-1b04-4a60-94d4-7c8d300d5481", "Select/Deselect All");
			selectAllButton.ToolTipCaption = null;
			selectAllButton.UseVisualStyleBackColor = true;
			selectAllButton.Click += new EventHandler(SelectAllButton_Click);
			Controls.Add(selectAllButton);
			Controls.SetChildIndex(selectAllButton, 0);

			SendButton.TabIndex = 9;
			CancelButton2.TabIndex = 10;
		}

		void SelectAllButton_Click(object sender, EventArgs e)
		{
			var shouldSelectAll = MessageSendingObjectParent.SendingObjectsCollection.Count != MessageSendingObjectParent.SelectedSendingObjects.Count();

			foreach (var messageSendingObject in MessageSendingObjectParent.SendingObjectsCollection.Cast<BaseMessageSendingObject>())
			{
				messageSendingObject.ShouldSend = shouldSelectAll;
			}
		}

		protected override bool PreviewMessageCheckboxVisible => true;

		protected override void SendButton_ClickCore()
		{
			base.SendButton_ClickCore();

			var progressForm = CreateProgressForm();

			try
			{
				var messagesSentCount = SendMessageToCustoms(ProgressUpdateCallBack(progressForm));

				if (messagesSentCount > 0)
				{
					TrySaveAndShowMessage(BusinessEntity.Factory, messagesSentCount);
				}
			}
			catch (Exception e)
			{
				ErrorReporter.ReportOnce("H7MessageSendingFailure", "Exception occurred when attempting to send message", e);
			}

			progressForm?.Close();
		}

		protected virtual bool ShouldShowProgressForm => !ShouldSendSingleMessageForMultipleObjects;

		protected virtual ProgressForm CreateProgressForm()
		{
			if (ShouldShowProgressForm)
			{
				var progressForm = new ProgressForm();

				progressForm.ShowCancelButton = false;
				progressForm.CaptionRenderingEnabled = true;
				progressForm.CaptionResourceString = ProgressFormCaption;
				progressForm.ShowModalTo(this);

				return progressForm;
			}

			return null;
		}

		protected virtual Action<int, int> ProgressUpdateCallBack(ProgressForm progressForm)
		{
			if (ShouldShowProgressForm)
			{
				return (int messagesSent, int messagesToBeSend) =>
				{
					var status = $"[{messagesSent} / {messagesToBeSend}] {ProgressFormStatusText}";
					var percent = (int)(messagesSent * 100F / messagesToBeSend);

					progressForm?.SetStatusAndPercentComplete(status, percent);
				};
			}
			else
			{
				return (int messagesSent, int messagesToBeSend) => { };
			}
		}

		protected virtual int SendMessageToCustoms(Action<int, int> updateProgressCallback)
		{
			var messagesSent = 0;
			var messagesToSend = BusinessEntity.SelectedSendingObjects.Count();

			foreach (var sendingObject in BusinessEntity.SelectedSendingObjects.OfType<IH7MessageSendingObject>())
			{
				var sender = sendingObject.CreateSender();

				if (sender.Send() != null)
				{
					messagesSent++;
					updateProgressCallback?.Invoke(messagesSent, messagesToSend);
				}
			}

			return messagesSent;
		}

		void TrySaveAndShowMessage(BusinessObjectFactory factory, int messagesCreated)
		{
			try
			{
				factory.Save();
				var message = Res.GetString("af7fe448-a40b-4ee7-9eb2-ffc1b08b4aef", "{0} message(s) queued for sending.", messagesCreated);
				Globals.Message.ShowInformation(message);
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		protected override IGridColumnLayoutProvider MessageSendingGridColumnLayoutProvider => messageSendingGridColumnLayoutProvider ?? (messageSendingGridColumnLayoutProvider = GetNewColumnLayoutProvider());
		IGridColumnLayoutProvider messageSendingGridColumnLayoutProvider;

		protected virtual IGridColumnLayoutProvider GetNewColumnLayoutProvider() => new MessageSendingGridColumnLayout();

		protected virtual bool AllowOverrideMessageType => true;

		protected virtual bool AllowOverrideAmendmentReason => false;

		protected virtual bool AllowOverrideCancellationReason => false;

		ResourceStringData ProgressFormCaption => Res.GetData("b9e4909e-4be7-4c0c-b117-dedb915147cf", "Sending H7 messages");

		string ProgressFormStatusText => Res.GetString("953c76e1-1518-49a9-815a-754c4454c763", "H7 Messages queued");
	}
}

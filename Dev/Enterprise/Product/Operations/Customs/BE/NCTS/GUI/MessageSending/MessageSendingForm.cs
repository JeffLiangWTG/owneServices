using System;
using System.Windows.Forms;
using Enterprise.Customs.BE.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.NCTS.GUI
{
	public partial class MessageSendingForm : EU.NCTS.GUI.MessageSendingForm
	{
		public MessageSendingForm(MessageSendingActionParent sendingObjectWrapper) : base(sendingObjectWrapper)
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			var parent = MessageSendingObjectParent;
			if (parent != null)
			{
				foreach (MessageSendingAction action in parent.SendingObjectsCollection)
				{
					action.EntryTypeInfo.ValueChanged -= EntryTypeInfo_ValueChanged;
					action.EntryTypeInfo.ValueChanged += EntryTypeInfo_ValueChanged;
				}
			}
			SurpressValidationsByEntryType();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
				var parent = MessageSendingObjectParent;
				if (parent != null)
				{
					foreach (MessageSendingAction action in parent.SendingObjectsCollection)
					{
						action.EntryTypeInfo.ValueChanged -= EntryTypeInfo_ValueChanged;
					}
				}
			}
			base.Dispose(disposing);
		}

		public new MessageSendingActionParent MessageSendingObjectParent => base.MessageSendingObjectParent as MessageSendingActionParent;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			MessageSendingObjectsGrid.AfterBind += MessageSendingObjectsGrid_AfterBind;
		}

		void MessageSendingObjectsGrid_AfterBind(object sender, EventArgs e)
		{
			if (MessageSendingObjectsGrid.ListManager is CurrencyManager currencyManager)
			{
				currencyManager.CurrentChanged += ListManager_CurrentChanged;
			}
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			bottomSectionUserControl.EntryTypeChanged((MessageSendingAction)MessageSendingObjectsGrid.ListManager.GetCurrent());
		}

		void EntryTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			SurpressValidationsByEntryType();
		}

		protected override ZUserControl GetBottomSectionUserControl()
		{
			if (bottomSectionUserControl == null)
			{
				bottomSectionUserControl = new MessageSendingFormBottomSectionUserControl();
			}
			return bottomSectionUserControl;
		}
		MessageSendingFormBottomSectionUserControl bottomSectionUserControl;

		void SurpressValidationsByEntryType()
		{
			ChangeSendWithValidationErrorsCheckBoxAvailability();
			ChangeValidationErrorsTextBoxAvailability();
			ChangeSendButtonAvailability();
		}

		protected override bool SendWithValidationErrorsTextBoxVisible => MessageSendingObjectParent?.ShowValidationErrors ?? false;
	}
}

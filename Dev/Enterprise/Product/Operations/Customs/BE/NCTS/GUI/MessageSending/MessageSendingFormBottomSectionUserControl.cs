using Enterprise.Customs.BE.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.NCTS.GUI
{
	public partial class MessageSendingFormBottomSectionUserControl : ZUserControl
	{
		public MessageSendingFormBottomSectionUserControl()
		{
			InitializeComponent();
			AfterFirstBinding += MessageSendingFormBottomSectionUserControl_AfterFirstBinding;
		}

		void MessageSendingFormBottomSectionUserControl_AfterFirstBinding(object sender, System.EventArgs e)
		{
			dataSource = (MessageSendingActionParent)DataSource;
			foreach (MessageSendingAction action in dataSource.SendingObjectsCollection)
			{
				action.EntryTypeInfo.ValueChanged += EntryTypeInfo_ValueChanged;
				EntryTypeChanged(action);
			}
		}

		MessageSendingActionParent dataSource;
		void EntryTypeInfo_ValueChanged(object sender, System.EventArgs e)
		{
			EntryTypeChanged((MessageSendingAction)sender);
		}

		internal void EntryTypeChanged(MessageSendingAction action)
		{
			SetControlsVisibility(action);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
				if (dataSource != null)
				{
					foreach (MessageSendingAction action in dataSource.SendingObjectsCollection)
					{
						action.EntryTypeInfo.ValueChanged -= EntryTypeInfo_ValueChanged;
					}
				}
			}
			base.Dispose(disposing);
		}

		void SetControlsVisibility(MessageSendingAction action)
		{
			var isEntryTypeInv = false;
			var isEntryTypeRNM = false;
			var showPresentationDateTime = false;
			bool showAgreeWithMinorDiscrepancies = false;
			if (action != null)
			{
				var actionEntryType = action.EntryType;
				var actionEntryStatus = action.EntryStatus;
				isEntryTypeInv = actionEntryType == NctsMessageTypeList.Codes.InvalidationCancellation;
				isEntryTypeRNM = actionEntryType == NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement;
				showPresentationDateTime = actionEntryType == NctsMessageTypeList.Codes.Amendment || actionEntryStatus == NctsTransitStatusList.Codes.DeclarationRejected || actionEntryStatus == NctsTransitStatusList.Codes.DeclarationAccepted || !action.PresentationDateTimeReadOnly;
				showAgreeWithMinorDiscrepancies = actionEntryType == NctsMessageTypeList.Codes.RequestARelease;
			}
			PresentationDateAndTimeOffsetEdit.Visible = showPresentationDateTime;
			JustificationTextBox.Visible = isEntryTypeInv;
			TCI11DateEdit.Visible = isEntryTypeRNM;
			QueryInformationTextBox.Visible = isEntryTypeRNM;
			ActualConsigneeDocAddressControl.Visible = isEntryTypeRNM;
			ActualOfficeOfDestinationFindBox.Visible = isEntryTypeRNM;
			AgreeWithMinorDiscrepanciesCheckBox.Visible = showAgreeWithMinorDiscrepancies;
		}
	}
}

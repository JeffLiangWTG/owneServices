using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class MessageSendingForm<TSendingAction> : MessageSendingFormWithValidationDetails
		where TSendingAction : CusEntryHeaderMessageSendingAction
	{
		public MessageSendingForm(CusEntryHeaderMessageSendingActionParent<TSendingAction> parent) : base(parent)
		{
			sendingActionParent = parent;
		}

		readonly CusEntryHeaderMessageSendingActionParent<TSendingAction> sendingActionParent;

		public ZString ConfirmReason;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			InitializeNewColumns();
			MessageSendingObjectsGrid.AfterBind += new EventHandler(MessageSendingObjectGrid_AfterBind);
		}

		protected override void AddUserControlToBottomSection()
		{
			WarningSplitContainer.Panel2Collapsed = true;
		}

		protected override MessageSendingNotificationCollection RunPreSendValidation()
		{
			sendingActionParent.JobDeclaration.ValidationModesCalculator.RecalculateValidationModes();
			return base.RunPreSendValidation();
		}

		protected override bool CheckIsOKToSend()
		{
			return ConfirmWhenAlreadySend() && base.CheckIsOKToSend();
		}

		bool ConfirmWhenAlreadySend()
		{
			var isComfirmed = true;
			if (sendingActionParent.IsNeedConfirm)
			{
				using (var form = new ConfirmSendForm())
				{
					if (isComfirmed = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
					{
						ConfirmReason = form.Reason;
					}
				}
			}
			else if (sendingActionParent.DuplicationPossible)
			{
				using (var form = new ConfirmSendForm(true))
				{
					if (isComfirmed = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
					{
						ConfirmReason = form.Reason;
					}
				}
			}

			return isComfirmed;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override bool PreviewMessageCheckboxVisible => true;

		void InitializeNewColumns()
		{
			var annotationMLTextBoxStyleInfo = new ZMultiLineTextBoxColumnInfo();
			annotationMLTextBoxStyleInfo.ColumnName = nameof(CusEntryHeaderMessageSendingAction.Annotation);
			annotationMLTextBoxStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			MessageSendingObjectsGrid.ColumnStyles.Add(annotationMLTextBoxStyleInfo);

			var altDateOfAcceptance = new ZDateEditColumnStyleInfo();
			altDateOfAcceptance.ColumnName = nameof(AISMessageSendingAction.AlternativeDateOfAcceptance);
			altDateOfAcceptance.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			altDateOfAcceptance.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			MessageSendingObjectsGrid.ColumnStyles.Add(altDateOfAcceptance);

			var customsReference = new ZTextBoxColumnStyleInfo();
			customsReference.ColumnName = nameof(AISMessageSendingAction.CustomsReferenceNumber);
			customsReference.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			MessageSendingObjectsGrid.ColumnStyles.Add(customsReference);

			var customsJustification = new ZTextBoxColumnStyleInfo();
			customsJustification.ColumnName = nameof(AISMessageSendingAction.CustomsJustification);
			customsJustification.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			MessageSendingObjectsGrid.ColumnStyles.Add(customsJustification);

			MessageSendingObjectsGrid.SetColumnGroupName(nameof(AISMessageSendingAction.AlternativeDateOfAcceptance), Res.GetData("4E511707-BAE4-427D-B56A-73D5C23B7A57", "Fallback Procedure"));
			MessageSendingObjectsGrid.SetColumnGroupName(nameof(AISMessageSendingAction.CustomsReferenceNumber), Res.GetData("4E511707-BAE4-427D-B56A-73D5C23B7A57", "Fallback Procedure"));
			MessageSendingObjectsGrid.SetColumnGroupName(nameof(AISMessageSendingAction.CustomsJustification), Res.GetData("4E511707-BAE4-427D-B56A-73D5C23B7A57", "Fallback Procedure"));
		}

		void MessageSendingObjectGrid_AfterBind(object sender, EventArgs e)
		{
			if (!(BindingSource.Current is AISMessageSendingActionParent parent && parent.JobDeclaration.IsImport))
			{
				var fallbackProcedureColumns = new[] {
					nameof(AISMessageSendingAction.AlternativeDateOfAcceptance),
					nameof(AISMessageSendingAction.CustomsReferenceNumber),
					nameof(AISMessageSendingAction.CustomsJustification)
				};

				MessageSendingObjectsGrid.SetColumnVisible(false, fallbackProcedureColumns);
				MessageSendingObjectsGrid.Columns.Remove(nameof(AISMessageSendingAction.AlternativeDateOfAcceptance));
				MessageSendingObjectsGrid.Columns.Remove(nameof(AISMessageSendingAction.CustomsReferenceNumber));
				MessageSendingObjectsGrid.Columns.Remove(nameof(AISMessageSendingAction.CustomsJustification));
			}
		}
	}
}

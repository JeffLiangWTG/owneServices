using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Shared.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public partial class MessageSendingForm : MessageSendingObjectForm, IMessageSendingForm
	{
		public MessageSendingForm()
		{
		}

		public MessageSendingForm(DeclarationMessageSendingObjectParent sendingObjectParent)
		: base(sendingObjectParent)
		{
			IsReadyForSending = sendingObjectParent.ParentDeclaration?.IsReadyForSending ?? false;
		}

		bool IsReadyForSending { get; }

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeNewColumns();
			InitializeComponent();
		}

		void InitializeNewColumns()
		{
			var procedureCodeDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			procedureCodeDropEditColumnStyleInfo.ColumnName = MessageSendingObject.JPSchema.ProcedureCode;
			procedureCodeDropEditColumnStyleInfo.IsReadOnly = true;
			procedureCodeDropEditColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			MessageSendingObjectsGrid.ColumnStyles.Add(procedureCodeDropEditColumnStyleInfo);

			var inputReferenceTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			inputReferenceTextBoxColumnStyleInfo.ColumnName = MessageSendingObject.JPSchema.InputReference;
			inputReferenceTextBoxColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			inputReferenceTextBoxColumnStyleInfo.IsReadOnly = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(inputReferenceTextBoxColumnStyleInfo);

			var entryNumberTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			entryNumberTextBoxColumnStyleInfo.ColumnName = MessageSendingObject.JPSchema.EntryNumber;
			entryNumberTextBoxColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			entryNumberTextBoxColumnStyleInfo.IsReadOnly = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(entryNumberTextBoxColumnStyleInfo);

			var entryStatusTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			entryStatusTextBoxColumnStyleInfo.ColumnName = MessageSendingObject.Schema.EntryStatus;
			entryStatusTextBoxColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			entryStatusTextBoxColumnStyleInfo.IsReadOnly = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(entryStatusTextBoxColumnStyleInfo);

			var statusTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			statusTextBoxColumnStyleInfo.ColumnName = MessageSendingObject.JPSchema.Status;
			statusTextBoxColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			statusTextBoxColumnStyleInfo.IsReadOnly = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(statusTextBoxColumnStyleInfo);

			var correctionCopyRequestzCheckBoxColumnStyleInfo = new ZCheckBoxColumnStyleInfo();
			correctionCopyRequestzCheckBoxColumnStyleInfo.ColumnName = MessageSendingObject.JPSchema.DeclarationCorrectionCopyRequest;
			correctionCopyRequestzCheckBoxColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			MessageSendingObjectsGrid.ColumnStyles.Add(correctionCopyRequestzCheckBoxColumnStyleInfo);

			if (BusinessEntity.Context.ProcedureCode == JPProcedureCodeList.Codes.ECR)
			{
				var actionTextBoxColumnStyleInfo = new ZDropEditColumnStyleInfo();
				actionTextBoxColumnStyleInfo.ColumnName = MessageSendingObject.JPSchema.Action;
				actionTextBoxColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
				MessageSendingObjectsGrid.ColumnStyles.Insert(1, actionTextBoxColumnStyleInfo);
			}
		}

		public override string FormHeading
		{
			get
			{
				var procedureCode = BusinessEntity?.Context?.ProcedureCode ?? string.Empty;
				return IsReadyForSending
					? Res.GetString("63043c16-e3c3-4be4-be57-6465abf632e7", "Send or Export {0} Messages", procedureCode)
					: Res.GetString("4d50f3d2-d4b6-40cc-98e5-9095ef5c030b", "Export {0} Messages", procedureCode);
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			ExportButton.Click -= ExportButton_Click;
			ExportPathButton.Click -= ExportPathButton_Click;

			ExportButton.DataBindings.RemoveBinding(nameof(ZButton.IsEnabledForBinding));
			SendButton.DataBindings.RemoveBinding(nameof(ZButton.IsEnabledForBinding));

			base.SetDataBinding(dataSource, dataMember);

			if (dataSource is DeclarationMessageSendingObjectParent declarationSendingObjectParent)
			{
				ExportPathButton.Click += ExportPathButton_Click;
				ExportButton.Click += ExportButton_Click;

				ExportButton.DataBindings.Add(new KBinding(nameof(ZButton.IsEnabledForBinding), declarationSendingObjectParent, nameof(declarationSendingObjectParent.AllowExportMessage)));
				SendButton.DataBindings.Add(new KBinding(nameof(ZButton.IsEnabledForBinding), declarationSendingObjectParent, nameof(declarationSendingObjectParent.AllowSendMessage)));
			}
		}

		void ExportPathButton_Click(object sender, EventArgs e)
		{
			using (var browser = new ZFolderBrowserDialog())
			{
				browser.ShowNewFolderButton = true;
				browser.RequireMappablePath = true;
				browser.Description = Res.GetString("01C6F584-9F80-4FE1-BFFD-410F50F77DC0", "Please select a valid directory path to export the message flat file.");

				if (ZFormModaliser.ShowCommonDialogWithoutDispose(browser) == System.Windows.Forms.DialogResult.OK)
				{
					BusinessEntity.ExportPath = browser.IsNeedingToUseEnterpriseChannel ? browser.UnmappedSelectedPath : browser.MappedSelectedPath;
				}
			}
		}

		void ExportButton_Click(object sender, EventArgs e)
		{
			((MessageSendingContext)BusinessEntity.Context).SendTarget = SendTarget.FlatFile;

			if (CheckIsOKToSend())
			{
				SendOrExportButtonClickCore();
				Close();
			}
		}

		protected override void SendButton_ClickCore()
		{
			((MessageSendingContext)BusinessEntity.Context).SendTarget = SendTarget.Normal;
			SendOrExportButtonClickCore();
		}

		protected override void CancelButton_ClickCore()
		{
			DialogResult = DialogResult.Cancel;
		}

		void SendOrExportButtonClickCore()
		{
			this.ShowEditableForm(BusinessEntity);
		}

		public new DeclarationMessageSendingObjectParent BusinessEntity => (DeclarationMessageSendingObjectParent)base.BusinessEntity;

		protected override bool CheckIsOKToSend()
		{
			var result = false;
			var sendingObjectParent = BusinessEntity;

			if (sendingObjectParent == null || (!sendingObjectParent.AllowEmptyDeclaration && !sendingObjectParent.HasAnyObjectToSend))
			{
				Globals.Message.ShowError(NothingSelectedMessage);
				return result;
			}

			var selectedSendingObjects = sendingObjectParent.SelectedSendingObjects.Cast<MessageSendingObject>();
			var declaration = sendingObjectParent.ParentDeclaration;
			var isTopLevel = declaration.IsTopLevel;

			try
			{
				declaration.IsTopLevel = false;
				sendingObjectParent.RegisterEditableChildObject(declaration);
				sendingObjectParent.Validation.ValidateAll();
				var notificationCollector = sendingObjectParent.Context.ProcedureCode switch
				{
					JPProcedureCodeList.Codes.ECR => new ECRMessageSendingNotificationCollector(sendingObjectParent, selectedSendingObjects.Select(x => x.Header)),
					_ => BusinessEntity.NotificationsIncludingChildren as ZNotificationCollector
				};
				if (notificationCollector.GetErrors().Any())
				{
					Globals.Message.ShowError(notificationCollector.GenerateFormattedErrors());
				}
				else if (!notificationCollector.GetMessageErrors().Any())
				{
					result = true;
				}
				else if (ContinueToSendWithWarnings(notificationCollector.GenerateFormattedMessageErrors()))
				{
					result = IsSupervisorApproved();
					if (!result)
					{
						Globals.Message.ShowError(CustomsNotificationMessages.SupervisorSecurityRightsMessage);
					}
				}

				return result;
			}

			finally
			{
				declaration.IsTopLevel = isTopLevel;
				BusinessEntity.UnRegisterEditableChildObject(declaration);
			}
		}

		protected override bool IsSupportSupervisorApprove(BaseMessageSendingObjectParent businessEntity) => !businessEntity.SecurityCheckpointToSendWithMessageError.IsAllowed;

		public new DialogResult DialogResult
		{
			get => base.DialogResult;
			set
			{
				base.DialogResult = value;
				ClearExportControlNumberIfNeeded();
			}
		}

		void ClearExportControlNumberIfNeeded()
		{
			if (DialogResult == DialogResult.Cancel
				&& BusinessEntity.Context is MessageSendingContext context
				&& context.ProcedureCode == JPProcedureCodeList.Codes.ECR
				&& context.Action == ActionList.Codes.One
				&& BusinessEntity.SendingObjectsCollection.Cast<MessageSendingObject>().Any(x => x.ShouldSend)
				&& Globals.Message.Show(Res.GetString("3163D781-E3E1-4AF4-9C78-5A7F9C89FA4B", "You have canceled the sending of ECR message. Export Control Number will not be deleted from NACCS system. Do you wish to delete it in CargoWise, so that you may manually input another one?"), Res.GetString("0A7053F3-56EA-4201-AA25-BD28A07CE7FC", "Delete Export Control Number"), MessageBoxButtons.OKCancel, DialogResult.OK) == DialogResult.OK
				&& context.EntryHeadersToBeSent.FirstOrDefault()?.EntryInstruction is Business.CusEntryInstruction entryInstruction)
			{
				entryInstruction.ExportControlNumberInfo.ClearValue();
			}
		}

		#region IMessageSendingForm

		void IMessageSendingForm.UpdateDialogResult(DialogResult result)
		{
			DialogResult = result;
		}

		#endregion
	}
}

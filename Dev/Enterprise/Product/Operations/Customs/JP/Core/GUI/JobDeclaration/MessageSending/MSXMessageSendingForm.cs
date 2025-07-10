using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Shared.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public partial class MSXMessageSendingForm : MessageSendingObjectForm, IMessageSendingForm
	{
		public MSXMessageSendingForm()
		{
		}

		public MSXMessageSendingForm(MSXMessageSendingObjectParent sendingObjectParent)
		: base(sendingObjectParent)
		{
			IsReadyForSending = sendingObjectParent.ParentDeclaration?.IsReadyForSending ?? false;
		}

		bool IsReadyForSending { get; }

		public override string FormHeading => IsReadyForSending
			? Res.GetString("061A553F-5323-4CCC-BF91-18A4768BAF60", "Send or Export MSX Messages")
			: Res.GetString("DD9858FF-C033-4AF5-B9DD-42F493317EF3", "Export MSX Messages");

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeNewColumns();
			InitializeComponent();

			DocumentsGroupBox.AllowOverlap(messageSendingObjectsGroupBox);
			DocumentsGrid.MaximumRows = MSXMessageSendingObjectAttachmentCollection.MaxRowCount;
		}

		void InitializeNewColumns()
		{
			var declarationNumberColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			declarationNumberColumnStyleInfo.ColumnName = "Header.EntryNumber";
			declarationNumberColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			MessageSendingObjectsGrid.ColumnStyles.Add(declarationNumberColumnStyleInfo);

			var phaseColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			phaseColumnStyleInfo.ColumnName = "Header.CH_PhaseStatus";
			phaseColumnStyleInfo.IsReadOnly = true;
			phaseColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			phaseColumnStyleInfo.GroupName = Res.GetData("DD7803F6-BFBE-4CB5-A137-1E8335C2E39F", "Phase");
			MessageSendingObjectsGrid.ColumnStyles.Add(phaseColumnStyleInfo);

			var phaseDescriptionColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			phaseDescriptionColumnStyleInfo.ColumnName = "Header.PhaseDescription";
			phaseDescriptionColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			phaseDescriptionColumnStyleInfo.GroupName = Res.GetData("DD7803F6-BFBE-4CB5-A137-1E8335C2E39F", "Phase");
			MessageSendingObjectsGrid.ColumnStyles.Add(phaseDescriptionColumnStyleInfo);

			var registerTypeColumnStyleInfo = new ZCheckBoxColumnStyleInfo();
			registerTypeColumnStyleInfo.ColumnName = "RegistrationType";
			registerTypeColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			MessageSendingObjectsGrid.ColumnStyles.Add(registerTypeColumnStyleInfo);

			var communicationColumnStyleInfo = new ZMultiLineTextBoxColumnInfo();
			communicationColumnStyleInfo.ColumnName = "Communication";
			communicationColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			MessageSendingObjectsGrid.ColumnStyles.Add(communicationColumnStyleInfo);
		}

		public new MSXMessageSendingObjectParent BusinessEntity => (MSXMessageSendingObjectParent)base.BusinessEntity;

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

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			ExportButton.Click -= ExportButton_Click;
			ExportPathButton.Click -= ExportPathButton_Click;

			ExportButton.DataBindings.RemoveBinding(nameof(ZButton.IsEnabledForBinding));
			SendButton.DataBindings.RemoveBinding(nameof(ZButton.IsEnabledForBinding));

			base.SetDataBinding(dataSource, dataMember);

			if (dataSource is MSXMessageSendingObjectParent sendingObjectParent)
			{
				ExportPathButton.Click += ExportPathButton_Click;
				ExportButton.Click += ExportButton_Click;

				ExportButton.DataBindings.Add(new KBinding(nameof(ZButton.IsEnabledForBinding), sendingObjectParent, nameof(sendingObjectParent.AllowExportMessage)));
				SendButton.DataBindings.Add(new KBinding(nameof(ZButton.IsEnabledForBinding), sendingObjectParent, nameof(sendingObjectParent.AllowSendMessage)));
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

		void SendOrExportButtonClickCore()
		{
			this.ShowEditableForm(BusinessEntity);
		}

		#region IMessageSendingForm

		void IMessageSendingForm.UpdateDialogResult(DialogResult result)
		{
			DialogResult = result;
		}

		#endregion
	}
}

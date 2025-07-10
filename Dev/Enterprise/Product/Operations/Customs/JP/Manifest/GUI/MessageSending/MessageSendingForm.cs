using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.Customs.JP.Shared.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	public partial class MessageSendingForm : Customs.GUI.MessageSendingObjectForm, IMessageSendingForm
	{
		public MessageSendingForm()
		{
		}

		public MessageSendingForm(ManifestMessageSendingObjectParent parent)
			: base(parent) { }

		public new ManifestMessageSendingObjectParent BusinessEntity => (ManifestMessageSendingObjectParent)base.BusinessEntity;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitialiseNewColumns();
			InitializeComponent();
			InitaliseControls();
		}

		void InitaliseControls()
		{
			HAWBGroupBox.Visible = BusinessEntity?.IsHCH01 ?? false;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (BusinessEntity.NeedsSorting)
			{
				(MessageSendingObjectsGrid.ListManager.List as IBindingList).RemoveSort();
				(BusinessEntity.SendingObjectsCollection as ManifestMessageSendingObjectCollection).SortByBillStatus();
			}
		}

		void InitialiseNewColumns()
		{
			if (BusinessEntity.Context.ProcedureCode == JPProcedureCodeList.Codes.HDF01 || BusinessEntity.Context.ProcedureCode == JPProcedureCodeList.Codes.NVC01)
			{
				var actionColumnStyleInfo = new ZDropEditColumnStyleInfo();
				actionColumnStyleInfo.ColumnName = nameof(ManifestMessageSendingObject.Action);
				actionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
				actionColumnStyleInfo.GroupName = Res.GetData("95967332-64A5-46DD-B348-81FFE06BB8D3", "Action");
				MessageSendingObjectsGrid.ColumnStyles.Add(actionColumnStyleInfo);

				var actionDescriptionColumnStyleInfo = new ZTextBoxColumnStyleInfo();
				actionDescriptionColumnStyleInfo.ColumnName = nameof(ManifestMessageSendingObject.ActionDescription);
				actionDescriptionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
				actionDescriptionColumnStyleInfo.GroupName = Res.GetData("95967332-64A5-46DD-B348-81FFE06BB8D3", "Action");
				MessageSendingObjectsGrid.ColumnStyles.Add(actionDescriptionColumnStyleInfo);
			}
			else if (BusinessEntity.Context.ProcedureCode == JPProcedureCodeList.Codes.CHA)
			{
				var chaReasonColumnStyleInfo = new ZDropEditColumnStyleInfo();
				chaReasonColumnStyleInfo.ColumnName = nameof(ManifestMessageSendingObject.Reason);
				chaReasonColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
				MessageSendingObjectsGrid.ColumnStyles.Insert(1, chaReasonColumnStyleInfo);
			}

			var messageTypeColumnStyleInfo = new ZDropEditColumnStyleInfo();
			messageTypeColumnStyleInfo.ColumnName = nameof(ManifestMessageSendingObject.MessageType);
			messageTypeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			messageTypeColumnStyleInfo.IsReadOnly = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(messageTypeColumnStyleInfo);

			var billNumberColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			billNumberColumnStyleInfo.ColumnName = "Bill." + AsycudaBill.Schema.ABL_BillNumber;
			billNumberColumnStyleInfo.IsReadOnly = true;
			billNumberColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			MessageSendingObjectsGrid.ColumnStyles.Add(billNumberColumnStyleInfo);

			var messageStatusColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			messageStatusColumnStyleInfo.ColumnName = "Bill." + AsycudaBill.Schema.ABL_MessageStatus;
			messageStatusColumnStyleInfo.IsReadOnly = true;
			messageStatusColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			messageStatusColumnStyleInfo.IsVisible = false;
			MessageSendingObjectsGrid.ColumnStyles.Add(messageStatusColumnStyleInfo);

			var customsStatusColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			customsStatusColumnStyleInfo.ColumnName = "Bill." + AsycudaBill.Schema.ABL_BillStatus;
			customsStatusColumnStyleInfo.IsReadOnly = true;
			customsStatusColumnStyleInfo.CaptionResourceString = Enterprise.Customs.JP.Manifest.GUI.Res.GetData("1C89435F-59CA-4394-A789-BF12171A7077", "Customs Status");
			customsStatusColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			MessageSendingObjectsGrid.ColumnStyles.Add(customsStatusColumnStyleInfo);
		}

		public override string FormHeading => Res.GetString("1782DBD5-B4F7-488E-B321-6E62754FAA99", "Send or Export Messages");

		protected override void SendButton_ClickCore()
		{
			((MessageSendingContext)BusinessEntity.Context).SendTarget = SendTarget.Normal;
			SendOrExportButtonClickCore();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			ExportButton.DataBindings.RemoveBinding(nameof(ZButton.IsEnabledForBinding));
			SendButton.DataBindings.RemoveBinding(nameof(ZButton.IsEnabledForBinding));
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource is ManifestMessageSendingObjectParent manifestSendingObjectParent)
			{
				ExportButton.DataBindings.Add(new KBinding(nameof(ZButton.IsEnabledForBinding), manifestSendingObjectParent, nameof(manifestSendingObjectParent.AllowExportMessage)));
				SendButton.DataBindings.Add(new KBinding(nameof(ZButton.IsEnabledForBinding), manifestSendingObjectParent, nameof(manifestSendingObjectParent.AllowSendMessage)));

				ExportButton.Click -= ExportButton_Click;
				ExportButton.Click += ExportButton_Click;

				ExportPathButton.Click -= ExportPathButton_Click;
				ExportPathButton.Click += ExportPathButton_Click;

				messageSendingObjectsGroupBox.CaptionResourceString = Res.GetData("BE2D9467-D54F-4C0A-B30A-DD123FE803F9", "House bills to be sent");
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

		void SendOrExportButtonClickCore()
		{
			this.ShowEditableForm(BusinessEntity);
		}

		void ExportPathButton_Click(object sender, EventArgs e)
		{
			using (var browser = new ZFolderBrowserDialog())
			{
				browser.ShowNewFolderButton = true;
				browser.RequireMappablePath = true;
				browser.Description = Res.GetString("18366BA9-95F9-4D7B-BE78-882CBD0F2182", "Please select a valid directory path to export the message flat file.");

				if (ZFormModaliser.ShowCommonDialogWithoutDispose(browser) == DialogResult.OK)
				{
					BusinessEntity.ExportPath = browser.IsNeedingToUseEnterpriseChannel ? browser.UnmappedSelectedPath : browser.MappedSelectedPath;
				}
			}
		}

		protected override MessageSendingNotificationCollection RunPreSendValidation()
		{
			var header = BusinessEntity.TopLevelBusinessObject as AsycudaManifestHeader;
			var isTopLevel = header.IsTopLevel;
			var notSelectedSendingObject = BusinessEntity.SendingObjectsCollection.Where(x => !x.ShouldSend);
			var notSelectedBill = notSelectedSendingObject.Select(x => x.Bill);

			try
			{
				header.IsTopLevel = false;
				BusinessEntity.RegisterEditableChildObject(header);
				notSelectedBill.ForEach(x => header.Bills.Remove(x));
				return base.RunPreSendValidation();
			}
			finally
			{
				header.IsTopLevel = isTopLevel;
				BusinessEntity.UnRegisterEditableChildObject(header);
				notSelectedBill.ForEach(x => header.Bills.Add(x));
			}
		}

		protected override bool CheckIsOKToSend()
		{
			var result = false;

			var sendingObjectParent = BusinessEntity;
			if (sendingObjectParent == null || (!sendingObjectParent.AllowEmptyDeclaration && !sendingObjectParent.HasAnyObjectToSend))
			{
				Globals.Message.ShowError(NothingSelectedMessage);
			}
			else
			{
				sendingObjectParent.RegisterEditableChildObject(sendingObjectParent.header);
				sendingObjectParent.Validation.ValidateAll();

				var notificationCollector = new ManifestMessageSendingNotificationCollector(sendingObjectParent, sendingObjectParent.SelectedSendingObjects.Cast<ManifestMessageSendingObject>().Select(x => x.Bill));
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
			}

			return result;
		}

		protected override bool IsSupportSupervisorApprove(BaseMessageSendingObjectParent businessEntity) => !businessEntity.SecurityCheckpointToSendWithMessageError.IsAllowed;

		#region IMessageSendingForm

		void IMessageSendingForm.UpdateDialogResult(DialogResult result)
		{
			DialogResult = result;
		}

		#endregion
	}
}

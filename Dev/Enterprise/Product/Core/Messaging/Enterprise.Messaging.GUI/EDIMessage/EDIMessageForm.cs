using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	public class EDIMessageForm : ZTemplateForm
	{
		public EDIMessageForm(EDIMessage message)
			: base(message)
		{
			InitializeComponent();
			LoadMessageDetailUserControl();
			LoadMessageInterpretationUserControl();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			fMessageUserControl.SetShowInterchangeButtonEnabled();
			UpdateSaveToDiskControls();
			fMessageUserControl.UpdateLinkedEDIMessageControls();
			InterpretationTabPage.TabVisible = Message.ShouldShowInterpretation;
		}

		void UpdateSaveToDiskControls()
		{
			fMessageUserControl.ModifyEditMessageControls(false);

			if (Message == null)
			{
				return;
			}

			if (!IsMessagaModificationAllowed)
			{
				return;
			}

			if (modifyMessageMenuID != 0)
			{
				MainMenu.MenuItems.RemoveAt(modifyMessageMenuID);
				modifyMessageMenuID = 0;
			}

			if (modifyRawMessageActionID != 0)
			{
				MainMenu.MenuItems[2].MenuItems.RemoveAt(modifyRawMessageActionID);
				modifyRawMessageActionID = 0;
			}

			if (modifyMessageActionID != 0)
			{
				MainMenu.MenuItems[2].MenuItems.RemoveAt(modifyMessageActionID);
				modifyMessageActionID = 0;
			}

			if (Message.IsMessageTextTooLong)
			{
				modifyMessageMenuID = MainMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("3EB2AEEE-361E-49fd-8018-CD9C7E75AE7E", "Upload Message From File"), new EventHandler(UploadModification_Click)));
				modifyMessageActionID = MainMenu.MenuItems[2].MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("0DCFFF29-EA4B-4385-9700-A99A1D7D7E46", "Save To Disk"), new EventHandler(fMessageUserControl.zButtonSaveFormatedMessage_Click)));
				modifyRawMessageActionID = MainMenu.MenuItems[2].MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("B271F188-6EEC-4798-B306-33690F7E3399", "Save Raw To Disk"), new EventHandler(fMessageUserControl.zButtonSaveRawMessage_Click)));
				fMessageUserControl.ModifyEditMessageControls(true);
			}
			else
			{
				modifyMessageMenuID = MainMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("3EB2AEEE-361E-49fd-8018-CD9C7E75AE7D", "Modify Message"), new EventHandler(MessageModification_Click)));
			}

			fMessageUserControl.UpdateSaveToDiskAction();
		}

		void MessageModification_Click(object sender, EventArgs e)
		{
			EDIMessageModificationForm form = new EDIMessageModificationForm(new BusinessObjectFactory().Load<EDIMessage>(Message.PK));
			form.FormClosing += new FormClosingEventHandler(form_FormClosing);
			ZFormModaliser.Show(form, this);
		}

		void UploadModification_Click(object sender, EventArgs e)
		{
			if (fileUploadDialog.ShowDialog() == DialogResult.OK)
			{
				Message.EM_MessageText = "Updating...";
				Message.SetEM_MessageTextSource(new LargeFileHolder(fileUploadDialog.ForceLocalFile()));
				Message.Factory.Save();
				Message.Refresh();
				UpdateSaveToDiskControls();
			}
		}

		void form_FormClosing(object sender, FormClosingEventArgs e)
		{
			Message.Refresh();
			UpdateSaveToDiskControls();
			EDIMessageModificationForm form = sender as EDIMessageModificationForm;
			if (form != null)
			{
				form.FormClosing -= new FormClosingEventHandler(form_FormClosing);
			}
		}

		bool IsMessagaModificationAllowed
		{
			get
			{
				bool result = false;
				if (Message != null && Message.EM_Status == EDIMessage.Status.Queued)
				{
					if (Message.IsTransmitMessage)
					{
						GlbGroup group = BusinessEntity.Factory.Load<GlbGroup>(SystemDataRegistry.Instance.MessageModificationBeforeSendingAuthorisationGroup.Value);
						result = GlbStaff.CurrentUser.GS_IsDeveloper || (SystemDataRegistry.Instance.AllowMessageModificationBeforeSending.Value && (group != null && group.Staff.Contains(GlbStaff.CurrentUser.PK)));
					}
					else
					{
						result = GlbStaff.CurrentUser.GS_IsDeveloper;
					}
				}
				return result;
			}
		}

		protected EDIMessage Message
		{
			get { return BusinessEntity as EDIMessage; }
		}

		public override string FormCaption
		{
			get { return Res.GetString("6d1f55e2-671e-4c26-9804-23e221bbe9b5", "Message : {0}", Message.EM_MessageNum); }
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}

		void LoadMessageDetailUserControl()
		{
			if (fMessageUserControl == null)
			{
				fMessageUserControl = GetNewMessageDetailUserControl();
				fMessageUserControl.ediMessage = Message;
				fMessageUserControl.Dock = DockStyle.Fill;
				MainTabPage.Controls.Add(fMessageUserControl);
				fMessageUserControl.Visible = true;
			}
		}

		protected virtual EDIMessageStandAloneUserControl GetNewMessageDetailUserControl()
		{
			return new EDIMessageStandAloneUserControl();
		}

		void LoadMessageInterpretationUserControl()
		{
			messageInterpretationUserControl = new EDIMessageInterpretationUserControl
			{
				Dock = DockStyle.Fill,
				Visible = true,
				CaptionRenderingEnabled = true
			};
			InterpretationTabPage.Controls.Add(messageInterpretationUserControl);
		}

#if DEBUG
		protected EDIMessageStandAloneUserControl MessageControlExposedForTest
		{
			get { return fMessageUserControl; }
		}
#endif

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (fileUploadDialog != null)
				{
					fileUploadDialog.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Component Designer generated code

		new void InitializeComponent()
		{
			this.fileUploadDialog = new Enterprise.ZArchitecture.GUI.ZOpenFileDialog();
			this.InterpretationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 411, true);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.InterpretationTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 438, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.InterpretationTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 411, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(410);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(411);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Messaging.Business.EDIMessage);
			// 
			// fileUploadDialog
			// 
			this.fileUploadDialog.AddExtension = false;
			this.fileUploadDialog.RestoreDirectory = true;
			// 
			// InterpretationTabPage
			// 
			this.InterpretationTabPage.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageForm|3546057c-7b51-4ad6-a3ef-2250da5ae15d", "Interpretation");
			this.InterpretationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InterpretationTabPage.Name = "InterpretationTabPage";
			this.InterpretationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.InterpretationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 411, true);
			this.InterpretationTabPage.TabIndex = 3;
			// 
			// EDIMessageForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 494, true);
			this.DataSourceAssemblyName = "Enterprise.Messaging.Business";
			this.DataSourceType = typeof(Enterprise.Messaging.Business.EDIMessage);
			this.DataSourceTypeName = "Enterprise.Messaging.Business.EDIMessage";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(911, 532, true);
			this.Name = "EDIMessageForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		ZTabPage InterpretationTabPage;

		#endregion

		int modifyMessageMenuID;
		int modifyMessageActionID;
		int modifyRawMessageActionID;
		ZOpenFileDialog fileUploadDialog;
		EDIMessageStandAloneUserControl fMessageUserControl;
		EDIMessageInterpretationUserControl messageInterpretationUserControl;
	}
}

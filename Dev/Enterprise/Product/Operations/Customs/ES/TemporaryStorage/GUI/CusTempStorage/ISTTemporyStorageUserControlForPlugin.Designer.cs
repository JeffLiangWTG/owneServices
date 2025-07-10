using System.Windows.Forms;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	partial class ISTTemporyStorageUserControlForPlugin
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.CusDecTabPageUserControl = GetTemporaryDeclarationUserControl();
			this.MessagesUserControl = GetTemporaryMessageUserControl();
			this.TemporaryStorageHeaderUserControl = GetTemporaryStorageEntrySummaryUserControl();
			this.DeclarationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntrySummaryDeclarationTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DeclarationMessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntrySummaryDeclarationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DeclarationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CoveringLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.DeclarationPanel.SuspendLayout();
			this.CusDecTabPageUserControl.SuspendLayout();
			this.MessagesUserControl.SuspendLayout();
			this.TemporaryStorageHeaderUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.EntrySummaryDeclarationTabPage.SuspendLayout();
			this.EntrySummaryDeclarationTabControl.SuspendLayout();
			this.EntrySummaryDeclarationTabPage.Controls.Add(this.EntrySummaryDeclarationTabControl);
			// 
			// EntrySummaryDeclarationTabControl
			// 
			//
			this.EntrySummaryDeclarationTabControl.TabPages.Add(this.DeclarationTabPage);
			this.EntrySummaryDeclarationTabControl.TabPages.Add(this.DeclarationMessagesTabPage);
			this.EntrySummaryDeclarationTabControl.Dock = DockStyle.Fill;
			this.EntrySummaryDeclarationTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntrySummaryDeclarationTabControl.Name = "EntrySummaryDeclarationTabControl";
			this.EntrySummaryDeclarationTabControl.SelectedIndex = 0;
			this.EntrySummaryDeclarationTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1093, 575, true);
			this.EntrySummaryDeclarationTabControl.TabIndex = 0;
			this.EntrySummaryDeclarationTabControl.SelectedIndexChanged += new System.EventHandler(this.DeclarationTabControl_SelectedIndexChanged);
			// 
			// DeclarationTabPage
			//
			this.DeclarationTabPage.Controls.Add(this.CoveringLabel);
			this.DeclarationTabPage.Controls.Add(this.DeclarationPanel);
			this.DeclarationTabPage.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("499A5D41-8084-4173-861A-9C94C904A994", "Declaration");
			this.DeclarationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DeclarationTabPage.Name = "DeclarationTabPage";
			this.DeclarationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 548, true);
			this.DeclarationTabPage.TabIndex = 0;
			this.DeclarationTabPage.UseVisualStyleBackColor = true;
			// 
			// DeclarationMessagesTabPage
			// 
			this.DeclarationMessagesTabPage.Controls.Add(this.MessagesUserControl);
			this.DeclarationMessagesTabPage.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("4E584A51-29A5-4936-97B0-7A991CB57735", "Messages");
			this.DeclarationMessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DeclarationMessagesTabPage.Name = "DeclarationMessagesTabPage";
			this.DeclarationMessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 548, true);
			this.DeclarationMessagesTabPage.TabIndex = 1;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader);
			//
			//MessagesUserControl
			//
			this.BindingSource.SetBindingMember(this.MessagesUserControl, "CusTempStorageDecs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)));
			this.MessagesUserControl.AllowDrop = true;
			this.MessagesUserControl.Dock = DockStyle.Fill;
			this.MessagesUserControl.Name = "MessagesUserControl";
			//
			//TemporaryStorageHeaderUserControl
			//
			this.TemporaryStorageHeaderUserControl.AllowDrop = true;
			BindingSource.SetBindingMember(this.TemporaryStorageHeaderUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader)(null));
			this.TemporaryStorageHeaderUserControl.Dock = DockStyle.Fill;
			this.TemporaryStorageHeaderUserControl.Name = "TemporaryStorageHeaderUserControl";
			// 
			// CusDecTabPageUserControl
			//
			//BindingSource.SetBindingMember(this.CusDecTabPageUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			//CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader)(null));
			this.CusDecTabPageUserControl.AllowDrop = true;
			this.CusDecTabPageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CusDecTabPageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CusDecTabPageUserControl.Name = "CusDecTabPageUserControl";
			this.CusDecTabPageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1079, 542, true);
			this.CusDecTabPageUserControl.TabIndex = 0;
			// 
			// EntrySummaryDeclarationTabPage
			// 
			//
			this.EntrySummaryDeclarationTabPage.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("C40D1B75-2110-4653-8265-BCC3BC0FE2D6", "IST");
			this.EntrySummaryDeclarationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.EntrySummaryDeclarationTabPage.Name = "EntrySummaryDeclarationTabPage";
			this.EntrySummaryDeclarationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EntrySummaryDeclarationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1093, 575, true);
			this.EntrySummaryDeclarationTabPage.TabIndex = 1;
			this.EntrySummaryDeclarationTabPage.UseVisualStyleBackColor = true;
			this.EntrySummaryDeclarationTabControl.SelectedIndexChanged += new System.EventHandler(this.DeclarationTabControl_SelectedIndexChanged);
			// 
			// MainTabPage
			//
			this.MainTabPage.Controls.Add(TemporaryStorageHeaderUserControl);
			this.MainTabPage.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("6C825BD1-39C3-4E63-80F7-38900BFAC1C5", "Temporary Storage Header");
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1093, 575, true);
			this.MainTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// MainTabControl
			// 
			this.MainTabControl.TabPages.Add(this.MainTabPage);
			this.MainTabControl.TabPages.Add(this.EntrySummaryDeclarationTabPage);
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1101, 602, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.SelectedIndexChanged += new System.EventHandler(this.MainTabControl_SelectedIndexChanged);
			// 
			// CoveringLabel
			// 
			this.CoveringLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CoveringLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.CoveringLabel.IsFontBold = true;
			this.CoveringLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CoveringLabel.Name = "CoveringLabel";
			this.CoveringLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 649, true);
			this.CoveringLabel.TabIndex = 0;
			this.CoveringLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.CoveringLabel.Visible = false;
			// 
			// DeclarationPanel
			// 
			this.DeclarationPanel.Controls.Add(this.CusDecTabPageUserControl);
			this.DeclarationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeclarationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeclarationPanel.Name = "DeclarationPanel";
			this.DeclarationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 649, true);
			this.DeclarationPanel.TabIndex = 0;
			this.DeclarationPanel.Visible = false;
			// 
			// ISTTemporyStorageUserControlForPlugin
			// 
			this.CaptionRenderingEnabled = true;
			this.ShouldSerializeTabPageMethods = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainTabControl);
			this.Name = "ISTTemporyStorageUserControlForPlugin";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 663, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.DeclarationPanel.ResumeLayout(false);
			this.DeclarationPanel.PerformLayout();
			this.CusDecTabPageUserControl.ResumeLayout(true);
			this.CusDecTabPageUserControl.PerformLayout();
			this.TemporaryStorageHeaderUserControl.ResumeLayout(true);
			this.TemporaryStorageHeaderUserControl.PerformLayout();
			this.MessagesUserControl.ResumeLayout(true);
			this.MessagesUserControl.PerformLayout();
			this.EntrySummaryDeclarationTabPage.PerformLayout();
			this.EntrySummaryDeclarationTabControl.ResumeLayout(false);
			this.EntrySummaryDeclarationTabControl.PerformLayout();
			this.EntrySummaryDeclarationTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		protected ZArchitecture.GUI.ZTabPage EntrySummaryDeclarationTabPage;
		private ZArchitecture.GUI.ZTabPage DeclarationTabPage;
		private ZArchitecture.GUI.ZTabPage DeclarationMessagesTabPage;
		private ZArchitecture.GUI.ZTabPage MainTabPage;
		private ZArchitecture.GUI.ZPanel DeclarationPanel;
		private TemporaryStorageHeaderUserControl TemporaryStorageHeaderUserControl;
		private ZArchitecture.GUI.ZTabControl MainTabControl;
		private MessagesUserControl MessagesUserControl;
		private ISTCusTempStorageDecUserControl CusDecTabPageUserControl;
		private ZArchitecture.GUI.ZTabControl EntrySummaryDeclarationTabControl;
		private ZArchitecture.ZLabel CoveringLabel;
	}
}


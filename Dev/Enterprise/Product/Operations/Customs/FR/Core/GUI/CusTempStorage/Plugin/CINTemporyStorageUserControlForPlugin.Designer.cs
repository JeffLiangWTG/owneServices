using System.Windows.Forms;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.FR.GUI.CusTempStorage
{
	partial class CINTemporyStorageUserControlForPlugin
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
			this.DeclarationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			EntrySummaryDeclarationTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DeclarationMessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntrySummaryDeclarationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DeclarationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CoveringLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.DeclarationPanel.SuspendLayout();
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
			this.DeclarationTabPage.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("59DA7CFB-C356-4AA2-8679-24819C5FD8B5", "Declaration");
			this.DeclarationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DeclarationTabPage.Name = "DeclarationTabPage";
			this.DeclarationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 548, true);
			this.DeclarationTabPage.TabIndex = 0;
			this.DeclarationTabPage.UseVisualStyleBackColor = true;
			// 
			// DeclarationMessagesTabPage
			// 
			this.DeclarationMessagesTabPage.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("910AF6A7-8ED5-4B01-B9D4-05F0BDC42BC9", "Messages");
			this.DeclarationMessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DeclarationMessagesTabPage.Name = "DeclarationMessagesTabPage";
			this.DeclarationMessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 548, true);
			this.DeclarationMessagesTabPage.TabIndex = 1;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader);
			// 
			// EntrySummaryDeclarationTabPage
			// 
			//
			this.EntrySummaryDeclarationTabPage.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("E7F6F8A9-A4F0-4278-A044-EE33B8C2DE9F", "CIN/IST");
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
			this.MainTabPage.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("8D69C573-3CB1-4AA6-A021-198C78D63FBA", "Temporary Storage Header");
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
			this.DeclarationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeclarationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeclarationPanel.Name = "DeclarationPanel";
			this.DeclarationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 649, true);
			this.DeclarationPanel.TabIndex = 0;
			this.DeclarationPanel.Visible = false;
			// 
			// CINTemporyStorageUserControlForPlugin
			// 
			this.CaptionRenderingEnabled = true;
			this.ShouldSerializeTabPageMethods = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MainTabControl);
			this.Name = "CINTemporyStorageUserControlForPlugin";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 663, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.DeclarationPanel.ResumeLayout(false);
			this.DeclarationPanel.PerformLayout();
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
		private CusTempStorageDecUserControl CusDecTabPageUserControl;
		private ZArchitecture.GUI.ZTabControl EntrySummaryDeclarationTabControl;
		private ZArchitecture.ZLabel CoveringLabel;
	}
}


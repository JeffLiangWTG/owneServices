using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class DepartureMovementsTabUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.Phase5DepartureMovementsTabGridUserControl = new Enterprise.Customs.EU.NCTS.GUI.Phase5DepartureMovementsTabGridUserControl();
			this.TabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DeclarationDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DeclarationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DynamicDeclarationDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.ModeOfTransportPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TransportBorderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransportBorderDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.TransportDepartureGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransportDepartureDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.TransportAndPackingPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TransportDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransportAndPackagingDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.CustomFieldTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ProcessTemplateCustomFieldsControl = new Enterprise.Customs.GUI.CustomFieldsWrapperControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.Phase5DepartureMovementsTabGridUserControl.SuspendLayout();
			this.TabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.DeclarationDetailsPanel.SuspendLayout();
			this.DeclarationDetailsGroupBox.SuspendLayout();
			this.ModeOfTransportPanel.SuspendLayout();
			this.TransportBorderGroupBox.SuspendLayout();
			this.TransportDepartureGroupBox.SuspendLayout();
			this.TransportAndPackingPanel.SuspendLayout();
			this.TransportDetailsGroupBox.SuspendLayout();
			this.CustomFieldTabPage.SuspendLayout();
			this.ProcessTemplateCustomFieldsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.INctsDepartureMovementHeaderCollection<Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader>);
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.Phase5DepartureMovementsTabGridUserControl);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1344, 490, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(75);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.TabControl);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(310);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(159);
			this.SplitContainer.SplitterWidth = 5;
			this.SplitContainer.TabIndex = 0;
			// 
			// Phase5DepartureMovementsTabGridUserControl
			// 
			this.Phase5DepartureMovementsTabGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Phase5DepartureMovementsTabGridUserControl, ".");
			this.Phase5DepartureMovementsTabGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Phase5DepartureMovementsTabGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Phase5DepartureMovementsTabGridUserControl.Name = "Phase5DepartureMovementsTabGridUserControl";
			this.Phase5DepartureMovementsTabGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1344, 159, true);
			this.Phase5DepartureMovementsTabGridUserControl.TabIndex = 0;
			// 
			// TabControl
			// 
			this.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControl.Controls.Add(this.DetailsTabPage);
			this.TabControl.Controls.Add(this.CustomFieldTabPage);
			this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1344, 326, true);
			this.TabControl.TabIndex = 0;
			// 
			// NCDDepartureMovementCustomTabPage
			// 
			this.CustomFieldTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("4FD33F59-8FA3-433B-969C-BFB841861C9D", "Custom Fields");
			this.CustomFieldTabPage.Controls.Add(this.ProcessTemplateCustomFieldsControl);
			this.CustomFieldTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomFieldTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldTabPage.UseVisualStyleBackColor = true;
			this.CustomFieldTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1201, 364, true);
			this.CustomFieldTabPage.TabIndex = 3;
			this.CustomFieldTabPage.Text = "Custom Fields";
			// 
			// NCDDepartureMovementFieldsControl
			// 
			this.ProcessTemplateCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProcessTemplateCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProcessTemplateCustomFieldsControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ProcessTemplateCustomFieldsControl.Name = "NCDProcessTemplateCustomFieldsControl";
			this.ProcessTemplateCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1201, 364, true);
			this.ProcessTemplateCustomFieldsControl.TabIndex = 1;
			this.ProcessTemplateCustomFieldsControl.NothingSetupMessageLabelText = Res.GetString("4BC51B6F-F95E-4EAD-9093-486FB017E623", "To make use of this tab, please setup NCTS Departure custom fields in Workflow Manager.");
			//
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("5b146b72-bece-4c2b-8f18-9cf752e11515", "Movement Details");
			this.DetailsTabPage.Controls.Add(this.TransportAndPackingPanel);
			this.DetailsTabPage.Controls.Add(this.ModeOfTransportPanel);
			this.DetailsTabPage.Controls.Add(this.DeclarationDetailsPanel);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1336, 299, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// DeclarationDetailsPanel
			// 
			this.DeclarationDetailsPanel.Controls.Add(this.DeclarationDetailsGroupBox);
			this.DeclarationDetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.DeclarationDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeclarationDetailsPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 145, true);
			this.DeclarationDetailsPanel.Name = "DeclarationDetailsPanel";
			this.DeclarationDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 145, true);
			this.DeclarationDetailsPanel.TabIndex = 0;
			// 
			// DeclarationDetailsGroupBox
			// 
			this.DeclarationDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("31C09794-9F01-429B-9A32-503CF7848EC7", "Declaration Details");
			this.DeclarationDetailsGroupBox.Controls.Add(this.DynamicDeclarationDetailsPanel);
			this.DeclarationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeclarationDetailsGroupBox.Name = "DeclarationDetailsGroupBox";
			this.DeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 141, true);
			this.DeclarationDetailsGroupBox.TabIndex = 0;
			this.DeclarationDetailsGroupBox.TabStop = false;
			// 
			// DynamicDeclarationDetailsPanel
			// 
			this.DynamicDeclarationDetailsPanel.AllowDrop = true;
			this.DynamicDeclarationDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicDeclarationDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicDeclarationDetailsPanel.Name = "DynamicDeclarationDetailsPanel";
			this.DynamicDeclarationDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 122, true);
			this.DynamicDeclarationDetailsPanel.TabIndex = 0;
			// 
			// ModeOfTransportPanel
			// 
			this.ModeOfTransportPanel.Controls.Add(this.TransportBorderGroupBox);
			this.ModeOfTransportPanel.Controls.Add(this.TransportDepartureGroupBox);
			this.ModeOfTransportPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ModeOfTransportPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ModeOfTransportPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1062, 140, true);
			this.ModeOfTransportPanel.Name = "ModeOfTransportPanel";
			this.ModeOfTransportPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1160, 166, true);
			this.ModeOfTransportPanel.TabIndex = 0;
			// 
			// TransportBorderGroupBox
			// 
			this.TransportBorderGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("252cad25-dd84-410d-af0e-5e7aa7a81c3d", "Transport Border");
			this.TransportBorderGroupBox.Controls.Add(this.TransportBorderDynamicLayoutPanel);
			this.TransportBorderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(474, 0, true);
			this.TransportBorderGroupBox.Name = "TransportBorderGroupBox";
			this.TransportBorderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(468, 163, true);
			this.TransportBorderGroupBox.TabIndex = 1;
			this.TransportBorderGroupBox.TabStop = false;
			// 
			// TransportBorderDynamicLayoutPanel
			// 
			this.TransportBorderDynamicLayoutPanel.AllowDrop = true;
			this.TransportBorderDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportBorderDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TransportBorderDynamicLayoutPanel.Name = "TransportBorderDynamicLayoutPanel";
			this.TransportBorderDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 144, true);
			this.TransportBorderDynamicLayoutPanel.TabIndex = 0;
			// 
			// TransportDepartureGroupBox
			// 
			this.TransportDepartureGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("BDF84996-47EC-4139-9EFA-BE3512EAA8A4", "Transport Departure");
			this.TransportDepartureGroupBox.Controls.Add(this.TransportDepartureDynamicLayoutPanel);
			this.TransportDepartureGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportDepartureGroupBox.Name = "TransportDepartureGroupBox";
			this.TransportDepartureGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(468, 163, true);
			this.TransportDepartureGroupBox.TabIndex = 0;
			this.TransportDepartureGroupBox.TabStop = false;
			// 
			// TransportDepartureDynamicLayoutPanel
			// 
			this.TransportDepartureDynamicLayoutPanel.AllowDrop = true;
			this.TransportDepartureDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportDepartureDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TransportDepartureDynamicLayoutPanel.Name = "TransportDepartureDynamicLayoutPanel";
			this.TransportDepartureDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 144, true);
			this.TransportDepartureDynamicLayoutPanel.TabIndex = 0;
			// 
			// TransportAndPackingPanel
			// 
			this.TransportAndPackingPanel.Controls.Add(this.TransportDetailsGroupBox);
			this.TransportAndPackingPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TransportAndPackingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 166, true);
			this.TransportAndPackingPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1160, 100, true);
			this.TransportAndPackingPanel.Name = "TransportAndPackingPanel";
			this.TransportAndPackingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1160, 154, true);
			this.TransportAndPackingPanel.TabIndex = 1;
			// 
			// TransportDetailsGroupBox
			// 
			this.TransportDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("0677CA8C-8A28-41AE-B9C1-D15BCCFF0984", "Transport Details");
			this.TransportDetailsGroupBox.Controls.Add(this.TransportAndPackagingDynamicLayoutPanel);
			this.TransportDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportDetailsGroupBox.Name = "TransportDetailsGroupBox";
			this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1160, 154, true);
			this.TransportDetailsGroupBox.TabIndex = 0;
			this.TransportDetailsGroupBox.TabStop = false;
			// 
			// TransportAndPackagingDynamicLayoutPanel
			// 
			this.TransportAndPackagingDynamicLayoutPanel.AllowDrop = true;
			this.TransportAndPackagingDynamicLayoutPanel.AutoScroll = true;
			this.TransportAndPackagingDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportAndPackagingDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TransportAndPackagingDynamicLayoutPanel.Name = "TransportAndPackagingDynamicLayoutPanel";
			this.TransportAndPackagingDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1154, 135, true);
			this.TransportAndPackagingDynamicLayoutPanel.TabIndex = 0;
			// 
			// DepartureMovementsTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "DepartureMovementsTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1344, 490, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.Phase5DepartureMovementsTabGridUserControl.ResumeLayout(true);
			this.Phase5DepartureMovementsTabGridUserControl.PerformLayout();
			this.TabControl.ResumeLayout(false);
			this.TabControl.PerformLayout();
			this.DeclarationDetailsPanel.ResumeLayout(false);
			this.DeclarationDetailsPanel.PerformLayout();
			this.DeclarationDetailsGroupBox.ResumeLayout(false);
			this.DeclarationDetailsGroupBox.PerformLayout();
			this.ModeOfTransportPanel.ResumeLayout(false);
			this.ModeOfTransportPanel.PerformLayout();
			this.TransportBorderGroupBox.ResumeLayout(false);
			this.TransportBorderGroupBox.PerformLayout();
			this.TransportDepartureGroupBox.ResumeLayout(false);
			this.TransportDepartureGroupBox.PerformLayout();
			this.TransportAndPackingPanel.ResumeLayout(false);
			this.TransportAndPackingPanel.PerformLayout();
			this.TransportDetailsGroupBox.ResumeLayout(false);
			this.TransportDetailsGroupBox.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.CustomFieldTabPage.ResumeLayout(false);
			this.CustomFieldTabPage.PerformLayout();
			this.ProcessTemplateCustomFieldsControl.ResumeLayout(false);
			this.ProcessTemplateCustomFieldsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer SplitContainer;
		internal ZArchitecture.GUI.ZTabControl TabControl;
		internal ZArchitecture.GUI.ZTabPage DetailsTabPage;
		internal Enterprise.ZArchitecture.GUI.ZPanel DeclarationDetailsPanel;
		internal ZArchitecture.GUI.DynamicLayoutPanel DynamicDeclarationDetailsPanel;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox DeclarationDetailsGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZPanel ModeOfTransportPanel;
		internal ZArchitecture.GUI.ZGroupBox TransportDepartureGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel TransportDepartureDynamicLayoutPanel;
		internal ZArchitecture.GUI.ZGroupBox TransportBorderGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel TransportBorderDynamicLayoutPanel;
		internal Enterprise.ZArchitecture.GUI.ZPanel TransportAndPackingPanel;
		internal ZArchitecture.GUI.ZGroupBox TransportDetailsGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel TransportAndPackagingDynamicLayoutPanel;
		internal Phase5DepartureMovementsTabGridUserControl Phase5DepartureMovementsTabGridUserControl;
		internal ZArchitecture.GUI.ZTabPage CustomFieldTabPage;
		internal Customs.GUI.CustomFieldsWrapperControl ProcessTemplateCustomFieldsControl;
		private System.ComponentModel.IContainer components;
	}
}


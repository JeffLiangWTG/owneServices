using System;
using System.Windows.Forms;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	partial class NetworkDiagramForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.NetworkDiagramControl = new Enterprise.BufferManagement.NetworkVisualisation.GUI.NetworkDiagramUserControl();
			this.Hotkeys.RegisterHotKey(Keys.Control | Keys.F, new Action(SwitchFocusToFinder), ResString.GetMultilingualString("CC149C86-DD42-4838-A50D-900774BE0E91", "Search"));
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.RelatedDiagramsPageTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DiagramsGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RelatedDiagramsPageTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DiagramsGrid)).BeginInit();
			this.DiagramsGrid.SuspendLayout();
			this.SuspendLayout();
			//
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.RelatedDiagramsPageTab);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 628, true);
			//
			// MainTabPage
			//
			this.MainTabPage.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("978db04e-b737-462f-a888-4865f8056ed0", "Network");
			this.MainTabPage.Controls.Add(this.NetworkDiagramControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1196, 601, true);
			//
			// LogsTabPage
			//
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1196, 601, true);
			//
			// MainPanel
			//
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 628, true);
			//
			// MainStatusBar
			//
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 24, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape);
			//
			// NetworkDiagramControl
			//
			this.NetworkDiagramControl.AllowDrop = true;
			this.NetworkDiagramControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NetworkDiagramControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NetworkDiagramControl.Name = "NetworkDiagramControl";
			this.NetworkDiagramControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1196, 601, true);
			this.NetworkDiagramControl.TabIndex = 0;
			//
			// RelatedDiagramsPageTab
			//
			this.RelatedDiagramsPageTab.Controls.Add(this.DiagramsGrid);
			this.RelatedDiagramsPageTab.Controls.Add(this.zLabel1);
			this.RelatedDiagramsPageTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.RelatedDiagramsPageTab.Name = "RelatedDiagramsPageTab";
			this.RelatedDiagramsPageTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RelatedDiagramsPageTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1199, 565, true);
			this.RelatedDiagramsPageTab.TabIndex = 3;
			this.RelatedDiagramsPageTab.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("9A13F378-3123-408B-93C1-BCCB46276E3D", "Related Diagrams");
			this.RelatedDiagramsPageTab.UseVisualStyleBackColor = true;
			//
			// DiagramsGrid
			//
			this.DiagramsGrid.AllowNavigation = false;
			this.DiagramsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DiagramsGrid, "RelatedDiagrams");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(null)).RelatedDiagrams)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.RelatedDiagramView)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(null)).RelatedDiagrams)).SyncRoot)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.RelatedDiagramView)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(null)).RelatedDiagrams)).SyncRoot)).RelationshipType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.NetworkVisualisation.Business.RelatedDiagramView)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(null)).RelatedDiagrams)).SyncRoot)).IsScaled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.NetworkVisualisation.Business.RelatedDiagramView)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(null)).RelatedDiagrams)).SyncRoot)).IsApproved)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.NetworkVisualisation.Business.RelatedDiagramView)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(null)).RelatedDiagrams)).SyncRoot)).ScheduledStartTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.NetworkVisualisation.Business.RelatedDiagramView)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(null)).RelatedDiagrams)).SyncRoot)).ScheduledFinishTimeLocal)));
			this.DiagramsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("feded501-3d6e-45bc-9ee6-9f44aae69093", "Name");
			zTextBoxColumnStyleInfo1.ColumnName = "Name";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(360);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("074c2ff9-8486-4ffd-bf21-a517e90ea7e8", "Relationship Type");
			zTextBoxColumnStyleInfo2.ColumnName = "RelationshipType";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("93a3f872-5598-4a43-9b81-f53fbadcf35a", "Is Scaled");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsScaled";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("ecfd018b-efd1-49e5-a369-82251f3c34b6", "Is Approved");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsApproved";
			zCheckBoxColumnStyleInfo2.IsMandatory = true;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("f325a90d-a947-4f1a-8b67-eaac1557bda8", "Scheduled Start");
			zDateEditColumnStyleInfo1.ColumnName = "ScheduledStartTimeLocal";
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("8249f8cf-02a4-4be9-a733-9383c76bbf1b", "Scheduled Finish");
			zDateEditColumnStyleInfo2.ColumnName = "ScheduledFinishTimeLocal";
			zDateEditColumnStyleInfo2.IsMandatory = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.DiagramsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DiagramsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DiagramsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DiagramsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.DiagramsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.DiagramsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.DiagramsGrid.GridId = "50144d86-f99b-4527-8460-c0519fa42e50";
			this.DiagramsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DiagramsGrid.IsWholeRowSelectedOnClick = true;
			this.DiagramsGrid.LayoutKey = "zGrid1";
			this.DiagramsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 29, true);
			this.DiagramsGrid.Name = "DiagramsGrid";
			this.DiagramsGrid.ReadOnly = true;
			this.DiagramsGrid.ShouldSetErrorsOnTabPage = false;
			this.DiagramsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1912, 503, true);
			this.DiagramsGrid.TabIndex = 3;
			this.DiagramsGrid.Dock = DockStyle.Fill;
			this.DiagramsGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.DiagramsGrid_MouseDoubleClick);
			//
			// zLabel1
			//
			this.zLabel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1193, 27, true);
			this.zLabel1.TabIndex = 2;
			this.zLabel1.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("1C22C74D-6FD5-4B6E-9DB7-0EBED91FD993", "Below are listed the Network Diagrams which are related to this diagram. Double click a row in this grid to open the diagram.");
			//
			// NetworkDiagramForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 684, true);
			this.DataSourceType = typeof(Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape);
#if !WINZOR
			// Blazor server doesn't calculate the scaled size precisely, limiting the form size when it is maximized.
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(CCPMConstants.MaxDiagramFormSizeWidth, CCPMConstants.MaxDiagramFormSizeHeight, true);
#endif
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 600, true);
			this.Name = "NetworkDiagramForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RelatedDiagramsPageTab.ResumeLayout(false);
			this.RelatedDiagramsPageTab.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DiagramsGrid)).EndInit();
			this.DiagramsGrid.ResumeLayout(false);
			this.DiagramsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		public NetworkDiagramUserControl NetworkDiagramControl;
		public ZArchitecture.GUI.ZTabPage RelatedDiagramsPageTab;
		private ZArchitecture.ZLabel zLabel1;
		public ZArchitecture.GUI.ZDisplayGrid DiagramsGrid;
	}
}

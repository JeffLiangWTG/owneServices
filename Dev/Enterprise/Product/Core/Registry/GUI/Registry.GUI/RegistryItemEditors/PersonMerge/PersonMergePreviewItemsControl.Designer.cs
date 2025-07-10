using CargoWise.Windows.UI;

namespace Enterprise.Registry.GUI
{
	partial class PersonMergePreviewItemsControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.PersonMergePreviewItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MoveDownButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MoveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.MiddleFlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PersonMergePreviewItemsGrid)).BeginInit();
			this.PersonMergePreviewItemsGrid.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.MiddleFlowLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.PersonMergePreviewItemCollection);
			// 
			// PersonMergePreviewItemsGrid
			// 
			this.PersonMergePreviewItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PersonMergePreviewItemsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.PersonMergePreviewItem)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.PersonMergePreviewItem)(null)).FriendlyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.PersonMergePreviewItem)(null)).Visibility)));
			this.PersonMergePreviewItemsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("396d4197-88cc-4433-98e9-9bc8fc2afa84", "Friendly Name");
			zTextBoxColumnStyleInfo1.ColumnName = "FriendlyName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("dd114761-9ebb-4526-b1f8-08995f424060", "Visibility");
			zCheckBoxColumnStyleInfo1.ColumnName = "Visibility";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PersonMergePreviewItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PersonMergePreviewItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PersonMergePreviewItemsGrid.CopySelectedRowsAllowed = false;
			this.PersonMergePreviewItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PersonMergePreviewItemsGrid.GridId = "de55eeeb-a8bf-4de5-8a2f-3de7de245a1f";
			this.PersonMergePreviewItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PersonMergePreviewItemsGrid.LayoutKey = "RatesPriorityGrid";
			this.PersonMergePreviewItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PersonMergePreviewItemsGrid.Name = "PersonMergePreviewItemsGrid";
			this.PersonMergePreviewItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 417, true);
			this.PersonMergePreviewItemsGrid.TabIndex = 3;
			// 
			// MoveDownButton
			// 
			this.MoveDownButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("568b23d1-1275-4872-979c-b9019a13ac6e", "Move Down");
			this.MoveDownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 32, true);
			this.MoveDownButton.Name = "MoveDownButton";
			this.MoveDownButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.MoveDownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.MoveDownButton.TabIndex = 5;
			this.MoveDownButton.ToolTipCaption = null;
			this.MoveDownButton.Click += new System.EventHandler(this.DownButton_Click);
			// 
			// MoveUpButton
			// 
			this.MoveUpButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("20b02c97-b7da-46e0-80fd-e740de8fa7b9", "Move Up");
			this.MoveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MoveUpButton.Name = "MoveUpButton";
			this.MoveUpButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.MoveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.MoveUpButton.TabIndex = 4;
			this.MoveUpButton.ToolTipCaption = null;
			this.MoveUpButton.Click += new System.EventHandler(this.UpButton_Click);
			// 
			// MainPanel
			// 
			this.MainPanel.ColumnCount = 3;
			this.MainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F));
			this.MainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)));
			this.MainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
			this.MainPanel.Controls.Add(this.MiddleFlowLayoutPanel, 1, 0);
			this.MainPanel.Controls.Add(this.PersonMergePreviewItemsGrid, 0, 0);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.RowCount = 1;
			this.MainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 423, true);
			this.MainPanel.TabIndex = 6;
			// 
			// MiddleFlowLayoutPanel
			// 
			this.MiddleFlowLayoutPanel.Controls.Add(this.MoveUpButton);
			this.MiddleFlowLayoutPanel.Controls.Add(this.MoveDownButton);
			this.MiddleFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MiddleFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.MiddleFlowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(386, 3, true);
			this.MiddleFlowLayoutPanel.Name = "MiddleFlowLayoutPanel";
			this.MiddleFlowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 100, true);
			this.MiddleFlowLayoutPanel.TabIndex = 0;
			// 
			// PersonMergePreviewItemsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainPanel);
			this.Name = "PersonMergePreviewItemsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 423, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PersonMergePreviewItemsGrid)).EndInit();
			this.PersonMergePreviewItemsGrid.ResumeLayout(false);
			this.PersonMergePreviewItemsGrid.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.MiddleFlowLayoutPanel.ResumeLayout(false);
			this.MiddleFlowLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid PersonMergePreviewItemsGrid;
		internal ZArchitecture.GUI.ZButton MoveDownButton;
		internal ZArchitecture.GUI.ZButton MoveUpButton;
		private KTableLayoutPanel MainPanel;
		private KFlowLayoutPanel MiddleFlowLayoutPanel;
	}
}

namespace Enterprise.VisualBoards.GUI
{
	partial class BoardFiltersForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.HintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FiltersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FiltersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ClearAllFiltersButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FiltersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FiltersGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 256, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.VisualBoards.Business.BoardFilterBusinessObjectCollection);
			// 
			// HintLabel
			// 
			this.HintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.HintLabel.AutoSize = true;
			this.HintLabel.CaptionResourceString = Enterprise.VisualBoards.GUI.Res.GetData("d3baf6bd-23fe-41fd-8018-67e6f9bcad1a", "Below are listed the filters that are currently applied to the Visual Board. Individual filters can be removed by deleting a row in the grid.");
			this.HintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 8, true);
			this.HintLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 0, true);
			this.HintLabel.Name = "HintLabel";
			this.HintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 26, true);
			this.HintLabel.TabIndex = 1;
			// 
			// FiltersGroupBox
			// 
			this.FiltersGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FiltersGroupBox.CaptionResourceString = Enterprise.VisualBoards.GUI.Res.GetData("289679a1-49a8-4616-b929-ef468521525e", "Filters");
			this.FiltersGroupBox.Controls.Add(this.FiltersGrid);
			this.FiltersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 37, true);
			this.FiltersGroupBox.Name = "FiltersGroupBox";
			this.FiltersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 219, true);
			this.FiltersGroupBox.TabIndex = 2;
			this.FiltersGroupBox.TabStop = false;
			// 
			// FiltersGrid
			// 
			this.FiltersGrid.AllowNavigation = false;
			this.FiltersGrid.AllowReadOnlyRowsToBeDeleted = true;
			this.BindingSource.SetBindingMember(this.FiltersGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.VisualBoards.Business.BoardFilterBusinessObject)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.VisualBoards.Business.BoardFilterBusinessObject)(null)).FilterName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.VisualBoards.Business.BoardFilterBusinessObject)(null)).ParentName)));
			this.FiltersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "FilterName";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
			zTextBoxColumnStyleInfo4.ColumnName = "ParentName";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.FiltersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FiltersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FiltersGrid.CopySelectedRowsAllowed = true;
			this.FiltersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FiltersGrid.GridId = "03ac6084-82e1-4c5d-ac44-7e63abcefaa1";
			this.FiltersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FiltersGrid.LayoutKey = "FiltersGrid";
			this.FiltersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.FiltersGrid.Name = "FiltersGrid";
			this.FiltersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 200, true);
			this.FiltersGrid.TabIndex = 0;
			// 
			// ClearAllFiltersButton
			// 
			this.ClearAllFiltersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ClearAllFiltersButton.CaptionResourceString = Enterprise.VisualBoards.GUI.Res.GetData("f18c9f05-a2c0-4af1-a42b-ed0e772b7a1e", "Clear All");
			this.ClearAllFiltersButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 6, true);
			this.ClearAllFiltersButton.Name = "ClearAllFiltersButton";
			this.ClearAllFiltersButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 25, true);
			this.ClearAllFiltersButton.TabIndex = 3;
			this.ClearAllFiltersButton.UseVisualStyleBackColor = true;
			this.ClearAllFiltersButton.Click += new System.EventHandler(this.ClearAllFiltersButton_Click);
			// 
			// BoardFiltersForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 280, true);
			this.Controls.Add(this.ClearAllFiltersButton);
			this.Controls.Add(this.FiltersGroupBox);
			this.Controls.Add(this.HintLabel);
			this.DataSourceType = typeof(Enterprise.VisualBoards.Business.BoardFilterBusinessObjectCollection);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 300, true);
			this.Name = "BoardFiltersForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.HintLabel, 0);
			this.Controls.SetChildIndex(this.FiltersGroupBox, 0);
			this.Controls.SetChildIndex(this.ClearAllFiltersButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FiltersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.FiltersGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel HintLabel;
		private ZArchitecture.GUI.ZGroupBox FiltersGroupBox;
		public ZArchitecture.ZGrid FiltersGrid;
		private ZArchitecture.GUI.ZButton ClearAllFiltersButton;
	}
}

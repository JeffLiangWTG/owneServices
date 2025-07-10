namespace Enterprise.BufferManagement.GUI
{
	partial class AdditionalComponentsTabPageControl
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
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.AdditionalComponentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalComponentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AdditionalComponentsHintLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalComponentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalComponentsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMBoardSection);
			// 
			// AdditionalComponentsGroupBox
			// 
			this.AdditionalComponentsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("b5a38c26-a914-4823-9f77-2dbec30e4c31", "Additional Components");
			this.AdditionalComponentsGroupBox.Controls.Add(this.AdditionalComponentsGrid);
			this.AdditionalComponentsGroupBox.Controls.Add(this.AdditionalComponentsHintLabel);
			this.AdditionalComponentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalComponentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalComponentsGroupBox.Name = "AdditionalComponentsGroupBox";
			this.AdditionalComponentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 466, true);
			this.AdditionalComponentsGroupBox.TabIndex = 1;
			this.AdditionalComponentsGroupBox.TabStop = false;
			// 
			// AdditionalComponentsGrid
			// 
			this.AdditionalComponentsGrid.AllowNavigation = false;
			this.AdditionalComponentsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AdditionalComponentsGrid, "AdditionalComponents");
			this.AdditionalComponentsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo2.ColumnName = "BSA_FC_Component";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.AdditionalComponentsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.AdditionalComponentsGrid.CopySelectedRowsAllowed = true;
			this.AdditionalComponentsGrid.GridId = "ed2ad4e4-70e0-46bd-953a-62532ceb783a";
			this.AdditionalComponentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalComponentsGrid.LayoutKey = "AdditionalComponentsGrid";
			this.AdditionalComponentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 49, true);
			this.AdditionalComponentsGrid.Name = "AdditionalComponentsGrid";
			this.AdditionalComponentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 411, true);
			this.AdditionalComponentsGrid.TabIndex = 1;
			// 
			// AdditionalComponentsHintLabel
			// 
			this.AdditionalComponentsHintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.AdditionalComponentsHintLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("ae2faa6b-30cd-4319-be42-f692287f938a", "Specify other components to be displayed on the same board section. Work from all components will be displayed in the relevant channels.");
			this.AdditionalComponentsHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.AdditionalComponentsHintLabel.Name = "AdditionalComponentsHintLabel";
			this.AdditionalComponentsHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 30, true);
			this.AdditionalComponentsHintLabel.TabIndex = 0;
			// 
			// AdditionalComponentsTabPageControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalComponentsGroupBox);
			this.Name = "AdditionalComponentsTabPageControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 466, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalComponentsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AdditionalComponentsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox AdditionalComponentsGroupBox;
		private ZArchitecture.ZGrid AdditionalComponentsGrid;
		private ZArchitecture.ZLabel AdditionalComponentsHintLabel;
	}
}

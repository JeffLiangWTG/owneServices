namespace Enterprise.BufferManagement.GUI
{
	partial class CustomisedLayoutLinksControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CustomisationLinksGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CustomisationLinksGrid)).BeginInit();
			this.CustomisationLinksGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMControlCustomisationLinkCollection);
			// 
			// CustomisationLinksGrid
			// 
			this.CustomisationLinksGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CustomisationLinksGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMControlCustomisationLink)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMControlCustomisationLink)(null)).FML_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.BMControlCustomisationLink)(null)).FML_FM_ControlCustomisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMControlCustomisationLink)(null)).CustomisedLayout.TypeDescription)));
			this.CustomisationLinksGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "FML_JobType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "FML_FM_ControlCustomisation";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo1.ColumnName = "CustomisedLayout+TypeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.CustomisationLinksGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CustomisationLinksGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CustomisationLinksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CustomisationLinksGrid.CopySelectedRowsAllowed = true;
			this.CustomisationLinksGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomisationLinksGrid.GridId = "06da217d-c6c5-4ea8-b212-21d6b1235fe0";
			this.CustomisationLinksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CustomisationLinksGrid.LayoutKey = "CustomisationLinksGrid";
			this.CustomisationLinksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomisationLinksGrid.Name = "CustomisationLinksGrid";
			this.CustomisationLinksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(601, 212, true);
			this.CustomisationLinksGrid.TabIndex = 0;
			// 
			// CustomisedLayoutLinksControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomisationLinksGrid);
			this.Name = "CustomisedLayoutLinksControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(601, 212, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CustomisationLinksGrid)).EndInit();
			this.CustomisationLinksGrid.ResumeLayout(false);
			this.CustomisationLinksGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid CustomisationLinksGrid;
	}
}

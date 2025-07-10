namespace Enterprise.Customs.BR.GUI
{
	partial class NveUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.NveGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NveGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NveGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NveGrid)).BeginInit();
			this.NveGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.NveCusCodeDataCollection);
			// 
			// NveGroupBox
			// 
			this.NveGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("C1B203A4-580E-4347-A05D-E55FD620D110", "NVE");
			this.NveGroupBox.Controls.Add(this.NveGrid);
			this.NveGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NveGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NveGroupBox.Name = "NveGroupBox";
			this.NveGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 325, true);
			this.NveGroupBox.TabIndex = 0;
			this.NveGroupBox.TabStop = false;
			// 
			// NveGrid
			// 
			this.NveGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.NveGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.NveCusCodeData)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.NveCusCodeData)(null)).Position)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.NveCusCodeData)(null)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.NveCusCodeData)(null)).Attribute)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.NveCusCodeData)(null)).Specification)));
			this.NveGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Position";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zTextBoxColumnStyleInfo2.ColumnName = "CY_Code";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zTextBoxColumnStyleInfo3.ColumnName = "Attribute";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.ColumnName = "Specification";
			zDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			this.NveGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.NveGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.NveGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.NveGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.NveGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NveGrid.GridId = "ede5fc68-a2b3-4e8d-889d-2308a5f5b64b";
			this.NveGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NveGrid.LayoutKey = "NveGrid";
			this.NveGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.NveGrid.Name = "NveGrid";
			this.NveGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 306, true);
			this.NveGrid.TabIndex = 1;
			// 
			// NveLayout
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NveGroupBox);
			this.Name = "NveLayout";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 325, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NveGroupBox.ResumeLayout(false);
			this.NveGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NveGrid)).EndInit();
			this.NveGrid.ResumeLayout(false);
			this.NveGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox NveGroupBox;
		internal ZArchitecture.ZGrid NveGrid;
	}
}

namespace Enterprise.Customs.BR.GUI
{
	partial class SiscomexUsageFeeUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.SiscomexUsageFeeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SiscomexUsageFeeGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SiscomexUsageFeeGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SiscomexUsageFeeGrid)).BeginInit();
			this.SiscomexUsageFeeGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.SiscomexUsageFeeCollection);
			// 
			// SiscomexUsageFeeGroupBox
			//
			this.SiscomexUsageFeeGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("B35FA3B7-8017-4AC2-9433-DDD494D34010", "SISCOMEX Usage Fee");
			this.SiscomexUsageFeeGroupBox.Controls.Add(this.SiscomexUsageFeeGrid);
			this.SiscomexUsageFeeGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SiscomexUsageFeeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SiscomexUsageFeeGroupBox.Name = "SiscomexUsageFeeGroupBox";
			this.SiscomexUsageFeeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 211, true);
			this.SiscomexUsageFeeGroupBox.TabIndex = 0;
			this.SiscomexUsageFeeGroupBox.TabStop = false;
			// 
			// SiscomexUsageFeeGrid
			// 
			this.SiscomexUsageFeeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SiscomexUsageFeeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.CusEntryLineFee)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusEntryLineFee)(null)).FormalEntryLineNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.CusEntryLineFee)(null)).CF_ChargeAmount)));
			this.SiscomexUsageFeeGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "FormalEntryLineNumbers";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CF_ChargeAmount";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.SiscomexUsageFeeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SiscomexUsageFeeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SiscomexUsageFeeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SiscomexUsageFeeGrid.GridId = "50c9531d-5816-4cdd-a78e-e7cf4a9b2a47";
			this.SiscomexUsageFeeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SiscomexUsageFeeGrid.LayoutKey = "SiscomexUsageFeeGrid";
			this.SiscomexUsageFeeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SiscomexUsageFeeGrid.Name = "SiscomexUsageFeeGrid";
			this.SiscomexUsageFeeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 192, true);
			this.SiscomexUsageFeeGrid.TabIndex = 0;
			// 
			// SiscomexUsageFeeUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SiscomexUsageFeeGroupBox);
			this.Name = "SiscomexUsageFeeUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 211, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SiscomexUsageFeeGroupBox.ResumeLayout(false);
			this.SiscomexUsageFeeGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SiscomexUsageFeeGrid)).EndInit();
			this.SiscomexUsageFeeGrid.ResumeLayout(false);
			this.SiscomexUsageFeeGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZGroupBox SiscomexUsageFeeGroupBox;
		protected ZArchitecture.ZGrid SiscomexUsageFeeGrid;
	}
}

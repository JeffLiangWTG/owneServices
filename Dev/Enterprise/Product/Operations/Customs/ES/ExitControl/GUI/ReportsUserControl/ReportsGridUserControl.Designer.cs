namespace Enterprise.Customs.ES.ExitControl.GUI
{
	partial class ReportsGridUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ReportsGrid)).BeginInit();
			this.ReportsGrid.SuspendLayout();
			this.ReportsGridGroupBox.SuspendLayout();
			this.BottomGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ExitControlBase.Business.ICusExitReportCollection<Enterprise.Customs.ES.ExitControl.Business.CusExitReport>);
			// 
			// ReportsGrid
			//
			this.BindingSource.SetBindingMember(this.ReportsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ES.ExitControl.Business.CusExitReport)(null)).ArrivalDate)));
			this.ReportsGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo1.ColumnName = "ArrivalDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145);
			this.ReportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ReportsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportsGrid.GridId = "CA503E0E-F9B3-43FE-8BE2-8C9B7BEC85C5";
			this.ReportsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReportsGrid.LayoutKey = "ReportsGrid";
			this.ReportsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ReportsGrid.Name = "ReportsGrid";
			this.ReportsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1394, 88, true);
			this.ReportsGrid.TabIndex = 0;
			// 
			// ReportsGridGroupBox
			// 
			this.ReportsGridGroupBox.CaptionResourceString = Enterprise.Customs.ES.ExitControl.GUI.Res.GetData("76794B78-4874-4832-B7A6-7F928C58B3C3", "Exit Reports");
			this.ReportsGridGroupBox.Controls.Add(this.ReportsGrid);
			this.ReportsGridGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportsGridGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportsGridGroupBox.Name = "ReportsGridGroupBox";
			this.ReportsGridGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1400, 185, true);
			this.ReportsGridGroupBox.TabIndex = 0;
			this.ReportsGridGroupBox.TabStop = false;
			// 
			// BottomGroupBox
			// 
			this.BottomGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 104, true);
			this.BottomGroupBox.Name = "BottomGroupBox";
			this.BottomGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1394, 150, true);
			this.BottomGroupBox.TabIndex = 1;
			// 
			// ReportsGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "ReportsGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1400, 107, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ReportsGrid)).EndInit();
			this.ReportsGrid.ResumeLayout(false);
			this.ReportsGrid.PerformLayout();
			this.ReportsGridGroupBox.ResumeLayout(false);
			this.ReportsGridGroupBox.PerformLayout();
			this.BottomGroupBox.ResumeLayout(false);
			this.BottomGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}


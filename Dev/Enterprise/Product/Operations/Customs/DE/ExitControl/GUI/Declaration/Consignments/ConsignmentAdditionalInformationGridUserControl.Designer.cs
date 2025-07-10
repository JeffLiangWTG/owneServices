namespace Enterprise.Customs.DE.ExitControl.GUI
{
	partial class ConsignmentAdditionalInformationGridUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.AdditionalInformationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInformationGrid)).BeginInit();
			this.AdditionalInformationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.ExitControl.Business.ExitControlAdditionalInfoCollection);
			// 
			// AdditionalInformationGrid
			// 
			this.AdditionalInformationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalInformationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.ExitControl.Business.ExitControlAdditionalInfo)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.ExitControlAdditionalInfo)(null)).CSI_Code)));
			this.AdditionalInformationGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Code";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AdditionalInformationGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AdditionalInformationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInformationGrid.GridId = "B28630A4-E6C7-4123-A66E-5A0302298184";
			this.AdditionalInformationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalInformationGrid.LayoutKey = "ConsignmentsGrid";
			this.AdditionalInformationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInformationGrid.Name = "AdditionalInformationGrid";
			this.AdditionalInformationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 86, true);
			this.AdditionalInformationGrid.TabIndex = 0;
			// 
			// ConsignmentAdditionalInformationGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalInformationGrid);
			this.Name = "ConsignmentAdditionalInformationGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 86, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInformationGrid)).EndInit();
			this.AdditionalInformationGrid.ResumeLayout(false);
			this.AdditionalInformationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid AdditionalInformationGrid;
	}
}


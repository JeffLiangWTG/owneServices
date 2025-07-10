namespace Enterprise.Accounting.Registry.GUI
{
	public partial class JobProfitLossReasonCodeControl
	{
		#region Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.JobProfitLossReasonCodeGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.JobProfitLossReasonCodeGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.JobProfitLossReasonCodeCollection);
			// 
			// JobProfitLossReasonCodeGrid
			// 
			this.JobProfitLossReasonCodeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.JobProfitLossReasonCodeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.JobProfitLossReasonCode)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.JobProfitLossReasonCode)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.JobProfitLossReasonCode)(null)).EnglishDescription)));
			this.JobProfitLossReasonCodeGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossReasonCodeControl|59959434-7b68-4c05-8e9d-08457b986f5e", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossReasonCodeControl|40fcccf4-575a-438e-ba44-68f340b3ee9e", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.JobProfitLossReasonCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.JobProfitLossReasonCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.JobProfitLossReasonCodeGrid.GridId = "3529525c-126e-4cdc-a814-4a83d61b0d0f";
			this.JobProfitLossReasonCodeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobProfitLossReasonCodeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobProfitLossReasonCodeGrid.LayoutKey = "RevenueRecognitionGrid";
			this.JobProfitLossReasonCodeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobProfitLossReasonCodeGrid.Name = "JobProfitLossReasonCodeGrid";
			this.JobProfitLossReasonCodeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 224, true);
			this.JobProfitLossReasonCodeGrid.TabIndex = 0;
			// 
			// JobProfitLossReasonCodeControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.JobProfitLossReasonCodeGrid);
			this.Name = "JobProfitLossReasonCodeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 224, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.JobProfitLossReasonCodeGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private ZArchitecture.ZGrid JobProfitLossReasonCodeGrid;
	}
}

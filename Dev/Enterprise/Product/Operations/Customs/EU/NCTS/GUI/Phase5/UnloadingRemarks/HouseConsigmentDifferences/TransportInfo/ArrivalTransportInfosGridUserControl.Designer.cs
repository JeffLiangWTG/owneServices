namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class ArrivalTransportInfosGridUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.TransportInfoGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TransportInfoGrid)).BeginInit();
			this.TransportInfoGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.IArrivalCusTransportMeansCollection<Enterprise.Customs.EU.NCTS.Business.ArrivalCusTransportMeans>);
			// 
			// TransportInfoGrid
			// 
			this.TransportInfoGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TransportInfoGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.ArrivalCusTransportMeans)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.ArrivalCusTransportMeans)(null)).TPM_SequenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.ArrivalCusTransportMeans)(null)).TPM_TransportState)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.ArrivalCusTransportMeans)(null)).TPM_TypeOfIdentification)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.ArrivalCusTransportMeans)(null)).TPM_IdentificationNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.ArrivalCusTransportMeans)(null)).TPM_RN_NKTransportNationality)));
			this.TransportInfoGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TPM_SequenceNumber";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "TPM_TransportState";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "TPM_TypeOfIdentification";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "TPM_IdentificationNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "TPM_RN_NKTransportNationality";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TransportInfoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TransportInfoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TransportInfoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TransportInfoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TransportInfoGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.TransportInfoGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportInfoGrid.GridId = "c2a39916-1950-42f9-857f-a9a5eae22be8";
			this.TransportInfoGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TransportInfoGrid.LayoutKey = "TransportInfoGrid";
			this.TransportInfoGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportInfoGrid.Name = "TransportInfoGrid";
			this.TransportInfoGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 721, true);
			this.TransportInfoGrid.TabIndex = 0;
			// 
			// ArrivalTransportInfosGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TransportInfoGrid);
			this.Name = "ArrivalTransportInfosGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 721, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TransportInfoGrid)).EndInit();
			this.TransportInfoGrid.ResumeLayout(false);
			this.TransportInfoGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid TransportInfoGrid;
	}
}

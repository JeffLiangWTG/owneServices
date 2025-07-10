namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5ArrivalContainersEquipmentGridUserControl
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.ContainersEquipmentTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ContainersEquipmentTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContainersEquipmentGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ContainersEquipmentTabControl.SuspendLayout();
			this.ContainersEquipmentTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersEquipmentGrid)).BeginInit();
			this.ContainersEquipmentGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsArrivalHeaderContainerCollection);
			// 
			// ContainersEquipmentTabControl
			// 
			this.ContainersEquipmentTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ContainersEquipmentTabControl.Controls.Add(this.ContainersEquipmentTabPage);
			this.ContainersEquipmentTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersEquipmentTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainersEquipmentTabControl.Name = "ContainersEquipmentTabControl";
			this.ContainersEquipmentTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 91, true);
			this.ContainersEquipmentTabControl.TabIndex = 0;
			// 
			// ContainersEquipmentTabPage
			// 
			this.ContainersEquipmentTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("97566ad6-a587-49d3-9577-973c00a8ed0e", "Containers/Equipment");
			this.ContainersEquipmentTabPage.Controls.Add(this.ContainersEquipmentGrid);
			this.ContainersEquipmentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 34, true);
			this.ContainersEquipmentTabPage.Name = "ContainersEquipmentTabPage";
			this.ContainersEquipmentTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ContainersEquipmentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 53, true);
			this.ContainersEquipmentTabPage.TabIndex = 0;
			// 
			// ContainersEquipmentGrid
			// 
			this.ContainersEquipmentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainersEquipmentGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalHeaderContainer)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalHeaderContainer)(null)).BC_SequenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalHeaderContainer)(null)).BC_UnloadedState)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalHeaderContainer)(null)).BC_Mode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalHeaderContainer)(null)).BC_ContainerNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalHeaderContainer)(null)).TotalSealCount)));
			this.ContainersEquipmentGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "BC_SequenceNumber";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "BC_UnloadedState";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "BC_Mode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "BC_ContainerNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "TotalSealCount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ContainersEquipmentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ContainersEquipmentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ContainersEquipmentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ContainersEquipmentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContainersEquipmentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ContainersEquipmentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersEquipmentGrid.GridId = "6d5f2476-7269-4aad-89a3-4ce373ba4d32";
			this.ContainersEquipmentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersEquipmentGrid.LayoutKey = "ContainersEquipmentGrid";
			this.ContainersEquipmentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ContainersEquipmentGrid.Name = "ContainersEquipmentGrid";
			this.ContainersEquipmentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 47, true);
			this.ContainersEquipmentGrid.TabIndex = 0;
			// 
			// Phase5ArrivalContainersEquipmentGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ContainersEquipmentTabControl);
			this.Name = "Phase5ArrivalContainersEquipmentGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 91, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ContainersEquipmentTabControl.ResumeLayout(false);
			this.ContainersEquipmentTabControl.PerformLayout();
			this.ContainersEquipmentTabPage.ResumeLayout(false);
			this.ContainersEquipmentTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersEquipmentGrid)).EndInit();
			this.ContainersEquipmentGrid.ResumeLayout(false);
			this.ContainersEquipmentGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZTabControl ContainersEquipmentTabControl;
		internal ZArchitecture.GUI.ZTabPage ContainersEquipmentTabPage;
		internal ZArchitecture.ZGrid ContainersEquipmentGrid;
	}
}

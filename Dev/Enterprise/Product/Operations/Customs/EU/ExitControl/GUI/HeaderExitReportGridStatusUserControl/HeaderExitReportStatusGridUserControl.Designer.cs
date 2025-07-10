namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed partial class HeaderExitReportStatusGridUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.ExitReportStatusGrid = new Enterprise.ZArchitecture.ZGrid();
			this.HeaderExitReportStatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ExitReportStatusGrid)).BeginInit();
			this.ExitReportStatusGrid.SuspendLayout();
			this.HeaderExitReportStatusGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.ExitControl.Business.CusExitHeader);
			// 
			// ExitReportStatusGrid
			// 
			this.ExitReportStatusGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ExitReportStatusGrid, "CusExitReportStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitHeader)(null)).CusExitReportStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitHeader)(null)).CusExitReportStatus)).SyncRoot)).CER_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitHeader)(null)).CusExitReportStatus)).SyncRoot)).TypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitHeader)(null)).CusExitReportStatus)).SyncRoot)).CER_CXC_Consignment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitHeader)(null)).CusExitReportStatus)).SyncRoot)).Header.CusExitConsignments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitHeader)(null)).CusExitReportStatus)).SyncRoot)).CER_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitHeader)(null)).CusExitReportStatus)).SyncRoot)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitHeader)(null)).CusExitReportStatus)).SyncRoot)).CER_MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitHeader)(null)).CusExitReportStatus)).SyncRoot)).MessageStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitHeader)(null)).CusExitReportStatus)).SyncRoot)).CER_Calc_Discrepancies)));
			this.ExitReportStatusGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CER_Type";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "TypeDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zGuidDropEditColumnStyleInfo1.BindToList = "Header.CusExitConsignments";
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "CER_CXC_Consignment";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "CER_Status";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "CER_MessageStatus";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "MessageStatusDescription";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCheckBoxColumnStyleInfo1.ColumnName = "CER_Calc_Discrepancies";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(83);
			this.ExitReportStatusGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ExitReportStatusGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ExitReportStatusGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.ExitReportStatusGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ExitReportStatusGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ExitReportStatusGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ExitReportStatusGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ExitReportStatusGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ExitReportStatusGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExitReportStatusGrid.GridId = "078dec2a-ca47-4dcd-a1b7-9c40e0a3cbbb";
			this.ExitReportStatusGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ExitReportStatusGrid.LayoutKey = "ExitReportStatusGrid";
			this.ExitReportStatusGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ExitReportStatusGrid.Name = "ExitReportStatusGrid";
			this.ExitReportStatusGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 504, true);
			this.ExitReportStatusGrid.TabIndex = 0;
			// 
			// HeaderExitReportStatusGroupBox
			// 
			this.HeaderExitReportStatusGroupBox.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("5575940F-1A00-47C0-BC1E-1246D8B32FEA", "Exit Report Status");
			this.HeaderExitReportStatusGroupBox.Controls.Add(this.ExitReportStatusGrid);
			this.HeaderExitReportStatusGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderExitReportStatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderExitReportStatusGroupBox.Name = "HeaderExitReportStatusGroupBox";
			this.HeaderExitReportStatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(713, 523, true);
			this.HeaderExitReportStatusGroupBox.TabIndex = 1;
			this.HeaderExitReportStatusGroupBox.TabStop = false;
			// 
			// HeaderExitReportStatusGridUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HeaderExitReportStatusGroupBox);
			this.Name = "HeaderExitReportStatusGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(713, 523, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ExitReportStatusGrid)).EndInit();
			this.ExitReportStatusGrid.ResumeLayout(false);
			this.ExitReportStatusGrid.PerformLayout();
			this.HeaderExitReportStatusGroupBox.ResumeLayout(false);
			this.HeaderExitReportStatusGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.ZGrid ExitReportStatusGrid;
		internal ZArchitecture.GUI.ZGroupBox HeaderExitReportStatusGroupBox;
	}
}

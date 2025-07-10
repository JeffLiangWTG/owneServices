using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.EU.ExitControl.GUI
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ReportsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ReportsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ReportsGrid)).BeginInit();
			this.ReportsGrid.SuspendLayout();
			this.ReportsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ExitControlBase.Business.ICusExitReportCollection<Enterprise.Customs.EU.ExitControl.Business.CusExitReport>);
			// 
			// ReportsGrid
			// 
			this.ReportsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReportsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_CXC_Consignment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).Header.CusExitConsignments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_Calc_Discrepancies)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_TransportType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_TransportID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_RN_NKTransportNationality)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_Location)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_DateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_OfficeOfExit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).MessageStatusDescription)));
			this.ReportsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CER_Type";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(83);
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "CER_CXC_Consignment";
			zGuidDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(168);
			zCheckBoxColumnStyleInfo1.ColumnName = "CER_Calc_Discrepancies";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(83);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CER_TransportMode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "CER_TransportType";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CER_TransportID";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "CER_RN_NKTransportNationality";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(108);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CER_Location";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateTimeOffsetEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "CER_DateTime";
			zDateTimeOffsetEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(136);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CER_OfficeOfExit";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "CER_Status";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "CER_MessageStatus";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(103);
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "MessageStatusDescription";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.ReportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ReportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ReportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ReportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ReportsGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ReportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ReportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ReportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ReportsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportsGrid.GridId = "15AB703C-5D8F-4008-9DC3-AE27E0EF44F9";
			this.ReportsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReportsGrid.LayoutKey = "ReportsGrid";
			this.ReportsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ReportsGrid.Name = "ReportsGrid";
			this.ReportsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1394, 88, true);
			this.ReportsGrid.TabIndex = 0;
			// 
			// ReportsGroupBox
			// 
			this.ReportsGroupBox.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("C7B7FE53-EDAD-4AA3-A10F-BBCE7C678976", "Exit Reports");
			this.ReportsGroupBox.Controls.Add(this.ReportsGrid);
			this.ReportsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportsGroupBox.Name = "ReportsGroupBox";
			this.ReportsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1400, 107, true);
			this.ReportsGroupBox.TabIndex = 0;
			this.ReportsGroupBox.TabStop = false;
			// 
			// ReportsGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReportsGroupBox);
			this.Name = "ReportsGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1400, 107, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ReportsGrid)).EndInit();
			this.ReportsGrid.ResumeLayout(false);
			this.ReportsGrid.PerformLayout();
			this.ReportsGroupBox.ResumeLayout(false);
			this.ReportsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid ReportsGrid;
		internal ZArchitecture.GUI.ZGroupBox ReportsGroupBox;
	}
}


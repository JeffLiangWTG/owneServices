namespace Enterprise.Accounting.Module
{
	public partial class CashAdvanceFilterControl
	{
		#region Component Designer generated code

		internal CargoWise.Windows.UI.KSplitter GridSplitter;
		internal Enterprise.ZArchitecture.ZGrid CashAdvanceRequestLineDisplayGrid;

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CashAdvanceRequestLineDisplayGrid = new Enterprise.ZArchitecture.ZGrid();
			this.GridSplitter = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CashAdvanceRequestLineDisplayGrid)).BeginInit();
			this.CashAdvanceRequestLineDisplayGrid.SuspendLayout();
			this.SuspendLayout();
			//
			// grid
			//
			this.grid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.grid, "RequestCashAdvanceCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).RequestCashAdvanceCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).RequestCashAdvanceCollection)).SyncRoot)).OrganizationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).RequestCashAdvanceCollection)).SyncRoot)).CAH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).RequestCashAdvanceCollection)).SyncRoot)).CAH_OSAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).RequestCashAdvanceCollection)).SyncRoot)).CAH_OSPaidAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).RequestCashAdvanceCollection)).SyncRoot)).CAH_OSOutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).RequestCashAdvanceCollection)).SyncRoot)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).RequestCashAdvanceCollection)).SyncRoot)).CAH_Printed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).RequestCashAdvanceCollection)).SyncRoot)).OrganizationName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).RequestCashAdvanceCollection)).SyncRoot)).CAH_LocalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).RequestCashAdvanceCollection)).SyncRoot)).CAH_LocalPaidAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).RequestCashAdvanceCollection)).SyncRoot)).CAH_LocalOutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).RequestCashAdvanceCollection)).SyncRoot)).CAH_RequestReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).RequestCashAdvanceCollection)).SyncRoot)).JobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).RequestCashAdvanceCollection)).SyncRoot)).JobDepartment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).RequestCashAdvanceCollection)).SyncRoot)).JobBranch)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("4c668840-6ff0-44f3-bd42-6fcfbe7fb278", "Organization");
			zTextBoxColumnStyleInfo1.ColumnName = "OrganizationCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("8a8c8b62-0eb8-4846-acd0-879274682795", "Currency");
			zTextBoxColumnStyleInfo2.ColumnName = "CAH_RX_NKTransactionCurrency";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("4ebceccb-5742-4256-96ac-9ecbc4b60cc9", "OS Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "CAH_OSAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("3ef4ca29-b695-4fda-b6db-e3d38cddd078", "OS Paid Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "CAH_OSPaidAmount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("b6861e11-041c-4b72-b8f8-c79621d15506", "OS Outstanding Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "CAH_OSOutstandingAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("68fc2854-e854-4509-97cb-90af13ae0f1a", "Status");
			zTextBoxColumnStyleInfo3.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("535f4a83-f88a-4f7e-a7d3-05e87c5ee1ce", "Printed");
			zCheckBoxColumnStyleInfo1.ColumnName = nameof(Enterprise.MasterFiles.Business.AccCashAdvanceRequestHeader.CAH_Printed);
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("ab72abf0-1c44-4ef8-ab4d-7aea4887aba9", "Organization Name");
			zTextBoxColumnStyleInfo4.ColumnName = "OrganizationName";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("7d428c6e-2dbd-462a-9653-1fcd798c5d90", "Local Amount");
			zCalcEditColumnStyleInfo4.ColumnName = "CAH_LocalAmount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("f6947a5d-f929-4efd-800f-1116ace575ae", "Local Paid Amount");
			zCalcEditColumnStyleInfo5.ColumnName = "CAH_LocalPaidAmount";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("b14c0031-8b3a-4e79-8726-023654361d04", "Local Outstanding Amount");
			zCalcEditColumnStyleInfo6.ColumnName = "CAH_LocalOutstandingAmount";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("67af78ae-568b-4116-9ad5-902625b38d21", "Advance Payment Number");
			zTextBoxColumnStyleInfo5.ColumnName = "CAH_RequestReferenceNumber";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("6602d41a-039e-4bf5-b8be-915a6bad5e1f", "Job Number");
			zTextBoxColumnStyleInfo6.ColumnName = "JobNumber";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("FD7D749B-C64F-412A-8891-EC27BABDCE6A", "Job Department");
			zTextBoxColumnStyleInfo7.ColumnName = "JobDepartment";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("17E1D060-400C-43B2-89B6-952DAEE6CD8E", "Job Branch");
			zTextBoxColumnStyleInfo8.ColumnName = "JobBranch";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 54, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1090, 474, true);
			this.grid.TabIndex = 0;
			this.grid.AfterBind += new System.EventHandler(this.FilteredGrid_AfterBind);
			this.grid.CurrentCellChanged += new System.EventHandler(this.FilteredGrid_CurrentCellChanged);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject);
			//
			// CashAdvanceRequestLineDisplayGrid
			//
			this.CashAdvanceRequestLineDisplayGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CashAdvanceRequestLineDisplayGrid, "CashAdvanceLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).CashAdvanceLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).CashAdvanceLines)).SyncRoot)).RelatedChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).CashAdvanceLines)).SyncRoot)).OrganizationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).CashAdvanceLines)).SyncRoot)).Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).CashAdvanceLines)).SyncRoot)).CAL_OSAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).CashAdvanceLines)).SyncRoot)).CAL_OSPaidAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).CashAdvanceLines)).SyncRoot)).CAL_OSOutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).CashAdvanceLines)).SyncRoot)).CAL_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).CashAdvanceLines)).SyncRoot)).CAL_LocalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).CashAdvanceLines)).SyncRoot)).CAL_LocalPaidAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.CashAdvanceFilterBusinessObject)(null)).CashAdvanceLines)).SyncRoot)).OrganizationName)));
			this.CashAdvanceRequestLineDisplayGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("986815e0-eb17-42d9-8271-8d46138fb559", "Charge Code");
			zTextBoxColumnStyleInfo9.ColumnName = "RelatedChargeCode";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("179ad61c-0d04-467b-9bd0-31cd01160009", "Organization");
			zTextBoxColumnStyleInfo10.ColumnName = "OrganizationCode";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("d015cfb0-a81a-47c2-ac4d-6cda702d02da", "Currency");
			zTextBoxColumnStyleInfo11.ColumnName = "Currency";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("b340bdd7-3e33-491f-ac85-25550f196459", "OS Amount");
			zCalcEditColumnStyleInfo7.ColumnName = "CAL_OSAmount";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("651e06ad-661c-4f5b-82a9-ded50af5771b", "OS Paid Amount");
			zCalcEditColumnStyleInfo8.ColumnName = "CAL_OSPaidAmount";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("caa41ba6-7a1c-4eea-8356-6c14f854f9f5", "OS Outstanding Amount");
			zCalcEditColumnStyleInfo9.ColumnName = "CAL_OSOutstandingAmount";
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("312db327-b72a-4a4c-9922-3ace12711c97", "Status");
			zTextBoxColumnStyleInfo12.ColumnName = "CAL_Status";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("3f271021-88e8-4d01-b8aa-8aaba4609afa", "Local Amount");
			zCalcEditColumnStyleInfo10.ColumnName = "CAL_LocalAmount";
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("8f479d45-742c-4035-bf32-c23e93acdbd4", "Local Paid Amount");
			zCalcEditColumnStyleInfo11.ColumnName = "CAL_LocalPaidAmount";
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("b6bd5771-c58a-468c-acfa-5efc3b4e7fa6", "Organization Name");
			zTextBoxColumnStyleInfo13.ColumnName = "OrganizationName";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CashAdvanceRequestLineDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.CashAdvanceRequestLineDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.CashAdvanceRequestLineDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.CashAdvanceRequestLineDisplayGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.CashAdvanceRequestLineDisplayGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.CashAdvanceRequestLineDisplayGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.CashAdvanceRequestLineDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.CashAdvanceRequestLineDisplayGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.CashAdvanceRequestLineDisplayGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.CashAdvanceRequestLineDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.CashAdvanceRequestLineDisplayGrid.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.CashAdvanceRequestLineDisplayGrid.GridId = "3F392B6D-5D75-474B-A643-B5EE368C091C";
			this.CashAdvanceRequestLineDisplayGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CashAdvanceRequestLineDisplayGrid.LayoutKey = "CashAdvanceRequestLineDisplayGrid";
			this.CashAdvanceRequestLineDisplayGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 333, true);
			this.CashAdvanceRequestLineDisplayGrid.Name = "CashAdvanceRequestLineDisplayGrid";
			this.CashAdvanceRequestLineDisplayGrid.ReadOnly = true;
			this.CashAdvanceRequestLineDisplayGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1090, 195, true);
			this.CashAdvanceRequestLineDisplayGrid.TabIndex = 17;
			//
			// GridSplitter
			//
			this.GridSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.GridSplitter.DoNotSaveSplitterLayout = false;
			this.GridSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 326, true);
			this.GridSplitter.Name = "GridSplitter";
			this.GridSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1090, 7, true);
			this.GridSplitter.TabIndex = 16;
			this.GridSplitter.TabStop = false;
			this.GridSplitter.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.GridSplitter_SplitterMoved);
			//
			// CashAdvanceFilterControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GridSplitter);
			this.Controls.Add(this.CashAdvanceRequestLineDisplayGrid);
			this.Name = "CashAdvanceFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1090, 528, true);
			this.Controls.SetChildIndex(this.ToolStripPermissionsLabel, 0);
			this.Controls.SetChildIndex(this.FilterStripsPanel, 0);
			this.Controls.SetChildIndex(this.AddStripButton, 0);
			this.Controls.SetChildIndex(this.ToolStripRecordsFoundLabel, 0);
			this.Controls.SetChildIndex(this.grid, 0);
			this.Controls.SetChildIndex(this.CashAdvanceRequestLineDisplayGrid, 0);
			this.Controls.SetChildIndex(this.GridSplitter, 0);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CashAdvanceRequestLineDisplayGrid)).EndInit();
			this.CashAdvanceRequestLineDisplayGrid.ResumeLayout(false);
			this.CashAdvanceRequestLineDisplayGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion
	}
}

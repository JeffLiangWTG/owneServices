using System.ComponentModel;
using Enterprise.Accounting.Business.GeneralLedger.GLBudget;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class GLBudgetForm
	{


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new Container();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			this.AU_OpeningBoundCalcEdit = new ZCalcEdit();
			this.AU_AGBoundFindBox = new ZGuidFindBox();
			this.AU_GBBoundFindBox = new ZGuidFindBox();
			this.AU_GEBoundFindBox = new ZGuidFindBox();
			this.AU_AllocationTypeBoundDropDownEdit = new ZDropEdit();
			this.AU_AllocationValueBoundCalcEdit = new ZCalcEdit();
			this.AU_AllocationIncrementBoundCalcEdit = new ZCalcEdit();
			this.AU_Calc_TotalAmountBoundReadOnlyCalcEdit = new ZCalcEdit();
			this.GuidPanel = new ZPanel();
			this.AllocationPanel = new ZPanel();
			this.AccGLBudgetLinesBoundGrid = new ZGrid();
			this.AU_YearBoundYearEdit = new ZYearEdit();
			this.zTabControl1 = new ZTemplateTabControl();
			this.Budget = new ZTabPage();
			this.TotalPercentageCalcEdit = new ZCalcEdit();
			this.zStmNoteTabPage1 = new ZStmNoteTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			this.BottomPanel = new ZPanel();
			this.PostingButtonsControl = new Core.Forms.ZPostingButtonsUserControl();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GuidPanel.SuspendLayout();
			this.AllocationPanel.SuspendLayout();
			((ISupportInitialize)(this.AccGLBudgetLinesBoundGrid)).BeginInit();
			this.zTabControl1.SuspendLayout();
			this.Budget.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 577, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 26, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(416);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(416);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(GLBudget);
			// 
			// AU_OpeningBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AU_OpeningBoundCalcEdit, "AU_Opening");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((GLBudget)(null)).AU_Opening)));
			this.AU_OpeningBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(521, 9, true);
			this.AU_OpeningBoundCalcEdit.Name = "AU_OpeningBoundCalcEdit";
			this.AU_OpeningBoundCalcEdit.ShowGroupSeparators = false;
			this.AU_OpeningBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.AU_OpeningBoundCalcEdit.TabIndex = 4;
			this.AU_OpeningBoundCalcEdit.Text = "0.00";
			this.AU_OpeningBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AU_AGBoundFindBox
			// 
			this.BindingSource.SetBindingMember(this.AU_AGBoundFindBox, "AU_AG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((GLBudget)(null)).AU_AG)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GLBudget)(null)).Lookups.GLHeaders)));
			this.AU_AGBoundFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLBudgetForm|088e3325-2cca-4c21-be23-694ead67c9d9", "Account Number");
			this.AU_AGBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 8, true);
			this.AU_AGBoundFindBox.Name = "AU_AGBoundFindBox";
			this.AU_AGBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 21, true);
			this.AU_AGBoundFindBox.TabIndex = 1;
			// 
			// AU_GBBoundFindBox
			// 
			this.BindingSource.SetBindingMember(this.AU_GBBoundFindBox, "AU_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((GLBudget)(null)).AU_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GLBudget)(null)).Lookups.Branches)));
			this.AU_GBBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 32, true);
			this.AU_GBBoundFindBox.Name = "AU_GBBoundFindBox";
			this.AU_GBBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 21, true);
			this.AU_GBBoundFindBox.TabIndex = 3;
			// 
			// AU_GEBoundFindBox
			// 
			this.BindingSource.SetBindingMember(this.AU_GEBoundFindBox, "AU_GE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((GLBudget)(null)).AU_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GLBudget)(null)).Lookups.Departments)));
			this.AU_GEBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 56, true);
			this.AU_GEBoundFindBox.Name = "AU_GEBoundFindBox";
			this.AU_GEBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 21, true);
			this.AU_GEBoundFindBox.TabIndex = 5;
			// 
			// AU_AllocationTypeBoundDropDownEdit
			// 
			this.BindingSource.SetBindingMember(this.AU_AllocationTypeBoundDropDownEdit, "AU_AllocationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((GLBudget)(null)).AU_AllocationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GLBudget)(null)).AllocationTypes)));
			this.AU_AllocationTypeBoundDropDownEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLBudgetForm|bbc40101-d265-4319-bc8c-1e979f17c2a2", "Allocate");
			this.AU_AllocationTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 8, true);
			this.AU_AllocationTypeBoundDropDownEdit.Name = "AU_AllocationTypeBoundDropDownEdit";
			this.AU_AllocationTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 20, true);
			this.AU_AllocationTypeBoundDropDownEdit.TabIndex = 1;
			// 
			// AU_AllocationValueBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AU_AllocationValueBoundCalcEdit, "AU_AllocationValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((GLBudget)(null)).AU_AllocationValue)));
			this.AU_AllocationValueBoundCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLBudgetForm|725be8fe-2fb7-4b3c-860f-84bd5535001f", "Amount");
			this.AU_AllocationValueBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 32, true);
			this.AU_AllocationValueBoundCalcEdit.Name = "AU_AllocationValueBoundCalcEdit";
			this.AU_AllocationValueBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 20, true);
			this.AU_AllocationValueBoundCalcEdit.TabIndex = 3;
			this.AU_AllocationValueBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AU_AllocationIncrementBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AU_AllocationIncrementBoundCalcEdit, "AU_AllocationIncrement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((GLBudget)(null)).AU_AllocationIncrement)));
			this.AU_AllocationIncrementBoundCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLBudgetForm|17fa949f-6eab-4ecc-a1ee-963b01ef59b0", "Increase (%)");
			this.AU_AllocationIncrementBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 56, true);
			this.AU_AllocationIncrementBoundCalcEdit.Name = "AU_AllocationIncrementBoundCalcEdit";
			this.AU_AllocationIncrementBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 20, true);
			this.AU_AllocationIncrementBoundCalcEdit.TabIndex = 5;
			this.AU_AllocationIncrementBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AU_Calc_TotalAmountBoundReadOnlyCalcEdit
			// 
			this.AU_Calc_TotalAmountBoundReadOnlyCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AU_Calc_TotalAmountBoundReadOnlyCalcEdit, "TotalAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((GLBudget)(null)).TotalAmount)));
			this.AU_Calc_TotalAmountBoundReadOnlyCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLBudgetForm|2f9c37af-15e2-4ac6-94a7-2161cfe70318", "Total Amount");
			this.AU_Calc_TotalAmountBoundReadOnlyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(580, 486, true);
			this.AU_Calc_TotalAmountBoundReadOnlyCalcEdit.Name = "AU_Calc_TotalAmountBoundReadOnlyCalcEdit";
			this.AU_Calc_TotalAmountBoundReadOnlyCalcEdit.ShowGroupSeparators = false;
			this.AU_Calc_TotalAmountBoundReadOnlyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.AU_Calc_TotalAmountBoundReadOnlyCalcEdit.TabIndex = 8;
			this.AU_Calc_TotalAmountBoundReadOnlyCalcEdit.Text = "0.00";
			this.AU_Calc_TotalAmountBoundReadOnlyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GuidPanel
			// 
			this.GuidPanel.Controls.Add(this.AU_GBBoundFindBox);
			this.GuidPanel.Controls.Add(this.AU_AGBoundFindBox);
			this.GuidPanel.Controls.Add(this.AU_GEBoundFindBox);
			this.GuidPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 35, true);
			this.GuidPanel.Name = "GuidPanel";
			this.GuidPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 87, true);
			this.GuidPanel.TabIndex = 2;
			// 
			// AllocationPanel
			// 
			this.AllocationPanel.AutoSize = true;
			this.AllocationPanel.Controls.Add(this.AU_AllocationTypeBoundDropDownEdit);
			this.AllocationPanel.Controls.Add(this.AU_AllocationValueBoundCalcEdit);
			this.AllocationPanel.Controls.Add(this.AU_AllocationIncrementBoundCalcEdit);
			this.AllocationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(434, 35, true);
			this.AllocationPanel.Name = "AllocationPanel";
			this.AllocationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 87, true);
			this.AllocationPanel.TabIndex = 5;
			// 
			// AccGLBudgetLinesBoundGrid
			// 
			this.AccGLBudgetLinesBoundGrid.AllowNavigation = false;
			this.AccGLBudgetLinesBoundGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AccGLBudgetLinesBoundGrid, "BudgetLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GLBudget)(null)).BudgetLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((GLBudgetLine)(((System.Collections.IList)(((GLBudget)(null)).BudgetLines)).SyncRoot)).AD_Period)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((GLBudgetLine)(((System.Collections.IList)(((GLBudget)(null)).BudgetLines)).SyncRoot)).LastYearActual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GLBudgetLine)(((System.Collections.IList)(((GLBudget)(null)).BudgetLines)).SyncRoot)).LastYearActualDebitCredit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((GLBudgetLine)(((System.Collections.IList)(((GLBudget)(null)).BudgetLines)).SyncRoot)).LastYearBudget)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GLBudgetLine)(((System.Collections.IList)(((GLBudget)(null)).BudgetLines)).SyncRoot)).LastYearBudgetDebitCredit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((GLBudgetLine)(((System.Collections.IList)(((GLBudget)(null)).BudgetLines)).SyncRoot)).UnsignedAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GLBudgetLine)(((System.Collections.IList)(((GLBudget)(null)).BudgetLines)).SyncRoot)).DebitCreditSign)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GLBudgetLine)(((System.Collections.IList)(((GLBudget)(null)).BudgetLines)).SyncRoot)).DebitCreditTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((GLBudgetLine)(((System.Collections.IList)(((GLBudget)(null)).BudgetLines)).SyncRoot)).AD_Percent)));
			this.AccGLBudgetLinesBoundGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "AD_Period";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.ShowGroupSeparators = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLBudgetForm|47256c28-5050-4347-aac8-ed7d981e5819", "Last Year Actual");
			zCalcEditColumnStyleInfo2.ColumnName = "LastYearActual";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.ShowGroupSeparators = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLBudgetForm|f241d7fc-11dd-4364-bbd7-f8e1dfb34848", "Debit/Credit");
			zTextBoxColumnStyleInfo1.ColumnName = "LastYearActualDebitCredit";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLBudgetForm|020d551f-7125-4e6f-845d-89fc903d0cf3", "Last Year Budget");
			zCalcEditColumnStyleInfo3.ColumnName = "LastYearBudget";
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.ShowGroupSeparators = false;
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLBudgetForm|fefa69a8-a875-4c1b-ad70-dc2831589c89", "Debit/Credit");
			zTextBoxColumnStyleInfo2.ColumnName = "LastYearBudgetDebitCredit";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLBudgetForm|0870363e-f15f-43a6-b973-562b8f455cf8", "Amount");
			zCalcEditColumnStyleInfo4.ColumnName = "UnsignedAmount";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLBudgetForm|ca376f68-cd86-489f-ad39-0601704a080f", "Debit/Credit");
			zDropEditColumnStyleInfo1.ColumnName = "DebitCreditSign";
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "AD_Percent";
			zCalcEditColumnStyleInfo5.Decimals = 3;
			zCalcEditColumnStyleInfo5.ShowGroupSeparators = false;
			this.AccGLBudgetLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.AccGLBudgetLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.AccGLBudgetLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AccGLBudgetLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.AccGLBudgetLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AccGLBudgetLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.AccGLBudgetLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AccGLBudgetLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.AccGLBudgetLinesBoundGrid.GridId = "4e1e9789-dce5-4082-b16c-95d5ec256ade";
			this.AccGLBudgetLinesBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AccGLBudgetLinesBoundGrid.LayoutKey = "zGrid1";
			this.AccGLBudgetLinesBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 128, true);
			this.AccGLBudgetLinesBoundGrid.Name = "AccGLBudgetLinesBoundGrid";
			this.AccGLBudgetLinesBoundGrid.RowHeadersVisible = false;
			this.AccGLBudgetLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(825, 352, true);
			this.AccGLBudgetLinesBoundGrid.TabIndex = 6;
			// 
			// AU_YearBoundYearEdit
			// 
			this.BindingSource.SetBindingMember(this.AU_YearBoundYearEdit, "AU_Year");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((GLBudget)(null)).AU_Year)));
			this.AU_YearBoundYearEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLBudgetForm|6e777cf1-06f5-4b22-9b75-0a62076dbdf2", "Budget Year");
			this.AU_YearBoundYearEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 9, true);
			this.AU_YearBoundYearEdit.Name = "AU_YearBoundYearEdit";
			this.AU_YearBoundYearEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.AU_YearBoundYearEdit.TabIndex = 1;
			this.AU_YearBoundYearEdit.Text = "2006";
			this.AU_YearBoundYearEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zTabControl1
			// 
			this.zTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.zTabControl1.Controls.Add(this.Budget);
			this.zTabControl1.Controls.Add(this.zStmNoteTabPage1);
			this.zTabControl1.Controls.Add(this.zEventTabPage1);
			this.zTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.SelectedIndex = 0;
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 541, true);
			this.zTabControl1.TabIndex = 0;
			// 
			// Budget
			// 
			this.Budget.BackColor = System.Drawing.SystemColors.Control;
			this.Budget.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLBudgetForm|a6958eee-1268-4b39-bddb-070d1990ca5c", "Budget");
			this.Budget.Controls.Add(this.TotalPercentageCalcEdit);
			this.Budget.Controls.Add(this.AllocationPanel);
			this.Budget.Controls.Add(this.AU_YearBoundYearEdit);
			this.Budget.Controls.Add(this.AU_Calc_TotalAmountBoundReadOnlyCalcEdit);
			this.Budget.Controls.Add(this.AccGLBudgetLinesBoundGrid);
			this.Budget.Controls.Add(this.GuidPanel);
			this.Budget.Controls.Add(this.AU_OpeningBoundCalcEdit);
			this.Budget.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.Budget.Name = "Budget";
			this.Budget.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Budget.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 514, true);
			this.Budget.TabIndex = 0;
			// 
			// TotalPercentageCalcEdit
			// 
			this.TotalPercentageCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalPercentageCalcEdit, "TotalPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((GLBudget)(null)).TotalPercentage)));
			this.TotalPercentageCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLBudgetForm|effc77f2-2490-4a40-9b68-30240120c0d0", "Total %");
			this.TotalPercentageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(763, 486, true);
			this.TotalPercentageCalcEdit.Name = "TotalPercentageCalcEdit";
			this.TotalPercentageCalcEdit.ShowGroupSeparators = false;
			this.TotalPercentageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.TotalPercentageCalcEdit.TabIndex = 10;
			this.TotalPercentageCalcEdit.Text = "0.00";
			this.TotalPercentageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 514, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 514, true);
			this.zEventTabPage1.TabIndex = 2;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.PostingButtonsControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 541, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 36, true);
			this.BottomPanel.TabIndex = 3;
			// 
			// PostingButtonsControl
			// 
			this.PostingButtonsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(595, 5, true);
			this.PostingButtonsControl.Name = "PostingButtonsControl";
			this.PostingButtonsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 25, true);
			this.PostingButtonsControl.TabIndex = 2;
			// 
			// GLBudgetForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 603, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLBudgetForm|f629dac4-0c9e-4372-a4f9-aaa1fba11a60", "GL Budget");
			this.Controls.Add(this.zTabControl1);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(GLBudget);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.GeneralLedger.GLBudget.GLBudget";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 630, true);
			this.Name = "GLBudgetForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.zTabControl1, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.GuidPanel.ResumeLayout(false);
			this.AllocationPanel.ResumeLayout(false);
			this.AllocationPanel.PerformLayout();
			((ISupportInitialize)(this.AccGLBudgetLinesBoundGrid)).EndInit();
			this.zTabControl1.ResumeLayout(false);
			this.Budget.ResumeLayout(false);
			this.Budget.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

	}
}
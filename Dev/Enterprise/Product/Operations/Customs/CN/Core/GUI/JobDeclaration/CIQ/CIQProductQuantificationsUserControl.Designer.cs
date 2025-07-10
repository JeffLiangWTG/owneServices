namespace Enterprise.Customs.CN.GUI
{
	partial class CIQProductQuantificationsUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.QuantificationGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CSI_CodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CSI_ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CSI_LineNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CSI_UnitOfQuantityDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VINGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BillOfLadingDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.VIN_ChassisNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VIN_EngineNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VIN_ModelENTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VIN_ProductNameCNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VIN_ProductNameENTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VIN_QGPTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VIN_VINTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VINCollectionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.QuantificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CSI_QuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.QuantificationGrid)).BeginInit();
			this.QuantificationGrid.SuspendLayout();
			this.CSI_CodeDropEdit.SuspendLayout();
			this.VINGroupBox.SuspendLayout();
			this.BillOfLadingDateEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.VINCollectionGrid)).BeginInit();
			this.VINCollectionGrid.SuspendLayout();
			this.QuantificationGroupBox.SuspendLayout();
			this.CSI_QuantityCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.JobComInvoiceLine);
			// 
			// QuantificationGrid
			// 
			this.QuantificationGrid.AllowNavigation = false;
			this.QuantificationGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QuantificationGrid, "CIQProductQualifications");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).CIQProductQualifications)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CIQProductQualification)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).CIQProductQualifications)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CIQProductQualification)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).CIQProductQualifications)).SyncRoot)).DocumentName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CIQProductQualification)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).CIQProductQualifications)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CN.Business.CIQProductQualification)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).CIQProductQualifications)).SyncRoot)).CSI_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CN.Business.CIQProductQualification)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).CIQProductQualifications)).SyncRoot)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CIQProductQualification)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).CIQProductQualifications)).SyncRoot)).CSI_UnitOfQuantity)));
			this.QuantificationGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "DocumentName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_LineNo";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CSI_Quantity";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.ColumnName = "CSI_UnitOfQuantity";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.QuantificationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.QuantificationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.QuantificationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.QuantificationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.QuantificationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.QuantificationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.QuantificationGrid.GridId = "68adc6af-468f-42e5-b495-fb263033f638";
			this.QuantificationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.QuantificationGrid.LayoutKey = "BatchNumbersGrid";
			this.QuantificationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 19, true);
			this.QuantificationGrid.Name = "QuantificationGrid";
			this.QuantificationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(903, 111, true);
			this.QuantificationGrid.TabIndex = 0;
			// 
			// CSI_CodeDropEdit
			// 
			this.CSI_CodeDropEdit.AllowDrop = true;
			this.CSI_CodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.CSI_CodeDropEdit, "CIQProductQualifications.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.CIQProductQualification)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).CIQProductQualifications)).SyncRoot)).CSI_Code)));
			this.CSI_CodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 136, true);
			this.CSI_CodeDropEdit.Name = "CSI_CodeDropEdit";
			this.CSI_CodeDropEdit.PreBoundMaxLength = 3;
			this.CSI_CodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 20, true);
			this.CSI_CodeDropEdit.TabIndex = 1;
			// 
			// CSI_ReferenceNumberTextBox
			// 
			this.CSI_ReferenceNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CSI_ReferenceNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_ReferenceNumberTextBox, "CIQProductQualifications.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CIQProductQualification)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).CIQProductQualifications)).SyncRoot)).CSI_ReferenceNumber)));
			this.CSI_ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 136, true);
			this.CSI_ReferenceNumberTextBox.Name = "CSI_ReferenceNumberTextBox";
			this.CSI_ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 20, true);
			this.CSI_ReferenceNumberTextBox.TabIndex = 2;
			// 
			// CSI_LineNoCalcEdit
			// 
			this.CSI_LineNoCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CSI_LineNoCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_LineNoCalcEdit, "CIQProductQualifications.CSI_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CN.Business.CIQProductQualification)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).CIQProductQualifications)).SyncRoot)).CSI_LineNo)));
			this.CSI_LineNoCalcEdit.DecimalPlaces = 0;
			this.CSI_LineNoCalcEdit.Decimals = 0;
			this.CSI_LineNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 160, true);
			this.CSI_LineNoCalcEdit.Name = "CSI_LineNoCalcEdit";
			this.CSI_LineNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 20, true);
			this.CSI_LineNoCalcEdit.TabIndex = 3;
			this.CSI_LineNoCalcEdit.Text = "0";
			this.CSI_LineNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CSI_UnitOfQuantityDescriptionTextBox
			// 
			// CSI_UnitOfQuantityDescriptionTextBox won't move with QuantificationGroupBox Dock change and will fail in Export BashForm UT with control overlap error if not defining Anchor.
			// Will fix and check in seperate WI
			//this.CSI_UnitOfQuantityDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.CSI_UnitOfQuantityDescriptionTextBox, "CIQProductQualifications.CSI_UnitOfQuantityDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CIQProductQualification)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).CIQProductQualifications)).SyncRoot)).CSI_UnitOfQuantityDescription)));
			this.CSI_UnitOfQuantityDescriptionTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CSI_UnitOfQuantityDescriptionTextBox, false);
			this.CSI_UnitOfQuantityDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(577, 160, true);
			this.CSI_UnitOfQuantityDescriptionTextBox.Name = "CSI_UnitOfQuantityDescriptionTextBox";
			this.CSI_UnitOfQuantityDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 20, true);
			this.CSI_UnitOfQuantityDescriptionTextBox.TabIndex = 5;
			// 
			// VINGroupBox
			// 
			this.VINGroupBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("b61c144a-6275-48af-925b-cbb1580140a5", "VINs");
			this.VINGroupBox.Controls.Add(this.BillOfLadingDateEdit);
			this.VINGroupBox.Controls.Add(this.VIN_ChassisNoTextBox);
			this.VINGroupBox.Controls.Add(this.VIN_EngineNoTextBox);
			this.VINGroupBox.Controls.Add(this.VIN_ModelENTextBox);
			this.VINGroupBox.Controls.Add(this.VIN_ProductNameCNTextBox);
			this.VINGroupBox.Controls.Add(this.VIN_ProductNameENTextBox);
			this.VINGroupBox.Controls.Add(this.VIN_QGPTextBox);
			this.VINGroupBox.Controls.Add(this.VIN_VINTextBox);
			this.VINGroupBox.Controls.Add(this.VINCollectionGrid);
			this.VINGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VINGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 188, true);
			this.VINGroupBox.Name = "VINGroupBox";
			this.VINGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 158, true);
			this.VINGroupBox.TabIndex = 5;
			this.VINGroupBox.TabStop = false;
			// 
			// BillOfLadingDateEdit
			// 
			this.BillOfLadingDateEdit.AllowDrop = true;
			this.BillOfLadingDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.BillOfLadingDateEdit, "EntryInstruction.BillOfLadingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).EntryInstruction.BillOfLadingDate)));
			this.BillOfLadingDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 84, true);
			this.BillOfLadingDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.BillOfLadingDateEdit.Name = "BillOfLadingDateEdit";
			this.BillOfLadingDateEdit.TabIndex = 1;
			this.BillOfLadingDateEdit.ReadOnly = true;
			this.BillOfLadingDateEdit.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("E84F0307-6943-4819-AAB7-C87DC0C1FD39", "Bill of Lading Date");
			// 
			// VIN_ChassisNoTextBox
			// 
			this.VIN_ChassisNoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.VIN_ChassisNoTextBox, "VINDataCollection.XC_ChassisNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.VINData)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).VINDataCollection)).SyncRoot)).XC_ChassisNo)));
			this.VIN_ChassisNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(421, 108, true);
			this.VIN_ChassisNoTextBox.Name = "VIN_ChassisNoTextBox";
			this.VIN_ChassisNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.VIN_ChassisNoTextBox.TabIndex = 5;
			// 
			// VIN_EngineNoTextBox
			// 
			this.VIN_EngineNoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.VIN_EngineNoTextBox, "VINDataCollection.XC_EngineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.VINData)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).VINDataCollection)).SyncRoot)).XC_EngineNo)));
			this.VIN_EngineNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 108, true);
			this.VIN_EngineNoTextBox.Name = "VIN_EngineNoTextBox";
			this.VIN_EngineNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.VIN_EngineNoTextBox.TabIndex = 4;
			// 
			// VIN_ModelENTextBox
			// 
			this.VIN_ModelENTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.VIN_ModelENTextBox, "VINDataCollection.XC_ModelEN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.VINData)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).VINDataCollection)).SyncRoot)).XC_ModelEN)));
			this.VIN_ModelENTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(729, 132, true);
			this.VIN_ModelENTextBox.Name = "VIN_ModelENTextBox";
			this.VIN_ModelENTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.VIN_ModelENTextBox.TabIndex = 8;
			// 
			// VIN_ProductNameCNTextBox
			// 
			this.VIN_ProductNameCNTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.VIN_ProductNameCNTextBox, "VINDataCollection.XC_ProductNameCN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.VINData)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).VINDataCollection)).SyncRoot)).XC_ProductNameCN)));
			this.VIN_ProductNameCNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 132, true);
			this.VIN_ProductNameCNTextBox.Name = "VIN_ProductNameCNTextBox";
			this.VIN_ProductNameCNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.VIN_ProductNameCNTextBox.TabIndex = 6;
			// 
			// VIN_ProductNameENTextBox
			// 
			this.VIN_ProductNameENTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.VIN_ProductNameENTextBox, "VINDataCollection.XC_ProductNameEN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.VINData)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).VINDataCollection)).SyncRoot)).XC_ProductNameEN)));
			this.VIN_ProductNameENTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(421, 132, true);
			this.VIN_ProductNameENTextBox.Name = "VIN_ProductNameENTextBox";
			this.VIN_ProductNameENTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.VIN_ProductNameENTextBox.TabIndex = 7;
			// 
			// VIN_QGPTextBox
			// 
			this.VIN_QGPTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.VIN_QGPTextBox, "VINDataCollection.XC_QGP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.VINData)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).VINDataCollection)).SyncRoot)).XC_QGP)));
			this.VIN_QGPTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(421, 84, true);
			this.VIN_QGPTextBox.Name = "VIN_QGPTextBox";
			this.VIN_QGPTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.VIN_QGPTextBox.TabIndex = 2;
			// 
			// VIN_VINTextBox
			// 
			this.VIN_VINTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.VIN_VINTextBox, "VINDataCollection.XC_VIN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.VINData)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).VINDataCollection)).SyncRoot)).XC_VIN)));
			this.VIN_VINTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(729, 84, true);
			this.VIN_VINTextBox.Name = "VIN_VINTextBox";
			this.VIN_VINTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.VIN_VINTextBox.TabIndex = 3;
			// 
			// VINCollectionGrid
			// 
			this.VINCollectionGrid.AllowNavigation = false;
			this.VINCollectionGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.VINCollectionGrid, "VINDataCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).VINDataCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.VINData)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).VINDataCollection)).SyncRoot)).XC_QGP)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.VINData)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).VINDataCollection)).SyncRoot)).XC_VIN)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.VINData)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).VINDataCollection)).SyncRoot)).XC_EngineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.VINData)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).VINDataCollection)).SyncRoot)).XC_ChassisNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.VINData)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).VINDataCollection)).SyncRoot)).XC_ProductNameCN)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.VINData)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).VINDataCollection)).SyncRoot)).XC_ProductNameEN)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.VINData)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).VINDataCollection)).SyncRoot)).XC_ModelEN)));
			this.VINCollectionGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("39b8cd32-6f87-48e2-8341-766d22915979", "QGP");
			zTextBoxColumnStyleInfo3.ColumnName = "XC_QGP";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("d1c1e395-27e2-4af5-8ae0-76b71020fe4c", "VIN");
			zTextBoxColumnStyleInfo4.ColumnName = "XC_VIN";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("40914aaa-aa8f-49aa-9189-7222ea9ca1d6", "Engine No.");
			zTextBoxColumnStyleInfo5.ColumnName = "XC_EngineNo";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("df82ab8f-b8ec-4441-bcfa-d8c57cc16fc7", "Chassis No.");
			zTextBoxColumnStyleInfo6.ColumnName = "XC_ChassisNo";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo7.ColumnName = "XC_ProductNameCN";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo8.ColumnName = "XC_ProductNameEN";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo9.ColumnName = "XC_ModelEN";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.VINCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.VINCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.VINCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.VINCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.VINCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.VINCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.VINCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.VINCollectionGrid.GridId = "f4be9c74-2b84-4572-9608-cdccd6aaf07f";
			this.VINCollectionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.VINCollectionGrid.LayoutKey = "VINCollectionGrid";
			this.VINCollectionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 15, true);
			this.VINCollectionGrid.Name = "VINCollectionGrid";
			this.VINCollectionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(902, 63, true);
			this.VINCollectionGrid.TabIndex = 0;
			// 
			// QuantificationGroupBox
			// 
			this.QuantificationGroupBox.Controls.Add(this.QuantificationGrid);
			this.QuantificationGroupBox.Controls.Add(this.CSI_QuantityCalcDropEdit);
			this.QuantificationGroupBox.Controls.Add(this.CSI_UnitOfQuantityDescriptionTextBox);
			this.QuantificationGroupBox.Controls.Add(this.CSI_LineNoCalcEdit);
			this.QuantificationGroupBox.Controls.Add(this.CSI_ReferenceNumberTextBox);
			this.QuantificationGroupBox.Controls.Add(this.CSI_CodeDropEdit);
			this.QuantificationGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.QuantificationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.QuantificationGroupBox.Name = "QuantificationGroupBox";
			this.QuantificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 188, true);
			this.QuantificationGroupBox.TabIndex = 5;
			this.QuantificationGroupBox.TabStop = false;
			this.QuantificationGroupBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("566DDB83-9CDB-42CC-8DEA-882F6A756BC1", "Product Qualifications");
			// 
			// CSI_QuantityCalcDropEdit
			// 
			this.CSI_QuantityCalcDropEdit.AllowDrop = true;
			this.CSI_QuantityCalcDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.CSI_QuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CN.Business.CIQProductQualification)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).CIQProductQualifications)).SyncRoot)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CIQProductQualification)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceLine)(null)).CIQProductQualifications)).SyncRoot)).CSI_UnitOfQuantity)));
			this.CSI_QuantityCalcDropEdit.BindToAmount = "CIQProductQualifications.CSI_Quantity";
			this.CSI_QuantityCalcDropEdit.BindToUnit = "CIQProductQualifications.CSI_UnitOfQuantity";
			this.CSI_QuantityCalcDropEdit.Decimals = 0;
			this.CSI_QuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 160, true);
			this.CSI_QuantityCalcDropEdit.Name = "CSI_QuantityCalcDropEdit";
			this.CSI_QuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 20, true);
			this.CSI_QuantityCalcDropEdit.TabIndex = 4;
			this.CSI_QuantityCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// CIQProductQuantificationsUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.VINGroupBox);
			this.Controls.Add(this.QuantificationGroupBox);
			this.Name = "CIQProductQuantificationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 346, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.QuantificationGrid)).EndInit();
			this.QuantificationGrid.ResumeLayout(false);
			this.QuantificationGrid.PerformLayout();
			this.CSI_CodeDropEdit.ResumeLayout(true);
			this.CSI_CodeDropEdit.PerformLayout();
			this.VINGroupBox.ResumeLayout(false);
			this.VINGroupBox.PerformLayout();
			this.BillOfLadingDateEdit.ResumeLayout(true);
			this.BillOfLadingDateEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.VINCollectionGrid)).EndInit();
			this.VINCollectionGrid.ResumeLayout(false);
			this.VINCollectionGrid.PerformLayout();
			this.QuantificationGroupBox.ResumeLayout(false);
			this.QuantificationGroupBox.PerformLayout();
			this.CSI_QuantityCalcDropEdit.ResumeLayout(true);
			this.CSI_QuantityCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid QuantificationGrid;
		private ZArchitecture.GUI.ZDropEdit CSI_CodeDropEdit;
		private ZArchitecture.ZTextBox CSI_ReferenceNumberTextBox;
		private ZArchitecture.ZCalcEdit CSI_LineNoCalcEdit;
		private ZArchitecture.GUI.ZGroupBox QuantificationGroupBox;
		private ZArchitecture.GUI.ZCalcDropEdit CSI_QuantityCalcDropEdit;
		private ZArchitecture.GUI.ZGroupBox VINGroupBox;
		private ZArchitecture.ZGrid VINCollectionGrid;
		private ZArchitecture.ZTextBox CSI_UnitOfQuantityDescriptionTextBox;
		private ZArchitecture.ZTextBox VIN_VINTextBox;
		private ZArchitecture.ZTextBox VIN_QGPTextBox;
		private ZArchitecture.ZTextBox VIN_EngineNoTextBox;
		private ZArchitecture.ZTextBox VIN_ChassisNoTextBox;
		private ZArchitecture.ZTextBox VIN_ProductNameCNTextBox;
		private ZArchitecture.ZTextBox VIN_ProductNameENTextBox;
		private ZArchitecture.ZTextBox VIN_ModelENTextBox;
		private ZArchitecture.GUI.ZDateEdit BillOfLadingDateEdit;
	}
}

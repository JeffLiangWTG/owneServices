using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;

partial class SumARegisterUserControl
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

	#region Windows Form Designer generated code

	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
		Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
		Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
		Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
		Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
		Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
		this.LinesGrid = new Enterprise.ZArchitecture.ZGrid();
		this.TransactionGrid = new Enterprise.ZArchitecture.ZGrid();
		this.BottomSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
		this.LinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		this.LinesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
		this.LineDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		this.TransactionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		this.HeaderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
		this.HeadersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		this.DetailsHeaderDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
		this.LinesDetailsDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
		this.LinesGrid.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.TransactionGrid)).BeginInit();
		this.TransactionGrid.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.BottomSplitContainer)).BeginInit();
		this.BottomSplitContainer.Panel1.SuspendLayout();
		this.BottomSplitContainer.Panel2.SuspendLayout();
		this.BottomSplitContainer.SuspendLayout();
		this.LinesGroupBox.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.LinesSplitContainer)).BeginInit();
		this.LinesSplitContainer.Panel1.SuspendLayout();
		this.LinesSplitContainer.Panel2.SuspendLayout();
		this.LinesSplitContainer.SuspendLayout();
		this.LineDetailsGroupBox.SuspendLayout();
		this.TransactionsGroupBox.SuspendLayout();
		this.HeaderPanel.SuspendLayout();
		this.HeadersGroupBox.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader);
		// 
		// LinesGrid
		// 
		this.LinesGrid.AllowNavigation = false;
		this.BindingSource.SetBindingMember(this.LinesGrid, "CusTempStorageRegLines");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_LineNumber)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_OwnerReferenceType)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_OwnerReference)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_LocationOfGoods)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_GoodsDescription)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_LimitDate)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_PackagesRemaining)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_PackageType)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_CustomsStatus)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_UnionStatus)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_CustodianIdentifier)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_CustodianIdentifierBranchNo)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_GoodsOwnerIdentifier)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_GoodsOwnerIdentifierBranchNo)));
		this.LinesGrid.CaptionVisible = false;
		zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
		zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("260ae329-37c2-4915-adc9-6dd551562d68", "Line No.");
		zCalcEditColumnStyleInfo1.ColumnName = "SRL_LineNumber";
		zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
		zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("bca68909-c5e5-4679-bc3d-d7094dfa5a2b", "Owner Reference Type");
		zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		zDropEditColumnStyleInfo1.ColumnName = "SRL_OwnerReferenceType";
		zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("5dbaef32-f854-457c-8410-fc22f2be3ba3", "Owner Reference");
		zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(134);
		zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("658d6eb1-2c5e-4f73-845f-1c23a1fef9d2", "Owner Reference Number");
		zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		zTextBoxColumnStyleInfo1.ColumnName = "SRL_OwnerReference";
		zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("5dbaef32-f854-457c-8410-fc22f2be3ba3", "Owner Reference");
		zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(149);
		zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("e8a85588-bdb8-4789-9db2-72aa8df0f512", "Location of Goods");
		zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		zTextBoxColumnStyleInfo2.ColumnName = "SRL_LocationOfGoods";
		zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
		zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("f8701bb1-ff04-489c-8db2-4a8efd95e59f", "Goods Description");
		zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		zTextBoxColumnStyleInfo3.ColumnName = "SRL_GoodsDescription";
		zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
		zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("cbff75c7-f268-4df5-87d3-abe81f1ed8be", "Limit Date");
		zDateEditColumnStyleInfo1.ColumnName = "SRL_LimitDate";
		zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
		zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(73);
		zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
		zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("4a430d29-2dbc-4097-9d2c-ee9223a76422", "Packages Remaining");
		zCalcEditColumnStyleInfo2.ColumnName = "SRL_PackagesRemaining";
		zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(126);
		zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("0599cc48-2fba-4e7e-b334-d7f7da3843cc", "Package Type");
		zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		zDropEditColumnStyleInfo2.ColumnName = "SRL_PackageType";
		zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
		zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("f457d861-486a-4bcb-8549-c464a75a9ef4", "Customs Status");
		zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		zDropEditColumnStyleInfo3.ColumnName = "SRL_CustomsStatus";
		zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
		zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("e1b229a3-91b9-4073-80ca-cb277145e4a0", "Union Status");
		zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		zDropEditColumnStyleInfo4.ColumnName = "SRL_UnionStatus";
		zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
		zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("0ba421b5-78e2-435c-a575-7846ba290fa0", "Custodian EORI");
		zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		zTextBoxColumnStyleInfo4.ColumnName = "SRL_CustodianIdentifier";
		zTextBoxColumnStyleInfo4.GroupName = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("2f1fbbb1-5a59-4d6d-93a8-8e3a12659c2f", "Custodian");
		zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(99);
		zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("5aadf001-39f1-42e6-992e-705ca3ec4a63", "Custodian Branch");
		zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		zTextBoxColumnStyleInfo5.ColumnName = "SRL_CustodianIdentifierBranchNo";
		zTextBoxColumnStyleInfo5.GroupName = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("2f1fbbb1-5a59-4d6d-93a8-8e3a12659c2f", "Custodian");
		zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(108);
		zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("9ef7f9f8-54ff-438b-8c82-5ecf3f00b529", "Disp. Ent. Trader EORI", "Disposal Entitled Trader EORI");
		zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		zTextBoxColumnStyleInfo6.ColumnName = "SRL_GoodsOwnerIdentifier";
		zTextBoxColumnStyleInfo6.GroupName = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("e68f4f91-af4b-4d47-97bf-c0c49e7cefff", "Disposal Entitled Trader");
		zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(134);
		zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("a0f7a1d0-4ac1-48e5-9893-2cdd3d38b658", "Disp. Ent. Trader Branch", "Disposal Entitled Trader EORI");
		zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		zTextBoxColumnStyleInfo7.ColumnName = "SRL_GoodsOwnerIdentifierBranchNo";
		zTextBoxColumnStyleInfo7.GroupName = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("e68f4f91-af4b-4d47-97bf-c0c49e7cefff", "Disposal Entitled Trader");
		zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(142);
		this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
		this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
		this.LinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
		this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
		this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
		this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
		this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
		this.LinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.LinesGrid.GridId = "b1863564-af0e-42f6-bff4-dfaef575edff";
		this.LinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
		this.LinesGrid.LayoutKey = "LinesGrid";
		this.LinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.LinesGrid.Name = "LinesGrid";
		this.LinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 199, true);
		this.LinesGrid.TabIndex = 0;
		// 
		// TransactionGrid
		// 
		this.TransactionGrid.AllowNavigation = false;
		this.BindingSource.SetBindingMember(this.TransactionGrid, "CusTempStorageRegLines.CusTempStorageRegLineTransactions");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactions)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactions)).SyncRoot)).SRT_TransactionType)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactions)).SyncRoot)).TransactionTypeDescription)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactions)).SyncRoot)).SRT_InternalReferenceNumber)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactions)).SyncRoot)).SRT_GrossWeight)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactions)).SyncRoot)).SRT_BondAmount)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactions)).SyncRoot)).SRT_PackageQty)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactions)).SyncRoot)).SRT_ReferenceType)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactions)).SyncRoot)).SRT_Reference)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactions)).SyncRoot)).SRT_Comments)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactions)).SyncRoot)).SRT_SystemCreateTimeUtc)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).CusTempStorageRegLineTransactions)).SyncRoot)).SRT_SystemCreateUser)));
		this.TransactionGrid.CaptionVisible = false;
		zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("6e91dded-8b8f-48ec-a151-ab4a618ebe98", "Transaction Type");
		zDropEditColumnStyleInfo5.ColumnName = "SRT_TransactionType";
		zDropEditColumnStyleInfo5.GroupName = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("fb6c467d-a9f5-48f2-9252-79a4e05a9e63", "Transaction Type");
		zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
		zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("3d2502fc-7560-41ef-b093-753e795ca24d", "Transaction Type Description");
		zTextBoxColumnStyleInfo8.ColumnName = "TransactionTypeDescription";
		zTextBoxColumnStyleInfo8.GroupName = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("fb6c467d-a9f5-48f2-9252-79a4e05a9e63", "Transaction Type");
		zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
		zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("498196e8-bad6-4735-b2c7-765530e9b79a", "Internal Reference No.");
		zTextBoxColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		zTextBoxColumnStyleInfo9.ColumnName = "SRT_InternalReferenceNumber";
		zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
		zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
		zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("d9c7f31f-f1bc-42f9-9c41-3ce7e1361b83", "Gross Weight in KGs");
		zCalcEditColumnStyleInfo3.ColumnName = "SRT_GrossWeight";
		zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
		zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
		zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("33966204-ed9f-472b-8463-4d8318432068", "Package Qty", "Package Quantity", "");
		zCalcEditColumnStyleInfo4.ColumnName = "SRT_PackageQty";
		zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		zCalcEditColumnStyleInfo5.IsVisible = false;
		zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
		zCalcEditColumnStyleInfo5.ColumnName = "SRT_BondAmount";
		zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
		zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("f3e4d947-ba18-46aa-93ba-23c1a3ccf6c2", "Reference Type");
		zDropEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		zDropEditColumnStyleInfo6.ColumnName = "SRT_ReferenceType";
		zDropEditColumnStyleInfo6.GroupName = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("f29ea6fb-940e-427d-a9a4-823a3bcd5fcf", "Reference");
		zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
		zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("7fc21202-2674-4d1a-97b0-549ef723aa38", "Reference Number");
		zTextBoxColumnStyleInfo10.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		zTextBoxColumnStyleInfo10.ColumnName = "SRT_Reference";
		zTextBoxColumnStyleInfo10.GroupName = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("f29ea6fb-940e-427d-a9a4-823a3bcd5fcf", "Reference");
		zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("4ee5ce5d-b7f4-43cd-aad7-e315ad880f90", "Comments");
		zTextBoxColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		zTextBoxColumnStyleInfo11.ColumnName = "SRT_Comments";
		zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("16ca4f1a-c27b-4823-b92b-2df569ebe3aa", "Create Time");
		zDateEditColumnStyleInfo2.ColumnName = "SRT_SystemCreateTimeUtc";
		zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("f1a9fad1-7d54-401d-8fc1-98cb58fd4379", "Create User");
		zTextBoxColumnStyleInfo12.ColumnName = "SRT_SystemCreateUser";
		zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		this.TransactionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
		this.TransactionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
		this.TransactionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
		this.TransactionGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
		this.TransactionGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
		this.TransactionGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
		this.TransactionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
		this.TransactionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
		this.TransactionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
		this.TransactionGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
		this.TransactionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
		this.TransactionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.TransactionGrid.GridId = "e0b8f184-2488-4f3c-8722-775fcefb1b2b";
		this.TransactionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
		this.TransactionGrid.LayoutKey = "TransactionGrid";
		this.TransactionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
		this.TransactionGrid.Name = "TransactionGrid";
		this.TransactionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1185, 239, true);
		this.TransactionGrid.TabIndex = 0;
		// 
		// BottomSplitContainer
		// 
		this.BottomSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
		                                                                          | System.Windows.Forms.AnchorStyles.Left) 
		                                                                         | System.Windows.Forms.AnchorStyles.Right)));
		this.BottomSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
		this.BottomSplitContainer.IsSplitterFixed = true;
		this.BottomSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 121, true);
		this.BottomSplitContainer.Name = "BottomSplitContainer";
		this.BottomSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
		// 
		// BottomSplitContainer.Panel1
		// 
		this.BottomSplitContainer.Panel1.Controls.Add(this.LinesGroupBox);
		// 
		// BottomSplitContainer.Panel2
		// 
		this.BottomSplitContainer.Panel2.Controls.Add(this.TransactionsGroupBox);
		this.BottomSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1189, 468, true);
		this.BottomSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(216);
		this.BottomSplitContainer.SplitterWidth = 12;
		this.BottomSplitContainer.TabIndex = 0;
		// 
		// LinesGroupBox
		// 
		this.LinesGroupBox.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("2e99bdfa-94df-4e08-b2d1-95a5f332d084", "Lines");
		this.LinesGroupBox.Controls.Add(this.LinesSplitContainer);
		this.LinesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.LinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.LinesGroupBox.Name = "LinesGroupBox";
		this.LinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1189, 215, true);
		this.LinesGroupBox.TabIndex = 0;
		this.LinesGroupBox.TabStop = false;
		// 
		// LinesSplitContainer
		// 
		this.LinesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
		this.LinesSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
		this.LinesSplitContainer.IsSplitterFixed = true;
		this.LinesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
		this.LinesSplitContainer.Name = "LinesSplitContainer";
		// 
		// LinesSplitContainer.Panel1
		// 
		this.LinesSplitContainer.Panel1.Controls.Add(this.LinesGrid);
		// 
		// LinesSplitContainer.Panel2
		// 
		this.LinesSplitContainer.Panel2.Controls.Add(this.LineDetailsGroupBox);
		this.LinesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1185, 194, true);
		this.LinesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(470);
		this.LinesSplitContainer.SplitterWidth = 12;
		this.LinesSplitContainer.TabIndex = 0;
		// 
		// LineDetailsGroupBox
		// 
		this.LineDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("9ab3118e-2ec5-4a3d-93b1-6c00f48d2c23", "Line Details");
		this.LineDetailsGroupBox.Controls.Add(this.LinesDetailsDynamicLayoutPanel);
		this.LineDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.LineDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.LineDetailsGroupBox.Name = "LineDetailsGroupBox";
		this.LineDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 194, true);
		this.LineDetailsGroupBox.TabIndex = 1;
		this.LineDetailsGroupBox.TabStop = false;
		// 
		// TransactionsGroupBox
		// 
		this.TransactionsGroupBox.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("4be4e51a-9fc7-4dfd-9c5b-5f35ec325058", "Transactions");
		this.TransactionsGroupBox.Controls.Add(this.TransactionGrid);
		this.TransactionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.TransactionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.TransactionsGroupBox.Name = "TransactionsGroupBox";
		this.TransactionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1189, 255, true);
		this.TransactionsGroupBox.TabIndex = 0;
		this.TransactionsGroupBox.TabStop = false;
		// 
		// HeaderPanel
		// 
		this.HeaderPanel.Controls.Add(this.HeadersGroupBox);
		this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
		this.HeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.HeaderPanel.Name = "HeaderPanel";
		this.HeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1192, 115, true);
		this.HeaderPanel.TabIndex = 0;
		// 
		// HeadersGroupBox
		//
		this.HeadersGroupBox.Controls.Add(DetailsHeaderDynamicLayoutPanel);
		this.HeadersGroupBox.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("9789a9dc-34b9-40f1-8884-77f194bb2545", "Header");
		this.HeadersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.HeadersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.HeadersGroupBox.Name = "HeadersGroupBox";
		this.HeadersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1192, 115, true);
		this.HeadersGroupBox.TabIndex = 0;
		this.HeadersGroupBox.TabStop = false;
		//
		// DetailsHeaderDynamicLayoutPanel
		//
		this.DetailsHeaderDynamicLayoutPanel.Name = "DetailsHeaderDynamicLayoutPanel";
		this.DetailsHeaderDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 10, true);
		this.DetailsHeaderDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 100, true);
		this.DetailsHeaderDynamicLayoutPanel.TabStop = false;
		this.DetailsHeaderDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		// 
		// LinesDetailsDynamicLayoutPanel
		//
		this.LinesDetailsDynamicLayoutPanel.Name = "LinesDetailsDynamicLayoutPanel";
		this.LinesDetailsDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
		this.LinesDetailsDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 100, true);
		this.LinesDetailsDynamicLayoutPanel.TabStop = false;
		this.LinesDetailsDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		// 
		// SumARegisterUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.HeaderPanel);
		this.Controls.Add(this.BottomSplitContainer);
		this.Name = "SumARegisterUserControl";
		this.ShouldSerializeTabPageMethods = true;
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1192, 590, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
		this.LinesGrid.ResumeLayout(false);
		this.LinesGrid.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.TransactionGrid)).EndInit();
		this.TransactionGrid.ResumeLayout(false);
		this.TransactionGrid.PerformLayout();
		this.BottomSplitContainer.Panel1.ResumeLayout(false);
		this.BottomSplitContainer.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)(this.BottomSplitContainer)).EndInit();
		this.BottomSplitContainer.ResumeLayout(false);
		this.BottomSplitContainer.PerformLayout();
		this.LinesGroupBox.ResumeLayout(false);
		this.LinesGroupBox.PerformLayout();
		this.LinesSplitContainer.Panel1.ResumeLayout(false);
		this.LinesSplitContainer.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)(this.LinesSplitContainer)).EndInit();
		this.LinesSplitContainer.ResumeLayout(false);
		this.LinesSplitContainer.PerformLayout();
		this.LineDetailsGroupBox.ResumeLayout(false);
		this.LineDetailsGroupBox.PerformLayout();
		this.TransactionsGroupBox.ResumeLayout(false);
		this.TransactionsGroupBox.PerformLayout();
		this.HeaderPanel.ResumeLayout(false);
		this.HeaderPanel.PerformLayout();
		this.HeadersGroupBox.ResumeLayout(false);
		this.HeadersGroupBox.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	private CargoWise.Windows.UI.KSplitContainer BottomSplitContainer;
	private ZArchitecture.GUI.ZGroupBox LinesGroupBox;
	private CargoWise.Windows.UI.KSplitContainer LinesSplitContainer;
	private ZArchitecture.GUI.ZPanel HeaderPanel;
	private ZArchitecture.GUI.ZGroupBox HeadersGroupBox;
	private ZArchitecture.ZGrid LinesGrid;
	private ZArchitecture.GUI.ZGroupBox TransactionsGroupBox;
	private ZArchitecture.ZGrid TransactionGrid;
	private ZArchitecture.GUI.ZGroupBox LineDetailsGroupBox;
	private ZArchitecture.GUI.DynamicLayoutPanel DetailsHeaderDynamicLayoutPanel;
	private ZArchitecture.GUI.DynamicLayoutPanel LinesDetailsDynamicLayoutPanel;
}

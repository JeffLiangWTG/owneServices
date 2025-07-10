using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.Module;

partial class TemporaryStorageRegisterFilterStripControl
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
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo24 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo25 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo26 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo27 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

		this.HeaderAndLinesGridSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
		this.LinesGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
		((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
		this.grid.SuspendLayout();
		this.AddStripButton.SuspendLayout();
		this.RecentItemsPanel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.HeaderAndLinesGridSplitContainer)).BeginInit();
		this.HeaderAndLinesGridSplitContainer.Panel1.SuspendLayout();
		this.HeaderAndLinesGridSplitContainer.Panel2.SuspendLayout();
		this.HeaderAndLinesGridSplitContainer.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
		this.LinesGrid.SuspendLayout();
		this.SuspendLayout();
		// 
		// grid
		// 
		this.grid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
		this.BindingSource.SetBindingMember(this.grid, ".");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)))));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).SRH_Reference)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).SRH_ArrivalDate)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).SRH_PresentationDate)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).SRH_PreviousReferenceType)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).SRH_PreviousReference)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).SRH_Status)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).SRH_SystemCreateTimeUtc)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).SRH_SystemCreateUser)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).SRH_InternalReference)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).Premises.SRP_Code)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).Premises.SRP_CustomsLocation)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).Premises.SRP_Type)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).Guarantee.PW_BondNumber)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).Guarantee.PW_BondAmount)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).Guarantee.PW_RX_NKCurrency)));
		zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("05FD0C87-1FCD-4E83-8685-42D05309C763", "Premise Code");
		zTextBoxColumnStyleInfo17.ColumnName = "Premises+SRP_Code";
		zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("BCBDD877-0B5C-4408-B45E-33A5EE29F016", "Premise Location");
		zTextBoxColumnStyleInfo18.ColumnName = "Premises+SRP_CustomsLocation";
		zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
		zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("88571508-9B07-4A02-9631-3A3A4482778F", "Premise Type");
		zTextBoxColumnStyleInfo19.ColumnName = "Premises+SRP_Type";
		zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("5d13408b-c5df-43cb-9877-35a7d863c5e2", "Job Reference");
		zTextBoxColumnStyleInfo7.ColumnName = "SRH_InternalReference";
		zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
		zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("199E2FFC-612E-4D38-8310-EF4F117C088A", "TSD Number");
		zTextBoxColumnStyleInfo1.ColumnName = "SRH_Reference";
		zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
		zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("89362fbb-a46b-439e-98a0-4218f59f3a88", "Arrival Date");
		zTextBoxColumnStyleInfo2.ColumnName = "SRH_ArrivalDate";
		zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("6868ea58-6b28-4987-be75-ef807da6b7d2", "Presentation Date");
		zDateEditColumnStyleInfo1.ColumnName = "SRH_PresentationDate";
		zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("157dab88-ea68-45ff-9056-7c053ef0e648", "Previous Ref Type");
		zTextBoxColumnStyleInfo3.ColumnName = "SRH_PreviousReferenceType";
		zTextBoxColumnStyleInfo3.GroupName = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("74E6AEA7-B7A0-4522-984A-F67DE53338CE", "Previous Ref");
		zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("d8b1a1b5-6618-4361-be69-3b5b7c67430f", "Previous Ref Number");
		zTextBoxColumnStyleInfo4.ColumnName = "SRH_PreviousReference";
		zTextBoxColumnStyleInfo4.GroupName = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("74E6AEA7-B7A0-4522-984A-F67DE53338CE", "Previous Ref");
		zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
		zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("7f1aa04b-4c0e-4604-88fd-ac8ab2fa05ae", "Status");
		zTextBoxColumnStyleInfo5.ColumnName = "SRH_Status";
		zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		zTextBoxColumnStyleInfo20.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("32C000E6-3AB4-4488-83A8-CB205C51EDEF", "Guarantee");
		zTextBoxColumnStyleInfo20.ColumnName = "Guarantee+PW_BondNumber";
		zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
		zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("A164FDFF-A78D-42AC-B19D-290A35AE05AB", "Liability Amount");
		zCalcEditColumnStyleInfo3.ColumnName = "Guarantee+PW_BondAmount";
		zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
		zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("E5301378-2BBF-458B-940D-4B61A67ABCAC", "Liability Amount");
		zTextBoxColumnStyleInfo21.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("D61B1032-BF52-4EF5-961A-6EAA4BB52810", "Currency");
		zTextBoxColumnStyleInfo21.ColumnName = "Guarantee+PW_RX_NKCurrency";
		zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
		zTextBoxColumnStyleInfo21.GroupName = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("E5301378-2BBF-458B-940D-4B61A67ABCAC", "Liability Amount");
		zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("080bdaf1-a0d9-46ec-92c5-10eba5fbb1c0", "Created Time");
		zDateEditColumnStyleInfo2.ColumnName = "SRH_SystemCreateTimeUtc";
		zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("9308ae07-e901-4365-bad8-714fdefabaf2", "Creating User");
		zTextBoxColumnStyleInfo6.ColumnName = "SRH_SystemCreateUser";
		zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
		this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
		this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
		this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
		this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
		this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
		this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
		this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
		this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
		this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
		this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
		this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
		this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
		this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
		this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
		this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(804, 140, true);
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader);
		// 
		// HeaderAndLinesGridSplitContainer
		// 
		this.HeaderAndLinesGridSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 70, true);
		this.HeaderAndLinesGridSplitContainer.Name = "HeaderAndLinesGridSplitContainer";
		this.HeaderAndLinesGridSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
		// 
		// HeaderAndLinesGridSplitContainer.Panel1
		// 
		this.HeaderAndLinesGridSplitContainer.Panel1.Controls.Add(this.grid);
		this.HeaderAndLinesGridSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(804, 294, true);
		this.HeaderAndLinesGridSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(140);
		// 
		// HeaderAndLinesGridSplitContainer.Panel2
		// 
		this.HeaderAndLinesGridSplitContainer.Panel2.Controls.Add(this.LinesGrid);
		this.HeaderAndLinesGridSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(140);
		this.HeaderAndLinesGridSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(140);
		this.HeaderAndLinesGridSplitContainer.TabIndex = 7;
		// 
		// LinesGrid
		// 
		this.LinesGrid.AllowNavigation = false;
		this.BindingSource.SetBindingMember(this.LinesGrid, "CusTempStorageRegLines");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_LineNumber)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_OwnerReferenceType)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_OwnerReference)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_LocationOfGoods)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_GoodsDescription)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_LimitDate)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_CustodianIdentifier)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_GoodsOwnerIdentifier)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).PackagesRemainingCalculated)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_CustomsStatus)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_PackageMarks)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).GrossWeightRemainingCalculated)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_GrossWeightUQ)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).NumberOfItems)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).TSDItemNumbers)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).ItemCommodityCode)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).ItemGoodsDescription)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).BondAmountRemainingCalculated)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_UnionStatus)));
		this.LinesGrid.CaptionVisible = false;
		zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
		zCalcEditColumnStyleInfo1.ColumnName = "SRL_LineNumber";
		zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		zTextBoxColumnStyleInfo12.ColumnName = "SRL_LimitDate";
		zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("D979E14D-7272-4C98-8733-6B648E5F9641", "Owner EORI");
		zTextBoxColumnStyleInfo14.ColumnName = "SRL_GoodsOwnerIdentifier";
		zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("CE60D310-F2CC-4331-B0F6-DB51C9346932", "Number of Items");
		zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
		zCalcEditColumnStyleInfo5.ColumnName = "NumberOfItems";
		zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		zTextBoxColumnStyleInfo23.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("AA56113C-6709-4FD5-9D92-C75F36B1CD10", "TSD Item Nº");
		zTextBoxColumnStyleInfo23.ColumnName = "TSDItemNumbers";
		zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
		zTextBoxColumnStyleInfo24.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("6F2F9BF2-8CD4-4B6A-B8DA-C330E768F7B5", "Item Commodity Code");
		zTextBoxColumnStyleInfo24.ColumnName = "ItemCommodityCode";
		zTextBoxColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
		zTextBoxColumnStyleInfo25.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("69972580-A30B-4D88-A1A1-CEBF5A6AC73D", "Item Goods Description");
		zTextBoxColumnStyleInfo25.ColumnName = "ItemGoodsDescription";
		zTextBoxColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
		zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
		zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("8cd9252f-170c-4d19-9fba-93bcb05f0ed7", "Package Count");
		zCalcEditColumnStyleInfo2.ColumnName = "PackagesRemainingCalculated";
		zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("D64D0D8D-8BAF-43C7-925C-317967A67241", "Package Count");
		zTextBoxColumnStyleInfo26.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("4F1EFE8C-5E6D-4C4C-980A-0BEF8814393A", "Package Type");
		zTextBoxColumnStyleInfo26.ColumnName = "SRL_PackageType";
		zTextBoxColumnStyleInfo26.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		zTextBoxColumnStyleInfo26.GroupName = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("D64D0D8D-8BAF-43C7-925C-317967A67241", "Package Count");
		zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("8DBE7DB2-F1D1-4299-8F79-41DF3F07DD4E", "Package Marks/Vehicles");
		zTextBoxColumnStyleInfo16.ColumnName = "SRL_PackageMarks";
		zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
		zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("07D6000C-93CB-46A9-923A-56574D9A26A0", "GWT Count");
		zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
		zCalcEditColumnStyleInfo4.ColumnName = "GrossWeightRemainingCalculated";
		zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
		zCalcEditColumnStyleInfo4.GroupName = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("E5688930-EAB1-4EDE-B01D-7D92F7221E28", "GWT Count");
		zTextBoxColumnStyleInfo22.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("B523DB54-8C8C-47EE-BAC6-94F8B7AD8763", "GWT UQ");
		zTextBoxColumnStyleInfo22.ColumnName = "SRL_GrossWeightUQ";
		zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		zTextBoxColumnStyleInfo22.GroupName = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("E5688930-EAB1-4EDE-B01D-7D92F7221E28", "GWT Count");
		zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("d5a5be58-ca5e-4f6c-a1b9-a1d8e6ea0f9e", "Line Status");
		zTextBoxColumnStyleInfo15.ColumnName = "SRL_CustomsStatus";
		zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		zTextBoxColumnStyleInfo10.ColumnName = "SRL_LocationOfGoods";
		zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
		zTextBoxColumnStyleInfo9.ColumnName = "SRL_OwnerReference";
		zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
		zTextBoxColumnStyleInfo8.ColumnName = "SRL_OwnerReferenceType";
		zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
		zTextBoxColumnStyleInfo11.ColumnName = "SRL_GoodsDescription";
		zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("BA4B8032-5E24-4AE8-ADC3-8AD9ADE86146", "Liability Amount Remaining");
		zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
		zCalcEditColumnStyleInfo6.ColumnName = "BondAmountRemainingCalculated";
		zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
		zTextBoxColumnStyleInfo27.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("DDB523A0-72CE-46C3-9ED2-4ABBBD773D21", "Union Status");
		zTextBoxColumnStyleInfo27.ColumnName = "SRL_UnionStatus";
		zTextBoxColumnStyleInfo27.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.Module.Res.GetData("dd4f5362-d295-4dff-bd98-ae1027951fac", "Custodian EORI");
		zTextBoxColumnStyleInfo13.ColumnName = "SRL_CustodianIdentifier";
		zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		
		this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
		this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo24);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo25);
		this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo26);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
		this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
		this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo27);
		this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
		this.LinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.LinesGrid.GridId = "4787aa12-fac2-4699-bb4d-5e36370bab7c";
		this.LinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
		this.LinesGrid.LayoutKey = "LinesGrid";
		this.LinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.LinesGrid.Name = "LinesGrid";
		this.LinesGrid.ReadOnly = true;
		this.LinesGrid.ShouldSetErrorsOnTabPage = false;
		this.LinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(804, 150, true);
		this.LinesGrid.TabIndex = 0;
		// 
		// TemporaryStorageRegisterFilterStripControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.HeaderAndLinesGridSplitContainer);
		this.Name = "TemporaryStorageRegisterFilterStripControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 367, true);
		this.Controls.SetChildIndex(this.HeaderAndLinesGridSplitContainer, 0);
		this.Controls.SetChildIndex(this.ToolStripPermissionsLabel, 0);
		this.Controls.SetChildIndex(this.FilterStripsPanel, 0);
		this.Controls.SetChildIndex(this.AddStripButton, 0);
		this.Controls.SetChildIndex(this.ToolStripRecordsFoundLabel, 0);
		((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
		this.grid.ResumeLayout(false);
		this.grid.PerformLayout();
		this.AddStripButton.ResumeLayout(true);
		this.AddStripButton.PerformLayout();
		this.RecentItemsPanel.ResumeLayout(false);
		this.RecentItemsPanel.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.HeaderAndLinesGridSplitContainer.Panel1.ResumeLayout(false);
		this.HeaderAndLinesGridSplitContainer.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)(this.HeaderAndLinesGridSplitContainer)).EndInit();
		this.HeaderAndLinesGridSplitContainer.ResumeLayout(false);
		this.HeaderAndLinesGridSplitContainer.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
		this.LinesGrid.ResumeLayout(false);
		this.LinesGrid.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	public CargoWise.Windows.UI.KSplitContainer HeaderAndLinesGridSplitContainer;
	public ZDisplayGrid LinesGrid;
}

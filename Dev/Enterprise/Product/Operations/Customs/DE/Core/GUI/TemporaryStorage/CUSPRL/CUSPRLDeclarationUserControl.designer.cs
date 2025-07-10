using System.Windows.Forms;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.DE.GUI
{
	partial class CUSPRLDeclarationUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CUSPRLDeclarationUserControl));
			this.SumADeclarationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DeclarationAndLinesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DeclarationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CreatedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LineDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ExtendedLineDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ExtendedLineDetailsPanel3 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TransportDocumentPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TransportDocumentMasterGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransportDocumentMasterReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportDocumentMasterTypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransportDocumentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransportNumberTypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransportNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReferenceDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ProofOfUnionStatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.POUSReferenceNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.POUSReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ESumAGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntrySequenceNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EntryReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ItemDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DestinationPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GoodsLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FreezoneCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExtendedLineDetailsPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ExtendedLineDetailsPanel4 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CarrierGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CarrierDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.DisposalEntitledTraderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DisposalEntitledTraderAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.CustodianGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustodianUserControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ClassifactionKeyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OwnerReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OwnerReferenceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SumADeclarationPanel.SuspendLayout();
			this.DeclarationAndLinesPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
			this.LinesGrid.SuspendLayout();
			this.DeclarationPanel.SuspendLayout();
			this.CreatedDateEdit.SuspendLayout();
			this.LineDetailsPanel.SuspendLayout();
			this.ExtendedLineDetailsPanel.SuspendLayout();
			this.ExtendedLineDetailsPanel3.SuspendLayout();
			this.TransportDocumentPanel.SuspendLayout();
			this.TransportDocumentMasterGroupBox.SuspendLayout();
			this.TransportDocumentMasterTypeCodeFindBox.SuspendLayout();
			this.TransportDocumentGroupBox.SuspendLayout();
			this.TransportNumberTypeCodeFindBox.SuspendLayout();
			this.ReferenceDetailsPanel.SuspendLayout();
			this.ProofOfUnionStatusGroupBox.SuspendLayout();
			this.ESumAGroupBox.SuspendLayout();
			this.ItemDetailsGroupBox.SuspendLayout();
			this.GoodsLocationDropEdit.SuspendLayout();
			this.GoodsTypeDropEdit.SuspendLayout();
			this.ExtendedLineDetailsPanel2.SuspendLayout();
			this.ExtendedLineDetailsPanel4.SuspendLayout();
			this.CarrierGroupBox.SuspendLayout();
			this.CarrierDocAddressControl.SuspendLayout();
			this.DisposalEntitledTraderGroupBox.SuspendLayout();
			this.DisposalEntitledTraderAddressControl.SuspendLayout();
			this.CustodianGroupBox.SuspendLayout();
			this.CustodianUserControl.SuspendLayout();
			this.ClassifactionKeyGroupBox.SuspendLayout();
			this.OwnerReferenceTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader);
			// 
			// SumADeclarationPanel
			// 
			this.SumADeclarationPanel.Controls.Add(this.DeclarationAndLinesPanel);
			this.SumADeclarationPanel.Controls.Add(this.LineDetailsPanel);
			this.SumADeclarationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SumADeclarationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SumADeclarationPanel.Name = "SumADeclarationPanel";
			this.SumADeclarationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 649, true);
			this.SumADeclarationPanel.TabIndex = 0;
			// 
			// DeclarationAndLinesPanel
			// 
			this.DeclarationAndLinesPanel.Controls.Add(this.LinesGrid);
			this.DeclarationAndLinesPanel.Controls.Add(this.DeclarationPanel);
			this.DeclarationAndLinesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeclarationAndLinesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeclarationAndLinesPanel.Name = "DeclarationAndLinesPanel";
			this.DeclarationAndLinesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 410, true);
			this.DeclarationAndLinesPanel.TabIndex = 2;
			// 
			// LinesGrid
			// 
			this.LinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LinesGrid, "CUSPRLCusTempStorageDec.CusTempStorageLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_PackageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_PackageQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_RN_NKDepartureCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_UnionStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_CustomsStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).CustodianName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_CustodianIdentifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_CustodianIdentifierBranchNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_GoodsOwnerIdentifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_GoodsOwnerIdentifierBranchNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).ContainerNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).Receptacle)));
			this.LinesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zCalcEditColumnStyleInfo1.ColumnName = "TSL_LineNo";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo1.ColumnName = "TSL_GoodsDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "TSL_OwnerReferenceType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo2.ColumnName = "TSL_OwnerReferenceNumber";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(270);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "TSL_GrossWeight";
			zCalcEditColumnStyleInfo2.Decimals = 3;
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(141);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "TSL_PackageType";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(89);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "TSL_PackageQty";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "TSL_RN_NKDepartureCountry";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(111);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "TSL_UnionStatus";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(83);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "TSL_CustomsStatus";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "CustodianName";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(202);
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "TSL_CustodianIdentifier";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "TSL_CustodianIdentifierBranchNo";
			zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106);
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "TSL_GoodsOwnerIdentifier";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(121);
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "TSL_GoodsOwnerIdentifierBranchNo";
			zDropEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo7.CharacterCasing = CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "ContainerNumber";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CharacterCasing = CharacterCasing.Upper;
			zTextBoxColumnStyleInfo8.ColumnName = "Receptacle";
			zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.LinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.LinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinesGrid.GridId = "65f1cbfd-aaf0-4c42-9799-3a85507997fd";
			this.LinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LinesGrid.LayoutKey = "LinesGrid";
			this.LinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.LinesGrid.Name = "LinesGrid";
			this.LinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 378, true);
			this.LinesGrid.TabIndex = 1;
			// 
			// DeclarationPanel
			// 
			this.DeclarationPanel.Controls.Add(this.ReferenceNumberTextBox);
			this.DeclarationPanel.Controls.Add(this.StatusTextBox);
			this.DeclarationPanel.Controls.Add(this.CreatedDateEdit);
			this.DeclarationPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.DeclarationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeclarationPanel.Name = "DeclarationPanel";
			this.DeclarationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 32, true);
			this.DeclarationPanel.TabIndex = 0;
			// 
			// ReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "CUSPRLCusTempStorageDec.ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.ReferenceNumber)));
			this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 7, true);
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 20, true);
			this.ReferenceNumberTextBox.TabIndex = 0;
			// 
			// StatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusTextBox, "CUSPRLCusTempStorageDec.STH_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.STH_MessageStatus)));
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 7, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.StatusTextBox.TabIndex = 1;
			this.StatusTextBox.TabStop = false;
			// 
			// CreatedDateEdit
			// 
			this.CreatedDateEdit.AllowDrop = true;
			this.CreatedDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.CreatedDateEdit, "CUSPRLCusTempStorageDec.STH_SystemCreateTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.STH_SystemCreateTimeUtc)));
			this.CreatedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			this.CreatedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 7, true);
			this.CreatedDateEdit.Name = "CreatedDateEdit";
			this.CreatedDateEdit.TabIndex = 2;
			this.CreatedDateEdit.TabStop = false;
			// 
			// LineDetailsPanel
			// 
			this.LineDetailsPanel.Controls.Add(this.ExtendedLineDetailsPanel);
			this.LineDetailsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.LineDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 410, true);
			this.LineDetailsPanel.Name = "LineDetailsPanel";
			this.LineDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 239, true);
			this.LineDetailsPanel.TabIndex = 1;
			// 
			// ExtendedLineDetailsPanel
			// 
			this.ExtendedLineDetailsPanel.Controls.Add(this.ExtendedLineDetailsPanel3);
			this.ExtendedLineDetailsPanel.Controls.Add(this.ExtendedLineDetailsPanel2);
			this.ExtendedLineDetailsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ExtendedLineDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExtendedLineDetailsPanel.Name = "ExtendedLineDetailsPanel";
			this.ExtendedLineDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 239, true);
			this.ExtendedLineDetailsPanel.TabIndex = 0;
			// 
			// ExtendedLineDetailsPanel3
			// 
			this.ExtendedLineDetailsPanel3.Controls.Add(this.TransportDocumentPanel);
			this.ExtendedLineDetailsPanel3.Controls.Add(this.ReferenceDetailsPanel);
			this.ExtendedLineDetailsPanel3.Controls.Add(this.ItemDetailsGroupBox);
			this.ExtendedLineDetailsPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExtendedLineDetailsPanel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 0, true);
			this.ExtendedLineDetailsPanel3.Name = "ExtendedLineDetailsPanel3";
			this.ExtendedLineDetailsPanel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 239, true);
			this.ExtendedLineDetailsPanel3.TabIndex = 1;
			// 
			// TransportDocumentPanel
			// 
			this.TransportDocumentPanel.Controls.Add(this.TransportDocumentMasterGroupBox);
			this.TransportDocumentPanel.Controls.Add(this.TransportDocumentGroupBox);
			this.TransportDocumentPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.TransportDocumentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 175, true);
			this.TransportDocumentPanel.Name = "TransportDocumentPanel";
			this.TransportDocumentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 64, true);
			this.TransportDocumentPanel.TabIndex = 2;
			// 
			// TransportDocumentMasterGroupBox
			//
			this.TransportDocumentMasterGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("1217E35E-B840-4AB4-80EA-C3AA4ACE8EAE", "Transport Document Master");
			this.TransportDocumentMasterGroupBox.Controls.Add(this.TransportDocumentMasterReferenceNumberTextBox);
			this.TransportDocumentMasterGroupBox.Controls.Add(this.TransportDocumentMasterTypeCodeFindBox);
			this.TransportDocumentMasterGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportDocumentMasterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 0, true);
			this.TransportDocumentMasterGroupBox.Name = "TransportDocumentMasterGroupBox";
			this.TransportDocumentMasterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 64, true);
			this.TransportDocumentMasterGroupBox.TabIndex = 3;
			this.TransportDocumentMasterGroupBox.TabStop = false;
			// 
			// TransportDocumentMasterReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransportDocumentMasterReferenceNumberTextBox, "CUSPRLCusTempStorageDec.CusTempStorageLines.TransportDocumentMaster.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TransportDocumentMaster.CSI_ReferenceNumber)));
			this.TransportDocumentMasterReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 42, true);
			this.TransportDocumentMasterReferenceNumberTextBox.Name = "TransportDocumentMasterReferenceNumberTextBox";
			this.TransportDocumentMasterReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			this.TransportDocumentMasterReferenceNumberTextBox.TabIndex = 2;
			// 
			// TransportDocumentMasterTypeCodeFindBox
			// 
			this.TransportDocumentMasterTypeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportDocumentMasterTypeCodeFindBox, "CUSPRLCusTempStorageDec.CusTempStorageLines.TransportDocumentMaster.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TransportDocumentMaster.CSI_Code)));
			this.TransportDocumentMasterTypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 16, true);
			this.TransportDocumentMasterTypeCodeFindBox.Name = "TransportDocumentMasterTypeCodeFindBox";
			this.TransportDocumentMasterTypeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TransportDocumentMasterTypeCodeFindBox.ParentType = null;
			this.TransportDocumentMasterTypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.TransportDocumentMasterTypeCodeFindBox.TabIndex = 0;
			// 
			// TransportDocumentGroupBox
			//
			this.TransportDocumentGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("DC9F9C3B-8858-4805-BE96-E15159ED4850", "Transport Document");
			this.TransportDocumentGroupBox.Controls.Add(this.TransportNumberTypeCodeFindBox);
			this.TransportDocumentGroupBox.Controls.Add(this.TransportNumberTextBox);
			this.TransportDocumentGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.TransportDocumentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportDocumentGroupBox.Name = "TransportDocumentGroupBox";
			this.TransportDocumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 64, true);
			this.TransportDocumentGroupBox.TabIndex = 2;
			this.TransportDocumentGroupBox.TabStop = false;
			// 
			// TransportNumberTypeCodeFindBox
			// 
			this.TransportNumberTypeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportNumberTypeCodeFindBox, "CUSPRLCusTempStorageDec.CusTempStorageLines.TSL_TransportNumberType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_TransportNumberType)));
			this.TransportNumberTypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 16, true);
			this.TransportNumberTypeCodeFindBox.Name = "TransportNumberTypeCodeFindBox";
			this.TransportNumberTypeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TransportNumberTypeCodeFindBox.ParentType = null;
			this.TransportNumberTypeCodeFindBox.ShouldResize = false;
			this.TransportNumberTypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			this.TransportNumberTypeCodeFindBox.TabIndex = 0;
			// 
			// TransportNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransportNumberTextBox, "CUSPRLCusTempStorageDec.CusTempStorageLines.TSL_TransportNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_TransportNumber)));
			this.TransportNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 42, true);
			this.TransportNumberTextBox.Name = "TransportNumberTextBox";
			this.TransportNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			this.TransportNumberTextBox.TabIndex = 1;
			// 
			// ReferenceDetailsPanel
			// 
			this.ReferenceDetailsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ReferenceDetailsPanel.Controls.Add(this.ProofOfUnionStatusGroupBox);
			this.ReferenceDetailsPanel.Controls.Add(this.ESumAGroupBox);
			this.ReferenceDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 109, true);
			this.ReferenceDetailsPanel.Name = "ReferenceDetailsPanel";
			this.ReferenceDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 66, true);
			this.ReferenceDetailsPanel.TabIndex = 1;
			// 
			// ProofOfUnionStatusGroupBox
			//
			this.ProofOfUnionStatusGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("2ae69310-b1ad-4592-8137-46e58609fcdc", "Proof Of Union Status");
			this.ProofOfUnionStatusGroupBox.Controls.Add(this.POUSReferenceNumberCalcEdit);
			this.ProofOfUnionStatusGroupBox.Controls.Add(this.POUSReferenceNumberTextBox);
			this.ProofOfUnionStatusGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProofOfUnionStatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 0, true);
			this.ProofOfUnionStatusGroupBox.Name = "ProofOfUnionStatusGroupBox";
			this.ProofOfUnionStatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 66, true);
			this.ProofOfUnionStatusGroupBox.TabIndex = 1;
			this.ProofOfUnionStatusGroupBox.TabStop = false;
			// 
			// POUSReferenceNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.POUSReferenceNumberCalcEdit, "CUSPRLCusTempStorageDec.CusTempStorageLines.TSL_ReferenceNumber2Line");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_ReferenceNumber2Line)));
			this.POUSReferenceNumberCalcEdit.DecimalPlaces = 0;
			this.POUSReferenceNumberCalcEdit.Decimals = 0;
			this.POUSReferenceNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 39, true);
			this.POUSReferenceNumberCalcEdit.MaxValue = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.POUSReferenceNumberCalcEdit.Name = "POUSReferenceNumberCalcEdit";
			this.POUSReferenceNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.POUSReferenceNumberCalcEdit.TabIndex = 1;
			this.POUSReferenceNumberCalcEdit.Text = "0";
			this.POUSReferenceNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.POUSReferenceNumberCalcEdit.TrackDisposedAccess = true;
			// 
			// POUSReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.POUSReferenceNumberTextBox, "CUSPRLCusTempStorageDec.CusTempStorageLines.TSL_ReferenceNumber2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_ReferenceNumber2)));
			this.POUSReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 13, true);
			this.POUSReferenceNumberTextBox.Name = "POUSReferenceNumberTextBox";
			this.POUSReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.POUSReferenceNumberTextBox.TabIndex = 0;
			// 
			// ESumAGroupBox
			//
			this.ESumAGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("9d35a946-37a0-4db2-b17c-780e19160da8", "ESumA/EAS2");
			this.ESumAGroupBox.Controls.Add(this.EntrySequenceNumberCalcEdit);
			this.ESumAGroupBox.Controls.Add(this.EntryReferenceNumberTextBox);
			this.ESumAGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ESumAGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ESumAGroupBox.Name = "ESumAGroupBox";
			this.ESumAGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 66, true);
			this.ESumAGroupBox.TabIndex = 0;
			this.ESumAGroupBox.TabStop = false;
			// 
			// EntrySequenceNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.EntrySequenceNumberCalcEdit, "CUSPRLCusTempStorageDec.CusTempStorageLines.TSL_ReferenceNumberLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_ReferenceNumberLine)));
			this.EntrySequenceNumberCalcEdit.DecimalPlaces = 0;
			this.EntrySequenceNumberCalcEdit.Decimals = 0;
			this.EntrySequenceNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 39, true);
			this.EntrySequenceNumberCalcEdit.MaxValue = new decimal(new int[] {
            999,
            0,
            0,
            0});
			this.EntrySequenceNumberCalcEdit.Name = "EntrySequenceNumberCalcEdit";
			this.EntrySequenceNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.EntrySequenceNumberCalcEdit.TabIndex = 1;
			this.EntrySequenceNumberCalcEdit.Text = "0";
			this.EntrySequenceNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.EntrySequenceNumberCalcEdit.TrackDisposedAccess = true;
			// 
			// EntryReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryReferenceNumberTextBox, "CUSPRLCusTempStorageDec.CusTempStorageLines.TSL_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_ReferenceNumber)));
			this.EntryReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 13, true);
			this.EntryReferenceNumberTextBox.Name = "EntryReferenceNumberTextBox";
			this.EntryReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.EntryReferenceNumberTextBox.TabIndex = 0;
			// 
			// ItemDetailsGroupBox
			//
			this.ItemDetailsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("4d11bf70-5b73-4b64-818f-8a8d74055115", "Item Details");
			this.ItemDetailsGroupBox.Controls.Add(this.DestinationPlaceTextBox);
			this.ItemDetailsGroupBox.Controls.Add(this.GoodsLocationDropEdit);
			this.ItemDetailsGroupBox.Controls.Add(this.GoodsTypeDropEdit);
			this.ItemDetailsGroupBox.Controls.Add(this.FreezoneCheckBox);
			this.ItemDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ItemDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemDetailsGroupBox.Name = "ItemDetailsGroupBox";
			this.ItemDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 109, true);
			this.ItemDetailsGroupBox.TabIndex = 0;
			this.ItemDetailsGroupBox.TabStop = false;
			// 
			// DestinationPlaceTextBox
			// 
			this.BindingSource.SetBindingMember(this.DestinationPlaceTextBox, "CUSPRLCusTempStorageDec.CusTempStorageLines.TSL_DestinationPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_DestinationPlace)));
			this.DestinationPlaceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DestinationPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 80, true);
			this.DestinationPlaceTextBox.Name = "DestinationPlaceTextBox";
			this.DestinationPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 20, true);
			this.DestinationPlaceTextBox.TabIndex = 3;
			// 
			// GoodsLocationDropEdit
			// 
			this.GoodsLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsLocationDropEdit, "CUSPRLCusTempStorageDec.CusTempStorageLines.TSL_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_LocationOfGoods)));
			this.GoodsLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 59, true);
			this.GoodsLocationDropEdit.Name = "GoodsLocationDropEdit";
			this.GoodsLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 20, true);
			this.GoodsLocationDropEdit.TabIndex = 2;
			// 
			// GoodsTypeDropEdit
			// 
			this.GoodsTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsTypeDropEdit, "CUSPRLCusTempStorageDec.CusTempStorageLines.TSL_GoodsType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_GoodsType)));
			this.GoodsTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 38, true);
			this.GoodsTypeDropEdit.Name = "GoodsTypeDropEdit";
			this.GoodsTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 20, true);
			this.GoodsTypeDropEdit.TabIndex = 1;
			// 
			// FreezoneCheckBox
			// 
			this.BindingSource.SetBindingMember(this.FreezoneCheckBox, "CUSPRLCusTempStorageDec.CusTempStorageLines.TSL_IsFTZ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_IsFTZ)));
			this.FreezoneCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.FreezoneCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 18, true);
			this.FreezoneCheckBox.Name = "FreezoneCheckBox";
			this.FreezoneCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 19, true);
			this.FreezoneCheckBox.TabIndex = 0;
			this.FreezoneCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.FreezoneCheckBox.UseVisualStyleBackColor = true;
			// 
			// ExtendedLineDetailsPanel2
			// 
			this.ExtendedLineDetailsPanel2.Controls.Add(this.ExtendedLineDetailsPanel4);
			this.ExtendedLineDetailsPanel2.Controls.Add(this.ClassifactionKeyGroupBox);
			this.ExtendedLineDetailsPanel2.Dock = System.Windows.Forms.DockStyle.Left;
			this.ExtendedLineDetailsPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExtendedLineDetailsPanel2.Name = "ExtendedLineDetailsPanel2";
			this.ExtendedLineDetailsPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 239, true);
			this.ExtendedLineDetailsPanel2.TabIndex = 0;
			// 
			// ExtendedLineDetailsPanel4
			// 
			this.ExtendedLineDetailsPanel4.Controls.Add(this.CarrierGroupBox);
			this.ExtendedLineDetailsPanel4.Controls.Add(this.DisposalEntitledTraderGroupBox);
			this.ExtendedLineDetailsPanel4.Controls.Add(this.CustodianGroupBox);
			this.ExtendedLineDetailsPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExtendedLineDetailsPanel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 73, true);
			this.ExtendedLineDetailsPanel4.Name = "ExtendedLineDetailsPanel4";
			this.ExtendedLineDetailsPanel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 166, true);
			this.ExtendedLineDetailsPanel4.TabIndex = 1;
			// 
			// CarrierGroupBox
			//
			this.CarrierGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("74F805C4-2258-4C98-B6D4-9C59E44BD45C", "Carrier");
			this.CarrierGroupBox.Controls.Add(this.CarrierDocAddressControl);
			this.CarrierGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CarrierGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 96, true);
			this.CarrierGroupBox.Name = "CarrierGroupBox";
			this.CarrierGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 70, true);
			this.CarrierGroupBox.TabIndex = 1;
			this.CarrierGroupBox.TabStop = false;
			// 
			// CarrierDocAddressControl
			// 
			this.CarrierDocAddressControl.AddressValidationProcessCmdKey = null;
			this.CarrierDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierDocAddressControl, "CUSPRLCusTempStorageDec.CusTempStorageLines.CarrierDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).CarrierDocAddress)));
			this.CarrierDocAddressControl.BindToOrganisations = "CUSPRLCusTempStorageDec.CusTempStorageLines.Lookups.OrgHeaderCollection";
			this.CarrierDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.CarrierDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 17, true);
			this.CarrierDocAddressControl.Name = "CarrierDocAddressControl";
			this.CarrierDocAddressControl.ReadOnly = false;
			this.CarrierDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.CarrierDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.CarrierDocAddressControl.TabIndex = 0;
			this.CarrierDocAddressControl.ValidationJustForced = false;
			// 
			// DisposalEntitledTraderGroupBox
			//
			this.DisposalEntitledTraderGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("b4427312-6f17-44f3-9977-e5ecab9af448", "Disposal Entitled Trader");
			this.DisposalEntitledTraderGroupBox.Controls.Add(this.DisposalEntitledTraderAddressControl);
			this.DisposalEntitledTraderGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.DisposalEntitledTraderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 48, true);
			this.DisposalEntitledTraderGroupBox.Name = "DisposalEntitledTraderGroupBox";
			this.DisposalEntitledTraderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 48, true);
			this.DisposalEntitledTraderGroupBox.TabIndex = 1;
			this.DisposalEntitledTraderGroupBox.TabStop = false;
			// 
			// DisposalEntitledTraderAddressControl
			// 
			this.DisposalEntitledTraderAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DisposalEntitledTraderAddressControl, "CUSPRLCusTempStorageDec.CusTempStorageLines.TSL_OA_GoodsOwner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_OA_GoodsOwner)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DisposalEntitledTraderAddressControl, false);
			this.DisposalEntitledTraderAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 17, true);
			this.DisposalEntitledTraderAddressControl.Name = "DisposalEntitledTraderAddressControl";
			this.DisposalEntitledTraderAddressControl.PopupCaption = "";
			this.DisposalEntitledTraderAddressControl.ShowAddress = false;
			this.DisposalEntitledTraderAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.DisposalEntitledTraderAddressControl.TabIndex = 0;
			// 
			// CustodianGroupBox
			//
			this.CustodianGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("c148ab6c-f0eb-4dcf-bb8d-b87430fd4ee8", "Custodian");
			this.CustodianGroupBox.Controls.Add(this.CustodianUserControl);
			this.CustodianGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.CustodianGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustodianGroupBox.Name = "CustodianGroupBox";
			this.CustodianGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 48, true);
			this.CustodianGroupBox.TabIndex = 0;
			this.CustodianGroupBox.TabStop = false;
			// 
			// CustodianUserControl
			// 
			this.CustodianUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustodianUserControl, "CUSPRLCusTempStorageDec.CusTempStorageLines.TSL_OA_Custodian");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_OA_Custodian)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CustodianUserControl, false);
			this.CustodianUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 17, true);
			this.CustodianUserControl.Name = "CustodianUserControl";
			this.CustodianUserControl.PopupCaption = "";
			this.CustodianUserControl.ShowAddress = false;
			this.CustodianUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.CustodianUserControl.TabIndex = 0;
			// 
			// ClassifactionKeyGroupBox
			//
			this.ClassifactionKeyGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("e5b63dfb-7f9a-41e0-9da8-0e8aa31711e7", "Classification Key");
			this.ClassifactionKeyGroupBox.Controls.Add(this.OwnerReferenceNumberTextBox);
			this.ClassifactionKeyGroupBox.Controls.Add(this.OwnerReferenceTypeDropEdit);
			this.ClassifactionKeyGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ClassifactionKeyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClassifactionKeyGroupBox.Name = "ClassifactionKeyGroupBox";
			this.ClassifactionKeyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 73, true);
			this.ClassifactionKeyGroupBox.TabIndex = 0;
			this.ClassifactionKeyGroupBox.TabStop = false;
			// 
			// OwnerReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.OwnerReferenceNumberTextBox, "CUSPRLCusTempStorageDec.CusTempStorageLines.TSL_OwnerReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceNumber)));
			this.OwnerReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OwnerReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 39, true);
			this.OwnerReferenceNumberTextBox.Name = "OwnerReferenceNumberTextBox";
			this.OwnerReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.OwnerReferenceNumberTextBox.TabIndex = 1;
			// 
			// OwnerReferenceTypeDropEdit
			// 
			this.OwnerReferenceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OwnerReferenceTypeDropEdit, "CUSPRLCusTempStorageDec.CusTempStorageLines.TSL_OwnerReferenceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPRLCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceType)));
			this.OwnerReferenceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 13, true);
			this.OwnerReferenceTypeDropEdit.Name = "OwnerReferenceTypeDropEdit";
			this.OwnerReferenceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.OwnerReferenceTypeDropEdit.TabIndex = 0;
			// 
			// CUSPRLDeclarationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SumADeclarationPanel);
			this.Name = "CUSPRLDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 649, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SumADeclarationPanel.ResumeLayout(false);
			this.SumADeclarationPanel.PerformLayout();
			this.DeclarationAndLinesPanel.ResumeLayout(false);
			this.DeclarationAndLinesPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
			this.LinesGrid.ResumeLayout(false);
			this.LinesGrid.PerformLayout();
			this.DeclarationPanel.ResumeLayout(false);
			this.DeclarationPanel.PerformLayout();
			this.CreatedDateEdit.ResumeLayout(true);
			this.CreatedDateEdit.PerformLayout();
			this.LineDetailsPanel.ResumeLayout(false);
			this.LineDetailsPanel.PerformLayout();
			this.ExtendedLineDetailsPanel.ResumeLayout(false);
			this.ExtendedLineDetailsPanel.PerformLayout();
			this.ExtendedLineDetailsPanel3.ResumeLayout(false);
			this.ExtendedLineDetailsPanel3.PerformLayout();
			this.TransportDocumentPanel.ResumeLayout(false);
			this.TransportDocumentPanel.PerformLayout();
			this.TransportDocumentMasterGroupBox.ResumeLayout(false);
			this.TransportDocumentMasterGroupBox.PerformLayout();
			this.TransportDocumentMasterTypeCodeFindBox.ResumeLayout(true);
			this.TransportDocumentMasterTypeCodeFindBox.PerformLayout();
			this.TransportDocumentGroupBox.ResumeLayout(false);
			this.TransportDocumentGroupBox.PerformLayout();
			this.TransportNumberTypeCodeFindBox.ResumeLayout(true);
			this.TransportNumberTypeCodeFindBox.PerformLayout();
			this.ReferenceDetailsPanel.ResumeLayout(false);
			this.ReferenceDetailsPanel.PerformLayout();
			this.ProofOfUnionStatusGroupBox.ResumeLayout(false);
			this.ProofOfUnionStatusGroupBox.PerformLayout();
			this.ESumAGroupBox.ResumeLayout(false);
			this.ESumAGroupBox.PerformLayout();
			this.ItemDetailsGroupBox.ResumeLayout(false);
			this.ItemDetailsGroupBox.PerformLayout();
			this.GoodsLocationDropEdit.ResumeLayout(true);
			this.GoodsLocationDropEdit.PerformLayout();
			this.GoodsTypeDropEdit.ResumeLayout(true);
			this.GoodsTypeDropEdit.PerformLayout();
			this.ExtendedLineDetailsPanel2.ResumeLayout(false);
			this.ExtendedLineDetailsPanel2.PerformLayout();
			this.ExtendedLineDetailsPanel4.ResumeLayout(false);
			this.ExtendedLineDetailsPanel4.PerformLayout();
			this.CarrierGroupBox.ResumeLayout(false);
			this.CarrierGroupBox.PerformLayout();
			this.CarrierDocAddressControl.ResumeLayout(true);
			this.CarrierDocAddressControl.PerformLayout();
			this.DisposalEntitledTraderGroupBox.ResumeLayout(false);
			this.DisposalEntitledTraderGroupBox.PerformLayout();
			this.DisposalEntitledTraderAddressControl.ResumeLayout(true);
			this.DisposalEntitledTraderAddressControl.PerformLayout();
			this.CustodianGroupBox.ResumeLayout(false);
			this.CustodianGroupBox.PerformLayout();
			this.CustodianUserControl.ResumeLayout(true);
			this.CustodianUserControl.PerformLayout();
			this.ClassifactionKeyGroupBox.ResumeLayout(false);
			this.ClassifactionKeyGroupBox.PerformLayout();
			this.OwnerReferenceTypeDropEdit.ResumeLayout(true);
			this.OwnerReferenceTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel SumADeclarationPanel;
		private ZArchitecture.GUI.ZPanel LineDetailsPanel;
		private ZArchitecture.GUI.ZPanel ExtendedLineDetailsPanel;
		private ZArchitecture.GUI.ZGroupBox ClassifactionKeyGroupBox;
		private ZArchitecture.ZTextBox OwnerReferenceNumberTextBox;
		public ZArchitecture.GUI.ZDropEditWithFixedWidth OwnerReferenceTypeDropEdit;
		private ZArchitecture.GUI.ZPanel ExtendedLineDetailsPanel3;
		private ZArchitecture.GUI.ZGroupBox ESumAGroupBox;
		private ZArchitecture.GUI.ZGroupBox ItemDetailsGroupBox;
		private ZArchitecture.ZTextBox EntryReferenceNumberTextBox;
		private ZArchitecture.GUI.ZDropEdit GoodsLocationDropEdit;
		private ZArchitecture.GUI.ZDropEdit GoodsTypeDropEdit;
		private ZArchitecture.GUI.ZCheckBox FreezoneCheckBox;
		private ZArchitecture.ZTextBox DestinationPlaceTextBox;
		private ZArchitecture.ZCalcEdit EntrySequenceNumberCalcEdit;
		private ZArchitecture.GUI.ZPanel ExtendedLineDetailsPanel2;
		private ZArchitecture.GUI.ZPanel ExtendedLineDetailsPanel4;
		private ZArchitecture.GUI.ZGroupBox DisposalEntitledTraderGroupBox;
		private ZArchitecture.GUI.ZAddressControl DisposalEntitledTraderAddressControl;
		private ZArchitecture.GUI.ZGroupBox CustodianGroupBox;
		private ZArchitecture.GUI.ZAddressControl CustodianUserControl;
		private ZArchitecture.GUI.ZPanel DeclarationAndLinesPanel;
		private ZArchitecture.ZGrid LinesGrid;
		private ZArchitecture.GUI.ZPanel DeclarationPanel;
		private ZArchitecture.ZTextBox ReferenceNumberTextBox;
		private ZArchitecture.ZTextBox StatusTextBox;
		private ZArchitecture.GUI.ZDateEdit CreatedDateEdit;
		private ZArchitecture.GUI.ZPanel ReferenceDetailsPanel;
		private ZArchitecture.GUI.ZGroupBox ProofOfUnionStatusGroupBox;
		private ZArchitecture.ZCalcEdit POUSReferenceNumberCalcEdit;
		private ZArchitecture.ZTextBox POUSReferenceNumberTextBox;
		private ZArchitecture.GUI.ZGroupBox TransportDocumentGroupBox;
		private ZArchitecture.ZTextBox TransportNumberTextBox;
		private ZArchitecture.GUI.ZCodeFindBox TransportNumberTypeCodeFindBox;
		private ZArchitecture.GUI.ZGroupBox CarrierGroupBox;
		private ZDocAddressControl CarrierDocAddressControl;
		private ZArchitecture.GUI.ZPanel TransportDocumentPanel;
		private ZArchitecture.GUI.ZGroupBox TransportDocumentMasterGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox TransportDocumentMasterTypeCodeFindBox;
		private ZArchitecture.ZTextBox TransportDocumentMasterReferenceNumberTextBox;
	}
}

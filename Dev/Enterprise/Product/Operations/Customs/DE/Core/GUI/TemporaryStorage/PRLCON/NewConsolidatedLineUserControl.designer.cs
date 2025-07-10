namespace Enterprise.Customs.DE.GUI
{
	partial class NewConsolidatedLineUserControl
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
			this.ConsolidationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ItemDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CountryOfDepartureFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DestinationPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GoodsLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UnionStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ConsolidatedLineDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DisposalEntitledTraderAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DisposalEntitledTraderBranchNoDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GrossWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CustodianUserControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.PackageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DisposalEntitledTraderEORITextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackageQtyZCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustodianBranchNoDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustodianEORITextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClassifactionKeyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OwnerReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OwnerReferenceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConsolidationPanel.SuspendLayout();
			this.ItemDetailsGroupBox.SuspendLayout();
			this.CountryOfDepartureFindBox.SuspendLayout();
			this.GoodsLocationDropEdit.SuspendLayout();
			this.UnionStatusDropEdit.SuspendLayout();
			this.GoodsTypeDropEdit.SuspendLayout();
			this.ConsolidatedLineDetailsGroupBox.SuspendLayout();
			this.DisposalEntitledTraderAddressControl.SuspendLayout();
			this.DisposalEntitledTraderBranchNoDropEdit.SuspendLayout();
			this.CustodianUserControl.SuspendLayout();
			this.PackageTypeDropEdit.SuspendLayout();
			this.CustodianBranchNoDropEdit.SuspendLayout();
			this.ClassifactionKeyGroupBox.SuspendLayout();
			this.OwnerReferenceTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec);
			// 
			// ConsolidationPanel
			// 
			this.ConsolidationPanel.Controls.Add(this.ItemDetailsGroupBox);
			this.ConsolidationPanel.Controls.Add(this.ConsolidatedLineDetailsGroupBox);
			this.ConsolidationPanel.Controls.Add(this.ClassifactionKeyGroupBox);
			this.ConsolidationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolidationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsolidationPanel.Name = "ConsolidationPanel";
			this.ConsolidationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 290, true);
			this.ConsolidationPanel.TabIndex = 0;
			// 
			// ItemDetailsGroupBox
			// 
			this.ItemDetailsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("55a45c5c-d4b0-4270-80b7-87692284321c", "Item Details");
			this.ItemDetailsGroupBox.Controls.Add(this.CountryOfDepartureFindBox);
			this.ItemDetailsGroupBox.Controls.Add(this.DestinationPlaceTextBox);
			this.ItemDetailsGroupBox.Controls.Add(this.GoodsLocationDropEdit);
			this.ItemDetailsGroupBox.Controls.Add(this.UnionStatusDropEdit);
			this.ItemDetailsGroupBox.Controls.Add(this.GoodsTypeDropEdit);
			this.ItemDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ItemDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 188, true);
			this.ItemDetailsGroupBox.Name = "ItemDetailsGroupBox";
			this.ItemDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 92, true);
			this.ItemDetailsGroupBox.TabIndex = 2;
			this.ItemDetailsGroupBox.TabStop = false;
			// 
			// CountryOfDepartureFindBox
			// 
			this.CountryOfDepartureFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfDepartureFindBox, "ConsolidatedLine+TSL_RN_NKDepartureCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).ConsolidatedLine.TSL_RN_NKDepartureCountry)));
			this.CountryOfDepartureFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(565, 17, true);
			this.CountryOfDepartureFindBox.Name = "CountryOfDepartureFindBox";
			this.CountryOfDepartureFindBox.PreBoundMaxLength = 2;
			this.CountryOfDepartureFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CountryOfDepartureFindBox.TabIndex = 3;
			// 
			// DestinationPlaceTextBox
			// 
			this.BindingSource.SetBindingMember(this.DestinationPlaceTextBox, "ConsolidatedLine+TSL_DestinationPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).ConsolidatedLine.TSL_DestinationPlace)));
			this.DestinationPlaceTextBox.CaptionResourceString = null;
			this.DestinationPlaceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DestinationPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(565, 40, true);
			this.DestinationPlaceTextBox.Name = "DestinationPlaceTextBox";
			this.DestinationPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.DestinationPlaceTextBox.TabIndex = 4;
			// 
			// GoodsLocationDropEdit
			// 
			this.GoodsLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsLocationDropEdit, "ConsolidatedLine+TSL_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).ConsolidatedLine.TSL_LocationOfGoods)));
			this.GoodsLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 40, true);
			this.GoodsLocationDropEdit.Name = "GoodsLocationDropEdit";
			this.GoodsLocationDropEdit.ShouldResizeByMaxLength = true;
			this.GoodsLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.GoodsLocationDropEdit.TabIndex = 1;
			// 
			// UnionStatusDropEdit
			// 
			this.UnionStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnionStatusDropEdit, "ConsolidatedLine+TSL_UnionStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).ConsolidatedLine.TSL_UnionStatus)));
			this.UnionStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 63, true);
			this.UnionStatusDropEdit.Name = "UnionStatusDropEdit";
			this.UnionStatusDropEdit.PreBoundMaxLength = 1;
			this.UnionStatusDropEdit.ShouldResizeByMaxLength = true;
			this.UnionStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.UnionStatusDropEdit.TabIndex = 2;
			// 
			// GoodsTypeDropEdit
			// 
			this.GoodsTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsTypeDropEdit, "ConsolidatedLine+TSL_GoodsType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).ConsolidatedLine.TSL_GoodsType)));
			this.GoodsTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 17, true);
			this.GoodsTypeDropEdit.Name = "GoodsTypeDropEdit";
			this.GoodsTypeDropEdit.PreBoundMaxLength = 1;
			this.GoodsTypeDropEdit.ShouldResizeByMaxLength = true;
			this.GoodsTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.GoodsTypeDropEdit.TabIndex = 0;
			// 
			// ConsolidatedLineDetailsGroupBox
			// 
			this.ConsolidatedLineDetailsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("3c428280-5b01-479b-8cbc-2ab175e35b49", "Consolidated Line Details");
			this.ConsolidatedLineDetailsGroupBox.Controls.Add(this.DisposalEntitledTraderAddressControl);
			this.ConsolidatedLineDetailsGroupBox.Controls.Add(this.DisposalEntitledTraderBranchNoDropEdit);
			this.ConsolidatedLineDetailsGroupBox.Controls.Add(this.CustomsStatusTextBox);
			this.ConsolidatedLineDetailsGroupBox.Controls.Add(this.GrossWeightCalcEdit);
			this.ConsolidatedLineDetailsGroupBox.Controls.Add(this.CustodianUserControl);
			this.ConsolidatedLineDetailsGroupBox.Controls.Add(this.PackageTypeDropEdit);
			this.ConsolidatedLineDetailsGroupBox.Controls.Add(this.DisposalEntitledTraderEORITextBox);
			this.ConsolidatedLineDetailsGroupBox.Controls.Add(this.PackageQtyZCalcEdit);
			this.ConsolidatedLineDetailsGroupBox.Controls.Add(this.GoodsDescriptionTextBox);
			this.ConsolidatedLineDetailsGroupBox.Controls.Add(this.CustodianBranchNoDropEdit);
			this.ConsolidatedLineDetailsGroupBox.Controls.Add(this.CustodianEORITextBox);
			this.ConsolidatedLineDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ConsolidatedLineDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 49, true);
			this.ConsolidatedLineDetailsGroupBox.Name = "ConsolidatedLineDetailsGroupBox";
			this.ConsolidatedLineDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 139, true);
			this.ConsolidatedLineDetailsGroupBox.TabIndex = 1;
			this.ConsolidatedLineDetailsGroupBox.TabStop = false;
			// 
			// DisposalEntitledTraderAddressControl
			// 
			this.DisposalEntitledTraderAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DisposalEntitledTraderAddressControl, "ConsolidatedLine+TSL_OA_GoodsOwner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).ConsolidatedLine.TSL_OA_GoodsOwner)));
			this.DisposalEntitledTraderAddressControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("32078422-6d54-408b-b3c4-8e4a63b5e1c3", "Entitled Trader Address");
			this.DisposalEntitledTraderAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(565, 42, true);
			this.DisposalEntitledTraderAddressControl.Name = "DisposalEntitledTraderAddressControl";
			this.DisposalEntitledTraderAddressControl.PopupCaption = "";
			this.DisposalEntitledTraderAddressControl.ReadOnly = false;
			this.DisposalEntitledTraderAddressControl.ShowAddress = false;
			this.DisposalEntitledTraderAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.DisposalEntitledTraderAddressControl.TabIndex = 7;
			// 
			// DisposalEntitledTraderBranchNoDropEdit
			// 
			this.DisposalEntitledTraderBranchNoDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DisposalEntitledTraderBranchNoDropEdit, "ConsolidatedLine+TSL_GoodsOwnerIdentifierBranchNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).ConsolidatedLine.TSL_GoodsOwnerIdentifierBranchNo)));
			this.DisposalEntitledTraderBranchNoDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(795, 65, true);
			this.DisposalEntitledTraderBranchNoDropEdit.Name = "DisposalEntitledTraderBranchNoDropEdit";
			this.DisposalEntitledTraderBranchNoDropEdit.PreBoundMaxLength = 4;
			this.DisposalEntitledTraderBranchNoDropEdit.ShouldResizeByMaxLength = true;
			this.DisposalEntitledTraderBranchNoDropEdit.ShowDescriptionBox = false;
			this.DisposalEntitledTraderBranchNoDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.DisposalEntitledTraderBranchNoDropEdit.TabIndex = 9;
			// 
			// CustomsStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsStatusTextBox, "ConsolidatedLine+TSL_CustomsStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).ConsolidatedLine.TSL_CustomsStatus)));
			this.CustomsStatusTextBox.CaptionResourceString = null;
			this.CustomsStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(565, 88, true);
			this.CustomsStatusTextBox.Name = "CustomsStatusTextBox";
			this.CustomsStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CustomsStatusTextBox.TabIndex = 10;
			// 
			// GrossWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightCalcEdit, "ConsolidatedLine+TSL_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).ConsolidatedLine.TSL_GrossWeight)));
			this.GrossWeightCalcEdit.CaptionResourceString = null;
			this.GrossWeightCalcEdit.DecimalPlaces = 2;
			this.GrossWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 111, true);
			this.GrossWeightCalcEdit.Name = "GrossWeightCalcEdit";
			this.GrossWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 20, true);
			this.GrossWeightCalcEdit.TabIndex = 6;
			this.GrossWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CustodianUserControl
			// 
			this.CustodianUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustodianUserControl, "ConsolidatedLine+TSL_OA_Custodian");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).ConsolidatedLine.TSL_OA_Custodian)));
			this.CustodianUserControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("3d6c2457-de37-4e25-b380-74eebd0b1329", "Custodian Address");
			this.CustodianUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 42, true);
			this.CustodianUserControl.Name = "CustodianUserControl";
			this.CustodianUserControl.PopupCaption = "";
			this.CustodianUserControl.ReadOnly = false;
			this.CustodianUserControl.ShowAddress = false;
			this.CustodianUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CustodianUserControl.TabIndex = 1;
			// 
			// PackageTypeDropEdit
			// 
			this.PackageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackageTypeDropEdit, "ConsolidatedLine+TSL_PackageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).ConsolidatedLine.TSL_PackageType)));
			this.PackageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 88, true);
			this.PackageTypeDropEdit.Name = "PackageTypeDropEdit";
			this.PackageTypeDropEdit.PreBoundMaxLength = 2;
			this.PackageTypeDropEdit.ShouldResizeByMaxLength = true;
			this.PackageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 20, true);
			this.PackageTypeDropEdit.TabIndex = 4;
			// 
			// DisposalEntitledTraderEORITextBox
			// 
			this.BindingSource.SetBindingMember(this.DisposalEntitledTraderEORITextBox, "ConsolidatedLine+TSL_GoodsOwnerIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).ConsolidatedLine.TSL_GoodsOwnerIdentifier)));
			this.DisposalEntitledTraderEORITextBox.CaptionResourceString = null;
			this.DisposalEntitledTraderEORITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(565, 65, true);
			this.DisposalEntitledTraderEORITextBox.Name = "DisposalEntitledTraderEORITextBox";
			this.DisposalEntitledTraderEORITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.DisposalEntitledTraderEORITextBox.TabIndex = 8;
			// 
			// PackageQtyZCalcEdit
			// 
			this.PackageQtyZCalcEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackageQtyZCalcEdit, "ConsolidatedLine+TSL_PackageQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).ConsolidatedLine.TSL_PackageQty)));
			this.PackageQtyZCalcEdit.CaptionResourceString = null;
			this.PackageQtyZCalcEdit.DecimalPlaces = 2;
			this.PackageQtyZCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(363, 88, true);
			this.PackageQtyZCalcEdit.Name = "PackageQtyZCalcEdit";
			this.PackageQtyZCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.PackageQtyZCalcEdit.TabIndex = 5;
			this.PackageQtyZCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GoodsDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "ConsolidatedLine+TSL_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).ConsolidatedLine.TSL_GoodsDescription)));
			this.GoodsDescriptionTextBox.CaptionResourceString = null;
			this.GoodsDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 19, true);
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 20, true);
			this.GoodsDescriptionTextBox.TabIndex = 0;
			// 
			// CustodianBranchNoDropEdit
			// 
			this.CustodianBranchNoDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustodianBranchNoDropEdit, "ConsolidatedLine+TSL_CustodianIdentifierBranchNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).ConsolidatedLine.TSL_CustodianIdentifierBranchNo)));
			this.CustodianBranchNoDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(363, 65, true);
			this.CustodianBranchNoDropEdit.Name = "CustodianBranchNoDropEdit";
			this.CustodianBranchNoDropEdit.PreBoundMaxLength = 4;
			this.CustodianBranchNoDropEdit.ShouldResizeByMaxLength = true;
			this.CustodianBranchNoDropEdit.ShowDescriptionBox = false;
			this.CustodianBranchNoDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.CustodianBranchNoDropEdit.TabIndex = 3;
			// 
			// CustodianEORITextBox
			// 
			this.BindingSource.SetBindingMember(this.CustodianEORITextBox, "ConsolidatedLine+TSL_CustodianIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).ConsolidatedLine.TSL_CustodianIdentifier)));
			this.CustodianEORITextBox.CaptionResourceString = null;
			this.CustodianEORITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 65, true);
			this.CustodianEORITextBox.Name = "CustodianEORITextBox";
			this.CustodianEORITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.CustodianEORITextBox.TabIndex = 2;
			// 
			// ClassifactionKeyGroupBox
			// 
			this.ClassifactionKeyGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("99861579-f720-4751-b092-88fc25d7826a", "Classification Key");
			this.ClassifactionKeyGroupBox.Controls.Add(this.OwnerReferenceNumberTextBox);
			this.ClassifactionKeyGroupBox.Controls.Add(this.OwnerReferenceTypeDropEdit);
			this.ClassifactionKeyGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ClassifactionKeyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClassifactionKeyGroupBox.Name = "ClassifactionKeyGroupBox";
			this.ClassifactionKeyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 49, true);
			this.ClassifactionKeyGroupBox.TabIndex = 0;
			this.ClassifactionKeyGroupBox.TabStop = false;
			// 
			// OwnerReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.OwnerReferenceNumberTextBox, "ConsolidatedLine+TSL_OwnerReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).ConsolidatedLine.TSL_OwnerReferenceNumber)));
			this.OwnerReferenceNumberTextBox.CaptionResourceString = null;
			this.OwnerReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OwnerReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(565, 17, true);
			this.OwnerReferenceNumberTextBox.Name = "OwnerReferenceNumberTextBox";
			this.OwnerReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.OwnerReferenceNumberTextBox.TabIndex = 1;
			// 
			// OwnerReferenceTypeDropEdit
			// 
			this.OwnerReferenceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OwnerReferenceTypeDropEdit, "ConsolidatedLine+TSL_OwnerReferenceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(null)).ConsolidatedLine.TSL_OwnerReferenceType)));
			this.OwnerReferenceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 17, true);
			this.OwnerReferenceTypeDropEdit.Name = "OwnerReferenceTypeDropEdit";
			this.OwnerReferenceTypeDropEdit.ShouldResizeByMaxLength = true;
			this.OwnerReferenceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.OwnerReferenceTypeDropEdit.TabIndex = 0;
			// 
			// NewConsolidatedLineUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ConsolidationPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 290, true);
			this.Name = "NewConsolidatedLineUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 290, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConsolidationPanel.ResumeLayout(false);
			this.ConsolidationPanel.PerformLayout();
			this.ItemDetailsGroupBox.ResumeLayout(false);
			this.ItemDetailsGroupBox.PerformLayout();
			this.CountryOfDepartureFindBox.ResumeLayout(true);
			this.CountryOfDepartureFindBox.PerformLayout();
			this.GoodsLocationDropEdit.ResumeLayout(true);
			this.GoodsLocationDropEdit.PerformLayout();
			this.UnionStatusDropEdit.ResumeLayout(true);
			this.UnionStatusDropEdit.PerformLayout();
			this.GoodsTypeDropEdit.ResumeLayout(true);
			this.GoodsTypeDropEdit.PerformLayout();
			this.ConsolidatedLineDetailsGroupBox.ResumeLayout(false);
			this.ConsolidatedLineDetailsGroupBox.PerformLayout();
			this.DisposalEntitledTraderAddressControl.ResumeLayout(true);
			this.DisposalEntitledTraderAddressControl.PerformLayout();
			this.DisposalEntitledTraderBranchNoDropEdit.ResumeLayout(true);
			this.DisposalEntitledTraderBranchNoDropEdit.PerformLayout();
			this.CustodianUserControl.ResumeLayout(true);
			this.CustodianUserControl.PerformLayout();
			this.PackageTypeDropEdit.ResumeLayout(true);
			this.PackageTypeDropEdit.PerformLayout();
			this.CustodianBranchNoDropEdit.ResumeLayout(true);
			this.CustodianBranchNoDropEdit.PerformLayout();
			this.ClassifactionKeyGroupBox.ResumeLayout(false);
			this.ClassifactionKeyGroupBox.PerformLayout();
			this.OwnerReferenceTypeDropEdit.ResumeLayout(true);
			this.OwnerReferenceTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel ConsolidationPanel;
		private ZArchitecture.GUI.ZAddressControl DisposalEntitledTraderAddressControl;
		private ZArchitecture.GUI.ZAddressControl CustodianUserControl;
		private ZArchitecture.GUI.ZGroupBox ClassifactionKeyGroupBox;
		private ZArchitecture.ZTextBox OwnerReferenceNumberTextBox;
		public ZArchitecture.GUI.ZDropEditWithFixedWidth OwnerReferenceTypeDropEdit;
		private ZArchitecture.GUI.ZGroupBox ItemDetailsGroupBox;
		private ZArchitecture.ZTextBox DestinationPlaceTextBox;
		private ZArchitecture.GUI.ZDropEdit GoodsLocationDropEdit;
		private ZArchitecture.GUI.ZDropEdit GoodsTypeDropEdit;
		private ZArchitecture.ZTextBox GoodsDescriptionTextBox;
		private ZArchitecture.ZCalcEdit GrossWeightCalcEdit;
		private ZArchitecture.ZCalcEdit PackageQtyZCalcEdit;
		private ZArchitecture.GUI.ZDropEdit PackageTypeDropEdit;
		private ZArchitecture.ZTextBox CustomsStatusTextBox;
		private ZArchitecture.GUI.ZCodeFindBox CountryOfDepartureFindBox;
		private ZArchitecture.GUI.ZDropEdit UnionStatusDropEdit;
		private ZArchitecture.ZTextBox CustodianEORITextBox;
		private ZArchitecture.ZTextBox DisposalEntitledTraderEORITextBox;
		private ZArchitecture.GUI.ZDropEdit DisposalEntitledTraderBranchNoDropEdit;
		private ZArchitecture.GUI.ZDropEdit CustodianBranchNoDropEdit;
		private ZArchitecture.GUI.ZGroupBox ConsolidatedLineDetailsGroupBox;
	}
}

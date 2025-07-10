namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class InvoiceLineDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CommentsLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
			this.OriginLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
			this.DescriptionLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
			this.WineCountryOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OperationCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.WineCategoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GrowingZoneDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OperationCodesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomsQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.SizeOfProducerCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DensityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DegreePlatoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BrandNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.WeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.FiscalMarkUserControl = new Enterprise.Customs.EU.EMCS.GUI.InvoiceLineFiscalMarkUserControl();
			this.AlcoholicStrengthUserControl = new Enterprise.Customs.EU.EMCS.GUI.InvoiceLineAlcoholicStrengthUserControl();
			this.TariffCodeFindBox = new Enterprise.Customs.GUI.TariffFindBox();
			this.ExciseProductCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ProductCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.LineNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IsMainPackCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WineDetailsSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.MaturationPeriodOrAgeOfProductsWordWrappingTextBox = new Enterprise.Customs.GUI.WordWrappingTextBox();
			this.IndependentSmallProducersDeclarationWordWrappingTextBox = new Enterprise.Customs.GUI.WordWrappingTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CommentsLongTextControl.SuspendLayout();
			this.OriginLongTextControl.SuspendLayout();
			this.DescriptionLongTextControl.SuspendLayout();
			this.WineCountryOriginCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OperationCodesGrid)).BeginInit();
			this.OperationCodesGrid.SuspendLayout();
			this.WineCategoryDropEdit.SuspendLayout();
			this.GrowingZoneDropEdit.SuspendLayout();
			this.OperationCodesGroupBox.SuspendLayout();
			this.CustomsQuantityCalcDropEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.WeightCalcDropEdit.SuspendLayout();
			this.FiscalMarkUserControl.SuspendLayout();
			this.AlcoholicStrengthUserControl.SuspendLayout();
			this.TariffCodeFindBox.SuspendLayout();
			this.ExciseProductCodeDropEdit.SuspendLayout();
			this.ProductCodeFindBox.SuspendLayout();
			this.WineDetailsSeparatorUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine);
			// 
			// CommentsLongTextControl
			// 
			this.CommentsLongTextControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(CommentsLongTextControl, "JI_WineDetailsComments");
			this.CommentsLongTextControl.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("1d0b9979-2a2c-46f6-a737-ecd7b224fcb0", "Comments");
			this.CommentsLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CommentsLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 115, true);
			this.CommentsLongTextControl.Name = "CommentsLongTextControl";
			this.CommentsLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 21, true);
			this.CommentsLongTextControl.TabIndex = 20;
			// 
			// OriginLongTextControl
			// 
			this.OriginLongTextControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginLongTextControl, "ZG_Origin");
			this.OriginLongTextControl.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("5d85f0bd-3d0e-4c31-a623-7e8cb36856ad", "Origin");
			this.OriginLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OriginLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 14, true);
			this.OriginLongTextControl.Name = "OriginLongTextControl";
			this.OriginLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.OriginLongTextControl.TabIndex = 0;
			// 
			// DescriptionLongTextControl
			// 
			this.DescriptionLongTextControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DescriptionLongTextControl, "JI_NDescription");
			this.DescriptionLongTextControl.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("a22d87d1-d1de-4f9e-b953-eb5baa34d13b", "Description");
			this.DescriptionLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 36, true);
			this.DescriptionLongTextControl.Name = "DescriptionLongTextControl";
			this.DescriptionLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 21, true);
			this.DescriptionLongTextControl.TabIndex = 1;
			// 
			// WineCountryOriginCodeFindBox
			// 
			this.WineCountryOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WineCountryOriginCodeFindBox, "ZG_WineCountryOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).ZG_WineCountryOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Lookups.CountryList)));
			this.WineCountryOriginCodeFindBox.BindToList = "Lookups+CountryList";
			this.WineCountryOriginCodeFindBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("29beac76-034f-4089-838e-81dc2077bc66", "Ctry./Reg. Or.", "Ctry./Reg. Origin", "Country/Region Origin");
			this.WineCountryOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 37, true);
			this.WineCountryOriginCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.WineCountryOriginCodeFindBox.Name = "WineCountryOriginCodeFindBox";
			this.WineCountryOriginCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.WineCountryOriginCodeFindBox.ParentType = null;
			this.WineCountryOriginCodeFindBox.PreBoundMaxLength = 2;
			this.WineCountryOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.WineCountryOriginCodeFindBox.TabIndex = 17;
			// 
			// OperationCodesGrid
			// 
			this.OperationCodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OperationCodesGrid, "OperationCodeDataCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).OperationCodeDataCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.WineCodeData)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).OperationCodeDataCollection)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.WineCodeData)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).OperationCodeDataCollection)).SyncRoot)).CY_Description)));
			this.OperationCodesGrid.CaptionText = "Operation Codes";
			this.OperationCodesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("3c882cf8-03d5-46d0-bb7b-56c2664ef07e", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "CY_Description";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(210);
			this.OperationCodesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.OperationCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OperationCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OperationCodesGrid.GridId = "9cbba5e3-bc63-4b30-96d1-6eac78a0e860";
			this.OperationCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OperationCodesGrid.LayoutKey = "OperationCodesGrid";
			this.OperationCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OperationCodesGrid.Name = "OperationCodesGrid";
			this.OperationCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 166, true);
			this.OperationCodesGrid.TabIndex = 21;
			// 
			// WineCategoryDropEdit
			// 
			this.WineCategoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WineCategoryDropEdit, "ZG_WineCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).ZG_WineCategory)));
			this.WineCategoryDropEdit.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("765def89-5ac7-4abd-85a1-aa557bb29e65", "Wine Category");
			this.WineCategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 89, true);
			this.WineCategoryDropEdit.Name = "WineCategoryDropEdit";
			this.WineCategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.WineCategoryDropEdit.TabIndex = 19;
			// 
			// GrowingZoneDropEdit
			// 
			this.GrowingZoneDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrowingZoneDropEdit, "ZG_GrowingZone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).ZG_GrowingZone)));
			this.GrowingZoneDropEdit.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("36a50f82-0ac9-4d57-8e3a-a977fff1b353", "Growing Zone");
			this.GrowingZoneDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 63, true);
			this.GrowingZoneDropEdit.Name = "GrowingZoneDropEdit";
			this.GrowingZoneDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.GrowingZoneDropEdit.TabIndex = 18;
			// 
			// OperationCodesGroupBox
			// 
			this.OperationCodesGroupBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("40f89b58-1fae-4951-b511-8825f7d91e43", "Operation Codes");
			this.OperationCodesGroupBox.Controls.Add(this.OperationCodesGrid);
			this.OperationCodesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 151, true);
			this.OperationCodesGroupBox.Name = "OperationCodesGroupBox";
			this.OperationCodesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 185, true);
			this.OperationCodesGroupBox.TabIndex = 21;
			this.OperationCodesGroupBox.TabStop = false;
			// 
			// CustomsQuantityCalcDropEdit
			// 
			this.CustomsQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_CustomsQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_CustomsUnitQty)));
			this.CustomsQuantityCalcDropEdit.BindToAmount = "JI_CustomsQuantity";
			this.CustomsQuantityCalcDropEdit.BindToUnit = "JI_CustomsUnitQty";
			this.CustomsQuantityCalcDropEdit.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("964a0c11-d160-4218-b535-1eacd6b1cb61", "Customs Quantity");
			this.CustomsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 173, true);
			this.CustomsQuantityCalcDropEdit.Name = "CustomsQuantityCalcDropEdit";
			this.CustomsQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.CustomsQuantityCalcDropEdit.TabIndex = 7;
			this.CustomsQuantityCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// SizeOfProducerCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SizeOfProducerCalcEdit, "ZG_SizeOfProducer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).ZG_SizeOfProducer)));
			this.SizeOfProducerCalcEdit.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("a2c7a4fb-d3c2-4628-99c4-66443b084995", "Size Of Producer");
			this.SizeOfProducerCalcEdit.DecimalPlaces = 0;
			this.SizeOfProducerCalcEdit.Decimals = 0;
			this.SizeOfProducerCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 307, true);
			this.SizeOfProducerCalcEdit.Name = "SizeOfProducerCalcEdit";
			this.SizeOfProducerCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.SizeOfProducerCalcEdit.TabIndex = 13;
			this.SizeOfProducerCalcEdit.Text = "0";
			this.SizeOfProducerCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DensityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DensityCalcEdit, "ZG_Density");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).ZG_Density)));
			this.DensityCalcEdit.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("dded77bf-35fc-4a5f-a1c7-0c8c82809743", "Density");
			this.DensityCalcEdit.DecimalPlaces = 2;
			this.DensityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 351, true);
			this.DensityCalcEdit.Name = "DensityCalcEdit";
			this.DensityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.DensityCalcEdit.TabIndex = 15;
			this.DensityCalcEdit.Text = "0.00";
			this.DensityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DegreePlatoCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DegreePlatoCalcEdit, "ZG_DegreePlato");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).ZG_DegreePlato)));
			this.DegreePlatoCalcEdit.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("8211613c-d7ae-4771-af05-e8cb8ad69ae2", "Degree Plato");
			this.DegreePlatoCalcEdit.DecimalPlaces = 2;
			this.DegreePlatoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 329, true);
			this.DegreePlatoCalcEdit.MaxValue = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.DegreePlatoCalcEdit.Name = "DegreePlatoCalcEdit";
			this.DegreePlatoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.DegreePlatoCalcEdit.TabIndex = 14;
			this.DegreePlatoCalcEdit.Text = "0.00";
			this.DegreePlatoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BrandNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.BrandNameTextBox, "JI_BrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_BrandName)));
			this.BrandNameTextBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("f3153048-b5f6-44e2-9ef4-e274fe2df3e3", "Brand Name");
			this.BrandNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BrandNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 262, true);
			this.BrandNameTextBox.Name = "BrandNameTextBox";
			this.BrandNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.BrandNameTextBox.TabIndex = 11;
			// 
			// NetWeightCalcDropEdit
			// 
			this.NetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_NetWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Lookups.WeightUQList)));
			this.NetWeightCalcDropEdit.BindToAmount = "JI_NetWeight";
			this.NetWeightCalcDropEdit.BindToList = "Lookups+WeightUQList";
			this.NetWeightCalcDropEdit.BindToUnit = "JI_NetWeightUQ";
			this.NetWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("8858f9fe-65b4-4ef2-a0eb-12a6905b6139", "Net Wgt");
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 240, true);
			this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.NetWeightCalcDropEdit.TabIndex = 10;
			this.NetWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// WeightCalcDropEdit
			// 
			this.WeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).Lookups.WeightUQList)));
			this.WeightCalcDropEdit.BindToAmount = "JI_Weight";
			this.WeightCalcDropEdit.BindToList = "Lookups+WeightUQList";
			this.WeightCalcDropEdit.BindToUnit = "JI_WeightUQ";
			this.WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 218, true);
			this.WeightCalcDropEdit.Name = "WeightCalcDropEdit";
			this.WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.WeightCalcDropEdit.TabIndex = 9;
			this.WeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// FiscalMarkUserControl
			// 
			this.FiscalMarkUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FiscalMarkUserControl, ".");
			this.FiscalMarkUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 195, true);
			this.FiscalMarkUserControl.Name = "FiscalMarkUserControl";
			this.FiscalMarkUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 21, true);
			this.FiscalMarkUserControl.TabIndex = 8;
			// 
			// AlcoholicStrengthUserControl
			// 
			this.AlcoholicStrengthUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AlcoholicStrengthUserControl, ".");
			this.AlcoholicStrengthUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 284, true);
			this.AlcoholicStrengthUserControl.Name = "AlcoholicStrengthUserControl";
			this.AlcoholicStrengthUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 21, true);
			this.AlcoholicStrengthUserControl.TabIndex = 12;
			// 
			// TariffCodeFindBox
			// 
			this.TariffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffCodeFindBox, "JI_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_Tariff)));
			this.TariffCodeFindBox.CodeBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.TariffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 129, true);
			this.TariffCodeFindBox.Name = "TariffCodeFindBox";
			this.TariffCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TariffCodeFindBox.ParentType = null;
			this.TariffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.TariffCodeFindBox.TabIndex = 5;
			// 
			// ExciseProductCodeDropEdit
			// 
			this.ExciseProductCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExciseProductCodeDropEdit, "ZG_ExciseProductCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).ZG_ExciseProductCode)));
			this.ExciseProductCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 151, true);
			this.ExciseProductCodeDropEdit.Name = "ExciseProductCodeDropEdit";
			this.ExciseProductCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 20, true);
			this.ExciseProductCodeDropEdit.TabIndex = 6;
			// 
			// ProductCodeFindBox
			// 
			this.ProductCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductCodeFindBox, "JI_PartNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_PartNo)));
			this.ProductCodeFindBox.CodeBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ProductCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 107, true);
			this.ProductCodeFindBox.Name = "ProductCodeFindBox";
			this.ProductCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ProductCodeFindBox.ParentType = null;
			this.ProductCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.ProductCodeFindBox.TabIndex = 4;
			// 
			// LineNoCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LineNoCalcEdit, "JI_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).JI_LineNo)));
			this.LineNoCalcEdit.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("d400a54e-9781-4b24-b161-22075919d2c8", "Line No");
			this.LineNoCalcEdit.DecimalPlaces = 2;
			this.LineNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 59, true);
			this.LineNoCalcEdit.Name = "LineNoCalcEdit";
			this.LineNoCalcEdit.ReadOnly = true;
			this.LineNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.LineNoCalcEdit.TabIndex = 2;
			this.LineNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// IsMainPackCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsMainPackCheckBox, "ZG_IsMainPack");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).ZG_IsMainPack)));
			this.IsMainPackCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 81, true);
			this.IsMainPackCheckBox.Name = "IsMainPackCheckBox";
			this.IsMainPackCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 24, true);
			this.IsMainPackCheckBox.TabIndex = 3;
			this.IsMainPackCheckBox.UseVisualStyleBackColor = true;
			// 
			// WineDetailsSeparatorUserControl
			// 
			this.WineDetailsSeparatorUserControl.AllowDrop = true;
			this.WineDetailsSeparatorUserControl.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("702ce612-ac4e-4b23-8036-ff405fb91eb0", "Wine Details");
			this.WineDetailsSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 10, true);
			this.WineDetailsSeparatorUserControl.Name = "WineDetailsSeparatorUserControl";
			this.WineDetailsSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 15, true);
			this.WineDetailsSeparatorUserControl.TabIndex = 16;
			// 
			// MaturationPeriodOrAgeOfProductsWordWrappingTextBox
			// 
			this.BindingSource.SetBindingMember(this.MaturationPeriodOrAgeOfProductsWordWrappingTextBox, "ZG_MaturationPeriodOrAgeOfProducts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).ZG_MaturationPeriodOrAgeOfProducts)));
			this.MaturationPeriodOrAgeOfProductsWordWrappingTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MaturationPeriodOrAgeOfProductsWordWrappingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 376, true);
			this.MaturationPeriodOrAgeOfProductsWordWrappingTextBox.Multiline = true;
			this.MaturationPeriodOrAgeOfProductsWordWrappingTextBox.Name = "MaturationPeriodOrAgeOfProductsWordWrappingTextBox";
			this.MaturationPeriodOrAgeOfProductsWordWrappingTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MaturationPeriodOrAgeOfProductsWordWrappingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 94, true);
			this.MaturationPeriodOrAgeOfProductsWordWrappingTextBox.TabIndex = 22;
			// 
			// IndependentSmallProducersDeclarationWordWrappingTextBox
			// 
			this.BindingSource.SetBindingMember(this.IndependentSmallProducersDeclarationWordWrappingTextBox, "ZG_IndependentSmallProducersDeclaration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).ZG_IndependentSmallProducersDeclaration)));
			this.IndependentSmallProducersDeclarationWordWrappingTextBox.CaptionResourceString = null;
			this.IndependentSmallProducersDeclarationWordWrappingTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.IndependentSmallProducersDeclarationWordWrappingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 479, true);
			this.IndependentSmallProducersDeclarationWordWrappingTextBox.Multiline = true;
			this.IndependentSmallProducersDeclarationWordWrappingTextBox.Name = "IndependentSmallProducersDeclarationWordWrappingTextBox";
			this.IndependentSmallProducersDeclarationWordWrappingTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.IndependentSmallProducersDeclarationWordWrappingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 94, true);
			this.IndependentSmallProducersDeclarationWordWrappingTextBox.TabIndex = 22;
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MaturationPeriodOrAgeOfProductsWordWrappingTextBox);
			this.Controls.Add(this.CommentsLongTextControl);
			this.Controls.Add(this.WineDetailsSeparatorUserControl);
			this.Controls.Add(this.WineCountryOriginCodeFindBox);
			this.Controls.Add(this.GrowingZoneDropEdit);
			this.Controls.Add(this.WineCategoryDropEdit);
			this.Controls.Add(this.OperationCodesGroupBox);
			this.Controls.Add(this.CustomsQuantityCalcDropEdit);
			this.Controls.Add(this.SizeOfProducerCalcEdit);
			this.Controls.Add(this.DensityCalcEdit);
			this.Controls.Add(this.DegreePlatoCalcEdit);
			this.Controls.Add(this.BrandNameTextBox);
			this.Controls.Add(this.NetWeightCalcDropEdit);
			this.Controls.Add(this.WeightCalcDropEdit);
			this.Controls.Add(this.FiscalMarkUserControl);
			this.Controls.Add(this.AlcoholicStrengthUserControl);
			this.Controls.Add(this.TariffCodeFindBox);
			this.Controls.Add(this.ExciseProductCodeDropEdit);
			this.Controls.Add(this.ProductCodeFindBox);
			this.Controls.Add(this.LineNoCalcEdit);
			this.Controls.Add(this.IsMainPackCheckBox);
			this.Controls.Add(this.OriginLongTextControl);
			this.Controls.Add(this.DescriptionLongTextControl);
			this.Controls.Add(this.IndependentSmallProducersDeclarationWordWrappingTextBox);
			this.Name = "InvoiceLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(939, 587, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CommentsLongTextControl.ResumeLayout(true);
			this.CommentsLongTextControl.PerformLayout();
			this.OriginLongTextControl.ResumeLayout(true);
			this.OriginLongTextControl.PerformLayout();
			this.DescriptionLongTextControl.ResumeLayout(true);
			this.DescriptionLongTextControl.PerformLayout();
			this.WineCountryOriginCodeFindBox.ResumeLayout(true);
			this.WineCountryOriginCodeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OperationCodesGrid)).EndInit();
			this.OperationCodesGrid.ResumeLayout(false);
			this.OperationCodesGrid.PerformLayout();
			this.WineCategoryDropEdit.ResumeLayout(true);
			this.WineCategoryDropEdit.PerformLayout();
			this.GrowingZoneDropEdit.ResumeLayout(true);
			this.GrowingZoneDropEdit.PerformLayout();
			this.OperationCodesGroupBox.ResumeLayout(false);
			this.OperationCodesGroupBox.PerformLayout();
			this.CustomsQuantityCalcDropEdit.ResumeLayout(true);
			this.CustomsQuantityCalcDropEdit.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.WeightCalcDropEdit.ResumeLayout(true);
			this.WeightCalcDropEdit.PerformLayout();
			this.FiscalMarkUserControl.ResumeLayout(true);
			this.FiscalMarkUserControl.PerformLayout();
			this.AlcoholicStrengthUserControl.ResumeLayout(true);
			this.AlcoholicStrengthUserControl.PerformLayout();
			this.TariffCodeFindBox.ResumeLayout(true);
			this.TariffCodeFindBox.PerformLayout();
			this.ExciseProductCodeDropEdit.ResumeLayout(true);
			this.ExciseProductCodeDropEdit.PerformLayout();
			this.ProductCodeFindBox.ResumeLayout(true);
			this.ProductCodeFindBox.PerformLayout();
			this.WineDetailsSeparatorUserControl.ResumeLayout(true);
			this.WineDetailsSeparatorUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Customs.GUI.LongTextControl CommentsLongTextControl;
		internal ZArchitecture.GUI.ZCalcDropEdit CustomsQuantityCalcDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox WineCountryOriginCodeFindBox;
		internal ZArchitecture.GUI.ZDropEdit GrowingZoneDropEdit;
		internal ZArchitecture.GUI.ZDropEdit WineCategoryDropEdit;
		internal ZArchitecture.ZGrid OperationCodesGrid;
		internal ZArchitecture.GUI.ZGroupBox OperationCodesGroupBox;
		internal ZArchitecture.ZCalcEdit SizeOfProducerCalcEdit;
		internal ZArchitecture.ZCalcEdit DensityCalcEdit;
		internal ZArchitecture.ZCalcEdit DegreePlatoCalcEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit NetWeightCalcDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit WeightCalcDropEdit;
		internal ZArchitecture.ZTextBox BrandNameTextBox;
		internal ZArchitecture.ZCalcEdit LineNoCalcEdit;
		internal ZArchitecture.GUI.ZCodeFindBox ProductCodeFindBox;
		internal ZArchitecture.GUI.ZDropEdit ExciseProductCodeDropEdit;
		internal Customs.GUI.TariffFindBox TariffCodeFindBox;
		internal ZArchitecture.GUI.ZCheckBox IsMainPackCheckBox;
		internal InvoiceLineFiscalMarkUserControl FiscalMarkUserControl;
		internal InvoiceLineAlcoholicStrengthUserControl AlcoholicStrengthUserControl;
		internal Customs.GUI.LongTextControl OriginLongTextControl;
		internal Customs.GUI.LongTextControl DescriptionLongTextControl;
		internal ZArchitecture.GUI.SeparatorUserControl WineDetailsSeparatorUserControl;
		internal Customs.GUI.WordWrappingTextBox MaturationPeriodOrAgeOfProductsWordWrappingTextBox;
		internal Customs.GUI.WordWrappingTextBox IndependentSmallProducersDeclarationWordWrappingTextBox;
	}
}

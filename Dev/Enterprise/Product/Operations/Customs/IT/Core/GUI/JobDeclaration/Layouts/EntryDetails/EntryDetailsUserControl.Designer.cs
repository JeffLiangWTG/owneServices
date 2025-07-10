using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI
{
	sealed partial class EntryDetailsUserControl
	{
		#region InitializeComponent

		void InitializeComponent()
		{
			this.ReferenceLabel = new Enterprise.Customs.IT.GUI.GroupLabel();
			this.TotalsLabel = new Enterprise.Customs.IT.GUI.GroupLabel();
			this.StatusLabel = new Enterprise.Customs.IT.GUI.GroupLabel();
			this.CustomsLabel = new Enterprise.Customs.IT.GUI.GroupLabel();
			this.A93Label = new Enterprise.Customs.IT.GUI.GroupLabel();
			this.ExitLabel = new Enterprise.Customs.IT.GUI.GroupLabel();
			this.A93Grid = new Enterprise.ZArchitecture.ZGrid();
			this.EntryTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IssueDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.IncotermTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GrossWeightUserControl = new Enterprise.Customs.IT.GUI.AmountAndUnitControl();
			this.NetWeightUserControl = new Enterprise.Customs.IT.GUI.AmountAndUnitControl();
			this.CustomsQuantityUserControl = new Enterprise.Customs.IT.GUI.AmountAndUnitControl();
			this.InvoiceAmountUserControl = new Enterprise.Customs.IT.GUI.InvoiceAmountUserControl();
			this.FreightAdjustmentCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MessageStatusUserControl = new Enterprise.Customs.IT.GUI.MessageStatusUserControl();
			this.ControlChannelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.WarehouseStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RegistrationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsOfficeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReleaseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExitDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExitOfficeUserControl = new Enterprise.Customs.IT.GUI.ExitOfficeUserControl();
			this.ExitStatusUserControl = new Enterprise.Customs.IT.GUI.ExitStatusUserControl();
			this.SubmittedDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MRNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReleaseDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EntryStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.A93Grid)).BeginInit();
			this.A93Grid.SuspendLayout();
			this.IssueDateDateEdit.SuspendLayout();
			this.GrossWeightUserControl.SuspendLayout();
			this.NetWeightUserControl.SuspendLayout();
			this.CustomsQuantityUserControl.SuspendLayout();
			this.InvoiceAmountUserControl.SuspendLayout();
			this.MessageStatusUserControl.SuspendLayout();
			this.ControlChannelDropEdit.SuspendLayout();
			this.MessageTypeDropEdit.SuspendLayout();
			this.WarehouseStatusDropEdit.SuspendLayout();
			this.ExitDateDateEdit.SuspendLayout();
			this.ExitOfficeUserControl.SuspendLayout();
			this.ExitStatusUserControl.SuspendLayout();
			this.SubmittedDateDateEdit.SuspendLayout();
			this.ReleaseDateDateEdit.SuspendLayout();
			this.EntryStatusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobDeclaration);
			// 
			// ReferenceLabel
			// 
			this.ReferenceLabel.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("324E29A1-DCF1-4AED-95EE-A65FB7DEB34A", "Reference");
			this.ReferenceLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ReferenceLabel.IsFontBold = true;
			this.ReferenceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 9, true);
			this.ReferenceLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
			this.ReferenceLabel.Name = "ReferenceLabel";
			this.ReferenceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.ReferenceLabel.TabIndex = 18;
			// 
			// TotalsLabel
			// 
			this.TotalsLabel.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("C5E2ECC2-470B-4AE2-AA55-4C5F8F5A6D17", "Totals");
			this.TotalsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.TotalsLabel.IsFontBold = true;
			this.TotalsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 30, true);
			this.TotalsLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
			this.TotalsLabel.Name = "TotalsLabel";
			this.TotalsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.TotalsLabel.TabIndex = 17;
			// 
			// StatusLabel
			// 
			this.StatusLabel.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("D7F0874C-B6DE-4588-88A8-315A60362DC6", "Status");
			this.StatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.StatusLabel.IsFontBold = true;
			this.StatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 50, true);
			this.StatusLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.StatusLabel.TabIndex = 16;
			// 
			// CustomsLabel
			// 
			this.CustomsLabel.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("59AE5555-9DE1-4209-941A-4BB3E01C0AE5", "Customs");
			this.CustomsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.CustomsLabel.IsFontBold = true;
			this.CustomsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 70, true);
			this.CustomsLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
			this.CustomsLabel.Name = "CustomsLabel";
			this.CustomsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.CustomsLabel.TabIndex = 15;
			// 
			// A93Label
			// 
			this.A93Label.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("7F5174F3-4B90-4223-9BFB-FD4E47CB4F6A", "A93");
			this.A93Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.A93Label.IsFontBold = true;
			this.A93Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 91, true);
			this.A93Label.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
			this.A93Label.Name = "A93Label";
			this.A93Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.A93Label.TabIndex = 14;
			// 
			// ExitLabel
			// 
			this.ExitLabel.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("18F38540-95F8-409F-B7B3-9E5F5364A42A", "Exit");
			this.ExitLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ExitLabel.IsFontBold = true;
			this.ExitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 109, true);
			this.ExitLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
			this.ExitLabel.Name = "ExitLabel";
			this.ExitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.ExitLabel.TabIndex = 13;
			// 
			// A93Grid
			// 
			this.A93Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.A93Grid, "CustomsEntryHeaders.EntryPayInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).EntryPayInfos)));
			this.A93Grid.CaptionVisible = false;
			this.A93Grid.GridId = "31347a8c-43b1-488b-a46e-b0a53e6a5521";
			this.A93Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.A93Grid.LayoutKey = "A93Grid";
			this.A93Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 419, true);
			this.A93Grid.Name = "A93Grid";
			this.A93Grid.ReadOnly = true;
			this.A93Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 102, true);
			this.A93Grid.TabIndex = 0;
			// 
			// EntryTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryTypeTextBox, "CustomsEntryHeaders.EntryTypeFriendlyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).EntryTypeFriendlyName)));
			this.EntryTypeTextBox.CaptionResourceString = null;
			this.EntryTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 553, true);
			this.EntryTypeTextBox.Name = "EntryTypeTextBox";
			this.EntryTypeTextBox.ReadOnly = true;
			this.EntryTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(179, 20, true);
			this.EntryTypeTextBox.TabIndex = 0;
			// 
			// ReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "CustomsEntryHeaders.CH_BGMReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_BGMReference)));
			this.ReferenceNumberTextBox.CaptionResourceString = null;
			this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 631, true);
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.ReadOnly = true;
			this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(179, 20, true);
			this.ReferenceNumberTextBox.TabIndex = 1;
			// 
			// IssueDateDateEdit
			// 
			this.IssueDateDateEdit.AllowDrop = true;
			this.IssueDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.IssueDateDateEdit, "CustomsEntryHeaders.CusEntryNumber.CE_IssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CusEntryNumber.CE_IssueDate)));
			this.IssueDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 683, true);
			this.IssueDateDateEdit.Name = "IssueDateDateEdit";
			this.IssueDateDateEdit.TabIndex = 2;
			this.IssueDateDateEdit.ReadOnly = true;
			// 
			// IncotermTextBox
			// 
			this.BindingSource.SetBindingMember(this.IncotermTextBox, "CustomsEntryHeaders.CH_IncoTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_IncoTerm)));
			this.IncotermTextBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("510E96AD-9EF2-48CC-9CF2-146E726EF421", "Incoterm");
			this.IncotermTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 709, true);
			this.IncotermTextBox.Name = "IncotermTextBox";
			this.IncotermTextBox.ReadOnly = true;
			this.IncotermTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.IncotermTextBox.TabIndex = 3;
			// 
			// GrossWeightUserControl
			//
			this.BindingSource.SetBindingMember(this.GrossWeightUserControl, "CustomsEntryHeaders.TotalGrossWeightInKG");
			this.GrossWeightUserControl.AllowDrop = true;
			this.GrossWeightUserControl.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("EE06C04A-F338-49EF-B52A-AEACE322F74E", "Gross Weight");
			this.GrossWeightUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 185, true);
			this.GrossWeightUserControl.Name = "GrossWeightUserControl";
			this.GrossWeightUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.GrossWeightUserControl.TabIndex = 12;
			this.GrossWeightUserControl.UnitOfMeasureText = "KG";
			// 
			// NetWeightUserControl
			//
			this.BindingSource.SetBindingMember(this.NetWeightUserControl, "CustomsEntryHeaders.TotalNetWeightInKG");
			this.NetWeightUserControl.AllowDrop = true;
			this.NetWeightUserControl.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("2DF5E3F1-600C-43C5-B43F-DAB51C19F783", "Net Weight");
			this.NetWeightUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 211, true);
			this.NetWeightUserControl.Name = "NetWeightUserControl";
			this.NetWeightUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.NetWeightUserControl.TabIndex = 11;
			this.NetWeightUserControl.UnitOfMeasureText = "KG";
			// 
			// CustomsQuantityUserControl
			// 
			this.BindingSource.SetBindingMember(this.CustomsQuantityUserControl, "CustomsEntryHeaders.TotalCustomsQuantity");
			this.CustomsQuantityUserControl.AllowDrop = true;
			this.CustomsQuantityUserControl.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("311EAFEE-EA40-4B75-949C-488BA903F21F", "Customs Quantity");
			this.CustomsQuantityUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 237, true);
			this.CustomsQuantityUserControl.Name = "CustomsQuantityUserControl";
			this.CustomsQuantityUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.CustomsQuantityUserControl.TabIndex = 10;
			this.CustomsQuantityUserControl.UnitOfMeasureText = "KG";
			// 
			// InvoiceAmountUserControl
			// 
			this.InvoiceAmountUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceAmountUserControl, ".");
			this.InvoiceAmountUserControl.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("39FCB631-2839-4968-9888-21D827C9D84D", "Invoice Amount");
			this.InvoiceAmountUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 839, true);
			this.InvoiceAmountUserControl.Name = "InvoiceAmountUserControl";
			this.InvoiceAmountUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(179, 20, true);
			this.InvoiceAmountUserControl.TabIndex = 7;
			// 
			// FreightAdjustmentCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FreightAdjustmentCalcEdit, "CustomsEntryHeaders.CH_FreightAdjustment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_FreightAdjustment)));
			this.FreightAdjustmentCalcEdit.CaptionResourceString = null;
			this.FreightAdjustmentCalcEdit.DecimalPlaces = 2;
			this.FreightAdjustmentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 787, true);
			this.FreightAdjustmentCalcEdit.Name = "FreightAdjustmentCalcEdit";
			this.FreightAdjustmentCalcEdit.ReadOnly = true;
			this.FreightAdjustmentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.FreightAdjustmentCalcEdit.TabIndex = 9;
			this.FreightAdjustmentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MessageStatusUserControl
			// 
			this.MessageStatusUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatusUserControl, ".");
			this.MessageStatusUserControl.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("A6EA3C57-3296-4449-B9A2-B5F1A3B3B60F", "Message Status");
			this.MessageStatusUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 579, true);
			this.MessageStatusUserControl.Name = "MessageStatusUserControl";
			this.MessageStatusUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.MessageStatusUserControl.TabIndex = 8;
			// 
			// ControlChannelDropEdit
			// 
			this.ControlChannelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ControlChannelDropEdit, "CustomsEntryHeaders.CustomsChannel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CustomsChannel)));
			this.ControlChannelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 735, true);
			this.ControlChannelDropEdit.Name = "ControlChannelDropEdit";
			this.ControlChannelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.ControlChannelDropEdit.TabIndex = 4;
			// 
			// MessageTypeDropEdit
			// 
			this.MessageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageTypeDropEdit, "CustomsEntryHeaders.CH_MessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_MessageType)));
			this.MessageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 761, true);
			this.MessageTypeDropEdit.Name = "MessageTypeDropEdit";
			this.MessageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.MessageTypeDropEdit.TabIndex = 6;
			// 
			// WarehouseStatusDropEdit
			// 
			this.WarehouseStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WarehouseStatusDropEdit, "CustomsEntryHeaders.CH_WarehouseTransactionStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_WarehouseTransactionStatus)));
			this.WarehouseStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 761, true);
			this.WarehouseStatusDropEdit.Name = "WarehouseStatusDropEdit";
			this.WarehouseStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.WarehouseStatusDropEdit.TabIndex = 7;
			// 
			// RegistrationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.RegistrationNumberTextBox, "CustomsEntryHeaders.RegistrationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).RegistrationNumber)));
			this.RegistrationNumberTextBox.CaptionResourceString = null;
			this.RegistrationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 605, true);
			this.RegistrationNumberTextBox.Name = "RegistrationNumberTextBox";
			this.RegistrationNumberTextBox.ReadOnly = true;
			this.RegistrationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.RegistrationNumberTextBox.TabIndex = 0;
			// 
			// CustomsOfficeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsOfficeTextBox, "CustomsEntryHeaders.CusEntryNumber.CE_EntryLineReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CusEntryNumber.CE_EntryLineReference)));
			this.CustomsOfficeTextBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("88F8AA49-8C39-4766-A736-4CB3794ADEEF", "Customs Office");
			this.CustomsOfficeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 657, true);
			this.CustomsOfficeTextBox.Name = "CustomsOfficeTextBox";
			this.CustomsOfficeTextBox.ReadOnly = true;
			this.CustomsOfficeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.CustomsOfficeTextBox.TabIndex = 1;
			// 
			// ReleaseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReleaseCodeTextBox, "CustomsEntryHeaders.EntryNumbersProvider.ReleaseInfo.CE_EntryNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).EntryNumbersProvider.ReleaseInfo.CE_EntryNum)));
			this.ReleaseCodeTextBox.CaptionResourceString = null;
			this.ReleaseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 813, true);
			this.ReleaseCodeTextBox.Name = "ReleaseCodeTextBox";
			this.ReleaseCodeTextBox.ReadOnly = true;
			this.ReleaseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.ReleaseCodeTextBox.TabIndex = 4;
			// 
			// ExitDateDateEdit
			// 
			this.ExitDateDateEdit.AllowDrop = true;
			this.ExitDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ExitDateDateEdit, "CustomsEntryHeaders.EntryNumbersProvider.IvistoWrapper.Date");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).EntryNumbersProvider.IvistoWrapper.Date)));
			this.ExitDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 133, true);
			this.ExitDateDateEdit.Name = "ExitDateDateEdit";
			this.ExitDateDateEdit.TabIndex = 0;
			// 
			// ExitOfficeUserControl
			// 
			this.ExitOfficeUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExitOfficeUserControl, ".");
			this.ExitOfficeUserControl.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("0cc25926-3f59-4c35-ae07-92a7b37b2d30", "Exit Office");
			this.ExitOfficeUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 341, true);
			this.ExitOfficeUserControl.Name = "ExitOfficeUserControl";
			this.ExitOfficeUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 20, true);
			this.ExitOfficeUserControl.TabIndex = 7;
			// 
			// ExitStatusUserControl
			// 
			this.ExitStatusUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExitStatusUserControl, ".");
			this.ExitStatusUserControl.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("01670162-833C-4745-9573-E7C3F54C657C", "Exit Status");
			this.ExitStatusUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 367, true);
			this.ExitStatusUserControl.Name = "ExitStatusUserControl";
			this.ExitStatusUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 20, true);
			this.ExitStatusUserControl.TabIndex = 6;
			// 
			// SubmittedDateDateEdit
			// 
			this.SubmittedDateDateEdit.AllowDrop = true;
			this.SubmittedDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.SubmittedDateDateEdit, "CustomsEntryHeaders.CH_EntrySubmittedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntrySubmittedDate)));
			this.SubmittedDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.SubmittedDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 865, true);
			this.SubmittedDateDateEdit.Name = "SubmittedDateDateEdit";
			this.SubmittedDateDateEdit.TabIndex = 2;
			this.SubmittedDateDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.SubmittedDateDateEdit.ReadOnly = true;
			// 
			// MRNTextBox
			// 
			this.BindingSource.SetBindingMember(this.MRNTextBox, "CustomsEntryHeaders.MovementReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MovementReferenceNumber)));
			this.MRNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 263, true);
			this.MRNTextBox.Name = "MRNTextBox";
			this.MRNTextBox.ReadOnly = true;
			this.MRNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.MRNTextBox.TabIndex = 3;
			// 
			// ReleaseDateDateEdit
			// 
			this.ReleaseDateDateEdit.AllowDrop = true;
			this.ReleaseDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ReleaseDateDateEdit, "CustomsEntryHeaders.CH_EntryReleaseDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntryReleaseDate)));
			this.ReleaseDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 289, true);
			this.ReleaseDateDateEdit.Name = "ReleaseDateDateEdit";
			this.ReleaseDateDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.ReleaseDateDateEdit.TabIndex = 5;
			this.ReleaseDateDateEdit.ReadOnly = true;
			// 
			// EntryStatusDropEdit
			// 
			this.EntryStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryStatusDropEdit, "CustomsEntryHeaders.CH_EntryStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntryStatus)));
			this.EntryStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 318, true);
			this.EntryStatusDropEdit.Name = "EntryStatusDropEdit";
			this.EntryStatusDropEdit.PreBoundMaxLength = 2;
			this.EntryStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.EntryStatusDropEdit.TabIndex = 8;
			// 
			// EntryDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExitStatusUserControl);
			this.Controls.Add(this.ExitOfficeUserControl);
			this.Controls.Add(this.ExitDateDateEdit);
			this.Controls.Add(this.CustomsOfficeTextBox);
			this.Controls.Add(this.ReleaseCodeTextBox);
			this.Controls.Add(this.RegistrationNumberTextBox);
			this.Controls.Add(this.MessageTypeDropEdit);
			this.Controls.Add(this.WarehouseStatusDropEdit);
			this.Controls.Add(this.ControlChannelDropEdit);
			this.Controls.Add(this.MessageStatusUserControl);
			this.Controls.Add(this.FreightAdjustmentCalcEdit);
			this.Controls.Add(this.InvoiceAmountUserControl);
			this.Controls.Add(this.CustomsQuantityUserControl);
			this.Controls.Add(this.NetWeightUserControl);
			this.Controls.Add(this.GrossWeightUserControl);
			this.Controls.Add(this.IncotermTextBox);
			this.Controls.Add(this.IssueDateDateEdit);
			this.Controls.Add(this.ReferenceNumberTextBox);
			this.Controls.Add(this.EntryTypeTextBox);
			this.Controls.Add(this.A93Grid);
			this.Controls.Add(this.ExitLabel);
			this.Controls.Add(this.A93Label);
			this.Controls.Add(this.CustomsLabel);
			this.Controls.Add(this.StatusLabel);
			this.Controls.Add(this.TotalsLabel);
			this.Controls.Add(this.ReferenceLabel);
			this.Controls.Add(this.SubmittedDateDateEdit);
			this.Controls.Add(this.MRNTextBox);
			this.Controls.Add(this.ReleaseDateDateEdit);
			this.Controls.Add(this.EntryStatusDropEdit);
			this.Name = "EntryDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(825, 955, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.A93Grid)).EndInit();
			this.A93Grid.ResumeLayout(false);
			this.A93Grid.PerformLayout();
			this.IssueDateDateEdit.ResumeLayout(true);
			this.IssueDateDateEdit.PerformLayout();
			this.GrossWeightUserControl.ResumeLayout(true);
			this.GrossWeightUserControl.PerformLayout();
			this.NetWeightUserControl.ResumeLayout(true);
			this.NetWeightUserControl.PerformLayout();
			this.CustomsQuantityUserControl.ResumeLayout(true);
			this.CustomsQuantityUserControl.PerformLayout();
			this.InvoiceAmountUserControl.ResumeLayout(true);
			this.InvoiceAmountUserControl.PerformLayout();
			this.MessageStatusUserControl.ResumeLayout(true);
			this.MessageStatusUserControl.PerformLayout();
			this.ControlChannelDropEdit.ResumeLayout(true);
			this.ControlChannelDropEdit.PerformLayout();
			this.MessageTypeDropEdit.ResumeLayout(true);
			this.MessageTypeDropEdit.PerformLayout();
			this.WarehouseStatusDropEdit.ResumeLayout(true);
			this.WarehouseStatusDropEdit.PerformLayout();
			this.ExitDateDateEdit.ResumeLayout(true);
			this.ExitDateDateEdit.PerformLayout();
			this.ExitOfficeUserControl.ResumeLayout(true);
			this.ExitOfficeUserControl.PerformLayout();
			this.ExitStatusUserControl.ResumeLayout(true);
			this.ExitStatusUserControl.PerformLayout();
			this.SubmittedDateDateEdit.ResumeLayout(true);
			this.SubmittedDateDateEdit.PerformLayout();
			this.ReleaseDateDateEdit.ResumeLayout(true);
			this.ReleaseDateDateEdit.PerformLayout();
			this.EntryStatusDropEdit.ResumeLayout(true);
			this.EntryStatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZTextBox EntryTypeTextBox;
		internal ZTextBox ReferenceNumberTextBox;
		internal ZDateEdit IssueDateDateEdit;
		internal ZTextBox IncotermTextBox;
		internal AmountAndUnitControl GrossWeightUserControl;
		internal AmountAndUnitControl NetWeightUserControl;
		internal AmountAndUnitControl CustomsQuantityUserControl;
		internal InvoiceAmountUserControl InvoiceAmountUserControl;
		internal ZCalcEdit FreightAdjustmentCalcEdit;
		internal MessageStatusUserControl MessageStatusUserControl;
		internal ZDropEdit ControlChannelDropEdit;
		internal ZDropEdit MessageTypeDropEdit;
		internal ZDropEdit WarehouseStatusDropEdit;
		internal ZTextBox RegistrationNumberTextBox;
		internal ZTextBox CustomsOfficeTextBox;
		internal ZTextBox ReleaseCodeTextBox;
		internal ZGrid A93Grid;
		internal ZDateEdit ExitDateDateEdit;
		internal ExitOfficeUserControl ExitOfficeUserControl;
		internal ExitStatusUserControl ExitStatusUserControl;
		internal GroupLabel ReferenceLabel;
		internal GroupLabel TotalsLabel;
		internal GroupLabel StatusLabel;
		internal GroupLabel CustomsLabel;
		internal GroupLabel A93Label;
		internal GroupLabel ExitLabel;
		internal ZArchitecture.GUI.ZDropEdit EntryStatusDropEdit;
		internal ZArchitecture.GUI.ZDateEdit SubmittedDateDateEdit;
		internal ZArchitecture.ZTextBox MRNTextBox;
		internal ZArchitecture.GUI.ZDateEdit ReleaseDateDateEdit;
	}
}

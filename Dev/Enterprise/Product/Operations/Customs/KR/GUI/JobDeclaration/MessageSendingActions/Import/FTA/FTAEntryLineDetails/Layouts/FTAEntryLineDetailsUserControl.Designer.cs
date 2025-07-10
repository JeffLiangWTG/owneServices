using CargoWise.Windows.UI;

namespace Enterprise.Customs.KR.GUI
{
	partial class FTAEntryLineDetailsUserControl
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
            this.EntryLineNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.FTASeqNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.COSplitOrderCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.COTotalNetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.CONetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.CONoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.COIssueDateTextBox = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.CountryOfOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.AssociatedCOCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.TarrifRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.PreferenceCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.TariffTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.FTACOProductTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ThirdCountryInvIssuedDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ThirdCountryCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.COExporterNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.COIssuingAgencyTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.COIssueAgencyNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.SupportingDocTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.COIssuerTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.COTotalNetWeightCalcDropEdit.SuspendLayout();
            this.CONetWeightCalcDropEdit.SuspendLayout();
            this.COIssueDateTextBox.SuspendLayout();
            this.CountryOfOriginCodeFindBox.SuspendLayout();
            this.AssociatedCOCountryCodeFindBox.SuspendLayout();
            this.PreferenceCodeDropEdit.SuspendLayout();
            this.FTACOProductTypeDropEdit.SuspendLayout();
            this.ThirdCountryInvIssuedDropEdit.SuspendLayout();
            this.ThirdCountryCodeCodeFindBox.SuspendLayout();
            this.COIssuingAgencyTypeDropEdit.SuspendLayout();
            this.SupportingDocTypeDropEdit.SuspendLayout();
            this.COIssuerTypeDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent);
            // 
            // EntryLineNumberCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.EntryLineNumberCalcEdit, "SendingObjectsCollection.FTALines.EntryLineNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).EntryLineNo)));
            this.EntryLineNumberCalcEdit.DecimalPlaces = 2;
            this.EntryLineNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 33, true);
            this.EntryLineNumberCalcEdit.Name = "EntryLineNumberCalcEdit";
            this.EntryLineNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 18, true);
            this.EntryLineNumberCalcEdit.TabIndex = 0;
            this.EntryLineNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.EntryLineNumberCalcEdit.TrackDisposedAccess = true;
            // 
            // FTASeqNoCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.FTASeqNoCalcEdit, "SendingObjectsCollection.FTALines.FTASequenceNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).FTASequenceNumber)));
            this.FTASeqNoCalcEdit.DecimalPlaces = 2;
            this.FTASeqNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 55, true);
            this.FTASeqNoCalcEdit.Name = "FTASeqNoCalcEdit";
            this.FTASeqNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 18, true);
            this.FTASeqNoCalcEdit.TabIndex = 1;
            this.FTASeqNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.FTASeqNoCalcEdit.TrackDisposedAccess = true;
            // 
            // COSplitOrderCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.COSplitOrderCalcEdit, "SendingObjectsCollection.FTALines.SplitOrder");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).SplitOrder)));
            this.COSplitOrderCalcEdit.DecimalPlaces = 2;
            this.COSplitOrderCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 77, true);
            this.COSplitOrderCalcEdit.Name = "COSplitOrderCalcEdit";
            this.COSplitOrderCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 18, true);
            this.COSplitOrderCalcEdit.TabIndex = 2;
            this.COSplitOrderCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.COSplitOrderCalcEdit.TrackDisposedAccess = true;
            // 
            // COTotalNetWeightCalcDropEdit
            // 
            this.COTotalNetWeightCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.COTotalNetWeightCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).TotalNetWeight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).NetWeightUnit)));
            this.COTotalNetWeightCalcDropEdit.BindToAmount = "SendingObjectsCollection.FTALines.TotalNetWeight";
            this.COTotalNetWeightCalcDropEdit.BindToUnit = "SendingObjectsCollection.FTALines.NetWeightUnit";
            this.COTotalNetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 98, true);
            this.COTotalNetWeightCalcDropEdit.Name = "COTotalNetWeightCalcDropEdit";
			this.COTotalNetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 18, true);
            this.COTotalNetWeightCalcDropEdit.TabIndex = 3;
            this.COTotalNetWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
            // 
            // CONetWeightCalcDropEdit
            // 
            this.CONetWeightCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CONetWeightCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).NetWeightInKG)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).NetWeightUnit)));
            this.CONetWeightCalcDropEdit.BindToAmount = "SendingObjectsCollection.FTALines.NetWeightInKG";
            this.CONetWeightCalcDropEdit.BindToUnit = "SendingObjectsCollection.FTALines.NetWeightUnit";
            this.CONetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 120, true);
            this.CONetWeightCalcDropEdit.Name = "CONetWeightCalcDropEdit";
			this.CONetWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("887DA90F-FEE5-4A6F-8F7A-85C3D55BE61E", "C/O Net Weight");
			this.CONetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
            this.CONetWeightCalcDropEdit.TabIndex = 4;
            this.CONetWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
            // 
            // CONoTextBox
            // 
            this.BindingSource.SetBindingMember(this.CONoTextBox, "SendingObjectsCollection.FTALines.COONo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).COONo)));
            this.CONoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 142, true);
            this.CONoTextBox.Name = "CONoTextBox";
			this.CONoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
            this.CONoTextBox.TabIndex = 5;
            // 
            // COIssueDateTextBox
            // 
            this.COIssueDateTextBox.AllowDrop = true;
            this.COIssueDateTextBox.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.COIssueDateTextBox, "SendingObjectsCollection.FTALines.COOIssuedDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).COOIssuedDate)));
            this.COIssueDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 163, true);
            this.COIssueDateTextBox.Name = "COIssueDateTextBox";
			this.COIssueDateTextBox.TabIndex = 6;
            // 
            // CountryOfOriginCodeFindBox
            // 
            this.CountryOfOriginCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CountryOfOriginCodeFindBox, "SendingObjectsCollection.FTALines.CountryOfOrigin");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).CountryOfOrigin)));
            this.CountryOfOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 185, true);
            this.CountryOfOriginCodeFindBox.Name = "CountryOfOriginCodeFindBox";
			this.CountryOfOriginCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CountryOfOriginCodeFindBox.ParentType = null;
            this.CountryOfOriginCodeFindBox.PreBoundMaxLength = 2;
            this.CountryOfOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 18, true);
            this.CountryOfOriginCodeFindBox.TabIndex = 7;
            // 
            // AssociatedCOCountryCodeFindBox
            // 
            this.AssociatedCOCountryCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.AssociatedCOCountryCodeFindBox, "SendingObjectsCollection.FTALines.AssociatedCOOIssuingCountryCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).AssociatedCOOIssuingCountryCode)));
            this.AssociatedCOCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 207, true);
            this.AssociatedCOCountryCodeFindBox.Name = "AssociatedCOCountryCodeFindBox";
            this.AssociatedCOCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.AssociatedCOCountryCodeFindBox.ParentType = null;
            this.AssociatedCOCountryCodeFindBox.PreBoundMaxLength = 2;
            this.AssociatedCOCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 18, true);
            this.AssociatedCOCountryCodeFindBox.TabIndex = 8;
            // 
            // TarrifRateCalcEdit
            // 
            this.TarrifRateCalcEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TarrifRateCalcEdit, "SendingObjectsCollection.FTALines.TariffRate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).TariffRate)));
            this.TarrifRateCalcEdit.DecimalPlaces = 2;
            this.TarrifRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 229, true);
            this.TarrifRateCalcEdit.Name = "TarrifRateCalcEdit";
            this.TarrifRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 18, true);
            this.TarrifRateCalcEdit.TabIndex = 9;
            this.TarrifRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TarrifRateCalcEdit.TrackDisposedAccess = true;
            // 
            // PreferenceCodeDropEdit
            // 
            this.PreferenceCodeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PreferenceCodeDropEdit, "SendingObjectsCollection.FTALines.Preference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).Preference)));
            this.PreferenceCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 33, true);
            this.PreferenceCodeDropEdit.Name = "PreferenceCodeDropEdit";
            this.PreferenceCodeDropEdit.PreBoundMaxLength = 6;
            this.PreferenceCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.PreferenceCodeDropEdit.TabIndex = 11;
			// 
			// TariffTextBox
			// 
			this.TariffTextBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TariffTextBox, "SendingObjectsCollection.FTALines.FormattedHSCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).FormattedHSCode)));
            this.TariffTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 55, true);
            this.TariffTextBox.Name = "TariffTextBox";
            this.TariffTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.TariffTextBox.TabIndex = 12;
            // 
            // FTACOProductTypeDropEdit
            // 
            this.FTACOProductTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.FTACOProductTypeDropEdit, "SendingObjectsCollection.FTALines.COOProductType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).COOProductType)));
            this.FTACOProductTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 77, true);
            this.FTACOProductTypeDropEdit.Name = "FTACOProductTypeDropEdit";
            this.FTACOProductTypeDropEdit.PreBoundMaxLength = 1;
            this.FTACOProductTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.FTACOProductTypeDropEdit.TabIndex = 13;
            // 
            // ThirdCountryInvIssuedDropEdit
            // 
            this.ThirdCountryInvIssuedDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ThirdCountryInvIssuedDropEdit, "SendingObjectsCollection.FTALines.ThirdCountryAdditionalInvoiceIssued");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).ThirdCountryAdditionalInvoiceIssued)));
            this.ThirdCountryInvIssuedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 98, true);
            this.ThirdCountryInvIssuedDropEdit.Name = "ThirdCountryInvIssuedDropEdit";
            this.ThirdCountryInvIssuedDropEdit.PreBoundMaxLength = 1;
            this.ThirdCountryInvIssuedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.ThirdCountryInvIssuedDropEdit.TabIndex = 14;
            // 
            // ThirdCountryCodeCodeFindBox
            // 
            this.ThirdCountryCodeCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ThirdCountryCodeCodeFindBox, "SendingObjectsCollection.FTALines.ThirdCountry");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).ThirdCountry)));
            this.ThirdCountryCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 120, true);
            this.ThirdCountryCodeCodeFindBox.Name = "ThirdCountryCodeCodeFindBox";
            this.ThirdCountryCodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.ThirdCountryCodeCodeFindBox.ParentType = null;
            this.ThirdCountryCodeCodeFindBox.PreBoundMaxLength = 2;
            this.ThirdCountryCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.ThirdCountryCodeCodeFindBox.TabIndex = 15;
            // 
            // COExporterNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.COExporterNumberTextBox, "SendingObjectsCollection.FTALines.COOExporterNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).COOExporterNumber)));
            this.COExporterNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 142, true);
            this.COExporterNumberTextBox.Name = "COExporterNumberTextBox";
            this.COExporterNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.COExporterNumberTextBox.TabIndex = 16;
            // 
            // COIssuingAgencyTypeDropEdit
            // 
            this.COIssuingAgencyTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.COIssuingAgencyTypeDropEdit, "SendingObjectsCollection.FTALines.COOIssuingAgencyType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).COOIssuingAgencyType)));
            this.COIssuingAgencyTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 163, true);
            this.COIssuingAgencyTypeDropEdit.Name = "COIssuingAgencyTypeDropEdit";
            this.COIssuingAgencyTypeDropEdit.PreBoundMaxLength = 1;
            this.COIssuingAgencyTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.COIssuingAgencyTypeDropEdit.TabIndex = 17;
            // 
            // COIssueAgencyNameTextBox
            // 
            this.BindingSource.SetBindingMember(this.COIssueAgencyNameTextBox, "SendingObjectsCollection.FTALines.COOAgencyName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).COOAgencyName)));
            this.COIssueAgencyNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 185, true);
            this.COIssueAgencyNameTextBox.Name = "COIssueAgencyNameTextBox";
            this.COIssueAgencyNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.COIssueAgencyNameTextBox.TabIndex = 18;
            // 
            // SupportingDocTypeDropEdit
            // 
            this.SupportingDocTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SupportingDocTypeDropEdit, "SendingObjectsCollection.FTALines.COOSupportingDocType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).COOSupportingDocType)));
            this.SupportingDocTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 207, true);
            this.SupportingDocTypeDropEdit.Name = "SupportingDocTypeDropEdit";
            this.SupportingDocTypeDropEdit.PreBoundMaxLength = 1;
            this.SupportingDocTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.SupportingDocTypeDropEdit.TabIndex = 19;
            // 
            // COIssuerTypeDropEdit
            // 
            this.COIssuerTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.COIssuerTypeDropEdit, "SendingObjectsCollection.FTALines.COOIssuerType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTALines)).SyncRoot)).COOIssuerType)));
            this.COIssuerTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 229, true);
            this.COIssuerTypeDropEdit.Name = "COIssuerTypeDropEdit";
            this.COIssuerTypeDropEdit.PreBoundMaxLength = 1;
            this.COIssuerTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.COIssuerTypeDropEdit.TabIndex = 20;
            // 
            // FTAEntryLineDetailsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.COIssuerTypeDropEdit);
            this.Controls.Add(this.SupportingDocTypeDropEdit);
            this.Controls.Add(this.COIssueAgencyNameTextBox);
            this.Controls.Add(this.COIssuingAgencyTypeDropEdit);
            this.Controls.Add(this.COExporterNumberTextBox);
            this.Controls.Add(this.ThirdCountryCodeCodeFindBox);
            this.Controls.Add(this.ThirdCountryInvIssuedDropEdit);
            this.Controls.Add(this.FTACOProductTypeDropEdit);
            this.Controls.Add(this.TariffTextBox);
            this.Controls.Add(this.PreferenceCodeDropEdit);
            this.Controls.Add(this.TarrifRateCalcEdit);
            this.Controls.Add(this.AssociatedCOCountryCodeFindBox);
            this.Controls.Add(this.CountryOfOriginCodeFindBox);
            this.Controls.Add(this.COIssueDateTextBox);
            this.Controls.Add(this.CONoTextBox);
            this.Controls.Add(this.CONetWeightCalcDropEdit);
            this.Controls.Add(this.COTotalNetWeightCalcDropEdit);
            this.Controls.Add(this.COSplitOrderCalcEdit);
            this.Controls.Add(this.FTASeqNoCalcEdit);
            this.Controls.Add(this.EntryLineNumberCalcEdit);
            this.Name = "FTAEntryLineDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 274, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.COTotalNetWeightCalcDropEdit.ResumeLayout(true);
            this.COTotalNetWeightCalcDropEdit.PerformLayout();
            this.CONetWeightCalcDropEdit.ResumeLayout(true);
            this.CONetWeightCalcDropEdit.PerformLayout();
            this.COIssueDateTextBox.ResumeLayout(true);
            this.COIssueDateTextBox.PerformLayout();
            this.CountryOfOriginCodeFindBox.ResumeLayout(true);
            this.CountryOfOriginCodeFindBox.PerformLayout();
            this.AssociatedCOCountryCodeFindBox.ResumeLayout(true);
            this.AssociatedCOCountryCodeFindBox.PerformLayout();
            this.PreferenceCodeDropEdit.ResumeLayout(true);
            this.PreferenceCodeDropEdit.PerformLayout();
            this.FTACOProductTypeDropEdit.ResumeLayout(true);
            this.FTACOProductTypeDropEdit.PerformLayout();
            this.ThirdCountryInvIssuedDropEdit.ResumeLayout(true);
            this.ThirdCountryInvIssuedDropEdit.PerformLayout();
            this.ThirdCountryCodeCodeFindBox.ResumeLayout(true);
            this.ThirdCountryCodeCodeFindBox.PerformLayout();
            this.COIssuingAgencyTypeDropEdit.ResumeLayout(true);
            this.COIssuingAgencyTypeDropEdit.PerformLayout();
            this.SupportingDocTypeDropEdit.ResumeLayout(true);
            this.SupportingDocTypeDropEdit.PerformLayout();
            this.COIssuerTypeDropEdit.ResumeLayout(true);
            this.COIssuerTypeDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZCalcEdit EntryLineNumberCalcEdit;
		public ZArchitecture.ZCalcEdit FTASeqNoCalcEdit;
		public ZArchitecture.ZCalcEdit COSplitOrderCalcEdit;
		public ZArchitecture.GUI.ZCalcDropEdit COTotalNetWeightCalcDropEdit;
		public ZArchitecture.GUI.ZCalcDropEdit CONetWeightCalcDropEdit;
		public ZArchitecture.ZTextBox CONoTextBox;
		public ZArchitecture.GUI.ZDateEdit COIssueDateTextBox;
		public ZArchitecture.GUI.ZCodeFindBox CountryOfOriginCodeFindBox;
		public ZArchitecture.GUI.ZCodeFindBox AssociatedCOCountryCodeFindBox;
		public ZArchitecture.ZCalcEdit TarrifRateCalcEdit;
		public ZArchitecture.GUI.ZDropEdit PreferenceCodeDropEdit;
		public ZArchitecture.ZTextBox TariffTextBox;
		public ZArchitecture.GUI.ZDropEdit FTACOProductTypeDropEdit;
		public ZArchitecture.GUI.ZDropEdit ThirdCountryInvIssuedDropEdit;
		public ZArchitecture.GUI.ZCodeFindBox ThirdCountryCodeCodeFindBox;
		public ZArchitecture.ZTextBox COExporterNumberTextBox;
		public ZArchitecture.GUI.ZDropEdit COIssuingAgencyTypeDropEdit;
		public ZArchitecture.ZTextBox COIssueAgencyNameTextBox;
		public ZArchitecture.GUI.ZDropEdit SupportingDocTypeDropEdit;
		public ZArchitecture.GUI.ZDropEdit COIssuerTypeDropEdit;
	}
}

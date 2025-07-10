namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class TransportDepartureUserControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TransportDepartureUserControl));
            this.InlandTransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.TransportAtDepartureTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TransportAtDepartureCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.TransportAtDepartureTrailer1RegNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TransportAtDepartureTrailer1NationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.TransportAtDepartureTrailer2RegNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TransportAtDepartureTrailer2NationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.AircraftIDAtDepartureTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TransportAtDepartureTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.VesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.VesselCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.AdditionalWagonNumbersButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.PlaceHolderLabel = new Enterprise.ZArchitecture.ZLabel();
            this.PlaceHolder2Label = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.InlandTransportModeDropEdit.SuspendLayout();
            this.TransportAtDepartureCountryCodeFindBox.SuspendLayout();
            this.TransportAtDepartureTrailer1NationalityCodeFindBox.SuspendLayout();
            this.TransportAtDepartureTrailer2NationalityCodeFindBox.SuspendLayout();
            this.TransportAtDepartureTypeDropEdit.SuspendLayout();
            this.VesselCodeFindBox.SuspendLayout();
            this.VesselCountryCodeFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.Interfaces.IDepartureTransportMeansProvider);
            // 
            // InlandTransportModeDropEdit
            // 
            this.InlandTransportModeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.InlandTransportModeDropEdit, "InlandTransportModeAtDeparture");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.Interfaces.IDepartureTransportMeansProvider)(null)).InlandTransportModeAtDeparture)));
            this.InlandTransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 17, true);
            this.InlandTransportModeDropEdit.Name = "InlandTransportModeDropEdit";
            this.InlandTransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
            this.InlandTransportModeDropEdit.TabIndex = 0;
            // 
            // TransportAtDepartureTextBox
            // 
            this.BindingSource.SetBindingMember(this.TransportAtDepartureTextBox, "TransportAtDeparture");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.Interfaces.IDepartureTransportMeansProvider)(null)).TransportAtDeparture)));
            this.TransportAtDepartureTextBox.CaptionResourceString = ((CargoWiseOne.ResourceStrings.ResourceStringData)(resources.GetObject("TransportAtDepartureTextBox.CaptionResourceString")));
            this.TransportAtDepartureTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.TransportAtDepartureTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 43, true);
            this.TransportAtDepartureTextBox.Name = "TransportAtDepartureTextBox";
            this.TransportAtDepartureTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 18, true);
            this.TransportAtDepartureTextBox.TabIndex = 1;
            // 
            // TransportAtDepartureCountryCodeFindBox
            // 
            this.TransportAtDepartureCountryCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TransportAtDepartureCountryCodeFindBox, "TransportCountryAtDeparture");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.Interfaces.IDepartureTransportMeansProvider)(null)).TransportCountryAtDeparture)));
            this.TransportAtDepartureCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 69, true);
            this.TransportAtDepartureCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
            this.TransportAtDepartureCountryCodeFindBox.Name = "TransportAtDepartureCountryCodeFindBox";
            this.TransportAtDepartureCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.TransportAtDepartureCountryCodeFindBox.ParentType = null;
            this.TransportAtDepartureCountryCodeFindBox.PreBoundMaxLength = 2;
            this.TransportAtDepartureCountryCodeFindBox.ShowDescriptionBox = false;
            this.TransportAtDepartureCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 18, true);
            this.TransportAtDepartureCountryCodeFindBox.TabIndex = 2;
            // 
            // TransportAtDepartureTrailer1RegNoTextBox
            // 
            this.BindingSource.SetBindingMember(this.TransportAtDepartureTrailer1RegNoTextBox, "Trailer1IDAtDeparture");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.Interfaces.IDepartureTransportMeansProvider)(null)).Trailer1IDAtDeparture)));
            this.TransportAtDepartureTrailer1RegNoTextBox.CaptionResourceString = ((CargoWiseOne.ResourceStrings.ResourceStringData)(resources.GetObject("TransportAtDepartureTrailer1RegNoTextBox.CaptionResourceString")));
            this.TransportAtDepartureTrailer1RegNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 95, true);
            this.TransportAtDepartureTrailer1RegNoTextBox.Name = "TransportAtDepartureTrailer1RegNoTextBox";
            this.TransportAtDepartureTrailer1RegNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 18, true);
            this.TransportAtDepartureTrailer1RegNoTextBox.TabIndex = 3;
            // 
            // TransportAtDepartureTrailer1NationalityCodeFindBox
            // 
            this.TransportAtDepartureTrailer1NationalityCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TransportAtDepartureTrailer1NationalityCodeFindBox, "Trailer1NationalityAtDeparture");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.Interfaces.IDepartureTransportMeansProvider)(null)).Trailer1NationalityAtDeparture)));
            this.TransportAtDepartureTrailer1NationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 121, true);
            this.TransportAtDepartureTrailer1NationalityCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
            this.TransportAtDepartureTrailer1NationalityCodeFindBox.Name = "TransportAtDepartureTrailer1NationalityCodeFindBox";
            this.TransportAtDepartureTrailer1NationalityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.TransportAtDepartureTrailer1NationalityCodeFindBox.ParentType = null;
            this.TransportAtDepartureTrailer1NationalityCodeFindBox.PreBoundMaxLength = 2;
            this.TransportAtDepartureTrailer1NationalityCodeFindBox.ShowDescriptionBox = false;
            this.TransportAtDepartureTrailer1NationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 18, true);
            this.TransportAtDepartureTrailer1NationalityCodeFindBox.TabIndex = 4;
            // 
            // TransportAtDepartureTrailer2RegNoTextBox
            // 
            this.BindingSource.SetBindingMember(this.TransportAtDepartureTrailer2RegNoTextBox, "Trailer2IDAtDeparture");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.Interfaces.IDepartureTransportMeansProvider)(null)).Trailer2IDAtDeparture)));
            this.TransportAtDepartureTrailer2RegNoTextBox.CaptionResourceString = ((CargoWiseOne.ResourceStrings.ResourceStringData)(resources.GetObject("TransportAtDepartureTrailer2RegNoTextBox.CaptionResourceString")));
            this.TransportAtDepartureTrailer2RegNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 147, true);
            this.TransportAtDepartureTrailer2RegNoTextBox.Name = "TransportAtDepartureTrailer2RegNoTextBox";
            this.TransportAtDepartureTrailer2RegNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 18, true);
            this.TransportAtDepartureTrailer2RegNoTextBox.TabIndex = 5;
            // 
            // TransportAtDepartureTrailer2NationalityCodeFindBox
            // 
            this.TransportAtDepartureTrailer2NationalityCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TransportAtDepartureTrailer2NationalityCodeFindBox, "Trailer2NationalityAtDeparture");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.Interfaces.IDepartureTransportMeansProvider)(null)).Trailer2NationalityAtDeparture)));
            this.TransportAtDepartureTrailer2NationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 173, true);
            this.TransportAtDepartureTrailer2NationalityCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
            this.TransportAtDepartureTrailer2NationalityCodeFindBox.Name = "TransportAtDepartureTrailer2NationalityCodeFindBox";
            this.TransportAtDepartureTrailer2NationalityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.TransportAtDepartureTrailer2NationalityCodeFindBox.ParentType = null;
            this.TransportAtDepartureTrailer2NationalityCodeFindBox.PreBoundMaxLength = 2;
            this.TransportAtDepartureTrailer2NationalityCodeFindBox.ShowDescriptionBox = false;
            this.TransportAtDepartureTrailer2NationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 18, true);
            this.TransportAtDepartureTrailer2NationalityCodeFindBox.TabIndex = 6;
            // 
            // AircraftIDAtDepartureTextBox
            // 
            this.BindingSource.SetBindingMember(this.AircraftIDAtDepartureTextBox, "AircraftIDAtDeparture");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.Interfaces.IDepartureTransportMeansProvider)(null)).AircraftIDAtDeparture)));
            this.AircraftIDAtDepartureTextBox.CaptionResourceString = ((CargoWiseOne.ResourceStrings.ResourceStringData)(resources.GetObject("AircraftIDAtDepartureTextBox.CaptionResourceString")));
            this.AircraftIDAtDepartureTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 199, true);
            this.AircraftIDAtDepartureTextBox.Name = "AircraftIDAtDepartureTextBox";
            this.AircraftIDAtDepartureTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 18, true);
            this.AircraftIDAtDepartureTextBox.TabIndex = 7;
            // 
            // TransportAtDepartureTypeDropEdit
            // 
            this.TransportAtDepartureTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TransportAtDepartureTypeDropEdit, "TransportTypeAtDeparture");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.Interfaces.IDepartureTransportMeansProvider)(null)).TransportTypeAtDeparture)));
            this.TransportAtDepartureTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 225, true);
            this.TransportAtDepartureTypeDropEdit.Name = "TransportAtDepartureTypeDropEdit";
            this.TransportAtDepartureTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
            this.TransportAtDepartureTypeDropEdit.TabIndex = 8;
            // 
            // VesselCodeFindBox
            // 
            this.VesselCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.VesselCodeFindBox, "VesselNameAtDeparture");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.Interfaces.IDepartureTransportMeansProvider)(null)).VesselNameAtDeparture)));
            this.VesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 251, true);
            this.VesselCodeFindBox.Name = "VesselCodeFindBox";
            this.VesselCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.VesselCodeFindBox.ParentType = null;
            this.VesselCodeFindBox.ShowDescriptionBox = false;
            this.VesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 18, true);
            this.VesselCodeFindBox.TabIndex = 9;
            // 
            // VesselCountryCodeFindBox
            // 
            this.VesselCountryCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.VesselCountryCodeFindBox, "VesselCountryAtDeparture");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.Interfaces.IDepartureTransportMeansProvider)(null)).VesselCountryAtDeparture)));
            this.VesselCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 277, true);
            this.VesselCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
            this.VesselCountryCodeFindBox.Name = "VesselCountryCodeFindBox";
            this.VesselCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.VesselCountryCodeFindBox.ParentType = null;
            this.VesselCountryCodeFindBox.PreBoundMaxLength = 2;
            this.VesselCountryCodeFindBox.ShowDescriptionBox = false;
            this.VesselCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 18, true);
            this.VesselCountryCodeFindBox.TabIndex = 10;
            // 
            // AdditionalWagonNumbersButton
            // 
            this.AdditionalWagonNumbersButton.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("d0daacff-0727-4c1d-822a-7899af5da659", "More..");
            this.AdditionalWagonNumbersButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 303, true);
            this.AdditionalWagonNumbersButton.Name = "AdditionalWagonNumbersButton";
            this.AdditionalWagonNumbersButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
            this.AdditionalWagonNumbersButton.TabIndex = 11;
            this.AdditionalWagonNumbersButton.ToolTipCaption = null;
            this.AdditionalWagonNumbersButton.Click += new System.EventHandler(this.AdditionalWagonNumbersButton_Click);
            // 
            // PlaceHolderLabel
            // 
            this.PlaceHolderLabel.AutoSize = true;
            this.PlaceHolderLabel.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("2339f4c1-5534-4f08-8cbd-3da3541aed04", "Caption");
            this.PlaceHolderLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.PlaceHolderLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.PlaceHolderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(418, 20, true);
            this.PlaceHolderLabel.Name = "PlaceHolderLabel";
            this.PlaceHolderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 14, true);
            this.PlaceHolderLabel.TabIndex = 12;
            // 
            // PlaceHolder2Label
            // 
            this.PlaceHolder2Label.AutoSize = true;
            this.PlaceHolder2Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.PlaceHolder2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 310, true);
            this.PlaceHolder2Label.Name = "PlaceHolder2Label";
            this.PlaceHolder2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
            this.PlaceHolder2Label.TabIndex = 13;
            // 
            // TransportDepartureUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.PlaceHolder2Label);
            this.Controls.Add(this.PlaceHolderLabel);
            this.Controls.Add(this.InlandTransportModeDropEdit);
            this.Controls.Add(this.TransportAtDepartureTextBox);
            this.Controls.Add(this.TransportAtDepartureCountryCodeFindBox);
            this.Controls.Add(this.TransportAtDepartureTrailer1RegNoTextBox);
            this.Controls.Add(this.TransportAtDepartureTrailer1NationalityCodeFindBox);
            this.Controls.Add(this.TransportAtDepartureTrailer2RegNoTextBox);
            this.Controls.Add(this.TransportAtDepartureTrailer2NationalityCodeFindBox);
            this.Controls.Add(this.AircraftIDAtDepartureTextBox);
            this.Controls.Add(this.TransportAtDepartureTypeDropEdit);
            this.Controls.Add(this.VesselCodeFindBox);
            this.Controls.Add(this.VesselCountryCodeFindBox);
            this.Controls.Add(this.AdditionalWagonNumbersButton);
            this.Name = "TransportDepartureUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(559, 343, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.InlandTransportModeDropEdit.ResumeLayout(true);
            this.InlandTransportModeDropEdit.PerformLayout();
            this.TransportAtDepartureCountryCodeFindBox.ResumeLayout(true);
            this.TransportAtDepartureCountryCodeFindBox.PerformLayout();
            this.TransportAtDepartureTrailer1NationalityCodeFindBox.ResumeLayout(true);
            this.TransportAtDepartureTrailer1NationalityCodeFindBox.PerformLayout();
            this.TransportAtDepartureTrailer2NationalityCodeFindBox.ResumeLayout(true);
            this.TransportAtDepartureTrailer2NationalityCodeFindBox.PerformLayout();
            this.TransportAtDepartureTypeDropEdit.ResumeLayout(true);
            this.TransportAtDepartureTypeDropEdit.PerformLayout();
            this.VesselCodeFindBox.ResumeLayout(true);
            this.VesselCodeFindBox.PerformLayout();
            this.VesselCountryCodeFindBox.ResumeLayout(true);
            this.VesselCountryCodeFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit InlandTransportModeDropEdit;
		internal ZArchitecture.ZTextBox TransportAtDepartureTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox TransportAtDepartureCountryCodeFindBox;
		internal ZArchitecture.ZTextBox TransportAtDepartureTrailer1RegNoTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox TransportAtDepartureTrailer1NationalityCodeFindBox;
		internal ZArchitecture.ZTextBox TransportAtDepartureTrailer2RegNoTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox TransportAtDepartureTrailer2NationalityCodeFindBox;
		internal ZArchitecture.ZTextBox AircraftIDAtDepartureTextBox;
		internal ZArchitecture.GUI.ZDropEdit TransportAtDepartureTypeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox VesselCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox VesselCountryCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZButton AdditionalWagonNumbersButton;
		internal Enterprise.ZArchitecture.ZLabel PlaceHolderLabel;
		internal ZArchitecture.ZLabel PlaceHolder2Label;
	}
}

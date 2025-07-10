namespace Enterprise.Customs.DE.GUI
{
	partial class InwardProcessingUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			this.SimplifiedGrantAuthorizationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AuthorizationNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InwardProcessingAdditionalInformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InwardProcessingDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CompletionCustomsOfficesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CompletionCustomsOfficesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CompletionDurationCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.InwardProcessingPlacesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.InwardProcessingPlacesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MainAccountingDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SimplifiedGrantAuthorizationDropEdit.SuspendLayout();
			this.AuthorizationNumberDropEdit.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.CompletionCustomsOfficesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CompletionCustomsOfficesGrid)).BeginInit();
			this.CompletionCustomsOfficesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InwardProcessingPlacesGrid)).BeginInit();
			this.InwardProcessingPlacesGrid.SuspendLayout();
			this.InwardProcessingPlacesGroupBox.SuspendLayout();
			this.MainAccountingDocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.JobDeclaration);
			// 
			// SimplifiedGrantAuthorizationDropEdit
			// 
			this.SimplifiedGrantAuthorizationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SimplifiedGrantAuthorizationDropEdit, "CustomsEntryInstructions.CEI_SimplifiedGrantAuthorization");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_SimplifiedGrantAuthorization)));
			this.SimplifiedGrantAuthorizationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 19, true);
			this.SimplifiedGrantAuthorizationDropEdit.Name = "SimplifiedGrantAuthorizationDropEdit";
			this.SimplifiedGrantAuthorizationDropEdit.ShouldResizeByMaxLength = true;
			this.SimplifiedGrantAuthorizationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 17, true);
			this.SimplifiedGrantAuthorizationDropEdit.TabIndex = 0;
			// 
			// AuthorizationNumberDropEdit
			// 
			this.AuthorizationNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorizationNumberDropEdit, "CustomsEntryInstructions.CEI_AuthorisationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_AuthorisationNumber)));
			this.AuthorizationNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 40, true);
			this.AuthorizationNumberDropEdit.Name = "AuthorizationNumberDropEdit";
			this.AuthorizationNumberDropEdit.ShouldResizeByMaxLength = true;
			this.AuthorizationNumberDropEdit.ShowDescriptionBox = false;
			this.AuthorizationNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 17, true);
			this.AuthorizationNumberDropEdit.TabIndex = 1;
			this.AuthorizationNumberDropEdit.UseFullWidthForCodeBox = true;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "CustomsEntryInstructions.CEI_CriteriaType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_CriteriaType)));
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 83, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.ShouldResizeByMaxLength = true;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 17, true);
			this.zDropEdit1.TabIndex = 3;
			// 
			// InwardProcessingAdditionalInformationTextBox
			// 
			this.BindingSource.SetBindingMember(this.InwardProcessingAdditionalInformationTextBox, "CustomsEntryInstructions.CEI_InwardProcessingAdditionalInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_InwardProcessingAdditionalInformation)));
			this.InwardProcessingAdditionalInformationTextBox.CaptionResourceString = null;
			this.InwardProcessingAdditionalInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 104, true);
			this.InwardProcessingAdditionalInformationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.InwardProcessingAdditionalInformationTextBox.Multiline = true;
			this.InwardProcessingAdditionalInformationTextBox.Name = "InwardProcessingAdditionalInformationTextBox";
			this.InwardProcessingAdditionalInformationTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.InwardProcessingAdditionalInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 120, true);
			this.InwardProcessingAdditionalInformationTextBox.TabIndex = 4;
			// 
			// InwardProcessingDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.InwardProcessingDescriptionTextBox, "CustomsEntryInstructions.CEI_InwardProcessingDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_InwardProcessingDescription)));
			this.InwardProcessingDescriptionTextBox.CaptionResourceString = null;
			this.InwardProcessingDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 228, true);
			this.InwardProcessingDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.InwardProcessingDescriptionTextBox.Multiline = true;
			this.InwardProcessingDescriptionTextBox.Name = "InwardProcessingDescriptionTextBox";
			this.InwardProcessingDescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.InwardProcessingDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 120, true);
			this.InwardProcessingDescriptionTextBox.TabIndex = 5;
			// 
			// CompletionCustomsOfficesGroupBox
			// 
			this.CompletionCustomsOfficesGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("b8eca90f-4229-48a6-939f-1fa38ef039c0", "Completion Customs Offices");
			this.CompletionCustomsOfficesGroupBox.Controls.Add(this.CompletionCustomsOfficesGrid);
			this.CompletionCustomsOfficesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 147, true);
			this.CompletionCustomsOfficesGroupBox.Name = "CompletionCustomsOfficesGroupBox";
			this.CompletionCustomsOfficesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 127, true);
			this.CompletionCustomsOfficesGroupBox.TabIndex = 8;
			this.CompletionCustomsOfficesGroupBox.TabStop = false;
			this.CompletionCustomsOfficesGroupBox.Text = "Completion Customs Offices";
			// 
			// CompletionCustomsOfficesGrid
			// 
			this.CompletionCustomsOfficesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CompletionCustomsOfficesGrid, "CustomsEntryInstructions.CompletionCustomsOffices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CompletionCustomsOffices)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CompletionCustomsOffice)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CompletionCustomsOffices)).SyncRoot)).CY_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CompletionCustomsOffice)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CompletionCustomsOffices)).SyncRoot)).CY_OfficeDescription)));
			this.CompletionCustomsOfficesGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiLineTextBoxColumnInfo1.ColumnName = "CY_OfficeDescription";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.CompletionCustomsOfficesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CompletionCustomsOfficesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.CompletionCustomsOfficesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CompletionCustomsOfficesGrid.GridId = "9037bbe9-287b-458e-bc71-6d83999b0983";
			this.CompletionCustomsOfficesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CompletionCustomsOfficesGrid.LayoutKey = "InwardProcessingPlacesGrid";
			this.CompletionCustomsOfficesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.CompletionCustomsOfficesGrid.Name = "CompletionCustomsOfficesGrid";
			this.CompletionCustomsOfficesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 110, true);
			this.CompletionCustomsOfficesGrid.TabIndex = 0;
			// 
			// CompletionDurationCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CompletionDurationCalcEdit, "CustomsEntryInstructions.CEI_CompletionDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_CompletionDuration)));
			this.CompletionDurationCalcEdit.CaptionResourceString = null;
			this.CompletionDurationCalcEdit.DecimalPlaces = 0;
			this.CompletionDurationCalcEdit.Decimals = 0;
			this.CompletionDurationCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 61, true);
			this.CompletionDurationCalcEdit.Name = "CompletionDurationCalcEdit";
			this.CompletionDurationCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 17, true);
			this.CompletionDurationCalcEdit.TabIndex = 2;
			this.CompletionDurationCalcEdit.Text = "0";
			this.CompletionDurationCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// InwardProcessingPlacesGrid
			// 
			this.InwardProcessingPlacesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InwardProcessingPlacesGrid, "CustomsEntryInstructions.InwardProcessingPlaces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).InwardProcessingPlaces)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.Declaration.InwardProcessingPlace)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).InwardProcessingPlaces)).SyncRoot)).E2_AddressSequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.Declaration.InwardProcessingPlace)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).InwardProcessingPlaces)).SyncRoot)).OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.Declaration.InwardProcessingPlace)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).InwardProcessingPlaces)).SyncRoot)).E2_OA_Address)));
			this.InwardProcessingPlacesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "E2_AddressSequence";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "OrganisationPK";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zAddressDropEditColumnStyleInfo1.ColumnName = "E2_OA_Address";
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.InwardProcessingPlacesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.InwardProcessingPlacesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.InwardProcessingPlacesGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.InwardProcessingPlacesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InwardProcessingPlacesGrid.GridId = "f3af862e-24f9-450f-845f-e66e81489f5e";
			this.InwardProcessingPlacesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InwardProcessingPlacesGrid.LayoutKey = "InwardProcessingPlacesGrid";
			this.InwardProcessingPlacesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.InwardProcessingPlacesGrid.Name = "InwardProcessingPlacesGrid";
			this.InwardProcessingPlacesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 105, true);
			this.InwardProcessingPlacesGrid.TabIndex = 8;
			// 
			// InwardProcessingPlacesGroupBox
			// 
			this.InwardProcessingPlacesGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("b12d41b4-4f44-4b13-9589-e9c5e63510d3", "Inward Processing Places");
			this.InwardProcessingPlacesGroupBox.Controls.Add(this.InwardProcessingPlacesGrid);
			this.InwardProcessingPlacesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 19, true);
			this.InwardProcessingPlacesGroupBox.Name = "InwardProcessingPlacesGroupBox";
			this.InwardProcessingPlacesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 122, true);
			this.InwardProcessingPlacesGroupBox.TabIndex = 7;
			this.InwardProcessingPlacesGroupBox.TabStop = false;
			this.InwardProcessingPlacesGroupBox.Text = "Inward Processing Places";
			// 
			// MainAccountingDocAddressControl
			// 
			this.MainAccountingDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MainAccountingDocAddressControl, "CustomsEntryInstructions.MainAccountingAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).MainAccountingAddress)));
			this.MainAccountingDocAddressControl.BindToOrganisations = "CustomsEntryInstructions.Lookups.MainAccountingOrganisations";
			this.MainAccountingDocAddressControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("5e80440d-b4c4-4569-9dc3-68553d74c01a", "Main Accounting");
			this.MainAccountingDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.HideOverrideAndTabs;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MainAccountingDocAddressControl, false);
			this.MainAccountingDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 279, true);
			this.MainAccountingDocAddressControl.Name = "MainAccountingDocAddressControl";
			this.MainAccountingDocAddressControl.ReadOnly = false;
			this.MainAccountingDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.MainAccountingDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 68, true);
			this.MainAccountingDocAddressControl.TabIndex = 9;
			this.MainAccountingDocAddressControl.ValidationJustForced = false;
			// 
			// InwardProcessingUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainAccountingDocAddressControl);
			this.Controls.Add(this.InwardProcessingPlacesGroupBox);
			this.Controls.Add(this.CompletionDurationCalcEdit);
			this.Controls.Add(this.CompletionCustomsOfficesGroupBox);
			this.Controls.Add(this.InwardProcessingDescriptionTextBox);
			this.Controls.Add(this.InwardProcessingAdditionalInformationTextBox);
			this.Controls.Add(this.zDropEdit1);
			this.Controls.Add(this.AuthorizationNumberDropEdit);
			this.Controls.Add(this.SimplifiedGrantAuthorizationDropEdit);
			this.Name = "InwardProcessingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 360, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SimplifiedGrantAuthorizationDropEdit.ResumeLayout(true);
			this.SimplifiedGrantAuthorizationDropEdit.PerformLayout();
			this.AuthorizationNumberDropEdit.ResumeLayout(true);
			this.AuthorizationNumberDropEdit.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.CompletionCustomsOfficesGroupBox.ResumeLayout(false);
			this.CompletionCustomsOfficesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CompletionCustomsOfficesGrid)).EndInit();
			this.CompletionCustomsOfficesGrid.ResumeLayout(false);
			this.CompletionCustomsOfficesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InwardProcessingPlacesGrid)).EndInit();
			this.InwardProcessingPlacesGrid.ResumeLayout(false);
			this.InwardProcessingPlacesGrid.PerformLayout();
			this.InwardProcessingPlacesGroupBox.ResumeLayout(false);
			this.InwardProcessingPlacesGroupBox.PerformLayout();
			this.MainAccountingDocAddressControl.ResumeLayout(true);
			this.MainAccountingDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit SimplifiedGrantAuthorizationDropEdit;
		private ZArchitecture.GUI.ZDropEdit AuthorizationNumberDropEdit;
		private ZArchitecture.GUI.ZDropEdit zDropEdit1;
		private ZArchitecture.ZTextBox InwardProcessingAdditionalInformationTextBox;
		private ZArchitecture.ZTextBox InwardProcessingDescriptionTextBox;
		private ZArchitecture.GUI.ZGroupBox CompletionCustomsOfficesGroupBox;
		private ZArchitecture.ZGrid CompletionCustomsOfficesGrid;
		private ZArchitecture.ZCalcEdit CompletionDurationCalcEdit;
		private ZArchitecture.ZGrid InwardProcessingPlacesGrid;
		private ZArchitecture.GUI.ZGroupBox InwardProcessingPlacesGroupBox;
		private MasterFiles.GUI.ZDocAddressControl MainAccountingDocAddressControl;
	}
}

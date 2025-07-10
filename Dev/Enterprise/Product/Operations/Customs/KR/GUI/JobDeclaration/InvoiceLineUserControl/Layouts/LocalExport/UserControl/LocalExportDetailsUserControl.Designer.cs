
namespace Enterprise.Customs.KR.GUI
{
	partial class LocalExportDetailsUserControl
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
			this.components = new System.ComponentModel.Container();
			this.DescriptionLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
			this.TariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.PartNoCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zBindingSource1 = new Enterprise.ZArchitecture.GUI.ZBindingSource(this.components);
			this.SupportingDocumentCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SupportingDocumentReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OriginalStateDocTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PreviousEntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InboundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.IngredientTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SerialNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DescriptionLongTextControl.SuspendLayout();
			this.TariffFindBox.SuspendLayout();
			this.PartNoCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zBindingSource1)).BeginInit();
			this.SupportingDocumentCodeDropEdit.SuspendLayout();
			this.OriginalStateDocTypeDropEdit.SuspendLayout();
			this.InboundDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceLine);
			// 
			// DescriptionLongTextControl
			// 
			this.DescriptionLongTextControl.AllowDrop = true;
			this.DescriptionLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.DescriptionLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 49, true);
			this.DescriptionLongTextControl.Name = "DescriptionLongTextControl";
			this.DescriptionLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 20, true);
			this.DescriptionLongTextControl.TabIndex = 10;
			// 
			// TariffFindBox
			// 
			this.TariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffFindBox, "JI_FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_FormattedTariff)));
			this.TariffFindBox.ErrorForUnsupportedCountry = null;
			this.TariffFindBox.GetEffectiveDate = null;
			this.TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 27, true);
			this.TariffFindBox.Name = "TariffFindBox";
			this.TariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TariffFindBox.ParentType = null;
			this.TariffFindBox.PreBoundMaxLength = 30;
			this.TariffFindBox.SelectNomenclatureModes = null;
			this.TariffFindBox.ShouldResize = false;
			this.TariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 15, true);
			this.TariffFindBox.TabIndex = 8;
			this.TariffFindBox.TariffType = null;
			// 
			// PartNoCodeFindBox
			// 
			this.PartNoCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PartNoCodeFindBox, "JI_PartNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_PartNo)));
			this.PartNoCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 4, true);
			this.PartNoCodeFindBox.Name = "PartNoCodeFindBox";
			this.PartNoCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PartNoCodeFindBox.ParentType = null;
			this.PartNoCodeFindBox.PreBoundMaxLength = 30;
			this.PartNoCodeFindBox.ShouldResize = false;
			this.PartNoCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 15, true);
			this.PartNoCodeFindBox.TabIndex = 7;
			// 
			// zBindingSource1
			// 
			this.zBindingSource1.ContainerControl = this;
			this.zBindingSource1.DataSourceType = null;
			// 
			// SupportingDocumentCodeDropEdit
			// 
			this.SupportingDocumentCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupportingDocumentCodeDropEdit, "SupportingDocumentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).SupportingDocumentCode)));
			this.SupportingDocumentCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 73, true);
			this.SupportingDocumentCodeDropEdit.Name = "SupportingDocumentCodeDropEdit";
			this.SupportingDocumentCodeDropEdit.PreBoundMaxLength = 2;
			this.SupportingDocumentCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 15, true);
			this.SupportingDocumentCodeDropEdit.TabIndex = 11;
			// 
			// SupportingDocumentReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.SupportingDocumentReferenceNumberTextBox, "SupportingDocumentReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).SupportingDocumentReferenceNumber)));
			this.SupportingDocumentReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SupportingDocumentReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(497, 73, true);
			this.SupportingDocumentReferenceNumberTextBox.Name = "SupportingDocumentReferenceNumberTextBox";
			this.SupportingDocumentReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 15, true);
			this.SupportingDocumentReferenceNumberTextBox.TabIndex = 12;
			// 
			// OriginalStateDocTypeDropEdit
			// 
			this.OriginalStateDocTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginalStateDocTypeDropEdit, "JI_OriginalStateDocType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_OriginalStateDocType)));
			this.OriginalStateDocTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 96, true);
			this.OriginalStateDocTypeDropEdit.Name = "OriginalStateDocTypeDropEdit";
			this.OriginalStateDocTypeDropEdit.PreBoundMaxLength = 2;
			this.OriginalStateDocTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 15, true);
			this.OriginalStateDocTypeDropEdit.TabIndex = 13;
			// 
			// PreviousEntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreviousEntryNumberTextBox, "JI_PreviousEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_PreviousEntryNumber)));
			this.PreviousEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(497, 96, true);
			this.PreviousEntryNumberTextBox.Name = "PreviousEntryNumberTextBox";
			this.PreviousEntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 15, true);
			this.PreviousEntryNumberTextBox.TabIndex = 14;
			// 
			// InboundDateEdit
			// 
			this.InboundDateEdit.AllowDrop = true;
			this.InboundDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.InboundDateEdit, "JI_InboundDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_InboundDate)));
			this.InboundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 119, true);
			this.InboundDateEdit.Name = "InboundDateEdit";
			this.InboundDateEdit.TabIndex = 15;
			// 
			// IngredientTextBox
			// 
			this.BindingSource.SetBindingMember(this.IngredientTextBox, "JI_Ingredient");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_Ingredient)));
			this.IngredientTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.IngredientTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 141, true);
			this.IngredientTextBox.Name = "IngredientTextBox";
			this.IngredientTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 15, true);
			this.IngredientTextBox.TabIndex = 16;
			// 
			// SerialNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.SerialNumberTextBox, "JI_SerialNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_SerialNumber)));
			this.SerialNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(497, 141, true);
			this.SerialNumberTextBox.Name = "SerialNumberTextBox";
			this.SerialNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 15, true);
			this.SerialNumberTextBox.TabIndex = 17;
			// 
			// LocalExportDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SerialNumberTextBox);
			this.Controls.Add(this.IngredientTextBox);
			this.Controls.Add(this.InboundDateEdit);
			this.Controls.Add(this.PreviousEntryNumberTextBox);
			this.Controls.Add(this.OriginalStateDocTypeDropEdit);
			this.Controls.Add(this.SupportingDocumentReferenceNumberTextBox);
			this.Controls.Add(this.SupportingDocumentCodeDropEdit);
			this.Controls.Add(this.DescriptionLongTextControl);
			this.Controls.Add(this.TariffFindBox);
			this.Controls.Add(this.PartNoCodeFindBox);
			this.Name = "LocalExportDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 165, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DescriptionLongTextControl.ResumeLayout(true);
			this.DescriptionLongTextControl.PerformLayout();
			this.TariffFindBox.ResumeLayout(true);
			this.TariffFindBox.PerformLayout();
			this.PartNoCodeFindBox.ResumeLayout(true);
			this.PartNoCodeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zBindingSource1)).EndInit();
			this.SupportingDocumentCodeDropEdit.ResumeLayout(true);
			this.SupportingDocumentCodeDropEdit.PerformLayout();
			this.OriginalStateDocTypeDropEdit.ResumeLayout(true);
			this.OriginalStateDocTypeDropEdit.PerformLayout();
			this.InboundDateEdit.ResumeLayout(true);
			this.InboundDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public Customs.GUI.LongTextControl DescriptionLongTextControl;
		private Universal.GUI.TariffFindBox TariffFindBox;
		public ZArchitecture.GUI.ZCodeFindBox PartNoCodeFindBox;
		private ZArchitecture.GUI.ZBindingSource zBindingSource1;
		private ZArchitecture.ZTextBox SerialNumberTextBox;
		private ZArchitecture.ZTextBox IngredientTextBox;
		private ZArchitecture.GUI.ZDateEdit InboundDateEdit;
		private ZArchitecture.ZTextBox PreviousEntryNumberTextBox;
		private ZArchitecture.GUI.ZDropEdit OriginalStateDocTypeDropEdit;
		private ZArchitecture.ZTextBox SupportingDocumentReferenceNumberTextBox;
		private ZArchitecture.GUI.ZDropEdit SupportingDocumentCodeDropEdit;
	}
}

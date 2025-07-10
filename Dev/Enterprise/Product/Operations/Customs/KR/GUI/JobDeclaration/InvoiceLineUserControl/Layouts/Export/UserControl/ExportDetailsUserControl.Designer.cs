
namespace Enterprise.Customs.KR.GUI
{
	partial class ExportDetailsUserControl
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
			this.ModelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BrandNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
			this.IngredientLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
			this.LotNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.PartNoCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DescriptionLongTextControl.SuspendLayout();
			this.IngredientLongTextControl.SuspendLayout();
			this.TariffFindBox.SuspendLayout();
			this.PartNoCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceLine);
			// 
			// ModelTextBox
			// 
			this.BindingSource.SetBindingMember(this.ModelTextBox, "JI_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_Model)));
			this.ModelTextBox.CaptionResourceString = null;
			this.ModelTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ModelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 103, true);
			this.ModelTextBox.Name = "ModelTextBox";
			this.ModelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 19, true);
			this.ModelTextBox.TabIndex = 12;
			// 
			// BrandNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.BrandNameTextBox, "JI_BrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_BrandName)));
			this.BrandNameTextBox.CaptionResourceString = null;
			this.BrandNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BrandNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 77, true);
			this.BrandNameTextBox.Name = "BrandNameTextBox";
			this.BrandNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 19, true);
			this.BrandNameTextBox.TabIndex = 11;
			// 
			// DescriptionLongTextControl
			// 
			this.DescriptionLongTextControl.AllowDrop = true;
			this.DescriptionLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.DescriptionLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 50, true);
			this.DescriptionLongTextControl.Name = "DescriptionLongTextControl";
			this.DescriptionLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 20, true);
			this.DescriptionLongTextControl.TabIndex = 10;
			// 
			// IngredientLongTextControl
			// 
			this.IngredientLongTextControl.AllowDrop = true;
			this.IngredientLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.IngredientLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 130, true);
			this.IngredientLongTextControl.Name = "IngredientLongTextControl";
			this.IngredientLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 22, true);
			this.IngredientLongTextControl.TabIndex = 13;
			// 
			// LotNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.LotNumberTextBox, "JI_LotNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_LotNumber)));
			this.LotNumberTextBox.CaptionResourceString = null;
			this.LotNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(478, 26, true);
			this.LotNumberTextBox.Name = "LotNumberTextBox";
			this.LotNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 19, true);
			this.LotNumberTextBox.TabIndex = 9;
			// 
			// TariffFindBox
			// 
			this.TariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffFindBox, "JI_FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_FormattedTariff)));
			this.TariffFindBox.ErrorForUnsupportedCountry = null;
			this.TariffFindBox.GetEffectiveDate = null;
			this.TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 26, true);
			this.TariffFindBox.Name = "TariffFindBox";
			this.TariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TariffFindBox.ParentType = null;
			this.TariffFindBox.PreBoundMaxLength = 30;
			this.TariffFindBox.SelectNomenclatureModes = null;
			this.TariffFindBox.ShouldResize = false;
			this.TariffFindBox.ShowDescriptionBox = false;
			this.TariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 19, true);
			this.TariffFindBox.TabIndex = 8;
			this.TariffFindBox.TariffType = null;
			// 
			// PartNoCodeFindBox
			// 
			this.PartNoCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PartNoCodeFindBox, "JI_PartNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_PartNo)));
			this.PartNoCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 0, true);
			this.PartNoCodeFindBox.Name = "PartNoCodeFindBox";
			this.PartNoCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PartNoCodeFindBox.ParentType = null;
			this.PartNoCodeFindBox.PreBoundMaxLength = 30;
			this.PartNoCodeFindBox.ShouldResize = false;
			this.PartNoCodeFindBox.ShowDescriptionBox = false;
			this.PartNoCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 19, true);
			this.PartNoCodeFindBox.TabIndex = 7;
			// 
			// ExportDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ModelTextBox);
			this.Controls.Add(this.BrandNameTextBox);
			this.Controls.Add(this.DescriptionLongTextControl);
			this.Controls.Add(this.IngredientLongTextControl);
			this.Controls.Add(this.LotNumberTextBox);
			this.Controls.Add(this.TariffFindBox);
			this.Controls.Add(this.PartNoCodeFindBox);
			this.Name = "ExportDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 154, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DescriptionLongTextControl.ResumeLayout(true);
			this.DescriptionLongTextControl.PerformLayout();
			this.IngredientLongTextControl.ResumeLayout(true);
			this.IngredientLongTextControl.PerformLayout();
			this.TariffFindBox.ResumeLayout(true);
			this.TariffFindBox.PerformLayout();
			this.PartNoCodeFindBox.ResumeLayout(true);
			this.PartNoCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZTextBox ModelTextBox;
		public ZArchitecture.ZTextBox BrandNameTextBox;
		public Customs.GUI.LongTextControl DescriptionLongTextControl;
		public Customs.GUI.LongTextControl IngredientLongTextControl;
		private ZArchitecture.ZTextBox LotNumberTextBox;
		private Universal.GUI.TariffFindBox TariffFindBox;
		public ZArchitecture.GUI.ZCodeFindBox PartNoCodeFindBox;
	}
}

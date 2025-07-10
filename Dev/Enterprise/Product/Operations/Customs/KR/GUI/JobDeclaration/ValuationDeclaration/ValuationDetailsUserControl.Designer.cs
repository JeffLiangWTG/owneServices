namespace Enterprise.Customs.KR.GUI
{
	partial class ValuationDetailsUserControl
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
			this.ProductCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.DescriptionLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
			this.ModelTradeNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BrandNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IngredientLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ProductCodeCodeFindBox.SuspendLayout();
			this.TariffFindBox.SuspendLayout();
			this.DescriptionLongTextControl.SuspendLayout();
			this.IngredientLongTextControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// ProductCodeCodeFindBox
			// 
			this.ProductCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductCodeCodeFindBox, "FilteredInvoiceLines.JI_PartNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PartNo)));
			this.ProductCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ProductCodeCodeFindBox.Name = "ProductCodeCodeFindBox";
			this.ProductCodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ProductCodeCodeFindBox.ParentType = null;
			this.ProductCodeCodeFindBox.ShowDescriptionBox = false;
			this.ProductCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.ProductCodeCodeFindBox.TabIndex = 1;
			// 
			// TariffFindBox
			// 
			this.TariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffFindBox, "FilteredInvoiceLines.JI_FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_FormattedTariff)));
			this.TariffFindBox.ErrorForUnsupportedCountry = null;
			this.TariffFindBox.GetDataGrouping = null;
			this.TariffFindBox.GetEffectiveDate = null;
			this.TariffFindBox.GetTariffType = null;
			this.TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 31, true);
			this.TariffFindBox.Name = "TariffFindBox";
			this.TariffFindBox.NeedLoadNomenclatureWhenTariffNotFound = false;
			this.TariffFindBox.NeedLoadParentDataGroup = true;
			this.TariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TariffFindBox.ParentType = null;
			this.TariffFindBox.SelectNomenclatureModes = null;
			this.TariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 20, true);
			this.TariffFindBox.TabIndex = 2;
			this.TariffFindBox.TariffType = null;
			// 
			// DescriptionLongTextControl
			// 
			this.DescriptionLongTextControl.AllowDrop = true;
			this.DescriptionLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 61, true);
			this.DescriptionLongTextControl.Name = "DescriptionLongTextControl";
			this.DescriptionLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 22, true);
			this.DescriptionLongTextControl.TabIndex = 3;
			// 
			// ModelTradeNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ModelTradeNameTextBox, "FilteredInvoiceLines.JI_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Model)));
			this.ModelTradeNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ModelTradeNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 89, true);
			this.ModelTradeNameTextBox.Name = "ModelTradeNameTextBox";
			this.ModelTradeNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 20, true);
			this.ModelTradeNameTextBox.TabIndex = 4;
			// 
			// BrandNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.BrandNameTextBox, "FilteredInvoiceLines.JI_BrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_BrandName)));
			this.BrandNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BrandNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 115, true);
			this.BrandNameTextBox.Name = "BrandNameTextBox";
			this.BrandNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 20, true);
			this.BrandNameTextBox.TabIndex = 5;
			// 
			// IngredientLongTextControl
			// 
			this.IngredientLongTextControl.AllowDrop = true;
			this.IngredientLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.IngredientLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 141, true);
			this.IngredientLongTextControl.Name = "IngredientLongTextControl";
			this.IngredientLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 21, true);
			this.IngredientLongTextControl.TabIndex = 6;
			// 
			// ValuationDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IngredientLongTextControl);
			this.Controls.Add(this.BrandNameTextBox);
			this.Controls.Add(this.ModelTradeNameTextBox);
			this.Controls.Add(this.DescriptionLongTextControl);
			this.Controls.Add(this.TariffFindBox);
			this.Controls.Add(this.ProductCodeCodeFindBox);
			this.Name = "ValuationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(647, 178, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ProductCodeCodeFindBox.ResumeLayout(true);
			this.ProductCodeCodeFindBox.PerformLayout();
			this.TariffFindBox.ResumeLayout(true);
			this.TariffFindBox.PerformLayout();
			this.DescriptionLongTextControl.ResumeLayout(true);
			this.DescriptionLongTextControl.PerformLayout();
			this.IngredientLongTextControl.ResumeLayout(true);
			this.IngredientLongTextControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZCodeFindBox ProductCodeCodeFindBox;
		public Universal.GUI.TariffFindBox TariffFindBox;
		public Customs.GUI.LongTextControl DescriptionLongTextControl;
		public ZArchitecture.ZTextBox ModelTradeNameTextBox;
		public ZArchitecture.ZTextBox BrandNameTextBox;
		public Customs.GUI.LongTextControl IngredientLongTextControl;
	}
}

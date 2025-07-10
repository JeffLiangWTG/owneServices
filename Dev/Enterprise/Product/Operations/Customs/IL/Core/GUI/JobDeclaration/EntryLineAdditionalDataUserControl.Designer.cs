using CargoWise.Windows.UI;

namespace Enterprise.Customs.IL.GUI
{
	public partial class EntryLineAdditionalDataUserControl
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
		void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.DutyAndTaxDetails = new Enterprise.Customs.IL.GUI.EntryLineTaxAndConfirmedFeeUserControl();
            this.ExtendedInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.TariffCodePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.TariffCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ExtendInfoTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
            this.TaxOrFeeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.ExtendedInfoTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.DutyAndTaxDetails.SuspendLayout();
            this.ExtendedInfoGroupBox.SuspendLayout();
            this.TariffCodePanel.SuspendLayout();
            this.ExtendInfoTabControl.SuspendLayout();
            this.TaxOrFeeTabPage.SuspendLayout();
            this.ExtendedInfoTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Business.JobDeclaration);
            // 
            // DutyAndTaxDetails
            // 
            this.DutyAndTaxDetails.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DutyAndTaxDetails, "CustomsEntryHeaders.AllEntryLines");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.Business.IAllCusEntryLineCollection<Enterprise.Customs.IL.Business.CusEntryLine>)(((Enterprise.Customs.IL.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)));
            this.DutyAndTaxDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DutyAndTaxDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.DutyAndTaxDetails.Name = "DutyAndTaxDetails";
            this.DutyAndTaxDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 237, true);
            this.DutyAndTaxDetails.TabIndex = 0;
            // 
            // ExtendedInfoGroupBox
            // 
            this.ExtendedInfoGroupBox.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("0F96E572-56EE-46E7-B5F6-77EBE1A84672", "Extended Information");
            this.ExtendedInfoGroupBox.Controls.Add(this.TariffCodePanel);
            this.ExtendedInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExtendedInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ExtendedInfoGroupBox.Name = "ExtendedInfoGroupBox";
            this.ExtendedInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 243, true);
            this.ExtendedInfoGroupBox.TabIndex = 2;
            this.ExtendedInfoGroupBox.TabStop = false;
            // 
            // TariffCodePanel
            // 
            this.TariffCodePanel.Controls.Add(this.TariffCodeTextBox);
            this.TariffCodePanel.Controls.Add(this.DescriptionTextBox);
            this.TariffCodePanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.TariffCodePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.TariffCodePanel.Name = "TariffCodePanel";
            this.TariffCodePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 224, true);
            this.TariffCodePanel.TabIndex = 2;
            // 
            // TariffCodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.TariffCodeTextBox, "CustomsEntryHeaders.AllEntryLines.CL_AdValoremTariff");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CL_AdValoremTariff)));
            this.TariffCodeTextBox.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("0FDD54C7-3BBB-4F0F-8058-D7E9313D0CA2", "Tariff Code:");
            this.TariffCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 17, true);
            this.TariffCodeTextBox.Name = "TariffCodeTextBox";
            this.TariffCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 20, true);
            this.TariffCodeTextBox.TabIndex = 0;
            // 
            // DescriptionTextBox
            // 
            this.BindingSource.SetBindingMember(this.DescriptionTextBox, "CustomsEntryHeaders.AllEntryLines.EffectiveDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).EffectiveDescription)));
            this.DescriptionTextBox.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("90DF1A8B-45A1-4910-A0EC-5A3C5B1F9D8A", "Description:");
            this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 41, true);
            this.DescriptionTextBox.Multiline = true;
            this.DescriptionTextBox.Name = "DescriptionTextBox";
            this.DescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 137, true);
            this.DescriptionTextBox.TabIndex = 1;
            // 
            // ExtendInfoTabControl
            // 
            this.ExtendInfoTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.ExtendInfoTabControl.Controls.Add(this.TaxOrFeeTabPage);
            this.ExtendInfoTabControl.Controls.Add(this.ExtendedInfoTabPage);
            this.ExtendInfoTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExtendInfoTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ExtendInfoTabControl.Name = "ExtendInfoTabControl";
            this.ExtendInfoTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 270, true);
            this.ExtendInfoTabControl.TabIndex = 15;
            // 
            // TaxOrFeeTabPage
            // 
            this.TaxOrFeeTabPage.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("3B4E87E4-CBC7-41E9-8090-24F92E335A16", "Tax Or Fee");
            this.TaxOrFeeTabPage.Controls.Add(this.DutyAndTaxDetails);
            this.TaxOrFeeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.TaxOrFeeTabPage.Name = "TaxOrFeeTabPage";
            this.TaxOrFeeTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.TaxOrFeeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 243, true);
            this.TaxOrFeeTabPage.TabIndex = 1;
            this.TaxOrFeeTabPage.UseVisualStyleBackColor = true;
            // 
            // ExtendedInfoTabPage
            // 
            this.ExtendedInfoTabPage.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("AE0804F9-A2E1-4D3D-AA96-36B296AC429B", "Extended Information");
            this.ExtendedInfoTabPage.Controls.Add(this.ExtendedInfoGroupBox);
            this.ExtendedInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.ExtendedInfoTabPage.Name = "ExtendedInfoTabPage";
            this.ExtendedInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 243, true);
            this.ExtendedInfoTabPage.TabIndex = 2;
            // 
            // EntryLineAdditionalDataUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ExtendInfoTabControl);
            this.Name = "EntryLineAdditionalDataUserControl";
            this.ShouldSerializeTabPageMethods = false;
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 270, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.DutyAndTaxDetails.ResumeLayout(true);
            this.DutyAndTaxDetails.PerformLayout();
            this.ExtendedInfoGroupBox.ResumeLayout(false);
            this.ExtendedInfoGroupBox.PerformLayout();
            this.TariffCodePanel.ResumeLayout(false);
            this.TariffCodePanel.PerformLayout();
            this.ExtendInfoTabControl.ResumeLayout(false);
            this.ExtendInfoTabControl.PerformLayout();
            this.TaxOrFeeTabPage.ResumeLayout(false);
            this.TaxOrFeeTabPage.PerformLayout();
            this.ExtendedInfoTabPage.ResumeLayout(false);
            this.ExtendedInfoTabPage.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		public ZArchitecture.GUI.ZTemplateTabControl ExtendInfoTabControl;
		public ZArchitecture.GUI.ZTabPage ExtendedInfoTabPage;
		public ZArchitecture.GUI.ZTabPage TaxOrFeeTabPage;
		protected ZArchitecture.GUI.ZGroupBox ExtendedInfoGroupBox;
		public EntryLineTaxAndConfirmedFeeUserControl DutyAndTaxDetails;
		private ZArchitecture.GUI.ZPanel TariffCodePanel;
		private ZArchitecture.ZTextBox TariffCodeTextBox;
		private ZArchitecture.ZTextBox DescriptionTextBox;
	}
}

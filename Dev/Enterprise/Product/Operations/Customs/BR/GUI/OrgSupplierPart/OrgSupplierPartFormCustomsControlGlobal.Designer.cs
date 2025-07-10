namespace Enterprise.Customs.BR.GUI
{
	public partial class OrgSupplierPartFormCustomsControlGlobal
	{
		void InitializeComponent()
		{
			this.ComplementaryDescriptionTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.NveTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TariffDetailsLayout = new Enterprise.Customs.BR.GUI.OrgSupplierTariffDetailsUserControl();
			this.AttributesNcmTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AttributesGridLayout = new Enterprise.Customs.BR.GUI.AttributesUserControl();
			this.AdditionalTariffsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalTariffsGridLayout = new Enterprise.Customs.BR.GUI.AdditionalTariffsUserControl();
			this.detailsPanel.SuspendLayout();
			this.DetailTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.CI_UsageCommentTextBox.SuspendLayout();
			this.ClassificationDescriptionTextBox.SuspendLayout();
			this.AttributesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttributesLeftSplitContainer)).BeginInit();
			this.AttributesLeftSplitContainer.Panel1.SuspendLayout();
			this.AttributesLeftSplitContainer.Panel2.SuspendLayout();
			this.AttributesLeftSplitContainer.SuspendLayout();
			this.Attributes1GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes1Grid)).BeginInit();
			this.Attributes1Grid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttributesRightSplitContainer)).BeginInit();
			this.AttributesRightSplitContainer.Panel1.SuspendLayout();
			this.AttributesRightSplitContainer.Panel2.SuspendLayout();
			this.AttributesRightSplitContainer.SuspendLayout();
			this.Attributes2GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes2Grid)).BeginInit();
			this.Attributes2Grid.SuspendLayout();
			this.Attributes3GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes3Grid)).BeginInit();
			this.Attributes3Grid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PivotGrid)).BeginInit();
			this.PivotGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ComplementaryDescriptionTextBox.SuspendLayout();
			this.NveTabPage.SuspendLayout();
			this.TariffDetailsLayout.SuspendLayout();
			this.AttributesNcmTabPage.SuspendLayout();
			this.AttributesGridLayout.SuspendLayout();
			this.AdditionalTariffsTabPage.SuspendLayout();
			this.AdditionalTariffsGridLayout.SuspendLayout();
			this.SuspendLayout();
			//
			// detailsPanel
			//
			this.detailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 315, true);
			//
			// DetailTabControl
			//
			this.DetailTabControl.Controls.Add(this.AttributesNcmTabPage);
			this.DetailTabControl.Controls.Add(this.NveTabPage);
			this.DetailTabControl.Controls.Add(this.AdditionalTariffsTabPage);
			this.DetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 315, true);
			this.DetailTabControl.Controls.SetChildIndex(this.AdditionalTariffsTabPage, 0);
			this.DetailTabControl.Controls.SetChildIndex(this.NveTabPage, 0);
			this.DetailTabControl.Controls.SetChildIndex(this.AttributesNcmTabPage, 0);
			//
			// DetailsTabPage
			//
			this.DetailsTabPage.Controls.Add(this.ComplementaryDescriptionTextBox);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 155, true);
			this.DetailsTabPage.Controls.SetChildIndex(this.ClassificationDescriptionTextBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.CI_UsageCommentTextBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.ComplementaryDescriptionTextBox, 0);
			//
			// CI_UsageCommentTextBox
			//
			this.CI_UsageCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 9, true);
			this.CI_UsageCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			//
			// ClassificationDescriptionTextBox
			//
			this.ClassificationDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 37, true);
			this.ClassificationDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 20, true);
			//
			// AttributesTabPage
			//
			this.AttributesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 155, true);
			//
			// AttributesLeftSplitContainer
			//
			this.AttributesLeftSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 149, true);
			//
			// Attributes1GroupBox
			//
			this.Attributes1GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 149, true);
			//
			// Attributes1Grid
			//
			this.Attributes1Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 130, true);
			//
			// AttributesRightSplitContainer
			//
			this.AttributesRightSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 149, true);
			//
			// Attributes2GroupBox
			//
			this.Attributes2GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 149, true);
			//
			// Attributes2Grid
			//
			this.Attributes2Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 130, true);
			//
			// Attributes3GroupBox
			//
			this.Attributes3GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 149, true);
			//
			// Attributes3Grid
			//
			this.Attributes3Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 130, true);
			//
			// PivotGrid
			//
			this.PivotGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 90, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.OrgSupplierPart);
			//
			// ComplementaryDescriptionTextBox
			//
			this.ComplementaryDescriptionTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ComplementaryDescriptionTextBox, "PivotsForBinding.ComplementaryDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).ComplementaryDescription)));
			this.ComplementaryDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.ComplementaryDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 63, true);
			this.ComplementaryDescriptionTextBox.Name = "ComplementaryDescriptionTextBox";
			this.ComplementaryDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 20, true);
			this.ComplementaryDescriptionTextBox.TabIndex = 7;
			//
			// NveTabPage
			//
			this.NveTabPage.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("OrgSupplierPartFormCustomsControlGlobal|781CF954-0EB6-479C-ADCE-60A8A2A66C1D", "Tariff Details");
			this.NveTabPage.Controls.Add(this.TariffDetailsLayout);
			this.NveTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NveTabPage.Name = "NveTabPage";
			this.NveTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.NveTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(761, 288, true);
			this.NveTabPage.TabIndex = 8;
			this.NveTabPage.UseVisualStyleBackColor = true;
			//
			// TariffDetailsLayout
			//
			this.TariffDetailsLayout.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffDetailsLayout, "PivotsForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.BR.Business.CusClassPartPivot)(((Enterprise.Customs.BR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)))));
			this.TariffDetailsLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TariffDetailsLayout.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TariffDetailsLayout.Name = "TariffDetailsLayout";
			this.TariffDetailsLayout.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 402, true);
			this.TariffDetailsLayout.TabIndex = 0;
			//
			// AttributesNcmTabPage
			//
			this.AttributesNcmTabPage.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("OrgSupplierPartFormCustomsControlGlobal|A00EBAAF-7AA2-49BB-8E8C-A2FB08F57B63", "Attributes");
			this.AttributesNcmTabPage.Controls.Add(this.AttributesGridLayout);
			this.AttributesNcmTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AttributesNcmTabPage.Name = "AttributesNcmTabPage";
			this.AttributesNcmTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AttributesNcmTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(761, 288, true);
			this.AttributesNcmTabPage.TabIndex = 8;
			this.AttributesNcmTabPage.UseVisualStyleBackColor = true;
			//
			// AttributesGridLayout
			//
			this.AttributesGridLayout.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AttributesGridLayout, "PivotsForBinding.Attributes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.BR.Business.AttributeCusCodeDataCollection)(((Enterprise.Customs.BR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).Attributes)));
			this.AttributesGridLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttributesGridLayout.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AttributesGridLayout.Name = "AttributesGridLayout";
			this.AttributesGridLayout.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(755, 282, true);
			this.AttributesGridLayout.TabIndex = 0;
			// 
			// AdditionalTariffTabPage
			// 
			this.AdditionalTariffsTabPage.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("OrgSupplierPartFormCustomsControlGlobal|A7CE8F55-D2F2-45A0-A9A0-A5ED0949DF27", "Additional Tariffs");
			this.AdditionalTariffsTabPage.Controls.Add(this.AdditionalTariffsGridLayout);
			this.AdditionalTariffsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalTariffsTabPage.Name = "AdditionalTariffTabPage";
			this.AdditionalTariffsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdditionalTariffsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(761, 288, true);
			this.AdditionalTariffsTabPage.TabIndex = 8;
			this.AdditionalTariffsTabPage.UseVisualStyleBackColor = true;
			// 
			// AdditionalTariffsGridLayout
			// 
			this.AdditionalTariffsGridLayout.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalTariffsGridLayout, "PivotsForBinding.AdditionalTariffs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.BR.Business.AdditionalTariffCollection)(((Enterprise.Customs.BR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).AdditionalTariffs)));
			this.AdditionalTariffsGridLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalTariffsGridLayout.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AdditionalTariffsGridLayout.Name = "AdditionalTariffsGridLayout";
			this.AdditionalTariffsGridLayout.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(755, 282, true);
			this.AdditionalTariffsGridLayout.TabIndex = 0;
			// 
			// OrgSupplierPartFormCustomsControlGlobal
			//
			this.Name = "OrgSupplierPartFormCustomsControlGlobal";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 405, true);
			this.detailsPanel.ResumeLayout(false);
			this.detailsPanel.PerformLayout();
			this.DetailTabControl.ResumeLayout(false);
			this.DetailTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.CI_UsageCommentTextBox.ResumeLayout(true);
			this.CI_UsageCommentTextBox.PerformLayout();
			this.ClassificationDescriptionTextBox.ResumeLayout(true);
			this.ClassificationDescriptionTextBox.PerformLayout();
			this.AttributesTabPage.ResumeLayout(false);
			this.AttributesTabPage.PerformLayout();
			this.AttributesLeftSplitContainer.Panel1.ResumeLayout(false);
			this.AttributesLeftSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AttributesLeftSplitContainer)).EndInit();
			this.AttributesLeftSplitContainer.ResumeLayout(false);
			this.AttributesLeftSplitContainer.PerformLayout();
			this.Attributes1GroupBox.ResumeLayout(false);
			this.Attributes1GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes1Grid)).EndInit();
			this.Attributes1Grid.ResumeLayout(false);
			this.Attributes1Grid.PerformLayout();
			this.AttributesRightSplitContainer.Panel1.ResumeLayout(false);
			this.AttributesRightSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AttributesRightSplitContainer)).EndInit();
			this.AttributesRightSplitContainer.ResumeLayout(false);
			this.AttributesRightSplitContainer.PerformLayout();
			this.Attributes2GroupBox.ResumeLayout(false);
			this.Attributes2GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes2Grid)).EndInit();
			this.Attributes2Grid.ResumeLayout(false);
			this.Attributes2Grid.PerformLayout();
			this.Attributes3GroupBox.ResumeLayout(false);
			this.Attributes3GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes3Grid)).EndInit();
			this.Attributes3Grid.ResumeLayout(false);
			this.Attributes3Grid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PivotGrid)).EndInit();
			this.PivotGrid.ResumeLayout(false);
			this.PivotGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ComplementaryDescriptionTextBox.ResumeLayout(true);
			this.ComplementaryDescriptionTextBox.PerformLayout();
			this.NveTabPage.ResumeLayout(false);
			this.NveTabPage.PerformLayout();
			this.TariffDetailsLayout.ResumeLayout(true);
			this.TariffDetailsLayout.PerformLayout();
			this.AttributesNcmTabPage.ResumeLayout(false);
			this.AttributesNcmTabPage.PerformLayout();
			this.AttributesGridLayout.ResumeLayout(true);
			this.AttributesGridLayout.PerformLayout();
			this.AdditionalTariffsTabPage.ResumeLayout(false);
			this.AdditionalTariffsTabPage.PerformLayout();
			this.AdditionalTariffsGridLayout.ResumeLayout(true);
			this.AdditionalTariffsGridLayout.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal Customs.GUI.LongTextControl ComplementaryDescriptionTextBox;
		internal ZArchitecture.GUI.ZTabPage NveTabPage;
		internal ZArchitecture.GUI.ZTabPage AttributesNcmTabPage;
		internal AttributesUserControl AttributesGridLayout;
		internal OrgSupplierTariffDetailsUserControl TariffDetailsLayout;
		internal ZArchitecture.GUI.ZTabPage AdditionalTariffsTabPage;
		internal AdditionalTariffsUserControl AdditionalTariffsGridLayout;
	}
}

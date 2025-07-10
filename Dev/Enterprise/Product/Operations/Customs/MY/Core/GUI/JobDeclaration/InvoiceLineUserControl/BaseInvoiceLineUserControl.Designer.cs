namespace Enterprise.Customs.MY.GUI
{
	partial class MYInvoiceLineUserControl
	{
		void InitializeComponent()
		{
			this.zTariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.InvoiceLinesSummaryGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.LineDetailTabControl.SuspendLayout();
			this.InvoiceDetailsGroupBox.SuspendLayout();
			this.ClassificationDetailsGroupBox.SuspendLayout();
			this.CurrentInvoicePanel.SuspendLayout();
			this.LineSummaryPanel.SuspendLayout();
			this.ContainersTabPage.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).BeginInit();
			this.LineDetailsTabPage.SuspendLayout();
			this.ClassificationPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// InvoiceLinesSummaryGroupBox
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.InvoiceLinesSummaryGroupBox, false);
			// 
			// InvoiceDetailsGroupBox
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.InvoiceDetailsGroupBox, false);
			// 
			// ClassificationDetailsGroupBox
			// 
			this.ClassificationDetailsGroupBox.Controls.Add(this.zTariffFindBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ClassificationDetailsGroupBox, false);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zTariffFindBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CustomsQuantityCalcDropEdit, 0);
			// 
			// zTariffFindBox
			// 
			this.BindingSource.SetBindingMember(this.zTariffFindBox, "FilteredInvoiceLines.JI_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Tariff);
			this.zTariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 16, true);
			this.zTariffFindBox.Name = "zTariffFindBox";
			this.zTariffFindBox.PreBoundMaxLength = 11;
			this.zTariffFindBox.ShowDescriptionBox = false;
			this.zTariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.zTariffFindBox.TabIndex = 0;
			// 
			// MYInvoiceLineUserControl
			// 
			this.Name = "MYInvoiceLineUserControl";
			this.InvoiceLinesSummaryGroupBox.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			this.TopPanel.ResumeLayout(false);
			this.LineDetailTabControl.ResumeLayout(false);
			this.InvoiceDetailsGroupBox.ResumeLayout(false);
			this.ClassificationDetailsGroupBox.ResumeLayout(false);
			this.CurrentInvoicePanel.ResumeLayout(false);
			this.LineSummaryPanel.ResumeLayout(false);
			this.ContainersTabPage.ResumeLayout(false);
			this.ContainersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).EndInit();
			this.LineDetailsTabPage.ResumeLayout(false);
			this.ClassificationPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}

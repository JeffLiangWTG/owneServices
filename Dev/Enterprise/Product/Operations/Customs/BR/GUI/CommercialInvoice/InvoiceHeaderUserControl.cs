using Enterprise.Customs.BR.Business;

namespace Enterprise.Customs.BR.GUI
{
	public partial class InvoiceHeaderUserControl : Customs.GUI.InvoiceHeaderUserControl
	{
		public ZArchitecture.GUI.ZGroupBox OtherDetailsGroupBox;
		public ZArchitecture.GUI.ZCodeFindBox ExchangeHedgeFinancialInstitutionFindBox;
		public ZArchitecture.GUI.ZDropEdit ExchangeHedgeReasonDropEdit;
		public ZArchitecture.GUI.ZDropEdit ExchangeHedgeTypeDropEdit;
		public ZArchitecture.ZTextBox ExchangeHedgeROFBACENNumberTextBox;
		public ZArchitecture.GUI.ZDropEdit JZ_RelatedIndicatorDropEdit;
		public ZArchitecture.GUI.ZDropEdit JZ_ValuationCodeDropEdit;
		public ZArchitecture.ZCalcEdit ExchangeHedgeValueCalcEdit;

		public InvoiceHeaderUserControl()
		{
			InitializeComponent();
		}

		protected override string ColumnTitleWhenExportForDutiable => base.ColumnTitleWhenImportForDutiable;

		protected override void ChangeControlsVisibilityWhenMessageTypeChanges()
		{
			base.ChangeControlsVisibilityWhenMessageTypeChanges();

			if (Invoice is JobComInvoiceHeader invoice && invoice.IsImportExcludingLicense)
			{
				OtherDetailsGroupBox.Show();
				JZ_ValuationCodeDropEdit.Show();
			}
			else
			{
				OtherDetailsGroupBox.Hide();
				JZ_ValuationCodeDropEdit.Hide();
			}
		}
	}
}

using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ValuationDetailsUserControl : ZUserControl
	{
		public ValuationDetailsUserControl()
		{
			InitializeComponent();
			InitializeLongTextControlBindings();
			this.SetTariffFindBoxDelegates(TariffFindBox);
		}
		void InitializeLongTextControlBindings()
		{
			BindingSource.SetBindingMember(IngredientLongTextControl, $"FilteredInvoiceLines.{JobComInvoiceLine.Schema.JI_Ingredient}");

			BindingSource.SetBindingMember(DescriptionLongTextControl, $"FilteredInvoiceLines.{JobComInvoiceLine.Schema.JI_Description}");
		}
	}
}

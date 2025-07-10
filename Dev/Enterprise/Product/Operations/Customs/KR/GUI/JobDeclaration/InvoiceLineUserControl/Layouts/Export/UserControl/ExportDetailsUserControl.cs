using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ExportDetailsUserControl : ZUserControl
	{
		public ExportDetailsUserControl()
		{
			InitializeComponent();
			InitializeLongTextControlBindings();
			this.SetTariffFindBoxDelegates(TariffFindBox);
		}

		void InitializeLongTextControlBindings()
		{
			BindingSource.SetBindingMember(IngredientLongTextControl, JobComInvoiceLine.Schema.JI_Ingredient);

			BindingSource.SetBindingMember(DescriptionLongTextControl, JobComInvoiceLine.Schema.JI_Description);
		}
	}
}

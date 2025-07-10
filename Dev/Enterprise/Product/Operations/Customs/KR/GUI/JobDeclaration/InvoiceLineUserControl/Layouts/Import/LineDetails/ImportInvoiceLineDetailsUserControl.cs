using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ImportInvoiceLineDetailsUserControl : ZUserControl
	{
		public ImportInvoiceLineDetailsUserControl()
		{
			InitializeComponent();
			this.BindingSource.SetBindingMember(this.IngredientLongTextControl, "JI_Ingredient");
			this.BindingSource.SetBindingMember(this.GoodsDescriptionLongTextControl, "JI_Description");
		}
	}
}

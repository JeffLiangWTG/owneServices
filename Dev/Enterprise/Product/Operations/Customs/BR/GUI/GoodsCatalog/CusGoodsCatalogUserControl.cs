namespace Enterprise.Customs.BR.GUI
{
	public partial class CusGoodsCatalogUserControl : Customs.GUI.CusGoodsCatalogUserControl
	{
		public CusGoodsCatalogUserControl()
		{
			InitializeComponent();
			BindingSource.SetBindingMember(ComplementaryDescriptionTextBox, "ComplementaryDescription");
		}
	}
}

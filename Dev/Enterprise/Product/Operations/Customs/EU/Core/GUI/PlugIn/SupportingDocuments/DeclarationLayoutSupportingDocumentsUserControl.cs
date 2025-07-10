namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class DeclarationLayoutSupportingDocumentsUserControl : LayoutSupportingDocumentsUserControl
	{
		public DeclarationLayoutSupportingDocumentsUserControl()
		{
			InitializeComponent();
		}

		protected override string GetSupportingDocumentsFieldsControlBindingString() => ".SupportingDocuments";
	}
}

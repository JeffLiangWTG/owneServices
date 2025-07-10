namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class PartPivotLayoutSupportingDocumentsUserControl : LayoutSupportingDocumentsUserControl
	{
		public PartPivotLayoutSupportingDocumentsUserControl()
		{
			InitializeComponent();
		}

		protected sealed override string GetSupportingDocumentsFieldsControlBindingString() => "PivotsForBinding.SupportingDocuments";
	}
}

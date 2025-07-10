namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public interface ISupportingDocumentsUserControl : ISupportingInfoUserControls
	{
		ISupportingDocumentsFieldsControl SupportingDocumentsFieldsControl { get; }
	}
}

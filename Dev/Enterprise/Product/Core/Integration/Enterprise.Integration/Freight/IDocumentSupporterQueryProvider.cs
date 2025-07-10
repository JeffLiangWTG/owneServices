namespace Enterprise.Integration.Freight
{
	public interface IDocumentSupporterQueryProvider
	{
		void ShowMessage(string message, string caption);
		bool ShowConfirmation(string message, string caption);
	}
}

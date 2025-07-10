namespace Enterprise.Diagnostics.Testing
{
	class eHubDiagnosticsFormTester : eHubDiagnosticsForm
	{
		internal string Message { get; set; }

		internal override void PopupErrorMessage(string message)
		{
			Message = message;
		}
	}
}

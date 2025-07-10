namespace Enterprise.Diagnostics.Testing
{
	class DirectxTDiagnosticsFormTester : DirectxTDiagnosticsForm
	{
		internal string Message { get; set; }

		internal override void PopupErrorMessage(string message)
		{
			Message = message;
		}
	}
}

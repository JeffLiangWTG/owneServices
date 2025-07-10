namespace Enterprise.DocumentScanning.Module.Testing
{
	class DocumentDbManagerModuleForTesting : DocumentDbManagerModule
	{
		public bool IsAllowedToShow_Exposed()
		{
			return base.IsAllowedToShow();
		}
	}
}

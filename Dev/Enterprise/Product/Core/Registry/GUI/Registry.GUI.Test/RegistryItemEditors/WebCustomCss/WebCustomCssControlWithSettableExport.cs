namespace Enterprise.Registry.GUI.Testing
{
	sealed class WebCustomCssControlWithSettableExport : WebCustomCssControl
	{
		public string FolderForExport { get; set; }

		protected override string GetFolderForExport() => FolderForExport;
	}
}

using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyDocumentLanguageProvider : IDocumentLanguageProvider
	{
		public DummyDocumentLanguageProvider(string language)
		{
			this.language = language;
		}

		readonly string language;

		public string GetDocumentLanguage() => language;
	}
}

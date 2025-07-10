using Enterprise.DocumentVisualizer.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class CurrentUserLanguageProvider : IDocumentLanguageProvider
	{
		public string GetDocumentLanguage() => GlbStaff.CurrentUser.GS_WorkingLanguage;
	}
}

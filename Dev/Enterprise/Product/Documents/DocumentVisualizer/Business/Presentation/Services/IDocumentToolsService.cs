using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IDocumentToolsService
	{
		void ShowMacroEvaluator(IMacroEvaluationContext context, IMacroScope scope, string input = null);

		void ShowMessagingData(IDocument document, string userDefinedXmlNamespace = "", string dataContext = "");

		void ShowDocumentData(IDocument document);

		void ShowOverridenData(IDocument document);
	}
}

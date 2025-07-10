using CargoWise.Macros;
using CargoWise.Macros.GUI;
using Enterprise.DocumentVisualizer.Models;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class DocumentToolsService : IDocumentToolsService
	{
		public void ShowMacroEvaluator(IMacroEvaluationContext context, IMacroScope scope, string input)
		{
			if (context != null && scope != null)
			{
				var macroEvaluationForm = new MacroEvaluationForm(
					context,
					input,
					scope,
					true);

				macroEvaluationForm.Show();
			}
		}

		public void ShowMessagingData(Core.IDocument document, string userDefinedXmlNamespace, string dataContext)
		{
			if (document?.Data != null)
			{
				var dataView = new DynamicDataView(document, DataViewModel.DataType.Messaging , userDefinedXmlNamespace, dataContext);
				dataView.Show();
			}
		}

		public void ShowDocumentData(Core.IDocument document)
		{
			if (document?.Data != null)
			{
				var dataView = new DynamicDataView(document, DataViewModel.DataType.Document);
				dataView.Show();
			}
		}

		public void ShowOverridenData(Core.IDocument document)
		{
			if (document?.Data != null)
			{
				var dataView = new DynamicDataView(document, DataViewModel.DataType.Overridden);
				dataView.Show();
			}
		}
	}
}

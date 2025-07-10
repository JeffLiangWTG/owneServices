using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class StandardPrintInstructions : IPrintInstructions
	{
		public StandardPrintInstructions(IDocumentPivot pivot, IStandardTemplate template, IMacroScope scope, IMacroEvaluationContext context)
		{
			Argument.NotNull(pivot, nameof(pivot));
			Argument.NotNull(template, nameof(template));

			this.pivot = pivot;
			this.template = template;
			this.scope = scope;
			this.context = context;
		}

		readonly IDocumentPivot pivot;
		readonly IStandardTemplate template;
		readonly IMacroScope scope;
		readonly IMacroEvaluationContext context;

		public string Title => pivot.DocumentTitle;
		public string GetDeliveryTitle(string deliveryMode) => Title;

		public string[] DeliveryModes => pivot.DeliveryModes;

		public int GetNumberOfCopies(string deliveryMode)
		{
			if (!numberOfCopies.HasValue)
			{
				numberOfCopies = numberOfCopies = template.GetNumberOfCopies(scope, context);
			}

			return numberOfCopies.Value;
		}
		int? numberOfCopies;

		public string AttachmentFilename { get; }

		public string DocumentName { get; }

		public IEnumerable<KeyValuePair<string, string>> GetParametersForDocumentDeliveryLog(string documentName)
		{
			yield return new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Name,
				documentName);

			if (string.CompareOrdinal(documentName, Title) != 0)
			{
				yield return new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type,
					Title);
			}
		}
	}
}

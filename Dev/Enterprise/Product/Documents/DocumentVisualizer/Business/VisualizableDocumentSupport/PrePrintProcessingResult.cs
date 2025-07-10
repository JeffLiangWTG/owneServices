using System;
using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class PrePrintProcessingResult : IPrePrintProcessingResult
	{
		public PrePrintProcessingResult(IDocument document, IReadOnlyDictionary<string, object> valuesSet)
		{
			Document = document ?? throw new ArgumentNullException(nameof(document));
			ValuesSet = valuesSet ?? throw new ArgumentNullException(nameof(document));
		}

		public IDocument Document { get; }
		public IReadOnlyDictionary<string, object> ValuesSet { get; }
	}
}

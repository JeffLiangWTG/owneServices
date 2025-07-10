using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IPrePrintProcessingResult
	{
		IDocument Document { get; }
		IReadOnlyDictionary<string, object> ValuesSet { get; }
	}
}

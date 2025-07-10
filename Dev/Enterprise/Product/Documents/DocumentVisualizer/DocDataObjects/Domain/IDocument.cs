using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IDocument
	{
		string Name { get; }
		string MenuName { get; }
		string Type { get; }
		IPrintInstructions PrintInstructions { get; }
		IMessageInstructions MessageInstructions { get; }
		IDisplayInstructions DisplayInstructions { get; }

		IReadOnlyCollection<IPage> Pages { get; }
		IReadOnlyCollection<ILog> Logs { get; }
	}
}
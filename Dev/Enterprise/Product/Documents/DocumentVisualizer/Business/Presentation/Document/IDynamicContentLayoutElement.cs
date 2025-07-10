using System.Collections.Generic;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IDynamicContentLayoutElement : ITextLayoutElement
	{
		IMacroScope Scope { get; }
		IDictionary<string, IDynamicData> EditableData { get; }
		bool HasDynamicContent { get; }
		void CancelOverride();
	}
}

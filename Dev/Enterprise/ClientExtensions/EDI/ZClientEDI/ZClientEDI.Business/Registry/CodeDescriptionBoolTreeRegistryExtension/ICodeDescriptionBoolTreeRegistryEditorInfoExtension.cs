using System.Collections.Generic;

namespace Enterprise.Client.EDI.Registry
{
	public interface ICodeDescriptionBoolTreeRegistryEditorInfoExtension
	{
		IReadOnlyList<string> CustomizedColumns { get; }

		IReadOnlyList<bool> AreCustomizedColumnsVisible { get; }

		IReadOnlyList<bool> AreBoolColumnsVisible { get; }
	}
}

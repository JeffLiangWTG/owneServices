using System.Collections.Generic;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public interface ICodeDescriptionBoolTreePanelExtension
	{
		IReadOnlyList<CodeDescriptionBoolControl> Grids { get; }

		IReadOnlyList<ZGroupBox> GridContainers { get; }

		CodeDescriptionBoolTreeRegistryEditorInfo EditorInfo { get; }
	}
}

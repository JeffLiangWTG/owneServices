using Enterprise.Integration;
using Enterprise.ProcessManagement.GUI;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public class IncidentClosureDispositionRegistryEditor : CodeDescriptionBoolTreeRegistryEditor
	{
		public IncidentClosureDispositionRegistryEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo, FallbackLevel fallbackLevel)
			: base(dataType, editorInfo, fallbackLevel)
		{
			this.editorInfo = (IncidentClosureDispositionRegistryEditorInfo)editorInfo;
		}
		readonly IncidentClosureDispositionRegistryEditorInfo editorInfo;

		protected new IncidentClosureDispositionRegistryEditorInfo EditorInfo
		{
			get { return editorInfo; }
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new IncidentClosureDispositionControl(editorInfo);
		}
	}
}

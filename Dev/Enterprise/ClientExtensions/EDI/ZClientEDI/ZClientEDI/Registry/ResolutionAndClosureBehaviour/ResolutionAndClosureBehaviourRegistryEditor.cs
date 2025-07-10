using Enterprise.Integration;
using Enterprise.ProcessManagement.GUI;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public class ResolutionAndClosureBehaviourRegistryEditor : CodeDescriptionBoolTreeRegistryEditor
	{
		public ResolutionAndClosureBehaviourRegistryEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo, FallbackLevel fallbackLevel)
			: base(dataType, editorInfo, fallbackLevel)
		{
			this.editorInfo = (ResolutionAndClosureBehaviourRegistryEditorInfo)editorInfo;
		}
		readonly ResolutionAndClosureBehaviourRegistryEditorInfo editorInfo;

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new ResolutionAndClosureBehaviourControl(editorInfo);
		}

		protected new ResolutionAndClosureBehaviourRegistryEditorInfo EditorInfo
		{
			get { return editorInfo; }
		}
	}
}

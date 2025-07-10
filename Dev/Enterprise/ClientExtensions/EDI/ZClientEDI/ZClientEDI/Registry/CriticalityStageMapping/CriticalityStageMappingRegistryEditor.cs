using Enterprise.Integration;
using Enterprise.ProcessManagement.GUI;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public class CriticalityStageMappingRegistryEditor : CodeDescriptionBoolTreeRegistryEditor
	{
		public CriticalityStageMappingRegistryEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo, FallbackLevel fallbackLevel)
			: base(dataType, editorInfo, fallbackLevel)
		{
			this.editorInfo = (CriticalityStageMappingRegistryEditorInfo)editorInfo;
		}
		readonly CriticalityStageMappingRegistryEditorInfo editorInfo;

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new CriticalityStageMappingControl(editorInfo);
		}

		protected new CriticalityStageMappingRegistryEditorInfo EditorInfo
		{
			get { return editorInfo; }
		}
	}
}

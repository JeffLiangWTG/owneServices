using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class CommunicationStatusRegistryItemEditor : CodeDescriptionBoolRegistryItemEditor
	{
		public CommunicationStatusRegistryItemEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo, FallbackLevel fallbackLevel)
			: base(dataType, null, fallbackLevel)
		{
			this.editorInfo = editorInfo as CommunicationStatusRegistryEditorInfo;
		}

		readonly CommunicationStatusRegistryEditorInfo editorInfo;

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			var control = new CommunicationStatusRegistryControl();
			control.SetupColumns(editorInfo.BoolColumnCaption);
			return control;
		}
	}
}

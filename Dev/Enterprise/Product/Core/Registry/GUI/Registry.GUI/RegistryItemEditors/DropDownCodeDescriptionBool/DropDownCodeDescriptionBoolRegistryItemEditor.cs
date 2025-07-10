using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class DropDownCodeDescriptionBoolRegistryItemEditor : CodeDescriptionBoolRegistryItemEditor
	{
		public DropDownCodeDescriptionBoolRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel)
			: base(dataType, null, fallbackLevel)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			var control = new DropDownCodeDescriptionBoolRegistryControl();
			return control;
		}
	}
}

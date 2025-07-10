using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.DropDownCodeDescriptionBoolRegistryItemEditor, Enterprise.Registry.GUI")]
	public class DropDownCodeDescriptionBoolRegistryEditorInfo : CodeDescriptionBoolRegistryEditorInfo
	{
		public DropDownCodeDescriptionBoolRegistryEditorInfo(MultilingualString boolColumnCaption)
			: base(boolColumnCaption)
		{
		}
	}
}

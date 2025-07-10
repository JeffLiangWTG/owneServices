using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class CodeDescriptionBoolRelatedItemRegistryItemEditor : CodeDescriptionBoolRegistryItemEditor
	{
		public CodeDescriptionBoolRelatedItemRegistryItemEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo, FallbackLevel fallbackLevel)
			: base(dataType, null, fallbackLevel)
		{
			this.editorInfo = editorInfo as CodeDescriptionBoolRelatedItemRegistryEditorInfo;
		}

		readonly CodeDescriptionBoolRelatedItemRegistryEditorInfo editorInfo;

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			var control = new CodeDescriptionBoolRelatedItemRegistryControl();
			control.SetupBoolColumn(editorInfo.BoolColumnCaption, editorInfo.IsBoolColumnVisible);
			control.SetupRelatedItemColumn(editorInfo.RelatedItemColumnCaption);
			return control;
		}
	}
}

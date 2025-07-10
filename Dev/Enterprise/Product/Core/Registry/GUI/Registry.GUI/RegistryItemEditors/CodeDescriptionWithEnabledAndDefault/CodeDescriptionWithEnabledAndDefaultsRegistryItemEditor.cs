using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class CodeDescriptionWithEnabledAndDefaultsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public CodeDescriptionWithEnabledAndDefaultsRegistryItemEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo, FallbackLevel fallbackLevel)
			: base(dataType, fallbackLevel, null)
		{
			this.editorInfo = (CodeDescriptionWithEnabledAndDefaultsRegistryEditorInfo)editorInfo;
		}

		public CodeDescriptionWithEnabledAndDefaultsRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		#region Implementation

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			var result = new CodeDescriptionWithEnabledAndDefaultsRegistryControl();

			if (editorInfo != null)
			{
				result.SetupCodeColumn(editorInfo.CodeColumnCaption);
				result.SetupDescriptionColumn(editorInfo.DescriptionColumnCaption);
			}

			return result;
		}

		protected override RegistryItemEditor.EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		readonly CodeDescriptionWithEnabledAndDefaultsRegistryEditorInfo editorInfo;

		#endregion
	}
}

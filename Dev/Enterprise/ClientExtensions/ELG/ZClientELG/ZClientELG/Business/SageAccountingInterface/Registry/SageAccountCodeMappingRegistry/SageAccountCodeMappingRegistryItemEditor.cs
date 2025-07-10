using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.ELG
{
	public class SageAccountCodeMappingRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public SageAccountCodeMappingRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryItemEditor.EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new SageAccountCodeMappingRegistryItemControl();
		}
	}
}

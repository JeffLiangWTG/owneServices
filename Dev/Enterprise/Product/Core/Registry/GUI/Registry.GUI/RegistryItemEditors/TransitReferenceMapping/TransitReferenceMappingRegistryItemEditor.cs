using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class TransitReferenceMappingRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public TransitReferenceMappingRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			   : base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new TransitReferenceMappingControl();
		}

		protected override Enterprise.Registry.GUI.RegistryItemEditor.EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}

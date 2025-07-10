using CargoWise.EntityFramework;
using Enterprise.Client.TGE.GUI;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.TGE.Business
{
	internal class TGEEventsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public TGEEventsRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryItemEditor.EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new TGEEventsRegistryItemControl();
		}
	}
}

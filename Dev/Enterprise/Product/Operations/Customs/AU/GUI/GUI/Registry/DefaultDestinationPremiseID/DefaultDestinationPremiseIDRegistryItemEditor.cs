using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class DefaultDestinationPremiseIDRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public DefaultDestinationPremiseIDRegistryItemEditor(DefaultDestinationPremiseIDRegistryDataType dataType, FallbackLevel currentFallbackLevel, BusinessObjectFactory factory)
			: base(dataType, currentFallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new DefaultDestinationPremiseIDControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}

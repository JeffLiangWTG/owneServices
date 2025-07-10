using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.EU.GUI.Registry
{
	public class NctsDefaultPrincipalRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public NctsDefaultPrincipalRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new NctsDefaultPrincipalRegistryItemUserControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}

using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.EU.GUI.Registry
{
	public class NctsDefaultConsignorConsigneeRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public NctsDefaultConsignorConsigneeRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new NctsDefaultConsignorConsigneeRegistryItemUserControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}

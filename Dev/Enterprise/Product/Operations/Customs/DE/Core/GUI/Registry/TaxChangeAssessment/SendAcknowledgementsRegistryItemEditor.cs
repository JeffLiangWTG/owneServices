using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.GUI.Registry
{
	public class SendAcknowledgementsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public SendAcknowledgementsRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new SendAcknowledgementsRegistryItemControl();
	}
}

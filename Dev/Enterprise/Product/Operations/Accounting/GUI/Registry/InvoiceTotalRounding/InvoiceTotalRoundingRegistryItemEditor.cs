using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.GUI
{
	class InvoiceTotalRoundingRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public InvoiceTotalRoundingRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new InvoiceTotalRoundingUserControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}

using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class CreditReportsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;

		public CreditReportsRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new CreditReportsRegistryControl();
		}
	}
}

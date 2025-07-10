using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class ConsolImportBranchRuleItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public ConsolImportBranchRuleItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new ConsolImportBranchRuleRegistryControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.TopLeft; }
		}
	}
}

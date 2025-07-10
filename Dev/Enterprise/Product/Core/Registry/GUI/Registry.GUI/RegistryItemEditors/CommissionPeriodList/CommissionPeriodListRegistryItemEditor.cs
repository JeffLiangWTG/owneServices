using Enterprise.Integration;

namespace Enterprise.Registry.GUI
{
	public class CommissionPeriodListRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public CommissionPeriodListRegistryItemEditor(IRegistryDataType dataType)
			: base(dataType, null, null)
		{
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new CommissionPeriodListRegistryControl();
		}
	}
}

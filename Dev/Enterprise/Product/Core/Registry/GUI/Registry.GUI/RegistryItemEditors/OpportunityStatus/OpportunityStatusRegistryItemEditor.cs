
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
namespace Enterprise.Registry.GUI
{
	class OpportunityStatusRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public OpportunityStatusRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		#region Implementation

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new OpportunityStatusRegistryControl();
		}

		protected override RegistryItemEditor.EditorPaneAnchor Anchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}

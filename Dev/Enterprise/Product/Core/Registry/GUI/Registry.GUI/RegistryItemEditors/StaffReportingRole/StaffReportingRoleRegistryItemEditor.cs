
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
namespace Enterprise.Registry.GUI
{
	class StaffReportingRoleRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public StaffReportingRoleRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		#region Implementation

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new StaffReportingRoleRegistryControl();
		}

		protected override RegistryItemEditor.EditorPaneAnchor Anchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}

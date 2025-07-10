using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class SupportEdwDataSourceReportRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;

		public SupportEdwDataSourceReportRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new SupportEdwDataSourceReportRegistryControl();
		}
	}
}

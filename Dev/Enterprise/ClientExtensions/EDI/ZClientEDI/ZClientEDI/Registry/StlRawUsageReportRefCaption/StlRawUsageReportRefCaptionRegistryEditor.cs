using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public class StlRawUsageReportRefCaptionRegistryEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public StlRawUsageReportRefCaptionRegistryEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory) { }

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new StlRawUsageReportRefCaptionControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}

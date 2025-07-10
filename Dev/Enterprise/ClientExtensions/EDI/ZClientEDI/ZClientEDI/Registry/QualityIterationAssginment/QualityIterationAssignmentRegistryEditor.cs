using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public class QualityIterationAssignmentRegistryEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public QualityIterationAssignmentRegistryEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory) { }

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new QualityIterationAssignmentControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}

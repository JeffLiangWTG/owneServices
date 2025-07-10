using CargoWise.EntityFramework;
using Enterprise.Accounting.GUI;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.GUI
{
	public class ComplianceDocumentRePrintRestrictionRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public ComplianceDocumentRePrintRestrictionRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new ComplianceDocumentRePrintRestrictionControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}

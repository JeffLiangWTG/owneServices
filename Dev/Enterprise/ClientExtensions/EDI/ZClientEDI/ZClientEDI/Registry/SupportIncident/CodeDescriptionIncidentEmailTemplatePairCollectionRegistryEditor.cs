using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public class CodeDescriptionIncidentEmailTemplatePairCollectionRegistryEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public CodeDescriptionIncidentEmailTemplatePairCollectionRegistryEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory, IRegistryEditorInfo editorInfo)
			: this(dataType, fallbackLevel, factory)
		{
			EditorInfo = editorInfo as CodeDescriptionIncidentEmailTemplatePairCollectionRegistryEditorInfo;
		}

		public CodeDescriptionIncidentEmailTemplatePairCollectionRegistryEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory) { }

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryControl(EditorInfo?.IsNeedUpgradeColumnVisible ?? false);
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		readonly CodeDescriptionIncidentEmailTemplatePairCollectionRegistryEditorInfo EditorInfo;
	}
}

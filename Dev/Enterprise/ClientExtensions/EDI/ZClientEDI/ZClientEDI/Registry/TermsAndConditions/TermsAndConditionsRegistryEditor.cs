using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public class TermsAndConditionsRegistryEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public TermsAndConditionsRegistryEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			string helpText = string.Format(
@"You can place special fields in the version or content template that will be merged with System information.
Special fields should start with {0} and end with {1} e.g. {0}{2}{1}

All available special fields are shown on the grid below. Double click on the row to insert field onto the current text position",
				Core.Constants.DocumentEngine.EmailParsing.StartTag, Core.Constants.DocumentEngine.EmailParsing.EndTag, "ClientName");

			return new NotificationEmailTemplateRegistryControl("Template", "Version", "Content", helpText);
		}
	}
}

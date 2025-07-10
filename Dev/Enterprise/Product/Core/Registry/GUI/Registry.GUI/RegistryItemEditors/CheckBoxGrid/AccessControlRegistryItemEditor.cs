using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.Registry.Business.Web;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class AccessControlRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public AccessControlRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
			accessRules = ((AccessControlRegistryDataType)dataType).AccessRules;
		}

		readonly AccessRulesBase accessRules;

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new WebAccessControl(accessRules);
		}

		public override void SetEditorPaneLayout(Control editorPane, int width, int height)
		{
			editorPane.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
			ControlDpiScalingHelper.SetHeight(ref editorPane, height, false);
			ControlDpiScalingHelper.SetWidth(ref editorPane, width, false);
		}
	}
}

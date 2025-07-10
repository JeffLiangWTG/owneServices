using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class BillOfLadingNumberCustomisationRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public BillOfLadingNumberCustomisationRegistryItemEditor(BillCustomisationRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
			this.dataType = dataType;
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new BillOfLadingNumberCustomisationControl(dataType);
		}

		public override void SetEditorPaneLayout(Control editorPane, int width, int height)
		{
			editorPane.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
			ControlDpiScalingHelper.SetHeight(ref editorPane, height, false);
			ControlDpiScalingHelper.SetWidth(ref editorPane, width, false);
		}

		readonly BillCustomisationRegistryDataType dataType;
	}
}

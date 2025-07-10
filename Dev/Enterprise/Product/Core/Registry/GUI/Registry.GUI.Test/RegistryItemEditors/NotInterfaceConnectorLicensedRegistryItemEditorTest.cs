using System.Windows.Forms;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class NotInterfaceConnectorLicensedRegistryItemEditorTest : NotInterfaceConnectorLicencedRegistryItemEditor
	{
		public NotInterfaceConnectorLicensedRegistryItemEditorTest() : base() { }

		public Control NewWinFormsEditorPaneCoreTest()
		{
			return NewWinFormsEditorPaneCore();
		}
	}
}

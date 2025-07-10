using System.Windows.Forms;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class CustomNotesControlForTesting : CustomNoteTypesControl
	{
		public void SimulateNodeClick(TreeViewEventArgs argsForHandler)
		{
			base.ModuleAndCountryTreeView_AfterSelect(this, argsForHandler);
		}
	}
}

using System.Windows.Forms;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZStmNoteTestTabPage : ZStmNoteTabPage
	{
		public ZStmNoteTestTabPage()
		{
		}

		public int NoteImageIndex => Icons.GetImageIndex(IconTypes.StmNote);

		public Form ParentFormFound;
		protected override void HookUpValidatingForSave()
		{
			base.HookUpValidatingForSave();
			ParentFormFound = FindForm();
		}
	}
}

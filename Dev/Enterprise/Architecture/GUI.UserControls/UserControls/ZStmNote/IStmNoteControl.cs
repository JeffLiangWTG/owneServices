using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IStmNoteControl
	{
		StmNote SelectedNote { get; set; }
		void FocusOnTabPage();
		void ShowMessage(string caption, string message);
	}
}

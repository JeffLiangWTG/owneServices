using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public class WorkflowNotesTabPage : ZTabPage, INotesTabPage
	{
		ProcessHeader Workflow
		{
			get
			{
				var parent = this.GetParent<WorkflowManagementUserControl>();
				return parent != null && parent.WorkflowsGrid.ListManager != null ? parent.WorkflowsGrid.ListManager.GetCurrent() as ProcessHeader : null;
			}
		}

		#region INotesTabPage Members

		IBusiness INotesTabPage.BusinessEntity
		{
			get { return Workflow; }
		}

		bool INotesTabPage.HasNotes
		{
			get
			{
				var parent = Workflow;
				var note = parent != null ? parent.WorkflowNote : null;

				return note != null && !note.IsDeleted && !note.ST_NoteDataAsText.IsEmpty;
			}
		}

		bool INotesTabPage.HasRelatedNotes
		{
			get { return false; }
		}

		#endregion
	}
}

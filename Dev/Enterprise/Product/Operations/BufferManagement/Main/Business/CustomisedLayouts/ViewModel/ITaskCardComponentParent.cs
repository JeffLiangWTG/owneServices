using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public interface ITaskCardComponentParent
	{
		void Save(object sender = null);
		void Close();
		void ShowParent();
		void VoteUp(bool jobCardsShown);
		void VoteDown(bool jobCardsShown);

		void UpdateStatus(string status);
		event EventHandler<StatusUpdatedEventArgs> StatusUpdated;

		ICardContent CardContent { get; }
		CellContent Cell { get; }
		ProcessTask Task { get; }
		bool IsPreview { get; }

		event EventHandler<TasksSavedArgs> Saved;
	}

	public class ParentShownEventArgs : EventArgs
	{
		public ParentShownEventArgs(BusinessObject parent)
		{
			Parent = parent;
		}

		public BusinessObject Parent { get; private set; }
	}
}

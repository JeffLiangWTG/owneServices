using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusUnderbondOutturnLogManager
	{
		public CusUnderbondOutturnLogManager(BusinessObject bizObj)
		{
			this.bizObj = bizObj;
		}

		readonly BusinessObject bizObj;

		#region HasOutturnLogs

		public bool HasSplitMessageOriginalRejectedLog
		{
			get { return SplitMessageOriginalRejectedLog.Count > 0; }
		}

		public bool HasSplitMessageFailedLog
		{
			get { return SplitMessageFailedLog.Count > 0; }
		}

		public bool HasNonExistantLineAtCustoms
		{
			get { return NonExistantLineLog.Count > 0; }
		}

		#endregion

		#region AddANewOutturnLog

		public StmALog AddANewSplitMessageOriginalRejectedLog(ZString reference)
		{
			return SplitMessageOriginalRejectedLog.AddNew(reference);
		}

		public StmALog AddANewSplitMessageFailedLog(ZString reference)
		{
			return SplitMessageFailedLog.AddNew(reference);
		}

		public StmALog AddANewHasNonExistantLineAtCustomsLog(ZString reference)
		{
			return NonExistantLineLog.AddNew(reference);
		}

		#endregion

		#region CancelAllOutturnLogs

		public void CancelAllOutturnLogs()
		{
			SplitMessageOriginalRejectedLog.CancelAll();
			SplitMessageFailedLog.CancelAll();
			NonExistantLineLog.CancelAll();
		}

		#endregion

		#region AllOutturnLogs

		public LogsForNominatedEvent SplitMessageOriginalRejectedLog
		{
			get
			{
				if (fSplitMessageOriginalRejectedLog == null)
				{
					fSplitMessageOriginalRejectedLog = new LogsForNominatedEvent(bizObj.GetLogs(), Events.UnderbondSplitOutturnOriginalRejected);
				}
				return fSplitMessageOriginalRejectedLog;
			}
		}
		LogsForNominatedEvent fSplitMessageOriginalRejectedLog;

		public LogsForNominatedEvent SplitMessageFailedLog
		{
			get
			{
				if (fSplitMessageFailedLog == null)
				{
					fSplitMessageFailedLog = new LogsForNominatedEvent(bizObj.GetLogs(), Events.Cancelled);
				}
				return fSplitMessageFailedLog;
			}
		}
		LogsForNominatedEvent fSplitMessageFailedLog;

		public LogsForNominatedEvent NonExistantLineLog
		{
			get
			{
				if (fNonExistantLineLog == null)
				{
					fNonExistantLineLog = new LogsForNominatedEvent(bizObj.GetLogs(), Events.UnderbondOutturnRejected);
				}
				return fNonExistantLineLog;
			}
		}
		LogsForNominatedEvent fNonExistantLineLog;

		#endregion
	}
}

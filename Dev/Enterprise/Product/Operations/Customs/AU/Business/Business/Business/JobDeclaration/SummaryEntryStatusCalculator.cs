using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SummaryEntryStatusCalculator
	{
		public SummaryEntryStatusCalculator(JobDeclaration jobDeclaration)
		{
			this.jobDeclaration = jobDeclaration;
		}

		public ZString SummaryMessageStatus
		{
			get
			{
				CustomsEntryStatus result = CustomsEntryStatus.MostImportantStatusForCMR(CurrentEntryMessageStatus);
				return result == null ? "" : result.Code;
			}
		}

		public ZString SummaryEntryStatus
		{
			get
			{
				ZString result = ZString.Empty;

				foreach (CusEntryHeader entryHeader in jobDeclaration.ActiveEntryHeaders)
				{
					if (result.IsEmpty)
					{
						result = entryHeader.CH_EntryStatus;
					}
					else if (entryHeader.CH_EntryStatus != result)
					{
						result = CMRImportEntryAdvice.MultiStatus.Code;
						break;
					}
				}

				return result;
			}
		}

		#region Implementation

		internal CustomsEntryStatus[] CurrentEntryMessageStatus
		{
			get
			{
				if (NeedRefreshCurrentEntryMessageStatus)
				{
					ArrayList result = new ArrayList();
					foreach (CusEntryHeader entryHeader in jobDeclaration.CustomsEntryHeaders)
					{
						if (entryHeader.CH_Status != CustomsEntryStatus.NotSent.Code)
						{
							CustomsEntryStatus status = MessageStatusList.GetCustomsEntryStatusFromCode(entryHeader.CH_Status);
							if (status != null)
							{
								result.Add(status);
							}
						}
					}

					if (jobDeclaration.IsHolding)
					{
						result.Add(CustomsEntryStatus.HoldAwaiting);
					}
					else if (jobDeclaration.IsDeclarationWorkFinished)
					{
						result.Add(CustomsEntryStatus.DeclarationWorkComplete);
					}

					fCurrentEntryMessageStatus = (CustomsEntryStatus[])result.ToArray(typeof(CustomsEntryStatus));
				}
				return fCurrentEntryMessageStatus;
			}
		}
		internal CustomsEntryStatus[] fCurrentEntryMessageStatus;

		internal bool NeedRefreshCurrentEntryMessageStatus
		{
			get
			{
				return fCurrentEntryMessageStatus == null
					|| jobDeclaration.CustomsEntryHeaders.StatusNeedsRecalculation
					|| (jobDeclaration.IsHolding && !jobDeclaration.MostRecentHold.IsInDatabase)
					|| (jobDeclaration.IsDeclarationWorkFinished && !jobDeclaration.MostRecentDWC.IsInDatabase)
					|| (jobDeclaration.CustomsEntryHeaders.HasChanges && jobDeclaration.CustomsEntryHeaders.Count == 0)
					|| jobDeclaration.RefreshCurrentEntryMessageStatus;
			}
		}

		CMRImportMessageStatusList MessageStatusList
		{
			get
			{
				if (fMessageStatusList == null)
				{
					fMessageStatusList = new CMRImportMessageStatusList();
				}
				return fMessageStatusList;
			}
		}
		CMRImportMessageStatusList fMessageStatusList;

		readonly JobDeclaration jobDeclaration;

		#endregion
	}
}

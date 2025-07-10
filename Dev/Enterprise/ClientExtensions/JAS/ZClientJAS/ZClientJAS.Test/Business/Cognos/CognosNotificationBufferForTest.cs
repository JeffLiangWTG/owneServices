using System.Collections.Generic;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	class CognosNotificationBufferForTest : NotificationBuffer, ICognosNotificationSubscriber
	{
		#region ICognosNotificationSubscriber Members
		void ICognosNotificationSubscriber.AdvanceProgressBy(int percentage)
		{
			CurrentProgress += percentage;
		}

		void ICognosNotificationSubscriber.CompleteProgress()
		{
			CurrentProgress = 100;
		}

		#endregion
		public int CurrentProgress
		{
			get
			{
				return fCurrentProgress;
			}

			set
			{
				fCurrentProgress = value;
				RegisterProgressPercentageAndEventCount();
			}
		}

		public int GetProgressPercentageAtEventIndex(int eventIndex)
		{
			int result = 0;
			foreach (int eventCount in ProgressPercentageAtEventCount.Keys)
			{
				if (eventCount > eventIndex)
				{
					break;
				}
				else
				{
					result = ProgressPercentageAtEventCount[eventCount];
				}
			}

			return result;
		}

		void RegisterProgressPercentageAndEventCount()
		{
			int numberOfEventCountAtCurrentProgress = Events.Length;
			if (ProgressPercentageAtEventCount.ContainsKey(numberOfEventCountAtCurrentProgress))
			{
				ProgressPercentageAtEventCount[numberOfEventCountAtCurrentProgress] = CurrentProgress;
			}
			else
			{
				ProgressPercentageAtEventCount.Add(numberOfEventCountAtCurrentProgress, CurrentProgress);
			}
		}

		readonly SortedDictionary<int, int> ProgressPercentageAtEventCount = new SortedDictionary<int, int>();
		int fCurrentProgress;
	}
}

using System;

namespace Enterprise.ZArchitecture
{
	/// <summary>
	/// This event is not reported to the user in the batch processor.
	/// </summary>
	[Serializable]
	public class VerboseInfoNotification : NotificationSubscriberNotification
	{
		public VerboseInfoNotification(string additionalInfo) : base(NotificationSubscriberType.VerboseInfo, additionalInfo)
		{
		}
	}
}

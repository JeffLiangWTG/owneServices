using System;

namespace Enterprise.ZArchitecture
{
	[Serializable]
	public class BOWarningNotification : BONotification
	{
		public BOWarningNotification(string message) : base(WarningType.Warning, message)
		{
		}
	}
}

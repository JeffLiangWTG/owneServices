using System;

namespace Enterprise.ZArchitecture
{
	[Serializable]
	public class BOErrorNotification : BONotification
	{
		public BOErrorNotification(string message) : base(ErrorType.Error, message)
		{
		}
	}
}

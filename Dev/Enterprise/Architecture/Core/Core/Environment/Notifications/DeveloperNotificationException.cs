using System;

namespace Enterprise.ZArchitecture.Environment
{
	[Serializable]
	public class DeveloperNotificationException : OdysseyException
	{
		public DeveloperNotificationException(string message) : base(message)
		{
		}

		public DeveloperNotificationException(string message, Exception e) : base(message, e)
		{
		}

#if NETFRAMEWORK
		protected DeveloperNotificationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

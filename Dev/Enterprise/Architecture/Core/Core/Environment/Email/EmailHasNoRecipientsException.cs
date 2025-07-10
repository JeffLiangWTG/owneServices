using System;

namespace Enterprise.ZArchitecture.Environment
{
	[Serializable]
	public class EmailHasNoRecipientsException : EmailSendFailedException
	{
		public EmailHasNoRecipientsException(string msg) : base(msg)
		{
			GroupName = "";
		}

		public EmailHasNoRecipientsException(string msg, string groupName)
			: base(msg)
		{
			if (groupName == null)
			{
				groupName = "";
			}
			GroupName = groupName;
		}

#if NETFRAMEWORK
		protected EmailHasNoRecipientsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public readonly string GroupName;
	}
}

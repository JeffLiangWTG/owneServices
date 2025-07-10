using System;

namespace Enterprise.ZArchitecture.Environment
{
	[Serializable]
	public class EmailNotCompleteException : EmailSendFailedException
	{
		public EmailNotCompleteException(string msg) : base(msg)
		{
			GroupName = "";
		}

		public EmailNotCompleteException(string msg, string groupName)
			: base(msg)
		{
			if (groupName == null)
			{
				groupName = "";
			}
			GroupName = groupName;
		}

#if NETFRAMEWORK
		protected EmailNotCompleteException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public readonly string GroupName;
	}
}

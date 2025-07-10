using System;

namespace Enterprise.ZArchitecture.Environment
{
	[Serializable]
	public class EmailHasNoFromAddressException : EmailNotCompleteException
	{
		public EmailHasNoFromAddressException(string msg) : base(msg)
		{
			NameOfGroup = "";
		}

		public EmailHasNoFromAddressException(string msg, string groupName)
			: base(msg)
		{
			if (groupName == null)
			{
				groupName = "";
			}
			NameOfGroup = groupName;
		}

#if NETFRAMEWORK
		protected EmailHasNoFromAddressException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public readonly string NameOfGroup;
	}
}

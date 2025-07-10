using System;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	[Serializable]
	public class LogSubscriberToAbortLogGroupProcessingSilentlyException : Exception
	{
		public LogSubscriberToAbortLogGroupProcessingSilentlyException(string message, params IEmailCreator[] emails) : base(message)
		{
			Emails = emails?.Where(x => x != null).ToArray();
		}

#if NETFRAMEWORK
		protected LogSubscriberToAbortLogGroupProcessingSilentlyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		public IEmailCreator[] Emails { get; }
	}
}

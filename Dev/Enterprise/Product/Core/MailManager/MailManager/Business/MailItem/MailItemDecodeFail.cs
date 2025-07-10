using System;

namespace Enterprise.MailManager.Business
{
	[Serializable]
	public class MailItemDecodeFail : Exception
	{
		public MailItemDecodeFail(string message, string emailBody, Exception innerException) : base(message, innerException)
		{
			this.EmailBody = emailBody;
		}

#if NETFRAMEWORK
		protected MailItemDecodeFail(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public readonly string EmailBody;
	}
}

using System;
using CargoWise.Types;

namespace Enterprise.Messaging.Business
{
	/// <summary>
	/// This exception is caught while processing a message or interchange
	/// </summary>
	[Serializable]
	public class MessageProcessingException : ApplicationException
	{
		public MessageProcessingException(ZString exceptionMessage, ZString messageOrInterchangeText, bool shouldSendEmailToUsers, bool shouldSendDeveloperInformation, bool shouldReportWholeMessage = false)
			: base(CreateExceptionMessage(shouldReportWholeMessage, exceptionMessage, messageOrInterchangeText))
		{
			this.ShouldSendEmailToUsers = shouldSendEmailToUsers;
			this.ShouldSendDeveloperInformation = shouldSendDeveloperInformation;
			this.MessageOrInterchangeText = messageOrInterchangeText;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		static ZString CreateExceptionMessage(bool shouldReportWholeMessage, ZString exceptionMessage, ZString messageOrInterchangeText)
		{
			var result = exceptionMessage;

			if (!messageOrInterchangeText.IsEmpty)
			{
				if (shouldReportWholeMessage)
				{
					result += "\r\nContents of failed file: \r\n" + messageOrInterchangeText;
				}
				else
				{
					result += "\r\nFirst 100 message or interchange characters: " + messageOrInterchangeText.SubstringSafe(0, 100);
				}
			}

			return result;
		}

#if NETFRAMEWORK
		protected MessageProcessingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public readonly bool ShouldSendEmailToUsers;
		public readonly bool ShouldSendDeveloperInformation;
		public readonly ZString MessageOrInterchangeText;
	}
}

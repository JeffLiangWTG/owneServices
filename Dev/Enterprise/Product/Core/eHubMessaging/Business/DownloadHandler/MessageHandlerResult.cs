using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	public interface IMessageHandlerResult
	{
		ZString InterchangeNumber { get; }
		ZGuid TrackingID { get; }
		IEnumerable<ZString> MessageNumbers { get; }
		ZString FailureReason { get; }
		IEnumerable<ZString> Warnings { get; }
		IEnumerable<ZString> ExternalReferenceNumbers { get; }
	}

	class MessageHandlerResult : IMessageHandlerResult
	{
		public static MessageHandlerResult Success(EDIInterchange interchange)
		{
			CheckArguments(interchange);
			return new MessageHandlerResult(interchange, ZString.Empty);
		}

		public static MessageHandlerResult SuccessWithWarnings(EDIInterchange interchange, IEnumerable<string> warnings)
		{
			CheckArguments(interchange);
			return new MessageHandlerResult(interchange, ZString.Empty);
		}

		public static MessageHandlerResult Failure(EDIInterchange interchange, ZString failureReason)
		{
			CheckArguments(interchange);
			return new MessageHandlerResult(interchange, failureReason);
		}

		static void CheckArguments(EDIInterchange interchange)
		{
			Argument.NotNull(interchange, nameof(interchange));
		}

		public static MessageHandlerResult Failure(ZString failureReason)
		{
			return new MessageHandlerResult(null, failureReason);
		}

		MessageHandlerResult(EDIInterchange interchange, ZString failureReason)
		{
			if (interchange != null)
			{
				InterchangeNumber = interchange.EI_InterchangeNum;
				MessageNumbers = interchange.ContainedMessages.Select(m => m.EM_MessageNum);
				TrackingID = interchange.EI_SessionGUID;
				Warnings = interchange.MessageProcessWarnings.Select(warning => new ZString(warning));
				ExternalReferenceNumbers = interchange.ContainedMessages.Select(m => m.EM_ExternalReferenceNumber);
			}

			FailureReason = failureReason;
		}

		#region IMessageHandlerResult

		public ZString InterchangeNumber { get; }

		public ZGuid TrackingID { get; }

		public IEnumerable<ZString> MessageNumbers { get; }

		public ZString FailureReason { get; }

		public IEnumerable<ZString> Warnings { get; }

		public IEnumerable<ZString> ExternalReferenceNumbers { get; }

		#endregion
	}
}

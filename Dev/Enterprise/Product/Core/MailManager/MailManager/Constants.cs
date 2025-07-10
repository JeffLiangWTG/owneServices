using System;
using MailManager;

namespace Enterprise.MailManager
{
	public static class MailDirection
	{
		public const string Transmit = DirectionList.Codes.Transmit;
		public const string Receive = DirectionList.Codes.Receive;
	}

	public static class MailApplication
	{
		public const string Standard = "STD";
	}

	public static class MailStatus
	{
		public const string Queued = StatusCodeList.Codes.Queued;
		public const string QueuedWithAck = StatusCodeList.Codes.QueuedWithAck;
		public const string Processed = StatusCodeList.Codes.Processed;
		public const string Unprocessed = StatusCodeList.Codes.Unprocessed;
		public const string Sent = StatusCodeList.Codes.Sent;
		public const string Failed = StatusCodeList.Codes.Failed;
		public const string Recognised = StatusCodeList.Codes.Recognized;
		public const string MarkedForReprocessing = StatusCodeList.Codes.MarkedForReProcessing;
	}

	public static class MailAcknowledgement
	{
		public const Byte MaxAttempts = 3;
		public const int AcknowledgmentTimeout = 30; // Minutes
	}

	public static class MailSendingLimitation
	{
		public const byte MaxToSend = 10;
		public const int LimitationPeriod = 30; // Minutes
	}
}

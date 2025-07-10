using System;
using System.Collections.Generic;
using System.IO;
using Enterprise.Messaging.Integration;

namespace Enterprise.xTMessaging.Shared
{
	public class BasicXtMessageInfo : IXtMessageInfo
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]

		public BasicXtMessageInfo(string applicationCode, string messageType, string sourceParty, string destinationParty, string messageTrackingId, BinaryReader contentReader)
		{
			ContentReader = contentReader ?? throw new ArgumentNullException("Message content reader");
			ApplicationCode = string.IsNullOrWhiteSpace(applicationCode) ? throw new ArgumentNullException("ApplicationCode") : applicationCode;
			MessageType = string.IsNullOrWhiteSpace(messageType) ? throw new ArgumentNullException("MessageType") : messageType;
			SourceParty = string.IsNullOrWhiteSpace(sourceParty) ? throw new ArgumentNullException("SourceParty") : sourceParty;
			DestinationParty = string.IsNullOrWhiteSpace(destinationParty) ? throw new ArgumentNullException("DestinationParty") : destinationParty;
			MessageTrackingID = Guid.TryParse(messageTrackingId, out var guid) && guid != Guid.Empty ? messageTrackingId : throw new ArgumentException("Value is not a valid GUID.", "MessageTrackingID");
		}

		BinaryReader ContentReader { get; }
		Dictionary<string, string> ExtraMessageAttributes { get; } = new();

		public BinaryReader GetMessageData() => ContentReader;

		public string ApplicationCode { get; }
		public string MessageType { get; }
		public string DestinationParty { get; }
		public string SourceParty { get; }
		public string MessageTrackingID { get; }

		public Dictionary<string, string> XTMessageAttributes => ExtraMessageAttributes;
	}
}

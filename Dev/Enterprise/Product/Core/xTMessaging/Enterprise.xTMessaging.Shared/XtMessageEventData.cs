using System;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Messaging.Integration;

namespace Enterprise.xTMessaging.Shared
{
	public class XtMessageEventData : IXtMessageEventData
	{
		public XtMessageEventData(int index, ulong xtMsgId, string json)
		{
			EventIndex = index;
			sourceJson = json  ?? throw new ArgumentNullException("xT event data json could not be null.");
			XtMsgId = xtMsgId;
		}

		public ulong XtMsgId { get; }

		public int EventIndex { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public int LogEvent
		{
			get
			{
				if (_logEvent == 0)
				{
					int.TryParse(EventAttributes.GetValueSafe("logevent"), out _logEvent);
				}
				return _logEvent;
			}
		}
		int _logEvent;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public string LogText
		{
			get
			{
				_logText = LogEventToText.GetValueSafe(LogEvent);
				var attributeValue = EventAttributes.GetValueSafe("logtext");
				if (!string.IsNullOrEmpty(attributeValue))
				{
					if (!string.IsNullOrEmpty(_logText))
					{
						_logText += ": " + attributeValue;
					}
					else
					{
						_logText = attributeValue;
					}
				}
				return _logText;
			}
		}
		string _logText;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public string LogTime => EventAttributes.GetValueSafe("time");

		Dictionary<string, string> EventAttributes
		{
			get
			{
				if (_eventAttributes == null)
				{
					try
					{
						_eventAttributes = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(sourceJson);
					}
					catch
					{
						_eventAttributes = new Dictionary<string, string>();
					}
				}
				return _eventAttributes;
			}
		}

		Dictionary<string, string> _eventAttributes;

		readonly string sourceJson;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		static readonly Dictionary<int, string> LogEventToText = new Dictionary<int, string>
		{
			{ 1, "Received from application" },
			{ 2, "Sent to application" },
			{ 3, "Acknowledged by application" },
			{ 4, "Message received from contract" },
			{ 5, "Message sent to contract" },
			{ 7, "Message accepted by contract" },
			{ 16, "Acknowledge timeout" },
			{ 17, "Erased by system" },
			{ 18, "Retransmitted by system" },
			{ 19, "Acknowledge set on original message in chain" },
			{ 20, "Owner switch requested" },
			{ 21, "Terminated by system" },
			{ 22, "Terminated by processing" },
			{ 23, "Moved to archive database" },
			{ 24, "Data synced to backing store" },
			{ 25, "Data synced from backing store" },
			{ 32, "Switched Message owner" },
			{ 33, "Switched Message owner (forced)" },
			{ 34, "Sent with xTRM" },
			{ 35, "Sent to Node" },
			{ 36, "Received with xTRM" },
			{ 37, "Received from Node" },
			{ 38, "Received from Processing" },
			{ 39, "Sent with X.400" },
			{ 40, "Received by X.400" },
			{ 41, "Acknowledge sent with X.400" },
			{ 43, "Acknowledge received by X.400" },
			{ 44, "Sent with X.435" },
			{ 45, "Received by X.435" },
			{ 46, "Acknowledge sent with X.435" },
			{ 47, "Acknowledge received by X.435" },
			{ 48, "Delivery ack" },
			{ 49, "Application ack received" },
			{ 50, "Required acknowledgement reached" },
			{ 51, "Processing step" },
			{ 52, "Processing started" },
			{ 53, "Internal Acknowledgement" },
			{ 55, "Contract found using routing" },
			{ 56, "Owner switch cancelled" },
			{ 64, "Priority changed by operator" },
			{ 65, "Timeout changed by operator" },
			{ 66, "Destination changed by operator" },
			{ 67, "Retry from error state by operator" },
			{ 68, "Terminated by operator" },
			{ 69, "Acknowledged by operator" },
			{ 70, "Checksum recalculated by operator" },
			{ 71, "Removed from active database by operator" },
			{ 72, "Buffer released" },
			{ 76, "Negative ack" },
			{ 79, "New transport selected by operator" },
			{ 80, "Configuration mode changed by operator" },
			{ 81, "Owner switch requested by operator" },
			{ 82, "Metadata resync requested by operator" },
			{ 83, "Owner switch cancelled by operator" },
			{ 256, "Negative ack received" },
			{ 257, "Nack by Application" },
			{ 259, "Terminated by Application" },
			{ 262, "Reply is created for this message" },
			{ 263, "Retry by Application" },
			{ 264, "Reply returned" },
			{ 265, "Ack returned" },
			{ 321, "Sent by HTTP-client" },
			{ 322, "Received by HTTP-client" },
			{ 326, "Sent AS2 Message" },
			{ 327, "Sent SwedInvoice Message" },
			{ 337, "Sent by FTP-server" },
			{ 338, "Received by FTP-server" },
			{ 353, "Sent with FTP-client" },
			{ 354, "Received by FTP-client" },
			{ 368, "SMTP MDN sent" },
			{ 369, "Sent with SMTP" },
			{ 370, "Received by SMTP" },
			{ 371, "Ack received by SMTP" },
			{ 372, "Nack received by SMTP" },
			{ 401, "Received by OFTP" },
			{ 402, "Sent with OFTP" },
			{ 403, "Ack received by OFTP" },
			{ 404, "Nack received by OFTP" },
			{ 513, "Received by MQ" },
			{ 514, "Sent with MQ" },
			{ 515, "Ack received by MQ" },
			{ 516, "Nack received by MQ" },
			{ 528, "Buffered" },
			{ 529, "Buffer threshold release" },
			{ 530, "Buffer scheduled release" },
			{ 545, "Sent with HTTP-server" },
			{ 546, "Received by HTTP-server" },
			{ 547, "Ack received by HTTP-server" },
			{ 548, "Nack received by HTTP-server" },
			{ 549, "AS2 message received by HTTP-server" },
			{ 550, "Received AS2 receipt by HTTP" },
			{ 551, "Sent AS2 receipt with HTTP" },
			{ 552, "Received SwedInvoice by HTTP" },
			{ 553, "Received SwedInvoice ack by HTTP" },
			{ 592, "AS2 message verified" },
			{ 593, "AS2 message signed" },
			{ 594, "AS2 ack verified" },
			{ 595, "AS2 ack signed" },
			{ 596, "AS2 message decrypted" },
			{ 597, "AS2 message encrypted" },
			{ 598, "AS2 message decompressed" },
			{ 599, "AS2 message compressed" },
			{ 609, "Received by SFTP-server" },
			{ 610, "Sent with SFTP-server" },
			{ 612, "Received by SFTP-client" },
			{ 613, "Sent with SFTP-client" },
			{ 4098, "Parsed as Edifact" },
			{ 4099, "Parsed as Edimgr" },
			{ 4100, "Parsed as xT-internal" },
			{ 4101, "Parsed by custom parser" },
			{ 4102, "Parsed as X12" },
			{ 4105, "Parsed as IDOC" },
			{ 4106, "Parsed as TRADACOMS" },
			{ 4107, "Parsed as Key-Value" },
			{ 4108, "Parsed as XML" },
			{ 4123, "Decoded before parsing" },
			{ 4124, "Routed to static contract" },
			{ 268435457, "Processing error" },
			{ 268435458, "xTRM sending error" },
			{ 268435460, "Node sending error" },
			{ 268435464, "Error sending acknowledge" },
			{ 268435466, "X.400 sending error" },
			{ 268435468, "X.435 sending error" },
			{ 268435471, "Routing error" },
			{ 268435473, "HTTP-client sending error" },
			{ 268435474, "FTP-server sending error" },
			{ 268435475, "FTP-client sending error" },
			{ 268435476, "SMTP sending error" },
			{ 268435477, "OFTP sending error" },
			{ 268435478, "MQ sending error" },
			{ 268435479, "Error sending to Contract" },
			{ 268435480, "Buffer error" },
			{ 268435482, "AS2 receipt is malformed" },
			{ 268435483, "AS2 receipt is missing required MIC" },
			{ 268435484, "AS2 receipt MIC is not correct" },
			{ 268435485, "AS2 receipt indicates reception failure" },
			{ 268435486, "Error sending to Application" },
			{ 268435487, "Synchronous reply session error" },
			{ 268435488, "Error returning reply" },
			{ 268435494, "SFTP-server sending error" },
			{ 268435495, "SFTP-client sending error" },
			{ 268435496, "Transport level session error" },
			{ 268435497, "Message data verification failure" },
			{ 268435498, "Error returning acknowledge" }
		};
	}
}

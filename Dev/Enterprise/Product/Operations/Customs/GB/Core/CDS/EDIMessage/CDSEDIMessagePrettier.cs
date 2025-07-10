using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Interfaces;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Customs.GB.CDS
{
	public abstract class CDSEDIMessagePrettier
	{
		protected CDSEDIMessagePrettier(CDSEDIMessage message)
		{
			Message = message ?? throw new ArgumentNullException(nameof(message));
			Entry = message.LinkedEntry;
			Declaration = Entry?.Declaration;
		}

		public abstract ZString MakeHumanReadable();

		protected ZString ToPIfNotEmpty(ZString p)
		{
			return !p.IsEmpty
				? (ZString)Invariant($"<p>{p}</p>")
				: ZString.Empty;
		}

		protected ZString ToH1IfNotEmpty(ZString h1)
		{
			return !h1.IsEmpty
				? (ZString)Invariant($"<H1>{h1}</H1>")
				: ZString.Empty;
		}

		protected ZString ToH3IfNotEmpty(ZString h3)
		{
			return !h3.IsEmpty
				? (ZString)Invariant($"<H3>{h3}</H3>")
				: ZString.Empty;
		}

		protected ZString ToH4IfNotEmpty(ZString h4)
		{
			return !h4.IsEmpty
				? (ZString)Invariant($"<H4>{h4}</H4>")
				: ZString.Empty;
		}

		protected ZString ToStrongIfNotEmpty(ZString strong)
		{
			return !strong.IsEmpty
				? (ZString)Invariant($"<strong>{strong}</strong>")
				: ZString.Empty;
		}

		protected ZString ToLiIfNotEmpty(ZString li)
		{
			return !li.IsEmpty
				? (ZString)Invariant($"<li>{li}</li>")
				: ZString.Empty;
		}

		protected ZString ToUlIfNotEmpty(ZString ul)
		{
			return !ul.IsEmpty
				? (ZString)Invariant($"<ul>{ul}</ul>")
				: ZString.Empty;
		}

		protected ZString ToTableSection(ZString caption, HtmlTableCreator creator)
		{
			var result = ZString.Empty;

			ZString tableString = creator?.ToHtml() ?? ZString.Empty;
			if (!tableString.IsEmpty)
			{
				var content = new ZString(Invariant($"{caption}:{tableString}")).Trim(' ', ':');
				result = ToPIfNotEmpty(content);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected ZString ToKeyValuePairSection(IEnumerable<(ZString key, ZString value)> pairs)
		{
			var content = pairs
				.Where(p => !p.value.IsEmpty)
				.Select(p => new ZString(ToStrongIfNotEmpty(p.key + ": ") + p.value).Trim(' ', ':'))
				.Where(x => !x.IsEmpty)
				.JoinAsString(BR);

			return ToPIfNotEmpty(content);
		}

		protected ZString ToCodeDescriptionDisplay(ZString code, ZString description)
		{
			return new ZString(Invariant($"{code} - {description}")).Trim(' ', '-');
		}

		protected ZString GetCodeAndDescription(ZString code, CodeDescriptionPairList list)
		{
			var description = list.GetDescriptionFromCode(code);

			return ToCodeDescriptionDisplay(code, description);
		}

		protected const string BR = "<br>";

		protected BusinessObjectFactory Factory => Message.Factory;

		protected HtmlTableCreator GetHtmlTableCreator(string width = "100%")
		{
			return new HtmlTableCreator(new NameValueCollection
			{
				{ "border", "1" },
				{ "cellpadding", "1" },
				{ "cellspacing", "0" },
				{ "width", width },
				{ "class", "table" }
			})
			{
				EnableHTMLEncoding = false
			};
		}

		protected CDSEDIMessage Message { get; }
		protected CusEntryHeader Entry { get; }
		protected JobDeclaration Declaration { get; }

		public static ZString GetCIDMessage(ZString? cspID, ZGuid trackingID, EDIMessage originalMessage, EDIMessage message, EDIMessage cspMessage, ZString recipient, ZDateTimeOffset? eventTime, string eventType = "")
		{
			var prettyString = ZString.Empty;
			var messageRejected = eventType == Constants.EHubEventTypes.MessageRejected;

			if (cspMessage != null)
			{
				prettyString = MessagePrettierCss.ProgressCSS + string.Format(CultureInfo.CurrentCulture,
															HtmlForCSP,
															GetCSPUploadMessage(originalMessage.EM_MessageNum, message.EM_ApplicationReference),
															originalMessage.EM_MessageNum.IsEmpty ? NotDone : Done,
															MessageNumText(originalMessage),
															trackingID.IsEmpty ? NotDone : Done,
															Invariant($"eHub Tracking ID = {trackingID}"),
															cspMessage.EM_ApplicationReference.IsEmpty ? NotDone : Done,
															Invariant($"CSP Tracking ID = {cspMessage.EM_ApplicationReference}"),
															message.EM_ApplicationReference.IsEmpty ? NotDone : Done,
															Invariant($"HMRC Conversation ID = {message.EM_ApplicationReference}"));
			}

			if (cspID.HasValue && !string.IsNullOrEmpty(cspID.Value))
			{
				prettyString = MessagePrettierCss.ProgressCSS + string.Format(CultureInfo.CurrentCulture,
														HtmlForCIDWithCSP,
														messageRejected ? GetRejectedMessage(originalMessage.EM_MessageNum, message.EM_ApplicationReference) : GetCIDReceivedMessage(originalMessage.EM_MessageNum, message.EM_ApplicationReference),
														messageRejected ? MessageRejectedDetails : string.Empty,
														originalMessage.EM_MessageNum.IsEmpty ? NotDone : Done,
														MessageNumText(originalMessage),
														trackingID.IsEmpty ? NotDone : Done,
														Invariant($"eHub Tracking ID = {trackingID}"),
														Done,
														Invariant($"CSP Tracking ID = {cspID}"),
														messageRejected ? Rejected : message.EM_ApplicationReference.IsEmpty ? NotDone : Done,
														Invariant($"HMRC Conversation ID = {message.EM_ApplicationReference}"));
			}
			else
			{
				prettyString = MessagePrettierCss.ProgressCSS + string.Format(CultureInfo.CurrentCulture,
															HtmlForCID,
															messageRejected ? GetRejectedMessage(originalMessage.EM_MessageNum, message.EM_ApplicationReference) : GetCIDReceivedMessage(originalMessage.EM_MessageNum, message.EM_ApplicationReference),
															messageRejected ? MessageRejectedDetails : string.Empty,
															originalMessage.EM_MessageNum.IsEmpty ? NotDone : Done,
															MessageNumText(originalMessage),
															trackingID.IsEmpty ? NotDone : Done,
															recipient,
															Invariant($"{recipient} Tracking ID = {trackingID}"),
															messageRejected ? Rejected : message.EM_ApplicationReference.IsEmpty ? NotDone : Done,
															Invariant($"HMRC Conversation ID = {message.EM_ApplicationReference}"));
			}

			prettyString += GetExtraData((originalMessage as CDSEDIMessage)?.LinkedMessageAttachee, eventTime);

			return prettyString;
		}

		static string MessageNumText(EDIMessage originalMessage) => Invariant($"Message number = {originalMessage.EM_MessageNum} at {originalMessage.EM_SystemCreateTimeUtc.ToString("dd/MM/yyyy HH:mm")} (UTC)");

		public static ZString GetCSPMessage(EDIMessage cidMessage, ZGuid trackingID, EDIMessage originalMessage, EDIMessage message, ZDateTimeOffset? eventTime)
		{
			var prettyString = MessagePrettierCss.ProgressCSS + string.Format(CultureInfo.CurrentCulture,
														HtmlForCSP,
														GetCSPUploadMessage(originalMessage.EM_MessageNum, message.EM_ApplicationReference),
														originalMessage.EM_MessageNum.IsEmpty ? NotDone : Done,
														MessageNumText(originalMessage),
														trackingID.IsEmpty ? NotDone : Done,
														Invariant($"eHub Tracking ID = {trackingID}"),
														message.EM_ApplicationReference.IsEmpty ? NotDone : Done,
														Invariant($"CSP Tracking ID = {message.EM_ApplicationReference}"),
														(cidMessage?.EM_ApplicationReference ?? ZString.Empty).IsEmpty ? NotDone : Done,
														(cidMessage?.EM_ApplicationReference ?? ZString.Empty).IsEmpty ? ZString.Empty : cidMessage.EM_ApplicationReference);

			prettyString += GetExtraData((originalMessage as CDSEDIMessage)?.LinkedMessageAttachee, eventTime);

			return prettyString;
		}

		public static ZString GetQueryResponseMessage(ZString entryNumberType, ZString entryNumber, ZString notificationType, ZString? encodedResponse)
		{
			return MessagePrettierCss.CSS +
				string.Format(CultureInfo.CurrentCulture,
					HtmlForQueryResponse,
					entryNumberType,
					entryNumber,
					notificationType,
					DecodeResponse(encodedResponse)
				);
		}

		static string DecodeResponse(ZString? response)
		{
			return WebUtility.HtmlEncode(
			 !response.HasValue || response.Value.IsEmpty ? string.Empty : Encoding.UTF8.GetString(Convert.FromBase64String(response.Value)));
		}

		static ZString GetCSPUploadMessage(ZString messageNum, ZString trackingID) => string.Format(CultureInfo.CurrentCulture, MessageUploadedToCSP, messageNum, trackingID);

		static ZString GetCIDReceivedMessage(ZString messageNum, ZString conversationID) => string.Format(CultureInfo.CurrentCulture, MessageReceivedCID, messageNum, conversationID);

		static ZString GetRejectedMessage(ZString messageNum, ZString conversationID) => string.Format(CultureInfo.CurrentCulture, MessageRejected, messageNum, conversationID);

		public static ZString GetExtraData(IMessageAttachee messageAttachee, ZDateTimeOffset? eventTime) => string.Format(CultureInfo.CurrentCulture, ExtraDataHtml, eventTime.HasValue ? eventTime.Value.ToUtcZDateTime().ToString("dd/MM/yyyy HH:mm") + " (UTC)" : string.Empty, messageAttachee?.JobNumber ?? ZString.Empty, messageAttachee?.LocalReferenceNumber ?? ZString.Empty, messageAttachee?.JobReference ?? ZString.Empty);

		const string MessageUploadedToCSP = "Message {0} was uploaded to the CSP and received tracking ID {1}; it has not yet necessarily reached CDS.";

		const string MessageReceivedCID = "Message {0} was uploaded to CDS and received Conversation ID {1}";

		const string MessageRejected = "Message {0} was rejected by CDS and received Conversation ID {1}";

		const string MessageRejectedDetails = "Please refer to other messages for details of the failure";

		const string Done = @"<td class=""StepProgress-item is-done"">&#10004;</td>";

		const string NotDone = @"<td class=""StepProgress-item current"">&#10004;</td>";

		const string Rejected = @"<td class=""StepProgress-item rejected"">&#x2716;</td>";

		const string HtmlForCID = @"<p><h4>{0}</h4></p><p><h5>{1}</h5</p><table>
<tr>
	{2}
	<td class=""Status"">Message created</td>
	<td><small>{3}</small></td>
</tr>
<tr>
	{4}
	<td class=""Status"">Sent to {5}</td>
	<td><small>{6}</small></td>
</tr>
<tr>
	{7}
	<td class=""Status"">Sent to CDS</td>
	<td><small>{8}</small></td>
</tr>
</table>";

		const string HtmlForCIDWithCSP = @"<p><h4>{0}</h4></p><p><h5>{1}</h5</p><table>
<tr>
	{2}
	<td class=""Status"">Message created</td>
	<td><small>{3}</small></td>
</tr>
<tr>
	{4}
	<td class=""Status"">Sent to eHub</td>
	<td><small>{5}</small></td>
</tr>
<tr>
	{6}
	<td class=""Status"">Sent to CSP</td>
	<td><small>{7}</small></td>
</tr>
<tr>
	{8}
	<td class=""Status"">Sent to CDS</td>
	<td><small>{9}</small></td>
</tr>
</table>";

		const string HtmlForCSP = @"<p><h4>{0}</h4></p><table>
<tr>
	{1}
	<td class=""Status"">Message created</td>
	<td><small>{2}</small></td>
</tr>
<tr>
	{3}
	<td class=""Status"">Sent to eHub</td>
	<td><small>{4}</small></td>
</tr>
<tr>
	{5}
	<td class=""Status"">Sent to CSP</td>
	<td><small>{6}</small></td>
</tr>
<tr>
	{7}
	<td class=""Status"">Sent to CDS</td>
	<td>
		<small>{8}</small>
	</td>
</tr>
</table>";

		const string ExtraDataHtml = @"<p/><table>
<tr style=""height: 20px;"">
<td>Status update time:      </td>
<td><small>{0}</small></td>
</tr>
<tr style=""height: 20px;"">
<td>DUCR (2/4):</td>
<td><small>{1}</small></td>
</tr>
<tr style=""height: 20px;"">
<td>LRN (2/5):</td>
<td><small>{2}</small></td>
</tr>
<tr style=""height: 20px;"">
<td>Job number:</td>
<td><small>{3}</small></td>
</tr>
</table>";
		const string HtmlForQueryResponse = @"<p><h4>Query Response</h4></p>
<table width=""100%"" border=""1"" cellpadding=""1"" class=""table"">
	<tr>
		<td>Entry Number Type</td>
		<td>{0}</td>
	</tr>
	<tr>
		<td>Entry Number</td>
		<td>{1}</td>
	</tr>
	<tr>
		<td>Notification Type</td>
		<td>{2}</td>
	</tr>
	<tr>
		<td>Response</td>
		<td>{3}</td>
	</tr>
</table>";
	}

	public abstract class CDSEDIMessagePrettier<T> : CDSEDIMessagePrettier where T : CDSEDIMessage
	{
		protected CDSEDIMessagePrettier(T message) : base(message)
		{
		}

		protected new T Message => (T)base.Message;
	}
}

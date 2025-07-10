using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.EConversation.Business
{
	public class EConversationMessageNotification : NonPersistentBusinessObject
	{
		public EConversationMessageNotification(JobConversation conversation, IEnumerable<IConversationMessage> newMessages, IEnumerable<IConversationMessage> previousMessages, string hyperlinkToParent, IConversationParticipant recipientParticipant)
		{
			this.conversation = conversation;
			this.newMessages = newMessages;
			this.previousMessages = previousMessages;
			this.hyperlinkToParent = hyperlinkToParent;
			this.recipientParticipant = recipientParticipant;
		}
		readonly JobConversation conversation;
		readonly IEnumerable<IConversationMessage> newMessages;
		readonly IEnumerable<IConversationMessage> previousMessages;
		readonly string hyperlinkToParent;
		readonly IConversationParticipant recipientParticipant;

		public ZString ID
		{
			get
			{
				var provider = conversation.Parent as IConversationProvider;
				var subjectContentOverride = provider?.EmailSubjectContentOverride;

				return !string.IsNullOrEmpty(subjectContentOverride) ? subjectContentOverride : conversation.Parent.HumanReadableName.ToString();
			}
		}

		public ZString BusinessObjectName => conversation.Parent.HumanReadableName;

		public ZString BusinessObjectHyperlink => hyperlinkToParent;

		public ZBool IsInternalRecipient => recipientParticipant == null || recipientParticipant.IsInternal;

		public ZString UtcOffset
		{
			get
			{
				var utcOffset = ZDateTimeOffset.Now.ToDateTimeOffset();

				return Env.Time.FormatOffset(utcOffset.Offset);
			}
		}

		#region SuppressResourceStringsCheckRegion

		public ZString NewMessages
		{
			get
			{
				var sb = new StringBuilder();
				sb.AppendLine($"<h2>{Res.GetString("c74e6ac1-98a9-4fec-9f70-ca530090792b", "New Messages")}</h2>");

				AddFormattedMessages(sb, newMessages.OrderByDescending(m => m.SystemCreateTimeInUtc));

				return sb.ToString();
			}
		}

		public ZString PreviousMessages
		{
			get
			{
				var sb = new StringBuilder();
				if (!previousMessages.Any())
				{
					sb.AppendLine($"<p>{Res.GetString("950da4c5-ee09-419c-80b6-a71a7b320f22", "There are no previous messages")}</p>");
				}
				else
				{
					sb.AppendLine($"<h2>{Res.GetString("bf4694f8-0444-4d49-8ee5-93eaeb08a907", "Previous Messages")}</h2>");

					AddFormattedMessages(sb, previousMessages.OrderByDescending(m => m.SystemCreateTimeInUtc));
				}

				return sb.ToString();
			}
		}

		public ZString EmailIdentifier => EConversationUniqueIDUtil.GenerateElement(conversation.Parent);

		public ZString Summary
		{
			get
			{
				var provider = conversation.Parent as IConversationWithDetails;

				return provider?.Summary ?? ZString.Empty;
			}
		}

		public ZString DetailedDescription
		{
			get
			{
				var provider = conversation.Parent as IConversationWithDetails;

				return provider?.DetailedDescription ?? ZString.Empty;
			}
		}

		static void AddFormattedMessages(StringBuilder sb, IEnumerable<IConversationMessage> messages)
		{
			// Tables are just about the only thing supported in email clients. Need to abuse them for emails.

			sb.AppendLine("<table>");
			foreach (var msg in messages)
			{
				sb.AppendLine("<tr>");

				// Display Name
				sb.AppendLine("<td align=\"right\" style=\"font-weight: bold; width: 120px; padding: 5px 10px 5px 5px;\">");
				sb.AppendLine(WebUtility.HtmlEncode(msg.SenderDisplayName));
				sb.AppendLine("</td>");

				// Message
				sb.AppendLine("<td style=\"border: solid 1px #A9A9A9; border-radius: 10px; padding: 5px 5px; width: 440px; background-color: #F5F5F5;\">");
				sb.AppendLine("<table border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\" height=\"100%\" bgcolor=\"#F5F5F5\" style=\"background-color: #F5F5F5;\">");

				// Message Body
				sb.Append("<tr width=\"100%\"><td style=\"background-color: #F5F5F5;\">");

				sb.Append(Regex.Replace(WebUtility.HtmlEncode(msg.Body), "\r\n|(?<!\r)\n", match => "<br />" + match.ToString()));

				sb.AppendLine("</td></tr>\n");

				// Time/Date
				sb.Append("<tr width=\"100%\"><td align=\"right\" style=\"color: #696969;background-color: #F5F5F5;\"><em style=\"font-size: 10px\">");

				var localTime = Env.Time.GetLocalTimeFromUtc(msg.SystemCreateTimeInUtc.ToDateTime());
				sb.Append(new ZDateTime(localTime, DateTimeKind.Local).ToSmallDateTime());
				sb.AppendLine("</em></td></tr>");

				sb.AppendLine("</table>");
				sb.AppendLine("</td>");
				sb.AppendLine("</tr>");
			}
			sb.AppendLine("</table>");
		}

		#endregion
	}

	class EConversationMessageNotificationParser : DocumentParser<EConversationMessageNotification>
	{
		public EConversationMessageNotificationParser(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override Type TypeOfWrapper => typeof(EConversationMessageNotificationWrapper);

		protected override ZString ParseCore(EConversationMessageNotification objectToWrap, ZString documentText)
		{
			DocBuilderParsingRoots = new[] { GetDocWrapper(objectToWrap, Factory) };

			return base.ParseCore(objectToWrap, documentText);
		}
	}

	public class EConversationMessageNotificationWrapper : DocumentWrapper, DocumentWrappers.IDocEConversationMessageNotification
	{
		public EConversationMessageNotificationWrapper()
		{
		}

		EConversationMessageNotificationWrapper(EConversationMessageNotification processResult, BusinessObjectFactory factoryToWrap)
			: base(processResult, factoryToWrap)
		{ }

		public static EConversationMessageNotificationWrapper New(EConversationMessageNotification processResult, BusinessObjectFactory factoryToWrap)
		{
			return (processResult != null) ? new EConversationMessageNotificationWrapper(processResult, factoryToWrap) : null;
		}

		EConversationMessageNotification ProcessResult => (EConversationMessageNotification)WrappedObject;

		[DocumentField("ID")]
		public ZString ID => ProcessResult.ID;

		[DocumentField("Business Object Name")]
		public ZString BusinessObjectName => ProcessResult.BusinessObjectName;

		[DocumentField("Business Object Hyperlink")]
		public ZString BusinessObjectHyperlink => ProcessResult.BusinessObjectHyperlink;

		[DocumentField("New Messages")]
		public ZString NewMessages => ProcessResult.NewMessages;

		[DocumentField("Previous Messages")]
		public ZString PreviousMessages => ProcessResult.PreviousMessages;

		[DocumentField("Is Internal Recipient")]
		public ZBool IsInternalRecipient => ProcessResult.IsInternalRecipient;

		[DocumentField("UTC Offset")]
		public ZString UtcOffset => ProcessResult.UtcOffset;

		[DocumentField("Email Identifier")]
		public ZString EmailIdentifier => ProcessResult.EmailIdentifier;

		[DocumentField("Business Summary")]
		public ZString Summary => ProcessResult.Summary;

		[DocumentField("Business Detailed Description")]
		public ZString DetailedDescription => ProcessResult.DetailedDescription;
	}
}

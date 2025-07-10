using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using CargoWise.Application;
using CargoWise.Definitions.Authentication;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	public static class SupportIncidentEmailBodyGeneralControls
	{
		public static string GetCommonEmailSubject(string actionName, string anotherInformation = "")
		{
			var result = $"{actionName} on Incident: (*IncidentNumber*) - (*IncidentDescription*)";
			if (string.IsNullOrEmpty(anotherInformation))
			{
				return result;
			}
			else
			{
				return $"{result} {anotherInformation}";
			}
		}

		#region URL/Hyper Link

		public static string GetIncidentFormUrl(IncidentMainBase dataSource, ClientControllerID controllerID)
		{
			return ObjectFactory.Get<IShowEditFormUrlCreator>().Create(controllerID, dataSource.PK.ToGuid());
		}

		public static string GetIncidentFormHyperlink(IncidentMainBase dataSource, ClientControllerID controllerID, string displayContents)
		{
			return $"<a href='{GetIncidentFormUrl(dataSource, controllerID)}'>{displayContents}</a>";
		}

		public static string GetIncidentGlowUrl(SupportIncident dataSource)
		{
			const string PkMacro = EDIDataRegistry.GlowEditERequestPageUri_PkMacro;
			var rootUrl = EDIDataRegistry.Instance.GlowPortalRootUrl.Value.TrimEnd('/');
			var portalPageUri = EDIDataRegistry.Instance.GlowEditERequestPageUri.Value;
			var url = rootUrl + "/" + portalPageUri.TrimStart('/').Replace(PkMacro, dataSource.Request.PK.ToString());

			url += url.Contains("?") ? "&OrgCode=" + dataSource.ClientCode : "?OrgCode=" + dataSource.ClientCode;
			return url;
		}

		public static string GetIncidentGlowHyperlink(SupportIncident dataSource, string displayContents = "")
		{
			if (string.IsNullOrEmpty(displayContents))
			{
				displayContents = $"Incident number {dataSource.IM_IncidentNumber}";
				if (!dataSource.IM_ClientIncidentReference.IsEmpty)
				{
					displayContents += $" / {dataSource.IM_ClientIncidentReference}";
				}
			}
			return $"<a href='{GetIncidentGlowUrl(dataSource)}'>{displayContents}</a>";
		}

		#endregion

		public static string GetReplyViaEConversationButton(SupportIncident dataSource, bool isPrimaryButton = true)
		{
			var replyViaEConversationButtonBlue = ConvertImageToBase64("Images.ReplyViaEConversationBlue.png");
			var replyViaEConversationButtonRed = ConvertImageToBase64("Images.ReplyViaEConversationRed.png");
			return GetButtonCore(isPrimaryButton ? replyViaEConversationButtonBlue : replyViaEConversationButtonRed, $"{EConversationHyperlinkButtonID}{dataSource.Number}", GetIncidentGlowUrl(dataSource), "Reply Via eConversation button");
		}

		public const string EConversationHyperlinkButtonID = "eConversationButton";

		public static string GetConfirmResolvedButton(SupportIncident dataSource)
		{
			var accessControl = ObjectFactory.Get<ITokenizedAccessControl>();
			var token = accessControl.CreateLimitedToken(AccessTokenTypes.IncidentEmailActionLink, new AccessTokenInfo(ConfirmResolvedButtonID, dataSource.PK.ToGuid(), dataSource.TablePrefix), TimeSpan.FromDays(7), 1);
			var url = $"{EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/')}/api/incident/confirm-resolve?token={token}";
			var confirmResolvedButton = ConvertImageToBase64("Images.ConfirmResolvedBlue.png");
			return GetButtonCore(confirmResolvedButton, $"{ConfirmResolvedButtonID}{dataSource.Number}", url, "Confirm Resolved button");
		}

		public const string ConfirmResolvedButtonID = "ConfirmResolvedButton";

		static string GetButtonCore(string buttonBase64, string buttonID, string url, string altAttribute)
		{
			var result = new ZStringBuilder();

			result.Append(FormattableString.Invariant($"<a id='{buttonID}' href='{url}' target='_blank' style='display: inline-block; text-decoration: none;'>"));
			result.Append(FormattableString.Invariant($"<img alt='{altAttribute}' src='data:image/png;base64, {buttonBase64}'></a>"));

			return result.ToString();
		}

		public static string ConvertImageToBase64(string imagePath)
		{
			var resourceRetriever = new EmbeddedResourceRetriever(typeof(SupportIncidentEmailBodyGeneralControls).Assembly);
			var imageBytes = resourceRetriever.GetBytes("ZClientEDI.Business." + imagePath);
			return Convert.ToBase64String(imageBytes);
		}

		public static string GetFormattedEConversationChatHistory(IEnumerable<IConversationMessage> messages, string title)
		{
			var sb = new ZStringBuilder();
			if (!string.IsNullOrEmpty(title))
			{
				sb.AppendLine($"<h2>{title}</h2>");
			}
			sb.AppendLine("<table>");
			foreach (var message in messages)
			{
				sb.AppendLine(GetFormattedEConversationMessageBubble(message));
			}
			sb.AppendLine("/<table>");
			return sb.ToString();
		}

		public static string GetFormattedEConversationChatHistory(IEnumerable<IConversationMessage> messages)
		{
			var eConversationMessagesStringBuilder = new ZStringBuilder();
			foreach (var message in messages)
			{
				eConversationMessagesStringBuilder.AppendLine(message.Body.CleanUpTextForHTML());
			}
			return eConversationMessagesStringBuilder.ToString();
		}

		public static string GetFormattedEConversationMessageBubble(IConversationMessage message)
		{
			var sb = new StringBuilder();
			sb.AppendLine("<tr>");

			// Display Name
			sb.AppendLine("<td align=\"right\" style=\"font-weight: bold; width: 120px; padding: 5px 10px 5px 5px;\">");
			sb.AppendLine(WebUtility.HtmlEncode(message.SenderDisplayName));
			sb.AppendLine("</td>");

			// Message
			sb.AppendLine("<td style=\"border: solid 1px #A9A9A9; border-radius: 10px; padding: 5px 5px; width: 440px; background-color: #F5F5F5;\">");
			sb.AppendLine("<table border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\" height=\"100%\" bgcolor=\"#F5F5F5\" style=\"background-color: #F5F5F5;\">");

			// Message Body
			sb.Append("<tr width=\"100%\"><td style=\"background-color: #F5F5F5;\">");
			sb.Append(WebUtility.HtmlEncode(message.Body).Replace("\n", "<br />\n"));
			sb.AppendLine("</td></tr>\n");

			// Time/Date
			sb.Append("<tr width=\"100%\"><td align=\"right\" style=\"color: #696969;background-color: #F5F5F5;\"><em style=\"font-size: 10px\">");

			var localTime = Env.Time.GetLocalTimeFromUtc(message.SystemCreateTimeInUtc.ToDateTime());
			sb.Append(new ZDateTime(localTime, DateTimeKind.Local).ToSmallDateTime());
			sb.AppendLine("</em></td></tr>");

			sb.AppendLine("</table>");
			sb.AppendLine("</td>");
			sb.AppendLine("</tr>");

			return sb.ToString();
		}

		public static string GetCreateFollowUpERequestButton(SupportIncident dataSource)
		{
			var createFollowUpERequestButton = ConvertImageToBase64("Images.CreateFollowUpERequestBlue.png");
			var url = GetFollowUpERequestUrl(dataSource);
			return GetButtonCore(createFollowUpERequestButton, $"{EConversationHyperlinkButtonID}{dataSource.Number}", url, "Create Follow-up eRequest button");
		}

		public static string GetFollowUpERequestUrl(SupportIncident dataSource)
		{
			var rootUrl = EDIDataRegistry.Instance.GlowPortalRootUrl.Value.TrimEnd('/');
			return $"{rootUrl}/goto/FollowUpERequest?incidentNumber={dataSource.Request.INC_IncidentNumber}";
		}
	}

	public static class SupportIncidentEmailExternalResources
	{
		public static string AssignedStaffEmailNotificationLetterFilePath => "Enterprise.Client.EDI.IncidentManager.Business.Common.AssignedStaffEmailNotificationLetter.txt";
	}
}

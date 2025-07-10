using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate
{
	static class RequestHyperlinkButtonChecker
	{
		public static bool CanAddButton(SupportIncident dataSource, string emailBody)
		{
			if (!IsDataSourceValid(dataSource))
			{
				return false;
			}

			if (string.IsNullOrEmpty(emailBody))
			{
				return true;
			}

			return !emailBody.Contains(SupportIncidentEmailBodyGeneralControls.EConversationHyperlinkButtonID + dataSource.Number);
		}

		public static string CheckAndGetEmailBodyWithButton(SupportIncident dataSource, string emailBody)
		{
			if (CanAddButton(dataSource, emailBody))
			{
				return $"{emailBody}{GetActionButtons(dataSource)}";
			}

			return emailBody;
		}

		public static string GetActionButtons(SupportIncident dataSource, bool shouldAddConfirmResolvedButton = false)
		{
			var newBody = new ZStringBuilder();
			newBody.Append("<p>");
			if (shouldAddConfirmResolvedButton)
			{
				newBody.Append(SupportIncidentEmailBodyGeneralControls.GetConfirmResolvedButton(dataSource));
				newBody.Append("&nbsp;&nbsp;&nbsp;");
			}
			newBody.Append(SupportIncidentEmailBodyGeneralControls.GetReplyViaEConversationButton(dataSource, isPrimaryButton: !shouldAddConfirmResolvedButton));

			newBody.Append("</p>");
			newBody.Append($"Regards,<br/>");

			return newBody.ToString();
		}

		public static string GetCreateFollowUpERequestButton(SupportIncident dataSource)
		{
			var newBody = new ZStringBuilder();
			newBody.Append($"<p>{SupportIncidentEmailBodyGeneralControls.GetCreateFollowUpERequestButton(dataSource)}</p>");
			newBody.Append($"Regards,<br/>");

			return newBody.ToString();
		}

		static bool IsDataSourceValid(SupportIncident dataSource)
		{
			return dataSource != null && dataSource.Request != null;
		}
	}
}

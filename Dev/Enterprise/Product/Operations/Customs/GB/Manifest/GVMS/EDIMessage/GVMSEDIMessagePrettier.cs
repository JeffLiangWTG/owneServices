using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using static System.FormattableString;

namespace Enterprise.Customs.GB.GVMS
{
	public class GVMSEDIMessagePrettier
	{
		public GVMSEDIMessagePrettier(GVMSEDIMessage message)
		{
			Message = message ?? throw new ArgumentNullException(nameof(message));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "Baseline")]
		public ZString MakeHumanReadable()
		{
			GVMSMessageDataObject messageData = Message.MessageDataObject;
			ZString prettyState = "";
			ZString prettyShortState = "";
			if (messageData.gmrState != null)
			{
				prettyShortState = messageData.gmrState;
				prettyState = new GVMSCustomsStatus().GetDescriptionFromCode(new GVMSRegistrationStatus().GetCodeFromDescription(messageData.gmrState) ?? string.Empty);
			}
			ZString prettyHtml = (ZString)MessagePrettierCss.CSS;
			var createdDateTime = DateTime.Parse(messageData.createdDateTime).ToUniversalTime();
			var prettyCreatedDateTime = new ZDateTime(Env.Time.GetLocalTimeFromUtc(createdDateTime));

			var updatedDateTime = DateTime.Parse(messageData.updatedDateTime).ToUniversalTime();
			var prettyUpdatedDateTime = new ZDateTime(Env.Time.GetLocalTimeFromUtc(updatedDateTime));

			prettyHtml += string.Format("<H3>GVMS Response</H3>" +
							"<p><strong>GMR ID:</strong>{0}<br/>" +
							"<strong>Status:</strong>{1} - {2}<br/>" +
							"<strong>Created Date/Time:</strong>{3}<br/>" +
							"<strong>Updated Date/Time:</strong>{4}<br/>" +
							"<strong>Version:</strong>{5}<br/>", messageData.gmrId, prettyShortState, prettyState, prettyCreatedDateTime, prettyUpdatedDateTime, messageData.gmrStatusVersion);

			if (messageData.reportToLocations?.Count > 0)
			{
				var typeLookup = GvmsInspectionAtLocationCusCodeDataLookups.GetInspectionTypeList(Message.Factory);
				var locationLookup = GvmsInspectionAtLocationCusCodeDataLookups.GetInspectionLocationList(Message.Factory);
				var locationHtml = new ZStringBuilder("<strong>Inspection(s) Details:</strong>");
				locationHtml.Append("<ul style=\"margin-top: 3px;\">");
				foreach (var inspection in messageData.reportToLocations)
				{
					foreach (var locationId in inspection.locationIds)
					{
						locationHtml.Append($"<li>By {typeLookup.GetDescriptionFromCode(inspection.inspectionTypeId)} at {locationLookup.GetDescriptionFromCode(locationId)} " +
							$"({inspection.inspectionTypeId}, {locationId})</li>");
					}
				}
				locationHtml.Append("</ul>");
				prettyHtml += locationHtml.ToString();
			}

			if (messageData.ruleFailures != null && messageData.ruleFailures.Count > 0)
			{
				prettyHtml += "<p><b>Rejection Details:</b><br/>";
				List<RuleFailure> rules = messageData.ruleFailures;
				foreach (RuleFailure rule in rules)
				{
					var errorMessage = GetErrorDescriptionFromDB(rule.code);
					if (!errorMessage.IsEmpty && errorMessage != rule.technicalMessage)
					{
						errorMessage = " (" + errorMessage + ")";
					}
					else
					{
						errorMessage = "";
					}
					errorMessage = rule.technicalMessage + errorMessage;

					char[] charsToTrim = { '$', '.' };
					prettyHtml += string.Format("<strong>Description:</strong>{0}<br/>" +
									"<strong>Field Name:</strong>{1}<br/>", errorMessage, rule.field.Trim(charsToTrim));
					if (rule.value != null)
					{
						prettyHtml += "<strong>Value:</strong>" + rule.value;
					}
					prettyHtml += "<p>";
				}
			}
			return prettyHtml;
		}

		public static ZString GetCIDMessage(ZGuid trackingID, EDIMessage originalMessage, EDIMessage message, ZString recipient, ZDateTime? eventTime)
		{
			var prettyString = MessagePrettierCss.ProgressCSS +
				string.Format(CultureInfo.CurrentCulture,
						HtmlForCID,
						GetCIDReceivedMessage(originalMessage.EM_MessageNum, message.EM_ApplicationReference),
						originalMessage.EM_MessageNum.IsEmpty ? NotDone : Done,
						MessageNumText(originalMessage),
						trackingID.IsEmpty ? NotDone : Done,
						recipient,
						Invariant($"{recipient} Tracking ID = {trackingID}"),
						message.EM_ApplicationReference.IsEmpty ? NotDone : Done,
						Invariant($"Tracking ID = {message.EM_ApplicationReference}"));

			prettyString += GetExtraData(originalMessage.EM_LinkedObject as AsycudaManifestHeader, eventTime);

			return prettyString;
		}

		ZString GetErrorDescriptionFromDB(ZString errorCode)
		{
			var errorDescription = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(
				Message.Factory,
				errorCode,
				Core.Constants.CountryCodes.UnitedKingdom,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ErrorCode,
				ZDateTime.Today,
				attributeFilters: new[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.Category, JoinCondition.And, Enterprise.Customs.GB.GVMS.Constants.MessageSubTypePreFixes.GVMS) })
				?.ZZD_Description ?? ZString.Empty;
			return errorDescription;
		}

		protected GVMSEDIMessage Message { get; }

		static string MessageNumText(EDIMessage originalMessage) => Invariant($"Message number = {originalMessage.EM_MessageNum} at {originalMessage.EM_SystemCreateTimeUtc.ToString("dd/MM/yyyy HH:mm")} (UTC)");
		static ZString GetCIDReceivedMessage(ZString messageNum, ZString trackingID) => string.Format(CultureInfo.CurrentCulture, MessageReceivedCID, messageNum, trackingID);
		public static ZString GetExtraData(AsycudaManifestHeader header, ZDateTime? eventTime) => string.Format(CultureInfo.CurrentCulture, ExtraDataHtml, eventTime.HasValue ? eventTime.Value.ToString("dd/MM/yyyy HH:mm") + " (UTC)" : string.Empty, header?.AMA_JobReference);

		const string MessageReceivedCID = "Message {0} was uploaded to GVMS and received Tracking ID {1}";

		const string HtmlForCID = @"<p><h4>{0}</h4></p><table>
<tr>
	{1}
	<td class=""Status"">Message created</td>
	<td><small>{2}</small></td>
</tr>
<tr>
	{3}
	<td class=""Status"">Sent to {4}</td>
	<td><small>{5}</small></td>
</tr>
<tr>
	{6}
	<td class=""Status"">Sent to GVMS</td>
	<td><small>{7}</small></td>
</tr>
</table>";

		const string ExtraDataHtml = @"<p/><table>
<tr style=""height: 20px;"">
<td>Status update time:</td>
<td><small>{0}</small></td>
</tr>
<tr style=""height: 20px;"">
<td>Job number:</td>
<td><small>{1}</small></td>
</tr>
</table>";

		const string Done = @"<td class=""StepProgress-item is-done"">&#10004;</td>";

		const string NotDone = @"<td class=""StepProgress-item current"">&#10004;</td>";
	}
}


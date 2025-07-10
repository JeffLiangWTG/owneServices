using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using static System.FormattableString;
using Context = Enterprise.UniversalDataBuss.DataObjects.Universal.Context;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.AU.Declaration.Business
{
	internal static class RFPMessageInterpretationGenerator
	{
		public static ZString GetInterpretatedHTML(RFPEDIMessage message)
		{
			Argument.NotNull(message, "message");

			var messageDetail = ZString.Empty;
			try
			{
				var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
				var templateModel = GetTemplateModel(universalEvent);
				messageDetail = Render(templateModel);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{ }

			if (messageDetail.IsEmpty)
			{
				messageDetail = ParsingFailureResult;
			}
			return messageDetail;
		}

		static TemplateModel GetTemplateModel(UniversalEvent universalEvent)
		{
			var result = new TemplateModel
			{
				StyleSheet = SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value,
				JobNo = GetJobNo(universalEvent)
			};

			var decoder = new NEXDOCEventDecoder(universalEvent, null);
			var responseType = decoder.GetResponseType(universalEvent);
			result.ResponseType = responseType;

			switch (responseType)
			{
				case Constants.ReponseType.Error:
					{
						result.Messages = GetErrorMessages(universalEvent);
						break;
					}

				case Constants.ReponseType.Notify:
					{
						result.Messages = GetNotifyMessages(universalEvent);
						break;
					}

				case Constants.ReponseType.Receipt:
				case Constants.ReponseType.Replace:
					{
						result.Messages = new List<string>() { universalEvent.ContextCollection.GetContextValue(Constants.ContextType.Message) };
						break;
					}

				default:
					{
						result.RFPNo = GetRFPNo(universalEvent);
						result.RFPStatusWithDescription = GetRFPStatus(universalEvent);
						result.CustomsAuthorityNo = GetCustomsAuthorityNo(universalEvent);
						result.PermitNumber = GetPermitNumber(universalEvent);
						result.Messages = IsRexCanResponse(universalEvent) ? GeRexCanResponseMessages(universalEvent) : GetSuccessMessages(universalEvent);
						break;
					}
			}

			return result;
		}

		static string GetCustomsAuthorityNo(UniversalEvent universalEvent)
		{
			return universalEvent.ContextCollection.GetContextValue(Constants.ContextType.ExportPermitNo);
		}

		static string GetRFPStatus(UniversalEvent universalEvent)
		{
			string result = string.Empty;
			var status = universalEvent.ContextCollection.GetContextValue(Constants.ContextType.RFPStatus);
			if (!status.IsNullOrEmpty())
			{
				var description = new EXDOCComplianceStatusCodes().GetDescriptionFromCode(status);
				result = Invariant($"{status} - {description}");
			}
			return result;
		}

		static string GetRFPNo(UniversalEvent universalEvent)
		{
			return universalEvent.ContextCollection.GetContextValue(Constants.ContextType.RFPNo);
		}

		static string GetPermitNumber(UniversalEvent universalEvent)
		{
			return universalEvent.ContextCollection.GetContextValue(Constants.ContextType.PermitNumber);
		}

		static List<string> GetSuccessMessages(UniversalEvent universalEvent)
		{
			return universalEvent.ContextCollection
				.Where(x => x.IsContextType(Constants.ContextType.ValidationNotice))
				.Select(x => (string)GetNoticeMessage(x))
				.Where(x => !x.IsNullOrEmpty()).ToList();
		}

		static ZString GetNoticeMessage(Context x)
		{
			return x.SubContextCollection?.FirstOrDefault(y => y.IsContextType(Constants.SubContextType.NoticeMessage))?.Value.GetValueOrDefault() ?? ZString.Empty;
		}

		static List<string> GetNotifyMessages(UniversalEvent universalEvent)
		{
			var result = new List<string>();
			var notificationTitle = universalEvent.ContextCollection.GetContextValue(Constants.ContextType.NotificationTitle);
			if (!notificationTitle.IsNullOrEmpty())
			{
				result.Add(notificationTitle);
			}
			var notificationText = universalEvent.ContextCollection.GetContextValue(Constants.ContextType.NotificationText);
			if (!notificationText.IsNullOrEmpty())
			{
				result.Add(notificationText);
			}

			return result;
		}

		static List<string> GetErrorMessages(UniversalEvent universalEvent)
		{
			return universalEvent.ContextCollection.Where(x => x.IsContextType(Constants.ContextType.Message))
				.Select(x => (string)x.Value.GetValueOrDefault())
				.Where(x => !x.IsNullOrEmpty()).ToList();
		}

		static string GetJobNo(UniversalEvent universalEvent)
		{
			return universalEvent.GetMatchingDataTarget(DataContextType.CustomsDeclaration)?.Key ?? string.Empty;
		}

		static bool IsRexCanResponse(UniversalEvent universalEvent)
		{
			return universalEvent.EventType.HasValue && (universalEvent.EventType.Value == Events.MessageReceivedCode)
				&& universalEvent.EventReference.HasValue && (universalEvent.EventReference.Value == Enterprise.Customs.AU.Declaration.Business.Constants.EventReference.CanRex);
		}

		static List<string> GeRexCanResponseMessages(UniversalEvent universalEvent)
		{
			var result = new List<string>();

			var messageContext = universalEvent.ContextCollection.FirstOrDefault(context => context.IsContextType(Constants.ContextType.Message));
			if (messageContext != null && messageContext.Value.HasValue)
			{
				result.Add(messageContext.Value.Value);
			}

			var serviceRequestIdentifierContext = universalEvent.ContextCollection.FirstOrDefault(context => context.IsContextType(Constants.ContextType.ServiceRequestIdentifier));
			if (serviceRequestIdentifierContext != null && serviceRequestIdentifierContext.Value.HasValue)
			{
				result.Add(Res.GetString("891F67FD-FAC2-43A4-A667-E236E0B298F7", "Service Request Identifier: {0}", serviceRequestIdentifierContext.Value.Value));
			}

			return result;
		}

		static string Render(TemplateModel model)
		{
			var sb = new ZStringBuilder();
			sb.Append(@"<html xmlns=""http://www.w3.org/1999/xhtml"">");
			sb.Append(Invariant($@"<head><title>Message Detail</title><style type=""text/css"">{model.StyleSheet}</style></head>"));
			sb.Append(Invariant($@"<body>{RenderHtmlBodyContent(model)}<body>"));
			sb.Append("</html>");
			return sb.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		static string RenderHtmlBodyContent(TemplateModel model)
		{
			var jobNumber = Res.GetString("b53e1249-d2a5-475c-b34d-d983e2846213", "Job Number: {0}", model.JobNo);

			var sb = new ZStringBuilder();
			sb.AppendHtmlLine(Invariant($"<b>{jobNumber}</b>"));

			if (model.ResponseType == Constants.ReponseType.Success)
			{
				var rfpNumber = Res.GetString("73e43f56-b80d-406a-99e5-4f5471fd013c", "RFP Number: {0}", model.RFPNo);
				sb.AppendHtmlLine(Invariant($"<b>{rfpNumber}</b>"));

				if (!model.RFPStatusWithDescription.IsNullOrEmpty())
				{
					var rfpStatusWithDescription = Res.GetString("6d453cc9-9ae6-4e90-9f34-64f537552575", "RFP Status: {0}", model.RFPStatusWithDescription);
					sb.AppendHtmlLine(Invariant($"<b>{rfpStatusWithDescription}</b>"));
				}

				if (!model.PermitNumber.IsNullOrEmpty())
				{
					var permitNumber = Res.GetString("7E3ED03A-3126-46FF-B000-867248D8BD16", "Permit Number: {0}", model.PermitNumber);
					sb.AppendHtmlLine(Invariant($"<b>{permitNumber}</b>"));
				}

				if (!model.CustomsAuthorityNo.IsNullOrEmpty())
				{
					var customsAuthorityNumber = Res.GetString("1af40a10-e542-4540-8f1a-ea6728074d09", "Customs Authority Number: {0}", model.CustomsAuthorityNo);
					sb.AppendHtmlLine(Invariant($"<b>{customsAuthorityNumber}</b>"));
				}
			}
			sb.AppendHtmlLine();

			if (model.ResponseType == Constants.ReponseType.Receipt)
			{
				sb.AppendHtmlLine(RenderMessageTable(model));
			}
			else
			{
				sb.AppendHtmlLine(model.ResponseType == Constants.ReponseType.Notify || model.ResponseType == Constants.ReponseType.Replace
					? Res.GetString("4d26fa72-ef62-456a-9566-8c3d6c351ced", "A Notification response message has been received from Quarantine.")
					: Res.GetString("ce2aef22-82ee-449d-b3aa-6d37255447ca", "A response message has been received from Quarantine."));

				if (model.Messages.Count > 0)
				{
					sb.AppendHtmlLine(Res.GetString("054a9a0a-ca82-4d2d-8d93-d94ac2b47a8f", "Shown below is a summary of relevant information received in this message."));
					sb.AppendHtmlLine();
					sb.AppendHtmlLine(RenderMessageTable(model));
				}
				else
				{
					sb.AppendHtmlLine();
				}
			}

			sb.AppendHtmlLine(Res.GetString("d714a979-0a9a-4a0e-b92d-3f1b582d939c", "Regards,"));
			sb.AppendHtmlLine();
			sb.AppendHtmlLine(Res.GetString("1ad4616c-1ee3-4434-a43b-ebc721a281a2", "{0} Administrative Message Sender", "CargoWise"));

			var tableCreator = new HtmlTableCreator(new NameValueCollection
			{
				{ "border", "1" },
				{ "cellpadding", "1" },
				{ "cellspacing", "0" },
				{ "width", "100%" },
				{ "class", "table" }
			})
			{
				EnableHTMLEncoding = false
			};
			tableCreator.WriteRowWithFormatting(new CellWithFormatting(sb.ToString()));
			return tableCreator.ToHtml();
		}

		static string RenderMessageTable(TemplateModel model)
		{
			var msgTableCreator = new HtmlTableCreator("table"
				, new List<string> {
					model.ResponseType == Constants.ReponseType.Notify || model.ResponseType == Constants.ReponseType.Replace
					? Res.GetString("db0cffb7-fe3a-4a6e-b8ad-d211becf4337", "Notification")
					: Res.GetString("588ef25b-b133-4d29-baf5-9060abede153", "Message")
				}
				, new NameValueCollection { { "width", "100%" } })
			{
				EnableHTMLEncoding = false
			};

			if (model.ResponseType == Constants.ReponseType.Replace)
			{
				foreach (var message in model.Messages)
				{
					msgTableCreator.WriteRowWithFormatting(new CellWithFormatting(Res.GetString("38498fe3-33b7-40c3-9621-a7bb9c8542f8", "Replacement Service Request Id - {0}", message)));
				}
			}
			else
			{
				foreach (var message in model.Messages)
				{
					msgTableCreator.WriteRowWithFormatting(new CellWithFormatting(message));
				}
			}

			return msgTableCreator.ToHtml();
		}

		#region Extension Methods

		static string GetContextValue(this IEnumerable<Context> contextCollection, string contextType)
		{
			return contextCollection.FirstOrDefault(x => x.IsContextType(contextType))?.Value.GetValueOrDefault() ?? ZString.Empty;
		}

		static bool IsContextType(this Context context, string contextType)
		{
			return context.Type.Type.GetValueOrDefault().EqualsIgnoringCase(contextType);
		}

		static ZStringBuilder AppendHtmlLine(this ZStringBuilder sb, string value)
		{
			return sb.Append(Invariant($"{value}<br />"));
		}

		static ZStringBuilder AppendHtmlLine(this ZStringBuilder sb)
		{
			return sb.Append("<br />");
		}

		#endregion

		class TemplateModel
		{
			public string StyleSheet { get; set; }
			public string ResponseType { get; set; }
			public string JobNo { get; set; }
			public string PermitNumber { get; set; }
			public string RFPNo { get; set; }
			public string RFPStatusWithDescription { get; set; }
			public string CustomsAuthorityNo { get; set; }
			public List<string> Messages { get; set; }
		}

		internal const string ParsingFailureResult = "<html><b>Failed parsing the message</b><br/>Please refer to 'Message Text' tab for more detailed information.</html>";

		public static class Constants
		{
			public static class ContextType
			{
				public const string ValidationNotice = "ValidationNotice";
				public const string MessageStatus = "MessageStatus";
				public const string NotificationTitle = "NotificationTitle";
				public const string NotificationText = "NotificationText";
				public const string Message = "Message";
				public const string RFPNo = "RexNumber";
				public const string RFPStatus = "ComplianceStatus";
				public const string ExportPermitNo = "ECNNumber";
				public const string PermitNumber = "PermitNumber";
				public const string ServiceRequestIdentifier = "ServiceRequestIdentifier";
			}

			public static class SubContextType
			{
				public const string NoticeMessage = "NoticeMessage";
			}

			public static class ContextValue
			{
				public const string MessageStatusError = "ERO";
			}

			public static class ReponseType
			{
				public const string Success = "success";
				public const string Error = "error";
				public const string Notify = "notify";
				public const string Receipt = "receipt";
				public const string Replace = "replace";
			}
		}
	}
}

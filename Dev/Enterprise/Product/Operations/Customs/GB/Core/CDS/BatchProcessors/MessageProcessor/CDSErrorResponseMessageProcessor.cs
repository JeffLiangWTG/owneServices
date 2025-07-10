using System;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSErrorResponseMessageProcessor : CDSMessageProcessor<CDSErrorResponseEDIMessage>
	{
		public CDSErrorResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString ProcessMessageCore(CDSErrorResponseEDIMessage cdsEDIMessage,
			BusinessObjectFactory factory)
		{
			EDIMessage lastOutgoingMessage = cdsEDIMessage?.LinkedEntry?.Messages?.Cast<EDIMessage>().FirstOrDefault(x => x.PK.ToString().Replace("-", string.Empty).Equals(cdsEDIMessage.EM_ApplicationReference, StringComparison.OrdinalIgnoreCase));
			cdsEDIMessage.EM_Status = EDIMessageStatusList.Codes.Failed;
			var htmlFriendlyError = new ZStringBuilder(MessagePrettierCss.CSS);
			try
			{
				var helper = new ErrorsHelper(cdsEDIMessage.EM_MessageText, "//errorResponse");

				htmlFriendlyError.AppendFormat(MessageHeader, lastOutgoingMessage?.EM_MessageNum ?? string.Empty);

				if (lastOutgoingMessage != null)
				{
					var namMessage = cdsEDIMessage?.LinkedEntry?.Messages?.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageType == CDSEDIMessageTypeList.Codes.NewAmendment
																												&& lastOutgoingMessage.PK.ToString().Replace("-", string.Empty).Equals(x.EM_ApplicationReference, StringComparison.OrdinalIgnoreCase));
					if (namMessage != null)
					{
						namMessage.EM_Status = EDIMessageStatusList.Codes.Discarded;
					}
				}

				if (helper.ErrorResponses.Any())
				{
					foreach (var error in helper.ErrorResponses)
					{
						htmlFriendlyError.AppendFormat("<li>{0}</li>", error.DisplayError);
					}
				}
				else
				{
					var errorResponseMessage = helper.XmlDocument.SelectSingleNode(ErrorResponseMessageXPath)?.InnerText;
					var errorResponseCode = helper.XmlDocument.SelectSingleNode(ErrorResponseCodeXPath)?.InnerText;
					if (!string.IsNullOrEmpty(errorResponseMessage))
					{
						htmlFriendlyError.AppendFormat("<li>{0}</li>", errorResponseMessage);
					}
					if (errorResponseCode == InvalidCredentails)
					{
						htmlFriendlyError.Append(htmlToExplainInvalidCredentials);
					}
				}

				cdsEDIMessage.EM_MessageInterpretation = htmlFriendlyError.ToString();
				cdsEDIMessage.EM_Status = EDIMessageStatusList.Codes.Received;
			}
			catch (XmlException ex)
			{
				htmlFriendlyError.AppendFormat(MessageHeader, lastOutgoingMessage?.EM_MessageNum ?? string.Empty);
				htmlFriendlyError.Append("\n<h3>Error reading xml</h3>\n" + ex.Message + "\nPlease refer to the Message Text tab for the original content.");
				cdsEDIMessage.EM_MessageInterpretation = htmlFriendlyError.ToString();
			}

			return cdsEDIMessage.EM_Status;
		}

		protected override string MessageFriendlyNameCore => "CDS Rejection Response Message";

		protected override ZString MessageType => CDSEDIMessageTypeList.Codes.EHubErrorResponse;

		const string MessageHeader = "<h3>Message {0} was rejected by an upstream system for the following reasons</h3>";
		const string ErrorResponseMessageXPath = "//errorResponse/message";
		const string ErrorResponseCodeXPath = "//errorResponse/code";
		const string InvalidCredentails = "INVALID_CREDENTIALS";

		public string htmlToExplainInvalidCredentials = @"<p>This usually means that "
								+ Core.Constants.ProductName
								+ "'s authority to use CDS on your behalf is no longer valid. The authority may have expired at the end of its 18-month life, or the authority may have been reset by HMRC or by the Government Gateway. You probably need to re-commence the authorisation process, even if CW1 shows that a record should not yet have expired. Please do not raise an eRequest until you have undertaken that step and attempted to send a second message.</p>"
								+ @"<h4>For further information please see our learning units</h4>"
								+ @"<ul>"
								+ @"<li>1BGB045 - How do I register "
								+ Core.Constants.ProductName
								+ " One for CDS access?</li>"
								+ @"</ul>";
	}
}

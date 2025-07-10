using System.Collections.Specialized;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business;

public class CustomsServiceErrorUniversalEventMessageInterpreter
{
	public CustomsServiceErrorUniversalEventMessageInterpreter(UniversalEventWrapper responseMessage)
	{
		response = Argument.NotNull(responseMessage, nameof(responseMessage));
	}
	readonly UniversalEventWrapper response;

	public ZString CreateMessageDetailsRejected()
	{
		string messageDetails = (NoResString)"<H3>Universal Event Service Error</H3>"; // html formatting
		var tableCreator = GetNewTableCreator();
		tableCreator.WriteRow((NoResString)"<strong>Event Type</strong>", (NoResString)"<strong>Message Type</strong>", (NoResString)"<strong>Reason</strong>"); // Row names for error table
		tableCreator.WriteRow(response.EventType, response.MessageType, response.Reason);
		messageDetails += tableCreator.ToHtml();

		return messageDetails;
	}

	HtmlTableCreator GetNewTableCreator()
	{
		var htmlAttributes =
			new NameValueCollection
			{
				{ (NoResString)"border", "1" }, // html formatting options
				{ (NoResString)"cellpadding", "1" }, // html formatting options
				{ (NoResString)"cellspacing", "0" }, // html formatting options
				TableInterpretation.Attributes.FullWidth,
				{ (NoResString)"class", (NoResString)"table" } // html formatting options
			};

		return new HtmlTableCreator(htmlAttributes) { EnableHTMLEncoding = false };
	}
}

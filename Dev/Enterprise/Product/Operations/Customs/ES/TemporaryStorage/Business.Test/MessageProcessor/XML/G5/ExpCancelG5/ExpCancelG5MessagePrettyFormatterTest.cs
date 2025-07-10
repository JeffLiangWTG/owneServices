using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.DE;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.DG;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.G5ExpCancelV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing
{
	class ExpCancelG5MessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new ExpCancelG5MessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedDeclaration()
		{
			var declarationResponse = SetResponseData("20240228140023", "24ES009999Y00094Y0", ChannelDe.V, "24ES00999880000448", "6FBUNZ2DEFR5Z2QL", "3F2F5F3B88F477D8");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Accepted Cancellation</H3>" +
				"<table border=\"0\"><tr><td>Cancellation Date:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 14:00:23</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>6FBUNZ2DEFR5Z2QL</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsError()
		{
			var declarationResponse = new G5ExpCancelV1Sal();
			var error1 = SetFunctionalError("10", "El mensaje enviado no cumple el esquema.", ErrorTypeDe.F, "GoodItem", 1);
			var error2 = SetFunctionalError("15", "Error en mensaje.", ErrorTypeDe.N, ZString.Empty);
			declarationResponse.Rejected = new RejectedDg()
			{
				ResponseCode = ResponseCodeDe.Re,
				Errors = new System.Collections.ObjectModel.Collection<ErrorDe> { error1, error2 }
			};

			var messagePrettyFormatter = new ExpCancelG5MessagePrettyFormatter(declarationResponse);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Error Type</strong></td><td><strong>Place</strong></td><td><strong>Goods Item Number</strong></td></tr>" +
				"<tr><td>10</td><td>El mensaje enviado no cumple el esquema.</td><td>F</td><td>GoodItem</td><td>1</td></tr>" +
				"<tr><td>15</td><td>Error en mensaje.</td><td>N</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>", messageInterpretationText);
		}

		G5ExpCancelV1Sal SetResponseData(ZString acceptanceDate, ZString mrn, ChannelDe circuit, ZString dsdtMrn, ZString declarationCSV, ZString csvCode)
		{
			var response = new G5ExpCancelV1Sal();
			response.EnvelopeG5 = new EnvelopeG5SalDg()
			{
				PreparationDate = acceptanceDate
			};
			response.Accepted = new AcceptedDg()
			{
				NotificationCsv = declarationCSV
			};
			return response;
		}

		ZString GetAcceptedInterpretationText(G5ExpCancelV1Sal response)
		{
			var messagePrettyFormatter = new ExpCancelG5MessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}

		ErrorDe SetFunctionalError(ZString code, ZString reason, ErrorTypeDe errorType, ZString place, int itemNumber = 0)
		{
			var error = new ErrorDe();
			error.Code = code;
			error.Description = reason;
			error.Type = errorType;
			error.Pointer = place;
			if (itemNumber != 0)
			{
				error.GoodsItemNumber = itemNumber;
			}

			return error;
		}
	}
}

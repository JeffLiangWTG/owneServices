using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.AnulaPreH7V1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.TD;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	sealed class CancellationH7MesssagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestThrowExceptionWhenResponseIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CancellationH7MessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAccepted()
		{
			var expectedInterpretation = "<H3>Accepted Cancellation</H3>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>OPC H7 Canceled</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Message ID:</td><td>&nbsp;&nbsp;</td><td>34567</td></tr></table>" +
				"<table border=\"0\"><tr><td>Cancellation Date:</td><td>&nbsp;&nbsp;</td><td>20-02-2025, 18:00:00</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>12345</td></tr></table>";

			var response = CreateAcceptedResponseData();
			var messagePrettyFormatter = new CancellationH7MessagePrettyFormatter(response);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			AssertEquals(expectedInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var expectedInterpretation = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Pointer</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Item</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>10</td><td>11</td><td>tag1</td><td>reason1</td><td>12</td><td>value1</td></tr>" +
				"<tr><td>2</td><td>3</td><td>tag2</td><td>reason2</td><td>4</td><td>value2</td></tr>" +
				"</table>";

			var response = CreatedRejectedResponseData();
			var messagePrettyFormatter = new CancellationH7MessagePrettyFormatter(response);
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();
			AssertEquals(expectedInterpretation, messageInterpretationText);
		}

		AnulaPreH7V1Sal CreateAcceptedResponseData()
		{
			var response = new AnulaPreH7V1Sal();
			response.Message = new MessageSalTd();
			response.EdeclarationCsvId = "12345";
			response.Message.PreparationDate = "20250220";
			response.Message.PreparationTime = "180000";
			response.Message.CorrelationIdentifier = "34567";
			response.Response = new ResponseTd();
			response.Response.ResponseCode = "A";
			response.Response.OperationCode = "OPC";
			return response;
		}

		AnulaPreH7V1Sal CreatedRejectedResponseData()
		{
			var response = new AnulaPreH7V1Sal();
			var error1 = new ErrorTd();
			error1.ErrorCode = 10;
			error1.ErrorPointerDataElement = 11;
			error1.ErrorPointerTag = "tag1";
			error1.ErrorReason = "reason1";
			error1.ErrorPointerGoodsItem = 12;
			error1.OriginalAttributeValue = "value1";
			var error2 = new ErrorTd();
			error2.ErrorCode = 2;
			error2.ErrorPointerDataElement = 3;
			error2.ErrorPointerTag = "tag2";
			error2.ErrorReason = "reason2";
			error2.ErrorPointerGoodsItem = 4;
			error2.OriginalAttributeValue = "value2";
			response.Error = [error1, error2];
			response.Response = new ResponseTd();
			response.Response.ResponseCode = "R";
			return response;
		}
	}
}

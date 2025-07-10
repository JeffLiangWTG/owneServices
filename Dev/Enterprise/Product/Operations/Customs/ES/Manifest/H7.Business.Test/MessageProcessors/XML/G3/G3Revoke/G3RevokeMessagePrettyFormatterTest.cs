using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G3.G3RevokeV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	class G3RevokeMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new G3RevokeMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAccepted()
		{
			var expectedAcceptedDeclarationText = "<H3>Accepted Declaration</H3>" + "<br>" +
				"<table border=\"0\"><tr><td>LRN:</td><td>&nbsp;&nbsp;</td><td>ACME20REF0000001</td></tr></table>" +
				"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>20ESG3B000000001Q6</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>RZSURPDJ8S62KF7U</td></tr></table>";

			var response = new G3RevokeV1Sal
			{
				Accepted = CreateAcceptedResponseData()
			};
			var messagePrettyFormatter = new G3RevokeMessagePrettyFormatter(response);
			CombineAssertions(() =>
			{
				var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
				AssertEquals("G3 Accepted", expectedAcceptedDeclarationText, messageInterpretationText);
			});
		}

		public void TestCreateMessageDetailsRejectedWithErrors()
		{
			var expectedRejectedDeclarationText = "<H3>Rejected</H3>" +
				"<br><table border=\"0\"><tr><td>LRN:</td><td>&nbsp;&nbsp;</td><td>ACME20REF0000001</td></tr></table>" +
				"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>20ESG3B000000001Q6</td></tr></table>" +
				"<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
					"<tr><td><strong>Code</strong></td><td><strong>Pointer</strong></td><td><strong>Description</strong></td></tr>" +
					"<tr><td>899</td><td>pointer 1</td><td>Description 1</td></tr>" +
					"<tr><td>900</td><td>pointer 2</td><td>Description 2</td></tr>" +
				"</table>";

			var err1 = CreateError("899", "pointer 1", "Description 1");
			var err2 = CreateError("900", "pointer 2", "Description 2");

			var response = new G3RevokeV1Sal
			{
				Message = new MessageSalTd
				{
					CorrId = "20250117170000000000",
				},
				Rejected = CreateRejectedResponseData(new Collection<ErrorTd> { err1, err2 })
			};
			var messagePrettyFormatter = new G3RevokeMessagePrettyFormatter(response);

			CombineAssertions(() =>
			{
				var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();
				AssertEquals("G3 Rejected with Errors", expectedRejectedDeclarationText, messageInterpretationText);
			});
		}

		AcceptedSalTd CreateAcceptedResponseData()
		{
			return new AcceptedSalTd
			{
				ResponseCode = CargoWise.Customs.ES.MessageDefinitions.Version1.G3.DE.ResponseCodeTd.Ac,
				Lrn = "ACME20REF0000001",
				Mrn = "20ESG3B000000001Q6",
				Csv = "RZSURPDJ8S62KF7U",
			};
		}

		RejectedSalTd CreateRejectedResponseData(Collection<ErrorTd> errors = null)
		{
			return new RejectedSalTd
			{
				ResponseCode = CargoWise.Customs.ES.MessageDefinitions.Version1.G3.DE.ResponseCodeTd.Re,
				Lrn = "ACME20REF0000001",
				Mrn = "20ESG3B000000001Q6",
				Errors = errors,
			};
		}

		ErrorTd CreateError(ZString code, ZString pointer, ZString description)
		{
			return new ErrorTd
			{
				Code = code,
				Pointer = pointer,
				Description = description,
			};
		}
	}
}

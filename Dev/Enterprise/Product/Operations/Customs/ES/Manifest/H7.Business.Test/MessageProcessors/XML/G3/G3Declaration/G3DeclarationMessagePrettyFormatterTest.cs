using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G3.DE;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G3.G3PresV1Sal;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	class G3DeclarationMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new G3DeclarationMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAccepted()
		{
			var expectedAcceptedDeclarationText = "<H3>Accepted Declaration</H3><br>" +
				"<table border=\"0\"><tr><td>Declaration Date Time</td><td>&nbsp;&nbsp;</td><td>20240227144150</td></tr></table>" +
				"<table border=\"0\"><tr><td>Presentation Date Time</td><td>&nbsp;&nbsp;</td><td>20240227144151</td></tr></table><br>" +
				"<table border=\"0\"><tr><td>LRN:</td><td>&nbsp;&nbsp;</td><td>ACME20REF0000001</td></tr></table>" +
				"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>20ESG3B000000001Q6</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>28SQ7LWDBC72MRKP</td></tr></table><br>" +
				"<table border=\"0\"><tr><td>Release Code</td><td>&nbsp;&nbsp;</td><td>VE</td></tr></table>" +
				"<table border=\"0\"><tr><td>Release Code Effective Date Time</td><td>&nbsp;&nbsp;</td><td>20200401124513</td></tr></table><br>" +
				"<strong>Master Consignment</strong><br>" +
				"<table border=\"0\"><tr><td>Previous Document Type</td><td>&nbsp;&nbsp;</td><td>337: 20200104AIR78456</td></tr></table>" +
				"<table border=\"0\"><tr><td>Transport Document Type</td><td>&nbsp;&nbsp;</td><td>N740: 99103033726</td></tr></table>" +
				"<table border=\"0\"><tr><td>Receptacle</td><td>&nbsp;&nbsp;</td><td>991030337260103668</td></tr></table>" +
				"<table border=\"0\"><tr><td>Release Code</td><td>&nbsp;&nbsp;</td><td>VE</td></tr></table><br>" +
				"<strong>House Consignment</strong><br>" +
				"<table border=\"0\"><tr><td>Transport Document</td><td>&nbsp;&nbsp;</td><td>5025: IR231113990HK</td></tr></table>" +
				"<table border=\"0\"><tr><td>Release Code</td><td>&nbsp;&nbsp;</td><td>VE</td></tr></table><br>" +
				"<strong>House Consignment</strong><br>" +
				"<table border=\"0\"><tr><td>Transport Document</td><td>&nbsp;&nbsp;</td><td>5025: IR231113991HK</td></tr></table>" +
				"<table border=\"0\"><tr><td>Release Code</td><td>&nbsp;&nbsp;</td><td>VE</td></tr></table>";

			var response = new G3PresV1Sal
			{
				Accepted = CreateAcceptedResponseData(2, true)
			};
			var messagePrettyFormatter = new G3DeclarationMessagePrettyFormatter(response);
			CombineAssertions(() =>
			{
				var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
				AssertEquals("G3 Declaration Accepted", expectedAcceptedDeclarationText, messageInterpretationText);
			});
		}

		public void TestCreateMessageDetailsAccepted_WithoutDocuments()
		{
			var expectedAcceptedDeclarationText = "<H3>Accepted Declaration</H3><br>" +
				"<table border=\"0\"><tr><td>Declaration Date Time</td><td>&nbsp;&nbsp;</td><td>20240227144150</td></tr></table>" +
				"<table border=\"0\"><tr><td>Presentation Date Time</td><td>&nbsp;&nbsp;</td><td>20240227144151</td></tr></table><br>" +
				"<table border=\"0\"><tr><td>LRN:</td><td>&nbsp;&nbsp;</td><td>ACME20REF0000001</td></tr></table>" +
				"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>20ESG3B000000001Q6</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>28SQ7LWDBC72MRKP</td></tr></table><br>" +
				"<table border=\"0\"><tr><td>Release Code</td><td>&nbsp;&nbsp;</td><td>VE</td></tr></table>" +
				"<table border=\"0\"><tr><td>Release Code Effective Date Time</td><td>&nbsp;&nbsp;</td><td>20200401124513</td></tr></table><br>" +
				"<strong>Master Consignment</strong><br>" +
				"<table border=\"0\"><tr><td>Receptacle</td><td>&nbsp;&nbsp;</td><td>991030337260103668</td></tr></table>" +
				"<table border=\"0\"><tr><td>Release Code</td><td>&nbsp;&nbsp;</td><td>VE</td></tr></table><br>" +
				"<strong>House Consignment</strong><br>" +
				"<table border=\"0\"><tr><td>Release Code</td><td>&nbsp;&nbsp;</td><td>VE</td></tr></table><br>" +
				"<strong>House Consignment</strong><br>" +
				"<table border=\"0\"><tr><td>Release Code</td><td>&nbsp;&nbsp;</td><td>VE</td></tr></table>";

			var response = new G3PresV1Sal
			{
				Accepted = CreateAcceptedResponseData(2, false)
			};
			var messagePrettyFormatter = new G3DeclarationMessagePrettyFormatter(response);
			CombineAssertions(() =>
			{
				var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
				AssertEquals("G3 Declaration Accepted", expectedAcceptedDeclarationText, messageInterpretationText);
			});
		}

		public void TestCreateMessageDetailsAccepted_WithOver20HouseConsignments_Shows20HouseConsignments()
		{
			var expectedAcceptedDeclarationText = "<H3>Accepted Declaration</H3><br>" +
				"<table border=\"0\"><tr><td>Declaration Date Time</td><td>&nbsp;&nbsp;</td><td>20240227144150</td></tr></table>" +
				"<table border=\"0\"><tr><td>Presentation Date Time</td><td>&nbsp;&nbsp;</td><td>20240227144151</td></tr></table><br>" +
				"<table border=\"0\"><tr><td>LRN:</td><td>&nbsp;&nbsp;</td><td>ACME20REF0000001</td></tr></table>" +
				"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>20ESG3B000000001Q6</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>28SQ7LWDBC72MRKP</td></tr></table><br>" +
				"<table border=\"0\"><tr><td>Release Code</td><td>&nbsp;&nbsp;</td><td>VE</td></tr></table>" +
				"<table border=\"0\"><tr><td>Release Code Effective Date Time</td><td>&nbsp;&nbsp;</td><td>20200401124513</td></tr></table><br>" +
				"<strong>Master Consignment</strong><br>" +
				"<table border=\"0\"><tr><td>Previous Document Type</td><td>&nbsp;&nbsp;</td><td>337: 20200104AIR78456</td></tr></table>" +
				"<table border=\"0\"><tr><td>Transport Document Type</td><td>&nbsp;&nbsp;</td><td>N740: 99103033726</td></tr></table>" +
				"<table border=\"0\"><tr><td>Receptacle</td><td>&nbsp;&nbsp;</td><td>991030337260103668</td></tr></table>" +
				"<table border=\"0\"><tr><td>Release Code</td><td>&nbsp;&nbsp;</td><td>VE</td></tr></table>";

			var amountOfHouseConsignments = 30;

			for (int i = 0; i < 20; i++)
			{
				var num = i % 10;
				expectedAcceptedDeclarationText += "<br><strong>House Consignment</strong><br>" +
				$"<table border=\"0\"><tr><td>Transport Document</td><td>&nbsp;&nbsp;</td><td>5025: IR23111399{num}HK</td></tr></table>" +
				"<table border=\"0\"><tr><td>Release Code</td><td>&nbsp;&nbsp;</td><td>VE</td></tr></table>";
			}

			expectedAcceptedDeclarationText += "<br>Note: only displaying details of first 20 House Consignments. Refer to 'Text' tab for details of all House Consignments.";

			var response = new G3PresV1Sal
			{
				Accepted = CreateAcceptedResponseData(amountOfHouseConsignments, true)
			};
			var messagePrettyFormatter = new G3DeclarationMessagePrettyFormatter(response);
			CombineAssertions(() =>
			{
				var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();
				AssertEquals("G3 Declaration Accepted With 20+ House Consignments Shows only 20 House Consignments and appends note message", expectedAcceptedDeclarationText, messageInterpretationText);
			});
		}

		public void TestCreateMessageDetailsRejectedWithErrors()
		{
			var expectedRejectedDeclarationText = "<H3>Rejected</H3>" +
				"<br><table border=\"0\"><tr><td>LRN:</td><td>&nbsp;&nbsp;</td><td>ACME20REF0000001</td></tr></table>" +
				"<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
					"<tr><td><strong>Code</strong></td><td><strong>Pointer</strong></td><td><strong>Description</strong></td></tr>" +
					"<tr><td>899</td><td>pointer 1</td><td>description 1</td></tr>" +
					"<tr><td>900</td><td>pointer 2</td><td>description 2</td></tr>" +
				"</table>";

			var response = new G3PresV1Sal
			{
				Rejected = CreateRejectedResponseData()
			};
			var messagePrettyFormatter = new G3DeclarationMessagePrettyFormatter(response);

			CombineAssertions(() =>
			{
				var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();
				AssertEquals("G3 Declaration Rejected with Errors", expectedRejectedDeclarationText, messageInterpretationText);
			});
		}

		AcceptedSalTd CreateAcceptedResponseData(int amountOfHouseConsignments, bool includeDocuments)
		{
			return new AcceptedSalTd
			{
				ResponseCode = CargoWise.Customs.ES.MessageDefinitions.Version1.G3.DE.ResponseCodeTd.Ac,
				Mrn = "20ESG3B000000001Q6",
				Csv = "28SQ7LWDBC72MRKP",
				EffPresentationDate = "20200401124513",
				ReleaseCode = "VE",
				Header = includeDocuments
					? CreateHeader(amountOfHouseConsignments)
					: CreateHeaderWithoutDocuments(amountOfHouseConsignments),
			};
		}

		HeaderTd CreateHeader(int amountOfHouseConsignments)
		{
			var houseConsignments = new Collection<HouseConsignmentTd>();
			for (int i = 0; i < amountOfHouseConsignments; i++)
			{
				var num = i % 10;
				houseConsignments.Add(new HouseConsignmentTd
				{
					PreviousDocument = new Collection<PreviousDocumentTd>
						{
							new PreviousDocumentTd
							{
								PrevDocType = "355",
								PrevDocRefNum = $"20ES0028017000000{num}",
							}
						},
					TransportDocument = new TransportDocumentTd
					{
						TransDocType = "5025",
						TransDocRefNum = $"IR23111399{num}HK",
					},
					ReleaseCode = "VE",
				});
			}
			return new HeaderTd
			{
				Lrn = "ACME20REF0000001",
				CustomsOffice = "",
				PersonPresentingGoods = "",
				DeclarationDate = "20240227144150",
				PresentationDate = "20240227144151",
				MasterConsignment = new Collection<MasterConsignmentTd> {
					new MasterConsignmentTd
					{
						PreviousDocument = new Collection<PreviousDocumentTd>
						{
							new PreviousDocumentTd
							{
								PrevDocType = "337",
								PrevDocRefNum = "20200104AIR78456",
							}
						},
						TransportDocument = new TransportDocumentTd
						{
							TransDocType = "N740",
							TransDocRefNum = "99103033726",
						},
						Receptacle = "991030337260103668",
						ReleaseCode = "VE",
						HouseConsignment = houseConsignments,
					}
				},
			};
		}

		HeaderTd CreateHeaderWithoutDocuments(int amountOfHouseConsignments)
		{
			var houseConsignments = new Collection<HouseConsignmentTd>();
			for (int i = 0; i < amountOfHouseConsignments; i++)
			{
				houseConsignments.Add(new HouseConsignmentTd
				{
					ReleaseCode = "VE",
				});
			}
			return new HeaderTd
			{
				Lrn = "ACME20REF0000001",
				CustomsOffice = "",
				PersonPresentingGoods = "",
				DeclarationDate = "20240227144150",
				PresentationDate = "20240227144151",
				MasterConsignment = new Collection<MasterConsignmentTd> {
					new MasterConsignmentTd
					{
						Receptacle = "991030337260103668",
						ReleaseCode = "VE",
						HouseConsignment = houseConsignments,
					}
				},
			};
		}

		RejectedSalTd CreateRejectedResponseData(Collection<ErrorTd> errors = null)
		{
			return new RejectedSalTd
			{
				ResponseCode = CargoWise.Customs.ES.MessageDefinitions.Version1.G3.DE.ResponseCodeTd.Re,
				Lrn = "ACME20REF0000001",
				Errors = new Collection<ErrorTd> {
					new ErrorTd
					{
						Code = "899",
						Pointer = "pointer 1",
						Description = "description 1",
					},
					new ErrorTd
					{
						Code = "900",
						Pointer = "pointer 2",
						Description = "description 2",
					}
				}
			};
		}
	}
}

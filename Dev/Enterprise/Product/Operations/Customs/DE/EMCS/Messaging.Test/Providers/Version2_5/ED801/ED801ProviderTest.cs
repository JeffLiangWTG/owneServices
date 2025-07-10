using System;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
{
	[TestedType(typeof(ED801Provider))]
	class ED801ProviderTest : InboundDataProviderTestCase<IED801, ED801Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED801Provider(null));
		}

		public void TestMessageGroup()
		{
			AssertEquals("EME", dataProvider.MessageGroup);
		}

		public void TestMessageSender()
		{
			AssertEquals("DE000050", dataProvider.MessageSender);
		}

		public void TestMessageIdentifier()
		{
			AssertEquals("0072260102", dataProvider.MessageIdentifier);
		}

		public void TestLocalReferenceNumber()
		{
			AssertEquals("19DE587500026773M4", dataProvider.LocalReferenceNumber);
		}

		public void TestMessageRecipient()
		{
			AssertEquals("MessageRecipient", "DE001348", dataProvider.MessageRecipient);
		}

		public void TestDateAndTimeOfValidationOfEadEsad()
		{
			AssertEquals(new DateTime(2020, 6, 11, 16, 59, 59), dataProvider.DateAndTimeOfValidationOfEadEsad);
		}

		public void TestExciseMovement()
		{
			CombineAssertions(() =>
			{
				var exciseMovement = dataProvider.ExciseMovement;
				AssertSame("Cached", exciseMovement, dataProvider.ExciseMovement);
				AssertEquals("AdministrativeReferenceCode", "MRN98761234", exciseMovement.AdministrativeReferenceCode);
				AssertEquals("SequenceNumber", "1", exciseMovement.SequenceNumber);
			});
		}

		public void TestDispatchImportOffice()
		{
			AssertEquals("DIO001", dataProvider.DispatchImportOffice);
			message.Body.EadContainer.DispatchImportOffice = null;
			AssertEquals(ZString.Empty, dataProvider.DispatchImportOffice);
		}

		public void TestDeliveryPlaceCustomsOffice()
		{
			AssertEquals("DPCO001", dataProvider.DeliveryPlaceCustomsOffice);
			message.Body.EadContainer.DeliveryPlaceCustomsOffice = null;
			AssertEquals(ZString.Empty, dataProvider.DeliveryPlaceCustomsOffice);
		}

		public void TestCompetentAuthorityDispatchOffice()
		{
			AssertEquals("CADO001", dataProvider.CompetentAuthorityDispatchOffice);
		}

		public void TestJourneyTime()
		{
			AssertEquals("12H", dataProvider.JourneyTime);
		}

		public void TestDestinationTypeCode()
		{
			AssertEquals("1", dataProvider.DestinationTypeCode);
		}

		public void TestTransportArrangement()
		{
			AssertEquals("3", dataProvider.TransportArrangement);
		}

		public void TestDispatchTime()
		{
			AssertEquals(new ZDateTime(2020, 7, 9, 7, 9, 0), dataProvider.DispatchTime);
			message.Body.EadContainer.EadEsad.TimeOfDispatch = ZString.Empty;
			AssertEquals(new ZDateTime(2020, 7, 9, 0, 0, 0), dataProvider.DispatchTime);
		}

		public void TestOriginTypeCode()
		{
			AssertEquals("1", dataProvider.OriginTypeCode);
		}

		public void TestInvoiceNumber()
		{
			AssertEquals("IN001", dataProvider.InvoiceNumber);
		}

		public void TestInvoiceDate()
		{
			AssertEquals(new ZDate(2020, 7, 9), dataProvider.InvoiceDate);
			message.Body.EadContainer.EadEsad.InvoiceDateSpecified = false;
			AssertEquals(ZDate.Empty, dataProvider.InvoiceDate);
		}

		public void TestImportSadNumbers()
		{
			CombineAssertions(() =>
			{
				var importSadNumbers = dataProvider.ImportSadNumbers;
				AssertSame("Cached", importSadNumbers, dataProvider.ImportSadNumbers);
				AssertEquals("Have 2 importSadNumber", 2, importSadNumbers.Count);
			});
		}

		public void TestImportSadNumbers_Null()
		{
			message.Body.EadContainer.EadEsad.ImportSad = null;
			AssertEquals("No exception", 0, dataProvider.ImportSadNumbers.Count);
		}

		public void TestGuarantorTypeCode()
		{
			AssertEquals("3", dataProvider.GuarantorTypeCode);
		}

		public void TestConsignee()
		{
			AssertSame("Cached", dataProvider.Consignee, dataProvider.Consignee);
		}

		public void TestConsignee_Null()
		{
			message.Body.EadContainer.ConsigneeTrader = null;
			AssertEquals("No exception", null, dataProvider.Consignee);
		}

		public void TestGuarantor()
		{
			AssertSame("Cached", dataProvider.Guarantor, dataProvider.Guarantor);
		}

		public void TestGuarantor_Null()
		{
			message.Body.EadContainer.MovementGuarantee.GuarantorTrader = null;
			AssertEquals("No exception", null, dataProvider.Guarantor);
		}

		public void TestGuarantor_GuarantorTypeCodeNot3()
		{
			message.Body.EadContainer.MovementGuarantee.GuarantorTypeCode = ED801EBodyEadContainerMovementGuaranteeGuarantorTypeCode.Item2;
			AssertEquals("Only return Guarantor when GuarantorTypeCode is 3", null, dataProvider.Guarantor);
		}

		public void TestConsignor()
		{
			AssertSame("Cached", dataProvider.Consignor, dataProvider.Consignor);
		}

		public void TestConsignor_Null()
		{
			message.Body.EadContainer.ConsignorTrader = null;
			AssertEquals("No exception", null, dataProvider.Consignor);
		}

		public void TestPlaceOfDispatch()
		{
			AssertSame("Cached", dataProvider.PlaceOfDispatch, dataProvider.PlaceOfDispatch);
		}

		public void TestPlaceOfDispatch_Null()
		{
			message.Body.EadContainer.PlaceOfDispatchTrader = null;
			AssertEquals("No exception", null, dataProvider.PlaceOfDispatch);
		}

		public void TestDeliveryPlace()
		{
			AssertSame("Cached", dataProvider.DeliveryPlace, dataProvider.DeliveryPlace);
		}

		public void TestDeliveryPlace_Null()
		{
			message.Body.EadContainer.DeliveryPlaceTrader = null;
			AssertEquals("No exception", null, dataProvider.DeliveryPlace);
		}

		public void TestTransportArranger()
		{
			AssertSame("Cached", dataProvider.TransportArranger, dataProvider.TransportArranger);
		}

		public void TestTransportArranger_Null()
		{
			message.Body.EadContainer.TransportArrangerTrader = null;
			AssertEquals("No exception", null, dataProvider.TransportArranger);
		}

		public void TestFirstTransporter()
		{
			AssertSame("Cached", dataProvider.FirstTransporter, dataProvider.FirstTransporter);
		}

		public void TestFirstTransporter_Null()
		{
			message.Body.EadContainer.FirstTransporterTrader = null;
			AssertEquals("No exception", null, dataProvider.FirstTransporter);
		}

		public void TestTransportModeCode()
		{
			AssertEquals("RAIL", dataProvider.TransportModeCode);
		}

		public void TestComplementaryInfo()
		{
			AssertEquals("Complementary Info", dataProvider.ComplementaryInfo);
		}

		public void TestDocumentCertificates()
		{
			CombineAssertions(() =>
			{
				var documentCertificates = dataProvider.DocumentCertificates;
				AssertSame("Cached", documentCertificates, dataProvider.DocumentCertificates);
				AssertEquals("Have 2 documentCertificate", 2, documentCertificates.Count);
			});
		}

		public void TestDocumentCertificates_Null()
		{
			message.Body.EadContainer.DocumentCertificate = null;
			AssertEquals("No exception", 0, dataProvider.DocumentCertificates.Count);
		}

		public void TestMemberStateCode()
		{
			AssertEquals("12", dataProvider.MemberStateCode);
		}

		public void TestCertificateOfExemption()
		{
			AssertEquals("00001", dataProvider.CertificateOfExemption);
		}

		public void TestTransportDetails()
		{
			CombineAssertions(() =>
			{
				var transportDetails = dataProvider.TransportDetails;
				AssertSame("Cached", transportDetails, dataProvider.TransportDetails);
				AssertEquals("Have 2 transportDetails", 2, transportDetails.Count);
			});
		}

		public void TestLines()
		{
			CombineAssertions(() =>
			{
				var lines = dataProvider.Lines;
				AssertSame("Cached", lines, dataProvider.Lines);
				AssertEquals("Have 2 lines", 2, lines.Count);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			message = new ED801E
			{
				Header = new ED801EHeader
				{
					MessageGroup = ED801EHeaderMessageGroup.EME,
					MessageIdentifier = "0072260102",
					MessageRecipient = "DE001348",
					MessageSender = "DE000050"
				},
				Body = new ED801EBody
				{
					EadContainer = new ED801EBodyEadContainer
					{
						ExciseMovement = new ED801EBodyEadContainerExciseMovement
						{
							AdministrativeReferenceCode = "MRN98761234",
							DateAndTimeOfValidationOfEadEsad = new DateTime(2020, 6, 11, 16, 59, 59),
						},
						HeaderEadEsad = new ED801EBodyEadContainerHeaderEadEsad
						{
							SequenceNumber = "1",
							JourneyTime = "H12",
							DestinationTypeCode = ED801EBodyEadContainerHeaderEadEsadDestinationTypeCode.Item1,
							TransportArrangement = ED801EBodyEadContainerHeaderEadEsadTransportArrangement.Item3
						},
						EadEsad = new ED801EBodyEadContainerEadEsad
						{
							LocalReferenceNumber = "19DE587500026773M4",
							DateOfDispatch = new DateTime(2020, 7, 9, 0, 0, 0),
							TimeOfDispatch = "07:09:23",
							OriginTypeCode = ED801EBodyEadContainerEadEsadOriginTypeCode.Item1,
							InvoiceNumber = "IN001",
							InvoiceDateSpecified = true,
							InvoiceDate = new DateTime(2020, 7, 9, 0, 0, 0),
							ImportSad = new ED801EBodyEadContainerEadEsadImportSad[2]
							{
								new ED801EBodyEadContainerEadEsadImportSad
								{
									ImportSadNumber = "ISN001"
								},
								new ED801EBodyEadContainerEadEsadImportSad
								{
									ImportSadNumber = "ISN002"
								},
							}
						},
						DispatchImportOffice = new ED801EBodyEadContainerDispatchImportOffice
						{
							ReferenceNumber = "DIO001"
						},
						DeliveryPlaceCustomsOffice = new ED801EBodyEadContainerDeliveryPlaceCustomsOffice
						{
							ReferenceNumber = "DPCO001"
						},
						CompetentAuthorityDispatchOffice = new ED801EBodyEadContainerCompetentAuthorityDispatchOffice
						{
							ReferenceNumber = "CADO001"
						},
						MovementGuarantee = new ED801EBodyEadContainerMovementGuarantee
						{
							GuarantorTypeCode = ED801EBodyEadContainerMovementGuaranteeGuarantorTypeCode.Item3,
							GuarantorTrader = new ED801EBodyEadContainerMovementGuaranteeGuarantorTrader[2]
							{
								new ED801EBodyEadContainerMovementGuaranteeGuarantorTrader
								{
								},
								new ED801EBodyEadContainerMovementGuaranteeGuarantorTrader
								{
								},
							},
						},
						ConsigneeTrader = new ED801EBodyEadContainerConsigneeTrader
						{
						},
						ConsignorTrader = new ED801EBodyEadContainerConsignorTrader
						{
						},
						PlaceOfDispatchTrader = new ED801EBodyEadContainerPlaceOfDispatchTrader
						{
						},
						DeliveryPlaceTrader = new ED801EBodyEadContainerDeliveryPlaceTrader
						{
						},
						TransportArrangerTrader = new ED801EBodyEadContainerTransportArrangerTrader
						{
						},
						FirstTransporterTrader = new ED801EBodyEadContainerFirstTransporterTrader
						{
						},
						TransportMode = new ED801EBodyEadContainerTransportMode
						{
							TransportModeCode = "RAIL",
							ComplementaryInformation = "Complementary Info",
						},
						DocumentCertificate = new ED801EBodyEadContainerDocumentCertificate[2]
						{
							new ED801EBodyEadContainerDocumentCertificate
							{
							},
							new ED801EBodyEadContainerDocumentCertificate
							{
							},
						},
						ComplementConsigneeTrader = new ED801EBodyEadContainerComplementConsigneeTrader
						{
							MemberStateCode = "12",
							SerialNumberOfCertificateOfExemption = "00001",
						},
						TransportDetails = new ED801EBodyEadContainerTransportDetails[2]
						{
							new ED801EBodyEadContainerTransportDetails
							{
							},
							new ED801EBodyEadContainerTransportDetails
							{
							},
						},
						BodyEadEsad = new ED801EBodyEadContainerBodyEadEsad[2]
						{
							new ED801EBodyEadContainerBodyEadEsad
							{
							},
							new ED801EBodyEadContainerBodyEadEsad
							{
							},
						},
					}
				}
			};
			dataProvider = new ED801Provider(message);
		}
		IED801 dataProvider;
		ED801E message;

		protected override ED801Provider GetProvider() => (ED801Provider)dataProvider;
	}
}

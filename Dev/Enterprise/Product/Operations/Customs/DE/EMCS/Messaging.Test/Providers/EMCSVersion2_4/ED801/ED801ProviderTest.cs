using System;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4.Testing
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
				var exciseMovementEad = dataProvider.ExciseMovement;
				AssertSame("Cached", exciseMovementEad, dataProvider.ExciseMovement);
				AssertEquals("AdministrativeReferenceCode", "MRN98761234", exciseMovementEad.AdministrativeReferenceCode);
				AssertEquals("SequenceNumber", "1", exciseMovementEad.SequenceNumber);
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
			message.Body.EadContainer.Ead.TimeOfDispatch = ZString.Empty;
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
			message.Body.EadContainer.Ead.InvoiceDateSpecified = false;
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
			message.Body.EadContainer.Ead.ImportSad = null;
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
			message.Body.EadContainer.MovementGuarantee.GuarantorTypeCode = ED801DBodyEadContainerMovementGuaranteeGuarantorTypeCode.Item2;
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

			message = new ED801D
			{
				Header = new ED801DHeader
				{
					MessageGroup = ED801DHeaderMessageGroup.EME,
					MessageIdentifier = "0072260102",
					MessageRecipient = "DE001348",
					MessageSender = "DE000050"
				},
				Body = new ED801DBody
				{
					EadContainer = new ED801DBodyEadContainer
					{
						ExciseMovementEad = new ED801DBodyEadContainerExciseMovementEad
						{
							AdministrativeReferenceCode = "MRN98761234",
							DateAndTimeOfValidationOfEad = new DateTime(2020, 6, 11, 16, 59, 59),
						},
						HeaderEad = new ED801DBodyEadContainerHeaderEad
						{
							SequenceNumber = "1",
							JourneyTime = "H12",
							DestinationTypeCode = ED801DBodyEadContainerHeaderEadDestinationTypeCode.Item1,
							TransportArrangement = ED801DBodyEadContainerHeaderEadTransportArrangement.Item3
						},
						Ead = new ED801DBodyEadContainerEad
						{
							LocalReferenceNumber = "19DE587500026773M4",
							DateOfDispatch = new DateTime(2020, 7, 9, 0, 0, 0),
							TimeOfDispatch = "07:09:23",
							OriginTypeCode = ED801DBodyEadContainerEadOriginTypeCode.Item1,
							InvoiceNumber = "IN001",
							InvoiceDateSpecified = true,
							InvoiceDate = new DateTime(2020, 7, 9, 0, 0, 0),
							ImportSad = new ED801DBodyEadContainerEadImportSad[2]
							{
								new ED801DBodyEadContainerEadImportSad
								{
									ImportSadNumber = "ISN001"
								},
								new ED801DBodyEadContainerEadImportSad
								{
									ImportSadNumber = "ISN002"
								},
							}
						},
						DispatchImportOffice = new ED801DBodyEadContainerDispatchImportOffice
						{
							ReferenceNumber = "DIO001"
						},
						DeliveryPlaceCustomsOffice = new ED801DBodyEadContainerDeliveryPlaceCustomsOffice
						{
							ReferenceNumber = "DPCO001"
						},
						CompetentAuthorityDispatchOffice = new ED801DBodyEadContainerCompetentAuthorityDispatchOffice
						{
							ReferenceNumber = "CADO001"
						},
						MovementGuarantee = new ED801DBodyEadContainerMovementGuarantee
						{
							GuarantorTypeCode = ED801DBodyEadContainerMovementGuaranteeGuarantorTypeCode.Item3,
							GuarantorTrader = new ED801DBodyEadContainerMovementGuaranteeGuarantorTrader[2]
							{
								new ED801DBodyEadContainerMovementGuaranteeGuarantorTrader
								{
								},
								new ED801DBodyEadContainerMovementGuaranteeGuarantorTrader
								{
								},
							},
						},
						ConsigneeTrader = new ED801DBodyEadContainerConsigneeTrader
						{
						},
						ConsignorTrader = new ED801DBodyEadContainerConsignorTrader
						{
						},
						PlaceOfDispatchTrader = new ED801DBodyEadContainerPlaceOfDispatchTrader
						{
						},
						DeliveryPlaceTrader = new ED801DBodyEadContainerDeliveryPlaceTrader
						{
						},
						TransportArrangerTrader = new ED801DBodyEadContainerTransportArrangerTrader
						{
						},
						FirstTransporterTrader = new ED801DBodyEadContainerFirstTransporterTrader
						{
						},
						TransportMode = new ED801DBodyEadContainerTransportMode
						{
							TransportModeCode = "RAIL",
							ComplementaryInformation = "Complementary Info",
						},
						DocumentCertificate = new ED801DBodyEadContainerDocumentCertificate[2]
						{
							new ED801DBodyEadContainerDocumentCertificate
							{
							},
							new ED801DBodyEadContainerDocumentCertificate
							{
							},
						},
						ComplementConsigneeTrader = new ED801DBodyEadContainerComplementConsigneeTrader
						{
							MemberStateCode = "12",
							SerialNumberOfCertificateOfExemption = "00001",
						},
						TransportDetails = new ED801DBodyEadContainerTransportDetails[2]
						{
							new ED801DBodyEadContainerTransportDetails
							{
							},
							new ED801DBodyEadContainerTransportDetails
							{
							},
						},
						BodyEad = new ED801DBodyEadContainerBodyEad[2]
						{
							new ED801DBodyEadContainerBodyEad
							{
							},
							new ED801DBodyEadContainerBodyEad
							{
							},
						},
					}
				}
			};
			dataProvider = new ED801Provider(message);
		}
		IED801 dataProvider;
		ED801D message;

		protected override ED801Provider GetProvider() => (ED801Provider)dataProvider;
	}
}

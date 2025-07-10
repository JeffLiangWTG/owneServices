using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
{
	sealed class IE801ProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE801Provider(null));
		}

		public void TestLocalReferenceNumber()
		{
			AssertEquals("19DE587500026773M4", dataProvider.LocalReferenceNumber);
		}

		public void TestDateAndTimeOfValidationOfEad()
		{
			AssertEquals(new DateTime(2020, 6, 11, 16, 59, 59), dataProvider.DateAndTimeOfValidationOfEad);
		}

		public void TestExciseMovementEad()
		{
			CombineAssertions(() =>
			{
				var exciseMovementEad = dataProvider.ExciseMovementEad;
				AssertSame("Cached", exciseMovementEad, dataProvider.ExciseMovementEad);
				AssertEquals("AdministrativeReferenceCode", "MRN98761234", exciseMovementEad.AdministrativeReferenceCode);
				AssertEquals("SequenceNumber", "1", exciseMovementEad.SequenceNumber);
			});
		}

		public void TestDispatchImportOffice()
		{
			AssertEquals("DIO001", dataProvider.DispatchImportOffice);
			message.Body.EadesadContainer.DispatchImportOffice = null;
			AssertEquals(ZString.Empty, dataProvider.DispatchImportOffice);
		}

		public void TestDeliveryPlaceCustomsOffice()
		{
			AssertEquals("DPCO001", dataProvider.DeliveryPlaceCustomsOffice);
			message.Body.EadesadContainer.DeliveryPlaceCustomsOffice = null;
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

			message.Body.EadesadContainer.EadEsad.TimeOfDispatch = "00:00:00";
			AssertEquals(new ZDateTime(2020, 7, 9, 0, 0, 0), dataProvider.DispatchTime);
		}

		public void TestEmptyDispatchTime()
		{
			AssertEquals(new ZDateTime(2020, 7, 9, 7, 9, 0), dataProvider.DispatchTime);

			message.Body.EadesadContainer.EadEsad.TimeOfDispatch = null;
			AssertEquals(new ZDateTime(2020, 7, 9, 0, 0, 0), dataProvider.DispatchTime);

			message.Body.EadesadContainer.EadEsad.TimeOfDispatch = "00:00:00";
			AssertEquals(new ZDateTime(2020, 7, 9, 0, 0, 0), dataProvider.DispatchTime);
		}

		public void TestValidDispatchTime()
		{
			AssertEquals(new ZDateTime(2020, 7, 9, 7, 9, 0), dataProvider.DispatchTime);

			message.Body.EadesadContainer.EadEsad.TimeOfDispatch = "19:57:00";
			AssertEquals(new ZDateTime(2020, 7, 9, 19, 57, 0), dataProvider.DispatchTime);

			message.Body.EadesadContainer.EadEsad.DateOfDispatch = new DateTime(2022, 12, 28, 00, 01, 0);
			message.Body.EadesadContainer.EadEsad.TimeOfDispatch = "00:01:00";
			AssertEquals(new ZDateTime(2022, 12, 28, 00, 01, 0), dataProvider.DispatchTime);

			message.Body.EadesadContainer.EadEsad.DateOfDispatch = new DateTime(2022, 01, 08, 23, 59, 0);
			message.Body.EadesadContainer.EadEsad.TimeOfDispatch = "23:59:00";
			AssertEquals(new ZDateTime(2022, 01, 08, 23, 59, 0), dataProvider.DispatchTime);
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
			message.Body.EadesadContainer.EadEsad.InvoiceDateValueSpecified = false;
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
			message.Body.EadesadContainer.EadEsad.ImportSad = null;
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
			message.Body.EadesadContainer.ConsigneeTrader = null;
			AssertEquals("No exception", null, dataProvider.Consignee);
		}

		public void TestGuarantor()
		{
			AssertSame("Cached", dataProvider.Guarantor, dataProvider.Guarantor);
		}

		public void TestGuarantor_Null()
		{
			message.Body.EadesadContainer.MovementGuarantee.GuarantorTrader = null;
			AssertEquals("No exception", null, dataProvider.Guarantor);
		}

		public void TestGuarantor_GuarantorTypeCodeNot3()
		{
			message.Body.EadesadContainer.MovementGuarantee.GuarantorTypeCode = CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tcl.GuarantorTypeCode.Item2;
			AssertEquals("Only return Guarantor when GuarantorTypeCode is 3", null, dataProvider.Guarantor);
		}

		public void TestConsignor()
		{
			AssertSame("Cached", dataProvider.Consignor, dataProvider.Consignor);
		}

		public void TestConsignor_Null()
		{
			message.Body.EadesadContainer.ConsignorTrader = null;
			AssertEquals("No exception", null, dataProvider.Consignor);
		}

		public void TestPlaceOfDispatch()
		{
			AssertSame("Cached", dataProvider.PlaceOfDispatch, dataProvider.PlaceOfDispatch);
		}

		public void TestPlaceOfDispatch_Null()
		{
			message.Body.EadesadContainer.PlaceOfDispatchTrader = null;
			AssertEquals("No exception", null, dataProvider.PlaceOfDispatch);
		}

		public void TestDeliveryPlace()
		{
			AssertSame("Cached", dataProvider.DeliveryPlace, dataProvider.DeliveryPlace);
		}

		public void TestDeliveryPlace_Null()
		{
			message.Body.EadesadContainer.DeliveryPlaceTrader = null;
			AssertEquals("No exception", null, dataProvider.DeliveryPlace);
		}

		public void TestTransportArranger()
		{
			AssertSame("Cached", dataProvider.TransportArranger, dataProvider.TransportArranger);
		}

		public void TestTransportArranger_Null()
		{
			message.Body.EadesadContainer.TransportArrangerTrader = null;
			AssertEquals("No exception", null, dataProvider.TransportArranger);
		}

		public void TestFirstTransporter()
		{
			AssertSame("Cached", dataProvider.FirstTransporter, dataProvider.FirstTransporter);
		}

		public void TestFirstTransporter_Null()
		{
			message.Body.EadesadContainer.FirstTransporterTrader = null;
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
			message.Body.EadesadContainer.DocumentCertificate = null;
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

			message = new Ie801Type
			{
				Header = new HeaderType
				{
				},
				Body = new BodyType
				{
					EadesadContainer = new EadesadContainerType
					{
						ExciseMovement = new ExciseMovementType
						{
							AdministrativeReferenceCode = "MRN98761234",
							DateAndTimeOfValidationOfEadEsad = new DateTime(2020, 6, 11, 16, 59, 59),
						},
						HeaderEadEsad = new HeaderEadEsadType
						{
							SequenceNumber = "1",
							JourneyTime = "H12",
							DestinationTypeCode = CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tcl.DestinationTypeCode.Item1,
							TransportArrangement = CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tcl.TransportArrangement.Item3
						},
						EadEsad = new EadEsadType
						{
							LocalReferenceNumber = "19DE587500026773M4",
							DateOfDispatch = new DateTime(2020, 7, 9, 0, 0, 0),
							TimeOfDispatch = "07:09:23",
							OriginTypeCode = CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tcl.OriginTypeCode.Item1,
							InvoiceNumber = "IN001",
							InvoiceDateValueSpecified = true,
							InvoiceDate = new DateTime(2020, 7, 9, 0, 0, 0),
							ImportSad = new Collection<ImportSadType>
							{
								new ImportSadType
								{
									ImportSadNumber = "ISN001"
								},
								new ImportSadType
								{
									ImportSadNumber = "ISN002"
								},
							}
						},
						DispatchImportOffice = new DispatchImportOfficeType
						{
							ReferenceNumber = "DIO001"
						},
						DeliveryPlaceCustomsOffice = new DeliveryPlaceCustomsOfficeType
						{
							ReferenceNumber = "DPCO001"
						},
						CompetentAuthorityDispatchOffice = new CompetentAuthorityDispatchOfficeType
						{
							ReferenceNumber = "CADO001"
						},
						MovementGuarantee = new MovementGuaranteeType
						{
							GuarantorTypeCode = CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tcl.GuarantorTypeCode.Item3,
							GuarantorTrader = new Collection<GuarantorTraderType>
							{
								new GuarantorTraderType
								{
								},
								new GuarantorTraderType
								{
								},
							},
						},
						ConsigneeTrader = new ConsigneeTraderType
						{
						},
						ConsignorTrader = new ConsignorTraderType
						{
						},
						PlaceOfDispatchTrader = new PlaceOfDispatchTraderType
						{
						},
						DeliveryPlaceTrader = new DeliveryPlaceTraderType
						{
						},
						TransportArrangerTrader = new TransportArrangerTraderType
						{
						},
						FirstTransporterTrader = new FirstTransporterTraderType
						{
						},
						TransportMode = new TransportModeType
						{
							TransportModeCode = "RAIL",
							ComplementaryInformation = new LsdComplementaryInformationType
							{
								Value = "Complementary Info",
								Language = "en",
							},
						},
						DocumentCertificate = new Collection<DocumentCertificateType>
						{
							new DocumentCertificateType
							{
							},
							new DocumentCertificateType
							{
							},
						},
						ComplementConsigneeTrader = new ComplementConsigneeTraderType
						{
							MemberStateCode = "12",
							SerialNumberOfCertificateOfExemption = "00001",
						},
						TransportDetails = new Collection<TransportDetailsType>
						{
							new TransportDetailsType
							{
							},
							new TransportDetailsType
							{
							},
						},
						BodyEadEsad = new Collection<BodyEadEsadType>
						{
							new BodyEadEsadType
							{
							},
							new BodyEadEsadType
							{
							},
						},
					}
				}
			};
			dataProvider = new IE801Provider(message);
		}
		IIE801 dataProvider;
		Ie801Type message;
	}
}

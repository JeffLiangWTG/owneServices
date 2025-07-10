using System;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4.Testing
{
	[TestedType(typeof(ED813Provider))]
	class ED813ProviderTest : InboundDataProviderTestCase<IED813, ED813Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED813Provider(null));
		}

		public void TestMessageGroup()
		{
			AssertEquals("EME", dataProvider.MessageGroup);
		}

		public void TestMessageIdentifier()
		{
			AssertEquals(MessageIdentifier, dataProvider.MessageIdentifier);
		}

		public void TestUpdateEadEsad()
		{
			AssertSame("Cached", dataProvider.UpdateEadEsad, dataProvider.UpdateEadEsad);
		}

		public void TestEventProviderConstructor()
		{
			message.Body.ChangeOfDestination.UpdateEad = null;
			AssertExceptionThrown<ArgumentException>(() => _ = dataProvider.UpdateEadEsad);
		}

		public void TestEventProviderValues()
		{
			CombineAssertions(() =>
			{
				var updateEad = dataProvider.UpdateEadEsad;
				AssertEquals("Ead", AdministrativeReferenceCode, updateEad.AdministrativeReferenceCode);
				AssertEquals("Sequence", SequenceNumber, updateEad.SequenceNumber);
			});
		}

		public void TestNewDestinationCode()
		{
			message.Body.ChangeOfDestination.DestinationChanged = new ED813EBodyChangeOfDestinationDestinationChanged()
			{
				DestinationTypeCode = ED813EBodyChangeOfDestinationDestinationChangedDestinationTypeCode.Item1
			};
			AssertEquals("1", dataProvider.NewDestinationCode);
		}

		public void TestJourneyTime()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, dataProvider.JourneyTime);

				message.Body.ChangeOfDestination.UpdateEad.JourneyTime = "H12";
				AssertEquals("Specified", "12H", dataProvider.JourneyTime);
			});
		}

		public void TestTransportModeCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, dataProvider.TransportModeCode);

				message.Body.ChangeOfDestination.UpdateEad.TransportModeCode = "RAIL";
				AssertEquals("Specified", "RAIL", dataProvider.TransportModeCode);
			});
		}

		public void TestTransportArrangement()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, dataProvider.TransportArrangement);

				message.Body.ChangeOfDestination.UpdateEad.ChangedTransportArrangement = ED813EBodyChangeOfDestinationUpdateEadChangedTransportArrangement.Item3;
				message.Body.ChangeOfDestination.UpdateEad.ChangedTransportArrangementSpecified = true;
				AssertEquals("Specified", "3", dataProvider.TransportArrangement);
			});
		}

		public void TestComplementaryInfo()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, dataProvider.ComplementaryInfo);

				message.Body.ChangeOfDestination.UpdateEad.ComplementaryInformation = "Complementary Info";
				AssertEquals("Specified", "Complementary Info", dataProvider.ComplementaryInfo);
			});
		}

		public void TestInvoiceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, dataProvider.InvoiceNumber);

				message.Body.ChangeOfDestination.UpdateEad.InvoiceNumber = "IN001";
				AssertEquals("Specified", "IN001", dataProvider.InvoiceNumber);
			});
		}

		public void TestInvoiceDate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZDate.Empty, dataProvider.InvoiceDate);

				message.Body.ChangeOfDestination.UpdateEad.InvoiceDate = new DateTime(2020, 7, 9);
				message.Body.ChangeOfDestination.UpdateEad.InvoiceDateSpecified = true;
				AssertEquals("Specified", new ZDate(2020, 7, 9), dataProvider.InvoiceDate);
			});
		}

		public void TestDestinationTypeCode()
		{
			AssertEquals("1", dataProvider.DestinationTypeCode);
		}

		public void TestGuarantorTypeCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, dataProvider.GuarantorTypeCode);

				message.Body.ChangeOfDestination.DestinationChanged.MovementGuarantee = new ED813EBodyChangeOfDestinationDestinationChangedMovementGuarantee
				{
					GuarantorTypeCode = ED813EBodyChangeOfDestinationDestinationChangedMovementGuaranteeGuarantorTypeCode.Item3
				};
				AssertEquals("Specified", "3", dataProvider.GuarantorTypeCode);
			});
		}

		public void TestGuarantor()
		{
			AssertEquals("Not specified, No exception", null, dataProvider.Guarantor);
		}

		public void TestGuarantor_GuarantorTypeCode3()
		{
			message.Body.ChangeOfDestination.DestinationChanged.MovementGuarantee = new ED813EBodyChangeOfDestinationDestinationChangedMovementGuarantee
			{
				GuarantorTypeCode = ED813EBodyChangeOfDestinationDestinationChangedMovementGuaranteeGuarantorTypeCode.Item3,
				GuarantorTrader = new[]
				{
					new ED813EBodyChangeOfDestinationDestinationChangedMovementGuaranteeGuarantorTrader { TraderExciseNumber = "DETEN001" },
					new ED813EBodyChangeOfDestinationDestinationChangedMovementGuaranteeGuarantorTrader { TraderExciseNumber = "DETEN002" }
				}
			};
			var guarantor = dataProvider.Guarantor;

			CombineAssertions(() =>
			{
				AssertEquals("TraderExciseNumber", "DETEN001", guarantor.TraderExciseNumber);
				AssertSame("Cached", guarantor, dataProvider.Guarantor);
			});
		}

		public void TestGuarantor_GuarantorTypeCodeNot3()
		{
			message.Body.ChangeOfDestination.DestinationChanged.MovementGuarantee = new ED813EBodyChangeOfDestinationDestinationChangedMovementGuarantee
			{
				GuarantorTypeCode = ED813EBodyChangeOfDestinationDestinationChangedMovementGuaranteeGuarantorTypeCode.Item2,
				GuarantorTrader = new[]
				{
					new ED813EBodyChangeOfDestinationDestinationChangedMovementGuaranteeGuarantorTrader { TraderExciseNumber = "DETEN001" }
				}
			};
			AssertEquals("Only return Guarantor when GuarantorTypeCode is 3", null, dataProvider.Guarantor);
		}

		public void TestDeliveryPlace()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", null, dataProvider.DeliveryPlace);

				message.Body.ChangeOfDestination.DestinationChanged.DeliveryPlaceTrader = new ED813EBodyChangeOfDestinationDestinationChangedDeliveryPlaceTrader { Traderid = "DETID001" };
				var deliveryPlace = dataProvider.DeliveryPlace;
				AssertSame("Cached", deliveryPlace, dataProvider.DeliveryPlace);
			});
		}

		public void TestNewTransportArranger()
		{
			AssertEquals("Not specified, No exception", null, dataProvider.NewTransportArranger);
		}

		public void TestNewTransportArranger_TransportArrangement1()
		{
			message.Body.ChangeOfDestination.NewTransportArrangerTrader = new ED813EBodyChangeOfDestinationNewTransportArrangerTrader { VatNumber = "DEVAT001" };
			message.Body.ChangeOfDestination.UpdateEad.ChangedTransportArrangement = ED813EBodyChangeOfDestinationUpdateEadChangedTransportArrangement.Item1;
			message.Body.ChangeOfDestination.UpdateEad.ChangedTransportArrangementSpecified = true;
			AssertEquals("TransportArrangement 1 and 2 -> NewTransportArranger null", null, dataProvider.NewTransportArranger);
		}

		public void TestNewTransportArranger_TransportArrangement2()
		{
			message.Body.ChangeOfDestination.NewTransportArrangerTrader = new ED813EBodyChangeOfDestinationNewTransportArrangerTrader { VatNumber = "DEVAT001" };
			message.Body.ChangeOfDestination.UpdateEad.ChangedTransportArrangement = ED813EBodyChangeOfDestinationUpdateEadChangedTransportArrangement.Item2;
			message.Body.ChangeOfDestination.UpdateEad.ChangedTransportArrangementSpecified = true;
			AssertEquals("TransportArrangement 1 and 2 -> NewTransportArranger null", null, dataProvider.NewTransportArranger);
		}

		public void TestNewTransportArranger_TransportArrangement3()
		{
			message.Body.ChangeOfDestination.NewTransportArrangerTrader = new ED813EBodyChangeOfDestinationNewTransportArrangerTrader { VatNumber = "DEVAT001" };
			message.Body.ChangeOfDestination.UpdateEad.ChangedTransportArrangement = ED813EBodyChangeOfDestinationUpdateEadChangedTransportArrangement.Item3;
			message.Body.ChangeOfDestination.UpdateEad.ChangedTransportArrangementSpecified = true;
			var transportArranger = dataProvider.NewTransportArranger;

			CombineAssertions(() =>
			{
				AssertNotNull("Not null", transportArranger);
				AssertSame("Cached", transportArranger, dataProvider.NewTransportArranger);
			});
		}

		public void TestNewTransportArranger_TransportArrangement4()
		{
			message.Body.ChangeOfDestination.NewTransportArrangerTrader = new ED813EBodyChangeOfDestinationNewTransportArrangerTrader { VatNumber = "DEVAT001" };
			message.Body.ChangeOfDestination.UpdateEad.ChangedTransportArrangement = ED813EBodyChangeOfDestinationUpdateEadChangedTransportArrangement.Item4;
			message.Body.ChangeOfDestination.UpdateEad.ChangedTransportArrangementSpecified = true;
			AssertNotNull(dataProvider.NewTransportArranger);
		}

		public void TestNewTransporter()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", null, dataProvider.NewTransporter);

				message.Body.ChangeOfDestination.NewTransporterTrader = new ED813EBodyChangeOfDestinationNewTransporterTrader { VatNumber = "DEVAT002" };
				var newTransporter = dataProvider.NewTransporter;
				AssertNotNull("Not null", newTransporter);
				AssertSame("Cached", newTransporter, dataProvider.NewTransporter);
			});
		}

		public void TestTransportDetails_Null()
		{
			AssertEquals("No exception", 0, dataProvider.TransportDetails.Count);
		}

		public void TestTransportDetails()
		{
			message.Body.ChangeOfDestination.TransportDetails = new[]
			{
				new ED813EBodyChangeOfDestinationTransportDetails { IdentityOfTransportUnits = "ID0001" },
				new ED813EBodyChangeOfDestinationTransportDetails { IdentityOfTransportUnits = "ID0002" }
			};
			var transportDetails = dataProvider.TransportDetails;

			CombineAssertions(() =>
			{
				AssertSame("Cached", transportDetails, dataProvider.TransportDetails);
				AssertEquals("Count", 2, dataProvider.TransportDetails.Count);
			});
		}

		public void TestConsignee()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", null, dataProvider.Consignee);

				message.Body.ChangeOfDestination.DestinationChanged.NewConsigneeTrader = new ED813EBodyChangeOfDestinationDestinationChangedNewConsigneeTrader { Traderid = "DETID002" };
				var consignee = dataProvider.Consignee;
				AssertSame("Cached", consignee, dataProvider.Consignee);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			message = new ED813E()
			{
				Header = new ED813EHeader()
				{
					MessageGroup = ED813EHeaderMessageGroup.EME,
					MessageIdentifier = MessageIdentifier
				},
				Body = new ED813EBody()
				{
					ChangeOfDestination = new ED813EBodyChangeOfDestination()
					{
						UpdateEad = new ED813EBodyChangeOfDestinationUpdateEad()
						{
							AdministrativeReferenceCode = AdministrativeReferenceCode,
							SequenceNumber = SequenceNumber
						},
						DestinationChanged = new ED813EBodyChangeOfDestinationDestinationChanged
						{
							DestinationTypeCode = ED813EBodyChangeOfDestinationDestinationChangedDestinationTypeCode.Item1
						}
					}
				}
			};
			dataProvider = new ED813Provider(message);
		}
		ED813E message;
		IED813 dataProvider;

		protected override ED813Provider GetProvider() => (ED813Provider)dataProvider;

		const string AdministrativeReferenceCode = "20DE41000000001870745";
		const string MessageIdentifier = "0072260102";
		const string SequenceNumber = "1";
	}
}

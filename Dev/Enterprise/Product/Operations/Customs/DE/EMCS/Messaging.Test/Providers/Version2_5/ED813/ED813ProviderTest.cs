using System;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
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
			message.Body.ChangeOfDestination.UpdateEadEsad = null;
			AssertExceptionThrown<ArgumentException>(() => _ = dataProvider.UpdateEadEsad);
		}

		public void TestEventProviderValues()
		{
			CombineAssertions(() =>
			{
				var updateEadEsad = dataProvider.UpdateEadEsad;
				AssertEquals("Ead Esad", AdministrativeReferenceCode, updateEadEsad.AdministrativeReferenceCode);
				AssertEquals("Sequence", SequenceNumber, updateEadEsad.SequenceNumber);
			});
		}

		public void TestNewDestinationCode()
		{
			message.Body.ChangeOfDestination.DestinationChanged = new ED813FBodyChangeOfDestinationDestinationChanged()
			{
				DestinationTypeCode = ED813FBodyChangeOfDestinationDestinationChangedDestinationTypeCode.Item1
			};
			AssertEquals("1", dataProvider.NewDestinationCode);
		}

		public void TestJourneyTime()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, dataProvider.JourneyTime);

				message.Body.ChangeOfDestination.UpdateEadEsad.JourneyTime = "H12";
				AssertEquals("Specified", "12H", dataProvider.JourneyTime);
			});
		}

		public void TestTransportModeCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, dataProvider.TransportModeCode);

				message.Body.ChangeOfDestination.UpdateEadEsad.TransportModeCode = "RAIL";
				AssertEquals("Specified", "RAIL", dataProvider.TransportModeCode);
			});
		}

		public void TestTransportArrangement()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, dataProvider.TransportArrangement);

				message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangement = ED813FBodyChangeOfDestinationUpdateEadEsadChangedTransportArrangement.Item3;
				message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangementSpecified = true;
				AssertEquals("Specified", "3", dataProvider.TransportArrangement);
			});
		}

		public void TestComplementaryInfo()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, dataProvider.ComplementaryInfo);

				message.Body.ChangeOfDestination.UpdateEadEsad.ComplementaryInformation = "Complementary Info";
				AssertEquals("Specified", "Complementary Info", dataProvider.ComplementaryInfo);
			});
		}

		public void TestInvoiceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, dataProvider.InvoiceNumber);

				message.Body.ChangeOfDestination.UpdateEadEsad.InvoiceNumber = "IN001";
				AssertEquals("Specified", "IN001", dataProvider.InvoiceNumber);
			});
		}

		public void TestInvoiceDate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZDate.Empty, dataProvider.InvoiceDate);

				message.Body.ChangeOfDestination.UpdateEadEsad.InvoiceDate = new DateTime(2020, 7, 9);
				message.Body.ChangeOfDestination.UpdateEadEsad.InvoiceDateSpecified = true;
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

				message.Body.ChangeOfDestination.DestinationChanged.MovementGuarantee = new ED813FBodyChangeOfDestinationDestinationChangedMovementGuarantee
				{
					GuarantorTypeCode = ED813FBodyChangeOfDestinationDestinationChangedMovementGuaranteeGuarantorTypeCode.Item3
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
			message.Body.ChangeOfDestination.DestinationChanged.MovementGuarantee = new ED813FBodyChangeOfDestinationDestinationChangedMovementGuarantee
			{
				GuarantorTypeCode = ED813FBodyChangeOfDestinationDestinationChangedMovementGuaranteeGuarantorTypeCode.Item3,
				GuarantorTrader = new[]
				{
					new ED813FBodyChangeOfDestinationDestinationChangedMovementGuaranteeGuarantorTrader { TraderExciseNumber = "DETEN001" },
					new ED813FBodyChangeOfDestinationDestinationChangedMovementGuaranteeGuarantorTrader { TraderExciseNumber = "DETEN002" }
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
			message.Body.ChangeOfDestination.DestinationChanged.MovementGuarantee = new ED813FBodyChangeOfDestinationDestinationChangedMovementGuarantee
			{
				GuarantorTypeCode = ED813FBodyChangeOfDestinationDestinationChangedMovementGuaranteeGuarantorTypeCode.Item2,
				GuarantorTrader = new[]
				{
					new ED813FBodyChangeOfDestinationDestinationChangedMovementGuaranteeGuarantorTrader { TraderExciseNumber = "DETEN001" }
				}
			};
			AssertEquals("Only return Guarantor when GuarantorTypeCode is 3", null, dataProvider.Guarantor);
		}

		public void TestDeliveryPlace()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", null, dataProvider.DeliveryPlace);

				message.Body.ChangeOfDestination.DestinationChanged.DeliveryPlaceTrader = new ED813FBodyChangeOfDestinationDestinationChangedDeliveryPlaceTrader { Traderid = "DETID001" };
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
			message.Body.ChangeOfDestination.NewTransportArrangerTrader = new ED813FBodyChangeOfDestinationNewTransportArrangerTrader { VatNumber = "DEVAT001" };
			message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangement = ED813FBodyChangeOfDestinationUpdateEadEsadChangedTransportArrangement.Item1;
			message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangementSpecified = true;
			AssertEquals("TransportArrangement 1 and 2 -> NewTransportArranger null", null, dataProvider.NewTransportArranger);
		}

		public void TestNewTransportArranger_TransportArrangement2()
		{
			message.Body.ChangeOfDestination.NewTransportArrangerTrader = new ED813FBodyChangeOfDestinationNewTransportArrangerTrader { VatNumber = "DEVAT001" };
			message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangement = ED813FBodyChangeOfDestinationUpdateEadEsadChangedTransportArrangement.Item2;
			message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangementSpecified = true;
			AssertEquals("TransportArrangement 1 and 2 -> NewTransportArranger null", null, dataProvider.NewTransportArranger);
		}

		public void TestNewTransportArranger_TransportArrangement3()
		{
			message.Body.ChangeOfDestination.NewTransportArrangerTrader = new ED813FBodyChangeOfDestinationNewTransportArrangerTrader { VatNumber = "DEVAT001" };
			message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangement = ED813FBodyChangeOfDestinationUpdateEadEsadChangedTransportArrangement.Item3;
			message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangementSpecified = true;
			var transportArranger = dataProvider.NewTransportArranger;

			CombineAssertions(() =>
			{
				AssertNotNull("Not null", transportArranger);
				AssertSame("Cached", transportArranger, dataProvider.NewTransportArranger);
			});
		}

		public void TestNewTransportArranger_TransportArrangement4()
		{
			message.Body.ChangeOfDestination.NewTransportArrangerTrader = new ED813FBodyChangeOfDestinationNewTransportArrangerTrader { VatNumber = "DEVAT001" };
			message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangement = ED813FBodyChangeOfDestinationUpdateEadEsadChangedTransportArrangement.Item4;
			message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangementSpecified = true;
			AssertNotNull(dataProvider.NewTransportArranger);
		}

		public void TestNewTransporter()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", null, dataProvider.NewTransporter);

				message.Body.ChangeOfDestination.NewTransporterTrader = new ED813FBodyChangeOfDestinationNewTransporterTrader { VatNumber = "DEVAT002" };
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
				new ED813FBodyChangeOfDestinationTransportDetails { IdentityOfTransportUnits = "ID0001" },
				new ED813FBodyChangeOfDestinationTransportDetails { IdentityOfTransportUnits = "ID0002" }
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

				message.Body.ChangeOfDestination.DestinationChanged.NewConsigneeTrader = new ED813FBodyChangeOfDestinationDestinationChangedNewConsigneeTrader { Traderid = "DETID002" };
				var consignee = dataProvider.Consignee;
				AssertSame("Cached", consignee, dataProvider.Consignee);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			message = new ED813F()
			{
				Header = new ED813FHeader()
				{
					MessageGroup = ED813FHeaderMessageGroup.EME,
					MessageIdentifier = MessageIdentifier
				},
				Body = new ED813FBody()
				{
					ChangeOfDestination = new ED813FBodyChangeOfDestination()
					{
						UpdateEadEsad = new ED813FBodyChangeOfDestinationUpdateEadEsad()
						{
							AdministrativeReferenceCode = AdministrativeReferenceCode,
							SequenceNumber = SequenceNumber
						},
						DestinationChanged = new ED813FBodyChangeOfDestinationDestinationChanged
						{
							DestinationTypeCode = ED813FBodyChangeOfDestinationDestinationChangedDestinationTypeCode.Item1
						}
					}
				}
			};
			dataProvider = new ED813Provider(message);
		}
		ED813F message;
		IED813 dataProvider;

		protected override ED813Provider GetProvider() => (ED813Provider)dataProvider;

		const string AdministrativeReferenceCode = "20DE41000000001870745";
		const string MessageIdentifier = "0072260102";
		const string SequenceNumber = "1";
	}
}

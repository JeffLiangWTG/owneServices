using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie813;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tcl;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tms;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE813ProviderTest : DataProviderTestCase<IE813Provider>
	{
		public void TestMrnNumber()
		{
			AssertEquals("20GB41000000001870745", Provider.MrnNumber);
		}

		public void TestMrnNumberSequenceNumber()
		{
			AssertEquals("4", Provider.MrnNumberSequenceNumber);
		}

		public void TestAdministrativeReferenceCode()
		{
			AssertEquals("20GB41000000001870745", Provider.AdministrativeReferenceCode);
		}

		public void TestEventProviderConstructor()
		{
			message.Body.ChangeOfDestination.UpdateEadEsad = null;
			AssertExceptionThrown<ArgumentException>(() => _ = Provider.UpdateEad);
		}

		public void TestUpdateEad()
		{
			CombineAssertions(() =>
			{
				var updateEad = Provider.UpdateEad;
				AssertEquals("Ead", AdministrativeReferenceCode, updateEad.AdministrativeReferenceCode);
				AssertEquals("Sequence", SequenceNumber, updateEad.SequenceNumber);
			});
		}

		public void TestNewDestinationCode()
		{
			message.Body.ChangeOfDestination.DestinationChanged = new DestinationChangedType()
			{
				DestinationTypeCode = ChangedDestinationTypeCode.Item1
			};
			AssertEquals("1", Provider.NewDestinationCode);
		}

		public void TestJourneyTime()
		{
			AssertEquals("Specified", "12H", Provider.JourneyTime);
		}

		public void TestTransportModeCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, Provider.TransportModeCode);

				message.Body.ChangeOfDestination.UpdateEadEsad.TransportModeCode = "RAIL";
				AssertEquals("Specified", "RAIL", Provider.TransportModeCode);
			});
		}

		public void TestTransportArrangement()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, Provider.TransportArrangement);

				message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangement = TransportArrangement.Item3;
				message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangementValueSpecified = true;
				AssertEquals("Specified", "3", Provider.TransportArrangement);
			});
		}

		public void TestComplementaryInfo()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, Provider.ComplementaryInfo);

				message.Body.ChangeOfDestination.UpdateEadEsad.ComplementaryInformation = new LsdComplementaryInformationType { Value = "Complementary Info" };
				AssertEquals("Specified", "Complementary Info", Provider.ComplementaryInfo);
			});
		}

		public void TestInvoiceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, Provider.InvoiceNumber);

				message.Body.ChangeOfDestination.UpdateEadEsad.InvoiceNumber = "IN001";
				AssertEquals("Specified", "IN001", Provider.InvoiceNumber);
			});
		}

		public void TestInvoiceDate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZDate.Empty, Provider.InvoiceDate);

				message.Body.ChangeOfDestination.UpdateEadEsad.InvoiceDate = new DateTime(2020, 7, 9);
				message.Body.ChangeOfDestination.UpdateEadEsad.InvoiceDateValueSpecified = true;
				AssertEquals("Specified", new ZDate(2020, 7, 9), Provider.InvoiceDate);
			});
		}

		public void TestDestinationTypeCode()
		{
			AssertEquals("1", Provider.DestinationTypeCode);
		}

		public void TestGuarantorTypeCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", ZString.Empty, Provider.GuarantorTypeCode);

				message.Body.ChangeOfDestination.DestinationChanged.MovementGuarantee = new MovementGuaranteeType
				{
					GuarantorTypeCode = GuarantorTypeCode.Item3
				};
				AssertEquals("Specified", "3", Provider.GuarantorTypeCode);
			});
		}

		public void TestGuarantor()
		{
			AssertEquals("Not specified, No exception", null, Provider.Guarantor);
		}

		public void TestGuarantor_GuarantorTypeCode3()
		{
			message.Body.ChangeOfDestination.DestinationChanged.MovementGuarantee = new MovementGuaranteeType
			{
				GuarantorTypeCode = GuarantorTypeCode.Item3,
				GuarantorTrader = new Collection<GuarantorTraderType>
				{
					new GuarantorTraderType { TraderExciseNumber = "DETEN001" },
					new GuarantorTraderType { TraderExciseNumber = "DETEN002" }
				}
			};
			var guarantor = Provider.Guarantor;

			CombineAssertions(() =>
			{
				AssertEquals("TraderExciseNumber", "DETEN001", guarantor.TraderExciseNumber);
			});
		}

		public void TestGuarantor_GuarantorTypeCodeNot3()
		{
			message.Body.ChangeOfDestination.DestinationChanged.MovementGuarantee = new MovementGuaranteeType
			{
				GuarantorTypeCode = GuarantorTypeCode.Item2,
				GuarantorTrader = new Collection<GuarantorTraderType>
				{
					new GuarantorTraderType { TraderExciseNumber = "DETEN001" }
				}
			};
			AssertEquals("Only return Guarantor when GuarantorTypeCode is 3", null, Provider.Guarantor);
		}

		public void TestDeliveryPlace()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", null, Provider.DeliveryPlace);

				message.Body.ChangeOfDestination.DestinationChanged.DeliveryPlaceTrader = new DeliveryPlaceTraderType { Traderid = "DETID001" };
				var deliveryPlace = Provider.DeliveryPlace;
			});
		}

		public void TestNewTransportArranger()
		{
			AssertEquals("Not specified, No exception", null, Provider.NewTransportArranger);
		}

		public void TestNewTransportArranger_TransportArrangement1()
		{
			message.Body.ChangeOfDestination.NewTransportArrangerTrader = new NewTransportArrangerTraderType { VatNumber = "DEVAT001" };
			message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangement = TransportArrangement.Item1;
			message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangementValueSpecified = true;
			AssertEquals("TransportArrangement 1 and 2 -> NewTransportArranger null", null, Provider.NewTransportArranger);
		}

		public void TestNewTransportArranger_TransportArrangement2()
		{
			message.Body.ChangeOfDestination.NewTransportArrangerTrader = new NewTransportArrangerTraderType { VatNumber = "DEVAT001" };
			message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangement = TransportArrangement.Item2;
			message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangementValueSpecified = true;
			AssertEquals("TransportArrangement 1 and 2 -> NewTransportArranger null", null, Provider.NewTransportArranger);
		}

		public void TestNewTransportArranger_TransportArrangement3()
		{
			message.Body.ChangeOfDestination.NewTransportArrangerTrader = new NewTransportArrangerTraderType { VatNumber = "DEVAT001" };
			message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangement = TransportArrangement.Item3;
			message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangementValueSpecified = true;
			var transportArranger = Provider.NewTransportArranger;

			CombineAssertions(() =>
			{
				AssertNotNull("Not null", transportArranger);
			});
		}

		public void TestNewTransportArranger_TransportArrangement4()
		{
			message.Body.ChangeOfDestination.NewTransportArrangerTrader = new NewTransportArrangerTraderType { VatNumber = "DEVAT001" };
			message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangement = TransportArrangement.Item4;
			message.Body.ChangeOfDestination.UpdateEadEsad.ChangedTransportArrangementValueSpecified = true;
			AssertNotNull(Provider.NewTransportArranger);
		}

		public void TestNewTransporter()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", null, Provider.NewTransporter);

				message.Body.ChangeOfDestination.NewTransporterTrader = new NewTransporterTraderType { VatNumber = "DEVAT002" };
				var newTransporter = Provider.NewTransporter;
				AssertNotNull("Not null", newTransporter);
			});
		}

		public void TestTransportDetails_Null()
		{
			AssertEquals("No exception", 0, Provider.TransportDetails.Count);
		}

		public void TestTransportDetails()
		{
			message.Body.ChangeOfDestination.TransportDetails = new Collection<TransportDetailsType>
			{
				new TransportDetailsType { IdentityOfTransportUnits = "ID0001" },
				new TransportDetailsType { IdentityOfTransportUnits = "ID0002" }
			};
			var transportDetails = Provider.TransportDetails;

			CombineAssertions(() =>
			{
				AssertEquals("Count", 2, Provider.TransportDetails.Count);
			});
		}

		public void TestConsignee()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", null, Provider.Consignee);
			});
		}

		protected override IEnumerable<Expression<Func<IE813Provider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Consignee;
			yield return x => x.DeliveryPlace;
			yield return x => x.Guarantor;
			yield return x => x.NewTransporter;
			yield return x => x.NewTransportArranger;
			yield return x => x.UpdateEad;
		}

		protected override IE813Provider GetProvider()
		{
			return new IE813Provider(message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new Ie813Type
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.GB",
					MessageRecipient = "NDEA.GB",
					DateOfPreparation = new DateTime(2022, 07, 14),
					TimeOfPreparation = new DateTime(2022, 07, 14, 13, 29, 08, 000),
					MessageIdentifier = "2C261468-087E-44E7-B6E5-164E1F5217C4",
				},
				Body = new BodyType()
				{
					ChangeOfDestination = new ChangeOfDestinationType()
					{
						UpdateEadEsad = new UpdateEadEsadType
						{
							AdministrativeReferenceCode = AdministrativeReferenceCode,
							SequenceNumber = SequenceNumber,
							JourneyTime = "H12"
						},
						DestinationChanged = new DestinationChangedType
						{
							DestinationTypeCode = ChangedDestinationTypeCode.Item1
						}
					}
				},
			};
		}
		Ie813Type message;

		const string AdministrativeReferenceCode = "20GB41000000001870745";
		const string SequenceNumber = "5";
	}
}

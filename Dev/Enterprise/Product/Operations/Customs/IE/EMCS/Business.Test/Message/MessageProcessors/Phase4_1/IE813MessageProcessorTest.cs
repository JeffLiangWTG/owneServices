using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE813;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tcl;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tms;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing.Phase4_1
{
	[TestedType(typeof(IE813MessageProcessor))]
	sealed class IE813MessageProcessorTest : IE813MessageProcessorAbstractTest<Ie813Type>
	{
		protected override string MessageSampleNameSpace => "Enterprise.Customs.IE.EMCS.Business.Testing.Message.MessageProcessors.Phase4_1.TestFiles.IE813MessageSample.txt";

		protected override Ie813Type CreateDefaultIE813Type()
		{
			return new Ie813Type
			{
				Header = new HeaderType()
				{
					MessageSender = "NDEA.IE",
					MessageRecipient = "NDEA.IE",
					DateOfPreparation = new DateTime(2022, 07, 14),
					TimeOfPreparation = "13:29:08",
					MessageIdentifier = "e3e6f99d-2bc3-4c11-a20f-c7aa391f8573",
				},
				Body = new BodyType()
				{
					ChangeOfDestination = new ChangeOfDestinationType()
					{
						UpdateEadEsad = new UpdateEadEsadType()
						{
							AdministrativeReferenceCode = "MRN198761234",
							SequenceNumber = "8888",
							JourneyTime = "D02",
							TransportModeCode = "9",
							ChangedTransportArrangement = TransportArrangement.Item3,
							InvoiceNumber = "1",
							InvoiceDate = new DateTime(2020, 7, 25),
							ComplementaryInformation = new LsdComplementaryInformationType
							{
								Value = "New instruction",
							},
						},
						DestinationChanged = new DestinationChangedType
						{
							DestinationTypeCode = ChangedDestinationTypeCode.Item4,
							MovementGuarantee = new MovementGuaranteeType
							{
								GuarantorTypeCode = GuarantorTypeCode.Item3,
								GuarantorTrader = new Collection<GuarantorTraderType>(new[]
								{
									new GuarantorTraderType
									{
										VatNumber = "VN004",
										TraderName = "Guarantor",
										Language = "EN",
										City = "Dublin",
										StreetName = "Gp Address",
										StreetNumber = "1",
										Postcode = "0005",
										TraderExciseNumber = "TN001"
									},
								})
							},
							DeliveryPlaceTrader = new DeliveryPlaceTraderType
							{
								Traderid = "TI002",
								Language = "EN",
								StreetName = "DP Address",
								City = "Dublin",
								StreetNumber = "1",
								Postcode = "0005",
								TraderName = "DeliveryPlace Party 1",
							},
							NewConsigneeTrader = new NewConsigneeTraderType
							{
								Traderid = "LVTI002",
								Language = "EN",
								StreetName = "Fifth Steet",
								City = "Dublin",
								StreetNumber = "7",
								Postcode = "0006",
								TraderName = "Consignee Party 1",
							}
						},
						TransportDetails = new Collection<TransportDetailsType>(new[]
						{
							new TransportDetailsType()
							{
								TransportUnitCode = "1",
								IdentityOfTransportUnits = "CO0001",
								CommercialSealIdentification = "SEAL1",
								ComplementaryInformation = new LsdComplementaryInformationType
								{
									Value = "CI001"
								},
								SealInformation = new LsdSealInformationType
								{
									Value = "SI001",
								}
							},
							new TransportDetailsType()
							{
								TransportUnitCode = "2",
								IdentityOfTransportUnits = "CO0002",
								CommercialSealIdentification = "SEAL2",
								ComplementaryInformation = new LsdComplementaryInformationType
								{
									Value = "CI002"
								},
								SealInformation = new LsdSealInformationType
								{
									Value = "SI002",
								}
							},
						}),
						NewTransporterTrader = new NewTransporterTraderType
						{
							VatNumber = "VN002",
							Language = "EN",
							StreetName = "TP Address",
							TraderName = "New Transporter Trader",
							City = "Dublin",
							Postcode = "0001",
							StreetNumber = "1",
						},
						NewTransportArrangerTrader = new NewTransportArrangerTraderType
						{
							VatNumber = "VN003",
							Language = "EN",
							StreetName = "TAP Address",
							TraderName = "New Transport Arranger Trader",
							City = "Dublin",
							Postcode = "0002",
							StreetNumber = "1",
						}
					},
				},
			};
		}

		protected override Ie813Type CreateMissingOrEmptyElementsMessage()
		{
			return new Ie813Type
			{
				Header = new HeaderType()
				{
					MessageSender = "NDEA.IE",
					MessageRecipient = "NDEA.IE",
					DateOfPreparation = new DateTime(2022, 07, 14),
					TimeOfPreparation = "13:29:08",
					MessageIdentifier = "e3e6f99d-2bc3-4c11-a20f-c7aa391f8573"
				},
				Body = new BodyType()
				{
					ChangeOfDestination = new ChangeOfDestinationType()
					{
						UpdateEadEsad = new UpdateEadEsadType()
						{
							SequenceNumber = "8888",
						},
						DestinationChanged = new DestinationChangedType
						{
						},
					},
				},
			};
		}

		protected override void UpdateTraderIdEmpty()
		{
			ie813.Body.ChangeOfDestination.DestinationChanged.NewConsigneeTrader.Traderid = "";
		}

		protected override void UpdateTraderId2NotMatchImporter()
		{
			ie813.Body.ChangeOfDestination.DestinationChanged.NewConsigneeTrader.Traderid = "DETI123";
		}

		protected override void UpdateTransportMode2Zero()
		{
			ie813.Body.ChangeOfDestination.UpdateEadEsad.TransportModeCode = "0";
		}

		protected override void UpdateTransportMode2Invalid()
		{
			ie813.Body.ChangeOfDestination.UpdateEadEsad.TransportModeCode = "-1";
		}

		protected override void UpdateGuarantor2Null()
		{
			ie813.Body.ChangeOfDestination.DestinationChanged.MovementGuarantee.GuarantorTrader = null;
		}

		protected override void UpdateTransportArranger2Null()
		{
			ie813.Body.ChangeOfDestination.NewTransportArrangerTrader = null;
		}
	}
}

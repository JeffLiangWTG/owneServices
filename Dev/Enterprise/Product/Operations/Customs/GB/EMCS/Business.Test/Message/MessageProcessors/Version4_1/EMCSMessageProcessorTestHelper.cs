using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tcl;
using CargoWise.Types;
using EMCSVersion4_1 = CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1;

namespace Enterprise.Customs.GB.EMCS.Business.Testing.Version4_1
{
	static class EMCSMessageProcessorTestHelper
	{
		internal static string GetStandardIE704Text() => EmbeddedResourceHelper.GetdMessageXml("TestFiles.Version4_1.IE704.xml");

		internal static string GetStandardIE801Text()
		{
			var ie801 = new EMCSVersion4_1.ie801.Ie801Type
			{
				Header = new EMCSVersion4_1.tms.HeaderType
				{
					MessageSender = "NDEA.GB",
					MessageRecipient = "NDEA.GB",
					DateOfPreparation = new DateTime(2022, 08, 30),
					TimeOfPreparation = new DateTime(2022, 08, 30, 13, 29, 08, 000),
					MessageIdentifier = "473D0B48-6C1D-4DF9-901F-CCD5326990C3",
				},
				Body = new EMCSVersion4_1.ie801.BodyType
				{
					EadesadContainer = new EMCSVersion4_1.ie801.EadesadContainerType
					{
						ExciseMovement = new EMCSVersion4_1.ie801.ExciseMovementType
						{
							AdministrativeReferenceCode = "MRN1234567",
							DateAndTimeOfValidationOfEadEsad = new DateTime(2022, 08, 30, 13, 30, 00),
						},
						HeaderEadEsad = new EMCSVersion4_1.ie801.HeaderEadEsadType
						{
							SequenceNumber = "1",
							JourneyTime = "H12",
							DestinationTypeCode = DestinationTypeCode.Item1,
							TransportArrangement = TransportArrangement.Item3
						},
						BodyEadEsad = new Collection<EMCSVersion4_1.ie801.BodyEadEsadType>
						{
							new EMCSVersion4_1.ie801.BodyEadEsadType
							{
								BodyRecordUniqueReference = "1",
								ExciseProductCode = "W200",
								CnCode = "22084011",
								FiscalMarkUsedFlagValueSpecified = true,
								FiscalMarkUsedFlag = Flag.Item1,
								FiscalMark = new EMCSVersion4_1.ie801.LsdFiscalMarkType
								{
									Language = "en",
									Value = "FM001"
								},
								DesignationOfOrigin = new EMCSVersion4_1.ie801.LsdDesignationOfOriginType
								{
									Language = "en",
									Value = "CN"
								},
								CommercialDescription = new EMCSVersion4_1.ie801.LsdCommercialDescriptionType
								{
									Language = "en",
									Value = "CD001"
								},
								BrandNameOfProducts = new EMCSVersion4_1.ie801.LsdBrandNameOfProductsType
								{
									Language = "en",
									Value = "BP001"
								},
								MaturationPeriodOrAgeOfProducts = new EMCSVersion4_1.ie801.LsdMaturationPeriodOrAgeOfProductsType
								{
									Language = "en",
									Value = "10Months"
								},
								IndependentSmallProducersDeclaration = new EMCSVersion4_1.ie801.LsdIndependentSmallProducersDeclarationType
								{
									Language = "en",
									Value = "Independent Small Producers Declaration"
								},
								Quantity = 2m,
								GrossMass = 3m,
								NetMass = 4m,
								AlcoholicStrengthByVolumeInPercentageValueSpecified = true,
								AlcoholicStrengthByVolumeInPercentage = 5m,
								DegreePlatoValueSpecified = true,
								DegreePlato = 6m,
								SizeOfProducer = "7",
								DensityValueSpecified = true,
								Density = 8m,
								WineProduct = new EMCSVersion4_1.ie801.WineProductType
								{
									WineGrowingZoneCode = "1",
									WineProductCategory = CategoryOfWineProduct.Item1,
									ThirdCountryOfOrigin = Core.Constants.CountryCodes.UnitedKingdom,
									OtherInformation = new EMCSVersion4_1.ie801.LsdOtherInformationType
									{
										Language = "en",
										Value = "WPOI001"
									},
									WineOperation = new Collection<EMCSVersion4_1.ie801.WineOperationType>
									{
										new EMCSVersion4_1.ie801.WineOperationType
										{
											WineOperationCode = "WO001"
										},
										new EMCSVersion4_1.ie801.WineOperationType
										{
											WineOperationCode = "WO002"
										}
									}
								},
								Package = new Collection<EMCSVersion4_1.ie801.PackageType>
								{
									new EMCSVersion4_1.ie801.PackageType
									{
										KindOfPackages = "CT",
										NumberOfPackages = "5",
										CommercialSealIdentification = "SN001",
										SealInformation = new EMCSVersion4_1.ie801.LsdSealInformationType
										{
											Language = "en",
											Value = "SC001"
										},
										ShippingMarks = "Shipping Marks 1"
									}
								}
							}
						},
						EadEsad = new EMCSVersion4_1.ie801.EadEsadType
						{
							LocalReferenceNumber = "B000222547896254786321",
							DateOfDispatch = new DateTime(2022, 8, 30),
							TimeOfDispatch = new DateTime(2022, 8, 30, 13, 30, 0),
							TimeOfDispatchValueSpecified = true,
							OriginTypeCode = OriginTypeCode.Item1,
							InvoiceNumber = "1",
							InvoiceDate = new DateTime(2022, 8, 29, 13, 30, 0),
							InvoiceDateValueSpecified = true,
							ImportSad = new Collection<EMCSVersion4_1.ie801.ImportSadType>
							{
								new EMCSVersion4_1.ie801.ImportSadType
								{
									ImportSadNumber = "SAD001"
								},
								new EMCSVersion4_1.ie801.ImportSadType
								{
									ImportSadNumber = "SAD002"
								}
							}
						},
						DispatchImportOffice = new EMCSVersion4_1.ie801.DispatchImportOfficeType
						{
							ReferenceNumber = "DIO001"
						},
						DeliveryPlaceCustomsOffice = new EMCSVersion4_1.ie801.DeliveryPlaceCustomsOfficeType
						{
							ReferenceNumber = "DPCO001"
						},
						CompetentAuthorityDispatchOffice = new EMCSVersion4_1.ie801.CompetentAuthorityDispatchOfficeType
						{
							ReferenceNumber = "CADO001"
						},
						MovementGuarantee = new EMCSVersion4_1.ie801.MovementGuaranteeType
						{
							GuarantorTypeCode = GuarantorTypeCode.Item3,
							GuarantorTrader = new Collection<EMCSVersion4_1.ie801.GuarantorTraderType>
							{
								new EMCSVersion4_1.ie801.GuarantorTraderType
								{
									TraderExciseNumber = "GB001",
									VatNumber = "GB001",
									Language = "en",
									TraderName = "Guarantor Party 1",
									StreetName = "GP Address",
									StreetNumber = "1",
									City = "London",
									Postcode = "0001"
								}
							}
						},
						TransportMode = new EMCSVersion4_1.ie801.TransportModeType
						{
							TransportModeCode = "4",
							ComplementaryInformation = new EMCSVersion4_1.ie801.LsdComplementaryInformationType
							{
								Language = "en",
								Value = "CI001"
							}
						},
						ComplementConsigneeTrader = new EMCSVersion4_1.ie801.ComplementConsigneeTraderType
						{
							MemberStateCode = "12",
							SerialNumberOfCertificateOfExemption = "CE001"
						},
						ConsigneeTrader = new EMCSVersion4_1.ie801.ConsigneeTraderType
						{
							Traderid = "GB001",
							Language = "en",
							TraderName = "Consignee Party 1",
							StreetName = "CP Address",
							StreetNumber = "1",
							City = "London",
							Postcode = "0001"
						},
						ConsignorTrader = new EMCSVersion4_1.ie801.ConsignorTraderType
						{
							TraderExciseNumber = "GB002",
							Language = "en",
							TraderName = "Consignor Party 1",
							StreetName = "CRP Address",
							StreetNumber = "1",
							City = "London",
							Postcode = "0001"
						},
						PlaceOfDispatchTrader = new EMCSVersion4_1.ie801.PlaceOfDispatchTraderType
						{
							ReferenceOfTaxWarehouse = "GBW001",
							Language = "en",
							TraderName = "PartyPlaceOfDispatch Party 1",
							StreetName = "PRD Address",
							StreetNumber = "1",
							City = "London",
							Postcode = "0001"
						},
						DeliveryPlaceTrader = new EMCSVersion4_1.ie801.DeliveryPlaceTraderType
						{
							Traderid = "GB002",
							Language = "en",
							TraderName = "DeliveryPlace Party 1",
							StreetName = "DP Address",
							StreetNumber = "1",
							City = "London",
							Postcode = "0001"
						},
						TransportArrangerTrader = new EMCSVersion4_1.ie801.TransportArrangerTraderType
						{
							VatNumber = "DE002",
							TraderName = "TransportArranger Party 1",
							StreetName = "TAP Address",
							StreetNumber = "1",
							Language = "de",
							City = "Berlin",
							Postcode = "0001"
						},
						FirstTransporterTrader = new EMCSVersion4_1.ie801.FirstTransporterTraderType
						{
							VatNumber = "DE003",
							TraderName = "FirstTransporter Party 1",
							StreetName = "FTP Address",
							StreetNumber = "1",
							Language = "de",
							City = "Berlin",
							Postcode = "0001"
						},
						DocumentCertificate = new Collection<EMCSVersion4_1.ie801.DocumentCertificateType>
						{
							new EMCSVersion4_1.ie801.DocumentCertificateType
							{
								DocumentReference = "Ref001",
								DocumentDescription = new EMCSVersion4_1.ie801.LsdDocumentDescriptionType
								{
									Language = "en",
									Value = "DC001"
								},
								DocumentType = "TP001"
							},
							new EMCSVersion4_1.ie801.DocumentCertificateType
							{
								DocumentReference = "Ref002",
								DocumentDescription = new EMCSVersion4_1.ie801.LsdDocumentDescriptionType
								{
									Language = "en",
									Value = "DC002"
								},
								DocumentType = "TP002"
							}
						},
						TransportDetails = new Collection<EMCSVersion4_1.ie801.TransportDetailsType>
						{
							new EMCSVersion4_1.ie801.TransportDetailsType
							{
								TransportUnitCode = "1",
								IdentityOfTransportUnits = "CO0001",
								CommercialSealIdentification = "SEAL1",
								ComplementaryInformation = new EMCSVersion4_1.ie801.LsdComplementaryInformationType
								{
									Language = "en",
									Value = "CI001"
								},
								SealInformation = new EMCSVersion4_1.ie801.LsdSealInformationType
								{
									Language = "EenN",
									Value = "SI001"
								}
							},
							new EMCSVersion4_1.ie801.TransportDetailsType
							{
								TransportUnitCode = "2",
								IdentityOfTransportUnits = "CO0002",
								CommercialSealIdentification = "SEAL2",
								ComplementaryInformation = new EMCSVersion4_1.ie801.LsdComplementaryInformationType
								{
									Language = "en",
									Value = "CI002"
								},
								SealInformation = new EMCSVersion4_1.ie801.LsdSealInformationType
								{
									Language = "en",
									Value = "SI002"
								}
							}
						}
					}
				}
			};
			return EMCSXmlObjectSerializer.Serialize(ie801);
		}

		internal static string GetStandardIE802Text()
		{
			var ie802 = new EMCSVersion4_1.ie802.Ie802Type
			{
				Header = new EMCSVersion4_1.tms.HeaderType
				{
					MessageSender = "NDEA.GB",
					MessageRecipient = "NDEA.GB",
					DateOfPreparation = new DateTime(2022, 09, 01),
					TimeOfPreparation = new DateTime(2022, 09, 01, 15, 30, 05),
					MessageIdentifier = "0F893C3F-B87A-4B35-938C-EB3B44401111",
				},
				Body = new EMCSVersion4_1.ie802.BodyType()
				{
					ReminderMessageForExciseMovement = new EMCSVersion4_1.ie802.ReminderMessageForExciseMovementType()
					{
						ExciseMovement = new EMCSVersion4_1.ie802.ExciseMovementType
						{
							AdministrativeReferenceCode = "MRN1234567",
							SequenceNumber = "1"
						},
						Attributes = new EMCSVersion4_1.ie802.AttributesType
						{
							LimitDateAndTime = new DateTime(2022, 09, 03, 13, 16, 30),
							ReminderInformation = new EMCSVersion4_1.ie802.LsdReminderInformationType
							{
								Value = "You received this reminder information.",
								Language = "en"
							},
							ReminderMessageType = ReminderMessageType.Item1
						}
					}
				}
			};
			return EMCSXmlObjectSerializer.Serialize(ie802);
		}

		internal static string GetStandardIE803Text()
		{
			var ie803 = new EMCSVersion4_1.ie803.Ie803Type
			{
				Header = new EMCSVersion4_1.tms.HeaderType
				{
					MessageSender = "NDEA.GB",
					MessageRecipient = "NDEA.GB",
					DateOfPreparation = new DateTime(2022, 08, 24),
					TimeOfPreparation = new DateTime(2022, 08, 24, 15, 30, 08, 000),
					MessageIdentifier = "EE838BA2-7024-49A7-ACF8-4C2F07DF535D",
				},
				Body = new EMCSVersion4_1.ie803.BodyType
				{
					NotificationOfDivertedEadesad = new EMCSVersion4_1.ie803.NotificationOfDivertedEadesadType
					{
						ExciseNotification = new EMCSVersion4_1.ie803.ExciseNotificationType
						{
							NotificationDateAndTime = new DateTime(2022, 09, 01, 15, 30, 05),
							NotificationType = NotificationType.Item1,
							AdministrativeReferenceCode = "MRN1234567",
							SequenceNumber = "1"
						},
						DownstreamArc = new Collection<EMCSVersion4_1.ie803.DownstreamArcType>
						{
							new EMCSVersion4_1.ie803.DownstreamArcType
							{
								AdministrativeReferenceCode = "20GB66421598431563461"
							}
						}
					}
				}
			};
			return EMCSXmlObjectSerializer.Serialize(ie803);
		}

		internal static EMCSVersion4_1.ie807.Ie807Type GetStandardIE807Type()
		{
			return new EMCSVersion4_1.ie807.Ie807Type
			{
				Header = new EMCSVersion4_1.tms.HeaderType
				{
					MessageIdentifier = "GB90003480001003"
				},
				Body = new EMCSVersion4_1.ie807.BodyType
				{
					InterruptionOfMovement = new EMCSVersion4_1.ie807.InterruptionOfMovementType()
					{
						Attributes = new EMCSVersion4_1.ie807.AttributesType
						{
							AdministrativeReferenceCode = "20GB41000000001870745",
							ReasonForInterruptionCode = "1",
							ComplementaryInformation = new EMCSVersion4_1.ie807.LsdComplementaryInformationType() { Language = "en", Value = "The movement has been interrupted" }
						},
						ReferenceControlReport = new Collection<EMCSVersion4_1.ie807.ReferenceControlReportType>
						{
							new EMCSVersion4_1.ie807.ReferenceControlReportType
							{
								ControlReportReference = "46332156"
							},
							new EMCSVersion4_1.ie807.ReferenceControlReportType
							{
								ControlReportReference = "64831215"
							}
						},
						ReferenceEventReport = new Collection<EMCSVersion4_1.ie807.ReferenceEventReportType>
						{
							new EMCSVersion4_1.ie807.ReferenceEventReportType
							{
								EventReportNumber = "64533189"
							},
							new EMCSVersion4_1.ie807.ReferenceEventReportType
							{
								EventReportNumber = "2036454D"
							}
						}
					}
				}
			};
		}

		internal static string GetStandardIE810Text()
		{
			var ie810 = new EMCSVersion4_1.ie810.Ie810Type
			{
				Header = new EMCSVersion4_1.tms.HeaderType
				{
					MessageSender = "NDEA.GB",
					MessageRecipient = "NDEA.GB",
					DateOfPreparation = new DateTime(2022, 07, 14),
					TimeOfPreparation = new DateTime(2022, 07, 14, 13, 29, 08, 000),
					MessageIdentifier = "183E4B2D-0EDC-4725-BD0C-7884ABCAA427",
				},
				Body = new EMCSVersion4_1.ie810.BodyType
				{
					CancellationOfEad = new EMCSVersion4_1.ie810.CancellationOfEadType
					{
						Attributes = new EMCSVersion4_1.ie810.AttributesType { DateAndTimeOfValidationOfCancellation = new DateTime(2022, 07, 14, 13, 29, 08, 000) },
						ExciseMovementEad = new EMCSVersion4_1.ie810.ExciseMovementEadType { AdministrativeReferenceCode = "22GB58500000004684557" },
						Cancellation = new EMCSVersion4_1.ie810.CancellationType { CancellationReasonCode = "0", ComplementaryInformation = new EMCSVersion4_1.ie810.LsdComplementaryInformationType { Language = "en" } }
					}
				}
			};
			return EMCSXmlObjectSerializer.Serialize(ie810);
		}

		internal static EMCSVersion4_1.ie813.Ie813Type GetStandardIE813Type()
		{
			return new EMCSVersion4_1.ie813.Ie813Type
			{
				Header = new EMCSVersion4_1.tms.HeaderType
				{
					MessageSender = "NDEA.GB",
					MessageRecipient = "NDEA.GB",
					DateOfPreparation = new DateTime(2022, 07, 14),
					TimeOfPreparation = new DateTime(2022, 07, 14, 13, 29, 08, 000),
					MessageIdentifier = "9ED0B929-E859-4DE9-8F41-20D511169F38",
				},
				Body = new EMCSVersion4_1.ie813.BodyType()
				{
					ChangeOfDestination = new EMCSVersion4_1.ie813.ChangeOfDestinationType()
					{
						UpdateEadEsad = new EMCSVersion4_1.ie813.UpdateEadEsadType()
						{
							AdministrativeReferenceCode = "MRN198761234",
							SequenceNumber = "8888",
							JourneyTime = "D02",
							TransportModeCode = "9",
							ChangedTransportArrangementValueSpecified = true,
							ChangedTransportArrangement = TransportArrangement.Item3,
							InvoiceNumber = "1",
							InvoiceDate = new DateTime(2020, 7, 25),
							InvoiceDateValueSpecified = true,
							ComplementaryInformation = new EMCSVersion4_1.ie813.LsdComplementaryInformationType
							{
								Value = "New instruction",
							},
						},
						DestinationChanged = new EMCSVersion4_1.ie813.DestinationChangedType
						{
							DestinationTypeCode = ChangedDestinationTypeCode.Item4,
							MovementGuarantee = new EMCSVersion4_1.ie813.MovementGuaranteeType
							{
								GuarantorTypeCode = GuarantorTypeCode.Item3,
								GuarantorTrader = new Collection<EMCSVersion4_1.ie813.GuarantorTraderType>
								{
									new EMCSVersion4_1.ie813.GuarantorTraderType
									{
										VatNumber = "VN004",
										TraderName = "Guarantor",
										Language = "EN",
										City = "London",
										StreetName = "Gp Address",
										StreetNumber = "1",
										Postcode = "0005",
										TraderExciseNumber = "TN001",
									},
								}
							},
							DeliveryPlaceTrader = new EMCSVersion4_1.ie813.DeliveryPlaceTraderType
							{
								Traderid = "TI002",
								Language = "EN",
								StreetName = "DP Address",
								City = "London",
								StreetNumber = "1",
								Postcode = "0005",
								TraderName = "DeliveryPlace Party 1",
							},
							NewConsigneeTrader = new EMCSVersion4_1.ie813.NewConsigneeTraderType
							{
								Traderid = "LVTI002",
								Language = "EN",
								StreetName = "Fifth Steet",
								City = "London",
								StreetNumber = "7",
								Postcode = "0006",
								TraderName = "Consignee Party 1",
							},
						},
						TransportDetails = new Collection<EMCSVersion4_1.ie813.TransportDetailsType>
						{
							new EMCSVersion4_1.ie813.TransportDetailsType()
							{
								TransportUnitCode = "1",
								IdentityOfTransportUnits = "CO0001",
								CommercialSealIdentification = "SEAL1",
								ComplementaryInformation = new EMCSVersion4_1.ie813.LsdComplementaryInformationType
								{
									Value = "CI001"
								},
								SealInformation = new EMCSVersion4_1.ie813.LsdSealInformationType
								{
									Value = "SI001",
								}
							},
							new EMCSVersion4_1.ie813.TransportDetailsType()
							{
								TransportUnitCode = "2",
								IdentityOfTransportUnits = "CO0002",
								CommercialSealIdentification = "SEAL2",
								ComplementaryInformation = new EMCSVersion4_1.ie813.LsdComplementaryInformationType
								{
									Value = "CI002"
								},
								SealInformation = new EMCSVersion4_1.ie813.LsdSealInformationType
								{
									Value = "SI002",
								}
							},
						},
						NewTransporterTrader = new EMCSVersion4_1.ie813.NewTransporterTraderType
						{
							VatNumber = "VN002",
							Language = "EN",
							StreetName = "TP Address",
							TraderName = "New Transporter Trader",
							City = "London",
							Postcode = "0001",
							StreetNumber = "1",
						},
						NewTransportArrangerTrader = new EMCSVersion4_1.ie813.NewTransportArrangerTraderType
						{
							VatNumber = "VN003",
							Language = "EN",
							StreetName = "TAP Address",
							TraderName = "New Transport Arranger Trader",
							City = "London",
							Postcode = "0002",
							StreetNumber = "1",
						}
					},
				},
			};
		}

		internal static string GetStandardIE818Text()
		{
			var reason1 = new EMCSVersion4_1.ie818.UnsatisfactoryReasonType
			{
				UnsatisfactoryReasonCode = "3",
				ComplementaryInformation = new EMCSVersion4_1.ie818.LsdComplementaryInformationType
				{
					Language = "en",
					Value = "Goods were damaged during transport"
				}
			};
			var reason2 = new EMCSVersion4_1.ie818.UnsatisfactoryReasonType
			{
				UnsatisfactoryReasonCode = "2",
				ComplementaryInformation = new EMCSVersion4_1.ie818.LsdComplementaryInformationType
				{
					Language = "en",
					Value = "Quantity is less than what was reported"
				}
			};
			var ie818 = new EMCSVersion4_1.ie818.Ie818Type
			{
				Header = new EMCSVersion4_1.tms.HeaderType
				{
					MessageSender = "NDEA.GB",
					MessageRecipient = "NDEA.GB",
					DateOfPreparation = new DateTime(2022, 09, 10),
					TimeOfPreparation = new DateTime(2022, 09, 10, 13, 29, 08, 000),
					MessageIdentifier = "9E6079A9-AFDB-41E6-B9C5-46BF616C1DA0",
				},
				Body = new EMCSVersion4_1.ie818.BodyType
				{
					AcceptedOrRejectedReportOfReceiptExport = new EMCSVersion4_1.ie818.AcceptedOrRejectedReportOfReceiptExportType
					{
						ExciseMovement = new EMCSVersion4_1.ie818.ExciseMovementType
						{
							AdministrativeReferenceCode = "MRN1234567",
							SequenceNumber = "1"
						},
						ReportOfReceiptExport = new EMCSVersion4_1.ie818.ReportOfReceiptExportType
						{
							GlobalConclusionOfReceipt = GlobalConclusionOfReceipt.Item4
						},
						BodyReportOfReceiptExport = new Collection<EMCSVersion4_1.ie818.BodyReportOfReceiptExportType>
						{
							new EMCSVersion4_1.ie818.BodyReportOfReceiptExportType
							{
								BodyRecordUniqueReference = "1",
								IndicatorOfShortageOrExcess = IndicatorOfShortageOrExcess.S,
								IndicatorOfShortageOrExcessValueSpecified = true,
								ObservedShortageOrExcess = 10,
								ObservedShortageOrExcessValueSpecified = true,
								RefusedQuantity = 20,
								RefusedQuantityValueSpecified = true,
								UnsatisfactoryReason = new Collection<EMCSVersion4_1.ie818.UnsatisfactoryReasonType>
								{
									reason1,
									reason2
								}
							},
							new EMCSVersion4_1.ie818.BodyReportOfReceiptExportType
							{
								BodyRecordUniqueReference = "2",
								IndicatorOfShortageOrExcessValueSpecified = true,
								IndicatorOfShortageOrExcess = IndicatorOfShortageOrExcess.E,
								ObservedShortageOrExcessValueSpecified = true,
								ObservedShortageOrExcess = 30,
								RefusedQuantity = 20,
								RefusedQuantityValueSpecified = true,
								UnsatisfactoryReason = new Collection<EMCSVersion4_1.ie818.UnsatisfactoryReasonType>
								{
									reason1
								}
							}
						}
					}
				}
			};
			return EMCSXmlObjectSerializer.Serialize(ie818);
		}

		internal static string GetStandardIE819Text()
		{
			var ie819 = new EMCSVersion4_1.ie819.Ie819Type
			{
				Header = new EMCSVersion4_1.tms.HeaderType
				{
					MessageSender = "NDEA.GB",
					MessageRecipient = "NDEA.GB",
					DateOfPreparation = new DateTime(2022, 09, 14),
					TimeOfPreparation = new DateTime(2022, 09, 14, 13, 29, 08, 000),
					MessageIdentifier = "ECB0B727-E99C-46B5-9BC3-0B303242B9F2",
				},
				Body = new EMCSVersion4_1.ie819.BodyType
				{
					AlertOrRejectionOfEadesad = new EMCSVersion4_1.ie819.AlertOrRejectionOfEadesadType
					{
						ExciseMovement = new EMCSVersion4_1.ie819.ExciseMovementType
						{
							AdministrativeReferenceCode = "MRN1234567",
							SequenceNumber = "1"
						},
						AlertOrRejectionOfEadEsadReason = new Collection<EMCSVersion4_1.ie819.AlertOrRejectionOfEadEsadReasonType>
						{
							new EMCSVersion4_1.ie819.AlertOrRejectionOfEadEsadReasonType
							{
								AlertOrRejectionOfMovementReasonCode = "0",
								ComplementaryInformation = new EMCSVersion4_1.ie819.LsdComplementaryInformationType
								{
									Language = "en",
									Value = "Test first information"
								}
							},
							new EMCSVersion4_1.ie819.AlertOrRejectionOfEadEsadReasonType
							{
								AlertOrRejectionOfMovementReasonCode = "3",
								ComplementaryInformation = new EMCSVersion4_1.ie819.LsdComplementaryInformationType
								{
									Language = "en",
									Value = "Test second information"
								}
							}
						}
					}
				}
			};
			return EMCSXmlObjectSerializer.Serialize(ie819);
		}

		internal static string GetStandardIE829Text(bool isMultipleMRN)
		{
			var exciseMovementEads = new Collection<EMCSVersion4_1.ie829.ExciseMovementEadType>
			{
				new EMCSVersion4_1.ie829.ExciseMovementEadType
				{
					AdministrativeReferenceCode = "MRN1234567",
					SequenceNumber = "1"
				}
			};
			if (isMultipleMRN)
			{
				exciseMovementEads.Add(new EMCSVersion4_1.ie829.ExciseMovementEadType
				{
					AdministrativeReferenceCode = "MRN7654321",
					SequenceNumber = "1"
				});
			}

			var ie829 = new EMCSVersion4_1.ie829.Ie829Type
			{
				Header = new EMCSVersion4_1.tms.HeaderType
				{
					MessageSender = "NDEA.GB",
					MessageRecipient = "NDEA.GB",
					DateOfPreparation = new DateTime(2022, 08, 24),
					TimeOfPreparation = new DateTime(2022, 08, 24, 15, 30, 08, 000),
					MessageIdentifier = "B6820B40-4313-4BB7-8D53-60524EE16CB6",
				},
				Body = new EMCSVersion4_1.ie829.BodyType
				{
					NotificationOfAcceptedExport = new EMCSVersion4_1.ie829.NotificationOfAcceptedExportType
					{
						ExportDeclarationAcceptanceRelease = new EMCSVersion4_1.ie829.ExportDeclarationAcceptanceReleaseType
						{
							DateOfAcceptance = new DateTime(2022, 8, 24),
							ReferenceNumberOfSenderCustomsOffice = "GB003302",
							DocumentReferenceNumber = "20GB12365485421158E2",
						},
						ExciseMovementEad = exciseMovementEads
					}
				}
			};
			return EMCSXmlObjectSerializer.Serialize(ie829);
		}

		internal static string GetStandardIE837Text() => EmbeddedResourceHelper.GetdMessageXml("TestFiles.Version4_1.IE837.xml");

		internal static string GetStandardIE839Text(bool isMultipleMRN)
		{
			var cEadVals = new Collection<EMCSVersion4_1.ie839.CEadValType>
			{
				new EMCSVersion4_1.ie839.CEadValType
				{
					AdministrativeReferenceCode = "MRN1234567",
					SequenceNumber = "1"
				}
			};
			if (isMultipleMRN)
			{
				cEadVals.Add(new EMCSVersion4_1.ie839.CEadValType
				{
					AdministrativeReferenceCode = "MRN7654321",
					SequenceNumber = "1"
				});
			}

			var ie839 = new EMCSVersion4_1.ie839.Ie839Type
			{
				Header = new EMCSVersion4_1.tms.HeaderType
				{
					MessageSender = "NDEA.GB",
					MessageRecipient = "NDEA.GB",
					DateOfPreparation = new DateTime(2022, 08, 24),
					TimeOfPreparation = new DateTime(2022, 08, 24, 15, 30, 08, 000),
					MessageIdentifier = "AB831A9B-83FC-4E9D-8B2F-A0CDFBE0700C",
				},
				Body = new EMCSVersion4_1.ie839.BodyType
				{
					RefusalByCustoms = new EMCSVersion4_1.ie839.RefusalByCustomsType
					{
						Attributes = new EMCSVersion4_1.ie839.AttributesType
						{
							DateAndTimeOfIssuance = new DateTime(2022, 8, 24)
						},
						ExportDeclarationInformation = new EMCSVersion4_1.ie839.ExportDeclarationInformationType
						{
							DocumentReferenceNumber = "20GB12365485421158E2"
						},
						Rejection = new EMCSVersion4_1.ie839.RejectionType
						{
							RejectionDateAndTime = new DateTime(2022, 8, 24),
							RejectionReasonCode = new CustomsRejectionReasonCode()
						},
						CEadVal = cEadVals
					}
				}
			};

			return EMCSXmlObjectSerializer.Serialize(ie839);
		}

		internal static EMCSVersion4_1.ie840.Ie840Type GetStandardIE840Type()
		{
			return new EMCSVersion4_1.ie840.Ie840Type()
			{
				Header = new EMCSVersion4_1.tms.HeaderType
				{
					MessageIdentifier = "0072260102"
				},
				Body = new EMCSVersion4_1.ie840.BodyType()
				{
					EventReportEnvelope = new EMCSVersion4_1.ie840.EventReportEnvelopeType()
					{
						ExciseMovement = new EMCSVersion4_1.ie840.ExciseMovementType()
						{
							AdministrativeReferenceCode = "20GB41000000001870745",
							SequenceNumber = "1"
						}
					}
				}
			};
		}

		internal static EMCSVersion4_1.ie871.Ie871Type GetStandardIE871Type()
		{
			return new EMCSVersion4_1.ie871.Ie871Type()
			{
				Header = new EMCSVersion4_1.tms.HeaderType
				{
					MessageIdentifier = "0072260102"
				},
				Body = new EMCSVersion4_1.ie871.BodyType()
				{
					ExplanationOnReasonForShortage = new EMCSVersion4_1.ie871.ExplanationOnReasonForShortageType()
					{
						ExciseMovement = new EMCSVersion4_1.ie871.ExciseMovementType()
						{
							AdministrativeReferenceCode = "20GB41000000001870745",
							SequenceNumber = "1"
						},
						Analysis = new EMCSVersion4_1.ie871.AnalysisType()
						{
							GlobalExplanation = new EMCSVersion4_1.ie871.LsdGlobalExplanationType() { Language = "en", Value = "Global Explanation for Shrtage or Excess" }
						},
						BodyAnalysis = new Collection<EMCSVersion4_1.ie871.BodyAnalysisType>
						{
							new EMCSVersion4_1.ie871.BodyAnalysisType() { ActualQuantity = 12.123m, ActualQuantityValueSpecified = true, BodyRecordUniqueReference = "1", ExciseProductCode = "B000",
								Explanation = new EMCSVersion4_1.ie871.LsdExplanationType() { Language = "en", Value = "Shortage explanation" } },
							new EMCSVersion4_1.ie871.BodyAnalysisType() { ActualQuantity = 3.55m, ActualQuantityValueSpecified = true, BodyRecordUniqueReference = "2", ExciseProductCode = "S200",
								Explanation = new EMCSVersion4_1.ie871.LsdExplanationType() { Language = "en", Value = "Excess explanation" } },
						},
					}
				}
			};
		}

		internal static EMCSVersion4_1.ie881.Ie881Type GetStandardIE881Type()
		{
			return new EMCSVersion4_1.ie881.Ie881Type
			{
				Header = new EMCSVersion4_1.tms.HeaderType
				{
					MessageSender = "NDEA.GB",
					MessageRecipient = "NDEA.GB",
					DateOfPreparation = new DateTime(2022, 10, 03),
					TimeOfPreparation = new DateTime(2022, 10, 03, 09, 15, 00),
					MessageIdentifier = "GB90003480001003",
				},
				Body = new EMCSVersion4_1.ie881.BodyType
				{
					ManualClosureResponse = new EMCSVersion4_1.ie881.ManualClosureResponseType
					{
						Attributes = new EMCSVersion4_1.ie881.AttributesType
						{
							AdministrativeReferenceCode = "MRN000001",
							SequenceNumber = "1",
							ManualClosureRequestAccepted = Flag.Item1,
						},
						BodyManualClosure = new Collection<EMCSVersion4_1.ie881.BodyManualClosureType>
						{
							new EMCSVersion4_1.ie881.BodyManualClosureType { },
						},
						SupportingDocuments = new Collection<EMCSVersion4_1.ie881.SupportingDocumentsType>
						{
							new EMCSVersion4_1.ie881.SupportingDocumentsType { }
						}
					}
				}
			};
		}

		internal static ZString GetStandardIE905Text()
		{
			var ie905 = new EMCSVersion4_1.ie905.Ie905Type
			{
				Header = new EMCSVersion4_1.tms.HeaderType
				{
					MessageSender = "NDEA.GB",
					MessageRecipient = "NDEA.GB",
					DateOfPreparation = new DateTime(2022, 07, 14),
					TimeOfPreparation = new DateTime(2022, 07, 14, 13, 29, 08, 000),
					MessageIdentifier = "D4EC48DB-14A0-46A2-941A-87D0A26E976D",
					CorrelationIdentifier = "00000000000215",
				},
				Body = new EMCSVersion4_1.ie905.BodyType
				{
					StatusResponse = new EMCSVersion4_1.ie905.StatusResponseType
					{
						Attributes = new EMCSVersion4_1.ie905.AttributesType
						{
							AdministrativeReferenceCode = "MRN1234567",
							LastReceivedMessageType = EMCSVersion4_1.tcl.RequestedMessageType.Ie801,
							SequenceNumber = "1",
							Status = EMCSVersion4_1.tcl.StatusType.X06
						}
					}
				}
			};
			return EMCSXmlObjectSerializer.Serialize(ie905);
		}
	}
}

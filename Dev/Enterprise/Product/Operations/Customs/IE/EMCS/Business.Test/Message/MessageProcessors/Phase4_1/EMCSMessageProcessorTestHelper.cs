using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tcl;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tms;
using EMCSPhase4_1 = CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1;

namespace Enterprise.Customs.IE.EMCS.Business.Testing.Phase4_1
{
	static class EMCSMessageProcessorTestHelper
	{
		public static string GetStandardIE704Text()
		{
			var ie704 = new EMCSPhase4_1.IE704.Ie704Type()
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.IE",
					MessageRecipient = "NDEA.IE",
					DateOfPreparation = new DateTime(2022, 07, 14),
					TimeOfPreparation = "13:29:08",
					MessageIdentifier = "f4259e39-c0cc-4304-8627-8a1fb056130a",
					CorrelationIdentifier = "00000000000215",
				},
				Body = new EMCSPhase4_1.IE704.BodyType
				{
					GenericRefusalMessage = new EMCSPhase4_1.IE704.GenericRefusalMessageType
					{
						Attributes = new EMCSPhase4_1.IE704.AttributesType
						{
							AdministrativeReferenceCode = "MRN98761234",
							LocalReferenceNumber = "B000222547896254786321",
							SequenceNumber = "5",
						},
						FunctionalError = new Collection<EMCSPhase4_1.IE704.FunctionalErrorType>(new[]
						{
							new EMCSPhase4_1.IE704.FunctionalErrorType { ErrorLocation = "location1", ErrorReason = "reason1", ErrorType = FunctionalErrorCodes.Item12, OriginalAttributeValue = "original value 1" },
							new EMCSPhase4_1.IE704.FunctionalErrorType { ErrorLocation = "location2", ErrorReason = "reason2", ErrorType = FunctionalErrorCodes.Item15, OriginalAttributeValue = "original value 2" },
						}),
					}
				}
			};
			return IEXmlObjectSerializer.Serialize(ie704);
		}

		public static string GetStandardIE801Text()
		{
			var bodyEadEsadCollection = new Collection<EMCSPhase4_1.IE801.BodyEadEsadType>();
			bodyEadEsadCollection.Add(GetBodyEadEsad("1", "SN001", "5", "Shipping Marks 1"));
			bodyEadEsadCollection.Add(GetBodyEadEsad("2", "SN002", "5", "Shipping Marks 1"));
			bodyEadEsadCollection.Add(GetBodyEadEsad("3", "SN003", "0", "Shipping Marks 2"));
			bodyEadEsadCollection.Add(GetBodyEadEsad("4", "SN004", "4", "Shipping Marks 2"));

			var ie801 = new EMCSPhase4_1.IE801.Ie801Type
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.IE",
					MessageRecipient = "NDEA.IE",
					DateOfPreparation = new DateTime(2022, 08, 30),
					TimeOfPreparation = "13:29:08",
					MessageIdentifier = "e3e6f99d-2bc3-4c11-a20f-c7aa391f8573",
				},
				Body = new EMCSPhase4_1.IE801.BodyType
				{
					EadesadContainer = new EMCSPhase4_1.IE801.EadesadContainerType
					{
						ExciseMovement = new EMCSPhase4_1.IE801.ExciseMovementType
						{
							AdministrativeReferenceCode = "MRN1234567",
							DateAndTimeOfValidationOfEadEsad = new DateTime(2022, 08, 30, 13, 30, 00),
						},
						HeaderEadEsad = new EMCSPhase4_1.IE801.HeaderEadEsadType
						{
							SequenceNumber = "1",
							JourneyTime = "H12",
							DestinationTypeCode = DestinationTypeCode.Item1,
							TransportArrangement = TransportArrangement.Item3
						},
						BodyEadEsad = bodyEadEsadCollection,
						EadEsad = new EMCSPhase4_1.IE801.EadEsadType
						{
							LocalReferenceNumber = "B000222547896254786321",
							DateOfDispatch = new DateTime(2022, 8, 30),
							TimeOfDispatch = "13:30:00",
							OriginTypeCode = OriginTypeCode.Item1,
							InvoiceNumber = "1",
							InvoiceDate = new DateTime(2022, 8, 29, 13, 30, 0),
							ImportSad = new Collection<EMCSPhase4_1.IE801.ImportSadType>
							{
								new EMCSPhase4_1.IE801.ImportSadType
								{
									ImportSadNumber = "SAD001"
								},
								new EMCSPhase4_1.IE801.ImportSadType
								{
									ImportSadNumber = "SAD002"
								}
							}
						},
						DispatchImportOffice = new EMCSPhase4_1.IE801.DispatchImportOfficeType
						{
							ReferenceNumber = "DIO001"
						},
						DeliveryPlaceCustomsOffice = new EMCSPhase4_1.IE801.DeliveryPlaceCustomsOfficeType
						{
							ReferenceNumber = "DPCO001"
						},
						CompetentAuthorityDispatchOffice = new EMCSPhase4_1.IE801.CompetentAuthorityDispatchOfficeType
						{
							ReferenceNumber = "CADO001"
						},
						MovementGuarantee = new EMCSPhase4_1.IE801.MovementGuaranteeType
						{
							GuarantorTypeCode = GuarantorTypeCode.Item3,
							GuarantorTrader = new Collection<EMCSPhase4_1.IE801.GuarantorTraderType>
							{
								new EMCSPhase4_1.IE801.GuarantorTraderType
								{
									TraderExciseNumber = "TN001",
									VatNumber = "VN001",
									Language = "IE",
									TraderName = "Guarantor Party 1",
									StreetName = "GP Address",
									StreetNumber = "1",
									City = "Dublin",
									Postcode = "0001"
								}
							}
						},
						TransportMode = new EMCSPhase4_1.IE801.TransportModeType
						{
							TransportModeCode = "4",
							ComplementaryInformation = new EMCSPhase4_1.IE801.LsdComplementaryInformationType
							{
								Language = "EN",
								Value = "CI001"
							}
						},
						ComplementConsigneeTrader = new EMCSPhase4_1.IE801.ComplementConsigneeTraderType
						{
							MemberStateCode = "12",
							SerialNumberOfCertificateOfExemption = "CE001"
						},
						ConsigneeTrader = new EMCSPhase4_1.IE801.ConsigneeTraderType
						{
							Traderid = "TI001",
							Language = "IE",
							TraderName = "Consignee Party 1",
							StreetName = "CP Address",
							StreetNumber = "1",
							City = "Dublin",
							Postcode = "0001"
						},
						ConsignorTrader = new EMCSPhase4_1.IE801.ConsignorTraderType
						{
							TraderExciseNumber = "TN002",
							Language = "IE",
							TraderName = "Consignor Party 1",
							StreetName = "CRP Address",
							StreetNumber = "1",
							City = "Dublin",
							Postcode = "0001"
						},
						PlaceOfDispatchTrader = new EMCSPhase4_1.IE801.PlaceOfDispatchTraderType
						{
							ReferenceOfTaxWarehouse = "RTW001",
							Language = "IE",
							TraderName = "PartyPlaceOfDispatch Party 1",
							StreetName = "PRD Address",
							StreetNumber = "1",
							City = "Dublin",
							Postcode = "0001"
						},
						DeliveryPlaceTrader = new EMCSPhase4_1.IE801.DeliveryPlaceTraderType
						{
							Traderid = "TI002",
							Language = "IE",
							TraderName = "DeliveryPlace Party 1",
							StreetName = "DP Address",
							StreetNumber = "1",
							City = "Dublin",
							Postcode = "0001"
						},
						TransportArrangerTrader = new EMCSPhase4_1.IE801.TransportArrangerTraderType
						{
							VatNumber = "DE002",
							TraderName = "TransportArranger Party 1",
							StreetName = "TAP Address",
							StreetNumber = "1",
							Language = "DE",
							City = "Berlin",
							Postcode = "0001"
						},
						FirstTransporterTrader = new EMCSPhase4_1.IE801.FirstTransporterTraderType
						{
							VatNumber = "DE003",
							TraderName = "FirstTransporter Party 1",
							StreetName = "FTP Address",
							StreetNumber = "1",
							Language = "DE",
							City = "Berlin",
							Postcode = "0001"
						},
						DocumentCertificate = new Collection<EMCSPhase4_1.IE801.DocumentCertificateType>
						{
							new EMCSPhase4_1.IE801.DocumentCertificateType
							{
								DocumentReference = "Ref001",
								DocumentDescription = new EMCSPhase4_1.IE801.LsdDocumentDescriptionType
								{
									Language = "EN",
									Value = "DC001"
								},
								DocumentType = "TP001"
							},
							new EMCSPhase4_1.IE801.DocumentCertificateType
							{
								DocumentReference = "Ref002",
								DocumentDescription = new EMCSPhase4_1.IE801.LsdDocumentDescriptionType
								{
									Language = "EN",
									Value = "DC002"
								},
								DocumentType = "TP002"
							}
						},
						TransportDetails = new Collection<EMCSPhase4_1.IE801.TransportDetailsType>
						{
							new EMCSPhase4_1.IE801.TransportDetailsType
							{
								TransportUnitCode = "1",
								IdentityOfTransportUnits = "CO0001",
								CommercialSealIdentification = "SEAL1",
								ComplementaryInformation = new EMCSPhase4_1.IE801.LsdComplementaryInformationType
								{
									Language = "EN",
									Value = "CI001"
								},
								SealInformation = new EMCSPhase4_1.IE801.LsdSealInformationType
								{
									Language = "EN",
									Value = "SI001"
								}
							},
							new EMCSPhase4_1.IE801.TransportDetailsType
							{
								TransportUnitCode = "2",
								IdentityOfTransportUnits = "CO0002",
								CommercialSealIdentification = "SEAL2",
								ComplementaryInformation = new EMCSPhase4_1.IE801.LsdComplementaryInformationType
								{
									Language = "EN",
									Value = "CI002"
								},
								SealInformation = new EMCSPhase4_1.IE801.LsdSealInformationType
								{
									Language = "EN",
									Value = "SI002"
								}
							}
						}
					}
				}
			};
			return IEXmlObjectSerializer.Serialize(ie801);
		}

		static EMCSPhase4_1.IE801.BodyEadEsadType GetBodyEadEsad(string lineNumber, string sealIdentification, string numberOfPackages, string shippingMarks)
		{
			return new EMCSPhase4_1.IE801.BodyEadEsadType
			{
				BodyRecordUniqueReference = lineNumber,
				ExciseProductCode = "W200",
				CnCode = "22084011",
				FiscalMarkUsedFlag = Flag.Item1,
				FiscalMark = new EMCSPhase4_1.IE801.LsdFiscalMarkType
				{
					Language = "EN",
					Value = "FM001"
				},
				DesignationOfOrigin = new EMCSPhase4_1.IE801.LsdDesignationOfOriginType
				{
					Language = "EN",
					Value = "CN"
				},
				CommercialDescription = new EMCSPhase4_1.IE801.LsdCommercialDescriptionType
				{
					Language = "EN",
					Value = "CD001"
				},
				BrandNameOfProducts = new EMCSPhase4_1.IE801.LsdBrandNameOfProductsType
				{
					Language = "EN",
					Value = "BP001"
				},
				Quantity = 2m,
				GrossMass = 3m,
				NetMass = 4m,
				AlcoholicStrengthByVolumeInPercentage = 5m,
				DegreePlato = 6m,
				SizeOfProducer = "7",
				Density = 8m,
				WineProduct = new EMCSPhase4_1.IE801.WineProductType
				{
					WineGrowingZoneCode = "1",
					WineProductCategory = CategoryOfWineProduct.Item1,
					ThirdCountryOfOrigin = "IE",
					OtherInformation = new EMCSPhase4_1.IE801.LsdOtherInformationType
					{
						Language = "EN",
						Value = "WPOI001"
					},
					WineOperation = new Collection<EMCSPhase4_1.IE801.WineOperationType>
					{
						new EMCSPhase4_1.IE801.WineOperationType
						{
							WineOperationCode = "WO001"
						},
						new EMCSPhase4_1.IE801.WineOperationType
						{
							WineOperationCode = "WO002"
						}
					}
				},
				Package = new Collection<EMCSPhase4_1.IE801.PackageType>
				{
					new EMCSPhase4_1.IE801.PackageType
					{
						KindOfPackages = "CT",
						NumberOfPackages = numberOfPackages,
						CommercialSealIdentification = sealIdentification,
						SealInformation = new EMCSPhase4_1.IE801.LsdSealInformationType
						{
							Language = "EN",
							Value = "SC001"
						},
						ShippingMarks = shippingMarks
					}
				}
			};
		}

		public static string GetStandardIE802Text()
		{
			var ie802 = new EMCSPhase4_1.IE802.Ie802Type
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.IE",
					MessageRecipient = "NDEA.IE",
					DateOfPreparation = new DateTime(2022, 09, 01),
					TimeOfPreparation = "15:30:05",
					MessageIdentifier = "e3e6f99d-2bc3-4c11-a20f-c7aa391f8573",
				},
				Body = new EMCSPhase4_1.IE802.BodyType()
				{
					ReminderMessageForExciseMovement = new EMCSPhase4_1.IE802.ReminderMessageForExciseMovementType()
					{
						ExciseMovement = new EMCSPhase4_1.IE802.ExciseMovementType
						{
							AdministrativeReferenceCode = "MRN1234567",
							SequenceNumber = "1"
						},
						Attributes = new EMCSPhase4_1.IE802.AttributesType
						{
							LimitDateAndTime = new DateTime(2022, 09, 03, 13, 16, 30),
							DateAndTimeOfIssuanceOfReminder = new DateTime(2021, 10, 04, 14, 15, 30),
							ReminderInformation = new EMCSPhase4_1.IE802.LsdReminderInformationType
							{
								Value = "You received this reminder information.",
								Language = "en"
							},
							ReminderMessageType = ReminderMessageType.Item1
						}
					}
				}
			};
			return IEXmlObjectSerializer.Serialize(ie802);
		}

		public static string GetStandardIE803Text()
		{
			var ie803 = new EMCSPhase4_1.IE803.Ie803Type
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.IE",
					MessageRecipient = "NDEA.IE",
					DateOfPreparation = new DateTime(2022, 08, 24),
					TimeOfPreparation = "15:30:08",
					MessageIdentifier = "e3e6f99d-2bc3-4c11-a20f-c7aa391f8573",
				},
				Body = new EMCSPhase4_1.IE803.BodyType
				{
					NotificationOfDivertedEadesad = new EMCSPhase4_1.IE803.NotificationOfDivertedEadesadType
					{
						ExciseNotification = new EMCSPhase4_1.IE803.ExciseNotificationType
						{
							NotificationDateAndTime = new DateTime(2022, 09, 01, 15, 30, 05),
							NotificationType = NotificationType.Item1,
							AdministrativeReferenceCode = "MRN1234567",
							SequenceNumber = "1"
						},
						DownstreamArc = new Collection<EMCSPhase4_1.IE803.DownstreamArcType>
						{
							new EMCSPhase4_1.IE803.DownstreamArcType
							{
								AdministrativeReferenceCode = "20DE66421598431563461"
							}
						}
					}
				}
			};
			return IEXmlObjectSerializer.Serialize(ie803);
		}

		public static string GetStandardIE810Text()
		{
			var ie810 = new EMCSPhase4_1.IE810.Ie810Type
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.IE",
					MessageRecipient = "NDEA.IE",
					DateOfPreparation = new DateTime(2022, 07, 14),
					TimeOfPreparation = "13:29:08",
					MessageIdentifier = "e3e6f99d-2bc3-4c11-a20f-c7aa391f8573",
				},
				Body = new EMCSPhase4_1.IE810.BodyType
				{
					CancellationOfEad = new EMCSPhase4_1.IE810.CancellationOfEadType
					{
						Attributes = new EMCSPhase4_1.IE810.AttributesType { DateAndTimeOfValidationOfCancellation = new DateTime(2022, 07, 14, 13, 29, 08, 000) },
						ExciseMovementEad = new EMCSPhase4_1.IE810.ExciseMovementEadType { AdministrativeReferenceCode = "22DE58500000004684557" },
						Cancellation = new EMCSPhase4_1.IE810.CancellationType { CancellationReasonCode = "0", ComplementaryInformation = new EMCSPhase4_1.IE810.LsdComplementaryInformationType { Language = "de" } }
					}
				}
			};
			return IEXmlObjectSerializer.Serialize(ie810);
		}

		public static string GetStandardIE818Text()
		{
			var reason1 = new EMCSPhase4_1.IE818.UnsatisfactoryReasonType
			{
				UnsatisfactoryReasonCode = "3",
				ComplementaryInformation = new EMCSPhase4_1.IE818.LsdComplementaryInformationType
				{
					Language = "en",
					Value = "Goods were damaged during transport"
				}
			};
			var reason2 = new EMCSPhase4_1.IE818.UnsatisfactoryReasonType
			{
				UnsatisfactoryReasonCode = "2",
				ComplementaryInformation = new EMCSPhase4_1.IE818.LsdComplementaryInformationType
				{
					Language = "en",
					Value = "Quantity is less than what was reported"
				}
			};
			var ie818 = new EMCSPhase4_1.IE818.Ie818Type
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.IE",
					MessageRecipient = "NDEA.IE",
					DateOfPreparation = new DateTime(2022, 09, 10),
					TimeOfPreparation = "13:29:08",
					MessageIdentifier = "e3e6f99d-2bc3-4c11-a20f-c7aa391f8573",
				},
				Body = new EMCSPhase4_1.IE818.BodyType
				{
					AcceptedOrRejectedReportOfReceiptExport = new EMCSPhase4_1.IE818.AcceptedOrRejectedReportOfReceiptExportType
					{
						ExciseMovement = new EMCSPhase4_1.IE818.ExciseMovementType
						{
							AdministrativeReferenceCode = "MRN1234567",
							SequenceNumber = "1"
						},
						ReportOfReceiptExport = new EMCSPhase4_1.IE818.ReportOfReceiptExportType
						{
							GlobalConclusionOfReceipt = GlobalConclusionOfReceipt.Item4
						},
						BodyReportOfReceiptExport = new Collection<EMCSPhase4_1.IE818.BodyReportOfReceiptExportType>
						{
							new EMCSPhase4_1.IE818.BodyReportOfReceiptExportType
							{
								BodyRecordUniqueReference = "1",
								IndicatorOfShortageOrExcess = IndicatorOfShortageOrExcess.S,
								ObservedShortageOrExcess = 10,
								RefusedQuantity = 20,
								UnsatisfactoryReason = new Collection<EMCSPhase4_1.IE818.UnsatisfactoryReasonType>
								{
									reason1,
									reason2
								}
							},
							new EMCSPhase4_1.IE818.BodyReportOfReceiptExportType
							{
								BodyRecordUniqueReference = "2",
								IndicatorOfShortageOrExcess = IndicatorOfShortageOrExcess.E,
								ObservedShortageOrExcess = 30,
								RefusedQuantity = 20,
								UnsatisfactoryReason = new Collection<EMCSPhase4_1.IE818.UnsatisfactoryReasonType>
								{
									reason1
								}
							}
						}
					}
				}
			};
			return IEXmlObjectSerializer.Serialize(ie818);
		}

		public static string GetStandardIE819Text()
		{
			var ie819 = new EMCSPhase4_1.IE819.Ie819Type
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.IE",
					MessageRecipient = "NDEA.IE",
					DateOfPreparation = new DateTime(2022, 09, 14),
					TimeOfPreparation = "13:29:08",
					MessageIdentifier = "e3e6f99d-2bc3-4c11-a20f-c7aa391f8573",
				},
				Body = new EMCSPhase4_1.IE819.BodyType
				{
					AlertOrRejectionOfEadesad = new EMCSPhase4_1.IE819.AlertOrRejectionOfEadesadType
					{
						ExciseMovement = new EMCSPhase4_1.IE819.ExciseMovementType
						{
							AdministrativeReferenceCode = "MRN1234567",
							SequenceNumber = "1"
						},
						AlertOrRejectionOfEadEsadReason = new Collection<EMCSPhase4_1.IE819.AlertOrRejectionOfEadEsadReasonType>
						{
							new EMCSPhase4_1.IE819.AlertOrRejectionOfEadEsadReasonType
							{
								AlertOrRejectionOfMovementReasonCode = "0",
								ComplementaryInformation = new EMCSPhase4_1.IE819.LsdComplementaryInformationType
								{
									Language = "en",
									Value = "Test first information"
								}
							},
							new EMCSPhase4_1.IE819.AlertOrRejectionOfEadEsadReasonType
							{
								AlertOrRejectionOfMovementReasonCode = "3",
								ComplementaryInformation = new EMCSPhase4_1.IE819.LsdComplementaryInformationType
								{
									Language = "en",
									Value = "Test second information"
								}
							}
						}
					}
				}
			};
			return IEXmlObjectSerializer.Serialize(ie819);
		}

		public static string GetStandardIE829Text(bool isMultipleMRN)
		{
			var exciseMovementEads = new Collection<EMCSPhase4_1.IE829.ExciseMovementEadType>
			{
				new EMCSPhase4_1.IE829.ExciseMovementEadType
				{
					AdministrativeReferenceCode = "MRN1234567",
					SequenceNumber = "1"
				}
			};
			if (isMultipleMRN)
			{
				exciseMovementEads.Add(new EMCSPhase4_1.IE829.ExciseMovementEadType
				{
					AdministrativeReferenceCode = "MRN7654321",
					SequenceNumber = "1"
				});
			}

			var ie829 = new EMCSPhase4_1.IE829.Ie829Type
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.IE",
					MessageRecipient = "NDEA.IE",
					DateOfPreparation = new DateTime(2022, 08, 24),
					TimeOfPreparation = "15:30:08",
					MessageIdentifier = "e3e6f99d-2bc3-4c11-a20f-c7aa391f8573",
				},
				Body = new EMCSPhase4_1.IE829.BodyType
				{
					NotificationOfAcceptedExport = new EMCSPhase4_1.IE829.NotificationOfAcceptedExportType
					{
						ExportDeclarationAcceptanceRelease = new EMCSPhase4_1.IE829.ExportDeclarationAcceptanceReleaseType
						{
							DateOfAcceptance = new DateTime(2022, 8, 24),
							ReferenceNumberOfSenderCustomsOffice = "IE003302",
							DocumentReferenceNumber = "20IE12365485421158E2",
						},
						ExciseMovementEad = exciseMovementEads
					}
				}
			};
			return IEXmlObjectSerializer.Serialize(ie829);
		}

		public static string GetStandardIE839Text(bool isMultipleMRN)
		{
			var cEadVals = new Collection<EMCSPhase4_1.IE839.CEadValType>
			{
				new EMCSPhase4_1.IE839.CEadValType
				{
					AdministrativeReferenceCode = "MRN1234567",
					SequenceNumber = "1"
				}
			};
			if (isMultipleMRN)
			{
				cEadVals.Add(new EMCSPhase4_1.IE839.CEadValType
				{
					AdministrativeReferenceCode = "MRN7654321",
					SequenceNumber = "1"
				});
			}

			var ie839 = new EMCSPhase4_1.IE839.Ie839Type
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.IE",
					MessageRecipient = "NDEA.IE",
					DateOfPreparation = new DateTime(2022, 08, 24),
					TimeOfPreparation = "15:30:08",
					MessageIdentifier = "e3e6f99d-2bc3-4c11-a20f-c7aa391f8573",
				},
				Body = new EMCSPhase4_1.IE839.BodyType
				{
					RefusalByCustoms = new EMCSPhase4_1.IE839.RefusalByCustomsType
					{
						Attributes = new EMCSPhase4_1.IE839.AttributesType
						{
							DateAndTimeOfIssuance = new DateTime(2022, 8, 24)
						},
						ExportDeclarationInformation = new EMCSPhase4_1.IE839.ExportDeclarationInformationType
						{
							DocumentReferenceNumber = "20IE12365485421158E2"
						},
						Rejection = new EMCSPhase4_1.IE839.RejectionType
						{
							RejectionDateAndTime = new DateTime(2022, 8, 24),
							RejectionReasonCode = new CustomsRejectionReasonCode()
						},
						CEadVal = cEadVals
					}
				}
			};

			return IEXmlObjectSerializer.Serialize(ie839);
		}

		public static string GetStandardIE917Text()
		{
			var ie917 = new EMCSPhase4_1.IE917.Ie917Type
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.IE",
					MessageRecipient = "NDEA.IE",
					DateOfPreparation = new DateTime(2022, 09, 24),
					TimeOfPreparation = "13:29:08",
					MessageIdentifier = "e3e6f99d-2bc3-4c11-a20f-c7aa391f8573",
				},
				Body = new EMCSPhase4_1.IE917.BodyType
				{
					XmlNegativeAcknowledgement = new EMCSPhase4_1.IE917.XmlNegativeAcknowledgementType
					{
						Attributes = new EMCSPhase4_1.IE917.AttributesType
						{
							AdministrativeReferenceCode = "MRN1234567",
							SequenceNumber = "1"
						},
						XmlError = new Collection<EMCSPhase4_1.IE917.XmlErrorType>
						{
							new EMCSPhase4_1.IE917.XmlErrorType
							{
								ErrorColumnNumber = "368",
								ErrorLineNumber = "1",
								ErrorReason = "cvc-elt.1: Cannot find the declaration of element ie:IE815.",
								ErrorLocation = "Location",
								OriginalAttributeValue = "Original Value"
							},
							new EMCSPhase4_1.IE917.XmlErrorType
							{
								ErrorColumnNumber = "12",
								ErrorLineNumber = "2",
								ErrorReason = "cvc-elt.2: Cannot find the declaration of element ie:IE815.",
								ErrorLocation = "Location 2",
								OriginalAttributeValue = "Original Value 2"
							}
						}
					}
				}
			};
			return IEXmlObjectSerializer.Serialize(ie917);
		}
	}
}

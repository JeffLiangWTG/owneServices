using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC004C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC019C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC022C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC023C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC025C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC028C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC035C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC037C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC043C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC045C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC055C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC056C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC057C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC060C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC140C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC225C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC228C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC229C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC231C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC917C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC928C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR015V;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR054C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR060C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR062C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR064C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR082C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR084C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR862C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR864C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR882C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR884C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	public static class InterchangeProcessorTestHelper
	{
		public static string GetStandardCC004CText(string lrn, string mrn)
		{
			var text = new Cc004CType()
			{
				MessageType = MessageTypes.Cc004C,
				TransitOperation = new TransitOperationType01
				{
					Lrn = lrn,
					Mrn = mrn,
					AmendmentSubmissionDateAndTime = ZDateTime.BrettsBirthday.ToDateTime(),
					AmendmentAcceptanceDateAndTime = ZDateTime.BrettsBirthday.AddDays(1).ToDateTime(),
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "RNALPHN8"
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType20
				{
					IdentificationNumber = "IN928",
					TirHolderIdentificationNumber = "TIRHIN928",
					Name = "BOB THE BUILDER",
					Address = new AddressType07
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				}
			};

			return PopulateNCTSCommonData(text).PopulateDataText();
		}

		public static string GetStandardCC019CText(string mrn)
		{
			var text = new Cc019CType
			{
				MessageType = MessageTypes.Cc019C,
				TransitOperation = new TransitOperationType08
				{
					Mrn = mrn,
					DiscrepanciesNotificationDate = ZDateTime.BrettsBirthday.ToDateTime(),
					DiscrepanciesNotificationText = "Test Discrepancies Notification"
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "REF12345"
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType20
				{
					Address = new AddressType07
					{
						StreetAndNumber = "123 WHERE ST",
						City = "Gotham",
						Country = "US"
					}
				}
			};

			return PopulateNCTSCommonData(text).PopulateDataText();
		}

		public static string GetStandardCC022CText(string mrn)
		{
			var text = new Cc022CType()
			{
				MessageType = MessageTypes.Cc022C,
				TransitOperation = new TransitOperationType09
				{
					Mrn = mrn,
					AmendmentNotificationDateAndTime = new DateTime(2023, 01, 31, 10, 22, 11),
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "RNALPHN8"
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType15
				{
					IdentificationNumber = "IN928",
					TirHolderIdentificationNumber = "TIRHIN928",
					Name = "BOB THE BUILDER",
					Address = new AddressType15
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				},
				FunctionalError = new Collection<FunctionalErrorType01>
				{
					new FunctionalErrorType01
					{
						SequenceNumber = "1",
						ErrorPointer = "EP01",
						ErrorCode = AesNctsP5FunctionalErrorCodes.Item26,
						ErrorReason = "ER0001",
						OriginalAttributeValue = "Original value 1"
					},
					new FunctionalErrorType01
					{
						SequenceNumber = "2",
						ErrorPointer = "EP02",
						ErrorCode = AesNctsP5FunctionalErrorCodes.Item12,
						ErrorReason = "ER0022",
						OriginalAttributeValue = "Previous value 2"
					},
				}
			};

			return PopulateNCTSCommonData(text).PopulateDataText();
		}

		public static string GetStandardCC023CText(string mrn)
		{
			var text = new Cc023CType()
			{
				TransitOperation = new TransitOperationType48
				{
					Mrn = mrn,
					DeclarationAcceptanceDate = ZDateTime.BrettsBirthday.ToDateTime(),
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "REF12345"
				},
				CustomsOfficeOfRecoveryAtDeparture = new CustomsOfficeOfRecoveryAtDepartureType01
				{
					ReferenceNumber = "IE000001"
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType20
				{
					Address = new AddressType07
					{
						StreetAndNumber = "123 WHERE ST",
						City = "Gotham",
						Country = "US"
					}
				},
				GuarantorNotification = new GuarantorNotificationType
				{
					GuarantorNotificationDate = ZDateTime.BrettsBirthday.ToDateTime(),
					GuarantorNotificationText = "Guarantor Notification Text"
				},
				Guarantor = new GuarantorType06
				{
					Name = "BOB THE BUILDER",
					Address = new AddressType16
					{
						City = "CITY",
						Country = CountryCodesCustomsOfficeLists.Ie,
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					},
					IdentificationNumber = "GU025",
				}
			};

			return PopulateNCTSCommonData(text).PopulateDataText();
		}

		internal static Cc025CType GetStandardCC025C(string mrn, string releaseIndicator)
		{
			return new Cc025CType
			{
				MessageType = MessageTypes.Cc025C,
				TransitOperation = new TransitOperationType10
				{
					Mrn = mrn,
					ReleaseDate = ZDateTime.BrettsBirthday.ToDateTime(),
					ReleaseIndicator = releaseIndicator
				},
				CustomsOfficeOfDestinationActual = new CustomsOfficeOfDestinationActualType03
				{
					ReferenceNumber = "RNALPHN8"
				},
				TraderAtDestination = new TraderAtDestinationType03
				{
					IdentificationNumber = "IN025",
				},
				Consignment = new Collection<HouseConsignmentType02>
				{
					new HouseConsignmentType02
					{
						SequenceNumber = "1",
						ReleaseType = "1",
						ConsignmentItem = new Collection<ConsignmentItemType02>
						{
							new ConsignmentItemType02
							{
								ReleaseType = "1",
								Commodity = new CommodityType02
								{
									DescriptionOfGoods = "Goods",
									CusCode = "123456789",
									CommodityCode = new CommodityCodeType06
									{
										CombinedNomenclatureCode = "12",
										HarmonizedSystemSubHeadingCode = "456789",
									},
									DangerousGoods = new Collection<DangerousGoodsType01>
									{
										new DangerousGoodsType01
										{
											SequenceNumber = "1",
											UnNumber = "1234"
										},
										new DangerousGoodsType01
										{
											SequenceNumber = "2",
											UnNumber = "1235"
										}
									},
									GoodsMeasure = new GoodsMeasureType03
									{
										GrossMass = 11.1m,
										NetMass = 10m,
									},
								},
								DeclarationGoodsItemNumber = "1",
								GoodsItemNumber = "1",
								Packaging = new Collection<PackagingType02>
								{
									new PackagingType02
									{
										SequenceNumber = "1",
										NumberOfPackages = "12",
										ShippingMarks = "ADR",
										TypeOfPackages = "PL",
									},
									new PackagingType02
									{
										SequenceNumber = "2",
										NumberOfPackages = "1",
										ShippingMarks = "ADR",
										TypeOfPackages = "CT",
									},
								}
							},
							new ConsignmentItemType02
							{
								ReleaseType = "2",
								Commodity = new CommodityType02
								{
									DescriptionOfGoods = "Goods",
									CusCode = "123456789",
									CommodityCode = new CommodityCodeType06
									{
										CombinedNomenclatureCode = "01",
										HarmonizedSystemSubHeadingCode = "456789",
									},
									DangerousGoods = new Collection<DangerousGoodsType01>
									{
										new DangerousGoodsType01
										{
											SequenceNumber = "1",
											UnNumber = "1234"
										}
									},
									GoodsMeasure = new GoodsMeasureType03
									{
										GrossMass = 25.5m,
										NetMass = 24m,
									},
								},
								DeclarationGoodsItemNumber = "2",
								GoodsItemNumber = "2",
								Packaging = new Collection<PackagingType02>
								{
									new PackagingType02
									{
										SequenceNumber = "1",
										NumberOfPackages = "22",
										ShippingMarks = "ADR",
										TypeOfPackages = "PT",
									},
									new PackagingType02
									{
										SequenceNumber = "2",
										NumberOfPackages = "2",
										ShippingMarks = "ADR",
										TypeOfPackages = "CT",
									},
								}
							},
						}
					},
					new HouseConsignmentType02
					{
						SequenceNumber = "2",
						ReleaseType = "2",
						ConsignmentItem = new Collection<ConsignmentItemType02>
						{
							new ConsignmentItemType02
							{
								ReleaseType = "2",
								Commodity = new CommodityType02
								{
									DescriptionOfGoods = "Goods",
									CusCode = "123456789",
									CommodityCode = new CommodityCodeType06
									{
										CombinedNomenclatureCode = "65",
										HarmonizedSystemSubHeadingCode = "012345",
									},
									DangerousGoods = new Collection<DangerousGoodsType01>
									{
										new DangerousGoodsType01
										{
											SequenceNumber = "1",
											UnNumber = "1234"
										}
									},
									GoodsMeasure = new GoodsMeasureType03
									{
										GrossMass = 35.5m,
										NetMass = 33m,
									},
								},
								DeclarationGoodsItemNumber = "3",
								GoodsItemNumber = "3",
								Packaging = new Collection<PackagingType02>
								{
									new PackagingType02
									{
										SequenceNumber = "1",
										NumberOfPackages = "6",
										ShippingMarks = "ADR",
										TypeOfPackages = "PT",
									},
								}
							},
						}
					}
				}
			};
		}

		public static string GetStandardCC025CText(string mrn, string releaseIndicator)
		{
			var text = GetStandardCC025C(mrn, releaseIndicator);
			return PopulateNCTSCommonData(text).PopulateDataText();
		}

		public static string GetStandardCC028CText(string lrn, string mrn)
		{
			var text = new Cc028CType
			{
				MessageType = MessageTypes.Cc028C,
				TransitOperation = new TransitOperationType11
				{
					Lrn = lrn,
					Mrn = mrn,
					DeclarationAcceptanceDate = ZDateTime.BrettsBirthday.ToDateTime()
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "RNALPHN8"
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType20
				{
					IdentificationNumber = "IN928",
					TirHolderIdentificationNumber = "TIRHIN928",
					Name = "BOB THE BUILDER",
					Address = new AddressType07
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				}
			};
			return PopulateNCTSCommonData(text).PopulateDataText();
		}

		public static string GetStandardCC029CText(string mrn = "19MRNCC055C0123456", string lrn = "LRNCC056C0123456789012") =>
			PopulateNCTSCommonData(CC029CProviderTest.CreateStandardProvider(mrn: mrn, lrn: lrn)).PopulateDataText();

		public static string GetStandardCC035CText(string mrn)
		{
			var text = new Cc035CType
			{
				MessageType = MessageTypes.Cc035C,
				TransitOperation = new TransitOperationType48
				{
					Mrn = mrn,
					DeclarationAcceptanceDate = ZDateTime.BrettsBirthday.ToDateTime(),
				},
				RecoveryNotification = new RecoveryNotificationType
				{
					RecoveryNotificationDate = ZDateTime.BrettsBirthday.ToDateTime(),
					RecoveryNotificationText = "Test Recovery Notification",
					AmountClaimed = 22.22m,
					Currency = "IEC"
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "RNALPHN8"
				},
				CustomsOfficeOfRecoveryAtDeparture = new CustomsOfficeOfRecoveryAtDepartureType01
				{
					ReferenceNumber = "IE000001"
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType20
				{
					IdentificationNumber = "IN928",
					TirHolderIdentificationNumber = "TIRHIN928",
					Name = "BOB THE BUILDER",
					Address = new AddressType07
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				}
			};

			return PopulateNCTSCommonData(text).PopulateDataText();
		}

		public static string GetStandardCC037CText(string grn = "12GRNCC055C012345A678901")
		{
			var cc037c = new Cc037CType
			{
				MessageType = MessageTypes.Cc037C,
				Requester = new RequesterType02
				{
					IdentificationNumber = "IN037",
					Role = "1",
				},
				CustomsOfficeOfGuarantee = new CustomsOfficeOfGuaranteeType02
				{
					ReferenceNumber = "RNCC037C",
				},
				GuaranteeReference = new Collection<GuaranteeReferenceType07>
				{
					new GuaranteeReferenceType07
					{
						SequenceNumber = "1",
						Grn = grn,
						AcceptanceDateAndTime = new DateTime(2023, 01, 31, 10, 22, 11),
						GuaranteeType = "1",
						GuaranteeMonitoringCode = "7",
						GuaranteeQuery = new GuaranteeQueryType
						{
							QueryIdentifier = "1",
							PeriodFromDate = new DateTime(2023, 01, 31, 10, 22, 11),
							PeriodToDate = new DateTime(2023, 02, 28, 9, 8, 7),
						},
						Owner = new OwnerType02
						{
							IdentificationNumber = "IN002",
							Name = "Guarantor 1",
							Address = new AddressType10
							{
								City = "CITY",
								Country = "ES",
								Postcode = "2020",
								StreetAndNumber = "123 WHERE ST"
							},
						},
						Usage = new Collection<UsageType>
						{
							new UsageType
							{
								SequenceNumber = "1",
								Mrn = "19MRNCC055C0123456",
								CoveredAmount = 10,
								Currency = "QWE",
								LockDate = new DateTime(2023, 02, 21, 1, 2, 7),
								ArrivalDateAndTime = new DateTime(2023, 02, 5, 3, 2, 2),
								ReleaseDate = new DateTime(2023, 02, 19, 2, 3, 6),
							},
							new UsageType
							{
								SequenceNumber = "2",
								Mrn = "20MRNCC055C0123456",
								CoveredAmount = 9,
								Currency = "POI",
								LockDate = new DateTime(2023, 03, 15, 1, 2, 7),
								ArrivalDateAndTime = new DateTime(2023, 02, 15, 4, 3, 3),
								ReleaseDate = new DateTime(2023, 04, 9, 2, 3, 6),
							}
						},
						Exposure = new ExposureType
						{
							Exposure = 7,
							ExposureCounter = "1",
							Balance = 4,
							Currency = "YGV"
						},
						Guarantor = new GuarantorType01
						{
							IdentificationNumber = "LKJ321",
							Name = "Guarantor 1",
							Address = new AddressType13
							{
								City = "CITY",
								Country = CountryCodesCustomsOfficeLists.Ie,
								Postcode = "2020",
								StreetAndNumber = "123 WHERE ST"
							},
							ContactPerson = new ContactPersonType01
							{
								Name = "Contact Person 1",
								PhoneNumber = "+123456789",
								EMailAddress = "tes@email.com"
							}
						},
						ComprehensiveGuarantee = new ComprehensiveGuaranteeType
						{
							ReferenceAmount = 7,
							PercentageOfReferenceAmount = "45",
							GuaranteeAmount = 12,
							Currency = "USD",
							NumberOfCertificates = "3",
							ValidityStartDate = new DateTime(2023, 02, 11, 2, 3, 6),
							ValidityEndDate = new DateTime(2023, 02, 15, 2, 4, 6),
							InvalidityReasonCode = "AB8",
							InvalidityReasonText = "Some reason text",
							LiabilityLiberationDate = new DateTime(2023, 05, 11, 2, 4, 6),
							RestrictedUseForSuspendedGoods = Flag.Item1,
							ValidityLimitation = new Collection<ValidityLimitationType>
							{
								new ValidityLimitationType
								{
									SequenceNumber = "1",
									GuaranteeNotValidIn = "AB"
								},
								new ValidityLimitationType
								{
									SequenceNumber = "2",
									GuaranteeNotValidIn = "DE"
								}
							}
						},
						IndividualGuaranteeByGuarantor = new IndividualGuaranteeByGuarantorType
						{   GuaranteeAmount = 8,
							Currency = "EUR",
							CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
							{
								ReferenceNumber = "FD123TYU",
							},
							CustomsOfficeOfDestination = new CustomsOfficeOfDestinationType02
							{
								ReferenceNumber = "UJ321YHN",
							}
						},
						IndividualGuaranteeVoucher = new IndividualGuaranteeVoucherType
						{
							IssueDate = new DateTime(2023, 01, 24, 2, 4, 6),
							ExpiryDate = new DateTime(2023, 01, 28, 2, 4, 8),
							CopyGiven = Flag.Item1,
							TirCarnet = Flag.Item0,
							VoucherAmount = 999,
							Currency = "AUD"
						}
					},
					new GuaranteeReferenceType07
					{
						SequenceNumber = "1",
						Grn = grn,
						AcceptanceDateAndTime = new DateTime(2023, 01, 31, 10, 22, 11),
						GuaranteeType = "2",
						GuaranteeMonitoringCode = "7",
						GuaranteeQuery = new GuaranteeQueryType
						{
							QueryIdentifier = "1",
							PeriodFromDate = new DateTime(2023, 01, 31, 10, 22, 11),
							PeriodToDate = new DateTime(2023, 02, 28, 9, 8, 7),
						},
					},
				},
			};

			return PopulateNCTSCommonData(cc037c).PopulateDataText();
		}

		public static Flag ContainerIndicator { get; set; } = Flag.Item1;
		public static decimal GrossMass { get; set; } = 1584.5m;
		public static string Security { get; set; } = "1";

		internal static Cc043CType GetStandardCC043C(string mrn)
		{
			return new Cc043CType()
			{
				TransitOperation = new TransitOperationType14
				{
					Mrn = mrn,
					Security = Security,
					ReducedDatasetIndicator = Flag.Item0
				},
				CustomsOfficeOfDestinationActual = new CustomsOfficeOfDestinationActualType03
				{
					ReferenceNumber = "RNALPHN9"
				},
				TraderAtDestination = new TraderAtDestinationType03
				{
					IdentificationNumber = "IN043"
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType06
				{
					IdentificationNumber = "IN043",
					TirHolderIdentificationNumber = "TIRHIN043",
					Name = "BOB THE BUILDER",
					Address = new AddressType10
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				},
				Consignment = new ConsignmentType05
				{
					GrossMass = GrossMass,
					ContainerIndicator = ContainerIndicator,
					InlandModeOfTransport = "2",
					TransportEquipment = new Collection<TransportEquipmentType05>
					{
						new TransportEquipmentType05
						{
							SequenceNumber = "1",
							ContainerIdentificationNumber = "1234",
							NumberOfSeals = "4",
							Seal = new Collection<SealType04>
							{
								new SealType04
								{
									SequenceNumber = "1",
									Identifier = "1111",
								},
								new SealType04
								{
									SequenceNumber = "2",
									Identifier = "1222",
								},
								new SealType04
								{
									SequenceNumber = "3",
									Identifier = "1333",
								},
								new SealType04
								{
									SequenceNumber = "4",
									Identifier = "1444",
								},
							}
						},
						new TransportEquipmentType05
						{
							SequenceNumber = "2",
							ContainerIdentificationNumber = "5678",
							NumberOfSeals = "1",
							Seal = new Collection<SealType04>
							{
								new SealType04
								{
									SequenceNumber = "1",
									Identifier = "2111",
								},
							}
						}
					},
					DepartureTransportMeans = new Collection<DepartureTransportMeansType02>
					{
						new DepartureTransportMeansType02
						{
							SequenceNumber = "1",
							IdentificationNumber = "111",
							Nationality = "IE",
							TypeOfIdentification = "21"
						},
						new DepartureTransportMeansType02
						{
							SequenceNumber = "2",
							IdentificationNumber = "222",
							Nationality = "XI",
							TypeOfIdentification = "41"
						}
					},
					SupportingDocument = new Collection<SupportingDocumentType02>
					{
						new SupportingDocumentType02
						{
							SequenceNumber = "1",
							Type = "XYZA",
							ReferenceNumber = "111",
							ComplementOfInformation = "112",
						},
						new SupportingDocumentType02
						{
							SequenceNumber = "2",
							Type = "ABCA",
							ReferenceNumber = "222",
							ComplementOfInformation = "122",
						},
						new SupportingDocumentType02
						{
							SequenceNumber = "3",
							Type = "DEFA",
							ReferenceNumber = "333",
							ComplementOfInformation = "132",
						},
					},
					TransportDocument = new Collection<TransportDocumentType02>
					{
						new TransportDocumentType02
						{
							SequenceNumber = "1",
							ReferenceNumber = "234",
							Type = "PQRB"
						},
						new TransportDocumentType02
						{
							SequenceNumber = "2",
							ReferenceNumber = "678",
							Type = "YZAB"
						}
					},
					AdditionalInformation = new Collection<AdditionalInformationType02>()
					{
						new AdditionalInformationType02
						{
							SequenceNumber = "1",
							Code = "30300",
							Text = "ABC"
						},
						new AdditionalInformationType02
						{
							SequenceNumber = "2",
							Code = "30600",
							Text = "DEF"
						}
					},
					PreviousDocument = new Collection<PreviousDocumentType06>()
					{
						new PreviousDocumentType06
						{
							SequenceNumber = "1",
							Type = "A001",
							ReferenceNumber = "123456",
							ComplementOfInformation = "Information test 1"
						},
						new PreviousDocumentType06
						{
							SequenceNumber = "2",
							Type = "A004",
							ReferenceNumber = "987654",
							ComplementOfInformation = "Information test 2"
						}
					},
					AdditionalReference = new Collection<AdditionalReferenceType03>
					{
						new AdditionalReferenceType03
						{
							SequenceNumber = "1",
							ReferenceNumber = "456",
							Type = "REFC"
						},
						new AdditionalReferenceType03
						{
							SequenceNumber = "2",
							ReferenceNumber = "789",
							Type = "REGC"
						}
					},
					Incident = new Collection<IncidentType04>
					{
						new IncidentType04
						{
							SequenceNumber = "1",
							Code = "2",
							Text = "Test",
							Endorsement = new EndorsementType03
							{
								Date = new DateTime(2023, 8, 22),
								Authority = "Authority",
								Place = "Meath",
								Country = "IE",
							},
							Location = new LocationType02
							{
								QualifierOfIdentification = "U",
								UnLocode = "IEROS",
								Country = "IE"
							}
						},
						new IncidentType04
						{
							SequenceNumber = "2",
							Code = "4",
							Text = "Extra",
							Endorsement = new EndorsementType03
							{
								Date = new DateTime(2023, 8, 23),
								Authority = "Authority 2",
								Place = "Dublin",
								Country = "IE",
							},
							Location = new LocationType02
							{
								QualifierOfIdentification = "U",
								UnLocode = "IEDUB",
								Country = "IE"
							}
						}
					},
					Consignee = new ConsigneeType04
					{
						IdentificationNumber = "C001",
						Name = "Consignee Name",
						Address = new AddressType07
						{
							StreetAndNumber = "89 WHERE ST",
							City = "Dublin",
							Postcode = "D1 289",
							Country = "IE",
						}
					},
					Consignor = new ConsignorType05
					{
						IdentificationNumber = "C002",
						Name = "Consignor Name",
						Address = new AddressType07
						{
							StreetAndNumber = "567 WHERE ST",
							City = "Dublin",
							Postcode = "D1 234",
							Country = "IE",
						}
					},
					HouseConsignment = new Collection<HouseConsignmentType04>
					{
						new HouseConsignmentType04
						{
							SequenceNumber = "1",
							GrossMass = 1250.75m,
							SecurityIndicatorFromExportDeclaration = "1",
							DepartureTransportMeans = new Collection<DepartureTransportMeansType02>
							{
								new DepartureTransportMeansType02
								{
									SequenceNumber = "1",
									TypeOfIdentification = "11",
									IdentificationNumber = "111",
									Nationality = "IE",
								},
								new DepartureTransportMeansType02
								{
									SequenceNumber = "2",
									TypeOfIdentification = "20",
									IdentificationNumber = "777",
									Nationality = "GB",
								},
							},
							SupportingDocument = new Collection<SupportingDocumentType02>
							{
								new SupportingDocumentType02
								{
									SequenceNumber = "1",
									Type = "PQRD",
									ReferenceNumber = "234",
									ComplementOfInformation = "2222",
								},
								new SupportingDocumentType02
								{
									SequenceNumber = "2",
									Type = "YZAD",
									ReferenceNumber = "678",
									ComplementOfInformation = "4444",
								},
							},
							TransportDocument = new Collection<TransportDocumentType02>
							{
								new TransportDocumentType02
								{
									SequenceNumber = "1",
									ReferenceNumber = "234",
									Type = "PQRD"
								},
								new TransportDocumentType02
								{
									SequenceNumber = "2",
									ReferenceNumber = "678",
									Type = "YZAD"
								}
							},
							AdditionalReference = new Collection<AdditionalReferenceType03>
							{
								new AdditionalReferenceType03
								{
									SequenceNumber = "1",
									ReferenceNumber = "456",
									Type = "REFE"
								},
								new AdditionalReferenceType03
								{
									SequenceNumber = "2",
									ReferenceNumber = "789",
									Type = "REGE"
								}
							},
							AdditionalInformation = new Collection<AdditionalInformationType02>
							{
								new AdditionalInformationType02
								{
									SequenceNumber = "1",
									Code = "Code1",
									Text = "Test"
								},
								new AdditionalInformationType02
								{
									SequenceNumber = "2",
									Code = "Code2",
									Text = "Test2"
								}
							},
							ConsignmentItem = new Collection<ConsignmentItemType04>
							{
								new ConsignmentItemType04
								{
									Consignee = new ConsigneeType03
									{
										IdentificationNumber = "1",
									},
									GoodsItemNumber = "1",
									DeclarationGoodsItemNumber = "1",
									DeclarationType = "A1",
									CountryOfDestination = "IE",
									Commodity = new CommodityType08
									{
										CommodityCode = new CommodityCodeType05
										{
											HarmonizedSystemSubHeadingCode = "101023",
											CombinedNomenclatureCode = "A1",
										},
										CusCode = "0018113-5",
										DescriptionOfGoods = "Treated Timber",
										DangerousGoods = new Collection<DangerousGoodsType01>
										{
											new DangerousGoodsType01
											{
												SequenceNumber = "1",
												UnNumber = "4.17"
											},
											new DangerousGoodsType01
											{
												SequenceNumber = "2",
												UnNumber = "3.14"
											}
										},
										GoodsMeasure = new GoodsMeasureType03
										{
											GrossMass = 1250.75m,
											NetMass = 1200m
										}
									},
									Packaging = new Collection<PackagingType02>
									{
										new PackagingType02
										{
											SequenceNumber = "1",
											NumberOfPackages = "1",
											ShippingMarks = "1",
											TypeOfPackages = "PT"
										},
										new PackagingType02
										{
											SequenceNumber = "2",
											NumberOfPackages = "4",
											ShippingMarks = "MARK",
											TypeOfPackages = "CT"
										},
									},
									SupportingDocument = new Collection<SupportingDocumentType02>
									{
										new SupportingDocumentType02
										{
											SequenceNumber = "1",
											Type = "AB12",
											ReferenceNumber = "AA1",
											ComplementOfInformation = "Test 123"
										},
										new SupportingDocumentType02
										{
											SequenceNumber = "2",
											Type = "CD34",
											ReferenceNumber = "BB2",
											ComplementOfInformation = "Test 456"
										}
									},
									TransportDocument = new Collection<TransportDocumentType02>
									{
										new TransportDocumentType02
										{
											SequenceNumber = "1",
											Type = "MO44",
											ReferenceNumber = "DEFG9876",
										}
									},
									AdditionalReference = new Collection<AdditionalReferenceType02>
									{
										new AdditionalReferenceType02
										{
											SequenceNumber = "1",
											Type = "N380",
											ReferenceNumber = "TEST6666"
										},
										new AdditionalReferenceType02
										{
											SequenceNumber = "2",
											Type = "ZZ46",
											ReferenceNumber = "TEST7788"
										}
									},
									AdditionalInformation = new Collection<AdditionalInformationType02>
									{
										new AdditionalInformationType02
										{
											SequenceNumber = "1",
											Code = "TEST1",
											Text = "Sample",
										},
										new AdditionalInformationType02
										{
											SequenceNumber = "2",
											Code = "TEST2",
											Text = "Example",
										}
									}
								}
							}
						}
					}
				}
			};
		}

		public static string GetStandardCC043CText(string mrn)
		{
			var text = GetStandardCC043C(mrn);
			return PopulateNCTSCommonData(text).PopulateDataText();
		}

		public static string GetStandardCC045CText(string mrn)
		{
			var text = new Cc045CType()
			{
				MessageType = MessageTypes.Cc045C,
				TransitOperation = new TransitOperationType16
				{
					Mrn = mrn,
					WriteOffDate = ZDateTime.BrettsBirthday.ToDateTime()
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "RNALPHN8"
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType20
				{
					IdentificationNumber = "IN045",
					TirHolderIdentificationNumber = "TIRHIN045",
					Name = "BOB THE BUILDER",
					Address = new AddressType07
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				}
			};

			return PopulateNCTSCommonData(text).PopulateDataText();
		}

		public static string GetStandardCC055CText(string mrn = "19MRNCC055C0123456", string grn = "12GRNCC055C012345A678901", string invalidCode = "G02", string invalidText = "Guarantee exists, but not valid")
		{
			var cc055c = new Cc055CType
			{
				MessageType = MessageTypes.Cc055C,
				TransitOperation = new TransitOperationType48
				{
					Mrn = mrn,
					DeclarationAcceptanceDate = new DateTime(2023, 01, 31, 10, 22, 11),
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "DEPART01",
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType07
				{
					IdentificationNumber = "IN055",
					Name = "BOB THE BUILDER",
					Address = new AddressType03
					{
						StreetAndNumber = "123 WHERE ST",
						Postcode = "2020",
						City = "CITY",
						Country = "IE",
					},
				},
				GuaranteeReference = new Collection<GuaranteeReferenceType08>
				{
					new GuaranteeReferenceType08
					{
						SequenceNumber = "1",
						Grn = grn,
						InvalidGuaranteeReason = new Collection<InvalidGuaranteeReasonType01>
						{
							new InvalidGuaranteeReasonType01
							{
								SequenceNumber = "1",
								Code = invalidCode,
								Text = invalidText,
							},
							new InvalidGuaranteeReasonType01
							{
								SequenceNumber = "2",
								Code = "G05",
								Text = "Guarantee error",
							},
						}
					},
					new GuaranteeReferenceType08
					{
						SequenceNumber = "2",
						Grn = grn,
						InvalidGuaranteeReason = new Collection<InvalidGuaranteeReasonType01>
						{
							new InvalidGuaranteeReasonType01
							{
								SequenceNumber = "1",
								Code = "G04",
								Text = "Holder of Guarantee is not equal to Holder of Transit procedure in declaration",
							},
						}
					},
				},
			};

			return PopulateNCTSCommonData(cc055c).PopulateDataText();
		}

		public static string GetStandardCC056CText(string lrn = "LRNCC056C0123456789012", string mrn = "19MRNCC056C0123456", string businessRejectionType = "015")
		{
			var cc056c = new Cc056CType
			{
				MessageType = MessageTypes.Cc056C,
				TransitOperation = new TransitOperationType20
				{
					Lrn = lrn,
					Mrn = mrn,
					BusinessRejectionType = businessRejectionType,
					RejectionDateAndTime = new DateTime(2023, 01, 31, 10, 22, 11),
					RejectionCode = "7",
					RejectionReason = "Guarantee not valid for this customs territory",
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "DEPART01",
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType08
				{
					IdentificationNumber = "IN056",
					Name = "BOB THE BUILDER",
					Address = new AddressType07
					{
						StreetAndNumber = "123 WHERE ST",
						Postcode = "2020",
						City = "CITY",
						Country = "IE",
					},
				},
				FunctionalError = new Collection<FunctionalErrorType04>
				{
					new FunctionalErrorType04()
					{
						ErrorCode = AesNctsP5FunctionalErrorCodes.Item12,
						ErrorPointer = "Error Pointer 1",
						ErrorReason = "REASON1",
						OriginalAttributeValue = "Original Attribute Value 1"
					}
				},
			};

			return PopulateNCTSCommonData(cc056c).PopulateDataText();
		}

		public static string GetStandardCC057CText(string mrn)
		{
			var cc057c = new Cc057CType
			{
				MessageType = MessageTypes.Cc057C,
				TransitOperation = new TransitOperationType21
				{
					Mrn = mrn,
					BusinessRejectionType = "RTP",
					RejectionCode = "22",
					RejectionDateAndTime = new DateTime(2023, 02, 12, 12, 34, 56),
					RejectionReason = "Invalid CC057C",
				},
				CustomsOfficeOfDestinationActual = new CustomsOfficeOfDestinationActualType03
				{
					ReferenceNumber = "RNCC057C",
				},
				TraderAtDestination = new TraderAtDestinationType03
				{
					IdentificationNumber = "IDCC057C",
				},
				FunctionalError = new Collection<FunctionalErrorType04>
				{
					new FunctionalErrorType04
					{
						ErrorPointer = "Test Pointer",
						ErrorCode = AesNctsP5FunctionalErrorCodes.Item93,
						ErrorReason = "Test",
						OriginalAttributeValue = "Test Attribute Value",
					},
					new FunctionalErrorType04
					{
						ErrorPointer = "Test Pointer 2",
						ErrorCode = AesNctsP5FunctionalErrorCodes.Item14,
						ErrorReason = "Test 2",
						OriginalAttributeValue = "Test Attribute Value 2",
					},
				},
			};

			return PopulateNCTSCommonData(cc057c).PopulateDataText();
		}

		public static string GetStandardCC060CText(string lrn = "LRNCC060C0123456789012", string mrn = "19MRNCC060C0123456", string notificationType = "0")
		{
			var cc060c = new Cc060CType
			{
				MessageType = MessageTypes.Cc060C,
				TransitOperation = new TransitOperationType22
				{
					Lrn = lrn,
					Mrn = mrn,
					ControlNotificationDateAndTime = new DateTime(2023, 01, 31, 10, 22, 11),
					NotificationType = notificationType,
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "DEPART01",
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType13
				{
					IdentificationNumber = "IN060",
					Name = "BOB THE BUILDER",
					Address = new AddressType07
					{
						StreetAndNumber = "123 WHERE ST",
						Postcode = "2020",
						City = "CITY",
						Country = "IE",
					},
				},
				TypeOfControls = new Collection<TypeOfControlsType>
				{
					new TypeOfControlsType
					{
						SequenceNumber = "1",
						Type = "10",
						Text = "Documentary controls",
					},
					new TypeOfControlsType
					{
						SequenceNumber = "2",
						Type = "50",
						Text = "Another text related to the type of control"
					},
				},
				RequestedDocument = new Collection<RequestedDocumentType>
				{
					new RequestedDocumentType
					{
						SequenceNumber = "1",
						DocumentType = "Y022",
						Description = "Consignor / exporter (AEO certificate number)",
					},
					new RequestedDocumentType
					{
						SequenceNumber = "2",
						DocumentType = "Y029",
						Description = "Other text"
					}
				},
			};

			return PopulateNCTSCommonData(cc060c).PopulateDataText();
		}

		public static string GetStandardCC060C_PhysicalControls_Text(string lrn = "LRNCC060C0123456789012", string mrn = "19MRNCC060C0123456")
		{
			var cc060c = new Cc060CType
			{
				MessageType = MessageTypes.Cc060C,
				TransitOperation = new TransitOperationType22
				{
					Lrn = lrn,
					Mrn = mrn,
					ControlNotificationDateAndTime = new DateTime(2023, 01, 31, 10, 22, 11),
					NotificationType = "0",
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "DEPART01",
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType13
				{
					IdentificationNumber = "IN060",
					Name = "BOB THE BUILDER",
					Address = new AddressType07
					{
						StreetAndNumber = "123 WHERE ST",
						Postcode = "2020",
						City = "CITY",
						Country = "IE",
					},
				},
				TypeOfControls = new Collection<TypeOfControlsType>
				{
					new TypeOfControlsType
					{
						SequenceNumber = "1",
						Type = "40",
						Text = "Physical controls",
					},
					new TypeOfControlsType
					{
						SequenceNumber = "2",
						Type = "50",
						Text = "Some text"
					},
				},
				RequestedDocument = new Collection<RequestedDocumentType>
				{
					new RequestedDocumentType
					{
						SequenceNumber = "1",
						DocumentType = "Y057",
						Description = "Information about the physical inspection",
					}
				},
			};

			return PopulateNCTSCommonData(cc060c).PopulateDataText();
		}

		public static string GetStandardCC060C_OtherControls_Text(string lrn = "LRNCC060C0123456789012", string mrn = "19MRNCC060C0123456")
		{
			var cc060c = new Cc060CType
			{
				MessageType = MessageTypes.Cc060C,
				TransitOperation = new TransitOperationType22
				{
					Lrn = lrn,
					Mrn = mrn,
					ControlNotificationDateAndTime = new DateTime(2023, 01, 31, 10, 22, 11),
					NotificationType = "0",
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "DEPART01",
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType13
				{
					IdentificationNumber = "IN060",
					Name = "BOB THE BUILDER",
					Address = new AddressType07
					{
						StreetAndNumber = "123 WHERE ST",
						Postcode = "2020",
						City = "CITY",
						Country = "IE",
					},
				},
				TypeOfControls = new Collection<TypeOfControlsType>
				{
					new TypeOfControlsType
					{
						SequenceNumber = "1",
						Type = "50",
						Text = "Other controls",
					},
				},
				RequestedDocument = new Collection<RequestedDocumentType>
				{
					new RequestedDocumentType
					{
						SequenceNumber = "1",
						DocumentType = "Y057",
						Description = "Description",
					}
				},
			};

			return PopulateNCTSCommonData(cc060c).PopulateDataText();
		}

		public static ZString GetStandardCC140CText()
		{
			var cc140c = new Cc140CType
			{
				MessageType = MessageTypes.Cc140C,
				TransitOperation = new TransitOperationType23
				{
					Mrn = "21IEDUB11A782454R2",
					RequestOnNonArrivedMovementDate = ZDateTime.BrettsBirthday.ToDateTime(),
					LimitForResponseDate = ZDateTime.BrettsBirthday.AddDays(1).ToDateTime(),
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "IEDUB123",
				},
				CustomsOfficeOfEnquiryAtDeparture = new CustomsOfficeOfEnquiryAtDepartureType01
				{
					ReferenceNumber = "IESNN456",
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType20
				{
					IdentificationNumber = "IN140",
					TirHolderIdentificationNumber = "TIRHIN140",
					Name = "BOB THE BUILDER",
					Address = new AddressType07
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				},
			};
			return PopulateNCTSCommonData(cc140c).PopulateDataText();
		}

		public static string GetStandardCC009CText(Flag decision = Flag.Item1) => PopulateNCTSCommonData(CC009CProviderTest.CreateStandardProvider(decision: decision)).PopulateDataText();

		public static string GetStandardCC051CText() => PopulateNCTSCommonData(CC051CProviderTest.CreateStandardProvider()).PopulateDataText();

		public static string GetStandardCC182CText() => PopulateNCTSCommonData(CC182CProviderTest.CreateStandardProvider()).PopulateDataText();

		public static string GetStandardCC225CText(params (string grn, DateTime? validityDate, DateTime? invalidityDate)[] guarantees)
		{
			var cc225c = new Cc225CType
			{
				MessageType = MessageTypes.Cc225C,
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType01
				{
					IdentificationNumber = "IN231",
				},
				GuaranteeReference = new Collection<GuaranteeReferenceType09>(guarantees.Select(x =>
					new GuaranteeReferenceType09
					{
						SequenceNumber = "1",
						Grn = x.grn,
						Currency = "EUR",
						ValidityDate = x.validityDate,
						InvalidityDate = x.invalidityDate,
						RestrictedUseSuspendedGoods = Flag.Item0,
						CustomsOfficeOfGuarantee = new CustomsOfficeOfGuaranteeType02
						{
							ReferenceNumber = "RNCC229A",
						}
					}).ToArray()),
			};
			return PopulateNCTSCommonData(cc225c).PopulateDataText();
		}

		public static string GetStandardCC225CText() => PopulateNCTSCommonData(CC225CProviderTest.CreateStandardProvider()).PopulateDataText();

		public static string GetStandardCC228CText(string grn1, string grn2, DateTime endDate, string identificationNumber)
		{
			var cc228c = new Cc228CType
			{
				MessageType = MessageTypes.Cc229C,
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType10
				{
					IdentificationNumber = "IN140",
					Name = "BOB THE BUILDER",
					Address = new AddressType07
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				},
				Guarantor = new GuarantorType03
				{
					IdentificationNumber = identificationNumber,
					Name = "Bond",
					Address = new AddressType06
					{
						Postcode = "MI5",
						City = "London",
						Country = CountryCodesCustomsOfficeLists.Gb,
					}
				},
				GuaranteeReference = new Collection<GuaranteeReferenceType10>
				{
					new GuaranteeReferenceType10
					{
						SequenceNumber = "1",
						Currency = "EUR",
						GuaranteeAmount = 123.45m,
						Grn = grn1,
						InvalidityDate = endDate,
						CustomsOfficeOfGuarantee = new CustomsOfficeOfGuaranteeType02
						{
							ReferenceNumber = "RNCC229A",
						}
					},
					new GuaranteeReferenceType10
					{
						SequenceNumber = "2",
						Currency = "EUR",
						GuaranteeAmount = 123.45m,
						Grn = grn2,
						InvalidityDate = endDate,
						CustomsOfficeOfGuarantee = new CustomsOfficeOfGuaranteeType02
						{
							ReferenceNumber = "RNCC229B",
						}
					}
				},
			};
			return PopulateNCTSCommonData(cc228c).PopulateDataText();
		}

		public static string GetStandardCC228CText() => PopulateNCTSCommonData(CC228CProviderTest.CreateStandardProvider()).PopulateDataText();

		public static string GetStandardCC229CText(string grn, DateTime endDate, string identificationNumber)
		{
			var cc229c = new Cc229CType
			{
				MessageType = MessageTypes.Cc229C,
				Guarantor = new GuarantorType04
				{
					IdentificationNumber = identificationNumber,
					Name = "Bond",
					Address = new AddressType16
					{
						StreetAndNumber = "7 Main Street",
						Postcode = "MI5",
						City = "London",
						Country = CountryCodesCustomsOfficeLists.Gb,
					}
				},
				GuaranteeReference = new GuaranteeReferenceType11
				{
					Grn = grn,
					InvalidityDate = endDate,
					CustomsOfficeOfGuarantee = new CustomsOfficeOfGuaranteeType02
					{
						ReferenceNumber = "RNCC229C",
					}
				},
			};
			return PopulateNCTSCommonData(cc229c).PopulateDataText();
		}

		public static string GetStandardCC231CText(string grn, DateTime endDate)
		{
			var cc231c = new Cc231CType
			{
				MessageType = MessageTypes.Cc231C,
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType01
				{
					IdentificationNumber = "IN231"
				},
				GuaranteeReference = new GuaranteeReferenceType12
				{
					Grn = grn,
					InvalidityDate = endDate,
					InvalidityReasonCode = "003",
					InvalidityReasonText = "Invalidity Reason Text",
					CustomsOfficeOfGuarantee = new CustomsOfficeOfGuaranteeType02
					{
						ReferenceNumber = "RNCC231C"
					}
				},
			};
			return PopulateNCTSCommonData(cc231c).PopulateDataText();
		}

		public static string GetStandardCC917CText(string lrn, string mrn)
		{
			var type = new Cc917CType
			{
				MessageType = MessageTypes.Cc917C,
				Header = new HeaderType02
				{
					Lrn = lrn,
					Mrn = mrn
				},
				XmlError = new Collection<XmlErrorType> { new XmlErrorType
				{
					ErrorLineNumber = "1",
					ErrorColumnNumber = "1",
					ErrorCode = XmlErrorCodes.Item12,
					ErrorPointer = "Pointer1",
					ErrorText = "ErrorText1"
				},
				new XmlErrorType
				{
					ErrorLineNumber = "2",
					ErrorColumnNumber = "1",
					ErrorCode = XmlErrorCodes.Item13,
					ErrorPointer = "Pointer2",
					ErrorText = "ErrorText2"
				}
				}
			};
			return PopulateNCTSCommonData(type).PopulateDataText();
		}

		public static string GetStandardCC928CText(string lrn, string referenceNumber)
		{
			var cc928c = new Cc928CType
			{
				MessageType = MessageTypes.Cc928C,
				TransitOperation = new TransitOperationType26
				{
					Lrn = lrn,
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = referenceNumber,
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType20
				{
					IdentificationNumber = "IN928",
					TirHolderIdentificationNumber = "TIRHIN928",
					Name = "BOB THE BUILDER",
					Address = new AddressType07
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					},
				},
			};
			return PopulateNCTSCommonData(cc928c).PopulateDataText();
		}

		public static string GetStandardTR015VText(string lrn = "LRNCC060C0123456789012", string mrn = "19MRNCC060C0123456")
		{
			var tr015v = new Tr015V
			{
				MessageType = nameof(CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_tcl.MessageTypes.Tr015V),
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.DeclarationType101
				{
					DeclarationType = "T1",
					AdditionalDeclarationType = "A",
					Lrn = lrn,
					Mrn = mrn,
					DeclarationAcknowledgmentDate = new DateTime(2023, 01, 31, 10, 22, 11),
					CustomsOfficeOfDeparture = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.CustomsOfficeOfDepartureType02
					{
						ReferenceNumber = "DEPART01",
					},
					CustomsOfficeOfDestinationDeclared = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.CustomsOfficeOfDestinationDeclaredType01
					{
						ReferenceNumber = "DESTIN01",
					},
				},
				HolderOfTheTransitProcedure = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.HolderOfTheTransitProcedureType03
				{
					IdentificationNumber = "INTR015V",
					TirHolderIdentificationNumber = "TIRHINTR015V",
					Name = "BOB THE BUILDER",
					Address = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.AddressType08
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					},
				},
				Representative = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.RepresentativeType01
				{
					IdentificationNumber = "REPINTR015V",
					Status = "1",
				},
			};
			return PopulateNCTSCommonData(tr015v).PopulateDataText();
		}

		public static string GetStandardTR054CText()
		{
			var tr054 = new Tr054C
			{
				MessageType = nameof(CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_tcl.MessageTypes.Tr054C),
				TransitOperation = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.TransitOperationType101
				{
					Mrn = "21IEDU4EX144268149",
					AdviceRequested = "1",
					AdviceRequestDateAndTime = new DateTime(2023, 1, 1, 10, 15, 30)
				},
				CustomsOfficeOfDeparture = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "IE123456"
				},
				HolderOfTheTransitProcedure = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.HolderOfTheTransitProcedureType19
				{
					IdentificationNumber = "IN928",
					TirHolderIdentificationNumber = "TIRHIN928",
					Name = "BOB THE BUILDER",
					Address = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.AddressType17
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				},
			};
			return PopulateNCTSCommonData(tr054).PopulateDataText();
		}

		public static string GetStandardTR060CText(string mrn)
		{
			var text = new Tr060C
			{
				MessageType = nameof(CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_tcl.MessageTypes.Tr060C),
				TraderAtDestination = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.TraderAtDestination
				{
					ReferenceNumber = "AA123456"
				},
				CustomsOfficeOfDestination = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.CustomsOfficeOfDestination
				{
					ReferenceNumber = "RNCC060C",
				},
				TransitOperation = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.TransitOperationType50
				{
					Mrn = mrn,
					ControlNotificationDateAndTime = new DateTime(2025, 04, 30, 10, 22, 11),
					NotificationType = "0",
				},
				TypeOfControls = new Collection<CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.TypeOfControlsType>
				{
					new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.TypeOfControlsType
					{
						SequenceNumber = "1",
						Type = "10",
						Text = "Documentary controls",
					},
					new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.TypeOfControlsType
					{
						SequenceNumber = "2",
						Type = "50",
						Text = "Another text"
					},
				},
				RequestedDocument = new Collection<CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.RequestedDocumentType>
				{
					new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.RequestedDocumentType
					{
						SequenceNumber = "1",
						DocumentType = "Y022",
						Description = "Consignor / exporter (AEO certificate number)",
					},
					new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.RequestedDocumentType
					{
						SequenceNumber = "2",
						DocumentType = "Y029",
						Description = "Other text"
					}
				}
			};
			return PopulateNCTSCommonData(text).PopulateDataText();
		}

		public static string GetStandardTR062CText(string mrn)
		{
			var text = new Tr062C
			{
				MessageType = nameof(CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_tcl.MessageTypes.Tr062C),
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.DeclarationType102
				{
					Mrn = mrn,
					CaseId = "ID123456",
					Remarks = "Remarks"
				}
			};
			return PopulateNCTSCommonData(text).PopulateDataText();
		}

		public static string GetStandardTR064CText(string mrn)
		{
			var text = new Tr064C
			{
				MessageType = nameof(CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_tcl.MessageTypes.Tr064C),
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.DeclarationType102
				{
					Mrn = mrn,
					CaseId = "ID123456",
					Remarks = "Remarks"
				}
			};
			return PopulateNCTSCommonData(text).PopulateDataText();
		}

		public static string GetStandardTR082CText(string mrn, string lrn)
		{
			var text = new Tr082C
			{
				MessageType = nameof(CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_tcl.MessageTypes.Tr082C),
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.DeclarationType105
				{
					Mrn = mrn,
					Lrn = lrn,
					RequestDate = ZDateTime.BrettsBirthday.ToDateTime(),
					DateLimit = ZDateTime.BrettsBirthday.AddMonths(1).ToDateTime(),
				},
				AdditionalInformation = new Collection<CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.AdditionalInformationType101> {
					new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.AdditionalInformationType101
					{
						DocumentType = "Y022",
						DocumentComplementaryInformation = "Info 1"
					},
					new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.AdditionalInformationType101
					{
						DocumentType = "Y029",
						DocumentComplementaryInformation = "Info 2"
					}
				}
			};

			return PopulateNCTSCommonData(text).PopulateDataText();
		}

		public static string GetStandardTR084CText(string mrn = "21IEDU4EX144268149", string lrn = "LRNTR084123456789")
		{
			var text = new Tr084C
			{
				MessageType = nameof(CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_tcl.MessageTypes.Tr084C),
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.DeclarationType105
				{
					Mrn = mrn,
					Lrn = lrn,
					RequestDate = new DateTime(2023, 02, 24, 15, 33, 23),
					DateLimit = new DateTime(2023, 03, 26, 16, 0, 0),
				},
				AdditionalInformation = new Collection<CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.AdditionalInformationType101>
				{
					new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.AdditionalInformationType101
					{
						DocumentType = "Y022",
						DocumentComplementaryInformation = "TR084 add info completementary information",
					},
					new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.AdditionalInformationType101
					{
						DocumentType = "Y024",
						DocumentComplementaryInformation = "TR084 add info completementary information 2",
					},
				},
			};
			return PopulateNCTSCommonData(text).PopulateDataText();
		}

		public static string GetStandardTR864CText(string mrn)
		{
			var text = new Tr864C
			{
				MessageType = nameof(CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_tcl.MessageTypes.Tr864C),
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.DeclarationType104
				{
					Mrn = mrn,
					CaseId = "ID123456",
					InvalidationRequestCancellationReason = "Reason for cancellation"
				}
			};
			return PopulateNCTSCommonData(text).PopulateDataText();
		}

		public static string GetStandardTR862CText(string mrn = "19MRNCC060C0123456")
		{
			var text = new Tr862C
			{
				MessageType = nameof(CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_tcl.MessageTypes.Tr862C),
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.DeclarationType103
				{
					Mrn = mrn,
					CaseId = "Test ID",
					AmendmentRequestCancellationReason = "Test Amendment request cancellation reason"
				}
			};
			return PopulateNCTSCommonData(text).PopulateDataText();
		}

		public static string GetStandardTR882CText(string mrn = "19MRNCC060C0123456")
		{
			var text = new Tr882C
			{
				MessageType = nameof(CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_tcl.MessageTypes.Tr882C),
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.DeclarationType106
				{
					Mrn = mrn,
					CaseId = "Test ID",
					UploadRequestCancellationReason = "Test Upload Request Cancellation Reason"
				}
			};
			return PopulateNCTSCommonData(text).PopulateDataText();
		}

		public static string GetStandardTR884CText(string mrn = "19MRNCC060C0123456")
		{
			var text = new Tr884C
			{
				MessageType = nameof(CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_tcl.MessageTypes.Tr884C),
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.DeclarationType107
				{
					Mrn = mrn,
					CaseId = "Test ID",
					PresentationRequestCancellationReason = "Test Presentation Request Cancellation Reason"
				}
			};
			return PopulateNCTSCommonData(text).PopulateDataText();
		}

		public static T PopulateNCTSCommonData<T>(T xmlObject) where T : IXMLMessageObject
		{
			xmlObject.MessageSender = "MESSAGESENDER";
			xmlObject.MessageRecipient = "MESSAGERECIPIENT";
			xmlObject.PreparationDateAndTime = ZDateTime.BrettsBirthday.ToDateTime();
			xmlObject.MessageIdentification = "MESSAGEIDENTIFICATION";
			xmlObject.CorrelationIdentifier = "CORRELATIONIDENTIFIER";
			return xmlObject;
		}

		public static string PopulateDataText<T>(this T xmlObject) where T : IXMLMessageObject
		{
			var outputText = IEXmlObjectSerializer.Serialize(xmlObject);
			outputText = Regex.Replace(outputText, @"T00:00:00\+\d{1,2}:00", "T00:00:00");

			return outputText;
		}

		public static string GetMailboxItemText(string transactionID, string messageText, string mailboxId = "ce45c655-c780-43be-94f8-69ef936ea871", bool includeResponseWrap = true, bool includeEncoding = true)
		{
			var result = $@"<cr:MailboxItem xmlns:cr=""http://www.ros.ie/schemas/customs/collectresponse/v1"">
	<cr:MailboxId>{mailboxId}</cr:MailboxId>
	<cr:TransactionId>{transactionID}</cr:TransactionId>
	<cr:Message>
{messageText}
	</cr:Message>
</cr:MailboxItem>";
			if (includeResponseWrap)
			{
				result = GetMailboxCollectResponseMessage(result);
			}
			return (!includeResponseWrap && includeEncoding ? @"<?xml version=""1.0"" encoding=""utf-8"" ?>
" : string.Empty) + result;
		}

		public static string GetMailboxCollectResponseMessage(params string[] mailBoxItemMessages)
		{
			return $@"		<mcr:MailboxCollectResponse xmlns:mcr=""http://www.ros.ie/schemas/customs/collectresponse/v1"">
			<mcr:MailboxItemList moremessages=""false"" messagecount=""{mailBoxItemMessages.Length}"">
		{string.Join("\r\n", mailBoxItemMessages)}
			</mcr:MailboxItemList>
		</mcr:MailboxCollectResponse>";
		}
	}
}

using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM415V;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM426;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM457;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM460;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM482;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM862;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM917;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS304;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Business;
using FunctionalErrorType = CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2.AIS_complex.FunctionalErrorType;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public static class AISInterchangeProcessorTestHelper
	{
		public static class Typical
		{
			public const string JobNumber = "B00001000";
			public const string Mrn = "21IEDUB11A782454R2";
			public const string Lrn = "LRN123456789";
			public const string TransactionNumber = "TRAN001";
		}

		public static void CreateCL180ReferenceTestData(BusinessObjectFactory factory)
		{
			var dataGrouping = Core.Constants.CountryCodes.Ireland;
			var codeType = Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL180;

			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(codeType, "Error Codes", dataGrouping);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "13", "Condition violation (Missing)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "52", "Functional violation post downgrade", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();
		}

		public static void CreateFunctionalErrorTypeReferenceTestData(BusinessObjectFactory factory)
		{
			var dataGrouping = Core.Constants.CountryCodes.Ireland;
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ErrorCode;

			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(codeType, "Error Codes", dataGrouping);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "13", "Missing value", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "40", "Element too short", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();
		}

		public static Collection<FunctionalErrorType> CreateFunctionalErrorTypeObjects() => new Collection<FunctionalErrorType>
		{
			new FunctionalErrorType
			{
				ErrorPointer = "ErrorPointer001",
				ErrorType = "13",
				ErrorReason = "ER1",
				ErrorMessage = "Functional Error Message 1",
				OriginalAttributeValue = "Original Attribute Value 1",
			},
			new FunctionalErrorType
			{
				ErrorPointer = "ErrorPointer002",
				ErrorType = "40",
				ErrorReason = "ER2",
				ErrorMessage = "Functional Error Message 2",
				OriginalAttributeValue = "Original Attribute Value 2",
			},
		};

		public const string ExpectedFunctionalErrorInterpretation = UCC5.Testing.AISUCC5InterchangeProcessorTestHelper.ExpectedFunctionalErrorInterpretation;

		public static Ts304 CreateTS304Object(string mrn, DateTime acceptanceDate, string remarks)
		{
			return new Ts304()
			{
				Declaration = new DeclarationType05()
				{
					Mrn = mrn,
					AmendmentAcceptanceDate = new DateOfAcceptanceType()
					{
						DateOfAcceptance = acceptanceDate
					},
					Remarks = remarks
				},
				SupervisingCustomsOffice = new SupervisingcustomofficeType()
				{
					ReferenceNumber = "SCO12345"
				},
				CustomsOfficeLodgement = new MScoType01()
				{
					ReferenceNumber = "LCO12345"
				},
				Declarant = new DeclarantType03()
				{
					IdentificationNumber = "ID2"
				},
			};
		}

		public static string GetAISVersion2_0TS304Text(string mrn, DateTime acceptanceDate, string remarks)
		{
			return IEXmlObjectSerializer.Serialize(CreateTS304Object(mrn, acceptanceDate, remarks));
		}

		public static EDIInterchange CreateAISVersion2_0IM415VInterchange(BusinessObjectFactory factory, string transactionID, string additionalDeclarationType, string lrn, string mrn, ZDateTime declarationAcknowledgementDate)
		{
			return InterchangeProcessorTestHelper.CreateIncomingInterchange(factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.MailboxRequest, GetAISVersion2_0IM415VInterchangeText(transactionID, additionalDeclarationType, lrn, mrn, declarationAcknowledgementDate));
		}

		public static string GetAISVersion2_0IM415VInterchangeText(string transactionID, string additionalDeclarationType, string lrn, string mrn, ZDateTime declarationAcknowledgementDate, string mailboxId = "ce45c655-c780-43be-94f8-69ef936ea871", bool includeResponseWrap = true, bool includeEncoding = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetAISVersion2_0IM415VText(additionalDeclarationType, lrn, mrn, declarationAcknowledgementDate), mailboxId: mailboxId, includeResponseWrap: includeResponseWrap, includeEncoding: includeEncoding);
		}

		public static string GetAISVersion2_0IM415VText(string additionalDeclarationType, string lrn, string mrn, ZDateTime declarationAcknowledgementDate)
		{
			return $@"<q1:IM415V xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <ImportOperation>
    <additionalDeclarationType>{additionalDeclarationType}</additionalDeclarationType>
    <LRN>{lrn}</LRN>
    <MRN>{mrn}</MRN>
    <DeclarationAcknowledgementDate>{declarationAcknowledgementDate.ToISO8601ShortDateString()}</DeclarationAcknowledgementDate>
  </ImportOperation>
</q1:IM415V>";
		}

		public static EDIInterchange CreateAIS_H7V1_IM415VInterchange(BusinessObjectFactory factory, string transactionID, string additionalDeclarationType, string lrn, string mrn, ZDateTime declarationAcknowledgementDate)
		{
			return InterchangeProcessorTestHelper.CreateIncomingInterchange(factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.MailboxRequest, GetAIS_H7V1_IM415VInterchangeText(transactionID, additionalDeclarationType, lrn, mrn, declarationAcknowledgementDate));
		}

		public static string GetAIS_H7V1_IM415VInterchangeText(string transactionID, string additionalDeclarationType, string lrn, string mrn, ZDateTime declarationAcknowledgementDate, string mailboxId = "ce45c655-c780-43be-94f8-69ef936ea871", bool includeResponseWrap = true, bool includeEncoding = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetAIS_H7V1_IM415VText(additionalDeclarationType, lrn, mrn, declarationAcknowledgementDate), mailboxId: mailboxId, includeResponseWrap: includeResponseWrap, includeEncoding: includeEncoding);
		}

		public static string GetAIS_H7V1_IM415VText(string additionalDeclarationType, string lrn, string mrn, ZDateTime declarationAcknowledgementDate)
		{
			var data = new CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM415V.Im415V
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM415V.DeclarationType
				{
					Lrn = lrn,
					Mrn = mrn,
					AdditionalDeclarationType = additionalDeclarationType,
					DeclarationAcknowledgementDate = declarationAcknowledgementDate.ToISO8601ShortDateString()
				}
			};
			return IEXmlObjectSerializer.Serialize(data);
		}

		public static EDIInterchange CreateAISVersion1_0IM416Interchange(BusinessObjectFactory factory, string transactionID)
		{
			return InterchangeProcessorTestHelper.CreateIncomingInterchange(factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.MailboxRequest, GetAISVersion1_0IM416InterchangeText(transactionID));
		}

		public static string GetAISVersion1_0IM416InterchangeText(string transactionID, string mailboxId = "ce45c655-c780-43be-94f8-69ef936ea871", bool includeResponseWrap = true, bool includeEncoding = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetAISVersion1_0IM416Text(), mailboxId: mailboxId, includeResponseWrap: includeResponseWrap, includeEncoding: includeEncoding);
		}

		public static string GetAISVersion1_0IM416Text()
		{
			return $@"<q1:IM416 xmlns:q1=""http://www.ros.ie/schemas/customs/IM416"">
	<q1:Declaration>
		<q1:DeclarationType_1_1>EX</q1:DeclarationType_1_1>
		<q1:AdditionalDeclarationType_1_2>A</q1:AdditionalDeclarationType_1_2>
		<q1:LRN_2_5>LRN001</q1:LRN_2_5>
		<q1:RejectionDate>20230810</q1:RejectionDate>
		<q1:RejectionMotivationText>Rejection Motivation Text</q1:RejectionMotivationText>
		<q1:CustomsOffices>
			<q1:CustomsOfficeLodgement>IEDUB100</q1:CustomsOfficeLodgement>
		</q1:CustomsOffices>
	</q1:Declaration>
</q1:IM416>";
		}

		public static EDIInterchange CreateStandardIE415VInterchange(BusinessObjectFactory factory, string transactionID, string lrn, string mrn)
		{
			return InterchangeProcessorTestHelper.CreateIncomingInterchange(factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.MailboxRequest, GetStandardIE415VInterchangeText(transactionID, lrn, mrn));
		}

		public static string GetStandardIE415VInterchangeText(string transactionID, string lrn, string mrn, string mailboxId = "ce45c655-c780-43be-94f8-69ef936ea871", bool includeResponseWrap = true, bool includeEncoding = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardIE415VText(lrn, mrn), mailboxId: mailboxId, includeResponseWrap: includeResponseWrap, includeEncoding: includeEncoding);
		}

		public static string GetStandardIE415VText(string lrn, string mrn)
		{
			var data = new Im415V
			{
				ImportOperation = new MCciOperationType415V
				{
					AdditionalDeclarationType = "D",
					Lrn = lrn,
					Mrn = mrn,
					DeclarationAcknowledgementDate = new DateTime(2021, 2, 15),
				},
				CustomsOfficeLodgement = new MScoType
				{
					ReferenceNumber = "IEDUB100"
				},
				Declarant = new MDeclarantType
				{
					IdentificationNumber = "IE1414141TA",
					Name = "John Smith",
					Address = new MAddressType01
					{
						StreetAndNumber = "Street1 1234567890"
					}
				},
				Representative = new MRepresentativeType
				{
					IdentificationNumber = "IE1414141TA",
					Status = "2"
				}
			};
			return IEXmlObjectSerializer.Serialize(data);
		}

		public static string GetStandardIM426Text(string lrn, string mrn)
		{
			var data = new Im426()
			{
				ImportOperation = new MCciOperationType42
				{
					Lrn = lrn,
					Mrn = mrn,
					CustomsRegistrationNumber = "21IEDUB11A782454R2",
					DeclarationRegistrationDateAndTime = new DateTime(2023, 8, 17, 14, 38, 2),
					PresentationNotificationDueDate = new DateTime(2023, 5, 20)
				},
				SupervisingCustomsOffice = new MScoType
				{
					ReferenceNumber = "RF111111"
				},
				CustomsOfficeLodgement = new MScoType
				{
					ReferenceNumber = "RF222222"
				},
				Declarant = new MDeclarantType
				{
					IdentificationNumber = "IE1",
				}
			};

			return IEXmlObjectSerializer.Serialize(data);
		}

		public static string GetStandardIM457Text() => IEXmlObjectSerializer.Serialize(new Im457
		{
			ImportOperation = new MCciOperationType30
			{
				Lrn = "LRN001",
				Mrn = "12MRN345CDEFG678R9",
				PresentationNotificationRegistrationDateAndTime = new DateTime(2023, 08, 15, 14, 10, 59),
			},
			CustomsOfficeOfPresentation = new MPcoType { ReferenceNumber = "PCO12345" },
			SupervisingCustomsOffice = new MScoType { ReferenceNumber = "SCO12345" },
			CustomsOfficeLodgement = new MScoType { ReferenceNumber = "LCO12345" },
			Declarant = new MDeclarantType { IdentificationNumber = "ID1" },
			Representative = new MRepresentativeType { IdentificationNumber = "REP001", Status = "0" },
		});

		public static Im460 CreateIM460Object(string lrn, string mrn, string customsRegNumber)
		{
			return new Im460
			{
				ImportOperation = new MCciOperationType13
				{
					Lrn = lrn,
					CustomsRegistrationNumber = customsRegNumber,
					Mrn = mrn,
					NotificationDate = new DateTime(2023, 09, 14),
					NotificationType = "4",
					AnticipatedControlDate = new DateTime(2023, 09, 21),
					Text = "Text"
				},
				CustomsOfficeLodgement = new MScoType
				{
					ReferenceNumber = "IEROS144"
				},
				Declarant = new MDeclarantType
				{
					IdentificationNumber = "IE8989"
				},
				OverallControlType = new OverAllControlsType
				{
					ControlTypeCoded = "Orange",
				},
				TypeOfControls = new Collection<MControlDecisionType>
				{
					new MControlDecisionType
					{
						SequenceNumber = "1",
						Type = "10",
						Remarks = "Test Remarks 1"
					},
					new MControlDecisionType
					{
						SequenceNumber = "2",
						Type = "50",
						Remarks = "Test Remarks 2"
					}
				},
				RequestedDocuments = new Collection<MRequestedDocumentsType02>
				{
					new MRequestedDocumentsType02
					{
						SequenceNumber = "1",
						Type = "Y057",
						CcQualifier = "AB",
						Description = "Please provide",
						ReferenceNumber = "12345"
					},
					new MRequestedDocumentsType02
					{
						SequenceNumber = "2",
						Type = "Y022",
						CcQualifier = "CD",
						Description = "Description",
						ReferenceNumber = "67890"
					},
				},
				GoodsShipment = new Collection<GoodsShipmentItemTypeIm460>
				{
					new GoodsShipmentItemTypeIm460
					{
						GoodsItemNumber = "1",
						ControlType = new Collection<ControlsType>
						{
							new ControlsType
							{
								ControlTypeCoded = "Orange",
								ControlAgency = "Revenue"
							}
						}
					}
				}
			};
		}

		public static string GetStandardIM460Text(string lrn, string mrn, string customsRegNumber)
		{
			return IEXmlObjectSerializer.Serialize(CreateIM460Object(lrn, mrn, customsRegNumber));
		}

		public static string GetStandardIM862Text(string mrn, string reason, string caseID)
		{
			var data = new Im862()
			{
				ImportOperation = new MCciOperationType862
				{
					Mrn = mrn,
					CaseId = caseID,
					AmendmentRequestCancellationReason = reason,
				}
			};

			return IEXmlObjectSerializer.Serialize(data);
		}

		public static string GetStandardIM482Text(string mrn, string lrn)
		{
			var data = new Im482()
			{
				ImportOperation = new MCciOperationType55
				{
					Lrn = lrn,
					Mrn = mrn,
					RequestDate = new DateTime(2023, 8, 11),
					DateLimit = new DateTime(2023, 8, 11),
				},
				DocumentAdditionalInformation = new Collection<DocumentAdditionalInformationType>()
				{
					new DocumentAdditionalInformationType()
					{
						DocumentType = "Z123",
						DocumentComplementaryInformation = "Test1"
					},
					new DocumentAdditionalInformationType()
					{
						DocumentType = "Z234",
						DocumentComplementaryInformation = "Test2"
					}
				}
			};

			return IEXmlObjectSerializer.Serialize(data);
		}

		public static string GetStandardUCC5IM428Text(string mrn = null, string lrn = null)
		{
			var data = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM428.Im428()
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM428.DeclarationType
				{
					Mrn = mrn ?? Typical.Mrn,
					Lrn25 = lrn ?? Typical.Lrn,
					DeclarationType11 = "AB",
					AdditionalDeclarationType12 = "C",
					AcceptanceDate = "20240222",
					ResponseDateLimit = "20240224",
					CustomsOffices = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM428.DeclarationTypeCustomsOffices { CustomsOfficeLodgement = "IE123456" }
				},
				GoodsShipment = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM428.GoodsShipmentType
				{
					GoodsShipmentItem = new Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM428.GoodsShipmentTypeItem>
					{
						new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM428.GoodsShipmentTypeItem { GoodsItemNumber16 = "12345" }
					}
				}
			};
			return IEXmlObjectSerializer.Serialize(data);
		}

		public static string GetStandardUCC5IM429Text(string mrn = null, string lrn = null)
		{
			var data = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM429.Im429()
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM429.DeclarationType
				{
					DeclarationType11 = "AB",
					AdditionalDeclarationType12 = "C",
					Mrn = mrn ?? Typical.Mrn,
					Lrn25 = lrn ?? Typical.Lrn,
					ResponseDateLimit = "20240224",
					Remarks = "Remarks001",
					PreferredPaymentMethod48 = "P",
					CustomsOffices = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM429.DeclarationTypeCustomsOffices { CustomsOfficeLodgement = "IE123456" },
				},
				GoodsShipment = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM429.GoodsShipmentType
				{
					DatesPlaces = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM429.GoodsShipmentTypeDatesPlaces
					{
						CountryDestination58 = "CN",
						AcceptanceDate531 = "20240224",
					},
					GoodsShipmentItem = new Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM429.GoodsShipmentItemType>
					{
						new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM429.GoodsShipmentItemType
						{
							GoodsItemNumber16 = "1",
							DatesPlaces = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM429.GoodsShipmentItemTypeDatesPlaces { CountryDestination58 = "CN" },
							GoodsInformation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM429.GoodsShipmentItemTypeGoodsInformation { GoodsDescription68 = "123" },
							Taxes =  new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.TaxesType
							{
								TaxTotalAmount47 = 1,
								TaxBox43Bis = new Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.TaxBoxType>
								{
									new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.TaxBoxType
									{
										BoxTaxType = "A00",
										BoxTaxBaseUnit = "Q",
										BoxQuantity = 200m,
										BoxAmount = 0m,
										BoxTaxRate = 1m,
										BoxTaxPayableAmount = 2m,
										BoxTaxPaymentMethod = "A"
									}
								}
							},
						},
						new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM429.GoodsShipmentItemType
						{
							GoodsItemNumber16 = "2",
							DatesPlaces = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM429.GoodsShipmentItemTypeDatesPlaces { CountryDestination58 = "CN" },
							GoodsInformation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM429.GoodsShipmentItemTypeGoodsInformation { GoodsDescription68 = "123" },
							Taxes =  new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.TaxesType
							{
								TaxTotalAmount47 = 1,
								TaxBox43Bis = new Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.TaxBoxType>
								{
									new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.TaxBoxType
									{
										BoxTaxType = "C00",
										BoxTaxBaseUnit = "F",
										BoxQuantity = 310m,
										BoxAmount = 0m,
										BoxTaxRate = 1m,
										BoxTaxPayableAmount = 3m,
										BoxTaxPaymentMethod = "C"
									},
									new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.TaxBoxType
									{
										BoxTaxType = "B00",
										BoxTaxBaseUnit = "X",
										BoxQuantity = 0m,
										BoxAmount = 425m,
										BoxTaxRate = 10m,
										BoxTaxPayableAmount = 42.5m,
										BoxTaxPaymentMethod = "B"
									},
								}
							},
						}
					}
				}
			};
			return IEXmlObjectSerializer.Serialize(data);
		}

		public static string GetStandardUCC5IM451Text()
		{
			var data = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM451.Im451()
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM451.DeclarationType()
				{
					DeclarationType11 = "CO",
					AdditionalDeclarationType12 = "A",
					Lrn25 = "LRN123",
					Mrn = "12MRN345CDEFG678R9",
					RejectionReason = "Rejection Reason",
					CustomsOffices = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM451.DeclarationTypeCustomsOffices()
					{
						CustomsOfficeLodgement = "IEDUB123",
					},
					ControlResult = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.ControlsType()
					{
						ControlResultCode = "AB",
						ControlDate = "20240229"
					},
					Remarks = "Remark",
				},
				GoodsShipment = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM451.GoodsShipmentType()
				{
					GoodsShipmentItem = new Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM451.GoodsShipmentItemType>()
					{
						new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM451.GoodsShipmentItemType()
						{
							GoodsItemNumber16 = "1",
							Procedure110 = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.ProcedureType() { },
							GoodsInformation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM451.GoodsShipmentItemTypeGoodsInformation()
							{
								GoodsDescription68 = "2",
							},
						},
					},
				}
			};
			return IEXmlObjectSerializer.Serialize(data);
		}

		public static string GetStandardUCC5IM460Text()
		{
			var data = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM460.Im460()
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM460.DeclarationType()
				{
					Mrn = "12MRN345CDEFG678R9",
					ControlNotificationDate = "202402202359GMT",
					TimeLimitForControl = "202403071437GMT",
					CustomsOffices = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM460.DeclarationTypeCustomsOffices()
					{
						CustomsOfficeLodgement = "OF123456",
					}
				},
				OverallControlType = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM460.OverAllControlsType()
				{
					ControlTypeCoded = "O"
				},
				GoodsShipment = new Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM460.GoodsShipmentItemType>()
				{
					new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM460.GoodsShipmentItemType()
					{
						GoodsItemNumber16 = "1",
						ControlType = new Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM460.ControlsType>()
						{
						new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM460.ControlsType()
							{
								ControlTypeCoded = "O",
								ControlAgency = "Revenue",
							}
						}
					},
					new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM460.GoodsShipmentItemType()
					{
						GoodsItemNumber16 = "2",
						ControlType = new Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM460.ControlsType>() {
							new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM460.ControlsType()
							{
								ControlTypeCoded = "O",
								ControlAgency = "Revenue",
							},
							new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM460.ControlsType()
							{
								ControlTypeCoded = "1",
								ControlAgency = "Revenue1",
							}
						}
					}
				}
			};
			return IEXmlObjectSerializer.Serialize(data);
		}

		public static string GetStandardUCC5IM462Text()
		{
			var data = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM462.Im462()
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM462.DeclarationType()
				{
					Mrn = "12MRN345CDEFG678R9",
					CaseId = "11111111-1111-1111-1111-111111111111",
					AmendReason = "Amend Reason",
				},
			};
			return IEXmlObjectSerializer.Serialize(data);
		}

		public static string GetStandardUCC5IM464Text()
		{
			var data = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM464.Im464()
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM464.DeclarationType()
				{
					Mrn = "12MRN345CDEFG678R9",
					CaseId = "11111111-1111-1111-1111-111111111111",
					Remarks = "Remarks",
				},
			};
			return IEXmlObjectSerializer.Serialize(data);
		}

		public static string GetStandardUCC5IM482Text(string mrn = null, string lrn = null)
		{
			var data = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM482.Im482()
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM482.DeclarationType
				{
					Mrn = mrn ?? Typical.Mrn,
					Lrn25 = lrn ?? Typical.Lrn,
					DateLimit = "20240401",
					RequestDate = "20240401",
				},
				AdditionalInformation = new Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.DocumentAdditionalInformationType>
				{
					new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.DocumentAdditionalInformationType
					{
						DocumentType = "D001",
						DocumentComplementaryInformation = "DocInfo1"
					}
				}
			};
			return IEXmlObjectSerializer.Serialize(data);
		}

		public static string GetStandardUCC5IM484Text(string mrn = null, string lrn = null)
		{
			var data = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM484.Im484()
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM484.DeclarationType
				{
					Mrn = mrn ?? Typical.Mrn,
					Lrn25 = lrn ?? Typical.Lrn,
					DateLimit = "20240401",
					RequestDate = "20240401",
				},
				GoodsShipment = new Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.DocumentAdditionalInformationType>
				{
					new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.DocumentAdditionalInformationType
					{
						DocumentType = "D001",
						DocumentComplementaryInformation = "DocInfo1"
					}
				}
			};
			return IEXmlObjectSerializer.Serialize(data);
		}

		public static string GetStandardUCC5IM862Text()
		{
			var data = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM862.Im862
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM862.DeclarationType
				{
					Mrn = "12MRN345CDEFG678R9",
					CaseId = "11111111-1111-1111-1111-111111111111",
					AmendmentRequestCancellationReason = "Amendment Request Cancellation Reason"
				},
			};

			return IEXmlObjectSerializer.Serialize(data);
		}

		public static string GetStandardUCC5IM864Text()
		{
			var data = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM864.Im864
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM864.DeclarationType
				{
					Mrn = "12MRN345CDEFG678R9",
					CaseId = "11111111-1111-1111-1111-111111111111",
					InvalidationRequestCancellationReason = "Invalidation Request Cancellation Reason"
				},
			};

			return IEXmlObjectSerializer.Serialize(data);
		}

		public static string GetStandardUCC5IM882Text()
		{
			var data = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM882.Im882
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM882.DeclarationType
				{
					Mrn = "12MRN345CDEFG678R9",
					CaseId = "11111111-1111-1111-1111-111111111111",
					DocumentsUploadRequestCancellationReason = "Documents Upload Request Cancellation Reason"
				},
			};

			return IEXmlObjectSerializer.Serialize(data);
		}

		public static string GetStandardUCC5IM884Text()
		{
			var data = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM884.Im884
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM884.DeclarationType
				{
					Mrn = "12MRN345CDEFG678R9",
					CaseId = "11111111-1111-1111-1111-111111111111",
					DocumentsPresentRequestCancellationReason = "Documents Present Request Cancellation Reason"
				},
			};

			return IEXmlObjectSerializer.Serialize(data);
		}

		public static EDIInterchange CreateStandardIM917Interchange(BusinessObjectFactory factory, string transactionID, string mailboxId = "ce45c655-c780-43be-94f8-69ef936ea871")
		{
			return InterchangeProcessorTestHelper.CreateIncomingInterchange(factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.MailboxRequest, GetStandardIM917InterchangeText(transactionID, mailboxId: mailboxId));
		}

		public static EDIInterchange CreateStandardCC917CInterchange_NCTS(BusinessObjectFactory factory, string transactionID, string mailboxId = "ce45c655-c780-43be-94f8-69ef936ea871")
		{
			return InterchangeProcessorTestHelper.CreateIncomingInterchange(factory, EDIInterchange.ApplicationCodes.IECustomsNCTS, CommonInterchangeTypeList.Codes.MailboxRequest, GetStandardCC917CInterchangeText_NCTS(transactionID, mailboxId: mailboxId));
		}

		public static string GetStandardIM917InterchangeText(string transactionID, string mailboxId = "ce45c655-c780-43be-94f8-69ef936ea871", bool includeResponseWrap = true, bool includeEncoding = true) => InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardIM917Text(), mailboxId: mailboxId, includeResponseWrap: includeResponseWrap, includeEncoding: includeEncoding);

		public static string GetStandardCC917CInterchangeText_NCTS(string transactionID, string mailboxId = "ce45c655-c780-43be-94f8-69ef936ea871", bool includeResponseWrap = true, bool includeEncoding = true) => InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardCC917CText_NCTS(), mailboxId: mailboxId, includeResponseWrap: includeResponseWrap, includeEncoding: includeEncoding);

		public static string GetStandardIM917Text()
		{
			var data = new Im917
			{
				XmlNegativeAcknowledgement = new Collection<XmlNegativeAcknowledgement>(new[]
				{
					new XmlNegativeAcknowledgement
					{
						ErrorLineNumber = "1",
						ErrorReason = "cvc-pattern-valid: Value '' is not facet-valid with respect to pattern '.{1,17}' for type 'TraderIdentification_type'.",
						ErrorColumnNumber = "437"
					}
				})
			};

			return IEXmlObjectSerializer.Serialize(data);
		}

		public static string GetStandardCC917CText_NCTS()
		{
			return
				$@"<ns3:CC917C xmlns=""http://www.revenue.ie/rcm/"" xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns3=""http://ncts.dgtaxud.ec"">
	<XMLError xmlns="""" xmlns:ns6=""http://www.revenue.ie/rcm/"">
		<errorLineNumber>1</errorLineNumber>
		<errorColumnNumber>2523</errorColumnNumber>
		<errorText>Invalid content was found</errorText></XMLError>
</ns3:CC917C>";
		}

		public static AISInboundEDIMessage GetAISMailboxMessage(BusinessObjectFactory factory, string jobNumber, string messageText) =>
			GetAISMailboxMessage<AISInboundEDIMessage>(factory, jobNumber, messageText);

		public static AISUCC5InboundEDIMessage GetAISUCC5MailboxMessage(BusinessObjectFactory factory, string jobNumber, string messageText) =>
			GetAISMailboxMessage<AISUCC5InboundEDIMessage>(factory, jobNumber, messageText);

		public static TMessage GetAISMailboxMessage<TMessage>(BusinessObjectFactory factory, string jobNumber = null, string messageText = null) where TMessage : InboundEDIMessage
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = jobNumber ?? Typical.JobNumber;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = factory.New<TMessage>();
			message.EM_LinkedObject = entryHeader;
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(Typical.TransactionNumber, messageText, includeResponseWrap: false);

			return message;
		}
	}
}

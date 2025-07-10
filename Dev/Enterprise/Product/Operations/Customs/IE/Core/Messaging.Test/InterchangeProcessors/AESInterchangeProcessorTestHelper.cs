using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC504C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC509C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC521C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC522C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC525C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC528C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC529C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC531C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC551C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC556C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC557C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC560C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC561C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC571C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC574C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC582C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC599C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC604C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC609C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC628C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC917C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX515V;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX562;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX564;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX582;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX584;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX862;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX864;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX882;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX884;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public static class AESInterchangeProcessorTestHelper
	{
		public static T PopulateAESCommonData<T>(T xmlObject, Enum messageType) where T : IXMLMessageObject
		{
			xmlObject.MessageSender = "MESSAGESENDER";
			xmlObject.MessageRecipient = "MESSAGERECIPIENT";
			xmlObject.PreparationDateAndTime = ZDateTime.BrettsBirthday.ToDateTime();
			xmlObject.MessageIdentification = "MESSAGEIDENTIFICATION";
			xmlObject.CorrelationIdentifier = "CORRELATIONIDENTIFIER";
			if (xmlObject is IAESMessageXmlObject)
			{
				((IAESMessageXmlObject)xmlObject).MessageType = (MessageTypes)messageType;
			}
			else
			{
				((IExternalMessageXmlObject)xmlObject).MessageType = (CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_tcl.MessageTypes)messageType;
			}
			return xmlObject;
		}

		public static string GetStandardAESCC509CMailboxItemText(string transactionID, string invalidationInitiatedByCustoms, string lrn = "LRN123456789", string mrn = "21IEDUB11A782454R2", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardCC509CText(lrn, mrn, invalidationInitiatedByCustoms), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardCC509CText(string lrn, string mrn, string invalidationInitiatedByCustoms)
		{
			var cc509c = new Cc509C
			{
				ExportOperation = new ExportOperationType05
				{
					Mrn = mrn,
					Lrn = lrn,
					InvalidationInitiatedByCustoms = invalidationInitiatedByCustoms
				},
				CustomsOfficeOfExport = new CustomsOfficeOfExportType01
				{
					ReferenceNumber = "REFNO123",
				},
				Declarant = new DeclarantType06()
				{
					IdentificationNumber = "DECID123",
					Name = "BOB THE BUILDER",
				},
				Exporter = new ExporterType03()
				{
					IdentificationNumber = "DECID456",
					Name = "BOB THE BUILDER 2",
					Address = new AddressType03()
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				}
			};

			return PopulateAESCommonData(cc509c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc509C).PopulateDataText();
		}

		public static string GetStandardCC504CMailboxItemText(string transactionID, string lrn = "LRN123456789", string mrn = "21IEDUB11A782454R2", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardCC504CText(lrn, mrn), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardCC504CText(string lrn, string mrn)
		{
			var cc504c = new Cc504C()
			{
				ExportOperation = new ExportOperationType03()
				{
					Mrn = mrn,
					Lrn = lrn,
					AmendmentAcceptanceDateAndTime = new DateTime(2022, 7, 1),
					AmendmentDateAndTime = ZDateTime.BrettsBirthday.ToDateTime(),
				},
				CustomsOfficeOfExport = new CustomsOfficeOfExportType01()
				{
					ReferenceNumber = "REFNO123"
				},
				Exporter = new ExporterType06()
				{
					IdentificationNumber = "EXP123"
				},
				Declarant = new DeclarantType06()
				{
					IdentificationNumber = "DECID123",
					Name = "BOB THE BUILDER",
					Address = new AddressType01()
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				}
			};
			return PopulateAESCommonData(cc504c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc504C).PopulateDataText();
		}

		public static EDIInterchange CreateStandardCC521CInterchange(BusinessObjectFactory factory, string transactionID, string mrn = "MRN123456789", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.CreateIncomingInterchange(factory, EDIInterchange.ApplicationCodes.IECustomsExport, CommonInterchangeTypeList.Codes.MailboxRequest, GetStandardCC521CMailboxItemText(transactionID, mrn, mailboxId, includeResponseWrap: includeResponseWrap));
		}

		public static string GetStandardCC521CMailboxItemText(string transactionID, string mrn = "21IEDUB11A782454R2", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardCC521CText(mrn), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardCC521CText(string mrn)
		{
			var cc521c = new Cc521C()
			{
				ExportOperation = new ExportOperationType11()
				{
					Mrn = mrn,
					DiversionRejectionReasonCode = "11",
					DiversionRejectionText = "Because bad data"
				},
				CustomsOfficeOfExitActual = new CustomsOfficeOfExitActualType02()
				{
					ReferenceNumber = "REFNO123"
				},
				ExitCarrier = new ExitCarrierType06()
				{
					IdentificationNumber = "DECID123",
					ContactPerson = new ContactPersonType02()
					{
						Name = "Con Tact",
						PhoneNumber = "0871234567",
						EMailAddress = "contact@contact.com"
					}
				},
			};

			return PopulateAESCommonData(cc521c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc521C).PopulateDataText();
		}

		public static string GetStandardCC522CMailboxItemText(string transactionID, string mrn = "21IEDUB11A782454R2", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardAESCC522CText(mrn), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardAESCC522CText(string mrn)
		{
			var cc522c = new Cc522C()
			{
				ExportOperation = new ExportOperationType12
				{
					Mrn = mrn,
					ExitRejectionMotivationCode = "B",
					ExitRejectionMotivation = "Exit Rejection Motivation B"
				},
				CustomsOfficeOfExitActual = new CustomsOfficeOfExitActualType02
				{
					ReferenceNumber = "REFNO123"
				},
				ExitCarrier = new ExitCarrierType06
				{
					IdentificationNumber = "EXTID123"
				},
				ControlResult = new ControlResultType06
				{
					Date = ZDateTime.BrettsBirthday.ToDateTime()
				}
			};

			return PopulateAESCommonData(cc522c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc522C).PopulateDataText();
		}

		public static string GetStandardCC525CMailboxItemText(string transactionID, string storingFlag, string mrn = "21IEDUB11A782454R2", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardAESCC525CText(mrn, storingFlag), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardAESCC525CText(string mrn, string storingFlag)
		{
			var cc525c = new Cc525C()
			{
				ExportOperation = new ExportOperationType13
				{
					Mrn = mrn,
					ReleaseDate = ZDateTime.BrettsBirthday.ToDateTime(),
					StoringFlag = storingFlag
				},
				CustomsOfficeOfExitActual = new CustomsOfficeOfExitActualType02
				{
					ReferenceNumber = "REFNO123"
				},
				ExitCarrier = new ExitCarrierType06
				{
					IdentificationNumber = "EXTID123"
				}
			};

			return PopulateAESCommonData(cc525c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc525C).PopulateDataText();
		}

		public static EDIInterchange CreateStandardCC528CInterchange(BusinessObjectFactory factory, string transactionID, string lrn = "LRN123456789", string mrn = "MRN123456789", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			var result = factory.New<EDIInterchange>();
			result.EI_ApplicationCode = "IEE";
			result.EI_InterchangeType = "MBR";
			result.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			result.EI_From = "IECustomsTest";
			result.EI_To = "IECustomsTest";
			result.EI_Status = EDIInterchange.Status.Queued;
			result.EI_BodyText = GetStandardCC528CMailboxItemText(transactionID, lrn, mrn, mailboxId, includeResponseWrap: includeResponseWrap);

			return result;
		}

		public static string GetStandardCC528CMailboxItemText(string transactionID, string lrn = "LRN123456789", string mrn = "21IEDUB11A782454R2", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardCC528CText(lrn, mrn), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardCC528CText(string lrn, string mrn)
		{
			var cc528c = new Cc528C()
			{
				ExportOperation = new ExportOperationType58()
				{
					Lrn = lrn,
					Mrn = mrn,
					DeclarationAcceptanceDate = new DateTime(2022, 6, 9)
				},
				CustomsOfficeOfExport = new CustomsOfficeOfExportType01()
				{
					ReferenceNumber = "REFNO123"
				},
				Declarant = new DeclarantType06()
				{
					IdentificationNumber = "DECID123",
					Name = "BOB THE BUILDER",
					Address = new AddressType01()
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				},
				Representative = new RepresentativeType01()
				{
					IdentificationNumber = "REPID456",
					Status = "1"
				}
			};

			return PopulateAESCommonData(cc528c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc528C).PopulateDataText();
		}

		public static string GetStandardCC529CMailboxItemText(string transactionID, string lrn = "LRN123456789", string mrn = "21IEDUB11A782454R2", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardCC529CText(lrn, mrn), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardCC529CText(string lrn, string mrn)
		{
			var cc529c = new Cc529C()
			{
				ExportOperation = new ExportOperationType53()
				{
					Lrn = lrn,
					Mrn = mrn,
					DeclarationType = "AES",
					AdditionalDeclarationType = "D",
					DeclarationAcceptanceDate = ZDateTime.BrettsBirthday.ToDateTime(),
					ReleaseDate = ZDateTime.BrettsBirthday.ToDateTime().AddDays(1),
					Security = "0"
				},
				CustomsOfficeOfExport = new CustomsOfficeOfExportType01()
				{
					ReferenceNumber = "REFNO123"
				},
				CustomsOfficeOfExitDeclared = new CustomsOfficeOfExitDeclaredType02()
				{
					ReferenceNumber = "COOED123"
				},
				Exporter = new ExporterType06()
				{
					IdentificationNumber = "EXP123"
				},
				Declarant = new DeclarantType10()
				{
					IdentificationNumber = "DECID456",
				},
				ControlResult = new ControlResultType06()
				{
					Date = ZDateTime.BrettsBirthday.ToDateTime().AddDays(2),
				},
				GoodsShipment = new GoodsShipmentType07()
				{
					Consignment = new ConsignmentType07()
					{
						GrossMass = 10,
						LocationOfGoods = new LocationOfGoodsType08()
						{
							TypeOfLocation = "C",
							QualifierOfIdentification = "A"
						}
					},
					GoodsItem = new Collection<GoodsItemType13>
					{
						new GoodsItemType13()
						{
							DeclarationGoodsItemNumber = "1",
							Procedure = new ProcedureType()
							{
								RequestedProcedure = "RE",
								PreviousProcedure = "PR",
							},
							Commodity = new CommodityType06()
							{
								DescriptionOfGoods = "TestGoods001",
								CommodityCode = new CommodityCodeType05()
								{
									HarmonizedSystemSubHeadingCode = "HSSHC1",
									CombinedNomenclatureCode = "01"
								},
								GoodsMeasure = new GoodsMeasureType06()
								{
									GrossMass = 10,
									NetMass = 10
								}
							},
							Packaging = new Collection<PackagingType02>
							{
								new PackagingType02()
								{
									SequenceNumber = "12345",
									TypeOfPackages = "T1"
								}
							}
						}
					}
				}
			};

			return PopulateAESCommonData(cc529c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc529C).PopulateDataText();
		}

		public static string GetStandardAESCC531CText(string lrn, string mrn)
		{
			var cc531c = new Cc531C()
			{
				ExportOperation = new ExportOperationType59()
				{
					Mrn = mrn
				},
				CustomsOfficeOfExport = new CustomsOfficeOfExportType04()
				{
					ReferenceNumber = "REF12345"
				},
				Declarant = new DeclarantType10()
				{
					IdentificationNumber = "DECID123"
				},
				TimerExpiryForSupplementaryDeclaration = new TimerExpiryForSupplementaryDeclarationType()
				{
					LodgementOfSupplementaryDeclarationExpiryDate = ZDateTime.BrettsBirthday.ToDateTime().AddDays(1),
					LodgementOfSupplementaryDeclarationStartDate = ZDateTime.BrettsBirthday.ToDateTime(),
					TimerExpiryInformation = "Test Information 123"
				}
			};

			return PopulateAESCommonData(cc531c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc531C).PopulateDataText();
		}

		public static string GetStandardCC551CMailboxItemText(string transactionID, string mrn = "21IEDUB11A782454R2", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardCC551CText(mrn), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardCC551CText(string mrn)
		{
			var cc551c = new Cc551C()
			{
				ExportOperation = new ExportOperationType55()
				{
					Mrn = mrn,
					OtherThingsToReport = "Test Report"
				},
				CustomsOfficeOfPresentation = new CustomsOfficeOfPresentationType01()
				{
					ReferenceNumber = "REF12345"
				},
				CustomsOfficeOfExport = new CustomsOfficeOfExportType01()
				{
					ReferenceNumber = "REF12345"
				},
				Exporter = new ExporterType06()
				{
					IdentificationNumber = "EXID123",
					Name = "BOB THE BUILDER",
					Address = new AddressType01()
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				},
				Declarant = new DeclarantType10()
				{
					IdentificationNumber = "DECID123",
					Name = "BILL THE BUILDER",
					Address = new AddressType01()
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				},
				Representative = new RepresentativeType05()
				{
					IdentificationNumber = "REPID456",
					Status = "1"
				},
				ControlResult = new ControlResultType07()
				{
					Date = ZDateTime.BrettsBirthday.ToDateTime(),
					Text = "Text of a control result"
				}
			};

			return PopulateAESCommonData(cc551c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc551C).PopulateDataText();
		}

		public static string GetStandardCC556CMailboxItemText(string transactionID, string lrn = "LRN123456789", string mrn = "21IEDUB11A782454R2", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardCC556CText(lrn, mrn), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardCC556CText(string lrn, string mrn)
		{
			var cc556c = new Cc556C()
			{
				MessageType = CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc556C,
				ExportOperation = new ExportOperationType20()
				{
					Lrn = lrn,
					Mrn = mrn,
					BusinessRejectionType = "ERR",
					RejectionDateAndTime = new DateTime(2022, 2, 22, 22, 0, 0),
					RejectionCode = "01",
					RejectionReason = "Test Reason"
				},
				CustomsOfficeOfExport = new CustomsOfficeOfExportType01()
				{
					ReferenceNumber = "REFNO123"
				},
				Declarant = new DeclarantType06()
				{
					IdentificationNumber = "DECID123",
					Name = "BOB THE BUILDER",
					Address = new AddressType01()
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				},
				Representative = new RepresentativeType01()
				{
					IdentificationNumber = "REPID456",
					Status = "1"
				},
				FunctionalError = new Collection<FunctionalErrorType04>
				{
					new FunctionalErrorType04()
					{
						ErrorCode = CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.AesNctsP5FunctionalErrorCodes.Item12,
						ErrorPointer = "Error Pointer 1",
						ErrorReason = "REASON1",
						OriginalAttributeValue = "Original Attribute Value 1"
					}
				},
			};

			return PopulateAESCommonData(cc556c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc556C).PopulateDataText();
		}

		public static string GetStandardAESCC557CMailboxItemText(string transactionID, string lrn = "LRN123456789", string mrn = "21IEDUB11A782454R2", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardAESCC557CText(lrn, mrn), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardAESCC557CText(string lrn, string mrn)
		{
			var cc557c = new Cc557C()
			{
				ExportOperation = new ExportOperationType21()
				{
					Mrn = mrn,
					Lrn = lrn,
					BusinessRejectionType = "557",
					RejectionCode = "10",
					RejectionDateAndTime = new DateTime(2022, 07, 01),
					RejectionReason = "Reason 1"
				},
				CustomsOfficeOfExitActual = new CustomsOfficeOfExitActualType02()
				{
					ReferenceNumber = "REFNO123"
				},
				FunctionalError = new Collection<FunctionalErrorType04>
				{
					new FunctionalErrorType04()
					{
						ErrorCode = CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.AesNctsP5FunctionalErrorCodes.Item12,
						ErrorPointer = "Error Pointer 1",
						ErrorReason = "REASON1",
						OriginalAttributeValue = "Original Attribute Value 1"
					}
				},
			};

			return PopulateAESCommonData(cc557c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc557C).PopulateDataText();
		}

		public static string GetStandardAESCC604CMailboxItemText(string transactionID, string mrn = "21IEDUB11A782454R2", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardAESCC604CText(mrn), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardCC599CMailboxItemText(string transactionID, string exitResult, string lrn = "LRN123456789", string mrn = "21IEDUB11A782454R2", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardCC599CText(exitResult, lrn, mrn), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardCC599CText(string exitResult, string lrn, string mrn)
		{
			var cc599c = new Cc599C()
			{
				ExportOperation = new ExportOperationType53()
				{
					Lrn = lrn,
					Mrn = mrn,
					DeclarationType = "EX",
					AdditionalDeclarationType = "A",
					Security = "0"
				},
				CustomsOfficeOfExport = new CustomsOfficeOfExportType01()
				{
					ReferenceNumber = "REFNO123"
				},
				Exporter = new ExporterType06()
				{
					IdentificationNumber = "EXID123",
					Name = "BOB THE BUILDER",
					Address = new AddressType01()
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				},
				Declarant = new DeclarantType10()
				{
					IdentificationNumber = "DECID123",
					Name = "BOB THE BUILDER",
					Address = new AddressType01()
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				},
				CustomsOfficeOfExitActual = new CustomsOfficeOfExitActualType02
				{
					ReferenceNumber = "REFNO123",
				},
				ExitControlResult = new ExitControlResultType02()
				{
					Code = exitResult,
					ExitDate = new DateTime(2022, 2, 22),
					ExitStoppedDate = new DateTime(2022, 7, 1),
					StateOfSeals = "0"
				},
				CurrencyExchange = new CurrencyExchangeType02()
				{
					ExchangeRate = 1.0m,
					InternalCurrencyUnit = "EUR",
				},
				DeferredPayment = new DeferredPaymentType02()
				{
					DeferredPayment = "0"
				},
				GoodsShipment = new GoodsShipmentType13()
				{
					Consignment = new ConsignmentType12()
					{
						GrossMass = 10,
						LocationOfGoods = new LocationOfGoodsType08()
						{
							TypeOfLocation = "C",
							QualifierOfIdentification = "A"
						}
					},
					GoodsItem = new Collection<GoodsItemType13>
					{
						new GoodsItemType13()
						{
							DeclarationGoodsItemNumber = "1",
							Procedure = new ProcedureType()
							{
								RequestedProcedure = "RE",
								PreviousProcedure = "PR",
							},
							Commodity = new CommodityType06()
							{
								DescriptionOfGoods = "TestGoods001",
								CommodityCode = new CommodityCodeType05()
								{
									HarmonizedSystemSubHeadingCode = "TestC1",
									CombinedNomenclatureCode = "01"
								},
								GoodsMeasure = new GoodsMeasureType06()
								{
									GrossMass = 10,
									NetMass = 10
								}
							},
							Packaging = new Collection<PackagingType02>
							{
								new PackagingType02()
								{
									SequenceNumber = "12345",
									TypeOfPackages = "T1"
								}
							}
						}
					}
				}
			};

			return PopulateAESCommonData(cc599c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc599C).PopulateDataText();
		}

		public static string GetStandardCC571CMailboxItemText(string transactionID, string lrn = "LRN123456789", string mrn = "21IEDUB11A782454R2", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardCC571CText(lrn, mrn), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardCC571CText(string lrn, string mrn)
		{
			var cc571c = new Cc571C()
			{
				ExportOperation = new ExportOperationType25
				{
					Mrn = mrn,
					Lrn = lrn,
					ReExportNotificationRegistrationDate = ZDateTime.BrettsBirthday.ToDateTime()
				},
				CustomsOfficeOfExitDeclared = new CustomsOfficeOfExitDeclaredType02
				{
					ReferenceNumber = "REFNO123"
				},
				Declarant = new DeclarantType01
				{
					IdentificationNumber = "DECID123"
				}
			};

			return PopulateAESCommonData(cc571c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc571C).PopulateDataText();
		}

		public static string GetStandardCC574CMailboxItemText(string transactionID, string mrn = "21IEDUB11A782454R2", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardCC574CText(mrn), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardCC574CText(string mrn)
		{
			var cc574c = new Cc574C()
			{
				ExportOperation = new ExportOperationType56
				{
					Mrn = mrn,
					AmendmentDateAndTime = ZDateTime.BrettsBirthday.ToDateTime(),
					AmendmentAcceptanceDateAndTime = ZDateTime.BrettsBirthday.ToDateTime(),
				},
				CustomsOfficeOfExitDeclared = new CustomsOfficeOfExitDeclaredType02
				{
					ReferenceNumber = "REFNO123"
				},
				Declarant = new DeclarantType01
				{
					IdentificationNumber = "DECID123"
				},
				Representative = new RepresentativeType01
				{
					Status = "1",
					IdentificationNumber = "REPID123"
				},
				Consignment = new ConsignmentType09
				{
					Carrier = new CarrierType02
					{
						IdentificationNumber = "CARID123"
					}
				}
			};

			return PopulateAESCommonData(cc574c, MessageTypes.Cc574C).PopulateDataText();
		}

		public static string GetStandardAESCC604CText(string mrn)
		{
			var cc604c = new Cc604C()
			{
				ExportOperation = new ExportOperationType56
				{
					Mrn = mrn,
					AmendmentDateAndTime = ZDateTime.BrettsBirthday.ToDateTime(),
					AmendmentAcceptanceDateAndTime = ZDateTime.BrettsBirthday.ToDateTime().AddDays(1)
				},
				CustomsOfficeOfExitActual = new CustomsOfficeOfExitActualType02
				{
					ReferenceNumber = "REFNO123"
				},
				Declarant = new DeclarantType01
				{
					IdentificationNumber = "DECID123"
				}
			};

			return PopulateAESCommonData(cc604c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc604C).PopulateDataText();
		}

		public static string GetStandardAESCC609CMailboxItemText(string transactionID, string lrn = "LRN123456789", string mrn = "21IEDUB11A782454R2", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardAESCC609CText(lrn, mrn), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardAESCC609CText(string lrn, string mrn)
		{
			var cc609c = new Cc609C()
			{
				ExportOperation = new ExportOperationType30
				{
					Mrn = mrn,
					InvalidationDecisionDateAndTime = ZDateTime.BrettsBirthday.ToDateTime().AddDays(1),
					InvalidationRequestDateAndTime = ZDateTime.BrettsBirthday.ToDateTime()
				},
				CustomsOfficeOfExit = new CustomsOfficeOfExitType04
				{
					ReferenceNumber = "REFNO123"
				},
				Declarant = new DeclarantType01()
				{
					IdentificationNumber = "DECID123"
				}
			};

			return PopulateAESCommonData(cc609c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc609C).PopulateDataText();
		}

		public static EDIInterchange CreateStandardCC560CInterchange(BusinessObjectFactory factory, string transactionID, string lrn = "LRN123456789", string mrn = "21IEDUB11A782454R2", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			var result = factory.New<EDIInterchange>();
			result.EI_ApplicationCode = "IEE";
			result.EI_InterchangeType = "MBR";
			result.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			result.EI_From = "IECustomsTest";
			result.EI_To = "IECustomsTest";
			result.EI_Status = EDIInterchange.Status.Queued;
			result.EI_BodyText = GetStandardCC560CMailboxItemText(transactionID, lrn, mrn, mailboxId, includeResponseWrap: includeResponseWrap);

			return result;
		}

		public static string GetStandardCC560CMailboxItemText(string transactionID, string lrn = "LRN123456789", string mrn = "21IEDUB11A782454R2", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardCC560CText(lrn, mrn), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardCC560CText(string lrn, string mrn)
		{
			var cc560c = new Cc560C()
			{
				ExportOperation = new ExportOperationType22()
				{
					Lrn = lrn,
					Mrn = mrn,
					ControlNotificationDateAndTime = ZDateTime.BrettsBirthday.ToDateTime(),
					NotificationType = "1",
					AnticipatedControlDate = ZDateTime.BrettsBirthday.ToDateTime(),
					Text = "Ex Op Control Text"
				},
				CustomsOfficeOfExport = new CustomsOfficeOfExportType01()
				{
					ReferenceNumber = "REFNO123"
				},
				Declarant = new DeclarantType10()
				{
					IdentificationNumber = "DECID123",
					Name = "BOB THE BUILDER",
					Address = new AddressType01()
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				},
				TypeOfControls = new Collection<TypeOfControlsType01>()
				{
					new TypeOfControlsType01()
					{
						SequenceNumber = "1",
						Type = "40",
						Text = "Documentary Control Type 1"
					},
					new TypeOfControlsType01()
					{
						SequenceNumber = "2",
						Type = "10",
						Text = "Documentary Control Type 2"
					},
					new TypeOfControlsType01()
					{
						SequenceNumber = "3",
						Type = "50",
						Text = "Documentary Control Type 3"
					}
				},
				RequestedDocument = new Collection<RequestedDocumentType01>()
				{
					new RequestedDocumentType01()
					{
						SequenceNumber = "1",
						DocumentType = "A001",
						Description = "Commercial Invoice"
					},
					new RequestedDocumentType01()
					{
						SequenceNumber = "2",
						DocumentType = "A004",
						Description = "Air waybill"
					}
				}
			};

			return PopulateAESCommonData(cc560c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc560C).PopulateDataText();
		}

		public static string GetStandardCC560C_Type2Text(string lrn, string mrn)
		{
			var cc560c = new Cc560C()
			{
				ExportOperation = new ExportOperationType22()
				{
					Lrn = lrn,
					Mrn = mrn,
					ControlNotificationDateAndTime = ZDateTime.BrettsBirthday.ToDateTime(),
					NotificationType = "2",
					AnticipatedControlDate = ZDateTime.BrettsBirthday.ToDateTime(),
					Text = "Ex Op Control Text"
				},
				CustomsOfficeOfExport = new CustomsOfficeOfExportType01()
				{
					ReferenceNumber = "REFNO123"
				},
				Declarant = new DeclarantType10()
				{
					IdentificationNumber = "DECID123",
					Name = "BOB THE BUILDER",
					Address = new AddressType01()
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				},
				TypeOfControls = new Collection<TypeOfControlsType01>()
				{
					new TypeOfControlsType01()
					{
						SequenceNumber = "1",
						Type = "10",
						Text = "Documentary Control Type 1"
					},
					new TypeOfControlsType01()
					{
						SequenceNumber = "2",
						Type = "10",
						Text = "Documentary Control Type 2"
					},
					new TypeOfControlsType01()
					{
						SequenceNumber = "3",
						Type = "50",
						Text = "Documentary Control Type 3"
					}
				},
				RequestedDocument = new Collection<RequestedDocumentType01>()
				{
					new RequestedDocumentType01()
					{
						SequenceNumber = "1",
						DocumentType = "A001",
						Description = "Commercial Invoice"
					},
					new RequestedDocumentType01()
					{
						SequenceNumber = "2",
						DocumentType = "A004",
						Description = "Air waybill"
					}
				}
			};

			return PopulateAESCommonData(cc560c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc560C).PopulateDataText();
		}

		public static string GetStandardAESCC561CText(string lrn, string mrn)
		{
			var cc561c = new Cc561C()
			{
				ExportOperation = new ExportOperationType23()
				{
					Mrn = mrn,
					ControlNotificationDateAndTime = ZDateTime.BrettsBirthday.ToDateTime()
				},
				CustomsOfficeOfExitActual = new CustomsOfficeOfExitActualType02()
				{
					ReferenceNumber = "REFNO123",
				},
				ExitCarrier = new ExitCarrierType06()
				{
					IdentificationNumber = "DECID123"
				},
				TypeOfControls = new Collection<TypeOfControlsType02>
				{
					new TypeOfControlsType02()
					{
						SequenceNumber = "12345",
						Type = "10",
						Text = "Documentary Control Type 1"
					},
					new TypeOfControlsType02()
					{
						SequenceNumber = "12346",
						Type = "20",
						Text = "Documentary Control Type 2"
					},
					new TypeOfControlsType02()
					{
						SequenceNumber = "12347",
						Type = "50",
						Text = "Documentary Control Type 3"
					}
				}
			};
			return PopulateAESCommonData(cc561c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc561C).PopulateDataText();
		}

		public static string GetStandardAESCC582CText(string lrn, string mrn)
		{
			var cc582c = new Cc582C()
			{
				ExportOperation = new ExportOperationType27()
				{
					Mrn = mrn,
					LimitForResponseDate = ZDateTime.BrettsBirthday.ToDateTime(),
					RequestOnNonExitedExportDate = ZDateTime.BrettsBirthday.ToDateTime()
				},
				CustomsOfficeOfExport = new CustomsOfficeOfExportType01()
				{
					ReferenceNumber = "REFNO123"
				},
				Declarant = new DeclarantType03()
				{
					IdentificationNumber = "DECID123",
					Name = "BOB THE BUILDER",
					Address = new AddressType01()
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				},
				Exporter = new ExporterType01()
				{
					IdentificationNumber = "EXID123",
					Name = "BOB THE BUILDER",
					Address = new AddressType01()
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				}
			};
			return PopulateAESCommonData(cc582c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc582C).PopulateDataText();
		}

		public static string GetStandardAESCC917CMailboxItemText(string transactionID, string lrn = "LRN123456789", string mrn = "21IEDUB11A782454R2", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardAESCC917CText(lrn, mrn), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardAESCC917CText(string lrn, string mrn)
		{
			var cc917c = new Cc917C()
			{
				Header = new HeaderType02
				{
					Lrn = lrn,
					Mrn = mrn,
				},
				XmlError = new Collection<XmlErrorType>()
				{
					new XmlErrorType { ErrorLineNumber = "1", ErrorColumnNumber = "1", ErrorPointer = "p1", ErrorCode = XmlErrorCodes.Item12, ErrorText = "Error Text 1" },
					new XmlErrorType { ErrorLineNumber = "2", ErrorColumnNumber = "2", ErrorPointer = "p2", ErrorCode = XmlErrorCodes.Item50, ErrorText = "Error Text 2" },
					new XmlErrorType { ErrorLineNumber = "3", ErrorColumnNumber = "3", ErrorPointer = "p3", ErrorCode = XmlErrorCodes.Empty, ErrorText = "Error Text 3" },
				}
			};
			return PopulateAESCommonData(cc917c, MessageTypes.Cc917C).PopulateDataText();
		}

		public static EDIInterchange CreateStandardCC917CInterchange_NCTS(BusinessObjectFactory factory, string transactionID, string mailboxId = "ce45c655-c780-43be-94f8-69ef936ea871")
		{
			return InterchangeProcessorTestHelper.CreateIncomingInterchange(factory, EDIInterchange.ApplicationCodes.IECustomsNCTS, CommonInterchangeTypeList.Codes.MailboxRequest, GetStandardCC917CInterchangeText_NCTS(transactionID, mailboxId: mailboxId));
		}

		public static string GetStandardCC917CInterchangeText_NCTS(string transactionID, string mailboxId = "ce45c655-c780-43be-94f8-69ef936ea871", bool includeResponseWrap = true, bool includeEncoding = true) => InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardCC917CText_NCTS(), mailboxId: mailboxId, includeResponseWrap: includeResponseWrap, includeEncoding: includeEncoding);

		public static string GetStandardEX562MailboxItemText(string transactionID, string mrn = "21IEDUB11A782454R2", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardEX562Text(mrn), includeResponseWrap: includeResponseWrap);
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

		public static string GetStandardEX515VMailboxItemText(string transactionID, string lrn = "LRN123456789", string mrn = "21IEDUB11A782454R2", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardEX515VText(lrn, mrn), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardEX515VText(string lrn, string mrn)
		{
			var ex515v = new Ex515V()
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.DeclarationType7
				{
					Lrn = lrn,
					Mrn = mrn,
					DeclarationType = "AES",
					AdditionalDeclarationType = "D",
					DeclarationAcknowledgementDate = new DateTime(2020, 12, 22),
					CustomsOffices = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.CustomsOfficesType
					{
						CustomsOfficeOfPresentation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.CustomsOfficeOfPresentationType01
						{
							ReferenceNumber = "REFNO123"
						},
						CustomsOfficeOfExport = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.CustomsOfficeOfExportType01
						{
							ReferenceNumber = "REFNO456",
						},
						CustomsOfficeOfExitDeclared = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.CustomsOfficeOfExitDeclaredType02
						{
							ReferenceNumber = "REFNO789"
						}
					},
					Parties = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.PartiesExportDeclarationType01
					{
						Declarant = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.DeclarantType07
						{
							IdentificationNumber = "DEID123",
							Name = "BOB THE BUILDER",
							Address = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.lowLevel_ctypes.AddressType02
							{
								City = "CITY",
								Country = "IE",
								Postcode = "2020",
								StreetAndNumber = "123 WHERE ST"
							}
						},
						Representative = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.RepresentativeType01
						{
							IdentificationNumber = "REID123",
							Status = "1"
						}
					}
				}
			};

			return PopulateAESCommonData(ex515v, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_tcl.MessageTypes.Ex515V).PopulateDataText();
		}

		public static string GetStandardEX562Text(string mrn)
		{
			var ex562 = new Ex562()
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.DeclarationType()
				{
					Mrn = mrn,
					CaseId = "CASE ID",
					Remarks = "REMARKS"
				},
			};
			return PopulateAESCommonData(ex562, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_tcl.MessageTypes.Ex562).PopulateDataText();
		}

		public static string GetStandardEX564MailboxItemText(string transactionID, string mrn = "21IEDUB11A782454R2", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardEX564Text(mrn), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardEX564Text(string mrn)
		{
			var ex564 = new Ex564()
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.DeclarationType()
				{
					Mrn = mrn,
					CaseId = "CASE ID",
					Remarks = "REMARKS"
				},
			};
			return PopulateAESCommonData(ex564, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_tcl.MessageTypes.Ex564).PopulateDataText();
		}

		public static string GetStandardEX584Text(string lrn, string mrn, int addInfoNumber = 2)
		{
			//TODO: Replace with valid sample
			var ex584 = new Ex584()
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.DeclarationType1
				{
					Mrn = mrn,
					Lrn = lrn,
					RequestDate = new DateTime(2022, 08, 03),
					DateLimit = new DateTime(2022, 08, 10),
				},
				GoodsShipment = new Collection<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.DocumentAdditionalInformationType>()
			};

			for (var idx = 1; idx <= addInfoNumber; idx++)
			{
				ex584.GoodsShipment.Add(new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.DocumentAdditionalInformationType
				{
					DocumentType = "D" + idx.ToString("D2"),
					DocumentComplementaryInformation = "Document of type " + idx.ToString("D2")
				});
			}
			return PopulateAESCommonData(ex584, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_tcl.MessageTypes.Ex584).PopulateDataText();
		}

		public static string GetStandardEX862Text(string mrn)
		{
			var ex862 = new Ex862
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.DeclarationType3
				{
					Mrn = mrn,
					CaseId = "CASEID",
					AmendmentRequestCancellationReason = "REASON",
				},
			};
			return PopulateAESCommonData(ex862, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_tcl.MessageTypes.Ex862).PopulateDataText();
		}

		public static string GetStandardEX582MailboxItemText(string transactionID, string lrn = "LRN123456789", string mrn = "21IEDUB11A782454R2", string mailboxId = "89918717-161F-45F5-BF3E-D100AC11B8F9", bool includeResponseWrap = true)
		{
			return InterchangeProcessorTestHelper.GetMailboxItemText(transactionID, GetStandardEX582Text(lrn, mrn), includeResponseWrap: includeResponseWrap);
		}

		public static string GetStandardEX582Text(string lrn, string mrn)
		{
			var ex582 = new Ex582()
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.DeclarationType1
				{
					Mrn = mrn,
					Lrn = lrn,
					RequestDate = new DateTime(2020, 12, 22),
					DateLimit = new DateTime(2022, 07, 29),
				},
				AdditionalInformation = new Collection<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.DocumentAdditionalInformationType>()
				{
					new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.DocumentAdditionalInformationType()
					{
						DocumentType = "Z740",
						DocumentComplementaryInformation = "Info123"
					},
					new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.DocumentAdditionalInformationType()
					{
						DocumentType = "Z750",
						DocumentComplementaryInformation = "Info456"
					}
				}
			};
			return PopulateAESCommonData(ex582, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_tcl.MessageTypes.Ex582).PopulateDataText();
		}

		public static string GetStandardEX864Text(string mrn)
		{
			var ex864 = new Ex864
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.DeclarationType4
				{
					Mrn = mrn,
					CaseId = "CASEID",
					InvalidationRequestCancellationReason = "REASON",
				},
			};
			return PopulateAESCommonData(ex864, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_tcl.MessageTypes.Ex864).PopulateDataText();
		}

		public static string GetStandardEX882Text(string mrn)
		{
			var ex882 = new Ex882
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.DeclarationType5
				{
					Mrn = mrn,
					CaseId = "ID01",
					DocumentsUploadRequestCancellationReason = "Reason"
				}
			};
			return PopulateAESCommonData(ex882, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_tcl.MessageTypes.Ex882).PopulateDataText();
		}

		public static string GetStandardEX884Text(string mrn)
		{
			var ex884 = new Ex884
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_ctypes.DeclarationType6
				{
					Mrn = mrn,
					CaseId = "ID01",
					DocumentsPresentRequestCancellationReason = "Reason"
				}
			};
			return PopulateAESCommonData(ex884, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.AES_tcl.MessageTypes.Ex884).PopulateDataText();
		}

		public static string GetStandardCC628CText(string lrn, string mrn)
		{
			var cc628c = new Cc628C()
			{
				ExportOperation = new ExportOperationType58()
				{
					Lrn = lrn,
					Mrn = mrn,
					DeclarationAcceptanceDate = new DateTime(2022, 6, 9)
				},
				CustomsOfficeOfExitDeclared = new CustomsOfficeOfExitDeclaredType02
				{
					ReferenceNumber = "REFNO123"
				},
				Declarant = new DeclarantType01()
				{
					IdentificationNumber = "DECID123",
				},
				Representative = new RepresentativeType01()
				{
					IdentificationNumber = "REPID456",
					Status = "1"
				}
			};

			return PopulateAESCommonData(cc628c, CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl.MessageTypes.Cc628C).PopulateDataText();
		}
	}
}

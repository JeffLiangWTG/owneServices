using CargoWise.Customs.FR.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	public class DeltaIEMessageBuilderManagerTest : TestCaseWithFactory
	{
		ZString PrepareDeclarationAndGetMessage(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var representative = Factory.New<OrgHeader>();
			representative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			representative.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			declaration.JE_OA_Representative = representative.MainAddress.PK;
			var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_SubStyle = "D";
			var goodsLocation = Factory.New<EU.Business.CusGoodsLocation>();
			goodsLocation.Parent = cusEntryInstruction;
			goodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.EntryInstruction;
			goodsLocation.CGL_Type = "F";
			goodsLocation.CGL_Qualifier = "C";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;
			entryHeader.CorrelationID = "LRN12345";
			entryHeader.CRN = "CRN12345";
			entryHeader.MRN = "MRN12345";
			entryHeader.CH_EntrySubmittedDate = new ZDateTime(2020, 01, 01, 12, 30, 00);
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			var mergeLine = entryHeader.MergedLines.AddNew();
			mergeLine.CL_InvoiceAmount = 123;
			declaration.JE_DeclarantType = "DIR";
			var sendingObject = new DeltaIEJobDeclarationMessageSendingObject(entryHeader);
			sendingObject.MessageType = messageType;
			sendingObject.VOCReason = "VOCReason";
			sendingObject.ChangeAcknowledgementIndicator = "MOTIV";
			var manager = new DeltaIEMessageBuilderManager();
			return manager.NewMessageBuilder(sendingObject).GetMessage();
		}

		[TestDate(2023, 01, 15)]
		public void TestGetCC415Message()
		{
			var message = PrepareDeclarationAndGetMessage(DeltaIESendMessageSubTypeList.Codes.ImportDeclaration);
			var expectedMessage = @"{
  ""ImportOperation"": {
    ""LRN"": ""LRN12345"",
    ""declarationType"": ""FR"",
    ""additionalDeclarationType"": ""D"",
    ""presentationNotificationEstimatedDateAndTime"": ""2023-01-15T00:00:00"",
    ""languageCode"": ""FR""
  },
  ""CustomsOfficeOfPresentation"": {
    ""referenceNumber"": """"
  },
  ""Declarant"": {
    ""name"": ""EDI CUSTOMS BROKERS""
  },
  ""Representative"": {
    ""identificationNumber"": ""FR12345678900001"",
    ""status"": ""2""
  },
  ""CurrencyExchange"": {
    ""internalCurrencyUnit"": ""EUR""
  },
  ""GoodsShipment"": [
    {
      ""sequenceNumber"": ""1"",
      ""invoiceCurrency"": ""EUR"",
      ""exchangeRate"": 1.0,
      ""AdditionalInformation"": [
        {
          ""sequenceNumber"": ""1"",
          ""code"": ""A0010"",
          ""ccQualifier"": ""FR""
        }
      ],
      ""Consignment"": {
        ""containerIndicator"": ""0"",
        ""LocationOfGoods"": {
          ""typeOfLocation"": ""F"",
          ""qualifierOfIdentification"": ""C""
        }
      },
      ""GoodsShipmentItem"": [
        {
          ""sequenceNumber"": ""0"",
          ""declarationGoodsItemNumber"": ""0"",
          ""Commodity"": {
            ""descriptionOfGoods"": """",
            ""GoodsMeasure"": {},
            ""InvoiceLine"": {
              ""itemAmountInvoiced"": 0.0
            }
          }
        }
      ]
    }
  ]
}";
			AssertEquals(expectedMessage, message);
		}

		[TestDate(2023, 01, 15)]
		public void TestGetCC432Message()
		{
			var message = PrepareDeclarationAndGetMessage(DeltaIESendMessageSubTypeList.Codes.PresentationNotification);
			var expectedMessage = @"{
  ""ImportOperation"": {
    ""LRN"": ""LRN12345"",
    ""customsRegistrationNumber"": ""CRN12345""
  },
  ""Declarant"": {
    ""name"": ""EDI CUSTOMS BROKERS""
  },
  ""Representative"": {
    ""identificationNumber"": ""FR12345678900001"",
    ""status"": ""2""
  },
  ""GoodsShipment"": {
    ""Consignment"": {
      ""LocationOfGoods"": {
        ""typeOfLocation"": ""F"",
        ""qualifierOfIdentification"": ""C""
      }
    },
    ""GoodsShipmentItem"": [
      {
        ""sequenceNumber"": ""0"",
        ""declarationGoodsItemNumber"": ""0""
      }
    ]
  }
}";
			AssertEquals(expectedMessage, message);
		}

		[TestDate(2025, 03, 05)]
		public void TestGetCC414Message()
		{
			var message = PrepareDeclarationAndGetMessage(DeltaIESendMessageSubTypeList.Codes.Invalidation);
			var expectedMessage = @"{
  ""Request"": {
    ""operatorRequestReference"": ""LRN12345-20250305120000""
  },
  ""ImportOperation"": [
    {
      ""sequenceNumber"": ""1"",
      ""LRN"": ""LRN12345"",
      ""MRN"": ""MRN12345"",
      ""invalidationRequestDateAndTime"": ""2025-03-05T00:00:00"",
      ""invalidationMotivation"": ""MOTIV"",
      ""invalidationReason"": ""VOCReason""
    }
  ],
  ""Declarant"": {
    ""name"": ""EDI CUSTOMS BROKERS""
  },
  ""Representative"": {
    ""identificationNumber"": ""FR12345678900001"",
    ""status"": ""2""
  }
}";
			AssertEquals(expectedMessage, message);
		}

		public void TestMessageBuilder()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MergedLines.AddNew();
			var sendingObject = new DeltaIEJobDeclarationMessageSendingObject(entryHeader);
			sendingObject.MessageType = DeltaIESendMessageSubTypeList.Codes.ImportDeclaration;
			var manager = new DeltaIEMessageBuilderManager();
			AssertType<CC415MessageBuilder>(manager.NewMessageBuilder(sendingObject));

			sendingObject.MessageType = DeltaIESendMessageSubTypeList.Codes.PresentationNotification;
			AssertType<CC432MessageBuilder>(manager.NewMessageBuilder(sendingObject));
		}

		public void TestBuilderType()
		{
			var manager = new DeltaIEMessageBuilderManager();
			AssertEquals(DeltaIEMessageTypes.Codes.DEC, manager.BuilderType);
		}

		[TestDate(2023, 01, 15, 08, 50, 30)]
		public void TestGetCC413Message()
		{
			var message = PrepareDeclarationAndGetMessage(DeltaIESendMessageSubTypeList.Codes.AmendmentRequest);
			var expectedMessage = @"{
  ""Request"": {
    ""operatorRequestReference"": ""LRN12345-20230115085030"",
    ""amendmentRequestDateAndTime"": ""2023-01-15T08:50:30"",
    ""amendmentMotivation"": ""MOTIV"",
    ""amendmentReason"": ""VOCReason""
  },
  ""ImportOperation"": {
    ""LRN"": ""LRN12345"",
    ""MRN"": ""MRN12345"",
    ""declarationType"": ""FR"",
    ""additionalDeclarationType"": ""D"",
    ""presentationNotificationEstimatedDateAndTime"": ""2023-01-15T00:00:00"",
    ""languageCode"": ""FR""
  },
  ""CustomsOfficeOfPresentation"": {
    ""referenceNumber"": """"
  },
  ""Declarant"": {
    ""name"": ""EDI CUSTOMS BROKERS""
  },
  ""Representative"": {
    ""identificationNumber"": ""FR12345678900001"",
    ""status"": ""2""
  },
  ""CurrencyExchange"": {
    ""internalCurrencyUnit"": ""EUR""
  },
  ""GoodsShipment"": [
    {
      ""sequenceNumber"": ""1"",
      ""invoiceCurrency"": ""EUR"",
      ""exchangeRate"": 1.0,
      ""AdditionalInformation"": [
        {
          ""sequenceNumber"": ""1"",
          ""code"": ""A0010"",
          ""ccQualifier"": ""FR""
        }
      ],
      ""Consignment"": {
        ""containerIndicator"": ""0"",
        ""LocationOfGoods"": {
          ""typeOfLocation"": ""F"",
          ""qualifierOfIdentification"": ""C""
        }
      },
      ""GoodsShipmentItem"": [
        {
          ""sequenceNumber"": ""0"",
          ""declarationGoodsItemNumber"": ""0"",
          ""Commodity"": {
            ""descriptionOfGoods"": """",
            ""GoodsMeasure"": {},
            ""InvoiceLine"": {
              ""itemAmountInvoiced"": 0.0
            }
          }
        }
      ]
    }
  ]
}";
			AssertEquals(expectedMessage, message);
		}
	}
}

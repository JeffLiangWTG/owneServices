using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages.Testing;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class DeltaIEOutgoingInterchangeProviderTest : FRInterchangeProviderBaseTest<DeltaIEOutgoingInterchangeProvider>
	{
		protected override ZString MessageText => @"{""ImportOperation"":{""LRN"":""0000005856""}}";

		protected override ZString MessageType => MessageTypeList.Codes.DEC;

		protected override ZString MessageSubType => DeltaIESendMessageSubTypeList.Codes.ImportDeclaration;

		protected override ZString ExpectedInterchangeType => GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsDeltaIE;

		protected override ZString ExpectedInterchangeBodyText(TestEDIMessage message) => @"{""SchemaId"":""IE415"",""TransactionId"":""0000005856"",""MessageJson"":{""ImportOperation"":{""LRN"":""0000005856""}}}";

		public void TestInterchangesIsUniqueForInvalidationMessage()
		{
			#region Message

			var messageText1 = @"{
  ""ImportOperation"": [
	{
	  ""sequenceNumber"": ""1"",
	  ""LRN"": ""0000005856"",
	  ""customsRegistrationNumber"": ""23FRD0000001228AR1"",
	  ""MRN"": ""23FRD2300001228AR1"",
	  ""invalidationRequestDateAndTime"": ""2023-10-23T10:43:06"",
	  ""invalidationMotivation"": ""TEST2"",
	  ""invalidationReason"": ""TEST2"",
	  ""SupportingDocument"": [
		{
		  ""sequenceNumber"": ""1"",
		  ""type"": ""1001"",
		  ""referenceNumber"": ""234324""
		},
		{
		  ""sequenceNumber"": ""2"",
		  ""type"": ""1008"",
		  ""referenceNumber"": ""0490286545""
		}
	  ]
	}
  ],
  ""Declarant"": {
	""identificationNumber"": ""FR33159700500064""
  },
  ""Representative"": {
	""identificationNumber"": ""FR32582075100080"",
	""status"": ""2""
  }
}";

			var messageText2 = @"{
  ""ImportOperation"": [
	{
	  ""sequenceNumber"": ""1"",
	  ""LRN"": ""0000005857"",
	  ""customsRegistrationNumber"": ""23FRD0000001228AR2"",
	  ""MRN"": ""23FRD2300001228AR2"",
	  ""invalidationRequestDateAndTime"": ""2023-10-23T10:43:07"",
	  ""invalidationMotivation"": ""TEST2"",
	  ""invalidationReason"": ""TEST2"",
	  ""SupportingDocument"": [
		{
		  ""sequenceNumber"": ""1"",
		  ""type"": ""1001"",
		  ""referenceNumber"": ""234324""
		},
		{
		  ""sequenceNumber"": ""2"",
		  ""type"": ""1008"",
		  ""referenceNumber"": ""0490286545""
		}
	  ]
	}
  ],
  ""Declarant"": {
	""identificationNumber"": ""FR33159700500064""
  },
  ""Representative"": {
	""identificationNumber"": ""FR32582075100080"",
	""status"": ""2""
  }
}";

			var messageText3 = @"{
  ""ImportOperation"": [
	{
	  ""sequenceNumber"": ""1"",
	  ""LRN"": ""0000005858"",
	  ""customsRegistrationNumber"": ""23FRD0000001228AR2"",
	  ""MRN"": ""23FRD2300001228AR2"",
	  ""invalidationRequestDateAndTime"": ""2023-10-23T10:43:07"",
	  ""invalidationMotivation"": ""TEST2"",
	  ""invalidationReason"": ""TEST2"",
	  ""SupportingDocument"": [
		{
		  ""sequenceNumber"": ""1"",
		  ""type"": ""1001"",
		  ""referenceNumber"": ""12345""
		},
		{
		  ""sequenceNumber"": ""2"",
		  ""type"": ""1008"",
		  ""referenceNumber"": ""0490286546""
		}
	  ]
	}
  ],
  ""Declarant"": {
	""identificationNumber"": ""FR33159700500064""
  },
  ""Representative"": {
	""identificationNumber"": ""FR32582075100080"",
	""status"": ""2""
  }
}";

			var messageText4 = @"{
  ""ImportOperation"": [
	{
	  ""sequenceNumber"": ""1"",
	  ""LRN"": ""0000005859"",
	  ""customsRegistrationNumber"": ""23FRD0000001228AR2"",
	  ""MRN"": ""23FRD2300001228AR2"",
	  ""invalidationRequestDateAndTime"": ""2023-10-23T10:43:07"",
	  ""invalidationMotivation"": ""TEST2"",
	  ""invalidationReason"": ""TEST2"",
	  ""SupportingDocument"": [
		{
		  ""sequenceNumber"": ""1"",
		  ""type"": ""1001"",
		  ""referenceNumber"": ""12345""
		},
		{
		  ""sequenceNumber"": ""2"",
		  ""type"": ""1008"",
		  ""referenceNumber"": ""0490286546""
		}
	  ]
	}
  ],
  ""Declarant"": {
	""identificationNumber"": ""FR33159700500064""
  },
  ""Representative"": {
	""identificationNumber"": ""FR32582075100080"",
	""status"": ""2""
  }
}";

			var messageText5 = @"{
  ""ImportOperation"": [
	{
	  ""sequenceNumber"": ""1"",
	  ""LRN"": ""0000005860"",
	  ""customsRegistrationNumber"": ""23FRD0000001228AR2"",
	  ""MRN"": ""23FRD2300001228AR2"",
	  ""invalidationRequestDateAndTime"": ""2023-10-23T10:43:07"",
	  ""invalidationMotivation"": ""TEST2"",
	  ""invalidationReason"": ""TEST2"",
	  ""SupportingDocument"": [
		{
		  ""sequenceNumber"": ""1"",
		  ""type"": ""1001"",
		  ""referenceNumber"": ""12345""
		},
		{
		  ""sequenceNumber"": ""2"",
		  ""type"": ""1008"",
		  ""referenceNumber"": ""0490286546""
		}
	  ]
	}
  ],
  ""Declarant"": {
	""identificationNumber"": ""FR33159700500064""
  },
  ""Representative"": {
	""identificationNumber"": ""FR32582075100080"",
	""status"": ""2""
  }
}";

			var messageFinal = @"{""SchemaId"":""IE414"",""TransactionId"":""0000005856"",""MessageJson"":{""ImportOperation"":[
{
	  ""sequenceNumber"": ""1"",
	  ""LRN"": ""0000005856"",
	  ""customsRegistrationNumber"": ""23FRD0000001228AR1"",
	  ""MRN"": ""23FRD2300001228AR1"",
	  ""invalidationRequestDateAndTime"": ""2023-10-23T10:43:06"",
	  ""invalidationMotivation"": ""TEST2"",
	  ""invalidationReason"": ""TEST2"",
	  ""SupportingDocument"": [
		{
		  ""sequenceNumber"": ""1"",
		  ""type"": ""1001"",
		  ""referenceNumber"": ""234324""
		},
		{
		  ""sequenceNumber"": ""2"",
		  ""type"": ""1008"",
		  ""referenceNumber"": ""0490286545""
		}
	  ]
	},
{
	  ""sequenceNumber"": ""2"",
	  ""LRN"": ""0000005857"",
	  ""customsRegistrationNumber"": ""23FRD0000001228AR2"",
	  ""MRN"": ""23FRD2300001228AR2"",
	  ""invalidationRequestDateAndTime"": ""2023-10-23T10:43:07"",
	  ""invalidationMotivation"": ""TEST2"",
	  ""invalidationReason"": ""TEST2"",
	  ""SupportingDocument"": [
		{
		  ""sequenceNumber"": ""1"",
		  ""type"": ""1001"",
		  ""referenceNumber"": ""234324""
		},
		{
		  ""sequenceNumber"": ""2"",
		  ""type"": ""1008"",
		  ""referenceNumber"": ""0490286545""
		}
	  ]
	},
{
	  ""sequenceNumber"": ""3"",
	  ""LRN"": ""0000005858"",
	  ""customsRegistrationNumber"": ""23FRD0000001228AR2"",
	  ""MRN"": ""23FRD2300001228AR2"",
	  ""invalidationRequestDateAndTime"": ""2023-10-23T10:43:07"",
	  ""invalidationMotivation"": ""TEST2"",
	  ""invalidationReason"": ""TEST2"",
	  ""SupportingDocument"": [
		{
		  ""sequenceNumber"": ""1"",
		  ""type"": ""1001"",
		  ""referenceNumber"": ""12345""
		},
		{
		  ""sequenceNumber"": ""2"",
		  ""type"": ""1008"",
		  ""referenceNumber"": ""0490286546""
		}
	  ]
	}
  ],
  ""Declarant"": {
	""identificationNumber"": ""FR33159700500064""
  },
  ""Representative"": {
	""identificationNumber"": ""FR32582075100080"",
	""status"": ""2""
  }
}}";

			#endregion

			var message = GetMessageForOutgoing("0000005856", DeltaIESendMessageSubTypeList.Codes.Invalidation, messageText1, "0000000000000001");
			var message2 = GetMessageForOutgoing("0000005857", DeltaIESendMessageSubTypeList.Codes.Invalidation, messageText2, "0000000000000001");
			var message3 = GetMessageForOutgoing("0000005858", DeltaIESendMessageSubTypeList.Codes.Invalidation, messageText3, "0000000000000001");
			var message4 = GetMessageForOutgoing("0000005859", DeltaIESendMessageSubTypeList.Codes.Invalidation, messageText4, "0000000000000002");
			var message5 = GetMessageForOutgoing("0000005860", DeltaIESendMessageSubTypeList.Codes.Invalidation, messageText5, ZString.Empty);

			var messages = new NonDependentEDIMessageCollection(Factory)
			{
				message,
				message2,
				message3,
				message4,
				message5
			};

			var provider = (DeltaIEOutgoingInterchangeProvider)Activator.CreateInstance(typeof(DeltaIEOutgoingInterchangeProvider), messages);
			provider.PackCollatedMessagesIntoInterchanges();

			var loadedInterchanges = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.GenericMessageDelivery));
			AssertEquals("One interchange should be created to input message.", 3, loadedInterchanges.Length);

			var interchangeCreated = loadedInterchanges.Cast<EDIInterchange>().FirstOrDefault(x => x.EI_BodyText.Contains("0000005856"));
			MessageProcessorTestHelper.AssertEDIInterchangePropertiesWithJson(interchangeCreated,
			expectedApplicationCode: "GMD",
			expectedInterchangeType: ExpectedInterchangeType,
			expectedReceiveTransmit: "TRX",
			expectedStatus: "HQU",
			expectedFrom: GlbCompany.CurrentCompany.LicenceKeyIdentifier,
			expectedTo: FRCustomsDataRegistry.Instance.RecipientID.Value,
			expectedBodyText: messageFinal,
			expectedHeaderText: $"<SenderID>EDIEDIDAT</SenderID><RecipientID>EASYLO2TEST_EAD</RecipientID><InterchangeType>{ExpectedInterchangeType}</InterchangeType><InterchangeNumber>",
			message: $"MessageType:{MessageType}&MessageSubType:{DeltaIESendMessageSubTypeList.Codes.Invalidation} Interchange properties");
		}

		TestEDIMessage GetMessageForOutgoing(string corellationID, string messageSubType, string messageText, string applicationReference)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CorrelationID = corellationID;

			var message = Factory.NewWithValidTestData<TestEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = messageText;
			message.EM_MessageType = MessageType;
			message.EM_ApplicationReference = applicationReference;
			message.EM_LinkedObject = entryHeader;
			message.EM_MessageSubType = messageSubType;

			return message;
		}
	}
}

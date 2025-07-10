using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE013TransitOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<IE013TransitOperationProvider>
	{
		public void TestMRN()
		{
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRNIE123412345";
			AssertEquals("MRN", "MRNIE123412345", Provider.MRN);
		}

		public void TestNoGuaranteeTypeWillNotCauseError()
		{
			AddGuarantee("1", "ABC123", "12345", "pass", 123.45m, "EUR");
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			SendMessage();
			movementHeader.Messages[0].EM_MessageText = @"<q1:CC015C xmlns:q1=""http://ncts.dgtaxud.ec"">
  <messageSender>_NCTSMessageSenderHolder_</messageSender>
  <messageRecipient>IE</messageRecipient>
  <preparationDateAndTime>2023-08-29T02:55:16</preparationDateAndTime>
  <messageIdentification>_NCTSMessageRecipientHolder_</messageIdentification>
  <messageType>CC015C</messageType>
  <TransitOperation>
	<LRN>NCTS__LRN_Holder</LRN>
	<security>0</security>
	<reducedDatasetIndicator>0</reducedDatasetIndicator>
	<communicationLanguageAtDeparture>IE</communicationLanguageAtDeparture>
	<bindingItinerary>0</bindingItinerary>
  </TransitOperation>
  <Guarantee>
	<sequenceNumber>1</sequenceNumber>
	<GuaranteeReference>
	  <sequenceNumber>1</sequenceNumber>
	  <GRN>12345</GRN>
	  <accessCode>pass</accessCode>
	  <amountToBeCovered>123.45</amountToBeCovered>
	  <currency>EUR</currency>
	</GuaranteeReference>
  </Guarantee>
  <Consignment>
	<containerIndicator>0</containerIndicator>
	<grossMass>0</grossMass>
	<LocationOfGoods>
	  <typeOfLocation />
	  <qualifierOfIdentification />
	</LocationOfGoods>
	<ActiveBorderTransportMeans>
	  <sequenceNumber>1</sequenceNumber>
	</ActiveBorderTransportMeans>
  </Consignment>
</q1:CC015C>
";
			movementHeader.Messages[0].EM_Status = EDIMessage.Status.Sent;
			Factory.Save();

			movementHeader.TirCarnetNumber = "NO5678";
			AssertNoExceptionThrown("Message without guaranteeType will not cause error", () =>
			{
				var flag = Provider.AmendmentTypeFlag;
			});
		}

		public void TestNoGRNWillNotCauseError()
		{
			AddGuarantee("1", "ABC123", "12345", "pass", 123.45m, "EUR");
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			SendMessage();
			movementHeader.Messages[0].EM_MessageText = @"<q1:CC015C xmlns:q1=""http://ncts.dgtaxud.ec"">
  <messageSender>_NCTSMessageSenderHolder_</messageSender>
  <messageRecipient>IE</messageRecipient>
  <preparationDateAndTime>2023-08-29T02:55:16</preparationDateAndTime>
  <messageIdentification>_NCTSMessageRecipientHolder_</messageIdentification>
  <messageType>CC015C</messageType>
  <TransitOperation>
	<LRN>NCTS__LRN_Holder</LRN>
	<security>0</security>
	<reducedDatasetIndicator>0</reducedDatasetIndicator>
	<communicationLanguageAtDeparture>IE</communicationLanguageAtDeparture>
	<bindingItinerary>0</bindingItinerary>
  </TransitOperation>
  <Guarantee>
	<sequenceNumber>1</sequenceNumber>
	<guaranteeType>1</guaranteeType>
	<GuaranteeReference>
	  <sequenceNumber>1</sequenceNumber>
	  <accessCode>pass</accessCode>
	  <amountToBeCovered>123.45</amountToBeCovered>
	  <currency>EUR</currency>
	</GuaranteeReference>
  </Guarantee>
  <Consignment>
	<containerIndicator>0</containerIndicator>
	<grossMass>0</grossMass>
	<LocationOfGoods>
	  <typeOfLocation />
	  <qualifierOfIdentification />
	</LocationOfGoods>
	<ActiveBorderTransportMeans>
	  <sequenceNumber>1</sequenceNumber>
	</ActiveBorderTransportMeans>
  </Consignment>
</q1:CC015C>
";
			movementHeader.Messages[0].EM_Status = EDIMessage.Status.Sent;
			Factory.Save();

			movementHeader.TirCarnetNumber = "NO5678";
			AssertNoExceptionThrown("Message without GRN will not cause error", () =>
			{
				var flag = Provider.AmendmentTypeFlag;
			});
		}

		public void TestNoAmountToBeCoveredWillNotCauseError()
		{
			AddGuarantee("1", "ABC123", "12345", "pass", 123.45m, "EUR");
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			SendMessage();
			movementHeader.Messages[0].EM_MessageText = @"<q1:CC015C xmlns:q1=""http://ncts.dgtaxud.ec"">
  <messageSender>_NCTSMessageSenderHolder_</messageSender>
  <messageRecipient>IE</messageRecipient>
  <preparationDateAndTime>2023-08-29T02:55:16</preparationDateAndTime>
  <messageIdentification>_NCTSMessageRecipientHolder_</messageIdentification>
  <messageType>CC015C</messageType>
  <TransitOperation>
	<LRN>NCTS__LRN_Holder</LRN>
	<security>0</security>
	<reducedDatasetIndicator>0</reducedDatasetIndicator>
	<communicationLanguageAtDeparture>IE</communicationLanguageAtDeparture>
	<bindingItinerary>0</bindingItinerary>
  </TransitOperation>
  <Guarantee>
	<sequenceNumber>1</sequenceNumber>
	<guaranteeType>1</guaranteeType>
	<GuaranteeReference>
	  <sequenceNumber>1</sequenceNumber>
	  <GRN>12345</GRN>
	  <accessCode>pass</accessCode>
	  <currency>EUR</currency>
	</GuaranteeReference>
  </Guarantee>
  <Consignment>
	<containerIndicator>0</containerIndicator>
	<grossMass>0</grossMass>
	<LocationOfGoods>
	  <typeOfLocation />
	  <qualifierOfIdentification />
	</LocationOfGoods>
	<ActiveBorderTransportMeans>
	  <sequenceNumber>1</sequenceNumber>
	</ActiveBorderTransportMeans>
  </Consignment>
</q1:CC015C>

";
			movementHeader.Messages[0].EM_Status = EDIMessage.Status.Sent;
			Factory.Save();

			movementHeader.TirCarnetNumber = "NO5678";
			AssertNoExceptionThrown("Message without AmountToBeCovered will not cause error", () =>
			{
				var flag = Provider.AmendmentTypeFlag;
			});
		}

		public void TestAmendmentTypeFlag()
		{
			AddGuarantee("1", "ABC123", "12345", "pass", 123.45m, "EUR");
			AddGuarantee("3", "DEF456", "67890", "word", 98.76m, "GBP");
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			SendMessage();

			AssertEquals("Message count", 1, movementHeader.Messages.Count);
			AssertEquals("Application code", "IEN", movementHeader.Messages[0].EM_ApplicationCode);
			AssertEquals("Message Type", "015", movementHeader.Messages[0].EM_MessageType);
			AssertEquals("Direction", "TRX", movementHeader.Messages[0].EM_ReceiveTransmit);
			AssertStartsWith("Message Text", "<q1:CC015C xmlns:q1=\"http://ncts.dgtaxud.ec\">", movementHeader.Messages[0].EM_MessageText);

			movementHeader.Messages[0].EM_Status = EDIMessage.Status.Acknowledged;
			Factory.Save();

			movementHeader.Guarantees[0].Reload();
			AssertEquals("Amendment Type flag should be false as nothing has changed", false, Provider.AmendmentTypeFlag);

			movementHeader.TirCarnetNumber = "NO5678";
			AssertEquals("Amendment Type flag should be false as guarantees not changed", false, Provider.AmendmentTypeFlag);

			movementHeader.Guarantees[0].PW_BondType = "2";
			AssertEquals("Amendment Type flag should be true as PW_BondType has been changed", true, Provider.AmendmentTypeFlag);

			movementHeader.Guarantees[0].PW_BondType = "1";
			AssertEquals("Amendment Type flag should be false as PW_BondType has been reverted", false, Provider.AmendmentTypeFlag);

			movementHeader.Guarantees[1].PW_BondNumber2 = "XYZ789";
			AssertEquals("Amendment Type flag should be true as PW_BondNumber2 has been changed", true, Provider.AmendmentTypeFlag);

			movementHeader.Guarantees[1].PW_BondNumber2 = "DEF456";
			AssertEquals("Amendment Type flag should be false as PW_BondNumber2 has been reverted", false, Provider.AmendmentTypeFlag);

			movementHeader.Guarantees[1].PW_BondNumber = "22222";
			AssertEquals("Amendment Type flag should be true as PW_BondNumber has been changed", true, Provider.AmendmentTypeFlag);

			movementHeader.Guarantees[1].PW_BondNumber = "67890";
			AssertEquals("Amendment Type flag should be false as PW_BondNumber has been reverted", false, Provider.AmendmentTypeFlag);

			movementHeader.Guarantees[0].PW_BondAmount = 123.0m;
			AssertEquals("Amendment Type flag should be true as PW_BondAmount has been changed", true, Provider.AmendmentTypeFlag);

			movementHeader.Guarantees[0].PW_BondAmount = 123.45m;
			AssertEquals("Amendment Type flag should be false as PW_BondAmount has been reverted", false, Provider.AmendmentTypeFlag);

			AddGuarantee("8", "IJK456", "33322", "sswo", 18.76m, "GBP");
			AssertEquals("Amendment Type flag should be true as new Guarantee has been added", true, Provider.AmendmentTypeFlag);
		}

		public void TestAmendmentTypeFlag_GuaranteeTypeB()
		{
			AddGuarantee("B", "ABC123", "12345", "pass", 123.45m, "EUR");
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			SendMessage();
			AssertEquals("Message count", 1, movementHeader.Messages.Count);
			AssertEquals("Application code", "IEN", movementHeader.Messages[0].EM_ApplicationCode);
			AssertEquals("Message Type", "015", movementHeader.Messages[0].EM_MessageType);
			AssertEquals("Direction", "TRX", movementHeader.Messages[0].EM_ReceiveTransmit);
			AssertStartsWith("Message Text", "<q1:CC015C xmlns:q1=\"http://ncts.dgtaxud.ec\">", movementHeader.Messages[0].EM_MessageText);

			movementHeader.Messages[0].EM_Status = EDIMessage.Status.Sent;
			Factory.Save();

			AssertEquals("Amendment Type flag should be false as nothing has changed", false, Provider.AmendmentTypeFlag);
		}

		public void TestAmendmentTypeFlag_NoPreviousMessage()
		{
			AssertEquals("Amendment Type flag should be false as default", false, Provider.AmendmentTypeFlag);
		}

		public void TestAmendmentTypeFlag_PreviousMessageIsIE013()
		{
			AddGuarantee("1", "ABC123", "12345", "pass", 123.45m, "EUR");
			AddGuarantee("3", "DEF456", "67890", "word", 98.76m, "GBP");
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;

			SendMessage();
			SendMessage(NCTSOutgoingMessageTypeList.Codes.DeclarationAmendment);

			AssertEquals("Message count", 2, movementHeader.Messages.Count);
			AssertEquals("Application code", "IEN", movementHeader.Messages[1].EM_ApplicationCode);
			AssertEquals("Message Type", "013", movementHeader.Messages[1].EM_MessageType);
			AssertEquals("Direction", "TRX", movementHeader.Messages[1].EM_ReceiveTransmit);
			AssertStartsWith("Message Text", "<q1:CC013C xmlns:q1=\"http://ncts.dgtaxud.ec\">", movementHeader.Messages[1].EM_MessageText);

			movementHeader.Messages[1].EM_Status = EDIMessage.Status.Sent;
			Factory.Save();

			movementHeader.Guarantees[0].Reload();
			AssertEquals("Amendment Type flag should be false as nothing has changed", false, Provider.AmendmentTypeFlag);

			movementHeader.TirCarnetNumber = "NO5678";
			AssertEquals("Amendment Type flag should be false as guarantees not changed", false, Provider.AmendmentTypeFlag);

			movementHeader.Guarantees[0].PW_BondType = "2";
			AssertEquals("Amendment Type flag should be true as PW_BondType has been changed", true, Provider.AmendmentTypeFlag);
		}

		public void TestAmendmentTypeFlag_GuaranteeTypeNotSet()
		{
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			var guarantee = movementHeader.Guarantees.AddNew();
			guarantee.PW_BondType = string.Empty;
			guarantee.PW_BondNumber = "1234";
			SendMessage();
			AssertEquals("Message count", 1, movementHeader.Messages.Count);
			movementHeader.Messages[0].EM_Status = EDIMessage.Status.Acknowledged;
			Factory.Save();
			AssertEquals("Amendment Type flag should be false as nothing changed", false, Provider.AmendmentTypeFlag);
		}

		public void TestAmendmentTypeFlag_GuaranteeReferenceNotSet()
		{
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			var guarantee = movementHeader.Guarantees.AddNew();
			guarantee.PW_BondType = "1";
			guarantee.PW_BondNumber2 = "23456";
			SendMessage();
			AssertEquals("Message count", 1, movementHeader.Messages.Count);
			movementHeader.Messages[0].EM_Status = EDIMessage.Status.Acknowledged;
			Factory.Save();
			AssertEquals("Amendment Type flag should be false as nothing changed", false, Provider.AmendmentTypeFlag);
		}

		void SendMessage(string messageType = NCTSOutgoingMessageTypeList.Codes.DeclarationData)
		{
			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			var sendingAction = new NctsMessageSendingAction(sendingObject);
			sendingAction.MessageType = messageType;
			var sender = new NctsMessageSender(sendingAction);
			sender.Send();
		}

		protected override IE013TransitOperationProvider GetProvider() => new IE013TransitOperationProvider(nctsHeader);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = nctsHeader.MovementHeader;
		}

		void AddGuarantee(string bondType, string bondNumber2, string bondNumber, string password, decimal bondAmount, string currency)
		{
			var guarantee = movementHeader.Guarantees.AddNew();
			guarantee.PW_BondType = bondType;
			guarantee.PW_BondNumber2 = bondNumber2;
			guarantee.PW_BondNumber = bondNumber;
			guarantee.PW_Password = password;
			guarantee.PW_BondAmount = bondAmount;
			guarantee.PW_RX_NKCurrency = currency;
		}

		NctsHeader nctsHeader;
		NctsDepartureMovementHeader movementHeader;
	}
}

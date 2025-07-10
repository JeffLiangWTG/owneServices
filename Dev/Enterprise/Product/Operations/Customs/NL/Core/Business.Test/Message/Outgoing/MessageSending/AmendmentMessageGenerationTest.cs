//using CargoWise.Types;
//using Enterprise.Customs.NL.Business.Declaration;
//using Enterprise.Customs.NL.Business.Message.MessageSenders;
//using Enterprise.Customs.NL.Business.Message.Wrappers.Testing;
//using Enterprise.Customs.Universal.Testing;
//using NUnit.Framework;

//namespace Enterprise.Customs.NL.Business.Testing
//{
//	class AmendmentMessageGenerationTest : MessageGenerationAbstractTest
//	{
//		protected override ZString messageType => "NAM";

//		protected override ZString expectedMessage => MessageGeneratorTestHelper.GetExpectedMessageXML("Enterprise.Customs.NL.Business.Testing.MessageSending.TestFiles.AmendmentMessage.xml");

//		protected override ZString expectedPrettyMessage => ZString.Empty;

//		public void TestBuildProducesCorrectOrder()
//		{
//			var helper = new UniversalReferenceTestDataHelper(Factory);
//			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "test", dataGrouping: "DMS");
//			var code1 = helper.CreateCusCodeList("DMS", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "ADD1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
//			helper.CreateCusCodeListAttribute(code1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.Header);

//			var entryHeader = WrapperTestHelper.GetEntryHeaderForTest(Factory);
//			CreateOriginalDecMessage(entryHeader);

//			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);

//			var addInfo = entryHeader.Declaration.AdditionalInfos.AddNew();
//			addInfo.CSI_Type = "INF";
//			addInfo.CSI_Code = "ADD1";
//			addInfo.CSI_Description = "DESC";
//			addInfo.CSI_ReferenceNumber = "123";
//			entryHeader.CH_CustomsMessageRemarks = "Amending";

//			Factory.Save();

//			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
//			JobDeclarationMessageSendingObject sendingObject = decWrapper.SendingObjectsCollection[0];
//			sendingObject.MessageType = SendMessageTypes.Codes.AMD;
//			sendingObject.TypeOfMessage = SendMessageTypes.Codes.AMD;
//			sendingObject.ShouldSend = true;

//			var messageBuilder = MessageGenerator.GetMessageBuilder(sendingObject);
//			var amendmentMessage = messageBuilder.CreateMessage(true).Replace(" ", "");

//			AssertXMLContains(MessageGeneratorTestHelper.GetExpectedMessageXML(@"Enterprise.Customs.NL.Business.Testing.MessageSending.TestFiles.ExpectedAmendmentCorrectOrder.xml").Replace(" ", ""), amendmentMessage);
//		}

//		public void TestBuildWithDataChanged()
//		{
//			var entryHeader = WrapperTestHelper.GetEntryHeaderForTest(Factory);

//			var invoiceLines = entryHeader.InvoiceLines;
//			foreach (JobComInvoiceLine line in invoiceLines)
//			{
//				line.CusSupplyChainActorReferences.RemoveAndDeleteAll();
//			}

//			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);
//			CreateOriginalDecMessage(entryHeader);

//			var invoice = entryHeader.Declaration.Invoices[0];
//			invoice.JZ_RX_NKInvoice_Currency = "USD";
//			var invoiceLine = invoice.InvoiceLines[0];
//			invoiceLine.JI_LinePrice = 200;

//			entryHeader.CH_CustomsMessageRemarks = "Amending";

//			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

//			JobDeclarationMessageSendingObject sendingObject = decWrapper.SendingObjectsCollection[0];
//			sendingObject.MessageType = SendMessageTypes.Codes.AMD;
//			sendingObject.TypeOfMessage = SendMessageTypes.Codes.AMD;
//			sendingObject.ShouldSend = true;

//			var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
//			var sender = new JobDeclarationMessageSender(decWrapper);
//			sender.Send(shutUp);

//			var amendmentMessage = entryHeader.Messages[3].EM_MessageText.Replace(" ", "");
//			amendmentMessage = amendmentMessage.Replace("\r\n", "");
//			var expectedMessage = MessageGeneratorTestHelper.GetExpectedMessageXML(@"Enterprise.Customs.NL.Business.Testing.MessageSending.TestFiles.ExpectedAmendmentWithAttributeChange.xml").Replace(" ", "");
//			expectedMessage = expectedMessage.Replace("\r\n", "");

//			AssertXMLContains(expectedMessage, amendmentMessage);
//		}

//		public void TestBuildWithDataAdded()
//		{
//			var helper = new UniversalReferenceTestDataHelper(Factory);
//			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "test", dataGrouping: "DMS");
//			var code1 = helper.CreateCusCodeList("DMS", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "ADD1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
//			helper.CreateCusCodeListAttribute(code1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.Header);

//			var entryHeader = WrapperTestHelper.GetEntryHeaderForTest(Factory);
//			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);

//			var invoiceLines = entryHeader.InvoiceLines;
//			foreach (JobComInvoiceLine line in invoiceLines)
//			{
//				line.CusSupplyChainActorReferences.RemoveAndDeleteAll();
//			}

//			CreateOriginalDecMessage(entryHeader);

//			var addInfo = entryHeader.Declaration.AdditionalInfos.AddNew();
//			addInfo.CSI_Type = "INF";
//			addInfo.CSI_Code = "ADD1";
//			addInfo.CSI_Description = "DESC";
//			addInfo.CSI_ReferenceNumber = "123";

//			entryHeader.CH_CustomsMessageRemarks = "Amending";

//			Factory.Save();

//			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

//			JobDeclarationMessageSendingObject sendingObject = decWrapper.SendingObjectsCollection[0];
//			sendingObject.MessageType = SendMessageTypes.Codes.AMD;
//			sendingObject.TypeOfMessage = SendMessageTypes.Codes.AMD;
//			sendingObject.ShouldSend = true;

//			var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
//			var sender = new JobDeclarationMessageSender(decWrapper);
//			sender.Send(shutUp);

//			var amendmentMessage = entryHeader.Messages[3].EM_MessageText.Replace(" ", "");
//			amendmentMessage = amendmentMessage.Replace("\r\n", "");
//			var expectedMessage = MessageGeneratorTestHelper.GetExpectedMessageXML(@"Enterprise.Customs.NL.Business.Testing.MessageSending.TestFiles.ExpectedAmendmentWithAddedAdditionalInfo.xml").Replace(" ", "");
//			expectedMessage = expectedMessage.Replace("\r\n", "");

//			AssertXMLContains(expectedMessage, amendmentMessage);
//		}

//		public void TestBuildWithDataDeleted()
//		{
//			var entryHeader = WrapperTestHelper.GetEntryHeaderForTest(Factory);

//			var invoiceLines = entryHeader.InvoiceLines;
//			foreach (JobComInvoiceLine line in invoiceLines)
//			{
//				line.CusSupplyChainActorReferences.RemoveAndDeleteAll();
//			}

//			var addInfo = entryHeader.Declaration.AdditionalInfos.AddNew();
//			addInfo.CSI_Type = "INF";
//			addInfo.CSI_Code = "ADD1";
//			addInfo.CSI_Description = "DESC";
//			addInfo.CSI_ReferenceNumber = "123";

//			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);
//			CreateOriginalDecMessage(entryHeader);

//			addInfo.Delete();

//			entryHeader.CH_CustomsMessageRemarks = "Amending";

//			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

//			JobDeclarationMessageSendingObject sendingObject = decWrapper.SendingObjectsCollection[0];
//			sendingObject.MessageType = SendMessageTypes.Codes.AMD;
//			sendingObject.TypeOfMessage = SendMessageTypes.Codes.AMD;
//			sendingObject.ShouldSend = true;

//			var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
//			var sender = new JobDeclarationMessageSender(decWrapper);
//			sender.Send(shutUp);

//			var amendmentMessage = entryHeader.Messages[3].EM_MessageText.Replace(" ", "");
//			amendmentMessage = amendmentMessage.Replace("\r\n", "");
//			var expectedMessage = MessageGeneratorTestHelper.GetExpectedMessageXML(@"Enterprise.Customs.NL.Business.Testing.MessageSending.TestFiles.ExpectedAmendmentWithDeletedAddInfo.xml").Replace(" ", "");
//			expectedMessage = expectedMessage.Replace("\r\n", "");

//			AssertXMLContains(expectedMessage, amendmentMessage);
//		}

//		[DeveloperOnlyTest]
//		public void TestBuildWithDataDeletedGivesOpid()
//		{
//			var entryHeader = WrapperTestHelper.GetEntryHeaderForTest(Factory);

//			var invoiceLines = entryHeader.InvoiceLines;
//			foreach (JobComInvoiceLine line in invoiceLines)
//			{
//				line.CusSupplyChainActorReferences.RemoveAndDeleteAll();
//			}

//			var addInfo = entryHeader.Declaration.AdditionalInfos[1];

//			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);
//			CreateOriginalDecMessage(entryHeader);

//			addInfo.Delete();

//			entryHeader.CH_CustomsMessageRemarks = "Amending";

//			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

//			JobDeclarationMessageSendingObject sendingObject = decWrapper.SendingObjectsCollection[0];
//			sendingObject.MessageType = SendMessageTypes.Codes.AMD;
//			sendingObject.TypeOfMessage = SendMessageTypes.Codes.AMD;
//			sendingObject.ShouldSend = true;

//			var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
//			var sender = new JobDeclarationMessageSender(decWrapper);
//			sender.Send(shutUp);

//			var amendmentMessage = entryHeader.Messages[3].EM_MessageText.Replace(" ", "");
//			amendmentMessage = amendmentMessage.Replace("\r\n", "");
//			var expectedMessage = MessageGeneratorTestHelper.GetExpectedMessageXML(@"Enterprise.Customs.NL.Business.Testing.MessageSending.TestFiles.ExpectedAmendmentWithDeletedAddInfo.xml").Replace(" ", "");
//			expectedMessage = expectedMessage.Replace("\r\n", "");

//			AssertXMLContains(expectedMessage, amendmentMessage);
//		}

//		//protected static void CreateOriginalDecMessage(CusEntryHeader entryHeader)
//		//{
//		//	var cei = entryHeader.Declaration.CustomsEntryInstructions.AddNew();
//		//	cei.CEI_Style = "H1";

//		//	var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

//		//	JobDeclarationMessageSendingObject sendingObject = decWrapper.SendingObjectsCollection[0];
//		//	sendingObject.MessageType = SendMessageTypes.Codes.DEC;
//		//	var messageBuilder = MessageGenerator.GetMessageBuilder(sendingObject);
//		//	var xml = messageBuilder.CreateMessage(true);

//		//	var originalMessageToAdd = entryHeader.Messages.AddNew(typeof(NLEDIMessage));
//		//	originalMessageToAdd.EM_MessageType = NLConstants.EdiMessageTypes.DMS;
//		//	originalMessageToAdd.EM_MessageSubType = sendingObject.MessageType;
//		//	originalMessageToAdd.EM_MessageText = xml.Replace("&lt;&lt;WCO TYPE PLACEHOLDER&gt;&gt;", "CC415A");
//		//	originalMessageToAdd.EM_ReceiveTransmit = "TRX";
//		//}
//	}
//}

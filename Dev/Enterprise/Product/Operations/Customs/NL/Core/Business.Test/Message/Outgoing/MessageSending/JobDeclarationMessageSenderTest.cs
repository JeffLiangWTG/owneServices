using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.NL.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class JobDeclarationMessageSenderTest : TestCaseWithFactory
{
	public void TestSend()
	{
		var declaration = GetTestDeclaration();
		var iSendObject = new SendsMessagesToCustomsShutterUpperer(true);
		declaration.MessageInitiator = iSendObject;

		declaration.DoMerge();

		Factory.Save();

		var entry = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault();

		var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
		JobDeclarationMessageSendingObject sendingobject = decWrapper.SendingObjectsCollection[0];
		sendingobject.Update = true;
		sendingobject.ShouldSend = true;

		var sender = new JobDeclarationMessageSender(decWrapper);

		sender.Send(iSendObject);
		Factory.Save();

		var testFactory = new BusinessObjectFactory();
		var chenckentry = testFactory.Load<CusEntryHeader>(entry.PK);

		AssertEquals(1, chenckentry.Messages.Count);
		AssertNull("There should be no warnings.", iSendObject.Warning);
	}

	public void TestSendMultiple()
	{
		var declaration = GetTestDeclarationMulti();
		var iSendObject = new SendsMessagesToCustomsShutterUpperer(true);
		declaration.MessageInitiator = iSendObject;

		declaration.DoMerge();

		Factory.Save();

		var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
		foreach (JobDeclarationMessageSendingObject sendingobject in decWrapper.SendingObjectsCollection)
		{
			sendingobject.Update = true;
			sendingobject.ShouldSend = true;
		}

		var sender = new JobDeclarationMessageSender(decWrapper);

		sender.Send(iSendObject);
		Factory.Save();

		AssertEquals(2, declaration.CustomsEntryHeaders.Count);

		var testFactory = new BusinessObjectFactory();

		foreach (var entry in declaration.CustomsEntryHeaders)
		{
			var chenckentry = testFactory.Load<CusEntryHeader>(entry.PK);
			AssertEquals(1, chenckentry.Messages.Count);
		}
		AssertNull("There should be no warnings.", iSendObject.Warning);
	}

	public void TestNoSend()
	{
		var declaration = GetTestDeclaration();
		var iSendObject = new SendsMessagesToCustomsShutterUpperer(true);
		declaration.MessageInitiator = iSendObject;

		declaration.DoMerge();

		Factory.Save();

		var entry = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault();

		var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
		JobDeclarationMessageSendingObject sendingobject = decWrapper.SendingObjectsCollection[0];
		sendingobject.Update = false;
		sendingobject.ShouldSend = false;

		var sender = new JobDeclarationMessageSender(decWrapper);

		sender.Send(iSendObject);
		Factory.Save();

		var testFactory = new BusinessObjectFactory();
		var chenckentry = testFactory.Load<CusEntryHeader>(entry.PK);

		AssertEquals(0, chenckentry.Messages.Count);
		AssertNull("There should be no warnings.", iSendObject.Warning);
	}

	public void TestSendPart()
	{
		var declaration = GetTestDeclarationMulti();
		var iSendObject = new SendsMessagesToCustomsShutterUpperer(true);
		declaration.MessageInitiator = iSendObject;

		declaration.DoMerge();

		Factory.Save();

		var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
		JobDeclarationMessageSendingObject sendingobject = decWrapper.SendingObjectsCollection[0];
		sendingobject.Update = true;
		sendingobject.ShouldSend = true;

		JobDeclarationMessageSendingObject sendingobject2 = decWrapper.SendingObjectsCollection[1];
		sendingobject2.Update = false;
		sendingobject2.ShouldSend = false;

		var sender = new JobDeclarationMessageSender(decWrapper);

		sender.Send(iSendObject);
		Factory.Save();

		AssertEquals(2, declaration.CustomsEntryHeaders.Count);

		var testFactory = new BusinessObjectFactory();

		CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
		var chenckentry = testFactory.Load<CusEntryHeader>(entry.PK);

		AssertEquals(1, chenckentry.Messages.Count);

		CusEntryHeader entry2 = declaration.CustomsEntryHeaders[1];
		var chenckentry2 = testFactory.Load<CusEntryHeader>(entry2.PK);

		AssertEquals(0, chenckentry2.Messages.Count);
		AssertNull("There should be no warnings.", iSendObject.Warning);
	}

	JobDeclaration GetTestDeclaration()
	{
		JobDeclaration declaration;
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		declaration.SupplierDocumentaryAddress.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = "A";
		entryInstruction.CEI_Style = "XX";

		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceAmount = 101.20;
		invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
		invoice.JZ_InvoiceNumber = "9478";

		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_LineNo = 1;
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_Weight = 500;
		invoiceLine.JI_NetWeight = 600;
		invoiceLine.JI_LinePrice = 101.20;

		Factory.Save();
		return declaration;
	}

	JobDeclaration GetTestDeclarationMulti()
	{
		JobDeclaration declaration;
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		declaration.SupplierDocumentaryAddress.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = "A";
		entryInstruction.CEI_Style = "XX";

		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.CEI_SubStyle = "A";
		entryInstruction2.CEI_Style = "XX";

		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceAmount = 101.20;
		invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
		invoice.JZ_InvoiceNumber = "9478";

		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_LineNo = 1;
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_Weight = 500;
		invoiceLine.JI_NetWeight = 600;
		invoiceLine.JI_LinePrice = 101.20;

		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_LineNo = 2;
		invoiceLine2.JI_CEI = entryInstruction2.PK;
		invoiceLine2.JI_Weight = 500;
		invoiceLine2.JI_NetWeight = 600;
		invoiceLine2.JI_LinePrice = 101.20;

		Factory.Save();
		return declaration;
	}

	//public void TestSendingAmendmentsCreatesNewMessageForComparison()
	//{
	//	var entry1 = WrapperTestHelper.GetEntryHeaderForTest(Factory);
	//	var declaration = entry1.Declaration;
	//	declaration.ZG_Gateway = "DMS";
	//	declaration.JE_ApplicationCode = "NLC";
	//	declaration.JE_MessageSubType = "IM";

	//	declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
	//	var cei = declaration.CustomsEntryInstructions.AddNew();
	//	cei.CEI_Style = "H1";

	//	var password = Factory.New<GlbExternalPassword>();
	//	password.GP_GC = declaration.CompanyPK;
	//	password.GP_ExpiryDate = new CargoWise.Types.ZDate(2015, 9, 1);
	//	password.GP_IssueDate = new CargoWise.Types.ZDate(2014, 3, 20);

	//	declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
	//	entry1.CH_CEI_Instruction = cei.PK;
	//	var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
	//	var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
	//	var sender = new JobDeclarationMessageSender(decWrapper);

	//	JobDeclarationMessageSendingObject sendingobject = decWrapper.SendingObjectsCollection[0];
	//	sendingobject.ShouldSend = true;
	//	sendingobject.MessageType = SendMessageTypes.Codes.DEC;

	//	var invoiceLines = entry1.InvoiceLines;
	//	foreach (JobComInvoiceLine invoiceLine in invoiceLines)
	//	{
	//		invoiceLine.CusSupplyChainActorReferences.RemoveAndDeleteAll();
	//	}

	//	sender.Send(shutUp);

	//	AssertEquals(2, entry1.Messages.Count);
	//	AssertEquals(NLConstants.EdiMessageTypes.DMS, entry1.Messages[1].EM_MessageType);
	//	AssertEquals(SendMessageTypes.Codes.DEC, entry1.Messages[1].EM_MessageSubType);

	//	declaration.PreviousDocuments.AddNew().CSI_Code = "123";
	//	declaration.JE_TotalNoOfPacks = 10;

	//	entry1.MovementReferenceNumberSetter("mrn123", ZDateTime.BrettsBirthday);
	//	Factory.Save();

	//	decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
	//	sendingobject = decWrapper.SendingObjectsCollection[0];
	//	sendingobject.MessageType = SendMessageTypes.Codes.AMD;
	//	sendingobject.TypeOfMessage = SendMessageTypes.Codes.AMD;
	//	sendingobject.ShouldSend = true;
	//	sender = new JobDeclarationMessageSender(decWrapper);
	//	sender.Send(shutUp);
	//	AssertEquals(4, entry1.Messages.Count);
	//	AssertEquals(NLConstants.EdiMessageTypes.DMS, entry1.Messages[2].EM_MessageType);
	//	AssertEquals(SendMessageTypes.Codes.NAM, entry1.Messages[2].EM_MessageSubType);
	//	AssertEquals(NLConstants.EdiMessageTypes.DMS, entry1.Messages[3].EM_MessageType);
	//	AssertEquals(SendMessageTypes.Codes.AMD, entry1.Messages[3].EM_MessageSubType);

	//	AssertEquals("NLC", entry1.Messages[2].EM_ApplicationCode);
	//	AssertEquals("NLC", entry1.Messages[3].EM_ApplicationCode);

	//	AssertEquals(FormattableString.Invariant($"This is a snapshot of the state of your entry at {entry1.Messages[2].EM_SystemCreateTimeUtc} UTC, which was used to create an amendment/CRI request.  The details of what was sent to DMS should be viewed on the AMD/CRI message."), entry1.Messages[2].EM_MessageInterpretation);
	//	AssertNull("There should be no warnings.", shutUp.Warning);
	//}

	//public void TestWarnings()
	//{
	//	var entry1 = WrapperTestHelper.GetEntryHeaderForTest(Factory);
	//	var declaration = entry1.Declaration;
	//	declaration.ZG_Gateway = "DMS";
	//	declaration.JE_ApplicationCode = "NLC";
	//	declaration.JE_MessageSubType = "IM";

	//	declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
	//	var cei = declaration.CustomsEntryInstructions.AddNew();
	//	cei.CEI_Style = "H1";

	//	var password = Factory.New<GlbExternalPassword>();
	//	password.GP_GC = declaration.CompanyPK;
	//	password.GP_ExpiryDate = new CargoWise.Types.ZDate(2015, 9, 1);
	//	password.GP_IssueDate = new CargoWise.Types.ZDate(2014, 3, 20);

	//	declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
	//	entry1.CH_CEI_Instruction = cei.PK;
	//	var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
	//	var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
	//	var sender = new JobDeclarationMessageSender(decWrapper);

	//	JobDeclarationMessageSendingObject sendingobject = decWrapper.SendingObjectsCollection[0];
	//	sendingobject.ShouldSend = true;
	//	sendingobject.MessageType = SendMessageTypes.Codes.DEC;

	//	var invoiceLines = entry1.InvoiceLines;
	//	foreach (JobComInvoiceLine invoiceLine in invoiceLines)
	//	{
	//		invoiceLine.CusSupplyChainActorReferences.RemoveAndDeleteAll();
	//	}

	//	sender.Send(shutUp);

	//	entry1.Messages[1].EM_MessageText = MessageGeneratorTestHelper.GetExpectedMessageXML("Enterprise.Customs.NL.Business.Testing.Message.Outgoing.MessageSending.TestFiles.WarningsInitialMessage.xml");

	//	decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
	//	sendingobject = decWrapper.SendingObjectsCollection[0];
	//	sendingobject.MessageType = SendMessageTypes.Codes.AMD;
	//	sendingobject.TypeOfMessage = SendMessageTypes.Codes.AMD;
	//	sendingobject.ShouldSend = true;
	//	sender = new JobDeclarationMessageSender(decWrapper);
	//	sender.Send(shutUp);
	//	AssertContains("The XML's should be too complex to compare because in the original message, the comminicationMetaData is re-positioned", "The following entries cannot be amended, please cancel and re-submit.  Please refer to learning unit 1BGB048", shutUp.Warning);
	//	AssertContains("The App. ref. ID should be mentioned", "EH00001", shutUp.Warning);

	//	entry1.Messages[1].EM_MessageText = MessageGeneratorTestHelper.GetExpectedMessageXML("Enterprise.Customs.NL.Business.Testing.Message.Outgoing.MessageSending.TestFiles.WarningsNoDifferences.xml");
	//	sendingobject.ShouldSend = true;
	//	sender.Send(shutUp);
	//	AssertNotContains("No message on 'no differences' because as some elements always need to be sent, there will always be changes.", "The following entries cannot be amended as they have no differences.", shutUp.Warning);
	//	AssertContains("There should be an error because there are no other entries left.", "There are no other entries selected to be sent", shutUp.Warning);
	//	AssertContains("The App. ref. ID should be mentioned", "EH00001", shutUp.Warning);
	//}
}

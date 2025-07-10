using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationCanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessagesTest : TestCaseWithFactory
{
	public void TestCanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessagesForImport()
	{
		TestCanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages("IMP");
	}

	public void TestCanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessagesWithNbMessages()
	{
		SetupDeclaration();
		Factory.Save();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			var invoice = declaration.Invoices.Single();
			var invoiceLine1 = invoice.InvoiceLines.Cast<JobComInvoiceLine>().Single();
			var entryInstruction = declaration.CustomsEntryInstructions.Single<CusEntryInstruction>();
			var entryHeader = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Single();
			var entryLine1 = entryHeader.MergedLines.Cast<CusEntryLine>().Single();

			PreviousDocumentHelperTest.AddPreviousDocumentsInOrderToGetNbMessages(invoiceLine1);

			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Tariff = "81";
			invoiceLine2.JI_Description = "DESCRIPTION";
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			declaration.ResetApportionedPreviousDocuments();
			PreviousDocumentHelperTest.AddPreviousDocumentsInOrderToGetNbMessages(invoiceLine2);

			AssertEquals("Entry is a draft -> Can I save the declaration?", ContinueWithSave.Yes, declaration.CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages());

			entryHeader.CH_Status = "ACO";
			entryHeader.CH_EntryStatus = "REG";
			entryLine1.ZG_NBStatus = "NBA";
			entryLine2.ZG_NBStatus = "NBA";
			Factory.Save();

			invoiceLine2.JI_Description = "DESCRIPTION EDITED";
			AssertEquals("[CH_EntryStatus: Registered, All NB Lines are Approved] Changes affect sent NB message, Can I save the declaration?",
				ContinueWithSave.No,
				declaration.CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages());

			invoiceLine2.JI_Description = "DESCRIPTION";
			entryHeader.CH_EntryStatus = "NBR";
			entryLine2.ZG_NBStatus = "NBR";
			Factory.Save();

			invoiceLine2.PreviousDocuments.Cast<PreviousDocument>().First().CSI_Quantity = 150;
			AssertEquals("[CH_EntryStatus: Nb Rejected, One NB Approved - One Rejected] Changes affect rejected NB Message, Can I save the declaration?",
				ContinueWithSave.Yes,
				declaration.CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages());
		}
	}

	public void TestCanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessagesForInterfacedDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_Status = ITMessageStatusList.Codes.AwaitingOriginal;
		entryHeader.MergedLines.AddNew();
		Factory.Save();
		AssertNull("PRE-CONDITION", entryHeader.EntryInstruction);
		AssertEquals("Can Save?", ContinueWithSave.Yes, declaration.CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages());
	}

	public void TestCanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessagesWhenInvoiceLineIsNoLongerLinkedToEntryInstruction()
	{
		SetupDeclaration();
		Factory.Save();
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			AssertEquals("No changes -> Can I save the declaration?", ContinueWithSave.Yes, declaration.CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages());

			var invoiceLine = declaration.InvoiceLines[0];
			var entryHeader = declaration.CustomsEntryHeaders[0];

			entryHeader.CH_Status = "AWO";
			Factory.Save();

			invoiceLine.JI_Description = "DESCRIPTION TEST";
			AssertEquals("[CH_Status = AWO] Changes affect sent message -> Can I save the declaration?", ContinueWithSave.No, declaration.CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages());
			Factory.Save();

			AssertEquals("No changes -> Can I save the declaration?", ContinueWithSave.Yes, declaration.CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages());
			invoiceLine.JI_CEI = ZGuid.Empty;
			declaration.DoMerge();
			AssertEquals("Invoice Line is no longer linked to entry instruction -> Can I save the declaration?", ContinueWithSave.No, declaration.CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages());
		}
	}

	public void TestCanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessagesAfterIvistoRequest()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			SetupDeclaration("EXP");
			var invoiceLine = declaration.InvoiceLines[0];
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.EntryInstruction.CEI_Style = "B1";
			entryHeader.CH_Status = "CLO";
			entryHeader.CH_EntryStatus = "ECC";
			Factory.Save();

			var ivistoMessageFactory = new IvistoRequestMessageFactory();
			ivistoMessageFactory.CreateIvistoRequestMessage(entryHeader, entryHeader.Factory);

			entryHeader.SetAsAmending();
			AssertEquals("Can save after amendment", ContinueWithSave.Yes, declaration.CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages());
			Factory.Save();

			invoiceLine.JI_Description = "NEW DESCRIPTION";
			AssertEquals("Can save after change description as in amendment status", ContinueWithSave.Yes, declaration.CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages());
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.DisableDefaultPackingInformation = true;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
	}
	JobDeclaration declaration;

	void TestCanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages(string messageType)
	{
		SetupDeclaration(messageType);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			var invoice = declaration.Invoices.Single();
			var invoiceLine1 = invoice.InvoiceLines.Cast<JobComInvoiceLine>().Single();
			var entryHeader = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Single();

			AssertEquals("No changes -> Can I save the declaration?", ContinueWithSave.Yes, declaration.CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages());

			invoiceLine1.JI_Description = "DESCRIPTION EDITED";
			AssertEquals("[CH_Status = Empty] No changes -> Can I save the declaration?", ContinueWithSave.Yes, declaration.CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages());

			entryHeader.CH_Status = "AWO";
			invoiceLine1.JI_Description = "DESCRIPTION";
			Factory.Save();

			invoiceLine1.JI_Description = "DESCRIPTION EDITED";
			AssertEquals("[CH_Status = AWO] Changes affect sent message -> Can I save the declaration?", ContinueWithSave.No, declaration.CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages());

			invoiceLine1.JI_Description = "DESCRIPTION";
			AssertEquals("[CH_Status = Awaiting Response] No changes -> Can I save the declaration?", ContinueWithSave.Yes, declaration.CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages());

			entryHeader.CH_Status = "ACO";
			entryHeader.CH_EntryStatus = "ICC";
			invoiceLine1.JI_Description = "DESCRIPTION EDITED";
			AssertEquals("[CH_EntryStatus = Accepted, CH_EntryStatus = Cleared] Changes affect sent message -> Can I save the declaration?",
				ContinueWithSave.No,
				declaration.CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages());

			entryHeader.CH_EntryStatus = "FTT";
			AssertEquals("[CH_EntryStatus = Awaiting Response, CH_EntryStatus = Failure To Sent], Changes affect sent message -> Can I save the declaration?",
				ContinueWithSave.Yes,
				declaration.CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages());

			entryHeader.CH_EntryStatus = "ICC";
			invoiceLine1.JI_Description = "DESCRIPTION";
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine2 = entryHeader2.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Description = "TEST-LINE2";
			invoiceLine2.JI_Tariff = "00";
			invoiceLine2.JI_CustomsQuantity = 100;
			invoiceLine2.JI_LinePrice = 1000;

			entryHeader2.CH_Status = "";
			AssertEquals("[New entry header CH_Status = Empty] Changes don't affect sent declaration -> Can I save the declaration?",
				ContinueWithSave.Yes,
				declaration.CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages());
		}
	}

	void SetupDeclaration(string messageType = null)
	{
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = messageType ?? "IMP";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = "40";
		invoiceLine1.JI_Description = "DESCRIPTION";
		invoiceLine1.JI_Tariff = "80";
		invoiceLine1.JI_CustomsQuantity = 100;
		invoiceLine1.JI_LinePrice = 1000;
		invoiceLine1.JI_CEI = entryInstruction.PK;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_Status = "";
		entryHeader.CH_EntryStatus = "";
	}
}

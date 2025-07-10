using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ITMessageChangedStatusDeterminerToDictateWhetherSavingAllowedTest : TestCaseWithFactory
{
	public void TestMakeMessagesOnThisBizoForComparisonReturnsEdifactString()
	{
		var expectedMessageText = "TIM           <<MSGNO PLACEHOLDER>>00	 							0	IM";
		AssertMessagesOnBizoComparisonReturns(isUcc6: false, expectedMessageText);
	}

	public void TestMakeMessagesOnThisBizoForComparisonReturnsEdifactStringWithNBMessages()
	{
		var declaration = SetupDeclaration();
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			var entryHeader = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Single();
			var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().Single();
			var invoiceHeader = declaration.Invoices.Cast<JobComInvoiceHeader>().Single();
			var invoiceLine = invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>().Single();

			PreviousDocumentHelperTest.AddPreviousDocumentsInOrderToGetNbMessages(invoiceLine);
			declaration.ResetApportionedPreviousDocuments();

			entryHeader.CH_Status = "ACO";
			entryHeader.CH_EntryStatus = "NBR";
			entryLine.ZG_NBStatus = "NBR";

			var ediMessages = GetEDIMessageForComparison(declaration);
			AssertEquals("[CH_Status = Accepted, CH_EntryStatus = NB Rejected] -> EDIMessages count for comparison", 1, ediMessages.Length);
			var messageText = ediMessages.Single().EM_MessageText;
			CombineAssertions("ZG_NBStatus = NB Rejected, EM_MessageText", () =>
			{
				AssertStartsWith("EM_MessageText", "TIM           <<MSGNO PLACEHOLDER>>00	 							0	IM", messageText);
				AssertNotContains("EM_MessageText", "NB ", messageText);
			});

			entryHeader.CH_Status = "ACO";
			entryHeader.CH_EntryStatus = "ICC";
			entryLine.ZG_NBStatus = "NBA";

			ediMessages = GetEDIMessageForComparison(declaration);
			AssertEquals("[CH_Status = Accepted, CH_EntryStatus = Cleared] -> EDIMessages count for comparison", 1, ediMessages.Length);
			messageText = ediMessages.Single().EM_MessageText;
			CombineAssertions("ZG_NBStatus = NB Approved, EM_MessageText", () =>
			{
				AssertStartsWith("EM_MessageText", "TIM           <<MSGNO PLACEHOLDER>>00	 							0	IM", messageText);
				AssertContains("EM_MessageText", "TNB           <<MSGNO PLACEHOLDER>>01IM	<<MSGNO PLACEHOLDER>>			0	A3", messageText);
			});

			entryHeader.CH_EntryStatus = "NBR";
			entryLine.ZG_NBStatus = "NBS";
			ediMessages = GetEDIMessageForComparison(declaration);
			AssertEquals("[CH_Status = Accepted, CH_EntryStatus = NB Rejected] -> EDIMessages count for comparison", 1, ediMessages.Length);
			messageText = ediMessages.Single().EM_MessageText;
			CombineAssertions("ZG_NBStatus = NB Sent, EM_MessageText", () =>
			{
				AssertStartsWith("EM_MessageText", "TIM           <<MSGNO PLACEHOLDER>>00	 							0	IM", messageText);
				AssertContains("EM_MessageText", "TNB           <<MSGNO PLACEHOLDER>>01IM	<<MSGNO PLACEHOLDER>>			0	A3", messageText);
			});

			//Adding another NB Message
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			declaration.ResetApportionedPreviousDocuments();
			PreviousDocumentHelperTest.AddPreviousDocumentsInOrderToGetNbMessages(invoiceLine2);
			entryLine2.ZG_NBStatus = "NBR";

			ediMessages = GetEDIMessageForComparison(declaration);
			AssertEquals("[CH_Status = Accepted, CH_EntryStatus = NB Rejected] -> EDIMessages count for comparison", 1, ediMessages.Length);
			messageText = ediMessages.Single().EM_MessageText;

			CombineAssertions("[EntryLine1.ZG_NBStatus = NB Sent, EntryLine2.ZG_NBStatus = NB Rejeted] Check EM_MessageText for comparison", () =>
			{
				AssertStartsWith("EM_MessageText", "TIM           <<MSGNO PLACEHOLDER>>00	 							0	IM", messageText);
				AssertContains("EM_MessageText", "TNB           <<MSGNO PLACEHOLDER>>01IM	<<MSGNO PLACEHOLDER>>			0	A3", messageText);
				AssertNotContains("EM_MessageText", "TNB           <<MSGNO PLACEHOLDER>>02IM	<<MSGNO PLACEHOLDER>>			0	A3", messageText);
			});
		}
	}

	public void TestMakeMessagesOnThisBizoForComparisonReturns_WhenDeclarationIsUcc6()
	{
		var expectedMessageText = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Messaggio>\r\n  <DichiarazioneH1>";
		AssertMessagesOnBizoComparisonReturns(isUcc6: true, expectedMessageText, instructionStyle: "H1");
	}

	public void TestMakeMessagesOnThisBizoForComparisonReturns_WhenMultipleEntriesOneMarkedAsAmendment()
	{
		var declaration = SetupDeclaration();
		var invoice = declaration.Invoices[0];
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		var entry1 = declaration.CustomsEntryHeaders[0];
		var entry2 = declaration.CustomsEntryHeaders.AddNew();
		var entryLine2 = entry2.MergedLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;

		entry1.CH_EntryStatus = "ICC";
		entry1.CH_Status = "ACO";
		entry2.CH_EntryStatus = "ICC";
		entry2.CH_Status = "ACO";
		Factory.Save();

		entry2.SetAsAmending();
		invoiceLine2.JI_Description = "NEW DESCRIPTION";
		AssertEquals("Can save after amendment",
			ContinueWithSave.Yes,
			declaration.CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages());
	}

	JobDeclaration SetupDeclaration(string instructionStyle = null)
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_CustomsProfile = "1234";
		declaration.JE_GS_NKCusAgent = "BBB";
		declaration.JE_CustomsOffice = "IT000000";
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = instructionStyle;

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_CL = entryLine.PK;
		entryHeader.CH_BGMReference = "A0001";

		Factory.Save();
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		var declarantTaxNumber = "11111111111";
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, declarantTaxNumber, currentValue: 1);
		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount(declarantTaxNumber + "-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();

		Factory.Save();
		return declaration;
	}

	EDIMessage[] GetEDIMessageForComparison(JobDeclaration declaration)
	{
		var messageChangedStatusDeterminer = new ITMessageChangedStatusDeterminerToDictateWhetherSavingAllowed(declaration);
		return messageChangedStatusDeterminer.MakeMessagesOnThisBizoForComparison(declaration);
	}

	void AssertMessagesOnBizoComparisonReturns(bool isUcc6, ZString expectedMessageText, string instructionStyle = null)
	{
		var declaration = SetupDeclaration(instructionStyle);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUcc6))
		{
			var entryHeader = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Single();

			entryHeader.CH_Status = "";
			entryHeader.CH_EntryStatus = "";
			var changedStatusDeterminer = new ITMessageChangedStatusDeterminerToDictateWhetherSavingAllowed(declaration);
			AssertEquals("No EDI Messages to compare are expected when the declaration is a draft -> EDIMessages count", 0, changedStatusDeterminer.MakeMessagesOnThisBizoForComparison(declaration).Length);

			entryHeader.CH_Status = "AWO";
			var ediMessages = GetEDIMessageForComparison(declaration);
			AssertEquals("[CH_Status = AWO] -> EDIMessages count", 1, ediMessages.Length);
			AssertStartsWith("EM_MessageText", expectedMessageText, ediMessages.Single().EM_MessageText);

			entryHeader.CH_Status = "ACO";
			entryHeader.CH_EntryStatus = "ICC";
			ediMessages = GetEDIMessageForComparison(declaration);
			AssertEquals("[CH_Status = ACO, CH_EntryStatus = ICC] -> EDIMessages count", 1, ediMessages.Length);
			AssertStartsWith("EM_MessageText", expectedMessageText, ediMessages.Single().EM_MessageText);
		}
	}
}

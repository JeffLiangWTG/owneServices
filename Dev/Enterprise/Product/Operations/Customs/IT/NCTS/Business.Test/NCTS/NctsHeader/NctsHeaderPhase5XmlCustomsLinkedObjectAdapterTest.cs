using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsHeaderPhase5XmlCustomsLinkedObjectAdapterTest : XmlCustomsLinkedObjectAdapterTest<NctsHeader>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When header is null", () => new NctsHeaderPhase5CustomsLinkedObjectAdapter(null));
		AssertExceptionThrown<ArgumentNullException>("When header without MovementHeader", () => new NctsHeaderPhase5CustomsLinkedObjectAdapter(Factory.New<NctsHeader>()));
	}

	public override void TestIsAwaitingMessage()
	{
		AssertEquals("When BM_MessageStatus is not SNT", false, Adapter.IsAwaitingMessage);

		Adaptee.MovementHeader.BM_MessageStatus = "SNT";
		AssertEquals("When BM_MessageStatus is SNT", true, Adapter.IsAwaitingMessage);
	}

	public override void TestIsDeposited()
	{
		AssertEquals(nameof(Adapter.IsDeposited), false, Adapter.IsDeposited);
	}

	public override void TestSetStatusAsError()
	{
		const string unlockForEditLogReference = "Response message error";
		var movementHeader = Adaptee.MovementHeader;
		AssertEquals("[PRE-CONDITION] BM_MessageStatus", "", movementHeader.BM_MessageStatus);
		AssertEquals("[PRE-CONDITION] IsLocked", expected: false, Adaptee.IsLocked);

		Adapter.SetStatusAsError();
		var unlockForEditLog = Adaptee.Logs.MostRecentLogByEventTime(Events.UnlockForEdit);

		AssertEquals(nameof(movementHeader.BM_MessageStatus), "ERR", movementHeader.BM_MessageStatus);
		AssertEquals(nameof(Adaptee.IsLocked), expected: false, Adaptee.IsLocked);
		AssertNull("Unlock log", unlockForEditLog);

		movementHeader.BM_MessageStatus = "SNT";
		Adapter.SetStatusAsError();
		unlockForEditLog = Adaptee.Logs.MostRecentLogByEventTime(Events.UnlockForEdit);

		AssertEquals(nameof(movementHeader.BM_MessageStatus), "ERR", movementHeader.BM_MessageStatus);
		AssertEquals(nameof(Adaptee.IsLocked), expected: false, Adaptee.IsLocked);
		AssertNull("Unlock log", unlockForEditLog);

		Adaptee.LockFile(string.Empty);
		movementHeader.BM_MessageStatus = string.Empty;

		AssertEquals("[PRE-CONDITION] BM_MessageStatus", "", movementHeader.BM_MessageStatus);
		AssertEquals("[PRE-CONDITION] IsLocked", expected: true, Adaptee.IsLocked);

		Adapter.SetStatusAsError();
		unlockForEditLog = Adaptee.Logs.MostRecentLogByEventTime(Events.UnlockForEdit);

		AssertEquals(nameof(movementHeader.BM_MessageStatus), "ERR", movementHeader.BM_MessageStatus);
		AssertEquals(nameof(Adaptee.IsLocked), expected: false, Adaptee.IsLocked);
		AssertNotNull("Unlock log", unlockForEditLog);
		AssertEquals("Unlock log ReferenceFreeText", unlockForEditLogReference, unlockForEditLog.ReferenceFreeText);

		movementHeader.BM_MessageStatus = "SNT";
		Adaptee.Logs.RemoveAndDeleteAll();
		Adaptee.LockFile(string.Empty);

		AssertEquals("[PRE-CONDITION] IsLocked", expected: true, Adaptee.IsLocked);

		Adapter.SetStatusAsError();
		unlockForEditLog = Adaptee.Logs.MostRecentLogByEventTime(Events.UnlockForEdit);

		AssertEquals(nameof(movementHeader.BM_MessageStatus), "ERR", movementHeader.BM_MessageStatus);
		AssertEquals(nameof(Adaptee.IsLocked), expected: false, Adaptee.IsLocked);
		AssertNotNull("Unlock log", unlockForEditLog);
		AssertEquals("Unlock log ReferenceFreeText", unlockForEditLogReference, unlockForEditLog.ReferenceFreeText);
	}

	public override void TestCustomsOfficeOfPresentation()
	{
		AssertNullOrEmpty("[PRE-CONDITION] CustomsOfficeOfPresentation", Adapter.CustomsOfficeOfPresentation);

		const string expectedCustomsOfficeOfPresentation = "IT279100";
		var customsOfficeOfPresentation = Adaptee.MovementHeader.CustomsOffices.AddNew();
		customsOfficeOfPresentation.CY_Code = "DEP";
		customsOfficeOfPresentation.CY_Data = expectedCustomsOfficeOfPresentation;

		AssertEquals("CustomsOfficeOfPresentation should match the expected value", expectedCustomsOfficeOfPresentation, Adapter.CustomsOfficeOfPresentation);
	}

	public override void TestGetAllEntryLines()
	{
		var allEntryLines = Adapter.GetAllEntryLines().ToList();
		AssertNotNull("The list of entry lines should not be null", allEntryLines);
		AssertEquals("The count of entry lines should be 0 initially", 0, allEntryLines.Count);

		var goodsItem = Adaptee.Bills.AddNew().GoodsItems.AddNew();
		allEntryLines = Adapter.GetAllEntryLines().ToList();
		AssertNotNull("The list of entry lines should not be null after adding a goods item", allEntryLines);
		AssertEquals("The count of entry lines should be 1 after adding a goods item", 1, allEntryLines.Count);
		AssertSame("The retrieved goods item should be the same as the first entry line", goodsItem, allEntryLines[0]);
	}

	public override void TestSetStatusAsCleared()
	{
		var movementHeader = Adaptee.MovementHeader;
		CombineAssertions("[PRE-CONDITION]", () =>
		{
			AssertEquals("[PRE-CONDITION] BM_CustomsStatus", "", movementHeader.BM_CustomsStatus);
			AssertEquals("[PRE-CONDITION] BM_MessageStatus", "", movementHeader.BM_MessageStatus);
		});

		Adapter.SetStatusAsCleared(ZDateTime.Now);
		CombineAssertions(() =>
		{
			AssertEquals("BM_CustomsStatus", "REL", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "ACC", movementHeader.BM_MessageStatus);
		});

		movementHeader.BM_CustomsStatus = "ACS";
		Adapter.SetStatusAsCleared(ZDateTime.Now);
		CombineAssertions(() =>
		{
			AssertEquals("BM_CustomsStatus", "ACS", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "ACC", movementHeader.BM_MessageStatus);
		});

		movementHeader.BM_Phase = "014";
		movementHeader.BM_CustomsStatus = "";
		movementHeader.BM_MessageStatus = "SNT";
		Adapter.SetStatusAsCleared(ZDateTime.Now);
		CombineAssertions(() =>
		{
			AssertEquals("BM_CustomsStatus", "", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "SNT", movementHeader.BM_MessageStatus);
		});
	}

	public override void TestSetStatusAsRegistered()
	{
		var movementHeader = Adaptee.MovementHeader;
		var acceptanceDate = ZDateTime.BrettsBirthday;

		CombineAssertions("[PRE-CONDITION]", () =>
		{
			AssertEquals("BM_CustomsStatus", "", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "", movementHeader.BM_MessageStatus);
			AssertEquals("BM_EntryDate", ZDateTime.Empty, movementHeader.BM_EntryDate);
		});

		Adapter.SetStatusAsRegistered(acceptanceDate);
		CombineAssertions(() =>
		{
			AssertEquals("BM_CustomsStatus", "MRN", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "ACC", movementHeader.BM_MessageStatus);
			AssertEquals("When BM_EntryDate was originally empty", acceptanceDate, movementHeader.BM_EntryDate);
		});

		movementHeader.BM_CustomsStatus = "REL";
		Adapter.SetStatusAsRegistered(acceptanceDate);
		CombineAssertions(() =>
		{
			AssertEquals("BM_CustomsStatus", "REL", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "ACC", movementHeader.BM_MessageStatus);
			AssertEquals("BM_EntryDate", acceptanceDate, movementHeader.BM_EntryDate);
		});

		Adapter.SetStatusAsRegistered(ZDateTime.Today);
		CombineAssertions(() =>
		{
			AssertEquals("BM_CustomsStatus", "REL", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "ACC", movementHeader.BM_MessageStatus);
			AssertEquals("BM_EntryDate is not empty", acceptanceDate, movementHeader.BM_EntryDate);
		});
	}

	public override void TestSetStatusAsCancelled()
	{
		var movementHeader = Adaptee.MovementHeader;
		AssertEquals("[PRE-CONDITION] BM_CustomsStatus", "", movementHeader.BM_CustomsStatus);
		AssertEquals("[PRE-CONDITION] BM_MessageStatus", "", movementHeader.BM_MessageStatus);

		Adapter.SetStatusAsCancelled();
		CombineAssertions(() =>
		{
			AssertEquals("BM_CustomsStatus", "CAN", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "ACC", movementHeader.BM_MessageStatus);
		});
	}

	public override void TestSetStatusAsAcceptedBySystem()
	{
		var movementHeader = Adaptee.MovementHeader;
		AssertEquals("[PRE-CONDITION] BM_CustomsStatus", "", movementHeader.BM_CustomsStatus);
		AssertEquals("[PRE-CONDITION] BM_MessageStatus", "", movementHeader.BM_MessageStatus);

		Adapter.SetStatusAsAcceptedBySystem();
		CombineAssertions("[POST_CONDITION]", () =>
		{
			AssertEquals("BM_CustomsStatus", "ACS", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "ACC", movementHeader.BM_MessageStatus);
		});

		movementHeader.BM_CustomsStatus = "CAN";
		Adapter.SetStatusAsAcceptedBySystem();
		CombineAssertions(() =>
		{
			AssertEquals("BM_CustomsStatus", "CAN", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "ACC", movementHeader.BM_MessageStatus);
		});
	}

	public override void TestUpdateOrInsertEntryNumber()
	{
		var entryNumbers = Adapter.GetAllRelatedEntryNumbers();
		AssertEquals("PRE-CONDITION", 0, entryNumbers.Count());

		var entryNumber = Adapter.UpdateOrInsertEntryNumber("REG",
			"123",
			"TEST",
			ZDateTime.Now,
			null);

		var insertedEntry = CusEntryNumber.Load(Adaptee, "REG", "IT");
		AssertNotNull(nameof(CusEntryNumber), entryNumber);
		AssertNotNull(nameof(CusEntryNumber), insertedEntry);
		AssertEquals(nameof(CusEntryNumber.CE_EntryNum), insertedEntry.CE_EntryNum, entryNumber.CE_EntryNum);

		Adapter.UpdateOrInsertEntryNumber("REG",
			"345",
			"TEST",
			ZDateTime.Now,
			null);

		var regEntry = CusEntryNumber.Load(Adaptee, "REG", "IT");
		AssertEquals(nameof(CusEntryNumber.CE_EntryNum), "345", regEntry.CE_EntryNum);
	}

	public override void TestGetAllRelatedEntryNumbers()
	{
		var entryNumbers = Adapter.GetAllRelatedEntryNumbers();
		AssertEquals("PRE-CONDITION", 0, entryNumbers.Count());

		Adapter.UpdateOrInsertEntryNumber("REG",
			"123",
			"TEST",
			ZDateTime.Now,
			null);

		entryNumbers = Adapter.GetAllRelatedEntryNumbers();
		AssertEquals("PRE-CONDITION", 1, entryNumbers.Count());
	}

	public override void TestSetStatusAsAcknowledged()
	{
		var movementHeader = Adaptee.MovementHeader;
		AssertEquals("[PRE-CONDITION] BM_CustomsStatus", "", movementHeader.BM_CustomsStatus);
		AssertEquals("[PRE-CONDITION] BM_MessageStatus", "", movementHeader.BM_MessageStatus);

		Adapter.SetStatusAsAcknowledged();
		CombineAssertions("[POST_CONDITION]", () =>
		{
			AssertEquals("BM_CustomsStatus", "ACK", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "ACC", movementHeader.BM_MessageStatus);
		});

		movementHeader.BM_CustomsStatus = "MRN";
		Adapter.SetStatusAsAcknowledged();
		CombineAssertions(() =>
		{
			AssertEquals("BM_CustomsStatus", "MRN", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "ACC", movementHeader.BM_MessageStatus);
		});

		movementHeader.BM_Phase = "014";
		movementHeader.BM_CustomsStatus = "";
		movementHeader.BM_MessageStatus = "SNT";
		Adapter.SetStatusAsAcknowledged();
		CombineAssertions(() =>
		{
			AssertEquals("BM_CustomsStatus", "ACK", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "ACC", movementHeader.BM_MessageStatus);
		});
	}

	public override void TestSetStatusAsUnderControl()
	{
		var movementHeader = Adaptee.MovementHeader;
		CombineAssertions("[PRE-CONDITION]", () =>
		{
			AssertEquals("BM_CustomsStatus", "", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "", movementHeader.BM_MessageStatus);
		});

		Adapter.SetStatusAsUnderControl();
		CombineAssertions(() =>
		{
			AssertEquals("BM_CustomsStatus", "CO3", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "ACC", movementHeader.BM_MessageStatus);
		});

		movementHeader.BM_CustomsStatus = "ACS";
		Adapter.SetStatusAsUnderControl();
		CombineAssertions(() =>
		{
			AssertEquals("BM_CustomsStatus", "ACS", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "ACC", movementHeader.BM_MessageStatus);
		});

		movementHeader.BM_Phase = "014";
		movementHeader.BM_CustomsStatus = "";
		movementHeader.BM_MessageStatus = "SNT";
		Adapter.SetStatusAsUnderControl();
		CombineAssertions(() =>
		{
			AssertEquals("BM_CustomsStatus", "", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "SNT", movementHeader.BM_MessageStatus);
		});
	}

	public override void TestSetStatusAsGoodsWrittenOffClosed()
	{
		var movementHeader = Adaptee.MovementHeader;
		CombineAssertions("[PRE-CONDITION]", () =>
		{
			AssertEquals("BM_CustomsStatus", "", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "", movementHeader.BM_MessageStatus);
		});

		Adapter.SetStatusAsGoodsWrittenOffClosed();
		CombineAssertions(() =>
		{
			AssertEquals("BM_CustomsStatus", "WRO", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "ACC", movementHeader.BM_MessageStatus);
		});

		movementHeader.BM_CustomsStatus = "ACS";
		Adapter.SetStatusAsGoodsWrittenOffClosed();
		CombineAssertions(() =>
		{
			AssertEquals("BM_CustomsStatus", "WRO", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "ACC", movementHeader.BM_MessageStatus);
		});

		movementHeader.BM_Phase = "014";
		movementHeader.BM_CustomsStatus = "";
		movementHeader.BM_MessageStatus = "SNT";
		Adapter.SetStatusAsGoodsWrittenOffClosed();
		CombineAssertions(() =>
		{
			AssertEquals("BM_CustomsStatus", "", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "SNT", movementHeader.BM_MessageStatus);
		});
	}

	public override void TestUpdateOrInsertIrildesEntryNumber()
	{
		var entryNumbers = Adapter.GetAllRelatedEntryNumbers();
		AssertEquals("PRE-CONDITION", 0, entryNumbers.Count());

		var now = DateTime.Now;

		Adapter.UpdateOrInsertIrildesEntryNumber("DEPOFFICE", now);

		var irildesEntryNumber = CusEntryNumber.Load(Adaptee, "IRI", "IT");
		AssertNotNull(nameof(CusEntryNumber), irildesEntryNumber);

		CombineAssertions(() =>
		{
			AssertEquals(nameof(CusEntryNumber.CE_EntryLineReference), "DEPOFFICE", irildesEntryNumber.CE_EntryLineReference);
			AssertEquals(nameof(CusEntryNumber.CE_IssueDate), now, irildesEntryNumber.CE_IssueDate);
			AssertEquals(nameof(CusEntryNumber.CE_EntryStatus), "WRO", irildesEntryNumber.CE_EntryStatus);
			AssertEquals(nameof(CusEntryNumber.CE_Category), "CUS", irildesEntryNumber.CE_Category);
		});

		var tomorrow = now.AddDays(1);

		Adapter.UpdateOrInsertIrildesEntryNumber("DEPOFFICE2", tomorrow);

		var irildesEntryNumberCollection = CusEntryNumber.Load(Factory, "IRI", "", "IT");
		AssertEquals("How many IRI EntryNumber?", 1, irildesEntryNumberCollection.Length);

		irildesEntryNumber = irildesEntryNumberCollection[0];

		CombineAssertions(() =>
		{
			AssertEquals(nameof(CusEntryNumber.CE_EntryLineReference), "DEPOFFICE2", irildesEntryNumber.CE_EntryLineReference);
			AssertEquals(nameof(CusEntryNumber.CE_IssueDate), tomorrow, irildesEntryNumber.CE_IssueDate);
		});
	}

	public override void TestSetStatusAsFailedForTransmission()
	{
		var movementHeader = Adaptee.MovementHeader;
		AssertEquals("[PRE-CONDITION] BM_MessageStatus", "", movementHeader.BM_MessageStatus);
		AssertEquals("[PRE-CONDITION] IsLocked", expected: false, Adaptee.IsLocked);

		Adapter.SetStatusAsFailedForTransmission();
		var unlockForEditLog = Adaptee.Logs.MostRecentLogByEventTime(Events.UnlockForEdit);
		AssertEquals("[POST-CONDITION] BM_MessageStatus", "FAL", movementHeader.BM_MessageStatus);
		AssertEquals("[POST-CONDITION] IsLocked", expected: false, Adaptee.IsLocked);
		AssertNull("Unlock log", unlockForEditLog);

		Adaptee.LockFile(string.Empty);
		movementHeader.BM_MessageStatus = string.Empty;

		AssertEquals("[PRE-CONDITION] BM_MessageStatus", "", movementHeader.BM_MessageStatus);
		AssertEquals("[PRE-CONDITION] IsLocked", expected: true, Adaptee.IsLocked);

		Adapter.SetStatusAsFailedForTransmission();
		unlockForEditLog = Adaptee.Logs.MostRecentLogByEventTime(Events.UnlockForEdit);

		AssertEquals("[POST-CONDITION] BM_MessageStatus", "FAL", movementHeader.BM_MessageStatus);
		AssertEquals("[POST-CONDITION] IsLocked", expected: false, Adaptee.IsLocked);
		AssertNotNull("[POST-CONDITION] Unlock log", unlockForEditLog);
		AssertEquals("[POST-CONDITION] Unlock log ReferenceFreeText", "Response message error failed for transmission", unlockForEditLog.ReferenceFreeText);
	}

	public override void TestSetStatusAsDeposited()
	{
		AssertNoExceptionThrown(() => Adapter.SetStatusAsDeposited());
	}

	public override void TestSetStatusAsExitCompleted()
	{
		AssertNoExceptionThrown(() => Adapter.SetStatusAsExitCompleted());
	}

	public override void TestGetAllPaymentInfo()
	{
		AssertEquals(Array.Empty<CusEntryPayInfo>(), Adapter.GetAllPaymentInfo());
	}

	public override void TestSetStatusAsAmended()
	{
		var movementHeader = Adaptee.MovementHeader;
		var bill = Adaptee.Bills.AddNew();
		var goodsItem = bill.GoodsItems.AddNew();
		CombineAssertions("[PRE-CONDITION]", () =>
		{
			AssertEquals("MovementHeader.BM_CustomsStatus", "", movementHeader.BM_CustomsStatus);
			AssertEquals("MovementHeader.BM_MessageStatus", "", movementHeader.BM_MessageStatus);
			AssertEquals("MovementHeader.BM_Phase", "", movementHeader.BM_Phase);
			AssertEquals("Bills.B0_BillStatus", "", bill.B0_BillStatus);
			AssertEquals("GoodsItems.BY_Status", "", goodsItem.BY_Status);
		});

		var sessionGuid = ZGuid.NewZGuid();

		var sentMessage = movementHeader.Messages.AddNew();
		sentMessage.EM_ApplicationCode = "ITH";
		sentMessage.EM_MessageType = "NEW";
		sentMessage.EM_Status = "SNT";

		var sentInterchange = Factory.New<EDIInterchange>();
		sentInterchange.EI_SessionGUID = sessionGuid;
		sentMessage.EM_EI = sentInterchange.PK;

		var responseInterchange = Factory.New<EDIInterchange>();
		responseInterchange.EI_SessionGUID = sessionGuid;

		var responseMessage = movementHeader.Messages.AddNew();
		responseMessage.EM_MessageType = "IRR";
		responseMessage.EM_EI = responseInterchange.PK;
		responseMessage.EM_Status = "RCV";
		responseMessage.EM_MessageText = ManifestResourceHelper.ReadManifestResourceContent(NoClearanceResponseManifestResourceKey);

		Adapter.SetStatusAsAmended();

		CombineAssertions("When there are no RES messages related to last NEW message", () =>
		{
			AssertEquals("MovementHeader.BM_CustomsStatus: do not update when EM_MessageType is not RES", "", movementHeader.BM_CustomsStatus);
			AssertEquals("MovementHeader.BM_MessageStatus: do not update when EM_MessageType is not RES", "", movementHeader.BM_MessageStatus);
			AssertEquals("MovementHeader.BM_Phase: always update", "015", movementHeader.BM_Phase);
			AssertEquals("Bills.B0_BillStatus: do not update when B0_BillStatus is not DLR", "", bill.B0_BillStatus);
			AssertEquals("GoodsItems.BY_Status: do not update when BY_Status is not DLR", "", goodsItem.BY_Status);
		});

		responseMessage.EM_MessageType = "RES";
		movementHeader.BM_Phase = "013";
		bill.B0_BillStatus = "DLR";
		goodsItem.BY_Status = "DLR";

		Adapter.SetStatusAsAmended();

		CombineAssertions("When there are RES messages related to last NEW message", () =>
		{
			AssertEquals("MovementHeader.BM_CustomsStatus: update when EM_MessageType is RES", "CO3", movementHeader.BM_CustomsStatus);
			AssertEquals("MovementHeader.BM_MessageStatus: update when EM_MessageType is RES", "ACC", movementHeader.BM_MessageStatus);
			AssertEquals("MovementHeader.BM_Phase: always update", "015", movementHeader.BM_Phase);
			AssertEquals("Bills.B0_BillStatus: update when B0_BillStatus is DLR", "DEL", bill.B0_BillStatus);
			AssertEquals("GoodsItems.BY_Status: update when BY_Status is DLR", "DEL", goodsItem.BY_Status);
		});
	}

	public void TestIrildesAutomaticRequestWhenSetStatusAsCleared()
	{
		var movementHeader = Adaptee.MovementHeader;
		CombineAssertions("[PRE-CONDITION]", () =>
		{
			AssertEquals("[PRE-CONDITION] BM_CustomsStatus", "", movementHeader.BM_CustomsStatus);
			AssertEquals("[PRE-CONDITION] BM_MessageStatus", "", movementHeader.BM_MessageStatus);
		});

		Adapter.SetStatusAsCleared(ZDateTime.Now);
		CombineAssertions(() =>
		{
			AssertEquals("BM_CustomsStatus", "REL", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", "ACC", movementHeader.BM_MessageStatus);

			var irildesMessage = movementHeader.Messages.GetLastMessageByType("IRI");
			AssertNotNull("Irildes Message", irildesMessage);
		});
	}

	public override void TestAddA93Number()
	{
		AssertNoExceptionThrown(() => Adapter.AddA93Number(Mock.Of<IUcc6A93NumberPayment>()));
	}

	public override void TestGetAllFees()
	{
		AssertEquals(Array.Empty<IFee>(), Adapter.GetAllFees(feeFilter: null));
	}

	public override void TestGetFeesForLine()
	{
		AssertEquals(Array.Empty<IFee>(), Adapter.GetFeesForLine(0, feeFilter: null));
	}

	public override void TestSetCustomsChannel()
	{
		var movementHeader = Adaptee.MovementHeader;
		AssertEquals("Precondition", ZString.Empty, movementHeader.BM_ControlChannel);

		Adapter.SetCustomsChannel("CA");
		AssertEquals("CA", movementHeader.BM_ControlChannel);
	}

	public override void TestGetUniqueTransactionIdentifierRequestContext()
	{
		AssertType<NctsUniqueTransactionIdentifierRequestContext>("Type", Adapter.GetUniqueTransactionIdentifierRequestContext("1234", "ABCDEF123456"));
	}

	public override void TestGetLastSuccessfullySentMessageForDepositedStatus()
	{
		AssertNull(Adapter.GetLastSuccessfullySentMessageForDepositedStatus());
	}

	public override void TestUpdateOrInsertIvistoEntryNumber()
	{
		AssertNoExceptionThrown(() => Adapter.UpdateOrInsertIvistoEntryNumber(ZDateTime.Empty, ZString.Empty, ZString.Empty));
	}

	public override void TestGetOriginalSentMessageByUniqueTransactionIdentifier()
	{
		var movementHeader = Adaptee.MovementHeader;
		var sentMessage = movementHeader.Messages.AddNew();
		sentMessage.IsTransmitMessage = true;
		sentMessage.EM_Status = "SNT";
		var sentInterchange = Factory.New<EDIInterchange>();
		sentInterchange.ContainedMessages.Add(sentMessage);
		sentInterchange.EI_SessionGUID = new ZGuid("EADC205E-BFD7-44AD-B547-14C4B3C02177");

		var acknowledgmentMessage = movementHeader.Messages.AddNew();
		acknowledgmentMessage.EM_MessageType = "ACK";
		acknowledgmentMessage.EM_MessageText = "<IUT>20220307D11000328189</IUT>";
		var acknowledgmentInterchange = Factory.New<EDIInterchange>();
		acknowledgmentInterchange.ContainedMessages.Add(acknowledgmentMessage);
		acknowledgmentInterchange.EI_SessionGUID = new ZGuid("EADC205E-BFD7-44AD-B547-14C4B3C02177");

		CombineAssertions(() =>
		{
			AssertSame("When the passed IUT matches the ACK and a sent message", sentMessage, Adapter.GetOriginalSentMessageByUniqueTransactionIdentifier("20220307D11000328189"));
			AssertNull("When the passed IUT does not match an ACK nor a sent message", Adapter.GetOriginalSentMessageByUniqueTransactionIdentifier("ABCDEFGH"));
		});
	}

	public override void TestAddEDoc()
	{
		var documentData = new byte[] { 0x20, 0x40 };

		Adapter.AddEDoc(documentData, "Test12.pdf", "CLR");
		var eDocs = Adaptee.DocManagerInfo().AllEDocs;
		var uniqueEdoc = eDocs[0];

		AssertEquals("eDocs count", 1, eDocs.Count);
		AssertEquals("DocType is CLR", "CLR", uniqueEdoc.DocType);
		AssertEquals("FileName", "Test12.pdf", uniqueEdoc.FileName);
	}

	public override void TestCustomsProfile()
	{
		Adaptee.BH_CustomsProfile = "12345";
		AssertEquals(nameof(Adapter.CustomsProfile), "12345", Adapter.CustomsProfile);
	}

	public override void TestCreateIvistoRequestMessage()
	{
		AssertExceptionThrown<CustomsMessageProcessorException>(Adapter.CreateIvistoRequestMessage);
	}

	public override void TestCreateIrildesRequestMessage()
	{
		Adapter.CreateIrildesRequestMessage();
		Adaptee.MovementHeader.Messages.Reload(reLoadExistingRows: false);
		var irildesMessage = Adaptee.MovementHeader.Messages.GetLastMessageByType("IRI");
		AssertNotNull("IRI message", irildesMessage);
	}

	protected override NctsHeader GetAdaptee() => Factory.NewDepartureNctsHeaderPhase5();

	protected override IXmlCustomsLinkedObjectAdapter GetAdapter() => new NctsHeaderPhase5CustomsLinkedObjectAdapterForTest(Adaptee);

	const string NoClearanceResponseManifestResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.NctsResponsePositiveWithMRNNotReleased.xml";

	sealed class NctsHeaderPhase5CustomsLinkedObjectAdapterForTest : NctsHeaderPhase5CustomsLinkedObjectAdapter
	{
		public NctsHeaderPhase5CustomsLinkedObjectAdapterForTest(NctsHeader header) : base(header)
		{
		}

		protected override IrildesRequestMessageFactory GetNewIrildesRequestMessageFactory()
		{
			return new IrildesRequestMessageFactoryForTest();
		}
	}
}

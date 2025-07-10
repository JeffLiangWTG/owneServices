using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStorageCustomsLinkedObjectAdapterTest : XmlCustomsLinkedObjectAdapterTest<TemporaryStorageHeader>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("TemporaryStorageHeader is required", () => new TemporaryStorageCustomsLinkedObjectAdapter(null));
		AssertNoExceptionThrown("Valid TemporaryStorageHeader", () => new TemporaryStorageCustomsLinkedObjectAdapter(Factory.New<TemporaryStorageHeader>()));
	}

	public override void TestIsAwaitingMessage()
	{
		var header = Adaptee;

		header.AMA_MessageStatus = ZString.Empty;
		AssertEquals(nameof(Adapter.IsAwaitingMessage), false, Adapter.IsAwaitingMessage);

		header.AMA_MessageStatus = PNTSMessageStatusList.Codes.Sent;
		AssertEquals(nameof(Adapter.IsAwaitingMessage), true, Adapter.IsAwaitingMessage);
	}

	public override void TestIsDeposited()
	{
		AssertEquals(nameof(Adapter.IsDeposited), false, Adapter.IsDeposited);
	}

	public override void TestSetStatusAsError()
	{
		var header = Adaptee;
		AssertEquals("[PRE-CONDITION] AMA_MessageStatus", "", header.AMA_MessageStatus);

		Adapter.SetStatusAsError();
		AssertEquals("[POST-CONDITION] AMA_MessageStatus", "REJ", header.AMA_MessageStatus);
	}

	public override void TestCustomsOfficeOfPresentation()
	{
		AssertEquals(nameof(Adapter.CustomsOfficeOfPresentation), ZString.Empty, Adapter.CustomsOfficeOfPresentation);
	}

	public override void TestGetAllEntryLines()
	{
		var bill = Adaptee.Bills.AddNew();
		var allEntryLines = Adapter.GetAllEntryLines().ToList();
		AssertNotNull("The list of entry lines should not be null", allEntryLines);
		AssertEquals("The count of entry lines", 2, allEntryLines.Count);
	}

	public override void TestSetStatusAsCleared()
	{
		AssertExceptionThrown<NotImplementedException>(() => Adapter.SetStatusAsCleared(ZDateTime.UtcNow));
	}

	public override void TestSetStatusAsRegistered()
	{
		var header = Adaptee;
		header.CustomsStatus = "";
		header.AMA_MessageStatus = "SNT";
		header.CustomsStatusDate = ZDateTime.BrettsBirthday;

		Adapter.SetStatusAsRegistered(ZDateTime.Today);

		AssertEquals("CustomsStatus", "TPA", header.CustomsStatus);
		AssertEquals("AMA_MessageStatus", "", header.AMA_MessageStatus);
		AssertEquals("CustomsStatusDate", ZDateTime.Today, header.CustomsStatusDate);
	}

	public override void TestSetStatusAsCancelled()
	{
		AssertExceptionThrown<NotImplementedException>(Adapter.SetStatusAsCancelled);
	}

	public override void TestSetStatusAsAcceptedBySystem()
	{
		var header = Adaptee;
		AssertEquals("[PRE-CONDITION] CustomsStatus", "", header.CustomsStatus);

		Adapter.SetStatusAsAcceptedBySystem();
		AssertEquals("[POST-CONDITION] CustomsStatus", "TSA", header.CustomsStatus);
	}

	public override void TestUpdateOrInsertEntryNumber()
	{
		var bill = Adaptee.Bills.AddNew();
		var item1 = bill.PackedItems.AddNew();
		item1.API_LineNo = 1;
		var item2 = bill.PackedItems.AddNew();
		item2.API_LineNo = 2;
		var item3 = bill.PackedItems.AddNew();
		item3.API_LineNo = 3;
		var lrnEntryNumber = GetNewCusEntryNumber(bill, CusEntryNumberTypes.Standard.LocalReferenceNumber);
		lrnEntryNumber.CE_EntryNum = "TestLRN123";

		_ = Adapter.UpdateOrInsertEntryNumber(
			"MRN",
			"123",
			"TestLRN123",
			ZDateTime.Now,
			null);

		_ = Adapter.UpdateOrInsertEntryNumber(
			"REG",
			"345",
			"TestLRN123",
			ZDateTime.Now,
			1);

		_ = Adapter.UpdateOrInsertEntryNumber(
			"REG",
			"567",
			"TestLRN123",
			ZDateTime.Now,
			2);

		AssertCusEntryNumber(bill, "MRN", "123");
		AssertCusEntryNumber(item1, "REG", "345");
		AssertCusEntryNumber(item2, "REG", "567");

		var item3RegEntry = CusEntryNumber.Load(item3, "REG", "IT");
		AssertNull(nameof(CusEntryNumber), item3RegEntry);
	}

	void AssertCusEntryNumber(BusinessObject businessObject, string expectedEntryType, string expectedEntryNum)
	{
		var cusEntryNumber = CusEntryNumber.Load(businessObject, expectedEntryType, "IT");

		CombineAssertions($"Entry Number {expectedEntryType}", () =>
		{
			AssertNotNull(nameof(CusEntryNumber), cusEntryNumber);
			AssertEquals(nameof(CusEntryNumber.CE_EntryNum), expectedEntryNum, cusEntryNumber.CE_EntryNum);
		});
	}

	CusEntryNumber GetNewCusEntryNumber(TemporaryStorageBill parent, ZString entryType)
	{
		var cusEntryNumber = Factory.New<CusEntryNumber>();
		cusEntryNumber.CE_ParentID = parent.PK;
		cusEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		cusEntryNumber.CE_EntryType = entryType;
		cusEntryNumber.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
		cusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;

		return cusEntryNumber;
	}

	public override void TestGetAllRelatedEntryNumbers()
	{
		AssertExceptionThrown<NotImplementedException>(() => Adapter.GetAllRelatedEntryNumbers());
	}

	public override void TestSetStatusAsAcknowledged()
	{
		var header = Adaptee;
		AssertEquals("[PRE-CONDITION] AMA_MessageStatus", "", header.AMA_MessageStatus);

		Adapter.SetStatusAsAcknowledged();
		AssertEquals("[POST-CONDITION] AMA_MessageStatus", "ACK", header.AMA_MessageStatus);
	}

	public override void TestSetStatusAsUnderControl()
	{
		AssertExceptionThrown<NotImplementedException>(Adapter.SetStatusAsUnderControl);
	}

	public override void TestSetStatusAsGoodsWrittenOffClosed()
	{
		AssertExceptionThrown<NotImplementedException>(Adapter.SetStatusAsGoodsWrittenOffClosed);
	}

	public override void TestUpdateOrInsertIrildesEntryNumber()
	{
		AssertExceptionThrown<NotImplementedException>(() => Adapter.UpdateOrInsertIrildesEntryNumber("", ZDateTime.Now));
	}

	public override void TestSetStatusAsFailedForTransmission()
	{
		var header = Adaptee;
		AssertEquals("[PRE-CONDITION] AMA_MessageStatus", "", header.AMA_MessageStatus);

		Adapter.SetStatusAsFailedForTransmission();
		AssertEquals("[POST-CONDITION] AMA_MessageStatus", "FAL", header.AMA_MessageStatus);
	}

	public override void TestSetStatusAsDeposited()
	{
		AssertExceptionThrown<NotImplementedException>(Adapter.SetStatusAsDeposited);
	}

	public override void TestSetStatusAsExitCompleted()
	{
		AssertExceptionThrown<NotImplementedException>(Adapter.SetStatusAsExitCompleted);
	}

	public override void TestGetAllPaymentInfo()
	{
		AssertExceptionThrown<NotImplementedException>(() => Adapter.GetAllPaymentInfo());
	}

	public override void TestSetStatusAsAmended()
	{
		AssertExceptionThrown<NotImplementedException>(Adapter.SetStatusAsAmended);
	}

	public override void TestAddA93Number()
	{
		AssertExceptionThrown<NotImplementedException>(() => Adapter.AddA93Number(paymentInfo: null));
	}

	public override void TestGetAllFees()
	{
		AssertExceptionThrown<NotImplementedException>(() => Adapter.GetAllFees(feeFilter: null));
	}

	public override void TestGetFeesForLine()
	{
		AssertExceptionThrown<NotImplementedException>(() => Adapter.GetFeesForLine(1, feeFilter: null));
	}

	public override void TestSetCustomsChannel()
	{
		AssertExceptionThrown<NotImplementedException>(() => Adapter.SetCustomsChannel(""));
	}

	public override void TestGetUniqueTransactionIdentifierRequestContext()
	{
		AssertType<TemporaryStorageHeaderUniqueTransactionIdentifierRequestContext>("Type", Adapter.GetUniqueTransactionIdentifierRequestContext("1234", "ABCDEF123456"));
	}

	public override void TestGetLastSuccessfullySentMessageForDepositedStatus()
	{
		AssertExceptionThrown<NotImplementedException>(() => Adapter.GetLastSuccessfullySentMessage());
	}

	public override void TestUpdateOrInsertIvistoEntryNumber()
	{
		AssertExceptionThrown<NotImplementedException>(() => Adapter.UpdateOrInsertIvistoEntryNumber(ZDateTime.Now, "", ""));
	}

	public override void TestGetOriginalSentMessageByUniqueTransactionIdentifier()
	{
		AssertExceptionThrown<NotImplementedException>(() => Adapter.GetOriginalSentMessageByUniqueTransactionIdentifier(""));
	}

	public override void TestAddEDoc()
	{
		AssertExceptionThrown<NotImplementedException>(() => Adapter.AddEDoc([0x00], "", ""));
	}

	public override void TestCustomsProfile()
	{
		Adaptee.AMA_CustomsProfile = "12345";
		AssertEquals(Adaptee.AMA_CustomsProfile, Adapter.CustomsProfile);
	}

	public override void TestCreateIvistoRequestMessage()
	{
		AssertExceptionThrown<CustomsMessageProcessorException>(Adapter.CreateIvistoRequestMessage);
	}

	public override void TestCreateIrildesRequestMessage()
	{
		AssertExceptionThrown<CustomsMessageProcessorException>(Adapter.CreateIrildesRequestMessage);
	}

	protected override TemporaryStorageHeader GetAdaptee()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		header.AMA_RN_NKCountry = "IT";
		return header;
	}

	protected override IXmlCustomsLinkedObjectAdapter GetAdapter() => new TemporaryStorageCustomsLinkedObjectAdapter(Adaptee);
}

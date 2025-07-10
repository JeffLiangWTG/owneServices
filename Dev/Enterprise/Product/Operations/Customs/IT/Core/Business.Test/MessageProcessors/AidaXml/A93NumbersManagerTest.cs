using System.Linq;
using CargoWise.Customs.IT.MessageDefinitions;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import.data;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class A93NumbersManagerTest : TestCaseWithFactory
{
	public void TestInvalidateNumbers_GenerateInvalidatedEntry()
	{
		var (entryHeader, _, _) = dataGenerator.GetEntryInfoWithSentMessage();
		var (_, payInfoTwo, _) = dataGenerator.GetCusEntryPayInfos();
		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);
		var numberManager = new A93NumbersManager(adapter);
		numberManager.InvalidateNumbers();

		entryHeader.EntryPayInfos.Load();
		var entryPayInfo = entryHeader
			.EntryPayInfos
			.Cast<CusEntryPayInfo>()
			.SingleOrDefault(i => i.A93Number == payInfoTwo.A93Number && i.C9_PaymentAmount < 0);

		AssertNotNull("Invalidated Info", entryPayInfo);
		AssertEquals("Amount", -789.99m, entryPayInfo.C9_PaymentAmount);
	}

	public void TestInvalidateNumbers_WithExistingInvalidatedNumbers()
	{
		var (entryHeader, _, _) = dataGenerator.GetEntryInfoWithSentMessage();
		var (payInfoOne, payInfoTwo, _) = dataGenerator.GetCusEntryPayInfos();

		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);
		var numberManager = new A93NumbersManager(adapter);
		numberManager.InvalidateNumbers();
		entryHeader.EntryPayInfos.Load();

		var allPaymentInfos = entryHeader.EntryPayInfos.Cast<CusEntryPayInfo>().ToArray();
		AssertEquals("A93 Numbers Count", 4, allPaymentInfos.Length);
		AssertEquals("A93 Negative Amount Record Count", 2, allPaymentInfos.Count(n => n.C9_PaymentAmount < 0));
		AssertEquals("A93 Positive Amount Record Count", 2, allPaymentInfos.Count(n => n.C9_PaymentAmount > 0));
		AssertCollectionContains(allPaymentInfos, i => i.A93Number == payInfoTwo.A93Number && i.C9_PaymentAmount == (payInfoTwo.C9_PaymentAmount * -1));
		AssertEquals("PayInfoOne Invalidated Record Count", 1, allPaymentInfos.Count(i => i.A93Number == payInfoOne.A93Number && i.C9_PaymentAmount < 0));

		var newA93Number = entryHeader.EntryPayInfos.AddNew();
		newA93Number.C9_IncomingPayResponseNo = "999";
		newA93Number.C9_PaymentAmount = 556.77m;
		newA93Number.C9_PaymentDate = new ZDateTime(2022, 10, 01);
		newA93Number.C9_PaymentParty = "E";
		newA93Number.C9_PaymentStatus = "PEN";

		adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);
		numberManager = new A93NumbersManager(adapter);
		numberManager.InvalidateNumbers();
		entryHeader.EntryPayInfos.Load();

		allPaymentInfos = entryHeader.EntryPayInfos.Cast<CusEntryPayInfo>().ToArray();
		AssertEquals("A93 Numbers Count", 6, allPaymentInfos.Length);
		AssertCollectionContains(allPaymentInfos, i => i.A93Number == newA93Number.A93Number && i.C9_PaymentAmount == (newA93Number.C9_PaymentAmount * -1));

		var corruptEntry = entryHeader.EntryPayInfos.AddNew();
		corruptEntry.C9_IncomingPayResponseNo = "CORRUPT";
		corruptEntry.C9_PaymentAmount = -126.77m;
		corruptEntry.C9_PaymentDate = new ZDateTime(2022, 05, 01);
		corruptEntry.C9_PaymentParty = "E";
		corruptEntry.C9_PaymentStatus = "PEN";

		var entryWithZeroAmount = entryHeader.EntryPayInfos.AddNew();
		entryWithZeroAmount.C9_IncomingPayResponseNo = "ZERO_ENTRY";
		entryWithZeroAmount.C9_PaymentAmount = 0m;
		entryWithZeroAmount.C9_PaymentDate = new ZDateTime(2022, 01, 01);
		entryWithZeroAmount.C9_PaymentParty = "G";
		entryWithZeroAmount.C9_PaymentStatus = "PEN";

		adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);
		numberManager = new A93NumbersManager(adapter);
		numberManager.InvalidateNumbers();
		entryHeader.EntryPayInfos.Load();

		allPaymentInfos = entryHeader.EntryPayInfos.Cast<CusEntryPayInfo>().ToArray();
		AssertEquals("A93 Numbers Count", 8, allPaymentInfos.Length);
		AssertCollectionContains("Corrupt Entry is not negated",
			allPaymentInfos,
			predicate: i => i.A93Number == corruptEntry.A93Number && i.C9_PaymentAmount != (newA93Number.C9_PaymentAmount * -1));
		AssertEquals("Number of Entries with Zero Amount", 1, allPaymentInfos.Count(i => i.C9_PaymentAmount == 0));
	}

	public void TestAddNumbers_EntryLevel()
	{
		var (entryHeader, _, _) = dataGenerator.GetEntryInfoWithSentMessage();
		dataGenerator.GenerateLineWithFees(2, "E");
		dataGenerator.GenerateLineWithFees(3, "G");

		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);
		var numberManager = new A93NumbersManager(adapter);
		var dataInfoOne = new DataInformazione
		{
			Numero = "87",
			Data = "2022-23/03/2022E-24/01/2021G",
			Livello = "P"
		};

		var a93Numbers = new IA93NumberInformation[] { dataInfoOne };
		numberManager.AddNumbers(new ResponseMessageDetailsReadOnlyCollection<IA93NumberInformation>(a93Numbers));

		var entryPayInfos = entryHeader.EntryPayInfos.Cast<CusEntryPayInfo>().ToArray();
		var lineTwoPayInfo = entryPayInfos.SingleOrDefault(e => e.A93Number == "87" && e.C9_PaymentParty == "E");
		var lineThreePayInfo = entryPayInfos.SingleOrDefault(e => e.A93Number == "87" && e.C9_PaymentParty == "G");

		AssertNotNull("CusEntryPayInfo For Payment Type E", lineTwoPayInfo);
		AssertNotNull("CusEntryPayInfo For Payment Type G", lineThreePayInfo);

		AssertA93Numbers(payInfoLineOne: lineTwoPayInfo,
			payInfoLineTwo: lineThreePayInfo,
			expectedPayInfoLineOneAmount: 139.1m,
			expectedPayInfoLineTwoAmount: 145.08m);
	}

	public void TestAddNumbers_EntryLineLevel()
	{
		var (entryHeader, _, _) = dataGenerator.GetEntryInfoWithSentMessage();
		dataGenerator.GenerateLineWithFees(2, "E");
		dataGenerator.GenerateLineWithFees(3, "G");

		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);
		var numberManager = new A93NumbersManager(adapter);
		var dataInfoOne = new DataInformazione
		{
			NumeroArticolo = 2,
			Numero = "87",
			Data = "2022-23/03/2022E",
			Livello = "P"
		};
		var dataInfoTwo = new DataInformazione
		{
			NumeroArticolo = 3,
			Numero = "87",
			Data = "2022-24/01/2021G",
			Livello = "P"
		};
		var a93Numbers = new IA93NumberInformation[]
		{
			dataInfoOne,
			dataInfoTwo,
		}.ToCollection();

		numberManager.AddNumbers(new ResponseMessageDetailsReadOnlyCollection<IA93NumberInformation>(a93Numbers));

		var entryPayInfos = entryHeader.EntryPayInfos.Cast<CusEntryPayInfo>().ToArray();
		var lineTwoPayInfo = entryPayInfos.SingleOrDefault(e => e.A93Number == "87-2");
		var lineThreePayInfo = entryPayInfos.SingleOrDefault(e => e.A93Number == "87-3");

		AssertNotNull("CusEntryPayInfo For Line 2", lineTwoPayInfo);
		AssertNotNull("CusEntryPayInfo For Line 3", lineThreePayInfo);

		AssertA93Numbers(payInfoLineOne: lineTwoPayInfo,
			payInfoLineTwo: lineThreePayInfo,
			expectedPayInfoLineOneAmount: 142.09m,
			expectedPayInfoLineTwoAmount: 145.08m);
	}

	protected override void SetUp()
	{
		base.SetUp();
		dataGenerator = new AmendmentMessageProcessorTestDataGenerator(Factory);
		dataGenerator.Generate();
	}

	static void AssertA93Numbers(CusEntryPayInfo payInfoLineOne, CusEntryPayInfo payInfoLineTwo, decimal expectedPayInfoLineOneAmount, decimal expectedPayInfoLineTwoAmount)
	{
		CombineAssertions(() =>
		{
			AssertEquals(nameof(CusEntryPayInfo.C9_PaymentAmount), expectedPayInfoLineOneAmount, payInfoLineOne.C9_PaymentAmount);
			AssertEquals(nameof(CusEntryPayInfo.C9_PaymentDate), new ZDateTime(2022, 03, 23), payInfoLineOne.C9_PaymentDate);
			AssertEquals(nameof(CusEntryPayInfo.C9_TransactionType), ZString.Empty, payInfoLineOne.C9_TransactionType);
			AssertEquals(nameof(CusEntryPayInfo.C9_PaymentReference), ZString.Empty, payInfoLineOne.C9_PaymentReference);
			AssertEquals(nameof(CusEntryPayInfo.C9_PaymentParty), "E", payInfoLineOne.C9_PaymentParty);
			AssertEquals(nameof(CusEntryPayInfo.C9_PaymentStatus), "PEN", payInfoLineOne.C9_PaymentStatus);

			AssertEquals(nameof(CusEntryPayInfo.C9_PaymentAmount), expectedPayInfoLineTwoAmount, payInfoLineTwo.C9_PaymentAmount);
			AssertEquals(nameof(CusEntryPayInfo.C9_PaymentDate), new ZDateTime(2021, 01, 24), payInfoLineTwo.C9_PaymentDate);
			AssertEquals(nameof(CusEntryPayInfo.C9_TransactionType), ZString.Empty, payInfoLineTwo.C9_TransactionType);
			AssertEquals(nameof(CusEntryPayInfo.C9_PaymentReference), ZString.Empty, payInfoLineTwo.C9_PaymentReference);
			AssertEquals(nameof(CusEntryPayInfo.C9_PaymentParty), "G", payInfoLineTwo.C9_PaymentParty);
			AssertEquals(nameof(CusEntryPayInfo.C9_PaymentStatus), "PEN", payInfoLineTwo.C9_PaymentStatus);
		});
	}

	AmendmentMessageProcessorTestDataGenerator dataGenerator;
}

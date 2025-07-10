using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageRegisterSupporter))]
sealed class TemporaryStorageRegisterSupporterTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new TemporaryStorageRegisterSupporter(null));
	}

	public void TestCreateRegisterTransactionsForBill()
	{
		header.AMA_JobReference = "TS001";
		header.ArrivalTransportMeansCode = "AN191CX";
		header.PresentationCustomsOffice = "IT278100";
		header.GoodsLocation.CGL_AdditionalIdentifier = "12345";

		for (var i = 0; i < 3; i++)
		{
			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = $"{i + 1}";
		}

		var bill1 = AddBill("24ITQYH300268983U7", "001", [(1000, 0), (1000, 1), (1000, 2)], "BW");
		var bill2 = AddBill("24ITQYH300268984U7", "002", [(1000, 0), (5000, 1), (3000, 2)], "BX");

		var supporter = new TemporaryStorageRegisterSupporter(header);
		supporter.CreateRegisterTransactionsForBill("24ITQYH300268984U7");

		AssertNull("Bill 1, Register Header", bill1.RegisterHeader);

		var registerHeader = bill2.RegisterHeader;
		TemporaryStorageRegisterTestHelper.AssertRegisterHeader(
			message: "Bill 2, Register Header",
			registerHeader: registerHeader,
			appCode: "TSR",
			reference: "REF001",
			internalReference: "TS001",
			status: "OPN",
			previousReferenceType: "G4",
			movementReferenceNumber: "24ITQYH300268984U7",
			presentationDate: new DateTime(2025, 1, 1),
			transportID: "AN191CX",
			customsOffice: "IT278100");

		var line1 = registerHeader.CusTempStorageRegLines[0];
		TemporaryStorageRegisterTestHelper.AssertRegisterLine(
			message: "Bill 2, Register Header, Line 1",
			line: line1,
			houseBill: "002",
			lineNumber: 1,
			limitDate: new ZDate(2025, 4, 10),
			locationOfGoods: "12345",
			ownerReference: "A3-1234",
			ownerReferenceType: "A3",
			goodsDescription: "Goods 1",
			packageType: "BX",
			grossWeightUQ: "KG",
			containers: ["1", "2"]);

		AssertEquals("Register 2 Line 1 transactions", 1, line1.CusTempStorageRegLineTransactions.Count);
		TemporaryStorageRegisterTestHelper.AssertRegisterLineTransaction(
			message: "Bill 2, Register Header, Line 1, OBL Transaction",
			transaction: line1.CusTempStorageRegLineTransactions[0],
			type: "OBL",
			grossWeight: 2500,
			packageQty: 6000,
			referenceType: "IST",
			reference: "TS001");

		var line2 = registerHeader.CusTempStorageRegLines[1];
		TemporaryStorageRegisterTestHelper.AssertRegisterLine(
			message: "Bill 2, Register Header, Line 2",
			line: line2,
			houseBill: "002",
			lineNumber: 2,
			limitDate: new ZDate(2025, 5, 21),
			locationOfGoods: "12345",
			ownerReference: "A4-5678",
			ownerReferenceType: "A4",
			goodsDescription: "Goods 2",
			packageType: "BX",
			grossWeightUQ: "LB",
			containers: ["2", "3"]);

		AssertEquals("Register 2 Line 2 transactions", 1, line2.CusTempStorageRegLineTransactions.Count);
		TemporaryStorageRegisterTestHelper.AssertRegisterLineTransaction(
			message: "Bill 2, Register Header, Line 1, OBL Transaction",
			transaction: line2.CusTempStorageRegLineTransactions[0],
			type: "OBL",
			grossWeight: 5000,
			packageQty: 8000,
			referenceType: "IST",
			reference: "TS001");
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<TemporaryStorageHeader>();
	}

	TemporaryStorageBill AddBill(ZString movementReferenceNumber, ZString billNumber, (ZInt PackQty, ZInt containerIndex)[] packs, ZString packUQ)
	{
		var bill = header.Bills.AddNew();
		bill.ABL_BillNumber = billNumber;

		var entryNumber = CusEntryNumber.LoadOrCreate(bill, "MRN", "IT");
		entryNumber.CE_EntryNum = movementReferenceNumber;
		entryNumber.CE_IssueDate = new DateTime(2025, 1, 1);

		foreach (var (packQty, containerIndex) in packs)
		{
			var pack = bill.Packs.AddNew();
			pack.APA_PackQty = packQty;
			pack.APA_PackUQ = packUQ;
			pack.ContainerPK = header.Containers[containerIndex].PK;
		}

		AddItem(bill, 1, "A3-1234", new DateTime(2025, 1, 10), "Goods 1", 2500, "KG", [bill.Packs[0], bill.Packs[1]]);
		AddItem(bill, 2, "A4-5678", new DateTime(2025, 2, 20), "Goods 2", 5000, "LB", [bill.Packs[1], bill.Packs[2]]);

		return bill;
	}

	void AddItem(TemporaryStorageBill bill, ZInt lineNumber, ZString registrationNumber, ZDateTime registrationDate, ZString goodsDescription, ZDecimal grossWeight, ZString grossWeightUQ, EU.Business.CusTempStorage.TemporaryStoragePack[] packs)
	{
		var item = bill.PackedItems.AddNew();
		item.API_LineNo = lineNumber;
		item.API_GoodsDescription = goodsDescription;
		item.API_GrossWeight = grossWeight;
		item.API_GrossWeightUQ = grossWeightUQ;

		foreach (var pack in packs)
		{
			var linkPackage = item.TemporaryStorageLinkPackages.First(x => x.Package == pack);
			linkPackage.IsLinked = true;
		}

		var entryNumber = CusEntryNumber.LoadOrCreate(item, "REG", "IT");
		entryNumber.CE_EntryNum = registrationNumber;
		entryNumber.CE_IssueDate = registrationDate;
	}

	TemporaryStorageHeader header;
}

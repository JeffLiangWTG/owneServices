using System.Linq;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import.data;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class A93NumberHandlerTest : TestCaseWithFactory
{
	public void TestHandleWithPayInfo_Invalidate()
	{
		var innerData = new Data();
		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);

		var handler = new A93NumberHandler(adapter, A93NumberHandleMode.Invalidate);
		handler.Handle(innerData);

		entryHeader.EntryPayInfos.Load();
		var entryPayInfos = entryHeader.EntryPayInfos.Cast<CusEntryPayInfo>().ToArray();
		AssertEquals("Count", 2, entryPayInfos.Length);
		AssertCollectionContains("Invalidated entry for A93 Number 123",
			entryPayInfos,
			e => e.A93Number == "123" && e.C9_PaymentAmount == -1234.22m);
	}

	public void TestHandleWithA93Numbers_ModeAdd()
	{
		var innerData = GetDataWithA93Number();
		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);
		var handler = new A93NumberHandler(adapter, A93NumberHandleMode.Add);
		handler.Handle(innerData);

		var entryPayInfos = entryHeader.EntryPayInfos.Cast<CusEntryPayInfo>().ToArray();
		AssertEquals("Count", 2, entryPayInfos.Length);
		AssertCollectionContains("New entry for A93 Number 87-2",
			entryPayInfos,
			e => e.A93Number == "87-2" && e.C9_PaymentAmount == 120.50m);
	}

	public void TestHandleWithA93Numbers_ModeInvalidateAndAdd()
	{
		var innerData = GetDataWithA93Number();
		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);
		var handler = new A93NumberHandler(adapter, A93NumberHandleMode.InvalidateAndAdd);
		handler.Handle(innerData);

		entryHeader.EntryPayInfos.Load();
		var entryPayInfos = entryHeader.EntryPayInfos.Cast<CusEntryPayInfo>().ToArray();
		AssertEquals("Count", 3, entryPayInfos.Length);
		AssertCollectionContains("Invalidated entry for A93 Number 123",
			entryPayInfos,
			e => e.A93Number == "123" && e.C9_PaymentAmount == -1234.22m);
		AssertCollectionContains("New entry for A93 Number 87-2",
			entryPayInfos,
			e => e.A93Number == "87-2" && e.C9_PaymentAmount == 120.50m);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var jobDeclaration = Factory.New<JobDeclaration>();
		entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		payInfo = entryHeader.EntryPayInfos.AddNew();
		payInfo.C9_IncomingPayResponseNo = "123";
		payInfo.C9_PaymentAmount = 1234.22m;
		payInfo.C9_PaymentDate = new ZDateTime(2022, 09, 01);
		payInfo.C9_PaymentParty = "E";
		payInfo.C9_PaymentStatus = "PEN";

		var entryLine = entryHeader.MergedLines.AddNew();
		entryLine.CL_LineNumber = 2;

		var feeOne = entryLine.Fees.AddNew();
		feeOne.CF_MethodOfPayment = "E";
		feeOne.CF_ChargeAmount = 120.50m;
		feeOne.CF_ChargeType = "DTY";
	}

	static Data GetDataWithA93Number()
	{
		var info = new DataInformazione
		{
			NumeroArticolo = 2,
			Numero = "87",
			Data = "2022-23/03/2022E",
			Livello = "P"
		};

		var innerData = new Data
		{
			Informazione = new[] { info }.ToCollection()
		};
		return innerData;
	}

	CusEntryHeader entryHeader;
	CusEntryPayInfo payInfo;
}

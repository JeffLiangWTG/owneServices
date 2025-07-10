using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(CusEntryPayInfo))]
sealed class CusEntryPayInfoTest : Customs.Business.Testing.CusEntryPayInfoTest
{
	public void TestA93Number()
	{
		var entryPayInfo = Factory.New<CusEntryPayInfo>();
		AssertEquals("A93Number (when C9_IncomingPayResponseNo is empty)", ZString.Empty, entryPayInfo.A93Number);

		entryPayInfo.C9_IncomingPayResponseNo = "XXXX";
		AssertEquals("A93Number", "XXXX", entryPayInfo.A93Number);

		entryPayInfo.C9_IncomingPayResponseNo = "XXXX|G";
		AssertEquals("A93Number (when C9_IncomingPayResponseNo has special characters)", "XXXX|G", entryPayInfo.A93Number);
	}

	public void TestMethodOfPayment()
	{
		var entryPayInfo = Factory.New<CusEntryPayInfo>();
		AssertEquals("MethodOfPayment (when C9_PaymentParty is empty)", ZString.Empty, entryPayInfo.MethodOfPayment);

		entryPayInfo.C9_PaymentParty = "G";
		AssertEquals("MethodOfPayment", "G", entryPayInfo.MethodOfPayment);

		entryPayInfo.C9_PaymentParty = "|";
		AssertEquals("MethodOfPayment (when C9_PaymentParty has a special character)", "|", entryPayInfo.MethodOfPayment);
	}

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew().EntryPayInfos.AddNew();
}

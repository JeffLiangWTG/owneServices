using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsDeparturePayInfoCollection))]
sealed class NctsDeparturePayInfoCollectionTest : ActiveBusinessObjectCollectionTestCase<NctsDeparturePayInfoCollection>
{
	public void TestInsertOrUpdatePayInfo()
	{
		var payInfoCollection = GetCollectionToTest();
		AssertEquals("PRE-CONDITION", 0, payInfoCollection.Count);

		var payInfo1 = payInfoCollection.InsertOrUpdatePayInfo(x => x != null, 1m, "4 T", new ZDateTime(2021, 01, 01), "000001", "G");
		CombineAssertions("PayInfo1 added", () =>
		{
			AssertNctsDeparturePayInfo(payInfo1, "000001", "G", 1m, "4 T", new ZDateTime(2021, 01, 01), CusEntryPayInfoStatusList.Codes.Pending);
		});

		CombineAssertions("PayInfo1 updated", () =>
		{
			var payInfo2 = payInfoCollection.InsertOrUpdatePayInfo(x => x != null, 999m, "4 T", new ZDateTime(2021, 12, 31), "000001", "G");
			AssertNctsDeparturePayInfo(payInfo2, "000001", "G", 999m, "4 T", new ZDateTime(2021, 12, 31), CusEntryPayInfoStatusList.Codes.Pending);
			AssertSame("Same object", payInfo1, payInfo2);
		});

		CombineAssertions("Edge Case 1: add element with empty method of payment", () =>
		{
			var payInfo3 = payInfoCollection.InsertOrUpdatePayInfo(x => false, 999m, "2", new ZDateTime(2021, 12, 31), "999999", "");
			AssertNctsDeparturePayInfo(payInfo3, "999999", "", 999m, "2", new ZDateTime(2021, 12, 31), CusEntryPayInfoStatusList.Codes.Pending);
			AssertEquals("Collection count", 2, payInfoCollection.Count);
		});

		CombineAssertions("Edge Case 2: add element with empty number", () =>
		{
			var payInfo4 = payInfoCollection.InsertOrUpdatePayInfo(x => false, 999m, "2", new ZDateTime(2021, 12, 31), "", "G");
			AssertNctsDeparturePayInfo(payInfo4, "", "G", 999m, "2", new ZDateTime(2021, 12, 31), CusEntryPayInfoStatusList.Codes.Pending);
			AssertEquals("Collection count", 3, payInfoCollection.Count);
		});
	}

	protected override NctsDeparturePayInfoCollection GetCollectionToTest()
	{
		var moveHeader = Factory.New<NctsDepartureMovementHeader>();
		return new NctsDeparturePayInfoCollection(moveHeader);
	}

	void AssertNctsDeparturePayInfo(CusInBondPayInfo payInfo, ZString incomingPayResponseNo, ZString methodOfPayment, ZDecimal paymentAmount, ZString declarationRegistry, ZDateTime expirationDate, ZString paymentStatus)
	{
		AssertEquals("BPI_IncomingPayResponseNo", incomingPayResponseNo, payInfo.BPI_IncomingPayResponseNo);
		AssertEquals("BPI_MethodOfPayment", methodOfPayment, payInfo.BPI_MethodOfPayment);
		AssertEquals("BPI_PaymentAmount", paymentAmount, payInfo.BPI_PaymentAmount);
		AssertEquals("BPI_TransactionType", declarationRegistry, payInfo.BPI_TransactionType);
		AssertEquals("BPI_PaymentDate", expirationDate, payInfo.BPI_PaymentDate);
		AssertEquals("BPI_PaymentStatus", paymentStatus, payInfo.BPI_PaymentStatus);
	}
}

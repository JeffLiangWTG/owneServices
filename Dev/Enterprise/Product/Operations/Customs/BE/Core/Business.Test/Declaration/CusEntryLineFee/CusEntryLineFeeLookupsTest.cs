using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(CusEntryLineFeeLookups))]
sealed class CusEntryLineFeeLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestMethodOfPaymentList()
	{
		AssertEquals("A, B, C, D, E, F, G, H, J, K, M, O, P, R, S, T, U, V", lookups.MethodOfPaymentList.CodesAsString);
	}

	public void TestMethodOfPaymentListIsCached()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();
		var feeLookups = entryLineFee.Lookups.MethodOfPaymentList;
		var newEntryLineFee = Factory.New<CusEntryLineFee>();
		var newFeeLookups = newEntryLineFee.Lookups.MethodOfPaymentList;
		AssertSame(feeLookups, newFeeLookups);
		Factory.ClearCachedValue<PaymentMethodList>();
		AssertEquals(false, object.ReferenceEquals(feeLookups, entryLineFee.Lookups.MethodOfPaymentList));
	}

	protected override void SetUp()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();
		lookups = entryLineFee.Lookups;
	}
	CusEntryLineFeeLookups lookups;
}

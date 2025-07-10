using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(CusEntryLineFeeCollection))]
sealed class CusEntryLineFeeCollectionTest : Customs.Business.Testing.CusEntryLineFeeCollectionTest
{
	public void TestAddNewCore()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_PaymentMethod = "A";
		var header = declaration.CustomsEntryHeaders.AddNew();
		var line = header.MergedLines.AddNew();
		var fee = line.Fees.AddNew();
		AssertEquals("Method of payment set from declaration", "A", fee.CF_MethodOfPayment);
	}
}

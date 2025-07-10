using NUnit.Framework;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
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

		public void TestCreateAdditionalFilter()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_PaymentMethod = "A";
			var header = declaration.CustomsEntryHeaders.AddNew();
			var line = header.MergedLines.AddNew();

			var collection = line.Fees;
			var newFee = Factory.New<CusEntryLineFee>();
			newFee.CF_CL = line.PK;
			newFee.CF_MethodOfCalculation = "NON";
			collection.Load();
			AssertEquals(true, collection.Contains(newFee));

			var confirmedReleaseFee = Factory.New<CusEntryLineFee>();
			confirmedReleaseFee.CF_CL = line.PK;
			confirmedReleaseFee.CF_MethodOfCalculation = CusEntryLineFeeRefundDutyMethodOfCalculation.ConfirmedRelease;
			collection.Load();
			AssertEquals("Should be contained in RefundFees instead.", false, collection.Contains(confirmedReleaseFee));
		}
	}
}

using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class PaymentProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(PaymentProvider.New(null));
			AssertType<PaymentProvider>(PaymentProvider.New(new List<CusEntryLineFee> { Factory.New<CusEntryLineFee>() }));
		}

		public void TestTaxAmount()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.Fees.AddOrUpdate(ChargeTypesList.Codes.DTY, 10.15m);

			var dataProvider = PaymentProvider.New(entryHeader.AllMergedLinesFees);
			AssertEquals((double)10.15m, dataProvider.TaxAmount);

			entryLine1.Fees.AddOrUpdate(ChargeTypesList.Codes.DTY, 10.1561m);
			AssertEquals((double)10.16m, dataProvider.TaxAmount);

			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate(ChargeTypesList.Codes.DTY, 35.456m);
			AssertEquals((double)45.61m, dataProvider.TaxAmount);
		}

		public void TestTaxType()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryLine = entryHeader.MergedLines.AddNew();

			var fee = entryLine.Fees.AddOrUpdate(ChargeTypesList.Codes.DTY, 10.15m);
			var dataProvider = PaymentProvider.New(entryHeader.AllMergedLinesFees);
			AssertEquals("II", dataProvider.TaxType);

			fee.CF_ChargeType = Constants.RateTypes.IPI;
			AssertEquals("IPI", dataProvider.TaxType);

			fee.CF_ChargeType = Constants.RateTypes.PIS;
			AssertEquals("PIS", dataProvider.TaxType);

			fee.CF_ChargeType = Constants.RateTypes.Cofins;
			AssertEquals("COFINS", dataProvider.TaxType);

			fee.CF_ChargeType = Constants.RateTypes.Antidumping;
			AssertEquals("ANTIDUMPING", dataProvider.TaxType);

			entryHeader.MergedLines.AddNew().Fees.AddOrUpdate(Constants.RateTypes.Antidumping, 10.15m);
			dataProvider = PaymentProvider.New(entryHeader.AllMergedLinesFees);
			AssertEquals("ANTIDUMPING", dataProvider.TaxType);
		}
	}
}

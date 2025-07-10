using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class CalculationOfTaxesProviderTest : DataProviderTestCase<CalculationOfTaxesProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<NullReferenceException>("JobComInvoiceLine missing", () => new CalculationOfTaxesProvider(null));
		}

		public void TestPreference()
		{
			SetUpTestData();
			invoiceLine.JI_PrimaryPreference = "AVC";
			AssertEquals("AVC", Provider.Preference);
		}

		public void TestTotalDutiesAndTaxesAmount()
		{
			AssertEquals(3m, Provider.TotalDutiesAndTaxesAmount);

			cusEntryLine.Fees.Clear();
			AssertEquals(decimal.Zero, GetProvider().TotalDutiesAndTaxesAmount);
		}

		public void TestDutiesAndTaxes()
		{
			var dutiesAndTaxes = Provider.DutiesAndTaxes.First();
			AssertType<DutiesandTaxesProvider>("Type", dutiesAndTaxes);
			AssertEquals("Sequence", "1", dutiesAndTaxes.SequenceNumber);

			cusEntryLine.Fees.Clear();
			AssertEquals(0, GetProvider().DutiesAndTaxes.Count);
		}

		protected override CalculationOfTaxesProvider GetProvider()
		{
			SetUpTestData();
			return new CalculationOfTaxesProvider(entryLineWrapper);
		}

		void SetUpTestData()
		{
			if (entryLineWrapper == null)
			{
				var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
				entryLineWrapper = testBizObjs.entryLineWrapper;
				entryLineWrapper.Declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				cusEntryLine = entryLineWrapper.EntryLine;
				invoiceLine = entryLineWrapper.RandomInvoiceLine;
				cusEntryLineFee = cusEntryLine.Fees.AddNew();
				cusEntryLineFee.CF_ChargeAmount = 1;
				cusEntryLineFee1 = cusEntryLine.Fees.AddNew();
				cusEntryLineFee1.CF_ChargeAmount = 2;
			}
		}

		EntryLineWrapper entryLineWrapper;
		CusEntryLineFee cusEntryLineFee;
		CusEntryLineFee cusEntryLineFee1;
		JobComInvoiceLine invoiceLine;
		CusEntryLine cusEntryLine;
	}
}

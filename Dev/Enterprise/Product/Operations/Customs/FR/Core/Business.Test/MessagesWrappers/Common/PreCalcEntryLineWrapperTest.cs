using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	public class PreCalcEntryLineWrapperTest : TestCaseWithFactory
	{
		public void TestPreCalcEntryLineWrapperConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PreCalcEntryLineWrapper(null));
		}

		public void TestPrecalcEntryLineRate()
		{
			fee.CF_Rate = 8.1m;
			fee.CF_BaseValue = 100m;
			AssertEquals("The precalc tax rate should be equal to 100 * CF_ChargeAmount/CF_BaseValue", 8.1m, preCalcEntryLineWrapper.Tax.TaxRate);
		}

		public void TestPreCalcEntryLineWrapperWhenTaxAmountIsZero()
		{
			fee.CF_ChargeType = "B01";
			AssertEquals("The EU code matching the French U165 code is not B00, so the IsVat property should be false.", false, preCalcEntryLineWrapper.IsVAT);

			fee.CF_ChargeType = "B00";
			AssertEquals("The EU code matching the French A445 code is B00, so the IsVat property should be true.", true, preCalcEntryLineWrapper.IsVAT);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var factory = new BusinessObjectFactory();
			var declaration = factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			fee = entryLine.ConfirmedFees.AddNew();
			preCalcEntryLineWrapper = new PreCalcEntryLineWrapper(fee);
		}
		CusEntryLineFee fee;
		PreCalcEntryLineWrapper preCalcEntryLineWrapper;
	}
}

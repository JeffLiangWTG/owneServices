using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;

namespace Enterprise.Customs.DE.Business.DocumentWrappers.Testing
{
	sealed class DocCusEntryHeaderFeeTest : DocBaseWrapperTest
	{
		public void TestNew()
		{
			AssertNull(DocEntryHeaderFee.New(null, Factory));
		}

		public void TestDescription()
		{
			fee.Description = "A";
			AssertEquals("A", Wrapper.Description);
		}

		public void TestChargeType()
		{
			fee.Description = "B00";
			AssertEquals("B00", Wrapper.Description);
		}

		public void TestChargeAmountAsString()
		{
			fee.ChargeAmount = 1.23456m;
			AssertEquals((ZString)"1,23", Wrapper.ChargeAmountAsString);
		}

		public void TestBaseValue()
		{
			fee.BaseValue = 1.23456m;
			AssertEquals("1,23", Wrapper.BaseValue);
		}

		public void TestMethodOfCalculation()
		{
			fee.MethodOfCalculation = "A";
			AssertEquals("A", Wrapper.MethodOfCalculation);
		}

		public void TestRate()
		{
			fee.Rate = 1.2345678m;
			AssertEquals("1,234568", Wrapper.Rate);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper() => DocEntryHeaderFee.New(fee, Factory);

		new DocEntryHeaderFee Wrapper => (DocEntryHeaderFee)base.Wrapper;

		protected override void SetUp()
		{
			base.SetUp();
			fee = new EntryFee();
		}
		EntryFee fee;
	}
}

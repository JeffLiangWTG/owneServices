using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing
{
	public class DebitCreditDataEntryTest : TestCaseWithFactory
	{
		public void TestInitialState()
		{
			AssertEquals("Debit Credit Sign", DebitCreditDataEntry.DR, DataEntry.DebitCreditSign);
			AssertUnsignedAmount(0M);
			AssertUnderlyingAmount(0M);
		}

		public void TestOnLoaded()
		{
			DataEntry.UnsignedAmount = 0;
			DataEntry.DebitCreditSign = "";
			UnderlyingAmount = 1;
			DataEntry.OnLoaded();
			AssertUnsignedAmount(1M);
			AssertDebitCreditSign(DebitCreditDataEntry.DR);

			DataEntry.UnsignedAmount = 0;
			DataEntry.DebitCreditSign = "";
			UnderlyingAmount = -1;
			DataEntry.OnLoaded();
			AssertUnsignedAmount(1M);
			AssertDebitCreditSign(DebitCreditDataEntry.CR);
		}

		public void TestDebitCreditSign()
		{
			UnderlyingAmount = 1;

			DataEntry.DebitCreditSign = DebitCreditDataEntry.CR;
			AssertEquals("Debit Credit Sign", DebitCreditDataEntry.CR, DataEntry.DebitCreditSign);
			AssertUnsignedAmount(1M);
			AssertUnderlyingAmount(-1M);

			DataEntry.DebitCreditSign = DebitCreditDataEntry.DR;
			AssertEquals("Debit Credit Sign", DebitCreditDataEntry.DR, DataEntry.DebitCreditSign);
			AssertUnsignedAmount(1M);
			AssertUnderlyingAmount(1M);
		}

		public void TestInvalidDebitCreditSign()
		{
			AssertInvalidDebitCreditSignIsUnaffectedBySettingAmounts("XX");
		}

		public void TestEmptyDebitCreditSign()
		{
			AssertInvalidDebitCreditSignIsUnaffectedBySettingAmounts("");
		}

		public void TestUnsignedAmount()
		{
			DataEntry.DebitCreditSign = DebitCreditDataEntry.DR;

			DataEntry.UnsignedAmount = -1;
			AssertUnsignedAmount(1M);
			AssertUnderlyingAmount(1M);

			DataEntry.UnsignedAmount = 1;
			AssertUnsignedAmount(1M);
			AssertUnderlyingAmount(1M);

			DataEntry.DebitCreditSign = DebitCreditDataEntry.CR;

			DataEntry.UnsignedAmount = -1;
			AssertUnsignedAmount(1M);
			AssertUnderlyingAmount(-1M);

			DataEntry.UnsignedAmount = 1;
			AssertUnsignedAmount(1M);
			AssertUnderlyingAmount(-1M);
		}

		public void TestUnderlyingAmount()
		{
			UnderlyingAmount = -1;
			AssertDebitCreditSign(DebitCreditDataEntry.CR);
			AssertUnsignedAmount(1M);
			AssertEquals("Amount", -1M, UnderlyingAmount);

			UnderlyingAmount = 1;
			AssertDebitCreditSign(DebitCreditDataEntry.DR);
			AssertUnsignedAmount(1M);
			AssertEquals("Amount", 1M, UnderlyingAmount);

			UnderlyingAmount = 0;
			AssertEquals("Debit Credit Sign", DebitCreditDataEntry.DR, DataEntry.DebitCreditSign);
			AssertUnsignedAmount(0M);
			AssertEquals("Amount", 0M, UnderlyingAmount);
		}

		public void TestList()
		{
			CodeDescriptionPairList list = DataEntry.List;
			AssertEquals("Count", 2, list.Count);
			Assert("Should contain " + DebitCreditDataEntry.DR, list.ContainsCode(DebitCreditDataEntry.DR));
			Assert("Should contain " + DebitCreditDataEntry.CR, list.ContainsCode(DebitCreditDataEntry.CR));
		}

		#region Implementation

		protected DummyBusinessObject Bizo;
		protected DebitCreditDataEntry DataEntry;

		protected override void SetUp()
		{
			base.SetUp();
			Bizo = Factory.New<DummyBusinessObject>();
			DataEntry = new DebitCreditDataEntry(() => Bizo.Z0_Decimal, x => Bizo.Z0_Decimal = x);
		}

		protected ZDecimal UnderlyingAmount
		{
			get { return Bizo.Z0_Decimal; }
			set { Bizo.Z0_Decimal = value; }
		}

		protected virtual void AssertDebitCreditSign(ZString expected)
		{
			AssertEquals("Debit Credit Sign", expected, DataEntry.DebitCreditSign);
		}

		void AssertUnsignedAmount(ZDecimal expected)
		{
			AssertEquals("Unsigned Amount", expected, DataEntry.UnsignedAmount);
		}

		protected virtual void AssertUnderlyingAmount(ZDecimal expected)
		{
			AssertEquals("Amount", expected, UnderlyingAmount);
		}

		void AssertInvalidDebitCreditSignIsUnaffectedBySettingAmounts(ZString invalidValue)
		{
			DataEntry.DebitCreditSign = invalidValue;
			AssertDebitCreditSign(invalidValue);

			UnderlyingAmount = -1;
			AssertDebitCreditSign(invalidValue);
			AssertUnsignedAmount(1M);
			AssertEquals("Amount", -1M, UnderlyingAmount);

			DataEntry.UnsignedAmount = 2;
			AssertDebitCreditSign(invalidValue);
			AssertUnsignedAmount(2M);
			AssertEquals("Amount", -2M, UnderlyingAmount);
		}

		#endregion
	}
}

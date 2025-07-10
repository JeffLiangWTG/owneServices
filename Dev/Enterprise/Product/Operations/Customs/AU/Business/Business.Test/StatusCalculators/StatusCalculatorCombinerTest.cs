using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class StatusCalculatorCombinerTest : TestCaseWithFactory
	{
		public void TestDeriveStatusIfEmptyWithMessages()
		{
			AssertEquals(false, dummy1.DeriveStatusIfEmptyWithMessagesCalled);
			AssertEquals(false, dummy2.DeriveStatusIfEmptyWithMessagesCalled);

			combiner.DeriveStatusIfEmptyWithMessages();

			AssertEquals(true, dummy1.DeriveStatusIfEmptyWithMessagesCalled);
			AssertEquals(true, dummy2.DeriveStatusIfEmptyWithMessagesCalled);
		}

		public void TestDeriveStatusNow()
		{
			AssertEquals(false, dummy1.DeriveStatusNowCalled);
			AssertEquals(false, dummy2.DeriveStatusNowCalled);

			combiner.DeriveStatusNow();

			AssertEquals(true, dummy1.DeriveStatusNowCalled);
			AssertEquals(true, dummy2.DeriveStatusNowCalled);
		}

		public void TestResetToOriginalCalled()
		{
			AssertEquals(false, dummy1.ResetToOriginalCalled);
			AssertEquals(false, dummy2.ResetToOriginalCalled);

			combiner.ResetToOriginal();

			AssertEquals(true, dummy1.ResetToOriginalCalled);
			AssertEquals(true, dummy2.ResetToOriginalCalled);
		}

		public void TestUserFriendlyStatus()
		{
			AssertEquals("DUMMYSTATUS\r\nDUMMYSTATUS\r\n", combiner.UserFriendlyStatusText);
		}

		CalculatedCusStatusCalculatorForTest dummy1;
		CalculatedCusStatusCalculatorForTest dummy2;
		StatusCalculatorCombiner combiner;

		protected override void SetUp()
		{
			base.SetUp();

			dummy1 = new CalculatedCusStatusCalculatorForTest();
			dummy2 = new CalculatedCusStatusCalculatorForTest();
			combiner = new StatusCalculatorCombiner(new ICalculatedCusStatusCalculator[] { dummy1, dummy2 });
		}

		sealed class CalculatedCusStatusCalculatorForTest : ICalculatedCusStatusCalculator
		{
			internal bool DeriveStatusIfEmptyWithMessagesCalled;

			internal bool DeriveStatusNowCalled;

			internal bool ResetToOriginalCalled;

			void ICalculatedCusStatusCalculator.DeriveStatusIfEmptyWithMessages()
			{
				DeriveStatusIfEmptyWithMessagesCalled = true;
			}

			void ICalculatedCusStatusCalculator.DeriveStatusNow()
			{
				DeriveStatusNowCalled = true;
			}

			ZString ICalculatedCusStatusCalculator.UserFriendlyStatusText => "DUMMYSTATUS";

			void ICalculatedCusStatusCalculator.ResetToOriginal()
			{
				ResetToOriginalCalled = true;
			}
		}
	}
}

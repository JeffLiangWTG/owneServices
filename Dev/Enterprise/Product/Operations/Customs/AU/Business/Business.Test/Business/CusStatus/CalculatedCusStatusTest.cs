using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CalculatedCusStatus))]
	public class CalculatedCusStatusTest : Customs.Business.Testing.CusStatusTest
	{
		public void TestDeriveStatusCalledOnCodeGet()
		{
			AssertEquals("status is blank", ZString.Empty, Status.Code);
			DummyCalculator.DeriveEnabled = true;
			AssertEquals("status is derived.", "Derived!", Status.Code);
		}

		public void TestUserFriendlyStatusesReturns()
		{
			AssertEquals("Status!", ((ICalculatedCusStatusCalculator)DummyCalculator).UserFriendlyStatusText);
		}

		#region Implementation

		#region TestHelpers

		protected class DummyCalculatorTestHelper : ICalculatedCusStatusCalculator
		{
			public DummyCalculatorTestHelper(ZPropertyInfo info)
			{
				this.info = info;
			}

			readonly ZPropertyInfo info;

			ZString ICalculatedCusStatusCalculator.UserFriendlyStatusText
			{
				get { return new ZString("Status!"); }
			}

			public void DeriveStatusIfEmptyWithMessages()
			{
				if (DeriveEnabled)
				{
					info.Value = new ZString("Derived!");
				}
			}

			public void ResetToOriginal()
			{
				info.Value = new ZString("Reset!");
			}

			public void DeriveStatusNow()
			{
				info.Value = new ZString("Derived Now!");
			}

			public bool DeriveEnabled;
		}

		#endregion

		#region Properties

		CalculatedCusStatus Status
		{
			get
			{
				if (fStatus == null)
				{
					fStatus = new CalculatedCusStatus(Dummy.StatusInfo, DummyCalculator, List);
				}
				return fStatus;
			}
		}
		CalculatedCusStatus fStatus;

		protected DummyCalculatorTestHelper DummyCalculator
		{
			get
			{
				if (fDummyCalculator == null)
				{
					fDummyCalculator = new DummyCalculatorTestHelper(Dummy.StatusInfo);
				}
				return fDummyCalculator;
			}
		}
		DummyCalculatorTestHelper fDummyCalculator;

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return Status;
		}

		#endregion
	}
}

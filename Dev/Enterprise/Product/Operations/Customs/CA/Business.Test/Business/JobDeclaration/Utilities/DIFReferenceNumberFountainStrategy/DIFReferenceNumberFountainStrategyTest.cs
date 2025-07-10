using System;
using System.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DIFReferenceNumberFountainStrategyTest : TestCaseWithFactory
	{
		public void TestGetDISReferenceNumber()
		{
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");

			TransactionNumberSetting setting = new TransactionNumberSetting(Factory, "Test", "12345", null, TransactionNumber.DIFNumberDeclarationType, true);
			setting.NextNumber = 100;
			setting.MaxNumber = 200;
			Factory.Save();

			var bo = Factory.New<DummyBOForTesting>();
			bo.NumberFountainStrategy = new DIFReferenceNumberFountainStrategy(Factory);
			Factory.Save();
			Assert("ReferenceNumber", bo.ReferenceNumber.StartsWith("1234500000100"));
		}

		[UseSnapshotProtection]
		public void TestGetError_NoErrors()
		{
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");

			var difFountain = TransactionNumber.GetNumberFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator("12345", null, TransactionNumber.DIFNumberDeclarationType)));
			difFountain.SetValues(Factory, minValue: 1, nextValue: 100000, maxValue: 19_999_999);

			IDISReferenceNumberFountainStrategy strategy = new DIFReferenceNumberFountainStrategy(Factory);
			var state = strategy.CheckState();
			AssertEquals(DISReferenceNumberFountainStrategyStateSeverity.Ok, state.Severity);
			AssertEquals(null, state.Message);
		}

		public void TestGetError_NoASECNumber()
		{
			IDISReferenceNumberFountainStrategy strategy = new DIFReferenceNumberFountainStrategy(Factory);
			var state = strategy.CheckState();
			AssertEquals(DISReferenceNumberFountainStrategyStateSeverity.Error, state.Severity);
			AssertEquals(TransactionNumberMessages.AsecNumberNotSet(), state.Message);
		}

		[UseSnapshotProtection]
		public void TestGetError_DIFRangeIsNotSet()
		{
			TestConnection.ExecuteNonQuery("DELETE FROM dbo.StmNums WHERE SN_Name like 'GeneratorFountain-C-%'");

			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");
			IDISReferenceNumberFountainStrategy strategy = new DIFReferenceNumberFountainStrategy(Factory);
			var state = strategy.CheckState();
			AssertEquals(DISReferenceNumberFountainStrategyStateSeverity.Error, state.Severity);
			AssertEquals(TransactionNumberMessages.DIFNotConfigured("12345"), state.Message);
		}

		[UseSnapshotProtection]
		public void TestRemainingCapacityValidation()
		{
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");

			var difFountain = TransactionNumber.GetNumberFountain(TransactionNumber.GetNumberFountainKey(TransactionNumber.GetRangeSeparator("12345", null, TransactionNumber.DIFNumberDeclarationType)));

			IDISReferenceNumberFountainStrategy strategy = new DIFReferenceNumberFountainStrategy(Factory);

			difFountain.SetValues(Factory, minValue: 1, nextValue: 10000, maxValue: 19_999_999);

			var state = strategy.CheckState();
			AssertEquals(DISReferenceNumberFountainStrategyStateSeverity.Ok, state.Severity);
			AssertEquals(null, state.Message);

			difFountain.SetNext(Factory, 19_999_000);
			state = strategy.CheckState();
			AssertEquals(DISReferenceNumberFountainStrategyStateSeverity.Ok, state.Severity);
			AssertEquals(null, state.Message);

			difFountain.SetNext(Factory, 19_999_001);
			state = strategy.CheckState();
			AssertEquals(DISReferenceNumberFountainStrategyStateSeverity.Warning, state.Severity);
			AssertEquals(TransactionNumberMessages.NumberRangeIsAlmostEmpty(true), state.Message);

			// due to that way number fountains work, we cannot set next value that is closer than 100 numbers to maximum value
			difFountain.SetNext(Factory, 19_999_898);
			for (int i = 0; i < 101; i++)
			{
				difFountain.GetNext(Factory);
			}
			AssertEquals(19_999_999, difFountain.PeekPreliminaryOrDefault(Factory, 0));
			state = strategy.CheckState();
			AssertEquals(DISReferenceNumberFountainStrategyStateSeverity.Warning, state.Severity);
			AssertEquals(TransactionNumberMessages.NumberRangeIsAlmostEmpty(true), state.Message);

			difFountain.GetNext(Factory);
			AssertEquals(19_999_999 + 1, difFountain.PeekPreliminaryOrDefault(Factory, 0));
			state = strategy.CheckState();
			AssertEquals(DISReferenceNumberFountainStrategyStateSeverity.Warning, state.Severity);
			AssertEquals(TransactionNumberMessages.DIFNumberRangeIsEmpty(), state.Message);

			state = strategy.CheckStateForAddingNewRecord();
			AssertEquals(DISReferenceNumberFountainStrategyStateSeverity.Error, state.Severity);
			AssertEquals(TransactionNumberMessages.DIFNumberRangeIsEmpty(), state.Message);

			var ex = AssertExceptionThrown<NumberFountain.NumberFountainMaximumValueReachedException>(() => difFountain.GetNext(Factory));
			AssertContains("Fountain has reached its maximum value of 19999999.", ex.Message);
		}

		class DummyBOForTesting : JobDeclaration
		{
			public DummyBOForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString ReferenceNumber { get; set; }
			public DIFReferenceNumberFountainStrategy NumberFountainStrategy { get; set; }

			public override void OnSaving()
			{
				base.OnSaving();
				ReferenceNumber = NumberFountainStrategy.GetDISReferenceNumber();
			}
		}
	}
}

using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(IEntryInstructionValidationDecider))]
	public abstract class EntryInstructionValidationDeciderTest<T> : TestCaseWithFactory
		where T : class, IEntryInstructionValidationDecider
	{
		public void TestIsRuleC0619ActiveForGoodsLocationDescription()
		{
			AssertEquals(ExpectedIsRuleC0619ActiveForGoodsLocationDescriptionResult, validationDecider.IsRuleC0619ActiveForGoodsLocationDescription);
		}
		protected abstract bool ExpectedIsRuleC0619ActiveForGoodsLocationDescriptionResult { get; }

		public void TestIsRuleC0626ActiveForCEI_OA_Warehouse2()
		{
			AssertEquals(ExpectedIsRuleC0626ActiveForCEI_OA_Warehouse2Result, validationDecider.IsRuleC0626ActiveForCEI_OA_Warehouse2);
		}
		protected abstract bool ExpectedIsRuleC0626ActiveForCEI_OA_Warehouse2Result { get; }

		public void TestIsRuleC0628ActiveForGoodsLocationDescription()
		{
			AssertEquals(ExpectedIsRuleC0628ActiveForGoodsLocationDescriptionResult, validationDecider.IsRuleC0628ActiveForGoodsLocationDescription);
		}
		protected abstract bool ExpectedIsRuleC0628ActiveForGoodsLocationDescriptionResult { get; }

		public void TestIsRuleC0829ActiveForCEI_OA_Warehouse2()
		{
			AssertEquals(ExpectedIsRuleC0829ActiveForCEI_OA_Warehouse2Result, validationDecider.IsRuleC0829ActiveForCEI_OA_Warehouse2);
		}
		protected abstract bool ExpectedIsRuleC0829ActiveForCEI_OA_Warehouse2Result { get; }

		public void TestIsRuleC0853ActiveForCEI_OA_Warehouse()
		{
			AssertEquals(ExpectedIsRuleC0853ActiveForCEI_OA_WarehouseResult, validationDecider.IsRuleC0853ActiveForCEI_OA_Warehouse);
		}
		protected abstract bool ExpectedIsRuleC0853ActiveForCEI_OA_WarehouseResult { get; }

		public void TestIsRuleC0382ActiveForGoodsLocationAddressHouseNumber()
		{
			AssertEquals(ExpectedIsRuleC0382ActiveForGoodsLocationAddressHouseNumberResult, validationDecider.IsRuleC0382ActiveForGoodsLocationAddressHouseNumber);
		}
		protected abstract bool ExpectedIsRuleC0382ActiveForGoodsLocationAddressHouseNumberResult { get; }

		public void TestIsIsMaximumEntryLinesAllowedRuleActive()
		{
			AssertEquals(ExpectedIsMaximumEntryLinesAllowedRuleActive, validationDecider.IsMaximumEntryLinesAllowedRuleActive);
		}
		protected abstract bool ExpectedIsMaximumEntryLinesAllowedRuleActive { get; }

		protected override void SetUp()
		{
			base.SetUp();
			validationDecider = GetNewValidationDecider();
		}

		protected virtual T GetNewValidationDecider()
		{
			return Activator.CreateInstance<T>();
		}

		protected T validationDecider;
	}
}

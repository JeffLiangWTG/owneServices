using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;
using VarianceSigns = Enterprise.Accounting.Registry.Business.CostVarianceApprovalAuthorisationRequirement.VarianceSigns;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CostVarianceApprovalAuthorisationRequirementCollection))]
	public class CostVarianceApprovalAuthorisationRequirementCollectionPositiveTest : CostVarianceApprovalAuthorisationRequirementCollectionBaseTest
	{
		protected override ZString VarianceSign
		{
			get { return VarianceSigns.Plus; }
		}
	}

	[TestedType(typeof(CostVarianceApprovalAuthorisationRequirementCollection))]
	public class CostVarianceApprovalAuthorisationRequirementCollectionNegativeTest : CostVarianceApprovalAuthorisationRequirementCollectionBaseTest
	{
		protected override ZString VarianceSign
		{
			get { return VarianceSigns.Minus; }
		}
	}

	public abstract class CostVarianceApprovalAuthorisationRequirementCollectionBaseTest : AmountBasedAuthorisationRequirementCollectionTest<CostVarianceApprovalAuthorisationRequirementCollection>
	{
		public void TestGetMatchingAuthorisationRequirement()
		{
			Collection.Parent.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			SetUpThreeItems();
			AssertAuthorisationRequirements(decimal.Zero);
			AssertAuthorisationRequirements(1000000m);

			Collection.Parent.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.PercentageVariance;
			SetUpThreeItems();
			AssertAuthorisationRequirements(100m);

			AssertNull("Null for 0", Collection.GetMatchingAuthorisationRequirement(decimal.Zero, decimal.Zero));
			AssertEquals("Item3 for 0.01 as the base for percentage is 0", item3, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 0.01m, decimal.Zero));
			AssertEquals("Item3 for 10 as the base for percentage is 0", item3, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 10m, decimal.Zero));
			AssertEquals("Item3 for 10.01 as the base for percentage is 0", item3, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 10.01m, decimal.Zero));
			AssertEquals("Item3 for 20 as the base for percentage is 0", item3, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 20m, decimal.Zero));
			AssertEquals("Item3 for 20.01 as the base for percentage is 0", item3, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 20.01m, decimal.Zero));

			Collection.Parent.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.PercentageVarianceAndMaximumLocalExTaxVariance;
			SetUpThreeItems();

			AssertNull("Null for 0", Collection.GetMatchingAuthorisationRequirement(decimal.Zero, decimal.Zero));
			AssertEquals("Item3 for 0.01 as the base for percentage is 0", item3, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 0.01m, decimal.Zero));
			AssertEquals("Item3 for 10 as the base for percentage is 0", item3, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 10m, decimal.Zero));
			AssertEquals("Item3 for 10.01 as the base for percentage is 0", item3, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 10.01m, decimal.Zero));
			AssertEquals("Item3 for 20 as the base for percentage is 0", item3, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 20m, decimal.Zero));
			AssertEquals("Item3 for 20.01 as the base for percentage is 0", item3, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 20.01m, decimal.Zero));

			AssertAuthorisationRequirements(100m);
			AssertAuthorisationRequirements(1000000m);
		}

		void AssertAuthorisationRequirements(decimal accrualAmount)
		{
			AssertNull("Null for 0", Collection.GetMatchingAuthorisationRequirement(decimal.Zero, accrualAmount));
			AssertEquals("Item1 for 0.01", item1, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 0.01m, accrualAmount));
			AssertEquals("Item1 for 10", item1, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 10m, accrualAmount));
			AssertEquals("Item2 for 10.01", item2, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 10.01m, accrualAmount));
			AssertEquals("Item2 for 20", item2, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 20m, accrualAmount));
			AssertEquals("Item3 for 20.01", item3, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 20.01m, accrualAmount));
		}

		public void TestGetAuthorisationRequiredForTotal()
		{
			#region Data Set Up

			SetUpThreeItems();
			item1.MonitorTotalInvoiceVariance = true;
			item1.TotalInvoiceVarianceAmount = 10m;
			item2.MonitorTotalInvoiceVariance = true;
			item2.TotalInvoiceVarianceAmount = 20m;
			item3.MonitorTotalInvoiceVariance = true;
			item3.TotalInvoiceVarianceAmount = 20m;

			var item2_ = (CostVarianceApprovalAuthorisationRequirement)GetNewElementToAddToTheCollection();
			Collection.Add(item2_);
			item2_.Range = RangeCodes.UpTo;
			item2_.MonitorTotalInvoiceVariance = false;
			item2_.TotalInvoiceVarianceAmount = 20m;

			var item3_ = (CostVarianceApprovalAuthorisationRequirement)GetNewElementToAddToTheCollection();
			Collection.Add(item3_);
			item3_.Range = RangeCodes.Above;
			item3_.MonitorTotalInvoiceVariance = false;
			item3_.TotalInvoiceVarianceAmount = 20m;

			#endregion

			AssertNull("Null for 0", Collection.GetAuthorisationRequiredForTotal(decimal.Zero));
			AssertEquals("Item1 for 0.01", item1, Collection.GetAuthorisationRequiredForTotal(VarianceMultiplier * 0.01m));
			AssertEquals("Item1 for 10", item1, Collection.GetAuthorisationRequiredForTotal(VarianceMultiplier * 10m));
			AssertEquals("Item2 for 10.01", item2, Collection.GetAuthorisationRequiredForTotal(VarianceMultiplier * 10.01m));
			AssertEquals("Item2 for 20", item2, Collection.GetAuthorisationRequiredForTotal(VarianceMultiplier * 20m));
			AssertEquals("Item3 for 20.01", item3, Collection.GetAuthorisationRequiredForTotal(VarianceMultiplier * 20.01m));
		}

		public void TestGetMatchingAuthorisationRequirementPAA_Normal()
		{
			Collection.Parent.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.PercentageVarianceAndLocalCostAmountExTax;
			Collection.RemoveAll();

			var itemNone = Collection.AddNew();
			itemNone.VarianceSign = VarianceSign;
			itemNone.Range = RangeCodes.UpTo;
			itemNone.Percentage = 10;
			itemNone.LocalCostAmount = 100m;

			var itemLevel1 = Collection.AddNew();
			itemLevel1.VarianceSign = VarianceSign;
			itemLevel1.Range = RangeCodes.UpTo;
			itemLevel1.Percentage = 20;
			itemLevel1.LocalCostAmount = 200m;

			var itemLevel2 = Collection.AddNew();
			itemLevel2.VarianceSign = VarianceSign;
			itemLevel2.Range = RangeCodes.Above;
			itemLevel2.Percentage = 20;
			itemLevel2.LocalCostAmount = 200m;

			AssertNull("Null for 0", Collection.GetMatchingAuthorisationRequirement(decimal.Zero, decimal.Zero));
			AssertEquals("itemNone, percentage and amount are both not over limit", itemNone, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 100m, 1000m));
			AssertEquals("itemNone, percentage is not over limit", itemNone, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 200m, 2000m));
			AssertEquals("itemNone, amount is not over limit", itemNone, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 100m, 500m));

			AssertEquals("itemLevel1, percentage and amount are both not over limit", itemLevel1, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 200m, 1000m));
			AssertEquals("itemLevel1, percentage is not over limit", itemLevel1, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 400m, 2000m));
			AssertEquals("itemLevel1, amount is not over limit", itemLevel1, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 200m, 500m));

			AssertEquals("itemLevel2, the final threshold", itemLevel2, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 210m, 1000m));
		}

		public void TestGetMatchingAuthorisationRequirementPAA_OnlyAbove()
		{
			Collection.Parent.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.PercentageVarianceAndLocalCostAmountExTax;
			Collection.RemoveAll();

			var itemAbove = Collection.AddNew();
			itemAbove.VarianceSign = VarianceSign;
			itemAbove.Range = RangeCodes.Above;
			itemAbove.Percentage = 20;
			itemAbove.LocalCostAmount = 200m;

			AssertNull("Null for 0", Collection.GetMatchingAuthorisationRequirement(decimal.Zero, decimal.Zero));

			AssertEquals("percentage is not over above limit, should not return restriction", null, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 400m, 2000m));
			AssertEquals("amount is not over above limit, should not return restriction", null, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 200m, 500m));

			AssertEquals("the Above restriction item, the final threshold", itemAbove, Collection.GetMatchingAuthorisationRequirement(VarianceMultiplier * 210m, 1000m));
		}

		public void TestGetElementsBySign()
		{
			#region Data Set Up

			var itemPositive1 = (CostVarianceApprovalAuthorisationRequirement)GetNewElementToAddToTheCollection();
			itemPositive1.VarianceSign = VarianceSigns.Plus;
			Collection.Add(itemPositive1);

			var itemPositive2 = (CostVarianceApprovalAuthorisationRequirement)GetNewElementToAddToTheCollection();
			itemPositive2.VarianceSign = VarianceSigns.Plus;
			Collection.Add(itemPositive2);

			var itemNegative1 = (CostVarianceApprovalAuthorisationRequirement)GetNewElementToAddToTheCollection();
			itemNegative1.VarianceSign = VarianceSigns.Minus;
			Collection.Add(itemNegative1);

			var itemNegative2 = (CostVarianceApprovalAuthorisationRequirement)GetNewElementToAddToTheCollection();
			itemNegative2.VarianceSign = VarianceSigns.Minus;
			Collection.Add(itemNegative2);

			#endregion

			AssertContainsExactElementsInAnyOrder("Any Positive amount should bring elements with VarianceSigns.Plus", new[] { itemPositive1, itemPositive2 }, Collection.GetElementsBySign(decimal.One));
			AssertEquals("No elements should be returned for Zero ", false, Collection.GetElementsBySign(decimal.Zero).Any());
			AssertContainsExactElementsInAnyOrder("Any Negative amount should bring elements with VarianceSigns.Minus", new[] { itemNegative1, itemNegative2 }, Collection.GetElementsBySign(decimal.MinusOne));
		}

		#region Implementation

		protected abstract ZString VarianceSign { get; }

		protected decimal VarianceMultiplier
		{
			get { return VarianceSign == CostVarianceApprovalAuthorisationRequirement.VarianceSigns.Plus ? decimal.One : decimal.MinusOne;  }
		}

		CostVarianceApprovalAuthorisationRequirement item1, item2, item3;

		void SetUpThreeItems()
		{
			if (item1 == null)
			{
				item1 = (CostVarianceApprovalAuthorisationRequirement)GetNewElementToAddToTheCollection();
				Collection.Add(item1);
			}
			item1.Range = RangeCodes.UpTo;
			item1.Amount = 10m;

			if (item2 == null)
			{
				item2 = (CostVarianceApprovalAuthorisationRequirement)GetNewElementToAddToTheCollection();
				Collection.Add(item2);
			}
			item2.Range = RangeCodes.UpTo;
			item2.Amount = 20m;

			if (item3 == null)
			{
				item3 = (CostVarianceApprovalAuthorisationRequirement)GetNewElementToAddToTheCollection();
				Collection.Add(item3);
			}
			item3.Range = RangeCodes.Above;
			item3.Amount = 20m;
		}

		protected override CostVarianceApprovalAuthorisationRequirementCollection GetCollectionToTest()
		{
			return new CostVarianceApprovalAuthorisationRequirementCollection(new CostVarianceApproval(), new FallbackLevel(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), null);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = new CostVarianceApprovalAuthorisationRequirement();
			result.VarianceSign = VarianceSign;
			return result;
		}

		#endregion
	}
}

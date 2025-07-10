using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(AccCommissionLineGroup))]
	internal class AccCommissionLineGroupTest : EnterpriseBusinessObjectTestCase
	{
		#region Override

		public void TestMarkAsOverriden()
		{
			var commissionLineGroup = Factory.New<AccCommissionLineGroup>();
			commissionLineGroup.CLG_TransactionAmount = 75;
			commissionLineGroup.CLG_TotalCommissionableAmount = 100;
			var commissionLine1 = commissionLineGroup.Lines.AddNew();
			var commissionLine2 = commissionLineGroup.Lines.AddNew();

			commissionLineGroup.MarkAsOverriden();

			AssertEquals(true, commissionLine1.IsOverriden);
			AssertEquals(true, commissionLine1.IsOverriden);
			AssertEquals(0m, commissionLineGroup.CLG_TransactionAmount);
			AssertEquals(0m, commissionLineGroup.CLG_TotalCommissionableAmount);
		}

		#endregion

		#region Decimals

		public void TestZDecimalsHaveCorrectDecimalPlacesAccCommissionLineGroup()
		{
			var group = Factory.New<AccCommissionLineGroup>();

			var transactionList = new List<string>
			{
				nameof(group.CLG_TransactionAmount)
			};

			var commissionList = new List<string>
			{
				nameof(group.CLG_TotalCommissionableAmount)
			};

			var tester = new DecimalPlacesAttributeTester(group);
			tester.CheckNonLocalCurrency(transactionList, nameof(group.TransactionDecimals), nameof(group.CLG_RX_NKTransactionCurrency), group);
			tester.CheckNonLocalCurrency(commissionList, nameof(group.CommissionDecimals), nameof(group.CLG_RX_NKCommissionCurrency), group);
		}

		#endregion

		#region Related Business Objects

		public void TestLines()
		{
			var commissionLineGroup = Factory.New<AccCommissionLineGroup>();
			AssertNotNull(commissionLineGroup.Lines);
		}

		#endregion
	}
}

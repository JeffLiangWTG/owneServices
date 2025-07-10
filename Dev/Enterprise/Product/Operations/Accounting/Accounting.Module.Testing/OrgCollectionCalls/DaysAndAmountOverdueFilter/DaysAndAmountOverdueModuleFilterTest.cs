using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(DaysAndAmountOverdueModuleFilter))]
	public class DaysAndAmountOverdueModuleFilterTest : ModuleNumberFilterTest
	{
		#region Properties

		public void TestDaysOverdueProperty1()
		{
			DaysAndAmountOverdueModuleFilter filter = (DaysAndAmountOverdueModuleFilter)GetNewModuleFilter();
			filter.DaysOverdue = 125;
			AssertEquals(filter.DaysOverdue.ToString(), filter.Property);
		}

		public void TestAmountOverdue()
		{
			DaysAndAmountOverdueModuleFilter filter = (DaysAndAmountOverdueModuleFilter)GetNewModuleFilter();
			filter.AmountOverdue = 155m;
			AssertEquals(155m, filter.AmountOverdue);
		}

		public void TestAndOrDecider()
		{
			DaysAndAmountOverdueModuleFilter filter = (DaysAndAmountOverdueModuleFilter)GetNewModuleFilter();
			filter.AndOrDecider = "OR";
			AssertEquals("OR", filter.AndOrDecider);
		}

		#endregion

		#region Empty

		public void TestIsEmpty()
		{
			DaysAndAmountOverdueModuleFilter filter = (DaysAndAmountOverdueModuleFilter)GetNewModuleFilter();
			AssertEquals(true, filter.IsEmpty);

			filter.DaysOverdue = 1;
			AssertEquals(false, filter.IsEmpty);

			filter.DaysOverdue = ZInt.Zero;
			filter.AmountOverdue = 1m;
			AssertEquals(false, filter.IsEmpty);
		}

		#endregion

		#region Clear

		public void TestClear()
		{
			DaysAndAmountOverdueModuleFilter filter = (DaysAndAmountOverdueModuleFilter)GetNewModuleFilter();
			filter.DaysOverdue = 1;
			filter.AmountOverdue = 1m;
			filter.AndOrDecider = "OR";
			filter.Clear();
			AssertEquals(ZInt.Zero, filter.DaysOverdue);
			AssertEquals(ZDecimal.Zero, filter.AmountOverdue);
			AssertEquals("AND", filter.AndOrDecider);
		}

		#endregion

		#region SetDefaultValues

		public void TestSetDefaultValues()
		{
			DaysAndAmountOverdueModuleFilter filter = (DaysAndAmountOverdueModuleFilter)GetNewModuleFilter();
			AssertEquals("AND", filter.AndOrDecider);
		}

		#endregion

		#region Query

		public void TestGetQuery()
		{
			DaysAndAmountOverdueModuleFilter filter = (DaysAndAmountOverdueModuleFilter)GetNewModuleFilter();
			AssertEquals("", filter.Query.LiteralTextADO);
			filter.DaysOverdue = 15;
			ZDateTime startDate = ZDateTime.Now.AddDays(-filter.DaysOverdue);
			string queryText = String.Format("CC_OldestDueDate <= {0}", startDate.ToString("#yyyy-MM-dd HH:mm:"));
			AssertContains(queryText, filter.Query.LiteralTextADO);
			filter.Clear();
			filter.AmountOverdue = 15m;
			AssertEquals("CC_TotalOverdueAmount >= 15", filter.Query.LiteralTextADO);
			filter.DaysOverdue = 155;
			filter.AndOrDecider = "AND";
			startDate = ZDateTime.Now.AddDays(-filter.DaysOverdue);
			string dateQueryText = String.Format("CC_OldestDueDate <= {0}", startDate.ToString("#yyyy-MM-dd HH:mm:"));
			AssertContains(dateQueryText, filter.Query.LiteralTextADO);
			AssertContains("and CC_TotalOverdueAmount >= 15", filter.Query.LiteralTextADO);
			filter.AndOrDecider = "OR";
			AssertContains(dateQueryText, filter.Query.LiteralTextADO);
			AssertContains("or CC_TotalOverdueAmount >= 15", filter.Query.LiteralTextADO);
		}

		#endregion

		#region AndOrDeciderList

		public void TestAndOrDeciderList()
		{
			DaysAndAmountOverdueModuleFilter filter = (DaysAndAmountOverdueModuleFilter)GetNewModuleFilter();
			Assert(filter.AndOrDeciderList.ContainsCode("AND"));
			Assert(filter.AndOrDeciderList.ContainsCode("OR"));
		}

		#endregion

		#region Implementation

		protected override ModuleTextFilter GetNewModuleFilter()
		{
			return new DaysAndAmountOverdueModuleFilter("moo");
		}

		#endregion
	}
}

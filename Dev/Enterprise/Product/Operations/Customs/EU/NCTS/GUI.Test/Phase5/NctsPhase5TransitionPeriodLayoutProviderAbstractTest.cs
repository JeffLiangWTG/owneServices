using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestsSubclassesOf(typeof(INctsPhase5TransitionPeriodLayoutProvider))]
	public abstract class NctsPhase5TransitionPeriodLayoutProviderAbstractTest : TestCaseWithFactory
	{
		public void TestGetHouseConsignmentGridColumnLayout()
		{
			AssertEquals(ExpectedGetHouseConsignmentDetailsGridColumnLayout, Provider.GetHouseConsignmentDetailsGridColumnLayout().GetType());
		}

		public void TestGetDepartureGoodsItemsGridColumnLayout()
		{
			AssertEquals(ExpectedGetDepartureGoodsItemsGridColumnLayout, Provider.GetDepartureGoodsItemsGridColumnLayout().GetType());
		}

		protected abstract Type ExpectedGetDepartureGoodsItemsGridColumnLayout { get; }

		protected abstract Type ExpectedGetHouseConsignmentDetailsGridColumnLayout { get; }

		protected abstract INctsPhase5TransitionPeriodLayoutProvider Provider { get; }
	}
}

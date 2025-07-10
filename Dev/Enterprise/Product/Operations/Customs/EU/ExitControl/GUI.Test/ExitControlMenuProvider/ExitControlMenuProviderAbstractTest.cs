using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	[TestsSubclassesOf(typeof(IExitControlMenuProvider))]
	public abstract class ExitControlMenuProviderAbstractTest<T> : TestCaseWithFactory
	where T : IExitControlMenuProvider, new()
	{
		public void TestGetConsignmentsGridUserControlMenuProvider()
		{
			var header = Factory.New<CusExitHeader>();
			using (var gridProvider = new ConsignmentsGridUserControlProviderForTesting(header))
			{
				gridProvider.UserControl.SetDataBinding(header.CusExitConsignments, "");
				AssertType(ExpectedConsignmentsGridUserControlMenuProviderType, provider.GetConsignmentsGridUserControlMenuProvider(gridProvider));
			}
		}

		protected abstract Type ExpectedConsignmentsGridUserControlMenuProviderType { get; }

		public void TestGetReportsGridUserControlMenuProvider()
		{
			var header = Factory.New<CusExitHeader>();
			using (var gridProvider = new ReportsGridUserControlProviderForTesting())
			{
				gridProvider.UserControl.SetDataBinding(header.CusExitReports, "");
				AssertType(ExpectedReportsGridUserControlMenuProviderType, provider.GetReportsGridUserControlMenuProvider(gridProvider));
			}
		}

		protected abstract Type ExpectedReportsGridUserControlMenuProviderType { get; }

		public void TestGetExitControlMainMenuProvider()
		{
			var header = Factory.New<CusExitHeader>();
			AssertType(ExpectedExitControlMainMenuProviderType, provider.GetExitControlMainMenuProvider(header));
		}

		protected abstract Type ExpectedExitControlMainMenuProviderType { get; }

		protected abstract string CountryOrGroupingCode { get; }

		protected override void SetUp()
		{
			base.SetUp();
			provider = ExitControlMenuProviderManager.GetMenuProvider(CountryOrGroupingCode);
		}
		IExitControlMenuProvider provider;
	}
}

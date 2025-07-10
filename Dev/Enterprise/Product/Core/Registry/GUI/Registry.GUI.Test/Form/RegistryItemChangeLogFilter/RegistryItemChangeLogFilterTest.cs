using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Environment.Registry;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Test
{
	[TestClass]
	[TestedType(typeof(RegistryItemChangeLogFilter))]
	sealed class RegistryItemChangeLogFilterTest : TestCaseWithFactory
	{
		readonly ZDateTime _today = new(DateTime.Today);
		readonly ZDateTime _yesterday = new(DateTime.Today.AddDays(-1));
		readonly ZDateTime _lastWeek = new(DateTime.Today.AddDays(-7));

		public void TestFilterHandlesEmptySetOfRegistryItems()
		{
			var filter = new RegistryItemChangeLogFilter(Factory);
			var mockProgressHandler = new Mock<IProgress<int>>();
			var mockFilterParameters = new Mock<IRegistryChangeLogFilterParameters>();
			mockFilterParameters.SetupAllProperties();
			mockFilterParameters.Setup(x => x.GenerateSubQueryFilterFromFilterParameters(It.IsAny<ZDBOnlySubQuery>()))
				.Returns(new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent, StmDataSchema.PK));

			var results = filter.FilterRegistryItemsWithEvents(new List<IRegistryItem>(), mockFilterParameters.Object, mockProgressHandler.Object);

			AssertNotNull(results);
			Assert(!results.Any());
		}

		public void TestFilterHandlesNullProgessHandler()
		{
			var filter = new RegistryItemChangeLogFilter(Factory);
			var mockFilterParameters = new Mock<IRegistryChangeLogFilterParameters>();
			mockFilterParameters.SetupAllProperties();
			mockFilterParameters.Setup(x => x.GenerateSubQueryFilterFromFilterParameters(It.IsAny<ZDBOnlySubQuery>()))
				.Returns(new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent, StmDataSchema.PK));

			var results = filter.FilterRegistryItemsWithEvents(GenerateRegistryItemsForTest(), mockFilterParameters.Object);
			AssertNotNull(results);
			Assert(!results.Any());
		}

		public void TestFilterExcludesRegistryItemsWithNoChangeEvents()
		{
			var filter = new RegistryItemChangeLogFilter(Factory);
			var mockProgressHandler = new Mock<IProgress<int>>();
			var filterParameters = new FilterRegistryChangeLogsByDate()
			{
				FromDate = _yesterday,
				ToDate = _today
			};

			var results =
				filter.FilterRegistryItemsWithEvents(GenerateRegistryItemsForTest(), filterParameters, mockProgressHandler.Object);

			AssertNotNull(results);
			Assert(!results.Any());
		}

		public void TestFilterIncludesRegistryItemsWithChangeEvents()
		{
			var filter = new RegistryItemChangeLogFilter(Factory);
			var mockProgressHandler = new Mock<IProgress<int>>();
			var regItems = GenerateRegistryItemsWithEvents(new ZDateTime(DateTime.Today.AddHours(-6)), 10);
			var filterParameters = new FilterRegistryChangeLogsByDate()
			{
				FromDate = _yesterday,
				ToDate = _today
			};

			var results =
				filter.FilterRegistryItemsWithEvents(regItems, filterParameters, mockProgressHandler.Object);

			AssertNotNull(results);
			Assert(results.Count() == regItems.Count);
		}

		public void TestFilterExcludesRegistryItemsWithChangeEventsOutsideDateRange()
		{
			var filter = new RegistryItemChangeLogFilter(Factory);
			var mockProgressHandler = new Mock<IProgress<int>>();
			var regItemsWithValidEvents = GenerateRegistryItemsWithEvents(new ZDateTime(DateTime.Today.AddHours(-6)), 10);
			var regItemsWithInvalidEvents = GenerateRegistryItemsWithEvents(_lastWeek, 10);
			var allRegItems = regItemsWithValidEvents.Concat(regItemsWithInvalidEvents).ToList();
			var filterParameters = new FilterRegistryChangeLogsByDate()
			{
				FromDate = _yesterday,
				ToDate = _today
			};

			var results =
				filter.FilterRegistryItemsWithEvents(allRegItems, filterParameters, mockProgressHandler.Object);

			AssertNotNull(results);
			Assert(results.Count() == regItemsWithValidEvents.Count);
		}

		public void TestFilterReportsToProgressHandler()
		{
			var filter = new RegistryItemChangeLogFilter(Factory);
			var numItems = 1000;
			var numItemsProcessed = 0;
			var mockProgressHandler = new Mock<IProgress<int>>();
			mockProgressHandler.Setup(p => p.Report(It.IsAny<int>())).Callback<int>(x => numItemsProcessed += x);
			var filterParameters = new FilterRegistryChangeLogsByDate()
			{
				FromDate = _yesterday,
				ToDate = _today
			};

			var results =
				filter.FilterRegistryItemsWithEvents(GenerateRegistryItemsForTest(numItems), filterParameters, mockProgressHandler.Object);

			AssertNotNull(results);
			Assert(numItemsProcessed == numItems);
		}

		static IList<IRegistryItem> GenerateRegistryItemsForTest(int numItems = 100)
		{
			return Enumerable.Range(0, numItems).Select(i => NewRegistryItem()).ToList();
		}

		IList<IRegistryItem> GenerateRegistryItemsWithEvents(ZDateTime loggedTime, int numItems = 100)
		{
			var regItems = Enumerable.Range(0, numItems).Select(_ => NewRegistryItem()).ToList();
			foreach (var regItem in regItems)
			{
				GenerateLogDataForRegistryItem(regItem, loggedTime);
			}
			return regItems;
		}

		void GenerateLogDataForRegistryItem(IRegistryItem registryItem, ZDateTime loggedTime)
		{
			var stmData = Factory.New<StmData>();
			stmData.SD_Name = registryItem.Name;
			stmData.SD_SystemLastEditTimeUtc = loggedTime;

			var stmLog = Factory.New<StmALog>();
			using (stmLog.LockForUpdatingKeyFieldsForTesting())
			{
				stmLog.SL_Table = StmDataSchema.Constants.TableName;
				stmLog.SL_Parent = stmData.PK;
				stmLog.SL_EventTimeUtc = loggedTime;
			}
			Factory.Save();
		}

		static IRegistryItem NewRegistryItem()
		{
			return new StringRegistryItem(ZGuid.NewZGuid().ToString(), (NoResString)"", (NoResString)"", (NoResString)"",
				RegistryStorageFlags.All);
		}
	}
}

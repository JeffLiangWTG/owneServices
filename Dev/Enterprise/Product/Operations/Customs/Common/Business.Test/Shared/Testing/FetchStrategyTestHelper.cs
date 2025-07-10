using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Common.Testing
{
	public static class FetchStrategyTestHelper
	{
		public static void AssertFetchForDelete<TBusinessObject>(Type testClassType, BusinessObjectFactory factory, SetupFetchHintsTestCase<TBusinessObject> setupFetchHintsTestCase, string[] tablesToCollectQueriesFor = null)
			where TBusinessObject : BusinessObject
		{
			AssertFetchHints(testClassType,
				factory,
				setupFetchHintsTestCase,
				businessObject => businessObject.FetchStrategy.FetchForDelete(),
				businessObject => businessObject.Delete(),
				tablesToCollectQueriesFor: tablesToCollectQueriesFor
			);
		}

		public static void AssertFetchForLoadChildEditableObjects<TBusinessObject>(Type testClassType, BusinessObjectFactory factory, SetupFetchHintsTestCase<TBusinessObject> setupFetchHintsTestCase, string[] tablesToCollectQueriesFor = null)
			where TBusinessObject : BusinessObject
		{
			AssertFetchHints(testClassType,
				factory,
				setupFetchHintsTestCase,
				businessObject => businessObject.FetchStrategy.FetchForLoadChildEditableObjects(),
				businessObject => businessObject.LoadChildEditableObjects(),
				businessObject =>
				{
					var childrenNotFetchHinted = new List<string>();
					foreach (var child in ((IBusiness)businessObject).Children)
					{
						if (!string.IsNullOrEmpty(child.TableName) && businessObject.Factory.GetLoadedFetchHintCountForTable(child.TableName) < 1)
						{
							childrenNotFetchHinted.Add(child.TableName);
						}
					}
					NUnit.Framework.AssertionWithHtml.HtmlAssert($"<caption><b>{testClassType.FullName}<b>The following tables are children of {businessObject.TableName}, but not FetchHinted: {string.Join(",", childrenNotFetchHinted)}</b></caption>", !childrenNotFetchHinted.Any());
				},
				tablesToCollectQueriesFor: tablesToCollectQueriesFor
			);
		}

		public static void AssertFetchForValidate<TBusinessObject>(Type testClassType, BusinessObjectFactory factory, SetupFetchHintsTestCase<TBusinessObject> setupFetchHintsTestCase, string[] tablesToCollectQueriesFor = null)
			where TBusinessObject : BusinessObject
		{
			AssertFetchHints(testClassType,
				factory,
				setupFetchHintsTestCase,
				businessObject => businessObject.FetchStrategy.FetchForValidate(),
				businessObject => businessObject.RunPreSaveValidation(),
				tablesToCollectQueriesFor: tablesToCollectQueriesFor
			);
		}

		public static void AssertFetchHints<TBusinessObject>(Type testClassType, BusinessObjectFactory factory, SetupFetchHintsTestCase<TBusinessObject> setupFetchHintsTestCase, FetchHintAction<TBusinessObject> fetchHintAction, ExecuteAction<TBusinessObject> executeAction, Action<TBusinessObject> additionalCheck = null, string[] tablesToCollectQueriesFor = null)
			where TBusinessObject : BusinessObject
		{
			NUnit.Framework.AssertionWithHtml.CombineAssertions(() =>
			{
				setupFetchHintsTestCase(out var testCases);
				foreach (var testCase in testCases)
				{
					factory.Save();
					factory.ResetDatabaseLoadCount();
					RowFactory.ResetCacheAfterDbUpgrade();

					var newFactory = new BusinessObjectFactory
					{
						NameForDebugging = FactoryNameForDebugging
					};
					var businessObjectInNewFactory = newFactory.Load<TBusinessObject>(testCase.BusinessObject.PK);

					ExecuteTestCaseCore(testClassType.FullName, businessObjectInNewFactory, testCase, fetchHintAction, executeAction, additionalCheck, null, " - With ClusterKey FetchHints", tablesToCollectQueriesFor: tablesToCollectQueriesFor, shouldAddTorTest: (x) => x.StartsWith(FactoryNameForDebugging, StringComparison.Ordinal));
				}
			});
		}
		const string FactoryNameForDebugging = "Factory With ClusterKey FetchHints";

		static void ExecuteTestCaseCore<TBusinessObject>(string testClassFullName, TBusinessObject businessObject, TestCase<TBusinessObject> testCase, FetchHintAction<TBusinessObject> fetchHintAction, ExecuteAction<TBusinessObject> executeAction, Action<TBusinessObject> additionalCheck, BusinessObjectFactory oldFactory, ZString executingTestCase, string[] tablesToCollectQueriesFor, Func<string, bool> shouldAddTorTest)
			where TBusinessObject : BusinessObject
		{
			var tablesToIgnore = new[] { RefCountrySchema.Constants.TableName };
			var factory = businessObject.Factory;
			factory.DropHints();
			var expectedHitCounts = testCase.FetchStrategyExpectedHitCounts ?? new Dictionary<string, int>();
			var message = FormattableString.Invariant($"{testCase.Message}{executingTestCase} - FetchStrategy should not result in multiple DB hits per table.\r\n{testClassFullName}");
			using (TestCaseWithFactory.AssertDbHitsForAllFactories(message, expectedHitCounts, true, false, 1, f => !ExcludedFactoryList.Contains(f.NameForDebugging), tablesToIgnore: tablesToIgnore, tablesToCollectQueriesFor: tablesToCollectQueriesFor, shouldCollectQueriesForExistingFactory: shouldAddTorTest))
			{
				fetchHintAction(businessObject);
			}

			expectedHitCounts = testCase.ExecuteActionExpectedHitCounts ?? new Dictionary<string, int>();
			message = FormattableString.Invariant($"{testCase.Message}{executingTestCase} - Execute should not result in multiple DB hits per table.\r\n{testClassFullName}");
			using (TestCaseWithFactory.AssertDbHitsForAllFactories(message, expectedHitCounts, true, false, 1, f => !ExcludedFactoryList.Contains(f.NameForDebugging) && f != oldFactory, tablesToIgnore: tablesToIgnore, tablesToCollectQueriesFor: tablesToCollectQueriesFor, shouldCollectQueriesForExistingFactory: shouldAddTorTest))
			{
				executeAction(businessObject);
			}

			expectedHitCounts = testCase.UnconsumedExpectedHitCounts ?? new Dictionary<string, int>();
			message = FormattableString.Invariant($"{testCase.Message}{executingTestCase} - These are the unconsumed FetchHints.\r\n{testClassFullName}");
			using (TestCaseWithFactory.AssertDbHitsForAllFactories(message, expectedHitCounts, true, false, testCase.ThresholdForUnspecified, f => !ExcludedFactoryList.Contains(f.NameForDebugging) && f != oldFactory, tablesToIgnore: tablesToIgnore, tablesToCollectQueriesFor: tablesToCollectQueriesFor, shouldCollectQueriesForExistingFactory: shouldAddTorTest))
			{
				factory.ExecuteAllFetchHints();
			}

			additionalCheck?.Invoke(businessObject);
		}

		public static IImmutableList<string> ExcludedFactoryList => new List<string>
		{
			"UserContext",
			"Client side Cache",
			"ServiceHostProvider Factory",
			"ZZCustomsFunctionalityEffectiveDate"
		}.ToImmutableList();

		public delegate void ExecuteAction<in TBusinessObject>(TBusinessObject businessObject)
			where TBusinessObject : BusinessObject;

		public delegate void FetchHintAction<in TBusinessObject>(TBusinessObject businessObject)
			where TBusinessObject : BusinessObject;

		public delegate void SetupFetchHintsTestCase<TBusinessObject>(out IList<TestCase<TBusinessObject>> testCases)
			where TBusinessObject : BusinessObject;

		public class TestCase<TBusinessObject>
			where TBusinessObject : BusinessObject
		{
			public string Message { get; set; }
			public TBusinessObject BusinessObject { get; set; }
			public IDictionary<string, int> FetchStrategyExpectedHitCounts { get; set; }
			public IDictionary<string, int> ExecuteActionExpectedHitCounts { get; set; }
			public IDictionary<string, int> UnconsumedExpectedHitCounts { get; set; }
			public int ThresholdForUnspecified { get; set; }
		}
	}
}

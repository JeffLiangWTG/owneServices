using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformations.Transforms;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	[TestsSubclassesOf(typeof(DataTransformation), new Type[0],
		new[]
		{
			typeof(SchemaChangeDataTransformation),  // this is specifically tested here - DataCopyTestCase
			typeof(DescriptionOnlyTransformation),
		})]
	public abstract partial class DataTransformationTestCase : TransactionedTestCase
	{
		public void TestTransformationRunsWithoutErrorsAgainAfterDBUpgrade()
		{
			// We don't do data preparation for this test because the database is already upgraded and all transformations are applied before any test run.
			// So, the transformation run below is executed after the regular database upgrade process.
			AssertNoExceptionThrown(RunTransformation);
		}

		public void TestRunAndAssertResultsTwice()
		{
			TestRunAndAssertResultsTwice<object>(() =>
			{
				PrepareTestData();
				return null;
			},
				_ => AssertPreConditions(),
				_ => AssertTransformationResults());
		}

		protected void TestRunAndAssertResultsTwice<T>(Func<T> prepareTestData, Action<T> assertPreConditions, Action<T> assertTransformationResults)
		{
			using (TransformationRunTwiceTestContextSetupAndDispose())
			{
				var testData = prepareTestData();

				assertPreConditions(testData);

				RunTransformation();
				assertTransformationResults(testData);

				RunTransformation();
				assertTransformationResults(testData);
			}
		}

		protected virtual void RunTransformation()
		{
			using (CheckNoTablesCreated())
			{
				if (TransformationTestShouldBeRunAgainstNewInstance)
				{
					var transformation = GetNewTestTransformationInstance();
					transformation.Run();
				}
				else
				{
					TransformationToTest.Run();
				}
			}
		}

		protected virtual bool TransformationTestShouldBeRunAgainstNewInstance => false;

		protected IDisposable CheckNoTablesCreated()
		{
			if (CanCreateAnyTable)
			{
				return null;
			}

			_ = TestConnection.ExecuteNonQuery("SELECT name into #BeforeTest FROM sys.tables WHERE is_ms_shipped = 0");

			return new DisposableAction(
				() =>
				{
					if (IsTransactionRolledBack())
					{
						return;
					}

					var sql = "SELECT isnull(min(t.name), '') FROM sys.tables t LEFT JOIN #BeforeTest bt ON bt.name = t.name WHERE t.is_ms_shipped = 0 AND t.name not like 'Client%' AND t.name not like 'Rpt%' AND bt.name is null; DROP TABLE #BeforeTest";
					var top1 = TestConnection.ExecuteScalar<string>(sql);
					Assert($"'{top1}' table was created by this transform. Please ensure if you use tables in online transforms that they start with the name 'Client'", string.IsNullOrEmpty(top1));
				}
			);
		}

		protected virtual bool CanCreateAnyTable => false;

		protected bool IsTransactionRolledBack()
		{
			return TestConnection.AppTransactionCount > 0 && !TestConnection.IsInTransaction;
		}

		protected virtual IDisposable TransformationRunTwiceTestContextSetupAndDispose() => DisposableAction.NoAction;

		/// <summary>
		/// NEW Instance of transformation being tested
		/// </summary>
		protected abstract DataTransformation GetNewTestTransformationInstance();
		/// <summary>
		/// Prepares test data before running the transformation
		/// </summary>
		protected virtual void PrepareTestData() { }

		/// <summary>
		/// Asserts data before running the transformation. This method is called right after the PrepareTestData() call.
		/// </summary>
		protected virtual void AssertPreConditions() { }

		/// <summary>
		/// Asserts data after running the transformation. This method is called twice (once after each run).
		/// </summary>
		protected virtual void AssertTransformationResults() { }

		#region TransformationToTest

		protected DataTransformation TransformationToTest => transformationToTest ??= GetNewTestTransformationInstance();

		DataTransformation transformationToTest;

		#endregion

		#region TestIsMapped

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Only using BaseSourcePath for local testing, not on DAT")]
		public void TestIsMapped()
		{
			var isMapped = IsTransformationMapped(TransformationToTest);

			if (string.IsNullOrWhiteSpace(ReasonNotToBeMapped))
			{
				HtmlAssert("This transform is not mapped but it should be.<br/>" + SeeWikiMessage, isMapped);
			}
			else
			{
				Assert($"This transformation is mapped but it shouldn't be (Reason: {ReasonNotToBeMapped})", !isMapped);
			}
		}

		protected virtual string ReasonNotToBeMapped => null;

		protected const string SeeWikiMessage = "Please see the <a href=\"https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/Data%20Transformation.aspx\">wiki</a> for instructions.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Only using BaseSourcePath for local testing, not on DAT")]
		protected bool IsTransformationMapped(DataTransformation dataTransformation)
		{
			var dataTransformationType = dataTransformation.GetType();

			bool result = IsTransformationTypeMapped(dataTransformationType);

			if (
				!result
				// It's nested class
				&& dataTransformationType.IsNestedPrivate
				// It's declared in the current test class or its parent
				&& (dataTransformationType.DeclaringType == GetType() || dataTransformationType.DeclaringType == GetType().BaseType))
			{
				var baseTransformType = dataTransformationType.BaseType;
				result = baseTransformType.IsAbstract
					|| IsTransformationTypeMapped(baseTransformType);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Only using BaseSourcePath for local testing, not on DAT")]
		protected static bool IsTransformationTypeMapped(Type dataTransformationType)
		{
			bool isMapped = false;

			if (!TestingState.IsRunningOnDAT)
			{
				string expectedTypeText = dataTransformationType.FullName;
				string mapperFile = Path.Combine(TestCase.BaseSourcePath, "Database", "Odyssey", "Transformations", "Transformations", "Transforms", "ShelfCheckinMapper.txt");

				using TextReader reader = new StreamReader(mapperFile);

				while (reader.ReadLine() is { } line)
				{
					if (line == expectedTypeText)
					{
						isMapped = true;
						break;
					}
				}
			}

			if (!isMapped)
			{
				foreach (var transform in Mapper.GetAllMappings())
				{
					Type transformType = transform.TransformationType;

					if (transformType == dataTransformationType)
					{
						isMapped = true;
						break;
					}
				}
			}

			return isMapped;
		}

		#endregion

		public void TestIsInvalidIndexProvider()
		{
			if (TransformationToTest is ITransformationIndexProvider)
			{
				var type = TransformationToTest.GetType();
				Assert("Only offline transformations should implement ITransformationIndexProvider. Temporary indexes created by the DbUpgrader are only used in the offline part of the upgrade.",
					IsOfflineUpgradeTransformMethodOverriden("OfflinePreUpgradeTransform") || IsOfflineUpgradeTransformMethodOverriden("OfflinePostUpgradeTransform"));

				bool IsOfflineUpgradeTransformMethodOverriden(string offlineUpgradeTransformMethodName)
				{
					var methodInfo = type.GetMethod(offlineUpgradeTransformMethodName, BindingFlags.NonPublic | BindingFlags.Instance, null, Array.Empty<Type>(), null);
					return methodInfo != null && methodInfo.DeclaringType != methodInfo.GetBaseDefinition().DeclaringType;
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestIndexFilterDefinitionSameAsStoredInDatabase()
		{
			if (TransformationToTest is ITransformationIndexProvider transformationInstance)
			{
				var type = TransformationToTest.GetType();
				CombineAssertions($@"{type.Name}, {type.Namespace}
-------------------------------------------------------------------------------------------------
Index filter definition created in the database is different from IndexInfo.Filter as provided in the code,
please correct the code with that created in the database so that they are exactly the same.
-------------------------------------------------------------------------------------------------",
				() =>
				{
					foreach (var indexInfo in transformationInstance.IndexProvider)
					{
						if (indexInfo.HasFilter)
						{
							indexInfo.Drop(TestConnection);
							_ = indexInfo.Create(TestConnection);
							var indexInfoCreatedInTheDatabase = IndexLoader.LoadTop1(TestConnection, indexInfo.SchemaName, indexInfo.TableName, indexInfo.IndexName);

							AssertEquals(indexInfoCreatedInTheDatabase.Filter, indexInfo.Filter);
						}
					}
				});
			}

			Assert(true);
		}

		public void TestTransformIsStateless()
		{
			// Baseline list of failing classes, this list should only shrink as classes are fixed or deleted
			// When this list becomes empty we can remove it.
			var baseline = new List<Type>
			{
				// NOTE TO REVIEWERS: This PR has changed the baseline of this test, please ensure that no new classes have been added to the baseline (removals are fine).
				typeof(Transformations.Transforms.Registry.UpdateEnableBoleroEBLAndEHBLIntegrationRegistryDataTransformation),
				typeof(Transformations.Transforms.DocumentScanning.PopulateRT_ParseTypeInRefDocTypeTableOffline),
				typeof(Transformations.Transforms.DocumentScanning.PopulateSC_SystemCreateUserFromLogs),
				typeof(Transformations.Transforms.Documents.ExternalStorageSizeRetrieval),
				// NOTE TO REVIEWERS: This PR has changed the baseline of this test, please ensure that no new classes have been added to the baseline (removals are fine).
				typeof(Transformations.Freight.ContainerYard.UpdatePickupAndReadyDateForROILine),
				typeof(Transformations.Transforms.Customs.US.UpdateISFJobBF_JS_Shipment),
				typeof(Transformations.BusinessIntelligence.RecreateIndexesForAuditDb),
				typeof(Transformations.BusinessIntelligence.RecreateIndexesForAuditDb),
				// NOTE TO REVIEWERS: This PR has changed the baseline of this test, please ensure that no new classes have been added to the baseline (removals are fine).
				typeof(Transformations.Transforms.BufferManagement.PurgeBMSystemFromStmALog),
				typeof(Transformations.Transforms.BufferManagement.PurgeBMComponentFromStmALog),
				typeof(Transformations.Transforms.BufferManagement.PurgeBMBoardSlideshowFromStmALog),
				typeof(Transformations.Transforms.BufferManagement.PurgeBMControlCustomisationFromStmALog),
				typeof(Transformations.Transforms.BufferManagement.PurgeBMBoardFromStmALog),
				typeof(Transformations.Transforms.BufferManagement.PurgeBMNCNShapeFromStmALog),
				typeof(Transformations.Transforms.Core.PurgeRedundantStorageMainLogsFromStmALog),
				// NOTE TO REVIEWERS: This PR has changed the baseline of this test, please ensure that no new classes have been added to the baseline (removals are fine).
			};

			var type = TestedTypeHelper.GetTestedType(GetType());
			var fields = new List<FieldInfo>();
			var currentType = type;
			while (currentType != typeof(DataTransformation) && currentType != null)
			{
				fields.AddRange(currentType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly));
				currentType = currentType.BaseType;
			}
			Assert("TestedType should be a strict subclass of DataTransformation", currentType != null);

			var disallowedFields = fields
				.Where(f => f.DeclaringType != typeof(DataTransformation))
				.Where(f => !f.IsLiteral)
				.Where(f => !(f.FieldType.IsPrimitive && f.IsInitOnly)).ToList();
			var fieldNames = string.Join("\n", disallowedFields.Select(f => $"{f.DeclaringType.Name}.{f.Name}"));
			var passing = disallowedFields.Count == 0;
			var isInBaseline = baseline.Contains(type);
			Assert(
$@"Transforms should not hold state as a new instance is created for every call.
Non-const fields detected:
{fieldNames}",
				passing || isInBaseline
			);
			Assert($"{type.FullName} is in the baseline but didn't fail the test, please remove it from the baseline", !(passing && isInBaseline));
		}

		public virtual string[] expectedIndex => null;

		[ExpectNoExceptions]
		public virtual void TestNewIndex()
		{
			if (transformationToTest is ITransformationIndexProvider transformationInstance)
			{
				var indexDefinitions = transformationInstance.IndexProvider.Select(index => index.Definition);

				AssertNotNull(expectedIndex);
				AssertContainsExactElementsInAnyOrder(expectedIndex, indexDefinitions);
				transformationInstance.IndexProvider.CreateIndexes(TestConnection);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var type = TransformationToTest.GetType();
			if (!transformationsWithWrongAuditStatementsBaseLine.Contains(type) && !transformationsWithWrongAuditStatementsBaseLine.Contains(type.BaseType))
			{
				enableAuditTriggersDisposable = TestConnection.EnableAllAuditTriggers();
			}
		}

		protected override void TearDown()
		{
			enableAuditTriggersDisposable?.Dispose();
			enableAuditTriggersDisposable = null;
			base.TearDown();
		}

		IDisposable enableAuditTriggersDisposable;

		protected static string EncloseInQuotesOrNullStringWhenNull(object o) => (o == null) ? "null" : $"'{o}'";
	}
}

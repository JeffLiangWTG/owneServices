using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class FilteredBusinessObjectReaderTest : TestCaseWithFactory
	{
		public void TestAllowTableValuedParameters_WhenWithinNormalLimitOfParameters()
		{
			CreateSomeDummiesAndEnsureTableValuedParametersAreUsedCorrectly(ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION);
		}

		public void TestAllowTableValuedParameters_WhenOutsideNormalLimitOfParameters()
		{
			CreateSomeDummiesAndEnsureTableValuedParametersAreUsedCorrectly(ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION + 1);
		}

		void CreateSomeDummiesAndEnsureTableValuedParametersAreUsedCorrectly(int nummies)
		{
			CreateSomeDummies(nummies);

			Factory.Save();

			using (TestConnection.TrackExecutedCommands())
			{
				var reader = new FilteredBusinessObjectReader<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.StartsWith, "in_filter"))
				{
					BatchSize = nummies,
					AllowTableValuedParameters = false,
				};

				AssertEquals(nummies, reader.Cast<DummyBusinessObject>().ToArray().Length);

				var executedCommand = TestConnection.ExecutedCommands.Single(c => c.Contains("TOP " + nummies));

				if (nummies > ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION)
				{
					AssertContains("Z0_PK in ('", executedCommand);
				}
				else
				{
					AssertContains("Z0_PK in (@", executedCommand);
				}
			}

			using (TestConnection.TrackExecutedCommands())
			{
				var reader = new FilteredBusinessObjectReader<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.StartsWith, "in_filter"))
				{
					BatchSize = nummies,
					AllowTableValuedParameters = true,
				};

				AssertEquals(nummies, reader.Cast<DummyBusinessObject>().ToArray().Length);

				var executedCommand = TestConnection.ExecutedCommands.Single(c => c.Contains("TOP " + nummies));

				AssertContains("Z0_PK in (SELECT Value FROM ", executedCommand);
			}
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestOrderByWithFunctionsNotSupported()
		{
			ZQuery filterWithOrderBy = new ZQuery();
			filterWithOrderBy.OrderBy = "cast(" + DummyBizoSchema.Z0_Code.Name + " as varchar(38)";
			ZQuery unusedFilter = new FilteredBusinessObjectReader(filterWithOrderBy, typeof(DummyBusinessObject)).ObjectFilter;
		}

		public void TestEnumerate()
		{
			CreateSomeDummies(30);

			var filter = new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.StartsWith, "in_filter");
			var reader = new FilteredBusinessObjectReader<DummyBusinessObject>(filter);

			var dummyNamesRead = new Hashtable();
			foreach (BusinessObject bizObj in reader)
			{
				var dummy = (DummyBusinessObject)bizObj;
				dummyNamesRead.Add(dummy.Z0_Description, null);
			}

			AssertEquals("All dummies should have been read", 30, dummyNamesRead.Count);
		}

		public void TestEnumerateWithNoInitialSet()
		{
			var filter = new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.StartsWith, "in_filter");
			filter.OrderBy = DummyBizoSchema.Z0_Description.Name;
			filter.AddOptionRecompileConditionally = true;

			var reader = new FilteredBusinessObjectReader<DummyBusinessObject>(filter);

			var objectsRead = new List<DummyBusinessObject>();
			foreach (DummyBusinessObject bizObj in reader)
			{
				objectsRead.Add(bizObj);
			}

			AssertEquals("No dummies should have been read", 0, objectsRead.Count);
		}

		public void TestLoadWithCacheSetsIgnoreActiveFilter()
		{
			var filter = new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.StartsWith, "in_filter");
			filter.OrderBy = DummyBizoSchema.Z0_Description.Name;
			filter.AddOptionRecompileConditionally = true;
			var dummy = Factory.NewWithValidTestData<DummyBusinessObjectWithActiveFilter>();
			dummy.IsActive = false;
			dummy.Z0_Description = "in_filter_Inactive";
			var dummyActive = Factory.NewWithValidTestData<DummyBusinessObjectWithActiveFilter>();
			dummyActive.IsActive = true;
			dummyActive.Z0_Description = "in_filter_Active";
			Factory.Save();

			var reader = new FilteredBusinessObjectReader<DummyBusinessObjectWithActiveFilter>(filter);
			reader.ObjectFilter.IgnoreActiveFilter = true;
			AssertEquals("Should load all rows", 2, reader.Count());

			reader.ObjectFilter.IgnoreActiveFilter = false;
			AssertEquals("Should load active rows only", 1, reader.Count());
		}

		public void TestEnumerateWithOrderBy()
		{
			var dummy4 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy4.Z0_Description = "in_filter3";
			var dummy3 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy3.Z0_Description = "in_filter2_Duplicate";
			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy2.Z0_Description = "in_filter2_Duplicate";
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Description = "in_filter1";

			Factory.Save();

			var filter = new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.StartsWith, "in_filter");
			filter.OrderBy = DummyBizoSchema.Z0_Description.Name;
			filter.AddOptionRecompileConditionally = true;

			var reader = new FilteredBusinessObjectReader<DummyBusinessObject>(filter);

			var objectsRead = new List<DummyBusinessObject>(4);
			foreach (DummyBusinessObject bizObj in reader)
			{
				objectsRead.Add(bizObj);
			}

			AssertEquals("4 dummies should have been read", 4, objectsRead.Count);
			AssertEquals("Dummies should be read in order", "in_filter1", objectsRead[0].Z0_Description);
			AssertEquals("Dummies should be read in order", "in_filter2_Duplicate", objectsRead[1].Z0_Description);
			AssertEquals("Dummies should be read in order", "in_filter2_Duplicate", objectsRead[2].Z0_Description);
			AssertEquals("Dummies should be read in order", "in_filter3", objectsRead[3].Z0_Description);
		}

		public void TestApproximateCount()
		{
			CreateSomeDummies(30);
			BusinessObjectFactoryProvider factoryProvider = new BusinessObjectFactoryProvider();

			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.StartsWith, "in_filter");
			FilteredBusinessObjectReader reader = new FilteredBusinessObjectReader(filter, typeof(DummyBusinessObject));

			AssertEquals(30, reader.ApproximateCount);
		}

		public void TestHasRecords()
		{
			DummyBusinessObject dummy = DummyBusinessObject.New(Factory);
			dummy.Z0_Code = "snowy";
			Factory.Save();

			ZQuery badFilter = new ZQuery(DummyBizoSchema.Z0_Code, "blah");
			FilteredBusinessObjectReader readerWithNoResults = new FilteredBusinessObjectReader(badFilter, typeof(DummyBusinessObject));
			AssertEquals(false, readerWithNoResults.HasRecords);

			ZQuery goodFilter = new ZQuery(DummyBizoSchema.Z0_Code, "snowy");
			FilteredBusinessObjectReader readerWithResults = new FilteredBusinessObjectReader(goodFilter, typeof(DummyBusinessObject));
			AssertEquals(true, readerWithResults.HasRecords);
		}

		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(DummyBusinessObject), new FilteredBusinessObjectReader(new ZQuery(), typeof(DummyBusinessObject)).BusinessObjectType);
		}

		public void TestNotSupportedExceptionIsThrownWhenObjectFilterIsNull()
		{
			try
			{
				FilteredBusinessObjectReader reader = new FilteredBusinessObjectReader(null, typeof(DummyBusinessObject));
				ZQuery unusedFilter = reader.ObjectFilter;
				Fail("Exception should be thrown here");
			}
			catch (NotSupportedException ex)
			{
				AssertEquals("Override this in the sub-class", ex.Message);
			}
		}

		public void TestDefaultObjectFilterIsUsedInSubClassWhenObjectFilterIsNull()
		{
			CreateSomeDummies(10);

			DummyBusinessObject notIncludedDummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			notIncludedDummy1.Z0_Description = "blah1";

			DummyBusinessObject notIncludedDummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			notIncludedDummy2.Z0_Description = "not_in_filter";

			FilteredDummyBusinessObjectReaderForTest reader = new FilteredDummyBusinessObjectReaderForTest();
			int dummyCounter = 0;
			foreach (BusinessObject bizObj in reader)
			{
				AssertNotNull("Type should be DummyBusinessObject", bizObj as DummyBusinessObject);
				dummyCounter++;
			}
			AssertEquals("There should be 10 dummies in the reader", 10, dummyCounter);
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "Cannot set the FilteredBusinessObjectReader.BatchSize to a value less than 1.")]
		public void TestBatchSizeThrowsExceptionWithValueLessThan1()
		{
			new FilteredBusinessObjectReader(new ZQuery(), typeof(DummyBusinessObject)).BatchSize = 0;
		}

		public void TestBatchSize()
		{
			CreateSomeDummies(7);

			FilteredBusinessObjectReader reader = new FilteredBusinessObjectReader(new ZQuery(), typeof(DummyBusinessObject));
			reader.BatchSize = 6;

			int dummyCounter = 0;
			reader.NumberOfRecordsInLastBatch_ForTest = 0;

			foreach (DummyBusinessObject dummy in reader)
			{
				object anObjectADayKeepsTheCompilerWarningAway = dummy;
				dummyCounter++;

				if (dummyCounter <= 6)
				{
					AssertEquals(6, reader.NumberOfRecordsInLastBatch_ForTest); // first batch
				}
				else
				{
					AssertEquals(1, reader.NumberOfRecordsInLastBatch_ForTest); // last batch
				}
			}

			AssertEquals("There should be 7 dummies in the reader.", 7, dummyCounter);
		}

		public void TestBlobs()
		{
			const int samplesCount = 10;
			string blobText = new string('A', 4096);
			string sampleDescription = "blob-test";

			for (int i = 0; i < samplesCount; i++)
			{
				var dummy = Factory.New<DummyBusinessObject>();
				dummy.Z0_Description = sampleDescription;
				dummy.Z0_NVarCharMax = blobText;
			}

			Factory.Save();

			var query = new ZQuery(DummyBizoSchema.Z0_Description, sampleDescription);
			FilteredBusinessObjectReader reader = new FilteredBusinessObjectReader<DummyBusinessObject>(query);

			using (AssertDbHitsForAllFactories(
				ignoreUnspecified: true,
				expectedHitCounts: new Dictionary<string, int> { { DummyBizoSchema.Constants.TableName, 1 + samplesCount } }
			))
			{
				foreach (DummyBusinessObject obj in reader)
				{
					AssertEquals(blobText, obj.Z0_NVarCharMax);
				}
			}

			query.IncludeBlob(DummyBizoSchema.Z0_NVarCharMax);
			reader = new FilteredBusinessObjectReader<DummyBusinessObject>(query);

			using (AssertDbHitsForAllFactories(
				ignoreUnspecified: true,
				expectedHitCounts: new Dictionary<string, int> { { DummyBizoSchema.Constants.TableName, 1 } }
			))
			{
				foreach (DummyBusinessObject obj in reader)
				{
					AssertEquals(blobText, obj.Z0_NVarCharMax);
				}
			}
		}

		public void TestPerformance()
		{
			const int samplesCount = 25000;
			const int batchSize = 100;
			const string sampleDescription = "performance test";

			TestConnection.ExecuteNonQuery($@"
				with
					t0(i) AS (SELECT 0 UNION ALL SELECT 0),
					t1(i) AS (SELECT 0 FROM t0 a, t0 b),
					t2(i) AS (SELECT 0 FROM t1 a, t1 b),
					t3(i) AS (SELECT 0 FROM t2 a, t2 b),
					t4(i) AS (SELECT 0 FROM t3 a, t3 b),
					t5(i) AS (SELECT 0 FROM t4 a, t4 b),
					tt(i) AS (select TOP {samplesCount} 0 from t5),
					NumberRange(i) AS (select cast(row_number() over (order by i) as int) from tt)
				insert into {DummyBusinessObject.Schema.TableName}
				(
					{DummyBizoSchema.PK.Name},
					{DummyBizoSchema.Z0_Description.Name},
					{DummyBizoSchema.Z0_Number.Name}
				)
				select newid(), '{sampleDescription}', i from NumberRange
			");

			var query = new ZQuery(DummyBizoSchema.Z0_Description, sampleDescription);
			query.OrderBy = DummyBizoSchema.Z0_Number.Name;

			Stopwatch sw = Stopwatch.StartNew();

			FilteredBusinessObjectReader reader = new FilteredBusinessObjectReader(query, typeof(DummyBusinessObject));
			reader.BatchSize = batchSize;

			var pkSet = new HashSet<ZGuid>();
			int lastNumber = 0;
			foreach (DummyBusinessObject dummy in reader)
			{
				if (!pkSet.Add(dummy.PK))
				{
					Fail("duplicate PK returned");
				}

				if (dummy.Z0_Number <= lastNumber)
				{
					Fail($"invalid order {lastNumber} before {dummy.Z0_Number}");
				}

				lastNumber = dummy.Z0_Number;
			}

			AssertEquals(samplesCount, pkSet.Count);
			AssertLessThanOrEqualTo("time limit exceeded", sw.ElapsedMilliseconds, 60_000);
		}

		#region Implementation

		void CreateSomeDummies(int count)
		{
			for (int i = 0; i < count; i++)
			{
				DummyBusinessObject dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
				dummy.Z0_Description = "in_filter" + i;
			}
			Factory.Save();
		}

		#region FilteredBusinessObjectReaderForTest

		class FilteredDummyBusinessObjectReaderForTest : FilteredBusinessObjectReader
		{
			public FilteredDummyBusinessObjectReaderForTest()
				: base(typeof(DummyBusinessObject))
			{
			}

			public FilteredDummyBusinessObjectReaderForTest(BusinessObjectFactoryProvider factoryProvider)
				: base(factoryProvider, typeof(DummyBusinessObject))
			{
			}

			protected override ZQuery GetDefaultObjectFilter()
			{
				return new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.StartsWith, "in_filter");
			}
		}

		#endregion // FilteredBusinessObjectReaderForTest

		#endregion // Implementation
	}
}

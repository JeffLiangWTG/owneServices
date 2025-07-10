using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformations;
using Enterprise.DbUpgrader.Transformations.Transforms;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Test
{
	public class TransformationDirectorTest : TransactionedTestCase
	{
		internal class MyDirector : TransformationDirector
		{
			public MyDirector(IUpgradeManager manager)
				: base(manager)
			{
			}

			protected override DataTransformation[] Transformations
			{
				get
				{
					return new DataTransformation[] {
						new DataTransformationForTesting_01(),
						new DataTransformationForTesting_02(),
					};
				}
			}

			public DataTransformation[] TransformationsExposedForTest => Transformations;
		}

		[ExpectNoExceptions]
		public void TestRunningTransforms()
		{
			var director = new MyDirector(new DummyUpgradeManager());
			director.OfflinePostUpgradeRun();
		}

		[ExpectNoExceptions]
		public void TestRunningTransformsReportsAll()
		{
			var director = new MyDirector(new DummyUpgradeManager());
			SlowTransformReporter.ResetLazyRegistry();
			DbRegistry.ReportSlowUpgradeTransformsSeconds.SaveValue(0, Db.Connection);

			director.OfflinePostUpgradeRun();

			AssertEquals(director.TransformationsExposedForTest.Length, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestNoDataTransformationMappedVersionIsGreaterThanCurrentTransformationVersion()
		{
			var allMappings = Mapper.GetAllMappings();

			var aheadMappedTransformations = GetTransformationsMappedToVersionGreaterThanCurrentTransformationVersion(allMappings);

			if (aheadMappedTransformations.Count > 0)
			{
				Fail(GetAheadMappedTransformationsFailMessage(aheadMappedTransformations));
			}

			AssertEquals("There should be NO data transformations mapped ahead of TransformationVersion.ApplicationNumber", 0, aheadMappedTransformations.Count);
		}

		string GetAheadMappedTransformationsFailMessage(List<Mapping> aheadMappedTransformations)
		{
			var failMessage = new StringBuilder(
				string.Format("The following Data transformations are mapped to a version greater than the current ({0}):\r\n",
				TransformationVersion.ApplicationNumber.ToString()));

			foreach (var aheadMappedTransformation in aheadMappedTransformations)
			{
				failMessage.Append(string.Format("{0} - {1}\r\n",
					aheadMappedTransformation.MappedVersion.ToString().PadRight(2),
					aheadMappedTransformation.TransformationType.FullName));
			}

			failMessage.Append("\r\n\r\n");
			failMessage.Append("  ** ** ** ** ** ** ** ** ** **    A T T E N T I O N    ** ** ** ** ** ** ** ** ** **\r\n\r\n");
			failMessage.Append("  To fix this test, bump the TransformationVersion.ApplicationNumber to be the same as the mapped transfomation.\r\n\r\n");
			failMessage.Append("\r\n\r\n");

			return failMessage.ToString();
		}

		List<Mapping> GetTransformationsMappedToVersionGreaterThanCurrentTransformationVersion(Mapping[] allMappedTransformations)
		{
			var result = new List<Mapping>();

			for (var i = 0; i < allMappedTransformations.Length; i++)
			{
				if (allMappedTransformations[i].MappedVersion.CompareTo(TransformationVersion.ApplicationNumber) > 0)
				{
					result.Add(allMappedTransformations[i]);
				}
			}

			return result;
		}

		#region Test DataTransformation Classes

		class DataTransformationForTesting_01 : DataTransformation
		{
			public override string UserDescription => "Dummy Data Transformation 1";
		}

		class DataTransformationForTesting_02 : DataTransformation
		{
			public override string UserDescription => "Dummy Data Transformation 2";
		}

		#endregion

		public void TestRequiredPreUpgradeTransformations()
		{
			var testDirector = new TransformationDirectorForTesting();
			var testTransformations = testDirector.Transformations_Exposed;
			Assert("There should be Pre-Upgrade transformations mapped", testTransformations.Length > 0);
		}

		public void TestGetRequiredSchemaChangeDataTransformations()
		{
			var testDirector = new TransformationDirectorForTesting();
			var testTransformations = testDirector.Transformations_Exposed;
			Assert("There should be Data-Copy transformations mapped", testTransformations.Length > 0);
		}

		public void TestCreateDataCopyDatabase()
		{
			var testDirector = new TransformationDirectorForTesting();

			try
			{
				testDirector.CreateDataCopyDb_Exposed();

				string slqText = string.Format("SELECT count(*) FROM sys.databases WHERE name = '{0}'", TransformationDirector.DataCopyDb);
				int dbCount = Convert.ToInt32(Db.Connection.ExecuteScalar(slqText));
				AssertEquals("DataCopy DB should have been created", 1, dbCount);
			}
			finally
			{
				using (var conn = Db.NewAdminConnection())
				{
					testDirector.DropDataCopyDb(conn);
				}
			}
		}
	}
}

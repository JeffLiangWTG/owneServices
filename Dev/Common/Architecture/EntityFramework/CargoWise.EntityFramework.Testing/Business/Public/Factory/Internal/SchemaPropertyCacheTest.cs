using System.Data;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class SchemaPropertyCacheTest : TestCase
	{
		#region Test Helper Classes

		class DummyBizOForTest : DummyBusinessObject
		{
			public DummyBizOForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new abstract class Schema
			{
				public const string TableName = null;
				public const string PK = "DummyBizOForTestPK";
			}
		}

		#endregion

		public void TestSchemaInfo()
		{
			SchemaInfo testSchemaInfo = new SchemaInfo(typeof(DummyBusinessObject));

			string tableName = testSchemaInfo["TableName"];
			AssertEquals("Value of TableName property", DummyBusinessObject.Schema.TableName, tableName);

			string pK = testSchemaInfo["PK"];
			AssertEquals("Value of PK property", DummyBusinessObject.Schema.PK, pK);

			tableName = testSchemaInfo["TableName"];
			pK = testSchemaInfo["PK"];

			AssertEquals("Value of TableName property", DummyBusinessObject.Schema.TableName, tableName);
			AssertEquals("Value of PK property", DummyBusinessObject.Schema.PK, pK);
			AssertEquals(2, testSchemaInfo.TimesOfRunGetValue);
		}

		public void TestBizOTypeHash()
		{
			BizOTypeHash testHash = new BizOTypeHash();
			SchemaInfo dummyInfo = testHash[typeof(DummyBusinessObject)];
			SchemaInfo testDummyInfo = testHash[typeof(DummyBizOForTest)];
			Assert("SchemaInfos for different BizOTypes should be different objects", dummyInfo != testDummyInfo);

			SchemaInfo testDummyInfo2 = testHash[typeof(DummyBizOForTest)];
			AssertEquals("SchemaInfos for same BizOTypes should be the same object", testDummyInfo, testDummyInfo2);

			// Performance test. Without caching, this code takes ~3.1s to run, with caching it takes ~0.5s
			ZDateTime startTime = ZDateTime.Now;
			for (int i = 0; i < 1000000; i++)
			{
				dummyInfo = testHash[typeof(DummyBusinessObject)];
				testDummyInfo = testHash[typeof(DummyBizOForTest)];
			}
			ZDateTime finishTime = ZDateTime.Now;

			double duration = (finishTime - startTime).TotalMilliseconds;
			Assert("Performance test duration should take less than 800ms, but was " + duration + "ms", duration < 800);
		}
	}
}

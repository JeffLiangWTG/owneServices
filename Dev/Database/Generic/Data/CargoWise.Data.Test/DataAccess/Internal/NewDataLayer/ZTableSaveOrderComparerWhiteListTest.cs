using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	class ZTableSaveOrderComparerWhiteListTest : TestCase
	{
		public void TestCompare()
		{
			AssertRegistered(AccExchangeRateConfigurationViewSchema.Constants.TableName, AccJobConfigPivotSchema.Constants.TableName);
			AssertRegistered(JobSupplierBookingLineSchema.Constants.TableName, JobPackLinesSchema.Constants.TableName);

			void AssertRegistered(string precedentTableName, string subsequentTableName)
			{
				AssertEquals(-1, ZTableSaveOrderComparerWhiteList.Compare(precedentTableName, subsequentTableName));
				AssertEquals(1, ZTableSaveOrderComparerWhiteList.Compare(subsequentTableName, precedentTableName));

				AssertEquals(0, ZTableSaveOrderComparerWhiteList.Compare(subsequentTableName, "NonRelevantTestTable"));
				AssertEquals(0, ZTableSaveOrderComparerWhiteList.Compare(precedentTableName, "NonRelevantTestTable"));
			}
		}
	}
}

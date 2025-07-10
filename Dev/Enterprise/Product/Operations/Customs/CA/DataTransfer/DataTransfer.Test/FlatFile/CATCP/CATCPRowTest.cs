using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.DataTransfer.Testing
{
	[TestsSubclassesOf(typeof(CATCPRow))]
	abstract class CATCPRowTest : TestCaseWithFactory
	{
		public void TestDataAreTruncated()
		{
			var row = GetFullyPopulatedDataRow();
			var firstData = row[0].PadRight(row.GetFieldLength(0));
			row[0] = firstData + "12".PadRight(10, 'A');
			AssertEquals(firstData, row[0]);
		}

		public void TestToString()
		{
			AssertEquals("Data", ExpectedData.PadRight(CATCPRow.DataMaxLength), GetFullyPopulatedDataRow().ToString());
		}

		protected abstract CATCPRow GetFullyPopulatedDataRow();

		protected abstract string ExpectedData { get; }
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.PTRS;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.PTRS
{
	[TestedType(typeof(PtrsReportDateColumn))]
	public class PtrsReportDateColumnTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValueProperty()
		{
			var column = GetNewBusinessObject() as PtrsReportDateColumn;
			AssertNotNull(column);
			Assert(column.Value.IsEmpty);

			column.Value = new ZDateTime(2021, 7, 28, 23, 43, 5);
			AssertEquals(new ZDateTime(2021, 7, 28), column.Value);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var column = Factory.NewWithValidTestData<AccTaxReturnColumn>();
			return new PtrsReportDateColumn(column);
		}
	}
}

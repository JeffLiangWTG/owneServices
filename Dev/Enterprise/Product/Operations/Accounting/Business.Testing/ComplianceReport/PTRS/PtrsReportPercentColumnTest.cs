using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.PTRS;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.PTRS
{
	[TestedType(typeof(PtrsReportPercentColumn))]
	public class PtrsReportPercentColumnTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValueProperty()
		{
			var column = GetNewBusinessObject() as PtrsReportPercentColumn;
			AssertNotNull(column);
			Assert(column.Value.IsEmpty);

			column.Value = 100.19m;
			AssertEquals(100.2m, column.Value);
		}

		public void TestValueDecimalPlaces()
		{
			var column = GetNewBusinessObject() as PtrsReportPercentColumn;
			AssertNotNull(column);
			var withDecimalPlaces = column as IHaveDecimalPlaces;
			AssertNotNull("As IHaveMaxLength", withDecimalPlaces);

			AssertEquals("Default Decimal Places for Percent Column", 1, withDecimalPlaces.DecimalPlaces);
			column.Value = 11.16m;
			AssertEquals("Rounded Value", 11.2m, column.Value);

			withDecimalPlaces.DecimalPlaces = 2;
			AssertEquals("Set DecimalPlaces", 2, withDecimalPlaces.DecimalPlaces);

			column.Value = 11.194m;
			AssertEquals("Rounded Value", 11.19m, column.Value);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var column = Factory.NewWithValidTestData<AccTaxReturnColumn>();
			return new PtrsReportPercentColumn(column);
		}
	}
}

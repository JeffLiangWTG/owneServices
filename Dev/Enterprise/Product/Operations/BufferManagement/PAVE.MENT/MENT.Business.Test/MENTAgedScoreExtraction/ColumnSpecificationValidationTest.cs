using CargoWise.EntityFramework.Testing;

namespace Enterprise.PAVE.MENT.Business.Test
{
	class ColumnSpecificationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSequenceChangedButNotSelected()
		{
			var column = new SQLColumnSpecification(Factory);

			column.Sequence = 10;
			AssertHasWarning(column.SequenceInfo, "This column is currently not included in the series");

			column.Selected = true;
			AssertNoWarnings(column.SequenceInfo);
		}
	}
}

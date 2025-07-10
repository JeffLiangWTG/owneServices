using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.PTRS;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.PTRS
{
	[TestedType(typeof(PtrsReportRegNumberColumn))]
	public class PtrsReportRegNumberColumnTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValueProperty()
		{
			var column = GetNewBusinessObject() as PtrsReportRegNumberColumn;
			AssertNotNull(column);
			Assert(column.Value.IsEmpty);

			column.Value = "0123456789";
			AssertEquals("0123456789", column.Value);

			column.Value = "0123456789 ";
			AssertEquals("Trims spaces at the end", "0123456789", column.Value);
		}

		public void TestValueMaxLength()
		{
			var column = GetNewBusinessObject() as PtrsReportRegNumberColumn;
			AssertNotNull(column);
			var withMaxLength = column as IHaveMaxLength;
			AssertNotNull("As IHaveMaxLength", withMaxLength);

			Assert("SupportsMaxLength", column.ValueInfo.SupportsMaxLength);
			AssertEquals("Default MaxLength", PtrsReportRegNumberColumn.ABNLength, column.ValueInfo.MaxLength);

			withMaxLength.MaxLength = AccTaxReturnColumn.Schema.ATC_CommentMaxLength + 1;
			AssertEquals("Actual MaxLength cannot be exceeded", AccTaxReturnColumn.Schema.ATC_CommentMaxLength, column.ValueInfo.MaxLength);

			withMaxLength.MaxLength = PtrsReportRegNumberColumn.BICLength;
			AssertEquals("Set MaxLength", PtrsReportRegNumberColumn.BICLength, column.ValueInfo.MaxLength);

			withMaxLength.MaxLength = 10;
			var expectedError = "The maximum length of 'Value' has been exceeded. The maximum length of this property is 10 characters, but 11 were entered.";
			column.Value = "01234567890";
			AssertHasError(column.ValueInfo, expectedError);
		}

		public void TestNumbersOnlyValidation()
		{
			var column = GetNewBusinessObject() as PtrsReportRegNumberColumn;
			AssertNotNull(column);
			Assert(column.Value.IsEmpty);
			AssertNoErrors(column.ValueInfo);

			column.Value = "01234567890";
			AssertNoErrors(column.ValueInfo);

			var expectedError = "Should contain numbers only.";
			column.Value = " 1234567890";
			AssertHasError(column.ValueInfo, expectedError);

			column.Value = "";
			AssertNoErrors(column.ValueInfo);
			column.Value = "a1234567890";
			AssertHasError(column.ValueInfo, expectedError);

			column.Value = "";
			AssertNoErrors(column.ValueInfo);
			column.Value = "0123456789~";
			AssertHasError(column.ValueInfo, expectedError);

			column.Value = "";
			AssertNoErrors(column.ValueInfo);
			column.Value = "012345|6789";
			AssertHasError(column.ValueInfo, expectedError);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var column = Factory.NewWithValidTestData<AccTaxReturnColumn>();
			return new PtrsReportRegNumberColumn(column);
		}
	}
}

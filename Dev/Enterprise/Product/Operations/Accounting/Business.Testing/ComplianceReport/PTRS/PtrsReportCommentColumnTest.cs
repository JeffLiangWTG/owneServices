using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.PTRS;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.PTRS
{
	[TestedType(typeof(PtrsReportCommentColumn))]
	public class PtrsReportCommentColumnTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValueProperty()
		{
			var column = GetNewBusinessObject() as PtrsReportCommentColumn;
			AssertNotNull(column);
			Assert(column.Value.IsEmpty);

			column.Value = "Some comment";
			AssertEquals("Some comment", column.Value);
		}

		public void TestValueMaxLength()
		{
			var column = GetNewBusinessObject() as PtrsReportCommentColumn;
			AssertNotNull(column);
			var withMaxLength = column as IHaveMaxLength;
			AssertNotNull("As IHaveMaxLength", withMaxLength);

			Assert("SupportsMaxLength", column.ValueInfo.SupportsMaxLength);
			AssertEquals("Default MaxLength", AccTaxReturnColumn.Schema.ATC_CommentMaxLength, column.ValueInfo.MaxLength);

			withMaxLength.MaxLength = AccTaxReturnColumn.Schema.ATC_CommentMaxLength + 1;
			AssertEquals("Actual MaxLength cannot be exceeded", AccTaxReturnColumn.Schema.ATC_CommentMaxLength, column.ValueInfo.MaxLength);

			withMaxLength.MaxLength = 400;
			AssertEquals("Set MaxLength", 400, column.ValueInfo.MaxLength);

			withMaxLength.MaxLength = 10;
			var expectedError = "The maximum length of 'Value' has been exceeded. The maximum length of this property is 10 characters, but 11 were entered.";
			column.Value = "01234567890";
			AssertHasError(column.ValueInfo, expectedError);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var column = Factory.NewWithValidTestData<AccTaxReturnColumn>();
			return new PtrsReportCommentColumn(column);
		}
	}
}

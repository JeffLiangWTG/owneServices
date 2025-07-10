using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.PTRS;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.PTRS
{
	[TestedType(typeof(PtrsReportEmailColumn))]
	public class PtrsReportEmailColumnTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValueProperty()
		{
			var column = GetNewBusinessObject() as PtrsReportEmailColumn;
			AssertNotNull(column);
			Assert(column.Value.IsEmpty);

			column.Value = "mail@server.com.au";
			AssertEquals("mail@server.com.au", column.Value);

			column.Value = "my.mail@server.com.au ";
			AssertEquals("Trims spaces at the end", "my.mail@server.com.au", column.Value);
		}

		public void TestValueMaxLength()
		{
			var column = GetNewBusinessObject() as PtrsReportEmailColumn;
			AssertNotNull(column);
			var withMaxLength = column as IHaveMaxLength;
			AssertNotNull("As IHaveMaxLength", withMaxLength);

			Assert("SupportsMaxLength", column.ValueInfo.SupportsMaxLength);
			AssertEquals("Default MaxLength", 50, column.ValueInfo.MaxLength);

			withMaxLength.MaxLength = AccTaxReturnColumn.Schema.ATC_CommentMaxLength + 1;
			AssertEquals("Actual MaxLength cannot be exceeded", AccTaxReturnColumn.Schema.ATC_CommentMaxLength, column.ValueInfo.MaxLength);

			withMaxLength.MaxLength = PtrsReportRegNumberColumn.BICLength;
			AssertEquals("Set MaxLength", PtrsReportRegNumberColumn.BICLength, column.ValueInfo.MaxLength);

			withMaxLength.MaxLength = 10;
			var expectedError = "The maximum length of 'Value' has been exceeded. The maximum length of this property is 10 characters, but 11 were entered.";
			column.Value = "01234567890";
			AssertHasError(column.ValueInfo, expectedError);
		}

		public void TestEmailValidation()
		{
			var column = GetNewBusinessObject() as PtrsReportEmailColumn;
			AssertNotNull(column);
			Assert(column.Value.IsEmpty);
			AssertNoErrors(column.ValueInfo);

			column.Value = "my_mail@server.com.au";
			AssertNoErrors(column.ValueInfo);

			var expectedError = "Email Address is not valid .";
			column.Value = "my_mail~server.com.au";
			AssertHasError(column.ValueInfo, expectedError);

			column.Value = "";
			AssertNoErrors(column.ValueInfo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var column = Factory.NewWithValidTestData<AccTaxReturnColumn>();
			return new PtrsReportEmailColumn(column);
		}
	}
}

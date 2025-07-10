using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_TakeUpALLSubLedgers))]
	class BatchAggregator_Validation_TransactionPeriodTest : BatchAggregatorValidationTest
	{
		public override void TestAggregationValidation()
		{
			PrepareForAggregation(insertPeriods: false);

			var transactionDate = new DateTime(2005, 5, 25);
			TestHelper.InsertTransactionHeader("AP", "JNL", invoiceAmount: 31, transactionDate: transactionDate, transactionNumber: "JNL123");
			TestHelper.InsertTransactionLine("WIP", postDate: transactionDate, reverseDate: transactionDate);

			AssertPeriodValidation("No accounting periods are specified for the current company. Please set up the accounting periods.");

			InsertPeriod(200502, new DateTime(2005, 2, 1, 0, 0, 0), new DateTime(2005, 2, 28, 23, 59, 0), new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC"));
			InsertPeriod(200504, new DateTime(2005, 4, 1, 0, 0, 0), new DateTime(2005, 4, 30, 23, 59, 0), new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC"));

			AssertPeriodValidation(@"The following accounting period has a gap/overlap from the previous period. Please fix the accounting periods.
200504");

			InsertPeriod(200503, new DateTime(2005, 3, 1, 0, 0, 0), new DateTime(2005, 3, 31, 23, 59, 0), new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC"));

			AssertPeriodValidation(@"The following transactions have post or reverse dates for which accounting periods do not exist. Please create the appropriate periods.
AP	JNL	JNL123	May 25 2005
AR	WIP		May 25 2005");

			InsertPeriod(200505, new DateTime(2005, 5, 1, 0, 0, 0), new DateTime(2005, 5, 31, 23, 58, 0), new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC"));

			var expected = new[]
			{
				(-31m, "", 200505, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(31m, "", 200505, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(+250m, "", 200505, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(-250m, "", 200505, "SYD", "FIR", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT"),
				(-250m, "", 200505, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+250m, "", 200505, "SYD", "FIR", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT"),
			};
			RunAndAssertAggregation(expected);

			InsertPeriod(200506, new DateTime(2005, 6, 1, 0, 0, 0), new DateTime(2005, 7, 1, 0, 0, 0), new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC"));

			AssertPeriodValidation(@"The following accounting period has a gap/overlap from the previous period. Please fix the accounting periods.
200506");

			InsertPeriod(200507, new DateTime(2005, 7, 1, 0, 0, 0), new DateTime(2005, 7, 31, 23, 59, 0), new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC"));

			AssertPeriodValidation(@"The following accounting period has a gap/overlap from the previous period. Please fix the accounting periods.
200506, 200507");
		}

		public void TestPeriodValidationForAllTransactionTypes()
		{
			AssertPeriodValidationForAllTransactionTypes(false);
		}

		public void TestPeriodValidationForAllTransactionTypes_WhenDateIsEmpty()
		{
			AssertPeriodValidationForAllTransactionTypes(true);
		}

		void AssertPeriodValidationForAllTransactionTypes(bool isDateEmpty)
		{
			PrepareForAggregation();

			var taxConfigurationPK = DbHelper.InsertTaxConfiguration("ADT", TestDbHelper.DefaultCompanyPK);
			var taxIdPK = DbHelper.InsertTaxRate("TAX1");

			var defaultDateWithoutPeriods = new DateTime(2006, 12, 25);
			var transactionDate = isDateEmpty ? DateTime.MinValue : defaultDateWithoutPeriods;

			TestHelper.InsertTransactionHeader("AP", "CTR", transactionDate: transactionDate, transactionNumber: "01");
			TestHelper.InsertTransactionHeader("AR", "CTR", transactionDate: transactionDate, transactionNumber: "02");

			TestHelper.InsertTransactionHeader("AP", "JNL", transactionDate: transactionDate, transactionNumber: "03");
			TestHelper.InsertTransactionHeader("AR", "JNL", transactionDate: transactionDate, transactionNumber: "04");

			TestHelper.InsertTransactionHeader("CB", "TRF", transactionDate: transactionDate, transactionNumber: "05");
			TestHelper.InsertTransactionHeader("CB", "EXX", transactionDate: transactionDate, transactionNumber: "06");

			var headerPK = TestHelper.InsertTransactionHeader("CB", "DPY", transactionDate: transactionDate, transactionNumber: "07");
			TestHelper.InsertTransactionLine("DPY", headerPK, postDate: transactionDate, reverseDate: transactionDate);
			headerPK = TestHelper.InsertTransactionHeader("CB", "DRC", transactionDate: transactionDate, transactionNumber: "08");
			TestHelper.InsertTransactionLine("DRC", headerPK, postDate: transactionDate, reverseDate: transactionDate);

			TestHelper.InsertTransactionHeader("AP", "EXX", transactionDate: transactionDate, transactionNumber: "09");
			TestHelper.InsertTransactionHeader("AP", "OVP", transactionDate: transactionDate, transactionNumber: "10");
			TestHelper.InsertTransactionHeader("AP", "DSC", transactionDate: transactionDate, transactionNumber: "11");
			TestHelper.InsertTransactionHeader("AR", "EXX", transactionDate: transactionDate, transactionNumber: "12");
			TestHelper.InsertTransactionHeader("AR", "OVP", transactionDate: transactionDate, transactionNumber: "13");
			TestHelper.InsertTransactionHeader("AR", "DSC", transactionDate: transactionDate, transactionNumber: "14");

			headerPK = TestHelper.InsertTransactionHeader("AP", "INV", transactionDate: transactionDate, transactionNumber: "15");
			var linePK = TestHelper.InsertTransactionLine("CST", headerPK, postDate: transactionDate, reverseDate: transactionDate);
			DbHelper.InsertCashBasisVAT(linePK, defaultDateWithoutPeriods, taxAmount: 6); //is not validated
			var taxTransactionPK = DbHelper.InsertTaxTransaction(headerPK, TestDbHelper.DefaultCompanyPK, TestHelper.Branch.PK, TestHelper.Department.PK, taxConfigurationPK, taxIdPK);
			DbHelper.InsertTaxGLMovement(taxTransactionPK, TestHelper.HeaderGLAccount.PK, TestHelper.HeaderGLAccount2.PK, period: 200612, date: defaultDateWithoutPeriods); //is not validated

			headerPK = TestHelper.InsertTransactionHeader("AP", "CRD", transactionDate: transactionDate, transactionNumber: "16");
			TestHelper.InsertTransactionLine("CST", headerPK, postDate: transactionDate, reverseDate: transactionDate);
			headerPK = TestHelper.InsertTransactionHeader("AP", "ADJ", transactionDate: transactionDate, transactionNumber: "17");
			TestHelper.InsertTransactionLine("CST", headerPK, postDate: transactionDate, reverseDate: transactionDate);
			headerPK = TestHelper.InsertTransactionHeader("AR", "INV", transactionDate: transactionDate, transactionNumber: "18");
			TestHelper.InsertTransactionLine("REV", headerPK, postDate: transactionDate, reverseDate: transactionDate);
			headerPK = TestHelper.InsertTransactionHeader("AR", "CRD", transactionDate: transactionDate, transactionNumber: "19");
			TestHelper.InsertTransactionLine("REV", headerPK, postDate: transactionDate, reverseDate: transactionDate);
			headerPK = TestHelper.InsertTransactionHeader("AR", "ADJ", transactionDate: transactionDate, transactionNumber: "20");
			TestHelper.InsertTransactionLine("REV", headerPK, postDate: transactionDate, reverseDate: transactionDate);

			headerPK = TestHelper.InsertTransactionHeader("JC", "JNL", transactionDate: transactionDate, transactionNumber: "21");
			TestHelper.InsertTransactionLine("REV", headerPK, postDate: transactionDate, reverseDate: transactionDate);

			headerPK = TestHelper.InsertTransactionHeader("JC", "JRJ", transactionDate: transactionDate, transactionNumber: "22");
			TestHelper.InsertTransactionLine("REV", headerPK, postDate: transactionDate, reverseDate: transactionDate);
			TestHelper.InsertTransactionLine("CST", headerPK, postDate: transactionDate, reverseDate: transactionDate);

			TestHelper.InsertTransactionHeader("AR", "REC", transactionDate: transactionDate, transactionNumber: "23");
			TestHelper.InsertTransactionHeader("AP", "REC", transactionDate: transactionDate, transactionNumber: "24");
			TestHelper.InsertTransactionHeader("AR", "PAY", transactionDate: transactionDate, transactionNumber: "25");
			TestHelper.InsertTransactionHeader("AP", "PAY", transactionDate: transactionDate, transactionNumber: "26");

			TestHelper.InsertTransactionLine("WIP", postDate: transactionDate, reverseDate: transactionDate);
			TestHelper.InsertTransactionLine("ACR", postDate: transactionDate, reverseDate: transactionDate);

			var dateInErrorMessage = isDateEmpty ? "Empty Date" : "Dec 25 2006";
			AssertPeriodValidation($@"The following transactions have post or reverse dates for which accounting periods do not exist. Please create the appropriate periods.
AP	ACR		{dateInErrorMessage}
AP	ADJ	17	{dateInErrorMessage}
AP	CRD	16	{dateInErrorMessage}
AP	CTR	01	{dateInErrorMessage}
AP	DSC	11	{dateInErrorMessage}
AP	EXX	09	{dateInErrorMessage}
AP	INV	15	{dateInErrorMessage}
AP	JNL	03	{dateInErrorMessage}
AP	OVP	10	{dateInErrorMessage}
AP	PAY	26	{dateInErrorMessage}
AP	REC	24	{dateInErrorMessage}
AR	ADJ	20	{dateInErrorMessage}
AR	CRD	19	{dateInErrorMessage}
AR	CTR	02	{dateInErrorMessage}
AR	DSC	14	{dateInErrorMessage}
AR	EXX	12	{dateInErrorMessage}
AR	INV	18	{dateInErrorMessage}
AR	JNL	04	{dateInErrorMessage}
AR	OVP	13	{dateInErrorMessage}
AR	PAY	25	{dateInErrorMessage}
AR	REC	23	{dateInErrorMessage}
AR	WIP		{dateInErrorMessage}
CB	DPY	07	{dateInErrorMessage}
CB	DRC	08	{dateInErrorMessage}
CB	EXX	06	{dateInErrorMessage}
CB	TRF	05	{dateInErrorMessage}
JC	JNL	21	{dateInErrorMessage}
JC	JRJ	22	{dateInErrorMessage}");
		}

		void AssertPeriodValidation(string expectedExceptionMessage = "")
		{
			var exceptionMessage = RunAndAssertAggregationWithError();
			AssertMultilineASCIIEquals("Exception Message", expectedExceptionMessage + "\r", exceptionMessage);
		}

		static void InsertPeriod(int period, DateTime fromDate, DateTime toDate, Guid company)
		{
			var insertPeriodCommand = Db.Connection.Command(@"
					INSERT INTO 
						dbo.AccPeriodManagement(
							AM_PK,
							AM_Period, 
							AM_StartDate, 
							AM_EndDate, 
							AM_GC_Company,
							AM_SystemCreateTimeUtc,
							AM_SystemCreateUser,
							AM_SystemLastEditTimeUtc,
							AM_SystemLastEditUser) 
					VALUES (
							NEWID(), 
							@AM_Period,
							@AM_StartDate,
							@AM_EndDate,
							@AM_GC_Company,
							GetUtcDate(),
							'~BP',
							GetUtcDate(),
							'~BP')");
			insertPeriodCommand.AddParameterBasedOnDbColumn("@AM_Period", period, AccPeriodManagementSchema.AM_Period);
			insertPeriodCommand.AddParameterBasedOnDbColumn("@AM_StartDate", fromDate, AccPeriodManagementSchema.AM_StartDate);
			insertPeriodCommand.AddParameterBasedOnDbColumn("@AM_EndDate", toDate, AccPeriodManagementSchema.AM_EndDate);
			insertPeriodCommand.AddParameterBasedOnDbColumn("@AM_GC_Company", company, AccPeriodManagementSchema.AM_GC_Company);
			insertPeriodCommand.ExecuteNonQuery();
		}
	}
}


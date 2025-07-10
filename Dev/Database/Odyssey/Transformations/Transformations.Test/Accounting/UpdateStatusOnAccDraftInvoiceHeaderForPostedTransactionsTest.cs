using System;
using System.Data;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Accounting;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Accounting
{
	[TestedType(typeof(UpdateStatusOnAccDraftInvoiceHeaderForPostedTransactions))]
	public class UpdateStatusOnAccDraftInvoiceHeaderForPostedTransactionsTest : DataTransformationTestCase
	{
		protected override void AssertPreConditions()
		{
			GetAccDraftInvoiceHeader();
			AssertEquals("PreCondition", "DFT", accDraftInvoiceHeader["AIH_Status"]);
			AssertEquals("PreCondition", "DFT", nonPostedAccDraftInvoiceHeader["AIH_Status"]);
		}

		protected override void AssertTransformationResults()
		{
			GetAccDraftInvoiceHeader();
			AssertEquals("AFP", accDraftInvoiceHeader["AIH_Status"]);
			AssertEquals("DFT", nonPostedAccDraftInvoiceHeader["AIH_Status"]);
		}

		public void GetAccDraftInvoiceHeader()
		{
			accDraftInvoiceHeader = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM AccDraftInvoiceHeader WHERE AIH_PK = '{accDraftInvoiceHeaderPK}'").Rows[0];
			nonPostedAccDraftInvoiceHeader = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM AccDraftInvoiceHeader WHERE AIH_PK = '{nonPostedAccDraftInvoiceHeaderPK}'").Rows[0];
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateStatusOnAccDraftInvoiceHeaderForPostedTransactions();
		}

		protected override void PrepareTestData()
		{
			var helper = new TestDbHelper(TestConnection);
			var company = TestDbHelper.DefaultCompanyPK;
			var branch = helper.InsertBranch("ZZB", company);
			var department = helper.InsertDepartment("ZZD");
			var transactionHeader = helper.InsertTransactionHeader("AP", "INV", "1000", 1000m, DateTime.Now);
			accDraftInvoiceHeaderPK = helper.InsertAccDraftInvoiceHeader(company, branch, department, transactionHeader);
			nonPostedAccDraftInvoiceHeaderPK = helper.InsertAccDraftInvoiceHeader(company, branch, department, null);
		}
		Guid accDraftInvoiceHeaderPK;
		Guid nonPostedAccDraftInvoiceHeaderPK;
		DataRow accDraftInvoiceHeader;
		DataRow nonPostedAccDraftInvoiceHeader;
	}
}

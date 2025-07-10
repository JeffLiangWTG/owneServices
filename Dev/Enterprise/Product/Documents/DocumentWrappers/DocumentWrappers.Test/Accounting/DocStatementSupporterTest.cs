using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using static Enterprise.DocumentWrappers.DocStatement;

namespace Enterprise.DocumentWrappers
{
	sealed class DocStatementSupporterTest : TestCaseWithFactory
	{
		PrintStatement statement;
		DocStatement statementWrapper;
		DocStatementGenericTransactionSupporter statementSupporter;

		protected override void SetUp()
		{
			base.SetUp();

			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "this is a test Org AAA";
			organisation.OH_Code = "AAA";
			OrgCompanyData companyData = organisation.CompanyData;
			companyData.OB_ARCreditAgreedPaymentMethod = "CHK";
			companyData.OB_APCreditAgreedPaymentMethod = "TRF";

			statement = new PrintStatement(Factory, GlbBranch.CurrentBranch);
			statement.OrganisationPK = organisation.PK;

			statementWrapper = DocStatement.New(statement, Factory);
			statementSupporter = new DocStatementGenericTransactionSupporter(statementWrapper);
			Factory.Save();
		}

		public void TestReceiptBankAccountIBAN()
		{
			statement.CurrencyNK = "USD";
			const string testIBAN = "ES2637011181545485279943";

			var accountBisObj = Factory.New<AccBankAccount>();
			accountBisObj.AB_GC = statement.Company.PK;
			accountBisObj.AB_RX_NKAccountCurrency = statement.CurrencyNK;
			accountBisObj.AB_IsDefaultReceiptBankAccount = ZBool.True;
			accountBisObj.AB_AG = Factory.NewWithValidTestData(typeof(AccGLHeader)).PK;
			accountBisObj.IBAN = testIBAN;
			accountBisObj.AB_Code = "ABCBANK";
			Factory.Save();

			AssertEquals("Precondition", testIBAN, statementWrapper.ReceiptBankAccount.IBAN);
			AssertEquals("IBAN value should as GetReceiptBankAccountIBAN result", testIBAN, statementSupporter.GetReceiptBankAccountIBAN());
		}

		public void TestGetOrganisationName()
		{
			AssertEquals("this is a test Org AAA", statementSupporter.GetOrganisationName());
		}

		public void TestOrganisationARAgreedPaymentMethod()
		{
			AssertEquals("Business Check", statementSupporter.GetOrganisationARAgreedPaymentMethod());
		}

		public void TestOrganisationAPAgreedPaymentMethod()
		{
			AssertEquals("Bank Transfer", statementSupporter.GetOrganisationAPAgreedPaymentMethod());
		}
	}
}

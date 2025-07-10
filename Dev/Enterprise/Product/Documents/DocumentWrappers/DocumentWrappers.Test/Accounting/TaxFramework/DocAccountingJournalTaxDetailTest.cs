using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.DocumentWrappers.Accounting.TaxFramework;
using Moq;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	sealed class DocAccountingJournalTaxDetailTest : DocBaseWrapperTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var accountingJournalTaxDetailsMock = new Mock<IAccountingJournalTaxDetail>();
			accountingJournalTaxDetailsMock.Setup(x => x.TaxConfiguration).Returns("AU-TAX1");
			accountingJournalTaxDetailsMock.Setup(x => x.GLAccount).Returns("10.10.10.01");
			accountingJournalTaxDetailsMock.Setup(x => x.GLAccountDesc).Returns("Test GL account");
			accountingJournalTaxDetailsMock.Setup(x => x.BranchCode).Returns("BRN");
			accountingJournalTaxDetailsMock.Setup(x => x.DepartmentCode).Returns("SYD");
			accountingJournalTaxDetailsMock.Setup(x => x.Amount).Returns(120M);
			accountingJournalTaxDetailsMock.Setup(x => x.PostDate).Returns(new ZDate("2021-03-13"));
			accountingJournalTaxDetailsMock.Setup(x => x.PostPeriod).Returns("202101");
			accountingJournalTaxDetailsMock.Setup(x => x.Basis).Returns("PTM");
			accountingJournalTaxDetailsMock.Setup(x => x.OSAmount).Returns(12M);
			accountingJournalTaxDetailsMock.Setup(x => x.Currency).Returns(testObjectCreator.USD);
			accountingJournalTaxDetailsMock.Setup(x => x.LocalCurrency).Returns(testObjectCreator.AUD);
			accountingJournalTaxDetailsMock.Setup(x => x.AlternateAccountNum).Returns("101");
			accountingJournalTaxDetailsMock.Setup(x => x.AlternateAccountDesc).Returns("A");

			return DocAccountingJournalTaxDetail.New(accountingJournalTaxDetailsMock.Object, Factory);
		}

		public void TestConfigurationCode()
		{
			var expectedValue = "ABC";
			var accTaxDetailMock = new Mock<IAccountingJournalTaxDetail>();
			accTaxDetailMock.Setup(x => x.TaxConfiguration).Returns(expectedValue);

			var docTaxDetail = DocAccountingJournalTaxDetail.New(accTaxDetailMock.Object, Factory);
			AssertEquals(expectedValue, docTaxDetail.ConfigurationCode);
		}

		public void TestBasis()
		{
			var expectedValue = "PMT";
			var accTaxDetailMock = new Mock<IAccountingJournalTaxDetail>();
			accTaxDetailMock.Setup(x => x.Basis).Returns(expectedValue);

			var docTaxDetail = DocAccountingJournalTaxDetail.New(accTaxDetailMock.Object, Factory);
			AssertEquals(expectedValue, docTaxDetail.Basis);
		}

		public void TestGLAccount()
		{
			var expectedValue = "10.10.10.10";
			var accTaxDetailMock = new Mock<IAccountingJournalTaxDetail>();
			accTaxDetailMock.Setup(x => x.GLAccount).Returns(expectedValue);

			var docTaxDetail = DocAccountingJournalTaxDetail.New(accTaxDetailMock.Object, Factory);
			AssertEquals(expectedValue, docTaxDetail.GLAccount);
		}

		public void TestGLDescription()
		{
			var expectedValue = "Test GL account";
			var accTaxDetailMock = new Mock<IAccountingJournalTaxDetail>();
			accTaxDetailMock.Setup(x => x.GLAccountDesc).Returns(expectedValue);

			var docTaxDetail = DocAccountingJournalTaxDetail.New(accTaxDetailMock.Object, Factory);
			AssertEquals(expectedValue, docTaxDetail.GLDescription);
		}

		public void TestAlternateAccountNum()
		{
			var expectedValue = "101";
			var accTaxDetailMock = new Mock<IAccountingJournalTaxDetail>();
			accTaxDetailMock.Setup(x => x.AlternateAccountNum).Returns(expectedValue);

			var docTaxDetail = DocAccountingJournalTaxDetail.New(accTaxDetailMock.Object, Factory);
			AssertEquals(expectedValue, docTaxDetail.AlternateAccountNum);
		}

		public void TestAlternateAccountDesc()
		{
			var expectedValue = "A";
			var accTaxDetailMock = new Mock<IAccountingJournalTaxDetail>();
			accTaxDetailMock.Setup(x => x.AlternateAccountDesc).Returns(expectedValue);

			var docTaxDetail = DocAccountingJournalTaxDetail.New(accTaxDetailMock.Object, Factory);
			AssertEquals(expectedValue, docTaxDetail.AlternateAccountDesc);
		}

		public void TestPostDate()
		{
			var expectedValue = new ZDate(2020, 10, 20);
			var accTaxDetailMock = new Mock<IAccountingJournalTaxDetail>();
			accTaxDetailMock.Setup(x => x.PostDate).Returns(expectedValue);

			var docTaxDetail = DocAccountingJournalTaxDetail.New(accTaxDetailMock.Object, Factory);
			AssertEquals(expectedValue, docTaxDetail.PostDate);
		}

		public void TestPostPeriod()
		{
			var expectedValue = "202101";
			var accTaxDetailMock = new Mock<IAccountingJournalTaxDetail>();
			accTaxDetailMock.Setup(x => x.PostPeriod).Returns(expectedValue);

			var docTaxDetail = DocAccountingJournalTaxDetail.New(accTaxDetailMock.Object, Factory);
			AssertEquals(expectedValue, docTaxDetail.PostPeriod);
		}

		public void TestBranchCode()
		{
			var expectedValue = "BRN";
			var accTaxDetailMock = new Mock<IAccountingJournalTaxDetail>();
			accTaxDetailMock.Setup(x => x.BranchCode).Returns(expectedValue);

			var docTaxDetail = DocAccountingJournalTaxDetail.New(accTaxDetailMock.Object, Factory);
			AssertEquals(expectedValue, docTaxDetail.BranchCode);
		}

		public void TestDepartmentCode()
		{
			var expectedValue = "FIA";
			var accTaxDetailMock = new Mock<IAccountingJournalTaxDetail>();
			accTaxDetailMock.Setup(x => x.DepartmentCode).Returns(expectedValue);

			var docTaxDetail = DocAccountingJournalTaxDetail.New(accTaxDetailMock.Object, Factory);
			AssertEquals(expectedValue, docTaxDetail.DepartmentCode);
		}

		public void TestPositiveAmount()
		{
			var expectedValue = 45M;
			var accTaxDetailMock = new Mock<IAccountingJournalTaxDetail>();
			accTaxDetailMock.Setup(x => x.Amount).Returns(expectedValue);

			var docTaxDetail = DocAccountingJournalTaxDetail.New(accTaxDetailMock.Object, Factory);
			AssertEquals(expectedValue, docTaxDetail.DebitAmountDecimal);
			AssertEquals("45.00", docTaxDetail.DebitAmountString);

			AssertEquals(0M, docTaxDetail.CreditAmountDecimal);
			AssertEquals(string.Empty, docTaxDetail.CreditAmountString);
		}

		public void TestPositiveAmountWithOSAmount()
		{
			var expectedValue = 45M;
			var accTaxDetailMock = new Mock<IAccountingJournalTaxDetail>();
			accTaxDetailMock.Setup(x => x.Amount).Returns(expectedValue);
			accTaxDetailMock.Setup(x => x.OSAmount).Returns(expectedValue);
			accTaxDetailMock.Setup(x => x.LocalCurrency).Returns(TestObjectCreator.AUD);
			accTaxDetailMock.Setup(x => x.Currency).Returns(TestObjectCreator.USD);

			var docTaxDetail = DocAccountingJournalTaxDetail.New(accTaxDetailMock.Object, Factory);
			AssertEquals(expectedValue, docTaxDetail.OSAmountDecimal);
			AssertEquals("45.00 DR", docTaxDetail.OSAmountString);

			accTaxDetailMock.Setup(x => x.Currency).Returns(TestObjectCreator.AUD);
			docTaxDetail = DocAccountingJournalTaxDetail.New(accTaxDetailMock.Object, Factory);
			AssertEquals(expectedValue, docTaxDetail.OSAmountDecimal);
			AssertEquals(string.Empty, docTaxDetail.OSAmountString);
		}

		public void TestNegativeAmount()
		{
			var expectedValue = -45M;
			var accTaxDetailMock = new Mock<IAccountingJournalTaxDetail>();
			accTaxDetailMock.Setup(x => x.Amount).Returns(expectedValue);

			var docTaxDetail = DocAccountingJournalTaxDetail.New(accTaxDetailMock.Object, Factory);
			AssertEquals(0M, docTaxDetail.DebitAmountDecimal);
			AssertEquals(string.Empty, docTaxDetail.DebitAmountString);

			AssertEquals(-1 * expectedValue, docTaxDetail.CreditAmountDecimal);
			AssertEquals("45.00", docTaxDetail.CreditAmountString);
		}

		public void TestNegativeAmountWithOSAmount()
		{
			var expectedValue = -45M;
			var accTaxDetailMock = new Mock<IAccountingJournalTaxDetail>();
			accTaxDetailMock.Setup(x => x.Amount).Returns(expectedValue);
			accTaxDetailMock.Setup(x => x.OSAmount).Returns(expectedValue);
			accTaxDetailMock.Setup(x => x.LocalCurrency).Returns(TestObjectCreator.AUD);
			accTaxDetailMock.Setup(x => x.Currency).Returns(TestObjectCreator.USD);

			var docTaxDetail = DocAccountingJournalTaxDetail.New(accTaxDetailMock.Object, Factory);
			AssertEquals(-1 * expectedValue, docTaxDetail.OSAmountDecimal);
			AssertEquals("45.00 CR", docTaxDetail.OSAmountString);

			accTaxDetailMock.Setup(x => x.Currency).Returns(TestObjectCreator.AUD);
			docTaxDetail = DocAccountingJournalTaxDetail.New(accTaxDetailMock.Object, Factory);
			AssertEquals(-1 * expectedValue, docTaxDetail.OSAmountDecimal);
			AssertEquals(string.Empty, docTaxDetail.OSAmountString);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}

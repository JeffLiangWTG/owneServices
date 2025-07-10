using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccountingJournalTaxDetail))]
	public class AccountingJournalTaxDetailTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructorParameters()
		{
			var glMovementDetailsMock = new Mock<IGLMovementDetails>();
			var glMovementDetails = glMovementDetailsMock.Object;
			AssertExceptionThrown("No parameters passed", typeof(ArgumentException), "Value cannot be null.\r\nParameter name: factory", () => _ = new AccountingJournalTaxDetailForTest(null, null));
			AssertExceptionThrown("No dynamicBusinessObject passed", typeof(ArgumentException), "Value cannot be null.\r\nParameter name: glMovementDetails", () => _ = new AccountingJournalTaxDetailForTest(Factory, null));
			AssertExceptionThrown("No factory passed", typeof(ArgumentException), "Value cannot be null.\r\nParameter name: factory", () => _ = new AccountingJournalTaxDetailForTest(null, glMovementDetails));

			AssertNoExceptionThrown("Both parameters passed", () => _ = new AccountingJournalTaxDetailForTest(Factory, glMovementDetails));
		}

		public void TestCreateMethod()
		{
			var glMovementDetailsList = new List<IGLMovementDetails>();
			var glMovementDetailsMock1 = new Mock<IGLMovementDetails>();
			var glMovementDetailsMock2 = new Mock<IGLMovementDetails>();

			glMovementDetailsList.Add(glMovementDetailsMock1.Object);
			glMovementDetailsList.Add(glMovementDetailsMock2.Object);

			var result = AccountingJournalTaxDetail.Create(Factory, glMovementDetailsList);

			AssertEquals("2 items retuned from the Create method", 2, result.Count);
			AssertEquals("The result is of type List<AccountingJournalTaxDetail>",  typeof(List<AccountingJournalTaxDetail>), result.GetType());
		}

		public void TestAccountingJournalTaxDetailMapping()
		{
			var taxConfiguration = "AU-TAX";
			var glAccount = "1001.10.10";
			var glAccountDesc = "Tax Account";
			var postPeriod = "202101";
			var postDate = new ZDate("2021-01-31");
			var amount = 100M;
			var basis = "PMT";
			var branchCode = "BRN";
			var departmentCode = "SYD";
			var osAmount = -12M;
			var currencyCode = "USD";

			var glMovementDetailsMock = new Mock<IGLMovementDetails>();
			glMovementDetailsMock.Setup(x => x.TaxConfiguration).Returns(taxConfiguration);
			glMovementDetailsMock.Setup(x => x.GLAccount).Returns(glAccount);
			glMovementDetailsMock.Setup(x => x.GLAccountDesc).Returns(glAccountDesc);
			glMovementDetailsMock.Setup(x => x.PostPeriod).Returns(postPeriod);
			glMovementDetailsMock.Setup(x => x.PostDate).Returns(postDate);
			glMovementDetailsMock.Setup(x => x.Amount).Returns(amount);
			glMovementDetailsMock.Setup(x => x.Basis).Returns(basis);
			glMovementDetailsMock.Setup(x => x.BranchCode).Returns(branchCode);
			glMovementDetailsMock.Setup(x => x.DepartmentCode).Returns(departmentCode);
			glMovementDetailsMock.Setup(x => x.OSAmount).Returns(osAmount);
			glMovementDetailsMock.Setup(x => x.CurrencyCode).Returns(currencyCode);

			var glMovementDetails = glMovementDetailsMock.Object;

			var journalTaxDetails = new AccountingJournalTaxDetailForTest(Factory, glMovementDetails);
			AssertEquals(glMovementDetails.TaxConfiguration, journalTaxDetails.TaxConfiguration);
			AssertEquals(glMovementDetails.GLAccount, journalTaxDetails.GLAccount);
			AssertEquals(glMovementDetails.GLAccountDesc, journalTaxDetails.GLAccountDesc);
			AssertEquals(glMovementDetails.PostPeriod, journalTaxDetails.PostPeriod);
			AssertEquals(glMovementDetails.PostDate, journalTaxDetails.PostDate);
			AssertEquals(glMovementDetails.Amount, journalTaxDetails.Amount);
			AssertEquals(glMovementDetails.Basis, journalTaxDetails.Basis);
			AssertEquals(glMovementDetails.BranchCode, journalTaxDetails.BranchCode);
			AssertEquals(glMovementDetails.DepartmentCode, journalTaxDetails.DepartmentCode);
			AssertEquals(glMovementDetails.OSAmount, journalTaxDetails.OSAmount);
			AssertNotNull(journalTaxDetails.Currency);
			AssertEquals(glMovementDetails.CurrencyCode, journalTaxDetails.Currency.RX_Code);
			AssertNotNull(journalTaxDetails.LocalCurrency);
			AssertEquals(GlbCompany.CurrentCompany.LocalCurrency.RX_Code, journalTaxDetails.LocalCurrency.RX_Code);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var glMovementDetailsMock = new Mock<IGLMovementDetails>();
			var glMovementDetails = glMovementDetailsMock.Object;

			return new AccountingJournalTaxDetailForTest(Factory, glMovementDetails);
		}
	}
}

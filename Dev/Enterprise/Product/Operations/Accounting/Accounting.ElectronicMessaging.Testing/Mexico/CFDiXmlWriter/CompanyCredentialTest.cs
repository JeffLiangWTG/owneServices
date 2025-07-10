using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	public class CompanyCredentialTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<CompanyCredential>(new CFDiXmlWriter().CompanyCredential_ExposedForTestOnly);
		}

		public void TestCompanyCredential_LoadBestCertificateUsingOrderByColumn()
		{
			var company = GlbCompany.CurrentCompany;
			var transactionInfo = SetTransactionInfo(company);
			var companyCredential = (ICompanyCredential)new CompanyCredential();

			var companyCredential1 = SaveAndGetCompanyEInvoicingCredential_ForTestOnly(company);
			var companyCredential2 = SaveAndGetCompanyEInvoicingCredential_ForTestOnly(company);
			companyCredential2.GP_ExpiryDate = ZDateTime.Now.AddMonths(1);
			Factory.Save();

			var getCompanyCredential = companyCredential.GetCompanyCredential(transactionInfo);

			AssertNotNull(getCompanyCredential);
			AssertEquals(companyCredential2.GP_ExpiryDate, getCompanyCredential.GP_ExpiryDate);

			companyCredential1.GP_ExpiryDate = ZDateTime.Now.AddMonths(2);
			companyCredential2.GP_ExpiryDate = ZDateTime.Now.AddMonths(1);
			Factory.Save();

			getCompanyCredential = companyCredential.GetCompanyCredential(transactionInfo);

			AssertNotNull(getCompanyCredential);
			AssertEquals(companyCredential1.GP_ExpiryDate, getCompanyCredential.GP_ExpiryDate);
		}

		public void TestCompanyCredential_PasswordTypes()
		{
			var company = GlbCompany.CurrentCompany;
			var transactionInfo = SetTransactionInfo(company);

			SaveAndGetCompanyEInvoicingCredential_ForTestOnly(company, "XXX");
			var companyCredential = (ICompanyCredential)new CompanyCredential();

			var getCompanyCredential = companyCredential.GetCompanyCredential(transactionInfo);
			AssertNull("Password type doesn't match with the requiered value (EIM)", getCompanyCredential);

			var existingCredential = SaveAndGetCompanyEInvoicingCredential_ForTestOnly(company);
			getCompanyCredential = companyCredential.GetCompanyCredential(transactionInfo);
			AssertNotNull(getCompanyCredential);
			AssertEquals(existingCredential.GP_PasswordType, getCompanyCredential.GP_PasswordType);
		}

		public void TestCompanyCredential_TransactionDate()
		{
			var company = GlbCompany.CurrentCompany;
			var transactionInfo = SetTransactionInfo(company);
			transactionInfo.TransactionDate = ZDateTime.Now.AddDays(-5);

			SaveAndGetCompanyEInvoicingCredential_ForTestOnly(company);
			var companyCredential = (ICompanyCredential)new CompanyCredential();

			var getCompanyCredential = companyCredential.GetCompanyCredential(transactionInfo);
			AssertNull("TransactionInfo not fit with certificate dates.", getCompanyCredential);

			transactionInfo.TransactionDate = ZDateTime.Now;
			getCompanyCredential = companyCredential.GetCompanyCredential(transactionInfo);
			AssertNotNull(getCompanyCredential);
		}

		public void TestCompanyCredential_WithDifferentCompanyOnTransactionInfo()
		{
			var company = GlbCompany.CurrentCompany;

			var otherCompany = Factory.CreateNewFactory().NewWithValidTestData<GlbCompany>();
			var otherCompanyBranch = otherCompany.Branches.AddNew();
			otherCompanyBranch.GB_Code = "MX1";

			var transactionInfo = SetTransactionInfo(otherCompany);

			var existingCredential = SaveAndGetCompanyEInvoicingCredential_ForTestOnly(company);
			var companyCredential = (ICompanyCredential)new CompanyCredential();

			var getCompanyCredential = companyCredential.GetCompanyCredential(transactionInfo);
			AssertNull("TransactionInfo correspond to a different company", getCompanyCredential);

			transactionInfo.Branch = new Branch() { Code = company.Branches[0].GB_Code };

			getCompanyCredential = companyCredential.GetCompanyCredential(transactionInfo);
			AssertNotNull(getCompanyCredential);
			AssertEquals(existingCredential.GP_GC, getCompanyCredential.GP_GC);
		}

		public void TestCompanyCredential_WithNullOrEmptyReturnValue()
		{
			TransactionInfo transactionInfo = null;
			var companyCredential = (ICompanyCredential)new CompanyCredential();

			var getCompanyCredential = companyCredential.GetCompanyCredential(transactionInfo);
			AssertNull("Transaction is null.", getCompanyCredential);

			transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			getCompanyCredential = companyCredential.GetCompanyCredential(transactionInfo);
			AssertNull("TransactionInfo exists but it is null.", getCompanyCredential);

			transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionDate = ZDateTime.Now,
				Branch = null
			};
			getCompanyCredential = companyCredential.GetCompanyCredential(transactionInfo);
			AssertNull("Branch is null.", getCompanyCredential);

			transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionDate = ZDateTime.Now,
				Branch = new Branch() { Code = null }
			};
			getCompanyCredential = companyCredential.GetCompanyCredential(transactionInfo);
			AssertNull("Branch is empty.", getCompanyCredential);

			transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionDate = ZDateTime.Now,
				Branch = new Branch() { Code = "XXX" }
			};
			getCompanyCredential = companyCredential.GetCompanyCredential(transactionInfo);
			AssertNull("Branch code doesn't exists and Company is null.", getCompanyCredential);

			transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionDate = ZDateTime.Now,
				Branch = new Branch() { Code = "SYD" }
			};
			getCompanyCredential = companyCredential.GetCompanyCredential(transactionInfo);
			AssertNull("Doesn't exist certificate for the Company", getCompanyCredential);
		}

		#region Implementation

		GlbCompanyEInvoicingCertificateCredential SaveAndGetCompanyEInvoicingCredential_ForTestOnly(GlbCompany company, string passwordType = PasswordTypesList.Codes.EIM)
		{
			var credentialForTestOnly = Factory.NewWithValidTestData<GlbCompanyEInvoicingCertificateCredential>();

			credentialForTestOnly.GP_GC = company.PK;
			credentialForTestOnly.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			credentialForTestOnly.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			credentialForTestOnly.GP_PasswordType = passwordType;
			credentialForTestOnly.GP_IssueDate = ZDateTime.Now.AddDays(-1);
			credentialForTestOnly.GP_ExpiryDate = ZDateTime.Now.AddDays(1);
			Factory.Save();

			return credentialForTestOnly;
		}

		static TransactionInfo SetTransactionInfo(GlbCompany company) => new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
		{
			TransactionDate = ZDateTime.Now,
			Branch = new Branch() { Code = company.Branches[0].GB_Code }
		};

		#endregion
	}
}

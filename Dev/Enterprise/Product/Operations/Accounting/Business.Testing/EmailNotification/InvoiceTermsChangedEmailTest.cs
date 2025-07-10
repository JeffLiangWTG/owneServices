using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class InvoiceTermsChangedEmailTest : MasterFiles.Business.Testing.AccountingEmailDefTest
	{
		protected override Type EmailDefType
		{
			get
			{
				return typeof(InvoiceTermsChangedEmail);
			}
		}

		public void TestContentForChangedTerms()
		{
			ZDateTime invoiceDate = new ZDateTime(2006, 3, 12, 14, 23, 04);
			TestObjectCreator.AALSHI.OH_FullName = "AALSHI";
			TestObjectCreator.AALSHI.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Core.Constants.InvoiceTerms.FromMonthEnd;
			TestObjectCreator.AALSHI.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 5;
			ARInvoice aRInvoice = Factory.New<ARInvoice>();
			aRInvoice.AH_TransactionNum = "TEST";
			aRInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			aRInvoice.AH_InvoiceDate = invoiceDate;
			aRInvoice.AH_InvoiceTerm = Core.Constants.InvoiceTerms.FromPeriodEnd;
			aRInvoice.AH_InvoiceTermDays = 5;
			InvoiceTermsChangedEmail email = new InvoiceTermsChangedEmail(aRInvoice);
			string expectedResult = String.Format("AR Invoice TEST for AALSHI dated {0} has been posted by {1} with invoice terms " + "different to the default invoice terms for AALSHI", invoiceDate.ToShortDateString(), Env.CurrentUser.FullName);
			AssertEquals(expectedResult, email.GetContent());
		}

		public void TestContentForChangedDays()
		{
			ZDateTime invoiceDate = new ZDateTime(2006, 3, 12, 14, 23, 04);
			TestObjectCreator.AALSHI.OH_FullName = "AALSHI";
			TestObjectCreator.AALSHI.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Core.Constants.InvoiceTerms.FromMonthEnd;
			TestObjectCreator.AALSHI.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 5;
			ARInvoice aRInvoice = Factory.New<ARInvoice>();
			aRInvoice.AH_TransactionNum = "TEST";
			aRInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			aRInvoice.AH_InvoiceDate = invoiceDate;
			aRInvoice.AH_InvoiceTerm = Core.Constants.InvoiceTerms.FromMonthEnd;
			aRInvoice.AH_InvoiceTermDays = 7;
			InvoiceTermsChangedEmail email = new InvoiceTermsChangedEmail(aRInvoice);
			string expectedResult = String.Format("AR Invoice TEST for AALSHI dated {0} has been posted by {1} with invoice days " + "different to the default invoice days for AALSHI", invoiceDate.ToShortDateString(), Env.CurrentUser.FullName);
			AssertEquals(expectedResult, email.GetContent());
		}

		public void TestContentForBothChangedDaysAndTerm()
		{
			ZDateTime invoiceDate = new ZDateTime(2006, 3, 12, 14, 23, 04);
			TestObjectCreator.AALSHI.OH_FullName = "AALSHI";
			TestObjectCreator.AALSHI.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Core.Constants.InvoiceTerms.FromPeriodEnd;
			TestObjectCreator.AALSHI.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 7;
			TestObjectCreator.AALSHI.CompanyData.CreateOrLoadARTerm(InvoiceTypesList.Codes.InvoicePerTaxCode).PY_InvoiceTerm = Core.Constants.InvoiceTerms.FromMonthEnd;
			TestObjectCreator.AALSHI.CompanyData.CreateOrLoadARTerm(InvoiceTypesList.Codes.InvoicePerTaxCode).PY_InvoiceDays = 5;
			ARInvoice aRInvoice = Factory.New<ARInvoice>();
			aRInvoice.AH_TransactionNum = "TEST";
			aRInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			aRInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.InvoicePerTaxCode;
			aRInvoice.AH_InvoiceDate = invoiceDate;
			aRInvoice.AH_InvoiceTerm = Core.Constants.InvoiceTerms.FromPeriodEnd;
			aRInvoice.AH_InvoiceTermDays = 7;
			InvoiceTermsChangedEmail email = new InvoiceTermsChangedEmail(aRInvoice);
			string expectedResult = String.Format("AR Invoice TEST for AALSHI dated {0} has been posted by {1} with invoice terms and days " + "different to the default invoice terms and days for AALSHI", invoiceDate.ToShortDateString(), Env.CurrentUser.FullName);
			AssertEquals(expectedResult, email.GetContent());
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}

				return fTestObjectCreator;
			}
		}
	}
}
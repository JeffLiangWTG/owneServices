using System;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Web.Business.Testing
{
	[TestedType(typeof(UpdateInvoicePaymentDetailsRequest))]
	public class UpdateInvoicePaymentDetailsRequestTestCase : BaseITransactionRequestTestCase
	{
		#region Test Cases

		public override void TestConstructor()
		{
			base.TestConstructor();
			AssertEquals("InternalReference", null, Request.InternalReference);
			AssertEquals("PaymentReference", null, Request.PaymentReference);
			AssertEquals("PaymentDate", null, Request.PaymentDate);
			AssertEquals("AmountPaidInCompanyCurrency", 0m, Request.AmountPaidInCompanyCurrency);
		}

		public void TestPaymentReference()
		{
			AssertEquals(null, Request.PaymentReference);

			Request.PaymentReference = "1234";
			AssertEquals("1234", Request.PaymentReference);

			Request.PaymentReference = "4321";
			AssertEquals("4321", Request.PaymentReference);
		}

		public void TestPaymentDate()
		{
			AssertEquals(null, Request.PaymentDate);

			DateTime date1 = ZDateTime.Today.ToDateTime();
			Request.PaymentDate = date1;
			AssertEquals(date1, Request.PaymentDate);

			DateTime date2 = ZDateTime.Today.AddDays(-5).ToDateTime();
			Request.PaymentDate = date2;
			AssertEquals(date2, Request.PaymentDate);
		}

		public void TestAmountPaidInCompanyCurrency()
		{
			AssertEquals(decimal.Zero, Request.AmountPaidInCompanyCurrency);

			Request.AmountPaidInCompanyCurrency = 1234M;
			AssertEquals(1234M, Request.AmountPaidInCompanyCurrency);

			Request.AmountPaidInCompanyCurrency = 4321M;
			AssertEquals(4321M, Request.AmountPaidInCompanyCurrency);
		}

		public void TestValidateAll()
		{
			var connection = ((IDbConnectionInternals)TestConnection).ADOConnection;
			var transaction = ((IDbConnectionInternals)TestConnection).ADOTransaction;
			var dataAccess = new TransactionPaymentDataAccess(connection, transaction);
			CreateNewCompanyWithCurrencySubUnitRatio("AAA", 1);
			CreateNewCompanyWithCurrencySubUnitRatio("BBB", 10);
			CreateNewCompanyWithCurrencySubUnitRatio("CCC", 100);
			CreateNewCompanyWithCurrencySubUnitRatio("DDD", 1000);

			Request.TransactionType = "INV";
			Request.AccLedger = "AR";
			Request.OrgCode = "ABC";
			Request.TransactionNumber = "12345";
			Request.AmountPaidInCompanyCurrency = 0M;
			AssertEquals("CompanyCode cannot be empty. Please, use " + Constants.ProductName + " Company Code.", Request.ValidateAll(dataAccess));
			Request.CompanyCode = "ABC";
			AssertEquals("Cannot get a valid company currency subunit ratio based on the specific company code: ABC.", Request.ValidateAll(dataAccess));
			Request.CompanyCode = "CCC";
			AssertEquals("AmountPaidInCompanyCurrency cannot be zero.", Request.ValidateAll(dataAccess));

			Request.AmountPaidInCompanyCurrency = 123M;
			Request.CompanyCode = "AAA";
			AssertNull(Request.ValidateAll(dataAccess));
			Request.CompanyCode = "BBB";
			AssertNull(Request.ValidateAll(dataAccess));
			Request.CompanyCode = "CCC";
			AssertNull(Request.ValidateAll(dataAccess));
			Request.CompanyCode = "DDD";
			AssertNull(Request.ValidateAll(dataAccess));

			Request.AmountPaidInCompanyCurrency = 123.4M;
			Request.CompanyCode = "AAA";
			AssertEquals("AmountPaidInCompanyCurrency exceed the number of decimals that are allowed by the company's currency.", Request.ValidateAll(dataAccess));
			Request.CompanyCode = "BBB";
			AssertNull(Request.ValidateAll(dataAccess));
			Request.CompanyCode = "CCC";
			AssertNull(Request.ValidateAll(dataAccess));
			Request.CompanyCode = "DDD";
			AssertNull(Request.ValidateAll(dataAccess));

			Request.AmountPaidInCompanyCurrency = 123.45M;
			Request.CompanyCode = "AAA";
			AssertEquals("AmountPaidInCompanyCurrency exceed the number of decimals that are allowed by the company's currency.", Request.ValidateAll(dataAccess));
			Request.CompanyCode = "BBB";
			AssertEquals("AmountPaidInCompanyCurrency exceed the number of decimals that are allowed by the company's currency.", Request.ValidateAll(dataAccess));
			Request.CompanyCode = "CCC";
			AssertNull(Request.ValidateAll(dataAccess));
			Request.CompanyCode = "DDD";
			AssertNull(Request.ValidateAll(dataAccess));

			Request.AmountPaidInCompanyCurrency = 123.456M;
			Request.CompanyCode = "AAA";
			AssertEquals("AmountPaidInCompanyCurrency exceed the number of decimals that are allowed by the company's currency.", Request.ValidateAll(dataAccess));
			Request.CompanyCode = "BBB";
			AssertEquals("AmountPaidInCompanyCurrency exceed the number of decimals that are allowed by the company's currency.", Request.ValidateAll(dataAccess));
			Request.CompanyCode = "CCC";
			AssertEquals("AmountPaidInCompanyCurrency exceed the number of decimals that are allowed by the company's currency.", Request.ValidateAll(dataAccess));
			Request.CompanyCode = "DDD";
			AssertNull(Request.ValidateAll(dataAccess));

			void CreateNewCompanyWithCurrencySubUnitRatio(string companyCode, int companyCurrencySubUnitRatio)
			{
				var testObjectCreator = new TestObjectCreator(Factory);
				var company = testObjectCreator.CreateNewCompany(companyCode, Constants.CountryCodes.Australia);

				var currency = Factory.NewWithValidTestData<RefCurrency>();
				currency.RX_SubUnitRatio = companyCurrencySubUnitRatio;

				company.GC_RX_NKLocalCurrency = currency.Code;
				Factory.Save();
			}
		}

		#endregion

		#region Implementation

		protected override string ExpectedTransactionTypeHint
		{
			get
			{
				return @"A valid TransactionType should be provided. Please, use 
	INV for Invoice or
	CRD for Credit Note.";
			}
		}

		protected override ITransactionNaturalKeys GetNewRequest()
		{
			return new UpdateInvoicePaymentDetailsRequest();
		}

		protected new UpdateInvoicePaymentDetailsRequest Request
		{
			get { return (UpdateInvoicePaymentDetailsRequest)base.Request; }
		}

		#endregion
	}
}

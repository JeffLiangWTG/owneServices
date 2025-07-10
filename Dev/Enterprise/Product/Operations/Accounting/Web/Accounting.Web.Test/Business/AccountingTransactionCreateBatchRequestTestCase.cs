using CargoWise.BrandManager;
using NUnit.Framework;

namespace Enterprise.Accounting.Web.Business.Testing
{
	public class AccountingTransactionCreateBatchRequestTestCase : TestCase
	{
		#region Test Cases

		public void TestConstructor()
		{
			AssertNotNull(Request);
			AssertEquals("CompanyCode", null, Request.CompanyCode);
		}

		public void TestCompanyCode()
		{
			AssertEquals(null, Request.CompanyCode);

			Request.CompanyCode = "1234";
			AssertEquals("1234", Request.CompanyCode);

			Request.CompanyCode = "4321";
			AssertEquals("4321", Request.CompanyCode);
		}

		public void TestValidation()
		{
			AssertEquals(@$"CompanyCode cannot be empty. Use {BrandingFactory.Instance.ProductName} Company Code.", Request.Validate());
			Request.CompanyCode = "EDI";
			AssertNull(Request.Validate());
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Request = new AccountingTransactionCreateBatchRequest();
		}

		AccountingTransactionCreateBatchRequest Request;

		#endregion
	}
}

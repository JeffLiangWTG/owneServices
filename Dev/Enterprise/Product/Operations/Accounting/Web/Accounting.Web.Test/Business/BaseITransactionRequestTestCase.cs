using CargoWise.BrandManager;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Web.Business.Testing
{
	[TestsSubclassesOf(typeof(ITransactionNaturalKeys), ExcludeClientDlls = true)]
	public abstract class BaseITransactionRequestTestCase : TestCaseWithFactory
	{
		#region Test Cases

		public virtual void TestConstructor()
		{
			AssertNotNull(Request);
			AssertEquals("OrgCode", null, Request.OrgCode);
			AssertEquals("CompanyCode", null, Request.CompanyCode);
			AssertEquals("AccLedger", null, Request.AccLedger);
			AssertEquals("TransactionType", null, Request.TransactionType);
			AssertEquals("TransactionNumber", null, Request.TransactionNumber);
			AssertEquals("JobTransactionNumber", null, Request.JobTransactionNumber);
			AssertEquals("InternalReference", null, Request.InternalReference);
		}

		public void TestOrgCode()
		{
			AssertEquals(null, Request.OrgCode);

			Request.OrgCode = "1234";
			AssertEquals("1234", Request.OrgCode);

			Request.OrgCode = "4321";
			AssertEquals("4321", Request.OrgCode);
		}

		public void TestCompanyCode()
		{
			AssertEquals(null, Request.CompanyCode);

			Request.CompanyCode = "1234";
			AssertEquals("1234", Request.CompanyCode);

			Request.CompanyCode = "4321";
			AssertEquals("4321", Request.CompanyCode);
		}

		public void TestAccLedger()
		{
			AssertEquals(null, Request.AccLedger);

			Request.AccLedger = "1234";
			AssertEquals("1234", Request.AccLedger);

			Request.AccLedger = "4321";
			AssertEquals("4321", Request.AccLedger);
		}

		public void TestTransactionType()
		{
			AssertEquals(null, Request.TransactionType);

			Request.TransactionType = "1234";
			AssertEquals("1234", Request.TransactionType);

			Request.TransactionType = "4321";
			AssertEquals("4321", Request.TransactionType);
		}

		public void TestTransactionNumber()
		{
			AssertEquals(null, Request.TransactionNumber);

			Request.TransactionNumber = "1234";
			AssertEquals("1234", Request.TransactionNumber);

			Request.TransactionNumber = "4321";
			AssertEquals("4321", Request.TransactionNumber);
		}

		public void TestJobTransactionNumber()
		{
			AssertEquals(null, Request.JobTransactionNumber);

			Request.JobTransactionNumber = "1234";
			AssertEquals("1234", Request.JobTransactionNumber);

			Request.JobTransactionNumber = "4321";
			AssertEquals("4321", Request.JobTransactionNumber);
		}

		public void TestInternalReference()
		{
			AssertEquals(null, Request.InternalReference);

			Request.InternalReference = "1234";
			AssertEquals("1234", Request.InternalReference);

			Request.InternalReference = "4321";
			AssertEquals("4321", Request.InternalReference);
		}

		public virtual void TestValidation()
		{
			string expected = @$"CompanyCode cannot be empty. Please, use {BrandingFactory.Instance.ProductName} Company Code.
" + ExpectedTransactionTypeHint + @"
AccLedger cannot be empty. Specify 'AR' for Accounts Receivable or 'AP' for Accounts Payable.";
			Assert("Must contain expected " + expected, Request.Validate().Contains(expected));

			Request.CompanyCode = "ABC";
			expected = ExpectedTransactionTypeHint + @"
AccLedger cannot be empty. Specify 'AR' for Accounts Receivable or 'AP' for Accounts Payable.";
			Assert("Must contain expected " + expected, Request.Validate().Contains(expected));

			Request.TransactionType = "INN";
			Assert("Must contain expected " + expected, Request.Validate().Contains(expected));

			Request.TransactionType = "INV";
			expected = "AccLedger cannot be empty. Specify 'AR' for Accounts Receivable or 'AP' for Accounts Payable.";
			Assert("Must contain expected " + expected, Request.Validate().Contains(expected));

			Request.AccLedger = "AAPT";
			expected = "Invalid AccLedger code. Specify 'AR' for Accounts Receivable or 'AP' for Accounts Payable.";
			Assert("Must contain expected " + expected, Request.Validate().Contains(expected));

			Request.AccLedger = "AR";
			expected = "For the 'AR' AccLedger a TransactionNumber or a JobTransactionNumber should be provided.";
			Assert("Must contain expected " + expected, Request.Validate().Contains(expected));

			Request.TransactionNumber = "123";
			AssertNull(Request.Validate());

			Request.TransactionNumber = null;
			Request.JobTransactionNumber = "321";
			AssertNull(Request.Validate());

			Request.AccLedger = "AP";
			expected = "For the 'AP' AccLedger OrgCode and either TransactionNumber or InternalReference should be provided.";
			Assert("Must contain expected " + expected, Request.Validate().Contains(expected));

			Request.OrgCode = "ABC";
			Assert("Must contain expected " + expected, Request.Validate().Contains(expected));

			Request.OrgCode = null;
			Request.TransactionNumber = "123";
			Assert("Must contain expected " + expected, Request.Validate().Contains(expected));

			Request.OrgCode = "ABC";
			AssertNull(Request.Validate());

			Request.TransactionNumber = null;
			Assert("Must contain expected " + expected, Request.Validate().Contains(expected));
			Request.InternalReference = "321";
			AssertNull(Request.Validate());
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			request = GetNewRequest();
		}

		protected ITransactionNaturalKeys Request
		{
			get { return request; }
		}
		ITransactionNaturalKeys request;

		protected abstract ITransactionNaturalKeys GetNewRequest();

		protected abstract string ExpectedTransactionTypeHint { get; }

		#endregion
	}
}

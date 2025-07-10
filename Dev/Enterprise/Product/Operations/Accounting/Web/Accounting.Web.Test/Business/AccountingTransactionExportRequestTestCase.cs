using CargoWise.BrandManager;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Accounting.Web.Business.Testing
{
	public class AccountingTransactionExportRequestTestCase : TestCase
	{
		#region Test Cases

		public void TestConstructor()
		{
			AssertNotNull(Request);
			AssertEquals("CompanyCode", null, Request.CompanyCode);
			AssertEquals("BatchNumber", 0, Request.BatchNumber);
			AssertEquals("NameSpace", UniversalXmlInfo.Namespace_2011_11, Request.Namespace);
		}

		public void TestCompanyCode()
		{
			AssertEquals(null, Request.CompanyCode);

			Request.CompanyCode = "1234";
			AssertEquals("1234", Request.CompanyCode);

			Request.CompanyCode = "4321";
			AssertEquals("4321", Request.CompanyCode);
		}

		public void TestBatchNumber()
		{
			AssertEquals(0, Request.BatchNumber);

			Request.BatchNumber = 1234;
			AssertEquals(1234, Request.BatchNumber);

			Request.BatchNumber = 4321;
			AssertEquals(4321, Request.BatchNumber);
		}

		public void TestNamespace()
		{
			AssertEquals("Default value", UniversalXmlInfo.Namespace_2011_11, Request.Namespace);

			Request.Namespace = UniversalXmlInfo.Namespace_2012_11;
			AssertEquals("Assigned value", UniversalXmlInfo.Namespace_2012_11, Request.Namespace);

			Request.Namespace = "blah";
			AssertEquals("Can assigned any value", "blah", Request.Namespace);
		}

		public void TestValidation()
		{
			AssertEquals(@$"CompanyCode cannot be empty. Use {BrandingFactory.Instance.ProductName} Company Code.
BatchNumber cannot be zero.", Request.Validate());
			Request.CompanyCode = "EDI";
			AssertEquals("BatchNumber cannot be zero.", Request.Validate());
			Request.BatchNumber = 1234;
			AssertNull(Request.Validate());

			Request.Namespace = null;
			AssertEquals("Invalid namespace [(null)] - Please use a valid Universal Namespace.", Request.Validate());
			Request.Namespace = string.Empty;
			AssertEquals("Invalid namespace [] - Please use a valid Universal Namespace.", Request.Validate());
			Request.Namespace = "blah";
			AssertEquals("Invalid namespace [blah] - Please use a valid Universal Namespace.", Request.Validate());

			Request.Namespace = UniversalXmlInfo.Namespace_2012_11;
			AssertNull(Request.Validate());
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Request = new AccountingTransactionExportRequest();
		}

		AccountingTransactionExportRequest Request;

		#endregion
	}
}

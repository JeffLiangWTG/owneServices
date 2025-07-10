using CargoWise.BrandManager;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Web.Business.Testing
{
	public class CreditLimitAndBalanceRequestTestCase : TestCase
	{
		#region Test Cases

		public void TestConstructor()
		{
			AssertNotNull(Request);
			AssertEquals("OrgCode", null, Request.OrgCode);
			AssertEquals("CompanyCode", null, Request.CompanyCode);
			AssertEquals("AccLedger", null, Request.AccLedger);
			AssertEquals("OverdueAgingPeriod", 30, Request.OverdueAgingPeriod);
			AssertEquals("BalanceOverdueAgingOption", null, Request.BalanceOverdueAgingOption);
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

		public void TestOverdueAgingPeriod()
		{
			AssertEquals(30, Request.OverdueAgingPeriod);

			Request.OverdueAgingPeriod = 60;
			AssertEquals(60, Request.OverdueAgingPeriod);

			Request.OverdueAgingPeriod = 45;
			AssertEquals(45, Request.OverdueAgingPeriod);
		}

		public void TestBalanceOverdueAgingOption()
		{
			AssertEquals(null, Request.BalanceOverdueAgingOption);

			Request.BalanceOverdueAgingOption = "1234";
			AssertEquals("1234", Request.BalanceOverdueAgingOption);

			Request.BalanceOverdueAgingOption = "4321";
			AssertEquals("4321", Request.BalanceOverdueAgingOption);
		}

		public void TestValidation()
		{
			AssertEquals(@$"OrgCode cannot be empty. Use {BrandingFactory.Instance.ProductName} Organization Code or Legacy system code.
CompanyCode cannot be empty. Use {BrandingFactory.Instance.ProductName} Company Code.
BalanceOverdueAgingOption cannot be empty.  " + Request.BalanceOverdueAgingOptionPossibleValues, Request.Validate());
			Request.OverdueAgingPeriod = 0;
			AssertEquals(@$"OrgCode cannot be empty. Use {BrandingFactory.Instance.ProductName} Organization Code or Legacy system code.
OverdueAgingPeriod must be greater than zero.
CompanyCode cannot be empty. Use {BrandingFactory.Instance.ProductName} Company Code.
BalanceOverdueAgingOption cannot be empty.  " + Request.BalanceOverdueAgingOptionPossibleValues, Request.Validate());
			Request.OrgCode = "ABC";
			Request.CompanyCode = "EDI";
			Request.BalanceOverdueAgingOption = Constants.BalanceOverdueAgingOption.Balance;
			AssertEquals("OverdueAgingPeriod must be greater than zero.", Request.Validate());
			Request.OverdueAgingPeriod = 60;
			AssertNull(Request.Validate());
			Request.BalanceOverdueAgingOption = "XYZ";
			AssertEquals(@"Invalid value provided for BalanceOverdueAgingOption.  " + Request.BalanceOverdueAgingOptionPossibleValues, Request.Validate());
			Request.BalanceOverdueAgingOption = Constants.BalanceOverdueAgingOption.Balance;
			AssertNull(Request.Validate());
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Request = new CreditLimitAndBalanceRequest();
		}

		CreditLimitAndBalanceRequest Request;

		#endregion
	}
}

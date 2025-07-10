using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(AccountingEBook))]
	public class AccountingEBookTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClassConstProperties()
		{
			AssertEquals("T101", AccountingEBook.LocID);
			AssertEquals("1", AccountingEBook.BookNumber);
			AssertEquals("企业单位", AccountingEBook.CompanyType);
			AssertEquals("装卸搬运和运输代理业", AccountingEBook.Industry);
			AssertEquals("慧咨 (上海) 信息技术有限公司", AccountingEBook.DevelopmentCompany);
			AssertEquals("CW1", AccountingEBook.Version);
			AssertEquals("GB/T 24589.1-2010", AccountingEBook.ChinaStandardVersion);
		}

		public void TestClassProperties()
		{
			AccountingEBook accountingEBook = new AccountingEBook();
			AssertEquals(ZString.Format("{0}{1}", LocalCompanyName.GetCurrentCompanyLocalName(), "账套"), accountingEBook.BookName);
			AssertEquals(LocalCompanyName.GetCurrentCompanyLocalName(), accountingEBook.CompanyName);
			AssertEquals(GlbCompany.CurrentCompany.OrgProxy.LocalBusinessRegNo, accountingEBook.CompanyRegistrationCode);
			AssertEquals(GlbCompany.CurrentCompany.LocalCurrency.RX_DescMultilingual.ToString(Constants.Languages.ChineseSimplified), accountingEBook.BaseCurrency);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AccountingEBook();
		}
	}
}

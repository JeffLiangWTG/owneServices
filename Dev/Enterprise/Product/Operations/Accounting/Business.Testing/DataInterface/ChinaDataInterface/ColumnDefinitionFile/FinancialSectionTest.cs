using CargoWise.BrandManager;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public class FinancialSectionTest : FileOutputTest
	{
		protected override void SetValuesBeforeAssert()
		{
			FinancialSection financialSection = OutputToTest as FinancialSection;
			financialSection.Date = new ZDateTime(2004, 11, 10);
			financialSection.fCurrentAccountingYear = "2001";
		}

		protected override IFileOutput GetOutputToTest()
		{
			return new FinancialSection(Factory);
		}

		protected override string SetExpectedResult()
		{
			return @$"[帐套]
帐套=1,1
帐套名称={BrandingFactory.Instance.ProductName}
单位名称=上海*****公司
会计年度=2004
行业=运输代理服务
单位组织机构代码=12453687521
";
		}

		protected override void SetUp()
		{
			base.SetUp();
			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper(Factory);
			periodTestHelper.SetupSinglePeriod(999999, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 12, 31));
			OriginalBusinessReg = GlbCompany.CurrentCompany.GC_BusinessRegNo2;
			OriginalBusinessName = GlbCompany.CurrentCompany.GC_Name;
			GlbCompany.CurrentCompany.GC_BusinessRegNo = "12453687521";
			GlbCompany.CurrentCompany.GC_Name = "上海*****公司";
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_BusinessRegNo2 = OriginalBusinessReg;
			GlbCompany.CurrentCompany.GC_Name = OriginalBusinessName;
		}

		ZString OriginalBusinessReg;
		ZString OriginalBusinessName;
	}
}

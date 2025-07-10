using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public class BookKeepingSectionTest : FileOutputTest
	{
		protected override void SetValuesBeforeAssert()
		{
		}

		protected override IFileOutput GetOutputToTest()
		{
			APInvoice testInvoice = Factory.New(typeof(APInvoice)) as APInvoice;
			testInvoice.AH_PostDate = new ZDateTime(2002, 7, 1);
			return new BookKeepingSection(new ZDateTime(2004, 2, 1), Factory);
		}

		protected override string SetExpectedResult()
		{
			return @"[帐务]
软件公司=Eagle Datamation International Pty. Ltd.
软件版本=1.1
会计期数=2
启用日期=2002-07-01
启用会计期=2004-01-01
当前会计期=2004-02-01
";
		}

		public void TestCommencement()
		{
			APInvoice testInvoice = Factory.New(typeof(APInvoice)) as APInvoice;
			testInvoice.AH_PostDate = new ZDateTime(2005, 3, 12);
			BookKeepingSection testSection = new BookKeepingSection(new ZDateTime(2004, 2, 15), Factory);
			testSection.Define_ForTestOnly();
			testSection.SetValue_ForTestOnly();
			AssertEquals("2005-03-12", testSection.CommencementDateLine.Value);
		}

		protected override void SetUp()
		{
			base.SetUp();
			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper(Factory);
			periodTestHelper.SetupSinglePeriod(200401, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31));
			periodTestHelper.SetupSinglePeriod(200402, new ZDateTime(2004, 2, 1), new ZDateTime(2004, 2, 29));
		}
	}
}
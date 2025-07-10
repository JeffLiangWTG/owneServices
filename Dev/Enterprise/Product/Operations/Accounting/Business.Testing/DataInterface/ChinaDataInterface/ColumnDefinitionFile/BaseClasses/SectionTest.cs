using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public class SectionTest : TestCaseWithFactory
	{
		public class ConcreteLine : Section
		{
			public ConcreteLine(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override void Define()
			{
			}

			protected override void SetDefault()
			{
			}

			protected override void SetValue()
			{
			}

			protected override void AddLines()
			{
			}
		}

		public void TestCurrentAccountingYear()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(200411, new ZDateTime(2004, 5, 1), new ZDateTime(2004, 5, 31));
			testHelper.SetupSinglePeriod(200501, new ZDateTime(2004, 7, 1), new ZDateTime(2004, 7, 31));
			testHelper.SetupSinglePeriod(200507, new ZDateTime(2005, 1, 1), new ZDateTime(2005, 1, 31));
			ConcreteLine testSection = new ConcreteLine(Factory);
			AssertEquals("2004", testSection.GetAccountingYear(new ZDateTime(2004, 5, 22)));
			AssertEquals("2004", testSection.GetAccountingYear(new ZDateTime(2004, 7, 22)));
			AssertEquals("2005", testSection.GetAccountingYear(new ZDateTime(2005, 1, 22)));
			AssertEquals("0", testSection.GetAccountingYear(new ZDateTime(1970, 1, 22)));
		}
	}
}
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(Report))]
	public class ReportTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClassProperties()
		{
			AssertEquals("T208", Report.LocID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new Report();
		}
	}

	[TestedType(typeof(ReportCollection))]
	public class ReportCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReportCollection>
	{
		public void TestAddElementsAndClassProperties()
		{
			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31));
			Factory.Save();
			ReportCollection collection = new ReportCollection(Factory, 200603);
			AssertEquals(4, collection.Count);
			Report report = collection[0];
			AssertEquals(report.ReportNumber, "1");
			AssertEquals(report.ReportName, "资产负债表");
			AssertEquals(report.ReportDate, ZDateTime.Today.ToString("yyyyMMdd"));
			AssertEquals(report.ReportPeriod, "200603");
			AssertEquals(report.CurrencyUnit, "元");
			report = collection[1];
			AssertEquals(report.ReportNumber, "201");
			AssertEquals(report.ReportName, "损益表");
			AssertEquals(report.ReportDate, ZDateTime.Today.ToString("yyyyMMdd"));
			AssertEquals(report.ReportPeriod, "200603");
			AssertEquals(report.CurrencyUnit, "元");
			report = collection[2];
			AssertEquals(report.ReportNumber, "3");
			AssertEquals(report.ReportName, "现金流量表");
			AssertEquals(report.ReportDate, ZDateTime.Today.ToString("yyyyMMdd"));
			AssertEquals(report.ReportPeriod, "200603");
			AssertEquals(report.CurrencyUnit, "元");
			report = collection[3];
			AssertEquals(report.ReportNumber, "7");
			AssertEquals(report.ReportName, "所有者权益变动表");
			AssertEquals(report.ReportDate, ZDateTime.Today.ToString("yyyyMMdd"));
			AssertEquals(report.ReportPeriod, "200603");
			AssertEquals(report.CurrencyUnit, "元");
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = countryCode;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new Report();
		}

		protected override ReportCollection GetCollectionToTest()
		{
			return new ReportCollection(Factory, 0);
		}
	}
}

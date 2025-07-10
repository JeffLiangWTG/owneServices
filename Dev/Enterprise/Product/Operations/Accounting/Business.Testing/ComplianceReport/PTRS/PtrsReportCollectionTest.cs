using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ComplianceReport.PTRS;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.PTRS
{
	[TestedType(typeof(PtrsReportCollection))]
	public class PtrsReportCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var report = Factory.NewWithValidTestData<PtrsReport>();
			var result = new PtrsReportCollection(Factory, report.PK);
			Factory.Save();
			return result;
		}
	}
}

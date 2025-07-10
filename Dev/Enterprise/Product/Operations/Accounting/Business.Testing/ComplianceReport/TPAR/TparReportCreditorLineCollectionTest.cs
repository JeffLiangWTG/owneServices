using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ComplianceReport.TPAR;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.TPAR
{
	[TestedType(typeof(TparReportCreditorLineCollection))]
	public class TparReportCreditorLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var tparReport = Factory.New<TparReport>();
			return new TparReportCreditorLineCollection(tparReport);
		}
	}
}

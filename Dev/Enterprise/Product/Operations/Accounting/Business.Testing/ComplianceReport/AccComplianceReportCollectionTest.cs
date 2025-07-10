
namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(AccComplianceReportCollection))]
	public class AccComplianceReportCollectionTest : ActiveBusinessObjectCollectionTestCase<AccComplianceReportCollection>
	{
		protected override AccComplianceReportCollection GetCollectionToTest()
		{
			return new AccComplianceReportCollection(Factory);
		}
	}
}


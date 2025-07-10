using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC.Testing
{
	[TestedType(typeof(MTDSubmissionDataRow))]
	public class MTDSubmissionDataRowTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			MTDSubmissionDataColumns columns = new MTDSubmissionDataColumns(Factory, Factory.New<AccComplianceReport>());
			return new MTDSubmissionDataRow(Factory, 1, "Test", columns);
		}
	}
}

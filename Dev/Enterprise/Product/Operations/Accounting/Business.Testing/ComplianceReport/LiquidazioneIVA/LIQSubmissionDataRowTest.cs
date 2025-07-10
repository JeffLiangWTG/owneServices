using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA.Testing
{
	[TestedType(typeof(LIQSubmissionDataRow))]
	class LIQSubmissionDataRowTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			LIQSubmissionDataColumns columns = new LIQSubmissionDataColumns(Factory, Factory.New<AccComplianceReport>());
			return new LIQSubmissionDataRow(Factory, RowType.TotalVatBaseReceivables, "Test", columns);
		}
	}
}

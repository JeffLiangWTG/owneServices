using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.Testing
{
	[TestedType(typeof(ConsolidationBatchExportRow))]
	public class ConsolidationBatchExportRowTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ConsolidationBatchExportRow(Factory, new ConsolidationBatchDetailsRow());
		}
	}
}

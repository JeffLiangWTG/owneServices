using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.Testing
{
	[TestedType(typeof(ConsolidationBatchExportRowCollection))]
	public class ConsolidationBatchExportRowCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ConsolidationBatchExportRowCollection>
	{
		protected override ConsolidationBatchExportRowCollection GetCollectionToTest()
		{
			return new ConsolidationBatchExportRowCollection(new BusinessObjectFactory());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ConsolidationBatchExportRow(Factory, new ConsolidationBatchDetailsRow());
		}
	}
}

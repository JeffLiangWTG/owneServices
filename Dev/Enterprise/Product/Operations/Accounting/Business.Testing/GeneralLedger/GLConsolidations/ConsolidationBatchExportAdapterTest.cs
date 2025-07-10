using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.Testing
{
	[TestedType(typeof(ConsolidationBatchExportAdapter))]
	public class ConsolidationBatchExportAdapterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAddPropertiesOnlyAddsProperties()
		{
			var rowForExport = new ConsolidationBatchExportRow(Factory, new ConsolidationBatchDetailsRow());
			var adapter = new ConsolidationBatchExportAdapter(Factory, ZGuid.Empty, false);
			adapter.RowsForExport.Add(rowForExport);
			var infos = adapter.GetMultiTypeCollectionInfo();
			var properties = infos.RowTypes.FirstOrDefault().Properties;
			Assert("Property should have header text and mapping", properties.All(x => !string.IsNullOrEmpty(x.HeaderText) && !string.IsNullOrEmpty(x.MappingName)));
		}
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ConsolidationBatchExportAdapter(Factory, ZGuid.Empty, false);
		}
	}
}

using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	[TestedType(typeof(CASSAdjustmentFileDataExporter))]
	public class CASSAdjustmentFileDataExporterTest : FlatFileDataExporterTestCase
	{
		public override FlatFileDataExporter GetDataExporter()
		{
			return new CASSAdjustmentFileDataExporter();
		}

		public override IBusinessObjectCollection GetPopulatedCollectionToSaveAndExport()
		{
			var billingCollection = new CASSBillingCollectionForTest();
			billingCollection.Add(CASSAdjustmentFileTestHelper.GetFullyPopulatedCASS(Factory));
			return billingCollection;
		}
	}

	public class CASSBillingCollectionForTest : NonPersistentBusinessObjectCollection<CASSBilling>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CASSBilling(new BusinessObjectFactory());
		}
	}
}

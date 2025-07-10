using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Accounting.Export.Business;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	public class KoreaSouthTransactionBatchToGEIConverterTest : GlobalTransactionBatchToGEIConverterTest
	{
		public void TestGetBatchExporter()
		{
			var exporter = new StubKoreaSouthTransactionBatchToGEIConverter().GetBatchExporter_ForTestOnly(null);
			AssertEquals(typeof(KoreaSouthTransactionBatchExporter), exporter.GetType());
		}

		public void TestGEIBuilderValidationType()
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, TestObjectCreator.NonCurrentCompany);

			var converter = new StubKoreaSouthTransactionBatchToGEIConverter();
			var (_, validationErrors, validationWarnings) = converter.GetGEIBuilder_ForTestOnly(batch).Create();

			AssertEquals(typeof(LoggerWithGroupKey), validationErrors.GetType());
			AssertEquals(typeof(LoggerWithGroupKey), validationWarnings.GetType());
		}

		TestObjectCreator TestObjectCreator
			=> testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));

		TestObjectCreator testObjectCreator;

		class StubKoreaSouthTransactionBatchToGEIConverter : KoreaSouthTransactionBatchToGEIConverter
		{
			public StubKoreaSouthTransactionBatchToGEIConverter() : base(new KoreaSouthEInvoicingObjectFactory())
			{
			}

			public TransactionBatchExporter GetBatchExporter_ForTestOnly(BatchExportDataAccess dataAccess) => GetBatchExporter(dataAccess);

			public IGlobalElectronicInvoiceBuilder GetGEIBuilder_ForTestOnly(AccEInvoicingBatch batch) {
				PerformBeforeConvert(batch);
				return GetGEIBuilder(batch.AIB_BatchNumber.ToString());
			}
		}
	}
}

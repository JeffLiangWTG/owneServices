using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	sealed class KoreaSouthTransactionBatchExporterTest : EInvoicingBatchTransactionExporterTest
	{
		[TestDate(2018, 10, 23, 10, 37, 0)]
		public void TestExportTransactionBatchWithTransactionPK()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				var transactionCreator = new TransactionCreator();

				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code, departmentCode: "FIP");
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
				Factory.Save();

				var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AP001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK));
				arInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
				Factory.Save();

				AssertEquals("PreCondition", "AP001", arInvoice.AH_TransactionNum);
				AssertNotNull(arInvoice.Lines[0].AL_JH);

				var invoicingBatch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				LinkTransactionPivot(Factory, arInvoice.PK, invoicingBatch.PK, GlbCompany.CurrentCompany, Core.Constants.EInvoicingPivotState.Batched);
				Factory.Save();

				var exportor = CreateExporter();
				var transactionBatch = exportor.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace);

				AssertEquals(1, transactionBatch.TransactionCollection.Count);
				AssertEquals(arInvoice.PK.ToString(), transactionBatch.TransactionCollection[0].ComplianceSubType);
			}
		}

		protected override TransactionBatchExporter CreateExporter(PopulateOptionalXUTFieldsSetting setting = null)
		{
			return new KoreaSouthTransactionBatchExporter(DataAccess, optionalXUTFieldsSetting: setting);
		}

		protected override void CleanupTransactionBatch(TransactionBatch batch)
		{
			batch.TransactionCollection.ForEach(x => x.ComplianceSubType = null);
		}
	}
}

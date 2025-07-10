using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.CountryFactory;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.DominicanRepublic.Testing
{
	public class DominicanRepublicEInvoicingAdditionalDataItemsProviderTest : TestCaseWithFactory
	{
		#region FechaVencimientoSecuencia

		const string FechaVencimientoKey = "FechaVencimientoSecuencia";

		public void TestInvoiceFechaVencimientoSecuencia_With_ComplianceSequence_Null()
		{
			var batch = CreateInvoicePivotAndBatch(null);

			var additionalDataItems = CreateAdditionalDataItems(batch);

			AssertCollectionNotContains("FechaVencimientoSecuencia must not exist", FechaVencimientoKey, additionalDataItems);
		}

		public void TestInvoiceFechaVencimientoSecuencia_With_XD_ExpiryDate_Empty()
		{
			var complianceSequence = Factory.New<AccComplianceSequence>();
			complianceSequence.XD_ExpiryDate = new ZDateTime("");

			var batch = CreateInvoicePivotAndBatch(complianceSequence);

			var additionalDataItems = CreateAdditionalDataItems(batch);

			AssertCollectionNotContains("FechaVencimientoSecuencia must not exist", FechaVencimientoKey, additionalDataItems);
		}

		public void TestInvoiceFechaVencimientoSecuencia_With_XD_ExpiryDate_InValid()
		{
			var complianceSequence = Factory.New<AccComplianceSequence>();
			complianceSequence.XD_ExpiryDate = new ZDateTime(DateTime.MinValue);

			var batch = CreateInvoicePivotAndBatch(complianceSequence);

			var additionalDataItems = CreateAdditionalDataItems(batch);

			AssertCollectionNotContains("FechaVencimientoSecuencia must not exist", FechaVencimientoKey, additionalDataItems);
		}

		public void TestInvoiceFechaVencimientoSecuencia_With_XD_ExpiryDate_Valid()
		{
			var complianceSequence = Factory.New<AccComplianceSequence>();
			complianceSequence.XD_ExpiryDate = new ZDateTime("25-03-2024 00:01:01");

			var batch = CreateInvoicePivotAndBatch(complianceSequence);

			var additionalDataItems = CreateAdditionalDataItems(batch);

			AssertCollectionContains(additionalDataItems, x => x.Key == FechaVencimientoKey && x.Value == "25-03-2024");
		}

		#endregion FechaVencimientoSecuencia

		#region Implementation

		AccEInvoicingBatch CreateInvoicePivotAndBatch(AccComplianceSequence accComplianceSequence)
		{
			var batch = Factory.New<AccEInvoicingBatch>();

			var invoice = Factory.New<ARInvoice>();

			if (accComplianceSequence != null)
			{
				invoice.AH_XD_ComplianceBook = accComplianceSequence.PK;
			}

			var pivot = batch.TransactionPivots.AddNew();
			pivot.AIP_ParentID = invoice.PK;

			return batch;
		}

		List<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem> CreateAdditionalDataItems(AccEInvoicingBatch batch)
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = Factory.New<GlbCompany>().PK;

			var additionalDataItemsProvider = new DominicanRepublicEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItems = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new DominicanRepublicEInvoicingObjectFactory(), new Logger());

			return additionalDataItems.ToList<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem>();
		}

		#endregion
	}
}

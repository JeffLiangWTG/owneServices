using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.APAutomation.Testing
{
	internal class APReconciliationNodeProviderTest : TestCaseWithFactory
	{
		public void TestGetAccrualSourceForInvalidJobType()
		{
			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			var invalidJobTypes = new List<string> { string.Empty, null };
			AssertExceptionThrown(typeof(ArgumentException), () => APReconciliationNodeProvider.GetAccrualSourceType(invalidJobTypes[0]));
			AssertExceptionThrown(typeof(ArgumentNullException), () => APReconciliationNodeProvider.GetAccrualSourceType(invalidJobTypes[1]));
		}

		public void TestGetAccrualSourceForConsolJobType()
		{
			var consolJobParentTableCodes = new List<string> { JobConsolSchema.Constants.Prefix,
				DtbBookingConsolidationSchema.Constants.Prefix,
				DtbBookingSchema.Constants.Prefix,
				DtbConsignmentRunSheetSchema.Constants.Prefix,
				DtbLinehaulManifestSchema.Constants.Prefix,
				JobCartageRunSheetSchema.Constants.Prefix,
				WhsItemReceiveTransportationUnitSchema.Constants.Prefix,
				WhsItemDispatchTransportationUnitSchema.Constants.Prefix,
				WhsItemDispatchLoadListSchema.Constants.Prefix };
			foreach (var parentTableCode in consolJobParentTableCodes)
			{
				var accrualSourceType = APReconciliationNodeProvider.GetAccrualSourceType(parentTableCode);
				AssertEquals(AccrualSourceTypes.Consol, accrualSourceType);
			}
		}

		public void TestGetAccrualSourceForNonConsolJobType()
		{
			var nonConsolParentTableCodes = new List<string> { JobShipmentSchema.Constants.Prefix,
				JobDeclarationSchema.Constants.Prefix,
				CYDTransportationUnitSchema.Constants.Prefix,
				CYDReleaseAdviceSchema.Constants.Prefix,
				CYDReceiveAdviceSchema.Constants.Prefix,
				WhsVASOrderSchema.Constants.Prefix,
				WhsAdHocServiceJobSchema.Constants.Prefix,
				WhsItemReceiveConsignmentSchema.Constants.Prefix,
				WhsStocktakeSchema.Constants.Prefix,
				WorkRequestSchema.Constants.Prefix,
				WorkProjectSchema.Constants.Prefix,
				WorkItemSchema.Constants.Prefix,
				WhsItemDispatchConsignmentSchema.Constants.Prefix,
				WhsDocketSchema.Constants.Prefix,
				JobContainerDetentionSchema.Constants.Prefix,
				JobVoyAccountSchema.Constants.Prefix,
				DtbConsignmentSchema.Constants.Prefix,
				DtbAgentBookingSchema.Constants.Prefix,
				JobMawbSchema.Constants.Prefix,
				JobCartageSchema.Constants.Prefix,
				JobContainerSchema.Constants.Prefix,
				JobStorageSchema.Constants.Prefix,
				ExportCustomsManifestLinesSchema.Constants.Prefix,
				JobSundryChargesSchema.Constants.Prefix,
				CusHAWBSchema.Constants.Prefix,
				CusPermitHeaderSchema.Constants.Prefix,
				CusMAWBSchema.Constants.Prefix,
				CusUnderbondSchema.Constants.Prefix,
				CusISFHeaderSchema.Constants.Prefix,
				CusInBondHeaderSchema.Constants.Prefix,
				CusCAeMHMasterSchema.Constants.Prefix,
				CarrierShipmentHeaderSchema.Constants.Prefix
			};
			foreach (var parentTableCode in nonConsolParentTableCodes)
			{
				var accrualSourceType = APReconciliationNodeProvider.GetAccrualSourceType(parentTableCode);
				AssertEquals(AccrualSourceTypes.Job, accrualSourceType);
			}
		}

		public void TestGetAPReconciliationNodeProviderForInvalidJobType()
		{
			var draftInvoice = Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			var job = Factory.NewWithValidTestData<AccDraftInvoiceJob>();
			job.AIJ_ParentTableCode = "XYZ";
			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), () => APReconciliationNodeProvider.GetAPReconciliationNode(job, draftInvoice));
		}

		public void TestGetAPReconciliationNodeProviderForConsolType()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = testObjectCreator.CreateShipment("S0001", forwardingConsol);
			var shipmentWithNoJob = testObjectCreator.CreateShipment("S0002", forwardingConsol);

			var draftInvoice = testObjectCreator.CreateDraftInvoice("ADI 0001", "ADIR 0001", testObjectCreator.Creditor1.PK, 110M, 0M, testObjectCreator.AUD.Code);
			var jobCluster1 = testObjectCreator.AddClusterToDraftTransaction(draftInvoice, 0M);
			var job = testObjectCreator.AddJobToTheCluster(jobCluster1, forwardingConsol);
			Factory.Save();

			var node = APReconciliationNodeProvider.GetAPReconciliationNode(job, draftInvoice);
			AssertNotNull(node);
			AssertEquals(typeof(ConsolCostRelatedAPReconciliationLineProvider), node.LineProvider.GetType());
		}

		public void TestGetAPReconciliationNodeProviderForNonConsolType()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var shipment = testObjectCreator.CreateShipment("S0001");
			var job = testObjectCreator.CreateJob(shipment);

			var draftInvoice = testObjectCreator.CreateDraftInvoice("ADI 0001", "ADIR 0001", testObjectCreator.Creditor1.PK, 110M, 0M, testObjectCreator.AUD.Code);
			var jobCluster1 = testObjectCreator.AddClusterToDraftTransaction(draftInvoice, 0M);
			var djob = testObjectCreator.AddJobToTheCluster(jobCluster1, shipment);
			Factory.Save();

			var node = APReconciliationNodeProvider.GetAPReconciliationNode(djob, draftInvoice);
			AssertNotNull(node);
			AssertEquals(typeof(JobChargeRelatedAPReconciliationLineProvider), node.LineProvider.GetType());
		}
	}
}

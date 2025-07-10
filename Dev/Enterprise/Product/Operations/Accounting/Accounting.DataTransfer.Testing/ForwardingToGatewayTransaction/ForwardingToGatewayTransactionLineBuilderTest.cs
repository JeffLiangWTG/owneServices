using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.Invoices.Testing;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class ForwardingToGatewayTransactionLineBuilderTest : TransactionLineBuilderTest
	{
		public void TestWhenChargeCodeIsNotGatewayClearInvoiceLineAL_JHBeforeSettingInvoicingJobToShipment()
		{
			var gatewayConsol = ObjectCreator.CreateGatewayConsol("HKHKG", "AUSYD", "C0000558", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var gatewayJob = ObjectCreator.CreateJob(gatewayConsol);
			var shipment = ObjectCreator.CreateShipment("S001", gatewayConsol);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var shipmentJob = newFactory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK).AddToFilter(JobHeaderSchema.JH_GC, Env.CurrentCompanyPK));
			AssertNull("Precondition : Shipment job should not exist.", shipmentJob);

			TxnLine.ChargeCode = ObjectCreator.FRT.AC_Code;
			TxnLine.OriginalShipmentJobNumber = shipment.JS_UniqueConsignRef;
			TxnLine.ConsolOrJobNo = gatewayConsol.JK_UniqueConsignRef;
			TxnLine.ConsolOrJobType = TxnLineConsolOrJobType.GCN;

			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			using (var shipmentJobInNewFactory = new JobHeader.Loader(shipmentInNewFactory).TryCreateWithMutex(GlbBranch.CurrentBranch))
			using (AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ObjectCreator.CC1.PK.ToString()))
			{
				Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");
			}

			AssertEquals(1, Invoice.Lines.Count);
			var invoiceLine = Invoice.Lines[0];
			var message = "When Charge Code is not gateway related, AL_JH should be set to empty, so that later when we try to load/create shipment job and encounter an error, AL_JH will remain empty and not show gateway job instead.";
			AssertNotEquals(message, gatewayJob.PK, invoiceLine.AL_JH);
			AssertEquals(message, ZGuid.Empty, invoiceLine.AL_JH);
		}

		public void TestWhenChargeCodeIsNotGatewayChargeCodeMappingIsNotPerformedAgainIfChargeCodeWasNotMappedUsingInterCompanyChargeCodeMapping()
		{
			var gatewayConsol = ObjectCreator.CreateGatewayConsol("HKHKG", "AUSYD", "C0000558", receivingGatewayCompany: GlbCompany.CurrentCompany);
			ObjectCreator.CreateJob(gatewayConsol);
			var shipment = ObjectCreator.CreateShipment("S001", gatewayConsol);
			TestObjectCreator.AddMatchingRuleForChargeCode(ObjectCreator.Creditor1, "CC1", ObjectCreator.FRT);
			Factory.Save();

			Invoice.AH_OH = ObjectCreator.Creditor1.PK;
			TxnLine.ChargeCode = "CC1";
			TxnLine.OriginalShipmentJobNumber = shipment.JS_UniqueConsignRef;
			TxnLine.ConsolOrJobNo = gatewayConsol.JK_UniqueConsignRef;
			TxnLine.ConsolOrJobType = TxnLineConsolOrJobType.GCN;

			var builder = new ForwardingToGatewayTransactionLineBuilderForTest(new NotificationManager(Notify), new TransactionBuilderConfig());
			AssertEquals("Precondition", 0, builder.ChargeCodeSetCounter);
			using (AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ObjectCreator.CC2.PK.ToString()))
			{
				builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");
			}
			AssertEquals("Charge code mapping should be done once only, because charge code was NOT mapped using intercompany charge code mapping.", 1, builder.ChargeCodeSetCounter);

			AssertEquals(1, Invoice.Lines.Count);
			var invoiceLine = Invoice.Lines[0];
			AssertEquals(shipment.Job.PK, invoiceLine.AL_JH);
			AssertEquals(ObjectCreator.FRT.PK, invoiceLine.AL_AC);
		}

		public void TestWhenChargeCodeIsNotGatewayChargeCodeMappingIsPerformedAgainIfChargeCodeWasMappedUsingInterCompanyChargeCodeMapping()
		{
			var gatewayConsol = ObjectCreator.CreateGatewayConsol("HKHKG", "AUSYD", "C0000558", receivingGatewayCompany: GlbCompany.CurrentCompany);
			ObjectCreator.CreateJob(gatewayConsol);
			var shipment = ObjectCreator.CreateShipment("S001", gatewayConsol);
			ObjectCreator.CreateJob(shipment, false, localClientOrg: ObjectCreator.LocalClient);
			var chargeCodeABC = ObjectCreator.CreateChargeCode("ABC", "ABC Charge Code", ChargeType.Margin, 100, ObjectCreator.GSTFREE1, ObjectCreator.WHTFREE1, ObjectCreator.NonCurrentCompany);
			var globalChargeCodeMap1 = ObjectCreator.CreateGlobalChargeCodeMapWithPivot("INT1", "Intercompany Charge Code 1", ZGuid.Empty, chargeCodeABC.PK, LedgerTypes.AccountsReceivable);
			ObjectCreator.CreateGlobalChargeCodeMapPivot(globalChargeCodeMap1, ObjectCreator.CC1.PK, LedgerTypes.AccountsPayable, ZGuid.Empty);
			ObjectCreator.CreateGlobalChargeCodeMapPivot(globalChargeCodeMap1, ObjectCreator.CC2.PK, LedgerTypes.AccountsPayable, ObjectCreator.LocalClient.PK);
			Factory.Save();

			Invoice.AH_OH = ObjectCreator.NonCurrentCompany.GC_OH_OrgProxy;
			TxnLine.ChargeCode = chargeCodeABC.AC_Code;
			TxnLine.OriginalShipmentJobNumber = shipment.JS_UniqueConsignRef;
			TxnLine.ConsolOrJobNo = gatewayConsol.JK_UniqueConsignRef;
			TxnLine.ConsolOrJobType = TxnLineConsolOrJobType.GCN;

			var builder = new ForwardingToGatewayTransactionLineBuilderForTest(new NotificationManager(Notify), new TransactionBuilderConfig());
			AssertEquals("Precondition", 0, builder.ChargeCodeSetCounter);
			using (AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ObjectCreator.CC3.PK.ToString()))
			{
				builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");
			}
			AssertEquals("Charge code mapping should be done twice, because charge code was mapped using intercompany charge code mapping.", 2, builder.ChargeCodeSetCounter);

			AssertEquals(1, Invoice.Lines.Count);
			var invoiceLine = Invoice.Lines[0];
			AssertEquals(shipment.Job.PK, invoiceLine.AL_JH);
			AssertEquals(ObjectCreator.CC2.PK, invoiceLine.AL_AC);
		}

		protected override void GetNewLineBuilder() => Builder = new ForwardingToGatewayTransactionLineBuilder(new NotificationManager(Notify), new TransactionBuilderConfig());

		class ForwardingToGatewayTransactionLineBuilderForTest : ForwardingToGatewayTransactionLineBuilder
		{
			public ForwardingToGatewayTransactionLineBuilderForTest(NotificationManager notifier, TransactionBuilderConfig config) : base(notifier, config)
			{
			}

			public ZInt ChargeCodeSetCounter;

			protected override void SetChargeCode(InvoicingLineBase invoiceLine, TxnLine xmlInvoiceLine, string errorContext)
			{
				ChargeCodeSetCounter++;
				base.SetChargeCode(invoiceLine, xmlInvoiceLine, errorContext);
			}
		}
	}
}

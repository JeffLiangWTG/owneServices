using System;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public abstract class JobInvoicePrintingFilterTest : NonPersistentBusinessObjectTestCase
	{
		public virtual void TestInvoiceListForGatewayBilling()
		{
			var consol = TestObjectCreator.CreateGatewayConsol("KRSEL", "AUSYD", "C1", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = TestObjectCreator.CreateShipment("S1", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var gatewayJob = TestObjectCreator.CreateJob(consol, false);
			var jobs = new Job[] { job };

			Type invoiceType = Ledger == LedgerTypes.AccountsReceivable ? typeof(ARInvoice) : typeof(APInvoice);
			string invoiceLineType = Ledger == LedgerTypes.AccountsReceivable ? TransactionLineTypes.Revenue : TransactionLineTypes.Cost;

			#region Freight Invoices (Job Invoice, Consol Invoice1, Consol Invoice2)

			InvoicingBase freightInvoice1 = TestObjectCreator.CreateInvoice(invoiceType, TestObjectCreator.AUD, 1m);
			freightInvoice1.AH_TransactionNum = "Inv1";
			InvoicingLineBase invoiceLine = TestObjectCreator.CreateInvoiceLine(invoiceLineType, freightInvoice1, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "desc", 100m);
			Charge charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
			charge.JR_OSCostAmt = 100m;
			charge.JR_AT_SellGSTRate = ZGuid.Empty;
			if (Ledger == LedgerTypes.AccountsReceivable)
			{
				charge.JR_AL_ARLine = invoiceLine.PK;
				charge.ARLine.AL_OSAmount = charge.ARLine.AL_LineAmount = charge.JR_OSSellAmt;
				charge.JR_AT_SellGSTRate = charge.ARLine.AL_AT;
			}
			else
			{
				charge.JR_AL_APLine = invoiceLine.PK;
				charge.APLine.AL_OSAmount = charge.APLine.AL_LineAmount = -charge.JR_OSCostAmt;
				charge.JR_AT_CostGSTRate = charge.APLine.AL_AT;
			}

			InvoicingBase freightInvoice2 = TestObjectCreator.CreateInvoice(invoiceType, TestObjectCreator.AUD, 1m);
			freightInvoice2.AH_TransactionNum = "Inv2";
			InvoicingLineBase invoiceLine2 = TestObjectCreator.CreateInvoiceLine(invoiceLineType, freightInvoice2, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "desc", 100m);
			Charge charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
			charge2.JR_OSCostAmt = 100m;
			charge2.JR_AT_SellGSTRate = ZGuid.Empty;
			if (Ledger == LedgerTypes.AccountsReceivable)
			{
				charge2.JR_AL_ARLine = invoiceLine2.PK;
				charge2.ARLine.AL_OSAmount = charge2.ARLine.AL_LineAmount = charge2.JR_OSSellAmt;
				charge2.JR_AT_SellGSTRate = charge2.ARLine.AL_AT;
			}
			else
			{
				charge2.JR_AL_APLine = invoiceLine2.PK;
				charge2.APLine.AL_OSAmount = charge2.APLine.AL_LineAmount = -charge2.JR_OSCostAmt;
				charge2.JR_AT_CostGSTRate = charge2.APLine.AL_AT;
			}

			InvoicingBase freightInvoice3 = TestObjectCreator.CreateInvoice(invoiceType, TestObjectCreator.AUD, 1m);
			freightInvoice3.AH_TransactionNum = "Inv3";
			InvoicingLineBase invoiceLine3 = TestObjectCreator.CreateInvoiceLine(invoiceLineType, freightInvoice3, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "desc", 100m);
			Charge charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
			charge3.JR_OSCostAmt = 100m;
			charge3.JR_AT_SellGSTRate = ZGuid.Empty;
			if (Ledger == LedgerTypes.AccountsReceivable)
			{
				charge3.JR_AL_ARLine = invoiceLine3.PK;
				charge3.ARLine.AL_OSAmount = charge3.ARLine.AL_LineAmount = charge3.JR_OSSellAmt;
				charge3.JR_AT_SellGSTRate = charge3.ARLine.AL_AT;
			}
			else
			{
				charge3.JR_AL_APLine = invoiceLine3.PK;
				charge3.APLine.AL_OSAmount = charge3.APLine.AL_LineAmount = -charge3.JR_OSCostAmt;
				charge3.JR_AT_CostGSTRate = charge3.APLine.AL_AT;
			}

			Factory.Save();

			#endregion

			#region Gateway invoice

			GlbDepartment gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));
			InvoicingBase invoiceFromGatewayJob;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
			{
				invoiceFromGatewayJob = TestObjectCreator.CreateInvoice(invoiceType, TestObjectCreator.AUD, 1m);
				invoiceFromGatewayJob.AH_TransactionNum = "Inv4";
				InvoicingLineBase invoiceLineFromGatewayJob = TestObjectCreator.CreateInvoiceLine(invoiceLineType, invoiceFromGatewayJob, gatewayJob, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "desc", 100m);
				Charge chargeFromGatewayJob = TestObjectCreator.CreateCharge(gatewayJob, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
				chargeFromGatewayJob.JR_OSCostAmt = 100m;
				chargeFromGatewayJob.JR_AT_SellGSTRate = ZGuid.Empty;
				if (Ledger == LedgerTypes.AccountsReceivable)
				{
					chargeFromGatewayJob.JR_AL_ARLine = invoiceLineFromGatewayJob.PK;
					chargeFromGatewayJob.ARLine.AL_OSAmount = chargeFromGatewayJob.ARLine.AL_LineAmount = chargeFromGatewayJob.JR_OSSellAmt;
					chargeFromGatewayJob.JR_AT_SellGSTRate = chargeFromGatewayJob.ARLine.AL_AT;
				}
				else
				{
					chargeFromGatewayJob.JR_AL_APLine = invoiceLineFromGatewayJob.PK;
					chargeFromGatewayJob.APLine.AL_OSAmount = chargeFromGatewayJob.APLine.AL_LineAmount = -chargeFromGatewayJob.JR_OSCostAmt;
					chargeFromGatewayJob.JR_AT_CostGSTRate = chargeFromGatewayJob.APLine.AL_AT;
				}
				Factory.Save();
			}

			#endregion

			freightInvoice1.AH_ConsolidatedInvoiceRef = "S1";
			freightInvoice2.AH_ConsolidatedInvoiceRef = "C1";
			freightInvoice3.AH_ConsolidatedInvoiceRef = "C1/A";
			invoiceFromGatewayJob.AH_ConsolidatedInvoiceRef = "C1GW";
			Factory.Save();

			JobInvoicePrintingFilter printingFilter = GetNewBusinessObject(consol, jobs);
			printingFilter.RefreshInvoiceList();
			AssertEquals("Not Gateway department", false, GlbDepartment.CurrentDepartment.IsGatewayDepartment);
			AssertEquals("Only Freight Transactions in list", 3, printingFilter.Transactions.Count);
			AssertEquals("Only Freight Transactions in list", true, printingFilter.Transactions.Contains(freightInvoice1));
			AssertEquals("Only Freight Transactions in list", true, printingFilter.Transactions.Contains(freightInvoice2));
			AssertEquals("Only Freight Transactions in list", true, printingFilter.Transactions.Contains(freightInvoice3));

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
			{
				printingFilter = GetNewBusinessObject(consol, jobs);
				printingFilter.RefreshInvoiceList();
				AssertEquals("Gateway department", true, GlbDepartment.CurrentDepartment.IsGatewayDepartment);
				AssertEquals("Should be transactions in list", 3, printingFilter.Transactions.Count);
				AssertEquals("Only Freight Transactions in list", true, printingFilter.Transactions.Contains(freightInvoice1));
				AssertEquals("Only Freight Transactions in list", true, printingFilter.Transactions.Contains(freightInvoice2));
				AssertEquals("Only Freight Transactions in list", true, printingFilter.Transactions.Contains(freightInvoice3));

				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
				var orgAppointedAgentPorts1 = Factory.New<OrgAppointedAgentPorts>();
				orgAppointedAgentPorts1.O5_PortOrCountry = "AUBNE";
				consol.SendingForwarder.AppointedGatewayAgentPorts.Add(orgAppointedAgentPorts1);
				AssertEquals(true, consol.IsGateway());

				printingFilter = GetNewBusinessObject(consol, gatewayJob);
				printingFilter.RefreshInvoiceList();
				AssertEquals("Should be transactions in list", 1, printingFilter.Transactions.Count);
				AssertEquals("Only Gateway Transactions in list", true, printingFilter.Transactions.Contains(invoiceFromGatewayJob));
			}
		}

		public virtual void TestUseReadOnlyFactory()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			JobInvoicePrintingFilter printingFilter = GetNewBusinessObject(shipment, Factory.NewJobForTesting<Job>());
			AssertNotEquals(printingFilter.Factory, printingFilter.HostBusinessObject.Factory);
		}

		public void TestDontLoadAnyTransactionsWhenNoJobsExist()
		{
			BusinessObjectFactory dataFactory = new BusinessObjectFactory();
			ForwardingConsol consol = dataFactory.New<ForwardingConsol>();
			JobInvoicePrintingFilter printingFilter = GetNewBusinessObject(Shipment1, Factory.NewJobForTesting<Job>());

			ARInvoice invoice = dataFactory.New<ARInvoice>();
			((ARInvoiceLine)invoice.Lines.AddNew()).AL_AG = TestObjectCreator.GLHeader1.PK;

			dataFactory.Save();

			printingFilter.RefreshInvoiceList();

			AssertEquals("Shouldn't be any transactions in list", 0, printingFilter.Transactions.Count);
		}

		public virtual void TestDontLoadAnyJobsWhenNoJobsArePassed()
		{
			BusinessObjectFactory dataFactory = new BusinessObjectFactory();
			ForwardingConsol consol = dataFactory.New<ForwardingConsol>();
			JobInvoicePrintingFilter printingFilter = GetNewBusinessObject(consol, Array.Empty<Job>());

			Job testJob = new TestObjectCreator(dataFactory).Job1;
			dataFactory.Save();

			printingFilter.RefreshJobNumbersCollection(Array.Empty<ZGuid>());

			AssertEquals("Shouldn't be any jobs in list", 0, printingFilter.JobNumbers.Count);
		}

		public virtual void TestResetInvoiceList()
		{
			Factory.Save();
			JobInvoicePrintingFilter printingFilter = GetNewBusinessObject(Shipment1, Header1);
			AssertEquals("Invoice Count", 7, printingFilter.Transactions.Count);

			printingFilter.Transactions.RemoveAll();
			AssertEquals("Invoice Count", 0, printingFilter.Transactions.Count);

			printingFilter.ResetInvoiceList();
			AssertEquals("Debtor", ZGuid.Empty, printingFilter.DebtorOrCreditor);
			AssertEquals("Job Number", ZGuid.Empty, printingFilter.JobNumber);
			AssertEquals("Transaction Type", ZString.Empty, printingFilter.TransactionType);
			AssertEquals("Invoice Count", 7, printingFilter.Transactions.Count);
		}

		public virtual void TestRefreshInvoiceListWithShipment()
		{
			Factory.Save();
			var printingFilter = GetNewBusinessObject(Shipment1, Header1);
			AssertEquals("Invoice Count", 7, printingFilter.Transactions.Count);

			AssertRefreshInvoiceList(printingFilter, Org1.PK, ZString.Empty, ZGuid.Empty, 5);
			AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.Invoice, ZGuid.Empty, 3);
			AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.CreditNote, ZGuid.Empty, 1);
			AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.AdjustmentNote, ZGuid.Empty, 1);

			AssertRefreshInvoiceList(printingFilter, Org2.PK, ZString.Empty, ZGuid.Empty, 2);
			AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.Invoice, ZGuid.Empty, 1);
			AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.CreditNote, ZGuid.Empty, 1);
			AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.AdjustmentNote, ZGuid.Empty, 0);

			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, ZString.Empty, ZGuid.Empty, 7);
			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.Invoice, ZGuid.Empty, 4);
			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.CreditNote, ZGuid.Empty, 2);
			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.AdjustmentNote, ZGuid.Empty, 1);
		}

		[TestDate(2015, 01, 01, 07, 30, 00)]
		public virtual void TestRefreshInvoiceListWithDefaultPostDateFilter()
		{
			Invoice1_1.AH_PostDate = ZDateTime.Now.AddHours(2);
			Invoice1_3.AH_PostDate = ZDateTime.Now.AddHours(2);
			Invoice1_4.AH_PostDate = ZDateTime.Now.AddHours(2);
			Invoice1_5.AH_PostDate = ZDateTime.Now.AddHours(2);
			Factory.Save();

			JobInvoicePrintingFilter printingFilter = GetNewBusinessObject(Shipment1, Header1);
			AssertEquals("Should find all invoices with Header1", 7, printingFilter.Transactions.Count);
		}

		public virtual void TestRefreshInvoiceWithLines()
		{
			Factory.Save();

			TestObjectCreator cR = new TestObjectCreator(Factory);

			Shipment1 = CreateShipment();
			Consol.Shipments.Add(Shipment1);
			JobHeader testJob1 = CreateJobHeader(Shipment1);
			Shipment1.JS_UniqueConsignRef = "1";

			AccChargeCode testChargeCode1 = cR.CreateChargeCode("TST", "Test Charge Code", "REV", 0m, null, null, "ALL");
			AccChargeCode testChargeCode2 = cR.CreateChargeCode("TST_2", "Test Charge Code", "REV", 0m, null, null, "ALL");

			InvoicingLineBase testLine1 = (InvoicingLineBase)Invoice1_4.Lines.AddNew();
			InvoicingLineBase testLine2 = (InvoicingLineBase)Invoice1_4.Lines.AddNew();

			testLine1.AL_JH = testJob1.PK;
			testLine2.AL_JH = testJob1.PK;

			testLine1.AL_AC = testChargeCode1.PK;
			testLine2.AL_AC = testChargeCode2.PK;

			testLine1.AL_AH = Invoice1_4.PK;
			testLine2.AL_AH = Invoice1_4.PK;

			TestObjectCreator.CreateJobCharge(testLine1, testJob1, testChargeCode1, TestObjectCreator.AUD);
			TestObjectCreator.CreateJobCharge(testLine2, testJob1, testChargeCode1, TestObjectCreator.AUD);

			Factory.Save();

			JobInvoicePrintingFilter printingFilter = GetNewBusinessObject(Consol, new Job[] { Header1 });
			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.Invoice, testJob1.PK, 1);
		}

		public virtual void TestIsFreightConsol()
		{
			JobInvoicePrintingFilter printingFilter = GetNewBusinessObject(Shipment1, Header1);
			AssertEquals("Is Freight Consol", false, printingFilter.IsFreightConsol);

			printingFilter = GetNewBusinessObject(Consol, new Job[] { Header1, Header2 });
			AssertEquals("Is Freight Consol", true, printingFilter.IsFreightConsol);

			printingFilter = GetNewBusinessObject(Consol, new Job[] { Header1 });
			AssertEquals("Is Freight Consol", true, printingFilter.IsFreightConsol);

			GlbDepartment gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
			{
				printingFilter = GetNewBusinessObject(Shipment1, Header1);
				AssertEquals("Is Freight Consol", false, printingFilter.IsFreightConsol);

				printingFilter = GetNewBusinessObject(Consol, new Job[] { Header1, Header2 });
				AssertEquals("Is Freight Consol", true, printingFilter.IsFreightConsol);

				printingFilter = GetNewBusinessObject(Consol, Header1);
				AssertEquals("Is Freight Consol", true, printingFilter.IsFreightConsol);
			}
		}

		public virtual void TestRefreshInvoiceListWithConsol()
		{
			Consol.Shipments.Add(Shipment1);
			Consol.Shipments.Add(Shipment2);
			Factory.Save();
			JobInvoicePrintingFilter printingFilter = GetNewBusinessObject(Consol, new Job[] { Header1, Header2 });
			AssertEquals("Invoice Count", 13, printingFilter.Transactions.Count);

			#region Header 1

			AssertRefreshInvoiceList(printingFilter, Org1.PK, ZString.Empty, Header1.PK, 5);
			AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.Invoice, Header1.PK, 3);
			AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.CreditNote, Header1.PK, 1);

			AssertRefreshInvoiceList(printingFilter, Org2.PK, ZString.Empty, Header1.PK, 2);
			AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.Invoice, Header1.PK, 1);
			AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.CreditNote, Header1.PK, 1);

			AssertRefreshInvoiceList(printingFilter, Org3.PK, ZString.Empty, Header1.PK, 0);
			AssertRefreshInvoiceList(printingFilter, Org3.PK, TransactionTypes.Invoice, Header1.PK, 0);
			AssertRefreshInvoiceList(printingFilter, Org3.PK, TransactionTypes.CreditNote, Header1.PK, 0);

			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, ZString.Empty, Header1.PK, 7);
			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.Invoice, Header1.PK, 4);
			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.CreditNote, Header1.PK, 2);

			#endregion

			#region Header 2

			AssertRefreshInvoiceList(printingFilter, Org1.PK, ZString.Empty, Header2.PK, 2);
			AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.Invoice, Header2.PK, 1);
			AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.CreditNote, Header2.PK, 1);

			AssertRefreshInvoiceList(printingFilter, Org2.PK, ZString.Empty, Header2.PK, 0);
			AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.Invoice, Header2.PK, 0);
			AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.CreditNote, Header2.PK, 0);

			AssertRefreshInvoiceList(printingFilter, Org3.PK, ZString.Empty, Header2.PK, 4);
			AssertRefreshInvoiceList(printingFilter, Org3.PK, TransactionTypes.Invoice, Header2.PK, 3);
			AssertRefreshInvoiceList(printingFilter, Org3.PK, TransactionTypes.CreditNote, Header2.PK, 0);

			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, ZString.Empty, Header2.PK, 6);
			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.Invoice, Header2.PK, 4);
			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.CreditNote, Header2.PK, 1);

			#endregion

			#region All Headers

			AssertRefreshInvoiceList(printingFilter, Org1.PK, ZString.Empty, ZGuid.Empty, 7);
			AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.Invoice, ZGuid.Empty, 4);
			AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.CreditNote, ZGuid.Empty, 2);

			AssertRefreshInvoiceList(printingFilter, Org2.PK, ZString.Empty, ZGuid.Empty, 2);
			AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.Invoice, ZGuid.Empty, 1);
			AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.CreditNote, ZGuid.Empty, 1);

			AssertRefreshInvoiceList(printingFilter, Org3.PK, ZString.Empty, ZGuid.Empty, 4);
			AssertRefreshInvoiceList(printingFilter, Org3.PK, TransactionTypes.Invoice, Header2.PK, 3);
			AssertRefreshInvoiceList(printingFilter, Org3.PK, TransactionTypes.CreditNote, Header2.PK, 0);

			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, ZString.Empty, ZGuid.Empty, 13);
			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.Invoice, ZGuid.Empty, 8);
			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.CreditNote, ZGuid.Empty, 3);

			#endregion
		}

		public void TestLocalTransportJobInvoiceIsInCollection()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			Job shipmentJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			shipmentJob.JH_GB = GlbBranch.CurrentBranch.PK;
			shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;

			ShipmentPickupCartageType cartageType = new ShipmentPickupCartageType(shipment);
			CommonCartage localTransport = Factory.NewWithValidTestData<CommonCartage>();
			localTransport.JJ_ParentID = cartageType.CartageParent.CartageParentID;
			localTransport.JJ_ParentTableCode = cartageType.CartageParent.CartageParentTableCode;
			localTransport.JJ_ConsignmentID = "ABC0123";
			localTransport.JJ_GB = cartageType.CartageParent.BranchPK.IsValid ? cartageType.CartageParent.BranchPK : GlbBranch.CurrentBranch.PK;
			localTransport.JJ_E3_NKJobType = cartageType.CartageJobType;
			localTransport.JJ_RS_NKServiceLevel = cartageType.CartageParent.ServiceLevel;
			localTransport.JJ_OrderReferenceNumber = cartageType.CartageParent.OrderReferenceNumber.SubstringSafe(0, localTransport.JJ_OrderReferenceNumberInfo.MaxLength);
			localTransport.JJ_WaybillNumber = cartageType.CartageParent.WayBillNumber.SubstringSafe(0, JobCartageSchema.JJ_WaybillNumber.MaxLength);
			localTransport.JJ_GoodsDescription = cartageType.CartageParent.GoodsDescription.SubstringSafe(0, JobCartageSchema.JJ_GoodsDescription.MaxLength);
			Job localTransportJob = new Job.Loader(localTransport).TryLoadOrCreateWithoutMutexForTestOnly();
			localTransport.Job.JH_JH_ParentJob = shipmentJob.PK;
			localTransportJob.JH_GB = GlbBranch.CurrentBranch.PK;
			localTransportJob.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Type invoiceType = Ledger == LedgerTypes.AccountsReceivable ? typeof(ARInvoice) : typeof(APInvoice);
			InvoicingBase invoice = TestObjectCreator.CreateInvoice(invoiceType, "00001000", GlbCompany.CurrentCompany.LocalCurrency, 1.0m);
			AssertNotNull("Should have created invoice", invoice);

			AccChargeCode chargeCode = TestObjectCreator.CC1;
			InvoicingLineBase invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine.AL_JH = localTransport.Job.PK;
			invoiceLine.AL_AC = chargeCode.PK;
			invoiceLine.AL_GB = GlbBranch.CurrentBranch.PK;
			invoiceLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			invoiceLine.AL_OSExTaxAmount = 100.00m;

			OrgHeader aALSHI = TestObjectCreator.AALSHI;
			OrgHeader aBIGAS = TestObjectCreator.ABIGAS;

			Charge charge = localTransportJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_OH_CostAccount = aALSHI.PK;
			charge.JR_AT_CostGSTRate = ZGuid.Empty;
			charge.JR_OSCostAmt = 100m;
			charge.JR_OH_SellAccount = aBIGAS.PK;
			charge.JR_AT_SellGSTRate = ZGuid.Empty;
			charge.JR_OSSellAmt = 100m;
			if (Ledger == LedgerTypes.AccountsReceivable)
			{
				charge.JR_AL_ARLine = invoiceLine.PK;
				charge.ARLine.AL_OSAmount = charge.ARLine.AL_LineAmount = charge.JR_OSSellAmt;
			}
			else
			{
				charge.JR_AL_APLine = invoiceLine.PK;
				charge.APLine.AL_OSAmount = charge.APLine.AL_LineAmount = -charge.JR_OSCostAmt;
			}

			Factory.Save();

			JobInvoicePrintingFilter filter = GetNewBusinessObject(shipment, shipmentJob);
			filter.RefreshInvoiceList();
			AssertEquals("Transactions should contain 1 invoice", 1, filter.Transactions.Count);
			AssertEquals("Transactions should contain localTransport Invoice", true, filter.Transactions.Contains(invoice.PK));
		}

		public virtual void TestFilteredCollectionsWithSecuritySettings()
		{
			GlbStaff testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_IsController = false;

			GlbCompany testCompany = Factory.NewWithValidTestData<GlbCompany>();
			var testBranch = TestObjectCreator.CreateBranch("TBR", "Test Branch", GlbCompany.CurrentCompany);
			Factory.Save();

			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment1.JS_UniqueConsignRef = "SHIPMENT1";
				Job shipmentJob1 = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
				shipmentJob1.JH_GB = GlbBranch.CurrentBranch.PK;
				shipmentJob1.JH_GE = GlbDepartment.CurrentDepartment.PK;
				Consol.Shipments.Add(shipment1);

				ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment2.JS_UniqueConsignRef = "SHIPMENT2";
				Job shipmentJob2 = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
				shipmentJob2.JH_GB = GlbBranch.CurrentBranch.PK;
				shipmentJob2.JH_GE = GlbDepartment.CurrentDepartment.PK;
				Consol.Shipments.Add(shipment2);

				AccChargeCode testChargeCode1 = TestObjectCreator.CreateChargeCode("TST", "Test Charge Code", "REV", 0m, null, null, "ALL");
				AccChargeCode testChargeCode2 = TestObjectCreator.CreateChargeCode("TST_2", "Test Charge Code", "REV", 0m, null, null, "ALL");

				InvoicingLineBase testLine1 = (InvoicingLineBase)Invoice1_4.Lines.AddNew();
				testLine1.AL_JH = shipmentJob1.PK;
				testLine1.AL_AC = testChargeCode1.PK;
				testLine1.AL_AH = Invoice1_4.PK;
				TestObjectCreator.CreateJobCharge(testLine1, shipmentJob1, testChargeCode1, TestObjectCreator.AUD);

				InvoicingLineBase testLine2 = (InvoicingLineBase)Invoice2_3.Lines.AddNew();
				testLine2.AL_JH = shipmentJob2.PK;
				testLine2.AL_AC = testChargeCode1.PK;
				testLine2.AL_AH = Invoice2_3.PK;
				TestObjectCreator.CreateJobCharge(testLine2, shipmentJob2, testChargeCode2, TestObjectCreator.AUD);

				Factory.Save();

				JobInvoicePrintingFilter printingFilter = GetNewBusinessObject(Consol, new Job[] { shipmentJob1, shipmentJob2 });

				BusinessObjectFactory securityFactory = new BusinessObjectFactory();

				SecurityCore security = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
				SecurityCore security2 = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, testBranch.PK.ToGuid(), Env.CurrentDepartment.PK);

				GlbSecurity loginSecurity = securityFactory.New<GlbSecurity>();
				loginSecurity.GU_GB = Env.CurrentBranch.PK;
				loginSecurity.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity.GU_GS = Env.CurrentUser.PK;
				loginSecurity.GU_SecurityRight = security.Login.Code;
				GlbSecurity loginSecurity2 = securityFactory.New<GlbSecurity>();
				loginSecurity2.GU_GB = testBranch.PK;
				loginSecurity2.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity2.GU_GS = Env.CurrentUser.PK;
				loginSecurity2.GU_SecurityRight = security2.Login.Code;

				GlbSecurity invoicingSecurity = securityFactory.New<GlbSecurity>();
				invoicingSecurity.GU_GB = Env.CurrentBranch.PK;
				invoicingSecurity.GU_GE = Env.CurrentDepartment.PK;
				invoicingSecurity.GU_GS = Env.CurrentUser.PK;
				invoicingSecurity.GU_SecurityRight = security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.AllowViewCosts).Code;

				loginSecurity.GU_SecurityItemIsAllowed = true;
				security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.AllowViewCosts).IsAllowed = true;
				security2.Login.IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = true;
				loginSecurity2.GU_SecurityItemIsAllowed = false;

				securityFactory.Save();

				Env.Security.ResetData(null, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid());

				AssertEquals("should show all invoices", 2, printingFilter.Transactions.Count);
				AssertEquals("should show all invoices", 2, printingFilter.FilteredTransactions.Count);

				loginSecurity.GU_SecurityItemIsAllowed = true;
				security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.AllowViewCosts).IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = false;
				securityFactory.Save();

				printingFilter.Transactions[0].Job.JH_GB = testBranch.PK;
				Factory.ClearCachedValue<bool>(("Login BRN:" + testBranch.GB_Code + " DEP:" + GlbDepartment.CurrentDepartment.GE_Code));
				Factory.Save();

				printingFilter.RefreshInvoiceList();
				AssertEquals("should show all invoices", 2, printingFilter.Transactions.Count);
				AssertEquals("should show the invoice with current branch", 1, printingFilter.FilteredTransactions.Count);
			}
		}

		[TestDate(2015, 01, 01, 07, 30, 00)]
		public virtual void TestExcludeReversed()
		{
			Invoice1_1.AH_PostDate = ZDateTime.Now.AddHours(2);
			Invoice1_3.AH_PostDate = ZDateTime.Now.AddHours(2);
			Invoice1_4.AH_PostDate = ZDateTime.Now.AddHours(2);
			Invoice1_5.AH_PostDate = ZDateTime.Now.AddHours(2);
			Factory.Save();

			Invoice1_4.AH_TransactionBelongsToGroup = Invoice1_1.PK;
			Invoice1_5.AH_TransactionBelongsToGroup = Invoice1_3.PK;
			new ReversingFactory().NewReversing(Invoice1_5).Reverse();
			Invoice1_5.ReverseInvoice.AH_TransactionNum = "REVERSE001";
			Factory.Save();

			JobInvoicePrintingFilter printingFilter = GetNewBusinessObject(Shipment1, Header1);
			printingFilter.RefreshInvoiceList();

			AssertEquals("Should find all invoices with Header1 plus reversal", 8, printingFilter.Transactions.Count);
		}

		#region Test ZQuery

		[SuspendCriticalValidation]
		public void TestIndexUsedForQueryOfJobInvoice()
		{
			SetDataForIndexTest();

			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);
			var shipment = factory.New<ForwardingShipment>();
			var jobHeader = new Job.Loader(factory, shipment).TryCreateWithoutMutexForTestOnly();
			var printingFilter = new JobARInvoicePrintingFilter(shipment, jobHeader, ZDateTime.Invalid, ZDateTime.Invalid);

			var querySql = printingFilter.GetQueryOfJobInvoice_ForTestOnly("AR");

			var invoice1 = testObjectCreator.CreateARInvoice<ARInvoice>("001", testObjectCreator.AUD, 1, testObjectCreator.ABIGAS);
			invoice1.AH_JH = jobHeader.PK;

			var invoice2 = testObjectCreator.CreateARInvoice<ARInvoice>("002", testObjectCreator.AUD, 1, testObjectCreator.ABIGAS);
			var invoice2_1 = testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.AUD, 1m, 100m);
			invoice2_1.AL_JH = jobHeader.PK;

			var childJob = testObjectCreator.CreateJob(testObjectCreator.LocalClient, 0M, testObjectCreator.Agent, 0m);
			childJob.JH_JH_ParentJob = jobHeader.PK;
			var invoice3 = testObjectCreator.CreateARInvoice<ARInvoice>("003", testObjectCreator.AUD, 1, testObjectCreator.ABIGAS);
			var invoice3_1 = testObjectCreator.CreateInvoiceLine(invoice3, testObjectCreator.AUD, 1m, 100m);
			invoice3_1.AL_JH = childJob.PK;

			var invoice4 = testObjectCreator.CreateARInvoice<ARInvoice>("004", testObjectCreator.AUD, 1, testObjectCreator.ABIGAS);
			var invoice4_1 = testObjectCreator.CreateInvoiceLine(invoice4, testObjectCreator.AUD, 1m, 100m);

			factory.Save();

			Db.Connection.ExecuteNonQuery($"UPDATE STATISTICS {AccTransactionHeaderSchema.Constants.TableName} WITH FULLSCAN");
			Db.Connection.ExecuteNonQuery($"UPDATE STATISTICS {JobHeaderSchema.Constants.TableName} WITH FULLSCAN");

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (Db.Connection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				factory.Load<AccTransactionHeader>(querySql);

				var queryPlan = Db.Connection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains("AccTransactionHeader"));
				var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());

				CombineAssertions(() =>
				{
					Assert("Non clustered index FK_RX__AH_JH must be used.", queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexName == "FK_RX__AH_JH"));
					Assert("Non clustered index FK_RX__AL_JH_AL_LineType must be used.", queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexName == "FK_RX__AL_JH_AL_LineType"));
					Assert("Non clustered index FK_RX__JH_JH_ParentJob must be used.", queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexName == "FK_RX__JH_JH_ParentJob"));
					Assert("There must have no Table Scan been used.", !queryPlanAnalyzer.TableScans.Any());
				});
			}
		}

		public virtual void TestIndexHintOnAH_JHIsAdded()
		{
			Factory.Save();
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (Db.Connection.TrackExecutedCommands())
			{
				var printingFilter = GetNewBusinessObject(Shipment1, Header1);
				printingFilter.RefreshInvoiceList();
				var executedQuery = Db.Connection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains("AccTransactionHeader")).Item1;
				AssertNotContains("Index hint is not added in base class", "FROM dbo.AccTransactionHeader WITH (FORCESEEK, INDEX(FK_RX__AH_JH, PK_UC__AH_PK))", executedQuery);
			}
		}

		public virtual void TestGetQueryForConsolWithoutJobNumber()
		{
			Assert("Not implemented", false);
		}

		public virtual void TestGetQueryWithJobNumber()
		{
			Assert("Not implemented", false);
		}

		protected void AssertQueryForJob(
			string expectedSql, string ledgerType, string transactionType, bool includePrinted,
			ZGuid debtorOrCreditor, ZDateTime from, ZDateTime to, bool isConsolJob = false)
		{
			var printingFilter = isConsolJob ?
				CreateInstanceForTest(Consol, Header1, from, to) :
				CreateInstanceForTest(Shipment1, Header1, from, to);
			printingFilter.TransactionType = transactionType;
			printingFilter.IncludePrinted = includePrinted;
			printingFilter.DebtorOrCreditor = debtorOrCreditor;
			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertEquals(
				expectedSql.Replace("\t", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty),
				printingFilter.GetQueryOfJobInvoice_ForTestOnly(ledgerType).LiteralTextSqlFormatted.Replace("\t", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty));
			}
		}

		protected virtual JobInvoicePrintingFilter CreateInstanceForTest(IBusiness hostBusinessObject, Job jobHeader, ZDateTime from, ZDateTime to)
		{
			throw new NotImplementedException();
		}

		protected void TestAPGetQueryForConsolJobWithoutJobNumberCore()
		{
			Consol.Shipments.Add(Shipment1);
			Consol.JK_UniqueConsignRef = "C001";
			Factory.Save();

			var companyPK = GlbCompany.CurrentCompany.PK;
			var fromDate = ZDateTime.Today.AddDays(-1);
			var fromDateString = fromDate.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
			var toDate = ZDateTime.Today;
			var toDateString = toDate.AddDays(1).Date.ToZDateTime().ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
			var ledger = "AP";
			var consolNumber = "C001";

			// Full params
			string expectedSql =
$@"(
	AH_GC = '{companyPK}' 
	AND
	(
		AH_PK IN 
		(
			SELECT AL_AH FROM dbo.AccTransactionLines WHERE AL_AH IS NOT NULL 
			AND
			(
				(
					AL_JH IN 
					(
						SELECT JH_PK FROM dbo.JobHeader WHERE JH_ParentID IN 
						(
							SELECT JS_PK FROM dbo.JobShipment WHERE JS_PK IN 
							(
								SELECT JN_JS FROM dbo.JobConShipLink WHERE JN_JK IN 
								(
									SELECT JK_PK FROM dbo.JobConsol WHERE JK_UniqueConsignRef = '{consolNumber}'
								)
							)
						)
						AND
						JH_GC = '{companyPK}' 
						AND
						JH_ParentTableCode = 'JS'
					)
				)
				OR
				(
					AL_JH IN 
					(
						SELECT JH_PK FROM dbo.JobHeader WHERE JH_JH_ParentJob IN 
						(
							SELECT JH_PK FROM dbo.JobHeader WHERE JH_ParentID IN 
							(
								SELECT JS_PK FROM dbo.JobShipment WHERE JS_PK IN 
								(
									SELECT JN_JS FROM dbo.JobConShipLink WHERE JN_JK IN 
									(
										SELECT JK_PK FROM dbo.JobConsol WHERE JK_UniqueConsignRef = '{consolNumber}'
									)
								)
							)
							AND
							JH_GC = '{companyPK}' 
							AND
							JH_ParentTableCode = 'JS'
						)
					)
				)
			)
			AND
			AL_AH is not NULL
		)
	)
)
AND
(
	AH_Ledger = '{ledger}' 
	AND
	(
		AH_TransactionType in 
		(
			'ADJ', 'CRD', 'INV'
		)
	)
)
AND
AH_TransactionType = 'INV' 
AND
AH_InvoicePrinted = 0 
AND
AH_OH = '{Org1.PK}' 
AND
AH_PostDate >= '{fromDateString}' 
AND
AH_PostDate < '{toDateString}'
";

			AssertQueryForJob(expectedSql, ledger, "INV", false, Org1.PK, fromDate, toDate, true);

			// Basic params
			expectedSql =
$@"(
	AH_GC = '{companyPK}' 
	AND
	(
		AH_PK IN 
		(
			SELECT AL_AH FROM dbo.AccTransactionLines WHERE AL_AH IS NOT NULL 
			AND
			(
				(
					AL_JH IN 
					(
						SELECT JH_PK FROM dbo.JobHeader WHERE JH_ParentID IN 
						(
							SELECT JS_PK FROM dbo.JobShipment WHERE JS_PK IN 
							(
								SELECT JN_JS FROM dbo.JobConShipLink WHERE JN_JK IN 
								(
									SELECT JK_PK FROM dbo.JobConsol WHERE JK_UniqueConsignRef = '{consolNumber}'
								)
							)
						)
						AND
						JH_GC = '{companyPK}' 
						AND
						JH_ParentTableCode = 'JS'
					)
				)
				OR
				(
					AL_JH IN 
					(
						SELECT JH_PK FROM dbo.JobHeader WHERE JH_JH_ParentJob IN 
						(
							SELECT JH_PK FROM dbo.JobHeader WHERE JH_ParentID IN 
							(
								SELECT JS_PK FROM dbo.JobShipment WHERE JS_PK IN 
								(
									SELECT JN_JS FROM dbo.JobConShipLink WHERE JN_JK IN 
									(
										SELECT JK_PK FROM dbo.JobConsol WHERE JK_UniqueConsignRef = '{consolNumber}'
									)
								)
							)
							AND
							JH_GC = '{companyPK}' 
							AND
							JH_ParentTableCode = 'JS'
						)
					)
				)
			)
			AND
			AL_AH is not NULL
		)
	)
)
AND
(
	AH_Ledger = '{ledger}' 
	AND
	(
		AH_TransactionType in 
		(
		'ADJ', 'CRD', 'INV'
		)
	)
)
";

			AssertQueryForJob(expectedSql, ledger, string.Empty, true, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, true);
		}

		protected void TestARGetQueryForConsolJobWithoutJobNumberCore()
		{
			Consol.Shipments.Add(Shipment1);
			Consol.JK_UniqueConsignRef = "C001";
			Shipment1.JS_UniqueConsignRef = "1";
			Factory.Save();

			var companyPK = GlbCompany.CurrentCompany.PK;
			var fromDate = ZDateTime.Today.AddDays(-1);
			var fromDateString = fromDate.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
			var toDate = ZDateTime.Today;
			var toDateString = toDate.AddDays(1).Date.ToZDateTime().ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
			var ledger = "AR";
			var consolNumber = "C001";

			// Full params
			string expectedSql =
				$@"(
	AH_GC = '{companyPK}' 
	AND
	(
		(
			AH_PK IN 
			(
				SELECT AL_AH FROM dbo.AccTransactionLines WHERE AL_AH IS NOT NULL 
				AND
				(
					(
						AL_JH IN 
						(
							SELECT JH_PK FROM dbo.JobHeader WHERE JH_ParentID IN 
							(
								SELECT JS_PK FROM dbo.JobShipment WHERE JS_PK IN 
								(
									SELECT JN_JS FROM dbo.JobConShipLink WHERE JN_JK IN 
									(
										SELECT JK_PK FROM dbo.JobConsol WHERE JK_UniqueConsignRef = '{consolNumber}'
									)
								)
							)
							AND
							JH_GC = '{companyPK}' 
							AND
							JH_ParentTableCode = 'JS'
						)
					)
					OR
					(
						AL_JH IN 
						(
							SELECT JH_PK FROM dbo.JobHeader WHERE JH_JH_ParentJob IN 
							(
								SELECT JH_PK FROM dbo.JobHeader WHERE JH_ParentID IN 
								(
									SELECT JS_PK FROM dbo.JobShipment WHERE JS_PK IN 
									(
										SELECT JN_JS FROM dbo.JobConShipLink WHERE JN_JK IN 
										(
											SELECT JK_PK FROM dbo.JobConsol WHERE JK_UniqueConsignRef = '{consolNumber}'
										)
									)
								)
								AND
								JH_GC = '{companyPK}' 
								AND
								JH_ParentTableCode = 'JS'
							)
						)
					)
				)
				AND
				AL_AH is not NULL
			)
		)
		OR
		(
			AH_GC = '{companyPK}' AND
			AH_JobNumber = '{consolNumber}'
		)
	)
)
AND
(
	AH_Ledger = '{ledger}' 
	AND
	(
		AH_TransactionType in 
		(
			'ADJ', 'CRD', 'INV'
		)
	)
)
AND
AH_TransactionType = 'INV' 
AND
AH_InvoicePrinted = 0 
AND
AH_OH = '{Org1.PK}' 
AND
AH_PostDate >= '{fromDateString}' 
AND
AH_PostDate < '{toDateString}'
";

			AssertQueryForJob(expectedSql, ledger, "INV", false, Org1.PK, fromDate, toDate, true);

			// Basic params
			expectedSql =
$@"(
	AH_GC = '{companyPK}' 
	AND
	(
		(
			AH_PK IN 
			(
				SELECT AL_AH FROM dbo.AccTransactionLines WHERE AL_AH IS NOT NULL 
				AND
				(
					(
						AL_JH IN 
						(
							SELECT JH_PK FROM dbo.JobHeader WHERE JH_ParentID IN 
							(
								SELECT JS_PK FROM dbo.JobShipment WHERE JS_PK IN 
								(
									SELECT JN_JS FROM dbo.JobConShipLink WHERE JN_JK IN 
									(
										SELECT JK_PK FROM dbo.JobConsol WHERE JK_UniqueConsignRef = '{consolNumber}'
									)
								)
							)
							AND
							JH_GC = '{companyPK}' 
							AND
							JH_ParentTableCode = 'JS'
						)
					)
					OR
					(
						AL_JH IN 
						(
							SELECT JH_PK FROM dbo.JobHeader WHERE JH_JH_ParentJob IN 
							(
								SELECT JH_PK FROM dbo.JobHeader WHERE JH_ParentID IN 
								(
									SELECT JS_PK FROM dbo.JobShipment WHERE JS_PK IN 
									(
										SELECT JN_JS FROM dbo.JobConShipLink WHERE JN_JK IN 
										(
											SELECT JK_PK FROM dbo.JobConsol WHERE JK_UniqueConsignRef = '{consolNumber}'
										)
									)
								)
								AND
								JH_GC = '{companyPK}' 
								AND
								JH_ParentTableCode = 'JS'
							)
						)
					)
				)
				AND
				AL_AH is not NULL
			)
		)
		OR
		(
			AH_GC = '{companyPK}' AND
			AH_JobNumber = '{consolNumber}'
		)
	)
)
AND
(
	AH_Ledger = '{ledger}' 
	AND
	(
		AH_TransactionType in 
		(
		'ADJ', 'CRD', 'INV'
		)
	)
)
";

			AssertQueryForJob(expectedSql, ledger, string.Empty, true, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, true);
		}

		[TestDate(2017, 10, 15)]
		protected void TestAPGetQueryWithJobNumberCore(bool isIncludeReversal = false)
		{
			var companyPK = GlbCompany.CurrentCompany.PK;
			var from = ZDateTime.Today.AddDays(-1);
			var from_str = from.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
			var to = ZDateTime.Today;
			var to_str = to.AddDays(1).Date.ToZDateTime().ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
			var ledger = "AP";

			var includeReversalPart1 = isIncludeReversal ? @"
(" : "";
			var includeReversalFullPart2 = isIncludeReversal ? @"
	AND
	(
		AH_IsCancelled = 0 
		OR
		AH_TransactionBelongsToGroup is NULL
	)
)" : "";

			var includeReversalSimplePart2 = isIncludeReversal ? @"
AND
(
	AH_IsCancelled = 0 
	OR
	AH_TransactionBelongsToGroup is NULL
)" : "";

			string expectedSql =
$@"({includeReversalPart1}
	AH_GC = '{companyPK}' 
	AND
	(
		AH_JH = '{Header1.PK}' 
		OR
		AH_PK IN 
		(
			SELECT AL_AH FROM dbo.AccTransactionLines WHERE AL_AH IS NOT NULL 
			AND
			AL_JH IN 
			(
				SELECT JH_PK FROM dbo.JobHeader WHERE JH_PK = '{Header1.PK}'
				 UNION 
				SELECT JH_PK FROM dbo.JobHeader WHERE JH_JH_ParentJob = '{Header1.PK}'
			)
		)
	)
)
AND
(
	AH_Ledger = '{ledger}' 
	AND
	(
		AH_TransactionType in 
		(
			'ADJ', 'CRD', 'INV'
		)
	)
)
AND
AH_TransactionType = 'INV' 
AND
AH_InvoicePrinted = 0 
AND
AH_OH = '{Org1.PK}' {includeReversalFullPart2}
AND
AH_PostDate >= '{from_str}' 
AND
AH_PostDate < '{to_str}'
";
			AssertQueryForJob(expectedSql, ledger, "INV", false, Org1.PK, from, to);

			expectedSql =
$@"(
		AH_GC = '{companyPK}' 
		AND
		(
			AH_JH = '{Header1.PK}' 
			OR
			AH_PK IN 
		(
			SELECT AL_AH FROM dbo.AccTransactionLines WHERE AL_AH IS NOT NULL 
			AND
			AL_JH IN 
			(
				SELECT JH_PK FROM dbo.JobHeader WHERE JH_PK = '{Header1.PK}'
				 UNION 
				SELECT JH_PK FROM dbo.JobHeader WHERE JH_JH_ParentJob = '{Header1.PK}'
			)
		)
		)
	)
	AND
	(
		AH_Ledger = '{ledger}' 
		AND
		(
			AH_TransactionType in 
			(
				'ADJ', 'CRD', 'INV'
			)
		)
){includeReversalSimplePart2}
";

			AssertQueryForJob(expectedSql, ledger, string.Empty, true, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty);
		}

		protected void TestARGetQueryWithJobNumberCore(bool isIncludeReversal = false)
		{
			var companyPK = GlbCompany.CurrentCompany.PK;
			var from = ZDateTime.Today.AddDays(-1);
			var from_str = from.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
			var to = ZDateTime.Today;
			var to_str = to.AddDays(1).Date.ToZDateTime().ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
			var ledger = "AR";

			var includeReversalPart1 = isIncludeReversal ? @"
(" : "";
			var includeReversalFullPart2 = isIncludeReversal ? @"
	AND
	(
		AH_IsCancelled = 0 
		OR
		AH_TransactionBelongsToGroup is NULL
	)
)" : "";

			var includeReversalSimplePart2 = isIncludeReversal ? @"
AND
(
	AH_IsCancelled = 0 
	OR
	AH_TransactionBelongsToGroup is NULL
)" : "";

			string expectedSql =
$@"({includeReversalPart1}
	AH_GC = '{companyPK}' 
	AND
	(
		AH_JH = '{Header1.PK}' 
		OR
		AH_PK IN 
		(
			SELECT AL_AH FROM dbo.AccTransactionLines WHERE AL_AH IS NOT NULL 
			AND
			AL_JH IN 
			(
				SELECT JH_PK FROM dbo.JobHeader WHERE JH_PK = '{Header1.PK}'
				 UNION 
				SELECT JH_PK FROM dbo.JobHeader WHERE JH_JH_ParentJob = '{Header1.PK}'
			)
		)
	)
)
AND
(
	AH_Ledger = '{ledger}' 
	AND
	(
		AH_TransactionType in 
		(
			'ADJ', 'CRD', 'INV'
		)
	)
)
AND
AH_TransactionType = 'INV' 
AND
AH_InvoicePrinted = 0 
AND
AH_OH = '{Org1.PK}' {includeReversalFullPart2}
AND
AH_PostDate >= '{from_str}' 
AND
AH_PostDate < '{to_str}'
";
			AssertQueryForJob(expectedSql, ledger, "INV", false, Org1.PK, from, to);

			expectedSql =
$@"(
	AH_GC = '{companyPK}' 
	AND
	(
		AH_JH = '{Header1.PK}' 
		OR
		AH_PK IN 
		(
			SELECT AL_AH FROM dbo.AccTransactionLines WHERE AL_AH IS NOT NULL 
			AND
			AL_JH IN 
			(
				SELECT JH_PK FROM dbo.JobHeader WHERE JH_PK = '{Header1.PK}'
				 UNION 
				SELECT JH_PK FROM dbo.JobHeader WHERE JH_JH_ParentJob = '{Header1.PK}'
			)
		)
	)
)
AND
(
	AH_Ledger = '{ledger}' 
	AND
	(
		AH_TransactionType in 
		(
			'ADJ', 'CRD', 'INV'
		)
	)
){includeReversalSimplePart2}
";

			AssertQueryForJob(expectedSql, ledger, string.Empty, true, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty);
		}

		protected void TestJobCostGetQueryWithJobNumberCore()
		{
			var companyPK = GlbCompany.CurrentCompany.PK;
			var from = ZDateTime.Today.AddDays(-1);
			var from_str = from.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
			var to = ZDateTime.Today;
			var to_str = to.AddDays(1).Date.ToZDateTime().ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
			var ledger = LedgerTypes.JobCosting;

			string expectedSql =
$@"(
	(
		AH_GC = '{companyPK}' 
		AND
		(
		AH_JH = '{Header1.PK}' 
		OR
		AH_PK IN 
		(
			SELECT AL_AH FROM dbo.AccTransactionLines WHERE AL_AH IS NOT NULL 
			AND
			AL_JH IN 
			(
				SELECT JH_PK FROM dbo.JobHeader WHERE JH_PK = '{Header1.PK}'
				 UNION 
				SELECT JH_PK FROM dbo.JobHeader WHERE JH_JH_ParentJob = '{Header1.PK}'
			)
		)
	)
	)
	AND
	(
		AH_Ledger = '{ledger}' 
		AND
		AH_TransactionType = 'JRJ'
	)
	AND
	AH_TransactionType = 'JRJ' 
	AND
	AH_InvoicePrinted = 0 
	AND
	AH_OH = '{Org1.PK}' 
	AND
	(
		AH_IsCancelled = 0 
		OR
		AH_TransactionBelongsToGroup is NULL
	)
)
AND
AH_PostDate >= '{from_str}' 
AND
AH_PostDate < '{to_str}'
";
			AssertQueryForJob(expectedSql, ledger, "JRJ", false, Org1.PK, from, to);

			expectedSql =
$@"(
	AH_GC = '{companyPK}' 
	AND
	(
		AH_JH = '{Header1.PK}' 
		OR
		AH_PK IN 
		(
			SELECT AL_AH FROM dbo.AccTransactionLines WHERE AL_AH IS NOT NULL 
			AND
			AL_JH IN 
			(
				SELECT JH_PK FROM dbo.JobHeader WHERE JH_PK = '{Header1.PK}'
				 UNION 
				SELECT JH_PK FROM dbo.JobHeader WHERE JH_JH_ParentJob = '{Header1.PK}'
			)
		)
	)
)
AND
(
	AH_Ledger = '{ledger}' 
	AND
	AH_TransactionType = 'JRJ'
)
AND
(
	AH_IsCancelled = 0 
	OR
	AH_TransactionBelongsToGroup is NULL
)
";

			AssertQueryForJob(expectedSql, ledger, string.Empty, true, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty);
		}

		#endregion

		#region Implementation

		protected void AssertRefreshInvoiceList(JobInvoicePrintingFilter filter, ZGuid debtor, ZString transactionType, ZGuid jobNumber, int count)
		{
			filter.ResetInvoiceList();
			filter.DebtorOrCreditor = debtor;
			filter.TransactionType = transactionType;
			filter.JobNumber = jobNumber;
			filter.RefreshInvoiceList();
			AssertEquals("Invoice List Count", count, filter.Transactions.Count);
		}

		protected void SetupConsolAndShipments(ZString transportMode, ZString origin, ZString destination, OrgHeader agent)
		{
			Shipment1.JS_TransportMode = transportMode;
			Shipment1.JS_RL_NKOrigin = origin;
			Shipment1.JS_RL_NKDestination = destination;
			Shipment1.JS_OH_DeliveryAgent = agent.PK;
			Consol.Shipments.Add(Shipment1);

			Shipment2.JS_TransportMode = transportMode;
			Shipment2.JS_RL_NKOrigin = origin;
			Shipment2.JS_RL_NKDestination = destination;
			Shipment2.JS_OH_DeliveryAgent = agent.PK;
			Consol.Shipments.Add(Shipment2);
		}

		protected void SetDataForIndexTest()
		{
			var sql = string.Empty;
			for (var i = 0; i < 1000; i++)
			{
				sql = sql + "\n" + $@"INSERT INTO dbo.AccTransactionHeader
(AH_PK, AH_GC, AH_GB, AH_GE, AH_OH, AH_Ledger, AH_TransactionNum, AH_TransactionType, AH_InvoiceDate, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_SystemLastEditTimeUtc, AH_SystemLastEditUser)
VALUES (NEWID(), '{Env.CurrentCompanyPK}', '{Env.CurrentBranchPK}', '{Env.CurrentDepartmentPK}', '{TestObjectCreator.ABIGAS.PK}', 'AR', 'INV{i}', 'INV', '{ZDateTime.Today.ToISO8601String()}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			}
			for (var i = 0; i < 300; i++)
			{
				sql = sql + "\n" + $@" INSERT INTO dbo.JobHeader
(JH_PK, JH_JobNum, JH_GB, JH_GC, JH_GE, JH_ParentTableCode, JH_ParentID, JH_Status, JH_SystemCreateTimeUtc, JH_SystemCreateUser, JH_SystemLastEditTimeUtc, JH_SystemLastEditUser)
VALUES (NEWID(), 'JOB{i}', '{Env.CurrentBranchPK}', '{Env.CurrentCompanyPK}', '{Env.CurrentDepartmentPK}', 'JS', NEWID(), 'WRK', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			}
			Db.Connection.ExecuteNonQuery(sql);
		}

		#region Create Business Objects

		protected OrgHeader CreateOrgHeader(string code)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			OrgHeader header = newFactory.New<OrgHeader>();
			header.OH_Code = code;
			newFactory.Save();

			return header;
		}

		protected Job CreateJobHeader(ForwardingShipment shipment)
		{
			return new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
		}

		protected InvoicingBase CreateInvoice(ZString ledgerType, JobHeader jobHeader, ZGuid debtorPK, ZString transactionType)
		{
			InvoicingBase header = null;
			switch (ledgerType)
			{
				case LedgerTypes.AccountsReceivable:
					switch (transactionType)
					{
						case TransactionTypes.CreditNote:
							header = Factory.New<ARCreditNote>();
							break;

						default:
							header = Factory.New<ARInvoice>();
							break;
					}
					break;
				case LedgerTypes.AccountsPayable:
					switch (transactionType)
					{
						case TransactionTypes.CreditNote:
							header = Factory.New<APCreditNote>();
							break;

						default:
							header = Factory.New<APInvoice>();
							break;
					}
					header.AH_TransactionNum = APCreates.ToString();
					APCreates++;
					break;
			}

			header.AH_Ledger = ledgerType;
			header.AH_JH = jobHeader.PK;
			header.AH_OH = debtorPK;
			header.AH_TransactionType = transactionType;

			InvoicingLineBase line = (InvoicingLineBase)header.Lines.AddNew();
			line.AL_JH = jobHeader.PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			TestObjectCreator.CreateJobCharge(line, jobHeader, TestObjectCreator.CC1, TestObjectCreator.AUD);

			return header;
		}

		int APCreates = 100;

		#endregion

		protected ForwardingConsol Consol;
		protected ForwardingShipment Shipment1;
		protected ForwardingShipment Shipment2;

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObject(Consol, new Job[] { Factory.NewJobForTesting<Job>() });
		}

		protected override void SetUp()
		{
			base.SetUp();

			Consol = CreateConsol();

			Shipment1 = CreateShipment();
			Shipment2 = CreateShipment();

			Header1 = CreateJobHeader(Shipment1);
			Header2 = CreateJobHeader(Shipment2);

			Org1 = CreateOrgHeader("Debtor1");
			Org2 = CreateOrgHeader("Debtor2");
			Org3 = CreateOrgHeader("Debtor3");
			New1 = CreateOrgHeader("New1");

			ZString oppositeLedger = Ledger == LedgerTypes.AccountsReceivable ? LedgerTypes.AccountsPayable : LedgerTypes.AccountsReceivable;

			Invoice1_1 = CreateInvoice(Ledger, Header1, Org1.PK, TransactionTypes.Invoice);
			Invoice1_3 = CreateInvoice(Ledger, Header1, Org2.PK, TransactionTypes.CreditNote);
			Invoice1_4 = CreateInvoice(Ledger, Header1, Org1.PK, TransactionTypes.Invoice);
			Invoice1_5 = CreateInvoice(Ledger, Header1, Org2.PK, TransactionTypes.Invoice);
			CreateInvoice(Ledger, Header1, Org1.PK, TransactionTypes.CreditNote);
			CreateInvoice(Ledger, Header1, Org1.PK, TransactionTypes.Invoice);
			CreateInvoice(oppositeLedger, Header1, New1.PK, TransactionTypes.Invoice);
			CreateInvoice(Ledger, Header1, Org1.PK, TransactionTypes.AdjustmentNote);
			var journal = TestObjectCreator.CreateJobRevenueJournal(typeof(JobRevenueJournal), TestObjectCreator.CC1, Header1.PK, 100M);
			journal.AH_OH = Org1.PK;
			foreach (AccTransactionLines line in journal.Lines)
			{
				line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			}

			CreateInvoice(Ledger, Header2, Org1.PK, TransactionTypes.Invoice);
			CreateInvoice(Ledger, Header2, Org1.PK, TransactionTypes.CreditNote);
			Invoice2_3 = CreateInvoice(Ledger, Header2, Org3.PK, TransactionTypes.Invoice);
			CreateInvoice(Ledger, Header2, Org3.PK, TransactionTypes.Invoice);
			CreateInvoice(Ledger, Header2, Org3.PK, TransactionTypes.Invoice);
			CreateInvoice(oppositeLedger, Header2, CreateOrgHeader("New2").PK, TransactionTypes.Invoice);
			CreateInvoice(Ledger, Header2, Org3.PK, TransactionTypes.AdjustmentNote);
		}

		protected abstract ZString Ledger { get; }

		protected OrgHeader Org1;
		protected OrgHeader Org2;
		protected OrgHeader Org3;
		protected OrgHeader New1;

		protected Job Header1;
		protected Job Header2;

		protected InvoicingBase Invoice1_1;
		protected InvoicingBase Invoice1_3;
		protected InvoicingBase Invoice1_4;
		protected InvoicingBase Invoice1_5;

		InvoicingBase Invoice2_3;

		protected ForwardingShipment CreateShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			return shipment;
		}

		protected ForwardingConsol CreateConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			return consol;
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		protected abstract JobInvoicePrintingFilter GetNewBusinessObject(IBusiness hostBusinessObject, Job jobHeader);
		protected abstract JobInvoicePrintingFilter GetNewBusinessObject(ForwardingConsol hostBusinessObject, Job[] jobHeaders);

		#endregion
	}
}

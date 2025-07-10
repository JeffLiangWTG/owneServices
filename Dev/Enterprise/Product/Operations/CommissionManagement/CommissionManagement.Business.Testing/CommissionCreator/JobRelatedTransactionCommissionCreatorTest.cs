using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing.Commission.CommissionCreator
{
	class JobRelatedTransactionCommissionCreatorTest : CommissionCreatorTestCase
	{
		#region Constructor

		public void TestConstructor()
		{
			var job = Factory.NewJobForTesting<Job>();
			var jobInvoice = Factory.New<ARInvoice>();
			jobInvoice.AH_JH = job.PK;
			AssertNoExceptionThrown(() =>
			{
				new JobRelatedTransactionCommissionCreator(jobInvoice, null);
			});

			var invoiceReversing = new ARInvoiceReversing(jobInvoice);
			invoiceReversing.Reverse();
			var reversal = jobInvoice.ReverseInvoice;
			AssertExceptionThrown(typeof(ArgumentException), string.Format("Passed in transaction (PK:{0}) which is a reversal. Use {1} to create commission for reversal transactions", reversal.PK, nameof(ReversalTransactionCommissionCreator)), () =>
			{
				new JobRelatedTransactionCommissionCreator(reversal, null);
			});

			var nonJobRelatedInvoice = Factory.New<ARInvoice>();
			AssertExceptionThrown(typeof(ArgumentException), string.Format("Must be job-related transaction, but passed in transaction (PK:{0}) with empty AH_JH and no job related transaction lines", nonJobRelatedInvoice.PK), () =>
			{
				new JobRelatedTransactionCommissionCreator(nonJobRelatedInvoice, null);
			});
		}

		#endregion

		#region CreateCommissions

		public void TestTransactionAmount_Reciprocal()
		{
			OrganisationsDataRegistry.Instance.CommissionTransactionJobTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CommissionTransactionJobTriggerList.Codes.RevenueCommissionCalculationsToBeCreatedAtInvStatus);

			var chargeXxx = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeYyy = Factory.NewWithValidTestData<AccChargeCode>();
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";
			var scwStaff = Factory.NewWithValidTestData<GlbStaff>();
			scwStaff.GS_Code = "SCW";

			var agreement = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAllItems(Factory, customer);
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			var recipient1 = agreement.Recipients.AddNew();
			recipient1.CAR_GS_NKStaff = "ADL";
			recipient1.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient1.CAR_Share = 1;
			var recipientRate1 = recipient1.Rates.AddNew();
			recipientRate1.CAT_CommissionPercentage = 50;

			var recipient2 = agreement.Recipients.AddNew();
			recipient2.CAR_OH_Party = org.PK;
			recipient2.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient2.CAR_Share = 3;
			var recipientRate2 = recipient2.Rates.AddNew();
			recipientRate2.CAT_CommissionPercentage = 10;

			var recipient3 = agreement.Recipients.AddNew();
			recipient3.CAR_GS_NKStaff = "SCW";
			recipient3.CAR_CommissionType = CommissionTypes.Codes.FIX;
			recipient3.CAR_Share = 1;
			var recipientRate3 = recipient3.Rates.AddNew();
			recipientRate3.CAT_CommissionAmount = 100;
			recipientRate3.CAT_RX_NKCommissionCurrency = "AUD";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "TEST00001";
			var job = new Job.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = customer.MainAddress.PK;
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_JH = job.PK;
			invoice.AH_RX_NKTransactionCurrency = "USD";
			invoice.AH_PostDate = new ZDateTime(2002, 2, 2);

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			GlbCompany.CurrentCompany.Factory.Save();

			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, audCurrency, 1, "", 10);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, audCurrency, 1, "", 100);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, audCurrency, 1, "", 1000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, usdCurrency, 0.5m, "", 10000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, usdCurrency, 0.5m, "", 100000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, usdCurrency, 0.5m, "", 1000000);

			foreach (TransactionLine line in invoice.Lines)
			{
				TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, line.TransactionCurrency);
			}

			Factory.Save();

			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.REV;
			Factory.Save();

			var commissionCreator = new JobRelatedTransactionCommissionCreator(invoice, null);
			commissionCreator.CreateCommissions();

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK));
			var commissionHeader = commissionHeaders[0];
			CombineAssertions("commissionHeader Properties", () =>
			{
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GC, invoice.AH_GC, commissionHeader.CH0_GC);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceTableCode, job.TablePrefix, commissionHeader.CH0_GroupingSourceTableCode);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceID, job.PK, commissionHeader.CH0_GroupingSourceID);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CA0, agreement.PK, commissionHeader.CH0_CA0);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OH_Customer, customer.PK, commissionHeader.CH0_OH_Customer);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Product, JobInvoicingConsumerTypes.Shipment.Code, commissionHeader.CH0_Product);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Service, OrgCommissionAgreementItemLookups.AllServicesCode, commissionHeader.CH0_Service);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SubModule, OrgCommissionAgreementItemLookups.AllSubModulesCode, commissionHeader.CH0_SubModule);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_JobNumber, "TEST00001", commissionHeader.CH0_JobNumber);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotDateTime, new ZDateTime(2002, 2, 2), commissionHeader.CH0_SnapshotDateTime);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotEventCode, AccCommissionHeaderSnapshotEventList.Codes.Posted, commissionHeader.CH0_SnapshotEventCode);
			});
			AssertEquals(1, commissionHeaders.Length);

			var lineGroupAudChargeXxx = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeXxx.PK);
			CombineAssertions("commission line group properties for charge Xxx and currency AUD", () =>
			{
				AssertEquals(110m, lineGroupAudChargeXxx.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeXxx.CLG_RX_NKTransactionCurrency);
				AssertEquals(110m, lineGroupAudChargeXxx.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeXxx.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupAudChargeXxx.Lines.Count);
			});

			var lineGroupUsdChargeXxx = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "USD" && x.CLG_AC == chargeXxx.PK);
			CombineAssertions("commission line group properties for charge Xxx and currency USD", () =>
			{
				AssertEquals(10000m, lineGroupUsdChargeXxx.CLG_TransactionAmount);
				AssertEquals("USD", lineGroupUsdChargeXxx.CLG_RX_NKTransactionCurrency);
				AssertEquals(5000m, lineGroupUsdChargeXxx.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupUsdChargeXxx.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupUsdChargeXxx.Lines.Count);
			});

			var lineGroupAudChargeYyy = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeYyy.PK);
			CombineAssertions("commission line group properties for charge Yyy and currency AUD", () =>
			{
				AssertEquals(1000m, lineGroupAudChargeYyy.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeYyy.CLG_RX_NKTransactionCurrency);
				AssertEquals(1000m, lineGroupAudChargeYyy.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeYyy.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupAudChargeYyy.Lines.Count);
			});

			var lineGroupUsdChargeYyy = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "USD" && x.CLG_AC == chargeYyy.PK);
			CombineAssertions("commission line group properties for charge Yyy and currency USD", () =>
			{
				AssertEquals(1100000m, lineGroupUsdChargeYyy.CLG_TransactionAmount);
				AssertEquals("USD", lineGroupUsdChargeYyy.CLG_RX_NKTransactionCurrency);
				AssertEquals(550000m, lineGroupUsdChargeYyy.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupUsdChargeYyy.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupUsdChargeYyy.Lines.Count);
			});

			AssertEquals("Should have grouped the transaction lines into charge code and invoice currency pairs", 4, commissionHeader.LineGroups.Count);

			AssertEquals("Should have 1 fixed commission line", 1, commissionHeader.Lines.Count);
		}

		public void TestCreateCommissions_OnJobClosure()
		{
			var chargeXxx = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeYyy = Factory.NewWithValidTestData<AccChargeCode>();
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";
			var scwStaff = Factory.NewWithValidTestData<GlbStaff>();
			scwStaff.GS_Code = "SCW";

			var agreement = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAllItems(Factory, customer);
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			var recipient1 = agreement.Recipients.AddNew();
			recipient1.CAR_GS_NKStaff = "ADL";
			recipient1.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient1.CAR_Share = 1;
			var recipientRate1 = recipient1.Rates.AddNew();
			recipientRate1.CAT_CommissionPercentage = 50;

			var recipient2 = agreement.Recipients.AddNew();
			recipient2.CAR_OH_Party = org.PK;
			recipient2.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient2.CAR_Share = 3;
			var recipientRate2 = recipient2.Rates.AddNew();
			recipientRate2.CAT_CommissionPercentage = 10;

			var recipient3 = agreement.Recipients.AddNew();
			recipient3.CAR_GS_NKStaff = "SCW";
			recipient3.CAR_CommissionType = CommissionTypes.Codes.FIX;
			recipient3.CAR_Share = 1;
			var recipientRate3 = recipient3.Rates.AddNew();
			recipientRate3.CAT_CommissionAmount = 100;
			recipientRate3.CAT_RX_NKCommissionCurrency = "AUD";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "TEST00001";
			var job = new Job.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = customer.MainAddress.PK;
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_JH = job.PK;
			invoice.AH_RX_NKTransactionCurrency = "USD";
			invoice.AH_PostDate = new ZDateTime(2002, 2, 2);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, audCurrency, 1, "", 10);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, audCurrency, 1, "", 100);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, audCurrency, 1, "", 1000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, usdCurrency, 0.5m, "", 10000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, usdCurrency, 0.5m, "", 100000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, usdCurrency, 0.5m, "", 1000000);

			foreach (TransactionLine line in invoice.Lines)
			{
				TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, line.TransactionCurrency);
			}

			Factory.Save();

			var commissionCreator = new JobRelatedTransactionCommissionCreator(invoice, Tuple.Create<IJobHeader, ZDateTime>(job, new ZDateTime(2003, 3, 3)));
			commissionCreator.CreateCommissions();

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK));
			var commissionHeader = commissionHeaders[0];
			CombineAssertions("commissionHeader Properties", () =>
			{
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GC, invoice.AH_GC, commissionHeader.CH0_GC);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceTableCode, job.TablePrefix, commissionHeader.CH0_GroupingSourceTableCode);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceID, job.PK, commissionHeader.CH0_GroupingSourceID);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CA0, agreement.PK, commissionHeader.CH0_CA0);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OH_Customer, customer.PK, commissionHeader.CH0_OH_Customer);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Product, JobInvoicingConsumerTypes.Shipment.Code, commissionHeader.CH0_Product);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Service, OrgCommissionAgreementItemLookups.AllServicesCode, commissionHeader.CH0_Service);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SubModule, OrgCommissionAgreementItemLookups.AllSubModulesCode, commissionHeader.CH0_SubModule);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_JobNumber, "TEST00001", commissionHeader.CH0_JobNumber);

				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotDateTime, new ZDateTime(2003, 3, 3), commissionHeader.CH0_SnapshotDateTime);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotEventCode, AccCommissionHeaderSnapshotEventList.Codes.JobClosure, commissionHeader.CH0_SnapshotEventCode);
			});
			AssertEquals(1, commissionHeaders.Length);

			var lineGroupAudChargeXxx = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeXxx.PK);
			CombineAssertions("commission line group properties for charge Xxx and currency AUD", () =>
			{
				AssertEquals(110m, lineGroupAudChargeXxx.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeXxx.CLG_RX_NKTransactionCurrency);
				AssertEquals(110m, lineGroupAudChargeXxx.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeXxx.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupAudChargeXxx.Lines.Count);
			});

			var lineGroupUsdChargeXxx = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "USD" && x.CLG_AC == chargeXxx.PK);
			CombineAssertions("commission line group properties for charge Xxx and currency USD", () =>
			{
				AssertEquals(10000m, lineGroupUsdChargeXxx.CLG_TransactionAmount);
				AssertEquals("USD", lineGroupUsdChargeXxx.CLG_RX_NKTransactionCurrency);
				AssertEquals(20000m, lineGroupUsdChargeXxx.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupUsdChargeXxx.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupUsdChargeXxx.Lines.Count);
			});

			var lineGroupAudChargeYyy = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeYyy.PK);
			CombineAssertions("commission line group properties for charge Yyy and currency AUD", () =>
			{
				AssertEquals(1000m, lineGroupAudChargeYyy.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeYyy.CLG_RX_NKTransactionCurrency);
				AssertEquals(1000m, lineGroupAudChargeYyy.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeYyy.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupAudChargeYyy.Lines.Count);
			});

			var lineGroupUsdChargeYyy = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "USD" && x.CLG_AC == chargeYyy.PK);
			CombineAssertions("commission line group properties for charge Yyy and currency USD", () =>
			{
				AssertEquals(1100000m, lineGroupUsdChargeYyy.CLG_TransactionAmount);
				AssertEquals("USD", lineGroupUsdChargeYyy.CLG_RX_NKTransactionCurrency);
				AssertEquals(2200000m, lineGroupUsdChargeYyy.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupUsdChargeYyy.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupUsdChargeYyy.Lines.Count);
			});

			AssertEquals("Should have grouped the transaction lines into charge code and invoice currency pairs", 4, commissionHeader.LineGroups.Count);

			AssertEquals("Should have 1 fixed commission line", 1, commissionHeader.Lines.Count);
		}

		public void TestCreateCommissions_OnJobClosure_Mode()
		{
			var chargeXxx = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeYyy = Factory.NewWithValidTestData<AccChargeCode>();
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";
			var scwStaff = Factory.NewWithValidTestData<GlbStaff>();
			scwStaff.GS_Code = "SCW";

			var agreement = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAirShipment(Factory, customer);
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			var recipient1 = agreement.Recipients.AddNew();
			recipient1.CAR_GS_NKStaff = "ADL";
			recipient1.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient1.CAR_Share = 1;
			var recipientRate1 = recipient1.Rates.AddNew();
			recipientRate1.CAT_CommissionPercentage = 50;

			var recipient2 = agreement.Recipients.AddNew();
			recipient2.CAR_OH_Party = org.PK;
			recipient2.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient2.CAR_Share = 3;
			var recipientRate2 = recipient2.Rates.AddNew();
			recipientRate2.CAT_CommissionPercentage = 10;

			var recipient3 = agreement.Recipients.AddNew();
			recipient3.CAR_GS_NKStaff = "SCW";
			recipient3.CAR_CommissionType = CommissionTypes.Codes.FIX;
			recipient3.CAR_Share = 1;
			var recipientRate3 = recipient3.Rates.AddNew();
			recipientRate3.CAT_CommissionAmount = 100;
			recipientRate3.CAT_RX_NKCommissionCurrency = "AUD";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_UniqueConsignRef = "TEST00001";

			var job = new Job.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = customer.MainAddress.PK;
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_JH = job.PK;
			invoice.AH_RX_NKTransactionCurrency = "USD";
			invoice.AH_PostDate = new ZDateTime(2002, 2, 2);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, audCurrency, 1, "", 10);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, audCurrency, 1, "", 100);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, audCurrency, 1, "", 1000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, usdCurrency, 0.5m, "", 10000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, usdCurrency, 0.5m, "", 100000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, usdCurrency, 0.5m, "", 1000000);

			foreach (TransactionLine line in invoice.Lines)
			{
				TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, line.TransactionCurrency);
			}

			Factory.Save();

			var commissionCreator = new JobRelatedTransactionCommissionCreator(invoice, Tuple.Create<IJobHeader, ZDateTime>(job, new ZDateTime(2003, 3, 3)));
			commissionCreator.CreateCommissions();

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK));
			AssertEquals(1, commissionHeaders.Length);
			var commissionHeader = commissionHeaders[0];
			CombineAssertions("commissionHeader Properties", () =>
			{
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GC, invoice.AH_GC, commissionHeader.CH0_GC);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceTableCode, job.TablePrefix, commissionHeader.CH0_GroupingSourceTableCode);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceID, job.PK, commissionHeader.CH0_GroupingSourceID);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CA0, agreement.PK, commissionHeader.CH0_CA0);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OH_Customer, customer.PK, commissionHeader.CH0_OH_Customer);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Product, JobInvoicingConsumerTypes.Shipment.Code, commissionHeader.CH0_Product);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Service, OrgCommissionAgreementItemLookups.AllServicesCode, commissionHeader.CH0_Service);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SubModule, OrgCommissionAgreementItemLookups.AllSubModulesCode, commissionHeader.CH0_SubModule);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_JobNumber, "TEST00001", commissionHeader.CH0_JobNumber);

				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotDateTime, new ZDateTime(2003, 3, 3), commissionHeader.CH0_SnapshotDateTime);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotEventCode, AccCommissionHeaderSnapshotEventList.Codes.JobClosure, commissionHeader.CH0_SnapshotEventCode);
			});
			AssertEquals(1, commissionHeaders.Length);

			var lineGroupAudChargeXxx = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeXxx.PK);
			CombineAssertions("commission line group properties for charge Xxx and currency AUD", () =>
			{
				AssertEquals(110m, lineGroupAudChargeXxx.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeXxx.CLG_RX_NKTransactionCurrency);
				AssertEquals(110m, lineGroupAudChargeXxx.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeXxx.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupAudChargeXxx.Lines.Count);
			});

			var lineGroupUsdChargeXxx = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "USD" && x.CLG_AC == chargeXxx.PK);
			CombineAssertions("commission line group properties for charge Xxx and currency USD", () =>
			{
				AssertEquals(10000m, lineGroupUsdChargeXxx.CLG_TransactionAmount);
				AssertEquals("USD", lineGroupUsdChargeXxx.CLG_RX_NKTransactionCurrency);
				AssertEquals(20000m, lineGroupUsdChargeXxx.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupUsdChargeXxx.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupUsdChargeXxx.Lines.Count);
			});

			var lineGroupAudChargeYyy = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeYyy.PK);
			CombineAssertions("commission line group properties for charge Yyy and currency AUD", () =>
			{
				AssertEquals(1000m, lineGroupAudChargeYyy.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeYyy.CLG_RX_NKTransactionCurrency);
				AssertEquals(1000m, lineGroupAudChargeYyy.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeYyy.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupAudChargeYyy.Lines.Count);
			});

			var lineGroupUsdChargeYyy = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "USD" && x.CLG_AC == chargeYyy.PK);
			CombineAssertions("commission line group properties for charge Yyy and currency USD", () =>
			{
				AssertEquals(1100000m, lineGroupUsdChargeYyy.CLG_TransactionAmount);
				AssertEquals("USD", lineGroupUsdChargeYyy.CLG_RX_NKTransactionCurrency);
				AssertEquals(2200000m, lineGroupUsdChargeYyy.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupUsdChargeYyy.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupUsdChargeYyy.Lines.Count);
			});

			AssertEquals("Should have grouped the transaction lines into charge code and invoice currency pairs", 4, commissionHeader.LineGroups.Count);

			AssertEquals("Should have 1 fixed commission line", 1, commissionHeader.Lines.Count);
		}

		public void TestCreateCommissions_AccountingRevenueRecognition()
		{
			SetupTimePeriods();
			OrganisationsDataRegistry.Instance.CommissionRecognitionDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CommissionRecognitionDateTypeList.Codes.AccountingRevenueRecognition);
			SetupRevenueRecognitionRegistry();

			var chargeXxx = Factory.NewWithValidTestData<AccChargeCode>();
			chargeXxx.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			var chargeYyy = Factory.NewWithValidTestData<AccChargeCode>();
			chargeYyy.AC_ChargeGroup = ChargeCodeGroupList.Codes.CFSShipment;
			var chargeZzz = Factory.NewWithValidTestData<AccChargeCode>();
			chargeZzz.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;

			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";

			var agreement = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAirShipment(Factory, customer);
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			var recipient1 = agreement.Recipients.AddNew();
			recipient1.CAR_GS_NKStaff = "ADL";
			recipient1.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient1.CAR_Share = 1;
			var recipientRate1 = recipient1.Rates.AddNew();
			recipientRate1.CAT_CommissionPercentage = 100;
			recipientRate1.CAT_CommissionEndDate = new ZDate(2020, 2, 2);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_E_DEP = new ZDateTime(2020, 2, 1);
			shipment.JS_E_ARV = new ZDateTime(2020, 2, 5);
			shipment.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2020, 2, 10);
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2020, 2, 10);
			shipment.JS_UniqueConsignRef = "TEST00001";

			var job = new Job.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = customer.MainAddress.PK;
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_JH = job.PK;
			invoice.AH_RX_NKTransactionCurrency = "AUD";
			invoice.AH_PostDate = new ZDateTime(2020, 1, 31);
			var arLine1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, audCurrency, 1, "", 2500);
			arLine1.AL_LineType = TransactionLineTypes.Revenue;

			var arLine2 = TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, audCurrency, 1, "", 3200);
			arLine2.AL_LineType = TransactionLineTypes.Revenue;

			var arLine3 = TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeZzz, audCurrency, 1, "", 4500);
			arLine3.AL_LineType = TransactionLineTypes.Revenue;

			foreach (TransactionLine line in invoice.Lines)
			{
				TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, line.TransactionCurrency);
			}

			Factory.Save();

			var commissionCreator = new JobRelatedTransactionCommissionCreator(invoice, Tuple.Create<IJobHeader, ZDateTime>(job, new ZDateTime(2003, 3, 3)));
			commissionCreator.CreateCommissions();

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK));
			AssertEquals(1, commissionHeaders.Length);
			var commissionHeader = commissionHeaders[0];
			CombineAssertions("commissionHeader Properties", () =>
			{
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GC, invoice.AH_GC, commissionHeader.CH0_GC);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceTableCode, job.TablePrefix, commissionHeader.CH0_GroupingSourceTableCode);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceID, job.PK, commissionHeader.CH0_GroupingSourceID);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CA0, agreement.PK, commissionHeader.CH0_CA0);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OH_Customer, customer.PK, commissionHeader.CH0_OH_Customer);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Product, JobInvoicingConsumerTypes.Shipment.Code, commissionHeader.CH0_Product);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Service, OrgCommissionAgreementItemLookups.AllServicesCode, commissionHeader.CH0_Service);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SubModule, OrgCommissionAgreementItemLookups.AllSubModulesCode, commissionHeader.CH0_SubModule);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CommissionDate, new ZDateTime(2020, 2, 1), commissionHeader.CH0_CommissionDate);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_JobNumber, "TEST00001", commissionHeader.CH0_JobNumber);

				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotDateTime, new ZDateTime(2003, 3, 3), commissionHeader.CH0_SnapshotDateTime);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotEventCode, AccCommissionHeaderSnapshotEventList.Codes.JobClosure, commissionHeader.CH0_SnapshotEventCode);
			});
			AssertEquals(1, commissionHeaders.Length);

			var lineGroupAudChargeXxx = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeXxx.PK);
			CombineAssertions("commission line group properties for charge Xxx", () =>
			{
				AssertEquals(2500m, lineGroupAudChargeXxx.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeXxx.CLG_RX_NKTransactionCurrency);
				AssertEquals(2500m, lineGroupAudChargeXxx.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeXxx.CLG_RX_NKCommissionCurrency);
				AssertEquals(new ZDateTime(2020, 2, 1), lineGroupAudChargeXxx.CLG_CommissionDate);
				AssertEquals(1, lineGroupAudChargeXxx.Lines.Count);
			});

			var lineGroupAudChargeYyy = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeYyy.PK);
			CombineAssertions("commission line group properties for charge Yyy", () =>
			{
				AssertEquals(3200m, lineGroupAudChargeYyy.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeYyy.CLG_RX_NKTransactionCurrency);
				AssertEquals(3200m, lineGroupAudChargeYyy.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeYyy.CLG_RX_NKCommissionCurrency);
				AssertEquals(new ZDateTime(2020, 2, 5), lineGroupAudChargeYyy.CLG_CommissionDate);
				AssertEquals(1, lineGroupAudChargeYyy.Lines.Count);
			});

			var lineGroupAudChargeZzz = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeZzz.PK);
			CombineAssertions("commission line group properties for charge Zzz", () =>
			{
				AssertEquals(4500m, lineGroupAudChargeZzz.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeZzz.CLG_RX_NKTransactionCurrency);
				AssertEquals(4500m, lineGroupAudChargeZzz.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeZzz.CLG_RX_NKCommissionCurrency);
				AssertEquals(new ZDateTime(2020, 2, 10), lineGroupAudChargeZzz.CLG_CommissionDate);
				AssertEquals(1, lineGroupAudChargeZzz.Lines.Count);
			});

			AssertEquals("Should have grouped the transaction lines into charge code", 3, commissionHeader.LineGroups.Count);
		}

		public void TestCreateCommissions_AccountingRevenueRecognition_MultipleInvoices()
		{
			SetupTimePeriods();
			OrganisationsDataRegistry.Instance.CommissionRecognitionDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CommissionRecognitionDateTypeList.Codes.AccountingRevenueRecognition);
			SetupRevenueRecognitionRegistry();

			var chargeUuu = Factory.NewWithValidTestData<AccChargeCode>();
			chargeUuu.AC_ChargeGroup = ChargeCodeGroupList.Codes.Insurance;
			var chargeVvv = Factory.NewWithValidTestData<AccChargeCode>();
			chargeVvv.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			var chargeWww = Factory.NewWithValidTestData<AccChargeCode>();
			chargeWww.AC_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;
			var chargeXxx = Factory.NewWithValidTestData<AccChargeCode>();
			chargeXxx.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			var chargeYyy = Factory.NewWithValidTestData<AccChargeCode>();
			chargeYyy.AC_ChargeGroup = ChargeCodeGroupList.Codes.CFSShipment;
			var chargeZzz = Factory.NewWithValidTestData<AccChargeCode>();
			chargeZzz.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;

			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";

			var agreement = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAirShipment(Factory, customer);
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			agreement.CA0_EffectiveDate = new ZDate(2020, 1, 10);
			agreement.CA0_ExpiredDate = new ZDate(2020, 2, 3);

			var recipient1 = agreement.Recipients.AddNew();
			recipient1.CAR_GS_NKStaff = "ADL";
			recipient1.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient1.CAR_Share = 1;
			var recipientRate1 = recipient1.Rates.AddNew();
			recipientRate1.CAT_CommissionPercentage = 100;
			recipientRate1.CAT_CommissionEndDate = new ZDate(2020, 2, 2);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_E_DEP = new ZDateTime(2020, 2, 1);
			shipment.JS_E_ARV = new ZDateTime(2020, 2, 5);
			shipment.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2020, 2, 10);
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2020, 2, 10);
			shipment.JS_UniqueConsignRef = "TEST00001";

			var job = new Job.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = customer.MainAddress.PK;
			var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_JH = job.PK;
			invoice1.AH_RX_NKTransactionCurrency = "AUD";
			invoice1.AH_PostDate = new ZDateTime(2020, 1, 15);

			var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2.AH_JH = job.PK;
			invoice2.AH_RX_NKTransactionCurrency = "AUD";
			invoice2.AH_PostDate = new ZDateTime(2020, 1, 31);

			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_LCLExport;
			cartage.JJ_ConsignmentID = "TCARTAGE1";

			var childJob = new Job.Loader(cartage).TryCreate();
			childJob.JH_OA_LocalChargesAddr = customer.MainAddress.PK;
			childJob.JH_JH_ParentJob = job.PK;

			var invoice3 = Factory.NewWithValidTestData<ARInvoice>();
			invoice3.AH_JH = childJob.PK;
			invoice3.AH_RX_NKTransactionCurrency = "AUD";
			invoice3.AH_PostDate = new ZDateTime(2020, 1, 31);

			var arLine1 = TestObjectCreator.CreateARInvoiceLine(invoice1, job, chargeWww, audCurrency, 1, "", 2000);
			arLine1.AL_LineType = TransactionLineTypes.Revenue;

			var arLine2 = TestObjectCreator.CreateARInvoiceLine(invoice1, job, chargeXxx, audCurrency, 1, "", 2500);
			arLine2.AL_LineType = TransactionLineTypes.Revenue;

			var arLine3 = TestObjectCreator.CreateARInvoiceLine(invoice2, job, chargeVvv, audCurrency, 1, "", 2600);
			arLine3.AL_LineType = TransactionLineTypes.Revenue;

			var arLine4 = TestObjectCreator.CreateARInvoiceLine(invoice2, job, chargeWww, audCurrency, 1, "", 2800);
			arLine4.AL_LineType = TransactionLineTypes.Revenue;

			var arLine5 = TestObjectCreator.CreateARInvoiceLine(invoice2, job, chargeYyy, audCurrency, 1, "", 3200);
			arLine5.AL_LineType = TransactionLineTypes.Revenue;

			var arLine6 = TestObjectCreator.CreateARInvoiceLine(invoice2, job, chargeZzz, audCurrency, 1, "", 4500);
			arLine6.AL_LineType = TransactionLineTypes.Revenue;

			var arLine7 = TestObjectCreator.CreateARInvoiceLine(invoice3, childJob, chargeVvv, audCurrency, 1, "", 4700);
			arLine7.AL_LineType = TransactionLineTypes.Revenue;

			var arLine8 = TestObjectCreator.CreateARInvoiceLine(invoice3, childJob, chargeUuu, audCurrency, 1, "", 4800);
			arLine8.AL_LineType = TransactionLineTypes.Revenue;

			foreach (TransactionLine line in invoice1.Lines.Union(invoice2.Lines))
			{
				TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, line.TransactionCurrency);
			}

			foreach (TransactionLine line in invoice3.Lines)
			{
				TestObjectCreator.CreateJobCharge(line, childJob, line.ChargeCode, line.TransactionCurrency);
			}

			Factory.Save();

			job.JH_A_JCL = new ZDateTime(2020, 2, 20);
			job.JH_Status = JobHeaderStatus.Closed.Code;

			var commissionCreator = new JobRelatedTransactionCommissionCreator(invoice1, Tuple.Create<IJobHeader, ZDateTime>(job, new ZDateTime(2003, 3, 3)));
			commissionCreator.CreateCommissions();

			commissionCreator = new JobRelatedTransactionCommissionCreator(invoice2, Tuple.Create<IJobHeader, ZDateTime>(job, new ZDateTime(2003, 3, 3)));
			commissionCreator.CreateCommissions();

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, new ZGuid[] { invoice1.PK, invoice2.PK }));
			AssertEquals(2, commissionHeaders.Length);
			var commissionHeader1 = commissionHeaders.Single(c => c.CH0_AH_Source == invoice1.PK);
			CombineAssertions("commissionHeader 1 Properties", () =>
			{
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GC, invoice1.AH_GC, commissionHeader1.CH0_GC);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceTableCode, job.TablePrefix, commissionHeader1.CH0_GroupingSourceTableCode);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceID, job.PK, commissionHeader1.CH0_GroupingSourceID);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CA0, agreement.PK, commissionHeader1.CH0_CA0);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OH_Customer, customer.PK, commissionHeader1.CH0_OH_Customer);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Product, JobInvoicingConsumerTypes.Shipment.Code, commissionHeader1.CH0_Product);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Service, OrgCommissionAgreementItemLookups.AllServicesCode, commissionHeader1.CH0_Service);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SubModule, OrgCommissionAgreementItemLookups.AllSubModulesCode, commissionHeader1.CH0_SubModule);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CommissionDate, new ZDateTime(2020, 1, 15), commissionHeader1.CH0_CommissionDate);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_JobNumber, "TEST00001", commissionHeader1.CH0_JobNumber);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotDateTime, new ZDateTime(2003, 3, 3), commissionHeader1.CH0_SnapshotDateTime);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotEventCode, AccCommissionHeaderSnapshotEventList.Codes.JobClosure, commissionHeader1.CH0_SnapshotEventCode);
			});

			var commissionHeader2 = commissionHeaders.Single(c => c.CH0_AH_Source == invoice2.PK);
			CombineAssertions("commissionHeader 2 Properties", () =>
			{
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GC, invoice2.AH_GC, commissionHeader2.CH0_GC);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceTableCode, job.TablePrefix, commissionHeader2.CH0_GroupingSourceTableCode);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceID, job.PK, commissionHeader2.CH0_GroupingSourceID);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CA0, agreement.PK, commissionHeader2.CH0_CA0);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OH_Customer, customer.PK, commissionHeader2.CH0_OH_Customer);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Product, JobInvoicingConsumerTypes.Shipment.Code, commissionHeader2.CH0_Product);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Service, OrgCommissionAgreementItemLookups.AllServicesCode, commissionHeader2.CH0_Service);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SubModule, OrgCommissionAgreementItemLookups.AllSubModulesCode, commissionHeader2.CH0_SubModule);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CommissionDate, new ZDateTime(2020, 1, 15), commissionHeader2.CH0_CommissionDate);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_JobNumber, "TEST00001", commissionHeader2.CH0_JobNumber);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotDateTime, new ZDateTime(2003, 3, 3), commissionHeader2.CH0_SnapshotDateTime);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotEventCode, AccCommissionHeaderSnapshotEventList.Codes.JobClosure, commissionHeader2.CH0_SnapshotEventCode);
			});

			AssertEquals(2, commissionHeaders.Length);

			var lineGroupAudChargeWww = commissionHeader1.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeWww.PK);
			CombineAssertions("commission line group properties for charge Www", () =>
			{
				AssertEquals(2000m, lineGroupAudChargeWww.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeWww.CLG_RX_NKTransactionCurrency);
				AssertEquals(2000m, lineGroupAudChargeWww.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeWww.CLG_RX_NKCommissionCurrency);
				AssertEquals(new ZDateTime(2020, 1, 15), lineGroupAudChargeWww.CLG_CommissionDate);
				AssertEquals(1, lineGroupAudChargeWww.Lines.Count);
			});

			var lineGroupAudChargeXxx = commissionHeader1.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeXxx.PK);
			CombineAssertions("commission line group properties for charge Xxx", () =>
			{
				AssertEquals(2500m, lineGroupAudChargeXxx.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeXxx.CLG_RX_NKTransactionCurrency);
				AssertEquals(2500m, lineGroupAudChargeXxx.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeXxx.CLG_RX_NKCommissionCurrency);
				AssertEquals(new ZDateTime(2020, 2, 1), lineGroupAudChargeXxx.CLG_CommissionDate);
				AssertEquals(1, lineGroupAudChargeXxx.Lines.Count);
			});

			var lineGroupAudChargeVvv = commissionHeader2.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeVvv.PK);
			CombineAssertions("commission line group properties for charge Vvv", () =>
			{
				AssertEquals(2600m, lineGroupAudChargeVvv.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeVvv.CLG_RX_NKTransactionCurrency);
				AssertEquals(2600m, lineGroupAudChargeVvv.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeVvv.CLG_RX_NKCommissionCurrency);
				AssertEquals(new ZDateTime(2020, 1, 31), lineGroupAudChargeVvv.CLG_CommissionDate);
				AssertEquals(1, lineGroupAudChargeVvv.Lines.Count);
			});

			lineGroupAudChargeWww = commissionHeader2.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeWww.PK);
			CombineAssertions("commission line group properties for charge Www", () =>
			{
				AssertEquals(2800m, lineGroupAudChargeWww.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeWww.CLG_RX_NKTransactionCurrency);
				AssertEquals(2800m, lineGroupAudChargeWww.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeWww.CLG_RX_NKCommissionCurrency);
				AssertEquals(new ZDateTime(2020, 1, 15), lineGroupAudChargeWww.CLG_CommissionDate);
				AssertEquals(1, lineGroupAudChargeWww.Lines.Count);
			});

			var lineGroupAudChargeYyy = commissionHeader2.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeYyy.PK);
			CombineAssertions("commission line group properties for charge Yyy", () =>
			{
				AssertEquals(3200m, lineGroupAudChargeYyy.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeYyy.CLG_RX_NKTransactionCurrency);
				AssertEquals(3200m, lineGroupAudChargeYyy.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeYyy.CLG_RX_NKCommissionCurrency);
				AssertEquals(new ZDateTime(2020, 2, 5), lineGroupAudChargeYyy.CLG_CommissionDate);
				AssertEquals(1, lineGroupAudChargeYyy.Lines.Count);
			});

			var lineGroupAudChargeZzz = commissionHeader2.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeZzz.PK);
			CombineAssertions("commission line group properties for charge Zzz", () =>
			{
				AssertEquals(4500m, lineGroupAudChargeZzz.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeZzz.CLG_RX_NKTransactionCurrency);
				AssertEquals(4500m, lineGroupAudChargeZzz.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeZzz.CLG_RX_NKCommissionCurrency);
				AssertEquals(new ZDateTime(2020, 2, 10), lineGroupAudChargeZzz.CLG_CommissionDate);
				AssertEquals(1, lineGroupAudChargeZzz.Lines.Count);
			});

			AssertEquals("Should have grouped the transaction lines into charge code", 2, commissionHeader1.LineGroups.Count);
			AssertEquals("Should have grouped the transaction lines into charge code", 4, commissionHeader2.LineGroups.Count);
		}

		public void TestCreateCommissions_AccountingRevenueRecognition_OverwriteItems()
		{
			SetupTimePeriods();
			OrganisationsDataRegistry.Instance.CommissionRecognitionDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CommissionRecognitionDateTypeList.Codes.AccountingRevenueRecognition);
			SetupRevenueRecognitionRegistry();

			var chargeXxx = Factory.NewWithValidTestData<AccChargeCode>();
			chargeXxx.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";

			var agreement = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAirShipment(Factory, customer);
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			agreement.CA0_EffectiveDate = new ZDate(2020, 2, 1);

			var recipient1 = agreement.Recipients.AddNew();
			recipient1.CAR_GS_NKStaff = "ADL";
			recipient1.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient1.CAR_Share = 1;
			var recipientRate1 = recipient1.Rates.AddNew();
			recipientRate1.CAT_CommissionPercentage = 100;
			recipientRate1.CAT_CommissionEndDate = new ZDate(2020, 4, 2);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_E_DEP = new ZDateTime(2020, 2, 10);
			shipment.JS_E_ARV = new ZDateTime(2020, 2, 15);
			shipment.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2020, 2, 18);
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2020, 2, 19);
			shipment.JS_UniqueConsignRef = "TEST00001";

			var job = new Job.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = customer.MainAddress.PK;
			var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_JH = job.PK;
			invoice1.AH_RX_NKTransactionCurrency = "AUD";
			invoice1.AH_PostDate = new ZDateTime(2020, 1, 31);

			var arLine1 = TestObjectCreator.CreateARInvoiceLine(invoice1, job, chargeXxx, audCurrency, 1, "", 2500);
			arLine1.AL_LineType = TransactionLineTypes.Revenue;

			foreach (TransactionLine line in invoice1.Lines)
			{
				TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, line.TransactionCurrency);
			}

			Factory.Save();

			var commissionCreator = new JobRelatedTransactionCommissionCreator(invoice1, Tuple.Create<IJobHeader, ZDateTime>(job, new ZDateTime(2020, 3, 3)));
			var context = new CreateCommissionContext() { OverwriteOldValues = true };
			commissionCreator.CreateCommissions(context);

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, new ZGuid[] { invoice1.PK }));
			AssertEquals(1, commissionHeaders.Length);
			var commissionHeader1 = commissionHeaders.Single(c => c.CH0_AH_Source == invoice1.PK);
			CombineAssertions("commissionHeader 1 Properties", () =>
			{
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GC, invoice1.AH_GC, commissionHeader1.CH0_GC);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceTableCode, job.TablePrefix, commissionHeader1.CH0_GroupingSourceTableCode);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceID, job.PK, commissionHeader1.CH0_GroupingSourceID);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CA0, agreement.PK, commissionHeader1.CH0_CA0);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OH_Customer, customer.PK, commissionHeader1.CH0_OH_Customer);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Product, JobInvoicingConsumerTypes.Shipment.Code, commissionHeader1.CH0_Product);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Service, OrgCommissionAgreementItemLookups.AllServicesCode, commissionHeader1.CH0_Service);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SubModule, OrgCommissionAgreementItemLookups.AllSubModulesCode, commissionHeader1.CH0_SubModule);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CommissionDate, new ZDateTime(2020, 2, 10), commissionHeader1.CH0_CommissionDate);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_JobNumber, "TEST00001", commissionHeader1.CH0_JobNumber);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotDateTime, new ZDateTime(2020, 3, 3), commissionHeader1.CH0_SnapshotDateTime);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotEventCode, AccCommissionHeaderSnapshotEventList.Codes.JobClosure, commissionHeader1.CH0_SnapshotEventCode);
			});

			var lineGroupAudChargeXxx = commissionHeader1.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeXxx.PK);
			CombineAssertions("commission line group for charge Xxx", () =>
			{
				AssertEquals(2500m, lineGroupAudChargeXxx.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeXxx.CLG_RX_NKTransactionCurrency);
				AssertEquals(2500m, lineGroupAudChargeXxx.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeXxx.CLG_RX_NKCommissionCurrency);
				AssertEquals(new ZDateTime(2020, 2, 10), lineGroupAudChargeXxx.CLG_CommissionDate);
				AssertEquals(1, lineGroupAudChargeXxx.Lines.Count);
			});

			AssertEquals("Should have grouped the transaction lines into charge code", 1, commissionHeader1.LineGroups.Count);

			agreement.CA0_ExpiredDate = new ZDate(2020, 2, 3);
			Factory.Save();

			commissionCreator.CreateCommissions(context);

			commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, new ZGuid[] { invoice1.PK }));
			AssertEquals(1, commissionHeaders.Length);
			commissionHeader1 = commissionHeaders.Single(c => c.CH0_AH_Source == invoice1.PK);
			AssertEquals(1, commissionHeader1.LineGroups.Count);
			AssertEquals(2, commissionHeader1.LineGroups[0].Lines.Count);
			AssertEquals("First line should be overwritten", true, commissionHeader1.LineGroups[0].Lines[0].IsOverriden);
			AssertEquals("Second line should be overwritten", true, commissionHeader1.LineGroups[0].Lines[1].IsOverriden);
		}

		[TestDate(2020, 2, 20)]
		public void TestCreateCommissions_AccountingRevenueRecognition_JobClosureRecognition()
		{
			SetupTimePeriods();
			OrganisationsDataRegistry.Instance.CommissionRecognitionDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CommissionRecognitionDateTypeList.Codes.AccountingRevenueRecognition);
			SetupRevenueRecognitionRegistry();

			var chargeXxx = Factory.NewWithValidTestData<AccChargeCode>();
			chargeXxx.AC_ChargeGroup = ChargeCodeGroupList.Codes.Insurance;
			var chargeYyy = Factory.NewWithValidTestData<AccChargeCode>();
			chargeYyy.AC_ChargeGroup = ChargeCodeGroupList.Codes.CFSShipment;

			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";

			var agreement = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAirShipment(Factory, customer);
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			var recipient1 = agreement.Recipients.AddNew();
			recipient1.CAR_GS_NKStaff = "ADL";
			recipient1.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient1.CAR_Share = 1;
			var recipientRate1 = recipient1.Rates.AddNew();
			recipientRate1.CAT_CommissionPercentage = 100;
			recipientRate1.CAT_CommissionEndDate = new ZDate(2020, 3, 2);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_E_DEP = new ZDateTime(2020, 2, 1);
			shipment.JS_E_ARV = new ZDateTime(2020, 2, 5);
			shipment.JS_UniqueConsignRef = "TEST00001";

			var job = new Job.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = customer.MainAddress.PK;
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_JH = job.PK;
			invoice.AH_RX_NKTransactionCurrency = "AUD";
			invoice.AH_PostDate = new ZDateTime(2020, 1, 31);
			var arLine1 = TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, audCurrency, 1, "", 2500);
			arLine1.AL_LineType = TransactionLineTypes.Revenue;

			var arLine2 = TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, audCurrency, 1, "", 3200);
			arLine2.AL_LineType = TransactionLineTypes.Revenue;

			foreach (TransactionLine line in invoice.Lines)
			{
				TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, line.TransactionCurrency);
			}

			Factory.Save();

			job.RefreshCharges();

			job.JH_A_JCL = new ZDateTime(2020, 2, 20);
			job.JH_Status = JobHeaderStatus.Closed.Code;

			var commissionCreator = new JobRelatedTransactionCommissionCreator(invoice, Tuple.Create<IJobHeader, ZDateTime>(job, new ZDateTime(2003, 3, 3)));
			commissionCreator.CreateCommissions();

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK));
			AssertEquals(1, commissionHeaders.Length);
			var commissionHeader = commissionHeaders[0];
			CombineAssertions("commissionHeader Properties", () =>
			{
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GC, invoice.AH_GC, commissionHeader.CH0_GC);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceTableCode, job.TablePrefix, commissionHeader.CH0_GroupingSourceTableCode);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceID, job.PK, commissionHeader.CH0_GroupingSourceID);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CA0, agreement.PK, commissionHeader.CH0_CA0);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OH_Customer, customer.PK, commissionHeader.CH0_OH_Customer);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Product, JobInvoicingConsumerTypes.Shipment.Code, commissionHeader.CH0_Product);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Service, OrgCommissionAgreementItemLookups.AllServicesCode, commissionHeader.CH0_Service);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SubModule, OrgCommissionAgreementItemLookups.AllSubModulesCode, commissionHeader.CH0_SubModule);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CommissionDate, new ZDateTime(2020, 2, 5), commissionHeader.CH0_CommissionDate);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_JobNumber, "TEST00001", commissionHeader.CH0_JobNumber);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotDateTime, new ZDateTime(2003, 3, 3), commissionHeader.CH0_SnapshotDateTime);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotEventCode, AccCommissionHeaderSnapshotEventList.Codes.JobClosure, commissionHeader.CH0_SnapshotEventCode);
			});
			AssertEquals(1, commissionHeaders.Length);
			var lineGroupAudChargeXxx = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeXxx.PK);
			CombineAssertions("commission line group properties for charge Xxx", () =>
			{
				AssertEquals(2500m, lineGroupAudChargeXxx.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeXxx.CLG_RX_NKTransactionCurrency);
				AssertEquals(2500m, lineGroupAudChargeXxx.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeXxx.CLG_RX_NKCommissionCurrency);
				AssertEquals(new ZDateTime(2020, 2, 20), lineGroupAudChargeXxx.CLG_CommissionDate);
				AssertEquals(1, lineGroupAudChargeXxx.Lines.Count);
			});

			var lineGroupAudChargeYyy = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeYyy.PK);
			CombineAssertions("commission line group properties for charge Yyy", () =>
			{
				AssertEquals(3200m, lineGroupAudChargeYyy.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeYyy.CLG_RX_NKTransactionCurrency);
				AssertEquals(3200m, lineGroupAudChargeYyy.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeYyy.CLG_RX_NKCommissionCurrency);
				AssertEquals(new ZDateTime(2020, 2, 5), lineGroupAudChargeYyy.CLG_CommissionDate);
				AssertEquals(1, lineGroupAudChargeYyy.Lines.Count);
			});

			AssertEquals("Should have grouped the transaction lines into charge code", 2, commissionHeader.LineGroups.Count);
		}

		void SetupTimePeriods()
		{
			TestCaseHelper.ClearTable(MasterFiles.Business.AccPeriodManagement.Schema.TableName);
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();

			testHelper.SetupSinglePeriod(202001, new ZDateTime(2020, 1, 1), new ZDateTime(2020, 1, 31));
			testHelper.SetupSinglePeriod(202002, new ZDateTime(2020, 2, 1), new ZDateTime(2020, 2, 29));
			testHelper.SetupSinglePeriod(202003, new ZDateTime(2020, 3, 1), new ZDateTime(2020, 3, 31));
		}

		void SetupRevenueRecognitionRegistry()
		{
			RevenueRecognitionByChargeGroupCollection revenueRecognitionByChargeGroupCollection = AccountingConfigurationRegistry.Instance.RevenueRecognitionByChargeGroupSetup.Value;
			foreach (RevenueRecognitionByChargeGroup revenueRecognitionByChargeGroup in revenueRecognitionByChargeGroupCollection)
			{
				if (revenueRecognitionByChargeGroup.ChargeGroup == ChargeCodeGroupList.Codes.Brokerage)
				{
					SetRevenueRecognition(revenueRecognitionByChargeGroup.ChargeGroupSettings, RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate);
				}
				if (revenueRecognitionByChargeGroup.ChargeGroup == ChargeCodeGroupList.Codes.Destination)
				{
					SetRevenueRecognition(revenueRecognitionByChargeGroup.ChargeGroupSettings, RevenueRecognitionLookups.RecognitionDateOptionCodes.EstimatedDepartureDate);
					SetRevenueRecognitionForTransport(revenueRecognitionByChargeGroup.ChargeGroupSettings, RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate);
				}
				if (revenueRecognitionByChargeGroup.ChargeGroup == ChargeCodeGroupList.Codes.Freight)
				{
					SetRevenueRecognition(revenueRecognitionByChargeGroup.ChargeGroupSettings, RevenueRecognitionLookups.RecognitionDateOptionCodes.EstimatedDepartureDate);
				}
				if (revenueRecognitionByChargeGroup.ChargeGroup == ChargeCodeGroupList.Codes.CFSShipment)
				{
					SetRevenueRecognition(revenueRecognitionByChargeGroup.ChargeGroupSettings, RevenueRecognitionLookups.RecognitionDateOptionCodes.EstimatedArrivalDate);
				}
				if (revenueRecognitionByChargeGroup.ChargeGroup == ChargeCodeGroupList.Codes.CustomsDuty)
				{
					SetRevenueRecognition(revenueRecognitionByChargeGroup.ChargeGroupSettings, RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate);
				}
				if (revenueRecognitionByChargeGroup.ChargeGroup == ChargeCodeGroupList.Codes.Insurance)
				{
					SetRevenueRecognition(revenueRecognitionByChargeGroup.ChargeGroupSettings, RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure);
					SetRevenueRecognitionForTransport(revenueRecognitionByChargeGroup.ChargeGroupSettings, RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure);
				}
			}
			AccountingConfigurationRegistry.Instance.RevenueRecognitionByChargeGroupSetup.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty, revenueRecognitionByChargeGroupCollection);
		}

		void SetRevenueRecognition(RevenueRecognitionCollection collection, ZString optionCode)
		{
			var revenueRecognition = (RevenueRecognition)collection.FirstOrDefault(s => ((RevenueRecognition)s).JobType == JobInvoicingConsumerTypes.Shipment.Code);
			if (revenueRecognition == null)
			{
				revenueRecognition = collection.AddNew();
				revenueRecognition.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			}

			revenueRecognition.DirectionCode = Core.Constants.FreightShipmentDirection.Code.All;
			revenueRecognition.Mode = Core.Constants.TransportModes.Air;
			revenueRecognition.RecognitionDateOptionCode = optionCode;
			revenueRecognition.Offset = 0;
			revenueRecognition.OffsetType = JobConfigurationSelectorHelper.OffsetTypeCodes.Days;
		}

		void SetRevenueRecognitionForTransport(RevenueRecognitionCollection collection, ZString optionCode)
		{
			var revenueRecognition = (RevenueRecognition)collection.FirstOrDefault(s => ((RevenueRecognition)s).JobType == JobInvoicingConsumerTypes.LocalCartage.Code);
			if (revenueRecognition == null)
			{
				revenueRecognition = collection.AddNew();
				revenueRecognition.JobType = JobInvoicingConsumerTypes.LocalCartage.Code;
			}

			revenueRecognition.RecognitionDateOptionCode = optionCode;
			revenueRecognition.Offset = 0;
			revenueRecognition.OffsetType = JobConfigurationSelectorHelper.OffsetTypeCodes.Days;
		}

		public void TestCreateCommissions_OnJobClosure_WrongMode()
		{
			var chargeXxx = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeYyy = Factory.NewWithValidTestData<AccChargeCode>();
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";
			var scwStaff = Factory.NewWithValidTestData<GlbStaff>();
			scwStaff.GS_Code = "SCW";

			var agreement = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAirShipment(Factory, customer);
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			var recipient1 = agreement.Recipients.AddNew();
			recipient1.CAR_GS_NKStaff = "ADL";
			recipient1.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient1.CAR_Share = 1;
			var recipientRate1 = recipient1.Rates.AddNew();
			recipientRate1.CAT_CommissionPercentage = 50;

			var recipient2 = agreement.Recipients.AddNew();
			recipient2.CAR_OH_Party = org.PK;
			recipient2.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient2.CAR_Share = 3;
			var recipientRate2 = recipient2.Rates.AddNew();
			recipientRate2.CAT_CommissionPercentage = 10;

			var recipient3 = agreement.Recipients.AddNew();
			recipient3.CAR_GS_NKStaff = "SCW";
			recipient3.CAR_CommissionType = CommissionTypes.Codes.FIX;
			recipient3.CAR_Share = 1;
			var recipientRate3 = recipient3.Rates.AddNew();
			recipientRate3.CAT_CommissionAmount = 100;
			recipientRate3.CAT_RX_NKCommissionCurrency = "AUD";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "ROA";

			var job = new Job.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = customer.MainAddress.PK;
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_JH = job.PK;
			invoice.AH_RX_NKTransactionCurrency = "USD";
			invoice.AH_PostDate = new ZDateTime(2002, 2, 2);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, audCurrency, 1, "", 10);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, audCurrency, 1, "", 100);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, audCurrency, 1, "", 1000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, usdCurrency, 0.5m, "", 10000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, usdCurrency, 0.5m, "", 100000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, usdCurrency, 0.5m, "", 1000000);

			foreach (TransactionLine line in invoice.Lines)
			{
				TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, line.TransactionCurrency);
			}

			Factory.Save();

			var commissionCreator = new JobRelatedTransactionCommissionCreator(invoice, Tuple.Create<IJobHeader, ZDateTime>(job, new ZDateTime(2003, 3, 3)));
			commissionCreator.CreateCommissions();

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK));
			AssertEquals(0, commissionHeaders.Length);
		}

		public void TestCreateCommissions_OnInvoicePost()
		{
			OrganisationsDataRegistry.Instance.CommissionTransactionJobTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CommissionTransactionJobTriggerList.Codes.RevenueCommissionCalculationsToBeCreatedAtInvStatus);

			var chargeXxx = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeYyy = Factory.NewWithValidTestData<AccChargeCode>();
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";
			var scwStaff = Factory.NewWithValidTestData<GlbStaff>();
			scwStaff.GS_Code = "SCW";

			var agreement = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAllItems(Factory, customer);
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			var recipient1 = agreement.Recipients.AddNew();
			recipient1.CAR_GS_NKStaff = "ADL";
			recipient1.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient1.CAR_Share = 1;
			var recipientRate1 = recipient1.Rates.AddNew();
			recipientRate1.CAT_CommissionPercentage = 50;

			var recipient2 = agreement.Recipients.AddNew();
			recipient2.CAR_OH_Party = org.PK;
			recipient2.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient2.CAR_Share = 3;
			var recipientRate2 = recipient2.Rates.AddNew();
			recipientRate2.CAT_CommissionPercentage = 10;

			var recipient3 = agreement.Recipients.AddNew();
			recipient3.CAR_GS_NKStaff = "SCW";
			recipient3.CAR_CommissionType = CommissionTypes.Codes.FIX;
			recipient3.CAR_Share = 1;
			var recipientRate3 = recipient3.Rates.AddNew();
			recipientRate3.CAT_CommissionAmount = 100;
			recipientRate3.CAT_RX_NKCommissionCurrency = "AUD";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "TEST00001";
			var job = new Job.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = customer.MainAddress.PK;
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_JH = job.PK;
			invoice.AH_RX_NKTransactionCurrency = "USD";
			invoice.AH_PostDate = new ZDateTime(2002, 2, 2);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, audCurrency, 1, "", 10);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, audCurrency, 1, "", 100);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, audCurrency, 1, "", 1000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, usdCurrency, 0.5m, "", 10000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, usdCurrency, 0.5m, "", 100000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, usdCurrency, 0.5m, "", 1000000);

			foreach (TransactionLine line in invoice.Lines)
			{
				TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, line.TransactionCurrency);
			}

			Factory.Save();

			var commissionCreator = new JobRelatedTransactionCommissionCreator(invoice, null);
			commissionCreator.CreateCommissions();
			AssertEquals("Should only create commission for job related invoices on job closure or if agreement is revenue based", 0, Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK)).Length);

			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.REV;
			Factory.Save();

			commissionCreator = new JobRelatedTransactionCommissionCreator(invoice, null);
			commissionCreator.CreateCommissions();

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK));
			var commissionHeader = commissionHeaders[0];
			CombineAssertions("commissionHeader Properties", () =>
			{
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GC, invoice.AH_GC, commissionHeader.CH0_GC);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceTableCode, job.TablePrefix, commissionHeader.CH0_GroupingSourceTableCode);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceID, job.PK, commissionHeader.CH0_GroupingSourceID);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CA0, agreement.PK, commissionHeader.CH0_CA0);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OH_Customer, customer.PK, commissionHeader.CH0_OH_Customer);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Product, JobInvoicingConsumerTypes.Shipment.Code, commissionHeader.CH0_Product);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Service, OrgCommissionAgreementItemLookups.AllServicesCode, commissionHeader.CH0_Service);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SubModule, OrgCommissionAgreementItemLookups.AllSubModulesCode, commissionHeader.CH0_SubModule);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_JobNumber, "TEST00001", commissionHeader.CH0_JobNumber);

				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotDateTime, new ZDateTime(2002, 2, 2), commissionHeader.CH0_SnapshotDateTime);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotEventCode, AccCommissionHeaderSnapshotEventList.Codes.Posted, commissionHeader.CH0_SnapshotEventCode);
			});
			AssertEquals(1, commissionHeaders.Length);
			{
				var lineGroupAudChargeXxx = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeXxx.PK);
				AssertEquals(110m, lineGroupAudChargeXxx.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeXxx.CLG_RX_NKTransactionCurrency);
				AssertEquals(110m, lineGroupAudChargeXxx.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeXxx.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupAudChargeXxx.Lines.Count);
			}

			{
				var lineGroupUsdChargeXxx = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "USD" && x.CLG_AC == chargeXxx.PK);
				AssertEquals(10000m, lineGroupUsdChargeXxx.CLG_TransactionAmount);
				AssertEquals("USD", lineGroupUsdChargeXxx.CLG_RX_NKTransactionCurrency);
				AssertEquals(20000m, lineGroupUsdChargeXxx.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupUsdChargeXxx.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupUsdChargeXxx.Lines.Count);
			}

			{
				var lineGroupAudChargeYyy = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeYyy.PK);
				AssertEquals(1000m, lineGroupAudChargeYyy.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeYyy.CLG_RX_NKTransactionCurrency);
				AssertEquals(1000m, lineGroupAudChargeYyy.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeYyy.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupAudChargeYyy.Lines.Count);
			}

			{
				var lineGroupUsdChargeYyy = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "USD" && x.CLG_AC == chargeYyy.PK);
				AssertEquals(1100000m, lineGroupUsdChargeYyy.CLG_TransactionAmount);
				AssertEquals("USD", lineGroupUsdChargeYyy.CLG_RX_NKTransactionCurrency);
				AssertEquals(2200000m, lineGroupUsdChargeYyy.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupUsdChargeYyy.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupUsdChargeYyy.Lines.Count);
			}

			AssertEquals("Should have grouped the transaction lines into charge code and invoice currency pairs", 4, commissionHeader.LineGroups.Count);

			AssertEquals("Should have 1 fixed commission line", 1, commissionHeader.Lines.Count);
		}

		public void TestCreateCommissions_OnInvoicePost_Mode()
		{
			OrganisationsDataRegistry.Instance.CommissionTransactionJobTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CommissionTransactionJobTriggerList.Codes.RevenueCommissionCalculationsToBeCreatedAtInvStatus);

			var chargeXxx = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeYyy = Factory.NewWithValidTestData<AccChargeCode>();
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";
			var scwStaff = Factory.NewWithValidTestData<GlbStaff>();
			scwStaff.GS_Code = "SCW";

			var agreement = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAirShipment(Factory, customer);
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			var recipient1 = agreement.Recipients.AddNew();
			recipient1.CAR_GS_NKStaff = "ADL";
			recipient1.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient1.CAR_Share = 1;
			var recipientRate1 = recipient1.Rates.AddNew();
			recipientRate1.CAT_CommissionPercentage = 50;

			var recipient2 = agreement.Recipients.AddNew();
			recipient2.CAR_OH_Party = org.PK;
			recipient2.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient2.CAR_Share = 3;
			var recipientRate2 = recipient2.Rates.AddNew();
			recipientRate2.CAT_CommissionPercentage = 10;

			var recipient3 = agreement.Recipients.AddNew();
			recipient3.CAR_GS_NKStaff = "SCW";
			recipient3.CAR_CommissionType = CommissionTypes.Codes.FIX;
			recipient3.CAR_Share = 1;
			var recipientRate3 = recipient3.Rates.AddNew();
			recipientRate3.CAT_CommissionAmount = 100;
			recipientRate3.CAT_RX_NKCommissionCurrency = "AUD";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_UniqueConsignRef = "TEST00001";

			var job = new Job.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = customer.MainAddress.PK;
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_JH = job.PK;
			invoice.AH_RX_NKTransactionCurrency = "USD";
			invoice.AH_PostDate = new ZDateTime(2002, 2, 2);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, audCurrency, 1, "", 10);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, audCurrency, 1, "", 100);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, audCurrency, 1, "", 1000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, usdCurrency, 0.5m, "", 10000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, usdCurrency, 0.5m, "", 100000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, usdCurrency, 0.5m, "", 1000000);

			foreach (TransactionLine line in invoice.Lines)
			{
				TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, line.TransactionCurrency);
			}

			Factory.Save();

			var commissionCreator = new JobRelatedTransactionCommissionCreator(invoice, null);
			commissionCreator.CreateCommissions();
			AssertEquals("Should only create commission for job related invoices on job closure or if agreement is revenue based", 0, Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK)).Length);

			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.REV;
			Factory.Save();

			commissionCreator = new JobRelatedTransactionCommissionCreator(invoice, null);
			commissionCreator.CreateCommissions();

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK));
			AssertEquals(1, commissionHeaders.Length);
			var commissionHeader = commissionHeaders[0];
			CombineAssertions("commissionHeader Properties", () =>
			{
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GC, invoice.AH_GC, commissionHeader.CH0_GC);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceTableCode, job.TablePrefix, commissionHeader.CH0_GroupingSourceTableCode);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceID, job.PK, commissionHeader.CH0_GroupingSourceID);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CA0, agreement.PK, commissionHeader.CH0_CA0);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OH_Customer, customer.PK, commissionHeader.CH0_OH_Customer);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Product, JobInvoicingConsumerTypes.Shipment.Code, commissionHeader.CH0_Product);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Service, OrgCommissionAgreementItemLookups.AllServicesCode, commissionHeader.CH0_Service);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SubModule, OrgCommissionAgreementItemLookups.AllSubModulesCode, commissionHeader.CH0_SubModule);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_JobNumber, "TEST00001", commissionHeader.CH0_JobNumber);

				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotDateTime, new ZDateTime(2002, 2, 2), commissionHeader.CH0_SnapshotDateTime);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotEventCode, AccCommissionHeaderSnapshotEventList.Codes.Posted, commissionHeader.CH0_SnapshotEventCode);
			});
			AssertEquals(1, commissionHeaders.Length);
			{
				var lineGroupAudChargeXxx = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeXxx.PK);
				AssertEquals(110m, lineGroupAudChargeXxx.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeXxx.CLG_RX_NKTransactionCurrency);
				AssertEquals(110m, lineGroupAudChargeXxx.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeXxx.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupAudChargeXxx.Lines.Count);
			}

			{
				var lineGroupUsdChargeXxx = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "USD" && x.CLG_AC == chargeXxx.PK);
				AssertEquals(10000m, lineGroupUsdChargeXxx.CLG_TransactionAmount);
				AssertEquals("USD", lineGroupUsdChargeXxx.CLG_RX_NKTransactionCurrency);
				AssertEquals(20000m, lineGroupUsdChargeXxx.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupUsdChargeXxx.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupUsdChargeXxx.Lines.Count);
			}

			{
				var lineGroupAudChargeYyy = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "AUD" && x.CLG_AC == chargeYyy.PK);
				AssertEquals(1000m, lineGroupAudChargeYyy.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupAudChargeYyy.CLG_RX_NKTransactionCurrency);
				AssertEquals(1000m, lineGroupAudChargeYyy.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupAudChargeYyy.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupAudChargeYyy.Lines.Count);
			}

			{
				var lineGroupUsdChargeYyy = commissionHeader.LineGroups.Single(x => x.CLG_RX_NKTransactionCurrency == "USD" && x.CLG_AC == chargeYyy.PK);
				AssertEquals(1100000m, lineGroupUsdChargeYyy.CLG_TransactionAmount);
				AssertEquals("USD", lineGroupUsdChargeYyy.CLG_RX_NKTransactionCurrency);
				AssertEquals(2200000m, lineGroupUsdChargeYyy.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupUsdChargeYyy.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupUsdChargeYyy.Lines.Count);
			}

			AssertEquals("Should have grouped the transaction lines into charge code and invoice currency pairs", 4, commissionHeader.LineGroups.Count);

			AssertEquals("Should have 1 fixed commission line", 1, commissionHeader.Lines.Count);

			var jrj = TestObjectCreator.CreateJobRevenueJournal(chargeXxx, job, 100);
			jrj.JournalLines[0].AL_GE = jrj.JournalLines[1].AL_GE = GlbDepartment.CurrentDepartment.PK;
			jrj.AH_RX_NKTransactionCurrency = "AUD";
			jrj.AH_PostDate = new ZDateTime(2002, 2, 2);
			Factory.Save();

			var jrjCommissionCreator = new JobRelatedTransactionCommissionCreator(jrj, Tuple.Create<IJobHeader, ZDateTime>(job, new ZDateTime(2003, 3, 3)));
			jrjCommissionCreator.CreateCommissions();

			var jrjCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, jrj.PK));
			AssertEquals(0, jrjCommissionHeaders.Length);
		}

		public void TestCreateCommissions_OnInvoicePost_WrongMode()
		{
			var chargeXxx = Factory.NewWithValidTestData<AccChargeCode>();
			var chargeYyy = Factory.NewWithValidTestData<AccChargeCode>();
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";
			var scwStaff = Factory.NewWithValidTestData<GlbStaff>();
			scwStaff.GS_Code = "SCW";

			var agreement = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAirShipment(Factory, customer);
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			var recipient1 = agreement.Recipients.AddNew();
			recipient1.CAR_GS_NKStaff = "ADL";
			recipient1.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient1.CAR_Share = 1;
			var recipientRate1 = recipient1.Rates.AddNew();
			recipientRate1.CAT_CommissionPercentage = 50;

			var recipient2 = agreement.Recipients.AddNew();
			recipient2.CAR_OH_Party = org.PK;
			recipient2.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient2.CAR_Share = 3;
			var recipientRate2 = recipient2.Rates.AddNew();
			recipientRate2.CAT_CommissionPercentage = 10;

			var recipient3 = agreement.Recipients.AddNew();
			recipient3.CAR_GS_NKStaff = "SCW";
			recipient3.CAR_CommissionType = CommissionTypes.Codes.FIX;
			recipient3.CAR_Share = 1;
			var recipientRate3 = recipient3.Rates.AddNew();
			recipientRate3.CAT_CommissionAmount = 100;
			recipientRate3.CAT_RX_NKCommissionCurrency = "AUD";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";

			var job = new Job.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = customer.MainAddress.PK;
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_JH = job.PK;
			invoice.AH_RX_NKTransactionCurrency = "USD";
			invoice.AH_PostDate = new ZDateTime(2002, 2, 2);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, audCurrency, 1, "", 10);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, audCurrency, 1, "", 100);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, audCurrency, 1, "", 1000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeXxx, usdCurrency, 0.5m, "", 10000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, usdCurrency, 0.5m, "", 100000);
			TestObjectCreator.CreateARInvoiceLine(invoice, job, chargeYyy, usdCurrency, 0.5m, "", 1000000);

			foreach (TransactionLine line in invoice.Lines)
			{
				TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, line.TransactionCurrency);
			}

			Factory.Save();

			var commissionCreator = new JobRelatedTransactionCommissionCreator(invoice, null);
			commissionCreator.CreateCommissions();
			AssertEquals("Should only create commission for job related invoices on job closure or if agreement is revenue based", 0, Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK)).Length);

			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.REV;
			Factory.Save();

			commissionCreator = new JobRelatedTransactionCommissionCreator(invoice, null);
			commissionCreator.CreateCommissions();

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK));
			AssertEquals(0, commissionHeaders.Length);
		}

		public void TestCreateCommissions_WithDoNotOverwriteOldValuesOption()
		{
			OrganisationsDataRegistry.Instance.CommissionTransactionJobTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CommissionTransactionJobTriggerList.Codes.RevenueCommissionCalculationsToBeCreatedAtInvStatus);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";

			var agreement = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAllItems(Factory, customer);
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.REV;
			var agreementRecipient = agreement.Recipients.AddNew();
			agreementRecipient.CAR_GS_NKStaff = "ADL";
			agreementRecipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			agreementRecipient.CAR_Share = 1;
			var recipientRate = agreementRecipient.Rates.AddNew();
			recipientRate.CAT_CommissionPercentage = 50;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new Job.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = customer.MainAddress.PK;
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_JH = job.PK;
			invoice.AH_RX_NKTransactionCurrency = "USD";
			TestObjectCreator.CreateARInvoiceLine(invoice, job, Factory.LoadTop1<AccChargeCode>(new ZQuery()), Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD"), 1, "", 10);

			foreach (TransactionLine line in invoice.Lines)
			{
				TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, line.TransactionCurrency);
			}

			Factory.Save();

			var commissionCreator = new JobRelatedTransactionCommissionCreator(invoice, null);
			var context = new CreateCommissionContext() { OverwriteOldValues = false };

			commissionCreator.CreateCommissions(context);
			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK));
			AssertEquals(1, commissionHeaders.Length);

			commissionCreator.CreateCommissions(context);
			commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK));
			AssertEquals("Should not have created another", 1, commissionHeaders.Length);
		}

		[TestDate(2002, 2, 2)]
		public void TestCreateCommissions_WithOverwriteOldValuesOption()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";

			var agreement = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAllItems(Factory, customer);
			agreement.CA0_LastApprovedDateUtc = new ZDateTime(2002, 2, 2);
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.REV;
			var agreementRecipient = agreement.Recipients.AddNew();
			agreementRecipient.CAR_GS_NKStaff = "ADL";
			agreementRecipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			agreementRecipient.CAR_Share = 1;
			var recipientRate = agreementRecipient.Rates.AddNew();
			recipientRate.CAT_CommissionPercentage = 50;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new JobHeader.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = customer.MainAddress.PK;
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = customer.PK;
			invoice.AH_JH = job.PK;
			invoice.AH_RX_NKTransactionCurrency = "USD";
			TestObjectCreator.CreateARInvoiceLine(invoice, null, Factory.LoadTop1<AccChargeCode>(new ZQuery()), Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD"), 1, "", 10);

			Factory.Save();

			var commissionCreator = new JobRelatedTransactionCommissionCreator(invoice, null);
			var context = new CreateCommissionContext() { OverwriteOldValues = true };

			commissionCreator.CreateCommissions(context);
			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK));
			AssertEquals(1, commissionHeaders.Length);
			var firstCommissionHeader = commissionHeaders[0];

			agreementRecipient.CAR_Share = 2;
			agreement.CA0_LastApprovedDateUtc = new ZDateTime(2003, 3, 3);
			Factory.Save();

			commissionCreator.CreateCommissions(context);
			commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK));
			AssertEquals("Should have created another commission header", 2, commissionHeaders.Length);
			AssertEquals("Should have overriden the first commission header", true, firstCommissionHeader.IsOverriden);

			var secondCommissionHeader = commissionHeaders.First(x => x.PK != firstCommissionHeader.PK);
			AssertEquals("Should not have overriden the second commission header", false, secondCommissionHeader.IsOverriden);
			AssertEquals("New commission header should have the updated share amount", (ZByte)2, secondCommissionHeader.LineGroups[0].Lines[0].CL0_SharePortion);
		}

		public void TestCreateCommissions_ChangeJobLocalClient()
		{
			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "OTSE"));
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");

			var customer1 = Factory.NewWithValidTestData<OrgHeader>();
			var customer2 = Factory.NewWithValidTestData<OrgHeader>();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SCW";

			var agreement1 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAllItems(Factory, customer1);
			agreement1.CA0_CommissionBasis = CommissionBasisType.Codes.REV;
			agreement1.CA0_LastApprovedDateUtc = new ZDateTime(2002, 2, 2);
			var agreementRecipient1 = agreement1.Recipients.AddNew();
			agreementRecipient1.CAR_GS_NKStaff = "SCW";
			agreementRecipient1.CAR_CommissionType = CommissionTypes.Codes.PCT;
			agreementRecipient1.CAR_Share = 1;
			var recipientRate1 = agreementRecipient1.Rates.AddNew();
			recipientRate1.CAT_CommissionPercentage = 50;

			var agreement2 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAllItems(Factory, customer2);
			agreement2.CA0_CommissionBasis = CommissionBasisType.Codes.REV;
			agreement2.CA0_LastApprovedDateUtc = new ZDateTime(2002, 2, 2);
			var agreementRecipient2 = agreement2.Recipients.AddNew();
			agreementRecipient2.CAR_GS_NKStaff = "SCW";
			agreementRecipient2.CAR_CommissionType = CommissionTypes.Codes.PCT;
			agreementRecipient2.CAR_Share = 1;
			var recipientRate2 = agreementRecipient2.Rates.AddNew();
			recipientRate2.CAT_CommissionPercentage = 30;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new JobHeader.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = customer1.MainAddress.PK;
			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = job.PK;
			jobCharge.JR_LocalSellAmt = 10;
			jobCharge.JR_OSSellAmt = 10;

			var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_OH = customer1.PK;
			invoice1.AH_JH = job.PK;
			invoice1.AH_RX_NKTransactionCurrency = "AUD";
			var line1 = TestObjectCreator.CreateARInvoiceLine(invoice1, null, chargeCode, audCurrency, 1, "Charge 1", 10);
			line1.AL_JH = job.PK;
			jobCharge.JR_AL_ARLine = line1.PK;

			Factory.Save();

			var commissionCreator1 = new JobRelatedTransactionCommissionCreator(invoice1, Tuple.Create<IJobHeader, ZDateTime>(job, new ZDateTime(2003, 3, 3)));
			commissionCreator1.CreateCommissions();

			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice1.PK));
			AssertEquals(1, commissionHeaders.Length);

			invoice1.IsCancelled = true;
			var invoice1Reversing = new ARInvoiceReversing(invoice1);
			invoice1Reversing.Reverse();
			var reversalInvoice1 = invoice1.ReverseInvoice;
			reversalInvoice1.AH_PostDate = new ZDateTime(2003, 3, 3);

			job.JH_OA_LocalChargesAddr = customer2.MainAddress.PK;

			var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2.AH_OH = customer2.PK;
			invoice2.AH_JH = job.PK;
			invoice2.AH_RX_NKTransactionCurrency = "AUD";
			var line2 = TestObjectCreator.CreateARInvoiceLine(invoice2, null, chargeCode, audCurrency, 1, "Charge 2", 20);
			line2.AL_JH = job.PK;
			jobCharge.SetARLineForcedForTest(line2.PK);
			jobCharge.JR_LocalSellAmt = 20;
			jobCharge.JR_OSSellAmt = 20;

			Factory.Save();

			commissionCreator1 = new JobRelatedTransactionCommissionCreator(invoice1, Tuple.Create<IJobHeader, ZDateTime>(job, new ZDateTime(2003, 3, 3)));
			commissionCreator1.CreateCommissions();

			var commissionCreator2 = new JobRelatedTransactionCommissionCreator(invoice2, Tuple.Create<IJobHeader, ZDateTime>(job, new ZDateTime(2003, 3, 3)));
			commissionCreator2.CreateCommissions();

			commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice1.PK));
			AssertEquals(1, commissionHeaders.Length);

			commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, reversalInvoice1.PK));
			AssertEquals(1, commissionHeaders.Length);

			commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice2.PK));
			AssertEquals(1, commissionHeaders.Length);
		}

		public void TestCreateCommissions_RecognitionDate_FirstARTransation()
		{
			OrganisationsDataRegistry.Instance.CommissionRecognitionDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CommissionRecognitionDateTypeList.Codes.PostDateOfFirstArTransaction);
			var arPostDate = new ZDateTime(2017, 6, 15);
			var apPostDate = new ZDateTime(2017, 6, 1);
			AssertRecognitionDate(arPostDate, apPostDate, arPostDate, arPostDate);
			AssertRecognitionDate(arPostDate, apPostDate, arPostDate, arPostDate, true);
		}

		public void TestCreateCommissions_RecognitionDate_Immediate()
		{
			OrganisationsDataRegistry.Instance.CommissionRecognitionDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CommissionRecognitionDateTypeList.Codes.Immediate);
			var arPostDate = new ZDateTime(2017, 6, 15);
			var apPostDate = new ZDateTime(2017, 6, 1);
			AssertRecognitionDate(arPostDate, apPostDate, arPostDate, apPostDate);
			AssertRecognitionDate(arPostDate, apPostDate, arPostDate, apPostDate, true);
		}

		public void TestCreateCommissions_RecognitionDate_AccountingRevenueRecognition()
		{
			OrganisationsDataRegistry.Instance.CommissionRecognitionDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CommissionRecognitionDateTypeList.Codes.AccountingRevenueRecognition);
			var arPostDate = new ZDateTime(2017, 6, 15);
			var apPostDate = new ZDateTime(2017, 6, 1);

			AssertRecognitionDate(ZDateTime.Empty, apPostDate, ZDateTime.Empty, apPostDate);
			AssertRecognitionDate(arPostDate, ZDateTime.Empty, arPostDate, ZDateTime.Empty);

			AssertRecognitionDate(arPostDate, apPostDate, apPostDate, apPostDate);
			AssertRecognitionDate(arPostDate, apPostDate, apPostDate, apPostDate, true);
		}

		void AssertRecognitionDate(ZDateTime arPostDate, ZDateTime apPostDate, ZDateTime expectedArCommissionDate, ZDateTime expectedApComissionDate, bool shouldClearAR_AH_JH = false)
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, "US1") ?? Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "US1";

			var agreement = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAllItems(Factory, customer);
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = staff.GS_Code;
			recipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient.CAR_Share = 1;
			var recipientRate = recipient.Rates.AddNew();
			recipientRate.CAT_CommissionPercentage = 50;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new Job.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = customer.MainAddress.PK;

			ARInvoice arInvoice = null;
			if (arPostDate != ZDateTime.Empty)
			{
				arInvoice = Factory.NewWithValidTestData<ARInvoice>();
				arInvoice.AH_JH = job.PK;
				arInvoice.AH_RX_NKTransactionCurrency = "AUD";
				arInvoice.AH_PostDate = arPostDate;
				TestObjectCreator.CreateARInvoiceLine(arInvoice, job, chargeCode, audCurrency, 1, "", 100);
				foreach (TransactionLine line in arInvoice.Lines)
				{
					TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, line.TransactionCurrency);
				}
			}

			APInvoice apInvoice = null;
			if (apPostDate != ZDateTime.Empty)
			{
				apInvoice = Factory.NewWithValidTestData<APInvoice>();
				apInvoice.AH_JH = job.PK;
				apInvoice.AH_RX_NKTransactionCurrency = "AUD";
				apInvoice.AH_PostDate = apPostDate;
				TestObjectCreator.CreateAPInvoiceLine(apInvoice, job, chargeCode, audCurrency, 1, "", 60);
				foreach (TransactionLine line in apInvoice.Lines)
				{
					TestObjectCreator.CreateJobCharge(line, job, line.ChargeCode, line.TransactionCurrency);
				}
			}

			if (shouldClearAR_AH_JH && arPostDate != ZDateTime.Empty)
			{
				arInvoice.AH_JH = ZGuid.Empty;
			}

			Factory.Save();

			if (shouldClearAR_AH_JH && arPostDate != ZDateTime.Empty)
			{
				AssertEquals(ZGuid.Empty, arInvoice.AH_JH);
				AssertEquals(job.PK, apInvoice.Lines[0].AL_JH);
			}

			if (arPostDate != ZDateTime.Empty)
			{
				var arCommissionCreator = new JobRelatedTransactionCommissionCreator(arInvoice, Tuple.Create<IJobHeader, ZDateTime>(job, new ZDateTime(2017, 6, 19)));
				arCommissionCreator.CreateCommissions();
				var arCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, arInvoice.PK));
				var arCommissionHeader = arCommissionHeaders[0];
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CommissionDate, expectedArCommissionDate, arCommissionHeader.CH0_CommissionDate);
			}

			if (apPostDate != ZDateTime.Empty)
			{
				var apCommissionCreator = new JobRelatedTransactionCommissionCreator(apInvoice, Tuple.Create<IJobHeader, ZDateTime>(job, new ZDateTime(2017, 6, 19)));
				apCommissionCreator.CreateCommissions();
				var apCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, apInvoice.PK));
				var apCommissionHeader = apCommissionHeaders[0];
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CommissionDate, expectedApComissionDate, apCommissionHeader.CH0_CommissionDate);
			}
		}

		public void TestCreateCommissions_SnapshotEventCode_RGN()
		{
			var helper = new CommissionTestObjectCreator(Factory);
			helper.SetupTransactionsAndCommissions(createLinesForJobOnly: true, closeJob: true);

			var headers = Factory.Load<AccCommissionHeader>(new ZQuery());
			var regeneratedHeader = headers.Where(x => x.CH0_SnapshotEventCode == AccCommissionHeaderSnapshotEventList.Codes.Regenerated);

			AssertEquals("Pre-condition", 1, headers.Length);
			AssertEquals("Pre-condition", 0, regeneratedHeader.Count());

			var commissionCreator = new JobRelatedTransactionCommissionCreator(helper.JobInvoice, new Tuple<IJobHeader, ZDateTime>(helper.Job, ZDateTime.Today));
			commissionCreator.CreateCommissions(new CreateCommissionContext() { RegeneratingCommissions = true });

			Factory.Save();

			headers = Factory.Load<AccCommissionHeader>(new ZQuery());
			regeneratedHeader = headers.Where(x => x.CH0_SnapshotEventCode == AccCommissionHeaderSnapshotEventList.Codes.Regenerated);

			AssertEquals("1 original + 1 regenerated", 2, headers.Length);
			AssertEquals("Regenerated header should have SnapshotEventCode == RGN", 1, regeneratedHeader.Count());
		}

		#endregion

		#region Commission Agreement Item

		public void TestGetCommissionableMode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			var job = new JobHeader.Loader(shipment).TryCreate();
			AssertEquals("AIR", JobRelatedTransactionCommissionCreator.GetCommissionableMode(job));
		}

		public void TestGetCommissionableOrigin()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "UAIEV";

			var job = new JobHeader.Loader(shipment).TryCreate();
			AssertEquals("AUSYD", JobRelatedTransactionCommissionCreator.GetCommissionableOrigin(job));
		}

		public void TestGetCommissionableDestination()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "UAIEV";

			var job = new JobHeader.Loader(shipment).TryCreate();
			AssertEquals("UAIEV", JobRelatedTransactionCommissionCreator.GetCommissionableDestination(job));
		}

		public void TestGetCommissionableProductCode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var job = new JobHeader.Loader(shipment).TryCreate();
			AssertEquals(JobInvoicingConsumerTypes.Shipment.Code, JobRelatedTransactionCommissionCreator.GetCommissionableProductCode(job));

			Factory.Save();
			var anotherFactory = new BusinessObjectFactory();
			var jobInOtherFactory = anotherFactory.Load<JobHeader>(job.PK);
			AssertEquals(JobInvoicingConsumerTypes.Shipment.Code, JobRelatedTransactionCommissionCreator.GetCommissionableProductCode(jobInOtherFactory));
		}

		public void TestGetCommissionableServiceCode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var job = new JobHeader.Loader(shipment).TryCreate();
			AssertEquals(OrgCommissionAgreementItemLookups.AllServicesCode, JobRelatedTransactionCommissionCreator.GetCommissionableServiceCode());

			Factory.Save();
			var anotherFactory = new BusinessObjectFactory();
			var jobInOtherFactory = anotherFactory.Load<JobHeader>(job.PK);
			AssertEquals(OrgCommissionAgreementItemLookups.AllServicesCode, JobRelatedTransactionCommissionCreator.GetCommissionableServiceCode());
		}

		public void TestGetCommissionableSubModuleCode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var job = new JobHeader.Loader(shipment).TryCreate();
			AssertEquals(OrgCommissionAgreementItemLookups.AllSubModulesCode, JobRelatedTransactionCommissionCreator.GetCommissionableSubModuleCode());

			Factory.Save();
			var anotherFactory = new BusinessObjectFactory();
			var jobInOtherFactory = anotherFactory.Load<JobHeader>(job.PK);
			AssertEquals(OrgCommissionAgreementItemLookups.AllServicesCode, JobRelatedTransactionCommissionCreator.GetCommissionableSubModuleCode());
		}

		public void TestGetShouldCreateCommissionPredicate()
		{
			var charge = Factory.NewWithValidTestData<AccChargeCode>();
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var customer = Factory.NewWithValidTestData<OrgHeader>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_CMClientCommenced = new ZDate(2000, 1, 1);

			var job = new Job.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = customer.MainAddress.PK;

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_JH = job.PK;
			invoice.AH_RX_NKTransactionCurrency = "USD";
			invoice.AH_PostDate = new ZDateTime(2002, 2, 2);

			var agreement = Factory.New<OrgCommissionAgreement>();
			agreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;
			agreement.CA0_OH_Customer = org.PK;

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			GlbCompany.CurrentCompany.Factory.Save();
			TestObjectCreator.CreateARInvoiceLine(invoice, job, charge, audCurrency, 1, string.Empty, 10);

			var agreementAndRates = new CommissionAgreementAndRatesForTesting { CommissionAgreement = agreement };
			var commissionCreator = new JobRelatedTransactionCommissionCreator(invoice, null);

			OrganisationsDataRegistry.Instance.CommissionTransactionJobTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CommissionTransactionJobTriggerList.Codes.RevenueProfitBasedCommissionCalculationsToBeCreatedAtClsStatus);

			var preCreateCommissionCheck = commissionCreator.GetShouldCreateCommissionPredicate(job);
			Assert(!preCreateCommissionCheck(agreementAndRates));

			OrganisationsDataRegistry.Instance.CommissionTransactionJobTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CommissionTransactionJobTriggerList.Codes.RevenueCommissionCalculationsToBeCreatedAtInvStatus);

			preCreateCommissionCheck = commissionCreator.GetShouldCreateCommissionPredicate(job);
			Assert(preCreateCommissionCheck(agreementAndRates));
		}

		#endregion
	}
}

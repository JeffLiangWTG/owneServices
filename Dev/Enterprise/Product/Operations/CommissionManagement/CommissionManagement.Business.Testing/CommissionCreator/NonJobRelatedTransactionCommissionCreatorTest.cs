using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	public class NonJobRelatedTransactionCommissionCreatorTest : CommissionCreatorTestCase
	{
		#region Constructor

		public void TestConstructor()
		{
			var invoice = Factory.New<ARInvoice>();
			AssertNoExceptionThrown(() =>
			{
				NonJobRelatedTransactionCommissionCreator.New(invoice);
			});

			var invoiceReversing = new ARInvoiceReversing(invoice);
			invoiceReversing.Reverse();
			var reversal = invoice.ReverseInvoice;
			AssertExceptionThrown(typeof(ArgumentException), string.Format("Passed in transaction (PK:{0}) which is a reversal. Use {1} to create commission for reversal transactions", reversal.PK, nameof(ReversalTransactionCommissionCreator)), () =>
			{
				NonJobRelatedTransactionCommissionCreator.New(reversal);
			});

			var job = Factory.NewJobForTesting<Job>();
			var jobInvoice = Factory.New<ARInvoice>();
			jobInvoice.AH_JH = job.PK;
			AssertExceptionThrown(typeof(ArgumentException), string.Format("Must be non job-related transaction, but passed in transaction (PK:{0}) with AH_JH:{1}", jobInvoice.PK, job.PK), () =>
			{
				NonJobRelatedTransactionCommissionCreator.New(jobInvoice);
			});
		}

		#endregion

		#region CreateCommissions

		public void TestCreateCommissions()
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

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_PostDate = new ZDateTime(2002, 2, 2);
			invoice.AH_OH = customer.PK;
			invoice.AH_RX_NKTransactionCurrency = "USD";
			TestObjectCreator.CreateARInvoiceLine(invoice, null, chargeXxx, audCurrency, 1, "", 10);
			TestObjectCreator.CreateARInvoiceLine(invoice, null, chargeXxx, audCurrency, 1, "", 100);
			TestObjectCreator.CreateARInvoiceLine(invoice, null, chargeYyy, audCurrency, 1, "", 1000);
			TestObjectCreator.CreateARInvoiceLine(invoice, null, chargeXxx, usdCurrency, 0.5m, "", 10000);
			TestObjectCreator.CreateARInvoiceLine(invoice, null, chargeYyy, usdCurrency, 0.5m, "", 100000);
			TestObjectCreator.CreateARInvoiceLine(invoice, null, chargeYyy, usdCurrency, 0.5m, "", 1000000);

			var jrj = TestObjectCreator.CreateJobRevenueJournal(typeof(JobRevenueJournal), TestObjectCreator.CC1, Guid.Empty, 100);
			jrj.JournalLines[0].AL_GE = jrj.JournalLines[1].AL_GE = GlbDepartment.CurrentDepartment.PK;
			jrj.AH_PostDate = new ZDateTime(2002, 2, 2);
			jrj.AH_OH = customer.PK;

			Factory.Save();

			var invoiceCommissionCreator = NonJobRelatedTransactionCommissionCreator.New(invoice);
			invoiceCommissionCreator.CreateCommissions();
			Factory.Save();

			var commissionHeaders = new BusinessObjectFactory().Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK));
			var commissionHeader = commissionHeaders[0];
			CombineAssertions("commissionHeader Properties", () =>
			{
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GC, invoice.AH_GC, commissionHeader.CH0_GC);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceTableCode, invoice.TablePrefix, commissionHeader.CH0_GroupingSourceTableCode);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_GroupingSourceID, invoice.PK, commissionHeader.CH0_GroupingSourceID);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_CA0, agreement.PK, commissionHeader.CH0_CA0);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_OH_Customer, customer.PK, commissionHeader.CH0_OH_Customer);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Product, OrgCommissionAgreementItemLookups.AllProductsCode, commissionHeader.CH0_Product);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_Service, OrgCommissionAgreementItemLookups.AllServicesCode, commissionHeader.CH0_Service);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SubModule, OrgCommissionAgreementItemLookups.AllSubModulesCode, commissionHeader.CH0_SubModule);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_JobNumber, string.Empty, commissionHeader.CH0_JobNumber);

				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotDateTime, new ZDateTime(2002, 2, 2), commissionHeader.CH0_SnapshotDateTime);
				AssertEquals(AccCommissionHeaderSchema.Constants.CH0_SnapshotEventCode, AccCommissionHeaderSnapshotEventList.Codes.Posted, commissionHeader.CH0_SnapshotEventCode);
			});
			AssertEquals(1, commissionHeaders.Length);
			{
				var lineGroupChargeXxxAudCur = commissionHeader.LineGroups.Single(x => x.CLG_AC == chargeXxx.PK && x.CLG_RX_NKTransactionCurrency == "AUD");
				AssertEquals(110m, lineGroupChargeXxxAudCur.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupChargeXxxAudCur.CLG_RX_NKTransactionCurrency);
				AssertEquals(110m, lineGroupChargeXxxAudCur.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupChargeXxxAudCur.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupChargeXxxAudCur.Lines.Count);
			}

			{
				var lineGroupChargeXxxUsdCur = commissionHeader.LineGroups.Single(x => x.CLG_AC == chargeXxx.PK && x.CLG_RX_NKTransactionCurrency == "USD");
				AssertEquals(10000m, lineGroupChargeXxxUsdCur.CLG_TransactionAmount);
				AssertEquals("USD", lineGroupChargeXxxUsdCur.CLG_RX_NKTransactionCurrency);
				AssertEquals(20000m, lineGroupChargeXxxUsdCur.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupChargeXxxUsdCur.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupChargeXxxUsdCur.Lines.Count);
			}

			{
				var lineGroupChargeYyyAudCur = commissionHeader.LineGroups.Single(x => x.CLG_AC == chargeYyy.PK && x.CLG_RX_NKTransactionCurrency == "AUD");
				AssertEquals(1000m, lineGroupChargeYyyAudCur.CLG_TransactionAmount);
				AssertEquals("AUD", lineGroupChargeYyyAudCur.CLG_RX_NKTransactionCurrency);
				AssertEquals(1000m, lineGroupChargeYyyAudCur.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupChargeYyyAudCur.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupChargeYyyAudCur.Lines.Count);
			}

			{
				var lineGroupChargeYyyUsdCur = commissionHeader.LineGroups.Single(x => x.CLG_AC == chargeYyy.PK && x.CLG_RX_NKTransactionCurrency == "USD");
				AssertEquals(1100000m, lineGroupChargeYyyUsdCur.CLG_TransactionAmount);
				AssertEquals("USD", lineGroupChargeYyyUsdCur.CLG_RX_NKTransactionCurrency);
				AssertEquals(2200000m, lineGroupChargeYyyUsdCur.CLG_TotalCommissionableAmount);
				AssertEquals("AUD", lineGroupChargeYyyUsdCur.CLG_RX_NKCommissionCurrency);
				AssertEquals(2, lineGroupChargeYyyUsdCur.Lines.Count);
			}

			AssertEquals("Should have grouped the transaction lines into 4 groups by charge code and invoice currency pairs", 4, commissionHeader.LineGroups.Count);

			AssertEquals("Should have 1 fixed commission line", 1, commissionHeader.Lines.Count);

			var jrjCommissionCreator = NonJobRelatedTransactionCommissionCreator.New(jrj);
			jrjCommissionCreator.CreateCommissions();

			var jrjCommissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, jrj.PK));
			AssertEquals(0, jrjCommissionHeaders.Length);
		}

		public void TestCreateCommissions_WithDoNotOverwriteOldValuesOption()
		{
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

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = customer.PK;
			invoice.AH_RX_NKTransactionCurrency = "USD";
			TestObjectCreator.CreateARInvoiceLine(invoice, null, Factory.LoadTop1<AccChargeCode>(new ZQuery()), Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD"), 1, "", 10);

			Factory.Save();

			var invoiceCommissionCreator = NonJobRelatedTransactionCommissionCreator.New(invoice);
			var context = new CreateCommissionContext() { OverwriteOldValues = false };

			invoiceCommissionCreator.CreateCommissions(context);
			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK));
			AssertEquals(1, commissionHeaders.Length);

			invoiceCommissionCreator.CreateCommissions(context);
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
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.REV;
			agreement.CA0_LastApprovedDateUtc = new ZDateTime(2002, 2, 2);
			var agreementRecipient = agreement.Recipients.AddNew();
			agreementRecipient.CAR_GS_NKStaff = "ADL";
			agreementRecipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			agreementRecipient.CAR_Share = 1;
			var recipientRate = agreementRecipient.Rates.AddNew();
			recipientRate.CAT_CommissionPercentage = 50;

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = customer.PK;
			invoice.AH_RX_NKTransactionCurrency = "USD";
			TestObjectCreator.CreateARInvoiceLine(invoice, null, Factory.LoadTop1<AccChargeCode>(new ZQuery()), Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD"), 1, "", 10);

			Factory.Save();

			var invoiceCommissionCreator = NonJobRelatedTransactionCommissionCreator.New(invoice);
			var context = new CreateCommissionContext() { OverwriteOldValues = true };

			invoiceCommissionCreator.CreateCommissions(context);
			var commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK));
			AssertEquals(1, commissionHeaders.Length);
			var firstCommissionHeader = commissionHeaders[0];

			agreementRecipient.CAR_Share = 2;
			agreement.CA0_LastApprovedDateUtc = new ZDateTime(2003, 3, 3);
			Factory.Save();

			invoiceCommissionCreator.CreateCommissions(context);
			commissionHeaders = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK));
			AssertEquals("Should have created another commission header", 2, commissionHeaders.Length);
			AssertEquals("Should have overriden the first commission header", true, firstCommissionHeader.IsOverriden);

			var secondCommissionHeader = commissionHeaders.First(x => x.PK != firstCommissionHeader.PK);
			AssertEquals("Should not have overriden the second commission header", false, secondCommissionHeader.IsOverriden);
			AssertEquals("New commission header should have the updated share amount", (ZByte)2, secondCommissionHeader.LineGroups[0].Lines[0].CL0_SharePortion);
		}

		public void TestCreateCommissions_SnapshotEventCode_RGN()
		{
			var helper = new CommissionTestObjectCreator(Factory);
			helper.SetupTransactionsAndCommissions(closeJob: false);

			var headers = Factory.Load<AccCommissionHeader>(new ZQuery());
			var regeneratedHeader = headers.Where(x => x.CH0_SnapshotEventCode == AccCommissionHeaderSnapshotEventList.Codes.Regenerated);

			AssertEquals("Pre-condition", 1, headers.Length);
			AssertEquals("Pre-condition", 0, regeneratedHeader.Count());

			var commissionCreator = NonJobRelatedTransactionCommissionCreator.New(helper.Invoice);
			commissionCreator.CreateCommissions(new CreateCommissionContext() { RegeneratingCommissions = true });

			Factory.Save();

			headers = Factory.Load<AccCommissionHeader>(new ZQuery());
			regeneratedHeader = headers.Where(x => x.CH0_SnapshotEventCode == AccCommissionHeaderSnapshotEventList.Codes.Regenerated);

			AssertEquals("1 original + 1 regenerated", 2, headers.Length);
			AssertEquals("Regenerated header should have SnapshotEventCode == RGN", 1, regeneratedHeader.Count());
		}

		#endregion
	}
}

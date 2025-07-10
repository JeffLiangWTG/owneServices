using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceLine))]
	public class APInvoiceLineValidationTest : InvoicingLineValidationTest
	{
		protected override Type InvoiceLineType
		{
			get { return typeof(APInvoiceLine); }
		}

		protected override Type InvoiceType
		{
			get { return typeof(APInvoice); }
		}

		protected override Type GetExpectedParentBusinessObjectType()
		{
			return typeof(APInvoice);
		}

		public void TestCheckUnpostedApportionmentGenerateErrorOrWarningMessagesBasedOnCreditorValuesAndRegistrySetting()
		{
			ZGuid chargePK = TestObjectCreator.CC1.PK;
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			Job job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			JobConsolCost cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = chargePK;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			cost.E6_OSCostAmount = 10m;
			cost.E6_LocalCostAmount = 10m;

			JobCharge jobCharge = job.Charges.AddNew();
			jobCharge.JR_AC = chargePK;
			jobCharge.JR_OSCostAmt = 10m;
			jobCharge.JR_LocalCostAmt = 10m;
			jobCharge.JR_E6 = cost.PK;
			jobCharge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_AT = GST10TaxRate.PK;
			line.AL_OSExTaxAmount = 10m;
			line.AL_OSTaxAmount = 10m;
			line.AL_JH = job.PK;
			line.AL_AC = chargePK;
			line.GenericCharge = chargePK;

			((APInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			Assert(!line.GenericChargeInfo.HasErrors());
			Assert(line.GenericChargeInfo.HasWarnings());
			Assert(line.GenericChargeInfo.HasWarning("There is an unposted apportionment relating to this charge code for the job number you have selected. The unposted apportionment is on Consol C00001000 for creditor AALSHI. Please review the creditor on the consol cost to ensure it is correct."));

			AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			((APInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			Assert(line.GenericChargeInfo.HasErrors());
			Assert(!line.GenericChargeInfo.HasWarnings());
			Assert(line.GenericChargeInfo.HasError("There is an unposted apportionment relating to this charge code for the job number that you have selected. Please cancel this line and click on the Apportion To Consols button to import the charge."));

			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			((APInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			Assert(line.GenericChargeInfo.HasErrors());
			Assert(!line.GenericChargeInfo.HasWarnings());
			Assert(line.GenericChargeInfo.HasError("There is an unposted apportionment relating to this charge code for the job number that you have selected. Please cancel this line and click on the Apportion To Consols button to import the charge."));

			line.OriginalJobCharge = jobCharge;
			((APInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			Assert(!line.GenericChargeInfo.HasErrors());
			Assert(line.GenericChargeInfo.HasWarnings());
			Assert(line.GenericChargeInfo.HasWarning("There is an unposted apportionment relating to this charge code for the job number that you have selected. If you are intending to post that apportionment, please cancel this line and click on the Apportion To Consols button to import the charge."));

			line.OriginalJobCharge = null;
			AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			((APInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			Assert(line.GenericChargeInfo.HasErrors());
			Assert(!line.GenericChargeInfo.HasWarnings());
			Assert(line.GenericChargeInfo.HasError("There is an unposted apportionment relating to this charge code for the job number that you have selected. Please cancel this line and click on the Apportion To Consols button to import the charge."));

			line.OriginalJobCharge = jobCharge;
			((APInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			Assert(!line.GenericChargeInfo.HasErrors());
			Assert(line.GenericChargeInfo.HasWarnings());
			Assert(line.GenericChargeInfo.HasWarning("There is an unposted apportionment relating to this charge code for the job number that you have selected. If you are intending to post that apportionment, please cancel this line and click on the Apportion To Consols button to import the charge."));
		}

		public void TestCheckGenericChargeWhereChargeTypeIsOverridden()
		{
			AccChargeCode chargeCMT = TestObjectCreator.CreateChargeCode("CMT1", "CMT Test", Core.Constants.ChargeType.Comment, 1m, null, null);
			AccChargeCode chargeDSB = TestObjectCreator.CreateChargeCode("DSB1", "DSB Test", Core.Constants.ChargeType.Disbursement, 1m, null, null);
			AccChargeCode chargeMJA = TestObjectCreator.CreateChargeCode("MJA1", "MJA Test", Core.Constants.ChargeType.ManualJobAccrual, 1m, null, null);
			AccChargeCode chargeMRG = TestObjectCreator.CreateChargeCode("MRG1", "MRG Test", Core.Constants.ChargeType.Margin, 1m, null, null);
			AccChargeCode chargeNON = TestObjectCreator.CreateChargeCode("NON1", "NON Test", Core.Constants.ChargeType.NonAccrual, 1m, null, null);
			AccChargeCode chargeOVR = TestObjectCreator.CreateChargeCode("OVR1", "OVR Test", Core.Constants.ChargeType.Overhead, 1m, null, null);
			AccChargeCode chargeREV = TestObjectCreator.CreateChargeCode("REV1", "REV Test", Core.Constants.ChargeType.Revenue, 1m, null, null);

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S0001");
			Job job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			APInvoiceLine line1 = (APInvoiceLine)invoice.Lines.AddNew();
			line1.GenericCharge = chargeCMT.PK;
			((APInvoiceLineValidation)line1.Validation).ValidateGenericCharge();
			AssertNoError("Generic Charge should not have an error", line1.GenericChargeInfo, "This charge code cannot be chosen here.");

			line1.AL_JH = job.PK;
			((APInvoiceLineValidation)line1.Validation).ValidateGenericCharge();
			AssertNoError("Generic Charge should have an error", line1.GenericChargeInfo, "This charge code cannot be chosen here.");

			APInvoiceLine line2 = (APInvoiceLine)invoice.Lines.AddNew();
			line2.GenericCharge = chargeDSB.PK;
			((APInvoiceLineValidation)line2.Validation).ValidateGenericCharge();
			AssertHasError("Generic Charge should not have an error", line2.GenericChargeInfo, "This charge code cannot be chosen here.");

			line2.AL_JH = job.PK;
			((APInvoiceLineValidation)line2.Validation).ValidateGenericCharge();
			AssertNoError("Generic Charge should not have an error", line2.GenericChargeInfo, "This charge code cannot be chosen here.");

			APInvoiceLine line3 = (APInvoiceLine)invoice.Lines.AddNew();
			line3.GenericCharge = chargeMJA.PK;
			((APInvoiceLineValidation)line3.Validation).ValidateGenericCharge();
			AssertHasError("Generic Charge should not have an error", line3.GenericChargeInfo, "This charge code cannot be chosen here.");

			line3.AL_JH = job.PK;
			((APInvoiceLineValidation)line3.Validation).ValidateGenericCharge();
			AssertNoError("Generic Charge should not have an error", line3.GenericChargeInfo, "This charge code cannot be chosen here.");

			APInvoiceLine line4 = (APInvoiceLine)invoice.Lines.AddNew();
			line4.GenericCharge = chargeMRG.PK;
			((APInvoiceLineValidation)line4.Validation).ValidateGenericCharge();
			AssertHasError("Generic Charge should have an error", line4.GenericChargeInfo, "This charge code cannot be chosen here.");

			line4.AL_JH = job.PK;
			((APInvoiceLineValidation)line4.Validation).ValidateGenericCharge();
			AssertNoError("Generic Charge should have an error", line4.GenericChargeInfo, "This charge code cannot be chosen here.");

			APInvoiceLine line5 = (APInvoiceLine)invoice.Lines.AddNew();
			line5.GenericCharge = chargeNON.PK;
			((APInvoiceLineValidation)line5.Validation).ValidateGenericCharge();
			AssertNoError("Generic Charge should have an error", line5.GenericChargeInfo, "This charge code cannot be chosen here.");

			line5.AL_JH = job.PK;
			((APInvoiceLineValidation)line5.Validation).ValidateGenericCharge();
			AssertNoError("Generic Charge should have an error", line5.GenericChargeInfo, "This charge code cannot be chosen here.");

			APInvoiceLine line6 = (APInvoiceLine)invoice.Lines.AddNew();
			line6.GenericCharge = chargeOVR.PK;
			((APInvoiceLineValidation)line6.Validation).ValidateGenericCharge();
			AssertNoError("Generic Charge should have an error", line6.GenericChargeInfo, "This charge code cannot be chosen here.");

			line6.AL_JH = job.PK;
			((APInvoiceLineValidation)line6.Validation).ValidateGenericCharge();
			AssertNoError("Generic Charge should have an error", line6.GenericChargeInfo, "This charge code cannot be chosen here.");

			APInvoiceLine line7 = (APInvoiceLine)invoice.Lines.AddNew();
			line7.GenericCharge = chargeREV.PK;
			((APInvoiceLineValidation)line7.Validation).ValidateGenericCharge();
			AssertHasError("Generic Charge should have an error", line7.GenericChargeInfo, "This charge code cannot be chosen here.");

			line7.AL_JH = job.PK;
			((APInvoiceLineValidation)line7.Validation).ValidateGenericCharge();
			AssertHasError("Generic Charge should have an error", line7.GenericChargeInfo, "This charge code cannot be chosen here.");
		}

		public void TestCheckGenericChargeExpectNoError()
		{
			ZGuid chargePK = InsertGenericCharge();
			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = GSTRegisteredOrg.PK;
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();

			line.AL_AT = GST10TaxRate.PK;
			line.AL_OSExTaxAmount = 10m;
			line.AL_OSTaxAmount = 10m;

			line.GenericCharge = chargePK;

			((APInvoiceLineValidation)line.Validation).ValidateGenericCharge();

			Assert(!line.GenericChargeInfo.HasErrors());
			Assert(!line.GenericChargeInfo.HasWarnings());
		}

		ZGuid InsertGenericCharge()
		{
			AccChargeCode charge = Factory.NewWithValidTestData(typeof(AccChargeCode)) as AccChargeCode;
			charge.AC_ChargeType = "OVR";
			charge.AC_IsActive = ZBool.True;
			charge.AC_AG_CostAccount = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			return charge.PK;
		}

		[MasterFiles.Business.Testing.SuspendGLAccountAndChargeCodeCriticalValidation]
		public override void TestCheckAL_AG()
		{
			base.TestCheckAL_AG();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_TransactionNum = "101";
			APInvoiceLine line1 = (APInvoiceLine)invoice.Lines.AddNew();
			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			Factory.Save();

			var job = Factory.LoadGenericJob<GenericJob.GenericJob>(shipment.PK, JobShipmentSchema.Constants.Prefix);

			line1.AL_JH = job.PK;
			line1.AL_AC = TestCharge.PK;

			line1.Validation.ValidateAL_AG();

			Assert("Line AL_AG should not have error", !line1.AL_AGInfo.HasErrors());
		}

		[MasterFiles.Business.Testing.SuspendGLAccountAndChargeCodeCriticalValidation]
		public override void TestCheckAL_AC()
		{
			base.TestCheckAL_AC();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_TransactionNum = "101";
			APInvoiceLine line1 = (APInvoiceLine)invoice.Lines.AddNew();
			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			Factory.Save();

			var job = Factory.LoadGenericJob<GenericJob.GenericJob>(shipment.PK, JobShipmentSchema.Constants.Prefix);

			line1.AL_JH = job.PK;
			line1.AL_AC = TestCharge.PK;

			line1.Validation.ValidateAL_AC();

			Assert("Line AL_AC should not have error", !line1.AL_ACInfo.HasErrors());
		}

		public virtual void TestCheckGenericFalse()
		{
			AccChargeCode jobChargeCode = TestCharge;
			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_TransactionNum = "101";
			APInvoiceLine line1 = (APInvoiceLine)invoice.Lines.AddNew();
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			Job jobHeader = TestObjectCreator.CreateJob(GSTRegisteredOrg, 100, NonGSTRegisteredOrg, 10);
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			JobConsolCost cost = Factory.New<JobConsolCost>();
			cost.E6_AC_ChargeCode = jobChargeCode.PK;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			using (cost.ReportSettingParentSuspender.GetSuspender())
			{
				cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			}
			cost.E6_OSCostAmount = 10m;
			cost.E6_LocalCostAmount = 10m;

			JobCharge jobCharge = jobHeader.Charges.AddNew();
			jobCharge.JR_AC = jobChargeCode.PK;
			jobCharge.JR_OSCostAmt = 10m;
			jobCharge.JR_LocalCostAmt = 10m;
			jobCharge.JR_E6 = cost.PK;

			ZQuery filter = new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK);
			filter.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			Job job = (Factory.LoadTop1<Job>(filter));

			//Set job detail so the test should fail
			line1.AL_JH = job.PK;
			line1.AL_AC = jobChargeCode.PK;
			TestObjectCreator.CreateJobCharge(line1, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

			invoice.RunPreSaveValidation();

			Assert("Line Generic Charge should not have error", !line1.GenericChargeInfo.HasErrors());

			Factory.Save();
			invoice.RunPreSaveValidation();
			Assert("Line Generic Charge should not have error", !line1.GenericChargeInfo.HasErrors());
		}

		public void TestCheckAL_IsFinalCharge_AllowPayablesInvoiceFinalFlag()
		{
			Env.Security.AllowUntickAutoTickedFinalFlag.IsAllowed = true;

			const string expectError = @"You do not have appropriate security rights to tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Tick Final Flag on Payables Invoice";

			Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = false;
			APInvoiceLine invLine = Factory.New<APInvoiceLine>();
			invLine.AL_IsFinalCharge = true;
			AssertHasError("Should have error", invLine.AL_IsFinalChargeInfo, expectError);
			invLine.AL_IsFinalCharge = false;
			AssertNoErrors("Should not have error", invLine.AL_IsFinalChargeInfo);

			Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = true;
			invLine.AL_IsFinalCharge = true;
			AssertNoErrors("Should not have error", invLine.AL_IsFinalChargeInfo);
			invLine.AL_IsFinalCharge = false;
			AssertNoErrors("Should not have error", invLine.AL_IsFinalChargeInfo);

			Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = false;
			ApportionSplitCharge apportionmentCharge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			apportionmentCharge.IsFinal = ZBool.True;
			invLine.ImportFromApportionSplitCharge(apportionmentCharge);
			Assert("Should be set", invLine.AL_IsFinalCharge);
			Assert("Should not be readonly", !invLine.AL_IsFinalChargeInfo.ReadOnly);
			AssertNoErrors("Should not have error", invLine.AL_IsFinalChargeInfo);

			//test validate all vs validating the single property
			Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = false;
			APInvoice invoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("0000001", TestObjectCreator.AUD, 1M, 20M, 0M, 0M, 20M, 0M, 0M);
			APInvoice invoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("0000002", TestObjectCreator.AUD, 1M, 20M, 0M, 0M, 20M, 0M, 0M);
			invoice1.Lines[0].AL_IsFinalCharge = true;
			invoice2.Lines[0].AL_IsFinalCharge = true;

			invoice1.Validation.ValidateAll();
			invoice2.Lines[0].Validation.ValidateAL_IsFinalCharge();

			AssertHasError(invoice1.Lines[0].AL_IsFinalChargeInfo, expectError);
			AssertHasError(invoice2.Lines[0].AL_IsFinalChargeInfo, expectError);
		}

		public void TestCheckAL_IsFinalCharge_AllowUntickAutoTickedFinalFlag()
		{
			Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = true;

			const string expectError = @"You do not have appropriate security rights to un-tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Untick Auto-ticked Final Flag";

			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndChargeCode);

			GlbBranch sYD = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SYD");
			GlbDepartment fEA = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FEA");
			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));
			Charge charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", TestObjectCreator.AUD, 100M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 0M, null);
			charge.JR_GB = sYD.PK;
			charge.JR_GE = fEA.PK;
			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;

			new CostVarianceApprovalHelper(invoice).ClearCachedCostVarianceApproval_ForTestOnly();

			var line = TestObjectCreator.CreateAPInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "line1", 0m);
			line.AL_GB = sYD.PK;
			line.AL_GE = fEA.PK;

			line.AL_LocalExTaxAmount = 120M;
			AssertEquals("The cost variance requires None approval, 120 - 100 is NOT above 100.", ZBool.True, line.AL_IsFinalCharge);
			AssertNoErrors("Should not have error", line.AL_IsFinalChargeInfo);

			Env.Security.AllowUntickAutoTickedFinalFlag.IsAllowed = false;
			line.AL_IsFinalCharge = false;
			AssertHasError("Should have error", line.AL_IsFinalChargeInfo, expectError);

			Env.Security.AllowUntickAutoTickedFinalFlag.IsAllowed = true;
			line.Validation.ValidateAL_IsFinalCharge();
			AssertNoErrors("Should not have error", line.AL_IsFinalChargeInfo);
		}

		public void TestCheckAL_IsFinalChargeDoesNotCheckUnchangedInvoice()
		{
			Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = true;
			AccTransactionHeader header = Factory.NewWithValidTestData<APInvoice>();
			APInvoiceLine invLine = Factory.NewWithValidTestData<APInvoiceLine>();
			invLine.AL_AH = header.PK;
			invLine.AL_IsFinalCharge = true;
			invLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			Assert("Should not have error", !invLine.AL_IsFinalChargeInfo.HasErrors());
			Factory.Save();

			Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = false;
			invLine.AL_IsFinalCharge = true;
			Assert("Should not have error", !invLine.AL_IsFinalChargeInfo.HasErrors());
		}

		public void TestCheckAL_IsFinalChargeTestsUnapprovedInvoice()
		{
			Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = true;
			AccTransactionHeader header = Factory.NewWithValidTestData<UAInvoice>();
			UAInvoiceLine uAInvLine = Factory.NewWithValidTestData<UAInvoiceLine>();
			uAInvLine.AL_AH = header.PK;
			uAInvLine.AL_IsFinalCharge = true;
			Factory.Save();

			var validation = uAInvLine.Validation as InvoicingLineBaseValidation;
			validation.ValidateAL_IsFinalCharge();
			Assert("Should not have error", !uAInvLine.AL_IsFinalChargeInfo.HasErrors());

			Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = false;
			validation.ValidateAL_IsFinalCharge();
			Assert("Should have error", uAInvLine.AL_IsFinalChargeInfo.HasErrors());
		}

		public void TestCheckAL_OSExTaxAmount()
		{
			CostVarianceApproval valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.Job;
			CostVarianceApprovalAuthorisationRequirement upTo1 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo1.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			upTo1.Range = RangeCodes.UpTo;
			upTo1.Amount = 100M;
			CostVarianceApprovalAuthorisationRequirement upTo2 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo2.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			upTo2.Range = RangeCodes.UpTo;
			upTo2.Amount = 200M;
			CostVarianceApprovalAuthorisationRequirement above = valuesForTest.AuthorisationRequirements.AddNew();
			above.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
			above.Range = RangeCodes.Above;
			above.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = true;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = false;

			GlbBranch sYD = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SYD");
			GlbDepartment fEA = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FEA");
			AccChargeCode cAF = TestObjectCreator.CC1;

			AssertNotNull(sYD);
			AssertNotNull(fEA);

			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));
			Charge charge = TestObjectCreator.CreateCharge(job, cAF, "",
				TestObjectCreator.AUD, 100M, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 0M, null);
			charge.JR_GB = sYD.PK;
			charge.JR_GE = fEA.PK;

			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;

			new CostVarianceApprovalHelper(invoice).ClearCachedCostVarianceApproval_ForTestOnly();

			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_JH = job.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line.AL_AC = cAF.PK;
			line.AL_GB = sYD.PK;
			line.AL_GE = fEA.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			line.AL_ExchangeRate = 1M;

			line.AL_OSExTaxAmount = 150M;
			AssertNoWarnings(line.AL_OSExTaxAmountInfo);

			line.AL_OSExTaxAmount = 250M;
			AssertHasWarning(line.AL_OSExTaxAmountInfo, "This cost will be automatically approved when you post because you already have the necessary authorization security right");

			line.AL_OSExTaxAmount = 350M;
			string expectedError = "This cost requires approval on posting because it exceeds the registry defined accrual variance threshold.";
			AssertHasWarning(line.AL_OSExTaxAmountInfo, expectedError);

			line.AL_LocalExTaxAmount = 150M;
			AssertNoWarnings(line.AL_OSExTaxAmountInfo);
		}

		public void TestCheckAL_OSExTaxAmountCSTExceedsREVWarning()
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));
			Factory.Save();

			var invoiceFactory = new BusinessObjectFactory();
			invoiceFactory.SetContext(BusinessContext.APInvoiceForm);

			var invoice = invoiceFactory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			var line1 = (APInvoiceLine)invoice.Lines.AddNew();
			line1.AL_JH = job.PK;
			line1.AL_LineType = "CST";
			line1.AL_AC = TestObjectCreator.CC1.PK;
			line1.AL_GB = GlbBranch.CurrentBranch.PK;
			line1.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line1.AL_RX_NKTransactionCurrency = "AUD";
			line1.AL_ExchangeRate = 1M;
			line1.AL_LocalExTaxAmount = 1M;

			AssertHasWarning("should show warning even no rev. at all", line1.AL_OSExTaxAmountInfo, "The cost on this line is greater than the corresponding billed revenue of $0.00.");

			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "",
							TestObjectCreator.AUD, 50M, TestObjectCreator.Creditor1,
							TestObjectCreator.AUD, 50M, TestObjectCreator.Debtor);
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			charge1.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge1.JR_APInvoiceDate = ZDateTime.Now;
			charge1.JR_APInvoiceNum = "inv12345";
			Factory.Save();

			invoice.ClearLineChargeAmountSumCache();
			line1.AL_LocalExTaxAmount = 51M;
			AssertEquals(false, charge1.IsCostPosted);
			AssertEquals(false, charge1.IsRevenuePosted);
			AssertHasWarning("should show warning when cost > unposted rev.", line1.AL_OSExTaxAmountInfo, "The cost on this line is greater than the corresponding billed revenue of $0.00 and un-billed revenue of $50.00.");

			TestObjectCreator.PostJobAsBillingTab(job, JobInvoicingPostingOption.Revenue);
			Factory.Save();

			invoice.ClearLineChargeAmountSumCache();
			line1.AL_LocalExTaxAmount = 51M;
			AssertEquals(false, charge1.IsCostPosted);
			AssertEquals(true, charge1.IsRevenuePosted);
			AssertHasWarning("should show warning when cost > posted rev.", line1.AL_OSExTaxAmountInfo, "The cost on this line is greater than the corresponding billed revenue of $50.00.");

			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "",
							TestObjectCreator.AUD, 50M, TestObjectCreator.Creditor1,
							TestObjectCreator.AUD, 50M, TestObjectCreator.Debtor);
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge2.JR_APInvoiceDate = ZDateTime.Now;
			charge2.JR_APInvoiceNum = "inv12345";
			Factory.Save();

			invoice.ClearLineChargeAmountSumCache();
			line1.AL_LocalExTaxAmount = 101M;
			AssertEquals(false, charge1.IsCostPosted);
			AssertEquals(true, charge1.IsRevenuePosted);
			AssertEquals(false, charge2.IsCostPosted);
			AssertEquals(false, charge2.IsRevenuePosted);
			AssertHasWarning("should show warning when cost > posted rev. and state un-billed rev.", line1.AL_OSExTaxAmountInfo, "The cost on this line is greater than the corresponding billed revenue of $50.00 and un-billed revenue of $50.00.");

			TestObjectCreator.PostJobAsBillingTab(job, JobInvoicingPostingOption.Costs);
			Factory.Save();

			invoice.ClearLineChargeAmountSumCache();
			line1.AL_LocalExTaxAmount = 1M;
			AssertEquals(true, charge1.IsCostPosted);
			AssertEquals(true, charge1.IsRevenuePosted);
			AssertEquals(true, charge2.IsCostPosted);
			AssertEquals(false, charge2.IsRevenuePosted);
			AssertHasWarning("should show warning when posted cost + cost entered > rev.", line1.AL_OSExTaxAmountInfo, "The cost on this line together with billed cost of $100.00 is greater than the corresponding billed revenue of $50.00 and un-billed revenue of $50.00.");

			TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.OldJob, AccountingConstants.RevenueRecognitionDateConstants.JobClosure);
			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "",
							TestObjectCreator.AUD, 0M, null,
							TestObjectCreator.AUD, 40M, TestObjectCreator.Debtor);
			charge3.JR_GB = GlbBranch.CurrentBranch.PK;
			charge3.JR_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			AssertEquals(false, charge3.IsCostPosted);
			AssertEquals(false, charge3.IsRevenuePosted);
			AssertNull("Unrecognized WIP", charge3.WIP);

			invoice.ClearLineChargeAmountSumCache();
			line1.AL_LocalExTaxAmount = 50M;
			AssertHasWarning("should show warning when posted cost + cost entered > rev (billed + unbilled + unrecognized).", line1.AL_OSExTaxAmountInfo, "The cost on this line together with billed cost of $100.00 is greater than the corresponding billed revenue of $50.00 and un-billed revenue of $90.00.");
		}

		[ExpectNoExceptions]
		public void TestCheckUnpostedApportionmentForExceptionWhenCreditorNotSet()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S00001001");
			Job job = TestObjectCreator.CreateJob(shipment1, false);

			InvoicingBase invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();

			InvoicingLineBaseValidation validation = invoice.Lines[0].Validation as InvoicingLineBaseValidation;
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_JH = job.PK;
			line.AL_AT = TestObjectCreator.GSTFREE1.PK;
			line.AL_OSExTaxAmount = 100m;
			line.AL_LocalExTaxAmount = 100m;
			Charge charge = TestObjectCreator.CreateCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Charge unpostedApportionment = job.Charges.AddNew();
			unpostedApportionment.JR_AC = TestObjectCreator.CC1.PK;
			unpostedApportionment.JR_OSCostAmt = 10m;
			unpostedApportionment.JR_LocalCostAmt = 10m;

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			JobConsolCost cost = Factory.New<JobConsolCost>();
			cost.E6_AC_ChargeCode = DisbursementChargeCode.PK;
			using (cost.ReportSettingParentSuspender.GetSuspender())
			{
				cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			}
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_OSCostAmount = 10m;
			cost.E6_LocalCostAmount = 10m;

			unpostedApportionment.JR_E6 = cost.PK;
			Factory.Save();

			validation.ValidateGenericCharge();

			AssertNull("Credit Must be null on Consol", cost.Creditor);
			AssertHasError("Must Show Error When Creditor is Empty", line.GenericChargeInfo, "There is an unposted apportionment relating to this charge code for the job number that you have selected. Please cancel this line and click on the Apportion To Consols button to import the charge.");

			line.OriginalJobCharge = unpostedApportionment;
			validation.ValidateGenericCharge();
			Assert(!line.GenericChargeInfo.HasErrors());
			Assert(line.GenericChargeInfo.HasWarnings());
			Assert(line.GenericChargeInfo.HasWarning("There is an unposted apportionment relating to this charge code for the job number that you have selected. If you are intending to post that apportionment, please cancel this line and click on the Apportion To Consols button to import the charge."));
		}

		public void TestCheckAL_JH_ReopenClosedJobAllowedWithBusinessContextCASS()
		{
			var shipment = TestObjectCreator.CreateAndSaveTestForwardingShipmentJob(JobHeaderStatus.Closed.Code);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, GSTRegisteredOrg);
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_JH = shipment.Job.PK;

			Env.Security.ReopenJob.IsAllowed = false;
			invoice.RunPreSaveValidation();

			AssertNoError(line.AL_JHInfo, InvoicingLineBaseValidation.ReopenClosedJobSecurityMessage);
			AssertHasWarning(line.AL_JHInfo, InvoicingLineBaseValidation.ReopenClosedJobSecurityMessage);
			AssertNoWarning(line.AL_JHInfo, InvoicingLineBaseValidation.ReopenClosedJobWarningMessage);

			invoice.Factory.SetContext(BusinessContext.CASS);
			invoice.RunPreSaveValidation();
			AssertNoError(line.AL_JHInfo, InvoicingLineBaseValidation.ReopenClosedJobSecurityMessage);
			AssertNoWarning(line.AL_JHInfo, InvoicingLineBaseValidation.ReopenClosedJobSecurityMessage);
			AssertHasWarning(line.AL_JHInfo, InvoicingLineBaseValidation.ReopenClosedJobWarningMessage);
		}

		public void TestCheckApportionSplitChargeCostAmount()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment, false);
			var list = new ApportionmentListing(Factory, consol);
			var consolCost = list.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			consolCost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			consolCost.E6_RX_NKCurrency = Enterprise.Core.Constants.CurrencyCodes.Australia;
			consolCost.E6_ExchangeRate = 1m;
			consolCost.E6_OSCostAmount = 10m;
			consolCost.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
			consolCost.E6_InvoiceNum = "I001";
			consolCost.E6_InvoiceDate = ZDateTime.Now;
			consolCost.E6_PaymentDate = ZDateTime.Now;
			Factory.Save();

			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			invoice.ImportSingleCostAndRevalidateLines(consolCost, line);
			AssertNotNull(line.ApportionmentChargeImportedFrom);

			var expectMessageError = "Linked apportion charge should not have zero oversea or local cost amount. Please remove this line and other associated lines then import the apportioned charges again.";
			Assert(!line.RowErrors.Contains(expectMessageError));

			line.ApportionmentChargeImportedFrom.JR_OSCostAmt = 0;
			line.Validation.ValidateAll();
			Assert(line.RowErrors.Contains(expectMessageError));
			line.ApportionmentChargeImportedFrom.JR_OSCostAmt = 10;
			line.Validation.ValidateAll();
			Assert(!line.RowErrors.Contains(expectMessageError));

			line.ApportionmentChargeImportedFrom.JR_RX_NKCostCurrency = "";
			line.ApportionmentChargeImportedFrom.JR_LocalCostAmt = 0;
			line.Validation.ValidateAll();
			Assert(line.RowErrors.Contains(expectMessageError));
			line.ApportionmentChargeImportedFrom.JR_LocalCostAmt = 10;
			line.Validation.ValidateAll();
			Assert(!line.RowErrors.Contains(expectMessageError));
		}

		#region Implementation

		protected AccChargeCode TestCharge
		{
			get
			{
				AccChargeCode result = Factory.NewWithValidTestData<AccChargeCode>();
				result.AC_AG_CostAccount = TestObjectCreator.GLHeader1.PK;
				return result;
			}
		}

		protected override InvoiceLineValidation GetValidation(InvoiceLine parent)
		{
			return new APInvoiceLineValidation((APInvoiceLine)parent);
		}

		#endregion
	}
}

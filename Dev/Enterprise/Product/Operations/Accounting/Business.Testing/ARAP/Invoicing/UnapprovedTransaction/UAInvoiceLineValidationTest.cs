using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(APInvoiceLine))]
	public class UAInvoiceLineValidationTest : APInvoiceLineValidationTest
	{
		protected override Type InvoiceLineType
		{
			get { return typeof(UAInvoiceLine); }
		}

		protected override Type InvoiceType
		{
			get { return typeof(UAInvoice); }
		}

		protected override InvoiceLineValidation GetValidation(InvoiceLine parent)
		{
			return new UAInvoiceLineValidation((UAInvoiceLine)parent);
		}

		public override void TestCheckAL_JH_ReopenClosedJobDenied()
		{
			Job job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Closed.Code);

			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			invoice.AH_OH = GSTRegisteredOrg.PK;

			UAInvoiceLine line = (UAInvoiceLine)invoice.Lines.AddNew();

			bool oldAllowReopenJob = Env.Security.ReopenJob.IsAllowed;

			try
			{
				Env.Security.ReopenJob.IsAllowed = false;
				line.AL_JH = job.PK;

				line.AL_LineType = TransactionLineTypes.UnapprovedCost;
				invoice.IsCreatedByENett = true;
				invoice.RunPreSaveValidation();

				string expectedWarning = InvoicingLineBaseValidation.ReopenClosedJobSecurityMessage;
				AssertNoWarning("Since UAInvoice was created by eNett should NOT have Warning", line.AL_JHInfo, expectedWarning);

				invoice.IsCreatedByENett = false;
				invoice.RunPreSaveValidation();

				AssertHasWarning("UAInvoice, Should have Warning", line.AL_JHInfo, expectedWarning);

				line.AL_LineType = TransactionLineTypes.Cost;
				invoice.RunPreSaveValidation();

				AssertHasWarning("APInvoice, Should have Warning", line.AL_JHInfo, expectedWarning);
			}
			finally
			{
				Env.Security.ReopenJob.IsAllowed = oldAllowReopenJob;
			}
		}

		public override void TestCheckAL_AC()
		{
			UAInvoice invoice = Factory.New<UAInvoice>();
			invoice.AH_TransactionNum = "TRAN101";
			UAInvoiceLine line = (UAInvoiceLine)invoice.Lines.AddNew();
			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			Factory.Save();

			GenericJob.GenericJob job = Factory.LoadGenericJob<GenericJob.GenericJob>(shipment.PK, JobShipmentSchema.Constants.Prefix);

			line.AL_JH = job.PK;
			line.AL_AC = TestCharge.PK;

			line.AL_LineType = TransactionLineTypes.UnapprovedCost;
			line.Validation.ValidateAL_AC();
			Assert("Line AL_AC should not have error", !line.AL_ACInfo.HasErrors());

			line.AL_LineType = TransactionLineTypes.Cost;
			line.Validation.ValidateAL_AC();
			Assert("Line AL_AC should not have error", !line.AL_ACInfo.HasErrors());
		}

		public override void TestCheckGenericFalse()
		{
			AccChargeCode jobChargeCode = TestCharge;
			UAInvoice invoice = Factory.New<UAInvoice>();
			invoice.AH_TransactionNum = "TRAN101";
			UAInvoiceLine line = (UAInvoiceLine)invoice.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			Job jobHeader = TestObjectCreator.CreateJob(GSTRegisteredOrg, 100, NonGSTRegisteredOrg, 10);
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			JobConsolCost cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = jobChargeCode.PK;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_OSCostAmount = 10m;
			cost.E6_LocalCostAmount = 10m;

			JobCharge jobCharge = jobHeader.Charges.AddNew();
			jobCharge.JR_AC = jobChargeCode.PK;
			jobCharge.JR_E6 = cost.PK;
			jobCharge.JR_OSCostAmt = 10m;
			jobCharge.JR_LocalCostAmt = 10m;

			ZQuery filter = new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK);
			filter.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			Job job = (Factory.LoadTop1<Job>(filter));

			//Set job detail so the test should fail
			line.AL_JH = job.PK;
			line.AL_AC = jobChargeCode.PK;
			TestObjectCreator.CreateJobCharge(line, job, jobChargeCode, TestObjectCreator.AUD);

			invoice.RunPreSaveValidation();

			AssertNoErrors("Line Generic Charge should not have error", line.GenericChargeInfo);

			Factory.Save();

			if (invoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
			{
				invoice.AH_Ledger = LedgerTypes.AccountsPayable;
				invoice.AH_TransactionType = TransactionTypes.Invoice;
			}

			invoice.RunPreSaveValidation();
			AssertHasErrors("Line Generic Charge should have error", line.GenericChargeInfo);

			line.AL_LineType = TransactionLineTypes.UnapprovedCost;
			invoice.IsCreatedByENett = true;
			invoice.RunPreSaveValidation();
			Assert("Line Generic Charge should not have error", !line.GenericChargeInfo.HasErrors());

			invoice.IsCreatedByENett = false;
			invoice.RunPreSaveValidation();
			Assert("Line Generic Charge should have error", line.GenericChargeInfo.HasErrors());
		}

		public override void TestCheckAL_JHForSelectedCharge()
		{
			ForwardingShipment testShipment = TestObjectCreator.CreateShipment("S00001234");
			Job testJob = TestObjectCreator.CreateJob(testShipment);

			ZQuery testFilter = new ZQuery(AccChargeCodeSchema.AC_ChargeType, "MRG");
			AccChargeCode chargeCode = Factory.LoadTop1(typeof(AccChargeCode), testFilter) as AccChargeCode;
			chargeCode.AC_DepartmentFilterList = "ALL";

			Factory.Save();

			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();

			line.GenericCharge = chargeCode.PK;

			invoice.IsCreatedByENett = true;
			line.Validation.ValidateAL_JH();

			string expectedError = "You must select a job for this charge code.";
			AssertNoError(line.AL_JHInfo, expectedError);

			line.GenericCharge = chargeCode.PK;
			invoice.IsCreatedByENett = false;
			line.Validation.ValidateAL_JH();

			AssertHasError(line.AL_JHInfo, expectedError);

			line.AL_JH = testJob.PK;
			line.Validation.ValidateAL_JH();

			AssertNoError(line.AL_JHInfo, expectedError);
		}

		[SuspendCriticalValidation]
		public void TestCheckAL_JHWithConvertedUAInvoice()
		{
			ForwardingShipment testShipment = TestObjectCreator.CreateShipment("S00001234");
			Job testJob = TestObjectCreator.CreateJob(testShipment);

			ZQuery query = new ZQuery(AccChargeCodeSchema.AC_IsActive, true);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccChargeCodeSchema.AC_ChargeType, Core.Constants.ChargeType.Margin);
			query.AddToFilter(AccChargeCodeSchema.AC_DepartmentFilterList, "ALL");
			AccChargeCode chargeCode = Factory.LoadTop1<AccChargeCode>(query);

			Factory.Save();

			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();

			invoice.AH_TransactionNum = "1111";

			ZDBOnlyQuery orgQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			orgQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, ZBool.True);
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			orgQuery.AddSubQuery(subQuery, JoinCondition.And);
			OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(orgQuery);
			invoice.AH_OH = orgHeader.PK;

			ZDecimal amount = 500.00m;
			line.GenericCharge = chargeCode.PK;
			line.AL_OSExTaxAmount = amount;
			line.AL_LocalExTaxAmount = amount;
			line.GenericCharge = chargeCode.PK;
			invoice.IsCreatedByENett = true;
			invoice.RunPreSaveValidation();
			Assert(string.Format("Invoice should have no errors: {0}", invoice.NotificationsIncludingChildren.ToMessageListString()), !invoice.HasErrors);
			Factory.Save();

			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);
			InvoicingBase convertedInvoice = converter.ConvertToAP(invoice, false);
			AssertNotNull("ConvertedInvoice not null", convertedInvoice);
			AssertEquals("convertedInvoice.Lines.Count should equal 1", 1, convertedInvoice.Lines.Count);

			InvoicingLineBase convertedLine = convertedInvoice.Lines[0];
			Assert("AL_JH should be readonly here", convertedLine.AL_JHInfo.ReadOnly);
			convertedInvoice.RunPreSaveValidation();
			Assert("Should have error: 'You must select a job for this charge code.'", convertedLine.AL_JHInfo.HasError("You must select a job for this charge code."));

			line.AL_JH = testJob.PK;
			TestObjectCreator.CreateCharge(line);
			Factory.Save();
			convertedInvoice = converter.ConvertToAP(invoice, false);
			AssertNotNull("ConvertedInvoice not null", convertedInvoice);
			AssertEquals("convertedInvoice Line Count == 1", 1, convertedInvoice.Lines.Count);

			convertedLine = convertedInvoice.Lines[0];
			Assert("AL_JH should be readonly here", convertedLine.AL_JHInfo.ReadOnly);
			convertedInvoice.RunPreSaveValidation();
			Assert("Should not have error: 'You must select a job for this charge code.'", !convertedLine.AL_JHInfo.HasError("You must select a job for this charge code."));
		}

		public override void TestCheckAL_JHForSelectedGLTypeCharge()
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			Job job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();

			ZQuery testFilter = new ZQuery(ViewGenericChargeSchema.VC_Type, "P&L");
			testFilter.AddToFilter(ViewGenericChargeSchema.VC_DisallowDirectPosting, false);
			GenericCharge.GenericCharge testGenericCharge = Factory.LoadTop1(typeof(GenericCharge.GenericCharge), testFilter) as GenericCharge.GenericCharge;
			line.GenericCharge = testGenericCharge.PK;
			line.Validation.ValidateAL_JH();

			Assert("Should not have error: 'You cannot select a job for a GL Account charge code.", !line.AL_JHInfo.HasError("You cannot select a job for a GL Account charge code."));
			Assert("Should not have error: 'You must select a job for this charge code.'", !line.AL_JHInfo.HasError("You must select a job for this charge code."));

			line.AL_JH = job.PK;
			invoice.IsCreatedByENett = true;
			line.Validation.ValidateAL_JH();

			Assert("Should not have error: 'You cannot select a job for a GL Account charge code.'", !line.AL_JHInfo.HasError("You cannot select a job for a GL Account charge code."));
			Assert("Should not have error: 'You must select a job for this charge code.'", !line.AL_JHInfo.HasError("You must select a job for this charge code."));

			line.AL_JH = job.PK;
			invoice.IsCreatedByENett = false;
			line.Validation.ValidateAL_JH();

			Assert("Should have error: 'You cannot select a job for a GL Account charge code.'", line.AL_JHInfo.HasError("You cannot select a job for a GL Account charge code."));
			Assert("Should have error: 'You must select a job for this charge code.'", !line.AL_JHInfo.HasError("You must select a job for this charge code."));
		}
	}
}

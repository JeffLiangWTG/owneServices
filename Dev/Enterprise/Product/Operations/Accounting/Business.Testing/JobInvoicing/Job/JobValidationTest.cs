using System;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Business.JobInvoicing.JobValidation;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JobValidationTest : TransactionCreatorBaseTest
	{
		public void TestCheckJH_StatusForChangeToCLS()
		{
			using (AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var job = TestObjectCreator.CreateJobHeader();
				job.JH_JobNum = "J001";

				//Create disbursement charges which are not balanced for two Line GL Accounts
				var surplusGLAccount1 = TestObjectCreator.CreateGLHeader("99991001");
				var shortfallGLAccount1 = TestObjectCreator.CreateGLHeader("99991002");
				var surplusGLAccount2 = TestObjectCreator.CreateGLHeader("99992001");
				var shortfallGLAccount2 = TestObjectCreator.CreateGLHeader("99992002");

				var costGLAccount1 = TestObjectCreator.CreateGLHeader("39991001");
				var revenueGLAccount1 = TestObjectCreator.CreateGLHeader("39991002");

				Factory.Save();

				var chargeCode1 = TestObjectCreator.DSBChargeCode;
				chargeCode1.AC_AG_CostAccount = costGLAccount1.PK;
				chargeCode1.AC_AG_RevenueAccount = revenueGLAccount1.PK;
				chargeCode1.AC_AG_DisbursementShortfallAccount = shortfallGLAccount1.PK;
				chargeCode1.AC_AG_DisbursementSurplusAccount = surplusGLAccount1.PK;

				Factory.Save();

				var charge1Cst = TestObjectCreator.CreateCharge(job, chargeCode1, 100m, 100m);
				var header1 = TestObjectCreator.CreateInvoice(typeof(APInvoice));
				var line11 = TestObjectCreator.CreateCostLine(charge1Cst, header1.PK);
				line11.AL_AG = charge1Cst.ChargeCode.AC_AG_CostAccount;

				Factory.Save();

				Assert("Pre-condition", job.GetShouldJobBeClosedByDsbBatch());

				using (AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					job.JH_Status = JobHeaderStatus.Closed.Code;
					job.Validation.ValidateJH_Status();
					AssertNoErrors("JH_StatusInfo should have no errors", job.JH_StatusInfo);
				}

				using (AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					job.JH_Status = JobHeaderStatus.Closed.Code;
					job.Validation.ValidateJH_Status();
					AssertHasError(job.JH_StatusInfo, @"This job contains posted disbursement clearing balance and can only be closed via the Auto Job Closure process.");
				}
			}
		}

		public void TestCheckWarningForJH_GS_NKRepSalesWhenDefaultingSalesRepFromControllingCustomer_ControllingCustomerUseSalesRepRegistryOn()
		{
			AssertSalesRepFieldWarningWhenDefaultingFromControllingCustomer(true);
		}

		public void TestCheckWarningForJH_GS_NKRepSalesWhenDefaultingSalesRepFromControllingCustomer_ControllingCustomerUseSalesRepRegistryOff()
		{
			AssertSalesRepFieldWarningWhenDefaultingFromControllingCustomer(false);
		}

		void AssertSalesRepFieldWarningWhenDefaultingFromControllingCustomer(bool isControllingCustomerUseSalesRepRegistryOn)
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUSYD", "NZAKL");
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Assert(shipment.IsExport());
			Assert(shipment.IsAir);
			var job = TestObjectCreator.CreateJob(shipment);
			var controllingCustomerOrgWithSalesRep = TestObjectCreator.CreateOrgHeader("ABCORG", false, false);
			controllingCustomerOrgWithSalesRep.StaffAssignments.ExportAirRep = "ABC";
			var controllingCustomerOrgWithoutSalesRep = TestObjectCreator.CreateOrgHeader("XYZORG", false, false);
			controllingCustomerOrgWithoutSalesRep.StaffAssignments.ExportAirRep = ZString.Empty;
			Factory.Save();

			var expectedWarningMessageForLocalClientSalesRep = "You have not entered a Sales Rep.";
			var expectedWarningMessageForControllingCustomerSalesRep = "Controlling Customer does not have a Sales Rep assigned.";

			using (FreightDataRegistry.Instance.ControllingCustomerUseSalesRep.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isControllingCustomerUseSalesRepRegistryOn))
			{
				AssertNull("Shipment has no controlling customer.", shipment.InvoicingSupporter.ControllingCustomer);

				job.JH_GS_NKRepSales = "AAA";
				Assert(!job.JH_GS_NKRepSalesInfo.HasWarning(expectedWarningMessageForLocalClientSalesRep));
				Assert(!job.JH_GS_NKRepSalesInfo.HasWarning(expectedWarningMessageForControllingCustomerSalesRep));

				job.JH_GS_NKRepSales = ZString.Empty;
				Assert(job.JH_GS_NKRepSalesInfo.HasWarning(expectedWarningMessageForLocalClientSalesRep));
				Assert(!job.JH_GS_NKRepSalesInfo.HasWarning(expectedWarningMessageForControllingCustomerSalesRep));

				shipment.ControllingCustomerAddress.E2_OA_Address = controllingCustomerOrgWithSalesRep.MainAddress.PK;
				AssertNotNull("Shipment has controlling customer.", shipment.InvoicingSupporter.ControllingCustomer);
				Assert("Controlling customer has a sales rep.", !shipment.InvoicingSupporter.ControllingCustomer.StaffAssignments.ExportAirRep.IsEmpty);

				job.JH_GS_NKRepSales = "AAA";
				Assert(!job.JH_GS_NKRepSalesInfo.HasWarning(expectedWarningMessageForLocalClientSalesRep));
				Assert(!job.JH_GS_NKRepSalesInfo.HasWarning(expectedWarningMessageForControllingCustomerSalesRep));

				job.JH_GS_NKRepSales = ZString.Empty;
				job.Validation.ValidateJH_GS_NKRepSales();
				Assert(job.JH_GS_NKRepSalesInfo.HasWarning(expectedWarningMessageForLocalClientSalesRep));
				Assert(!job.JH_GS_NKRepSalesInfo.HasWarning(expectedWarningMessageForControllingCustomerSalesRep));

				shipment.ControllingCustomerAddress.E2_OA_Address = controllingCustomerOrgWithoutSalesRep.MainAddress.PK;
				Assert("Controlling customer does not have a sales rep.", shipment.InvoicingSupporter.ControllingCustomer.StaffAssignments.ExportAirRep.IsEmpty);

				job.JH_GS_NKRepSales = "AAA";
				Assert(!job.JH_GS_NKRepSalesInfo.HasWarning(expectedWarningMessageForLocalClientSalesRep));
				Assert(!job.JH_GS_NKRepSalesInfo.HasWarning(expectedWarningMessageForControllingCustomerSalesRep));

				job.JH_GS_NKRepSales = ZString.Empty;
				job.Validation.ValidateJH_GS_NKRepSales();
				AssertEquals(!isControllingCustomerUseSalesRepRegistryOn, job.JH_GS_NKRepSalesInfo.HasWarning(expectedWarningMessageForLocalClientSalesRep));
				AssertEquals(isControllingCustomerUseSalesRepRegistryOn, job.JH_GS_NKRepSalesInfo.HasWarning(expectedWarningMessageForControllingCustomerSalesRep));
			}
		}

		public void TestGetGroupsOfCharges()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var job = creator.Job1;

			var charge = creator.CreateMarginCharge(job, creator.AUD, creator.AUD, 200m);
			var charge1 = creator.CreateDSBCharge(job, creator.AUD, creator.AUD, 100m, GlbBranch.CurrentBranch, creator.FEADepartment, creator.DSBChargeCode);
			var charge2 = creator.CreateDSBCharge(job, creator.USD, creator.AUD, 100m, GlbBranch.CurrentBranch, creator.FEADepartment, creator.DSBChargeCode);
			var charge3 = creator.CreateDSBCharge(job, creator.AUD, creator.AUD, 100m, creator.NonCurrentBranch, creator.FEADepartment, creator.DSBChargeCode);
			var charge4 = creator.CreateDSBCharge(job, creator.USD, creator.AUD, 100m, creator.NonCurrentBranch, creator.FEADepartment, creator.DSBChargeCode);
			var charge5 = creator.CreateDSBCharge(job, creator.AUD, creator.AUD, 100m, GlbBranch.CurrentBranch, creator.FESDepartment, creator.DSBChargeCode);
			var charge6 = creator.CreateDSBCharge(job, creator.USD, creator.AUD, 100m, GlbBranch.CurrentBranch, creator.FESDepartment, creator.DSBChargeCode);
			var charge7 = creator.CreateDSBCharge(job, creator.AUD, creator.AUD, 100m, creator.NonCurrentBranch, creator.FESDepartment, creator.DSBChargeCode);
			var charge8 = creator.CreateDSBCharge(job, creator.USD, creator.AUD, 100m, creator.NonCurrentBranch, creator.FESDepartment, creator.DSBChargeCode);
			var charge9 = creator.CreateDSBCharge(job, creator.AUD, creator.AUD, 100m, GlbBranch.CurrentBranch, creator.FEADepartment, creator.DSBChargeCode1);
			var charge10 = creator.CreateDSBCharge(job, creator.USD, creator.AUD, 100m, GlbBranch.CurrentBranch, creator.FEADepartment, creator.DSBChargeCode1);
			var charge11 = creator.CreateDSBCharge(job, creator.AUD, creator.AUD, 100m, creator.NonCurrentBranch, creator.FEADepartment, creator.DSBChargeCode1);
			var charge12 = creator.CreateDSBCharge(job, creator.USD, creator.AUD, 100m, creator.NonCurrentBranch, creator.FEADepartment, creator.DSBChargeCode1);
			var charge13 = creator.CreateDSBCharge(job, creator.AUD, creator.AUD, 100m, GlbBranch.CurrentBranch, creator.FESDepartment, creator.DSBChargeCode1);
			var charge14 = creator.CreateDSBCharge(job, creator.USD, creator.AUD, 100m, GlbBranch.CurrentBranch, creator.FESDepartment, creator.DSBChargeCode1);
			var charge15 = creator.CreateDSBCharge(job, creator.AUD, creator.AUD, 100m, creator.NonCurrentBranch, creator.FESDepartment, creator.DSBChargeCode1);
			var charge16 = creator.CreateDSBCharge(job, creator.USD, creator.AUD, 100m, creator.NonCurrentBranch, creator.FESDepartment, creator.DSBChargeCode1);
			var charge17 = creator.CreateDSBCharge(job, creator.AUD, creator.USD, 100m, GlbBranch.CurrentBranch, creator.FEADepartment, creator.DSBChargeCode);
			var charge18 = creator.CreateDSBCharge(job, creator.USD, creator.USD, 100m, GlbBranch.CurrentBranch, creator.FEADepartment, creator.DSBChargeCode);
			var charge19 = creator.CreateDSBCharge(job, creator.AUD, creator.USD, 100m, creator.NonCurrentBranch, creator.FEADepartment, creator.DSBChargeCode);
			var charge20 = creator.CreateDSBCharge(job, creator.USD, creator.USD, 100m, creator.NonCurrentBranch, creator.FEADepartment, creator.DSBChargeCode);
			var charge21 = creator.CreateDSBCharge(job, creator.AUD, creator.USD, 100m, GlbBranch.CurrentBranch, creator.FESDepartment, creator.DSBChargeCode);
			var charge22 = creator.CreateDSBCharge(job, creator.USD, creator.USD, 100m, GlbBranch.CurrentBranch, creator.FESDepartment, creator.DSBChargeCode);
			var charge23 = creator.CreateDSBCharge(job, creator.AUD, creator.USD, 100m, creator.NonCurrentBranch, creator.FESDepartment, creator.DSBChargeCode);
			var charge24 = creator.CreateDSBCharge(job, creator.USD, creator.USD, 100m, creator.NonCurrentBranch, creator.FESDepartment, creator.DSBChargeCode);
			var charge25 = creator.CreateDSBCharge(job, creator.AUD, creator.USD, 100m, GlbBranch.CurrentBranch, creator.FEADepartment, creator.DSBChargeCode1);
			var charge26 = creator.CreateDSBCharge(job, creator.USD, creator.USD, 100m, GlbBranch.CurrentBranch, creator.FEADepartment, creator.DSBChargeCode1);
			var charge27 = creator.CreateDSBCharge(job, creator.AUD, creator.USD, 100m, creator.NonCurrentBranch, creator.FEADepartment, creator.DSBChargeCode1);
			var charge28 = creator.CreateDSBCharge(job, creator.USD, creator.USD, 100m, creator.NonCurrentBranch, creator.FEADepartment, creator.DSBChargeCode1);
			var charge29 = creator.CreateDSBCharge(job, creator.AUD, creator.USD, 100m, GlbBranch.CurrentBranch, creator.FESDepartment, creator.DSBChargeCode1);
			var charge30 = creator.CreateDSBCharge(job, creator.USD, creator.USD, 100m, GlbBranch.CurrentBranch, creator.FESDepartment, creator.DSBChargeCode1);
			var charge31 = creator.CreateDSBCharge(job, creator.AUD, creator.USD, 100m, creator.NonCurrentBranch, creator.FESDepartment, creator.DSBChargeCode1);
			var charge32 = creator.CreateDSBCharge(job, creator.USD, creator.USD, 100m, creator.NonCurrentBranch, creator.FESDepartment, creator.DSBChargeCode1);

			var validation = new JobValidation(job);

			var groups = validation.GetGroupsOfCharges(GroupingStrategyForCharges.ToCheckFailureForForeignAmountByCostCurrency);
			Assert("Number of groups", groups.Count() == 16);
			groups.ForEach(c => Assert("Number of charges per group", c.Count() == 2));

			groups = validation.GetGroupsOfCharges(GroupingStrategyForCharges.ToCheckFailureForForeignAmountBySellCurrency);
			Assert("Number of groups", groups.Count() == 16);
			groups.ForEach(c => Assert("Number of charges per group", c.Count() == 2));

			groups = validation.GetGroupsOfCharges(GroupingStrategyForCharges.ToCheckFailureForLocalAmount);
			Assert("Number of groups", groups.Count() == 8);
			groups.ForEach(c => Assert("Number of charges per group", c.Count() == 4));
		}

		[ExpectNoExceptions]
		public void TestGetFallBackValueAtAllLevels_IsLocaleIndependent()
		{
			CultureInfo originalCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

			// Serialize objects with the locale being Australian english
			System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-AU");
			TestObjectCreator creator = new TestObjectCreator(Factory);

			var mockSupporter = TestObjectCreator.GetIJobInvoicingSupporterMock();
			IJobInvoicingPlugIn shipment = TestObjectCreator.GetTestShipmentPlugIn("S00001234", mockSupporter);

			TestJob.PlugInData = shipment;
			TestJob.JH_ProfitLossReasonCode = "";

			Factory.Save();

			JobProfitLossRequiringReasonParameters plRequiringReasonParameters = new JobProfitLossRequiringReasonParameters();
			plRequiringReasonParameters.ProfitThreshold = 1000.01M;
			plRequiringReasonParameters.JobStatusCollection.AddNew().Code = TestJob.JobStatusList[TestJob.JobStatusList.Count - 1].Code;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(TestJob.JH_GC.ToGuid(), Guid.Empty, Guid.Empty, plRequiringReasonParameters);

			Factory.Save();

			// Deserialize the objects using France's french
			System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("fr-FR");
			object fallbackValue = AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.GetFallBackValueAtAllLevels(TestJob.JH_GC.ToGuid(), Guid.Empty, Guid.Empty);

			// Put culture info back to what it was
			System.Threading.Thread.CurrentThread.CurrentCulture = originalCulture;
		}

		public void TestCheckGatewayConsolChildShipmentsJobsMutexesNotLocked()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var creator = new TestObjectCreator(Factory);
			var consol = TestObjectCreator.CreateGatewayConsol("KRSEL", "AUSYD", "C1", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = TestObjectCreator.CreateShipment("S1", consol);
			Factory.Save();

			AssertNull("Precondition: shipment job is not created", new JobHeader.Loader(shipment).Load());

			var job = new JobHeader.Loader(consol).TryLoadOrCreateWithoutMutexForTestOnly();

			using (var shipmentJob1 = new JobHeader.Loader(new BusinessObjectFactory(), shipment).TryCreateWithMutex())
			{
				job.Validation.ValidateAll();
				Assert(job.RowErrors.GetFirstMessage().Contains("on another form, but haven't saved it yet"));
			}
		}

		[SuspendCriticalValidation]
		public void TestJH_ProfitLoss_Validation()
		{
			AccountingMasterFilesRegistry.Instance.OrphanWIPOrACRDetection.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var shipment1 = TestObjectCreator.CreateShipment("S001001", false);
			var job1 = TestObjectCreator.CreateJob(shipment1, false, false);

			var wip1 = TestObjectCreator.CreateWIP(job1);
			Factory.Save();

			Assert("Precondition", !job1.HasOrphanWIPsorAccruals);

			JobValidation validation = job1.Validation as JobValidation;
			validation.ValidateJH_ProfitLoss();
			AssertNoErrors(job1.JH_ProfitLossInfo);

			var charge = wip1.LoadRelatedJobCharge();
			charge.SetARLineForcedForTest(Guid.Empty);
			Factory.Save();

			Assert("Precondition", job1.HasOrphanWIPsorAccruals);

			validation.ValidateJH_ProfitLoss();

			string expectedWarningMessage =
@"There are WIPs or Accruals linked to this Job which should be reversed but are not. As a result the Profit and Loss figure might not be accurate.
Please review the costs and revenues entered for this job.";
			AssertHasWarning(job1.JH_ProfitLossInfo, expectedWarningMessage);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertNoErrors(job1.JH_ProfitLossInfo);

			validation.ValidateAll(); //Checking that ValidateAll() calls ValidateJH_ProfitLoss()
			AssertHasWarning(job1.JH_ProfitLossInfo, expectedWarningMessage);
		}

		public void TestLocalClientAddressContact_Validation()
		{
			OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "LCLORG";
			localClient.OH_IsDebtor = true;
			localClient.MiscServ.OM_ARCreditLimit = 99m;
			localClient.OH_IsActive = true;

			OrgHeader localClient2 = Factory.NewWithValidTestData<OrgHeader>();
			localClient2.OH_Code = "LCLORG2";
			localClient2.OH_IsDebtor = true;
			localClient2.MiscServ.OM_ARCreditLimit = 99m;
			localClient2.OH_IsActive = true;

			Factory.Save();

			var address1 = TestObjectCreator.CreateAddress(localClient, "1st Street");
			var address2 = TestObjectCreator.CreateAddress(localClient, "2nd Street");

			var address3 = TestObjectCreator.CreateAddress(localClient2, "3rd Street");

			var agentAddress = TestObjectCreator.CreateAddress(TestObjectCreator.Agent);

			var contact1 = TestObjectCreator.CreateContact(localClient, "contact 1");
			var contact2 = TestObjectCreator.CreateContact(localClient, "contact 2");

			var contact3 = TestObjectCreator.CreateContact(localClient2, "contact 3");

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = TestObjectCreator.CreateJob(shipment, false, false);

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M);
			var apInvoiceLine = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1M, 120M);

			job.JH_OA_AgentCollectAddr = agentAddress.PK;
			job.JH_OA_LocalChargesAddr = Guid.Empty;
			job.JH_OC_LocalBillingContact = Guid.Empty;
			AssertNoErrors(job.JH_OA_LocalChargesAddrInfo);
			AssertNoErrors(job.JH_OC_LocalBillingContactInfo);

			JobValidation validation = job.Validation as JobValidation;

			Assert("Precondition", !job.IsAnyCostOrRevenuePosted());

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = true;
			validation.ValidateJH_OA_LocalChargesAddr();
			validation.ValidateJH_OC_LocalBillingContact();
			AssertNoErrors(job.JH_OA_LocalChargesAddrInfo);
			AssertNoErrors(job.JH_OC_LocalBillingContactInfo);

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = false;
			validation.ValidateJH_OA_LocalChargesAddr();
			validation.ValidateJH_OC_LocalBillingContact();
			AssertNoErrors(job.JH_OA_LocalChargesAddrInfo);
			AssertNoErrors(job.JH_OC_LocalBillingContactInfo);

			apInvoiceLine.AL_JH = job.PK;
			apInvoiceLine.AL_AC = TestObjectCreator.FRT.PK;

			var jobCharge = TestObjectCreator.CreateCharge(apInvoiceLine); //post cost charge

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var jobInNewFactory = newFactory.Load<Job>(job.PK);
			validation = jobInNewFactory.Validation as JobValidation;

			Assert("Precondition", jobInNewFactory.IsAnyCostOrRevenuePosted());
			Assert("Precondition", !jobInNewFactory.IsAnyRevenuePosted());

			#region precondition: no change - no error
			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = true;
			validation.ValidateJH_OA_LocalChargesAddr();
			validation.ValidateJH_OC_LocalBillingContact();
			AssertNoErrors(jobInNewFactory.JH_OA_LocalChargesAddrInfo);
			AssertNoErrors(jobInNewFactory.JH_OC_LocalBillingContactInfo);

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = false;
			validation.ValidateJH_OA_LocalChargesAddr();
			validation.ValidateJH_OC_LocalBillingContact();
			AssertNoErrors(jobInNewFactory.JH_OA_LocalChargesAddrInfo);
			AssertNoErrors(jobInNewFactory.JH_OC_LocalBillingContactInfo);
			#endregion

			#region change address and contact produces no error for posted COST charge (add1)
			jobInNewFactory.JH_OA_LocalChargesAddr = address1.PK;
			jobInNewFactory.JH_OC_LocalBillingContact = contact1.PK;

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = true;
			validation.ValidateJH_OA_LocalChargesAddr();
			validation.ValidateJH_OC_LocalBillingContact();
			AssertNoErrors(jobInNewFactory.JH_OA_LocalChargesAddrInfo);
			AssertNoErrors(jobInNewFactory.JH_OC_LocalBillingContactInfo);

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = false;
			validation.ValidateJH_OA_LocalChargesAddr();
			validation.ValidateJH_OC_LocalBillingContact();
			AssertNoErrors(jobInNewFactory.JH_OA_LocalChargesAddrInfo);
			AssertNoErrors(jobInNewFactory.JH_OC_LocalBillingContactInfo);
			#endregion

			#region post AR charge
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.USD, 2M);
			var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 2M, 220M);
			arInvoiceLine.AL_JH = job.PK;
			arInvoiceLine.AL_AC = TestObjectCreator.FRT.PK;
			var jobRevCharge = TestObjectCreator.CreateCharge(arInvoiceLine); //post rev charge

			//update base rates to allow saving;
			foreach (var rate in job.ExchangeRates.Cast<ExchangeRate>().Where(r => r.JF_BaseRate.IsEmpty))
			{
				rate.JF_BaseRate = 1m;
			}

			Factory.Save();

			newFactory = new BusinessObjectFactory();
			jobInNewFactory = newFactory.Load<Job>(job.PK);
			validation = jobInNewFactory.Validation as JobValidation;
			#endregion

			#region contact/address change when revenue posted produces error unless allowed (add1)
			Assert("Precondition", jobInNewFactory.IsAnyRevenuePosted());
			jobInNewFactory.JH_OA_LocalChargesAddr = address1.PK;
			jobInNewFactory.JH_OC_LocalBillingContact = contact1.PK;

			string expectedAddressErrorMessage =
@"You cannot modify Local Client address once a revenue charge is posted. 
You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Management -> Modify Local Client address/contact and Overseas agent address after charges are posted";

			AssertHasError("Should have error as user does not have permission, atleast one charge posted and user attempts to change local client address.", jobInNewFactory.JH_OA_LocalChargesAddrInfo, expectedAddressErrorMessage);

			string expectedContactErrorMessage =
@"You cannot modify Local Client contact once a revenue charge is posted. 
You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Management -> Modify Local Client address/contact and Overseas agent address after charges are posted";

			AssertHasError("Should have error as user does not have permission, atleast one charge posted and user attempts to change local client contact.", jobInNewFactory.JH_OC_LocalBillingContactInfo, expectedContactErrorMessage);

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = true;
			validation.ValidateJH_OA_LocalChargesAddr();
			AssertNoErrors(jobInNewFactory.JH_OA_AgentCollectAddrInfo);

			validation.ValidateJH_OC_LocalBillingContact();
			AssertNoErrors(jobInNewFactory.JH_OC_LocalBillingContactInfo);

			newFactory.Save();
			#endregion

			#region add1 to add2 and empty guild fails
			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = false;
			jobInNewFactory.JH_OA_LocalChargesAddr = address2.PK;
			jobInNewFactory.JH_OC_LocalBillingContact = contact2.PK;

			expectedAddressErrorMessage =
@"You cannot modify Local Client address once a revenue charge is posted. 
Original address: '1st Street'
You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Management -> Modify Local Client address/contact and Overseas agent address after charges are posted";
			AssertHasError("Should have error as user does not have permission, atleast one charge posted and user attempts to change local client address - original address shown.", jobInNewFactory.JH_OA_LocalChargesAddrInfo, expectedAddressErrorMessage);

			expectedContactErrorMessage =
@"You cannot modify Local Client contact once a revenue charge is posted. 
Original contact: 'contact 1'
You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Management -> Modify Local Client address/contact and Overseas agent address after charges are posted";

			AssertHasError("Should have error as user does not have permission, atleast one charge posted and user attempts to change local client contact - original contact shown.", jobInNewFactory.JH_OC_LocalBillingContactInfo, expectedContactErrorMessage);

			jobInNewFactory.JH_OA_LocalChargesAddr = Guid.Empty;

			expectedAddressErrorMessage =
@"You cannot modify Local Client address once a revenue charge is posted. 
Original organization: 'LCLORG'
Original address: '1st Street'
You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Management -> Modify Local Client address/contact and Overseas agent address after charges are posted";

			AssertHasError("Should have error as user does not have permission, atleast one charge posted and user attempts to empty local client addess - both original address and org shown.", jobInNewFactory.JH_OA_LocalChargesAddrInfo, expectedAddressErrorMessage);

			jobInNewFactory.JH_OC_LocalBillingContact = Guid.Empty;

			expectedContactErrorMessage =
@"You cannot modify Local Client contact once a revenue charge is posted. 
Original organization: 'LCLORG'
Original contact: 'contact 1'
You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Management -> Modify Local Client address/contact and Overseas agent address after charges are posted";

			AssertHasError("Should have error as user does not have permission, atleast one charge posted and user attempts to empty local client contact - both original contact and org shown.", jobInNewFactory.JH_OC_LocalBillingContactInfo, expectedContactErrorMessage);
			#endregion

			#region addres 3 to add 1
			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = true;
			jobInNewFactory.JH_OA_LocalChargesAddr = address3.PK;
			validation.ValidateJH_OA_LocalChargesAddr();
			AssertNoErrors(jobInNewFactory.JH_OA_AgentCollectAddrInfo);

			jobInNewFactory.JH_OC_LocalBillingContact = contact3.PK;
			validation.ValidateJH_OC_LocalBillingContact();
			AssertNoErrors(jobInNewFactory.JH_OC_LocalBillingContactInfo);

			newFactory.Save();

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = false;
			jobInNewFactory.JH_OA_LocalChargesAddr = address1.PK;

			expectedAddressErrorMessage =
@"You cannot modify Local Client address once a revenue charge is posted. 
Original organization: 'LCLORG2'
Original address: '3rd Street'
You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Management -> Modify Local Client address/contact and Overseas agent address after charges are posted";

			AssertHasError("Should have error as user does not have permission, atleast one charge posted and user attempts to empty local client addess - both original address and org shown.", jobInNewFactory.JH_OA_LocalChargesAddrInfo, expectedAddressErrorMessage);

			jobInNewFactory.JH_OC_LocalBillingContact = contact1.PK;

			expectedContactErrorMessage =
@"You cannot modify Local Client contact once a revenue charge is posted. 
Original organization: 'LCLORG2'
Original contact: 'contact 3'
You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Management -> Modify Local Client address/contact and Overseas agent address after charges are posted";

			AssertHasError("Should have error as user does not have permission, atleast one charge posted and user attempts to empty local client contact - both original contact and org shown.", jobInNewFactory.JH_OC_LocalBillingContactInfo, expectedContactErrorMessage);
			#endregion
		}

		#region Test ValidateOrganisation

		public void TestValidateOrganisation_OOQHasLocalClientHasCharges_NoError()
		{
			ValidateOrganisation_OOQRunner(true, true, string.Empty);
		}

		public void TestValidateOrganisation_OOQHasLocalClientNoCharges_NoError()
		{
			ValidateOrganisation_OOQRunner(true, false, string.Empty);
		}

		public void TestValidateOrganisation_OOQNoLocalClientNoCharges_NoError()
		{
			ValidateOrganisation_OOQRunner(false, false, string.Empty);
		}

		public void TestValidateOrganisation_OOQNoLocalClientHasCharges_ProducesError()
		{
			ValidateOrganisation_OOQRunner(false, true, "Please enter Local Client or Overseas Agent.");
		}

		void ValidateOrganisation_OOQRunner(bool hasLocalClient, bool hasCharges, string error)
		{
			var oneOffQuote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			oneOffQuote.TH_OneTimeQuote = true;

			var job = new Job.Loader(oneOffQuote).TryCreateWithoutMutexForTestOnly();
			OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
			job.LocalChargesPK = hasLocalClient ? localClient.PK : Guid.Empty;
			if (hasCharges)
			{
				job.Charges.AddNew();
			}

			JobValidation validation = job.Validation as JobValidation;
			var result = validation.ValidateOrganisation(true);
			AssertEquals(error, result);
		}

		public void TestValidateOrganisation_NullParentJobType_ProducesError()
		{
			var oneOffQuote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);

			var job = new Job.Loader(oneOffQuote).TryCreateWithoutMutexForTestOnly();
			job.LocalChargesPK = Guid.Empty;
			AssertNull(job.JobType?.Code);

			JobValidation validation = job.Validation as JobValidation;
			var result = validation.ValidateOrganisation(false);

			AssertEquals("Please enter Local Client or Overseas Agent.", result);
		}

		#endregion

		public void TestLocalClientAddressValidationWhenOriginalAddressHeaderIsMerged()
		{
			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = false;

			var org1 = TestObjectCreator.CreateOrgHeader("Org1", true, true);
			var address1 = TestObjectCreator.CreateAddress(org1, "25st Street");
			var org2 = TestObjectCreator.CreateOrgHeader("Org2", true, true);
			var address2 = TestObjectCreator.CreateAddress(org2, "26st Street");

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = TestObjectCreator.CreateJob(shipment, false, false);
			job.JH_OA_LocalChargesAddr = address1.PK;
			Factory.Save();

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.USD, 2M, TestObjectCreator.Debtor);
			var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 2M, 220M);
			arInvoiceLine.AL_JH = job.PK;
			arInvoiceLine.AL_AC = TestObjectCreator.FRT.PK;
			TestObjectCreator.CreateCharge(arInvoiceLine);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var mergeOrgHeader = new MergeOrgHeader(factory2, org1, TestObjectCreator.AALSHI);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			job.JH_OA_LocalChargesAddr = address2.PK;
			job.Validation.ValidateJH_OA_LocalChargesAddr();

			var expectedErrorMessage =
@"You cannot modify Local Client address once a revenue charge is posted. 
You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Management -> Modify Local Client address/contact and Overseas agent address after charges are posted";

			AssertHasError(job.JH_OA_LocalChargesAddrInfo, expectedErrorMessage);
		}

		public void TestLocalChargesAddrValidationForGateWayConsol()
		{
			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var proxyBranch = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany, branchOrgProxy);

			var gatewayConsol = TestObjectCreator.CreateConsol("USCHI", "AUSYD", "C00100192");
			gatewayConsol.JK_AgentType = Constants.AgentType.Agent;
			gatewayConsol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = "USCHI";
			port.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_SendingForwarderAddress;
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			gatewayConsol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);
			var gatewayConsolJob = new JobHeader.Loader(gatewayConsol).TryLoadOrCreateWithoutMutexForTestOnly();
			Factory.Save();

			AssertEquals("Precondition: Is Gateway billing Job", true, gatewayConsolJob.IsGatewayBillingJob());
			AssertEquals("Precondition: Prepaid Agent should default to Sending Forwarder", gatewayConsol.SendingForwarder.PK, gatewayConsolJob.LocalChargesPK);
			AssertNoErrorContaining(gatewayConsolJob.JH_OA_LocalChargesAddrInfo, "Prepaid Agent has to be a proxy of the Gateway Agents Company.");

			gatewayConsolJob.LocalChargesPK = ZGuid.Empty;
			AssertHasErrorContaining(gatewayConsolJob.JH_OA_LocalChargesAddrInfo, "Prepaid Agent has to be a proxy of the Gateway Agents Company.");

			gatewayConsolJob.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertHasErrorContaining(gatewayConsolJob.JH_OA_LocalChargesAddrInfo, "Prepaid Agent has to be a proxy of the Gateway Agents Company.");

			gatewayConsolJob.LocalChargesPK = branchOrgProxy.PK;
			AssertNoErrorContaining(gatewayConsolJob.JH_OA_LocalChargesAddrInfo, "Prepaid Agent has to be a proxy of the Gateway Agents Company.");

			proxyBranch.GB_IsActive = false;
			Factory.Save();
			AssertHasErrorContaining(gatewayConsolJob.JH_OA_LocalChargesAddrInfo, "Prepaid Agent has to be a proxy of the Gateway Agents Company.");
		}

		public void TestAgentCollectAddrValidationForGateWayConsol()
		{
			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var proxyBranch = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany, branchOrgProxy);

			var gatewayConsol = TestObjectCreator.CreateConsol("USCHI", "AUSYD", "C00100192");
			gatewayConsol.JK_AgentType = Constants.AgentType.Agent;
			gatewayConsol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayConsol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = "AUSYD";
			port.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_ReceivingForwarderAddress;
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			gatewayConsol.ReceivingForwarder.AppointedGatewayAgentPorts.Add(port);
			var gatewayConsolJob = new JobHeader.Loader(gatewayConsol).TryLoadOrCreateWithoutMutexForTestOnly();
			Factory.Save();

			AssertEquals("Precondition: Is Gateway billing Job", true, gatewayConsolJob.IsGatewayBillingJob());
			AssertEquals("Precondition: Collect Agent should default to Receiving Forwarder", gatewayConsol.ReceivingForwarder.PK, gatewayConsolJob.AgentCollectPK);
			AssertNoErrorContaining(gatewayConsolJob.JH_OA_AgentCollectAddrInfo, "Collect Agent has to be a proxy of the Gateway Agents Company.");

			gatewayConsolJob.AgentCollectPK = ZGuid.Empty;
			AssertHasErrorContaining(gatewayConsolJob.JH_OA_AgentCollectAddrInfo, "Collect Agent has to be a proxy of the Gateway Agents Company.");

			gatewayConsolJob.AgentCollectPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertHasErrorContaining(gatewayConsolJob.JH_OA_AgentCollectAddrInfo, "Collect Agent has to be a proxy of the Gateway Agents Company.");

			gatewayConsolJob.AgentCollectPK = branchOrgProxy.PK;
			AssertNoErrorContaining(gatewayConsolJob.JH_OA_AgentCollectAddrInfo, "Collect Agent has to be a proxy of the Gateway Agents Company.");

			proxyBranch.GB_IsActive = false;
			Factory.Save();
			AssertHasErrorContaining(gatewayConsolJob.JH_OA_AgentCollectAddrInfo, "Collect Agent has to be a proxy of the Gateway Agents Company.");
		}

		public void TestOverseasAgentAddress_Validation()
		{
			OrgHeader oSAgent = Factory.NewWithValidTestData<OrgHeader>();
			oSAgent.OH_Code = "AGTORG";
			oSAgent.OH_IsDebtor = true;
			oSAgent.MiscServ.OM_ARCreditLimit = 99m;
			oSAgent.OH_IsActive = true;

			OrgHeader oSAgent2 = Factory.NewWithValidTestData<OrgHeader>();
			oSAgent2.OH_Code = "AGTORG2";
			oSAgent2.OH_IsDebtor = true;
			oSAgent2.MiscServ.OM_ARCreditLimit = 99m;
			oSAgent2.OH_IsActive = true;

			Factory.Save();

			var address1 = TestObjectCreator.CreateAddress(oSAgent, "1st Street");
			var address2 = TestObjectCreator.CreateAddress(oSAgent, "2nd Street");

			var address3 = TestObjectCreator.CreateAddress(oSAgent2, "3rd Street");

			var localClientAddress = TestObjectCreator.CreateAddress(TestObjectCreator.LocalClient);

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = TestObjectCreator.CreateJob(shipment, false, false);

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M);
			var apInvoiceLine = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1M, 120M);

			job.JH_OA_AgentCollectAddr = ZGuid.Empty;
			job.JH_OA_LocalChargesAddr = localClientAddress.PK;
			AssertNoErrors(job.JH_OA_AgentCollectAddrInfo);

			JobValidation validation = job.Validation as JobValidation;

			Assert("Precondition", !job.IsAnyCostOrRevenuePosted());

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = true;
			validation.ValidateJH_OA_AgentCollectAddr();
			AssertNoErrors(job.JH_OA_AgentCollectAddrInfo);

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = false;
			validation.ValidateJH_OA_AgentCollectAddr();
			AssertNoErrors(job.JH_OA_AgentCollectAddrInfo);

			apInvoiceLine.AL_JH = job.PK;
			apInvoiceLine.AL_AC = TestObjectCreator.FRT.PK;

			var jobCharge = TestObjectCreator.CreateCharge(apInvoiceLine); //post cost charge

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var jobInNewFactory = newFactory.Load<Job>(job.PK);
			validation = jobInNewFactory.Validation as JobValidation;

			Assert("Precondition", jobInNewFactory.IsAnyCostOrRevenuePosted());
			Assert("Precondition", !jobInNewFactory.IsAnyRevenuePosted());

			#region precondition: no change - no error
			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = true;
			validation.ValidateJH_OA_AgentCollectAddr();
			AssertNoErrors(jobInNewFactory.JH_OA_LocalChargesAddrInfo);

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = false;
			validation.ValidateJH_OA_AgentCollectAddr();
			AssertNoErrors(jobInNewFactory.JH_OA_LocalChargesAddrInfo);
			#endregion

			#region change address and contact produces no error for posted COST charge (add1)
			jobInNewFactory.JH_OA_AgentCollectAddr = address1.PK;

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = true;
			validation.ValidateJH_OA_AgentCollectAddr();
			AssertNoErrors(jobInNewFactory.JH_OA_LocalChargesAddrInfo);

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = false;
			validation.ValidateJH_OA_AgentCollectAddr();
			AssertNoErrors(jobInNewFactory.JH_OA_LocalChargesAddrInfo);
			#endregion

			#region post AR charge
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.USD, 2M);
			var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 2M, 220M);
			arInvoiceLine.AL_JH = job.PK;
			arInvoiceLine.AL_AC = TestObjectCreator.FRT.PK;
			var jobRevCharge = TestObjectCreator.CreateCharge(arInvoiceLine); //post rev charge

			job.ExchangeRates[0].JF_BaseRate = 1m;

			Factory.Save();

			newFactory = new BusinessObjectFactory();
			jobInNewFactory = newFactory.Load<Job>(job.PK);
			validation = jobInNewFactory.Validation as JobValidation;
			#endregion

			#region contact/address change when revenue posted produces error unless allowed (add1)
			Assert("Precondition", jobInNewFactory.IsAnyRevenuePosted());
			jobInNewFactory.JH_OA_AgentCollectAddr = address1.PK;

			string expectedAddressErrorMessage =
@"You cannot modify Overseas Agent address once a revenue charge is posted. 
You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Management -> Modify Local Client address/contact and Overseas agent address after charges are posted";

			AssertHasError("Should have error as user does not have permission, atleast one charge posted and user attempts to change local client address.", jobInNewFactory.JH_OA_AgentCollectAddrInfo, expectedAddressErrorMessage);

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = true;
			validation.ValidateJH_OA_AgentCollectAddr();
			AssertNoErrors(jobInNewFactory.JH_OA_AgentCollectAddrInfo);

			newFactory.Save();
			#endregion

			#region add1 to add2 and empty guild fails
			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = false;
			jobInNewFactory.JH_OA_AgentCollectAddr = address2.PK;

			expectedAddressErrorMessage =
@"You cannot modify Overseas Agent address once a revenue charge is posted. 
Original address: '1st Street'
You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Management -> Modify Local Client address/contact and Overseas agent address after charges are posted";
			AssertHasError("Should have error as user does not have permission, atleast one charge posted and user attempts to change local client address - original address shown.", jobInNewFactory.JH_OA_AgentCollectAddrInfo, expectedAddressErrorMessage);

			jobInNewFactory.JH_OA_AgentCollectAddr = Guid.Empty;

			expectedAddressErrorMessage =
@"You cannot modify Overseas Agent address once a revenue charge is posted. 
Original organization: 'AGTORG'
Original address: '1st Street'
You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Management -> Modify Local Client address/contact and Overseas agent address after charges are posted";

			AssertHasError("Should have error as user does not have permission, atleast one charge posted and user attempts to empty local client addess - both original address and org shown.", jobInNewFactory.JH_OA_AgentCollectAddrInfo, expectedAddressErrorMessage);

			#endregion

			#region addres 3 to add 1
			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = true;
			jobInNewFactory.JH_OA_AgentCollectAddr = address3.PK;
			validation.ValidateJH_OA_AgentCollectAddr();
			AssertNoErrors(jobInNewFactory.JH_OA_AgentCollectAddrInfo);

			newFactory.Save();

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = false;
			jobInNewFactory.JH_OA_AgentCollectAddr = address1.PK;

			expectedAddressErrorMessage =
@"You cannot modify Overseas Agent address once a revenue charge is posted. 
Original organization: 'AGTORG2'
Original address: '3rd Street'
You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Management -> Modify Local Client address/contact and Overseas agent address after charges are posted";

			AssertHasError("Should have error as user does not have permission, atleast one charge posted and user attempts to empty local client addess - both original address and org shown.", jobInNewFactory.JH_OA_AgentCollectAddrInfo, expectedAddressErrorMessage);

			#endregion
		}

		public void TestAgentsHaveCorrectNamesInWarnings_GatewayJob()
		{
			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			using (var consolJob = TestObjectCreator.CreateJob(consol))
			{
				AssertAgentsHaveCorrectNamesInWarnings(consolJob);
			}
		}

		public void TestAgentsHaveCorrectNamesInWarnings_Shipment()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			using (var shipmentJob = TestObjectCreator.CreateJob(shipment))
			{
				AssertAgentsHaveCorrectNamesInWarnings(shipmentJob);
			}
		}

		public void TestAgentsHaveCorrectNamesInWarnings_NoJobType()
		{
			AssertNull(TestObjectCreator.Job1.JobType);
			AssertAgentsHaveCorrectNamesInWarnings(TestObjectCreator.Job1);
		}

		public void TestAgentsHaveCorrectNamesInWarnings_CrossTrade()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "USLAX";

			using (var shipmentJob = TestObjectCreator.CreateJob(shipment))
			using (AccountingMasterFilesRegistry.Instance.EnableCrossTradeDebtorDefaultingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertAgentsHaveCorrectNamesInWarnings(shipmentJob);
			}
		}

		public void AssertAgentsHaveCorrectNamesInWarnings(Job job)
		{
			var localChargesName = job.JobType?.LocalClientText ?? "Local Client";
			var agentCollectName = job.JobType?.OverseasAgentText ?? "Overseas Agent";

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.USD, 2M);
			var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 2M, 220M);
			arInvoiceLine.AL_JH = job.PK;
			arInvoiceLine.AL_AC = TestObjectCreator.FRT.PK;
			TestObjectCreator.CreateCharge(arInvoiceLine);
			job.ExchangeRates[0].JF_BaseRate = 1m;

			if (job.CanCrossTradeDebtorDefaultingBeApplied)
			{
				localChargesName = job.JobType?.PrepaidBillToPartyText ?? "Prepaid Bill-To Party";
				agentCollectName = job.JobType?.CollectBillToPartyText ?? "Collect Bill-To Party";
			}

			job.JH_OA_AgentCollectAddr = ZGuid.Empty;
			job.JH_OA_LocalChargesAddr = ZGuid.Empty;
			Factory.Save();
			job.Validation.ValidateJH_OA_AgentCollectAddr();
			job.Validation.ValidateJH_OA_LocalChargesAddr();

			AssertHasError(job.JH_OA_LocalChargesAddrInfo, $"Please enter {localChargesName} or {agentCollectName}.");

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			job.JH_OA_AgentCollectAddr = org1.MainAddress.PK;
			job.JH_OA_LocalChargesAddr = org1.MainAddress.PK;
			job.Validation.ValidateJH_OA_AgentCollectAddr();
			job.Validation.ValidateJH_OA_LocalChargesAddr();

			AssertHasError(job.JH_OA_LocalChargesAddrInfo, $"{agentCollectName} and {localChargesName} must not be the same.");
			AssertHasWarning(job.JH_OA_AgentCollectAddrInfo, $"You must save this form to have this organization recorded as the {agentCollectName} on this job.  Currently this {agentCollectName} is NOT saved against this job.");
			AssertHasWarning(job.JH_OA_LocalChargesAddrInfo, $"You must save this form to have this organization recorded as the {localChargesName} on this job.  Currently this {localChargesName} is NOT saved against this job.");
			AssertHasWarning(job.JH_OA_AgentCollectAddrInfo, $"In most cases, the {agentCollectName} should be flagged as a Receivables organization.");
			AssertHasWarning(job.JH_OA_LocalChargesAddrInfo, $"In most cases, the {localChargesName} should be flagged as a Receivables organization.");

			org1 = TestObjectCreator.CreateOrgHeader("newOrg", false, false);
			var contact1 = TestObjectCreator.CreateContact(org1, "Jo");
			var contact2 = TestObjectCreator.CreateContact(org1, "Blo");

			job.JH_OA_LocalChargesAddr = org2.MainAddress.PK;
			job.JH_OC_LocalBillingContact = contact1.PK;

			Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed = false;

			job.JH_OA_AgentCollectAddr = org2.MainAddress.PK;
			job.JH_OA_LocalChargesAddr = org1.MainAddress.PK;
			job.JH_OC_LocalBillingContact = contact2.PK;
			job.Validation.ValidateJH_OA_AgentCollectAddr();
			job.Validation.ValidateJH_OA_LocalChargesAddr();

			AssertHasErrorContaining(job.JH_OA_AgentCollectAddrInfo, $"You cannot modify {agentCollectName} address once a revenue charge is posted.");
			AssertHasErrorContaining(job.JH_OA_LocalChargesAddrInfo, $"You cannot modify {localChargesName} address once a revenue charge is posted.");
			AssertHasErrorContaining(job.JH_OC_LocalBillingContactInfo, $"You cannot modify {localChargesName} contact once a revenue charge is posted.");
		}

		public void TestCheckJH_OA_LocalChargesAddrAndCheckJH_OA_AgentCollectAddr()
		{
			JobValidation validation = TestJob.Validation as JobValidation;
			AssertNotNull(validation);
			validation.CheckJH_OA_LocalChargesAddr_Count = 0;
			validation.CheckJH_OA_AgentCollectAddr_Count = 0;

			AssertEquals("validationStartedBy should be None", JobValidation.ValidationStartedBy.None, validation.validationStartedBy);

			validation.ValidateJH_OA_AgentCollectAddr();
			AssertEquals("CheckJH_OA_LocalChargesAddr should be called once", 1, validation.CheckJH_OA_LocalChargesAddr_Count);
			AssertEquals("CheckJH_OA_AgentCollectAddr should be called once", 1, validation.CheckJH_OA_AgentCollectAddr_Count);
			AssertEquals("validationStartedBy should be None", JobValidation.ValidationStartedBy.None, validation.validationStartedBy);

			validation.CheckJH_OA_LocalChargesAddr_Count = 0;
			validation.CheckJH_OA_AgentCollectAddr_Count = 0;
			validation.ValidateJH_OA_AgentCollectAddr();
			AssertEquals("CheckJH_OA_LocalChargesAddr should be called once", 1, validation.CheckJH_OA_LocalChargesAddr_Count);
			AssertEquals("CheckJH_OA_AgentCollectAddr should be called once", 1, validation.CheckJH_OA_AgentCollectAddr_Count);
			AssertEquals("validationStartedBy should be None", JobValidation.ValidationStartedBy.None, validation.validationStartedBy);
		}

		[ExpectNoExceptions]
		public void TestCheckJH_OA_LocalCHargesAddrAfterJobIsDeleted()
		{
			JobValidation validation = TestJob.Validation as JobValidation;
			TestJob.Delete();
			validation.ValidateJH_OA_LocalChargesAddr();
		}

		[ExpectNoExceptions]
		public void TestCheckJH_OA_AgentCollectAddrAfterJobIsDeleted()
		{
			JobValidation validation = TestJob.Validation as JobValidation;
			TestJob.Delete();
			validation.ValidateJH_OA_AgentCollectAddr();
		}

		public void TestCheckActiveOrgForOverSeasAgentAddrEditOldShipment()
		{
			OrgHeader oSAgent = Factory.NewWithValidTestData<OrgHeader>();
			oSAgent.OH_IsDebtor = true;
			oSAgent.MiscServ.OM_ARCreditLimit = 99m;
			oSAgent.OH_IsActive = true;
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("1000");
			TestJob.JH_OA_AgentCollectAddr = oSAgent.Addresses[0].PK;
			TestJob.JH_ParentID = shipment.PK;

			TestJob.Validation.ValidateJH_OA_AgentCollectAddr();
			AssertNoErrors("Org is Active no error", TestJob.JH_OA_AgentCollectAddrInfo);

			oSAgent.OH_IsActive = false;
			Factory.Save();
			TestJob.Validation.ValidateJH_OA_AgentCollectAddr();
			AssertHasWarning(TestJob.JH_OA_AgentCollectAddrInfo, "This Overseas Agent is inactive.");
		}

		public void TestCheckActiveOrgForOverSeasAgentAddrNewShipment()
		{
			var otherfactory = new BusinessObjectFactory();
			OrgHeader oSAgent = otherfactory.NewWithValidTestData<OrgHeader>();
			oSAgent.OH_IsDebtor = true;
			oSAgent.MiscServ.OM_ARCreditLimit = 99m;
			oSAgent.OH_IsActive = true;
			otherfactory.Save();

			var shipment = TestObjectCreator.CreateShipment("1000");
			TestJob.JH_OA_AgentCollectAddr = oSAgent.Addresses[0].PK;
			TestJob.JH_ParentID = shipment.PK;

			TestJob.Validation.ValidateJH_OA_AgentCollectAddr();
			AssertNoErrors("Org is Active no error", TestJob.JH_OA_AgentCollectAddrInfo);

			oSAgent.OH_IsActive = false;
			otherfactory.Save();
			TestJob.Validation.ValidateJH_OA_AgentCollectAddr();
			AssertHasError(TestJob.JH_OA_AgentCollectAddrInfo, "This Overseas Agent is inactive - it may not be used.");
		}

		public void TestCheckInActiveOrgForLocalChargesAddrEditOldShipment()
		{
			OrgHeader localCharges = Factory.NewWithValidTestData<OrgHeader>();
			localCharges.OH_IsDebtor = true;
			localCharges.MiscServ.OM_ARCreditLimit = 99m;
			localCharges.OH_IsActive = true;
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("1000");
			TestJob.JH_OA_LocalChargesAddr = localCharges.Addresses[0].PK;
			TestJob.JH_ParentID = shipment.PK;

			TestJob.Validation.ValidateJH_OA_LocalChargesAddr();
			AssertNoErrors("Org is Active no error", TestJob.JH_OA_LocalChargesAddrInfo);

			localCharges.OH_IsActive = false;
			Factory.Save();
			TestJob.Validation.ValidateJH_OA_LocalChargesAddr();
			AssertHasWarning(TestJob.JH_OA_LocalChargesAddrInfo, "This Local Client is inactive.");
		}

		public void TestCheckActiveOrgForLocalChargesAddrNewShipment()
		{
			var otherfactory = new BusinessObjectFactory();
			OrgHeader localCharges = otherfactory.NewWithValidTestData<OrgHeader>();
			localCharges.OH_IsDebtor = true;
			localCharges.MiscServ.OM_ARCreditLimit = 99m;
			localCharges.OH_IsActive = true;
			otherfactory.Save();

			var shipment = TestObjectCreator.CreateShipment("1000");
			TestJob.JH_OA_LocalChargesAddr = localCharges.Addresses[0].PK;
			TestJob.JH_ParentID = shipment.PK;

			TestJob.Validation.ValidateJH_OA_LocalChargesAddr();
			AssertNoErrors("Org is Active no error", TestJob.JH_OA_LocalChargesAddrInfo);

			localCharges.OH_IsActive = false;
			otherfactory.Save();
			TestJob.Validation.ValidateJH_OA_LocalChargesAddr();
			AssertHasError(TestJob.JH_OA_LocalChargesAddrInfo, "This Local Client is inactive - it may not be used.");
		}

		public void TestCheckJH_GB()
		{
			GlbBranch brn = Factory.NewWithValidTestData<GlbBranch>();
			brn.GB_IsActive = false;
			Factory.Save();
			TestJob.Branches.Add(brn);
			TestJob.JH_GB = brn.PK;
			Assert("Should have errors", TestJob.JH_GBInfo.HasErrors());

			GlbBranch gbrn = Factory.NewWithValidTestData<GlbBranch>();
			gbrn.GB_IsActive = true;
			Factory.Save();
			TestJob.Branches.Add(gbrn);
			TestJob.JH_GB = gbrn.PK;
			Assert("Should have no errors", !TestJob.JH_GBInfo.HasErrors());
		}

		public void TestValidateInvoiceNum()
		{
			TestJob.InvoiceNum = "";
			((JobValidation)TestJob.Validation).ValidateInvoiceNum();
			AssertHasErrors("Should have errors", TestJob.InvoiceNumInfo);
		}

		public void TestValidateInvoiceDate()
		{
			TestJob.InvoiceDate = ZDateTime.Empty;
			((JobValidation)TestJob.Validation).ValidateInvoiceDate();
			AssertHasErrors("Should have errors", TestJob.InvoiceDateInfo);
		}

		public void TestValidateInvoiceDueDate()
		{
			TestJob.InvoiceDueDate = ZDateTime.Empty;
			((JobValidation)TestJob.Validation).ValidateInvoiceDueDate();
			AssertHasErrors("Should have errors", TestJob.InvoiceDueDateInfo);
		}

		public void TestCheckJH_StatusForJobStatusUpdateRestrictionRule()
		{
			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "NZAKL");
			var job = new Job.Loader(Factory, shipment).TryCreateWithMutex(false);

			var expectedSecurityRightMessage = "You do not have the appropriate security rights to run this function";
			var fromJobStatus = JobHeaderStatus.Working.Code;

			var jobStatusUpdateRestrictionRuleCollection = AccountingConfigurationRegistry.Instance.JobStatusUpdateRestrictionRule.DefaultValue;
			var jobStatusUpdateRestrictionRule = jobStatusUpdateRestrictionRuleCollection.Cast<JobStatusUpdateRestrictionRule>().FirstOrDefault(x => x.JobStatus == fromJobStatus);
			jobStatusUpdateRestrictionRule.WorkOnHold = "YES";
			jobStatusUpdateRestrictionRule.JobReadyForRevenuePosting = "NO";

			using (AccountingConfigurationRegistry.Instance.JobStatusUpdateRestrictionRule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, jobStatusUpdateRestrictionRuleCollection))
			{
				job.JH_Status = fromJobStatus;
				job.JH_Status = JobHeaderStatus.WorkOnHold.Code;
				AssertEquals("Job should not be in database", false, job.IsInDatabase);
				AssertNoErrors("Should be no errors when job is not in database", job.JH_StatusInfo);

				Factory.Save();
				AssertEquals("Job should be in database", true, job.IsInDatabase);

				job.JH_Status = "";
				AssertNoErrorContaining("Should be no errors when to job status is empty", job.JH_StatusInfo, expectedSecurityRightMessage);

				job.JH_Status = JobHeaderStatus.Closed.Code;
				Factory.Save();
				job.JH_Status = JobHeaderStatus.JobReadyForRevenuePosting.Code;
				AssertNoErrors("Should be no errors when the configuration of 'CLS' cannot be found in the registry 'Job Status Update Restriction Rule'", job.JH_StatusInfo);

				Env.Security.ChangeStatusOfWorkingJobs.IsAllowed = false;
				job.JH_Status = fromJobStatus;
				Factory.Save();
				job.JH_Status = JobHeaderStatus.WorkOnHold.Code;
				AssertHasErrorContaining(job.JH_StatusInfo, expectedSecurityRightMessage);
				AssertHasErrors(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Management -> Change Status of Working Jobs

You must revert the value of this field to its original value of 'WRK - Working'", job.JH_StatusInfo);

				Env.Security.ChangeStatusOfWorkingJobs.IsAllowed = true;
				job.JH_Status = fromJobStatus;
				job.JH_Status = JobHeaderStatus.WorkOnHold.Code;
				AssertNoErrors("User has security from WRK to WHL", job.JH_StatusInfo);

				Env.Security.ChangeStatusOfWorkingJobs.IsAllowed = false;
				job.JH_Status = fromJobStatus;
				job.JH_Status = JobHeaderStatus.JobReadyForRevenuePosting.Code;
				AssertNoErrors("No security is required to change from status WKR to JRB", job.JH_StatusInfo);

				Env.Security.ChangeStatusOfWorkingJobs.IsAllowed = true;
				job.JH_Status = fromJobStatus;
				job.JH_Status = JobHeaderStatus.JobReadyForRevenuePosting.Code;
				AssertNoErrors("No security is required to change from status WKR to JRB", job.JH_StatusInfo);
			}
		}

		public void TestCheckJH_StatusForCloseJob()
		{
			bool originalCloseJobCheckpoint = Env.Security.CloseSingleJob.IsAllowed;
			bool originalCloseMultipleJobCheckpoint = Env.Security.CloseMultipleJobs.IsAllowed;

			try
			{
				TestJob.JH_Status = "";
				AssertHasError("Empty Job Status should cause error.", TestJob.JH_StatusInfo, "Please enter a " + TestJob.JH_StatusInfo.Description + ".");

				TestJob.JH_Status = "BAD";
				AssertHasError("Invalid Job Status should cause error.", TestJob.JH_StatusInfo, "Enter a valid " + TestJob.JH_StatusInfo.Description + ".");

				Env.Security.CloseSingleJob.IsAllowed = false;
				Env.Security.CloseMultipleJobs.IsAllowed = false;

				TestJob.JH_Status = JobHeaderStatus.Closed.Code;
				AssertHasError("Creating a closed job if no security access should cause error.", TestJob.JH_StatusInfo, JobValidation.ClosingJobSecurityErrorMessage);

				TestJob.JH_Status = JobHeaderStatus.Working.Code;
				AssertNoErrors("Valid job status should clear errors", TestJob.JH_StatusInfo);

				Factory.Save();

				TestJob.JH_Status = JobHeaderStatus.Closed.Code;
				AssertHasError("Closing a job if no security access should cause error.", TestJob.JH_StatusInfo, JobValidation.ClosingJobSecurityErrorMessage);

				Env.Security.CloseSingleJob.IsAllowed = true;
				Env.Security.CloseMultipleJobs.IsAllowed = true;

				TestJob.Validation.ValidateJH_Status();
				AssertNoErrors("Closing a job should be allowed if we have security access.", TestJob.JH_StatusInfo);

				Env.Security.CloseSingleJob.IsAllowed = false;
				Env.Security.CloseMultipleJobs.IsAllowed = true;

				TestJob.Validation.ValidateJH_Status();
				AssertNoErrors("Closing a job should be allowed if we can close multiple jobs but not single jobs", TestJob.JH_StatusInfo);

				Factory.Save();

				Env.Security.CloseSingleJob.IsAllowed = false;
				Env.Security.CloseMultipleJobs.IsAllowed = false;

				TestJob.Validation.ValidateJH_Status();
				AssertNoErrors("Viewing a closed job should not cause errors regardless of security access level.", TestJob.JH_StatusInfo);

				TestJob.JH_Status = JobHeaderStatus.Working.Code;
				AssertNoErrors("Reopening a closed job should not cause errors regardless of security access level.", TestJob.JH_StatusInfo);
			}
			finally
			{
				Env.Security.CloseSingleJob.IsAllowed = originalCloseJobCheckpoint;
				Env.Security.CloseMultipleJobs.IsAllowed = originalCloseMultipleJobCheckpoint;
			}
		}

		public void TestCheckJH_StatusForCompleteJob()
		{
			bool isAllowed = Env.Security.ChangeStatusOfCompleteJobs.IsAllowed;
			try
			{
				TestJob.JH_Status = JobHeaderStatus.Complete.Code;
				Factory.Save();

				Env.Security.ChangeStatusOfCompleteJobs.IsAllowed = false;

				TestJob.JH_Status = JobHeaderStatus.Working.Code;
				TestJob.Validation.ValidateJH_Status();
				AssertHasError("Changing Status of Complete Job if no security access should cause error.", TestJob.JH_StatusInfo, Env.Security.ChangeStatusOfCompleteJobs.ErrorMessageForNotAllowed + JobValidation.CompleteJobSecurityErrorMessage);

				TestJob.JH_Status = JobHeaderStatus.Complete.Code;
				TestJob.Validation.ValidateJH_Status();
				AssertNoErrors("Valid job status should clear errors", TestJob.JH_StatusInfo);

				Env.Security.ChangeStatusOfCompleteJobs.IsAllowed = true;

				TestJob.JH_Status = JobHeaderStatus.Working.Code;
				AssertNoErrors("Changing Status of Complete Job if we have security access.", TestJob.JH_StatusInfo);
			}
			finally
			{
				Env.Security.ChangeStatusOfCompleteJobs.IsAllowed = isAllowed;
			}
		}

		public void TestCheckJH_StatusForOldStyleGatewayJob()
		{
			var consol = Creator.CreateConsol("AUSYD", "NZAKL", "C0001");
			TestJob.JH_ParentID = consol.PK;
			TestJob.JH_ParentTableCode = consol.TablePrefix;

			TestJob.JH_Status = JobHeaderStatus.Working.Code;
			AssertNull("PlugInData", TestJob.PlugInData);
			TestJob.Validation.ValidateJH_Status();
			AssertNoErrors("PlugIn is requred to check ConsumerType", TestJob.JH_StatusInfo);

			Factory.Save();

			AssertNotNull("PlugInData", TestJob.PlugInData);
			Assert("Is not a Gateway Consol", !consol.IsGatewayConsol);
			//WI00111976
			//AssertEquals("Old style Gateway Consol ConsumerType", JobInvoicingConsumerTypes.GatewayConsol, TestJob.PlugInData.InvoicingSupporter.ConsumerType);

			//var expectedError = @"C0001 is a legacy Gateway Billing Job which does not satisfy current requirements for Gateway Billing of a Gateway Agent Consol.  Please change the consol to be a valid Gateway Agent Consol or close this job.";

			//TestJob.Validation.ValidateJH_Status();
			//AssertHasError("", TestJob.JH_StatusInfo, expectedError);

			TestJob.JH_Status = JobHeaderStatus.Closed.Code;
			TestJob.Validation.ValidateJH_Status();
			AssertNoErrors("Job is closed", TestJob.JH_StatusInfo);

			consol.JK_AgentType = Constants.AgentType.Agent;
			var forwarder = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.GB_OH_OrgProxy);
			var orgAppointedAgentPorts = forwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPorts.O5_PortOrCountry = "AUSYD";
			orgAppointedAgentPorts.O5_OA_AgentOfficeAddress = GlbBranch.CurrentBranch.OrgProxy.Addresses.DefaultAddressOfType(OrgAddressType.Office).PK;
			consol.JK_OA_SendingForwarderAddress = orgAppointedAgentPorts.O5_OA_AgentOfficeAddress;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			Factory.Save();

			Assert("Is a Gateway Consol", consol.IsGateway());
			TestJob.JH_Status = JobHeaderStatus.Working.Code;
			TestJob.Validation.ValidateJH_Status();
			AssertNoErrors("As Job is linked to valid Gateway Consol", TestJob.JH_StatusInfo);
		}

		public void TestCheckJH_StatusForPresenceOfApportionedChargesInConsol()
		{
			var consol = Creator.CreateConsol();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 351.73m;
			var job1 = Creator.CreateJob(shipment1);

			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualWeight = 786.23m;
			shipment2.JS_UniqueConsignRef = "1234";
			var job2 = Creator.CreateJob(shipment2);

			Factory.Save();

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				cost.E6_OSCostAmount = 200m;
				cost.E6_AC_ChargeCode = Creator.CC1.PK;

				Factory.Save();

				var job2OriginalPk = job2.Charges[0].JR_E6;
				job2.Charges[0].JR_E6 = ZGuid.Empty;

				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				var shipment2InDifferentFactory = factory2.Load<ForwardingShipment>(shipment2.PK);
				var job2InDiffFactory = shipment2InDifferentFactory.GetJob(GlbCompany.CurrentCompany);
				job2InDiffFactory.JH_Status = JobHeaderStatus.Closed.Code;
				Factory.Save();

				AssertNoErrors("Changing Job status to CLS when an Apportionment charge IS NOT LINKED throws no error", job2InDiffFactory.JH_StatusInfo);

				job2InDiffFactory.JH_Status = JobHeaderStatus.Working.Code;
				Factory.Save();
				job2InDiffFactory.JH_Status = JobHeaderStatus.Closed.Code;
				Factory.Save();

				job2.Charges[0].JR_E6 = job2OriginalPk;

				Factory.Save();

				string expectedErrMsg = @"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Apportion Split Charge should not have 0 Cost Amount.";

				string expectedStackTraceMsg = "A stack trace should appear here";
				var collectorService = CriticalValidationInfoCollectorService.GetOrCreateService(factory2);
				collectorService.AddInfoWhenAllowed(job2.Charges[0].PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeLocalAmountSetToZero, () => expectedStackTraceMsg, CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
				try
				{
					factory2.Save();
					Fail("Critical Validation should prevent saving");
				}
				catch (OnSavingCriticalCheckException ex)
				{
					AssertContains("Critical Validation Error", expectedErrMsg, ex.Message);
					AssertContains("Critical Validation Error", expectedStackTraceMsg, ex.DeveloperErrorMessage);
					ExceptionReporterTestListener.Instance.Clear();
				}
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestCheckJH_StatusDoesDbCheckForUnpostedApportionedChargesOnlyOnValidateAll()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S00010002", "AUSYD", "NZAKL", consol);

			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC10);
			cost.E6_OSCostAmount = 100M;
			cost.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
			Factory.Save();

			var isShipment1JobGetErrorOnStatusSetter = false;
			var shipment1Job = new Job.Loader(Factory, shipment1).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertNotEquals("PreCondition", JobHeaderStatus.Closed.Code, shipment1Job.JH_Status);
			shipment1Job.OnCloseJobError += (s, e) => isShipment1JobGetErrorOnStatusSetter = true;

			var charge = shipment1Job.Charges.Where(x => x.JR_AC == TestObjectCreator.CC10.PK).Single();
			charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			Factory.Save();

			cost.ApportionmentCharges.FindChargeForJob(shipment1).JR_IsUsedForApportionment = false;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var consolInNewFactory = newFactory.Load<ForwardingConsol>(consol.PK);
			var shipment1InNewFactory = newFactory.Load<ForwardingShipment>(shipment1.PK);
			var shipment1JobInNewFactory = new Job.Loader(newFactory, shipment1InNewFactory).TryLoadOrCreateWithoutMutexForTestOnly();
			var shipment2InNewFactory = newFactory.Load<ForwardingShipment>(shipment2.PK);

			var costInNewFactory = consolInNewFactory.GetApportionments().CostsCollection.FindByPK(cost.PK) as JobConsolCost;
			costInNewFactory.E6_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			costInNewFactory.E6_ExchangeRate = 1;

			var chargeForShipemnt1 = costInNewFactory.ApportionmentCharges.FindChargeForJob(shipment1InNewFactory);
			chargeForShipemnt1.JR_IsUsedForApportionment = true;
			var chargeForShipemnt2 = costInNewFactory.ApportionmentCharges.FindChargeForJob(shipment2InNewFactory);
			chargeForShipemnt2.JR_IsUsedForApportionment = true;

			costInNewFactory.E6_OSCostAmount = 10m;

			newFactory.Save();

			shipment1Job.JH_Status = JobHeaderStatus.Closed.Code;
			AssertEquals("PreCondition", false, isShipment1JobGetErrorOnStatusSetter);
			AssertNoErrors("PreCondition", shipment1Job.JH_StatusInfo);
			shipment1Job.Validation.ValidateAll();
			AssertHasError("we run the db-query only when calling ValidateAll, it is only for GUI saving."
				, shipment1Job.JH_StatusInfo
				, @"The job S00010001 contains apportioned charges and cannot be closed.
Please open consol and remove or post the apportionment(s) first and try again after closing and reopening the current form.");
		}

		public void TestCheckJH_GB_GatewayJobHeaderShouldMatchBranchToGatewayAgentOrgProxy()
		{
			var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GES"));
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, gatewayDepartment.PK.ToGuid()))
			{
				var consol = Creator.CreateConsol("AUSYD", "NZAKL", "C0001");
				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				var forwarder = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.GB_OH_OrgProxy);
				var orgAppointedAgentPorts = forwarder.AppointedGatewayAgentPorts.AddNew();
				orgAppointedAgentPorts.O5_PortOrCountry = "AUSYD";
				orgAppointedAgentPorts.O5_OA_AgentOfficeAddress = GlbBranch.CurrentBranch.OrgProxy.Addresses.DefaultAddressOfType(OrgAddressType.Office).PK;
				orgAppointedAgentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
				orgAppointedAgentPorts.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
				consol.JK_OA_SendingForwarderAddress = orgAppointedAgentPorts.O5_OA_AgentOfficeAddress;

				TestJob.JH_ParentID = consol.PK;
				TestJob.JH_ParentTableCode = consol.TablePrefix;
				TestJob.JH_Status = JobHeaderStatus.Working.Code;
				TestJob.Branch.GB_OH_OrgProxy = Creator.AALSHI.MainAddress.PK;

				var gateway = (IGateway)consol;

				AssertEquals(false, TestJob.Branch.GB_OH_OrgProxy == gateway.GatewayBillingSupporter.GatewayAgent().sendingAgent.PK);
				var errorMsg = @"Gateway Job Header must match the branch code relating to the Gateway Agent's Org. Proxy.";

				TestJob.Validation.ValidateJH_GB();
				AssertHasError(TestJob.JH_GBInfo, errorMsg);

				TestJob.Branch.GB_OH_OrgProxy = gateway.GatewayBillingSupporter.GatewayAgent().sendingAgent.PK;
				TestJob.Validation.ValidateJH_GB();
				AssertNoError(TestJob.JH_GBInfo, errorMsg);
			}
		}

		public void TestAllowReopenJobWhenSecurityAllowed()
		{
			bool originalDisallowReopenJobFunction = Env.Security.ReopenJob.IsAllowed;
			Env.Security.ReopenJob.IsAllowed = true;
			try
			{
				TestJob.JH_Status = JobHeaderStatus.Closed.Code;

				TestJob.Factory.Save();

				TestJob.JH_Status = JobHeaderStatus.Working.Code;

				Assert("Should be no error on job because security is allowed", !TestJob.JH_StatusInfo.HasErrors());
			}
			finally
			{
				Env.Security.ReopenJob.IsAllowed = originalDisallowReopenJobFunction;
			}
		}

		public void TestCreditCheckingForLocalCharges()
		{
			OrgHeader localCharges = Factory.NewWithValidTestData<OrgHeader>();
			localCharges.OH_IsDebtor = true;
			localCharges.MiscServ.OM_ARCreditLimit = 99m;

			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));
			AccChargeCode chargeCode = TestObjectCreator.CC1;
			Factory.Save();

			CreateARInvoiceWithLine(job, chargeCode, localCharges, 90m);

			TestJob.LocalChargesPK = localCharges.PK;
			Factory.Save();

			TestJob.Validation.ValidateJH_OA_LocalChargesAddr();
			Assert("Local Charges should not have any warnings", !TestJob.JH_OA_LocalChargesAddrInfo.HasWarnings());

			CreateARInvoiceWithLine(job, chargeCode, localCharges, 20m);

			TestJob.LocalCharges.CreditChecker.ValidateIsCreditLimitExceeded(TestJob.JH_OA_LocalChargesAddrInfo, LedgerTypes.AccountsReceivable);
			Assert("Local Charges should not have any warnings because amount has been cached", !TestJob.JH_OA_LocalChargesAddrInfo.HasWarnings());

			// create new job in a different factory - amount is not cached
			Job testJob2 = new BusinessObjectFactory().NewJobWithValidTestDataForTesting<Job>();
			testJob2.LocalChargesPK = localCharges.PK;
			AssertHasWarningContaining(testJob2.JH_OA_LocalChargesAddrInfo, "The Credit Limit for " + localCharges.OH_Code.Trim() + " is set to ");
		}

		void CreateARInvoiceWithLine(Job job, AccChargeCode chargeCode, OrgHeader localCharges, decimal amount)
		{
			var aRInv = new BusinessObjectFactory().NewWithValidTestData<ARInvoice>();
			aRInv.AH_GB = GlbBranch.CurrentBranch.PK;
			aRInv.AH_OH = localCharges.PK;
			aRInv.AH_InvoiceAmount = amount;
			aRInv.AH_OutstandingAmount = amount;

			var line = (ARInvoiceLine)aRInv.Lines.AddNew();
			line.AL_JH = job.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_AC = chargeCode.PK;
			line.AL_AT = ZGuid.Empty;
			line.AL_LocalExTaxAmount = amount;

			var jobCharge = aRInv.Factory.Load<Job>(job.PK).Charges.AddNew();
			jobCharge.JR_AL_ARLine = line.PK;
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_OSSellAmt = amount;

			aRInv.Factory.Save();
		}

		public void TestCreditCheckingForLocalChargesWithOrgNotDebtor()
		{
			var newFactory = new BusinessObjectFactory();
			var newTestObjectCreator = new TestObjectCreator(newFactory);

			OrgHeader localCharges = newFactory.NewWithValidTestData<OrgHeader>();
			localCharges.OH_IsDebtor = true;
			localCharges.MiscServ.OM_ARCreditLimit = 99m;
			OrgHeader localCharges2 = newFactory.NewWithValidTestData<OrgHeader>();
			localCharges2.OH_IsDebtor = false;
			localCharges2.MiscServ.OM_ARCreditLimit = 99m;
			newFactory.Save();

			Job job = newTestObjectCreator.CreateJob(newTestObjectCreator.CreateShipment("S00001001"));

			ARInvoice aRInv = newFactory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_GB = GlbBranch.CurrentBranch.PK;
			aRInv.AH_OH = localCharges.PK;
			aRInv.AH_InvoiceAmount = 120m;
			aRInv.AH_OutstandingAmount = 120m;

			ARInvoiceLine line = (ARInvoiceLine)aRInv.Lines.AddNew();
			AccChargeCode chargeCode = newTestObjectCreator.CC1;
			line.AL_JH = job.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_AC = chargeCode.PK;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			line.AL_ExchangeRate = 1m;
			line.AL_AT = ZGuid.Empty;
			line.AL_LocalExTaxAmount = 120m;

			JobCharge jobCharge = job.Charges.AddNew();
			jobCharge.JR_AL_ARLine = line.PK;
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_OSSellAmt = 120m;

			newFactory.Save();

			TestJob.LocalChargesPK = ZGuid.Empty;
			TestJob.LocalChargesPK = localCharges.PK;
			Assert("Local Charges should have warnings", TestJob.JH_OA_LocalChargesAddrInfo.HasWarnings());
			AssertHasWarningContaining(TestJob.JH_OA_LocalChargesAddrInfo, "over the credit limit");
			AssertNoWarningContaining(TestJob.JH_OA_LocalChargesAddrInfo, "In most cases, the Local Client should be flagged as a Receivables organization.");

			TestJob.LocalChargesPK = ZGuid.Empty;
			TestJob.LocalChargesPK = localCharges2.PK;
			Assert("Local Charges should have warning", TestJob.JH_OA_LocalChargesAddrInfo.HasWarnings());
			AssertNoWarningContaining(TestJob.JH_OA_LocalChargesAddrInfo, "over the credit limit");
			AssertHasWarningContaining(TestJob.JH_OA_LocalChargesAddrInfo, "In most cases, the Local Client should be flagged as a Receivables organization.");
		}

		public void TestCreditCheckingForAgentCollect()
		{
			var newFactory = new BusinessObjectFactory();
			var newTestObjectCreator = new TestObjectCreator(newFactory);

			OrgHeader agentCollect = newFactory.NewWithValidTestData<OrgHeader>();
			agentCollect.OH_Code = "AC1";
			agentCollect.OH_IsDebtor = true;
			agentCollect.CompanyData.OB_ARCreditLimit = 50m;
			newFactory.Save();

			Job job = newTestObjectCreator.CreateJob(newTestObjectCreator.CreateShipment("S00001001"));

			ARInvoice aRInv = newFactory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_GB = GlbBranch.CurrentBranch.PK;
			aRInv.AH_OH = agentCollect.PK;
			aRInv.AH_InvoiceAmount = 51m;
			aRInv.AH_OutstandingAmount = 51m;

			ARInvoiceLine line = (ARInvoiceLine)aRInv.Lines.AddNew();
			AccChargeCode chargeCode = newTestObjectCreator.CC1;
			line.AL_JH = job.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_AC = chargeCode.PK;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			line.AL_ExchangeRate = 1m;
			line.AL_AT = ZGuid.Empty;
			line.AL_LocalExTaxAmount = 51m;

			JobCharge jobCharge = job.Charges.AddNew();
			jobCharge.JR_AL_ARLine = line.PK;
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_OSSellAmt = 51m;
			jobCharge.JR_LocalSellAmt = 51m;

			newFactory.Save();

			TestJob.AgentCollectPK = agentCollect.PK;
			AssertHasWarningContaining(TestJob.JH_OA_AgentCollectAddrInfo, "The Credit Limit for " + agentCollect.OH_Code.Trim() + " is set to ");
		}

		public void TestCreditCheckingForAgentCollectWithOrgNotDebtor()
		{
			var newFactory = new BusinessObjectFactory();
			var newTestObjectCreator = new TestObjectCreator(newFactory);

			OrgHeader agentCollect = newFactory.NewWithValidTestData<OrgHeader>();
			agentCollect.OH_Code = "AC1";
			agentCollect.OH_IsDebtor = true;
			agentCollect.CompanyData.OB_ARCreditLimit = 50m;
			OrgHeader agentCollect2 = newFactory.NewWithValidTestData<OrgHeader>();
			agentCollect2.OH_Code = "AC2";
			agentCollect2.OH_IsDebtor = false;
			agentCollect2.CompanyData.OB_ARCreditLimit = 33m;
			newFactory.Save();

			Job job = newTestObjectCreator.CreateJob(newTestObjectCreator.CreateShipment("S00001001"));

			ARInvoice aRInv = newFactory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_GB = GlbBranch.CurrentBranch.PK;
			aRInv.AH_OH = agentCollect.PK;
			aRInv.AH_InvoiceAmount = 100m;
			aRInv.AH_OutstandingAmount = 100m;

			ARInvoiceLine line = (ARInvoiceLine)aRInv.Lines.AddNew();
			AccChargeCode chargeCode = newTestObjectCreator.CC1;
			line.AL_JH = job.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_AC = chargeCode.PK;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			line.AL_ExchangeRate = 1m;
			line.AL_AT = ZGuid.Empty;
			line.AL_LocalExTaxAmount = 100m;

			JobCharge jobCharge = job.Charges.AddNew();
			jobCharge.JR_AL_ARLine = line.PK;
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_OSSellAmt = 100m;

			newFactory.Save();

			TestJob.AgentCollectPK = ZGuid.Empty;
			TestJob.AgentCollectPK = agentCollect.PK;
			Assert("Local Charges should have warnings", TestJob.JH_OA_AgentCollectAddrInfo.HasWarnings());
			AssertHasWarningContaining(TestJob.JH_OA_AgentCollectAddrInfo, "over the credit limit");
			AssertNoWarningContaining(TestJob.JH_OA_AgentCollectAddrInfo, "In most cases, the Overseas Agent should be flagged as a Receivables organization.");

			TestJob.AgentCollectPK = ZGuid.Empty;
			TestJob.AgentCollectPK = agentCollect2.PK;
			Assert("Local Charges should have warning", TestJob.JH_OA_AgentCollectAddrInfo.HasWarnings());
			AssertNoWarningContaining(TestJob.JH_OA_AgentCollectAddrInfo, "over the credit limit");
			AssertHasWarningContaining(TestJob.JH_OA_AgentCollectAddrInfo, "In most cases, the Overseas Agent should be flagged as a Receivables organization.");
		}

		public void TestBranchValidation()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			TestJob.JH_GB = ZGuid.NewZGuid();
			TestJob.Validation.ValidateJH_GB();
			AssertHasErrors("Should have errors", TestJob.JH_GBInfo);

			TestJob.JH_GB = GlbBranch.CurrentBranch.PK;
			TestJob.Validation.ValidateJH_GB();
			AssertNoErrors(TestJob.JH_GBInfo);

			TestJob.JH_GB = TestJob.JH_GB = creator.NonCurrentCompanyBranch.PK;
			TestJob.Validation.ValidateJH_GB();
			AssertHasErrors(TestJob.JH_GBInfo);

			TestJob.JH_GB = GlbBranch.CurrentBranch.PK;
			TestJob.Validation.ValidateJH_GB();
			AssertNoErrors(TestJob.JH_GBInfo);
		}

		public void TestDepartmentValidation()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			GlbDepartment inactiveDepartment = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FEA");
			TestJob.JH_GE = inactiveDepartment.PK;
			AssertNoErrors("Should not have errors", TestJob.JH_GEInfo);
			inactiveDepartment.GE_IsActive = false;
			TestJob.Validation.ValidateJH_GE();
			AssertHasErrors(TestJob.JH_GEInfo);

			TestJob.Factory.Save();

			inactiveDepartment.GE_IsActive = false;
			GlbDepartment secondInactiveDepartment = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FIA");
			secondInactiveDepartment.GE_IsActive = false;

			TestJob.Validation.ValidateJH_GE();
			AssertNoErrors("Should not have errors", TestJob.JH_GEInfo);

			TestJob.JH_GE = secondInactiveDepartment.PK;
			TestJob.Validation.ValidateJH_GE();
			AssertHasErrors("Should have errors", TestJob.JH_GEInfo);
		}

		public void TestDepartmentValidation_MiscellaneousDepartment()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_GE = MiscDepartment.PK;
			job.Validation.ValidateJH_GE();
			Assert("Department should have error", job.JH_GEInfo.HasErrors());
			Assert("Department Error", job.JH_GEInfo.HasError("Cannot issue job charges for a miscellaneous department."));

			job.JH_GE = NonMiscDepartment.PK;
			job.Validation.ValidateJH_GE();
			Assert("Department should not have error", !job.JH_GEInfo.HasErrors());

			job.Factory.Save();
			job.JH_GE = MiscDepartment.PK;
			job.Validation.ValidateJH_GE();
			Assert("Department should have error", job.JH_GEInfo.HasErrors());
			Assert("Department Error", job.JH_GEInfo.HasError("Cannot issue job charges for a miscellaneous department."));

			job.PlugInData = new MockJobInvoicingPlugIn(false);
			job.Validation.ValidateJH_GE();
			Assert("Department should not have error", !job.JH_GEInfo.HasErrors());

			job.PlugInData = new MockJobInvoicingPlugIn(true);
			job.Validation.ValidateJH_GE();
			Assert("Department should have error", job.JH_GEInfo.HasErrors());
			Assert("Department Error", job.JH_GEInfo.HasError("Cannot issue job charges for a miscellaneous department."));

			job.PlugInData = null;
			job.Factory.Save();
			job.Validation.ValidateJH_GE();
			Assert("Department should not have error", !job.JH_GEInfo.HasErrors());
		}

		public void TestQuotedBookingHasNoNullReferenceExceptionWhenPlugInDataIsNull()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			using (var job = new Job.Loader(quotedBooking).TryCreateWithoutMutexForTestOnly())
			{
				job.PlugInData = null;
				AssertNull(job.PlugInData);
				AssertNoExceptionThrown(() => job.Validation.ValidateJH_ProfitLossReasonCode());
			}
		}

		public void TestOneOffQuoteHasNoNullReferenceExceptionWhenPlugInDataIsNull()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var oneOffQuote = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			using (var job = new Job.Loader(oneOffQuote).TryCreateWithoutMutexForTestOnly())
			{
				job.PlugInData = null;
				AssertNull(job.PlugInData);
				AssertNoExceptionThrown(() => job.Validation.ValidateJH_ProfitLossReasonCode());
			}
		}

		public void TestTransportBookingHasNoNullReferenceExceptionWhenPlugInDataIsNull()
		{
			var consolidationBooking = Factory.New<IDtbBookingConsolidation>();
			consolidationBooking.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;
			var transportBooking = Factory.New<IDtbBooking>();
			transportBooking.KM_KB_Booking = consolidationBooking.PK;
			using (var job = new Job.Loader((IJobHeaderParent)transportBooking).TryCreateWithoutMutexForTestOnly())
			{
				job.PlugInData = null;
				AssertNull(job.PlugInData);
				AssertNoExceptionThrown(() => job.Validation.ValidateJH_ProfitLossReasonCode());
			}
		}

		public void TestQuoteMustBeApproved()
		{
			DataRegistryRating.Instance.SpotQuoteRequireInternalApproval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipment = Creator.CreateShipment("S0001");
			var unapprovedOneOffQuote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			Factory.Save();

			unapprovedOneOffQuote.CurrentOneOffQuote.TT_QuoteApprovedByManager = false;

			using (var quoteJob = new Job.Loader(unapprovedOneOffQuote).TryCreateWithoutMutexForTestOnly())
			using (var shipmentJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly())
			{
				quoteJob.JH_OA_LocalChargesAddr = Creator.AALSHI.PK;
				shipmentJob.JH_OA_LocalChargesAddr = Creator.AALSHI.PK;

				AssertEquals("Pre-condition", false, unapprovedOneOffQuote.CurrentOneOffQuote.TT_QuoteApprovedByManager);
				var expectedError = "This spot quote must be approved before being used.";
				AssertNoError(shipmentJob.JH_TH_NKQuoteNumberInfo, expectedError);

				shipmentJob.JH_TH_NKQuoteNumber = unapprovedOneOffQuote.TH_QuoteNumber;

				AssertHasError(shipmentJob.JH_TH_NKQuoteNumberInfo, expectedError);

				shipmentJob.ReadOnly = true;
				shipmentJob.Validation.ValidateJH_TH_NKQuoteNumber();

				AssertNoError("Should not have errors on jobs we can't change", shipmentJob.JH_TH_NKQuoteNumberInfo, expectedError);
			}
		}

		public void TestQuoteNumberHasErrorIfValueChanged_OneOfQuote()
		{
			AssertQuoteNumberIfValueChangedFromOneOfQuoteOrBookingWithQuote(() => {
				var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
				return QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			}, "Quote Charges of subject One Off Quote could NOT be assigned with another One Off Quote.");
		}

		public void TestQuoteNumberHasNoErrorIfValueChanged_BookingWithQuote()
		{
			AssertQuoteNumberIfValueChangedFromOneOfQuoteOrBookingWithQuote(() => {
				var booking = QuotedBooking.CreateNewBooking(Factory);
				var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
				return QuotedBooking.New(quote.PK, booking.PK, Factory);
			}, string.Empty);
		}

		public void TestQuoteNumberHasNoErrorIfValueChanged_QuickBooking()
		{
			AssertQuoteNumberIfValueChangedFromOneOfQuoteOrBookingWithQuote(() => {
				var booking = QuotedBooking.CreateNewBooking(Factory);
				var quickBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
				return quickBooking;
			}, string.Empty);
		}

		void AssertQuoteNumberIfValueChangedFromOneOfQuoteOrBookingWithQuote(Func<QuotedBooking> getQuotedBooking, string errorExpected)
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			Factory.Save();

			var quotedBooking = getQuotedBooking();

			quotedBooking.TryLoadOrCreateJob();
			using (quotedBooking.Job)
			{
				quotedBooking.Job.JH_TH_NKQuoteNumber = quote.TH_QuoteNumber;

				if (!string.IsNullOrEmpty(errorExpected))
				{
					AssertHasError(quotedBooking.Job.JH_TH_NKQuoteNumberInfo, errorExpected);
					quotedBooking.Job.JH_TH_NKQuoteNumber = string.Empty;
					AssertNoErrors(quotedBooking.Job.JH_TH_NKQuoteNumberInfo);
				}
				else
				{
					AssertNoErrors(quotedBooking.Job.JH_TH_NKQuoteNumberInfo);
				}

				if (!string.IsNullOrEmpty(errorExpected))
				{
					quotedBooking.Job.JH_TH_NKQuoteNumber = quote.TH_QuoteNumber;

					Factory.SuspendValidation();
					Factory.Save();
					Factory.ResumeValidation();

					var anotherQuote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
					Factory.Save();

					var oldQuoteNumber = quotedBooking.Job.JH_TH_NKQuoteNumber;
					AssertNotNullOrEmpty("Pre-condition:Job.JH_TH_NKQuoteNumber", oldQuoteNumber);

					quotedBooking.Job.JH_TH_NKQuoteNumber = anotherQuote.TH_QuoteNumber;
					AssertHasError(quotedBooking.Job.JH_TH_NKQuoteNumberInfo, errorExpected);

					quotedBooking.Job.JH_TH_NKQuoteNumber = oldQuoteNumber;
					AssertNoErrors(quotedBooking.Job.JH_TH_NKQuoteNumberInfo);
				}
			}
		}

		public void TestTH_OH_QuoteNumberHasErrorWhenReferencedQuoteHasOtherLocalClient()
		{
			var quote1 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking1 = QuotedBooking.New(quote1.PK, ZGuid.Empty, Factory);

			Factory.Save();

			quotedBooking1.TryLoadOrCreateJob();
			using (quotedBooking1.Job)
			{
				var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
				quotedBooking1.Job.JH_OA_LocalChargesAddr = orgHeader1.MainAddress.PK;

				var booking = QuotedBooking.CreateNewBooking(Factory);
				var quotedBooking2 = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

				quotedBooking2.TryLoadOrCreateJob();
				using (quotedBooking2.Job)
				{
					var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
					quotedBooking2.Job.JH_OA_LocalChargesAddr = orgHeader2.MainAddress.PK;
					quotedBooking2.Job.JH_TH_NKQuoteNumber = quote1.TH_QuoteNumber;
					AssertHasError("QB1: OrgHeader1, QB2: OrgHeader2 This quote is invalid as it references a different Local Client.",
						quotedBooking2.Job.JH_TH_NKQuoteNumberInfo,
						"This quote is invalid as it references a different Local Client.");

					quotedBooking1.Job.JH_OA_LocalChargesAddr = orgHeader2.MainAddress.PK;
					Factory.Save();
					quotedBooking2.Job.JH_TH_NKQuoteNumberInfo.ClearValue();
					quotedBooking2.Job.JH_TH_NKQuoteNumber = quote1.TH_QuoteNumber;
					AssertNoError("QB1: OrgHeader2, QB2: OrgHeader2 This quote is Valid as it references the same Local Client.",
						quotedBooking2.Job.JH_TH_NKQuoteNumberInfo,
						"This quote is invalid as it references a different Local Client.");

					quotedBooking2.Job.JH_OA_LocalChargesAddrInfo.ClearValue();
					Factory.Save();
					quotedBooking2.Job.JH_TH_NKQuoteNumberInfo.ClearValue();
					quotedBooking2.Job.JH_TH_NKQuoteNumber = quote1.TH_QuoteNumber;
					AssertNoError("QB1: OrgHeader1, QB2: null This quote is Valid as QB2 Local Client is blank.",
						quotedBooking2.Job.JH_TH_NKQuoteNumberInfo,
						"This quote is invalid as it references a different Local Client.");

					quotedBooking1.Job.JH_OA_LocalChargesAddrInfo.ClearValue();
					Factory.Save();
					quotedBooking2.Job.JH_TH_NKQuoteNumberInfo.ClearValue();
					quotedBooking2.Job.JH_TH_NKQuoteNumber = quote1.TH_QuoteNumber;
					AssertHasError("QB1: null, QB2: OrgHeader1 This quote is invalid as it references a different Local Client.",
						quotedBooking2.Job.JH_TH_NKQuoteNumberInfo,
						"This quote is invalid as it references a different Local Client.");
				}
			}
		}

		public void TestTH_OH_QuoteNumberHasErrorWhenReferencedQuoteHasOtherOverseasAgent()
		{
			var quote1 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var quotedBooking1 = QuotedBooking.New(quote1.PK, ZGuid.Empty, Factory);

			Factory.Save();

			quotedBooking1.TryLoadOrCreateJob();
			using (quotedBooking1.Job)
			{
				var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
				quotedBooking1.Job.JH_OA_AgentCollectAddr = orgHeader1.MainAddress.PK;

				var booking = QuotedBooking.CreateNewBooking(Factory);
				var quotedBooking2 = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

				quotedBooking2.TryLoadOrCreateJob();
				using (quotedBooking2.Job)
				using (RatingDataRegistry.Instance.EnableOverseasAgentInOneOffQuote.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
					quotedBooking2.Job.JH_OA_AgentCollectAddr = orgHeader2.MainAddress.PK;
					quotedBooking2.Job.JH_TH_NKQuoteNumber = quote1.TH_QuoteNumber;
					AssertHasError("QB1: OrgHeader1, QB2: OrgHeader2 This quote is invalid as it references a different Overseas Agent.",
						quotedBooking2.Job.JH_TH_NKQuoteNumberInfo,
						"This quote is invalid as it references a different Overseas Agent.");

					quotedBooking1.Job.JH_OA_AgentCollectAddr = orgHeader2.MainAddress.PK;
					Factory.Save();
					quotedBooking2.Job.JH_TH_NKQuoteNumberInfo.ClearValue();
					quotedBooking2.Job.JH_TH_NKQuoteNumber = quote1.TH_QuoteNumber;
					AssertNoError("QB1: OrgHeader2, QB2: OrgHeader2 This quote is Valid as it references the same Overseas Agent.",
						quotedBooking2.Job.JH_TH_NKQuoteNumberInfo,
						"This quote is invalid as it references a different Overseas Agent.");

					quotedBooking2.Job.JH_OA_AgentCollectAddrInfo.ClearValue();
					Factory.Save();
					quotedBooking2.Job.JH_TH_NKQuoteNumberInfo.ClearValue();
					quotedBooking2.Job.JH_TH_NKQuoteNumber = quote1.TH_QuoteNumber;
					AssertNoError("QB1: OrgHeader1, QB2: null This quote is Valid as QB2 Overseas agent is blank.",
						quotedBooking2.Job.JH_TH_NKQuoteNumberInfo,
						"This quote is invalid as it references a different Overseas Agent.");

					quotedBooking1.Job.JH_OA_AgentCollectAddrInfo.ClearValue();
					Factory.Save();
					quotedBooking2.Job.JH_TH_NKQuoteNumberInfo.ClearValue();
					quotedBooking2.Job.JH_TH_NKQuoteNumber = quote1.TH_QuoteNumber;
					AssertHasError("QB1: null, QB2: OrgHeader1 This quote is invalid as it references a different Overseas Agent.",
						quotedBooking2.Job.JH_TH_NKQuoteNumberInfo,
						"This quote is invalid as it references a different Overseas Agent.");
				}
			}
		}

		public void TestOneOffQuoteValidation()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			Quote quote1 = Factory.New<Quote>();
			quote1.TH_OH = creator.ABIGAS.PK;
			quote1.TH_OneTimeQuote = true;
			quote1.TH_QuoteNumber = "A";

			Quote quote2 = Factory.New<Quote>();
			quote2.TH_OH = creator.ABIGAS.PK;
			quote2.TH_OneTimeQuote = false;
			quote2.TH_QuoteNumber = "B";

			Quote quote3 = Factory.New<Quote>();
			quote3.TH_OH = creator.ABIGAS.PK;
			quote3.TH_OneTimeQuote = true;
			quote3.TH_IsOneOffQuoteConsumed = true;
			quote3.TH_QuoteNumber = "C";

			Factory.Save();

			Job existingTestJob = Factory.NewJobForTesting<Job>();
			existingTestJob.JH_GB = GlbBranch.CurrentBranch.PK;
			existingTestJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			existingTestJob.JH_JobNum = "222";
			existingTestJob.LocalChargesPK = creator.ABIGAS.PK;
			existingTestJob.JH_TH_NKQuoteNumber = quote3.TH_QuoteNumber;

			Job testJob = Factory.NewJobForTesting<Job>();
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			testJob.JH_JobNum = "11111111";
			testJob.LocalChargesPK = creator.ABIGAS.PK;
			testJob.JH_TH_NKQuoteNumber = quote1.TH_QuoteNumber;

			testJob.Validation.ValidateJH_TH_NKQuoteNumber();
			AssertNoErrors(testJob.JH_TH_NKQuoteNumberInfo);

			testJob.LocalChargesPK = creator.ABIGAS.PK;
			testJob.Validation.ValidateJH_TH_NKQuoteNumber();
			AssertNoErrors(testJob.JH_TH_NKQuoteNumberInfo);

			testJob.JH_TH_NKQuoteNumber = quote2.TH_QuoteNumber;
			AssertHasErrors(testJob.JH_TH_NKQuoteNumberInfo);

			testJob.JH_TH_NKQuoteNumber = quote1.TH_QuoteNumber;
			AssertNoErrors(testJob.JH_TH_NKQuoteNumberInfo);

			testJob.JH_TH_NKQuoteNumber = "hello";
			AssertHasErrors(testJob.JH_TH_NKQuoteNumberInfo);

			testJob.JH_TH_NKQuoteNumber = quote1.TH_QuoteNumber;
			AssertNoErrors(testJob.JH_TH_NKQuoteNumberInfo);

			testJob.JH_TH_NKQuoteNumber = quote3.TH_QuoteNumber;
			AssertHasErrors(testJob.JH_TH_NKQuoteNumberInfo);

			testJob.JH_TH_NKQuoteNumber = "";
			AssertNoErrors(testJob.JH_TH_NKQuoteNumberInfo);
		}

		public void TestDebtorValidation()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsDebtor = false;

			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_IsDebtor = true;

			Factory.Save();

			TestJob.LocalChargesPK = org.PK;
			AssertHasWarnings(TestJob.JH_OA_LocalChargesAddrInfo);
			AssertEquals("Should be 2 warnings", 2, TestJob.JH_OA_LocalChargesAddrInfo.GetWarnings().GetUniqueMessageList().Length);

			Factory.Save();
			TestJob.Validation.ValidateJH_OA_LocalChargesAddr();
			Assert("Should not contain message about not saved value", !TestJob.JH_OA_LocalChargesAddrInfo.GetWarnings().Contains("You must save this form to have this organisation recorded as the Local Client on this job.  Currently this Local Client is NOT saved against this job."));

			TestJob.LocalChargesPK = debtor.PK;
			AssertNoWarnings(TestJob.JH_OA_LocalChargesAddrInfo);

			TestJob.AgentCollectPK = org.PK;
			AssertHasWarnings(TestJob.JH_OA_AgentCollectAddrInfo);
			AssertEquals("Should be 2 warnings", 2, TestJob.JH_OA_AgentCollectAddrInfo.GetWarnings().GetUniqueMessageList().Length);

			Factory.Save();
			TestJob.Validation.ValidateJH_OA_AgentCollectAddr();
			Assert("Should not contain message about not saved value", !TestJob.JH_OA_AgentCollectAddrInfo.GetWarnings().Contains("You must save this form to have this organisation recorded as the Overseas Agent on this job.  Currently this Overseas Agent is NOT saved against this job."));

			TestJob.AgentCollectPK = debtor.PK;
			AssertNoWarnings(TestJob.JH_OA_AgentCollectAddrInfo);
		}

		public void TestLocalClientNotSavedDefaultedValueValidation()
		{
			AssertNotSavedDefaultedValueValidation(TestJob.JH_OA_LocalChargesAddrInfo, "You must save this form to have this organization recorded as the Local Client on this job.  Currently this Local Client is NOT saved against this job.", typeof(OrgAddress), OrgAddressSchema.PK);
		}

		public void TestAgentCollectNotSavedDefaultedValueValidation()
		{
			AssertNotSavedDefaultedValueValidation(TestJob.JH_OA_AgentCollectAddrInfo, "You must save this form to have this organization recorded as the Overseas Agent on this job.  Currently this Overseas Agent is NOT saved against this job.", typeof(OrgAddress), OrgAddressSchema.PK);
		}

		public void TestSalesRepNotSavedDefaultedValueValidation()
		{
			AssertNotSavedDefaultedValueValidation(TestJob.JH_GS_NKRepSalesInfo, "You must save this form to have this staff member recorded as the Sales Rep on this job. Currently this Sales Rep is NOT saved against this job.", typeof(GlbStaff), GlbStaffSchema.GS_Code);
		}

		public void TestMockJobInvoicingPlugInEditSecurity()
		{
			IJobInvoicingPlugIn testJob = new MockJobInvoicingPlugIn(true);
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);

			testJob = new MockJobInvoicingPlugIn(false);
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		void AssertNotSavedDefaultedValueValidation(ZPropertyInfo info, string warning, Type valueBOType, SchemaColumn boColumn)
		{
			BusinessObject bO1 = Factory.NewWithValidTestData(valueBOType);
			BusinessObject bO2 = Factory.NewWithValidTestData(valueBOType);

			Factory.Save();

			info.BizObj[info.Name] = bO1[boColumn];
			AssertHasWarningContaining(info, warning);

			Factory.Save();
			((Job)info.BizObj).Validation.ValidateAll();
			AssertNoWarningContaining(info, warning);

			info.BizObj[info.Name] = bO2[boColumn];
			AssertNoWarningContaining(info, warning);

			if (info is ZPropertyInfoGuid)
			{
				info.BizObj[info.Name] = ZGuid.Empty;
			}
			else if (info is ZPropertyInfoString)
			{
				info.BizObj[info.Name] = ZString.Empty;
			}
			else
			{
				throw new InvalidOperationException("This test has not been written to handle anything but ZGuid and ZString");
			}

			AssertNoWarningContaining(info, warning);
			Factory.Save();
			AssertNoWarningContaining(info, warning);

			info.BizObj[info.Name] = bO2[boColumn];
			AssertHasWarningContaining(info, warning);
		}

		public void TestGetRevenueRecognitionDateNotInGLPeriodError()
		{
			JobValidation testValidation = (JobValidation)TestJob.Validation;
			AssertEquals("Empty RevenueRecognitionDate should not cause error.", "", testValidation.GetRevenueRecognitionDateNotInGLPeriodError(ZDateTime.Empty));

			AssertEquals("Min date in RevenueRecognitionDate should not cause error.", "", testValidation.GetRevenueRecognitionDateNotInGLPeriodError(AccountingConstants.RevenueRecognitionDateConstants.Immediate));

			AssertEquals("Max date in RevenueRecognitionDate should not cause error.", "", testValidation.GetRevenueRecognitionDateNotInGLPeriodError(AccountingConstants.RevenueRecognitionDateConstants.JobClosure));

			AssertEquals("Max date in RevenueRecognitionDate should not cause error.", "", testValidation.GetRevenueRecognitionDateNotInGLPeriodError(AccountingConstants.RevenueRecognitionDateConstants.CustomsClearanceDate));

			ZDateTime expectedDate = AccountingConstants.RevenueRecognitionDateConstants.MinSpecialDate.AddDays(-10);
			AssertEquals("Date not in GL Period in RevenueRecognitionDate should cause error.", true,
				testValidation.GetRevenueRecognitionDateNotInGLPeriodError(expectedDate).Contains(expectedDate.Date.ToShortDateString()));

			expectedDate = ZDateTime.Now.AddDays(-10);
			AssertEquals("Date not in GL Period in RevenueRecognitionDate should cause error.", true,
				testValidation.GetRevenueRecognitionDateNotInGLPeriodError(expectedDate).Contains(expectedDate.Date.ToString()));

			Creator.CreateTestPeriods(ZDateTime.Now.Date);

			AssertEquals("Other date in GL Period in RevenueRecognitionDate should not cause error.", "", testValidation.GetRevenueRecognitionDateNotInGLPeriodError(ZDateTime.Now));

			AssertEquals("Max DateTime value should cause error.",
					"Please have your Accounting Department create Accounting Periods for the next year.\r\n" +
					"The Revenue Recognition Date cannot be set because an appropriate General Ledger Accounting Period has not been created.",
					testValidation.GetRevenueRecognitionDateNotInGLPeriodError(AccountingConstants.RevenueRecognitionDateConstants.DateAfterLastPeriod));
		}

		public void TestGetRevenueRecognitionDatePriorToExistingGLPeriodError()
		{
			JobValidation validator = new JobValidation(TestJob);

			AssertEquals("Pre-condition: No periods", 0, Factory.GetDatabaseCount(typeof(AccPeriodManagement), new ZQuery()));
			AssertEquals("Should get a message for Brets brithday as it's invalid", "The Revenue Recognition Date 18-Sep-71 cannot be set because an appropriate General Ledger Accounting Period has not been created to include this date. A General Ledger Accounting Period cannot be created because it is earlier than the first period currently existing. Hit Yes to continue and Immediate revenue recognition treatment will be applied. Hit No to cancel saving and change the relevant operational dates.",
				validator.GetRevenueRecognitionDatePriorToExistingGLPeriodError(ZDateTime.BrettsBirthday));

			(new TestObjectCreator(new BusinessObjectFactory())).CreateTestPeriods(ZDateTime.Today).Factory.Save();

			AssertEquals("Should get a message for Brets brithday as it's invalid", "The Revenue Recognition Date 18-Sep-71 cannot be set because an appropriate General Ledger Accounting Period has not been created to include this date. A General Ledger Accounting Period cannot be created because it is earlier than the first period currently existing. Hit Yes to continue and Immediate revenue recognition treatment will be applied. Hit No to cancel saving and change the relevant operational dates.",
				validator.GetRevenueRecognitionDatePriorToExistingGLPeriodError(ZDateTime.BrettsBirthday));
			AssertEquals("Shouldn't get a message for period end date as it's valid", "", validator.GetRevenueRecognitionDatePriorToExistingGLPeriodError(ZDateTime.Today));
			AssertEquals("Shouldn't get a message for period end date as it's future date.", "", validator.GetRevenueRecognitionDatePriorToExistingGLPeriodError(ZDateTime.Today.AddYears(3)));
		}

		public void TestCheckJH_ProfitLossReasonCode()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			string expectedValueError = @"Please assign a Job Profit / Loss Reason Code to this job.
This job's Status requires a Reason Code because its Profit/Revenue Margin('{0}') falls outside the tolerated margin threshold.";
			string emptyRegistryError = @"Job Profit/Loss Reason Codes registry item (Accounting/Job Invoicing/Job Profit Reason) is empty. Please set correct values.";

			Mock<IJobInvoicingSupporter> mockSupporter = TestObjectCreator.GetIJobInvoicingSupporterMock();
			IJobInvoicingPlugIn shipment = TestObjectCreator.GetTestShipmentPlugIn("S00001234", mockSupporter);

			TestJob.PlugInData = shipment;
			TestJob.JH_ProfitLossReasonCode = "";

			Factory.Save();

			JobValidation testJobValidation = (JobValidation)TestJob.Validation;

			JobProfitLossReasonCodeCollection plReasonCodes = new JobProfitLossReasonCodeCollection();
			JobProfitLossReasonCode plReasonCode = plReasonCodes.AddNew();
			plReasonCode.Code = "TST";
			plReasonCode.Description = (NoResString)"Test";
			AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plReasonCodes);

			JobProfitLossRequiringReasonParameters plRequiringReasonParameters = new JobProfitLossRequiringReasonParameters();
			plRequiringReasonParameters.ProfitThreshold = 10M;
			plRequiringReasonParameters.JobStatusCollection.AddNew().Code = TestJob.JobStatusList[TestJob.JobStatusList.Count - 1].Code;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(TestJob.JH_GC.ToGuid(), Guid.Empty, Guid.Empty, plRequiringReasonParameters);
			TestJob.Validation.ValidateJH_ProfitLossReasonCode();
			AssertEquals("ProfitRevenueMargin and JobStatus are not fit set registry requirements for validation.", false, testJobValidation.IsProfitLossReasonCodeInvalidForThisJobStatus(TestJob.JH_Status));
			AssertNoErrors("ProfitRevenueMargin and JobStatus are not fit set registry requirements for validation.", TestJob.JH_ProfitLossReasonCodeInfo);

			TestJob.JH_Status = TestJob.JobStatusList[TestJob.JobStatusList.Count - 1].Code;
			TestJob.Validation.ValidateJH_ProfitLossReasonCode();
			AssertEquals("ProfitRevenueMargin is not fit set registry requirements for validation.", false, testJobValidation.IsProfitLossReasonCodeInvalidForThisJobStatus(TestJob.JH_Status));
			AssertNoErrors("ProfitRevenueMargin is not fit set registry requirements for validation.", TestJob.JH_ProfitLossReasonCodeInfo);

			plRequiringReasonParameters.LossThreshold = 1M;
			plRequiringReasonParameters.ProfitThreshold = 5M;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(TestJob.JH_GC.ToGuid(), Guid.Empty, Guid.Empty, plRequiringReasonParameters);
			TestJob.Validation.ValidateJH_ProfitLossReasonCode();
			AssertEquals("ProfitRevenueMargin and JobStatus are fit set registry requirements for validation.", false, testJobValidation.IsProfitLossReasonCodeInvalidForThisJobStatus(TestJob.JH_Status));
			AssertNoErrors("ProfitRevenueMargin and JobStatus are fit set registry requirements for validation.", TestJob.JH_ProfitLossReasonCodeInfo);

			Charge testCharge1 = TestJob.Charges.AddNew();
			testCharge1.JR_AC = Creator.CC1.PK;
			testCharge1.JR_LocalCostAmt = 200;
			testCharge1.JR_LocalSellAmt = 201;
			TestJob.Validation.ValidateJH_ProfitLossReasonCode();
			AssertEquals("ProfitRevenueMargin and JobStatus are fit set registry requirements for validation.", true, testJobValidation.IsProfitLossReasonCodeInvalidForThisJobStatus(TestJob.JH_Status));
			AssertHasError("ProfitRevenueMargin and JobStatus are fit set registry requirements for validation.", TestJob.JH_ProfitLossReasonCodeInfo, string.Format(expectedValueError, "0.50%"));

			testCharge1.JR_LocalCostAmt = 200;
			testCharge1.JR_LocalSellAmt = 210;
			plRequiringReasonParameters.ProfitThreshold = 3M;
			// Set at the Branch & Department level to override Company level settings
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, TestJob.JH_GB.ToGuid(), TestJob.JH_GE.ToGuid(), plRequiringReasonParameters);
			TestJob.Validation.ValidateJH_ProfitLossReasonCode();
			AssertEquals("ProfitRevenueMargin and JobStatus are out of set registry requirements for validation.", true, testJobValidation.IsProfitLossReasonCodeInvalidForThisJobStatus(TestJob.JH_Status));
			AssertHasError("ProfitRevenueMargin and JobStatus are fit set registry requirements for validation.", TestJob.JH_ProfitLossReasonCodeInfo, string.Format(expectedValueError, "4.76%"));

			TestJob.JH_Status = "XXX";
			TestJob.Validation.ValidateJH_ProfitLossReasonCode();
			AssertEquals("JobStatus is not fit set registry requirements for validation.", false, testJobValidation.IsProfitLossReasonCodeInvalidForThisJobStatus(TestJob.JH_Status));
			AssertNoErrors("JobStatus is not fit set registry requirements for validation.", TestJob.JH_ProfitLossReasonCodeInfo);

			TestJob.JH_ProfitLossReasonCode = "V";
			TestJob.Validation.ValidateJH_ProfitLossReasonCode();
			AssertEquals("JobStatus is not fit set registry requirements for validation for validation.", false, testJobValidation.IsProfitLossReasonCodeInvalidForThisJobStatus(TestJob.JH_Status));
			AssertNoError("JobStatus is not fit set registry requirements for validation.", TestJob.JH_ProfitLossReasonCodeInfo, string.Format(expectedValueError, "4.76%"));
			AssertHasErrors("JH_ProfitLossReasonCode is not in the list.", TestJob.JH_ProfitLossReasonCodeInfo);

			TestJob.JH_ProfitLossReasonCode = "TST";
			TestJob.Validation.ValidateJH_ProfitLossReasonCode();
			AssertNoErrors("JH_ProfitLossReasonCode is in the list.", TestJob.JH_ProfitLossReasonCodeInfo);

			plReasonCodes.RemoveAll();
			AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plReasonCodes);
			TestJob.Validation.ValidateJH_ProfitLossReasonCode();
			AssertNoError("JobStatus is not fit set registry requirements for validation.", TestJob.JH_ProfitLossReasonCodeInfo, string.Format(expectedValueError, "4.76%"));
			AssertHasError("Incorrect registry settings.", TestJob.JH_ProfitLossReasonCodeInfo, emptyRegistryError);

			// JobHeader is not valid with empty JH_Status.
			TestJob.JH_Status = JobHeaderStatus.Working.Code;

			Mock<IJobInvoicingSupporter> mockSupporter2 = TestObjectCreator.GetIJobInvoicingSupporterMock(new OneOffQuoteConsumerType("QTE", (NoResString)"Spot Quotation"));
			IJobInvoicingPlugIn shipment2 = TestObjectCreator.GetTestShipmentPlugIn("S00001235", mockSupporter2);

			Job testJob2 = creator.CreateJob(null, 0, null, 0);

			testJob2.PlugInData = shipment2;
			testJob2.JH_Status = "ARC";

			JobValidation testJobValidation2 = (JobValidation)testJob2.Validation;

			//testing higher threshold
			Charge testCharge2 = testJob2.Charges.AddNew();
			testCharge2.JR_AC = Creator.CC1.PK;
			testCharge2.JR_LocalCostAmt = 100;
			testCharge2.JR_LocalSellAmt = 300;

			Factory.Save();

			plRequiringReasonParameters.LossThreshold = 1M;
			plRequiringReasonParameters.ProfitThreshold = 50M;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(testJob2.JH_GC.ToGuid(), Guid.Empty, Guid.Empty, plRequiringReasonParameters);

			testJob2.Validation.ValidateJH_ProfitLossReasonCode();

			AssertEquals("ProfitRevenueMargin and JobStatus are not fit set registry requirements for validation.", true, testJobValidation2.IsProfitLossReasonCodeInvalidForThisJobStatus(testJob2.JH_Status));
			AssertNoError("ProfitRevenueMargin and JobStatus are fit set registry requirements for validation.", testJob2.JH_ProfitLossReasonCodeInfo, string.Format(expectedValueError, "4.76%"));
		}

		public void TestCheckJH_ProfitLossReasonCodeForClosedJob()
		{
			string expectedValueError = @"Please assign a Job Profit / Loss Reason Code to this job.
This job's Status requires a Reason Code because its Profit/Revenue Margin('{0}') falls outside the tolerated margin threshold.";

			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			ARInvoice arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			Charge testCharge = TestJob.Charges.AddNew();
			testCharge.JR_AC = Creator.CC1.PK;
			testCharge.JR_LocalCostAmt = 200;
			testCharge.JR_LocalSellAmt = 300;
			InvoicingLineBase line = (InvoicingLineBase)apInvoice.Lines.AddNew();
			line.FillWithValidTestData();
			line.AL_AC = testCharge.JR_AC;
			line.AL_JH = testCharge.JR_JH;
			testCharge.JR_AL_APLine = line.PK;
			testCharge.SetAmountsToLinkedLinesForTests();

			testCharge = TestJob.Charges.AddNew();
			testCharge.JR_AC = Creator.CC1.PK;
			testCharge.JR_LocalCostAmt = 5;
			testCharge.JR_LocalSellAmt = 150;
			testCharge.SetAmountsToLinkedLinesForTests();

			testCharge = TestJob.Charges.AddNew();
			testCharge.JR_AC = Creator.CC1.PK;
			testCharge.JR_LocalCostAmt = 10;
			testCharge.JR_LocalSellAmt = 10;
			line = (InvoicingLineBase)arInvoice.Lines.AddNew();
			line.FillWithValidTestData();
			line.AL_AC = testCharge.JR_AC;
			line.AL_JH = testCharge.JR_JH;
			testCharge.JR_AL_ARLine = line.PK;
			testCharge.SetAmountsToLinkedLinesForTests();

			testCharge = TestJob.Charges.AddNew();
			testCharge.JR_AC = Creator.CC1.PK;
			testCharge.JR_LocalCostAmt = 220;
			testCharge.JR_LocalSellAmt = 10;
			line = (InvoicingLineBase)apInvoice.Lines.AddNew();
			line.FillWithValidTestData();
			line.AL_AC = testCharge.JR_AC;
			line.AL_JH = testCharge.JR_JH;
			testCharge.JR_AL_APLine = line.PK;
			line = (InvoicingLineBase)arInvoice.Lines.AddNew();
			line.FillWithValidTestData();
			line.AL_AC = testCharge.JR_AC;
			line.AL_JH = testCharge.JR_JH;
			testCharge.JR_AL_ARLine = line.PK;
			testCharge.SetAmountsToLinkedLinesForTests();

			Factory.Save();

			JobValidation testJobValidation = (JobValidation)TestJob.Validation;

			JobProfitLossRequiringReasonParameters plRequiringReasonParameters = new JobProfitLossRequiringReasonParameters();
			plRequiringReasonParameters.ProfitThreshold = 10M;
			plRequiringReasonParameters.LossThreshold = 5M;
			plRequiringReasonParameters.JobStatusCollection.AddNew().Code = JobHeaderStatus.Closed.Code;
			plRequiringReasonParameters.JobStatusCollection.AddNew().Code = JobHeaderStatus.Complete.Code;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, TestJob.JH_GB.ToGuid(), TestJob.JH_GE.ToGuid(), plRequiringReasonParameters);
			TestJob.Validation.ValidateJH_ProfitLossReasonCode();
			AssertEquals("ProfitRevenueMargin and JobStatus are not fit set registry requirements for validation.", false, testJobValidation.IsProfitLossReasonCodeInvalidForThisJobStatus(TestJob.JH_Status));
			AssertNoErrors("ProfitRevenueMargin and JobStatus are not fit set registry requirements for validation.", TestJob.JH_ProfitLossReasonCodeInfo);

			TestJob.JH_Status = JobHeaderStatus.Complete.Code;
			TestJob.Validation.ValidateJH_ProfitLossReasonCode();
			AssertEquals("ProfitRevenueMargin is not fit set registry requirements for validation.", false, testJobValidation.IsProfitLossReasonCodeInvalidForThisJobStatus(TestJob.JH_Status));
			AssertNoErrors("ProfitRevenueMargin is not fit set registry requirements for validation.", TestJob.JH_ProfitLossReasonCodeInfo);

			TestJob.JH_Status = JobHeaderStatus.Closed.Code;
			TestJob.Validation.ValidateJH_ProfitLossReasonCode();
			AssertEquals("ProfitRevenueMargin and JobStatus are fit set registry requirements for validation.", true, testJobValidation.IsProfitLossReasonCodeInvalidForThisJobStatus(TestJob.JH_Status));
			AssertHasError("ProfitRevenueMargin and JobStatus are fit set registry requirements for validation.", TestJob.JH_ProfitLossReasonCodeInfo, string.Format(expectedValueError, "-2000%"));
			Assert(TestJob.JH_ProfitRevenueMargin > TestJob.JH_ProfitRevenueMarginPosted);
			ZDecimal postedProfRevMarginBeforeSaving = TestJob.JH_ProfitRevenueMarginPosted;

			Factory.Save();

			AssertEquals(postedProfRevMarginBeforeSaving, TestJob.JH_ProfitRevenueMarginPosted);
			AssertEquals(TestJob.JH_ProfitRevenueMargin, TestJob.JH_ProfitRevenueMarginPosted);
		}

		public void TestGetRevenueRecognitionDateValidationError()
		{
			JobValidation testValidation = (JobValidation)TestJob.Validation;
			TestJob.PlugInData = Creator.CreateShipment("S0001");

			RevenueRecognitionCollection revenueRecognitionCollection = new RevenueRecognitionCollection();
			RevenueRecognition revenueRecognition = revenueRecognitionCollection.AddNew();
			revenueRecognition.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			revenueRecognition.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			revenueRecognition.Mode = Core.Constants.TransportModes.All;
			revenueRecognition.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, revenueRecognitionCollection);
			TestJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			string errorPrefix = "Test empty messsage: Empty Date for {0} recognition code.";
			string expectedError = "Test empty messsage: Empty Date for 'Actual/Estimated Arrival Date' recognition code.";

			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("", testValidation.GetRevenueRecognitionDateValidationError(errorPrefix, null));
			AssertEquals(expectedError, testValidation.GetRevenueRecognitionDateValidationError(errorPrefix, Creator.CC1));
			AssertEquals("", testValidation.GetRevenueRecognitionDateValidationError(errorPrefix, null, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));
			AssertEquals(expectedError, testValidation.GetRevenueRecognitionDateValidationError(errorPrefix, Creator.CC1, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));
			AssertEquals("Test empty messsage: Empty Date for 'Delivery Date' recognition code.", testValidation.GetRevenueRecognitionDateValidationError(errorPrefix, Creator.CC1, RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate));

			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("", testValidation.GetRevenueRecognitionDateValidationError(errorPrefix, Creator.CC1));
			AssertEquals("", testValidation.GetRevenueRecognitionDateValidationError(errorPrefix, Creator.CC1, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));

			TestJob.JH_Status = JobHeaderStatus.Complete.Code;
			AssertEquals(expectedError, testValidation.GetRevenueRecognitionDateValidationError(errorPrefix, Creator.CC1));
			AssertEquals(expectedError, testValidation.GetRevenueRecognitionDateValidationError(errorPrefix, Creator.CC1, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));

			TestJob.JH_Status = JobHeaderStatus.Closed.Code;
			AssertEquals("", testValidation.GetRevenueRecognitionDateValidationError(errorPrefix, Creator.CC1));
			AssertEquals("", testValidation.GetRevenueRecognitionDateValidationError(errorPrefix, Creator.CC1, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));
			TestJob.JH_Status = JobHeaderStatus.Working.Code;
			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			revenueRecognition.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, revenueRecognitionCollection);
			TestJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			TestJob.JH_A_JOP = ZDateTime.Now.AddDays(-10);
			AssertEquals("Date not in GL Period in RevenueRecognitionDate should cause error.", true,
				testValidation.GetRevenueRecognitionDateValidationError("Test messsage.", Creator.CC1).Contains(TestJob.JH_A_JOP.Date.ToString()));
			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Date not in GL Period in RevenueRecognitionDate should cause error.", true,
				testValidation.GetRevenueRecognitionDateValidationError("Test messsage.", Creator.CC1).Contains(TestJob.JH_A_JOP.Date.ToString()));
			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Creator.CreateTestPeriods(TestJob.JH_A_JOP.Date);
			AssertEquals("Date in GL Period in RevenueRecognitionDate should not cause error.", "",
				testValidation.GetRevenueRecognitionDateValidationError("Test messsage.", Creator.CC1));

			((CommonShipment)TestJob.PlugInData).JS_E_ARV = ZDateTime.Now;
			AssertEquals(expectedError, testValidation.GetRevenueRecognitionDateValidationError(errorPrefix, Creator.CC1, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));
		}

		[TestDate(2011, 12, 7)]
		[ExpectNoExceptions]
		public void TestGetRevenueRecognitionDateValidationErrorDoesNotThrowEnumeratorException()
		{
			RevenueRecognitionCollection revenueRecognitionCollection = new RevenueRecognitionCollection();
			RevenueRecognition revenueRecognition = revenueRecognitionCollection.AddNew();
			revenueRecognition.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			revenueRecognition.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			revenueRecognition.Mode = Core.Constants.TransportModes.All;
			revenueRecognition.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, revenueRecognitionCollection);
			TestJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			var shipment = Creator.CreateShipment("S0001");
			shipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			TestJob.JH_OA_LocalChargesAddr = Creator.AALSHI.Addresses[0].PK;
			TestJob.LocalCharges.CompanyData.OB_ARBuyersConsolInvoicingStyle = Constants.ConsolInvoicingStyles.Master;

			TestJob.JH_ParentID = shipment.PK;
			TestJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Charge testCharge = TestJob.Charges.AddNew();
			testCharge.JR_AC = Creator.CC1.PK;
			testCharge.JR_LocalCostAmt = 200m;
			testCharge.JR_LocalSellAmt = 300m;

			Job relatedJob = Creator.CreateJob(null, 0, null, 0);
			var relatedShipment = Creator.CreateShipment("S0002");
			relatedJob.JH_ParentID = relatedShipment.PK;
			relatedJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			testCharge = relatedJob.Charges.AddNew();
			testCharge.JR_AC = Creator.CC1.PK;
			testCharge.JR_LocalCostAmt = 100m;
			testCharge.JR_LocalSellAmt = 50m;

			shipment.CoLoadShipments.Add(relatedShipment);
			Factory.Save();

			AssertEquals(1, ((Integration.IJobInvoicingPlugInAdditionalJobs)shipment).AdditionalJobsToShowChargesFor.Length);
			AssertEquals("Should contain a charge from both parent jobs", 2, TestJob.Charges.Count);

			JobValidation testValidation = (JobValidation)TestJob.Validation;
			testValidation.GetRevenueRecognitionDateValidationErrors(false);

			AssertNotNull(TestJob.Parent);
			AssertEquals("Should contain two charges from both parent jobs and not throw any exceptions", 2, TestJob.Charges.Count);
		}

		public void TestIsErrorsCanBeFixedBySettingNowDate()
		{
			JobValidation testValidation = (JobValidation)TestJob.Validation;
			TestJob.PlugInData = Creator.CreateShipment("S0001");

			RevenueRecognitionCollection revenueRecognitionCollection = new RevenueRecognitionCollection();
			RevenueRecognition revenueRecognition = revenueRecognitionCollection.AddNew();
			revenueRecognition.JobType = JobInvoicingConsumerTypes.Brokerage.Code;
			revenueRecognition.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			revenueRecognition.Mode = Core.Constants.TransportModes.All;
			revenueRecognition.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, revenueRecognitionCollection);
			TestJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			AssertEquals(false, testValidation.IsErrorsCanBeFixedBySettingNowDate("Other errors" + testValidation.GetRevenueRecognitionDateValidationError("Error: {0}", CC1)));

			revenueRecognitionCollection = new RevenueRecognitionCollection();
			revenueRecognition = revenueRecognitionCollection.AddNew();
			revenueRecognition.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			revenueRecognition.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			revenueRecognition.Mode = Core.Constants.TransportModes.All;
			revenueRecognition.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, revenueRecognitionCollection);
			TestJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			AssertEquals(true, testValidation.IsErrorsCanBeFixedBySettingNowDate(testValidation.GetRevenueRecognitionDateValidationError("Error: {0}", CC1)));

			revenueRecognition.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, revenueRecognitionCollection);
			TestJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			TestJob.JH_A_JOP = ZDateTime.Now.AddDays(-10);
			AssertEquals(true, testValidation.IsErrorsCanBeFixedBySettingNowDate(testValidation.GetRevenueRecognitionDateValidationError("Error: {0}", CC1)));

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1234", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
			var line = invoice.Lines[0];
			line.AL_JH = TestJob.PK;
			line.AL_ReverseDate = ZDateTime.Empty;
			line.AL_RevRecognitionType = "";
			AssertEquals(false, testValidation.IsErrorsCanBeFixedBySettingNowDate("Other errors" + testValidation.GetRevenueRecognitionDateValidationErrors()));

			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AssertEquals(true, testValidation.IsErrorsCanBeFixedBySettingNowDate(testValidation.GetRevenueRecognitionDateValidationErrors()));

			AssertEquals(true, testValidation.IsErrorsCanBeFixedBySettingNowDate(""));
			AssertEquals(true, testValidation.IsErrorsCanBeFixedBySettingNowDate("Some error"));
			AssertEquals(false, testValidation.IsErrorsCanBeFixedBySettingNowDate("Some error" + BaseChargeValidation.EmptyRevenueRecognitionTypeErrorMessageForCost));
			AssertEquals(false, testValidation.IsErrorsCanBeFixedBySettingNowDate("Some error" + BaseChargeValidation.EmptyRevenueRecognitionTypeErrorMessageForSell));
			AssertEquals(false, testValidation.IsErrorsCanBeFixedBySettingNowDate("Some error" + JobValidation.EmptyRevenueRecognitionTypeOnJobRelatedLineErrorMessage));
			AssertEquals(false, testValidation.IsErrorsCanBeFixedBySettingNowDate(JobValidation.EmptyRevenueRecognitionTypeOnJobRelatedLineErrorMessage +
				BaseChargeValidation.EmptyRevenueRecognitionTypeErrorMessageForCost + BaseChargeValidation.EmptyRevenueRecognitionTypeErrorMessageForSell));
		}

		[TestDate(2011, 05, 04)]
		public void TestGetRevenueRecognitionDateValidationErrorsWithEmptyRecognitionRegistry()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			var revenueRecognitionConfig = AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.Value;
			revenueRecognitionConfig.RemoveAll();
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, revenueRecognitionConfig);

			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_E_ARV = ZDateTime.Now.AddYears(3);

			Job job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			JobValidation testValidation = (JobValidation)job.Validation;
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1234", TestObjectCreator.AUD, 1M);
			var line = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 100M);
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 100M, null);
			charge.JR_AL_APLine = line.PK;

			line = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 200M);
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;
			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 200M, null, TestObjectCreator.AUD, 200M, null);
			charge.JR_AL_APLine = line.PK;

			line = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 300M);
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 300M, null, TestObjectCreator.AUD, 300M, null);
			charge.JR_AL_APLine = line.PK;

			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 400M, null, TestObjectCreator.AUD, 400M, null);
			var arLine = TestObjectCreator.CreateRevenueLine(charge, Factory.New<ARInvoice>().PK);
			arLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

			job.RunPreSaveValidation();
			AssertNoErrors("Precondition", job);

			arLine.AL_RevRecognitionType = "";

			AssertEquals("Message should be shown",
@"Please have your Accounting Department create an appropriate Accounting Period.
The Revenue Recognition Date 04-May-14 cannot be set because an appropriate General Ledger Accounting Period has not been created to include this date.
You have not setup Revenue Recognition for this job type. Go to Registry -> Accounting -> Job Invoicing -> Revenue Recognition Setup to configure Revenue Recognition.
Sell revenue recognition type can't be empty. To fix it use menu item 'Recognize Revenue' form Job Invoicing menu.
The Job has related line with empty revenue recognition type. To fix it use menu item 'Recognize Revenue' form Job Invoicing menu.
The following information has not been recorded for this job. It is required for revenue recognition purposes: 'Delivery Date', 'Pickup Date'.",
					testValidation.GetRevenueRecognitionDateValidationErrors());

			AssertEquals("Message should be shown",
@"Please have your Accounting Department create an appropriate Accounting Period.
The Revenue Recognition Date 04-May-14 cannot be set because an appropriate General Ledger Accounting Period has not been created to include this date.
You have not setup Revenue Recognition for this job type. Go to Registry -> Accounting -> Job Invoicing -> Revenue Recognition Setup to configure Revenue Recognition.
Sell revenue recognition type can't be empty. To fix it use menu item 'Recognize Revenue' form Job Invoicing menu.
The Job has related line with empty revenue recognition type. To fix it use menu item 'Recognize Revenue' form Job Invoicing menu.",
					testValidation.GetRevenueRecognitionDateValidationErrors(false));
		}

		[TestDate(2011, 05, 04)]
		public void TestGetRevenueRecognitionDateValidationErrors()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_E_ARV = ZDateTime.Now.AddYears(3);

			Job job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			JobValidation testValidation = (JobValidation)job.Validation;
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1234", TestObjectCreator.AUD, 1M);
			var line = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 100M);
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 100M, null);
			charge.JR_AL_APLine = line.PK;

			line = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 200M);
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;
			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 200M, null, TestObjectCreator.AUD, 200M, null);
			charge.JR_AL_APLine = line.PK;

			line = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 300M);
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 300M, null, TestObjectCreator.AUD, 300M, null);
			charge.JR_AL_APLine = line.PK;

			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 400M, null, TestObjectCreator.AUD, 400M, null);
			var accrual = TestObjectCreator.CreateAccrual(charge);
			accrual.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

			job.RunPreSaveValidation();
			AssertNoErrors("Precondition", job);

			accrual.AL_RevRecognitionType = "";

			AssertEquals("Message should be shown",
@"Please have your Accounting Department create an appropriate Accounting Period.
The Revenue Recognition Date 04-May-14 cannot be set because an appropriate General Ledger Accounting Period has not been created to include this date.
Cost revenue recognition type can't be empty. To fix it use menu item 'Recognize Revenue' form Job Invoicing menu.
The following information has not been recorded for this job. It is required for revenue recognition purposes: 'Delivery Date', 'Pickup Date'.",
					testValidation.GetRevenueRecognitionDateValidationErrors());

			var unlinkedLine = TestObjectCreator.CreateInvoiceLine(TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M), TestObjectCreator.AUD, 1M, 10M);
			unlinkedLine.AL_AC = TestObjectCreator.CC1.PK;
			unlinkedLine.AL_JH = job.PK;
			unlinkedLine.AL_RevRecognitionType = "";

			AssertEquals("Message should be shown",
@"Please have your Accounting Department create an appropriate Accounting Period.
The Revenue Recognition Date 04-May-14 cannot be set because an appropriate General Ledger Accounting Period has not been created to include this date.
Cost revenue recognition type can't be empty. To fix it use menu item 'Recognize Revenue' form Job Invoicing menu.
The Job has related line with empty revenue recognition type. To fix it use menu item 'Recognize Revenue' form Job Invoicing menu.",
					testValidation.GetRevenueRecognitionDateValidationErrors(false));
		}

		public void TestGetRevenueRecognitionDateValidationErrorsDontValidateRecognizedCharges()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			var revenueRecognitionConfig = new RevenueRecognitionCollection();
			var setting = revenueRecognitionConfig.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, revenueRecognitionConfig);

			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();

			Job job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			JobValidation testValidation = (JobValidation)job.Validation;
			Factory.Save();

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "Desc", TestObjectCreator.AUD, 400M, null, TestObjectCreator.AUD, 400M, null);

			AssertEquals("Message should be shown",
@"The following information has not been recorded for this job. It is required for revenue recognition purposes: 'Actual/Estimated Arrival Date'.",
			testValidation.GetRevenueRecognitionDateValidationErrors());

			charge.JR_OSCostAmt = 0M;
			charge.JR_OSSellAmt = 0M;
			AssertEquals("Message should not be shown", "", testValidation.GetRevenueRecognitionDateValidationErrors());

			charge.JR_OSSellAmt = 10M;
			var arLine = TestObjectCreator.CreateRevenueLine(charge, Factory.New<ARInvoice>().PK);
			arLine.AL_RevRecognitionType = "";

			AssertEquals("Message should be shown",
@"Sell revenue recognition type can't be empty. To fix it use menu item 'Recognize Revenue' form Job Invoicing menu.
The Job has related line with empty revenue recognition type. To fix it use menu item 'Recognize Revenue' form Job Invoicing menu.",
				testValidation.GetRevenueRecognitionDateValidationErrors());

			charge.JR_OSCostAmt = 10M;
			charge.JR_OSSellAmt = 0M;
			charge.ClearRevenueLink();
			arLine.Delete();
			var apLine = TestObjectCreator.CreateCostLine(charge, Factory.New<APInvoice>().PK);
			apLine.AL_RevRecognitionType = "";

			AssertEquals("Message should be shown",
@"Cost revenue recognition type can't be empty. To fix it use menu item 'Recognize Revenue' form Job Invoicing menu.
The Job has related line with empty revenue recognition type. To fix it use menu item 'Recognize Revenue' form Job Invoicing menu.",
				testValidation.GetRevenueRecognitionDateValidationErrors());

			charge.JR_OSCostAmt = 0M;
			charge.ClearCostLink();
			apLine.Delete();

			var unlinkedLine = TestObjectCreator.CreateInvoiceLine(TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M), TestObjectCreator.AUD, 1M, 10M);
			unlinkedLine.AL_AC = TestObjectCreator.CC1.PK;
			unlinkedLine.AL_JH = job.PK;

			AssertEquals("Message should be shown", "The following information has not been recorded for this job. It is required for revenue recognition purposes: 'Actual/Estimated Arrival Date'.",
				testValidation.GetRevenueRecognitionDateValidationErrors());

			unlinkedLine.AL_RevRecognitionType = "";
			AssertEquals("Message should be shown", "The Job has related line with empty revenue recognition type. To fix it use menu item 'Recognize Revenue' form Job Invoicing menu.",
				testValidation.GetRevenueRecognitionDateValidationErrors());
		}

		public void TestRunRevenueRecognitionDateValidation()
		{
			Create2MonthPeriod();

			JobCollection jobs = new JobCollection(Factory);
			Job testJob = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			testJob.Parent = shipment;
			jobs.Add(testJob);
			CreateCharge(testJob, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			testJob.LocalChargesPK = ZGuid.Empty;
			Factory.Save();

			JobValidation validation = new JobValidation(testJob);
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting1 = valuesForTest.AddNew();
			setting1.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting1.DirectionCode = Constants.FreightShipmentDirection.Code.Domestic;
			setting1.Mode = Core.Constants.TransportModes.Air;
			setting1.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			string expectedErrorMessage = "You have not setup Revenue Recognition for this job type. Go to Registry -> Accounting -> Job Invoicing -> Revenue Recognition Setup to configure Revenue Recognition.";
			AssertEquals("There should be an error", expectedErrorMessage, validation.GetRevenueRecognitionDateValidationError("Error message", CC1));

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("There should be an error", expectedErrorMessage, validation.GetRevenueRecognitionDateValidationError("Error message", CC1));

			AssertEquals("There should be an error", expectedErrorMessage, validation.GetRevenueRecognitionDateValidationError("Error message", CC1, ""));

			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("There should be an error", expectedErrorMessage, validation.GetRevenueRecognitionDateValidationError("Error message", CC1, ""));
			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			setting1.DirectionCode = Constants.FreightShipmentDirection.Code.Other;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			AssertEquals("There should be another error", "Error message", validation.GetRevenueRecognitionDateValidationError("Error message", CC1));
			AssertEquals("There should not be an error, because registry is set up.", "", validation.GetRevenueRecognitionDateValidationError("Error message", CC1, ""));
		}

		public void TestRunRevenueRecognitionDateValidation_ConsumerTypeNoWIPsAccruals()
		{
			Create2MonthPeriod();

			JobCollection jobs = new JobCollection(Factory);
			Job testJob = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			testJob.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix; // no WIPs/Accruals
			jobs.Add(testJob);
			CreateCharge(testJob, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			testJob.LocalChargesPK = ZGuid.Empty;
			Factory.Save();

			JobValidation validation = new JobValidation(testJob);
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting1 = valuesForTest.AddNew();
			setting1.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting1.DirectionCode = Constants.FreightShipmentDirection.Code.Domestic;
			setting1.Mode = Core.Constants.TransportModes.Air;
			setting1.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			AssertEquals("There should be NO error for a consumer type that doesn't support WIPs/Accruals", string.Empty, validation.GetRevenueRecognitionDateValidationError("Error message", CC1));
		}

		public void TestCheckJH_GS_NKRepSales()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OH1";
			org1.StaffAssignments.ImportAirRep = "AAA";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OH2";
			org2.StaffAssignments.ImportAirRep = "BBB";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "OH3";
			org3.StaffAssignments.ImportAirRep = "CCC";

			org1.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			org2.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			org3.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;

			var salesRep = Factory.NewWithValidTestData<GlbStaff>();
			salesRep.GS_Code = "AAA";
			var salesRep2 = Factory.NewWithValidTestData<GlbStaff>();
			salesRep2.GS_Code = "BBB";
			var salesRep3 = Factory.NewWithValidTestData<GlbStaff>();
			salesRep3.GS_Code = "CCC";
			var salesRep4 = Factory.NewWithValidTestData<GlbStaff>();
			salesRep4.GS_Code = "DDD";
			salesRep4.GS_CanLogin = false;

			var dept = Factory.NewWithValidTestData<GlbDepartment>();
			dept.GE_Misc = false;
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S00001", "JPTYO", "AUBNE");
			var job = TestObjectCreator.CreateJob(shipment);
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			Assert("Shipment is import", shipment.IsImport());
			AssertEquals("Job has no local charges org", ZGuid.Empty, job.LocalChargesPK);
			job.PlugInData = shipment;
			job.JH_GE = dept.PK;

			var shipmentAllowOverrideSalesRepCheckPoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideSalesRep);
			shipmentAllowOverrideSalesRepCheckPoint.IsAllowed = false;

			AssertEquals("Expecting default sales rep to be blank", "", job.JH_GS_NKRepSales);

			shipmentAllowOverrideSalesRepCheckPoint.IsAllowed = false;
			job.Validation.ValidateJH_GS_NKRepSales();
			AssertNoErrors("No errors because sales rep not changed", job);
			shipmentAllowOverrideSalesRepCheckPoint.IsAllowed = true;
			job.Validation.ValidateJH_GS_NKRepSales();
			AssertNoErrors("No errors because sales rep not changed", job);

			job.JH_GS_NKRepSales = "BBB";
			shipmentAllowOverrideSalesRepCheckPoint.IsAllowed = false;
			job.Validation.ValidateJH_GS_NKRepSales();
			AssertHasError(job.JH_GS_NKRepSalesInfo, "You do not have sufficient security rights to modify this field. You must reset the value to its previous value (none)");
			shipmentAllowOverrideSalesRepCheckPoint.IsAllowed = true;
			job.Validation.ValidateJH_GS_NKRepSales();
			AssertNoErrors("No errors because security allows this", job);

			job.JH_GS_NKRepSales = "";
			shipmentAllowOverrideSalesRepCheckPoint.IsAllowed = false;
			job.Validation.ValidateJH_GS_NKRepSales();
			AssertNoErrors("No errors because sales rep same as default", job);
			shipmentAllowOverrideSalesRepCheckPoint.IsAllowed = true;
			job.Validation.ValidateJH_GS_NKRepSales();
			AssertNoErrors("No errors because sales rep same as default", job);

			job.LocalChargesPK = org1.PK;

			AssertEquals("Expecting default sales rep to be AAA", "AAA", job.JH_GS_NKRepSales);

			shipmentAllowOverrideSalesRepCheckPoint.IsAllowed = false;
			job.Validation.ValidateJH_GS_NKRepSales();
			AssertNoErrors("No errors because sales rep not changed", job);
			shipmentAllowOverrideSalesRepCheckPoint.IsAllowed = true;
			job.Validation.ValidateJH_GS_NKRepSales();
			AssertNoErrors("No errors because sales rep not changed", job);

			job.JH_GS_NKRepSales = "BBB";
			shipmentAllowOverrideSalesRepCheckPoint.IsAllowed = false;
			job.Validation.ValidateJH_GS_NKRepSales();
			AssertHasError(job.JH_GS_NKRepSalesInfo, "You do not have sufficient security rights to modify this field. You must reset the value to its previous value AAA");
			shipmentAllowOverrideSalesRepCheckPoint.IsAllowed = true;
			job.Validation.ValidateJH_GS_NKRepSales();
			AssertNoErrors("No errors because security allows this", job);

			Factory.Save();

			job.JH_GS_NKRepSales = "AAA";
			shipmentAllowOverrideSalesRepCheckPoint.IsAllowed = false;
			job.Validation.ValidateJH_GS_NKRepSales();
			AssertHasError(job.JH_GS_NKRepSalesInfo, "You do not have sufficient security rights to modify this field. You must reset the value to its previous value BBB");
			shipmentAllowOverrideSalesRepCheckPoint.IsAllowed = true;
			job.Validation.ValidateJH_GS_NKRepSales();
			AssertNoErrors("No errors because security allows this", job);

			job.LocalChargesPK = org3.PK;
			AssertEquals("Expecting default sales rep to be CCC", "CCC", job.JH_GS_NKRepSales);

			shipmentAllowOverrideSalesRepCheckPoint.IsAllowed = false;
			job.Validation.ValidateJH_GS_NKRepSales();
			AssertNoErrors("No errors because sales rep not changed", job);
			shipmentAllowOverrideSalesRepCheckPoint.IsAllowed = true;
			job.Validation.ValidateJH_GS_NKRepSales();
			AssertNoErrors("No errors because sales rep not changed", job);

			job.JH_GS_NKRepSales = "BBB";
			shipmentAllowOverrideSalesRepCheckPoint.IsAllowed = false;
			job.Validation.ValidateJH_GS_NKRepSales();
			AssertHasError(job.JH_GS_NKRepSalesInfo, "You do not have sufficient security rights to modify this field. You must reset the value to its previous value CCC");
			shipmentAllowOverrideSalesRepCheckPoint.IsAllowed = true;
			job.Validation.ValidateJH_GS_NKRepSales();
			AssertNoErrors("No errors because security allows this", job);

			job.JH_GS_NKRepSales = "DDD";
			job.Validation.ValidateJH_GS_NKRepSales();
			AssertNoErrors("No errors - can't login sales rep is OK", job);

			job.JH_GS_NKRepSales = "EEE";
			job.Validation.ValidateJH_GS_NKRepSales();
			AssertHasError(job.JH_GS_NKRepSalesInfo, "Enter a valid Sales Rep.");
		}

		public void TestLoadChargesForDeactivatedJob()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var jobInNewFactory = newFactory.Load<Job>(job.PK);

			job.MarkAsInactive();
			AssertNoExceptionThrown(() => Factory.Save());

			AssertNotNull(jobInNewFactory.Charges);
		}

		public void TestJobClientContractNumber()
		{
			var clientContractNumber = "ABCDEF";
			var contractRatingValidatorMock = new Mock<IRatingContractValidationHelper>();
			ObjectFactory.Substitute(contractRatingValidatorMock.Object);
			contractRatingValidatorMock.Setup(f => f.DoesClientContractNumberExist(Factory, It.IsAny<string>())).Returns<BusinessObjectFactory, string>((factory, ccn) => ccn == clientContractNumber);
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();

			job.JH_ClientContractNumber = clientContractNumber;
			AssertNoWarning(job.JH_ClientContractNumberInfo, JobValidation.ContractDoesNotExistWarningMessage);

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				job.JH_ClientContractNumber = "ABCDEG";
				AssertHasWarning(job.JH_ClientContractNumberInfo, JobValidation.ContractDoesNotExistWarningMessage);
			}

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				job.JH_ClientContractNumber = "ABCDEG";
				AssertNoWarning(job.JH_ClientContractNumberInfo, JobValidation.ContractDoesNotExistWarningMessage);
			}
		}

		public void TestCheckHasCurrencyExchangeRate()
		{
			var chargeCurrencies = new ElectronicProcessingChargeCurrencyCollection();
			chargeCurrencies.Add(new ElectronicProcessingChargeCurrency { CurrencyPK = TestObjectCreator.CNY.PK, ValidFromDate = ZDateTime.Today.AddDays(-10) });
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCurrency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeCurrencies);

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();

			var electronicProcessingChargeProviderMock = new Mock<IElectronicProcessingChargeProvider>();
			electronicProcessingChargeProviderMock
					.Setup(x => x.HasElectronicProcessingChargeCurrencyExchangeRate(It.IsAny<Job>()))
					.Returns(false);
			using (ObjectFactory.Substitute(electronicProcessingChargeProviderMock.Object))
			{
				job.RunPreSaveValidation();
				AssertEquals(true, job.RowErrors.GetFirstMessage().Contains("The Invoicing Job cannot be created due to missing CNY exchange rate required for the creation of the disbursement license fee transactions."));
			}

			electronicProcessingChargeProviderMock
					.Setup(x => x.HasElectronicProcessingChargeCurrencyExchangeRate(It.IsAny<Job>()))
					.Returns(true);
			using (ObjectFactory.Substitute(electronicProcessingChargeProviderMock.Object))
			{
				job.RunPreSaveValidation();
				AssertNull(job.RowErrors.GetFirstMessage());
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Creator = new TestObjectCreator(Factory);
			TestJob = Creator.CreateJob(null, 0, null, 0);
			Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
		}

		Job TestJob;
		TestObjectCreator Creator;

		protected GlbDepartment NonMiscDepartment
		{
			get { return Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)); }
		}

		protected GlbDepartment MiscDepartment
		{
			get { return Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, true)); }
		}

		#region MockJobInvoicingPlugIn

		public class MockJobInvoicingPlugIn : IJobInvoicingPlugIn
		{
			public MockJobInvoicingPlugIn(bool validateForMiscellaneousDepartment)
			{
				((MockJobInvoicingPlugInJobInvoicingSupporter)InvoicingSupporter).ValidateForMiscellaneousDepartment = validateForMiscellaneousDepartment;
			}

			#region IJobNumber

			string IJobNumber.JobNumber
			{
				get { return "12345"; }
			}

			#endregion

			#region IJobHeaderParent

			ZGuid IJobHeaderParentCore.PK
			{
				get { return ZGuid.Empty; }
			}

			string IJobHeaderParentCore.TableName
			{
				get { return ""; }
			}

			bool IJobHeaderParentCore.IsInDatabase
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			BusinessObjectFactory IJobHeaderParentCore.Factory
			{
				get { return null; }
			}

			void IJobHeaderParent.SetJobNumberFieldOnSaving()
			{
			}

			void IJobHeaderParent.OnJobCreating(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobCreated(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleting(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleted(JobHeader job)
			{
			}

			bool IJobHeaderParent.AllowInvoiceDeletion
			{
				get { return true; }
			}

			bool IJobHeaderParent.IsDeleted
			{
				get { return false; }
			}

			#endregion

			#region IJobInvoicingPlugIn Members

			MockJobInvoicingPlugInJobInvoicingSupporter fInvoicingSupporter;
			public IJobInvoicingSupporter InvoicingSupporter
			{
				get { return fInvoicingSupporter ?? (fInvoicingSupporter = new MockJobInvoicingPlugInJobInvoicingSupporter(this)); }
			}

			#endregion
		}

		class MockJobInvoicingPlugInJobInvoicingSupporter : JobInvoicingSupporter
		{
			public MockJobInvoicingPlugInJobInvoicingSupporter(IJobHeaderParent parent) : base(parent) { }

			public override JobInvoicingConsumerType ConsumerType
			{
				get { return new MockConsumerType("", "", ValidateForMiscellaneousDepartment); }
			}

			public override ZString EditSecurityMessage
			{
				get { return EditSecurityMessageCore; }
			}

			protected virtual ZString EditSecurityMessageCore
			{
				get { return ZString.Empty; }
			}

			public override bool EditSecurityLock
			{
				get { return EditSecurityLockCore; }
			}

			protected virtual ZBool EditSecurityLockCore
			{
				get { return ZBool.False; }
			}

			public bool ValidateForMiscellaneousDepartment;
		}

		#endregion

		#region MockConsumerType

		public class MockConsumerType : JobInvoicingConsumerType
		{
			public MockConsumerType(string code, string description, bool validateForMiscellaneousDepartment)
				: base(code, (NoResString)description)
			{
				this.ValidateForMiscellaneousDepartment = validateForMiscellaneousDepartment;
			}

			public override ControllerID ControllerID
			{
				get { return null; }
			}

			public override Type BizoType
			{
				get { return null; }
			}

			public override SecurityCheckpoint DistanceCalculationCheckpoint
			{
				get { return Env.Security.RoadDistanceCalculationServiceCustoms; }
			}

			public override bool ValidateJobForMiscellaneousDepartment
			{
				get { return ValidateForMiscellaneousDepartment; }
			}

			readonly bool ValidateForMiscellaneousDepartment;
		}

		#endregion

		#endregion
	}
}

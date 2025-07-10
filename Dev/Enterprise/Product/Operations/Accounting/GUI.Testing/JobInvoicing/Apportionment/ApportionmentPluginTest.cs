using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting;
using Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting.Testing.ApportionmentPlugInBaseTest;

namespace Enterprise.Accounting.GUI.Testing.JobInvoicing.Apportionment
{
	public class ApportionmentPluginTest : TestCaseWithFactory
	{
		public void TestGatewaySellApportionmentOneToManyJRJValidationError()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			PrepareGatewayConsolTestEnvironment(out ForwardingConsol consol, out OrgHeader raBne);

			var s2cnr = TestObjectCreator.CreateOrgHeader("S2CNR", false, true, "AUSYD");
			var s2cne = TestObjectCreator.CreateOrgHeader("S2CNE", false, true, "CNSHA");
			var shipment2 = TestObjectCreator.CreateShipment("S0002", "AUSYD", "CNSHA", consol, incoTerm: "FOB", housebill: "S0002");
			shipment2.ConsignorPK = s2cnr.PK;
			shipment2.ConsigneePK = s2cne.PK;

			Factory.Save();

			using (var gatewayJob = TestObjectCreator.CreateJob(consol))
			{
				var gatewayJobCharge = gatewayJob.Charges.AddNew();
				gatewayJobCharge.JR_AC = TestObjectCreator.FRT.PK;
				gatewayJobCharge.JR_OH_SellAccount = raBne.PK;
				gatewayJobCharge.JR_OSSellAmt = 200m;
				gatewayJobCharge.JR_RX_NKSellCurrency = "AUD";
				gatewayJobCharge.JR_JH_InternalJob = gatewayJob.PK;
				gatewayJobCharge.JR_GB_InternalBranch = gatewayJobCharge.JR_GB;
				gatewayJobCharge.JR_GE_InternalDept = gatewayJobCharge.JR_GE;

				//1 the goal of this test is to replicate ANY condition where one of the apportioned shipment charges could not create JRJ while the rest can, so that validation could be tested
				//2 in the above setup one of the apportionment shipment charges ends up having costAccount and sellAccount both orgProxies preventing internalJob field copied over, which in turn halts JRJ creation
				//3 if one day things change, 2 should be adjusted to satisfy 1

				using (var plugIn = new ApportionmentPlugin(consol))
				using (var control = plugIn.UserControl)
				using (var form = new ConsolForm(consol))
				{
					form.Show();
					form.FireSaveButton();
					AssertHasError(gatewayJobCharge.GatewayConsolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JobNumber == "S0002").JR_OH_SellAccountInfo,
						"You cannot set both the Cost Account and Sell Account to be the organization proxies.");
					AssertHasError(gatewayJobCharge.GatewayConsolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JobNumber == "S0002").JR_OH_CostAccountInfo,
						"You cannot set both the Cost Account and Sell Account to be the organization proxies.");

					AssertExceptionThrown<CannotSaveAfterCriticalErrorException>("JRJCreator should fail with friendly message if validation somehow bypassed",
						@"One to many job revenue journal cannot be created due to S0002 FRT cost charge internal job, branch, department fields (S0002 BNE FEA) don't match gateway billing sell charge (C001 BNE BRN). This can occur if the system tries to set Debtor and Creditor of the charge both organization proxies",
						() => Factory.Save());
				}
			}
		}

		public void TestGatewaySellApportionmentWithJFCJob()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			var expectedMsg = "Auto Job Revenue Journal cannot be created as its Invoicing or Internal Job has Ready For Financial Closure status.";
			PrepareGatewayConsolTestEnvironment(out ForwardingConsol consol, out OrgHeader proxyOrg);

			var gatewayJob = TestObjectCreator.CreateJob(consol);
			var shipmentJob = TestObjectCreator.CreateJob(consol.Shipments[0]);
			shipmentJob.JH_Status = JobHeaderStatus.Codes.JobReadyForFinancialClosure;
			Factory.Save();

			var charge = testObjectCreator.CreateCharge(gatewayJob, testObjectCreator.FRT, 100m, 100m);
			charge.JR_OH_SellAccount = proxyOrg.PK;
			charge.JR_OH_CostAccount = TestObjectCreator.Debtor1.PK;
			charge.JR_JH_InternalJob = gatewayJob.PK;
			charge.JR_GB_InternalBranch = charge.JR_GB;
			charge.JR_GE_InternalDept = charge.JR_GE;

			Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false;

			using (var plugIn = new ApportionmentPlugin(consol))
			using (var control = plugIn.UserControl)
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				form.FireSaveButton();
				AssertExceptionThrown<CannotSaveAfterCriticalErrorException>("Not allow to create JRJ Apportioned JFC Job", expectedMsg, () => Factory.Save());
			}

			Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = true;

			using (var plugIn = new ApportionmentPlugin(consol))
			using (var control = plugIn.UserControl)
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				form.FireSaveButton();
				AssertNoExceptionThrown("Allow to create JRJ when has security right", () => Factory.Save());
			}
		}

		public void TestNoExceptionWhenInitializePreviousConsolDatesWithNoTransport()
		{
			var consol = TestObjectCreator.CreateConsol(consolNum: "C001");
			var transports = consol.Transports;
			AssertEquals(1, transports.Count);

			ApportionmentPlugin plugIn = null;
			try
			{
				((IBusiness)transports[0]).DeleteForDataRefresh();
				AssertEquals(0, transports.Count);
				AssertNoExceptionThrown(() => plugIn = new ApportionmentPlugin(consol));
			}
			finally
			{
				plugIn.Dispose();
			}
		}

		public void TestNoExceptionWhenSaveApportionmentPluginWithNoTransport()
		{
			var consol = TestObjectCreator.CreateConsol(consolNum: "C001");
			var transports = consol.Transports;
			AssertEquals(1, transports.Count);

			using (var plugIn = new ApportionmentPlugin(consol))
			using (var control = plugIn.UserControl)
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				((IBusiness)transports[0]).DeleteForDataRefresh();
				AssertEquals(0, transports.Count);

				form.FireSaveButton();
				AssertNoExceptionThrown(() => Factory.Save());
			}
		}

		public void TestNoJobCreatedOnConsolSavedIfApportionmentListingInactive()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			var gatewayJob = TestObjectCreator.CreateJob(setup.gC0002);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var consol = newFactory.Load<ForwardingConsol>(setup.gC0002.PK);

			var s1 = newFactory.Load<ForwardingShipment>(setup.s0001.PK);
			var s2 = newFactory.Load<ForwardingShipment>(setup.s0002.PK);
			var s3 = newFactory.Load<ForwardingShipment>(setup.s0003.PK);

			AssertNull(new Job.Loader(s1).Load());
			AssertNull(new Job.Loader(s2).Load());
			AssertNull(new Job.Loader(s3).Load());

			using (var plugIn = new ApportionmentPlugin(consol))
			using (var control = plugIn.UserControl)
			using (var form = new ConsolForm(consol))
			{
				AssertNull(new Job.Loader(s1).Load());
				AssertNull(new Job.Loader(s2).Load());
				AssertNull(new Job.Loader(s3).Load());

				consol.JK_CustomDate1 = new ZDateTime(2019, 10, 10);
				consol.Factory.SuspendValidation();         //just to make sure consol can proceed with save

				form.Show();
				form.FireSaveButton();

				AssertEquals("Ensure save succeeded", new ZDateTime(2019, 10, 10), new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK).JK_CustomDate1);

				AssertNull(new Job.Loader(s1).Load());
				AssertNull(new Job.Loader(s2).Load());
				AssertNull(new Job.Loader(s3).Load());
			}
		}

		public void TestAppportionmentListingJobMutexes_NotReleased_OnMutexErrorDuringJobCreation()
		{
			var consol = TestObjectCreator.CreateConsol(consolNum: "C001");
			var shipment1 = TestObjectCreator.CreateShipment("S0001", consol: consol);
			var shipment2 = TestObjectCreator.CreateShipment("S0002", consol: consol);
			var shipment3 = TestObjectCreator.CreateShipment("S0003", consol: consol);
			Factory.Save();

			var testObjectCreatorInNewFactory = new TestObjectCreator(new BusinessObjectFactory());
			var jobForShipment3 = testObjectCreatorInNewFactory.CreateJob(shipment3, createWithMutex: true);
			var jobMutexForShipment3 = JobHeader.GetMutexForTest(jobForShipment3);
			AssertEquals("Precondition: job mutex for shipment3", expected: true, jobMutexForShipment3.HasLock);

			var expectedExceptionMessage = @"You have created the job S0003 on another form, but haven't saved it yet.
Please close or save other forms that use job S0003 to continue.";

			using (var plugIn = new ApportionmentPlugin(consol))
			using (var control = plugIn.UserControl)
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				AssertExceptionThrown<JobCreationException>("PrepareForConsolCosting for creation/loading of all jobs", expectedExceptionMessage, () => plugIn.Apportionments.PrepareForConsolCosting() );

				var jobMutexForShipment1 = JobHeader.GetMutexForTest(shipment1.Job);
				var jobMutexForShipment2 = JobHeader.GetMutexForTest(shipment2.Job);

				CombineAssertions(
					() =>
					{
						AssertEquals("job mutex for shipment1", expected: true, jobMutexForShipment1.HasLock);
						AssertEquals("job mutex for shipment2", expected: true, jobMutexForShipment2.HasLock);
					});
			}

			jobForShipment3.Dispose();
		}

		public void TestSkipDataRefreshBusUpdate_AnyChange_NonDeletedSubscriber()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			TestSkipDataRefreshBusUpdate_NonDeletedSubscriber();
		}

		public void TestSkipDataRefreshBusUpdate_AnyChange_DeletedSubscriber()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			TestSkipDataRefreshBusUpdate_DeletedSubscriber();
		}

		public void TestExchangeRateReadonly_UsingJobInvoicingSecurity()
		{
			var gatewayConsolJobBillingSecurity = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.GatewayConsolJobInvoicing, SecurityCore.AllowOverrideBaseExchangeRate);
			gatewayConsolJobBillingSecurity.IsAllowed = false;
			AssertOverrideExchangeRateScurityRight(CreateGateWayConsolInNewFactory(), false);

			gatewayConsolJobBillingSecurity.IsAllowed = true;
			AssertOverrideExchangeRateScurityRight(CreateGateWayConsolInNewFactory(), true);
		}

		public void TestExchangeRateReadonly_UsingMaintainConsolSecurity()
		{
			var gatewayConsolJobBillingSecurity = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.AllowOverrideBaseExchangeRate);
			gatewayConsolJobBillingSecurity.IsAllowed = false;
			AssertOverrideExchangeRateScurityRight(CreateConsolInNewFactory("C001"), false);

			gatewayConsolJobBillingSecurity.IsAllowed = true;
			AssertOverrideExchangeRateScurityRight(CreateConsolInNewFactory("C002"), true);
		}

		ForwardingConsol CreateGateWayConsolInNewFactory()
		{
			var factory = new BusinessObjectFactory();
			var testObjectCreatorInNewFactory = new TestObjectCreator(factory);
			var consol = testObjectCreatorInNewFactory.CreateGatewayConsol("KRSEL", "AUSYD", "C0002", receivingGatewayCompany: GlbCompany.CurrentCompany);

			return consol;
		}

		ForwardingConsol CreateConsolInNewFactory(string consolNum)
		{
			var factory = new BusinessObjectFactory();
			var testObjectCreatorInNewFactory = new TestObjectCreator(factory);
			var consol = testObjectCreatorInNewFactory.CreateConsol(consolNum: consolNum);

			return consol;
		}

		void AssertOverrideExchangeRateScurityRight(ForwardingConsol consol, bool isAllowOverrideExRate)
		{
			using (var plugIn = new ApportionmentPluginForTest(consol))
			using (var newApportionmentControl = (NewApportionmentUserControlForTest)plugIn.UserControl)
			using (var form = new ZForm(consol))
			{
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(newApportionmentControl, "");
				form.Controls.Add(newApportionmentControl);
				form.Show();
				AssertEquals(isAllowOverrideExRate, plugIn.Apportionments.IsAllowOverrideBaseExchangeRate);
				AssertEquals(!isAllowOverrideExRate, newApportionmentControl.CostSummaryGrid_Exposed.GetColumnStyle("CostExchangeRate+Rate").IsReadOnly);
			}
		}

		void TestSkipDataRefreshBusUpdate_DeletedSubscriber()
		{
			using (AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var chargeRevRecOverride1 = TestObjectCreator.CC1.RevenueRecOverrides.AddNew();
				chargeRevRecOverride1.AE_JobType = JobInvoicingConsumerTypes.Shipment.Code;
				chargeRevRecOverride1.AE_Direction = Core.Constants.FreightShipmentDirection.Code.All;
				chargeRevRecOverride1.AE_Mode = Core.Constants.TransportModes.All;
				chargeRevRecOverride1.AE_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
				TestObjectCreator.Creditor1.OH_IsConsignee = true;
				TestObjectCreator.Creditor2.OH_IsConsignor = true;
				Factory.Save();

				var consol = TestObjectCreator.CreateConsol(consolNum: "C002");
				var shipment = TestObjectCreator.CreateShipment("S0002", consol);
				shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
				shipment.JS_RL_NKDestination = "HKHKG";
				shipment.JS_E_ARV = ZDateTime.Empty;
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.Creditor1.PK;
				shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.Creditor2.PK;
				var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor3, 250M, true);
				shipment.Job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
				Factory.Save();

				var jobCharge = Factory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, shipment.Job.PK));
				jobCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				jobCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				jobCharge.JR_OA_SellInvoiceAddress = TestObjectCreator.ABIGAS.AddressForSendingARDocuments.PK;
				Factory.Save();

				consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
				using (var form = new ConsolForm(consol))
				{
					var apportionmentListing = form.PlugIns.GetPlugIn(ControllerIDs.Apportionment).BusinessEntity as Business.ConsolCosting.ApportionmentListing;
					AssertEquals("Should have one cost", 1, apportionmentListing.CostsFilteredCollection.Count);
					AssertEquals("Should have one charge", 1, apportionmentListing.CostsFilteredCollection[0].ApportionmentCharges.Count);

					var charge = apportionmentListing.CostsFilteredCollection[0].ApportionmentCharges[0];
					Assert("precondition: charge is not deleted", !charge.IsDeleted);
					apportionmentListing.CostsFilteredCollection.RemoveAndDeleteAll();
					Assert("precondition: charge is deleted", charge.IsDeleted);

					var newFactory = new BusinessObjectFactory();
					var newShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
					var job = new Job.Loader(newShipment).Load();
					var wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, newFactory, job, null);
					wrapper.DoTestPostTransactions = true;
					wrapper.Post();

					var newFactory2 = new BusinessObjectFactory();
					var invoicePosted = newFactory2.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK));
					AssertNotNull("Should be one invoice created", invoicePosted);

					new ARInvoiceReversing(invoicePosted as ARInvoice).Reverse();
					newFactory2.Save();
					Assert("Invoice must be reversed", invoicePosted.IsCancelled);

					form.Show();
					form.FireSaveButton();
					AssertContains("Job charge was modified by this user during another operation", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("Should NOT trigger error report", CargoWise.Common.ErrorReporter.LastExceptionReported);
				}
			}
		}

		void TestSkipDataRefreshBusUpdate_NonDeletedSubscriber()
		{
			using (AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var chargeRevRecOverride1 = TestObjectCreator.CC1.RevenueRecOverrides.AddNew();
				chargeRevRecOverride1.AE_JobType = JobInvoicingConsumerTypes.Shipment.Code;
				chargeRevRecOverride1.AE_Direction = Core.Constants.FreightShipmentDirection.Code.All;
				chargeRevRecOverride1.AE_Mode = Core.Constants.TransportModes.All;
				chargeRevRecOverride1.AE_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
				TestObjectCreator.Creditor1.OH_IsConsignee = true;
				TestObjectCreator.Creditor2.OH_IsConsignor = true;
				TestObjectCreator.CC1.AC_MarginPercentage = 0m;
				Factory.Save();

				var consol = TestObjectCreator.CreateConsol(consolNum: "C001");
				var shipment = TestObjectCreator.CreateShipment("S0001", consol);
				shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
				shipment.JS_RL_NKDestination = "HKHKG";
				shipment.JS_E_ARV = ZDateTime.Empty;
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.Creditor1.PK;
				shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.Creditor2.PK;
				var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor3, 250M, true);
				shipment.Job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
				Factory.Save();

				var jobCharge = Factory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, shipment.Job.PK));
				jobCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				jobCharge.JR_OSSellAmt = 100m;
				jobCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				jobCharge.JR_OA_SellInvoiceAddress = TestObjectCreator.ABIGAS.AddressForSendingARDocuments.PK;
				Factory.Save();

				consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
				using (var form = new ConsolForm(consol))
				{
					var apportionmentListing = form.PlugIns.GetPlugIn(ControllerIDs.Apportionment).BusinessEntity as Business.ConsolCosting.ApportionmentListing;
					AssertEquals("Should have one cost", 1, apportionmentListing.CostsFilteredCollection.Count);
					AssertEquals("Should have one charge", 1, apportionmentListing.CostsFilteredCollection[0].ApportionmentCharges.Count);

					var charge = apportionmentListing.CostsFilteredCollection[0].ApportionmentCharges[0];

					using (consol.Factory.Load<Job>(jobCharge.JR_JH).ChargesLoadSuspender_ExposedForTestOnly.GetSuspender())
					{
						Assert("precondition: charge is not deleted", !charge.IsDeleted);
						apportionmentListing.CostsFilteredCollection.RemoveAndDeleteAll();
						Assert("precondition: charge is not deleted", !charge.IsDeleted);

						var newFactory = new BusinessObjectFactory();
						var newShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
						var job = new Job.Loader(newShipment).Load();
						var wrapper = new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.Revenue, newFactory, job, null);
						wrapper.DoTestPostTransactions = true;
						wrapper.Post();

						var newFactory2 = new BusinessObjectFactory();
						var invoicePosted = newFactory2.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK));
						AssertNotNull("Should be one invoice created", invoicePosted);

						new ARInvoiceReversing(invoicePosted as ARInvoice).Reverse();
						newFactory2.Save();
						Assert("Invoice must be reversed", invoicePosted.IsCancelled);
					}

					form.Show();
					form.FireSaveButton();
					AssertContains("Job charge was modified by this user during another operation", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("Should NOT trigger error report", CargoWise.Common.ErrorReporter.LastExceptionReported);
				}
			}
		}

		public void TestOnBusinessObjectIsCancelledChanged()
		{
			PrepareGatewayConsolTestEnvironment(out ForwardingConsol consol, out OrgHeader raBne);
			consol.Shipments.RemoveAll();
			Factory.Save();

			var plugIn = new ApportionmentPlugin(consol);
			var consolCost1 = plugIn.Apportionments.CostsCollection.AddNew();
			consolCost1.ApportionmentCharges.AddNew();
			var consolCost2 = plugIn.Apportionments.CostsCollection.AddNew();

			plugIn.OnBusinessObjectIsCancelledChanged(false);
			AssertEquals("Active should not delete charges.", 2, plugIn.Apportionments.CostsCollection.Count);

			plugIn.OnBusinessObjectIsCancelledChanged(true);
			AssertEquals("Inactive should delete unpost charges.", 1, plugIn.Apportionments.CostsCollection.Count);
			AssertEquals(consolCost1, plugIn.Apportionments.CostsCollection[0]);
			Assert("Unpost charge should be deleted.", consolCost2.IsDeleted);

			ErrorReporter.Clear();
			plugIn.Dispose();
		}

		void PrepareGatewayConsolTestEnvironment(out ForwardingConsol gatewayConsol, out OrgHeader proxyOrg)
		{
			var s1cnr = TestObjectCreator.CreateOrgHeader("S1CNR", false, true, "AUSYD");
			var s1cne = TestObjectCreator.CreateOrgHeader("S1CNE", false, true, "SGSIN");

			var saSyd = TestObjectCreator.CreateOrgHeader("SA_SYD", true, true, "AUSYD");
			var raBne = TestObjectCreator.CreateOrgHeader("RA_BNE", true, true, "AUBNE");

			var appPort = raBne.AppointedGatewayAgentPorts.AddNew();
			appPort.O5_OA_AgentOfficeAddress = raBne.MainAddress.PK;
			appPort.O5_PortOrCountry = "AUBNE";
			appPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appPort.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var bneBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BNE"));
			bneBranch.GB_OH_OrgProxy = raBne.PK;

			Factory.Save();

			var consol = TestObjectCreator.CreateGatewayConsol("AUSYD", "AUBNE", receivingGatewayAgent: raBne);
			consol.JK_OA_SendingForwarderAddress = saSyd.MainAddress.PK;

			var shipment1 = TestObjectCreator.CreateShipment("S0001", "AUSYD", "SGSIN", consol, incoTerm: "CFR", housebill: "S0001");
			shipment1.ConsignorPK = s1cnr.PK;
			shipment1.ConsigneePK = s1cne.PK;

			Factory.Save();

			Assert("Precondition: consol.IsGateway.", ((IGateway)consol).GatewayBillingSupporter.IsGatewayBillingEnabled());

			proxyOrg = raBne;
			gatewayConsol = consol;
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}

				return testObjectCreator;
			}
		}
		TestObjectCreator testObjectCreator;

		bool rawEnableComplianceRisk;
		EnableComplianceWiseRegistryBusinessObject rawFreightComplianceWiseRegistry;

		protected override void SetUp()
		{
			base.SetUp();
			rawEnableComplianceRisk = RawDataRegistry.Instance.EnableComplianceRisk.Value;
			rawFreightComplianceWiseRegistry = FreightDataRegistry.Instance.FreightEnableComplianceWise.DefaultValue;

			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false));
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			base.TearDown();
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawEnableComplianceRisk);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawFreightComplianceWiseRegistry);
		}
	}
}

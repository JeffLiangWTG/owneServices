using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.ConsolRevenue;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.ARAP.UnapprovedAPTransaction;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting.Testing
{
	public class ApportionmentPlugInBaseTest : ApportionmentPlugInTestBase
	{
		#region Gateway Target Job Query Tests

		public void TestImportAPInvoicesForForwardingConsolWithJobChargeTarget()
		{
			var sisterCompanyOrgProxy1 = TestObjectCreator.CreateOrgHeader("SISOR1", true, false);
			var sisterCompany1 = TestObjectCreator.CreateNewCompany("SI1", orgProxy: sisterCompanyOrgProxy1);
			var sisterBranch1 = TestObjectCreator.CreateNewBranch(sisterCompany1, "SI1");

			var sisterCompanyOrgProxy2 = TestObjectCreator.CreateOrgHeader("SISOR2", true, false);
			var sisterCompany2 = TestObjectCreator.CreateNewCompany("SI2", orgProxy: sisterCompanyOrgProxy2);
			var sisterBranch2 = TestObjectCreator.CreateNewBranch(sisterCompany2, "SI2");

			Factory.Save();

			var consol1 = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", receivingGatewayCompany: sisterCompany1);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", consol1);
			var shipment2 = TestObjectCreator.CreateShipment("S1112", consol1);
			var consol2 = TestObjectCreator.CreateGatewayConsol("KRSEL", "USLAX", "C002", receivingGatewayCompany: sisterCompany1);
			consol2.Shipments.AddRange(new[] { shipment1, shipment2 });
			Factory.Save();

			var debtorForARInvoice = GlbCompany.CurrentCompany.OrgProxy;
			var expectedARInvoicePksForConsol1 = new List<ZGuid>();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				GlbCompany.CurrentCompany.OrgProxy.CompanyData.OB_IsDebtor = true;
				GlbCompany.CurrentCompany.Factory.Save();

				var shipment1Job = TestObjectCreator.CreateJob(shipment1);
				Factory.Save();

				var invoicePostedFromConsol1 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol1.AH_ConsolidatedInvoiceRef = consol1.JK_UniqueConsignRef;
				var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol1, shipment1Job, TestObjectCreator.CC1, 100m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateCharge(invoiceLine);

				var invoicePostedFromConsol2 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol2.AH_ConsolidatedInvoiceRef = consol2.JK_UniqueConsignRef;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol2, shipment1Job, TestObjectCreator.CC1, 200m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateCharge(invoiceLine);

				Factory.Save();

				expectedARInvoicePksForConsol1.Add(invoicePostedFromConsol1.PK);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				GlbCompany.CurrentCompany.OrgProxy.CompanyData.OB_IsDebtor = true;
				GlbCompany.CurrentCompany.Factory.Save();

				var shipment1Job = TestObjectCreator.CreateJob(shipment1);
				var shipment2Job = TestObjectCreator.CreateJob(shipment2);
				var consol1GatewayJob = TestObjectCreator.CreateJob(consol1);
				var consol2GatewayJob = TestObjectCreator.CreateJob(consol2);
				Factory.Save();

				var invoicePostedFromConsol1WithoutJobChargeTargets = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol1WithoutJobChargeTargets.AH_ConsolidatedInvoiceRef = consol1.JK_UniqueConsignRef;
				invoicePostedFromConsol1WithoutJobChargeTargets.AH_JH = consol1GatewayJob.PK;
				var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol1WithoutJobChargeTargets, consol1GatewayJob, TestObjectCreator.CC1, 300m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateCharge(invoiceLine);

				var invoicePostedFromConsol1WithJobTargetShipment1 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol1WithJobTargetShipment1.AH_ConsolidatedInvoiceRef = consol1.JK_UniqueConsignRef;
				invoicePostedFromConsol1WithJobTargetShipment1.AH_JH = consol1GatewayJob.PK;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol1WithJobTargetShipment1, consol1GatewayJob, TestObjectCreator.CC1, 400m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateChargeWithTarget(invoiceLine, shipment1, shipment1);

				var invoicePostedFromConsol1WithJobTargetConsol2 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol1WithJobTargetConsol2.AH_ConsolidatedInvoiceRef = consol1.JK_UniqueConsignRef;
				invoicePostedFromConsol1WithJobTargetConsol2.AH_JH = consol1GatewayJob.PK;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol1WithJobTargetConsol2, consol1GatewayJob, TestObjectCreator.CC1, 500m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateChargeWithTarget(invoiceLine, shipment1, consol2);

				var invoicePostedFromConsol1WithJobTargetConsol1 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol1WithJobTargetConsol1.AH_ConsolidatedInvoiceRef = consol1.JK_UniqueConsignRef;
				invoicePostedFromConsol1WithJobTargetConsol1.AH_JH = consol1GatewayJob.PK;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol1WithJobTargetConsol1, consol1GatewayJob, TestObjectCreator.CC1, 600m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateChargeWithTarget(invoiceLine, shipment1, consol1);

				var invoicePostedFromConsol2WithoutJobChargeTargets = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol2WithoutJobChargeTargets.AH_ConsolidatedInvoiceRef = consol2.JK_UniqueConsignRef;
				invoicePostedFromConsol2WithoutJobChargeTargets.AH_JH = consol2GatewayJob.PK;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol2WithoutJobChargeTargets, consol2GatewayJob, TestObjectCreator.CC1, 700m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateCharge(invoiceLine);

				var invoicePostedFromConsol2WithJobTargetShipment1 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol2WithJobTargetShipment1.AH_ConsolidatedInvoiceRef = consol2.JK_UniqueConsignRef;
				invoicePostedFromConsol2WithJobTargetShipment1.AH_JH = consol2GatewayJob.PK;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol2WithJobTargetShipment1, consol2GatewayJob, TestObjectCreator.CC1, 800m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateChargeWithTarget(invoiceLine, shipment1, shipment1);

				var invoicePostedFromConsol2WithJobTargetConsol1 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol2WithJobTargetConsol1.AH_ConsolidatedInvoiceRef = consol2.JK_UniqueConsignRef;
				invoicePostedFromConsol2WithJobTargetConsol1.AH_JH = consol2GatewayJob.PK;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol2WithJobTargetConsol1, consol2GatewayJob, TestObjectCreator.CC1, 900m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateChargeWithTarget(invoiceLine, shipment1, consol1);

				var invoicePostedFromConsol2WithJobTargetConsol2 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, debtorForARInvoice);
				invoicePostedFromConsol2WithJobTargetConsol2.AH_ConsolidatedInvoiceRef = consol2.JK_UniqueConsignRef;
				invoicePostedFromConsol2WithJobTargetConsol2.AH_JH = consol2GatewayJob.PK;
				invoiceLine = TestObjectCreator.CreateInvoiceLine(invoicePostedFromConsol2WithJobTargetConsol2, consol2GatewayJob, TestObjectCreator.CC1, 1000m, TestObjectCreator.AUD, 1m);
				TestObjectCreator.CreateChargeWithTarget(invoiceLine, shipment1, consol2);

				Factory.Save();

				expectedARInvoicePksForConsol1.Add(invoicePostedFromConsol1WithoutJobChargeTargets.PK);
				expectedARInvoicePksForConsol1.Add(invoicePostedFromConsol1WithJobTargetShipment1.PK);
				expectedARInvoicePksForConsol1.Add(invoicePostedFromConsol1WithJobTargetConsol2.PK);
				expectedARInvoicePksForConsol1.Add(invoicePostedFromConsol1WithJobTargetConsol1.PK);
				expectedARInvoicePksForConsol1.Add(invoicePostedFromConsol2WithJobTargetConsol1.PK);
			}

			using (var plugin = new ApportionmentPlugin(consol1))
			{
				plugin.MenuItemImportAPInvoices_Click_ForTestOnly(null, EventArgs.Empty);
				var converter = (UnapprovedTransactionConverter)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				AssertEquals(6, converter.Candidates.Count);
				AssertContainsExactElementsInAnyOrder(expectedARInvoicePksForConsol1, converter.Candidates.Select(x => x.PK));
			}
		}

		public void TestGatewayTargetJobQueryFilterIsOnlyAddedToForwardingConsol()
		{
			var sisterCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("SISORG", true, false);
			var sisterCompany = TestObjectCreator.CreateNewCompany("SIS", orgProxy: sisterCompanyOrgProxy);
			var sisterBranch = TestObjectCreator.CreateNewBranch(sisterCompany, "SIS");
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				GlbCompany.CurrentCompany.OrgProxy.CompanyData.OB_IsDebtor = true;
				GlbCompany.CurrentCompany.Factory.Save();
			}

			var forwardingConsol = TestObjectCreator.CreateConsol();
			TestObjectCreator.CreateShipment("S1111", forwardingConsol);
			Factory.Save();
			AssertGatewayTargetJobQueryFilter(forwardingConsol, true);

			var transportBookingConsol = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbBookingConsolidation>());
			transportBookingConsol[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			Factory.Save();
			AssertGatewayTargetJobQueryFilter(transportBookingConsol, false);

			var landTransportRunSheet = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbConsignmentRunSheet>());
			Factory.Save();
			AssertGatewayTargetJobQueryFilter(landTransportRunSheet, false);

			var manifest = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbLinehaulManifest>());
			Factory.Save();
			AssertGatewayTargetJobQueryFilter(manifest, false);

			var portTransportRunSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			Factory.Save();
			AssertGatewayTargetJobQueryFilter(portTransportRunSheet, false);

			void AssertGatewayTargetJobQueryFilter(IBusiness consol, bool shouldContainJobChargeTargetFilter)
			{
				var job = TestObjectCreator.CreateJobHeader();
				job.JH_ParentID = (consol as BusinessObject).PK;
				Factory.Save();

				using (var plugin = new ApportionmentPlugin(consol))
				{
					plugin.MenuItemImportAPInvoices_Click_ForTestOnly(null, EventArgs.Empty);
					var converter = (UnapprovedTransactionConverter)ZFormModaliser.LastIBusinessShownOnDialogForTest;
					// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
					using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
					{
						var candidates = converter.Candidates;
						var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains("AH_ConsolidatedInvoiceRef"));
						var devMessage = "JobChargeTarget filter should " + (shouldContainJobChargeTargetFilter ? "" : "NOT ") + $"be added to {consol.TableName}";
						AssertEquals(devMessage, shouldContainJobChargeTargetFilter, queryPlan.Item1.Contains($"SELECT JRT_JR FROM dbo.JobChargeTarget WHERE JRT_InvoiceTargetID ="));
					}
				}
			}
		}

		#endregion

		[TestDate(2017, 01, 10)]
		public void TestUpdatingConsolCostExchangeRateDuringPostingConsolCostsOnlyDoesNotCreateDefaultApportionCharge()
		{
			var expectedExRate = 2.5m;
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2017, 01, 01));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, expectedExRate, new ZDateTime(2017, 01, 01), new ZDateTime(2017, 01, 31));
			Factory.Save();

			var consol = CreateConsol();
			var shipment1 = CreateShipment(consol, 10m, 10m);
			TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0m, TestObjectCreator.Agent, 0m);

			var shipment2 = CreateShipment(consol, 0m, 0m);
			TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0m, TestObjectCreator.Agent, 0m);
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, 100m);
			consolCost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			consolCost.E6_InvoiceNum = "INV123";
			consolCost.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
			consolCost.E6_InvoiceDate = new ZDateTime(2017, 01, 10);
			consolCost.E6_ExchangeRate = 1.5m;
			Factory.Save();

			AssertEquals(2, consolCost.ApportionmentCharges.Count);
			AssertEquals(1, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Count(x => x.JR_OSCostAmt == 0));
			AssertEquals(1, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Count(x => x.JR_OSCostAmt == 100m));
			Assert(!consolCost.IsPosted);
			AssertNotEquals(expectedExRate, consolCost.E6_ExchangeRate);

			using (PostingExRateRegistryAP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code))
			using (var testPlugIn = new ApportionmentPlugin(consol))
			{
				var menu = testPlugIn.GetNewTopLevelMenu_ForTestOnly().MenuItems.FindByText("Post Consol Costs Only");
				AssertNotNull("Post Consol Costs Only menu item should be available", menu);
				AssertNoExceptionThrown(() => menu.PerformClick());
			}

			var newFactory = new BusinessObjectFactory();
			var consolCostReloaded = newFactory.Load<JobConsolCost>(consolCost.PK);
			Assert(consolCostReloaded.IsPosted);
			AssertEquals(expectedExRate, consolCostReloaded.E6_ExchangeRate);
		}

		public void TestShouldAddAutoRates()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (var plugin = new ApportionmentPlugin(consol))
			{
				var strategy = new AutoRateApportionmentStrategy(plugin.BusinessEntity, plugin.Apportionments);
				AssertEquals("Consol rates always added", true, strategy.ShouldAddAutoRates);
			}
		}

		public void TestAddAutoRates_ShouldSetJobChargeAttributesForApportionSplitCharges()
		{
			var consol = CreateConsol();
			CreateShipment(consol);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, TestObjectCreator.FESDepartment.PK.ToGuid()))
			using (var plugin = new ApportionmentPlugin(consol))
			{
				var strategy = new AutoRateApportionmentStrategy(plugin.BusinessEntity, plugin.Apportionments);

				var collection = new AutoRateInfoCollection(Factory);
				var rate = new AutoRateInfo(Factory)
				{
					ChargeCode = GetChargeCode("FRT"),
					Currency = "AUD",
					ChargeUnit = QuantityUnit.CN
				};
				rate.AddFlatPaymentBasis(90m, consol.RatingAdapter.OperationalJobCode);
				rate.Attributes.Add(JobChargeAttribTypeList.Codes.ContainerCode, "20GP");
				rate.Attributes.Add(JobChargeAttribTypeList.Codes.ContainerNumber, "ABC");
				rate.Attributes.Add(JobChargeAttribTypeList.Codes.Commodity, "GEN");
				collection.Add(rate);

				strategy.AddAutoRates(new LoggerDecorator(), collection, CostSell.Cost, new[] { consol.RatingAdapter.OperationalJobCode });

				var cost = (IApportionedChargesHeader)plugin.Apportionments.CostsCollection.Single();
				var charge = (BaseCharge)cost.Charges.Single();

				var expectedAttributes = new[]
				{
					(JobChargeAttribTypeList.Codes.ContainerCode, "20GP"),
					(JobChargeAttribTypeList.Codes.ContainerNumber, "ABC"),
					(JobChargeAttribTypeList.Codes.Commodity, "GEN")
				};
				AssertContainsExactElementsInAnyOrder(
					expectedAttributes,
					charge.JobChargeAttributes
						.Cast<JobChargeAttrib>()
						.Select(x => (x.EC_Name.ToString(), x.EC_Value.ToString())));

				plugin.Apportionments.ReleaseMutexes();
			}
		}

		public void TestAddAutoRates_WithNullableChargeCode()
		{
			var consol = CreateConsol();
			CreateShipment(consol);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, TestObjectCreator.FESDepartment.PK.ToGuid()))
			using (var plugin = new ApportionmentPlugin(consol))
			{
				JobConsolCost cost = plugin.Apportionments.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = ZGuid.Invalid;
				cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
				cost.E6_OSCostAmount = 100m;
				cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
				cost.E6_InvoiceNum = "ABC123";
				cost.E6_InvoiceDate = ZDateTime.Now;
				cost.E6_RatingBehaviour = RatingBehaviours.StopFromAutorating;

				AssertNull("ChargeCode should be null.", cost.ChargeCode);

				var strategy = new AutoRateApportionmentStrategy(plugin.BusinessEntity, plugin.Apportionments);

				var collection = new AutoRateInfoCollection(Factory);
				var rate = new AutoRateInfo(Factory)
				{
					ChargeCode = GetChargeCode("FRT"),
					Currency = "AUD",
					ChargeUnit = QuantityUnit.CN
				};
				rate.AddFlatPaymentBasis(90m, consol.RatingAdapter.OperationalJobCode);
				rate.Attributes.Add(JobChargeAttribTypeList.Codes.ContainerCode, "20GP");
				rate.Attributes.Add(JobChargeAttribTypeList.Codes.ContainerNumber, "ABC");
				rate.Attributes.Add(JobChargeAttribTypeList.Codes.Commodity, "GEN");
				collection.Add(rate);

				AssertNoExceptionThrown("Should not throw a NullReferenceException even if charge code does not exsist.", () => strategy.AddAutoRates(new LoggerDecorator(), collection, CostSell.Cost, new[] { consol.RatingAdapter.OperationalJobCode }));

				plugin.Apportionments.ReleaseMutexes();
			}
		}

		public void TestPreviewInvoiceSecurity()
		{
			var consol = Factory.New<ForwardingConsol>();
			Factory.Save();

			using (var plugin = new ApportionmentPlugin(consol))
			{
				var topLevelMenu = plugin.GetNewTopLevelMenu_ForTestOnly();

				Env.Security.MaintainConsolJobInvoicingPreviewInvoices.IsAllowed = false;

				var previewInvoicesMenuItem = topLevelMenu.MenuItems.FindByText(Constants.MenuNameConstants.PreviewInvoices);
				previewInvoicesMenuItem.PerformClick();

				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.ConsolInvPreviewInvoices));
			}
		}

		public void TestSplitApportionAmount()
		{
			var consol = CreateConsol();
			Factory.Save();
			CreateShipment(consol);
			CreateShipment(consol);
			CreateShipment(consol);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, TestObjectCreator.FESDepartment.PK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (var plugin = new ApportionmentPlugin(consol))
			{
				var strategy = new AutoRateApportionmentStrategy(plugin.BusinessEntity, plugin.Apportionments);

				var collection = new AutoRateInfoCollection(Factory);
				var rate = new AutoRateInfo(Factory)
				{
					ChargeCode = GetChargeCode("FRT"),
					Currency = "AUD",
					ChargeUnit = RatingConstants.Units.HB
				};
				rate.AddFlatPaymentBasis(90m, consol.RatingAdapter.OperationalJobCode);
				rate.AddAgentFlatPaymentBasis(180m, consol.RatingAdapter.OperationalJobCode);

				collection.Add(rate);
				strategy.AddAutoRates(new LoggerDecorator(), collection, CostSell.Cost, new[] { consol.RatingAdapter.OperationalJobCode });

				AssertEquals(1, plugin.Apportionments.CostsCollection.Count);
				AssertEquals(3, plugin.Apportionments.CostsCollection[0].ApportionmentCharges.Count);

				AssertEquals(30m, plugin.Apportionments.CostsCollection[0].ApportionmentCharges[0].JR_OSCostAmt);
				AssertEquals(60m, plugin.Apportionments.CostsCollection[0].ApportionmentCharges[0].JR_AgentDeclaredCostAmt);

				AssertEquals(30m, plugin.Apportionments.CostsCollection[0].ApportionmentCharges[1].JR_OSCostAmt);
				AssertEquals(60m, plugin.Apportionments.CostsCollection[0].ApportionmentCharges[1].JR_AgentDeclaredCostAmt);

				AssertEquals(30m, plugin.Apportionments.CostsCollection[0].ApportionmentCharges[2].JR_OSCostAmt);
				AssertEquals(60m, plugin.Apportionments.CostsCollection[0].ApportionmentCharges[2].JR_AgentDeclaredCostAmt);

				plugin.Apportionments.ReleaseMutexes();
			}
		}

		public void TestAutoRatingApportionAmountForceZero()
		{
			var consol = CreateConsol();
			var shipment1 = CreateShipment(consol, 351.73m, 40m);
			var shipment2 = CreateShipment(consol, 786.23m, 60m);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, TestObjectCreator.FESDepartment.PK.ToGuid()))
			using (var plugin = new ApportionmentPlugin(consol))
			using (RatingDataRegistry.Instance.UnspecifiedCostShouldForceZeroToBePulledThrough.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var strategy = new AutoRateApportionmentStrategy(plugin.BusinessEntity, plugin.Apportionments);

				var collection = new AutoRateInfoCollection(Factory);
				var rate1 = new AutoRateInfo(Factory)
				{
					ChargeCode = GetChargeCode("FRT"),
					Currency = "AUD",
					ChargeUnit = RatingConstants.Units.HB
				};
				var rate2 = new AutoRateInfo(Factory)
				{
					ChargeCode = GetChargeCode("BAF"),
					Currency = "AUD",
					ChargeUnit = RatingConstants.Units.DO
				};
				rate1.AddFlatPaymentBasis(200m, consol.RatingAdapter.OperationalJobCode);
				rate2.AddFlatPaymentBasis(200m, consol.RatingAdapter.OperationalJobCode);

				collection.Add(rate1);
				collection.Add(rate2);

				strategy.AddAutoRates(new LoggerDecorator(), collection, CostSell.Cost, new[] { consol.RatingAdapter.OperationalJobCode });

				var cost1 = plugin.Apportionments.CostsCollection[0];
				var charge11 = cost1.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault(x => x.Job.JH_ParentID == shipment1.PK);
				var charge12 = cost1.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault(x => x.Job.JH_ParentID == shipment2.PK);

				AssertEquals(100m, charge11.JR_OSCostAmt);
				AssertEquals(100m, charge12.JR_OSCostAmt);
				AssertEquals(0m, charge11.JR_AgentDeclaredCostAmt);
				AssertEquals(0m, charge12.JR_AgentDeclaredCostAmt);

				var cost2 = plugin.Apportionments.CostsCollection[1];
				var charge21 = cost2.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault(x => x.Job.JH_ParentID == shipment1.PK);
				var charge22 = cost2.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault(x => x.Job.JH_ParentID == shipment2.PK);

				AssertEquals(80m, charge21.JR_OSCostAmt);
				AssertEquals(120m, charge22.JR_OSCostAmt);
				AssertEquals(0m, charge11.JR_AgentDeclaredCostAmt);
				AssertEquals(0m, charge12.JR_AgentDeclaredCostAmt);

				plugin.Apportionments.ReleaseMutexes();
			}
		}

		public void TestAutoRatingApportionAmountDoNotForceZero()
		{
			var consol = CreateConsol();
			var shipment1 = CreateShipment(consol, 351.73m, 40m);
			var shipment2 = CreateShipment(consol, 786.23m, 60m);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, TestObjectCreator.FESDepartment.PK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (var plugin = new ApportionmentPlugin(consol))
			{
				RatingDataRegistry.Instance.UnspecifiedCostShouldForceZeroToBePulledThrough.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				var strategy = new AutoRateApportionmentStrategy(plugin.BusinessEntity, plugin.Apportionments);

				var collection = new AutoRateInfoCollection(Factory);
				var rate1 = new AutoRateInfo(Factory)
				{
					ChargeCode = GetChargeCode("FRT"),
					Currency = "AUD",
					ChargeUnit = RatingConstants.Units.HB
				};
				var rate2 = new AutoRateInfo(Factory)
				{
					ChargeCode = GetChargeCode("BAF"),
					Currency = "AUD",
					ChargeUnit = RatingConstants.Units.DO
				};
				rate1.AddFlatPaymentBasis(200m, consol.RatingAdapter.OperationalJobCode);
				rate2.AddFlatPaymentBasis(200m, consol.RatingAdapter.OperationalJobCode);
				collection.Add(rate1);
				collection.Add(rate2);

				strategy.AddAutoRates(new LoggerDecorator(), collection, CostSell.Cost, new[] { consol.RatingAdapter.OperationalJobCode });

				var cost1 = plugin.Apportionments.CostsCollection[0];
				var charge11 = cost1.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault(x => x.Job.JH_ParentID == shipment1.PK);
				var charge12 = cost1.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault(x => x.Job.JH_ParentID == shipment2.PK);

				AssertEquals(100m, charge11.JR_OSCostAmt);
				AssertEquals(100m, charge12.JR_OSCostAmt);
				AssertEquals(100m, charge11.JR_AgentDeclaredCostAmt);
				AssertEquals(100m, charge12.JR_AgentDeclaredCostAmt);

				var cost2 = plugin.Apportionments.CostsCollection[1];
				var charge21 = cost2.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault(x => x.Job.JH_ParentID == shipment1.PK);
				var charge22 = cost2.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault(x => x.Job.JH_ParentID == shipment2.PK);

				AssertEquals(80m, charge21.JR_OSCostAmt);
				AssertEquals(120m, charge22.JR_OSCostAmt);
				AssertEquals(80m, charge21.JR_AgentDeclaredCostAmt);
				AssertEquals(120m, charge22.JR_AgentDeclaredCostAmt);

				plugin.Apportionments.ReleaseMutexes();
			}
		}

		public void TestPostOverseasAgentChargesWithProfitSharesWithMultipleLinesAsAgent()
		{
			AssertPostingOverseasAgentChargeWithProfitShares(JobInvoicingPostingOption.Agent, true, new ZDecimal[] { -23m, -27m });
		}

		public void TestPostOverseasAgentChargesWithProfitSharesWithMultipleLinesAsAll()
		{
			AssertPostingOverseasAgentChargeWithProfitShares(JobInvoicingPostingOption.All, true, new ZDecimal[] { -23m, -27m });
		}

		public void TestPostOverseasAgentChargesWithProfitSharesWithOneLineAsAgent()
		{
			AssertPostingOverseasAgentChargeWithProfitShares(JobInvoicingPostingOption.Agent, false, new ZDecimal[] { -307.20m, -148.80m });
		}

		public void TestPostOverseasAgentChargesWithProfitSharesWithOneLineAsAll()
		{
			AssertPostingOverseasAgentChargeWithProfitShares(JobInvoicingPostingOption.All, false, new ZDecimal[] { -307.20m, -148.80m });
		}

		void AssertPostingOverseasAgentChargeWithProfitShares(JobInvoicingPostingOption postingOption, bool isMultiLines, ZDecimal[] expectedARCRDAmounts)
		{
			var idTest = string.Format("isMultiLines={0} postingOption={1}: ", isMultiLines, postingOption);
			AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			SetupOrgHeadersForPosting();
			SetupConsolAndShipmentForPosting(Factory);
			CreateProfitShareAgreement(ConsolReceivingAgent, ConsolSendingAgent);
			Factory.Save();

			var mockIPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			mockIPrintTaskUIProvider.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ZArchitecture.Modules.ISecurityCheckpoint>()))
				.Returns(true);
			mockIPrintTaskUIProvider.Setup(m => m.ShowPrinterSelectionUI(It.IsAny<DeliveryInstructions>()))
				.Callback((DeliveryInstructions instructions) =>
				{
					instructions.PrinterDelivery.PrintQueuePK = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue))).PK;
					Factory.Save();
				});

			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockIPrintTaskUIProvider.Object))
			using (var plugIn = new ApportionmentPlugin(Consol))
			{
				var cost = plugIn.Apportionments.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
				cost.E6_OH_Creditor = ConsolReceivingAgent.PK;
				cost.E6_RX_NKCurrency = "AUD";
				cost.E6_OSCostAmount = 80m;
				cost.E6_LocalCostAmount = 80m;
				cost.E6_InvoiceNum = TestObjectCreator.GetRandomString(10);
				cost.E6_InvoiceDate = ZDateTime.Now;
				cost.E6_PaymentDate = ZDateTime.Now;
				Factory.Save();

				if (isMultiLines)
				{
					ShipmentJob.Charges[0].JR_OH_SellAccount = ZGuid.Empty;
					ShipmentJob.Charges[0].JR_LocalSellAmt = 0m;
					var jobCharge2 = ShipmentJob.Charges.AddNew();
					jobCharge2.JR_AC = Env.Registry.FreightChargeCode;
					jobCharge2.JR_LocalSellAmt = 120m;
					jobCharge2.JR_OH_SellAccount = ConsolReceivingAgent.PK;
					jobCharge2.JR_AgentDeclaredSellAmt = 190m;
					jobCharge2.JR_AgentDeclaredCostAmt = 20m;
					jobCharge2.JR_IsIncludedInProfitShare = true;
				}
				else
				{
					ShipmentJob.Charges[0].JR_OH_SellAccount = ConsolReceivingAgent.PK;
					ShipmentJob.Charges[0].JR_LocalSellAmt = 120m;
					ShipmentJob.Charges[0].JR_AgentDeclaredSellAmt = 500m;
					ShipmentJob.Charges[0].JR_AgentDeclaredCostAmt = 4m;
				}
				Factory.Save();

				Assert(idTest + "The Consol Cost must be a collect invoice because the creditor is the receiving agent", cost.E6_IsForCollectInvoice);
				Assert(idTest + "The charge must be include in profitshare", cost.ApportionmentCharges[0].JR_IsIncludedInProfitShare);

				plugIn.PostTransactions_ForTestOnly(postingOption);
			}

			var postedARCRD = Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, SQLComparisonOperator.StartsWith, ShipmentJob.JH_ConsolNo).AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable).AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
			AssertContainsExactElementsInAnyOrder(idTest + "Posted AR CRD have incorrect invoice amounts", expectedARCRDAmounts, postedARCRD.Select(x => x.AH_InvoiceAmount));

			var postedARINV = Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, SQLComparisonOperator.StartsWith, ShipmentJob.JH_ConsolNo).AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable).AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
			AssertEquals(idTest + "No posted AR INV", 0, postedARINV.Length);

			var postedAPINV = Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_Desc, SQLComparisonOperator.StartsWith, Shipment.JS_HouseBill).AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
			AssertEquals(idTest + "3 AP INV", 3, postedAPINV.Length);
			Assert(idTest + "AP INV with zero invoice amounts", postedAPINV.All(x => x.AH_InvoiceAmount == ZDecimal.Zero));
		}

		public void TestPostOverseasAgentChargesWhenExchangeRateOfJobLineNotEqualToNonJobLine()
		{
			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			SetupOrgHeadersForPosting();
			SetupConsolAndShipmentForPosting(Factory);

			var usdExchangeRate = TestObjectCreator.USD.ExchangeRates.AddNew();
			usdExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			usdExchangeRate.RE_SellRate = 1.5M;
			usdExchangeRate.RE_StartDate = ZDateTime.Today;
			usdExchangeRate.RE_ExpiryDate = ZDateTime.Today;

			using (var plugIn = new ApportionmentPlugin(Consol))
			{
				var cost = plugIn.Apportionments.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
				cost.E6_OH_Creditor = ConsolReceivingAgent.PK;
				cost.E6_RX_NKCurrency = "USD";
				cost.E6_ExchangeRate = 1.2M;
				cost.E6_OSCostAmount = 120M;
				cost.E6_LocalCostAmount = 100M;
				cost.E6_InvoiceNum = TestObjectCreator.GetRandomString(10);
				cost.E6_InvoiceDate = ZDateTime.Now;
				cost.E6_PaymentDate = ZDateTime.Now;
				Factory.Save();

				AssertNoExceptionThrown(() => plugIn.PostTransactions_ForTestOnly(JobInvoicingPostingOption.Agent));
			}

			var postedAPINVs = Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_Desc, SQLComparisonOperator.StartsWith, Shipment.JS_HouseBill).AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable));

			AssertNotNull(postedAPINVs);
			AssertEquals(1, postedAPINVs.Length);

			var postedAPINV = postedAPINVs[0];
			AssertEquals(0M, postedAPINV.AH_InvoiceAmount);
			AssertEquals(0M, postedAPINV.AH_OSTotal);
			AssertEquals(1M, postedAPINV.AH_ExchangeRate);
			AssertEquals(2, postedAPINV.Lines.Count);
			AssertEquals(1.2M, postedAPINV.Lines[0].AL_ExchangeRate);
			AssertEquals(1.2M, postedAPINV.Lines[1].AL_ExchangeRate);
		}

		public override void TestName()
		{
			var consol = CreateConsol();
			using (var plugIn = GetTestApportionmentPluginObject(consol))
			{
				AssertEquals("Consol Costing", plugIn.Name);
			}
		}

		public virtual void TestMenuName()
		{
			var consol = CreateConsol();
			using (var plugIn = GetTestApportionmentPluginObject(consol))
			{
				AssertEquals("&Job Invoicing", plugIn.MenuName);
			}
		}

		public override void TestRefreshGatewayElements()
		{
			var consol = CreateConsol();
			using (var plugIn = GetTestApportionmentPluginObject(consol))
			using (var control = plugIn.UserControl)
			using (var form = new ZForm(consol))
			{
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(control, "");
				form.Controls.Add(control);
				form.Show();

				var menuItems = plugIn.TopLevelMenu.MenuItems.Cast<MenuItem>();
				((IPluginShouldRefreshMenuForGateway)plugIn).RefreshGatewayElements(false);
				foreach (var item in menuItems)
				{
					AssertEquals(!item.Text.Equals(Constants.MenuNameConstants.PostGatewayAgentCharges), item.Visible);
				}

				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				((IPluginShouldRefreshMenuForGateway)plugIn).RefreshGatewayElements(false);
				Assert(menuItems.All(x => x.Visible));

				((IPluginShouldRefreshMenuForGateway)plugIn).RefreshGatewayElements(true);
				Assert(menuItems.All(x => x.Visible));
			}
		}

		public override void TestCheckIsUsedForGatewayApportionments()
		{
			var nonGatewayConsol = CreateConsol();
			var sisterCompanyOrgProxy1 = TestObjectCreator.CreateOrgHeader("SISOR1", true, false);
			var sisterCompany1 = TestObjectCreator.CreateNewCompany("SI1", orgProxy: sisterCompanyOrgProxy1);
			var sisterBranch1 = TestObjectCreator.CreateNewBranch(sisterCompany1, "SI1");
			Factory.Save();

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", receivingGatewayCompany: sisterCompany1);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sisterBranch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertCheckIsUsedForGatewayApportionments(nonGatewayConsol, false);
				AssertCheckIsUsedForGatewayApportionments(gatewayConsol, false);
			}
		}

		public void TestMenuItemGateway()
		{
			Env.Security.MaintainConsolJobInvoicingPostGateway.IsAllowed = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var consol = CreateConsol();

			using (var plugIn = new ApportionmentPlugin(consol))
			using (var control = plugIn.UserControl)
			using (var form = new ZForm(consol))
			{
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(control, "");
				form.Controls.Add(control);
				form.Show();

				var menu = plugIn.GetNewTopLevelMenu_ForTestOnly().MenuItems[0];
				AssertEquals(Constants.MenuNameConstants.PostGatewayAgentCharges, menu.Text);
				Assert("Gateway should be invisible by default to only be shown for gateway consol", !menu.Visible);
				menu.PerformClick();
				AssertEquals(plugIn.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.ConsolInvPostGateway), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPostChargesOptionAsGateway_NoGatewayAgents() => AssertPostingGatewayAgentCharges();

		public void TestPostChargesOptionAsGateway_ReceivingGatewayAgent() => AssertPostingGatewayAgentCharges(isReceivingAgentGateway: true);

		public void TestPostChargesOptionAsGateway_SendingGatewayAgents() => AssertPostingGatewayAgentCharges(isSendingAgentGateway: true);

		public void TestPostChargesOptionAsGateway_BothGatewayAgents() => AssertPostingGatewayAgentCharges(true, true);

		void AssertPostingGatewayAgentCharges(bool isReceivingAgentGateway = false, bool isSendingAgentGateway = false)
		{
			SetupOrgHeadersForPosting();

			CombineAssertions(delegate
			{
				AssertPostingGatewayAgentCharge(30m, 80m, null, (isReceivingAgentGateway ? (ZDecimal?)80 : null), false, ConsolReceivingAgent, isReceivingAgentGateway, isSendingAgentGateway);
				AssertPostingGatewayAgentCharge(30m, 80m, null, null, false, OtherForwarder, isReceivingAgentGateway, isSendingAgentGateway);
				AssertPostingGatewayAgentCharge(30m, 80m, null, (isSendingAgentGateway ? (ZDecimal?)80 : null), false, ConsolSendingAgent, isReceivingAgentGateway, isSendingAgentGateway);

				AssertPostingGatewayAgentCharge(-10m, -50m, (isReceivingAgentGateway ? (ZDecimal?)-50 : null), null, false, ConsolReceivingAgent, isReceivingAgentGateway, isSendingAgentGateway);
				AssertPostingGatewayAgentCharge(-10m, -50m, null, null, false, OtherForwarder, isReceivingAgentGateway, isSendingAgentGateway);
				AssertPostingGatewayAgentCharge(-10m, -50m, (isSendingAgentGateway ? (ZDecimal?)-50 : null), null, false, ConsolSendingAgent, isReceivingAgentGateway, isSendingAgentGateway);

				AssertPostingGatewayAgentCharge(25m, 0m, null, null, false, ConsolReceivingAgent, isReceivingAgentGateway, isSendingAgentGateway);
				AssertPostingGatewayAgentCharge(25m, 0m, null, null, false, OtherForwarder, isReceivingAgentGateway, isSendingAgentGateway);
				AssertPostingGatewayAgentCharge(25m, 0m, null, null, false, ConsolSendingAgent, isReceivingAgentGateway, isSendingAgentGateway);
			});
		}

		public void TestPostAgentChargesWithRNVProcessTask()
		{
			SetupOrgHeadersForPosting();

			CombineAssertions(delegate
			{
				AssertPostingOverseasAgentCharge(30m, 80m, -30, null, 0, true, false, OtherForwarder, JobInvoicingPostingOption.Agent);
				AssertPostingOverseasAgentCharge(30m, 80m, -30, null, 0, true, false, OtherForwarder, JobInvoicingPostingOption.All);
				AssertPostingGatewayAgentCharge(30m, 80m, null, 80m, true, ConsolReceivingAgent, true, false);
			});
		}

		public void TestPostOverseasAgentChargesWithoutCreateProfitShareAsARPostingOptionAsAgent() => AssertPostingOverseasAgentCharges(false, JobInvoicingPostingOption.Agent);

		public void TestPostOverseasAgentChargesWithCreateProfitShareAsARPostingOptionAsAgent() => AssertPostingOverseasAgentCharges(true, JobInvoicingPostingOption.Agent);

		public void TestPostOverseasAgentChargesWithoutCreateProfitShareAsARPostingOptionAsAll() => AssertPostingOverseasAgentCharges(false, JobInvoicingPostingOption.All);

		public void TestPostOverseasAgentChargesWithCreateProfitShareAsARPostingOptionAsAll() => AssertPostingOverseasAgentCharges(true, JobInvoicingPostingOption.All);

		void AssertPostingOverseasAgentCharges(bool createProfitShareAsAR, JobInvoicingPostingOption postingOption)
		{
			SetupOrgHeadersForPosting();

			CombineAssertions(delegate
			{
				AssertPostingOverseasAgentCharge(30m, 80m, null, 50, 0, false, createProfitShareAsAR, ConsolReceivingAgent, postingOption);
				AssertPostingOverseasAgentCharge(30m, 80m, -30, null, 0, false, createProfitShareAsAR, OtherForwarder, postingOption);
				AssertPostingOverseasAgentCharge(30m, 80m, -30, 80, 0, false, createProfitShareAsAR, ConsolSendingAgent, postingOption);

				AssertPostingOverseasAgentCharge(80m, 15m, -65, null, 0, false, createProfitShareAsAR, ConsolReceivingAgent, postingOption);
				AssertPostingOverseasAgentCharge(80m, 15m, -80, null, 0, false, createProfitShareAsAR, OtherForwarder, postingOption);
				AssertPostingOverseasAgentCharge(80m, 15m, -80, 15, 0, false, createProfitShareAsAR, ConsolSendingAgent, postingOption);

				AssertPostingOverseasAgentCharge(-10m, -50m, -40, null, 0, false, createProfitShareAsAR, ConsolReceivingAgent, postingOption);
				AssertPostingOverseasAgentCharge(-10m, -50m, null, 10, 0, false, createProfitShareAsAR, OtherForwarder, postingOption);
				AssertPostingOverseasAgentCharge(-10m, -50m, -50, 10, 0, false, createProfitShareAsAR, ConsolSendingAgent, postingOption);

				AssertPostingOverseasAgentCharge(-100m, -50m, null, 50, 0, false, createProfitShareAsAR, ConsolReceivingAgent, postingOption);
				AssertPostingOverseasAgentCharge(-100m, -50m, null, 100, 0, false, createProfitShareAsAR, OtherForwarder, postingOption);
				AssertPostingOverseasAgentCharge(-100m, -50m, -50, 100, 0, false, createProfitShareAsAR, ConsolSendingAgent, postingOption);

				AssertPostingOverseasAgentCharge(25m, 0m, -25, null, 0, false, createProfitShareAsAR, ConsolReceivingAgent, postingOption);
				AssertPostingOverseasAgentCharge(25m, 0m, -25, null, 0, false, createProfitShareAsAR, OtherForwarder, postingOption);
				AssertPostingOverseasAgentCharge(25m, 0m, -25, null, 0, false, createProfitShareAsAR, ConsolSendingAgent, postingOption);

				AssertPostingOverseasAgentCharge(-30m, 0m, null, 30, 0, false, createProfitShareAsAR, ConsolReceivingAgent, postingOption);
				AssertPostingOverseasAgentCharge(-30m, 0m, null, 30, 0, false, createProfitShareAsAR, OtherForwarder, postingOption);
				AssertPostingOverseasAgentCharge(-30m, 0m, null, 30, 0, false, createProfitShareAsAR, ConsolSendingAgent, postingOption);
			});
		}

		void AssertPostingGatewayAgentCharge(ZDecimal costAmt, ZDecimal sellAmt, ZDecimal? expectedARCRDAmount, ZDecimal? expectedARINVAmount, bool withRNVProcessTask, OrgHeader debtor, bool isReceivingAgentGateway, bool isSendingAgentGateway)
		{
			AssertPostingAgentCharge(costAmt, sellAmt, expectedARCRDAmount, expectedARINVAmount, null, withRNVProcessTask, false, debtor, JobInvoicingPostingOption.Gateway, isReceivingAgentGateway, isSendingAgentGateway);
		}

		void AssertPostingOverseasAgentCharge(ZDecimal costAmt, ZDecimal sellAmt, ZDecimal? expectedARCRDAmount, ZDecimal? expectedARINVAmount, ZDecimal? expectedAPINVAmount, bool withRNVProcessTask, bool isCreateProfitShareAsAR, OrgHeader debtor, JobInvoicingPostingOption postingOption)
		{
			AssertPostingAgentCharge(costAmt, sellAmt, expectedARCRDAmount, expectedARINVAmount, expectedAPINVAmount, withRNVProcessTask, isCreateProfitShareAsAR, debtor, postingOption);
		}

		void AssertPostingAgentCharge(ZDecimal costAmt, ZDecimal sellAmt, ZDecimal? expectedARCRDAmount, ZDecimal? expectedARINVAmount, ZDecimal? expectedAPINVAmount, bool withRNVProcessTask, bool isCreateProfitShareAsAR, OrgHeader debtor, JobInvoicingPostingOption postingOption, bool isReceivingAgentGateway = false, bool isSendingAgentGateway = false)
		{
			var idTest = "";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, isCreateProfitShareAsAR);

			MasterFilesTestHelper.ClearWorkflowTables();

			var expectedARCRDAmounts = expectedARCRDAmount.HasValue ? new ZDecimal[] { expectedARCRDAmount.Value } : Array.Empty<ZDecimal>();
			var expectedARINVAmounts = expectedARINVAmount.HasValue ? new ZDecimal[] { expectedARINVAmount.Value } : Array.Empty<ZDecimal>();
			var expectedAPINVAmounts = expectedAPINVAmount.HasValue ? new ZDecimal[] { expectedAPINVAmount.Value } : Array.Empty<ZDecimal>();

			if (withRNVProcessTask)
			{
				var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				template.P0_Name = "AR Invoice Template";
				template.P0_ProcessType = "RNV";
				template.P0_GC = ZGuid.Empty;
				var templateTrigger = template.WorkflowItems.AddNew();
				templateTrigger.IsWorkflowTrigger = false;
				templateTrigger.IsMilestone = true;
				templateTrigger.TemplateConditions.TemplateCondition2 = "UDF";
				templateTrigger.TriggerConditions.TriggerEventCode = AutoEvents.IncidentClosedCode;
				Factory.Save();
			}

			SetupConsolAndShipmentForPosting(Factory);
			Consol.JK_ReceivingForwarderHandlingType = isReceivingAgentGateway ? AgentStatusList.Codes.GatewayAgent : string.Empty;
			Consol.JK_SendingForwarderHandlingType = isSendingAgentGateway ? AgentStatusList.Codes.GatewayAgent : string.Empty;

			using (var plugIn = new ApportionmentPlugin(Consol))
			{
				idTest += string.Format("cost={0} sell={1} debtor={2} postingOption={3}: ", costAmt, sellAmt, debtor.OH_FullName, postingOption);
				var cost = plugIn.Apportionments.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
				cost.E6_OH_Creditor = ConsolReceivingAgent.PK;
				cost.E6_RX_NKCurrency = "AUD";
				cost.E6_OSCostAmount = costAmt;
				cost.E6_LocalCostAmount = costAmt;
				cost.E6_InvoiceNum = TestObjectCreator.GetRandomString(10);
				cost.E6_InvoiceDate = ZDateTime.Now;
				cost.E6_PaymentDate = ZDateTime.Now;
				Factory.Save();

				ShipmentJob.Charges[0].JR_OH_SellAccount = sellAmt != 0 ? debtor.PK : ZGuid.Empty;
				ShipmentJob.Charges[0].JR_LocalSellAmt = sellAmt;
				Factory.Save();
				Assert(idTest + "The Consol Cost must be a collect invoice because the creditor is the receiving agent", cost.E6_IsForCollectInvoice);

				plugIn.PostTransactions_ForTestOnly(postingOption);
			}

			var postedARCRD = Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, SQLComparisonOperator.StartsWith, ShipmentJob.JH_ConsolNo).AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable).AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
			AssertContainsExactElementsInAnyOrder(idTest + "Posted AR CRD should have correct invoice amounts", expectedARCRDAmounts, postedARCRD.Select(x => x.AH_InvoiceAmount));
			var postedARINV = Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, SQLComparisonOperator.StartsWith, ShipmentJob.JH_ConsolNo).AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable).AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
			AssertContainsExactElementsInAnyOrder(idTest + "Posted AR INV should have correct invoice amounts", expectedARINVAmounts, postedARINV.Select(x => x.AH_InvoiceAmount));
			var postedAPINV = Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_Desc, SQLComparisonOperator.StartsWith, Shipment.JS_HouseBill).AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
			AssertContainsExactElementsInAnyOrder(idTest + "Posted AP INV should have correct invoice amounts", expectedAPINVAmounts, postedAPINV.Select(x => x.AH_InvoiceAmount));
		}

		void SetupOrgHeadersForPosting()
		{
			var helper = new AccountingPeriodTestHelper(Factory);
			helper.PostPeriodsForEntireYear(ZDateTime.Today.Year, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			ConsolReceivingAgent = CreateForwarder("consolReceivingAgent");
			ConsolSendingAgent = CreateForwarder("consolSendingAgent");
			OtherForwarder = CreateForwarder("otherForwarder");
			Factory.Save();

			ConsolReceivingCompany = TestObjectCreator.CreateNewCompany("GWR", orgProxy: ConsolReceivingAgent);
			ConsolSendingCompany = TestObjectCreator.CreateNewCompany("GWS", orgProxy: ConsolSendingAgent);

			Factory.Save();
		}

		void SetupConsolAndShipmentForPosting(BusinessObjectFactory factory)
		{
			Consol = TestObjectCreator.CreateGatewayConsol("AUBNE", "NZAKL", $"C00{ConsolCount++}", sendingGatewayCompany: ConsolSendingCompany, receivingGatewayCompany: ConsolReceivingCompany);
			factory.Save();

			Shipment = Consol.Shipments.AddNew();
			Shipment.JS_TransportMode = Constants.TransportModes.Air;
			Shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			Shipment.JS_RL_NKOrigin = "AUBNE";
			Shipment.JS_RL_NKDestination = "NZAKL";
			Shipment.ConsigneePK = TestObjectCreator.AALSHI.PK;
			Shipment.ConsignorPK = TestObjectCreator.ABIGAS.PK;
			factory.Save();

			ShipmentJob = new Job.Loader(factory, Shipment).TryCreateWithoutMutexForTestOnly();
			ShipmentJob.JH_OA_LocalChargesAddr = Shipment.Consignor.MainAddress.PK;
			ShipmentJob.JH_OA_AgentCollectAddr = ConsolReceivingAgent.MainAddress.PK;
			ShipmentJob.JH_GE = TestObjectCreator.FEADepartment.PK;
			factory.Save();
		}

		Job ShipmentJob;
		ForwardingShipment Shipment;
		ForwardingConsol Consol;
		OrgHeader ConsolReceivingAgent;
		OrgHeader ConsolSendingAgent;
		OrgHeader OtherForwarder;
		GlbCompany ConsolReceivingCompany;
		GlbCompany ConsolSendingCompany;
		ZInt ConsolCount;

		public void TestApportionmentStrategy()
		{
			var allocationMethods = new List<string>
				{
					AllocationMethod.ChargeableUnits,
					AllocationMethod.ContainerCount,
					AllocationMethod.GrossWeight,
					AllocationMethod.GrossVolume,
					AllocationMethod.Manual,
					AllocationMethod.Revenue,
					AllocationMethod.Shipment,
					AllocationMethod.TwentyFootEquivalentUnit
				};

			var containerModes = new List<string>
				{
					Constants.ContainerModes.FCL,
					Constants.ContainerModes.LCL,
				};

			var transportModes = new List<string>
				{
					Constants.TransportModes.Road,
					Constants.TransportModes.Sea,
				};

			var consolTypes = new List<string>
				{
					Constants.AgentType.Agent,
					Constants.AgentType.CoLoad,
				};

			var newConfig = AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod.Value;
			var counter = 0;
			var frtChargeCode = GetChargeCode("FRT");
			var bafChargeCode = GetChargeCode("BAF");
			var cafChargeCode = GetChargeCode("CAF");

			foreach (var consolType in consolTypes)
			{
				newConfig.ConsolCostDefaultApportionmentMethodCollection.RemoveAll();
				foreach (var transportMode in transportModes)
				{
					foreach (var containerMode in containerModes)
					{
						var allocationMethod = allocationMethods[counter % allocationMethods.Count];
						var newMethod = new ConsolCostDefaultApportionmentMethod
						{
							Apportionment = allocationMethod,
							ContainerMode = containerMode,
							TransportMode = transportMode,
							ConsolType = consolType,
							Module = ApportionmentMethodModules.Forwarding
						};
						newConfig.ConsolCostDefaultApportionmentMethodCollection.Add(newMethod);
						AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newConfig);

						var consol = CreateConsol();
						consol.JK_TransportMode = transportMode;
						consol.JK_ConsolMode = containerMode;
						consol.JK_AgentType = consolType;
						Factory.Save();

						var shipment1 = CreateShipment(consol, 351.73m);
						shipment1.JS_ActualChargeable = 40m;
						var shipment2 = CreateShipment(consol, 786.23m);
						shipment2.JS_ActualChargeable = 60m;

						using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
						using (var plugin = new ApportionmentPlugin(consol))
						{
							var strategy = new AutoRateApportionmentStrategy(plugin.BusinessEntity, plugin.Apportionments);

							var collection = new AutoRateInfoCollection(Factory);
							var rate1 = new AutoRateInfo(Factory)
							{
								ChargeCode = frtChargeCode,
								Currency = "AUD",
								ChargeUnit = RatingConstants.Units.HB
							};
							var rate2 = new AutoRateInfo(Factory)
							{
								ChargeCode = bafChargeCode,
								Currency = "AUD",
								ChargeUnit = RatingConstants.Units.DO
							};
							var rate3 = new AutoRateInfo(Factory)
							{
								ChargeCode = cafChargeCode,
								Currency = "AUD",
								ChargeUnit = RatingConstants.Units.KG
							};
							rate1.AddFlatPaymentBasis(200m, consol.RatingAdapter.OperationalJobCode);
							rate2.AddFlatPaymentBasis(200m, consol.RatingAdapter.OperationalJobCode);
							rate3.AddFlatPaymentBasis(200m, consol.RatingAdapter.OperationalJobCode);
							collection.Add(rate1);
							collection.Add(rate2);
							collection.Add(rate3);

							strategy.AddAutoRates(new LoggerDecorator(), collection, CostSell.Cost, new[] { consol.RatingAdapter.OperationalJobCode });

							var cost1 = plugin.Apportionments.CostsCollection[0];
							AssertEquals(allocationMethod + ": HB rate is Allocated as SHP", AllocationMethod.Shipment, cost1.E6_ApportionmentMethod);

							var cost2 = plugin.Apportionments.CostsCollection[1];
							AssertEquals(allocationMethod + ": DO rate is Allocated as " + allocationMethod, allocationMethod, cost2.E6_ApportionmentMethod);

							var cost3 = plugin.Apportionments.CostsCollection[2];
							AssertEquals(allocationMethod + ": KG rate is Allocated as " + allocationMethod, allocationMethod, cost3.E6_ApportionmentMethod);

							plugin.Apportionments.ReleaseMutexes();
						}
					}
				}
			}
		}

		public void TestApportionmentStrategyWithJFCJob()
		{
			var consol = CreateConsol();
			Factory.Save();

			var shipment1 = CreateShipment(consol, 351.73m);
			shipment1.JS_ActualChargeable = 40m;
			var shipment2 = CreateShipment(consol, 786.23m);
			shipment2.JS_ActualChargeable = 60m;
			Factory.Save();

			TestObjectCreator.CreateJob(shipment1);
			TestObjectCreator.CreateJob(shipment2);
			Factory.Save();

			TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT);
			Factory.Save();

			using (var plugin = new ApportionmentPlugin(consol))
			{
				IAutoRatingValidator strategy = new AutoRateApportionmentStrategy(plugin.BusinessEntity, plugin.Apportionments, costingPlugIn: consol);

				Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false;
				shipment2.Job.JH_Status = JobHeaderStatus.Codes.JobReadyForFinancialClosure;
				Factory.Save();
				AssertNotEquals("Pre-condition", JobHeaderStatus.Codes.JobReadyForFinancialClosure, shipment1.Job.JH_Status);
				AssertEquals(JobHeaderStatus.Codes.JobReadyForFinancialClosure, shipment2.Job.JH_Status);

				AssertNotNullOrEmpty("Should NOT allow auto-rating when Consol has Job with JFC status", strategy.GetAutoratingNotPermittedReason());

				shipment2.Job.JH_Status = JobHeaderStatus.Codes.Working;
				Factory.Save();
				AssertNotEquals("Pre-condition", JobHeaderStatus.Codes.JobReadyForFinancialClosure, shipment1.Job.JH_Status);
				AssertNotEquals(JobHeaderStatus.Codes.JobReadyForFinancialClosure, shipment2.Job.JH_Status);

				AssertNullOrEmpty("Should allow auto-rating when Consol has no Job with JFC status", strategy.GetAutoratingNotPermittedReason());

				Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true;
				shipment2.Job.JH_Status = JobHeaderStatus.Codes.JobReadyForFinancialClosure;
				Factory.Save();
				AssertNotEquals("Pre-condition", JobHeaderStatus.Codes.JobReadyForFinancialClosure, shipment1.Job.JH_Status);
				AssertEquals(JobHeaderStatus.Codes.JobReadyForFinancialClosure, shipment2.Job.JH_Status);

				AssertNullOrEmpty("Should allow auto-rating when Consol has Job with JFC status but client has security right", strategy.GetAutoratingNotPermittedReason());
					
				plugin.Apportionments.ReleaseMutexes();
			}
		}

		public void TestApportionmentStrategyWithCLSJob()
		{
			var consol = CreateConsol();
			var shipment = CreateShipment(consol, 786.23m);
			shipment.JS_ActualChargeable = 60m;
			TestObjectCreator.CreateJob(shipment);
			TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT);
			Factory.Save();

			using (var plugin = new ApportionmentPlugin(consol))
			{
				IAutoRatingValidator strategy = new AutoRateApportionmentStrategy(plugin.BusinessEntity, plugin.Apportionments, costingPlugIn: consol);
				shipment.Job.JH_Status = JobHeaderStatus.Codes.Closed;

				AssertNotNullOrEmpty("Should NOT allow auto-rating when Consol has Job with CLS status", strategy.GetAutoratingNotPermittedReason());

				shipment.Job.JH_Status = JobHeaderStatus.Codes.Working;
				AssertNullOrEmpty("Should allow auto-rating when Consol has no Job with CLS status", strategy.GetAutoratingNotPermittedReason());
				plugin.Apportionments.ReleaseMutexes();
			}
		}

		public void TestAutoRatingCalculationLogs()
		{
			var consol = CreateConsol();
			CreateShipment(consol);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, TestObjectCreator.FESDepartment.PK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (var plugin = new ApportionmentPlugin(consol))
			{
				var rateInfo1 = new AutoRateInfo(Factory);
				rateInfo1.ChargeCode = GetChargeCode("FRT");
				rateInfo1.Currency = "AUD";
				rateInfo1.AddFlatPaymentBasis(10m, consol.RatingAdapter.OperationalJobCode);

				var entry = Factory.NewWithValidTestData<ClientRate>().EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection.AddNew();
				var rateLine = entry.RateLines.AddNew();
				rateLine.TL_AC = Env.Registry.FreightChargeCode;
				rateLine.TL_RateCalculator = FlatCalculator.Code;
				var calcLog = new Rating.Integration.CalculationLog();
				calcLog.BaseRate = 100m;

				var autoRatingParameters = new AutoRatingCalculatorParametersForTesting(new TestRatingCriteria());
				var calcOutput = new CalculatorOutput(autoRatingParameters, calcLog);
				var calculationResult = new CalculationResult(rateLine, calcOutput);

				var rateInfo2 = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);
				rateInfo2.ChargeCode = GetChargeCode("BAF");
				rateInfo2.Currency = "AUD";
				rateInfo2.AddFlatPaymentBasis(20m, consol.RatingAdapter.OperationalJobCode);

				var collection = new AutoRateInfoCollection(Factory);
				collection.Add(rateInfo1);
				collection.Add(rateInfo2);

				var strategy = new AutoRateApportionmentStrategy(plugin.BusinessEntity, plugin.Apportionments);
				strategy.AddAutoRates(new LoggerDecorator(), collection, CostSell.Cost, new[] { consol.RatingAdapter.OperationalJobCode });

				consol.Factory.ClearCachedValue<ApportionmentListing>("ApportionmentListing|" + consol.PK.ToString());
				var consolCosts = plugin.Apportionments.CostsCollection;
				AssertEquals(2, consolCosts.Count);

				var logsWrapper = CalculationLogsLoader.Load(consolCosts[0]);
				AssertNull("No calculation logs", logsWrapper);

				logsWrapper = CalculationLogsLoader.Load(consolCosts[1]);
				AssertNotNull("Calculation logs added", logsWrapper);
				AssertEquals(100m, logsWrapper.Logs[0].BaseRate);

				plugin.Apportionments.ReleaseMutexes();
			}
		}

		public void TestAutoRateCost_MenuItemCreatesJobs()
		{
			TestObjectCreator.CreateFlatCalculatorCosting("AIR", "LSE", "AUSYD", "NZAKL", null, "FRT", 10000m);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
			var shipment1 = TestObjectCreator.CreateShipment("S00001", "AUSYD", "NZAKL", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S00002", "AUSYD", "NZAKL", consol);

			Factory.Save();

			using (var plugIn = new ApportionmentPlugin(consol))
			{
				plugIn.OnGUIShown(); // simulating form

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.MenuItemAutoRateCosts_Click_ForTestOnly(plugIn, new EventArgs());

				var cost = (JobConsolCost)plugIn.Apportionments.CostsCollection.Single();

				CombineAssertions("WHEN AutoRateCosts THEN should apportion JobConsolCost to 2 JobCharge i.e. one of each shipment ", () =>
				{
					AssertEquals("Total charge", 2, cost.ApportionmentCharges.Count);

					foreach (var charge in cost.ApportionmentCharges.Cast<JobCharge>())
					{
						AssertEquals("charge amount", 5000m, charge.JR_LocalCostAmt);
					}
				});

				shipment1.Job?.Dispose();
				shipment2.Job?.Dispose();
			}
		}

		public void TestCostAndSellAmountsWhenAutoratingCostsAndRevenue()
		{
			TestObjectCreator.CreateFlatCalculatorCosting("AIR", "LSE", "AUSYD", "NZAKL", null, "FRT", 200m);
			TestObjectCreator.CreateFlatCalculatorClientRate("AIR", "LSE", "AUSYD", "NZAKL", TestObjectCreator.LocalClient, "FRT", 500m);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUSYD", "NZAKL", consol);
			shipment.JS_INCO = "CIF";
			var job = TestObjectCreator.CreateJob(shipment, false);

			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;

			Factory.Save();

			using (var plugIn = new ApportionmentPlugin(consol))
			{
				var starter = new AutoRatingStarter(consol, new LoggerDecorator());
				starter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			}

			Factory.Save();

			var charges = ((Job)shipment.Job).Charges;

			AssertEquals("Number of job charges", 1, charges.Count);
			AssertEquals("Charge code on charge line", TestObjectCreator.FRT.AC_Code, charges[0].ChargeCode.AC_Code);
			AssertEquals("Cost amount on charge line", 200m, charges[0].JR_OSCostAmt);
			AssertEquals("Sale amount on charge line", 500m, charges[0].JR_OSSellAmt);
		}

		public void TestExchangeRateForApportionmentUseConsolExchangeRate()
		{
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.ForwardingConsol.Code, Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.ConsolExchangeRate);
			GlbCompany.CurrentCompany.Factory.Save();

			var usdExchangeRate = 0.916m;
			var gbpExchangeRate = 2.016m;
			var voyageRate = 2.816m;
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, usdExchangeRate, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.GBP, Constants.ExchangeRateTypes.Code.BuyRate, gbpExchangeRate, ZDateTime.Today, ZDateTime.Today);
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;

			consol.Transports[0].JW_Vessel = "QF";
			consol.Transports[0].JW_VoyageFlight = "1234";
			consol.Transports[0].JW_RL_NKLoadPort = "USLAX";
			consol.Transports[0].JW_RL_NKDiscPort = "AUSYD";
			consol.Transports[0].JW_ETA = ZDateTime.Now;
			consol.Transports[0].JW_ETD = ZDateTime.Now.AddDays(1);
			consol.Transports[0].JW_IsLinked = ZBool.True;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ActualWeight = 1m;
			shipment.JS_ActualVolume = 3m;
			shipment.JS_OH_DeliveryAgent = consignee.PK;
			shipment.ConsignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.ConsigneePK = consignee.PK;

			var rate = consol.Transports[0].Sailing.Voyage.ExRates.AddNew();
			rate.E8_GC = GlbCompany.CurrentCompany.PK;
			rate.E8_RX_NKExCurrency = "GBP";
			rate.E8_VoyageExchangeRate = voyageRate;

			Factory.Save();
			using (var plugIn = new ApportionmentPlugin(consol))
			using (var form = new ZForm(consol))
			{
				var cost1 = plugIn.Apportionments.CostsCollection.TryAddNew();
				AssertEquals("Default currency", cost1.E6_RX_NKCurrency, Core.Constants.CurrencyCodes.Australia);
				AssertEquals("Default exchange rate", cost1.E6_ExchangeRate, 1m);

				cost1.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				cost1.E6_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				AssertEquals("Exchange rate should be current currency exchange rate", cost1.E6_ExchangeRate, usdExchangeRate);
				cost1.E6_ExchangeRate = 1.016m;

				CreateAutoRatingCosting();
				var starter = new AutoRatingStarter((IBusiness)plugIn.Consol_ForTestOnly, new LoggerDecorator());
				starter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
				plugIn.Apportionments.CostsCollection.Load();

				JobConsolCost costToTest = GetCostFromCollectionByChargeCode("WAR", plugIn.Apportionments.CostsCollection);
				AssertNotNull("A cost with charge type WAR should exist", costToTest);
				AssertEquals("Same currency as freight cost currency", costToTest.Currency.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
				AssertEquals("Exchange rate should be taken from freight cost", costToTest.E6_ExchangeRate, 1.016m);
				costToTest = GetCostFromCollectionByChargeCode("FSC", plugIn.Apportionments.CostsCollection);
				AssertNotNull("A cost with charge type FSC should exist", costToTest);
				AssertEquals("Other currency than freight cost currency", costToTest.Currency.RX_Code, Core.Constants.CurrencyCodes.UnitedKingdom);
				AssertEquals("Exchange rate should be taken from voyage rate", costToTest.E6_ExchangeRate, voyageRate);
			}
		}

		JobConsolCost GetCostFromCollectionByChargeCode(ZString code, JobConsolCostCollection collection)
		{
			bool found = false;
			int i = -1;
			while (!found && i < collection.Count)
			{
				i++;
				found = collection[i].ChargeCode.AC_Code == code;
			}
			return found ? collection[i] : null;
		}

		void CreateAutoRatingCosting()
		{
			OrgHeader transportProvider = Factory.NewWithValidTestData<OrgHeader>();
			transportProvider.OH_FullName = "TransportProvider";
			transportProvider.MainAddress.OA_Address1 = "FakeStreet";
			transportProvider.MainAddress.OA_City = "Sydney";
			transportProvider.MainAddress.OA_State = "NSW";
			transportProvider.MainAddress.OA_PostCode = "2000";
			transportProvider.OH_RL_NKClosestPort = "AUSYD";
			transportProvider.OH_Code = "TESTCR";

			Costing testCosting = Factory.New<Costing>();

			ClientRate testRate = Factory.NewWithValidTestData<ClientRate>();
			testRate.TH_OH = transportProvider.PK;

			RateEntry entry = testCosting.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD");
			entry.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			RateLine rate1 = entry.AddRateLine("WAR");
			rate1.TL_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			rate1.TL_RateCalculator = FlatCalculator.Code;
			rate1.GetCalculator<FlatCalculator>().BaseRate = 20;

			RateLine rate2 = entry.AddRateLine("FSC");
			rate2.TL_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			rate2.TL_RateCalculator = FlatCalculator.Code;
			rate2.GetCalculator<FlatCalculator>().BaseRate = 30;

			Factory.Save();
		}

		public void TestJobRefNum()
		{
			GlbCompany differentCompany = TestObjectCreator.CreateNewCompany("ABC");
			GlbBranch differentBranch = TestObjectCreator.CreateNewBranch(differentCompany, "AB1");
			OrgHeader differentCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYC", true, true);
			OrgHeader differentBranchOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYB", true, true);
			GlbBranch originalBranch = GlbBranch.CurrentBranch;
			GlbDepartment originalDepartment = GlbDepartment.CurrentDepartment;
			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
			differentCompany.GC_OH_OrgProxy = differentCompanyOrgProxy.PK;
			Factory.Save();

			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "HKHKG", "C0000558");
			consol.JK_UniqueConsignRef = "C0000558";

			ForwardingShipment consolShipment = consol.Shipments.AddNew();
			consolShipment.JS_UniqueConsignRef = "S0000558";

			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_ParentID = consolShipment.PK;
			job.JH_ParentTableCode = "JS";

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				ARInvoice aRConsolInvoiceSource1 = CreateInvoice<ARInvoice>(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testARConsol1", ZGuid.Empty, ZString.Empty, -10);
				ARInvoiceLine aRLine = (ARInvoiceLine)aRConsolInvoiceSource1.Lines[0];
				aRLine.AL_JH = job.PK;
				aRLine.AL_AC = TestObjectCreator.CC1.PK;
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = aRLine.AL_LineAmount = -10m;
				aRConsolInvoiceSource1.AH_JH = ZGuid.Empty;
				aRConsolInvoiceSource1.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
				aRLine.AL_GB = aRConsolInvoiceSource1.AH_GB;
				TestObjectCreator.CreateCharge(aRLine);
				Factory.Save();

				ARInvoice aRConsolInvoiceSource2 = CreateInvoice<ARInvoice>(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testARConsol2", ZGuid.Empty, ZString.Empty, -10);
				aRLine = (ARInvoiceLine)aRConsolInvoiceSource2.Lines[0];
				aRLine.AL_JH = job.PK;
				aRLine.AL_AC = TestObjectCreator.CC1.PK;
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = aRLine.AL_LineAmount = -10m;
				aRConsolInvoiceSource2.AH_JH = ZGuid.Empty;
				aRConsolInvoiceSource2.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef + "/A";
				aRLine.AL_GB = aRConsolInvoiceSource2.AH_GB;
				TestObjectCreator.CreateCharge(aRLine);
				Factory.Save();

				ARInvoice aRConsolInvoiceSource3 = CreateInvoice<ARInvoice>(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testARConsol3", ZGuid.Empty, ZString.Empty, -10);
				aRLine = (ARInvoiceLine)aRConsolInvoiceSource3.Lines[0];
				aRLine.AL_JH = job.PK;
				aRLine.AL_AC = TestObjectCreator.CC1.PK;
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = aRLine.AL_LineAmount = -10m;
				aRConsolInvoiceSource3.AH_JH = ZGuid.Empty;
				aRConsolInvoiceSource3.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef + "1";
				aRLine.AL_GB = aRConsolInvoiceSource3.AH_GB;
				TestObjectCreator.CreateCharge(aRLine);
				Factory.Save();

				ARInvoice aRShipmentInvoiceSource = CreateInvoice<ARInvoice>(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testARShipment", ZGuid.Empty, ZString.Empty, -10);
				aRLine = (ARInvoiceLine)aRShipmentInvoiceSource.Lines[0];
				aRLine.AL_JH = job.PK;
				aRLine.AL_AC = TestObjectCreator.CC1.PK;
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = aRLine.AL_LineAmount = -10m;
				aRShipmentInvoiceSource.AH_JH = job.PK;
				aRShipmentInvoiceSource.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
				aRLine.AL_GB = aRShipmentInvoiceSource.AH_GB;
				TestObjectCreator.CreateCharge(aRLine);
				Factory.Save();
			}

			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
			{
				plugIn.MenuItemImportAPInvoices_Click_ForTestOnly(plugIn, new EventArgs());

				UnapprovedTransactionConverter converter = (UnapprovedTransactionConverter)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				AssertEquals("Only 2 Candidates (AR Invoices) exist", 2, converter.Candidates.Count);
			}
		}

		T CreateInvoice<T>(GlbBranch branch, OrgHeader org, ZBool isPostedInternal, ZString invoiceNum, ZGuid transactionGroup, ZString transactionReference, ZDecimal amount) where T : InvoicingBase
		{
			T result = Factory.New<T>();
			result.AH_GB = branch.PK;
			result.AH_Ledger = typeof(T).Name.Substring(0, 2);
			result.AH_PostedInternal = isPostedInternal;
			result.AH_OH = org.PK;
			result.AH_TransactionBelongsToGroup = transactionGroup;
			result.AH_TransactionReference = transactionReference;
			result.AH_PostDate = ZDateTime.Now;

			InvoicingLineBase line = (InvoicingLineBase)result.Lines.AddNew();
			line.AL_LineAmount = line.AL_OSAmount = amount;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			result.AH_TransactionNum = invoiceNum;
			result.IsManuallySetTransactionNumber_ForTestOnly = true;
			Factory.Save();

			return result;
		}

		public void TestMenuItemAutoRatingAndRevenueSecurity()
		{
			Env.Security.MaintainConsolJobInvoicingAutoRate.IsAllowed = false;

			var consol = CreateConsol();
			var shipment1 = CreateShipment(consol);
			var shipment2 = CreateShipment(consol);

			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
			{
				plugIn.MenuItemAutoRateCostsAndRevenue_Click_ForTestOnly(plugIn, new EventArgs());
				AssertEquals(plugIn.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.ConsolInvAutoRate), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMenuItemAutoRatingeAndRevenueSecurityForGatewayConsol()
		{
			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsController = false;

			Factory.Save();

			var security = new SecurityCore(null, staff, Guid.Empty, Guid.Empty, Guid.Empty);
			using (Env.SetTemporarySecurityInstanceForTest(security))
			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(gatewayConsol))
			{
				Env.Security.MaintainConsolJobInvoicingAutoRate.IsAllowed = true;
				plugIn.MenuItemAutoRateCostsAndRevenue_Click_ForTestOnly(plugIn, new EventArgs());
				AssertContains("Costing/Invoicing", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains("No costs were found.", UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.MaintainConsolJobInvoicing.IsAllowed = true;
				plugIn.MenuItemAutoRateCostsAndRevenue_Click_ForTestOnly(plugIn, new EventArgs());
				AssertNotContains("Costing/Invoicing", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("No costs were found.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMenuItemSecurityForGatewayConsol()
		{
			var newOrgProxy = TestObjectCreator.CreateOrgHeader("TSTORG", false, false);
			var newCompany = TestObjectCreator.CreateNewCompany("ZZZ", orgProxy: newOrgProxy);
			var newBranch = TestObjectCreator.CreateBranch("ZZZ", "Branch ZZZ", newCompany);
			var newBranch1 = TestObjectCreator.CreateBranch("ZZ1", "Branch ZZ1", newCompany, newOrgProxy);

			using (newBranch.SetAsTemporaryContext())
			{
				var gatewayConsol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
				Job job = TestObjectCreator.CreateJob(gatewayConsol);
				var shipment1 = CreateShipment(gatewayConsol);
				var shipment2 = CreateShipment(gatewayConsol);
				Factory.Save();
				Assert(gatewayConsol.IsGatewayBillingEnabled());

				AssertMenuSecurityDependsOnSpecificCheckPoint(gatewayConsol, Env.Security.ApportionRevToShipment, SecurityCore.ApportionRevenueToShipment, Core.Constants.MenuNameConstants.ApportionRevenueToShipments);
				AssertMenuSecurityDependsOnSpecificCheckPoint(gatewayConsol, Env.Security.MaintainConsolJobInvoicingPreviewInvoices, Core.Constants.MenuNameConstants.PreviewInvoices, SecurityCore.ConsolInvPreviewInvoices);
				AssertMenuSecurityDependsOnSpecificCheckPoint(gatewayConsol, Env.Security.MaintainConsolJobInvoicingPreviewInvoices, Core.Constants.MenuNameConstants.PreviewCosts, SecurityCore.ConsolInvPreviewInvoices);
				AssertMenuSecurityDependsOnSpecificCheckPoint(gatewayConsol, Env.Security.MaintainConsolJobInvoicingAutoRate, SecurityCore.ConsolInvAutoRate, "Autorate Costs");
				AssertMenuSecurityDependsOnSpecificCheckPoint(gatewayConsol, Env.Security.MaintainConsolJobInvoicingAutoRate, SecurityCore.ConsolInvAutoRate, "Autorate Costs and Revenue");
				AssertMenuSecurityDependsOnSpecificCheckPoint(gatewayConsol, Env.Security.MaintainConsolJobInvoicingPostGateway, SecurityCore.ConsolInvPostGateway, Constants.MenuNameConstants.PostGatewayAgentCharges);
				AssertMenuSecurityDependsOnSpecificCheckPoint(gatewayConsol, Env.Security.MaintainConsolJobInvoicingPostAgent, SecurityCore.ConsolInvPostAgent, Core.Constants.MenuNameConstants.PostOverseasAgentCharges);
				AssertMenuSecurityDependsOnSpecificCheckPoint(gatewayConsol, Env.Security.MaintainConsolJobInvoicingPostCosts, SecurityCore.ConsolInvPostAllCosts, Core.Constants.MenuNameConstants.PostAllCosts);
				AssertMenuSecurityDependsOnSpecificCheckPoint(gatewayConsol, Env.Security.MaintainConsolJobInvoicingPostCosts, SecurityCore.ConsolInvPostAllCosts, Core.Constants.MenuNameConstants.PostConsolCostsOnly);
				AssertMenuSecurityDependsOnSpecificCheckPoint(gatewayConsol, Env.Security.MaintainConsolJobInvoicingPostWholeConsol, SecurityCore.ConsolInvPostWholeConsol, Core.Constants.MenuNameConstants.PostWholeConsol);
			}
		}

		void AssertMenuSecurityDependsOnSpecificCheckPoint(ForwardingConsol gatewayConsol, SecurityCheckpoint menuSpecificCheckPoint, string specificCheckPointSecurityText, string menuName)
		{
			AssertSecurityErrorText(gatewayConsol, menuSpecificCheckPoint, specificCheckPointSecurityText, menuName, true, true, false);
			AssertSecurityErrorText(gatewayConsol, menuSpecificCheckPoint, specificCheckPointSecurityText, menuName, true, false, false);
			AssertSecurityErrorText(gatewayConsol, menuSpecificCheckPoint, specificCheckPointSecurityText, menuName, false, true, true);
			AssertSecurityErrorText(gatewayConsol, menuSpecificCheckPoint, specificCheckPointSecurityText, menuName, false, false, true);
		}

		void AssertSecurityErrorText(ForwardingConsol gatewayConsol, SecurityCheckpoint menuSpecificCheckPoint, string specificCheckPointSecurityText, string menuName, bool isAllowSpecificCheckPoint, bool isAllowGatewayConsolJobInvoicing, bool expectSecurityError)
		{
			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(gatewayConsol))
			{
				menuSpecificCheckPoint.IsAllowed = isAllowSpecificCheckPoint;
				Env.Security.GatewayConsolJobInvoicing.IsAllowed = isAllowGatewayConsolJobInvoicing;
				UnitTestUserNotification.Instance.ClearMessages();

				((IPluginShouldRefreshMenuForGateway)plugIn).RefreshGatewayElements(true);
				var menu = plugIn.GetNewTopLevelMenu_ForTestOnly().MenuItems.FindByText(menuName);
				if (menu != null && menu.Visible)
				{
					menu.PerformClick();
					if (expectSecurityError)
					{
						AssertEquals(plugIn.MenuSecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(specificCheckPointSecurityText), UnitTestUserNotification.Instance.LastMessage.Text);
					}
					else
					{
						AssertNotEquals(plugIn.MenuSecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(specificCheckPointSecurityText), UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestMenuItemApportion()
		{
			var consol = CreateConsol();
			var shipment1 = CreateShipment(consol);
			var shipment2 = CreateShipment(consol);
			Factory.Save();

			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
			{
				try
				{
					AssertEquals("Apportion Revenue To Shipments", plugIn.GetNewTopLevelMenu_ForTestOnly().MenuItems[5].Text);
					MenuItem menu = plugIn.GetNewTopLevelMenu_ForTestOnly().MenuItems[5];
					AssertNotNull(menu);

					Env.Security.ApportionRevToShipment.IsAllowed = false;
					menu.PerformClick();
					AssertEquals(plugIn.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.ApportionRevenueToShipment), UnitTestUserNotification.Instance.LastMessage.Text);

					Env.Security.ApportionRevToShipment.IsAllowed = true;
					menu.PerformClick();
					AssertEquals(typeof(ConsolRevenueApportionForm).ToString(), ZFormModaliser.LastFormShownDialogForTest.GetType().ToString());
				}
				finally
				{
					foreach (Control ctr in ZFormModaliser.LastFormShownDialogForTest.Controls)
					{
						ctr.Dispose();
					}
					ZFormModaliser.LastFormShownDialogForTest.Dispose();
				}
			}
		}

		public void TestMenuItemApportion_NewCreatedConsolShouldBeSavedBeforeShowingApportionForm()
		{
			var consol = Factory.New<ForwardingConsol>();

			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
			{
				var apportionRevenueToShipmentsMenuItem = plugIn.GetNewTopLevelMenu_ForTestOnly().MenuItems.FindByText(Constants.MenuNameConstants.ApportionRevenueToShipments);
				apportionRevenueToShipmentsMenuItem.PerformClick();
				AssertEquals("Please save before apportioning.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestMenuItemApportion_EnableCustomBranchDefaultingRulesEngineConfiguration_WithNoJobHeader()
		{
			var consol = CreateConsol();
			CreateShipment(consol);
			CreateShipment(consol);
			Factory.Save();

			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
			{
				AccountingConfigurationRegistry.Instance.CustomBranchDefaultingRulesEngineConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var apportionRevenueToShipmentsMenuItem = plugIn.GetNewTopLevelMenu_ForTestOnly().MenuItems.FindByText(Constants.MenuNameConstants.ApportionRevenueToShipments);
				apportionRevenueToShipmentsMenuItem.PerformClick();
				var consolRevenueMaster = ZFormModaliser.LastIBusinessShownOnDialogForTest as ConsolRevenueMaster;
				var revenue = consolRevenueMaster.Revenues.AddNew();
				AssertEquals("should has no exception and correct result", 2, revenue.SplitCharges.Count);
				consolRevenueMaster.ReleaseMutexes();
			}
		}

		public void TestMenuItemPostConsolCostOnly()
		{
			var consol = CreateConsol();
			var shipment1 = CreateShipment(consol);
			var shipment2 = CreateShipment(consol);

			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
			{
				AssertEquals("Post Consol Costs Only", plugIn.GetNewTopLevelMenu_ForTestOnly().MenuItems[3].Text);
				MenuItem menu = plugIn.GetNewTopLevelMenu_ForTestOnly().MenuItems[3];
				AssertNotNull(menu);
				menu.PerformClick();
				AssertEquals("Please save before posting", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConsolMenuItemSecurity()
		{
			Env.Security.MaintainConsolJobInvoicingPostWholeConsol.IsAllowed = false;

			var consol = CreateConsol();
			var shipment1 = CreateShipment(consol);
			var shipment2 = CreateShipment(consol);

			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
			{
				plugIn.MenuItemPostConsol_Click_ForTestOnly(plugIn, new EventArgs());
				AssertEquals(plugIn.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.ConsolInvPostWholeConsol), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMustSaveBeforePosting()
		{
			var consol = CreateConsol();
			Factory.Save();

			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
			using (Control control = plugIn.UserControl)
			{
				consol.HasChanges = true;
				plugIn.PostTransactions_ForTestOnly(JobInvoicingPostingOption.All);
				AssertEquals("Please save before posting", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRunPrePostValidation()
		{
			var consol = CreateConsol();
			Factory.Save();

			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
			using (Control control = plugIn.UserControl)
			using (ZForm form = new ZForm(consol))
			{
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(control, "");
				form.Controls.Add(control);
				form.Show();
				plugIn.PostTransactions_ForTestOnly(JobInvoicingPostingOption.All);

				AssertEquals(AccountingPeriodCalculator.GetInvalidPeriodValidationError(ZDateTime.Today), UnitTestUserNotification.Instance.LastMessage.Text);
				AccountingPeriodTestHelper periodManagementTestHelper = new AccountingPeriodTestHelper();
				periodManagementTestHelper.SetupPeriods();

				plugIn.PostTransactions_ForTestOnly(JobInvoicingPostingOption.All);
				AssertEquals("You cannot post because no Job Invoices have been created.", UnitTestUserNotification.Instance.LastMessage.Text);

				ForwardingShipment shipment = consol.Shipments.AddNew();
				shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
				Job shipmentJob = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
				shipmentJob.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;

				shipment = consol.Shipments.AddNew();
				shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
				shipmentJob = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
				shipmentJob.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
				Factory.Save();

				JobConsolCost cost = plugIn.Apportionments.CostsCollection.AddNew();
				cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
				cost.E6_OSCostAmount = 100m;
				cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
				cost.E6_InvoiceNum = "ABC123";
				cost.E6_InvoiceDate = ZDateTime.Now;

				Factory.Save();

				foreach (ApportionSplitCharge charge in cost.ApportionmentCharges)
				{
					charge.JR_OH_SellAccount = ZGuid.Empty;
				}
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				plugIn.PostTransactions_ForTestOnly(JobInvoicingPostingOption.All);
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

				cost = plugIn.Apportionments.CostsCollection.AddNew();
				cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
				cost.E6_OSCostAmount = 100m;
				cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
				cost.E6_InvoiceNum = "ABC123";
				cost.E6_InvoiceDate = ZDateTime.Now;
				Assert(!cost.IsPosted);
				TestObjectCreator.ABIGAS.OH_IsDebtor = false;
				foreach (ApportionSplitCharge charge in cost.ApportionmentCharges)
				{
					charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
					charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				}
				Factory.Save();

				Assert(!TestObjectCreator.ABIGAS.OH_IsDebtor);

				plugIn.PostTransactions_ForTestOnly(JobInvoicingPostingOption.All);

				string actualMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				bool expectedMessageShown = "Job S00001000 has error: Invalid Debtor: Enter a valid Debtor." == actualMessage;
				expectedMessageShown = expectedMessageShown || "Job S00001001 has error: Invalid Debtor: Enter a valid Debtor." == actualMessage;

				Assert("Cannot control order of validation in post manger validation so expect message for either job S0001000 or S0001001", expectedMessageShown);
				AssertEquals("charge.Debtors.Load must not be called", 0, shipmentJob.Charges[0].Debtors.Count);
			}
		}

		public void TestRunPrePostValidationForInvoiceType()
		{
			var consol = CreateConsol();
			Factory.Save();

			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
			using (Control control = plugIn.UserControl)
			using (ZForm form = new ZForm(consol))
			{
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(control, "");
				form.Controls.Add(control);
				form.Show();
				plugIn.PostTransactions_ForTestOnly(JobInvoicingPostingOption.All);

				AssertEquals(AccountingPeriodCalculator.GetInvalidPeriodValidationError(ZDateTime.Today), UnitTestUserNotification.Instance.LastMessage.Text);
				AccountingPeriodTestHelper periodManagementTestHelper = new AccountingPeriodTestHelper();
				periodManagementTestHelper.SetupPeriods();

				plugIn.PostTransactions_ForTestOnly(JobInvoicingPostingOption.All);
				AssertEquals("You cannot post because no Job Invoices have been created.", UnitTestUserNotification.Instance.LastMessage.Text);

				ForwardingShipment shipment = consol.Shipments.AddNew();
				shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
				Job shipmentJob = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
				shipmentJob.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
				Factory.Save();

				JobConsolCost cost = plugIn.Apportionments.CostsCollection.AddNew();
				cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				cost.E6_OSCostAmount = 100m;
				cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
				cost.E6_InvoiceNum = "ABC123";
				cost.E6_InvoiceDate = ZDateTime.Now;

				Factory.Save();

				foreach (Charge charge in shipmentJob.Charges)
				{
					charge.JR_OH_SellAccount = ZGuid.Empty;
				}

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				plugIn.PostTransactions_ForTestOnly(JobInvoicingPostingOption.All);
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

				foreach (Charge charge in shipmentJob.Charges)
				{
					charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
					charge.JR_InvoiceType = ""; // simulate a problem with defaulting invoice type
				}

				cost = plugIn.Apportionments.CostsCollection.AddNew();
				cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				cost.E6_OSCostAmount = 100m;
				cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
				cost.E6_InvoiceNum = "ABC123";
				cost.E6_InvoiceDate = ZDateTime.Now;
				Assert(!cost.IsPosted);

				Factory.Save();
				TestObjectCreator.ABIGAS.OH_IsDebtor = true;
				Factory.Save();

				plugIn.PostTransactions_ForTestOnly(JobInvoicingPostingOption.All);
				AssertEquals("Job S00001000 has error: Invalid invoice type: Please enter an Invoice Type.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("charge.Debtors.Load must not be called", 0, shipmentJob.Charges[0].Debtors.Count);
			}
		}

		public void TestIsActivated()
		{
			var consol = CreateConsol();
			var shipment = CreateShipment(consol);
			Factory.Save();

			AssertNull(Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK)));

			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
			using (Control control = plugIn.UserControl)
			{
				Assert(!plugIn.Apportionments.IsActivated);
				plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly();
				Job shipmentJob = new Job.Loader(shipment).Load();
				AssertNotNull(shipmentJob);
				Assert(!shipmentJob.IsInDatabase);
				Assert(plugIn.Apportionments.IsActivated);
			}

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			try
			{
				using (Job shipmentJobInNewFactory = new Job.Loader(newFactory, shipment).TryCreateWithMutex())
				{
					AssertNotNull("Mutex was not released on disposal of plugin", shipmentJobInNewFactory);
				}
			}
			catch (JobCreationException)
			{
				Fail("Mutex was not released on disposal of plugin");
			}
		}

		public void TestPlugQueryDoesTriggerValidateOnSaveIfDisplayOK()
		{
			var consol = CreateConsol();
			var shipment = CreateShipment(consol);
			shipment.CreateShipmentJobHeaderWithMutex();
			Factory.Save();

			var apps = new ApportionmentListing(Factory, consol);
			var cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost.E6_OSCostAmount = 100m;
			cost.RunPreSaveValidation();

			Factory.Save();
			Assert("Precondition - cost validation should not be suspended", !cost.IsValidationSuspended);

			using (var plugIn = new ApportionmentPlugin(consol))
			{
				bool isPlugInShouldShown = plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly();
				Assert("The PlugIn should be shown", isPlugInShouldShown);
				Assert("cost validation should not be suspended", !cost.IsValidationSuspended);
			}
		}

		public void TestPlugQueryDoesNotTriggerValidateOnSaveIfDisplayNotOK()
		{
			var foreignCompany = Factory.NewWithValidTestData<GlbCompany>();
			var foreignBranch = Factory.NewWithValidTestData<GlbBranch>();
			foreignBranch.GB_GC = foreignCompany.PK;
			var foreighDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001985";
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001985";
			consol.Shipments.Add(shipment);

			var apps = new ApportionmentListing(Factory, consol);
			var cost1 = apps.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost1.E6_OSCostAmount = 100m;
			cost1.RunPreSaveValidation();

			var cost2 = apps.CostsCollection.TryAddNew();
			cost2.E6_AC_ChargeCode = TestObjectCreator.CC2.PK;
			cost2.E6_OSCostAmount = 200m;
			cost2.E6_GC = foreignCompany.PK;

			var cost3 = apps.CostsCollection.TryAddNew();
			cost3.E6_AC_ChargeCode = TestObjectCreator.CC3.PK;
			cost3.E6_OSCostAmount = 300m;
			cost3.E6_GC = foreignCompany.PK;

			Factory.Save();

			//Creating shipment job with Mutex but without saving it
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, foreignBranch.PK.ToGuid(), foreighDepartment.PK.ToGuid()))
			using (JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>())
			{
				job.JH_ParentID = shipment.PK;
				job.JH_JobNum = shipment.JS_UniqueConsignRef;
				var mutex = JobHeader.GetMutex_ForTestOnly(shipment.PK);
				try
				{
					mutex.Lock();
					var consolLoadingFactory = new BusinessObjectFactory();
					var loadedConsol = consolLoadingFactory.Load<ForwardingConsol>(consol.PK);
					var reloadedCost2 = consolLoadingFactory.Load<JobConsolCost>(cost2.PK);
					var reloadedCost3 = consolLoadingFactory.Load<JobConsolCost>(cost3.PK);
					Assert("Precondition - cost validation should not be suspended", !reloadedCost2.IsValidationSuspended);
					Assert("Precondition - cost validation should not be suspended", !reloadedCost3.IsValidationSuspended);

					using (var plugIn = new ApportionmentPlugin(loadedConsol))
					{
						bool isPlugInShouldShown = plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly();
						Assert("The PlugIn shouldn't be shown due to Job Mutex problem", !isPlugInShouldShown);
						AssertEquals("The PlugInNotDisplayedMessage should say about mutex problem",
@"You have created the job S00001985 on another form, but haven't saved it yet.
Please close or save other forms that use job S00001985 to continue.",
							plugIn.PlugInNotDisplayedMessage);
						Assert("cost validation should be suspended", reloadedCost2.IsValidationSuspended);
						Assert("cost validation should be suspended", reloadedCost3.IsValidationSuspended);

						//Unlocking shipment job mutex
						mutex.Unlock();

						isPlugInShouldShown = plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly();
						Assert("Mutex is unlocked, The PlugIn should be shown", isPlugInShouldShown);
						Assert("cost validation is not suspended", !reloadedCost2.IsValidationSuspended);
						Assert("cost validation is not suspended", !reloadedCost3.IsValidationSuspended);
					}
				}
				finally
				{
					if (mutex.HasLock)
					{
						mutex.Unlock();
					}
				}
			}
		}

		public void TestInactiveShipment()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.CreateShipmentJobHeaderWithMutex();
			Factory.Save();
			consol.JK_UniqueConsignRef = "C001";
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();

			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost.E6_OSCostAmount = 100m;
			Factory.Save();
			ApportionSplitCharge charge = cost.ApportionmentCharges.ToArray<ApportionSplitCharge>().FirstOrDefault();
			AssertNotNull("Precondition: charge", charge);
			AssertNotNull("Precondition: charge.InvoicingJob", charge.InvoicingJob);
			charge.JR_IsUsedForApportionment = true;
			Factory.Save();

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			var shipmentInOtherFactory = otherFactory.Load<ForwardingShipment>(shipment.PK);
			shipmentInOtherFactory.JS_IsCancelled = true;
			otherFactory.Save();

			var newFactory = new BusinessObjectFactory();
			var consolInNewFactory = newFactory.Load<ForwardingConsol>(consol.PK);
			var appsInNewFactory = new ApportionmentListing(newFactory, consolInNewFactory);
			var costInNewFactory = appsInNewFactory.CostsCollection[0];

			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consolInNewFactory))
			using (Control control = plugIn.UserControl)
			{
				costInNewFactory.RunPreSaveValidation();
				var chargeToTest = costInNewFactory.ApportionmentCharges.FindChargeForJob(shipment);
				AssertHasError("JR_IsUsedForApportionment should have error",
								 chargeToTest.JR_IsUsedForApportionmentInfo,
								 "This apportioned charge belongs to job: " + chargeToTest.JR_JobNumber + " which is no longer attached to the consol or is inactive. Please untick Is Used or re-attach job to consol/activate job.");
			}
		}

		public virtual void TestApportionmentsWithJobNumbersWithoutShipmentsOnConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00009999";
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00009999";
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S00009998";
			Factory.Save();

			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
			using (Control control = plugIn.UserControl)
			using (ZForm form = new ZForm(consol))
			{
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(control, "");
				form.Controls.Add(control);
				form.Show();
				JobConsolCost cost = plugIn.Apportionments.CostsCollection.AddNew();
				cost.E6_OSCostAmount = 100m;
				cost.E6_ApportionmentMethod = "SHP";
				cost.E6_PPDCLT = "ALL";

				ApportionSplitCharge charge = cost.ApportionmentCharges.AddNew();
				ApportionSplitCharge charge2 = cost.ApportionmentCharges.AddNew();
				Job job = Factory.NewJobForTesting<Job>();
				Job job2 = Factory.NewJobForTesting<Job>();

				job.JH_GC = GlbCompany.CurrentCompany.PK;
				job.JH_ParentTableCode = "JS";
				job.JH_ParentID = shipment.PK;
				job.JH_JobNum = shipment.JS_UniqueConsignRef;
				charge.JR_E6 = cost.PK;

				job2.JH_GC = GlbCompany.CurrentCompany.PK;
				job2.JH_ParentTableCode = "JS";
				job2.JH_ParentID = shipment2.PK;
				job2.JH_JobNum = shipment2.JS_UniqueConsignRef;
				charge2.JR_E6 = cost.PK;

				cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				cost.E6_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				charge.JR_GB = GlbBranch.CurrentBranch.PK;
				charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
				charge.JR_JH = job.PK;

				charge2.JR_GB = GlbBranch.CurrentBranch.PK;
				charge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
				charge2.JR_JH = job2.PK;

				cost.E6_InvoiceNum = "ABC123";
				cost.E6_InvoiceDate = ZDateTime.Now;

				plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly();

				ZBool coveringLabelFound = false;

				foreach (Control ctrl in plugIn.TabPage.Controls)
				{
					if (ctrl is ZLabel && ctrl.Dock == DockStyle.Fill)
					{
						if (ctrl.Text == ApportionmentPlugin.LabelMessage +
											shipment.JS_UniqueConsignRef +
											System.Environment.NewLine +
											shipment2.JS_UniqueConsignRef)
						{
							coveringLabelFound = true;
						}
					}
				}
				AssertEquals("Covering Label is not shown", false, coveringLabelFound);
			}
		}

		public void TestApportionmentsEventHandler()
		{
			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(Factory.New<ForwardingConsol>()))
			{
				ReplaceShipmentExchangeRateEventArgs eventArgs = new ReplaceShipmentExchangeRateEventArgs();
				Assert("Does not replace shipment ExchangeRate by default.", !eventArgs.DoesReplaceShipmentExchangeRate);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.Apportionments.RaiseOnReplaceShipmentExchangeRate(this, eventArgs);
				Assert("Does replace shipment ExchangeRate.", eventArgs.DoesReplaceShipmentExchangeRate);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				plugIn.Apportionments.RaiseOnReplaceShipmentExchangeRate(this, eventArgs);
				Assert("Does not replace shipment ExchangeRate.", !eventArgs.DoesReplaceShipmentExchangeRate);
			}
		}

		public void TestShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001985";

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00000001";
			JobHeader.Loader loader1 = new JobHeader.Loader(Factory, shipment1);
			JobHeader job1 = loader1.TryCreateWithMutex();
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save(); //This will drop mutex on job1

			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S00000002";

			ForwardingShipment shipment3 = consol.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "S00000003";
			JobHeader.Loader loader3 = new JobHeader.Loader(Factory, shipment3);

			//Job created and Mutex set in current factory
			using (JobHeader job3 = loader3.TryCreateWithMutex())
			{
				ForwardingShipment shipment4 = consol.Shipments.AddNew();
				shipment4.JS_UniqueConsignRef = "S00000004";
				JobHeader.Loader loader4 = new JobHeader.Loader(new BusinessObjectFactory(), shipment4);
				//Job created and Mutex set in another factory -> mutex conflict -> mutex exception
				JobHeader job4 = loader4.TryCreateWithMutex();

				using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
				{
					AssertEquals("Cannot bind to plug in due to mutex conflict, which is set in another Factory and thats why we cannot create or load Job", false, plugIn.ShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly());
					job4.Dispose(); // should drop mutex locks
					AssertEquals("ShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly always must be false ", false, plugIn.ShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly());
				}
			}
		}

		public void TestControlsHiding()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();

			AssertEquals("Company should have no branch in Quebec tax zone", false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada);
			using (ApportionmentPluginForTest plugIn = new ApportionmentPluginForTest(consol))
			using (NewApportionmentUserControlForTest control = (NewApportionmentUserControlForTest)plugIn.UserControl)
			using (ZForm form = new ZForm(consol))
			{
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(control, "");
				form.Controls.Add(control);
				form.Show();

				AssertEquals(false, control.ExtraTaxPanel_Exposed.Visible);
				AssertEquals(true, control.CostSummaryGrid_Exposed.GetColumnStyle(JobConsolCost.Schema.E6_OSExtraTaxAmount).IsUnavailable);
			}

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
			Factory.Save();
			AssertEquals("Company should be in Canada ", true, GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada);
			using (ApportionmentPluginForTest plugIn = new ApportionmentPluginForTest(consol))
			using (NewApportionmentUserControlForTest control = (NewApportionmentUserControlForTest)plugIn.UserControl)
			using (ZForm form = new ZForm(consol))
			{
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(control, "");
				form.Controls.Add(control);
				form.Show();

				AssertEquals(true, control.ExtraTaxPanel_Exposed.Visible);
				AssertEquals(false, control.CostSummaryGrid_Exposed.GetColumnStyle(JobConsolCost.Schema.E6_OSExtraTaxAmount).IsUnavailable);
			}
		}

		public void TestPlugInNotDisplayedMessageIfJobMutexProblem()
		{
			BusinessObjectFactory factoryForConsolShipmentWithMutexJob = new BusinessObjectFactory();
			ForwardingShipment shipment = factoryForConsolShipmentWithMutexJob.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001985";
			ForwardingConsol consol = factoryForConsolShipmentWithMutexJob.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001985";
			consol.Shipments.Add(shipment);
			factoryForConsolShipmentWithMutexJob.Save();

			//Creating shipment job with Mutex but without saving it
			using (JobHeader job = factoryForConsolShipmentWithMutexJob.NewJobWithValidTestDataForTesting<JobHeader>())
			{
				job.JH_ParentID = shipment.PK;
				job.JH_JobNum = shipment.JS_UniqueConsignRef;
				ZGlobalMutex mutex = JobHeader.GetMutex_ForTestOnly(shipment.PK);
				try
				{
					mutex.Lock();

					BusinessObjectFactory consolLoadingFactory = new BusinessObjectFactory();
					ForwardingConsol loadedConsol = consolLoadingFactory.Load<ForwardingConsol>(consol.PK);

					using (ApportionmentPlugin plugIn = new ApportionmentPlugin(loadedConsol))
					{
						bool isPlugInShoulShown = plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly();
						Assert("The PlugIn shouldn't be shown due to Job Mutex probem", !isPlugInShoulShown);

						AssertEquals("The PlugInNotDisplayedMessage should say about mutex problem",
@"You have created the job S00001985 on another form, but haven't saved it yet.
Please close or save other forms that use job S00001985 to continue.",
							plugIn.PlugInNotDisplayedMessage);
					}
				}
				finally
				{
					if (mutex.HasLock)
					{
						mutex.Unlock();
					}
				}
			}
		}

		public void AssertMenuItemPreview(string menuName, int menuIndex)
		{
			var expectedErrorMessage = "Please save this form before previewing " + (menuName == Constants.MenuNameConstants.PreviewInvoices ? "Invoices." : "Cost Confirmation Documents.");

			new AccountingPeriodTestHelper().SetupPeriods();
			var consol = TestObjectCreator.CreateConsol(saveIt: false);

			using (var form = new ZForm(consol))
			{
				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ZArchitecture.Modules.ControllerIDs.Apportionment);
				form.Show();

				using (var plugIn = (ApportionmentPlugin)form.PlugIns.Instances[0])
				using (var menu = plugIn.GetNewTopLevelMenu_ForTestOnly().MenuItems[menuIndex])
				{
					AssertNotNull(menu);
					AssertEquals(menuName, menu.Text);
					Assert("Consol_ForTestOnly is not in DB", !plugIn.Consol_ForTestOnly.CostSupporter.IsInDatabase);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.PerformClick();
					AssertEquals("Should not preview, when consol is not in DB", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

					Factory.Save();

					var shipment = TestObjectCreator.CreateShipment("S001", consol);
					var shipmentJob = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0m, TestObjectCreator.Agent, 0m);
					shipment = TestObjectCreator.CreateShipment("S002", consol);
					shipmentJob = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0m, TestObjectCreator.Agent, 0m);

					var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m, TestObjectCreator.AALSHI, AllocationMethod.Shipment, plugIn.Apportionments);
					cost.E6_InvoiceNum = "INVA1";
					cost.E6_InvoiceDate = ZDateTime.Now;

					Assert("Consol_ForTestOnly is in DB", plugIn.Consol_ForTestOnly.CostSupporter.IsInDatabase);
					Assert("Consol_ForTestOnly has changes", plugIn.Consol_ForTestOnly.CostSupporter.HasChanges);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.PerformClick();
					AssertEquals("Should not preview, when consol has changes", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

					Factory.Save();

					Assert("Consol_ForTestOnly is in DB", plugIn.Consol_ForTestOnly.CostSupporter.IsInDatabase);
					Assert("Consol_ForTestOnly has no changes", !plugIn.Consol_ForTestOnly.CostSupporter.HasChanges);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.PerformClick();
					AssertNull("Should preview because consol is in DB and has no changes now", UnitTestUserNotification.Instance.LastMessage.Text);
					using (var activeForm = ZFormModaliser.ActiveForm)
					{
						AssertNotNull(activeForm);
						AssertType<InvoicePreviewForm>(activeForm);
						AssertEquals(AllowedDeliveryOptions.PreviewOnly, ((InvoicePreviewForm)activeForm).DeliveryOptions);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Testing")]
		public void TestMenuItemPreviewInvoices()
		{
			AssertMenuItemPreview(Constants.MenuNameConstants.PreviewInvoices, 7);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Testing")]
		public void TestMenuItemPreviewCosts()
		{
			AssertMenuItemPreview(Constants.MenuNameConstants.PreviewCosts, 8);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Testing")]
		public void TestMenuItemImportAPInvoices()
		{
			var consol = CreateConsol();
			var shipment1 = CreateShipment(consol);
			var shipment2 = CreateShipment(consol);
			Factory.Save();

			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
			{
				try
				{
					AssertEquals(Constants.MenuNameConstants.ImportAPInvoicesIssuedByOtherGroupCompanies, plugIn.GetNewTopLevelMenu_ForTestOnly().MenuItems[10].Text);
					MenuItem menu = plugIn.GetNewTopLevelMenu_ForTestOnly().MenuItems[10];
					AssertNotNull(menu);
					menu.PerformClick();
					AssertEquals(typeof(UnapprovedTransactionAuthorisationForm).ToString(), ZFormModaliser.LastFormShownDialogForTest.GetType().ToString());
				}
				finally
				{
					foreach (Control ctr in ZFormModaliser.LastFormShownDialogForTest.Controls)
					{
						ctr.Dispose();
					}
					ZFormModaliser.LastFormShownDialogForTest.Dispose();
				}
			}
		}

		public void TestMenuItemPenaltyTaxInfo()
		{
			var expectedMenuItemTextForKoreaSouth = "Additional Tax Information";

			var consol = CreateConsol();
			CreateShipment(consol);
			CreateShipment(consol);
			Factory.Save();

			using (TestObjectCreator.SetUpForTestingEInvoicing(Constants.CountryCodes.KoreaSouth, true))
			using (var plugIn = new ApportionmentPlugin(consol))
			{
				var topLevelMenu = plugIn.GetNewTopLevelMenu_ForTestOnly();
				AssertNotNull(topLevelMenu.MenuItems.FindByText(expectedMenuItemTextForKoreaSouth));
			}

			using (TestObjectCreator.SetUpForTestingEInvoicing(Constants.CountryCodes.KoreaSouth, false))
			using (var plugIn = new ApportionmentPlugin(consol))
			{
				var topLevelMenu = plugIn.GetNewTopLevelMenu_ForTestOnly();
				AssertNull(topLevelMenu.MenuItems.FindByText(expectedMenuItemTextForKoreaSouth));
			}
		}

		public void TestMenuItemImportAPInvoicesFromGatewayConsolToForwardingConsol()
		{
			var gatewayCompany = TestObjectCreator.CreateNewCompany("ABC");
			var gatewayBranch = TestObjectCreator.CreateNewBranch(gatewayCompany, "AB1");
			var gatewayDepartment = TestObjectCreator.CreateDepartment("DEP");
			var gatewayCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYB", true, true);
			gatewayCompany.GC_OH_OrgProxy = gatewayCompanyOrgProxy.PK;
			gatewayBranch.GB_OH_OrgProxy = gatewayCompanyOrgProxy.PK;
			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: gatewayCompany);
			gatewayConsol.Shipments.AddNew();
			Factory.Save();

			var expectedgatewayJobPK = ZGuid.Empty;
			var forwardingBranchOrgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.OrgProxy.PK);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, gatewayBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
			{
				var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol);
				expectedgatewayJobPK = gatewayJob.PK;

				forwardingBranchOrgProxy.CompanyData.OB_IsDebtor = true;
				var arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1.0m, forwardingBranchOrgProxy);
				arInvoice.AH_OSExTaxAmount = 150m;
				arInvoice.AH_ConsolidatedInvoiceRef = gatewayConsol.JK_UniqueConsignRef;
				arInvoice.AH_JH = gatewayJob.PK;
				var aRInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, gatewayJob, TestObjectCreator.CC1, 150m, TestObjectCreator.AUD, 1.0m);
				TestObjectCreator.CreateCharge(aRInvoiceLine);
				Factory.Save();
			}

			using (var plugIn = new ApportionmentPlugin(gatewayConsol))
			{
				plugIn.MenuItemImportAPInvoices_Click_ForTestOnly(plugIn, new EventArgs());
				var converter = (UnapprovedTransactionConverter)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				AssertEquals("Only 1 Candidate (AR Invoices) exists", 1, converter.Candidates.Count);
				AssertEquals("The job of the imported invoice should be the gateway job", expectedgatewayJobPK, converter.Candidates[0].AH_JH);
			}
		}

		public void TestRedefaultJobBillingExchangeRateMenuItem()
		{
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Sea, preference: Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalDate);
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.ForwardingConsol.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Sea, preference: Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			GlbCompany.CurrentCompany.Factory.Save();

			var defaultRateForShipmentJob = 3.5m;
			var defaultRateForConsolCost = 0.896m;
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, defaultRateForShipmentJob, ZDateTime.Now.AddDays(-11), ZDateTime.Now.AddDays(-9));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, defaultRateForConsolCost, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));

			var consol = CreateConsol("", "");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.Transports.ArrivalTransport.JW_ATA = ZDateTime.Now.AddDays(-10);

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_RL_NKOrigin = "USLAX";
			shipment1.JS_RL_NKDestination = "AUMEL";
			consol.Shipments.Add(shipment1);
			var job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			job1.AddCurrency(TestObjectCreator.USD, 2M, ExchangeRateValidLedgerEnum.AR);

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = "SEA";
			shipment2.JS_RL_NKOrigin = "USLAX";
			shipment2.JS_RL_NKDestination = "AUMEL";
			consol.Shipments.Add(shipment2);
			var job2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			job2.AddCurrency(TestObjectCreator.USD, 3M, ExchangeRateValidLedgerEnum.AR);

			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1);
			consolCost1.E6_OSCostAmount = 100m;
			consolCost1.E6_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			consolCost1.E6_ExchangeRate = 1.345m;

			AssertEquals(2, consolCost1.ApportionmentCharges.Count);
			AssertEquals(1.345m, consolCost1.ApportionmentCharges[0].JR_OSCostExRate);
			AssertEquals(1.345m, consolCost1.ApportionmentCharges[1].JR_OSCostExRate);

			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2);
			consolCost2.E6_OSCostAmount = 200m;
			consolCost2.E6_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			consolCost2.E6_ExchangeRate = 2.873m;

			AssertEquals(2, consolCost2.ApportionmentCharges.Count);
			AssertEquals(2.873m, consolCost2.ApportionmentCharges[0].JR_OSCostExRate);
			AssertEquals(2.873m, consolCost2.ApportionmentCharges[1].JR_OSCostExRate);

			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			using (var plugIn = new ApportionmentPlugin(consol))
			{
				var menu = plugIn.GetNewTopLevelMenu_ForTestOnly().MenuItems[12];
				AssertNotNull(menu);
				AssertEquals("Re-default Job Billing Exchange Rate", menu.Text);

				AssertEquals("Precondition for job1:", 2M, job1.ExchangeRates[0].JF_BaseRate);
				AssertEquals("Precondition for job2: ", 3M, job2.ExchangeRates[0].JF_BaseRate);
				AssertEquals("Precondition for ConsolCost1:", 1.345m, consolCost1.E6_ExchangeRate);
				AssertEquals("Precondition for apportion charge linked to ConsolCost1:", 1.345m, consolCost1.ApportionmentCharges[0].JR_OSCostExRate);
				AssertEquals("Precondition for apportion charge linked to ConsolCost1:", 1.345m, consolCost1.ApportionmentCharges[1].JR_OSCostExRate);
				AssertEquals("Precondition for ConsolCost2:", 2.873m, consolCost2.E6_ExchangeRate);
				AssertEquals("Precondition for apportion charge linked to ConsolCost2:", 2.873m, consolCost2.ApportionmentCharges[0].JR_OSCostExRate);
				AssertEquals("Precondition for apportion charge linked to ConsolCost2:", 2.873m, consolCost2.ApportionmentCharges[1].JR_OSCostExRate);
				AssertEquals("Precondition: plugIn.HostBusinessEntity_ForTestOnly must not have changes.", false, plugIn.HostBusinessEntity_ForTestOnly.HasChanges);

				menu.PerformClick();

				AssertEquals("plugIn.HostBusinessEntity_ForTestOnly must have changes.", true, plugIn.HostBusinessEntity_ForTestOnly.HasChanges);

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var job1InNewFactory = newFactory.Load<Job>(job1.PK);
				var job2InNewFactory = newFactory.Load<Job>(job2.PK);
				var consolCost1InNewFactory = newFactory.Load<JobConsolCost>(consolCost1.PK);
				var consolCost2InNewFactory = newFactory.Load<JobConsolCost>(consolCost2.PK);

				AssertEquals("Job1 Exchange Rate must be updated.", defaultRateForShipmentJob, job1InNewFactory.ExchangeRates[0].JF_BaseRate);
				AssertEquals("Job2 Exchange Rate must be updated.", defaultRateForShipmentJob, job2InNewFactory.ExchangeRates[0].JF_BaseRate);
				AssertEquals("ConsolCost1 Exchange Rate must be updated.", defaultRateForConsolCost, consolCost1.E6_ExchangeRate);
				AssertEquals("Exchange rate of apportion charge linked to ConsolCost1 must be updated.", defaultRateForConsolCost, consolCost1.ApportionmentCharges[0].JR_OSCostExRate);
				AssertEquals("Exchange rate of apportion charge linked to ConsolCost1 must be updated.", defaultRateForConsolCost, consolCost1.ApportionmentCharges[1].JR_OSCostExRate);
				AssertEquals("ConsolCost2 Exchange Rate must be updated.", defaultRateForConsolCost, consolCost2.E6_ExchangeRate);
				AssertEquals("Exchange rate of apportion charge linked to ConsolCost2 must be updated.", defaultRateForConsolCost, consolCost2.ApportionmentCharges[0].JR_OSCostExRate);
				AssertEquals("Exchange rate of apportion charge linked to ConsolCost2 must be updated.", defaultRateForConsolCost, consolCost2.ApportionmentCharges[1].JR_OSCostExRate);
			}
		}

		public void TestRedefaultJobBillingExchangeRateOnSaving()
		{
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "IMP", "SEA", preference: "HAR", prompt: true);
			GlbCompany.CurrentCompany.Factory.Save();

			RefExchangeRate rate = TestObjectCreator.USD.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Now.AddDays(-11).Date;
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(-9);
			rate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			rate.RE_SellRate = 3.5m;

			var consol = CreateConsol("", "");
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.Transports.AddNew().FillWithValidTestData();
			consol.Transports.ArrivalTransport.JW_RL_NKDiscPort = consol.JK_RL_NKDischargePort;
			consol.Transports.DepartureTransport.JW_RL_NKLoadPort = consol.JK_RL_NKLoadPort;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_RL_NKOrigin = "USLAX";
			shipment1.JS_RL_NKDestination = "AUMEL";
			consol.Shipments.Add(shipment1);
			Job job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			job1.AddCurrency(TestObjectCreator.USD, 2M, ExchangeRateValidLedgerEnum.AR);

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = "SEA";
			shipment2.JS_RL_NKOrigin = "USLAX";
			shipment2.JS_RL_NKDestination = "AUMEL";
			consol.Shipments.Add(shipment2);
			Job job2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			job2.AddCurrency(TestObjectCreator.USD, 3M, ExchangeRateValidLedgerEnum.AR);

			Factory.Save();

			consol.Transports.ArrivalTransport.JW_IsLinked = true;

			ExchangeRateReader.GetReaderInstance().ClearCache();

			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
			{
				AssertEquals("Precondition for job1: ", 2M, job1.ExchangeRates[0].JF_BaseRate);
				AssertEquals("Precondition for job2: ", 3M, job2.ExchangeRates[0].JF_BaseRate);
				AssertEquals("Precondition: ArrivalTransport must be linked as it different way or working with the dates.", true, consol.Transports.ArrivalTransport.JW_IsLinked);
				AssertEquals("Precondition DepartureTransport must not be linked as we need to test both cases.", false, consol.Transports.DepartureTransport.JW_IsLinked);
				plugIn.HookFormEventsCore_ForTestOnly();
				string expectedMessage = "Departure or Arrival dates have been changed.\r\nDo you want to re-default the exchange rates on the shipment billing tabs?";

				consol.Transports.ArrivalTransport.JW_ATA = ZDateTime.Now.AddDays(-10);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertEquals("Consol ATA date is changed. Prompt must be shown.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Job1 Exchange Rate must be updated.", 3.5M, job1.ExchangeRates[0].JF_BaseRate);
				AssertEquals("Job2 Exchange Rate must be updated.", 3.5M, job2.ExchangeRates[0].JF_BaseRate);

				consol.Transports.ArrivalTransport.JW_ATA = ZDateTime.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertEquals("Consol ATA date is empty. Prompt must be shown.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				consol.Transports.ArrivalTransport.JW_ETA = ZDateTime.Now.AddDays(-11);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertEquals("Consol ETA date is changed. Prompt must be shown.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				consol.Transports.ArrivalTransport.JW_ETA = ZDateTime.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertEquals("Consol ETA date is empty. Prompt must be shown.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				consol.Transports.DepartureTransport.JW_ATD = ZDateTime.Now.AddDays(-12);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertEquals("Consol ATD date is changed. Prompt must be shown.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				consol.Transports.DepartureTransport.JW_ATD = ZDateTime.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertEquals("Consol ATD date is empty. Prompt must be shown.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				consol.Transports.DepartureTransport.JW_ETD = ZDateTime.Now.AddDays(-13);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertEquals("Consol ETD date is changed. Prompt must be shown.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				consol.Transports.DepartureTransport.JW_ETD = ZDateTime.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertEquals("Consol ETD date is empty. Prompt must be shown.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertNull("Consol dates is not changed. Prompt must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "IMP", "SEA", preference: "HAR");
				GlbCompany.CurrentCompany.Factory.Save();

				consol.Transports.ArrivalTransport.JW_ATA = ZDateTime.Now.AddDays(-14);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertNull("Consol ATA date is changed, but registry is not configured for Prompt. Prompt must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "IMP", "SEA", preference: "HAR", prompt: true);
				GlbCompany.CurrentCompany.Factory.Save();

				consol.Transports.ArrivalTransport.JW_ATA = ZDateTime.Now.AddDays(-15);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertEquals("Consol ATA date is changed again and registry set up to Prompt. Prompt must be shown.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				consol.Transports.ArrivalTransport.JW_ATA = ZDateTime.Now.AddDays(-16);
				job1.ExchangeRates.RemoveAndDeleteAll();
				job2.ExchangeRates.RemoveAndDeleteAll();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnSaving();
				Factory.Save();
				AssertNull("Consol ATA date is changed, but there aren't rates to update. Prompt must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSynchroniseInvoiceDetailsMenuItem()
		{
			var consol = CreateConsol();
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUMEL";

			var shipment1 = CreateShipment(consol);
			var job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();

			var shipment2 = CreateShipment(consol);
			var job2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();

			Factory.Save();

			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
			{
				MenuItem menu = plugIn.GetNewTopLevelMenu_ForTestOnly().MenuItems[14];
				AssertNotNull(menu);
				AssertEquals("Synchronize Cost Invoice Details", menu.Text);

				var apportionmentListing = consol.GetApportionments();

				//Before Posting

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.PerformClick();

				AssertNotNull("A message must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("All Invoice details on Apportionment Charges are in sync with Consol Cost ones.", UnitTestUserNotification.Instance.LastMessage.Text);

				var cost = CreateConsolCost(apportionmentListing);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.PerformClick();

				AssertNotNull("A message must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("All Invoice details on Apportionment Charges are in sync with Consol Cost ones.", UnitTestUserNotification.Instance.LastMessage.Text);

				cost.ApportionmentCharges[0].JR_APInvoiceNum = "XYZ";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.PerformClick();

				AssertNotNull("A message must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("There were some adjustment made to the Invoice details on Apportionment Charges.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("ABC123", cost.ApportionmentCharges[0].JR_APInvoiceNum);

				//After Posting

				var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1.0m, TestObjectCreator.AALSHI) as APInvoice;
				invoice.AH_TransactionNum = "INV0099";
				invoice.AH_OH = TestObjectCreator.Creditor1.PK;
				invoice.AH_InvoiceDate = ZDateTime.Today.AddDays(-1);
				invoice.AH_DueDate = ZDateTime.Today.AddDays(5);
				invoice.AH_TransactionReference = "NWREF#";

				var apline1 = TestObjectCreator.CreateAPInvoiceLine(invoice, cost.ApportionmentCharges[0].Job as Job, cost.ChargeCode, TestObjectCreator.AUD, 1m, "Test Line 1", 100m);
				var apline2 = TestObjectCreator.CreateAPInvoiceLine(invoice, cost.ApportionmentCharges[1].Job as Job, cost.ChargeCode, TestObjectCreator.AUD, 1m, "Test Line 2", 110m);

				cost.E6_AH_APInvoice = invoice.PK;
				cost.ApportionmentCharges[0].JR_AL_APLine = invoice.Lines[0].PK;
				cost.ApportionmentCharges[1].JR_AL_APLine = invoice.Lines[1].PK;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.PerformClick();

				AssertEquals("There were some adjustment made to the Invoice details on Apportionment Charges.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertSynchronizedConsolCost(invoice, cost, invoice.Lines[0].AL_AT, invoice.Lines[0].AL_A9_VATClass);
				AssertSynchronizedApportionedCharges(invoice, cost.ApportionmentCharges[0], invoice.Lines[0]);
				AssertSynchronizedApportionedCharges(invoice, cost.ApportionmentCharges[1], invoice.Lines[1]);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.PerformClick();

				AssertNotNull("A message must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("All Invoice details on Apportionment Charges are in sync with Consol Cost ones.", UnitTestUserNotification.Instance.LastMessage.Text);

				//Partial Posting
				cost = CreateConsolCost(apportionmentListing);

				var invoice1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1.0m, TestObjectCreator.AALSHI) as APInvoice;
				invoice1.AH_TransactionNum = "INV0001";
				apline1 = TestObjectCreator.CreateAPInvoiceLine(invoice1, cost.ApportionmentCharges[0].Job as Job, cost.ChargeCode, TestObjectCreator.AUD, 1m, "Test Line 1", 100m);
				apline2 = TestObjectCreator.CreateAPInvoiceLine(invoice1, cost.ApportionmentCharges[1].Job as Job, cost.ChargeCode, TestObjectCreator.AUD, 1m, "Test Line 2", 110m);

				cost.E6_AH_APInvoice = invoice1.PK;
				cost.ApportionmentCharges[0].JR_AL_APLine = apline1.PK;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.PerformClick();

				AssertContains(string.Format(@"Following Consol Cost(s) cannot be synchronized. You should try deleting them. Deleting will keep charges, only Consol Cost can be unlinked and removed.
Consol Cost with Charge Code {0}, Invoice # {1} for Creditor {2}
- Has Charge(s) not posted or posted to different Invoice.
- Has Charge(s) posted with different Tax Rate.", cost.ChargeCode.AC_Code, cost.E6_InvoiceNum, cost.Creditor.OH_Code), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestShowsMessageOnMutexException_Posting()
		{
			TestShowsMessageOnMutexException(false);
		}

		[ExpectNoExceptions]
		public void TestShowsMessageOnMutexException_Preview()
		{
			TestShowsMessageOnMutexException(true);
		}

		void TestShowsMessageOnMutexException(bool preview)
		{
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			var consolShipment1 = consol.Shipments.AddNew();
			consolShipment1.JS_UniqueConsignRef = "S0000558";

			var apportionmentListing = new ApportionmentListing(Factory, consol);
			var cost = apportionmentListing.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.ChargeableUnits;
			cost.E6_OSCostAmount = 100m;
			cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			cost.E6_InvoiceNum = "ABC123";
			cost.E6_InvoiceDate = ZDateTime.Now;

			consolShipment1.ShipmentJobHeader.JH_OA_LocalChargesAddr = TestObjectCreator.AALSHI.ActiveOrAllAddresses[0].PK;

			Factory.Save();

			var consolShipment2 = consol.Shipments.AddNew();
			consolShipment2.JS_UniqueConsignRef = "S0000559";

			Factory.Save();

			var lockingFactory = new BusinessObjectFactory();
			var loader = new Job.Loader(lockingFactory, lockingFactory.Load<ForwardingShipment>(consolShipment2.PK));
			var job = loader.TryLoadOrCreateWithMutex();
			AssertNotNull("pre condition, we can aquire mutex for testing", job);

			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
			using (Control control = plugIn.UserControl)
			{
				if (preview)
				{
					plugIn.PreviewTransactions_ForTestOnly(JobInvoicingPostingOption.All);
				}
				else
				{
					plugIn.PostTransactions_ForTestOnly(JobInvoicingPostingOption.All);
				}

				AssertEquals(@"You have created the job S0000559 on another form, but haven't saved it yet.
Please close or save other forms that use job S0000559 to continue.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			job.Dispose();
			consolShipment1.ShipmentJobHeader.Dispose();
		}

		public void TestShowPreSaveDialogsShouldNotAskToReopenClosedJobs()
		{
			var consol = Factory.New<ForwardingConsol>();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
			using (Control control = plugIn.UserControl)
			{
				plugIn.ShowPreSaveDialogs();
				AssertNull("Job Reopen Dialog Should not be shown", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowPreSaveDialogsShouldNotCauseMutexErrorWhenCostingTabIsNotActive()
		{
			using (AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol = Factory.New<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				Factory.Save();

				var job = new Job.Loader(shipment).TryCreateWithMutex(); // simulate another user clicking billing tab

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
				using (Control control = plugIn.UserControl)
				{
					plugIn.ShowPreSaveDialogs(); // simulate save button without clicking on consol costing tab
					AssertNull("job reopen dialog should not be shown", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				job.Factory.Save(); // get rid of the mutex
			}
		}

		public void TestShowsMessageOnMutexException_MenuItemSynchroniseInvoiceDetails_Click()
		{
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			var consol = CreateConsol();
			var consolShipment1 = CreateShipment(consol);
			consolShipment1.JS_UniqueConsignRef = "S0000558";

			var apportionmentListing = new ApportionmentListing(Factory, consol);
			var cost = apportionmentListing.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.ChargeableUnits;
			cost.E6_OSCostAmount = 100m;
			cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			cost.E6_InvoiceNum = "ABC123";
			cost.E6_InvoiceDate = ZDateTime.Now;

			consolShipment1.ShipmentJobHeader.JH_OA_LocalChargesAddr = TestObjectCreator.AALSHI.ActiveOrAllAddresses[0].PK;

			Factory.Save();

			var consolShipment2 = CreateShipment(consol);
			consolShipment2.JS_UniqueConsignRef = "S0000559";

			Factory.Save();

			var lockingFactory = new BusinessObjectFactory();
			var loader = new Job.Loader(lockingFactory, lockingFactory.Load<ForwardingShipment>(consolShipment2.PK));
			var job = loader.TryLoadOrCreateWithMutex();
			AssertNotNull("pre condition, we can aquire mutex for testing", job);

			using (ApportionmentPlugin plugIn = new ApportionmentPlugin(consol))
			using (Control control = plugIn.UserControl)
			{
				plugIn.MenuItemSynchroniseInvoiceDetails_Click_ForTestOnly(this, null);

				AssertEquals(@"You have created the job S0000559 on another form, but haven't saved it yet.
Please close or save other forms that use job S0000559 to continue.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			job.Dispose();
			consolShipment1.ShipmentJobHeader.Dispose();
		}

		public void TestGatewaySync_NoExceptionWhenShipmentAddedAfterViewingApportionmentTab()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			consol.Transports[0].JW_VoyageFlight = "QF1234";
			consol.Transports[0].JW_ETA = ZDateTime.Today;

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var gatewayAgentPort = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var gatewayJob = TestObjectCreator.CreateJob(consol);
			gatewayJob.JH_OA_LocalChargesAddr = consol.JK_OA_SendingForwarderAddress;
			gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

			Factory.Save();

			Assert("Pre-condition", consol.IsGateway());

			var charge = gatewayJob.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.FRT.PK;
			charge.JR_GE = TestObjectCreator.GEADepartment.PK;
			charge.JR_OH_SellAccount = consol.SendingForwarder.PK;
			charge.JR_LocalSellAmt = 111m;
			charge.JR_JH_InternalJob = gatewayJob.PK;

			using (var form = new ZForm(consol))
			{
				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ZArchitecture.Modules.ControllerIDs.Apportionment);
				form.Show();

				using (var plugIn = (ApportionmentPlugin)form.PlugIns.Instances[0])
				{
					plugIn.TabPage.Show();
				}

				var shipment = consol.Shipments.AddNew();
				var shipmentJob = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));

				AssertNull("Pre-condition", shipmentJob);

				AssertNoExceptionThrown("Expected to be able to save", () => GatewaySellToCostSynchroniser.Synchronise(gatewayJob));

				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK).AddToFilter(JobConsolCostSchema.E6_GC, GlbCompany.CurrentCompany.PK));
				shipmentJob = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));

				AssertEquals("Consol should have an cost", 1, consolCosts.Length);
				AssertEquals(111m, consolCosts[0].E6_LocalCostAmount);
				AssertNotNull("Shipment should now have a job", shipmentJob);
				shipmentJob.Dispose();
			}
		}

		[TestDate(2015, 1, 1)]
		public void TestGetNewTopLevelMenuCurrentInvoiceDateMenu()
		{
			var consol = CreateConsol();
			var shipment1 = CreateShipment(consol);
			var shipment2 = CreateShipment(consol);

			MenuItemTestHelper.ValidateDefaultARInvoiceDateMenuText(
				() =>
				{
					using (var plugIn = new ApportionmentPlugin(consol))
					{
						var menu = plugIn.GetNewTopLevelMenu_ForTestOnly();
						return menu.MenuItems[0].Text;
					}
				}
				);
		}

		public void TestPostTransactionsWhenCreditorSelfBilling()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			Factory.Save();

			using (var plugIn = new ApportionmentPlugin(consol))
			{
				TestObjectCreator.AALSHI.CompanyData.OB_APCostsSelfBilled = true;

				var shipment1 = consol.Shipments.AddNew();
				shipment1.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.ABIGAS.PK;
				var job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
				job1.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
				Factory.Save();

				var newCharge = job1.Charges.AddNew();
				newCharge.JR_AC = TestObjectCreator.FRT.PK;
				newCharge.JR_RX_NKCostCurrency = "AUD";
				newCharge.JR_OSSellAmt = 500;
				newCharge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
				Factory.Save();

				var shipment2 = consol.Shipments.AddNew();
				shipment2.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
				var job2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
				job2.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
				Factory.Save();

				newCharge = job2.Charges.AddNew();
				newCharge.JR_AC = TestObjectCreator.DSBChargeCode.PK;
				newCharge.JR_RX_NKCostCurrency = "AUD";
				newCharge.JR_OSSellAmt = 500;
				newCharge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
				Factory.Save();

				job1.Charges[0].JR_OH_SellAccount = ZGuid.Empty;
				job2.Charges[0].JR_OH_SellAccount = ZGuid.Empty;

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Assert("AP Invoice Number hasn't been set yet.", string.IsNullOrEmpty(job1.Charges[0].JR_APInvoiceNum));
				Assert("AP Invoice Number hasn't been set yet.", string.IsNullOrEmpty(job2.Charges[0].JR_APInvoiceNum));

				plugIn.PostTransactions_ForTestOnly(JobInvoicingPostingOption.All);
				AssertContains("The invoices are posted.", "Do you want to print Self Billing Invoice", UnitTestUserNotification.Instance.LastMessage.Text);

				var invoices = Factory.Load<APInvoice>(new ZQuery());
				AssertEquals("Should be 2 AP Invoices", 2, invoices.Length);
				Assert("AP Invoice Number should has to be set by successfully posted.", !string.IsNullOrEmpty(invoices[0].AH_TransactionNum));
				Assert("AP Invoice Number should has to be set by successfully posted.", !string.IsNullOrEmpty(invoices[0].AH_TransactionNum));
			}
		}

		public void TestApportionmentPluginCouldShowNotDisplayedMessageWhenShipmentJobIsCreated()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("KRSEL", "AUSYD", "C1", true);
			var shipment = creator.CreateShipment("S1", consol);
			Factory.Save();

			AssertNull("Precondition: shipment job is not created", new JobHeader.Loader(shipment).Load());

			using (var shipmentJob = new JobHeader.Loader(new BusinessObjectFactory(), shipment).TryCreateWithMutex())
			using (ApportionmentPluginForTest plugIn = new ApportionmentPluginForTest(consol) { Enabled = false })
			{
				AssertNotNull(plugIn.MainMenuItem_ForTestOnly);

				plugIn.MainMenuItem_ForTestOnly.Visible = true;
				plugIn.MainMenuItem_ForTestOnly.PerformClick();
				AssertNull("No Error Message should be displayed", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNewApportionmentUserControlSetsEnableDirectSettingConsolCostParentBusinessContext()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();

			Assert("Precondition: No EnableDirectSettingConsolCostParent context on Consol", !consol.HasContext(BusinessContext.EnableDirectSettingConsolCostParent));
			using (ApportionmentPluginForTest plugIn = new ApportionmentPluginForTest(consol))
			using (NewApportionmentUserControlForTest control = (NewApportionmentUserControlForTest)plugIn.UserControl)
			using (ZForm form = new ZForm(consol))
			{
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(control, "");
				form.Controls.Add(control);
				form.Show();
				Assert("Consol should have EnableDirectSettingConsolCostParent context", consol.HasContext(BusinessContext.EnableDirectSettingConsolCostParent));

				form.Close();
				Assert("EnableDirectSettingConsolCostParent context should be removed after closing form", !consol.HasContext(BusinessContext.EnableDirectSettingConsolCostParent));
			}
		}

		public void TestErrorIsNotReportedWhenCallQueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInBackground()
		{
			var consol = TestObjectCreator.CreateConsol(consolNum: "C1111");
			var shipment1 = TestObjectCreator.CreateShipment("S0001", consol);

			Globals.IsUserInteractive = false;

			Factory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_Saving);

			using (new DisposableAction(() => ErrorReporter.Clear(), () => ErrorReporter.Clear()))
			{
				Factory.Save();
				Assert(!ErrorReporter.HasBeenReported("ApportionmentListing_NewJobCreatedWhenFactoryIsInSaveTransaction"));
			}

			void Factory_Saving(BusinessObjectFactory factory)
			{
				using (var plugIn = new ApportionmentPlugin(consol))
				{
					plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_ForTestOnly();
				}
			}
		}

		public void TestMenuItemResetTaxDefaults()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			var creditor = TestObjectCreator.Creditor1;
			var debtor = TestObjectCreator.Debtor;

			var consol = CreateConsol("USLAX", "AUMEL");
			consol.Transports.ArrivalTransport.JW_ATA = ZDateTime.Now.AddDays(-10);

			var shipment1 = TestObjectCreator.CreateShipment("TSTJob0001", "USLAX", "AUMEL", consol, false, "SEA");
			var job_shipment1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			AssertNotEquals("PreCondition", ZGuid.Empty, job_shipment1.JH_GB_TaxBranch);

			var shipment2 = TestObjectCreator.CreateShipment("TSTJob0002", "USLAX", "AUMEL", consol, false, "SEA");
			var job_shipment2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			AssertNotEquals("PreCondition", ZGuid.Empty, job_shipment2.JH_GB_TaxBranch);

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1);
			consolCost.E6_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			consolCost.E6_OSCostAmount = 10;
			consolCost.E6_OH_Creditor = creditor.PK;

			var chargeNotBelongConsol = job_shipment1.Charges.AddNew();
			chargeNotBelongConsol.JR_AC = TestObjectCreator.CC2.PK;
			chargeNotBelongConsol.JR_OH_CostAccount = creditor.PK;
			chargeNotBelongConsol.JR_OH_SellAccount = debtor.PK;

			Factory.Save();

			consolCost.ApportionmentCharges.FindChargeForJob(shipment2).JR_IsUsedForApportionment = false;
			Factory.Save();

			consolCost.UpdateApportionmentChargesListing();

			var chargeApportioned = job_shipment1.Charges.Where(x => x.JR_AC == TestObjectCreator.CC1.PK).First();
			chargeApportioned.JR_OH_SellAccount = debtor.PK;

			var chargeNotApportioned = job_shipment2.Charges.Where(x => x.JR_AC == TestObjectCreator.CC1.PK).First();
			chargeNotApportioned.JR_OH_CostAccount = creditor.PK;
			chargeNotApportioned.JR_OH_SellAccount = debtor.PK;

			var testCharges = new BaseCharge[] {
				chargeNotBelongConsol,
				chargeNotApportioned,
				chargeApportioned
			};

			using (var plugIn = new ApportionmentPlugin(consol))
			{
				var menu = GetMenuItem_ResetUnpostedLinesTaxDefault(plugIn);

				SetConsolWithTaxInfo(consolCost);
				testCharges.ForEach(charge => SetChargeWithTaxInfo(charge));
				creditor.CompanyData.SetAPTaxApplicable(false);
				debtor.CompanyData.SetARTaxApplicable(false);
				menu.PerformClick();
				Factory.Save();
				CombineAssertions("defaulting result", () =>
				{
					AssertConsolWithoutTaxInfo(consolCost, "consol cost should be re-defaulted.");
					AssertChargeWithoutTaxInfo(chargeApportioned, "should be re-defaulted due to apportioned charge.");

					AssertChargeWithoutTaxInfo(chargeNotBelongConsol, "should be reset even not belong to consol cost.");
					AssertChargeWithoutTaxInfo(chargeNotApportioned, "should be reset even not being apportioned.");
				});

				SetConsolWithoutTaxInfo(consolCost);
				testCharges.ForEach(charge => SetChargeWithoutTaxInfo(charge));
				creditor.CompanyData.SetAPTaxApplicable(true);
				debtor.CompanyData.SetARTaxApplicable(false);
				menu.PerformClick();
				Factory.Save();
				CombineAssertions("defaulting result", () =>
				{
					AssertConsolWithTaxInfo(consolCost, "consol cost should be re-defaulted  due to creditor is TaxApplicable.", false);
					AssertDefautResult_EmptySell(chargeApportioned, "should still be empty due to debtor is not TaxApplicable.");
					AssertDefautResult_Cost(chargeApportioned, "should be set default value due to apportioned charge.");

					AssertDefautResult_EmptySell(chargeNotBelongConsol, "should still be empty due to debtor is not TaxApplicable.");
					AssertDefautResult_Cost(chargeNotBelongConsol, "should be set default value even not belonging to consol cost.");

					AssertDefautResult_EmptySell(chargeNotApportioned, "should still be empty due to debtor is not TaxApplicable.");
					AssertDefautResult_Cost(chargeNotApportioned, "should be set default value even not being apportioned.");
				});

				SetConsolWithoutTaxInfo(consolCost);
				testCharges.ForEach(charge => SetChargeWithoutTaxInfo(charge));
				creditor.CompanyData.SetAPTaxApplicable(false);
				debtor.CompanyData.SetARTaxApplicable(true);
				menu.PerformClick();
				Factory.Save();
				CombineAssertions("defaulting result", () =>
				{
					AssertConsolWithoutTaxInfo(consolCost, "consol cost should not be re-defaulted due to creditor is not TaxApplicable.");
					AssertDefautResult_Sell(chargeApportioned, "should be set default value due to apportioned charge.");
					AssertDefautResult_EmptyCost(chargeApportioned, "should still be empty due to creditor is not TaxApplicable.");

					AssertDefautResult_Sell(chargeNotBelongConsol, "should be set default value even not belonging to consol cost.");
					AssertDefautResult_EmptyCost(chargeNotBelongConsol, "should still be empty due to creditor is not TaxApplicable.");

					AssertDefautResult_Sell(chargeNotApportioned, "should be set default value even not being apportioned.");
					AssertDefautResult_EmptyCost(chargeNotApportioned, "should still be empty due to creditor is not TaxApplicable.");
				});

				SetConsolWithoutTaxInfo(consolCost);
				testCharges.ForEach(charge => SetChargeWithoutTaxInfo(charge));
				creditor.CompanyData.SetAPTaxApplicable(true);
				debtor.CompanyData.SetARTaxApplicable(true);
				menu.PerformClick();
				Factory.Save();
				CombineAssertions("defaulting result", () =>
				{
					AssertConsolWithTaxInfo(consolCost, "consol cost should be re-defaulted due to creditor is TaxApplicable.", false);
					AssertDefautResult_Sell(chargeApportioned, "should be set default value due to apportioned charge.");
					AssertDefautResult_Cost(chargeApportioned, "should be set default value due to apportioned charge.");

					AssertDefautResult_Sell(chargeNotBelongConsol, "should be set default value even not belonging to consol cost.");
					AssertDefautResult_Cost(chargeNotBelongConsol, "should be set default value even not belonging to consol cost.");

					AssertDefautResult_Sell(chargeNotApportioned, "should be set default value even not being apportioned.");
					AssertDefautResult_Cost(chargeNotApportioned, "should be set default value even not being apportioned.");
				});

				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					SetConsolWithoutTaxInfo(consolCost);
					testCharges.ForEach(charge => SetChargeWithoutTaxInfo(charge));
					creditor.CompanyData.SetAPTaxApplicable(true);
					debtor.CompanyData.SetARTaxApplicable(true);
					menu.PerformClick();
					Factory.Save();
					CombineAssertions("defaulting result", () =>
					{
						AssertConsolWithTaxInfo(consolCost, "consol cost should be re-defaulted due to creditor is TaxApplicable.", false, false);
						AssertEquals("Tax Branch should remain to empty due to registry is not enabled", ZGuid.Empty, consolCost.E6_GB_CostTaxBranch);
						AssertDefautResult_Sell(chargeApportioned, "should be set default value due to apportioned charge.", false);
						AssertEquals("Tax Branch should remain to empty due to registry is not enabled", ZGuid.Empty, chargeApportioned.JR_GB_SellTaxBranch);
						AssertDefautResult_Cost(chargeApportioned, "should be set default value due to apportioned charge.", false);
						AssertEquals("Tax Branch should remain to empty due to registry is not enabled", ZGuid.Empty, chargeApportioned.JR_GB_CostTaxBranch);

						AssertDefautResult_Sell(chargeNotBelongConsol, "should be set default value even not belonging to consol cost.", false);
						AssertEquals("Tax Branch should remain to empty due to registry is not enabled", ZGuid.Empty, chargeNotBelongConsol.JR_GB_SellTaxBranch);
						AssertDefautResult_Cost(chargeNotBelongConsol, "should be set default value even not belonging to consol cost.", false);
						AssertEquals("Tax Branch should remain to empty due to registry is not enabled", ZGuid.Empty, chargeNotBelongConsol.JR_GB_CostTaxBranch);

						AssertDefautResult_Sell(chargeNotApportioned, "should be set default value even not being apportioned.", false);
						AssertEquals("Tax Branch should remain to empty due to registry is not enabled", ZGuid.Empty, chargeNotApportioned.JR_GB_SellTaxBranch);
						AssertDefautResult_Cost(chargeNotApportioned, "should be set default value even not being apportioned.", false);
						AssertEquals("Tax Branch should remain to empty due to registry is not enabled", ZGuid.Empty, chargeNotApportioned.JR_GB_CostTaxBranch);
					});
				}

				SetConsolWithoutTaxInfo(consolCost);
				testCharges.ForEach(charge => SetChargeWithoutTaxInfo(charge));
				consolCost.ApportionmentCharges.FindChargeForJob(chargeNotApportioned.ShipmentInfo).JR_IsUsedForApportionment = true;
				creditor.CompanyData.SetAPTaxApplicable(true);
				debtor.CompanyData.SetARTaxApplicable(true);
				menu.PerformClick();
				Factory.Save();
				CombineAssertions("defaulting result", () =>
				{
					AssertConsolWithTaxInfo(consolCost, "consol cost should be re-defaulted due to creditor is TaxApplicable.", false);
					AssertDefautResult_Sell(chargeNotApportioned, "should be set default value without concurrency error.");
					AssertDefautResult_Cost(chargeNotApportioned, "should be set default value without concurrency error.");
				});

				SetConsolWithoutTaxInfo(consolCost);
				testCharges.ForEach(charge => SetChargeWithoutTaxInfo(charge));
				consolCost.ApportionmentCharges.FindChargeForJob(chargeNotApportioned.ShipmentInfo).JR_IsUsedForApportionment = false;
				creditor.CompanyData.SetAPTaxApplicable(true);
				debtor.CompanyData.SetARTaxApplicable(true);
				menu.PerformClick();
				Factory.Save();
				CombineAssertions("defaulting result", () =>
				{
					AssertConsolWithTaxInfo(consolCost, "consol cost should be re-defaulted due to creditor is TaxApplicable.", false);
					AssertDefautResult_Sell(chargeNotApportioned, "should be set default value without concurrency error.");
					AssertDefautResult_EmptyCost(chargeNotApportioned, "cost info should be cleared due to canceling apportionment.");
				});
			}

			void SetConsolWithoutTaxInfo(JobConsolCost cost)
			{
				cost.E6_GB_CostTaxBranch = ZGuid.Empty;
				cost.E6_AT_TaxRate = ZGuid.Empty;
				cost.E6_TaxDate = ZDate.Empty;
				AssertConsolWithoutTaxInfo(cost, "PreCondition");
			}

			void AssertConsolWithoutTaxInfo(JobConsolCost cost, string comment)
			{
				AssertEquals($"{comment}E6_AT_TaxRate", ZGuid.Empty, cost.E6_AT_TaxRate);
				AssertEquals($"{comment}E6_TaxDate", ZDate.Empty, cost.E6_TaxDate);
				AssertEquals($"{comment}E6_GB_CostTaxBranch", ZGuid.Empty, cost.E6_GB_CostTaxBranch);
			}

			void SetConsolWithTaxInfo(JobConsolCost cost)
			{
				cost.E6_GB_CostTaxBranch = GlbBranch.CurrentBranch.PK;
				cost.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
				cost.E6_TaxDate = ZDate.Today;
				AssertConsolWithTaxInfo(cost, "PreCondition");
			}

			void AssertConsolWithTaxInfo(JobConsolCost cost, string comment, bool isAssertTaxDate = true, bool isAssertTaxBranch = true)
			{
				AssertNotEquals($"{comment}E6_AT_TaxRate", ZGuid.Empty, cost.E6_AT_TaxRate);

				if (isAssertTaxDate)
				{
					AssertNotEquals($"{comment}E6_TaxDate", ZDate.Empty, cost.E6_TaxDate);
				}

				if (isAssertTaxBranch)
				{
					AssertNotEquals($"{comment}E6_GB_CostTaxBranch", ZGuid.Empty, cost.E6_GB_CostTaxBranch);
				}
			}

			void SetChargeWithoutTaxInfo(BaseCharge charge)
			{
				charge.JR_AT_CostGSTRate = ZGuid.Empty;
				charge.JR_CostTaxDate = ZDate.Empty;
				charge.JR_GB_CostTaxBranch = ZGuid.Empty;
				charge.JR_AT_SellGSTRate = ZGuid.Empty;
				charge.JR_SellTaxDate = ZDate.Empty;
				charge.JR_GB_SellTaxBranch = ZGuid.Empty;
				AssertChargeWithoutTaxInfo(charge, "PreCondition");
			}

			void SetChargeWithTaxInfo(BaseCharge charge)
			{
				charge.JR_GB_CostTaxBranch = GlbBranch.CurrentBranch.PK;
				charge.JR_CostTaxDate = ZDate.Today;
				charge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
				charge.JR_GB_SellTaxBranch = GlbBranch.CurrentBranch.PK;
				charge.JR_SellTaxDate = ZDate.Today;
				charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
				AssertChargeWithTaxInfo(charge, "PreCondition");
			}

			void AssertChargeWithTaxInfo(BaseCharge charge, string comment = "")
			{
				AssertNotEquals($"{comment}JR_AT_CostGSTRate", ZGuid.Empty, charge.JR_AT_CostGSTRate);
				AssertNotEquals($"{comment}JR_CostTaxDate", ZDate.Empty, charge.JR_CostTaxDate);
				AssertNotEquals($"{comment}JR_GB_CostTaxBranch", ZGuid.Empty, charge.JR_GB_CostTaxBranch);

				AssertNotEquals($"{comment}JR_AT_SellGSTRate", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				AssertNotEquals($"{comment}JR_SellTaxDate", ZDate.Empty, charge.JR_SellTaxDate);
				AssertNotEquals($"{comment}JR_GB_SellTaxBranch", ZGuid.Empty, charge.JR_GB_SellTaxBranch);
			}

			void AssertChargeWithoutTaxInfo(BaseCharge charge, string comment = "")
			{
				AssertEquals($"{comment}JR_AT_CostGSTRate", ZGuid.Empty, charge.JR_AT_CostGSTRate);
				AssertEquals($"{comment}JR_CostTaxDate", ZDate.Empty, charge.JR_CostTaxDate);
				AssertEquals($"{comment}JR_GB_CostTaxBranch", ZGuid.Empty, charge.JR_GB_CostTaxBranch);

				AssertEquals($"{comment}JR_AT_SellGSTRate", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				AssertEquals($"{comment}JR_SellTaxDate", ZDate.Empty, charge.JR_SellTaxDate);
				AssertEquals($"{comment}JR_GB_SellTaxBranch", ZGuid.Empty, charge.JR_GB_SellTaxBranch);
			}

			void AssertDefautResult_Cost(BaseCharge charge, string comment = "", bool isTestTaxbranch = true)
			{
				AssertNotEquals($"{comment}JR_AT_CostGSTRate", ZGuid.Empty, charge.JR_AT_CostGSTRate);
				AssertEquals($"{comment}JR_CostTaxDate", ZDate.Empty, charge.JR_CostTaxDate);

				if (isTestTaxbranch)
				{
					AssertNotEquals($"{comment}JR_GB_CostTaxBranch", ZGuid.Empty, charge.JR_GB_CostTaxBranch);
				}
			}

			void AssertDefautResult_EmptyCost(BaseCharge charge, string comment = "")
			{
				AssertEquals($"{comment}JR_AT_CostGSTRate", ZGuid.Empty, charge.JR_AT_CostGSTRate);
				AssertEquals($"{comment}JR_CostTaxDate", ZDate.Empty, charge.JR_CostTaxDate);
				AssertEquals($"{comment}JR_GB_CostTaxBranch", ZGuid.Empty, charge.JR_GB_CostTaxBranch);
			}

			void AssertDefautResult_Sell(BaseCharge charge, string comment = "", bool isTestTaxbranch = true)
			{
				AssertNotEquals($"{comment}JR_AT_SellGSTRate", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				AssertEquals($"{comment}JR_SellTaxDate", ZDate.Empty, charge.JR_SellTaxDate);

				if (isTestTaxbranch)
				{
					AssertNotEquals($"{comment}JR_GB_SellTaxBranch", ZGuid.Empty, charge.JR_GB_SellTaxBranch);
				}
			}

			void AssertDefautResult_EmptySell(BaseCharge charge, string comment = "")
			{
				AssertEquals($"{comment}JR_AT_SellGSTRate", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				AssertEquals($"{comment}JR_SellTaxDate", ZDate.Empty, charge.JR_SellTaxDate);
				AssertEquals($"{comment}JR_GB_SellTaxBranch", ZGuid.Empty, charge.JR_GB_SellTaxBranch);
			}
		}

		public void TestMenuItemResetTaxDefaults_JobTaxBranch()
		{
			var consol = CreateConsol("USLAX", "AUMEL");
			consol.Transports.ArrivalTransport.JW_ATA = ZDateTime.Now.AddDays(-10);

			var jobWithTaxBranch = new Job.Loader(Factory, TestObjectCreator.CreateShipment("TSTJob0001", "USLAX", "AUMEL", consol, false, "SEA")).TryCreateWithoutMutexForTestOnly();

			var jobWithoutTaxBranch = new Job.Loader(Factory, TestObjectCreator.CreateShipment("TSTJob0002", "USLAX", "AUMEL", consol, false, "SEA")).TryCreateWithoutMutexForTestOnly();

			Factory.Save();

			using (var plugIn = new ApportionmentPlugin(consol))
			{
				var resetMenu = GetMenuItem_ResetUnpostedLinesTaxDefault(plugIn);

				var boolList = new[] { false, true };

				foreach (var isGSTRegistered in boolList)
				{
					foreach (var enableTaxBranchReporting in boolList)
					{
						GlbCompany.CurrentCompany.GC_IsGSTRegistered = isGSTRegistered;
						AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting);
						AssertBehaviour(resetMenu, isGSTRegistered && enableTaxBranchReporting);
					}
				}
			}

			void DefaultJobTaxbranch()
			{
				jobWithTaxBranch.JH_GB_TaxBranch = TestObjectCreator.NonCurrentBranch.PK;
				AssertEquals("PreCondition", TestObjectCreator.NonCurrentBranch.PK, jobWithTaxBranch.JH_GB_TaxBranch);

				jobWithoutTaxBranch.JH_GB_TaxBranch = ZGuid.Empty;
				AssertEquals("PreCondition", ZGuid.Empty, jobWithoutTaxBranch.JH_GB_TaxBranch);
			}

			void AssertBehaviour(MenuItem resetMenu, bool isTaxBranchEnabled)
			{
				DefaultJobTaxbranch();
				resetMenu.PerformClick();

				if (isTaxBranchEnabled)
				{
					AssertJobTaxbranchEnabled();
				}
				else
				{
					AssertJobTaxbranchDisabled();
				}
			}

			void AssertJobTaxbranchDisabled()
			{
				CombineAssertions("When TaxBranch is disabled, Job's TaxBranch should be set as empty, But when Job's TaxBranch is Non-empty, It will not be changed. ", () =>
				{
					AssertEquals("jobWithTaxBranch", TestObjectCreator.NonCurrentBranch.PK, jobWithTaxBranch.JH_GB_TaxBranch);
					AssertEquals("jobWithoutTaxBranch", ZGuid.Empty, jobWithoutTaxBranch.JH_GB_TaxBranch);
				});
			}

			void AssertJobTaxbranchEnabled()
			{
				CombineAssertions("When TaxBranch is enabled, Job's TaxBranch should be set to login branch, But when Job's TaxBranch is Non-empty, It will not be changed. ", () =>
				{
					AssertEquals("jobWithTaxBranch", TestObjectCreator.NonCurrentBranch.PK, jobWithTaxBranch.JH_GB_TaxBranch);
					AssertEquals("jobWithoutTaxBranch", GlbBranch.CurrentBranch.PK, jobWithoutTaxBranch.JH_GB_TaxBranch);
				});
			}
		}

		public void TestMenuItemResetTaxDefaults_ReadyForFinancialClosureWithoutModifySecurity()
		{
			var consol = CreateConsol("USLAX", "AUMEL");
			consol.Transports.ArrivalTransport.JW_ATA = ZDateTime.Now.AddDays(-10);

			var jobIsReadyForFinancialClosure = new Job.Loader(Factory, TestObjectCreator.CreateShipment("TSTJob0001", "USLAX", "AUMEL", consol, false, "SEA")).TryCreateWithoutMutexForTestOnly();
			jobIsReadyForFinancialClosure.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;

			new Job.Loader(Factory, TestObjectCreator.CreateShipment("TSTJob0002", "USLAX", "AUMEL", consol, false, "SEA")).TryCreateWithoutMutexForTestOnly();

			Factory.Save();

			var securityRightForModifyChargeWhichJobIsJFC = Env.Security.ModifyChargesforFinancialClosureJob;
			var originalValueRightForModifyChargeWhichJobIsJFC = securityRightForModifyChargeWhichJobIsJFC.IsAllowed;

			using (new DisposableAction(() => { }, () => securityRightForModifyChargeWhichJobIsJFC.IsAllowed = originalValueRightForModifyChargeWhichJobIsJFC))
			using (var plugIn = new ApportionmentPlugin(consol))
			{
				var resetMenu = GetMenuItem_ResetUnpostedLinesTaxDefault(plugIn);

				UnitTestUserNotification.Instance.ClearMessages();
				securityRightForModifyChargeWhichJobIsJFC.IsAllowed = false;
				resetMenu.PerformClick();
				AssertEquals("Should have this error message when missing security."
					, @"Cannot reset tax defaults of unposted lines for this consol, because one or more jobs have Ready For Financial Closure status 
TSTJob0001"
					, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				securityRightForModifyChargeWhichJobIsJFC.IsAllowed = true;
				resetMenu.PerformClick();
				AssertEquals("Should not have error message when user have security.", null, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				securityRightForModifyChargeWhichJobIsJFC.IsAllowed = false;
				jobIsReadyForFinancialClosure.JH_Status = JobHeaderStatus.Working.Code;
				resetMenu.PerformClick();
				AssertEquals("Should not have error message when no job is Ready For Financial Closure status.", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		MenuItem GetMenuItem_ResetUnpostedLinesTaxDefault(ApportionmentPlugin plugIn)
		{
			var menu = plugIn.GetNewTopLevelMenu_ForTestOnly().MenuItems[13];
			AssertNotNull(menu);
			AssertEquals("PreCondition", "Reset Unposted lines Tax Default", menu.Text);
			return menu;
		}

		#region Implementation

		protected override ApportionmentPlugin GetTestApportionmentPluginObject(IBusiness consol)
		{
			return new ApportionmentPlugin(consol);
		}

		JobConsolCost CreateConsolCost(ApportionmentListing apportionmentListing)
		{
			var cost = apportionmentListing.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.ChargeableUnits;
			cost.E6_OSCostAmount = 100m;
			cost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			cost.E6_InvoiceNum = "ABC123";
			cost.E6_InvoiceDate = ZDateTime.Now;
			return cost;
		}

		void AssertSynchronizedApportionedCharges(APInvoice apInvoice, ApportionSplitCharge charge, AccTransactionLines aPLine)
		{
			AssertEquals(apInvoice.AH_TransactionNum, charge.JR_APInvoiceNum);
			AssertEquals(apInvoice.AH_OH, charge.JR_OH_CostAccount);
			AssertEquals(apInvoice.AH_InvoiceDate, charge.JR_APInvoiceDate);
			AssertEquals(apInvoice.AH_DueDate, charge.JR_PaymentDate);
			AssertEquals(apInvoice.AH_TransactionReference, charge.JR_CostReference);
			AssertEquals(aPLine.AL_AT, charge.JR_AT_CostGSTRate);
			AssertEquals(aPLine.AL_A9_VATClass, charge.JR_A9_CostVATClass);
		}

		void AssertSynchronizedConsolCost(APInvoice apInvoice, JobConsolCost cost, ZGuid taxRatePK, ZGuid vATClassPK)
		{
			AssertEquals(apInvoice.AH_TransactionNum, cost.E6_InvoiceNum);
			AssertEquals(apInvoice.AH_OH, cost.E6_OH_Creditor);
			AssertEquals(apInvoice.AH_InvoiceDate, cost.E6_InvoiceDate);
			AssertEquals(apInvoice.AH_DueDate, cost.E6_PaymentDate);
			AssertEquals(apInvoice.AH_TransactionReference, cost.E6_CostReference);
			AssertEquals(taxRatePK, cost.E6_AT_TaxRate);
			AssertEquals(vATClassPK, cost.E6_A9_VATClass);
		}

		AccChargeCode GetChargeCode(string chargeCode)
		{
			var query = new ZQuery();
			query.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			var accChargeCode = Factory.LoadTop1<AccChargeCode>(query);
			if (accChargeCode != null)
			{
				accChargeCode.AC_DepartmentFilterList = "ALL";
			}

			return accChargeCode;
		}

		ForwardingShipment CreateShipment(ForwardingConsol consol, decimal actualWeight = 100m, decimal actualVolume = 0.1m)
		{
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = consol.JK_TransportMode;
			shipment.JS_PackingMode = consol.JK_ConsolMode;
			shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort;
			shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort;
			shipment.JS_ActualWeight = actualWeight;
			shipment.JS_ActualVolume = actualVolume;

			return shipment;
		}

		public class ApportionmentPluginForTest : ApportionmentPlugin
		{
			public ApportionmentPluginForTest(IBusiness consol)
				: base(consol)
			{
			}

			protected override Control GetNewUserControl()
			{
				return new NewApportionmentUserControlForTest(Apportionments);
			}
		}

		OrgHeader CreateForwarder(string fullName)
		{
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			forwarder.OH_IsCreditor = true;
			forwarder.OH_IsDebtor = true;
			forwarder.OH_FullName = fullName;
			var forwarderAddress = Factory.NewWithValidTestData<OrgAddress>();
			forwarderAddress.OA_Code = "consolReceivingAgentAdr";
			forwarderAddress.OA_OH = forwarder.PK;
			forwarder.CompanyData.OB_RX_NKAPDefltCurrency = "NZD";
			forwarder.CompanyData.SetAPTaxApplicable(false);
			forwarder.CompanyData.SetARTaxApplicable(false);

			return forwarder;
		}

		void CreateProfitShareAgreement(OrgHeader consolReceivingAgent, OrgHeader consolSendingAgent)
		{
			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = consolSendingAgent.PK;
			agentRelationship.O3_OH_ReceivingAgent = consolReceivingAgent.PK;
			var profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "AIR";
			profitShareAgreement.O4_SendingPortOrCountry = consolSendingAgent.MainAddress.OA_RN_NKCountryCode;
			profitShareAgreement.O4_ReceivingPortOrCountry = consolReceivingAgent.MainAddress.OA_RN_NKCountryCode;
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(100);
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-100);
			var rcvParty = profitShareAgreement.PartyDetails.AddNew();
			rcvParty.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent;
			rcvParty.PS_PartyProfitSharePercent = 70m;
			var sndParty = profitShareAgreement.PartyDetails.AddNew();
			sndParty.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent;
			sndParty.PS_PartyProfitSharePercent = 30m;
		}

		#endregion
	}
}

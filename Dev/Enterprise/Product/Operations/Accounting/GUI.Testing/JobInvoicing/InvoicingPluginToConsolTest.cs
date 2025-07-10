using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class InvoicingPluginToConsolTest : TestCaseWithFactory
	{
		public void TestInvoicingPluginToConsolConstructorSetsBusinessContext()
		{
			var consol = Factory.New<ForwardingConsol>();
			Assert("Consol Factory should not have context", !consol.Factory.HasContext(BusinessContext.InvoicingPlugInGUI));
			using (new InvoicingPluginToConsol(consol))
			{
				Assert("Consol Factory should have InvoicingPlugInGUI context", consol.Factory.HasContext(BusinessContext.InvoicingPlugInGUI));
			}
			Assert("Consol Factory should still have InvoicingPlugInGUI context", consol.Factory.HasContext(BusinessContext.InvoicingPlugInGUI));
		}

		public void TestProfitShareMenuItem()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			using (InvoicingPluginToConsol plugin = new InvoicingPluginToConsol(consol))
			{
				Assert("Must be a DocumentCommand", plugin.ProfitShareMenuItem_ForTestOnly is DocumentCommand);
				Assert("SU_IsSystemDefined should be true", plugin.ProfitShareMenuItem_ForTestOnly.SU_IsSystemDefined);
				Assert("SU_SupportsVisualisation should be false", !plugin.ProfitShareMenuItem_ForTestOnly.SU_SupportsVisualisation);
				Assert("SU_IsModifiable should be false", !plugin.ProfitShareMenuItem_ForTestOnly.SU_IsModifiable);
			}
		}

		#region Profit Share

		public void TestProfitShareDocumentGeneration()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			using (TestInvoicingPluginToConsol plugIn = new TestInvoicingPluginToConsol(consol))
			{
				using (Control control = plugIn.UserControl)
				{
					plugIn.SetJobs(new JobCollection(Factory));

					PrintTask task = new PrintTask();
					AssertEquals("Precondition: No packs in the print task", 0, task.Count);

					DocumentPack pack1 = plugIn.GetDocumentPackForOrg_ForTestOnly(task, null, org1);
					AssertNotNull("Pack returned", pack1);
					AssertEquals("1 pack in the print task", 1, task.Count);

					DocumentPack pack2 = plugIn.GetDocumentPackForOrg_ForTestOnly(task, null, org2);
					AssertNotNull("Pack returned", pack2);
					Assert("Different doc pack returned for Org 2", pack1 != pack2);
					AssertEquals("2 packs in the print task", 2, task.Count);

					DocumentPack pack3 = plugIn.GetDocumentPackForOrg_ForTestOnly(task, null, org1);
					AssertNotNull("Pack returned", pack3);
					AssertEquals("Pack returned is the same as the first pack returned", pack1, pack3);
					AssertEquals("Still 2 packs in the print task", 2, task.Count);

					AssertEquals("Both returned packs have the Consol as the business object to log against", consol, pack1.BusinessObjectToLogAgainst);
					AssertEquals("Both returned packs have the Consol as the business object to log against", consol, pack2.BusinessObjectToLogAgainst);
					AssertEquals("Both returned packs have the Consol as the business object to log against", consol, pack3.BusinessObjectToLogAgainst);

					AssertEquals("Packs have the correct org", org1, pack1.Organisation);
					AssertEquals("Packs have the correct org", org2, pack2.Organisation);
					AssertEquals("Packs have the correct org", org1, pack3.Organisation);
				}
			}
		}

		#endregion

		#region Job Invoicing Header Creation

		public void TestJobHeadersNotCreatedWhenRegistryIsSetToFalse()
		{
			SetUpJobHeaderCreationTest(false);

			try
			{
				ForwardingConsol consol = CreateConsol("", "", "");
				var shipment1 = CreateShipment("S00010001", "AUSYD", "NZAKL");
				consol.Shipments.Add(shipment1);
				var shipment2 = CreateShipment("S00010002", "", "");
				consol.Shipments.Add(shipment2);

				using (TestInvoicingPluginToConsol plugIn = new TestInvoicingPluginToConsol(consol))
				{
					using (Control control = plugIn.UserControl)
					{
						Job shipment1Job = FindJobUsingNewFactoryRelatedToShipment(shipment1.PK);
						AssertNull("Should be no job related to Shipment 1", shipment1Job);
						Job shipment2Job = FindJobUsingNewFactoryRelatedToShipment(shipment2.PK);
						AssertNull("Should be no job related to Shipment 2", shipment2Job);

						plugIn.OnSaving();
						Factory.Save();

						shipment1Job = FindJobUsingNewFactoryRelatedToShipment(shipment1.PK);
						AssertNull("Should be no job related to Shipment 1", shipment1Job);
						shipment2Job = FindJobUsingNewFactoryRelatedToShipment(shipment2.PK);
						AssertNull("Should be no job related to Shipment 2", shipment2Job);
					}
				}
			}
			finally
			{
				TearDownJobHeaderCreationTest();
			}
		}

		ForwardingConsol CreateConsol(string portOfLoading, string portOfDischarge, string lastForeignPort)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportCodes.Sea;

			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = portOfDischarge;
			transport.JW_RL_NKLoadPort = portOfLoading;
			consol.JK_RL_NKLastForeignPort = lastForeignPort;

			return consol;
		}

		ForwardingShipment CreateShipment(ZString jobNumber, ZString origin, ZString destination)
		{
			return CreateShipment(jobNumber, origin, destination, ZString.Empty, null);
		}

		ForwardingShipment CreateShipment(ZString jobNumber, ZString origin, ZString destination, ZString shipmentType, ForwardingShipment parent)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment.JS_TransportMode = "SEA";
			shipment.JS_UniqueConsignRef = jobNumber;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;

			if (parent != null)
			{
				shipment.JS_JS_ColoadMasterShipment = parent.PK;
			}

			if (!shipmentType.IsEmpty)
			{
				shipment.JS_ShipmentType = shipmentType;
			}

			return shipment;
		}

		Job FindJobUsingNewFactoryRelatedToShipment(ZGuid shipmentPK)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ZQuery filter = new ZQuery(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(JobHeaderSchema.JH_ParentID, shipmentPK);
			return newFactory.LoadTop1<Job>(filter);
		}

		void SetUpJobHeaderCreationTest(bool registryValue)
		{
			OriginalRegistryValue = AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.Value;
			OriginalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			OriginalHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
		}

		void TearDownJobHeaderCreationTest()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = OriginalCountryCode;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = OriginalHomePort;
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OriginalRegistryValue);
		}

		bool OriginalRegistryValue;
		ZString OriginalCountryCode;
		ZString OriginalHomePort;

		#endregion

		public void TestSecurityARJobInvoicingPrintingPanel_WhenAccessIsAllowed()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			using (InvoicingPluginToConsol plugin = new InvoicingPluginToConsol(consol))
			{
				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.ARInvoices).IsAllowed = true;
				plugin.DiscardCurrentUserControl_ForTestOnly();
				AssertEquals("Security Panel should not be visible", false, ((ConsolInvoicePrintingUserControl)plugin.UserControl).ARJobInvoicingPrintingSecurityPanel.Visible);
			}
		}

		public void TestSecurityARJobInvoicingPrintingPanel_WhenAccessIsNotAllowed()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			using (InvoicingPluginToConsol plugin = new InvoicingPluginToConsol(consol))
			{
				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.ARInvoices).IsAllowed = false;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.ARInvoices);
				plugin.DiscardCurrentUserControl_ForTestOnly();
				AssertEquals("Content on security label is not correct.", expectedError, ((ConsolInvoicePrintingUserControl)plugin.UserControl).ARInvoicePrintingSecurityLabel.Text);
			}
		}

		public void TestSecurityAPJobInvoicingPrintingPanel_WhenAccessIsAllowed()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			using (InvoicingPluginToConsol plugin = new InvoicingPluginToConsol(consol))
			{
				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.APInvoices).IsAllowed = true;
				plugin.DiscardCurrentUserControl_ForTestOnly();
				AssertEquals("Security Panel should not be visible", false, ((ConsolInvoicePrintingUserControl)plugin.UserControl).APJobInvoicingPrintingSecurityPanel.Visible);
			}
		}

		public void TestSecurityAPJobInvoicingPrintingPanel_WhenAccessIsNotAllowed()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			using (InvoicingPluginToConsol plugin = new InvoicingPluginToConsol(consol))
			{
				plugin.SecurityHelper_ForTestOnly.GetInvSecurity(SecurityCore.APInvoices).IsAllowed = false;
				string expectedError = plugin.SecurityHelper_ForTestOnly.GetErrorTextForSecurityCheckPoint(SecurityCore.APInvoices);
				plugin.DiscardCurrentUserControl_ForTestOnly();
				AssertEquals("Content on security label is not correct.", expectedError, ((ConsolInvoicePrintingUserControl)plugin.UserControl).APInvoicePrintingSecurityLabel.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestConsolCanBeSavedWhenJobHasErrors()
		{
			SetUpJobHeaderCreationTest(true);

			try
			{
				var objectCreator = new TestObjectCreator(Factory);
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";
				var shipment = consol.Shipments.AddNew();
				var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
				job1.JH_ParentTableCode = "JS";
				job1.JH_ParentID = shipment.PK;
				job1.JH_GE = objectCreator.MiscDepartment.PK;
				job1.Validation.ValidateJH_GE();
				AssertEquals(true, job1.JH_GEInfo.HasErrors());

				var apps = new ApportionmentListing(Factory, consol);
				var cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = objectCreator.FRT.PK;
				cost.E6_RX_NKCurrency = objectCreator.USD.RX_Code;
				cost.E6_ExchangeRate = 1M;
				cost.E6_OSCostAmount = 100m;
				cost.E6_LocalCostAmount = 100m;
				cost.E6_ApportionmentMethod = "GWT";

				using (InvoicingPluginToConsol plugIn = new InvoicingPluginToConsol(consol))
				{
					plugIn.OnSaving();
					AssertEquals("Although the job has smth wrong ,it should not be deleted.", false, job1.IsDeleted);
					Factory.Save();
				}
			}
			finally
			{
				TearDownJobHeaderCreationTest();
			}
		}

		#region Test Objects

		public class TestInvoicingPluginToConsol : InvoicingPluginToConsol
		{
			public TestInvoicingPluginToConsol(IBusiness hostEntity)
				: base(hostEntity)
			{
			}

			protected JobCollection fJobs;
			public void SetJobs(JobCollection jobs)
			{
				fJobs = jobs;
			}

			protected override JobCollection Jobs
			{
				get { return fJobs; }
			}
		}
		#endregion
	}
}

using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing
{
	public abstract class JobBillingDefaultingManagerTest : TestCaseWithFactory
	{
		protected abstract JobBillingDefaultingManager GetDefaultingManager(IFactLoaderProvider factLoaderProvider);
		protected abstract string Context { get; }
		protected abstract Guid PK1 { get; }
		protected abstract Guid PK2 { get; }

		public void TestNullPlugin_NoDefaultValueSet()
		{
			var factLoaderProviderMock = new Mock<IFactLoaderProvider>();
			var defaultingManager = GetDefaultingManager(factLoaderProviderMock.Object);

			var companyMock = new Mock<ICompany>();
			var branchMock = new Mock<IBranch>();
			var departmentMock = new Mock<IDepartment>();
			defaultingManager.SetDefaultValue(null, companyMock.Object, branchMock.Object, departmentMock.Object);

			AssertNull(defaultingManager.DefaultValue);
		}

		public void TestNullInvoiceSupporter_NoDefaultValueSet()
		{
			var factLoaderProviderMock = new Mock<IFactLoaderProvider>();
			var defaultingManager = GetDefaultingManager(factLoaderProviderMock.Object);

			var jobPluginMock = new Mock<IJobInvoicingPlugIn>();
			jobPluginMock.Setup(x => x.InvoicingSupporter).Returns((IJobInvoicingSupporter)null);

			var companyMock = new Mock<ICompany>();
			var branchMock = new Mock<IBranch>();
			var departmentMock = new Mock<IDepartment>();
			defaultingManager.SetDefaultValue(jobPluginMock.Object, companyMock.Object, branchMock.Object, departmentMock.Object);

			AssertNull(defaultingManager.DefaultValue);
		}

		public void TestNullJob_NoDefaultValueSet()
		{
			var factLoaderMock = new Mock<IFactLoader>();
			var factLoaderProviderMock = new Mock<IFactLoaderProvider>();
			var defaultingManager = GetDefaultingManager(factLoaderProviderMock.Object);
			factLoaderProviderMock.Setup(x => x.GetFactLoader(It.IsAny<string>(), It.IsAny<RulesContextType>())).Returns(factLoaderMock.Object);

			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns((Job)null);
			invoicingSupporterMock.Setup(x => x.ConsumerType).Returns(JobInvoicingConsumerTypes.Shipment);

			var jobPluginMock = new Mock<IJobInvoicingPlugIn>();
			jobPluginMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var companyMock = new Mock<ICompany>();
			var branchMock = new Mock<IBranch>();
			var departmentMock = new Mock<IDepartment>();
			defaultingManager.SetDefaultValue(jobPluginMock.Object, companyMock.Object, branchMock.Object, departmentMock.Object);

			AssertNull(defaultingManager.DefaultValue);
		}

		public void TestNullConsumerType_NoDefaultValueSet()
		{
			var factLoaderProviderMock = new Mock<IFactLoaderProvider>();
			var defaultingManager = GetDefaultingManager(factLoaderProviderMock.Object);

			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.ConsumerType).Returns((JobInvoicingConsumerType)null);

			var jobPluginMock = new Mock<IJobInvoicingPlugIn>();
			jobPluginMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);

			var companyMock = new Mock<ICompany>();
			var branchMock = new Mock<IBranch>();
			var departmentMock = new Mock<IDepartment>();
			defaultingManager.SetDefaultValue(jobPluginMock.Object, companyMock.Object, branchMock.Object, departmentMock.Object);

			AssertNull(defaultingManager.DefaultValue);
		}

		public void TestDefaulting_SHP_EndToEnd()
		{
			var localClient = TestObjectCreator.ABIGAS;
			var subContext = RulesContextSubType.ShipmentJob.GetCode();

			var ruleSet1 = TestObjectCreator.CreateProductionRuleSet(Context, subContext, 1);
			TestObjectCreator.CreateProductionRules(ruleSet1, Context, 1, "LocalClient", localClient.PK, PK1);

			var ruleSet2 = TestObjectCreator.CreateProductionRuleSet(Context, subContext, 2, GlbCompany.CurrentCompany.PK);
			TestObjectCreator.CreateProductionRules(ruleSet2, Context, 2, "LocalClient", localClient.PK, PK2);

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001001", false);
			TestObjectCreator.CreateJob(shipment, false, localClientOrg: localClient);

			var factLoaderProvider = new FactLoaderProvider();
			var defaultingManager = GetDefaultingManager(factLoaderProvider);

			defaultingManager.SetDefaultValue(shipment, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment);

			AssertEquals(PK2, defaultingManager.DefaultValue);
		}

		public void TestDefaulting_SHP_EndToEnd_WithMultiLevelRules_UNLOCO()
		{
			var subContext = RulesContextSubType.ShipmentJob.GetCode();

			var ruleSet = TestObjectCreator.CreateProductionRuleSet(Context, subContext, 1);
			TestObjectCreator.CreateProductionRuleWithOriginCountryCode(ruleSet, Context, 1, "US", PK1);
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001001", origin: "USCHI", saveIt: false);
			TestObjectCreator.CreateJob(shipment, false);

			var factLoaderProvider = new FactLoaderProvider();
			var defaultingManager = GetDefaultingManager(factLoaderProvider);
			defaultingManager.SetDefaultValue(shipment, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment);

			AssertEquals(PK1, defaultingManager.DefaultValue);
		}

		public void TestDefaulting_SHP_EndToEnd_WithMultiLevelRules_Address()
		{
			var subContext = RulesContextSubType.ShipmentJob.GetCode();

			var ruleSet = TestObjectCreator.CreateProductionRuleSet(Context, subContext, 1);
			TestObjectCreator.CreateProductionRuleWithConsignorAddressCountryCode(ruleSet, Context, 1, "US", PK1);

			var usOrg = TestObjectCreator.CreateOrgHeader("USIMPRT", true, true, "USCHI");
			usOrg.MainAddress.OA_RN_NKCountryCode = "US";

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001001", false);
			shipment.ConsignorPK = usOrg.PK;
			TestObjectCreator.CreateJob(shipment, false);

			var factLoaderProvider = new FactLoaderProvider();
			var defaultingManager = GetDefaultingManager(factLoaderProvider);
			defaultingManager.SetDefaultValue(shipment, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment);

			AssertEquals(PK1, defaultingManager.DefaultValue);
		}

		public void TestDefaulting_NoRuleSet()
		{
			var shipment = TestObjectCreator.CreateShipment("S001001", false);
			TestObjectCreator.CreateJob(shipment, false, localClientOrg: TestObjectCreator.ABIGAS);

			var factLoaderProvider = new FactLoaderProvider();
			var defaultingManager = GetDefaultingManager(factLoaderProvider);

			defaultingManager.SetDefaultValue(shipment, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment);

			AssertEquals(null, defaultingManager.DefaultValue);
		}

		public void TestDefaulting_CON_EndToEnd()
		{
			var localClient = TestObjectCreator.ABIGAS;
			var subContext = RulesContextSubType.GatewayConsolJob.GetCode();

			var ruleSet1 = TestObjectCreator.CreateProductionRuleSet(Context, subContext, 1);
			TestObjectCreator.CreateProductionRules(ruleSet1, Context, 1, "LocalClient", localClient.PK, PK1);

			Factory.Save();

			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			TestObjectCreator.CreateJob(setup.gC0001, false, localClientOrg: localClient);

			var factLoaderProvider = new FactLoaderProvider();
			var defaultingManager = GetDefaultingManager(factLoaderProvider);

			defaultingManager.SetDefaultValue(setup.gC0001, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment);

			AssertEquals(PK1, defaultingManager.DefaultValue);
		}

		protected override void SetUp()
		{
			base.SetUp();

			Globals.IsUserInteractive = false;
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}

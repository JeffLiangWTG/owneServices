using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(JobChargesImporter))]
	public class JobChargesImporterTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			return new JobChargesImporter(line);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_innerValue ?? (TestObjectCreator_innerValue = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_innerValue;

		void AssertDoesChargeExist(IndependentChargeCollectionForBinding collection, OrgHeader creditor)
		{
			ZQuery chargeByCreditorFilter = creditor == null ?
				new ZQuery(JobChargeSchema.JR_OH_CostAccount, null) :
				new ZQuery(JobChargeSchema.JR_OH_CostAccount, creditor.PK);
			BusinessObject[] foundCharges = collection.Find(chargeByCreditorFilter);
			Assert(string.Format("Charge with {0} Creditor should be in collection.", creditor == null ? "empty" : (string)creditor.OH_Code), foundCharges.Length == 1);
		}

		#endregion

		public void TestChargeImporterDoesntCauseStackOverflow()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ZGuid chargeCode1 = creator.CC1.PK;
			ZGuid chargeCode2 = creator.CC2.PK;
			ZGuid chargeCode3 = creator.CC3.PK;

			IJobInvoicingPlugIn shipment1 = (IJobInvoicingPlugIn)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			IJobInvoicingPlugIn shipment2 = (IJobInvoicingPlugIn)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			Job shipment1job = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
			shipment1job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipment1job.JH_GB = GlbBranch.CurrentBranch.PK;

			Job shipment2job = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
			shipment2job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipment2job.JH_GB = GlbBranch.CurrentBranch.PK;

			for (int index = 0; index < 300; index++)
			{
				Charge charge1 = shipment1job.Charges.AddNew();
				charge1.JR_AC = chargeCode1;
				charge1.JR_OSCostAmt = 100m;

				Charge charge2 = shipment1job.Charges.AddNew();
				charge2.JR_AC = chargeCode2;
				charge2.JR_OSCostAmt = 50m;
			}

			Charge charge3 = shipment2job.Charges.AddNew();
			charge3.JR_AC = chargeCode3;
			charge3.JR_OSCostAmt = 200m;

			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			APInvoiceLine line1 = (APInvoiceLine)invoice.Lines.AddNew();
			line1.AL_AC = chargeCode1;
			line1.AL_JH = shipment1job.PK;
			JobChargesImporter importer = new JobChargesImporter(line1);
			AssertEquals(600, importer.JobChargesCollection.Count);
			var chargesToImport = importer.JobChargesCollection.ToArray<Charge>();
			importer.SetJobChargesToImport(chargesToImport);

			AssertEquals(600, invoice.Lines.Count);

			line1 = (APInvoiceLine)invoice.Lines.AddNew();
			line1.AL_JH = shipment2job.PK;
			importer = new JobChargesImporter(line1);
			AssertEquals(1, importer.JobChargesCollection.Count);
			chargesToImport = importer.JobChargesCollection.ToArray<Charge>();
			importer.SetJobChargesToImport(chargesToImport);

			AssertEquals(601, invoice.Lines.Count);
		}

		public void TestChargePopupEventWithUnpostedApportionment()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			ApportionmentListing appList = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = appList.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = TestObjectCreator.CC2.PK;
			cost.E6_OSCostAmount = 100m;
			cost.E6_ApportionmentMethod = "SHP";

			Factory.Save();

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			Job testJob = TestObjectCreator.CreateJob(shipment);

			Charge charge1 = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Test", TestObjectCreator.USD, 1M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			charge1.JR_JH = testJob.PK;
			charge1.JR_OH_CostAccount = ZGuid.Empty;

			Charge charge2 = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Test", TestObjectCreator.USD, 10M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			charge2.JR_JH = testJob.PK;
			charge2.JR_OH_CostAccount = ZGuid.Empty;

			Charge charge3 = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC2, "Test", TestObjectCreator.USD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			charge3.JR_JH = testJob.PK;
			charge3.JR_OH_CostAccount = ZGuid.Empty;
			charge3.JR_E6 = cost.PK;
			charge3.JR_LocalCostAmt = charge3.JR_OSCostAmt = 100m;
			Factory.Save();

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_JH = testJob.PK;

			JobChargesImporter chargesImporter = new JobChargesImporter(line);
			AssertEquals("Charges Collection passed should have 2 items", 2, chargesImporter.JobChargesCollection.Count);
		}

		public void TestGetChargesForCurrentShipment()
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			Job testJob = TestObjectCreator.CreateJob(shipment);
			Charge charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Test", TestObjectCreator.USD, 1M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			charge.JR_JH = testJob.PK;
			charge.JR_OH_CostAccount = ZGuid.Empty;
			charge.JR_LocalCostAmt = 100M;
			Factory.Save();

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_JH = testJob.PK;

			JobChargesImporter chargesImporter = new JobChargesImporter(line);

			AssertEquals("Should be 1 Charge in list", 1, chargesImporter.JobChargesCollection.Count);

			line.OriginalJobCharge = charge;
			chargesImporter = new JobChargesImporter(line);
			AssertEquals("Should be no one charge in list as first one has already been imported", 0, chargesImporter.JobChargesCollection.Count);
		}

		public void TestIncludeChargesForAllOtherCreditors()
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			Job testJob = TestObjectCreator.CreateJob(shipment);
			Charge charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Test", TestObjectCreator.USD, 1M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			charge.JR_JH = testJob.PK;
			charge.JR_OH_CostAccount = ZGuid.Empty;
			charge.JR_LocalCostAmt = 100M;

			charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Test", TestObjectCreator.USD, 1M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			charge.JR_JH = testJob.PK;
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge.JR_LocalCostAmt = 200M;

			charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Test", TestObjectCreator.USD, 1M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			charge.JR_JH = testJob.PK;
			charge.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_LocalCostAmt = 300M;

			charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Test", TestObjectCreator.USD, 1M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			charge.JR_JH = testJob.PK;
			charge.JR_OH_CostAccount = TestObjectCreator.Agent.PK;
			charge.JR_LocalCostAmt = 400M;

			Factory.Save();

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_JH = testJob.PK;

			JobChargesImporter chargesImporter = new JobChargesImporter(line);
			Assert("IncludeChargesForAllOtherCreditors should be unticked by default", !chargesImporter.IncludeChargesForAllOtherCreditors);
			AssertEquals("Should be Invoice Creditor and empty Creditor charges in list", 2, chargesImporter.JobChargesCollection.Count);
			AssertDoesChargeExist(chargesImporter.JobChargesCollection, TestObjectCreator.AALSHI);
			AssertDoesChargeExist(chargesImporter.JobChargesCollection, null);

			chargesImporter.IncludeChargesForAllOtherCreditors = true;
			AssertEquals("Should be all charges in list", 4, chargesImporter.JobChargesCollection.Count);

			AccountingConfigurationRegistry.Instance.IncludeChargesForAllOtherCreditors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			chargesImporter = new JobChargesImporter(line);
			AssertEquals("PreCondition: IncludeChargesForAllOtherCreditors should be ticked by default", true, chargesImporter.IncludeChargesForAllOtherCreditors);
			AssertEquals("Should be all charges in list", 4, chargesImporter.JobChargesCollection.Count);
			AssertDoesChargeExist(chargesImporter.JobChargesCollection, TestObjectCreator.AALSHI);
			AssertDoesChargeExist(chargesImporter.JobChargesCollection, TestObjectCreator.ABIGAS);
			AssertDoesChargeExist(chargesImporter.JobChargesCollection, TestObjectCreator.Agent);
			AssertDoesChargeExist(chargesImporter.JobChargesCollection, null);
		}

		public void TestIncludeChargesForCreditorsWithTheSameAPSettlementGroup()
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			Job testJob = TestObjectCreator.CreateJob(shipment);
			Charge charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Test", TestObjectCreator.USD, 1M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			charge.JR_JH = testJob.PK;
			charge.JR_OH_CostAccount = ZGuid.Empty;
			charge.JR_LocalCostAmt = 100M;

			charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Test", TestObjectCreator.USD, 1M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			charge.JR_JH = testJob.PK;
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge.JR_LocalCostAmt = 200M;

			charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Test", TestObjectCreator.USD, 1M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			charge.JR_JH = testJob.PK;
			charge.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_LocalCostAmt = 400M;

			charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Test", TestObjectCreator.USD, 1M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			charge.JR_JH = testJob.PK;
			charge.JR_OH_CostAccount = TestObjectCreator.Agent.PK;
			charge.JR_LocalCostAmt = 500M;

			TestObjectCreator.AALSHI.APSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.Agent.APSettlementGroupPK = TestObjectCreator.ABIGAS.PK;

			Factory.Save();

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_JH = testJob.PK;

			JobChargesImporter chargesImporter = new JobChargesImporter(line);
			Assert("IncludeChargesForAllOtherCreditors should be unticked by default", !chargesImporter.IncludeChargesForAllOtherCreditors);
			Assert("IncludeChargesForCreditorsWithTheSameAPSettlementGroup should be unticked by default", !chargesImporter.IncludeChargesForCreditorsWithTheSameAPSettlementGroup);
			AssertEquals("Should be Invoice Creditor and empty Creditor charges in list", 2, chargesImporter.JobChargesCollection.Count);
			AssertDoesChargeExist(chargesImporter.JobChargesCollection, TestObjectCreator.AALSHI);
			AssertDoesChargeExist(chargesImporter.JobChargesCollection, null);

			chargesImporter.IncludeChargesForCreditorsWithTheSameAPSettlementGroup = true;
			Assert("IncludeChargesForAllOtherCreditors should be unchecked when IncludeChargesForCreditorsWithTheSameAPSettlementGroup is checked.",
				!chargesImporter.IncludeChargesForAllOtherCreditors);
			AssertEquals("Should be only Invoice Creditor APSettlementGroup charges in list", 3, chargesImporter.JobChargesCollection.Count);
			AssertDoesChargeExist(chargesImporter.JobChargesCollection, TestObjectCreator.AALSHI);
			AssertDoesChargeExist(chargesImporter.JobChargesCollection, TestObjectCreator.Agent);
			AssertDoesChargeExist(chargesImporter.JobChargesCollection, null);

			AccountingConfigurationRegistry.Instance.IncludeChargesForCreditorsWithTheSameAPSettlementGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			chargesImporter = new JobChargesImporter(line);
			AssertEquals("PreCondition: IncludeChargesForCreditorsWithTheSameAPSettlementGroup should be ticked by default", true, chargesImporter.IncludeChargesForCreditorsWithTheSameAPSettlementGroup);
			AssertEquals("Should be only Invoice Creditor APSettlementGroup charges in list", 3, chargesImporter.JobChargesCollection.Count);
			AssertDoesChargeExist(chargesImporter.JobChargesCollection, TestObjectCreator.AALSHI);
			AssertDoesChargeExist(chargesImporter.JobChargesCollection, TestObjectCreator.Agent);
			AssertDoesChargeExist(chargesImporter.JobChargesCollection, null);
		}

		public void TestDBHitForChargeLoading()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001234");
			var testJob = TestObjectCreator.CreateJob(shipment);
			var charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Test", TestObjectCreator.USD, 1M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			charge.JR_JH = testJob.PK;
			charge.JR_OH_CostAccount = ZGuid.Empty;
			charge.JR_LocalCostAmt = 100M;
			Factory.Save();

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			var line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_JH = testJob.PK;

			var beforeLoadingDBHits = Factory.GetTableHitCount(JobChargeSchema.Constants.TableName);

			AccountingConfigurationRegistry.Instance.IncludeChargesForAllOtherCreditors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var chargesImporter = new JobChargesImporter(line);

			var afterLoadingDBHits = Factory.GetTableHitCount(JobChargeSchema.Constants.TableName);
			AssertEquals("should be only two DB hits - one for loading all job charges and another to load charges that are imported to incomplete invoices", 2, afterLoadingDBHits - beforeLoadingDBHits);
		}

		public void TestSetDefaultValues()
		{
			ARInvoiceLine line = Factory.NewWithValidTestData<ARInvoiceLine>();

			JobChargesImporter testObject = new JobChargesImporter(line);
			Assert("Default values according to PopupImportAccrualsScreenOnAPInvoice registry item sholud be set", !testObject.IncludeChargesForAllOtherCreditors);
			Assert("Default values according to PopupImportAccrualsScreenOnAPInvoice registry item sholud be set", !testObject.IncludeChargesForCreditorsWithTheSameAPSettlementGroup);

			AccountingConfigurationRegistry.Instance.IncludeChargesForAllOtherCreditors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			testObject = new JobChargesImporter(line);
			Assert("Default values according to PopupImportAccrualsScreenOnAPInvoice registry item sholud be set", testObject.IncludeChargesForAllOtherCreditors);
			Assert("Default values according to PopupImportAccrualsScreenOnAPInvoice registry item sholud be set", !testObject.IncludeChargesForCreditorsWithTheSameAPSettlementGroup);

			AccountingConfigurationRegistry.Instance.IncludeChargesForCreditorsWithTheSameAPSettlementGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			testObject = new JobChargesImporter(line);
			Assert("Default values according to PopupImportAccrualsScreenOnAPInvoice registry item sholud be set", !testObject.IncludeChargesForAllOtherCreditors);
			Assert("Default values according to PopupImportAccrualsScreenOnAPInvoice registry item sholud be set", testObject.IncludeChargesForCreditorsWithTheSameAPSettlementGroup);
		}

		public void TestGetCharges_WithNegativeAccrualsBehaviour()
		{
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Env.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			Job testJob = TestObjectCreator.CreateJob(shipment);
			Charge charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Test", TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			charge.JR_JH = testJob.PK;
			charge.JR_OH_CostAccount = ZGuid.Empty;
			charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Test", TestObjectCreator.AUD, -100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			charge.JR_JH = testJob.PK;
			charge.JR_OH_CostAccount = ZGuid.Empty;
			Factory.Save();

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_JH = testJob.PK;

			JobChargesImporter chargesImporter = new JobChargesImporter(line);
			AssertEquals("Only charges with positive cost amount must be in a list.", 1, chargesImporter.JobChargesCollection.Count);

			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Env.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			chargesImporter = new JobChargesImporter(line);
			AssertEquals("Charges with non zero cost amount must be in a list.", 2, chargesImporter.JobChargesCollection.Count);
		}

		public void TestJobChargesCollectionIncludeDifferentTaxBranchFromInvoice()
		{
			var taxBranch = TestObjectCreator.CreateBranch("BR1", "Branch1", GlbCompany.CurrentCompany);
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S00001234");
			var testJob = TestObjectCreator.CreateJob(shipment);
			var charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Test", TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			charge.JR_JH = testJob.PK;
			charge.JR_GB_CostTaxBranch = GlbBranch.CurrentBranch.PK;
			var charge1 = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Test", TestObjectCreator.AUD, 200M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			charge1.JR_JH = testJob.PK;
			charge1.JR_GB_CostTaxBranch = taxBranch.PK;
			Factory.Save();

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
			var line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_JH = testJob.PK;

			var chargesImporter = new JobChargesImporter(line);
			AssertEquals("Two charges will be in the list.", 2, chargesImporter.JobChargesCollection.Count);
			AssertCollectionContains(GlbBranch.CurrentBranch.PK, chargesImporter.JobChargesCollection.Select(x => x.JR_GB_CostTaxBranch));
			AssertCollectionContains(taxBranch.PK, chargesImporter.JobChargesCollection.Select(x => x.JR_GB_CostTaxBranch));
		}

		public void TestJobChargeFilterExcludesChargesThatAreAlreadyImportedToAnIncompleteInvoice()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001234");
			var testJob = TestObjectCreator.CreateJob(shipment);
			var charge1 = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Test", TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			charge1.JR_JH = testJob.PK;
			var charge2 = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Test", TestObjectCreator.AUD, 200M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			charge2.JR_JH = testJob.PK;
			Factory.Save();

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_JH = testJob.PK;

			var chargesImporter = new JobChargesImporter(line);
			AssertEquals("Two charges will be in the list.", 2, chargesImporter.JobChargesCollection.Count);
			AssertCollectionContains("Should contain charge1", charge1.PK, chargesImporter.JobChargesCollection.Select(x => x.PK));
			AssertCollectionContains("Should contain charge2", charge2.PK, chargesImporter.JobChargesCollection.Select(x => x.PK));

			var attribFactory = new BusinessObjectFactory();
			var dummyExistingInvoicePK = new ZGuid(Guid.NewGuid());
			var relaodedCost2 = attribFactory.Load<JobCharge>(charge2.PK);
			var attrib = relaodedCost2.JobChargeAttributes.AddNew();
			attrib.EC_Name = JobChargeAttribTypeList.Codes.LinkedToIncompleteInvoice;
			attrib.EC_Value = dummyExistingInvoicePK.ToString();
			attribFactory.Save();

			chargesImporter = new JobChargesImporter(line);
			AssertEquals("One charges will be in the list.", 1, chargesImporter.JobChargesCollection.Count);
			AssertCollectionContains("Should contain charge1", charge1.PK, chargesImporter.JobChargesCollection.Select(x => x.PK));
		}
	}
}

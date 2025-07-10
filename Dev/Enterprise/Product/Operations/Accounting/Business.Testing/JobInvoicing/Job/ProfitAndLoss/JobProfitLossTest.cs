using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Security;
using NUnit.Framework;
using static Enterprise.Accounting.Business.JobInvoicing.JobProfitLoss;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobProfitLoss))]
	public class JobProfitLossTest : NonPersistentBusinessObjectTestCase
	{
		public void TestJobPKsandCollections()
		{
			JobProfitLoss profitLoss = new JobProfitLoss(Factory);
			AssertNull(profitLoss.ManualJobPKs);
			AssertNull(profitLoss.Plugin);

			ProfitLossCollection coll = (ProfitLossCollection)profitLoss.ProfitLossDetails;
			ProfitLossSummaryCollection summaryColl = (ProfitLossSummaryCollection)profitLoss.ProfitLossSummaryDetails;
			AssertNull("No plugin or job pks yet", coll);
			AssertNull("No plugin or job pks yet", summaryColl);

			profitLoss.SetJobPKs(new ZGuid[] { ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid() });
			coll = (ProfitLossCollection)profitLoss.ProfitLossDetails;
			summaryColl = (ProfitLossSummaryCollection)profitLoss.ProfitLossSummaryDetails;
			AssertNotNull("Plugin can now be created as manual Job Pks specified", coll);
			AssertNotNull("Plugin can now be created as manual Job Pks specified", summaryColl);

			profitLoss.SetJobPKs(null);
			coll = (ProfitLossCollection)profitLoss.ProfitLossDetails;
			summaryColl = (ProfitLossSummaryCollection)profitLoss.ProfitLossSummaryDetails;
			AssertNull("No plugin or job pks yet", coll);
			AssertNull("No plugin or job pks yet", summaryColl);

			profitLoss.SetConsol(Factory.New<ForwardingConsol>());
			coll = (ProfitLossCollection)profitLoss.ProfitLossDetails;
			summaryColl = (ProfitLossSummaryCollection)profitLoss.ProfitLossSummaryDetails;
			AssertNotNull("Plugin can now be created as Consol specified", coll);
			AssertNotNull("Plugin can now be created as Consol specified", summaryColl);

			profitLoss.SetParent(Factory.New<ForwardingShipment>());
			GlobalProfitLossCollection globalCol = (GlobalProfitLossCollection)profitLoss.GlobalJobCostingProfitLoss;
			AssertNotNull("GlobalProfitLossCollection", globalCol);
		}

		public void TestFilteredCollections()
		{
			var profitLoss = new JobProfitLoss(Factory);

			var coll = (ProfitLossDetailCollectionView)profitLoss.ProfitLossFilteredDetails;
			var summaryColl = (ProfitLossSummaryCollectionView)profitLoss.ProfitLossSummaryFilteredDetails;
			AssertNull("No plugin or job pks yet", coll);
			AssertNull("No plugin or job pks yet", summaryColl);

			profitLoss.SetJobPKs(new ZGuid[] { ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid() });
			coll = (ProfitLossDetailCollectionView)profitLoss.ProfitLossFilteredDetails;
			summaryColl = (ProfitLossSummaryCollectionView)profitLoss.ProfitLossSummaryFilteredDetails;
			AssertNotNull("Plugin can now be created as manual Job Pks specified", coll);
			AssertNotNull("Plugin can now be created as manual Job Pks specified", summaryColl);

			profitLoss.SetJobPKs(null);
			coll = (ProfitLossDetailCollectionView)profitLoss.ProfitLossFilteredDetails;
			summaryColl = (ProfitLossSummaryCollectionView)profitLoss.ProfitLossSummaryFilteredDetails;
			AssertNull("No plugin or job pks yet", coll);
			AssertNull("No plugin or job pks yet", summaryColl);

			profitLoss.SetConsol(Factory.New<ForwardingConsol>());
			coll = (ProfitLossDetailCollectionView)profitLoss.ProfitLossFilteredDetails;
			summaryColl = (ProfitLossSummaryCollectionView)profitLoss.ProfitLossSummaryFilteredDetails;
			AssertNotNull("Plugin can now be created as Consol specified", coll);
			AssertNotNull("Plugin can now be created as Consol specified", summaryColl);
		}

		public void TestFilteredCollectionsWithSecuritySettings()
		{
			GlbStaff testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_IsController = false;

			GlbCompany testCompany = Factory.NewWithValidTestData<GlbCompany>();
			var testBranch = TestObjectCreator.CreateBranch("TBR", "Test Branch", GlbCompany.CurrentCompany);
			Factory.Save();

			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var consol = Factory.New<ForwardingConsol>();
				var testShipment1 = TestObjectCreator.CreateShipment("S00001234");
				var testJob1 = TestObjectCreator.CreateJob(testShipment1);
				var testCharge1 = TestObjectCreator.CreateCharge(testJob1, TestObjectCreator.CC1, 100m, 100m);
				var testCharge2 = TestObjectCreator.CreateCharge(testJob1, TestObjectCreator.CC2, 100m, 100m);
				consol.Shipments.Add(testShipment1);

				var apps = new ApportionmentListing(Factory, consol);
				var cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				cost.E6_LocalCostAmount = 200;
				cost.E6_OSCostAmount = 200;
				var profitLoss = new JobProfitLoss(Factory);
				profitLoss.SetConsol(consol);
				Factory.Save();

				BusinessObjectFactory securityFactory = new BusinessObjectFactory();

				SecurityCore security = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
				SecurityCore security2 = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, testBranch.PK.ToGuid(), Env.CurrentDepartment.PK);

				GlbSecurity loginSecurity = securityFactory.New<GlbSecurity>();
				loginSecurity.GU_GB = Env.CurrentBranch.PK;
				loginSecurity.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity.GU_GS = Env.CurrentUser.PK;
				loginSecurity.GU_SecurityRight = security.Login.Code;
				GlbSecurity loginSecurity2 = securityFactory.New<GlbSecurity>();
				loginSecurity2.GU_GB = testBranch.PK;
				loginSecurity2.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity2.GU_GS = Env.CurrentUser.PK;
				loginSecurity2.GU_SecurityRight = security2.Login.Code;

				GlbSecurity invoicingSecurity = securityFactory.New<GlbSecurity>();
				invoicingSecurity.GU_GB = Env.CurrentBranch.PK;
				invoicingSecurity.GU_GE = Env.CurrentDepartment.PK;
				invoicingSecurity.GU_GS = Env.CurrentUser.PK;
				invoicingSecurity.GU_SecurityRight = security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.AllowViewCosts).Code;

				loginSecurity.GU_SecurityItemIsAllowed = true;
				security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.AllowViewCosts).IsAllowed = true;
				security2.Login.IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = true;
				loginSecurity2.GU_SecurityItemIsAllowed = false;

				securityFactory.Save();

				Env.Security.ResetData(null, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid());

				AssertEquals("should show all charges", 4, profitLoss.ProfitLossDetails.Count);
				AssertEquals("should show all charges", 4, profitLoss.ProfitLossFilteredDetails.Count);
				AssertEquals("should show all charges", 2, profitLoss.ProfitLossSummaryDetails.Count);
				AssertEquals("should show all charges", 2, profitLoss.ProfitLossSummaryFilteredDetails.Count);

				loginSecurity.GU_SecurityItemIsAllowed = true;
				security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.AllowViewCosts).IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = false;
				securityFactory.Save();

				profitLoss.SetConsol(consol);
				AssertEquals("should show all charges", 4, profitLoss.ProfitLossDetails.Count);
				AssertEquals("should show all charges", 4, profitLoss.ProfitLossFilteredDetails.Count);
				AssertEquals("should show all charges", 2, profitLoss.ProfitLossSummaryDetails.Count);
				AssertEquals("should show all charges", 2, profitLoss.ProfitLossSummaryFilteredDetails.Count);

				testCharge1.JR_GB = testBranch.PK;
				Factory.ClearCachedValue<bool>(("Login BRN:" + testBranch.GB_Code + " DEP:" + GlbDepartment.CurrentDepartment.GE_Code));
				Factory.Save();
				profitLoss.SetConsol(consol);
				profitLoss.ProfitLossFilteredDetails.FilterProfitLossCollection();
				profitLoss.ProfitLossSummaryFilteredDetails.FilterProfitLossSummaryCollection();

				AssertEquals("should show all charges", 8, profitLoss.ProfitLossDetails.Count);
				AssertEquals("should show testCharge2", 6, profitLoss.ProfitLossFilteredDetails.Count);
				AssertEquals("should show all charges", 3, profitLoss.ProfitLossSummaryDetails.Count);
				AssertEquals("should show testCharge2", 2, profitLoss.ProfitLossSummaryFilteredDetails.Count);

				invoicingSecurity.GU_SecurityRight = security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowViewCharges).Code;
				security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowViewCharges).IsAllowed = true;
				securityFactory.Save();

				profitLoss.SetConsol(null);
				profitLoss.SetJobPKs(new ZGuid[] { testShipment1.Job.PK });
				profitLoss.ProfitLossFilteredDetails.FilterProfitLossCollection();
				profitLoss.ProfitLossSummaryFilteredDetails.FilterProfitLossSummaryCollection();

				AssertEquals("should show all charges", 8, profitLoss.ProfitLossDetails.Count);
				AssertEquals("should show all charges", 8, profitLoss.ProfitLossFilteredDetails.Count);
				AssertEquals("should show all charges", 3, profitLoss.ProfitLossSummaryDetails.Count);
				AssertEquals("should show all charges", 3, profitLoss.ProfitLossSummaryFilteredDetails.Count);

				security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowViewCharges).IsAllowed = false;
				securityFactory.Save();

				profitLoss.SetJobPKs(new ZGuid[] { testShipment1.Job.PK });
				profitLoss.ProfitLossFilteredDetails.FilterProfitLossCollection();
				profitLoss.ProfitLossSummaryFilteredDetails.FilterProfitLossSummaryCollection();

				AssertEquals("should show all charges", 8, profitLoss.ProfitLossDetails.Count);
				AssertEquals("should show testCharge2", 6, profitLoss.ProfitLossFilteredDetails.Count);
				AssertEquals("should show all charges", 3, profitLoss.ProfitLossSummaryDetails.Count);
				AssertEquals("should show testCharge2", 2, profitLoss.ProfitLossSummaryFilteredDetails.Count);
			}
		}

		public void TestAddJobPKsToDisplay()
		{
			JobProfitLoss profitLoss = new JobProfitLoss(Factory);
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job1 = creator.CreateJob(creator.AALSHI, 0m, null, 0m);
			Charge job1Charge = job1.Charges.AddNew();
			job1Charge.JR_AC = creator.CC2.PK;
			job1Charge.JR_OSSellAmt = 100m;

			Job job2 = creator.CreateJob(creator.AALSHI, 0m, null, 0m);
			Charge job2Charge = job2.Charges.AddNew();
			job2Charge.JR_AC = creator.CC2.PK;
			job2Charge.JR_OSSellAmt = 100m;
			Factory.Save();

			profitLoss.SetJobPKs(new ZGuid[] { job1.PK, job2.PK });

			ProfitLossCollection coll = (ProfitLossCollection)profitLoss.ProfitLossDetails;
			ProfitLossSummaryCollection summaryColl = (ProfitLossSummaryCollection)profitLoss.ProfitLossSummaryDetails;
			AssertNotNull("Plugin can now be created as manual Job Pks specified", coll);
			AssertNotNull("Plugin can now be created as manual Job Pks specified", summaryColl);
			AssertEquals(4, coll.Count);
			AssertEquals(2, summaryColl.Count);
		}

		public void TestReloadCollectionsAndRelatedProperties()
		{
			var consol = Factory.New<ForwardingConsol>();
			var testShipment1 = TestObjectCreator.CreateShipment("S00001234");
			var testJob1 = TestObjectCreator.CreateJob(testShipment1);
			var testCharge1 = TestObjectCreator.CreateCharge(testJob1, TestObjectCreator.CC1, 100m, 100m);
			var testCharge2 = TestObjectCreator.CreateCharge(testJob1, TestObjectCreator.CC2, 100m, 100m);
			consol.Shipments.Add(testShipment1);

			var apps = new ApportionmentListing(Factory, consol);
			var cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost.E6_LocalCostAmount = 200;
			cost.E6_OSCostAmount = 200;
			var profitLoss = new JobProfitLoss(Factory);
			profitLoss.SetConsol(consol);
			profitLoss.SetParent(testShipment1);
			Factory.Save();

			AssertEquals("should show all charges", 4, profitLoss.ProfitLossDetails.Count);
			AssertEquals("should show all charges", 4, profitLoss.ProfitLossFilteredDetails.Count);
			AssertEquals("should show all charges", 2, profitLoss.ProfitLossSummaryDetails.Count);
			AssertEquals("should show all charges", 2, profitLoss.ProfitLossSummaryFilteredDetails.Count);
			AssertEquals("should show all charges", 4, profitLoss.GlobalJobCostingProfitLoss.Count);

			profitLoss.Filter.ChargeCodeFilter = TestObjectCreator.CC1.AC_Code;
			profitLoss.ReloadCollectionsAndRelatedProperties(JobProfitLoss.JobProfitLossType.Summary);
			AssertEquals("should show all charges", 4, profitLoss.ProfitLossDetails.Count);
			AssertEquals("should show all charges", 4, profitLoss.ProfitLossFilteredDetails.Count);
			AssertEquals("Count shoud be reduced", 1, profitLoss.ProfitLossSummaryDetails.Count);
			Assert("should show only CC1 charges", profitLoss.ProfitLossSummaryDetails.All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
			AssertEquals("Count shoud be reduced", 1, profitLoss.ProfitLossSummaryFilteredDetails.Count);
			Assert("should show only CC1 charges", profitLoss.ProfitLossSummaryFilteredDetails.Cast<ProfitLossSummaryDetailView>().All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
			AssertEquals("should show all charges", 4, profitLoss.GlobalJobCostingProfitLoss.Count);

			profitLoss.ReloadCollectionsAndRelatedProperties(JobProfitLoss.JobProfitLossType.Details);
			AssertEquals("Count shoud be reduced", 2, profitLoss.ProfitLossDetails.Count);
			Assert("should show only CC1 charges", profitLoss.ProfitLossDetails.All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
			AssertEquals("Count shoud be reduced", 2, profitLoss.ProfitLossFilteredDetails.Count);
			Assert("should show only CC1 charges", profitLoss.ProfitLossFilteredDetails.Cast<ProfitLossDetailView>().All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
			AssertEquals("Count shoud be reduced", 1, profitLoss.ProfitLossSummaryDetails.Count);
			Assert("should show only CC1 charges", profitLoss.ProfitLossSummaryDetails.All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
			AssertEquals("Count shoud be reduced", 1, profitLoss.ProfitLossSummaryFilteredDetails.Count);
			Assert("should show only CC1 charges", profitLoss.ProfitLossSummaryFilteredDetails.Cast<ProfitLossSummaryDetailView>().All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
			AssertEquals("should show all charges", 4, profitLoss.GlobalJobCostingProfitLoss.Count);

			profitLoss.ReloadCollectionsAndRelatedProperties(JobProfitLoss.JobProfitLossType.Global);
			AssertEquals("Count shoud be reduced", 2, profitLoss.ProfitLossDetails.Count);
			Assert("should show only CC1 charges", profitLoss.ProfitLossDetails.All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
			AssertEquals("Count shoud be reduced", 2, profitLoss.ProfitLossFilteredDetails.Count);
			Assert("should show only CC1 charges", profitLoss.ProfitLossFilteredDetails.Cast<ProfitLossDetailView>().All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
			AssertEquals("Count shoud be reduced", 1, profitLoss.ProfitLossSummaryDetails.Count);
			Assert("should show only CC1 charges", profitLoss.ProfitLossSummaryDetails.All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
			AssertEquals("Count shoud be reduced", 1, profitLoss.ProfitLossSummaryFilteredDetails.Count);
			Assert("should show only CC1 charges", profitLoss.ProfitLossSummaryFilteredDetails.Cast<ProfitLossSummaryDetailView>().All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
			AssertEquals("Count shoud be reduced", 2, profitLoss.GlobalJobCostingProfitLoss.Count);
			Assert("should show only CC1 charges", profitLoss.GlobalJobCostingProfitLoss.All(x => x.ChargeCode.PK == TestObjectCreator.CC1.PK));
		}

		[SuspendCriticalValidation]
		public void TestTaxExpenseValues()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment = TestObjectCreator.CreateShipment("S001001", consol);
			var job = TestObjectCreator.CreateJob(shipment, createWithMutex: false);

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV10010", TestObjectCreator.AUD, 1M, 150M, 0M, 150M, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK);
			var revenueLine = arInvoice.Lines[0];
			revenueLine.AL_JH = job.PK;
			TestObjectCreator.CreateJobCharge(revenueLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "AP101", TestObjectCreator.AUD, 1M, 50M, 0M, 50M, 0M, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
			var costLine = apInvoice.Lines[0];
			costLine.AL_JH = job.PK;
			TestObjectCreator.CreateJobCharge(costLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			taxSystem.Name = "PBW - COMPANY LVL - NAT AUTH - OFT NEG";

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N01AU");
			taxAuthority.Name = "AU NATIONAL - ATO";

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -10M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(revenueLine));
			pivot1.ATP_IsTaxExpense = true;
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -3M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(revenueLine));
			pivot2.ATP_IsTaxExpense = true;

			var taxRecord3 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = costLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -15M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot3 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot3.ATP_ATT = taxRecord3.PK;
			pivot3.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(costLine));
			pivot3.ATP_IsTaxExpense = true;
			var taxRecord4 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = costLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -7M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot4 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot4.ATP_ATT = taxRecord4.PK;
			pivot4.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(costLine));
			pivot4.ATP_IsTaxExpense = true;

			Factory.Save();

			var profitLoss = new JobProfitLoss(Factory);
			profitLoss.SetParent(shipment);
			profitLoss.SetJobPKs(new[] { job.PK });

			AssertEquals(-13m, profitLoss.TotalTaxExpenseRevenue);
			AssertEquals(-22m, profitLoss.TotalTaxExpenseCost);
			AssertEquals(137m, profitLoss.TotalRevenueRecognized);
			AssertEquals(-72m, profitLoss.TotalCostRecognized);
			AssertEquals(137m, profitLoss.TotalRevenue);
			AssertEquals(-72m, profitLoss.TotalCost);
			AssertEquals(-35m, profitLoss.TotalLineAmountRecognized);
			AssertEquals(-35m, profitLoss.TotalLineAmount);

			AssertEquals(6, profitLoss.GlobalJobCostingProfitLoss.Count);
			profitLoss.GlobalJobCostingProfitLoss.ForEach(x => AssertEquals(-13m, x.TotalTaxExpenseRevenue));
			profitLoss.GlobalJobCostingProfitLoss.ForEach(x => AssertEquals(-22m, x.TotalTaxExpenseCost));
			profitLoss.GlobalJobCostingProfitLoss.ForEach(x => AssertEquals(137m, x.TotalRevenueRecognized));
			profitLoss.GlobalJobCostingProfitLoss.ForEach(x => AssertEquals(-72m, x.TotalCostRecognized));
			profitLoss.GlobalJobCostingProfitLoss.ForEach(x => AssertEquals(137m, x.TotalRevenue));
			profitLoss.GlobalJobCostingProfitLoss.ForEach(x => AssertEquals(-72m, x.TotalCost));
			profitLoss.GlobalJobCostingProfitLoss.ForEach(x => AssertEquals(-35m, x.TotalLineAmountRecognized));
			profitLoss.GlobalJobCostingProfitLoss.ForEach(x => AssertEquals(-35m, x.TotalLineAmount));

			taxConfig.ETC_ParentId = ZGuid.Empty;
			Assert(!profitLoss.IsTaxExpenseSupported);
		}

		#region IncludeDisbursementsPercentageMarginCalculationsRegistryTest

		public void TestMarginProfitRevAndCostRecognizedWithIncludeDisbursementsPercentageMarginCalculationsRegistry()
		{
			try
			{
				AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var shipmentRecognized = TestObjectCreator.CreateShipment("S001");
				var jobRecognized = TestObjectCreator.CreateJob(shipmentRecognized, false);
				//CC2 is disbursement charge code
				SetUpTestProfitLossInDB(jobRecognized, TestObjectCreator.CC2);
				Factory.Save();

				var profitLoss = new JobProfitLoss(Factory);
				profitLoss.SetJobPKs(new ZGuid[] { jobRecognized.PK });

				AssertEquals(true, AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.Value);
				AssertEquals("Should use disbursement charge in margin calculations", "123.68%", profitLoss.MarginProfitRevRecognized);
				AssertEquals("Should use disbursement charge in margin calculations", "123.68%", profitLoss.MarginProfitRev);
				AssertEquals("Should use disbursement charge in margin calculations", "-522.22%", profitLoss.MarginProfitCostRecognized);
				AssertEquals("Should use disbursement charge in margin calculations", "-522.22%", profitLoss.MarginProfitCost);

				AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				AssertEquals(false, AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.Value);
				AssertEquals("Should not use disbursement charge in margin calculations", "110.00%", profitLoss.MarginProfitRevRecognized);
				AssertEquals("Should not use disbursement charge in margin calculations", "110.00%", profitLoss.MarginProfitRev);
				AssertEquals("Should not use disbursement charge in margin calculations", "-1,100.00%", profitLoss.MarginProfitCostRecognized);
				AssertEquals("Should not use disbursement charge in margin calculations", "-1,100.00%", profitLoss.MarginProfitCost);
			}

			finally
			{
				AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			}
		}

		public void TestMarginProfitRevAndCostNotRecognizedWithIncludeDisbursementsPercentageMarginCalculationsRegistry()
		{
			try
			{
				AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				//to create unrecognized values
				var revRecOverride = TestObjectCreator.CC2.RevenueRecOverrides.AddNew();
				revRecOverride.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
				revRecOverride.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
				revRecOverride.Offset = 0;

				var shipmentNotRecognized = TestObjectCreator.CreateShipment("S002");
				var jobNotRecognized = TestObjectCreator.CreateJob(shipmentNotRecognized, false);
				//CC2 is disbursement charge code
				SetUpTestProfitLossInDB(jobNotRecognized, TestObjectCreator.CC2);
				Factory.Save();

				var profitLoss = new JobProfitLoss(Factory);
				profitLoss.SetJobPKs(new ZGuid[] { jobNotRecognized.PK });

				AssertEquals(true, AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.Value);
				AssertEquals("Should use disbursement charge in margin calculations", "175.00%", profitLoss.MarginProfitRevNotRecognized);
				AssertEquals("Should use disbursement charge in margin calculations", "123.68%", profitLoss.MarginProfitRev);
				AssertEquals("Should use disbursement charge in margin calculations", "-233.33%", profitLoss.MarginProfitCostNotRecognized);
				AssertEquals("Should use disbursement charge in margin calculations", "-522.22%", profitLoss.MarginProfitCost);

				AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				AssertEquals(false, AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.Value);
				AssertEquals("Should not use disbursement charge in margin calculations", "0.00%", profitLoss.MarginProfitRevNotRecognized);
				AssertEquals("Should not use disbursement charge in margin calculations", "110.00%", profitLoss.MarginProfitRev);
				AssertEquals("Should not use disbursement charge in margin calculations", "0.00%", profitLoss.MarginProfitCostNotRecognized);
				AssertEquals("Should not use disbursement charge in margin calculations", "-1,100.00%", profitLoss.MarginProfitCost);
			}

			finally
			{
				AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			}
		}

		public void TestMarginProfitRevAndCostRecognizedWithIncludeDisbursementsPercentageMarginCalculationsRegistryAndChargeCodeTypeOverrides()
		{
			try
			{
				AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var chargeOverride = TestObjectCreator.CC3.ChargeTypeOverrides.AddNew();
				chargeOverride.AN_ChargeType = Core.Constants.ChargeType.Disbursement;
				chargeOverride.AN_JobType = JobInvoicingConsumerTypes.Shipment.Code;
				chargeOverride.AN_JobDirection = "ALL";
				Factory.Save();

				var shipmentRecognized = TestObjectCreator.CreateShipment("S001");
				var jobRecognized = TestObjectCreator.CreateJob(shipmentRecognized, false);
				//CC3 is margin charge code with disbursement override for shipment
				SetUpTestProfitLossInDB(jobRecognized, TestObjectCreator.CC3);
				Factory.Save();

				var profitLoss = new JobProfitLoss(Factory);
				profitLoss.SetJobPKs(new ZGuid[] { jobRecognized.PK });

				AssertEquals(true, AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.Value);
				AssertEquals("Should not respect charge code overrides in margin calculations", "123.68%", profitLoss.MarginProfitRevRecognized);
				AssertEquals("Should not respect charge code overrides in margin calculations", "123.68%", profitLoss.MarginProfitRev);
				AssertEquals("Should not respect charge code overrides in margin calculations", "-522.22%", profitLoss.MarginProfitCostRecognized);
				AssertEquals("Should not respect charge code overrides in margin calculations", "-522.22%", profitLoss.MarginProfitCost);

				AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				AssertEquals(false, AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.Value);
				AssertEquals("Should respect charge code overrides in margin calculations", "110.00%", profitLoss.MarginProfitRevRecognized);
				AssertEquals("Should respect charge code overrides in margin calculations", "110.00%", profitLoss.MarginProfitRev);
				AssertEquals("Should respect charge code overrides in margin calculations", "-1,100.00%", profitLoss.MarginProfitCostRecognized);
				AssertEquals("Should respect charge code overrides in margin calculations", "-1,100.00%", profitLoss.MarginProfitCost);
			}

			finally
			{
				AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			}
		}

		public void TestMarginProfitRevAndCostNotRecognizedWithIncludeDisbursementsPercentageMarginCalculationsRegistryAndChargeCodeTypeOverrides()
		{
			try
			{
				AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var chargeOverride = TestObjectCreator.CC3.ChargeTypeOverrides.AddNew();
				chargeOverride.AN_ChargeType = Core.Constants.ChargeType.Disbursement;
				chargeOverride.AN_JobType = JobInvoicingConsumerTypes.Shipment.Code;
				chargeOverride.AN_JobDirection = "ALL";

				//to create unrecognized values
				var revRecOverride = TestObjectCreator.CC3.RevenueRecOverrides.AddNew();
				revRecOverride.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
				revRecOverride.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
				revRecOverride.Offset = 0;

				Factory.Save();

				var shipmentNotRecognized = TestObjectCreator.CreateShipment("S002");
				var jobNotRecognized = TestObjectCreator.CreateJob(shipmentNotRecognized, false);
				//CC3 is margin charge code with disbursement override for shipment
				SetUpTestProfitLossInDB(jobNotRecognized, TestObjectCreator.CC3);
				Factory.Save();

				var profitLoss = new JobProfitLoss(Factory);
				profitLoss.SetJobPKs(new ZGuid[] { jobNotRecognized.PK });

				AssertEquals(true, AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.Value);
				AssertEquals("Should not respect charge code overrides in margin calculations", "175.00%", profitLoss.MarginProfitRevNotRecognized);
				AssertEquals("Should not respect charge code overrides in margin calculations", "123.68%", profitLoss.MarginProfitRev);
				AssertEquals("Should not respect charge code overrides in margin calculations", "-233.33%", profitLoss.MarginProfitCostNotRecognized);
				AssertEquals("Should not respect charge code overrides in margin calculations", "-522.22%", profitLoss.MarginProfitCost);

				AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				AssertEquals("Should respect charge code overrides in margin calculations", "0.00%", profitLoss.MarginProfitRevNotRecognized);
				AssertEquals("Should respect charge code overrides in margin calculations", "110.00%", profitLoss.MarginProfitRev);
				AssertEquals("Should respect charge code overrides in margin calculations", "0.00%", profitLoss.MarginProfitCostNotRecognized);
				AssertEquals("Should respect charge code overrides in margin calculations", "-1,100.00%", profitLoss.MarginProfitCost);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			}
		}

		public void TestTotalAmounts()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			//CC2 is disbursement charge code
			SetUpTestProfitLossInDB(job, TestObjectCreator.CC2);
			Factory.Save();

			var profitLoss = new JobProfitLoss(Factory);
			profitLoss.SetJobPKs(new ZGuid[] { job.PK });

			AssertEquals(-110M, profitLoss.TotalAccrual);
			AssertEquals(-110M, profitLoss.TotalAccrualExcludingDSB);
			AssertEquals(180M, profitLoss.TotalWIP);
			AssertEquals(180M, profitLoss.TotalWIPExcludingDSB);
			AssertEquals(200M, profitLoss.TotalCost);
			AssertEquals(140M, profitLoss.TotalCostExcludingDSB);
			AssertEquals(200M, profitLoss.TotalRevenue);
			AssertEquals(120M, profitLoss.TotalRevenueExcludingDSB);
			AssertEquals(470M, profitLoss.TotalLineAmount);
			AssertEquals(330M, profitLoss.TotalLineAmountExcludingDSB);

			AssertEquals(-110M, profitLoss.TotalAccrualRecognized);
			AssertEquals(-110M, profitLoss.TotalAccrualRecognizedExcludingDSB);
			AssertEquals(180M, profitLoss.TotalWIPRecognized);
			AssertEquals(180M, profitLoss.TotalWIPRecognizedExcludingDSB);
			AssertEquals(200M, profitLoss.TotalCostRecognized);
			AssertEquals(140M, profitLoss.TotalCostRecognizedExcludingDSB);
			AssertEquals(200M, profitLoss.TotalRevenueRecognized);
			AssertEquals(120M, profitLoss.TotalRevenueRecognizedExcludingDSB);
			AssertEquals(470M, profitLoss.TotalLineAmountRecognized);
			AssertEquals(330M, profitLoss.TotalLineAmountRecognizedExcludingDSB);

			AssertEquals(0M, profitLoss.TotalAccrualNotRecognized);
			AssertEquals(0M, profitLoss.TotalAccrualNotRecognizedExcludingDSB);
			AssertEquals(0M, profitLoss.TotalWIPNotRecognized);
			AssertEquals(0M, profitLoss.TotalWIPNotRecognizedExcludingDSB);
			AssertEquals(0M, profitLoss.TotalCostNotRecognized);
			AssertEquals(0M, profitLoss.TotalCostNotRecognizedExcludingDSB);
			AssertEquals(0M, profitLoss.TotalRevenueNotRecognized);
			AssertEquals(0M, profitLoss.TotalRevenueNotRecognizedExcludingDSB);
			AssertEquals(0M, profitLoss.TotalLineAmountNotRecognized);
			AssertEquals(0M, profitLoss.TotalLineAmountNotRecognizedExcludingDSB);

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var charge3 = job.Charges.AddNew();
			//CC3 is Margin charge code
			charge3.JR_AC = TestObjectCreator.CC3.PK;
			charge3.JR_LocalCostAmt = 700M;
			charge3.JR_LocalSellAmt = 801M;
			Factory.Save();

			TestObjectCreator.CreateRevenueLineAndCharge(job.PK, 500, "ZZCC3");
			TestObjectCreator.CreateCostLineAndCharge(job.PK, 600, "ZZCC3");
			Factory.Save();

			profitLoss.ReloadCollectionsAndRelatedProperties(JobProfitLossType.Summary);

			AssertEquals(-110M + -700m, profitLoss.TotalAccrual);
			AssertEquals(-110M + -700m, profitLoss.TotalAccrualExcludingDSB);
			AssertEquals(180M + 801m, profitLoss.TotalWIP);
			AssertEquals(180M + 801m, profitLoss.TotalWIPExcludingDSB);
			AssertEquals(200M + -600m, profitLoss.TotalCost);
			AssertEquals(140M + -600m, profitLoss.TotalCostExcludingDSB);
			AssertEquals(200M + 500M, profitLoss.TotalRevenue);
			AssertEquals(120M + 500M, profitLoss.TotalRevenueExcludingDSB);
			AssertEquals(470M + 801m + -700m + 500M + -600M, profitLoss.TotalLineAmount);
			AssertEquals(330M + 801m + -700m + 500M + -600M, profitLoss.TotalLineAmountExcludingDSB);

			AssertEquals(-110M, profitLoss.TotalAccrualRecognized);
			AssertEquals(-110M, profitLoss.TotalAccrualRecognizedExcludingDSB);
			AssertEquals(180M, profitLoss.TotalWIPRecognized);
			AssertEquals(180M, profitLoss.TotalWIPRecognizedExcludingDSB);
			AssertEquals(200M, profitLoss.TotalCostRecognized);
			AssertEquals(140M, profitLoss.TotalCostRecognizedExcludingDSB);
			AssertEquals(200M, profitLoss.TotalRevenueRecognized);
			AssertEquals(120M, profitLoss.TotalRevenueRecognizedExcludingDSB);
			AssertEquals(470M, profitLoss.TotalLineAmountRecognized);
			AssertEquals(330M, profitLoss.TotalLineAmountRecognizedExcludingDSB);

			AssertEquals(-700M, profitLoss.TotalAccrualNotRecognized);
			AssertEquals(-700M, profitLoss.TotalAccrualNotRecognizedExcludingDSB);
			AssertEquals(801M, profitLoss.TotalWIPNotRecognized);
			AssertEquals(801M, profitLoss.TotalWIPNotRecognizedExcludingDSB);
			AssertEquals(-600M, profitLoss.TotalCostNotRecognized);
			AssertEquals(-600M, profitLoss.TotalCostNotRecognizedExcludingDSB);
			AssertEquals(500M, profitLoss.TotalRevenueNotRecognized);
			AssertEquals(500M, profitLoss.TotalRevenueNotRecognizedExcludingDSB);
			AssertEquals(801m + -700m + -600M + 500M, profitLoss.TotalLineAmountNotRecognized);
			AssertEquals(801m + -700m + -600M + 500M, profitLoss.TotalLineAmountNotRecognizedExcludingDSB);

			var charge4 = job.Charges.AddNew();
			//CC3 is Margin charge code
			charge4.JR_AC = TestObjectCreator.CC3.PK;
			charge4.JR_LocalCostAmt = 900M;
			charge4.JR_LocalSellAmt = 0M;
			Factory.Save();
			profitLoss.ResetCache();

			AssertNotNull(profitLoss.ProfitLossSummaryDetails);

			AssertEquals(-110M + -700m + -900m, profitLoss.TotalAccrual);
			AssertEquals(-110M + -700m + -900m, profitLoss.TotalAccrualExcludingDSB);
			AssertEquals(180M + 801m, profitLoss.TotalWIP);
			AssertEquals(180M + 801m, profitLoss.TotalWIPExcludingDSB);
			AssertEquals(200M + -600m, profitLoss.TotalCost);
			AssertEquals(140M + -600m, profitLoss.TotalCostExcludingDSB);
			AssertEquals(200M + 500M, profitLoss.TotalRevenue);
			AssertEquals(120M + 500M, profitLoss.TotalRevenueExcludingDSB);
			AssertEquals(470M + 801m + -700m + 500M + -600M + -900m, profitLoss.TotalLineAmount);
			AssertEquals(330M + 801m + -700m + 500M + -600M + -900m, profitLoss.TotalLineAmountExcludingDSB);

			AssertEquals(-110M, profitLoss.TotalAccrualRecognized);
			AssertEquals(-110M, profitLoss.TotalAccrualRecognizedExcludingDSB);
			AssertEquals(180M, profitLoss.TotalWIPRecognized);
			AssertEquals(180M, profitLoss.TotalWIPRecognizedExcludingDSB);
			AssertEquals(200M, profitLoss.TotalCostRecognized);
			AssertEquals(140M, profitLoss.TotalCostRecognizedExcludingDSB);
			AssertEquals(200M, profitLoss.TotalRevenueRecognized);
			AssertEquals(120M, profitLoss.TotalRevenueRecognizedExcludingDSB);
			AssertEquals(470M, profitLoss.TotalLineAmountRecognized);
			AssertEquals(330M, profitLoss.TotalLineAmountRecognizedExcludingDSB);

			AssertEquals(-700M + -900m, profitLoss.TotalAccrualNotRecognized);
			AssertEquals(-700M + -900m, profitLoss.TotalAccrualNotRecognizedExcludingDSB);
			AssertEquals(801M, profitLoss.TotalWIPNotRecognized);
			AssertEquals(801M, profitLoss.TotalWIPNotRecognizedExcludingDSB);
			AssertEquals(-600M, profitLoss.TotalCostNotRecognized);
			AssertEquals(-600M, profitLoss.TotalCostNotRecognizedExcludingDSB);
			AssertEquals(500M, profitLoss.TotalRevenueNotRecognized);
			AssertEquals(500M, profitLoss.TotalRevenueNotRecognizedExcludingDSB);
			AssertEquals(801m + -700m + -600M + 500M + -900m, profitLoss.TotalLineAmountNotRecognized);
			AssertEquals(801m + -700m + -600M + 500M + -900m, profitLoss.TotalLineAmountNotRecognizedExcludingDSB);
		}

		void SetUpTestProfitLossInDB(Job job, AccChargeCode chargeCode)
		{
			var aRInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARInv", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var aRInvoiceLine1 = TestObjectCreator.CreateARInvoiceLine(aRInvoice, job, TestObjectCreator.CC10, TestObjectCreator.AUD, 1m, "", 10m);
			aRInvoiceLine1.AL_LineAmount = 120m;
			aRInvoiceLine1.AL_OSAmount = aRInvoiceLine1.AL_LineAmount + aRInvoiceLine1.AL_GSTVAT;
			var aRInvoiceLine1JobCharge = TestObjectCreator.CreateJobCharge(aRInvoiceLine1, job, aRInvoiceLine1.ChargeCode, aRInvoiceLine1.TransactionCurrency);
			aRInvoiceLine1JobCharge.JR_OSCostAmt = 0;
			var aRInvoiceLine2 = TestObjectCreator.CreateARInvoiceLine(aRInvoice, job, chargeCode, TestObjectCreator.AUD, 1m, "", 10m);
			aRInvoiceLine2.AL_LineAmount = 80m;
			aRInvoiceLine2.AL_OSAmount = aRInvoiceLine2.AL_LineAmount + aRInvoiceLine2.AL_GSTVAT;
			var aRInvoiceLine2JobCharge = TestObjectCreator.CreateJobCharge(aRInvoiceLine2, job, aRInvoiceLine2.ChargeCode, aRInvoiceLine2.TransactionCurrency);
			aRInvoiceLine2JobCharge.JR_OSCostAmt = 0;

			var aPInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("APInv", TestObjectCreator.AUD, 1m, 0m, 0m, 0m, 0m, 0m, 0m);
			var aPInvoiceLine1 = aPInvoice.Lines[0];
			aPInvoiceLine1.AL_JH = job.PK;
			aPInvoiceLine1.AL_LineAmount = 140m;
			aPInvoiceLine1.AL_AC = TestObjectCreator.CC10.PK;
			aPInvoiceLine1.AL_OSAmount = aPInvoiceLine1.AL_LineAmount + aPInvoiceLine1.AL_GSTVAT;
			var aPInvoiceLine1JobCharge = TestObjectCreator.CreateJobCharge(aPInvoiceLine1, job, aPInvoiceLine1.ChargeCode, aPInvoiceLine1.TransactionCurrency);
			aPInvoiceLine1JobCharge.JR_OSSellAmt = 0;
			var aPInvoiceLine2 = TestObjectCreator.CreateAPInvoiceLine(aPInvoice, job, chargeCode, TestObjectCreator.AUD, 1m, "", 10m);
			aPInvoiceLine2.AL_JH = job.PK;
			aPInvoiceLine2.AL_LineAmount = 60m;
			aPInvoiceLine2.AL_OSAmount = aPInvoiceLine2.AL_LineAmount + aPInvoiceLine2.AL_GSTVAT;
			var aPInvoiceLine2JobCharge = TestObjectCreator.CreateJobCharge(aPInvoiceLine2, job, aPInvoiceLine2.ChargeCode, aPInvoiceLine2.TransactionCurrency);
			aPInvoiceLine2JobCharge.JR_OSSellAmt = 0;

			var charge1 = Factory.NewWithValidTestData<BaseCharge>();
			charge1.JR_JH = job.PK;

			var charge2 = Factory.NewWithValidTestData<BaseCharge>();
			charge2.JR_JH = job.PK;

			var wIP1 = Factory.New<WIP>();
			wIP1.AL_OSExTaxAmount = 130m;
			wIP1.AL_JH = job.PK;
			charge1.JR_AL_ARLine = wIP1.PK;

			var accrual1 = Factory.New<Accrual>();
			accrual1.AL_OSExTaxAmount = 80m;
			accrual1.AL_JH = job.PK;
			charge1.JR_AL_APLine = accrual1.PK;

			var wIP2 = Factory.New<WIP>();
			wIP2.AL_OSExTaxAmount = 50m;
			wIP2.AL_JH = job.PK;
			charge2.JR_AL_ARLine = wIP2.PK;

			var accrual2 = Factory.New<Accrual>();
			accrual2.AL_OSExTaxAmount = 30m;
			accrual2.AL_JH = job.PK;
			charge2.JR_AL_APLine = accrual2.PK;

			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP1);
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP2);
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(accrual1);
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(accrual2);
		}

		#endregion

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator
		{
			get
			{
				if (taxFrameworkTestObjectCreator == null)
				{
					taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory);
				}
				return taxFrameworkTestObjectCreator;
			}
		}
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}

		TestObjectCreator testObjectCreator;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new JobProfitLoss(Factory);
		}
	}
}

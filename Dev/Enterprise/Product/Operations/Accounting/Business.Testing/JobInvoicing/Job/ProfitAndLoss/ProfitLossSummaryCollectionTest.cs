using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class ProfitLossSummaryCollectionTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainFreeGSTTaxRegistryID, Env.CurrentCompanyPK);
			rate.SetRateNumerator_ForTestOnly(0);
			rate.Factory.Save();

			Consol = Factory.New<ForwardingConsol>();
			Ship1 = Consol.Shipments.AddNew();
			Ship2 = Consol.Shipments.AddNew();
			Ship3 = Consol.Shipments.AddNew();
			Ship4 = Consol.Shipments.AddNew();

			Ship1Job = Job.CreateWithMutex(Factory, Ship1);
			Ship1Job.JH_ParentTableCode = "JS";
			Ship1Job.JH_ParentID = Ship1.PK;
			Ship1Job.JH_GB = GlbBranch.CurrentBranch.PK;
			Ship1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Ship1Job.JH_JobNum = "111";

			Chrg1 = Ship1Job.Charges.AddNew();
			Chrg1.JR_AC = Env.Registry.FreightChargeCode;
			Chrg1.JR_LocalSellAmt = 300m;
			Chrg1.JR_LocalCostAmt = 100m;

			Ship2Job = Job.CreateWithMutex(Factory, Ship2);
			Ship2Job.JH_ParentTableCode = "JS";
			Ship2Job.JH_ParentID = Ship2.PK;
			Ship2Job.JH_GB = GlbBranch.CurrentBranch.PK;
			Ship2Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Ship2Job.JH_JobNum = "222";

			Chrg2 = Ship2Job.Charges.AddNew();
			Chrg2.JR_AC = Env.Registry.FreightChargeCode;
			Chrg2.JR_LocalSellAmt = 400m;
			Chrg2.JR_LocalCostAmt = 200m;

			Ship4Job = Job.CreateWithMutex(Factory, Ship4);
			Ship4Job.JH_ParentTableCode = "JS";
			Ship4Job.JH_ParentID = Ship4.PK;
			Ship4Job.JH_GB = GlbBranch.CurrentBranch.PK;
			Ship4Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Ship4Job.JH_JobNum = "444";
		}

		ForwardingConsol Consol;
		CommonShipment Ship1;
		CommonShipment Ship2;
		CommonShipment Ship3;
		CommonShipment Ship4;
		Job Ship1Job;
		Charge Chrg1;
		Job Ship2Job;
		Charge Chrg2;
		Job Ship4Job;

		[SuspendCriticalValidation]
		public void TestWarningMessageOnTotalLineAmount()
		{
			JobProfitLoss pL = new JobProfitLoss(Factory);
			pL.SetJobPKs(new ZGuid[] { Ship1Job.PK });
			pL.SetParent(Ship1);

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			var wip = testObjectCreator.CreateWIP(Ship1Job);

			Factory.Save();

			Assert("Precondition", !Job.HasOrphanWIPsorAccrualsForJobs(Factory, pL.JobPKs));
			pL.ProfitLossSummaryDetails.Load();
			AssertNoErrors(pL.ProfitLossSummaryDetails.ProfitLossParent.TotalLineAmountInfo);

			var charge = wip.LoadRelatedJobCharge();
			charge.SetARLineForcedForTest(Guid.Empty);
			Factory.Save();

			Assert("Precondition", Job.HasOrphanWIPsorAccrualsForJobs(Factory, pL.JobPKs));
			pL.ProfitLossSummaryDetails.Load();
			string expectedWarningMessage =
@"There are WIPs or Accruals linked to this Job which should be reversed but are not. As a result the Profit and Loss figure might not be accurate.
Please review the costs and revenues entered for this job.";
			AssertHasWarning(pL.ProfitLossSummaryDetails.ProfitLossParent.TotalLineAmountInfo, expectedWarningMessage);
		}

		public void TestLoad()
		{
			Factory.Save();
			var creator = new TestObjectCreator(Factory);

			var chargeCode = creator.CC1;
			var revRecOverride = chargeCode.RevenueRecOverrides.AddNew();
			revRecOverride.AE_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			revRecOverride.AE_Mode = "ALL";
			revRecOverride.AE_Direction = "ALL";
			revRecOverride.AE_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate;
			var charge4 = Ship4Job.Charges.AddNew();
			charge4.JR_AC = chargeCode.PK;
			charge4.JR_OSSellAmt = 100m;
			charge4.JR_OH_SellAccount = creator.ABIGAS.PK;
			charge4.JR_OSCostAmt = 0;
			Factory.Save();

			var postManager = new InvoicingPostManager(Ship4Job);
			postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);
			Factory.Save();

			AssertEquals("Should have created one invoice", 1, postManager.Poster.PostedInvoices.Count);
			AssertEquals("Invoice should have one line", 1, postManager.Poster.PostedInvoices[0].Lines.Count);
			AssertEquals("Recognition Date should be empty", ZDateTime.Empty, postManager.Poster.PostedInvoices[0].Lines[0].AL_ReverseDate);

			bool oldCreateWIPOrAccrualWhenNoInvoicesPosted = AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var ship3Job = Job.CreateWithMutex(Factory, Ship3);
				ship3Job.JH_ParentTableCode = "JS";
				ship3Job.JH_ParentID = Ship3.PK;
				ship3Job.JH_GB = GlbBranch.CurrentBranch.PK;
				ship3Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				ship3Job.JH_JobNum = "222";

				var chrg3 = ship3Job.Charges.AddNew();
				chrg3.JR_AC = Env.Registry.FreightChargeCode;
				chrg3.JR_LocalSellAmt = 555m;
				chrg3.JR_LocalCostAmt = 666m;
				Factory.Save();
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldCreateWIPOrAccrualWhenNoInvoicesPosted);
			}

			var pL = new JobProfitLoss(Factory);
			pL.SetConsol(Consol);
			AssertEquals("4 summary items exist in details - 1 for each charge", 4, pL.ProfitLossSummaryDetails.Count);

			bool charge1Found = false;
			bool charge2Found = false;
			bool charge3Found = false;
			bool charge4Found = false;

			foreach (ProfitLossSummaryDetail detail in pL.ProfitLossSummaryDetails)
			{
				if (detail.ZZ_Calc_Revenue == 100m && detail.ZZ_Calc_Revenue_NotRecognized == 100m)
				{
					AssertEquals(creator.CC1.PK, detail.ZZ_Calc_AC);
					AssertEquals(creator.CC1.AC_Desc, detail.ZZ_Calc_ChargeCodeDescription);
					charge4Found = true;
					AssertIsRecognized(detail, false);
				}
				else
				{
					AssertEquals(Env.Registry.FreightChargeCode, detail.ZZ_Calc_AC);
					AssertEquals("International Freight", detail.ZZ_Calc_ChargeCodeDescription);

					if (detail.ZZ_Calc_WIP == 300m && detail.ZZ_Calc_Accrual == -100m)
					{
						charge1Found = true;
						AssertIsRecognized(detail, true);
					}
					else if (detail.ZZ_Calc_WIP == 400m && detail.ZZ_Calc_Accrual == -200m)
					{
						charge2Found = true;
						AssertIsRecognized(detail, true);
					}
					else if (detail.ZZ_Calc_WIP == 555m && detail.ZZ_Calc_Accrual == -666m)
					{
						charge3Found = true;
						AssertIsRecognized(detail, false);
					}
				}
			}

			Assert("Charge 1 found", charge1Found);
			Assert("Charge 2 found", charge2Found);
			Assert("Charge 3 found", charge3Found);
			Assert("Charge 4 found", charge4Found);

			pL.Filter.ChargeCodeFilter = "BLAH!";
			pL.ProfitLossSummaryDetails.Load();
			AssertEquals("No items exist with that charge code", 0, pL.ProfitLossSummaryDetails.Count);

			AccChargeCode freightChargeCode = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);

			pL.Filter.ChargeCodeFilter = freightChargeCode.AC_Code;
			pL.ProfitLossSummaryDetails.Load();
			AssertEquals("Should be 3 items in result collection", 3, pL.ProfitLossSummaryDetails.Count);

			pL.Filter.ChargeCodeFilter = creator.CC1.AC_Code;
			pL.ProfitLossSummaryDetails.Load();
			AssertEquals("Should be 1 items in result collection", 1, pL.ProfitLossSummaryDetails.Count);

			pL.Filter.ChargeCodeFilter = ZString.Empty;
			pL.Filter.DepartmentFilter = ZGuid.NewZGuid();
			pL.ProfitLossSummaryDetails.Load();
			AssertEquals("No items exist with that department", 0, pL.ProfitLossSummaryDetails.Count);

			pL.Filter.DepartmentFilter = ZGuid.Empty;
			pL.Filter.BranchFilter = ZGuid.NewZGuid();
			pL.ProfitLossSummaryDetails.Load();
			AssertEquals("No items exist with that branch", 0, pL.ProfitLossSummaryDetails.Count);

			pL.Filter.BranchFilter = ZGuid.Empty;
			pL.ProfitLossSummaryDetails.Load();
			AssertEquals("4 items exist again", 4, pL.ProfitLossSummaryDetails.Count);

			pL.Filter.JobNumberFilter = Ship2Job.PK;
			pL.ProfitLossSummaryDetails.Load();
			AssertEquals("1 item exists", 1, pL.ProfitLossSummaryDetails.Count);

			pL.Filter.JobNumberFilter = ZGuid.Empty;
			pL.Filter.RecognizedChargesFilter = "ALL";
			pL.ProfitLossSummaryDetails.Load();
			AssertEquals("4 items exist again", 4, pL.ProfitLossSummaryDetails.Count);

			pL.Filter.RecognizedChargesFilter = "REC";
			pL.ProfitLossSummaryDetails.Load();
			AssertEquals("2 items exist", 2, pL.ProfitLossSummaryDetails.Count);

			pL.Filter.RecognizedChargesFilter = "NRC";
			pL.ProfitLossSummaryDetails.Load();
			AssertEquals("1 items exists", 2, pL.ProfitLossSummaryDetails.Count);
		}

		public void TestJobPKs()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			CommonShipment ship1 = consol.Shipments.AddNew();
			CommonShipment ship2 = consol.Shipments.AddNew();

			JobProfitLoss pL = new JobProfitLoss(Factory);
			ProfitLossSummaryCollection coll = new ProfitLossSummaryCollection(pL, consol);

			AssertEquals("No jobs", 0, coll.JobPKs.Length);

			Job ship1Job = Job.CreateWithMutex(Factory, ship1);
			ship1Job.JH_ParentTableCode = "JS";
			ship1Job.JH_ParentID = ship1.PK;
			ship1Job.JH_GB = GlbBranch.CurrentBranch.PK;
			ship1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			ship1Job.JH_JobNum = "111";
			Factory.Save();
			coll = new ProfitLossSummaryCollection(pL, consol);
			AssertEquals("1 job", 1, coll.JobPKs.Length);

			Job ship2Job = Job.CreateWithMutex(Factory, ship2);
			ship2Job.JH_ParentTableCode = "JS";
			ship2Job.JH_ParentID = ship2.PK;
			ship2Job.JH_GB = GlbBranch.CurrentBranch.PK;
			ship2Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			ship2Job.JH_JobNum = "222";
			Factory.Save();
			AssertEquals("2 jobs", 2, coll.JobPKs.Length);

			coll = new ProfitLossSummaryCollection(pL, new ZGuid[] { ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid() });
			AssertEquals("3 jobs", 3, coll.JobPKs.Length);
		}

		public void TestALLJobPKs()
		{
			var jobPKList = new List<ZGuid>();
			var profitLoss = new JobProfitLoss(Factory);
			Factory.Save();

			for (var i = 0; i < 2110; i++)
			{
				var id = Guid.NewGuid();
				jobPKList.Add(id);
			}

			var collection = new ProfitLossSummaryCollection(profitLoss, jobPKList.ToArray());
			AssertNoExceptionThrown(() => { collection.Load(); });
		}

		public void TestRevenueCostPostedButNotRecognised()
		{
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = "SHP";
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			Ship1Job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			Chrg1.JR_OH_SellAccount = testObjectCreator.ABIGAS.PK;
			Chrg1.JR_OH_CostAccount = testObjectCreator.AALSHI.PK;
			Chrg1.JR_APInvoiceNum = "TESTAPINV01245";
			Chrg1.JR_APInvoiceDate = ZDateTime.Now;
			Chrg1.JR_InvoiceType = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
			Chrg1.JR_LocalSellAmt = 100;
			Chrg1.JR_LocalCostAmt = 220;

			Factory.Save();

			InvoicingPostManager poster = new InvoicingPostManager(Ship1Job);
			poster.CreateTransactions(JobInvoicingPostingOption.All);

			AccTransactionLines line = Factory.NewWithValidTestData<ARInvoice>().Lines.AddNew();
			line.AL_OSAmount = line.AL_LineAmount = 33M;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_AG = testObjectCreator.GLHeader1.PK;

			Charge chrg3 = Ship1Job.Charges.AddNew();
			chrg3.JR_AC = testObjectCreator.CC1.PK;
			chrg3.JR_LocalSellAmt = 333;
			chrg3.JR_LocalCostAmt = 111m;
			chrg3.JR_AL_CFXLine = line.PK;

			Factory.Save();

			JobProfitLoss pL = new JobProfitLoss(Factory);
			pL.SetJobPKs(new ZGuid[] { Ship1Job.PK });
			ProfitLossSummaryCollection coll = new ProfitLossSummaryCollection(pL, new ZGuid[] { Ship1Job.PK });
			coll.Load();
			AssertEquals("PL summary collection should contain two records", 2, coll.Count);

			ProfitLossSummaryDetail freightElement = GetElementForCode(coll, Env.Registry.FreightChargeCode);
			AssertNotNull("1 records should be for the freight code", freightElement);

			bool recognizedValuesAreAllZero = freightElement.ZZ_Calc_Revenue_Recognized == 0;
			recognizedValuesAreAllZero &= freightElement.ZZ_Calc_WIP_Recognized == 0;
			recognizedValuesAreAllZero &= freightElement.ZZ_Calc_Cost_Recognized == 0;
			recognizedValuesAreAllZero &= freightElement.ZZ_Calc_Accrual_Recognized == 0;
			Assert("There should be no Recognised values", recognizedValuesAreAllZero);

			AssertEquals("Unrecognised WIP should be 0 as charge is posted", 0m, freightElement.ZZ_Calc_WIP_NotRecognized);
			AssertEquals("Unrecognised REV should be 100 as charge is posted", 100m, freightElement.ZZ_Calc_Revenue_NotRecognized);
			AssertEquals("Unrecognised ACR should be 0 as charge is posted", 0m, freightElement.ZZ_Calc_Accrual_NotRecognized);
			AssertEquals("Unrecognised CST should be 220 as charge is posted", -220m, freightElement.ZZ_Calc_Cost_NotRecognized);

			ProfitLossSummaryDetail element = GetElementForCode(coll, testObjectCreator.CC1.PK);
			AssertNotNull("1 records should be for the freight code", element);

			recognizedValuesAreAllZero = element.ZZ_Calc_Revenue_Recognized == 0;
			recognizedValuesAreAllZero &= element.ZZ_Calc_WIP_Recognized == 0;
			recognizedValuesAreAllZero &= element.ZZ_Calc_Cost_Recognized == 0;
			recognizedValuesAreAllZero &= element.ZZ_Calc_Accrual_Recognized == 0;
			Assert("There should be no Recognised values", recognizedValuesAreAllZero);

			AssertEquals("Unrecognised WIP should be 366 as CFX line Amount is 33", 366m, element.ZZ_Calc_WIP_NotRecognized);
			AssertEquals("Unrecognised Line Amount should be 255 as CFX line Amount is 33", 255m, element.ZZ_Calc_LineAmount_NotRecognized);
			AssertEquals("Unrecognised REV should be 0", 0m, element.ZZ_Calc_Revenue_NotRecognized);
			AssertEquals("Unrecognised ACR should be -111", -111m, element.ZZ_Calc_Accrual_NotRecognized);
			AssertEquals("Unrecognised CST should be 0", 0m, element.ZZ_Calc_Cost_NotRecognized);
		}

		[SuspendCriticalValidation]
		public void TestTaxExpense_NotTaxExpense_NoPivots()
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
			pivot1.ATP_IsTaxExpense = false;
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -3M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(revenueLine));
			pivot2.ATP_IsTaxExpense = false;

			Factory.Save();

			var profitLoss = new JobProfitLoss(Factory);
			profitLoss.SetParent(shipment);
			profitLoss.SetJobPKs(new[] { job.PK });

			AssertEquals(1, profitLoss.ProfitLossSummaryDetails.Count);
			var profitLossSummaryDetail = profitLoss.ProfitLossSummaryDetails[0];
			AssertEquals(0m, profitLossSummaryDetail.ZZ_Calc_TaxExpenseRevenue);
			AssertEquals(0m, profitLossSummaryDetail.ZZ_Calc_TaxExpenseCost);
			AssertEquals(150m, profitLossSummaryDetail.ZZ_Calc_Revenue_Recognized);
			AssertEquals(-50m, profitLossSummaryDetail.ZZ_Calc_Cost_Recognized);
			AssertEquals(150m, profitLossSummaryDetail.ZZ_Calc_Revenue);
			AssertEquals(-50m, profitLossSummaryDetail.ZZ_Calc_Cost);
			AssertEquals(0m, profitLossSummaryDetail.ZZ_Calc_LineAmount_Recognized);
			AssertEquals(0m, profitLossSummaryDetail.ZZ_Calc_LineAmount);
		}

		[SuspendCriticalValidation]
		public void TestTaxExpense_IsTaxExpense()
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

			AssertEquals(1, profitLoss.ProfitLossSummaryDetails.Count);
			var profitLossSummaryDetail = profitLoss.ProfitLossSummaryDetails[0];
			AssertEquals(-13m, profitLossSummaryDetail.ZZ_Calc_TaxExpenseRevenue);
			AssertEquals(-22m, profitLossSummaryDetail.ZZ_Calc_TaxExpenseCost);
			AssertEquals(137m, profitLossSummaryDetail.ZZ_Calc_Revenue_Recognized);
			AssertEquals(-72m, profitLossSummaryDetail.ZZ_Calc_Cost_Recognized);
			AssertEquals(137m, profitLossSummaryDetail.ZZ_Calc_Revenue);
			AssertEquals(-72m, profitLossSummaryDetail.ZZ_Calc_Cost);
			AssertEquals(-35m, profitLossSummaryDetail.ZZ_Calc_LineAmount_Recognized);
			AssertEquals(-35m, profitLossSummaryDetail.ZZ_Calc_LineAmount);
		}

		[SuspendCriticalValidation]
		public void TestTaxExpense_IsTaxExpense_AndNotTaxExpense()
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
			pivot2.ATP_IsTaxExpense = false;

			var taxRecord3 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = costLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -15M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot3 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot3.ATP_ATT = taxRecord3.PK;
			pivot3.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(costLine));
			pivot3.ATP_IsTaxExpense = false;
			var taxRecord4 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = costLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -7M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot4 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot4.ATP_ATT = taxRecord4.PK;
			pivot4.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(costLine));
			pivot4.ATP_IsTaxExpense = true;

			Factory.Save();

			var profitLoss = new JobProfitLoss(Factory);
			profitLoss.SetParent(shipment);
			profitLoss.SetJobPKs(new[] { job.PK });

			AssertEquals(1, profitLoss.ProfitLossSummaryDetails.Count);
			var profitLossSummaryDetail = profitLoss.ProfitLossSummaryDetails[0];
			AssertEquals(-10m, profitLossSummaryDetail.ZZ_Calc_TaxExpenseRevenue);
			AssertEquals(-7m, profitLossSummaryDetail.ZZ_Calc_TaxExpenseCost);
			AssertEquals(140m, profitLossSummaryDetail.ZZ_Calc_Revenue_Recognized);
			AssertEquals(-57m, profitLossSummaryDetail.ZZ_Calc_Cost_Recognized);
			AssertEquals(140m, profitLossSummaryDetail.ZZ_Calc_Revenue);
			AssertEquals(-57m, profitLossSummaryDetail.ZZ_Calc_Cost);
			AssertEquals(-17m, profitLossSummaryDetail.ZZ_Calc_LineAmount_Recognized);
			AssertEquals(-17m, profitLossSummaryDetail.ZZ_Calc_LineAmount);
		}

		#region Implementation

		ProfitLossSummaryDetail GetElementForCode(ProfitLossSummaryCollection pLCollection, ZGuid codeToFind)
		{
			ProfitLossSummaryDetail result = null;
			foreach (ProfitLossSummaryDetail element in pLCollection)
			{
				if (element.ZZ_Calc_AC == codeToFind)
				{
					result = element;
					break;
				}
			}
			return result;
		}

		void AssertIsRecognized(ProfitLossSummaryDetail detail, bool isRecognized)
		{
			if (isRecognized)
			{
				AssertEquals("REV", detail.ZZ_Calc_Revenue_Recognized, detail.ZZ_Calc_Revenue);
				AssertEquals("ACR", detail.ZZ_Calc_Accrual_Recognized, detail.ZZ_Calc_Accrual);
				AssertEquals("CST", detail.ZZ_Calc_Cost_Recognized, detail.ZZ_Calc_Cost);
				AssertEquals("WIP", detail.ZZ_Calc_WIP_Recognized, detail.ZZ_Calc_WIP);
			}
			else
			{
				AssertEquals("REV", detail.ZZ_Calc_Revenue_NotRecognized, detail.ZZ_Calc_Revenue);
				AssertEquals("ACR", detail.ZZ_Calc_Accrual_NotRecognized, detail.ZZ_Calc_Accrual);
				AssertEquals("CST", detail.ZZ_Calc_Cost_NotRecognized, detail.ZZ_Calc_Cost);
				AssertEquals("WIP", detail.ZZ_Calc_WIP_NotRecognized, detail.ZZ_Calc_WIP);
			}
		}

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

		#endregion
	}
}

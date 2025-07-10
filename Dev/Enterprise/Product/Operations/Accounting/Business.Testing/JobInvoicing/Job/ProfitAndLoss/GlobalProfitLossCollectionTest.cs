using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class GlobalProfitLossCollectionTest : TestCaseWithFactory
	{
		[SuspendGLAccountAndChargeCodeCriticalValidation]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestLoad()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			CommonShipment ship1 = consol.Shipments.AddNew();
			CommonShipment ship2 = consol.Shipments.AddNew();
			CommonShipment ship3 = consol.Shipments.AddNew();

			Job ship1Job = Job.CreateWithMutex(Factory, ship1);
			ship1Job.JH_GB = GlbBranch.CurrentBranch.PK;
			ship1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			ship1Job.JH_JobNum = "111";

			Charge chrg1 = ship1Job.Charges.AddNew();
			chrg1.JR_AC = Env.Registry.FreightChargeCode;
			chrg1.JR_LocalSellAmt = 300m;
			chrg1.JR_LocalCostAmt = 100m;

			Job ship2Job = Job.CreateWithMutex(Factory, ship2);
			ship2Job.JH_GB = GlbBranch.CurrentBranch.PK;
			ship2Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			ship2Job.JH_JobNum = "222";

			Charge chrg2 = ship2Job.Charges.AddNew();
			chrg2.JR_AC = Env.Registry.FreightChargeCode;
			chrg2.JR_LocalSellAmt = 400m;
			chrg2.JR_LocalCostAmt = 200m;

			ZDateTime now = ZDateTime.Now;
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			chrg1.CreateCostTransactionLine(invoice, ZDateTime.Now);

			JCJournalHeader cFXHeader = Factory.NewWithValidTestData<JCJournalHeader>();
			chrg1.CreateCFXTransactionLine(cFXHeader, ZDateTime.Now);
			chrg1.CFXLine.AL_OSAmount = chrg1.CFXLine.AL_LineAmount = 500m;
			chrg2.CreateCFXTransactionLine(cFXHeader, ZDateTime.Now);
			chrg2.CFXLine.AL_OSAmount = chrg2.CFXLine.AL_LineAmount = 600m;

			ZDBOnlyQuery branchQuery = new ZDBOnlyQuery(typeof(GlbBranch)) { OrderBy = GlbBranchSchema.GB_Code.Name }; // To avoid picking up unsaved Branches from the current Factory
			branchQuery.AddToFilter(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);

			GlbBranch otherBranch = Factory.LoadTop1<GlbBranch>(branchQuery);
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, otherBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Job oShip1Job = Job.CreateWithMutex(Factory, ship1);
				oShip1Job.JH_GB = GlbBranch.CurrentBranch.PK;
				oShip1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				oShip1Job.JH_JobNum = "o111";

				Charge oChrg1 = oShip1Job.Charges.AddNew();
				oChrg1.JR_AC = Env.Registry.FreightChargeCode;
				oChrg1.JR_LocalSellAmt = 301m;
				oChrg1.JR_LocalCostAmt = 101m;

				Job oShip2Job = Job.CreateWithMutex(Factory, ship2);
				oShip2Job.JH_GB = GlbBranch.CurrentBranch.PK;
				oShip2Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				oShip2Job.JH_JobNum = "o222";

				Charge oChrg2 = oShip2Job.Charges.AddNew();
				oChrg2.JR_AC = Env.Registry.FreightChargeCode;
				oChrg2.JR_LocalSellAmt = 401m;
				oChrg2.JR_LocalCostAmt = 201m;

				APInvoice oInvoice = Factory.NewWithValidTestData<APInvoice>();
				oChrg1.CreateCostTransactionLine(oInvoice, ZDateTime.Now);

				JCJournalHeader oCFXHeader = Factory.NewWithValidTestData<JCJournalHeader>();
				oChrg1.CreateCFXTransactionLine(oCFXHeader, ZDateTime.Now);
				oChrg1.CFXLine.AL_OSAmount = oChrg1.CFXLine.AL_LineAmount = 501m;
				oChrg2.CreateCFXTransactionLine(oCFXHeader, ZDateTime.Now);
				oChrg2.CFXLine.AL_OSAmount = oChrg2.CFXLine.AL_LineAmount = 601m;
			}

			Factory.Save();

			bool oldCreateWIPOrAccrualWhenNoInvoicesPosted = AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				Job ship3Job = Job.CreateWithMutex(Factory, ship3);
				ship3Job.JH_GB = GlbBranch.CurrentBranch.PK;
				ship3Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				ship3Job.JH_JobNum = "222";

				AccTransactionLines line = Factory.NewWithValidTestData<ARInvoice>().Lines.AddNew();
				line.AL_OSAmount = line.AL_LineAmount = 33M;
				line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;

				Charge chrg3 = ship3Job.Charges.AddNew();
				chrg3.JR_AC = Env.Registry.FreightChargeCode;
				chrg3.JR_LocalSellAmt = 555m;
				chrg3.JR_LocalCostAmt = 666m;
				chrg3.JR_AL_CFXLine = line.PK;

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, otherBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					Job oShip3Job = Job.CreateWithMutex(Factory, ship3);
					oShip3Job.JH_GB = GlbBranch.CurrentBranch.PK;
					oShip3Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
					oShip3Job.JH_JobNum = "222";

					Charge oChrg3 = oShip3Job.Charges.AddNew();
					oChrg3.JR_AC = Env.Registry.FreightChargeCode;
					oChrg3.JR_LocalSellAmt = 555m;
					oChrg3.JR_LocalCostAmt = 666m;
				}

				Factory.Save();
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldCreateWIPOrAccrualWhenNoInvoicesPosted);
			}

			JobProfitLoss pL = new JobProfitLoss(Factory);
			pL.SetConsol(consol);
			pL.SetParent(ship1);
			AssertEquals("8 items exist in details - 2 WIPs, 1 ACR, 1 CST, 2 REVs", 8, pL.ProfitLossDetails.Count);
			AssertEquals("6 items exist in details - 2 WIPs, 1 ACR, 1 CST, 2 REVs", 6, pL.GlobalJobCostingProfitLoss.Count);

			bool charge1WIPFound = false;
			bool charge1ACRFound = false;
			bool charge1CSTFound = false;
			bool charge1REVFound = false;
			bool charge2WIPFound = false;
			bool charge2ACRFound = false;
			bool charge2CSTFound = false;
			bool charge2REVFound = false;
			bool charge3WIPFound = false;
			bool charge3ACRFound = false;

			BusinessObjectFactory tempFactory = new BusinessObjectFactory();
			foreach (ProfitLossDetail detail in pL.ProfitLossDetails)
			{
				AssertEquals(Env.Registry.FreightChargeCode, detail.ZY_Calc_AC);
				AssertEquals("International Freight", detail.ZY_Calc_ChargeCodeDescription);

				if (detail.ZY_Calc_AH.IsValid)
				{
					AccTransactionHeader transHeader = tempFactory.Load<AccTransactionHeader>(detail.ZY_Calc_AH);
					AssertEquals("PostDate", transHeader.AH_PostDate, detail.ZY_Calc_PostDate);
					AssertEquals("RecognizedDate", transHeader.AH_PostDate, detail.ZY_Calc_RecognizedDate);
					AssertEquals("InvoiceDate", transHeader.AH_InvoiceDate, detail.ZY_Calc_InvoiceDate);
					AssertEquals("FullyPaidDate", transHeader.AH_FullyPaidDate, detail.ZY_Calc_FullyPaidDate);
					AssertEquals("Recognition Type", "IMM", detail.ZY_Calc_RecognitionType);
					AssertEquals("Reversal Date", ZDateTime.Empty, detail.ZY_Calc_ReversalDate);
					AssertEquals("Audit Details", transHeader.AH_SystemCreateTimeUtc, detail.ZY_Calc_SystemCreateTime);
				}
				else if (detail.ZY_Calc_AL.IsValid)
				{
					AccTransactionLines transLine = tempFactory.Load<AccTransactionLines>(detail.ZY_Calc_AL);
					ZDateTime postDate = (transLine.AL_LineAmount == detail.ZY_Calc_LineAmount) ? transLine.AL_ReverseDate : transLine.AL_PostDate;
					AssertEquals("PostDate", postDate, detail.ZY_Calc_PostDate);
					AssertEquals("RecognizedDate", postDate, detail.ZY_Calc_RecognizedDate);
					AssertEquals("InvoiceDate", transLine.AL_PostDate, detail.ZY_Calc_InvoiceDate);
				}

				if (detail.ZY_Calc_LineAmount == 800m && detail.ZY_Calc_LineType == "WIP" && detail.ZY_Calc_Ledger == "JC")
				{
					charge1WIPFound = true;
				}
				else if (detail.ZY_Calc_LineAmount == -100m)
				{
					if (detail.ZY_Calc_LineType == "ACR" && detail.ZY_Calc_Ledger == "JC")
					{
						charge1ACRFound = true;
					}
					else if (detail.ZY_Calc_LineType == "CST")
					{
						charge1CSTFound = true;
					}
				}
				else if (detail.ZY_Calc_LineAmount == 1000m && detail.ZY_Calc_LineType == "WIP" && detail.ZY_Calc_Ledger == "JC")
				{
					charge2WIPFound = true;
				}
				else if (detail.ZY_Calc_LineAmount == 500m && detail.ZY_Calc_LineType == "REV")
				{
					charge1REVFound = true;
				}
				else if (detail.ZY_Calc_LineAmount == -200m)
				{
					if (detail.ZY_Calc_LineType == "ACR" && detail.ZY_Calc_Ledger == "JC")
					{
						charge2ACRFound = true;
					}
					else if (detail.ZY_Calc_LineType == "CST")
					{
						charge2CSTFound = true;
					}
				}
				else if (detail.ZY_Calc_LineAmount == 600m && detail.ZY_Calc_LineType == "REV")
				{
					charge2REVFound = true;
				}
				else if (detail.ZY_Calc_LineAmount == 588m && detail.ZY_Calc_LineType == "WIP" && detail.ZY_Calc_Ledger.IsEmpty)
				{
					charge3WIPFound = true;
				}
				else if (detail.ZY_Calc_LineAmount == -666m && detail.ZY_Calc_LineType == "ACR" && detail.ZY_Calc_Ledger.IsEmpty)
				{
					charge3ACRFound = true;
				}
			}

			Assert("Charge1WIPFound", charge1WIPFound);
			Assert("!Charge1ACRFound", !charge1ACRFound);
			Assert("Charge1CSTFound", charge1CSTFound);
			Assert("Charge1REVFound", charge1REVFound);
			Assert("Charge2WIPFound", charge2WIPFound);
			Assert("Charge2CSTFound", !charge2CSTFound);
			Assert("Charge2REVFound", charge2REVFound);
			Assert("Charge2ACRFound", charge2ACRFound);
			Assert("Charge3WIPFound", charge3WIPFound);
			Assert("Charge3ACRFound", charge3ACRFound);

			pL.Filter.ChargeCodeFilter = "BLAH!";
			pL.ProfitLossDetails.Load();
			AssertEquals("No items exist with that charge code", 0, pL.ProfitLossDetails.Count);

			AccChargeCode freightChargeCode = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);

			pL.Filter.ChargeCodeFilter = freightChargeCode.AC_Code;
			pL.ProfitLossDetails.Load();
			AssertEquals("Should be 8 items in result collection", 8, pL.ProfitLossDetails.Count);

			pL.Filter.ChargeCodeFilter = ZString.Empty;
			pL.Filter.DepartmentFilter = ZGuid.NewZGuid();
			pL.ProfitLossDetails.Load();
			AssertEquals("No items exist with that department", 0, pL.ProfitLossDetails.Count);

			pL.Filter.DepartmentFilter = ZGuid.Empty;
			pL.Filter.BranchFilter = ZGuid.NewZGuid();
			pL.ProfitLossDetails.Load();
			AssertEquals("No items exist with that branch", 0, pL.ProfitLossDetails.Count);

			pL.Filter.BranchFilter = ZGuid.Empty;
			pL.ProfitLossDetails.Load();
			AssertEquals("8 items exist again", 8, pL.ProfitLossDetails.Count);

			pL.Filter.JobNumberFilter = ship2Job.PK;
			pL.ProfitLossDetails.Load();
			AssertEquals("3 items exist", 3, pL.ProfitLossDetails.Count);

			pL.Filter.JobNumberFilter = ZGuid.Empty;
			pL.Filter.RecognizedChargesFilter = "ALL";
			pL.ProfitLossDetails.Load();
			AssertEquals("8 items exist again", 8, pL.ProfitLossDetails.Count);

			pL.Filter.RecognizedChargesFilter = "REC";
			pL.ProfitLossDetails.Load();
			AssertEquals("6 items exist", 6, pL.ProfitLossDetails.Count);

			pL.Filter.RecognizedChargesFilter = "NRC";
			pL.ProfitLossDetails.Load();
			AssertEquals("2 items exists", 2, pL.ProfitLossDetails.Count);

			AssertGlobalTotals(pL.GlobalJobCostingProfitLoss[0], 1200, 0, -100, 500, 800);
			AssertGlobalTotals(pL.GlobalJobCostingProfitLoss[1], 1200, 0, -100, 500, 800);
			AssertGlobalTotals(pL.GlobalJobCostingProfitLoss[2], 1200, 0, -100, 500, 800);
			AssertGlobalTotals(pL.GlobalJobCostingProfitLoss[3], 1202, 0, -101, 501, 802);
			AssertGlobalTotals(pL.GlobalJobCostingProfitLoss[4], 1202, 0, -101, 501, 802);
			AssertGlobalTotals(pL.GlobalJobCostingProfitLoss[5], 1202, 0, -101, 501, 802);
		}

		public void TestAPLineAndARLine()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S0001");
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = creator.Creditor1.PK;
			Factory.Save();

			var job = creator.CreateJob(shipment);
			var charge = creator.CreateCharge(job, creator.ManualJobAccrualChargeCode, "Desc", creator.AUD, 200M, creator.Creditor1, creator.AUD, 500M, null);
			charge.JR_GE_InternalDept = creator.GEADepartment.PK;

			var jobRevenueJournal = creator.CreateJobRevenueJournal(creator.CC1, job, 100M);
			jobRevenueJournal.JournalLines[0].CostRevenueType = TransactionLineTypes.Revenue;
			jobRevenueJournal.JournalLines[1].CostRevenueType = TransactionLineTypes.Cost;
			Factory.Save();

			var pL = new JobProfitLoss(Factory);
			pL.SetJobPKs(new ZGuid[] { job.PK });
			pL.SetParent(shipment);
			AssertEquals("5 items exist in details", 5, pL.ProfitLossDetails.Count);
			AssertEquals(2, pL.ProfitLossDetails.Where(x => x.ZY_Calc_APLine.IsValid && !x.ZY_Calc_ARLine.IsValid).Count());
			AssertEquals(3, pL.ProfitLossDetails.Where(x => !x.ZY_Calc_APLine.IsValid && x.ZY_Calc_ARLine.IsValid).Count());
		}

		public void TestQueryPlan_NoIndexScanOnJobCharge()
		{
			var creator = new TestObjectCreator(Factory);
			var firstTwoJobPKList = SetDataForIndexTest(creator);

			var profitLoss = new JobProfitLoss(Factory);
			Db.Connection.ExecuteNonQuery($"UPDATE STATISTICS {JobChargeSchema.Constants.TableName} WITH FULLSCAN");

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var collection = new ProfitLossCollection(profitLoss, firstTwoJobPKList.ToArray());
				collection.Load();
				var count = collection.Count;
				var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains("JobCharge"));
				var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());
				CombineAssertions(() =>
				{
					Assert("There must have no Index Scan on JobCharge.", !queryPlanAnalyzer.IndexScans.Any(x => x.TableName == JobChargeSchema.Constants.TableName));
					Assert("There must have no Table Scan on JobCharge.", !queryPlanAnalyzer.TableScans.Any(x => x.TableName == JobChargeSchema.Constants.TableName));
				});
			}
		}

		IEnumerable<ZGuid> SetDataForIndexTest(TestObjectCreator creator)
		{
			var firstTwoJobPKs = new List<ZGuid>();
			var sql = string.Empty;
			for (var i = 0; i < 1000; i++)
			{
				var jobPK = Guid.NewGuid();
				sql = sql + "\n" + $@"INSERT INTO dbo.JobHeader
(JH_PK, JH_JobNum, JH_GB, JH_GC, JH_GE, JH_ParentTableCode, JH_ParentID, JH_Status, JH_SystemCreateTimeUtc, JH_SystemCreateUser, JH_SystemLastEditTimeUtc, JH_SystemLastEditUser)
VALUES ('{jobPK}', 'JOB{i}', '{Env.CurrentBranchPK}', '{Env.CurrentCompanyPK}', '{Env.CurrentDepartmentPK}', 'JS', NEWID(), 'WRK', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

				var transactionPK = Guid.NewGuid();
				sql = sql + "\n" + $@"INSERT INTO dbo.AccTransactionHeader
(AH_PK, AH_GC, AH_GB, AH_GE, AH_OH, AH_Ledger, AH_TransactionNum, AH_TransactionType, AH_InvoiceDate, AH_JH, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_SystemLastEditTimeUtc, AH_SystemLastEditUser)
VALUES ('{transactionPK}', '{Env.CurrentCompanyPK}', '{Env.CurrentBranchPK}', '{Env.CurrentDepartmentPK}', '{creator.ABIGAS.PK}', 'AR', 'INV{i}', 'INV', '{ZDateTime.Today.ToISO8601String()}', '{jobPK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

				var linePK = Guid.NewGuid();
				sql = sql + "\n" + $@"INSERT INTO dbo.Acctransactionlines
(AL_PK, AL_AH, AL_JH, AL_GB, AL_GE, AL_AC, AL_LineAmount, AL_LineType, AL_OH, AL_PostDate, AL_GC, AL_SystemCreateTimeUtc, AL_SystemCreateUser, AL_SystemLastEditTimeUtc, AL_SystemLastEditUser)
VALUES('{linePK}', '{transactionPK}', '{jobPK}', '{Env.CurrentBranchPK}', '{Env.CurrentDepartmentPK}', '{creator.CC1.PK.ToGuid()}', 100, 'REV', '{creator.ABIGAS.PK}', '2020-12-21', '{Env.CurrentCompanyPK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

				sql = sql + "\n" + $@"INSERT INTO dbo.JobCharge
(JR_PK, JR_JH, JR_GB, JR_GE, JR_AC, JR_AL_ARLine, JR_AL_APLine, JR_GC, JR_SystemCreateTimeUtc, JR_SystemCreateUser, JR_SystemLastEditTimeUtc, JR_SystemLastEditUser)
VALUES(NEWID(), '{jobPK}', '{Env.CurrentBranchPK}', '{Env.CurrentDepartmentPK}', '{creator.CC1.PK.ToGuid()}', '{linePK}', null, '{Env.CurrentCompanyPK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

				if (i < 2)
				{
					firstTwoJobPKs.Add(jobPK);
				}
			}

			Db.Connection.ExecuteNonQuery(sql);
			return firstTwoJobPKs;
		}

		[SuspendCriticalValidation]
		public void TestTaxExpense_NotTaxExpense()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S0001");
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = creator.Creditor1.PK;
			Factory.Save();

			var job = creator.CreateJob(shipment);
			var charge = creator.CreateCharge(job, creator.ManualJobAccrualChargeCode, "Desc", creator.AUD, 200M, creator.Creditor1, creator.AUD, 500M, null);
			charge.JR_GE_InternalDept = creator.GEADepartment.PK;

			var invoice = creator.CreateInvoice(typeof(APInvoice));
			var line = creator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, creator.CC1, creator.AUD, 1M, "Cost line", 100M);
			creator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, creator.CC1, creator.AUD, 1M, "Cost line", 200M);

			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			taxSystem.Name = "PBW - COMPANY LVL - NAT AUTH - OFT NEG";

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N01AU");
			taxAuthority.Name = "AU NATIONAL - ATO";

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var lineMock = new Mock<ITaxableTransactionLine>();
			lineMock.Setup(l => l.PK).Returns(line.PK);
			lineMock.Setup(l => l.BaseOSAmount).Returns(300m);
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = line.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentBranch.PK, DepartmentPK = creator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -45M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(lineMock.Object);
			pivot1.ATP_IsTaxExpense = false;
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = line.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentBranch.PK, DepartmentPK = creator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -45M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(lineMock.Object);
			pivot2.ATP_IsTaxExpense = false;

			Factory.Save();

			var pL = new JobProfitLoss(Factory);
			pL.SetJobPKs(new ZGuid[] { job.PK });
			pL.SetParent(shipment);
			AssertEquals("5 items exist in details", 5, pL.ProfitLossDetails.Count);

			var profitLossDetails = pL.ProfitLossDetails.Where(x => x.ZY_Calc_AL == line.PK).ToArray();
			AssertEquals(1, profitLossDetails.Length);
			var profitLossDetail = profitLossDetails.ToArray()[0];
			AssertEquals(-100.0000m, profitLossDetail.ZY_Calc_LineAmount);
			AssertEquals("Charge Code 1", profitLossDetail.ZY_Calc_ChargeCodeDescription);
			var transLine = NewFactory().Load<AccTransactionLines>(profitLossDetail.ZY_Calc_AL);
			AssertEquals(transLine.AL_PostDate, profitLossDetail.ZY_Calc_PostDate);
			AssertEquals(transLine.AL_PostDate, profitLossDetail.ZY_Calc_RecognizedDate);
			AssertEquals(line.AL_GB, profitLossDetail.ZY_Calc_GB);
			AssertEquals(line.AL_GE, profitLossDetail.ZY_Calc_GE);
		}

		[SuspendCriticalValidation]
		public void TestTaxExpense_IsTaxExpense()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S0001");
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = creator.Creditor1.PK;
			Factory.Save();

			var job = creator.CreateJob(shipment);
			var charge = creator.CreateCharge(job, creator.ManualJobAccrualChargeCode, "Desc", creator.AUD, 200M, creator.Creditor1, creator.AUD, 500M, null);
			charge.JR_GE_InternalDept = creator.GEADepartment.PK;

			var invoice = creator.CreateInvoice(typeof(APInvoice));
			var line = creator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, creator.CC1, creator.AUD, 1M, "Cost line", 100M);
			creator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, creator.CC1, creator.AUD, 1M, "Cost line", 200M);

			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			taxSystem.Name = "PBW - COMPANY LVL - NAT AUTH - OFT NEG";

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N01AU");
			taxAuthority.Name = "AU NATIONAL - ATO";

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var lineMock = new Mock<ITaxableTransactionLine>();
			lineMock.Setup(l => l.PK).Returns(line.PK);
			lineMock.Setup(l => l.BaseOSAmount).Returns(300m);
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = line.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentBranch.PK, DepartmentPK = creator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -10M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(lineMock.Object);
			pivot1.ATP_IsTaxExpense = true;
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = line.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentBranch.PK, DepartmentPK = creator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -3M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(lineMock.Object);
			pivot2.ATP_IsTaxExpense = true;

			Factory.Save();

			var pL = new JobProfitLoss(Factory);
			pL.SetJobPKs(new ZGuid[] { job.PK });
			pL.SetParent(shipment);
			AssertEquals("6 items exist in details", 6, pL.ProfitLossDetails.Count);

			var profitLossDetails = pL.ProfitLossDetails.Where(x => x.ZY_Calc_AL == line.PK).ToArray();
			AssertEquals(2, profitLossDetails.Length);
			var profitLossDetail1 = profitLossDetails[0];
			AssertEquals(-13m, profitLossDetail1.ZY_Calc_LineAmount);
			AssertEquals("Tax Expense", profitLossDetail1.ZY_Calc_ChargeCodeDescription);
			AssertEquals(ZDate.Today.AddDays(1), profitLossDetail1.ZY_Calc_PostDate);
			AssertEquals(ZDate.Today.AddDays(2), profitLossDetail1.ZY_Calc_RecognizedDate);
			AssertEquals(creator.NonCurrentBranch.PK, profitLossDetail1.ZY_Calc_GB);
			AssertEquals(creator.NonCurrentDepartment.PK, profitLossDetail1.ZY_Calc_GE);

			var profitLossDetail2 = profitLossDetails[1];
			AssertEquals(-100.0000m, profitLossDetail2.ZY_Calc_LineAmount);
			AssertEquals("Charge Code 1", profitLossDetail2.ZY_Calc_ChargeCodeDescription);
			var transLine = NewFactory().Load<AccTransactionLines>(profitLossDetail2.ZY_Calc_AL);
			AssertEquals(transLine.AL_PostDate, profitLossDetail2.ZY_Calc_PostDate);
			AssertEquals(transLine.AL_PostDate, profitLossDetail2.ZY_Calc_RecognizedDate);
			AssertEquals(line.AL_GB, profitLossDetail2.ZY_Calc_GB);
			AssertEquals(line.AL_GE, profitLossDetail2.ZY_Calc_GE);

			CombineAssertions(() =>
			{
				AssertEquals(profitLossDetail1.ZY_Calc_JR, profitLossDetail2.ZY_Calc_JR);
				AssertEquals(profitLossDetail1.ZY_Calc_AH, profitLossDetail2.ZY_Calc_AH);
				AssertEquals(profitLossDetail1.ZY_Calc_AL, profitLossDetail2.ZY_Calc_AL);
				AssertEquals(profitLossDetail1.ZY_Calc_Ledger, profitLossDetail2.ZY_Calc_Ledger);
				AssertEquals(profitLossDetail1.ZY_Calc_TransactionType, profitLossDetail2.ZY_Calc_TransactionType);
				AssertEquals(profitLossDetail1.ZY_Calc_AC, profitLossDetail2.ZY_Calc_AC);
				AssertEquals(profitLossDetail1.ZY_Calc_JH, profitLossDetail2.ZY_Calc_JH);
				AssertEquals(profitLossDetail1.ZY_Calc_JobLocalReferenceNum, profitLossDetail2.ZY_Calc_JobLocalReferenceNum);
				AssertEquals(profitLossDetail1.ZY_Calc_GC, profitLossDetail2.ZY_Calc_GC);
				AssertEquals(profitLossDetail1.ZY_Calc_LineType, profitLossDetail2.ZY_Calc_LineType);
				AssertEquals(profitLossDetail1.ZY_Calc_InvoiceDate, profitLossDetail2.ZY_Calc_InvoiceDate);
				AssertEquals(profitLossDetail1.ZY_Calc_TransactionNum, profitLossDetail2.ZY_Calc_TransactionNum);
				AssertEquals(profitLossDetail1.ZY_Calc_FullyPaidDate, profitLossDetail2.ZY_Calc_FullyPaidDate);
				AssertEquals(profitLossDetail1.ZY_Calc_OH, profitLossDetail2.ZY_Calc_OH);
				AssertEquals(profitLossDetail1.ZY_Calc_LocalCurrency, profitLossDetail2.ZY_Calc_LocalCurrency);
				AssertEquals(profitLossDetail1.ZY_Calc_RecognitionType, profitLossDetail2.ZY_Calc_RecognitionType);
				AssertEquals(profitLossDetail1.ZY_Calc_ConsolNum, profitLossDetail2.ZY_Calc_ConsolNum);
				AssertEquals(profitLossDetail1.ZY_Calc_ReversalDate, profitLossDetail2.ZY_Calc_ReversalDate);
				AssertEquals(profitLossDetail1.ZY_Calc_SystemCreateTime, profitLossDetail2.ZY_Calc_SystemCreateTime);
				AssertEquals(profitLossDetail1.TotalRevenue, profitLossDetail2.TotalRevenue);
				AssertEquals(profitLossDetail1.TotalWIP, profitLossDetail2.TotalWIP);
				AssertEquals(profitLossDetail1.TotalCost, profitLossDetail2.TotalCost);
				AssertEquals(profitLossDetail1.TotalAccrual, profitLossDetail2.TotalAccrual);
				AssertEquals(profitLossDetail1.TotalLineAmount, profitLossDetail2.TotalLineAmount);
				AssertEquals(profitLossDetail1.TotalRevenueRecognized, profitLossDetail2.TotalRevenueRecognized);
				AssertEquals(profitLossDetail1.TotalWIPRecognized, profitLossDetail2.TotalWIPRecognized);
				AssertEquals(profitLossDetail1.TotalCostRecognized, profitLossDetail2.TotalCostRecognized);
				AssertEquals(profitLossDetail1.TotalAccrualRecognized, profitLossDetail2.TotalAccrualRecognized);
				AssertEquals(profitLossDetail1.TotalRevenueNotRecognized, profitLossDetail2.TotalRevenueNotRecognized);
				AssertEquals(profitLossDetail1.TotalWIPNotRecognized, profitLossDetail2.TotalWIPNotRecognized);
				AssertEquals(profitLossDetail1.TotalCostNotRecognized, profitLossDetail2.TotalCostNotRecognized);
				AssertEquals(profitLossDetail1.TotalAccrualNotRecognized, profitLossDetail2.TotalAccrualNotRecognized);
				AssertEquals(profitLossDetail1.ZY_Calc_AuditedBy, profitLossDetail2.ZY_Calc_AuditedBy);
				AssertEquals(profitLossDetail1.ZY_Calc_APLine, profitLossDetail2.ZY_Calc_APLine);
				AssertEquals(profitLossDetail1.ZY_Calc_ARLine, profitLossDetail2.ZY_Calc_ARLine);
			});
		}

		[SuspendCriticalValidation]
		public void TestTaxExpense_IsTaxExpense_AndNotTaxExpense()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S0001");
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = creator.Creditor1.PK;
			Factory.Save();

			var job = creator.CreateJob(shipment);
			var charge = creator.CreateCharge(job, creator.ManualJobAccrualChargeCode, "Desc", creator.AUD, 200M, creator.Creditor1, creator.AUD, 500M, null);
			charge.JR_GE_InternalDept = creator.GEADepartment.PK;

			var invoice = creator.CreateInvoice(typeof(APInvoice));
			var line = creator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, creator.CC1, creator.AUD, 1M, "Cost line", 100M);
			creator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, creator.CC1, creator.AUD, 1M, "Cost line", 200M);

			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			taxSystem.Name = "PBW - COMPANY LVL - NAT AUTH - OFT NEG";

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N01AU");
			taxAuthority.Name = "AU NATIONAL - ATO";

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var lineMock = new Mock<ITaxableTransactionLine>();
			lineMock.Setup(l => l.PK).Returns(line.PK);
			lineMock.Setup(l => l.BaseOSAmount).Returns(300m);
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = line.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentBranch.PK, DepartmentPK = creator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -10M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(lineMock.Object);
			pivot1.ATP_IsTaxExpense = true;
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = line.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentBranch.PK, DepartmentPK = creator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -3M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(lineMock.Object);
			pivot2.ATP_IsTaxExpense = false;

			Factory.Save();

			var pL = new JobProfitLoss(Factory);
			pL.SetJobPKs(new ZGuid[] { job.PK });
			pL.SetParent(shipment);
			AssertEquals("6 items exist in details", 6, pL.ProfitLossDetails.Count);

			var profitLossDetails = pL.ProfitLossDetails.Where(x => x.ZY_Calc_AL == line.PK).ToArray();
			AssertEquals(2, profitLossDetails.Length);
			var profitLossDetail = profitLossDetails.ToArray()[0];
			AssertEquals(-10m, profitLossDetail.ZY_Calc_LineAmount);
			AssertEquals("Tax Expense", profitLossDetail.ZY_Calc_ChargeCodeDescription);
			AssertEquals(ZDate.Today.AddDays(1), profitLossDetail.ZY_Calc_PostDate);
			AssertEquals(ZDate.Today.AddDays(2), profitLossDetail.ZY_Calc_RecognizedDate);
			AssertEquals(creator.NonCurrentBranch.PK, profitLossDetail.ZY_Calc_GB);
			AssertEquals(creator.NonCurrentDepartment.PK, profitLossDetail.ZY_Calc_GE);

			profitLossDetail = profitLossDetails.ToArray()[1];
			AssertEquals(-100.0000m, profitLossDetail.ZY_Calc_LineAmount);
			AssertEquals("Charge Code 1", profitLossDetail.ZY_Calc_ChargeCodeDescription);
			var transLine = NewFactory().Load<AccTransactionLines>(profitLossDetail.ZY_Calc_AL);
			AssertEquals(transLine.AL_PostDate, profitLossDetail.ZY_Calc_PostDate);
			AssertEquals(transLine.AL_PostDate, profitLossDetail.ZY_Calc_RecognizedDate);
			AssertEquals(line.AL_GB, profitLossDetail.ZY_Calc_GB);
			AssertEquals(line.AL_GE, profitLossDetail.ZY_Calc_GE);
		}

		[SuspendCriticalValidation]
		public void TestTaxExpense_IsTaxExpense_MultipleLines()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S0001");
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = creator.Creditor1.PK;
			Factory.Save();

			var job = creator.CreateJob(shipment);
			var charge = creator.CreateCharge(job, creator.ManualJobAccrualChargeCode, "Desc", creator.AUD, 200M, creator.Creditor1, creator.AUD, 500M, null);
			charge.JR_GE_InternalDept = creator.GEADepartment.PK;

			var invoice = creator.CreateInvoice(typeof(APInvoice));
			var line1 = creator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, creator.CC1, creator.AUD, 1M, "Cost line", 100M);
			var line2 = creator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, creator.CC1, creator.AUD, 1M, "Cost line", 200M);

			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			taxSystem.Name = "PBW - COMPANY LVL - NAT AUTH - OFT NEG";

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N01AU");
			taxAuthority.Name = "AU NATIONAL - ATO";

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var lineMock = new Mock<ITaxableTransactionLine>();
			lineMock.Setup(l => l.PK).Returns(line1.PK);
			lineMock.Setup(l => l.BaseOSAmount).Returns(300m);
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = line1.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentBranch.PK, DepartmentPK = creator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -10M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(lineMock.Object);
			pivot1.ATP_IsTaxExpense = true;

			lineMock.Setup(l => l.PK).Returns(line2.PK);
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = line2.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentCompanyBranch.PK, DepartmentPK = creator.MiscDepartment.PK, PostDate = ZDate.Today.AddDays(3), RealisationDate = ZDate.Today.AddDays(4), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -3M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(lineMock.Object);
			pivot2.ATP_IsTaxExpense = true;

			Factory.Save();

			var pL = new JobProfitLoss(Factory);
			pL.SetJobPKs(new ZGuid[] { job.PK });
			pL.SetParent(shipment);
			AssertEquals("7 items exist in details", 7, pL.ProfitLossDetails.Count);

			var profitLossDetails = pL.ProfitLossDetails.Where(x => x.ZY_Calc_AL == line1.PK).ToArray();
			AssertEquals(2, profitLossDetails.Length);
			var profitLossDetail = profitLossDetails.ToArray()[0];
			AssertEquals(-10m, profitLossDetail.ZY_Calc_LineAmount);
			AssertEquals("Tax Expense", profitLossDetail.ZY_Calc_ChargeCodeDescription);
			AssertEquals(ZDate.Today.AddDays(1), profitLossDetail.ZY_Calc_PostDate);
			AssertEquals(ZDate.Today.AddDays(2), profitLossDetail.ZY_Calc_RecognizedDate);
			AssertEquals(creator.NonCurrentBranch.PK, profitLossDetail.ZY_Calc_GB);
			AssertEquals(creator.NonCurrentDepartment.PK, profitLossDetail.ZY_Calc_GE);

			profitLossDetail = profitLossDetails.ToArray()[1];
			AssertEquals(-100.0000m, profitLossDetail.ZY_Calc_LineAmount);
			AssertEquals("Charge Code 1", profitLossDetail.ZY_Calc_ChargeCodeDescription);
			var transLine = NewFactory().Load<AccTransactionLines>(profitLossDetail.ZY_Calc_AL);
			AssertEquals(transLine.AL_PostDate, profitLossDetail.ZY_Calc_PostDate);
			AssertEquals(transLine.AL_PostDate, profitLossDetail.ZY_Calc_RecognizedDate);
			AssertEquals(line1.AL_GB, profitLossDetail.ZY_Calc_GB);
			AssertEquals(line1.AL_GE, profitLossDetail.ZY_Calc_GE);

			profitLossDetails = pL.ProfitLossDetails.Where(x => x.ZY_Calc_AL == line2.PK).ToArray();
			AssertEquals(2, profitLossDetails.Length);
			profitLossDetail = profitLossDetails.ToArray()[0];
			AssertEquals(-3m, profitLossDetail.ZY_Calc_LineAmount);
			AssertEquals("Tax Expense", profitLossDetail.ZY_Calc_ChargeCodeDescription);
			AssertEquals(ZDate.Today.AddDays(3), profitLossDetail.ZY_Calc_PostDate);
			AssertEquals(ZDate.Today.AddDays(4), profitLossDetail.ZY_Calc_RecognizedDate);
			AssertEquals(creator.NonCurrentCompanyBranch.PK, profitLossDetail.ZY_Calc_GB);
			AssertEquals(creator.MiscDepartment.PK, profitLossDetail.ZY_Calc_GE);

			profitLossDetail = profitLossDetails.ToArray()[1];
			AssertEquals(-200.0000m, profitLossDetail.ZY_Calc_LineAmount);
			AssertEquals("Charge Code 1", profitLossDetail.ZY_Calc_ChargeCodeDescription);
			transLine = NewFactory().Load<AccTransactionLines>(profitLossDetail.ZY_Calc_AL);
			AssertEquals(transLine.AL_PostDate, profitLossDetail.ZY_Calc_PostDate);
			AssertEquals(transLine.AL_PostDate, profitLossDetail.ZY_Calc_RecognizedDate);
			AssertEquals(line2.AL_GB, profitLossDetail.ZY_Calc_GB);
			AssertEquals(line2.AL_GE, profitLossDetail.ZY_Calc_GE);
		}

		[SuspendCriticalValidation]
		public void TestTaxExpense_DifferentPostDates()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S0001");
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = creator.Creditor1.PK;
			Factory.Save();

			var job = creator.CreateJob(shipment);
			var charge = creator.CreateCharge(job, creator.ManualJobAccrualChargeCode, "Desc", creator.AUD, 200M, creator.Creditor1, creator.AUD, 500M, null);
			charge.JR_GE_InternalDept = creator.GEADepartment.PK;

			var invoice = creator.CreateInvoice(typeof(APInvoice));
			var line = creator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, creator.CC1, creator.AUD, 1M, "Cost line", 100M);
			creator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, creator.CC1, creator.AUD, 1M, "Cost line", 200M);

			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			taxSystem.Name = "PBW - COMPANY LVL - NAT AUTH - OFT NEG";

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N01AU");
			taxAuthority.Name = "AU NATIONAL - ATO";

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var lineMock = new Mock<ITaxableTransactionLine>();
			lineMock.Setup(l => l.PK).Returns(line.PK);
			lineMock.Setup(l => l.BaseOSAmount).Returns(300m);
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = line.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentBranch.PK, DepartmentPK = creator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(3), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -10M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(lineMock.Object);
			pivot1.ATP_IsTaxExpense = true;
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = line.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentBranch.PK, DepartmentPK = creator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(2), RealisationDate = ZDate.Today.AddDays(3), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -3M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(lineMock.Object);
			pivot2.ATP_IsTaxExpense = true;

			Factory.Save();

			var pL = new JobProfitLoss(Factory);
			pL.SetJobPKs(new ZGuid[] { job.PK });
			pL.SetParent(shipment);
			AssertEquals("7 items exist in details", 7, pL.ProfitLossDetails.Count);

			var profitLossDetails = pL.ProfitLossDetails.Where(x => x.ZY_Calc_AL == line.PK).ToArray();
			AssertEquals(3, profitLossDetails.Length);
			var profitLossDetail = profitLossDetails[0];
			AssertEquals(-10m, profitLossDetail.ZY_Calc_LineAmount);
			AssertEquals("Tax Expense", profitLossDetail.ZY_Calc_ChargeCodeDescription);
			AssertEquals(ZDate.Today.AddDays(1), profitLossDetail.ZY_Calc_PostDate);
			AssertEquals(ZDate.Today.AddDays(3), profitLossDetail.ZY_Calc_RecognizedDate);
			AssertEquals(creator.NonCurrentBranch.PK, profitLossDetail.ZY_Calc_GB);
			AssertEquals(creator.NonCurrentDepartment.PK, profitLossDetail.ZY_Calc_GE);

			profitLossDetail = profitLossDetails[1];
			AssertEquals(-3m, profitLossDetail.ZY_Calc_LineAmount);
			AssertEquals("Tax Expense", profitLossDetail.ZY_Calc_ChargeCodeDescription);
			AssertEquals(ZDate.Today.AddDays(2), profitLossDetail.ZY_Calc_PostDate);
			AssertEquals(ZDate.Today.AddDays(3), profitLossDetail.ZY_Calc_RecognizedDate);
			AssertEquals(creator.NonCurrentBranch.PK, profitLossDetail.ZY_Calc_GB);
			AssertEquals(creator.NonCurrentDepartment.PK, profitLossDetail.ZY_Calc_GE);

			profitLossDetail = profitLossDetails[2];
			AssertEquals(-100.0000m, profitLossDetail.ZY_Calc_LineAmount);
			AssertEquals("Charge Code 1", profitLossDetail.ZY_Calc_ChargeCodeDescription);
			var transLine = NewFactory().Load<AccTransactionLines>(profitLossDetail.ZY_Calc_AL);
			AssertEquals(transLine.AL_PostDate, profitLossDetail.ZY_Calc_PostDate);
			AssertEquals(transLine.AL_PostDate, profitLossDetail.ZY_Calc_RecognizedDate);
			AssertEquals(line.AL_GB, profitLossDetail.ZY_Calc_GB);
			AssertEquals(line.AL_GE, profitLossDetail.ZY_Calc_GE);
		}

		[SuspendCriticalValidation]
		public void TestTaxExpense_DifferentRealisationDates()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S0001");
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = creator.Creditor1.PK;
			Factory.Save();

			var job = creator.CreateJob(shipment);
			var charge = creator.CreateCharge(job, creator.ManualJobAccrualChargeCode, "Desc", creator.AUD, 200M, creator.Creditor1, creator.AUD, 500M, null);
			charge.JR_GE_InternalDept = creator.GEADepartment.PK;

			var invoice = creator.CreateInvoice(typeof(APInvoice));
			var line = creator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, creator.CC1, creator.AUD, 1M, "Cost line", 100M);
			creator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, creator.CC1, creator.AUD, 1M, "Cost line", 200M);

			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			taxSystem.Name = "PBW - COMPANY LVL - NAT AUTH - OFT NEG";

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N01AU");
			taxAuthority.Name = "AU NATIONAL - ATO";

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var lineMock = new Mock<ITaxableTransactionLine>();
			lineMock.Setup(l => l.PK).Returns(line.PK);
			lineMock.Setup(l => l.BaseOSAmount).Returns(300m);
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = line.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentBranch.PK, DepartmentPK = creator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -10M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(lineMock.Object);
			pivot1.ATP_IsTaxExpense = true;
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = line.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentBranch.PK, DepartmentPK = creator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(3), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -3M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(lineMock.Object);
			pivot2.ATP_IsTaxExpense = true;

			Factory.Save();

			var pL = new JobProfitLoss(Factory);
			pL.SetJobPKs(new ZGuid[] { job.PK });
			pL.SetParent(shipment);
			AssertEquals("7 items exist in details", 7, pL.ProfitLossDetails.Count);

			var profitLossDetails = pL.ProfitLossDetails.Where(x => x.ZY_Calc_AL == line.PK).ToArray();
			AssertEquals(3, profitLossDetails.Length);
			var profitLossDetail = profitLossDetails[0];
			AssertEquals(-10m, profitLossDetail.ZY_Calc_LineAmount);
			AssertEquals("Tax Expense", profitLossDetail.ZY_Calc_ChargeCodeDescription);
			AssertEquals(ZDate.Today.AddDays(1), profitLossDetail.ZY_Calc_PostDate);
			AssertEquals(ZDate.Today.AddDays(2), profitLossDetail.ZY_Calc_RecognizedDate);
			AssertEquals(creator.NonCurrentBranch.PK, profitLossDetail.ZY_Calc_GB);
			AssertEquals(creator.NonCurrentDepartment.PK, profitLossDetail.ZY_Calc_GE);

			profitLossDetail = profitLossDetails[1];
			AssertEquals(-3m, profitLossDetail.ZY_Calc_LineAmount);
			AssertEquals("Tax Expense", profitLossDetail.ZY_Calc_ChargeCodeDescription);
			AssertEquals(ZDate.Today.AddDays(1), profitLossDetail.ZY_Calc_PostDate);
			AssertEquals(ZDate.Today.AddDays(3), profitLossDetail.ZY_Calc_RecognizedDate);
			AssertEquals(creator.NonCurrentBranch.PK, profitLossDetail.ZY_Calc_GB);
			AssertEquals(creator.NonCurrentDepartment.PK, profitLossDetail.ZY_Calc_GE);

			profitLossDetail = profitLossDetails[2];
			AssertEquals(-100.0000m, profitLossDetail.ZY_Calc_LineAmount);
			AssertEquals("Charge Code 1", profitLossDetail.ZY_Calc_ChargeCodeDescription);
			var transLine = NewFactory().Load<AccTransactionLines>(profitLossDetail.ZY_Calc_AL);
			AssertEquals(transLine.AL_PostDate, profitLossDetail.ZY_Calc_PostDate);
			AssertEquals(transLine.AL_PostDate, profitLossDetail.ZY_Calc_RecognizedDate);
			AssertEquals(line.AL_GB, profitLossDetail.ZY_Calc_GB);
			AssertEquals(line.AL_GE, profitLossDetail.ZY_Calc_GE);
		}

		[SuspendCriticalValidation]
		public void TestTaxExpense_DifferentBranches()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S0001");
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = creator.Creditor1.PK;
			Factory.Save();

			var job = creator.CreateJob(shipment);
			var charge = creator.CreateCharge(job, creator.ManualJobAccrualChargeCode, "Desc", creator.AUD, 200M, creator.Creditor1, creator.AUD, 500M, null);
			charge.JR_GE_InternalDept = creator.GEADepartment.PK;

			var invoice = creator.CreateInvoice(typeof(APInvoice));
			var line = creator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, creator.CC1, creator.AUD, 1M, "Cost line", 100M);
			creator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, creator.CC1, creator.AUD, 1M, "Cost line", 200M);

			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			taxSystem.Name = "PBW - COMPANY LVL - NAT AUTH - OFT NEG";

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N01AU");
			taxAuthority.Name = "AU NATIONAL - ATO";

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var lineMock = new Mock<ITaxableTransactionLine>();
			lineMock.Setup(l => l.PK).Returns(line.PK);
			lineMock.Setup(l => l.BaseOSAmount).Returns(300m);
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = line.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentBranch.PK, DepartmentPK = creator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -10M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(lineMock.Object);
			pivot1.ATP_IsTaxExpense = true;
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = line.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentCompanyBranch.PK, DepartmentPK = creator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -3M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(lineMock.Object);
			pivot2.ATP_IsTaxExpense = true;

			Factory.Save();

			var pL = new JobProfitLoss(Factory);
			pL.SetJobPKs(new ZGuid[] { job.PK });
			pL.SetParent(shipment);
			AssertEquals("7 items exist in details", 7, pL.ProfitLossDetails.Count);

			var profitLossDetails = pL.ProfitLossDetails.Where(x => x.ZY_Calc_AL == line.PK).ToArray();
			AssertEquals(3, profitLossDetails.Length);
			var profitLossDetail = profitLossDetails[0];
			AssertEquals(-10m, profitLossDetail.ZY_Calc_LineAmount);
			AssertEquals("Tax Expense", profitLossDetail.ZY_Calc_ChargeCodeDescription);
			AssertEquals(ZDate.Today.AddDays(1), profitLossDetail.ZY_Calc_PostDate);
			AssertEquals(ZDate.Today.AddDays(2), profitLossDetail.ZY_Calc_RecognizedDate);
			AssertEquals(creator.NonCurrentBranch.PK, profitLossDetail.ZY_Calc_GB);
			AssertEquals(creator.NonCurrentDepartment.PK, profitLossDetail.ZY_Calc_GE);

			profitLossDetail = profitLossDetails[1];
			AssertEquals(-3m, profitLossDetail.ZY_Calc_LineAmount);
			AssertEquals("Tax Expense", profitLossDetail.ZY_Calc_ChargeCodeDescription);
			AssertEquals(ZDate.Today.AddDays(1), profitLossDetail.ZY_Calc_PostDate);
			AssertEquals(ZDate.Today.AddDays(2), profitLossDetail.ZY_Calc_RecognizedDate);
			AssertEquals(creator.NonCurrentCompanyBranch.PK, profitLossDetail.ZY_Calc_GB);
			AssertEquals(creator.NonCurrentDepartment.PK, profitLossDetail.ZY_Calc_GE);

			profitLossDetail = profitLossDetails[2];
			AssertEquals(-100.0000m, profitLossDetail.ZY_Calc_LineAmount);
			AssertEquals("Charge Code 1", profitLossDetail.ZY_Calc_ChargeCodeDescription);
			var transLine = NewFactory().Load<AccTransactionLines>(profitLossDetail.ZY_Calc_AL);
			AssertEquals(transLine.AL_PostDate, profitLossDetail.ZY_Calc_PostDate);
			AssertEquals(transLine.AL_PostDate, profitLossDetail.ZY_Calc_RecognizedDate);
			AssertEquals(line.AL_GB, profitLossDetail.ZY_Calc_GB);
			AssertEquals(line.AL_GE, profitLossDetail.ZY_Calc_GE);
		}

		[SuspendCriticalValidation]
		public void TestTaxExpense_DifferentDepartments()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S0001");
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = creator.Creditor1.PK;
			Factory.Save();

			var job = creator.CreateJob(shipment);
			var charge = creator.CreateCharge(job, creator.ManualJobAccrualChargeCode, "Desc", creator.AUD, 200M, creator.Creditor1, creator.AUD, 500M, null);
			charge.JR_GE_InternalDept = creator.GEADepartment.PK;

			var invoice = creator.CreateInvoice(typeof(APInvoice));
			var line = creator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, creator.CC1, creator.AUD, 1M, "Cost line", 100M);
			creator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job, creator.CC1, creator.AUD, 1M, "Cost line", 200M);

			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			taxSystem.Name = "PBW - COMPANY LVL - NAT AUTH - OFT NEG";

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N01AU");
			taxAuthority.Name = "AU NATIONAL - ATO";

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var lineMock = new Mock<ITaxableTransactionLine>();
			lineMock.Setup(l => l.PK).Returns(line.PK);
			lineMock.Setup(l => l.BaseOSAmount).Returns(300m);
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = line.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentBranch.PK, DepartmentPK = creator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -10M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(lineMock.Object);
			pivot1.ATP_IsTaxExpense = true;
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = line.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentBranch.PK, DepartmentPK = creator.MiscDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -3M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(lineMock.Object);
			pivot2.ATP_IsTaxExpense = true;

			Factory.Save();

			var pL = new JobProfitLoss(Factory);
			pL.SetJobPKs(new ZGuid[] { job.PK });
			pL.SetParent(shipment);
			AssertEquals("7 items exist in details", 7, pL.ProfitLossDetails.Count);

			var profitLossDetails = pL.ProfitLossDetails.Where(x => x.ZY_Calc_AL == line.PK).OrderByDescending(x => x.ZY_Calc_LineAmount).ToArray();
			AssertEquals(3, profitLossDetails.Length);

			var profitLossDetail = profitLossDetails[0];
			AssertEquals(-3m, profitLossDetail.ZY_Calc_LineAmount);
			AssertEquals("Tax Expense", profitLossDetail.ZY_Calc_ChargeCodeDescription);
			AssertEquals(ZDate.Today.AddDays(1), profitLossDetail.ZY_Calc_PostDate);
			AssertEquals(ZDate.Today.AddDays(2), profitLossDetail.ZY_Calc_RecognizedDate);
			AssertEquals(creator.NonCurrentBranch.PK, profitLossDetail.ZY_Calc_GB);
			AssertEquals(creator.MiscDepartment.PK, profitLossDetail.ZY_Calc_GE);

			profitLossDetail = profitLossDetails[1];
			AssertEquals(-10m, profitLossDetail.ZY_Calc_LineAmount);
			AssertEquals("Tax Expense", profitLossDetail.ZY_Calc_ChargeCodeDescription);
			AssertEquals(ZDate.Today.AddDays(1), profitLossDetail.ZY_Calc_PostDate);
			AssertEquals(ZDate.Today.AddDays(2), profitLossDetail.ZY_Calc_RecognizedDate);
			AssertEquals(creator.NonCurrentBranch.PK, profitLossDetail.ZY_Calc_GB);
			AssertEquals(creator.NonCurrentDepartment.PK, profitLossDetail.ZY_Calc_GE);

			profitLossDetail = profitLossDetails[2];
			AssertEquals(-100.0000m, profitLossDetail.ZY_Calc_LineAmount);
			AssertEquals("Charge Code 1", profitLossDetail.ZY_Calc_ChargeCodeDescription);
			var transLine = NewFactory().Load<AccTransactionLines>(profitLossDetail.ZY_Calc_AL);
			AssertEquals(transLine.AL_PostDate, profitLossDetail.ZY_Calc_PostDate);
			AssertEquals(transLine.AL_PostDate, profitLossDetail.ZY_Calc_RecognizedDate);
			AssertEquals(line.AL_GB, profitLossDetail.ZY_Calc_GB);
			AssertEquals(line.AL_GE, profitLossDetail.ZY_Calc_GE);
		}

		[SuspendCriticalValidation]
		public void TestTotalTaxExpenseRevenueAndCost()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment = creator.CreateShipment("S001001", consol);
			var job = creator.CreateJob(shipment, createWithMutex: false);

			var arInvoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV10010", creator.AUD, 1M, 150M, 0M, 150M, 0M, creator.ABIGAS, creator.CC1.PK);
			var revenueLine = arInvoice.Lines[0];
			revenueLine.AL_JH = job.PK;
			creator.CreateJobCharge(revenueLine, job, creator.CC1, creator.AUD);

			var apInvoice = creator.CreateInvoiceWithLine(typeof(APInvoice), "AP101", creator.AUD, 1M, 50M, 0M, 50M, 0M, creator.AALSHI, creator.CC1.PK);
			var costLine = apInvoice.Lines[0];
			costLine.AL_JH = job.PK;
			creator.CreateJobCharge(costLine, job, creator.CC1, creator.AUD);

			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			taxSystem.Name = "PBW - COMPANY LVL - NAT AUTH - OFT NEG";

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N01AU");
			taxAuthority.Name = "AU NATIONAL - ATO";

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentBranch.PK, DepartmentPK = creator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -10M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(revenueLine));
			pivot1.ATP_IsTaxExpense = true;
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentBranch.PK, DepartmentPK = creator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -3M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(revenueLine));
			pivot2.ATP_IsTaxExpense = true;

			var taxRecord3 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = costLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentBranch.PK, DepartmentPK = creator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -15M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot3 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot3.ATP_ATT = taxRecord3.PK;
			pivot3.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(costLine));
			pivot3.ATP_IsTaxExpense = true;
			var taxRecord4 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = costLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = creator.NonCurrentBranch.PK, DepartmentPK = creator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -7M, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot4 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot4.ATP_ATT = taxRecord4.PK;
			pivot4.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(costLine));
			pivot4.ATP_IsTaxExpense = true;

			Factory.Save();

			var profitLoss = new JobProfitLoss(Factory);
			profitLoss.SetParent(shipment);
			profitLoss.SetJobPKs(new[] { job.PK });

			AssertEquals(6, profitLoss.GlobalJobCostingProfitLoss.Count);
			profitLoss.GlobalJobCostingProfitLoss.ForEach(x => AssertEquals(-13m, x.TotalTaxExpenseRevenue));
			profitLoss.GlobalJobCostingProfitLoss.ForEach(x => AssertEquals(-22m, x.TotalTaxExpenseCost));
			profitLoss.GlobalJobCostingProfitLoss.ForEach(x => AssertEquals(137m, x.TotalRevenueRecognized));
			profitLoss.GlobalJobCostingProfitLoss.ForEach(x => AssertEquals(-72m, x.TotalCostRecognized));
			profitLoss.GlobalJobCostingProfitLoss.ForEach(x => AssertEquals(137m, x.TotalRevenue));
			profitLoss.GlobalJobCostingProfitLoss.ForEach(x => AssertEquals(-72m, x.TotalCost));
			profitLoss.GlobalJobCostingProfitLoss.ForEach(x => AssertEquals(-35m, x.TotalLineAmountRecognized));
			profitLoss.GlobalJobCostingProfitLoss.ForEach(x => AssertEquals(-35m, x.TotalLineAmount));
		}

		public void TestReversalDate()
		{
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S0001");
			var job = creator.CreateJob(shipment);
			var charge = creator.CreateCharge(job, creator.CC1, 100m, 100m);
			Factory.Save();

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			charge.CreateCostTransactionLine(invoice, ZDateTime.Now);
			Factory.Save();

			var pL = new JobProfitLoss(Factory);
			pL.SetParent(shipment);
			pL.SetJobPKs(new ZGuid[] { job.PK });
			AssertEquals("4 items exist in details - 1 WIP, 2 ACR, 1 CST", 4, pL.ProfitLossDetails.Count);

			var acrDetail = pL.ProfitLossDetails.Where(x => x.ZY_Calc_LineType == "ACR").First();
			var query = new ZQuery(StmALogSchema.SL_Parent, acrDetail.ZY_Calc_AL);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "REV");
			var log = Factory.Load<StmALog>(query);
			AssertEquals(1, log.Length);
			AssertEquals(log[0].SL_EventTime, acrDetail.ZY_Calc_ReversalDate);
		}

		[TestDate(2016, 5, 25, 0, 0, 0)]
		public void TestAuditDetails()
		{
			ZDateTime time1 = new ZDateTime(TestDateAttribute.Date);
			ZDateTime time2 = time1.AddMinutes(2);

			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S0001");
			var job = creator.CreateJob(shipment);
			var charge = creator.CreateCharge(job, creator.CC1, 100m, 100m);
			Factory.Save();

			JobProfitLoss pL = new JobProfitLoss(Factory);
			pL.SetParent(shipment);
			pL.SetJobPKs(new ZGuid[] { job.PK });
			AssertEquals("2 items exist in details - 1 WIP, 1 ACR", 2, pL.ProfitLossDetails.Count);

			var line1 = Factory.Load<AccTransactionLines>(pL.ProfitLossDetails[0].ZY_Calc_AL);
			var line2 = Factory.Load<AccTransactionLines>(pL.ProfitLossDetails[1].ZY_Calc_AL);
			AssertEquals(line1.AL_SystemCreateTimeUtc.ToSmallDateTimeFloor(), pL.ProfitLossDetails[0].ZY_Calc_SystemCreateTime.ToSmallDateTimeFloor());
			AssertEquals(line2.AL_SystemCreateTimeUtc.ToSmallDateTimeFloor(), pL.ProfitLossDetails[1].ZY_Calc_SystemCreateTime.ToSmallDateTimeFloor());

			TestDateAttribute.Date = time2.ToDateTime();
			charge.JR_OSSellAmt = 200m;
			Factory.Save();

			pL.ProfitLossDetails.Load();
			AssertEquals("4 items exist in details - 3 WIP, 1 ACR", 4, pL.ProfitLossDetails.Count);

			var profitLossDetail = pL.ProfitLossDetails.FirstOrDefault(x => x.ZY_Calc_LineAmount < 0 && x.ZY_Calc_LineType == "WIP");
			var line3 = Factory.Load<AccTransactionLines>(profitLossDetail.ZY_Calc_AL);
			AssertEquals(line3.AL_SystemLastEditTimeUtc.ToSmallDateTimeFloor(), profitLossDetail.ZY_Calc_SystemCreateTime.ToSmallDateTimeFloor());
			AssertNotEquals(line3.AL_SystemCreateTimeUtc.ToSmallDateTimeFloor(), profitLossDetail.ZY_Calc_SystemCreateTime.ToSmallDateTimeFloor());
		}

		public void TestJobPKs()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			CommonShipment ship1 = consol.Shipments.AddNew();
			CommonShipment ship2 = consol.Shipments.AddNew();

			JobProfitLoss pL = new JobProfitLoss(Factory);
			GlobalProfitLossCollection coll = new GlobalProfitLossCollection(pL, ship1);

			AssertEquals("No jobs", 0, coll.JobPKs.Length);

			Job ship1Job = Job.CreateWithMutex(Factory, ship1);
			ship1Job.JH_ParentTableCode = "JS";
			ship1Job.JH_ParentID = ship1.PK;
			ship1Job.JH_GB = GlbBranch.CurrentBranch.PK;
			ship1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			ship1Job.JH_JobNum = "111";
			Factory.Save();
			coll = new GlobalProfitLossCollection(pL, ship1);
			AssertEquals("1 job", 1, coll.JobPKs.Length);
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

			var collection = new GlobalProfitLossCollection(profitLoss, jobPKList.ToArray());
			AssertNoExceptionThrown(() => { collection.Load(); });
		}

		public void TestRecognisedChargeOnlyFilter()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			creator.CreateTestPeriods(ZDateTime.Now.AddMonths(-1));
			var shipment = creator.GetTestShipmentPlugIn();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			var chargeCode = creator.CC1;
			var revRecOverride = chargeCode.RevenueRecOverrides.AddNew();
			revRecOverride.AE_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			revRecOverride.AE_Mode = "ALL";
			revRecOverride.AE_Direction = "ALL";
			revRecOverride.AE_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate;
			var charge = job.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OSSellAmt = 100m;
			charge.JR_OH_SellAccount = creator.ABIGAS.PK;
			charge.JR_OSCostAmt = 100m;
			Factory.Save();
			InvoicingPostManager postManager = new InvoicingPostManager(job);
			postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);

			AssertEquals("Should have created one invoice", 1, postManager.Poster.PostedInvoices.Count);
			AssertEquals("Invoice should have one line", 1, postManager.Poster.PostedInvoices[0].Lines.Count);
			AssertEquals("Recognition Date should be empty", ZDateTime.Empty, postManager.Poster.PostedInvoices[0].Lines[0].AL_ReverseDate);

			Factory.Save();

			JobProfitLoss pL = new JobProfitLoss(Factory);
			pL.SetParent(shipment);
			pL.SetConsol(Factory.New<ForwardingConsol>());
			pL.SetJobPKs(new ZGuid[] { job.PK });
			pL.Filter.RecognizedChargesFilter = "ALL";
			pL.ProfitLossDetails.Load();
			pL.GlobalJobCostingProfitLoss.Load();
			AssertEquals("Should have 2 records present", 2, pL.ProfitLossDetails.Count);
			AssertEquals("Should have 2 records present", 2, pL.GlobalJobCostingProfitLoss.Count);

			pL.Filter.RecognizedChargesFilter = "REC";
			pL.ProfitLossDetails.Load();
			pL.GlobalJobCostingProfitLoss.Load();
			AssertEquals("Should have no records present (i.e. nothing is recognised)", 0, pL.ProfitLossDetails.Count);
			AssertEquals("Should not do the filtering as global profit loss does not have recognized charges filter", 2, pL.GlobalJobCostingProfitLoss.Count);

			postManager.Poster.PostedInvoices[0].Lines[0].AL_ReverseDate = ZDateTime.Now;
			Factory.Save();

			pL.ProfitLossDetails.Load();
			pL.GlobalJobCostingProfitLoss.Load();
			AssertEquals("Should have 1 record present (invoice line is recognised)", 1, pL.ProfitLossDetails.Count);
			AssertEquals("Should not do the filtering as global profit loss does not have recognized charges filter", 2, pL.GlobalJobCostingProfitLoss.Count);

			pL.Filter.RecognizedChargesFilter = "ALL";
			pL.ProfitLossDetails.Load();
			pL.GlobalJobCostingProfitLoss.Load();
			AssertEquals("Should have 2 records present (invoice line is recognised, cost accrual is not)", 2, pL.ProfitLossDetails.Count);
			AssertEquals("Should not do the filtering as global profit loss does not have recognized charges filter", 2, pL.GlobalJobCostingProfitLoss.Count);
		}

		public void TestDuplicateChargeRelateToShowConsolNumber()
		{
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S0001");
			var consol1 = creator.CreateConsol("AUSYD", "USLAX", "C001");
			consol1.Shipments.Add(shipment);

			var listing = new ApportionmentListing(Factory, consol1);
			var consolCost = listing.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			consolCost.E6_OSCostAmount = 100m;
			consolCost.CostExchangeRate.Currency = "AUD";
			consolCost.E6_ApportionmentMethod = AllocationMethod.Manual;
			consolCost.ApportionmentCharges[0].JR_OSCostAmt = 100m;

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_UniqueConsignRef = "C002";

			Factory.Save();

			var pL = new JobProfitLoss(Factory);
			pL.SetConsol(consol1);
			pL.SetParent(shipment);

			AssertEquals("Should have 2 records present", 2, pL.ProfitLossDetails.Count);

			foreach (ProfitLossDetail detail in pL.ProfitLossDetails)
			{
				if (detail.ZY_Calc_LineType == "WIP")
				{
					AssertEquals(100m, detail.ZY_Calc_LineAmount);
					AssertEquals("", detail.ZY_Calc_ConsolNum);
				}
				else if (detail.ZY_Calc_LineType == "ACR")
				{
					AssertEquals(-100m, detail.ZY_Calc_LineAmount);
					AssertEquals("C001", detail.ZY_Calc_ConsolNum);
				}
			}
		}

		public void TestNonLoginCompanyChargesWouldNotBeRecognizedDuringSaving()
		{
			var shipment = TestObjectCreator.CreateShipment("S001000", true);

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var factoryForChargeCode = new BusinessObjectFactory();
				var chargeCodeFRT = factoryForChargeCode.Load<AccChargeCode>(TestObjectCreator.FRT.PK);

				var recOverrideSHP = chargeCodeFRT.RevenueRecOverrides.AddNew();
				recOverrideSHP.AE_JobType = JobInvoicingConsumerTypes.Shipment.Code;
				recOverrideSHP.AE_Direction = Core.Constants.FreightShipmentDirection.Code.All;
				recOverrideSHP.AE_Mode = Core.Constants.TransportModes.All;
				recOverrideSHP.AE_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
				factoryForChargeCode.Save();

				var factoryInAnotherCompany = new BusinessObjectFactory();
				var shipmentInAnotherCompany = factoryInAnotherCompany.Load<ForwardingShipment>(shipment.PK);
				using (var jobInAnotherCompany = new Job.Loader(shipmentInAnotherCompany).TryCreateWithMutex())
				{
					jobInAnotherCompany.JH_GE = TestObjectCreator.FEADepartment.PK;

					var charge = jobInAnotherCompany.Charges.AddNew();
					charge.JR_AC = chargeCodeFRT.PK;
					charge.JR_OSSellAmt = 100m;
					charge.JR_OSCostAmt = 0m;

					AssertEquals("PreCondition, charge is not able to be recognized when creating.", false, charge.CanRecognizeProfitOnWIPsAndAccruals);
					factoryInAnotherCompany.Save();

					recOverrideSHP.AE_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
					factoryForChargeCode.Save();
					CombineAssertions("PreCondition, cahrge is now ready to be recognized.", () => {
						AssertEquals("CanRecognizeProfitOnWIPsAndAccruals", true, charge.CanRecognizeProfitOnWIPsAndAccruals);
						AssertEquals("JR_AL_ARLine", ZGuid.Empty, charge.JR_AL_ARLine);
						AssertEquals("HasChanges", false, charge.HasChanges);
					});
				}
			}

			var coll = new GlobalProfitLossCollection(new JobProfitLoss(Factory), shipment);
			coll.Load();
			CombineAssertions("PreCondition, we get the non-login company charge.", () => {
				AssertEquals("Count", 1, coll.Count);
				AssertEquals("Non-login Company", TestObjectCreator.NonCurrentCompanyBranch.Company.PK, coll[0].ZY_Calc_GC);
				AssertEquals("ZY_Calc_LineAmount, this calculation will get CFX amount via loading charge.", 100m, coll[0].ZY_Calc_LineAmount);
			});

			Factory.Save();

			var chargeInAnotherCompany = new BusinessObjectFactory().Load<BaseCharge>(coll[0].ZY_Calc_JR);
			CombineAssertions("Should not recognize non-login company charge.", () => {
				AssertEquals(true, chargeInAnotherCompany.CanRecognizeProfitOnWIPsAndAccruals);
				AssertEquals(ZGuid.Empty, chargeInAnotherCompany.JR_AL_ARLine);
				AssertEquals(false, chargeInAnotherCompany.HasChanges);
			});
		}

		public void TestChargesUnsavedAmountChangingWouldNotAffectResult()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var shipment = TestObjectCreator.CreateShipment("S001000", true);
			using (var job = new Job.Loader(shipment).TryCreateWithMutex())
			{
				var recOverrideSHP = TestObjectCreator.FRT.RevenueRecOverrides.AddNew();
				recOverrideSHP.AE_JobType = JobInvoicingConsumerTypes.Shipment.Code;
				recOverrideSHP.AE_Direction = Core.Constants.FreightShipmentDirection.Code.All;
				recOverrideSHP.AE_Mode = Core.Constants.TransportModes.All;
				recOverrideSHP.AE_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
				Factory.Save();

				var rateUSD = job.ExchangeRates.AddNew();
				rateUSD.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code;
				rateUSD.JF_BaseRate = 1m;
				rateUSD.OrgType = ExchangeRateOrgTypeEnum.Debtor;
				rateUSD.JF_CFXPercent = 25;
				Factory.Save();

				var charge = job.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.FRT.PK;
				charge.JR_RX_NKSellCurrency = new ZString(TestObjectCreator.USD.Code);
				charge.JR_OSSellAmt = 100m;

				charge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				charge.JR_OSCostAmt = 0m;

				CombineAssertions("PreCondition, the unrecognized charge should show 100(133.33 - 33.33) amount on screen..", () => {
					AssertEquals("CanRecognizeProfitOnWIPsAndAccruals", false, charge.CanRecognizeProfitOnWIPsAndAccruals);
					AssertEquals("JR_LocalSellAmt", 133.33m, charge.JR_LocalSellAmt);
					AssertEquals("JR_CFXAmt", 33.33m, charge.JR_CFXAmt);
				});
				Factory.Save();

				var coll = new GlobalProfitLossCollection(new JobProfitLoss(Factory), shipment);
				coll.Load();
				CombineAssertions("PreCondition, we get testing charge.", () => {
					AssertEquals("Count", 1, coll.Count);
					AssertEquals("Charge PK", charge.PK, coll[0].ZY_Calc_JR);
					AssertEquals("ZY_Calc_LineAmount", 100m, coll[0].ZY_Calc_LineAmount);
				});

				rateUSD.JF_BaseRate = 2m;
				rateUSD.JF_CFXPercent = 0;
				CombineAssertions("Because CFX rate and base rate are changed, charge's local sell amount is changed too.", () => {
					AssertEquals("JR_LocalSellAmt", 50m, charge.JR_LocalSellAmt);
					AssertEquals("JR_CFXAmt", 0m, charge.JR_CFXAmt);
				});

				coll.Load();
				AssertEquals("However, the unsaved charge amount should not affect profit loss amount.", 100m, coll[0].ZY_Calc_LineAmount);

				Factory.Save();
				coll.Load();
				AssertEquals("After charge is saved, we should get amount from charge.", 50m, coll[0].ZY_Calc_LineAmount);
			}
		}

		#region Implementation

		void AssertGlobalTotals(ProfitLossDetail detail, ZDecimal profitLoss, ZDecimal aCR, ZDecimal cST, ZDecimal rEV, ZDecimal wIP)
		{
			AssertEquals("Profit/Loss", profitLoss, detail.TotalLineAmount);
			AssertEquals("ACR", aCR, detail.TotalAccrual);
			AssertEquals("CST", cST, detail.TotalCost);
			AssertEquals("REV", rEV, detail.TotalRevenue);
			AssertEquals("WIP", wIP, detail.TotalWIP);
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
		#endregion

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}

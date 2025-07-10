using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class ProfitLossCollectionTest : TestCaseWithFactory
	{
		public void TestLoad()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			CommonShipment ship1 = consol.Shipments.AddNew();
			CommonShipment ship2 = consol.Shipments.AddNew();

			Job ship1Job = Job.CreateWithMutex(Factory, ship1);
			ship1Job.JH_ParentTableCode = "JS";
			ship1Job.JH_ParentID = ship1.PK;
			ship1Job.JH_GB = GlbBranch.CurrentBranch.PK;
			ship1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			ship1Job.JH_JobNum = "111";

			Charge chrg1 = ship1Job.Charges.AddNew();
			chrg1.JR_AC = Env.Registry.FreightChargeCode;
			chrg1.JR_LocalSellAmt = 300m;
			chrg1.JR_LocalCostAmt = 100m;

			Job ship2Job = Job.CreateWithMutex(Factory, ship2);
			ship2Job.JH_ParentTableCode = "JS";
			ship2Job.JH_ParentID = ship2.PK;
			ship2Job.JH_GB = GlbBranch.CurrentBranch.PK;
			ship2Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			ship2Job.JH_JobNum = "222";

			Charge chrg2 = ship2Job.Charges.AddNew();
			chrg2.JR_AC = Env.Registry.FreightChargeCode;
			chrg2.JR_LocalSellAmt = 400m;
			chrg2.JR_LocalCostAmt = 200m;

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			chrg1.CreateCostTransactionLine(invoice, ZDateTime.Now);

			JCJournalHeader cFXHeader = Factory.NewWithValidTestData<JCJournalHeader>();
			chrg1.CreateCFXTransactionLine(cFXHeader, ZDateTime.Now);
			chrg1.CFXLine.AL_OSAmount = chrg1.CFXLine.AL_LineAmount = 500m;
			chrg2.CreateCFXTransactionLine(cFXHeader, ZDateTime.Now);
			chrg2.CFXLine.AL_OSAmount = chrg2.CFXLine.AL_LineAmount = 600m;

			Factory.Save();

			JobProfitLoss pL = new JobProfitLoss(Factory);
			pL.SetConsol(consol);
			AssertEquals("6 items exist in details - 2 WIPs, 1 ACR, 1 CST, 2 REVs", 6, pL.ProfitLossDetails.Count);

			bool charge1WIPFound = false;
			bool charge1ACRFound = false;
			bool charge1CSTFound = false;
			bool charge1REVFound = false;
			bool charge2WIPFound = false;
			bool charge2ACRFound = false;
			bool charge2CSTFound = false;
			bool charge2REVFound = false;

			BusinessObjectFactory tempFactory = new BusinessObjectFactory();
			foreach (ProfitLossDetail detail in pL.ProfitLossDetails)
			{
				AssertEquals(Env.Registry.FreightChargeCode, detail.ZY_Calc_AC);
				AssertEquals("International Freight", detail.ZY_Calc_ChargeCodeDescription);

				if (detail.ZY_Calc_AH.IsValid)
				{
					AccTransactionHeader transHeader = tempFactory.Load<AccTransactionHeader>(detail.ZY_Calc_AH);
					AssertEquals("PostDate", transHeader.AH_PostDate, detail.ZY_Calc_PostDate);
					AssertEquals("InvoiceDate", transHeader.AH_InvoiceDate, detail.ZY_Calc_InvoiceDate);
				}
				else
				{
					AccTransactionLines transLine = tempFactory.Load<AccTransactionLines>(detail.ZY_Calc_AL);
					ZDateTime postDate = (transLine.AL_LineAmount == detail.ZY_Calc_LineAmount) ? transLine.AL_ReverseDate : transLine.AL_PostDate;
					AssertEquals("PostDate", postDate, detail.ZY_Calc_PostDate);
					AssertEquals("InvoiceDate", transLine.AL_PostDate, detail.ZY_Calc_InvoiceDate);
				}

				if (detail.ZY_Calc_LineAmount == 800m && detail.ZY_Calc_LineType == "WIP")
				{
					charge1WIPFound = true;
				}
				else if (detail.ZY_Calc_LineAmount == -100m)
				{
					if (detail.ZY_Calc_LineType == "ACR")
					{
						charge1ACRFound = true;
					}
					else if (detail.ZY_Calc_LineType == "CST")
					{
						charge1CSTFound = true;
					}
				}
				else if (detail.ZY_Calc_LineAmount == 1000m && detail.ZY_Calc_LineType == "WIP")
				{
					charge2WIPFound = true;
				}
				else if (detail.ZY_Calc_LineAmount == 500m && detail.ZY_Calc_LineType == "REV")
				{
					charge1REVFound = true;
				}
				else if (detail.ZY_Calc_LineAmount == -200m)
				{
					if (detail.ZY_Calc_LineType == "ACR")
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
			}

			Assert(charge1WIPFound);
			Assert(!charge1ACRFound);
			Assert(charge1CSTFound);
			Assert(charge1REVFound);
			Assert(charge2WIPFound);
			Assert(!charge2CSTFound);
			Assert(charge2REVFound);
			Assert(charge2ACRFound);

			pL.Filter.ChargeCodeFilter = "BLAH!";
			pL.ProfitLossDetails.Load();
			AssertEquals("No items exist with that charge code", 0, pL.ProfitLossDetails.Count);

			AccChargeCode freightChargeCode = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);

			pL.Filter.ChargeCodeFilter = freightChargeCode.AC_Code;
			pL.ProfitLossDetails.Load();
			AssertEquals("Should be 6 items in result collection", 6, pL.ProfitLossDetails.Count);

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
			AssertEquals("6 items exist again", 6, pL.ProfitLossDetails.Count);

			pL.Filter.JobNumberFilter = ship2Job.PK;
			pL.ProfitLossDetails.Load();
			AssertEquals("3 items exist", 3, pL.ProfitLossDetails.Count);
		}

		public void TestJobPKs()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			CommonShipment ship1 = consol.Shipments.AddNew();
			CommonShipment ship2 = consol.Shipments.AddNew();

			JobProfitLoss pL = new JobProfitLoss(Factory);
			ProfitLossCollection coll = new ProfitLossCollection(pL, consol);

			AssertEquals("No jobs", 0, coll.JobPKs.Length);

			Job ship1Job = Job.CreateWithMutex(Factory, ship1);
			ship1Job.JH_ParentTableCode = "JS";
			ship1Job.JH_ParentID = ship1.PK;
			ship1Job.JH_GB = GlbBranch.CurrentBranch.PK;
			ship1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			ship1Job.JH_JobNum = "111";
			Factory.Save();
			coll = new ProfitLossCollection(pL, consol);
			AssertEquals("1 job", 1, coll.JobPKs.Length);

			Job ship2Job = Job.CreateWithMutex(Factory, ship2);
			ship2Job.JH_ParentTableCode = "JS";
			ship2Job.JH_ParentID = ship2.PK;
			ship2Job.JH_GB = GlbBranch.CurrentBranch.PK;
			ship2Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			ship2Job.JH_JobNum = "222";
			Factory.Save();
			AssertEquals("2 jobs", 2, coll.JobPKs.Length);

			coll = new ProfitLossCollection(pL, new ZGuid[] { ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid() });
			AssertEquals("3 jobs", 3, coll.JobPKs.Length);
		}
	}
}
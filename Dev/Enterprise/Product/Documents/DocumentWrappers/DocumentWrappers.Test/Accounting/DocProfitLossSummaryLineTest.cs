using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocProfitLossSummaryLineTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolWrapper = DocForwardingConsol.New(consol, Factory);
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			ForwardingShipment shipment = consol.Shipments.AddNew();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			ObjectCreator.CreateRevenueLineAndCharge(job.PK, 500m, "FRT");
			ObjectCreator.CreateWIPLineAndCharge(job.PK, 400m, "FRT");
			ObjectCreator.CreateCostLineAndCharge(job.PK, 200m, "FRT");
			ObjectCreator.CreateAccrualLineAndCharge(job.PK, 200m, "FRT");
			Factory.Save();

			job.LoadCharges_ForTestOnly();

			consolWrapper = DocForwardingConsol.New(consol, Factory);
			AssertEquals("TotalProfit", 500M, consolWrapper.TotalProfit);

			foreach (ProfitLossSummaryDetailView line in consolWrapper.ProfitLoss.ProfitLossSummaryFilteredDetails)
			{
				var lineWrapper = DocProfitLossSummaryLine.New(line, Factory);
				AssertEquals("Branch", line.Branch.GB_Code, lineWrapper.Branch);
				AssertEquals("Department", line.Department.GE_Code, lineWrapper.Department);
				AssertEquals("Revenue", line.ZZ_Calc_Revenue, lineWrapper.Revenue);
				AssertEquals("WIP", line.ZZ_Calc_WIP, lineWrapper.WIP);
				AssertEquals("Cost", -line.ZZ_Calc_Cost, lineWrapper.Cost);
				AssertEquals("Accrual", -line.ZZ_Calc_Accrual, lineWrapper.Accrual);
				AssertEquals("Income", line.ZZ_Calc_Revenue + line.ZZ_Calc_WIP, lineWrapper.Income);
				AssertEquals("Expense", line.ZZ_Calc_Cost + line.ZZ_Calc_Accrual, lineWrapper.Expense);
				AssertEquals("Profit", line.ZZ_Calc_Revenue + line.ZZ_Calc_WIP + line.ZZ_Calc_Cost + line.ZZ_Calc_Accrual, lineWrapper.Profit);
				AssertEquals("ChargeCode", line.ChargeCode.AC_Code, lineWrapper.ChargeCode.Code);
				AssertEquals("JobNum", line.Job.JH_JobNum, lineWrapper.Job.JobNum);
			}
		}

		TestObjectCreator ObjectCreator
		{
			get
			{
				if (fObjectCreator == null)
				{
					fObjectCreator = new TestObjectCreator(Factory);
				}

				return fObjectCreator;
			}
		}
		TestObjectCreator fObjectCreator;
	}
}

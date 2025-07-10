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
	sealed class DocProfitLossDetailedLineTest : TestCaseWithFactory
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

			foreach (ProfitLossDetailView line in consolWrapper.ProfitLoss.ProfitLossFilteredDetails)
			{
				var lineWrapper = DocProfitLossDetailedLine.New(line, Factory);
				AssertEquals("Branch", line.Branch.GB_Code, lineWrapper.Branch);
				AssertEquals("Department", line.Department.GE_Code, lineWrapper.Department);
				AssertEquals("LineAmount", line.ZY_Calc_LineAmount, lineWrapper.LineAmount);
				AssertEquals("LineType", line.ZY_Calc_LineType, lineWrapper.LineType);
				AssertEquals("Income", line.ZY_Calc_ARLine.IsValid ? lineWrapper.LineAmount : 0, lineWrapper.Income);
				AssertEquals("Expense", line.ZY_Calc_APLine.IsValid ? -lineWrapper.LineAmount : 0, lineWrapper.Expense);
				AssertEquals("Profit", line.ZY_Calc_LineAmount != 0 ? line.ZY_Calc_LineAmount : 0, lineWrapper.Profit);
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

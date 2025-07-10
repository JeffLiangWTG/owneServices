using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class JobProfitLossTotalsControlTest : TestCaseWithFactory
	{
		protected JobProfitLossTotalsControl ControlToTest
		{
			get { return new JobProfitLossTotalsControl(); }
		}

		public void TestBindPrepend()
		{
			using (JobProfitLossTotalsControl ctrl = ControlToTest)
			{
				ctrl.SetDataBinding(null, "yuri.");
				ctrl.SetDataBinding(null, "andrei.");
				ctrl.Show();

				Assert(ctrl.TotalRevenue_ForTestOnly.BindToDecimalPlaces.IndexOf("yuri.") > -1);
				Assert(ctrl.TotalRevenueRecognized_ForTestOnly.BindToDecimalPlaces.IndexOf("yuri.") > -1);
				Assert(ctrl.TotalRevenueNotRecognized_ForTestOnly.BindToDecimalPlaces.IndexOf("yuri.") > -1);
				Assert(ctrl.TotalWIP_ForTestOnly.BindToDecimalPlaces.IndexOf("yuri.") > -1);
				Assert(ctrl.TotalWIPRecognized_ForTestOnly.BindToDecimalPlaces.IndexOf("yuri.") > -1);
				Assert(ctrl.TotalWIPNotRecognized_ForTestOnly.BindToDecimalPlaces.IndexOf("yuri.") > -1);
				Assert(ctrl.TotalAccrual_ForTestOnly.BindToDecimalPlaces.IndexOf("yuri.") > -1);
				Assert(ctrl.TotalAccrualRecognized_ForTestOnly.BindToDecimalPlaces.IndexOf("yuri.") > -1);
				Assert(ctrl.TotalAccrualNotRecognized_ForTestOnly.BindToDecimalPlaces.IndexOf("yuri.") > -1);
				Assert(ctrl.TotalCost_ForTestOnly.BindToDecimalPlaces.IndexOf("yuri.") > -1);
				Assert(ctrl.TotalCostRecognized_ForTestOnly.BindToDecimalPlaces.IndexOf("yuri.") > -1);
				Assert(ctrl.TotalCostNotRecognized_ForTestOnly.BindToDecimalPlaces.IndexOf("yuri.") > -1);
				Assert(ctrl.TotalLineAmount_ForTestOnly.BindToDecimalPlaces.IndexOf("yuri.") > -1);
				Assert(ctrl.TotalLineAmountRecognized_ForTestOnly.BindToDecimalPlaces.IndexOf("yuri.") > -1);
				Assert(ctrl.TotalLineAmountNotRecognized_ForTestOnly.BindToDecimalPlaces.IndexOf("yuri.") > -1);
				Assert(ctrl.TotalTaxExpenseRevenue_ForTestOnly.BindToDecimalPlaces.IndexOf("yuri.") > -1);
				Assert(ctrl.TotalTaxExpenseCost_ForTestOnly.BindToDecimalPlaces.IndexOf("yuri.") > -1);
			}
		}

		public void TestTaxExpensePanelVisibility()
		{
			var creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S001001", false);
			var job = creator.CreateJob(shipment, createWithMutex: false);

			var arInvoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV10010", creator.AUD, 1M, 150M, 0M, 150M, 0M, creator.ABIGAS, creator.CC1.PK);
			var revenueLine = arInvoice.Lines[0];
			revenueLine.AL_JH = job.PK;
			creator.CreateJobCharge(revenueLine, job, creator.CC1, creator.AUD);

			Factory.Save();

			var profitLoss = new JobProfitLoss(Factory);
			profitLoss.SetParent(shipment);
			profitLoss.SetJobPKs(new[] { job.PK });
			var company = GlbCompany.CurrentCompany;
			var taxConfig = Factory.New<AccTaxConfiguration>();
			taxConfig.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			taxConfig.ETC_ParentId = company.PK;

			Assert(profitLoss.IsTaxExpenseSupported);
			using (var form = new ZForm())
			using (var ctrl = ControlToTest)
			{
				ctrl.SetDataBinding(job, "ProfitLoss");
				form.Controls.Add(ctrl);
				form.Show();
				Application.DoEvents();
				Assert(ctrl.TaxExpensePanel_ForTestOnly.Visible);
			}

			taxConfig.ETC_ParentId = ZGuid.Empty;
			Assert(!profitLoss.IsTaxExpenseSupported);
			using (var form = new ZForm())
			using (var ctrl = ControlToTest)
			{
				ctrl.SetDataBinding(job, "ProfitLoss");
				form.Controls.Add(ctrl);
				form.Show();
				Application.DoEvents();
				Assert(!ctrl.TaxExpensePanel_ForTestOnly.Visible);
			}

			AssertEquals(2, profitLoss.GlobalJobCostingProfitLoss.Count);
			using (var form = new ZForm())
			using (var ctrl = ControlToTest)
			{
				ctrl.SetDataBinding(job, "ProfitLoss.GlobalJobCostingProfitLoss");
				form.Controls.Add(ctrl);
				form.Show();
				Application.DoEvents();
				Assert(ctrl.TaxExpensePanel_ForTestOnly.Visible);
			}
		}
	}
}

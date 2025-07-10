using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ConsolRevenue;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting.Testing
{
	public class ConsolRevenueApportionmentUserControlTest : TestCaseWithFactory
	{
		public void TestColumnsAddCorrectly()
		{
			using (var control = new ConsolRevenueApportionmentUserControl())
			{
				var columns = control.RevenueGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				AssertColumnsAddCorrectly("UnApportionedAmount", columns);

				columns = control.ApportionedChargesGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				AssertColumnsAddCorrectly("IsUsedForApportionment", columns, false);
				AssertColumnsAddCorrectly("JR_RL_NKOrigin", columns);
				AssertColumnsAddCorrectly("JR_RL_NKDestination", columns);
			}

			void AssertColumnsAddCorrectly(string expectedColumn, IEnumerable<ZGridColumnInfo> columns, bool? isReadOnly = null)
			{
				Assert("New column should be added.", columns.Any(x => x.ColumnName == expectedColumn));
				Assert("IsVisible", columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsVisible);
				if (isReadOnly != null)
				{
					AssertEquals("IsReadOnly", isReadOnly, columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsReadOnly);
				}
			}
		}

		public void TestCreditorAndDebtorColumnVisibility()
		{
			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			ConsolRevenueMaster revenueMaster = new ConsolRevenueMaster(consol, Factory);
			ConsolRevenue revenue = revenueMaster.Revenues.AddNew();

			using (ConsolRevenueApportionForm form = new ConsolRevenueApportionForm(revenueMaster))
			{
				form.Show();
				Application.DoEvents();

				Assert("Debtor Column", form.ConsolRevenuesControl.ApportionedChargesGrid.Columns.Contains(JobChargeSchema.JR_OH_SellAccount.Name));
			}

			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			using (ConsolRevenueApportionForm form = new ConsolRevenueApportionForm(revenueMaster))
			{
				form.Show();
				Application.DoEvents();

				Assert("Debtor Column", form.ConsolRevenuesControl.ApportionedChargesGrid.Columns.Contains(JobChargeSchema.JR_OH_SellAccount.Name));
			}

			revenueMaster.ReleaseMutexes();
		}
	}
}

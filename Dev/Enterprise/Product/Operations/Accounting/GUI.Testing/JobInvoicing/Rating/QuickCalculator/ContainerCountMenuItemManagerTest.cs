using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class ContainerCountMenuItemManagerTest : TestCaseWithFactory
	{
		public void TestQuickCalculate_RowSelected()
		{
			var orgFactory = new BusinessObjectFactory();
			var testDebtor = orgFactory.NewWithValidTestData<OrgHeader>();
			orgFactory.Save();

			var job = Factory.NewJobForTesting<Job>();
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_ChargeType = "MRG";
			charge.JR_MarginPercentage = 0m;
			charge.JR_OH_SellAccount = testDebtor.PK;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(Factory.New<DummyAutoRating>(), charge);

			using (var form = new ZForm(quickCalculator))
			using (var grid = new ZGrid())
			{
				grid.BindTo = "Containers";
				grid.Columns.AddTextColumn("ContainerType", 100);
				form.Controls.Add(grid);
				form.Show();
				grid.SetDataBinding(quickCalculator, "Containers");

				new ContainerCountMenuItemManager(grid).AddMenuItem();

				MenuItem menuItem = grid.ContextMenu.MenuItems.FindByText("Assign Container(s)");
				AssertNotNull(menuItem);

				grid.Select(0);

				using (ZFormModaliser.SuspendDispose())
				{
					menuItem.PerformClick();
					AssertEquals(typeof(ContainersSelectionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				}
			}
		}
	}
}

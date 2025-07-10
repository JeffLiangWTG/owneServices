using Enterprise.Customs.Forwarding.GUI;
using Enterprise.Customs.Forwarding.GUI.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Forwarding.Module.Testing
{
	class ForwardingConsolModuleCustomsColumnsProviderTest : ForwardingConsolModuleCustomsColumnsProviderAbstractTest
	{
		public void TestAddedColumns()
		{
			var consols = Factory.New<ForwardingShipment>().Consols;

			using (var form = new ZForm())
			using (var grid = new DumyZGrid())
			{
				form.Controls.Add(grid);
				form.Show();

				grid.SetDataBinding(consols, "");
				var columnsProvider = new ForwardingConsolModuleCustomsColumnsProvider();
				columnsProvider.AddColumns(grid);

				AssertNotNull(FindColumnByName(grid, ForwardingConsolCustomsColumnConstants.Schema.CustomsCargoStatus));
				AssertNotNull(FindColumnByName(grid, ForwardingConsolCustomsColumnConstants.Schema.AMSBillStatus));
				AssertNotNull(FindColumnByName(grid, ForwardingConsolCustomsColumnConstants.Schema.AMSBillStatusDescription));
				AssertNotNull(FindColumnByName(grid, ForwardingConsolCustomsColumnConstants.Schema.LatestAMSDispositionCode));
				AssertNotNull(FindColumnByName(grid, ForwardingConsolCustomsColumnConstants.Schema.LatestAMSDispositionDesc));
				AssertNotNull(FindColumnByName(grid, ForwardingConsolCustomsColumnConstants.Schema.AsycudaRegistrationStatus));
			}
		}

		public void TestOutwardReportColumnsAppearOnlyForNZ()
		{
			var consols = Factory.New<ForwardingShipment>().Consols;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);

			using (var form = new ZForm())
			using (var grid = new DumyZGrid())
			{
				form.Controls.Add(grid);
				form.Show();

				grid.SetDataBinding(consols, "");
				var columnsProvider = new ForwardingConsolModuleCustomsColumnsProvider();
				columnsProvider.AddColumns(grid);

				AssertNull("OutwardReportStatus column should not be available for non NZ companies", FindColumnByName(grid, ForwardingConsolCustomsColumnConstants.Schema.OutwardReportEntryNumber));
				AssertNull("OutwardReportStatusDescription column should not be available for non NZ companies", FindColumnByName(grid, ForwardingConsolCustomsColumnConstants.Schema.OutwardReportStatus));
				AssertNull("OutwardReportEntryNumber column should not be available for non NZ companies", FindColumnByName(grid, ForwardingConsolCustomsColumnConstants.Schema.OutwardReportStatusDescription));
			}

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			using (var form = new ZForm())
			using (var grid = new DumyZGrid())
			{
				form.Controls.Add(grid);
				form.Show();

				grid.SetDataBinding(consols, "");
				var columnsProvider = new ForwardingConsolModuleCustomsColumnsProvider();
				columnsProvider.AddColumns(grid);

				AssertNotNull("OutwardReportStatus column should be available for NZ companies", FindColumnByName(grid, ForwardingConsolCustomsColumnConstants.Schema.OutwardReportEntryNumber));
				AssertNotNull("OutwardReportStatusDescription column should be available for NZ companies", FindColumnByName(grid, ForwardingConsolCustomsColumnConstants.Schema.OutwardReportStatus));
				AssertNotNull("OutwardReportEntryNumber column should be available for NZ companies", FindColumnByName(grid, ForwardingConsolCustomsColumnConstants.Schema.OutwardReportStatusDescription));
			}
		}

		public void TestJPColumn()
		{
			var consols = Factory.New<ForwardingShipment>().Consols;

			using (var form = new ZForm())
			using (var grid = new DumyZGrid())
			{
				form.Controls.Add(grid);
				form.Show();

				grid.SetDataBinding(consols, "");
				var columnsProvider = new ForwardingConsolModuleCustomsColumnsProvider();
				columnsProvider.AddColumns(grid);

				AssertNotNull(FindColumnByName(grid, ForwardingConsolCustomsColumnConstants.Schema.AFRBillStatus));
				AssertNotNull(FindColumnByName(grid, ForwardingConsolCustomsColumnConstants.Schema.AFRBillStatusDescription));
			}
		}

		public void TestUSAMSColumn()
		{
			var consols = Factory.New<ForwardingShipment>().Consols;

			using (var form = new ZForm())
			using (var grid = new DumyZGrid())
			{
				form.Controls.Add(grid);
				form.Show();

				grid.SetDataBinding(consols, "");
				var columnsProvider = new ForwardingConsolModuleCustomsColumnsProvider();
				columnsProvider.AddColumns(grid);

				AssertNotNull(FindColumnByName(grid, ForwardingConsolCustomsColumnConstants.Schema.AMSBillStatus));
				AssertNotNull(FindColumnByName(grid, ForwardingConsolCustomsColumnConstants.Schema.AMSBillStatusDescription));
			}
		}

		public void TestBashFetchForView_CustomsCargoStatus()
		{
			BashFetchForView("CustomsCargoStatus", 0);
		}

		public void TestBashFetchForView_AMSBillStatus()
		{
			BashFetchForView("AMSBillStatus", 0);
		}

		public void TestBashFetchForView_AMSBillStatusDescription()
		{
			BashFetchForView("AMSBillStatusDescription", 0);
		}

		public void TestBashFetchForView_AFRBillStatus()
		{
			BashFetchForView("AFRBillStatus", 0);
		}

		public void TestBashFetchForView_AFRBillStatusDescription()
		{
			BashFetchForView("AFRBillStatusDescription", 0);
		}

		public void TestBashFetchForView_LatestAMSDispositionCode()
		{
			BashFetchForView(ForwardingConsolCustomsColumnConstants.Schema.LatestAMSDispositionCode, 0);
		}

		public void TestBashFetchForView_LatestAMSDispositionDesc()
		{
			BashFetchForView(ForwardingConsolCustomsColumnConstants.Schema.LatestAMSDispositionDesc, 0);
		}

		public void TestBashFetchForView_AsycudaRegistrationStatus()
		{
			BashFetchForView(ForwardingConsolCustomsColumnConstants.Schema.AsycudaRegistrationStatus, 0);
		}
	}
}

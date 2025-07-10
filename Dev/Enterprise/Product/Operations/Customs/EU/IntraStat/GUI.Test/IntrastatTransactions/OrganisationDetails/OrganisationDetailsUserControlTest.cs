using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.EU.Intrastat.GUI.Testing.IntrastatTransactions.OrganisationDetails
{
	sealed class OrganisationDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(CusIntrastatHeader), control.BindingSource.DataSourceType);
		}

		public void TestConsigneeOrganisationControl()
		{
			var consigneeOrganisationControl = control.ConsigneeOrganisationControl;

			AssertType<ZOrganisationControl>("Type", consigneeOrganisationControl);
			AssertEquals("BindTo", nameof(CusIntrastatHeader.CIH_OH_Consignee), consigneeOrganisationControl.BindTo);
			AssertEquals("BindToList", nameof(CusIntrastatHeader.Lookups) + "." + nameof(CusIntrastatHeaderLookups.Consignees), consigneeOrganisationControl.BindToOrganisations);
		}

		public void TestSupplierOrganisationControl()
		{
			var supplierOrganisationControl = control.SupplierOrganisationControl;

			AssertType<ZOrganisationControl>("Type", supplierOrganisationControl);
			AssertEquals("BindTo", nameof(CusIntrastatHeader.CIH_OH_Supplier), supplierOrganisationControl.BindTo);
			AssertEquals("BindToList", nameof(CusIntrastatHeader.Lookups) + "." + nameof(CusIntrastatHeaderLookups.Suppliers), supplierOrganisationControl.BindToOrganisations);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new OrganisationDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		OrganisationDetailsUserControl control;
	}
}

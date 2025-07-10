using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class OrganisationsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestDefermentPartyDocAddressControl()
		{
			AssertType<ZDocAddressControl>(control.DefermentPartyDocAddressControl);
		}

		public void TestExporterDocAddressControl()
		{
			AssertType<ZDocAddressControl>(control.ExporterDocAddressControl);
		}

		public void TestContractualPartnerDocAddressControl()
		{
			AssertType<ZDocAddressControl>(control.ContractualPartnerDocAddressControl);
		}

		public void TestCarrierEUBorderDocAddressControl()
		{
			AssertType<ZDocAddressControl>(control.CarrierEUBorderDocAddressControl);
		}

		public void TestDutyPayerAddressControl()
		{
			AssertType<ZGuidFindBox>(control.DutyPayerGuidFindBox);
		}

		OrganisationsUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new OrganisationsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}

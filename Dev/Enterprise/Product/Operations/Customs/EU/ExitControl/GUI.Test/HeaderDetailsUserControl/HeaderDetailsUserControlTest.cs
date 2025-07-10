using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Organisation.UserControls.Address;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class HeaderDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(CusExitHeader), userControl.BindingSource.DataSourceType);
		}

		public void TestCarrierAddressWithContactControl()
		{
			AssertType<ZAddressWithContactControl>(userControl.CarrierAddressWithContactControl);
		}

		public void TestBranchGuidFindBox()
		{
			var branchGuidFindBox = userControl.BranchGuidFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZGuidFindBox>("Type", branchGuidFindBox);
				AssertEquals("Caption", "Branch", branchGuidFindBox.CaptionResourceString.Caption);
				AssertEquals("BindTo", nameof(CusExitHeader.CXH_GB_Branch), branchGuidFindBox.BindTo);
			});
		}

		public void TestBrokerCodeFindBox()
		{
			var brokerCodeFindBox = userControl.BrokerCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", brokerCodeFindBox);
				AssertEquals("Caption", "Broker", brokerCodeFindBox.CaptionResourceString.Caption);
				AssertEquals("BindTo", nameof(CusExitHeader.CXH_GS_NKCustomsAgent), brokerCodeFindBox.BindTo);
			});
		}

		public void TestExporterOrgAddressControl()
		{
			var exporterOrgAddressControl = userControl.ExporterOrgAddressControl;
			CombineAssertions(() =>
			{
				AssertType<ZOrganisationControl>("Type", exporterOrgAddressControl);
				AssertEquals("Exporter/Client", "Exporter/Client", exporterOrgAddressControl.CaptionResourceString.Caption);
				AssertEquals("BindTo", nameof(CusExitHeader.CXH_OH_Exporter), userControl.BindingSource.GetBindingMember(exporterOrgAddressControl));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new HeaderDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
		HeaderDetailsUserControl userControl;
	}
}

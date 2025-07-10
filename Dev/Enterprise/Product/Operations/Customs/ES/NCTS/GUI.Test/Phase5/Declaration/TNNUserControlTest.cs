using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	sealed class TNNUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using (var userControl = new TNNUserControl())
			{
				AssertEquals(typeof(NctsHeader), userControl.BindingSource.DataSourceType);
			}
		}
		public void TestTabControlPages()
		{
			var expectedTabPagesInOrder = new string[]
			{
				"DeclarationDetailsTabPage", "TransportAndPackagingTabPage", "HouseConsignmentsTabPage", "AnnexTabPage"
			};

			using (var userControl = new TNNUserControl())
			{
				var mainTabControl = userControl.FindSingle<ZTemplateTabControl>("MainTabControl");
				AssertSequencesEqual("TabPageNames", expectedTabPagesInOrder, mainTabControl.TabPages.Cast<ZTabPage>().Select(x => x.Name));
			}
		}

		[RequiresSTA]
		public void TestControls()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			using (var form = new ZForm(header))
			using (var userControl = new TNNUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var mainTabControl = userControl.FindSingle<ZTemplateTabControl>("MainTabControl");

				CombineAssertions(() =>
				{
					mainTabControl.SelectedTab = userControl.DeclarationDetailsTabPage;
					AssertEquals("DeclarationDetailsTabPage Caption", "Details", userControl.DeclarationDetailsTabPage.CaptionResourceString.Caption);
					AssertEquals("DeclarationDetailsTabUserControl is within DeclarationDetailsTabPage", true, userControl.DeclarationDetailsTabPage.Contains(userControl.DeclarationDetailsTabUserControl));
					AssertEquals("DeclarationDetailsTabUserControl DataSourceType", typeof(EU.NCTS.Business.NctsHeader), userControl.DeclarationDetailsTabUserControl.BindingSource.DataSourceType);
					AssertType<Phase5DeclarationDetailsTabUserControl>("DeclarationDetailsTabUserControl type", userControl.DeclarationDetailsTabUserControl);

					mainTabControl.SelectedTab = userControl.TransportAndPackagingTabPage;
					AssertEquals("TransportAndPackagingTabPage Caption", "Transport && Containers", userControl.TransportAndPackagingTabPage.CaptionResourceString.Caption);
					AssertEquals("TransportAndPackagingTabUserControl is within TransportAndPackagingTabPage", true, userControl.TransportAndPackagingTabPage.Contains(userControl.TransportAndPackagingTabUserControl));
					AssertEquals("TransportAndPackagingTabUserControl DataSourceType", typeof(EU.NCTS.Business.NctsHeader), userControl.TransportAndPackagingTabUserControl.BindingSource.DataSourceType);
					AssertType<EU.NCTS.GUI.Phase5TransportAndPackagingTabUserControl>("TransportAndPackagingTabUserControl type", userControl.TransportAndPackagingTabUserControl);

					mainTabControl.SelectedTab = userControl.HouseConsignmentsTabPage;
					AssertEquals("HouseConsignmentsTabPage Caption", "House Consignments", userControl.HouseConsignmentsTabPage.CaptionResourceString.Caption);
					AssertEquals("HouseConsignmentsTabUserControl is within HouseConsignmentsTabPage", true, userControl.HouseConsignmentsTabPage.Contains(userControl.HouseConsignmentsTabUserControl));
					AssertEquals("HouseConsignmentsTabUserControl DataSourceType", typeof(EU.NCTS.Business.INctsBillCollection<EU.NCTS.Business.NctsBill>), userControl.HouseConsignmentsTabUserControl.BindingSource.DataSourceType);
					AssertType<HouseConsignmentsTabUserControl>("HouseConsignmentsTabUserControl type", userControl.HouseConsignmentsTabUserControl);

					mainTabControl.SelectedTab = userControl.AnnexTabPage;
					AssertEquals("AnnexTabPage Caption", "Annexes", userControl.AnnexTabPage.CaptionResourceString.Caption);
					AssertEquals("AnnexesTabUserControl is within AnnexTabPage", true, userControl.AnnexTabPage.Contains(userControl.AnnexesTabUserControl));
					AssertEquals("AnnexesTabUserControl DataSourceType", typeof(ES.Business.Declaration.CusStorageDocPivotCollection), userControl.AnnexesTabUserControl.BindingSource.DataSourceType);
					AssertType<ES.GUI.AnnexesTabUserControl>("AnnexesTabUserControl type", userControl.AnnexesTabUserControl);
				});
			}
		}
	}
}

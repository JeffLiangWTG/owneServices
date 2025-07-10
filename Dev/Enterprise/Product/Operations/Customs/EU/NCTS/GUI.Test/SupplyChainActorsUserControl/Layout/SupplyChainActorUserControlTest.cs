using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class SupplyChainActorUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(CusSupplyChainActorReference), control.BindingSource.DataSourceType);
		}

		public void TestRoleDropEdit()
		{
			var roleDropEdit = control.RoleDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", roleDropEdit);
				AssertEquals("BindTo", nameof(CusSupplyChainActorReference.CFR_Code), roleDropEdit.BindTo);
				AssertEquals("ShowDescriptionBox", true, roleDropEdit.ShowDescriptionBox);
			});
		}

		public void TestReferenceTextBox()
		{
			var referenceTextBox = control.ReferenceTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", referenceTextBox);
				AssertEquals("BindTo", nameof(CusSupplyChainActorReference.CFR_Reference), referenceTextBox.BindTo);
				AssertEquals("CharacterCasing", referenceTextBox.CharacterCasing, System.Windows.Forms.CharacterCasing.Normal);
			});
		}

		public void TestOwnerOrganisationFindBox()
		{
			var ownerOrganisationFindBox = control.OwnerOrganisationFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZOrganisationFindBox>("Type", ownerOrganisationFindBox);
				AssertEquals("BindTo", nameof(CusSupplyChainActorReference.OwnerOrgPK), ownerOrganisationFindBox.BindTo);
				AssertEquals("ShowDescriptionBox", false, ownerOrganisationFindBox.ShowDescriptionBox);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new SupplyChainActorUserControl();
		}
		SupplyChainActorUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	sealed class SupplyChainActorUserControlTest : TestCaseWithFactory
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
			});
		}
		public void TestReferenceNumberTextBox()
		{
			var referenceNumberTextBox = control.ReferenceNumberTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", referenceNumberTextBox);
				AssertEquals("BindTo", nameof(CusSupplyChainActorReference.CFR_Reference), referenceNumberTextBox.BindTo);
			});
		}

		public void TestOwnerOrganisationFindBox()
		{
			var ownerOrganisationFindBox = control.OwnerOrganisationFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZOrganisationFindBox>("Type", ownerOrganisationFindBox);
				AssertEquals("BindTo", nameof(CusSupplyChainActorReference.OwnerOrgPK), ownerOrganisationFindBox.BindTo);
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

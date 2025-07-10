using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing
{
	sealed class CusGoodsLocationUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals("CusGoodsLocationUserControl data source type", typeof(CusGoodsLocation), control.BindingSource.DataSourceType);
		}

		public void TestOrganisationFindBox()
		{
			var organisationFindBox = control.OrganisationFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("BindTo", nameof(CusGoodsLocation.AddressIdentificationHolderPK), organisationFindBox.BindTo);
				AssertEquals("ModuleID", ZArchitecture.Modules.ModuleIDs.Organisation, organisationFindBox.ModuleID);
			});
		}

		public void TestAdditionalIdentifierDropEdit()
		{
			var additionalIdentifierDropEdit = control.AdditionalIdentifierDropEdit;

			CombineAssertions(() =>
			{
				AssertEquals("BindTo", nameof(CusGoodsLocation.CGL_AdditionalIdentifier), additionalIdentifierDropEdit.BindTo);
			});
		}

		public void TestAuthorizationCodeFindBox()
		{
			var authorizationCodeFindBox = control.AuthorizationCodeFindBox;

			AssertEquals("BindTo", nameof(CusGoodsLocation.AddressAuthorisationNumber), authorizationCodeFindBox.BindTo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new CusGoodsLocationUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		CusGoodsLocationUserControl control;
	}
}

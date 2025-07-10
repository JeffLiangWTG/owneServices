using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class IncidentDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals(typeof(EnRouteIncident), userControl.BindingSource.DataSourceType);
		}

		public void TestIncidentCode()
		{
			var incidentCode = userControl.IncidentCodeDropEdit;
			CombineAssertions(() =>
			{
				AssertEquals("Binding", "BN_IncidentCode", incidentCode.GetBindingMember());
				AssertType<ZDropEdit>("Type", incidentCode);
			});
		}

		public void TestInformation()
		{
			var information = userControl.InformationTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Binding", "BN_Information", information.GetBindingMember());
				AssertType<ZTextBox>("Type", information);
				AssertEquals("Character casing", System.Windows.Forms.CharacterCasing.Normal, information.CharacterCasing);
			});
		}

		public void TestEndorsementDate()
		{
			var endorsementDate = userControl.EndorsementDateEdit;
			CombineAssertions(() =>
			{
				AssertEquals("Binding", "BN_EndorsementDate", endorsementDate.GetBindingMember());
				AssertType<ZDateEdit>("Type", endorsementDate);
			});
		}

		public void TestEndorsementAuthority()
		{
			var endorsementAuthority = userControl.EndorsementAuthorityTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Binding", "BN_EndorsementAuthority", endorsementAuthority.GetBindingMember());
				AssertType<ZTextBox>("Type", endorsementAuthority);
				AssertEquals("Character casing", System.Windows.Forms.CharacterCasing.Normal, endorsementAuthority.CharacterCasing);
			});
		}

		public void TestEndorsementCountryCode()
		{
			var endorsementCountryCode = userControl.EndorsementCountryCodeDropEdit;
			CombineAssertions(() =>
			{
				AssertEquals("Binding", "BN_EndorsementCountryCode", endorsementCountryCode.GetBindingMember());
				AssertType<ZDropEdit>("Type", endorsementCountryCode);
			});
		}

		public void TestEndorsementPlace()
		{
			var endorsementPlace = userControl.EndorsementPlaceTextBox;

			CombineAssertions(() =>
			{
				AssertEquals("Binding", "BN_EndorsementPlace", endorsementPlace.GetBindingMember());
				AssertType<ZTextBox>("Type", endorsementPlace);
				AssertEquals("Character casing", System.Windows.Forms.CharacterCasing.Normal, endorsementPlace.CharacterCasing);
			});
		}

		public void TestLocationOfGoodsUserControl()
		{
			var goodsUserControl = userControl.LocationOfGoodsUserControl;
			CombineAssertions(() =>
			{
				AssertEquals("Binding", ".", goodsUserControl.GetBindingMember());
				AssertType<EU.GUI.LocationOfGoodsUserControl>("Type", goodsUserControl);
				AssertEquals("CusGoodsLocationProviderType", typeof(EnRouteIncident), goodsUserControl.CusGoodsLocationProviderType);
			});
		}

		public void TestEventCountryCodeDropEdit()
		{
			var eventCountryCodeDropEdit = userControl.EventCountryCodeDropEdit;
			CombineAssertions(() =>
			{
				AssertEquals("Binding", "BN_EventCountryCode", eventCountryCodeDropEdit.GetBindingMember());
				AssertType<ZDropEdit>("Type", eventCountryCodeDropEdit);
			});
		}

		public void TestTransportMeansGroupUserControl()
		{
			var transportMeansGroupUserControl = userControl.TransportMeansGroupUserControl;
			CombineAssertions(() =>
			{
				AssertEquals("Binding", ".", transportMeansGroupUserControl.GetBindingMember());
				AssertType<Phase5TransportMeansGroupUserControl>("Type", transportMeansGroupUserControl);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new IncidentDetailsUserControl();
		}
		IncidentDetailsUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}

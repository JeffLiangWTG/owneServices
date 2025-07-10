using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class CusGoodsLocationUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(CusGoodsLocation), control.BindingSource.DataSourceType);
		}

		public void TestQualifierDropEdit()
		{
			var qualifierDropEdit = control.QualifierDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", qualifierDropEdit);
				AssertEquals("BindTo", nameof(CusGoodsLocation.CGL_Qualifier), qualifierDropEdit.BindTo);
			});
		}

		public void TestTypeDropEdit()
		{
			var typeDropEdit = control.TypeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", typeDropEdit);
				AssertEquals("BindTo", nameof(CusGoodsLocation.CGL_Type), typeDropEdit.BindTo);
			});
		}

		public void TestAdditionalIdentifierTextBox()
		{
			var additionalIdentifierTextBox = control.AdditionalIdentifierTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", additionalIdentifierTextBox);
				AssertEquals("BindTo", nameof(CusGoodsLocation.AdditionalIdentifier), additionalIdentifierTextBox.BindTo);
			});
		}

		public void TestPostcodeTextBox()
		{
			var postcodeTextBox = control.PostcodeTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", postcodeTextBox);
				AssertEquals("BindTo", nameof(CusGoodsLocation.Address) + "." + nameof(CusGoodsLocationAddress.E2_Postcode), postcodeTextBox.BindTo);
			});
		}

		public void TestCountryCodeFindBox()
		{
			var countryCodeFindBox = control.CountryCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", countryCodeFindBox);
				AssertEquals("BindTo", nameof(CusGoodsLocation.Address) + "." + nameof(CusGoodsLocationAddress.E2_RN_NKCountryCode), countryCodeFindBox.BindTo);
				AssertEquals("ModuleID", ZArchitecture.Modules.ModuleIDs.RefCountry, countryCodeFindBox.ModuleID);
			});
		}

		public void TestUnlocoCodeFindBox()
		{
			var unlocoCodeFindBox = control.UnlocoCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", unlocoCodeFindBox);
				AssertEquals("BindTo", nameof(CusGoodsLocation.Unlocode), unlocoCodeFindBox.BindTo);
			});
		}

		public void TestCustomsOfficeCodeFindBox()
		{
			var customsOfficeCodeFindBox = control.CustomsOfficeCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", customsOfficeCodeFindBox);
				AssertEquals("BindTo", nameof(CusGoodsLocation.CGL_CustomsOffice), customsOfficeCodeFindBox.BindTo);
			});
		}

		public void TestGeoLocationLatitudeTextBox()
		{
			var geoLocationLatitudeTextBox = control.GeoLocationLatitudeTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", geoLocationLatitudeTextBox);
				AssertEquals("BindTo", nameof(CusGoodsLocation.Address) + "." + nameof(CusGoodsLocationAddress.E2_Latitude), geoLocationLatitudeTextBox.BindTo);
				AssertEquals("DecimalPlaces", 7, geoLocationLatitudeTextBox.DecimalPlaces);
			});
		}

		public void TestGeoLocationLongitudeTextBox()
		{
			var geoLocationLongitudeTextBox = control.GeoLocationLongitudeTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", geoLocationLongitudeTextBox);
				AssertEquals("BindTo", nameof(CusGoodsLocation.Address) + "." + nameof(CusGoodsLocationAddress.E2_Longitude), geoLocationLongitudeTextBox.BindTo);
				AssertEquals("DecimalPlaces", 7, geoLocationLongitudeTextBox.DecimalPlaces);
			});
		}

		public void TestOrganisationFindBox()
		{
			var organisationCodeFindBox = control.OrganisationFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZOrganisationFindBox>("Type", organisationCodeFindBox);
				AssertEquals("BindTo", nameof(CusGoodsLocation.Address) + "." + nameof(CusGoodsLocationAddress.IdentificationHolderPK), organisationCodeFindBox.BindTo);
			});
		}

		public void TestEoriNumberTextBox()
		{
			var eoriNumberTextBox = control.EoriNumberTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", eoriNumberTextBox);
				AssertEquals("BindTo", nameof(CusGoodsLocation.Address) + "." + nameof(CusGoodsLocationAddress.E2_GovRegNum), eoriNumberTextBox.BindTo);
			});
		}

		public void TestAuthorizationCodeFindBox()
		{
			var authorizationCodeFindBox = control.AuthorizationCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", authorizationCodeFindBox);
				AssertEquals("BindTo", nameof(CusGoodsLocation.Address) + "." + nameof(CusGoodsLocationAddress.AuthorisationNumber), authorizationCodeFindBox.BindTo);
				AssertEquals("ShowDescriptionBox", false, authorizationCodeFindBox.ShowDescriptionBox);
			});
		}

		public void TetAuthorizationDropEdit()
		{
			var authorizationCodeDropEdit = control.AuthorizationDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", authorizationCodeDropEdit);
				AssertEquals("BindTo", nameof(CusGoodsLocation.Address) + "." + nameof(CusGoodsLocationAddress.AuthorisationNumber), authorizationCodeDropEdit.BindTo);
				AssertEquals("ShowDescriptionBox", false, authorizationCodeDropEdit.ShowDescriptionBox);
			});
		}

		public void TestStreetAndNumberTextBox()
		{
			var streetAndNumberTextBox = control.StreetAndNumberTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", streetAndNumberTextBox);
				AssertEquals("BindTo", nameof(CusGoodsLocation.Address) + "." + nameof(CusGoodsLocationAddress.E2_Address1AndE2_Address2), streetAndNumberTextBox.BindTo);
			});
		}

		public void TestStreetAndNumberWithAddressValidationControl()
		{
			var streetAndNumberWithAddressValidationControl = control.StreetAndNumberWithAddressValidationUserControl;
			AssertType<StreetAndNumberWithAddressValidationControl>("Type", streetAndNumberWithAddressValidationControl);
		}

		public void TestValidateAddressButton()
		{
			var validateAddressButton = control.StreetAndNumberWithAddressValidationUserControl.ValidateAddressButton;
			AssertType<ZButton>("Type", validateAddressButton);
		}

		public void TestCityTextBox()
		{
			var cityTextBox = control.CityTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", cityTextBox);
				AssertEquals("BindTo", nameof(CusGoodsLocation.Address) + "." + nameof(CusGoodsLocationAddress.E2_City), cityTextBox.BindTo);
			});
		}

		public void TestContactTextBox()
		{
			var contactTextBox = control.ContactTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", contactTextBox);
				AssertEquals("BindTo", nameof(CusGoodsLocation.Address) + "." + nameof(CusGoodsLocationAddress.E2_Contact), contactTextBox.BindTo);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, contactTextBox.CharacterCasing);
			});
		}

		public void TestPhoneTextBox()
		{
			var phoneTextBox = control.PhoneTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", phoneTextBox);
				AssertEquals("BindTo", nameof(CusGoodsLocation.Address) + "." + nameof(CusGoodsLocationAddress.E2_Phone), phoneTextBox.BindTo);
			});
		}

		public void TestEmailTextBox()
		{
			var emailTextBox = control.EmailTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", emailTextBox);
				AssertEquals("BindTo", nameof(CusGoodsLocation.Address) + "." + nameof(CusGoodsLocationAddress.E2_Email), emailTextBox.BindTo);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, emailTextBox.CharacterCasing);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new CusGoodsLocationUserControl();
		}
		CusGoodsLocationUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}

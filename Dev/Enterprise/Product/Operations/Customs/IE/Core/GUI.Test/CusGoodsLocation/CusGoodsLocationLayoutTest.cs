using System.Collections.Generic;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(CusGoodsLocationLayout))]
	sealed class CusGoodsLocationLayoutTest : LayoutsAbstractTest
	{
		public void TestPostcodeTextBoxVisible()
		{
			CusGoodsLocationLayoutAssertions.AssertControlVisibleForCertainQualifiers(layout, EU.GUI.CusGoodsLocationControlBag.Instance.PostcodeTextBox, cusGoodsLocation,
				Customs.Business.CusGoodsLocationQualifierList.Codes.UnLocode,
				Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress,
				Customs.Business.CusGoodsLocationQualifierList.Codes.Address);
		}

		public void TestStreetAndNumberWithAddressValidationUserControlVisible()
		{
			CusGoodsLocationLayoutAssertions.AssertControlVisibleForCertainQualifiers(layout, EU.GUI.CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, cusGoodsLocation,
				Customs.Business.CusGoodsLocationQualifierList.Codes.UnLocode,
				Customs.Business.CusGoodsLocationQualifierList.Codes.Address);
		}

		public void TestCityTextBoxVisible()
		{
			CusGoodsLocationLayoutAssertions.AssertControlVisibleForCertainQualifiers(layout, EU.GUI.CusGoodsLocationControlBag.Instance.CityTextBox, cusGoodsLocation,
				Customs.Business.CusGoodsLocationQualifierList.Codes.UnLocode,
				Customs.Business.CusGoodsLocationQualifierList.Codes.Address);
		}

		public void TestContactDetailTextBoxesVisible()
		{
			AssertEquals("ContactTextBox hidden when CusGoodsLocation attached to a V1 TemporaryStorage", false, layout.IsVisible(EU.GUI.CusGoodsLocationControlBag.Instance.ContactTextBox, cusGoodsLocation));
			AssertEquals("PhoneTextBox hidden when CusGoodsLocation attached to a V1 TemporaryStorage", false, layout.IsVisible(EU.GUI.CusGoodsLocationControlBag.Instance.PhoneTextBox, cusGoodsLocation));
			AssertEquals("EmailTextBox hidden when CusGoodsLocation attached to a V1 TemporaryStorage", false, layout.IsVisible(EU.GUI.CusGoodsLocationControlBag.Instance.EmailTextBox, cusGoodsLocation));

			temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V2;
			cusGoodsLocation = (CusGoodsLocation)temporaryStorageHeader.GoodsLocation;
			AssertEquals("ContactTextBox shown when CusGoodsLocation attached to a non-V1 TemporaryStorage", true, layout.IsVisible(EU.GUI.CusGoodsLocationControlBag.Instance.ContactTextBox, cusGoodsLocation));
			AssertEquals("PhoneTextBox shown when CusGoodsLocation attached to a non-V1 TemporaryStorage", true, layout.IsVisible(EU.GUI.CusGoodsLocationControlBag.Instance.PhoneTextBox, cusGoodsLocation));
			AssertEquals("EmailTextBox shown when CusGoodsLocation attached to a non-V1 TemporaryStorage", true, layout.IsVisible(EU.GUI.CusGoodsLocationControlBag.Instance.EmailTextBox, cusGoodsLocation));
		}

		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EU.GUI.CusGoodsLocationControlBag.Instance.QualifierDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.CusGoodsLocationControlBag.Instance.TypeDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.CusGoodsLocationControlBag.Instance.OrganisationFindBox, ControlWidthClass.Long);
				yield return (EU.GUI.CusGoodsLocationControlBag.Instance.EoriNumberTextBox, ControlWidthClass.Long);
				yield return (EU.GUI.CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox, ControlWidthClass.Long);
				yield return (EU.GUI.CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox, ControlWidthClass.Long);
				yield return (EU.GUI.CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, ControlWidthClass.Long);
				yield return (EU.GUI.CusGoodsLocationControlBag.Instance.CityTextBox, ControlWidthClass.Long);
				yield return (EU.GUI.CusGoodsLocationControlBag.Instance.PostcodeTextBox, ControlWidthClass.Long);
				yield return (EU.GUI.CusGoodsLocationControlBag.Instance.CountryCodeFindBox, ControlWidthClass.Long);
				yield return (EU.GUI.CusGoodsLocationControlBag.Instance.UnlocoCodeFindBox, ControlWidthClass.Long);
				yield return (EU.GUI.CusGoodsLocationControlBag.Instance.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
				yield return (EU.GUI.CusGoodsLocationControlBag.Instance.GeoLocationLatitudeTextBox, ControlWidthClass.Long);
				yield return (EU.GUI.CusGoodsLocationControlBag.Instance.GeoLocationLongitudeTextBox, ControlWidthClass.Long);
				yield return (EU.GUI.CusGoodsLocationControlBag.Instance.ContactTextBox, ControlWidthClass.Long);
				yield return (EU.GUI.CusGoodsLocationControlBag.Instance.PhoneTextBox, ControlWidthClass.Long);
				yield return (EU.GUI.CusGoodsLocationControlBag.Instance.EmailTextBox, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.GUI.CusGoodsLocationLayoutBuilder<CusGoodsLocation>();

		protected override void SetUp()
		{
			base.SetUp();
			temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V1;
			cusGoodsLocation = (CusGoodsLocation)temporaryStorageHeader.GoodsLocation;
			layout = ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;
		}

		TemporaryStorageHeader temporaryStorageHeader;
		CusGoodsLocation cusGoodsLocation;
		PanelLayout layout;
	}
}

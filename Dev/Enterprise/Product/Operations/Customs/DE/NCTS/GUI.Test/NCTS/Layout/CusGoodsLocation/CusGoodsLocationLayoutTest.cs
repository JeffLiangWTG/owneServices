using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.NCTS.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using CusGoodsLocation = Enterprise.Customs.DE.NCTS.Business.CusGoodsLocation;
using EUCusGoodsLocationControlBag = Enterprise.Customs.EU.GUI.CusGoodsLocationControlBag;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing
{
	[TestedType(typeof(CusGoodsLocationLayout))]
	sealed class CusGoodsLocationLayoutTest : LayoutsAbstractTest
	{
		public void TestOrganisationFindBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(
				CusGoodsLocationControlBag.Instance.OrganisationFindBox,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
		}

		public void TestOrganisationFindBoxVisibility_ParentIsNotArrivalMovementHeader()
		{
			AssertControlVisibleForCertainQualifiersAndParent(CusGoodsLocationControlBag.Instance.OrganisationFindBox, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
		}

		public void TestAuthorizationCodeFindBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(
				CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
		}

		public void TestAuthorizationCodeFindBoxVisibility_ParentIsNotArrivalMovementHeader()
		{
			AssertControlVisibleForCertainQualifiersAndParent(CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
		}

		public void TestAdditionalIdentifierDropEditVisibility()
		{
			AssertControlVisibleForCertainQualifiers(
				CusGoodsLocationControlBag.Instance.AdditionalIdentifierDropEdit,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
		}

		public void TestContactTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(
				EUCusGoodsLocationControlBag.Instance.ContactTextBox,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
		}

		public void TestPhoneTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(
				EUCusGoodsLocationControlBag.Instance.PhoneTextBox,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
		}

		public void TestEmailTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(
				EUCusGoodsLocationControlBag.Instance.EmailTextBox,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
		}

		public void TestUnlocoCodeFindBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(
				EUCusGoodsLocationControlBag.Instance.UnlocoCodeFindBox,
				CusGoodsLocationQualifierList.Codes.UnLocode);
		}

		public void TestGeoLocationLatitudeTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(
				EUCusGoodsLocationControlBag.Instance.GeoLocationLatitudeTextBox,
				CusGoodsLocationQualifierList.Codes.GnssCoordinates);
		}

		public void TestGeoLocationLongitudeTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(
				EUCusGoodsLocationControlBag.Instance.GeoLocationLongitudeTextBox,
				CusGoodsLocationQualifierList.Codes.GnssCoordinates);
		}

		public void TestAddress1TextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(
				EUCusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl,
				CusGoodsLocationQualifierList.Codes.Address);
		}

		public void TestCityTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(
				EUCusGoodsLocationControlBag.Instance.CityTextBox,
				CusGoodsLocationQualifierList.Codes.Address);
		}

		public void TestPostcodeTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(
				EUCusGoodsLocationControlBag.Instance.PostcodeTextBox,
				CusGoodsLocationQualifierList.Codes.Address);
		}

		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CusGoodsLocationLayoutBuilder<CusGoodsLocation>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EUCusGoodsLocationControlBag.Instance.QualifierDropEdit, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.OrganisationFindBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.AdditionalIdentifierDropEdit, ControlWidthClass.Long);
				yield return (EUCusGoodsLocationControlBag.Instance.ContactTextBox, ControlWidthClass.Long);
				yield return (EUCusGoodsLocationControlBag.Instance.PhoneTextBox, ControlWidthClass.Long);
				yield return (EUCusGoodsLocationControlBag.Instance.EmailTextBox, ControlWidthClass.Long);
				yield return (EUCusGoodsLocationControlBag.Instance.UnlocoCodeFindBox, ControlWidthClass.Long);
				yield return (EUCusGoodsLocationControlBag.Instance.GeoLocationLatitudeTextBox, ControlWidthClass.Long);
				yield return (EUCusGoodsLocationControlBag.Instance.GeoLocationLongitudeTextBox, ControlWidthClass.Long);
				yield return (EUCusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, ControlWidthClass.Long);
				yield return (EUCusGoodsLocationControlBag.Instance.CityTextBox, ControlWidthClass.Long);
				yield return (EUCusGoodsLocationControlBag.Instance.PostcodeTextBox, ControlWidthClass.Long);
			}
		}

		void AssertControlVisibleForCertainQualifiers(ControlReference control, params string[] visibleForQualifiers)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var location = nctsHeader.ArrivalMovementHeader.GoodsLocation;

			var layout = ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;

			CusGoodsLocationLayoutAssertions.AssertControlVisibleForCertainQualifiers(layout, control, location, visibleForQualifiers);
		}

		void AssertControlVisibleForCertainQualifiersAndParent(ControlReference control, ZString qualifier)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var location = nctsHeader.MovementHeader.GoodsLocation;

			var layout = ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;
			location.CGL_Qualifier = qualifier;
			Assert(!layout.IsVisible(control, location));
		}
	}
}

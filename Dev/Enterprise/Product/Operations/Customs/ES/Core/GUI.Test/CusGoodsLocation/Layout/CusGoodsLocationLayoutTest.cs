using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(CusGoodsLocationLayout))]
	public class CusGoodsLocationLayoutTest : LayoutsAbstractTest
	{
		public void TestDynamicAuthorizationControl()
		{
			var location = Factory.New<Business.Declaration.CusEntryInstruction>().GoodsLocation;
			var layout = ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;

			CombineAssertions(() =>
			{
				location.CGL_Qualifier = ZString.Empty;
				AssertEquals("CodeFindBox CGL_Qualifier empty in ES", false, layout.IsVisible(CusGoodsLocationControlBag.Instance.ESAuthorizationCodeFindBox, location));
				AssertEquals("CodeFindBox CGL_Qualifier empty in EU", false, layout.IsVisible(EU.GUI.CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox, location));

				location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
				AssertEquals("CodeFindBox CGL_Qualifier Y and CGL_Type empty in ES", false, layout.IsVisible(CusGoodsLocationControlBag.Instance.ESAuthorizationCodeFindBox, location));
				AssertEquals("CodeFindBox CGL_Qualifier Y and CGL_Type empty in EU", true, layout.IsVisible(EU.GUI.CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox, location));

				location.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
				AssertEquals("CodeFindBox CGL_Qualifier Y and CGL_Type B in ES", true, layout.IsVisible(CusGoodsLocationControlBag.Instance.ESAuthorizationCodeFindBox, location));
				AssertEquals("CodeFindBox CGL_Qualifier Y and CGL_Type B in EU", false, layout.IsVisible(EU.GUI.CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox, location));
			});
		}

		public void TestAdditionalIdentifierTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox,
				CusGoodsLocationQualifierList.Codes.PostcodeAddress,
				CusGoodsLocationQualifierList.Codes.EoriNumber,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
		}

		public void TestPostcodeTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.PostcodeTextBox,
				CusGoodsLocationQualifierList.Codes.PostcodeAddress,
				CusGoodsLocationQualifierList.Codes.Address);
		}

		public void TestCountryCodeFindBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.CountryCodeFindBox,
				CusGoodsLocationQualifierList.Codes.PostcodeAddress,
				CusGoodsLocationQualifierList.Codes.Address);
		}

		public void TestUnlocoCodeFindBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.UnlocoCodeFindBox, CusGoodsLocationQualifierList.Codes.UnLocode);
		}

		public void TestCustomsOfficeCodeFindBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.CustomsOfficeCodeFindBox, CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier);
		}

		public void TestGeoLocationLatitudeTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.GeoLocationLatitudeTextBox, CusGoodsLocationQualifierList.Codes.GnssCoordinates);
		}

		public void TestGeoLocationLongitudeTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.GeoLocationLongitudeTextBox, CusGoodsLocationQualifierList.Codes.GnssCoordinates);
		}

		public void TesOrganisationFindBoxisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.OrganisationFindBox,
				CusGoodsLocationQualifierList.Codes.EoriNumber,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
		}

		public void TestEoriNumberTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.EoriNumberTextBox, CusGoodsLocationQualifierList.Codes.EoriNumber);
		}

		public void TestAuthorizationCodeFindBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
		}

		public void TestStreetAndNumberTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, CusGoodsLocationQualifierList.Codes.Address);
		}

		public void TestCityTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.CityTextBox, CusGoodsLocationQualifierList.Codes.Address);
		}

		public void TestContactTextBoxVisibility()
		{
			AssertControlVisibleContactPhoneEmail(EU.GUI.CusGoodsLocationControlBag.Instance.ContactTextBox);
		}

		public void TestPhoneTextBoxVisibility()
		{
			AssertControlVisibleContactPhoneEmail(EU.GUI.CusGoodsLocationControlBag.Instance.PhoneTextBox);
		}

		public void TestEmailTextBoxVisibility()
		{
			AssertControlVisibleContactPhoneEmail(EU.GUI.CusGoodsLocationControlBag.Instance.EmailTextBox);
		}

		public void TestAdditionalIdentifierTextBoxCaption()
		{
			var location = Factory.New<Business.Declaration.CusEntryInstruction>().GoodsLocation;
			var layout = ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;

			CombineAssertions(() =>
			{
				location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.PostcodeAddress;
				layout.TryGetCaption(EU.GUI.CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox, location, out var resourceStringData);
				AssertEquals("CGL_Qualifier = 'T'", "House Number", resourceStringData.Caption);

				location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
				layout.TryGetCaption(EU.GUI.CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox, location, out resourceStringData);
				AssertEquals("CGL_Qualifier = 'X'", "Additional Identifier", resourceStringData.Caption);

				location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
				layout.TryGetCaption(EU.GUI.CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox, location, out resourceStringData);
				AssertEquals("CGL_Qualifier = 'Y'", "Additional Identifier", resourceStringData.Caption);
			});
		}

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
				yield return (CusGoodsLocationControlBag.Instance.ESAuthorizationCodeFindBox, ControlWidthClass.Long);
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

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.GUI.CusGoodsLocationLayoutBuilder<Business.CusGoodsLocation>();

		void AssertControlVisibleForCertainQualifiers(ControlReference control, params string[] visibleForQualifiers)
		{
			var location = Factory.New<Business.Declaration.CusEntryInstruction>().GoodsLocation;
			var layout = ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;

			CombineAssertions(() =>
			{
				foreach (var qualifier in new CusGoodsLocationQualifierList().GetAllCodes())
				{
					location.CGL_Qualifier = qualifier;
					AssertEquals($"CGL_Qualifier = '{qualifier}'", qualifier.In(visibleForQualifiers), layout.IsVisible(control, location));
				}
			});
		}

		void AssertControlVisibleContactPhoneEmail(ControlReference control)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var location = instruction.GoodsLocation;
			var layout = ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;

			CombineAssertions(() =>
			{
				foreach (var qualifier in new CusGoodsLocationQualifierList().GetAllCodes())
				{
					location.CGL_Qualifier = qualifier;
					AssertEquals($"CGL_Qualifier = '{qualifier}'", true, layout.IsVisible(control, location));
				}

				location.CGL_Qualifier = ZString.Empty;
				AssertEquals("CGL_Qualifier empty", false, layout.IsVisible(control, location));

				location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				instruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
				location.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
				AssertEquals("One of the cases it should be false, check TestNamePhoneAndEmailVisible", false, layout.IsVisible(control, location));
			});
		}
	}
}

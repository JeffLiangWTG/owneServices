using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(ICS2JobDocAddress))]
	sealed class ICS2JobDocAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCaptionResourceString()
		{
			CombineAssertions("fields caption", () =>
			{
				var companyNameResAttribute = facilityPlace.CompanyNameInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertEquals("Facility Place Name", companyNameResAttribute.Caption);
				AssertEquals("Name", companyNameResAttribute.ShortCaption);

				var cityResAttribute = facilityPlace.E2_CityInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertEquals("Facility Place City", cityResAttribute.Caption);
				AssertEquals("City", cityResAttribute.ShortCaption);

				var countryResAttribute = facilityPlace.E2_RN_NKCountryCodeInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertEquals("Facility Place Country", countryResAttribute.Caption);
				AssertEquals("Country", countryResAttribute.ShortCaption);

				var subDivisionResAttribute = facilityPlace.SubDivisionInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertEquals("Facility Place Sub-Division", subDivisionResAttribute.Caption);
				AssertEquals("Sub-Division", subDivisionResAttribute.ShortCaption);

				var streetResAttribute = facilityPlace.E2_Address1Info.GetAttribute<ResourceStringDataAttribute>();
				AssertEquals("Facility Place Street", streetResAttribute.Caption);
				AssertEquals("Street", streetResAttribute.ShortCaption);

				var postcodeResAttribute = facilityPlace.E2_PostcodeInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertEquals("Facility Place Postcode", postcodeResAttribute.Caption);
				AssertEquals("Postcode", postcodeResAttribute.ShortCaption);

				var streetAdditionalLineResAttribute = facilityPlace.Address2Info.GetAttribute<ResourceStringDataAttribute>();
				AssertEquals("Facility Place Street additional line", streetAdditionalLineResAttribute.Caption);
				AssertEquals("Street additional line", streetAdditionalLineResAttribute.ShortCaption);

				var numberResAttribute = facilityPlace.NumberInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertEquals("Facility Place Number", numberResAttribute.Caption);
				AssertEquals("Number", numberResAttribute.ShortCaption);

				var poBoxResAttribute = facilityPlace.POBoxInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertEquals("Facility Place P.O. Box", poBoxResAttribute.Caption);
				AssertEquals("P.O. Box", poBoxResAttribute.ShortCaption);

				var stateResAttribute = facilityPlace.E2_StateInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertEquals("Facility Place State", stateResAttribute.Caption);
				AssertEquals("State", stateResAttribute.ShortCaption);
			});
		}

		public void TestMaxLength()
		{
			CombineAssertions("Max length", () =>
			{
				AssertEquals(35, facilityPlace.E2_CityInfo.MaxLength);
				AssertEquals(35, facilityPlace.SubDivisionInfo.MaxLength);
				AssertEquals(35, facilityPlace.NumberInfo.MaxLength);
				AssertEquals(70, facilityPlace.POBoxInfo.MaxLength);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			facilityPlace = Factory.New<ICS2JobDocAddress>();
		}
		ICS2JobDocAddress facilityPlace;
	}
}

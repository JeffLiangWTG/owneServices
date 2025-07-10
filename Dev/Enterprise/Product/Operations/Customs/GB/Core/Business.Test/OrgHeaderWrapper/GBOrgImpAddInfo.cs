using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(GBOrgImpAddInfo))]
	public class GBOrgImpAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var countryData = Factory.New<OrgCountryData>();
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedKingdom;
			return new GBOrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
		}

		public void TestProperties()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var countryData = org.CountryData;
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedKingdom;
			var gbAddInfo = GBOrgImpAddInfo.Get(org);
			var euAddInfo = EU.Business.EUOrgImpAddInfo.Get(org, Core.Constants.CountryCodes.UnitedKingdom);

			AssertEquals(org, gbAddInfo.Organisation);
			AssertEquals(false, euAddInfo.ZO_Box14UseIndirectRepresentation);
			AssertEquals(string.Empty, euAddInfo.ZO_OtherDeferType);
			AssertEquals(string.Empty, gbAddInfo.ZO_VATDeferType);
			AssertEquals(false, gbAddInfo.ZO_Box44UseClientsEoriForDucrs);
			AssertEquals(string.Empty, gbAddInfo.ZO_Box44ClientsDucrSourceAttributeField);
			AssertEquals(string.Empty, gbAddInfo.ZO_Box7DeclarantsReferenceSourceAttributeField);

			euAddInfo.ZO_Box14UseIndirectRepresentation = true;
			euAddInfo.ZO_OtherDeferType = "X";
			gbAddInfo.ZO_VATDeferType = "Y";
			gbAddInfo.ZO_Box44ClientsDucrSourceAttributeField = "NON";
			gbAddInfo.ZO_Box44UseClientsEoriForDucrs = true;
			gbAddInfo.ZO_Box7DeclarantsReferenceSourceAttributeField = "NON";

			AssertEquals(true, euAddInfo.ZO_Box14UseIndirectRepresentation);
			AssertEquals("X", euAddInfo.ZO_OtherDeferType);
			AssertEquals("Y", gbAddInfo.ZO_VATDeferType);
			AssertEquals(true, gbAddInfo.ZO_Box44UseClientsEoriForDucrs);
			AssertEquals("NON", gbAddInfo.ZO_Box44ClientsDucrSourceAttributeField);
			AssertEquals("NON", gbAddInfo.ZO_Box7DeclarantsReferenceSourceAttributeField);

			Factory.Save();

			var newOrg = Factory.Load<OrgHeader>(org.PK);
			var newGBAddInfo = GBOrgImpAddInfo.Get(newOrg);
			var newEUAddInfo = EU.Business.EUOrgImpAddInfo.Get(org, Core.Constants.CountryCodes.UnitedKingdom);

			AssertEquals(true, newEUAddInfo.ZO_Box14UseIndirectRepresentation);
			AssertEquals("X", newEUAddInfo.ZO_OtherDeferType);
			AssertEquals("Y", newGBAddInfo.ZO_VATDeferType);
			AssertEquals(true, newGBAddInfo.ZO_Box44UseClientsEoriForDucrs);
			AssertEquals("NON", newGBAddInfo.ZO_Box44ClientsDucrSourceAttributeField);
			AssertEquals("NON", newGBAddInfo.ZO_Box7DeclarantsReferenceSourceAttributeField);
		}

		public void TestZO_Box44ClientsDucrSourceAttributeFieldLookups()
		{
			var countryData = Factory.New<OrgCountryData>();
			var addInfo = new GBOrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
			AssertEquals(1, addInfo.Lookups.ClientsDucrSourceAttributeFieldList.Count);
			AssertEquals("NON", addInfo.Lookups.ClientsDucrSourceAttributeFieldList[0].Code);
			AssertContains("set up values in Registry", addInfo.Lookups.ClientsDucrSourceAttributeFieldList[0].Description);

			FreightDataRegistry.Instance.ShipmentCustomText1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Daniel", ""));
			FreightDataRegistry.Instance.ShipmentCustomText2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Clarke", ""));
			countryData = Factory.New<OrgCountryData>();
			addInfo = new GBOrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
			AssertEquals(3, addInfo.Lookups.ClientsDucrSourceAttributeFieldList.Count);
			AssertEquals("NON", addInfo.Lookups.ClientsDucrSourceAttributeFieldList[0].Code);
			AssertNotContains("set up values in Registry", addInfo.Lookups.ClientsDucrSourceAttributeFieldList[0].Description);
			AssertContains("Daniel", addInfo.Lookups.ClientsDucrSourceAttributeFieldList[1].Description);
			AssertContains("Clarke", addInfo.Lookups.ClientsDucrSourceAttributeFieldList[2].Description);
			AssertEquals("CA1", addInfo.Lookups.ClientsDucrSourceAttributeFieldList[1].Code);
			AssertEquals("CA2", addInfo.Lookups.ClientsDucrSourceAttributeFieldList[2].Code);
		}
	}
}

using System;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(CusGoodsLocation))]
	public class CusGoodsLocationTest : EU.H7.Business.Testing.CusGoodsLocationTest
	{
		protected override Type LookupType => typeof(CusGoodsLocationLookups);

		public void TestDisplayText()
		{
			CombineAssertions(() =>
			{
				AssertEquals("All fields empty", string.Empty, location.DisplayText);

				location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				location.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
				location.CGL_AdditionalIdentifier = "WiseTech Global Sydney";
				location.CGL_CustomsOffice = "FR230023";
				var address = location.Address;
				address.E2_Latitude = -33.9164669;
				address.E2_Longitude = 151.1944765;
				address.E2_GovRegNum = "EOR0001";
				address.E2_Address1AndE2_Address2 = "WiseTech Global Sydney Headquarter, 74 O'Riordan Street";
				address.E2_Postcode = "2015";
				address.E2_City = "Alexandria";
				address.E2_RN_NKCountryCode = "AU";
				address.E2_Contact = "John Smith";
				address.E2_Phone = "02 8001 2200";
				address.E2_Email = "john.smith@wisetechglobal.com";
				AssertEquals("All fields filled", "Z;C;-33.9164669,151.1944765;EOR0001;WiseTech Global Sydney Headquarter, 74 O'Riordan Street;2015;FR230023;WiseTech Global Sydney;Alexandria;AU;Contact John Smith;Ph 02 8001 2200;john.smith@wisetechglobal.com", location.DisplayText);
			});
		}

		public void TestUnlocode()
		{
			location.Unlocode = "UNLocode12";
			AssertEquals("UNLocode12", location.CGL_CustomsOffice);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			location = bill.CusGoodsLocation;
			location.CGL_LocationUse = "DEP";
		}

		CusGoodsLocation location;
	}
}

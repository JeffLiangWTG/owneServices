using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(OrgHeaderCollectionWithSpecificRegistrationCodes))]
	sealed class OrgHeaderCollectionWithSpecificRegistrationCodesTest : BusinessObjectCollectionTestCase
	{
		public void TestOrgString()
		{
			AssertEquals("Organisation does not contain a registration number / code for: ABC123", OrgHeaderCollectionWithSpecificRegistrationCodes.OrgDoesNotHaveRegNo("ABC123"));
		}

		public void TestAdditionalFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			CreateRegCode(org1, "AAA", "ZZZ");
			CreateRegCode(org2, "AAA", "BBB");
			CreateRegCode(org3, "BBB", "CCC");

			Factory.Save();

			var a = new OrgHeaderCollectionWithSpecificRegistrationCodes(Factory, "AAA");
			var ac = new OrgHeaderCollectionWithSpecificRegistrationCodes(Factory, "AAA", "CCC");
			var b = new OrgHeaderCollectionWithSpecificRegistrationCodes(Factory, "BBB");
			var z = new OrgHeaderCollectionWithSpecificRegistrationCodes(Factory, "ZZZ");
			var d = new OrgHeaderCollectionWithSpecificRegistrationCodes(Factory, "DDD");

			a.Load();
			ac.Load();
			b.Load();
			z.Load();
			d.Load();

			AssertContainsExactElementsInAnyOrder(new[] { org1, org2 }, a);
			AssertContainsExactElementsInAnyOrder(new[] { org1, org2, org3 }, ac);
			AssertContainsExactElementsInAnyOrder(new[] { org2, org3 }, b);
			AssertContainsExactElementsInAnyOrder(new[] { org1 }, z);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<object>(), d);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new OrgHeaderCollectionWithSpecificRegistrationCodes(Factory);

		void CreateRegCode(OrgHeader org, params string[] codes)
		{
			foreach (var code in codes)
			{
				var cusCode = org.CustomsCodes.AddNew();
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
				cusCode.OK_CodeType = code;
				cusCode.OK_CustomsRegNo = "1234";
			}
		}
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	class KRAddressFormatterTest : TestCaseWithFactory
	{
		public void TestNoExceptionWhenAddressIsNull()
		{
			var addressFormatter = new KRAddressFormatter(Factory, null);
			AssertNoExceptionThrown("Null check should prevent formatter to crash when address parameter is null.", () => addressFormatter.PostalAddress());
		}

		public void TestKRCompaniesAddressFormat()
		{
			var supplierKOR = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "SUPPLIERKOR", "(주)레디코리아");
			supplierKOR.OH_RL_NKClosestPort = Core.Constants.CountryCodes.KoreaSouth;
			TestOrgDataSetUpHelper.AddOrgContact(supplierKOR, "CONTACT NM9", true);
			TestOrgDataSetUpHelper.AddOrgAddress(supplierKOR.MainAddress, "서울특별시 서초구 동광로 41 (방배동)", "411", "06561", "101010", "020120");
			var supplierKORCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "관세상사1234561" },
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1208174197" },
				new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = "USACEANT0001T" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplierKOR, supplierKORCodes);

			var addressFormatter = new KRAddressFormatter(Factory, supplierKOR.MainAddress);
			var formattedAddress = addressFormatter.PostalAddress();
			Assert(formattedAddress.Contains("CONTACT NM9\n(GBR)1208174197,(06)관세상사1234561\n서울특별시 서초구 동광로 41 (방배동)\n411\n06561"));
			Assert(!formattedAddress.Contains("(07)USACEANT0001T"));
		}

		public void TestNonKRCompaniesAddressFormat()
		{
			var supplierOTH = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "SUPPLIEROTH", "ACE ANTENNA COMPANY");
			TestOrgDataSetUpHelper.AddOrgContact(supplierOTH, "Bill", true);
			TestOrgDataSetUpHelper.AddOrgAddress(supplierOTH.MainAddress, "16181 SCIENTIFIC WAY IRVNE CA 92618", "3-1", "16181", "", "");
			supplierOTH.MainAddress.OA_City = "IRVNE";
			supplierOTH.MainAddress.OA_State = "California";
			supplierOTH.OH_RL_NKClosestPort = Core.Constants.CountryCodes.UnitedStates;
			var supplierOTHCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "관세상사1234561" },
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1208174197" },
				new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = "USACEANT0001T" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplierOTH, supplierOTHCodes);

			var addressFormatter = new KRAddressFormatter(Factory, supplierOTH.MainAddress);
			var formattedAddress = addressFormatter.PostalAddress();
			Assert(formattedAddress.Contains("(07)USACEANT0001T\n"));
			Assert(!formattedAddress.Contains("(GBR)관세상사1234561"));
			Assert(!formattedAddress.Contains("(06)1208174197"));
		}
	}
}

using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.ICS.Business.Testing
{
	public class AsycudaManifestHeaderSSLookupTests : BusinessObjectLookupsTestCase
	{
		public void TestRegistrationStatusList()
		{
			var header = Factory.New<AsycudaManifestHeaderSS>();
			AssertEquals("ACK, DIV, DNL, NDV", header.Lookups.RegistrationStatusList.CodesAsString);
		}

		public void TestProfileList()
		{
			var pwd = Factory.New<GlbExternalPassword_GB>();
			pwd.GP_GC = GlbCompany.CurrentCompany.PK;
			pwd.Badge = "WTG";
			pwd.EORI = GlbBranch.CurrentBranch.OrgProxy.GetEuIdentificationNumber();
			pwd.Status = PasswordStatusList.Codes.Valid;
			pwd.GP_ExpiryDate = ZDateTime.Today.AddDays(2);
			pwd.GP_IssueDate = ZDateTime.Today.AddDays(-2);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("WTG", header.Lookups.ProfileList.CodesAsString);
		}

		public void TestTransportModeList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var expectedTransportModeList = "AIR - Air Freight\r\nIWT - Inland Water Transport\r\nRAI - Rail Freight\r\nROA - Road Freight\r\nROR - RoRo - Accompanied\r\nROU - RoRo - Unaccompanied\r\nSEA - Sea Freight";
				var declaration = Factory.New<AsycudaManifestHeaderSS>();
				declaration.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
				declaration.AMA_ManifestType = "S&S";
				var actualTransportModeList = declaration.Lookups.TransportModeList;
				AssertEquals(expectedTransportModeList, actualTransportModeList.GetHumanReadableListOfElements());
			}
		}

		public void TestSpecificCircumstanceList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals("C, D, E", header.Lookups.SpecificCircumstanceList.CodesAsString);
		}
	}
}

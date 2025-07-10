using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.ICS.Business.Testing
{
	public class AsycudaManifestHeaderLookupTests : BusinessObjectLookupsTestCase
	{
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

		public void TestSpecificCircumstanceIndicatorLookup()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
				AssertEquals(3, header.Lookups.SpecificCircumstanceList.Count);
				AssertEquals("C - Road mode of transport", header.Lookups.SpecificCircumstanceList.GetWithDescription(ASYCUDA.Business.SpecificCircumstanceList.Codes.C));
				AssertEquals("D - Rail mode of transport", header.Lookups.SpecificCircumstanceList.GetWithDescription(ASYCUDA.Business.SpecificCircumstanceList.Codes.D));
				AssertEquals("E - Authorized Economic Operators", header.Lookups.SpecificCircumstanceList.GetWithDescription(ASYCUDA.Business.SpecificCircumstanceList.Codes.E));
			}
		}
	}
}

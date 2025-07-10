using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Manifest.Business;

namespace Enterprise.Customs.GB.ICS.Business.Testing
{
	sealed class AsycudaManifestHeaderCustomsOfficeRequirementHelperTest : TestCaseWithFactory
	{
		public void TestGetOtherRequirements()
		{
			var manifest = Factory.New<AsycudaManifestHeaderSS>();
			var helper = new AsycudaManifestHeaderCustomsOfficeRequirementHelperForTest(manifest);

			CombineAssertions(() =>
			{
				var roles = helper.GetOtherRequirementsExposed;
				AssertEquals(3, roles.Length);
				AssertRole(roles[0], OfficeCodes_ICS.Codes.OfficeOfActualEntryDiversion, false);
				AssertRole(roles[1], OfficeCodes_ICS.Codes.OfficeOfFirstEntry, false);
				AssertRole(roles[2], OfficeCodes_ICS.Codes.OfficeOfSubsequentEntry, false);
			});
		}

		void AssertRole(CustomsOfficeRequirement customsOffice, string expectedRole, bool isMandatory = true, bool isLocalCountryOnly = true)
		{
			AssertEquals("Role", customsOffice.OfficeRole, expectedRole);
			AssertEquals("Role is mandatory", customsOffice.IsMandatory, isMandatory);
			AssertEquals("Role is local country", customsOffice.IsLocalCountryOnly, isLocalCountryOnly);
		}

		class AsycudaManifestHeaderCustomsOfficeRequirementHelperForTest : AsycudaManifestHeaderCustomsOfficeRequirementHelper
		{
			public AsycudaManifestHeaderCustomsOfficeRequirementHelperForTest(IEuOfficeCodeProvider officeCodeProvider) : base(officeCodeProvider)
			{
			}

			public CustomsOfficeRequirement[] GetOtherRequirementsExposed => GetOtherRequirements().ToArray();
		}
	}
}

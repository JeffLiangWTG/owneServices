using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	static class TestHelper
	{
		internal static void ModifyOrgCusCode(this OrgCusCode orgCusCode, ZGuid orgHeaderPK, ZString cusCodeType, ZString countryCode, ZString cusCode, ZGuid premisesAddressPK = default)
		{
			orgCusCode.OK_OH = orgHeaderPK;
			orgCusCode.OK_RN_NKCodeCountry = countryCode;
			orgCusCode.OK_CodeType = cusCodeType;
			orgCusCode.OK_CustomsRegNo = cusCode;
			if (premisesAddressPK != ZGuid.Empty)
			{
				orgCusCode.OK_OA_PremisesAddress = premisesAddressPK;
			}
		}

		internal static CodeDescriptionPair CreateValidCredential() => new CodeDescriptionPair("AR1", "Credential Identifier");

		internal static GlbStaff SetupStaffData(BusinessObjectFactory factory) => CreateStaff(factory, "!1@", "Staff 1", "staff1@where.com");

		static GlbStaff CreateStaff(BusinessObjectFactory factory, ZString code, ZString name, ZString email)
		{
			var glbStaff = factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_Code = code;
			glbStaff.GS_FullName = name;
			glbStaff.GS_EmailAddress = email;
			return glbStaff;
		}

		internal static void CreateEMCSCredential(BusinessObjectFactory factory, ZGuid companyPK, ZString badge, ZString eori, ZString passwordType)
		{
			TestDataHelper.CreateCredentials(factory, companyPK, badge, eori, passwordType);
		}

		internal static OrgHeader GetPartyTraderExciseNumberOrg(BusinessObjectFactory factory, ZString exciseNumber, string countryCode, bool isWarehouse = false)
		{
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			if (isWarehouse)
			{
				_ = orgHeader.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, exciseNumber, countryCode);
			}
			else
			{
				_ = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, exciseNumber, countryCode);
			}

			orgHeader.MainAddress.OA_RN_NKCountryCode = countryCode;
			return orgHeader;
		}
	}
}

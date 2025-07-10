using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[CodeAlive("Soon to be used")]
	public static class TestOrgDataSetUpHelper
	{
		public static OrgHeader CreateOrgHeader(BusinessObjectFactory factory, ZString category, ZString code, ZString fullName)
		{
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Category = category;
			orgHeader.OH_Code = code;
			orgHeader.OH_FullName = fullName;

			return orgHeader;
		}
		public static OrgContact AddOrgContact(OrgHeader orgHeader, ZString contactName, bool setAsCEO)
		{
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_OH = orgHeader.PK;
			contact.OC_ContactName = contactName;
			if (setAsCEO)
			{
				var declarantAllocation = contact.Allocations.AddNew();
				declarantAllocation.PC_Type = OrgConstants.ContactAllocationType.CEOForKRCustoms;
			}

			return contact;
		}
		public static void AddCustomsCode(OrgHeader orgHeader, IDNumberAndType[] iDNumAndTypes)
		{
			foreach (IDNumberAndType id in iDNumAndTypes)
			{
				var cusCode = orgHeader.CustomsCodes.AddNew(id.Type, id.Number, id.CountryOfIssue);
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;
			}
		}

		public static void AddCustomsCode(OrgAddress orgAddress, IDNumberAndType[] iDNumAndTypes)
		{
			foreach (IDNumberAndType id in iDNumAndTypes)
			{
				orgAddress.CustomsCodes.AddNew(id.Type, id.Number, id.CountryOfIssue);
			}
		}
		public static void AddOrgAddress(OrgAddress orgAddress, string address1, string address2 = null, string postCode = null, string roadNameCode = null, string buildingNumber = null, string countryCode = null)
		{
			orgAddress.OA_Address1 = address1;
			orgAddress.OA_Address2 = address2;
			orgAddress.OA_PostCode = postCode;
			orgAddress.OA_RN_NKCountryCode = countryCode;
			if (roadNameCode != null)
			{
				orgAddress.CustomsCodes.AddNew(IdentificationType.RoadNameCode, roadNameCode, Core.Constants.CountryCodes.KoreaSouth);
			}
			if (buildingNumber != null)
			{
				orgAddress.CustomsCodes.AddNew(IdentificationType.BuildingNumber, buildingNumber, Core.Constants.CountryCodes.KoreaSouth);
			}
		}
	}
}

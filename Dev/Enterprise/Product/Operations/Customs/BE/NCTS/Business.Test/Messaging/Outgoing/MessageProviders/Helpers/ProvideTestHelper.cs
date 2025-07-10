using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	static class ProvideTestHelper
	{
		public static T CreateCusReference<T>(this BusinessObjectFactory factory, string code, string type, string reference = null, BusinessObject parent = null) where T : CusReference
		{
			var cusReference = factory.New<T>();
			if (parent != null)
			{
				cusReference.CFR_ParentID = parent.PK;
				cusReference.CFR_ParentTableCode = parent.TablePrefix;
			}
			cusReference.CFR_Type = type;
			cusReference.CFR_Reference = reference;
			cusReference.CFR_Code = code;
			return cusReference;
		}

		public static T CreateCusCodeData<T>(this BusinessObjectFactory factory, string type, short order = 1, string code = null, string data = null, BusinessObject parent = null) where T : CusCodeData
		{
			var cusCodeData = factory.New<T>();
			cusCodeData.Parent = parent;
			cusCodeData.CY_Type = type;
			cusCodeData.CY_Order = order;
			cusCodeData.CY_Code = code;
			cusCodeData.CY_Data = data;
			cusCodeData.CY_Date = System.DateTime.FromOADate(1234);
			return cusCodeData;
		}

		public static CusSupportingInfo CreateCusSupportingInfo(this BusinessObjectFactory factory, string type, string subtype, string reference = null, string reference2 = null, string code = null, BusinessObject parent = null)
		{
			var cusSupportingInfo = factory.New<CusSupportingInfo>();
			if (parent != null)
			{
				cusSupportingInfo.CSI_ParentID = parent.PK;
				cusSupportingInfo.CSI_ParentTableCode = parent.TablePrefix;
			}
			cusSupportingInfo.CSI_Type = type;
			cusSupportingInfo.CSI_SubType = subtype;
			cusSupportingInfo.CSI_ReferenceNumber = reference;
			cusSupportingInfo.CSI_ReferenceNumber2 = reference2;
			cusSupportingInfo.CSI_Code = code;
			return cusSupportingInfo;
		}

		public static JobDocAddress CreateJobDocAddress(this BusinessObjectFactory factory, string addressType = null, string orgHeaderFullName = null, string contactName = null, string contactPhone = null, string contactEmail = null
			, JobDocAddress jobDocAddress = null
			, OrgHeader orgHeader = null
			, OrgAddress orgAddress = null
			, IEnumerable<OrgCusCode> orgCusCodes = null, BusinessObject parent = null)
		{
			jobDocAddress = jobDocAddress ?? factory.New<JobDocAddress>();
			orgHeader = orgHeader ?? factory.New<OrgHeader>();
			var orgContact = factory.New<OrgContact>();

			orgHeader.OH_FullName = orgHeaderFullName;
			jobDocAddress.E2_AddressType = addressType;

			jobDocAddress.E2_Contact = contactName;
			jobDocAddress.E2_Phone = contactPhone;
			jobDocAddress.E2_Email = contactEmail;

			orgContact.OC_ContactName = contactName;
			orgContact.OC_Phone = contactPhone;
			orgContact.OC_Email = contactEmail;

			if (orgAddress != null)
			{
				jobDocAddress.E2_OA_Address = orgAddress.PK;
				orgAddress.OA_OH = orgHeader.PK;
			}

			jobDocAddress.OrganisationPK = orgHeader.PK;
			if (parent != null)
			{
				jobDocAddress.E2_ParentID = parent.PK;
				jobDocAddress.E2_ParentTableCode = parent.TablePrefix;
			}
			jobDocAddress.ContactPK = orgContact.PK;

			orgHeader.CustomsCodes.AddRange(orgCusCodes ?? Enumerable.Empty<OrgCusCode>());
			return jobDocAddress;
		}

		public static OrgAddress CreateOrgAddress(this BusinessObjectFactory factory, string address1 = null, string address2 = null, string postCode = null, string city = null, string countryCode = null)
		{
			var orgAddress = factory.New<OrgAddress>();

			orgAddress.OA_Address1 = address1;
			orgAddress.OA_Address2 = address2;
			orgAddress.Postcode = postCode;
			orgAddress.City = city;
			orgAddress.OA_RN_NKCountryCode = countryCode;

			return orgAddress;
		}

		public static OrgCusCode CreateOrgCusCode(this BusinessObjectFactory factory, string codeType, string customsRegNo, string country)
		{
			var orgCusCode = factory.New<OrgCusCode>();
			orgCusCode.OK_CodeType = codeType;
			orgCusCode.OK_CustomsRegNo = customsRegNo;
			orgCusCode.OK_RN_NKCodeCountry = country;
			return orgCusCode;
		}

		public static CusPermitHeader CreateCusPermitHeader(this BusinessObjectFactory factory, string type, OrgAddress orgAddress)
		{
			var cusPermitHeader = factory.New<CusPermitHeader>();
			cusPermitHeader.CPH_Type = type;
			cusPermitHeader.CPH_Number = "CPH";
			cusPermitHeader.CPH_OH_PermitHolder = orgAddress.OA_OH;
			return cusPermitHeader;
		}
	}
}

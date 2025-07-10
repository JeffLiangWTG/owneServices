using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class OrgWrapper : IDLMOrganisation
	{
		public static OrgWrapper New(OrgHeader organisation)
		{
			return (organisation == null) ? null : new OrgWrapper(organisation);
		}

		protected OrgWrapper(OrgHeader organisation)
		{
			if (organisation == null)
			{
				throw new ArgumentNullException(nameof(organisation));
			}
			this.organisation = organisation;
		}

		#region Implementation
		readonly OrgHeader organisation;
		ZString GetCountryName(RefCountry country)
		{
			ZString result = ZString.Empty;
			if (country != null)
			{
				result = country.RN_DescMultilingual;
			}
			return result;
		}

		ZString GetState(OrgAddress address)
		{
			ZString result = ZString.Empty;
			if (address != null)
			{
				result = address.OA_State_List.GetDescriptionFromCode(address.OA_State) ?? address.OA_State;
			}
			return result;
		}

		ZString GetNumber(ZString number)
		{
			return number.KeepNumericCharacters().Right(10);
		}
		#endregion Implementation

		#region IDLMOrganisation Members

		ZString IDLMOrganisation.AuthorizationId
		{
			get { return organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.CACodeTypes.AuthorizationID); }
		}

		ZString IDLMOrganisation.BusinessNumberForImportExport
		{
			get
			{
				return organisation.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.Canada, OrgCusCode.CACodeTypes.BusinessNumberForExport, OrgCusCode.CACodeTypes.BusinessNumberForImportExport);
			}
		}

		ZString IDLMOrganisation.CompanyName
		{
			get { return organisation.OH_FullNameTruncated; }
		}

		ZString IDLMOrganisation.Street
		{
			get { return organisation.MainAddress.OA_Address1 + " " + organisation.MainAddress.OA_Address2; }
		}

		ZString IDLMOrganisation.City
		{
			get { return organisation.MainAddress.OA_City; }
		}

		ZString IDLMOrganisation.ProvinceState
		{
			get { return GetState(organisation.MainAddress); }
		}

		ZString IDLMOrganisation.Country
		{
			get { return GetCountryName(organisation.Country); }
		}

		ZString IDLMOrganisation.PostalZipCode
		{
			get { return organisation.MainAddress.OA_PostCode; }
		}

		ZString IDLMOrganisation.Telephone
		{
			get { return GetNumber(organisation.MainAddress.OA_Phone); }
		}

		ZString IDLMOrganisation.TelephoneExtension
		{
			get { return ZString.Empty; }
		}

		ZString IDLMOrganisation.Fax
		{
			get { return GetNumber(organisation.MainAddress.OA_Fax); }
		}

		#endregion
	}
}

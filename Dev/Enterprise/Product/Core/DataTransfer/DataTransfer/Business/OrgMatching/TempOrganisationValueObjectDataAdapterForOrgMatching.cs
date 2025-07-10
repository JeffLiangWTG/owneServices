using System.Collections;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using OrganisationTypes = Enterprise.MasterFiles.Integration.OrganisationTypes;

namespace Enterprise.DataTransfer.Business
{
	class TempOrganisationValueObjectDataAdapterForOrgMatching
	{
		public TempOrganisationValueObjectDataAdapterForOrgMatching(OrganisationTypes orgTypes)
			: this(orgTypes, null)
		{
		}

		public TempOrganisationValueObjectDataAdapterForOrgMatching(OrganisationTypes orgTypes, IOrgAddressSorter sorter)
		{
			this.sorter = sorter;
			OrgTypes = orgTypes;
		}

		readonly IOrgAddressSorter sorter;
		public readonly OrganisationTypes OrgTypes;

		public void ImportFromValueObjectForMatching(OrgHeaderForMatching org, Organisation valueObj, IValueObjectImportContext context)
		{
			var value = valueObj;

			if (OrgTypes != OrganisationTypes.None)
			{
				org.OrganisationTypes = OrgTypes;
			}

			if (value.OrganisationDetails.EDICodeSpecified)
			{
				org.OH_Code = value.OrganisationDetails.EDICode.Left(OrgHeader.Schema.OH_CodeMaxLength);
			}
			else if (value.EDICodeSpecified)
			{
				org.OH_Code = value.EDICode.Left(OrgHeader.Schema.OH_CodeMaxLength);
			}

			if (value.OrganisationDetails.NameSpecified)
			{
				org.OH_FullName = value.OrganisationDetails.Name.Left(OrgHeader.Schema.OH_FullNameMaxLength);
			}

			org.OH_Language = value.OrganisationDetails.Language.Left(OrgHeader.Schema.OH_LanguageMaxLength);

			if (value.OrganisationDetails.Location.IsSpecified)
			{
				org.OH_RL_NKClosestPort = value.OrganisationDetails.Location.Value.Left(OrgHeader.Schema.OH_RL_NKClosestPortMaxLength);
			}

			var errorContext = Res.GetString("38a7a74c-63d9-46a5-b010-2a5ec1cb1fd6", "Organization with code '{0}'", org.OH_Code);

			ImportOrganisationDetails(org, value.OrganisationDetails, context, errorContext);
			ImportOrGenerateEDICode(org, (XmlInterchange)context.Interchange, value);
			org.OnAfterImport();
		}

		protected void ImportOrganisationDetails(OrgHeaderForMatching org, OrganisationDetail organisationDetailsValue, IValueObjectImportContext context, string errorContext)
		{
			if (!organisationDetailsValue.IsSpecified)
			{
				return;
			}

			OrganisationValueObjectDataAdapterForOrgMatching.ImportAddresses(organisationDetailsValue.Addresses, org, context, errorContext, sorter);
			ImportRegistrationNumbers(org, organisationDetailsValue.RegistrationNumbers, context);

			ImportOrganisationTypes(org, organisationDetailsValue);
		}

		void ImportRegistrationNumbers(IOrgHeaderForMatching org, RegistrationNumberCollection registrationNumbers, IValueObjectImportContext context)
		{
			if (!registrationNumbers.IsSpecified)
			{
				return;
			}

			foreach (Xml.XsdVersion1.RegistrationNumber number in registrationNumbers)
			{
				if (OrganisationValueObjectDataAdapterHelper.NumberIsAUGSTAndABNIsSpecified(number, registrationNumbers))
				{
					continue;
				}

				if (OrganisationValueObjectDataAdapterHelper.IsUSDeprecatedSAN(number, org.OH_Code, context))
				{
					continue;
				}

				var numberType = OrganisationValueObjectDataAdapterHelper.GetNumberType(number, org.OH_Code, context);
				var country = RefCountry.LoadFromCountryCode(org.Factory, number.CountryOfRegistration);

				var orgCusCode = GetOrgCusCodeObjectForCodeAndCountry(org.CustomsCodes, numberType, country);
				if (!number.NumberSpecified)
				{
					continue;
				}

				if (orgCusCode == null)
				{
					orgCusCode = new OrgCusCodeForMatching();
					orgCusCode.OK_RN_NKCodeCountry = country == null ? ZString.Empty : country.Code;
					orgCusCode.OK_CodeType = numberType;
					orgCusCode.OK_CustomsRegNo = number.Number.Left(OrgCusCode.Schema.OK_CustomsRegNoMaxLength);
					org.CustomsCodes.Add(orgCusCode);

					if (!number.AddressCodeSpecified || !OrgCusCode.GetPremisesAddressIsAllowed(orgCusCode.OK_CodeType, orgCusCode.OK_RN_NKCodeCountry))
					{
						continue;
					}

					IMatchingAddress firstMatch = null;
					foreach (var adr in org.Addresses)
					{
						if (adr.OA_Code == number.AddressCode)
						{
							firstMatch = adr;
							break;
						}
					}

					if (firstMatch == null)
					{
						continue;
					}

					orgCusCode.OK_OA_PremisesAddress = firstMatch.PK;
				}
				else
				{
					orgCusCode.OK_CustomsRegNo = number.Number.Left(OrgCusCode.Schema.OK_CustomsRegNoMaxLength);
				}
			}
		}

		IOrgCusCodeForMatching GetOrgCusCodeObjectForCodeAndCountry(IList cusCodes, string numberType, RefCountry refCountry)
		{
			IOrgCusCodeForMatching result = null;
			ZString countryCode = refCountry == null ? ZString.Empty : refCountry.Code;
			foreach (IOrgCusCodeForMatching obj in cusCodes)
			{
				if (obj.OK_CodeType != numberType)
				{
					continue;
				}

				if ((!countryCode.IsValid || obj.OK_RN_NKCodeCountry != countryCode) && countryCode.IsValid)
				{
					continue;
				}

				result = obj;
				break;
			}
			return result;
		}

		void ImportOrganisationTypes(OrgHeaderForMatching organisation, AutoOrganisationDetail organisationDetail)
		{
			if (organisationDetail.OrganisationTypes != null)
			{
				foreach (Xml.XsdVersion1.OrganisationDetailOrganisationType orgType in organisationDetail.OrganisationTypes)
				{
					switch (orgType.Value)
					{
						case Xml.XsdVersion1.OrganisationTypes.A_P:
							organisation.OH_IsCreditor = true;
							break;

						case Xml.XsdVersion1.OrganisationTypes.A_R:
							organisation.OH_IsDebtor = true;
							break;

						case Xml.XsdVersion1.OrganisationTypes.ACT:
							organisation.OH_IsActive = true;
							break;

						case Xml.XsdVersion1.OrganisationTypes.BRK:
							organisation.OH_IsBroker = true;
							break;

						case Xml.XsdVersion1.OrganisationTypes.CAR:
							organisation.OH_IsShippingProvider = true;
							break;

						case Xml.XsdVersion1.OrganisationTypes.CNE:
							organisation.OH_IsConsignee = true;
							break;

						case Xml.XsdVersion1.OrganisationTypes.CNR:
							organisation.OH_IsConsignor = true;
							break;

						case Xml.XsdVersion1.OrganisationTypes.COM:
							organisation.OH_IsCompetitor = true;
							break;

						case Xml.XsdVersion1.OrganisationTypes.FWD:
							organisation.OH_IsForwarder = true;
							break;

						case Xml.XsdVersion1.OrganisationTypes.GLB:
							organisation.OH_IsGlobalAccount = true;
							break;

						case Xml.XsdVersion1.OrganisationTypes.NAT:
							organisation.OH_IsNationalAccount = true;
							break;

						case Xml.XsdVersion1.OrganisationTypes.SAL:
							organisation.OH_IsSalesLead = true;
							break;

						case Xml.XsdVersion1.OrganisationTypes.SVS:
							organisation.OH_IsMiscFreightServices = true;
							break;

						case Xml.XsdVersion1.OrganisationTypes.TMP:
							organisation.OH_IsTempAccount = true;
							break;

						case Xml.XsdVersion1.OrganisationTypes.TRN:
							organisation.OH_IsTransportClient = true;
							break;

						case Xml.XsdVersion1.OrganisationTypes.WHS:
							organisation.OH_IsWarehouseClient = true;
							break;
					}
				}
			}
		}

		void ImportOrGenerateEDICode(OrgHeaderForMatching organisation, XmlInterchange interchange, AutoOrganisation orgValue)
		{
			if (interchange.ImportEDICode)
			{
				organisation.OH_Code = orgValue.EDICode.Left(OrgHeader.Schema.OH_CodeMaxLength);
			}
			else
			{
				GenerateRandomCodeForOrg(organisation);
			}
		}

		void GenerateRandomCodeForOrg(OrgHeaderForMatching organisation)
		{
			organisation.OH_Code = organisation.OH_FullName.IsEmpty ? ZString.Empty : new ZString(ZGuid.NewZGuid().ToString().Replace("-", "")).Left(OrgHeader.Schema.OH_CodeMaxLength);
		}
	}
}

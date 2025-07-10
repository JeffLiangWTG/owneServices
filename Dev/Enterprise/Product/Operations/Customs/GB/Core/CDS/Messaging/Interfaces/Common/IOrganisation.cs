using System;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IOrganisation
	{
		ZString Name { get; }
		IAddress Address { get; }
		ZString ID { get; }
		ZBool IsForeignEori { get; }
		ZBool IsPrivateIndividual { get; }
	}

	public class OrganisationWrapper : IOrganisation
	{
		OrganisationWrapper(ZString name, ZString id, IAddress address, bool isPrivateIndividual, int nameLength = 70)
		{
			this.name = name;
			this.id = id;
			this.address = address;
			this.isPrivateIndividual = isPrivateIndividual;
			this.nameLength = nameLength;
		}

		public static OrganisationWrapper New(ZString name, ZString id, IAddress address = null, bool isPrivateIndividual = false, EoriNameAndAddressOrBoth outputPreference = EoriNameAndAddressOrBoth.BOTH, int nameLength = 70)
		{
			var shouldJustOutputID = outputPreference == EoriNameAndAddressOrBoth.EORI && !id.IsEmpty;
			var shouldJustOutputNameAndAddress = outputPreference == EoriNameAndAddressOrBoth.NAMEANDADDRESS && !id.IsEmpty;
			return new OrganisationWrapper(shouldJustOutputID ? ZString.Empty : name, shouldJustOutputNameAndAddress ? ZString.Empty : id, shouldJustOutputID ? null : address, isPrivateIndividual, nameLength);
		}

		public static OrganisationWrapper New(OrgAddress orgAddress, int nameLength = 70, int lineLength = 35)
		{
			return New(orgAddress, orgAddress?.Header.GetEuIdentificationNumber(), nameLength, lineLength);
		}

		public static OrganisationWrapper New(JobDocAddress organisationJobDocAddress, bool ignoreID = false, EoriNameAndAddressOrBoth outputPreference = EoriNameAndAddressOrBoth.BOTH)
		{
			OrganisationWrapper wrapper = null;

			if (organisationJobDocAddress != null)
			{
				var addressWrapper = AddressWrapper.New(AddressWrapper.GetAddressLine(organisationJobDocAddress.E2_Address1, organisationJobDocAddress.E2_Address2)
					, organisationJobDocAddress.E2_City
					, organisationJobDocAddress.E2_RN_NKCountryCode
					, organisationJobDocAddress.E2_Postcode
					);
				var org = organisationJobDocAddress.Organisation;
				var isNat = (org?.OH_Category ?? string.Empty) == OrgConstants.Category.NaturalPersonIndividual;
				ZString name;
				if (isNat)
				{
					ignoreID = true;
					name = $"{org.CountryCode}{CDSJobDeclarationValueSetStrategy.AddInfoCodeForImportPerson} {org.OH_FullName}";
				}
				else
				{
					name = organisationJobDocAddress.E2_CompanyName;
				}

				wrapper = New(name, !ignoreID ? GetID(organisationJobDocAddress) : ZString.Empty, addressWrapper, isNat, outputPreference);
			}

			return wrapper;
		}

		public static OrganisationWrapper New(OrgAddress address, string orgID, int nameLength = 70, int lineLength = 70)
		{
			return address == null ? null : New(address.CompanyName, orgID, AddressWrapper.New(address, lineLength), nameLength: nameLength);
		}

		public static OrganisationWrapper New(OrgAddress address, string orgID, EoriNameAndAddressOrBoth outputPreference)
		{
			return outputPreference switch
			{
				EoriNameAndAddressOrBoth.EORI => orgID.IsEmpty() ? New(address, ZString.Empty) : New(ZString.Empty, orgID),
				EoriNameAndAddressOrBoth.NAMEANDADDRESS => address != null ? New(address, ZString.Empty) : New(null, orgID),
				_ => New(address, orgID),
			};
		}

		static ZString GetID(JobDocAddress organisationJobDocAddress)
		{
			ZString result;
			if (organisationJobDocAddress.E2_AddressOverride)
			{
				var govRegNum = organisationJobDocAddress.E2_GovRegNum;
				var countryCode = organisationJobDocAddress.E2_RN_NKCountryCode;
				result = govRegNum.StartsWith(countryCode) ? govRegNum : countryCode + govRegNum;
			}
			else
			{
				result = organisationJobDocAddress.GetEuIdentificationNumberForCDS();
			}
			return result;
		}

		ZString IOrganisation.Name => name.StripNewlineCharacters(nameLength);

		ZString IOrganisation.ID => id.SubstringSafe(0, 17);

		IAddress IOrganisation.Address => address;

		ZBool IOrganisation.IsForeignEori => !id.IsEmpty
												&& !id.StartsWith(Core.Constants.CountryCodes.UnitedKingdom, StringComparison.OrdinalIgnoreCase)  // GB
												&& !id.StartsWith(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, StringComparison.OrdinalIgnoreCase)  // XI
												&& !id.StartsWith("00");  // special case for e.g. 00200. 00400, etc

		ZBool IOrganisation.IsPrivateIndividual => isPrivateIndividual;

		readonly ZString name;
		readonly ZString id;
		readonly IAddress address;
		readonly bool isPrivateIndividual;
		readonly int nameLength;
	}

	public enum EoriNameAndAddressOrBoth
	{
		EORI,
		NAMEANDADDRESS,
		BOTH
	}
}

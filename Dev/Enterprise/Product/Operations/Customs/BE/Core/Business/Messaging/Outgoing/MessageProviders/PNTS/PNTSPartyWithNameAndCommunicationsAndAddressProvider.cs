using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.BE.Business;

public class PNTSPartyWithNameAndCommunicationsAndAddressProvider : IPNTSPartyWithNameAndCommunicationsAndAddress
{
	public PNTSPartyWithNameAndCommunicationsAndAddressProvider(OrgAddress address, ZString name, ZString typeOfPerson, ZString street, ZString additionalLine, ZString country, ZString postcode, ZString city)
	{
		this.header = address?.Header;
		this.name = name;
		this.typeOfPerson = typeOfPerson;
		this.street = street;
		this.additionalLine = additionalLine;
		this.country = country;
		this.postcode = postcode;
		this.city = city;
	}

	readonly OrgHeader header;
	readonly ZString name;
	readonly ZString typeOfPerson;
	readonly ZString street;
	readonly ZString additionalLine;
	readonly ZString country;
	readonly ZString postcode;
	readonly ZString city;

	public int TypeOfPerson => ConvertTypeOfPerson(typeOfPerson);

	public IPNTSAddressExtended Address => addressExtended ??= new PNTSAddressExtendedProvider(street, additionalLine, country, postcode, city);
	IPNTSAddressExtended addressExtended;

	public IReadOnlyCollection<ICommunication> Communications => communications ??= MessageProviderHelper.GetCommunicationsFromOrgHeader(header);
	IReadOnlyCollection<ICommunication> communications;

	public string Name => name;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber, () => header?.GetConcatenatedSingleOrgCusCode(EuropeanUnionSharedCodeTypes.Eori));
	CachedValue<string> identificationNumber;

	int ConvertTypeOfPerson(ZString category)
	{
		var typeOfPerson = 0;

		switch (category)
		{
			case OrgConstants.Category.NaturalPersonIndividual:
				typeOfPerson = 1;
				break;
			case OrgConstants.Category.Business:
			case OrgConstants.Category.Government:
				typeOfPerson = 2;
				break;
			case OrgConstants.Category.NonGovernmentOrganisation:
				typeOfPerson = 3;
				break;
		}

		return typeOfPerson;
	}
}

using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public abstract class OrganizationProvider
	{
		public static ZString GetEori(OrgAddress orgAddress) => orgAddress?.GetEuIdentificationNumber(Core.Constants.CountryCodes.Ireland, ignoreCountryOfIssuanceIfNotMatched: true) ?? ZString.Empty;

		protected OrganizationProvider(OrgAddress orgAddress, string id)
		{
			this.orgAddress = orgAddress;
			this.id = id;
		}
		readonly OrgAddress orgAddress;
		readonly string id;

		public IFullAddress Address => address ?? (address = FullAddressProvider.New(orgAddress));
		IFullAddress address;

		public IReadOnlyCollection<IIdType> Communication => communications ?? (communications = new[] { CommunicationProvider.New(orgAddress.Header?.AllocatedContacts.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS), ZString.Empty) });
		IReadOnlyCollection<IIdType> communications;

		public string Name => orgAddress.EffectiveCompanyName;

		public string Id => id;
	}
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class PartyProvider : IParty
	{
		protected PartyProvider(OrgAddress address)
		{
			orgAddress = Argument.NotNull(address, nameof(address));
		}

		public static PartyProvider NewOrNull(OrgAddress address) => address == null ? null : new PartyProvider(address);

		readonly OrgAddress orgAddress;

		public string Name => orgAddress.Header?.OH_FullName.GetNullIfEmpty();

		public string IdentificationNumber => orgAddress.Header?.GetICS2EoriDetails().GetNullIfEmpty();

		public string Status => CachedValueHelper.GetValue(ref status, () => GetStatus());
		CachedValue<string> status;

		protected virtual string GetStatus()
		{
			return null;
		}

		public IAddress Address => CachedValueHelper.GetValue(ref addressCached, () => AddressProvider.NewOrNull(orgAddress));
		CachedValue<IAddress> addressCached;

		public IReadOnlyCollection<IIdentifierTypePair> Communications => communications ??= GetCommunicationProviderCollection();
		IReadOnlyCollection<IIdentifierTypePair> communications;

		IReadOnlyCollection<IIdentifierTypePair> GetCommunicationProviderCollection()
		{
			return new Collection<IIdentifierTypePair>()
			{
				new CommunicationProvider(orgAddress.OA_Phone_Formatted.GetNullIfEmpty(), CommunicationType.Codes.TE),
				new CommunicationProvider(orgAddress.OA_Email.GetNullIfEmpty(), CommunicationType.Codes.EM),
			};
		}

		public string TypeOfPerson => null;
	}
}

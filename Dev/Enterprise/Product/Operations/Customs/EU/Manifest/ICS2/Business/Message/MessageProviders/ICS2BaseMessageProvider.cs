using System;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ICS2BaseMessageProvider
	{
		public ICS2BaseMessageProvider(AsycudaManifestHeader header)
		{
			manifestHeader = Argument.NotNull(header, nameof(header));
		}

		protected readonly AsycudaManifestHeader manifestHeader;

		public string LRN => UsesPlaceHolder ? ICS2OutboundEDIMessage.LRNPlaceHolder : manifestHeader.LocalReferenceNumber.ToString();

		public string MRN => manifestHeader.RegistrationNumber;

		public DateTime CurrentDateTimeUtc => ZDateTime.UtcNow.ToDateTime();

		public IParty Declarant => CachedValueHelper.GetValue(ref declarant, () => DeclarantPartyProvider.NewOrNull(manifestHeader));
		CachedValue<IParty> declarant;

		protected virtual bool UsesPlaceHolder => false;
	}
}

using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend15;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmendHeader15Provider : ICS2BaseMessageProvider, ISendAndAmendHeader15
	{
		public SendAndAmendHeader15Provider(AsycudaManifestHeader header)
			: base(header)
		{
			helper = new MessageHeaderProviderHelper(Argument.NotNull(header, nameof(header)));
		}
		protected readonly MessageHeaderProviderHelper helper;

		public string ReferralRequestReference => GetReferralRequestReferenceCore();

		protected virtual string GetReferralRequestReferenceCore() => string.Empty;

		public string SpecificCircumstanceIndicator => helper.SpecificCircumstanceIndicator;

		public string AddressedMemberStateCountry => helper.AddressedMemberStateCountry;

		public IParty Representative => helper.Representative;

		public IActiveBorderTransportMeans ActiveBorderTransportMeans => CachedValueHelper.GetValue(ref activeBorderTransportMeans, () => ActiveBorderTransportMeansProvider.NewOrNull(manifestHeader));
		CachedValue<IActiveBorderTransportMeans> activeBorderTransportMeans;

		public IConsignmentMasterLevel ConsignmentMasterLevel => CachedValueHelper.GetValue(ref consignmentMasterLevel, () => SendAndAmend15ConsignmentMasterLevelProvider.NewOrNull(manifestHeader));
		CachedValue<IConsignmentMasterLevel> consignmentMasterLevel;

		public string TransportMode => helper.WCOTransportMode;

		public IConsignmentHouseLevel ConsignmentHouseLevel => SendAndAmend15ConsignmentHouseLevelProvider.NewOrNull(manifestHeader.Bills.Count > 0 ? manifestHeader.Bills[0] : null);

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformationMasterLevel => helper.AdditionalInformationsMasterLevel;
	}
}

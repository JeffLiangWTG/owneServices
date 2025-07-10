using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend14;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmendHeader14Provider : ICS2BaseMessageProvider, ISendAndAmendHeader14
	{
		public SendAndAmendHeader14Provider(AsycudaManifestHeader header) : base(header)
		{
			helper = new MessageHeaderProviderHelper(Argument.NotNull(header, nameof(header)));
		}

		protected readonly MessageHeaderProviderHelper helper;

		public string ReferralRequestReference => GetReferralRequestReferenceCore();

		protected virtual string GetReferralRequestReferenceCore() => string.Empty;

		public string SpecificCircumstanceIndicator => helper.SpecificCircumstanceIndicator;

		public string AddressedMemberStateCountry => helper.AddressedMemberStateCountry;

		public IParty Representative => helper.Representative;

		public string TransportMode => helper.WCOTransportMode;

		public IConsignmentMasterLevel ConsignmentMasterLevel => CachedValueHelper.GetValue(ref consignmentMasterLevel, () => SendAndAmend14ConsignmentMasterLevelProvider.NewOrNull(manifestHeader));
		CachedValue<IConsignmentMasterLevel> consignmentMasterLevel;
	}
}

using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend13;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmendHeader13Provider : ICS2BaseMessageProvider, ISendAndAmendHeader13
	{
		public SendAndAmendHeader13Provider(AsycudaManifestHeader header) : base(header)
		{
			helper = new MessageHeaderProviderHelper(Argument.NotNull(header, nameof(header)));
		}

		protected readonly MessageHeaderProviderHelper helper;

		public string ReferralRequestReference => GetReferralRequestReferenceCore();

		protected virtual string GetReferralRequestReferenceCore() => string.Empty;

		public string SpecificCircumstanceIndicator => helper.SpecificCircumstanceIndicator;

		public int ReentryIndicator => manifestHeader.ReEntryIndicator ? 1 : 0;

		public ISplitConsignment SplitConsignment => CachedValueHelper.GetValue(ref splitConsignment, () =>
		SplitConsignmentProvider.NewOrNull(manifestHeader));
		CachedValue<ISplitConsignment> splitConsignment;

		public IParty Representative => helper.Representative;

		public IActiveBorderTransportMeans ActiveBorderTransportMeans => CachedValueHelper.GetValue(ref activeBorderTransportMeans, () => ActiveBorderTransportMeansProvider.NewOrNull(manifestHeader));
		CachedValue<IActiveBorderTransportMeans> activeBorderTransportMeans;

		public IConsignmentMasterLevel ConsignmentMasterLevel => CachedValueHelper.GetValue(ref consignmentMasterLevel, () => SendAndAmend13ConsignmentMasterLevelProvider.NewOrNull(manifestHeader));
		CachedValue<IConsignmentMasterLevel> consignmentMasterLevel;

		public string EntryCustomsOfficeReferenceNumber => manifestHeader.AMA_CustomsOffice;
	}
}

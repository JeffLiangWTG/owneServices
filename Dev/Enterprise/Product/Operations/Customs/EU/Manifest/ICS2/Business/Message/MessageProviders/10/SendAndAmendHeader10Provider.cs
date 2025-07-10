using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend10;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmendHeader10Provider : ICS2BaseMessageProvider, ISendAndAmendHeaderF10
	{
		public SendAndAmendHeader10Provider(AsycudaManifestHeader header) : base(header)
		{
			helper = new MessageHeaderProviderHelper(Argument.NotNull(header, nameof(header)));
		}

		protected readonly MessageHeaderProviderHelper helper;

		public string SpecificCircumstanceIndicator => helper.SpecificCircumstanceIndicator;

		public int ReEntryIndicator => helper.ReEntryIndicator;

		public int SplitConsignmentIndicator => manifestHeader.SplitConsignmentIndicator ? 1 : 0;

		public string PreviousMRN => manifestHeader.PreviousMRN;

		public IParty Representative => helper.Representative;

		public IActiveBorderTransportMeans ActiveBorderTransportMeans => CachedValueHelper.GetValue(ref activeBorderTransportMeans, () => ActiveBorderTransportMeansProvider.NewOrNull(manifestHeader));
		CachedValue<IActiveBorderTransportMeans> activeBorderTransportMeans;

		public IConsignmentMasterLevel ConsignmentMasterLevel => CachedValueHelper.GetValue(ref consignmentMasterLevel, () => SendAndAmend10ConsignmentMasterLevelProvider.NewOrNull(manifestHeader));
		CachedValue<IConsignmentMasterLevel> consignmentMasterLevel;

		public string CustomsOfficeOfFirstEntry => helper.CustomsOfficeReferenceNumber;
	}
}

using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend16;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmendHeader16Provider : ICS2BaseMessageProvider, ISendAndAmendHeader16
	{
		protected readonly MessageHeaderProviderHelper helper;

		public SendAndAmendHeader16Provider(AsycudaManifestHeader header) : base(header)
		{
			helper = new MessageHeaderProviderHelper(Argument.NotNull(header, nameof(header)));
		}

		public string ReferralRequestReference => GetReferralRequestReferenceCore();

		protected virtual string GetReferralRequestReferenceCore() => string.Empty;

		public string SpecificCircumstanceIndicator => helper.SpecificCircumstanceIndicator;

		public string AddressedMemberStateCountry => helper.AddressedMemberStateCountry;

		public IParty Representative => helper.Representative;

		public IConsignmentHouseLevel ConsignmentHouseLevel => CachedValueHelper.GetValue(ref consignmentHouseLevel, () => SendAndAmend16ConsignmentHouseLevelProvider.NewOrNull(manifestHeader.Bills.Cast<AsycudaBill>().FirstOrDefault()));
		CachedValue<IConsignmentHouseLevel> consignmentHouseLevel;
	}
}

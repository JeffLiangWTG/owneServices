using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend17;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmendHeader17Provider : ICS2BaseMessageProvider, ISendAndAmendHeader17
	{
		public SendAndAmendHeader17Provider(AsycudaManifestHeader header) : base(header)
		{
			helper = new MessageHeaderProviderHelper(Argument.NotNull(header, nameof(header)));
		}

		protected readonly MessageHeaderProviderHelper helper;

		public string ReferralRequestReference => GetReferralRequestReferenceCore();

		protected virtual string GetReferralRequestReferenceCore() => string.Empty;

		public string SpecificCircumstanceIndicator => helper.SpecificCircumstanceIndicator;

		public string AddressedMemberStateCountry => helper.AddressedMemberStateCountry;

		public IParty Representative => helper.Representative;

		public IConsignmentHouseLevel ConsignmentHouseLevel => consignment ??= SendAndAmend17ConsignmentHouseLevelProvider.NewOrNull(manifestHeader.Bills.Cast<AsycudaBill>().FirstOrDefault());
		IConsignmentHouseLevel consignment;
	}
}

using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend26;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmendHeader26Provider : ICS2BaseMessageProvider, ISendAndAmendHeader26
	{
		public SendAndAmendHeader26Provider(AsycudaManifestHeader header) : base(header)
		{
			helper = new MessageHeaderProviderHelper(Argument.NotNull(header, nameof(header)));
		}

		protected readonly MessageHeaderProviderHelper helper;

		public string SpecificCircumstanceIndicator => helper.SpecificCircumstanceIndicator;

		public string AddressedMemberStateCountry => helper.AddressedMemberStateCountry;

		public string ReferralRequestReference => GetReferralRequestReferenceCore();

		protected virtual string GetReferralRequestReferenceCore() => string.Empty;

		public IParty Representative => helper.Representative;

		public string TransportMode => helper.WCOTransportMode;

		public IReadOnlyCollection<IConsignmentHouseLevel> Consignments => consignments ??= manifestHeader.Bills.ToArray<AsycudaBill, IConsignmentHouseLevel>(SendAndAmend26ConsignmentHouseLevelProvider.NewOrNull);
		IReadOnlyCollection<IConsignmentHouseLevel> consignments;

		public IReadOnlyCollection<IHrcmScreeningResults> HrcmScreeningResults => helper.HrcmScreeningResults;
	}
}

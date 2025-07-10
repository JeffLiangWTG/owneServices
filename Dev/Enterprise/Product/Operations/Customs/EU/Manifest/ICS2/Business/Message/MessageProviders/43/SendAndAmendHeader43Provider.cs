using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend43;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmendHeader43Provider : ICS2BaseMessageProvider, ISendAndAmendHeader43
	{
		public SendAndAmendHeader43Provider(AsycudaManifestHeader header) : base(header)
		{
			helper = new MessageHeaderProviderHelper(Argument.NotNull(header, nameof(header)));
		}

		protected readonly MessageHeaderProviderHelper helper;

		public string SpecificCircumstanceIndicator => helper.SpecificCircumstanceIndicator;

		public string AddressedMemberStateCountry => helper.AddressedMemberStateCountry;

		public IParty Representative => helper.Representative;

		public IReadOnlyCollection<IConsignmentHouseLevel> Consignments => consignments ??= manifestHeader.Bills.ToArray<AsycudaBill, IConsignmentHouseLevel>(SendAndAmend43ConsignmentHouseLevelProvider.NewOrNull);
		IReadOnlyCollection<IConsignmentHouseLevel> consignments;
	}
}

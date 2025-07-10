using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend25;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmendHeader25Provider : ICS2BaseMessageProvider, ISendAndAmendHeader25
	{
		public SendAndAmendHeader25Provider(AsycudaManifestHeader header) : base(header)
		{
			helper = new MessageHeaderProviderHelper(Argument.NotNull(header, nameof(header)));
		}

		protected readonly MessageHeaderProviderHelper helper;

		public string SpecificCircumstanceIndicator => helper.SpecificCircumstanceIndicator;

		public string AddressedMemberStateCountry => helper.AddressedMemberStateCountry;

		public IParty Representative => helper.Representative;

		public string PreviousDocumentIdentification => helper.PreviousDocumentIdentification;

		public IConsignmentHouseLevel Consignment => SendAndAmend25ConsignmentHouseLevelProvider.NewOrNull(manifestHeader.Bills.Count > 0 ? manifestHeader.Bills[0] : null);
	}
}

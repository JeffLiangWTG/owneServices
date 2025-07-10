using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class Q04HeaderProvider : ICS2BaseMessageProvider, IQ04Header
	{
		public Q04HeaderProvider(AsycudaManifestHeader manifestHeader) : base(manifestHeader)
		{
			helper = new MessageHeaderProviderHelper(Argument.NotNull(manifestHeader, nameof(manifestHeader)));
		}

		readonly MessageHeaderProviderHelper helper;

		public string AddressedMemberStateCountry => helper.AddressedMemberStateCountry;

		public string RepresentativeIdentificationNumber => helper.Representative?.IdentificationNumber ?? string.Empty;

		public IIdentifierTypePair TransportDocument => helper.TransportDocument;

		public string DeclarantIdentificationNumber => Declarant.IdentificationNumber;

		public string CustomsOfficeOfFirstEntry => string.IsNullOrEmpty(helper.AddressedMemberStateCountry) ? helper.CustomsOfficeReferenceNumber : string.Empty;
	}
}

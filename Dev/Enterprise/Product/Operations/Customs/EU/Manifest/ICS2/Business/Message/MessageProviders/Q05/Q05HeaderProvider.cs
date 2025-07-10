using System;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business;

public class Q05HeaderProvider : IQ05Header
{
	public Q05HeaderProvider(AsycudaManifestHeader header)
	{
		Argument.NotNull(header, nameof(header));
		manifestHeader = header;
	}
	readonly AsycudaManifestHeader manifestHeader;

	public string LRN { get; }
	public string MRN => manifestHeader.RegistrationNumber;
	public DateTime CurrentDateTimeUtc => ZDateTime.UtcNow.ToDateTime();
	public string RequestNotification => "1";
	public string FunctionalReference => ICS2OutboundEDIMessage.LRNPlaceHolder;
	public string RequestIdentificationNumber => manifestHeader.DeclarantEori;
	public IIdentifierTypePair TransportDocumentMasterLevel { get; }
	public IIdentifierTypePair TransportDocumentHouseLevel { get; }
}

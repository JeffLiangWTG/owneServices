using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity.CC313A;
using CargoWise.Types;
using Enterprise.Customs.GB.ICS.Business;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging.CC313A
{
	class HeaderWrapper : CommonHeaderWrapper, IHeader
	{
		public HeaderWrapper(AsycudaManifestHeaderBase asycudaManifestHeader, ZDateTime utcDateTime) : base(asycudaManifestHeader, utcDateTime)
		{
		}

		public string DocumentReferenceNumber => Manifest.RegistrationNumber.Left(21);

		public string AmendmentPlace => Manifest.Branch.Address1.Left(35);

		public string AmendmentPlaceLNG => string.Empty; // Do not send.

		public string DateAndTimeOfAmendment => UtcDateTime.ToString("yyyyMMddHHmm");
	}
}

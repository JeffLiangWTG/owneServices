using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity.CC315A;
using CargoWise.Types;
using Enterprise.Customs.GB.ICS.Business;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging.CC315A
{
	class HeaderWrapper : CommonHeaderWrapper, IHeader
	{
		public HeaderWrapper(AsycudaManifestHeaderBase asycudaManifestHeader, ZDateTime utcDateTime) : base(asycudaManifestHeader, utcDateTime)
		{
		}

		public string ReferenceNumber => Manifest.AMA_JobReference.Left(22);

		public string DeclarationPlace => Manifest.Branch.Address1.Left(35);

		public string DeclarationPlaceLNG => string.Empty; // Do not send.

		public string DeclarationDateAndTime => UtcDateTime.ToString("yyyyMMddHHmm");
	}
}

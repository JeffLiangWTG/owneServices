using System.Runtime.Serialization;

namespace Enterprise.Client.EDI.Licencing.Business
{
	[DataContract]
	public class UpgradePackageUrlRequest
	{
		[DataMember]
		public string EncryptedMessage { get; set; }

		[DataMember]
		public string LicenceCode { get; set; }

		[DataMember]
		public string CurrentVersionNumber { get; set; }

		[DataMember]
		public bool IsPatchOnly { get; set; }
	}
}

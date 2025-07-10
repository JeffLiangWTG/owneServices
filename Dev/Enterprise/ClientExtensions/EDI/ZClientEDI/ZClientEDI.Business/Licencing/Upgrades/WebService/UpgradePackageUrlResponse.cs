using System.Runtime.Serialization;

namespace Enterprise.Client.EDI.Licencing.Business
{
	[DataContract]
	public class UpgradePackageUrlResponse
	{
		[DataContract]
		public enum ResponseClassType
		{
			[EnumMember(Value = "Success")]
			Success,
			[EnumMember(Value = "Failed")]
			Failed,
			[EnumMember(Value = "Error")]
			Error
		}

		[DataMember]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		public string URL { get; set; }

		[DataMember]
		public string VersionNumber { get; set; }

		[DataMember]
		public ResponseClassType ResponseClass { get; set; }

		[DataMember]
		public string ErrorMessage { get; set; }
	}
}

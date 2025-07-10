using System.Runtime.Serialization;

namespace Enterprise.CustomerService.Business.WebService
{
	public class IncidentResponse
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
		public ResponseClassType ResponseClass { get; set; }

		[DataMember]
		public string Response { get; set; }
	}
}

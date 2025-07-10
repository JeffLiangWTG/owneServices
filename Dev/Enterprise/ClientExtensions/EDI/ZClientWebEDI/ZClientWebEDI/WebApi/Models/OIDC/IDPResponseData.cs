using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace Enterprise.ZClientWebCargoWiseEDI.OIDC
{
	public class IDPResponseData
	{
		public IDPResponseData() { }

		public IDPResponseData(NameValueCollection querySet)
		{
			Code = querySet[OIDCLoginHelper.Constants.CodeKey];
			State = querySet[OIDCLoginHelper.Constants.StateKey];
			Error = querySet[OIDCLoginHelper.Constants.ErrorKey];
			ErrorDescription = querySet[OIDCLoginHelper.Constants.ErrorDescriptionKey];
		}

		[DataMember(Name = "code")]
		public string Code { get; set; }

		[DataMember(Name = "state")]
		public string State { get; set; }

		[DataMember(Name = "error")]
		public string Error { get; set; }

		[DataMember(Name = "error_description")]
		public string ErrorDescription { get; set; }

		public bool HasError => !string.IsNullOrEmpty(Error);
	}
}

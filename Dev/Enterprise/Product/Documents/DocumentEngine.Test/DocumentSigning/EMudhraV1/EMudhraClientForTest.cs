using System;
using System.Net;
using Enterprise.DocumentEngine.DigitalSignature.EMudhra.V1;

namespace Enterprise.DocumentEngine.DigitalSignature.Testing
{
	sealed class EMudhraClientForTest : EMudhraClient
	{
		public string LastSignDocReqAsString;
		public SignDocResp Response;
		public bool ThrowOnWeb;
		public bool ThrowWebException;
		public string ResponseAsString = string.Empty;
		protected override SignDocResp DoWebRequest(string signDocReqAsString)
		{
			if (Response == null && !ThrowOnWeb)
			{
				return base.DoWebRequest(signDocReqAsString);
			}

			LastSignDocReqAsString = signDocReqAsString;

			if (ThrowOnWeb)
			{
				if (ThrowWebException)
				{
					throw new WebException();
				}
				else
				{
					throw new Exception();
				}
			}

			return Response;
		}

		protected override string GetResponseString(string signDocReqAsString)
		{
			return string.IsNullOrEmpty(ResponseAsString) ? base.GetResponseString(signDocReqAsString) : ResponseAsString;
		}
	}
}

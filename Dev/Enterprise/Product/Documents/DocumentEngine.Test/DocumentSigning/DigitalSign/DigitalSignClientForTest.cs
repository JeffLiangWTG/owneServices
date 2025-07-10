using System;
using System.Collections.Generic;
using System.Net;
using Enterprise.DocumentEngine.DigitalSignature.DigitalSign;
using Newtonsoft.Json.Linq;

namespace Enterprise.DocumentEngine.DigitalSignature.Testing
{
	sealed class DigitalSignClientForTest : DigitalSignClient
	{
		public int LastRequestType;
		public Dictionary<string, object> LastBodyContent;
		public JObject Response;
		public bool ThrowOnWeb;
		public bool ThrowWebException;
		public JObject WSResponse;
		public bool EmptyCertificateContent;

		public DigitalSignClientForTest(bool emptyCertificateContent = false)
		{
			EmptyCertificateContent = emptyCertificateContent;
			InitializeCertificate(out var response);
		}

		protected override JObject DoWebRequest(int requestType, Dictionary<string, object> bodyContent)
		{
			if (Response == null && !ThrowOnWeb)
			{
				return base.DoWebRequest(requestType, bodyContent);
			}

			LastRequestType = requestType;
			LastBodyContent = bodyContent;

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

		protected override byte[] TryGetCertificateContent(out JObject response)
		{
			response = new JObject();
			if (EmptyCertificateContent)
			{
				return Array.Empty<byte>();
			}

			var encodedCert = DigitalSignClientTest.CreateTestCertificate();
			return encodedCert;
		}

		protected override JObject GetWebServiceResponse(string url, string accessToken, Dictionary<string, object> bodyContent)
		{
			return WSResponse ?? base.GetWebServiceResponse(url, accessToken, bodyContent);
		}
	}
}

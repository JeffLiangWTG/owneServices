using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using CargoWise.Common;
using Enterprise.DocumentEngine.DocumentSigning.DigitalSign;
using Newtonsoft.Json.Linq;

namespace Enterprise.DocumentEngine.DigitalSignature.DigitalSign
{
	class DigitalSignClient
	{
		JObject RequestSign(Dictionary<string, byte[]> fileHashes)
		{
			var docsToSign = new List<Dictionary<string, string>>();
			var sha = SHA256.Create();

			using (sha)
			{
				foreach (var signableContent in fileHashes)
				{
					var document = new Dictionary<string, string>();
					document[DigitalSignConstants.Body.DocumentAlias] = signableContent.Key;
					document[DigitalSignConstants.Body.HashAlgorithm] = DigitalSignConstants.SHA256_ID;

					var hash = sha.ComputeHash(signableContent.Value);
					var doc1Hash64 = Convert.ToBase64String(hash);
					document[DigitalSignConstants.Body.DocumentHashBase64] = doc1Hash64;

					docsToSign.Add(document);
				}
			}

			var bodyContentSign = new Dictionary<string, object>();
			bodyContentSign[DigitalSignConstants.Body.CertificateName] = certificateAlias;
			bodyContentSign[DigitalSignConstants.Body.RequestDescription] = "Request_" + DateTimeOffset.Now.ToUnixTimeSeconds();
			bodyContentSign[DigitalSignConstants.Body.TotpID] = AuthorizerTotpID;
			bodyContentSign[DigitalSignConstants.Body.TotpValue] = DigitalSignHelper.GetNewTotpValue(AuthorizerTotpSecretKey);
			bodyContentSign[DigitalSignConstants.Body.DocumentsArray] = docsToSign;

			return DoWebRequest(DigitalSignConstants.RequestType.SigningRequest, bodyContentSign);
		}

		public bool InitializeCertificate(out JObject certResponse)
		{
			CertificateContent = TryGetCertificateContent(out var response);
			if (response.ContainsKey(DigitalSignConstants.Response.ErrorCode))
			{
				certResponse = response;
				return false;
			}
			certResponse = new JObject();
			return true;
		}

		public virtual JObject Sign(Dictionary<string, byte[]> fileHashes)
		{
			if (CertificateContent.IsNullOrEmpty())
			{
				return DigitalSignHelper.GetError(DocumentSigningConstants.WebCertificateServerTimeOutCode, DocumentSigningConstants.WebCertificateServerTimeOutMsg);
			}

			JObject signResponse;

			try
			{
				var responseRequest = RequestSign(fileHashes);
				var signRequestID = responseRequest.GetValue(DigitalSignConstants.Response.RequestID)?.ToString();

				signResponse = responseRequest;
				if (signRequestID != null)
				{
					var bodyContent = new Dictionary<string, object>();
					bodyContent[DigitalSignConstants.Body.RequestID] = signRequestID;

					var i = 0;
					do
					{
						signResponse = DoWebRequest(DigitalSignConstants.RequestType.FinalizeSigning, bodyContent);
						var signedDocuments = signResponse.SelectToken(DigitalSignConstants.Response.SignedDocuments);
						if (signedDocuments != null)
						{
							break;
						}
						else
						{
							Thread.Sleep(MsServerTimeout);
						}

						i++;
					}
					while (i < WebRequestReadAttempts);
				}
			}
			catch (DigitalSignHelper.BadResponseException ex)
			{
				signResponse = DigitalSignHelper.GetError(DocumentSigningConstants.BadResponse, ex.ToString());
			}
			catch (WebException ex)
			{
				signResponse = DigitalSignHelper.GetError(DocumentSigningConstants.WebException, ex.ToString());
			}
			catch (Exception ex)
			{
				signResponse = DigitalSignHelper.GetError(DocumentSigningConstants.Unhandled, ex.ToString());
			}

			return signResponse;
		}

		public string AuthorizerTotpID { get; set; }
		public string AuthorizerTotpSecretKey { get; set; }
		public string AccessToken { get; set; }
		public string EndpointUrl { get; set; }
		public byte[] CertificateContent { get; set; }

		string certificateAlias
		{
			get
			{
				if (fCertificateAlias == null)
				{
					fCertificateAlias = GetAuthorizerCertificate();
					return fCertificateAlias;
				}
				return fCertificateAlias;
			}
			set
			{
				fCertificateAlias = value;
			}
		}
		string fCertificateAlias;

		string GetAuthorizerCertificate()
		{
			var bodyContent = new Dictionary<string, object>();
			bodyContent[DigitalSignConstants.Body.TotpID] = AuthorizerTotpID;
			var response = DoWebRequest(DigitalSignConstants.RequestType.GetAssociatedCertificate, bodyContent);
			var certificate = response.GetValue(DigitalSignConstants.Response.CertificateName)?.ToString();
			return certificate ?? string.Empty;
		}

		protected virtual byte[] TryGetCertificateContent(out JObject response)
		{
			var certificateContent = Array.Empty<byte>();
			var bodyContent = new Dictionary<string, object>();
			bodyContent[DigitalSignConstants.Body.TotpID] = AuthorizerTotpID;

			response = DoWebRequest(DigitalSignConstants.RequestType.GetAssociatedCertificate, bodyContent);
			var cert64 = response.GetValue(DigitalSignConstants.Response.CertificateContent)?.ToString();
			if (cert64 != null)
			{
				certificateContent = Convert.FromBase64String(cert64);
			}
			return certificateContent;
		}

		protected virtual JObject DoWebRequest(int requestType, Dictionary<string, object> bodyContent)
		{
			JObject response = null;
			string url;

			try
			{
				switch (requestType)
				{
					case DigitalSignConstants.RequestType.GetAssociatedCertificate:
						url = EndpointUrl + DigitalSignConstants.AssociatedCertificateUrlSuffix;
						response = GetWebServiceResponse(url, AccessToken, bodyContent);
						break;
					case DigitalSignConstants.RequestType.SigningRequest:
						url = EndpointUrl + DigitalSignConstants.RequestSignUrlSuffix;
						response = GetWebServiceResponse(url, AccessToken, bodyContent);
						break;
					case DigitalSignConstants.RequestType.FinalizeSigning:
						url = EndpointUrl + DigitalSignConstants.SignUrlSuffix;
						response = GetWebServiceResponse(url, AccessToken, bodyContent);
						break;
				}
			}
			catch (DigitalSignHelper.BadResponseException ex)
			{
				throw new DigitalSignHelper.BadResponseException($"Incorrect response received from the API. Please verify the endpoint url in the registry: {EndpointUrl}", ex);
			}

			return response;
		}

		protected virtual JObject GetWebServiceResponse(string url, string accessToken, Dictionary<string, object> bodyContent)
		{
			return DigitalSignHelper.getResponse(url, AccessToken, bodyContent);
		}

		protected const int MsServerTimeout = 250;
		protected const int WebRequestReadAttempts = 5;
	}
}

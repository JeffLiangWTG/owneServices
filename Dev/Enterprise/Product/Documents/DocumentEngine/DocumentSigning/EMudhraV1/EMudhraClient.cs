using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Xml;
using WTG.Foundation.Http;

namespace Enterprise.DocumentEngine.DigitalSignature.EMudhra.V1
{
	class EMudhraClient
	{
		public SignDocRespDocSignature Sign(byte[] fileHash)
		{
			var rsp = Sign(new[] { fileHash });
			var sig = rsp.DocSignatures.OfType<SignDocRespDocSignature>().FirstOrDefault();

			return sig ?? new SignDocRespDocSignature()
			{
				docErrorCode = rsp.errorCode,
				docErrorMessage = rsp.errorMessage
			};
		}

		public virtual SignDocResp Sign(IEnumerable<byte[]> fileHashes)
		{
			try
			{
				var doc = new Base64();
				var id = 1;
				foreach (var fileHash in fileHashes)
				{
					var hash = doc.InputHash.AddNew();
					hash.id = id.ToString();
					hash.Value = BitConverter.ToString(fileHash).Replace("-", "").ToLower();
					hash.hashAlgorithm = "SHA256";
					hash.responseSigType = "PKCS7";
					id++;
				}

				var docAsXml = SerializeWithoutXmlHeader(doc);
				var docAsBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(docAsXml));
				var signDocReq = new SignDocReq();
				signDocReq.Docs = docAsBase64;
				signDocReq.version = "1.0";
				signDocReq.sessionKey = "0";

				//TODO:use ZDateTime?
				var tzString = (NoResString)"India Standard Time";
				var tz = TimeZoneInfo.FindSystemTimeZoneById(tzString);
				var localTm = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
				var ts = localTm.ToString("yyyy-MM-ddTHH:mm:ss") + tz.BaseUtcOffset.ToString(@"\+hh\:mm");
				signDocReq.ts = ts;

				signDocReq.txn = Guid.NewGuid().ToString().Replace("-", "");
				signDocReq.clientID = ClientID;
				signDocReq.keyID = KeyID;
				signDocReq.partnerID = PartnerID;
				signDocReq.partnerAccessKey = PartnerAccessKey;

				var accessKeyhash = SHA256AsString(signDocReq.txn + AccessKey + docAsBase64);
				signDocReq.accessKeyhash = accessKeyhash;

				var signDocReqAsString = SerializeWithoutXmlHeader(signDocReq);

				return DoWebRequest(signDocReqAsString);
			}
			catch (BadResponseException ex)
			{
				return new SignDocResp()
				{
					errorCode = DocumentSigningConstants.BadResponse,
					errorMessage = ex.ToString()
				};
			}
			catch (WebException ex)
			{
				return new SignDocResp()
				{
					errorCode = DocumentSigningConstants.WebException,
					errorMessage = ex.ToString()
				};
			}
			catch (Exception ex)
			{
				return new SignDocResp()
				{
					errorCode = DocumentSigningConstants.Unhandled,
					errorMessage = ex.ToString()
				};
			}
		}

		protected virtual SignDocResp DoWebRequest(string signDocReqAsString)
		{
			var responseAsString = GetResponseString(signDocReqAsString);
			try
			{
				return Deserialize<SignDocResp>(responseAsString);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw new BadResponseException($"Incorrect response received from the API. Please verify the endpoint url in the registry: {WebApiUrlSource}\r\nResponse: {responseAsString}", ex);
			}
		}

		protected virtual string GetResponseString(string signDocReqAsString) =>
			GetHttpResponseString(signDocReqAsString);

		internal string GetHttpResponseString(string signDocReqAsString)
		{
			using var httpClient = ObjectFactory.Get<IHttpClientFactory>().Create();

			var content = new StringContent(WebUtility.UrlEncode(signDocReqAsString));

			var response = httpClient.PostAsync(WebApiUri, content).GetAwaiter().GetResult();
			response.EnsureSuccessStatusCode();
			var responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			return responseBody;
		}

		public string WebApiUri { get; set; }
		public string WebApiUrlSource { get; set; }
		public string AccessKey { get; set; }
		public string ClientID { get; set; }
		public string KeyID { get; set; }
		public string PartnerID { get; set; }
		public string PartnerAccessKey { get; set; }

		static string SerializeWithoutXmlHeader<T>(T obj)
		{
			// Remove Declaration
			var settings = new XmlWriterSettings
			{
				Indent = true,
				OmitXmlDeclaration = true
			};

			// Remove Namespace
			var ns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

			using (var stream = new StringWriter())
			using (var writer = XmlWriter.Create(stream, settings))
			{
				var serializer = ZXmlSerializer.New(typeof(T));
				serializer.Serialize(writer, obj, ns);
				return stream.ToString();
			}
		}

		static T Deserialize<T>(string xml)
		{
			using (var stream = new StringReader(xml))
			{
				return (T)ZXmlSerializer.New(typeof(T)).Deserialize(stream);
			}
		}

		static string SHA256AsString(string content)
		{
			using var sha256 = SHA256.Create();
			return BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(content))).Replace("-", "").ToLower();
		}

		[Serializable]
		class BadResponseException : Exception
		{
			public BadResponseException(string message, Exception innerException) : base(message, innerException)
			{
			}

#if NETFRAMEWORK
			protected BadResponseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
			{
			}
#endif
		}
	}
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.DocumentEngine.DigitalSignature;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OtpNet;

namespace Enterprise.DocumentEngine.DocumentSigning.DigitalSign
{
	class DigitalSignHelper
	{
		public static JObject getResponse(string endPoint, string accessToken, Dictionary<string, object> bodyContent)
		{
			var client = new HttpClient();
			var bodyContentJson = JsonConvert.SerializeObject(bodyContent);

			var request = new HttpRequestMessage
			{
				Method = HttpMethod.Post,
				RequestUri = new Uri(endPoint),
				Headers =
				{
					{ (NoResString)"Authorization", (NoResString)"Bearer " + accessToken }
				},
				Content = new StringContent(bodyContentJson, Encoding.UTF8, "application/json")
			};

			try
			{
				using (var response = client.SendAsync(request))
				{
					var body = response.Result.Content.ReadAsStringAsync();
					if (response.Result.StatusCode != HttpStatusCode.OK)
					{
						return GetResponseError(body.Result);
					}
					else
					{
						return JsonConvert.DeserializeObject<JObject>(body.Result);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw new BadResponseException($"Incorrect response received from the API. Please verify the endpoint url in the registry: {endPoint}", ex);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "The string literal is safe to use in this context and does not need to be externalized.")]
		static JObject GetResponseError(string body)
		{
			JObject response;
			try
			{
				var jsonObject = JsonConvert.DeserializeObject<JObject>(body);
				var jsonErrorCode = jsonObject.GetValue("error") ?? jsonObject.GetValue("errorCode");
				response = GetError(jsonErrorCode.ToString(), jsonObject.GetValue("error_description").ToString());
			}
			catch (Exception)
			{
				var bodyElement = XElement.Parse(body);
				response = GetError(bodyElement.Element("errorCode").Value, bodyElement.Element("error_description").Value);
			}
			return response;
		}

		public static string GetNewTotpValue(string authorizerTotpSecretKey)
		{
			try
			{
				var totp = new Totp(Base32Encoding.ToBytes(authorizerTotpSecretKey));
				return totp.ComputeTotp();
			}
			catch (ArgumentException)
			{
				return string.Empty;
			}
		}

		public static JObject GetError(string errorCode, string errorMessage)
		{
			var errorResponse = new JObject();
			errorResponse.Add(DigitalSignConstants.Response.ErrorCode, errorCode);
			errorResponse.Add(DigitalSignConstants.Response.ErrorMessage, errorMessage.Length > 500 ? errorMessage.Substring(0, 500) : errorMessage);
			return errorResponse;
		}

		[Serializable]
		public class BadResponseException : Exception
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

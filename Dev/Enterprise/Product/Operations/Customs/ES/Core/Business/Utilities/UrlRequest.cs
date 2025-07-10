#if NETFRAMEWORK
using System;
using System.IO;
#else
using System.Net.Http;
using CargoWise.Application;
using WTG.Foundation.Http;
#endif
using System.Net;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class UrlRequest
	{
		public UrlRequest(ZString url)
		{
			this.url = Argument.NotNullOrEmpty(url, nameof(url));
		}
		readonly ZString url;

		public ZString GetResponseFromUrl(GlbExternalPassword certificate)
		{
#if NETFRAMEWORK
			var htmlResponseText = string.Empty;
			var httpRequest = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));

			httpRequest.ClientCertificates.Add(GetCertificateForRequest(certificate));

			using (var response = (HttpWebResponse)httpRequest.GetResponse())
			{
				using (var responseStream = response.GetResponseStream())
				{
					using (var reader = new StreamReader(responseStream))
					{
						var responseText = reader.ReadToEnd();

						htmlResponseText = WebUtility.HtmlDecode(responseText);
					}
				}
			}

			return htmlResponseText;
#else
			var htmlResponseText = string.Empty;

			using (var handler = new HttpClientHandler())
			{
				handler.ClientCertificates.Add(GetCertificateForRequest(certificate));

				using (var httpClient = ObjectFactory.Get<IHttpClientFactory>().CreateNew(handler))
				{
					var response = httpClient.GetAsync(url.ToString()).Result;

					response.EnsureSuccessStatusCode();

					var responseText = response.Content.ReadAsStringAsync().Result;

					htmlResponseText = WebUtility.HtmlDecode(responseText);
				}
			}

			return htmlResponseText;
#endif
		}

		X509Certificate2 GetCertificateForRequest(GlbExternalPassword cert)
		{
			byte[] certifacteBytes = cert.GP_Certificate;
			var certificatePass = cert.CurrentDecryptedCertificatePassphrase;
			var certificate2 = new X509Certificate2(certifacteBytes, certificatePass);
			return certificate2;
		}
	}
}

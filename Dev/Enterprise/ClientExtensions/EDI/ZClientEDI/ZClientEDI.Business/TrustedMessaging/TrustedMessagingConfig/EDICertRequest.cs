using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.TrustedMessaging.Business
{
	public interface IEDICertRequest
	{
		bool TrySubmitSafe(string soapRequestTemplate, string subjectName, byte[] keyBlob, ICredentials credentials, EdiCertRequestContext context, out string output);
	}

	public class EDICertRequest : IEDICertRequest
	{
		readonly IEDICertRequestEncoder requestEncoder;

		public EDICertRequest()
		{
			this.requestEncoder = new EDICertRequestEncoder();
		}

		public EDICertRequest(IEDICertRequestEncoder requestEncoder)
		{
			this.requestEncoder = requestEncoder;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public bool TrySubmitSafe(string soapRequestTemplate, string subjectName, byte[] keyBlob, ICredentials credentials, EdiCertRequestContext context, out string output)
		{
			output = "";
			Exception currentException = null;

			for (var tryCount = 0; tryCount < MaxTryCount; tryCount++)
			{
				try
				{
					TrySubmitCore(soapRequestTemplate, subjectName, keyBlob, credentials, context, out output);
					return true;
				}
				catch (Exception ex)
				{
					currentException = ex;
					SleepOnRetry();
				}
			}

			ReportError(output, currentException);
			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void TrySubmitCore(string soapRequestTemplate, string subjectName, byte[] keyBlob, ICredentials credentials, EdiCertRequestContext context, out string output)
		{
			output = "";
			try
			{
				var xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(soapRequestTemplate);
				var caServer = xmlDoc.GetElementsByTagName("a:To")[0].InnerText;

				var soapRequestXml = requestEncoder.Encode(soapRequestTemplate, subjectName, keyBlob);

				var soapResponseXml = ExecuteRequest(caServer, credentials, soapRequestXml);

				output = requestEncoder.Decode(soapResponseXml, subjectName);
			}
			catch (WebException ex)
			{
				using (var stream = ex.Response?.GetResponseStream())
				{
					var responseAsString = stream != null ? new StreamReader(stream).ReadToEnd() : "";
					output = FormattableString.Invariant($"{ex} - {responseAsString}\n{context}");
				}
				throw;
			}
			catch (Exception ex)
			{
				output = FormattableString.Invariant($"{ex}\n{context}");
				throw;
			}
		}

		static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

		static void ReportError(string errorMsg, Exception ex)
		{
			if (GlbCompany.CurrentCompany.DatabaseType == DatabaseTypes.Codes.Production)
			{
				ExceptionReporter.Instance.ReportDeveloperException("Could not complete certificate request", errorMsg, ex);

				try
				{
					var email = new EmailDef();
					email.Body = errorMsg;
					email.Subject = "Could not complete certificate request";
					Env.OutgoingMailManager.CreateAndSave(email, EDIDataRegistry.Instance.InternalNotificationGroup.Value, GroupSourceLocator.GetFromRegistryItem(EDIDataRegistry.Instance.InternalNotificationGroup));
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
				}
			}
		}

		protected virtual string ExecuteRequest(string caServer, ICredentials credentials, string soapRequestXml)
		{
			try
			{
				ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidationCallback;

				var httpRequest = CreateHttpWebRequest(new Uri(caServer));
				httpRequest.Method = "POST";

				if (credentials != null)
				{
					httpRequest.UseDefaultCredentials = false;
					httpRequest.PreAuthenticate = true;
					httpRequest.Credentials = credentials;
				}

				var soapRequestBytes = Encoding.UTF8.GetBytes(soapRequestXml);
				httpRequest.ContentType = "application/soap+xml; encoding='utf-8'";
				httpRequest.ContentLength = soapRequestBytes.Length;

				using (var requestStream = httpRequest.GetRequestStream())
				{
					requestStream.Write(soapRequestBytes, 0, soapRequestBytes.Length);
					requestStream.Close();
				}

				var soapResponseXml = string.Empty;
				using (var response = (HttpWebResponse)httpRequest.GetResponse())
				{
					using (var responseStream = response.GetResponseStream())
					{
						using (var reader = new StreamReader(responseStream))
						{
							soapResponseXml = reader.ReadToEnd();
						}
					}
				}

				return soapResponseXml;
			}
			finally
			{
				ServicePointManager.ServerCertificateValidationCallback -= RemoteCertificateValidationCallback;
			}
		}

#pragma warning disable SYSLIB0014 // WebRequest.Create(Uri) is obsolete: WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.
		protected virtual HttpWebRequest CreateHttpWebRequest(Uri uri) => (HttpWebRequest)HttpWebRequest.Create(uri);
#pragma warning restore SYSLIB0014

		protected virtual void SleepOnRetry() => System.Threading.Thread.Sleep(TimeSpan.FromSeconds(10));

		protected virtual int MaxTryCount => 3;
	}
}

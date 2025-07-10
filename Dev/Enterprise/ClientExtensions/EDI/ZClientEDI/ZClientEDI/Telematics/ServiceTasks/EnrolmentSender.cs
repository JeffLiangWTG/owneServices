using System;
using System.Net;
using System.Net.Http;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Telematics.Tca;
using Enterprise.Integration;
using Enterprise.Telematics.Business.Registry;
using WTG.Foundation.Http;
using IHttpClientFactory = WTG.Foundation.Http.IHttpClientFactory;

namespace Enterprise.Client.EDI.Telematics.ServiceTasks
{
	class EnrolmentSender : IEnrolmentSender
	{
		public EnrolmentSender(IHttpClientFactory httpClientFactory, ILogger logger, TimeSpan timeout)
		{
			this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.timeout = timeout;
		}

		public void Send(BusinessObjectFactory factory, EnrolmentReportType enrolments)
		{
			var enrollmentReportId = TelematicsRimEnrolmentReportNumberStrategy.GetReportNumber(factory);
			var message = TcaXmlSerializer.SerializeToTelematicsRimData(enrolments, "http://www.tca.gov.au/schemas/tde/core/enrolment-report/2018-07");
			var content = new StringContent(message, Encoding.UTF8, "application/xml");
			using (var client = GetHttpClient())
			using (var response = client.PutAsync($"{TelematicsConfigurationRegistry.Instance.TcaRimUrl.Value}enrolment-report/{enrollmentReportId}", content).Result)
			{
				if (!response.IsSuccessStatusCode)
				{
					logger.Log(LogType.Error, $"TCA Enrollment report failure, Status Code: {response.StatusCode} Response: {response.Content.ReadAsStringAsync().Result}");
				}
			}
		}

		HttpClient GetHttpClient()
		{
			var httpClient = httpClientFactory
					.CreateNew(
						new HttpClientHandlerWithDiagnostics(new CookieContainer())
						{
							Credentials = new NetworkCredential(TelematicsConfigurationRegistry.Instance.TcaRimUsername.Value, TelematicsConfigurationRegistry.Instance.TcaRimPassword.Value),
							ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
						},
						timeout);
				httpClient.BaseAddress = new Uri(TelematicsConfigurationRegistry.Instance.TcaRimUrl.Value);
				return httpClient;
		}

		readonly ILogger logger;
		readonly IHttpClientFactory httpClientFactory;
		readonly TimeSpan timeout;
	}
}

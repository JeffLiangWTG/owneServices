using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.eHubMessaging.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskCodes.EAM,
	ServiceTaskNames.EAdaptorOutboundMessages,
	"ESV",
	typeof(Enterprise.eHubMessaging.ServiceTasks.eAdaptorOutboundServiceTask),
		AllowsMultipleInstances = true,
		MinimumPeriod = "10seconds",
		MaximumPeriod = "1day",
		DefaultScheduleRunEvery = "15minutes"
		)]
	
[assembly: HostedServiceBusinessObjectBinding("EAM", EDIInterchangeSchema.Constants.TableName, new[] { EDIInterchangeSchema.Constants.EI_IsActive + "=Y", EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=TRX", EDIInterchangeSchema.Constants.EI_Status + "=AQU", }, "eAdaptor Outbound")]
namespace Enterprise.eHubMessaging.ServiceTasks
{
	class eAdaptorOutboundServiceTask : eHubServiceTaskWithAdaptor
	{
		public eAdaptorOutboundServiceTask()
			: base(new eAdaptorFactory(), new eAdaptorOutboundCommunicationDiagnosterFactory())
		{
		}

#if DEBUG
		public
#endif
		eAdaptorOutboundServiceTask(IAdaptorFactory adaptorFactory, IEHubCommunicationDiagnosterFactory diagnosterFactory)
			: base(adaptorFactory, diagnosterFactory)
		{
		}

		internal override IEnumerable<IeHubServiceTaskJob> GetJobs()
		{
			yield return new eAdaptorOutboundServiceTaskJob(this, Notifier, AdaptorFactory);
		}

		public override string ServiceTaskName => ServiceTaskNames.EAdaptorOutboundMessages;
		public override string DefaultServerAddress => eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

		protected override bool ExecuteJob(IeHubServiceTaskJob job)
		{
			try
			{
				return base.ExecuteJob(job);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Notifier.AddError(GetErrorLog(ex, ex.Message));
				ErrorReporter.ReportOnce(GetErrorReportKey(ex), ex);
			}

			return false;
		}

		protected override bool HandleCommunicationException(CommunicationException communicationException)
		{
			var hint = CreateHint(communicationException);
			if (hint == null) // this means we did not expect this sort of exception... *yet*
			{
				return false;
			}

			var messageFromExceptions = GetExceptionsAndInnerExceptions(communicationException, false);
			Notifier.AddWarning(Res.GetString("{0EE33408-16A0-4C4B-A17C-86BBD8545B75}", "A connection error occurred whilst connecting to {0}.\r\n{1}\r\n{2}", DefaultServerAddress, hint, messageFromExceptions));
			return true;
		}

		static string GetExceptionsAndInnerExceptions(Exception exception, bool isInner)
		{
			var exceptionMessage = (isInner ? "\t" : string.Empty) +
				Res.GetString("{0EE33408-16A0-1234-1234-86BBD8545B75}", "{0}: {1}",
				exception.GetType(), exception.Message);

			return exceptionMessage + (exception.InnerException != null ? "\r\n" + GetExceptionsAndInnerExceptions(exception.InnerException, true) : string.Empty);
		}

		static string CreateHint(Exception communicationException)
		{
			if (communicationException.IsExceptionPresentIncludingInner<SocketException>())
			{
				return Res.GetString("{39A3AA42-84B5-45AD-85E8-1F759ED3C5AC}",
					"Hint: This might be due to a fault on a switch or router.");
			}

			if (communicationException.IsExceptionPresentIncludingInner<IOException>())
			{
			return Res.GetString("{7871AB1D-90F9-40D2-9821-3F469532C365}",
					"Hint: This might be due to a full HDD or incorrect permissions on the service task host.");
			}

			if (communicationException.Find<WebException>() is WebException webException)
			{
				var statusCode = (webException.Response as HttpWebResponse)?.StatusCode ?? 0;
				var tail = GetHintFromStatusCode(statusCode);
				return CreateResponseCodeDescription(statusCode) + (tail != null ? System.Environment.NewLine + tail : string.Empty);
			}

			return null;
		}

		static string GetHintFromStatusCode(HttpStatusCode statusCode)
		{
			switch (statusCode)
			{
				case HttpStatusCode.BadGateway:
					return Res.GetString("{16574818-A713-4990-8DA4-24150E725CED}",
						"Hint: This might be due to a bad gateway between the service host and the server.");
				case HttpStatusCode.Forbidden:
					return Res.GetString("{2E0393CE-2896-4E7F-8D4B-C3F7F5A101D2}",
						"Hint: This might be due to the server restarting.");
				case HttpStatusCode.Unauthorized:
					return Res.GetString("{065B5F3E-0F72-4D5E-80C9-3458AF200ABF}",
						"Hint: The password specified by the registry (eServices > eAdaptor > Outbound eAdaptor Service URL) was not accepted by the server.");
				case HttpStatusCode.GatewayTimeout:
					return Res.GetString("{ABAE5B7B-DECC-4124-AA52-77E737C97FE8}",
						"Hint: The server may be busy or there may be an issue with the network.");
			}
			return null;
		}

		static string CreateResponseCodeDescription(HttpStatusCode statusCode)
		{
			var header = Res.GetString("{16574818-A713-4990-8DA4-14150E725CED}", "HTTP Response:");
			var description = statusCode.ToString();
			return header + " " + (int)statusCode + " - " + description;
		}

		public override ICompanySettingsManager CompanySettingsManager { get; } = new eAdaptorMessagingCompanySettingsManager();
		internal override DateTime OutageStartTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		internal override bool ReportKnownExceptionsAsIssues => false;
		internal override string EndpointNotFoundMessage => LogMessages.EAdaptorEndpointNotFoundMessage;
		protected override IEHubCommunicationDiagnoster EHubCommunicationDiagnoster => DiagnosterFactory.Create(DefaultServerAddress);
	}
}

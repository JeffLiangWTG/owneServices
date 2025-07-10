using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.eHub.Adapter;
using Enterprise.eHubMessaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	abstract class eHubServiceTaskWithAdaptor : eHubServiceTask
	{
		public eHubServiceTaskWithAdaptor()
			: this(new GatewayAdaptorFactory(), new EHubCommunicationDiagnosterFactory())
		{
		}

		public eHubServiceTaskWithAdaptor(IAdaptorFactory adaptorFactory, IEHubCommunicationDiagnosterFactory diagnosterFactory)
		{
			AdaptorFactory = adaptorFactory;
			DiagnosterFactory = diagnosterFactory;
		}

		public IAdaptorFactory AdaptorFactory { get; }
		public IEHubCommunicationDiagnosterFactory DiagnosterFactory { get; }

		protected override bool ExecuteJob(IeHubServiceTaskJob job)
		{
			try
			{
				return base.ExecuteJob(job);
			}
			catch (eHubAdapterException ex)
			{
				ReportKnownException(ex);
			}
			catch (CreateAdapterException ex)
			{
				ReportKnownException(ex, (NoResString)"Adapter was null");
			}
			catch (TimeoutException ex)
			{
				ReportKnownException(ex, AppendDiagnosticsToMessage(LogMessages.TimeoutMessage, EHubCommunicationDiagnoster.Run()));
			}
			catch (ProtocolViolationException ex)
			{
				ReportKnownException(ex, LogMessages.ProtocolViolationMessage);
			}
			catch (ProtocolException ex) when (ex.InnerException is WebException webException) // do not move. must be above CommunicationException
			{
				ReportKnownException(ex, webException.Message);
			}
			catch (ProtocolException ex) // do not move. must be above CommunicationException
			{
				ReportKnownException(ex, (NoResString)"Protocal Error Occurred");
			}
			catch (ArgumentException ex) when (IsInvalidSchemeException(ex, out var expectedScheme, out var providedScheme))
			{
				ReportKnownException(ex, LogMessages.InvalidScheme(expectedScheme, providedScheme));
			}
			catch (ServerTooBusyException ex) // do not move. must be above CommunicationException
			{
				ReportKnownException(ex, LogMessages.ServerTooBusyMessage);
			}
			catch (EndpointNotFoundException ex) // do not move. must be above CommunicationException
			{
				ReportKnownException(ex, AppendDiagnosticsToMessage(EndpointNotFoundMessage, EHubCommunicationDiagnoster.Run()));
			}
			catch (ServiceActivationException ex) // do not move. must be above CommunicationException
			{
				ReportKnownException(ex);
			}
			catch (CommunicationException ex) when (HandleCommunicationException(ex))
			{
			}
			catch (UriFormatException ex)
			{
				ReportKnownException(ex, LogMessages.BadUriMessage);
			}
			return false;
		}

		protected virtual bool HandleCommunicationException(CommunicationException ex)
		{
			if (IsUnknownCommunicationException(ex))
			{
				return false;
			}

			ReportKnownException(ex, AppendDiagnosticsToMessage(LogMessages.TimeoutMessage, EHubCommunicationDiagnoster.Run()));
			return true;
		}

		protected virtual IEHubCommunicationDiagnoster EHubCommunicationDiagnoster => DiagnosterFactory.Create($"https://{DefaultServerAddress}/eHubGateway/eHubStreamedService.svc?wsdl");

		static bool IsUnknownCommunicationException(CommunicationException ex)
		{
			return !ex.IsExceptionPresentIncludingInner<WebException>() && !ex.IsExceptionPresentIncludingInner<IOException>() && !ex.IsExceptionPresentIncludingInner<SocketException>();
		}

		static string AppendDiagnosticsToMessage(string message, string diagnostics)
		{
			var messageToLog = new StringBuilder();
			messageToLog.Append(message);

			if (!string.IsNullOrWhiteSpace(diagnostics))
			{
				messageToLog.AppendLine();
				messageToLog.Append(diagnostics);
			}

			return messageToLog.ToString();
		}

		static bool IsInvalidSchemeException(ArgumentException ex, out string expectedScheme, out string providedScheme)
		{
			var match = Regex.Match(ex.Message, @"The provided URI scheme '(.+?)' is invalid; expected '(.+?)'\.");
			providedScheme = match.Success ? match.Groups[1].Value : null;
			expectedScheme = match.Success ? match.Groups[2].Value : null;
			return match.Success;
		}
	}
}

using CargoWise.Types;

namespace Enterprise.eHubMessaging.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const Variable for Service task")]
	public static class ServiceTaskNames
	{
		public const string EHubOutboundMessages = "eHub Outbound Messages";
		public const string EHubInboundMessages = "eHub Inbound Messages";
		public const string EAdaptorOutboundMessages = "eAdaptor Outbound Messages";
		public const string EHubStatusMessageProcessorName = "eHub Status Message Processor";
		public const string EServicesHealthCheck = "eServices Health Check";
		public const string UsageSubmissionService = "Usage Submission Service";
		public const string UsageSubmissionFailureResetter = "Usage Submission Failure Re-setter";
		public const string ODPLCollectorService = "ODPL Collector Service";
		public const string TestEHubInboundMessages = "eHub Test Inbound Messages";
	}

	public static class ServiceTaskCodes
	{
		public const string EHI = "EHI";
		public const string EHO = "EHO";
		public const string EAM = "EAM";
		public const string EHC = "EHC";
		public const string USS = "USS";
		public const string USF = "USF";
		public const string OCS = "OCS";
		public const string THI = "THI";
	}

	public static class LogMessages
	{
		public static string TimeoutMessage
		{
			get
			{
				return Res.GetString("4b858c93-9444-4a17-a7a6-d1d577c27700", "Communication with the server has timed out - The service task will reattempt the transfer on the next run. You do not need to take action unless this timeout has occurred frequently over an extended period of time. The problem may have occurred because of insufficient network bandwidth or eHub was under load and took too long to respond.");
			}
		}

		public static string ProtocolViolationMessage
		{
			get
			{
				return Res.GetString("266c3eaf-40c4-4cfa-b129-24fe0c3057eb", "Communication over HTTP 1.0 protocol not supported.");
			}
		}

		public static string LoginFailureMessage
		{
			get
			{
				return Res.GetString("9a8048f6-96a4-44fb-a76e-0a72666c3493", "Login failed.");
			}
		}

		public static string ServerTooBusyMessage
		{
			get
			{
				return Res.GetString("3026810d-d49e-469f-a894-5e21e13ee671", "The server is currently too busy. The service task will reattempt the transfer on the next run. You do not need to take action unless the server remains unavailable for an extended period of time.");
			}
		}

		public static string EHubEndpointNotFoundMessage
		{
			get
			{
				return Res.GetString("28e111df-de57-476c-8685-10a3d593c0c9", "The server is currently down for maintenance. The service task will reattempt the transfer on the next run. You do not need to take action unless the server remains offline for an extended period of time.");
			}
		}

		public static string EAdaptorEndpointNotFoundMessage
		{
			get
			{
				return Res.GetString("c1dd4178-926f-4cb0-8450-45333180f116", "The server is currently offline or the URL is configured incorrectly in the registry (eServices > eAdaptor > Outbound eAdaptor Service URL). The service task will reattempt the transfer on the next run. You do not need to take action unless the server remains offline for an extended period of time.");
			}
		}

		public static string BadUriMessage
		{
			get
			{
				return Res.GetString("d47a4914-b6aa-4b30-821a-418b3c89f818", "The URI entered in the registry is not in the correct format. A connection cannot be made.");
			}
		}

		public static string SaveInterchangeFailed
		{
			get
			{
				return Res.GetString("6510e983-f2f6-4b0b-9885-eaca1f12298b", "Failed to insert/update the Interchange to the database. The Interchange will be downloaded again next service task run.");
			}
		}

		public static string InvalidScheme(string expectedScheme, string providedScheme)
		{
			return Res.GetString("ebae669e-870b-4114-8d96-134d4bd20b60", "The provided URI scheme '{0}' is invalid; expected '{1}'.", providedScheme, expectedScheme);
		}

		public static string FailedInterchangesReportMessage(int count, ZDateTime lastReportedTimePointForFailedInterchanges, ZDateTime currentTime)
		{
			return Res.GetString("abad159a-2ea7-4b01-9f9d-7d7a7b92aadf", "{0} failed Interchange(s) found between {1} and {2}.", count, lastReportedTimePointForFailedInterchanges, currentTime);
		}

		public static string FailedInterchangesReportMessageForCompany(string companyLicenceKeyIdentifier, int count, ZDateTime lastReportedTimePointForFailedInterchanges, ZDateTime currentTime)
		{
			return Res.GetString("85b851ab-83d3-48e7-a218-3be7941f62a9", "{0} failed Interchange(s) found for company {1} between {2} and {3}.", count, companyLicenceKeyIdentifier, lastReportedTimePointForFailedInterchanges, currentTime);
		}

		public static string RegistrationException
		{
			get
			{
				return Res.GetString("d88adecb-9baa-469e-9218-82a00a4a6e64", "Unexpected response from Client Registration Request");
			}
		}
	}

	public static class MutexConstants
	{
		public static string MessageMutexPrefix
		{
			get { return "ESV_"; }
		}
	}
}

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Enterprise.Server.Setup
{
	static class WindowsFirewallWrapper
	{
		public static void AddAuthorizedApplication(string friendlyName, string executablePath)
		{
			INetFwMgr manager = (INetFwMgr)new NetFwMgr();
			INetFwProfile profile = manager.LocalPolicy.CurrentProfile;
			INetFwAuthorizedApplication app = (INetFwAuthorizedApplication)new NetFwAuthorizedApplication();
			app.ProcessImageFileName = executablePath;
			app.Name = friendlyName;
			app.Scope = NET_FW_SCOPE_.NET_FW_SCOPE_ALL;
			app.IpVersion = NET_FW_IP_VERSION_.NET_FW_IP_VERSION_ANY;
			app.Enabled = true;
			profile.AuthorizedApplications.Add(app);
		}

		#region NetFwTypeLib

		#region Classes

		#region NetFwAuthorizedApplication

		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		[ComImport, ComVisible(false), Guid("EC9846B3-2762-4A6B-A214-6ACB603462D2")]
		class NetFwAuthorizedApplication
		{
		}

		#endregion

		#region NetFwMgr

		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		[ComImport, ComVisible(false), Guid("304CE942-6E39-40D8-943A-B913C40C9CD4")]
		class NetFwMgr
		{
		}

		#endregion

		#endregion

		#region Enums

		#region NET_FW_IP_PROTOCOL_

		enum NET_FW_IP_PROTOCOL_
		{
			NET_FW_IP_PROTOCOL_TCP = 6,
			NET_FW_IP_PROTOCOL_UDP = 17
		}

		#endregion

		#region NET_FW_IP_VERSION_

		enum NET_FW_IP_VERSION_
		{
			NET_FW_IP_VERSION_V4 = 0,
			NET_FW_IP_VERSION_V6 = 1,
			NET_FW_IP_VERSION_ANY = 2,
			NET_FW_IP_VERSION_MAX = 3
		}

		#endregion

		#region NET_FW_PROFILE_TYPE_

		enum NET_FW_PROFILE_TYPE_
		{
			NET_FW_PROFILE_DOMAIN = 0,
			NET_FW_PROFILE_STANDARD = 1,
			NET_FW_PROFILE_CURRENT = 2,
			NET_FW_PROFILE_TYPE_MAX = 3
		}

		#endregion

		#region NET_FW_SCOPE_

		enum NET_FW_SCOPE_
		{
			NET_FW_SCOPE_ALL = 0,
			NET_FW_SCOPE_LOCAL_SUBNET = 1,
			NET_FW_SCOPE_CUSTOM = 2,
			NET_FW_SCOPE_MAX = 3
		}

		#endregion

		#region NET_FW_SERVICE_TYPE_

		enum NET_FW_SERVICE_TYPE_
		{
			NET_FW_SERVICE_FILE_AND_PRINT = 0,
			NET_FW_SERVICE_UPNP = 1,
			NET_FW_SERVICE_REMOTE_DESKTOP = 2,
			NET_FW_SERVICE_NONE = 3,
			NET_FW_SERVICE_TYPE_MAX = 4
		}

		#endregion

		#endregion

		#region Interfaces

		#region INetFwAuthorizedApplication

		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		[ComImport, ComVisible(false), Guid("B5E64FFA-C2C5-444E-A301-FB5E00018050"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
		interface INetFwAuthorizedApplication
		{
			string Name { get; set; }
			string ProcessImageFileName { get; set; }
			NET_FW_IP_VERSION_ IpVersion { get; set; }
			NET_FW_SCOPE_ Scope { get; set; }
			string RemoteAddresses { get; set; }
			bool Enabled { get; set; }
		}

		#endregion

		#region INetFwAuthorizedApplications

		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		[ComImport, ComVisible(false), Guid("644EFD52-CCF9-486C-97A2-39F352570B30"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
		interface INetFwAuthorizedApplications
		{
			long Count { get; }
			void Add(INetFwAuthorizedApplication port);
			void Remove(string imageFileName);
			INetFwAuthorizedApplication Item(string imageFileName);
			IEnumerator _NewEnum { get; }
		}

		#endregion

		#region INetFwIcmpSettings

		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		[ComImport, ComVisible(false), Guid("A6207B2E-7CDD-426A-951E-5E1CBC5AFEAD"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
		interface INetFwIcmpSettings
		{
			bool AllowOutboundDestinationUnreachable { get; set; }
			bool AllowRedirect { get; set; }
			bool AllowInboundEchoRequest { get; set; }
			bool AllowOutboundTimeExceeded { get; set; }
			bool AllowOutboundParameterProblem { get; set; }
			bool AllowOutboundSourceQuench { get; set; }
			bool AllowInboundRouterRequest { get; set; }
			bool AllowInboundTimestampRequest { get; set; }
			bool AllowInboundMaskRequest { get; set; }
			bool AllowOutboundPacketTooBig { get; set; }
		}

		#endregion

		#region INetFwMgr

		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		[ComImport, ComVisible(false), Guid("F7898AF5-CAC4-4632-A2EC-DA06E5111AF2"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
		interface INetFwMgr
		{
			INetFwPolicy LocalPolicy { get; }
			NET_FW_PROFILE_TYPE_ CurrentProfileType { get; }
			void RestoreDefaults();
			void IsPortAllowed(string imageFileName,
				NET_FW_IP_VERSION_ ipVersion,
				long portNumber,
				string localAddress,
				NET_FW_IP_PROTOCOL_ ipProtocol,
				[Out] out bool allowed,
				[Out] out bool restricted);
			void IsIcmpTypeAllowed(NET_FW_IP_VERSION_ ipVersion,
				string localAddress,
				byte type,
				[Out] out bool allowed,
				[Out] out bool restricted);
		}

		#endregion

		#region INetFwOpenPort

		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		[ComImport, ComVisible(false), Guid("E0483BA0-47FF-4D9C-A6D6-7741D0B195F7"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
		interface INetFwOpenPort
		{
			string Name { get; set; }
			NET_FW_IP_VERSION_ IpVersion { get; set; }
			NET_FW_IP_PROTOCOL_ Protocol { get; set; }
			long Port { get; set; }
			NET_FW_SCOPE_ Scope { get; set; }
			string RemoteAddresses { get; set; }
			bool Enabled { get; set; }
			bool BuiltIn { get; }
		}

		#endregion

		#region INetFwOpenPorts

		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		[ComImport, ComVisible(false), Guid("C0E9D7FA-E07E-430A-B19A-090CE82D92E2"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
		interface INetFwOpenPorts
		{
			long Count { get; }
			void Add(INetFwOpenPort port);
			void Remove(long portNumber, NET_FW_IP_PROTOCOL_ ipProtocol);
			INetFwOpenPort Item(long portNumber, NET_FW_IP_PROTOCOL_ ipProtocol);
			IEnumerator _NewEnum { get; }
		}

		#endregion

		#region INetFwPolicy

		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		[ComImport, ComVisible(false), Guid("D46D2478-9AC9-4008-9DC7-5563CE5536CC"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
		interface INetFwPolicy
		{
			INetFwProfile CurrentProfile { get; }
			INetFwProfile GetProfileByType(NET_FW_PROFILE_TYPE_ profileType);
		}

		#endregion

		#region INetFwProfile

		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		[ComImport, ComVisible(false), Guid("174A0DDA-E9F9-449D-993B-21AB667CA456"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
		interface INetFwProfile
		{
			NET_FW_PROFILE_TYPE_ Type { get; }
			bool FirewallEnabled { get; set; }
			bool ExceptionsNotAllowed { get; set; }
			bool NotificationsDisabled { get; set; }
			bool UnicastResponsesToMulticastBroadcastDisabled { get; set; }
			INetFwRemoteAdminSettings RemoteAdminSettings { get; }
			INetFwIcmpSettings IcmpSettings { get; }
			INetFwOpenPorts GloballyOpenPorts { get; }
			INetFwServices Services { get; }
			INetFwAuthorizedApplications AuthorizedApplications { get; }
		}

		#endregion

		#region INetFwRemoteAdminSettings

		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		[ComImport, ComVisible(false), Guid("D4BECDDF-6F73-4A83-B832-9C66874CD20E"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
		interface INetFwRemoteAdminSettings
		{
			NET_FW_IP_VERSION_ IpVersion { get; set; }
			NET_FW_SCOPE_ Scope { get; set; }
			string RemoteAddresses { get; set; }
			bool Enabled { get; set; }
		}

		#endregion

		#region INetFwService

		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		[ComImport, ComVisible(false), Guid("79FD57C8-908E-4A36-9888-D5B3F0A444CF"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
		interface INetFwService
		{
			string Name { get; }
			NET_FW_SERVICE_TYPE_ Type { get; }
			bool Customized { get; }
			NET_FW_IP_VERSION_ IpVersion { get; set; }
			NET_FW_SCOPE_ Scope { get; set; }
			string RemoteAddresses { get; set; }
			bool Enabled { get; set; }
			INetFwOpenPorts GloballyOpenPorts { get; }
		}

		#endregion

		#region INetFwServices

		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		[ComImport, ComVisible(false), Guid("79649BB4-903E-421B-94C9-79848E79F6EE"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
		interface INetFwServices
		{
			long Count { get; }
			INetFwService Item(NET_FW_SERVICE_TYPE_ svcType);
			IEnumerator _NewEnum { get; }
		}

		#endregion

		#endregion

		#endregion
	}
}

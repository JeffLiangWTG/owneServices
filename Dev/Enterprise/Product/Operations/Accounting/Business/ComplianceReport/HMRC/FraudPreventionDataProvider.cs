using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	public class FraudPreventionDataProvider
	{
		internal List<KeyValuePair<string, string>> Get()
		{
			var result = new List<KeyValuePair<string, string>>();
			AddConnectionMethod(result);
			AddSystemManagementData(result);
			AddNetworkData(result);
			AddScreenData(result);
			AddSoftwareData(result);
			return result;
		}

		void AddConnectionMethod(List<KeyValuePair<string, string>> result)
		{
			result.Add(new KeyValuePair<string, string>("Gov-Client-Connection-Method", "DESKTOP_APP_DIRECT"));
		}

		void AddSystemManagementData(List<KeyValuePair<string, string>> result)
		{
			#region SuppressResourceStringsCheckRegion

			using (var searcherProxy = GetManagementObjectSearcherProxy("select * from Win32_ComputerSystemProduct"))
			{
				var computerSystemProduct = searcherProxy.Get().SingleOrDefault();
				if (computerSystemProduct != null)
				{
					var uuid = computerSystemProduct["UUID"].ToString();
					result.Add(new KeyValuePair<string, string>("Gov-Client-Device-ID", uuid));
				}
			}

			var manufacturer = string.Empty;
			var model = string.Empty;
			using (var searcherProxy = GetManagementObjectSearcherProxy("select * from Win32_ComputerSystem"))
			{
				var computerSystem = searcherProxy.Get().SingleOrDefault();
				if (computerSystem != null)
				{
					if (computerSystem["CurrentTimeZone"] != null)
					{
						var timeZone = (short)computerSystem["CurrentTimeZone"];
						result.Add(new KeyValuePair<string, string>("Gov-Client-Timezone", FormattableString.Invariant($"UTC{(timeZone < 0 ? "-" : "+")}{Math.Abs(timeZone / 60).ToString("D2", CultureInfo.InvariantCulture)}:{Math.Abs(timeZone % 60).ToString("D2", CultureInfo.InvariantCulture)}")));
					}

					manufacturer = EmptyIfDefaultStringOrNull(computerSystem["Manufacturer"]?.ToString());
					model = EmptyIfDefaultStringOrNull(computerSystem["Model"]?.ToString());
				}
			}

			using (var searcherProxy = GetManagementObjectSearcherProxy("select * from Win32_OperatingSystem"))
			{
				var operatingSystem = searcherProxy.Get().SingleOrDefault();
				if (operatingSystem != null)
				{
					var name = EmptyIfDefaultStringOrNull(operatingSystem["Caption"]?.ToString());
					var version = EmptyIfDefaultStringOrNull(operatingSystem["Version"]?.ToString());
					result.Add(new KeyValuePair<string, string>("Gov-Client-User-Agent",
						FormattableString.Invariant($"os-family={Uri.EscapeDataString(name)}&os-version={Uri.EscapeDataString(version)}&device-manufacturer={Uri.EscapeDataString(manufacturer)}&device-model={Uri.EscapeDataString(model)}")));
				}
			}

			#endregion
		}

		void AddNetworkData(List<KeyValuePair<string, string>> result)
		{
			#region SuppressResourceStringsCheckRegion

			var networkInterfaceProxies = GetAllNetworkInterfaceProxies()
				.Where(x => x.OperationalStatus == OperationalStatus.Up && x.NetworkInterfaceType != NetworkInterfaceType.Loopback
					&& x.GetIPProperties().UnicastAddresses.Any(ip => IsLocalIP(ip.Address.GetAddressBytes())))
				.ToArray();
			if (networkInterfaceProxies.Any())
			{
				var ipAddresses = networkInterfaceProxies.SelectMany(networkInterface => networkInterface.GetIPProperties().UnicastAddresses).Where(ip => IsLocalIP(ip.Address.GetAddressBytes()));
				var ipAddressesText = string.Join(",", ipAddresses.Select(ipAddress => ipAddress.Address.ToString()));
				result.Add(new KeyValuePair<string, string>("Gov-Client-Local-IPs", ipAddressesText));

				result.Add(new KeyValuePair<string, string>("Gov-Client-Local-IPs-Timestamp", ZDateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss.fffZ")));

				var macAddressesText = string.Join(",", networkInterfaceProxies
					.Select(networkInterface => Uri.EscapeDataString((BitConverter.ToString(networkInterface.GetPhysicalAddress().GetAddressBytes()).Replace("-", ":")))));
				result.Add(new KeyValuePair<string, string>("Gov-Client-MAC-Addresses", macAddressesText));
			}

			bool IsLocalIP(byte[] addressBytes) =>
				addressBytes.Length == 4
				&& (IsInAddressRange(addressBytes, classA_Min, classA_Max)
					|| IsInAddressRange(addressBytes, classB_Min, classB_Max)
					|| IsInAddressRange(addressBytes, classC_Min, classC_Max)
					|| IsInAddressRange(addressBytes, special_Min, special_Max));

			bool IsInAddressRange(byte[] address, byte[] minAddress, byte[] maxAddress) =>
				address[0] >= minAddress[0] && address[0] <= maxAddress[0]
				&& address[1] >= minAddress[1] && address[1] <= maxAddress[1]
				&& address[2] >= minAddress[2] && address[2] <= maxAddress[2]
				&& address[3] >= minAddress[3] && address[3] <= maxAddress[3];

			#endregion
		}

		#region Local IP Addresses detection

		readonly byte[] classA_Min = new byte[] { 10, 0, 0, 0 };
		readonly byte[] classA_Max = new byte[] { 10, 255, 255, 255 };

		readonly byte[] classB_Min = new byte[] { 172, 16, 0, 0 };
		readonly byte[] classB_Max = new byte[] { 172, 31, 255, 255 };

		readonly byte[] classC_Min = new byte[] { 192, 168, 0, 0 };
		readonly byte[] classC_Max = new byte[] { 192, 168, 255, 255 };

		readonly byte[] special_Min = new byte[] { 100, 64, 0, 0 };
		readonly byte[] special_Max = new byte[] { 100, 127, 255, 255 };

		#endregion

		void AddScreenData(List<KeyValuePair<string, string>> result)
		{
			#region SuppressResourceStringsCheckRegion

			var currentFormProxy = ApplicationOpenFormProxies.FirstOrDefault(x => x.ContainsFocus) ?? ApplicationOpenFormProxies.FirstOrDefault();
			if (currentFormProxy != null)
			{
				result.Add(new KeyValuePair<string, string>("Gov-Client-Window-Size", FormattableString.Invariant($"width={currentFormProxy.Width}&height={currentFormProxy.Height}")));
			}

			string getDpi(uint dpi) => (dpi % 96) == 0 ? (dpi / 96).ToString(CultureInfo.InvariantCulture) : (dpi / 96.0).ToString("G");

			var screens = string.Join(",", CachedScreenInfoProxyInstance.Screens.Select(screen => FormattableString.Invariant($"width={screen.WorkingArea.Width}&height={screen.WorkingArea.Height}&scaling-factor={getDpi(screen.DpiX)}&colour-depth={screen.Depth}")));
			result.Add(new KeyValuePair<string, string>("Gov-Client-Screens", screens));

			#endregion
		}

		void AddSoftwareData(List<KeyValuePair<string, string>> result)
		{
			#region SuppressResourceStringsCheckRegion

			var softwareDataProxy = SoftwareDataProxy;
			result.Add(new KeyValuePair<string, string>("Gov-Client-User-IDs", "os=" + softwareDataProxy.CurrentUser.Initials));

			var productNameUrlEncoded = Uri.EscapeDataString(softwareDataProxy.ProductName);

			using (var sha256 = SHA256.Create())
			{
				if (softwareDataProxy.CurrentUser.IsTwoFactorAuthenticationEnabled)
				{
					var bytes = softwareDataProxy.CurrentUser.PK.ToByteArray().Concat(Encoding.Unicode.GetBytes(softwareDataProxy.CurrentUser.EmailAddress)).ToArray();
					var hash = BitConverter.ToString(sha256.ComputeHash(bytes).Take(8).ToArray()).Replace("-", string.Empty);

					result.Add(new KeyValuePair<string, string>("Gov-Client-Multi-Factor",
						FormattableString.Invariant($"type=OTHER&timestamp={Uri.EscapeDataString(softwareDataProxy.CurrentUser.LastActivityDateTimeUtc.ToString("yyyy-MM-ddThh:mm:ss.fffZ"))}&unique-reference={hash}")));
				}

				var licenseIdBytes = Encoding.Unicode.GetBytes(FormattableString.Invariant($"{GlbCompany.CurrentCompany.LicenceEnterpriseCode}-{GlbCompany.CurrentCompany.LicenceServerID}-{GlbCompany.CurrentCompany.GC_Code}")).ToArray();
				var licenseIdHash = BitConverter.ToString(sha256.ComputeHash(licenseIdBytes).ToArray()).Replace("-", string.Empty);
				result.Add(new KeyValuePair<string, string>("Gov-Vendor-License-IDs", FormattableString.Invariant($"{productNameUrlEncoded}={licenseIdHash}")));
			}

			var versionNumberUrlEncoded = Uri.EscapeDataString(softwareDataProxy.VersionNumber.ToString());

			result.Add(new KeyValuePair<string, string>("Gov-Vendor-Product-Name", productNameUrlEncoded));
			result.Add(new KeyValuePair<string, string>("Gov-Vendor-Version", FormattableString.Invariant($"{productNameUrlEncoded}={versionNumberUrlEncoded}")));

			#endregion
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hardcoded value to compare with to check if there is a meaningful value")]
		string EmptyIfDefaultStringOrNull(string value) => value == null || value == "Default string" ? string.Empty : value;

		internal virtual IManagementObjectSearcherProxy GetManagementObjectSearcherProxy(string queryString) => new ManagementObjectSearcherProxyImpl(queryString);

		internal virtual INetworkInterfaceProxy[] GetAllNetworkInterfaceProxies() => NetworkInterface.GetAllNetworkInterfaces().Select(networkInterface => new NetworkInterfaceProxyImpl(networkInterface)).ToArray();

		internal virtual ICachedScreenInfoProxy CachedScreenInfoProxyInstance => cachedScreenInfoProxyInstance ?? (cachedScreenInfoProxyInstance = new CachedScreenInfoProxyImpl());
		ICachedScreenInfoProxy cachedScreenInfoProxyInstance;

		internal virtual IEnumerable<IFormProxy> ApplicationOpenFormProxies => ObjectFactory.Get<IFormProxy>().GetApplicationOpenFormProxies();

		internal virtual ISoftwareDataProxy SoftwareDataProxy => softwareData ?? (softwareData = new SoftwareDataProxyImpl());
		ISoftwareDataProxy softwareData;

		internal virtual float DpiScaling
		{
			get
			{
				using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
				{
					return graphics.DpiX / 96;
				}
			}
		}

		#region FraudPreventionDataProvider internal interfaces

		#region For AddSystemManagementData

		internal interface IManagementObjectSearcherProxy : IDisposable
		{
			IEnumerable<IManagementObjectProxy> Get();
		}

		internal interface IManagementObjectProxy
		{
			object this[string propertyName] { get; }
		}

		#endregion

		#region For AddNetworkData

		internal interface INetworkInterfaceProxy
		{
			OperationalStatus OperationalStatus { get; }
			NetworkInterfaceType NetworkInterfaceType { get; }
			IIPInterfacePropertiesProxy GetIPProperties();
			PhysicalAddress GetPhysicalAddress();
		}

		internal interface IIPInterfacePropertiesProxy
		{
			IEnumerable<IUnicastIPAddressInformationProxy> UnicastAddresses { get; }
		}

		internal interface IUnicastIPAddressInformationProxy
		{
			IPAddress Address { get; }
			IPAddress IPv4Mask { get; }
		}

		#endregion

		#region For AddScreenData

		public interface IFormProxy
		{
			bool ContainsFocus { get; }
			int Width { get; }
			int Height { get; }
			IEnumerable<IFormProxy> GetApplicationOpenFormProxies();
		}

		internal interface ICachedScreenInfoProxy
		{
			IEnumerable<IPhysicalScreenInfo> Screens { get; }
		}

		#endregion

		#region For AddSoftwareData

		internal interface ISoftwareDataProxy
		{
			IUser CurrentUser { get; }
			string ProductName { get; }
			VersionNumber VersionNumber { get; }
		}

		#endregion

		#endregion

		#region FraudPreventionDataProvider implementation of internal interfaces

		#region For AddSystemManagementData

		sealed class ManagementObjectSearcherProxyImpl : IManagementObjectSearcherProxy
		{
			ManagementObjectSearcher managementObjectSearcher;

			public ManagementObjectSearcherProxyImpl(string query)
			{
				managementObjectSearcher = new ManagementObjectSearcher(query);
			}

			public void Dispose()
			{
				managementObjectSearcher.Dispose();
				managementObjectSearcher = null;
			}

			public IEnumerable<IManagementObjectProxy> Get() => managementObjectSearcher?.Get().Cast<ManagementBaseObject>().Select(managementObject => new ManagementObjectProxyImpl(managementObject));
		}

		sealed class ManagementObjectProxyImpl : IManagementObjectProxy
		{
			readonly ManagementBaseObject managementObject;

			public ManagementObjectProxyImpl(ManagementBaseObject managementObject)
			{
				this.managementObject = managementObject;
			}

			public object this[string propertyName] => managementObject[propertyName];
		}

		#endregion

		#region For AddNetworkData

		sealed class NetworkInterfaceProxyImpl : INetworkInterfaceProxy
		{
			readonly NetworkInterface networkInterface;

			IIPInterfacePropertiesProxy ipInterfacePropertiesProxy;

			public NetworkInterfaceProxyImpl(NetworkInterface networkInterface)
			{
				this.networkInterface = networkInterface;
			}

			public OperationalStatus OperationalStatus => networkInterface.OperationalStatus;

			public NetworkInterfaceType NetworkInterfaceType => networkInterface.NetworkInterfaceType;

			public IIPInterfacePropertiesProxy GetIPProperties() => ipInterfacePropertiesProxy ?? (ipInterfacePropertiesProxy = new IPInterfacePropertiesProxyImpl(networkInterface.GetIPProperties()));

			public PhysicalAddress GetPhysicalAddress() => networkInterface.GetPhysicalAddress();
		}

		sealed class IPInterfacePropertiesProxyImpl : IIPInterfacePropertiesProxy
		{
			readonly IPInterfaceProperties ipInterfaceProperties;

			public IPInterfacePropertiesProxyImpl(IPInterfaceProperties ipInterfaceProperties)
			{
				this.ipInterfaceProperties = ipInterfaceProperties;
			}

			public IEnumerable<IUnicastIPAddressInformationProxy> UnicastAddresses => ipInterfaceProperties.UnicastAddresses.Select(ipInterfaceProperty => new UnicastIPAddressInformationProxyImpl(ipInterfaceProperty));
		}

		sealed class UnicastIPAddressInformationProxyImpl : IUnicastIPAddressInformationProxy
		{
			readonly UnicastIPAddressInformation unicastIPAddressInformation;

			public UnicastIPAddressInformationProxyImpl(UnicastIPAddressInformation unicastIPAddressInformation)
			{
				this.unicastIPAddressInformation = unicastIPAddressInformation;
			}

			public IPAddress Address => unicastIPAddressInformation.Address;
			public IPAddress IPv4Mask => unicastIPAddressInformation.IPv4Mask;
		}

		#endregion

		#region For AddScreenData

		sealed class CachedScreenInfoProxyImpl : ICachedScreenInfoProxy
		{
			public IEnumerable<IPhysicalScreenInfo> Screens => ObjectFactory.Get<ICachedScreenInfo>().Screens;
		}

		#endregion

		#region For AddSoftwareData

		sealed class SoftwareDataProxyImpl : ISoftwareDataProxy
		{
			public IUser CurrentUser => Environment.Env.CurrentUser;

			public string ProductName => BrandingFactory.Instance.ProductName;

			public VersionNumber VersionNumber => ReleaseInfo.Instance.VersionNumber;
		}

		#endregion

		#endregion
	}
}

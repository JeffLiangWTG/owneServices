using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Business
{
	[CodeProperty(AutoStmServiceHost.Schema.SH_HostName)]
	[DescriptionProperty(AutoStmServiceHost.Schema.SH_HostName)]
	public class StmServiceHost : AutoStmServiceHost
	{
		public StmServiceHost(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public abstract new class Schema : AutoStmServiceHost.Schema
		{
			public const string StatusDescription = nameof(StatusDescription);
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			InitializeProxyAuthentication();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			SH_HostName = "DefaultServiceHostName";
			SH_ProxyAutoDetect = true;

			InitializeProxyAuthentication();
		}

		#region Web Proxy

		protected bool SH_ProxyHost_ReadOnly { get { return SH_ProxyAutoDetect; } }
		protected bool SH_ProxyPort_ReadOnly { get { return SH_ProxyAutoDetect; } }
		protected bool SH_ProxyAuthentication_ReadOnly { get { return SH_ProxyAutoDetect; } }
		protected bool SH_ProxyUserName_ReadOnly { get { return SH_ProxyAutoDetect || !proxyAuthentication; } }
		protected bool SH_ProxyPassword_ReadOnly { get { return SH_ProxyAutoDetect || !proxyAuthentication; } }
		protected bool ProxyBypassOnLocal_ReadOnly { get { return SH_ProxyAutoDetect; } }
		protected bool ProxyBypassList_ReadOnly { get { return SH_ProxyAutoDetect; } }

		void InitializeProxyAuthentication()
		{
			proxyAuthentication = !SH_ProxyAutoDetect && !SH_ProxyUserName.IsEmpty;
		}

		#region SH_ProxyAutoDetect

		public override ZBool SH_ProxyAutoDetect
		{
			get
			{
				return base.SH_ProxyAutoDetect;
			}
			set
			{
				if (base.SH_ProxyAutoDetect != value)
				{
					base.SH_ProxyAutoDetect = value;

					if (value)
					{
						SH_ProxyHost = "";
						SH_ProxyPort = 0;
						SH_ProxyAuthentication = false;
					}
				}
			}
		}

		#endregion

		#region SH_ProxyAuthentication

		ZBool proxyAuthentication;

		public virtual ZBool SH_ProxyAuthentication
		{
			get
			{
				return proxyAuthentication;
			}
			set
			{
				if (proxyAuthentication != value)
				{
					SetNonPersistentPropertyValue(SH_ProxyAuthenticationInfo, ref proxyAuthentication, value);
					if (!value)
					{
						SH_ProxyUserName = "";
						SH_ProxyPassword = "";
					}
				}
			}
		}

		public virtual ZPropertyInfo SH_ProxyAuthenticationInfo
		{
			get
			{
				return this.GetZPropertyInfo(nameof(SH_ProxyAuthentication));
			}
		}

		#endregion

		#region ProxyBypassOnLocal

		public ZBool ProxyBypassOnLocal
		{
			get
			{
				EnsureLoadBypassData();
				return bypassData != null ? bypassData.SD_IsLogged : ZBool.True;
			}
			set
			{
				if (value != ProxyBypassOnLocal)
				{
					EnsureBypassData();
					bypassData.SD_IsLogged = value;
					HasChanges = true;
					ProxyBypassOnLocalInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ProxyBypassOnLocalInfo
		{
			get { return GetZPropertyInfo(nameof(ProxyBypassOnLocal)); }
		}

		#endregion

		#region ProxyBypassList

		const int ProxyBypassListMaxLength = 255;

		[ResourceStringData("A5867693-5160-4644-8441-B99FBBA491D9", Caption = "Proxy Bypass List")]
		[CargoWise.ComponentModel.MaxLength(ProxyBypassListMaxLength)]
		public ZString ProxyBypassList
		{
			get
			{
				EnsureLoadBypassData();
				return bypassData != null ? bypassData.SD_BinaryValue.ToUTF8() : ZString.Empty;
			}
			set
			{
				if (value != ProxyBypassList)
				{
					CheckMaximumLength(ProxyBypassListInfo, value);
					EnsureBypassData();
					bypassData.SD_BinaryValue = ZBlob.FromUTF8(value);
					HasChanges = true;
					ProxyBypassListInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ProxyBypassListInfo
		{
			get { return GetZPropertyInfo(nameof(ProxyBypassList)); }
		}

		const string BypassDataName = "StmServiceHost.BypassData";

		void EnsureBypassData()
		{
			EnsureLoadBypassData();
			if (bypassData == null)
			{
				bypassData = Factory.New<StmData>();
				bypassData.SD_Name = BypassDataName;
				bypassData.SD_Owner = PK;
				bypassData.SD_IsLogged = true;
			}
		}

		void EnsureLoadBypassData()
		{
			if (!bypassDataLoaded)
			{
				bypassDataLoaded = true;
				LoadBypassData();
			}
		}

		void LoadBypassData()
		{
			var query = new ZQuery(StmDataSchema.SD_Name, BypassDataName);
			query.AddToFilter(StmDataSchema.SD_Owner, PK);
			bypassData = Factory.LoadTop1<StmData>(query);
		}

		public override void Delete()
		{
			base.Delete();
			if (bypassData != null)
			{
				bypassData.Delete();
				bypassData = null;
			}
		}

		StmData bypassData;
		bool bypassDataLoaded;

		#endregion

		public IWebProxy GetWebProxy()
		{
			IWebProxy result;

			if (SH_ProxyAutoDetect)
			{
				result = WebRequest.DefaultWebProxy;
			}
			else if (SH_ProxyHost.IsEmpty)
			{
				result = new WebProxy();
			}
			else
			{
				var proxy = new WebProxy(SH_ProxyHost, SH_ProxyPort);
				result = proxy;
				if (SH_ProxyAuthentication)
				{
					result.Credentials = new NetworkCredential(SH_ProxyUserName, SH_ProxyPassword);
				}

				proxy.BypassProxyOnLocal = ProxyBypassOnLocal;
				var bypassList = ProxyBypassList;
				if (!bypassList.IsEmpty)
				{
					proxy.BypassList = ConvertBypassListToRegex(bypassList);
				}
			}
			return result;
		}

		static string[] ConvertBypassListToRegex(string bypassList)
		{
			var addresses = bypassList.Split(";".ToCharArray());
			var patterns = new List<string>();
			foreach (var address in addresses)
			{
				var pattern = ConvertBypassAddressToRegex(address);
				if (!string.IsNullOrEmpty(pattern))
				{
					patterns.Add(pattern);
				}
			}

			return patterns.ToArray();
		}

		internal static string ConvertBypassAddressToRegex(string address)
		{
			const string ProtocolTerminator = "://";
			string protocolPattern;
			string portPattern;
			string addressPattern;

			address = address.Trim();

			var protocolEndIndex = address.IndexOf(ProtocolTerminator);
			if (protocolEndIndex >= 0)
			{
				protocolEndIndex += ProtocolTerminator.Length;
				var protocol = address.Substring(0, protocolEndIndex);
				protocolPattern = AddressWildcardToRegex(protocol);
				address = address.Substring(protocolEndIndex);
			}
			else
			{
				protocolPattern = "(?:.*://)?";
			}

			var portIndex = address.IndexOf(':');
			if (portIndex >= 0)
			{
				portPattern = address.Substring(portIndex);
				address = address.Substring(0, portIndex);
			}
			else
			{
				portPattern = "(?::[0-9]{1,5})?";
			}

			var result = string.Empty;
			if (address.Length > 0)
			{
				addressPattern = AddressWildcardToRegex(address);
				result = "^" + protocolPattern + addressPattern + portPattern + "$";
			}

			return result;
		}

		static string AddressWildcardToRegex(string s)
		{
			return s.Replace(".", @"\.").Replace("*", ".*");
		}

		#endregion

		#region SH_Status

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public ZString SH_Status
		{
			get
			{
				var timeDiff = !base.SH_DeleteTimeStampUtc.IsEmpty
					? Math.Round((DateTime.UtcNow - base.SH_DeleteTimeStampUtc).TotalMinutes)
					: double.NaN;

				return (base.SH_DeleteTimeStampUtc.IsEmpty || timeDiff < 0)
					? StmServiceHostStatusTypes.Codes.Installed
					: (timeDiff < 60)
						? StmServiceHostStatusTypes.Codes.Deleting
						: StmServiceHostStatusTypes.Codes.Obsolete;
			}
		}

		public ZPropertyInfo SH_StatusInfo => GetZPropertyInfo(Schema.SH_Status);

		public ZBool IsInstalled => SH_Status.Equals(StmServiceHostStatusTypes.Codes.Installed);

		public ResponseStatus IsResponding { get; set; } = ResponseStatus.Unknown;

		public ZBool IsAlive => IsResponding == ResponseStatus.Running;

		public enum ResponseStatus
		{
			Unknown,
			Stopped,
			Running
		}

		#endregion

		#region StatusDescription

		public ZString StatusDescription => Lookups.StmServiceHostStatusTypes.GetDescriptionFromCode(SH_Status);

		public ZPropertyInfo StatusDescriptionInfo => GetZPropertyInfo(Schema.StatusDescription);

		#endregion
	}
}


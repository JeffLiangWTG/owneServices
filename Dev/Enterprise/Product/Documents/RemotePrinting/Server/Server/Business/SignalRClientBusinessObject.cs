using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.RemotePrinting.Server.Business
{
	public class SignalRClientBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public abstract class Schema
		{
			public const string ServerName = nameof(SignalRClientBusinessObject.ServerName);
			public const string ClientId = nameof(SignalRClientBusinessObject.ClientId);
			public const string WebPrintServerAddress = nameof(SignalRClientBusinessObject.WebPrintServerAddress);
			public const string WebPrintServerHostName = nameof(SignalRClientBusinessObject.WebPrintServerHostName);
			public const string PrintersCount = nameof(SignalRClientBusinessObject.PrintersCount);
			public const string WebPrintClientVersionNumber = nameof(SignalRClientBusinessObject.WebPrintClientVersionNumber);
			public const string RetrieveLogs = nameof(SignalRClientBusinessObject.RetrieveLogs);
		}

		public ZString ServerName
		{
			get { return serverName; }
			set
			{
				SetNonPersistentPropertyValue(ServerNameInfo, ref serverName, value);
			}
		}
		ZString serverName;

		public ZPropertyInfo ServerNameInfo
		{
			get { return GetZPropertyInfo(Schema.ServerName); }
		}

		public ZString ClientId
		{
			get { return clientId; }
			set
			{
				SetNonPersistentPropertyValue(ClientIdInfo, ref clientId, value);
			}
		}
		ZString clientId;

		public ZPropertyInfo ClientIdInfo
		{
			get { return GetZPropertyInfo(Schema.ClientId); }
		}

		public ZString WebPrintServerAddress
		{
			get { return webPrintServerAddress; }
			set
			{
				SetNonPersistentPropertyValue(WebPrintServerAddressInfo, ref webPrintServerAddress, value);
			}
		}
		ZString webPrintServerAddress;

		public ZPropertyInfo WebPrintServerAddressInfo
		{
			get { return GetZPropertyInfo(Schema.WebPrintServerAddress); }
		}

		public ZString WebPrintServerHostName
		{
			get { return webPrintServerHostName; }
			set
			{
				SetNonPersistentPropertyValue(WebPrintServerHostNameInfo, ref webPrintServerHostName, value);
			}
		}
		ZString webPrintServerHostName;

		public ZPropertyInfo WebPrintServerHostNameInfo
		{
			get { return GetZPropertyInfo(Schema.WebPrintServerHostName); }
		}

		public ZString WebPrintClientVersionNumber
		{
			get { return webPrintClientVersionNumber; }
			set
			{
				SetNonPersistentPropertyValue(WebPrintClientVersionNumberInfo, ref webPrintClientVersionNumber, value);
			}
		}
		ZString webPrintClientVersionNumber;

		public ZPropertyInfo WebPrintClientVersionNumberInfo
		{
			get { return GetZPropertyInfo(Schema.WebPrintClientVersionNumber); }
		}

		public ZInt PrintersCount
		{
			get { return printersCount; }
			set
			{
				SetNonPersistentPropertyValue(PrintersCountInfo, ref printersCount, value);
			}
		}
		ZInt printersCount;

		public ZPropertyInfo PrintersCountInfo
		{
			get { return GetZPropertyInfo(Schema.PrintersCount); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public ZString RetrieveLogs
		{
			get { return "Retrieve Logs"; }
		}

		public ZPropertyInfo RetrieveLogsInfo
		{
			get { return GetZPropertyInfo(Schema.RetrieveLogs); }
		}
	}
}

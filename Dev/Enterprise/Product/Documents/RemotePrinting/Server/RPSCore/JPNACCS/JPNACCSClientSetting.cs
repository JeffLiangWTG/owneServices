using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.eHub.Common;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.xTMessaging.Business.ConfigurationProvider;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public class JPNACCSClientSetting
	{
		JPNACCSClientSetting() { }

		public static JPNACCSClientSetting New(XElement element, DbConnection connection)
		{
			var result = new JPNACCSClientSetting();
			result.SetValues(element, connection);
			return result;
		}

		[ThreadSafe]
		public static readonly JPNACCSClientSetting Empty = new ();

		const string ApplicationNodeExtension = "_JPC";
		const string NACCSProdMailboxKey = "NACCSMailP";
		const string NACCSTestMailboxKey = "NACCSMailT";
		const string DateTimeFormat = "yyyyMMddHHmm";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void SetValues(XElement element, DbConnection connection)
		{
			NACCSMailbox = GetNACCSMailbox(connection);
			MailBoxInfos = GetMailBoxInfos(connection).ToArray();

			xTServerAddress = DbRegistryHelper.GetxTServerAddress(connection);
			xTServerCertificate = DbRegistryHelper.GetxTServerCertificate(connection);

			var regKey = ObjectFactory.Get<IProductRegistration>()?.Key;
			xTApplicationNode = regKey != null ? regKey.EnterpriseCode + regKey.ServerCode + ApplicationNodeExtension : string.Empty;

			var machineId = element.Element("LocalComputerAlias")?.Value ?? string.Empty;
			DomainName = element.Element("DomainName")?.Value ?? string.Empty;

			xTPassword = CustomsServerEncryptor.Encrypt(Authentication.Instance, SHA512Encryptor.Encrypt(machineId + DomainName));

			ReceivingInterval = ZInt.ParseSafe(element.Element("ReceivingInterval")?.Value ?? string.Empty, 3);
			SendingInterval = ZInt.ParseSafe(element.Element("SendingInterval")?.Value ?? string.Empty, 15);
			Verbose = ZBool.ParseSafe(element.Element("Verbose")?.Value ?? string.Empty, defaultValue: false);

			var isStartTimeValid = ZDateTime.TryParseExact(element.Element("DownTimeStart")?.Value ?? string.Empty, out var downTimeStart, DateTimeFormat) && downTimeStart.IsValid;
			var isEndTimeValid = ZDateTime.TryParseExact(element.Element("DownTimeEnd")?.Value ?? string.Empty, out var downTimeEnd, DateTimeFormat) && downTimeEnd.IsValid;
			DownTimeStart = isStartTimeValid ? downTimeStart.ToDateTime() : DateTime.MaxValue;
			DownTimeEnd = isEndTimeValid ? downTimeEnd.ToDateTime() : DateTime.MinValue;

			XTIdleConnectionKeepAliveInSecondsValue = DbRegistryHelper.GetXTIdleConnectionKeepAlive();
			XTIdleConnectionRetryPauseInSecondsValue = DbRegistryHelper.GetXTIdleConnectionRetryPause();
			InterchangeCountPerBatchOnReceivingValue = DbRegistryHelper.GetInterchangeCountPerBatchOnReceiving();
			XTServerMessageChunkSizeWhenSendingValue = DbRegistryHelper.GetXTServerMessageChunkSizeWhenSending();
		}

		#region Properties

		public MailBoxInfo[] MailBoxInfos { get; set; }

		public string NACCSMailbox { get; set; }

		public string DomainName { get; set; }

		public string xTServerAddress { get; set; }

		public string xTServerCertificate { get; set; }

		public string xTApplicationNode { get; set; }

		public string xTPassword { get; set; }

		public int ReceivingInterval { get; set; }

		public int SendingInterval { get; set; }

		public DateTime DownTimeStart { get; set; }

		public DateTime DownTimeEnd { get; set; }

		public double XTIdleConnectionKeepAliveInSecondsValue { get; set; }

		public double XTIdleConnectionRetryPauseInSecondsValue { get; set; }

		public int InterchangeCountPerBatchOnReceivingValue { get; set; }

		public int XTServerMessageChunkSizeWhenSendingValue { get ; set; }

		public bool Verbose { get; set; }

		#endregion

		IEnumerable<MailBoxInfo> GetMailBoxInfos(DbConnection connection)
		{
			const string sql = $@"SELECT GP_PK, GC_Code, GP_MailBoxID, GP_CurrentPassword
FROM [dbo].[GlbExternalPassword]
JOIN [dbo].[GlbCompany] ON GC_PK = GP_GC
JOIN [dbo].[GenAddOnColumn] ON XA_ParentID = GP_PK AND XA_ParentTableCode = 'GP'
WHERE GP_PasswordType = 'NMC' AND XA_Name = 'JP_ShouldReceive' AND XA_Data = 'Y'";

			using var command = connection.Command(sql);
			using var resultReader = command.ExecuteReader();
			while (resultReader.Read())
			{
				var pk = resultReader[0]?.ToString() ?? string.Empty;
				var companyCode = resultReader[1]?.ToString() ?? string.Empty;
				var mailboxId = resultReader[2]?.ToString() ?? string.Empty;
				if (!string.IsNullOrEmpty(mailboxId))
				{
					var splittedNACCSMailbox = NACCSMailbox.Split('@');
					if (splittedNACCSMailbox.Length == 2)
					{
						mailboxId = $"{mailboxId}@{splittedNACCSMailbox[1]}";
					}
				}
				var currentPassword = resultReader[3]?.ToString() ?? string.Empty;

				if (Guid.TryParse(pk, out var key) && mailboxId.Length > 0 && currentPassword.Length > 0)
				{
					var descryptedPassword = new TwoWayEncoder(key).Decrypt(currentPassword);
					yield return new MailBoxInfo { CompanyCode = companyCode, MailBox = mailboxId, MailBoxPassword = CustomsServerEncryptor.Encrypt(Authentication.Instance, descryptedPassword) };
				}
			}
		}

		string GetNACCSMailbox(DbConnection connection)
		{
			var isProdSystem = (ObjectFactory.Get<IProductRegistration>()?.Key?.DatabaseType ?? string.Empty) == DatabaseTypes.Codes.Production;
			var key = isProdSystem ? NACCSProdMailboxKey : NACCSTestMailboxKey;
			var refDatabase = new ReferenceDatabaseUtils(connection);
			return refDatabase.RefSysConfigLoader.Load(key, ZDateTime.Today)?.ZRC_StringValue ?? ZString.Empty;
		}

		public sealed class MailBoxInfo
		{
			public string CompanyCode { get; set; }

			public string MailBox { get; set; }

			public string MailBoxPassword { get; set; }
		}
	}
}

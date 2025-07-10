using System;
using System.Globalization;
using System.Security;
using System.Xml;
using CargoWise.Common.Testing;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	public class SystemRegistrationKey : ISystemRegistrationKey
	{
		/// <summary>
		/// Constructor for a key for a new install (see ~\Dev\Common\Product\ediLoad\Enterprise.Server.Setup\RegistryInitialiser.cs)
		/// </summary>
		public SystemRegistrationKey(
			DateTime systemExpiryDate, Guid serverSid, string dbInstanceName, string databaseName, string databaseType, string dbSecurityMode)
			: this(systemExpiryDate, serverSid, dbInstanceName, databaseName, databaseType, dbSecurityMode, "", "", "", "", "", 0, 0, DateTime.UtcNow, "")
		{ }

		public SystemRegistrationKey(
			DateTime systemExpiryDate, Guid serverSid, string dbInstanceName, string databaseName,
			string databaseType, string dbSecurityMode, string hostedLocation, string expiredMessage, string expiryWeekMessage, string expiryMonthMessage,
			string systemId,
			double currentBillingTimeZoneUtcOffset, double nextBillingTimeZoneUtcOffset, DateTime nextUtcOffsetEffectiveTimeUtc,
			string billingModel)
		{
			ValidateKeyInfo(serverSid, databaseName, databaseType, dbSecurityMode, hostedLocation);

			this.systemExpiryDate = systemExpiryDate;
			this.serverSid = serverSid;
			this.dbInstanceName = dbInstanceName;
			this.databaseName = databaseName;
			this.databaseType = databaseType;
			this.dbSecurityMode = dbSecurityMode;
			this.hostedLocation = hostedLocation;

			this.expiredMessage = expiredMessage;
			this.expiryWeekMessage = expiryWeekMessage;
			this.expiryMonthMessage = expiryMonthMessage;

			this.systemId = systemId;

			this.currentBillingTimeZoneUtcOffset = currentBillingTimeZoneUtcOffset;
			this.nextBillingTimeZoneUtcOffset = nextBillingTimeZoneUtcOffset;
			this.nextUtcOffsetEffectiveTimeUtc = nextUtcOffsetEffectiveTimeUtc;

			this.billingModel = billingModel;
		}

		#region New From Encrypted Xml Key

		public static ISystemRegistrationKey NewFromEncryptedXmlKey(string encryptedXMLKey)
		{
			try
			{
				return LoadFromEncryptedXMLKey(encryptedXMLKey);
			}
			catch (SystemRegistrationKeyMissingInformationException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new SystemRegistrationInvalidKeyException(ex);
			}
		}

		static ISystemRegistrationKey LoadFromEncryptedXMLKey(string encryptedXMLKey)
		{
			string xmlKey = TwoWayEncoder.NewWithStandardInitialisationVector().Decrypt(encryptedXMLKey);
			return LoadFromXMLKey(xmlKey);
		}

		static ISystemRegistrationKey LoadFromXMLKey(string xmlKey)
		{
			DateTime? systemExpiryDate = null;
			Guid? serverSid = null;
			string dbInstanceName = null;
			string databaseName = null;
			string databaseType = null;
			string dbSecurityMode = null;
			string hostedLocation = "";
			string expiredMessage = "";
			string expiryWeekMessage = "";
			string expiryMonthMessage = "";
			double currentBillingTimeZoneUtcOffset = 0;
			double nextBillingTimeZoneUtcOffset = 0;
			DateTime nextUtcOffsetEffectiveTimeUtc = DateTime.UtcNow;
			string billingModel = "";

			string systemId = "";

			using (var keyParser = new XmlTextReader(xmlKey, XmlNodeType.Element, null))
			{
				while (keyParser.Read())
				{
					switch (keyParser.Name)
					{
						case "ExpiryDate":
							systemExpiryDate = new DateTime(XmlConvert.ToInt64(keyParser.ReadString()));
							break;
						case "ServerSID":
							serverSid = new Guid(keyParser.ReadString());
							break;
						case "DBInstanceName":
							dbInstanceName = keyParser.ReadString();
							break;
						case "DBName":
							databaseName = keyParser.ReadString();
							break;
						case "DatabaseType":
							databaseType = keyParser.ReadString();
							break;
						case "DbSecurityMode":
							dbSecurityMode = keyParser.ReadString();
							break;
						case "HostedLocation":
							hostedLocation = keyParser.ReadString();
							break;
						case "ExpiredMessage":
							expiredMessage = keyParser.ReadString();
							break;
						case "ExpiryWeekMessage":
							expiryWeekMessage = keyParser.ReadString();
							break;
						case "ExpiryMonthMessage":
							expiryMonthMessage = keyParser.ReadString();
							break;
						case "SystemId":
							systemId = keyParser.ReadString();
							break;
						case "CurrentBillingTimeZoneUtcOffset":
							if (!Double.TryParse(keyParser.ReadString(), out currentBillingTimeZoneUtcOffset))
							{
								currentBillingTimeZoneUtcOffset = 0;
							}
							break;
						case "NextBillingTimeZoneUtcOffset":
							if (!Double.TryParse(keyParser.ReadString(), out nextBillingTimeZoneUtcOffset))
							{
								nextBillingTimeZoneUtcOffset = 0;
							}
							break;
						case "NextUtcOffsetEffectiveTimeUtc":
							nextUtcOffsetEffectiveTimeUtc = new DateTime(XmlConvert.ToInt64(keyParser.ReadString()));
							break;
						case "BillingModel":
							billingModel = keyParser.ReadString();
							break;
					}
				}
			}

			return new SystemRegistrationKey(systemExpiryDate.Value, serverSid.Value, dbInstanceName, databaseName, databaseType, dbSecurityMode, hostedLocation,
				expiredMessage, expiryWeekMessage, expiryMonthMessage,
				systemId,
				currentBillingTimeZoneUtcOffset, nextBillingTimeZoneUtcOffset, nextUtcOffsetEffectiveTimeUtc,
				billingModel);
		}

		#endregion

		#region Validate Key Information

		void ValidateKeyInfo(Guid serverSid, string dBName, string databaseType, string dbSecurityMode, string hostedLocation)
		{
			if (serverSid == Guid.Empty)
			{
				throw new SystemRegistrationKeyMissingInformationException(nameof(serverSid));
			}
			else if (string.IsNullOrEmpty(dBName))
			{
				throw new SystemRegistrationKeyMissingInformationException(nameof(dBName));
			}
			else if (databaseType == null || new DatabaseTypes().IndexOfCode(databaseType) == -1)
			{
				throw new SystemRegistrationKeyMissingInformationException(nameof(databaseType));
			}
			else if (!IsValidDatabaseSecurityMode(dbSecurityMode))
			{
				throw new SystemRegistrationKeyMissingInformationException(nameof(dbSecurityMode));
			}
			else if (hostedLocation == null)
			{
				throw new SystemRegistrationKeyMissingInformationException(nameof(hostedLocation));
			}
		}

		bool IsValidDatabaseSecurityMode(string dbSecurityMode)
		{
			return
				dbSecurityMode == DatabaseSecurityModePairList.Codes.Locked
				|| dbSecurityMode == DatabaseSecurityModePairList.Codes.ExOpen
				|| dbSecurityMode == DatabaseSecurityModePairList.Codes.OpenMode;
		}

		#endregion

		#region XML Template

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML Template")]
		static readonly string XmlTemplate =
			"<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
			"	<SystemRegistrationKey>" + System.Environment.NewLine +
			"		<ExpiryDate>{0}</ExpiryDate>" + System.Environment.NewLine +
			"		<ServerSID>{1}</ServerSID>" + System.Environment.NewLine +
			"		<DBName>{2}</DBName>" + System.Environment.NewLine +
			"		<DBInstanceName>{3}</DBInstanceName>" + System.Environment.NewLine +
			"		<DatabaseType>{4}</DatabaseType>" + System.Environment.NewLine +
			"		<DbSecurityMode>{5}</DbSecurityMode>" + System.Environment.NewLine +
			"		<HostedLocation>{6}</HostedLocation>" + System.Environment.NewLine +
			"		<ExpiredMessage>{7}</ExpiredMessage>" + System.Environment.NewLine +
			"		<ExpiryWeekMessage>{8}</ExpiryWeekMessage>" + System.Environment.NewLine +
			"		<ExpiryMonthMessage>{9}</ExpiryMonthMessage>" + System.Environment.NewLine +
			"		<SystemId>{10}</SystemId>" + System.Environment.NewLine +
			"		<CurrentBillingTimeZoneUtcOffset>{11}</CurrentBillingTimeZoneUtcOffset>" + System.Environment.NewLine +
			"		<NextBillingTimeZoneUtcOffset>{12}</NextBillingTimeZoneUtcOffset>" + System.Environment.NewLine +
			"		<NextUtcOffsetEffectiveTimeUtc>{13}</NextUtcOffsetEffectiveTimeUtc>" + System.Environment.NewLine +
			"		<BillingModel>{14}</BillingModel>" + System.Environment.NewLine +
			"	</SystemRegistrationKey>";

		#endregion

		#region ISystemRegistrationKey Members

		DateTime ISystemRegistrationKey.SystemExpiryDate
		{
			get { return systemExpiryDate; }
		}
		readonly DateTime systemExpiryDate;

		Guid ISystemRegistrationKey.ServerSid
		{
			get { return serverSid; }
		}
		readonly Guid serverSid;

		string ISystemRegistrationKey.DbInstanceName
		{
			get { return dbInstanceName; }
		}
		readonly string dbInstanceName;

		string ISystemRegistrationKey.DatabaseName
		{
			get { return databaseName; }
		}
		readonly string databaseName;

		string ISystemRegistrationKey.DatabaseType
		{
			get { return databaseType; }
		}
		readonly string databaseType;

		string ISystemRegistrationKey.DbSecurityMode
		{
			get { return dbSecurityMode; }
		}
		readonly string dbSecurityMode;

		string ISystemRegistrationKey.HostedLocation
		{
			get { return hostedLocation; }
		}
		readonly string hostedLocation;

		string ISystemRegistrationKey.ExpiryMonthMessage
		{
			get { return expiryMonthMessage; }
		}
		string ISystemRegistrationKey.ExpiryWeekMessage
		{
			get { return expiryWeekMessage; }
		}
		string ISystemRegistrationKey.ExpiredMessage
		{
			get { return expiredMessage; }
		}
		readonly string expiryMonthMessage;
		readonly string expiryWeekMessage;
		readonly string expiredMessage;

		string ISystemRegistrationKey.SystemId
		{
			get { return systemId; }
		}
		readonly string systemId;

		string ISystemRegistrationKey.BillingModel
		{
			get { return billingModel; }
		}
		readonly string billingModel;

		double ISystemRegistrationKey.CurrentBillingTimeZoneUtcOffset
		{
			get { return currentBillingTimeZoneUtcOffset; }
		}
		double ISystemRegistrationKey.NextBillingTimeZoneUtcOffset
		{
			get { return nextBillingTimeZoneUtcOffset; }
		}
		DateTime ISystemRegistrationKey.NextUtcOffsetEffectiveTimeUtc
		{
			get { return nextUtcOffsetEffectiveTimeUtc; }
		}
		readonly double currentBillingTimeZoneUtcOffset;
		readonly double nextBillingTimeZoneUtcOffset;
		readonly DateTime nextUtcOffsetEffectiveTimeUtc;

		string ISystemRegistrationKey.ToEncryptedKeyString()
		{
			string xmlKey = string.Format(XmlTemplate, systemExpiryDate.Ticks, serverSid.ToString(), databaseName, dbInstanceName, databaseType, dbSecurityMode,
				hostedLocation,
				SecurityElement.Escape(expiredMessage),
				SecurityElement.Escape(expiryWeekMessage),
				SecurityElement.Escape(expiryMonthMessage),
				systemId,
				currentBillingTimeZoneUtcOffset.ToString(CultureInfo.InvariantCulture),
				nextBillingTimeZoneUtcOffset.ToString(CultureInfo.InvariantCulture),
				nextUtcOffsetEffectiveTimeUtc.Ticks,
				billingModel);

			return TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(xmlKey);
		}

		#endregion

		public static ISystemRegistrationKey Current
		{
			get
			{
				string newEncrypted = DataRegistry.Instance.RawRegistry.LegacyEncryptedSystemRegistrationKey.Value;

				if (encryptedSystemKey != newEncrypted)
				{
					ISystemRegistrationKey newKey;

					if (string.IsNullOrEmpty(newEncrypted))
					{
						// Default key when nothing in registry
						newKey = new SystemRegistrationKey(new DateTime(2005, 12, 05),
							AdminConnection.ServerSid,
							Db.Connection.ServerInstanceName,
							Db.DatabaseName,
							DatabaseTypes.Codes.Test,
							DatabaseSecurityModePairList.Codes.Locked);
					}
					else
					{
						newKey = NewFromEncryptedXmlKey(newEncrypted);
					}

					if (0 != System.Threading.Interlocked.Exchange(ref systemKeyLock, 1))
					{
						// we lost a race, don't use cache and don't wait for the other thread
						return newKey;
					}

					systemKey = newKey;
					encryptedSystemKey = newEncrypted;
					System.Threading.Interlocked.Exchange(ref systemKeyLock, 0);
				}

				return systemKey;
			}
		}

		[SuppressThreadStaticFieldMessage] // since it's thread safe
		static ISystemRegistrationKey systemKey;

		[SuppressThreadStaticFieldMessage]
		static volatile string encryptedSystemKey;

		[SuppressThreadStaticFieldMessage]
		static int systemKeyLock = 0;
	}
}

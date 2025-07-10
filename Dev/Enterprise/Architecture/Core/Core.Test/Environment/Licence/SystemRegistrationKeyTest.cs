using System;
using Enterprise.ZArchitecture.Core.Encryption;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class SystemRegistrationKeyTest : TestCase
	{
		public void TestConstructWithNewData()
		{
			ISystemRegistrationKey sysKey = new SystemRegistrationKey(TestDate, TestSID, TestDBInstanceName, TestDBName, TestDatabaseType, TestDbSecurityMode);
			AssertEquals(TestDate, sysKey.SystemExpiryDate);
			AssertEquals(TestSID, sysKey.ServerSid);
			AssertEquals(TestDBInstanceName, sysKey.DbInstanceName);
			AssertEquals(TestDBName, sysKey.DatabaseName);
			AssertEquals(TestDatabaseType, sysKey.DatabaseType);
		}

		public void TestConstructWithXMLKey()
		{
			string encryptedXMLKey = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(TestXMLKey);
			ISystemRegistrationKey sysKey = SystemRegistrationKey.NewFromEncryptedXmlKey(encryptedXMLKey);
			AssertEquals(TestDate, sysKey.SystemExpiryDate);
			AssertEquals(TestSID, sysKey.ServerSid);
			AssertEquals(TestDBInstanceName, sysKey.DbInstanceName);
			AssertEquals(TestDBName, sysKey.DatabaseName);
			AssertEquals(TestDatabaseType, sysKey.DatabaseType);
		}

		public void TestConstructWithXMLKeyIncludingHostedLocation()
		{
			string encryptedXMLKey = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(TestXMLKeyWithHostedLocation);
			ISystemRegistrationKey sysKey = SystemRegistrationKey.NewFromEncryptedXmlKey(encryptedXMLKey);
			AssertEquals(TestDate, sysKey.SystemExpiryDate);
			AssertEquals(TestSID, sysKey.ServerSid);
			AssertEquals(TestDBInstanceName, sysKey.DbInstanceName);
			AssertEquals(TestDBName, sysKey.DatabaseName);
			AssertEquals(TestDatabaseType, sysKey.DatabaseType);
			AssertEquals(TestDatabaseType, sysKey.DatabaseType);
			AssertEquals(TestHostedLocation, sysKey.HostedLocation);
		}

		public void TestConstructWithXMLKeyIncludingExpiryMessage()
		{
			string encryptedXMLKey = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(TestXMLKeyWithExpiryMessage);
			ISystemRegistrationKey sysKey = SystemRegistrationKey.NewFromEncryptedXmlKey(encryptedXMLKey);
			AssertEquals(TestDate, sysKey.SystemExpiryDate);
			AssertEquals(TestSID, sysKey.ServerSid);
			AssertEquals(TestDBInstanceName, sysKey.DbInstanceName);
			AssertEquals(TestDBName, sysKey.DatabaseName);
			AssertEquals(TestDatabaseType, sysKey.DatabaseType);
			AssertEquals(TestDatabaseType, sysKey.DatabaseType);
			AssertEquals(TestHostedLocation, sysKey.HostedLocation);
			AssertEquals("Expired", sysKey.ExpiredMessage);
			AssertEquals("Expiry in a week", sysKey.ExpiryWeekMessage);
			AssertEquals("Expiry in a month", sysKey.ExpiryMonthMessage);
		}

		public void TestConstructWithXMLKeyIncludingSystemId()
		{
			string encryptedXMLKey = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(TestXMLKeyWithSystemId);
			ISystemRegistrationKey sysKey = SystemRegistrationKey.NewFromEncryptedXmlKey(encryptedXMLKey);
			AssertEquals(TestDate, sysKey.SystemExpiryDate);
			AssertEquals(TestSID, sysKey.ServerSid);
			AssertEquals(TestDBInstanceName, sysKey.DbInstanceName);
			AssertEquals(TestDBName, sysKey.DatabaseName);
			AssertEquals(TestDatabaseType, sysKey.DatabaseType);
			AssertEquals(TestDatabaseType, sysKey.DatabaseType);
			AssertEquals(TestHostedLocation, sysKey.HostedLocation);
			AssertEquals("Expired", sysKey.ExpiredMessage);
			AssertEquals("Expiry in a week", sysKey.ExpiryWeekMessage);
			AssertEquals("Expiry in a month", sysKey.ExpiryMonthMessage);
			AssertEquals("CC", sysKey.SystemId);
		}

		public void TestToEncryptedKeyString()
		{
			ISystemRegistrationKey sysKey = SystemRegistrationKey.NewFromEncryptedXmlKey(TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(TestXMLKey));
			ISystemRegistrationKey decryptedKey = SystemRegistrationKey.NewFromEncryptedXmlKey(sysKey.ToEncryptedKeyString());
			AssertEquals(TestDate, decryptedKey.SystemExpiryDate);
			AssertEquals(TestSID, decryptedKey.ServerSid);
			AssertEquals(TestDBInstanceName, decryptedKey.DbInstanceName);
			AssertEquals(TestDBName, decryptedKey.DatabaseName);
			AssertEquals(TestDatabaseType, decryptedKey.DatabaseType);
			AssertEquals("", sysKey.HostedLocation);
			AssertEquals("", sysKey.ExpiredMessage);
			AssertEquals("", sysKey.ExpiryWeekMessage);
			AssertEquals("", sysKey.ExpiryMonthMessage);
		}

		public void TestToEncryptedKeyStringIncludingHostedLocation()
		{
			ISystemRegistrationKey sysKey = SystemRegistrationKey.NewFromEncryptedXmlKey(TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(TestXMLKeyWithHostedLocation));
			ISystemRegistrationKey decryptedKey = SystemRegistrationKey.NewFromEncryptedXmlKey(sysKey.ToEncryptedKeyString());
			AssertEquals(TestDate, decryptedKey.SystemExpiryDate);
			AssertEquals(TestSID, decryptedKey.ServerSid);
			AssertEquals(TestDBInstanceName, decryptedKey.DbInstanceName);
			AssertEquals(TestDBName, decryptedKey.DatabaseName);
			AssertEquals(TestDatabaseType, decryptedKey.DatabaseType);
			AssertEquals(TestHostedLocation, sysKey.HostedLocation);
		}

		public void TestToEncryptedKeyStringIncludingExpiryMessage()
		{
			ISystemRegistrationKey sysKey = SystemRegistrationKey.NewFromEncryptedXmlKey(TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(TestXMLKeyWithExpiryMessage));
			ISystemRegistrationKey decryptedKey = SystemRegistrationKey.NewFromEncryptedXmlKey(sysKey.ToEncryptedKeyString());
			AssertEquals(TestDate, decryptedKey.SystemExpiryDate);
			AssertEquals(TestSID, decryptedKey.ServerSid);
			AssertEquals(TestDBInstanceName, decryptedKey.DbInstanceName);
			AssertEquals(TestDBName, decryptedKey.DatabaseName);
			AssertEquals(TestDatabaseType, decryptedKey.DatabaseType);
			AssertEquals(TestHostedLocation, sysKey.HostedLocation);
			AssertEquals("Expired", sysKey.ExpiredMessage);
			AssertEquals("Expiry in a week", sysKey.ExpiryWeekMessage);
			AssertEquals("Expiry in a month", sysKey.ExpiryMonthMessage);

			const string msg1 = "</ExpiredMessage><>\n";
			const string msg2 = "2><\n";
			const string msg3 = "3\n<tag>";
			sysKey = new SystemRegistrationKey(TestDate, TestSID, TestDBInstanceName, TestDBName, TestDatabaseType, TestDbSecurityMode, TestHostedLocation, msg1, msg2, msg3, "", 0, 0, DateTime.MaxValue, "");
			decryptedKey = SystemRegistrationKey.NewFromEncryptedXmlKey(sysKey.ToEncryptedKeyString());
			AssertEquals(msg1, sysKey.ExpiredMessage);
			AssertEquals(msg2, sysKey.ExpiryWeekMessage);
			AssertEquals(msg3, sysKey.ExpiryMonthMessage);
		}

		public void TestToEncryptedKeyStringIncludingSystemId()
		{
			ISystemRegistrationKey sysKey = SystemRegistrationKey.NewFromEncryptedXmlKey(TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(TestXMLKeyWithSystemId));
			ISystemRegistrationKey decryptedKey = SystemRegistrationKey.NewFromEncryptedXmlKey(sysKey.ToEncryptedKeyString());
			AssertEquals(TestDate, decryptedKey.SystemExpiryDate);
			AssertEquals(TestSID, decryptedKey.ServerSid);
			AssertEquals(TestDBInstanceName, decryptedKey.DbInstanceName);
			AssertEquals(TestDBName, decryptedKey.DatabaseName);
			AssertEquals(TestDatabaseType, decryptedKey.DatabaseType);
			AssertEquals(TestHostedLocation, sysKey.HostedLocation);
			AssertEquals("Expired", sysKey.ExpiredMessage);
			AssertEquals("Expiry in a week", sysKey.ExpiryWeekMessage);
			AssertEquals("Expiry in a month", sysKey.ExpiryMonthMessage);
			AssertEquals("CC", decryptedKey.SystemId);

			sysKey = new SystemRegistrationKey(TestDate, TestSID, TestDBInstanceName, TestDBName, TestDatabaseType, TestDbSecurityMode, TestHostedLocation, "", "", "", "DDD", 0, 0, DateTime.MaxValue, "");
			decryptedKey = SystemRegistrationKey.NewFromEncryptedXmlKey(sysKey.ToEncryptedKeyString());
			AssertEquals("DDD", decryptedKey.SystemId);
		}

		public void TestToEncryptedKeyStringIncludingBillingModel()
		{
			ISystemRegistrationKey sysKey = SystemRegistrationKey.NewFromEncryptedXmlKey(TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(TestXMLKeyWithBillingModel));
			ISystemRegistrationKey decryptedKey = SystemRegistrationKey.NewFromEncryptedXmlKey(sysKey.ToEncryptedKeyString());
			AssertEquals(TestDate, decryptedKey.SystemExpiryDate);
			AssertEquals(TestSID, decryptedKey.ServerSid);
			AssertEquals(TestDBInstanceName, decryptedKey.DbInstanceName);
			AssertEquals(TestDBName, decryptedKey.DatabaseName);
			AssertEquals(TestDatabaseType, decryptedKey.DatabaseType);
			AssertEquals(TestHostedLocation, sysKey.HostedLocation);
			AssertEquals("Expired", sysKey.ExpiredMessage);
			AssertEquals("Expiry in a week", sysKey.ExpiryWeekMessage);
			AssertEquals("Expiry in a month", sysKey.ExpiryMonthMessage);
			AssertEquals("STL", decryptedKey.BillingModel);

			sysKey = new SystemRegistrationKey(TestDate, TestSID, TestDBInstanceName, TestDBName, TestDatabaseType, TestDbSecurityMode, TestHostedLocation, "", "", "", "", 0, 0, DateTime.MaxValue, "ODM");
			decryptedKey = SystemRegistrationKey.NewFromEncryptedXmlKey(sysKey.ToEncryptedKeyString());
			AssertEquals("ODM", decryptedKey.BillingModel);
		}

		public void TestToEncryptedKeyStringIncludingBillingTimeZone()
		{
			ISystemRegistrationKey sysKey = SystemRegistrationKey.NewFromEncryptedXmlKey(TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(TestXMLKeyWithBillingTimeZone));
			ISystemRegistrationKey decryptedKey = SystemRegistrationKey.NewFromEncryptedXmlKey(sysKey.ToEncryptedKeyString());
			AssertEquals(TestDate, decryptedKey.SystemExpiryDate);
			AssertEquals(TestSID, decryptedKey.ServerSid);
			AssertEquals(TestDBInstanceName, decryptedKey.DbInstanceName);
			AssertEquals(TestDBName, decryptedKey.DatabaseName);
			AssertEquals(TestDatabaseType, decryptedKey.DatabaseType);
			AssertEquals(TestHostedLocation, sysKey.HostedLocation);
			AssertEquals("Expired", sysKey.ExpiredMessage);
			AssertEquals("Expiry in a week", sysKey.ExpiryWeekMessage);
			AssertEquals("Expiry in a month", sysKey.ExpiryMonthMessage);
			AssertEquals("CC", decryptedKey.SystemId);
			AssertEquals(11.0d, decryptedKey.CurrentBillingTimeZoneUtcOffset);
			AssertEquals(10.5d, decryptedKey.NextBillingTimeZoneUtcOffset);
			AssertEquals(new DateTime(2016, 4, 2, 16, 0, 0), decryptedKey.NextUtcOffsetEffectiveTimeUtc);

			sysKey = new SystemRegistrationKey(TestDate, TestSID, TestDBInstanceName, TestDBName, TestDatabaseType, TestDbSecurityMode, TestHostedLocation, "", "", "", "", 10.0d, 11.0d, new DateTime(2016, 10, 1, 13, 0, 0), "");
			decryptedKey = SystemRegistrationKey.NewFromEncryptedXmlKey(sysKey.ToEncryptedKeyString());
			AssertEquals(10.0d, decryptedKey.CurrentBillingTimeZoneUtcOffset);
			AssertEquals(11.0d, decryptedKey.NextBillingTimeZoneUtcOffset);
			AssertEquals(new DateTime(2016, 10, 1, 13, 0, 0), decryptedKey.NextUtcOffsetEffectiveTimeUtc);
		}

		[ExpectException(typeof(SystemRegistrationInvalidKeyException))]
		public void TestCorruptKey()
		{
			SystemRegistrationKey.NewFromEncryptedXmlKey("blah blah this is a crap key");
		}

		public void TestNewFromEncryptedXmlKey_WithAnInvalidElementValue()
		{
			string invaliXmlKey = TestXMLKey.Replace(">LCK<", ">SomeInvalidStuff<");
			string encryptedXMLKey = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(invaliXmlKey);

			try
			{
				SystemRegistrationKey.NewFromEncryptedXmlKey(encryptedXMLKey);
				Fail("Should throw exception");
			}
			catch (SystemRegistrationKeyMissingInformationException ex)
			{
				AssertEquals("Parameter name", "dbSecurityMode", ex.ParamName);
			}
		}

		public void TestEnoughInformationToGenerateKey_InvalidSID()
		{
			try
			{
				new SystemRegistrationKey(TestDate, Guid.Empty, TestDBInstanceName, TestDBName, TestDatabaseType, TestDbSecurityMode);
				Fail("Create Key should throw exception");
			}
			catch (SystemRegistrationKeyMissingInformationException ex)
			{
				AssertEquals("Parameter name", "serverSid", ex.ParamName);
			}
		}

		public void TestEnoughInformationToGenerateKey_InvalidDBName()
		{
			try
			{
				new SystemRegistrationKey(TestDate, TestSID, TestDBInstanceName, "", TestDatabaseType, TestDbSecurityMode);
				Fail("Create Key should throw exception");
			}
			catch (SystemRegistrationKeyMissingInformationException ex)
			{
				AssertEquals("Parameter name", "dBName", ex.ParamName);
			}

			try
			{
				new SystemRegistrationKey(TestDate, TestSID, TestDBInstanceName, null, TestDatabaseType, TestDbSecurityMode);
				Fail("Create Key should throw exception");
			}
			catch (SystemRegistrationKeyMissingInformationException ex)
			{
				AssertEquals("Parameter name", "dBName", ex.ParamName);
			}
		}

		public void TestEnoughInformationToGenerateKey_InvalidDatabaseType()
		{
			try
			{
				new SystemRegistrationKey(TestDate, TestSID, TestDBInstanceName, TestDBName, "MAD", TestDbSecurityMode);
				Fail("Create Key should throw exception");
			}
			catch (SystemRegistrationKeyMissingInformationException ex)
			{
				AssertEquals("Parameter name", "databaseType", ex.ParamName);
			}

			try
			{
				new SystemRegistrationKey(TestDate, TestSID, TestDBInstanceName, TestDBName, null, TestDbSecurityMode);
				Fail("Create Key should throw exception");
			}
			catch (SystemRegistrationKeyMissingInformationException ex)
			{
				AssertEquals("Parameter name", "databaseType", ex.ParamName);
			}
		}

		public void TestEnoughInformationToGenerateKey_InvalidDbSecurityMode()
		{
			try
			{
				new SystemRegistrationKey(TestDate, TestSID, TestDBInstanceName, TestDBName, TestDatabaseType, "SomeInvalidMode");
				Fail("Create Key should throw exception");
			}
			catch (SystemRegistrationKeyMissingInformationException ex)
			{
				AssertEquals("Parameter name", "dbSecurityMode", ex.ParamName);
			}
		}

		public void TestEnoughInformationToGenerateKey_InvalidHostedLocation()
		{
			try
			{
				new SystemRegistrationKey(TestDate, TestSID, TestDBInstanceName, TestDBName, TestDatabaseType, TestDbSecurityMode, null, "", "", "", "", 0, 0, DateTime.UtcNow, "");
				Fail("Create Key should throw exception");
			}
			catch (SystemRegistrationKeyMissingInformationException ex)
			{
				AssertEquals("Parameter name", "hostedLocation", ex.ParamName);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			TestDate = new DateTime(632644992000000000);
			TestSID = new Guid("AF116187-E01C-477A-A535-B2A38EE6ED65");
			TestDBInstanceName = "Instance";
			TestDBName = "DBName";
			TestDatabaseType = DatabaseTypes.Codes.Production;
			TestDbSecurityMode = DatabaseSecurityModePairList.Codes.Locked;
			TestHostedLocation = "SYD";
		}

		DateTime TestDate;
		Guid TestSID;
		string TestDBInstanceName;
		string TestDBName;
		string TestDatabaseType;
		string TestDbSecurityMode;
		string TestHostedLocation;

		readonly string TestXMLKey = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
										"	<SystemRegistrationKey>" + System.Environment.NewLine +
										"		<ExpiryDate>632644992000000000</ExpiryDate>" + System.Environment.NewLine +
										"		<ServerSID>AF116187-E01C-477A-A535-B2A38EE6ED65</ServerSID>" + System.Environment.NewLine +
										"		<DBName>DBName</DBName>" + System.Environment.NewLine +
										"		<DBInstanceName>Instance</DBInstanceName>" + System.Environment.NewLine +
										"		<DatabaseType>PRD</DatabaseType>" + System.Environment.NewLine +
										"		<DbSecurityMode>LCK</DbSecurityMode>" + System.Environment.NewLine +
										"	</SystemRegistrationKey>";

		readonly string TestXMLKeyWithHostedLocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
										"	<SystemRegistrationKey>" + System.Environment.NewLine +
										"		<ExpiryDate>632644992000000000</ExpiryDate>" + System.Environment.NewLine +
										"		<ServerSID>AF116187-E01C-477A-A535-B2A38EE6ED65</ServerSID>" + System.Environment.NewLine +
										"		<DBName>DBName</DBName>" + System.Environment.NewLine +
										"		<DBInstanceName>Instance</DBInstanceName>" + System.Environment.NewLine +
										"		<DatabaseType>PRD</DatabaseType>" + System.Environment.NewLine +
										"		<DbSecurityMode>LCK</DbSecurityMode>" + System.Environment.NewLine +
										"		<HostedLocation>SYD</HostedLocation>" + System.Environment.NewLine +
										"	</SystemRegistrationKey>";

		readonly string TestXMLKeyWithExpiryMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
										"	<SystemRegistrationKey>" + System.Environment.NewLine +
										"		<ExpiryDate>632644992000000000</ExpiryDate>" + System.Environment.NewLine +
										"		<ServerSID>AF116187-E01C-477A-A535-B2A38EE6ED65</ServerSID>" + System.Environment.NewLine +
										"		<DBName>DBName</DBName>" + System.Environment.NewLine +
										"		<DBInstanceName>Instance</DBInstanceName>" + System.Environment.NewLine +
										"		<DatabaseType>PRD</DatabaseType>" + System.Environment.NewLine +
										"		<DbSecurityMode>LCK</DbSecurityMode>" + System.Environment.NewLine +
										"		<HostedLocation>SYD</HostedLocation>" + System.Environment.NewLine +
										"		<ExpiredMessage>Expired</ExpiredMessage>" + System.Environment.NewLine +
										"		<ExpiryWeekMessage>Expiry in a week</ExpiryWeekMessage>" + System.Environment.NewLine +
										"		<ExpiryMonthMessage>Expiry in a month</ExpiryMonthMessage>" + System.Environment.NewLine +
										"	</SystemRegistrationKey>";

		readonly string TestXMLKeyWithSystemId = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
										"	<SystemRegistrationKey>" + System.Environment.NewLine +
										"		<ExpiryDate>632644992000000000</ExpiryDate>" + System.Environment.NewLine +
										"		<ServerSID>AF116187-E01C-477A-A535-B2A38EE6ED65</ServerSID>" + System.Environment.NewLine +
										"		<DBName>DBName</DBName>" + System.Environment.NewLine +
										"		<DBInstanceName>Instance</DBInstanceName>" + System.Environment.NewLine +
										"		<DatabaseType>PRD</DatabaseType>" + System.Environment.NewLine +
										"		<DbSecurityMode>LCK</DbSecurityMode>" + System.Environment.NewLine +
										"		<HostedLocation>SYD</HostedLocation>" + System.Environment.NewLine +
										"		<ExpiredMessage>Expired</ExpiredMessage>" + System.Environment.NewLine +
										"		<ExpiryWeekMessage>Expiry in a week</ExpiryWeekMessage>" + System.Environment.NewLine +
										"		<ExpiryMonthMessage>Expiry in a month</ExpiryMonthMessage>" + System.Environment.NewLine +
										"		<SystemId>CC</SystemId>" + System.Environment.NewLine +
										"	</SystemRegistrationKey>";

		readonly string TestXMLKeyWithBillingTimeZone = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
										"	<SystemRegistrationKey>" + System.Environment.NewLine +
										"		<ExpiryDate>632644992000000000</ExpiryDate>" + System.Environment.NewLine +
										"		<ServerSID>AF116187-E01C-477A-A535-B2A38EE6ED65</ServerSID>" + System.Environment.NewLine +
										"		<DBName>DBName</DBName>" + System.Environment.NewLine +
										"		<DBInstanceName>Instance</DBInstanceName>" + System.Environment.NewLine +
										"		<DatabaseType>PRD</DatabaseType>" + System.Environment.NewLine +
										"		<DbSecurityMode>LCK</DbSecurityMode>" + System.Environment.NewLine +
										"		<HostedLocation>SYD</HostedLocation>" + System.Environment.NewLine +
										"		<ExpiredMessage>Expired</ExpiredMessage>" + System.Environment.NewLine +
										"		<ExpiryWeekMessage>Expiry in a week</ExpiryWeekMessage>" + System.Environment.NewLine +
										"		<ExpiryMonthMessage>Expiry in a month</ExpiryMonthMessage>" + System.Environment.NewLine +
										"		<SystemId>CC</SystemId>" + System.Environment.NewLine +
										"		<CurrentBillingTimeZoneUtcOffset>11</CurrentBillingTimeZoneUtcOffset>" + System.Environment.NewLine +
										"		<NextBillingTimeZoneUtcOffset>10.5</NextBillingTimeZoneUtcOffset>" + System.Environment.NewLine +
										"		<NextUtcOffsetEffectiveTimeUtc>635952096000000000</NextUtcOffsetEffectiveTimeUtc>" + System.Environment.NewLine +
										"	</SystemRegistrationKey>";

		readonly string TestXMLKeyWithBillingModel = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
										"	<SystemRegistrationKey>" + System.Environment.NewLine +
										"		<ExpiryDate>632644992000000000</ExpiryDate>" + System.Environment.NewLine +
										"		<ServerSID>AF116187-E01C-477A-A535-B2A38EE6ED65</ServerSID>" + System.Environment.NewLine +
										"		<DBName>DBName</DBName>" + System.Environment.NewLine +
										"		<DBInstanceName>Instance</DBInstanceName>" + System.Environment.NewLine +
										"		<DatabaseType>PRD</DatabaseType>" + System.Environment.NewLine +
										"		<DbSecurityMode>LCK</DbSecurityMode>" + System.Environment.NewLine +
										"		<HostedLocation>SYD</HostedLocation>" + System.Environment.NewLine +
										"		<ExpiredMessage>Expired</ExpiredMessage>" + System.Environment.NewLine +
										"		<ExpiryWeekMessage>Expiry in a week</ExpiryWeekMessage>" + System.Environment.NewLine +
										"		<ExpiryMonthMessage>Expiry in a month</ExpiryMonthMessage>" + System.Environment.NewLine +
										"		<SystemId>CC</SystemId>" + System.Environment.NewLine +
										"		<CurrentBillingTimeZoneUtcOffset>11</CurrentBillingTimeZoneUtcOffset>" + System.Environment.NewLine +
										"		<NextBillingTimeZoneUtcOffset>10.5</NextBillingTimeZoneUtcOffset>" + System.Environment.NewLine +
										"		<NextUtcOffsetEffectiveTimeUtc>635952096000000000</NextUtcOffsetEffectiveTimeUtc>" + System.Environment.NewLine +
										"		<BillingModel>STL</BillingModel>" + System.Environment.NewLine +
										"	</SystemRegistrationKey>";

		#endregion
	}
}

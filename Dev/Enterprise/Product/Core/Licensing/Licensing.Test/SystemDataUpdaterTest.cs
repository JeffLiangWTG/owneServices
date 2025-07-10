using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Licensing.Testing
{
	sealed class SystemDataUpdaterTest : TransactionedTestCase
	{
		public void TestDefaultLicenceUpdateNotificationType()
		{
			var updater = new SystemDataUpdater("asdasda");
			AssertEquals(typeof(LicenceUpdateNotification), updater.Response.GetType());
		}

		public void TestProcessBadXMLPacket()
		{
			var response = new LicenceUpdateNotificationForTest();
			var updater = new SystemDataUpdater("asdasda", response);
			updater.Process();
			AssertEquals("notifications", 1, response.TotalCalls);
			AssertEquals("response message", SystemDataUpdater.DataPacketCorrupt, response.Message);
			AssertEquals("response packet", updater.UpdateDataXMLPacket, response.UpdateDataXMLPacket);
		}

		public void TestProcessNoKeySpecified()
		{
			AssertSystemRegistrationKey("Initially the system will expire on the 5th Dec 2005", Env.Registry.LegacyEncryptedSystemRegistrationKey);
			Dictionary<Guid, string> licenceKeyList = new Dictionary<Guid, string>();
			BusinessObject[] companies = new BusinessObjectFactory().Load(typeof(GlbCompany), new ZQuery());

			SystemUpdatePacket updatePacket = new SystemUpdatePacket();
			var response = new LicenceUpdateNotificationForTest();
			SystemDataUpdater updater = new SystemDataUpdater(updatePacket.ToXMLString(), response);
			updater.Process();

			AssertSystemRegistrationKey("The system will still expire on the 5th Dec 2005", Env.Registry.LegacyEncryptedSystemRegistrationKey);
			AssertEquals("notifications", 0, response.TotalCalls);
		}

		#region System Registration Key Update

		public void TestProcessSystemKey()
		{
			AssertSystemRegistrationKey("Initially the system will expire on the 5th Dec 2005", Env.Registry.LegacyEncryptedSystemRegistrationKey);

			SystemUpdatePacket updatePacket = new SystemUpdatePacket();
			updatePacket.EncryptedSysRegKey = EncryptedSystemKeyForTesting(false);
			var response = new LicenceUpdateNotificationForTest();
			SystemDataUpdater updater = new SystemDataUpdater(updatePacket.ToXMLString(), response);
			updater.Process();

			AssertSystemRegistrationKey("System now set to expire TODAY", updatePacket.EncryptedSysRegKey);

			AssertEquals("notifications", 1, response.TotalCalls);
			AssertEquals("response message", "OK", response.Message);
			AssertEquals("response packet", updater.UpdateDataXMLPacket, response.UpdateDataXMLPacket);
		}

		public void TestProcessSystemKeyInvalidSID()
		{
			SystemUpdatePacket updatePacket = new SystemUpdatePacket();
			updatePacket.EncryptedSysRegKey = EncryptedSystemKeyForTesting(true);
			var response = new LicenceUpdateNotificationForTest();
			SystemDataUpdater updater = new SystemDataUpdater(updatePacket.ToXMLString(), response);

			AssertSystemRegistrationKey("Initially the system will expire on the 5th Dec 2005", Env.Registry.LegacyEncryptedSystemRegistrationKey);
			updater.Process();

			AssertSystemRegistrationKey("The system will still expire on the 5th Dec 2005", Env.Registry.LegacyEncryptedSystemRegistrationKey);
			AssertEquals("notifications", 1, response.TotalCalls);
			AssertEquals("response message", SystemDataUpdater.InvalidSID, response.Message);
			AssertEquals("response packet", updater.UpdateDataXMLPacket, response.UpdateDataXMLPacket);
		}

		void AssertSystemRegistrationKey(string message, string expectedKey)
		{
			AssertEquals(message, expectedKey, Env.Registry.LegacyEncryptedSystemRegistrationKey);
		}

		#endregion

		#region Set up

		string EncryptedSystemKeyForTesting(bool useDifferentSIDToDefault)
		{
			ISystemRegistrationKey newKey = new SystemRegistrationKey(ZDateTime.Today.ToDateTime(),
				useDifferentSIDToDefault ? Guid.NewGuid() : AdminConnection.ServerSid,
				Db.Connection.ServerInstanceName,
				Db.DatabaseName, DatabaseTypes.Codes.Test, DatabaseSecurityModePairList.Codes.Locked);
			return newKey.ToEncryptedKeyString();
		}

		#endregion

		class LicenceUpdateNotificationForTest : ILicenceUpdateNotification
		{
			public string UpdateDataXMLPacket;
			public string Message;
			public int TotalCalls;

			public void Send(BusinessObjectFactory factory, string updateDataXMLPacket, string message)
			{
				UpdateDataXMLPacket = updateDataXMLPacket;
				Message = message;
				++TotalCalls;
			}
		}
	}
}

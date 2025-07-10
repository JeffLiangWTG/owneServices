using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.Data.SqlServer.Testing
{
	public class AvailabilityGroupInfoTest : TestCase
	{
		public void TestGetSecondaryServers_NoException()
		{
			var grp = AvailabilityGroupInfo.GetGroupInfo(Db.NewAdminConnection(), Db.DatabaseName);
			AssertNoExceptionThrown(() => { grp.GetSecondaryServers(Db.NewAdminConnection()); });
		}

		public void TestCheckNonAlwaysOnDatabase()
		{
			var grp = AvailabilityGroupInfo.GetGroupInfo(Db.NewAdminConnection(), Db.DatabaseName);
			AssertEquals("Should be an empty group name", null, grp.GroupName);
			AssertEquals("Should be an empty GUID", Guid.Empty, grp.GroupId);
			AssertEquals("Should be an empty GUID", Guid.Empty, grp.PrimaryReplicaId);
			AssertEquals("Listener IP list should be null", null, grp.ListenerIPs);
		}

		[ExpectNoExceptions]
		public void TestAgWithMultipleListenerIPs()
		{
			var grp = AvailabilityGroupInfoForTesting.GetGroupInfo(Db.NewAdminConnection(), Db.DatabaseName);
			AssertEquals("Group name", Db.DatabaseName, grp.GroupName);
			AssertEquals("Should be an empty GUID", Guid.Parse("77B3C783-C79F-409D-B307-223F138AEEEE"), grp.GroupId);
			AssertEquals("Should be an empty GUID", Guid.Parse("77B3C783-C79F-409D-B307-223F13ABCDEF"), grp.PrimaryReplicaId);
			AssertEquals("Number of IPs for the listener", 3, grp.ListenerIPs.Count);
			AssertEquals("First IP", "10.1.2.3", grp.ListenerIPs[0]);
			AssertEquals("Second IP", "10.1.2.5", grp.ListenerIPs[1]);
			AssertEquals("Third IP", "10.1.2.7", grp.ListenerIPs[2]);
		}

		public void TestConstructorWithEmptyArray()
		{
			// Act
			var info = new AvailabilityGroupInfo(Array.Empty<string>());

			// Assert
			AssertEquals("Group name", null, info.GroupName);
			AssertEquals("Should be an empty GUID", Guid.Empty, info.GroupId);
			AssertEquals("Should be an empty GUID", Guid.Empty, info.PrimaryReplicaId);
			AssertEquals("Number of IPs for the listener", null, info.ListenerIPs);
		}

		public void TestConstructor()
		{
			// Arrange
			var infoStrings = new[]
			{
				"groupName",
				"77B3C783-C79F-409D-B307-223F138AEEEE",
				"77B3C783-C79F-409D-B307-223F138AEEEE",
				"10.1.2.3,10.1.2.4,10.1.2.5"
			};

			// Act
			var info = new AvailabilityGroupInfo(infoStrings);

			// Assert
			AssertAvailabilityGroupInfoEquals(infoStrings, info);
		}

		void AssertAvailabilityGroupInfoEquals(string[] infoStrings, AvailabilityGroupInfo info)
		{
			AssertEquals("Group name", infoStrings[0], info.GroupName);
			AssertEquals("Should be an empty GUID", Guid.Parse(infoStrings[1]), info.GroupId);
			AssertEquals("Should be an empty GUID", Guid.Parse(infoStrings[2]), info.PrimaryReplicaId);
			var ips = infoStrings[3].Split(',');
			AssertEquals("Number of IPs for the listener", ips.Length, info.ListenerIPs.Count);
			var index = 0;
			foreach (var ip in ips)
			{
				AssertEquals(ip, info.ListenerIPs[index++]);
			}
		}

		public void TestConvertToArray()
		{
			// Arrange
			var infoStrings = Array.Empty<string>();

			// Act & Assert
			AssertConvertToArray(infoStrings);

			// Arrange
			infoStrings = new[]
			{
				"groupName",
				"77b3c783-c79f-409d-b307-223f138aeeee",
				"77b3c783-c79f-409d-b307-223f138aeeee",
				"10.1.2.3,10.1.2.4,10.1.2.5"
			};

			// Act & Assert
			AssertConvertToArray(infoStrings);
		}

		void AssertConvertToArray(string[] infoStrings)
		{
			// Act
			var info = new AvailabilityGroupInfo(infoStrings);
			var newInfoStrings = info.ConvertToArray();

			// Assert
			AssertArrayEqualsByElements(infoStrings, newInfoStrings);
		}
	}

	public class AvailabilityGroupInfoForTesting : AvailabilityGroupInfo
	{
		public AvailabilityGroupInfoForTesting(string groupName, Guid groupId, Guid primaryReplicaId, List<string> listenerIP) : base(groupName, groupId, primaryReplicaId, listenerIP)
		{
		}

		public static new AvailabilityGroupInfo GetGroupInfo(AdminConnection connection, string mainDbName)
		{
			AvailabilityGroupInfo result = null;

			var sqlScript = @"
SELECT '" + mainDbName + @"' AS GroupName, CAST('77B3C783-C79F-409D-B307-223F138AEEEE' as uniqueidentifier) AS GroupId, CAST('77B3C783-C79F-409D-B307-223F13ABCDEF' as uniqueidentifier) AS replica_id, '10.1.2.3' ip_address UNION ALL
SELECT '" + mainDbName + @"' AS GroupName, CAST('77B3C783-C79F-409D-B307-223F138AEEEE' as uniqueidentifier) AS GroupId, CAST('77B3C783-C79F-409D-B307-223F13ABCDEF' as uniqueidentifier) AS replica_id, '10.1.2.5' ip_address UNION ALL
SELECT '" + mainDbName + @"' AS GroupName, CAST('77B3C783-C79F-409D-B307-223F138AEEEE' as uniqueidentifier) AS GroupId, CAST('77B3C783-C79F-409D-B307-223F13ABCDEF' as uniqueidentifier) AS replica_id, '10.1.2.7' ip_address
";

			using (var reader = connection.Command(sqlScript).ExecuteReader())
			{
				result = GetGroupInfoFromDataReader(reader);
			}

			return result;
		}
	}
}

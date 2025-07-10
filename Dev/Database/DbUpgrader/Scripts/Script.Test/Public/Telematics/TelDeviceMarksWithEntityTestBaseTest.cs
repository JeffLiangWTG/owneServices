using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;

namespace Enterprise.Build.Database.Script.Public.Telematics
{
	abstract class TelDeviceMarksWithEntityTestBase : TelDeviceTestCase
	{
		public void TestSelectsAllData()
		{
			TestEntity(Guid.Empty, new[]
			{
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000000")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000001")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000002")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000003")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000004")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000005")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000006")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000007")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000008")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000009")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-00000000000A")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-00000000000B")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-00000000000C")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-00000000000D")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-00000000000E")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-00000000000F")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000000")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000001")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000002")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000003")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000004")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000005")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000006")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000007")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000008")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000009")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-00000000000A")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-00000000000B")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-00000000000C")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-00000000000D")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-00000000000E")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-00000000000F")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000000")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000001")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000002")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000003")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000004")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000005")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000006")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000007")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000008")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000009")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-00000000000A")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-00000000000B")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-00000000000C")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-00000000000D")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-00000000000E")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-00000000000F")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000000")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000001")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000002")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000003")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000004")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000005")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000006")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000007")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000008")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000009")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-00000000000A")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-00000000000B")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-00000000000C")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-00000000000D")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-00000000000E")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-00000000000F")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000000")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000001")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000002")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000003")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000004")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000005")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000006")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000007")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000008")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000009")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-00000000000A")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-00000000000B")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-00000000000C")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-00000000000D")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-00000000000E")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-00000000000F"))
			});
		}

		[UseSnapshotProtection]
		public void TestBlockedRowsReading()
		{
			TestBlockedRowsReading(Guid.Empty, new[]
			{
				new Guid("00000001-0000-0000-0000-000000000000"),
				new Guid("00000001-0000-0000-0000-000000000001"),
				new Guid("00000001-0000-0000-0000-000000000002"),
				new Guid("00000001-0000-0000-0000-000000000003"),
				new Guid("00000001-0000-0000-0000-000000000004"),
				new Guid("00000002-0000-0000-0000-000000000000"),
				new Guid("00000002-0000-0000-0000-000000000001"),
				new Guid("00000002-0000-0000-0000-000000000002"),
				new Guid("00000002-0000-0000-0000-000000000003"),
				new Guid("00000002-0000-0000-0000-000000000004"),
				new Guid("00000003-0000-0000-0000-000000000000"),
				new Guid("00000003-0000-0000-0000-000000000001"),
				new Guid("00000003-0000-0000-0000-000000000002"),
				new Guid("00000003-0000-0000-0000-000000000003"),
				new Guid("00000003-0000-0000-0000-000000000004"),
				new Guid("00000004-0000-0000-0000-000000000000"),
				new Guid("00000004-0000-0000-0000-000000000001"),
				new Guid("00000004-0000-0000-0000-000000000002"),
				new Guid("00000004-0000-0000-0000-000000000003"),
				new Guid("00000004-0000-0000-0000-000000000004"),
				new Guid("00000005-0000-0000-0000-000000000000"),
				new Guid("00000005-0000-0000-0000-000000000001"),
				new Guid("00000005-0000-0000-0000-000000000002"),
				new Guid("00000005-0000-0000-0000-000000000003"),
				new Guid("00000005-0000-0000-0000-000000000004")
			}, new[]
			{
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000005")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000006")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000007")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000008")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-000000000009")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-00000000000A")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-00000000000B")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-00000000000C")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-00000000000D")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-00000000000E")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0001-0000-0000-000000000000"), new Guid("00000000-0000-0001-0000-000000000000"), new Guid("00000001-0000-0000-0000-00000000000F")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000005")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000006")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000007")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000008")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000009")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-00000000000A")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-00000000000B")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-00000000000C")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-00000000000D")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-00000000000E")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0002-0000-0000-000000000000"), new Guid("00000000-0000-0002-0000-000000000000"), new Guid("00000002-0000-0000-0000-00000000000F")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000005")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000006")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000007")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000008")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000009")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-00000000000A")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-00000000000B")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-00000000000C")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-00000000000D")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-00000000000E")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0003-0000-000000000000"), new Guid("00000003-0000-0000-0000-00000000000F")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000005")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000006")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000007")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000008")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000009")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-00000000000A")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-00000000000B")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-00000000000C")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-00000000000D")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-00000000000E")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0003-0000-0000-000000000000"), new Guid("00000000-0000-0004-0000-000000000000"), new Guid("00000004-0000-0000-0000-00000000000F")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000005")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000006")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000007")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000008")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000009")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-00000000000A")),
				Tuple.Create<string, Guid?, Guid, Guid>("RQ", new Guid("00000000-0004-0000-0000-000000000000"), new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-00000000000B")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-00000000000C")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-00000000000D")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-00000000000E")),
				Tuple.Create<string, Guid?, Guid, Guid>(null, null, new Guid("00000000-0000-0005-0000-000000000000"), new Guid("00000005-0000-0000-0000-00000000000F"))
			});
		}

		void TestBlockedRowsReading(Guid entityPk, IEnumerable<Guid> pksToBlock, Tuple<string, Guid?, Guid, Guid>[] expectedPks)
		{
			// Arrange
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.DefaultCommandTimeOutInSeconds = 5;
				CreateData(connection);

				using (var blockingConnection = Db.NewExtraConnectionToMainDb())
				{
					using (blockingConnection.BeginTransactionWithManager())
					{
						using (var command = blockingConnection.Command(BlockingQueryPattern(pksToBlock)))
						{
							command.ExecuteNonQuery();
						}

						// Act
						var result = DataUtils.GetDataTableFromQuery(connection, ActQuery(entityPk)).Rows.Cast<DataRow>();

						// Assert
						Assert(result, expectedPks);
					}
				}
			}
		}

		void TestEntity(Guid entityPk, Tuple<string, Guid?, Guid, Guid>[] expectedPks)
		{
			// Arrange
			CreateData();

			// Act
			var result = DataUtils.GetDataTableFromQuery(TestConnection, ActQuery(entityPk)).Rows.Cast<DataRow>();

			// Assert
			Assert(result, expectedPks);
		}

		protected abstract string ActQuery(Guid entityPk);
		protected abstract void Assert(IEnumerable<DataRow> result, Tuple<string, Guid?, Guid, Guid>[] expectedPks);
		protected abstract string BlockingQueryPattern(IEnumerable<Guid> pksToBlock);
	}
}

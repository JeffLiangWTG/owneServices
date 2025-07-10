using System;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics.Test
{
	[TestedType(typeof(TelEdge_RimRegistrationTrigger))]
	class TelEdge_RimRegistrationTriggerTest : DBCreateTriggerScriptTest
	{
		readonly ImmutableArray<Guid> RefEquipment = new[]
		{
			Guid.Parse("00000000-1000-1000-0000-000000000000"),
			Guid.Parse("00000000-2000-2000-0000-000000000000"),
		}.ToImmutableArray();

		readonly ImmutableArray<Guid> Devices = new[]
		{
			Guid.Parse("00000000-0000-0000-1111-000000000000"),
			Guid.Parse("00000000-0000-0000-2222-000000000000"),
		}.ToImmutableArray();

		void SaveToDB(Guid refEquipment, Guid device, DateTimeOffset dateTime)
		{
			new TelEdge
			{
				TE_EntityIdFrom = refEquipment,
				TE_EntityIdTo = device,
				TE_EntityTableCodeFrom = "RQ",
				TE_EntityTableCodeTo = "V3",
				TE_RelationshipType = "RIM",
				TE_StartTime = dateTime
			}.Insert(TestConnection);
		}

		[UseSnapshotProtection]
		public void TestTriggerPreventsNewRegistrationForDevicesAlreadyRegistered()
		{
			// Arrange
			SaveToDB(RefEquipment[0], Devices[0], new DateTimeOffset(new DateTime(2017, 2, 1), TimeSpan.Zero));

			// Act
			AssertNoExceptionThrown("", () => SaveToDB(RefEquipment[1], Devices[1], new DateTimeOffset(new DateTime(2017, 2, 1), TimeSpan.Zero)));
			AssertNoExceptionThrown("", () => SaveToDB(RefEquipment[0], Devices[0], new DateTimeOffset(new DateTime(2017, 2, 3), TimeSpan.Zero)));

			// Assert
			var result = TelEdge.ShallowLoadFromDB(TestConnection)
				.Select(row =>
				(
					TE_EntityTableCodeFrom: row.TE_EntityTableCodeFrom,
					TE_EntityIdFrom: row.TE_EntityIdFrom,
					TE_EntityTableCodeTo: row.TE_EntityTableCodeTo,
					TE_EntityIdTo: row.TE_EntityIdTo,
					TE_StartTime: row.TE_StartTime
				))
				.ToArray();

			AssertArrayEqualsByElements(new[]
			{
				(
					TE_EntityTableCodeFrom: "RQ",
					TE_EntityIdFrom: RefEquipment[0],
					TE_EntityTableCodeTo: "V3",
					TE_EntityIdTo: Devices[0],
					TE_StartTime: DateTimeOffset.Parse("1/02/2017 12:00:00 AM +00:00")
				),
				(
					TE_EntityTableCodeFrom: "RQ",
					TE_EntityIdFrom: RefEquipment[1],
					TE_EntityTableCodeTo: "V3",
					TE_EntityIdTo: Devices[1],
					TE_StartTime: DateTimeOffset.Parse("1/02/2017 12:00:00 AM +00:00")
				),
			}, result);
		}

		[UseSnapshotProtection]
		public void TestAuditColumnsAreCopied()
		{
			var now = DateTime.UtcNow.ToSmallDateTimeFloor();
			var telEdge = new TelEdge
			{
				TE_EntityIdFrom = RefEquipment[0],
				TE_EntityIdTo = Devices[0],
				TE_EntityTableCodeFrom = "RQ",
				TE_EntityTableCodeTo = "V3",
				TE_RelationshipType = "RIM",
				TE_StartTime = new DateTimeOffset(new DateTime(2017, 2, 1)),
				TE_SystemCreateTimeUtc = now,
				TE_SystemLastEditTimeUtc = now.AddDays(1),
				TE_SystemCreateUser = "A",
				TE_SystemLastEditUser = "B"
			}.InsertAndReturnObject(TestConnection);

			TelEdge.AssertFromDB(TestConnection, telEdge.PK)
				.ExpectEquals(nameof(TelEdge.TE_SystemCreateTimeUtc), te => te.TE_SystemCreateTimeUtc, now)
				.ExpectEquals(nameof(TelEdge.TE_SystemLastEditTimeUtc), te => te.TE_SystemLastEditTimeUtc, now.AddDays(1))
				.ExpectEquals(nameof(TelEdge.TE_SystemCreateUser), te => te.TE_SystemCreateUser, "A")
				.ExpectEquals(nameof(TelEdge.TE_SystemLastEditUser), te => te.TE_SystemLastEditUser, "B")
				.VerifyAll();
		}

		class TelEdge : SQLDataObject<TelEdge>
		{
			public TelEdge()
			{
				TE_SystemCreateTimeUtc = DateTime.UtcNow;
				TE_SystemLastEditTimeUtc = DateTime.UtcNow;
				TE_SystemCreateUser = "~BP";
				TE_SystemLastEditUser = "~BP";
			}

			public string TE_EntityTableCodeFrom { get; set; }
			public Guid TE_EntityIdFrom { get; set; }
			public string TE_EntityTableCodeTo { get; set; }
			public Guid TE_EntityIdTo { get; set; }
			public DateTimeOffset TE_StartTime { get; set; }
			public DateTimeOffset? TE_EndTime { get; set; }
			public byte TE_TemplateMapping { get; set; }
			public string TE_RelationshipType { get; set; }
			public DateTime? TE_SystemCreateTimeUtc { get; set; }
			public DateTime? TE_SystemLastEditTimeUtc { get; set; }
			public string TE_SystemCreateUser { get; set; }
			public string TE_SystemLastEditUser { get; set; }
		}
	}
}

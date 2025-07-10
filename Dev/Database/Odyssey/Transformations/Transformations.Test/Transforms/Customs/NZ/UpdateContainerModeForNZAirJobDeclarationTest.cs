using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.NZ;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.NZ
{
	[TestedType(typeof(UpdateContainerModeForNZAirJobDeclaration))]
	sealed class UpdateContainerModeForNZAirJobDeclarationTest : DataTransformationTestCase
	{
		public override void TestNewIndex()
		{
			new TransformationTestDataCreator().CreateGlbCompany("~NZ", "NZ");
			base.TestNewIndex();
		}

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update JE_ContainerMode For NZ Air JobDeclaration_1] ON [dbo].[JobDeclaration] ([JE_JS], [JE_TransportMode], [JE_ContainerMode], [JE_DataModel]) INCLUDE ([JE_SystemLastEditTimeUtc], [JE_SystemLastEditUser]) WHERE ([JE_JS] IS NULL AND [JE_TransportMode]='AIR' AND [JE_ContainerMode]<>'' AND [JE_DataModel]='NZ') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void AssertTransformationResults()
		{
			var sql = "SELECT JE_PK FROM JobDeclaration WHERE JE_ContainerMode = ''";

			var resultList = new List<Tuple<Guid>>();
			using (var command = Db.Connection.Command(sql))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					resultList.Add(Tuple.Create(reader.GetGuid(0)));
				}
			}

			AssertEquals(3, resultList.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				Tuple.Create(declarationPK1),
				Tuple.Create(declarationPK2),
				Tuple.Create(declarationPK3),
			}, resultList);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateContainerModeForNZAirJobDeclaration();
		}

		protected override void PrepareTestData()
		{
			declarationPK1 = Guid.NewGuid();
			declarationPK2 = Guid.NewGuid();
			declarationPK3 = Guid.NewGuid();
			var declarationPK4 = Guid.NewGuid();
			var declarationPK5 = Guid.NewGuid();
			var declarationPK6 = Guid.NewGuid();
			var declarationPK7 = Guid.NewGuid();

			var sqlText = @"
DECLARE @NZCompanyPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_SystemCreateTimeUtc, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser, GC_SystemCreateUser) VALUES (@NZCompanyPK, 'CNZ', 'NZ company', 'NZ', GetUtcDate(), GetUtcDate(), 'E', 'E');
DECLARE @NZBranchPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO GlbBranch(GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser, GB_SystemCreateUser) VALUES(@NZBranchPK, @NZCompanyPK, 'BNZ', GetUtcDate(), GetUtcDate(), 'E', 'E');
DECLARE @ShipmentPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser) VALUES(@ShipmentPK, 'B0001', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DeclarationReference, JE_DataModel, JE_TransportMode, JE_MessageType, JE_ContainerMode, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
	VALUES (@declarationPK1, @NZBranchPK, @NZCompanyPK, 1, 'Ref1', 'NZ', 'AIR', 'IMP', '', '2024-02-29 17:12:00', 'AAA', '2024-02-29 17:12:00', 'AAA');
INSERT INTO JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DeclarationReference, JE_DataModel, JE_TransportMode, JE_MessageType, JE_ContainerMode, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
	VALUES (@declarationPK2, @NZBranchPK, @NZCompanyPK, 2, 'Ref2', 'NZ', 'AIR', 'EXP', 'CNT', '2024-02-29 17:14:00', 'BBB', '2024-02-29 17:14:00', 'BBB');
INSERT INTO JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DeclarationReference, JE_DataModel, JE_TransportMode, JE_MessageType, JE_ContainerMode, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
	VALUES (@declarationPK3, @NZBranchPK, @NZCompanyPK, 3, 'Ref3', 'NZ', 'AIR', 'IMP', 'CNT', '2024-02-29 17:15:00', 'CCC', '2024-02-29 17:15:00', 'CCC');
INSERT INTO JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DeclarationReference, JE_DataModel, JE_TransportMode, JE_MessageType, JE_ContainerMode, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
	VALUES (@declarationPK4, @NZBranchPK, @NZCompanyPK, 4, 'Ref4', 'NZ', 'PST', 'IMP', 'TST', '2024-02-29 17:16:00', 'DDD', '2024-02-29 17:16:00', 'DDD');
INSERT INTO JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DeclarationReference, JE_DataModel, JE_TransportMode, JE_MessageType, JE_ContainerMode, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
	VALUES (@declarationPK5, @NZBranchPK, @NZCompanyPK, 5, 'Ref5', 'NZ', 'SEA', 'IMP', 'TST', '2024-02-29 17:17:00', 'EEE', '2024-02-29 17:17:00', 'EEE');
INSERT INTO JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DeclarationReference, JE_DataModel, JE_TransportMode, JE_MessageType, JE_ContainerMode, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
	VALUES (@declarationPK6, @NZBranchPK, @NZCompanyPK, 6, 'Ref6', 'FR', 'AIR', 'IMP', 'TST', '2024-02-29 17:18:00', 'FFF', '2024-02-29 17:18:00', 'FFF');
INSERT INTO JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DeclarationReference, JE_DataModel, JE_TransportMode, JE_MessageType, JE_ContainerMode, JE_JS, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
	VALUES (@declarationPK7, @NZBranchPK, @NZCompanyPK, 7, 'Ref7', 'NZ', 'AIR', 'IMP', 'CNT', @ShipmentPK, '2024-02-29 17:15:00', 'CCC', '2024-02-29 17:15:00', 'CCC');
";
			var cmd = Db.Connection.Command(sqlText);
			cmd.AddParameter("@declarationPK1", SqlDbType.UniqueIdentifier, declarationPK1);
			cmd.AddParameter("@declarationPK2", SqlDbType.UniqueIdentifier, declarationPK2);
			cmd.AddParameter("@declarationPK3", SqlDbType.UniqueIdentifier, declarationPK3);
			cmd.AddParameter("@declarationPK4", SqlDbType.UniqueIdentifier, declarationPK4);
			cmd.AddParameter("@declarationPK5", SqlDbType.UniqueIdentifier, declarationPK5);
			cmd.AddParameter("@declarationPK6", SqlDbType.UniqueIdentifier, declarationPK6);
			cmd.AddParameter("@declarationPK7", SqlDbType.UniqueIdentifier, declarationPK7);
			cmd.ExecuteNonQuery();
		}

		Guid declarationPK1;
		Guid declarationPK2;
		Guid declarationPK3;
	}
}

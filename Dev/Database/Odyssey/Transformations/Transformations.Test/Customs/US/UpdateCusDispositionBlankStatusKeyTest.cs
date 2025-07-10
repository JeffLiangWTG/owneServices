using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.US;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.US
{
	[TestedType(typeof(UpdateCusDispositionBlankStatusKey))]
	public class UpdateCusDispositionBlankStatusKeyTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateCusDispositionBlankStatusKey();

		Guid pk1 = Guid.NewGuid();
		Guid pk2 = Guid.NewGuid();
		Guid pk3 = Guid.NewGuid();
		Guid pk4 = Guid.NewGuid();

		protected override void PrepareTestData()
		{
			CreateCusDisposition(pk1, "CH", "AES", "Blank");
			CreateCusDisposition(pk2, "JE", "PES", "AMS");
			CreateCusDisposition(pk3, "CH", "AES", "Space");
			CreateCusDisposition(pk4, "CH", "AES", "W");
		}

		void CreateCusDisposition(Guid pk, string parentTableCode, string type, string statusKey)
		{
			var sql = @"
				INSERT INTO dbo.CusDisposition 
					(CDI_PK, CDI_ParentID, CDI_ParentTableCode, CDI_Type, CDI_StatusKey, CDI_Status, CDI_StatusDate, CDI_Sequence, CDI_Notes, CDI_SystemCreateTimeUtc, CDI_SystemCreateUser, CDI_SystemLastEditTimeUtc, CDI_SystemLastEditUser) 
				VALUES 
					(@CDI_PK, @CDI_ParentID, @CDI_ParentTableCode, @CDI_Type, @CDI_StatusKey, '888', GETDATE(), 1, '', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@CDI_PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@CDI_ParentID", SqlDbType.UniqueIdentifier, Guid.Empty);
				cmd.AddParameter("@CDI_ParentTableCode", SqlDbType.VarChar, parentTableCode);
				cmd.AddParameter("@CDI_Type", SqlDbType.VarChar, type);
				cmd.AddParameter("@CDI_StatusKey", SqlDbType.VarChar, statusKey);
				cmd.ExecuteNonQuery();
			}
		}

		protected override void AssertTransformationResults()
		{
			using (var cmd = Db.Connection.Command("SELECT * FROM dbo.CusDisposition"))
			using (var reader = cmd.ExecuteReader())
			{
				var dt = new DataTable();
				dt.Load(reader);

				AssertEquals(4, dt.Rows.Count);
				AssertEquals(1, dt.Select($"CDI_PK = '{pk1}' AND CDI_ParentTableCode = 'CH' AND CDI_Type = 'AES' AND CDI_StatusKey = 'Space'").Length);
				AssertEquals(1, dt.Select($"CDI_PK = '{pk2}' AND CDI_ParentTableCode = 'JE' AND CDI_Type = 'PES' AND CDI_StatusKey = 'AMS'").Length);
				AssertEquals(1, dt.Select($"CDI_PK = '{pk3}' AND CDI_ParentTableCode = 'CH' AND CDI_Type = 'AES' AND CDI_StatusKey = 'Space'").Length);
				AssertEquals(1, dt.Select($"CDI_PK = '{pk4}' AND CDI_ParentTableCode = 'CH' AND CDI_Type = 'AES' AND CDI_StatusKey = 'W'").Length);
			}
		}

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update CusDisposition StatusKey column_1] ON [dbo].[CusDisposition] ([CDI_ParentTableCode], [CDI_Type], [CDI_StatusKey]) INCLUDE ([CDI_SystemLastEditTimeUtc], [CDI_SystemLastEditUser]) WHERE ([CDI_ParentTableCode]='CH' AND [CDI_Type]='AES' AND [CDI_StatusKey]='Blank') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};
	}
}

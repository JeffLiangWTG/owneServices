using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.SG.Testing
{
	[TestedType(typeof(CopyTradersRemarksToCusSupportingInfo))]
	public class CopyTradersRemarksToCusSupportingInfoTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var resultList = new List<(Guid ParentID, string ParentTableCode, string Description, int LineNo)>();
			var sqlText = "SELECT CSI_ParentID, CSI_ParentTableCode, CSI_Description, CSI_LineNo FROM CusSupportingInfo WHERE CSI_Type = 'TRK' AND CSI_RN_NKCountryCode = 'SG'";
			using (var cmd = Db.Connection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					resultList.Add((
						reader.GetGuid(0),
						reader.GetString(1),
						reader.GetString(2),
						reader.GetInt32(3)
						));
				}
			}

			AssertContainsExactElementsInAnyOrder(new[]
			{
				(n1_Parent_PK, "JE", "first line", 1),
				(n1_Parent_PK, "JE", "second line", 2),
				(n2_Parent_PK, "JE", new string('a', 512), 1),
				(n2_Parent_PK, "JE", new string('a', 512), 2),
				(n2_Parent_PK, "JE", "a", 3),
				(n2_Parent_PK, "JE", "third line", 4),
			}, resultList);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new CopyTradersRemarksToCusSupportingInfo();

		protected override void PrepareTestData()
		{
			string n1_Text = $"first line{Environment.NewLine}second line";
			string n2_Text = new string('a', 1025) + $"{Environment.NewLine}third line";

			var sql = $@"
				DECLARE @companyPK UNIQUEIDENTIFIER = NEWID()
				DECLARE @branchPK UNIQUEIDENTIFIER = NEWID()
				INSERT INTO dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)
				VALUES (@companyPK, 'SG', 'BLO', 'SGC', 'SG company', '2023-01-01 01:01:00', 'E', '2023-01-01 01:01:00', 'E');
				INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser)
				VALUES (@branchPK, @companyPK, 'XXX', '', '2023-01-01 01:01:00', 'E', '2023-01-01 01:01:00', 'E');

				INSERT INTO dbo.JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DeclarationReference, JE_DataModel, JE_MessageType, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
				VALUES
					('{n1_Parent_PK}', @branchPK, @companyPK, 1, 'Ref1', 'SG', 'IMP', '2023-01-01 01:01:00', 'E', '2023-01-01 01:01:00', 'E'),
					('{n2_Parent_PK}', @branchPK, @companyPK, 2, 'Ref2', 'SG', 'IMP', '2023-01-01 01:01:00', 'E', '2023-01-01 01:01:00', 'E');

				INSERT INTO dbo.StmNote(ST_PK, ST_ParentID, ST_Table, ST_NoteText, ST_NoteType, ST_NoteContext, ST_Description, ST_SystemCreateTimeUtc, ST_SystemCreateUser, ST_SystemLastEditTimeUtc, ST_SystemLastEditUser)
				VALUES
					(NEWID(), '{n1_Parent_PK}', 'JobDeclaration', '{n1_Text}', 'DOC', 'AAA', 'SG Traders Remarks', '2023-01-01 01:01:00', 'E', '2023-01-01 01:01:00', 'E'),
					(NEWID(), '{n2_Parent_PK}', 'JobDeclaration', '{n2_Text}', 'DOC', 'AAA', 'SG Traders Remarks', '2023-01-01 01:01:00', 'E', '2023-01-01 01:01:00', 'E');
				";

			TestConnection.ExecuteNonQuery(sql);
		}

		Guid n1_Parent_PK = Guid.NewGuid();
		Guid n2_Parent_PK = Guid.NewGuid();
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.IE;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.IE
{
	[TestedType(typeof(UpdateCGL_AdditionalIdentifierForUCC5))]
	public class UpdateCGL_AdditionalIdentifierForUCC5Test : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var sql = $"SELECT CGL_AdditionalIdentifier, CGL_CustomsOffice FROM dbo.CusGoodsLocation WHERE CGL_PK IN ('{goodsLocationPK1}', '{goodsLocationPK2}')";

			var resultList = new List<Tuple<string, string>>();
			using (var command = Db.Connection.Command(sql))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					resultList.Add(Tuple.Create(reader.GetString(0), reader.GetString(1)));
				}
			}
			AssertEquals(2, resultList.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				Tuple.Create("", "IELMK200"),
				Tuple.Create("IELMK300", ""),
			}, resultList);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateCGL_AdditionalIdentifierForUCC5();
		}

		protected override void PrepareTestData()
		{
			var declarationPk = Guid.NewGuid();
			goodsLocationPK1 = Guid.NewGuid();
			goodsLocationPK2 = Guid.NewGuid();

			var sqlText = @"
DECLARE @IECompanyPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)
	VALUES (@IECompanyPK, 'CIE', 'IE company', 'IE', GETDATE(), 'AAA', GETDATE(), 'AAA');
DECLARE @IEBranchPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO GlbBranch(GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser)
	VALUES(@IEBranchPK, @IECompanyPK, 'BIE', GETDATE(), 'AAA', GETDATE(), 'AAA');

INSERT INTO JobDeclaration(JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DataModel, JE_ApplicationCode, JE_MessageType, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
	VALUES (@declarationPK, @IEBranchPK, @IECompanyPK, 1, 'IE', 'V1', 'IMP', '2024-07-09 17:12:00', 'AAA', '2024-07-09 17:12:00', 'AAA');

INSERT INTO dbo.CusGoodsLocation (CGL_PK, CGL_ParentID, CGL_ParentTableCode, CGL_LocationUse, CGL_Qualifier, CGL_Type, CGL_AdditionalIdentifier, CGL_SystemCreateTimeUtc, CGL_SystemCreateUser, CGL_SystemLastEditTimeUtc, CGL_SystemLastEditUser, CGL_CustomsOffice)
	VALUES (@goodsLocationPK1, @declarationPk, 'JE', 'DEC', 'U', 'B', 'IELMK200', '2024-07-09 17:12:00', 'AAA', '2024-07-09 17:12:00', 'AAA', '');

INSERT INTO dbo.CusGoodsLocation (CGL_PK, CGL_ParentID, CGL_ParentTableCode, CGL_LocationUse, CGL_Qualifier, CGL_Type, CGL_AdditionalIdentifier, CGL_SystemCreateTimeUtc, CGL_SystemCreateUser, CGL_SystemLastEditTimeUtc, CGL_SystemLastEditUser, CGL_CustomsOffice)
	VALUES (@goodsLocationPK2, @declarationPk, 'JE', 'DEC', '', '', 'IELMK300', '2024-07-09 17:12:00', 'AAA', '2024-07-09 17:12:00', 'AAA', '');
";
			var cmd = Db.Connection.Command(sqlText);
			cmd.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPk);
			cmd.AddParameter("@goodsLocationPK1", SqlDbType.UniqueIdentifier, goodsLocationPK1);
			cmd.AddParameter("@goodsLocationPK2", SqlDbType.UniqueIdentifier, goodsLocationPK2);
			cmd.ExecuteNonQuery();
		}
		Guid goodsLocationPK1, goodsLocationPK2;
	}
}

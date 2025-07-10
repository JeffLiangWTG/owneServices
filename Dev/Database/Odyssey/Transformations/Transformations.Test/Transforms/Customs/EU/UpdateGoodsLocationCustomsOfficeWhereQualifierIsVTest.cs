using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.EU
{
	[TestedType(typeof(UpdateGoodsLocationCustomsOfficeWhereQualifierIsV))]
	class UpdateGoodsLocationCustomsOfficeWhereQualifierIsVTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateGoodsLocationCustomsOfficeWhereQualifierIsV();

		public void TestUserDescription()
		{
			AssertEquals("Update CGL_CustomsOffice With values from CGL_AdditionalIdentifier when CGL_Qualifier = V.", GetNewTestTransformationInstance().UserDescription);
		}

		protected override void PrepareTestData()
		{
			Db.Connection.ExecuteNonQuery(@"ALTER TABLE dbo.CusGoodsLocation DROP CONSTRAINT Constraint_CGL_CustomsOffice");

			_ = TestConnection.ExecuteNonQuery($@"
				declare @cglPK1				UNIQUEIDENTIFIER = '{cglPK1}',
						@cglPK2				UNIQUEIDENTIFIER = '{cglPK2}',
						@cglPK3				UNIQUEIDENTIFIER = '{cglPK3}',
						@cglPK4				UNIQUEIDENTIFIER = '{cglPK4}',
						@cglPK5				UNIQUEIDENTIFIER = '{cglPK5}',
						@cglPK6				UNIQUEIDENTIFIER = '{cglPK6}'

				insert into dbo.CusGoodsLocation (CGL_PK, CGL_ParentID, CGL_ParentTableCode, CGL_LocationUse, CGL_Qualifier, CGL_CustomsOffice, CGL_AdditionalIdentifier, CGL_SystemCreateTimeUtc, CGL_SystemCreateUser, CGL_SystemLastEditTimeUtc, CGL_SystemLastEditUser)
				values
				(@cglPK1, NEWID(), 'CEI', 'DEP', 'X', '', 'FR110110', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
				(@cglPK2, NEWID(), 'CEI', 'CEI', 'V', '','FR220220', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
				(@cglPK3, NEWID(), 'CEI', 'CEI', 'V', 'FR200023', 'FR230230', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
				(@cglPK4, NEWID(), 'CEI', 'CEI', 'V','FR200024', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
				(@cglPK5, NEWID(), 'CEI', 'CEI', 'V','', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
				(@cglPK6, NEWID(), 'CEI', 'CEI', 'V','', 'FR200024_OVERLONG', GetUtcDate(), '~BP', GetUtcDate(), '~BP')"
				);
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals("The length of CGL_CustomsOffice should be 10 when this transformation being performed.", (Int16)10, TestConnection.ExecuteScalar("SELECT COL_LENGTH('CusGoodsLocation', 'CGL_CustomsOffice')"));
			var resultList = new List<Tuple<Guid, string, string>>();
			TestConnection.ExecuteReader($"SELECT * FROM dbo.CusGoodsLocation where CGL_PK in ('{cglPK1}', '{cglPK2}', '{cglPK3}', '{cglPK4}', '{cglPK5}', '{cglPK6}')", reader => resultList.Add(Tuple.Create((Guid)reader["CGL_PK"], (string)reader["CGL_CustomsOffice"], (string)reader["CGL_AdditionalIdentifier"])));
			AssertContainsExactElementsInAnyOrder("CusGoodsLocation", new[] { $"{cglPK1}, , FR110110", $"{cglPK2}, FR220220, ", $"{cglPK3}, FR200023, FR230230", $"{cglPK4}, FR200024, ", $"{cglPK5}, , ", $"{cglPK6}, FR200024_O, " }, resultList.Select(x => $"{x.Item1}, {x.Item2}, {x.Item3}").ToArray());
		}

		Guid cglPK1 = Guid.NewGuid();
		Guid cglPK2 = Guid.NewGuid();
		Guid cglPK3 = Guid.NewGuid();
		Guid cglPK4 = Guid.NewGuid();
		Guid cglPK5 = Guid.NewGuid();
		Guid cglPK6 = Guid.NewGuid();
	}
}

using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Ecommerce;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Ecommerce
{
	[TestedType(typeof(DeleteProductCodesLookupRoleTransformation))]
	public class DeleteProductCodesLookupRoleTransformationTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			Assert(!Db.Connection.Exists(@"FROM dbo.GlbGroupRole WHERE GGR_RoleName = 'ProductCodesLookup'"));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new DeleteProductCodesLookupRoleTransformation();

		protected override void PrepareTestData()
		{
			var gR_PK = new Guid();
			var gGR_PK = new Guid();
			Db.Connection.ExecuteNonQuery(@$"
				INSERT INTO [dbo].[GlbGroup]
				   ([GG_PK]
				   ,[GG_IsValid]
				   ,[GG_Code]
				   ,[GG_Desc]
				   ,[GG_IsSystemDefined]
				   ,[GG_IsSales]
				   ,[GG_IsActive]
				   ,[GG_SystemCreateUser]
				   ,[GG_SystemCreateTimeUtc]
				   ,[GG_SystemLastEditUser]
                   ,[GG_SystemLastEditTimeUtc]
				   ,[GG_IsSecurityEnabled]
				   ,[GG_DomainName]
				   ,[GG_AutoVersion]
				   ,[GG_Category]
				   ,[GG_ExternalId]
				   ,[GG_Type])
			    VALUES
					('{gR_PK.ToString()}'
					,1
					,'TESTGROUP'
					,'test group'
					,0
					,0
					,1
					,'E'
					,'2023-08-17 22:41:00'
					,'E'
					,'2023-08-17 22:41:00'
					,0
					,''
					,0
					,''
					,''
					,'ORG');");
			Db.Connection.ExecuteNonQuery(@$"
				INSERT INTO [dbo].[GlbGroupRole]
					([GGR_PK]
					,[GGR_RoleName]
					,[GGR_GG_Group]
					,[GGR_AutoVersion]
					,[GGR_SystemCreateTimeUtc]
					,[GGR_SystemCreateUser]
					,[GGR_SystemLastEditTimeUtc]
					,[GGR_SystemLastEditUser])
				VALUES
					('{gGR_PK.ToString()}'
					,'ProductCodesLookup'
					,'{gR_PK.ToString()}'
					,0
					,'2023-08-17 22:41:00'
					,'E'
					,'2023-08-17 22:41:00'
					,'E');");
			Assert(Db.Connection.Exists(@"FROM dbo.GlbGroup WHERE GG_Code = 'TESTGROUP'"));
			Assert(Db.Connection.Exists(@"FROM dbo.GlbGroupRole WHERE GGR_RoleName = 'ProductCodesLookup'"));
		}
	}
}

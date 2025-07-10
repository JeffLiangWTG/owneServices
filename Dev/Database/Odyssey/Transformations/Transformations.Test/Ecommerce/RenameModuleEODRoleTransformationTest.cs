using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Ecommerce;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Ecommerce
{
	[TestedType(typeof(RenameModuleEODRoleTransformation))]
	public class RenameModuleEODRoleTransformationTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new RenameModuleEODRoleTransformation();

		protected override void AssertTransformationResults()
		{
			Assert(Db.Connection.Exists(@"FROM dbo.GlbGroupRole WHERE GGR_RoleName = 'eCommerceOriginDepotViewer'"));
		}

		protected override void PrepareTestData()
		{
			var groupRole_PK1 = Guid.NewGuid();
			var group_FK = "de15a613-95de-48f4-878b-e6f451e3efeb";

			if (!EODModuleEntryExists())
			{
				Db.Connection.ExecuteNonQuery($@"
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
						('{groupRole_PK1.ToString()}'
						,'module-EOD'
						,'{group_FK}'
						,0
						,'2023-10-04 04:41:00'
						,'E'
						,'2023-10-04 04:41:00'
						,'E');
				");
			}
			Assert(EODModuleEntryExists());
		}

		bool EODModuleEntryExists()
		{
			return Db.Connection.Exists(@"FROM dbo.GlbGroupRole WHERE GGR_RoleName = 'module-EOD'");
		}
	}
}

using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Ecommerce;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Ecommerce
{
	[TestedType(typeof(RenameModuleEDPRoleTransformation))]
	public class RenameModuleEDPRoleTransformationTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new RenameModuleEDPRoleTransformation();

		protected override void AssertTransformationResults()
		{
			Assert(Db.Connection.Exists(@"FROM dbo.GlbGroupRole WHERE GGR_RoleName = 'eCommerceDestinationDepotViewer'"));
		}

		protected override void PrepareTestData()
		{
			var groupRole_PK1 = Guid.NewGuid();
			var group_FK = "8fd2f921-fc83-4de7-aeac-296a565b8a85";

			if (!EDPModuleEntryExists())
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
					,'module-EDP'
					,'{group_FK}'
					,0
					,'2023-10-04 04:41:00'
					,'E'
					,'2023-10-04 04:41:00'
					,'E');
			");
			}
			Assert(EDPModuleEntryExists());
		}

		bool EDPModuleEntryExists()
		{
			return Db.Connection.Exists(@"FROM dbo.GlbGroupRole WHERE GGR_RoleName = 'module-EDP'");
		}
	}
}

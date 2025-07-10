using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing
{
	[TestsSubclassesOf(typeof(DeleteRegistryItem))]
	public abstract class DeleteRegistryItemTest : DataTransformationTestCase
	{
		protected abstract string[] GetRegistryItemNames();

		protected override sealed DataTransformation GetNewTestTransformationInstance() =>
			(DataTransformation)Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType()));

		protected override void PrepareTestData()
		{
			const string insertSQL = @"IF NOT EXISTS (SELECT null FROM dbo.StmData WHERE SD_Name = @registryItemName)
INSERT dbo.StmData(SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_IsLogged, SD_BinaryValue, SD_GuidValue, SD_IsCancelled) VALUES
	(NEWID(), @registryItemName, NULL, NULL, 'BIN', 1, 0xFFFE3C003F0078006D006C002000760065007200730069006F006E003D00220031002E0030002200200065006E0063006F00640069006E0067003D0022007500740066002D003100360022003F003E003C00440069007300740061006E0063006500430061006C00630075006C006100740069006F006E00500072006F007600690064006500720043006F006E00660069006700750072006100740069006F006E003E003C00500072006F00760069006400650072003E00500043004D003C002F00500072006F00760069006400650072003E003C00560065007200730069006F006E003E004300550052003C002F00560065007200730069006F006E003E003C00430061006C00630075006C006100740069006F006E004D006500740068006F0064003E005000520041003C002F00430061006C00630075006C006100740069006F006E004D006500740068006F0064003E003C002F00440069007300740061006E0063006500430061006C00630075006C006100740069006F006E00500072006F007600690064006500720043006F006E00660069006700750072006100740069006F006E003E00, '00000000-0000-0000-0000-000000000000', 0)";

			using (DbCommand command = Db.Connection.Command(insertSQL)) // It is impossible to use BusinessObjectFactory here
			{
				foreach (var registryItemName in GetRegistryItemNames())
				{
					command.AddParameter("registryItemName", SqlDbType.VarChar, registryItemName);
					command.ExecuteNonQuery();
					command.RemoveParameterIfExists("registryItemName");
				}
			}
		}

		protected override void AssertTransformationResults()
		{
			using (DbCommand command = Db.Connection.Command("SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = @registryItemName")) // It is impossible to use BusinessObjectFactory here
			{
				foreach (var registryItemName in GetRegistryItemNames())
				{
					command.AddParameter("registryItemName", SqlDbType.VarChar, registryItemName);
					AssertEquals(0, command.ExecuteScalar());
					command.RemoveParameterIfExists("registryItemName");
				}
			}
		}
	}
}

using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Registry
{
	[TestedType(typeof(UpdateAviationSecurityTrainingRestrictionsEuDataTransformation))]
	public class UpdateAviationSecurityTrainingRestrictionsEuDataTransformationTest : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateAviationSecurityTrainingRestrictionsEuDataTransformation();
		}

		protected override void PrepareTestData()
		{
			var sql = @"INSERT dbo.StmData(SD_PK, SD_Name, SD_Type, SD_IsLogged, SD_BinaryValue, SD_IsCancelled) VALUES	(NEWID(), 'AviationSecurityTrainingRestrictions_EU', 'BOL', 1, 0x5400720075006500, 0);";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		protected override void AssertTransformationResults()
		{
			var data = Db.Connection.ExecuteScalar("SELECT SD_BinaryValue FROM StmData WHERE SD_Name = 'AviationSecurityTrainingRestrictions_EU' AND SD_Type = 'BIN'");
			AssertEquals("<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
						 "<AviationSecurityTrainingRestriction>" +
						 "<Enabled>Y</Enabled>" +
						 "<ApplyCertificationRestriction>N</ApplyCertificationRestriction>" +
						 "</AviationSecurityTrainingRestriction>", Encoding.Unicode.GetString((byte[])data));
		}
	}
}

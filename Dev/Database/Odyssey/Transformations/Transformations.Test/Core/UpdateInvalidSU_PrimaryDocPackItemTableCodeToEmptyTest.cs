using System;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core.Testing;

[TestedType(typeof(UpdateInvalidSU_PrimaryDocPackItemTableCodeToEmpty))]
public class UpdateInvalidSU_PrimaryDocPackItemTableCodeToEmptyTest : DataTransformationTestCase
{
	readonly Guid validSuGuid1 = Guid.NewGuid();
	readonly Guid validSuGuid2 = Guid.NewGuid();
	readonly Guid validSuGuid3 = Guid.NewGuid();
	readonly Guid invalidSuGuid = Guid.NewGuid();

	protected override void PrepareTestData()
	{
		Db.Connection.ExecuteNonQuery(@"IF (OBJECT_ID('Constraint_SU_PrimaryDocPackItemTableCode', 'C') IS NOT NULL)
										BEGIN
											ALTER TABLE dbo.StmMenuItem NOCHECK CONSTRAINT Constraint_SU_PrimaryDocPackItemTableCode
										END");
		var helper = new TestDbHelper(TestConnection);
		helper.Insert(StmMenuItemSchema.Constants.TableName, new
		{
			SU_PK = validSuGuid1,
			SU_BusinessContext = "Shipment",
			SU_PrimaryDocPackItemTableCode = "SI",
		});
		helper.Insert(StmMenuItemSchema.Constants.TableName, new
		{
			SU_PK = validSuGuid2,
			SU_BusinessContext = "Shipment2",
			SU_PrimaryDocPackItemTableCode = "SF",
		});
		helper.Insert(StmMenuItemSchema.Constants.TableName, new
		{
			SU_PK = validSuGuid3,
			SU_BusinessContext = "Shipment3",
			SU_PrimaryDocPackItemTableCode = "",
		});
		helper.Insert(StmMenuItemSchema.Constants.TableName, new
		{
			SU_PK = invalidSuGuid,
			SU_BusinessContext = "Consol",
			SU_PrimaryDocPackItemTableCode = "XX",
		});
	}

	protected override DataTransformation GetNewTestTransformationInstance()
	{
		return new UpdateInvalidSU_PrimaryDocPackItemTableCodeToEmpty();
	}

	protected override void AssertTransformationResults()
	{
		AssertEquals("SI", TestConnection.ExecuteScalar<string>($"SELECT SU_PrimaryDocPackItemTableCode FROM dbo.StmMenuItem WHERE SU_PK = '{validSuGuid1}'"));
		AssertEquals("SF", TestConnection.ExecuteScalar<string>($"SELECT SU_PrimaryDocPackItemTableCode FROM dbo.StmMenuItem WHERE SU_PK = '{validSuGuid2}'"));
		AssertEquals("", TestConnection.ExecuteScalar<string>($"SELECT SU_PrimaryDocPackItemTableCode FROM dbo.StmMenuItem WHERE SU_PK = '{validSuGuid3}'"));
		AssertEquals("", TestConnection.ExecuteScalar<string>($"SELECT SU_PrimaryDocPackItemTableCode FROM dbo.StmMenuItem WHERE SU_PK = '{invalidSuGuid}'"));
	}
}

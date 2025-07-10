using System.Text;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.TransitWarehouse.Test
{
	[TestedType(typeof(DeleteEmptyReferenceNumberAddOnValue))]
	public class DeleteEmptyReferenceNumberAddOnValueTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(GenCustomAddOnValue.CountInDB(TestConnection, t => t.PK == emptySourceTypeAddOnValue.PK), 0);
			AssertEquals(GenCustomAddOnValue.CountInDB(TestConnection, t => t.PK == emptyOtherTypeAddOnValue.PK), 1);
			AssertEquals(GenCustomAddOnValue.CountInDB(TestConnection, t => t.PK == notEmptySourceTypeAddOnValue.PK), 1);
			AssertEquals(GenCustomAddOnValue.CountInDB(TestConnection, t => t.PK == notCusEntryNumAddOnValue.PK), 1);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new DeleteEmptyReferenceNumberAddOnValue();

		protected override void PrepareTestData()
		{
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(Db.Connection, StmUniversalJobLinkSchema.Constants.SqlSchemaName, StmUniversalJobLinkSchema.Constants.TableName, "Constraint_UCL_ParentTableCode_NoCheck"))
			{
				var sql = new StringBuilder();

				var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
				var whs = new WhsWarehouse("WH1", branch1.PK) { WW_TransitSecurityProcessingRequired = false }.WithDockDoor(TestConnection);
				var rcn = new WhsItemReceiveConsignment(whs, "RCN1", "RCN1", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
				var cenReference = new CusEntryNum(rcn.PK, "WhsItemReceiveConsignment", "Test123", "CEN").AppendInsertAndReturnObject(sql);
				emptySourceTypeAddOnValue = new GenCustomAddOnValue(cenReference.PK, "CE", "STR", "SourceType", "").AppendInsertAndReturnObject(sql);
				emptyOtherTypeAddOnValue = new GenCustomAddOnValue(cenReference.PK, "CE", "STR", "Test", "").AppendInsertAndReturnObject(sql);
				var crnReference = new CusEntryNum(rcn.PK, "WhsItemReceiveConsignment", "Test234", "CRN").AppendInsertAndReturnObject(sql);
				notEmptySourceTypeAddOnValue = new GenCustomAddOnValue(crnReference.PK, "CE", "STR", "SourceType", "T1").AppendInsertAndReturnObject(sql);
				notCusEntryNumAddOnValue = new GenCustomAddOnValue(rcn.PK, "WRC", "STR", "SourceType", "").AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToString());
			}
		}

		GenCustomAddOnValue emptySourceTypeAddOnValue;
		GenCustomAddOnValue emptyOtherTypeAddOnValue;
		GenCustomAddOnValue notEmptySourceTypeAddOnValue;
		GenCustomAddOnValue notCusEntryNumAddOnValue;
	}
}

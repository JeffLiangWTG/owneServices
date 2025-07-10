using System;
using System.Linq;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Freight.ContainerYard;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Freight.ContainerYard
{
	[TestedType(typeof(PopulateAcceptanceAndReleaseNumber))]
	internal class PopulateAcceptanceAndReleaseNumberTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();
			DropConstraints();

			var warehouse = new WhsWarehouse("WH1", "CYD").WithDockDoor(TestConnection);

			var receiveAdvice = new CYDReceiveAdvice(warehouse, "JOB001")
			{
				YRA_FromDate = DateTime.Now.AddDays(10),
				YRA_ToDate = DateTime.Now.AddDays(30),
				YRA_AcceptanceNumber = ""
			}.AppendInsertAndReturnObject(sql);

			var releaseAdvice = new CYDReleaseAdvice(warehouse, "JOB002")
			{
				YRE_FromDate = DateTime.Now.AddDays(10),
				YRE_ToDate = DateTime.Now.AddDays(30),
				YRE_ReleaseNumber = ""
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertEquals("Pre-condition: Receive Advice Acceptance Number should be empty", "", receiveAdvice.YRA_AcceptanceNumber);
			AssertEquals("Pre-condition: Release Advice Release Number should be empty", "", releaseAdvice.YRE_ReleaseNumber);
		}

		protected override void AssertTransformationResults()
		{
			var receiveAdvice = CYDReceiveAdvice.ShallowLoadFromDB(TestConnection).FirstOrDefault();
			var releaseAdvice = CYDReleaseAdvice.ShallowLoadFromDB(TestConnection).FirstOrDefault();

			AssertEquals("Acceptance Number should be updated to JobNumber 'JOB001'", "JOB001", receiveAdvice?.YRA_AcceptanceNumber);

			AssertEquals("Release Number should be updated to JobNumber 'JOB002'", "JOB002", releaseAdvice?.YRE_ReleaseNumber);
		}
		void DropConstraints()
		{
			var sql = new StringBuilder();

			sql.AppendLine($@"
			IF EXISTS (
				SELECT 1 
				FROM sys.objects 
				WHERE object_id = OBJECT_ID('Constraint_YRA_AcceptanceNumber') AND type = 'C'
			)
			ALTER TABLE {CYDReceiveAdviceSchema.Constants.TableName} DROP CONSTRAINT Constraint_YRA_AcceptanceNumber;");

			sql.AppendLine($@"
			IF EXISTS (
				SELECT 1 
				FROM sys.objects 
				WHERE object_id = OBJECT_ID('Constraint_YRE_ReleaseNumber') AND type = 'C'
			)
			ALTER TABLE {CYDReleaseAdviceSchema.Constants.TableName} DROP CONSTRAINT Constraint_YRE_ReleaseNumber;");

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override DataTransformation GetNewTestTransformationInstance() =>
			new PopulateAcceptanceAndReleaseNumber();	
	}
}

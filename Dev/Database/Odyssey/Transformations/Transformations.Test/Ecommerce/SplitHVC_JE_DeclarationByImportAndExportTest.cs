using System;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations;
using Enterprise.DbUpgrader.Transformations.Transforms.Ecommerce;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.Transforms.Ecommerce.Testing
{
	[TestedType(typeof(SplitHVC_JE_DeclarationByImportAndExport))]
	class SplitHVC_JE_DeclarationByImportAndExportTest : DataTransformationTestCase
	{
		Guid consignmentWithIMPDeclaration;
		Guid consignmentWithEXPDeclaration;
		Guid consignmentWithNoDeclaration;

		Guid importDeclaration;
		Guid exportDeclaration;

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new SplitHVC_JE_DeclarationByImportAndExport();
		}

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, HVLVConsignmentSchema.Constants.TableName, "HVC_JE_Declaration", "uniqueidentifier");
			var testDataCreator = new TransformationTestDataCreator();

			var orgAddressPK = (Guid)Db.Connection.ExecuteScalar(@"SELECT TOP 1 OA_PK FROM dbo.OrgAddress");
			var bookingHeaderPK = testDataCreator.CreateHVLVBookingHeader(orgAddressPK, "M00000112", "B", 1);

			consignmentWithIMPDeclaration = testDataCreator.CreateHVLVConsignment(bookingHeaderPK, "HVC00001", "HVC00001", "B", 1, 1);
			consignmentWithEXPDeclaration = testDataCreator.CreateHVLVConsignment(bookingHeaderPK, "HVC00002", "HVC00002", "B", 1, 1);
			consignmentWithNoDeclaration = testDataCreator.CreateHVLVConsignment(bookingHeaderPK, "HVC00003", "HVC00003", "B", 1, 1);

			var companyPK = testDataCreator.CreateCompany("TC1", "CA");
			var branchPK = testDataCreator.CreateBranch("TB1", "CABLO", companyPK);
			var organizationPK = testDataCreator.CreateOrg("OrgCode1", "Company Name 1");

			importDeclaration = testDataCreator.CreateJobDeclaration(dataModel: "CA", branchPK, companyPK, organizationPK, "IMP", DateTime.Now, 1, messageStatus: "");
			exportDeclaration = testDataCreator.CreateJobDeclaration(dataModel: "CA", branchPK, companyPK, organizationPK, "EXP", DateTime.Now, 2, messageStatus: "");

			Db.Connection.ExecuteNonQuery(SetHVC_JE_DeclarationSql(importDeclaration, consignmentWithIMPDeclaration));
			Db.Connection.ExecuteNonQuery(SetHVC_JE_DeclarationSql(exportDeclaration, consignmentWithEXPDeclaration));
		}

		protected override void AssertTransformationResults()
		{
			CombineAssertions("HVC_JE_Declaration is correctly split into HVC_JE_ImportDeclaration and HVC_JE_ExportDeclaration", () =>
			{
				AssertDeclarations(consignmentWithIMPDeclaration, importDeclaration, DBNull.Value);
				AssertDeclarations(consignmentWithEXPDeclaration, DBNull.Value, exportDeclaration);
				AssertDeclarations(consignmentWithNoDeclaration, DBNull.Value, DBNull.Value);
			});
		}

		public void TestSyncTriggerIsCreatedAndDropped()
		{
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, HVLVConsignmentSchema.Constants.TableName, "HVC_JE_Declaration", "uniqueidentifier");

			var triggerName = "TG_HVLVConsignment_SyncDeclaration";
			Assert("Trigger should not exist", !DbObjectCreator.TriggerExists(TestConnection, HVLVConsignmentSchema.Constants.TableName, triggerName));

			var transform = GetNewTestTransformationInstance();

			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			Assert("Trigger should be created.", DbObjectCreator.TriggerExists(TestConnection, HVLVConsignmentSchema.Constants.TableName, triggerName));
		}

		public void TestTriggerWorksProperlyToPopulateNewDeclarationColumns()
		{
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, HVLVConsignmentSchema.Constants.TableName, "HVC_JE_Declaration", "uniqueidentifier");

			var mockManager = new Mock<IUpgradeManager>();
			mockManager.Setup(m => m.ShowInfoMessage(It.IsAny<string>()));
			mockManager.Setup(m => m.StartSubtask(It.IsAny<string>()));

			var transform = GetNewTestTransformationInstance();
			transform.Initialise(manager: mockManager.Object);
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var testDataCreator = new TransformationTestDataCreator();

			var orgAddressPK = (Guid)Db.Connection.ExecuteScalar(@"SELECT TOP 1 OA_PK FROM dbo.OrgAddress");
			var bookingHeaderPK = testDataCreator.CreateHVLVBookingHeader(orgAddressPK, "M00000114", "B", 1);

			var importConsignmentPK = testDataCreator.CreateHVLVConsignment(bookingHeaderPK, "HVC00004", "HVC00004", "B", 1, 1);
			var exportConsignmentPK = testDataCreator.CreateHVLVConsignment(bookingHeaderPK, "HVC00005", "HVC00005", "B", 1, 1);

			var companyPK = testDataCreator.CreateCompany("TC2", "BB");
			var branchPK = testDataCreator.CreateBranch("TB2", "CABCC", companyPK);
			var organizationPK = testDataCreator.CreateOrg("OrgCode2", "Company Name 2");

			var importDeclarationPK = testDataCreator.CreateJobDeclaration(dataModel: "CA", branchPK, companyPK, organizationPK, "IMP", DateTime.Now, 1, messageStatus: "");
			var exportDeclarationPK = testDataCreator.CreateJobDeclaration(dataModel: "CA", branchPK, companyPK, organizationPK, "EXP", DateTime.Now, 2, messageStatus: "");

			Db.Connection.ExecuteNonQuery(SetHVC_JE_DeclarationSql(importDeclarationPK, importConsignmentPK));
			Db.Connection.ExecuteNonQuery(SetHVC_JE_DeclarationSql(exportDeclarationPK, exportConsignmentPK));

			AssertDeclarations(importConsignmentPK, importDeclarationPK, DBNull.Value);
			AssertDeclarations(exportConsignmentPK, DBNull.Value, exportDeclarationPK);
		}

		string SetHVC_JE_DeclarationSql(Guid declarationPK, Guid consignmentPK)
		{
			return @$"
UPDATE
    dbo.HVLVConsignment
SET
    HVC_JE_Declaration = '{declarationPK}',
    HVC_SystemLastEditTimeUtc = GETUTCDATE(),
    HVC_SystemLastEditUser = '~BP'
WHERE
    HVC_PK = '{consignmentPK}'";
		}

		void AssertDeclarations(Guid consignmentPK, object expectedImportDeclaration, object expectedExportDeclaration)
		{
			var getResultSQL = $"SELECT HVC_JE_ImportDeclaration, HVC_JE_ExportDeclaration FROM dbo.HVLVConsignment WHERE HVC_PK = '{consignmentPK}'";
			using (var cmd = Db.Connection.Command(getResultSQL))
			using (var reader = cmd.ExecuteReader())
			{
				Assert("precondition: there should be one record", reader.Read());
				AssertEquals("Import Declaration", expectedImportDeclaration, reader["HVC_JE_ImportDeclaration"]);
				AssertEquals("Export Declaration", expectedExportDeclaration, reader["HVC_JE_ExportDeclaration"]);
			}
		}
	}
}


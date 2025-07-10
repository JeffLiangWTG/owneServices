using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.TR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.TR
{
	[TestedType(typeof(DeleteCusTRPreviousDocument))]
	class DeleteCusTRPreviousDocumentTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new DeleteCusTRPreviousDocument();

		protected override void AssertTransformationResults()
		{
			AssertEquals("All the data in CusTRPreviousDocumentItem should be removed", 0, TestConnection.ExecuteScalar<int>("SELECT count(0) FROM dbo.CusTRPreviousDocumentItem"));
			AssertEquals("All the data in CusTRPreviousDocument should be removed", 0, TestConnection.ExecuteScalar<int>("SELECT count(0) FROM dbo.CusTRPreviousDocument"));
		}

		protected override void PrepareTestData()
		{
			var creator = new TransformationTestDataCreator();
			var company = creator.CreateCompany("TR1", "TR");
			var branch = creator.CreateBranch("BR1", "BR001", company);
			var org = creator.CreateOrg("DTR");

			declaration = creator.CreateJobDeclaration("TR", branch, company, org, "IMP", DateTime.Now, 1, "");
			cusEntryHeader = creator.CreateCusEntryHeader("TR", declaration, 1);
			creator.CreateCusEntryNum(entryNum, cusEntryHeader, CusEntryHeaderSchema.Constants.TableName, "ENT001", "MTO", "TR");

			var sql = $@"INSERT INTO dbo.CusTRPreviousDocument (TPD_PK, TPD_ClusterKey, TPD_IsValid, TPD_DocumentNumber, TPD_SystemCreateTimeUtc, TPD_SystemCreateUser, TPD_SystemLastEditTimeUtc, TPD_SystemLastEditUser, TPD_CE_EntryNumber) 
VALUES ('{cusTRPreviousDocument}', 1, 1, '11', GETDATE(), '~BP', GETDATE(), '~BP', '{entryNum}')

INSERT INTO dbo.CusTRPreviousDocumentItem (TPI_PK, TPI_ClusterKey, TPI_IsValid, TPI_TPD, TPI_LineNumber, TPI_SystemCreateTimeUtc, TPI_SystemCreateUser, TPI_SystemLastEditTimeUtc, TPI_SystemLastEditUser) 
VALUES ('6F9619FF-8B86-D011-B42D-00CF4FC964FF', 1, 1, '{cusTRPreviousDocument}', 5 , GETDATE(), '~BP', GETDATE(), '~BP')

INSERT INTO dbo.CusTRPreviousDocumentItem (TPI_PK, TPI_ClusterKey, TPI_IsValid, TPI_TPD, TPI_LineNumber, TPI_SystemCreateTimeUtc, TPI_SystemCreateUser, TPI_SystemLastEditTimeUtc, TPI_SystemLastEditUser) 
VALUES ('D2D0FBE3-C0C2-4970-AF8B-C9A91A8F93DB', 1, 1, '{cusTRPreviousDocument}', 6 , GETDATE(), '~BP', GETDATE(), '~BP')";

			Db.Connection.ExecuteNonQuery(sql);
		}

		Guid cusTRPreviousDocument = Guid.NewGuid();
		Guid entryNum = Guid.NewGuid();
		Guid declaration;
		Guid cusEntryHeader;
	}
}

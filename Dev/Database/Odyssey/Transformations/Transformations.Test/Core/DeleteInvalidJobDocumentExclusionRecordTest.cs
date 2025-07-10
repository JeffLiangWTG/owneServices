using System;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Core;

[TestedType(typeof(DeleteInvalidJobDocumentExclusionRecord))]
public class DeleteInvalidJobDocumentExclusionRecordTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance()
	{
		return new DeleteInvalidJobDocumentExclusionRecord();
	}

	readonly string DNDNotifyMode = "DND";
	readonly string DummyContactName = "DUMMY CONTACT TO SUPPRESS DOCS";

	protected override void PrepareTestData()
	{
		var helper = new TestDbHelper(TestConnection);
		var orgHeader = helper.InsertOrgHeader("OH1", "OH1Name");
		var dummyContactGuid = helper.InsertOrgContact(DummyContactName, "test@wtg.com", false, orgHeader, DNDNotifyMode);
		var orgHeader1Contact1Doc = helper.InsertOrgDocument("AAA", null, dummyContactGuid, DNDNotifyMode);
		var newGuid = Guid.NewGuid();
		var orgDocumentWithNullODOC = Guid.NewGuid();
		helper.Insert(OrgDocumentSchema.Constants.TableName, new
		{
			OD_PK = orgDocumentWithNullODOC,
			OD_DocumentGroup = "AAA",
			OD_DeliverBy = DNDNotifyMode,
			OD_AttachmentType = "",
			OD_FilterShipmentMode = "ALL",
			OD_FilterDirection = "ALL",
			OD_IsValid = 1,
			OD_OH_Suppressed = orgHeader
		});

		helper.Insert(JobDocumentExclusionSchema.Constants.TableName, new
		{
			JDE_PK = Guid.NewGuid(),
			JDE_OD_Document = orgHeader1Contact1Doc,
			JDE_ParentID = newGuid,
			JDE_ParentTableCode = "JS"
		});
		helper.Insert(JobDocumentExclusionSchema.Constants.TableName, new
		{
			JDE_PK = Guid.NewGuid(),
			JDE_OD_Document = orgDocumentWithNullODOC,
			JDE_ParentID = newGuid,
			JDE_ParentTableCode = "JS"
		});
	}

	protected override void AssertTransformationResults()
	{
		var sql = @"SELECT COUNT(*)
FROM dbo.JobDocumentExclusion
JOIN dbo.OrgDocument ON dbo.JobDocumentExclusion.JDE_OD_Document = dbo.OrgDocument.OD_PK AND dbo.OrgDocument.OD_DeliverBy = 'DND'
LEFT JOIN dbo.OrgContact ON dbo.OrgDocument.OD_OC = dbo.OrgContact.OC_PK
WHERE dbo.OrgContact.OC_PK IS NULL OR dbo.OrgContact.OC_ContactName = 'DUMMY CONTACT TO SUPPRESS DOCS'";

		AssertEquals(0, TestConnection.ExecuteScalar<int>(sql));
	}
}
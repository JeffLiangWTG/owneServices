using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.DocumentScanning;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.DocumentScanning
{
	[TestedType(typeof(PopulateRT_ParseTypeInRefDocTypeTableOffline))]
	public class PopulateRT_ParseTypeInRefDocTypeTableOfflineTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var query = "Select RT_ParseType From dbo.RefDocType With(nolock) Where RT_IsSystem = {0} And RT_DocType = '{1}'";
			using (var adminConnection = Db.NewAdminConnection())
			{
				foreach (var docType in PopulateRT_ParseTypeInRefDocTypeTableOffline.ParseTypes.Keys)
				{
					var docTypes = DataUtils.GetDataTableFromQuery(adminConnection, string.Format(query, 1, docType));
					var expectedParseType = PopulateRT_ParseTypeInRefDocTypeTableOffline.ParseTypes[docType];
					AssertEquals($"{docType} should have one row in RefDocType table", 1, docTypes.Rows.Count);
					AssertEquals($"{docType} should have parse type {expectedParseType}", expectedParseType, docTypes.Rows[0]["RT_ParseType"]);
				}

				var nonSystemDocType = DataUtils.GetDataTableFromQuery(adminConnection, string.Format(query, 0, nonSystemDocTypeCode));
				AssertEquals("Non-system doc type shouldn't be updated", string.Empty, nonSystemDocType.Rows[0]["RT_ParseType"]);
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new PopulateRT_ParseTypeInRefDocTypeTableOffline();
		}

		protected override void PrepareTestData()
		{
			var sqlText = GetInsertNonSystemDocTypeSql(nonSystemDocTypeCode);
			_ = Db.Connection.Command(sqlText).ExecuteNonQuery();
		}

		const string nonSystemDocTypeCode = "PIN";

		string GetInsertNonSystemDocTypeSql(string docType)
		{
			return $@"
IF NOT EXISTS (SELECT 1 FROM dbo.RefDocType WHERE RT_IsSystem = 0 AND RT_DocType = '{docType}')
BEGIN
	INSERT INTO [dbo].[RefDocType]
		([RT_PK]
		,[RT_ReferenceType]
		,[RT_SE_NKDocumentReceivedEvent]
		,[RT_Desc]
		,[RT_HPPclPrintFile]
		,[RT_ForceUserToRead]
		,[RT_IsActive]
		,[RT_IsSystem]
		,[RT_IsCompanySpecific]
		,[RT_IsPublished]
		,[RT_IsPublishUpdatable]
		,[RT_SaveVersions]
		,[RT_LogSystemCreatedDocsToEDocs]
		,[RT_OriginalDocumentReleasedOnPayment]
		,[RT_IsBranchSpecific]
		,[RT_IsDepartmentSpecific]
		,[RT_AllowMultiplePeriodicDocs]
		,[RT_LogMacro]
		,[RT_AutoVersion]
		,[RT_SystemCreateTimeUtc]
		,[RT_SystemCreateUser]
		,[RT_SystemLastEditTimeUtc]
		,[RT_SystemLastEditUser]
		,[RT_DocType])
	VALUES
		(NewID()
		,'SCL'
		,''
		,''
		,0
		,0
		,1
		,0
		,0
		,0
		,1
		,0
		,1
		,0
		,0
		,0
		,0
		,''
		,0
		,GetUtcDate()
		,'~BP'
		,GetUtcDate()
		,'~BP'
		,'{docType}')
END;
";
		}
	}
}

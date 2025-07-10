using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.GB;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.GB;

[TestedType(typeof(EnableAllApplicationsToExistingTokens))]
class EnableAllApplicationsToExistingTokensTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() => new EnableAllApplicationsToExistingTokens();

	protected override void PrepareTestData()
	{
		var sql = @"
			INSERT INTO [dbo].[GlbExternalPassword] ([GP_PK], [GP_PasswordType], [GP_UserID], [GP_CurrentPassword], [GP_PasswordStatus], [GP_GC], [GP_SystemCreateTimeUtc], [GP_SystemCreateUser], [GP_SystemLastEditTimeUtc], [GP_SystemLastEditUser])
			VALUES (NEWID(), 'CDS', 'username1', 'password1', 'VAL', @GC_PK, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
			INSERT INTO [dbo].[GlbExternalPassword] ([GP_PK], [GP_PasswordType], [GP_UserID], [GP_CurrentPassword], [GP_PasswordStatus], [GP_GC], [GP_SystemCreateTimeUtc], [GP_SystemCreateUser], [GP_SystemLastEditTimeUtc], [GP_SystemLastEditUser])
			VALUES (NEWID(), 'CDS', 'username2', 'password2', 'VAL', @GC_PK, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
";

		using (DbCommand command = Db.Connection.Command(sql))
		{
			command.AddParameter("@GC_PK", SqlDbType.UniqueIdentifier, companyPK);
			command.ExecuteNonQuery();
		}
	}

	protected override void AssertTransformationResults()
	{
		var tokenCount = (int)TestConnection.ExecuteScalar("SELECT COUNT(1) FROM [dbo].[GlbExternalPassword] GP INNER JOIN [dbo].[GlbCompany] GC ON GP.GP_GC = GC.GC_PK WHERE GC.GC_RN_NKCountryCode = 'GB';");
		AssertEquals("Count of IsTokenForCDS = true", tokenCount, (int)TestConnection.ExecuteScalar("SELECT COUNT(1) FROM [dbo].[GenAddOnColumn] GAC INNER JOIN [dbo].[GlbExternalPassword] GP ON GAC.XA_ParentID = GP.GP_PK INNER JOIN [dbo].[GlbCompany] GC ON GP.GP_GC = GC.GC_PK WHERE GAC.XA_Name = 'IsTokenForCDS' AND GAC.XA_Data = 'Y' AND GC.GC_RN_NKCountryCode = 'GB';"));
		AssertEquals("Count of IsTokenForEMCS = true", tokenCount, (int)TestConnection.ExecuteScalar("SELECT COUNT(1) FROM [dbo].[GenAddOnColumn] GAC INNER JOIN [dbo].[GlbExternalPassword] GP ON GAC.XA_ParentID = GP.GP_PK INNER JOIN [dbo].[GlbCompany] GC ON GP.GP_GC = GC.GC_PK WHERE GAC.XA_Name = 'IsTokenForEMCS' AND GAC.XA_Data = 'Y' AND GC.GC_RN_NKCountryCode = 'GB';"));
		AssertEquals("Count of IsTokenForGVMS = true", tokenCount, (int)TestConnection.ExecuteScalar("SELECT COUNT(1) FROM [dbo].[GenAddOnColumn] GAC INNER JOIN [dbo].[GlbExternalPassword] GP ON GAC.XA_ParentID = GP.GP_PK INNER JOIN [dbo].[GlbCompany] GC ON GP.GP_GC = GC.GC_PK WHERE GAC.XA_Name = 'IsTokenForGVMS' AND GAC.XA_Data = 'Y' AND GC.GC_RN_NKCountryCode = 'GB';"));
		AssertEquals("Count of IsTokenForNCTS = true", tokenCount, (int)TestConnection.ExecuteScalar("SELECT COUNT(1) FROM [dbo].[GenAddOnColumn] GAC INNER JOIN [dbo].[GlbExternalPassword] GP ON GAC.XA_ParentID = GP.GP_PK INNER JOIN [dbo].[GlbCompany] GC ON GP.GP_GC = GC.GC_PK WHERE GAC.XA_Name = 'IsTokenForNCTS' AND GAC.XA_Data = 'Y' AND GC.GC_RN_NKCountryCode = 'GB';"));
		AssertEquals("Count of IsTokenForSnSGB = true", tokenCount, (int)TestConnection.ExecuteScalar("SELECT COUNT(1) FROM [dbo].[GenAddOnColumn] GAC INNER JOIN [dbo].[GlbExternalPassword] GP ON GAC.XA_ParentID = GP.GP_PK INNER JOIN [dbo].[GlbCompany] GC ON GP.GP_GC = GC.GC_PK WHERE GAC.XA_Name = 'IsTokenForSnSGB' AND GAC.XA_Data = 'Y' AND GC.GC_RN_NKCountryCode = 'GB';"));
	}

	protected override void SetUp()
	{
		base.SetUp();
		dataCreator = new TransformationTestDataCreator();
		companyPK = dataCreator.CreateCompany(Guid.NewGuid(), "DUK", "GB", "GBP");
	}

	TransformationTestDataCreator dataCreator;
	Guid companyPK;
}

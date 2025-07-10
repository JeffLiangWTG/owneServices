using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Recruiter;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Recruiter
{
	[TestedType(typeof(ViewAccreditationsWithLevels))]
	class ViewAccreditationsWithLevelsTest : DbCreateScriptTest
	{
		public void TestFunctionGroups()
		{
			var cclpAccreditationGroupPK = Guid.NewGuid();
			var internalAccreditationGroupPK = Guid.NewGuid();
			var ccoAccreditationPK = Guid.NewGuid();
			var fcoAccreditationPK = Guid.NewGuid();
			var lcoAccreditationPK = Guid.NewGuid();
			var ccsAccreditationPK = Guid.NewGuid();
			var fcsAccreditationPK = Guid.NewGuid();
			var lcsAccreditationPK = Guid.NewGuid();
			var ccpAccreditationPK = Guid.NewGuid();
			var rcsAccreditationPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbAccreditationGroup (HAG_PK, HAG_Description) VALUES ('{cclpAccreditationGroupPK}', 'CCLP')
INSERT INTO dbo.GlbAccreditationGroup (HAG_PK, HAG_Description) VALUES ('{internalAccreditationGroupPK}', 'Internal')

INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{ccoAccreditationPK}', 'CCO', 'CargoWise Certified Operator', 'COC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{fcoAccreditationPK}', 'FCO', 'Forwarding CargoWise Certified Operator', 'FOC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{lcoAccreditationPK}', 'LCO', 'Liner CargoWise Certified Operator', 'LOC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{ccsAccreditationPK}', 'CCS', 'CargoWise Certified Specialist', 'CSC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{fcsAccreditationPK}', 'FCS', 'Forwarding CargoWise Certified Specialist', 'FSC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{lcsAccreditationPK}', 'LCS', 'Liner CargoWise Certified Specialist', 'LSC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{ccpAccreditationPK}', 'CCP', 'CargoWise Certified Professional', 'CPC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_IsRefresher, HAC_RefresherCertificateExpiryType, HAC_CertificateCode) VALUES ('{rcsAccreditationPK}', 'RCS', 'Refresher - CargoWise Certified Specialist', 1, 'RCD', 'CSC')

INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{ccoAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{ccsAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{ccpAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{internalAccreditationGroupPK}', '{fcoAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{internalAccreditationGroupPK}', '{fcsAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{internalAccreditationGroupPK}', '{lcoAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{internalAccreditationGroupPK}', '{lcsAccreditationPK}')

INSERT INTO dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) VALUES (NEWID(), '{ccsAccreditationPK}', '{ccoAccreditationPK}')
INSERT INTO dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) VALUES (NEWID(), '{ccpAccreditationPK}', '{ccsAccreditationPK}')
INSERT INTO dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) VALUES (NEWID(), '{rcsAccreditationPK}', '{ccsAccreditationPK}')
INSERT INTO dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) VALUES (NEWID(), '{fcsAccreditationPK}', '{fcoAccreditationPK}')
INSERT INTO dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) VALUES (NEWID(), '{lcsAccreditationPK}', '{lcoAccreditationPK}')
");

			TestConnection.ExecuteNonQuery(commandText);

			var result = GetAccreditationsWithLevels(cclpAccreditationGroupPK);
			AssertEquals("Should be 4 rows", 4, result.Rows.Count);
			AssertAccreditationWithLevel(result.Rows[0], ccoAccreditationPK, ccoAccreditationPK, 0);
			AssertAccreditationWithLevel(result.Rows[1], ccpAccreditationPK, ccpAccreditationPK, 2);
			AssertAccreditationWithLevel(result.Rows[2], ccsAccreditationPK, ccsAccreditationPK, 1);
			AssertAccreditationWithLevel(result.Rows[3], rcsAccreditationPK, ccsAccreditationPK, 1);

			result = GetAccreditationsWithLevels(internalAccreditationGroupPK);
			AssertEquals("Should be 4 rows", 4, result.Rows.Count);
			AssertAccreditationWithLevel(result.Rows[0], fcoAccreditationPK, fcoAccreditationPK, 0);
			AssertAccreditationWithLevel(result.Rows[1], fcsAccreditationPK, fcsAccreditationPK, 1);
			AssertAccreditationWithLevel(result.Rows[2], lcoAccreditationPK, lcoAccreditationPK, 0);
			AssertAccreditationWithLevel(result.Rows[3], lcsAccreditationPK, lcsAccreditationPK, 1);
		}

		public void TestAccreditationPartOfMultipleGroups()
		{
			var cclpAccreditationGroupPK = Guid.NewGuid();
			var internalAccreditationGroupPK = Guid.NewGuid();
			var ccoAccreditationPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbAccreditationGroup (HAG_PK, HAG_Description) VALUES ('{cclpAccreditationGroupPK}', 'CCLP')
INSERT INTO dbo.GlbAccreditationGroup (HAG_PK, HAG_Description) VALUES ('{internalAccreditationGroupPK}', 'Internal')

INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{ccoAccreditationPK}', 'CCO', 'CargoWise Certified Operator', 'COC')

INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{ccoAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{internalAccreditationGroupPK}', '{ccoAccreditationPK}')
");

			TestConnection.ExecuteNonQuery(commandText);

			var result = GetAccreditationsWithLevels(cclpAccreditationGroupPK);
			AssertEquals("Should be 1 row", 1, result.Rows.Count);
			AssertAccreditationWithLevel(result.Rows[0], ccoAccreditationPK, ccoAccreditationPK, 0);

			result = GetAccreditationsWithLevels(internalAccreditationGroupPK);
			AssertEquals("Should be 1 row", 1, result.Rows.Count);
			AssertAccreditationWithLevel(result.Rows[0], ccoAccreditationPK, ccoAccreditationPK, 0);
		}

		public void TestDependenciesOutsideGroup()
		{
			var cclpAccreditationGroupPK = Guid.NewGuid();
			var internalAccreditationGroupPK = Guid.NewGuid();
			var ccsAccreditationPK = Guid.NewGuid();
			var acsAccreditationPK = Guid.NewGuid();
			var ccpAccreditationPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbAccreditationGroup (HAG_PK, HAG_Description) VALUES ('{cclpAccreditationGroupPK}', 'CCLP')
INSERT INTO dbo.GlbAccreditationGroup (HAG_PK, HAG_Description) VALUES ('{internalAccreditationGroupPK}', 'Internal')

INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{ccsAccreditationPK}', 'CCS', 'CargoWise Certified Specialist', 'CSC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{acsAccreditationPK}', 'ACS', 'Alternative Certified Specialist', 'ASC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{ccpAccreditationPK}', 'CCP', 'CargoWise Certified Professional', 'CPC')

INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{ccsAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{ccpAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{internalAccreditationGroupPK}', '{acsAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{internalAccreditationGroupPK}', '{ccpAccreditationPK}')

INSERT INTO dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) VALUES (NEWID(), '{ccpAccreditationPK}', '{ccsAccreditationPK}')
INSERT INTO dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) VALUES (NEWID(), '{ccpAccreditationPK}', '{acsAccreditationPK}')
");

			TestConnection.ExecuteNonQuery(commandText);

			var result = GetAccreditationsWithLevels(cclpAccreditationGroupPK);
			AssertEquals("Should be 2 rows", 2, result.Rows.Count);
			AssertAccreditationWithLevel(result.Rows[0], ccpAccreditationPK, ccpAccreditationPK, 1);
			AssertAccreditationWithLevel(result.Rows[1], ccsAccreditationPK, ccsAccreditationPK, 0);
		}

		public void TestMainAccreditationsShouldMatchThemselvesRegardlessOfCertificateCode()
		{
			var cclpAccreditationGroupPK = Guid.NewGuid();
			var ccsAccreditationPK = Guid.NewGuid();
			var ccpAccreditationPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbAccreditationGroup (HAG_PK, HAG_Description) VALUES ('{cclpAccreditationGroupPK}', 'CCLP')

INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{ccsAccreditationPK}', 'CCS', 'CargoWise Certified Specialist', '')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{ccpAccreditationPK}', 'CCP', 'CargoWise Certified Professional', '')

INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{ccsAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{ccpAccreditationPK}')

INSERT INTO dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) VALUES (NEWID(), '{ccpAccreditationPK}', '{ccsAccreditationPK}')
");

			TestConnection.ExecuteNonQuery(commandText);

			var result = GetAccreditationsWithLevels(cclpAccreditationGroupPK);
			AssertEquals("Should be 2 rows", 2, result.Rows.Count);
			AssertAccreditationWithLevel(result.Rows[0], ccpAccreditationPK, ccpAccreditationPK, 1);
			AssertAccreditationWithLevel(result.Rows[1], ccsAccreditationPK, ccsAccreditationPK, 0);
		}

		#region Implementation

		DataTable GetAccreditationsWithLevels(Guid accreditationGroup)
		{
			var command = TestConnection.Command(@"
SELECT HAL_HAC, HAL_HAC_Main, HAL_Level
FROM dbo.ViewAccreditationsWithLevels
JOIN dbo.GlbAccreditation g ON g.HAC_PK = HAL_HAC
WHERE 
HAL_HAG = @HAG_PK
ORDER BY HAC_Code");
			command.AddParameter("@HAG_PK", SqlDbType.UniqueIdentifier, accreditationGroup);

			return DataUtils.GetDataTableFromCommand(command);
		}

		void AssertAccreditationWithLevel(DataRow row, Guid accreditationPK, Guid mainAccreditationPK, int level)
		{
			AssertEquals("Accreditation should match (pass in in alphabetical order of HAC_Code)", accreditationPK, row[0]);
			AssertEquals("Main Accreditation should match main accreditation for refresher or itself if it is not a refresher", mainAccreditationPK, row[1]);
			AssertEquals("Level based on Accreditation Requirements within the Group", level, row[2]);
		}
		#endregion
	}
}


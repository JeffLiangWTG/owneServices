using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Recruiter;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Recruiter
{
	[TestedType(typeof(GetHighestLevelAccreditationAttemptForGroup))]
	class GetHighestLevelAccreditationAttemptForGroupTest : DbCreateScriptTest
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
			var person1PK = Guid.NewGuid();
			var person2PK = Guid.NewGuid();
			var person3PK = Guid.NewGuid();
			var person1CCOAttemptPK = Guid.NewGuid();
			var person1CCSAttemptPK = Guid.NewGuid();
			var person1CCPAttemptPK = Guid.NewGuid();
			var person1RCSAttemptPK = Guid.NewGuid();
			var person2CCOAttemptPK = Guid.NewGuid();
			var person2CCSAttemptPK = Guid.NewGuid();
			var person2RCSAttemptPK = Guid.NewGuid();
			var person3CCOAttemptPK = Guid.NewGuid();
			var person3CCSAttemptPK = Guid.NewGuid();
			var person3FCOAttemptPK = Guid.NewGuid();
			var person3FCSAttemptPK = Guid.NewGuid();
			var person3LCOAttemptPK = Guid.NewGuid();
			var person3LCSAttemptPK = Guid.NewGuid();

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
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{rcsAccreditationPK}')
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

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person1PK}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person2PK}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person3PK}', 'asdasd', 'NZ')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1CCOAttemptPK}', '{ccoAccreditationPK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1CCSAttemptPK}', '{ccsAccreditationPK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1CCPAttemptPK}', '{ccpAccreditationPK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1RCSAttemptPK}', '{rcsAccreditationPK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person2CCOAttemptPK}', '{ccoAccreditationPK}', '{person2PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person2CCSAttemptPK}', '{ccsAccreditationPK}', '{person2PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person2RCSAttemptPK}', '{rcsAccreditationPK}', '{person2PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2021-01-01')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person3CCOAttemptPK}', '{ccoAccreditationPK}', '{person3PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person3CCSAttemptPK}', '{ccsAccreditationPK}', '{person3PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person3FCOAttemptPK}', '{fcoAccreditationPK}', '{person3PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person3FCSAttemptPK}', '{fcsAccreditationPK}', '{person3PK}', '2018-01-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person3LCOAttemptPK}', '{lcoAccreditationPK}', '{person3PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person3LCSAttemptPK}', '{lcsAccreditationPK}', '{person3PK}', '2018-01-01', '2019-01-01', '2021-01-01')");

			TestConnection.ExecuteNonQuery(commandText);

			var levelResult = GetAccreditationsWithLevels(cclpAccreditationGroupPK);
			AssertEquals("Precondition", 4, levelResult.Rows.Count);
			AssertAccreditationWithLevel(levelResult.Rows[0], ccoAccreditationPK, ccoAccreditationPK, 0);
			AssertAccreditationWithLevel(levelResult.Rows[1], ccpAccreditationPK, ccpAccreditationPK, 2);
			AssertAccreditationWithLevel(levelResult.Rows[2], ccsAccreditationPK, ccsAccreditationPK, 1);
			AssertAccreditationWithLevel(levelResult.Rows[3], rcsAccreditationPK, ccsAccreditationPK, 1);

			levelResult = GetAccreditationsWithLevels(internalAccreditationGroupPK);
			AssertEquals("Precondition", 4, levelResult.Rows.Count);
			AssertAccreditationWithLevel(levelResult.Rows[0], fcoAccreditationPK, fcoAccreditationPK, 0);
			AssertAccreditationWithLevel(levelResult.Rows[1], fcsAccreditationPK, fcsAccreditationPK, 1);
			AssertAccreditationWithLevel(levelResult.Rows[2], lcoAccreditationPK, lcoAccreditationPK, 0);
			AssertAccreditationWithLevel(levelResult.Rows[3], lcsAccreditationPK, lcsAccreditationPK, 1);

			var result = GetHighestLevelAccreditationAttemptForGroupResult(cclpAccreditationGroupPK, false, person1PK);
			AssertEquals("Should be 1 row", 1, result.Rows.Count);
			AssertEquals("CCP should be the highest level accreditation", person1CCPAttemptPK, result.Rows[0][0]);

			result = GetHighestLevelAccreditationAttemptForGroupResult(internalAccreditationGroupPK, false, person1PK);
			AssertEquals("Should be no rows for this group", 0, result.Rows.Count);

			result = GetHighestLevelAccreditationAttemptForGroupResult(cclpAccreditationGroupPK, false, person2PK);
			AssertEquals("Should be 1 row", 1, result.Rows.Count);
			AssertEquals("CCS and RCS should be the highest level accreditations but RCS has a later expiry date", person2RCSAttemptPK, result.Rows[0][0]);

			result = GetHighestLevelAccreditationAttemptForGroupResult(internalAccreditationGroupPK, false, person2PK);
			AssertEquals("Should be no rows for this group", 0, result.Rows.Count);

			result = GetHighestLevelAccreditationAttemptForGroupResult(cclpAccreditationGroupPK, false, person3PK);
			AssertEquals("Should be 1 row", 1, result.Rows.Count);
			AssertEquals("CCS should be the highest level accreditation", person3CCSAttemptPK, result.Rows[0][0]);

			result = GetHighestLevelAccreditationAttemptForGroupResult(internalAccreditationGroupPK, false, person3PK);
			AssertEquals("Should be 2 rows (FCS and LCS are both highest level)", 2, result.Rows.Count);
			AssertEquals("FCS and LCS should be the highest level accreditations", person3FCSAttemptPK, result.Rows[0][0]);
			AssertEquals("FCS and LCS should be the highest level accreditations", person3LCSAttemptPK, result.Rows[1][0]);

			result = GetHighestLevelAccreditationAttemptForGroupResult(false, person3PK);
			AssertEquals("Should be 3 rows", 3, result.Rows.Count);
			AssertEquals("CCS should be the highest level accreditation", person3CCSAttemptPK, result.Rows[0][0]);
			AssertEquals("FCS and LCS should be the highest level accreditations", person3FCSAttemptPK, result.Rows[1][0]);
			AssertEquals("FCS and LCS should be the highest level accreditations", person3LCSAttemptPK, result.Rows[2][0]);

			result = GetHighestLevelAccreditationAttemptForGroupResult(true, person3PK);
			AssertEquals("Should be 3 rows", 3, result.Rows.Count);
			AssertEquals("CCS should be the highest level accreditation", person3CCSAttemptPK, result.Rows[0][0]);
			AssertEquals("FCO and LCO should be the highest level accreditations", person3FCOAttemptPK, result.Rows[1][0]);
			AssertEquals("FCO and LCO should be the highest level accreditations", person3LCOAttemptPK, result.Rows[2][0]);
		}

		public void TestFunctionGroups_SamePersonDiffGroups()
		{
			var cclpAccreditationGroupPK = Guid.NewGuid();
			var internalAccreditationGroupPK = Guid.NewGuid();
			var ccoAccreditationPK = Guid.NewGuid();
			var fcoAccreditationPK = Guid.NewGuid();
			var ccsAccreditationPK = Guid.NewGuid();
			var fcsAccreditationPK = Guid.NewGuid();
			var person1PK = Guid.NewGuid();
			var person1CCOAttemptPK = Guid.NewGuid();
			var person1CCSAttemptPK = Guid.NewGuid();
			var person1FCOAttemptPK = Guid.NewGuid();
			var person1FCSAttemptPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbAccreditationGroup (HAG_PK, HAG_Description) VALUES ('{cclpAccreditationGroupPK}', 'CCLP')
INSERT INTO dbo.GlbAccreditationGroup (HAG_PK, HAG_Description) VALUES ('{internalAccreditationGroupPK}', 'Internal')

INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{ccoAccreditationPK}', 'CCO', 'CargoWise Certified Operator', 'COC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{fcoAccreditationPK}', 'FCO', 'Forwarding CargoWise Certified Operator', 'FOC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{ccsAccreditationPK}', 'CCS', 'CargoWise Certified Specialist', 'CSC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{fcsAccreditationPK}', 'FCS', 'Forwarding CargoWise Certified Specialist', 'FSC')

INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{ccoAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{ccsAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{internalAccreditationGroupPK}', '{fcoAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{internalAccreditationGroupPK}', '{fcsAccreditationPK}')

INSERT INTO dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) VALUES (NEWID(), '{ccsAccreditationPK}', '{ccoAccreditationPK}')
INSERT INTO dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) VALUES (NEWID(), '{fcsAccreditationPK}', '{fcoAccreditationPK}')

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person1PK}', 'asdasd', 'NZ')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1CCOAttemptPK}', '{ccoAccreditationPK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1CCSAttemptPK}', '{ccsAccreditationPK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1FCOAttemptPK}', '{fcoAccreditationPK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1FCSAttemptPK}', '{fcsAccreditationPK}', '{person1PK}', '2018-01-01', '2019-01-01', '2020-01-01')");

			TestConnection.ExecuteNonQuery(commandText);

			var result = GetHighestLevelAccreditationAttemptForGroupResult(false, person1PK);
			AssertEquals("Should be 2 rows", 2, result.Rows.Count);
			AssertEquals("CCS should be the highest level accreditation", person1CCSAttemptPK, result.Rows[0][0]);
			AssertEquals("FCS should be the highest level accreditation", person1FCSAttemptPK, result.Rows[1][0]);

			result = GetHighestLevelAccreditationAttemptForGroupResult(true, person1PK);
			AssertEquals("Should be 2 rows", 2, result.Rows.Count);
			AssertEquals("CCS should be the highest level accreditation", person1CCSAttemptPK, result.Rows[0][0]);
			AssertEquals("FCO should be the highest level accreditation", person1FCOAttemptPK, result.Rows[1][0]);
		}

		public void TestFunctionShouldReturnAllAttemptsOnTheSameLevel()
		{
			var internalAccreditationGroupPK = Guid.NewGuid();
			var fcoAccreditationPK = Guid.NewGuid();
			var lcoAccreditationPK = Guid.NewGuid();
			var fcsAccreditationPK = Guid.NewGuid();
			var lcsAccreditationPK = Guid.NewGuid();
			var rcsAccreditationPK = Guid.NewGuid();
			var person1PK = Guid.NewGuid();
			var person1FCOAttemptPK = Guid.NewGuid();
			var person1FCSAttemptPK = Guid.NewGuid();
			var person1LCOAttemptPK = Guid.NewGuid();
			var person1LCSAttemptPK = Guid.NewGuid();
			var person1RCSAttemptPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbAccreditationGroup (HAG_PK, HAG_Description) VALUES ('{internalAccreditationGroupPK}', 'Internal')

INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{fcoAccreditationPK}', 'FCO', 'Forwarding CargoWise Certified Operator', 'FOC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{lcoAccreditationPK}', 'LCO', 'Liner CargoWise Certified Operator', 'LOC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{fcsAccreditationPK}', 'FCS', 'Forwarding CargoWise Certified Specialist', 'FSC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{lcsAccreditationPK}', 'LCS', 'Liner CargoWise Certified Specialist', 'LSC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_IsRefresher, HAC_RefresherCertificateExpiryType, HAC_CertificateCode) VALUES ('{rcsAccreditationPK}', 'RCS', 'Refresher - CargoWise Certified Specialist', 1, 'RCD', 'FSC')

INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{internalAccreditationGroupPK}', '{fcoAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{internalAccreditationGroupPK}', '{fcsAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{internalAccreditationGroupPK}', '{lcoAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{internalAccreditationGroupPK}', '{lcsAccreditationPK}')

INSERT INTO dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) VALUES (NEWID(), '{rcsAccreditationPK}', '{fcsAccreditationPK}')
INSERT INTO dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) VALUES (NEWID(), '{fcsAccreditationPK}', '{fcoAccreditationPK}')
INSERT INTO dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) VALUES (NEWID(), '{lcsAccreditationPK}', '{lcoAccreditationPK}')

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person1PK}', 'asdasd', 'NZ')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1FCOAttemptPK}', '{fcoAccreditationPK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1FCSAttemptPK}', '{fcsAccreditationPK}', '{person1PK}', '2018-01-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1LCOAttemptPK}', '{lcoAccreditationPK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1LCSAttemptPK}', '{lcsAccreditationPK}', '{person1PK}', '2018-01-01', '2019-01-01', '2021-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1RCSAttemptPK}', '{rcsAccreditationPK}', '{person1PK}', '2018-01-03', '2019-01-01', '2021-01-03')
");

			TestConnection.ExecuteNonQuery(commandText);

			var levelResult = GetAccreditationsWithLevels(internalAccreditationGroupPK);
			AssertEquals("Precondition", 5, levelResult.Rows.Count);
			AssertAccreditationWithLevel(levelResult.Rows[0], fcoAccreditationPK, fcoAccreditationPK, 0);
			AssertAccreditationWithLevel(levelResult.Rows[1], fcsAccreditationPK, fcsAccreditationPK, 1);
			AssertAccreditationWithLevel(levelResult.Rows[2], lcoAccreditationPK, lcoAccreditationPK, 0);
			AssertAccreditationWithLevel(levelResult.Rows[3], lcsAccreditationPK, lcsAccreditationPK, 1);
			AssertAccreditationWithLevel(levelResult.Rows[4], rcsAccreditationPK, fcsAccreditationPK, 1);

			var result = GetHighestLevelAccreditationAttemptForGroupResult(internalAccreditationGroupPK, false, person1PK);
			AssertEquals("Should be 2 rows (FCS, LCS and RCS are all highest level)", 2, result.Rows.Count);
			AssertEquals("LCS should be a highest level accreditations", person1LCSAttemptPK, result.Rows[0][0]);
			AssertEquals("FCS and RCS are grouped, but the RCS attempt expires later", person1RCSAttemptPK, result.Rows[1][0]);
		}

		public void TestFunctionIsCompleted()
		{
			var cclpAccreditationGroupPK = Guid.NewGuid();
			var ccoAccreditationPK = Guid.NewGuid();
			var ccsAccreditationPK = Guid.NewGuid();
			var ccpAccreditationPK = Guid.NewGuid();
			var rcsAccreditationPK = Guid.NewGuid();
			var person1PK = Guid.NewGuid();
			var person1CCOAttemptPK = Guid.NewGuid();
			var person1CCSAttemptPK = Guid.NewGuid();
			var person1RCSAttemptPK = Guid.NewGuid();
			var person1CCPAttemptPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbAccreditationGroup (HAG_PK, HAG_Description) VALUES ('{cclpAccreditationGroupPK}', 'CCLP')

INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{ccoAccreditationPK}', 'CCO', 'CargoWise Certified Operator', 'COC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{ccsAccreditationPK}', 'CCS', 'CargoWise Certified Specialist', 'CSC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{ccpAccreditationPK}', 'CCP', 'CargoWise Certified Professional', 'CPC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_IsRefresher, HAC_RefresherCertificateExpiryType, HAC_CertificateCode) VALUES ('{rcsAccreditationPK}', 'RCS', 'Refresher - CargoWise Certified Specialist', 1, 'RCD', 'CSC')

INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{ccoAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{ccsAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{rcsAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{ccpAccreditationPK}')

INSERT INTO dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) VALUES (NEWID(), '{ccsAccreditationPK}', '{ccoAccreditationPK}')
INSERT INTO dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) VALUES (NEWID(), '{ccpAccreditationPK}', '{ccsAccreditationPK}')
INSERT INTO dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) VALUES (NEWID(), '{rcsAccreditationPK}', '{ccsAccreditationPK}')

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person1PK}', 'asdasd', 'NZ')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1CCOAttemptPK}', '{ccoAccreditationPK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2019-06-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1CCSAttemptPK}', '{ccsAccreditationPK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2019-08-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1RCSAttemptPK}', '{rcsAccreditationPK}', '{person1PK}', '2018-04-01', '2018-07-01', '2019-01-01', '2019-10-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1CCPAttemptPK}', '{ccpAccreditationPK}', '{person1PK}', '2018-01-01', '2019-01-01', '2021-01-01')
");

			TestConnection.ExecuteNonQuery(commandText);

			var levelResult = GetAccreditationsWithLevels(cclpAccreditationGroupPK);
			AssertEquals("Precondition", 4, levelResult.Rows.Count);
			AssertAccreditationWithLevel(levelResult.Rows[0], ccoAccreditationPK, ccoAccreditationPK, 0);
			AssertAccreditationWithLevel(levelResult.Rows[1], ccpAccreditationPK, ccpAccreditationPK, 2);
			AssertAccreditationWithLevel(levelResult.Rows[2], ccsAccreditationPK, ccsAccreditationPK, 1);
			AssertAccreditationWithLevel(levelResult.Rows[3], rcsAccreditationPK, ccsAccreditationPK, 1);

			var result = GetHighestLevelAccreditationAttemptForGroupResult(cclpAccreditationGroupPK, false, person1PK);
			AssertEquals("Should be 1 row", 1, result.Rows.Count);
			AssertEquals("CCP should be the highest level accreditation", person1CCPAttemptPK, result.Rows[0][0]);
			result = GetHighestLevelAccreditationAttemptForGroupResult(cclpAccreditationGroupPK, true, person1PK);
			AssertEquals("Should be 1 row", 1, result.Rows.Count);
			AssertEquals("RCS should be the highest level completed accreditation", person1RCSAttemptPK, result.Rows[0][0]);
		}

		public void TestAccreditationPartOfMultipleGroups()
		{
			var cclpAccreditationGroupPK = Guid.NewGuid();
			var internalAccreditationGroupPK = Guid.NewGuid();
			var ccoAccreditationPK = Guid.NewGuid();
			var person1PK = Guid.NewGuid();
			var person1CCOAttemptPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbAccreditationGroup (HAG_PK, HAG_Description) VALUES ('{cclpAccreditationGroupPK}', 'CCLP')
INSERT INTO dbo.GlbAccreditationGroup (HAG_PK, HAG_Description) VALUES ('{internalAccreditationGroupPK}', 'Internal')

INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{ccoAccreditationPK}', 'CCO', 'CargoWise Certified Operator', 'COC')

INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{ccoAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{internalAccreditationGroupPK}', '{ccoAccreditationPK}')

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person1PK}', 'asdasd', 'NZ')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1CCOAttemptPK}', '{ccoAccreditationPK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2019-06-01')
");

			TestConnection.ExecuteNonQuery(commandText);

			var levelResult = GetAccreditationsWithLevels(cclpAccreditationGroupPK);
			AssertEquals("Precondition", 1, levelResult.Rows.Count);
			AssertAccreditationWithLevel(levelResult.Rows[0], ccoAccreditationPK, ccoAccreditationPK, 0);

			levelResult = GetAccreditationsWithLevels(internalAccreditationGroupPK);
			AssertEquals("Precondition", 1, levelResult.Rows.Count);
			AssertAccreditationWithLevel(levelResult.Rows[0], ccoAccreditationPK, ccoAccreditationPK, 0);

			var result = GetHighestLevelAccreditationAttemptForGroupResult(cclpAccreditationGroupPK, true, person1PK);
			AssertEquals("Should be 1 row", 1, result.Rows.Count);
			AssertEquals("CCO should be the highest level completed accreditation", person1CCOAttemptPK, result.Rows[0][0]);

			result = GetHighestLevelAccreditationAttemptForGroupResult(internalAccreditationGroupPK, true, person1PK);
			AssertEquals("Should be 1 row", 1, result.Rows.Count);
			AssertEquals("CCO should be the highest level completed accreditation", person1CCOAttemptPK, result.Rows[0][0]);
		}

		public void TestMultipleAttemptsForTheSameAccreditationShouldChooseMostRecent()
		{
			var cclpAccreditationGroupPK = Guid.NewGuid();
			var ccoAccreditationPK = Guid.NewGuid();
			var attempt1PK = Guid.NewGuid();
			var attempt2PK = Guid.NewGuid();
			var attempt3PK = Guid.NewGuid();
			var attempt4PK = Guid.NewGuid();
			var attempt5PK = Guid.NewGuid();
			var person1PK = Guid.NewGuid();
			var person2PK = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbAccreditationGroup (HAG_PK, HAG_Description) VALUES ('{cclpAccreditationGroupPK}', 'CCLP')

INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{ccoAccreditationPK}', 'CCO', 'CargoWise Certified Operator', 'COC')

INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{ccoAccreditationPK}')

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person1PK}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person2PK}', 'asdasd', 'NZ')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate) VALUES ('{attempt1PK}', '{ccoAccreditationPK}', '{person1PK}', '2018-01-01', '2019-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate) VALUES ('{attempt2PK}', '{ccoAccreditationPK}', '{person1PK}', '2018-01-02', '2019-01-01')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{attempt3PK}', '{ccoAccreditationPK}', '{person2PK}', '2018-01-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{attempt4PK}', '{ccoAccreditationPK}', '{person2PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{attempt5PK}', '{ccoAccreditationPK}', '{person2PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-02')
");

			TestConnection.ExecuteNonQuery(commandText);

			var levelResult = GetAccreditationsWithLevels(cclpAccreditationGroupPK);
			AssertEquals("Precondition", 1, levelResult.Rows.Count);
			AssertAccreditationWithLevel(levelResult.Rows[0], ccoAccreditationPK, ccoAccreditationPK, 0);

			var result = GetHighestLevelAccreditationAttemptForGroupResult(cclpAccreditationGroupPK, false, person1PK);
			AssertEquals("Should be 1 row", 1, result.Rows.Count);
			AssertEquals("2018-01-02 is the most recent commencement date", attempt2PK, result.Rows[0][0]);

			result = GetHighestLevelAccreditationAttemptForGroupResult(cclpAccreditationGroupPK, false, person2PK);
			AssertEquals("Should be 1 row", 1, result.Rows.Count);
			AssertEquals("2020-01-02 is the most recent expiry date", attempt5PK, result.Rows[0][0]);

			result = GetHighestLevelAccreditationAttemptForGroupResult(cclpAccreditationGroupPK, true, person2PK);
			AssertEquals("Should be 1 row", 1, result.Rows.Count);
			AssertEquals("2020-01-02 is the most recent expiry date", attempt5PK, result.Rows[0][0]);
		}

		public void TestRefresherOnSameLevelAsAccreditationSoShouldChooseMostRecent()
		{
			var cclpAccreditationGroupPK = Guid.NewGuid();
			var ccoAccreditationPK = Guid.NewGuid();
			var croAccreditationPK = Guid.NewGuid();
			var attempt1PK = Guid.NewGuid();
			var attempt2PK = Guid.NewGuid();
			var attempt3PK = Guid.NewGuid();
			var attempt4PK = Guid.NewGuid();
			var attempt5PK = Guid.NewGuid();
			var person1PK = Guid.NewGuid();
			var person2PK = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbAccreditationGroup (HAG_PK, HAG_Description) VALUES ('{cclpAccreditationGroupPK}', 'CCLP')

INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{ccoAccreditationPK}', 'CCO', 'CargoWise Certified Operator', 'COC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_IsRefresher, HAC_RefresherCertificateExpiryType, HAC_CertificateCode) VALUES ('{croAccreditationPK}', 'CRO', 'CargoWise Certified Operator - Refresher', 1, 'RCD', 'COC')

INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{ccoAccreditationPK}')
INSERT INTO dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) VALUES (NEWID(), '{croAccreditationPK}', '{ccoAccreditationPK}')

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person1PK}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person2PK}', 'asdasd', 'NZ')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{attempt1PK}', '{ccoAccreditationPK}', '{person1PK}', '2018-01-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{attempt2PK}', '{croAccreditationPK}', '{person1PK}', '2018-01-02', '2019-01-01', '2020-01-01')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{attempt3PK}', '{ccoAccreditationPK}', '{person2PK}', '2018-01-02', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{attempt4PK}', '{croAccreditationPK}', '{person2PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{attempt5PK}', '{ccoAccreditationPK}', '{person2PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-02')
");

			TestConnection.ExecuteNonQuery(commandText);

			var levelResult = GetAccreditationsWithLevels(cclpAccreditationGroupPK);
			AssertEquals("Precondition", 2, levelResult.Rows.Count);
			AssertAccreditationWithLevel(levelResult.Rows[0], ccoAccreditationPK, ccoAccreditationPK, 0);
			AssertAccreditationWithLevel(levelResult.Rows[1], croAccreditationPK, ccoAccreditationPK, 0);

			var result = GetHighestLevelAccreditationAttemptForGroupResult(cclpAccreditationGroupPK, false, person1PK);
			AssertEquals("Should be 1 row", 1, result.Rows.Count);
			AssertEquals("2018-01-02 is the most recent commencement date", attempt2PK, result.Rows[0][0]);

			result = GetHighestLevelAccreditationAttemptForGroupResult(cclpAccreditationGroupPK, false, person2PK);
			AssertEquals("Should be 1 row", 1, result.Rows.Count);
			AssertEquals("2020-01-02 is the most recent expiry date", attempt5PK, result.Rows[0][0]);
		}

		public void TestDependenciesOutsideGroup()
		{
			var cclpAccreditationGroupPK = Guid.NewGuid();
			var internalAccreditationGroupPK = Guid.NewGuid();
			var ccsAccreditationPK = Guid.NewGuid();
			var acsAccreditationPK = Guid.NewGuid();
			var ccpAccreditationPK = Guid.NewGuid();
			var attempt1PK = Guid.NewGuid();
			var attempt2PK = Guid.NewGuid();
			var person1PK = Guid.NewGuid();
			var person2PK = Guid.NewGuid();

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

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person1PK}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person2PK}', 'asdasd', 'NZ')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{attempt1PK}', '{ccsAccreditationPK}', '{person1PK}', '2018-01-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{attempt2PK}', '{ccpAccreditationPK}', '{person1PK}', '2018-01-02', '2019-01-01', '2020-01-01')
");

			TestConnection.ExecuteNonQuery(commandText);

			var levelResult = GetAccreditationsWithLevels(cclpAccreditationGroupPK);
			AssertEquals("Precondition", 2, levelResult.Rows.Count);
			AssertAccreditationWithLevel(levelResult.Rows[0], ccpAccreditationPK, ccpAccreditationPK, 1);
			AssertAccreditationWithLevel(levelResult.Rows[1], ccsAccreditationPK, ccsAccreditationPK, 0);

			levelResult = GetAccreditationsWithLevels(internalAccreditationGroupPK);
			AssertEquals("Precondition", 2, levelResult.Rows.Count);
			AssertAccreditationWithLevel(levelResult.Rows[0], acsAccreditationPK, acsAccreditationPK, 0);
			AssertAccreditationWithLevel(levelResult.Rows[1], ccpAccreditationPK, ccpAccreditationPK, 1);

			var result = GetHighestLevelAccreditationAttemptForGroupResult(cclpAccreditationGroupPK, false, person1PK);
			AssertEquals("Should be 1 row", 1, result.Rows.Count);
			AssertEquals("CCP should be highest level in CCLP", attempt2PK, result.Rows[0][0]);
		}

		public void TestUsingPersonParameter()
		{
			var cclpAccreditationGroupPK = Guid.NewGuid();
			var ccoAccreditationPK = Guid.NewGuid();
			var attempt1PK = Guid.NewGuid();
			var attempt2PK = Guid.NewGuid();
			var person1PK = Guid.NewGuid();
			var person2PK = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbAccreditationGroup (HAG_PK, HAG_Description) VALUES ('{cclpAccreditationGroupPK}', 'CCLP')

INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{ccoAccreditationPK}', 'CCO', 'CargoWise Certified Operator', 'COC')

INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{ccoAccreditationPK}')

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person1PK}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person2PK}', 'asdasd', 'NZ')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate) VALUES ('{attempt1PK}', '{ccoAccreditationPK}', '{person1PK}', '2018-01-01', '2019-01-01')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{attempt2PK}', '{ccoAccreditationPK}', '{person2PK}', '2018-01-01', '2019-01-01', '2020-01-01')
");

			TestConnection.ExecuteNonQuery(commandText);

			var levelResult = GetAccreditationsWithLevels(cclpAccreditationGroupPK);
			AssertEquals("Precondition", 1, levelResult.Rows.Count);
			AssertAccreditationWithLevel(levelResult.Rows[0], ccoAccreditationPK, ccoAccreditationPK, 0);

			var result = GetHighestLevelAccreditationAttemptForGroupResult(cclpAccreditationGroupPK, false, person1PK);
			AssertEquals("Should be 1 row", 1, result.Rows.Count);
			AssertEquals("2018-01-02 is the most recent commencement date", attempt1PK, result.Rows[0][0]);

			result = GetHighestLevelAccreditationAttemptForGroupResult(cclpAccreditationGroupPK, false, person2PK);
			AssertEquals("Should be 1 row", 1, result.Rows.Count);
			AssertEquals("2020-01-02 is the most recent expiry date", attempt2PK, result.Rows[0][0]);
		}

		public void TestShouldGetHighestLevelForPerson()
		{
			var cclpAccreditationGroupPK = Guid.NewGuid();
			var internalAccreditationGroupPK = Guid.NewGuid();
			var ccoAccreditationPK = Guid.NewGuid();
			var ccsAccreditationPK = Guid.NewGuid();
			var rcoAccreditationPK = Guid.NewGuid();
			var attempt1PK = Guid.NewGuid();
			var attempt2PK = Guid.NewGuid();
			var attempt3PK = Guid.NewGuid();
			var person1PK = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbAccreditationGroup (HAG_PK, HAG_Description) VALUES ('{cclpAccreditationGroupPK}', 'CCLP')
INSERT INTO dbo.GlbAccreditationGroup (HAG_PK, HAG_Description) VALUES ('{internalAccreditationGroupPK}', 'Internal')

INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{ccoAccreditationPK}', 'CCO', 'CargoWise Certified Operator', 'COC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{ccsAccreditationPK}', 'CCS', 'CargoWise Certified Specialist', 'CSC')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_IsRefresher, HAC_RefresherCertificateExpiryType, HAC_CertificateCode) VALUES ('{rcoAccreditationPK}', 'RCO', 'Refresher - CargoWise Certified Operator', 1, 'RCD', 'COC')

INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{ccoAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{cclpAccreditationGroupPK}', '{ccsAccreditationPK}')
INSERT INTO dbo.GlbAccreditationGroupPivot (HAP_PK, HAP_HAG, HAP_HAC) VALUES (NEWID(), '{internalAccreditationGroupPK}', '{ccsAccreditationPK}')

INSERT INTO dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) VALUES (NEWID(), '{ccsAccreditationPK}', '{ccoAccreditationPK}')
INSERT INTO dbo.GlbAccreditationRequirementPivot (HAR_PK, HAR_HAC, HAR_HAC_Parent) VALUES (NEWID(), '{rcoAccreditationPK}', '{ccoAccreditationPK}')

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person1PK}', 'asdasd', 'NZ')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate) VALUES ('{attempt1PK}', '{ccoAccreditationPK}', '{person1PK}', '2018-01-01', '2019-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate) VALUES ('{attempt2PK}', '{ccsAccreditationPK}', '{person1PK}', '2018-01-01', '2019-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate) VALUES ('{attempt3PK}', '{rcoAccreditationPK}', '{person1PK}', '2018-01-01', '2019-01-01')
");

			TestConnection.ExecuteNonQuery(commandText);

			var levelResult = GetAccreditationsWithLevels(cclpAccreditationGroupPK);
			AssertEquals("Precondition", 3, levelResult.Rows.Count);
			AssertAccreditationWithLevel(levelResult.Rows[0], ccoAccreditationPK, ccoAccreditationPK, 0);
			AssertAccreditationWithLevel(levelResult.Rows[1], ccsAccreditationPK, ccsAccreditationPK, 1);
			AssertAccreditationWithLevel(levelResult.Rows[2], rcoAccreditationPK, ccoAccreditationPK, 0);

			levelResult = GetAccreditationsWithLevels(internalAccreditationGroupPK);
			AssertEquals("Precondition", 1, levelResult.Rows.Count);
			AssertAccreditationWithLevel(levelResult.Rows[0], ccsAccreditationPK, ccsAccreditationPK, 0);

			var result = GetHighestLevelAccreditationAttemptForGroupResult(cclpAccreditationGroupPK, false, person1PK);
			AssertEquals("Should be 1 row", 1, result.Rows.Count);
			AssertEquals("Attempt 2 is highest level", attempt2PK, result.Rows[0][0]);
		}

		DataTable GetHighestLevelAccreditationAttemptForGroupResult(Guid accreditationGroup, bool isCompleted, Guid personPK)
		{
			var command = TestConnection.Command(@"
SELECT AttemptPk, IsCompleted, PersonPk, GroupPk
FROM GetHighestLevelAccreditationAttemptForGroup(@HAG_PK, @IsCompleted, @PersonPK)
JOIN dbo.GlbAccreditationAttempt ON HAA_PK = AttemptPk
JOIN dbo.GlbAccreditation ON HAC_PK = HAA_HAC
ORDER BY HAC_Code");

			AddParametersToCommand(command, accreditationGroup, isCompleted, personPK);
			return DataUtils.GetDataTableFromCommand(command);
		}

		DataTable GetHighestLevelAccreditationAttemptForGroupResult(bool isCompleted, Guid personPK)
		{
			var command = TestConnection.Command(@"
SELECT AttemptPk, IsCompleted, PersonPk, GroupPk
FROM GetHighestLevelAccreditationAttemptForGroup(null, @IsCompleted, @PersonPK)
JOIN dbo.GlbAccreditationAttempt ON HAA_PK = AttemptPk
JOIN dbo.GlbAccreditation ON HAC_PK = HAA_HAC
ORDER BY HAC_Code");

			AddParametersToCommand(command, Guid.Empty, isCompleted, personPK);
			return DataUtils.GetDataTableFromCommand(command);
		}

		void AddParametersToCommand(DbCommand command, Guid accreditationGroup, bool isCompleted, Guid personPK)
		{
			if (accreditationGroup != Guid.Empty)
			{
				command.AddParameter("@HAG_PK", SqlDbType.UniqueIdentifier, accreditationGroup);
			}

			command.AddParameter("@IsCompleted", SqlDbType.Bit, isCompleted);
			command.AddParameter("@PersonPK", SqlDbType.UniqueIdentifier, personPK);
		}

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
			AssertEquals("Precondition - Accreditation should match (pass in in alphabetical order of HAC_Code)", accreditationPK, row[0]);
			AssertEquals("Precondition - Main Accreditation should match main accreditation for refresher or itself if it is not a refresher", mainAccreditationPK, row[1]);
			AssertEquals("Precondition - Level based on Accreditation Requirements within the Group", level, row[2]);
		}
	}
}


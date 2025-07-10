using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.FR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.FR
{
	[TestedType(typeof(UpdateOldGuaranteesMissingEntRuleCode))]
	sealed class UpdateOldGuaranteesMissingEntRuleCodeTestTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateOldGuaranteesMissingEntRuleCode();

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update old FR COD and DEF guarantees to have ENT rule code with Both value._1] ON [dbo].[CusPermitHeader] ([CPH_RN_NKCountryCode]) INCLUDE ([CPH_PK]) WHERE (([CPH_RN_NKCountryCode] IN ('FR', 'GF', 'GP', 'MQ', 'YT', 'RE', 'MF', 'BL')) AND ([CPH_Type] IN ('COD', 'DEF'))) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update old FR COD and DEF guarantees to have ENT rule code with Both value._2] ON [dbo].[CusPermitRule] ([CPR_CPH_PermitHeader]) WHERE ([CPR_RuleCode]='ENT') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		};

		protected override void PrepareTestData()
		{
			guaranteeCODPK = Guid.NewGuid();
			guaranteeCODAffiliateCountryPK = Guid.NewGuid();
			guaranteeCODWithoutRuleCodePK = Guid.NewGuid();
			guaranteeDEFPK = Guid.NewGuid();
			guaranteeOTHPK = Guid.NewGuid();
			guaranteeWithEntPK = Guid.NewGuid();
			guaranteeCODNotFR = Guid.NewGuid();

			ruleCodeForCODPK = Guid.NewGuid();
			ruleCodeForCODAffiliateCountryPK = Guid.NewGuid();
			ruleCodeForDEFPK = Guid.NewGuid();
			ruleCodeForOTHPK = Guid.NewGuid();
			ruleCodeENTPK = Guid.NewGuid();
			ruleCodeForCODnotFRPK = Guid.NewGuid();

			var organisationPk = TestDataCreator.CreateOrganisation(new string('o', OrgHeaderSchema.OH_Code.MaxLength), new string('o', OrgHeaderSchema.OH_FullName.MaxLength));
			var organisationPk1 = TestDataCreator.CreateOrganisation(new string('r', OrgHeaderSchema.OH_Code.MaxLength), new string('r', OrgHeaderSchema.OH_FullName.MaxLength));
			var organisationPk2 = TestDataCreator.CreateOrganisation(new string('g', OrgHeaderSchema.OH_Code.MaxLength), new string('g', OrgHeaderSchema.OH_FullName.MaxLength));
			var organisationPk3 = TestDataCreator.CreateOrganisation(new string('a', OrgHeaderSchema.OH_Code.MaxLength), new string('a', OrgHeaderSchema.OH_FullName.MaxLength));
			var organisationPk4 = TestDataCreator.CreateOrganisation(new string('n', OrgHeaderSchema.OH_Code.MaxLength), new string('n', OrgHeaderSchema.OH_FullName.MaxLength));
			var organisationPk5 = TestDataCreator.CreateOrganisation(new string('i', OrgHeaderSchema.OH_Code.MaxLength), new string('i', OrgHeaderSchema.OH_FullName.MaxLength));
			var organisationPk6 = TestDataCreator.CreateOrganisation(new string('s', OrgHeaderSchema.OH_Code.MaxLength), new string('s', OrgHeaderSchema.OH_FullName.MaxLength));

			var sql = $@"
DECLARE @frCompanyPK UNIQUEIDENTIFIER = NEWID();
INSERT INTO dbo.GlbCompany
    (GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)
VALUES
    (@frCompanyPK, 'FR', 'TES', 'FR company', getutcdate(), '~BP', getutcdate(), '~BP');

INSERT INTO CusPermitHeader
	(CPH_PK, CPH_RN_NKCountryCode, CPH_OH_PermitHolder, CPH_Number, CPH_Type, CPH_StartDate, CPH_SystemCreateTimeUtc, CPH_SystemCreateUser, CPH_SystemLastEditTimeUtc, CPH_SystemLastEditUser)
VALUES
	('{guaranteeCODPK}', 'FR', '{organisationPk}', '123445', 'COD', getutcdate(), getutcdate(), '~BP', getutcdate(), '~BP'),
	('{guaranteeCODAffiliateCountryPK}', 'GF', '{organisationPk1}', '123445', 'COD', getutcdate(), getutcdate(), '~BP', getutcdate(), '~BP'),
	('{guaranteeDEFPK}', 'FR', '{organisationPk2}', '123446', 'DEF', getutcdate(), getutcdate(), '~BP', getutcdate(), '~BP'),
	('{guaranteeOTHPK}', 'FR', '{organisationPk3}', '123447', 'CAN', getutcdate(), getutcdate(), '~BP', getutcdate(), '~BP'),
	('{guaranteeCODWithoutRuleCodePK}', 'FR', '{organisationPk4}', '123448', 'COD', getutcdate(), getutcdate(), '~BP', getutcdate(), '~BP'),
	('{guaranteeWithEntPK}', 'FR', '{organisationPk5}', '123449', 'COD', getutcdate(), getutcdate(), '~BP', getutcdate(), '~BP'),
	('{guaranteeCODNotFR}', 'DE', '{organisationPk6}', '123450', 'COD', getutcdate(), getutcdate(), '~BP', getutcdate(), '~BP');

INSERT INTO CusPermitRule
	(CPR_PK, CPR_CPH_PermitHeader, CPR_RuleCode, CPR_ValueFrom, CPR_SystemCreateTimeUtc, CPR_SystemCreateUser, CPR_SystemLastEditTimeUtc, CPR_SystemLastEditUser)
VALUES
	('{ruleCodeForCODPK}', '{guaranteeCODPK}', 'zzz', 'DG', getutcdate(), '~BP', getutcdate(), '~BP'),
	('{ruleCodeForCODAffiliateCountryPK}', '{guaranteeCODAffiliateCountryPK}', 'zzz', 'DG', getutcdate(), '~BP', getutcdate(), '~BP'),
	('{ruleCodeForDEFPK}', '{guaranteeDEFPK}', 'zzz', 'DG', getutcdate(), '~BP', getutcdate(), '~BP'),
	('{ruleCodeForOTHPK}', '{guaranteeOTHPK}', 'zzz', 'DG', getutcdate(), '~BP', getutcdate(), '~BP'),
	('{ruleCodeENTPK}', '{guaranteeWithEntPK}', 'ENT', 'EXP', getutcdate(), '~BP', getutcdate(), '~BP'),
	('{ruleCodeForCODnotFRPK}', '{guaranteeCODNotFR}','zzz', 'DG', getutcdate(), '~BP', getutcdate(), '~BP');";

			Db.Connection.ExecuteNonQuery(sql);
		}

		protected override void AssertTransformationResults()
		{
			var entResults = new List<(Guid, string, string)>();
			TestConnection.ExecuteReader($"SELECT CPR_CPH_PermitHeader, CPR_RuleCode, CPR_ValueFrom FROM CusPermitRule where CPR_RuleCode = 'ENT' and CPR_CPH_PermitHeader in ('{guaranteeCODPK}', '{guaranteeDEFPK}', '{guaranteeOTHPK}', '{guaranteeCODWithoutRuleCodePK}', '{guaranteeWithEntPK}','{guaranteeCODNotFR}','{guaranteeCODAffiliateCountryPK}' )",
				reader => entResults.Add(((Guid)reader["CPR_CPH_PermitHeader"], (string)reader["CPR_RuleCode"], (string)reader["CPR_ValueFrom"])));
			AssertContainsExactElementsInAnyOrder("New ENT rules should have 'Value From' equal to BTH, old ENT rules should keep their values.", new List<(Guid, string, string)>()
			{
				(guaranteeCODPK, "ENT", "BTH"),
				(guaranteeCODAffiliateCountryPK, "ENT", "BTH"),
				(guaranteeDEFPK, "ENT", "BTH"),
				(guaranteeCODWithoutRuleCodePK, "ENT", "BTH"),
				(guaranteeWithEntPK, "ENT", "EXP"),
			}, entResults);

			var othResults = new List<(Guid, string, string)>();
			TestConnection.ExecuteReader($"SELECT CPR_CPH_PermitHeader, CPR_RuleCode, CPR_ValueFrom FROM CusPermitRule where CPR_RuleCode <> 'ENT' and CPR_CPH_PermitHeader in ('{guaranteeCODPK}', '{guaranteeDEFPK}', '{guaranteeOTHPK}', '{guaranteeCODWithoutRuleCodePK}', '{guaranteeWithEntPK}','{guaranteeCODNotFR}','{guaranteeCODAffiliateCountryPK}' )",
				reader => othResults.Add(((Guid)reader["CPR_CPH_PermitHeader"], (string)reader["CPR_RuleCode"], (string)reader["CPR_ValueFrom"])));
			AssertContainsExactElementsInAnyOrder("Other rules codes should be left unchanged.", new List<(Guid, string, string)>()
			{
				(guaranteeCODPK, "zzz", "DG"),
				(guaranteeCODAffiliateCountryPK, "zzz", "DG"),
				(guaranteeDEFPK, "zzz", "DG"),
				(guaranteeOTHPK, "zzz", "DG"),
				(guaranteeCODNotFR, "zzz", "DG"),
			}, othResults);
		}

		Guid guaranteeCODPK;
		Guid guaranteeCODAffiliateCountryPK;
		Guid guaranteeCODWithoutRuleCodePK;
		Guid guaranteeDEFPK;
		Guid guaranteeOTHPK;
		Guid guaranteeWithEntPK;
		Guid guaranteeCODNotFR;

		Guid ruleCodeForCODPK;
		Guid ruleCodeForCODAffiliateCountryPK;
		Guid ruleCodeForDEFPK;
		Guid ruleCodeForOTHPK;
		Guid ruleCodeENTPK;
		Guid ruleCodeForCODnotFRPK;
	}
}

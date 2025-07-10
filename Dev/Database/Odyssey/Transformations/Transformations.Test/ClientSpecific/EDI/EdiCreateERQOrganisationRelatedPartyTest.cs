using System;
using CargoWise.Data;
using Enterprise.Client.EDI.DbUpgrader;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI;

[TestedType(typeof(EdiCreateERQOrganisationRelatedParty))]
public class EdiCreateERQOrganisationRelatedPartyTest : DataTransformationTestCase
{
	Guid relatedPartyPk1, relatedPartyPk2, relatedPartyPk3;
	Guid parentOrgPk1, parentOrgPk2, relatedPartyOrg1, relatedPartyOrg2;

	protected override void PrepareTestData()
	{
		DbObjectCreator.CreateTableIfNotExists(Db.Connection, "EdiBilledUsage", "CREATE TABLE EdiBilledUsage (ID INT)");

		var creator = new TransformationTestDataCreator();
		parentOrgPk1 = creator.CreateOrg("ABC", "Abc org name");
		relatedPartyOrg1 = creator.CreateOrg("PTY", "Pty org name");
		parentOrgPk2 = creator.CreateOrg("EFG", "Efg full name");
		relatedPartyOrg2 = creator.CreateOrg("TYU", "Tyu full name");

		relatedPartyPk1 = creator.CreateOrgRelatedParty("MNG", "PIC", "FWD", "ULD", relatedPartyOrg1, parentOrgPk1);
		relatedPartyPk2 = creator.CreateOrgRelatedParty("IFT", "PIC", "AIR", "ULD", relatedPartyOrg1, parentOrgPk1);
		relatedPartyPk3 = creator.CreateOrgRelatedParty("MNG", "PIC", "AIR", "ULD", relatedPartyOrg2, parentOrgPk2);
	}

	protected override void AssertTransformationResults()
	{
		AssertEquals("No change should be done in the original record", "MNG", TestConnection.ExecuteScalar($"SELECT PR_PartyType FROM dbo.OrgRelatedParty WHERE PR_PK = '{relatedPartyPk1}'"));
		AssertEquals("No change should be done in the original record", "IFT", TestConnection.ExecuteScalar($"SELECT PR_PartyType FROM dbo.OrgRelatedParty WHERE PR_PK = '{relatedPartyPk2}'"));
		AssertEquals("No change should be done in the original record", "MNG", TestConnection.ExecuteScalar($"SELECT PR_PartyType FROM dbo.OrgRelatedParty WHERE PR_PK = '{relatedPartyPk3}'"));

		AssertEquals("Should add a new row for each Related Party with type MNG", 2, TestConnection.ExecuteScalar($"SELECT COUNT(PR_PK) FROM dbo.OrgRelatedParty WHERE PR_PartyType = 'ERQ'"));
		AssertEquals(1, TestConnection.ExecuteScalar($"SELECT COUNT(PR_PK) FROM dbo.OrgRelatedParty WHERE PR_PartyType = 'ERQ' AND PR_OH_Parent = '{parentOrgPk1}'"));
		AssertEquals(1, TestConnection.ExecuteScalar($"SELECT COUNT(PR_PK) FROM dbo.OrgRelatedParty WHERE PR_PartyType = 'ERQ' AND PR_OH_Parent = '{parentOrgPk2}'"));
	}

	protected override DataTransformation GetNewTestTransformationInstance()
	{
		return new EdiCreateERQOrganisationRelatedParty();
	}
}

[TestedType(typeof(EdiCreateERQOrganisationRelatedParty))]
public class EdiCreateERQOrganisationRelatedPartyNonEdiTest : EdiCreateERQOrganisationRelatedPartyTest
{
	Guid relatedPartyPk1, relatedPartyPk2, relatedPartyPk3;
	Guid parentOrgPk1, parentOrgPk2, relatedPartyOrg1, relatedPartyOrg2;

	protected override void PrepareTestData()
	{
		DbObjectCreator.DropTableIfExists(Db.Connection, "EdiBilledDiscount");
		DbObjectCreator.DropTableIfExists(Db.Connection, "EdiBilledUsage");

		var creator = new TransformationTestDataCreator();
		parentOrgPk1 = creator.CreateOrg("ABC", "Abc org name");
		relatedPartyOrg1 = creator.CreateOrg("PTY", "Pty org name");
		parentOrgPk2 = creator.CreateOrg("EFG", "Efg full name");
		relatedPartyOrg2 = creator.CreateOrg("TYU", "Tyu full name");

		relatedPartyPk1 = creator.CreateOrgRelatedParty("MNG", "PIC", "FWD", "ULD", relatedPartyOrg1, parentOrgPk1);
		relatedPartyPk2 = creator.CreateOrgRelatedParty("IFT", "PIC", "AIR", "ULD", relatedPartyOrg1, parentOrgPk1);
		relatedPartyPk3 = creator.CreateOrgRelatedParty("MNG", "PIC", "AIR", "ULD", relatedPartyOrg2, parentOrgPk2);
	}

	protected override void AssertTransformationResults()
	{
		AssertEquals("No change should be done in the original record", "MNG", TestConnection.ExecuteScalar($"SELECT PR_PartyType FROM dbo.OrgRelatedParty WHERE PR_PK = '{relatedPartyPk1}'"));
		AssertEquals("No change should be done in the original record", "IFT", TestConnection.ExecuteScalar($"SELECT PR_PartyType FROM dbo.OrgRelatedParty WHERE PR_PK = '{relatedPartyPk2}'"));
		AssertEquals("No change should be done in the original record", "MNG", TestConnection.ExecuteScalar($"SELECT PR_PartyType FROM dbo.OrgRelatedParty WHERE PR_PK = '{relatedPartyPk3}'"));

		AssertEquals("Should not add ERQ records in non EDI databases", 0, TestConnection.ExecuteScalar($"SELECT COUNT(PR_PK) FROM dbo.OrgRelatedParty WHERE PR_PartyType = 'ERQ'"));
	}
}

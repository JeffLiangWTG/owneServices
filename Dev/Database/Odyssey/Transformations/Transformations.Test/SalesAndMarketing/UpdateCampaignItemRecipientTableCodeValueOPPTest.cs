using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.SalesAndMarketing
{
	[TestedType(typeof(UpdateCampaignItemRecipientTableCodeValueOPP))]
	public class UpdateCampaignItemRecipientTableCodeValueOPPTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
	=> new UpdateCampaignItemRecipientTableCodeValueOPP();

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update Campaign Item Recipient Table Code Value OPP to a contact table code_1] ON [dbo].[GlbCompanyCampaignItem] ([G8_RecipientTableCode]) INCLUDE ([G8_PK], [G8_RecipientID], [G8_SystemLastEditTimeUtc], [G8_SystemLastEditUser]) WHERE ([G8_RecipientTableCode]='OPP') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void PrepareTestData()
		{
			var helper = new TransformationTestDataCreator();
			var companyPk = helper.CreateCompany("TST", "AU");
			var orgPk = helper.CreateOrg("TST01");
			var contact1Pk = helper.CreateContact("Fred Nerk", orgPk);
			var contact2Pk = helper.CreateContact("Sam Brown", orgPk);
			var staff1Pk = helper.CreateStaff("jbrown", "JBR");
			var staff2Pk = helper.CreateStaff("rbaxter", "RBX");
			var person1Pk = helper.CreatePerson("Rita Smith");
			var person2Pk = helper.CreatePerson("George Bentley");
			var applicant1Pk = helper.CreateHRJobApplicant("rsmith@a.com", person1Pk);
			var applicant2Pk = helper.CreateHRJobApplicant("gbentley@a.com", person2Pk);
			var register1Pk = helper.CreateSalesEnquiry("I00101000", "Test", "Harry Porter", "INQ", "");
			var register2Pk = helper.CreateSalesEnquiry("I00101001", "Test", "Karen Jones", "INQ", "");

			var oppCampaignPk = helper.CreateGlbCompanyCampaign("Campaign With OPP", "OPP", companyPk, "CMP01000001");
			oppCampaignItem1Pk = helper.CreateGlbCompanyCampaignItem(oppCampaignPk, contact1Pk, "OPP");
			oppCampaignItem2Pk = helper.CreateGlbCompanyCampaignItem(oppCampaignPk, staff1Pk, "OPP");
			oppCampaignItem3Pk = helper.CreateGlbCompanyCampaignItem(oppCampaignPk, applicant1Pk, "OPP");
			oppCampaignItem4Pk = helper.CreateGlbCompanyCampaignItem(oppCampaignPk, register1Pk, "OPP");

			var campaignPk = helper.CreateGlbCompanyCampaign("Normal Campaign", "BRD", companyPk, "CMP01000002");
			campaignItem1Pk = helper.CreateGlbCompanyCampaignItem(campaignPk, contact2Pk, "OC");
			campaignItem2Pk = helper.CreateGlbCompanyCampaignItem(campaignPk, staff2Pk, "GS");
			campaignItem3Pk = helper.CreateGlbCompanyCampaignItem(campaignPk, applicant2Pk, "HA");
			campaignItem4Pk = helper.CreateGlbCompanyCampaignItem(campaignPk, register2Pk, "O1");
		}

		protected override void AssertTransformationResults()
		{
			AssertCampaignItemResult("Campaign With OPP, Fred Nerk", oppCampaignItem1Pk, "OC");
			AssertCampaignItemResult("Campaign With OPP, jbrown", oppCampaignItem2Pk, "GS");
			AssertCampaignItemResult("Campaign With OPP, Rita Smoth", oppCampaignItem3Pk, "HA");
			AssertCampaignItemResult("Campaign With OPP, Harry Porter", oppCampaignItem4Pk, "O1");
			AssertCampaignItemResult("Normal Campaign, Sam Brown", campaignItem1Pk, "OC");
			AssertCampaignItemResult("Normal Campaign, rbaxter", campaignItem2Pk, "GS");
			AssertCampaignItemResult("Normal Campaign, George Bentley", campaignItem3Pk, "HA");
			AssertCampaignItemResult("Normal Campaign, Karen Jones", campaignItem4Pk, "O1");
		}

		void AssertCampaignItemResult(string description, Guid pk, string expectedRecipientTableCode)
		{
			var sql = $@"SELECT {GlbCompanyCampaignItemSchema.Constants.G8_RecipientTableCode} FROM dbo.GlbCompanyCampaignItem
						WHERE {GlbCompanyCampaignItemSchema.Constants.PK} = @pk";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				var tableCode = (string)command.ExecuteScalar();

				AssertEquals($"Item: {description}", expectedRecipientTableCode, tableCode);
			}
		}

		Guid oppCampaignItem1Pk;
		Guid oppCampaignItem2Pk;
		Guid oppCampaignItem3Pk;
		Guid oppCampaignItem4Pk;
		Guid campaignItem1Pk;
		Guid campaignItem2Pk;
		Guid campaignItem3Pk;
		Guid campaignItem4Pk;
	}
}

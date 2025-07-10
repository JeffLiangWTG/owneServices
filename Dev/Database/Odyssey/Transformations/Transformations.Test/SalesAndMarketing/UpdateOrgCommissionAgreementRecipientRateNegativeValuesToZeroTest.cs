using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.SalesAndMarketing
{
	[TestedType(typeof(UpdateOrgCommissionAgreementRecipientRateNegativeValuesToZero))]
	class UpdateOrgCommissionAgreementRecipientRateNegativeValuesToZeroTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateOrgCommissionAgreementRecipientRateNegativeValuesToZero();
		}

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(OrgCommissionAgreementRecipientRateSchema.Constants.TableName, "Constraint_CAT_CommissionAmount");
			DBTransformationTestHelper.DropConstraintIfExists(OrgCommissionAgreementRecipientRateSchema.Constants.TableName, "Constraint_CAT_CommissionPercentage");

			var helper = new TransformationTestDataCreator();
			var companyPK = helper.CreateGlbCompany("ABC", "AU");
			var orgPK = helper.CreateOrgHeader("ABC", "headerFullNameABC");
			var oppPK = helper.CreateOrgOpportunity("uniqueOpportunityId", orgPK, companyPK);
			var orgCommissionAgreementPK = helper.CreateOrgCommissionAgreement(oppPK, "OCA1", orgPK);
			var orgCommissionAgreementRecipientPK = helper.CreateOrgCommissionAgreementRecipient(orgCommissionAgreementPK);

			testOrgCommissionAgreementRecipientRate1PK = helper.CreateOrgCommissionAgreementRecipientRate(orgCommissionAgreementRecipientPK, commissionAmount: 1);
			testOrgCommissionAgreementRecipientRate2PK = helper.CreateOrgCommissionAgreementRecipientRate(orgCommissionAgreementRecipientPK, commissionPercentage: 2);
			testOrgCommissionAgreementRecipientRate3PK = helper.CreateOrgCommissionAgreementRecipientRate(orgCommissionAgreementRecipientPK, commissionAmount: -1);
			testOrgCommissionAgreementRecipientRate4PK = helper.CreateOrgCommissionAgreementRecipientRate(orgCommissionAgreementRecipientPK, commissionPercentage: -2);
		}

		protected override void AssertTransformationResults()
		{
			var testNonNegativeCommissionAmount = GetDataRow(testOrgCommissionAgreementRecipientRate1PK);
			var testNonNegativeCommissionPercentage = GetDataRow(testOrgCommissionAgreementRecipientRate2PK);
			CombineAssertions("All non-negative column values should not be changed.", () =>
			{
				AssertEquals((decimal)1, testNonNegativeCommissionAmount["CAT_CommissionAmount"]);
				AssertEquals((decimal)2, testNonNegativeCommissionPercentage["CAT_CommissionPercentage"]);
			});

			var testNegativeCommissionAmount = GetDataRow(testOrgCommissionAgreementRecipientRate3PK);
			var testNegativeCommissionPercentage = GetDataRow(testOrgCommissionAgreementRecipientRate4PK);
			CombineAssertions("All negative column values should be replaced with 0.", () =>
			{
				AssertEquals((decimal)0, testNegativeCommissionAmount["CAT_CommissionAmount"]);
				AssertEquals((decimal)0, testNegativeCommissionPercentage["CAT_CommissionPercentage"]);
			});
		}

		DataRow GetDataRow(Guid pK)
		{
			var dataTable = new DataTable();
			var sql = $"SELECT CAT_CommissionAmount, CAT_CommissionPercentage FROM dbo.OrgCommissionAgreementRecipientRate WHERE CAT_PK = '{pK}'";
			using (var cmd = Db.Connection.Command(sql))
			using (var adapter = cmd.NewDataAdapter())
			{
				adapter.Fill(dataTable);
			}

			return dataTable.Rows[0];
		}

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative OrgCommissionAgreementRecipientRate columns_1] ON [dbo].[OrgCommissionAgreementRecipientRate] ([CAT_CommissionAmount]) WHERE ([CAT_CommissionAmount]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative OrgCommissionAgreementRecipientRate columns_2] ON [dbo].[OrgCommissionAgreementRecipientRate] ([CAT_CommissionPercentage]) WHERE ([CAT_CommissionPercentage]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		};

		Guid testOrgCommissionAgreementRecipientRate1PK;
		Guid testOrgCommissionAgreementRecipientRate2PK;
		Guid testOrgCommissionAgreementRecipientRate3PK;
		Guid testOrgCommissionAgreementRecipientRate4PK;
	}
}

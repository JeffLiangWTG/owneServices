using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
namespace Enterprise.DbUpgrader.Transformations.Test.SalesAndMarketing.Testing
{
	[TestedType(typeof(RemoveCommisionAgreementItemsWhereCodeIsEmpty))]
	public class RemoveCommisionAgreementItemsWhereCodeIsEmptyTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertExpectedAgreementResults(agreement1, true);
			AssertExpectedAgreementResults(agreement2, true);
			AssertExpectedItemResults(item1, true);
			AssertExpectedItemResults(item2, false);
			AssertExpectedItemResults(item3, true);
			AssertExpectedItemResults(item4, false);
			AssertExpectedItemResults(item5, true);
			AssertExpectedItemResults(item6, false);
			AssertExpectedConditionResults(con1, true);
			AssertExpectedConditionResults(con2, false);
			AssertExpectedConditionResults(con3, true);
			AssertExpectedConditionResults(con4, false);
			AssertExpectedConditionResults(con5, true);
			AssertExpectedConditionResults(con6, false);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
			=> new RemoveCommisionAgreementItemsWhereCodeIsEmpty();

		void AssertExpectedItemResults(Guid pk, bool exists)
		{
			var sql = Db.Connection.Exists($@"FROM dbo.OrgCommissionAgreementItem
						WHERE {OrgCommissionAgreementItemSchema.Constants.PK} = '{pk}'");
			
			AssertEquals(exists, sql);
		}

		void AssertExpectedConditionResults(Guid pk, bool exists)
		{
			var sql = Db.Connection.Exists($@"FROM dbo.OrgCommissionAgreementItemCondition
						WHERE {OrgCommissionAgreementItemConditionSchema.Constants.PK} = '{pk}'");

			AssertEquals(exists, sql);
		}

		void AssertExpectedAgreementResults(Guid pk, bool exists)
		{
			var sql = Db.Connection.Exists($@"FROM dbo.OrgCommissionAgreement
						WHERE {OrgCommissionAgreementSchema.Constants.PK} = '{pk}'");

			AssertEquals(exists, sql);
		}

		protected override void PrepareTestData()
		{
			using (TestWhsDataSetupHelper.DisableConstraint(OrgCommissionAgreementItemSchema.Constants.TableName, "Constraint_CAI_Code"))
			{
				var helper = new TransformationTestDataCreator();
				var companyPk = helper.CreateCompany("TST", "AU");
				var orgPk = helper.CreateOrg("TESTORG");
				var opp = helper.CreateOrgOpportunity("New Opportunity", orgPk, companyPk);

				agreement1 = helper.CreateOrgCommissionAgreement(opp, "Test Agreement 1", orgPk);
				agreement2 = helper.CreateOrgCommissionAgreement(opp, "Test Agreement 2", orgPk);

				item1 = helper.CreateOrgCommissionAgreementItem(agreement1, "CTO", "CA0");
				item2 = helper.CreateOrgCommissionAgreementItem(agreement2, "", "CA0");
				item3 = helper.CreateOrgCommissionAgreementItem(item1, "CTO", "CAI");
				item4 = helper.CreateOrgCommissionAgreementItem(item2, "CSH", "CAI");
				item5 = helper.CreateOrgCommissionAgreementItem(item3, "CTO", "CAI");
				item6 = helper.CreateOrgCommissionAgreementItem(item4, "CTO", "CAI");

				con1 = helper.CreateOrgCommissionAgreementItemCondition(item1, "AIR");
				con2 = helper.CreateOrgCommissionAgreementItemCondition(item2, "SEA");
				con3 = helper.CreateOrgCommissionAgreementItemCondition(item3, "CON");
				con4 = helper.CreateOrgCommissionAgreementItemCondition(item4, "SHP");
				con5 = helper.CreateOrgCommissionAgreementItemCondition(item5, "SHP");
				con6 = helper.CreateOrgCommissionAgreementItemCondition(item6, "SHP");
			}
		}

		Guid agreement1;
		Guid agreement2;
		Guid item1;
		Guid item2;
		Guid item3;
		Guid item4;
		Guid item5;
		Guid item6;
		Guid con1;
		Guid con2;
		Guid con3;
		Guid con4;
		Guid con5;
		Guid con6;
	}
}

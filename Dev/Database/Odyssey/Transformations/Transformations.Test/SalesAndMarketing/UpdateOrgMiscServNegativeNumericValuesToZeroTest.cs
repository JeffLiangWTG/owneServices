using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.SalesAndMarketing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.SalesAndMarketing
{
	[TestedType(typeof(UpdateOrgMiscServNegativeNumericValuesToZero))]
	class UpdateOrgMiscServNegativeNumericValuesToZeroTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative OrgMiscServ Client Intel columns_1] ON [dbo].[OrgMiscServ] ([OM_CICapitalEmployed]) WHERE ([OM_CICapitalEmployed]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative OrgMiscServ Client Intel columns_2] ON [dbo].[OrgMiscServ] ([OM_CIEstimatedStaffThisCountry]) WHERE ([OM_CIEstimatedStaffThisCountry]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative OrgMiscServ Client Intel columns_3] ON [dbo].[OrgMiscServ] ([OM_CIEstimatedStaffThisLocation]) WHERE ([OM_CIEstimatedStaffThisLocation]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative OrgMiscServ Client Intel columns_4] ON [dbo].[OrgMiscServ] ([OM_CITurnover]) WHERE ([OM_CITurnover]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative OrgMiscServ Client Intel columns_5] ON [dbo].[OrgMiscServ] ([OM_CIProfit]) WHERE ([OM_CIProfit]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override DataTransformation GetNewTestTransformationInstance()
			=> new UpdateOrgMiscServNegativeNumericValuesToZero();

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(OrgMiscServSchema.Constants.TableName, "Constraint_OM_CICapitalEmployed");
			DBTransformationTestHelper.DropConstraintIfExists(OrgMiscServSchema.Constants.TableName, "Constraint_OM_CIEstimatedStaffThisCountry");
			DBTransformationTestHelper.DropConstraintIfExists(OrgMiscServSchema.Constants.TableName, "Constraint_OM_CIEstimatedStaffThisLocation");
			DBTransformationTestHelper.DropConstraintIfExists(OrgMiscServSchema.Constants.TableName, "Constraint_OM_CIProfit");
			DBTransformationTestHelper.DropConstraintIfExists(OrgMiscServSchema.Constants.TableName, "Constraint_OM_CITurnover");

			var helper = new TransformationTestDataCreator();
			var orgHeader1 = helper.CreateOrgHeader("US1", "US WHS Org1");
			var orgHeader2 = helper.CreateOrgHeader("US2", "US WHS Org2");

			orgMiscServ1 = helper.CreateOrgMiscServWithClientIntel(orgHeader1, -1.0m, -1, -1, -1.0m, -1.0m);
			orgMiscServ2 = helper.CreateOrgMiscServWithClientIntel(orgHeader2, 0.0m, 2, 3, 4.0m, 5.0m);
		}

		protected override void AssertTransformationResults()
		{
			var testAllNegative = GetDataRow(orgMiscServ1);

			CombineAssertions("All negative column values should be replaced with 0.", () =>
			{
				AssertEquals(testAllNegative["OM_CICapitalEmployed"], 0.0m);
				AssertEquals(testAllNegative["OM_CIEstimatedStaffThisCountry"], (short)0);
				AssertEquals(testAllNegative["OM_CIEstimatedStaffThisLocation"], (short)0);
				AssertEquals(testAllNegative["OM_CITurnover"], 0.0m);
				AssertEquals(testAllNegative["OM_CIProfit"], 0.0m);
			});

			var testAllNonNegative = GetDataRow(orgMiscServ2);

			CombineAssertions("All non-negative column values should not be changed.", () =>
			{
				AssertEquals(testAllNonNegative["OM_CICapitalEmployed"], 0.0m);
				AssertEquals(testAllNonNegative["OM_CIEstimatedStaffThisCountry"], (short)2);
				AssertEquals(testAllNonNegative["OM_CIEstimatedStaffThisLocation"], (short)3);
				AssertEquals(testAllNonNegative["OM_CITurnover"], 4.0m);
				AssertEquals(testAllNonNegative["OM_CIProfit"], 5.0m);
			});
		}

		DataRow GetDataRow(Guid pk)
		{
			var sql = $"SELECT OM_CICapitalEmployed, OM_CIEstimatedStaffThisCountry, OM_CIEstimatedStaffThisLocation, OM_CITurnover, OM_CIProfit FROM dbo.OrgMiscServ WHERE OM_PK = '{pk}'";
			var dataTable = new DataTable();

			using (var cmd = Db.Connection.Command(sql))
			using (var adapter = cmd.NewDataAdapter())
			{
				adapter.Fill(dataTable);
			}

			return dataTable.Rows[0];
		}

		Guid orgMiscServ1;
		Guid orgMiscServ2;
	}
}

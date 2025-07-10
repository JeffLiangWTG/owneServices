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
	[TestedType(typeof(UpdateOrgMiscServCMNegativeNumericValuesToZero))]
	class UpdateOrgMiscServCMNegativeNumericValuesToZeroTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative OrgMiscServ CM columns_1] ON [dbo].[OrgMiscServ] ([OM_CMAcheivableClientRevenue]) INCLUDE ([OM_SystemLastEditTimeUtc], [OM_SystemLastEditUser]) WHERE ([OM_CMAcheivableClientRevenue]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative OrgMiscServ CM columns_2] ON [dbo].[OrgMiscServ] ([OM_CMConsultingRevenue]) INCLUDE ([OM_SystemLastEditTimeUtc], [OM_SystemLastEditUser]) WHERE ([OM_CMConsultingRevenue]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative OrgMiscServ CM columns_3] ON [dbo].[OrgMiscServ] ([OM_CMEstimatedProfit]) INCLUDE ([OM_SystemLastEditTimeUtc], [OM_SystemLastEditUser]) WHERE ([OM_CMEstimatedProfit]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative OrgMiscServ CM columns_4] ON [dbo].[OrgMiscServ] ([OM_CMNoOfEmployees]) INCLUDE ([OM_SystemLastEditTimeUtc], [OM_SystemLastEditUser]) WHERE ([OM_CMNoOfEmployees]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative OrgMiscServ CM columns_5] ON [dbo].[OrgMiscServ] ([OM_CMPaidUpCapital]) INCLUDE ([OM_SystemLastEditTimeUtc], [OM_SystemLastEditUser]) WHERE ([OM_CMPaidUpCapital]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative OrgMiscServ CM columns_6] ON [dbo].[OrgMiscServ] ([OM_CMPercentage]) INCLUDE ([OM_SystemLastEditTimeUtc], [OM_SystemLastEditUser]) WHERE ([OM_CMPercentage]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative OrgMiscServ CM columns_7] ON [dbo].[OrgMiscServ] ([OM_CMTotalClientRevenue]) INCLUDE ([OM_SystemLastEditTimeUtc], [OM_SystemLastEditUser]) WHERE ([OM_CMTotalClientRevenue]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Set negative values to zero for non-negative OrgMiscServ CM columns_8] ON [dbo].[OrgMiscServ] ([OM_CMWarehouseRevenue]) INCLUDE ([OM_SystemLastEditTimeUtc], [OM_SystemLastEditUser]) WHERE ([OM_CMWarehouseRevenue]<(0)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override DataTransformation GetNewTestTransformationInstance()
			=> new UpdateOrgMiscServCMNegativeNumericValuesToZero();

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(OrgMiscServSchema.Constants.TableName, "Constraint_OM_CMAcheivableClientRevenue");
			DBTransformationTestHelper.DropConstraintIfExists(OrgMiscServSchema.Constants.TableName, "Constraint_OM_CMConsultingRevenue");
			DBTransformationTestHelper.DropConstraintIfExists(OrgMiscServSchema.Constants.TableName, "Constraint_OM_CMEstimatedProfit");
			DBTransformationTestHelper.DropConstraintIfExists(OrgMiscServSchema.Constants.TableName, "Constraint_OM_CMNoOfEmployees");
			DBTransformationTestHelper.DropConstraintIfExists(OrgMiscServSchema.Constants.TableName, "Constraint_OM_CMPaidUpCapital");
			DBTransformationTestHelper.DropConstraintIfExists(OrgMiscServSchema.Constants.TableName, "Constraint_OM_CMPercentage");
			DBTransformationTestHelper.DropConstraintIfExists(OrgMiscServSchema.Constants.TableName, "Constraint_OM_CMTotalClientRevenue");
			DBTransformationTestHelper.DropConstraintIfExists(OrgMiscServSchema.Constants.TableName, "Constraint_OM_CMWarehouseRevenue");

			var helper = new TransformationTestDataCreator();
			var orgHeader1 = helper.CreateOrgHeader("US1", "US WHS Org1");
			var orgHeader2 = helper.CreateOrgHeader("US2", "US WHS Org2");

			orgMiscServ1 = helper.CreateOrgMiscServWithCM(orgHeader1, -1.0m, -1, -1, -1, -1.0m, -1.0m, -1.0m, -1.0m);
			orgMiscServ2 = helper.CreateOrgMiscServWithCM(orgHeader2, 0.0m, 2.0m, 3.0m, 4, 5.0m, 1.0m, 3.0m, 4.0m);
		}

		protected override void AssertTransformationResults()
		{
			var testAllNegative = GetDataRow(orgMiscServ1);

			CombineAssertions("All negative column values should be replaced with 0.", () =>
			{
				AssertEquals(testAllNegative["OM_CMAcheivableClientRevenue"], 0.0m);
				AssertEquals(testAllNegative["OM_CMConsultingRevenue"], 0.0m);
				AssertEquals(testAllNegative["OM_CMEstimatedProfit"], 0.0m);
				AssertEquals(testAllNegative["OM_CMNoOfEmployees"], 0);
				AssertEquals(testAllNegative["OM_CMPaidUpCapital"], 0.0m);
				AssertEquals(testAllNegative["OM_CMPercentage"], 0.0m);
				AssertEquals(testAllNegative["OM_CMTotalClientRevenue"], 0.0m);
				AssertEquals(testAllNegative["OM_CMWarehouseRevenue"], 0.0m);
			});

			var testAllNonNegative = GetDataRow(orgMiscServ2);

			CombineAssertions("All non-negative column values should not be changed.", () =>
			{
				AssertEquals(testAllNonNegative["OM_CMAcheivableClientRevenue"], 0.0m);
				AssertEquals(testAllNonNegative["OM_CMConsultingRevenue"], 2.0m);
				AssertEquals(testAllNonNegative["OM_CMEstimatedProfit"], 3.0m);
				AssertEquals(testAllNonNegative["OM_CMNoOfEmployees"], 4);
				AssertEquals(testAllNonNegative["OM_CMPaidUpCapital"], 5.0m);
				AssertEquals(testAllNonNegative["OM_CMPercentage"],1.0m);
				AssertEquals(testAllNonNegative["OM_CMTotalClientRevenue"], 3.0m);
				AssertEquals(testAllNonNegative["OM_CMWarehouseRevenue"], 4.0m);
			});
		}

		DataRow GetDataRow(Guid pk)
		{
			var sql = $"SELECT OM_CMAcheivableClientRevenue, OM_CMConsultingRevenue, OM_CMEstimatedProfit, OM_CMNoOfEmployees, OM_CMPaidUpCapital, OM_CMPercentage, OM_CMTotalClientRevenue, OM_CMWarehouseRevenue FROM dbo.OrgMiscServ WHERE OM_PK = '{pk}'";
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

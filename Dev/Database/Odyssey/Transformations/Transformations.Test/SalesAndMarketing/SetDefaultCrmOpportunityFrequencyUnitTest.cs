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
	[TestedType(typeof(SetDefaultCrmOpportunityFrequencyUnit))]
	class SetDefaultCrmOpportunityFrequencyUnitTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Set default opportunity overall Frequency unit to ('Y')_1] ON [dbo].[CrmOpportunity] ([COP_OverallFrequencyUnit]) WHERE ([COP_OverallFrequencyUnit]='') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override DataTransformation GetNewTestTransformationInstance()
			=> new SetDefaultCrmOpportunityFrequencyUnit();

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(CrmOpportunitySchema.Constants.TableName, "Constraint_COP_OverallFrequencyUnit");

			var helper = new TransformationTestDataCreator();
			var orgHeader = helper.CreateOrgHeader("US1", "US WHS Org1");
			var companyPK = helper.CreateCompany("VN1", "VN");

			crmOpportunity1 = helper.CreateCrmOpportunity("O001", "Test Opportunity one", orgHeader, companyPK, "");
			crmOpportunity2 = helper.CreateCrmOpportunity("O002", "Test Opportunity two", orgHeader, companyPK, "M");
		}

		protected override void AssertTransformationResults()
		{
			var testEmptyOverallFrequencyUnit = GetDataRow(crmOpportunity1);

			CombineAssertions("All empty value will be replaced by Y", () =>
			{
				AssertEquals(testEmptyOverallFrequencyUnit["COP_OverallFrequencyUnit"], "Y");
			});

			var testNonEmptyOverallFrequencyUnit = GetDataRow(crmOpportunity2);

			CombineAssertions("All non empty value should not be changed.", () =>
			{
				AssertEquals(testNonEmptyOverallFrequencyUnit["COP_OverallFrequencyUnit"], "M");
			});
		}

		DataRow GetDataRow(Guid pk)
		{
			var sql = $"SELECT COP_OverallFrequencyUnit FROM dbo.CrmOpportunity WHERE COP_PK = '{pk}'";
			var dataTable = new DataTable();

			using (var cmd = Db.Connection.Command(sql))
			using (var adapter = cmd.NewDataAdapter())
			{
				adapter.Fill(dataTable);
			}

			return dataTable.Rows[0];
		}

		Guid crmOpportunity1;
		Guid crmOpportunity2;
	}
}

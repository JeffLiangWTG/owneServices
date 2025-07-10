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
	[TestedType(typeof(PopulateCommissionHeaderJobNumber))]
	public class PopulateCommissionHeaderJobNumberTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
	=> new PopulateCommissionHeaderJobNumber();

		protected override void PrepareTestData()
		{
			var companyPK = (Guid)Db.Connection.Command("SELECT TOP(1) GC_PK FROM dbo.GlbCompany").ExecuteScalar();
			var branchPK = (Guid)Db.Connection.Command($"SELECT TOP(1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = '{companyPK}'").ExecuteScalar();
			var departmentPK = (Guid)Db.Connection.Command("SELECT TOP(1) GE_PK FROM dbo.GlbDepartment").ExecuteScalar();

			var helper = new TransformationTestDataCreator();

			var transactionHeader1PK = helper.CreateAccTransactionHeader("AP", "INV", branchPK, companyPK, departmentPK, "001");
			var transactionHeader2PK = helper.CreateAccTransactionHeader("AP", "INV", branchPK, companyPK, departmentPK, "002");
			var transactionHeader3PK = helper.CreateAccTransactionHeader("AP", "INV", branchPK, companyPK, departmentPK, "003");
			var transactionHeader4PK = helper.CreateAccTransactionHeader("AP", "INV", branchPK, companyPK, departmentPK, "004");

			var shipment1PK = helper.CreateJobShipment("JS001", "CTN", "1");
			var job1PK = Guid.NewGuid();
			helper.CreateJobHeader(job1PK, shipment1PK, "JS", "SHP00001", companyPK, branchPK, departmentPK);

			var shipment2PK = helper.CreateJobShipment("JS002", "CTN", "2");
			var job2PK = Guid.NewGuid();
			helper.CreateJobHeader(job2PK, shipment2PK, "JS", "SHP00002", companyPK, branchPK, departmentPK);

			var orgPk = helper.CreateOrgHeader("TEST01", "TEST ORGANISATION");
			var oppPk = helper.CreateOrgOpportunity("O00100001", orgPk, companyPK);
			var agreementPk = helper.CreateOrgCommissionAgreement(oppPk, "#1", orgPk);

			header1 = helper.CreateAccCommissionHeader(companyPK, transactionHeader1PK, transactionHeader1PK, "AH", agreementPk, orgPk, "SHP", "", "", DateTime.Now, "SEA", "AU", "NZ");
			header2 = helper.CreateAccCommissionHeader(companyPK, transactionHeader2PK, job1PK, "JH", agreementPk, orgPk, "SHP", "", "", DateTime.Now, "SEA", "AU", "NZ");
			header3 = helper.CreateAccCommissionHeader(companyPK, transactionHeader3PK, job2PK, "JH", agreementPk, orgPk, "SHP", "", "", DateTime.Now, "SEA", "AU", "NZ");
			header4 = helper.CreateAccCommissionHeader(companyPK, transactionHeader4PK, Guid.Empty, "JH", agreementPk, orgPk, "SHP", "", "", DateTime.Now, "SEA", "AU", "NZ");
		}

		protected override void AssertTransformationResults()
		{
			AssertJobNumberResult(1, header1, string.Empty);
			AssertJobNumberResult(2, header2, "SHP00001");
			AssertJobNumberResult(3, header3, "SHP00002");
			AssertJobNumberResult(4, header4, string.Empty);
		}

		void AssertJobNumberResult(int headerNumber, Guid pk, string expectedJobNumber)
		{
			var sql = $@"SELECT CH0_JobNumber FROM dbo.AccCommissionHeader
						WHERE {AccCommissionHeaderSchema.Constants.PK} = @pk";

			string jobNumber;

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				jobNumber = (string)command.ExecuteScalar();
			}

			AssertEquals($"Header {headerNumber}", expectedJobNumber, jobNumber);
		}

		Guid header1;
		Guid header2;
		Guid header3;
		Guid header4;
	}
}

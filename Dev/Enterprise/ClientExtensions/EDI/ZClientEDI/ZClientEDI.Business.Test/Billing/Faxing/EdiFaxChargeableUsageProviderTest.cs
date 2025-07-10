using System;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Billing.Test;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Fax.Test
{
	[TestedType(typeof(EdiFaxChargeableUsageProvider))]
	public class EdiFaxChargeableUsageProviderTest : ExternalChargeableUsageProviderTestCase<EdiFaxChargeableUsageProvider>
	{
		[UseSnapshotProtection]
		public override void TestBulkCopyUsages()
		{
			DbHelper.InsertFaxJobs();
			DbHelper.InsertFaxJob(10, new DateTime(2009, 9, 1, 2, 3, 4), "CCC Inc. (AU) <mail.edi@ccc.test>", 0.08m, "12345678", true, true, true, "CCE", "CCO", "PRD");
			DbHelper.InsertFaxJob(5, new DateTime(2009, 9, 2, 2, 2, 2), "CCC Inc. (AU) <mail.edi@ccc.test>", 0.07m, "22222222", true, true, true, "CCE", "CO2", "PRD");

			Db.Connection.ExecuteNonQuery(@"INSERT INTO [dbo].[EdiExternalChargeableUsage]
           ([EXU_PK]
           ,[EXU_Period]
           ,[EXU_Code]
           ,[EXU_EnterpriseCode]
           ,[EXU_CompanyCode]
           ,[EXU_ServerCode]
           ,[EXU_UnitCount])
     VALUES
           (NEWID()
           ,200909
           ,'FAX'
           ,'ABC'
           ,'EDF'
           ,'HIG'
           ,10);");

			var period = new BillingPeriod(new ZDateTime(2009, 9, 1));
			var provider = new EdiFaxChargeableUsageProviderForTest();
			provider.BulkCopyUsages(period, null);

			var rowsAsString = string.Join("\r\n", Utilities.GetDataTableFromQuery("SELECT EXU_Period, EXU_Code, EXU_EnterpriseCode, EXU_CompanyCode, EXU_ServerCode, EXU_UnitCount FROM dbo.EdiExternalChargeableUsage;")
				.Rows.OfType<DataRow>().Select(r => $"{r[0]}-{r[1]}-{r[2]}-{r[3]}-{r[4]}-{r[5]}").OrderBy(x => x));
			AssertEquals(@"200909-FAX-AAE-ACO-PRD-30
200909-FAX-BBE-BCO-PRD-12
200909-FAX-CCE-CCO-PRD-10
200909-FAX-CCE-CO2-PRD-5", rowsAsString);
		}

		[UseSnapshotProtection]
		public override void TestLoadRawUsage()
		{
			var factory = new BusinessObjectFactory(Db.Connection);
			var licHeader1 = BillingTestHelper.CreateLicence(factory, "CCE", "CCO", "PRD");
			var clientCompany2 = BillingTestHelper.CreateClientCompany(licHeader1.Database, "CO2");

			DbHelper.InsertFaxJobs();
			DbHelper.InsertFaxJob(10, new DateTime(2009, 9, 1, 2, 3, 4), "CCC Inc. (AU) <mail.edi@ccc.test>", 0.08m, "12345678", true, true, true, "CCE", "CCO", "PRD");
			DbHelper.InsertFaxJob(5, new DateTime(2009, 9, 2, 2, 2, 2), "CCC Inc. (AU) <mail.edi@ccc.test>", 0.07m, "22222222", true, true, true, "CCE", "CO2", "PRD");

			factory.Save();

			var period = new BillingPeriod(new ZDateTime(2009, 9, 1));
			var provider = new EdiFaxChargeableUsageProviderForTest();
			var rowsAsString = new StringBuilder();
			var context = new BillingLoadRawUsageContext(factory, new ZDateTime(2009, 09, 01), licHeader1.Company.Header.PK, licHeader1.ClientCompany.PK, licHeader1.Company.PK, licHeader1.Database.PK);
			provider.LoadRawUsage(context, (r) =>
			{
				rowsAsString.AppendLine($"{r["NumberOfPages"]}-{r["FaxNumber"]}-{r["ReceivedDateTime"]}-{r["CompanyCode"]}");
			});

			AssertEquals("10-12345678-1/09/2009 2:09:00 AM-CCO\r\n", rowsAsString.ToString());
		}

		FaxDbTestHelper DbHelper;

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.RunClientDbCreateScripts();
			DbHelper = new FaxDbTestHelper();
			DbHelper.RunFaxDbCreateScripts();
		}

		public override void RunBare()
		{
			base.RunBare();
			DbHelper.DropFaxDb();
		}
	}
}

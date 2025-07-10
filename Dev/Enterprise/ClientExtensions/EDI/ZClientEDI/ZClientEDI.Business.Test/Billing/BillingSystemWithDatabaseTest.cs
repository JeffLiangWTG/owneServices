using System;
using System.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class BillingSystemWithDatabaseTest : TestCaseWithFactory
	{
		public void TestLoadOdplRawUsage()
		{
			AssertLoadOdplRawUsage(organisation1.PK, organisation1.OH_Code, 16);
			AssertLoadOdplRawUsage(organisation2.PK, organisation2.OH_Code, 32);
			AssertLoadOdplRawUsage(organisation3.PK, organisation3.OH_Code, 64);
		}

		void AssertLoadOdplRawUsage(ZGuid orgPk, ZString orgCode, int amount)
		{
			var dummyBilling = new DummyBillingSystemWithDatabase();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2010, 10, 1), orgPk, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			DummyRawUsage dummyRawUsage = (DummyRawUsage)dummyBilling.LoadOdplRawUsage(context);
			AssertEquals(orgCode, dummyRawUsage.OrgCode);
			AssertEquals("Amount: " + amount, dummyRawUsage.RawLines[0]);
		}

		public void TestLoadStlRawUsage()
		{
			var db1 = organisation1.LicCompany.ActiveOrAllLicDatabases[0];
			var db2 = organisation2.LicCompany.ActiveOrAllLicDatabases[0];
			var db3 = organisation3.LicCompany.ActiveOrAllLicDatabases[0];

			AssertLoadStlRawUsage(db1.PK, db1.LD_ServerCode, 16);
			AssertLoadStlRawUsage(db2.PK, db2.LD_ServerCode, 32);
			AssertLoadStlRawUsage(db3.PK, db3.LD_ServerCode, 64);
		}

		void AssertLoadStlRawUsage(ZGuid dbPk, ZString serverCode, int amount)
		{
			var dummyBilling = new DummyBillingSystemWithDatabase();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2010, 10, 1), dbPk, ZGuid.Empty, ZGuid.Empty);
			var dummyRawUsage = (DummyStlRawUsage)dummyBilling.LoadStlRawUsage(context);
			AssertEquals(serverCode, dummyRawUsage.ServerCode);
			AssertEquals("Amount: " + amount, dummyRawUsage.RawLines[0]);
		}

		#region Implementation

		EDIOrgHeader organisation1;
		EDIOrgHeader organisation2;
		EDIOrgHeader organisation3;

		protected override void SetUp()
		{
			base.SetUp();

			organisation1 = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			organisation2 = BillingTestHelper.CreateOrganisation(Factory, "BBB");
			organisation3 = BillingTestHelper.CreateOrganisation(Factory, "CCC");
			organisation3.LicCompany.InvoiceDeliveries.AddNew().L9_OH_InvoiceTo = organisation2.PK;

			DummyBusinessObject dummyOdpl1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyOdpl1.Z0_Guid = organisation1.PK;
			dummyOdpl1.Z0_Date = new ZDateTime(2010, 10, 10);
			dummyOdpl1.Z0_Number = 16;

			DummyBusinessObject dummyOdpl2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyOdpl2.Z0_Guid = organisation2.PK;
			dummyOdpl2.Z0_Date = new ZDateTime(2010, 10, 10);
			dummyOdpl2.Z0_Number = 32;

			DummyBusinessObject dummyOdpl3 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyOdpl3.Z0_Guid = organisation3.PK;
			dummyOdpl3.Z0_Date = new ZDateTime(2010, 10, 10);
			dummyOdpl3.Z0_Number = 64;

			DummyBusinessObject dummyStl1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyStl1.Z0_Guid = organisation1.LicCompany.ActiveOrAllLicDatabases[0].PK;
			dummyStl1.Z0_Date = new ZDateTime(2010, 10, 10);
			dummyStl1.Z0_Number = 16;

			DummyBusinessObject dummyStl2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyStl2.Z0_Guid = organisation2.LicCompany.ActiveOrAllLicDatabases[0].PK;
			dummyStl2.Z0_Date = new ZDateTime(2010, 10, 10);
			dummyStl2.Z0_Number = 32;

			DummyBusinessObject dummyStl3 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyStl3.Z0_Guid = organisation3.LicCompany.ActiveOrAllLicDatabases[0].PK;
			dummyStl3.Z0_Date = new ZDateTime(2010, 10, 10);
			dummyStl3.Z0_Number = 64;

			Factory.Save();
		}

		#region DummyBillingSystemWithDatabase

		class DummyBillingSystemWithDatabase : BillingSystemWithDatabase
		{
			public DummyBillingSystemWithDatabase()
				: base()
			{
			}

			public override string SystemCode
			{
				get { return "DUM"; }
			}

			protected override SystemBill CreateSystemBill()
			{
				return new SystemBill(Context.Factory);
			}

			protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
			{
				DummyRawUsage result = new DummyRawUsage(context);
				while (reader.Read())
				{
					Guid organisationPK = (Guid)reader[DummyBusinessObject.Schema.Z0_Guid];
					int amount = (int)reader[DummyBusinessObject.Schema.Z0_Number];

					if (organisationPK == context.OrganisationPK)
					{
						result.RawLines.Add("Amount: " + amount.ToString());
					}
				}

				return result;
			}

			protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
			{
				var result = new DummyStlRawUsage(context);
				while (reader.Read())
				{
					Guid dbPk = (Guid)reader[DummyBusinessObject.Schema.Z0_Guid];
					int amount = (int)reader[DummyBusinessObject.Schema.Z0_Number];

					if (dbPk == context.DatabasePK)
					{
						result.RawLines.Add("Amount: " + amount.ToString());
					}
				}

				return result;
			}

			public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
			{
			}

			protected override SystemUsage[] CreateSystemUsages(ClientChargeableUsage[] chargeableUsages)
			{
				return Array.Empty<SystemUsage>();
			}

			public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
			{
			}

			protected override string Query_Raw_Usage
			{
				get { return query_Raw_Usage; }
			}

			const string query_Raw_Usage =
@"SELECT Z0_Guid, Z0_Number FROM dbo.DummyBizo
WHERE Z0_Date >= @DateFrom
  AND Z0_Date < @DateTo
";
		}

		#endregion

		#endregion
	}
}

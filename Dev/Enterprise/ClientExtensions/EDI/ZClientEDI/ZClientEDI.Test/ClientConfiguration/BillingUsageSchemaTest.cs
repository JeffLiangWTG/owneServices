using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Billing.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Test
{
	public class BillingUsageSchemaTest : TestCaseWithFactory
	{
		#region ClientMappingNameSyncLastRun

		public void TestClientMappingNameSyncLastRun()
		{
			EServicesBillingTestHelper.CreateTable();
			EServicesBillingTestHelper.CreateTableClientMappingInterface();

			EServicesBillingTestHelper.InsertClientMappingInterface("Interface1", new DateTime(2016, 8, 28));
			EServicesBillingTestHelper.InsertClientMappingInterface("Interface2", new DateTime(2016, 8, 29));

			var cmd = Db.Connection.Command("ClientMappingNameSync");
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.ExecuteNonQuery();

			var regItem = EDIDataRegistry.Instance.ClientMappingBillingNames;
			((IRegistryItemInternals)regItem).ClearCache();
			RegistryFactory.Instance.ClearQueryCache();
			var regValue = regItem.Value;
			AssertEquals("Interface1", regValue[0].Code);
			AssertEquals("Interface2", regValue[1].Code);
			AssertEquals("", regValue[0].Description);
			AssertEquals("", regValue[1].Description);
			AssertEquals(2, regValue.Count);

			EServicesBillingTestHelper.InsertClientMappingInterface("Interface3", new DateTime(2016, 8, 27));
			((IRegistryItemInternals)regItem).ClearCache();
			regValue = regItem.Value;
			cmd.ExecuteNonQuery();
			AssertEquals("Interface3 not added since older than last scan", 2, regValue.Count);

			var newList = new CodeDescriptionPairList(regValue);
			newList.AddPair("ManualInterface", "some text");

			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newList);

			Db.Connection.ExecuteNonQuery("delete from dbo.StmData where SD_Name = 'ClientMappingNameSyncLastRun'");
			cmd.ExecuteNonQuery();
			((IRegistryItemInternals)regItem).ClearCache();
			regValue = regItem.Value;
			AssertEquals("Interface1", regValue[0].Code);
			AssertEquals("Interface2", regValue[1].Code);
			AssertEquals("Interface3", regValue[2].Code);
			AssertEquals("ManualInterface", regValue[3].Code);
			AssertEquals("", regValue[0].Description);
			AssertEquals("", regValue[1].Description);
			AssertEquals("", regValue[2].Description);
			AssertEquals("some text", regValue[3].Description);
			AssertEquals(4, regValue.Count);
		}

		#endregion

		public void TestClientChargeableUsageUpdate()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var periodStart = new ZDateTime(2015, 5, 1);
			var usage = BillingTestHelper.CreateChargeableUsage(Factory, "000", "", periodStart, lic, 99);
			Factory.Save();

			string sql = BillingUsageSchema.GetUsageCreateScript("000", "SELECT * from NonExistentDatabase.dbo.DummyTable").CreateScript;
			Db.Connection.ExecuteNonQuery(sql);

			using (DbCommand cmd = Db.Connection.Command(BillingUsageSchema.ChargeableUsageUpdate))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@PeriodStartTimeUtc", SqlDbType.SmallDateTime, periodStart.ToDateTime());
				cmd.AddParameter("@PeriodEndTimeUtc", SqlDbType.SmallDateTime, periodStart.AddMonths(1).ToDateTime());
				cmd.AddParameter("@Code", SqlDbType.VarChar, 3, "000");
				try
				{
					cmd.ExecuteProcedureWithReturnValue();
				}
				catch (Exception)
				{
				}
			}

			usage = new BusinessObjectFactory().Load<ClientChargeableUsage>(usage.PK);
			AssertEquals(99m, usage.U1_UnitCount);
		}

		public void TestClientChargeableUsageUpdate_InvalidClientCompanyPk()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var clientCompany = lic.ClientCompany;
			var periodStart = new ZDateTime(2019, 8, 1);
			Factory.Save();

			string sql = BillingUsageSchema.GetUsageCreateScript("000",
$@"select LC_PK, LD_PK, LCC_PK, SubCode, UnitCount, Reference1, Reference2, Reference3, Reference4
	from (values
		  (null, cast({lic.LA_LD.ToSqlGuid()} as uniqueidentifier), cast({clientCompany.PK.ToSqlGuid()} as uniqueidentifier), 'Abc', 1, 'Ref1', 'Ref2', null, null)
		, (null, cast({lic.LA_LD.ToSqlGuid()} as uniqueidentifier), cast({ZGuid.NewZGuid().ToSqlGuid()} as uniqueidentifier), 'Abc', 1, 'Ref1b', 'Ref2b', null, null)
		) a(LC_PK, LD_PK, LCC_PK, SubCode, UnitCount, Reference1, Reference2, Reference3, Reference4)
"

			).CreateScript;
			Db.Connection.ExecuteNonQuery(sql);

			string dropChargeableUsageSql = @"DROP PROCEDURE EdiChargeableUsageUpdate";
			Db.Connection.ExecuteNonQuery(dropChargeableUsageSql);
			string createChargeableUsageUpdateSql = BillingUsageSchema.GetChargeableUsageUpdateScript().CreateScript;
			Db.Connection.ExecuteNonQuery(createChargeableUsageUpdateSql);

			using (DbCommand cmd = Db.Connection.Command(BillingUsageSchema.ChargeableUsageUpdate))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@FirstDayOfMonth", SqlDbType.SmallDateTime, periodStart.ToDateTime());
				cmd.AddParameter("@PeriodStartTimeUtc", SqlDbType.SmallDateTime, periodStart.ToDateTime());
				cmd.AddParameter("@PeriodEndTimeUtc", SqlDbType.SmallDateTime, periodStart.AddMonths(1).ToDateTime());
				cmd.AddParameter("@Code", SqlDbType.VarChar, 3, "000");
				cmd.ExecuteProcedureWithReturnValue();
			}

			var usages = new BusinessObjectFactory().Load<ClientChargeableUsage>(new ZQuery());
			AssertEquals(1, usages.Length);
			AssertEquals(clientCompany.PK, usages[0].U1_LCC);
			AssertEquals(lic.LA_LD, usages[0].U1_LD);
			AssertEquals("Ref1", usages[0].U1_Reference1);
			AssertEquals("ABC", usages[0].U1_SubCode);
		}

		public void TestClientChargeableUsageUpdate_SubCodeCaseInsensitive()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var clientCompany = lic.ClientCompany;
			var periodStart = new ZDateTime(2019, 8, 1);
			Factory.Save();

			string sql = BillingUsageSchema.GetUsageCreateScript("000",
$@"select LC_PK, LD_PK, LCC_PK, SubCode, UnitCount, Reference1, Reference2, Reference3, Reference4
	from (values
		  (null, cast({lic.LA_LD.ToSqlGuid()} as uniqueidentifier), cast({clientCompany.PK.ToSqlGuid()} as uniqueidentifier), 'Abc', 1, 'Ref1', 'Ref2', null, null)
		, (null, cast({lic.LA_LD.ToSqlGuid()} as uniqueidentifier), cast({clientCompany.PK.ToSqlGuid()} as uniqueidentifier), 'Abc is description here', 1, 'Ref1b', 'Ref2b', null, null)
		) a(LC_PK, LD_PK, LCC_PK, SubCode, UnitCount, Reference1, Reference2, Reference3, Reference4)
"

			).CreateScript;
			Db.Connection.ExecuteNonQuery(sql);

			string dropChargeableUsageSql = @"DROP PROCEDURE EdiChargeableUsageUpdate";
			Db.Connection.ExecuteNonQuery(dropChargeableUsageSql);
			string createChargeableUsageUpdateSql = BillingUsageSchema.GetChargeableUsageUpdateScript().CreateScript;
			Db.Connection.ExecuteNonQuery(createChargeableUsageUpdateSql);

			using (DbCommand cmd = Db.Connection.Command(BillingUsageSchema.ChargeableUsageUpdate))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@FirstDayOfMonth", SqlDbType.SmallDateTime, periodStart.ToDateTime());
				cmd.AddParameter("@PeriodStartTimeUtc", SqlDbType.SmallDateTime, periodStart.ToDateTime());
				cmd.AddParameter("@PeriodEndTimeUtc", SqlDbType.SmallDateTime, periodStart.AddMonths(1).ToDateTime());
				cmd.AddParameter("@Code", SqlDbType.VarChar, 3, "000");
				cmd.ExecuteProcedureWithReturnValue();
			}

			var usages = new BusinessObjectFactory().Load<ClientChargeableUsage>(new ZQuery());
			AssertEquals(2, usages.Length);
			AssertEquals("ABC", usages.Single(x => x.U1_SubCode == "ABC").U1_SubCode);
			AssertEquals("Abc is description here", usages.Single(x => x.U1_SubCode == "Abc is description here").U1_SubCode);
		}

		public void TestGetUsage()
		{
			EServicesBillingTestHelper.CreateTable();

			foreach (var code in new[] {
				  BillingConstants.BillingSystem.STL
				, BillingConstants.BillingSystem.AirlineMessaging
				, BillingConstants.BillingSystem.NZCustoms
				, BillingConstants.BillingSystem.JapanAFR
				, BillingConstants.BillingSystem.ClientMapping
				, BillingConstants.BillingSystem.eAdaptor
				, BillingConstants.BillingSystem.USCustoms
				, BillingConstants.BillingSystem.RailincByMessage
				, BillingConstants.BillingSystem.OceanTracing
				, BillingConstants.BillingSystem.ForwardAir
				, BillingConstants.BillingSystem.ShippingPortMessaging
				, BillingConstants.BillingSystem.GBCustoms
				, BillingConstants.BillingSystem.ImporterSecurityFiling
				, BillingConstants.BillingSystem.ASYCUDA
				, BillingConstants.BillingSystem.E2E
				})
			{
				AssertGetUsage(code);
			}
		}

		void AssertGetUsage(string code)
		{
			string sql = "select * from EdiGetChargeableUsage" + code + "(201608, '2016-07-31 14:00', '2016-08-31 14:00')";
			using (var cmd = Db.Connection.Command(sql))
			{
				var table = new DataTable();
				cmd.NewDataAdapter().Fill(table);
				AssertEquals(code, 0, table.Rows.Count);
			}
		}

		public void TestNonBilledEnterpriseCodesScript()
		{
			using (var cmd = Db.Connection.Command("select EnterpriseCode from dbo.EdiNonBilledEnterpriseCodes()"))
			{
				var table = new DataTable();
				cmd.NewDataAdapter().Fill(table);

				AssertEquals("EDI", table.Rows[0][0].ToString());
				AssertEquals("HYE", table.Rows[1][0].ToString());
				AssertEquals("EHW", table.Rows[2][0].ToString());
				AssertEquals("WTL", table.Rows[3][0].ToString());
				AssertEquals(4, table.Rows.Count);
			}

			EDIDataRegistry.Instance.NonBilledEnterpriseCodesAsStringArray = new string[] { "AAA", "ZZZ", "CCC", "DDD" };
			//RegistryFactory.Instance.Save();

			using (var cmd = Db.Connection.Command("select EnterpriseCode from dbo.EdiNonBilledEnterpriseCodes()"))
			{
				var table = new DataTable();
				cmd.NewDataAdapter().Fill(table);

				AssertEquals("AAA", table.Rows[0][0].ToString());
				AssertEquals("ZZZ", table.Rows[1][0].ToString());
				AssertEquals("CCC", table.Rows[2][0].ToString());
				AssertEquals("DDD", table.Rows[3][0].ToString());
				AssertEquals(4, table.Rows.Count);
			}
		}

		public void TestEdiGetOdmPriceHeaderForDate()
		{
			var parentLic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var childLic1 = BillingTestHelper.CreateDependentLicence(parentLic1, "BBB");
			var parentLic2 = BillingTestHelper.CreateLicence(Factory, "CCC");
			var childLic2 = BillingTestHelper.CreateDependentLicence(parentLic2, "DDD");
			var parent1Prices = BillingTestHelper.CreatePriceList(parentLic1);
			var parent2Prices = BillingTestHelper.CreatePriceList(parentLic2);
			var child1Prices = BillingTestHelper.CreatePriceList(childLic1);
			var child2Prices = BillingTestHelper.CreatePriceList(childLic2);
			childLic1.Company.InvoiceDeliveries[0].L9_UseParentPrices = true;
			childLic2.Company.InvoiceDeliveries[0].L9_UseParentPrices = false;

			Factory.Save();

			Dictionary<Guid, Guid> licToPrices = new Dictionary<Guid, Guid>();
			using (var cmd = Db.Connection.Command("select LA_PK, L6_PK from dbo.EdiGetOdmPriceHeadersForDate('2015-5-1')"))
			{
				var table = new DataTable();
				cmd.NewDataAdapter().Fill(table);
				foreach (DataRow row in table.Rows)
				{
					licToPrices.Add((Guid)row[0], (Guid)row[1]);
				}
			}

			AssertEquals("child1 using parent prices", parent1Prices.PK, licToPrices[childLic1.PK.ToGuid()]);
			AssertEquals("child2 using local prices", child2Prices.PK, licToPrices[childLic2.PK.ToGuid()]);

			childLic1.Company.InvoiceDeliveries[0].L9_UseParentPrices = false;
			childLic2.Company.InvoiceDeliveries[0].L9_UseParentPrices = true;
			Factory.Save();
			licToPrices = new Dictionary<Guid, Guid>();
			using (var cmd = Db.Connection.Command("select LA_PK, L6_PK from dbo.EdiGetOdmPriceHeadersForDate('2015-5-1')"))
			{
				var table = new DataTable();
				cmd.NewDataAdapter().Fill(table);
				foreach (DataRow row in table.Rows)
				{
					licToPrices.Add((Guid)row[0], (Guid)row[1]);
				}
			}

			AssertEquals("child1 using local prices", child1Prices.PK, licToPrices[childLic1.PK.ToGuid()]);
			AssertEquals("child2 using parent prices", parent2Prices.PK, licToPrices[childLic2.PK.ToGuid()]);
		}

		public void TestGetChargeableUsageSTL_ExcludeCodesNotOnAnyPricelist()
		{
			var stdLicence = BillingTestHelper.CreateLicence(Factory, "EDI", "EDI", "SYD", false);
			var stdPriceCompany = stdLicence.Company;

			var specialPricesPk = new ZGuid("66803A9D-C5CD-4F97-A932-9B343D744F0C");
			var specialPricelistForCodeReference = Factory.NewWithPrimaryKey<ClientLicencePriceHeader>(specialPricesPk.ToGuid());
			stdPriceCompany.PriceHeaders.Add(specialPricelistForCodeReference);
			BillingTestHelper.AddPriceItem(specialPricelistForCodeReference, "STL", "", "", 0);
			BillingTestHelper.AddPriceItem(specialPricelistForCodeReference, "USR", "", "", 0);
			BillingTestHelper.AddPriceItem(specialPricelistForCodeReference, "HRU", "", "", 0);

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "USR", "SHP");
			var zzzMap = stlPrices.UsageMaps.AddNew();
			zzzMap.PUM_PriceCategory = "STL";
			zzzMap.PUM_PriceCode = "SHP";
			zzzMap.PUM_UsageCategory = "STL";
			zzzMap.PUM_UsageCode = "ZZZ";
			var userLicence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV");

			Factory.Save();

			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdLicence.LicenceCode);

			var db = userLicence.Database;
			var clientNumber = db.DatabaseId + ".COM";
			var clientCompany = userLicence.ClientCompany;

			EServicesBillingTestHelper.CreateTable();

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCOMSRV", clientNumber, db.DatabaseId, clientCompany.PK, "", "", null, null, null, "ENT", 77));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "SHP", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCOMSRV", clientNumber, db.DatabaseId, clientCompany.PK, "", "", null, null, null, "ENT", 33));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "HRU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCOMSRV", clientNumber, db.DatabaseId, clientCompany.PK, "", "", null, null, null, "ENT", 11));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "ZZZ", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCOMSRV", clientNumber, db.DatabaseId, clientCompany.PK, "", "", null, null, null, "ENT", 5));

			EServicesBillingTestHelper.AddTransactions(infoList);

			var table = GetChargeableUsage("STL", 201808, +10, +10);
			var rowsAsText = string.Join("\r\n", table.Rows.OfType<DataRow>().Select(r => $"{r["SubCode"]} - {r["UnitCount"]}").OrderBy(x => x));
			AssertEquals(
@"SHP - 33.0000
USR - 77.0000
ZZZ - 5.0000", rowsAsText);
		}

		public void TestGetChargeableUsageSTL_Milestone()
		{
			var stdLicence = BillingTestHelper.CreateLicence(Factory, "EDI", "EDI", "SYD", false);
			var stdPriceCompany = stdLicence.Company;

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "USR", "SHP");

			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var db = lic.Database;
			var clientCompany = lic.ClientCompany;

			var clientCompanies = new[] { "AU", "GB", "GB", "CN", "HK", "MO" }
				.Select((countryCode, index) =>
				{
					var org = BillingTestHelper.CreateOrganisation(Factory, "ENT", "C0" + index.ToString(), "SRV");
					return new { ClientCompany.FindOrCreate(Factory, "C0" + index.ToString(), db.PK, org.PK, "", countryCode).PK, UnitCount = (index + 1) * 3 };
				}).ToList();

			Factory.Save();

			var clientNumber = lic.Database.DatabaseId + ".COM";

			EServicesBillingTestHelper.CreateTable();

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "STL", new ZDateTime(2016, 10, 31, 12, 59, 59), "ENTCOMSRV", clientNumber, db.DatabaseId, clientCompany.PK, "", "", null, null, null, "STL", 1));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "STL", new ZDateTime(2016, 11, 30, 12, 59, 59), "ENTCOMSRV", clientNumber, db.DatabaseId, clientCompany.PK, "", "", null, null, null, "STL", 1));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "STL", new ZDateTime(2016, 12, 15, 12, 59, 59), "ENTCOMSRV", clientNumber, db.DatabaseId, clientCompany.PK, "", "", null, null, null, "STL", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "STL", new ZDateTime(2016, 12, 16, 12, 59, 59), "ENTCOMSRV", clientNumber, db.DatabaseId, clientCompany.PK, "", "", null, null, null, "STL", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "STL", new ZDateTime(2016, 12, 17, 12, 59, 59), "ENTCOMSRV", clientNumber, db.DatabaseId, clientCompany.PK, "", "", null, null, null, "STL", 1));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "STL", new ZDateTime(2017, 1, 30, 12, 59, 59), "ENTCOMSRV", clientNumber, db.DatabaseId, clientCompany.PK, "", "", null, null, null, "STL", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "STL", new ZDateTime(2017, 1, 31, 12, 59, 59), "ENTCOMSRV", clientNumber, db.DatabaseId, clientCompany.PK, "", "", null, null, null, "STL", 1));

			clientCompanies.ForEach((x) =>
			{
				infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2017, 2, 10, 0, 0, 0), "ENTCOMSRV", clientNumber, db.DatabaseId, x.PK, "", "", null, null, null, "STL", x.UnitCount));
			});

			EServicesBillingTestHelper.AddTransactions(infoList);

			var table = GetChargeableUsage("STL", 201610, +10, +11);
			AssertEquals("No milestone calculated before Nov 2016", 0, table.Rows.Count);

			table = GetChargeableUsage("STL", 201611, +11, +11);
			AssertEquals("UnitCount 1 when last milestone only", 1m, (decimal)table.Rows[0]["UnitCount"]);

			table = GetChargeableUsage("STL", 201612, +11, +11);
			AssertEquals("UnitCount 1 + n when last milestone NOT found", 4m, (decimal)table.Rows[0]["UnitCount"]);

			table = GetChargeableUsage("STL", 201701, +11, +11);
			AssertEquals("UnitCount 1 when last milestone and other milestone", 1m, (decimal)table.Rows[0]["UnitCount"]);

			table = GetChargeableUsage("STL", 201702, +11, +11);
			var rowsAsText = string.Join("\r\n", table.Rows.OfType<DataRow>().Select(r => $"{r["SubCode"]} - {r["UnitCount"]}").OrderBy(x => x));
			AssertEquals(@"USR - 12.0000
USR - 15.0000
USR - 18.0000
USR - 3.0000
USR - 6.0000
USR - 9.0000", rowsAsText);
		}

		public void TestGetChargeableUsageSTL_WTU()
		{
			var stdLicence = BillingTestHelper.CreateLicence(Factory, "EDI", "EDI", "SYD", false);
			var stdPriceCompany = stdLicence.Company;

			var specialPricesPk = new ZGuid("66803A9D-C5CD-4F97-A932-9B343D744F0C");
			var specialPricelistForCodeReference = Factory.NewWithPrimaryKey<ClientLicencePriceHeader>(specialPricesPk.ToGuid());
			stdPriceCompany.PriceHeaders.Add(specialPricelistForCodeReference);
			BillingTestHelper.AddPriceItem(specialPricelistForCodeReference, "STL", "", "", 0);
			BillingTestHelper.AddPriceItem(specialPricelistForCodeReference, "USR", "", "", 0);
			BillingTestHelper.AddPriceItem(specialPricelistForCodeReference, "HRU", "", "", 0);

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "USR", "WTU");
			var zzzMap = stlPrices.UsageMaps.AddNew();
			zzzMap.PUM_PriceCategory = "STL";
			zzzMap.PUM_PriceCode = "WTU";
			zzzMap.PUM_UsageCategory = "STL";
			zzzMap.PUM_UsageCode = "ZZZ";
			var userLicence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV");

			Factory.Save();

			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdLicence.LicenceCode);

			var db = userLicence.Database;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "CC1", db.PK, ZGuid.Empty, "", "");
			var clientNumber1 = db.DatabaseId + ".CC1";
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "CC2", db.PK, ZGuid.Empty, "", "");
			var clientNumber2 = db.DatabaseId + ".CC2";
			Factory.Save();

			EServicesBillingTestHelper.CreateTable();

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 77));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "HRU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 11));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "ZZZ", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 5));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", "guid1", "1", null, "ENT", 1));   //f(1) = 1
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid2", "1", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", "guid2", "2", null, "ENT", 1));   //f(2) = 1.5
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid3", "3", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", "guid3", "4", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid3", "5", null, "ENT", 1));   //f(5) - f(f2) = 2.28-1.5

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid50", "50", null, "ENT", 1)); //f(50) - f(49) = 12.26 - 12.02
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid51", "51", null, "ENT", 1)); //f(51) - f(50) = 12.26 + 0.245 - 12.26 
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid88", "88", null, "ENT", 1)); //f(88) - f(87) = (12.26 + (88-50) * 0.245) - (12.26 + (87-50) * 0.245)

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid90-94", "90", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid90-94", "91", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid90-94", "92", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid90-94", "93", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid90-94", "94", null, "ENT", 1)); //f(94) - f(89) = (12.26 + (94-50) * 0.245) - (12.26 + (89-50) * 0.245)

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid45-55", "45", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid45-55", "46", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid45-55", "47", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid45-55", "48", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid45-55", "49", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid45-55", "50", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid45-55", "51", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid45-55", "52", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid45-55", "53", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid45-55", "54", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid45-55", "55", null, "ENT", 1)); //f(55) - f(44) = (12.26 + (55-50) * 0.245) - 10.79

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid50-51", "50", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid50-51", "51", null, "ENT", 1)); //f(51) - f(49) = 12.26 + 0.245 - 12.02

			EServicesBillingTestHelper.AddTransactions(infoList);

			//default registry value.
			AssertEquals(0, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'BILLINGUNITCOUNTADJUSTMENTS'"));
			var table = GetChargeableUsage("STL", 201808, +10, +10);
			var rowsAsText = string.Join("\r\n", table.Rows.OfType<DataRow>().Select(r => $"{r["SubCode"]} - {r["UnitCount"]} - {r["LCC_PK"]}").OrderBy(x => x));
			AssertEquals(
$@"USR - 77.0000 - {clientCompany1.PK}
WTU - 8.4150 - 
ZZZ - 5.0000 - {clientCompany1.PK}", rowsAsText);
			AssertWTU(infoList, 8.4150m);

			//user settings.
			var registryValue = new BillingUnitCountAdjustmentCollection();
			var adjustment0 = registryValue.AddNew();
			adjustment0.PriceCode = "W00";
			adjustment0.DefaultAdjustedIncrement = 0.2681;
			adjustment0.AdjustmentSettings.AddNew(1, 0.38);
			adjustment0.AdjustmentSettings.AddNew(2, 1.67);

			var adjustment1 = registryValue.AddNew();
			adjustment1.PriceCode = "WTU";
			var setting1 = adjustment1.AdjustmentSettings.AddNew();
			setting1.OriginalUnitCount = 1;
			setting1.AdjustedUnitCount = 1;
			var setting2 = adjustment1.AdjustmentSettings.AddNew();
			setting2.OriginalUnitCount = 2;
			setting2.AdjustedUnitCount = 1.5;
			var setting3 = adjustment1.AdjustmentSettings.AddNew();
			setting3.OriginalUnitCount = 3;
			setting3.AdjustedUnitCount = 2.1;
			var setting4 = adjustment1.AdjustmentSettings.AddNew();
			setting4.OriginalUnitCount = 4;
			setting4.AdjustedUnitCount = 3.1;

			var adjustment2 = registryValue.AddNew();
			adjustment2.PriceCode = "W09";
			adjustment2.AdjustmentSettings.AddNew(1, 0.62);
			adjustment2.AdjustmentSettings.AddNew(2, 1.47);

			EDIDataRegistry.Instance.BillingUnitCountAdjustments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			AssertEquals(1, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'BILLINGUNITCOUNTADJUSTMENTS'"));

			table = GetChargeableUsage("STL", 201808, +10, +10);
			rowsAsText = string.Join("\r\n", table.Rows.OfType<DataRow>().Select(r => $"{r["SubCode"]} - {r["UnitCount"]} - {r["LCC_PK"]}").OrderBy(x => x));
			AssertEquals(
$@"USR - 77.0000 - {clientCompany1.PK}
WTU - 4.1000 - 
ZZZ - 5.0000 - {clientCompany1.PK}", rowsAsText);
			AssertWTU(infoList, 4.1m);

			//empty user settings
			EDIDataRegistry.Instance.BillingUnitCountAdjustments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new BillingUnitCountAdjustmentCollection());
			AssertEquals(1, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'BILLINGUNITCOUNTADJUSTMENTS'"));

			table = GetChargeableUsage("STL", 201808, +10, +10);
			rowsAsText = string.Join("\r\n", table.Rows.OfType<DataRow>().Select(r => $"{r["SubCode"]} - {r["UnitCount"]} - {r["LCC_PK"]}").OrderBy(x => x));
			AssertEquals(
string.Join("\r\n", $@"USR - 77.0000 - {clientCompany1.PK}
WTU - 24.0000 - {clientCompany2.PK}
WTU - 3.0000 - {clientCompany1.PK}
ZZZ - 5.0000 - {clientCompany1.PK}".Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).OrderBy(x => x)), rowsAsText);

			//user settings without WTU.
			registryValue = new BillingUnitCountAdjustmentCollection();
			var adjustment3 = registryValue.AddNew();
			adjustment3.PriceCode = "ABC";
			adjustment3.AdjustmentSettings.AddNew(1, 0.76);
			EDIDataRegistry.Instance.BillingUnitCountAdjustments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			AssertEquals(1, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'BILLINGUNITCOUNTADJUSTMENTS'"));

			table = GetChargeableUsage("STL", 201808, +10, +10);
			rowsAsText = string.Join("\r\n", table.Rows.OfType<DataRow>().Select(r => $"{r["SubCode"]} - {r["UnitCount"]} - {r["LCC_PK"]}").OrderBy(x => x));
			AssertEquals(
string.Join("\r\n", $@"USR - 77.0000 - {clientCompany1.PK}
WTU - 24.0000 - {clientCompany2.PK}
WTU - 3.0000 - {clientCompany1.PK}
ZZZ - 5.0000 - {clientCompany1.PK}".Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).OrderBy(x => x)), rowsAsText);
		}

		public void TestGetChargeableUsageSTL_HRD()
		{
			var stdLicence = BillingTestHelper.CreateLicence(Factory, "EDI", "EDI", "SYD", false);
			var stdPriceCompany = stdLicence.Company;

			var specialPricesPk = new ZGuid("66803A9D-C5CD-4F97-A932-9B343D744F0C");
			var specialPricelistForCodeReference = Factory.NewWithPrimaryKey<ClientLicencePriceHeader>(specialPricesPk.ToGuid());
			stdPriceCompany.PriceHeaders.Add(specialPricelistForCodeReference);
			BillingTestHelper.AddPriceItem(specialPricelistForCodeReference, "STL", "", "", 0);
			BillingTestHelper.AddPriceItem(specialPricelistForCodeReference, "USR", "", "", 0);
			BillingTestHelper.AddPriceItem(specialPricelistForCodeReference, "HRU", "", "", 0);

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "USR", "#HR", "#HS");
			var map1 = stlPrices.UsageMaps.AddNew();
			map1.PUM_PriceCategory = "STL";
			map1.PUM_PriceCode = "#HR";
			map1.PUM_UsageCategory = "STL";
			map1.PUM_UsageCode = "PRT";
			var map2 = stlPrices.UsageMaps.AddNew();
			map2.PUM_PriceCategory = "STL";
			map2.PUM_PriceCode = "#HS";
			map2.PUM_UsageCategory = "STL";
			map2.PUM_UsageCode = "PRS";
			var userLicence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV");

			Factory.Save();

			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdLicence.LicenceCode);

			var db = userLicence.Database;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "CC1", db.PK, ZGuid.Empty, "", "");
			var clientNumber1 = db.DatabaseId + ".CC1";
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "CC2", db.PK, ZGuid.Empty, "", "");
			var clientNumber2 = db.DatabaseId + ".CC2";
			Factory.Save();

			EServicesBillingTestHelper.CreateTable();

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 77));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "PRS", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", "guid1", "1", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "PRS", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid2", "1", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "PRS", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", "guid2", "2", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "PRT", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid3", "3", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "PRT", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", "guid3", "4", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "PRT", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", "guid3", "5", null, "ENT", 1));

			EServicesBillingTestHelper.AddTransactions(infoList);

			void AssertUsages(string usagesAsString)
			{
				var table = GetChargeableUsage("STL", 201808, +10, +10);
				var rowsAsText = string.Join("\r\n", table.Rows.OfType<DataRow>().Select(r => $"{r["SubCode"]} - {r["UnitCount"]} - {r["LCC_PK"]}").OrderBy(x => x));
				AssertEquals(usagesAsString, rowsAsText);
			}

			var fullUsages = $@"PRS - 1.0000 - {clientCompany2.PK}
PRS - 2.0000 - {clientCompany1.PK}
PRT - 1.0000 - {clientCompany1.PK}
PRT - 2.0000 - {clientCompany2.PK}
USR - 77.0000 - {clientCompany1.PK}";
			var usagesWithoutHRD = $"USR - 77.0000 - {clientCompany1.PK}";

			AssertUsages(fullUsages);

			db.LD_IsActive = false;
			Factory.Save();
			AssertUsages(usagesWithoutHRD);

			db.LD_IsActive = true;
			Factory.Save();
			AssertUsages(fullUsages);

			userLicence.LA_IsActive = false;
			Factory.Save();
			AssertUsages(usagesWithoutHRD);

			userLicence.LA_IsActive = true;
			userLicence.LA_AgreedLiveDate = new ZDate(2020, 1, 1);
			Factory.Save();
			AssertUsages(usagesWithoutHRD);

			userLicence.LA_AgreedLiveDate = new ZDate(2018, 7, 1);
			Factory.Save();
			AssertUsages(fullUsages);
		}

		void AssertWTU(IEnumerable<EServicesBillingTestHelper.RawUsageInfo> rawUsages, decimal wtuExpected)
		{
			var wtuAdj = EDIDataRegistry.Instance.BillingUnitCountAdjustments.Value.OfType<BillingUnitCountAdjustment>().Single(x => x.PriceCode == "WTU");

			decimal fx(int txTotal, int txCount)
			{
				var settings = wtuAdj.AdjustmentSettings.OfType<BillingUnitCountAdjustmentSetting>();
				var maxSetting = settings.OrderByDescending(x => x.OriginalUnitCount).First();

				var totalUsage = settings.FirstOrDefault(x => x.OriginalUnitCount == txTotal)?.AdjustedUnitCount
									?? maxSetting.AdjustedUnitCount + (txTotal - maxSetting.OriginalUnitCount) * wtuAdj.DefaultAdjustedIncrement;

				var billedTx = txTotal - txCount;
				var billedUsage = billedTx == 0 ? 0 :
					settings.FirstOrDefault(x => x.OriginalUnitCount == billedTx)?.AdjustedUnitCount
					?? maxSetting.AdjustedUnitCount + (billedTx - maxSetting.OriginalUnitCount) * wtuAdj.DefaultAdjustedIncrement;

				return totalUsage - billedUsage;
			}

			var actualWtu = rawUsages.Where(x => x.PriceItemCode == "WTU").GroupBy(x => x.Reference3)
				.Select(x => new { Total = x.Max(y => int.Parse(y.Reference4)), Count = x.Count() })
				.Sum(x => fx(x.Total, x.Count));
			AssertEquals(wtuExpected, actualWtu);
		}

		public void TestGetChargeableUsageSTL_UCN()
		{
			var stdLicence = BillingTestHelper.CreateLicence(Factory, "EDI", "EDI", "SYD", false);
			var stdPriceCompany = stdLicence.Company;

			var specialPricesPk = new ZGuid("66803A9D-C5CD-4F97-A932-9B343D744F0C");
			var specialPricelistForCodeReference = Factory.NewWithPrimaryKey<ClientLicencePriceHeader>(specialPricesPk.ToGuid());
			stdPriceCompany.PriceHeaders.Add(specialPricelistForCodeReference);
			BillingTestHelper.AddPriceItem(specialPricelistForCodeReference, "STL", "", "", 0);
			BillingTestHelper.AddPriceItem(specialPricelistForCodeReference, "USR", "", "", 0);

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "USR");
			var userLicence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV");

			Factory.Save();

			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdLicence.LicenceCode);

			var db = userLicence.Database;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "CC1", db.PK, ZGuid.Empty, "", "");
			var clientNumber1 = db.DatabaseId + ".CC1";
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "CC2", db.PK, ZGuid.Empty, "", "");
			var clientNumber2 = db.DatabaseId + ".CC2";
			db.LD_HostedLocation = "SYD";
			Factory.Save();

			EServicesBillingTestHelper.CreateTable();

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 77));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", null, null, null, "ENT", 80));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 5, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 66));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 5, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", null, null, null, "ENT", 25));

			EServicesBillingTestHelper.AddTransactions(infoList);

			var table = GetChargeableUsage("STL", 201808, +10, +10);
			var rowsAsText = string.Join("\r\n", table.Rows.OfType<DataRow>().Select(r => $"{r["SubCode"]} - {r["UnitCount"]} - {r["LCC_PK"]}").OrderBy(x => x));
			AssertEquals(
$@"USR - 77.0000 - {clientCompany1.PK}
USR - 80.0000 - {clientCompany2.PK}
USW - 77.0000 - {clientCompany1.PK}
USW - 80.0000 - {clientCompany2.PK}", rowsAsText);

			table = GetChargeableUsage("STL", 202205, +10, +10);
			rowsAsText = string.Join("\r\n", table.Rows.OfType<DataRow>().Select(r => $"{r["SubCode"]} - {r["UnitCount"]} - {r["LCC_PK"]}").OrderBy(x => x));
			AssertEquals(
$@"UCS - 25.0000 - {clientCompany2.PK}
UCS - 66.0000 - {clientCompany1.PK}
USR - 25.0000 - {clientCompany2.PK}
USR - 66.0000 - {clientCompany1.PK}
USW - 25.0000 - {clientCompany2.PK}
USW - 66.0000 - {clientCompany1.PK}", rowsAsText);

			db.LD_HostedLocation = "CN1";
			Factory.Save();

			table = GetChargeableUsage("STL", 201808, +10, +10);
			rowsAsText = string.Join("\r\n", table.Rows.OfType<DataRow>().Select(r => $"{r["SubCode"]} - {r["UnitCount"]} - {r["LCC_PK"]}").OrderBy(x => x));
			AssertEquals(
$@"USR - 77.0000 - {clientCompany1.PK}
USR - 80.0000 - {clientCompany2.PK}", rowsAsText);

			table = GetChargeableUsage("STL", 202205, +10, +10);
			rowsAsText = string.Join("\r\n", table.Rows.OfType<DataRow>().Select(r => $"{r["SubCode"]} - {r["UnitCount"]} - {r["LCC_PK"]}").OrderBy(x => x));
			AssertEquals(
$@"UCN - 25.0000 - {clientCompany2.PK}
UCN - 66.0000 - {clientCompany1.PK}
USR - 25.0000 - {clientCompany2.PK}
USR - 66.0000 - {clientCompany1.PK}", rowsAsText);
		}

		public void TestGetChargeableUsageSTL_UCS()
		{
			var stdLicence = BillingTestHelper.CreateLicence(Factory, "EDI", "EDI", "SYD", false);
			var stdPriceCompany = stdLicence.Company;

			var specialPricesPk = new ZGuid("66803A9D-C5CD-4F97-A932-9B343D744F0C");
			var specialPricelistForCodeReference = Factory.NewWithPrimaryKey<ClientLicencePriceHeader>(specialPricesPk.ToGuid());
			stdPriceCompany.PriceHeaders.Add(specialPricelistForCodeReference);
			BillingTestHelper.AddPriceItem(specialPricelistForCodeReference, "STL", "", "", 0);
			BillingTestHelper.AddPriceItem(specialPricelistForCodeReference, "USR", "", "", 0);

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "USR", "UCS", "RBU");
			var userLicence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV");

			Factory.Save();

			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdLicence.LicenceCode);

			var db = userLicence.Database;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "CC1", db.PK, ZGuid.Empty, "", "");
			var clientNumber1 = db.DatabaseId + ".CC1";
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "CC2", db.PK, ZGuid.Empty, "", "");
			var clientNumber2 = db.DatabaseId + ".CC2";
			db.LD_HostedLocation = "SYD";
			Factory.Save();

			EServicesBillingTestHelper.CreateTable();

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 77));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", null, null, null, "ENT", 80));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 10));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", null, null, null, "ENT", 20));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 5, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 66));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 5, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", null, null, null, "ENT", 25));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2022, 5, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 30));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2022, 5, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", null, null, null, "ENT", 40));
			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var table = GetChargeableUsage("STL", 201808, +10, +10);
			var rowsAsText = string.Join("\r\n", table.Rows.OfType<DataRow>().Select(r => $"{r["SubCode"]} - {r["UnitCount"]} - {r["LCC_PK"]}").OrderBy(x => x));

			AssertEquals(
$@"RBU - 10.0000 - {clientCompany1.PK}
RBU - 20.0000 - {clientCompany2.PK}
USR - 77.0000 - {clientCompany1.PK}
USR - 80.0000 - {clientCompany2.PK}
USW - 100.0000 - {clientCompany2.PK}
USW - 87.0000 - {clientCompany1.PK}", rowsAsText);

			table = GetChargeableUsage("STL", 202205, +10, +10);
			rowsAsText = string.Join("\r\n", table.Rows.OfType<DataRow>().Select(r => $"{r["SubCode"]} - {r["UnitCount"]} - {r["LCC_PK"]}").OrderBy(x => x));
			AssertEquals(
$@"RBU - 30.0000 - {clientCompany1.PK}
RBU - 40.0000 - {clientCompany2.PK}
UCS - 65.0000 - {clientCompany2.PK}
UCS - 96.0000 - {clientCompany1.PK}
USR - 25.0000 - {clientCompany2.PK}
USR - 66.0000 - {clientCompany1.PK}
USW - 65.0000 - {clientCompany2.PK}
USW - 96.0000 - {clientCompany1.PK}", rowsAsText);
		}

		public void TestGetChargeableUsageSTL_BYO()
		{
			void assertUsage(string usagesAsText)
			{
				var table = GetChargeableUsage("STL", 202204, +10, +10);
				var rowsAsText = string.Join("\r\n", table.Rows.OfType<DataRow>().Select(r => $"LC:{r["LC_CompanyCode"]} - LD:{r["LD_ServerCode"]} - LCC:{r["LCC_Code"]} - {r["SubCode"]} - {r["UnitCount"]}").OrderBy(x => x));
				AssertEquals(usagesAsText, rowsAsText);
			}

			var stdLicence = BillingTestHelper.CreateLicence(Factory, "EDI", "EDI", "SYD", false);
			var stdPriceCompany = stdLicence.Company;

			var specialPricesPk = new ZGuid("66803A9D-C5CD-4F97-A932-9B343D744F0C");
			var specialPricelistForCodeReference = Factory.NewWithPrimaryKey<ClientLicencePriceHeader>(specialPricesPk.ToGuid());
			stdPriceCompany.PriceHeaders.Add(specialPricelistForCodeReference);
			BillingTestHelper.AddPriceItem(specialPricelistForCodeReference, "STL", "", "", 0);
			BillingTestHelper.AddPriceItem(specialPricelistForCodeReference, "USR", "", "", 0);

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "USR");
			BillingTestHelper.AddPriceItem(stlPrices, "STL", "", "", 0);
			var priceBYO = BillingTestHelper.AddPriceItem(stlPrices, "BYO", "", "", 100);
			priceBYO.L7_Category = "SVC";

			var mapBYO = stlPrices.UsageMaps.AddNew();
			mapBYO.PUM_PriceCategory = "SVC";
			mapBYO.PUM_PriceCode = "BYO";
			mapBYO.PUM_UsageCategory = "STL";
			mapBYO.PUM_UsageCode = "BYO";

			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdLicence.LicenceCode);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "LE1", "LC1", "LD1");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "LE2", "LC2", "LD2");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "LE3", "LC3", "LD3");
			var lic4 = BillingTestHelper.CreateLicence(Factory, "LE4", "LC4", "LD4");

			var db1 = lic1.Database;
			var db2 = lic2.Database;
			var db3 = lic3.Database;
			var db4 = lic4.Database;

			Factory.Save();

			var lc1CC1 = ClientCompany.FindOrCreate(Factory, "CC1", db1.PK, ZGuid.Empty, "", "");
			var lc1CC1Num = db1.DatabaseId + ".CC1";
			var lc1CC2 = ClientCompany.FindOrCreate(Factory, "CC2", db1.PK, ZGuid.Empty, "", "");
			var lc1CC2Num = db1.DatabaseId + ".CC2";

			var lc2CC1 = ClientCompany.FindOrCreate(Factory, "CC1", db2.PK, ZGuid.Empty, "", "");
			var lc2CC1Num = db2.DatabaseId + ".CC1";
			var lc2CC2 = ClientCompany.FindOrCreate(Factory, "CC2", db2.PK, ZGuid.Empty, "", "");
			var lc2CC2Num = db2.DatabaseId + ".CC2";

			var lc3CC1 = ClientCompany.FindOrCreate(Factory, "CC1", db3.PK, ZGuid.Empty, "", "");
			var lc3CC1Num = db3.DatabaseId + ".CC1";
			var lc3CC2 = ClientCompany.FindOrCreate(Factory, "CC2", db3.PK, ZGuid.Empty, "", "");
			var lc3CC2Num = db3.DatabaseId + ".CC2";

			var lc4CC1 = ClientCompany.FindOrCreate(Factory, "CC1", db4.PK, ZGuid.Empty, "", "");
			var lc4CC1Num = db4.DatabaseId + ".CC1";

			Factory.Save();

			EServicesBillingTestHelper.CreateTable();
			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 4, 1, 0, 0, 0), "EN1CC1LD1", lc1CC1Num, db1.DatabaseId, lc1CC1.PK, "", "", null, null, null, "ENT", 10));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 4, 1, 0, 0, 0), "EN2CC2LD2", lc2CC2Num, db2.DatabaseId, lc2CC2.PK, "", "", null, null, null, "ENT", 20));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 4, 1, 0, 0, 0), "EN3CC1LD3", lc3CC1Num, db3.DatabaseId, lc3CC1.PK, "", "", null, null, null, "ENT", 30));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 1, 0, 0, 0), "EN2CC1LD2", lc2CC1Num, db2.DatabaseId, lc2CC1.PK, "dev#ABC", "", null, null, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 1, 0, 0, 0), "EN2CC1LD2", lc2CC2Num, db2.DatabaseId, lc2CC2.PK, "dev#DEF", "", null, null, null, "ENT", 1));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 1, 0, 0, 0), "EN3CC1LD3", lc3CC1Num, db3.DatabaseId, lc3CC1.PK, "dev#001", "", null, null, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 1, 0, 0, 0), "EN3CC1LD3", lc3CC1Num, db3.DatabaseId, lc3CC1.PK, "dev#002", "", null, null, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 1, 0, 0, 0), "EN3CC1LD3", lc3CC1Num, db3.DatabaseId, lc3CC1.PK, "dev#002", "", null, null, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 1, 0, 0, 0), "EN3CC1LD3", lc3CC1Num, db3.DatabaseId, lc3CC1.PK, "dev#003", "", null, null, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 1, 0, 0, 0), "EN3CC1LD3", lc3CC1Num, db3.DatabaseId, lc3CC1.PK, "dev#003", "", null, null, null, "ENT", 1));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 1, 0, 0, 0), "EN3CC2LD3", lc3CC2Num, db3.DatabaseId, lc3CC2.PK, "dev#004", "", null, null, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 1, 0, 0, 0), "EN3CC2LD3", lc3CC2Num, db3.DatabaseId, lc3CC2.PK, "dev#004", "", null, null, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 1, 0, 0, 0), "EN3CC2LD3", lc3CC2Num, db3.DatabaseId, lc3CC2.PK, "dev#005", "", null, null, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 1, 0, 0, 0), "EN3CC2LD3", lc3CC2Num, db3.DatabaseId, lc3CC2.PK, "dev#005", "", null, null, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 1, 0, 0, 0), "EN3CC2LD3", lc3CC2Num, db3.DatabaseId, lc3CC2.PK, "dev#006", "", null, null, null, "ENT", 1));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 1, 0, 0, 0), "EN4CC1LD4", lc4CC1Num, db4.DatabaseId, lc4CC1.PK, "#id0001", "", null, null, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 1, 0, 0, 0), "EN4CC1LD4", lc4CC1Num, db4.DatabaseId, lc4CC1.PK, "#id0002", "", null, null, null, "ENT", 1));

			EServicesBillingTestHelper.AddTransactions(infoList);

			//default registry value, no premiums yet.
			AssertEquals(0, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'HandheldDevicePremiumTypes'"));
			assertUsage(@"LC: - LD:LD1 - LCC:CC1 - USR - 10.0000
LC: - LD:LD2 - LCC: - BYO - 2.0000
LC: - LD:LD2 - LCC:CC2 - USR - 20.0000
LC: - LD:LD3 - LCC: - BYO - 10.0000
LC: - LD:LD3 - LCC:CC1 - USR - 30.0000
LC: - LD:LD4 - LCC: - BYO - 2.0000");

			//default registry value, with premiums.
			var prem1 = db1.PremiumServices.AddNew();
			prem1.CPS_Type = "PLC";
			prem1.CPS_Units = 100;
			prem1.CPS_StartDate = new ZDate(2000, 1, 1);

			var prem2 = db2.PremiumServices.AddNew();
			prem2.CPS_Type = "PLC";
			prem2.CPS_Units = 10;
			prem2.CPS_StartDate = new ZDate(2000, 1, 1);

			var prem3_1 = db3.PremiumServices.AddNew();
			prem3_1.CPS_Type = "PLC";
			prem3_1.CPS_Units = 1;
			prem3_1.CPS_StartDate = new ZDate(2000, 1, 1);

			var prem3_2 = db3.PremiumServices.AddNew();
			prem3_2.CPS_Type = "PLS";
			prem3_2.CPS_Units = 2;
			prem3_2.CPS_StartDate = new ZDate(2000, 1, 1);

			var prem3_4 = db3.PremiumServices.AddNew();
			prem3_4.CPS_Type = "PS1";
			prem3_4.CPS_Units = 3;
			prem3_4.CPS_StartDate = new ZDate(2000, 1, 1);
			prem3_4.CPS_EndDate = new ZDate(2001, 1, 1);

			Factory.Save();
			assertUsage(@"LC: - LD:LD1 - LCC:CC1 - USR - 10.0000
LC: - LD:LD2 - LCC:CC2 - USR - 20.0000
LC: - LD:LD3 - LCC: - BYO - 7.0000
LC: - LD:LD3 - LCC:CC1 - USR - 30.0000
LC: - LD:LD4 - LCC: - BYO - 2.0000");

			//user settings, with premiums.
			var registryValue = new CodeDescriptionPairList();
			registryValue.AddPairIfNotExist("PLC", "");
			EDIDataRegistry.Instance.HandheldDevicePremiumTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			AssertEquals(1, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'HandheldDevicePremiumTypes'"));
			assertUsage(@"LC: - LD:LD1 - LCC:CC1 - USR - 10.0000
LC: - LD:LD2 - LCC:CC2 - USR - 20.0000
LC: - LD:LD3 - LCC: - BYO - 9.0000
LC: - LD:LD3 - LCC:CC1 - USR - 30.0000
LC: - LD:LD4 - LCC: - BYO - 2.0000");

			//empty settings, with premiums.
			registryValue.Clear();
			EDIDataRegistry.Instance.HandheldDevicePremiumTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			AssertEquals(1, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'HandheldDevicePremiumTypes'"));
			assertUsage(@"LC: - LD:LD1 - LCC:CC1 - USR - 10.0000
LC: - LD:LD2 - LCC: - BYO - 2.0000
LC: - LD:LD2 - LCC:CC2 - USR - 20.0000
LC: - LD:LD3 - LCC: - BYO - 10.0000
LC: - LD:LD3 - LCC:CC1 - USR - 30.0000
LC: - LD:LD4 - LCC: - BYO - 2.0000");

			//empty settings, with BYR
			registryValue.Clear();
			EDIDataRegistry.Instance.HandheldDevicePremiumTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			AssertEquals(1, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'HandheldDevicePremiumTypes'"));
			var premBYR = db3.PremiumServices.AddNew();
			premBYR.CPS_Type = "BYR";
			premBYR.CPS_Units = 2;
			premBYR.CPS_StartDate = new ZDate(2000, 1, 1);
			Factory.Save();

			assertUsage(@"LC: - LD:LD1 - LCC:CC1 - USR - 10.0000
LC: - LD:LD2 - LCC: - BYO - 2.0000
LC: - LD:LD2 - LCC:CC2 - USR - 20.0000
LC: - LD:LD3 - LCC: - BYO - 8.0000
LC: - LD:LD3 - LCC:CC1 - USR - 30.0000
LC: - LD:LD4 - LCC: - BYO - 2.0000");
		}

		public void TestGetChargeableUsageSTL_USR_RBU()
		{
			var stdLicence = BillingTestHelper.CreateLicence(Factory, "EDI", "EDI", "SYD", false);
			var stdPriceCompany = stdLicence.Company;

			var specialPricesPk = new ZGuid("66803A9D-C5CD-4F97-A932-9B343D744F0C");
			var specialPricelistForCodeReference = Factory.NewWithPrimaryKey<ClientLicencePriceHeader>(specialPricesPk.ToGuid());
			stdPriceCompany.PriceHeaders.Add(specialPricelistForCodeReference);
			BillingTestHelper.AddPriceItem(specialPricelistForCodeReference, "STL", "", "", 0);
			BillingTestHelper.AddPriceItem(specialPricelistForCodeReference, "USR", "", "", 0);

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "USR", "UCS", "RBU");
			var userLicence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV");

			Factory.Save();

			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdLicence.LicenceCode);

			var db = userLicence.Database;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "CC1", db.PK, ZGuid.Empty, "", "");
			var clientNumber1 = db.DatabaseId + ".CC1";
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "CC2", db.PK, ZGuid.Empty, "", "");
			var clientNumber2 = db.DatabaseId + ".CC2";
			db.LD_HostedLocation = "SYD";
			Factory.Save();

			EServicesBillingTestHelper.CreateTable();

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 77));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", null, null, null, "ENT", 80));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 10));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", null, null, null, "ENT", 20));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 5, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 66));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 5, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", null, null, null, "ENT", 25));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2022, 5, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 30));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2022, 5, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", null, null, null, "ENT", 40));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 1) { StaffCode = "TXZ" });
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2018, 8, 1, 0, 0, 0), "ENTCC1SRV", clientNumber1, db.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 1) { StaffCode = "TXZ" });

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 5, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", null, null, null, "ENT", 1) { StaffCode = "TXZ" });
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2022, 5, 1, 0, 0, 0), "ENTCC2SRV", clientNumber2, db.DatabaseId, clientCompany2.PK, "", "", null, null, null, "ENT", 1) { StaffCode = "TXZ" });

			EServicesBillingTestHelper.AddTransactions(infoList);

			var table = GetChargeableUsage("STL", 201808, +10, +10);
			var rowsAsText = string.Join("\r\n", table.Rows.OfType<DataRow>().Select(r => $"{r["SubCode"]} - {r["UnitCount"]} - {r["LCC_PK"]}").OrderBy(x => x));

			AssertEquals(
$@"RBU - 11.0000 - {clientCompany1.PK}
RBU - 20.0000 - {clientCompany2.PK}
USR - 77.0000 - {clientCompany1.PK}
USR - 80.0000 - {clientCompany2.PK}
USW - 100.0000 - {clientCompany2.PK}
USW - 88.0000 - {clientCompany1.PK}", rowsAsText);

			table = GetChargeableUsage("STL", 202205, +10, +10);
			rowsAsText = string.Join("\r\n", table.Rows.OfType<DataRow>().Select(r => $"{r["SubCode"]} - {r["UnitCount"]} - {r["LCC_PK"]}").OrderBy(x => x));
			AssertEquals(
$@"RBU - 30.0000 - {clientCompany1.PK}
RBU - 41.0000 - {clientCompany2.PK}
UCS - 66.0000 - {clientCompany2.PK}
UCS - 96.0000 - {clientCompany1.PK}
USR - 25.0000 - {clientCompany2.PK}
USR - 66.0000 - {clientCompany1.PK}
USW - 66.0000 - {clientCompany2.PK}
USW - 96.0000 - {clientCompany1.PK}", rowsAsText);
		}

		DataTable GetChargeableUsage(string code, int period, int utcToLocalTimeHour1, int utcToLocalTimeHour2)
		{
			var periodStartUtc = new DateTime(period / 100, period % 100, 1).AddHours(-utcToLocalTimeHour1);
			var periodEndUtc = periodStartUtc.AddMonths(1).AddHours(utcToLocalTimeHour1 - utcToLocalTimeHour2);
			var sql = @"select U1.*, LC.LC_CompanyCode, LD.LD_DatabaseNumber, LD.LD_ServerCode, LCC.LCC_Code from EdiGetChargeableUsage" + code + "(" + period + ", @startUtc, @endUtc) U1 " +
				@"
					LEFT JOIN dbo.LicenceCompany LC ON U1.LC_PK = LC.LC_PK
					LEFT JOIN dbo.LicenceDatabase LD ON U1.LD_PK = LD.LD_PK
					LEFT JOIN dbo.ClientCompany LCC ON U1.LCC_PK = LCC.LCC_PK;";
			var table = new DataTable();
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@startUtc", SqlDbType.DateTime, periodStartUtc);
				cmd.AddParameter("@endUtc", SqlDbType.DateTime, periodEndUtc);
				cmd.NewDataAdapter().Fill(table);
			}
			return table;
		}

		#region GetOdmUsages

		public void TestGetOdmUsages()
		{
			EServicesBillingTestHelper.CreateTable();

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA", false);
			var db = licence.Database;
			db.LD_DatabaseNumber = 1983;

			var clientCompany1 = db.ClientCompanies.AddNew();
			clientCompany1.LCC_LD = db.PK;
			clientCompany1.LCC_Code = "AAA";
			var clientCompany2 = db.ClientCompanies.AddNew();
			clientCompany2.LCC_LD = db.PK;
			clientCompany2.LCC_Code = "BBB";

			var clientStaff1 = Factory.New<ClientStaff>();
			clientStaff1.LS_Code = "AAA";
			clientStaff1.LS_FullName = "AAA Name";
			clientStaff1.LS_LD = db.PK;

			var clientStaff2 = Factory.New<ClientStaff>();
			clientStaff2.LS_Code = "BBB";
			clientStaff2.LS_FullName = "BBB Name";
			clientStaff2.LS_LD = db.PK;

			var clientStaff3 = Factory.New<ClientStaff>();
			clientStaff3.LS_Code = "TUR";
			clientStaff3.LS_FullName = "Test User";
			clientStaff3.LS_LD = db.PK;

			var clientStaff4 = Factory.New<ClientStaff>();
			clientStaff4.LS_Code = "DUR";
			clientStaff4.LS_FullName = "Demo User";
			clientStaff4.LS_LD = db.PK;

			var clientStaff5 = Factory.New<ClientStaff>();
			clientStaff5.LS_Code = "CCC";
			clientStaff5.LS_FullName = "CCC Name";
			clientStaff5.LS_LD = db.PK;

			//Active users: Company 1 (AAA, BBB, CCC) Company 2 (TUR, DUR)
			//Login users: Company 1 (AAA, TUR) Company 2 (TUR, DUR, CCC) No login (BBB)
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2016, 10, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2016, 11, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2016, 12, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2017, 1, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5, createLoginUsers: true, createActiveUsers: false);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2017, 2, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5, createLoginUsers: false, createActiveUsers: true);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertChargeableUsages("Not yet start billing registered users - company 1", new ZDateTime(2016, 10, 1), clientCompany1.PK, true, 2, false, 0);
				AssertChargeableUsages("Not yet start billing registered users - company 2", new ZDateTime(2016, 10, 1), clientCompany2.PK, true, 3, false, 0);

				AssertChargeableUsages("Start billing registered users - company 1", new ZDateTime(2016, 11, 1), clientCompany1.PK, true, 2, true, 3);
				AssertChargeableUsages("Start billing registered users - company 2", new ZDateTime(2016, 11, 1), clientCompany2.PK, true, 3, true, 3);

				AssertChargeableUsages("Continue billing registered users - company 1", new ZDateTime(2016, 12, 1), clientCompany1.PK, true, 2, true, 3);
				AssertChargeableUsages("Continue billing registered users - company 2", new ZDateTime(2016, 12, 1), clientCompany2.PK, true, 3, true, 3);

				AssertChargeableUsages("No active users data - company 1", new ZDateTime(2017, 1, 1), clientCompany1.PK, true, 2, true, 2);
				AssertChargeableUsages("No active users data - company 2", new ZDateTime(2017, 1, 1), clientCompany2.PK, true, 3, true, 3);

				AssertChargeableUsages("No login users data - company 1", new ZDateTime(2017, 2, 1), clientCompany1.PK, false, 0, true, 3);
				AssertChargeableUsages("No login users data - company 2", new ZDateTime(2017, 2, 1), clientCompany2.PK, false, 0, true, 2);
			});

			db.LD_LicenceType = "TRN";
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2017, 3, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5);
			Factory.Save();

			AssertChargeableUsages("Non production database - company 1", new ZDateTime(2017, 3, 1), clientCompany1.PK, false, 0, false, 0);
			AssertChargeableUsages("Non production database - company 2", new ZDateTime(2017, 3, 1), clientCompany2.PK, false, 0, false, 0);
		}

		public void TestGetOdmUsages_CountryUsages_MCC()
		{
			EServicesBillingTestHelper.CreateTable();

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA", false);
			var db = licence.Database;
			db.LD_DatabaseNumber = 1983;

			var clientCompany1 = db.ClientCompanies.AddNew();
			clientCompany1.LCC_LD = db.PK;
			clientCompany1.LCC_Code = "AAA";
			clientCompany1.LCC_RN_NKCountryCode = "AU";
			var clientCompany2 = db.ClientCompanies.AddNew();
			clientCompany2.LCC_LD = db.PK;
			clientCompany2.LCC_Code = "BBB";
			clientCompany2.LCC_RN_NKCountryCode = "GB";

			var clientStaff1 = Factory.New<ClientStaff>();
			clientStaff1.LS_Code = "AAA";
			clientStaff1.LS_FullName = "AAA Name";
			clientStaff1.LS_LD = db.PK;

			var clientStaff2 = Factory.New<ClientStaff>();
			clientStaff2.LS_Code = "BBB";
			clientStaff2.LS_FullName = "BBB Name";
			clientStaff2.LS_LD = db.PK;

			var clientStaff3 = Factory.New<ClientStaff>();
			clientStaff3.LS_Code = "TUR";
			clientStaff3.LS_FullName = "Test User";
			clientStaff3.LS_LD = db.PK;

			var clientStaff4 = Factory.New<ClientStaff>();
			clientStaff4.LS_Code = "DUR";
			clientStaff4.LS_FullName = "Demo User";
			clientStaff4.LS_LD = db.PK;

			var clientStaff5 = Factory.New<ClientStaff>();
			clientStaff5.LS_Code = "CCC";
			clientStaff5.LS_FullName = "CCC Name";
			clientStaff5.LS_LD = db.PK;

			//Active users: Company 1 (AAA, BBB, CCC) Company 2 (TUR, DUR)
			//Login users: Company 1 (AAA, TUR) Company 2 (TUR, DUR, CCC) No login (BBB)
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2018, 10, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2018, 11, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2018, 12, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2019, 1, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5, createLoginUsers: true, createActiveUsers: false);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2019, 2, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5, createLoginUsers: false, createActiveUsers: true);
			Factory.Save();

			UpdateChargeableUsage(new ZDateTime(2018, 10, 1));
			UpdateChargeableUsage(new ZDateTime(2018, 11, 1));
			UpdateChargeableUsage(new ZDateTime(2018, 12, 1));
			UpdateChargeableUsage(new ZDateTime(2019, 1, 1));
			UpdateChargeableUsage(new ZDateTime(2019, 2, 1));

			//CountryUsage(MCC)
			var countryUsagesAsText = string.Join("\r\n", Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_SubCode, new[] { "MCC", "DCC" }))
				.Select(x => $"{x.U1_PeriodStart.ToShortDateString()}-{x.U1_Code}-{x.U1_SubCode}-{x.U1_UnitCount}-{x.Database.LD_ServerCode}-{x.CompanyCode}-{x.ClientCompany.LCC_RN_NKCountryCode}")
				.OrderBy(x => x));

			AssertEquals(
@"01-Dec-18-ODM-MCC-1.0000-AAA-AAA-AU
01-Dec-18-ODM-MCC-1.0000-AAA-BBB-GB
01-Jan-19-ODM-MCC-1.0000-AAA-AAA-AU
01-Jan-19-ODM-MCC-1.0000-AAA-BBB-GB
01-Nov-18-ODM-MCC-1.0000-AAA-AAA-AU
01-Nov-18-ODM-MCC-1.0000-AAA-BBB-GB
01-Oct-18-ODM-MCC-1.0000-AAA-AAA-AU
01-Oct-18-ODM-MCC-1.0000-AAA-BBB-GB", countryUsagesAsText);
		}

		public void TestGetOdmUsages_CountryUsages_DCC()
		{
			EServicesBillingTestHelper.CreateTable();

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA", false);
			var db = licence.Database;
			db.LD_DatabaseNumber = 1983;

			var clientCompany1 = db.ClientCompanies.AddNew();
			clientCompany1.LCC_LD = db.PK;
			clientCompany1.LCC_Code = "AAA";
			clientCompany1.LCC_RN_NKCountryCode = "AU";
			var clientCompany2 = db.ClientCompanies.AddNew();
			clientCompany2.LCC_LD = db.PK;
			clientCompany2.LCC_Code = "BBB";
			clientCompany2.LCC_RN_NKCountryCode = "AU";

			var clientStaff1 = Factory.New<ClientStaff>();
			clientStaff1.LS_Code = "AAA";
			clientStaff1.LS_FullName = "AAA Name";
			clientStaff1.LS_LD = db.PK;

			var clientStaff2 = Factory.New<ClientStaff>();
			clientStaff2.LS_Code = "BBB";
			clientStaff2.LS_FullName = "BBB Name";
			clientStaff2.LS_LD = db.PK;

			var clientStaff3 = Factory.New<ClientStaff>();
			clientStaff3.LS_Code = "TUR";
			clientStaff3.LS_FullName = "Test User";
			clientStaff3.LS_LD = db.PK;

			var clientStaff4 = Factory.New<ClientStaff>();
			clientStaff4.LS_Code = "DUR";
			clientStaff4.LS_FullName = "Demo User";
			clientStaff4.LS_LD = db.PK;

			var clientStaff5 = Factory.New<ClientStaff>();
			clientStaff5.LS_Code = "CCC";
			clientStaff5.LS_FullName = "CCC Name";
			clientStaff5.LS_LD = db.PK;

			//Active users: Company 1 (AAA, BBB, CCC) Company 2 (TUR, DUR)
			//Login users: Company 1 (AAA, TUR) Company 2 (TUR, DUR, CCC) No login (BBB)
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2018, 10, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2018, 11, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2018, 12, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2019, 1, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5, createLoginUsers: true, createActiveUsers: false);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2019, 2, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5, createLoginUsers: false, createActiveUsers: true);
			Factory.Save();

			UpdateChargeableUsage(new ZDateTime(2018, 10, 1));
			UpdateChargeableUsage(new ZDateTime(2018, 11, 1));
			UpdateChargeableUsage(new ZDateTime(2018, 12, 1));
			UpdateChargeableUsage(new ZDateTime(2019, 1, 1));
			UpdateChargeableUsage(new ZDateTime(2019, 2, 1));

			var countryUsagesAsText = string.Join("\r\n", Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_SubCode, new[] { "MCC", "DCC" }))
				.Select(x => $"{x.U1_PeriodStart.ToShortDateString()}-{x.U1_Code}-{x.U1_SubCode}-{x.U1_UnitCount}-{x.Database.LD_ServerCode}-{x.CompanyCode}-{x.ClientCompany.LCC_RN_NKCountryCode}")
				.OrderBy(x => x));

			AssertEquals(
@"01-Dec-18-ODM-DCC-1.0000-AAA-BBB-AU
01-Jan-19-ODM-DCC-1.0000-AAA-BBB-AU
01-Nov-18-ODM-DCC-1.0000-AAA-BBB-AU
01-Oct-18-ODM-DCC-1.0000-AAA-BBB-AU", countryUsagesAsText);
		}

		public void TestGetOdmUsages_CountryUsages_DCC_TW()
		{
			var list = new CodeDescriptionBoolCollection();
			list.Add("CN", (NoResString)"CN");
			list.Add("HK", (NoResString)"CN");
			list.Add("MO", (NoResString)"CN");
			list.Add("TW", (NoResString)"CN");
			EDIDataRegistry.Instance.BillingCountryGroups.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			EServicesBillingTestHelper.CreateTable();

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA", false);
			var db = licence.Database;
			db.LD_DatabaseNumber = 1983;

			var clientCompany1 = db.ClientCompanies.AddNew();
			clientCompany1.LCC_LD = db.PK;
			clientCompany1.LCC_Code = "AAA";
			clientCompany1.LCC_RN_NKCountryCode = "TW";
			var clientCompany2 = db.ClientCompanies.AddNew();
			clientCompany2.LCC_LD = db.PK;
			clientCompany2.LCC_Code = "BBB";
			clientCompany2.LCC_RN_NKCountryCode = "CN";

			var clientStaff1 = Factory.New<ClientStaff>();
			clientStaff1.LS_Code = "AAA";
			clientStaff1.LS_FullName = "AAA Name";
			clientStaff1.LS_LD = db.PK;

			var clientStaff2 = Factory.New<ClientStaff>();
			clientStaff2.LS_Code = "BBB";
			clientStaff2.LS_FullName = "BBB Name";
			clientStaff2.LS_LD = db.PK;

			var clientStaff3 = Factory.New<ClientStaff>();
			clientStaff3.LS_Code = "TUR";
			clientStaff3.LS_FullName = "Test User";
			clientStaff3.LS_LD = db.PK;

			var clientStaff4 = Factory.New<ClientStaff>();
			clientStaff4.LS_Code = "DUR";
			clientStaff4.LS_FullName = "Demo User";
			clientStaff4.LS_LD = db.PK;

			var clientStaff5 = Factory.New<ClientStaff>();
			clientStaff5.LS_Code = "CCC";
			clientStaff5.LS_FullName = "CCC Name";
			clientStaff5.LS_LD = db.PK;

			//Active users: Company 1 (AAA, BBB, CCC) Company 2 (TUR, DUR)
			//Login users: Company 1 (AAA, TUR) Company 2 (TUR, DUR, CCC) No login (BBB)
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2018, 10, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2018, 11, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2018, 12, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2019, 1, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5, createLoginUsers: true, createActiveUsers: false);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2019, 2, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5, createLoginUsers: false, createActiveUsers: true);
			Factory.Save();

			UpdateChargeableUsage(new ZDateTime(2018, 10, 1));
			UpdateChargeableUsage(new ZDateTime(2018, 11, 1));
			UpdateChargeableUsage(new ZDateTime(2018, 12, 1));
			UpdateChargeableUsage(new ZDateTime(2019, 1, 1));
			UpdateChargeableUsage(new ZDateTime(2019, 2, 1));

			var countryUsagesAsText = string.Join("\r\n", Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_SubCode, new[] { "MCC", "DCC" }))
				.Select(x => $"{x.U1_PeriodStart.ToShortDateString()}-{x.U1_Code}-{x.U1_SubCode}-{x.U1_UnitCount}-{x.Database.LD_ServerCode}-{x.CompanyCode}-{x.ClientCompany.LCC_RN_NKCountryCode}")
				.OrderBy(x => x));

			AssertEquals(
@"01-Dec-18-ODM-DCC-1.0000-AAA-BBB-CN
01-Jan-19-ODM-DCC-1.0000-AAA-BBB-CN
01-Nov-18-ODM-DCC-1.0000-AAA-BBB-CN
01-Oct-18-ODM-DCC-1.0000-AAA-BBB-CN", countryUsagesAsText);
		}

		public void TestGetOdmUsages_CountryUsages_MCC_ClientCompanyActiveStatus()
		{
			EServicesBillingTestHelper.CreateTable();

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA", false);
			var db = licence.Database;
			db.LD_DatabaseNumber = 1983;

			var clientCompany1 = db.ClientCompanies.AddNew();
			clientCompany1.LCC_LD = db.PK;
			clientCompany1.LCC_Code = "AAA";
			clientCompany1.LCC_RN_NKCountryCode = "AU";
			var clientCompany2 = db.ClientCompanies.AddNew();
			clientCompany2.LCC_LD = db.PK;
			clientCompany2.LCC_Code = "BBB";
			clientCompany2.LCC_RN_NKCountryCode = "GB";

			var clientStaff1 = Factory.New<ClientStaff>();
			clientStaff1.LS_Code = "AAA";
			clientStaff1.LS_FullName = "AAA Name";
			clientStaff1.LS_LD = db.PK;

			var clientStaff2 = Factory.New<ClientStaff>();
			clientStaff2.LS_Code = "BBB";
			clientStaff2.LS_FullName = "BBB Name";
			clientStaff2.LS_LD = db.PK;

			var clientStaff3 = Factory.New<ClientStaff>();
			clientStaff3.LS_Code = "TUR";
			clientStaff3.LS_FullName = "Test User";
			clientStaff3.LS_LD = db.PK;

			var clientStaff4 = Factory.New<ClientStaff>();
			clientStaff4.LS_Code = "DUR";
			clientStaff4.LS_FullName = "Demo User";
			clientStaff4.LS_LD = db.PK;

			var clientStaff5 = Factory.New<ClientStaff>();
			clientStaff5.LS_Code = "CCC";
			clientStaff5.LS_FullName = "CCC Name";
			clientStaff5.LS_LD = db.PK;

			Factory.Save();

			CreateClientCompanyActiveStatusHistory(clientCompany1, 201810);
			CreateClientCompanyActiveStatusHistory(clientCompany2, 201810);
			CreateClientCompanyActiveStatusHistory(clientCompany1, 201811);
			CreateClientCompanyActiveStatusHistory(clientCompany2, 201811);
			CreateClientCompanyActiveStatusHistory(clientCompany1, 201812);
			CreateClientCompanyActiveStatusHistory(clientCompany2, 201812);
			CreateClientCompanyActiveStatusHistory(clientCompany1, 201901);
			CreateClientCompanyActiveStatusHistory(clientCompany2, 201901);
			CreateClientCompanyActiveStatusHistory(clientCompany1, 201902);
			CreateClientCompanyActiveStatusHistory(clientCompany2, 201902);

			UpdateChargeableUsage(new ZDateTime(2018, 10, 1));
			UpdateChargeableUsage(new ZDateTime(2018, 11, 1));
			UpdateChargeableUsage(new ZDateTime(2018, 12, 1));
			UpdateChargeableUsage(new ZDateTime(2019, 1, 1));
			UpdateChargeableUsage(new ZDateTime(2019, 2, 1));

			//CountryUsage(MCC)
			var countryUsagesAsText = string.Join("\r\n", Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_SubCode, new[] { "MCC", "DCC" }))
				.Select(x => $"{x.U1_PeriodStart.ToShortDateString()}-{x.U1_Code}-{x.U1_SubCode}-{x.U1_UnitCount}-{x.Database.LD_ServerCode}-{x.CompanyCode}-{x.ClientCompany.LCC_RN_NKCountryCode}")
				.OrderBy(x => x));

			AssertEquals(
@"01-Dec-18-ODM-MCC-1.0000-AAA-AAA-AU
01-Dec-18-ODM-MCC-1.0000-AAA-BBB-GB
01-Feb-19-ODM-MCC-1.0000-AAA-AAA-AU
01-Feb-19-ODM-MCC-1.0000-AAA-BBB-GB
01-Jan-19-ODM-MCC-1.0000-AAA-AAA-AU
01-Jan-19-ODM-MCC-1.0000-AAA-BBB-GB
01-Nov-18-ODM-MCC-1.0000-AAA-AAA-AU
01-Nov-18-ODM-MCC-1.0000-AAA-BBB-GB
01-Oct-18-ODM-MCC-1.0000-AAA-AAA-AU
01-Oct-18-ODM-MCC-1.0000-AAA-BBB-GB", countryUsagesAsText);
		}

		public void TestGetOdmUsages_CountryUsages_DCC_ClientCompanyActiveStatus()
		{
			EServicesBillingTestHelper.CreateTable();

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA", false);
			var db = licence.Database;
			db.LD_DatabaseNumber = 1983;

			var clientCompany1 = db.ClientCompanies.AddNew();
			clientCompany1.LCC_LD = db.PK;
			clientCompany1.LCC_Code = "AAA";
			clientCompany1.LCC_RN_NKCountryCode = "AU";
			var clientCompany2 = db.ClientCompanies.AddNew();
			clientCompany2.LCC_LD = db.PK;
			clientCompany2.LCC_Code = "BBB";
			clientCompany2.LCC_RN_NKCountryCode = "AU";

			var clientStaff1 = Factory.New<ClientStaff>();
			clientStaff1.LS_Code = "AAA";
			clientStaff1.LS_FullName = "AAA Name";
			clientStaff1.LS_LD = db.PK;

			var clientStaff2 = Factory.New<ClientStaff>();
			clientStaff2.LS_Code = "BBB";
			clientStaff2.LS_FullName = "BBB Name";
			clientStaff2.LS_LD = db.PK;

			var clientStaff3 = Factory.New<ClientStaff>();
			clientStaff3.LS_Code = "TUR";
			clientStaff3.LS_FullName = "Test User";
			clientStaff3.LS_LD = db.PK;

			var clientStaff4 = Factory.New<ClientStaff>();
			clientStaff4.LS_Code = "DUR";
			clientStaff4.LS_FullName = "Demo User";
			clientStaff4.LS_LD = db.PK;

			var clientStaff5 = Factory.New<ClientStaff>();
			clientStaff5.LS_Code = "CCC";
			clientStaff5.LS_FullName = "CCC Name";
			clientStaff5.LS_LD = db.PK;

			Factory.Save();

			CreateClientCompanyActiveStatusHistory(clientCompany1, 201810);
			CreateClientCompanyActiveStatusHistory(clientCompany2, 201810);
			CreateClientCompanyActiveStatusHistory(clientCompany1, 201811);
			CreateClientCompanyActiveStatusHistory(clientCompany2, 201811);
			CreateClientCompanyActiveStatusHistory(clientCompany1, 201812);
			CreateClientCompanyActiveStatusHistory(clientCompany2, 201812);
			CreateClientCompanyActiveStatusHistory(clientCompany1, 201901);
			CreateClientCompanyActiveStatusHistory(clientCompany2, 201901);
			CreateClientCompanyActiveStatusHistory(clientCompany1, 201902);
			CreateClientCompanyActiveStatusHistory(clientCompany2, 201902);

			UpdateChargeableUsage(new ZDateTime(2018, 10, 1));
			UpdateChargeableUsage(new ZDateTime(2018, 11, 1));
			UpdateChargeableUsage(new ZDateTime(2018, 12, 1));
			UpdateChargeableUsage(new ZDateTime(2019, 1, 1));
			UpdateChargeableUsage(new ZDateTime(2019, 2, 1));

			var countryUsagesAsText = string.Join("\r\n", Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_SubCode, new[] { "MCC", "DCC" }))
				.Select(x => $"{x.U1_PeriodStart.ToShortDateString()}-{x.U1_Code}-{x.U1_SubCode}-{x.U1_UnitCount}-{x.Database.LD_ServerCode}-{x.CompanyCode}-{x.ClientCompany.LCC_RN_NKCountryCode}")
				.OrderBy(x => x));

			AssertEquals(
@"01-Dec-18-ODM-DCC-1.0000-AAA-AAA-AU
01-Feb-19-ODM-DCC-1.0000-AAA-AAA-AU
01-Jan-19-ODM-DCC-1.0000-AAA-AAA-AU
01-Nov-18-ODM-DCC-1.0000-AAA-AAA-AU
01-Oct-18-ODM-DCC-1.0000-AAA-AAA-AU", countryUsagesAsText);
		}

		public void TestGetOdmUsages_COR_COW()
		{
			EServicesBillingTestHelper.CreateTable();

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA", false);
			var db = licence.Database;
			db.LD_DatabaseNumber = 1983;
			db.LD_HostedLocation = "SYD";

			var clientCompany1 = db.ClientCompanies.AddNew();
			clientCompany1.LCC_LD = db.PK;
			clientCompany1.LCC_Code = "AAA";
			var clientCompany2 = db.ClientCompanies.AddNew();
			clientCompany2.LCC_LD = db.PK;
			clientCompany2.LCC_Code = "BBB";

			var clientStaff1 = Factory.New<ClientStaff>();
			clientStaff1.LS_Code = "AAA";
			clientStaff1.LS_FullName = "AAA Name";
			clientStaff1.LS_LD = db.PK;

			var clientStaff2 = Factory.New<ClientStaff>();
			clientStaff2.LS_Code = "BBB";
			clientStaff2.LS_FullName = "BBB Name";
			clientStaff2.LS_LD = db.PK;

			var clientStaff3 = Factory.New<ClientStaff>();
			clientStaff3.LS_Code = "TUR";
			clientStaff3.LS_FullName = "Test User";
			clientStaff3.LS_LD = db.PK;

			var clientStaff4 = Factory.New<ClientStaff>();
			clientStaff4.LS_Code = "DUR";
			clientStaff4.LS_FullName = "Demo User";
			clientStaff4.LS_LD = db.PK;

			var clientStaff5 = Factory.New<ClientStaff>();
			clientStaff5.LS_Code = "CCC";
			clientStaff5.LS_FullName = "CCC Name";
			clientStaff5.LS_LD = db.PK;

			//Active users: Company 1 (AAA, BBB, CCC) Company 2 (TUR, DUR)
			//Login users: Company 1 (AAA, TUR) Company 2 (TUR, DUR, CCC) No login (BBB)
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2016, 10, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2016, 11, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2016, 12, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2018, 6, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5, createLoginUsers: true, createActiveUsers: false);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2018, 7, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5, createLoginUsers: false, createActiveUsers: true);
			Factory.Save();

			UpdateChargeableUsage(new ZDateTime(2016, 10, 1));
			UpdateChargeableUsage(new ZDateTime(2016, 11, 1));
			UpdateChargeableUsage(new ZDateTime(2016, 12, 1));
			UpdateChargeableUsage(new ZDateTime(2018, 6, 1));
			UpdateChargeableUsage(new ZDateTime(2018, 7, 1));
			UpdateChargeableUsage(new ZDateTime(2016, 10, 1));
			UpdateChargeableUsage(new ZDateTime(2016, 11, 1));
			UpdateChargeableUsage(new ZDateTime(2016, 12, 1));
			UpdateChargeableUsage(new ZDateTime(2018, 6, 1));
			UpdateChargeableUsage(new ZDateTime(2018, 7, 1));

			var usagesAsText = string.Join("\r\n", Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_SubCode, new[] { "COR", "COW" }))
				.Select(x => $"{x.U1_PeriodStart.ToShortDateString()}-{x.U1_Code}-{x.U1_SubCode}-{x.U1_UnitCount}-{x.Database.LD_ServerCode}-{x.CompanyCode}-{x.ClientCompany.LCC_RN_NKCountryCode}")
				.OrderBy(x => x));

			AssertEquals(
@"01-Dec-16-ODM-COR-2.0000-AAA-AAA-
01-Dec-16-ODM-COR-3.0000-AAA-BBB-
01-Jul-18-ODM-COW-2.0000-AAA-BBB-
01-Jul-18-ODM-COW-3.0000-AAA-AAA-
01-Jun-18-ODM-COR-2.0000-AAA-AAA-
01-Jun-18-ODM-COR-3.0000-AAA-BBB-
01-Jun-18-ODM-COW-2.0000-AAA-AAA-
01-Jun-18-ODM-COW-2.0000-AAA-BBB-
01-Nov-16-ODM-COR-2.0000-AAA-AAA-
01-Nov-16-ODM-COR-3.0000-AAA-BBB-
01-Oct-16-ODM-COR-2.0000-AAA-AAA-
01-Oct-16-ODM-COR-3.0000-AAA-BBB-", usagesAsText);
		}

		public void TestGetStlUsages_USR_USW()
		{
			EServicesBillingTestHelper.CreateTable();

			var stdLicence = BillingTestHelper.CreateLicence(Factory, "EDI", "EDI", "SYD", false);
			var stdPriceCompany = stdLicence.Company;

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "USR", "SHP", "RBU");

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA", false);
			var db = licence.Database;
			db.LD_DatabaseNumber = 1983;
			db.LD_HostedLocation = "SYD";

			var clientCompany1 = db.ClientCompanies.AddNew();
			clientCompany1.LCC_LD = db.PK;
			clientCompany1.LCC_Code = "AAA";
			var clientCompany2 = db.ClientCompanies.AddNew();
			clientCompany2.LCC_LD = db.PK;
			clientCompany2.LCC_Code = "BBB";

			Factory.Save();

			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdLicence.LicenceCode);

			var clientStaff1 = Factory.New<ClientStaff>();
			clientStaff1.LS_Code = "AAA";
			clientStaff1.LS_FullName = "AAA Name";
			clientStaff1.LS_LD = db.PK;

			var clientStaff2 = Factory.New<ClientStaff>();
			clientStaff2.LS_Code = "BBB";
			clientStaff2.LS_FullName = "BBB Name";
			clientStaff2.LS_LD = db.PK;

			var clientStaff3 = Factory.New<ClientStaff>();
			clientStaff3.LS_Code = "TUR";
			clientStaff3.LS_FullName = "Test User";
			clientStaff3.LS_LD = db.PK;

			var clientStaff4 = Factory.New<ClientStaff>();
			clientStaff4.LS_Code = "DUR";
			clientStaff4.LS_FullName = "Demo User";
			clientStaff4.LS_LD = db.PK;

			var clientStaff5 = Factory.New<ClientStaff>();
			clientStaff5.LS_Code = "CCC";
			clientStaff5.LS_FullName = "CCC Name";
			clientStaff5.LS_LD = db.PK;

			//Active users: Company 1 (AAA, BBB, CCC) Company 2 (TUR, DUR)
			//Login users: Company 1 (AAA, TUR) Company 2 (TUR, DUR, CCC) No login (BBB)
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2016, 10, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2016, 11, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2016, 12, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5, createRoboticLoginUsages: true);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2018, 6, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5, createLoginUsers: true, createActiveUsers: false);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2018, 7, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff3, clientStaff4, clientStaff5, createLoginUsers: false, createActiveUsers: true, createRoboticLoginUsages: true);
			Factory.Save();

			UpdateChargeableUsage(new ZDateTime(2016, 10, 1), "STL");
			UpdateChargeableUsage(new ZDateTime(2016, 11, 1), "STL");
			UpdateChargeableUsage(new ZDateTime(2016, 12, 1), "STL");
			UpdateChargeableUsage(new ZDateTime(2018, 6, 1), "STL");
			UpdateChargeableUsage(new ZDateTime(2018, 7, 1), "STL");
			UpdateChargeableUsage(new ZDateTime(2016, 10, 1), "STL");
			UpdateChargeableUsage(new ZDateTime(2016, 11, 1), "STL");
			UpdateChargeableUsage(new ZDateTime(2016, 12, 1), "STL");
			UpdateChargeableUsage(new ZDateTime(2018, 6, 1), "STL");
			UpdateChargeableUsage(new ZDateTime(2018, 7, 1), "STL");

			var usagesAsText = string.Join("\r\n", Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_SubCode, new[] { "USR", "USW" }))
				.Select(x => $"{x.U1_PeriodStart.ToShortDateString()}-{x.U1_Code}-{x.U1_SubCode}-{x.U1_UnitCount}-{x.Database.LD_ServerCode}-{x.CompanyCode}-{x.ClientCompany.LCC_RN_NKCountryCode}")
				.OrderBy(x => x));

			AssertEquals(
@"01-Dec-16-STL-USR-2.0000-AAA-BBB-
01-Dec-16-STL-USR-3.0000-AAA-AAA-
01-Jul-18-STL-USR-2.0000-AAA-BBB-
01-Jul-18-STL-USR-3.0000-AAA-AAA-
01-Jul-18-STL-USW-4.0000-AAA-BBB-
01-Jul-18-STL-USW-6.0000-AAA-AAA-
01-Nov-16-STL-USR-2.0000-AAA-BBB-
01-Nov-16-STL-USR-3.0000-AAA-AAA-
01-Oct-16-STL-USR-2.0000-AAA-BBB-
01-Oct-16-STL-USR-3.0000-AAA-AAA-", usagesAsText);
		}

		void CreateLoginUsagesAndActiveUsers(ZDateTime periodStart, LicenceDatabase db, ClientCompany clientCompany1, ClientCompany clientCompany2, ClientStaff staffLoginToCompany1, ClientStaff staffLoginToBothCompanies, ClientStaff staffLoginToCompany2, ClientStaff staffInCompany1LoginToCompany2, bool createLoginUsers = true, bool createActiveUsers = true, bool createRoboticLoginUsages = false)
		{
			if (createLoginUsers)
			{
				BillingTestHelper.CreateEdiLicenceUsage(clientCompany1, staffLoginToCompany1, BillingConstants.BillingModel.ODM, BillingConstants.CoreModuleCode, periodStart.AddDays(5));
				BillingTestHelper.CreateEdiLicenceUsage(clientCompany1, staffLoginToBothCompanies, BillingConstants.BillingModel.ODM, BillingConstants.CoreModuleCode, periodStart.AddDays(5));
				BillingTestHelper.CreateEdiLicenceUsage(clientCompany2, staffLoginToBothCompanies, BillingConstants.BillingModel.ODM, BillingConstants.CoreModuleCode, periodStart.AddDays(10));
				BillingTestHelper.CreateEdiLicenceUsage(clientCompany2, staffLoginToCompany2, BillingConstants.BillingModel.ODM, BillingConstants.CoreModuleCode, periodStart.AddDays(15));
				BillingTestHelper.CreateEdiLicenceUsage(clientCompany2, staffInCompany1LoginToCompany2, BillingConstants.BillingModel.ODM, BillingConstants.CoreModuleCode, periodStart.AddDays(5));
			}

			if (createActiveUsers)
			{
				List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
				infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", periodStart.AddDays(1), "DDDABCSYD", "", db.DatabaseId, clientCompany1.PK, "AAA", "AAA Name", "", "", null));
				infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", periodStart.AddDays(1), "DDDABCSYD", "", db.DatabaseId, clientCompany1.PK, "BBB", "BBB Name", "", "", null));
				infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", periodStart.AddDays(1), "DDDABCSYD", "", db.DatabaseId, clientCompany1.PK, "CCC", "CCC Name", "", "", null));
				infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", periodStart.AddDays(1), "DDDABCSYD", "", db.DatabaseId, clientCompany2.PK, "TUR", "Test User", "", "", null));
				infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", periodStart.AddDays(1), "DDDABCSYD", "", db.DatabaseId, clientCompany2.PK, "DUR", "Demo User", "", "", null));
				EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
				EServicesBillingTestHelper.AddTransactions(infoList);
			}

			if (createRoboticLoginUsages)
			{
				List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
				infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", periodStart.AddDays(1), "DDDABCSYD", "", db.DatabaseId, clientCompany1.PK, "AAA", "AAA Name", "", "", null));
				infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", periodStart.AddDays(1), "DDDABCSYD", "", db.DatabaseId, clientCompany1.PK, "BBB", "BBB Name", "", "", null));
				infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", periodStart.AddDays(1), "DDDABCSYD", "", db.DatabaseId, clientCompany1.PK, "CCC", "CCC Name", "", "", null));
				infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", periodStart.AddDays(1), "DDDABCSYD", "", db.DatabaseId, clientCompany2.PK, "TUR", "Test User", "", "", null));
				infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", periodStart.AddDays(1), "DDDABCSYD", "", db.DatabaseId, clientCompany2.PK, "DUR", "Demo User", "", "", null));
				EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList, 20);
				EServicesBillingTestHelper.AddTransactions(infoList);
			}
		}

		void CreateClientCompanyActiveStatusHistory(ClientCompany clientCompany, int period)
		{
			const string sql = @"
if not exists(select top 1 CSH_LCC from dbo.ClientCompanyActiveStatusHistory where CSH_LCC = @LCC_PK and CSH_Period = @Period)
	insert dbo.ClientCompanyActiveStatusHistory(CSH_LCC, CSH_Period)
	values (@LCC_PK, @Period);
";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@LCC_PK", SqlDbType.UniqueIdentifier, clientCompany.PK.ToGuid());
				cmd.AddParameter("@Period", SqlDbType.Int, period);

				cmd.ExecuteNonQuery();
			}
		}

		void UpdateChargeableUsage(ZDateTime periodStart, string systemCode = "ODM")
		{
			using (var cmd = Db.Connection.Command(BillingUsageSchema.ChargeableUsageUpdate))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@FirstDayOfMonth", SqlDbType.SmallDateTime, periodStart.ToDateTime());
				cmd.AddParameter("@PeriodStartTimeUtc", SqlDbType.SmallDateTime, periodStart.ToDateTime());
				cmd.AddParameter("@PeriodEndTimeUtc", SqlDbType.SmallDateTime, periodStart.AddMonths(1).ToDateTime());
				cmd.AddParameter("@Code", SqlDbType.VarChar, 3, systemCode);
				cmd.ExecuteProcedureWithReturnValue();
			}
		}

		void AssertChargeableUsages(string message, ZDateTime periodStart, ZGuid companyPk, bool expectedCOR, int expectedCORCount, bool expectedUSO, int expectedUSOCount)
		{
			using (var cmd = Db.Connection.Command(BillingUsageSchema.ChargeableUsageUpdate))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@FirstDayOfMonth", SqlDbType.SmallDateTime, periodStart.ToDateTime());
				cmd.AddParameter("@PeriodStartTimeUtc", SqlDbType.SmallDateTime, periodStart.ToDateTime());
				cmd.AddParameter("@PeriodEndTimeUtc", SqlDbType.SmallDateTime, periodStart.AddMonths(1).ToDateTime());
				cmd.AddParameter("@Code", SqlDbType.VarChar, 3, "ODM");
				cmd.ExecuteProcedureWithReturnValue();
			}

			var corChargeQuery = new ZQuery(ClientChargeableUsageSchema.U1_PeriodStart, periodStart);
			corChargeQuery.AddToFilter(ClientChargeableUsageSchema.U1_Code, BillingConstants.BillingSystem.ODM);
			corChargeQuery.AddToFilter(ClientChargeableUsageSchema.U1_SubCode, BillingConstants.CoreModuleCode);
			corChargeQuery.AddToFilter(ClientChargeableUsageSchema.U1_LCC, companyPk);
			var corCharge = Factory.LoadTop1<ClientChargeableUsage>(corChargeQuery);
			if (expectedCOR)
			{
				AssertEquals(message + ": Core users count", expectedCORCount, corCharge.U1_UnitCount.ToZInt());
			}
			else
			{
				AssertNull(message + ": No Core users count", corCharge);
			}

			var usoChargeQuery = new ZQuery(ClientChargeableUsageSchema.U1_PeriodStart, periodStart);
			usoChargeQuery.AddToFilter(ClientChargeableUsageSchema.U1_Code, BillingConstants.BillingSystem.ODM);
			usoChargeQuery.AddToFilter(ClientChargeableUsageSchema.U1_SubCode, BillingConstants.RegisteredUserModuleCode);
			usoChargeQuery.AddToFilter(ClientChargeableUsageSchema.U1_LCC, companyPk);
			var usoCharge = Factory.LoadTop1<ClientChargeableUsage>(usoChargeQuery);
			if (expectedUSO)
			{
				AssertEquals(message + ": Registered users count", expectedUSOCount, usoCharge.U1_UnitCount.ToZInt());
			}
			else
			{
				AssertNull(message + ": No registered users count", usoCharge);
			}
		}

		#endregion

		#region EdiGetOdmPriceHeadersForDate

		public void TestEdiGetOdmPriceHeaderForDate_StandardPrices()
		{
			var stdPriceEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			Guid stdPriceCompanyPk;
			Guid.TryParse("31754C3F-4782-4504-AC75-C92B0EEB1B73", out stdPriceCompanyPk);
			var stdPriceCompany = Factory.NewWithPrimaryKey<LicenceCompany>(stdPriceCompanyPk);
			stdPriceCompany.FillWithValidTestData();
			stdPriceCompany.LC_LE = stdPriceEnterprise.PK;
			stdPriceCompany.LC_CompanyCode = "DDD";

			var stdPriceHeader = stdPriceCompany.PriceHeaders.AddNew();
			stdPriceHeader.L6_SystemCode = "ODM";
			stdPriceHeader.L6_RX_NKCurrency = "AUD";
			stdPriceHeader.L6_PricelistVersion = "CW1 v8.1";
			stdPriceHeader.L6_IsStandard = false;
			stdPriceHeader.L6_ValidFrom = new ZDateTime(2015, 7, 1);

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA", false);
			var db = licence.Database;
			db.LD_DatabaseNumber = 1983;
			var company = licence.Company;

			var clientCompany = db.ClientCompanies.AddNew();
			clientCompany.LCC_LD = db.PK;
			clientCompany.LCC_Code = "AAA";

			var priceHeader = company.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = "ODM";
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_PricelistVersion = "CW1 v8.1";
			priceHeader.L6_IsStandard = true;
			priceHeader.L6_ValidFrom = new ZDateTime(2016, 10, 1);

			Factory.Save();

			var sql = "select LA_LC, L6_PK, L6_ValidFrom from EdiGetOdmPriceHeadersForDate(@DateFrom) where LA_PK = @LicenceHeaderPk";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@DateFrom", SqlDbType.SmallDateTime, new DateTime(2016, 11, 1));
				cmd.AddParameter("@LicenceHeaderPk", SqlDbType.UniqueIdentifier, licence.PK.ToGuid());

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						AssertEquals("LA_LC", company.PK.ToGuid(), (Guid)reader[0]);
						AssertEquals("L6_PK", stdPriceHeader.PK.ToGuid(), (Guid)reader[1]);
						AssertEquals("L6_ValidFrom", priceHeader.L6_ValidFrom.ToDateTime(), (DateTime)reader[2]);
					}
				}
			}
		}

		#endregion

		#region OdmMonthlyModuleBundleUsers

		public void TestOdmMonthlyModuleBundleUsers()
		{
			EServicesBillingTestHelper.CreateTable();

			var stdPriceEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			Guid stdPriceCompanyPk;
			Guid.TryParse("31754C3F-4782-4504-AC75-C92B0EEB1B73", out stdPriceCompanyPk);
			var stdPriceCompany = Factory.NewWithPrimaryKey<LicenceCompany>(stdPriceCompanyPk);
			stdPriceCompany.FillWithValidTestData();
			stdPriceCompany.LC_LE = stdPriceEnterprise.PK;
			stdPriceCompany.LC_CompanyCode = "DDD";

			var stdPriceHeader1 = stdPriceCompany.PriceHeaders.AddNew();
			stdPriceHeader1.L6_SystemCode = "ODM";
			stdPriceHeader1.L6_RX_NKCurrency = "AUD";
			stdPriceHeader1.L6_PricelistVersion = "CW1 v8.1";
			stdPriceHeader1.L6_IsStandard = false;
			stdPriceHeader1.L6_ValidFrom = new ZDateTime(2015, 10, 1);

			var stdPriceHeader2 = stdPriceCompany.PriceHeaders.AddNew();
			stdPriceHeader2.L6_SystemCode = "ODM";
			stdPriceHeader2.L6_RX_NKCurrency = "AUD";
			stdPriceHeader2.L6_PricelistVersion = "CW1 v9";
			stdPriceHeader2.L6_IsStandard = false;
			stdPriceHeader2.L6_ValidFrom = new ZDateTime(2016, 6, 1);

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA", false);
			var db = licence.Database;
			db.LD_DatabaseNumber = 1983;

			var priceHeader1 = licence.Company.PriceHeaders.AddNew();
			priceHeader1.L6_SystemCode = "ODM";
			priceHeader1.L6_RX_NKCurrency = "AUD";
			priceHeader1.L6_PricelistVersion = "CW1 v8.1";
			priceHeader1.L6_IsStandard = true;
			priceHeader1.L6_ValidFrom = new ZDateTime(2016, 1, 1);

			var priceHeader2 = licence.Company.PriceHeaders.AddNew();
			priceHeader2.L6_SystemCode = "ODM";
			priceHeader2.L6_RX_NKCurrency = "AUD";
			priceHeader2.L6_PricelistVersion = "CW1 v9";
			priceHeader2.L6_IsStandard = true;
			priceHeader2.L6_ValidFrom = new ZDateTime(2016, 7, 1);

			var clientCompany = db.ClientCompanies.AddNew();
			clientCompany.LCC_LD = db.PK;
			clientCompany.LCC_Code = "AAA";

			var clientStaff = Factory.New<ClientStaff>();
			clientStaff.LS_Code = "AAA";
			clientStaff.LS_FullName = "AAA Name";
			clientStaff.LS_LD = db.PK;

			BillingTestHelper.CreateEdiLicenceUsage(clientCompany, clientStaff, BillingConstants.BillingModel.ODM, BillingConstants.CoreModuleCode, new ZDateTime(2016, 7, 5));

			Factory.Save();

			var sql = "select L6_PK, L6_ValidFrom from " + BillingUsageSchema.OdmMonthlyModuleBundleUsers + "(@FirstDayOfMonth)";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@FirstDayOfMonth", SqlDbType.SmallDateTime, new DateTime(2016, 7, 1));

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						AssertEquals("L6_PK", stdPriceHeader2.PK.ToGuid(), (Guid)reader[0]);
						AssertEquals("L6_ValidFrom", priceHeader2.L6_ValidFrom.ToDateTime(), (DateTime)reader[1]);
					}
				}
			}
		}

		#endregion

		#region E2E / E2W

		public void TestGetUsageE2E_E2W()
		{
			EServicesBillingTestHelper.CreateTable();

			var licReceiver = BillingTestHelper.CreateLicence(Factory, "EN0", "CM0", "SV0", true);
			var dbReceiver = licReceiver.Database;
			dbReceiver.LD_DatabaseNumber = 1900;

			var licSender1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CM1", "SV1", true);
			var dbSender1 = licSender1.Database;
			dbSender1.LD_DatabaseNumber = 1901;

			var licSender2 = BillingTestHelper.CreateLicence(Factory, "EN2", "CM2", "SV2", true);
			var dbSender2 = licSender2.Database;
			dbSender2.LD_DatabaseNumber = 1902;

			var licSender3 = BillingTestHelper.CreateLicence(Factory, "EN3", "CM3", "SV3", true);
			var dbSender3 = licSender3.Database;
			dbSender3.LD_DatabaseNumber = 1903;

			var orgWIP = Factory.NewWithValidTestData<OrgHeader>();
			orgWIP.OH_IsUserFlag24 = true; //Marketing Option of  "WiseIndustry Partner" 

			dbSender1.ClientCompanies[0].Org.SetRelatedParty(orgWIP, "WRP", "CM");
			dbSender1.ClientCompanies[0].Org.AllRelatedParties[0].PR_SystemCreateTimeUtc = new ZDateTime(2017, 1, 1);

			Factory.Save();

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2017, 5, 2), "EN0CM0SV0", "EN0CM0SV0", dbReceiver.DatabaseId, dbReceiver.ClientCompanies[0].PK, "EN0CM0SV0", "", null, null, null, "STL", 2));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2017, 5, 3), "EN0CM0SV0", "EN0CM0SV0", dbReceiver.DatabaseId, dbReceiver.ClientCompanies[0].PK, "EN1CM1SV1", "", null, null, null, "STL", 4));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2017, 5, 4), "EN0CM0SV0", "EN0CM0SV0", dbReceiver.DatabaseId, dbReceiver.ClientCompanies[0].PK, "EN2CM2SV2", "", null, null, null, "STL", 8));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2017, 5, 5), "EN0CM0SV0", "EN0CM0SV0", dbReceiver.DatabaseId, dbReceiver.ClientCompanies[0].PK, "EN3CM3SV3", "", null, null, null, "STL", 16));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2017, 5, 6), "EN0CM0SV0", "EN0CM0SV0", dbReceiver.DatabaseId, dbReceiver.ClientCompanies[0].PK, "EN9CM9SV9", "", null, null, null, "STL", 32));

			EServicesBillingTestHelper.AddTransactions(infoList);

			var sql = "select * from EdiGetChargeableUsage" + BillingConstants.BillingSystem.E2E + "(201705, '2017-05-01 00:00', '2017-06-01 00:00')";

			using (var cmd = Db.Connection.Command(sql))
			{
				var table = new DataTable();
				cmd.NewDataAdapter().Fill(table);
				AssertEquals(2, table.Rows.Count);
				var rowE2E = table.Rows.OfType<DataRow>().First(x => x["SubCode"].ToString() == "E2E");
				var rowE2W = table.Rows.OfType<DataRow>().First(x => x["SubCode"].ToString() == "E2W");

				AssertEquals(dbReceiver.PK.ToString(), rowE2E["LD_PK"].ToString());
				AssertEquals(dbReceiver.ClientCompanies[0].PK.ToString(), rowE2E["LCC_PK"].ToString());
				AssertEquals("E2E", rowE2E["SubCode"].ToString());
				AssertEquals("58.0000", rowE2E["UnitCount"].ToString());

				AssertEquals(dbReceiver.PK.ToString(), rowE2W["LD_PK"].ToString());
				AssertEquals(dbReceiver.ClientCompanies[0].PK.ToString(), rowE2W["LCC_PK"].ToString());
				AssertEquals("E2W", rowE2W["SubCode"].ToString());
				AssertEquals("4.0000", rowE2W["UnitCount"].ToString());
			}
		}

		#endregion

		/// <summary>
		/// Tests vwBillingClientCompany which is used to populate the Client Company dimension in the billing cube.
		/// </summary>
		public void TestBillingClientCompany()
		{
			var licA1 = BillingTestHelper.CreateLicence(Factory, "AA1");

			var lic1Prices = licA1.Company.PriceHeaders.AddNew();
			BillingTestHelper.CreatePriceLink(licA1.Database, lic1Prices, new ZDateTime(2015, 7, 1));
			BillingTestHelper.CreatePriceLink(licA1.Database, lic1Prices, new ZDateTime(2017, 8, 12));

			var licA2 = BillingTestHelper.CreateAnotherDatabase(licA1, "AA2");
			licA2.Database.LD_HostedLocation = "SYD";
			licA2.Database.LD_LicenceType = "TST";
			licA2.ClientCompany.LCC_RN_NKCountryCode = "AU";

			var licB = BillingTestHelper.CreateLicence(Factory, "BBB");

			Factory.Save();

			var resultTable = DataUtils.GetDataTableFromQuery(
				((IDbConnected)Factory).Connection,
				"SELECT * FROM dbo.vwBillingClientCompany ORDER BY [Licence Key]");
			AssertEquals("Row count", 3, resultTable.Rows.Count);

			AssertEquals("Client Company PK [0]", licA1.ClientCompany.PK, new ZGuid(resultTable.Rows[0]["Client Company PK"]));
			AssertEquals("Enterprise Code [0]", "AA1", resultTable.Rows[0]["Enterprise Code"].ToString());
			AssertEquals("Database Number [0] > 0", true, Convert.ToInt32(resultTable.Rows[0]["Database Number"]) > 0);
			AssertEquals("Licence Key [0]", "AA1-AA1-AA1", resultTable.Rows[0]["Licence Key"].ToString());
			AssertEquals("Client Enterprise [0]", "(AA1) AA1AA1 Company", resultTable.Rows[0]["Client Enterprise"].ToString());
			AssertEquals("Client System [0]", "AA1-AA1", resultTable.Rows[0]["Client System"].ToString());
			AssertEquals("Company [0]", "(AA1-AA1-AA1) AA1AA1 Company [AA1AA1]", resultTable.Rows[0]["Company"].ToString());
			AssertEquals("Home Country [0]", "", resultTable.Rows[0]["Home Country"].ToString());
			AssertEquals("Hosted Location [0]", "NCW", resultTable.Rows[0]["Hosted Location"].ToString());
			AssertEquals("System Type [0]", "PRD", resultTable.Rows[0]["System Type"].ToString());
			AssertEquals("STL Start Date [0]", new DateTime(2015, 7, 1), Convert.ToDateTime(resultTable.Rows[0]["STL Start Date"]));

			AssertEquals("Client Company PK [1]", licA2.ClientCompany.PK, new ZGuid(resultTable.Rows[1]["Client Company PK"]));
			AssertEquals("Enterprise Code [1]", "AA1", resultTable.Rows[1]["Enterprise Code"].ToString());
			AssertEquals("Database Number [1] > 0", true, Convert.ToInt32(resultTable.Rows[1]["Database Number"]) > 0);
			AssertEquals("Licence Key [1]", "AA1-AA1-AA2", resultTable.Rows[1]["Licence Key"].ToString());
			AssertEquals("Client Enterprise [1]", "(AA1) AA1AA1 Company", resultTable.Rows[1]["Client Enterprise"].ToString());
			AssertEquals("Client System [1]", "AA1-AA2", resultTable.Rows[1]["Client System"].ToString());
			AssertEquals("Company [1]", "(AA1-AA1-AA2) AA1AA1 Company [AA1AA1]", resultTable.Rows[1]["Company"].ToString());
			AssertEquals("Home Country [1]", "(AU) Australia", resultTable.Rows[1]["Home Country"].ToString());
			AssertEquals("Hosted Location [1]", "SYD", resultTable.Rows[1]["Hosted Location"].ToString());
			AssertEquals("System Type [1]", "TST", resultTable.Rows[1]["System Type"].ToString());
			AssertEquals("STL Start Date [1]", DBNull.Value, resultTable.Rows[1]["STL Start Date"]);

			AssertEquals("Client Company PK [2]", licB.ClientCompany.PK, new ZGuid(resultTable.Rows[2]["Client Company PK"]));
			AssertEquals("Enterprise Code [2]", "BBB", resultTable.Rows[2]["Enterprise Code"].ToString());
			AssertEquals("Database Number [2] > 0", true, Convert.ToInt32(resultTable.Rows[2]["Database Number"]) > 0);
			AssertEquals("Licence Key [2]", "BBB-BBB-BBB", resultTable.Rows[2]["Licence Key"].ToString());
			AssertEquals("Client Enterprise [2]", "(BBB) BBBBBB Company", resultTable.Rows[2]["Client Enterprise"].ToString());
			AssertEquals("Client System [2]", "BBB-BBB", resultTable.Rows[2]["Client System"].ToString());
			AssertEquals("Company [2]", "(BBB-BBB-BBB) BBBBBB Company [BBBBBB]", resultTable.Rows[2]["Company"].ToString());
			AssertEquals("Home Country [2]", "", resultTable.Rows[2]["Home Country"].ToString());
			AssertEquals("Hosted Location [2]", "NCW", resultTable.Rows[2]["Hosted Location"].ToString());
			AssertEquals("System Type [2]", "PRD", resultTable.Rows[2]["System Type"].ToString());
			AssertEquals("STL Start Date [2]", DBNull.Value, resultTable.Rows[2]["STL Start Date"]);
		}

		public void TestBillingClientCompany_WithUnlinkedObjects()
		{
			var enterpriseCode = "ENT";
			var companyCode = "COM";
			var serverCode = "SYD";

			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_FullName = enterpriseCode + companyCode + " Company";
			org.OH_Code = enterpriseCode + companyCode;

			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = enterpriseCode;
			enterprise.LE_OH = org.PK;

			var database = enterprise.Databases.AddNew();
			database.LD_ServerCode = serverCode;
			database.LD_LicenceType = DatabaseTypes.Codes.Production;
			database.LD_Product = ProductTypes.Codes.Enterprise;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_LD = database.PK;
			clientCompany.LCC_Code = companyCode;
			clientCompany.LCC_Name = companyCode + " Co";

			Factory.Save();

			var resultTable = DataUtils.GetDataTableFromQuery(
				((IDbConnected)Factory).Connection,
				"SELECT * FROM dbo.vwBillingClientCompany ORDER BY [Licence Key]");
			AssertEquals("Row count", 1, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertEquals("Client Company PK [0]", clientCompany.PK, new ZGuid(resultTable.Rows[0]["Client Company PK"]));
				AssertEquals("Enterprise Code [0]", "ENT", resultTable.Rows[0]["Enterprise Code"].ToString());
				AssertEquals("Database Number [0] > 0", true, Convert.ToInt32(resultTable.Rows[0]["Database Number"]) > 0);
				AssertEquals("Licence Key [0]", "ENT-COM-SYD", resultTable.Rows[0]["Licence Key"].ToString());
				AssertEquals("Client Enterprise [0]", "(ENT) ENTCOM Company", resultTable.Rows[0]["Client Enterprise"].ToString());
				AssertEquals("Client System [0]", "ENT-SYD", resultTable.Rows[0]["Client System"].ToString());
				AssertEquals("Company [0]", "(ENT-COM-SYD) COM Co", resultTable.Rows[0]["Company"].ToString());
				AssertEquals("Home Country [0]", "", resultTable.Rows[0]["Home Country"].ToString());
				AssertEquals("Hosted Location [0]", "NCW", resultTable.Rows[0]["Hosted Location"].ToString());
				AssertEquals("System Type [0]", "PRD", resultTable.Rows[0]["System Type"].ToString());
				AssertEquals("STL Start Date [0]", DBNull.Value, resultTable.Rows[0]["STL Start Date"]);
			});
		}

		public void TestGetSystemUsage()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var licProduction = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "PRD");
			var licTest = BillingTestHelper.CreateAnotherDatabase(licProduction, "TST");
			licTest.Database.LD_LD_ParentDatabase = licProduction.LA_LD;
			licTest.Database.LD_LicenceType = DatabaseTypes.Codes.Test;

			BillingTestHelper.CreateChargeableUsage(Factory, "HOS", "#HE", periodStart, licProduction.ClientCompany, 2000);
			BillingTestHelper.CreateChargeableUsage(Factory, "HOS", "#HN", periodStart, licTest.ClientCompany, 2000);
			Factory.Save();

			string query = "SELECT OH_PK, OH_Code, OH_FullName, U1_Code, ServerCode, CompanyCode, LCC_PK, LC_PK, LD_PK FROM " + BillingUsageSchema.GetSystemUsage +
				"('" + licProduction.Company.LC_OH + "', '" + periodStart.SqlFormat + "') order by ServerCode;";

			using (var table = new DataTable())
			{
				using (var cmd = Db.Connection.Command(query))
				{
					using (var adapter = cmd.NewDataAdapter())
					{
						adapter.Fill(table);
					}
				}

				AssertEquals(2, table.Rows.Count);

				CombineAssertions(() =>
				{
					var row = table.Rows[0];
					AssertEquals("1 OH_PK", licProduction.Company.LC_OH.ToGuid(), (Guid)row["OH_PK"]);
					AssertEquals("1 OH_Code", licProduction.Company.Header.OH_Code, (string)row["OH_Code"]);
					AssertEquals("1 U1_Code", "HOS", (string)row["U1_Code"]);
					AssertEquals("1 ServerCode", "PRD", (string)row["ServerCode"]);
					AssertEquals("1 CompanyCode", "COM", (string)row["CompanyCode"]);
					AssertEquals("1 LD_PK", licProduction.LA_LD.ToGuid(), (Guid)row["LD_PK"]);

					row = table.Rows[1];
					AssertEquals("2 OH_PK", licTest.Company.LC_OH.ToGuid(), (Guid)row["OH_PK"]);
					AssertEquals("2 OH_Code", licTest.Company.Header.OH_Code, (string)row["OH_Code"]);
					AssertEquals("2 U1_Code", "HOS", (string)row["U1_Code"]);
					AssertEquals("2 ServerCode", "TST", (string)row["ServerCode"]);
					AssertEquals("2 CompanyCode", "COM", (string)row["CompanyCode"]);
					AssertEquals("2 LD_PK", licTest.LA_LD.ToGuid(), (Guid)row["LD_PK"]);
				});
			}
		}

		public void TestGetSystemUsage_ValidStlUsageCodesOnOdplPricelists()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "PRD");
			var prices = BillingTestHelper.CreatePriceList(lic);

			var shpPrice = prices.Items.AddNew();
			shpPrice.L7_Code = "SHP";
			shpPrice.L7_FeeType = BillingConstants.FeeType.NamedUser;
			shpPrice.L7_Price = 5.00;
			shpPrice.L7_Description = "SHP (STL)";

			BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "COR", periodStart, lic.ClientCompany, 200);
			BillingTestHelper.CreateChargeableUsage(Factory, "HOS", "#HE", periodStart, lic.ClientCompany, 3000);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHP", periodStart, lic.ClientCompany, 50);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHX", periodStart, lic.ClientCompany, 40);
			Factory.Save();

			var regValue = new CodeDescriptionPairList();
			regValue.AddPair("SHP", "");
			EDIDataRegistry.Instance.ValidStlUsageCodesOnOdplPricelists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue);

			string query = "SELECT OH_PK, OH_Code, OH_FullName, U1_Code, ServerCode, CompanyCode, LCC_PK, LC_PK, LD_PK, U1_UnitCount, L7_PK, L7_Description FROM " + BillingUsageSchema.GetSystemUsage +
				"('" + lic.Company.LC_OH + "', '" + periodStart.SqlFormat + "') order by U1_UnitCount;";

			using (var table = new DataTable())
			{
				using (var cmd = Db.Connection.Command(query))
				{
					using (var adapter = cmd.NewDataAdapter())
					{
						adapter.Fill(table);
					}
				}

				AssertEquals(3, table.Rows.Count);

				CombineAssertions(() =>
				{
					var row = table.Rows[0];
					AssertEquals("1 OH_PK", lic.Company.LC_OH.ToGuid(), (Guid)row["OH_PK"]);
					AssertEquals("1 OH_Code", lic.Company.Header.OH_Code, (string)row["OH_Code"]);
					AssertEquals("1 U1_Code", "STL", (string)row["U1_Code"]);
					AssertEquals("1 ServerCode", "PRD", (string)row["ServerCode"]);
					AssertEquals("1 CompanyCode", "COM", (string)row["CompanyCode"]);
					AssertEquals("1 LD_PK", lic.LA_LD.ToGuid(), (Guid)row["LD_PK"]);
					AssertEquals("1 U1_UnitCount", 50, (int)row["U1_UnitCount"]);
					AssertEquals("1 L7_PK", shpPrice.PK, (Guid)row["L7_PK"]);
					AssertEquals("1 L7_Description", "SHP (STL)", (string)row["L7_Description"]);

					row = table.Rows[1];
					AssertEquals("2 OH_PK", lic.Company.LC_OH.ToGuid(), (Guid)row["OH_PK"]);
					AssertEquals("2 OH_Code", lic.Company.Header.OH_Code, (string)row["OH_Code"]);
					AssertEquals("2 U1_Code", "ODM", (string)row["U1_Code"]);
					AssertEquals("2 ServerCode", "PRD", (string)row["ServerCode"]);
					AssertEquals("2 CompanyCode", "COM", (string)row["CompanyCode"]);
					AssertEquals("2 LD_PK", lic.LA_LD.ToGuid(), (Guid)row["LD_PK"]);
					AssertEquals("2 U1_UnitCount", 200, (int)row["U1_UnitCount"]);
					AssertEquals("2 L7_PK", DBNull.Value, row["L7_PK"]);
					AssertEquals("2 L7_Description", DBNull.Value, row["L7_Description"]);

					row = table.Rows[2];
					AssertEquals("3 OH_PK", lic.Company.LC_OH.ToGuid(), (Guid)row["OH_PK"]);
					AssertEquals("3 OH_Code", lic.Company.Header.OH_Code, (string)row["OH_Code"]);
					AssertEquals("3 U1_Code", "HOS", (string)row["U1_Code"]);
					AssertEquals("3 ServerCode", "PRD", (string)row["ServerCode"]);
					AssertEquals("3 CompanyCode", "COM", (string)row["CompanyCode"]);
					AssertEquals("3 LD_PK", lic.LA_LD.ToGuid(), (Guid)row["LD_PK"]);
					AssertEquals("3 U1_UnitCount", 3000, (int)row["U1_UnitCount"]);
					AssertEquals("3 L7_PK", DBNull.Value, row["L7_PK"]);
					AssertEquals("3 L7_Description", DBNull.Value, row["L7_Description"]);
				});
			}
		}

		public void TestGetSystemUsage_ExcludeInactiveLicenceHeader()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "PRD");
			var db1 = licence1.Database;
			var company1 = licence1.Company;
			var licence2a = BillingTestHelper.CreateAnotherDatabase(company1, "SYD");
			licence2a.LA_IsActive = false;
			var db2 = licence2a.Database;
			var licence2b = BillingTestHelper.CreateAnotherLicence(db2, "DEF");
			var company2 = licence2b.Company;

			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "COR", periodStart, licence1.ClientCompany, 2000);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "COR", periodStart, licence2b.ClientCompany, 2000);
			Factory.Save();

			string query = $"select OH_PK, OH_Code, OH_FullName, U1_Code, ServerCode, CompanyCode, LCC_PK, LC_PK, LD_PK from {BillingUsageSchema.GetSystemUsage}(@Org, '{periodStart.SqlFormat}') order by ServerCode";

			using (var table = new DataTable())
			using (var cmd = Db.Connection.Command(query))
			{
				cmd.AddParameter("@Org", SqlDbType.UniqueIdentifier, company1.LC_OH.ToGuid());

				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(table);
				}

				AssertEquals(1, table.Rows.Count);

				CombineAssertions(() =>
				{
					var row = table.Rows[0];
					AssertEquals("OH_PK", company1.LC_OH.ToGuid(), (Guid)row["OH_PK"]);
					AssertEquals("OH_Code", company1.Header.OH_Code, (string)row["OH_Code"]);
					AssertEquals("U1_Code", "DUM", (string)row["U1_Code"]);
					AssertEquals("ServerCode", "PRD", (string)row["ServerCode"]);
					AssertEquals("CompanyCode", "ABC", (string)row["CompanyCode"]);
					AssertEquals("LD_PK", db1.PK.ToGuid(), (Guid)row["LD_PK"]);
				});
			}

			using (var table = new DataTable())
			using (var cmd = Db.Connection.Command(query))
			{
				cmd.AddParameter("@Org", SqlDbType.UniqueIdentifier, company2.LC_OH.ToGuid());

				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(table);
				}

				AssertEquals(1, table.Rows.Count);

				CombineAssertions(() =>
				{
					var row = table.Rows[0];
					AssertEquals("OH_PK", company2.LC_OH.ToGuid(), (Guid)row["OH_PK"]);
					AssertEquals("OH_Code", company2.Header.OH_Code, (string)row["OH_Code"]);
					AssertEquals("U1_Code", "DUM", (string)row["U1_Code"]);
					AssertEquals("ServerCode", "SYD", (string)row["ServerCode"]);
					AssertEquals("CompanyCode", "DEF", (string)row["CompanyCode"]);
					AssertEquals("LD_PK", db2.PK.ToGuid(), (Guid)row["LD_PK"]);
				});
			}
		}

		public void TestGetBorderWiseUsage_Pre201807()
		{
			EServicesBillingTestHelper.CreateTable();

			var periodStart = new ZDateTime(2018, 6, 1);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD", false);
			var org1 = lic1.Company.Header;

			var contact1 = (EDIOrgContact)org1.Contacts[0];

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(CreateBorderWiseUsage(lic1, contact1, "BOR", periodStart, "Machine 1a", "Global", ""));
			EServicesBillingTestHelper.AddTransactions(infoList);

			Factory.Save();

			var sql = "select * from EdiGetChargeableUsage" + BillingConstants.BillingSystem.BorderWise + "(201806, '2018-06-01 00:00', '2018-07-01 00:00')";

			using (var cmd = Db.Connection.Command(sql))
			{
				var table = new DataTable();
				cmd.NewDataAdapter().Fill(table);

				AssertEquals("usage before 201807 is not calculated", 0, table.Rows.Count);
			}
		}

		public void TestGetBorderWiseUsage_201807_Editions()
		{
			EServicesBillingTestHelper.CreateTable();

			var periodStart = new ZDateTime(2018, 7, 1);
			var lic1 = CreateBorderWiseLicence(1, periodStart, "FTA", hasCW: false, hasCWBillTo: false);
			var org1 = lic1.Company.Header;
			var contactGlobal = AddContact(org1, "User1", "user1@test.com");
			var contactAUandNZ = AddContact(org1, "User3", "user3@test.com");
			var contactAUandAUpro = AddContact(org1, "User4", "user4@test.com");
			var contactNZandGlobal = AddContact(org1, "User5", "user5@test.com");
			var contactNZPro = AddContact(org1, "User6", "user6@test.com");
			var contactNZProAndGlobal = AddContact(org1, "User7", "user7@test.com");

			/// Global x 3
			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(CreateBorderWiseUsage(lic1, contactGlobal, "BOR", periodStart, "Machine 1", "Global", ""));
			infoList.Add(CreateBorderWiseUsage(lic1, contactNZProAndGlobal, "BOR", periodStart, "Machine 1", "Global", ""));
			infoList.Add(CreateBorderWiseUsage(lic1, contactNZandGlobal, "BOR", periodStart, "Machine 1", "Global", ""));

			// Pro x 2
			infoList.Add(CreateBorderWiseUsage(lic1, contactAUandAUpro, "AU3", periodStart, "Machine 1", "Single AU Pro", "AU"));
			infoList.Add(CreateBorderWiseUsage(lic1, contactNZPro, "NZ3", periodStart, "Machine 1", "Single NZ Pro", "NZ"));

			// Single Window x 1
			infoList.Add(CreateBorderWiseUsage(lic1, contactAUandNZ, "AUS", periodStart, "Machine 1", "Single AU", "AU"));

			EServicesBillingTestHelper.AddTransactions(infoList);
			Factory.Save();

			var sql = "select * from EdiGetChargeableUsage" + BillingConstants.BillingSystem.BorderWise + "(201807, '2018-07-01 00:00', '2018-08-01 00:00')";

			using (var cmd = Db.Connection.Command(sql))
			{
				var table = new DataTable();
				cmd.NewDataAdapter().Fill(table);

				var rows = table.Rows.Cast<DataRow>();
				AssertEquals("usage", "AU3,1 | AUP,2 | BW3,3 | BWX,3 | NZ3,1 | NZP,1", RowsToString(lic1, rows));
			}
		}

		public void TestGetBorderWiseUsage_201807_CustomerTypes_AU()
		{
			EServicesBillingTestHelper.CreateTable();

			var periodStart = new ZDateTime(2018, 7, 1);
			var lic1 = CreateBorderWiseLicence(1, periodStart, null, hasCW: false, hasCWBillTo: false);
			var lic2FtaNoCw = CreateBorderWiseLicence(2, periodStart, "FTA", hasCW: false, hasCWBillTo: false);
			var lic3FtaCw = CreateBorderWiseLicence(3, periodStart, "FTA", hasCW: true, hasCWBillTo: false);
			var lic4NoTradeCw = CreateBorderWiseLicence(4, periodStart, null, hasCW: true, hasCWBillTo: false);
			var lic5NoTradeCwShared = CreateBorderWiseLicence(5, periodStart, null, hasCW: false, hasCWBillTo: true);
			var lic6CbaffCw = CreateBorderWiseLicence(6, periodStart, "CBAFF", hasCW: true, hasCWBillTo: false);
			var lic7IfcbaaNoCw = CreateBorderWiseLicence(7, periodStart, "IFCBAA", hasCW: false, hasCWBillTo: false);

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.AddRange(CreateBorderWiseUsersX5(1, lic1, periodStart, "", "AU3", "Single Window AU Pro", "AU"));
			infoList.AddRange(CreateBorderWiseUsersX5(2, lic2FtaNoCw, periodStart, "AU", "AU3", "Single Window AU Pro", "AU"));
			infoList.AddRange(CreateBorderWiseUsersX5(3, lic3FtaCw, periodStart, "AU", "AU3", "Single Window AU Pro", "AU"));
			infoList.AddRange(CreateBorderWiseUsersX5(4, lic4NoTradeCw, periodStart, "", "AU3", "Single Window AU Pro", "AU"));
			infoList.AddRange(CreateBorderWiseUsersX5(5, lic5NoTradeCwShared, periodStart, "", "AU3", "Single Window AU Pro", "AU"));
			infoList.AddRange(CreateBorderWiseUsersX5(6, lic6CbaffCw, periodStart, "NZ", "AU3", "Single Window AU Pro", "AU"));
			infoList.AddRange(CreateBorderWiseUsersX5(7, lic7IfcbaaNoCw, periodStart, "AU", "AU3", "Single Window AU Pro", "AU"));
			EServicesBillingTestHelper.AddTransactions(infoList);
			Factory.Save();

			var sql = "select * from EdiGetChargeableUsage" + BillingConstants.BillingSystem.BorderWise + "(201807, '2018-07-01 00:00', '2018-08-01 00:00')";

			using (var cmd = Db.Connection.Command(sql))
			{
				var table = new DataTable();
				cmd.NewDataAdapter().Fill(table);

				var rows = table.Rows.Cast<DataRow>();

				CombineAssertions(() =>
				{
					AssertEquals("1", "AU3,5 | AUS,5", RowsToString(lic1, rows));
					AssertEquals("2", "AU3,5 | AUP,2 | AUS,3", RowsToString(lic2FtaNoCw, rows));
					AssertEquals("3", "AU3,5 | AUW,3 | AUX,2", RowsToString(lic3FtaCw, rows));
					AssertEquals("4", "AU3,5 | AUW,5", RowsToString(lic4NoTradeCw, rows));
					AssertEquals("5", "AU3,5 | AUW,5", RowsToString(lic5NoTradeCwShared, rows));
					AssertEquals("6", "AU3,5 | AUW,3 | AUX,2", RowsToString(lic6CbaffCw, rows));
					AssertEquals("7", "AU3,5 | AUP,2 | AUS,3", RowsToString(lic7IfcbaaNoCw, rows));

					AssertEquals(18, table.Rows.Count);
				});
			}
		}

		public void TestGetBorderWiseUsage_201807_CustomerTypes_Global()
		{
			EServicesBillingTestHelper.CreateTable();
			var periodStart = new ZDateTime(2018, 7, 1);
			var lic1 = CreateBorderWiseLicence(1, periodStart, null, hasCW: false, hasCWBillTo: false);
			var lic2FtaNoCw = CreateBorderWiseLicence(2, periodStart, "FTA", hasCW: false, hasCWBillTo: false);
			var lic3FtaCw = CreateBorderWiseLicence(3, periodStart, "FTA", hasCW: true, hasCWBillTo: false);
			var lic4NoTradeCw = CreateBorderWiseLicence(4, periodStart, null, hasCW: true, hasCWBillTo: false);
			var lic5NoTradeCwShared = CreateBorderWiseLicence(5, periodStart, null, hasCW: false, hasCWBillTo: true);
			var lic6CbaffCw = CreateBorderWiseLicence(6, periodStart, "CBAFF", hasCW: true, hasCWBillTo: false);

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.AddRange(CreateBorderWiseUsersX5(1, lic1, periodStart, "", "BW3", "Single Window Global Pro", ""));
			infoList.AddRange(CreateBorderWiseUsersX5(2, lic2FtaNoCw, periodStart, "AU", "BW3", "Single Window Global Pro", ""));
			infoList.AddRange(CreateBorderWiseUsersX5(3, lic3FtaCw, periodStart, "AU", "BW3", "Single Window Global Pro", ""));
			infoList.AddRange(CreateBorderWiseUsersX5(4, lic4NoTradeCw, periodStart, "", "BW3", "Single Window Global Pro", ""));
			infoList.AddRange(CreateBorderWiseUsersX5(5, lic5NoTradeCwShared, periodStart, "", "BW3", "Single Window Global Pro", ""));
			infoList.AddRange(CreateBorderWiseUsersX5(6, lic6CbaffCw, periodStart, "NZ", "BW3", "Single Window Global Pro", ""));
			EServicesBillingTestHelper.AddTransactions(infoList);
			Factory.Save();

			var sql = "select * from EdiGetChargeableUsage" + BillingConstants.BillingSystem.BorderWise + "(201807, '2018-07-01 00:00', '2018-08-01 00:00')";

			using (var cmd = Db.Connection.Command(sql))
			{
				var table = new DataTable();
				cmd.NewDataAdapter().Fill(table);

				var rows = table.Rows.Cast<DataRow>();

				CombineAssertions(() =>
				{
					AssertEquals("1", "BW3,5 | BWS,5", RowsToString(lic1, rows));
					AssertEquals("2", "BW3,5 | BWS,3 | BWX,2", RowsToString(lic2FtaNoCw, rows));
					AssertEquals("3", "BW3,5 | BWX,5", RowsToString(lic3FtaCw, rows));
					AssertEquals("4", "BW3,5 | BWX,5", RowsToString(lic4NoTradeCw, rows));
					AssertEquals("5", "BW3,5 | BWX,5", RowsToString(lic5NoTradeCwShared, rows));
					AssertEquals("6", "BW3,5 | BWX,5", RowsToString(lic6CbaffCw, rows));

					AssertEquals(13, table.Rows.Count);
				});
			}
		}

		string RowsToString(LicenceHeader lic, IEnumerable<DataRow> rows)
		{
			return string.Join(" | ", rows.Where(x => (Guid)x["LC_PK"] == lic.LA_LC.ToGuid())
				.OrderBy(x => x["SubCode"].ToString())
				.Select(x => x["SubCode"].ToString() + "," + x["UnitCount"].ToString()));
		}

		LicenceHeader CreateBorderWiseLicence(int index, ZDateTime periodStart, string tradeMembershipType, bool hasCW, bool hasCWBillTo)
		{
			var suffix = index.ToString("0#");
			var bwLic = BillingTestHelper.CreateLicence(Factory, "E" + suffix, "C" + suffix, "S" + suffix);
			bwLic.Database.LD_Product = ProductTypes.Codes.BorderWise;
			var org = bwLic.Company.Header;

			if (!string.IsNullOrEmpty(tradeMembershipType))
			{
				var orgMembership = org.Memberships.AddNew();
				orgMembership.EOR_MembershipType = tradeMembershipType;
				orgMembership.EOR_ValidFrom = periodStart.Date;
				orgMembership.EOR_ValidTo = periodStart.Date.AddYears(5);

				var orgMembershipDupe = org.Memberships.AddNew();
				orgMembershipDupe.EOR_MembershipType = tradeMembershipType;
				orgMembershipDupe.EOR_ValidFrom = periodStart.Date.AddMonths(-1);
				orgMembershipDupe.EOR_ValidTo = periodStart.Date.AddYears(5);
			}
			else
			{
				var orgFtaExpired = org.Memberships.AddNew();
				orgFtaExpired.EOR_MembershipType = "FTA";
				orgFtaExpired.EOR_ValidFrom = periodStart.Date.AddMonths(-1);
				orgFtaExpired.EOR_ValidTo = periodStart.Date.AddMonths(-1).AddDays(-1);
			}

			if (hasCW)
			{
				var licCW = BillingTestHelper.CreateAnotherDatabase(bwLic, "CW1", false);
				licCW.LA_AgreedLiveDate = periodStart;
			}

			if (hasCWBillTo)
			{
				var cwLic = BillingTestHelper.CreateLicence(Factory, "E" + suffix, "D" + suffix, "CW1", false);
				var cwPayer = BillingTestHelper.CreateLicenceCompany(Factory, "E" + suffix, "P" + suffix);
				BillingTestHelper.SetInvoicingTo(cwLic, cwPayer);

				/// The BW db is billed to an org that also pays for CW1
				BillingTestHelper.SetInvoicingTo(bwLic, cwPayer);
			}

			return bwLic;
		}

		List<EServicesBillingTestHelper.RawUsageInfo> CreateBorderWiseUsersX5(int index, LicenceHeader lic, ZDateTime periodStart, string nationality, string priceCode, string editionName, string editionCountry)
		{
			var result = new List<EServicesBillingTestHelper.RawUsageInfo>();
			var prevPeriodStart = periodStart.AddMonths(-1);
			var nextPeriodStart = periodStart.AddMonths(1);

			var org = lic.Company.Header;

			var suffix = index.ToString("0#");
			{
				var exStudent = AddContact(org, "User 2", "user2@org" + suffix + ".com");
				exStudent.OC_RN_NKNationality = "AE";
				var expiredStudentCert = exStudent.Certificates.AddNew();
				expiredStudentCert.XZ_Comment = "Student - CBFCA";
				expiredStudentCert.XZ_Type = "MSC";
				expiredStudentCert.XZ_ExpiryOrDueDate = periodStart.AddHours(-1);
				result.Add(CreateBorderWiseUsage(lic, exStudent, priceCode, periodStart.AddMinutes(5), "Machine 1a", editionName, editionCountry));
			}

			{
				// Trade member
				var contact1 = AddContact(org, "User 3", "user3@org" + suffix + ".com");
				contact1.OC_RN_NKNationality = nationality;
				result.Add(CreateBorderWiseUsage(lic, contact1, priceCode, periodStart.AddDays(5), "Machine 1b", editionName, editionCountry));

				// Usage not in the month
				result.Add(CreateBorderWiseUsage(lic, contact1, priceCode, prevPeriodStart, "Machine 1b", editionName, editionCountry));
				result.Add(CreateBorderWiseUsage(lic, contact1, priceCode, nextPeriodStart, "Machine 1b", editionName, editionCountry));
			}

			{
				// Trade member
				var contact2 = AddContact(org, "User 4", "user4@org" + suffix + ".com");
				contact2.OC_RN_NKNationality = nationality;
				result.Add(CreateBorderWiseUsage(lic, contact2, priceCode, periodStart.AddMinutes(5), "Machine 1a", editionName, editionCountry));
			}

			{
				// Not trade member
				var contact5 = AddContact(org, "User 5", "user5@org" + suffix + ".com");
				contact5.OC_RN_NKNationality = "AE";
				result.Add(CreateBorderWiseUsage(lic, contact5, priceCode, periodStart.AddDays(5), "Machine 1a", editionName, editionCountry));
			}

			{
				// Not trade member
				var contact6 = AddContact(org, "User 6", "user6@org" + suffix + ".com");
				contact6.OC_RN_NKNationality = "AE";
				result.Add(CreateBorderWiseUsage(lic, contact6, priceCode, periodStart.AddMinutes(5), "Machine 1a", editionName, editionCountry));
			}

			return result;
		}

		public void TestGetBorderWiseUsage_Student()
		{
			EServicesBillingTestHelper.CreateTable();
			var periodStart = new ZDateTime(2018, 7, 1);

			var lic1 = CreateBorderWiseLicence(1, periodStart, "", hasCW: false, hasCWBillTo: false);
			var org1 = lic1.Company.Header;

			var contact1 = (EDIOrgContact)org1.Contacts[0];
			var student1 = (EDIOrgContact)org1.Contacts.AddNew();
			student1.OC_Email = "student1@test.com";
			student1.OC_ContactName = "Student One";

			var studentCert = student1.Certificates.AddNew();
			studentCert.XZ_Comment = "Student - CBFCA";
			studentCert.XZ_Type = "MSC";
			studentCert.XZ_IssueDate = ZDateTime.Empty;
			var expiredStudentCert = contact1.Certificates.AddNew();
			expiredStudentCert.XZ_Comment = "Student - CBFCA";
			expiredStudentCert.XZ_Type = "MSC";
			expiredStudentCert.XZ_ExpiryOrDueDate = periodStart.AddHours(-1);

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(CreateBorderWiseUsage(lic1, contact1, "AUS", periodStart, "Machine 1a", "Single Window AU", "AU"));
			infoList.Add(CreateBorderWiseUsage(lic1, student1, "AUS", periodStart, "Student Machine 1a", "Single Window AU", "AU"));
			EServicesBillingTestHelper.AddTransactions(infoList);

			Factory.Save();

			var sql = "select * from EdiGetChargeableUsage" + BillingConstants.BillingSystem.BorderWise + "(201807, '2018-07-01 00:00', '2018-08-01 00:00')";

			using (var cmd = Db.Connection.Command(sql))
			{
				var table = new DataTable();
				cmd.NewDataAdapter().Fill(table);

				System.Text.StringBuilder s = new System.Text.StringBuilder();
				foreach (DataRow row in table.Rows)
				{
					s.AppendLine("SubCode=" + row["SubCode"].ToString() + ", LC_PK=" + row["LC_PK"].ToString() + ", Units=" + row["UnitCount"].ToString());
				}

				AssertEquals(s.ToString(), 2, table.Rows.Count);
				var rows = table.Rows.Cast<DataRow>();

				CombineAssertions(() =>
				{
					var rowOrg1a = rows.First(x => (Guid)x["LC_PK"] == lic1.LA_LC.ToGuid() && (string)x["SubCode"] != BillingConstants.BorderWise.StudentUserPriceCode);

					var rowStudent1 = rows.First(x => (Guid)x["LC_PK"] == lic1.LA_LC.ToGuid() && (string)x["SubCode"] == BillingConstants.BorderWise.StudentUserPriceCode);

					AssertEquals("Org 1 users", 1, (int)rowOrg1a["UnitCount"]);
					AssertEquals("Org 1 student users", 1, (int)rowStudent1["UnitCount"]);
				});
			}
		}

		public void TestGetBorderWiseUsage_FreeTrialWithIssueDate()
		{
			EServicesBillingTestHelper.CreateTable();
			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			var periodStart = new ZDateTime(2018, 7, 1);
			var lic1 = CreateBorderWiseLicence(1, periodStart, "", hasCW: false, hasCWBillTo: false);
			var org1 = lic1.Company.Header;
			var licCompany1 = lic1.Company;
			{
				var contact1 = AddContact(org1, "User1", "user1@test.com");
				var trial1 = contact1.Certificates.AddNew();
				trial1.XZ_Comment = "BorderWise Promo Free Trial";
				trial1.XZ_IssueDate = new ZDateTime(2018, 7, 29);
				trial1.XZ_ExpiryOrDueDate = new ZDateTime(2018, 8, 31);
				trial1.XZ_Type = "MSC";

				var trial1b = contact1.Certificates.AddNew();
				trial1b.XZ_Comment = "BorderWise Country Free Trial";
				trial1b.XZ_IssueDate = new ZDateTime(2018, 7, 1);
				trial1b.XZ_ExpiryOrDueDate = new ZDateTime(2018, 7, 31);
				trial1b.XZ_Type = "MSC";

				infoList.Add(CreateBorderWiseUsage(lic1, contact1, "AUS", trial1.XZ_IssueDate, "Machine 1", "Single Window AU", "AU"));
			}
			{
				var contact2 = AddContact(org1, "User2", "user2@test.com");
				var trial2 = contact2.Certificates.AddNew();
				trial2.XZ_Comment = "BorderWise Promo Free Trial";
				trial2.XZ_IssueDate = new ZDateTime(2018, 7, 29);
				trial2.XZ_Type = "MSC";

				infoList.Add(CreateBorderWiseUsage(lic1, contact2, "AUS", trial2.XZ_IssueDate.AddDays(-1), "Machine 1", "Single Window AU", "AU"));
			}
			{
				var contact3 = AddContact(org1, "User3", "user3@test.com");
				var trial3 = contact3.Certificates.AddNew();
				trial3.XZ_Comment = "BorderWise Promo Free Trial";
				trial3.XZ_IssueDate = new ZDateTime(2018, 7, 29);
				trial3.XZ_Type = "MSC";

				infoList.Add(CreateBorderWiseUsage(lic1, contact3, "AUS", trial3.XZ_IssueDate.AddDays(-1), "Machine 3", "Single Window AU", "AU"));
			}
			{
				var contact4 = AddContact(org1, "User4", "user4@test.com");
				var trial4 = contact4.Certificates.AddNew();
				trial4.XZ_Comment = "BorderWise Country Free Trial";
				trial4.XZ_IssueDate = new ZDateTime(2018, 7, 1);
				trial4.XZ_ExpiryOrDueDate = new ZDateTime(2018, 7, 31);
				trial4.XZ_Type = "MSC";

				var trialPrev = contact4.Certificates.AddNew();
				trialPrev.XZ_Comment = "BorderWise Country Free Trial";
				trialPrev.XZ_IssueDate = new ZDateTime(2018, 6, 1);
				trialPrev.XZ_ExpiryOrDueDate = new ZDateTime(2018, 6, 30);
				trialPrev.XZ_Type = "MSC";

				infoList.Add(CreateBorderWiseUsage(lic1, contact4, "AUS", periodStart, "Machine 4", "Single Window AU", "AU"));
			}

			EServicesBillingTestHelper.AddTransactions(infoList);
			Factory.Save();

			var sql = "select * from EdiGetChargeableUsage" + BillingConstants.BillingSystem.BorderWise + "(201807, '2018-07-01 00:00', '2018-08-01 00:00')";

			using (var cmd = Db.Connection.Command(sql))
			{
				var table = new DataTable();
				cmd.NewDataAdapter().Fill(table);

				var rows = table.Rows.Cast<DataRow>();
				AssertEquals("usage", "AUS,2 | BF1,2", RowsToString(lic1, rows));
			}
		}

		static EDIOrgContact AddContact(OrgHeader org, string name, string email, string nationality = null)
		{
			var contact = (EDIOrgContact)org.Contacts.AddNew();
			contact.OC_Email = email;
			contact.OC_ContactName = name;
			if (nationality != null)
			{
				contact.OC_RN_NKNationality = nationality;
			}
			return contact;
		}

		public void TestGetAirlineUsage()
		{
			var stdPriceOrg = Factory.New<EDIOrgHeader>();
			stdPriceOrg.OH_Code = "SOMEORG";
			stdPriceOrg.CreateAndLoadLicenceForOrg();

			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			var db = lic.Database;
			var clientCompany = lic.ClientCompany;
			var stdPriceCompany = lic.Company;
			var stdPrices = stdPriceCompany.PriceHeaders.AddNew();

			Factory.Save();

			EServicesBillingTestHelper.CreateTable();
			var clientNumber = lic.Database.DatabaseId + ".COM";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			// 2 FWB/FHL for every -WB/-HL so they don't cancel out
			foreach (var priceCode in new[] { "FWB", "FWB", "-WB", "FHL", "FHL", "-HL", "FSU", "###" })
			{
				foreach (var ref4 in new[] { "BT", "BT_WithUnsupported", "Delta", "CCSJ", "CCN", "Descartes", "Descartes_WithUnsupported", "GLSHK", "GLSHK_WithUnsupported", "Traxon", "Traxon_WithUnsupported", "TraxonEDP", "TraxonRCF", "###", "Nallian", "ARINC", "Qatar", "Cargonaut", "CargoStart", "Tradevan", "PakFresh" })
				{
					if (new[] { "Traxon", "Traxon_WithUnsupported", "TraxonEDP", "TraxonRCF" }.Contains(ref4))
					{
						foreach (var ref3 in new[] { "114", "098" })
						{
							infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("AMG", priceCode, new ZDateTime(2018, 8, 5), "ENTCOMSRV", clientNumber, db.DatabaseId, clientCompany.PK, "ref1", "ref2", ref3, ref4, null, "AMG", 1));
						}
					}
					else
					{
						infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("AMG", priceCode, new ZDateTime(2018, 8, 5), "ENTCOMSRV", clientNumber, db.DatabaseId, clientCompany.PK, "ref1", "ref2", "ref3", ref4, null, "AMG", 1));
					}
				}
			}

			EServicesBillingTestHelper.AddTransactions(infoList);

			var table = GetChargeableUsage(BillingConstants.BillingSystem.AirlineMessaging, 201808, +10, +10);
			var rowsAsText = string.Join("\r\n", table.Rows.OfType<DataRow>().Select(r => $"{r["SubCode"]} {r["UnitCount"]}").OrderBy(x => x));
			AssertEquals(
@"H1C 4
H2C 2
H3C 2
H4C 2
H5C 4
H6C 4
HAC 8
HAP 8
HEC 4
HEP 4
HFC 2
HGC 2
HIC 2
HLC 2
HQC 2
HRC 4
HRP 4
HTC 2
HUC 2
HVC 2
HXC 8
HXP 8
S1C 2
S2C 1
S3C 1
S4C 1
S5C 2
S6C 2
SAC 4
SAP 4
SFC 1
SGC 1
SIC 1
SLC 1
SQC 1
STC 1
SUC 1
SVC 1
SXC 4
SXP 4
W1C 4
W2C 2
W3C 2
W4C 2
W5C 4
W6C 4
WAC 8
WAP 8
WEC 4
WEP 4
WFC 2
WGC 2
WIC 2
WLC 2
WQC 2
WRC 4
WRP 4
WTC 2
WUC 2
WVC 2
WXC 8
WXP 8", rowsAsText);
		}

		public void TestGetStlUsageSummary_FeeType_UCB()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var licence = org1.LicCompany.LicHeadersForAllDatabases[0];
			licence.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = licence.Database;
			db1.LD_Product = "CW1";
			var prices = BillingTestHelper.CreatePriceList(licence);
			prices.L6_SystemCode = "STL";

			var priceItemUSR = BillingTestHelper.AddPriceItem(prices, "USR", BillingConstants.FeeType.Transactional, "", 10m);
			priceItemUSR.L7_Description = "USR";
			priceItemUSR.L7_UnitBreak = 0;

			var priceItemGPC1 = BillingTestHelper.AddPriceItem(prices, "GPC", BillingConstants.FeeType.UsersPerCountryVolumeBreak, "", 5.11m);
			priceItemGPC1.L7_Category = BillingConstants.BillingSystem.ODM;
			priceItemGPC1.L7_Description = "GPC(1)";
			priceItemGPC1.L7_UnitBreak = 1;

			var priceItemGPC2 = BillingTestHelper.AddPriceItem(prices, "GPC", BillingConstants.FeeType.UsersPerCountryVolumeBreak, "", 10.22m);
			priceItemGPC2.L7_Category = BillingConstants.BillingSystem.ODM;
			priceItemGPC2.L7_Description = "GPC(2)";
			priceItemGPC2.L7_UnitBreak = 2;

			var priceItemGPC3 = BillingTestHelper.AddPriceItem(prices, "GPC", BillingConstants.FeeType.UsersPerCountryVolumeBreak, "", 15.33m);
			priceItemGPC3.L7_Category = BillingConstants.BillingSystem.ODM;
			priceItemGPC3.L7_Description = "GPC(3)";
			priceItemGPC3.L7_UnitBreak = 3;

			BillingTestHelper.CreatePriceLink(db1, prices, new ZDateTime(2010, 1, 1));

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", new ZDateTime(2017, 1, 1), licence.ClientCompany, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "GPC", new ZDateTime(2017, 1, 1), licence.ClientCompany, 1);

			Factory.Save();

			var rowsAsText = string.Join("\r\n", Utilities.GetDataTableFromQuery($"select * from EdiGetStlUsageSummary('{org1.PK}', '2017-1-1')").Rows.OfType<DataRow>()
				.Select(r => $"{r["LD_ServerCode"]}-{r["LE_EnterpriseCode"]}-{r["L7_Description"]}-{r["LCC_Code"]}-{r["U1_Code"]}-{r["U1_UnitCount"]}").OrderBy(x => x));

			AssertEquals(@"SYD-DDD-GPC(2)-ABC-ODM-1
SYD-DDD-USR-ABC-STL-3", rowsAsText);
		}

		public void TestGetStlUsageSummary_FeeType_TRB()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var licence = org1.LicCompany.LicHeadersForAllDatabases[0];
			licence.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = licence.Database;
			db1.LD_Product = "CW1";

			Factory.Save();

			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "CC1", db1.PK, ZGuid.Empty, "", "");
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "CC2", db1.PK, ZGuid.Empty, "", "");

			var prices = BillingTestHelper.CreatePriceList(licence);
			prices.L6_SystemCode = "STL";

			var priceItemUSR = BillingTestHelper.AddPriceItem(prices, "USR", BillingConstants.FeeType.Transactional, "", 10m);
			priceItemUSR.L7_Description = "USR";
			priceItemUSR.L7_UnitBreak = 0;

			var priceItemGPC1 = BillingTestHelper.AddPriceItem(prices, "P01", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 1m);
			priceItemGPC1.L7_Description = "TRB > 1";
			priceItemGPC1.L7_UnitBreak = 1;

			var priceItemGPC2 = BillingTestHelper.AddPriceItem(prices, "P01", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 5m);
			priceItemGPC2.L7_Description = "TRB > 5";
			priceItemGPC2.L7_UnitBreak = 5;

			var priceItemGPC3 = BillingTestHelper.AddPriceItem(prices, "P01", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 10m);
			priceItemGPC3.L7_Description = "TRB > 10";
			priceItemGPC3.L7_UnitBreak = 10;

			BillingTestHelper.CreatePriceLink(db1, prices, new ZDateTime(2010, 1, 1));

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2017, 1, 1), licence.ClientCompany, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "P01", new ZDateTime(2017, 1, 1), clientCompany1, 2);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "P01", new ZDateTime(2017, 1, 1), clientCompany2, 4);

			Factory.Save();

			AssertNotEquals(clientCompany1.PK, clientCompany2.PK);
			var rowsAsText = string.Join("\r\n", Utilities.GetDataTableFromQuery($"select * from EdiGetStlUsageSummary('{org1.PK}', '2017-1-1')").Rows.OfType<DataRow>()
				.Select(r => $"{r["LD_ServerCode"]}-{r["LE_EnterpriseCode"]}-{r["L7_Description"]}-{r["LCC_Code"]}-{r["U1_Code"]}-{r["U1_UnitCount"]}").OrderBy(x => x));

			AssertEquals(@"SYD-DDD-TRB > 5--STL-6
SYD-DDD-USR-ABC-STL-3", rowsAsText);
		}

		public void TestGetStlUsageSummary_FeeType_TRB_UsageMapping()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var licence = org1.LicCompany.LicHeadersForAllDatabases[0];
			licence.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = licence.Database;
			db1.LD_Product = "CW1";

			Factory.Save();

			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "CC1", db1.PK, ZGuid.Empty, "", "");
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "CC2", db1.PK, ZGuid.Empty, "", "");

			var prices = BillingTestHelper.CreatePriceList(licence);
			prices.L6_SystemCode = "STL";

			var priceItemUSR = BillingTestHelper.AddPriceItem(prices, "USR", BillingConstants.FeeType.Transactional, "", 10m);
			priceItemUSR.L7_Description = "USR";
			priceItemUSR.L7_UnitBreak = 0;

			var priceItemGPC1 = BillingTestHelper.AddPriceItem(prices, "P01", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 1m);
			priceItemGPC1.L7_Description = "TRB > 1";
			priceItemGPC1.L7_UnitBreak = 1;

			var priceItemGPC2 = BillingTestHelper.AddPriceItem(prices, "P01", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 5m);
			priceItemGPC2.L7_Description = "TRB > 5";
			priceItemGPC2.L7_UnitBreak = 5;

			var priceItemGPC3 = BillingTestHelper.AddPriceItem(prices, "P01", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 10m);
			priceItemGPC3.L7_Description = "TRB > 10";
			priceItemGPC3.L7_UnitBreak = 10;

			BillingTestHelper.AddUsageMap(prices, "STL", "P01", "XYZ");
			BillingTestHelper.CreatePriceLink(db1, prices, new ZDateTime(2010, 1, 1));

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2017, 1, 1), licence.ClientCompany, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "P01", new ZDateTime(2017, 1, 1), clientCompany1, 2);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "P01", new ZDateTime(2017, 1, 1), clientCompany2, 4);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "XYZ", new ZDateTime(2017, 1, 1), clientCompany2, 8);

			Factory.Save();

			AssertNotEquals(clientCompany1.PK, clientCompany2.PK);
			var rowsAsText = string.Join("\r\n", Utilities.GetDataTableFromQuery($"select * from EdiGetStlUsageSummary('{org1.PK}', '2017-1-1')").Rows.OfType<DataRow>()
				.Select(r => $"{r["LD_ServerCode"]}-{r["LE_EnterpriseCode"]}-{r["L7_Description"]}-{r["LCC_Code"]}-{r["U1_Code"]}-{r["U1_UnitCount"]}").OrderBy(x => x));

			AssertEquals(@"SYD-DDD-TRB > 10--STL-14
SYD-DDD-USR-ABC-STL-3", rowsAsText);
		}

		public void TestGetStlUsageSummary_UsageMappingDifferentCategory()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var licence = org1.LicCompany.LicHeadersForAllDatabases[0];
			licence.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = licence.Database;
			db1.LD_Product = "CW1";
			Factory.Save();

			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "CC1", db1.PK, ZGuid.Empty, "", "");
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "CC2", db1.PK, ZGuid.Empty, "", "");

			var prices = BillingTestHelper.CreatePriceList(licence);
			prices.L6_SystemCode = "STL";

			var priceItemUSR = BillingTestHelper.AddPriceItem(prices, "USR", BillingConstants.FeeType.Transactional, "", 10m);
			priceItemUSR.L7_Description = "USR";
			priceItemUSR.L7_UnitBreak = 0;

			var priceItemBYO = BillingTestHelper.AddPriceItem(prices, "BYO", BillingConstants.FeeType.Transactional, "", 1m);
			priceItemBYO.L7_Description = "BYO";
			priceItemBYO.L7_UnitBreak = 0;
			priceItemBYO.L7_Category = "SVC";

			var mapping = BillingTestHelper.AddUsageMap(prices, "STL", "BYO", "XYZ");
			mapping.PUM_PriceCategory = "SVC";

			BillingTestHelper.CreatePriceLink(db1, prices, new ZDateTime(2010, 1, 1));

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "XYZ", new ZDateTime(2017, 1, 1), clientCompany1, 4);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "XYZ", new ZDateTime(2017, 1, 1), clientCompany2, 8);

			Factory.Save();

			AssertNotEquals(clientCompany1.PK, clientCompany2.PK);
			var rowsAsText = string.Join("\r\n", Utilities.GetDataTableFromQuery($"select * from EdiGetStlUsageSummary('{org1.PK}', '2017-1-1')").Rows.OfType<DataRow>()
				.Select(r => $"{r["LD_ServerCode"]}-{r["LE_EnterpriseCode"]}-{r["L7_Description"]}-{r["LCC_Code"]}-{r["U1_Code"]}-{r["U1_UnitCount"]}").OrderBy(x => x));

			AssertEquals(@"SYD-DDD-BYO-CC1-STL-4
SYD-DDD-BYO-CC2-STL-8", rowsAsText);
		}

		public void TestGetStlUsageSummary_GoldenTax()
		{
			var billingCodes = new BillingDbUsageCodesCollection();
			var usageCodes1 = billingCodes.AddNew();
			usageCodes1.Category = "ACC";
			usageCodes1.PriceItemCode = "GTS";
			usageCodes1.PriceHeaderCode = BillingConstants.PriceHeaderType.GoldenTax;
			usageCodes1.KeyRefIndex1 = 3;
			EDIDataRegistry.Instance.BillingDbUsageCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, billingCodes);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = lic1.Database;
			db1.LD_Product = "CW1";

			var stlPrices = BillingTestHelper.CreateStlPriceList(lic1.Company, "USR");
			stlPrices.L6_ValidFrom = new ZDateTime(2019, 1, 1);
			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices, new ZDateTime(2019, 1, 1));

			var gts = BillingTestHelper.CreateGoldenTaxPriceList(lic1.Company);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2019, 1, 1), lic1.ClientCompany, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, "ACC", "GTS", new ZDateTime(2019, 1, 1), lic1.ClientCompany, 1);

			Factory.Save();

			var rowsAsText = string.Join("\r\n", Utilities.GetDataTableFromQuery($"select * from EdiGetStlUsageSummary('{lic1.Company.LC_OH}', '2019-1-1')").Rows.OfType<DataRow>()
				.Select(r => $"{r["LD_ServerCode"]}-{r["LE_EnterpriseCode"]}-{r["L7_Description"]}-{r["LCC_Code"]}-{r["U1_Code"]}-{r["U1_UnitCount"]}").OrderBy(x => x));

			AssertEquals(@"SYD-DDD-Golden Tax Additional Invoices-ABC-ACC-1
SYD-DDD-Item USR-ABC-STL-3", rowsAsText);

			var goldenTaxOnStlPrices = BillingTestHelper.AddPriceItem(stlPrices, new UsageCodeKey("ACC", "GTS"), "TRA", 0.16m);
			goldenTaxOnStlPrices.L7_Description = "  New Per Additional Invoice";
			Factory.Save();

			rowsAsText = string.Join("\r\n", Utilities.GetDataTableFromQuery($"select * from EdiGetStlUsageSummary('{lic1.Company.LC_OH}', '2019-1-1')").Rows.OfType<DataRow>()
				.Select(r => $"{r["LD_ServerCode"]}-{r["LE_EnterpriseCode"]}-{r["L7_Description"]}-{r["LCC_Code"]}-{r["U1_Code"]}-{r["U1_UnitCount"]}").OrderBy(x => x));

			AssertEquals(@"SYD-DDD-  New Per Additional Invoice-ABC-ACC-1
SYD-DDD-Item USR-ABC-STL-3", rowsAsText);
		}

		public void TestGetStlUsageSummary_FlightStats()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var licence = org1.LicCompany.LicHeadersForAllDatabases[0];
			licence.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = licence.Database;
			db1.LD_Product = "CW1";
			var prices = BillingTestHelper.CreatePriceList(licence);
			prices.L6_SystemCode = "STL";

			var fms = org1.LicCompany.PriceHeaders.AddNew();
			fms.L6_SystemCode = BillingConstants.PriceHeaderType.FlightStats;
			fms.L6_UseStandardDiscount = false;
			fms.L6_DiscountCode = "STL1";
			fms.L6_PricelistVersion = "FMS v1";
			fms.L6_RX_NKCurrency = "AUD";
			fms.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var item1 = fms.Items.AddNew();
			item1.L7_Code = "FMS";
			item1.L7_FeeType = BillingConstants.FeeType.Transactional;
			item1.L7_Description = "Air Waybill Automation";
			item1.L7_Price = 200m;
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			item1.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;

			var priceItemUSR = BillingTestHelper.AddPriceItem(prices, "USR", BillingConstants.FeeType.Transactional, "", 10m);
			priceItemUSR.L7_Description = "USR";
			priceItemUSR.L7_UnitBreak = 0;

			BillingTestHelper.CreatePriceLink(db1, prices, new ZDateTime(2010, 1, 1));

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2017, 1, 1), licence.ClientCompany, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, "FMS", "FMS", new ZDateTime(2017, 1, 1), licence.ClientCompany, 1);

			Factory.Save();

			var rowsAsText = string.Join("\r\n", Utilities.GetDataTableFromQuery($"select * from EdiGetStlUsageSummary('{org1.PK}', '2017-1-1')").Rows.OfType<DataRow>()
				.Select(r => $"{r["LD_ServerCode"]}-{r["LE_EnterpriseCode"]}-{r["L7_Description"]}-{r["LCC_Code"]}-{r["U1_Code"]}-{r["U1_UnitCount"]}").OrderBy(x => x));

			AssertEquals(@"SYD-DDD-Air Waybill Automation-ABC-FMS-1
SYD-DDD-USR-ABC-STL-3", rowsAsText);
		}

		public void TestGetStlUsageSummary_ProductivityWise()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var licence = org1.LicCompany.LicHeadersForAllDatabases[0];
			licence.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = licence.Database;
			db1.LD_Product = "PRW";
			var prices = BillingTestHelper.CreatePriceList(licence);
			prices.L6_SystemCode = "STL";

			var priceItemUSR = BillingTestHelper.AddPriceItem(prices, "USR", BillingConstants.FeeType.Transactional, "", 10m);
			priceItemUSR.L7_Description = "USR";
			priceItemUSR.L7_UnitBreak = 0;

			BillingTestHelper.CreatePriceLink(db1, prices, new ZDateTime(2010, 1, 1));
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2017, 1, 1), licence.ClientCompany, 3);
			Factory.Save();

			var rowsAsText = string.Join("\r\n", Utilities.GetDataTableFromQuery($"select * from EdiGetStlUsageSummary('{org1.PK}', '2017-1-1')").Rows.OfType<DataRow>()
				.Select(r => $"{r["LD_ServerCode"]}-{r["LE_EnterpriseCode"]}-{r["L7_Description"]}-{r["LCC_Code"]}-{r["U1_Code"]}-{r["U1_UnitCount"]}").OrderBy(x => x));
			AssertEquals(@"SYD-DDD-USR-ABC-STL-3", rowsAsText);
		}

		public void TestGetStlUsageSummary_UserDefinedCodes()
		{
			var periodStart = new ZDateTime(2019, 1, 1);

			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair("INV", "E-Invoicing");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);

			var billingCodes = new BillingDbUsageCodesCollection();
			var usageCodes1 = billingCodes.AddNew();
			usageCodes1.Category = "ACC";
			usageCodes1.PriceItemCode = "IT1";
			usageCodes1.PriceHeaderCode = "INV";
			EDIDataRegistry.Instance.BillingDbUsageCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, billingCodes);

			var stdLicence = BillingTestHelper.CreateLicence(Factory, "EDI", "EDI", "SYD", false);
			var stdPriceCompany = stdLicence.Company;

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "USR");
			var globalPrices = BillingTestHelper.CreateStlPrices(stdPriceCompany, new UsageCodeKey("ACC", "IT1"));
			globalPrices.L6_SystemCode = "INV";
			globalPrices.L6_ValidFrom = periodStart;

			var oldGlobalPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "IT1");
			oldGlobalPrices.L6_SystemCode = "INV";
			oldGlobalPrices.L6_ValidFrom = periodStart.AddMonths(-12);
			oldGlobalPrices.Items[0].L7_Description = "Old IT1";

			var futureGlobalPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "IT1");
			futureGlobalPrices.L6_SystemCode = "INV";
			futureGlobalPrices.L6_ValidFrom = periodStart.AddMonths(1);
			futureGlobalPrices.Items[0].L7_Description = "Future IT1";

			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = lic1.Database;

			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices, periodStart);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, "ACC", "IT1", periodStart, lic1.ClientCompany, 11);

			// To ingore since not on pricelist...
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHP", periodStart, lic1.ClientCompany, 15);

			// To ignore since not in period...
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart.AddMonths(-1), lic1.ClientCompany, 5);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart.AddMonths(1), lic1.ClientCompany, 7);

			Factory.Save();

			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdLicence.LicenceCode);

			var rowsAsText = string.Join("\r\n", Utilities.GetDataTableFromQuery($"select * from EdiGetStlUsageSummary('{lic1.Company.LC_OH}', '2019-1-1')").Rows.OfType<DataRow>()
				.Select(r => $"{r["LD_ServerCode"]}-{r["LE_EnterpriseCode"]}-{r["L7_Description"]}-{r["LCC_Code"]}-{r["U1_Code"]}-{r["U1_UnitCount"]}").OrderBy(x => x));

			var expected =
@"SYD-DDD-Item IT1-ABC-ACC-11
SYD-DDD-Item USR-ABC-STL-3";

			AssertEquals(expected, rowsAsText);
		}

		public void TestGetStlUsageSummary_UniversalPricelistWithUsageMapping()
		{
			var periodStart = new ZDateTime(2021, 2, 1);

			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair("INV", "E-Invoicing");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);

			var billingCodes = new BillingDbUsageCodesCollection();
			var usageCodes1 = billingCodes.AddNew();
			usageCodes1.Category = "ACC";
			usageCodes1.PriceItemCode = "IT1";
			usageCodes1.PriceHeaderCode = "INV";
			var usageCodes2 = billingCodes.AddNew();
			usageCodes2.Category = "ACC";
			usageCodes2.PriceItemCode = "IN1";
			usageCodes2.PriceHeaderCode = "INV";
			EDIDataRegistry.Instance.BillingDbUsageCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, billingCodes);

			var stdLicence = BillingTestHelper.CreateLicence(Factory, "EDI", "EDI", "SYD", false);
			var stdPriceCompany = stdLicence.Company;

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "USR");
			var globalPrices = BillingTestHelper.CreateStlPrices(stdPriceCompany, new UsageCodeKey("ACC", "IT1"));
			globalPrices.L6_SystemCode = "INV";
			globalPrices.L6_ValidFrom = periodStart;
			BillingTestHelper.AddUsageMap(globalPrices, "ACC", "IT1", "IN1");

			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = lic1.Database;

			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices, periodStart);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, "ACC", "IN1", periodStart, lic1.ClientCompany, 11);

			var period2 = periodStart.AddMonths(1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period2, lic1.ClientCompany, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, "ACC", "IT1", period2, lic1.ClientCompany, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, "ACC", "IN1", period2, lic1.ClientCompany, 11);

			Factory.Save();

			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdLicence.LicenceCode);

			var rowsAsText = string.Join("\r\n", Utilities.GetDataTableFromQuery($"select * from EdiGetStlUsageSummary('{lic1.Company.LC_OH}', '2021-3-1')").Rows.OfType<DataRow>()
				.Select(r => $"{r["LD_ServerCode"]}-{r["LE_EnterpriseCode"]}-{r["L7_Description"]}-{r["LCC_Code"]}-{r["U1_Code"]}-{r["U1_UnitCount"]}").OrderBy(x => x));

			var expected =
@"SYD-DDD-Item IT1-ABC-ACC-18
SYD-DDD-Item USR-ABC-STL-3";

			AssertEquals("mapped usage IN1->IT1 is loaded", expected, rowsAsText);
		}

		public void TestGetStlUsageSummary_UniversalPricelistWithUsageMapping_TRB()
		{
			var periodStart = new ZDateTime(2021, 2, 1);

			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair("INV", "E-Invoicing");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);

			var billingCodes = new BillingDbUsageCodesCollection();
			var usageCodes1 = billingCodes.AddNew();
			usageCodes1.Category = "ACC";
			usageCodes1.PriceItemCode = "IT1";
			usageCodes1.PriceHeaderCode = "INV";
			var usageCodes2 = billingCodes.AddNew();
			usageCodes2.Category = "ACC";
			usageCodes2.PriceItemCode = "IN1";
			usageCodes2.PriceHeaderCode = "INV";
			EDIDataRegistry.Instance.BillingDbUsageCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, billingCodes);

			var stdLicence = BillingTestHelper.CreateLicence(Factory, "EDI", "EDI", "SYD", false);
			var stdPriceCompany = stdLicence.Company;

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "USR");
			var globalPrices = BillingTestHelper.CreateStlPrices(stdPriceCompany, new UsageCodeKey("ACC", "IT1"), new UsageCodeKey("ACC", "IT1"), new UsageCodeKey("ACC", "IT1"));
			globalPrices.L6_SystemCode = "INV";
			globalPrices.L6_ValidFrom = periodStart;

			var l7_1 = globalPrices.Items[0];
			l7_1.L7_FeeType = "TRB";
			l7_1.L7_UnitBreak = 0;
			l7_1.L7_Description = "IT1.TRB.0";

			var l7_2 = globalPrices.Items[1];
			l7_2.L7_FeeType = "TRB";
			l7_2.L7_UnitBreak = 5;
			l7_2.L7_Description = "IT1.TRB.5";

			var l7_3 = globalPrices.Items[2];
			l7_3.L7_FeeType = "TRB";
			l7_3.L7_UnitBreak = 15;
			l7_3.L7_Description = "IT1.TRB.15";

			BillingTestHelper.AddUsageMap(globalPrices, "ACC", "IT1", "IN1");

			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = lic1.Database;

			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices, periodStart);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, "ACC", "IN1", periodStart, lic1.ClientCompany, 11);

			var period2 = periodStart.AddMonths(1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period2, lic1.ClientCompany, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, "ACC", "IT1", period2, lic1.ClientCompany, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, "ACC", "IN1", period2, lic1.ClientCompany, 11);

			Factory.Save();

			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdLicence.LicenceCode);

			var rowsAsText = string.Join("\r\n", Utilities.GetDataTableFromQuery($"select * from EdiGetStlUsageSummary('{lic1.Company.LC_OH}', '2021-3-1')").Rows.OfType<DataRow>()
				.Select(r => $"{r["LD_ServerCode"]}-{r["LE_EnterpriseCode"]}-{r["L7_Description"]}-{r["LCC_Code"]}-{r["U1_Code"]}-{r["U1_UnitCount"]}").OrderBy(x => x));

			var expected =
@"SYD-DDD-IT1.TRB.15--ACC-18
SYD-DDD-Item USR-ABC-STL-3";

			AssertEquals("mapped usage IN1->IT1 is loaded", expected, rowsAsText);
		}

		public void TestGetStlUsageSummary_ExcludeInactiveLicenceHeader()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "PRD");
			var db1 = licence1.Database;
			var company1 = licence1.Company;
			var licence2a = BillingTestHelper.CreateAnotherDatabase(company1, "SYD");
			licence2a.LA_IsActive = false;
			var db2 = licence2a.Database;
			var licence2b = BillingTestHelper.CreateAnotherLicence(db2, "DEF");
			var company2 = licence2b.Company;

			var prices = BillingTestHelper.CreatePriceList(company1);
			prices.L6_SystemCode = "STL";
			var priceItemUSR = BillingTestHelper.AddPriceItem(prices, "USR", BillingConstants.FeeType.Transactional, "", 10m);
			priceItemUSR.L7_Description = "USR";
			priceItemUSR.L7_UnitBreak = 0;

			BillingTestHelper.CreatePriceLink(db1, prices, new ZDateTime(2010, 1, 1));
			BillingTestHelper.CreatePriceLink(db2, prices, new ZDateTime(2010, 1, 1));

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2017, 1, 1), licence1.ClientCompany, 10);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2017, 1, 1), licence2b.ClientCompany, 20);

			Factory.Save();

			var dataTable = Utilities.GetDataTableFromQuery($"select * from EdiGetStlUsageSummary('{company1.LC_OH}', '2017-1-1')");
			var rowsAsText = string.Join("\r\n", dataTable.Rows.OfType<DataRow>()
				.Select(r => $"{r["LD_ServerCode"]}-{r["LE_EnterpriseCode"]}-{r["L7_Description"]}-{r["LCC_Code"]}-{r["U1_Code"]}-{r["U1_UnitCount"]}").OrderBy(x => x));
			AssertEquals(@"PRD-DDD-USR-ABC-STL-10", rowsAsText);

			dataTable = Utilities.GetDataTableFromQuery($"select * from EdiGetStlUsageSummary('{company2.LC_OH}', '2017-1-1')");
			rowsAsText = string.Join("\r\n", dataTable.Rows.OfType<DataRow>()
				.Select(r => $"{r["LD_ServerCode"]}-{r["LE_EnterpriseCode"]}-{r["L7_Description"]}-{r["LCC_Code"]}-{r["U1_Code"]}-{r["U1_UnitCount"]}").OrderBy(x => x));
			AssertEquals(@"SYD-DDD-USR-DEF-STL-20", rowsAsText);
		}

		public void TestGetStlUsageSummary_NonProduction()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "PRD");
			var db1 = licence1.Database;
			var company1 = licence1.Company;
			var testLic = BillingTestHelper.CreateAnotherDatabase(licence1, "TST", false);
			testLic.Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			testLic.Database.LD_LD_ParentDatabase = db1.PK;

			var prices = BillingTestHelper.CreatePriceList(company1);
			prices.L6_SystemCode = "STL";
			prices.L6_TestDbPriceCode = "#NP";
			var priceItemUSR = BillingTestHelper.AddPriceItem(prices, "USR", BillingConstants.FeeType.Transactional, "", 10m);
			priceItemUSR.L7_Description = "USR";
			var priceItemNP = BillingTestHelper.AddPriceItem(prices, "#NP", BillingConstants.FeeType.Transactional, "", 150m);
			priceItemNP.L7_Category = BillingConstants.BillingSystem.Service;
			priceItemNP.L7_Description = "   Non-Production System";
			var priceItemDataAcess = BillingTestHelper.AddPriceItem(prices, "#HG", BillingConstants.FeeType.PerMBPerMonthMin1GB, "", 0.01m);
			priceItemDataAcess.L7_Category = BillingConstants.BillingSystem.HostingDataAccess;
			priceItemDataAcess.L7_Description = " Per Megabyte Charges for additional services";

			BillingTestHelper.CreatePriceLink(db1, prices, new ZDateTime(2019, 1, 1));

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", new ZDateTime(2019, 5, 1), licence1.ClientCompany, 10);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.Service, "#NP", new ZDateTime(2019, 5, 1), licence1.ClientCompany, 1);
			var usageDataAccess = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingDataAccess, "#HG", new ZDateTime(2019, 5, 1), licence1.LA_LC, 50000);
			usageDataAccess.U1_LD = testLic.LA_LD;
			usageDataAccess.U1_LC = ZGuid.Empty;

			Factory.Save();

			// Non-production system fee is NOT included
			// Hosting data access is included
			var dataTable = Utilities.GetDataTableFromQuery($"select * from EdiGetStlUsageSummary('{company1.LC_OH}', '2019-5-1')");
			var rowsAsText = string.Join("\r\n", dataTable.Rows.OfType<DataRow>()
				.Select(r => $"{r["LD_ServerCode"]}-{r["LE_EnterpriseCode"]}-{r["L7_Description"]}-{r["LCC_Code"]}-{r["U1_Code"]}-{r["U1_UnitCount"]}").OrderBy(x => x));
			AssertEquals(
@"PRD-DDD-USR-ABC-STL-10
TST-DDD- Per Megabyte Charges for additional services--HDA-50000", rowsAsText);
		}

		public void TestGetStlUsageSummary_CargoWiseNext()
		{
			var stdLicence = BillingTestHelper.CreateLicence(Factory, "EDI", "EDI", "SYD", false);
			var stdPriceCompany = stdLicence.Company;
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "USR", "SHP");
			var cwnPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "SHD", "SHD", "SHD", "SHX");
			cwnPrices.L6_SystemCode = "CWN";
			var usageMap = cwnPrices.UsageMaps.AddNew();
			usageMap.PUM_PriceCategory = "CWN";
			usageMap.PUM_PriceCode = "SHD";
			usageMap.PUM_UsageCategory = "STL";
			usageMap.PUM_UsageCode = "SHD";
			var shds = cwnPrices.Items.Where(x => x.L7_Code == "SHD").ToArray();
			shds[0].L7_Category = "CWN";
			shds[0].L7_RN_NKDisbursementCountry = "US";
			shds[0].L7_RX_NKCurrency = "USD";
			shds[0].L7_Order = 1000;
			shds[0].L7_Description = "SHD - Order 1000";
			shds[1].L7_Category = "CWN";
			shds[1].L7_RN_NKDisbursementCountry = "UK";
			shds[1].L7_RX_NKCurrency = "GBP";
			shds[1].L7_Order = 500;
			shds[1].L7_Description = "SHD - Order 500";
			shds[2].L7_Category = "CWN";
			shds[2].L7_RN_NKDisbursementCountry = "CA";
			shds[2].L7_RX_NKCurrency = "CAD";
			shds[2].L7_Order = 8000;
			shds[2].L7_Description = "SHD - Order 8000";
			Factory.Save();

			var periodStart = BillingTestHelper.MonthToday;
			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");
			var lc = licence.Company;
			var db = licence.Database;
			db.LD_DatabaseNumber = 1983;
			db.LD_HostedLocation = "SYD";
			db.LD_Product = ProductTypes.Codes.CargoWiseNext;
			var clientCompany1 = db.ClientCompanies[0];

			BillingTestHelper.CreatePriceLink(db, stlPrices, periodStart);
			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdLicence.LicenceCode);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, clientCompany1, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "US1", periodStart, clientCompany1, 3);
			var shdUsage = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHD", periodStart, clientCompany1, 1);
			shdUsage.U1_RX_NKCurrency = "USD";
			shdUsage.U1_TotalPrice = 13.5m;
			Factory.Save();

			var rowsAsText = string.Join("\r\n", Utilities.GetDataTableFromQuery($"select * from EdiGetStlUsageSummary('{lc.LC_OH}', '{periodStart:yyyy-MM-dd}')").Rows.OfType<DataRow>()
				.Select(r => $"{r["LD_ServerCode"]}-{r["LE_EnterpriseCode"]}-{r["L7_Description"]}-{r["LCC_Code"]}-{r["U1_Code"]}-{r["U1_UnitCount"]}").OrderBy(x => x));

			AssertEquals(@"AAA-AAA-Item USR-SYD-STL-3
AAA-AAA-SHD - Order 500-SYD-STL-1", rowsAsText);
		}

		public void TestGetBillingDbDetailedUsage()
		{
			EServicesBillingTestHelper.CreateTable();

			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var licence = org1.LicCompany.LicHeadersForAllDatabases[0];
			licence.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = licence.Database;
			db1.LD_Product = "CW1";
			var prices = BillingTestHelper.CreatePriceList(licence);
			prices.L6_SystemCode = "STL";

			var usr = BillingTestHelper.AddPriceItem(prices, "USR", BillingConstants.FeeType.Transactional, "", 10m);
			var usw = BillingTestHelper.AddPriceItem(prices, "USW", BillingConstants.FeeType.Transactional, "", 10m);
			var us1 = BillingTestHelper.AddPriceItem(prices, "US1", BillingConstants.FeeType.Transactional, "", 10m);
			var csm = BillingTestHelper.AddPriceItem(prices, "CSM", BillingConstants.FeeType.Transactional, "", 10m);
			csm.L7_Category = "CSP";

			var pr1 = BillingTestHelper.AddPriceItem(prices, "PR1", BillingConstants.FeeType.Transactional, "", 10m);
			var mapping1 = BillingTestHelper.AddUsageMap(prices, "PR1", "AAA");
			mapping1.PUM_UsageCategory = "ACC";

			var pr2 = BillingTestHelper.AddPriceItem(prices, "PR2", BillingConstants.FeeType.Transactional, "", 10m);
			var mapping2 = BillingTestHelper.AddUsageMap(prices, "PR2", "CTR");
			mapping2.PUM_UsageCategory = "CTR";
			var mapping3 = BillingTestHelper.AddUsageMap(prices, "PR2", "AB3");
			mapping3.PUM_UsageCategory = "ABM";
			var mapping4 = BillingTestHelper.AddUsageMap(prices, "PR2", "PM1");

			db1.LD_DatabaseNumber = 80239;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db1.PK, org1.PK, "", "");
			var clientNumber1 = db1.DatabaseId + ".ABC";

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "USR", "", "", "", null, billableCount: 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "US1", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "US1", "", "", "", null, billableCount: 2));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "US2", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "US2", "", "", "", null, billableCount: 3));
			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSP", "CSM", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "Ref1", "Ref2", "", "", null, billableCount: 4));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "AAA", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "Ref1", "", "", "", null, billableCount: 5));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "PR2", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "Ref1", "", "", "", null, billableCount: 6));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "PM1", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "Ref1", "", "", "", null, billableCount: 3));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CTR", "CTO", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "Ref1", "", "", "", null, billableCount: 7));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CTR", "CTR", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "Ref1", "", "", "", null, billableCount: 99));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "AB3", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "Ref1", "", "", "", null, billableCount: 1));
			var usageInfo = new EServicesBillingTestHelper.RawUsageInfo("ABM", "AB3", new ZDateTime(2021, 1, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "Ref1", "", "", "", null, billableCount: 1);
			usageInfo.SystemID = "";
			infoList.Add(usageInfo);

			EServicesBillingTestHelper.AddTransactions(infoList);

			Factory.Save();

			//GetBillingDbDetailedUsageRows(... excludedSystemCodes) should filter the categories by excludedSystemCodes
			CombineAssertions(() =>
			{
				var rowsAsText = string.Join("\r\n", GetBillingDbDetailedUsageRows(202101, db1, clientCompany1, usr)
					.Select(r => $"{r["TX_PriceItemCode"]}-{r["CompanyCode"]}-{r["BranchCode"]}-{r["TX_BillableCount"]}-{r["TX_Reference1"]}").OrderBy(x => x));
				AssertEquals(@"USR-ABC-B10-1-USR", rowsAsText);

				rowsAsText = string.Join("\r\n", GetBillingDbDetailedUsageRows(202101, db1, clientCompany1, usw)
		.Select(r => $"{r["TX_PriceItemCode"]}-{r["CompanyCode"]}-{r["BranchCode"]}-{r["TX_BillableCount"]}-{r["TX_Reference1"]}").OrderBy(x => x));
				AssertEquals(@"USW-ABC-B10-1-USR", rowsAsText);

				rowsAsText = string.Join("\r\n", GetBillingDbDetailedUsageRows(202101, db1, clientCompany1, us1)
		.Select(r => $"{r["TX_PriceItemCode"]}-{r["CompanyCode"]}-{r["BranchCode"]}-{r["TX_BillableCount"]}-{r["TX_Reference1"]}").OrderBy(x => x));
				AssertEquals(@"US1-ABC-B11-2-US1", rowsAsText);

				rowsAsText = string.Join("\r\n", GetBillingDbDetailedUsageRows(202101, db1, clientCompany1, csm, "CSP")
		.Select(r => $"{r["TX_PriceItemCode"]}-{r["CompanyCode"]}-{r["BranchCode"]}-{r["TX_BillableCount"]}-{r["TX_Reference1"]}").OrderBy(x => x));
				AssertEquals(@"CSM-ABC--4-Ref1", rowsAsText);

				rowsAsText = string.Join("\r\n", GetBillingDbDetailedUsageRows(202101, db1, clientCompany1, pr1)
		.Select(r => $"{r["TX_PriceItemCode"]}-{r["CompanyCode"]}-{r["BranchCode"]}-{r["TX_BillableCount"]}-{r["TX_Reference1"]}").OrderBy(x => x));
				AssertEquals(@"AAA-ABC--5-Ref1", rowsAsText);

				rowsAsText = string.Join("\r\n", GetBillingDbDetailedUsageRows(202101, db1, clientCompany1, pr2)
		.Select(r => $"{r["TX_PriceItemCode"]}-{r["CompanyCode"]}-{r["BranchCode"]}-{r["TX_BillableCount"]}-{r["TX_Reference1"]}").OrderBy(x => x));
				AssertEquals(@"AB3-ABC--1-Ref1
AB3-ABC--1-Ref1
CTR-ABC--7-Ref1
PM1-ABC--3-Ref1
PR2-ABC--6-Ref1", rowsAsText);

				rowsAsText = string.Join("\r\n", GetBillingDbDetailedUsageRows(202101, db1, clientCompany1, pr2, "ABM")
		.Select(r => $"{r["TX_PriceItemCode"]}-{r["CompanyCode"]}-{r["BranchCode"]}-{r["TX_BillableCount"]}-{r["TX_Reference1"]}").OrderBy(x => x));
				AssertEquals(@"CTR-ABC--7-Ref1
PM1-ABC--3-Ref1
PR2-ABC--6-Ref1", rowsAsText);
			});
		}

		public void TestGetBillingDbDetailedUsage_FeeType_TRB()
		{
			EServicesBillingTestHelper.CreateTable();

			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var licence = org1.LicCompany.LicHeadersForAllDatabases[0];
			licence.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = licence.Database;
			db1.LD_Product = "CW1";
			var prices = BillingTestHelper.CreatePriceList(licence);
			prices.L6_SystemCode = "STL";

			Factory.Save();

			db1.LD_DatabaseNumber = 80239;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "CC1", db1.PK, ZGuid.Empty, "", "");
			var clientNumber1 = db1.DatabaseId + ".CC1";
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "CC2", db1.PK, ZGuid.Empty, "", "");
			var clientNumber2 = db1.DatabaseId + ".CC2";

			var ctr_1_250 = BillingTestHelper.AddPriceItem(prices, "CTR", "TRB", "", 1.623332m);
			ctr_1_250.L7_UnitBreak = 0;
			ctr_1_250.L7_Category = "CTR";
			var ctr_251_1000 = BillingTestHelper.AddPriceItem(prices, "CTR", "TRB", "", 1.537893m);
			ctr_251_1000.L7_UnitBreak = 250;
			ctr_251_1000.L7_Category = "CTR";
			var ctr_1001_2000 = BillingTestHelper.AddPriceItem(prices, "CTR", "TRB", "", 1.452455m);
			ctr_1001_2000.L7_UnitBreak = 1000;
			ctr_1001_2000.L7_Category = "CTR";
			var ctr_2001_more = BillingTestHelper.AddPriceItem(prices, "CTR", "TRB", "", 1.367016m);
			ctr_2001_more.L7_UnitBreak = 2000;
			ctr_2001_more.L7_Category = "CTR";

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CTR", "CTO", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "Ref-CC1-200", "", "", "", null, billableCount: 200));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CTR", "CTO", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "Ref-CC1-80", "", "", "", null, billableCount: 80));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CTR", "CTO", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDABCSYD", clientNumber2, db1.DatabaseId, clientCompany2.PK, "Ref-CC2-400", "", "", "", null, billableCount: 400));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CTR", "CTO", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDABCSYD", clientNumber2, db1.DatabaseId, clientCompany2.PK, "Ref-CC2-3000", "", "", "", null, billableCount: 3000));
			EServicesBillingTestHelper.AddTransactions(infoList);

			Factory.Save();

			CombineAssertions(() =>
			{
				//We need to load all usages for TRB type, because the summary page shows [All] only for TRB type.
				var rowsAsText = string.Join("\r\n", GetBillingDbDetailedUsageRows(202101, db1, null, ctr_251_1000, "CTR")
					.Select(r => $"{r["TX_PriceItemCode"]}-{r["CompanyCode"]}-{r["BranchCode"]}-{r["TX_BillableCount"]}-{r["TX_Reference1"]}").OrderBy(x => x));
				AssertEquals(@"CTR-CC1--200-Ref-CC1-200
CTR-CC1--80-Ref-CC1-80
CTR-CC2--3000-Ref-CC2-3000
CTR-CC2--400-Ref-CC2-400", rowsAsText);

				rowsAsText = string.Join("\r\n", GetBillingDbDetailedUsageRows(202101, db1, null, ctr_2001_more, "CTR")
					.Select(r => $"{r["TX_PriceItemCode"]}-{r["CompanyCode"]}-{r["BranchCode"]}-{r["TX_BillableCount"]}-{r["TX_Reference1"]}").OrderBy(x => x));
				AssertEquals(@"CTR-CC1--200-Ref-CC1-200
CTR-CC1--80-Ref-CC1-80
CTR-CC2--3000-Ref-CC2-3000
CTR-CC2--400-Ref-CC2-400", rowsAsText);
			});
		}

		public void TestGetBillingDbDetailedUsage_WTU()
		{
			EServicesBillingTestHelper.CreateTable();

			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var licence = org1.LicCompany.LicHeadersForAllDatabases[0];
			licence.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = licence.Database;
			db1.LD_Product = "CW1";
			var prices = BillingTestHelper.CreatePriceList(licence);
			prices.L6_SystemCode = "STL";

			Factory.Save();

			var usr = BillingTestHelper.AddPriceItem(prices, "USR", BillingConstants.FeeType.Transactional, "", 10m);

			var wtp_0_1 = BillingTestHelper.AddPriceItem(prices, "WTP", "TRB", "", 1.623332m);
			wtp_0_1.L7_UnitBreak = 0;
			var wtp_1_2 = BillingTestHelper.AddPriceItem(prices, "WTP", "TRB", "", 1.537893m);
			wtp_1_2.L7_UnitBreak = 1;
			var wtp_2_up = BillingTestHelper.AddPriceItem(prices, "WTP", "TRB", "", 1.452455m);
			wtp_2_up.L7_UnitBreak = 2;

			var mapping1 = BillingTestHelper.AddUsageMap(prices, "WTP", "WTU");
			mapping1.PUM_UsageCategory = "STL";

			db1.LD_DatabaseNumber = 80239;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "CC1", db1.PK, ZGuid.Empty, "", "");
			var clientNumber1 = db1.DatabaseId + ".CC1";
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "CC2", db1.PK, ZGuid.Empty, "", "");
			var clientNumber2 = db1.DatabaseId + ".CC2";
			Factory.Save();

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "USR", "", "", "", null, billableCount: 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK, "USR", "", "", "", null, billableCount: 1));
			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK,
						"PackageID#1", "JobID#1", "PkgPackageHeader.PK.#1", "1", null, billableCount: 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK,
						"PackageID#1", "JobID#1", "PkgPackageHeader.PK.#1", "2", null, billableCount: 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK,
						"PackageID#1", "JobID#1", "PkgPackageHeader.PK.#1", "3", null, billableCount: 1));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK,
						"PackageID#2", "JobID#2", "PkgPackageHeader.PK.#2", "2", null, billableCount: 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK,
						"PackageID#5", "JobID#5", "PkgPackageHeader.PK.#5", "5", null, billableCount: 1));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK,
						"PackageID#49_51", "JobID#49_51", "PkgPackageHeader.PK.#49_51", "49", null, billableCount: 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK,
						"PackageID#49_51", "JobID#49_51", "PkgPackageHeader.PK.#49_51", "50", null, billableCount: 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK,
						"PackageID#49_51", "JobID#49_51", "PkgPackageHeader.PK.#49_51", "51", null, billableCount: 1));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK,
						"PackageID#88", "JobID#88", "PkgPackageHeader.PK.#88", "88", null, billableCount: 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK,
						"PackageID#50", "JobID#50", "PkgPackageHeader.PK.#50", "50", null, billableCount: 1));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK,
						"PackageID#91_93", "JobID#91_93", "PkgPackageHeader.PK.#91_93", "90", null, billableCount: 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK,
						"PackageID#91_93", "JobID#91_93", "PkgPackageHeader.PK.#91_93", "91", null, billableCount: 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK,
						"PackageID#91_93", "JobID#91_93", "PkgPackageHeader.PK.#91_93", "92", null, billableCount: 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK,
						"PackageID#91_93", "JobID#91_93", "PkgPackageHeader.PK.#91_93", "93", null, billableCount: 1));

			EServicesBillingTestHelper.AddTransactions(infoList);
			Factory.Save();

			void assertRows(string rowsAsTextExpected, ClientCompany lcc, ClientLicencePriceItem l7)
			{
				var rowsAsText = string.Join("\r\n", GetBillingDbDetailedUsageRows(202101, db1, lcc, l7)
	.Select(r => $"{r["TX_PriceItemCode"]}-{r["CompanyCode"]}-{r["BranchCode"]}-{r["TX_BillableCount"]}-{r["TX_Reference1"]}-{r["TX_Reference2"]}-{r["TX_Reference3"]}-{r["TX_Reference4"]}-{r["AdjustedUnitCount"]}").OrderBy(x => x));
				AssertEquals(rowsAsTextExpected, rowsAsText);
			}

			assertRows(@"USR-CC1-B10-1-USR----
USR-CC2-B11-1-USR----", null, usr);
			assertRows(@"USR-CC1-B10-1-USR----", clientCompany1, usr);
			assertRows(@"USR-CC2-B11-1-USR----", clientCompany2, usr);

			//We need to load all usages for TRB type, because the summary page shows [All] only for TRB type.
			assertRows(@"WTU-CC1--1-PackageID#1-JobID#1-PkgPackageHeader.PK.#1-1-1.0000
WTU-CC1--1-PackageID#1-JobID#1-PkgPackageHeader.PK.#1-2-0.5000
WTU-CC1--1-PackageID#1-JobID#1-PkgPackageHeader.PK.#1-3-0.3300
WTU-CC2--1-PackageID#2-JobID#2-PkgPackageHeader.PK.#2-2-0.5000
WTU-CC2--1-PackageID#49_51-JobID#49_51-PkgPackageHeader.PK.#49_51-49-0.2500
WTU-CC2--1-PackageID#49_51-JobID#49_51-PkgPackageHeader.PK.#49_51-50-0.2400
WTU-CC2--1-PackageID#49_51-JobID#49_51-PkgPackageHeader.PK.#49_51-51-0.2450
WTU-CC2--1-PackageID#50-JobID#50-PkgPackageHeader.PK.#50-50-0.2400
WTU-CC2--1-PackageID#5-JobID#5-PkgPackageHeader.PK.#5-5-0.2000
WTU-CC2--1-PackageID#88-JobID#88-PkgPackageHeader.PK.#88-88-0.2450
WTU-CC2--1-PackageID#91_93-JobID#91_93-PkgPackageHeader.PK.#91_93-90-0.2450
WTU-CC2--1-PackageID#91_93-JobID#91_93-PkgPackageHeader.PK.#91_93-91-0.2450
WTU-CC2--1-PackageID#91_93-JobID#91_93-PkgPackageHeader.PK.#91_93-92-0.2450
WTU-CC2--1-PackageID#91_93-JobID#91_93-PkgPackageHeader.PK.#91_93-93-0.2450", null, wtp_1_2);
		}

		public void TestGetBillingDbDetailedUsage_UCN()
		{
			EServicesBillingTestHelper.CreateTable();

			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var licence = org1.LicCompany.LicHeadersForAllDatabases[0];
			licence.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = licence.Database;
			db1.LD_Product = "CW1";
			var prices = BillingTestHelper.CreatePriceList(licence);
			prices.L6_SystemCode = "STL";

			Factory.Save();

			var usr = BillingTestHelper.AddPriceItem(prices, "USR", BillingConstants.FeeType.Transactional, "", 10m);
			var ucn = BillingTestHelper.AddPriceItem(prices, "UCN", BillingConstants.FeeType.Transactional, "", 5m);

			db1.LD_DatabaseNumber = 80239;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "CC1", db1.PK, ZGuid.Empty, "", "");
			var clientNumber1 = db1.DatabaseId + ".CC1";
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "CC2", db1.PK, ZGuid.Empty, "", "");
			var clientNumber2 = db1.DatabaseId + ".CC2";
			Factory.Save();

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 5, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "USR", "", "", "", null, billableCount: 130));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 5, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK, "USR", "", "", "", null, billableCount: 150));
			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);

			EServicesBillingTestHelper.AddTransactions(infoList);
			Factory.Save();

			void assertRows(string rowsAsTextExpected, ClientCompany lcc, ClientLicencePriceItem l7)
			{
				var rowsAsText = string.Join("\r\n", GetBillingDbDetailedUsageRows(202205, db1, lcc, l7)
	.Select(r => $"{r["TX_PriceItemCode"]}-{r["CompanyCode"]}-{r["BranchCode"]}-{r["TX_BillableCount"]}-{r["TX_Reference1"]}-{r["TX_Reference2"]}-{r["TX_Reference3"]}-{r["TX_Reference4"]}-{r["AdjustedUnitCount"]}").OrderBy(x => x));
				AssertEquals(rowsAsTextExpected, rowsAsText);
			}

			assertRows(@"USR-CC1-B10-130-USR----
USR-CC2-B11-150-USR----", null, usr);
			assertRows(@"USR-CC1-B10-130-USR----", clientCompany1, usr);
			assertRows(@"USR-CC2-B11-150-USR----", clientCompany2, usr);

			assertRows(@"UCN-CC1-B10-130-USR----
UCN-CC2-B11-150-USR----", null, ucn);
			assertRows(@"UCN-CC1-B10-130-USR----", clientCompany1, ucn);
			assertRows(@"UCN-CC2-B11-150-USR----", clientCompany2, ucn);
		}

		public void TestGetBillingDbDetailedUsage_UCS_USW()
		{
			EServicesBillingTestHelper.CreateTable();

			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var licence = org1.LicCompany.LicHeadersForAllDatabases[0];
			licence.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = licence.Database;
			db1.LD_Product = "CW1";
			var prices = BillingTestHelper.CreatePriceList(licence);
			prices.L6_SystemCode = "STL";

			Factory.Save();

			var usr = BillingTestHelper.AddPriceItem(prices, "USR", BillingConstants.FeeType.Transactional, "", 10m);
			var ucs = BillingTestHelper.AddPriceItem(prices, "UCS", BillingConstants.FeeType.Transactional, "", 5m);
			var usw = BillingTestHelper.AddPriceItem(prices, "USW", BillingConstants.FeeType.Transactional, "", 8m);

			db1.LD_DatabaseNumber = 80239;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "CC1", db1.PK, ZGuid.Empty, "", "");
			var clientNumber1 = db1.DatabaseId + ".CC1";
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "CC2", db1.PK, ZGuid.Empty, "", "");
			var clientNumber2 = db1.DatabaseId + ".CC2";
			Factory.Save();

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 5, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "USR", "", "", "", null, billableCount: 130));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 5, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK, "USR", "", "", "", null, billableCount: 150));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2022, 5, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "RBU", "", "", "", null, billableCount: 20));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2022, 5, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK, "RBU", "", "", "", null, billableCount: 30));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 7, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "USR", "", "", "", null, billableCount: 45));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 7, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK, "USR", "", "", "", null, billableCount: 50));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2022, 7, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "RBU", "", "", "", null, billableCount: 15));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2022, 7, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK, "RBU", "", "", "", null, billableCount: 18));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);

			EServicesBillingTestHelper.AddTransactions(infoList);
			Factory.Save();

			void assertRows(int period, string rowsAsTextExpected, ClientCompany lcc, ClientLicencePriceItem l7)
			{
				var rowsAsText = string.Join("\r\n", GetBillingDbDetailedUsageRows(period, db1, lcc, l7)
	.Select(r => $"{r["TX_PriceItemCode"]}-{r["CompanyCode"]}-{r["BranchCode"]}-{r["TX_BillableCount"]}-{r["TX_Reference1"]}-{r["TX_Reference2"]}-{r["TX_Reference3"]}-{r["TX_Reference4"]}-{r["AdjustedUnitCount"]}").OrderBy(x => x));
				AssertEquals(rowsAsTextExpected, rowsAsText);
			}

			assertRows(202205, @"USR-CC1-B10-130-USR----
USR-CC2-B11-150-USR----", null, usr);
			assertRows(202205, @"USR-CC1-B10-130-USR----", clientCompany1, usr);
			assertRows(202205, @"USR-CC2-B11-150-USR----", clientCompany2, usr);

			assertRows(202205, @"UCS-CC1-B10-130-USR----
UCS-CC2-B11-150-USR----", null, ucs);
			assertRows(202205, @"UCS-CC1-B10-130-USR----", clientCompany1, ucs);
			assertRows(202205, @"UCS-CC2-B11-150-USR----", clientCompany2, ucs);

			assertRows(202205, @"USW-CC1-B10-130-USR----
USW-CC2-B11-150-USR----", null, usw);
			assertRows(202205, @"USW-CC1-B10-130-USR----", clientCompany1, usw);
			assertRows(202205, @"USW-CC2-B11-150-USR----", clientCompany2, usw);

			assertRows(202207, @"USR-CC1-B14-45-USR----
USR-CC2-B15-50-USR----", null, usr);
			assertRows(202207, @"USR-CC1-B14-45-USR----", clientCompany1, usr);
			assertRows(202207, @"USR-CC2-B15-50-USR----", clientCompany2, usr);

			assertRows(202207, @"UCS-CC1-B14-45-USR----
UCS-CC1-B16-15-RBU----
UCS-CC2-B15-50-USR----
UCS-CC2-B17-18-RBU----", null, ucs);
			assertRows(202207, @"UCS-CC1-B14-45-USR----
UCS-CC1-B16-15-RBU----", clientCompany1, ucs);
			assertRows(202207, @"UCS-CC2-B15-50-USR----
UCS-CC2-B17-18-RBU----", clientCompany2, ucs);

			assertRows(202207, @"USW-CC1-B14-45-USR----
USW-CC1-B16-15-RBU----
USW-CC2-B15-50-USR----
USW-CC2-B17-18-RBU----", null, usw);
			assertRows(202207, @"USW-CC1-B14-45-USR----
USW-CC1-B16-15-RBU----", clientCompany1, usw);
			assertRows(202207, @"USW-CC2-B15-50-USR----
USW-CC2-B17-18-RBU----", clientCompany2, usw);
		}

		public void TestGetBillingDbDetailedUsage_USR_RBU()
		{
			EServicesBillingTestHelper.CreateTable();

			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var licence = org1.LicCompany.LicHeadersForAllDatabases[0];
			licence.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = licence.Database;
			db1.LD_Product = "CW1";
			var prices = BillingTestHelper.CreatePriceList(licence);
			prices.L6_SystemCode = "STL";

			Factory.Save();

			var usr = BillingTestHelper.AddPriceItem(prices, "USR", BillingConstants.FeeType.Transactional, "", 10m);
			var ucs = BillingTestHelper.AddPriceItem(prices, "UCS", BillingConstants.FeeType.Transactional, "", 5m);
			var usw = BillingTestHelper.AddPriceItem(prices, "USW", BillingConstants.FeeType.Transactional, "", 8m);

			db1.LD_DatabaseNumber = 80239;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "CC1", db1.PK, ZGuid.Empty, "", "");
			var clientNumber1 = db1.DatabaseId + ".CC1";
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "CC2", db1.PK, ZGuid.Empty, "", "");
			var clientNumber2 = db1.DatabaseId + ".CC2";
			Factory.Save();

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 5, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "USR", "", "", "", null, billableCount: 130));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 5, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK, "USR", "", "", "", null, billableCount: 150));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2022, 5, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "RBU", "", "", "", null, billableCount: 20));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2022, 5, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK, "RBU", "", "", "", null, billableCount: 30));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 7, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "USR", "", "", "", null, billableCount: 45));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 7, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK, "USR", "", "", "", null, billableCount: 50));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2022, 7, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "RBU", "", "", "", null, billableCount: 15));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2022, 7, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK, "RBU", "", "", "", null, billableCount: 18));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 5, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "USR", "", "", "", null, billableCount: 1) { StaffCode = "TXZ" });
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2022, 5, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "RBU", "", "", "", null, billableCount: 1) { StaffCode = "TXZ" });

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2022, 7, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "USR", "", "", "", null, billableCount: 1) { StaffCode = "TXZ" });
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RBU", new ZDateTime(2022, 7, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "RBU", "", "", "", null, billableCount: 1) { StaffCode = "TXZ" });

			EServicesBillingTestHelper.AddTransactions(infoList);
			Factory.Save();

			void assertRows(int period, string rowsAsTextExpected, ClientCompany lcc, ClientLicencePriceItem l7)
			{
				var rowsAsText = string.Join("\r\n", GetBillingDbDetailedUsageRows(period, db1, lcc, l7)
	.Select(r => $"{r["TX_PriceItemCode"]}-{r["CompanyCode"]}-{r["BranchCode"]}-{r["TX_BillableCount"]}-{r["TX_Reference1"]}-{r["TX_Reference2"]}-{r["TX_Reference3"]}-{r["TX_Reference4"]}-{r["AdjustedUnitCount"]}").OrderBy(x => x));
				AssertEquals(rowsAsTextExpected, rowsAsText);
			}

			assertRows(202205, @"USR-CC1-B10-130-USR----
USR-CC2-B11-150-USR----", null, usr);
			assertRows(202205, @"USR-CC1-B10-130-USR----", clientCompany1, usr);
			assertRows(202205, @"USR-CC2-B11-150-USR----", clientCompany2, usr);

			assertRows(202205, @"UCS-CC1-B10-130-USR----
UCS-CC2-B11-150-USR----", null, ucs);
			assertRows(202205, @"UCS-CC1-B10-130-USR----", clientCompany1, ucs);
			assertRows(202205, @"UCS-CC2-B11-150-USR----", clientCompany2, ucs);

			assertRows(202205, @"USW-CC1-B10-130-USR----
USW-CC2-B11-150-USR----", null, usw);
			assertRows(202205, @"USW-CC1-B10-130-USR----", clientCompany1, usw);
			assertRows(202205, @"USW-CC2-B11-150-USR----", clientCompany2, usw);

			assertRows(202207, @"USR-CC1-B14-45-USR----
USR-CC2-B15-50-USR----", null, usr);
			assertRows(202207, @"USR-CC1-B14-45-USR----", clientCompany1, usr);
			assertRows(202207, @"USR-CC2-B15-50-USR----", clientCompany2, usr);

			assertRows(202207, @"UCS-CC1--1-RBU----
UCS-CC1-B14-45-USR----
UCS-CC1-B16-15-RBU----
UCS-CC2-B15-50-USR----
UCS-CC2-B17-18-RBU----", null, ucs);
			assertRows(202207, @"UCS-CC1--1-RBU----
UCS-CC1-B14-45-USR----
UCS-CC1-B16-15-RBU----", clientCompany1, ucs);
			assertRows(202207, @"UCS-CC2-B15-50-USR----
UCS-CC2-B17-18-RBU----", clientCompany2, ucs);

			assertRows(202207, @"USW-CC1--1-RBU----
USW-CC1-B14-45-USR----
USW-CC1-B16-15-RBU----
USW-CC2-B15-50-USR----
USW-CC2-B17-18-RBU----", null, usw);
			assertRows(202207, @"USW-CC1--1-RBU----
USW-CC1-B14-45-USR----
USW-CC1-B16-15-RBU----", clientCompany1, usw);
			assertRows(202207, @"USW-CC2-B15-50-USR----
USW-CC2-B17-18-RBU----", clientCompany2, usw);
		}

		public void TestGetBillingDbDetailedUsage_Disbursement()
		{
			EServicesBillingTestHelper.CreateTable();
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var licence = org1.LicCompany.LicHeadersForAllDatabases[0];
			licence.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = licence.Database;
			db1.LD_Product = "CW1";
			var prices = BillingTestHelper.CreatePriceList(licence);
			prices.L6_SystemCode = "STL";

			Factory.Save();
			var shp = BillingTestHelper.AddPriceItem(prices, "SHP", BillingConstants.FeeType.Transactional, "", 10m);

			db1.LD_DatabaseNumber = 80239;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "CC1", db1.PK, ZGuid.Empty, "", "");
			var clientNumber1 = db1.DatabaseId + ".CC1";
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "CC2", db1.PK, ZGuid.Empty, "", "");
			var clientNumber2 = db1.DatabaseId + ".CC2";
			Factory.Save();

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "SHP", new ZDateTime(2022, 5, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "JOB001", "", "", "", "JOB_PK_0001", null, billableCount: 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "SHP", new ZDateTime(2022, 5, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK, "JOB002", "", "", "", "JOB_PK_0002", null, billableCount: 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "SHP", new ZDateTime(2022, 5, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "JOB003", "", "", "", "JOB_PK_0003", null, billableCount: 1));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "SHD", new ZDateTime(2022, 5, 1, 10, 0, 0), "DDDAB2SYD", clientNumber2, db1.DatabaseId, clientCompany2.PK, "JOB002", "AUD", "1.5", "", "JOB_PK_0002", null, billableCount: 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "SHD", new ZDateTime(2022, 5, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "JOB009", "AUD", "1.5", "", "JOB_PK_0009", null, billableCount: 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
 
			EServicesBillingTestHelper.AddTransactions(infoList);
			Factory.Save();

			void assertRows(int period, string rowsAsTextExpected, ClientCompany lcc, ClientLicencePriceItem l7)
			{
				var rowsAsText = string.Join("\r\n", GetBillingDbDetailedUsageRows(period, db1, lcc, l7)
	.Select(r => $"{r["TX_PriceItemCode"]}-{r["CompanyCode"]}-{r["BranchCode"]}-{r["TX_BillableCount"]}-{r["TX_Reference1"]}-{r["TX_Reference2"]}-{r["TX_Reference3"]}-{r["TX_Reference4"]}-{r["AdjustedUnitCount"]}").OrderBy(x => x));
				AssertEquals(rowsAsTextExpected, rowsAsText);
			}

			var mappings = new CodeDescriptionBoolCollection();
			EDIDataRegistry.Instance.BillingDisbursementUsageMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);
			assertRows(202205,
//all SHP here, because there is no mapping yet.
@"SHP-CC1-B10-1-JOB001----
SHP-CC1-B12-1-JOB003----
SHP-CC2-B11-1-JOB002----", null, shp);

			mappings = new CodeDescriptionBoolCollection();
			mappings.Add("SHD", (NoResString)"SHP");
			EDIDataRegistry.Instance.BillingDisbursementUsageMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);
			//JOB001 - no Disbursement flag (#ref4) 
			//JOB002 - matched by SHD
			//JOB003 - not matched by SHD
			assertRows(202205,
@"SHP-CC1-B10-1-JOB001----
SHP-CC1-B12-1-JOB003----", null, shp);
		}

		public void TestGetBillingDbDetailedUsage_ConsolidatedUsages()
		{
			EServicesBillingTestHelper.CreateTable();

			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "AA1", "SYD");
			var licence = org1.LicCompany.LicHeadersForAllDatabases[0];
			licence.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = licence.Database;
			db1.LD_Product = "SMF";
			db1.LD_DatabaseNumber = 2000;
			db1.LD_TenantID = "1900";

			var lic2 = BillingTestHelper.CreateAnotherDatabase(licence, "AA2");
			var db2 = lic2.Database;
			db2.LD_Product = "SMF";
			db2.LD_DatabaseNumber = 2001;
			db2.LD_TenantID = "1901";

			var lic3 = BillingTestHelper.CreateAnotherDatabase(licence, "AA3");
			var db3 = lic3.Database;
			db3.LD_Product = "SMF";
			db3.LD_DatabaseNumber = 2002;
			db3.LD_TenantID = "1902";

			var prices = BillingTestHelper.CreatePriceList(licence);
			prices.L6_SystemCode = "SMF";
			var price01 = BillingTestHelper.AddPriceItem(prices, "P01", BillingConstants.FeeType.Transactional, "", 10m);

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SMF", "P01", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDAA1SYD", "", db1.DatabaseId, ZGuid.Empty, "DB1", "", "", "", null, billableCount: 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SMF", "P01", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDAA2SYD", "", db2.DatabaseId, ZGuid.Empty, "DB2", "", "", "", null, billableCount: 2));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SMF", "P01", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDAA3SYD", "", db3.DatabaseId, ZGuid.Empty, "DB3", "", "", "", null, billableCount: 3));
			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			Factory.Save();

			var rowsAsText = string.Join("\r\n", GetBillingDbDetailedUsageRows(202101, db1, null, price01)
					.Select(r => $"{r["TX_PriceItemCode"]}-{r["CompanyCode"]}-{r["BranchCode"]}-{r["TX_BillableCount"]}-{r["TX_Reference1"]}-{r["TenantID"]}").OrderBy(x => x));
			AssertEquals(@"P01-   -B10-1-DB1-", rowsAsText);

			Db.Connection.ExecuteNonQuery(
$@"INSERT INTO EdiLicenceDatabaseConsolidationHistory(EDH_Period, EDH_LD, EDH_LD_ConsolidatedDatabase)
VALUES (202101, '{db1.PK}', '{db1.PK}');

INSERT INTO EdiLicenceDatabaseConsolidationHistory(EDH_Period, EDH_LD, EDH_LD_ConsolidatedDatabase)
VALUES (202101, '{db2.PK}', '{db1.PK}');

INSERT INTO EdiLicenceDatabaseConsolidationHistory(EDH_Period, EDH_LD, EDH_LD_ConsolidatedDatabase)
VALUES (202101, '{db3.PK}', '{db1.PK}');
");
			rowsAsText = string.Join("\r\n", GetBillingDbDetailedUsageRows(202101, db1, null, price01)
				.Select(r => $"{r["TX_PriceItemCode"]}-{r["CompanyCode"]}-{r["BranchCode"]}-{r["TX_BillableCount"]}-{r["TX_Reference1"]}-{r["TenantID"]}").OrderBy(x => x));
			AssertEquals(@"P01-   -B10-1-DB1-1900
P01-   -B11-2-DB2-1901
P01-   -B12-3-DB3-1902", rowsAsText);
		}

		public void TestGetBillingDbDetailedUsage_PriceCodeMapping()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "PRD");
			var db1 = licence1.Database;
			db1.LD_DatabaseNumber = 12345;
			var company1 = licence1.Company;
			var clientCompany1 = licence1.ClientCompany;
			var clientNumber1 = db1.DatabaseId + ".ABC";

			var prices = BillingTestHelper.CreatePriceList(company1);
			prices.L6_SystemCode = "STL";
			var priceItemEFC = BillingTestHelper.AddPriceItem(prices, "EFC", BillingConstants.FeeType.Transactional, "", 10m);
			priceItemEFC.L7_Description = "Export Formal Clearance / Fiscal Report";

			BillingTestHelper.AddUsageMap(prices, "EFC", "EF2");
			BillingTestHelper.AddUsageMap(prices, "EFC", "COO");
			BillingTestHelper.AddUsageMap(prices, "EFC", "TNP");

			BillingTestHelper.CreatePriceLink(db1, prices, new ZDateTime(2022, 1, 1));

			EServicesBillingTestHelper.CreateTable();
			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "EFC", new ZDateTime(2022, 7, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "SBNE22079499", "AU", "", "", new DateTime(2022, 7, 2), "ENT", billableCount: 1));
			EServicesBillingTestHelper.AddTransactions(infoList);

			Factory.Save();

			var rowsAsText = string.Join("\r\n", GetBillingDbDetailedUsageRows(202207, db1, null, priceItemEFC)
						.Select(r => $"{r["TX_PriceItemCode"]}-{r["CompanyCode"]}-{r["TX_BillableCount"]}-{r["TX_Reference1"]}-{r["TX_Reference2"]}").OrderBy(x => x));
			AssertEquals("Should be no duplicate rows from usage code mapping", @"EFC-ABC-1-SBNE22079499-AU", rowsAsText);
		}

		public void TestGetBillingDbDetailedUsage_AFR_CutOff()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "PRD");
			var db1 = licence1.Database;
			db1.LD_DatabaseNumber = 12345;
			var company1 = licence1.Company;
			var clientCompany1 = licence1.ClientCompany;
			var clientNumber1 = db1.DatabaseId + ".ABC";

			var prices = BillingTestHelper.CreatePriceList(company1);
			prices.L6_SystemCode = "STL";
			var priceItemPD = BillingTestHelper.AddPriceItem(prices, "#PD", BillingConstants.FeeType.Transactional, "", 10m);
			priceItemPD.L7_Description = "Pre Departure Manifest (eManifest, AMS, ACI, ICS, AFR etc)";

			var mapping = BillingTestHelper.AddUsageMap(prices, "STL", "#PD", "AFR");
			mapping.PUM_UsageCategory = "JPC";

			BillingTestHelper.CreatePriceLink(db1, prices, new ZDateTime(2022, 1, 1));

			var licenceDSV = BillingTestHelper.CreateLicence(Factory, "DSV", "ABC", "PRD");
			var dbDSV = licenceDSV.Database;
			dbDSV.LD_DatabaseNumber = 3125;
			var companyDSV = licenceDSV.Company;
			var clientcompanyDSV = licenceDSV.ClientCompany;
			var clientNumberDSV = dbDSV.DatabaseId + ".ABC";

			var pricesDSV = BillingTestHelper.CreatePriceList(companyDSV);
			pricesDSV.L6_SystemCode = "STL";
			var priceItemDSV_PD = BillingTestHelper.AddPriceItem(pricesDSV, "#PD", BillingConstants.FeeType.Transactional, "", 2m);
			priceItemDSV_PD.L7_Description = "DSV_Pre Departure Manifest (eManifest, AMS, ACI, ICS, AFR etc)";

			var mappingDSV = BillingTestHelper.AddUsageMap(pricesDSV, "STL", "#PD", "AFR");
			mappingDSV.PUM_UsageCategory = "JPC";
			BillingTestHelper.CreatePriceLink(dbDSV, pricesDSV, new ZDateTime(2022, 1, 1));

			EServicesBillingTestHelper.CreateTable();
			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "AFR", new ZDateTime(2022, 7, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "AFR from STL collector", "AU", "", "", new DateTime(2022, 7, 2), "ENT", billableCount: 2));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("JPC", "AFR", new ZDateTime(2022, 7, 3, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "AFR from JPC billing message", "AU", "", "", new DateTime(2022, 7, 3), "HUB", billableCount: 3));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "AFR", new ZDateTime(2024, 6, 3, 10, 0, 0), "DSVABCSYD", clientNumberDSV, dbDSV.DatabaseId, clientcompanyDSV.PK, "AFR from STL billing message (DSV)", "AU", "", "", new DateTime(2024, 6, 3), "HUB", billableCount: 4));
			EServicesBillingTestHelper.AddTransactions(infoList);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertGetBillingDbDetailedUsageRows("old mapping (JPC.AFR)", "AFR-ABC--3-AFR from JPC billing message-AU---", 202207, db1, null, priceItemPD);

				mapping.PUM_UsageCategory = "STL";
				Factory.Save();
				AssertGetBillingDbDetailedUsageRows("new mapping (STL.AFR)", "AFR-ABC--2-AFR from STL collector-AU---", 202207, db1, null, priceItemPD);

				var gstTaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
				var charge1 = BillingTestHelper.CreateChargeCode(Factory, gstTaxRate, "SALESFEE");
				var billed = Factory.New<EdiBilledUsage>();
				billed.BU9_LD = db1.PK;
				billed.BU9_L7 = priceItemPD.PK;
				billed.BU9_PriceCode = "#PD";
				billed.BU9_AC_AmountChargeCode = charge1.PK;
				billed.BU9_AC_DiscountChargeCode = charge1.PK;
				billed.BU9_AH_Invoice = Factory.NewWithValidTestData<ARInvoice>().PK;
				billed.BU9_UsageCode = "JPC";
				billed.BU9_BillingModel = "STL";
				billed.BU9_UsageSubCode = "AFR";
				billed.BU9_UnitCount = 3;
				billed.BU9_UnitPrice = 1;
				billed.BU9_LocalAmountPostDiscount = 1;
				billed.BU9_LocalAmountPreDiscount = 1;
				billed.BU9_LocalProcessingAmount = 1;
				billed.BU9_PeriodStart = new ZDate(2022, 7, 1);
				billed.BU9_PriceCurrency = "AUD";
				billed.BU9_TransactionAmountPostDiscount = 1;
				billed.BU9_TransactionAmountPreDiscount = 1;
				billed.BU9_TransactionProcessingAmount = 1;
				Factory.Save();
				AssertGetBillingDbDetailedUsageRows("new mapping (STL.AFR) but billed by JPC.AFR", "AFR-ABC--3-AFR from JPC billing message-AU---", 202207, db1, null, priceItemPD);

				AssertGetBillingDbDetailedUsageRows("special condition (DSV)", "AFR-ABC--4-AFR from STL billing message (DSV)-AU---", 202406, dbDSV, null, priceItemDSV_PD);
			});
		}

		IEnumerable<DataRow> GetBillingDbDetailedUsageRows(int period, LicenceDatabase db, ClientCompany clientCompany, ClientLicencePriceItem priceItem, string excludedSystemCodes = "")
		{
			var sqlClientCompany = clientCompany != null ? $"'{clientCompany.PK}'" : "NULL";
			var sql = $"select * from {BillingUsageSchema.GetBillingDbDetailedUsageQuery}({period}, '{db.DatabaseId}', {db.LD_DatabaseNumber}, {sqlClientCompany}, '{priceItem.PK}', '{excludedSystemCodes}')";
			return Utilities.GetDataTableFromQuery(sql).Rows.OfType<DataRow>();
		}

		void AssertGetBillingDbDetailedUsageRows(string message, string rowsAsTextExpected, int period, LicenceDatabase db, ClientCompany lcc, ClientLicencePriceItem l7)
		{
			var rowsAsText = string.Join("\r\n", GetBillingDbDetailedUsageRows(period, db, lcc, l7)
.Select(r => $"{r["TX_PriceItemCode"]}-{r["CompanyCode"]}-{r["BranchCode"]}-{r["TX_BillableCount"]}-{r["TX_Reference1"]}-{r["TX_Reference2"]}-{r["TX_Reference3"]}-{r["TX_Reference4"]}-{r["AdjustedUnitCount"]}").OrderBy(x => x));
			AssertEquals(message, rowsAsTextExpected, rowsAsText);
		}

		public void TestGetDatabaseBillingHostedLocations()
		{
			Action<string> assertRows = (expectedRows) =>
			{
				var rowsAsText = string.Join("\r\n", Utilities.GetDataTableFromQuery($"select * from EdiGetDatabaseBillingHostedLocations();").Rows.OfType<DataRow>().Select(r => $"{r["Code"]}-{r["IsCW"]}").OrderBy(x => x));
				AssertEquals(expectedRows, rowsAsText);
			};

			//read from registry
			var list = new CodeDescriptionBoolCollection();
			list.Add("Z10", (NoResString)"New Data Center 1", true);
			list.Add("Z11", (NoResString)"New Data Center 2", false);
			list.Add("Z12", (NoResString)"New Data Center 3", true);
			EDIDataRegistry.Instance.DatabaseHostedLocations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			assertRows(@"Z10-Y
Z11-N
Z12-Y");

			//default values, The default values of dbo.EdiGetDatabaseBillingHostedLocations()/Registry.DatabaseBillingHostedLocations.Default must be synchronized logically.
			Db.Connection.ExecuteNonQuery("DELETE dbo.StmData WHERE SD_Name = 'DatabaseBillingHostedLocations';");
			assertRows(@"CHI-Y
LON-Y
NCW-N
SYD-Y
TRA-N");
			var defaultValues = string.Join("\r\n", EDIDataRegistry.Instance.DatabaseHostedLocations.DefaultValue.OfType<CodeDescriptionBool>().Select(x => $"{x.Code}-{x.Bool}").OrderBy(x => x));
			assertRows(defaultValues);
		}

		public void TestGetBillingCountryGroups()
		{
			Action<string> assertRows = (expectedRows) =>
			{
				var rowsAsText = string.Join("\r\n", Utilities.GetDataTableFromQuery($"select * from EdiGetBillingCountryGroups();").Rows.OfType<DataRow>().Select(r => $"{r["BCG_CountryCode"]}-{r["BCG_MainCountryCode"]}").OrderBy(x => x));
				AssertEquals(expectedRows, rowsAsText);
			};

			//read from registry
			var list = new CodeDescriptionBoolCollection();
			list.Add("C1", (NoResString)"C9");
			list.Add("C2", (NoResString)"C9");
			list.Add("C5", (NoResString)"C8");
			list.Add("C7", (NoResString)"C8");
			EDIDataRegistry.Instance.BillingCountryGroups.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			assertRows(@"C1-C9
C2-C9
C5-C8
C7-C8");
			Db.Connection.ExecuteNonQuery("DELETE dbo.StmData WHERE SD_Name = 'BillingCountryGroup';");
			assertRows(@"CN-CN
HK-CN
MO-CN");

			//default values, The default values of dbo.GetBillingCountryGroups()/Registry.BillingCountryGroups.Default must be synchronized logically.
			var defaultValues = string.Join("\r\n", EDIDataRegistry.Instance.BillingCountryGroups.DefaultValue.OfType<ICodeDescription>().Select(x => $"{x.Code}-{x.Description}").OrderBy(x => x));
			assertRows(defaultValues);
		}

		public void TestUpdateContactCountry()
		{
			EServicesBillingTestHelper.CreateTable();

			var periodStart = BillingTestHelper.MonthToday;
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "SYD", "PRD");
			var db = lic.Database;
			var clientCompany1 = lic.ClientCompany;
			var clientCompany2 = BillingTestHelper.CreateClientCompany(db, "AKL");
			clientCompany1.LCC_RN_NKCountryCode = "AU";
			clientCompany2.LCC_RN_NKCountryCode = "NZ";
			var staff1 = BillingTestHelper.CreateClientStaff(db, "ALF", "Alf Alfa");
			staff1.LS_Email = "alf@test.com";
			var staff2 = BillingTestHelper.CreateClientStaff(db, "BOB", "Bob Builder");
			staff2.LS_Email = "bob@test.com";
			var contact1 = lic.Company.Header.Contacts.AddNew();
			var contact2 = lic.Company.Header.Contacts.AddNew();
			contact1.OC_ContactName = staff1.LS_FullName;
			contact2.OC_ContactName = staff2.LS_FullName;
			contact1.OC_Email = staff1.LS_Email;
			contact2.OC_Email = staff2.LS_Email;

			// Alf is on two other DBs
			var db2 = BillingTestHelper.CreateAnotherDatabase(lic, "DB2", false).Database;
			var db3 = BillingTestHelper.CreateAnotherDatabase(lic, "DB3", false).Database;
			var clientCompanyDb2 = BillingTestHelper.CreateClientCompany(db2, "SYD");
			var clientCompanyDb3 = BillingTestHelper.CreateClientCompany(db3, "SYD");
			clientCompanyDb2.LCC_RN_NKCountryCode = "AU";
			clientCompanyDb3.LCC_RN_NKCountryCode = "NZ";
			BillingTestHelper.CreateClientCompany(db3, "AKL");
			var staff1b = BillingTestHelper.CreateClientStaff(db2, "ALF", "Alf Alfa");
			staff1b.LS_Email = "alf@test.com";
			var staff1c = BillingTestHelper.CreateClientStaff(db3, "ALF", "Alf Alfa");
			staff1c.LS_Email = "alf@test.com";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", periodStart, "ENTSYDPRD", "", db.DatabaseId, clientCompany1.PK, "ALF", "Alf Alfa", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", periodStart, "ENTSYDDB2", "", db2.DatabaseId, clientCompanyDb2.PK, "ALF", "Alf Alfa", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", periodStart, "ENTSYDDB3", "", db3.DatabaseId, clientCompanyDb3.PK, "ALF", "Alf Alfa", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", periodStart, "ENTAKLPRD", "", db.DatabaseId, clientCompany2.PK, "BOB", "Bob Builder", "", "", null));
			EServicesBillingTestHelper.AddTransactions(infoList);

			Factory.Save();

			var cmd = Db.Connection.Command(BillingUsageSchema.UpdateContactCountry);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.AddParameter("@Period", SqlDbType.Int, periodStart.Year * 100 + periodStart.Month);
			cmd.ExecuteNonQuery();

			contact1.Reload();
			contact2.Reload();
			AssertEquals("AU", contact1.OC_RN_NKNationality);
			AssertEquals("NZ", contact2.OC_RN_NKNationality);

			// Make Alf have 2 out of 3 country NZ
			clientCompanyDb2.LCC_RN_NKCountryCode = "NZ";
			Factory.Save();

			cmd.ExecuteNonQuery();
			contact1.Reload();
			AssertEquals("no change if already defined", "AU", contact1.OC_RN_NKNationality);

			contact1.OC_RN_NKNationality = "";
			Factory.Save();
			cmd.ExecuteNonQuery();
			contact1.Reload();
			AssertEquals("NZ", contact1.OC_RN_NKNationality);
		}

		public void TestGetHostUsage()
		{
			EServicesBillingTestHelper.CreateTable();

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA", false);
			var db1 = lic1.Database;
			db1.LD_DatabaseNumber = 1983;
			db1.LD_HostedLocation = "SYD";
			db1.LD_HostDBName = "OdysseyAAASYD";
			db1.LD_LicenceType = "PRD";
			lic1.LA_AgreedLiveDate = new ZDateTime(2018, 1, 1);
			lic1.LA_IsActive = true;

			var clientCompany1 = db1.ClientCompanies.AddNew();
			clientCompany1.LCC_LD = db1.PK;
			clientCompany1.LCC_Code = "AAA";
			var clientNumber1 = db1.DatabaseId + ".SYD";

			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1.Company, "MEL");
			var db2 = lic2.Database;
			db2.LD_DatabaseNumber = 1984;
			db2.LD_HostedLocation = "MEL";
			db2.LD_HostDBName = "OdysseyAAAMEL";
			db2.LD_LicenceType = "TST";
			lic2.LA_AgreedLiveDate = new ZDateTime(2018, 1, 1);
			lic2.LA_IsActive = true;

			var clientCompany2 = db2.ClientCompanies.AddNew();
			clientCompany2.LCC_LD = db2.PK;
			clientCompany2.LCC_Code = "BBB";
			var clientNumber2 = db2.DatabaseId + ".MEL";

			Factory.Save();

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "STS", new ZDateTime(2018, 6, 30, 0, 0, 0), "AAASYDAAA", clientNumber1, db1.DatabaseId, clientCompany1.PK, "OdysseyAAASYD", "", null, null, null, "ENT", 4000000));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "STS", new ZDateTime(2018, 6, 30, 0, 0, 0), "AAASYDAAA", clientNumber1, db1.DatabaseId, clientCompany1.PK, "OdysseyAAASYD_SD001", "", null, null, null, "ENT", 2000000));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "STS", new ZDateTime(2018, 6, 30, 0, 0, 0), "AAASYDAAA", clientNumber1, db1.DatabaseId, clientCompany1.PK, "OdysseyAAASYD_UserRepository", "", null, null, null, "ENT", 1000000));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "STS", new ZDateTime(2018, 6, 30, 0, 0, 0), "AAASYDAAA", clientNumber1, db1.DatabaseId, clientCompany1.PK, "wced-apac-001701280923-ac3syd-001-pri", "", null, null, null, "ENT", 100));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "STS", new ZDateTime(2018, 6, 30, 0, 0, 0), "AAAMELBBB", clientNumber2, db2.DatabaseId, clientCompany2.PK, "OdysseyAAAMEL", "", null, null, null, "ENT", 2500000));
			EServicesBillingTestHelper.AddTransactions(infoList);

			UpdateChargeableUsage(new ZDateTime(2018, 6, 1), "HOS");

			var usagesAsText = string.Join("\r\n", Factory.Load<ClientChargeableUsage>(new ZQuery())
				.Select(x => $"{x.U1_PeriodStart.ToShortDateString()}-{x.U1_Code}-{x.U1_SubCode}-{x.U1_UnitCount}-{x.Database?.LD_ServerCode}-{x.CompanyCode}-{x.ClientCompany?.LCC_Code}")
				.OrderBy(x => x));

			AssertEquals(
@"01-Jun-18-HOS-#HA-7000100.0000-AAA-SYD-
01-Jun-18-HOS-#HD-5000000.0000-AAA-SYD-
01-Jun-18-HOS-#HE-2000100.0000-AAA-SYD-
01-Jun-18-HOS-#HN-2500000.0000-MEL-SYD-", usagesAsText);

			EServicesBillingTestHelper.DropTable();
		}

		public void TestEdiGetBillingDbUsage()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "DB1");
			var lic1a = BillingTestHelper.CreateAnotherLicence(lic1, "CO2");

			var lic2 = BillingTestHelper.CreateLicence(Factory, "EN2", "CO2", "DB2");

			Factory.Save();

			var db1 = lic1.Database;
			var clientNumber1 = db1.DatabaseId + ".CO1";
			var clientCompany1 = lic1.ClientCompany;
			var clientNumber1a = db1.DatabaseId + ".CO2";
			var clientCompany1a = lic1a.ClientCompany;

			var db2 = lic2.Database;
			var clientNumber2 = db2.DatabaseId + ".CO2";
			var clientCompany2 = lic2.ClientCompany;

			EServicesBillingTestHelper.CreateTable();

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2019, 1, 1, 0, 0, 0), "EN1CO1DB1", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "IT1", new ZDateTime(2019, 1, 1, 0, 0, 0), "EN1CO1DB1", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "GTS", new ZDateTime(2019, 1, 1, 0, 0, 0), "EN1CO1DB1", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "IT1", new ZDateTime(2019, 1, 2, 0, 0, 0), "EN1CO1DB1", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "IT1", new ZDateTime(2019, 1, 3, 0, 0, 0), "EN1CO2DB1", clientNumber1a, db1.DatabaseId, clientCompany1a.PK, "", "", null, null, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "IT1", new ZDateTime(2019, 1, 4, 0, 0, 0), "EN1CO2DB1", clientNumber1a, db1.DatabaseId, clientCompany1a.PK, "", "", null, null, null, "ENT", 5));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "IT1", new ZDateTime(2019, 1, 1, 0, 0, 0), "EN2CO2DB2", clientNumber2, db2.DatabaseId, clientCompany2.PK, "", "", null, null, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "GTS", new ZDateTime(2019, 1, 1, 0, 0, 0), "EN2CO2DB2", clientNumber2, db2.DatabaseId, clientCompany2.PK, "", "", null, null, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "IT1", new ZDateTime(2019, 1, 2, 0, 0, 0), "EN2CO2DB2", clientNumber2, db2.DatabaseId, clientCompany2.PK, "", "", null, null, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "IT1", new ZDateTime(2019, 1, 3, 0, 0, 0), "EN2CO2DB2", clientNumber2, db2.DatabaseId, clientCompany2.PK, "", "", null, null, null, "ENT", 1));

			EServicesBillingTestHelper.AddTransactions(infoList);

			var sql = "select * from EdiGetBillingDbUsage(201901, 'ACC', 'IT1', 0)";
			var table = new DataTable();
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.NewDataAdapter().Fill(table);
			}

			AssertEquals(3, table.Rows.Count);
			var row1 = table.Rows.Cast<DataRow>().Single(x => (Guid)x["LCC_PK"] == clientCompany1.PK.ToGuid());
			var row1a = table.Rows.Cast<DataRow>().Single(x => (Guid)x["LCC_PK"] == clientCompany1a.PK.ToGuid());
			var row2 = table.Rows.Cast<DataRow>().Single(x => (Guid)x["LCC_PK"] == clientCompany2.PK.ToGuid());

			AssertEquals("IT1 - 2", row1["SubCode"] + " - " + row1["UnitCount"]);
			AssertEquals("IT1 - 6", row1a["SubCode"] + " - " + row1a["UnitCount"]);
			AssertEquals("IT1 - 3", row2["SubCode"] + " - " + row2["UnitCount"]);

			var periodStart = new ZDateTime(2019, 1, 1);

			using (DbCommand cmd = Db.Connection.Command(BillingUsageSchema.ChargeableUsageUpdate))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@FirstDayOfMonth", SqlDbType.SmallDateTime, periodStart.ToDateTime());
				cmd.AddParameter("@PeriodStartTimeUtc", SqlDbType.SmallDateTime, periodStart.ToDateTime());
				cmd.AddParameter("@PeriodEndTimeUtc", SqlDbType.SmallDateTime, periodStart.AddMonths(1).ToDateTime());
				cmd.AddParameter("@Code", SqlDbType.VarChar, 3, "ACC");
				cmd.AddParameter("@BillingDbPriceItemCode", SqlDbType.VarChar, 3, "IT1");
				try
				{
					cmd.ExecuteProcedureWithReturnValue();
				}
				catch (Exception)
				{
				}
			}

			using (DbCommand cmd = Db.Connection.Command(BillingUsageSchema.ChargeableUsageUpdate))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@FirstDayOfMonth", SqlDbType.SmallDateTime, periodStart.ToDateTime());
				cmd.AddParameter("@PeriodStartTimeUtc", SqlDbType.SmallDateTime, periodStart.ToDateTime());
				cmd.AddParameter("@PeriodEndTimeUtc", SqlDbType.SmallDateTime, periodStart.AddMonths(1).ToDateTime());
				cmd.AddParameter("@Code", SqlDbType.VarChar, 3, "ACC");
				cmd.AddParameter("@BillingDbPriceItemCode", SqlDbType.VarChar, 3, "GTS");
				try
				{
					cmd.ExecuteProcedureWithReturnValue();
				}
				catch (Exception)
				{
				}
			}

			var usageList = new BusinessObjectFactory().Load<ClientChargeableUsage>(new ZQuery());
			AssertEquals(5, usageList.Length);
			var usage1 = usageList.Single(x => x.U1_LCC == clientCompany1.PK && x.U1_SubCode == "IT1");
			var usage1a = usageList.Single(x => x.U1_LCC == clientCompany1a.PK && x.U1_SubCode == "IT1");
			var usage2 = usageList.Single(x => x.U1_LCC == clientCompany2.PK && x.U1_SubCode == "IT1");
			var usageGTS1 = usageList.Single(x => x.U1_LCC == clientCompany1.PK && x.U1_SubCode == "GTS");
			var usageGTS2 = usageList.Single(x => x.U1_LCC == clientCompany2.PK && x.U1_SubCode == "GTS");

			AssertEquals("ACC - IT1 - 2.0000", usage1.U1_Code + " - " + usage1.U1_SubCode + " - " + usage1.U1_UnitCount);
			AssertEquals("ACC - IT1 - 6.0000", usage1a.U1_Code + " - " + usage1a.U1_SubCode + " - " + usage1a.U1_UnitCount);
			AssertEquals("ACC - IT1 - 3.0000", usage2.U1_Code + " - " + usage2.U1_SubCode + " - " + usage2.U1_UnitCount);

			AssertEquals("ACC - GTS - 1.0000", usageGTS1.U1_Code + " - " + usageGTS1.U1_SubCode + " - " + usageGTS1.U1_UnitCount);
			AssertEquals("ACC - GTS - 1.0000", usageGTS2.U1_Code + " - " + usageGTS2.U1_SubCode + " - " + usageGTS2.U1_UnitCount);
		}

		public void TestEdiGetBillingDbUsage_KeyRef()
		{
			EServicesBillingTestHelper.CreateTable();
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var db1 = lic1.Database;
			db1.LD_DatabaseNumber = 80239;

			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");
			var db2 = lic1.Database;
			db2.LD_DatabaseNumber = 45678;

			Factory.Save();

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF1", new ZDateTime(2019, 1, 1, 0, 0, 0), lic1, "Key Val 1", "Ref 2", "Ref 3", "Ref 4", "Ref 5", billableCount: 75));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF1", new ZDateTime(2019, 1, 1, 0, 0, 0), lic1, "Key Val 1", "Ref 2", "Ref 3", "Ref 4", "Ref 5", billableCount: 6));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF1", new ZDateTime(2019, 1, 1, 0, 0, 0), lic1, "Key Val 2", "Ref 2", "Ref 3", "Ref 4", "Ref 5", billableCount: 5));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF1", new ZDateTime(2019, 1, 1, 0, 0, 0), lic2, "Key Val 1", "Ref 2", "Ref 3", "Ref 4", "Ref 5", billableCount: 3));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF1", new ZDateTime(2019, 1, 1, 0, 0, 0), lic2, "Key Val 2", "Ref 2", "Ref 3", "Ref 4", "Ref 5", billableCount: 8));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF2", new ZDateTime(2019, 1, 1, 0, 0, 0), lic1, "Ref 1", "Key Val 1", "Ref 3", "Ref 4", "Ref 5", billableCount: 75));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF2", new ZDateTime(2019, 1, 1, 0, 0, 0), lic1, "Ref 1", "Key Val 1", "Ref 3", "Ref 4", "Ref 5", billableCount: 6));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF2", new ZDateTime(2019, 1, 1, 0, 0, 0), lic1, "Ref 1", "Key Val 2", "Ref 3", "Ref 4", "Ref 5", billableCount: 5));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF2", new ZDateTime(2019, 1, 1, 0, 0, 0), lic2, "Ref 1", "Key Val 1", "Ref 3", "Ref 4", "Ref 5", billableCount: 3));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF2", new ZDateTime(2019, 1, 1, 0, 0, 0), lic2, "Ref 1", "Key Val 2", "Ref 3", "Ref 4", "Ref 5", billableCount: 8));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF3", new ZDateTime(2019, 1, 1, 0, 0, 0), lic1, "Ref 1", "Ref 2", "Key Val 1", "Ref 4", "Ref 5", billableCount: 75));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF3", new ZDateTime(2019, 1, 1, 0, 0, 0), lic1, "Ref 1", "Ref 2", "Key Val 1", "Ref 4", "Ref 5", billableCount: 6));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF3", new ZDateTime(2019, 1, 1, 0, 0, 0), lic1, "Ref 1", "Ref 2", "Key Val 2", "Ref 4", "Ref 5", billableCount: 5));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF3", new ZDateTime(2019, 1, 1, 0, 0, 0), lic2, "Ref 1", "Ref 2", "Key Val 1", "Ref 4", "Ref 5", billableCount: 3));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF3", new ZDateTime(2019, 1, 1, 0, 0, 0), lic2, "Ref 1", "Ref 2", "Key Val 2", "Ref 4", "Ref 5", billableCount: 8));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF4", new ZDateTime(2019, 1, 1, 0, 0, 0), lic1, "Ref 1", "Ref 2", "Ref 3", "Key Val 1", "Ref 5", billableCount: 75));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF4", new ZDateTime(2019, 1, 1, 0, 0, 0), lic1, "Ref 1", "Ref 2", "Ref 3", "Key Val 1", "Ref 5", billableCount: 6));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF4", new ZDateTime(2019, 1, 1, 0, 0, 0), lic1, "Ref 1", "Ref 2", "Ref 3", "Key Val 2", "Ref 5", billableCount: 5));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF4", new ZDateTime(2019, 1, 1, 0, 0, 0), lic2, "Ref 1", "Ref 2", "Ref 3", "Key Val 1", "Ref 5", billableCount: 3));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF4", new ZDateTime(2019, 1, 1, 0, 0, 0), lic2, "Ref 1", "Ref 2", "Ref 3", "Key Val 2", "Ref 5", billableCount: 8));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF5", new ZDateTime(2019, 1, 1, 0, 0, 0), lic1, "Ref 1", "Ref 2", "Ref 3", "Ref 4", "Key Val 1", billableCount: 75));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF5", new ZDateTime(2019, 1, 1, 0, 0, 0), lic1, "Ref 1", "Ref 2", "Ref 3", "Ref 4", "Key Val 1", billableCount: 6));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF5", new ZDateTime(2019, 1, 1, 0, 0, 0), lic1, "Ref 1", "Ref 2", "Ref 3", "Ref 4", "Key Val 2", billableCount: 5));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF5", new ZDateTime(2019, 1, 1, 0, 0, 0), lic2, "Ref 1", "Ref 2", "Ref 3", "Ref 4", "Key Val 1", billableCount: 3));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF5", new ZDateTime(2019, 1, 1, 0, 0, 0), lic2, "Ref 1", "Ref 2", "Ref 3", "Ref 4", "Key Val 2", billableCount: 8));

			// Other data
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF1", new ZDateTime(2019, 2, 1, 0, 0, 0), lic1, "Key Val 1", "Ref 2", "Ref 3", "Ref 4", "Ref 5", billableCount: 75));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "RF1", new ZDateTime(2018, 12, 1, 0, 0, 0), lic1, "Key Val 1", "Ref 2", "Ref 3", "Ref 4", "Ref 5", billableCount: 75));

			EServicesBillingTestHelper.AddTransactions(infoList);

			var expected =
@"AAA-AAA-RF1--5-Key Val 2
AAA-AAA-RF1--81-Key Val 1
BBB-BBB-RF1--3-Key Val 1
BBB-BBB-RF1--8-Key Val 2";

			for (int refIndex = 1; refIndex <= 5; ++refIndex)
			{
				var table = Utilities.GetDataTableFromQuery("select LD_ServerCode, LCC_Code, SubCode, UnitCount, Reference1 from EdiGetBillingDbUsage(201901, 'CSC', 'RF" + refIndex + "', " + refIndex + ") u join dbo.LicenceDatabase db on u.LD_PK = db.LD_PK join dbo.ClientCompany co on u.LCC_PK = co.LCC_PK");
				var rows = string.Join("\r\n", table.Rows.OfType<DataRow>()
						.Select(r => $"{r["LD_ServerCode"]}-{r["LCC_Code"]}-{r["SubCode"]}--{r["UnitCount"]}-{r["Reference1"]}").OrderBy(x => x));
				AssertEquals(refIndex.ToString(), expected.Replace("RF1", "RF" + refIndex), rows);
			}
		}

		public void TestEdiGetBillingDbUsageCTRtoCTO()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "DB1");
			Factory.Save();

			var db1 = lic1.Database;
			var clientNumber1 = db1.DatabaseId + ".CO1";
			var clientCompany1 = lic1.ClientCompany;
			EServicesBillingTestHelper.CreateTable();

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CTR", "CTO", new ZDateTime(2021, 3, 1, 0, 0, 0), "EN1CO1DB1", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", null, null, null, "ENT", 1));
			EServicesBillingTestHelper.AddTransactions(infoList);

			var sql = "select * from EdiGetBillingDbUsage(202103, 'CTR', 'CTR', 0)";
			var table = new DataTable();
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.NewDataAdapter().Fill(table);
			}

			AssertEquals(1, table.Rows.Count);
			var row1 = table.Rows.Cast<DataRow>().Single(x => (Guid)x["LCC_PK"] == clientCompany1.PK.ToGuid());

			AssertEquals("CTR - 1", row1["SubCode"] + " - " + row1["UnitCount"]);

			var periodStart = new ZDateTime(2021, 3, 1);

			using (DbCommand cmd = Db.Connection.Command(BillingUsageSchema.ChargeableUsageUpdate))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@FirstDayOfMonth", SqlDbType.SmallDateTime, periodStart.ToDateTime());
				cmd.AddParameter("@PeriodStartTimeUtc", SqlDbType.SmallDateTime, periodStart.ToDateTime());
				cmd.AddParameter("@PeriodEndTimeUtc", SqlDbType.SmallDateTime, periodStart.AddMonths(1).ToDateTime());
				cmd.AddParameter("@Code", SqlDbType.VarChar, 3, "CTR");
				cmd.AddParameter("@BillingDbPriceItemCode", SqlDbType.VarChar, 3, "CTR");
				try
				{
					cmd.ExecuteProcedureWithReturnValue();
				}
				catch (Exception)
				{
				}
			}

			var usageList = new BusinessObjectFactory().Load<ClientChargeableUsage>(new ZQuery());
			AssertEquals(1, usageList.Length);
			var usage1 = usageList[0];

			AssertEquals("CTR - CTR - 1.0000", usage1.U1_Code + " - " + usage1.U1_SubCode + " - " + usage1.U1_UnitCount);
		}

		public void TestEdiUpdateLicenceDatabaseConsolidation()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "A01", true);
			var db1 = lic1.Database;
			db1.LD_DatabaseNumber = 1901;
			db1.LD_HostedLocation = "SYD";
			db1.LD_Product = "ABC";
			db1.LD_OH_WebAccessOrg = org1.PK;

			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1, "A02");
			var db2 = lic2.Database;
			db2.LD_DatabaseNumber = 1902;
			db2.LD_HostedLocation = "SYD";
			db2.LD_Product = "ABC";
			db2.LD_OH_WebAccessOrg = org2.PK;

			var lic3 = BillingTestHelper.CreateAnotherDatabase(lic1, "A03");
			var db3 = lic3.Database;
			db3.LD_DatabaseNumber = 1903;
			db3.LD_HostedLocation = "SYD";
			db3.LD_Product = "ABC";
			db3.LD_OH_WebAccessOrg = org1.PK;

			var lic4 = BillingTestHelper.CreateAnotherDatabase(lic1, "A04");
			var db4 = lic4.Database;
			db4.LD_DatabaseNumber = 1904;
			db4.LD_HostedLocation = "SYD";
			db4.LD_Product = "ABC";
			db4.LD_OH_WebAccessOrg = ZGuid.Empty;

			var lic5 = BillingTestHelper.CreateAnotherDatabase(lic1, "A05");
			var db5 = lic5.Database;
			db5.LD_DatabaseNumber = 1905;
			db5.LD_HostedLocation = "SYD";
			db5.LD_Product = "ABC";
			db5.LD_OH_WebAccessOrg = org3.PK;

			var lic6 = BillingTestHelper.CreateAnotherDatabase(lic1, "A06");
			var db6 = lic5.Database;
			db6.LD_DatabaseNumber = 1906;
			db6.LD_HostedLocation = "SYD";
			db6.LD_Product = "ABC";
			db6.LD_OH_WebAccessOrg = org1.PK;

			Factory.Save();
			Db.Connection.ExecuteNonQuery("EXEC EdiUpdateLicenceDatabaseConsolidation 202201, 'ABC,SMF';");

			db3.LD_OH_WebAccessOrg = org2.PK;
			Factory.Save();
			Db.Connection.ExecuteNonQuery("EXEC EdiUpdateLicenceDatabaseConsolidation 202201, 'ABC,SMF';");

			db6.LD_OH_WebAccessOrg = ZGuid.Empty;
			Factory.Save();
			Db.Connection.ExecuteNonQuery("EXEC EdiUpdateLicenceDatabaseConsolidation 202201, 'ABC,SMF';");

			db3.LD_OH_WebAccessOrg = org1.PK;
			db6.LD_OH_WebAccessOrg = org2.PK;
			Factory.Save();
			Db.Connection.ExecuteNonQuery("EXEC EdiUpdateLicenceDatabaseConsolidation 202202, 'ABC,SMF';");

			db2.LD_LicenceType = DatabaseTypes.Codes.Test;   //1902
			db3.LD_LicenceType = DatabaseTypes.Codes.Test;   //1903
			Factory.Save();
			Db.Connection.ExecuteNonQuery("EXEC EdiUpdateLicenceDatabaseConsolidation 202203, 'ABC,SMF';");

			var rowsAsString = string.Join("\r\n", Utilities.GetDataTableFromQuery(@"SELECT
EDH_Period, LD = D1.LD_DatabaseNumber, ConsolidatedDatabase = D2.LD_DatabaseNumber
FROM dbo.EdiLicenceDatabaseConsolidationHistory
JOIN dbo.LicenceDatabase D1 ON EDH_LD = D1.LD_PK
JOIN dbo.LicenceDatabase D2 ON EDH_LD_ConsolidatedDatabase = D2.LD_PK
ORDER BY 1,2,3;").Rows.OfType<DataRow>().Select(x => $"{x[0]}-{x[1]}-{x[2]}"));
			AssertEquals(@"202201-1901-1901
202201-1902-1902
202201-1903-1902
202202-1901-1901
202202-1902-1902
202202-1903-1901
202202-1906-1902
202203-1901-1901
202203-1906-1906", rowsAsString);

			rowsAsString = string.Join("\r\n", Utilities.GetDataTableFromQuery(@"SELECT LD_DatabaseNumber, SL_Reference
FROM dbo.StmALog
JOIN dbo.LicenceDatabase ON SL_Parent = LD_PK
WHERE SL_Table = 'LicenceDatabase' AND SL_SE_NKEvent = 'EDT' 
AND SL_Reference LIKE 'Database Consolidation Target set %'
ORDER BY LD_DatabaseNumber, SL_PostedTimeUtc;").Rows.OfType<DataRow>().Select(x => $"{x[0]}-{x[1]}"));
			AssertEquals(@"1901-Database Consolidation Target set to 1901 for Period 202201
1901-Database Consolidation Target set to 1901 for Period 202202
1901-Database Consolidation Target set to 1901 for Period 202203
1902-Database Consolidation Target set to 1902 for Period 202201
1902-Database Consolidation Target set to 1902 for Period 202202
1903-Database Consolidation Target set to 1901 for Period 202201
1903-Database Consolidation Target set to 1902 for Period 202201
1903-Database Consolidation Target set to 1901 for Period 202202
1906-Database Consolidation Target set to 1901 for Period 202201
1906-Database Consolidation Target set to 0 for Period 202201
1906-Database Consolidation Target set to 1902 for Period 202202
1906-Database Consolidation Target set to 1906 for Period 202203", rowsAsString);
		}

		public void TestEdiClientCompanyMerge()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA", false);
			var clientCompany1 = BillingTestHelper.CreateClientCompany(lic.Database, "CO1");
			var clientCompany2 = BillingTestHelper.CreateClientCompany(lic.Database, "CO2");
			BillingTestHelper.CreateChargeableUsage(Factory, "000", "", new ZDateTime(2019, 8, 1), clientCompany1, 1);
			Factory.Save();

			using (DbCommand cmd = Db.Connection.Command("EdiClientCompanyMerge"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@OldPk", SqlDbType.UniqueIdentifier, clientCompany1.PK.ToGuid());
				cmd.AddParameter("@NewPk", SqlDbType.UniqueIdentifier, clientCompany2.PK.ToGuid());
				cmd.ExecuteNonQuery();
			}

			var usages = new BusinessObjectFactory().Load<ClientChargeableUsage>(new ZQuery());
			AssertEquals(1, usages.Length);
			AssertEquals(clientCompany2.PK, usages[0].U1_LCC);

			var table = Utilities.GetDataTableFromQuery("select * from dbo.EdiClientCompanyMergeHistory");
			AssertEquals(1, table.Rows.Count);
			AssertEquals(clientCompany1.PK.ToGuid(), (Guid)table.Rows[0]["CMH_FromPK"]);
			AssertEquals(clientCompany2.PK.ToGuid(), (Guid)table.Rows[0]["CMH_ToPK"]);
			AssertEquals("CO1", (string)table.Rows[0]["CMH_FromCode"]);
			AssertEquals(true, table.Rows[0]["CMH_MergeTimeUtc"] is DateTime);
		}

		public void TestChargeableUsageUpdate_GenericUsage()
		{
			EServicesBillingTestHelper.CreateTable();

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA", false);
			var db = licence.Database;
			db.LD_DatabaseNumber = 1983;
			db.LD_HostedLocation = "SYD";
			db.LD_Product = "ABC";

			var clientCompany1 = db.ClientCompanies.AddNew();
			clientCompany1.LCC_LD = db.PK;
			clientCompany1.LCC_Code = "AAA";

			var clientCompany2 = db.ClientCompanies.AddNew();
			clientCompany2.LCC_LD = db.PK;
			clientCompany2.LCC_Code = "AA2";

			Factory.Save();

			var periodStart = new ZDateTime(2022, 11, 1);

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABC", "P01", periodStart.AddDays(1), "DDDAAASYD", "", db.DatabaseId, clientCompany1.PK, "AAA", "AAA Name", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABC", "P01", periodStart.AddDays(1), "DDDAAASYD", "", db.DatabaseId, clientCompany1.PK, "AAA", "AAA Name", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABC", "P02", periodStart.AddDays(1), "DDDAA2SYD", "", db.DatabaseId, clientCompany2.PK, "AA2", "AA2 Name", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABC", "P02", periodStart.AddDays(1), "DDDAA2SYD", "", db.DatabaseId, clientCompany2.PK, "AA2", "AA2 Name", "", "", null));
			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			UpdateChargeableUsage(new ZDateTime(2022, 11, 1), "ABC");

			var usagesAsText = string.Join("\r\n", Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_Code, "ABC"))
				.Select(x => $"{x.U1_PeriodStart.ToShortDateString()}-{x.U1_Code}-{x.U1_SubCode}-{x.U1_UnitCount}-{x.Database.LD_ServerCode}-{x.CompanyCode}-{x.ClientCompany.LCC_RN_NKCountryCode}")
				.OrderBy(x => x));

			AssertEquals(
@"01-Nov-22-ABC-P01-2.0000-AAA-AAA-
01-Nov-22-ABC-P02-2.0000-AAA-AA2-", usagesAsText);
		}

		public void TestChargeableUsageUpdate_GenericUsage_DatabaseConsolidation()
		{
			EServicesBillingTestHelper.CreateTable();

			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "A01", true);
			var db1 = lic1.Database;
			db1.LD_DatabaseNumber = 1901;
			db1.LD_HostedLocation = "SYD";
			db1.LD_Product = "ABC";

			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1, "A02");
			var db2 = lic2.Database;
			db2.LD_DatabaseNumber = 1902;
			db2.LD_HostedLocation = "SYD";
			db2.LD_Product = "ABC";

			var lic3 = BillingTestHelper.CreateAnotherDatabase(lic1, "A03");
			var db3 = lic3.Database;
			db3.LD_DatabaseNumber = 1903;
			db3.LD_HostedLocation = "SYD";
			db3.LD_Product = "ABC";

			var lic4 = BillingTestHelper.CreateAnotherDatabase(lic1, "A04");
			var db4 = lic4.Database;
			db4.LD_DatabaseNumber = 1904;
			db4.LD_HostedLocation = "SYD";
			db4.LD_Product = "ABC";

			Factory.Save();

			var clientCompany = lic1.ClientCompany;
			Db.Connection.ExecuteNonQuery(
$@"INSERT INTO dbo.EdiLicenceDatabaseConsolidationHistory(EDH_Period, EDH_LD, EDH_LD_ConsolidatedDatabase)
VALUES (202210, '{db1.PK}', '{db2.PK}');

INSERT INTO dbo.EdiLicenceDatabaseConsolidationHistory(EDH_Period, EDH_LD, EDH_LD_ConsolidatedDatabase)
VALUES (202211, '{db1.PK}', '{db1.PK}');

INSERT INTO dbo.EdiLicenceDatabaseConsolidationHistory(EDH_Period, EDH_LD, EDH_LD_ConsolidatedDatabase)
VALUES (202211, '{db4.PK}', '{db1.PK}');

INSERT INTO dbo.EdiLicenceDatabaseConsolidationHistory(EDH_Period, EDH_LD, EDH_LD_ConsolidatedDatabase)
VALUES (202212, '{db2.PK}', '{db3.PK}');
");

			var periodStart = new ZDateTime(2022, 11, 1);

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABC", "P01", periodStart.AddDays(1), "ENTCOMA01", "", db1.DatabaseId, clientCompany.PK, "REF1", "REF2", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABC", "P09", periodStart.AddDays(1), "ENTCOMA01", "", db1.DatabaseId, clientCompany.PK, "REF1", "REF2", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABC", "P02", periodStart.AddDays(1), "ENTCOMA02", "", db2.DatabaseId, clientCompany.PK, "REF1", "REF2", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABC", "P03", periodStart.AddDays(1), "ENTCOMA03", "", db3.DatabaseId, clientCompany.PK, "REF1", "REF2", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABC", "P04", periodStart.AddDays(1), "ENTCOMA04", "", db4.DatabaseId, clientCompany.PK, "REF1", "REF2", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABC", "P09", periodStart.AddDays(1), "ENTCOMA04", "", db4.DatabaseId, clientCompany.PK, "REF1", "REF2", "", "", null));
			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			UpdateChargeableUsage(new ZDateTime(2022, 11, 1), "ABC");

			var usagesAsText = string.Join("\r\n", Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_Code, "ABC"))
				.Select(x => $"{x.U1_PeriodStart.ToShortDateString()}-{x.U1_Code}-{x.U1_SubCode}-{x.U1_UnitCount}-{x.Database.LD_DatabaseNumber}-{x.CompanyCode}")
				.OrderBy(x => x));

			AssertEquals(
@"01-Nov-22-ABC-P01-1.0000-1901-COM
01-Nov-22-ABC-P02-1.0000-1902-COM
01-Nov-22-ABC-P03-1.0000-1903-COM
01-Nov-22-ABC-P04-1.0000-1901-COM
01-Nov-22-ABC-P09-2.0000-1901-COM", usagesAsText);
		}

		public void TestChargeableUsageUpdate_GenericUsage_MinimumFee()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var categories = new CodeDescriptionPairList(EDIDataRegistry.Instance.BillingUsageCategoryCodes.Value);
			categories.AddPair("SHP", "ABC Category");
			EDIDataRegistry.Instance.BillingUsageCategoryCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categories);

			var settings = new UsageBillingSettings();
			var priceLists = settings.PriceLists;
			var priceList2 = priceLists.AddNew();
			priceList2.ProductCode = "ABC";
			priceList2.RawUsageCategory = "SHP";
			priceList2.PriceListCode = "PPP";
			priceList2.Description = "ABC/SHP/PPP";
			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var feeSettings = new UsageMinimumFeeSettings();
			var fee = feeSettings.MinimumFeeList.AddNew();
			fee.ProductCode = "ABC";
			fee.PriceListCode = "PPP";
			fee.MinimumFeeCode = "MFC";
			EDIDataRegistry.Instance.UsageMinimumFeeSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, feeSettings);

			EServicesBillingTestHelper.CreateTable();

			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "A01", true);
			var db1 = lic1.Database;
			db1.LD_DatabaseNumber = 1901;
			db1.LD_HostedLocation = "SYD";
			db1.LD_Product = "ABC";

			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1, "A02");
			var db2 = lic2.Database;
			db2.LD_DatabaseNumber = 1902;
			db2.LD_HostedLocation = "SYD";
			db2.LD_Product = "ABC";

			var lic3 = BillingTestHelper.CreateAnotherDatabase(lic1, "A03");
			var db3 = lic3.Database;
			db3.LD_DatabaseNumber = 1903;
			db3.LD_HostedLocation = "SYD";
			db3.LD_Product = "ABC";

			var lic4 = BillingTestHelper.CreateAnotherDatabase(lic1, "A04");
			var db4 = lic4.Database;
			db4.LD_DatabaseNumber = 1904;
			db4.LD_HostedLocation = "SYD";
			db4.LD_Product = "ABC";

			Factory.Save();

			var clientCompany = lic1.ClientCompany;
			Db.Connection.ExecuteNonQuery(
$@"INSERT INTO dbo.EdiLicenceDatabaseConsolidationHistory(EDH_Period, EDH_LD, EDH_LD_ConsolidatedDatabase)
VALUES (202210, '{db1.PK}', '{db2.PK}');

INSERT INTO dbo.EdiLicenceDatabaseConsolidationHistory(EDH_Period, EDH_LD, EDH_LD_ConsolidatedDatabase)
VALUES (202211, '{db1.PK}', '{db1.PK}');

INSERT INTO dbo.EdiLicenceDatabaseConsolidationHistory(EDH_Period, EDH_LD, EDH_LD_ConsolidatedDatabase)
VALUES (202211, '{db4.PK}', '{db1.PK}');

INSERT INTO dbo.EdiLicenceDatabaseConsolidationHistory(EDH_Period, EDH_LD, EDH_LD_ConsolidatedDatabase)
VALUES (202212, '{db2.PK}', '{db3.PK}');
");

			var periodStart = new ZDateTime(2022, 11, 1);

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SHP", "P02", periodStart.AddDays(1), "ENTCOMA02", "", db2.DatabaseId, clientCompany.PK, "REF1", "REF2", "", "", null));
			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			UpdateChargeableUsage(new ZDateTime(2022, 11, 1), "SHP");

			var usagesAsText = string.Join("\r\n", Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_Code, "SHP"))
				.Select(x => $"{x.U1_PeriodStart.ToShortDateString()}-{x.U1_Code}-{x.U1_SubCode}-{x.U1_UnitCount}-{x.Database.LD_DatabaseNumber}-{x.CompanyCode}")
				.OrderBy(x => x));

			AssertEquals(
@"01-Nov-22-SHP-MFC-1.0000-1901-COM
01-Nov-22-SHP-MFC-1.0000-1903-COM
01-Nov-22-SHP-P02-1.0000-1902-COM", usagesAsText);
		}

		public void TestChargeableUsageUpdate_HostingDataAccess_HDA()
		{
			EServicesBillingTestHelper.CreateTable();

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA", false);
			var db = licence.Database;
			db.LD_DatabaseNumber = 1983;
			db.LD_HostedLocation = "SYD";
			db.LD_Product = "ABC";

			var db2 = BillingTestHelper.CreateAnotherDatabase(licence, "BBB").Database;
			db2.LD_DatabaseNumber = 1984;
			db2.LD_HostedLocation = "SYD";
			db2.LD_Product = "ABC";

			var clientCompany1 = db.ClientCompanies.AddNew();
			clientCompany1.LCC_LD = db.PK;
			clientCompany1.LCC_Code = "AAA";

			var clientCompany2 = db.ClientCompanies.AddNew();
			clientCompany2.LCC_LD = db.PK;
			clientCompany2.LCC_Code = "AA2";

			var prices = BillingTestHelper.CreatePriceList(licence.Company);
			prices.L6_SystemCode = "STL";
			var priceItemDataAcess = BillingTestHelper.AddPriceItem(prices, "#HG", BillingConstants.FeeType.PerMBPerMonthMin1GB, "", 0.01m);
			priceItemDataAcess.L7_Category = BillingConstants.BillingSystem.HostingDataAccess;

			var prices2 = BillingTestHelper.CreatePriceList(licence.Company);
			prices2.L6_SystemCode = "STL";
			var priceItemDataAcessByGB = BillingTestHelper.AddPriceItem(prices2, "#RG", BillingConstants.FeeType.PerGBPerMonthMin1GB, "", 10m);
			priceItemDataAcessByGB.L7_Category = BillingConstants.BillingSystem.HostingDataAccess;

			Factory.Save();

			var periodStart = new ZDateTime(2022, 11, 1);
			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("HOS", "#HG", periodStart.AddDays(1), "DDDAAASYD", "", db.DatabaseId, clientCompany1.PK, "AAA", "AAA Name", "", "", null, billableCount: 1024 * 3 + 345));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("HOS", "#HG", periodStart.AddDays(1), "DDDAAASYD", "", db.DatabaseId, clientCompany2.PK, "AAA", "AAA Name", "", "", null, billableCount: 588));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("HOS", "#HG", periodStart.AddDays(1), "DDDBBBSYD", "", db2.DatabaseId, ZGuid.Empty, "BBB", "BBB Name", "", "", null, billableCount: 102));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			UpdateChargeableUsage(new ZDateTime(2022, 11, 1), "HDA");

			var usagesAsText = string.Join("\r\n", Factory.Load<ClientChargeableUsage>(new ZQuery())
				.Select(x => $"{x.U1_PeriodStart.ToShortDateString()}-{x.U1_Code}-{x.U1_SubCode}-{x.U1_UnitCount}-{x.Database.LD_ServerCode}")
				.OrderBy(x => x));

			AssertEquals(
@"01-Nov-22-HDA-#HG-102.0000-BBB
01-Nov-22-HDA-#HG-4005.0000-AAA
01-Nov-22-HDA-#RG-1.0000-BBB
01-Nov-22-HDA-#RG-4.0000-AAA", usagesAsText);
		}

		public void TestGetStlGenericUsageSummary()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var settings = new UsageBillingSettings();
			var priceLists = settings.PriceLists;
			var priceList1 = priceLists.AddNew();
			priceList1.ProductCode = "ABC";
			priceList1.RawUsageCategory = "SAT";
			priceList1.PriceListCode = "DEF";
			priceList1.Description = "ABC - Price List #0";
			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic1.LA_LicenceAdvStdOth = "STL";
			lic1.Database.LD_Product = "ABC";
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			var db = lic1.Database;
			db.LD_DatabaseNumber = 342;
			db.LD_LicenceType = "PRD";

			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1, "AA2");
			lic2.LA_LicenceAdvStdOth = "STL";
			var db2 = lic2.Database;
			db2.LD_DatabaseNumber = 343;
			db2.LD_LicenceType = "PRD";
			db2.LD_Product = "ABC";
			Factory.Save();

			var org = lic1.Company.LicEnterprise.Organisation;

			var clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "AU");
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "AB2", db2.PK, org.PK, "", "AU");
			var priceHeader1 = org.LicCompany.PriceHeaders.AddNew();
			priceHeader1.L6_ValidFrom = new ZDateTime(2022, 1, 1);
			priceHeader1.L6_SystemCode = "DEF";
			var price1 = priceHeader1.Items.AddNew();
			price1.L7_Category = "SAT";
			price1.L7_Code = "P01";
			price1.L7_Description = "P01 - DESC";
			var price2 = priceHeader1.Items.AddNew();
			price2.L7_Category = "SAT";
			price2.L7_Code = "P02";
			price2.L7_Description = "P02";
			Factory.Save();
			var link = BillingTestHelper.CreatePriceLink(db, priceHeader1, new ZDateTime(2022, 1, 1));
			var link2 = BillingTestHelper.CreatePriceLink(db2, priceHeader1, new ZDateTime(2022, 1, 1));
			BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P01", new ZDateTime(2022, 11, 1), clientCompany, 23);
			BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P02", new ZDateTime(2022, 11, 1), clientCompany, 189);
			BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P01", new ZDateTime(2022, 11, 1), clientCompany2, 25);
			BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P02", new ZDateTime(2022, 11, 1), clientCompany2, 213);
			Factory.Save();

			AssertUsage(@"AA2-AAA-P01 - DESC-AAA-SAT-25
AA2-AAA-P02-AAA-SAT-213
AAA-AAA-P01 - DESC-AAA-SAT-23
AAA-AAA-P02-AAA-SAT-189");

			var org2 = BillingTestHelper.CreateOrganisation(Factory, "CCC");
			var delivery = BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK, "AUD");
			delivery.L9_SystemCode = "ABC";
			delivery.L9_ServerCode = db2.LD_ServerCode;
			delivery.L9_OH_InvoiceTo = org2.PK;
			Factory.Save();

			AssertUsage(@"AAA-AAA-P01 - DESC-AAA-SAT-23
AAA-AAA-P02-AAA-SAT-189");

			void AssertUsage(string usageExpected)
			{
				var rowsAsText = string.Join("\r\n", Utilities.GetDataTableFromQuery($"select * from EdiGetStlGenericUsageSummary('{org.PK}', '2022-11-1', 'ABC')").Rows.OfType<DataRow>()
					.Select(r => $"{r["LD_ServerCode"]}-{r["LE_EnterpriseCode"]}-{r["L7_Description"]}-{r["LCC_Code"]}-{r["U1_Code"]}-{r["U1_UnitCount"]}").OrderBy(x => x));
				AssertEquals(usageExpected, rowsAsText);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestEdiLoadAllGenericChargeableUsage()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "CO1", "CW1", false);
			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1, "ABC");
			var cw1Db = lic1.Database;
			var abcDb = lic2.Database;
			abcDb.LD_Product = "ABC";

			BillingTestHelper.SetInvoicing(lic1.Company.Header, Env.CurrentBranchPK);
			BillingTestHelper.SetInvoicing(lic2.Company.Header, Env.CurrentBranchPK);

			var u1 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "P01", periodStart, lic1, 10);
			var u2 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "P02", periodStart, lic1, 20);
			var u3 = BillingTestHelper.CreateChargeableUsage(Factory, "ABC", "P03", periodStart, lic2, 30);
			var u4 = BillingTestHelper.CreateChargeableUsage(Factory, "ABC", "P04", periodStart, lic2, 40);
			Factory.Save();

			using (var dataSet = new DataSet())
			{
				GenericUsageSet.LoadAllChargeableUsage(dataSet, periodStart, orgPk: lic2.Company.LC_OH, enterpriseCode: "ENT", "ABC", "ABC");

				var usageRows = dataSet.Tables[0].Rows.OfType<DataRow>().OrderBy(x => x["U1_SubCode"]).ToArray();
				AssertEquals(2, usageRows.Length);
				AssertEquals("P03", usageRows[0]["U1_SubCode"].ToString());
				AssertEquals("P04", usageRows[1]["U1_SubCode"].ToString());
				AssertEquals(u3.PK, DatabaseUsageSet.AsZGuid(usageRows[0]["U1_PK"]));
				AssertEquals(u4.PK, DatabaseUsageSet.AsZGuid(usageRows[1]["U1_PK"]));

				var dbRows = dataSet.Tables[1].Rows;
				AssertEquals(1, dbRows.Count);
				AssertEquals(lic2.LA_LD, DatabaseUsageSet.AsZGuid(dbRows[0]["LD_PK"]));
			}
		}

		public void TestGetChargeableUsageByLegacyIdFax()
		{
			Db.Connection.ExecuteNonQuery(@"INSERT INTO [dbo].[EdiExternalChargeableUsage]
		   ([EXU_PK]
		   ,[EXU_Period]
		   ,[EXU_Code]
		   ,[EXU_EnterpriseCode]
		   ,[EXU_CompanyCode]
		   ,[EXU_ServerCode]
		   ,[EXU_UnitCount])
	 VALUES
		   (NEWID() ,200909 ,'FAX' ,'ABC' ,'EDF' ,'HIG' ,9),
		   (NEWID() ,200910 ,'FAX' ,'ABC' ,'EDF' ,'HIG' ,10),
		   (NEWID() ,200910 ,'HOS' ,'ABC' ,'EDF' ,'HIG' ,11);");

			void assertRows(string rowsAsStringExpected)
			{
				var rowsAsString = string.Join("\r\n", Utilities.GetDataTableFromQuery("SELECT * FROM EdiGetChargeableUsageByLegacyIdFAX(200910, NULL, NULL)")
					.Rows.OfType<DataRow>().Select(r => $"{r["EnterpriseCode"]}-{r["CompanyCode"]}-{r["ServerCode"]}-{r["SubCode"]}-{r["UnitCount"]}"));
				AssertEquals(rowsAsStringExpected, rowsAsString);
			}

			assertRows(@"ABC-EDF-HIG--10");
		}

		public void TestGetStlUsages_CargoWiseNext()
		{
			EServicesBillingTestHelper.CreateTable();

			var stdLicence = BillingTestHelper.CreateLicence(Factory, "EDI", "EDI", "SYD", false);
			var stdPriceCompany = stdLicence.Company;

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "USR", "SHP", "IFG", "EFC");

			var cwnPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "SHD", "BRD");
			cwnPrices.L6_SystemCode = "CWN";
			var usageMap = cwnPrices.UsageMaps.AddNew();
			usageMap.PUM_PriceCategory = "CWN";
			usageMap.PUM_PriceCode = "SHD";
			usageMap.PUM_UsageCategory = "STL";
			usageMap.PUM_UsageCode = "SHD";
			var usageMap2 = cwnPrices.UsageMaps.AddNew();
			usageMap2.PUM_PriceCategory = "CWN";
			usageMap2.PUM_PriceCode = "BRD";
			usageMap2.PUM_UsageCategory = "STL";
			usageMap2.PUM_UsageCode = "BRD";

			var shd = cwnPrices.Items.FindByCode("SHD");
			shd.L7_Category = "CWN";
			var brd = cwnPrices.Items.FindByCode("BRD");
			brd.L7_Category = "CWN";

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA", false);
			var db = licence.Database;
			db.LD_DatabaseNumber = 1983;
			db.LD_HostedLocation = "SYD";
			db.LD_Product = ProductTypes.Codes.CargoWiseNext;

			var clientCompany1 = db.ClientCompanies.AddNew();
			clientCompany1.LCC_LD = db.PK;
			clientCompany1.LCC_Code = "AAA";
			var clientNumber = db.DatabaseId + ".SYD";
			Factory.Save();

			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdLicence.LicenceCode);

			var jh1 = Guid.NewGuid().ToString();
			var jh2 = Guid.NewGuid().ToString();
			var jh3 = Guid.NewGuid().ToString();
			var jh4 = Guid.NewGuid().ToString();
			var jh5 = Guid.NewGuid().ToString();
			var jh6 = Guid.NewGuid().ToString();
			var jh7 = Guid.NewGuid().ToString();
			var jh8 = Guid.NewGuid().ToString();

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			// normal SHP
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "SHP", new ZDateTime(2024, 1, 1, 0, 0, 0), "ENTCC1SRV", clientNumber, db.DatabaseId, clientCompany1.PK, "", "", null, null, reference5: Guid.NewGuid().ToString(), null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "SHP", new ZDateTime(2024, 1, 1, 0, 0, 0), "ENTCC2SRV", clientNumber, db.DatabaseId, clientCompany1.PK, "", "", null, null, reference5: Guid.NewGuid().ToString(), null, "ENT", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);
			UpdateChargeableUsage(new ZDateTime(2024, 1, 1), "STL");
			AssertUsages("normal SHP", @"01-Jan-24-STL-SHP-2.0000-AAA-AAA---0.0000-");

			// SHP + SHD
			infoList.Clear();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "SHP", new ZDateTime(2024, 1, 1, 0, 0, 0), "ENTCC1SRV", clientNumber, db.DatabaseId, clientCompany1.PK, "", "", null, reference4: null, reference5: jh1, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "SHP", new ZDateTime(2024, 1, 1, 0, 0, 0), "ENTCC1SRV", clientNumber, db.DatabaseId, clientCompany1.PK, "", "", null, reference4: null, reference5: jh2, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "SHP", new ZDateTime(2024, 1, 1, 0, 0, 0), "ENTCC1SRV", clientNumber, db.DatabaseId, clientCompany1.PK, "", "", null, reference4: null, reference5: jh3, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "SHD", new ZDateTime(2024, 1, 1, 0, 0, 0), "ENTCC1SRV", clientNumber, db.DatabaseId, clientCompany1.PK, "", reference2: "USD", reference3: "1.2345", reference4: "IMP", reference5: jh1, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "SHD", new ZDateTime(2024, 1, 1, 0, 0, 0), "ENTCC1SRV", clientNumber, db.DatabaseId, clientCompany1.PK, "", reference2: "AUD", reference3: "0.1489", reference4: "EXP", reference5: jh2, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "SHD", new ZDateTime(2024, 1, 1, 0, 0, 0), "ENTCC1SRV", clientNumber, db.DatabaseId, clientCompany1.PK, "", reference2: "USD", reference3: "2.5", reference4: "IMP", reference5: jh3, null, "ENT", 1));

			// SHP only
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "SHP", new ZDateTime(2024, 1, 1, 0, 0, 0), "ENTCC1SRV", clientNumber, db.DatabaseId, clientCompany1.PK, "", "", null, reference4: null, reference5: jh4, null, "ENT", 1));

			// SHD only
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "SHD", new ZDateTime(2024, 1, 1, 0, 0, 0), "ENTCC1SRV", clientNumber, db.DatabaseId, clientCompany1.PK, "", reference2: "GPB", reference3: "0.4785", reference4: null, reference5: jh5, null, "ENT", 1));

			// invalid SHD format
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "SHD", new ZDateTime(2024, 1, 1, 0, 0, 0), "ENTCC1SRV", clientNumber, db.DatabaseId, clientCompany1.PK, "", reference2: "1233456", reference3: "one~", reference4: null, reference5: "guid~~~", null, "ENT", 1));

			// EFC,IFG + BRD
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "EFC", new ZDateTime(2024, 1, 1, 0, 0, 0), "ENTCC1SRV", clientNumber, db.DatabaseId, clientCompany1.PK, "", "", null, reference4: jh7, reference5: jh7, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "IFG", new ZDateTime(2024, 1, 1, 0, 0, 0), "ENTCC1SRV", clientNumber, db.DatabaseId, clientCompany1.PK, "", "", null, reference4: jh8, reference5: jh8, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "BRD", new ZDateTime(2024, 1, 1, 0, 0, 0), "ENTCC1SRV", clientNumber, db.DatabaseId, clientCompany1.PK, "", reference2: "CNY", reference3: "18.34", reference4: null, reference5: jh7, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "BRD", new ZDateTime(2024, 1, 1, 0, 0, 0), "ENTCC1SRV", clientNumber, db.DatabaseId, clientCompany1.PK, "", reference2: "CAD", reference3: "6.48", reference4: "ALL", reference5: jh8, null, "ENT", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);
			UpdateChargeableUsage(new ZDateTime(2024, 1, 1), "STL");

			CombineAssertions(() =>
			{
				//mapping: default, not mappings yet
				AssertEquals(0, EDIDataRegistry.Instance.BillingDisbursementUsageMappings.Value.Count);
				AssertUsages("default, not mappings yet",
@"01-Jan-24-STL-BRD-2.0000-AAA-AAA---0.0000-
01-Jan-24-STL-EFC-1.0000-AAA-AAA---0.0000-
01-Jan-24-STL-IFG-1.0000-AAA-AAA---0.0000-
01-Jan-24-STL-SHD-5.0000-AAA-AAA---0.0000-
01-Jan-24-STL-SHP-6.0000-AAA-AAA---0.0000-");

				//mapping: only disbursement, no usages
				var mappings = new CodeDescriptionBoolCollection();
				mappings.Add("SHD", (NoResString)"AAA,BBB, CCC");
				mappings.Add("BRD", (NoResString)"AAA,BBB, CCC");
				EDIDataRegistry.Instance.BillingDisbursementUsageMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);
				UpdateChargeableUsage(new ZDateTime(2024, 1, 1), "STL");
				AssertUsages("mapping: only disbursement, no usages",
@"01-Jan-24-STL-EFC-1.0000-AAA-AAA---0.0000-
01-Jan-24-STL-IFG-1.0000-AAA-AAA---0.0000-
01-Jan-24-STL-SHP-6.0000-AAA-AAA---0.0000-");

				//correct mappings (SHD->SHP, BRD->EFC,IFG)
				mappings = new CodeDescriptionBoolCollection();
				mappings.Add("BRD", (NoResString)"AAA,BBB, EFC,IFG,CCC");
				mappings.Add("SHD", (NoResString)"AAA,BBB, SHP ,CCC");
				EDIDataRegistry.Instance.BillingDisbursementUsageMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);
				UpdateChargeableUsage(new ZDateTime(2024, 1, 1), "STL");
				AssertUsages("correct mappings (SHD->SHP, BRD->EFC,IFG)",
@"01-Jan-24-STL-BRD-1.0000-AAA-AAA--CAD-6.4800-ALL
01-Jan-24-STL-BRD-1.0000-AAA-AAA--CNY-18.3400-
01-Jan-24-STL-SHD-1.0000-AAA-AAA--AUD-0.1489-EXP
01-Jan-24-STL-SHD-2.0000-AAA-AAA--USD-3.7345-IMP
01-Jan-24-STL-SHP-3.0000-AAA-AAA---0.0000-");

				//mapping: no disbursement, only usages  (XXX -> SHP, YYY->EFC,IFG)
				mappings = new CodeDescriptionBoolCollection();
				mappings.Add("XXX", (NoResString)"AAA,BBB, SHP ,CCC");
				mappings.Add("YYY", (NoResString)"AAA,BBB, EFC,IFG,CCC");
				EDIDataRegistry.Instance.BillingDisbursementUsageMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);
				UpdateChargeableUsage(new ZDateTime(2024, 1, 1), "STL");
				AssertUsages("mapping: no disbursement, only usages  (XXX -> SHP, YYY->EFC,IFG)",
@"01-Jan-24-STL-BRD-2.0000-AAA-AAA---0.0000-
01-Jan-24-STL-EFC-1.0000-AAA-AAA---0.0000-
01-Jan-24-STL-IFG-1.0000-AAA-AAA---0.0000-
01-Jan-24-STL-SHD-5.0000-AAA-AAA---0.0000-
01-Jan-24-STL-SHP-6.0000-AAA-AAA---0.0000-");
			});

			void AssertUsages(string message, string expectedUsages)
			{
				var query = new ZDBOnlyQuery(typeof(ClientChargeableUsage));
				var usagesAsText = string.Join("\r\n", new BusinessObjectFactory().Load<ClientChargeableUsage>(query)
					.Select(x => $"{x.U1_PeriodStart.ToShortDateString()}-{x.U1_Code}-{x.U1_SubCode}-{x.U1_UnitCount}-{x.Database.LD_ServerCode}-{x.CompanyCode}-{x.ClientCompany.LCC_RN_NKCountryCode}-{x.U1_RX_NKCurrency}-{x.U1_TotalPrice}-{x.U1_Direction}")
					.OrderBy(x => x));
				AssertEquals(message, expectedUsages, usagesAsText);
			}
		}

		EServicesBillingTestHelper.RawUsageInfo CreateBorderWiseUsage(
			LicenceHeader lic,
			EDIOrgContact contact,
			string priceCode,
			ZDateTime serviceTimeUtc,
			string machineId,
			string editionName,
			string editionCountry)
		{
			var usage = new EServicesBillingTestHelper.RawUsageInfo("BOR", priceCode, serviceTimeUtc, lic.LicenceCode, lic.Database.DatabaseId, lic.Database.DatabaseId,
				ZGuid.Empty, lic.Company.LC_OH.ToString(), contact.PK.ToString(), machineId, editionName, editionCountry, null, billableCount: 1);

			return usage;
		}
	}
}

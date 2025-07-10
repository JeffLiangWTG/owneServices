using System;
using System.Linq;
using System.Net.NetworkInformation;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class RegistryItemTagExtensionsTest : TransactionedTestCase
	{
		public void TestCheckItemIsVisible()
		{
			TestCaseHelper.ClearTable(AccCFXUpliftConfigurationViewSchema.Constants.TableName);
			using (RowFactory.SetCachedTables())
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();

				GlbCompanyCollection companies = new GlbCompanyCollection(factory);
				IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All, RegistryOptions.IsHidden);
				RegistryItemTagForTest item = new RegistryItemTagForTest(regItem);
				Assert("Item should not be visible", !item.CheckItemIsVisible(companies));

				regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint2", RegistryStorageFlags.Company);
				item = new RegistryItemTagForTest(regItem);
				Assert("Item should be visible", item.CheckItemIsVisible(companies));

				// Use current company's PK as demo company PK for testing only
				// because it's the only PK in the database we can't delete while Enterprise is running
				Guid demoCompanyGuid = Env.CurrentCompany.PK;
				Guid currentCountryPK = Env.CurrentCompany.Country.PK;
				ZString currentCountryCode = Env.CurrentCompany.Country.Code;
				regItem.CountryFilterPKs = new[] { currentCountryPK };

				TestCaseHelper.ClearTable(AccChargeCodeSchema.Constants.TableName);
				TestCaseHelper.ClearTable(AccTaxRateSchema.Constants.TableName);
				TestCaseHelper.ClearTable(RefExchangeRateSchema.Constants.TableName);
				TestCaseHelper.ClearTable(TagRuleSchema.Constants.TableName);
				SetTableColumnToNull(GlbStaffSchema.Constants.TableName, GlbStaffSchema.Constants.GS_GB_HomeBranch, "GS");
				SetTableColumnToNull(OrgCompanyDataSchema.Constants.TableName, OrgCompanyDataSchema.Constants.OB_GB_ControllingBranch, "OB");

				GlbBranchCollection branches = new GlbBranchCollection(factory, new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK));
				branches.Load();
				branches.RemoveAndDeleteAll();

				TestCaseHelper.ClearTable(OrgRateTariffLevelSchema.Constants.TableName);
				TestCaseHelper.ClearTable(AccAPAccountDetailsSchema.Constants.TableName);
				TestCaseHelper.ClearTable(OrgCompanyDataSchema.Constants.TableName);

				companies = new GlbCompanyCollection(factory, new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, demoCompanyGuid));
				companies.DeleteAll();

				companies = new GlbCompanyCollection(factory);

				AssertEquals("There should only be one company", 1, companies.Count);
				AssertEquals("Only the Demo Company should be loaded", demoCompanyGuid, companies[0].PK.ToGuid());
				Assert("Item should be visible", item.CheckItemIsVisible(companies));

				GlbCompany testCompany = factory.NewWithValidTestData<GlbCompany>();
				testCompany.GC_Code = "ABC";

				String newCountryCode = currentCountryCode != "AU" ? "AU" : "US";
				testCompany.GC_RN_NKCountryCode = newCountryCode;
				companies = new GlbCompanyCollection(factory, new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, demoCompanyGuid));
				factory.Save();
				Assert("Item should not be visible", !item.CheckItemIsVisible(companies));

				var testCompany2 = factory.NewWithValidTestData<GlbCompany>();
				testCompany2.GC_Code = "XYZ";
				testCompany2.GC_RN_NKCountryCode = currentCountryCode;
				factory.Save();
				Assert("Item should be visible", item.CheckItemIsVisible(companies));

				regItem.Options = RegistryOptions.IsOnlyForDevelopers;
				AssertEquals("Item should be visible to developers.", true, item.CheckItemIsVisible(companies));
				GlbStaff.CurrentUser.GS_IsDeveloper = false;
				AssertEquals("Item should not be visible to non-developers.", false, item.CheckItemIsVisible(companies));

				regItem.Options = RegistryOptions.IsOnlyForSupport;
				GlbStaff.CurrentUser.GS_LoginName = "sysadmin";
				AssertEquals("Item should not be visible to non-supporters.", false, item.CheckItemIsVisible(companies));
				GlbStaff.CurrentUser.GS_LoginName = User.SupportUserName;
				AssertEquals("Item should be visible to support.", true, item.CheckItemIsVisible(companies));

				regItem.Options = RegistryOptions.Default;
				regItem.CountryFilterPKs = Enumerable.Empty<Guid>();
				AssertEquals("Item should be visible when countryfilter pks is empty", true, item.CheckItemIsVisible(companies));

				regItem.CountryFilterPKs = null;
				AssertExceptionThrown("Exception thrown when CountryFilterPKs is null. If you want to allow all countries visible (aka no filter at all), set it to Enumerable.Empty", typeof(ArgumentNullException), delegate
				{ item.CheckItemIsVisible(companies); });
			}
		}

		public void TestCheckItemIsVisible_IsForCargoWiseOnly()
		{
			var propeties = new Mock<IPGlobalProperties>(MockBehavior.Strict);
			propeties.SetupSequence(m => m.DomainName).Returns("BLAH").Returns("corporate.cargowise.com");

			var regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint2", RegistryStorageFlags.Company);
			regItem.Options = RegistryOptions.IsOnlyForCargoWise;

			var item = new Mock<RegistryItemTag>(regItem) { CallBase = true };
			item.Setup(m => m.GetIPGlobalProperties()).Returns(propeties.Object);
			Assert(!item.Object.CheckItemIsVisible(new GlbCompanyCollection(new BusinessObjectFactory())));
			Assert(item.Object.CheckItemIsVisible(new GlbCompanyCollection(new BusinessObjectFactory())));

			propeties.VerifyAll();
			item.VerifyAll();
			item.Verify(m => m.GetIPGlobalProperties(), Times.Exactly(2));
		}

		public void TestCheckItemIsVisible_IsForControllerOnly()
		{
			IRegistryItem registryItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All, RegistryOptions.IsOnlyForController);
			RegistryItemTagForTest item = new RegistryItemTagForTest(registryItem);

			BusinessObjectFactory factory = new BusinessObjectFactory { RefreshEnabled = false };

			GlbStaff staff1 = factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "SS1";
			staff1.GS_LoginName = "SS1";

			GlbStaff staff2 = factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "SS2";
			staff2.GS_LoginName = "SS2";
			staff2.GS_IsController = true;

			factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext("SS1", GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Assert("Should not be visible for ordinal user", !item.CheckItemIsVisible(new GlbCompanyCollection(new BusinessObjectFactory())));
			}

			using (EnvProxy.Instance.SetTemporaryUserContext("SS2", GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Assert("Should be visible for controller", item.CheckItemIsVisible(new GlbCompanyCollection(new BusinessObjectFactory())));
			}
		}

		public void TestCheckItemIsVisible_DbHints()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };

			var item = new StringRegistryItem("Item1", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.Company);
			item.CountryFilterPKs = [Env.CurrentCompany.PK];
			var itemTag = new RegistryItemTagForTest(item);

			var testCompany1 = factory.NewWithValidTestData<GlbCompany>();
			var testCompany2 = factory.NewWithValidTestData<GlbCompany>();
			var testCompany3 = factory.NewWithValidTestData<GlbCompany>();
			var testCompany4 = factory.NewWithValidTestData<GlbCompany>();
			var testCompany5 = factory.NewWithValidTestData<GlbCompany>();
			var testCompany6 = factory.NewWithValidTestData<GlbCompany>();
			var testCompany7 = factory.NewWithValidTestData<GlbCompany>();
			var testCompany8 = factory.NewWithValidTestData<GlbCompany>();
			var testCompany9 = factory.NewWithValidTestData<GlbCompany>();
			var testCompany10 = factory.NewWithValidTestData<GlbCompany>();

			var companies = new GlbCompanyCollection(factory, new ZQuery());
			factory.Save();

			// Simulate old IsVisible method (registryItemVisibility is null)
			var commandCountInOldWay = GetFactoryHints(() => itemTag.CheckItemIsVisible(companies));

			// Simulate new IsVisible method (registryItemVisibility is not null)
			var commandCountInNewWay = 0;
			using (var registryItemVisibility = new RegistryForm())
			{
				commandCountInNewWay = GetFactoryHints(() => itemTag.CheckItemIsVisible(companies, registryItemVisibility));
			}

			AssertLessThan($"The count in new way {commandCountInNewWay} should less than old way {commandCountInOldWay}", commandCountInNewWay, commandCountInOldWay);
		}

		int GetFactoryHints(Action action)
		{
			var commandCountBefore = RegistryLoadingHelper.RegistryLoadCount;
			action();
			var commandCountAfter = RegistryLoadingHelper.RegistryLoadCount;

			return commandCountAfter - commandCountBefore;
		}

		#region Implementation

		void SetTableColumnToNull(string tableName, string columnName, string tablePrefix)
		{
			string sqlText = string.Format("UPDATE {0} SET {1} = NULL, {2}_SystemLastEditTimeUtc = GetUtcDate(), {2}_SystemLastEditUser = 'E'", tableName, columnName, tablePrefix);
			Db.Connection.ExecuteNonQuery(sqlText); // Need to manually update all columns for the table to null for tests.
		}

		protected override void TearDown()
		{
			EnvProxy.SetHostedLocationForTest(null);
			base.TearDown();
		}

		#endregion
	}
}

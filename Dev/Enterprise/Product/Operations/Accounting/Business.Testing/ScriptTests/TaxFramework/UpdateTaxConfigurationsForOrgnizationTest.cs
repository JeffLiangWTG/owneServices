using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.TaxFramework
{
	class UpdateTaxConfigurationsForOrgnizationTest : ScriptTest
	{
		#region Insert

		public void TestInsertOrgTaxConfigurationsAR()
		{
			AssertInsertOrgTaxConfigurationsCore(true);
		}

		public void TestInsertOrgTaxConfigurationsAP()
		{
			AssertInsertOrgTaxConfigurationsCore(false);
		}

		void AssertInsertOrgTaxConfigurationsCore(bool isReceivable)
		{
			PrepareTestData(isReceivable);

			var orgTaxConfigCom1_TP = AccountingTestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigCom1);
			orgTaxConfigCom1_TP.OTC_RecoverTax = true;
			var orgTaxConfigBrn2_TP = AccountingTestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigBrn2);
			orgTaxConfigBrn2_TP.OTC_IsThresholdUsed = true;
			Factory.Save();

			var collection = Factory.Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals("Pre-condition", 2, collection.Length);
			var configCom1_TP = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			var configBrn2_TP = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			AssertOrgTaxConfigEquals(configCom1_TP, true, false, true);
			AssertOrgTaxConfigEquals(configBrn2_TP, true, true, false);

			GlbStaff.CurrentUser.GS_Code = AnotherUserCode;
			RunScript(template.PK.ToGuid());

			collection = Factory.CreateNewFactory().Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals(6, collection.Length);
			var configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configCom1_ObB = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			configCom1_TP = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			configBrn2_TP = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			AssertOrgTaxConfigEquals(configCom1_TP, true, false, true, "Template tax config is unchanged");
			AssertOrgTaxConfigEquals(configBrn2_TP, true, true, false, "Template tax config is unchanged");
			AssertOrgTaxConfigEquals(configCom1_ObA, true, false, true, "Org-A has new org tax config added", lastEditUser: AnotherUserCode, createUser: AnotherUserCode);
			AssertOrgTaxConfigEquals(configBrn2_ObA, true, true, false, "Org-A has new org tax config added", lastEditUser: AnotherUserCode, createUser: AnotherUserCode);
			AssertOrgTaxConfigEquals(configCom1_ObB, true, false, true, "Org-B has new org tax config added", lastEditUser: AnotherUserCode, createUser: AnotherUserCode);
			AssertOrgTaxConfigEquals(configBrn2_ObB, true, true, false, "Org-B has new org tax config added", lastEditUser: AnotherUserCode, createUser: AnotherUserCode);
		}

		#endregion

		#region Update

		public void TestUpdateOrgTaxConfigurationsAR()
		{
			AssertUpdateOrgTaxConfigurationsCore(true);
		}

		public void TestUpdateOrgTaxConfigurationsAP()
		{
			AssertUpdateOrgTaxConfigurationsCore(false);
		}

		void AssertUpdateOrgTaxConfigurationsCore(bool isReceivable)
		{
			PrepareTestData(isReceivable);

			var orgTaxConfigCom1_ObA = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfigCom1, companyDataA);
			orgTaxConfigCom1_ObA.OTC_IsThresholdUsed = true;
			orgTaxConfigCom1_ObA.OTC_RecoverTax = true;
			var orgTaxConfigBrn2_ObA = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn2, companyDataA);
			orgTaxConfigBrn2_ObA.OTC_IsThresholdUsed = true;
			orgTaxConfigBrn2_ObA.OTC_RecoverTax = true;

			var orgTaxConfigCom1_ObB = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfigCom1, companyDataB);
			orgTaxConfigCom1_ObB.OTC_IsActive = false;
			var orgTaxConfigBrn2_ObB = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn2, companyDataB);
			orgTaxConfigBrn2_ObB.OTC_IsActive = false;

			var orgTaxConfigCom1_TP = AccountingTestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigCom1);
			orgTaxConfigCom1_TP.OTC_RecoverTax = true;
			var orgTaxConfigBrn2_TP = AccountingTestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigBrn2);
			orgTaxConfigBrn2_TP.OTC_IsThresholdUsed = true;
			Factory.Save();

			var collection = Factory.Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals("Pre-condition", 6, collection.Length);
			var configCom1_TP = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			var configBrn2_TP = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			var configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configCom1_ObB = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			AssertOrgTaxConfigEquals(configCom1_TP, true, false, true);
			AssertOrgTaxConfigEquals(configBrn2_TP, true, true, false);
			AssertOrgTaxConfigEquals(configCom1_ObA, true, true, true);
			AssertOrgTaxConfigEquals(configBrn2_ObA, true, true, true);
			AssertOrgTaxConfigEquals(configCom1_ObB, false, false, false);
			AssertOrgTaxConfigEquals(configBrn2_ObB, false, false, false);

			GlbStaff.CurrentUser.GS_Code = AnotherUserCode;
			RunScript(template.PK.ToGuid());

			collection = Factory.CreateNewFactory().Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals(6, collection.Length);
			configCom1_TP = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			configBrn2_TP = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			configBrn2_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			configCom1_ObB = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			configBrn2_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			AssertOrgTaxConfigEquals(configCom1_TP, true, false, true, "Template tax config is unchanged");
			AssertOrgTaxConfigEquals(configBrn2_TP, true, true, false, "Template tax config is unchanged");
			AssertOrgTaxConfigEquals(configCom1_ObA, true, false, true, "Org-A has old org tax config updated", lastEditUser: AnotherUserCode);
			AssertOrgTaxConfigEquals(configBrn2_ObA, true, true, false, "Org-A has old org tax config updated", lastEditUser: AnotherUserCode);
			AssertOrgTaxConfigEquals(configCom1_ObB, true, false, true, "Org-B has old org tax config updated", lastEditUser: AnotherUserCode);
			AssertOrgTaxConfigEquals(configBrn2_ObB, true, true, false, "Org-B has old org tax config updated", lastEditUser: AnotherUserCode);
		}

		#endregion

		#region Deactive

		public void TestDeactivateOrgTaxConfigurationsAR()
		{
			AssertDeactivateOrgTaxConfigurationsCore(true);
		}

		public void TestDeactivateOrgTaxConfigurationsAP()
		{
			AssertDeactivateOrgTaxConfigurationsCore(false);
		}

		void AssertDeactivateOrgTaxConfigurationsCore(bool isReceivable)
		{
			PrepareTestData(isReceivable);

			var orgTaxConfigCom1_ObA = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfigCom1, companyDataA);
			orgTaxConfigCom1_ObA.OTC_IsThresholdUsed = true;
			orgTaxConfigCom1_ObA.OTC_RecoverTax = true;
			var orgTaxConfigBrn2_ObA = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn2, companyDataA);
			orgTaxConfigBrn2_ObA.OTC_IsThresholdUsed = true;
			orgTaxConfigBrn2_ObA.OTC_RecoverTax = true;

			var orgTaxConfigCom1_ObB = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfigCom1, companyDataB);
			var orgTaxConfigBrn2_ObB = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn2, companyDataB);
			Factory.Save();

			var collection = Factory.Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals("Pre-condition", 4, collection.Length);
			var configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configCom1_ObB = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			AssertOrgTaxConfigEquals(configCom1_ObA, true, true, true);
			AssertOrgTaxConfigEquals(configBrn2_ObA, true, true, true);
			AssertOrgTaxConfigEquals(configCom1_ObB, true, false, false);
			AssertOrgTaxConfigEquals(configBrn2_ObB, true, false, false);

			GlbStaff.CurrentUser.GS_Code = AnotherUserCode;
			RunScript(template.PK.ToGuid());

			collection = Factory.CreateNewFactory().Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals(4, collection.Length);
			configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			configBrn2_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			configCom1_ObB = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			configBrn2_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			AssertOrgTaxConfigEquals(configCom1_ObA, false, true, true, "Org-A has old org tax config deactived", lastEditUser: AnotherUserCode);
			AssertOrgTaxConfigEquals(configBrn2_ObA, false, true, true, "Org-A has old org tax config deactived", lastEditUser: AnotherUserCode);
			AssertOrgTaxConfigEquals(configCom1_ObB, false, false, false, "Org-B has old org tax config deactived", lastEditUser: AnotherUserCode);
			AssertOrgTaxConfigEquals(configBrn2_ObB, false, false, false, "Org-B has old org tax config deactived", lastEditUser: AnotherUserCode);
		}

		#endregion

		#region Mixed Operations

		public void TestMixedOperationOrgTaxConfigurationsAR()
		{
			AssertMixedOperationOrgTaxConfigurationsCore(true);
		}

		public void TestMixedOperationOrgTaxConfigurationsAP()
		{
			AssertMixedOperationOrgTaxConfigurationsCore(false);
		}

		void AssertMixedOperationOrgTaxConfigurationsCore(bool isReceivable)
		{
			PrepareTestData(isReceivable);

			var orgTaxConfigCom1_ObA = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfigCom1, companyDataA);
			orgTaxConfigCom1_ObA.OTC_IsThresholdUsed = true;
			orgTaxConfigCom1_ObA.OTC_RecoverTax = true;

			var orgTaxConfigBrn2_ObB = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn2, companyDataB);

			var orgTaxConfigBrn1_ObA = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn1, companyDataA);
			var orgTaxConfigBrn1_ObB = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn1, companyDataB);

			var orgTaxConfigBrn1_ObB_NonCurrentCompany = AccountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfigBrn1, companyDataB_NonCurrentCompany);
			Factory.Save();

			var orgTaxConfigCom1_TP = AccountingTestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigCom1);
			orgTaxConfigCom1_TP.OTC_RecoverTax = true;
			var orgTaxConfigBrn2_TP = AccountingTestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfigBrn2);
			orgTaxConfigBrn2_TP.OTC_IsThresholdUsed = true;
			Factory.Save();

			var collection = Factory.Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals("Pre-condition", 7, collection.Length);
			var configCom1_TP = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			var configBrn2_TP = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			var configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			var configBrn1_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configBrn1_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			var configBrn1_ObB_NonCurrentCompany = collection.Single(x => x.OTC_ETC == taxConfigBrn1.PK && x.OTC_OB == companyDataB_NonCurrentCompany.PK && x.OTC_OCT.IsEmpty);
			AssertOrgTaxConfigEquals(configCom1_TP, true, false, true);
			AssertOrgTaxConfigEquals(configBrn2_TP, true, true, false);
			AssertOrgTaxConfigEquals(configCom1_ObA, true, true, true);
			AssertOrgTaxConfigEquals(configBrn2_ObB, true, false, false);
			AssertOrgTaxConfigEquals(configBrn1_ObA, true, false, false);
			AssertOrgTaxConfigEquals(configBrn1_ObB, true, false, false);
			AssertOrgTaxConfigEquals(configBrn1_ObB_NonCurrentCompany, true, false, false);

			GlbStaff.CurrentUser.GS_Code = AnotherUserCode;
			RunScript(template.PK.ToGuid());

			collection = Factory.CreateNewFactory().Load<AccOrgTaxConfiguration>(new ZQuery());
			AssertEquals(9, collection.Length);
			configCom1_TP = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			configBrn2_TP = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB.IsEmpty && x.OTC_OCT == template.PK);
			configCom1_ObA = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configBrn2_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			var configCom1_ObB = collection.Single(x => x.OTC_ETC == taxConfigCom1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			configBrn2_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn2.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			configBrn1_ObA = collection.Single(x => x.OTC_ETC == taxConfigBrn1.PK && x.OTC_OB == companyDataA.PK && x.OTC_OCT.IsEmpty);
			configBrn1_ObB = collection.Single(x => x.OTC_ETC == taxConfigBrn1.PK && x.OTC_OB == companyDataB.PK && x.OTC_OCT.IsEmpty);
			configBrn1_ObB_NonCurrentCompany = collection.Single(x => x.OTC_ETC == taxConfigBrn1.PK && x.OTC_OB == companyDataB_NonCurrentCompany.PK && x.OTC_OCT.IsEmpty);
			AssertOrgTaxConfigEquals(configCom1_TP, true, false, true, "Template config is unchanged");
			AssertOrgTaxConfigEquals(configBrn2_TP, true, true, false, "Template config is unchanged");
			AssertOrgTaxConfigEquals(configCom1_ObA, true, false, true, "Org-A has old org tax config updated", lastEditUser: AnotherUserCode);
			AssertOrgTaxConfigEquals(configBrn2_ObA, true, true, false, "Org-A has new org tax config added", lastEditUser: AnotherUserCode, createUser: AnotherUserCode);
			AssertOrgTaxConfigEquals(configCom1_ObB, true, false, true, "Org-B has new org tax config added", lastEditUser: AnotherUserCode, createUser: AnotherUserCode);
			AssertOrgTaxConfigEquals(configBrn2_ObB, true, true, false, "Org-B has old org tax config updated", lastEditUser: AnotherUserCode);
			AssertOrgTaxConfigEquals(configBrn1_ObA, false, false, false, "Org-A has old org tax config deactived", lastEditUser: AnotherUserCode);
			AssertOrgTaxConfigEquals(configBrn1_ObB, false, false, false, "Org-B has old org tax config deactived", lastEditUser: AnotherUserCode);
			AssertOrgTaxConfigEquals(configBrn1_ObB_NonCurrentCompany, true, false, false, "Org-B non current company data is not affected");
		}

		#endregion

		void RunScript(Guid templatePK)
		{
			using (var cmd = Db.Connection.Command("UpdateTaxConfigurationsForOrgnization"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@AccOrgTaxConfigurationTemplatePK", SqlDbType.UniqueIdentifier, templatePK);
				cmd.AddParameter("@OperationStaff", SqlDbType.VarChar, 3, GlbStaff.CurrentUser.GS_Code.ToString());
				cmd.ExecuteNonQuery();
			}
		}

		void AssertOrgTaxConfigEquals(AccOrgTaxConfiguration expected, bool isActive, bool isThresholdUsed, bool isRecoverTax, string assertMsg = null, string lastEditUser = null, string createUser = null)
		{
			var connector = assertMsg == null ? string.Empty : ", and ";
			CombineAssertions($"{assertMsg}{connector}value should match expected ones",
			() =>
			{
				AssertEquals("Threshold Used", isThresholdUsed, expected.OTC_IsThresholdUsed);
				AssertEquals("Active", isActive, expected.OTC_IsActive);
				AssertEquals("Recover Tax", isRecoverTax, expected.OTC_RecoverTax);
				AssertEquals("Edit user", lastEditUser ?? DefaultUserCode, expected.OTC_SystemLastEditUser);
				AssertEquals("Create user", createUser ?? DefaultUserCode, expected.OTC_SystemCreateUser);
			});
		}

		void PrepareTestData(bool isReceivable)
		{
			var ledger = isReceivable ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
			DefaultUserCode = GlbStaff.CurrentUser.GS_Code;

			companyDataA = BizOTestObjectCreator.CreateOrgHeader("OrgA", true, true, true, true, true, true).CompanyData;
			var orgB = BizOTestObjectCreator.CreateOrgHeader("OrgB", true, true, true, true, true, true);
			companyDataB = orgB.CompanyData;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), BizOTestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				companyDataB_NonCurrentCompany = orgB.CompanyData;
			}
			Factory.Save();

			var taxSystem1 = AccountingTestObjectCreator.CreateTaxSystem("TS1");
			var taxSystem2 = AccountingTestObjectCreator.CreateTaxSystem("TS2");
			var taxAuthority1 = AccountingTestObjectCreator.CreateTaxAuthority("TA1");
			var taxAuthority2 = AccountingTestObjectCreator.CreateTaxAuthority("TA2");

			taxConfigBrn1 = AccountingTestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority1, taxSystem1, ledger, true, "TestCodeA1", "TestDescA1");
			taxConfigCom1 = AccountingTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority1, taxSystem1, ledger, true, "TestCodeA2", "TestDescA2");
			taxConfigBrn2 = AccountingTestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority2, taxSystem2, ledger, true, "TestCodeB1", "TestDescB1");
			AccountingTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority2, taxSystem2, ledger, true, "TestCodeB2", "TestDescB2");
			Factory.Save();

			template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			template.OCT_IsReceivable = isReceivable;
			Factory.Save();

			if (isReceivable)
			{
				companyDataA.OB_OCT_ARTaxTemplate = template.PK;
				companyDataB.OB_OCT_ARTaxTemplate = template.PK;
				companyDataB_NonCurrentCompany.OB_OCT_ARTaxTemplate = template.PK;
			}
			else
			{
				companyDataA.OB_OCT_APTaxTemplate = template.PK;
				companyDataB.OB_OCT_APTaxTemplate = template.PK;
				companyDataB_NonCurrentCompany.OB_OCT_APTaxTemplate = template.PK;
			}
			Factory.Save();
		}

		AccountingTestObjectCreator AccountingTestObjectCreator => accountingTestObjectCreator ?? (accountingTestObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator accountingTestObjectCreator;

		TestObjectCreator BizOTestObjectCreator => bizOTestObjectCreator ?? (bizOTestObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator bizOTestObjectCreator;

		OrgCompanyData companyDataA, companyDataB, companyDataB_NonCurrentCompany;
		AccTaxConfiguration taxConfigBrn1, taxConfigCom1, taxConfigBrn2;
		AccOrgTaxConfigurationTemplate template;

		string DefaultUserCode;
		const string AnotherUserCode = "TST";
	}
}

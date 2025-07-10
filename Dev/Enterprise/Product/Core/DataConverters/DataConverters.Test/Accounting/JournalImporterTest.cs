using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataConverters.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DataConverters.Testing.Accounting
{
	[TestedType(typeof(JournalImporter))]
	sealed internal class JournalImporterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAddJournalLine()
		{
			var jImporter = new JournalImporter(LedgerTypes.AccountsReceivable, ZDateTime.Now, (Guid)AccountingConfigurationRegistry.Instance.ARJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			var journal = jImporter.AddJournalLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", OrgDebtor.OH_Code));
			AssertEquals("Should be 100.11", 100.11M, journal.AH_InvoiceAmount);
		}

		[TestDate(2003, 06, 05)]
		public void TestValidateIsValid()
		{
			AccountingPeriodTestHelper.SetupPeriods();
			var jImporter = new JournalImporter(LedgerTypes.AccountsReceivable, ZDateTime.Now, (Guid)AccountingConfigurationRegistry.Instance.ARJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			jImporter.AddJournalLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", "Invalid oh_code"));
			AssertEquals("Should be false", false, jImporter.IsValid);

			jImporter.AddJournalLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", OrgDebtor.OH_Code));
			AssertEquals("Should be true", true, jImporter.IsValid);
		}

		[TestDate(2003, 06, 05)]
		public void TestValidateErrors()
		{
			AccountingPeriodTestHelper.SetupPeriods();
			var jImporter = new JournalImporter(LedgerTypes.AccountsReceivable, ZDateTime.Now, (Guid)AccountingConfigurationRegistry.Instance.ARJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			jImporter.AddJournalLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", "Invalid oh_code"));
			AssertEquals("Should be true", true, jImporter.Errors.Length > 0);

			jImporter.AddJournalLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", OrgDebtor.OH_Code));
			AssertEquals("Should be false", false, jImporter.Errors.Length > 0);
		}

		public void TestOrgError()
		{
			var jImporter = new JournalImporter(LedgerTypes.AccountsReceivable, ZDateTime.Now, (Guid)AccountingConfigurationRegistry.Instance.ARJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			jImporter.AddJournalLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", "Invalid oh_code"));
			AssertEquals(Res.GetString("a4c572e8-44e1-4705-a29a-e6ef86bd14dd", "Organization code is invalid: ({0}).", "Invalid oh_code") + " ", jImporter.Errors);
		}

		public void TestOrgErrorForValidCode()
		{
			var jImporter = new JournalImporter(LedgerTypes.AccountsPayable, ZDateTime.Now, (Guid)AccountingConfigurationRegistry.Instance.ARJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			jImporter.AddJournalLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", "LegacyCode"));
			AssertContains("[Accounts Payable Journal] Account: The Organization must have an Organization Type of Payables selected.", jImporter.Errors);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new JournalImporter(LedgerTypes.AccountsReceivable, ZDateTime.Now, (Guid)AccountingConfigurationRegistry.Instance.ARJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		OrgHeader OrgDebtor;

		AccountingPeriodTestHelper AccountingPeriodTestHelper
		{
			get	{ return accountingPeriodTestHelper ?? (accountingPeriodTestHelper = new AccountingPeriodTestHelper()); }
		}
		AccountingPeriodTestHelper accountingPeriodTestHelper;

		protected override void SetUp()
		{
			base.SetUp();

			OrgDebtor = Factory.NewWithValidTestData<OrgHeader>();
			OrgDebtor.OH_Code = "TestDebtor";
			OrgDebtor.OH_IsDebtor = true;

			var organisationOrgLegacyCode = OrgDebtor.CustomsCodes.AddNew();
			organisationOrgLegacyCode.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
			organisationOrgLegacyCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			organisationOrgLegacyCode.OK_CustomsRegNo = "LegacyCode";

			Factory.Save();
		}
	}
}

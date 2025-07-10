using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.China.Testing
{
	[TestedType(typeof(ChinaGLJournalTypesProvider))]
	public class ChinaGLJournalTypesProviderTest : TestCaseWithFactory
	{
		public void TestGetGLJournalTypes()
		{
			TestObjectCreator.SetTemporaryControlAccounts();
			AssertArrayEqualsByElements("Transaction type list for China", new string[] { "GJL", "NJL" }, GLJournalTypesProvider.GetGLJournalTypes().GetAllCodes());

			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting
			{
				AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN,
				NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL
			};
			var testCreator = new TestObjectCreator(Factory);
			testCreator.CreateTestPeriodsForEntireYear(DateTime.Now.Year);
			using (AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, journalEntriesNumberCustomisationSetting))
			using (AccountingConfigurationRegistry.Instance.EnableAJLandRJL.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertArrayEqualsByElements("Transaction type list", new string[] { "AJL", "RJL", "GJL", "NJL" }, GLJournalTypesProvider.GetGLJournalTypes().GetAllCodes());
			}
		}

		protected override void SetUp()
		{
			GLJournalTypesProvider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.China) as IInstanceProvider<IGLJournalTypesProvider>).Get();

			AssertNotNull(GLJournalTypesProvider);
		}

		IGLJournalTypesProvider GLJournalTypesProvider;
	}
}

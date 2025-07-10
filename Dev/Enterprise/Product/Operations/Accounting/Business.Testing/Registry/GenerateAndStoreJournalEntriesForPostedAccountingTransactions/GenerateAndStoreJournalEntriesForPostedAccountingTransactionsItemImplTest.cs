using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Billing.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using static Enterprise.Accounting.Registry.Business.AccountingConfigurationRegistry;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(GenerateAndStoreJournalEntriesForPostedAccountingTransactionsItemImpl))]
	class GenerateAndStoreJournalEntriesForPostedAccountingTransactionsItemImplTest : BooleanRegistryItemTest
	{
		[TestDate(2023, 3, 28, 0, 0, 0)]
		public void TestSetValueCore()
		{
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var today = ZDateTime.Today.ToDateTime();
			var year = today.Year;
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(year, companyPK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);
			testObjectCreator.SetControlAccountsForGenerateJournalEntriesStartDate();
			var item = GetNewRegistryItem();
			item.SetValue(companyPK, Guid.Empty, Guid.Empty, true);
			AssertEquals(new DateTime(2023, 1, 1), AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty));

			var ediMessages = factory.Load<IUsageEDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
			AssertEquals("Wrong number of rows in EDIMessage table.", 1, ediMessages.Length);
			var jObject = ediMessages.Select(msg => JObject.Parse(msg.EM_MessageTextDetail)).First();
			AssertEquals(UsageFeatures.Codes.AccGeneralLedgerData, jObject.Properties().FirstOrDefault(kp => kp.Name.Equals(UsageProperties.FeatureCode, StringComparison.InvariantCulture))?.Value.ToString());
			AssertEquals(UsageFeatures.Modules.Accounting, jObject.Properties().FirstOrDefault(kp => kp.Name.Equals(UsageProperties.Module, StringComparison.InvariantCulture))?.Value.ToString());
			AssertEquals("Generate and Store Journal Entries for Posted Accounting Transactions", jObject.Properties().FirstOrDefault(kp => kp.Name.Equals(UsageProperties.FeatureDescription, StringComparison.InvariantCulture))?.Value.ToString());
		}

		protected override StronglyTypedRegistryItem<bool, bool> GetNewRegistryItem()
		{
			return new GenerateAndStoreJournalEntriesForPostedAccountingTransactionsItemImpl(
						"GenerateAndStoreJournalEntriesForPostedAccountingTransactions",
						Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries,
						(NoResString)"Category",
						(NoResString)"Caption",
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false,
						new BooleanRegistryDataType());
		}
	}
}

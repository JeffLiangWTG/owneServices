using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Accounting.ElectronicMessaging.Registry.AccountingElectronicMessagingRegistry;
using static Enterprise.Core.Constants;
using static Enterprise.ZArchitecture.Environment.RegistryItemSet;

namespace Enterprise.Accounting.ElectronicMessaging.Registry.Testing
{
	[TestedType(typeof(AccountingElectronicMessagingRegistry))]
	class AccountingElectronicMessagingRegistryTest : RegistryItemSetTestCaseWithFactory<AccountingElectronicMessagingRegistry>
	{
		protected override IEnumerable<string> ConditionallyVisibleRegistryItems => new string[]
		{
			nameof(ItemSet.EReportingAutomaticRetryLimit),
			nameof(ItemSet.RomaniaEReportingDelayTimeForQueryInvoiceRequest),
			nameof(ItemSet.QueueOldTransactionsFromDateReceivables)
		};

		#region Reflection Based Tests

		public void TestCountrySpecificDefaultValueRegistryItemImpl_RegistriesMustAlsoSetCacheExpensiveDefaultValueOption()
			=> AccountingMasterFilesRegistryTest.CountrySpecificDefaultValueRegistryItemImpl_RegistriesMustAlsoSetCacheExpensiveDefaultValueOption(AllItems);

		#endregion

		public void TestRomaniaEReportingDelayTimeForQueryInvoiceRequest()
		{
			var registryItem = ItemSet.RomaniaEReportingDelayTimeForQueryInvoiceRequest;

			AssertEquals("Name", "RomaniaEReportingDelayTimeForQueryInvoiceRequest", registryItem.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Romania, registryItem.Category);
			AssertEquals("Caption", "E-Reporting Delay Time For Query Invoice Request (CargoWiseOne Support Only)", registryItem.Caption);
			AssertEquals("Hint", @"By default, the delay time is set to 15 minutes.
However, should there be a need to get an approval status quicker, we can adjust the delay time so that a query can be submitted to obtain the status earlier.", registryItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, registryItem.Storage);
			AssertEquals("Options", AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden, registryItem.Options);
			AssertEquals("Default Value", 15, registryItem.DefaultValue);

			AssertEquals("Min Value", (double)0, ((IntRegistryDataType)registryItem.DataType).LowerBound);
			AssertEquals("Max Value", (double)1440, ((IntRegistryDataType)registryItem.DataType).UpperBound);

			AssertEquals("CountryFilterPK", CountryFilterPKs.Romania, registryItem.CountryFilterPKs);

			var oldValue = 30;
			var newValue = 31;
			var expectLogMessage = $"The value changed from [{oldValue}] to [{newValue}]";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registryItem, oldValue, newValue);
			var logReference = registryItem.OnBuildLogReference(args);
			AssertEquals(expectLogMessage, logReference);
		}

		public void TestRomaniaEReportingDelayTimeForQueryInvoiceRequestVisible()
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.RomaniaEReportingDelayTimeForQueryInvoiceRequest.Options);
		}

		public void TestRomaniaEReportingDelayTimeForQueryInvoiceRequestIsNotVisible()
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.RomaniaEReportingDelayTimeForQueryInvoiceRequest.Options);
		}

		public void TestRomaniaEReportingDelayTimeForQueryInvoiceRequestSwitchFromNotVisibleToVisible()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(RegistryOptions.IsHidden, ItemSet.RomaniaEReportingDelayTimeForQueryInvoiceRequest.Options);
			}
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.RomaniaEReportingDelayTimeForQueryInvoiceRequest.Options);
			}
		}

		public void TestRomaniaEReportingDelayTimeForQueryInvoiceRequestSwitchFromVisibleToNotVisible()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.RomaniaEReportingDelayTimeForQueryInvoiceRequest.Options);
			}
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(RegistryOptions.IsHidden, ItemSet.RomaniaEReportingDelayTimeForQueryInvoiceRequest.Options);
			}
		}

		public void TestQueueOldTransactionsFromDateReceivables()
		{
			var registryItem = ItemSet.QueueOldTransactionsFromDateReceivables;

			AssertEquals("Name", "QueueOldTransactionsFromDateReceivables", registryItem.Name);
			AssertEquals("Category", AccountingMasterFilesRegistry.Categories.Accounting_EReportingAndEInvoicingConfigurations, registryItem.Category);
			AssertEquals("Caption", "Queue Old Transactions From Date - Receivables", registryItem.Caption);
			AssertEquals("Hint", @"Once a new date is entered, the Batch Queue Invoice For e-Invoicing Service Task (BQI) will queue 500 transactions, which were posted from the entered date, at every run.
When all those transactions were posted, the entered date of this registry item will be reset to empty, and the processing of old transactions will be stopped.", registryItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, registryItem.Storage);
		}

		public void TestQueueOldTransactionsFromDateReceivablesVisibleCompanies()
		{
			var romaniaCompany = Factory.New<GlbCompany>();
			romaniaCompany.GC_RN_NKCountryCode = CountryCodes.Romania;
			romaniaCompany.GC_Code = "ABC";
			romaniaCompany.GC_Name = "TestRoCompany";

			var romaniaCompany1 = Factory.New<GlbCompany>();
			romaniaCompany1.GC_RN_NKCountryCode = CountryCodes.Romania;
			romaniaCompany1.GC_Code = "DEF";
			romaniaCompany1.GC_Name = "TestRoCompany1";
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(romaniaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(romaniaCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var registryItem = ItemSet.QueueOldTransactionsFromDateReceivables;
				var companies = new GlbCompanyCollection(Factory);

				var visibleCountries = QueueOldTransactionsForEInvoicingCountryHelper.GetQueueOldTransactionsCountryPKs();
				foreach (var company in companies)
				{
					if (visibleCountries.Any(x => x == company.Country.PK) && company.PK == romaniaCompany.PK)
					{
						AssertEquals(true, registryItem.IsVisible(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
					}
					else
					{
						AssertEquals(false, registryItem.IsVisible(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
					}
				}
			}
		}

		public void TestQueueOldTransactionsFromDateReceivablesVisible()
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(RegistryOptions.Default, ItemSet.QueueOldTransactionsFromDateReceivables.Options);
		}

		public void TestQueueOldTransactionsFromDateReceivablesIsNotVisible()
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.QueueOldTransactionsFromDateReceivables.Options);
		}

		public void TestQueueOldTransactionsFromDateReceivablesSwitchFromNotVisibleToVisible()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(RegistryOptions.IsHidden, ItemSet.QueueOldTransactionsFromDateReceivables.Options);
			}
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(RegistryOptions.Default, ItemSet.QueueOldTransactionsFromDateReceivables.Options);
			}
		}

		public void TestQueueOldTransactionsFromDateReceivablesSwitchFromVisibleToNotVisible()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(RegistryOptions.Default, ItemSet.QueueOldTransactionsFromDateReceivables.Options);
			}
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(RegistryOptions.IsHidden, ItemSet.QueueOldTransactionsFromDateReceivables.Options);
			}
		}

		public void TestMexicoTaxRegimeID()
		{
			AssertEquals("Name", "MexicoTaxRegimeID", ItemSet.MexicoTaxRegimeID.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Mexico, ItemSet.MexicoTaxRegimeID.Category);
			AssertEquals("Caption", "Mexico Tax Regime ID (CargoWiseOne Support Only)", ItemSet.MexicoTaxRegimeID.Caption);
			AssertEquals("Hint", "This registry provides Tax Regime ID for <RegimenFiscal> element in Mexico e-Invoicing XML mapping.", ItemSet.MexicoTaxRegimeID.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.MexicoTaxRegimeID.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.MexicoTaxRegimeID.Options);
			AssertEquals("Default Value", "601", ItemSet.MexicoTaxRegimeID.DefaultValue);

			var mexicoTaxRegimeIdTypesList = ObjectFactory.Get<IAccountingCountryComplianceGlobalFactory>().GetFeatureInterface<IDebtorTaxRegime>(CountryCodes.Mexico).GetTaxRegimeIdTypes();
			foreach (CodeDescriptionPair taxRegimeId in mexicoTaxRegimeIdTypesList)
			{
				ItemSet.MexicoTaxRegimeID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxRegimeId.Code);
				AssertEquals("Value", taxRegimeId.Code, ItemSet.MexicoTaxRegimeID.Value);
			}
		}

		public void TestEInvoicingServicePointSuffix()
		{
			AssertEquals("Name", "eInvoicingServicePointSuffix", ItemSet.eInvoicingServicePointSuffix.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations, ItemSet.eInvoicingServicePointSuffix.Category);
			AssertEquals("Caption", "E-Invoicing Testing Suffix for EInvoicingServicePoint (CargoWise Support Only)", ItemSet.eInvoicingServicePointSuffix.Caption);
			AssertEquals("Hint", @"The Suffix set in this registry is used in xT routing tables for testing purposes and on testing environments. Do not use this registry on production environments.

This Suffix is concatenated to the EInvoicingServicePoint being placed at the end of this DestinationParty.
E.g. for an EInvoicingServicePoint 'XHUB_AU_EINVOICING', setting this registry to ""ABC"" will change the EInvoicingServicePoint to 'XHUB_AU_EINVOICING_ABC'.", ItemSet.eInvoicingServicePointSuffix.Hint);
			AssertEquals("Min lenght", 0, ((StringRegistryDataType)ItemSet.eInvoicingServicePointSuffix.DataType).MinLength);
			AssertEquals("Max lenght", 6, ((StringRegistryDataType)ItemSet.eInvoicingServicePointSuffix.DataType).MaxLength);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.eInvoicingServicePointSuffix.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.eInvoicingServicePointSuffix.Options);
			AssertEquals("Default Value", ZString.Empty, ItemSet.eInvoicingServicePointSuffix.DefaultValue);
		}

		public void TestEgyptEInvoicingCredentials()
		{
			AssertEquals("Name", nameof(ItemSet.EgyptEInvoicingCredentials), ItemSet.EgyptEInvoicingCredentials.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Egypt, ItemSet.EgyptEInvoicingCredentials.Category);
			AssertEquals("Caption", "E-Invoicing Credentials", ItemSet.EgyptEInvoicingCredentials.Caption);
			AssertEquals("Hint", "This registry defines the Client ID, Client Secret and API Key used to process Egypt E-Invoices.\r\n\r\nAPI Key must be entered in the Egypt Invoicing Portal when registering or editing the CargoWise to receive notifications for the final invoice status. If you do not generate the API Key and enter it into your Egypt Invoicing Portal for CargoWise, the e-reporting status of the transactions can not be updated to Successful (SUC) or Failed (FAL), and will remain in Delivered (DLV) status.\r\n\r\nImportant: Whenever you generate a new API Key, the existing API Key must be changed in the Egypt Invoicing Portal for the CargoWise.", ItemSet.EgyptEInvoicingCredentials.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.EgyptEInvoicingCredentials.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.EgyptEInvoicingCredentials.Options);
			AssertNotNull("DefaultValue", ItemSet.EgyptEInvoicingCredentials.DefaultValue);
			AssertEquals("DefaultValue.ClientId", "", ItemSet.EgyptEInvoicingCredentials.DefaultValue.ClientId);
			AssertEquals("DefaultValue.ClientSecret", "", ItemSet.EgyptEInvoicingCredentials.DefaultValue.ClientSecret);
			Assert((ItemSet.EgyptEInvoicingCredentials.DataType as EInvoicingCredentialsRegistryDataType).Behavior.HasAPIKeyFlag());
			Assert((ItemSet.EgyptEInvoicingCredentials.DataType as EInvoicingCredentialsRegistryDataType).Behavior.SetPasswordCharFlag());
		}

		public void TestEnableReceivingEInvoiceStatusNotification()
		{
			AssertEquals("Name", "EnableReceivingEInvoiceStatusNotification", ItemSet.EnableReceivingEInvoiceStatusNotification.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Egypt, ItemSet.EnableReceivingEInvoiceStatusNotification.Category);
			AssertEquals("Caption", "Enable Receiving e-Invoice Status Notification", ItemSet.EnableReceivingEInvoiceStatusNotification.Caption);
			AssertEquals("Hint", @"This registry enables e-invoice notifications for the status of eligible AR transactions, indicating whether they are valid or invalid according to the Egypt Tax Authority (ETA).

By default, the registry is set to 'No', when the notification configuration is completed in the ETA Invoicing Portal, it should be overridden and set to 'Yes'.

After completing the notification configuration and enabling this registry, the e-reporting status of eligible AR transactions will be set to 'Delivered - DLV.' It will then be updated to its final status based on the notification received from the ETA.

* The registry will be temporary and will be removed once the transition is complete.", ItemSet.EnableReceivingEInvoiceStatusNotification.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.EnableReceivingEInvoiceStatusNotification.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.EnableReceivingEInvoiceStatusNotification.Options);
			AssertEquals("DefaultValue", false, ItemSet.EnableReceivingEInvoiceStatusNotification.DefaultValue);
			AssertContainsExactElementsInAnyOrder("CountryFilterPKs", new[] { CountryGuids.Egypt }, ItemSet.EnableReceivingEInvoiceStatusNotification.CountryFilterPKs);
		}

		public void TestPolandEInvoicingCredentials()
		{
			AssertEquals("Name", "PolandEInvoicingCredentials", ItemSet.PolandEInvoicingCredentials.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Poland, ItemSet.PolandEInvoicingCredentials.Category);
			AssertEquals("Caption", "E-Invoicing Credentials", ItemSet.PolandEInvoicingCredentials.Caption);
			AssertEquals("Hint", "This registry defines the KSeF Token Name and KSeF Token used to process Poland E-Invoices.", ItemSet.PolandEInvoicingCredentials.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.PolandEInvoicingCredentials.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.PolandEInvoicingCredentials.Options);
			AssertNotNull("DefaultValue", ItemSet.PolandEInvoicingCredentials.DefaultValue);
			AssertEquals("DefaultValue.ClientId", "", ItemSet.PolandEInvoicingCredentials.DefaultValue.ClientId);
			AssertEquals("DefaultValue.ClientSecret", "", ItemSet.PolandEInvoicingCredentials.DefaultValue.ClientSecret);
			AssertContainsExactElementsInAnyOrder("CountryFilterPKs", new[] { Core.Constants.CountryGuids.Poland }, ItemSet.PolandEInvoicingCredentials.CountryFilterPKs);
		}

		public void TestMalaysiaEInvoicingCredentials()
		{
			AssertEquals("MalaysiaEInvoicingCredentials", ItemSet.MalaysiaEInvoicingCredentials.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_Malaysia, ItemSet.MalaysiaEInvoicingCredentials.Category);
			AssertEquals("E-Invoicing Credentials", ItemSet.MalaysiaEInvoicingCredentials.Caption);
			AssertEquals("This registry defines the Client ID and Client Secret used to process Malaysia E-Invoices.", ItemSet.MalaysiaEInvoicingCredentials.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.MalaysiaEInvoicingCredentials.Storage);
			AssertEquals(RegistryOptions.Default, ItemSet.MalaysiaEInvoicingCredentials.Options);
			AssertEquals(string.Empty, ItemSet.MalaysiaEInvoicingCredentials.DefaultValue.ClientId);
			AssertEquals(string.Empty, ItemSet.MalaysiaEInvoicingCredentials.DefaultValue.ClientSecret);
			AssertContainsExactElementsInAnyOrder("CountryFilterPKs", new[] { Core.Constants.CountryGuids.Malaysia }, ItemSet.MalaysiaEInvoicingCredentials.CountryFilterPKs);
			Assert((ItemSet.MalaysiaEInvoicingCredentials.DataType as EInvoicingCredentialsRegistryDataType).Behavior.SetPasswordCharFlag());

			EInvoicingCredentials eInvoicingCredentials = new EInvoicingCredentials();
			eInvoicingCredentials.ClientId = "testClientId";
			eInvoicingCredentials.ClientSecret = "testClientSecret";
			ItemSet.MalaysiaEInvoicingCredentials.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, eInvoicingCredentials);

			AssertEquals("testClientId", ItemSet.MalaysiaEInvoicingCredentials.Value.ClientId);
			AssertEquals("testClientSecret", ItemSet.MalaysiaEInvoicingCredentials.Value.ClientSecret);	
		}

		public void TestJOEInvoicingCredentials()
		{
			CombineAssertions("Test Jordan EInvoicing Credentials Property For", () =>
			{
				AssertEquals("Name", "JordanEInvoicingCredentials", ItemSet.JordanEInvoicingCredentials.Name);
				AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Jordan, ItemSet.JordanEInvoicingCredentials.Category);
				AssertEquals("Caption", "E-Invoicing Credentials", ItemSet.JordanEInvoicingCredentials.Caption);
				AssertEquals("Hint", "This registry defines the Client ID and Client Secret used to process Jordan E-Invoices.", ItemSet.JordanEInvoicingCredentials.Hint);
				AssertEquals("Registry Storage Flags", RegistryStorageFlags.Company, ItemSet.JordanEInvoicingCredentials.Storage);
				AssertEquals("Registry Options", RegistryOptions.Default, ItemSet.JordanEInvoicingCredentials.Options);
				AssertEquals("Client Id", string.Empty, ItemSet.JordanEInvoicingCredentials.DefaultValue.ClientId);
				AssertEquals("Client Secret", string.Empty, ItemSet.JordanEInvoicingCredentials.DefaultValue.ClientSecret);
				AssertContainsExactElementsInAnyOrder("Country Filter", new[] { Core.Constants.CountryGuids.Jordan }, ItemSet.JordanEInvoicingCredentials.CountryFilterPKs);
				Assert("Behavior", (ItemSet.JordanEInvoicingCredentials.DataType as EInvoicingCredentialsRegistryDataType).Behavior.SetPasswordCharFlag());
			});

			var eInvoicingCredentials = new EInvoicingCredentials();
			eInvoicingCredentials.ClientId = "happyClientId";
			eInvoicingCredentials.ClientSecret = "happyClientSecret";
			ItemSet.JordanEInvoicingCredentials.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, eInvoicingCredentials);

			CombineAssertions("Test Jordan EInvoicing Credentials Property Change For", () =>
			{
				AssertEquals("Client Id", "happyClientId", ItemSet.JordanEInvoicingCredentials.Value.ClientId);
				AssertEquals("Client Secret", "happyClientSecret", ItemSet.JordanEInvoicingCredentials.Value.ClientSecret);
			});
		}

		public void TestMauritiusEInvoicingCredentials()
		{
			CombineAssertions(() =>
			{
				var item = ItemSet.MauritiusEInvoicingUsername;
				AssertEquals("MauritiusEInvoicingUsername", item.Name);
				AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_Mauritius, item.Category);
				AssertEquals("E-Invoicing Username", item.Caption);
				AssertEquals("This registry defines the User ID used to process Mauritius E-Invoices.", item.Hint);
				AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.Branch, item.Storage);
				AssertEquals(RegistryOptions.Default, item.Options);
				AssertEquals(string.Empty, item.DefaultValue);
				AssertContainsExactElementsInAnyOrder("CountryFilterPKs", new[] { CountryGuids.Mauritius }, item.CountryFilterPKs);
			});
		}

		public void TestMauritiusEInvoicingPassword()
		{
			CombineAssertions(() =>
			{
				var item = ItemSet.MauritiusEInvoicingPassword;
				AssertEquals("MauritiusEInvoicingPassword", item.Name);
				AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_Mauritius, item.Category);
				AssertEquals("E-Invoicing Password", item.Caption);
				AssertEquals("This registry defines the Password used to process Mauritius E-Invoices.", item.Hint);
				AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.Branch, item.Storage);
				AssertEquals(RegistryOptions.Default, item.Options);
				AssertEquals(string.Empty, item.DefaultValue);
				var editorInfo = (TextRegistryEditorInfo)item.EditorInfo;
				AssertEquals(TextEditorType.Password, editorInfo.EditorType);
				AssertContainsExactElementsInAnyOrder("CountryFilterPKs", new[] { CountryGuids.Mauritius }, item.CountryFilterPKs);
			});
		}

		public void TestMauritiusEInvoicingEbsMraID()
		{
			CombineAssertions(() =>
			{
				var item = ItemSet.MauritiusEInvoicingEbsMraID;
				AssertEquals("MauritiusEInvoicingEbsMraID", item.Name);
				AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_Mauritius, item.Category);
				AssertEquals("E-Invoicing EBS MRA ID", item.Caption);
				AssertEquals("This registry defines the EBS MRA ID used to process Mauritius E-Invoices.", item.Hint);
				AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.Branch, item.Storage);
				AssertEquals(RegistryOptions.Default, item.Options);
				AssertEquals(string.Empty, item.DefaultValue);
				AssertContainsExactElementsInAnyOrder("CountryFilterPKs", new[] { CountryGuids.Mauritius }, item.CountryFilterPKs);
			});
		}

		public void TestEInvoicingBatchSize()
		{
			AssertEquals("Name", nameof(ItemSet.EInvoicingBatchSize), ItemSet.EInvoicingBatchSize.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations, ItemSet.EInvoicingBatchSize.Category);
			AssertEquals("Caption", "E-Reporting Batch Size (CargoWiseOne Support Only)", ItemSet.EInvoicingBatchSize.Caption);
			AssertEquals("Hint", @"This registry defines the batch size of E-Invoicing GEI Messages for countries that support batching.

The following countries support batches and against each is the default system defined batch size:
 - Egypt: 98
 - Korea South: 98
 - Poland: 3
 - Spain: 98
 
Set this registry to a number greater than zero to override the default batch number.
Note that architectural limitations prevent batches greater than 98.", ItemSet.EInvoicingBatchSize.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.EInvoicingBatchSize.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.EInvoicingBatchSize.Options);
			AssertEquals("Default Value", 0, ItemSet.EInvoicingBatchSize.DefaultValue);
			AssertEquals("Min Value", 0.0, ((IntRegistryDataType)ItemSet.EInvoicingBatchSize.DataType).LowerBound);
			AssertEquals("Max Value should be 98 due to a limitation when importing Universal Events", 98.0, ((IntRegistryDataType)ItemSet.EInvoicingBatchSize.DataType).UpperBound);
		}

		public void TestEReportingAutomaticRetryLimit()
		{
			AssertEquals("Name", nameof(ItemSet.EReportingAutomaticRetryLimit), ItemSet.EReportingAutomaticRetryLimit.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations, ItemSet.EReportingAutomaticRetryLimit.Category);
			AssertEquals("Caption", "E-Reporting Automatic Retry Limit", ItemSet.EReportingAutomaticRetryLimit.Caption);
			AssertEquals("Hint", @"This registry item is referenced by Turkey login companies only.

When electronic invoicing is enabled for your Login company, CargoWise sends messages to the external provider. Occasionally there may be an interruption to receiving the expected response.
By default, CargoWise will automatically retry to communicate to receive the response message.  This registry item sets the number of times CargoWise should retry. If CargoWise is still detecting an issue after the nominated number of retries, an error will be shown against the transaction in the e-Reporting Message field. 

The default value is 5 retry attempts. The maximum number of retries allowed is 10 retries. 
If the number of retries is set to 0, the failure will be reported immediately on the transaction without any attempt to automatically retry to resolve the issue.", ItemSet.EReportingAutomaticRetryLimit.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.EReportingAutomaticRetryLimit.Storage);
			AssertEquals("DefaultValue", 5, ItemSet.EReportingAutomaticRetryLimit.DefaultValue);

			AssertExceptionThrown("Min", typeof(RegistryValidationException), "Value must be greater than or equal to the minimum (0)", () => ItemSet.EReportingAutomaticRetryLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, -9));
			AssertExceptionThrown("Max", typeof(RegistryValidationException), "Value must be less than or equal to the maximum (10)", () => ItemSet.EReportingAutomaticRetryLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 11));
			AssertExceptionThrown("Type", typeof(ArgumentException), "Input value data type is incorrect", () => ItemSet.EReportingAutomaticRetryLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Hello"), true);

			ItemSet.EReportingAutomaticRetryLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ItemSet.EReportingAutomaticRetryLimit.DefaultValue);
			AssertEquals("Default value, not overridden", 5, ItemSet.EReportingAutomaticRetryLimit.Value);

			ItemSet.EReportingAutomaticRetryLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 9);
			AssertEquals("Overridden with different value", 9, ItemSet.EReportingAutomaticRetryLimit.Value);
		}

		public void TestEReportingAutomaticRetryLimit_HiddenWithoutTurkeyCompliance() =>
			AssertEReportingAutomaticRetryLimitVisibility(RegistryOptions.IsHidden, false, false);

		public void TestEReportingAutomaticRetryLimit_VisibleWithFullTurkeyCompliance() =>
			AssertEReportingAutomaticRetryLimitVisibility(RegistryOptions.Default, true, true);

		public void TestEReportingAutomaticRetryLimit_VisibleWithTurkeyAPCompliance() =>
			AssertEReportingAutomaticRetryLimitVisibility(RegistryOptions.Default, false, true);

		public void TestEReportingAutomaticRetryLimit_VisibleWithTurkeyARCompliance() =>
			AssertEReportingAutomaticRetryLimitVisibility(RegistryOptions.Default, true, false);

		public void TestEReportingAutomaticRetryLimit_SwitchFromNotVisibleToVisible()
		{
			AssertEReportingAutomaticRetryLimitVisibility(RegistryOptions.IsHidden, false, false);
			AssertEReportingAutomaticRetryLimitVisibility(RegistryOptions.Default, true, true);
		}

		public void TestEReportingAutomaticRetryLimit_SwitchFromVisibleToNotVisible()
		{
			AssertEReportingAutomaticRetryLimitVisibility(RegistryOptions.Default, true, true);
			AssertEReportingAutomaticRetryLimitVisibility(RegistryOptions.IsHidden, false, false);
		}

		void AssertEReportingAutomaticRetryLimitVisibility(RegistryOptions option, bool enableAR, bool enableAP)
		{
			var apStartDate = enableAP ? ZDateTime.Now.AddDays(-2) : ZDateTime.Now.AddDays(2);
			var arStartDate = enableAR ? ZDateTime.Now.AddDays(-2) : ZDateTime.Now.AddDays(2);

			var amfRegistry = AccountingMasterFilesRegistry.Instance;

			var apFeature = nameof(amfRegistry.EnableNewTurkeyAPComplianceFeatures);
			var arFeature = nameof(amfRegistry.EnableNewTurkeyARComplianceFeatures);

			AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, apStartDate.ToDateTime());
			AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, arStartDate.ToDateTime());

			AssertEquals($"Test must be able to enable/disable {apFeature}", amfRegistry.EnableNewTurkeyAPComplianceFeatures, enableAP);
			AssertEquals($"Test must be able to enable/disable {arFeature}", amfRegistry.EnableNewTurkeyARComplianceFeatures, enableAR);

			AssertEquals($"Option must be '{option}' when '{apFeature}' is {enableAP} and '{arFeature}' is {enableAR}.", option, ItemSet.EReportingAutomaticRetryLimit.Options);

			Guid countryGuid = Guid.Empty;
			AssertNoExceptionThrown("CountryFilterPKs must only contain Turkey", () => countryGuid = ItemSet.EReportingAutomaticRetryLimit.CountryFilterPKs.Single());
			AssertEquals("CountryFilterPKs must only contain Turkey", Core.Constants.CountryGuids.Turkey, countryGuid);
		}

		public void TestReportDomesticTransactionsWithWSMTXCAWebService()
		{
			AssertEquals("Name", nameof(ItemSet.ReportDomesticTransactionsWithWSMTXCAWebService), ItemSet.ReportDomesticTransactionsWithWSMTXCAWebService.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Argentina, ItemSet.ReportDomesticTransactionsWithWSMTXCAWebService.Category);
			AssertEquals("Caption", "Report Domestic Transactions With WSMTXCA Web Service", ItemSet.ReportDomesticTransactionsWithWSMTXCAWebService.Caption);
			AssertEquals("Hint", @"This registry is used to control which Web Service will be used to report domestic AR transactions to AFIP.

By default, domestic AR transactions are reported with the WSFEV Web Service.

If this registry is set to 'Yes', domestic AR transactions will be reported to AFIP using the WSMTXCA Web Service(with item detail), instead of using the WSFEV Web Service", ItemSet.ReportDomesticTransactionsWithWSMTXCAWebService.Hint);

			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.EgyptEInvoicingCredentials.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.EgyptEInvoicingCredentials.Options);
			AssertEquals("DefaultValue", false, ItemSet.ReportDomesticTransactionsWithWSMTXCAWebService.DefaultValue);
			AssertEquals("CountryFilterPK", CountryFilterPKs.Argentina, ItemSet.ReportDomesticTransactionsWithWSMTXCAWebService.CountryFilterPKs);

			ItemSet.ReportDomesticTransactionsWithWSMTXCAWebService.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			Assert("Overridden with different value", ItemSet.ReportDomesticTransactionsWithWSMTXCAWebService.Value);
		}

		public void TestLimitNumberOfInvoicesCanSimultaneouslyBeSentToXT()
		{
			AssertEquals("Name", nameof(ItemSet.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT), ItemSet.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations, ItemSet.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.Category);
			AssertEquals("Caption", "Limit the number of invoices simultaneously sent to XT (CargoWiseOne Support Only)", ItemSet.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.Caption);
			AssertEquals("Hint", @"This registry configures the maximum number of invoices that can be sent to XT for processing at the same time. You can use this registry to throttle the number of invoices concurrently processed on XT. You may need this feature if you want to avoid overwhelming XT by sending too many invoices. If you do not want to throttle, please set the value to 0.", ItemSet.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.Hint);

			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.Options);
			AssertEquals("DefaultValue", 0, ItemSet.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.DefaultValue);

			ItemSet.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 4);
			AssertEquals("Overridden with different value", 4, ItemSet.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.Value);
		}

		public void TestDefaultValueOfLimitNumberOfInvoicesCanSimultaneouslyBeSentToXT_ForSACompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.SaudiArabia))
			{
				AssertEquals("DefaultValue", 1, ItemSet.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.DefaultValue);

				ItemSet.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 10);
				AssertEquals("Overridden with different value", 10, ItemSet.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.Value);
			}
		}

		public void TestEnableEInvoicingQRCode()
		{
			TestGenericRegistryItem(ItemSet.EnableEInvoicingQRCode, "EnableEInvoicingQRCode", Categories.Accounting_EReportingAndEInvoicingConfigurations_Malaysia, "Print E-Invoice QR Code", @"This registry is used to control the Malaysia E-Invoice QR Code. For Malaysia Electronic Invoice, it is required to include e-Invoice QR Code in the invoice document.
By default this registry is not enabled. E-Invoice QR Code does not display on invoice documents. 
When enabled, if QR Code is available it will be included in the Malaysia invoice document.", RegistryStorageFlags.Company, RegistryOptions.Default);

			AssertEquals("CountryFilterPK", CountryFilterPKs.Malaysia, ItemSet.EnableEInvoicingQRCode.CountryFilterPKs);

			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Print E-Invoice QR Code = [True].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.EnableEInvoicingQRCode, oldValue, newValue);
			var logReference = ItemSet.EnableEInvoicingQRCode.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestElectronicInvoiceDocumentType()
		{
			var item = ItemSet.ElectronicInvoiceDocumentType;
			AssertEquals("ElectronicInvoiceDocumentType", item.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_Malaysia, item.Category);
			AssertEquals("Electronic Invoice Document Type", item.Caption);
			AssertEquals(@"This registry is used to define the Document Type of the Malaysia Electronic Invoice Document automatically attached to eDocs tab.
By default, the value is INV - Invoice.", item.Hint);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals(RegistryOptions.CacheExpensiveDefaultValue, item.Options);
			AssertContainsExactElementsInAnyOrder("CountryFilterPKs", new[] { Core.Constants.CountryGuids.Malaysia }, ItemSet.ElectronicInvoiceDocumentType.CountryFilterPKs);

			var codeDescriptionpairList = ((CodePairRegistryDataType)item.DataType).LookUpList;
			var expectedDocTypes = "ACV, BOA, BOD, BRC, CAT, CLL, COM, COO, COT, CRP, DDR, DEC, EXD, FCR, GLJ, IEP, INV, JRJ, MCD, MSC, MSD, NAF, PAY, PER, PIN, PPO, PRV, PUB, QRA, SCCD, SIMG, SREP, TDM";
			AssertEquals(expectedDocTypes, codeDescriptionpairList.CodesAsString);
			AssertEquals("INV", ItemSet.ElectronicInvoiceDocumentType.DefaultValue);

			AssertExceptionThrown<RegistryValidationException>("Invalid Selection. Please choose a code from the list.", () => item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TEST"));
		}
	}
}

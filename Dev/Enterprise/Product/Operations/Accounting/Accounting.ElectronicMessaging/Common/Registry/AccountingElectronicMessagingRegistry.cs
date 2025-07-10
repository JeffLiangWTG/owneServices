using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Security.Cryptography;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Accounting.ElectronicMessaging.Common.Registry;
using Enterprise.Accounting.ElectronicMessaging.Malaysia;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Registry
{
	public sealed class AccountingElectronicMessagingRegistry : RegistryItemSet
	{
		public static AccountingElectronicMessagingRegistry Instance
		{
			get { return instance ?? (instance = new AccountingElectronicMessagingRegistry()); }
		}
		[ThreadStatic]
		static AccountingElectronicMessagingRegistry instance;

		public override bool IsForProductivityWise => true;

		public abstract class Categories : AccountingMasterFilesRegistry.Categories
		{
			public static MultilingualString Accounting_EReportingAndEInvoicingConfigurations_Argentina => CombineCategories(Accounting_EReportingAndEInvoicingConfigurations, ResString.GetMultilingualString("DBF90D89-6A5B-45B1-9525-1EEA15739311", "Argentina (AR)"));

			public static MultilingualString Accounting_EReportingAndEInvoicingConfigurations_Egypt => CombineCategories(Accounting_EReportingAndEInvoicingConfigurations, ResString.GetMultilingualString("2fae3860-3e8f-4dff-9912-0862c0950137", "Egypt (EG)"));

			public static MultilingualString Accounting_EReportingAndEInvoicingConfigurations_Jordan => CombineCategories(Accounting_EReportingAndEInvoicingConfigurations, ResString.GetMultilingualString("F7689A31-F64E-4D34-8C39-A22EDBFD12DC", "Jordan (JO)"));

			public static MultilingualString Accounting_EReportingAndEInvoicingConfigurations_Poland => CombineCategories(Accounting_EReportingAndEInvoicingConfigurations, ResString.GetMultilingualString("12ea49a9-30e7-4697-854f-ef415416370d", "Poland (PL)"));

			public static MultilingualString Accounting_EReportingAndEInvoicingConfigurations_Mauritius => CombineCategories(Accounting_EReportingAndEInvoicingConfigurations, ResString.GetMultilingualString("a388f704-c934-49af-b412-2ae28ec03cd8", "Mauritius (MU)"));
		}

		public IntRegistryItem EInvoicingBatchSize
			=> GetItem(nameof(EInvoicingBatchSize), () =>
				new IntRegistryItem(nameof(EInvoicingBatchSize),
					Categories.Accounting_EReportingAndEInvoicingConfigurations,
					(NoResString)"E-Reporting Batch Size (CargoWiseOne Support Only)", // CWSupport only Registry Item
					(NoResString)@"This registry defines the batch size of E-Invoicing GEI Messages for countries that support batching.

The following countries support batches and against each is the default system defined batch size:
 - Egypt: 98
 - Korea South: 98
 - Poland: 3
 - Spain: 98
 
Set this registry to a number greater than zero to override the default batch number.
Note that architectural limitations prevent batches greater than 98.", // CWSupport only Registry Item
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					0, 0, MaximumBatchSizeSupportedInXUEProcessor)
			);

		public const int MaximumBatchSizeSupportedInXUEProcessor = 98;

		public IntRegistryItem RomaniaEReportingDelayTimeForQueryInvoiceRequest
		{
			get
			{
				var item = GetItem("RomaniaEReportingDelayTimeForQueryInvoiceRequest", delegate
				{
					var item = new IntRegistryItem(
						"RomaniaEReportingDelayTimeForQueryInvoiceRequest",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Romania,
						ResString.GetMultilingualString("3DE90E88-0C5F-4562-A536-ED2C0778682F", "E-Reporting Delay Time For Query Invoice Request (CargoWiseOne Support Only)"),
						ResString.GetMultilingualString("746C98C2-4762-467F-ACE1-20014CBB6FB0", @"By default, the delay time is set to 15 minutes.
However, should there be a need to get an approval status quicker, we can adjust the delay time so that a query can be submitted to obtain the status earlier."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsHidden,
						15, minValue: 0, maxValue: 1440)
					{
						CountryFilterPKs = CountryFilterPKs.Romania
					};
					item.OnBuildLogReference += args => ResString.GetMultilingualString("655009AE-DB21-4EFF-8FC5-F9E1F53CEAFF", "The value changed from [{0}] to [{1}]", args.OriginalValue, args.NewValue);
					return item;
				});
				item.Options = AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden;
				return item;
			}
		}

		public DateTimeRegistryItem QueueOldTransactionsFromDateReceivables
		{
			get
			{
				var item = GetItem("QueueOldTransactionsFromDateReceivables", () => new EnableEInvoicingFunctionalityDateTimeRegistryItem("QueueOldTransactionsFromDateReceivables",
					AccountingMasterFilesRegistry.Categories.Accounting_EReportingAndEInvoicingConfigurations,
					ResString.GetMultilingualString("3319EDE3-A78B-4785-A1A0-DB1524861E48", "Queue Old Transactions From Date - Receivables"),
					ResString.GetMultilingualString("AF82832A-8F5D-4D58-B506-01FAE91A2E5C", @"Once a new date is entered, the Batch Queue Invoice For e-Invoicing Service Task (BQI) will queue 500 transactions, which were posted from the entered date, at every run.
When all those transactions were posted, the entered date of this registry item will be reset to empty, and the processing of old transactions will be stopped."),
					RegistryStorageFlags.Company,
					RegistryOptions.IsHidden)
				{
					CountryFilterPKs = QueueOldTransactionsForEInvoicingCountryHelper.GetQueueOldTransactionsCountryPKs()
				});
				item.Options = AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value ? RegistryOptions.Default : RegistryOptions.IsHidden;
				return item;
			}
		}

		public IntRegistryItem EReportingAutomaticRetryLimit
		{
			get
			{
				var item = GetItem(nameof(EReportingAutomaticRetryLimit),
					() => new IntRegistryItem(nameof(EReportingAutomaticRetryLimit),
						AccountingMasterFilesRegistry.Categories.Accounting_EReportingAndEInvoicingConfigurations,
						ResString.GetMultilingualString("99cd908b-eae1-4e90-b6cd-820ff90f4930", "E-Reporting Automatic Retry Limit"),
						ResString.GetMultilingualString("814b52a9-425f-47c0-9003-7fcc7b23b53a", @"This registry item is referenced by Turkey login companies only.

When electronic invoicing is enabled for your Login company, CargoWise sends messages to the external provider. Occasionally there may be an interruption to receiving the expected response.
By default, CargoWise will automatically retry to communicate to receive the response message.  This registry item sets the number of times CargoWise should retry. If CargoWise is still detecting an issue after the nominated number of retries, an error will be shown against the transaction in the e-Reporting Message field. 

The default value is 5 retry attempts. The maximum number of retries allowed is 10 retries. 
If the number of retries is set to 0, the failure will be reported immediately on the transaction without any attempt to automatically retry to resolve the issue."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsHidden,
						defaultValue: 5,
						minValue: 0,
						maxValue: 10)
				{ CountryFilterPKs = CountryFilterPKs.Turkey });
				item.Options = AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeatures || AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeatures ? RegistryOptions.Default : RegistryOptions.IsHidden;
				return item;
			}
		}

		#region Country Specific Items

		public EInvoicingCredentialsRegistryItem EgyptEInvoicingCredentials
			=> GetItem(nameof(EgyptEInvoicingCredentials), () =>
				new EInvoicingCredentialsRegistryItem(nameof(EgyptEInvoicingCredentials),
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Egypt,
					ResString.GetMultilingualString("23cf15c5-044e-42ab-9a0f-b36abdf48773", "E-Invoicing Credentials"),
					ResString.GetMultilingualString("d40f8bdb-70fb-498b-ae26-cb40aa912585", @"This registry defines the Client ID, Client Secret and API Key used to process Egypt E-Invoices.

API Key must be entered in the Egypt Invoicing Portal when registering or editing the CargoWise to receive notifications for the final invoice status. If you do not generate the API Key and enter it into your Egypt Invoicing Portal for CargoWise, the e-reporting status of the transactions can not be updated to Successful (SUC) or Failed (FAL), and will remain in Delivered (DLV) status.

Important: Whenever you generate a new API Key, the existing API Key must be changed in the Egypt Invoicing Portal for the CargoWise."),
					RegistryStorageFlags.Company,
					RegistryOptions.Default,
					EInvoicingCredentialsRegistryItem.Behavior.HasAPIKey | EInvoicingCredentialsRegistryItem.Behavior.SetPasswordChar,
					new APIKeyGeneratorStrategy(new AESCrypto(HashAlgorithmName.SHA256))));

		public BooleanRegistryItem EnableReceivingEInvoiceStatusNotification
			=> GetItem(nameof(EnableReceivingEInvoiceStatusNotification), () =>
				new BooleanRegistryItem(nameof(EnableReceivingEInvoiceStatusNotification),
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Egypt,
					ResString.GetMultilingualString("331B4F86-CB20-4FB1-B507-CD199F55DDCC", "Enable Receiving e-Invoice Status Notification"),
					ResString.GetMultilingualString("F44CD1BB-DF7E-49F0-B78C-B05F4DA601CE", @"This registry enables e-invoice notifications for the status of eligible AR transactions, indicating whether they are valid or invalid according to the Egypt Tax Authority (ETA).

By default, the registry is set to 'No', when the notification configuration is completed in the ETA Invoicing Portal, it should be overridden and set to 'Yes'.

After completing the notification configuration and enabling this registry, the e-reporting status of eligible AR transactions will be set to 'Delivered - DLV.' It will then be updated to its final status based on the notification received from the ETA.

* The registry will be temporary and will be removed once the transition is complete."),
					RegistryStorageFlags.Company,
							RegistryOptions.Default,
							false)
				{ CountryFilterPKs = new Guid[] { Constants.CountryGuids.Egypt } });

		public StringRegistryItem MauritiusEInvoicingUsername
			=> GetItem("MauritiusEInvoicingUsername", () =>
				new StringRegistryItem("MauritiusEInvoicingUsername",
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Mauritius,
					ResString.GetMultilingualString("0562c4aa-170e-4f43-baf4-6daf794df71d", "E-Invoicing Username"),
					ResString.GetMultilingualString("9deab4ae-5cef-47fe-b4ea-a98130eb1aef", "This registry defines the User ID used to process Mauritius E-Invoices."),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default)
				{ CountryFilterPKs = CountryFilterPKs.Mauritius }
			);

		public StringRegistryItem MauritiusEInvoicingPassword
			=> GetItem("MauritiusEInvoicingPassword", () =>
				new StringRegistryItem("MauritiusEInvoicingPassword",
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Mauritius,
					ResString.GetMultilingualString("111d80cb-271f-4f5d-b62b-7eafacb5b1ba", "E-Invoicing Password"),
					ResString.GetMultilingualString("9664c558-86bc-45b5-be58-401b9a4c51bb", "This registry defines the Password used to process Mauritius E-Invoices."),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default)
				{
					CountryFilterPKs = CountryFilterPKs.Mauritius,
					EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password),
				}
			);

		public StringRegistryItem MauritiusEInvoicingEbsMraID
			=> GetItem("MauritiusEInvoicingEbsMraID", () =>
				new StringRegistryItem("MauritiusEInvoicingEbsMraID",
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Mauritius,
					ResString.GetMultilingualString("a1fed6cc-bd55-4182-9516-4ccff121fd6a", "E-Invoicing EBS MRA ID"),
					ResString.GetMultilingualString("2605e41d-8476-4d74-8dc7-75fdee1e657b", "This registry defines the EBS MRA ID used to process Mauritius E-Invoices."),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default,
					string.Empty)
				{ CountryFilterPKs = CountryFilterPKs.Mauritius }
			);

		public CodePairRegistryItem MexicoTaxRegimeID
		{
			get
			{
				var taxRegimeIdTypes = ObjectFactory.Get<IAccountingCountryComplianceGlobalFactory>().GetFeatureInterface<IDebtorTaxRegime>(CountryCodes.Mexico).GetTaxRegimeIdTypes();

				return GetItem("MexicoTaxRegimeID",
				  () => new CodePairRegistryItem(
					"MexicoTaxRegimeID",
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Mexico,
					(NoResString)"Mexico Tax Regime ID (CargoWiseOne Support Only)", // CWSupport only Registry Item
					(NoResString)"This registry provides Tax Regime ID for <RegimenFiscal> element in Mexico e-Invoicing XML mapping.", // CWSupport only Registry Item
					new CodeDescriptionPairListProvider(() => taxRegimeIdTypes),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					taxRegimeIdTypes[0].Code));
			}
		}

		public StringRegistryItem eInvoicingServicePointSuffix
			=> GetItem(nameof(eInvoicingServicePointSuffix),
				() => new StringRegistryItem(nameof(eInvoicingServicePointSuffix),
					Categories.Accounting_EReportingAndEInvoicingConfigurations,
					(NoResString)"E-Invoicing Testing Suffix for EInvoicingServicePoint (CargoWise Support Only)", // CWSupport only Registry Item
					(NoResString)@"The Suffix set in this registry is used in xT routing tables for testing purposes and on testing environments. Do not use this registry on production environments.

This Suffix is concatenated to the EInvoicingServicePoint being placed at the end of this DestinationParty.
E.g. for an EInvoicingServicePoint 'XHUB_AU_EINVOICING', setting this registry to ""ABC"" will change the EInvoicingServicePoint to 'XHUB_AU_EINVOICING_ABC'.", // CWSupport only Registry Item
					new StringRegistryDataType(0, 6),
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport));

		public BooleanRegistryItem ReportDomesticTransactionsWithWSMTXCAWebService
			=> GetItem(nameof(ReportDomesticTransactionsWithWSMTXCAWebService),
				() => new BooleanRegistryItem(nameof(ReportDomesticTransactionsWithWSMTXCAWebService),
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Argentina,
					   ResString.GetMultilingualString("264F23B7-8137-4A0B-813F-2547E273187F", "Report Domestic Transactions With WSMTXCA Web Service"),
					   ResString.GetMultilingualString("BAFF1FF8-D83F-4092-99B5-D29F60ACAF55", @"This registry is used to control which Web Service will be used to report domestic AR transactions to AFIP.

By default, domestic AR transactions are reported with the WSFEV Web Service.

If this registry is set to 'Yes', domestic AR transactions will be reported to AFIP using the WSMTXCA Web Service(with item detail), instead of using the WSFEV Web Service"),
					   RegistryStorageFlags.Company,
					   RegistryOptions.Default,
					   false)
				{ CountryFilterPKs = CountryFilterPKs.Argentina });

		public EInvoicingCredentialsRegistryItem PolandEInvoicingCredentials
			=> GetItem("PolandEInvoicingCredentials", () =>
				new EInvoicingCredentialsRegistryItem("PolandEInvoicingCredentials",
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Poland,
					ResString.GetMultilingualString("f513d302-86ea-4649-834b-ecacc5be70ee", "E-Invoicing Credentials"),
					ResString.GetMultilingualString("750d0400-ffff-43e3-b6bf-411c7fcd20d1", "This registry defines the KSeF Token Name and KSeF Token used to process Poland E-Invoices."),
					RegistryStorageFlags.Company,
					RegistryOptions.Default)
				{ CountryFilterPKs = CountryFilterPKs.Poland }
			);

		public SimultaneousInvoiceProcessingRateLimiterRegistryItem LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT
			=> GetItem(nameof(LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT),
				  () => new SimultaneousInvoiceProcessingRateLimiterRegistryItem(
								nameof(LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT),
								AccountingMasterFilesRegistry.Categories.Accounting_EReportingAndEInvoicingConfigurations,
								(NoResString)"Limit the number of invoices simultaneously sent to XT (CargoWiseOne Support Only)", // CWSupport only Registry Item
								(NoResString)@"This registry configures the maximum number of invoices that can be sent to XT for processing at the same time. You can use this registry to throttle the number of invoices concurrently processed on XT. You may need this feature if you want to avoid overwhelming XT by sending too many invoices. If you do not want to throttle, please set the value to 0.", // CWSupport only Registry Item
								RegistryStorageFlags.Company,
								RegistryOptions.IsOnlyForSupport,
								new List<(string countryCode, int defaultNumberOfInvoicesProcessedSimultaneously)> { (CountryCodes.SaudiArabia, 1) }.ToImmutableList()));

		public EInvoicingCredentialsRegistryItem MalaysiaEInvoicingCredentials
			=> GetItem(nameof(MalaysiaEInvoicingCredentials), () =>
				new EInvoicingCredentialsRegistryItem(nameof(MalaysiaEInvoicingCredentials),
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Malaysia,
					ResString.GetMultilingualString("16290F54-5568-4769-8C97-E1FE2FA1B631", "E-Invoicing Credentials"),
					ResString.GetMultilingualString("B656DB93-87A1-46B0-B525-7604614254AA", "This registry defines the Client ID and Client Secret used to process Malaysia E-Invoices."),
					RegistryStorageFlags.Company,
					RegistryOptions.Default,
					EInvoicingCredentialsRegistryItem.Behavior.SetPasswordChar)
				{ CountryFilterPKs = CountryFilterPKs.Malaysia }
		);

		public CodePairRegistryItem ElectronicInvoiceDocumentType
			=> GetItem(
				 nameof(ElectronicInvoiceDocumentType),
				 () => new CodePairRegistryItem(
					 new CountrySpecificDefaultValueRegistryItemImpl<string>(
						 nameof(ElectronicInvoiceDocumentType),
						 Categories.Accounting_EReportingAndEInvoicingConfigurations_Malaysia,
						 ResString.GetMultilingualString("D485CE96-87AD-4891-B65E-287835E67F52", @"This registry is used to define the Document Type of the Malaysia Electronic Invoice Document automatically attached to eDocs tab.
By default, the value is INV - Invoice."),
						 new CodePairRegistryDataType(new CodeDescriptionPairListProvider(() => DocTypeListProvider()), false, true),
						 RegistryOptions.CacheExpensiveDefaultValue,
						 FactoryForCountryDefaultValues,
						 new MalaysiaEInvoiceDocType_RegistryDescriptor()
						 )
					 )
				 { CountryFilterPKs = CountryFilterPKs.Malaysia }
				 );

		CodeDescriptionPairList DocTypeListProvider()
		{
			var masterFactory = new DocumentScanning.Business.DocumentFactoryProvider().GetFactory(FactoryForCountryDefaultValues);
			var docTypeCategoryQuery = new DocTypeCategoryQuery(masterFactory, new ZString(Constants.ReferenceTypes.Accounting));

			return DocumentScanning.Business.DocScanningHelper.GetCategoryDocTypesFromJobType(docTypeCategoryQuery, masterFactory, true);
		}

		BusinessObjectFactory FactoryForCountryDefaultValues
			=> factoryForCountryDefaultValues ?? (factoryForCountryDefaultValues = new BusinessObjectFactory() { NameForDebugging = "AccountingConfigurationRegistry Factory for Country Defaults" });

		[ThreadStatic]
		static BusinessObjectFactory factoryForCountryDefaultValues;

		public BooleanRegistryItem EnableEInvoicingQRCode
		{
			get
			{
				var item = GetItem("EnableEInvoicingQRCode", () =>
					new BooleanRegistryItem("EnableEInvoicingQRCode",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Malaysia,
						ResString.GetMultilingualString("F760DD24-3824-4115-B3FE-F9336A189224", "Print E-Invoice QR Code"),
						ResString.GetMultilingualString("60BAC544-4911-46B3-95F4-B6D1C4767241", @"This registry is used to control the Malaysia E-Invoice QR Code. For Malaysia Electronic Invoice, it is required to include e-Invoice QR Code in the invoice document.
By default this registry is not enabled. E-Invoice QR Code does not display on invoice documents. 
When enabled, if QR Code is available it will be included in the Malaysia invoice document."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Malaysia }
					);

				item.OnBuildLogReference += (args) => Res.GetString("CECDBB27-D066-4CEB-AC3D-6EEB8119C346", "Print E-Invoice QR Code = [{0}].", args.NewValue);

				return item;
			}
		}

		public EInvoicingCredentialsRegistryItem JordanEInvoicingCredentials
			=> GetItem(nameof(JordanEInvoicingCredentials), () =>
				new EInvoicingCredentialsRegistryItem(nameof(JordanEInvoicingCredentials),
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Jordan,
						ResString.GetMultilingualString("C9239174-3A0B-4BCB-89FB-83A92CBCA334", "E-Invoicing Credentials"),
						ResString.GetMultilingualString("1124E09C-2404-4D5F-A9E1-55E579574A1B", "This registry defines the Client ID and Client Secret used to process Jordan E-Invoices."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						EInvoicingCredentialsRegistryItem.Behavior.SetPasswordChar)
				{ CountryFilterPKs = CountryFilterPKs.Jordan }
			);

		#endregion
	}
}

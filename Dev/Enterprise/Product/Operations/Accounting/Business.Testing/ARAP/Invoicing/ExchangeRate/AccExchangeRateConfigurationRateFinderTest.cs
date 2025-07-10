using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing
{
	class AccExchangeRateConfigurationRateFinderTest : TestCaseWithFactory
	{
		[TestDate(2023, 01, 16)]
		[DisableZeroExchangeRateOverriding]
		public void TestGetExchangeRateWithInvoiceCurrencyType()
		{
			var company = ObjectCreator.CreateNewCompany("ABC");
			var branch = ObjectCreator.CreateBranch("DEF", company);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				const decimal defaultRateForUSD = 5.5m;
				const decimal defaultRateForEUR = 2.5m;
				ObjectCreator.USD.SetCustomsRate(new ZDateTime(2023, 1, 1), new ZDateTime(2023, 12, 1), defaultRateForUSD);

				var exRate = Factory.NewWithValidTestData<RefExchangeRate>();
				exRate.RE_GC = company.PK;
				exRate.RE_RX_NKExCurrency = ObjectCreator.EUR.RX_Code;
				exRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
				exRate.RE_StartDate = new ZDateTime(2023, 1, 16);
				exRate.RE_ExpiryDate = new ZDateTime(2023, 1, 16);
				exRate.RE_SellRate = defaultRateForEUR;

				CreateAccExchangeRateConfiguration(company, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.BuyRate, JobBillingExchangeRatePreference.Code.TodaysRate, -10, false, InvoicePostingExchangeRateCurrencyType.Code.Local,
					currencyCode: CurrencyCodes.EuropeanUnion);
				CreateAccExchangeRateConfiguration(company, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.CustomsRate, JobBillingExchangeRatePreference.Code.TodaysRate, -10, false, InvoicePostingExchangeRateCurrencyType.Code.Foreign,
					currencyCode: CurrencyCodes.UnitedStates);

				var shipment = ObjectCreator.CreateShipment("S0001000");
				var job = ObjectCreator.CreateJob(shipment);
				job.JH_GC = company.PK;
				Factory.Save();

				ExchangeRateReader.GetReaderInstance().ClearCache();
				AssertEquals("PreCondition", ExchangeRateTypes.Code.BuyRate, SystemDefaultExchangeRateConfig.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType);
				AssertEquals("PreCondition", JobBillingExchangeRatePreference.Code.TodaysRate, SystemDefaultExchangeRateConfig.JCE_Preference);
				AssertEquals("PreCondition", 0, SystemDefaultExchangeRateConfig.JCE_Offset);
				AssertEquals("PreCondition", InvoiceCurrencyType.NotApplicable, ExchangeRateEnumsExtensions.GetInvoiceCurrencyTypeFromCode(SystemDefaultExchangeRateConfig.JCE_InvoiceCurrencyType));
				AssertExchangeRate("There is a Local config(CUS-TDR) but no exRate for offset(2023-01-07), so fallback to default config(BUY-TDR).",
					ObjectCreator.EUR,
					InvoiceCurrencyType.Local,
					defaultRateForEUR,
					InvoiceCurrencyType.NotApplicable
				);
				AssertExchangeRate("[EUR]There is no Foreign config, so fallback to default config(BUY-TDR).",
					ObjectCreator.EUR,
					InvoiceCurrencyType.Foreign,
					defaultRateForEUR,
					InvoiceCurrencyType.NotApplicable
				);
				AssertExchangeRate("[EUR]There is no NotApplicable config, so fallback to default config(BUY-TDR).",
					ObjectCreator.EUR,
					InvoiceCurrencyType.NotApplicable,
					defaultRateForEUR,
					InvoiceCurrencyType.NotApplicable
				);
				AssertExchangeRate("[USD]No config for Local, so fallback to default config(BUY-TDR). However, we have no rate for default config.",
					ObjectCreator.USD,
					InvoiceCurrencyType.Local,
					0m,
					InvoiceCurrencyType.NotApplicable
				);
				AssertExchangeRate("[USD]No config for NotApplicable, so fallback to default config(BUY-TDR). However, we have no rate for default config.",
					ObjectCreator.USD,
					InvoiceCurrencyType.NotApplicable,
					0m,
					InvoiceCurrencyType.NotApplicable
				);
				AssertExchangeRate($"[USD]The Foreign config is set for CUS rate({defaultRateForUSD}).",
					ObjectCreator.USD,
					InvoiceCurrencyType.Foreign,
					defaultRateForUSD,
					InvoiceCurrencyType.Foreign
				);

				var exRate2 = Factory.NewWithValidTestData<RefExchangeRate>();
				exRate2.RE_GC = company.PK;
				exRate2.RE_RX_NKExCurrency = ObjectCreator.EUR.RX_Code;
				exRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
				exRate2.RE_StartDate = new ZDateTime(2023, 1, 1);
				exRate2.RE_ExpiryDate = new ZDateTime(2023, 1, 7);
				exRate2.RE_SellRate = 1.5m;
				Factory.Save();

				ExchangeRateReader.GetReaderInstance().ClearCache();
				AssertExchangeRate("[EUR]Now we can use the exRate from Local config.",
					ObjectCreator.EUR,
					InvoiceCurrencyType.Local,
					1.5m,
					InvoiceCurrencyType.Local
				);
				AssertExchangeRate("[EUR]There is no Foreign config, so fallback to default config(BUY-TDR).",
					ObjectCreator.EUR,
					InvoiceCurrencyType.Foreign,
					defaultRateForEUR,
					InvoiceCurrencyType.NotApplicable
				);
				AssertExchangeRate("[EUR]There is no NotApplicable config, so fallback to default config(BUY-TDR).",
					ObjectCreator.EUR,
					InvoiceCurrencyType.NotApplicable,
					defaultRateForEUR,
					InvoiceCurrencyType.NotApplicable
				);

				void AssertExchangeRate(string comment, RefCurrency currency, InvoiceCurrencyType invoiceCurrencyType, decimal expectedRate, InvoiceCurrencyType expectedInvoiceCurrencyType)
				{
					var exchangeRate = AccExchangeRateConfigurationRateFinder.GetExchangeRate(job.ExchangeRateConfigurationRateConsumer, currency, null, ExchangeRateValidLedgerEnum.AR, invoiceCurrencyType);
					AssertEquals(comment, expectedRate, exchangeRate);
				}
			}
		}

		public void TestGetInvoiceCurrencyType()
		{
			var company = ObjectCreator.CreateNewCompany("ABC");

			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(InvoiceCurrencyType.NotApplicable, AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(company, ExchangeRateValidLedgerEnum.None, InvoiceCurrencyType.Foreign));
				AssertEquals(InvoiceCurrencyType.NotApplicable, AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(company, ExchangeRateValidLedgerEnum.AP, InvoiceCurrencyType.Foreign));
				AssertEquals(InvoiceCurrencyType.NotApplicable, AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(company, ExchangeRateValidLedgerEnum.AR, InvoiceCurrencyType.Foreign));
				AssertEquals(InvoiceCurrencyType.NotApplicable, AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(company, ExchangeRateValidLedgerEnum.AR, InvoiceCurrencyType.Local));
			}

			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(InvoiceCurrencyType.NotApplicable, AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(company, ExchangeRateValidLedgerEnum.None, InvoiceCurrencyType.Foreign));
				AssertEquals(InvoiceCurrencyType.NotApplicable, AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(company, ExchangeRateValidLedgerEnum.AP, InvoiceCurrencyType.Foreign));
				AssertEquals(InvoiceCurrencyType.Foreign, AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(company, ExchangeRateValidLedgerEnum.AR, InvoiceCurrencyType.Foreign));
				AssertEquals(InvoiceCurrencyType.Local, AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(company, ExchangeRateValidLedgerEnum.AR, InvoiceCurrencyType.Local));

				AssertEquals(InvoiceCurrencyType.NotApplicable, AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(GlbCompany.CurrentCompany, ExchangeRateValidLedgerEnum.AR, InvoiceCurrencyType.Foreign));
				AssertEquals(InvoiceCurrencyType.NotApplicable, AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(GlbCompany.CurrentCompany, ExchangeRateValidLedgerEnum.AR, InvoiceCurrencyType.Local));
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestGetExchangeRateConfigurationPromptAtCompanyLevel()
		{
			var companyA = ObjectCreator.CreateNewCompany("ABC");
			var companyB = ObjectCreator.CreateNewCompany("XYZ");
			var shipment = ObjectCreator.CreateShipment("S000029", "AUSYD", "NKAKL");
			var jobInCompanyA = ObjectCreator.CreateJob(shipment);
			jobInCompanyA.JH_GC = companyA.PK;
			Factory.Save();
			var jobInCompanyB = ObjectCreator.CreateJob(shipment);
			jobInCompanyB.JH_GC = companyB.PK;
			Factory.Save();

			AssertEquals("PreCondition", false, SystemDefaultExchangeRateConfig.JCE_Prompt);
			Assert("Should select prompt from default system level config.", !AccExchangeRateConfigurationRateFinder.GetExchangeRateConfigurationPromptAtCompanyLevel(jobInCompanyA.ExchangeRateConfigurationRateConsumer));
			Assert("Should select prompt from default system level config.", !AccExchangeRateConfigurationRateFinder.GetExchangeRateConfigurationPromptAtCompanyLevel(jobInCompanyB.ExchangeRateConfigurationRateConsumer));

			var configCompanyA = CreateAccExchangeRateConfiguration(companyA, JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.Export, ExchangeRateTypes.Code.SellRate, JobBillingExchangeRatePreference.Code.TodaysRate, 0
				, prompt: true, currencyCode: string.Empty);

			Assert("Should select prompt from company level config, since the configCompanyA has been ticked JCE_Prompt.", AccExchangeRateConfigurationRateFinder.GetExchangeRateConfigurationPromptAtCompanyLevel(jobInCompanyA.ExchangeRateConfigurationRateConsumer));
			Assert("Should select prompt from default system level config.", !AccExchangeRateConfigurationRateFinder.GetExchangeRateConfigurationPromptAtCompanyLevel(jobInCompanyB.ExchangeRateConfigurationRateConsumer));

			SystemDefaultExchangeRateConfig.JCE_Prompt = true;
			configCompanyA.JCE_Prompt = false;
			Factory.Save();

			var reloadFactory = new BusinessObjectFactory();
			var reloadedJobInCompanyA = reloadFactory.Load<Job>(jobInCompanyA.PK);
			reloadedJobInCompanyA.InitializeParentFromGenericJobWithoutSettingDefaults();
			var reloadedJobInCompanyB = reloadFactory.Load<Job>(jobInCompanyB.PK);
			reloadedJobInCompanyB.InitializeParentFromGenericJobWithoutSettingDefaults();
			AssertEquals("Should select prompt from company level config.", false, AccExchangeRateConfigurationRateFinder.GetExchangeRateConfigurationPromptAtCompanyLevel(reloadedJobInCompanyA.ExchangeRateConfigurationRateConsumer));
			AssertEquals("Should select prompt from system level config.", true, AccExchangeRateConfigurationRateFinder.GetExchangeRateConfigurationPromptAtCompanyLevel(reloadedJobInCompanyB.ExchangeRateConfigurationRateConsumer));
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestGetExchangeRateConfigurationRateType()
		{
			var organisation = ObjectCreator.AALSHI;
			var company = ObjectCreator.CreateNewCompany("ABC");
			var creditorGroup = ObjectCreator.CreateCreditorGroup();
			organisation.CompanyData.OB_OG_APCreditorGroup = creditorGroup.PK;
			var debtorGroup = ObjectCreator.CreateDebtorGroup();
			organisation.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;
			var shipment = ObjectCreator.CreateShipment("S000029");
			var job = ObjectCreator.CreateJob(shipment);
			job.JH_GC = company.PK;
			Factory.Save();

			AssertEquals("PreCondition", ExchangeRateType.Buy, SystemDefaultExchangeRateConfig.GetCurrencyConfig(ZString.Empty, ZDate.Empty).ExchangeRateType);
			AssertWithCurrency(CurrencyCodes.UnitedStates);
			AssertWithCurrency(CurrencyCodes.Canada);

			void AssertWithCurrency(string currencyCode)
			{
				ResetExchangeRateConfiguration();

				CombineAssertions("Should get exchange rate type from default setting.",
					() => AssertRateType(ExchangeRateType.Buy, ExchangeRateType.Buy, ExchangeRateType.Buy, ExchangeRateType.Buy, ExchangeRateType.Buy, ExchangeRateType.Buy, currencyCode)
				);

				CreateAccExchangeRateConfiguration(job.Company, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, TransportModes.All, FreightShipmentDirection.Code.All, ExchangeRateTypes.Code.SellRate, JobBillingExchangeRatePreference.Code.TodaysRate, 0, true, currencyCode: currencyCode);
				CreateAccExchangeRateConfiguration(job.Company, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsPayable, TransportModes.All, FreightShipmentDirection.Code.All, ExchangeRateTypes.Code.CustomsRate, JobBillingExchangeRatePreference.Code.TodaysRate, 0, true, currencyCode: currencyCode);
				CreateAccExchangeRateConfiguration(job.Company, JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.All, ExchangeRateTypes.Code.PeriodEndRate, JobBillingExchangeRatePreference.Code.TodaysRate, 0, true, currencyCode: currencyCode);

				AssertRateType(ExchangeRateType.PeriodEnd, ExchangeRateType.Sell, ExchangeRateType.Customs, ExchangeRateType.PeriodEnd, ExchangeRateType.Buy, ExchangeRateType.Buy, currencyCode);
				CombineAssertions("Should get exchange rate value from default setting since it is only one set for all currencies.",
					() => AssertRateType(ExchangeRateType.Buy, ExchangeRateType.Buy, ExchangeRateType.Buy, ExchangeRateType.Buy, ExchangeRateType.Buy, ExchangeRateType.Buy, string.Empty)
				);

				CreateAccExchangeRateConfiguration(debtorGroup, JobInvoicingConsumerTypes.Shipment.Code, TransportModes.All, FreightShipmentDirection.Code.All, ExchangeRateTypes.Code.IATARate, JobBillingExchangeRatePreference.Code.TodaysRate, 0, true, currencyCode: currencyCode);
				CreateAccExchangeRateConfiguration(creditorGroup, JobInvoicingConsumerTypes.Shipment.Code, TransportModes.All, FreightShipmentDirection.Code.All, ExchangeRateTypes.Code.C01Rate, JobBillingExchangeRatePreference.Code.TodaysRate, 0, true, currencyCode: currencyCode);
				Factory.Save();

				ReloadJobAndOrganisationForTest();
				AssertRateType(ExchangeRateType.PeriodEnd, ExchangeRateType.Sell, ExchangeRateType.Customs, ExchangeRateType.PeriodEnd, ExchangeRateType.IATA, ExchangeRateType.C01, currencyCode);
				CombineAssertions("Should get exchange rate value from default setting since it is only one set for all currencies.",
					() => AssertRateType(ExchangeRateType.Buy, ExchangeRateType.Buy, ExchangeRateType.Buy, ExchangeRateType.Buy, ExchangeRateType.Buy, ExchangeRateType.Buy, string.Empty)
				);

				CreateAccExchangeRateConfiguration(ObjectCreator.AALSHI.CompanyData, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, TransportModes.All, FreightShipmentDirection.Code.All, ExchangeRateTypes.Code.C02Rate, JobBillingExchangeRatePreference.Code.TodaysRate, 0, true, currencyCode: currencyCode);
				CreateAccExchangeRateConfiguration(ObjectCreator.AALSHI.CompanyData, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsPayable, TransportModes.All, FreightShipmentDirection.Code.All, ExchangeRateTypes.Code.C03Rate, JobBillingExchangeRatePreference.Code.TodaysRate, 0, true, currencyCode: currencyCode);
				Factory.Save();

				ReloadJobAndOrganisationForTest();
				AssertRateType(ExchangeRateType.PeriodEnd, ExchangeRateType.Sell, ExchangeRateType.Customs, ExchangeRateType.PeriodEnd, ExchangeRateType.C02, ExchangeRateType.C03, currencyCode);
				CombineAssertions("Should get exchange rate value from default setting since it is only one set for all currencies.",
					() => AssertRateType(ExchangeRateType.Buy, ExchangeRateType.Buy, ExchangeRateType.Buy, ExchangeRateType.Buy, ExchangeRateType.Buy, ExchangeRateType.Buy, string.Empty)
				);
			}

			void AssertRateType(ExchangeRateType expectedRateTypeForCompanyLevelEmptyLedger, ExchangeRateType expectedRateTypeForCompanyLevelARLedger, ExchangeRateType expectedRateTypeForCompanyLevelAPLedger, ExchangeRateType expectedRateTypeForOrgLevelEmptyLedger, ExchangeRateType expectedRateTypeForOrgLevelARLedger, ExchangeRateType expectedRateTypeForOrgLevelAPLedger, string currencyCode)
			{
				AssertEquals(expectedRateTypeForCompanyLevelEmptyLedger, AccExchangeRateConfigurationRateFinder.GetExchangeRateConfigurationRateType(job.ExchangeRateConfigurationRateConsumer, null, ExchangeRateValidLedgerEnum.None, currencyCode));
				AssertEquals(expectedRateTypeForCompanyLevelARLedger, AccExchangeRateConfigurationRateFinder.GetExchangeRateConfigurationRateType(job.ExchangeRateConfigurationRateConsumer, null, ExchangeRateValidLedgerEnum.AR, currencyCode));
				AssertEquals(expectedRateTypeForCompanyLevelAPLedger, AccExchangeRateConfigurationRateFinder.GetExchangeRateConfigurationRateType(job.ExchangeRateConfigurationRateConsumer, null, ExchangeRateValidLedgerEnum.AP, currencyCode));
				AssertEquals(expectedRateTypeForOrgLevelEmptyLedger, AccExchangeRateConfigurationRateFinder.GetExchangeRateConfigurationRateType(job.ExchangeRateConfigurationRateConsumer, organisation, ExchangeRateValidLedgerEnum.None, currencyCode));
				AssertEquals(expectedRateTypeForOrgLevelARLedger, AccExchangeRateConfigurationRateFinder.GetExchangeRateConfigurationRateType(job.ExchangeRateConfigurationRateConsumer, organisation, ExchangeRateValidLedgerEnum.AR, currencyCode));
				AssertEquals(expectedRateTypeForOrgLevelAPLedger, AccExchangeRateConfigurationRateFinder.GetExchangeRateConfigurationRateType(job.ExchangeRateConfigurationRateConsumer, organisation, ExchangeRateValidLedgerEnum.AP, currencyCode));
			}

			void ReloadJobAndOrganisationForTest()
			{
				var reloadFactory = new BusinessObjectFactory();
				job = reloadFactory.Load<Job>(job.PK);
				job.InitializeParentFromGenericJobWithoutSettingDefaults();
				organisation = reloadFactory.Load<OrgHeader>(organisation.PK);
			}
		}

		[DisableZeroExchangeRateOverriding]
		[TestDate(2018, 03, 10)]
		public void TestGetExchangeRate()
		{
			const int offsetForHEA = -1;
			SetUpExchangeRates(out decimal todaysBuyRate, out decimal todaysSellRate, out decimal todaysCustomsRate, out decimal todaysIATARate, out decimal todaysC01Rate, out decimal todaysC02Rate, out decimal todaysC03Rate, out decimal todaysC04Rate);
			SetUpExchangeRatesWithLocalClient(out decimal todaysLocalClientBuyRate, out decimal todaysLocalClientSellRate, ObjectCreator.LocalClient.PK);

			var todaysBuyRateWithOffsetForHEA = GetExpectExchangeRateWithOffset(todaysBuyRate, offsetForHEA);
			var todaysLocalClientBuyRateWithOffsetForHEA = GetExpectExchangeRateWithOffset(todaysLocalClientBuyRate, offsetForHEA);

			var shipmentEstimatedArrivalDate = ZDateTime.Today.AddDays(offsetForHEA);
			var shipment = ObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			shipment.JS_E_ARV = shipmentEstimatedArrivalDate;
			var shipmentJob = ObjectCreator.CreateJob(shipment);
			shipmentJob.LocalChargesPK = ObjectCreator.LocalClient.PK;
			Factory.Save();

			var organisation = ObjectCreator.AALSHI;
			var companyData = ObjectCreator.AALSHI.CompanyData;
			var creditorGroup = ObjectCreator.CreateCreditorGroup();
			companyData.OB_OG_APCreditorGroup = creditorGroup.PK;
			var debtorGroup = ObjectCreator.CreateDebtorGroup();
			companyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;

			AssertEquals("PreCondition", ExchangeRateTypes.Code.BuyRate, SystemDefaultExchangeRateConfig.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType);
			AssertEquals("PreCondition", 0, SystemDefaultExchangeRateConfig.JCE_Offset);
			AssertRateWithoutLocalClient(todaysBuyRate, todaysBuyRate, todaysBuyRate, todaysBuyRate, todaysBuyRate, todaysBuyRate);
			AssertRateWithLocalClient(todaysLocalClientBuyRate, todaysLocalClientBuyRate, todaysLocalClientBuyRate, todaysLocalClientBuyRate, todaysLocalClientBuyRate, todaysLocalClientBuyRate);

			CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.All,
				ExchangeRateTypes.Code.BuyRate, JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalDate, 0, false, currencyCode: ObjectCreator.USD.RX_Code);
			CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.All,
				ExchangeRateTypes.Code.BuyRate, JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalDate, 1, false, currencyCode: string.Empty);

			ReloadJobAndOrganisationForTest();
			AssertRateWithoutLocalClient(
				todaysBuyRateWithOffsetForHEA,
				todaysBuyRateWithOffsetForHEA,
				todaysBuyRateWithOffsetForHEA,
				todaysBuyRateWithOffsetForHEA,
				todaysBuyRateWithOffsetForHEA,
				todaysBuyRateWithOffsetForHEA
			);
			AssertRateWithLocalClient(
				todaysLocalClientBuyRateWithOffsetForHEA,
				todaysLocalClientBuyRateWithOffsetForHEA,
				todaysLocalClientBuyRateWithOffsetForHEA,
				todaysLocalClientBuyRateWithOffsetForHEA,
				todaysLocalClientBuyRateWithOffsetForHEA,
				todaysLocalClientBuyRateWithOffsetForHEA
			);

			CreateAccExchangeRateConfiguration(debtorGroup, JobInvoicingConsumerTypes.Shipment.Code, TransportModes.All, FreightShipmentDirection.Code.All,
				ExchangeRateTypes.Code.SellRate, JobBillingExchangeRatePreference.Code.TodaysRate, 1, true, currencyCode: ObjectCreator.USD.RX_Code);
			CreateAccExchangeRateConfiguration(debtorGroup, JobInvoicingConsumerTypes.Shipment.Code, TransportModes.All, FreightShipmentDirection.Code.All,
				ExchangeRateTypes.Code.SellRate, JobBillingExchangeRatePreference.Code.TodaysRate, 2, true, currencyCode: string.Empty);
			CreateAccExchangeRateConfiguration(creditorGroup, JobInvoicingConsumerTypes.Shipment.Code, TransportModes.All, FreightShipmentDirection.Code.All,
				ExchangeRateTypes.Code.CustomsRate, JobBillingExchangeRatePreference.Code.TodaysRate, 1, true, currencyCode: ObjectCreator.USD.RX_Code);
			CreateAccExchangeRateConfiguration(creditorGroup, JobInvoicingConsumerTypes.Shipment.Code, TransportModes.All, FreightShipmentDirection.Code.All,
				ExchangeRateTypes.Code.CustomsRate, JobBillingExchangeRatePreference.Code.TodaysRate, 2, true, currencyCode: string.Empty);
			Factory.Save();

			ReloadJobAndOrganisationForTest();
			AssertRateWithoutLocalClient(
				todaysBuyRateWithOffsetForHEA,
				todaysBuyRateWithOffsetForHEA,
				todaysBuyRateWithOffsetForHEA,
				todaysBuyRateWithOffsetForHEA,
				GetExpectExchangeRateWithOffset(todaysSellRate, offsetFlag: 1),
				GetExpectExchangeRateWithOffset(todaysCustomsRate, offsetFlag: 1)
			);
			AssertRateWithLocalClient(
				todaysLocalClientBuyRateWithOffsetForHEA,
				todaysLocalClientBuyRateWithOffsetForHEA,
				todaysLocalClientBuyRateWithOffsetForHEA,
				todaysLocalClientBuyRateWithOffsetForHEA,
				GetExpectExchangeRateWithOffset(todaysLocalClientSellRate, offsetFlag: 1),
				GetExpectExchangeRateWithOffset(todaysCustomsRate, offsetFlag: 1)
			);

			CreateAccExchangeRateConfiguration(companyData, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, TransportModes.All, FreightShipmentDirection.Code.All,
				ExchangeRateTypes.Code.IATARate, JobBillingExchangeRatePreference.Code.TodaysRate, 0, true, currencyCode: ObjectCreator.USD.RX_Code);
			CreateAccExchangeRateConfiguration(companyData, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, TransportModes.All, FreightShipmentDirection.Code.All,
				ExchangeRateTypes.Code.IATARate, JobBillingExchangeRatePreference.Code.TodaysRate, 2, true, currencyCode: string.Empty);
			CreateAccExchangeRateConfiguration(companyData, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsPayable, TransportModes.All, FreightShipmentDirection.Code.All,
				ExchangeRateTypes.Code.C01Rate, JobBillingExchangeRatePreference.Code.TodaysRate, 0, true, currencyCode: ObjectCreator.USD.RX_Code);
			CreateAccExchangeRateConfiguration(companyData, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsPayable, TransportModes.All, FreightShipmentDirection.Code.All,
				ExchangeRateTypes.Code.C01Rate, JobBillingExchangeRatePreference.Code.TodaysRate, 2, true, currencyCode: string.Empty);
			Factory.Save();

			ReloadJobAndOrganisationForTest();

			ReloadJobAndOrganisationForTest();
			AssertRateWithoutLocalClient(
				todaysBuyRateWithOffsetForHEA,
				todaysBuyRateWithOffsetForHEA,
				todaysBuyRateWithOffsetForHEA,
				todaysBuyRateWithOffsetForHEA,
				todaysIATARate,
				todaysC01Rate
			);
			AssertRateWithLocalClient(
				todaysLocalClientBuyRateWithOffsetForHEA,
				todaysLocalClientBuyRateWithOffsetForHEA,
				todaysLocalClientBuyRateWithOffsetForHEA,
				todaysLocalClientBuyRateWithOffsetForHEA,
				todaysIATARate,
				todaysC01Rate
			);

			void ReloadJobAndOrganisationForTest()
			{
				var reloadFactory = new BusinessObjectFactory();
				shipmentJob = reloadFactory.Load<Job>(shipmentJob.PK);
				shipmentJob.InitializeParentFromGenericJobWithoutSettingDefaults();
				organisation = reloadFactory.Load<OrgHeader>(organisation.PK);
			}

			void AssertRateWithoutLocalClient(ZDecimal expectedRateForCompanyLevelEmptyLedger, ZDecimal expectedRateForCompanyLevelARLedger, ZDecimal expectedRateForCompanyLevelAPLedger, ZDecimal expectedRateForOrgLevelEmptyLedger, ZDecimal expectedRateForOrgLevelARLedger, ZDecimal expectedRateForOrgLevelAPLedger)
			{
				AssertRateCore(expectedRateForCompanyLevelEmptyLedger, expectedRateForCompanyLevelARLedger, expectedRateForCompanyLevelAPLedger, expectedRateForOrgLevelEmptyLedger, expectedRateForOrgLevelARLedger, expectedRateForOrgLevelAPLedger, false);
			}

			void AssertRateWithLocalClient(ZDecimal expectedRateForCompanyLevelEmptyLedger, ZDecimal expectedRateForCompanyLevelARLedger, ZDecimal expectedRateForCompanyLevelAPLedger, ZDecimal expectedRateForOrgLevelEmptyLedger, ZDecimal expectedRateForOrgLevelARLedger, ZDecimal expectedRateForOrgLevelAPLedger)
			{
				AssertRateCore(expectedRateForCompanyLevelEmptyLedger, expectedRateForCompanyLevelARLedger, expectedRateForCompanyLevelAPLedger, expectedRateForOrgLevelEmptyLedger, expectedRateForOrgLevelARLedger, expectedRateForOrgLevelAPLedger, true);
			}

			void AssertRateCore(ZDecimal expectedRateForCompanyLevelEmptyLedger, ZDecimal expectedRateForCompanyLevelARLedger, ZDecimal expectedRateForCompanyLevelAPLedger, ZDecimal expectedRateForOrgLevelEmptyLedger, ZDecimal expectedRateForOrgLevelARLedger, ZDecimal expectedRateForOrgLevelAPLedger, bool useLocalClient)
			{
				ExchangeRateReader.GetReaderInstance().ClearCache();
				AssertEquals(expectedRateForCompanyLevelEmptyLedger, AccExchangeRateConfigurationRateFinder.GetExchangeRate(shipmentJob.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, null, ExchangeRateValidLedgerEnum.None, useLocaClient: useLocalClient));
				AssertEquals(expectedRateForCompanyLevelARLedger, AccExchangeRateConfigurationRateFinder.GetExchangeRate(shipmentJob.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, null, ExchangeRateValidLedgerEnum.AR, useLocaClient: useLocalClient));
				AssertEquals(expectedRateForCompanyLevelAPLedger, AccExchangeRateConfigurationRateFinder.GetExchangeRate(shipmentJob.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, null, ExchangeRateValidLedgerEnum.AP, useLocaClient: useLocalClient));
				AssertEquals(expectedRateForOrgLevelEmptyLedger, AccExchangeRateConfigurationRateFinder.GetExchangeRate(shipmentJob.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, organisation, ExchangeRateValidLedgerEnum.None, useLocaClient: useLocalClient));
				AssertEquals(expectedRateForOrgLevelARLedger, AccExchangeRateConfigurationRateFinder.GetExchangeRate(shipmentJob.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, organisation, ExchangeRateValidLedgerEnum.AR, useLocaClient: useLocalClient));
				AssertEquals(expectedRateForOrgLevelAPLedger, AccExchangeRateConfigurationRateFinder.GetExchangeRate(shipmentJob.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, organisation, ExchangeRateValidLedgerEnum.AP, useLocaClient: useLocalClient));
			}
		}

		public void TestGetExchangeRateConfigurationByInvoiceCurrencyType()
		{
			var config1 = SystemDefaultExchangeRateConfig;
			config1.JCE_Prompt = true;
			var config2 = CreateAccExchangeRateConfiguration(
				GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, "", TransportModes.All, FreightShipmentDirection.Code.Export,
				ExchangeRateTypes.Code.BuyRate);
			var config3 = CreateAccExchangeRateConfiguration(
				ObjectCreator.AALSHI.CompanyData, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, TransportModes.All, FreightShipmentDirection.Code.All,
				ExchangeRateTypes.Code.C02Rate, invoiceCurrencyType: InvoicePostingExchangeRateCurrencyType.Code.Foreign);
			var config4 = CreateAccExchangeRateConfiguration(
				ObjectCreator.AALSHI.CompanyData, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, TransportModes.All, FreightShipmentDirection.Code.All,
				ExchangeRateTypes.Code.C03Rate);
			Factory.Save();

			var sydneyUnloco = "AUSYD";
			var aucklandUnloco = "NZAKL";

			var importConsol = ObjectCreator.CreateConsol(aucklandUnloco, sydneyUnloco, "C001001");
			var exportConsol = ObjectCreator.CreateConsol(sydneyUnloco, aucklandUnloco, "C001002");
			var importShipment = importConsol.Shipments.AddNew();
			importShipment.JS_RL_NKOrigin = aucklandUnloco;
			importShipment.JS_RL_NKDestination = sydneyUnloco;

			var importJob = ObjectCreator.CreateJob(importShipment);

			var exportShipment = exportConsol.Shipments.AddNew();
			exportShipment.JS_RL_NKOrigin = sydneyUnloco;
			exportShipment.JS_RL_NKDestination = aucklandUnloco;

			var exportJob = ObjectCreator.CreateJob(exportShipment);

			var consol = ObjectCreator.CreateGatewayConsol(sydneyUnloco, aucklandUnloco, "C001003", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var consolJob = new JobHeader.Loader(consol).TryLoadOrCreate() as Job;

			Factory.Save();

			AssertGetExchangeRateConfiguration(importJob, "[AR]Debtor Foreign Organisation C02",
				ObjectCreator.AALSHI, ExchangeRateValidLedgerEnum.AR,
				config3.PK,
				InvoiceCurrencyType.Foreign
			);

			AssertGetExchangeRateConfiguration(exportJob, "[AR]Debtor Foreign Configuration C02",
				ObjectCreator.AALSHI, ExchangeRateValidLedgerEnum.AR,
				config3.PK,
				InvoiceCurrencyType.Foreign
			);

			AssertGetExchangeRateConfiguration(importJob, "[AR]Debtor Local Configuration C03",
				ObjectCreator.AALSHI, ExchangeRateValidLedgerEnum.AR,
				config4.PK,
				InvoiceCurrencyType.Local
			);

			AssertGetExchangeRateConfiguration(exportJob, "[AR]Debtor Local Configuration C03",
				ObjectCreator.AALSHI, ExchangeRateValidLedgerEnum.AR,
				config4.PK,
				InvoiceCurrencyType.Local
			);

			AssertGetExchangeRateConfiguration(consolJob, "[AR]System Configuration BUY",
				ObjectCreator.AALSHI, ExchangeRateValidLedgerEnum.AR,
				config1.PK,
				InvoiceCurrencyType.Local
			);
		}

		[TestDate(2025, 2, 2)]
		public void TestGetPreferredExchangeRateDate()
		{
			var organisation = ObjectCreator.AALSHI;
			var companyData = ObjectCreator.AALSHI.CompanyData;

			CreateAccExchangeRateConfiguration(companyData, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, TransportModes.All, FreightShipmentDirection.Code.All,
				ExchangeRateTypes.Code.BuyRate, JobBillingExchangeRatePreference.Code.DeliveryDate, -10, false, InvoicePostingExchangeRateCurrencyType.Code.Local,
				currencyCode: CurrencyCodes.EuropeanUnion);

			Factory.Save();

			var deliveryDate = new ZDate(2025, 1, 1);
			var exchangeRateConfigurationRateConsumerMock = new Mock<IAccExchangeRateConfigurationRateConsumer>();
			exchangeRateConfigurationRateConsumerMock.Setup(x => x.Company).Returns(() => ObjectCreator.DefaultCompany);
			exchangeRateConfigurationRateConsumerMock.Setup(x => x.JobType).Returns(() => JobInvoicingConsumerTypes.Shipment.Code);
			exchangeRateConfigurationRateConsumerMock.Setup(x => x.Direction).Returns(() => FreightShipmentDirection.Code.All);
			exchangeRateConfigurationRateConsumerMock.Setup(x => x.TransportMode).Returns(() => TransportModes.All);
			exchangeRateConfigurationRateConsumerMock.Setup(x => x.DeliveryDate).Returns(() => deliveryDate);

			var exchangeRateDate = AccExchangeRateConfigurationRateFinder.GetPreferredExchangeRateDate(
				exchangeRateConfigurationRateConsumerMock.Object, organisation, CurrencyCodes.EuropeanUnion,
				ExchangeRateValidLedgerEnum.AR, InvoiceCurrencyType.Local);

			AssertEquals("The preferred exchange rate date should be the delivery date", deliveryDate, exchangeRateDate);

			exchangeRateDate = AccExchangeRateConfigurationRateFinder.GetPreferredExchangeRateDate(
				exchangeRateConfigurationRateConsumerMock.Object, organisation, CurrencyCodes.Australia,
				ExchangeRateValidLedgerEnum.AR, InvoiceCurrencyType.Local);

			AssertEquals("The preferred exchange rate date should be today's date", new ZDate(2025, 2, 2), exchangeRateDate);

			exchangeRateDate = AccExchangeRateConfigurationRateFinder.GetPreferredExchangeRateDate(
				null, organisation, CurrencyCodes.Australia,
				ExchangeRateValidLedgerEnum.AR, InvoiceCurrencyType.Local);

			AssertEquals("No preferred exchange rate date could be calculated", ZDate.Invalid, exchangeRateDate);
		}

		#region IsPreference Test Cases

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestIsPreferenceArrivalDateAtCompanyLevel_HistoricalRateFromActualArrivalDate()
		{
			TestPreferenceAtCompanyLevel(Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalDate, AccExchangeRateConfigurationRateFinder.IsPreferenceArrivalDateAtCompanyLevel);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestIsPreferenceArrivalDateAtCompanyLevel_HistoricalRateFromEstimatedArrivalDate()
		{
			TestPreferenceAtCompanyLevel(Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalDate, AccExchangeRateConfigurationRateFinder.IsPreferenceArrivalDateAtCompanyLevel);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestIsPreferenceDepartureDateAtCompanyLevel_HistoricalRateFromActualDepartureDate()
		{
			TestPreferenceAtCompanyLevel(Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualDepartureDate, AccExchangeRateConfigurationRateFinder.IsPreferenceDepartureDateAtCompanyLevel);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestIsPreferenceDepartureDateAtCompanyLevel_HistoricalRateFromEstimatedDepartureDate()
		{
			TestPreferenceAtCompanyLevel(Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedDepartureDate, AccExchangeRateConfigurationRateFinder.IsPreferenceDepartureDateAtCompanyLevel);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestIsPreferenceConsolExchangeRateAtCompanyLevel()
		{
			var companyA = ObjectCreator.CreateNewCompany("ABC");
			var companyB = ObjectCreator.CreateNewCompany("XYZ");

			var consol = ObjectCreator.CreateConsol();
			var shipment = ObjectCreator.CreateShipment("S000029", "AUSYD", "NKAKL", consol);

			var consolCostForCompanyA = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 10m);
			consolCostForCompanyA.E6_GC = companyA.PK;
			var consolCostForCompanyB = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC2, 10m);
			consolCostForCompanyB.E6_GC = companyB.PK;
			Factory.Save();

			var jobInCompanyA = ObjectCreator.CreateJob(shipment);
			jobInCompanyA.JH_GC = companyA.PK;
			Factory.Save();
			var jobInCompanyB = ObjectCreator.CreateJob(shipment);
			jobInCompanyB.JH_GC = companyB.PK;
			Factory.Save();

			AssertNotEquals("PreCondition", JobBillingExchangeRatePreference.Code.ConsolExchangeRate, SystemDefaultExchangeRateConfig.JCE_Preference);
			Assert("Should select preference from default system level config.", !AccExchangeRateConfigurationRateFinder.IsPreferenceConsolExchangeRateAtCompanyLevelForJobWithConsolDirectionAndConsolTransportMode(jobInCompanyA.ExchangeRateConfigurationRateConsumer, consolCostForCompanyA.ExchangeRateConfigurationRateConsumer, string.Empty));
			Assert("Should select preference from default system level config.", !AccExchangeRateConfigurationRateFinder.IsPreferenceConsolExchangeRateAtCompanyLevelForJobWithConsolDirectionAndConsolTransportMode(jobInCompanyB.ExchangeRateConfigurationRateConsumer, consolCostForCompanyB.ExchangeRateConfigurationRateConsumer, string.Empty));

			var configCompanyA = CreateAccExchangeRateConfiguration(companyA, JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.Export, ExchangeRateTypes.Code.SellRate,
				JobBillingExchangeRatePreference.Code.ConsolExchangeRate, currencyCode: string.Empty);

			Assert("Should select preference from company A config.", AccExchangeRateConfigurationRateFinder.IsPreferenceConsolExchangeRateAtCompanyLevelForJobWithConsolDirectionAndConsolTransportMode(jobInCompanyA.ExchangeRateConfigurationRateConsumer, consolCostForCompanyA.ExchangeRateConfigurationRateConsumer, string.Empty));
			Assert("Should select preference from system level config.", !AccExchangeRateConfigurationRateFinder.IsPreferenceConsolExchangeRateAtCompanyLevelForJobWithConsolDirectionAndConsolTransportMode(jobInCompanyB.ExchangeRateConfigurationRateConsumer, consolCostForCompanyB.ExchangeRateConfigurationRateConsumer, string.Empty));

			var configSystemForSHP = CreateAccExchangeRateConfiguration(JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.Export, ExchangeRateTypes.Code.SellRate,
				JobBillingExchangeRatePreference.Code.ConsolExchangeRate, currencyCode: string.Empty);
			Factory.Save();

			ReloadJobForTest();
			Assert("Should select preference from company A config.", AccExchangeRateConfigurationRateFinder.IsPreferenceConsolExchangeRateAtCompanyLevelForJobWithConsolDirectionAndConsolTransportMode(jobInCompanyA.ExchangeRateConfigurationRateConsumer, consolCostForCompanyA.ExchangeRateConfigurationRateConsumer, string.Empty));
			Assert("Should select preference from system level config.", AccExchangeRateConfigurationRateFinder.IsPreferenceConsolExchangeRateAtCompanyLevelForJobWithConsolDirectionAndConsolTransportMode(jobInCompanyB.ExchangeRateConfigurationRateConsumer, consolCostForCompanyB.ExchangeRateConfigurationRateConsumer, string.Empty));

			configCompanyA.JCE_Preference = JobBillingExchangeRatePreference.Code.TodaysRate;
			Factory.Save();

			ReloadJobForTest();
			Assert("Should select preference from company A config.", !AccExchangeRateConfigurationRateFinder.IsPreferenceConsolExchangeRateAtCompanyLevelForJobWithConsolDirectionAndConsolTransportMode(jobInCompanyA.ExchangeRateConfigurationRateConsumer, consolCostForCompanyA.ExchangeRateConfigurationRateConsumer, string.Empty));
			Assert("Should select preference from system level config.", AccExchangeRateConfigurationRateFinder.IsPreferenceConsolExchangeRateAtCompanyLevelForJobWithConsolDirectionAndConsolTransportMode(jobInCompanyB.ExchangeRateConfigurationRateConsumer, consolCostForCompanyB.ExchangeRateConfigurationRateConsumer, string.Empty));

			configSystemForSHP.JCE_Preference = JobBillingExchangeRatePreference.Code.TodaysRate;
			Factory.Save();

			ReloadJobForTest();
			Assert("Should select preference from company A config.", !AccExchangeRateConfigurationRateFinder.IsPreferenceConsolExchangeRateAtCompanyLevelForJobWithConsolDirectionAndConsolTransportMode(jobInCompanyA.ExchangeRateConfigurationRateConsumer, consolCostForCompanyA.ExchangeRateConfigurationRateConsumer, string.Empty));
			Assert("Should select preference from system level config.", !AccExchangeRateConfigurationRateFinder.IsPreferenceConsolExchangeRateAtCompanyLevelForJobWithConsolDirectionAndConsolTransportMode(jobInCompanyB.ExchangeRateConfigurationRateConsumer, consolCostForCompanyB.ExchangeRateConfigurationRateConsumer, string.Empty));

			void ReloadJobForTest()
			{
				var reloadFactory = new BusinessObjectFactory();
				jobInCompanyA = reloadFactory.Load<Job>(jobInCompanyA.PK);
				jobInCompanyA.InitializeParentFromGenericJobWithoutSettingDefaults();
				jobInCompanyB = reloadFactory.Load<Job>(jobInCompanyB.PK);
				jobInCompanyB.InitializeParentFromGenericJobWithoutSettingDefaults();
			}
		}

		public void TestIsPreferenceConsolExchangeRateAtCompanyLevel_Currency()
		{
			var consol = ObjectCreator.CreateConsol();
			var shipment = ObjectCreator.CreateShipment("S000029", "AUSYD", "NKAKL", consol);
			var shipmentJob = ObjectCreator.CreateJob(shipment);
			Factory.Save();

			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 10m);
			Factory.Save();

			var consolConsumer = consolCost.ExchangeRateConfigurationRateConsumer;
			var shipmentConsumer = shipmentJob.ExchangeRateConfigurationRateConsumer;

			CreateAccExchangeRateConfiguration(JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.All, ExchangeRateTypes.Code.SellRate,
				JobBillingExchangeRatePreference.Code.ConsolExchangeRate, currencyCode: string.Empty);
			CreateAccExchangeRateConfiguration(JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.All, ExchangeRateTypes.Code.SellRate,
				JobBillingExchangeRatePreference.Code.TodaysRate, currencyCode: ObjectCreator.USD.RX_Code);
			shipmentJob.Company.AccExchangeRateConfigurations.Reload(true);

			AssertPreferenceConsolExchangeRate("Should get preference CER(ConsolExchangeRate) from setting for All Currency since currency code is empty."
				, string.Empty
				, expectedResult: true
				, shipmentConsumer
				, consolConsumer
			);
			AssertPreferenceConsolExchangeRate("Should get preference CER(ConsolExchangeRate) from setting for All Currency since no secific setting for EUR."
				, ObjectCreator.EUR.RX_Code
				, expectedResult: true
				, shipmentConsumer
				, consolConsumer
			);
			AssertPreferenceConsolExchangeRate("Should get preference TDR(TodaysRate) from setting for USD."
				, ObjectCreator.USD.RX_Code
				, expectedResult: false
				, shipmentConsumer
				, consolConsumer
			);

			CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.All, ExchangeRateTypes.Code.SellRate,
				JobBillingExchangeRatePreference.Code.TodaysRate, currencyCode: string.Empty);
			CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.All, ExchangeRateTypes.Code.SellRate,
				JobBillingExchangeRatePreference.Code.ConsolExchangeRate, currencyCode: ObjectCreator.USD.RX_Code);
			shipmentJob.Company.AccExchangeRateConfigurations.Reload(true);

			AssertPreferenceConsolExchangeRate("Should get preference TDR(TodaysRate) from setting for All Currency at company level, since currency code is empty."
				, string.Empty
				, expectedResult: false
				, shipmentConsumer
				, consolConsumer
			);
			AssertPreferenceConsolExchangeRate("Should get preference TDR(TodaysRate) from setting for All Currency at company level, since no secific setting for EUR."
				, ObjectCreator.EUR.RX_Code
				, expectedResult: false
				, shipmentConsumer
				, consolConsumer
			);
			AssertPreferenceConsolExchangeRate("Should get preference CER(ConsolExchangeRate) from setting for USD at company level."
				, ObjectCreator.USD.RX_Code
				, expectedResult: true
				, shipmentConsumer
				, consolConsumer
			);
		}

		public void TestIsPreferenceConsolExchangeRateAtCompanyLevel_WithinCurrencyConfigDateRange()
		{
			var consol = ObjectCreator.CreateConsol();
			var shipment = ObjectCreator.CreateShipment("S000029", "AUSYD", "NKAKL", consol);
			var shipmentJob = ObjectCreator.CreateJob(shipment);
			Factory.Save();

			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 10m);
			Factory.Save();

			var consolConsumer = consolCost.ExchangeRateConfigurationRateConsumer;
			var shipmentConsumer = shipmentJob.ExchangeRateConfigurationRateConsumer;

			var companyExRateConfig_USD = CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.All, ExchangeRateTypes.Code.SellRate,
				JobBillingExchangeRatePreference.Code.ConsolExchangeRate, currencyCode: ObjectCreator.USD.RX_Code);

			var companyCurrencyConfig_USD = companyExRateConfig_USD.GetCurrencyConfig(ObjectCreator.USD.RX_Code, ZDate.Empty);
			companyCurrencyConfig_USD.JCT_StartDate = new ZDate(2025, 2, 2);
			companyCurrencyConfig_USD.JCT_ExpiryDate = new ZDate(2025, 2, 7);

			var getExchangeRateDateMock = MockIAccExchangeRateConfigurationsHelper(consolConsumer, "CER", new ZDateTime(2025, 2, 5));

			using (ObjectFactory.Substitute(getExchangeRateDateMock.Object))
			{
				AssertPreferenceConsolExchangeRate("Should get preference CER(ConsolExchangeRate) from setting for USD at company level."
					, ObjectCreator.USD.RX_Code
					, expectedResult: true
					, shipmentConsumer
					, consolConsumer
				);
			}

			getExchangeRateDateMock.Verify(x => x.GetExchangeRateDate(It.IsAny<IAccExchangeRateConfigurationRateConsumer>(), It.IsAny<ZString>()), Times.Exactly(3));
		}

		public void TestIsPreferenceConsolExchangeRateAtCompanyLevel_OutsideCurrencyConfigDateRange()
		{
			var consol = ObjectCreator.CreateConsol();
			var shipment = ObjectCreator.CreateShipment("S000029", "AUSYD", "NKAKL", consol);
			var shipmentJob = ObjectCreator.CreateJob(shipment);
			Factory.Save();

			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 10m);
			Factory.Save();

			var consolConsumer = consolCost.ExchangeRateConfigurationRateConsumer;
			var shipmentConsumer = shipmentJob.ExchangeRateConfigurationRateConsumer;

			var companyExRateConfig_USD = CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.All, ExchangeRateTypes.Code.SellRate,
				JobBillingExchangeRatePreference.Code.ConsolExchangeRate, currencyCode: ObjectCreator.USD.RX_Code);

			var companyCurrencyConfig_USD = companyExRateConfig_USD.GetCurrencyConfig(ObjectCreator.USD.RX_Code, ZDate.Empty);
			companyCurrencyConfig_USD.JCT_StartDate = new ZDate(2025, 2, 2);
			companyCurrencyConfig_USD.JCT_ExpiryDate = new ZDate(2025, 2, 7);

			var getExchangeRateDateMock = MockIAccExchangeRateConfigurationsHelper(consolConsumer, "CER", new ZDateTime(2025, 2, 10));

			using (ObjectFactory.Substitute(getExchangeRateDateMock.Object))
			{
				AssertPreferenceConsolExchangeRate("Should get preference TDR(TodaysRate) from setting for All Currency at company level, since no valid date range for USD currency code."
					, ObjectCreator.USD.RX_Code
					, expectedResult: false
					, shipmentConsumer
					, consolConsumer
				);
			}

			getExchangeRateDateMock.Verify(x => x.GetExchangeRateDate(It.IsAny<IAccExchangeRateConfigurationRateConsumer>(), It.IsAny<ZString>()), Times.Exactly(3));
		}

		void TestPreferenceAtCompanyLevel(ZString preferenceToTest, Func<IAccExchangeRateConfigurationRateConsumer, bool> methodToTest)
		{
			var companyA = ObjectCreator.CreateNewCompany("ABC");
			var companyB = ObjectCreator.CreateNewCompany("XYZ");
			var shipment = ObjectCreator.CreateShipment("S000029", "AUSYD", "NKAKL");
			var jobInCompanyA = ObjectCreator.CreateJob(shipment);
			jobInCompanyA.JH_GC = companyA.PK;
			Factory.Save();
			var jobInCompanyB = ObjectCreator.CreateJob(shipment);
			jobInCompanyB.JH_GC = companyB.PK;
			Factory.Save();

			AssertNotEquals("PreCondition", preferenceToTest, SystemDefaultExchangeRateConfig.JCE_Preference);
			Assert("Should select preference from default system level config.", !methodToTest(jobInCompanyA.ExchangeRateConfigurationRateConsumer));
			Assert("Should select preference from default system level config.", !methodToTest(jobInCompanyB.ExchangeRateConfigurationRateConsumer));

			var configCompanyA = CreateAccExchangeRateConfiguration(companyA, JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.Export, ExchangeRateTypes.Code.SellRate,
				preferenceToTest, currencyCode: string.Empty);

			Assert("Should select preference from company A config.", methodToTest(jobInCompanyA.ExchangeRateConfigurationRateConsumer));
			Assert("Should select preference from system level config.", !methodToTest(jobInCompanyB.ExchangeRateConfigurationRateConsumer));

			var configSystemForSHP = CreateAccExchangeRateConfiguration(JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.Export, ExchangeRateTypes.Code.SellRate,
				preferenceToTest, currencyCode: string.Empty);
			Factory.Save();

			ReloadJobForTest();
			Assert("Should select preference from company A config.", methodToTest(jobInCompanyA.ExchangeRateConfigurationRateConsumer));
			Assert("Should select preference from system level config.", methodToTest(jobInCompanyB.ExchangeRateConfigurationRateConsumer));

			configCompanyA.JCE_Preference = JobBillingExchangeRatePreference.Code.TodaysRate;
			AssertNotEquals("PreCondition", preferenceToTest, configCompanyA.JCE_Preference);
			Factory.Save();

			ReloadJobForTest();
			Assert("Should select preference from company A config.", !methodToTest(jobInCompanyA.ExchangeRateConfigurationRateConsumer));
			Assert("Should select preference from system level config.", methodToTest(jobInCompanyB.ExchangeRateConfigurationRateConsumer));

			configSystemForSHP.JCE_Preference = JobBillingExchangeRatePreference.Code.TodaysRate;
			AssertNotEquals("PreCondition", preferenceToTest, configSystemForSHP.JCE_Preference);
			Factory.Save();

			ReloadJobForTest();
			Assert("Should select preference from company A config.", !methodToTest(jobInCompanyA.ExchangeRateConfigurationRateConsumer));
			Assert("Should select preference from system level config.", !methodToTest(jobInCompanyB.ExchangeRateConfigurationRateConsumer));

			void ReloadJobForTest()
			{
				var reloadFactory = new BusinessObjectFactory();
				jobInCompanyA = reloadFactory.Load<Job>(jobInCompanyA.PK);
				jobInCompanyA.InitializeParentFromGenericJobWithoutSettingDefaults();
				jobInCompanyB = reloadFactory.Load<Job>(jobInCompanyB.PK);
				jobInCompanyB.InitializeParentFromGenericJobWithoutSettingDefaults();
			}
		}

		#endregion

		#region Exchange Rate For Date Test Cases

		[TestDate(2018, 03, 10)]
		public void TestTodaysExchangeRate()
		{
			TestExchangeRateForDate(true, false, InvoiceCurrencyType.NotApplicable);
		}

		[TestDate(2018, 03, 10)]
		public void TestTodaysExchangeRate_ForeignInvoiceCurrencyType()
		{
			TestExchangeRateForDate(true, false, InvoiceCurrencyType.Foreign);
		}

		[TestDate(2018, 03, 10)]
		public void TestTodaysExchangeRate_LocalInvoiceCurrencyType()
		{
			TestExchangeRateForDate(true, false, InvoiceCurrencyType.Local);
		}

		[TestDate(2018, 03, 10)]
		public void TestGetExchangeRateForDate()
		{
			TestExchangeRateForDate(false, false, InvoiceCurrencyType.NotApplicable);
		}

		[TestDate(2018, 03, 10)]
		public void TestGetExchangeRateForDate_ForeignInvoiceCurrencyType()
		{
			TestExchangeRateForDate(false, false, InvoiceCurrencyType.Foreign);
		}

		[TestDate(2018, 03, 10)]
		public void TestGetExchangeRateForDate_LocalInvoiceCurrencyType()
		{
			TestExchangeRateForDate(false, false, InvoiceCurrencyType.Local);
		}

		[TestDate(2018, 03, 10)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestGetExchangeRateForDate_UseBuyRateAsFallback()
		{
			var testDate = ZDate.Today;
			var todaysBuyRate = 0.123m;
			CreateExchangeRate(Core.Constants.ExchangeRateTypes.Code.BuyRate, todaysBuyRate, ZGuid.Empty);
			var company = ObjectCreator.CreateNewCompany("ABC");
			var shipment = ObjectCreator.CreateShipment("S000029");
			var job = ObjectCreator.CreateJob(shipment);
			job.JH_GC = company.PK;
			Factory.Save();

			job.JH_GC = ZGuid.Empty;//rate consumer company is null, config will be null

			AssertRateWithUseBuyRateAsFallBack(null, ExchangeRateValidLedgerEnum.None, todaysBuyRate, 0m);
			AssertRateWithUseBuyRateAsFallBack(null, ExchangeRateValidLedgerEnum.AR, todaysBuyRate, 0m);
			AssertRateWithUseBuyRateAsFallBack(null, ExchangeRateValidLedgerEnum.AP, todaysBuyRate, 0m);
			AssertRateWithUseBuyRateAsFallBack(ObjectCreator.AALSHI, ExchangeRateValidLedgerEnum.None, todaysBuyRate, 0m);

			void AssertRateWithUseBuyRateAsFallBack(OrgHeader organisation, ExchangeRateValidLedgerEnum ledger, ZDecimal expectedRateWithFallback, ZDecimal expectedRateWithoutFallback)
			{
				ExchangeRateReader.GetReaderInstance().ClearCache();
				AssertEquals(expectedRateWithFallback, AccExchangeRateConfigurationRateFinder.GetExchangeRateForDate(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, organisation, ledger, testDate, useBuyRateAsFallback: true));
				AssertEquals(expectedRateWithoutFallback, AccExchangeRateConfigurationRateFinder.GetExchangeRateForDate(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, organisation, ledger, testDate, useBuyRateAsFallback: false));
			}
		}

		[TestDate(2018, 03, 10)]
		public void TestBuyExchangeRateForDate()
		{
			TestExchangeRateForDate(false, true, InvoiceCurrencyType.NotApplicable);
		}

		[TestDate(2018, 03, 10)]
		public void TestBuyExchangeRateForDate_LocalInvoiceCurrencyType()
		{
			TestExchangeRateForDate(false, true, InvoiceCurrencyType.Local);
		}

		[TestDate(2018, 03, 10)]
		public void TestBuyExchangeRateForDate_ForeignInvoiceCurrencyType()
		{
			TestExchangeRateForDate(false, true, InvoiceCurrencyType.Foreign);
		}

		void TestExchangeRateForDate(bool isTestingTodaysExchangeRate, bool isTestingBuyExchangeRateForDate, InvoiceCurrencyType invoiceCurrencyType)
		{
			var organisation = ObjectCreator.AALSHI;
			var companyData = organisation.CompanyData;
			var creditorGroup = ObjectCreator.CreateCreditorGroup();
			companyData.OB_OG_APCreditorGroup = creditorGroup.PK;
			var debtorGroup = ObjectCreator.CreateDebtorGroup();
			companyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;

			var shipment = ObjectCreator.CreateShipment("S000029");
			var job = ObjectCreator.CreateJob(shipment);
			Factory.Save();

			var exRateForAllCurrencies = Factory.NewWithValidTestData<RefExchangeRate>();
			exRateForAllCurrencies.RE_GC = job.Company.PK;
			exRateForAllCurrencies.RE_RX_NKExCurrency = ObjectCreator.USD.RX_Code;
			exRateForAllCurrencies.RE_ExRateType = ExchangeRateTypes.Code.C99Rate;
			exRateForAllCurrencies.RE_StartDate = new ZDateTime(ZDateTime.Today.Year, 1, 1);
			exRateForAllCurrencies.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.Year, 12, 31);
			exRateForAllCurrencies.RE_SellRate = 10000m;
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.SetTemporaryValue(job.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetUpExchangeRates(out decimal todaysBuyRate, out decimal todaysSellRate, out decimal todaysCustomsRate, out decimal todaysIATARate, out decimal todaysC01Rate, out decimal todaysC02Rate, out decimal todaysC03Rate, out decimal todaysC04Rate);
				AssertRateForDate(todaysBuyRate, todaysBuyRate, todaysBuyRate, todaysBuyRate, todaysBuyRate, todaysBuyRate);

				CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.SellRate, invoiceCurrencyType: invoiceCurrencyType.ToCode(), currencyCode: ObjectCreator.USD.RX_Code);
				CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C99Rate, invoiceCurrencyType: invoiceCurrencyType.ToCode(), currencyCode: string.Empty);
				CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsPayable, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.CustomsRate, invoiceCurrencyType: invoiceCurrencyType.ToCode(), currencyCode: ObjectCreator.USD.RX_Code);
				CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsPayable, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C99Rate, invoiceCurrencyType: invoiceCurrencyType.ToCode(), currencyCode: string.Empty);
				CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.IATARate, invoiceCurrencyType: invoiceCurrencyType.ToCode(), currencyCode: ObjectCreator.USD.RX_Code);
				CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C99Rate, invoiceCurrencyType: invoiceCurrencyType.ToCode(), currencyCode: string.Empty);

				ReloadJobAndOrganisationForTest();
				AssertRateForDate(todaysIATARate, todaysSellRate, todaysCustomsRate, todaysIATARate, todaysSellRate, todaysCustomsRate);

				CreateAccExchangeRateConfiguration(debtorGroup, JobInvoicingConsumerTypes.Shipment.Code, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C01Rate, invoiceCurrencyType: invoiceCurrencyType.ToCode(), currencyCode: ObjectCreator.USD.RX_Code);
				CreateAccExchangeRateConfiguration(debtorGroup, JobInvoicingConsumerTypes.Shipment.Code, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C99Rate, invoiceCurrencyType: invoiceCurrencyType.ToCode(), currencyCode: string.Empty);
				CreateAccExchangeRateConfiguration(creditorGroup, JobInvoicingConsumerTypes.Shipment.Code, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C02Rate, invoiceCurrencyType: invoiceCurrencyType.ToCode(), currencyCode: ObjectCreator.USD.RX_Code);
				CreateAccExchangeRateConfiguration(creditorGroup, JobInvoicingConsumerTypes.Shipment.Code, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C99Rate, invoiceCurrencyType: invoiceCurrencyType.ToCode(), currencyCode: string.Empty);
				Factory.Save();

				ReloadJobAndOrganisationForTest();
				AssertRateForDate(todaysIATARate, todaysSellRate, todaysCustomsRate, todaysIATARate, todaysC01Rate, todaysC02Rate);

				CreateAccExchangeRateConfiguration(companyData, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C03Rate, invoiceCurrencyType: invoiceCurrencyType.ToCode(), currencyCode: ObjectCreator.USD.RX_Code);
				CreateAccExchangeRateConfiguration(companyData, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C99Rate, invoiceCurrencyType: invoiceCurrencyType.ToCode(), currencyCode: string.Empty);
				CreateAccExchangeRateConfiguration(companyData, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsPayable, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C04Rate, invoiceCurrencyType: invoiceCurrencyType.ToCode(), currencyCode: ObjectCreator.USD.RX_Code);
				CreateAccExchangeRateConfiguration(companyData, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsPayable, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C99Rate, invoiceCurrencyType: invoiceCurrencyType.ToCode(), currencyCode: string.Empty);
				Factory.Save();

				ReloadJobAndOrganisationForTest();
				AssertRateForDate(todaysIATARate, todaysSellRate, todaysCustomsRate, todaysIATARate, todaysC03Rate, todaysC04Rate);

				void AssertRateForDate(ZDecimal expectedRateForCompanyLevelEmptyLedger, ZDecimal expectedRateForCompanyLevelARLedger, ZDecimal expectedRateForCompanyLevelAPLedger, ZDecimal expectedRateForOrgLevelEmptyLedger, ZDecimal expectedRateForOrgLevelARLedger, ZDecimal expectedRateForOrgLevelAPLedger)
				{
					if (isTestingBuyExchangeRateForDate)
					{
						ExchangeRateReader.GetReaderInstance().ClearCache();
						AssertEquals(GetExpectExchangeRateWithOffset(todaysBuyRate, offsetFlag: 0),
							AccExchangeRateConfigurationRateFinder.GetBuyExchangeRateForDate(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, ZDateTime.Today)
						);
						AssertEquals(GetExpectExchangeRateWithOffset(todaysBuyRate, offsetFlag: -1),
							AccExchangeRateConfigurationRateFinder.GetBuyExchangeRateForDate(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, ZDateTime.Today.AddDays(-1))
						);
						AssertEquals(GetExpectExchangeRateWithOffset(todaysBuyRate, offsetFlag: 1),
							AccExchangeRateConfigurationRateFinder.GetBuyExchangeRateForDate(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, ZDateTime.Today.AddDays(1))
						);
					}
					else if (isTestingTodaysExchangeRate)
					{
						ExchangeRateReader.GetReaderInstance().ClearCache();
						AssertEquals(expectedRateForCompanyLevelEmptyLedger, AccExchangeRateConfigurationRateFinder.GetTodaysExchangeRate(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, null, ExchangeRateValidLedgerEnum.None, invoiceCurrencyType));
						AssertEquals(expectedRateForCompanyLevelARLedger, AccExchangeRateConfigurationRateFinder.GetTodaysExchangeRate(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, null, ExchangeRateValidLedgerEnum.AR, invoiceCurrencyType));
						AssertEquals(expectedRateForCompanyLevelAPLedger, AccExchangeRateConfigurationRateFinder.GetTodaysExchangeRate(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, null, ExchangeRateValidLedgerEnum.AP, invoiceCurrencyType));
						AssertEquals(expectedRateForOrgLevelEmptyLedger, AccExchangeRateConfigurationRateFinder.GetTodaysExchangeRate(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, organisation, ExchangeRateValidLedgerEnum.None, invoiceCurrencyType));
						AssertEquals(expectedRateForOrgLevelARLedger, AccExchangeRateConfigurationRateFinder.GetTodaysExchangeRate(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, organisation, ExchangeRateValidLedgerEnum.AR, invoiceCurrencyType));
						AssertEquals(expectedRateForOrgLevelAPLedger, AccExchangeRateConfigurationRateFinder.GetTodaysExchangeRate(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, organisation, ExchangeRateValidLedgerEnum.AP, invoiceCurrencyType));
					}
					else
					{
						AssertRateForDateCore(expectedRateForCompanyLevelEmptyLedger, null, ExchangeRateValidLedgerEnum.None);
						AssertRateForDateCore(expectedRateForCompanyLevelARLedger, null, ExchangeRateValidLedgerEnum.AR);
						AssertRateForDateCore(expectedRateForCompanyLevelAPLedger, null, ExchangeRateValidLedgerEnum.AP);
						AssertRateForDateCore(expectedRateForOrgLevelEmptyLedger, organisation, ExchangeRateValidLedgerEnum.None);
						AssertRateForDateCore(expectedRateForOrgLevelARLedger, organisation, ExchangeRateValidLedgerEnum.AR);
						AssertRateForDateCore(expectedRateForOrgLevelAPLedger, organisation, ExchangeRateValidLedgerEnum.AP);
					}

					void AssertRateForDateCore(ZDecimal expectedRateForToday, OrgHeader org, ExchangeRateValidLedgerEnum ledger)
					{
						ExchangeRateReader.GetReaderInstance().ClearCache();
						AssertEquals(expectedRateForToday, AccExchangeRateConfigurationRateFinder.GetExchangeRateForDate(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, org, ledger, ZDateTime.Today, invoiceCurrencyType));
						AssertEquals(GetExpectExchangeRateWithOffset(expectedRateForToday, offsetFlag: -1), AccExchangeRateConfigurationRateFinder.GetExchangeRateForDate(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, org, ledger,
							ZDateTime.Today.AddDays(-1), invoiceCurrencyType)
						);
						AssertEquals(GetExpectExchangeRateWithOffset(expectedRateForToday, offsetFlag: 1), AccExchangeRateConfigurationRateFinder.GetExchangeRateForDate(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, org, ledger,
							ZDateTime.Today.AddDays(1), invoiceCurrencyType)
						);
					}
				}

				void ReloadJobAndOrganisationForTest()
				{
					var reloadFactory = new BusinessObjectFactory();
					job = reloadFactory.Load<Job>(job.PK);
					job.InitializeParentFromGenericJobWithoutSettingDefaults();
					organisation = reloadFactory.Load<OrgHeader>(organisation.PK);
				}
			}
		}

		#endregion

		#region Exchange Rate Based On Invoice Posting Option Test Cases

		[TestDate(2018, 03, 10)]
		public void TestGetExchangeRateBasedOnInvoicePostingOption_ExchangeRateBasedOnInvoiceDate()
		{
			TestGetExchangeRateBasedOnInvoicePostingOption(AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code, false);
		}

		[TestDate(2018, 03, 10)]
		public void TestGetExchangeRateBasedOnInvoicePostingOption_ExchangeRateBasedOnPostDate()
		{
			TestGetExchangeRateBasedOnInvoicePostingOption(AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code, false);
		}

		[TestDate(2018, 03, 10)]
		public void TestGetExchangeRateBasedOnInvoicePostingOption_TodayExchangeRate()
		{
			TestGetExchangeRateBasedOnInvoicePostingOption(AccountingConstants.InvoicePostingExchangeRateOption.TodayExchangeRate.Code, false);
		}

		[TestDate(2018, 03, 10)]
		public void TestGetBuyExchangeRateBasedOnInvoicePostingOption_ExchangeRateBasedOnInvoiceDate()
		{
			TestGetExchangeRateBasedOnInvoicePostingOption(AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code, true);
		}

		[TestDate(2018, 03, 10)]
		public void TestGetBuyExchangeRateBasedOnInvoicePostingOption_ExchangeRateBasedOnPostDate()
		{
			TestGetExchangeRateBasedOnInvoicePostingOption(AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code, true);
		}

		[TestDate(2018, 03, 10)]
		public void TestGetBuyExchangeRateBasedOnInvoicePostingOption_TodayExchangeRate()
		{
			TestGetExchangeRateBasedOnInvoicePostingOption(AccountingConstants.InvoicePostingExchangeRateOption.TodayExchangeRate.Code, true);
		}

		void TestGetExchangeRateBasedOnInvoicePostingOption(string postingOption, bool isTestingBuyExchangeRate)
		{
			var company = GlbCompany.CurrentCompany;
			var organisation = ObjectCreator.AALSHI;
			var companyData = organisation.CompanyData;
			var creditorGroup = ObjectCreator.CreateCreditorGroup();
			companyData.OB_OG_APCreditorGroup = creditorGroup.PK;
			var debtorGroup = ObjectCreator.CreateDebtorGroup();
			companyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;

			var shipment = ObjectCreator.CreateShipment("S000029");
			var job = ObjectCreator.CreateJob(shipment);
			Factory.Save();

			var exRateForAllCurrencies = Factory.NewWithValidTestData<RefExchangeRate>();
			exRateForAllCurrencies.RE_GC = job.Company.PK;
			exRateForAllCurrencies.RE_RX_NKExCurrency = ObjectCreator.USD.RX_Code;
			exRateForAllCurrencies.RE_ExRateType = ExchangeRateTypes.Code.C99Rate;
			exRateForAllCurrencies.RE_StartDate = new ZDateTime(ZDateTime.Today.Year, 1, 1);
			exRateForAllCurrencies.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.Year, 12, 31);
			exRateForAllCurrencies.RE_SellRate = 10000m;
			Factory.Save();

			SetUpExchangeRates(out decimal todaysBuyRate, out decimal todaysSellRate, out decimal todaysCustomsRate, out decimal todaysIATARate, out decimal todaysC01Rate, out decimal todaysC02Rate, out decimal todaysC03Rate, out decimal todaysC04Rate);

			const int invoiceDateOffset = -1;
			const int postDateOffset = 1;
			var offsetForExpectedRate = postingOption == AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code
				? invoiceDateOffset
				: postingOption == AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code
					? postDateOffset
					: 0;

			var invoiceDate = ZDateTime.Today.AddDays(invoiceDateOffset);
			var postDate = ZDateTime.Today.AddDays(postDateOffset);
			var taxDate = ZDateTime.Today.AddDays(2);

			using (AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, postingOption))
			using (AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, postingOption))
			{
				ExchangeRateReader.GetReaderInstance().ClearCache();

				AssertEquals("PreCondition", ExchangeRateTypes.Code.BuyRate, SystemDefaultExchangeRateConfig.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType);
				if (isTestingBuyExchangeRate)
				{
					AssertEquals(GetExpectedRate(todaysBuyRate), AccExchangeRateConfigurationRateFinder.GetBuyExchangeRateBasedOnInvoicePostingOption(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, false, company.PK, ExchangeRateValidLedgerEnum.AP, invoiceDate, postDate, taxDate));
					AssertEquals(GetExpectedRate(todaysBuyRate), AccExchangeRateConfigurationRateFinder.GetBuyExchangeRateBasedOnInvoicePostingOption(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, false, company.PK, ExchangeRateValidLedgerEnum.AR, invoiceDate, postDate, taxDate));
				}
				else
				{
					AssertRateBasedOnInvoicePostingOption(GetExpectedRate(todaysBuyRate), GetExpectedRate(todaysBuyRate), GetExpectedRate(todaysBuyRate), GetExpectedRate(todaysBuyRate), GetExpectedRate(todaysBuyRate), GetExpectedRate(todaysBuyRate));
				}

				CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.SellRate, currencyCode: ObjectCreator.USD.RX_Code);
				CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C99Rate, currencyCode: string.Empty);
				CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsPayable, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.CustomsRate, currencyCode: ObjectCreator.USD.RX_Code);
				CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsPayable, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C99Rate, currencyCode: string.Empty);
				CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.IATARate, currencyCode: ObjectCreator.USD.RX_Code);
				CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C99Rate, currencyCode: string.Empty);

				ReloadJobAndOrganisationForTest();
				ExchangeRateReader.GetReaderInstance().ClearCache();
				if (isTestingBuyExchangeRate)
				{
					AssertEquals(GetExpectedRate(todaysBuyRate), AccExchangeRateConfigurationRateFinder.GetBuyExchangeRateBasedOnInvoicePostingOption(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, false, company.PK, ExchangeRateValidLedgerEnum.AP, invoiceDate, postDate, taxDate));
					AssertEquals(GetExpectedRate(todaysBuyRate), AccExchangeRateConfigurationRateFinder.GetBuyExchangeRateBasedOnInvoicePostingOption(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, false, company.PK, ExchangeRateValidLedgerEnum.AR, invoiceDate, postDate, taxDate));
				}
				else
				{
					AssertRateBasedOnInvoicePostingOption(
						GetExpectedRate(todaysIATARate),
						GetExpectedRate(todaysSellRate),
						GetExpectedRate(todaysCustomsRate),
						GetExpectedRate(todaysIATARate),
						GetExpectedRate(todaysSellRate),
						GetExpectedRate(todaysCustomsRate)
					);
				}

				CreateAccExchangeRateConfiguration(debtorGroup, JobInvoicingConsumerTypes.Shipment.Code, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C01Rate, currencyCode: ObjectCreator.USD.RX_Code);
				CreateAccExchangeRateConfiguration(debtorGroup, JobInvoicingConsumerTypes.Shipment.Code, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C99Rate, currencyCode: string.Empty);
				CreateAccExchangeRateConfiguration(creditorGroup, JobInvoicingConsumerTypes.Shipment.Code, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C02Rate, currencyCode: ObjectCreator.USD.RX_Code);
				CreateAccExchangeRateConfiguration(creditorGroup, JobInvoicingConsumerTypes.Shipment.Code, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C99Rate, currencyCode: string.Empty);
				Factory.Save();

				ReloadJobAndOrganisationForTest();
				ExchangeRateReader.GetReaderInstance().ClearCache();
				if (isTestingBuyExchangeRate)
				{
					AssertEquals(GetExpectedRate(todaysBuyRate), AccExchangeRateConfigurationRateFinder.GetBuyExchangeRateBasedOnInvoicePostingOption(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, false, company.PK, ExchangeRateValidLedgerEnum.AP, invoiceDate, postDate, taxDate));
					AssertEquals(GetExpectedRate(todaysBuyRate), AccExchangeRateConfigurationRateFinder.GetBuyExchangeRateBasedOnInvoicePostingOption(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, false, company.PK, ExchangeRateValidLedgerEnum.AR, invoiceDate, postDate, taxDate));
				}
				else
				{
					AssertRateBasedOnInvoicePostingOption(
						GetExpectedRate(todaysIATARate),
						GetExpectedRate(todaysSellRate),
						GetExpectedRate(todaysCustomsRate),
						GetExpectedRate(todaysIATARate),
						GetExpectedRate(todaysC01Rate),
						GetExpectedRate(todaysC02Rate)
					);
				}

				CreateAccExchangeRateConfiguration(companyData, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C03Rate, currencyCode: ObjectCreator.USD.RX_Code);
				CreateAccExchangeRateConfiguration(companyData, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsReceivable, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C99Rate, currencyCode: string.Empty);
				CreateAccExchangeRateConfiguration(companyData, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsPayable, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C04Rate, currencyCode: ObjectCreator.USD.RX_Code);
				CreateAccExchangeRateConfiguration(companyData, JobInvoicingConsumerTypes.Shipment.Code, LedgerTypes.AccountsPayable, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.C99Rate, currencyCode: string.Empty);
				Factory.Save();

				ReloadJobAndOrganisationForTest();
				ExchangeRateReader.GetReaderInstance().ClearCache();
				if (isTestingBuyExchangeRate)
				{
					AssertEquals(GetExpectedRate(todaysBuyRate), AccExchangeRateConfigurationRateFinder.GetBuyExchangeRateBasedOnInvoicePostingOption(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, false, company.PK, ExchangeRateValidLedgerEnum.AP, invoiceDate, postDate, taxDate));
					AssertEquals(GetExpectedRate(todaysBuyRate), AccExchangeRateConfigurationRateFinder.GetBuyExchangeRateBasedOnInvoicePostingOption(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, false, company.PK, ExchangeRateValidLedgerEnum.AR, invoiceDate, postDate, taxDate));
				}
				else
				{
					AssertRateBasedOnInvoicePostingOption(GetExpectedRate(todaysIATARate), GetExpectedRate(todaysSellRate), GetExpectedRate(todaysCustomsRate), GetExpectedRate(todaysIATARate), GetExpectedRate(todaysC03Rate), GetExpectedRate(todaysC04Rate));
				}
			}

			void ReloadJobAndOrganisationForTest()
			{
				var reloadFactory = new BusinessObjectFactory();
				job = reloadFactory.Load<Job>(job.PK);
				job.InitializeParentFromGenericJobWithoutSettingDefaults();
				organisation = reloadFactory.Load<OrgHeader>(organisation.PK);
			}

			void AssertRateBasedOnInvoicePostingOption(ZDecimal expectedRateForCompanyLevelEmptyLedger, ZDecimal expectedRateForCompanyLevelARLedger, ZDecimal expectedRateForCompanyLevelAPLedger, ZDecimal expectedRateForOrgLevelEmptyLedger, ZDecimal expectedRateForOrgLevelARLedger, ZDecimal expectedRateForOrgLevelAPLedger)
			{
				AssertEquals(expectedRateForCompanyLevelARLedger, AccExchangeRateConfigurationRateFinder.GetExchangeRateBasedOnInvoicePostingOption(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, false, company.PK, null, ExchangeRateValidLedgerEnum.AR, invoiceDate, postDate, taxDate));
				AssertEquals(expectedRateForCompanyLevelAPLedger, AccExchangeRateConfigurationRateFinder.GetExchangeRateBasedOnInvoicePostingOption(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, false, company.PK, null, ExchangeRateValidLedgerEnum.AP, invoiceDate, postDate, taxDate));
				AssertEquals(expectedRateForOrgLevelARLedger, AccExchangeRateConfigurationRateFinder.GetExchangeRateBasedOnInvoicePostingOption(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, false, company.PK, organisation, ExchangeRateValidLedgerEnum.AR, invoiceDate, postDate, taxDate));
				AssertEquals(expectedRateForOrgLevelAPLedger, AccExchangeRateConfigurationRateFinder.GetExchangeRateBasedOnInvoicePostingOption(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, false, company.PK, organisation, ExchangeRateValidLedgerEnum.AP, invoiceDate, postDate, taxDate));
			}

			ZDecimal GetExpectedRate(decimal baseRate) => GetExpectExchangeRateWithOffset(baseRate, offsetForExpectedRate);
		}

		[TestDate(2018, 03, 10)]
		public void TestGetExchangeRateBasedOnInvoicePostingOption_TodayExchangeRate_UseBuyRateAsFallBack()
		{
			TestGetExchangeRateBasedOnInvoicePostingOption_UseBuyRateAsFallBack(AccountingConstants.InvoicePostingExchangeRateOption.TodayExchangeRate.Code);
		}

		[TestDate(2018, 03, 10)]
		public void TestGetBuyExchangeRateBasedOnInvoicePostingOption_ExchangeRateBasedOnInvoiceDate_UseBuyRateAsFallBack()
		{
			TestGetExchangeRateBasedOnInvoicePostingOption_UseBuyRateAsFallBack(AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);
		}

		[TestDate(2018, 03, 10)]
		public void TestGetBuyExchangeRateBasedOnInvoicePostingOption_ExchangeRateBasedOnPostDate_UseBuyRateAsFallBack()
		{
			TestGetExchangeRateBasedOnInvoicePostingOption_UseBuyRateAsFallBack(AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code);
		}

		void TestGetExchangeRateBasedOnInvoicePostingOption_UseBuyRateAsFallBack(string postingOption)
		{
			var todaysBuyRate = 0.123m;
			CreateExchangeRate(Core.Constants.ExchangeRateTypes.Code.BuyRate, todaysBuyRate, ZGuid.Empty);
			var shipment = ObjectCreator.CreateShipment("S000029");
			var job = ObjectCreator.CreateJob(shipment);
			Factory.Save();
			TestGetExchangeRateBasedOnInvoicePostingOption_UseBuyRateAsFallBack(postingOption, job, todaysBuyRate);
		}

		void TestGetExchangeRateBasedOnInvoicePostingOption_UseBuyRateAsFallBack(string postingOption, Job job, decimal todaysBuyRate)
		{
			var invoiceDate = ZDateTime.Today.AddDays(-1);
			var postDate = ZDateTime.Today.AddDays(1);
			var taxDate = ZDateTime.Today.AddDays(2);
			Factory.Save();

			job.JH_GC = ZGuid.Empty;//rate consumer company is null, config will be null

			using (PostingExRateRegistryAR.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, postingOption))
			using (PostingExRateRegistryAP.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, postingOption))
			{
				AssertRateWithUseBuyRateAsFallBack(null, ExchangeRateValidLedgerEnum.AR);
				AssertRateWithUseBuyRateAsFallBack(null, ExchangeRateValidLedgerEnum.AP);
			}

			void AssertRateWithUseBuyRateAsFallBack(OrgHeader organisation, ExchangeRateValidLedgerEnum ledger)
			{
				var company = GlbCompany.CurrentCompany;
				var expectedRateWithFallback = GetExpectedRate();
				ExchangeRateReader.GetReaderInstance().ClearCache();
				AssertEquals(expectedRateWithFallback, AccExchangeRateConfigurationRateFinder.GetExchangeRateBasedOnInvoicePostingOption(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, false, company.PK, organisation, ledger, invoiceDate, postDate, taxDate, true));
				AssertEquals(0m, AccExchangeRateConfigurationRateFinder.GetExchangeRateBasedOnInvoicePostingOption(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, false, company.PK, organisation, ledger, invoiceDate, postDate, taxDate, false));

				ZDecimal GetExpectedRate() => postingOption == AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code ? todaysBuyRate + 1 : postingOption == AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code ? todaysBuyRate + 2 : todaysBuyRate;
			}
		}

		public void TestGetExchangeRateDate_WhenInvoicePostingExchangeRateRegistryOptionIsNonDEF_ThenDefaultExchangeRateAccordingToInvoicePostingExchangeRateOption(Job job, string exchangeRateConfigJobType, string exchangeRateConfigPreference)
		{
			var todaysBuyRate = 0.123m;

			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(exchangeRateConfigJobType, "ALL", "ALL", preference: exchangeRateConfigPreference);
			GlbCompany.CurrentCompany.Factory.Save();

			CreateExchangeRate(Core.Constants.ExchangeRateTypes.Code.BuyRate, todaysBuyRate, ZGuid.Empty);

			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();
			AssertEquals(todaysBuyRate, AccExchangeRateConfigurationRateFinder.GetExchangeRate(job.ExchangeRateConfigurationRateConsumer, ObjectCreator.USD, null, ExchangeRateValidLedgerEnum.AR));

			TestGetExchangeRateBasedOnInvoicePostingOption_UseBuyRateAsFallBack(AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code, job, todaysBuyRate);
		}

		public void TestGetExchangeRateDate_WhenJobIsShipment_AndWhenJobBillingExchangeRatePreferenceIsPickupDate_AndWhenInvoicePostingExchangeRateRegistryOptionIsNonDEF_ThenDefaultExchangeRateAccordingToInvoicePostingExchangeRateOption()
		{
			var consol = ObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			var shipment = ObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			consol.Shipments.Add(shipment);

			consol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today;
			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Today;
			var job = new Job.Loader(shipment).TryLoadOrCreate();

			TestGetExchangeRateDate_WhenInvoicePostingExchangeRateRegistryOptionIsNonDEF_ThenDefaultExchangeRateAccordingToInvoicePostingExchangeRateOption(job, JobInvoicingConsumerTypes.Shipment.Code, JobBillingExchangeRatePreference.Code.ShipmentOrBrokeragePickupDate);
		}

		public void TestGetExchangeRateDate_WhenJobIsShipment_AndWhenJobBillingExchangeRatePreferenceIsDeliveryDate_AndWhenInvoicePostingExchangeRateRegistryOptionIsNonDEF_ThenDefaultExchangeRateAccordingToInvoicePostingExchangeRateOption()
		{
			var consol = ObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			var shipment = ObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			consol.Shipments.Add(shipment);

			consol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today;
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Today;

			var job = new Job.Loader(shipment).TryLoadOrCreate();

			TestGetExchangeRateDate_WhenInvoicePostingExchangeRateRegistryOptionIsNonDEF_ThenDefaultExchangeRateAccordingToInvoicePostingExchangeRateOption(job, JobInvoicingConsumerTypes.Shipment.Code, JobBillingExchangeRatePreference.Code.ShipmentOrBrokerageDeliveryDate);
		}

		public void TestGetExchangeRateDate_WhenJobIsBrokerage_AndWhenJobBillingExchangeRatePreferenceIsPickupDate_AndWhenInvoicePostingExchangeRateRegistryOptionIsNonDEF_ThenDefaultExchangeRateAccordingToInvoicePostingExchangeRateOption()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Today;
			var job = ObjectCreator.CreateJob(declaration, ObjectCreator.LocalClient, 5M, ObjectCreator.Agent, 10M);
			job.Parent = declaration;

			TestGetExchangeRateDate_WhenInvoicePostingExchangeRateRegistryOptionIsNonDEF_ThenDefaultExchangeRateAccordingToInvoicePostingExchangeRateOption(job, JobInvoicingConsumerTypes.Brokerage.Code, JobBillingExchangeRatePreference.Code.ShipmentOrBrokeragePickupDate);
		}

		public void TestGetExchangeRateDate_WhenJobIsBrokerage_AndWhenJobBillingExchangeRatePreferenceIsDeliveryDate_AndWhenInvoicePostingExchangeRateRegistryOptionIsNonDEF_ThenDefaultExchangeRateAccordingToInvoicePostingExchangeRateOption()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Today;
			var job = ObjectCreator.CreateJob(declaration, ObjectCreator.LocalClient, 5M, ObjectCreator.Agent, 10M);
			job.Parent = declaration;

			TestGetExchangeRateDate_WhenInvoicePostingExchangeRateRegistryOptionIsNonDEF_ThenDefaultExchangeRateAccordingToInvoicePostingExchangeRateOption(job, JobInvoicingConsumerTypes.Brokerage.Code, JobBillingExchangeRatePreference.Code.ShipmentOrBrokerageDeliveryDate);
		}

		[ExpectNoExceptions]
		public void TestNoExceptionThrownWhenJobBillingExchangeRatePreferenceIsPickupOrDeliveryDateAndJobParentIsNotIJobInvoicingPlugIn()
		{
			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.SpotQuote, Factory);
			var quote = quotedBooking.Quote as IQuote;
			quote.TH_QuoteNumber = "0001";
			Factory.Save();

			using (var job = new Job.Loader((IJobHeaderParent)quote).TryCreateWithMutex())
			{
				Assert(!(quote is IJobInvoicingPlugIn));
				AssertEquals(ZDateTime.Invalid, job.ExchangeRateConfigurationRateConsumer.ShipmentOrBrokeragePickupDate);
				AssertEquals(ZDateTime.Invalid, job.ExchangeRateConfigurationRateConsumer.ShipmentOrBrokerageDeliveryDate);
			}
		}

		#endregion

		#region Is Exchange Rate Type Not Equal To Specified

		[TestDate(2018, 03, 10)]
		public void TestIsExchangeRateTypeNotEqualToSpecified()
		{
			ObjectCreator.USD.ExchangeRates.DeleteAll();
			var shipment = ObjectCreator.CreateShipment("S000029");
			var job = ObjectCreator.CreateJob(shipment);
			Factory.Save();

			CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, TransportModes.All,
				ExchangeRateTypes.Code.SellRate, JobBillingExchangeRatePreference.Code.TodaysRate, currencyCode: ObjectCreator.USD.Code);
			CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, TransportModes.All,
				ExchangeRateTypes.Code.C99Rate, JobBillingExchangeRatePreference.Code.TodaysRate, currencyCode: string.Empty);
			CombineAssertions("PreCondition, USD would get rate type from it specific config(SellRate), but EUR would get rate type from config for all currencies(C99Rate).",
				() => {
					AssertSpecifiedRateType(job, ObjectCreator.USD, false, ExchangeRateTypes.Code.SellRate);
					AssertSpecifiedRateType(job, ObjectCreator.EUR, false, ExchangeRateTypes.Code.C99Rate);
				}
			);

			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.453m, ZDateTime.Today, ZDateTime.Today);
			AssertSpecifiedRateType(job, ObjectCreator.USD, false, ExchangeRateTypes.Code.SellRate);

			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, ExchangeRateTypes.Code.SellRate, 0.922m, ZDateTime.Today, ZDateTime.Today);
			AssertSpecifiedRateType(job, ObjectCreator.USD, false, ExchangeRateTypes.Code.SellRate);

			job.LocalChargesPK = ObjectCreator.LocalClient.PK;
			AssertSpecifiedRateType(job, ObjectCreator.USD, false, ExchangeRateTypes.Code.SellRate);

			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.231m, ZDateTime.Today, ZDateTime.Today, ObjectCreator.LocalClient.PK);
			AssertSpecifiedRateType(job, ObjectCreator.USD, true, ExchangeRateTypes.Code.SellRate);

			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, ExchangeRateTypes.Code.SellRate, 0.873m, ZDateTime.Today, ZDateTime.Today, ObjectCreator.LocalClient.PK);
			AssertSpecifiedRateType(job, ObjectCreator.USD, false, ExchangeRateTypes.Code.SellRate);

			ObjectCreator.CreateExchangeRate(ObjectCreator.EUR, ExchangeRateTypes.Code.C99Rate, 0.6547m, ZDateTime.Today, ZDateTime.Today);
			ObjectCreator.CreateExchangeRate(ObjectCreator.EUR, ExchangeRateTypes.Code.BuyRate, 0.2m, ZDateTime.Today, ZDateTime.Today, ObjectCreator.LocalClient.PK);
			AssertSpecifiedRateType(job, ObjectCreator.EUR, true, ExchangeRateTypes.Code.C99Rate);
		}

		[TestDate(2018, 03, 10)]
		public void TestIsExchangeRateTypeNotEqualToSpecified_WithFallBackToPreviousExchangeRateRegistry()
		{
			ObjectCreator.USD.ExchangeRates.DeleteAll();
			var shipment = ObjectCreator.CreateShipment("S000029");
			var job = ObjectCreator.CreateJob(shipment);
			Factory.Save();
			CreateAccExchangeRateConfiguration(GlbCompany.CurrentCompany, JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, Core.Constants.TransportModes.All, Core.Constants.TransportModes.All, Core.Constants.ExchangeRateTypes.Code.SellRate, Core.Constants.JobBillingExchangeRatePreference.Code.TodaysRate, 0, false);
			GlbCompany.CurrentCompany.Factory.Save();

			AssertSpecifiedRateTypeWithFallBackToPreviousExchangeRateRegistry(true, false, Core.Constants.ExchangeRateTypes.Code.SellRate);
			AssertSpecifiedRateTypeWithFallBackToPreviousExchangeRateRegistry(false, false, Core.Constants.ExchangeRateTypes.Code.SellRate);

			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, Core.Constants.ExchangeRateTypes.Code.BuyRate, 0.893m, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-1));

			AssertSpecifiedRateTypeWithFallBackToPreviousExchangeRateRegistry(true, false, Core.Constants.ExchangeRateTypes.Code.SellRate);
			AssertSpecifiedRateTypeWithFallBackToPreviousExchangeRateRegistry(false, false, Core.Constants.ExchangeRateTypes.Code.SellRate);

			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, Core.Constants.ExchangeRateTypes.Code.SellRate, 0.789m, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-1));

			AssertSpecifiedRateTypeWithFallBackToPreviousExchangeRateRegistry(true, false, Core.Constants.ExchangeRateTypes.Code.SellRate);
			AssertSpecifiedRateTypeWithFallBackToPreviousExchangeRateRegistry(false, false, Core.Constants.ExchangeRateTypes.Code.SellRate);

			job.LocalChargesPK = ObjectCreator.LocalClient.PK;

			AssertSpecifiedRateTypeWithFallBackToPreviousExchangeRateRegistry(true, false, Core.Constants.ExchangeRateTypes.Code.SellRate);
			AssertSpecifiedRateTypeWithFallBackToPreviousExchangeRateRegistry(false, false, Core.Constants.ExchangeRateTypes.Code.SellRate);

			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, Core.Constants.ExchangeRateTypes.Code.BuyRate, 0.412m, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-1), ObjectCreator.LocalClient.PK);

			AssertSpecifiedRateTypeWithFallBackToPreviousExchangeRateRegistry(true, true, Core.Constants.ExchangeRateTypes.Code.SellRate);
			AssertSpecifiedRateTypeWithFallBackToPreviousExchangeRateRegistry(false, false, Core.Constants.ExchangeRateTypes.Code.SellRate);

			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, Core.Constants.ExchangeRateTypes.Code.SellRate, 0.524m, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-1), ObjectCreator.LocalClient.PK);

			AssertSpecifiedRateTypeWithFallBackToPreviousExchangeRateRegistry(true, false, Core.Constants.ExchangeRateTypes.Code.SellRate);
			AssertSpecifiedRateTypeWithFallBackToPreviousExchangeRateRegistry(false, false, Core.Constants.ExchangeRateTypes.Code.SellRate);

			void AssertSpecifiedRateTypeWithFallBackToPreviousExchangeRateRegistry(bool shouldUseFallback, bool isExchangeRateTypeNotEqualToSpecified, ZString expectedRateType)
			{
				using (AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, shouldUseFallback))
				{
					AssertSpecifiedRateType(job, ObjectCreator.USD, isExchangeRateTypeNotEqualToSpecified, expectedRateType);
				}
			}
		}

		void AssertSpecifiedRateType(Job job, RefCurrency currency, bool isExchangeRateTypeNotEqualToSpecified, ZString expectedRateType)
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			var rateTypeInConfig = ZString.Empty;
			AssertEquals(isExchangeRateTypeNotEqualToSpecified, AccExchangeRateConfigurationRateFinder.IsExchangeRateTypeNotEqualToSpecified(job.ExchangeRateConfigurationRateConsumer, currency, ref rateTypeInConfig));
			AssertEquals(expectedRateType, rateTypeInConfig);
		}

		#endregion

		public void TestGetExchangeRateConfiguration()
		{
			var testingCur = ObjectCreator.USD.RX_Code;
			var company = GlbCompany.CurrentCompany;
			var organisationWithoutGroup = ObjectCreator.ABIGAS;
			var organisationWithGroup = ObjectCreator.AALSHI;
			var companyData = organisationWithGroup.CompanyData;
			var creditorGroup = ObjectCreator.CreateCreditorGroup();
			companyData.OB_OG_APCreditorGroup = creditorGroup.PK;
			var debtorGroup = ObjectCreator.CreateDebtorGroup();
			companyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;

			var shipment = ObjectCreator.CreateShipment("S000029");
			var job = ObjectCreator.CreateJob(shipment);
			Factory.Save();

			CreateConfigSet(LedgerTypes.AccountsReceivable, out var companyConfigAR, out var orgConfigAR, out var debtorGroupConfig);
			CreateConfigSet(LedgerTypes.AccountsPayable, out var companyConfigAP, out var orgConfigAP, out var creditorGroupConfig);

			AssertGetExchangeRateConfiguration(job, "[AR]When no orgHeader as filter condition, we should only get result from company's setting.",
				null, ExchangeRateValidLedgerEnum.AR,
				companyConfigAR.PK,
				testingCur: testingCur
			);
			AssertGetExchangeRateConfiguration(job, "[AP]When no orgHeader as filter condition, we should only get result from company's setting.",
				null, ExchangeRateValidLedgerEnum.AP,
				companyConfigAP.PK,
				testingCur: testingCur
			);
			AssertGetExchangeRateConfiguration(job, "[AR]When putting orgHeader as filter condition, we would get result from AccARExchangeRateConfigurations.",
				organisationWithoutGroup, ExchangeRateValidLedgerEnum.AR,
				orgConfigAR.PK,
				testingCur: testingCur
			);
			AssertGetExchangeRateConfiguration(job, "[AP]When putting orgHeader as filter condition, we would get result from AccAPExchangeRateConfigurations.",
				organisationWithoutGroup, ExchangeRateValidLedgerEnum.AP,
				orgConfigAP.PK,
				testingCur: testingCur
			);
			AssertGetExchangeRateConfiguration(job, "[DebtorGroup]When putting orgHeader as filter condition, we would get result from AccARExchangeRateConfigurations.",
				organisationWithGroup, ExchangeRateValidLedgerEnum.AR,
				debtorGroupConfig.PK,
				testingCur: testingCur
			);
			AssertGetExchangeRateConfiguration(job, "[CreditorGroup]When putting orgHeader as filter condition, we would get result from AccAPExchangeRateConfigurations.",
				organisationWithGroup, ExchangeRateValidLedgerEnum.AP,
				creditorGroupConfig.PK,
				testingCur: testingCur
			);

			void CreateConfigSet(string ledger, out AccExchangeRateConfiguration companyConfig, out AccExchangeRateConfiguration orgConfig, out AccExchangeRateConfiguration orgGroupConfig)
			{
				companyConfig = CreateAccExchangeRateConfiguration(company, JobInvoicingConsumerTypes.Shipment.Code, ledger, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.BuyRate, currencyCode: testingCur
				);

				orgConfig = CreateAccExchangeRateConfiguration(organisationWithoutGroup.CompanyData, JobInvoicingConsumerTypes.Shipment.Code, ledger, TransportModes.All, FreightShipmentDirection.Code.All,
					ExchangeRateTypes.Code.BuyRate, currencyCode: testingCur
				);

				orgGroupConfig = ledger == LedgerTypes.AccountsReceivable
					? CreateAccExchangeRateConfiguration(debtorGroup, JobInvoicingConsumerTypes.Shipment.Code, TransportModes.All, FreightShipmentDirection.Code.All,
						ExchangeRateTypes.Code.BuyRate, currencyCode: testingCur)
					: CreateAccExchangeRateConfiguration(creditorGroup, JobInvoicingConsumerTypes.Shipment.Code, TransportModes.All, FreightShipmentDirection.Code.All,
						ExchangeRateTypes.Code.BuyRate, currencyCode: testingCur);
			}
		}

		void AssertGetExchangeRateConfiguration(Job job, string comment, OrgHeader orgHeader, ExchangeRateValidLedgerEnum ledger, ZGuid expectedConfigPK, InvoiceCurrencyType invoiceCurrencyType = InvoiceCurrencyType.NotApplicable, string testingCur = "")
		{
			AssertEquals(
				comment,
				expectedConfigPK,
				AccExchangeRateConfigurationRateFinder.GetExchangeRateConfiguration(job.ExchangeRateConfigurationRateConsumer, orgHeader, ledger, invoiceCurrencyType, testingCur).PK
			);
		}

		public void TestGetExchangeRateConfiguration_WithinCurrencyConfigDateRange()
		{
			var shipment = ObjectCreator.CreateShipment("S000029");
			var shipmentJob = ObjectCreator.CreateJob(shipment);
			Factory.Save();

			var rateConsumer = shipmentJob.ExchangeRateConfigurationRateConsumer;
			var exRateConfig = CreateAccExchangeRateConfiguration(JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.All, ExchangeRateTypes.Code.SellRate, preference: "TST", currencyCode: "USD");

			var currencyConfig = exRateConfig.GetCurrencyConfig("USD", ZDate.Empty);
			currencyConfig.JCT_StartDate = new ZDate(2025, 2, 3);
			currencyConfig.JCT_ExpiryDate = new ZDate(2025, 2, 5);

			var getExchangeRateDateMock = MockIAccExchangeRateConfigurationsHelper(rateConsumer, "TST", new ZDateTime(2025, 2, 4));

			using (ObjectFactory.Substitute(getExchangeRateDateMock.Object))
			{
				var actualExRateConfig = AccExchangeRateConfigurationRateFinder.GetExchangeRateConfiguration(rateConsumer, null, ExchangeRateValidLedgerEnum.AR, InvoiceCurrencyType.NotApplicable, "USD");
				AssertEquals("Should find the setup currency config since valid date range", exRateConfig.PK, actualExRateConfig.PK);
			}

			getExchangeRateDateMock.Verify(x => x.GetExchangeRateDate(It.IsAny<IAccExchangeRateConfigurationRateConsumer>(), It.IsAny<ZString>()), Times.Exactly(3));
		}

		public void TestGetExchangeRateConfiguration_OutsideCurrencyConfigDateRange()
		{
			var shipment = ObjectCreator.CreateShipment("S000029");
			var shipmentJob = ObjectCreator.CreateJob(shipment);
			Factory.Save();

			var rateConsumer = shipmentJob.ExchangeRateConfigurationRateConsumer;
			var exRateConfig = CreateAccExchangeRateConfiguration(JobInvoicingConsumerTypes.Shipment.Code, ZString.Empty, TransportModes.All, FreightShipmentDirection.Code.All, ExchangeRateTypes.Code.SellRate, preference: "TST", currencyCode: "USD");

			var currencyConfig = exRateConfig.GetCurrencyConfig("USD", ZDate.Empty);
			currencyConfig.JCT_StartDate = new ZDate(2025, 2, 3);
			currencyConfig.JCT_ExpiryDate = new ZDate(2025, 2, 5);

			var getExchangeRateDateMock = MockIAccExchangeRateConfigurationsHelper(rateConsumer, "TST", new ZDateTime(2025, 2, 10));

			using (ObjectFactory.Substitute(getExchangeRateDateMock.Object))
			{
				var actualExRateConfig = AccExchangeRateConfigurationRateFinder.GetExchangeRateConfiguration(rateConsumer, null, ExchangeRateValidLedgerEnum.AR, InvoiceCurrencyType.NotApplicable, "USD");
				AssertEquals("When no date range found, should use system default currency config", SystemDefaultExchangeRateConfig.PK, actualExRateConfig.PK);
			}

			getExchangeRateDateMock.Verify(x => x.GetExchangeRateDate(It.IsAny<IAccExchangeRateConfigurationRateConsumer>(), It.IsAny<ZString>()), Times.Exactly(3));
		}

		#region Helpers

		AccExchangeRateConfiguration CreateAccExchangeRateConfiguration(ZString jobType, ZString ledgerType, ZString transportMode, ZString direction, ZString rateType, string preference = JobBillingExchangeRatePreference.Code.TodaysRate, int offset = 0, bool prompt = false, string invoiceCurrencyType = "", string currencyCode = "")
		{
			var config = MasterTestObjectCreator.CreateAccExchangeRateConfiguration(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, ledgerType, jobType, direction, transportMode, invoiceCurrencyType, new string[] { currencyCode });
			SetConfig(config, rateType, preference, offset, prompt, currencyCode);
			return config;
		}

		AccExchangeRateConfiguration CreateAccExchangeRateConfiguration(GlbCompany company, ZString jobType, ZString ledgerType, ZString transportMode, ZString direction, ZString rateType, string preference = JobBillingExchangeRatePreference.Code.TodaysRate, int offset = 0, bool prompt = false, string invoiceCurrencyType = "", string currencyCode = "")
		{
			var config = MasterTestObjectCreator.CreateAccExchangeRateConfiguration(AccExRateConfigurationLevelEnum.Company, ZGuid.Empty, ledgerType, jobType, direction, transportMode, invoiceCurrencyType, new string[] { currencyCode });
			config.JCE_GC = company.PK;
			SetConfig(config, rateType, preference, offset, prompt, currencyCode);
			Factory.Save();
			company.AccExchangeRateConfigurations.Load();
			return config;
		}

		AccExchangeRateConfiguration CreateAccExchangeRateConfiguration(OrgCompanyData companyData, ZString jobType, ZString ledgerType, ZString transportMode, ZString direction, ZString rateType, string preference = JobBillingExchangeRatePreference.Code.TodaysRate, int offset = 0, bool prompt = false, string invoiceCurrencyType = "", string currencyCode = "")
		{
			AccExchangeRateConfiguration config;
			if (ledgerType == LedgerTypes.AccountsPayable)
			{
				config = MasterTestObjectCreator.CreateAccExchangeRateConfiguration(AccExRateConfigurationLevelEnum.Creditor, companyData.Header.PK, ledgerType, jobType, direction, transportMode, invoiceCurrencyType, new string[] { currencyCode });
			}
			else
			{
				config = MasterTestObjectCreator.CreateAccExchangeRateConfiguration(AccExRateConfigurationLevelEnum.Debtor, companyData.Header.PK, ledgerType, jobType, direction, transportMode, invoiceCurrencyType, new string[] { currencyCode });
			}
			SetConfig(config, rateType, preference, offset, prompt, currencyCode);
			return config;
		}

		AccExchangeRateConfiguration CreateAccExchangeRateConfiguration(OrgDebtorGroup debtorGroup, ZString jobType, ZString transportMode, ZString direction, ZString rateType, string preference = JobBillingExchangeRatePreference.Code.TodaysRate, int offset = 0, bool prompt = false, string invoiceCurrencyType = "", string currencyCode = "")
		{
			var config = MasterTestObjectCreator.CreateAccExchangeRateConfiguration(AccExRateConfigurationLevelEnum.DebtorGroup, debtorGroup.PK, LedgerTypes.AccountsReceivable, jobType, direction, transportMode, invoiceCurrencyType, new string[] { currencyCode });
			SetConfig(config, rateType, preference, offset, prompt, currencyCode);
			return config;
		}

		AccExchangeRateConfiguration CreateAccExchangeRateConfiguration(OrgCreditorGroup creditorGroup, ZString jobType, ZString transportMode, ZString direction, ZString rateType, string preference = JobBillingExchangeRatePreference.Code.TodaysRate, int offset = 0, bool prompt = false, string invoiceCurrencyType = "", string currencyCode = "")
		{
			var config = MasterTestObjectCreator.CreateAccExchangeRateConfiguration(AccExRateConfigurationLevelEnum.CreditorGroup, creditorGroup.PK, LedgerTypes.AccountsPayable, jobType, direction, transportMode, invoiceCurrencyType, new string[] { currencyCode });
			SetConfig(config, rateType, preference, offset, prompt, currencyCode);
			return config;
		}

		void SetConfig(AccExchangeRateConfiguration config, ZString rateType, ZString preference, ZInt offset, ZBool prompt, ZString currencyCode)
		{
			config.JCE_Preference = preference;
			config.JCE_Offset = offset;
			config.JCE_Prompt = prompt;
			config.GetCurrencyConfig(currencyCode, ZDate.Empty).JCT_ExRateType = rateType;
		}

		void SetUpExchangeRates(out decimal todaysBuyRate, out decimal todaysSellRate, out decimal todaysCustomsRate, out decimal todaysIATARate, out decimal todaysC01Rate, out decimal todaysC02Rate, out decimal todaysC03Rate, out decimal todaysC04Rate)
		{
			var localClientPk = ZGuid.Empty;
			todaysBuyRate = 0.123m;
			todaysSellRate = 0.234m;
			todaysCustomsRate = 0.345m;
			todaysIATARate = 0.567m;
			todaysC01Rate = 0.678m;
			todaysC02Rate = 0.789m;
			todaysC03Rate = 0.891m;
			todaysC04Rate = 0.456m;
			CreateExchangeRate(Core.Constants.ExchangeRateTypes.Code.BuyRate, todaysBuyRate, localClientPk);
			CreateExchangeRate(Core.Constants.ExchangeRateTypes.Code.SellRate, todaysSellRate, localClientPk);
			CreateExchangeRate(Core.Constants.ExchangeRateTypes.Code.CustomsRate, todaysCustomsRate, localClientPk);
			CreateExchangeRate(Core.Constants.ExchangeRateTypes.Code.IATARate, todaysIATARate, localClientPk);
			CreateExchangeRate(Core.Constants.ExchangeRateTypes.Code.C01Rate, todaysC01Rate, localClientPk);
			CreateExchangeRate(Core.Constants.ExchangeRateTypes.Code.C02Rate, todaysC02Rate, localClientPk);
			CreateExchangeRate(Core.Constants.ExchangeRateTypes.Code.C03Rate, todaysC03Rate, localClientPk);
			CreateExchangeRate(Core.Constants.ExchangeRateTypes.Code.C04Rate, todaysC04Rate, localClientPk);
		}

		void SetUpExchangeRatesWithLocalClient(out decimal todaysBuyRate, out decimal todaysSellRate, ZGuid localClientPK)
		{
			todaysBuyRate = 1.888m;
			todaysSellRate = 1.766m;
			CreateExchangeRate(Core.Constants.ExchangeRateTypes.Code.BuyRate, todaysBuyRate, localClientPK);
			CreateExchangeRate(Core.Constants.ExchangeRateTypes.Code.SellRate, todaysSellRate, localClientPK);
		}

		void CreateExchangeRate(ZString rateType, ZDecimal todaysRate, ZGuid localClientPK)
		{
			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, rateType, GetExpectExchangeRateWithOffset(todaysRate, offsetFlag: 0), ZDateTime.Today, ZDateTime.Today, localClientPK);
			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, rateType, GetExpectExchangeRateWithOffset(todaysRate, offsetFlag: -1), ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-1), localClientPK);
			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, rateType, GetExpectExchangeRateWithOffset(todaysRate, offsetFlag: 1), ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(1), localClientPK);
			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, rateType, GetExpectExchangeRateWithOffset(todaysRate, offsetFlag: 2), ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(2), localClientPK);
		}

		ZDecimal GetExpectExchangeRateWithOffset(ZDecimal baseRate, int offsetFlag)
		{
			switch (offsetFlag)
			{
				case 1:
					return baseRate + 2;
				case 2:
					return baseRate + 20;
				case -1:
					return baseRate + 1;
				default:
					return baseRate;
			}
		}

		void AssertPreferenceConsolExchangeRate(string comment, string currencyCode, bool expectedResult, IAccExchangeRateConfigurationRateConsumer shipmentJobConsumer, IAccExchangeRateConfigurationRateConsumer consolCostConsumer)
		{
			AssertEquals(comment, expectedResult, AccExchangeRateConfigurationRateFinder.IsPreferenceConsolExchangeRateAtCompanyLevelForJobWithConsolDirectionAndConsolTransportMode(
				shipmentJobConsumer,
				consolCostConsumer,
				currencyCode)
			);
		}

		Mock<IAccExchangeRateConfigurationsHelper> MockIAccExchangeRateConfigurationsHelper(IAccExchangeRateConfigurationRateConsumer consumer, ZString preference, ZDateTime returnDate)
		{
			var getExchangeRateDateMock = new Mock<IAccExchangeRateConfigurationsHelper>(MockBehavior.Strict);
			getExchangeRateDateMock.Setup(x => x.GetExchangeRateDate(consumer, preference)).Returns(returnDate);
			getExchangeRateDateMock.Setup(x => x.GetExchangeRateDate(consumer, "TDR")).Returns(ZDateTime.MaxSmallDateTime);
			return getExchangeRateDateMock;
		}

		#endregion
		protected override void SetUp()
		{
			base.SetUp();

			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);

			SystemDefaultExchangeRateConfig = MasterTestObjectCreator.CreateAccExchangeRateConfiguration(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, ledger: string.Empty, TransportModes.All, FreightShipmentDirection.Code.All, TransportModes.All);
			Factory.Save();
		}

		void ResetExchangeRateConfiguration()
		{
			var allSettings = Factory.Load<AccExchangeRateConfiguration>(new ZQuery(AccExchangeRateConfigurationViewSchema.PK, SQLComparisonOperator.NotEqual, SystemDefaultExchangeRateConfig.PK))
				.Cast<AccExchangeRateConfiguration>().ToArray();

			allSettings.ForEach(x => {
				x.CurrencyConfigurations.RemoveAndDeleteAll();
			});
			Factory.Save();

			allSettings.ForEach(x => {
				x.Delete();
			});

			Factory.Save();
		}

		AccExchangeRateConfiguration SystemDefaultExchangeRateConfig;

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;

		AccountingTestObjectCreator MasterTestObjectCreator => masterTestObjectCreator ?? (masterTestObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator masterTestObjectCreator;

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;
	}
}

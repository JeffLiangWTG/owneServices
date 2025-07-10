using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public class ElectronicProcessingChargeProviderTest : TestCaseWithFactory
	{
		public void TestCreateElectronicProcessingCharge_NotCreated()
		{
			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("CC1");
			globalChargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader2.PK;
			globalChargeCode.AC_MarginPercentage = 80m;
			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, TestObjectCreator.LocalCurrency.Code, 1, ZDateTime.Today.AddDays(-10));
			Factory.Save();

			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
			var jobNotCreateElectronicProcessingCharge = TestObjectCreator.CreateJobHeader();

			var jRJQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.JobRevenueJournal);
			var wipQuery = new ZQuery(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.WIP);
			AssertEquals(false, Factory.Exists(typeof(AccTransactionHeader), jRJQuery));
			AssertEquals(false, Factory.Exists(typeof(AccTransactionLines), wipQuery));

			IElectronicProcessingChargeProvider electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
			electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobNotCreateElectronicProcessingCharge);

			AssertEquals("JRJ is not created because EnableElectronicProcessingChargeFunctionality is false.", false, Factory.Exists(typeof(AccTransactionHeader), jRJQuery));
			AssertEquals("WIP is not created because EnableElectronicProcessingChargeFunctionality is false.", false, Factory.Exists(typeof(AccTransactionLines), wipQuery));

			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(TestObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			jobNotCreateElectronicProcessingCharge = TestObjectCreator.CreateJobHeader();
			electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobNotCreateElectronicProcessingCharge);

			AssertEquals("JRJ is not created because ElectronicProcessingChargePayableClearingAccount is not set.", false, Factory.Exists(typeof(AccTransactionHeader), jRJQuery));
			AssertEquals("WIP is not created because ElectronicProcessingChargePayableClearingAccount is not set.", false, Factory.Exists(typeof(AccTransactionLines), wipQuery));

			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			jobNotCreateElectronicProcessingCharge = TestObjectCreator.CreateJobHeader();
			electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobNotCreateElectronicProcessingCharge);

			AssertEquals("JRJ is not created because ElectronicProcessingChargeCode is not set.", false, Factory.Exists(typeof(AccTransactionHeader), jRJQuery));
			AssertEquals("WIP is not created because ElectronicProcessingChargeCode is not set.", false, Factory.Exists(typeof(AccTransactionLines), wipQuery));

			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToGuid());
			jobNotCreateElectronicProcessingCharge = TestObjectCreator.CreateJobHeader();
			electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobNotCreateElectronicProcessingCharge);

			AssertEquals("JRJ is not created because job parent is not shipment.", false, Factory.Exists(typeof(AccTransactionHeader), jRJQuery));
			AssertEquals("WIP is not created because job parent is not shipment.", false, Factory.Exists(typeof(AccTransactionLines), wipQuery));

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var jobCreateElectronicProcessingCharge = TestObjectCreator.CreateJob(shipment, false);
			jobCreateElectronicProcessingCharge.PlugInData = shipment;

			var chargeCurrencies = new ElectronicProcessingChargeCurrencyCollection();
			chargeCurrencies.Add(new ElectronicProcessingChargeCurrency { CurrencyPK = TestObjectCreator.CNY.PK, ValidFromDate = ZDateTime.Today.AddDays(-10) });
			using (AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCurrency.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeCurrencies))
			{
				electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobNotCreateElectronicProcessingCharge);

				AssertEquals("JRJ is not created because cannot find matched currency in Ref DB with the Charge Currency registry setting.", false, Factory.Exists(typeof(AccTransactionHeader), jRJQuery));
				AssertEquals("WIP is not created because cannot find matched currency in Ref DB with the Charge Currency registry setting.", false, Factory.Exists(typeof(AccTransactionLines), wipQuery));
			}

			electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobCreateElectronicProcessingCharge);
			AssertEquals("JRJ is not created because EnableElectronicProcessingChargeFunctionality registry of current login company is false.", false, Factory.Exists(typeof(AccTransactionHeader), jRJQuery));
			AssertEquals("WIP is not created because EnableElectronicProcessingChargeFunctionality registry of current login company is false.", false, Factory.Exists(typeof(AccTransactionLines), wipQuery));

			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobCreateElectronicProcessingCharge);
			AssertEquals("JRJ is not created because ElectronicProcessingChargeConfiguration registry doesn't include current job type.", false, Factory.Exists(typeof(AccTransactionHeader), jRJQuery));
			AssertEquals("WIP is not created because ElectronicProcessingChargeConfiguration registry doesn't include current job type.", false, Factory.Exists(typeof(AccTransactionLines), wipQuery));

			var mock = new Mock<IAccounting>();
			mock.Setup(x => x.IsIncludedInElectronicProcessingChargeConfiguration(It.IsAny<ZDateTime>(), It.IsAny<string>())).Returns(true);

			using (ObjectFactory.Substitute(mock.Object))
			{
				electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobCreateElectronicProcessingCharge);
				AssertEquals("JRJ is created.", true, Factory.Exists(typeof(AccTransactionHeader), jRJQuery));
				AssertEquals("WIP is created.", true, Factory.Exists(typeof(AccTransactionLines), wipQuery));
			}
		}

		[TestDate(2024, 10, 09, 13, 00, 00)]
		public void TestCreateElectronicProcessingCharge_CreatedJRJWIPWithChargeCurrencySetting_Shipment()
		{
			AssertCreateElectronicProcessingCharge_CreatedJRJWIPWithChargeCurrencySetting(JobInvoicingConsumerTypes.Shipment.Code);
		}

		[TestDate(2024, 10, 09, 13, 00, 00)]
		public void TestCreateElectronicProcessingCharge_CreatedJRJWIPWithChargeCurrencySetting_Borkerage()
		{
			AssertCreateElectronicProcessingCharge_CreatedJRJWIPWithChargeCurrencySetting(JobInvoicingConsumerTypes.Brokerage.Code);
		}

		void AssertCreateElectronicProcessingCharge_CreatedJRJWIPWithChargeCurrencySetting(string jobType)
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "SEL", 4m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 4m);

			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("CC1");
			globalChargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader2.PK;

			var revenueRecOverride = globalChargeCode.RevenueRecOverrides.AddNew();
			revenueRecOverride.AE_JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			revenueRecOverride.AE_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

			AssertEquals("Local Currency is AUD.", "AUD", TestObjectCreator.LocalCurrency.Code);

			var expectedCurrency = TestObjectCreator.CNY;
			var electronicProcessingFeeCode = string.Empty;
			switch (jobType)
			{
				case "SHP":
					electronicProcessingFeeCode = RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD;
					break;
				case "BRK":
					electronicProcessingFeeCode = RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.BRD;
					break;
				default:
					break;
			}

			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, electronicProcessingFeeCode, expectedCurrency.RX_Code, 13, ZDateTime.Today.AddDays(-10), "AU", "EXP");
			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, electronicProcessingFeeCode, expectedCurrency.RX_Code, 14, ZDateTime.Today.AddDays(-10), "AU", "ALL");
			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, electronicProcessingFeeCode, "USD", 15, ZDateTime.Today.AddDays(-20), "AU", "EXP");
			Factory.Save();

			var expectFee = 13m;

			var currentLocalChargeCode = globalChargeCode.ChildChargeCodes.FirstOrDefault(c => c.AC_GC == Env.CurrentCompanyPK);
			currentLocalChargeCode.AC_Desc = "efee 2";
			Factory.Save();

			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetUpElectronicProcessingChargeConfiguration(jobType);
			var chargeCurrencies = new ElectronicProcessingChargeCurrencyCollection();
			chargeCurrencies.Add(new ElectronicProcessingChargeCurrency { CurrencyPK = expectedCurrency.PK, ValidFromDate = ZDateTime.Today.AddDays(-10) });
			chargeCurrencies.Add(new ElectronicProcessingChargeCurrency { CurrencyPK = TestObjectCreator.USD.PK, ValidFromDate = ZDateTime.Today.AddDays(-20) });
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCurrency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeCurrencies);

			var jRJQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.JobRevenueJournal);
			var wipQuery = new ZQuery(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.WIP);
			AssertEquals(false, Factory.Exists(typeof(AccTransactionHeader), jRJQuery));
			AssertEquals(false, Factory.Exists(typeof(AccTransactionLines), wipQuery));

			var jobParent = (IJobInvoicingPlugIn)null;
			if (jobType == "SHP")
			{
				var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
				AssertEquals("Shipment direction is export.", Directions.Export, shipment.JobDirection);
				jobParent = shipment;
			}
			else if (jobType == "BRK")
			{
				jobParent = TestObjectCreator.CreateDeclaration() as IJobInvoicingPlugIn;
			}

			var jobCreateElectronicProcessingCharge = TestObjectCreator.CreateJob(jobParent, false);
			jobCreateElectronicProcessingCharge.PlugInData = jobParent;

			IElectronicProcessingChargeProvider electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
			electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobCreateElectronicProcessingCharge);

			AssertEquals("Current company country is AU.", "AU", GlbCompany.CurrentCompany.Country.Code);
			AssertEquals("JRJ is created.", true, Factory.Exists(typeof(AccTransactionHeader), jRJQuery));
			AssertEquals("WIP is created.", true, Factory.Exists(typeof(AccTransactionLines), wipQuery));

			var jRJ = Factory.LoadTop1<JobRevenueJournal>(jRJQuery);
			var lines = jRJ.JournalLines.Cast<JobRevenueJournalLine>();
			var lineWithCharge = lines.FirstOrDefault(x => x.AL_AC.IsValid);
			AssertEquals(expectedCurrency.RX_Code, lineWithCharge.AL_RX_NKTransactionCurrency);
			AssertEquals(4m, lineWithCharge.AL_ExchangeRate);
			AssertEquals((-1) * expectFee, lineWithCharge.AL_OSExTaxAmount);
			AssertEquals((-1) * expectFee / 4, lineWithCharge.AL_LocalExTaxAmount);

			AssertNotNull(lineWithCharge.RelatedJobCharge);
			AssertEquals(expectedCurrency.RX_Code, lineWithCharge.RelatedJobCharge.JR_RX_NKCostCurrency);
			AssertEquals(expectedCurrency.RX_Code, lineWithCharge.RelatedJobCharge.JR_RX_NKSellCurrency);
			AssertEquals(4m, lineWithCharge.RelatedJobCharge.JR_OSCostExRate);
			AssertEquals(4m, lineWithCharge.RelatedJobCharge.JR_OSSellExRate);
			AssertEquals(expectFee, lineWithCharge.RelatedJobCharge.JR_OSCostAmt);
			AssertEquals(expectFee / 4, lineWithCharge.RelatedJobCharge.JR_LocalCostAmt);
			AssertEquals(expectFee, lineWithCharge.RelatedJobCharge.JR_OSSellAmt);
			AssertEquals(expectFee / 4, lineWithCharge.RelatedJobCharge.JR_LocalSellAmt);

			var lineWithoutCharge = lines.FirstOrDefault(x => x.AL_AC == ZGuid.Empty);

			AssertEquals(expectedCurrency.RX_Code, lineWithoutCharge.AL_RX_NKTransactionCurrency);
			AssertEquals(4m, lineWithoutCharge.AL_ExchangeRate);
			AssertEquals(expectFee, lineWithoutCharge.AL_OSExTaxAmount);
			AssertEquals(expectFee / 4, lineWithoutCharge.AL_LocalExTaxAmount);

			var wip = Factory.LoadTop1<WIP>(wipQuery);
			AssertEquals(jobCreateElectronicProcessingCharge.PK, wip.AL_JH);
			AssertEquals("AUD", wip.AL_RX_NKTransactionCurrency);
			AssertEquals(1m, wip.AL_ExchangeRate);
			AssertEquals(expectFee / 4, wip.AL_OSExTaxAmount);
			AssertEquals(expectFee / 4, wip.AL_LocalExTaxAmount);
		}

		[TestDate(2024, 10, 09, 13, 00, 00)]
		public void TestCreateElectronicProcessingCharge_CreatedJRJWIP()
		{
			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("CC1");
			globalChargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader2.PK;
			globalChargeCode.AC_MarginPercentage = 80m;

			var revenueRecOverride = globalChargeCode.RevenueRecOverrides.AddNew();
			revenueRecOverride.AE_JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			revenueRecOverride.AE_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

			AssertEquals("Local Currency is AUD.", "AUD", TestObjectCreator.LocalCurrency.Code);

			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, TestObjectCreator.LocalCurrency.Code, 13, ZDateTime.Today.AddDays(-10), "AU", "EXP");
			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, TestObjectCreator.LocalCurrency.Code, 14, ZDateTime.Today, "AU", "EXP");

			Factory.Save();

			var currentLocalChargeCode = globalChargeCode.ChildChargeCodes.FirstOrDefault(c => c.AC_GC == Env.CurrentCompanyPK);
			currentLocalChargeCode.AC_Desc = "efee 2";
			Factory.Save();

			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			SetUpElectronicProcessingChargeConfiguration();

			var jRJQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.JobRevenueJournal);
			var wipQuery = new ZQuery(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.WIP);
			AssertEquals(false, Factory.Exists(typeof(AccTransactionHeader), jRJQuery));
			AssertEquals(false, Factory.Exists(typeof(AccTransactionLines), wipQuery));

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var jobCreateElectronicProcessingCharge = TestObjectCreator.CreateJob(shipment, false);
			jobCreateElectronicProcessingCharge.PlugInData = shipment;

			IElectronicProcessingChargeProvider electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
			electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobCreateElectronicProcessingCharge);

			AssertEquals("Shipment direction is export.", Directions.Export, shipment.JobDirection);
			AssertEquals("Current company country is AU.", "AU", GlbCompany.CurrentCompany.Country.Code);
			AssertEquals("JRJ is created.", true, Factory.Exists(typeof(AccTransactionHeader), jRJQuery));
			AssertEquals("WIP is created.", true, Factory.Exists(typeof(AccTransactionLines), wipQuery));

			var jRJ = Factory.LoadTop1<JobRevenueJournal>(jRJQuery);
			var lines = jRJ.JournalLines.Cast<JobRevenueJournalLine>();
			var lineWithCharge = lines.FirstOrDefault(x => x.AL_AC.IsValid);
			AssertEquals(ZDateTime.Now, lineWithCharge.AL_ReverseDate);
			AssertEquals(currentLocalChargeCode.PK, lineWithCharge.AL_AC);
			AssertEquals(TestObjectCreator.GLHeader2.PK, lineWithCharge.AL_AG);
			AssertEquals(jobCreateElectronicProcessingCharge.PK, lineWithCharge.AL_JH);
			AssertEquals(currentLocalChargeCode.AC_Desc, lineWithCharge.AL_Desc);
			AssertEquals(jobCreateElectronicProcessingCharge.JH_GC, lineWithCharge.AL_GC);
			AssertEquals(jobCreateElectronicProcessingCharge.JH_GB, lineWithCharge.AL_GB);
			AssertEquals(jobCreateElectronicProcessingCharge.JH_GE, lineWithCharge.AL_GE);
			AssertEquals(jobCreateElectronicProcessingCharge.Company.GC_RX_NKLocalCurrency, lineWithCharge.AL_RX_NKTransactionCurrency);
			AssertEquals(1m, lineWithCharge.AL_ExchangeRate);
			AssertEquals(-14m, lineWithCharge.AL_LocalExTaxAmount);
			AssertEquals(-14m, lineWithCharge.AL_OSAmount);

			var lineWithoutCharge = lines.FirstOrDefault(x => x.AL_AC == ZGuid.Empty);

			AssertEquals(ZDateTime.Now, lineWithoutCharge.AL_ReverseDate);
			AssertEquals(TestObjectCreator.GLHeader1.PK, lineWithoutCharge.AL_AG);
			AssertEquals(Guid.Empty, lineWithoutCharge.AL_JH);
			AssertEquals($"{currentLocalChargeCode.AC_Desc} {jobCreateElectronicProcessingCharge.JH_JobNum}", lineWithoutCharge.AL_Desc);
			AssertEquals(jobCreateElectronicProcessingCharge.JH_GC, lineWithoutCharge.AL_GC);
			AssertEquals(jobCreateElectronicProcessingCharge.JH_GB, lineWithoutCharge.AL_GB);
			AssertEquals(jobCreateElectronicProcessingCharge.JH_GE, lineWithoutCharge.AL_GE);
			AssertEquals(jobCreateElectronicProcessingCharge.Company.GC_RX_NKLocalCurrency, lineWithoutCharge.AL_RX_NKTransactionCurrency);
			AssertEquals(1m, lineWithoutCharge.AL_ExchangeRate);
			AssertEquals(14m, lineWithoutCharge.AL_LocalExTaxAmount);
			AssertEquals(14m, lineWithoutCharge.AL_OSAmount);

			var wip = Factory.LoadTop1<WIP>(wipQuery);
			AssertEquals(jobCreateElectronicProcessingCharge.PK, wip.AL_JH);
			AssertEquals(-17.5m, wip.AL_OSAmount);
		}

		[TestDate(2024, 10, 09, 13, 00, 00)]
		public void TestCreateElectronicProcessingCharge_DisbursementFee()
		{
			var newFactory = new BusinessObjectFactory();
			var newTestObjectCreator = new TestObjectCreator(newFactory);
			var globalChargeCode = newTestObjectCreator.CreateGlobalChargeCode("CC1");
			globalChargeCode.AC_AG_CostAccount = newTestObjectCreator.GLHeader2.PK;
			globalChargeCode.AC_MarginPercentage = 80m;

			AssertEquals("Local Currency is AUD.", "AUD", newTestObjectCreator.LocalCurrency.Code);

			newTestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, newTestObjectCreator.LocalCurrency.Code, 1, ZDateTime.Today.AddDays(-10));
			newTestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, newTestObjectCreator.LocalCurrency.Code, 2, ZDateTime.Today);
			newTestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, newTestObjectCreator.LocalCurrency.Code, 3, ZDateTime.Today.AddDays(10));
			newTestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, newTestObjectCreator.IDR.Code, 4, ZDateTime.Today);
			newTestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, newTestObjectCreator.TWD.Code, 5, ZDateTime.Today);
			newTestObjectCreator.CreateRefAccElectronicProcessingFee("XXX", RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, newTestObjectCreator.LocalCurrency.Code, 6, ZDateTime.Today);
			newTestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, "YYY", RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, newTestObjectCreator.LocalCurrency.Code, 7, ZDateTime.Today);
			newTestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, "ZZZ", newTestObjectCreator.LocalCurrency.Code, 8, ZDateTime.Today);

			newFactory.Save();

			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			SetUpElectronicProcessingChargeConfiguration();

			AssertDisbursementFee("S00001", 2m, -2.5m);

			newTestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, newTestObjectCreator.LocalCurrency.Code, 11, ZDateTime.Today.AddDays(-10), "", "EXP");
			newTestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, newTestObjectCreator.LocalCurrency.Code, 12, ZDateTime.Today, "", "EXP");
			newFactory.Save();

			AssertDisbursementFee("S00002", 12m, -15m);

			newTestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, newTestObjectCreator.LocalCurrency.Code, 9, ZDateTime.Today.AddDays(-10), "AU");
			newTestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, newTestObjectCreator.LocalCurrency.Code, 10, ZDateTime.Today, "AU");
			newFactory.Save();

			AssertDisbursementFee("S00003", 10m, -12.5m);

			newTestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, newTestObjectCreator.LocalCurrency.Code, 13, ZDateTime.Today.AddDays(-10), "AU", "EXP");
			newTestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, newTestObjectCreator.LocalCurrency.Code, 14, ZDateTime.Today, "AU", "EXP");
			newFactory.Save();

			AssertDisbursementFee("S00004", 14m, -17.5m);

			void AssertDisbursementFee(string shipmentNumber, ZDecimal expectJRJAmount, ZDecimal expectWIPAmount)
			{
				var shipment = TestObjectCreator.CreateShipment(shipmentNumber, "AUBNE", "JPTYO");
				var jobCreateElectronicProcessingCharge = TestObjectCreator.CreateJob(shipment, false);
				jobCreateElectronicProcessingCharge.PlugInData = shipment;

				IElectronicProcessingChargeProvider electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
				electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobCreateElectronicProcessingCharge);

				AssertEquals("Shipment direction is export.", Directions.Export, shipment.JobDirection);
				AssertEquals("Current company country is AU.", "AU", GlbCompany.CurrentCompany.Country.Code);

				Factory.Save();

				var jRJQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				jRJQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.JobRevenueJournal);

				var lineSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
				lineSubQuery.AddToFilter(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.Cost);
				lineSubQuery.AddToFilter(AccTransactionLinesSchema.AL_JH, jobCreateElectronicProcessingCharge.PK);
				jRJQuery.AddSubQuery(lineSubQuery, JoinCondition.And);

				var wipQuery = new ZQuery(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.WIP);
				wipQuery.AddToFilter(AccTransactionLinesSchema.AL_JH, jobCreateElectronicProcessingCharge.PK);

				var jRJ = Factory.LoadTop1<JobRevenueJournal>(jRJQuery);
				var lines = jRJ.JournalLines.Cast<JobRevenueJournalLine>();
				var lineWithCharge = lines.FirstOrDefault(x => x.AL_AC.IsValid);
				AssertEquals((-1) * expectJRJAmount, lineWithCharge.AL_LocalExTaxAmount);
				AssertEquals((-1) * expectJRJAmount, lineWithCharge.AL_OSAmount);

				var lineWithoutCharge = lines.FirstOrDefault(x => x.AL_AC == ZGuid.Empty);

				AssertEquals(expectJRJAmount, lineWithoutCharge.AL_LocalExTaxAmount);
				AssertEquals(expectJRJAmount, lineWithoutCharge.AL_OSAmount);

				var wip = Factory.LoadTop1<WIP>(wipQuery);
				AssertEquals(jobCreateElectronicProcessingCharge.PK, wip.AL_JH);
				AssertEquals(expectWIPAmount, wip.AL_OSAmount);
			}
		}

		[TestDate(2024, 10, 09, 13, 00, 00)]
		public void TestCreateElectronicProcessingCharge_SavedJH_Direction_WhenCreatedJRJ()
		{
			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("CC1");
			globalChargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader2.PK;
			globalChargeCode.AC_MarginPercentage = 80m;

			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, TestObjectCreator.LocalCurrency.Code, 11, ZDateTime.Today.AddDays(-10), "", "EXP");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.PlugInData = shipment;

			AssertNullOrEmpty("Precondition: JH_Direction is empty", job.JH_Direction);

			var jobCreateElectronicProcessingCharge = TestObjectCreator.CreateJob(shipment, false);
			jobCreateElectronicProcessingCharge.PlugInData = shipment;
			SetUpElectronicProcessingChargeConfiguration();

			IElectronicProcessingChargeProvider electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
			electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobCreateElectronicProcessingCharge);

			AssertEquals("job direction is EXP", "EXP", job.JH_Direction);
		}

		[TestDate(2024, 10, 09, 13, 00, 00)]
		public void TestCreateElectronicProcessingCharge_NotCreatedWithNoValidCurrency()
		{
			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("CC1");
			globalChargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader2.PK;
			globalChargeCode.AC_MarginPercentage = 80m;

			AssertEquals("Local Currency is AUD.", "AUD", TestObjectCreator.LocalCurrency.Code);

			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, TestObjectCreator.LocalCurrency.Code, 3, ZDateTime.Today.AddDays(10));
			Factory.Save();

			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			SetUpElectronicProcessingChargeConfiguration();

			var jRJQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.JobRevenueJournal);
			var wipQuery = new ZQuery(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.WIP);
			AssertEquals(false, Factory.Exists(typeof(AccTransactionHeader), jRJQuery));
			AssertEquals(false, Factory.Exists(typeof(AccTransactionLines), wipQuery));

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var jobCreateElectronicProcessingCharge = TestObjectCreator.CreateJob(shipment, false);
			jobCreateElectronicProcessingCharge.PlugInData = shipment;

			IElectronicProcessingChargeProvider electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
			electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobCreateElectronicProcessingCharge);

			AssertEquals("JRJ is not created.", false, Factory.Exists(typeof(AccTransactionHeader), jRJQuery));
			AssertEquals("WIP is not created.", false, Factory.Exists(typeof(AccTransactionLines), wipQuery));
		}

		[TestDate(2024, 10, 09, 13, 00, 00)]
		public void TestCreateElectronicProcessingCharge_NotCreatedWIP()
		{
			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("CC1");
			globalChargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader2.PK;
			globalChargeCode.AC_MarginPercentage = 80m;

			AssertEquals("Local Currency is AUD.", "AUD", TestObjectCreator.LocalCurrency.Code);

			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, TestObjectCreator.LocalCurrency.Code, 2, ZDateTime.Today);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			SetUpElectronicProcessingChargeConfiguration();

			var jRJQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.JobRevenueJournal);
			var wipQuery = new ZQuery(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.WIP);
			AssertEquals(false, Factory.Exists(typeof(AccTransactionHeader), jRJQuery));
			AssertEquals(false, Factory.Exists(typeof(AccTransactionLines), wipQuery));

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var jobCreateElectronicProcessingCharge = TestObjectCreator.CreateJob(shipment, false);
			jobCreateElectronicProcessingCharge.PlugInData = shipment;

			IElectronicProcessingChargeProvider electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
			electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobCreateElectronicProcessingCharge);

			AssertEquals("JRJ is created.", true, Factory.Exists(typeof(AccTransactionHeader), jRJQuery));
			AssertEquals("WIP is not created.", false, Factory.Exists(typeof(AccTransactionLines), wipQuery));
		}

		public void TestCreateElectronicProcessingCharge_JobChargeDescription()
		{
			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("CC1");
			globalChargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader2.PK;
			globalChargeCode.AC_MarginPercentage = 80m;

			AssertEquals("Local Currency is AUD.", "AUD", TestObjectCreator.LocalCurrency.Code);

			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, TestObjectCreator.LocalCurrency.Code, 1, ZDateTime.Today.AddDays(-10));
			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, TestObjectCreator.LocalCurrency.Code, 2, ZDateTime.Today);
			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, TestObjectCreator.LocalCurrency.Code, 3, ZDateTime.Today.AddDays(10));
			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, TestObjectCreator.IDR.Code, 4, ZDateTime.Today);
			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, TestObjectCreator.TWD.Code, 5, ZDateTime.Today);
			TestObjectCreator.CreateRefAccElectronicProcessingFee("XXX", RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, TestObjectCreator.LocalCurrency.Code, 6, ZDateTime.Today);
			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, "YYY", RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, TestObjectCreator.LocalCurrency.Code, 7, ZDateTime.Today);
			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, "ZZZ", TestObjectCreator.LocalCurrency.Code, 8, ZDateTime.Today);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			SetUpElectronicProcessingChargeConfiguration();

			var electronicProcessingChargeDescriptionOverrideCollection = new ElectronicProcessingChargeDescriptionOverrideCollection();
			electronicProcessingChargeDescriptionOverrideCollection.Add(new ElectronicProcessingChargeDescriptionOverride { Transport = "ALL", Container = "ALL", ShipmentType = "ALL", Origin = "SCC", Destination = "NSC", PrefixSuffix = "PRE", Text = "TEST1", IncludeShipmentNumber = true });
			electronicProcessingChargeDescriptionOverrideCollection.Add(new ElectronicProcessingChargeDescriptionOverride { Transport = "ALL", Container = "ULD", ShipmentType = "STD", Origin = "SCC", Destination = "NSC", PrefixSuffix = "PRE", Text = "TEST2", IncludeShipmentNumber = true });
			electronicProcessingChargeDescriptionOverrideCollection.Add(new ElectronicProcessingChargeDescriptionOverride { Transport = "AIR", Container = "ALL", ShipmentType = "STD", Origin = "SCC", Destination = "NSC", PrefixSuffix = "PRE", Text = "TEST3", IncludeShipmentNumber = true });
			electronicProcessingChargeDescriptionOverrideCollection.Add(new ElectronicProcessingChargeDescriptionOverride { Transport = "AIR", Container = "ULD", ShipmentType = "ALL", Origin = "SCC", Destination = "NSC", PrefixSuffix = "PRE", Text = "TEST4", IncludeShipmentNumber = true });
			electronicProcessingChargeDescriptionOverrideCollection.Add(new ElectronicProcessingChargeDescriptionOverride { Transport = "SEA", Container = "FCL", ShipmentType = "STD", Origin = "SCC", Destination = "NSC", PrefixSuffix = "PRE", Text = "TEST5", IncludeShipmentNumber = true });
			electronicProcessingChargeDescriptionOverrideCollection.Add(new ElectronicProcessingChargeDescriptionOverride { Transport = "AIR", Container = "ULD", ShipmentType = "STD", Origin = "NSC", Destination = "NSC", PrefixSuffix = "PRE", Text = "TEST6", IncludeShipmentNumber = true });

			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDescriptionOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, electronicProcessingChargeDescriptionOverrideCollection);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "ULD";
			shipment.JS_ShipmentType = "STD";
			shipment.IsDomesticFreight = false;

			var jobCreateElectronicProcessingCharge = TestObjectCreator.CreateJob(shipment, false);
			jobCreateElectronicProcessingCharge.PlugInData = shipment;

			IElectronicProcessingChargeProvider electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
			electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobCreateElectronicProcessingCharge);

			AssertEquals(1, jobCreateElectronicProcessingCharge.Charges.Count);
			AssertEquals("TEST4 CC1 Global Charge S00001", jobCreateElectronicProcessingCharge.Charges.Cast<JobCharge>().FirstOrDefault().JR_Desc);

			var countryCN = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.China));
			var countryAU = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia));
			FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new Guid[] { countryCN.PK.ToGuid(), countryAU.PK.ToGuid() });

			shipment = TestObjectCreator.CreateShipment("S00002", "CNBJS", "JPTYO");
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "ULD";
			shipment.JS_ShipmentType = "STD";
			shipment.IsDomesticFreight = false;

			jobCreateElectronicProcessingCharge = TestObjectCreator.CreateJob(shipment, false);
			jobCreateElectronicProcessingCharge.PlugInData = shipment;

			electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
			electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobCreateElectronicProcessingCharge);

			AssertEquals(1, jobCreateElectronicProcessingCharge.Charges.Count);
			AssertEquals("TEST4 CC1 Global Charge S00002", jobCreateElectronicProcessingCharge.Charges.Cast<JobCharge>().FirstOrDefault().JR_Desc);

			electronicProcessingChargeDescriptionOverrideCollection = new ElectronicProcessingChargeDescriptionOverrideCollection();
			electronicProcessingChargeDescriptionOverrideCollection.Add(new ElectronicProcessingChargeDescriptionOverride { Transport = "ALL", Container = "ALL", ShipmentType = "ALL", Origin = "SUJ", Destination = "NSJ", PrefixSuffix = "PRE", Text = "TEST7", IncludeShipmentNumber = true });
			electronicProcessingChargeDescriptionOverrideCollection.Add(new ElectronicProcessingChargeDescriptionOverride { Transport = "ALL", Container = "ULD", ShipmentType = "STD", Origin = "SUJ", Destination = "NSJ", PrefixSuffix = "PRE", Text = "TEST8", IncludeShipmentNumber = true });
			electronicProcessingChargeDescriptionOverrideCollection.Add(new ElectronicProcessingChargeDescriptionOverride { Transport = "AIR", Container = "ALL", ShipmentType = "STD", Origin = "SUJ", Destination = "NSJ", PrefixSuffix = "PRE", Text = "TEST9", IncludeShipmentNumber = true });
			electronicProcessingChargeDescriptionOverrideCollection.Add(new ElectronicProcessingChargeDescriptionOverride { Transport = "AIR", Container = "ULD", ShipmentType = "ALL", Origin = "SUJ", Destination = "NSJ", PrefixSuffix = "PRE", Text = "TEST10", IncludeShipmentNumber = true });
			electronicProcessingChargeDescriptionOverrideCollection.Add(new ElectronicProcessingChargeDescriptionOverride { Transport = "SEA", Container = "FCL", ShipmentType = "STD", Origin = "SUJ", Destination = "NSJ", PrefixSuffix = "PRE", Text = "TEST11", IncludeShipmentNumber = true });
			electronicProcessingChargeDescriptionOverrideCollection.Add(new ElectronicProcessingChargeDescriptionOverride { Transport = "AIR", Container = "ULD", ShipmentType = "STD", Origin = "NSJ", Destination = "NSJ", PrefixSuffix = "PRE", Text = "TEST12", IncludeShipmentNumber = true });

			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDescriptionOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, electronicProcessingChargeDescriptionOverrideCollection);

			var (newCompany, newBranch) = TestObjectCreator.CreateCompanyAndBranch("CNBJS", null);
			newBranch.GB_RL_NKHomePort = "CNBJS";

			shipment = TestObjectCreator.CreateShipment("S00003", "CNBJS", "JPTYO");
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "ULD";
			shipment.JS_ShipmentType = "STD";
			shipment.IsDomesticFreight = true;
			jobCreateElectronicProcessingCharge = TestObjectCreator.CreateJob(shipment, false);
			jobCreateElectronicProcessingCharge.PlugInData = shipment;

			jobCreateElectronicProcessingCharge.JH_GB = newBranch.PK;

			electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
			electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobCreateElectronicProcessingCharge);

			AssertEquals(1, jobCreateElectronicProcessingCharge.Charges.Count);
			AssertEquals("TEST10 CC1 Global Charge S00003", jobCreateElectronicProcessingCharge.Charges.Cast<JobCharge>().FirstOrDefault().JR_Desc);
		}

		public void TestCreateElectronicProcessingCharge_JobChargeDescription_Domestic()
		{
			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("CC1");
			globalChargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader2.PK;
			globalChargeCode.AC_MarginPercentage = 80m;

			AssertEquals("Local Currency is AUD.", "AUD", TestObjectCreator.LocalCurrency.Code);

			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, TestObjectCreator.LocalCurrency.Code, 2, ZDateTime.Today);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetUpElectronicProcessingChargeConfiguration();

			var electronicProcessingChargeDescriptionOverrideCollection = new ElectronicProcessingChargeDescriptionOverrideCollection();
			electronicProcessingChargeDescriptionOverrideCollection.Add(new ElectronicProcessingChargeDescriptionOverride { Transport = "ALL", Container = "ALL", ShipmentType = "ALL", Origin = "SCC", Destination = "NSC", PrefixSuffix = "PRE", Text = "TEST1", IncludeShipmentNumber = true });
			electronicProcessingChargeDescriptionOverrideCollection.Add(new ElectronicProcessingChargeDescriptionOverride { Transport = "ALL", Container = "ALL", ShipmentType = "ALL", Origin = "SUJ", Destination = "NSJ", PrefixSuffix = "PRE", Text = "TEST2", IncludeShipmentNumber = true });

			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDescriptionOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, electronicProcessingChargeDescriptionOverrideCollection);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "ULD";
			shipment.JS_ShipmentType = "STD";
			shipment.IsDomesticFreight = true;

			var jobCreateElectronicProcessingCharge = TestObjectCreator.CreateJob(shipment, false);
			jobCreateElectronicProcessingCharge.PlugInData = shipment;

			IElectronicProcessingChargeProvider electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
			electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobCreateElectronicProcessingCharge);

			AssertEquals(1, jobCreateElectronicProcessingCharge.Charges.Count);
			AssertEquals("TEST2 CC1 Global Charge S00001", jobCreateElectronicProcessingCharge.Charges.Cast<JobCharge>().FirstOrDefault().JR_Desc);

			electronicProcessingChargeDescriptionOverrideCollection = new ElectronicProcessingChargeDescriptionOverrideCollection();
			electronicProcessingChargeDescriptionOverrideCollection.Add(new ElectronicProcessingChargeDescriptionOverride { Transport = "ALL", Container = "ALL", ShipmentType = "ALL", Origin = "SCC", Destination = "NSC", PrefixSuffix = "PRE", Text = "TEST1", IncludeShipmentNumber = true });

			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDescriptionOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, electronicProcessingChargeDescriptionOverrideCollection);

			shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "ULD";
			shipment.JS_ShipmentType = "STD";
			shipment.IsDomesticFreight = true;

			jobCreateElectronicProcessingCharge = TestObjectCreator.CreateJob(shipment, false);
			jobCreateElectronicProcessingCharge.PlugInData = shipment;

			electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
			electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobCreateElectronicProcessingCharge);

			AssertEquals(1, jobCreateElectronicProcessingCharge.Charges.Count);
			AssertEquals("TEST1 CC1 Global Charge S00001", jobCreateElectronicProcessingCharge.Charges.Cast<JobCharge>().FirstOrDefault().JR_Desc);
		}

		public void TestCreateElectronicProcessingCharge_JobChargeLocalDescription()
		{
			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("CC1");
			globalChargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader2.PK;
			globalChargeCode.AC_MarginPercentage = 80m;
			globalChargeCode.AC_Desc = "CC1 DESC";
			Factory.Save();
			var chargeInCurrenctCompany = globalChargeCode.ChildChargeCodes.FirstOrDefault(c => c.AC_GC == Env.CurrentCompanyPK);
			chargeInCurrenctCompany.AC_LocalLanguageDescription = "CC1 LOCAL DESC";
			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, TestObjectCreator.LocalCurrency.Code, 1, ZDateTime.Today.AddDays(-10));
			Factory.Save();

			var jRJQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.JobRevenueJournal);
			jRJQuery.OrderBy = AccTransactionHeaderSchema.Constants.AH_InvoiceDate + OrderByClause.Descending;

			IElectronicProcessingChargeProvider electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();

			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToGuid());

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var jobCreateElectronicProcessingCharge = TestObjectCreator.CreateJob(shipment, false);
			jobCreateElectronicProcessingCharge.PlugInData = shipment;
			var collection = new ElectronicProcessingChargeConfigurationCollection();
			var config = new ElectronicProcessingChargeConfiguration() { JobType = "SHP", StartDate = ZDate.Today.AddDays(-1) };
			collection.Add(config);
			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (AccountingConfigurationRegistry.Instance.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobCreateElectronicProcessingCharge);
				AssertEquals(1, jobCreateElectronicProcessingCharge.Charges.Count);
				AssertEquals("CC1 DESC", jobCreateElectronicProcessingCharge.Charges.Cast<JobCharge>().FirstOrDefault().JR_Desc);

				var lines = Factory.LoadTop1<JobRevenueJournal>(jRJQuery).JournalLines.Cast<JobRevenueJournalLine>();
				var lineWithCharge = lines.FirstOrDefault(x => x.AL_AC.IsValid);
				AssertEquals("CC1 DESC", lineWithCharge.AL_Desc);
				var lineWithOutCharge = lines.FirstOrDefault(x => x.AL_AC == ZGuid.Empty);
				AssertEquals("CC1 DESC S00001", lineWithOutCharge.AL_Desc);
			}

			shipment = TestObjectCreator.CreateShipment("S00002", "AUBNE", "JPTYO");
			jobCreateElectronicProcessingCharge = TestObjectCreator.CreateJob(shipment, false);
			jobCreateElectronicProcessingCharge.PlugInData = shipment;

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (AccountingConfigurationRegistry.Instance.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobCreateElectronicProcessingCharge);
				AssertEquals(1, jobCreateElectronicProcessingCharge.Charges.Count);
				AssertEquals("CC1 DESC", jobCreateElectronicProcessingCharge.Charges.Cast<JobCharge>().FirstOrDefault().JR_Desc);

				var lines = Factory.LoadTop1<JobRevenueJournal>(jRJQuery).JournalLines.Cast<JobRevenueJournalLine>();
				var lineWithCharge = lines.FirstOrDefault(x => x.AL_AC.IsValid);
				AssertEquals("CC1 DESC", lineWithCharge.AL_Desc);
				var lineWithOutCharge = lines.FirstOrDefault(x => x.AL_AC == ZGuid.Empty);
				AssertEquals("CC1 DESC S00002", lineWithOutCharge.AL_Desc);
			}

			shipment = TestObjectCreator.CreateShipment("S00003", "AUBNE", "JPTYO");
			jobCreateElectronicProcessingCharge = TestObjectCreator.CreateJob(shipment, false);
			jobCreateElectronicProcessingCharge.PlugInData = shipment;

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (AccountingConfigurationRegistry.Instance.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobCreateElectronicProcessingCharge);
				AssertEquals(1, jobCreateElectronicProcessingCharge.Charges.Count);
				AssertEquals("CC1 LOCAL DESC", jobCreateElectronicProcessingCharge.Charges.Cast<JobCharge>().FirstOrDefault().JR_Desc);

				var lines = Factory.LoadTop1<JobRevenueJournal>(jRJQuery).JournalLines.Cast<JobRevenueJournalLine>();
				var lineWithCharge = lines.FirstOrDefault(x => x.AL_AC.IsValid);
				AssertEquals("CC1 DESC", lineWithCharge.AL_Desc);
				var lineWithOutCharge = lines.FirstOrDefault(x => x.AL_AC == ZGuid.Empty);
				AssertEquals("CC1 DESC S00003", lineWithOutCharge.AL_Desc);
			}

			shipment = TestObjectCreator.CreateShipment("S00004", "AUBNE", "JPTYO");
			shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight;
			jobCreateElectronicProcessingCharge = TestObjectCreator.CreateJob(shipment, false);
			jobCreateElectronicProcessingCharge.LocalChargesPK = TestObjectCreator.ABIGAS.PK;
			jobCreateElectronicProcessingCharge.PlugInData = shipment;

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (AccountingConfigurationRegistry.Instance.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobCreateElectronicProcessingCharge);
				AssertEquals(1, jobCreateElectronicProcessingCharge.Charges.Count);
				AssertEquals(TestObjectCreator.ABIGAS.PK, jobCreateElectronicProcessingCharge.Charges.Cast<JobCharge>().FirstOrDefault().JR_OH_SellAccount);
				AssertEquals("CC1 LOCAL DESC", jobCreateElectronicProcessingCharge.Charges.Cast<JobCharge>().FirstOrDefault().JR_Desc);

				var lines = Factory.LoadTop1<JobRevenueJournal>(jRJQuery).JournalLines.Cast<JobRevenueJournalLine>();
				var lineWithCharge = lines.FirstOrDefault(x => x.AL_AC.IsValid);
				AssertEquals("CC1 DESC", lineWithCharge.AL_Desc);
				var lineWithOutCharge = lines.FirstOrDefault(x => x.AL_AC == ZGuid.Empty);
				AssertEquals("CC1 DESC S00004", lineWithOutCharge.AL_Desc);
			}
		}

		[TestDate(2024, 10, 09, 13, 00, 00)]
		public void TestCreateElectronicProcessingCharge_ReverseDate()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Now.Year, 1, 1));

			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("CC1");
			globalChargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader2.PK;
			globalChargeCode.AC_MarginPercentage = 80m;
			var revenueRecOverride = globalChargeCode.RevenueRecOverrides.AddNew();
			revenueRecOverride.AE_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			revenueRecOverride.AE_Direction = Core.Constants.FreightShipmentDirection.Code.All;
			revenueRecOverride.AE_Mode = Core.Constants.TransportModes.All;
			revenueRecOverride.AE_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;

			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, TestObjectCreator.LocalCurrency.Code, 2, ZDateTime.Today);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetUpElectronicProcessingChargeConfiguration();

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var jobCreateElectronicProcessingCharge = TestObjectCreator.CreateJob(shipment, false);
			jobCreateElectronicProcessingCharge.PlugInData = shipment;

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			invoice.AH_JH = jobCreateElectronicProcessingCharge.PK;

			var line = invoice.Lines.AddNew() as InvoicingLineBase;
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_JH = jobCreateElectronicProcessingCharge.PK;
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_PostDate = ZDateTime.Today;
			jobCreateElectronicProcessingCharge.ApplyRevenueRecognitionDate(line);

			AssertRecognitionDate(false);
			AssertRecognitionDate(true);

			void AssertRecognitionDate(bool hasActualArrivalDate)
			{
				if (hasActualArrivalDate)
				{
					shipment.JS_E_ARV = ZDateTime.Now.AddDays(-5);
				}
				else
				{
					AssertEquals(ZDateTime.Empty, shipment.JS_E_ARV);
				}

				IElectronicProcessingChargeProvider electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
				electronicProcessingChargeProvider.CreateElectronicProcessingCharge(jobCreateElectronicProcessingCharge);

				var jRJQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.JobRevenueJournal);
				var wipQuery = new ZQuery(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.WIP);

				AssertEquals("JRJ is created.", true, Factory.Exists(typeof(AccTransactionHeader), jRJQuery));
				AssertEquals("WIP is created.", hasActualArrivalDate, Factory.Exists(typeof(AccTransactionLines), wipQuery));

				var jRJ = Factory.LoadTop1<JobRevenueJournal>(jRJQuery);
				var lines = jRJ.JournalLines.Cast<JobRevenueJournalLine>();
				var lineWithCharge = lines.FirstOrDefault(x => x.AL_AC.IsValid);
				AssertEquals(hasActualArrivalDate ? ZDateTime.Now.AddDays(-5) : ZDateTime.Empty, lineWithCharge.AL_ReverseDate);

				var lineWithoutCharge = lines.FirstOrDefault(x => x.AL_AC == ZGuid.Empty);

				AssertEquals(ZDateTime.Now, lineWithoutCharge.AL_ReverseDate);

				var wip = Factory.LoadTop1<WIP>(wipQuery);
				if (hasActualArrivalDate)
				{
					AssertEquals(ZDateTime.Now.AddDays(-5), wip.AL_PostDate);
				}
				else
				{
					AssertNull(wip);
				}
			}
		}

		public void TestInsertAndSetElectronicProcessingChargeRegistry()
		{
			AssertEquals("Pre - ElectronicProcessingChargePayableClearingAccount is empty", Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.Value);
			AssertEquals("Pre - ElectronicProcessingChargeDisbursementClearingAccount is empty", Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Value);
			AssertEquals("Pre - ElectronicProcessingChargeCode is empty", Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value);

			IElectronicProcessingChargeProvider electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
			electronicProcessingChargeProvider.InsertAndSetElectronicProcessingChargeRegistry();

			AssertEquals("ElectronicProcessingChargePayableClearingAccount will not be set because feature control is not set.", Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.Value);
			AssertEquals("ElectronicProcessingChargeDisbursementClearingAccount will not be set because feature control is not set.", Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Value);
			AssertEquals("ElectronicProcessingChargeCode will not be set because feature control is not set.", Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value);

			var mockIFeatureData = new Mock<IFeatureData>();
			var list = new List<CompanySecurityModel>
			{
				new CompanySecurityModel { Enable = false, CompanyCode = "ALL" }
			};
			var mockIFeatureControlManager = SetupElectronicProcessingChargeFeatureControl(false, list);

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				electronicProcessingChargeProvider.InsertAndSetElectronicProcessingChargeRegistry();

				var allCompanyPks = Factory.Load<GlbCompany>(new ZQuery()).Select(x => x.PK);
				allCompanyPks.ForEach(x => Assert(!AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetValueWithoutFallback(x.ToGuid(), Guid.Empty, Guid.Empty)));
				AssertEquals("ElectronicProcessingChargePayableClearingAccount will not be set because EnableInsertGLAccountsandChargeCodeForTheDisbursmentFee is false.", Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.Value);
				AssertEquals("ElectronicProcessingChargeDisbursementClearingAccount will not be set because EnableInsertGLAccountsandChargeCodeForTheDisbursmentFee is false.", Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Value);
				AssertEquals("ElectronicProcessingChargeCode will not be set because EnableInsertGLAccountsandChargeCodeForTheDisbursmentFee is false.", Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value);
			}

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "ALL";
			Factory.Save();
			list = new List<CompanySecurityModel>
			{
				new CompanySecurityModel { Enable = true, CompanyCode = "ALL" }
			};
			mockIFeatureControlManager = SetupElectronicProcessingChargeFeatureControl(false, list);
			var allCompanies = Factory.Load<GlbCompany>(new ZQuery());

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				electronicProcessingChargeProvider.InsertAndSetElectronicProcessingChargeRegistry();

				var nonAllCompanyPks = allCompanies.Where(x => x.PK != company.PK).Select(x => x.PK);
				Assert(AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
				nonAllCompanyPks.ForEach(x => Assert(!AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetValueWithoutFallback(x.ToGuid(), Guid.Empty, Guid.Empty)));
				AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.Value);
				AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Value);
				AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value);

				var electronicProcessingChargePayableClearingAccount = Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.Value);

				AssertEquals("ELECTRONIC PROCESSING FEE PAYABLE", electronicProcessingChargePayableClearingAccount.AG_Description);
				AssertEquals(Core.Constants.AccountType.BalanceSheetAccount, electronicProcessingChargePayableClearingAccount.AG_AccountType);
				AssertEquals(Core.Constants.DebitCredit.Credit, electronicProcessingChargePayableClearingAccount.AG_DebitCredit);
				AssertEquals(false, electronicProcessingChargePayableClearingAccount.AG_ControlAccount);
				AssertEquals(false, electronicProcessingChargePayableClearingAccount.AG_DisallowDirectPosting);
				AssertEquals(CashFlowCodeLists.Codes.XXX, electronicProcessingChargePayableClearingAccount.AG_CashFlowType);
				AssertEquals(User.ServiceUserCode, electronicProcessingChargePayableClearingAccount.AG_SystemCreateUser);

				var electronicProcessingChargeRecoveryClearingAccount = Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Value);

				AssertEquals("ELECTRONIC PROCESSING FEE CLEARING", electronicProcessingChargeRecoveryClearingAccount.AG_Description);
				AssertEquals(Core.Constants.AccountType.BalanceSheetAccount, electronicProcessingChargeRecoveryClearingAccount.AG_AccountType);
				AssertEquals(Core.Constants.DebitCredit.Credit, electronicProcessingChargeRecoveryClearingAccount.AG_DebitCredit);
				AssertEquals(false, electronicProcessingChargeRecoveryClearingAccount.AG_ControlAccount);
				AssertEquals(true, electronicProcessingChargeRecoveryClearingAccount.AG_DisallowDirectPosting);
				AssertEquals(CashFlowCodeLists.Codes.XXX, electronicProcessingChargeRecoveryClearingAccount.AG_CashFlowType);
				AssertEquals(User.ServiceUserCode, electronicProcessingChargeRecoveryClearingAccount.AG_SystemCreateUser);

				var electronicProcessingChargeCode = Factory.Load<AccChargeCode>(AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value);

				AssertEquals("EFEE", electronicProcessingChargeCode.AC_Code);
				AssertEquals("ELECTRONIC PROCESSING FEE", electronicProcessingChargeCode.AC_Desc);
				AssertEquals(Core.Constants.ChargeType.Disbursement, electronicProcessingChargeCode.AC_ChargeType);
				AssertEquals(100m, electronicProcessingChargeCode.AC_MarginPercentage);
				AssertEquals(AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Value, electronicProcessingChargeCode.AC_AG_RevenueAccount);
				AssertEquals(AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Value, electronicProcessingChargeCode.AC_AG_WIPAccount);
				AssertEquals(AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Value, electronicProcessingChargeCode.AC_AG_CostAccount);
				AssertEquals(AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Value, electronicProcessingChargeCode.AC_AG_AccrualAccount);
				AssertEquals(ChargeCodeGroupList.Codes.Freight, electronicProcessingChargeCode.AC_ChargeGroup);
				AssertEquals(Guid.Empty, electronicProcessingChargeCode.AC_GC);
				AssertEquals(User.ServiceUserCode, electronicProcessingChargeCode.AC_SystemCreateUser);
				AssertEquals(1, electronicProcessingChargeCode.RevenueRecOverrides.Count);

				var revenueRecOverride = electronicProcessingChargeCode.RevenueRecOverrides.Cast<AccChargeRevRecOverride>().First();
				AssertEquals(JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, revenueRecOverride.AE_JobType);
				AssertEquals(RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, revenueRecOverride.AE_RecognitionType);
			}
		}

		public void TestInsertAndSetElectronicProcessingChargeRegistry_UpdateSystemLevelConfig()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "DDD";
			Factory.Save();
			var mockIFeatureControlManager = SetupElectronicProcessingChargeFeatureControl(true, null);
			IElectronicProcessingChargeProvider electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
			var allCompanies = Factory.Load<GlbCompany>(new ZQuery());

			Assert(!AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			Assert(!AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty));

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				electronicProcessingChargeProvider.InsertAndSetElectronicProcessingChargeRegistry();

				Assert(AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
				Assert(AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty));

				AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.Value);
				AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Value);
				AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value);

				Assert(AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
				Assert(!AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.HasActualValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}

			var list = new List<CompanySecurityModel>
			{
				new CompanySecurityModel { Enable = true, CompanyCode = "DDD" }
			};
			mockIFeatureControlManager = SetupElectronicProcessingChargeFeatureControl(false, list);

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				electronicProcessingChargeProvider.InsertAndSetElectronicProcessingChargeRegistry();

				Assert(!AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
				Assert(!AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty));

				Assert(AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
				Assert(AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.HasActualValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}

			mockIFeatureControlManager = SetupElectronicProcessingChargeFeatureControl(false, null);

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				electronicProcessingChargeProvider.InsertAndSetElectronicProcessingChargeRegistry();

				Assert(!AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
				Assert(!AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty));
				Assert(!AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
				Assert(!AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.HasActualValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}
		}

		public void TestInsertAndSetElectronicProcessingChargeRegistry_UpdateCompaniesConfig()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "DDD";
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "CCC";
			Factory.Save();
			var list = new List<CompanySecurityModel>
			{
				new CompanySecurityModel { Enable = true, CompanyCode = "DDD" }
			};
			var mockIFeatureControlManager = SetupElectronicProcessingChargeFeatureControl(false, list);
			IElectronicProcessingChargeProvider electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
			var allCompanies = Factory.Load<GlbCompany>(new ZQuery());

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				electronicProcessingChargeProvider.InsertAndSetElectronicProcessingChargeRegistry();

				AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.Value);
				AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Value);
				AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value);

				Assert(AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
				Assert(AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.HasActualValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
				Assert(!AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetValueWithoutFallback(company2.PK.ToGuid(), Guid.Empty, Guid.Empty));
				Assert(!AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.HasActualValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}

			list = new List<CompanySecurityModel>
			{
				new CompanySecurityModel { Enable = true, CompanyCode = "CCC" },
			};
			mockIFeatureControlManager = SetupElectronicProcessingChargeFeatureControl(false, list);

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				electronicProcessingChargeProvider.InsertAndSetElectronicProcessingChargeRegistry();

				Assert(!AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
				Assert(!AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.HasActualValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
				Assert(AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetFallBackValueAtAllLevels(company2.PK.ToGuid(), Guid.Empty, Guid.Empty));
				Assert(AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.HasActualValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}

			list = new List<CompanySecurityModel>
			{
				new CompanySecurityModel { Enable = true, CompanyCode = "DDD" }
			};
			mockIFeatureControlManager = SetupElectronicProcessingChargeFeatureControl(false, list);

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				electronicProcessingChargeProvider.InsertAndSetElectronicProcessingChargeRegistry();

				Assert(AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
				Assert(AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.HasActualValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
				Assert(!AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetFallBackValueAtAllLevels(company2.PK.ToGuid(), Guid.Empty, Guid.Empty));
				Assert(!AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.HasActualValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}
		}

		public void TestInsertAndSetElectronicProcessingChargeRegistry_UpdateOtherConpaniesConfig()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "DDD";
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "CCC";
			Factory.Save();
			var list = new List<CompanySecurityModel>
			{
				new CompanySecurityModel { Enable = true, CompanyCode = "DDD" }
			};
			var mockIFeatureControlManager = SetupElectronicProcessingChargeFeatureControl(false, list);
			IElectronicProcessingChargeProvider electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
			var allCompanies = Factory.Load<GlbCompany>(new ZQuery());

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				electronicProcessingChargeProvider.InsertAndSetElectronicProcessingChargeRegistry();

				AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.Value);
				AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Value);
				AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value);

				Assert(AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
				Assert(AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.HasActualValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}

			list = new List<CompanySecurityModel>
			{
				new CompanySecurityModel { Enable = true, CompanyCode = "CCC" }
			};
			mockIFeatureControlManager = SetupElectronicProcessingChargeFeatureControl(false, list);

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				electronicProcessingChargeProvider.InsertAndSetElectronicProcessingChargeRegistry();

				Assert(!AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
				Assert(!AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.HasActualValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
				Assert(AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetValueWithoutFallback(company2.PK.ToGuid(), Guid.Empty, Guid.Empty));
				Assert(AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Inner.HasActualValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}
		}

		Mock<IFeatureControlManager> SetupElectronicProcessingChargeFeatureControl(bool enableSystemLevel, List<CompanySecurityModel> companyLevel)
		{
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			var mockIFeatureData = new Mock<IFeatureData>();
			var enableDisbursementLicenseFeeModel = new EnableDisbursementLicenseFeeModel() { EnableSystemLevel = enableSystemLevel, CompanyLevel = companyLevel };
			var electronicProcessingChargeFeatureControlModel = new ElectronicProcessingChargeFeatureControlModel() { EnableDisbursementLicenseFee = enableDisbursementLicenseFeeModel };
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out electronicProcessingChargeFeatureControlModel)).Returns(true);
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingElectronicProcessingChargeFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));
			return mockIFeatureControlManager;
		}

		[DisableZeroExchangeRateOverriding]
		public void TestHasElectronicProcessingChargeCurrencyExchangeRate_NotSetElectronicProcessingChargeFunctionality()
		{
			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("CC1");
			globalChargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader2.PK;
			globalChargeCode.AC_MarginPercentage = 80m;

			var revenueRecOverride = globalChargeCode.RevenueRecOverrides.AddNew();
			revenueRecOverride.AE_JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			revenueRecOverride.AE_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, TestObjectCreator.USD.Code, 14, ZDateTime.Today, "AU", "EXP");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.PlugInData = shipment;
			IElectronicProcessingChargeProvider electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
			var result = electronicProcessingChargeProvider.HasElectronicProcessingChargeCurrencyExchangeRate(job);

			AssertEquals("HasElectronicProcessingChargeCurrencyExchangeRate will be true when EnableElectronicProcessingChargeFunctionality is false.", true, result);

			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			result = electronicProcessingChargeProvider.HasElectronicProcessingChargeCurrencyExchangeRate(job);
			AssertEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value);
			AssertEquals("HasElectronicProcessingChargeCurrencyExchangeRate will be true when ElectronicProcessingChargeCode is not set.", true, result);

			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToGuid());
			result = electronicProcessingChargeProvider.HasElectronicProcessingChargeCurrencyExchangeRate(job);
			AssertEquals(false, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCurrency.Value.Any());
			AssertEquals("HasElectronicProcessingChargeCurrencyExchangeRate will be true when ElectronicProcessingChargeCurrency is not set.", true, result);

			var chargeCurrencies = new ElectronicProcessingChargeCurrencyCollection();
			chargeCurrencies.Add(new ElectronicProcessingChargeCurrency { CurrencyPK = TestObjectCreator.USD.PK, ValidFromDate = ZDateTime.Today.AddDays(-10) });
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCurrency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeCurrencies);
			var invoiceCurrencyType = AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(job.Company, ExchangeRateValidLedgerEnum.AR, "USD");

			var mock = new Mock<IAccounting>();
			mock.Setup(x => x.IsIncludedInElectronicProcessingChargeConfiguration(It.IsAny<ZDateTime>(), It.IsAny<string>())).Returns(false);

			using (ObjectFactory.Substitute(mock.Object))
			{
				result = electronicProcessingChargeProvider.HasElectronicProcessingChargeCurrencyExchangeRate(job);
				AssertEquals("HasElectronicProcessingChargeCurrencyExchangeRate will be true when IsIncludedInElectronicProcessingChargeConfiguration is false.", true, result);
			}

			mock.Setup(x => x.IsIncludedInElectronicProcessingChargeConfiguration(It.IsAny<ZDateTime>(), It.IsAny<string>())).Returns(true);

			using (ObjectFactory.Substitute(mock.Object))
			{
				result = electronicProcessingChargeProvider.HasElectronicProcessingChargeCurrencyExchangeRate(job);
				AssertEquals(false, job.ExchangeRates.Any());
				AssertEquals(0m, AccExchangeRateConfigurationRateFinder.GetExchangeRate(job.ExchangeRateConfigurationRateConsumer, TestObjectCreator.USD, null, ExchangeRateValidLedgerEnum.AR, invoiceCurrencyType));
				AssertEquals(0m, AccExchangeRateConfigurationRateFinder.GetExchangeRate(job.ExchangeRateConfigurationRateConsumer, TestObjectCreator.USD, null, ExchangeRateValidLedgerEnum.AP, InvoiceCurrencyType.NotApplicable));
				AssertEquals("HasElectronicProcessingChargeCurrencyExchangeRate will be false because no exchange rate for USD can be find.", false, result);

				AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Factory.Save();
				AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				result = electronicProcessingChargeProvider.HasElectronicProcessingChargeCurrencyExchangeRate(job);
				AssertEquals(true, job.IsInDatabase);
				AssertEquals("HasElectronicProcessingChargeCurrencyExchangeRate will be true when job is already saved.", true, result);
			}
		}

		[DisableZeroExchangeRateOverriding]
		public void TestHasElectronicProcessingChargeCurrencyExchangeRate_NoExchangeRate()
		{
			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("CC1");
			globalChargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader2.PK;
			globalChargeCode.AC_MarginPercentage = 80m;

			var revenueRecOverride = globalChargeCode.RevenueRecOverrides.AddNew();
			revenueRecOverride.AE_JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			revenueRecOverride.AE_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

			TestObjectCreator.CreateRefAccElectronicProcessingFee(RefAccElectronicProcessingFeeLookups.SystemCodes.CWN, RefAccElectronicProcessingFeeLookups.CategoryCodes.STL, RefAccElectronicProcessingFeeLookups.ElectronicProcessingFeeCodes.SHD, TestObjectCreator.USD.Code, 14, ZDateTime.Today, "AU", "EXP");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToGuid());
			var chargeCurrencies = new ElectronicProcessingChargeCurrencyCollection();
			chargeCurrencies.Add(new ElectronicProcessingChargeCurrency { CurrencyPK = TestObjectCreator.USD.PK, ValidFromDate = ZDateTime.Today.AddDays(-10) });
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCurrency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeCurrencies);
			var mock = new Mock<IAccounting>();
			mock.Setup(x => x.IsIncludedInElectronicProcessingChargeConfiguration(It.IsAny<ZDateTime>(), It.IsAny<string>())).Returns(true);
			using (ObjectFactory.Substitute(mock.Object))
			{
				var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
				var job = TestObjectCreator.CreateJob(shipment, false);
				job.PlugInData = shipment;
				var invoiceCurrencyType = AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(job.Company, ExchangeRateValidLedgerEnum.AR, "USD");

				IElectronicProcessingChargeProvider electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
				var result = electronicProcessingChargeProvider.HasElectronicProcessingChargeCurrencyExchangeRate(job);

				AssertEquals(false, job.ExchangeRates.Any());
				AssertEquals(0m, AccExchangeRateConfigurationRateFinder.GetExchangeRate(job.ExchangeRateConfigurationRateConsumer, TestObjectCreator.USD, null, ExchangeRateValidLedgerEnum.AR, invoiceCurrencyType));
				AssertEquals(0m, AccExchangeRateConfigurationRateFinder.GetExchangeRate(job.ExchangeRateConfigurationRateConsumer, TestObjectCreator.USD, null, ExchangeRateValidLedgerEnum.AP, InvoiceCurrencyType.NotApplicable));
				AssertEquals("HasElectronicProcessingChargeCurrencyExchangeRate will be false because no exchange rate for USD can be find.", false, result);

				var jobSellExchangeRate = job.ExchangeRates.AddNew();
				jobSellExchangeRate.JF_RX_NKRateCurrency = "USD";
				jobSellExchangeRate.JF_BaseRate = 2m;
				jobSellExchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
				result = electronicProcessingChargeProvider.HasElectronicProcessingChargeCurrencyExchangeRate(job);

				AssertEquals(0m, job.GetExchangeRate("USD", Guid.Empty, ExchangeRateOrgTypeEnum.Creditor, ExchangeRateType.Buy, invoiceCurrencyType));
				AssertEquals(0m, AccExchangeRateConfigurationRateFinder.GetExchangeRate(job.ExchangeRateConfigurationRateConsumer, TestObjectCreator.USD, null, ExchangeRateValidLedgerEnum.AR, invoiceCurrencyType));
				AssertEquals(0m, AccExchangeRateConfigurationRateFinder.GetExchangeRate(job.ExchangeRateConfigurationRateConsumer, TestObjectCreator.USD, null, ExchangeRateValidLedgerEnum.AP, InvoiceCurrencyType.NotApplicable));
				AssertEquals("HasElectronicProcessingChargeCurrencyExchangeRate will be false because no Sell exchange rate for USD can be find.", false, result);

				var jobBuyExchangeRate = job.ExchangeRates.AddNew();
				jobBuyExchangeRate.JF_RX_NKRateCurrency = "USD";
				jobBuyExchangeRate.JF_BaseRate = 3m;
				jobBuyExchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Creditor.ToCode();
				result = electronicProcessingChargeProvider.HasElectronicProcessingChargeCurrencyExchangeRate(job);

				AssertEquals(2m, job.GetExchangeRate("USD", Guid.Empty, ExchangeRateOrgTypeEnum.Debtor, ExchangeRateType.Sell, invoiceCurrencyType));
				AssertEquals(3m, job.GetExchangeRate("USD", Guid.Empty, ExchangeRateOrgTypeEnum.Creditor, ExchangeRateType.Buy, invoiceCurrencyType));
				AssertEquals("HasElectronicProcessingChargeCurrencyExchangeRate will be true.", true, result);

				job.ExchangeRates.RemoveAndDeleteAll();
				var buyUSDExchangeRate = new BusinessObjectFactory().NewWithValidTestData<RefExchangeRate>();
				buyUSDExchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
				buyUSDExchangeRate.RE_StartDate = ZDateTime.Today;
				buyUSDExchangeRate.RE_ExpiryDate = ZDateTime.Today;
				buyUSDExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
				buyUSDExchangeRate.RE_SellRate = 2m;
				buyUSDExchangeRate.RE_RX_NKExCurrency = TestObjectCreator.USD.Code;
				buyUSDExchangeRate.Factory.Save();

				AssertEquals(false, job.ExchangeRates.Any());
				AssertEquals(2m, AccExchangeRateConfigurationRateFinder.GetExchangeRate(job.ExchangeRateConfigurationRateConsumer, TestObjectCreator.USD, null, ExchangeRateValidLedgerEnum.AR, invoiceCurrencyType));
				AssertEquals(2m, AccExchangeRateConfigurationRateFinder.GetExchangeRate(job.ExchangeRateConfigurationRateConsumer, TestObjectCreator.USD, null, ExchangeRateValidLedgerEnum.AP, InvoiceCurrencyType.NotApplicable));
				AssertEquals(true, result);
			}
		}

		void SetUpElectronicProcessingChargeConfiguration(string jobType = "SHP")
		{
			var collection = new ElectronicProcessingChargeConfigurationCollection();
			var config = collection.AddNew();
			config.JobType = jobType;
			config.StartDate = ZDate.Today.AddDays(-1);
			config.EndDate = ZDate.Today.AddDays(1);
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		public void TestShouldCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting()
		{
			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode("CC1");
			globalChargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader2.PK;
			globalChargeCode.AC_MarginPercentage = 80m;

			var revenueRecOverride = globalChargeCode.RevenueRecOverrides.AddNew();
			revenueRecOverride.AE_JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			revenueRecOverride.AE_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

			Factory.Save();

			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var electronicProcessingChargeCodeInCurrentCompany = globalChargeCode.ChildChargeCodes.FirstOrDefault(x => x.AC_GC == GlbCompany.CurrentCompany.PK);
			AssertNotNull(electronicProcessingChargeCodeInCurrentCompany);

			IElectronicProcessingChargeProvider electronicProcessingChargeProvider = new ElectronicProcessingChargeProvider();
			AssertEquals("ShouldCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting will be false when charge is not electronicProcessingCharge", false, electronicProcessingChargeProvider.ShouldCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting(TestObjectCreator.CC11.PK, GlbCompany.CurrentCompany.PK));
			AssertEquals("ShouldCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting will be false when CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting is not true for the company.", false, electronicProcessingChargeProvider.ShouldCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting(electronicProcessingChargeCodeInCurrentCompany.PK, TestObjectCreator.NonCurrentCompany.PK));
			AssertEquals(true, electronicProcessingChargeProvider.ShouldCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting(electronicProcessingChargeCodeInCurrentCompany.PK, GlbCompany.CurrentCompany.PK));
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}

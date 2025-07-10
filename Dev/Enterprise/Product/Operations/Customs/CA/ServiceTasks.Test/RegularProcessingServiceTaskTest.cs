using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.NUnit;

namespace Enterprise.Customs.CA.ServiceTasks.Testing
{
	[TestedType(typeof(RegularProcessingServiceTask))]
	sealed class RegularProcessingServiceTaskTest : ServiceTaskTestCase<RegularProcessingServiceTask>
	{
		public void TestCanRunInAnyBranch()
		{
			var hostedServiceAttribute = GetHostedServiceAttributes().Single();
			Assert("CanRunInAnyBranch", hostedServiceAttribute.CanRunInAnyBranch);
		}

		[TestDate(2016, 8, 1, 8, 0, 0)]
		[ExpectNoExceptions]
		public void TestRunTask()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var caCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			caCompany1.GC_Code = "CA1";
			caCompany1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			caCompany1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Canada;
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AIR";
			caCompany1.GC_OH_OrgProxy = org1.PK;
			var branch1 = caCompany1.Branches.AddNew();
			branch1.GB_Code = "AAA";
			CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetValue(caCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, "111");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(caCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, "2222");
			CACustomsDataRegistry.Instance.TransmissionSite.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "333");

			var caCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			caCompany2.GC_Code = "CA2";
			caCompany2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			caCompany2.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Canada;
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "SEA";
			caCompany2.GC_OH_OrgProxy = org2.PK;
			var branch2 = caCompany2.Branches.AddNew();
			branch2.GB_Code = "BBB";

			var caInactiveCompany = Factory.NewWithValidTestData<GlbCompany>();
			caInactiveCompany.GC_Code = "CA3";
			caInactiveCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			caInactiveCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Canada;
			caInactiveCompany.GC_IsActive = false;
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "INA";
			caInactiveCompany.GC_OH_OrgProxy = org2.PK;
			var branch3 = caInactiveCompany.Branches.AddNew();
			branch3.GB_Code = "III";
			branch3.GB_IsActive = false;

			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.GC_Code = "USA";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			usCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var org4 = Factory.New<OrgHeader>();
			org4.OH_Code = "CHI";
			usCompany.GC_OH_OrgProxy = org4.PK;
			var branch4 = usCompany.Branches.AddNew();
			branch4.GB_Code = "DDD";

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntrySubmittedDate = new ZDateTime(2016, 8, 1, 6, 0, 0);
			declaration.JE_DateOfFirstArrival = new ZDateTime(2016, 7, 30);
			var entryNumber = declaration.AdditionalReferenceNumbers.AddNew();
			entryNumber.CE_EntryNum = "CCN123 456";
			entryNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			declaration.JE_GB = branch1.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = "REL";
			entryHeader.CH_Status = "AWO";
			entryHeader.CH_EntrySubmittedDate = new ZDateTime(2016, 8, 1);
			var message = entryHeader.Messages.AddNew();
			message.EM_ApplicationCode = "CAI";
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageType = "REL";
			message.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-3);
			Factory.Save();

			var logger = new TestServiceLogger();
			var task = new RegularProcessingServiceTask();
			task.ServiceLogger = logger;
			InitialiseTaskSchedule(task);

			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest(companyPk: caCompany1.PK.ToGuid()))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				RunTaskSchedule(task);
			}

			NUnit.Framework.Assert.That(logger.Count, NUnit.Framework.Is.EqualTo(5), "AIRS Validation Query executing for 2 Companies");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(logger[0], NUnit.Framework.Is.EqualTo("Information|Global Tariff count has not changed, Tariff Synchronization skip."));
				if (logger[1] == "Information|Regular Processing for Company CA1.")
				{
					NUnit.Framework.Assert.That(logger[2], NUnit.Framework.Is.EqualTo("Information|Accounting Age Calculation Finished for 0 Declaration(s)"));
					NUnit.Framework.Assert.That(logger[3], NUnit.Framework.Is.EqualTo(string.Format(CultureInfo.InvariantCulture, "Information|Set Exception Code 040 to declaration {0}.", declaration.JE_DeclarationReference)));
					NUnit.Framework.Assert.That(logger[4], NUnit.Framework.Is.EqualTo("Warning|Account Security No hasn't been setup for Company CA2. Please set it up in Registry -> Customs -> Country or Region Specific -> Canada -> Import -> Declaration -> ASEC Number"));
				}
				else
				{
					NUnit.Framework.Assert.That(logger[1], NUnit.Framework.Is.EqualTo("Warning|Account Security No hasn't been setup for Company CA2. Please set it up in Registry -> Customs -> Country or Region Specific -> Canada -> Import -> Declaration -> ASEC Number"));

					NUnit.Framework.Assert.That(logger[2], NUnit.Framework.Is.EqualTo("Information|Regular Processing for Company CA1."));
					NUnit.Framework.Assert.That(logger[3], NUnit.Framework.Is.EqualTo("Information|Accounting Age Calculation Finished for 0 Declaration(s)"));
					NUnit.Framework.Assert.That(logger[4], NUnit.Framework.Is.EqualTo(string.Format(CultureInfo.InvariantCulture, "Information|Set Exception Code 040 to declaration {0}.", declaration.JE_DeclarationReference)));
				}
			});
		}

		[TestDate(2022, 05, 05)]
		[ExpectNoExceptions]
		public void TestCalculateAccountingAge()
		{
			var company = GlbCompany.CurrentCompany;
			company.GC_Code = "CA~";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoiceLine1, 1, 1, 90m, 0, 0, 0, Core.Constants.Weight.Kilograms, 200m);
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today.AddDays(-10);
			declaration.CA_AccountingAge = ZInt.Zero;
			Factory.Save();

			var logger = new TestServiceLogger();
			var task = new RegularProcessingServiceTask();
			task.ServiceLogger = logger;
			InitialiseTaskSchedule(task);

			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest(companyPk: company.PK.ToGuid()))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				RunTaskSchedule(task);
			}

			CombineAssertions(() =>
			{
				if (logger[0] == "Information|Regular Processing for Company CA~.")
				{
					NUnit.Framework.Assert.That(logger[1], NUnit.Framework.Is.EqualTo("Information|Accounting Age Calculation Finished for 1 Declaration(s)"));
				}

				declaration.Reload();
				NUnit.Framework.Assert.That(declaration.CA_AccountingAge, NUnit.Framework.Is.EqualTo(8).Using(CustomComparers.TypeComparison));
			});
		}

		[TestDate(2024, 10, 14)]
		public void TestRefTariffSynchronize()
		{
			var company = GlbCompany.CurrentCompany;
			company.GC_Code = "CA~";

			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = "00000001";
			classHeader.ZA_EffectiveDate = ZDateTime.Today;
			classHeader.ZA_ExpiryDate = ZDateTime.MaxSmallDateTime;
			classHeader.ZA_AreaCode = "TTT";

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType1 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.Excise);
			var rateCode1 = universalHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRate.Codes.EXS, rateType1.PK);
			var rateType2 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.Duty);
			var rateCode2 = universalHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRate.Codes.DTY, rateType2.PK);
			var preference1 = universalHelper.CreatePreferenceForCountry("01", "Preference 01", Core.Constants.CountryCodes.Canada);
			var preference2 = universalHelper.CreatePreferenceForCountry("02", "Preference 02", Core.Constants.CountryCodes.Canada);

			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "00000001", ZDateTime.Today, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(tariff1, UOMTypeList.Codes.CU1, CustomsUnitOfMeasureList.Codes.MetricTon);
			universalHelper.CreateRate(tariff1, rateCode1.PK, ZDateTime.Today.AddDays(-5), ZDateTime.MaxSmallDateTime, "0.4*VFD", preference1.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			universalHelper.CreateRate(tariff1, rateCode2.PK, ZDateTime.Today, ZDateTime.MaxSmallDateTime, "MAX(1.65*VFD,2.94*[KGM])", preference1.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			universalHelper.CreateRate(tariff1, rateCode2.PK, ZDateTime.Today, ZDateTime.MaxSmallDateTime, "15.90*[TNE] + 0.07*VFD", preference2.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			Factory.Save();

			var logger = new TestServiceLogger();
			var task = new RegularProcessingServiceTask();
			task.ServiceLogger = logger;
			InitialiseTaskSchedule(task);

			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest(companyPk: company.PK.ToGuid()))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				RunTaskSchedule(task);
			}

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(logger[0], NUnit.Framework.Is.EqualTo("Information|Start Tariff Synchronization processing"));
				NUnit.Framework.Assert.That(logger[1], NUnit.Framework.Is.EqualTo("Information|Successfully synchronized 1 Tariff."));

				classHeader.Reload();
				NUnit.Framework.Assert.That(classHeader.ZA_AreaCode.ToString(), NUnit.Framework.Is.EqualTo("XXX"), "ZA_AreaCode");
				NUnit.Framework.Assert.That(classHeader.ZA_ExpiryDate, NUnit.Framework.Is.EqualTo(ZDateTime.MaxSmallDateTime), "ZA_ExpiryDate");
				NUnit.Framework.Assert.That((bool)classHeader.ZA_InactiveInd, NUnit.Framework.Is.EqualTo(false), "ZA_InactiveInd");
				NUnit.Framework.Assert.That((bool)classHeader.ZA_ExchangeDateDeterminationFlag, NUnit.Framework.Is.EqualTo(false), "ZA_ExchangeDateDeterminationFlag");
				NUnit.Framework.Assert.That((bool)classHeader.ZA_PermitInd, NUnit.Framework.Is.EqualTo(false), "ZA_PermitInd");
				NUnit.Framework.Assert.That((bool)classHeader.ZA_QuotaInd, NUnit.Framework.Is.EqualTo(false), "ZA_QuotaInd");
				AssertRateHeader(classHeader.ExciseDutyRates[0], ZDateTime.Today.AddDays(-5), ZDateTime.MaxSmallDateTime, false, ZString.Empty, false);
				AssertRate(classHeader.ExciseDutyRates[0].Rates[0], ZString.Empty, false);
				AssertRateLine(classHeader.ExciseDutyRates[0].Rates[0].RateLines[0], RateTypes.Codes.AdValorem, 40m, 0m, 0m);
				AssertRateHeader(classHeader.ClassRates[0], ZDateTime.Today, ZDateTime.MaxSmallDateTime, false, CustomsUnitOfMeasureList.Codes.MetricTon, false);
				var rate1 = classHeader.ClassRates[0].Rates.Cast<CACRate>().FirstOrDefault(r => r.ZC_TreatmentCode == "01");
				AssertRate(rate1, "01", false);
				AssertRateLine(rate1.RateLines.Cast<CACRateLine>().FirstOrDefault(r => r.ZR_DutyRateType == RateTypes.Codes.AdValorem), RateTypes.Codes.AdValorem, 165m, 0m, 0m);
				AssertRateLine(rate1.RateLines.Cast<CACRateLine>().FirstOrDefault(r => r.ZR_DutyRateType == RateTypes.Codes.Specific), RateTypes.Codes.Specific, 0m, 2.94m, 0m);
				var rate2 = classHeader.ClassRates[0].Rates.Cast<CACRate>().FirstOrDefault(r => r.ZC_TreatmentCode == "02");
				AssertRate(rate2, "02", false);
				AssertRateLine(rate2.RateLines.Cast<CACRateLine>().FirstOrDefault(r => r.ZR_DutyRateType == RateTypes.Codes.AdValorem), RateTypes.Codes.AdValorem, 7m, 0m, 0m);
				AssertRateLine(rate2.RateLines.Cast<CACRateLine>().FirstOrDefault(r => r.ZR_DutyRateType == RateTypes.Codes.Specific), RateTypes.Codes.Specific, 15.9m, 0m, 0m);
			});

			logger.ClearLog();
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest(companyPk: company.PK.ToGuid()))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				RunTaskSchedule(task);
			}
			NUnit.Framework.Assert.That(logger[0], NUnit.Framework.Is.EqualTo("Information|Global Tariff count has not changed, Tariff Synchronization skip."));
		}

		void AssertRateHeader(CACRateHeader classRateHeader, ZDateTime effectiveDate, ZDateTime expiryDate, bool free, string uom, bool inactive)
		{
			NUnit.Framework.Assert.That(classRateHeader.ZB_EffectiveDate, NUnit.Framework.Is.EqualTo(effectiveDate), "ZB_EffectiveDate");
			NUnit.Framework.Assert.That(classRateHeader.ZB_ExpiryDate, NUnit.Framework.Is.EqualTo(expiryDate), "ZB_ExpiryDate");
			NUnit.Framework.Assert.That((bool)classRateHeader.ZB_FreeInd, NUnit.Framework.Is.EqualTo(free), "ZB_FreeInd");
			NUnit.Framework.Assert.That(classRateHeader.ZB_UnitOfMeasure.ToString(), NUnit.Framework.Is.EqualTo(uom), "ZB_UnitOfMeasure");
			NUnit.Framework.Assert.That((bool)classRateHeader.ZB_Inactive, NUnit.Framework.Is.EqualTo(inactive), "ZB_Inactive");
		}

		void AssertRate(CACRate rate, string treatmentCode, bool isFree)
		{
			NUnit.Framework.Assert.That(rate.ZC_TreatmentCode.ToString(), NUnit.Framework.Is.EqualTo(treatmentCode), "ZC_TreatmentCode");
			NUnit.Framework.Assert.That((bool)rate.ZC_FreeInd, NUnit.Framework.Is.EqualTo(isFree), "ZC_FreeInd");
			NUnit.Framework.Assert.That((bool)rate.ZC_Inactive, NUnit.Framework.Is.EqualTo(false), "ZC_Inactive");
		}

		void AssertRateLine(CACRateLine line, string type, ZDecimal regular, ZDecimal min, ZDecimal max)
		{
			NUnit.Framework.Assert.That(line.ZR_DutyRateType.ToString(), NUnit.Framework.Is.EqualTo(type), "ZR_DutyRateType");
			NUnit.Framework.Assert.That(line.ZR_DutyRateRegular, NUnit.Framework.Is.EqualTo(regular), "ZR_DutyRateRegular");
			NUnit.Framework.Assert.That(line.ZR_DutyRateMin, NUnit.Framework.Is.EqualTo(min), "ZR_DutyRateMin");
			NUnit.Framework.Assert.That(line.ZR_DutyRateMax, NUnit.Framework.Is.EqualTo(max), "ZR_DutyRateMax");
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}

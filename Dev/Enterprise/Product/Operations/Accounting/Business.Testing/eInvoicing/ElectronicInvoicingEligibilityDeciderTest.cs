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
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Business.EInvoicing.ElectronicInvoicingEligibilityDecider;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Business.eInvoicing.Testing
{
	class ElectronicInvoicingEligibilityDeciderTest : TestCaseWithFactory
	{
		public void TestHasOriginalTransaction()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var arInvoice1 = testObjectCreator.CreateARInvoice<ARInvoice>("AR001", testObjectCreator.KRW, 1m, testObjectCreator.Debtor);
			var arInvoice2 = testObjectCreator.CreateARInvoice<ARInvoice>("AR002", testObjectCreator.KRW, 1m, testObjectCreator.Debtor);
			var arInvoice3 = testObjectCreator.CreateARInvoice<ARInvoice>("AR002", testObjectCreator.KRW, 1m, testObjectCreator.Debtor);
			var arCreditNote1 = Factory.NewWithValidTestData<ARCreditNote>();
			var arCreditNote2 = Factory.NewWithValidTestData<ARCreditNote>();
			var arCreditNote3 = Factory.NewWithValidTestData<ARCreditNote>();
			Factory.Save();

			AssertEquals(false, ElectronicInvoicingEligibilityDecider.CreateWrapper(arInvoice1).HasOriginalTransaction);
			AssertEquals(false, ElectronicInvoicingEligibilityDecider.CreateWrapper(arInvoice2).HasOriginalTransaction);
			AssertEquals(false, ElectronicInvoicingEligibilityDecider.CreateWrapper(arInvoice3).HasOriginalTransaction);
			AssertEquals(false, ElectronicInvoicingEligibilityDecider.CreateWrapper(arCreditNote1).HasOriginalTransaction);
			AssertEquals(false, ElectronicInvoicingEligibilityDecider.CreateWrapper(arCreditNote2).HasOriginalTransaction);
			AssertEquals(false, ElectronicInvoicingEligibilityDecider.CreateWrapper(arCreditNote3).HasOriginalTransaction);

			new ReversingFactory().NewReversing(arInvoice1).Reverse();
			var reverseTransactionForARInvoice1 = arInvoice1.ReverseInvoice;
			var amendTransactionForARInvoice2 = testObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arInvoice2).amendTransaction as InvoicingBase;
			var amendTransactionForARInvoice3 = testObjectCreator.AmendARTransaction(TransactionTypes.Invoice, arInvoice3).amendTransaction as InvoicingBase;

			AssertEquals(true, ElectronicInvoicingEligibilityDecider.CreateWrapper(reverseTransactionForARInvoice1).HasOriginalTransaction);
			AssertEquals(true, ElectronicInvoicingEligibilityDecider.CreateWrapper(amendTransactionForARInvoice2).HasOriginalTransaction);
			AssertEquals(true, ElectronicInvoicingEligibilityDecider.CreateWrapper(amendTransactionForARInvoice3).HasOriginalTransaction);

			new ReversingFactory().NewReversing(arCreditNote1).Reverse();
			var reverseTransactionForARCreditNote1 = arCreditNote1.ReverseInvoice;
			var amendTransactionForARCreditNote2 = testObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arCreditNote2).amendTransaction as InvoicingBase;
			var amendTransactionForARCreditNote3 = testObjectCreator.AmendARTransaction(TransactionTypes.Invoice, arCreditNote3).amendTransaction as InvoicingBase;

			AssertEquals(true, ElectronicInvoicingEligibilityDecider.CreateWrapper(reverseTransactionForARCreditNote1).HasOriginalTransaction);
			AssertEquals(true, ElectronicInvoicingEligibilityDecider.CreateWrapper(amendTransactionForARCreditNote2).HasOriginalTransaction);
			AssertEquals(true, ElectronicInvoicingEligibilityDecider.CreateWrapper(amendTransactionForARCreditNote3).HasOriginalTransaction);
		}

		public void TestIsEligibleTraceLog()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var arInvoice = testObjectCreator.CreateARInvoice<ARInvoice>("AR001", testObjectCreator.KRW, 1m, testObjectCreator.Debtor);
			Factory.Save();

			var dummyTracer = new DummyTracer();
			ObjectFactory.Substitute<ITracer>(dummyTracer);

			ElectronicInvoicingEligibilityDecider.IsEligible(arInvoice);
			AssertEquals("AU eInvoicing Eligibility for AR INV 00001000:", dummyTracer.Traces[0]);
		}

		public void TestAllEInvoicingCountriesSupportEligibility()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			{
				var supportedCountryCodes = ObjectFactory.Get<IGlobalEInvoicingObjectFactory>().SupportedCountryCodes;
				var countryCodesImplementingEligibility = new List<ZString>();

				foreach (var countryCode in Country.LicenceKeyBuilderSupportedCountryCodes)
				{
					if (ElectronicInvoicingEligibilityDecider.IsSupportedCountry(countryCode))
					{
						countryCodesImplementingEligibility.Add(countryCode);
					}
				}

				var countryCodesNotImplementingEligibility = supportedCountryCodes.Except(countryCodesImplementingEligibility);
				Assert("All supported e-Invoicing countries should implement eligibility, but the following do not: " + string.Join(",", countryCodesNotImplementingEligibility), !countryCodesNotImplementingEligibility.Any());
			}
		}

		public void TestEligibleTransactionHeaderItaly_Receivables()
		{
			var italianCompany = Factory.NewWithValidTestData<GlbCompany>();
			italianCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Italy;

			var testConfigurations = new[]
			{
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.AdjustmentNote },
				new { Expected = false, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice },
				new { Expected = false, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote },
				new { Expected = false, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.AdjustmentNote },
			};

			foreach (var testConfigurationEnumerated in Enumerable.Range(1, testConfigurations.Length).Zip(testConfigurations, (index, testConfiguration) => new { index, testConfiguration }))
			{
				var index = testConfigurationEnumerated.index;
				var testConfiguration = testConfigurationEnumerated.testConfiguration;
				var actual = ElectronicInvoicingEligibilityDecider.IsEligible(new TestTransactionHeaderWrapper(Constants.CountryCodes.Italy, testConfiguration.LedgerType, testConfiguration.TransactionType, companyPK: italianCompany.PK));
				AssertEquals($"REC#{index} {nameof(Constants.CountryCodes.Italy)} {testConfiguration.LedgerType} {testConfiguration.TransactionType}", testConfiguration.Expected, actual);
			}
		}

		public void TestEligibleTransactionHeaderItaly_Payables()
		{
			var italianCompany = Factory.NewWithValidTestData<GlbCompany>();
			italianCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Italy;

			var rvsLine = new { IsRvs = true };
			var notRvsLine = new { IsRvs = false };

			var withoutAnyRvs = new[] { notRvsLine, notRvsLine };
			var withAtLeastOneRvs = new[] { notRvsLine, rvsLine };
			var testConfigurations_APEinvoicingEnabled = new[]
			{
						new { Expected = false, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, Lines = withoutAnyRvs },
						new { Expected = false, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, Lines = withoutAnyRvs },
						new { Expected = false, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.AdjustmentNote, Lines = withoutAnyRvs },
						new { Expected = true, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, Lines = withAtLeastOneRvs },
						new { Expected = true, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, Lines = withAtLeastOneRvs },
						new { Expected = true, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.AdjustmentNote, Lines = withAtLeastOneRvs },
					};

			foreach (var testConfigurationEnumerated in Enumerable.Range(1, testConfigurations_APEinvoicingEnabled.Length).Zip(testConfigurations_APEinvoicingEnabled, (index, testConfiguration) => new { index, testConfiguration }))
			{
				var index = testConfigurationEnumerated.index;
				var testConfiguration = testConfigurationEnumerated.testConfiguration;
				var actual = ElectronicInvoicingEligibilityDecider.IsEligible(new TestTransactionHeaderWrapper(Constants.CountryCodes.Italy, testConfiguration.LedgerType, testConfiguration.TransactionType, companyPK: italianCompany.PK, isRVSs: testConfiguration.Lines.Select(x => x.IsRvs)));
				AssertEquals($"REC#{index} {nameof(Constants.CountryCodes.Italy)} {testConfiguration.LedgerType} {testConfiguration.TransactionType}", testConfiguration.Expected, actual);
			}
		}

		public void TestEligibleTransactionHeaderMexico()
		{
			var testConfigurations = new[]
			{
				new { Expected = true, Country = Constants.CountryCodes.Mexico, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TXI },
				new { Expected = true, Country = Constants.CountryCodes.Mexico, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TXI },
				new { Expected = true, Country = Constants.CountryCodes.Mexico, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TCR },
				new { Expected = true, Country = Constants.CountryCodes.Mexico, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TCR },
				new { Expected = true, Country = Constants.CountryCodes.Mexico, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TDR },
				new { Expected = true, Country = Constants.CountryCodes.Mexico, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TDR },

				new { Expected = false, Country = Constants.CountryCodes.Mexico, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.XCL },
				new { Expected = false, Country = Constants.CountryCodes.Mexico, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.XCL },
				new { Expected = false, Country = Constants.CountryCodes.Mexico, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TXI },
				new { Expected = false, Country = Constants.CountryCodes.Mexico, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TXI },
				new { Expected = false, Country = Constants.CountryCodes.Mexico, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TCR },
				new { Expected = false, Country = Constants.CountryCodes.Mexico, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.XCL },
				new { Expected = false, Country = Constants.CountryCodes.Mexico, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.XCL },
				new { Expected = false, Country = Constants.CountryCodes.Mexico, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = MexicoComplianceInfo.ComplianceSubTypeCodes.TDR },
			};

			for (int index = 0; index < testConfigurations.Length; index++)
			{
				var testConfiguration = testConfigurations[index];
				var actual = ElectronicInvoicingEligibilityDecider.IsEligible(new TestTransactionHeaderWrapper(Constants.CountryCodes.Mexico, testConfiguration.LedgerType, testConfiguration.TransactionType, headerOrgCusCodeCountryCode: testConfiguration.Country, complianceSubType: testConfiguration.ComplianceSubType));
				AssertEquals($"REC#{index} {nameof(Constants.CountryCodes.Mexico)} {testConfiguration.LedgerType} {testConfiguration.TransactionType} {testConfiguration.Country} {testConfiguration.ComplianceSubType}", testConfiguration.Expected, actual);
			}
		}

		public void TestEligibleTransactionHeaderTaxCoreCountries()
		{
			foreach (var countryCode in TaxCoreCountryHelper.GetTaxCoreSupportedCountries())
			{
				var reportableLine = new { IsNotReportable = false };
				var notReportableLine = new { IsNotReportable = true };

				var reportableLinesOnly = new[] { reportableLine, reportableLine };
				var notReportableLinesOnly = new[] { notReportableLine, notReportableLine };
				var mixedReportableAndNotReportableLines = new[] { notReportableLine, reportableLine };
				var noLines = reportableLinesOnly.Where(x => x.IsNotReportable).ToArray();

				var testConfigurations = new[]
				{
					new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, IsDisbursementCalc = false, Lines = reportableLinesOnly },
					new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, IsDisbursementCalc = false , Lines = reportableLinesOnly },
					new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, IsDisbursementCalc = false, Lines = mixedReportableAndNotReportableLines },
					new { Expected = false, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, IsDisbursementCalc = false , Lines = reportableLinesOnly },
					new { Expected = false, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, IsDisbursementCalc = false , Lines = reportableLinesOnly },
					new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.AdjustmentNote, IsDisbursementCalc = false , Lines = reportableLinesOnly },
					new { Expected = false, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.AdjustmentNote, IsDisbursementCalc = false , Lines = reportableLinesOnly },
					new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, IsDisbursementCalc = true, Lines = reportableLinesOnly },
					new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, IsDisbursementCalc = true , Lines = reportableLinesOnly },
					new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, IsDisbursementCalc = false , Lines = notReportableLinesOnly },
					new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, IsDisbursementCalc = false , Lines = noLines },
				};

				foreach (var testConfigurationEnumerated in Enumerable.Range(1, testConfigurations.Length).Zip(testConfigurations, (index, testConfiguration) => new { index, testConfiguration }))
				{
					var index = testConfigurationEnumerated.index;
					var testConfiguration = testConfigurationEnumerated.testConfiguration;
					var actual = ElectronicInvoicingEligibilityDecider.IsEligible(new TestTransactionHeaderWrapper(countryCode, testConfiguration.LedgerType, testConfiguration.TransactionType, testConfiguration.IsDisbursementCalc, isNotReportables: testConfiguration.Lines.Select(x => x.IsNotReportable)));
					AssertEquals($"REC#{index} {nameof(countryCode)} {testConfiguration.LedgerType} {testConfiguration.TransactionType} {(testConfiguration.IsDisbursementCalc ? "IsDisbursementCalc" : "NOT-IsDisbursementCalc")} {string.Join("/", testConfiguration.Lines.Select(x => x.IsNotReportable))}", testConfiguration.Expected, actual);
				}
			}
		}

		public void TestEligibleComplianceDocumentHeaderTaiwan()
		{
			var testConfigurations = new[]
			{
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC },
			};

			foreach (var testConfigurationEnumerated in Enumerable.Range(1, testConfigurations.Length).Zip(testConfigurations, (index, testConfiguration) => new { index, testConfiguration }))
			{
				var index = testConfigurationEnumerated.index;
				var testConfiguration = testConfigurationEnumerated.testConfiguration;
				var actual = ElectronicInvoicingEligibilityDecider.IsEligible(new TestComplianceDocumentHeaderWrapper(Constants.CountryCodes.Taiwan, testConfiguration.LedgerType, testConfiguration.TransactionType, testConfiguration.ComplianceSubType));
				AssertEquals($"REC#{index} {nameof(Constants.CountryCodes.Taiwan)} {testConfiguration.LedgerType} {testConfiguration.TransactionType}", testConfiguration.Expected, actual);
			}
		}

		public void TestEligibleTransactionHeader_SupportsLiteInterface_WhenDeciderIsImplemented_AndReturnsTrue()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();

			AssertSupportsLiteInterface(transaction);
			AssertSupportsLiteInterface(arInvoice);

			void AssertSupportsLiteInterface(IEInvoicingEligibilityLiteTransaction header)
			{
				var globalFactory = new Mock<IAccountingCountryFactory>()
					.WithEInvoicingEligibilityDecider(isEligible: true, out var eligibilityDecider)
					.ToGlobalFactory();
				using (ObjectFactory.Substitute(globalFactory.Object))
				{
					Assert(ElectronicInvoicingEligibilityDecider.IsEligible(header));
					globalFactory.Verify(c => c.GetCountryFactory(header.CountryCode), Times.Once);
					eligibilityDecider.Verify(x => x.IsTransactionEligible(header), Times.Once);
				}
			}
		}

		public void TestEligibleTransactionHeader_SupportsLiteInterface_WhenDeciderIsImplemented_AndReturnsFalse()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();

			AssertSupportsLiteInterface(transaction);
			AssertSupportsLiteInterface(arInvoice);

			void AssertSupportsLiteInterface(IEInvoicingEligibilityLiteTransaction header)
			{
				var globalFactory = new Mock<IAccountingCountryFactory>()
					.WithEInvoicingEligibilityDecider(isEligible: false, out var eligibilityDecider)
					.ToGlobalFactory();
				using (ObjectFactory.Substitute(globalFactory.Object))
				{
					Assert(!ElectronicInvoicingEligibilityDecider.IsEligible(header));
					globalFactory.Verify(c => c.GetCountryFactory(header.CountryCode), Times.Once);
					eligibilityDecider.Verify(x => x.IsTransactionEligible(header), Times.Once);
				}
			}
		}

		public void TestEligibleTransactionHeader_SupportsLiteInterface_WhenDeciderIsNotImplemented()
		{
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();

			AssertSupportsLiteInterface(transaction);
			AssertSupportsLiteInterface(arInvoice);

			void AssertSupportsLiteInterface(IEInvoicingEligibilityLiteTransaction header)
			{
				var countryFactory = new Mock<IAccountingCountryFactory>(MockBehavior.Strict);
				var globalFactory = countryFactory.ToGlobalFactory();
				using (ObjectFactory.Substitute(globalFactory.Object))
				{
					Assert(!ElectronicInvoicingEligibilityDecider.IsEligible(header));
					globalFactory.Verify(c => c.GetCountryFactory(header.CountryCode), Times.Once);
					countryFactory.Verify();
				}
			}
		}

		public void TestEligibleTransactionHeader_SupportsLiteInterface_ComplexDecider()
		{
			var eligibilityDecider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.VietNam) as IInstanceProvider<IEInvoicingEligibilityDecider>;
			AssertNull("Precondition: VietNam does not support lite eligibility", eligibilityDecider);

			var vietnamCompany = Factory.NewWithValidTestData<GlbCompany>();
			vietnamCompany.GC_RN_NKCountryCode = Constants.CountryCodes.VietNam;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			transaction.AH_TransactionReference = "ABC123";
			transaction.AH_GC = vietnamCompany.PK;
			transaction.AH_OH = orgHeader.PK;

			Assert("VietNam should not support lite eligibility which is only implemented on AccTransactionHeader", !ElectronicInvoicingEligibilityDecider.IsEligible(transaction));

			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			arInvoice.AH_GC = vietnamCompany.PK;
			arInvoice.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			arInvoice.AH_TransactionReference = "ABC123";
			arInvoice.AH_OH = orgHeader.PK;

			Assert("VietNam eligibility should work with a real ARInvoice business object", ElectronicInvoicingEligibilityDecider.IsEligible(arInvoice));
			var asLiteInterface = (IEInvoicingEligibilityLiteTransaction)arInvoice;
			Assert("VietNam eligibility should work with a real ARInvoice business object when cast to lite interface", ElectronicInvoicingEligibilityDecider.IsEligible(asLiteInterface));
		}

		#region Eligible Transaction Header Turkey

		public void TestEligibleTransactionHeaderTurkey()
		{
			AssertEligibleTransactionHeaderTurkey_Payables();
			AssertEligibleTransactionHeaderTurkey_Receivables();
			AssertEligibleTransactionHeaderTurkey_DifferentCountry();
			AssertEligibleTransactionHeaderTurkey_ReturnOfPurchases();
			AssertEligibleTransactionHeaderTurkey_ReturnOfSales();
			AssertEligibleTransactionHeaderTurkey_TransationsPendingAllocation();
		}
		void AssertEligibleTransactionHeaderTurkey_Payables()
		{
			var testConfigurations = new[]
			{
				new { Expected = false, vatNumber = "",   TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PAR, TransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "01", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PAR, TransactionReference = ""                 },
				new { Expected = false, vatNumber = "02", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PAR, TransactionReference = "ComplianceNumber" },

				new { Expected = false, vatNumber = "",   TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN, TransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "03", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN, TransactionReference = ""                 },
				new { Expected = false, vatNumber = "04", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN, TransactionReference = "ComplianceNumber" },

				new { Expected = false, vatNumber = "07", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = string.Empty,                                    TransactionReference = ""                 },

				new { Expected = false, vatNumber = "08", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PCL, TransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "09", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PCL, TransactionReference = "ComplianceNumber" },
			};

			for (int index = 0; index < testConfigurations.Length; index++)
			{
				var testConfiguration = testConfigurations[index];
				var actual = ElectronicInvoicingEligibilityDecider.IsEligible(
					new TestTransactionHeaderWrapper(
						CountryCodes.Turkey,
						LedgerTypes.AccountsPayable,
						testConfiguration.TransactionType,
						headerOrgCusCodeCountryCode: CountryCodes.Turkey,
						headerOrgCusCodeNumber: testConfiguration.vatNumber,
						complianceSubType: testConfiguration.ComplianceSubType,
						transactionReference: testConfiguration.TransactionReference));
				AssertEquals($"REC#{index} {testConfiguration.TransactionType} {testConfiguration.vatNumber} {testConfiguration.ComplianceSubType} '{testConfiguration.TransactionReference}'", testConfiguration.Expected, actual);
			}
		}

		void AssertEligibleTransactionHeaderTurkey_Receivables()
		{
			var testConfigurations = new[]
			{
				new { Expected = true,  vatNumber = "1234", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = ""                , OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "",     TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = true,  vatNumber = "1234", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = ""                , OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "",     TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = true,  vatNumber = "1234", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIC, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIC, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = ""                , OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "",     TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIC, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.XCL, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = ""                , OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = "",                                              OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = ""                , OriginalTransactionReference = "ComplianceNumber" },

				new { Expected = true,  vatNumber = "1234", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN, OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, TransactionReference = "ComplianceNumber", OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN, OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, TransactionReference = ""                , OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "",     TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN, OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, TransactionReference = "ComplianceNumber", OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN, OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN, TransactionReference = "ComplianceNumber", OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN, OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIC, TransactionReference = "ComplianceNumber", OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.XCL, OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, TransactionReference = "ComplianceNumber", OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = "",                                              OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, TransactionReference = ""                , OriginalTransactionReference = "ComplianceNumber" },

				new { Expected = true,  vatNumber = "1234", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = ""                 },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = ""                , OriginalTransactionReference = ""                 },
				new { Expected = false, vatNumber = "",     TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = ""                 },
				new { Expected = true,  vatNumber = "1234", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = ""                 },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = ""                , OriginalTransactionReference = ""                 },
				new { Expected = false, vatNumber = "",     TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = ""                 },
				new { Expected = true,  vatNumber = "1234", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIC, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = ""                 },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIC, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = ""                , OriginalTransactionReference = ""                 },
				new { Expected = false, vatNumber = "",     TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIC, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = ""                 },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.XCL, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = ""                , OriginalTransactionReference = ""                 },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = "",                                              OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = ""                , OriginalTransactionReference = ""                 },

				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN, OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, TransactionReference = "ComplianceNumber", OriginalTransactionReference = ""                 },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN, OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, TransactionReference = ""                , OriginalTransactionReference = ""                 },
				new { Expected = false, vatNumber = "",     TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN, OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, TransactionReference = "ComplianceNumber", OriginalTransactionReference = ""                 },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN, OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN, TransactionReference = "ComplianceNumber", OriginalTransactionReference = ""                 },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN, OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIC, TransactionReference = "ComplianceNumber", OriginalTransactionReference = ""                 },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.XCL, OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, TransactionReference = "ComplianceNumber", OriginalTransactionReference = ""                 },
				new { Expected = false, vatNumber = "1234", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = "",                                              OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, TransactionReference = ""                , OriginalTransactionReference = ""                 },
			};

			for (int index = 0; index < testConfigurations.Length; index++)
			{
				var testConfiguration = testConfigurations[index];
				var actual = ElectronicInvoicingEligibilityDecider.IsEligible(
					new TestTransactionHeaderWrapper(
						CountryCodes.Turkey,
						LedgerTypes.AccountsReceivable,
						testConfiguration.TransactionType,
						headerOrgCusCodeCountryCode: CountryCodes.Turkey,
						headerOrgCusCodeNumber: testConfiguration.vatNumber,
						complianceSubType: testConfiguration.ComplianceSubType,
						transactionReference: testConfiguration.TransactionReference,
						originalTransactionComplianceSubType: testConfiguration.OriginalTransactionComplianceSubType,
						originalTransactionReference: testConfiguration.OriginalTransactionReference));
				AssertEquals($"REC#{index} {nameof(Constants.CountryCodes.Turkey)} {testConfiguration.TransactionType} {testConfiguration.vatNumber} {testConfiguration.ComplianceSubType} {testConfiguration.OriginalTransactionComplianceSubType} '{testConfiguration.TransactionReference}' '{testConfiguration.OriginalTransactionReference}'", testConfiguration.Expected, actual);
			}
		}

		void AssertEligibleTransactionHeaderTurkey_DifferentCountry()
		{
			var testConfigurations = new[]
			{
				new { Expected = false, vatNumber = "1234", LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "1234", LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "1234", LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIC, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "ComplianceNumber" },

				new { Expected = false, vatNumber = "1234", LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN, OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, TransactionReference = "ComplianceNumber", OriginalTransactionReference = "ComplianceNumber" },

				new { Expected = false, vatNumber = "1234", LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = ""                 },
				new { Expected = false, vatNumber = "1234", LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = ""                 },
				new { Expected = false, vatNumber = "1234", LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIC, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = ""                 },

				new { Expected = false, vatNumber = "1234", LedgerType = LedgerTypes.AccountsPayable,    TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DAR, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "" },
				new { Expected = false, vatNumber = "1234", LedgerType = LedgerTypes.AccountsPayable,    TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "" },
				new { Expected = false, vatNumber = "1234", LedgerType = LedgerTypes.AccountsPayable,    TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DCN, OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DAR, TransactionReference = "ComplianceNumber", OriginalTransactionReference = "ComplianceNumber" },

				new { Expected = false, vatNumber = "1234", LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.CCN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "" },
			};

			for (int index = 0; index < testConfigurations.Length; index++)
			{
				var testConfiguration = testConfigurations[index];
				var actual = ElectronicInvoicingEligibilityDecider.IsEligible(
					new TestTransactionHeaderWrapper(
						CountryCodes.Turkey,
						testConfiguration.LedgerType,
						testConfiguration.TransactionType,
						headerOrgCusCodeCountryCode: CountryCodes.India,
						headerOrgCusCodeNumber: testConfiguration.vatNumber,
						complianceSubType: testConfiguration.ComplianceSubType,
						transactionReference: testConfiguration.TransactionReference,
						originalTransactionComplianceSubType: testConfiguration.OriginalTransactionComplianceSubType,
						originalTransactionReference: testConfiguration.OriginalTransactionReference));
				AssertEquals($"REC#{index} {nameof(Constants.CountryCodes.Turkey)} {testConfiguration.LedgerType} {testConfiguration.TransactionType} {testConfiguration.vatNumber} {testConfiguration.ComplianceSubType} {testConfiguration.OriginalTransactionComplianceSubType} '{testConfiguration.TransactionReference}' '{testConfiguration.OriginalTransactionReference}'", testConfiguration.Expected, actual);
			}
		}

		void AssertEligibleTransactionHeaderTurkey_ReturnOfPurchases()
		{
			var testConfigurations = new[]
			{
				new { Expected = false, vatNumber = "01", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DAR, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = ""                , OriginalTransactionReference = "" },
				new { Expected = false, vatNumber = "",   TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DAR, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "" },
				new { Expected = true,  vatNumber = "02", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DAR, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "" },

				new { Expected = false, vatNumber = "03", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = ""                , OriginalTransactionReference = "" },
				new { Expected = false, vatNumber = "",   TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "" },
				new { Expected = true,  vatNumber = "04", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "" },

				new { Expected = false, vatNumber = "",   TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DCN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = ""                , OriginalTransactionReference = "" },
				new { Expected = false, vatNumber = "",   TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DCN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "" },
				new { Expected = false, vatNumber = "05", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DCN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = ""                , OriginalTransactionReference = "" },
				new { Expected = false, vatNumber = "06", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DCN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "" },

				new { Expected = false, vatNumber = "",   TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DCN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "" },
				new { Expected = false, vatNumber = "07", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DCN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "" },
				new { Expected = false, vatNumber = "08", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DCN, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = "ComplianceNumber", OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "09", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DCN, OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DAR, TransactionReference = "ComplianceNumber", OriginalTransactionReference = "" },
				new { Expected = true,  vatNumber = "10", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DCN, OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DAR, TransactionReference = "ComplianceNumber", OriginalTransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "11", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DCN, OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN, TransactionReference = "ComplianceNumber", OriginalTransactionReference = "" },
				new { Expected = false, vatNumber = "12", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DCN, OriginalTransactionComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN, TransactionReference = "ComplianceNumber", OriginalTransactionReference = "ComplianceNumber" },

				new { Expected = false, vatNumber = "13", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DCL, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = ""                , OriginalTransactionReference = "" },
				new { Expected = false, vatNumber = "14", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DCL, OriginalTransactionComplianceSubType = string.Empty,                                    TransactionReference = ""                , OriginalTransactionReference = "" },
			};

			for (int index = 0; index < testConfigurations.Length; index++)
			{
				var testConfiguration = testConfigurations[index];
				var actual = ElectronicInvoicingEligibilityDecider.IsEligible(
					new TestTransactionHeaderWrapper(
						CountryCodes.Turkey,
						LedgerTypes.AccountsPayable,
						testConfiguration.TransactionType,
						headerOrgCusCodeCountryCode: CountryCodes.Turkey,
						headerOrgCusCodeNumber: testConfiguration.vatNumber,
						complianceSubType: testConfiguration.ComplianceSubType,
						transactionReference: testConfiguration.TransactionReference,
						originalTransactionComplianceSubType: testConfiguration.OriginalTransactionComplianceSubType,
						originalTransactionReference: testConfiguration.OriginalTransactionReference));
				AssertEquals($"REC#{index} {testConfiguration.TransactionType} {testConfiguration.vatNumber} {testConfiguration.ComplianceSubType} {testConfiguration.OriginalTransactionComplianceSubType} '{testConfiguration.TransactionReference}' '{testConfiguration.OriginalTransactionReference}'", testConfiguration.Expected, actual);
			}
		}

		void AssertEligibleTransactionHeaderTurkey_ReturnOfSales()
		{
			var testConfigurations = new[]
			{
				new { Expected = false, vatNumber = "",   TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.CAR, TransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "01", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.CAR, TransactionReference = ""                 },
				new { Expected = false, vatNumber = "02", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.CAR, TransactionReference = "ComplianceNumber" },

				new { Expected = false, vatNumber = "",   TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.CIN, TransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "03", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.CIN, TransactionReference = ""                 },
				new { Expected = false, vatNumber = "04", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.CIN, TransactionReference = "ComplianceNumber" },

				new { Expected = false, vatNumber = "",   TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.CCN, TransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "05", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.CCN, TransactionReference = ""                 },
				new { Expected = true,  vatNumber = "06", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.CCN, TransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "07", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = string.Empty,                                    TransactionReference = ""                 },

				new { Expected = false, vatNumber = "08", TransactionType = TransactionTypes.Invoice,    ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.CCL, TransactionReference = "ComplianceNumber" },
				new { Expected = false, vatNumber = "09", TransactionType = TransactionTypes.CreditNote, ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.CCL, TransactionReference = "ComplianceNumber" },
			};

			for (int index = 0; index < testConfigurations.Length; index++)
			{
				var testConfiguration = testConfigurations[index];
				var actual = ElectronicInvoicingEligibilityDecider.IsEligible(
					new TestTransactionHeaderWrapper(
						CountryCodes.Turkey,
						LedgerTypes.AccountsPayable,
						testConfiguration.TransactionType,
						headerOrgCusCodeCountryCode: CountryCodes.Turkey,
						headerOrgCusCodeNumber: testConfiguration.vatNumber,
						complianceSubType: testConfiguration.ComplianceSubType,
						transactionReference: testConfiguration.TransactionReference));
				AssertEquals($"REC#{index} {testConfiguration.TransactionType} {testConfiguration.vatNumber} {testConfiguration.ComplianceSubType} '{testConfiguration.TransactionReference}'", testConfiguration.Expected, actual);
			}
		}

		void AssertEligibleTransactionHeaderTurkey_TransationsPendingAllocation()
		{
			var testConfigurations = new[]
			{
				new { Expected = true , GovernmentAllocatedID = "ID" },
				new { Expected = false, GovernmentAllocatedID = "" },
			};

			for (int index = 0; index < testConfigurations.Length; index++)
			{
				var testConfiguration = testConfigurations[index];
				var actual = ElectronicInvoicingEligibilityDecider.IsEligible(
					new TestTransactionHeaderWrapper(
						CountryCodes.Turkey,
						LedgerTypes.TransactionsPendingAllocation,
						TransactionTypes.InvoicePendingAllocation,
						headerOrgCusCodeCountryCode: CountryCodes.Turkey,
						headerOrgCusCodeNumber: "1234",
						complianceSubType: string.Empty,
						transactionReference: string.Empty,
						originalTransactionComplianceSubType: string.Empty,
						originalTransactionReference: string.Empty,
						governmentAllocatedID: testConfiguration.GovernmentAllocatedID));
				AssertEquals($"REC#{index} '{testConfiguration.GovernmentAllocatedID}'", testConfiguration.Expected, actual);
			}
		}

		#endregion Eligible Transaction Header Turkey

		public void TestEligibleTransactionHeaderArgentina()
		{
			var testConfigurations = new[]
			{
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXB },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXC },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCA },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCE },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCB },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCC },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDA },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDE },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDB },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDC },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXA },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDA },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCA },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXB },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDB },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXC },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDC },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCC },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXM },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDM },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCM },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.BOL },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.XCL },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.XCL },

				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCB },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCB },
				new { Expected = false, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCB },
				new { Expected = false, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCB },
			};

			for (int index = 0; index < testConfigurations.Length; index++)
			{
				var testConfiguration = testConfigurations[index];
				var actual = ElectronicInvoicingEligibilityDecider.IsEligible(new TestTransactionHeaderWrapper(Constants.CountryCodes.Argentina, testConfiguration.LedgerType, testConfiguration.TransactionType,
							complianceSubType: testConfiguration.ComplianceSubType));
				AssertEquals($"REC#{index} {nameof(Constants.CountryCodes.Argentina)} {testConfiguration.LedgerType} {testConfiguration.TransactionType} {testConfiguration.ComplianceSubType}", testConfiguration.Expected, actual);
			}
		}

		[TestDate(2025, 5, 1)]
		public void TestEligibleTransactionHeaderVietnam()
		{
			var testConfigurations = new[]
			{
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.EXI, TransactionReference = "AA12345" },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI, TransactionReference = "AA12345" },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.EXI, TransactionReference = "AA12345" },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI, TransactionReference = "AA12345" },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.EXI, TransactionReference = string.Empty },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI, TransactionReference = string.Empty },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.EXI, TransactionReference = string.Empty },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI, TransactionReference = string.Empty },
				new { Expected = false, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.EXI, TransactionReference = string.Empty },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.AdjustmentNote, ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI, TransactionReference = string.Empty },
			};

			for (int index = 0; index < testConfigurations.Length; index++)
			{
				var testConfiguration = testConfigurations[index];
				var actual = ElectronicInvoicingEligibilityDecider.IsEligible(new TestTransactionHeaderWrapper(Constants.CountryCodes.VietNam, testConfiguration.LedgerType, testConfiguration.TransactionType,
							complianceSubType: testConfiguration.ComplianceSubType, transactionReference: testConfiguration.TransactionReference));
				AssertEquals($"REC#{index} {nameof(Constants.CountryCodes.VietNam)} {testConfiguration.LedgerType} {testConfiguration.TransactionType} {testConfiguration.ComplianceSubType} {testConfiguration.TransactionReference}", testConfiguration.Expected, actual);
			}

			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			{
				var testObjectCreator = new TestObjectCreator(Factory);
				var arInvoice = testObjectCreator.CreateARInvoice<ARInvoice>("0001", testObjectCreator.VND, 1m, testObjectCreator.AALSHI);
				arInvoice.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				arInvoice.AH_TransactionReference = "123";
				Factory.Save();
				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
				AssertNotNull(pivot);
				AssertEquals(Constants.EInvoicingPivotActionType.Submit, pivot.AIP_ActionType);

				pivot.AIP_Status = Constants.EInvoicingPivotState.Succeed;
				var creditNote = (ARCreditNote)testObjectCreator.ReverseTransaction(arInvoice, out _);
				Factory.Save();

				pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, creditNote.PK));
				AssertNotNull(pivot);
				AssertEquals(Constants.EInvoicingPivotActionType.Cancel, pivot.AIP_ActionType);
			}
		}

		public void TestEligibleTransactionHeaderVietnam_ForAmendingTransaction()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var job = testObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "charge1", testObjectCreator.VND, 10m, testObjectCreator.Creditor1, testObjectCreator.VND, 10m, testObjectCreator.Debtor1);
				Factory.Save();

				var arInvoice = (ARInvoice)testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV123456", testObjectCreator.VND, 1m, 10m, 0M, 10m, 0m, testObjectCreator.Debtor1, testObjectCreator.CC1.PK);
				arInvoice.Lines[0].AL_AT = charge.JR_AT_SellGSTRate;
				arInvoice.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
				arInvoice.AH_TransactionReference = "ABC123";
				var pivot = testObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, status: "SUC");
				pivot.AIP_LastSentTimeUtc = ZDateTime.Now;
				pivot.AIP_LastResponseReceivedUtc = ZDateTime.Now;
				var batch = testObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, "SNT");
				batch.TransactionPivots.Add(pivot);
				Factory.Save();

				var arCreditNote = testObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arInvoice).amendTransaction as ARCreditNote;
				Factory.Save();
				AssertNullOrEmpty("Precondition: Amending CRD has no compliance number", arCreditNote.AH_TransactionReference);
				AssertNull("Precondition: Amending CRD is not eligible therefore has no pivot", arCreditNote.GetMostRecentEInvoicingTransactionPivot());

				var newFactory = Factory.CreateNewFactory();
				var arCreditNoteInNewFactory = newFactory.Load<ARCreditNote>(arCreditNote.PK);
				arCreditNoteInNewFactory.AH_TransactionReference = "ABC456";
				newFactory.Save();
				AssertNotNull("Amending CRD is eligible after compliance number is allocated therefore has a pivot", arCreditNoteInNewFactory.GetMostRecentEInvoicingTransactionPivot());
				AssertEquals("Amending CRD pivot action type should be AJD", "ADJ", arCreditNoteInNewFactory.GetMostRecentEInvoicingTransactionPivot().AIP_ActionType);
			}
		}

		public void TestTransactionHeaderWrapper_ForAmendingAndReversalOfARInvoiceTransactionAsGovernmentInvoice()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var arInvoice1 = testObjectCreator.CreateARInvoice<ARInvoice>("0001", testObjectCreator.AUD, 1m, testObjectCreator.AALSHI, TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, "ABC1");
			var arInvoice2 = testObjectCreator.CreateARInvoice<ARInvoice>("0002", testObjectCreator.AUD, 1m, testObjectCreator.AALSHI, TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, "ABC2");
			var arInvoice3 = testObjectCreator.CreateARInvoice<ARInvoice>("0003", testObjectCreator.AUD, 1m, testObjectCreator.AALSHI, TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, "ABC3");
			AssertWarpperPropertiesOfTransaction("AR Invoice 1", "", arInvoice1, hasOriginalTransaction: false, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: false, assertGovernmentInvoice: false);
			AssertWarpperPropertiesOfTransaction("AR Invoice 2", "", arInvoice2, hasOriginalTransaction: false, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: false, assertGovernmentInvoice: false);
			AssertWarpperPropertiesOfTransaction("AR Invoice 3", "", arInvoice3, hasOriginalTransaction: false, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: false, assertGovernmentInvoice: false);
			Factory.Save();

			// Amending 
			var amendARInvoice1 = CreateAndAssertAmendARTransaction<ARInvoice>(testObjectCreator, "Amend AR Invoice 1 Pre Save", TransactionTypes.Invoice, "ABC4", arInvoice1, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: false);
			var amendARCreditNote1 = CreateAndAssertAmendARTransaction<ARCreditNote>(testObjectCreator, "Amend AR Credit Note 1 Pre Save", TransactionTypes.CreditNote, "ABC5", arInvoice1, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: true);
			var amendARInvoice2 = CreateAndAssertAmendARTransaction<ARInvoice>(testObjectCreator, "Amend AR Invoice 2 Pre Save", TransactionTypes.Invoice, "ABC6", arInvoice2, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: false);
			var amendARCreditNote2 = CreateAndAssertAmendARTransaction<ARCreditNote>(testObjectCreator, "Amend AR Credit Note 2 Pre Save", TransactionTypes.CreditNote, "ABC7", arInvoice2, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: true);
			Factory.Save();

			AssertWarpperPropertiesOfTransaction("Amend AR Invoice 1 Post Save", arInvoice1.AH_TransactionReference, amendARInvoice1, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: false);
			AssertWarpperPropertiesOfTransaction("Amend AR Credit Note 1 Post Save", arInvoice1.AH_TransactionReference, amendARCreditNote1, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: true);
			AssertWarpperPropertiesOfTransaction("Amend AR Invoice 2 Post Save", arInvoice2.AH_TransactionReference, amendARInvoice2, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: false);
			AssertWarpperPropertiesOfTransaction("Amend AR Credit Note 2 Post Save", arInvoice2.AH_TransactionReference, amendARCreditNote2, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: true);

			var reversalARCreditNoteOfAmendARInvoice1 = testObjectCreator.CreateReversalTransaction(amendARInvoice1, "ABC8") as ARCreditNote;
			var reversalARInvoiceOfAmendARCreditNote1 = testObjectCreator.CreateReversalTransaction(amendARCreditNote1, "ABC9") as ARInvoice;
			AssertWarpperPropertiesOfTransaction("Reversal AR Credit Note 1 of Amend AR Invoice 1 Pre Save", amendARInvoice1.AH_TransactionReference, reversalARCreditNoteOfAmendARInvoice1, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: true, isAmendingCreditNote: false, assertGovernmentInvoice: false);
			AssertWarpperPropertiesOfTransaction("Reversal AR Credit Note 1 of Amend AR Invoice 1 Pre Save", amendARCreditNote1.AH_TransactionReference, reversalARInvoiceOfAmendARCreditNote1, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: true, isAmendingCreditNote: false, assertGovernmentInvoice: false);
			Factory.Save();

			var newFactory1 = new BusinessObjectFactory();

			var amendARInvoice1NewFactory = newFactory1.Load<ARInvoice>(amendARInvoice1.PK);
			var amendARCreditNote1NewFactory = newFactory1.Load<ARCreditNote>(amendARCreditNote1.PK);
			var amendARInvoice2NewFactory = newFactory1.Load<ARInvoice>(amendARInvoice2.PK);
			var amendARCreditNote2NewFactory = newFactory1.Load<ARCreditNote>(amendARCreditNote2.PK);
			AssertWarpperPropertiesOfTransaction("Amend AR Invoice 1 Post Save and Post Reversal New Factory", arInvoice1.AH_TransactionReference, amendARInvoice1NewFactory, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: true, isAmendingCreditNote: false);
			AssertWarpperPropertiesOfTransaction("Amend AR Credit Note 1 Post Save and Post Reversal New Factory", arInvoice1.AH_TransactionReference, amendARCreditNote1NewFactory, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: true, isAmendingCreditNote: true);
			AssertWarpperPropertiesOfTransaction("Amend AR Invoice 2 Post Save and Post Reversal New Factory", arInvoice2.AH_TransactionReference, amendARInvoice2NewFactory, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: false);
			AssertWarpperPropertiesOfTransaction("Amend AR Credit Note 2 Post Save and Post Reversal New Factory", arInvoice2.AH_TransactionReference, amendARCreditNote2NewFactory, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: true);

			var reversalARCreditNoteOfAmendARInvoice1NewFactory = newFactory1.Load<ARCreditNote>(reversalARCreditNoteOfAmendARInvoice1.PK);
			var reversalARInvoiceOfAmendARCreditNote1NewFactory = newFactory1.Load<ARInvoice>(reversalARInvoiceOfAmendARCreditNote1.PK);
			AssertWarpperPropertiesOfTransaction("Reversal AR Credit Note of Amend AR Invoice 1 Post Save New Factory", amendARInvoice1.AH_TransactionReference, reversalARCreditNoteOfAmendARInvoice1NewFactory, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: true, isAmendingCreditNote: false);
			AssertWarpperPropertiesOfTransaction("Reversal AR Invoice of Amend AR Credit Note 1 Post Save New Factory", amendARCreditNote1.AH_TransactionReference, reversalARInvoiceOfAmendARCreditNote1NewFactory, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: true, isAmendingCreditNote: false);

			var reversalARCreditNote1 = testObjectCreator.CreateReversalTransaction(arInvoice1, "ABC10") as ARCreditNote;
			var reversalARCreditNote2 = testObjectCreator.CreateReversalTransaction(arInvoice2, "ABC11") as ARCreditNote;
			var reversalARCreditNote3 = testObjectCreator.CreateReversalTransaction(arInvoice3, "ABC12") as ARCreditNote;
			AssertWarpperPropertiesOfTransaction("Reversal AR Credit Note 1 of AR Invoice 1 Pre Save", arInvoice1.AH_TransactionReference, reversalARCreditNote1, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: true, isAmendingCreditNote: false, assertGovernmentInvoice: false);
			AssertWarpperPropertiesOfTransaction("Reversal AR Credit Note 2 of AR Invoice 2 Pre Save", arInvoice2.AH_TransactionReference, reversalARCreditNote2, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: true, isAmendingCreditNote: false, assertGovernmentInvoice: false);
			AssertWarpperPropertiesOfTransaction("Reversal AR Credit Note 3 of AR Invoice 3 Pre Save", arInvoice3.AH_TransactionReference, reversalARCreditNote3, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: true, isAmendingCreditNote: false, assertGovernmentInvoice: false);

			Factory.Save();

			var reversalARCreditNote1NewFactory = newFactory1.Load<ARCreditNote>(reversalARCreditNote1.PK);
			var reversalARCreditNote2NewFactory = newFactory1.Load<ARCreditNote>(reversalARCreditNote2.PK);
			var reversalARCreditNote3NewFactory = newFactory1.Load<ARCreditNote>(reversalARCreditNote3.PK);
			AssertWarpperPropertiesOfTransaction("Reversal AR Credit Note 1 of AR Invoice 1 Post Save New Factory", arInvoice1.AH_TransactionReference, reversalARCreditNote1NewFactory, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: true, isAmendingCreditNote: false);
			AssertWarpperPropertiesOfTransaction("Reversal AR Credit Note 2 of AR Invoice 2 Post Save New Factory", arInvoice2.AH_TransactionReference, reversalARCreditNote2NewFactory, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: true, isAmendingCreditNote: false);
			AssertWarpperPropertiesOfTransaction("Reversal AR Credit Note 3 of AR Invoice 3 Post Save New Factory", arInvoice3.AH_TransactionReference, reversalARCreditNote3NewFactory, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: true, isAmendingCreditNote: false);

			var arInvoice1NewFactory = newFactory1.Load<ARInvoice>(arInvoice1.PK);
			var arInvoice2NewFactory = newFactory1.Load<ARInvoice>(arInvoice2.PK);
			var arInvoice3NewFactory = newFactory1.Load<ARInvoice>(arInvoice3.PK);
			AssertWarpperPropertiesOfTransaction("AR Invoice 1 Post Reversal New Factory", "", arInvoice1NewFactory, hasOriginalTransaction: false, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: false);
			AssertWarpperPropertiesOfTransaction("AR Invoice 2 Post Reversal New Factory", "", arInvoice2NewFactory, hasOriginalTransaction: false, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: false);
			AssertWarpperPropertiesOfTransaction("AR Invoice 3 Post Reversal New Factory", "", arInvoice3NewFactory, hasOriginalTransaction: false, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: false);

			var newFactory2 = new BusinessObjectFactory();

			amendARInvoice1NewFactory = newFactory2.Load<ARInvoice>(amendARInvoice1.PK);
			amendARCreditNote1NewFactory = newFactory2.Load<ARCreditNote>(amendARCreditNote1.PK);
			amendARInvoice2NewFactory = newFactory2.Load<ARInvoice>(amendARInvoice2.PK);
			amendARCreditNote2NewFactory = newFactory2.Load<ARCreditNote>(amendARCreditNote2.PK);
			AssertWarpperPropertiesOfTransaction("Amend AR Invoice 1 Post Save and Post Reversal New Factory 2", arInvoice1.AH_TransactionReference, amendARInvoice1NewFactory, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: true, isAmendingCreditNote: false);
			AssertWarpperPropertiesOfTransaction("Amend AR Credit Note 1 Post Save and Post Reversal New Factory 2", arInvoice1.AH_TransactionReference, amendARCreditNote1NewFactory, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: true, isAmendingCreditNote: false);
			AssertWarpperPropertiesOfTransaction("Amend AR Invoice 2 Post Save and Post Reversal New Factory 2", arInvoice2.AH_TransactionReference, amendARInvoice2NewFactory, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: false);
			AssertWarpperPropertiesOfTransaction("Amend AR Credit Note 2 Post Save and Post Reversal New Factory 2", arInvoice2.AH_TransactionReference, amendARCreditNote2NewFactory, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: false);
		}

		T CreateAndAssertAmendARTransaction<T>(TestObjectCreator creator, string message, string transactionType, string reference, InvoicingBase baseTransaction, bool hasOriginalTransaction = false, bool isWritingOff = false, bool isReverseTransaction = false, bool isAmendingCreditNote = false)
		{
			var amendARInvoice = (T)creator.AmendARTransaction(transactionType, baseTransaction).amendTransaction;
			(amendARInvoice as InvoicingBase).AH_TransactionReference = reference;
			AssertWarpperPropertiesOfTransaction(message, baseTransaction.AH_TransactionReference, amendARInvoice, hasOriginalTransaction, isWritingOff, isReverseTransaction, isAmendingCreditNote, assertGovernmentInvoice: false);
			return amendARInvoice;
		}

		void AssertWarpperPropertiesOfTransaction<T>(string message, string originalTransactionRef, T amendTransaction, bool hasOriginalTransaction, bool isWritingOff, bool isReverseTransaction, bool isAmendingCreditNote, bool assertGovernmentInvoice = true)
		{
			var amendARInvoiceWrapper = ElectronicInvoicingEligibilityDecider.CreateWrapper(amendTransaction as TransactionHeader);
			AssertTransactionHeaderWrapper(message, originalTransactionRef, amendARInvoiceWrapper, hasOriginalTransaction, isWritingOff, isReverseTransaction, isAmendingCreditNote);
			if (assertGovernmentInvoice)
			{
				var invoicingBase = amendTransaction as InvoicingBase;
				AssertGovernmentInvoice(invoicingBase.Factory, invoicingBase.PK, message + " as Government Invoice", originalTransactionRef, hasOriginalTransaction, isWritingOff, isReverseTransaction, isAmendingCreditNote);
			}
		}

		public void TestTransactionHeaderWrapper_ForReversalOfARCreditNoteTransactionAsGovernmentInvoice()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			arCreditNote.AH_TransactionReference = "ABC1";
			AssertWarpperPropertiesOfTransaction("AR Credit Note", "", arCreditNote, hasOriginalTransaction: false, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: false, assertGovernmentInvoice: false);

			var badDebt = arCreditNote as IBadDebtWritingOff;
			badDebt.IsWritingOff = true;
			AssertWarpperPropertiesOfTransaction("AR Credit Note as Bad Dept", arCreditNote.AH_TransactionReference, arCreditNote, hasOriginalTransaction: false, isWritingOff: true, isReverseTransaction: false, isAmendingCreditNote: false, assertGovernmentInvoice: false);

			Factory.Save();

			var reversalARInvoice = testObjectCreator.CreateReversalTransaction(arCreditNote, "ABC2") as ARInvoice;
			AssertWarpperPropertiesOfTransaction("Reversal AR Invoice", arCreditNote.AH_TransactionReference, reversalARInvoice, hasOriginalTransaction: true, isWritingOff: true, isReverseTransaction: true, isAmendingCreditNote: false, assertGovernmentInvoice: false);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var reversalARInvoiceNewFactory = newFactory.Load<ARInvoice>(reversalARInvoice.PK);
			AssertWarpperPropertiesOfTransaction("Reversal AR Credit Note New Factory", arCreditNote.AH_TransactionReference, reversalARInvoiceNewFactory, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: true, isAmendingCreditNote: false);

			var arCreditNoteNewFactory = newFactory.Load<ARInvoice>(arCreditNote.PK);
			AssertWarpperPropertiesOfTransaction("AR Credit Note as Government Invoice", arCreditNote.AH_TransactionReference, arCreditNoteNewFactory, hasOriginalTransaction: false, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: false);
		}

		public void TestTransactionHeaderWrapperForTransactionsLoadedAsGovernmentInvoice()
		{
			var transactionTypes = new[]
			{
				new { TransactionType = typeof(APInvoice) },
				new { TransactionType = typeof(APCreditNote) },
				new { TransactionType = typeof(APAdjustmentNote) },
				new { TransactionType = typeof(ARAP.APContraRow) },
				new { TransactionType = typeof(ARAP.Journal.APJournal) },
				new { TransactionType = typeof(ARAP.ReceiptPayment.APPayment) },
				new { TransactionType = typeof(ARAP.ReceiptPayment.APReceipt) },
				new { TransactionType = typeof(ARAP.APDiscount) },
				new { TransactionType = typeof(ARAP.Overpayment.APOverpayment) },
				new { TransactionType = typeof(ARAP.APExchangeDifference) },
				new { TransactionType = typeof(ARInvoice) },
				new { TransactionType = typeof(ARCreditNote) },
				new { TransactionType = typeof(ARAdjustmentNote) },
				new { TransactionType = typeof(ARAP.ARContraRow) },
				new { TransactionType = typeof(ARAP.Journal.ARJournal) },
				new { TransactionType = typeof(ARAP.ReceiptPayment.ARPayment) },
				new { TransactionType = typeof(ARAP.ReceiptPayment.ARReceipt) },
				new { TransactionType = typeof(ARAP.ARDiscount) },
				new { TransactionType = typeof(ARAP.Overpayment.AROverpayment) },
				new { TransactionType = typeof(ARAP.ARExchangeDifference) },
				new { TransactionType = typeof(InvoiceBatchHeader) },
				new { TransactionType = typeof(CashBook.DirectPayment.DirectPayment) },
				new { TransactionType = typeof(CashBook.DirectReceipt.DirectReceipt) },
				new { TransactionType = typeof(CashBook.OpeningReceipt.OpeningReceipt) },
				new { TransactionType = typeof(CashBook.OpeningPayment.OpeningPayment) },
				new { TransactionType = typeof(CashBook.ExchangeDifference.CashbookExchangeDiff) },
				new { TransactionType = typeof(CashBook.DirectDebitBatch.DirectDebitBatchHeader) },
				new { TransactionType = typeof(JobInvoicing.JCJournalHeader) },
				new { TransactionType = typeof(JobInvoicing.JobRevenueJournal) },
				new { TransactionType = typeof(UAInvoice) },
				new { TransactionType = typeof(UACreditNote) }
			};

			foreach (var transactionType in transactionTypes)
			{
				var transaction = Factory.NewWithValidTestData(transactionType.TransactionType) as TransactionHeader;
				Factory.Save();

				var govTransaction = new BusinessObjectFactory().Load<GovernmentInvoice>(transaction.PK);
				AssertNoExceptionThrown(() => ElectronicInvoicingEligibilityDecider.CreateWrapper(govTransaction));
			}
		}

		public void TestTransactionHeaderAuthorisationRecordForTurkey_Post()
			=> AssertTransactionHeaderAuthorisationRecordForTurkey();

		public void TestTransactionHeaderAuthorisationRecordForTurkey_Manual_HasComplianceNumber()
			=> AssertTransactionHeaderAuthorisationRecordForTurkey("MAN2021000000001", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual);

		public void TestTransactionHeaderAuthorisationRecordForTurkey_Manual_NoComplianceNumber()
			=> AssertTransactionHeaderAuthorisationRecordForTurkey("", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual);

		void AssertTransactionHeaderAuthorisationRecordForTurkey(string complianceNumber = "", string registryValue = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post)
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				var objectCreator = new TestObjectCreator(Factory);
				var signatureCredential = Factory.NewWithValidTestData<GlbCompanySignatureCredential>();
				signatureCredential.GP_GC = GlbCompany.CurrentCompany.PK;
				signatureCredential.GP_UserID = "abc";
				signatureCredential.CurrentDecryptedPassword = "1234";
				GlbCompany.CurrentCompany.SignatureCredentials.Add(signatureCredential);
				Factory.Save();

				var cusCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_RN_NKCodeCountry == Constants.CountryCodes.Turkey && x.OK_CodeType == OrgCusCode.CodeTypes.VATCode);
				if (cusCode == null)
				{
					cusCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1234567890");
					cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Turkey;
					GlbCompany.CurrentCompany.Factory.Save();
				}

				objectCreator.CreateNewComplianceSequence(GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR);
				objectCreator.CreateNewComplianceSequence(GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN);

				var arInvoice = objectCreator.CreateARInvoice<ARInvoice>("0001", objectCreator.TRY, 1m, objectCreator.AALSHI, TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR);
				Factory.Save();

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));

				var newFactory = new BusinessObjectFactory();
				if (registryValue == AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post)
				{
					AssertNotNull(pivot);
					AssertEquals(EInvoicingPivotActionType.Submit, pivot.AIP_ActionType);
				}
				else
				{
					AssertNull(pivot);

					if (!string.IsNullOrEmpty(complianceNumber))
					{
						var govTransaction = newFactory.Load<GovernmentInvoice>(arInvoice.PK);
						govTransaction.AH_TransactionReference = complianceNumber;
						AssertNoExceptionThrown(() => newFactory.Save());
						pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
						AssertNotNull(pivot);
						AssertEquals(EInvoicingPivotActionType.Submit, pivot.AIP_ActionType);
						AssertEquals(EInvoicingPivotState.Queued, pivot.AIP_Status);
					}
				}

				var creditNote = objectCreator.CreateReversalTransaction(arInvoice, "") as ARCreditNote;
				creditNote.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN;
				Factory.Save();

				var arCreditNoteWrapper = new TransactionHeaderWrapper(creditNote);
				AssertEquals("Original transaction's compliance subtype should be earchive.", TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR, arCreditNoteWrapper.OriginalTransaction.AH_ComplianceSubType);

				var postedArInvoice = objectCreator.Factory.Load<ARInvoice>(arInvoice.PK);

				pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, creditNote.PK));
				if (!postedArInvoice.AH_TransactionReference.IsEmpty)
				{
					AssertNotNull(pivot);
					AssertEquals(EInvoicingPivotActionType.Cancel, pivot.AIP_ActionType);
				}
				else
				{
					AssertNull(pivot);
				}

				AssertGovernmentInvoice(newFactory, arInvoice.PK, "AR Invoice as Government Invoice", originalTransactionReference: "", hasOriginalTransaction: false, isWritingOff: false, isReverseTransaction: false, isAmendingCreditNote: false);
				AssertGovernmentInvoice(newFactory, creditNote.PK, "Reversal Credit Note as Government Invoice", originalTransactionReference: arInvoice.AH_TransactionReference, hasOriginalTransaction: true, isWritingOff: false, isReverseTransaction: true, isAmendingCreditNote: false);
			}
		}

		void AssertGovernmentInvoice(BusinessObjectFactory factory, ZGuid guid, string message, string originalTransactionReference = "", bool hasOriginalTransaction = false, bool isWritingOff = false, bool isReverseTransaction = false, bool isAmendingCreditNote = false)
		{
			var governmentInvoice = factory.Load<GovernmentInvoice>(guid);
			var governmentInvoiceWrapper = ElectronicInvoicingEligibilityDecider.CreateWrapper(governmentInvoice);
			AssertTransactionHeaderWrapper(message, originalTransactionReference, governmentInvoiceWrapper, hasOriginalTransaction, isWritingOff, isReverseTransaction, isAmendingCreditNote);
		}

		void AssertTransactionHeaderWrapper(string message, string transactionReference, ITransactionHeaderWrapper wrapper, bool hasOriginalTransaction = false, bool isWritingOff = false, bool isReverseTransaction = false, bool isAmendingCreditNote = false)
		{
			AssertEquals(message + ": OriginalTransaction", hasOriginalTransaction, wrapper.OriginalTransaction != null);
			if (hasOriginalTransaction)
			{
				AssertEquals(message + ": OriginalTransactionReference", transactionReference, wrapper.OriginalTransaction.AH_TransactionReference);
			}
			AssertEquals(message + ": IsWritingOff", isWritingOff, wrapper.IsWritingOff);
			AssertEquals(message + ": IsReverseTransaction", isReverseTransaction, wrapper.IsReverseTransaction);
			AssertEquals(message + ": IsAmendingCreditNote", isAmendingCreditNote, wrapper.IsAmendingCreditNote);
		}

		public void TestEligibleTransactionHeaderUruguay()
		{
			var testConfigurations = new[]
			{
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TXI },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TXI },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TCD },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TCD },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TCR },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TCR },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TKC },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TKC },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TKD },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TKD },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TKT },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TKT },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YCD },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YCD },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YCR },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YCR },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YKD },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YKD },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YKR },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YKR },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YKT },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YKT },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YXI },
				new { Expected = true, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YXI },

				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.XCL },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.XCL },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.XCL },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.XCL },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TXI },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TXI },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TCD },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TCD },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TCR },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TCR },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TKC },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TKC },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TKD },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TKD },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TKT },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TKT },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.XCL },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.XCL },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YCD },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YCD },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YCR },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YCR },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YKD },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YKD },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YKR },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YKR },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YKT },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YKT },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YXI },
				new { Expected = false, Country = Constants.CountryCodes.Uruguay, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote, ComplianceSubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YXI },
			};

			for (int index = 0; index < testConfigurations.Length; index++)
			{
				var testConfiguration = testConfigurations[index];
				var actual = ElectronicInvoicingEligibilityDecider.IsEligible(new TestTransactionHeaderWrapper(Constants.CountryCodes.Uruguay, testConfiguration.LedgerType, testConfiguration.TransactionType, headerOrgCusCodeCountryCode: testConfiguration.Country, complianceSubType: testConfiguration.ComplianceSubType));
				AssertEquals($"REC#{index} {nameof(Constants.CountryCodes.Uruguay)} {testConfiguration.LedgerType} {testConfiguration.TransactionType} {testConfiguration.Country} {testConfiguration.ComplianceSubType}", testConfiguration.Expected, actual);
			}
		}

		#region Hungary

		public void TestEligibleTransactionHeaderHungary()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var isEligible = ElectronicInvoicingEligibilityDecider.IsEligible(
			new TestTransactionHeaderWrapper(
				Constants.CountryCodes.Hungary,
				LedgerTypes.AccountsReceivable,
				TransactionTypes.Invoice,
				headerOrgCusCodeCountryCode: Constants.CountryCodes.Hungary,
				headerOrgCusCodeNumber: "1234",
				gstAmountLocalCurrency: 1m,
				companyPK: Env.CurrentCompanyPK
			));
			AssertEquals("HU AR INV with HU VAT/GBR is eligible", true, isEligible);

			isEligible = ElectronicInvoicingEligibilityDecider.IsEligible(
			new TestTransactionHeaderWrapper(
				Constants.CountryCodes.Hungary,
				LedgerTypes.AccountsReceivable,
				TransactionTypes.CreditNote,
				headerOrgCusCodeCountryCode: Constants.CountryCodes.Hungary,
				headerOrgCusCodeNumber: "1234",
				gstAmountLocalCurrency: -1m,
				companyPK: Env.CurrentCompanyPK
			));
			AssertEquals("HU AR CRD with HU VAT/GBR is eligible", true, isEligible);

			isEligible = ElectronicInvoicingEligibilityDecider.IsEligible(
			new TestTransactionHeaderWrapper(
				Constants.CountryCodes.Iceland,
				LedgerTypes.AccountsReceivable,
				TransactionTypes.Invoice,
				headerOrgCusCodeCountryCode: Constants.CountryCodes.Iceland,
				headerOrgCusCodeNumber: "1234",
				gstAmountLocalCurrency: 1m,
				companyPK: Env.CurrentCompanyPK
			));
			AssertEquals("HU AR INV without HU VAT/GBR is NOT eligible", false, isEligible);

			isEligible = ElectronicInvoicingEligibilityDecider.IsEligible(
			new TestTransactionHeaderWrapper(
				Constants.CountryCodes.Hungary,
				LedgerTypes.AccountsReceivable,
				TransactionTypes.Invoice,
				headerOrgCusCodeCountryCode: Constants.CountryCodes.Hungary,
				headerOrgGBRNumber: "4321",
				gstAmountLocalCurrency: 1m,
				companyPK: Env.CurrentCompanyPK
			));
			AssertEquals("HU AR INV with HU GBR is eligible", true, isEligible);

			var huCompany = testObjectCreator.CreateCompanyAndBranch("HUBUD");
			var isCompany = testObjectCreator.CreateCompanyAndBranch("ISREY");
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, huCompany.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				isEligible = ElectronicInvoicingEligibilityDecider.IsEligible(
				new TestTransactionHeaderWrapper(
					Constants.CountryCodes.Iceland,
					LedgerTypes.AccountsReceivable,
					TransactionTypes.Invoice,
					headerOrgCusCodeCountryCode: Constants.CountryCodes.Iceland,
					headerOrgCusCodeNumber: "1234",
					gstAmountLocalCurrency: 1m,
					companyPK: Env.CurrentCompanyPK
				));
				AssertEquals("Current branch / company should not affect eligibility", false, isEligible);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, isCompany.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				isEligible = ElectronicInvoicingEligibilityDecider.IsEligible(
				new TestTransactionHeaderWrapper(
					Constants.CountryCodes.Hungary,
					LedgerTypes.AccountsReceivable,
					TransactionTypes.Invoice,
					headerOrgCusCodeCountryCode: Constants.CountryCodes.Hungary,
					headerOrgGBRNumber: "4321",
					gstAmountLocalCurrency: 1m,
					companyPK: Env.CurrentCompanyPK
				));
				AssertEquals("Current branch / company should not affect eligibility", true, isEligible);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, huCompany.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				isEligible = ElectronicInvoicingEligibilityDecider.IsEligible(
				new TestTransactionHeaderWrapper(
					Constants.CountryCodes.Hungary,
					LedgerTypes.AccountsReceivable,
					TransactionTypes.Invoice,
					headerOrgCusCodeCountryCode: Constants.CountryCodes.Hungary,
					gstAmountLocalCurrency: 1m,
					orgCountryCode: Constants.CountryCodes.Hungary,
					companyPK: Env.CurrentCompanyPK
				));
				AssertEquals("HU AR INV without HU VAT/GBR is eligible", true, isEligible);

				isEligible = ElectronicInvoicingEligibilityDecider.IsEligible(
				new TestTransactionHeaderWrapper(
					Constants.CountryCodes.Hungary,
					LedgerTypes.AccountsReceivable,
					TransactionTypes.Invoice,
					headerOrgCusCodeCountryCode: Constants.CountryCodes.Iceland,
					gstAmountLocalCurrency: 1m,
					orgCountryCode: Constants.CountryCodes.Iceland,
					companyPK: Env.CurrentCompanyPK
				));
				AssertEquals("NON HU AR INV when registry threshold is zero is eligible", true, isEligible);
			}

			isEligible = ElectronicInvoicingEligibilityDecider.IsEligible(
			new TestTransactionHeaderWrapper(
				Constants.CountryCodes.Hungary,
				LedgerTypes.AccountsPayable,
				TransactionTypes.Invoice,
				headerOrgCusCodeCountryCode: Constants.CountryCodes.Hungary,
				headerOrgCusCodeNumber: "1234",
				gstAmountLocalCurrency: 1000.00m,
				companyPK: Env.CurrentCompanyPK
			));
			AssertEquals("HU AP transactions are NOT eligible", false, isEligible);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, huCompany.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				isEligible = ElectronicInvoicingEligibilityDecider.IsEligible(
				new TestTransactionHeaderWrapper(
					Constants.CountryCodes.Hungary,
					LedgerTypes.AccountsReceivable,
					TransactionTypes.Invoice,
					headerOrgCusCodeCountryCode: Constants.CountryCodes.Hungary,
					headerOrgCusCodeNumber: "1234",
					gstAmountLocalCurrency: 200.0m,
					relatedTransactionWasEligible: false,
					companyPK: Env.CurrentCompanyPK
				));
				AssertEquals("HU AR INV is eligible", true, isEligible);

				isEligible = ElectronicInvoicingEligibilityDecider.IsEligible(
				new TestTransactionHeaderWrapper(
					Constants.CountryCodes.Hungary,
					LedgerTypes.AccountsReceivable,
					TransactionTypes.Invoice,
					headerOrgCusCodeCountryCode: Constants.CountryCodes.Iceland,
					gstAmountLocalCurrency: 1m,
					orgCountryCode: Constants.CountryCodes.Iceland,
					companyPK: Env.CurrentCompanyPK
				));
				AssertEquals("NON HU AR INV is eligible", true, isEligible);
			}
		}

		#endregion

		public void TestEligibleTransactionHeaderBrazil()
		{
			var testConfigurations = new[]
			{
				// Expected cases
				new { Expected = true,  LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice,        ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS, ComplianceNumber = "1234", IsCancelled = false, TaxSystemCodes = new List<ZString> { "ISS" } },
				new { Expected = true,  LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote,     ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS, ComplianceNumber = "1234", IsCancelled = true, TaxSystemCodes = new List<ZString> { "ISS" }  },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice,        ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS, ComplianceNumber = "",     IsCancelled = false, TaxSystemCodes = new List<ZString> { "ISS" } },

				// Technically correct cases (but should not appear in real life)
				new { Expected = true,  LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice,        ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS, ComplianceNumber = "1234", IsCancelled = false, TaxSystemCodes = new List<ZString> { "ISS" } },
				new { Expected = true,  LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote,     ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS, ComplianceNumber = "1234", IsCancelled = true, TaxSystemCodes = new List<ZString> { "ISS" }  },

				// Negative cases
				new { Expected = false, LedgerType = LedgerTypes.AccountsPayable,    TransactionType = TransactionTypes.Invoice,        ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS, ComplianceNumber = "1234", IsCancelled = false, TaxSystemCodes = new List<ZString> { "ISS" } },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote,     ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS, ComplianceNumber = "1234", IsCancelled = false, TaxSystemCodes = new List<ZString> { "ISS" } },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.AdjustmentNote, ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS, ComplianceNumber = "1234", IsCancelled = false, TaxSystemCodes = new List<ZString> { "ISS" } },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice,        ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFE, ComplianceNumber = "1234", IsCancelled = false, TaxSystemCodes = new List<ZString> { "ISS" } },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice,        ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNE, ComplianceNumber = "1234", IsCancelled = false, TaxSystemCodes = new List<ZString> { "ISS" } },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice,        ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XND, ComplianceNumber = "1234", IsCancelled = false, TaxSystemCodes = new List<ZString> { "ISS" } },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice,        ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XNC, ComplianceNumber = "1234", IsCancelled = false, TaxSystemCodes = new List<ZString> { "ISS" } },
				new { Expected = false,  LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice,        ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS, ComplianceNumber = "1234", IsCancelled = false, TaxSystemCodes = (List<ZString>)null },
				new { Expected = false,  LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote,     ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS, ComplianceNumber = "1234", IsCancelled = true, TaxSystemCodes = (List<ZString>)null  },
				new { Expected = false,  LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice,        ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS, ComplianceNumber = "1234", IsCancelled = false, TaxSystemCodes = (List<ZString>)null },
				new { Expected = false,  LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote,     ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS, ComplianceNumber = "1234", IsCancelled = true, TaxSystemCodes = (List<ZString>)null  },
			};

			for (int index = 0; index < testConfigurations.Length; index++)
			{
				var testConfiguration = testConfigurations[index];
				var transactionWrapper = new TestTransactionHeaderWrapper(
						Constants.CountryCodes.Brazil,
						testConfiguration.LedgerType,
						testConfiguration.TransactionType,
						complianceSubType: testConfiguration.ComplianceSubType,
						transactionReference: testConfiguration.ComplianceNumber,
						isCancelled: testConfiguration.IsCancelled,
						taxSystemCodes: testConfiguration.TaxSystemCodes
					);
				var actual = ElectronicInvoicingEligibilityDecider.IsEligible(transactionWrapper);
				AssertEquals($"REC#{index} {nameof(Constants.CountryCodes.Brazil)} {testConfiguration.LedgerType} {testConfiguration.TransactionType} {testConfiguration.ComplianceSubType} {testConfiguration.ComplianceNumber} Is{(testConfiguration.IsCancelled ? "" : "NOT")}Cancelled", testConfiguration.Expected, actual);
			}
		}

		public void TestEligibleTransactionHeaderBrazil_WhenCRDIsAllocatedComplianceNumberAfterPosting()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var company = testObjectCreator.CreateNewCompany("BR1", "BR");
			company.GC_Name = "Brazil Company";
			var branch = testObjectCreator.CreateNewBranch(company, "B01");
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-7).ToDateTime()))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var job = testObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "charge1", testObjectCreator.EUR, 10m, testObjectCreator.Creditor1, testObjectCreator.EUR, 10m, testObjectCreator.Debtor1);
				charge.JR_AT_SellGSTRate = testObjectCreator.ServiceTax.PK;
				Factory.Save();

				// Original transaction (already sent)
				var arInvoice = (ARInvoice)testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV123456", testObjectCreator.EUR, 1m, 10m, 0M, 10m, 0m, testObjectCreator.Debtor1, testObjectCreator.CC1.PK);
				arInvoice.Lines[0].AL_AT = charge.JR_AT_SellGSTRate;
				arInvoice.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
				arInvoice.AH_TransactionReference = "ABC123";
				var pivot = testObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, status: "SUC");
				pivot.AIP_LastSentTimeUtc = ZDateTime.Now;
				pivot.AIP_LastResponseReceivedUtc = ZDateTime.Now;
				var batch = testObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, "SNT");
				batch.AIB_EHubAllocatedNumber = "329a0994-f5ac-48c1-8d49-4df6a3e167f7";
				batch.AIB_GovernmentAllocatedNumber = "GvtNumber1234";
				batch.TransactionPivots.Add(pivot);
				Factory.Save();

				// Subsequent reversal (not eligible yet)
				var arCreditNote = (ARCreditNote)testObjectCreator.ReverseTransaction(arInvoice, out _);
				arCreditNote.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS;

				var taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
				taxTransaction.ATT_AH = arCreditNote.PK;
				taxTransaction.ATT_TaxSystemCode = "ISS";

				Factory.Save();
				AssertNullOrEmpty("Precondition: Reversal CRD has no compliance number", arCreditNote.AH_TransactionReference);
				AssertNotNull("Precondition: Reversal CRD is eligible therefore has pivot", arCreditNote.GetMostRecentEInvoicingTransactionPivot());

				// Allocation of compliance number AFTER posting
				var newFactory = Factory.CreateNewFactory();
				var arCreditNoteInNewFactory = newFactory.Load<ARCreditNote>(arCreditNote.PK);
				arCreditNoteInNewFactory.AH_TransactionReference = "ABC456";
				newFactory.Save();
				AssertNotNull("Reversal CRD is eligible after compliance number is allocated therefore has a pivot", arCreditNoteInNewFactory.GetMostRecentEInvoicingTransactionPivot());
				AssertEquals("Reversal CRD pivot action type should be CAN", "CAN", arCreditNoteInNewFactory.GetMostRecentEInvoicingTransactionPivot().AIP_ActionType);
			}
		}

		public void TestEligibleTransactionHeaderEgypt()
		{
			var testConfigurations = new[]
			{
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice },
				new { Expected = true, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.AdjustmentNote },
				new { Expected = false, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice },
				new { Expected = false, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote },
				new { Expected = false, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.AdjustmentNote },
			};

			foreach (var testConfigurationEnumerated in Enumerable.Range(1, testConfigurations.Length).Zip(testConfigurations, (index, testConfiguration) => new { index, testConfiguration }))
			{
				var index = testConfigurationEnumerated.index;
				var testConfiguration = testConfigurationEnumerated.testConfiguration;
				var actual = ElectronicInvoicingEligibilityDecider.IsEligible(new TestTransactionHeaderWrapper(Constants.CountryCodes.Egypt, testConfiguration.LedgerType, testConfiguration.TransactionType));
				AssertEquals($"REC#{index} {nameof(Constants.CountryCodes.Egypt)} {testConfiguration.LedgerType} {testConfiguration.TransactionType}", testConfiguration.Expected, actual);
			}
		}

		#region Related Transactions - GetAnyRelatedTransactionsWereEligible() and GetOriginalAndRelatedTransactions()

		public void TestTransactionHeaderWrapper_RelatedTransactions_NoRelatedTransactions()
		{
			var objectCreator = new TestObjectCreator(Factory);

			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", objectCreator.AUD, 1m, 100m, 10m, 100m, 10m);
			Factory.Save();

			var wrappedInvoice = ElectronicInvoicingEligibilityDecider.CreateWrapper(arInvoice);
			AssertEquals("When no related transactions, none can be eligible", false, wrappedInvoice.GetAnyRelatedTransactionsWereEligible());
			AssertEquals("When no related transactions, there are... well... no related transactions", 0, wrappedInvoice.GetOriginalAndRelatedTransactions().Count);
		}

		public void TestTransactionHeaderWrapper_RelatedTransactions_WithoutPivot()
		{
			var objectCreator = new TestObjectCreator(Factory);

			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", objectCreator.AUD, 1m, 100m, 10m, 100m, 10m);
			var arCredit = (ARCreditNote)((IAmending)arInvoice).GenerateAmendingTransaction(TransactionTypes.CreditNote);
			objectCreator.AddLineToCreditNote(arCredit, ZGuid.Empty, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, 10);
			Factory.Save();

			var wrappedCredit = ElectronicInvoicingEligibilityDecider.CreateWrapper(arCredit);
			AssertEquals("When CRD related to INV without pivot, it cannot be eligible", false, wrappedCredit.GetAnyRelatedTransactionsWereEligible());
			AssertEquals("When CRD related to INV without pivot, there is a related transaction", 1, wrappedCredit.GetOriginalAndRelatedTransactions().Count);
			var relatedInvoice = wrappedCredit.GetOriginalAndRelatedTransactions().First();
			AssertEquals("When CRD related to INV: ledger", LedgerTypes.AccountsReceivable, relatedInvoice.AH_Ledger);
			AssertEquals("When CRD related to INV: type", TransactionTypes.Invoice, relatedInvoice.AH_TransactionType);
			AssertEquals("When CRD related to INV: tax value", 10m, relatedInvoice.AH_GSTAmount);
		}

		public void TestTransactionHeaderWrapper_RelatedTransactions_WithInvalidPivot()
		{
			var objectCreator = new TestObjectCreator(Factory);

			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", objectCreator.AUD, 1m, 100m, 10m, 100m, 10m);
			var batch = objectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Constants.EInvoicingPivotState.Discarded);

			var arCredit = (ARCreditNote)((IAmending)arInvoice).GenerateAmendingTransaction(TransactionTypes.CreditNote);
			objectCreator.AddLineToCreditNote(arCredit, ZGuid.Empty, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, 10);
			Factory.Save();

			var wrappedCredit = ElectronicInvoicingEligibilityDecider.CreateWrapper(arCredit);
			AssertEquals("When CRD related to INV with Discarded pivot (DCD), it cannot be eligible", false, wrappedCredit.GetAnyRelatedTransactionsWereEligible());
			AssertEquals("When CRD related to INV with Discarded pivot (DCD), there is a related transaction", 1, wrappedCredit.GetOriginalAndRelatedTransactions().Count);
			var relatedInvoice = wrappedCredit.GetOriginalAndRelatedTransactions().First();
			AssertEquals("When CRD related to INV: ledger", LedgerTypes.AccountsReceivable, relatedInvoice.AH_Ledger);
			AssertEquals("When CRD related to INV: type", TransactionTypes.Invoice, relatedInvoice.AH_TransactionType);
			AssertEquals("When CRD related to INV: tax value", 10m, relatedInvoice.AH_GSTAmount);
		}

		public void TestTransactionHeaderWrapper_RelatedTransactions_WithValidPivot()
		{
			var objectCreator = new TestObjectCreator(Factory);

			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", objectCreator.AUD, 1m, 100m, 10m, 100m, 10m);
			var batch = objectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Constants.EInvoicingPivotState.Batched);

			var arCredit = (ARCreditNote)((IAmending)arInvoice).GenerateAmendingTransaction(TransactionTypes.CreditNote);
			objectCreator.AddLineToCreditNote(arCredit, ZGuid.Empty, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, 10);
			Factory.Save();

			var wrappedCredit = ElectronicInvoicingEligibilityDecider.CreateWrapper(arCredit);
			AssertEquals("When CRD related to INV with pivot, it is eligible", true, wrappedCredit.GetAnyRelatedTransactionsWereEligible());
			AssertEquals("When CRD related to INV with pivot, there is a related transaction", 1, wrappedCredit.GetOriginalAndRelatedTransactions().Count);
			var relatedInvoice = wrappedCredit.GetOriginalAndRelatedTransactions().First();
			AssertEquals("When CRD related to INV: ledger", LedgerTypes.AccountsReceivable, relatedInvoice.AH_Ledger);
			AssertEquals("When CRD related to INV: type", TransactionTypes.Invoice, relatedInvoice.AH_TransactionType);
			AssertEquals("When CRD related to INV: tax value", 10m, relatedInvoice.AH_GSTAmount);
		}

		public void TestTransactionHeaderWrapper_RelatedTransactions_WithValidPivotAndManyAmendingInvoices()
		{
			var objectCreator = new TestObjectCreator(Factory);

			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", objectCreator.AUD, 1m, 100m, 10m, 100m, 10m);
			var arInvoice2 = (ARInvoice)((IAmending)arInvoice).GenerateAmendingTransaction(TransactionTypes.Invoice);
			objectCreator.CreateARInvoiceLine(arInvoice2, null, objectCreator.RevenueChargeCode, objectCreator.AUD, 1m, "number 2", 200m);
			var arInvoice3 = (ARInvoice)((IAmending)arInvoice).GenerateAmendingTransaction(TransactionTypes.Invoice);
			objectCreator.CreateARInvoiceLine(arInvoice3, null, objectCreator.RevenueChargeCode, objectCreator.AUD, 1m, "invoice the third", 300m);

			var batch = objectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice2, Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			var wrappedCredit = ElectronicInvoicingEligibilityDecider.CreateWrapper(arInvoice3);
			AssertEquals("When many amending INVs with pivot, it is eligible", true, wrappedCredit.GetAnyRelatedTransactionsWereEligible());
			AssertEquals("When many amending INVs, there are related transactions", 2, wrappedCredit.GetOriginalAndRelatedTransactions().Count);
		}

		#endregion

		#region KoreaSouth

		public void TestEligibleTransactionHeaderKoreaSouth()
		{
			var testConfigurations = new[]
			{
				new { Expected = true,  LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, HasOriginalTransaction = false, OriginalTransactionEInvoicingStatus = string.Empty },
				new { Expected = true,  LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.Invoice, HasOriginalTransaction = true, OriginalTransactionEInvoicingStatus = string.Empty },

				new { Expected = true,  LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, HasOriginalTransaction = true, OriginalTransactionEInvoicingStatus = EInvoicingPivotState.Queued },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, HasOriginalTransaction = true, OriginalTransactionEInvoicingStatus = string.Empty },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, HasOriginalTransaction = false, OriginalTransactionEInvoicingStatus = string.Empty },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.CreditNote, HasOriginalTransaction = false, OriginalTransactionEInvoicingStatus = string.Empty },

				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.AdjustmentNote, HasOriginalTransaction = false, OriginalTransactionEInvoicingStatus = string.Empty },
				new { Expected = false, LedgerType = LedgerTypes.AccountsReceivable, TransactionType = TransactionTypes.AdjustmentNote, HasOriginalTransaction = true, OriginalTransactionEInvoicingStatus = string.Empty },
				new { Expected = false, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.Invoice , HasOriginalTransaction = false, OriginalTransactionEInvoicingStatus = string.Empty },
				new { Expected = false, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.CreditNote , HasOriginalTransaction = false, OriginalTransactionEInvoicingStatus = string.Empty },
				new { Expected = false, LedgerType = LedgerTypes.AccountsPayable, TransactionType = TransactionTypes.AdjustmentNote , HasOriginalTransaction = false, OriginalTransactionEInvoicingStatus = string.Empty }
			};

			for (int index = 0; index < testConfigurations.Length; index++)
			{
				var testConfiguration = testConfigurations[index];
				var actual = ElectronicInvoicingEligibilityDecider.IsEligible
				(
					new TestTransactionHeaderWrapper
					(
						Constants.CountryCodes.KoreaSouth,
						testConfiguration.LedgerType,
						testConfiguration.TransactionType,
						hasOriginalTransaction: testConfiguration.HasOriginalTransaction,
						originalTransactionEInvoicingStatus: testConfiguration.OriginalTransactionEInvoicingStatus,
						lines: new[] { new TestTransactionLineWrapper(new TestTaxRateWrapper(taxType: AccTaxRate.Types.Rated)) }
					)
				);
				AssertEquals($"REC#{index} {nameof(Constants.CountryCodes.KoreaSouth)} {testConfiguration.LedgerType} {testConfiguration.TransactionType}", testConfiguration.Expected, actual);
			}
		}

		public void TestEligibleTransactionHeaderKoreaSouth_ForStandAloneCreditNote()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			var defaultPivotStatus = EInvoicingPivotState.Queued;

			var testObjectCreator = new TestObjectCreator(Factory);
			using (testObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultPivotStatus))
			{
				var arCreditNote1 = Factory.NewWithValidTestData<ARCreditNote>();
				var arCreditNote2 = Factory.NewWithValidTestData<ARCreditNote>();
				Factory.Save();

				new ReversingFactory().NewReversing(arCreditNote2).Reverse();
				var arInvoice = arCreditNote2.ReverseInvoice;
				Factory.Save();

				CombineAssertions("Precondition: ", () =>
				{
					AssertNull("Precondition", arCreditNote1.OriginalTransaction);
					AssertNull("Precondition", arCreditNote2.OriginalTransaction);
					AssertNotNull("Precondition", arInvoice.OriginalTransaction);
				});

				CombineAssertions("Precondition: arCreditNote1", () =>
				{
					AssertEquals(LedgerTypes.AccountsReceivable, arCreditNote1.AH_Ledger);
					AssertEquals(TransactionTypes.CreditNote, arCreditNote1.AH_TransactionType);
				});

				CombineAssertions("Precondition: arInvoice", () =>
				{
					AssertEquals(LedgerTypes.AccountsReceivable, arInvoice.OriginalTransaction.AH_Ledger);
					AssertEquals(TransactionTypes.CreditNote, arInvoice.OriginalTransaction.AH_TransactionType);
					AssertEquals(true, arInvoice.IsReverseTransaction);
					AssertEquals(false, ElectronicInvoicingEligibilityDecider.CreateWrapper(arInvoice).OriginalTransaction.HasOriginalTransaction);
				});

				AssertEquals("IsEligible should be false for Stand Alone Credit Note", false, ElectronicInvoicingEligibilityDecider.IsEligible(arCreditNote1));
				AssertEquals("IsEligible should be false for Reversal Transaction of Stand Alone Credit Note", false, ElectronicInvoicingEligibilityDecider.IsEligible(arInvoice));
			}
		}

		public void TestEligibleTransactionHeaderKoreaSouth_WhenOriginalTransactionEInvoicingStatusIsNotEmpty()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			var defaultPivotStatus = EInvoicingPivotState.Queued;

			var testObjectCreator = new TestObjectCreator(Factory);
			using (testObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultPivotStatus))
			{
				var arInvoice1 = testObjectCreator.CreateARInvoice<ARInvoice>("AR001", testObjectCreator.KRW, 1m, testObjectCreator.Debtor);
				testObjectCreator.CreateInvoiceLine(arInvoice1, testObjectCreator.KRW, 1.0m, 200m, 20m, 0m, taxRate: testObjectCreator.GST1);
				Assert("PreCondition", arInvoice1.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

				var arInvoice2 = testObjectCreator.CreateARInvoice<ARInvoice>("AR002", testObjectCreator.KRW, 1m, testObjectCreator.Debtor);
				testObjectCreator.CreateInvoiceLine(arInvoice2, testObjectCreator.KRW, 1.0m, 200m, 20m, 0m, taxRate: testObjectCreator.GST1);
				Assert("PreCondition", arInvoice2.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
				Factory.Save();

				var arCreditNote1 = testObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arInvoice1).amendTransaction as ARCreditNote;
				new ReversingFactory().NewReversing(arInvoice2).Reverse();
				var arCreditNote2 = arInvoice2.ReverseInvoice;
				var arCreditNote3 = Factory.NewWithValidTestData<ARCreditNote>();
				Factory.Save();

				AssertEquals("Precondition", true, arCreditNote1.IsARCreditNote);
				AssertEquals("Precondition", true, arCreditNote2.IsARCreditNote);
				AssertEquals("Precondition", true, arCreditNote3.IsARCreditNote);

				AssertEquals("Precondition", EInvoicingPivotState.Queued, arCreditNote1.OriginalTransaction.EInvoicingStatus);
				AssertEquals("Precondition", EInvoicingPivotState.Queued, arCreditNote2.OriginalTransaction.EInvoicingStatus);
				AssertNull("Precondition", arCreditNote3.OriginalTransaction);

				AssertEquals(true, ElectronicInvoicingEligibilityDecider.IsEligible(arCreditNote1));
				AssertEquals(true, ElectronicInvoicingEligibilityDecider.IsEligible(arCreditNote2));
				AssertEquals(false, ElectronicInvoicingEligibilityDecider.IsEligible(arCreditNote3));
			}
		}

		public void TestEligibleTransactionHeaderKoreaSouth_WhenOriginalTransactionEInvoicingStatusIsNullOrEmpty()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var arInvoice1 = testObjectCreator.CreateARInvoice<ARInvoice>("AR001", testObjectCreator.KRW, 1m, testObjectCreator.Debtor);
			var arInvoice2 = testObjectCreator.CreateARInvoice<ARInvoice>("AR002", testObjectCreator.KRW, 1m, testObjectCreator.Debtor);
			Factory.Save();

			var complianceDate = ZDateTime.Today.AddDays(-10);
			var defaultPivotStatus = EInvoicingPivotState.Queued;

			using (testObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultPivotStatus))
			{
				var arCreditNote1 = testObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arInvoice1).amendTransaction as ARCreditNote;
				new ReversingFactory().NewReversing(arInvoice2).Reverse();
				var arCreditNote2 = arInvoice2.ReverseInvoice;
				var arCreditNote3 = Factory.NewWithValidTestData<ARCreditNote>();
				Factory.Save();

				AssertEquals("Precondition", true, arCreditNote1.IsARCreditNote);
				AssertEquals("Precondition", true, arCreditNote2.IsARCreditNote);
				AssertEquals("Precondition", true, arCreditNote3.IsARCreditNote);

				AssertNullOrEmpty("Precondition", arCreditNote1.OriginalTransaction.EInvoicingStatus);
				AssertNullOrEmpty("Precondition", arCreditNote2.OriginalTransaction.EInvoicingStatus);
				AssertNull("Precondition", arCreditNote3.OriginalTransaction);

				AssertEquals(false, ElectronicInvoicingEligibilityDecider.IsEligible(arCreditNote1));
				AssertEquals(false, ElectronicInvoicingEligibilityDecider.IsEligible(arCreditNote2));
				AssertEquals(false, ElectronicInvoicingEligibilityDecider.IsEligible(arCreditNote3));
			}
		}

		public void TestEligibleTransactionHeaderKoreaSouth_TaxInvoiceDocumentTypeCodeIsEmpty()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			var defaultPivotStatus = EInvoicingPivotState.Queued;

			var registryItem = AccountingMasterFilesRegistry.Instance.TaxTypeToTaxInvoiceDocumentTypeCodeMapping;
			var newValueForRegistry = registryItem.DefaultValue.Clone(null, null) as CodeDescriptionWithGroupCollection;

			var taxType = NullTaxRateType.Code;

			newValueForRegistry.Cast<CodeDescriptionWithGroup>().Single(x => x.Code == taxType).Group = KoreaEInvoicingTypeCodeCategory.NotApplicable;
			newValueForRegistry.Cast<CodeDescriptionWithGroup>().Single(x => x.Code == "NOT").Group = KoreaEInvoicingTypeCodeCategory.NotApplicable;
			newValueForRegistry.Cast<CodeDescriptionWithGroup>().Single(x => x.Code == "EXL").Group = KoreaEInvoicingTypeCodeCategory.NotApplicable;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValueForRegistry);

			var testObjectCreator = new TestObjectCreator(Factory);
			using (testObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultPivotStatus))
			{
				var arInvoice1 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001",
					testObjectCreator.KRW, 1M, 100M, 10M, 100M, 10M);

				var arInvoice2 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR002",
					testObjectCreator.KRW, 1M, 100M, 10M, 100M, 10M);
				arInvoice2.Lines[0].AL_AT = testObjectCreator.ExcludedTax.PK;

				var arInvoice3 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR003",
					testObjectCreator.KRW, 1M, 100M, 10M, 100M, 10M);
				arInvoice3.Lines[0].AL_AT = testObjectCreator.ExtraServiceTax.PK;

				Factory.Save();

				AssertEquals(false, ElectronicInvoicingEligibilityDecider.IsEligible(arInvoice1));
				AssertEquals(false, ElectronicInvoicingEligibilityDecider.IsEligible(arInvoice2));
				AssertEquals(false, ElectronicInvoicingEligibilityDecider.IsEligible(arInvoice3));
			}
		}

		public void TestEligibleTransactionHeaderKoreaSouth_TaxInvoiceDocumentTypeCodeIsNotEmpty()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			var defaultPivotStatus = EInvoicingPivotState.Queued;

			var registryItem = AccountingMasterFilesRegistry.Instance.TaxTypeToTaxInvoiceDocumentTypeCodeMapping;
			var newValueForRegistry = registryItem.DefaultValue.Clone(null, null) as CodeDescriptionWithGroupCollection;

			var taxType = NullTaxRateType.Code;

			newValueForRegistry.Cast<CodeDescriptionWithGroup>().Single(x => x.Code == taxType).Group = KoreaEInvoicingTypeCodeCategory.NotApplicable;
			newValueForRegistry.Cast<CodeDescriptionWithGroup>().Single(x => x.Code == "NOT").Group = KoreaEInvoicingTypeCodeCategory.NotApplicable;
			newValueForRegistry.Cast<CodeDescriptionWithGroup>().Single(x => x.Code == "EXL").Group = KoreaEInvoicingTypeCodeCategory.NotApplicable;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValueForRegistry);

			var mockIFeatureControlManager = new FeatureControlTestDataFactory().CreateKoreaSouthComplianceSubTypeFeatureControlMock();
			var testObjectCreator = new TestObjectCreator(Factory);

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			using (testObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultPivotStatus))
			{
				var arInvoice = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001",
					testObjectCreator.KRW, 1M, 100M, 10M, 100M, 10M);
				arInvoice.AH_ComplianceSubType = "101";

				Factory.Save();

				AssertEquals(true, ElectronicInvoicingEligibilityDecider.IsEligible(arInvoice));
			}
		}

		public void TestEligibleTransactionHeaderKoreaSouth_ComplicanceSubTypeIsEmpty()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			var defaultPivotStatus = EInvoicingPivotState.Queued;

			var registryItem = AccountingMasterFilesRegistry.Instance.TaxTypeToTaxInvoiceDocumentTypeCodeMapping;
			var newValueForRegistry = registryItem.DefaultValue.Clone(null, null) as CodeDescriptionWithGroupCollection;

			var taxType = NullTaxRateType.Code;

			newValueForRegistry.Cast<CodeDescriptionWithGroup>().Single(x => x.Code == taxType).Group = KoreaEInvoicingTypeCodeCategory.NotApplicable;
			newValueForRegistry.Cast<CodeDescriptionWithGroup>().Single(x => x.Code == "NOT").Group = KoreaEInvoicingTypeCodeCategory.NotApplicable;
			newValueForRegistry.Cast<CodeDescriptionWithGroup>().Single(x => x.Code == "EXL").Group = KoreaEInvoicingTypeCodeCategory.NotApplicable;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValueForRegistry);

			var mockIFeatureData = new Mock<IFeatureData>();
			var accountingEInvoicingKoreaSubTypesFeatureControlData = new AccountingEInvoicingKoreaSubTypesFeatureControlData();
			accountingEInvoicingKoreaSubTypesFeatureControlData.IsComplianceSubTypeFeatureEnabled = true;
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out accountingEInvoicingKoreaSubTypesFeatureControlData)).Returns(true);
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingEInvoicingKoreaSouthComplianceSubTypeFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			var testObjectCreator = new TestObjectCreator(Factory);
			using (testObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultPivotStatus))
			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				var arInvoice = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001",
					testObjectCreator.KRW, 1M, 100M, 10M, 100M, 10M);

				var arInvoice1 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR002",
					testObjectCreator.KRW, 1M, 100M, 10M, 100M, 10M);
				arInvoice1.AH_ComplianceSubType = "101";

				Factory.Save();

				AssertEquals(false, ElectronicInvoicingEligibilityDecider.IsEligible(arInvoice));
				AssertEquals(true, ElectronicInvoicingEligibilityDecider.IsEligible(arInvoice1));
			}
		}

		#endregion
	}

	#region Implementation

	#region ElectronicInvoicingEligibilityDecider.ITransactionHeaderWrapperBase

	class TestTransactionHeaderWrapperBase : ITransactionHeaderWrapperBase
	{
		public TestTransactionHeaderWrapperBase(ZString countryCode,
				ZString ledger,
				ZString transactionType,
				bool isDisbursementCalc = false,
				IEnumerable<bool> isNotReportables = null,
				IEnumerable<bool> isRVSs = null,
				string headerOrgCusCodeCountryCode = null,
				string headerOrgCusCodeNumber = null,
				string complianceSubType = null,
				string transactionReference = null,
				string governmentAllocatedID = null,
				decimal gstAmountLocalCurrency = 0m,
				string headerOrgCategory = null,
				string headerOrgCountry = null,
				ZGuid? companyPK = null,
				bool isCancelled = false,
				IEnumerable<ZString> taxSystemCodes = null,
				ZDateTime? postDate = null,
				IEnumerable<bool> isLineWithTaxRates = null,
				int maximumNumberDigits = 7,
				ZGuid? branchPK = null,
				decimal invoiceAmountLocalCurrency = 0m,
				string eInvoicingStatus = null,
				IEnumerable<ITransactionLineWrapper> lines = null)
		{
			Company = new TestGlbCompanyWrapper(countryCode, companyPK);
			Branch = new TestGlbBranchWrapper(branchPK);
			AH_Ledger = ledger;
			AH_TransactionType = transactionType;
			AH_IsDisbursementCalc = isDisbursementCalc;
			AH_ComplianceSubType = complianceSubType;
			AH_TransactionReference = transactionReference;
			AH_GovernmentAllocatedID = governmentAllocatedID;
			AH_GSTAmount = gstAmountLocalCurrency;
			AH_InvoiceAmount = invoiceAmountLocalCurrency;
			Lines = lines
					?? isNotReportables?.Select(isNotReportable => new TestTransactionLineWrapper(isNotReportable: isNotReportable))
					?? isRVSs?.Select(isRVS => new TestTransactionLineWrapper(isRVS: isRVS))
					?? isLineWithTaxRates?.Select(isLineWithTaxRate => new TestTransactionLineWrapper(isLineWithTaxRate: isLineWithTaxRate))
					?? Enumerable.Empty<ITransactionLineWrapper>();
			HeaderTaxNumbersByCode[OrgCusCode.CodeTypes.GSTCode] = new TestOrgHeaderCusCodeInfo(string.Empty, headerOrgCusCodeNumber, headerOrgCusCodeCountryCode, OrgCusCode.CodeTypes.GSTCode);
			HeaderTaxNumbersByCode[OrgCusCode.CodeTypes.VATCode] = new TestOrgHeaderCusCodeInfo(string.Empty, headerOrgCusCodeNumber, headerOrgCusCodeCountryCode, OrgCusCode.CodeTypes.VATCode);
			OH_Category = headerOrgCategory;
			OrgCountryCode = headerOrgCountry;
			AH_IsCancelled = isCancelled;
			TaxTransactions = taxSystemCodes?.Select(taxSystemCode => new TestTaxTransactionWrapper(taxSystemCode)) ?? Enumerable.Empty<ITaxTransactionWrapper>();
			AH_PostDate = (ZDateTime)(postDate != null ? postDate : ZDateTime.Empty);
			ComplianceSequence = new TestComplianceSequenceWrapper(maximumNumberDigits);
			EInvoicingStatus = eInvoicingStatus;
		}

		public IGlbCompanyWrapper Company { get; }

		public IGlbBranchWrapper Branch { get; }

		public ZString AH_Ledger { get; }

		public ZString AH_TransactionType { get; }
		public ZString AH_TransactionReference { get; }
		public ZString AH_GovernmentAllocatedID { get; }
		public ZBool AH_IsDisbursementCalc { get; }

		public ZDecimal AH_GSTAmount { get; }

		public ZDecimal AH_InvoiceAmount { get; }

		public ZDateTime AH_PostDate { get; }
		public IEnumerable<ITransactionLineWrapper> Lines { get; }
		public IEnumerable<ITaxTransactionWrapper> TaxTransactions { get; }
		public IOrgHeaderCusCodeInfo HeaderTaxNumber(string cusCode) => HeaderTaxNumbersByCode[cusCode];

		public TransactionHeader LoadOriginalTransaction()
		{
			return OriginalTransaction;
		}

		protected readonly Dictionary<string, IOrgHeaderCusCodeInfo> HeaderTaxNumbersByCode = new Dictionary<string, IOrgHeaderCusCodeInfo>();

		public IOrgHeaderCusCodeInfo HeaderGSTInfo => HeaderTaxNumber(OrgCusCode.CodeTypes.GSTCode);

		public ZString AH_ComplianceSubType { get; }

		public IOrgHeaderCusCodeInfo HeaderVATInfo => HeaderTaxNumber(OrgCusCode.CodeTypes.VATCode);
		public ZString OH_Category { get; }
		public ZString OrgCountryCode { get; }
		public ZBool AH_IsCancelled { get; }
		public IComplianceSequenceWrapper ComplianceSequence { get; }

		TransactionHeader OriginalTransaction { get; }

		public ZString EInvoicingStatus { get; }

		public ZBool HasOriginalTransaction => OriginalTransaction == null;
	}

	#endregion

	#region ElectronicInvoicingEligibilityDecider.ITransactionHeaderWrapper

	sealed class TestTransactionHeaderWrapper : TestTransactionHeaderWrapperBase, ITransactionHeaderWrapper
	{
		public TestTransactionHeaderWrapper(ZString countryCode,
			ZString ledger,
			ZString transactionType,
			bool isDisbursementCalc = false,
			IEnumerable<bool> isNotReportables = null,
			IEnumerable<bool> isRVSs = null,
			string headerOrgGBRNumber = null,
			string headerOrgCusCodeCountryCode = null,
			string headerOrgCusCodeNumber = null,
			string complianceSubType = null,
			string transactionReference = null,
			string governmentAllocatedID = null,
			string originalTransactionReference = null,
			string originalTransactionComplianceSubType = null,
			decimal gstAmountLocalCurrency = 0m,
			bool relatedTransactionWasEligible = false,
			IReadOnlyCollection<ITransactionHeaderWrapper> originalAndRelatedTransactions = null,
			string orgCategory = null,
			string orgCountryCode = null,
			ZGuid? companyPK = null,
			bool isCancelled = false,
			IEnumerable<ZString> taxSystemCodes = null,
			ZDateTime? postDate = null,
			IEnumerable<bool> isLineWithTaxRates = null,
			ZGuid? branchPK = null,
			decimal invoiceAmountLocalCurrency = 0m,
			bool hasOriginalTransaction = true,
			string originalTransactionEInvoicingStatus = null,
			IEnumerable<ITransactionLineWrapper> lines = null)
			: base(countryCode, ledger, transactionType, isDisbursementCalc, isNotReportables, isRVSs, headerOrgCusCodeCountryCode, headerOrgCusCodeNumber, complianceSubType, transactionReference, governmentAllocatedID, gstAmountLocalCurrency, orgCategory, orgCountryCode, companyPK, isCancelled, taxSystemCodes, postDate, isLineWithTaxRates, branchPK: branchPK, invoiceAmountLocalCurrency: invoiceAmountLocalCurrency, lines: lines)
		{
			if (hasOriginalTransaction)
			{
				OriginalTransaction = new TestTransactionHeaderWrapperBase(countryCode, ledger, transactionType,
																			isDisbursementCalc: isDisbursementCalc,
																			isNotReportables: isNotReportables,
																			isRVSs: isRVSs,
																			headerOrgCusCodeCountryCode: headerOrgCusCodeCountryCode,
																			headerOrgCusCodeNumber: headerOrgCusCodeNumber,
																			complianceSubType: originalTransactionComplianceSubType,
																			gstAmountLocalCurrency: gstAmountLocalCurrency,
																			transactionReference: originalTransactionReference,
																			governmentAllocatedID: governmentAllocatedID,
																			invoiceAmountLocalCurrency: invoiceAmountLocalCurrency,
																			eInvoicingStatus: originalTransactionEInvoicingStatus);
			}

			AnyRelatedTransactionsWereEligible = relatedTransactionWasEligible;
			OriginalAndRelatedTransactions = originalAndRelatedTransactions ?? Array.Empty<ITransactionHeaderWrapper>();
			HeaderTaxNumbersByCode[OrgCusCode.CodeTypes.GovBusinessCode] = new TestOrgHeaderCusCodeInfo(string.Empty, headerOrgGBRNumber, headerOrgCusCodeCountryCode, OrgCusCode.CodeTypes.GovBusinessCode);
			HasSubmitPivotForOriginalTransaction = !originalTransactionReference.IsNullOrEmpty();
		}

		public ITransactionHeaderWrapperBase OriginalTransaction { get; }

		public ZBool IsWritingOff => false;

		public ZBool IsReverseTransaction => false;
		public ZBool IsAmendingCreditNote => false;

		public ZBool HasSubmitPivotForOriginalTransaction { get; }

		readonly bool AnyRelatedTransactionsWereEligible;
		public bool GetAnyRelatedTransactionsWereEligible() => AnyRelatedTransactionsWereEligible;

		readonly IReadOnlyCollection<ITransactionHeaderWrapper> OriginalAndRelatedTransactions;
		public IReadOnlyCollection<ITransactionHeaderWrapper> GetOriginalAndRelatedTransactions() => OriginalAndRelatedTransactions;
	}

	#endregion

	#region ElectronicInvoicingEligibilityDecider.ITaxTransactionWrapper

	sealed class TestTaxTransactionWrapper : ITaxTransactionWrapper
	{
		public TestTaxTransactionWrapper(ZString taxSystemCode)
		{
			TaxSystemCode = taxSystemCode;
		}

		public ZString TaxSystemCode { get; }
	}

	#endregion

	#region ElectronicInvoicingEligibilityDecider.IComplianceDocumentHeaderWrapper

	sealed class TestComplianceDocumentHeaderWrapper : IComplianceDocumentHeaderWrapper
	{
		public TestComplianceDocumentHeaderWrapper(ZString countryCode, ZString ledger, ZString transactionType, ZString complianceSubType)
		{
			Company = new TestGlbCompanyWrapper(countryCode);
			ADH_Ledger = ledger;
			ADH_TransactionType = transactionType;
			ADH_ComplianceSubType = complianceSubType;
		}

		public IGlbCompanyWrapper Company { get; }

		public ZString ADH_Ledger { get; private set; }

		public ZString ADH_TransactionType { get; private set; }

		public ZString ADH_ComplianceSubType { get; private set; }
	}

	#endregion

	#region ElectronicInvoicingEligibilityDecider.IGlbCompanyWrapper

	sealed class TestGlbCompanyWrapper : IGlbCompanyWrapper
	{
		public TestGlbCompanyWrapper(ZString code, ZGuid? companyPK = null)
		{
			CountryCode = new TestRefCountryWrapper(code);
			PK = companyPK != null ? companyPK.Value : ZGuid.Empty;
		}

		public IRefCountryWrapper CountryCode { get; }

		public ZGuid PK { get; }
	}

	#endregion

	#region ElectronicInvoicingEligibilityDecider.IGlbBranchWrapper

	sealed class TestGlbBranchWrapper : IGlbBranchWrapper
	{
		public TestGlbBranchWrapper(ZGuid? branchPK = null)
		{
			PK = branchPK != null ? branchPK.Value : ZGuid.Empty;
		}

		public ZGuid PK { get; }
	}

	#endregion

	#region ElectronicInvoicingEligibilityDecider.IRefCountryWrapper

	sealed class TestRefCountryWrapper : IRefCountryWrapper
	{
		public TestRefCountryWrapper(ZString code)
		{
			Code = code;
		}

		public ZString Code { get; }
	}

	#endregion

	#region ElectronicInvoicingEligibilityDecider.ITransactionLineWrapper

	sealed class TestTransactionLineWrapper : ITransactionLineWrapper
	{
		public TestTransactionLineWrapper(bool isNotReportable = false, bool isRVS = false, bool isLineWithTaxRate = true)
		{
			if (isLineWithTaxRate)
			{
				TaxRate = new TestTaxRateWrapper(isNotReportable, isRVS);
			}
		}

		public TestTransactionLineWrapper(ITaxRateWrapper taxRate)
		{
			TaxRate = taxRate;
		}

		public ITaxRateWrapper TaxRate { get; }

		public ZShort PostingGroupID { get; }
	}

	#endregion

	#region ElectronicInvoicingEligibilityDecider.ITaxRateWrapper

	sealed class TestTaxRateWrapper : ITaxRateWrapper
	{
		public TestTaxRateWrapper(bool isNotReportable = false, bool isRVS = false, string taxType = null)
		{
			IsNotReportable = isNotReportable;
			IsRVS = isRVS;
			TaxType = taxType;
		}

		public bool IsNotReportable { get; }
		public bool IsRVS { get; }
		public ZString TaxType { get; }
	}

	#endregion

	#region ElectronicInvoicingEligibilityDecider.IOrgHeaderCusCodeInfo

	sealed class TestOrgHeaderCusCodeInfo : IOrgHeaderCusCodeInfo
	{
		public TestOrgHeaderCusCodeInfo(string code, string number, string countryCode, string orgCusCode)
		{
			Code = code;
			Number = number;
			CountryCode = countryCode;
			OrgCusCode = orgCusCode;
		}

		public string Code { get; }

		public string Number { get; }

		public string CountryCode { get; }

		public string OrgCusCode { get; }
	}

	#endregion

	#region ElectronicInvoicingEligibilityDecider.IComplianceSequenceWrapper

	sealed class TestComplianceSequenceWrapper : IComplianceSequenceWrapper
	{
		public TestComplianceSequenceWrapper(int maximumNumberDigits)
		{
			XD_MaximumNumberDigits = maximumNumberDigits;
		}

		public ZInt XD_MaximumNumberDigits { get; }
	}

	#endregion

	#endregion
}

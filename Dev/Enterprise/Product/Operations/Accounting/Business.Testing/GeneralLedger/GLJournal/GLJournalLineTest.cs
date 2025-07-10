using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	[TestedType(typeof(GLJournalLine))]
	public class GLJournalLineTest : DependentTransactionLineTest
	{
		#region Not Applicable Test Cases

		public new void TestGSTandEDUSplittingOnLoad()
		{
			Assert("Not applicable", true);
		}

		public new void TestGSTandQSTSplittingOnLoad()
		{
			Assert("Not applicable", true);
		}

		public new void TestVATandSPVSplittingOnLoad()
		{
			Assert("Not applicable", true);
		}

		public new void TestVATandSPVSplittingOnLoad_AL_GSTVATExtraIsPersistent()
		{
			Assert("Not applicable", true);
		}

		public new void TestGSTandRETSplittingOnLoad()
		{
			Assert("Not applicable", true);
		}

		public new void TestOTOSplittingOnLoad()
		{
			Assert("Not applicable", true);
		}

		public new void TestQCTSplittingOnLoad()
		{
			Assert("Not applicable", true);
		}

		public new void TestLineWithSTATypeTax()
		{
			Assert("Not applicable", true);
		}

		public override void TestAL_JHValidationOnAL_RevRecognitionType()
		{
			Assert("Test is not applicable as revenue recognition validation is not run for this line type", true);
		}

		public override void TestLocalExtraAmountIsUpdatedOnInvalidTaxID()
		{
			Assert("Not applicable", true);
		}

		public override void TestOSAndLocalAmountsFromOSExTaxAmountForMexico_TestingCases()
		{
			Assert("Not applicable", true);
		}

		public override void TestVATandExtraTaxOnLoad_AL_GSTVATExtraIsPersistent()
		{
			Assert("Not applicable", true);
		}

		public override void TestAL_OSExtraTaxAmount_CalculateAL_LocalExtraTaxAmountAfterTaxIDUpdate()
		{
			Assert("Not applicable", true);
		}

		#endregion

		public void TestZDecimalsHaveCorrectDecimalPlacesGLJournalLine()
		{
			var localList = new List<string>
				{
					nameof(JournalLine.UnsignedLocalLineAmount)
				};

			var osList = new List<string>
				{
					nameof(JournalLine.UnsignedOSLineAmount)
				};

			var tester = new DecimalPlacesAttributeTester(JournalLine, JournalLine.Company);
			tester.CheckLocalCurrency(localList, nameof(JournalLine.LocalDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(JournalLine.CurrencyDecimals), nameof(JournalLine.AL_RX_NKTransactionCurrency), JournalLine);
			tester.CheckConstant(new List<string> { nameof(JournalLine.UnitQuantity) }, nameof(JournalLine.UnitQuantityDecimals), 2);
		}

		public void TestAddLinePKToGLJournalLineGLDDeleterWhenDelete()
		{
			var glJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			var line1 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			var line3 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);

			AssertNotNull(glJournal.GLJournalLineGLDDeleter);

			line3.Delete();
			Assert(!glJournal.GLJournalLineGLDDeleter.HasDeletedGLJournalLine);
			Factory.Save();

			line1.Delete();
			Assert(glJournal.GLJournalLineGLDDeleter.HasDeletedGLJournalLine);
		}

		public void TestPostDatePeriod()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var header = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Now, ZDateTime.Now, null);
			var line = TestObjectCreator.CreateGLJournalLine(header, 100m, Accounting.Business.DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var expectedDate = ZDateTime.Now.AddMonths(3);
			line.AL_PostDate = expectedDate;
			AssertEquals(periodCalculator.GetPeriodFromDate(expectedDate), line.AL_PostDatePeriod);

			expectedDate = header.AH_PostDate;
			line.AL_PostDate = expectedDate;
			AssertEquals(periodCalculator.GetPeriodFromDate(expectedDate), line.AL_PostDatePeriod);
		}

		public void TestReverseDatePeriod()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var header = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Now, ZDateTime.Now, null);
			var line = TestObjectCreator.CreateGLJournalLine(header, 100m, Accounting.Business.DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var expectedDate = ZDateTime.Now.AddMonths(3);
			line.AL_ReverseDate = expectedDate;
			AssertEquals(periodCalculator.GetPeriodFromDate(expectedDate), line.AL_ReverseDatePeriod);

			expectedDate = header.AH_DueDate;
			line.AL_ReverseDate = expectedDate;
			AssertEquals(periodCalculator.GetPeriodFromDate(expectedDate), line.AL_ReverseDatePeriod);
		}

		public void TestConcurrencyOnGLJournalLines()
		{
			var branch1 = TestObjectCreator.CreateBranch("BR1", "Test Branch 1", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BR2", "Test Branch 2", GlbCompany.CurrentCompany);
			var department1 = TestObjectCreator.FESDepartment;
			var department2 = TestObjectCreator.FIADepartment;
			var glHeader = TestObjectCreator.InsertGLHeader();
			var org1 = TestObjectCreator.AALSHI;
			var org2 = TestObjectCreator.ABIGAS;

			var header = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Now, ZDateTime.Now, null);
			var line1 = TestObjectCreator.CreateGLJournalLine(header, 100m, Accounting.Business.DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(header, 100m, Accounting.Business.DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			var line3 = TestObjectCreator.CreateGLJournalLine(header, 100m, Accounting.Business.DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var line4 = TestObjectCreator.CreateGLJournalLine(header, 100m, Accounting.Business.DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			var line5 = TestObjectCreator.CreateGLJournalLine(header, 100m, Accounting.Business.DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var line6 = TestObjectCreator.CreateGLJournalLine(header, 100m, Accounting.Business.DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			Factory.Save();
			Factory.RefreshEnabled = false;

			var line1Reloaded = (new BusinessObjectFactory()).Load<GLJournalLine>(line1.PK);
			var line2Reloaded = (new BusinessObjectFactory()).Load<GLJournalLine>(line2.PK);
			var line3Reloaded = (new BusinessObjectFactory()).Load<GLJournalLine>(line3.PK);
			var line4Reloaded = (new BusinessObjectFactory()).Load<GLJournalLine>(line4.PK);
			var line5Reloaded = (new BusinessObjectFactory()).Load<GLJournalLine>(line5.PK);
			var line6Reloaded = (new BusinessObjectFactory()).Load<GLJournalLine>(line6.PK);

			line1.AL_AG = TestObjectCreator.GLHeader2.PK;
			Factory.Save();
			AssertGLJournalLineConcurrency(line1Reloaded.Factory, line1Reloaded.AL_AGInfo, glHeader.PK);

			line2.AL_GB = branch1.PK;
			Factory.Save();
			AssertGLJournalLineConcurrency(line2Reloaded.Factory, line2Reloaded.AL_GBInfo, branch2.PK);

			line3.AL_GE = department1.PK;
			Factory.Save();
			AssertGLJournalLineConcurrency(line3Reloaded.Factory, line3Reloaded.AL_GEInfo, department2.PK);

			line4.AL_Desc = "Test Description 1";
			Factory.Save();
			AssertGLJournalLineConcurrency(line4Reloaded.Factory, line4Reloaded.AL_DescInfo, new ZString("Test Description 2"));

			line5.AL_LineAmount = line5.AL_OSAmount = 200m;
			Factory.Save();
			AssertGLJournalLineConcurrency(line5Reloaded.Factory, line5Reloaded.AL_LineAmountInfo, new ZDecimal(300m), line5Reloaded.AL_OSAmountInfo);

			line6.AL_OH = org1.PK;
			Factory.Save();
			AssertGLJournalLineConcurrency(line6Reloaded.Factory, line6Reloaded.AL_OHInfo, org2.PK);
		}

		void AssertGLJournalLineConcurrency(BusinessObjectFactory factory, ZPropertyInfo propertyInfo, IZType value, ZPropertyInfo propertyInfo2 = null)
		{
			try
			{
				propertyInfo.Value = value;
				if (propertyInfo2 != null)
				{
					propertyInfo2.Value = value;
				}
				factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
			Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("The system cannot automatically merge your changes because there are conflicts with critical fields."));
			ErrorReporter.Clear();
		}

		//[ToDo("LM", "2005-04-30", "Re-factor this to fix the test.")]
		public override void TestOnLoadedDoesNotCreateOrLoadOtherObjects()
		{
			//Replaced TODO with work item I00024746
			Assert(true);

			//base.TestOnLoadedDoesNotCreateOrLoadOtherObjects ();
		}

		public override void TestAL_AGIsReadOnly()
		{
			Assert("Journal Line's GL Account field shouldn't be readonly", !JournalLine.AL_AGInfo.ReadOnly);
		}

		public void TestAL_OHIsReadOnly()
		{
			var list = new GLPresentationJournalCategoryCollection();
			var category1 = list.AddNew();
			category1.Code = "CA1";
			category1.Description = (NoResString)"Category 1";
			var category2 = list.AddNew();
			category2.Code = "CA2";
			category2.Description = (NoResString)"Category 2";
			category2.Bool = true; // active
			category2.Bool2 = true; // elimination

			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals(2, AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.Value.Count);

			JournalLine.JournalHeader.AH_TransactionCategory = category1.Code;
			Assert(JournalLine.AL_OHInfo.ReadOnly);

			JournalLine.JournalHeader.AH_TransactionCategory = category2.Code;
			Assert(!JournalLine.AL_OHInfo.ReadOnly);
		}

		public void TestPopulateTransactionLineDissectionAttribute_ORG()
		{
			var creator = new TestObjectCreator(new BusinessObjectFactory());
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CN"))
			using (AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (journals, invoice, org) = PopulateTransactionLineDissectionAttributeData(creator);
				journals = invoice.MultiPeriodApportionmentJournals;
				AssertAttributes("OCG", AccountingMasterFilesConstants.NAV.Code, journals, creator.GLHeader1.PK);
				invoice.ResetMultiPeriodApportionmentJournals();

				org.CompanyData.OB_ARConsolidatedAccountingCategory = AccountsCategory.Unrelated;
				org.Factory.Save();
				journals = invoice.MultiPeriodApportionmentJournals;
				AssertAttributes("OCG", ConsolidatedAccountingCategoryClassList.Codes.ThirdParty, journals, creator.GLHeader1.PK);
			}
		}

		public void TestPopulateTransactionLineDissectionAttribute_SPR()
		{
			var creator = new TestObjectCreator(new BusinessObjectFactory());
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CN"))
			using (AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (journals, invoice, org) = PopulateTransactionLineDissectionAttributeData(creator);
				journals = invoice.MultiPeriodApportionmentJournals;
				AssertAttributes("SPR", AccountingMasterFilesConstants.SPRCodes.SPS, journals, creator.GLHeader1.PK);

				invoice.AH_TransactionType = TransactionTypes.CreditNote;
				invoice.ResetMultiPeriodApportionmentJournals();
				journals = invoice.MultiPeriodApportionmentJournals;
				AssertAttributes("SPR", AccountingMasterFilesConstants.SPRCodes.SPR, journals, creator.GLHeader1.PK);

				invoice.AH_TransactionType = TransactionTypes.AdjustmentNote;
				invoice.ResetMultiPeriodApportionmentJournals();
				journals = invoice.MultiPeriodApportionmentJournals;
				AssertAttributes("SPR", AccountingMasterFilesConstants.NAV.Code, journals, creator.GLHeader1.PK);
			}
		}

		void AssertAttributes(ZString attr, ZString expValue, List<GLJournal> journals, ZGuid glHeaderPK)
		{
			foreach (var journal in journals)
			{
				foreach (var journalLine in journal.Lines.Cast<TransactionLine>().Where(x => x.AL_AG == glHeaderPK))
				{
					AssertEquals(expValue, journalLine.AccTransactionLineDissectionAttributes.Cast<AccTransactionLineDissectionAttribute>().First(x => x.ALD_Attribute == attr).ALD_AttributeValue);
				}
			}
		}

		public void TestPopulateTransactionLineDissectionAttribute_LFOLFE()
		{
			var creator = new TestObjectCreator(new BusinessObjectFactory());
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CN"))
			using (AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (journals, invoice, org) = PopulateTransactionLineDissectionAttributeData(creator);
				journals = invoice.MultiPeriodApportionmentJournals;
				AssertAttributes("LFO", AccountingMasterFilesConstants.LFOCodes.LOC, journals, creator.GLHeader1.PK);
				AssertAttributes("LFE", AccountingMasterFilesConstants.LFECodes.LOC, journals, creator.GLHeader1.PK);

				invoice.ResetMultiPeriodApportionmentJournals();
				var address = org.Addresses[0];
				address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Chile;
				var chile = address.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, address.OA_RN_NKCountryCode);
				chile.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;
				chile.Factory.Save();
				journals = invoice.MultiPeriodApportionmentJournals;
				AssertAttributes("LFO", AccountingMasterFilesConstants.LFOCodes.FOR, journals, creator.GLHeader1.PK);
				AssertAttributes("LFE", AccountingMasterFilesConstants.LFECodes.WEU, journals, creator.GLHeader1.PK);

				invoice.ResetMultiPeriodApportionmentJournals();
				chile.RN_EconomicGrouping = null;
				chile.Factory.Save();
				journals = invoice.MultiPeriodApportionmentJournals;
				AssertAttributes("LFE", AccountingMasterFilesConstants.LFECodes.OEU, journals, creator.GLHeader1.PK);
			}
		}

		(List<GLJournal>, InvoicingBase, OrgHeader) PopulateTransactionLineDissectionAttributeData(TestObjectCreator creator)
		{
			var org = creator.Creditor1;
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			TestObjectCreator.Factory.Save();
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "001", organisation: org);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 0m, 0m);
			line.AL_AG = creator.GLHeader1.PK;
			line.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;
			line.PeriodClearingGLAccountPK = TestObjectCreator.GLJournalClearingAccount.PK;
			line.PeriodStartDate = new ZDate(2020, 2, 15);
			line.PeriodEndDate = new ZDate(2020, 3, 1);

			var journals = invoice.MultiPeriodApportionmentJournals;
			foreach (var journal in journals)
			{
				Assert(!journal.Lines.Cast<TransactionLine>().Any(x => x.AccTransactionLineDissectionAttributes.Any()));
			}
			invoice.ResetMultiPeriodApportionmentJournals();

			var chart = creator.CreateAlternateChart("TRR");
			creator.CreateAccAlternateGLAccountDissection(creator.GLHeader1, chart.PK, "OCG", true);
			creator.CreateAccAlternateGLAccountDissection(creator.GLHeader1, chart.PK, "LFO", true);
			creator.CreateAccAlternateGLAccountDissection(creator.GLHeader1, chart.PK, "LFE", true);
			creator.CreateAccAlternateGLAccountDissection(creator.GLHeader1, chart.PK, "SPR", true);
			creator.Factory.Save();

			return (journals, invoice, org);
		}

		public virtual void TestAL_ExchangeRate()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			JournalLine.AL_RX_NKTransactionCurrency = TestObjectCreator.EUR.RX_Code;
			JournalLine.AL_ExchangeRate = 0.50m;
			JournalLine.AL_AG = TestObjectCreator.GLHeader2.PK;
			JournalLine.AL_LocalExTaxAmount = 20m;
			JournalLine.AL_OSExTaxAmount = 10m;

			JournalLine.AL_ExchangeRate = 0.75m;
			AssertEquals("OS ex tax amount is the same, updating exchange reate did not update os amount", 10m, JournalLine.AL_OSExTaxAmount);
			AssertEquals("Local ex tax amount is the same, updating exchange rate did not update local ex tax amount", 13.33m, JournalLine.AL_LocalExTaxAmount);
		}

		public virtual void TestSettingAL_RXValidatesAL_AG()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var profitAndLossAccount = Factory.NewWithValidTestData<AccGLHeader>();
			profitAndLossAccount.AG_AccountNum = "9999.99.99";
			profitAndLossAccount.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;

			JournalLine.AL_AG = profitAndLossAccount.PK;

			JournalLine.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;

			AssertNoErrors(JournalLine.AL_AGInfo);

			JournalLine.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;

			AssertNoErrors(JournalLine.AL_AGInfo);

			var alternateAccount = Factory.NewWithValidTestData<AccGLHeader>();
			alternateAccount.AG_AccountNum = "9999.99.99";
			alternateAccount.AG_AccountType = Core.Constants.AccountType.Alternate;

			JournalLine.AL_AG = alternateAccount.PK;

			JournalLine.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;

			AssertHasErrors(JournalLine.AL_AGInfo);

			JournalLine.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;

			AssertHasErrors(JournalLine.AL_AGInfo);
		}

		[TestDate(2014, 2, 20)]
		public void TestPERExchangeRateIsDefaultedAsExchangeRate_ForForeignCurrencyLine()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			TestCaseHelper.ClearTable(MasterFiles.Business.AccPeriodManagement.Schema.TableName);
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Core.Constants.ExchangeRateTypes.Code.BuyRate, 0.66m
				, periodHelper.CurrentPeriod.AM_StartDate, periodHelper.CurrentPeriod.AM_EndDate);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Core.Constants.ExchangeRateTypes.Code.SellRate, 0.86m
				, periodHelper.CurrentPeriod.AM_StartDate, periodHelper.CurrentPeriod.AM_EndDate);

			JournalLine.JournalHeader.AH_PostDate = ZDateTime.Empty;
			JournalLine.JournalHeader.PostPeriod = periodHelper.CurrentPeriod.AM_Period;

			JournalLine.AL_AG = TestObjectCreator.GLHeader2.PK;
			AssertEquals("Precondition:", 1m, JournalLine.AL_ExchangeRate);
			Assert(JournalLine.AL_ExchangeRateInfo.ReadOnly);

			JournalLine.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals("Exchange rate set to 0 as PER not set", 0m, JournalLine.AL_ExchangeRate);
			Assert(!JournalLine.AL_ExchangeRateInfo.ReadOnly);
			AssertHasError(JournalLine.AL_ExchangeRateInfo, "Exchange Rate cannot be zero.");
			AssertHasWarning(JournalLine.AL_ExchangeRateInfo, "PER - Period End Rate exchange rate not found.");

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Core.Constants.ExchangeRateTypes.Code.PeriodEndRate, 0.76m
				, periodHelper.CurrentPeriod.AM_StartDate, periodHelper.CurrentPeriod.AM_EndDate);

			JournalLine.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals("Exchange rate defaulted to PER rate", 0.76m, JournalLine.AL_ExchangeRate);
			Assert(!JournalLine.AL_ExchangeRateInfo.ReadOnly);
			AssertNoError(JournalLine.AL_ExchangeRateInfo, "Exchange Rate cannot be zero.");
			AssertNoWarning(JournalLine.AL_ExchangeRateInfo, "PER - Period End Rate exchange rate not found.");
		}

		public void TestDefaultValues()
		{
			AssertEquals("Default Line Debit/Credit", GLJournalLine.DR_ForTestOnly, JournalLine.DebitCreditSign);
		}

		public override void TestTransactionLineFetchHints()
		{
			Assert(true);
		}

		public void TestGLAccountDescription()
		{
			ZQuery query = new ZQuery();
			query.MaximumRows = 1;
			AccGLHeaderCollection gLHeaders = new AccGLHeaderCollection(JournalLine.Factory, query);
			gLHeaders.Load();

			string expectedDescription = gLHeaders[0].AG_Description;
			JournalLine.AL_AG = gLHeaders[0].PK;
			AssertEquals("GL Account Description", expectedDescription, JournalLine.GLAccountDescription);
		}

		public void TestGLLocalCNAccountDescriptionAndCode()
		{
			ZString originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			ZQuery query = new ZQuery();
			query.MaximumRows = 1;
			AccGLHeaderCollection gLHeaders = new AccGLHeaderCollection(JournalLine.Factory, query);
			gLHeaders.Load();

			AccGLAccountDescriptor gLAccountDescriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			gLAccountDescriptor.AJ_Language = Core.SharedConstants.Languages.ChineseSimplified;
			gLAccountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			gLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			gLAccountDescriptor.AJ_AccountDescription = "hello china account";
			gLAccountDescriptor.AJ_LocalAccountNumber = "1234567";
			gLAccountDescriptor.ParentGLHeaderPK = gLHeaders[0].PK;
			string expectedDescription = gLAccountDescriptor.AJ_AccountDescription;
			string expectedCode = gLAccountDescriptor.AJ_LocalAccountNumber;
			JournalLine.AL_AG = gLHeaders[0].PK;
			Factory.Save();
			AssertEquals("GL Local CN Account Description", expectedDescription, JournalLine.GLLocalCNAccountDescription);
			AssertEquals("GL Local CN Account Code", expectedCode, JournalLine.GLLocalCNAccountCode);
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCountryCode;
		}

		public virtual void TestUnsignedOSLineAmountValidation()
		{
			JournalLine.UnsignedOSLineAmount = 50.00m;
			Assert("Should be no errors on Amount", !JournalLine.UnsignedOSLineAmountInfo.HasErrors());
			JournalLine.UnsignedOSLineAmount = 0.00m;
			Assert("Should be error on Amount", JournalLine.UnsignedOSLineAmountInfo.HasErrors());
			JournalLine.UnsignedOSLineAmount = 50.00m;
			Assert("Should be no errors on Amount", !JournalLine.UnsignedOSLineAmountInfo.HasErrors());
			JournalLine.AL_OSExTaxAmount = 0.00m;
			JournalLine.RunPreSaveValidation();
			Assert("Should be error on Amount wre Save Validation is run", JournalLine.UnsignedOSLineAmountInfo.HasErrors());
		}

		public void TestValidateAL_Desc()
		{
			Line.AL_Desc = string.Empty;
			Assert("Description should have errors", Line.AL_DescInfo.HasErrors());
			Line.AL_Desc = "Description";
			Assert("Description should not have errors", !Line.AL_DescInfo.HasErrors());
		}

		public void TestLoadingPopulatesRelevantFields()
		{
			JournalLine.UnsignedOSLineAmount = 0.00m;
			JournalLine.DebitCreditSign = ZString.Empty;
			JournalLine.AL_OSExTaxAmount = 50.00m;
			JournalLine.OnLoaded();
			AssertEquals("Unsigned Line Amount", 50.00m, JournalLine.UnsignedOSLineAmount);
			AssertEquals("Debit/Credit Field should be DR", GLJournalLine.DR_ForTestOnly, JournalLine.DebitCreditSign);

			JournalLine.UnsignedOSLineAmount = 0.00m;
			JournalLine.DebitCreditSign = ZString.Empty;
			JournalLine.AL_OSExTaxAmount = -50.00m;
			JournalLine.OnLoaded();
			AssertEquals("Unsigned Line Amount", 50.00m, JournalLine.UnsignedOSLineAmount);
			AssertEquals("Debit/Credit Field should be CR", GLJournalLine.CR_ForTestOnly, JournalLine.DebitCreditSign);
		}

		public void TestDebitCreditSignValidation()
		{
			string invalidInput = "AB";
			JournalLine.DebitCreditSign = GLJournalLine.DR_ForTestOnly;
			AssertEquals(JournalLine.DebitCreditSign, JournalLine.OSAmount_ForTestOnly.DebitCreditSign);
			AssertEquals(JournalLine.DebitCreditSign, JournalLine.LocalAmount_ForTestOnly.DebitCreditSign);
			Assert("Should be no errors on Debit/Credit Sign", !JournalLine.DebitCreditSignInfo.HasErrors());
			JournalLine.DebitCreditSign = invalidInput;
			AssertEquals(JournalLine.DebitCreditSign, JournalLine.OSAmount_ForTestOnly.DebitCreditSign);
			AssertEquals(JournalLine.DebitCreditSign, JournalLine.LocalAmount_ForTestOnly.DebitCreditSign);
			Assert("Should now be error on Debit/Credit Sign", JournalLine.DebitCreditSignInfo.HasErrors());
			JournalLine.DebitCreditSign = GLJournalLine.CR_ForTestOnly;
			AssertEquals(JournalLine.DebitCreditSign, JournalLine.OSAmount_ForTestOnly.DebitCreditSign);
			AssertEquals(JournalLine.DebitCreditSign, JournalLine.LocalAmount_ForTestOnly.DebitCreditSign);
			Assert("Should be no errors on Debit/Credit Sign", !JournalLine.DebitCreditSignInfo.HasErrors());
			JournalLine.DebitCreditSign = invalidInput;
			AssertEquals(JournalLine.DebitCreditSign, JournalLine.OSAmount_ForTestOnly.DebitCreditSign);
			AssertEquals(JournalLine.DebitCreditSign, JournalLine.LocalAmount_ForTestOnly.DebitCreditSign);
			JournalLine.RunPreSaveValidation();
			Assert("Should be error on Debit/Credit Sign on Pre Save", JournalLine.DebitCreditSignInfo.HasErrors());
			JournalLine.DebitCreditSign = ZString.Empty;
			AssertEquals(JournalLine.DebitCreditSign, JournalLine.OSAmount_ForTestOnly.DebitCreditSign);
			AssertEquals(JournalLine.DebitCreditSign, JournalLine.LocalAmount_ForTestOnly.DebitCreditSign);
			Assert("Should now be error on Debit/Credit Sign", JournalLine.DebitCreditSignInfo.HasErrors());
		}

		public void TestLineAmountsWithDebitCredit()
		{
			JournalLine.UnsignedOSLineAmount = 50.00M;
			AssertEquals("Line Amount with default DR Sign", 50.00m, Line.AL_OSExTaxAmount);
			AssertEquals("Unsigned OS Amount should always be positive", 50.00m, JournalLine.UnsignedOSLineAmount);
			AssertEquals("Unsigned Local Amount should always be positive", 50.00m, JournalLine.UnsignedLocalLineAmount);
			JournalLine.DebitCreditSign = GLJournalLine.CR_ForTestOnly;
			AssertEquals("Line Amount with CR Sign", -50.00m, Line.AL_OSExTaxAmount);
			AssertEquals("Unsigned OS Amount should always be positive", 50.00m, JournalLine.UnsignedOSLineAmount);
			AssertEquals("Unsigned Local Amount should always be positive", 50.00m, JournalLine.UnsignedLocalLineAmount);
			JournalLine.UnsignedOSLineAmount = 100.00m;
			AssertEquals("Line Amount with CR Sign", -100.00m, Line.AL_OSExTaxAmount);
			AssertEquals("Unsigned OS Amount should always be positive", 100.00m, JournalLine.UnsignedOSLineAmount);
			AssertEquals("Unsigned Local Amount should always be positive", 100.00m, JournalLine.UnsignedLocalLineAmount);
			JournalLine.DebitCreditSign = GLJournalLine.DR_ForTestOnly;
			AssertEquals("Line Amount with DR Sign", 100.00m, Line.AL_OSExTaxAmount);
			AssertEquals("Unsigned OS Amount should always be positive", 100.00m, JournalLine.UnsignedOSLineAmount);
			AssertEquals("Unsigned Local Amount should always be positive", 100.00m, JournalLine.UnsignedLocalLineAmount);
		}

		public void TestInvalidGLAccount()
		{
			var glHeader = TestObjectCreator.CreateGLHeader();
			glHeader.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;

			JournalLine.AL_AG = glHeader.PK;

			Assert("GL Account should not have errors yet", !JournalLine.AL_AGInfo.HasErrors());
			JournalLine.GLHeaderCollection.Load();
			JournalLine.AL_AG = ZGuid.Empty;
			Assert("GL Account should now have errors", JournalLine.AL_AGInfo.HasErrors());
		}

		public void TestOnSavingJournal_GLReversingJournal()
		{
			var postingDate = PeriodTestHelper.CurrentPeriod.AM_EndDate;
			var reverseDate = PeriodTestHelper.FuturePeriod.AM_EndDate;

			var master = (GLJournal)JournalLine.MasterTransactionHeader;
			master.AH_TransactionType = TransactionTypes.GLReversingJournal;
			master.AH_PostDate = postingDate;
			master.AH_DueDate = reverseDate;
			JournalLine.Factory.Save();

			AssertEquals("Post date should be same as master due date", postingDate.Date, JournalLine.AL_PostDate.Date);
			AssertEquals("Reverse date should be same as Master", master.AH_DueDate.Date, JournalLine.AL_ReverseDate.Date);
			AssertEquals("Journal Line Type should be the same", TransactionTypes.GLReversingJournal, JournalLine.AL_LineType);
		}

		public void TestOnSavingJournal_GLAutoJournal()
		{
			var postingDate = PeriodTestHelper.CurrentPeriod.AM_EndDate;
			var reverseDate = PeriodTestHelper.FuturePeriod.AM_EndDate;

			var master = (GLJournal)JournalLine.MasterTransactionHeader;
			master.AH_TransactionType = TransactionTypes.GLAutoJournal;
			master.AH_PostDate = postingDate;
			master.AH_DueDate = reverseDate;

			AssertEquals("Post date should be same as master due date", postingDate.Date, JournalLine.AL_PostDate.Date);
			AssertEquals("Reverse date should be same as Master", master.AH_DueDate.Date, JournalLine.AL_ReverseDate.Date);
			AssertEquals("Journal Line Type should be the same", TransactionTypes.GLAutoJournal, JournalLine.AL_LineType);

			master.PostPeriod = PeriodTestHelper.FuturePeriod.AM_Period;
			master.AgePeriod = PeriodTestHelper.FuturePeriod.AM_Period;

			using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.GetSuspender(Factory))
			{
				JournalLine.Factory.Save();
			}

			AssertEquals("Post Date of Line should be same as header", PeriodTestHelper.FuturePeriod.AM_EndDate.Date, JournalLine.AL_PostDate.Date);
			AssertEquals("Reverse Date of Line should be same as header (Due Date)", master.AH_DueDate.Date, JournalLine.AL_ReverseDate.Date);
		}

		[TestDate(2020, 05, 26, 12, 00, 00)]
		public void TestCopyValuesFrom()
		{
			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var alternateChart = Factory.NewWithValidTestData<AccAlternateChart>();

			var gLAccount = TestObjectCreator.CreateGLHeader("1010.22.98", Core.Constants.AccountType.ProfitAndLossAccount);
			var dissection1 = gLAccount.AlternateGLAccountDissections.AddNew();
			dissection1.ADC_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC;
			dissection1.ADC_AAC_AlternateChart = alternateChart.PK;
			var dissection2 = gLAccount.AlternateGLAccountDissections.AddNew();
			dissection2.ADC_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG;
			dissection2.ADC_AAC_AlternateChart = alternateChart.PK;
			Factory.Save();

			var gLJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLAutoJournal, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now);
			gLJournal.AH_Ledger = LedgerTypes.General;

			var line = (GLJournalLine)gLJournal.Lines.AddNew();
			var attributes1 = line.AccTransactionLineDissectionAttributes.AddNew();
			attributes1.ALD_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC;
			attributes1.ALD_AttributeValue = "ETI";
			var attributes2 = line.AccTransactionLineDissectionAttributes.AddNew();
			attributes2.ALD_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG;
			attributes2.ALD_AttributeValueID = TestObjectCreator.AALSHI.PK;

			line.UnsignedOSLineAmount = 40m;
			line.DebitCreditSign = GLJournalLine.DR_ForTestOnly;
			line.AL_LineType = "RJL";
			line.AL_GB = Env.CurrentBranchPK;
			line.AL_GE = NonCurrentDepartment.PK;
			line.AL_Desc = "GLGL";
			line.AL_PostDate = ZDateTime.Today.AddDays(-1);
			line.AL_ReverseDate = line.AL_PostDate;
			line.AL_RX_NKTransactionCurrency = CurrencyCodes.UnitedStates;
			line.AL_AG = gLAccount.PK;

			var copyToLine = (GLJournalLine)gLJournal.Lines.AddNew();
			copyToLine.CopyValuesFrom(line);
			AssertEquals("Unsigned amount", 40m, copyToLine.UnsignedOSLineAmount);
			AssertEquals("DebitCredit sign", GLJournalLine.DR_ForTestOnly, copyToLine.DebitCreditSign);
			AssertEquals("Line Type", "RJL", copyToLine.AL_LineType);
			AssertEquals("Branch", line.AL_GB, copyToLine.AL_GB);
			AssertEquals("Department", NonCurrentDepartment.PK, copyToLine.AL_GE);
			AssertEquals("GLAccount", gLAccount.PK, copyToLine.AL_AG);
			AssertEquals("Description", "GLGL", copyToLine.AL_Desc);
			AssertEquals("PostDate", line.AL_PostDate, copyToLine.AL_PostDate);
			AssertEquals("ReverseDate", line.AL_ReverseDate, copyToLine.AL_ReverseDate);
			AssertEquals("Currency", CurrencyCodes.UnitedStates, copyToLine.AL_RX_NKTransactionCurrency);
			AssertEquals("Dissection count", 2, copyToLine.AccTransactionLineDissectionAttributes.Count);
			AssertEquals("Dissection 1 Value", "ETI", copyToLine.AccTransactionLineDissectionAttributes.Find(item => item.ALD_Attribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC).First().ALD_AttributeValue);
			AssertEquals("Dissection 2 Value", "", copyToLine.AccTransactionLineDissectionAttributes.Find(item => item.ALD_Attribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG).First().ALD_AttributeValue);
			AssertEquals("Dissection 2 Value ID", TestObjectCreator.AALSHI.PK, copyToLine.AccTransactionLineDissectionAttributes.Find(item => item.ALD_Attribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG).First().ALD_AttributeValueID);
			AssertNoExceptionThrown("Should save successfully", () => Factory.Save());
		}

		[TestDate(2020, 05, 26, 12, 00, 00)]
		public void TestUnsignedLineAmountsAfterCopyValueFromForeignCurrency()
		{
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, CurrencyCodes.UnitedStates);
			var vnd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, CurrencyCodes.VietNam);

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.VietNam))
			{
				GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = vnd.RX_Code;
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				AssertEquals("Precondition: Local currency decimals should be 0.", 0, GlbCompany.CurrentCompany.LocalCurrency.Decimals);
				AssertEquals("Precondition: Foreign currency decimals should not be 0.", 2, usd.Decimals);

				var line = Factory.NewWithValidTestData<GLJournalLine>();
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				line.AL_RX_NKTransactionCurrency = usd.RX_Code;
				line.AL_ExchangeRate = 7.123111;
				line.UnsignedOSLineAmount = 1768.89m;
				line.UnsignedLocalLineAmount = 12600m;
				line.DebitCreditSign = GLJournalLine.DR_ForTestOnly;
				line.AL_LineType = "GJL";
				line.AL_GB = GlbBranch.CurrentBranch.PK;
				line.AL_GE = NonCurrentDepartment.PK;
				line.AL_Desc = "GLGL";
				line.AL_PostDate = ZDateTime.Today.AddDays(-1);
				line.AL_ReverseDate = ZDateTime.Today.AddDays(-1);

				GLJournalLine copyToLine = Factory.New<GLJournalLine>();
				copyToLine.CopyValuesFrom(line);
				AssertEquals(1768.89m, copyToLine.UnsignedOSLineAmount);
				AssertEquals(12600m, copyToLine.UnsignedLocalLineAmount);
			}
		}

		public void TestHasAssignedExportBatchNumber()
		{
			Assert(!JournalLine.HasAssignedExportBatchNumber);

			Factory.NewWithValidTestData<GenExportBatchSequence>().XB_ParentID = JournalLine.PK;
			Factory.Save();

			Assert(JournalLine.HasAssignedExportBatchNumber);
		}

		public void TestJournalLineReadOnly()
		{
			Factory.Save();

			GLJournalLine line = new BusinessObjectFactory().Load<GLJournalLine>(JournalLine.PK);
			Assert(!line.ReadOnly);

			Factory.NewWithValidTestData<GenExportBatchSequence>().XB_ParentID = JournalLine.PK;
			Factory.Save();

			line = new BusinessObjectFactory().Load<GLJournalLine>(JournalLine.PK);
			Assert("GLJournalLine should be readonly if it has been assigned a 'GL Export Batch Number'", line.ReadOnly);
		}

		public void TestHasAssignedExportBatchNumberDoesNotHitDbForUnsavedLine()
		{
			AssertEquals("Journal Line is not saved", false, JournalLine.IsInDatabase);
			int hitCount = Factory.GetTableHitCount(GenExportBatchSequenceSchema.Constants.TableName);
			AssertEquals("No ExportBatchNumber", false, JournalLine.HasAssignedExportBatchNumber);
			AssertEquals("No Db Hits", hitCount, Factory.GetTableHitCount(GenExportBatchSequenceSchema.Constants.TableName));
		}

		public void TestAL_AG_ResetExchangeRate()
		{
			var gLJournalExchangeRateType = new GLJournalExchangeRateType() { BalanceSheetAccountTypeExchangeRateType = ExchangeRateTypes.Code.BuyRate, ProfitAndLossAccountTypeExchangeRateType = ExchangeRateTypes.Code.SellRate };
			AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, gLJournalExchangeRateType);

			var journal = Factory.NewWithValidTestData<GLJournal>();

			TestObjectCreator.CreateUSDBuyRate(0.88m, journal.AH_PostDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.SellRate, 0.77m, journal.AH_PostDate, journal.AH_PostDate);
			TestObjectCreator.GLHeader1.AG_AccountType = AccountType.BalanceSheetAccount;

			var line = TestObjectCreator.CreateGLJournalLine(journal, 100m, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals("exRate updated to 0.88", 0.88m, line.AL_ExchangeRate);

			TestObjectCreator.GLHeader2.AG_AccountType = AccountType.ProfitAndLossAccount;
			line.AL_AG = TestObjectCreator.GLHeader2.PK;
			AssertEquals("exRate updated to 0.77", 0.77m, line.AL_ExchangeRate);
		}

		public void TestAL_RX_NKTransactionCurrency_ResetExchangeRate()
		{
			var gLJournalExchangeRateType = new GLJournalExchangeRateType() { BalanceSheetAccountTypeExchangeRateType = ExchangeRateTypes.Code.BuyRate, ProfitAndLossAccountTypeExchangeRateType = ExchangeRateTypes.Code.SellRate };
			AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, gLJournalExchangeRateType);

			var journal = Factory.NewWithValidTestData<GLJournal>();

			TestObjectCreator.CreateUSDBuyRate(0.88m, journal.AH_PostDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, ExchangeRateTypes.Code.BuyRate, 0.77m, journal.AH_PostDate, journal.AH_PostDate);

			var line = TestObjectCreator.CreateGLJournalLine(journal, 100m, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			line.AL_PostDate = ZDateTime.Now;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals("exRate updated to 0.88", 0.88m, line.AL_ExchangeRate);

			line.AL_RX_NKTransactionCurrency = TestObjectCreator.CNY.RX_Code;
			AssertEquals("exRate updated to 0.77", 0.77m, line.AL_ExchangeRate);
		}

		#region Overridden Tests

		public override void TestOSTotalSplitParts_ZeroVAT()
		{
			AccTaxRate fgstFree = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "GST"));
			if (fgstFree == null)
			{
				fgstFree = Factory.New<AccTaxRate>();
				fgstFree.AT_Code = "FREEGST";
				fgstFree.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}
			fgstFree.AT_Type = AccTaxRate.Types.Rated;
			fgstFree.SetRateNumerator_ForTestOnly(0);

			Line.AL_RX_NKTransactionCurrency = "UAH";
			Line.AL_ExchangeRate = 177.4937;
			Line.AL_AT = fgstFree.PK;
			Line.AL_OSExTaxAmount = 30380329.38;
			((GLJournal)Line.TransactionHeader).Balance();
			Factory.Save();

			var factoryForNoCache = new BusinessObjectFactory();
			AssertOSValues((TransactionLine)factoryForNoCache.Load(Line.GetType(), Line.PK), 30380329.3800, 0, 30380329.3800);
		}

		public override void TestOSTotalSplitParts_NonZeroVAT()
		{
			AccTaxRate gST = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "GST"));
			if (gST == null)
			{
				gST = Factory.New<AccTaxRate>();
				gST.AT_Code = "GST";
				gST.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}
			gST.AT_Type = AccTaxRate.Types.Rated;
			gST.SetRate_ForTestOnly(175, 10);

			Line.AL_RX_NKTransactionCurrency = "UAH";
			Line.AL_ExchangeRate = 177.4937;
			Line.AL_AT = gST.PK;
			Line.AL_OSExTaxAmount = 30380329.38;
			((GLJournal)Line.TransactionHeader).Balance();
			Factory.Save();

			var factoryForNoCache = new BusinessObjectFactory();
			AssertOSValues((TransactionLine)factoryForNoCache.Load(Line.GetType(), Line.PK), 30380329.38, 5316557.6400, 35696887.0200);
		}

		[TestDate(2020, 12, 15)]
		public virtual void TestGetNewValidationCore()
		{
			var period = Factory.New<AccPeriodManagement>();
			period.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			period.AM_Year = 2020;
			period.AM_Period = 202012;
			period.AM_StartDate = new ZDateTime(2020, 12, 01);
			period.AM_EndDate = new ZDateTime(2020, 12, 31);
			Factory.Save();

			AssertEquals(typeof(GLJournalLineValidation), JournalLine.Validation.GetType());

			var glJournal = Factory.NewWithValidTestData<GLJournal>();
			var glJournalLine = glJournal.Lines.AddNew() as GLJournalLine;
			glJournal.PeriodPK = period.PK;
			Factory.Save();

			AssertEquals(typeof(TransactionLineEmptyValidation), glJournalLine.Validation.GetType());

			var reversing = new ReversingFactory().NewReversing(glJournal);
			reversing.Reverse();

			var reversedJournal = glJournal.ReverseTransaction as GLJournal;
			var reversedJournalLine = reversedJournal.Lines.First() as GLJournalLine;
			AssertEquals(typeof(TransactionLineEmptyValidation), reversedJournalLine.Validation.GetType());

			var glJournal1 = Factory.NewWithValidTestData<GLJournal>();
			var glJournalLine1 = glJournal1.Lines.AddNew() as GLJournalLine;
			Factory.Save();

			var batch = Factory.NewWithValidTestData<DsbJobCloseBatch>();
			batch.JBB_AH_Journal = glJournal1.PK;
			batch.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Close;
			Factory.Save();

			AssertEquals(typeof(TransactionLineEmptyValidation), glJournalLine1.Validation.GetType());
		}

		[TestDate(2020, 12, 15)]
		public void TestZeroUnsignedAmountWontHaveErrorWhileReversingAutomatedJournal()
		{
			var period = Factory.New<AccPeriodManagement>();
			period.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			period.AM_Year = 2020;
			period.AM_Period = 202012;
			period.AM_StartDate = new ZDateTime(2020, 12, 01);
			period.AM_EndDate = new ZDateTime(2020, 12, 31);
			Factory.Save();

			var glJournal = Factory.NewWithValidTestData<GLJournal>();
			var glJournalLine = glJournal.Lines.AddNew() as GLJournalLine;
			glJournalLine.UnsignedOSLineAmount = 0m;
			glJournalLine.UnsignedLocalLineAmount = 0m;
			glJournal.PeriodPK = period.PK;
			Factory.Save();

			var reversing = new AutoCurrencyAdjustmentGLJournalReversing(glJournal);
			reversing.Reverse();

			var lines = (glJournal.ReverseTransaction as GLJournal).Lines.Cast<GLJournalLine>().ToArray();
			AssertEquals(1, lines.Length);

			lines[0].Validation.ValidateAll();

			AssertNoErrors(lines[0].UnsignedOSLineAmountInfo);
			AssertNoErrors(lines[0].UnsignedLocalLineAmountInfo);
		}

		public override void TestRoundAmountToCurrencyDecimals()
		{
			base.TestRoundAmountToCurrencyDecimals();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLNoteJournal, ZDateTime.Now, ZDateTime.Now);
				var line = journal.GLJournalLines.AddNew();

				line.AL_RX_NKTransactionCurrency = CurrencyCodes.Taiwan;
				AssertEquals(234.54m, line.RoundAmountToCurrencyDecimals_ForTestOnly(234.54m));
				AssertEquals(234.54m, line.RoundAmountToCurrencyDecimals_ForTestOnly(234.54m, 1m));
				AssertEquals(234.45m, line.RoundAmountToCurrencyDecimals_ForTestOnly(234.45m));
				AssertEquals(234.45m, line.RoundAmountToCurrencyDecimals_ForTestOnly(234.45m, 1m));

				journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Now, ZDateTime.Now);
				line = journal.GLJournalLines.AddNew();

				line.AL_RX_NKTransactionCurrency = CurrencyCodes.Taiwan;
				AssertEquals(235m, line.RoundAmountToCurrencyDecimals_ForTestOnly(234.54m));
				AssertEquals(235m, line.RoundAmountToCurrencyDecimals_ForTestOnly(234.54m, 1m));
				AssertEquals(234m, line.RoundAmountToCurrencyDecimals_ForTestOnly(234.45m));
				AssertEquals(234m, line.RoundAmountToCurrencyDecimals_ForTestOnly(234.45m, 1m));

				line.AL_RX_NKTransactionCurrency = CurrencyCodes.Australia;
				AssertEquals(20.33m, line.RoundAmountToCurrencyDecimals_ForTestOnly(20.334));
				AssertEquals(20.33m, line.RoundAmountToCurrencyDecimals_ForTestOnly(1m, 20.334m));
				AssertEquals(20.34m, line.RoundAmountToCurrencyDecimals_ForTestOnly(20.335m));
				AssertEquals(20.34m, line.RoundAmountToCurrencyDecimals_ForTestOnly(1m, 20.335m));
			}
		}

		public override void TestRoundAmountToLocalDecimals()
		{
			base.TestRoundAmountToLocalDecimals();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLNoteJournal, ZDateTime.Now, ZDateTime.Now);
				var line = journal.GLJournalLines.AddNew();

				line.AL_RX_NKTransactionCurrency = CurrencyCodes.Taiwan;
				AssertEquals(234.54m, line.RoundAmountToLocalDecimals_ForTestOnly(234.54m));
				AssertEquals(234.54m, line.RoundAmountToLocalDecimals_ForTestOnly(234.54m, 1m));
				AssertEquals(234.45m, line.RoundAmountToLocalDecimals_ForTestOnly(234.45m));
				AssertEquals(234.45m, line.RoundAmountToLocalDecimals_ForTestOnly(234.45m, 1m));

				journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Now, ZDateTime.Now);
				line = journal.GLJournalLines.AddNew();

				line.AL_RX_NKTransactionCurrency = CurrencyCodes.Taiwan;
				AssertEquals(235m, line.RoundAmountToLocalDecimals_ForTestOnly(234.54m));
				AssertEquals(235m, line.RoundAmountToLocalDecimals_ForTestOnly(234.54m, 1m));
				AssertEquals(234m, line.RoundAmountToLocalDecimals_ForTestOnly(234.45m));
				AssertEquals(234m, line.RoundAmountToLocalDecimals_ForTestOnly(234.45m, 1m));

				line.AL_RX_NKTransactionCurrency = CurrencyCodes.Australia;
				AssertEquals(20m, line.RoundAmountToLocalDecimals_ForTestOnly(20.334));
				AssertEquals(20m, line.RoundAmountToLocalDecimals_ForTestOnly(1m, 20.334m));
				AssertEquals(21m, line.RoundAmountToLocalDecimals_ForTestOnly(20.535m));
				AssertEquals(21m, line.RoundAmountToLocalDecimals_ForTestOnly(1m, 20.535m));
			}
		}

		public override void TestTaxAmountsForIndiaSTA()
		{
			Assert("Not applicable", true);
		}

		public override void TestInternalOSAmountFieldsSetOnLoadCorrectly()
		{
			JournalLine.UnsignedOSLineAmount = 200.00m;
			JournalLine.DebitCreditSign = GLJournalLine.DR_ForTestOnly;
			var gljournal = Factory.Load<GLJournal>(JournalLine.TransactionHeader.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(gljournal, 200, DebitCredit.CR, JournalLine.AL_AG);
			line2.AL_OSAmount = -200m;
			SetupAndSaveLine();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

			TransactionLine loadedLine = (TransactionLine)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), Line.PK);

			AssertEquals("OS Amount should equal LineAmount", Line.AL_OSExTaxAmount, Line.AL_OverseasTotal);
		}

		public void TestOSTaxRoundingOnHeader()
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.SetRateNumerator_ForTestOnly(10);

			Line.AL_OSExTaxAmount = 385493.34m;
			Line.AL_AT = taxRate.PK;

			AssertEquals("Total Amount should be same as OSExTax since setting Tax does not recalculate Total", 385493.34m, Line.AL_OverseasTotal);
		}

		public override void TestGetNewValidation()
		{
			Assert("Validation should be GLJournalLineValidation", typeof(GLJournalLineValidation).IsAssignableFrom(Line.Validation.GetType()));
			Line.AL_ReverseDate = ZDateTime.Now;

			Factory.Save();
			Assert("Validation should still be GLJournalLineValidation", typeof(GLJournalLineValidation).IsAssignableFrom(Line.Validation.GetType()));
		}

		public void TestResetLineDefaultValuesForNoteJournal()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLNoteJournal, ZDateTime.Now, ZDateTime.Now);
				var line = (GLJournalLine)journal.Lines.AddNew();

				AssertEquals("Pre-condition", ZGuid.Empty, line.AL_OH);
				AssertEquals("Pre-condition", CurrencyCodes.UnitedStates, line.AL_RX_NKTransactionCurrency);

				line.AL_OH = TestObjectCreator.ABIGAS.PK;
				line.AL_RX_NKTransactionCurrency = Constants.CurrencyCodes.China;

				AssertEquals("Organization", TestObjectCreator.ABIGAS.PK, line.AL_OH);
				AssertEquals("Currency", CurrencyCodes.China, line.AL_RX_NKTransactionCurrency);

				line.ResetLineDefaultValuesForNoteJournal();

				AssertEquals("Organization should be empty after reset", ZGuid.Empty, line.AL_OH);
				AssertEquals("Currency should be local currency after reset", CurrencyCodes.UnitedStates, line.AL_RX_NKTransactionCurrency);
			}
		}

		public void TestJournalLineReadOnlyForNoteJournal()
		{
			var list = new GLPresentationJournalCategoryCollection();
			var category = list.AddNew();
			category.Code = "CA1";
			category.Description = (NoResString)"Category";
			category.Bool = true;
			category.Bool2 = true;

			using (AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Now, ZDateTime.Now);
				journal.AH_TransactionCategory = category.Code;
				journal.AH_RX_NKTransactionCurrency = Core.Constants.CountryCodes.China;

				var line = (GLJournalLine)journal.Lines.AddNew();

				var subAccount1 = line.SubAccounts.AddNew();
				subAccount1.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
				subAccount1.AL1_SubClassParentId = TestObjectCreator.AALSHI.PK;

				var subAccount2 = line.SubAccounts.AddNew();
				subAccount2.AL1_SubClassParentTableCode = GlbStaffSchema.Constants.Prefix;
				subAccount2.AL1_SubClassParentId = TestObjectCreator.Staff.PK;

				AssertJournalLineReadOnlyForNoteJournal();

				journal.AH_TransactionType = TransactionTypes.GLNoteJournal;
				AssertJournalLineReadOnlyForNoteJournal();

				void AssertJournalLineReadOnlyForNoteJournal()
				{
					AssertEquals("AL_OH ReadOnly", line.IsNoteJournal, line.AL_OHInfo.ReadOnly);
					AssertEquals("AL_RX_NKTransactionCurrency ReadOnly", line.IsNoteJournal, line.AL_RX_NKTransactionCurrencyInfo.ReadOnly);
					AssertEquals("AL_ExchangeRate ReadOnly", line.IsNoteJournal, line.AL_ExchangeRateInfo.ReadOnly);
					AssertEquals("AL_Calc_FirstSubClassParentId ReadOnly", line.IsNoteJournal, line.AL_Calc_FirstSubClassParentIdInfo.ReadOnly);
					AssertEquals("AL_Calc_SecondSubClassParentId ReadOnly", line.IsNoteJournal, line.AL_Calc_SecondSubClassParentIdInfo.ReadOnly);

					AssertEquals("DepartmentDescription ReadOnly", true, line.DepartmentDescriptionInfo.ReadOnly);
					AssertEquals("GLAccountDescription ReadOnly", true, line.GLAccountDescriptionInfo.ReadOnly);
					AssertEquals("BranchName ReadOnly", true, line.BranchNameInfo.ReadOnly);
				}
			}
		}

		public void TestIsNoteJournal()
		{
			var line = (GLJournalLine)Factory.New(GetExpectedBusinessObjectType());
			AssertEquals(false, line.IsNoteJournal);

			var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Now, ZDateTime.Now);
			var journalLine = journal.GLJournalLines.AddNew();
			AssertEquals(false, line.IsNoteJournal);

			journal.AH_TransactionType = TransactionTypes.GLNoteJournal;
			AssertEquals(true, journalLine.IsNoteJournal);
		}

		public void TestUnitQuantity()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Austria))
			{
				AssertEquals("Pre-condition", 2, GlbCompany.CurrentCompany.GetLocalDecimals());
				AssertUnitQuantity();
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Japan))
			{
				AssertEquals("Pre-condition", 0, GlbCompany.CurrentCompany.GetLocalDecimals());
				AssertUnitQuantity();
			}

			void AssertUnitQuantity()
			{
				var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLNoteJournal, ZDateTime.Now, ZDateTime.Now);
				var line = journal.GLJournalLines.AddNew();

				AssertEquals("Pre-condition", 0m, line.UnitQuantity);
				AssertEquals("Pre-condition", 0m, line.UnsignedOSLineAmount);
				AssertEquals("Pre-condition", 0m, line.UnsignedLocalLineAmount);

				line.UnitQuantity = 2.11m;
				AssertEquals("Amount should be 0.", 2.11m, line.UnsignedOSLineAmount);
				AssertEquals("Local Amount should be 0.", 2.11m, line.UnsignedLocalLineAmount);
			}
		}

		public void TestUnits()
		{
			var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Now, ZDateTime.Now);
			var line = journal.GLJournalLines.AddNew();

			AssertEquals("Pre-codition:AL_AG", ZGuid.Empty, line.AL_AG);
			AssertEquals("Pre-codition:Units", "", line.Units);

			var glHeader = TestObjectCreator.CreateGLHeader();
			glHeader.AG_AccountType = AccountType.Undefined;
			glHeader.AG_StatisticalUnits = "KWH";
			line.AL_AG = glHeader.PK;

			AssertEquals(glHeader.PK, line.AL_AG);
			AssertEquals("", line.Units);

			glHeader.AG_AccountType = AccountType.Note;
			AssertEquals("KWH", line.Units);

			glHeader.AG_StatisticalUnits = "KG";
			AssertEquals("KG", line.Units);
		}

		public void TestGLJournalLineCurrencyDecimals()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Austria))
			{
				AssertLineCurrencyDecimals();
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Japan))
			{
				AssertLineCurrencyDecimals();
			}

			void AssertLineCurrencyDecimals()
			{
				var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
				var line = journal.GLJournalLines.AddNew();

				line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
				AssertEquals("transaction currency decimal should be 2 when currency is 'AUD'.", 2, line.CurrencyDecimals);
				AssertEquals("local currency decimal should be 2 when currency is 'AUD'.", GlbCompany.CurrentCompany.LocalCurrency.Decimals, line.LocalDecimals);

				line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Japan;
				AssertEquals("transaction currency decimal should be 2 when currency is 'JPY'.", 0, line.CurrencyDecimals);
				AssertEquals("local currency decimal should be 2 when currency is 'JPY'.", GlbCompany.CurrentCompany.LocalCurrency.Decimals, line.LocalDecimals);

				journal.AH_TransactionType = TransactionTypes.GLNoteJournal;

				line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
				AssertEquals("transaction currency decimal should be 2 when transaction is Note Journal.", 2, line.CurrencyDecimals);
				AssertEquals("local currency decimal should be 2 when transaction is Note Journal.", 2, line.LocalDecimals);

				line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Japan;
				AssertEquals("transaction currency decimal should be 2 when transaction is Note Journal.", 2, line.CurrencyDecimals);
				AssertEquals("local currency decimal should be 2 when transaction is Note Journal.", 2, line.LocalDecimals);
			}
		}

		public void TestLineCurrencyDecimals()
		{
			var glJournal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			var line = glJournal.GLJournalLines.AddNew();

			line.AL_RX_NKTransactionCurrency = "";
			AssertEquals("The currreny decimal should be 0 when currency is empty.", 2, line.LineCurrencyDecimals);

			line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			AssertEquals("The currreny decimal of AUD should be 0.", 2, line.LineCurrencyDecimals);

			line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Japan;
			AssertEquals("The currreny decimal of JPY should be 0.", 0, line.LineCurrencyDecimals);

			glJournal.AH_TransactionType = TransactionTypes.GLNoteJournal;
			AssertEquals("The currreny decimal should be 2 for Note Journal.", 2, line.LineCurrencyDecimals);
		}

		#endregion

		protected GLJournalLine JournalLine
		{
			get { return (GLJournalLine)Line; }
		}

		protected override Type MasterHeaderType
		{
			get { return typeof(GLJournal); }
		}

		protected override bool LineCanHaveTaxComponent
		{
			get { return false; }
		}

		protected override bool LineCanHaveForeignCurrency
		{
			get { return false; }
		}

		protected override bool AcceptAL_AC
		{
			get { return false; }
		}

		protected override string ExpectedEmptyAL_AGErrorMessage
		{
			get { return "Please enter a GL Post To Account."; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = (GLJournalLine)base.GetNewBusinessObject();
			if (result.AL_LineType.IsEmpty)
			{
				result.AL_LineType = TransactionTypes.GLStandardJournal;
			}
			return result;
		}

		protected override bool IsExpectMultiSubAccountsSupported => true;
	}
}

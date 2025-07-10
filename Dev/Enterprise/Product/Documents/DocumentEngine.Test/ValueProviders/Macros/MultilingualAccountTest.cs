using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(MultilingualAccount))]
	sealed class MultilingualAccountTest : ValueProviderTest
	{
		protected override List<FieldInfo> FieldCollection
		{
			get
			{
				return new List<FieldInfo>()
				{
					typeof(MultilingualAccount).GetField("fFactory", BindingFlags.Instance | BindingFlags.NonPublic),
					typeof(MultilingualAccount).GetField("fLanguage", BindingFlags.Instance | BindingFlags.NonPublic),
					typeof(MultilingualAccount).GetField("fSecondReportStartFromAccountNumber", BindingFlags.Instance | BindingFlags.NonPublic),
					typeof(MultilingualAccount).GetField("countryCode", BindingFlags.Instance | BindingFlags.NonPublic)
				};
			}
		}

		public class MockMultilingualAccount : MultilingualAccount
		{
			public AccountOrderType fAccountOrderType = AccountOrderType.ProfitAndLoss;
			public Guid fSecondReportStartFrom = Guid.Empty;
			public string fTestStartAccount;
			protected override AccountOrderType AccountOrderBeginWith
			{
				get
				{
					return fAccountOrderType;
				}
			}
			protected override Guid SecondReportStartFrom
			{
				get
				{
					ZQuery filter = new ZQuery(AccGLAccountDescriptorSchema.AJ_Language, SQLComparisonOperator.Equal, Language);
					filter.MaximumRows = 1;
					filter.AddToFilter(JoinCondition.And, AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, SQLComparisonOperator.Equal, fTestStartAccount);
					return ((AccGLAccountDescriptor)Factory.Load(typeof(AccGLAccountDescriptor), filter)[0]).PK.ToGuid();
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			CreateFrameworkAccounts(Core.Constants.Languages.ChineseSimplified, Core.Constants.CountryCodes.China);
			CreateFrameworkAccounts(Core.Constants.Languages.EnglishAmerican, Core.Constants.CountryCodes.Australia);
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Multilingual Account(12,0,1,2)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Multilingual Account(ZH-CN,CN,BSH,START)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Multilingual Account(EN-US,AU,BSH,START)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Multilingual Account(12,0,1,2)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<MultilingualAccount(wqqw,1,22,11)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<MULTILINGUAL ACCOUNT(12,2,qwe,22)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<MultilingualAccount( )>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<MultilingualAccount(0,2)>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(ZString.Empty, GetNewValueProvider().GetReplacement("<Multilingual Account(invalid,0,1,2)>", Report));
		}

		public void TestBalanceSheetStartReplacement()
		{
			MockMultilingualAccount mockMultilingualAccount = new MockMultilingualAccount();
			mockMultilingualAccount.fAccountOrderType = AccountOrderType.BalanceSheet;
			mockMultilingualAccount.fTestStartAccount = AccountNum5000;

			AssertEquals("ZH-CN, CN, BSH, START", AccountNum1000, mockMultilingualAccount.GetReplacement("<Multilingual Account(ZH-CN, CN, BSH, START)>", Report));
			AssertEquals("EN-US, AU, BSH, START", AccountNum1000, mockMultilingualAccount.GetReplacement("<Multilingual Account(EN-US, AU, BSH, START)>", Report));

			mockMultilingualAccount.fAccountOrderType = AccountOrderType.ProfitAndLoss;

			AssertEquals("ZH-CN, CN, BSH, START", AccountNum5000, mockMultilingualAccount.GetReplacement("<Multilingual Account(ZH-CN, CN, BSH, START)>", Report));
			AssertEquals("EN-US, AU, BSH, START", AccountNum5000, mockMultilingualAccount.GetReplacement("<Multilingual Account(EN-US, AU, BSH, START)>", Report));
		}

		public void TestProfitAndLossStartReplacement()
		{
			MockMultilingualAccount mockMultilingualAccount = new MockMultilingualAccount();
			mockMultilingualAccount.fAccountOrderType = AccountOrderType.BalanceSheet;
			mockMultilingualAccount.fTestStartAccount = AccountNum5000;

			AssertEquals("ZH-CN, CN, P&L, START", AccountNum5000, mockMultilingualAccount.GetReplacement("<Multilingual Account(ZH-CN, CN, P&L, START)>", Report));
			AssertEquals("EN-US, AU, P&L, START", AccountNum5000, mockMultilingualAccount.GetReplacement("<Multilingual Account(EN-US, AU, P&L, START)>", Report));
			mockMultilingualAccount.fAccountOrderType = AccountOrderType.ProfitAndLoss;
			AssertEquals("ZH-CN, CN, P&L, START", AccountNum1000, mockMultilingualAccount.GetReplacement("<Multilingual Account(ZH-CN, CN, P&L, START)>", Report));
			AssertEquals("EN-US, AU, P&L, START", AccountNum1000, mockMultilingualAccount.GetReplacement("<Multilingual Account(EN-US, AU, P&L, START)>", Report));
		}

		public void TestBalanceSheetEndReplacement()
		{
			MockMultilingualAccount mockMultilingualAccount = new MockMultilingualAccount();
			mockMultilingualAccount.fAccountOrderType = AccountOrderType.BalanceSheet;
			mockMultilingualAccount.fTestStartAccount = AccountNum5000;

			AssertEquals("ZH-CN, CN, BSH, END", AccountNum4999, mockMultilingualAccount.GetReplacement("<Multilingual Account(ZH-CN, CN, BSH, END)>", Report));
			AssertEquals("EN-US, AU, BSH, END", AccountNum4999, mockMultilingualAccount.GetReplacement("<Multilingual Account(EN-US, AU, BSH, END)>", Report));

			mockMultilingualAccount.fAccountOrderType = AccountOrderType.ProfitAndLoss;

			AssertEquals("ZH-CN, CN, BSH, END", AccountNum9999, mockMultilingualAccount.GetReplacement("<Multilingual Account(ZH-CN, CN, BSH, END)>", Report));
			AssertEquals("EN-US, AU, BSH, END", AccountNum9999, mockMultilingualAccount.GetReplacement("<Multilingual Account(EN-US, AU, BSH, END)>", Report));
		}

		public void TestProfitAndLossEndReplacement()
		{
			MockMultilingualAccount mockMultilingualAccount = new MockMultilingualAccount();
			mockMultilingualAccount.fAccountOrderType = AccountOrderType.BalanceSheet;
			mockMultilingualAccount.fTestStartAccount = AccountNum5000;

			AssertEquals("ZH-CN, CN, P&L, END", AccountNum9999, mockMultilingualAccount.GetReplacement("<Multilingual Account(ZH-CN, CN, P&L, END)>", Report));
			AssertEquals("EN-US, AU, P&L, END", AccountNum9999, mockMultilingualAccount.GetReplacement("<Multilingual Account(EN-US, AU, P&L, END)>", Report));

			mockMultilingualAccount.fAccountOrderType = AccountOrderType.ProfitAndLoss;

			AssertEquals("ZH-CN, CN, P&L, END", AccountNum4999, mockMultilingualAccount.GetReplacement("<Multilingual Account(ZH-CN, CN, P&L, END)>", Report));
			AssertEquals("EN-US, AU, P&L, END", AccountNum4999, mockMultilingualAccount.GetReplacement("<Multilingual Account(EN-US, AU, P&L, END)>", Report));
		}

		public void TestAllReplacements()
		{
			var mockMultilingualAccount = new MockMultilingualAccount();
			mockMultilingualAccount.fAccountOrderType = AccountOrderType.BalanceSheet;
			mockMultilingualAccount.fTestStartAccount = AccountNum5000;

			AssertEquals("ZH-CN, CN, P&L, START", AccountNum5000, mockMultilingualAccount.GetReplacement("<Multilingual Account(ZH-CN, CN, P&L, START)>", Report));
			AssertEquals("ZH-CN, CN, BSH, START", AccountNum1000, mockMultilingualAccount.GetReplacement("<Multilingual Account(ZH-CN, CN, BSH, START)>", Report));
			AssertEquals("ZH-CN, CN, P&L, END", AccountNum9999, mockMultilingualAccount.GetReplacement("<Multilingual Account(ZH-CN, CN, P&L, END)>", Report));
			AssertEquals("ZH-CN, CN, BSH, END", AccountNum4999, mockMultilingualAccount.GetReplacement("<Multilingual Account(ZH-CN, CN, BSH, END)>", Report));

			AssertEquals("EN-US, AU, P&L, START", AccountNum5000, mockMultilingualAccount.GetReplacement("<Multilingual Account(EN-US, AU, P&L, START)>", Report));
			AssertEquals("EN-US, AU, BSH, START", AccountNum1000, mockMultilingualAccount.GetReplacement("<Multilingual Account(EN-US, AU, BSH, START)>", Report));
			AssertEquals("EN-US, AU, P&L, END", AccountNum9999, mockMultilingualAccount.GetReplacement("<Multilingual Account(EN-US, AU, P&L, END)>", Report));
			AssertEquals("EN-US, AU, BSH, END", AccountNum4999, mockMultilingualAccount.GetReplacement("<Multilingual Account(EN-US, AU, BSH, END)>", Report));
		}

		public void TestThrowExceptionOnInvalidReportOrderConfiguration()
		{
			MockMultilingualAccount mockMultilingualAccount = new MockMultilingualAccount();
			mockMultilingualAccount.fAccountOrderType = AccountOrderType.BalanceSheet;
			mockMultilingualAccount.fTestStartAccount = AccountNum1000;

			AssertEquals("ZH-CN, CN, P&L, START", AccountNum1000, mockMultilingualAccount.GetReplacement("<Multilingual Account(ZH-CN, CN, P&L, START)>", Report));
			try
			{
				mockMultilingualAccount.GetReplacement("<Multilingual Account(ZH-CN, CN, BSH, START)>", Report);
				Fail("ApplicationException should be thrown");
			}
			catch (ApplicationException e)
			{
				AssertEquals("Message as expected", "Cannot get start Local Account Number for the first report. Please check Accounting>Framework>Report Order>Report Order Registry setting.", e.Message);
			}

			mockMultilingualAccount.fTestStartAccount = AccountNum9999;
			AssertEquals("ZH-CN, CN, BSH, END", AccountNum5100, mockMultilingualAccount.GetReplacement("<Multilingual Account(ZH-CN, CN, BSH, END)>", Report));

			try
			{
				mockMultilingualAccount.GetReplacement("<Multilingual Account(ZH-CN, CN, P&L, END)>", Report);
				Fail("ApplicationException should be thrown");
			}
			catch (ApplicationException e)
			{
				AssertEquals("Message as expected", "Cannot get end Local Account Number for the second report. Please check Accounting>Framework>Report Order>Report Order Registry setting.", e.Message);
			}
		}

		void CreateFrameworkAccounts(ZString language, ZString country)
		{
			BusinessObjectFactory testDataFactory = new BusinessObjectFactory();
			BusinessObject accountDescriptor = testDataFactory.NewWithValidTestData<AccGLAccountDescriptor>();
			accountDescriptor[AccGLAccountDescriptor.Schema.AJ_LocalAccountNumber] = AccountNum5000;
			accountDescriptor[AccGLAccountDescriptor.Schema.AJ_Language] = language;
			accountDescriptor[AccGLAccountDescriptor.Schema.AJ_RN_NKCountryOfCompliance] = country;
			testDataFactory.Save();
			accountDescriptor = testDataFactory.NewWithValidTestData<AccGLAccountDescriptor>();
			accountDescriptor[AccGLAccountDescriptor.Schema.AJ_LocalAccountNumber] = AccountNum1000;
			accountDescriptor[AccGLAccountDescriptor.Schema.AJ_Language] = language;
			accountDescriptor[AccGLAccountDescriptor.Schema.AJ_RN_NKCountryOfCompliance] = country;
			testDataFactory.Save();
			accountDescriptor = testDataFactory.NewWithValidTestData<AccGLAccountDescriptor>();
			accountDescriptor[AccGLAccountDescriptor.Schema.AJ_LocalAccountNumber] = AccountNum5100;
			accountDescriptor[AccGLAccountDescriptor.Schema.AJ_Language] = language;
			accountDescriptor[AccGLAccountDescriptor.Schema.AJ_RN_NKCountryOfCompliance] = country;
			testDataFactory.Save();
			accountDescriptor = testDataFactory.NewWithValidTestData<AccGLAccountDescriptor>();
			accountDescriptor[AccGLAccountDescriptor.Schema.AJ_LocalAccountNumber] = AccountNum4999;
			accountDescriptor[AccGLAccountDescriptor.Schema.AJ_Language] = language;
			accountDescriptor[AccGLAccountDescriptor.Schema.AJ_RN_NKCountryOfCompliance] = country;
			testDataFactory.Save();
			accountDescriptor = testDataFactory.NewWithValidTestData<AccGLAccountDescriptor>();
			accountDescriptor[AccGLAccountDescriptor.Schema.AJ_LocalAccountNumber] = AccountNum9999;
			accountDescriptor[AccGLAccountDescriptor.Schema.AJ_Language] = language;
			accountDescriptor[AccGLAccountDescriptor.Schema.AJ_RN_NKCountryOfCompliance] = country;
			testDataFactory.Save();
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new MultilingualAccount();
		}

		protected override void AssertExamplesAreReplacedAsExpected(string example, object expectedResult)
		{
			var mockMultilingualAccount = new MockMultilingualAccount();
			mockMultilingualAccount.fAccountOrderType = AccountOrderType.BalanceSheet;
			mockMultilingualAccount.fTestStartAccount = AccountNum5000;
			AssertEquals(expectedResult, mockMultilingualAccount.GetReplacement(example, Report));
		}

		const string AccountNum5000 = "5000.00.00";
		const string AccountNum1000 = "1000.00.00";
		const string AccountNum5100 = "5100.00.00";
		const string AccountNum4999 = "4999.00.00";
		const string AccountNum9999 = "9999.00.00";
	}
}

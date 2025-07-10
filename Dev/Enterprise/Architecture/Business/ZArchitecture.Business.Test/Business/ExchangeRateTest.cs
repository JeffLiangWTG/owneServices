using System;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ZExchangeRate))]
	sealed class ExchangeRateTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNegativeExchangeRateValidation()
		{
			var expectedError = "Exchange Rate for Currency IQD must be greater than 0.";
			ExchangeRate.IsRateRequired = true;
			ExchangeRate.Currency = ForeignCurrencyNK;
			ExchangeRate.Rate = -1.69m;
			AssertHasError(ExchangeRate.RateInfo, expectedError);

			using (ExchangeRate.GetValidationSuspender())
			{
				ExchangeRate.RateInfo.ClearValue();
				ExchangeRate.RunPreSaveValidation();
				AssertNoError("No error due to suspended validation", ExchangeRate.RateInfo, expectedError);
			}

			ExchangeRate.Currency = "";
			ExchangeRate.Rate = -1.5m;
			AssertHasError(ExchangeRate.RateInfo, "Rate must be greater than 0.");
		}

		public void TestIsCurrencyRequired()
		{
			ExchangeRate.IsCurrencyRequired = false;
			ExchangeRate.Currency = ForeignCurrencyNK;
			AssertNoErrors("Should not be an error on currency info", ExchangeRate.CurrencyInfo);

			ExchangeRate.Currency = "XXX";
			AssertHasErrors("Should be an error on currency info", ExchangeRate.CurrencyInfo);

			ExchangeRate.Currency = ZString.Empty;
			AssertNoErrors("Should not be an error on currency info", ExchangeRate.CurrencyInfo);

			ExchangeRate.IsCurrencyRequired = true;
			ExchangeRate.Currency = "XXX";
			AssertHasErrors("Should be an error on currency info", ExchangeRate.CurrencyInfo);

			ExchangeRate.Currency = ZString.Empty;
			AssertHasErrors("Should be an error on currency info", ExchangeRate.CurrencyInfo);
		}

		public void TestIsRateRequired()
		{
			ExchangeRate.IsRateRequired = true;
			ExchangeRate.Currency = ForeignCurrencyNK;
			ExchangeRate.Rate = 0.6m;
			ExchangeRate.Rate = 0m;
			AssertHasErrors("Should be an error on exrate because rate is required and currency is valid", ExchangeRate.RateInfo);

			ExchangeRate.Rate = 0.65m;
			AssertNoErrors("Should not be any errors on exrate", ExchangeRate.RateInfo);

			ExchangeRate.Currency = "XXX";
			AssertNoErrors("Should not be an error on rate because currency is invalid", ExchangeRate.RateInfo);

			ExchangeRate.IsRateRequired = false;
			ExchangeRate.Currency = ForeignCurrencyNK;
			ExchangeRate.Rate = 0.65m;
			ExchangeRate.Rate = 0m;
			AssertNoErrors("Should be no error on rate because rate isn't required for save", ExchangeRate.RateInfo);
		}

		public void TestInitialAmountReadOnly_InitialisedWithForeign()
		{
			ExchangeRate.Currency = ForeignCurrencyNK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var retrievedRateDummy = newFactory.Load<DummyWithExchangeRateBusinessObject>(RateDummy.PK);

			AssertEquals("Rate should not be read only when reloaded", false, retrievedRateDummy.ExchangeRate.RateInfo.ReadOnly);
		}

		public void TestInitialAmountReadOnly_InitialisedWithLocal()
		{
			ExchangeRate.Currency = LocalCurrencyNK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var retrievedRateDummy = newFactory.Load<DummyWithExchangeRateBusinessObject>(RateDummy.PK);

			AssertEquals("Rate should be read only when reloaded", true, retrievedRateDummy.ExchangeRate.RateInfo.ReadOnly);
		}

		public void TestRateReadOnly()
		{
			AssertEquals("Rate should be readonly when New.", true, ExchangeRate.RateInfo.ReadOnly);

			ExchangeRate.Currency = ForeignCurrencyNK;
			AssertEquals("Rate should not be readonly when Currency is Foreign.", false, ExchangeRate.RateInfo.ReadOnly);

			ExchangeRate.Currency = LocalCurrencyNK;
			AssertEquals("Rate should be readonly when Currency is Local.", true, ExchangeRate.RateInfo.ReadOnly);
		}

		public void TestRateReadOnly_FromInnerInfo()
		{
			ExchangeRate.Currency = ForeignCurrencyNK;
			AssertEquals("Rate not readonly initially.", false, ExchangeRate.RateInfo.ReadOnly);
			ExchangeRate.IsRateReadOnly = true;
			AssertEquals("Rate readonly when InnerInfo.ReadOnly.", true, ExchangeRate.RateInfo.ReadOnly);
		}

		public void TestRateSetWhenCurrencySet()
		{
			SetupExchangeRate();

			ExchangeRate.Currency = LocalCurrencyNK;
			RateDummy.Validation.ValidateZ0_Guid();
			AssertEquals("Rate should be set to 1 when Currency is Local.", new ZDecimal(1), ExchangeRate.Rate);

			ExchangeRate.Currency = ((IRefCurrency)DummyCurrency).RX_Code;
			RateDummy.Validation.ValidateZ0_Guid();
			AssertEquals("Rate should be test to exchange rate from database.", new ZDecimal(50.5M), ExchangeRate.Rate);
		}

		public void TestValidRate()
		{
			ExchangeRate.IsCurrencyRequired = true;
			ExchangeRate.IsRateRequired = true;
			ExchangeRate.Currency = ForeignCurrencyNK;
			ExchangeRate.Rate = 0.65m;
			ExchangeRate.Rate = 0m;
			RateDummy.Validation.ValidateZ0_Decimal();
			AssertHasErrors("Error when Rate set to 0", ExchangeRate.RateInfo);

			ExchangeRate.Rate = 1;
			RateDummy.Validation.ValidateZ0_Decimal();
			AssertNoErrors("No Error when Rate greater than 0", ExchangeRate.RateInfo);

			ExchangeRate.IsRateRequired = false;
			ExchangeRate.Rate = 0m;
			AssertNoErrors("No error when rate is zero and rate is not required", ExchangeRate.RateInfo);
		}

		public void TestValidRateAfterCurrencySet()
		{
			ExchangeRate.Rate = 10;
			AssertEquals("Rate has no warnings.", false, ExchangeRate.RateInfo.HasNotifications());

			ExchangeRate.Currency = "USD";
			AssertEquals("Warnings exist on Rate when today's exchange rate not found.", true, ExchangeRate.RateInfo.HasWarnings());
			AssertEquals("Today's Buy Rate Not Found.", ExchangeRate.RateInfo.GetWarnings().GetFirstMessage());

			ExchangeRate.Rate = DummyRate;
			AssertEquals("Rate clears warnings.", false, ExchangeRate.RateInfo.HasNotifications());

			var customsExchangeRate = new DummyZExchangeRate(RateDummy, ExchangeRateType.GlobalCreditControl, RateDummy.Z0_AnotherDecimalInfo, (ZPropertyInfoString)RateDummy.Z0_CodeInfo);

			customsExchangeRate.Currency = "CNY";
			AssertEquals("Warnings exist on Rate when today's exchange rate not found.", true, customsExchangeRate.RateInfo.HasWarnings());
			AssertEquals("Today's Global Credit Control Not Found.", customsExchangeRate.RateInfo.GetWarnings().GetFirstMessage());
		}

		public void TestRateDecimalPlaces()
		{
			AssertEquals("Pre-condition: CurrentCompany.IsReciprocal", false, EnvProxy.Instance.CurrentCompany.IsReciprocal);
			AssertEquals("RateDecimalPlaces", 6, ExchangeRate.RateDecimalPlaces);

			// Setting IsReciprocal to True. Does not require explicit rolling back
			BusinessObject currentCompany = (BusinessObject)Factory.Load<IGlbCompany>(new ZGuid(EnvProxy.Instance.CurrentCompany.PK));
			currentCompany["GC_IsReciprocal"] = true;
			Factory.Save();
			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertEquals("CurrentCompany.IsReciprocal", true, EnvProxy.Instance.CurrentCompany.IsReciprocal);
				AssertEquals("RateDecimalPlaces", 6, ExchangeRate.RateDecimalPlaces);
			}
		}

		public void TestCurrency()
		{
			ExchangeRate.Currency = "XXX";
			AssertEquals("Errors exist on Currency when invalid.", true, ExchangeRate.CurrencyInfo.HasErrors());

			ExchangeRate.Currency = "AUD";
			AssertEquals("No errors exist on Currency when valid.", false, ExchangeRate.CurrencyInfo.HasErrors());

			ExchangeRate.IsCurrencyRequired = true;
			ExchangeRate.Currency = ZString.Empty;
			AssertEquals("Errors exist on Currency when empty.", true, ExchangeRate.CurrencyInfo.HasErrors());

			ExchangeRate.IsCurrencyRequired = false;
			ExchangeRate.Currency = ZString.Empty;
			AssertEquals("No Errors exist on Currency when empty and currency is not required.", true, ExchangeRate.CurrencyInfo.HasErrors());
		}

		public void TestCurrencyList()
		{
			AssertNotNull("List should be created.", ExchangeRate.CurrencyList);
		}

		public void TestRefetchExchangeRate()
		{
			SetupExchangeRate();
			ExchangeRate.Currency = ((IRefCurrency)DummyCurrency).RX_Code;
			ExchangeRate.Rate = 0M;

			ExchangeRate.RefetchExchangeRate();
			AssertEquals("Rate set by Refetching.", DummyRate, ExchangeRate.Rate);
		}

		public void TestCallThroughToSetterOfBizObj()
		{
			AssertEquals("Z0_AnotherDate initially", ZDateTime.Empty, RateDummy.Z0_AnotherDate);
			ExchangeRate.Currency = "XXX";
			AssertEquals("Z0_AnotherDate after Currency set", new ZDateTime(2000, 12, 12), RateDummy.Z0_AnotherDate);
		}

		public void TestCallThroughToValidateOfBizObj()
		{
			AssertEquals("No Errors initially", false, ExchangeRate.RateInfo.HasErrors());
			ExchangeRate.Rate = 52;
			AssertEquals("One error after setting Rate to invalid value", 1, ExchangeRate.RateInfo.Notifications.GetErrors().Count());
		}

		[TestDate(2050, 10, 1, 1, 0, 0)]
		public void TestGetTodaysRate()
		{
			Enterprise.ZArchitecture.Core.ExchangeRateReader.GetReaderInstance().ClearCache();
			AssertEquals("PreCondition: StaticCurrentFetcher.Instance.CurrentCompany is equal to EnvProxy.Instance.CurrentCompany", StaticCurrentFetcher.Instance.CurrentCompany.PK.ToGuid(), EnvProxy.Instance.CurrentCompany.PK);

			BusinessObject localExRate = (BusinessObject)Factory.New<IRefExchangeRate>();
			localExRate[RefExchangeRateSchema.RE_GC.Name] = StaticCurrentFetcher.Instance.CurrentCompany.PK;
			localExRate[RefExchangeRateSchema.RE_RX_NKExCurrency.Name] = LocalCurrencyNK;
			localExRate[RefExchangeRateSchema.RE_StartDate.Name] = TestDateAttribute.Date.AddDays(-1).Date;
			localExRate[RefExchangeRateSchema.RE_ExpiryDate.Name] = TestDateAttribute.Date.AddDays(1).Date;
			localExRate[RefExchangeRateSchema.RE_SellRate.Name] = .999M;
			localExRate[RefExchangeRateSchema.RE_ExRateType.Name] = ExchangeRate.Type.ToString();

			BusinessObject expiredCurrency = Factory.NewWithValidTestData(ObjectFactory.GetType<IRefCurrency>());
			BusinessObject expiredExRate = (BusinessObject)Factory.New<IRefExchangeRate>();
			expiredExRate[RefExchangeRateSchema.RE_GC.Name] = StaticCurrentFetcher.Instance.CurrentCompany.PK;
			expiredExRate[RefExchangeRateSchema.RE_RX_NKExCurrency.Name] = ((IRefCurrency)expiredCurrency).RX_Code;
			expiredExRate[RefExchangeRateSchema.RE_StartDate.Name] = TestDateAttribute.Date.AddDays(1).Date;
			expiredExRate[RefExchangeRateSchema.RE_ExpiryDate.Name] = TestDateAttribute.Date.AddDays(2).Date;
			expiredExRate[RefExchangeRateSchema.RE_SellRate.Name] = 25.5M;
			expiredExRate[RefExchangeRateSchema.RE_ExRateType.Name] = ExchangeRate.Type.ToString();
			Factory.Save();

			ExchangeRate.Currency = LocalCurrencyNK;
			AssertEquals("GetTodaysRate should return 0.999", 0.999M, ExchangeRate.GetTodaysRate_Exposed(LocalCurrencyNK));

			ExchangeRate.Currency = ((IRefCurrency)expiredCurrency).RX_Code;
			AssertEquals("GetTodaysRate should return 0", 0M, ExchangeRate.GetTodaysRate_Exposed(((IRefCurrency)expiredCurrency).RX_Code));
		}

		#region Recursive ReadOnly

		[ExpectNoExceptions]
		public void TestNoRecursiveRateReadOnly()
		{
			DummyBizOWithExchangeRate dummy = Factory.New<DummyBizOWithExchangeRate>();

			dummy.RX = "INR";

			dummy.SellExRate_ReadOnly_WasCalled = false;
			dummy.ExchangeRate.IsRateReadOnly = dummy.ExchangeRate.RateInfo.ReadOnly;
			Assert("SellExRate_ReadOnly should be called", dummy.SellExRate_ReadOnly_WasCalled);
		}

		class DummyBizOWithExchangeRate : DummyEnterpriseBusinessObject
		{
			public DummyBizOWithExchangeRate(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public ZExchangeRate ExchangeRate
			{
				get
				{
					if (fExchangeRate == null)
					{
						fExchangeRate = new ZExchangeRate(this, ExchangeRateType.Sell, SellExRateInfo, (ZPropertyInfoString)RXInfo);
					}
					return fExchangeRate;
				}
			}
			ZExchangeRate fExchangeRate;

			public ZDecimal SellExRate
			{
				get { return fSellExRate; }
				set
				{
					fSellExRate = value;
					SellExRateInfo.RefreshBinding();
				}
			}
			ZDecimal fSellExRate;

			public ZPropertyInfo SellExRateInfo
			{
				get { return GetZPropertyInfo(nameof(SellExRate)); }
			}

			protected bool SellExRate_ReadOnly
			{
				get
				{
					SellExRate_ReadOnly_WasCalled = true;
					return ExchangeRate.IsRateReadOnly;
				}
			}

			public bool SellExRate_ReadOnly_WasCalled;

			public ZString RX
			{
				get { return fRX; }
				set
				{
					fRX = value;
					RXInfo.RefreshBinding();
				}
			}
			ZString fRX;

			public ZPropertyInfo RXInfo
			{
				get { return GetZPropertyInfo(nameof(RX)); }
			}
		}

		#endregion

		#region Implementation

		void SetupExchangeRate()
		{
			DummyCurrency = (BusinessObject)Factory.New<IRefCurrency>();
			DummyCurrency["RX_Code"] = "DUM";
			DummyCurrency["RX_Desc"] = "Dummy Currency";
			DummyCurrency["RX_UnitName"] = "Dummy Dollars";
			DummyCurrency["RX_SubUnitName"] = "Dummy Cents";
			DummyCurrency["RX_Symbol"] = "$D$";

			DummyExchangeRate = (BusinessObject)Factory.New<IRefExchangeRate>();
			DummyExchangeRate["RE_GC"] = StaticCurrentFetcher.Instance.CurrentCompany.PK;
			DummyExchangeRate["RE_RX_NKExCurrency"] = ((IRefCurrency)DummyCurrency).RX_Code;
			DummyExchangeRate["RE_StartDate"] = EnvProxy.Instance.Time.CurrentLocalDate.AddDays(-1);
			DummyExchangeRate["RE_ExpiryDate"] = EnvProxy.Instance.Time.CurrentLocalDate.AddDays(1);
			DummyExchangeRate["RE_SellRate"] = DummyRate;
			DummyExchangeRate["RE_ExRateType"] = ExchangeRate.Type.ToString();
			Factory.Save();
		}

		BusinessObject DummyCurrency;
		BusinessObject DummyExchangeRate;
		readonly ZDecimal DummyRate = 50.5M;

		ZString LocalCurrencyNK
		{
			get { return StaticCurrentFetcher.Instance.CurrentCompany.Currency.RX_Code; }
		}

		ZString ForeignCurrencyNK
		{
			get
			{
				ZQuery filter = new ZQuery(RefCurrencySchema.RX_Code, "IQD");
				BusinessObject iraqiCurrency = (BusinessObject)Factory.LoadTop1<IRefCurrency>(filter);
				AssertNotNull("Could not find Foreign Currency in Database: IQD.", iraqiCurrency);

				return ((IRefCurrency)iraqiCurrency).RX_Code;
			}
		}

		DummyBusinessObject Dummy;

		protected override void SetUp()
		{
			base.SetUp();
			Dummy = (DummyBusinessObject)Factory.New(TypeOfDummy);
			AssertNotNull("Dummy should be created!", Dummy);
			RateDummy = Factory.New<DummyWithExchangeRateBusinessObject>();
			AssertNotNull("RateDummy should be created.", RateDummy);
		}

		Type TypeOfDummy { get { return typeof(DummyBusinessObject); } }

		DummyWithExchangeRateBusinessObject RateDummy;

		DummyZExchangeRate ExchangeRate
		{
			get { return RateDummy.ExchangeRate; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return (Factory.New<DummyWithExchangeRateBusinessObject>()).ExchangeRate;
		}

		#endregion

		#region Empty Tests

		public override void TestBizObjectFields()
		{
			Assert("Do nothing", true);
		}

		#endregion
	}
}

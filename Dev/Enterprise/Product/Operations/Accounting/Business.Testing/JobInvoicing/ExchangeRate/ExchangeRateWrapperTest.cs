using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Business;
using NUnit.Framework;
using RefCurrency = Enterprise.MasterFiles.Business.RefCurrency;
using ServiceDirection = Enterprise.MasterFiles.Business.OrgConstants.ServiceDirection.Code;
using TransportMode = Enterprise.MasterFiles.Business.OrgConstants.ModesForGroupOrSubTotal.Codes;

namespace Enterprise.Accounting.Business.Testing
{
	class ExchangeRateWrapperTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			try
			{
				new ExchangeRateWrapper(null, Creator.LocalClient);
				Fail("Should throw an ArgumentNullException");
			}
			catch (ArgumentNullException ex)
			{
				AssertEquals(@"Value cannot be null.
Parameter name: exchangeRate", ex.Message);
			}

			Assert(ExchangeRate.IsGenericRate);

			var wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertNotNull(wrapper);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertNotNull(wrapper);

			ExchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Creditor.ToCode();
			ExchangeRate.JF_OH_Org = Creator.Creditor1.PK;
			Assert(!ExchangeRate.IsGenericRate);

			try
			{
				new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
				Fail("Should throw an ArgumentException");
			}
			catch (ArgumentException ex)
			{
				AssertEquals(@"Org parameter must match the Exchange Rate Organisation on non-generic Exchange Rate.
Parameter name: org", ex.Message);
			}
		}

		public void TestRate()
		{
			Creator.ABIGAS.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			Creator.CreateCFXUplift(Creator.LocalClient.CompanyData.AccCFXConfigurations, serviceDirection: ServiceDirection.Export, transportMode: TransportMode.Air, percentage: 10m);

			ExchangeRate.JF_RX_NKRateCurrency = Creator.USD.Code;
			ExchangeRate.JF_BaseRate = 1.5m;
			Assert(ExchangeRate.IsGenericRate);

			var genericWrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(1.5m, genericWrapper.Rate);

			genericWrapper = new ExchangeRateWrapper(ExchangeRate, Creator.ABIGAS);
			AssertEquals(1.5m, genericWrapper.Rate);

			ExchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
			ExchangeRate.JF_OH_Org = Creator.ABIGAS.PK;
			Assert(!ExchangeRate.IsGenericRate);

			var wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(1.5m, genericWrapper.Rate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.ABIGAS);
			AssertEquals(1.5m, genericWrapper.Rate);

			ExchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Creditor.ToCode();
			Assert(!ExchangeRate.IsGenericRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(1.5m, genericWrapper.Rate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.ABIGAS);
			AssertEquals(1.5m, genericWrapper.Rate);
		}

		public void TestSellRate()
		{
			Creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			Creator.CreateCFXUplift(Creator.LocalClient.CompanyData.AccCFXConfigurations, serviceDirection: ServiceDirection.Export, transportMode: TransportMode.Air, percentage: 10m);

			ExchangeRate.JF_RX_NKRateCurrency = Creator.USD.Code;
			ExchangeRate.JF_BaseRate = 1.5m;
			Assert(ExchangeRate.IsGenericRate);
			AssertEquals(1.5m, ExchangeRate.JF_SellRate);
			AssertEquals(decimal.Zero, ExchangeRate.JF_CFXPercent);

			var wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(1.5m, wrapper.SellRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(1.35m, wrapper.SellRate);

			ExchangeRate.JF_CFXPercent = 5m;
			AssertEquals(1.425m, ExchangeRate.JF_SellRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(1.5m, wrapper.SellRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(1.35m, wrapper.SellRate);

			ExchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
			ExchangeRate.JF_OH_Org = Creator.LocalClient.PK;
			ExchangeRate.JF_CFXPercent = decimal.Zero;
			Assert(!ExchangeRate.IsGenericRate);
			AssertEquals(1.5m, ExchangeRate.JF_SellRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(1.5m, wrapper.SellRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(1.5m, wrapper.SellRate);

			ExchangeRate.JF_CFXPercent = 5m;
			AssertEquals(1.425m, ExchangeRate.JF_SellRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(1.425m, wrapper.SellRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(1.425m, wrapper.SellRate);

			ExchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Creditor.ToCode();
			Assert(!ExchangeRate.IsGenericRate);
			AssertEquals(1.425m, ExchangeRate.JF_SellRate);

			// Wrapper will take Sell Rate non-generic ExchangeRate even for Creditor
			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(1.425m, wrapper.SellRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(1.425m, wrapper.SellRate);
		}

		[TestDate(2025, 1, 8)]
		public void TestCFXPercent()
		{
			Creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			Creator.CreateCFXUplift(Creator.LocalClient.CompanyData.AccCFXConfigurations, serviceDirection: ServiceDirection.Export, transportMode: TransportMode.Air, percentage: 5m);
			Creator.CreateCFXUplift(Creator.LocalClient.CompanyData.AccCFXConfigurations, serviceDirection: ServiceDirection.Export, transportMode: TransportMode.Air, percentage: 10m, startDate: new ZDate(2025, 1, 6), expiryDate: new ZDate(2025, 1, 9));

			ExchangeRate.JF_RX_NKRateCurrency = Creator.USD.Code;
			ExchangeRate.JF_BaseRate = 1.5m;
			Assert(ExchangeRate.IsGenericRate);
			AssertEquals(1.5m, ExchangeRate.JF_SellRate);
			AssertEquals(decimal.Zero, ExchangeRate.JF_CFXPercent);

			var wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(decimal.Zero, wrapper.CFXPercent);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(10m, wrapper.CFXPercent);

			ExchangeRate.JF_CFXPercent = 5m;
			AssertEquals(1.425m, ExchangeRate.JF_SellRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(decimal.Zero, wrapper.CFXPercent);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(10m, wrapper.CFXPercent);

			ExchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
			ExchangeRate.JF_OH_Org = Creator.LocalClient.PK;
			ExchangeRate.JF_CFXPercent = decimal.Zero;
			Assert(!ExchangeRate.IsGenericRate);
			AssertEquals(1.5m, ExchangeRate.JF_SellRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(decimal.Zero, wrapper.CFXPercent);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(1.5m, wrapper.SellRate);
			AssertEquals(decimal.Zero, wrapper.CFXPercent);

			ExchangeRate.JF_CFXPercent = 5m;
			AssertEquals(1.425m, ExchangeRate.JF_SellRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(5m, wrapper.CFXPercent);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(5m, wrapper.CFXPercent);

			ExchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Creditor.ToCode();
			Assert(!ExchangeRate.IsGenericRate);
			AssertEquals(5m, wrapper.CFXPercent);

			// Wrapper will take CFX Percent from non-generic ExchangeRate even for Creditor
			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(5m, wrapper.CFXPercent);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(5m, wrapper.CFXPercent);
		}

		public void TestCFXPercent_CurrencyExcludedCFXCalculation()
		{
			Creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			Creator.CreateCFXUplift(Creator.LocalClient.CompanyData.AccCFXConfigurations, serviceDirection: ServiceDirection.Export, transportMode: TransportMode.Air, percentage: 10m);
			Creator.USD.RX_IsExcludedCFXCalculation = true;
			Factory.Save();
			Assert(RefCurrency.IsExcludedCFXCalculation(Creator.USD.Code));

			ExchangeRate.JF_RX_NKRateCurrency = Creator.USD.Code;
			ExchangeRate.JF_BaseRate = 1.5m;
			Assert(ExchangeRate.IsGenericRate);
			AssertEquals(1.5m, ExchangeRate.JF_SellRate);
			AssertEquals(decimal.Zero, ExchangeRate.JF_CFXPercent);

			var wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(decimal.Zero, wrapper.CFXPercent);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(decimal.Zero, wrapper.CFXPercent);

			ExchangeRate.JF_CFXPercent = 5m;
			AssertEquals(1.5m, ExchangeRate.JF_SellRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(decimal.Zero, wrapper.CFXPercent);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(decimal.Zero, wrapper.CFXPercent);

			ExchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
			ExchangeRate.JF_OH_Org = Creator.LocalClient.PK;
			ExchangeRate.JF_CFXPercent = decimal.Zero;
			Assert(!ExchangeRate.IsGenericRate);
			AssertEquals(1.5m, ExchangeRate.JF_SellRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(decimal.Zero, wrapper.CFXPercent);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(decimal.Zero, wrapper.CFXPercent);

			ExchangeRate.JF_CFXPercent = 5m;
			AssertEquals(1.5m, ExchangeRate.JF_SellRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(5m, wrapper.CFXPercent);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(5m, wrapper.CFXPercent);

			ExchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Creditor.ToCode();
			Assert(!ExchangeRate.IsGenericRate);
			AssertEquals(5m, wrapper.CFXPercent);

			// Wrapper will take CFX Percent from non-generic ExchangeRate even for Creditor
			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(5m, wrapper.CFXPercent);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(5m, wrapper.CFXPercent);
		}

		public void TestCFXMinimum()
		{
			Creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			Creator.CreateCFXUplift(Creator.LocalClient.CompanyData.AccCFXConfigurations, serviceDirection: ServiceDirection.Export, transportMode: TransportMode.Air, percentage: 1m, minimum: 10m);

			ExchangeRate.JF_RX_NKRateCurrency = Creator.USD.Code;
			Assert(ExchangeRate.IsGenericRate);
			AssertEquals(decimal.Zero, ExchangeRate.JF_CFXMinimum);

			var wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(decimal.Zero, wrapper.CFXMinimum);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(10m, wrapper.CFXMinimum);

			ExchangeRate.JF_CFXMinimum = 5m;

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(decimal.Zero, wrapper.CFXMinimum);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(10m, wrapper.CFXMinimum);

			ExchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
			ExchangeRate.JF_OH_Org = Creator.LocalClient.PK;
			ExchangeRate.JF_CFXMinimum = decimal.Zero;
			Assert(!ExchangeRate.IsGenericRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(decimal.Zero, wrapper.CFXMinimum);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(decimal.Zero, wrapper.CFXMinimum);

			ExchangeRate.JF_CFXMinimum = 5m;

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(5m, wrapper.CFXMinimum);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(5m, wrapper.CFXMinimum);

			ExchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Creditor.ToCode();
			Assert(!ExchangeRate.IsGenericRate);
			AssertEquals(5m, wrapper.CFXMinimum);

			// Wrapper will take CFX Minimum from non-generic ExchangeRate even for Creditor
			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(5m, wrapper.CFXMinimum);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(5m, wrapper.CFXMinimum);
		}

		public void TestCFXMinimum_CurrencyExcludedCFXCalculation()
		{
			Creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			Creator.CreateCFXUplift(Creator.LocalClient.CompanyData.AccCFXConfigurations, serviceDirection: ServiceDirection.Export, transportMode: TransportMode.Air, percentage: 10m);
			Creator.USD.RX_IsExcludedCFXCalculation = true;
			Factory.Save();
			Assert(RefCurrency.IsExcludedCFXCalculation(Creator.USD.Code));

			ExchangeRate.JF_RX_NKRateCurrency = Creator.USD.Code;
			ExchangeRate.JF_BaseRate = 1.5m;
			Assert(ExchangeRate.IsGenericRate);
			AssertEquals(decimal.Zero, ExchangeRate.JF_CFXMinimum);

			var wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(decimal.Zero, wrapper.CFXMinimum);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(decimal.Zero, wrapper.CFXMinimum);

			ExchangeRate.JF_CFXMinimum = 5m;

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(decimal.Zero, wrapper.CFXMinimum);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(decimal.Zero, wrapper.CFXMinimum);

			ExchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
			ExchangeRate.JF_OH_Org = Creator.LocalClient.PK;
			ExchangeRate.JF_CFXMinimum = decimal.Zero;
			Assert(!ExchangeRate.IsGenericRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(decimal.Zero, wrapper.CFXMinimum);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(decimal.Zero, wrapper.CFXMinimum);

			ExchangeRate.JF_CFXMinimum = 5m;

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(5m, wrapper.CFXMinimum);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(5m, wrapper.CFXMinimum);

			ExchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Creditor.ToCode();
			Assert(!ExchangeRate.IsGenericRate);
			AssertEquals(5m, wrapper.CFXMinimum);

			// Wrapper will take CFX Minimum from non-generic ExchangeRate even for Creditor
			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(5m, wrapper.CFXMinimum);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(5m, wrapper.CFXMinimum);
		}

		public void TestOrgPk()
		{
			ExchangeRate.JF_RX_NKRateCurrency = Creator.USD.Code;
			ExchangeRate.JF_BaseRate = 1.5m;
			Assert(ExchangeRate.IsGenericRate);
			Assert(ExchangeRate.JF_OH_Org.IsEmpty);

			var wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			Assert(wrapper.OrgPk.IsEmpty);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(Creator.LocalClient.PK, wrapper.OrgPk);

			exchangeRate.JF_OH_Org = Creator.LocalClient.PK;
			Assert(!ExchangeRate.IsGenericRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(Creator.LocalClient.PK, wrapper.OrgPk);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(Creator.LocalClient.PK, wrapper.OrgPk);
		}

		public void TestOrgType()
		{
			ExchangeRate.JF_RX_NKRateCurrency = Creator.USD.Code;
			ExchangeRate.JF_BaseRate = 1.5m;
			Assert(ExchangeRate.IsGenericRate);
			Assert(ExchangeRate.JF_OrgType.IsEmpty);

			var wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(ExchangeRateOrgTypeEnum.None, wrapper.OrgType);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(ExchangeRateOrgTypeEnum.None, wrapper.OrgType);

			ExchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Creditor.ToCode();
			Assert(!ExchangeRate.IsGenericRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(ExchangeRateOrgTypeEnum.Creditor, wrapper.OrgType);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(ExchangeRateOrgTypeEnum.Creditor, wrapper.OrgType);

			ExchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
			Assert(!ExchangeRate.IsGenericRate);

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(ExchangeRateOrgTypeEnum.Debtor, wrapper.OrgType);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(ExchangeRateOrgTypeEnum.Debtor, wrapper.OrgType);
		}

		public void TestCurrencyCode()
		{
			Assert(ExchangeRate.JF_RX_NKRateCurrency.IsEmpty);

			var wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			Assert(wrapper.CurrencyCode.IsEmpty);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			Assert(wrapper.CurrencyCode.IsEmpty);

			exchangeRate.JF_RX_NKRateCurrency = Creator.USD.RX_Code;

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(Creator.USD.RX_Code, wrapper.CurrencyCode);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(Creator.USD.RX_Code, wrapper.CurrencyCode);

			exchangeRate.JF_RX_NKRateCurrency = Creator.EUR.RX_Code;

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(Creator.EUR.RX_Code, wrapper.CurrencyCode);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(Creator.EUR.RX_Code, wrapper.CurrencyCode);
		}

		public void TestChanged()
		{
			var wasChangedCalled = false;
			var handler = new EventHandler((x, y) => wasChangedCalled = true);

			var wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			wrapper.Changed += handler;

			ExchangeRate.JF_RX_NKRateCurrency = Creator.USD.RX_Code;
			Assert(wasChangedCalled);

			wasChangedCalled = false;
			wrapper.Changed -= handler;

			ExchangeRate.JF_RX_NKRateCurrency = Creator.EUR.RX_Code;
			Assert(!wasChangedCalled);
		}

		public void TestIsUserDefinedOrTransformed()
		{
			Assert(ExchangeRate.JF_IsTransformed);

			var wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			Assert(wrapper.IsUserDefinedOrTransformed);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			Assert(wrapper.IsUserDefinedOrTransformed);

			ExchangeRate.JF_IsTransformed = false;

			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			Assert(!wrapper.IsUserDefinedOrTransformed);

			wrapper = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			Assert(!wrapper.IsUserDefinedOrTransformed);
		}

		public void TestSetBaseRate()
		{
			Assert(ExchangeRate.JF_BaseRate.IsEmpty);

			var wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			wrapper.SetBaseRate(1.5m);

			AssertEquals(1.5m, ExchangeRate.JF_BaseRate);

			wrapper.SetBaseRate(3m);

			AssertEquals(3m, ExchangeRate.JF_BaseRate);
		}

		public void TestRefreshCFXMinimum()
		{
			Creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			Creator.CreateCFXUplift(Creator.LocalClient.CompanyData.AccCFXConfigurations, serviceDirection: ServiceDirection.Export, transportMode: TransportMode.Air, percentage: 5m, minimum: 10m);

			Assert(ExchangeRate.JF_CFXMinimum.IsEmpty);
			ExchangeRate.JF_OH_Org = Creator.LocalClient.PK;
			Assert(ExchangeRate.JF_CFXPercent.IsEmpty);
			Assert(ExchangeRate.JF_CFXMinimum.IsEmpty);

			var wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			Assert(wrapper.CFXPercent.IsEmpty);
			Assert(wrapper.CFXMinimum.IsEmpty);
			AssertNotEquals(ExchangeRateOrgTypeEnum.Debtor, wrapper.OrgType);

			wrapper.RefreshCFXMinimum();    // Noithing updated because OrgType is not Debtor
			Assert(ExchangeRate.JF_CFXPercent.IsEmpty);
			Assert(ExchangeRate.JF_CFXMinimum.IsEmpty);
			Assert(wrapper.CFXPercent.IsEmpty);
			Assert(wrapper.CFXMinimum.IsEmpty);

			ExchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
			wrapper.RefreshCFXMinimum();// Now only Minimum should be updated
			Assert(ExchangeRate.JF_CFXPercent.IsEmpty);
			AssertEquals(10m, ExchangeRate.JF_CFXMinimum);
			Assert(wrapper.CFXPercent.IsEmpty);
			AssertEquals(10m, wrapper.CFXMinimum);
		}

		public void TestEnsureWillNotBeAutoDeleted()
		{
			ExchangeRate.JF_IsTransformed = false;

			var wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			Assert(!ExchangeRate.JF_IsTransformed);

			wrapper.EnsureWillNotBeAutoDeleted();
			Assert(ExchangeRate.JF_IsTransformed);
		}

		public void TestExchangeRatePk()
		{
			var wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			AssertEquals(ExchangeRate.PK, wrapper.ExchangeRatePk);
		}

		public void TestEquals()
		{
			Assert(ExchangeRate.JF_OH_Org.IsEmpty);

			var wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			Assert(!wrapper.Equals(null));
			Assert("Different types", !wrapper.Equals(ExchangeRate));

			var wrapperWithOrg = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			Assert("No Org and Org passed to constructor", !wrapper.Equals(wrapperWithOrg));

			ExchangeRate.JF_OH_Org = Creator.LocalClient.PK;
			Assert("Same Exchange Rate and Org", wrapper.Equals(wrapperWithOrg));
			AssertEquals(wrapper.OrgPk, wrapperWithOrg.OrgPk);

			ExchangeRate.JF_OH_Org = Creator.ABIGAS.PK;
			Assert("Same Exchange Rate and Org from Exchange Rate different to one passed to constructor", !wrapper.Equals(wrapperWithOrg));
			AssertNotEquals(wrapper.OrgPk, wrapperWithOrg.OrgPk);

			ExchangeRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();

			var exRate = Job.ExchangeRates.AddNew();
			exRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
			exRate.JF_OH_Org = Creator.ABIGAS.PK;

			var newWrapper = new ExchangeRateWrapper(exRate, null);
			Assert("Same Org but different Exchange Rates", !wrapper.Equals(newWrapper));
			AssertEquals(wrapper.OrgPk, newWrapper.OrgPk);
		}

		public void TestGetHashCode()
		{
			Assert(ExchangeRate.JF_OH_Org.IsEmpty);
			var wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			var wrapperWithOrg = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertNotEquals(wrapper.GetHashCode(), wrapperWithOrg.GetHashCode());

			ExchangeRate.JF_OH_Org = Creator.LocalClient.PK;
			wrapper = new ExchangeRateWrapper(ExchangeRate, null);
			wrapperWithOrg = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient);
			AssertEquals(wrapper.GetHashCode(), wrapperWithOrg.GetHashCode());

			var exRate = Job.ExchangeRates.AddNew();
			exRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
			exRate.JF_OH_Org = Creator.ABIGAS.PK;

			AssertNotEquals(wrapper.GetHashCode(), new ExchangeRateWrapper(exRate, null).GetHashCode());
		}

		public void TestIsDeleted()
		{
			var wrapper = new ExchangeRateWrapper(ExchangeRate, null);

			ExchangeRate.Delete();
			Assert("Precondition: IsDeleted", ExchangeRate.IsDeleted);
			AssertEquals("Wrapper IsDeleted should reflect underlying business object IsDeleted", true, wrapper.IsDeleted);
		}

		public void TestPropertiesToString()
		{
			ExchangeRate.JF_RX_NKRateCurrency = "USD";
			ExchangeRate.JF_BaseRate = 1.234m;
			ExchangeRate.JF_CFXPercent = 2m;
			ExchangeRate.JF_CFXMinimum = 1.5m;
			ExchangeRate.JF_OH_Org = Creator.LocalClient.PK;
			ExchangeRate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			ExchangeRate.JF_IsTransformed = false;

			var result = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient).PropertiesToString();
			var expected = $@"ExchangeRatePk: {ExchangeRate.PK}
CurrencyCode: USD
Rate: 1.234
SellRate: 1.20932
OrgPk: {Creator.LocalClient.PK}
OrgType: Debtor
CFXPercent: 2
CFXMinimum: 1.5
IsUserDefinedOrTransformed: No
IsDeleted: No";
			AssertEquals(expected, result);

			var exRateNoOrg = Job.ExchangeRates.AddNew();
			exRateNoOrg.JF_RX_NKRateCurrency = "EUR";
			exRateNoOrg.JF_BaseRate = 0.762m;
			exRateNoOrg.JF_CFXPercent = 0m;
			exRateNoOrg.JF_CFXMinimum = 0m;
			exRateNoOrg.JF_IsTransformed = true;

			var result2 = new ExchangeRateWrapper(exRateNoOrg, null).PropertiesToString();
			expected = $@"ExchangeRatePk: {exRateNoOrg.PK}
CurrencyCode: EUR
Rate: 0.762
SellRate: 0.762
OrgPk: 00000000-0000-0000-0000-000000000000
OrgType: None
CFXPercent: 0
CFXMinimum: 0
IsUserDefinedOrTransformed: Yes
IsDeleted: No";
			AssertEquals(expected, result2);

			var wrapper = new ExchangeRateWrapper(exRateNoOrg, null);
			var resultWithIndent = wrapper.PropertiesToString(indentCharDepth: 4);
			expected = $@"    ExchangeRatePk: {exRateNoOrg.PK}
    CurrencyCode: EUR
    Rate: 0.762
    SellRate: 0.762
    OrgPk: 00000000-0000-0000-0000-000000000000
    OrgType: None
    CFXPercent: 0
    CFXMinimum: 0
    IsUserDefinedOrTransformed: Yes
    IsDeleted: No";
			AssertEquals(expected, resultWithIndent);

			exRateNoOrg.Delete();
			var resultDeleted = wrapper.PropertiesToString();
			expected = $@"ExchangeRatePk: {exRateNoOrg.PK}
IsDeleted: Yes";
			AssertEquals(expected, resultDeleted);
		}

		public void TestPropertiesToString_CoversAllInterfaceProperties()
		{
			var propertyNames = typeof(IExchangeRateJobBilling)
				.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
				.Select(x => x.Name);

			var result = new ExchangeRateWrapper(ExchangeRate, Creator.LocalClient).PropertiesToString();
			CombineAssertions("IExchangeRateJobBilling.PropertiesToString() must include all public properties.", () =>
			{
				foreach (var name in propertyNames)
				{
					AssertContains($"Missing property '{name}'.", $"{name}:", result);
				}
			});
		}

		#region Implementation

		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;

		CommonShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = CommonShipment.New(Factory);
					shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
					shipment.JS_RL_NKOrigin = MasterFiles.Business.GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					shipment.JS_RL_NKDestination = "INBOM";
				}
				return shipment;
			}
		}
		CommonShipment shipment;

		Job Job => job ?? (job = Creator.CreateJob(Shipment, Creator.LocalClient, 0m, Creator.Agent, 0m));
		Job job;

		ExchangeRate ExchangeRate => exchangeRate ?? (exchangeRate = Job.ExchangeRates.AddNew());
		ExchangeRate exchangeRate;

		#endregion
	}
}

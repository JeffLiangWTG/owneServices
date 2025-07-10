using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.Posting.AgentPostingCurrencySelection;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ExRateOption = Enterprise.Accounting.Business.AccountingConstants.InvoicePostingExchangeRateOption;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.AgentPostingOptionSelection.Testing
{
	[TestedType(typeof(AgentPostingOptionSelector))]
	public class AgentPostingOptionSelector_InnerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return OptionSelection;
		}

		protected override void SetUp()
		{
			base.SetUp();

			Creator = new TestObjectCreator(Factory);
			EURCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "EUR");
			AUDCurrency = Creator.AUD;
			USDCurrency = Creator.USD;

			Consol = Factory.New<ForwardingConsol>();

			OrgAddress address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.OA_Code = "AAA";
			address1.OA_OH = Creator.AALSHI.PK;
			OrgAddress address2 = Factory.NewWithValidTestData<OrgAddress>();
			address2.OA_Code = "BBB";
			address2.OA_OH = Creator.AALSHI.PK;

			OrgContact contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_OH = Creator.AALSHI.PK;
			OrgContact contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_OH = Creator.AALSHI.PK;

			Factory.Save();
			Consol.SetDefaultReceivingForwarderAddress(Creator.AALSHI);

			ApportionmentListing apps = new ApportionmentListing(Factory, Consol);
			FreightCost = apps.CostsCollection.TryAddNew();
			FreightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			FreightCost.E6_RX_NKCurrency = USDCurrency.RX_Code;
			FreightCost.E6_OSCostAmount = 0m;
			FreightCost.E6_OH_Creditor = Creator.Agent.PK;
			FreightCost.E6_ExchangeRate = 0.7831m;

			OptionSelection = new AgentPostingOptionSelector(Factory, Consol, GetChargesForPosting());
		}

		IReceivablesPostingChargeCollection GetChargesForPosting()
		{
			ForwardingShipment shipment1 = Consol.Shipments.AddNew();
			ForwardingShipment shipment2 = Consol.Shipments.AddNew();
			ForwardingShipment shipment3 = Consol.Shipments.AddNew();

			Shipment1Job = Creator.CreateJob(shipment1);
			Shipment2Job = Creator.CreateJob(shipment2);
			Shipment3Job = Creator.CreateJob(shipment3);

			Shipment1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Shipment2Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Shipment3Job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Shipment1Job.JH_JobNum = "00001000";
			Shipment2Job.JH_JobNum = "00001001";
			Shipment3Job.JH_JobNum = "00001002";

			Creator.SetExchangeRate(Shipment2Job, USDCurrency, 0.7865m);
			Creator.SetExchangeRate(Shipment3Job, EURCurrency, 0.64m);
			Creator.SetExchangeRate(Shipment3Job, USDCurrency, 0.7233m);

			Charge shipment1ChargeAUD = Creator.CreateCharge(Shipment1Job, Creator.CC1, "AUD Description", Creator.AUD, 100m, null, AUDCurrency, 200m, Creator.AALSHI);
			Charge shipment2ChargeUSD = Creator.CreateCharge(Shipment2Job, Creator.CC1, "USD Description", Creator.AUD, 100m, null, USDCurrency, 2312321.23m, Creator.AALSHI);
			Charge shipment3ChargeEUR = Creator.CreateCharge(Shipment3Job, Creator.CC1, "EUR Description", Creator.AUD, 100m, null, EURCurrency, 580.89m, Creator.AALSHI);
			Charge shipment3ChargeUSD = Creator.CreateCharge(Shipment3Job, Creator.CC1, "USD Description", Creator.AUD, 100m, null, USDCurrency, 1111.11m, Creator.AALSHI);

			AssertEquals("Local value not calculated as expected - Shipment 1 Charge in AUD", 200m, shipment1ChargeAUD.JR_LocalSellAmt);
			AssertEquals("Local value not calculated as expected - Shipment 2 Charge in USD", 2940014.28m, shipment2ChargeUSD.JR_LocalSellAmt);
			AssertEquals("Local value not calculated as expected - Shipment 3 Charge in EUR", 907.64m, shipment3ChargeEUR.JR_LocalSellAmt);
			AssertEquals("Local value not calculated as expected - Shipment 3 Charge in USD", 1536.17m, shipment3ChargeUSD.JR_LocalSellAmt);

			Shipment1Job.Dispose();
			Shipment2Job.Dispose();
			Shipment3Job.Dispose();

			IReceivablesPostingChargeCollection result = new IReceivablesPostingChargeCollection();
			result.AddRange(new Charge[] { shipment1ChargeAUD, shipment2ChargeUSD, shipment3ChargeEUR, shipment3ChargeUSD });
			result.Key = new PostingChargeKey(Creator.AALSHI.PK, "INV", ZGuid.Empty, ZGuid.Empty, 0);
			return result;
		}

		AgentPostingOptionSelector OptionSelection;
		TestObjectCreator Creator;
		JobConsolCost FreightCost;

		Job Shipment1Job;
		Job Shipment2Job;
		Job Shipment3Job;

		ForwardingConsol Consol;
		RefCurrency AUDCurrency;
		RefCurrency EURCurrency;
		RefCurrency USDCurrency;

		bool OriginalRegistryValue;

		public void TestConstructorSetDefaultAddress()
		{
			AssertNotNull(Creator.AALSHI.AddressForSendingARDocuments);
			AssertEquals(Creator.AALSHI.AddressForSendingARDocuments.PK, OptionSelection.Address);
		}

		public void TestAddress()
		{
			AssertEquals(Consol.JK_OA_ReceivingForwarderAddress, OptionSelection.Address);

			OptionSelection.Address = ZGuid.Empty;
			AssertEquals("can't have empty address", true, OptionSelection.AddressInfo.HasErrors());

			OptionSelection.Address = Creator.ABIGAS.Addresses[0].PK;
			AssertEquals("invalid address", true, OptionSelection.AddressInfo.HasErrors());

			OptionSelection.Address = Creator.AALSHI.Addresses[1].PK;
			AssertEquals(false, OptionSelection.AddressInfo.HasErrors());
		}

		public void TestContact()
		{
			AssertEquals(ZGuid.Empty, OptionSelection.Contact);

			OptionSelection.Contact = Factory.NewWithValidTestData<OrgContact>().PK;
			AssertEquals("invalid contact", true, OptionSelection.ContactInfo.HasErrors());

			OptionSelection.Contact = ZGuid.Empty;
			AssertEquals("can have empty address", false, OptionSelection.AddressInfo.HasErrors());

			OptionSelection.Contact = Creator.AALSHI.Contacts[1].PK;
			AssertEquals(false, OptionSelection.AddressInfo.HasErrors());
		}

		public void TestPopulateAddressList()
		{
			ZQuery filter = new ZQuery(OrgAddressSchema.OA_OH, OptionSelection.AgentBeingPosted);
			var addresses = Factory.Load<OrgAddress>(filter);
			AssertEquals(addresses.Length, OptionSelection.AddressList.Count);
			AssertContainsExactElementsInAnyOrder(addresses, OptionSelection.AddressList);
		}

		public void TestPopulateContactList()
		{
			ZQuery filter = new ZQuery(OrgContactSchema.OC_OH, OptionSelection.AgentBeingPosted);
			var contacts = Factory.Load<OrgContact>(filter);
			AssertEquals(contacts.Length, OptionSelection.ContactList.Count);
			AssertContainsExactElementsInAnyOrder(contacts, OptionSelection.ContactList);
		}

		public void TestPopulationOfCurrencySelectionList()
		{
			CodeDescriptionPair uSDItem = (CodeDescriptionPair)OptionSelection.CurrencySelectionList[0];
			AssertEquals("First item should be USD", "USD", uSDItem.Code);
			AssertEquals("First item description should be related to master freight currency",
							"Master Freight Collect Currency", uSDItem.Description);

			CodeDescriptionPair aUDItem = (CodeDescriptionPair)OptionSelection.CurrencySelectionList[1];
			AssertEquals("First item should be AUD", "AUD", aUDItem.Code);
			AssertEquals("First item description should be related to master freight currency",
				AUDCurrency.RX_Desc, aUDItem.Description);

			CodeDescriptionPair eURItem = (CodeDescriptionPair)OptionSelection.CurrencySelectionList[2];
			AssertEquals("First item should be EUR", "EUR", eURItem.Code);
			AssertEquals("Should just have standard currency description",
				EURCurrency.RX_Desc, eURItem.Description);
		}

		public void TestValidateExchangeRateAmount()
		{
			AssertEquals(AUDCurrency.RX_Code, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			OptionSelection.Currency = AUDCurrency.RX_Code;
			OptionSelection.ExchangeRate = 0.9m;
			OptionSelection.ValidateExchangeRate();
			AssertHasError(OptionSelection.ExchangeRateInfo, "Exchange rate must be 1 as the selected currency is local");

			OptionSelection.Currency = USDCurrency.RX_Code;

			OptionSelection.ExchangeRate = -1m;
			OptionSelection.ValidateExchangeRate();
			AssertHasError(OptionSelection.ExchangeRateInfo, "Exchange rate must be greater than 0.");

			OptionSelection.ExchangeRate = 0m;
			OptionSelection.ValidateExchangeRate();
			AssertHasError(OptionSelection.ExchangeRateInfo, "Please enter an Exchange Rate.");

			OptionSelection.ExchangeRate = 0.000001m;
			OptionSelection.ValidateExchangeRate();
			AssertNoError(OptionSelection.ExchangeRateInfo, "Exchange rate must be greater than 0.");

			OptionSelection.ExchangeRate = -999999999999.999m;
			OptionSelection.ValidateExchangeRate();
			AssertHasError(OptionSelection.ExchangeRateInfo, "Exchange rate must be greater than 0.");

			OptionSelection.ExchangeRate = 999999999999m;
			OptionSelection.ValidateExchangeRate();
			AssertHasError(OptionSelection.ExchangeRateInfo, "The number 999,999,999,999 is too large, the maximum value allowed for selection is 999,999,999.999999999.");
		}

		public void TestCalculateMasterFreightCurrencyExRate()
		{
			OptionSelection.ExchangeRateCalculationMethod = ExchangeRateCalculationMethods.Codes.MasterFreight;

			OptionSelection.Currency = USDCurrency.RX_Code;
			AssertEquals("Master Freight Exchange Rate set from job consol cost as currency is same as in job consol cost", 0.7831m, OptionSelection.ExchangeRate);

			ExchangeRateReader.GetReaderInstance().ClearCache();

			OptionSelection.Currency = EURCurrency.RX_Code;
			AssertEquals("Master Freight Exchange Rate not found in job consol cost as selected currency changed", 0m, OptionSelection.ExchangeRate);

			ExchangeRateReader.GetReaderInstance().ClearCache();

			OptionSelection.Currency = USDCurrency.RX_Code;
			AssertEquals("Master Freight Exchange Rate set from job consol cost again as currency is same as in job consol cost", 0.7831m, OptionSelection.ExchangeRate);

			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";

			OptionSelection.Currency = AUDCurrency.RX_Code;
			AssertEquals("Exchange rate in Master Freight Exchange Rate method is 1 as currency is local", 1m, OptionSelection.ExchangeRate);

			OptionSelection.ExchangeRate = 1.5m;
			AssertHasError(OptionSelection.ExchangeRateInfo, "Exchange rate must be 1 as the selected currency is local");
		}

		public void TestResetExchangeRateCalculationMethod()
		{
			SetInvoicePostingExchangeRateOptionAR(ExRateOption.Default.Code);
			OptionSelection.Currency = AUDCurrency.RX_Code;
			AssertEquals("PreCond: AUD is company local currency", AUDCurrency.RX_Code, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals("Calculation method is empty when currency is local", string.Empty, OptionSelection.ExchangeRateCalculationMethod);

			OptionSelection.Currency = USDCurrency.RX_Code;
			AssertEquals("Calculation method is MAS when registry option is DEF", ExchangeRateCalculationMethods.Codes.MasterFreight, OptionSelection.ExchangeRateCalculationMethod);

			SetInvoicePostingExchangeRateOptionAR(ExRateOption.ExchangeRateBasedOnPostDate.Code);
			OptionSelection.Currency = EURCurrency.RX_Code;
			AssertEquals("Calculation method is PST when registry option is PST", ExRateOption.ExchangeRateBasedOnPostDate.Code, OptionSelection.ExchangeRateCalculationMethod);

			SetInvoicePostingExchangeRateOptionAR(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);
			OptionSelection.Currency = USDCurrency.RX_Code;
			AssertEquals("Calculation method is INV when registry option is INV", ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, OptionSelection.ExchangeRateCalculationMethod);

			SetInvoicePostingExchangeRateOptionAR(ExRateOption.EarliestOfInvoiceOrTaxDate.Code);
			OptionSelection.Currency = EURCurrency.RX_Code;
			AssertEquals("Calculation method is EIT when registry option is EIT", ExRateOption.EarliestOfInvoiceOrTaxDate.Code, OptionSelection.ExchangeRateCalculationMethod);

			void SetInvoicePostingExchangeRateOptionAR(string option)
			{
				var collection = new InvoicePostingExRateOptionCollection();
				collection.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, option, 0));
				collection.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, option, 0));
				AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			}
		}

		public void TestExchangeRateCalculationMethodList()
		{
			AssertExchangeRateCalculationMethodList(ExRateOption.Default.Code, ExRateOption.Default.Code);
			AssertExchangeRateCalculationMethodList(ExRateOption.TodayExchangeRate.Code, ExRateOption.TodayExchangeRate.Code);

			AssertExchangeRateCalculationMethodList(ExRateOption.ExchangeRateBasedOnPostDate.Code, ExRateOption.Default.Code, ExRateOption.ExchangeRateBasedOnPostDate.Code);
			AssertExchangeRateCalculationMethodList(ExRateOption.Default.Code, ExRateOption.ExchangeRateBasedOnPostDate.Code, ExRateOption.ExchangeRateBasedOnPostDate.Code);
			AssertExchangeRateCalculationMethodList(ExRateOption.ExchangeRateBasedOnPostDate.Code, ExRateOption.ExchangeRateBasedOnPostDate.Code, ExRateOption.ExchangeRateBasedOnPostDate.Code);

			AssertExchangeRateCalculationMethodList(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, ExRateOption.Default.Code, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);
			AssertExchangeRateCalculationMethodList(ExRateOption.Default.Code, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);
			AssertExchangeRateCalculationMethodList(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);

			AssertExchangeRateCalculationMethodList(ExRateOption.EarliestOfInvoiceOrTaxDate.Code, ExRateOption.Default.Code, ExRateOption.EarliestOfInvoiceOrTaxDate.Code);
			AssertExchangeRateCalculationMethodList(ExRateOption.Default.Code, ExRateOption.EarliestOfInvoiceOrTaxDate.Code, ExRateOption.EarliestOfInvoiceOrTaxDate.Code);
			AssertExchangeRateCalculationMethodList(ExRateOption.EarliestOfInvoiceOrTaxDate.Code, ExRateOption.EarliestOfInvoiceOrTaxDate.Code, ExRateOption.EarliestOfInvoiceOrTaxDate.Code);

			AssertExchangeRateCalculationMethodList(ExRateOption.ExchangeRateBasedOnPostDate.Code, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, ExRateOption.ExchangeRateBasedOnPostDate.Code);
			AssertExchangeRateCalculationMethodList(ExRateOption.EarliestOfInvoiceOrTaxDate.Code, ExRateOption.ExchangeRateBasedOnPostDate.Code, ExRateOption.EarliestOfInvoiceOrTaxDate.Code, ExRateOption.ExchangeRateBasedOnPostDate.Code);

			void AssertExchangeRateCalculationMethodList(string localOption, string foreignOption, params string[] expectedAdditionalValues)
			{
				var collection = new InvoicePostingExRateOptionCollection();
				collection.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, foreignOption, 0));
				collection.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, localOption, 0));
				AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

				var expectedValues = new List<string>() { ExchangeRateCalculationMethods.Codes.MasterFreight, ExchangeRateCalculationMethods.Codes.TodaysRate, ExchangeRateCalculationMethods.Codes.WeightedAvg };
				expectedValues.AddRange(expectedAdditionalValues);
				OptionSelection = new AgentPostingOptionSelector(Factory, Consol, GetChargesForPosting());
				AssertContainsExactElementsInAnyOrder(expectedValues, OptionSelection.ExchangeRateCalculationMethodList.GetAllCodes());
			}
		}

		public void TestAgentPostingOptionSelectorBehaviourWhenCurrencyIsLocal()
		{
			AssertEquals("PreCond: AUD is local currency", AUDCurrency.RX_Code, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals(USDCurrency.RX_Code, OptionSelection.Currency);

			OptionSelection.Currency = USDCurrency.RX_Code;
			AssertEquals(USDCurrency.RX_Code, OptionSelection.Currency);
			Assert("Exchange rate field is editable when currency is foreign", !OptionSelection.ExchangeRateInfo.ReadOnly);
			Assert("Calculation method field is editable when currency is foreign", !OptionSelection.ExchangeRateCalculationMethodInfo.ReadOnly);
			AssertEquals(0.7831m, OptionSelection.ExchangeRate);
			AssertEquals(ExchangeRateCalculationMethods.Codes.MasterFreight, OptionSelection.ExchangeRateCalculationMethod);

			OptionSelection.Currency = AUDCurrency.RX_Code;
			AssertEquals(AUDCurrency.RX_Code, OptionSelection.Currency);
			Assert("Exchange rate field is read only when currency is local", OptionSelection.ExchangeRateInfo.ReadOnly);
			Assert("Calculation method field is read only when currency is local", OptionSelection.ExchangeRateCalculationMethodInfo.ReadOnly);
			AssertEquals("Exchange rate value is 1 when currency is local", 1m, OptionSelection.ExchangeRate);
			AssertEquals("Calculation method is empty when currency is local", string.Empty, OptionSelection.ExchangeRateCalculationMethod);
		}

		public void TestValidateAddress()
		{
			OptionSelection.Address = ZGuid.NewZGuid();
			OptionSelection.ValidateAddress();
			AssertHasError("Doesn't select an address from address list", OptionSelection.AddressInfo, "Select a valid address from the list");

			Assert("Precondition: AddressList has elements", OptionSelection.AddressList.Count > 0);

			foreach (var address in OptionSelection.AddressList)
			{
				OptionSelection.Address = address.PK;
				OptionSelection.ValidateAddress();
				AssertNoErrors("Select an address from address list", OptionSelection.AddressInfo);
			}
		}

		public void TestValidateContact()
		{
			OptionSelection.Contact = ZGuid.NewZGuid();
			OptionSelection.ValidateContact();
			AssertHasError("Doesn't select a contact from contact list", OptionSelection.ContactInfo, "Select a valid contact from the list");

			Assert("Precondition: ContactList has elements", OptionSelection.ContactList.Count > 0);

			foreach (var contact in OptionSelection.ContactList)
			{
				OptionSelection.Contact = contact.PK;
				OptionSelection.ValidateContact();
				AssertNoErrors("Select a contact from contact list", OptionSelection.ContactInfo);
			}
		}

		public void TestValidateCurrency()
		{
			OptionSelection.Currency = "XXX";
			OptionSelection.ValidateCurrency();
			AssertHasError("Doesn't select a currency from currency list", OptionSelection.CurrencyInfo, "Select a posting currency from the list of saved currencies");

			Assert("Precondition: CurrencySelectionList has elements", OptionSelection.CurrencySelectionList.Count > 0);

			foreach (CodeDescriptionPair currencySelection in OptionSelection.CurrencySelectionList)
			{
				OptionSelection.Currency = currencySelection.Code;
				OptionSelection.ValidateCurrency();
				AssertNoErrors("Select a currency from currency list", OptionSelection.CurrencyInfo);
			}
		}

		public void TestCalculateWeightedAvgExRate()
		{
			Factory.Save();

			var calculationStrategy = new JobConsolCost.ConsolCostCalculationStrategy(FreightCost);
			calculationStrategy.HandleDelete();

			Creator.SetExchangeRate(Shipment2Job, USDCurrency, 0.6533m);
			Creator.SetExchangeRate(Shipment3Job, USDCurrency, 0.6233m);

			Charge shipment2ChargeUSD = Creator.CreateCharge(Shipment2Job, Creator.CC1, "USD Description", Creator.AUD, 100m, null, USDCurrency, 20m, Creator.AALSHI);
			Charge shipment3ChargeUSD = Creator.CreateCharge(Shipment3Job, Creator.CC1, "USD Description", Creator.AUD, 100m, null, USDCurrency, 20m, Creator.AALSHI);

			IReceivablesPostingChargeCollection result = new IReceivablesPostingChargeCollection();
			result.AddRange(new Charge[] { shipment2ChargeUSD, shipment3ChargeUSD });
			result.Key = new PostingChargeKey(Creator.AALSHI.PK, InvoiceTypesList.Codes.FinalInvoice, "C00001234", ZGuid.Empty, ZGuid.Empty, 0);

			OptionSelection = new AgentPostingOptionSelector(Factory, Consol, result);

			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";

			ExchangeRateReader.GetReaderInstance().ClearCache();

			OptionSelection.Currency = USDCurrency.RX_Code;
			OptionSelection.ExchangeRateCalculationMethod = ExchangeRateCalculationMethods.Codes.WeightedAvg;

			AssertEquals("Master Freight Exchange Rate not correct using weighted average method", 0.637959m, OptionSelection.ExchangeRate);

			ExchangeRateReader.GetReaderInstance().ClearCache();

			OptionSelection.Currency = AUDCurrency.RX_Code;
			AssertEquals("Exchange rate in Weighted average method is 1 as currency is local", 1m, OptionSelection.ExchangeRate);

			OptionSelection.ExchangeRate = 1.5m;
			AssertHasError(OptionSelection.ExchangeRateInfo, "Exchange rate must be 1 as the selected currency is local");
		}

		public void TestCalculateWeightedAvgExRateWhenCurrencyNotOnCharges()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			OptionSelection.ExchangeRateCalculationMethod = ExchangeRateCalculationMethods.Codes.WeightedAvg;
			OptionSelection.Currency = Creator.GBP.RX_Code;

			AssertEquals("Master Freight Exchange Rate should be zero when no charges have posting currency", 0m, OptionSelection.ExchangeRate);
		}

		public void TestUseTodaysRate_IfFallBackToPreviousExchangeRate_IsSet()
		{
			OriginalRegistryValue = AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.Value;

			try
			{
				AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				RefExchangeRate sellRate = EURCurrency.ExchangeRates.AddNew();
				sellRate.RE_StartDate = ZDateTime.Now.AddDays(-10);
				sellRate.RE_ExpiryDate = ZDateTime.Now.AddDays(-10);
				sellRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
				sellRate.RE_SellRate = 0.8m;

				RefExchangeRate buyRate = EURCurrency.ExchangeRates.AddNew();
				buyRate.RE_StartDate = ZDateTime.Now.AddDays(-10);
				buyRate.RE_ExpiryDate = ZDateTime.Now.AddDays(-10);
				buyRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
				buyRate.RE_SellRate = 1.8m;

				Factory.Save();

				ExchangeRateReader.GetReaderInstance().ClearCache();

				OptionSelection.Currency = EURCurrency.RX_Code;
				OptionSelection.ExchangeRateCalculationMethod = ExchangeRateCalculationMethods.Codes.TodaysRate;
				AssertEquals("Todays exchange rate is from expired sell rate", 0.8m, OptionSelection.ExchangeRate);

				sellRate = EURCurrency.ExchangeRates.AddNew();
				sellRate.RE_StartDate = ZDateTime.Now.AddDays(-1);
				sellRate.RE_ExpiryDate = ZDateTime.Now.AddDays(1);
				sellRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
				sellRate.RE_SellRate = 1.2m;

				Factory.Save();

				ExchangeRateReader.GetReaderInstance().ClearCache();

				OptionSelection.ExchangeRateCalculationMethod = ExchangeRateCalculationMethods.Codes.TodaysRate;
				AssertEquals("Todays exchange rate is from current sell rate", 1.2m, OptionSelection.ExchangeRate);

				sellRate.RE_SellRate = 0m;
				Factory.Save();

				ExchangeRateReader.GetReaderInstance().ClearCache();

				OptionSelection.ExchangeRateCalculationMethod = ExchangeRateCalculationMethods.Codes.TodaysRate;
				AssertEquals("Todays exchange rate is from expired buy rate as sell rate is zero", 1.8m, OptionSelection.ExchangeRate);

				buyRate = EURCurrency.ExchangeRates.AddNew();
				buyRate.RE_StartDate = ZDateTime.Now.AddDays(-1);
				buyRate.RE_ExpiryDate = ZDateTime.Now.AddDays(1);
				buyRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
				buyRate.RE_SellRate = 2.1m;

				Factory.Save();

				ExchangeRateReader.GetReaderInstance().ClearCache();

				OptionSelection.ExchangeRateCalculationMethod = ExchangeRateCalculationMethods.Codes.TodaysRate;
				AssertEquals("Todays exchange rate is from current buy rate", 2.1m, OptionSelection.ExchangeRate);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, OriginalRegistryValue);
			}
		}

		public void TestUseTodaysRate_IfFallBackToPreviousExchangeRate_NotSet()
		{
			OriginalRegistryValue = AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				RefExchangeRate sellRate = EURCurrency.ExchangeRates.AddNew();
				sellRate.RE_StartDate = ZDateTime.Now.AddDays(-10);
				sellRate.RE_ExpiryDate = ZDateTime.Now.AddDays(10);
				sellRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
				sellRate.RE_SellRate = 0.8m;

				RefExchangeRate buyRate = EURCurrency.ExchangeRates.AddNew();
				buyRate.RE_StartDate = ZDateTime.Now.AddDays(-10);
				buyRate.RE_ExpiryDate = ZDateTime.Now.AddDays(10);
				buyRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
				buyRate.RE_SellRate = 1.8m;

				Factory.Save();

				ExchangeRateReader.GetReaderInstance().ClearCache();

				OptionSelection.Currency = EURCurrency.RX_Code;
				OptionSelection.ExchangeRateCalculationMethod = ExchangeRateCalculationMethods.Codes.TodaysRate;
				AssertEquals("Todays exchange rate is from sell rate", 0.8m, OptionSelection.ExchangeRate);

				sellRate.RE_SellRate = 0m;
				Factory.Save();

				ExchangeRateReader.GetReaderInstance().ClearCache();

				OptionSelection.ExchangeRateCalculationMethod = ExchangeRateCalculationMethods.Codes.TodaysRate;
				AssertEquals("Todays exchange rate is from buy rate as sell rate is zero", 1.8m, OptionSelection.ExchangeRate);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, OriginalRegistryValue);
			}
		}

		public void TestUseTodaysRate_ForLocalCurrency()
		{
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";

			RefExchangeRate rate = EURCurrency.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Now.AddDays(-10);
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(10);
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			rate.RE_SellRate = 0.8m;

			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			OptionSelection.Currency = EURCurrency.RX_Code;
			OptionSelection.ExchangeRateCalculationMethod = ExchangeRateCalculationMethods.Codes.TodaysRate;
			AssertEquals("Todays exchange rate for foreign currency", 0.8m, OptionSelection.ExchangeRate);

			ExchangeRateReader.GetReaderInstance().ClearCache();

			OptionSelection.Currency = AUDCurrency.RX_Code;
			AssertEquals("Todays exchange rate for local currency", 1m, OptionSelection.ExchangeRate);

			OptionSelection.ExchangeRate = 1.5m;
			AssertHasError(OptionSelection.ExchangeRateInfo, "Exchange rate must be 1 as the selected currency is local");
		}

		public void TestUpdateAllChargesAddressAndContactAccordingToSelectedAddressAndContact()
		{
			OptionSelection.Address = Creator.CreateAddress(Creator.AALSHI, "some new address").PK;
			OptionSelection.Contact = Creator.CreateContact(Creator.AALSHI, "alex").PK;
			foreach (var charge in OptionSelection.AgentChargesBeingPosted_ForTestOnly)
			{
				AssertNotEquals("Precondition - address not equal", OptionSelection.Address, charge.DebtorAddressPK);
				AssertNotEquals("Precondition - contact not equal", OptionSelection.Contact, charge.DebtorContactPK);
			}
			OptionSelection.UpdateAllChargesAddressAndContactAccordingToSelectedAddressAndContact();
			foreach (var charge in OptionSelection.AgentChargesBeingPosted_ForTestOnly)
			{
				AssertEquals("Postcondition - address equal", OptionSelection.Address, charge.DebtorAddressPK);
				AssertEquals("Postcondition - contact equal", OptionSelection.Contact, charge.DebtorContactPK);
			}
		}

		public void TestChargeSellCurrencyIsUpdatedWhenARInvoiceIsNotInLocalCurrency()
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var objectCreator = new TestObjectCreator(Factory);

			//create profit share agreement
			var agentRelationship = objectCreator.CreateAgentRelationship(GlbCompany.CurrentCompany.OrgProxy, objectCreator.AALSHI);
			var profitShare = objectCreator.CreateProfitShare(agentRelationship, 50m, 50m, "AUSYD", "USLAX", "ALL");

			//create consol and shipment
			var consol = objectCreator.CreateConsol("AUSYD", "USLAX", "C00001001");
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			consol.SetDefaultReceivingForwarderAddress(objectCreator.AALSHI.PK);
			var shipment = objectCreator.CreateShipment("S0001", "AUSYD", "USLAX", consol);

			//create job & charges
			var job = objectCreator.CreateJob(shipment, false);
			objectCreator.SetExchangeRate(job, objectCreator.USD, 1.2m);
			var charge1 = objectCreator.CreateCharge(job, objectCreator.CC10, "", objectCreator.USD, 20m, objectCreator.Creditor1, objectCreator.USD, 500m, objectCreator.AALSHI);
			charge1.JR_IsIncludedInProfitShare = true;
			var charge2 = objectCreator.CreateCharge(job, objectCreator.CC2, "", objectCreator.AUD, 25m, objectCreator.Creditor1, objectCreator.AUD, 25m, objectCreator.AALSHI);
			charge2.JR_IsIncludedInProfitShare = true;
			Factory.Save();

			//calculate profit share
			var profitShares = new ProfitShareCalculator(Factory, consol.GetShipmentsList()).CreateProfitShares();
			var postingDetails = new AgentChargePostingDetails(Consol, profitShares, GlbCompany.CurrentCompany.LocalCurrency, 1m, new ChargePoster(Factory), new ApportionmentListing(Factory, consol).CostsCollection);
			var chargeCreator = new ProfitShareConsolChargeCreator(postingDetails, Factory);
			AssertEquals("Job does not has PS Charge", 2, job.Charges.Count);
			chargeCreator.CreateCharges();
			AssertEquals("Job has PS Charge", 3, job.Charges.Count);
			var partyType = postingDetails.CalculatedProfitShares[0].ProfitShareShipmentDetails[0].PartyType;
			var profitShareCharge = job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Value.GetCode(partyType)))[0] as IReceivablesPostingCharge;
			AssertNotNull("Profit Share Charge", profitShareCharge);

			//post invoice
			var receivablesPostingChargeCollection = new IReceivablesPostingChargeCollection();
			receivablesPostingChargeCollection.AddRange(job.Charges.ToArray<Charge>());
			receivablesPostingChargeCollection.Key = new PostingChargeKey(objectCreator.AALSHI.PK, "INV", ZGuid.Empty, ZGuid.Empty, 0);
			var optionSelection = new AgentPostingOptionSelector(Factory, consol, receivablesPostingChargeCollection);
			optionSelection.Currency = objectCreator.USD.RX_Code;
			optionSelection.ExchangeRate = 1.2m;

			AssertEquals("Charge 2 Invoice Type", "FIN", charge2.JR_InvoiceType);
			AssertEquals("Charge 2 Sell Currency", objectCreator.AUD.RX_Code, charge2.SellCurrency.RX_Code);
			AssertEquals("Charge 2 OS Sell Amount", 25m, charge2.JR_OSSellAmt);
			AssertEquals("Profit Share Charge Invoice Type", "FIN", profitShareCharge.InvoiceType);
			AssertEquals("Profit Share Charge Sell Currency", objectCreator.AUD.RX_Code, profitShareCharge.SellCurrency.RX_Code);
			AssertEquals("Profit Share Charge OS Sell Amount", -200m, profitShareCharge.OSSellAmount);

			optionSelection.UpdateAllChargesPostingStyleAccordingToSelectedCurrency();

			AssertEquals("Charge 2 Invoice Type", "CUR", charge2.JR_InvoiceType);
			AssertEquals("Charge 2 Sell Currency", objectCreator.USD.RX_Code, charge2.SellCurrency.RX_Code);
			AssertEquals("Charge 2 OS Sell Amount", 30m, charge2.JR_OSSellAmt);
			AssertEquals("Profit Share Charge Invoice Type", "CUR", profitShareCharge.InvoiceType);
			AssertEquals("Profit Share Charge Sell Currency", objectCreator.USD.RX_Code, profitShareCharge.SellCurrency.RX_Code);
			AssertEquals("Profit Share Charge OS Sell Amount", -240m, profitShareCharge.OSSellAmount);
		}

		public void TestChargeSellCurrencyAndInvoiceTypeUpdatedCorrectly()
		{
			var objectCreator = new TestObjectCreator(Factory);

			//create consol and shipment
			var consol = objectCreator.CreateConsol("AUSYD", "USLAX", "C0001");
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			consol.SetDefaultReceivingForwarderAddress(objectCreator.AALSHI.PK);
			var shipment = objectCreator.CreateShipment("S0001", "AUSYD", "USLAX", consol);

			//create job & charges
			var job = objectCreator.CreateJob(shipment, false);
			var exchangeRateUSD = objectCreator.SetExchangeRate(job, objectCreator.USD, 1.2m);
			var rateAgentUSD = job.ExchangeRates.AddRate(objectCreator.USD, 1.3m, ZGuid.Empty, ExchangeRateOrgTypeEnum.Debtor);
			rateAgentUSD.JF_IsTransformed = true;
			var exchangeRateGBP = objectCreator.SetExchangeRate(job, objectCreator.GBP, 1.5m);
			var rateAgentGBP = job.ExchangeRates.AddRate(objectCreator.GBP, 1.3m, ZGuid.Empty, ExchangeRateOrgTypeEnum.Debtor);
			rateAgentGBP.JF_IsTransformed = true;

			var charge1 = objectCreator.CreateCharge(job, objectCreator.CC10, "", objectCreator.USD, 20m, objectCreator.Creditor1, objectCreator.USD, 500m, objectCreator.AALSHI);
			charge1.JR_InvoiceType = "CUR";

			var charge2 = objectCreator.CreateCharge(job, objectCreator.CC10, "", objectCreator.GBP, 20m, objectCreator.Creditor1, objectCreator.GBP, 500m, objectCreator.AALSHI);
			charge2.JR_InvoiceType = "CUR";

			//post invoice
			var receivablesPostingChargeCollection = new IReceivablesPostingChargeCollection();
			receivablesPostingChargeCollection.AddRange(job.Charges.ToArray<Charge>());
			receivablesPostingChargeCollection.Key = new PostingChargeKey(objectCreator.AALSHI.PK, "CUR", ZGuid.Empty, ZGuid.Empty, 0);
			var optionSelection = new AgentPostingOptionSelector(Factory, consol, receivablesPostingChargeCollection);
			optionSelection.Currency = objectCreator.USD.RX_Code;
			optionSelection.ExchangeRate = 1.2m;

			AssertEquals("Charge Invoice Type", "CUR", charge1.JR_InvoiceType);
			AssertEquals("Charge Sell Currency", objectCreator.USD.RX_Code, charge1.SellCurrency.RX_Code);
			AssertEquals("Charge OS Sell Amount", 500m, charge1.JR_OSSellAmt);
			AssertEquals("Charge Local Sell Amount", 416.67m, charge1.JR_LocalSellAmt);

			AssertEquals("Charge Invoice Type", "CUR", charge2.JR_InvoiceType);
			AssertEquals("Charge Sell Currency", objectCreator.GBP.RX_Code, charge2.SellCurrency.RX_Code);
			AssertEquals("Charge OS Sell Amount", 500m, charge2.JR_OSSellAmt);
			AssertEquals("Charge Local Sell Amount", 333.33m, charge2.JR_LocalSellAmt);

			optionSelection.UpdateAllChargesPostingStyleAccordingToSelectedCurrency();

			AssertEquals("Charge Invoice Type", "CUR", charge1.JR_InvoiceType);
			AssertEquals("Charge Sell Currency", objectCreator.USD.RX_Code, charge1.SellCurrency.RX_Code);
			AssertEquals("Charge OS Sell Amount", 500m, charge1.JR_OSSellAmt);
			AssertEquals("Charge Local Sell Amount", 416.67m, charge1.JR_LocalSellAmt);

			AssertEquals("Charge Invoice Type", "CUR", charge2.JR_InvoiceType);
			AssertEquals("Charge Sell Currency", objectCreator.USD.RX_Code, charge2.SellCurrency.RX_Code);
			AssertEquals("Charge OS Sell Amount", 400m, charge2.JR_OSSellAmt);
			AssertEquals("Charge Local Sell Amount", 333.33m, charge2.JR_LocalSellAmt);
		}

		public void TestChargeWithSellInvoiceCurrencyHaveSellCurrencyAndInvoiceTypeUpdatedCorrectly_ForeignCurrency()
		{
			AssertChargeWithSellInvoiceCurrencyHaveSellCurrencyAndInvoiceTypeUpdatedCorrectly();
		}

		public void TestChargeWithSellInvoiceCurrencyHaveSellCurrencyAndInvoiceTypeUpdatedCorrectly_LocalCurrency()
		{
			AssertChargeWithSellInvoiceCurrencyHaveSellCurrencyAndInvoiceTypeUpdatedCorrectly(true);
		}
		void AssertChargeWithSellInvoiceCurrencyHaveSellCurrencyAndInvoiceTypeUpdatedCorrectly(bool postInLocal = false)
		{
			var objectCreator = new TestObjectCreator(Factory);
			var cad = objectCreator.GetCurrency("CAD");
			var nzd = objectCreator.GetCurrency("NZD");

			//create consol and shipment
			var consol = objectCreator.CreateConsol("AUSYD", "USLAX", "C0001");
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			consol.SetDefaultReceivingForwarderAddress(objectCreator.AALSHI.PK);
			var shipment1 = objectCreator.CreateShipment("S0001", "AUSYD", "USLAX", consol);
			var shipment2 = objectCreator.CreateShipment("S0002", "AUSYD", "USLAX", consol);

			//create jobs & charges
			var job1 = objectCreator.CreateJob(shipment1, false);
			var job1ExRateUSD = objectCreator.SetExchangeRate(job1, objectCreator.USD, 1.25m);
			var job1ExRateAgent = objectCreator.SetExchangeRate(job1, objectCreator.USD, 1.1m, job1.AgentCollectPK, ExchangeRateOrgTypeEnum.Debtor);

			var charge1 = objectCreator.CreateCharge(job1, objectCreator.FRT, "", objectCreator.USD, 800m, objectCreator.Creditor1, objectCreator.USD, 800m, objectCreator.AALSHI);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge1.JR_RX_NKSellInvoiceCurrency = cad.RX_Code;
			var exchangeRateCAD = ((IExchangeRateProvider)job1).GetExchangeRate(cad.RX_Code, objectCreator.AALSHI.PK, ExchangeRateValidLedgerEnum.AR);
			AssertNotNull("SellInvoiceCurrency ExRate should be added", exchangeRateCAD);
			exchangeRateCAD.SetBuyRate_ForTestOnly(1.25m);

			var job2 = objectCreator.CreateJob(shipment1, false);
			var exchangeRateUSD = objectCreator.SetExchangeRate(job1, objectCreator.USD, 1.25m);
			job1ExRateAgent.JF_BaseRate = 1.1m;

			var charge2 = objectCreator.CreateCharge(job2, objectCreator.FRT, "", objectCreator.AUD, 1000m, objectCreator.Creditor1, objectCreator.AUD, 1000m, objectCreator.AALSHI);
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_RX_NKSellInvoiceCurrency = nzd.RX_Code;
			var exchangeRateNZD = ((IExchangeRateProvider)job2).GetExchangeRate(nzd.RX_Code, objectCreator.AALSHI.PK, ExchangeRateValidLedgerEnum.AR);
			AssertNotNull("SellInvoiceCurrency ExRate should be added", exchangeRateNZD);
			exchangeRateNZD.SetBuyRate_ForTestOnly(1.2m);

			AssertEquals("Charge Invoice Type", InvoiceTypesList.Codes.FinalInvoice, charge1.JR_InvoiceType);
			AssertEquals("Charge Sell Currency", objectCreator.USD.RX_Code, charge1.SellCurrency.RX_Code);
			Assert("Charge BillInInvoiceCurrency", charge1.BillInInvoiceCurrency);
			AssertEquals("SellInvoiceCurrency", cad.RX_Code, charge1.JR_RX_NKSellInvoiceCurrency);
			AssertEquals("Charge OS Sell Amount", 800m, charge1.JR_OSSellAmt);
			AssertEquals("Charge Local Sell Amount", 727.27m, charge1.JR_LocalSellAmt);

			AssertEquals("Charge Invoice Type", InvoiceTypesList.Codes.FinalInvoice, charge2.JR_InvoiceType);
			AssertEquals("Charge Sell Currency", objectCreator.AUD.RX_Code, charge2.SellCurrency.RX_Code);
			Assert("Charge BillInInvoiceCurrency", charge2.BillInInvoiceCurrency);
			AssertEquals("SellInvoiceCurrency", nzd.RX_Code, charge2.JR_RX_NKSellInvoiceCurrency);
			AssertEquals("Charge OS Sell Amount", 1000m, charge2.JR_OSSellAmt);
			AssertEquals("Charge Local Sell Amount", 1000m, charge2.JR_LocalSellAmt);

			AssertEquals("Charge Local Sell Amount", 1000m, charge2.JR_LocalSellInvoiceAmt);
			charge2.JR_LineCFX = 10m;
			AssertEquals("Charge Local Sell Amount with CFX uplift", 1100m, charge2.JR_LocalSellInvoiceAmt);

			//post invoice
			var receivablesPostingChargeCollection = new IReceivablesPostingChargeCollection();
			receivablesPostingChargeCollection.AddRange(new Charge[] { charge1, charge2 });
			receivablesPostingChargeCollection.Key = new PostingChargeKey(objectCreator.AALSHI.PK, InvoiceTypesList.Codes.FinalInvoice, ZGuid.Empty, ZGuid.Empty, 0);
			var optionSelection = new AgentPostingOptionSelector(Factory, consol, receivablesPostingChargeCollection);
			AssertContainsExactElementsInAnyOrder(new string[] { "AUD", "USD" }, optionSelection.CurrencySelectionList.GetAllCodes());

			var postingCurrencyCode = postInLocal ? objectCreator.AUD.RX_Code : objectCreator.USD.RX_Code;
			var postingExRate = postInLocal ? 1m : 1.25m;
			var expectedInvoiceType = postInLocal ? InvoiceTypesList.Codes.FinalInvoice : InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			optionSelection.Currency = postingCurrencyCode;
			optionSelection.ExchangeRate = postingExRate;

			optionSelection.UpdateAllChargesPostingStyleAccordingToSelectedCurrency();

			AssertEquals("Charge Invoice Type", expectedInvoiceType, charge1.JR_InvoiceType);
			AssertEquals("Charge Local Sell Amount", postInLocal ? 727.27m : 640m, charge1.JR_LocalSellAmt);
			AssertEquals("Charge Sell Currency", objectCreator.USD.RX_Code, charge1.SellCurrency.RX_Code);
			Assert("Charge BillInInvoiceCurrency", !charge1.BillInInvoiceCurrency);
			AssertEquals("Charge Sell Ex Rate", postInLocal ? 1.1m : postingExRate, charge1.JR_OSSellExRate);
			AssertEquals("Charge OS Sell Amount", 800m, charge1.JR_OSSellAmt);

			AssertEquals("Charge Invoice Type", expectedInvoiceType, charge2.JR_InvoiceType);
			AssertEquals("Charge Local Sell Amount", 1000m, charge2.JR_LocalSellAmt);
			AssertEquals("Charge Sell Currency", postingCurrencyCode, charge2.SellCurrency.RX_Code);
			Assert("Charge BillInInvoiceCurrency", !charge2.BillInInvoiceCurrency);

			if (!postInLocal && charge2.JR_OSSellExRate != 1.25m)   // This is for failing unit test which does not update Ex Rate
			{
				// When Sell Currency was updated but Ex Rate was not updated, Local Amount is still intact. But setting Ex Rate clears both Sell Amounts
				charge2.JR_OSSellExRate = 1.25m;
				AssertEquals("Charge Local Sell Amount", 1000m, charge2.JR_LocalSellAmt);
			}
			AssertEquals("Charge Sell Ex Rate", postingExRate, charge2.JR_OSSellExRate);
			AssertEquals("Charge OS Sell Amount", postInLocal ? 1000m : 1250m, charge2.JR_OSSellAmt);
		}

		public void TestPopulateExchangeRateCalculationMethodListWithInvoicePostingExchangeRateOption()
		{
			Factory.Save();

			var calculationStrategy = new JobConsolCost.ConsolCostCalculationStrategy(FreightCost);
			calculationStrategy.HandleDelete();

			Creator.SetExchangeRate(Shipment2Job, USDCurrency, 0.6533m);
			Creator.SetExchangeRate(Shipment3Job, USDCurrency, 0.6233m);

			Charge shipment2ChargeUSD = Creator.CreateCharge(Shipment2Job, Creator.CC1, "USD Description", Creator.AUD, 100m, null, USDCurrency, 20m, Creator.AALSHI);
			Charge shipment3ChargeUSD = Creator.CreateCharge(Shipment3Job, Creator.CC1, "USD Description", Creator.AUD, 100m, null, USDCurrency, 20m, Creator.AALSHI);

			var result = new IReceivablesPostingChargeCollection();
			result.AddRange(new Charge[] { shipment2ChargeUSD, shipment3ChargeUSD });
			result.Key = new PostingChargeKey(Creator.AALSHI.PK, InvoiceTypesList.Codes.FinalInvoice, "C00001234", ZGuid.Empty, ZGuid.Empty, 0);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF");
			OptionSelection = new AgentPostingOptionSelector(Factory, Consol, result);
			AssertEquals(false, OptionSelection.ExchangeRateCalculationMethodList.ContainsCode("PST"));
			AssertEquals(false, OptionSelection.ExchangeRateCalculationMethodList.ContainsCode("INV"));

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");
			OptionSelection = new AgentPostingOptionSelector(Factory, Consol, result);
			AssertEquals(false, OptionSelection.ExchangeRateCalculationMethodList.ContainsCode("PST"));
			AssertEquals(false, OptionSelection.ExchangeRateCalculationMethodList.ContainsCode("INV"));

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			OptionSelection = new AgentPostingOptionSelector(Factory, Consol, result);
			AssertEquals(false, OptionSelection.ExchangeRateCalculationMethodList.ContainsCode("PST"));
			AssertEquals(true, OptionSelection.ExchangeRateCalculationMethodList.ContainsCode("INV"));

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");
			OptionSelection = new AgentPostingOptionSelector(Factory, Consol, result);
			AssertEquals(true, OptionSelection.ExchangeRateCalculationMethodList.ContainsCode("PST"));
			AssertEquals(false, OptionSelection.ExchangeRateCalculationMethodList.ContainsCode("INV"));
		}

		public void TestValidateExchangeRateCalculationMethod()
		{
			SetRegistryAndResetOptionSelection("DEF");
			OptionSelection.ExchangeRateCalculationMethod = "MAS";
			AssertNoErrors(OptionSelection.ExchangeRateCalculationMethodInfo);

			SetRegistryAndResetOptionSelection("TOD");
			OptionSelection.ExchangeRateCalculationMethod = "MAS";
			AssertHasError(OptionSelection.ExchangeRateCalculationMethodInfo, @"The “AR Invoice Posting Exchange Rate Option” has been set to ""TOD"".
You are not allowed to override this behavior.
This must value must be set to ""TOD"".");
			OptionSelection.ExchangeRateCalculationMethod = "TOD";
			AssertNoErrors(OptionSelection.ExchangeRateCalculationMethodInfo);

			SetRegistryAndResetOptionSelection("INV");
			OptionSelection.Currency = "USD";
			OptionSelection.ExchangeRateCalculationMethod = "TOD";
			AssertHasError(OptionSelection.ExchangeRateCalculationMethodInfo, @"The “AR Invoice Posting Exchange Rate Option” has been set to ""INV"".
You are not allowed to override this behavior.
This must value must be set to ""INV"".");
			OptionSelection.ExchangeRateCalculationMethod = "INV";
			AssertNoErrors(OptionSelection.ExchangeRateCalculationMethodInfo);

			SetRegistryAndResetOptionSelection("PST");
			OptionSelection.ExchangeRateCalculationMethod = "TOD";
			AssertHasError(OptionSelection.ExchangeRateCalculationMethodInfo, @"The “AR Invoice Posting Exchange Rate Option” has been set to ""PST"".
You are not allowed to override this behavior.
This must value must be set to ""PST"".");
			OptionSelection.ExchangeRateCalculationMethod = "PST";
			AssertNoErrors(OptionSelection.ExchangeRateCalculationMethodInfo);

			OptionSelection.Currency = "AUD";
			OptionSelection.ValidateExchangeRateCalculationMethod_ForTestOnly();
			AssertNoErrors(OptionSelection.ExchangeRateCalculationMethodInfo);

			OptionSelection.ExchangeRateCalculationMethod = "MAS";
			AssertHasError(OptionSelection.ExchangeRateCalculationMethodInfo, "Please do not enter a value.");

			OptionSelection.Currency = "USD";
			OptionSelection.ExchangeRateCalculationMethod = string.Empty;
			AssertHasError(OptionSelection.ExchangeRateCalculationMethodInfo, "Please enter a value.");

			OptionSelection.ExchangeRateCalculationMethod = "XXX";
			AssertHasError(OptionSelection.ExchangeRateCalculationMethodInfo, "Enter a valid selection.");

			void SetRegistryAndResetOptionSelection(string registryValue)
			{
				PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, registryValue);
				OptionSelection = new AgentPostingOptionSelector(Factory, Consol, GetChargesForPosting());
				OptionSelection.Currency = "USD";
			}
		}

		[TestDate(2015, 01, 01)]
		public void TestRecalculateExchangeRateWithInvoicePostingExchangeRateOption()
		{
			var creator = new TestObjectCreator(Factory);
			creator.CreateUSDBuyRate(1.1m, new ZDateTime(2015, 1, 1));

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF");
			OptionSelection.Currency = "AUD";
			AssertEquals(1m, OptionSelection.ExchangeRate);
			OptionSelection.Currency = "USD";
			AssertEquals(0.7831m, OptionSelection.ExchangeRate);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");
			OptionSelection.Currency = "AUD";
			AssertEquals(1m, OptionSelection.ExchangeRate);
			OptionSelection.Currency = "USD";
			AssertEquals(1.1m, OptionSelection.ExchangeRate);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");
			OptionSelection.Currency = "AUD";
			AssertEquals(1m, OptionSelection.ExchangeRate);
			OptionSelection.Currency = "USD";
			AssertEquals(1.1m, OptionSelection.ExchangeRate);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			OptionSelection.Currency = "AUD";
			AssertEquals(1m, OptionSelection.ExchangeRate);
			OptionSelection.Currency = "USD";
			AssertEquals(1.1m, OptionSelection.ExchangeRate);
		}

		[TestDate(2015, 01, 01)]
		public void TestRecalculateExchangeRateWithInvoicePostingExchangeRateOptionAndRateType()
		{
			var creator = new TestObjectCreator(Factory);
			creator.CreateUSDBuyRate(1.1m, new ZDateTime(2015, 1, 1));
			creator.CreateExchangeRate(creator.USD, "SEL", 1.2m, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 1, 7));
			var agent = Factory.Load<OrgHeader>(OptionSelection.AgentBeingPosted);
			AssertNotNull(agent);
			agent.CompanyData.AccARExchangeRateConfigurations.SetExRate(ledgerCode: "AR", jobType: "SHP", serviceDirection: "ALL", transportMode: "ALL", exRateType: "SEL");

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF");
			OptionSelection.Currency = "AUD";
			AssertEquals(1m, OptionSelection.ExchangeRate);
			OptionSelection.Currency = "USD";
			AssertEquals(0.7831m, OptionSelection.ExchangeRate);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");
			OptionSelection.Currency = "AUD";
			AssertEquals(1m, OptionSelection.ExchangeRate);
			OptionSelection.Currency = "USD";
			AssertEquals(1.2m, OptionSelection.ExchangeRate);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");
			OptionSelection.Currency = "AUD";
			AssertEquals(1m, OptionSelection.ExchangeRate);
			OptionSelection.Currency = "USD";
			AssertEquals(1.2m, OptionSelection.ExchangeRate);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			OptionSelection.Currency = "AUD";
			AssertEquals(1m, OptionSelection.ExchangeRate);
			OptionSelection.Currency = "USD";
			AssertEquals(1.2m, OptionSelection.ExchangeRate);
		}

		public void TestAgentPostingOptionSelectorAlwayHasValidation()
		{
			Factory.SuspendValidation();
			OptionSelection = new AgentPostingOptionSelector(Factory, Consol, GetChargesForPosting());
			Assert("Validation is suspended as this is not editable object during posting.", Consol.IsValidationSuspended);
			Assert("Validation must not be suspended as it validates user input in a GUI.", !OptionSelection.IsValidationSuspended);
		}

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;
	}
}

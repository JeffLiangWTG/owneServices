using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting.AgentPostingCurrencySelection;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.AgentPostingOptionSelection
{
	public partial class AgentPostingOptionSelector : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AgentPostingOptionSelector(BusinessObjectFactory factory, IJobCostingPlugIn consol, IReceivablesPostingChargeCollection agentChargesBeingPosted)
			: base(factory)
		{
			if (factory.IsValidationSuspended)
			{
				IgnoreValidationSuspended = true;
			}
			this.Consol = consol;
			this.AgentChargesBeingPosted = agentChargesBeingPosted;
			currency = consol.ConsolCurrency != null ? consol.ConsolCurrency.RX_Code : ZString.Empty;
			exchangeRate = consol.ConsolExchangeRate;
			ResetExchangeRateCalculationMethod();
			address = DefaultAgentOrgAddressPK;
		}

		#region Address

		[List(nameof(AddressList))]
		public ZGuid Address
		{
			get
			{
				return address;
			}
			set
			{
				if (address != value)
				{
					SetNonPersistentPropertyValue(AddressInfo, ref address, value);
					if (!IsValidationSuspended)
					{
						ValidateAddress();
					}
				}
			}
		}

		ZGuid address;

		public ZPropertyInfo AddressInfo
		{
			get { return GetZPropertyInfo(nameof(Address)); }
		}

		public OrgAddressDependentCollection AddressList
		{
			get
			{
				if (fAddresses == null)
				{
					fAddresses = new OrgAddressDependentCollection(Factory);
					OrgHeader parent = Factory.Load<OrgHeader>(AgentBeingPosted);
					if (parent != null)
					{
						ZQuery filter = new ZQuery(OrgAddressSchema.OA_IsActive, SQLComparisonOperator.Equal, ZBool.True);
						fAddresses = new OrgAddressDependentCollection(parent, filter);
						fAddresses.Load();
					}
				}
				return fAddresses;
			}
		}

		OrgAddressDependentCollection fAddresses;

		public void ValidateAddress()
		{
			if (!IsValidationSuspended)
			{
				AddressInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(AddressInfo, Res.GetString("36474536-12ae-4b07-8024-9f89aa5d995f", "Address"));
				ListValidation.ErrorIfInvalidPK(AddressInfo, ResString.GetMultilingualString("940f4d58-5971-4521-9f5a-a4070e5c7788", "Select a valid address from the list"));
			}
		}

		ZGuid DefaultAgentOrgAddressPK
		{
			get
			{
				ZGuid defaultAddressPK = ZGuid.Empty;
				OrgHeader orgAgent = Factory.Load<OrgHeader>(AgentBeingPosted);
				if (orgAgent != null && orgAgent.AddressForSendingARDocuments != null)
				{
					defaultAddressPK = orgAgent.AddressForSendingARDocuments.PK;
				}
				return defaultAddressPK;
			}
		}

		#endregion

		#region Contact

		[List(nameof(ContactList))]
		public ZGuid Contact
		{
			get { return contact; }
			set
			{
				SetNonPersistentPropertyValue(ContactInfo, ref contact, value, false);
				if (!IsValidationSuspended)
				{
					ValidateContact();
				}
			}
		}

		ZGuid contact;

		public ZPropertyInfo ContactInfo
		{
			get { return GetZPropertyInfo(nameof(Contact)); }
		}

		public OrgContactDependentCollection ContactList
		{
			get
			{
				if (fContact == null)
				{
					fContact = new OrgContactDependentCollection(Factory);
					OrgHeader parent = Factory.Load<OrgHeader>(AgentBeingPosted);
					if (parent != null)
					{
						ZQuery filter = new ZQuery(OrgContactSchema.OC_IsActive, SQLComparisonOperator.Equal, ZBool.True);
						fContact = new OrgContactDependentCollection(parent, filter);
						fContact.Load();
					}
				}
				return fContact;
			}
		}

		OrgContactDependentCollection fContact;

		public void ValidateContact()
		{
			if (!IsValidationSuspended)
			{
				ContactInfo.ClearAllNotifications();
				ListValidation.ErrorIfInvalidPK(ContactInfo, ResString.GetMultilingualString("a268a6a3-048f-4da6-83b3-c735d3f5dcbf", "Select a valid contact from the list"));
			}
		}

		#endregion

		#region Currency

		public CodeDescriptionPairList CurrencySelectionList
		{
			get
			{
				if (fCurrencySelectionList == null)
				{
					fCurrencySelectionList = new CodeDescriptionPairList();
					PopulateCurrencySelectionListAndCollection(fCurrencySelectionList);
				}
				return fCurrencySelectionList;
			}
		}

		CodeDescriptionPairList fCurrencySelectionList;

		ZString currency;

		[MaxLength(3)]
		[List(nameof(CurrencySelectionList))]
		public ZString Currency
		{
			get { return currency; }
			set
			{
				if (currency != value)
				{
					SetNonPersistentPropertyValue(CurrencyInfo, ref currency, value);
					ResetExchangeRateCalculationMethod();
					if (!IsValidationSuspended)
					{
						ValidateCurrency();
						ValidateExchangeRateCalculationMethod();
					}
					RecalculateExchangeRate();
				}
			}
		}

		public ZPropertyInfo CurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(Currency)); }
		}

		public void ValidateCurrency()
		{
			if (!IsValidationSuspended)
			{
				CurrencyInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(CurrencyInfo, Res.GetString("fbb3d252-c980-4e87-90a9-5084133034b3", "Posting Currency"));
				ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("b01356c4-e926-4d2f-89f1-fb1e24dab3ad", "Select a posting currency from the list of saved currencies"), CurrencyInfo);
			}
		}

		#endregion

		#region Agent Being Posted

		[List(nameof(AgentBeingPostedList))]
		public ZGuid AgentBeingPosted
		{
			get { return AgentChargesBeingPosted.Key.Org; }
		}

		public OrgHeaderCollection AgentBeingPostedList
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public ZPropertyInfo AgentBeingPostedInfo
		{
			get { return GetZPropertyInfo(nameof(AgentBeingPosted)); }
		}

		#endregion

		public void UpdateAllChargesPostingStyleAccordingToSelectedCurrency()
		{
			ZString postingStyle = InvoiceTypesList.Codes.FinalInvoice;
			var shouldChangeSellCurrency = false;
			RefCurrency newCurrency = null;
			if (!IsLocalCurrency)
			{
				postingStyle = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
				shouldChangeSellCurrency = true;
				newCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Currency);
			}

			foreach (IReceivablesPostingCharge postingCharge in AgentChargesBeingPosted)
			{
				postingCharge.InvoiceType = postingStyle;
				var charge = (Charge)postingCharge;

				if (shouldChangeSellCurrency)
				{
					var invoiceCurrencyType = AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(Company, ExchangeRateValidLedgerEnum.AR, charge);
					var jobExRate = charge.InvoicingJob.AddCurrency(newCurrency, AgentBeingPosted, ExchangeRateValidLedgerEnum.AR, invoiceCurrencyType);
					if (jobExRate.JF_BaseRate != ExchangeRate)
					{
						jobExRate.JF_BaseRate = ExchangeRate;
					}

					if (charge.JR_RX_NKSellCurrency != Currency)
					{
						using (charge.InvoiceTypeUpdateSuspender.GetSuspender())
						{
							charge.JR_RX_NKSellCurrency = Currency;
						}
					}
				}
				// Clear Sell Invoice Currency as we do not use it for this type of posting 
				charge.JR_RX_NKSellInvoiceCurrency = ZString.Empty;
			}
		}

		public void UpdateAllChargesAddressAndContactAccordingToSelectedAddressAndContact()
		{
			foreach (IReceivablesPostingCharge charge in AgentChargesBeingPosted)
			{
				if (charge.DebtorAddressPK != Address)
				{
					charge.DebtorAddressPK = Address;
				}
				if (charge.DebtorContactPK != Contact)
				{
					charge.DebtorContactPK = Contact;
				}
			}
		}

		#region Exchange Rate

		[ReadOnlyMember(nameof(IsLocalCurrency))]
		public ZDecimal ExchangeRate
		{
			get { return exchangeRate; }
			set
			{
				SetNonPersistentPropertyValue(ExchangeRateInfo, ref exchangeRate, value);
				if (!IsValidationSuspended)
				{
					ValidateExchangeRate();
				}
			}
		}

		ZDecimal exchangeRate;

		public ZInt ExchangeRateDecimalPlaces
		{
			get { return Env.CurrentCompany.ExchangeRate.RateDecimals; }
		}

		public ZPropertyInfo ExchangeRateInfo
		{
			get { return GetZPropertyInfo(nameof(ExchangeRate)); }
		}

		public bool ExchangeRate_ReadOnly
		 => !IsLocalCurrency && ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AR, IsLocalCurrency, Company.PK);

		public void ValidateExchangeRate()
		{
			if (!IsValidationSuspended)
			{
				ExchangeRateInfo.ClearAllNotifications();
				if (IsLocalCurrency || !ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AR, IsLocalCurrency, Company.PK))
				{
					MandatoryValidation.CheckEntered(ExchangeRateInfo, Res.GetString("b6671cfa-8da6-48a9-aaf1-2a4ee5ec0f30", "Exchange Rate"));
				}
				else
				{
					var regItem = AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateRegistryItem(ExchangeRateValidLedgerEnum.AR);
					var invoiceCurrencyType = IsLocalCurrency ? Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local : Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign;
					var exchangeRateOptionCode = regItem.Value.Cast<InvoicePostingExRateOption>().FirstOrDefault(x => x.InvoiceCurrencyType == invoiceCurrencyType).ExRateOption;
					var exchangeRateOption = AccountingConstants.InvoicePostingExchangeRateOption.CodeList.GetDescriptionFromCode(exchangeRateOptionCode);

					ExchangeRateInfo.AddWarning(Res.GetString("51cb4e64-e027-4898-8d06-16d645a27790",
						@"The ""{0}"" has been set to ""{1}"".
You are not allowed to change the exchange rate manually.", AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateRegistryItemCaption(ExchangeRateValidLedgerEnum.AR), exchangeRateOption));
				}
				ValidateExchangeRateForLocalCurrency();
				if (!ExchangeRateInfo.HasErrors() && ExchangeRate <= decimal.Zero)
				{
					ExchangeRateInfo.AddError(Res.GetString("ca3b1fb8-8f1b-4956-b810-fba2b8a7e121", "Exchange rate must be greater than 0."));
				}
				if (!ExchangeRateInfo.HasErrors())
				{
					TypeValidation.CheckValidDecimal(ExchangeRateInfo, 18, 9);
				}
			}
		}

		void ValidateExchangeRateForLocalCurrency()
		{
			if (IsLocalCurrency && ExchangeRate != 1m)
			{
				ExchangeRateInfo.AddError(Res.GetString("12e2cd23-424c-4327-baca-c2a20171aded", "Exchange rate must be 1 as the selected currency is local"));
			}
		}

		#endregion

		#region Exchange Rate Calculation Method

		ZString exchangeRateCalculationMethod;

		[MaxLength(3)]
		[List(nameof(ExchangeRateCalculationMethodList))]
		[ReadOnlyMember(nameof(IsLocalCurrency))]
		public ZString ExchangeRateCalculationMethod
		{
			get { return exchangeRateCalculationMethod; }
			set
			{
				CheckMaximumLength(ExchangeRateCalculationMethodInfo, value);
				SetNonPersistentPropertyValue(ExchangeRateCalculationMethodInfo, ref exchangeRateCalculationMethod, value);
				RecalculateExchangeRate();

				if (!IsValidationSuspended)
				{
					ValidateExchangeRateCalculationMethod();
				}
			}
		}

		public ZPropertyInfo ExchangeRateCalculationMethodInfo
		{
			get { return GetZPropertyInfo(nameof(ExchangeRateCalculationMethod)); }
		}

		CodeDescriptionPairList fExchangeRateCalculationMethodList;
		public CodeDescriptionPairList ExchangeRateCalculationMethodList
		{
			get
			{
				if (fExchangeRateCalculationMethodList == null)
				{
					fExchangeRateCalculationMethodList = new CodeDescriptionPairList();
					PopulateExchangeRateCalculationMethodList(fExchangeRateCalculationMethodList);
				}
				return fExchangeRateCalculationMethodList;
			}
		}

		void ResetExchangeRateCalculationMethod()
		{
			if (IsLocalCurrency)
			{
				ExchangeRateCalculationMethod = string.Empty;
			}
			else if (ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AR, IsLocalCurrency, Company.PK))
			{
				var exchangeRateOption = AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateOption(ExchangeRateValidLedgerEnum.AR, IsLocalCurrency, Company.PK);

				if (exchangeRateOption == AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code)
				{
					ExchangeRateCalculationMethod = AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code;
				}
				else if (exchangeRateOption == AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code)
				{
					ExchangeRateCalculationMethod = AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code;
				}
				else if (exchangeRateOption == AccountingConstants.InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code)
				{
					ExchangeRateCalculationMethod = AccountingConstants.InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code;
				}
				else if (exchangeRateOption == ExchangeRateCalculationMethods.Codes.TodaysRate)
				{
					ExchangeRateCalculationMethod = ExchangeRateCalculationMethods.Codes.TodaysRate;
				}
			}
			else
			{
				ExchangeRateCalculationMethod = ExchangeRateCalculationMethods.Codes.MasterFreight;
			}
		}

		void RecalculateExchangeRate()
		{
			if (IsLocalCurrency)
			{
				ExchangeRate = 1m;
				return;
			}

			if (!IsLocalCurrency && ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AR, IsLocalCurrency, Company.PK))
			{
				var firstJob = AgentChargesBeingPosted.FirstOrDefault().Job as Job;
				var agent = Factory.Load<OrgHeader>(AgentBeingPosted);
				var invoiceDate = AgentChargesBeingPosted.Count > 0 ? AgentChargesBeingPosted[0].ARInvoiceDate : ZDateTime.Now;
				var taxDate = AgentChargesBeingPosted.FirstOrDefault()?.ARInvoiceTaxDate ?? ZDateTime.Now;
				var rateCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Currency);
				ExchangeRate = AccExchangeRateConfigurationRateFinder.GetExchangeRateBasedOnInvoicePostingOption(firstJob.ExchangeRateConfigurationRateConsumer, rateCurrency,
					IsLocalCurrency, firstJob.JH_GC, agent, ExchangeRateEnumsExtensions.GetLedgerFromCode(LedgerTypes.AccountsReceivable), invoiceDate, ZDateTime.Now, taxDate, true);
				return;
			}

			switch (ExchangeRateCalculationMethod)
			{
				case ExchangeRateCalculationMethods.Codes.TodaysRate:

					if (AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.Value)
					{
						ExchangeRate = Env.CurrentCompany.ExchangeRate.TodaysRateIncludingExpired(Currency, ExchangeRateType.Sell);
						if (ExchangeRate == 0)
						{
							ExchangeRate = Env.CurrentCompany.ExchangeRate.TodaysRateIncludingExpired(Currency, ExchangeRateType.Buy);
						}
					}
					else
					{
						ExchangeRate = Env.CurrentCompany.ExchangeRate.TodaysRate(Currency, ExchangeRateType.Sell);
						if (ExchangeRate == 0)
						{
							ExchangeRate = Env.CurrentCompany.ExchangeRate.TodaysRate(Currency, ExchangeRateType.Buy);
						}
					}
					break;
				case ExchangeRateCalculationMethods.Codes.MasterFreight:
					ExchangeRate = Consol.ExchangeRateForCurrency(Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Currency), ZGuid.Empty);
					break;
				case ExchangeRateCalculationMethods.Codes.WeightedAvg:
					ExchangeRate = GetWeightedAverageExRate();
					break;
				default:
						break;
			}
		}

		ZDecimal GetWeightedAverageExRate()
		{
			ZDecimal foreignTotal = 0m;
			ZDecimal roundedLocalTotal = 0m;
			foreach (IReceivablesPostingCharge charge in AgentChargesBeingPosted)
			{
				if (charge.SellCurrency.RX_Code == Currency)
				{
					foreignTotal += charge.OSSellAmount;
					roundedLocalTotal += Env.CurrentCompany.ExchangeRate.ForeignToLocal(charge.OSSellAmount, charge.SellExchangeRate);
				}
			}
			ZDecimal roundedExRate = 0m;
			if (roundedLocalTotal != 0)
			{
				roundedExRate = Company.GC_IsReciprocal ? (roundedLocalTotal / foreignTotal) : (foreignTotal / roundedLocalTotal);
			}
			return Utilities.Round(roundedExRate, ExchangeRateDecimalPlaces);
		}

		void ValidateExchangeRateCalculationMethod()
		{
			if (!IsValidationSuspended)
			{
				ExchangeRateCalculationMethodInfo.ClearAllNotifications();

				var exchangeRateOption = AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateOption(ExchangeRateValidLedgerEnum.AR, IsLocalCurrency, Company.PK);

				if (IsLocalCurrency)
				{
					MandatoryValidation.CheckNotEntered(ExchangeRateCalculationMethodInfo);
				}
				else
				{
					MandatoryValidation.CheckEntered(ExchangeRateCalculationMethodInfo);
					ListValidation.ErrorIfInvalidCode(ExchangeRateCalculationMethodInfo);

					if (!ExchangeRateCalculationMethodInfo.HasErrors()
						&& exchangeRateOption != AccountingConstants.InvoicePostingExchangeRateOption.Default.Code
						&& exchangeRateOption != ExchangeRateCalculationMethod
						&& ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AR, IsLocalCurrency, Company.PK))
					{
						ExchangeRateCalculationMethodInfo.AddError(Res.GetString("a33154ae-d7d5-464e-a6fc-ade77b407df6",
						@"The “{0}” has been set to ""{1}"".
You are not allowed to override this behavior.
This must value must be set to ""{1}"".", AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateRegistryItemCaption(ExchangeRateValidLedgerEnum.AR), exchangeRateOption));
					}
				}
			}
		}

		#endregion

		bool IsLocalCurrency => Currency == Company.GC_RX_NKLocalCurrency;

		GlbCompany Company => (AgentChargesBeingPosted?.FirstOrDefault()?.Job as Job)?.Company ?? GlbCompany.CurrentCompany;

		protected override void RunPreSaveValidationCore()
		{
			ValidateAddress();
			ValidateContact();
			ValidateCurrency();
			ValidateExchangeRate();
			ValidateExchangeRateCalculationMethod();
			ValidateExchangeRateForLocalCurrency();

			base.RunPreSaveValidationCore();
		}

		#region Implementation

		readonly IReceivablesPostingChargeCollection AgentChargesBeingPosted;
		readonly IJobCostingPlugIn Consol;

		void PopulateCurrencySelectionListAndCollection(CodeDescriptionPairList currencyCodes)
		{
			RefCurrency consolCurrency = Consol.ConsolCurrency;
			if (consolCurrency != null)
			{
				currencyCodes.AddPair(consolCurrency.RX_Code, consolCurrency.RX_Code, Res.GetString("d1bcfb25-4c66-4a2d-8c2a-1bdf0792d10d", "Master Freight Collect Currency"));
			}
			foreach (IReceivablesPostingCharge charge in AgentChargesBeingPosted)
			{
				// This type of posting does not support Sell Invoice Currency. We need to go around the IReceivablesPostingCharge implementation.
				// Could be changed to do it with a Context or to support Sell Invoice Currency for this type of posting
				var jobCharge = charge as JobCharge;
				var chargeCurrency = jobCharge != null && jobCharge.BillInInvoiceCurrency ? jobCharge.SellCurrency : charge.SellCurrency;

				if (!currencyCodes.ContainsCode(chargeCurrency.RX_Code))
				{
					currencyCodes.AddPair(chargeCurrency.RX_Code, chargeCurrency.RX_Code.ToString(), chargeCurrency.RX_DescMultilingual);
				}
			}
		}

		void PopulateExchangeRateCalculationMethodList(CodeDescriptionPairList list)
		{
			list.AddPair(ExchangeRateCalculationMethods.Codes.MasterFreight, ExchangeRateCalculationMethods.Descriptions.MasterFreight);
			list.AddPair(ExchangeRateCalculationMethods.Codes.TodaysRate, ExchangeRateCalculationMethods.Descriptions.TodaysRate);
			list.AddPair(ExchangeRateCalculationMethods.Codes.WeightedAvg, ExchangeRateCalculationMethods.Descriptions.WeightedAvg);

			var localExchangeRateOption = AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateOption(ExchangeRateValidLedgerEnum.AR, true, Company.PK);
			var foreignExchangeRateOption = AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateOption(ExchangeRateValidLedgerEnum.AR, false, Company.PK);

			if (localExchangeRateOption == AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code
				|| foreignExchangeRateOption == AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code)
			{
				list.AddPair(AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code,
					AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.MultilingualDescription);
			}
			if (localExchangeRateOption == AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code
				|| foreignExchangeRateOption == AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code)
			{
				list.AddPair(AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code,
					AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.MultilingualDescription);
			}
			if (localExchangeRateOption == AccountingConstants.InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code
				|| foreignExchangeRateOption == AccountingConstants.InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code)
			{
				list.AddPair(AccountingConstants.InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code,
					AccountingConstants.InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.MultilingualDescription);
			}
		}

		#endregion
	}
}

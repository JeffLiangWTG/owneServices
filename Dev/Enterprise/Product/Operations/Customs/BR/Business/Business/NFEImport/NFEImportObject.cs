using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class NFEImportObject : NonPersistentBusinessObject
	{
		public NFEImportObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static class Schema
		{
			public const string NfeKey = "NfeKey";
			public const string NfeNumber = "NfeNumber";
			public const string NfeSerie = "NfeSerie";
			public const string NfeGrossWeight = "NfeGrossWeight";
			public const string NfeNetWeight = "NfeNetWeight";
			public const string NfeInvoiceAmount = "NfeInvoiceAmount";
			public const string NfeDate = "NfeDate";
			public const string CurrencyCode = "CurrencyCode";
			public const string NfeTotalFreight = "NfeTotalFreight";
			public const string NfeTotalInsurance = "NfeTotalInsurance";
			public const string NfeTotalOtherCharges = "NfeTotalOtherCharges";
			public const string NfeTotalDiscount = "NfeTotalDiscount";
			public const string ExchangeRate = "ExchangeRate";
			public const string Incoterm = "Incoterm";
			public const string InvoiceHeaderPK = "InvoiceHeaderPK";
			public const string ExchangeRateDate = "ExchangeRateDate";
			public const string ExchangeRateSell = "ExchangeRateSell";
			public const string ExchangeRateBuy = "ExchangeRateBuy";
			public const string SupplierID = "SupplierID";
			public const string EntryInstructionPK = "EntryInstructionPK";
		}

		public JobDeclaration Declaration
		{
			get => fDeclaration;
			set
			{
				if (fDeclaration != value)
				{
					fDeclaration = value;

					var invoice = fDeclaration.Invoices.FirstOrDefault();

					ExchangeRate = ZDecimal.Zero;
					CurrencyCode = invoice?.JZ_RX_NKInvoice_Currency ?? ZString.Empty;
					Incoterm = invoice?.JZ_IncoTerm ?? ZString.Empty;

					if (fDeclaration.IsPersistent)
					{
						var entryInstructions = Lookups.EntryInstructionList;
						EntryInstructionPK = entryInstructions.Count == 1 ? entryInstructions.First().PK : ZGuid.Empty;
					}
				}
			}
		}
		JobDeclaration fDeclaration;

		public NFEImportObjectParent ObjectParent { get; set; }

		public JobComInvoiceHeader InvoiceHeader => Declaration?.Invoices.FindByPK(InvoiceHeaderPK) as JobComInvoiceHeader;

		public CusEntryInstruction EntryInstruction => Declaration?.CustomsEntryInstructions.FindByPK(EntryInstructionPK) as CusEntryInstruction;

		#region Lookups

		public NFEImportObjectLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = new NFEImportObjectLookups(this);
				}
				return fLookups;
			}
		}

		NFEImportObjectLookups fLookups;

		#endregion

		#region Validation

		public NFEImportObjectValidation Validation
		{
			get { return new NFEImportObjectValidation(this); }
		}

		protected NFEImportObjectValidation GetNewValidation()
		{
			return new NFEImportObjectValidation(this);
		}

		#endregion

		#region NfeKey

		[ReadOnly(true)]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFEImportObject|NfeKey", Caption = "NF-e Key")]
		public ZString NfeKey
		{
			get { return fNfeKey; }
			set { SetNonPersistentPropertyValue(NfeKeyInfo, ref fNfeKey, value); }
		}

		ZString fNfeKey;

		public ZPropertyInfo NfeKeyInfo => GetZPropertyInfo(Schema.NfeKey);

		#endregion

		#region NfeNumber

		[ReadOnly(true)]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFEImportObject|NfeNumber", Caption = "NF-e Number")]
		public ZString NfeNumber
		{
			get { return fNfeNumber; }
			set { SetNonPersistentPropertyValue(NfeNumberInfo, ref fNfeNumber, value); }
		}

		ZString fNfeNumber;

		public ZPropertyInfo NfeNumberInfo => GetZPropertyInfo(Schema.NfeNumber);

		#endregion

		#region NfeSerie

		[ReadOnly(true)]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFEImportObject|NfeSerie", Caption = "NF-e Series")]
		public ZString NfeSerie
		{
			get { return fNfeSerie; }
			set { SetNonPersistentPropertyValue(NfeSerieInfo, ref fNfeSerie, value); }
		}

		ZString fNfeSerie;

		public ZPropertyInfo NfeSerieInfo => GetZPropertyInfo(Schema.NfeSerie);

		#endregion

		#region NfeGrossWeight

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFEImportObject|NfeGrossWeight", Caption = "Gross Weight")]
		public ZDecimal NfeGrossWeight
		{
			get { return fNfeGrossWeight; }
			set { SetNonPersistentPropertyValue(NfeGrossWeightInfo, ref fNfeGrossWeight, value); }
		}

		ZDecimal fNfeGrossWeight;

		public ZPropertyInfo NfeGrossWeightInfo => GetZPropertyInfo(Schema.NfeGrossWeight);

		#endregion

		#region NfeNetWeight

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFEImportObject|NfeNetWeight", Caption = "Net Weight")]
		public ZDecimal NfeNetWeight
		{
			get { return fNfeNetWeight; }
			set { SetNonPersistentPropertyValue(NfeNetWeightInfo, ref fNfeNetWeight, value); }
		}

		ZDecimal fNfeNetWeight;

		public ZPropertyInfo NfeNetWeightInfo => GetZPropertyInfo(Schema.NfeNetWeight);

		#endregion

		#region NfeInvoiceAmount

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFEImportObject|NfeInvoiceAmount", Caption = "Invoice Amount")]
		public ZDecimal NfeInvoiceAmount
		{
			get { return fNfeInvoiceAmount; }
			set { SetNonPersistentPropertyValue(NfeInvoiceAmountInfo, ref fNfeInvoiceAmount, value); }
		}

		ZDecimal fNfeInvoiceAmount;

		public ZPropertyInfo NfeInvoiceAmountInfo => GetZPropertyInfo(Schema.NfeInvoiceAmount);

		#endregion

		#region NfeDate

		[ReadOnly(true)]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFEImportObject|NfeDate", Caption = "NF-e Date")]
		public ZDateTime NfeDate
		{
			get { return fNfeDate; }
			set { SetNonPersistentPropertyValue(NfeDateInfo, ref fNfeDate, value); }
		}

		ZDateTime fNfeDate;

		public ZPropertyInfo NfeDateInfo => GetZPropertyInfo(Schema.NfeDate);

		#endregion

		#region NFEImportObjectItemCollection

		[ChildEditable(true)]
		public NFEImportObjectItemCollection Items
		{
			get
			{
				if (fItems == null)
				{
					fItems = new NFEImportObjectItemCollection(this);
				}
				return fItems;
			}
		}

		NFEImportObjectItemCollection fItems;

		#endregion

		#region CurrencyCode

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(NFEImportObjectLookups.CurrencyList))]
		[ReadOnlyMember(nameof(IncotermCurrency_Readonly))]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFEImport.NFEImportObject|CurrencyCode", Caption = "Currency")]
		public ZString CurrencyCode
		{
			get { return fCurrencyCode; }
			set
			{
				var oldValue = CurrencyCode;
				SetNonPersistentPropertyValue(CurrencyCodeInfo, ref fCurrencyCode, value);

				if (!IsCopying && oldValue != CurrencyCode)
				{
					DefaultExchangeRate();
				}

				if (!IsValidationSuspended)
				{
					ValidateCurrencyCodeOnAllRows();
				}
			}
		}

		ZString fCurrencyCode;

		public ZPropertyInfo CurrencyCodeInfo => GetZPropertyInfo(Schema.CurrencyCode);

		public RefCurrency Currency => RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCode);

		void ValidateCurrencyCodeOnAllRows()
		{
			if (ObjectParent != null)
			{
				ObjectParent.NFEImportObjectCollection.Cast<NFEImportObject>().ToList().ForEach(x => x.Validation.ValidateCurrencyCode());
			}
			else
			{
				Validation.ValidateCurrencyCode();
			}
		}

		#endregion

		#region ExchangeRate

		[DecimalPlaces(9)]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFEImport.NFEImportObject|ExchangeRate", Caption = "Exchange Rate")]
		public ZDecimal ExchangeRate
		{
			get { return fExchangeRate; }
			set
			{
				SetNonPersistentPropertyValue(ExchangeRateInfo, ref fExchangeRate, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateExchangeRate();
				}
			}
		}

		ZDecimal fExchangeRate;

		public ZPropertyInfo ExchangeRateInfo => GetZPropertyInfo(Schema.ExchangeRate);

		#endregion

		#region NfeTotalFreight

		public ZDecimal NfeTotalFreight
		{
			get { return fNfeTotalFreight; }
			set { SetNonPersistentPropertyValue(NfeTotalFreightInfo, ref fNfeTotalFreight, value); }
		}

		ZDecimal fNfeTotalFreight;

		public ZPropertyInfo NfeTotalFreightInfo => GetZPropertyInfo(Schema.NfeTotalFreight);

		#endregion

		#region NfeTotalInsurance

		public ZDecimal NfeTotalInsurance
		{
			get { return fNfeTotalInsurance; }
			set { SetNonPersistentPropertyValue(NfeTotalInsuranceInfo, ref fNfeTotalInsurance, value); }
		}

		ZDecimal fNfeTotalInsurance;

		public ZPropertyInfo NfeTotalInsuranceInfo => GetZPropertyInfo(Schema.NfeTotalInsurance);

		#endregion

		#region NfeTotalOtherCharges

		public ZDecimal NfeTotalOtherCharges
		{
			get { return fNfeTotalOtherCharges; }
			set { SetNonPersistentPropertyValue(NfeTotalOtherChargesInfo, ref fNfeTotalOtherCharges, value); }
		}

		ZDecimal fNfeTotalOtherCharges;

		public ZPropertyInfo NfeTotalOtherChargesInfo => GetZPropertyInfo(Schema.NfeTotalOtherCharges);

		#endregion

		#region NfeTotalDiscount

		public ZDecimal NfeTotalDiscount
		{
			get { return fNfeTotalDiscount; }
			set { SetNonPersistentPropertyValue(NfeTotalDiscountInfo, ref fNfeTotalDiscount, value); }
		}

		ZDecimal fNfeTotalDiscount;

		public ZPropertyInfo NfeTotalDiscountInfo => GetZPropertyInfo(Schema.NfeTotalDiscount);

		#endregion

		#region Incoterm

		[List(nameof(Lookups) + "." + nameof(NFEImportObjectLookups.IncoTermList))]
		[MaxLength(3)]
		[ReadOnlyMember(nameof(IncotermCurrency_Readonly))]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFEImport.NFEImportObject|Incoterm", Caption = "Incoterm")]
		public ZString Incoterm
		{
			get { return fIncoterm; }
			set
			{
				SetNonPersistentPropertyValue(IncotermInfo, ref fIncoterm, value);

				if (!IsValidationSuspended)
				{
					ValidateIncotermOnAllRows();
				}
			}
		}

		ZString fIncoterm;
		public ZPropertyInfo IncotermInfo => GetZPropertyInfo(Schema.Incoterm);

		public bool IncotermCurrency_Readonly => !InvoiceHeaderPK.IsEmpty;

		void ValidateIncotermOnAllRows()
		{
			if (ObjectParent != null)
			{
				ObjectParent.NFEImportObjectCollection.Cast<NFEImportObject>().ToList().ForEach(x => x.Validation.ValidateIncoterm());
			}
			else
			{
				Validation.ValidateIncoterm();
			}
		}

		#endregion

		#region InvoiceHeaderPK

		[List(nameof(Lookups) + "." + nameof(NFEImportObjectLookups.InvoiceHeaderList))]
		[ReadOnlyMember(nameof(InvoiceHeaderPK_Readonly))]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFEImport.NFEImportObject|InvoiceHeaderPK", Caption = "Invoice No.")]
		[RelatedBusinessObject(nameof(InvoiceHeader))]
		public ZGuid InvoiceHeaderPK
		{
			get { return fInvoiceHeaderPK; }
			set
			{
				var oldValue = InvoiceHeaderPK;
				SetNonPersistentPropertyValue(InvoiceHeaderPKInfo, ref fInvoiceHeaderPK, value);

				if (!IsCopying && oldValue != InvoiceHeaderPK)
				{
					if (InvoiceHeader is JobComInvoiceHeader invoice)
					{
						Incoterm = invoice.IncoTerm;
						CurrencyCode = invoice.JZ_RX_NKInvoice_Currency;
					}
					else if (oldValue.IsValid && InvoiceHeaderPK.IsEmpty)
					{
						Incoterm = ZString.Empty;
						CurrencyCode = ZString.Empty;
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateInvoiceHeaderPK();
					Validation.ValidateCurrencyCode();
					Validation.ValidateIncoterm();
				}
			}
		}
		ZGuid fInvoiceHeaderPK;

		public ZPropertyInfo InvoiceHeaderPKInfo => GetZPropertyInfo(Schema.InvoiceHeaderPK);

		public bool InvoiceHeaderPK_Readonly => !Declaration?.IsPersistent ?? true;

		#endregion

		#region ExchangeRateDate

		[ReadOnly(true)]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFEImportObject|ExchangeRateDate", Caption = "Exchange Rate Date")]
		public ZDateTime ExchangeRateDate
		{
			get { return fExchangeRateDate; }
			set { SetNonPersistentPropertyValue(ExchangeRateDateInfo, ref fExchangeRateDate, value); }
		}
		ZDateTime fExchangeRateDate;

		public ZPropertyInfo ExchangeRateDateInfo => GetZPropertyInfo(Schema.ExchangeRateDate);

		#endregion

		#region ExchangeRateSell

		[ReadOnly(true)]
		[DecimalPlaces(9)]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFEImport.NFEImportObject|ExchangeRateSell", Caption = "Exchange Rate Sell")]
		public ZDecimal ExchangeRateSell
		{
			get { return fExchangeRateSell; }
			set { SetNonPersistentPropertyValue(ExchangeRateSellInfo, ref fExchangeRateSell, value); }
		}
		ZDecimal fExchangeRateSell;

		public ZPropertyInfo ExchangeRateSellInfo => GetZPropertyInfo(Schema.ExchangeRateSell);

		#endregion

		#region ExchangeRateBuy

		[ReadOnly(true)]
		[DecimalPlaces(9)]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFEImport.NFEImportObject|ExchangeRateBuy", Caption = "Exchange Rate Buy")]
		public ZDecimal ExchangeRateBuy
		{
			get { return fExchangeRateBuy; }
			set { SetNonPersistentPropertyValue(ExchangeRateBuyInfo, ref fExchangeRateBuy, value); }
		}
		ZDecimal fExchangeRateBuy;

		public ZPropertyInfo ExchangeRateBuyInfo => GetZPropertyInfo(Schema.ExchangeRateBuy);

		#endregion

		#region SupplierID

		[ReadOnly(true)]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFEImportObject|SupplierID", Caption = "Supplier ID")]
		public ZString SupplierID
		{
			get { return fSupplierID; }
			set { SetNonPersistentPropertyValue(SupplierIDInfo, ref fSupplierID, value); }
		}

		ZString fSupplierID;

		public ZPropertyInfo SupplierIDInfo => GetZPropertyInfo(Schema.SupplierID);

		#endregion

		#region EntryInstructionPK

		[List(nameof(Lookups) + "." + nameof(NFEImportObjectLookups.EntryInstructionList))]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFEImport.NFEImportObject|EntryInstructionPK", Caption = "Entry Instruction")]
		[RelatedBusinessObject(nameof(EntryInstruction))]
		public ZGuid EntryInstructionPK
		{
			get { return fEntryInstructionPK; }
			set
			{
				SetNonPersistentPropertyValue(EntryInstructionPKInfo, ref fEntryInstructionPK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateEntryInstructionPK();
				}
			}
		}
		ZGuid fEntryInstructionPK;

		public ZPropertyInfo EntryInstructionPKInfo => GetZPropertyInfo(Schema.EntryInstructionPK);

		#endregion

		void DefaultExchangeRate()
		{
			if (NfeDate.IsValid)
			{
				var previousDate = NfeDate.GetNearestWorkingdayBefore();
				var foundRateDateBuy = ZDateTime.Empty;
				var foundRateDateSell = ZDateTime.Empty;
				var exchangeRateBuy = Currency?.GetRateForDate(ZArchitecture.Core.ExchangeRateType.CustomsSecondary, previousDate, 10, out foundRateDateBuy) ?? 0;
				var exchangeRateSell = Currency?.GetRateForDate(ZArchitecture.Core.ExchangeRateType.Customs, previousDate, 10, out foundRateDateSell) ?? 0;

				ExchangeRateDate = foundRateDateBuy.IsValid ? foundRateDateBuy : foundRateDateSell;
				ExchangeRate = exchangeRateBuy;
				ExchangeRateBuy = exchangeRateBuy;
				ExchangeRateSell = exchangeRateSell;
			}
		}
	}
}

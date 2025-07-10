using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class APInvoiceForBulkPoster : APInvoice
	{
		public APInvoiceForBulkPoster(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region AH_OSExTaxAmount
		public override ZDecimal AH_OSExTaxAmount
		{
			get { return base.AH_OSExTaxAmount; }
			set
			{
				bool hasChanged = AH_OSExTaxAmount != value;
				base.AH_OSExTaxAmount = value;
				if (hasChanged)
				{
					UpdateAmountsInBulkPoster(false);
				}
			}
		}

		protected override bool AH_OSExTaxAmount_ReadOnly => false;
		#endregion

		#region AH_LocalExTaxAmount
		public override ZDecimal AH_LocalExTaxAmount
		{
			get { return base.AH_LocalExTaxAmount; }
			set
			{
				bool hasChanged = AH_LocalExTaxAmount != value;
				base.AH_LocalExTaxAmount = value;
				if (hasChanged)
				{
					UpdateAmountsInBulkPoster(false);
				}
			}
		}
		#endregion

		#region AH_LocalTaxAmount
		public override ZDecimal AH_LocalTaxAmount
		{
			get { return base.AH_LocalTaxAmount; }
			set
			{
				bool hasChanged = AH_LocalTaxAmount != value;
				base.AH_LocalTaxAmount = value;
				if (hasChanged)
				{
					UpdateAmountsInBulkPoster(true);
				}
			}
		}
		#endregion

		#region AH_OSTaxAmount
		public override ZDecimal AH_OSTaxAmount
		{
			get
			{
				if (base.AH_OSTaxAmount != 0m && !IsInDatabase && AH_OSTaxAmount_ReadOnly)
				{
					base.AH_OSTaxAmount = 0m;
				}

				return base.AH_OSTaxAmount;
			}
			set
			{
				bool hasChanged = AH_OSTaxAmount != value;
				base.AH_OSTaxAmount = value;
				if (hasChanged)
				{
					UpdateAmountsInBulkPoster(true);
				}
			}
		}

		protected override bool AH_OSTaxAmount_ReadOnly
		{
			get
			{
				OrgHeader creditor = Factory.Load<OrgHeader>(AH_OH);
				return !GlbCompany.CurrentCompany.GC_IsGSTRegistered ||
						creditor == null || !creditor.CompanyData.IsAPTaxApplicable ||
						AccTaxRate == null || AccTaxRate.GetRate(TaxDate) == 0;
			}
		}

		#endregion

		#region UpdateAmountsInBulkPoster
		void UpdateAmountsInBulkPoster(bool tax)
		{
			foreach (BusinessObjectCollection collection in ((IBusinessObjectInternals)this).ParentCollections)
			{
				if (collection is APInvoiceForBulkPosterCollection && ((APInvoiceForBulkPosterCollection)collection).MasterAPBulkInvoicePoster != null)
				{
					if (tax)
					{
						((APInvoiceForBulkPosterCollection)collection).MasterAPBulkInvoicePoster.UpdateTotalOSTaxAmount();
					}
					else
					{
						((APInvoiceForBulkPosterCollection)collection).MasterAPBulkInvoicePoster.UpdateTotalOSExTaxAmount();
					}
				}
			}
		}
		#endregion

		#region AH_RX_NKTransactionCurrency
		public override ZString AH_RX_NKTransactionCurrency
		{
			get { return base.AH_RX_NKTransactionCurrency; }
			set
			{
				bool hasChanged = AH_RX_NKTransactionCurrency != value;
				base.AH_RX_NKTransactionCurrency = value;
				if (hasChanged)
				{
					AH_OSTaxAmount = 0m;
				}
			}
		}
		#endregion

		#region AH_ExchangeRate

		public override ZDecimal AH_ExchangeRate
		{
			get
			{ return base.AH_ExchangeRate; }
			set
			{
				bool hasChanged = AH_ExchangeRate != value;
				base.AH_ExchangeRate = value;
				if (hasChanged)
				{
					AH_LocalExTaxAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(AH_OSExTaxAmount, AH_ExchangeRate);
					AH_LocalTaxAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(AH_OSTaxAmount, AH_ExchangeRate);
				}
			}
		}
		#endregion

		#region TaxRate
		[ReadOnlyMember(nameof(IsTaxReadOnly))]
		[List("TaxRates")]
		public ZGuid TaxRate
		{
			get
			{
				if (TaxRateInfo.ReadOnly && fTaxRate != ZGuid.Empty)
				{
					fTaxRate = ZGuid.Empty;
				}
				return fTaxRate;
			}
			set
			{
				fTaxRate = value;
				if (fTaxRate.IsValid)
				{
					TaxDate = ZDate.Today;
				}
				TaxRateInfo.RefreshBinding();
			}
		}

		ZGuid fTaxRate;

		public ZPropertyInfo TaxRateInfo
		{
			get { return GetZPropertyInfo(nameof(TaxRate)); }
		}

		internal AccTaxRate AccTaxRate
		{
			get { return Factory.Load<AccTaxRate>(TaxRate); }
		}

		#endregion

		#region TaxDate

		[ReadOnlyMember(nameof(IsTaxReadOnly))]
		[ResourceStringData("TaxDate", Caption = "Tax Date")]
		public ZDate TaxDate
		{
			get
			{
				if (TaxDateInfo.ReadOnly && taxDate != ZDate.Empty)
				{
					taxDate = ZDate.Empty;
				}
				return taxDate;
			}
			set
			{
				taxDate = value;

				if (!IsValidationSuspended)
				{
					(Validation as APInvoiceForBulkPosterValidation)?.ValidateTaxDate();
				}

				TaxDateInfo.RefreshBinding();
			}
		}

		ZDate taxDate;

		public ZPropertyInfo TaxDateInfo => GetZPropertyInfo(nameof(TaxDate));

		#endregion

		protected override ZBool DefaultIsCashInvoiceCore
		{
			get { return ZBool.False; }
		}

		#region Validation
		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new APInvoiceForBulkPosterValidation(this);
		}
		#endregion
	}
}

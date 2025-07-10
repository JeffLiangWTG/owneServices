using System;
using System.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoicingBasePayLineMediator : NonPersistentBusinessObject, IObsoleteValidation
	{
		public InvoicingBasePayLineMediator(MatchingBase matching, InvoicingBase masterInvoice)
			: base(matching.Factory)
		{
			this.Matching = matching;
			this.MasterInvoice = masterInvoice;
			IsNew = true;
		}

		#region Public Properties

		ILineMatchingCollection fLines;
		public ILineMatchingCollection Lines
		{
			get
			{
				if (fLines == null)
				{
					fLines = new ILineMatchingCollection(MasterInvoice, Matching);
					foreach (ILineMatching line in MasterInvoice.Lines)
					{
						fLines.Add(line);
						line.PaidAmountInfo.ValueChanged += PaidAmountInfo_ValueChanged;
						line.SetDefaultValues();
					}
					RegisterEditableChildObject(fLines);
				}

				return fLines;
			}
		}

		public ZDecimal LineTotalPaidAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (ILineMatching line in Lines)
				{
					result += line.PaidAmount;
				}

				return result;
			}
		}

		public ZPropertyInfo LineTotalPaidAmountInfo
		{
			get { return GetZPropertyInfo(nameof(LineTotalPaidAmount)); }
		}

		public virtual ZDecimal LineTotalLocalPaidAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (ILineMatching line in Lines)
				{
					result += line.PaidAmount;
				}
				return Enterprise.Environment.Env.CurrentCompany.ExchangeRate.ForeignToLocal(result, MasterInvoice.AH_ExchangeRate);
			}
		}

		public ZPropertyInfo LineTotalLocalPaidAmountInfo
		{
			get { return GetZPropertyInfo(nameof(LineTotalLocalPaidAmount)); }
		}

		public bool IsNew { get; private set; }

		#endregion

		#region Public Methods

		public void ConveyData()
		{
			ISupportMatchingOfMyLines masterInvoiceAsISupportMatchingOfMyLines = MasterInvoice;
			masterInvoiceAsISupportMatchingOfMyLines.LineTotalPaidAmount = LineTotalPaidAmount;
			masterInvoiceAsISupportMatchingOfMyLines.LineTotalLocalPaidAmount = LineTotalLocalPaidAmount;
			masterInvoiceAsISupportMatchingOfMyLines.IsPaidAmountApportionedToLines = false;

			IsNew = false;
			MasterInvoice.RefreshBinding();
			MatchingValidation validation = MasterInvoice.Validation as MatchingValidation;
			if (validation != null)
			{
				validation.ValidateOSPartialPaymentAmount();
			}
			Matching.UpdateAndValidateBalance();

			foreach (ILineMatching line in Lines)
			{
				line.UpdateOriginalAmounts();
			}
		}

		public void CancelEdits()
		{
			foreach (ILineMatching line in Lines)
			{
				line.PaidAmount = line.OriginalPaidAmount;
				((InvoicingLineBase)line).Validation.ValidateAll();
			}
		}

		public void SetAllLinesMatchingFiltersToFullyPaid()
		{
			foreach (ILineMatching line in Lines)
			{
				line.IsFullyPay = IsMatchesAllFilters(line);
			}
		}

		public void SetAllLinesToFullyPaid(bool isFullyPay)
		{
			SetSelectedLinesToFullyPaid(Lines, isFullyPay);
		}

		public void SetSelectedLinesToFullyPaid(IEnumerable selectedLines, bool isFullyPay)
		{
			foreach (ILineMatching line in selectedLines)
			{
				line.IsFullyPay = isFullyPay;
			}
		}

		#endregion

		#region Filters

		bool IsMatchesAllFilters(ILineMatching line)
		{
			return IsMatchesChargeCodeFilter(line) && IsMatchesChargeGroupFilter(line) && IsMatchesChargeTypeFilter(line) && IsMatchesChargeCurrencyFilter(line);
		}

		bool IsMatchesChargeCodeFilter(ILineMatching line)
		{
			bool result = true;

			if (ChargeCodeFilter.IsValid)
			{
				result = line.ChargeCode != null && line.ChargeCode.PK == ChargeCodeFilter;
			}

			return result;
		}

		bool IsMatchesChargeGroupFilter(ILineMatching line)
		{
			bool result = true;

			if (!ChargeGroupFilter.IsEmpty)
			{
				result = line.ChargeCode != null && line.ChargeCode.AC_ChargeGroup == ChargeGroupFilter;
			}

			return result;
		}

		bool IsMatchesChargeTypeFilter(ILineMatching line)
		{
			bool result = true;

			if (!ChargeTypeFilter.IsEmpty)
			{
				result = line.ChargeType == ChargeTypeFilter;
			}

			return result;
		}

		bool IsMatchesChargeCurrencyFilter(ILineMatching line)
		{
			bool result = true;

			if (!ChargeCurrencyFilter.IsEmpty)
			{
				result = line.ChargeCurrency == ChargeCurrencyFilter;
			}

			return result;
		}

		[List("ChargeCodes")]
		public ZGuid ChargeCodeFilter { get; set; }

		[List("ChargeGroups")]
		public ZString ChargeGroupFilter { get; set; }

		[List("ChargeTypes")]
		public ZString ChargeTypeFilter { get; set; }

		[List("Currencies")]
		public ZString ChargeCurrencyFilter { get; set; }

		#region Lookups

		#region Charge Codes

		public AccChargeCodeCollection ChargeCodes
		{
			get { return FindboxLookupCollections.GetChargeCodeCollection(Factory); }
		}

		#endregion

		#region Charge Types

		CodeDescriptionPairList fChargeTypes;
		public CodeDescriptionPairList ChargeTypes
		{
			get { return fChargeTypes ?? (fChargeTypes = new CodeDescriptionPairList(OLookUpEditType.ChargeTypes)); }
		}

		#endregion

		#region Charge Groups

		CodeDescriptionPairList fChargeGroups;
		public CodeDescriptionPairList ChargeGroups
		{
			get { return fChargeGroups ?? (fChargeGroups = new ChargeCodeGroupList()); }
		}

		#endregion

		#region Currencies

		public RefCurrencyCollection Currencies
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		#endregion

		#endregion

		#endregion

		#region Implementation

		public readonly InvoicingBase MasterInvoice;
		readonly MatchingBase Matching;

		void PaidAmountInfo_ValueChanged(object sender, EventArgs e)
		{
			LineTotalPaidAmountInfo.RefreshBinding();
		}

		#endregion
	}
}

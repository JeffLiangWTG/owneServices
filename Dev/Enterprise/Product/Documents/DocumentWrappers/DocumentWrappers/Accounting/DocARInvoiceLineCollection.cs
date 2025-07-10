using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocARInvoiceLineCollection : DocumentWrapperCollection, ISortableDocLineList
	{
		#region Constructors && Type Overriding

		public DocARInvoiceLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static DocARInvoiceLineCollection New(BusinessObjectFactory factory)
		{
			DocARInvoiceLineCollection result = null;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(factory);
			}
			else
			{
				result = new DocARInvoiceLineCollection(factory);
			}
			return result;
		}

		protected delegate DocARInvoiceLineCollection NewDelegate(BusinessObjectFactory factory);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		public new IDocARInvoiceLine this[int index]
		{
			get { return (IDocARInvoiceLine)base[index]; }
		}

		#endregion

		#region Rollup Support

		internal bool OnlyOneCurrency
		{
			get
			{
				bool result = true;

				if (this.Count > 0)
				{
					var firstLineCurrency = ((DocARInvoiceLine)this[0]).CurrencyExRateAmount.Currency;
					ZString firstLineCurrencyCode = firstLineCurrency != null ? firstLineCurrency.Code : ZString.Empty;
					var linesWithAnotherCurrency =
						from DocARInvoiceLine line in this
						let lineCurrency = line.CurrencyExRateAmount.Currency
						where (lineCurrency != null ? lineCurrency.Code : ZString.Empty) != firstLineCurrencyCode
						select lineCurrency;

					result = !linesWithAnotherCurrency.Any();
				}
				return result;
			}
		}

		internal bool OnlyOneJob
		{
			get
			{
				bool result = false;
				if (this.Count > 0)
				{
					var firstLineJobHeader = this[0].JobHeader;
					if (firstLineJobHeader != null)
					{
						var linesWithAnotherJob =
							from DocARInvoiceLine line in this
							let lineJob = line.JobHeader
							where lineJob == null || lineJob.JobHeader.PK != firstLineJobHeader.JobHeader.PK
							select line;
						result = !linesWithAnotherJob.Any();
					}
				}
				return result;
			}
		}

		internal string CombinedGroupDescription
		{
			get
			{
				ZString result = "";

				foreach (DocARInvoiceLine line in this)
				{
					if (!line.LineDescription.IsEmpty)
					{
						if (result.IsEmpty)
						{
							result = line.LineDescription;
						}
						else if (result != line.LineDescription)
						{
							result = ZString.Empty;
							break;
						}
					}
				}

				if (result.IsEmpty)
				{
					result = CombinedGroupDescriptionWithChargeAndGLAccount;
				}

				return result;
			}
		}

		internal string CombinedGroupDescriptionWithChargeAndGLAccount
		{
			get
			{
				ZString result = "";

				if (this.Count > 0)
				{
					if (this[0].ChargeCode != null)
					{
						result = UseLocalLanguage((DocARInvoiceLine)this[0]) ? this[0].ChargeCode.AccChargeCode.AC_LocalLanguageDescription : this[0].ChargeCode.Desc;
					}
					else if (this[0].GLAccount != null)
					{
						result = this[0].GLAccount.Description;
					}
				}

				return result;
			}
		}

		bool UseLocalLanguage(DocARInvoiceLine line)
		{
			return AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.Value &&
				!line.ChargeCode.AccChargeCode.AC_LocalLanguageDescription.IsEmpty &&
				((line.Invoice.AccountOrg != null && line.Invoice.AccountOrg.Country != null && line.Invoice.AccountOrg.Country.Code == GlbCompany.CurrentCompany.GC_RN_NKCountryCode) || AccountingConfigurationRegistry.Instance.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.Value);
		}

		internal bool IsAtLeastOneTaxRateSpecified
		{
			get
			{
				foreach (IDocARInvoiceLine line in this)
				{
					if (line.TaxRate != null)
					{
						return true;
					}
				}
				return false;
			}
		}

		internal bool IsSameTaxRateForAllLines
		{
			get
			{
				if (this.Count > 0)
				{
					DocTaxRate rate = this[0].TaxRate;
					foreach (IDocARInvoiceLine line in this)
					{
						bool oneIsNull = line.TaxRate == null || rate == null;
						if ((oneIsNull && line.TaxRate != rate) ||
							(!oneIsNull && line.TaxRate.AccTaxRate.PK != rate.AccTaxRate.PK))
						{
							return false;
						}
					}
					return true;
				}
				return false;
			}
		}

		internal (DocTaxRate Rate, ZDecimal RateAmount_Raw, ZDecimal ExtraRateAmount) FirstReportableTaxRate
		{
			get
			{
				if (this.Count > 0)
				{
					foreach (IDocARInvoiceLine line in this)
					{
						if (line.TaxRate != null && line.TaxRate.Type.ToString().ToUpper() != AccTaxRate.Types.NotReportable)
						{
							return (line.TaxRate, line.TaxRateAmount_Raw, line.TaxExtraRateAmount);
						}
					}
				}

				return (null, 0, 0);
			}
		}

		internal decimal TotalOSExTaxamount
		{
			get
			{
				CalculateTotals();
				return fTotalOSExTaxamount;
			}
		}
		decimal fTotalOSExTaxamount;

		internal decimal TotalOSTaxAmount
		{
			get
			{
				CalculateTotals();
				return fTotalOSTaxAmount;
			}
		}
		decimal fTotalOSTaxAmount;

		internal decimal TotalOSSERAmount
		{
			get
			{
				CalculateTotals();
				return fTotalOSSERAmount;
			}
		}
		decimal fTotalOSSERAmount;

		internal decimal TotalLineAmount
		{
			get
			{
				CalculateTotals();
				return fTotalLineAmount;
			}
		}
		decimal fTotalLineAmount;

		internal decimal TotalGSTVAT
		{
			get
			{
				CalculateTotals();
				return fTotalGSTVat;
			}
		}
		decimal fTotalGSTVat;

		internal decimal TotalOSGSTAmount
		{
			get
			{
				CalculateTotals();
				return fTotalOSGSTAmount;
			}
		}
		decimal fTotalOSGSTAmount;

		internal decimal TotalOSEDUAmount
		{
			get
			{
				CalculateTotals();
				return fTotalOSEDUAmount;
			}
		}
		decimal fTotalOSEDUAmount;

		internal decimal TotalOSQSTAmount
		{
			get
			{
				CalculateTotals();
				return fTotalOSQSTAmount;
			}
		}
		decimal fTotalOSQSTAmount;

		internal decimal TotalOSRETAmount
		{
			get
			{
				CalculateTotals();
				return fTotalOSRETAmount;
			}
		}
		decimal fTotalOSRETAmount;

		internal decimal TotalOSSPVAmount
		{
			get
			{
				CalculateTotals();
				return fTotalOSSPVAmount;
			}
		}
		decimal fTotalOSSPVAmount;

		internal decimal TotalOSIntegratedGSTAmount
		{
			get
			{
				CalculateTotals();
				return fTotalOSIntegratedGSTAmount;
			}
		}
		decimal fTotalOSIntegratedGSTAmount;

		internal decimal TotalOSCentreGSTAmount
		{
			get
			{
				CalculateTotals();
				return fTotalOSCentreGSTAmount;
			}
		}
		decimal fTotalOSCentreGSTAmount;

		internal decimal TotalOSStateGSTAmount
		{
			get
			{
				CalculateTotals();
				return fTotalOSStateGSTAmount;
			}
		}
		decimal fTotalOSStateGSTAmount;

		internal decimal ConvertedTotalOSAmount
		{
			get
			{
				CalculateTotals();
				return fConvertedTotalOSAmount;
			}
		}
		decimal fConvertedTotalOSAmount;

		internal string Asterisks
		{
			get
			{
				CalculateTotals();
				return fAsterisks;
			}
		}
		string fAsterisks;

		internal ZDecimal OneCurrencyExchangeRate
		{
			get
			{
				CalculateTotals();

				ZDecimal? oneExchangeRate = null;

				foreach (IDocARInvoiceLine docLine in this)
				{
					DocARInvoiceLine invoiceLine = docLine as DocARInvoiceLine;
					if (invoiceLine != null)
					{
						if (oneExchangeRate.HasValue)
						{
							if (oneExchangeRate.Value != invoiceLine.CurrencyExRateAmount.ExchangeRate)
							{
								oneExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(fTotalOSExTaxamount, fConvertedTotalOSAmount);
								break;
							}
						}
						else
						{
							oneExchangeRate = invoiceLine.CurrencyExRateAmount.ExchangeRate;
						}
					}
				}

				return oneExchangeRate ?? 0;
			}
		}

		void CalculateTotals()
		{
			if (!TotalCalculated)
			{
				foreach (IDocARInvoiceLine line in this)
				{
					DocARInvoiceLine line1 = line as DocARInvoiceLine;
					if (line1 != null)
					{
						fTotalOSExTaxamount += line1.OSExTaxAmount;
						if (line.TaxRate != null)
						{
							fTotalOSTaxAmount += (line.TaxRate.ExtraType == AccTaxRate.ExtraTypes.ServiceTax
												&& GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Malaysia)
													? line1.OSSERAmount
													: line.TaxRate.ExtraType == AccTaxRate.ExtraTypes.RegionalTax
														? line1.OSGSTAmount
														: line1.OSTaxAmount;

							if (line.TaxRate.ExtraType == AccTaxRate.ExtraTypes.ServiceTax &&
								GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Malaysia)
							{
								fTotalOSSERAmount += line1.OSSERAmount;
							}
						}
						fTotalLineAmount += line1.LineAmount;
						fTotalGSTVat += line1.GSTVAT;
						fTotalOSGSTAmount += line1.OSGSTAmount;
						fTotalOSEDUAmount += line1.OSEDUAmount;
						fTotalOSQSTAmount += line1.OSQSTAmount;
						fTotalOSRETAmount += line1.OSRETAmount;
						fTotalOSSPVAmount += line1.OSSPVAmount;
						fTotalOSIntegratedGSTAmount += line1.OSIntegratedGSTAmount;
						fTotalOSCentreGSTAmount += line1.OSCentreGSTAmount;
						fTotalOSStateGSTAmount += line1.OSStateGSTAmount;
						fConvertedTotalOSAmount += line1.CurrencyExRateAmount.Amount;
						fAsterisks += line1.TaxRateAsterisks.Replace(' ', ',');
					}
				}
				SelectDistinctAndSortAsterisks();
				TotalCalculated = true;
			}
		}

		bool TotalCalculated;

		void SelectDistinctAndSortAsterisks()
		{
			if (!string.IsNullOrEmpty(fAsterisks))
			{
				List<string> asterisksList = new List<string>(fAsterisks.TrimStart(',').Split(','));
				asterisksList.Sort();

				fAsterisks = " " + String.Join(",", asterisksList.Distinct().ToArray());
			}
		}

		internal string AsterisksAsNumbers
		{
			get
			{
				List<ZString> list = new List<ZString>();
				foreach (IDocARInvoiceLine line in this)
				{
					ZString number = line.TaxRateAsterisksAsNumbers.Trim();
					if (number.Length > 0 && !list.Contains(number))
					{
						list.Add(number);
					}
				}
				list.Sort();
				return string.Join(",", list);
			}
		}

		public override void Add(BusinessObject businessObject)
		{
			TotalCalculated = false;
			fTotalOSExTaxamount = 0;
			fTotalOSTaxAmount = 0;
			fTotalLineAmount = 0;
			fTotalGSTVat = 0;
			fTotalOSGSTAmount = 0;
			fTotalOSEDUAmount = 0;
			fTotalOSQSTAmount = 0;
			fTotalOSRETAmount = 0;
			fTotalOSSPVAmount = 0;
			fConvertedTotalOSAmount = 0;
			fTotalOSIntegratedGSTAmount = 0;
			fTotalOSCentreGSTAmount = 0;
			fTotalOSStateGSTAmount = 0;
			fAsterisks = string.Empty;

			base.Add(businessObject);
		}

		public void ResetMultiplierTo1()
		{
			foreach (IDocARInvoiceLine line in this)
			{
				DocBatchARInvoiceLineTransactionLine line1 = line as DocBatchARInvoiceLineTransactionLine;
				if (line1 != null)
				{
					line1.ValueMultiplier = 1;
				}
			}
		}

		#endregion

		#region ISortableDocLineList

		void ISortableDocLineList.Sort(IComparer comparer)
		{
			Sort(comparer);
		}

		#endregion
	}
}

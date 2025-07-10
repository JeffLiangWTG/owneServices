using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.US.ISF;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers
{
	public class DocARInvoiceLine : DocumentWrapper
		, IDocARInvoiceLine
		, IGenericTransactionLinePlugIn
		, IDocLine
	{
		#region Constructors && Type Overidding
		protected DocARInvoiceLine(InvoicingLineBase line, BusinessObjectFactory factoryToWrap)
			: base(line, factoryToWrap)
		{
		}

		public static DocARInvoiceLine New(InvoicingLineBase line, BusinessObjectFactory factoryToWrap)
		{
			DocARInvoiceLine result = null;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(line, factoryToWrap);
			}
			else if (line != null)
			{
				result = new DocARInvoiceLine(line, factoryToWrap);
			}
			return result;
		}

		protected delegate DocARInvoiceLine NewDelegate(InvoicingLineBase line, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();
		#endregion

		#region IGenericTransactionLinePlugIn members

		GenericTransactionLineSupporter IGenericTransactionLinePlugIn.LineSupporter
		{
			get { return fGenericTransactionSupporter ?? (fGenericTransactionSupporter = new DocARInvoiceLineGenericTransactionSupporter(this)); }
		}
		DocARInvoiceLineGenericTransactionSupporter fGenericTransactionSupporter;

		#endregion

		#region DocARInvoiceLineGenericTransactionSupporter

		internal class DocARInvoiceLineGenericTransactionSupporter : GenericTransactionLineSupporter
		{
			public DocARInvoiceLineGenericTransactionSupporter(DocARInvoiceLine parent)
			{
				this.Parent = parent;
			}
			protected readonly DocARInvoiceLine Parent;

			protected internal override ZDecimal GetOSAmount()
			{
				return Parent.OSAmount;
			}

			protected internal override ZDecimal GetOSExTaxAmount()
			{
				return Parent.OSExTaxAmount;
			}

			protected internal override ZString GetOSTaxDisplay()
			{
				return Parent.OSTaxDisplay;
			}

			protected internal override ZString GetOSTaxAmountDisplay()
			{
				return Parent.OSTaxAmount_RawDisplay;
			}

			protected internal override ZString GetOSTaxDisplayNoAsterisksWithRegistryRule()
			{
				return Parent.OSTaxDisplayNoAsterisksWithRegistryRule;
			}

			protected internal override ZString GetGovernmentReportingCode()
			{
				return Parent.GovernmentReportingCode;
			}

			protected internal override ZString GetGovernmentReportingCodeHeading()
			{
				return Parent.GovernmentReportingCodeHeading;
			}

			protected internal override ZString GetTaxRateAsterisksAsNumbers()
			{
				return Parent.TaxRateAsterisksAsNumbers;
			}

			protected internal override ZBool GetIsSpacerLine()
			{
				return Parent.IsSpacerLine;
			}

			protected internal override ZString GetDescription()
			{
				return Parent.LineDescription;
			}

			protected internal override DocChargeCode GetChargeCode()
			{
				return Parent.ChargeCode;
			}

			protected internal override ZString GetGenericChargeCode()
			{
				return Parent.GenericChargeCode;
			}

			protected internal override ZString GetGenericChargeDescription()
			{
				return Parent.GenericChargeDescription;
			}

			protected internal override ZString GetGenericChargeType()
			{
				return Parent.GenericChargeType;
			}

			protected internal override DocJobHeader GetJobHeader()
			{
				return Parent.JobHeader;
			}

			protected internal override DocShipment GetShipment()
			{
				return Parent.Shipment;
			}

			protected internal override DocBranch GetBranch()
			{
				return Parent.Branch;
			}

			protected internal override DocDepartment GetDepartment()
			{
				return Parent.Department;
			}
		}

		#endregion

		#region ToString

		public override string ToString()
		{
			return NoDefaultPropertyErrorMessage;
		}

		#endregion

		#region CreatingUserName

		public ZString CreatingUserName
		{
			get { return Line.AL_Calc_CreatingUserName; }
		}

		#endregion

		#region CreatedDate

		public ZDateTime CreatedDate
		{
			get { return Line.AL_Calc_CreatedDate; }
		}

		#endregion

		#region ChargeCode

		public DocChargeCode ChargeCode
		{
			get
			{
				DocChargeCode result = null;
				if (!Line.AL_AC.IsEmpty)
				{
					result = DocChargeCode.New(Line.ChargeCode, Factory);
				}
				return result;
			}
		}

		#endregion

		public ZDateTime TaxDate => ShowInvoiceLineTaxDate ? Line.AL_TaxDate : ZDateTime.Empty;

		bool ShowInvoiceLineTaxDate => Line.AL_TaxDate.IsValid && Line.TaxRate != null && !Line.TaxRate.IsNonReportable;

		#region ARGlobalChargeCodes

		public DocGlobalChargeCodeCollection ARGlobalChargeCodes
		{
			get
			{
				return new DocGlobalChargeCodeCollection(Line.ARGlobalChargeCodes, Factory);
			}
		}

		#endregion

		#region GenericChargeCode

		public ZString GenericChargeCode
		{
			get
			{
				ZString result = ZString.Empty;
				if (Line.GenericChargeBizO != null)
				{
					result = Line.GenericChargeBizO.VC_Code;
				}
				return result;
			}
		}

		#endregion

		#region GenericChargeDescription

		public ZString GenericChargeDescription
		{
			get
			{
				ZString result = ZString.Empty;
				if (Line.GenericChargeBizO != null)
				{
					result = Line.GenericChargeBizO.VC_Description;
				}
				return result;
			}
		}

		#endregion

		#region GenericChargeType

		public ZString GenericChargeType
		{
			get
			{
				ZString result = ZString.Empty;
				if (Line.GenericChargeBizO != null)
				{
					result = Line.GenericChargeBizO.VC_Type;
				}
				return result;
			}
		}

		#endregion

		#region GLAccount

		public DocGLAccount GLAccount
		{
			get { return DocGLAccount.New(Line.GLHeader, Factory); }
		}

		#endregion

		#region PercentOfGLAccount

		public DocGLAccount PercentOfGLAccount
		{
			get { return DocGLAccount.New(Line.PercentOf, Factory); }
		}

		#endregion

		#region Invoice

		public DocARInvoice Invoice
		{
			get
			{
				return Factory.GetCachedValue(Line.TransactionHeader.PK.ToStringKey(), GetDocARinvoice());
			}
		}

		GetValueDelegate<DocARInvoice> GetDocARinvoice()
		{
			return delegate
			{
				InvoicingBase invoice = Factory.Load<InvoicingBase>(Line.TransactionHeader.PK);
				return DocARInvoice.New(invoice, Factory);
			};
		}

		#endregion

		#region InvoiceLineDescriptionForPeriodicInvoice

		public ZString InvoiceLineDescriptionForPeriodicInvoice
		{
			get { return ShowLocalAmountAndExRateOnInvoice ? LineDescriptionAndExchangeRate : LineDescription; }
		}

		protected ZBool ShowLocalAmountAndExRateOnInvoice
		{
			get
			{
				ZBool result = ZBool.False;

				ZString invoiceLineDisplayOption = "";
				if (GenericInvoicingJob != null && Line.TransactionHeader.Header != null)
				{
					invoiceLineDisplayOption = new OrgInvoiceRollupOrGroup.Loader(Line.TransactionHeader).GetInvoiceLineDisplayOption(
					GetServiceDirection(GenericInvoicingJob.Origin, GenericInvoicingJob.Destination), GenericInvoicingJob.TransportMode, Mode, GenericInvoicingJob.JobType);
				}

				return OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionIncludesExchangeRate(invoiceLineDisplayOption);
			}
		}

		protected ZString GetServiceDirection(ZString origin, ZString destination)
		{
			ZString result;

			if (origin.IsEmpty || destination.IsEmpty)
			{
				return ZString.Empty;
			}

			if (origin.Left(2) == GlbCompany.CurrentCompany.GC_RN_NKCountryCode &&
				destination.Left(2) == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				result = OrgConstants.ServiceDirection.Code.Domestic;
			}
			else if (origin.Left(2) == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				result = OrgConstants.ServiceDirection.Code.Export;
			}
			else if (destination.Left(2) == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				result = OrgConstants.ServiceDirection.Code.Import;
			}
			else
			{
				result = OrgConstants.ServiceDirection.Code.CrossTrade;
			}
			return result;
		}

		#endregion

		#region TaxRate

		public DocTaxRate TaxRate
		{
			get
			{
				DocTaxRate result = null;
				if (!Line.AL_AT.IsEmpty)
				{
					result = DocTaxRate.New(Line.TaxRate, Factory);
				}

				return result;
			}
		}

		public ZDecimal TaxRateAmount_Raw => Line.AL_TaxRateCalc_Raw;
		public ZDecimal TaxExtraRateAmount => Line.AL_TaxExtraRateCalc;
		public ZDecimal GetEffectiveExtraRate() => Line.GetEffectiveExtraRate();

		#endregion

		#region WithholdingTaxRate

		public DocWithholdingTaxRate WithholdingTaxRate
		{
			get { return DocWithholdingTaxRate.New(Line.Withholding, Factory); }
		}

		#endregion

		#region LineDescription

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required for condition checks")]
		public ZString LineDescription
		{
			get
			{
				var lineDescription = DocWrapperUtilities.GetMultilingualDescription(Line.AL_Desc, Line.ChargeCode);
				if (lineDescription == null && Line.GLHeader != null && Line.AL_Desc == Line.GLHeader.AG_Description)
				{
					lineDescription = Line.GLHeader.AG_DescriptionMultilingual;
				}
				else if (lineDescription == null)
				{
					lineDescription = Line.AL_Desc;
				}

				return lineDescription.Replace("\r", "");
			}
		}

		#endregion

		#region LineDescriptionAndExchangeRate

		public ZString LineDescriptionAndExchangeRate
		{
			get
			{
				ZString result = LineDescription;
				if (ChargeCode != null && ChargeCode.ChargeType != "CMT")
				{
					result = LineDescription + " " + ExchangeRateAndAmount;
				}
				return result;
			}
		}

		#endregion

		#region ExchangeRate

		public ZDecimal ExchangeRate
		{
			get { return Line.TransactionHeader != null ? Line.TransactionHeader.AH_ExchangeRate : Line.AL_ExchangeRate; }
		}

		#endregion

		#region Branch

		public DocBranch Branch
		{
			get { return DocBranch.New(Line.Branch, Factory); }
		}

		#endregion

		#region Department

		public DocDepartment Department
		{
			get { return DocDepartment.New(Line.Department, Factory); }
		}

		#endregion

		#region AmountMultiplier

		internal int AmountMultiplier
		{
			get
			{
				return Line.TransactionHeader != null && Line.TransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable &&
					Line.TransactionHeader.AH_TransactionType == TransactionTypes.CreditNote &&
					AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty) ? -1 : 1;
			}
		}

		#endregion

		#region IsARCreditNote

		internal bool IsARCreditNote
		{
			get
			{
				return Line.TransactionHeader != null && Line.TransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable &&
					Line.TransactionHeader.AH_TransactionType == TransactionTypes.CreditNote;
			}
		}

		#endregion

		#region GSTVAT

		public ZDecimal GSTVAT
		{
			get { return Line.AL_LocalTaxAmount * AmountMultiplier; }
		}

		#endregion

		#region ShowPercentInGSTDisplay

		public ZBool ShowPercentInGSTDisplay
		{
			get
			{
				return fShowPercentInGSTDisplay;
			}
			set
			{
				fShowPercentInGSTDisplay = value;
			}
		}
		protected ZBool fShowPercentInGSTDisplay = ZBool.True;

		#endregion

		#region OSAmountDisplay

		public ZString OSAmountDisplay
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(OSAmount, Line.TransactionCurrency);
			}
		}

		#endregion

		#region OSAmountDisplay

		public ZString OSTaxAmount_RawDisplay
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(OSTaxAmount_Raw, Currency);
			}
		}

		#endregion

		#region ExchangeRateDisplay

		public ZString ExchangeRateDisplay
		{
			get { return ExchangeRate.ToString("#####0.0000"); }
		}

		#endregion

		#region TaxRateDisplay

		public ZString TaxRateDisplay
		{
			get { return this.GetTaxRateDisplay(); }
		}

		#endregion

		#region TaxRateDisplayAlwaysShowPercentage

		public ZString TaxRateDisplayAlwaysShowPercentage
		{
			get { return this.GetTaxRateDisplay(true); }
		}

		#endregion

		#region TaxAmountDisplay

		public ZString TaxAmountDisplay
		{
			get { return this.GetTaxAmountDisplay(); }
		}

		#endregion

		#region OSTaxDisplayNoAsterisks

		public ZString OSTaxDisplayNoAsterisks
		{
			get { return this.GetOSTaxAmountDisplay(); }
		}

		#endregion

		#region OSTaxDisplayNoAsterisksWithRegistryRule

		public ZString OSTaxDisplayNoAsterisksWithRegistryRule
		{
			get { return this.GetOSTaxAmountDisplayWithRegistryRule(); }
		}

		#endregion

		#region OSTaxDisplayMainRate

		public ZString OSTaxDisplayMainRate
		{
			get
			{
				ZString result = OSTaxDisplayMainRateNoAsterisks;
				if (!result.IsEmpty)
				{
					result += GetAsterisks();
				}
				return result;
			}
		}

		#endregion

		#region OSTaxDisplayMainRateNoAsterisks

		public ZString OSTaxDisplayMainRateNoAsterisks
		{
			get { return this.GetOSTaxMainRateDisplay(); }
		}

		#endregion

		#region OSTaxDisplayMainAmount

		public ZString OSTaxDisplayMainAmount
		{
			get { return this.GetOSTaxMainAmountDisplay(); }
		}

		#endregion

		#region OSTaxDisplayExtraRate

		public ZString OSTaxDisplayExtraRate
		{
			get { return this.GetOSTaxExtraRateDisplay(); }
		}

		#endregion

		#region OSTaxDisplayExtraAmount

		public ZString OSTaxDisplayExtraAmount
		{
			get { return this.GetOSTaxExtraAmountDisplay(); }
		}

		#endregion

		#region OSTaxDisplay

		public ZString OSTaxDisplay
		{
			get
			{
				ZString result = OSTaxDisplayNoAsterisks;
				if (!result.IsEmpty)
				{
					result += GetAsterisks();
				}
				return result;
			}
		}

		#endregion

		#region TaxRateAsterisks / Numbers

		public ZString TaxRateAsterisks
		{
			get
			{
				ZString result = ZString.Empty;
				if (!IsSpacerLine && (ChargeCode == null || ChargeCode.ChargeType != "CMT") && Line.TaxRate != null)
				{
					result = GetAsterisks();
				}
				return result;
			}
		}

		public ZString TaxRateAsterisksAsNumbers
		{
			get
			{
				ZString result = ZString.Empty;
				if (!IsSpacerLine && (ChargeCode == null || ChargeCode.ChargeType != "CMT") && Line.TaxRate != null)
				{
					result = GetNumber();
				}
				return result;
			}
		}

		#endregion

		#region IncludeTaxAmountInOsTaxDisplay

		public ZBool IncludeTaxAmountInOsTaxDisplay
		{
			get { return !AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.Value; }
		}

		#endregion

		#region OSExTaxAmount

		public ZDecimal OSExTaxAmount
		{
			get { return OSExTaxAmountCore * AmountMultiplier; }
		}

		protected virtual ZDecimal OSExTaxAmountCore
		{
			get { return Line.InvoiceBase != null && Line.InvoiceBase.AH_RX_NKTransactionCurrency == Line.InvoiceBase.AH_Calc_LocalRXCode ? Line.AL_LocalExTaxAmount : Line.AL_OSExTaxAmount; }
		}

		#endregion

		#region OSTaxAmount_Raw 
		public ZDecimal OSTaxAmount_Raw
		{
			get
			{
				var oSTaxAmount = OSExTaxAmount * (TaxRateAmount_Raw / 100);
				return Utilities.Round(oSTaxAmount, LocalDecimals);
			}
		}

		#endregion

		#region LocalTaxAmount_Raw  
		public ZDecimal LocalTaxAmount_Raw
		{
			get
			{
				var localTaxAmount = LocalExTaxAmount * (TaxRateAmount_Raw / 100);
				return localTaxAmount;
			}
		}

		int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		#endregion

		#region LocalExTaxAmount

		public ZDecimal LocalExTaxAmount
		{
			get { return LocalExTaxAmountCore * AmountMultiplier; }
		}

		protected virtual ZDecimal LocalExTaxAmountCore
		{
			get { return Line.AL_LocalExTaxAmount; }
		}

		#endregion

		#region GovtComplianceDocumentExchangeRate

		public ZDecimal GovtComplianceDocumentExchangeRate
		{
			get
			{
				if (govtComplianceDocumentExchangeRate == 0m)
				{
					string rateType = AccountingConfigurationRegistry.Instance.AccGovtComplianceDocumentExchangeRateType.Value;
					string rateDate = AccountingConfigurationRegistry.Instance.AccGovtComplianceDocumentExchangeRateDate.Value;
					ZDateTime exRateDate = rateDate == "PST" ? Line.InvoiceBase.AH_PostDate : Line.InvoiceBase.AH_InvoiceDate;

					ExchangeRateType exchangeRateType = Enterprise.ZArchitecture.Environment.ExchangeRate.GetExchangeRateType(rateType);
					govtComplianceDocumentExchangeRate = Env.CurrentCompany.ExchangeRate.GetRateIncludingExpired(GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, exchangeRateType, exRateDate.ToDateTime());
				}
				return govtComplianceDocumentExchangeRate;
			}
		}
		ZDecimal govtComplianceDocumentExchangeRate;

		#endregion

		#region CalculatedLocalExTaxAmount

		public ZDecimal CalculatedLocalExTaxAmount
		{
			get
			{
				ZString currency = HeaderCurrency != null ? HeaderCurrency.Code : Invoice.Currency != null ? Invoice.Currency.Code : ZString.Empty;
				if (GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency == GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency)
				{
					return LocalExTaxAmount;
				}
				else if (currency == GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency)
				{
					return OSExTaxAmount;
				}
				else
				{
					return Env.CurrentCompany.ExchangeRate.LocalToForeign(LocalExTaxAmount, GovtComplianceDocumentExchangeRate, currency);
				}
			}
		}

		#endregion

		#region CalculatedLocalTaxAmount

		public ZDecimal CalculatedLocalTaxAmount
		{
			get
			{
				ZString currency = HeaderCurrency != null ? HeaderCurrency.Code : Invoice.Currency != null ? Invoice.Currency.Code : ZString.Empty;
				if (GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency == GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency)
				{
					return GSTVAT;
				}
				else if (currency == GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency)
				{
					return OSTaxAmount;
				}
				else
				{
					return TaxAmountCalculator.GetOSTaxAmount(Factory, OSExTaxAmount, Line.TaxRate, Line.AL_TaxRateCalc, Line.AL_TaxExtraRateCalc, GSTVAT, GovtComplianceDocumentExchangeRate, RefCurrency.LoadFromCurrencyCode(Factory, currency), Line.AL_GC);
				}
			}
		}

		#endregion

		#region DisplayCurrency

		public DocCurrency DisplayCurrency
		{
			get { return Line.InvoiceBase != null && Line.InvoiceBase.AH_RX_NKTransactionCurrency == Line.InvoiceBase.AH_Calc_LocalRXCode ? DocCurrency.New(Factory, GlbCompany.CurrentCompany.LocalCurrency) : Currency; }
		}

		#endregion

		#region OSTaxAmount

		public ZDecimal OSTaxAmount
		{
			get { return OSTaxAmountCore * AmountMultiplier; }
		}

		protected virtual ZDecimal OSTaxAmountCore
		{
			get
			{
				var result = 0m;
				if (Line.TaxRate != null && Line.TaxRate.AT_ExtraTaxRateType != AccTaxRate.ExtraTypes.ServiceTax && Line.TaxRate.AT_ExtraTaxRateType != AccTaxRate.ExtraTypes.RegionalTax)
				{
					result = Line.InvoiceBase != null && Line.InvoiceBase.AH_RX_NKTransactionCurrency == Line.InvoiceBase.AH_Calc_LocalRXCode ? Line.AL_LocalTaxAmount : Line.AL_OSTaxAmount;
				}
				return result;
			}
		}

		#endregion

		#region OSGSTAmount

		public ZDecimal OSGSTAmount
		{
			get { return (Line.InvoiceBase != null && Line.InvoiceBase.AH_RX_NKTransactionCurrency == Line.InvoiceBase.AH_Calc_LocalRXCode ? Line.AL_LocalGSTAmount : Line.AL_OSGSTAmount) * AmountMultiplier; }
		}

		#endregion

		#region ExtraTaxAmount

		public ZDecimal OSExtraTaxAmount
		{
			get
			{
				return (Line.InvoiceBase != null && Line.InvoiceBase.AH_RX_NKTransactionCurrency == Line.InvoiceBase.AH_Calc_LocalRXCode ? Line.AL_LocalExtraTaxAmount : Line.AL_OSExtraTaxAmount) * AmountMultiplier;
			}
		}

		#endregion

		#region OSIGICAmount

		public ZDecimal OSIGICAmount
		{
			get
			{
				var result = 0m;
				if (Line.TaxRate != null && Line.TaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.RegionalTax)
				{
					result = Line.InvoiceBase != null && Line.InvoiceBase.AH_RX_NKTransactionCurrency == Line.InvoiceBase.AH_Calc_LocalRXCode ? Line.AL_LocalTaxAmount : Line.AL_OSTaxAmount;
				}
				return result;
			}
		}

		#endregion

		#region OSSPVAmount

		public ZDecimal OSSPVAmount
		{
			get
			{
				var result = 0m;
				if (Line.TaxRate != null && Line.TaxRate.IsVATRemittedByCustomer)
				{
					result = Line.InvoiceBase != null && Line.InvoiceBase.AH_RX_NKTransactionCurrency == Line.InvoiceBase.AH_Calc_LocalRXCode ? Line.AL_LocalExtraTaxAmount : Line.AL_OSExtraTaxAmount;
				}
				return result * AmountMultiplier;
			}
		}

		public ZDecimal LocalSPVAmount
		{
			get
			{
				var result = 0m;
				if (Line.TaxRate != null && Line.TaxRate.IsVATRemittedByCustomer)
				{
					result = Line.AL_LocalExtraTaxAmount;
				}
				return result * AmountMultiplier;
			}
		}

		#endregion

		#region OSSERAmount

		public ZDecimal OSSERAmount
		{
			get
			{
				var result = 0m;
				if (Line.TaxRate != null
					&& Line.TaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.ServiceTax
					&& GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Malaysia)
				{
					result = Line.InvoiceBase != null && Line.InvoiceBase.AH_RX_NKTransactionCurrency == Line.InvoiceBase.AH_Calc_LocalRXCode ? Line.AL_LocalTaxAmount : Line.AL_OSTaxAmount;
				}
				return result;
			}
		}

		#endregion

		#region OSQSTAmount

		public ZDecimal OSQSTAmount
		{
			get { return (Line.InvoiceBase != null && Line.InvoiceBase.AH_RX_NKTransactionCurrency == Line.InvoiceBase.AH_Calc_LocalRXCode ? Line.AL_LocalQSTAmount : Line.AL_OSQSTAmount) * AmountMultiplier; }
		}

		#endregion

		#region OSSBCAmount

		public ZDecimal OSSBCAmount
		{
			get
			{
				var result = ZDecimal.Zero;
				if (IsExtraTaxSBCAndKKC && OSQSTAmount != ZDecimal.Zero)
				{
					result = OSQSTAmount / 2;
				}
				else if (IsExtraTaxSBCOrKKC && LineType == TransactionLineTypes.Revenue)
				{
					result = OSQSTAmount;
				}
				return result;
			}
		}

		#endregion

		#region OSKKCAmount

		public ZDecimal OSKKCAmount
		{
			get
			{
				var result = ZDecimal.Zero;
				if (OSQSTAmount != ZDecimal.Zero)
				{
					if (IsExtraTaxSBCAndKKC)
					{
						result = OSQSTAmount / 2;
					}
					else if (IsExtraTaxSBCOrKKC && LineType == TransactionLineTypes.Cost)
					{
						result = OSQSTAmount;
					}
				}
				return result;
			}
		}

		#endregion

		#region IsExtraTaxSBCAndKKC

		public ZBool IsExtraTaxSBCAndKKC
		{
			get
			{
				return Line.TaxRate != null
					&& Line.TaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India
					&& Line.TaxRate.IsRatedTax()
					&& Line.TaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase
					&& (Line.AL_TaxExtraRateCalc == 1m
						|| Line.AL_TaxExtraRateCalc == 0.40m
						|| Line.AL_TaxExtraRateCalc == 0.30m);
			}
		}

		#endregion

		#region IsExtraTaxSBCOrKKC

		public ZBool IsExtraTaxSBCOrKKC
		{
			get
			{
				return Line.TaxRate != null
					&& Line.TaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India
					&& Line.TaxRate.IsRatedTax()
					&& Line.TaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase
					&& (Line.AL_TaxExtraRateCalc == 0.5m
						|| Line.AL_TaxExtraRateCalc == 0.20m
						|| Line.AL_TaxExtraRateCalc == 0.15m);
			}
		}

		#endregion

		#region OSEDUAmount

		public ZDecimal OSEDUAmount
		{
			get { return (Line.InvoiceBase != null && Line.InvoiceBase.AH_RX_NKTransactionCurrency == Line.InvoiceBase.AH_Calc_LocalRXCode ? Line.AL_LocalEDUAmount : Line.AL_OSEDUAmount) * AmountMultiplier; }
		}

		#endregion

		#region OSEDUPrimaryAmount

		public ZDecimal OSEDUPrimaryAmount
		{
			get { return (Line.InvoiceBase != null && Line.InvoiceBase.AH_RX_NKTransactionCurrency == Line.InvoiceBase.AH_Calc_LocalRXCode ? Line.AL_LocalEDUPrimaryAmount : Line.AL_OSEDUPrimaryAmount) * AmountMultiplier; }
		}

		#endregion

		#region OSEDUSecondaryAmount

		public ZDecimal OSEDUSecondaryAmount
		{
			get { return (Line.InvoiceBase != null && Line.InvoiceBase.AH_RX_NKTransactionCurrency == Line.InvoiceBase.AH_Calc_LocalRXCode ? Line.AL_LocalEDUSecondaryAmount : Line.AL_OSEDUSecondaryAmount) * AmountMultiplier; }
		}
		const int RETMultiplier = -1;

		#endregion

		#region OSRETAmount

		public ZDecimal OSRETAmount
		{
			get { return (Line.InvoiceBase != null && Line.InvoiceBase.AH_RX_NKTransactionCurrency == Line.InvoiceBase.AH_Calc_LocalRXCode ? Line.AL_LocalExtraTaxAmount : Line.AL_OSExtraTaxAmount) * RETMultiplier * AmountMultiplier; }
		}

		#endregion

		#region JobHeader

		public DocJobHeader JobHeader
		{
			get
			{
				DocJobHeader result = null;
				if (!Line.AL_JH.IsEmpty)
				{
					result = DocJobHeader.New(Line.Job, Factory);
				}

				return result;
			}
		}

		#endregion

		#region JobNumber

		public ZString JobNumber { get { return JobHeader != null ? JobHeader.JobNumber : ZString.Empty; } }

		#endregion

		#region JobTypeForPeriodicInvoice

		public ZString JobTypeForPeriodicInvoice
		{
			get
			{
				if (IsLoadListJob || IsCFSShipmentJob)
				{
					return ResString.GetMultilingualString("5e2c101b-a0d0-4c9d-8dc1-65ff2b72595d", "CTO / CFS JOBS");
				}
				else if (IsForwardingJob)
				{
					return ResString.GetMultilingualString("3865b086-3375-435f-bd36-9f4e86fc087b", "SHIPMENT JOBS");
				}
				else if (IsAgencyShippingJob)
				{
					return ResString.GetMultilingualString("2b9622e3-ea92-4a2a-9718-a8f2a308024b", "LINER & AGENCY JOBS");
				}
				else if (IsLocalCartage)
				{
					return ResString.GetMultilingualString("22609864-2350-4f8b-9c9c-9a7082e46c47", "LOCAL CARTAGE");
				}
				else if (IsCustomJob)
				{
					return ResString.GetMultilingualString("4facb4fb-568f-4274-bd31-bff042d6e1cd", "DECLARATION JOBS");
				}
				else if (IsISFJob)
				{
					return ResString.GetMultilingualString("fdbe902c-c092-4b00-ab0e-500a6b8418a0", "SECURITY FILING JOBS");
				}
				else if (IsConsignmentJob)
				{
					return ResString.GetMultilingualString("7f3045a7-7ccb-4838-88b5-b85316d2b5b1", "CONSIGNMENT");
				}
				else if (IsSundryCharges)
				{
					return ResString.GetMultilingualString("771034ae-65d6-462e-8a5f-b22d54b78cd7", "SUNDRY CHARGES");
				}
				else if (IsVoyageAccounting)
				{
					return ResString.GetMultilingualString("49399372-ce65-48cf-af01-2347752ec9dc", "VOYAGE ACCOUNTING");
				}
				else if (IsContainerDetention)
				{
					return ResString.GetMultilingualString("c0b61bb1-c600-4073-b713-d0d0f9a9f98a", "CONTAINER DETENTION");
				}
				else if (IsWarehouseJob) // Leave at bottom, more expensive than loading other types of jobs
				{
					return ResString.GetMultilingualString("8fb295dd-1e00-441e-b60d-3180caaba3a0", "WAREHOUSE JOBS");
				}
				else if (IsNCTSJob)
				{
					return ResString.GetMultilingualString("90C31CD8-174E-4DA6-A9C1-FC7CA90B16FC", "NCTS JOBS");
				}
				else
				{
					return ResString.GetMultilingualString("0f453f76-e5f7-4096-92b5-34348f860d5a", "MISCELLANEOUS JOBS");
				}
			}
		}

		#endregion

		public DocAmountByChargeCodeCollection AmountSplittedByChargeCode
		{
			get { return new DocAmountByChargeCodeCollection(Factory); }
		}

		#region OperationsJob

		public FreightWrapper OperationsJob
		{
			get
			{
				return OpJobHelper.OperationsJob;
			}
		}

		#endregion

		#region GenericInvoicingJob

		protected DocJobInvoicingJob GenericInvoicingJob
		{
			get
			{
				DocJobInvoicingJob genericInvoicingJob = null;
				if (!Line.AL_JH.IsEmpty)
				{
					genericInvoicingJob = DocJobInvoicingJob.New(Line.InvoicingJob, Factory);
				}
				return genericInvoicingJob;
			}
		}

		#endregion

		#region Mode

		protected ZString Mode
		{
			get
			{
				if (GenericInvoicingJob != null)
				{
					return GenericInvoicingJob.TransportMode == Core.Constants.TransportModes.Sea ? GenericInvoicingJob.ContainerMode : GenericInvoicingJob.TransportMode;
				}
				return ZString.Empty;
			}
		}

		#endregion

		#region LineAmount

		public ZDecimal LineAmount
		{
			get { return LineAmountCore * AmountMultiplier; }
		}

		protected virtual ZDecimal LineAmountCore
		{
			get { return Line.AL_LocalExTaxAmount; }
		}

		#endregion

		#region LineType

		public ZString LineType
		{
			get { return Line.AL_LineType; }
		}

		#endregion

		#region Organisation

		public DocOrganisation Organisation
		{
			get { return DocOrganisation.New(Line.Header, Factory); }
		}

		#endregion

		#region OSAmount

		public ZDecimal OSAmount
		{
			get { return OSAmountCore * AmountMultiplier; }
		}

		protected virtual ZDecimal OSAmountCore
		{
			get { return Line.InvoiceBase != null && Line.InvoiceBase.AH_RX_NKTransactionCurrency == Line.InvoiceBase.AH_Calc_LocalRXCode ? Line.AL_LocalTotalAmount : Line.AL_OverseasTotal; }
		}

		#endregion

		#region LocalAmount

		public ZDecimal LocalAmount
		{
			get { return LocalAmountCore * AmountMultiplier; }
		}

		protected virtual ZDecimal LocalAmountCore
		{
			get { return Line.AL_LocalTotalAmount; }
		}

		#endregion

		#region OSUnitPrice

		public ZDecimal OSUnitPrice
		{
			get { return Line.AL_OSUnitPrice; }
		}

		#endregion

		#region PercentageOfPeriod

		public ZInt PercentageOfPeriod
		{
			get { return Line.AL_PercentageOfPeriod; }
		}

		#endregion

		#region PostDate

		public ZDateTime PostDate
		{
			get { return Line.AL_PostDate; }
		}

		#endregion

		#region PostPeriod

		public ZInt PostPeriod
		{
			get { return Line.AL_PostPeriod; }
		}

		#endregion

		#region PostToGL

		public ZBool PostToGL
		{
			get { return Line.AL_PostToGL == "Y"; }
		}

		#endregion

		#region PreventInvoicePrintGrouping

		public ZBool PreventInvoicePrintGrouping
		{
			get { return Line.AL_PreventInvoicePrintGrouping; }
		}

		#endregion

		#region PreventGrouping

		public ZBool PreventGrouping
		{
			get
			{
				if (!preventGrouping.HasValue)
				{
					preventGrouping = PreventInvoicePrintGrouping || (!Invoice.IsPeriodicInvoice && (ChargeCode != null && ChargeCode.ChargeGroup == ChargeCodeGroupList.Codes.NotGrouped));
				}

				return preventGrouping.Value;
			}
		}
		ZBool? preventGrouping;

		#endregion

		#region ReverseDate

		public ZDateTime ReverseDate
		{
			get { return Line.AL_ReverseDate; }
		}

		#endregion

		#region ReversePeriod

		public ZInt ReversePeriod
		{
			get { return Line.AL_ReversePeriod; }
		}

		#endregion

		#region ReverseToGL

		public ZBool ReverseToGL
		{
			get { return Line.AL_ReverseToGL == "Y"; }
		}

		#endregion

		#region Currency

		public DocCurrency Currency
		{
			get { return DocCurrency.New(Line.TransactionCurrency, Factory); }
		}

		#endregion

		#region Sequence

		public ZShort Sequence
		{
			get { return Line.AL_Sequence; }
		}

		#endregion

		#region UnitPrice

		public ZDecimal UnitPrice
		{
			get { return Line.AL_UnitPrice; }
		}

		#endregion

		#region UnitQty

		public ZInt UnitQty
		{
			get { return Line.AL_UnitQty; }
		}

		#endregion

		#region WithholdingTax

		public ZDecimal WithholdingTax
		{
			get { return Line.AL_WithholdingTax; }
		}

		#endregion

		#region Shipment

		public DocShipment Shipment
		{
			get
			{
				DocShipment result = null;
				if (JobHeader != null)
				{
					if (JobHeader.ParentTableCode == JobShipmentSchema.Constants.Prefix)
					{
						result = Factory.GetCachedValue(JobHeader.ParentID.ToStringKey(), () => DocShipment.New(Factory, JobHeader.ParentID), CacheStalenessPolicy.StaleOnFactorySave);
					}
				}

				return result;
			}
		}

		#endregion

		#region ISFHeader

		public FreightWrapper ISFHeader
		{
			get
			{
				FreightWrapper result = null;

				if (this.JobHeader != null)
				{
					if (this.JobHeader.ParentTableCode == CusISFHeaderSchema.Constants.Prefix)
					{
						result = FreightWrapper.New((BusinessObject)Factory.Load<ICusISFHeader>(this.JobHeader.ParentID), Factory)[0];
					}
				}

				return result;
			}
		}

		#endregion

		#region FKToShipment

		public ZString FKToShipment
		{
			get
			{
				if (this.JobHeader != null)
				{
					return (this.JobHeader.ParentTableCode == "JS") ? this.JobHeader.ParentID.ToString() : "";
				}
				else
				{
					return "";
				}
			}
		}

		#endregion

		#region AmountWithExchangeRate

		public struct AmountWithExchangeRate
		{
			public ZDecimal Amount;
			public ZDecimal ExchangeRate;
			public DocCurrency Currency;

			public override string ToString()
			{
				string result = "";

				if (Currency != null)
				{
					ZDecimal roundedOverseasAmount = ZArchitecture.Core.Utilities.Round(Amount, Currency.Decimals);
					int exRateNumDecimals = GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;
					ZDecimal roundedExchangeRate = ZArchitecture.Core.Utilities.Round(ExchangeRate, exRateNumDecimals);
					result = Currency.Code + " " + FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(roundedOverseasAmount, Currency) + " @ " + roundedExchangeRate.ToString();
				}

				return result;
			}
		}

		#endregion

		#region SellRecognition

		public ZString SellRecognition
		{
			get { return Charge != null ? Charge.SellRecognition : ZString.Empty; }
		}

		#endregion

		#region CostRecognition

		public ZString CostRecognition
		{
			get { return Charge != null ? Charge.CostRecognition : ZString.Empty; }
		}

		#endregion

		#region ProductName

		public ZString ProductName
		{
			get { return Charge != null ? Charge.ProductName : ZString.Empty; }
		}

		#endregion

		#region IsApproved

		public ZBool IsApproved
		{
			get { return Charge != null ? Charge.IsApproved : ZBool.False; }
		}

		#endregion

		#region SellAccount

		public ZString SellAccount
		{
			get { return Charge != null && Charge.SellAccount != null ? Charge.SellAccount.Name : ZString.Empty; }
		}

		#endregion

		#region CostAccount

		public ZString CostAccount
		{
			get { return (Charge != null && Charge.CostAccount != null) ? Charge.CostAccount.Name : ZString.Empty; }
		}

		#endregion

		#region IsApportioned

		public ZBool IsApportioned
		{
			get { return Charge != null ? Charge.IsApportioned : ZBool.False; }
		}

		#endregion

		#region CostPosted

		public ZBool CostPosted
		{
			get { return Charge != null ? Charge.CostPosted : ZBool.False; }
		}

		#endregion

		#region SellPosted

		public ZBool SellPosted
		{
			get { return Charge != null ? Charge.SellPosted : ZBool.False; }
		}

		#endregion

		#region CFXJnl

		public ZDecimal CFXJnl
		{
			get { return Charge != null ? Charge.CFXJnl : new ZDecimal(); }
		}

		#endregion

		#region ChargeCodePrintSequence

		public ZShort ChargeCodePrintSequence
		{
			get { return Charge != null ? Charge.ChargeCodePrintSequence : new ZShort(); }
		}

		#endregion

		#region ExchangeRateAndAmount

		public ZString ExchangeRateAndAmount
		{
			get { return CurrencyExRateAmount.ToString(); }
		}

		#endregion

		#region CurrencyExRateAmount

		internal AmountWithExchangeRate CurrencyExRateAmount
		{
			get
			{
				if (!currencyExRateAmount.HasValue)
				{
					var result = new AmountWithExchangeRate();

					if ((Invoice.ShowLocalAmountAndExRateOnInvoice && !Invoice.IsPeriodicInvoice) || (ShowLocalAmountAndExRateOnInvoice && Invoice.IsPeriodicInvoice))
					{
						var transactionCurrency = Line.TransactionHeader.AH_RX_NKTransactionCurrency;
						var localCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

						if (WrappedCharge != null &&
							(transactionCurrency == localCurrency ||
							(WrappedCharge.BillInInvoiceCurrency && transactionCurrency == WrappedCharge.JR_RX_NKSellInvoiceCurrency)))
						{
							if (Charge.OSSellCurrency != null &&
								(Charge.OSSellCurrency.Code != localCurrency || WrappedCharge.BillInInvoiceCurrency))
							{
								result.Currency = Charge.OSSellCurrency;
								result.Amount = WrappedCharge.JR_OSSellAmt;
								if (WrappedCharge.BillInInvoiceCurrency)
								{
									result.ExchangeRate = Invoice.CrossExchangeRatesBasedOnJobChargeSellCurrency[Charge.OSSellCurrency.Code];
								}
								else
								{
									result.ExchangeRate = Charge.OSSellExRate;
								}
							}
						}
					}
					currencyExRateAmount = result;
				}
				return currencyExRateAmount.Value;
			}
		}
		AmountWithExchangeRate? currencyExRateAmount;

		#endregion

		#region OSChargeAmountProperties

		public struct OSChargeAmountProperties
		{
			public ZDecimal OSAmount;
			public ZDecimal ExchangeRate;
			public DocCurrency Currency;
		}

		public struct ChargeAmountWithoutRate
		{
			public ZDecimal Amount;
			public DocCurrency Currency;
		}

		#endregion

		#region OSChargeAmountWithCurrency

		internal OSChargeAmountProperties OSChargeAmountWithCurrency
		{
			get
			{
				OSChargeAmountProperties result = new OSChargeAmountProperties();

				if (Charge != null)
				{
					if (Charge.OSSellCurrency != null && Currency != null)
					{
						result.OSAmount = Charge.OSSellAmt;
						result.Currency = Charge.OSSellCurrency;
						result.ExchangeRate = Charge.OSSellExRate;
					}
				}

				return result;
			}
		}

		#endregion

		#region ChargeAmountWithoutRate

		internal ChargeAmountWithoutRate ChargeAmount(ZDecimal amount)
		{
			var result = new ChargeAmountWithoutRate();

			if (Charge != null)
			{
				if (Charge.OSSellCurrency != null && Currency != null)
				{
					result.Amount = amount;
					result.Currency = Charge.OSSellCurrency;
				}
			}
			return result;
		}

		#endregion

		#region ChargeOSAmountAndCurrency

		public ZString ChargeOSAmountAndCurrency
		{
			get
			{
				ZString result = ZString.Empty;
				if (OSChargeAmountWithCurrency.Currency != null)
				{
					return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(OSChargeAmountWithCurrency.OSAmount, OSChargeAmountWithCurrency.Currency) + " " + OSChargeAmountWithCurrency.Currency;
				}
				return result;
			}
		}

		#endregion

		#region ChargeExchangeRate

		public ZDecimal ChargeExchangeRate
		{
			get
			{
				var result = ZDecimal.Zero;
				if (Charge != null)
				{
					if (WrappedCharge != null && WrappedCharge.BillInInvoiceCurrency)
					{
						result = Invoice.CrossExchangeRatesBasedOnJobChargeSellCurrency[Charge.OSSellCurrency.Code];
					}
					else
					{
						result = Charge.OSSellExRate;
					}
				}
				return result;
			}
		}

		#endregion

		#region ChargeOSAmount

		public ZDecimal ChargeOSAmount
		{
			get { return Charge == null ? ZDecimal.Zero : Charge.OSSellAmt; }
		}

		#endregion

		#region ChargeOSAmountForCLC

		public ZDecimal ChargeOSAmountForCLC
		{
			get
			{
				var multiplier = Line.TransactionHeader != null && Line.TransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable && Line.TransactionHeader.AH_TransactionType == TransactionTypes.CreditNote ? -1 : 1;
				return ChargeOSAmount * AmountMultiplier * multiplier;
			}
		}

		#endregion

		#region ChargeCurrency

		public ZString ChargeCurrency
		{
			get { return Charge == null || Charge.OSSellCurrency == null ? ZString.Empty : Charge.OSSellCurrency.Code; }
		}

		#endregion

		#region DisplaySequence

		public ZInt DisplaySequence
		{
			get { return Charge != null ? Charge.DisplaySequence : ZInt.Zero; }
		}

		#endregion

		#region EstimatedCostWithCurrency

		public ZString EstimatedCostWithCurrency
		{
			get
			{
				var result = ZString.Empty;
				var chargeAmount = ChargeAmount(EstimatedCost);
				if (chargeAmount.Currency != null)
				{
					result = FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(chargeAmount.Amount, chargeAmount.Currency) + " " + chargeAmount.Currency;
				}
				return result;
			}
		}

		#endregion

		#region EstimatedCost

		public ZDecimal EstimatedCost
		{
			get { return Charge != null ? Charge.EstimatedCost : ZDecimal.Zero; }
		}

		#endregion

		#region EstimatedRevenueWithCurrency

		public ZString EstimatedRevenueWithCurrency
		{
			get
			{
				var result = ZString.Empty;
				var chargeAmount = ChargeAmount(EstimatedRevenue);
				if (chargeAmount.Currency != null)
				{
					result = FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(chargeAmount.Amount, chargeAmount.Currency) + " " + chargeAmount.Currency;
				}
				return result;
			}
		}

		#endregion

		#region EstimatedRevenue

		public ZDecimal EstimatedRevenue
		{
			get { return Charge != null ? Charge.EstimatedRevenue : ZDecimal.Zero; }
		}

		#endregion

		#region LocalCostAmountWithCurrency

		public ZString LocalCostAmountWithCurrency
		{
			get
			{
				var result = ZString.Empty;
				var chargeAmount = ChargeAmount(LocalCostAmount);
				if (chargeAmount.Currency != null)
				{
					result = FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(chargeAmount.Amount, chargeAmount.Currency) + " " + chargeAmount.Currency;
				}
				return result;
			}
		}

		#endregion

		#region LocalCostAmount

		public ZDecimal LocalCostAmount
		{
			get { return Charge != null ? Charge.LocalCostAmt : ZDecimal.Zero; }
		}

		#endregion

		#region LocalSellAmountWithCurrency

		public ZString LocalSellAmountWithCurrency
		{
			get
			{
				var result = ZString.Empty;
				var chargeAmount = ChargeAmount(LocalSellAmount);
				if (chargeAmount.Currency != null)
				{
					result = FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(chargeAmount.Amount, chargeAmount.Currency) + " " + chargeAmount.Currency;
				}
				return result;
			}
		}

		#endregion

		#region LocalSellAmount

		public ZDecimal LocalSellAmount
		{
			get { return Charge != null ? Charge.LocalSellAmt : ZDecimal.Zero; }
		}

		#endregion

		#region OSCostAmountWithCurrency

		internal OSChargeAmountProperties OSCostAmountWithCurrency
		{
			get
			{
				var result = new OSChargeAmountProperties();

				if (Charge != null)
				{
					if (Charge.OSCostCurrency != null && Currency != null)
					{
						result.OSAmount = Charge.OSCostAmt;
						result.Currency = Charge.OSCostCurrency;
						result.ExchangeRate = Charge.OSCostExRate;
					}
				}
				return result;
			}
		}

		#endregion

		#region CostOSAmountAndCurrency

		public ZString CostOSAmountAndCurrency
		{
			get
			{
				var result = ZString.Empty;
				if (OSCostAmountWithCurrency.Currency != null)
				{
					result = FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(OSCostAmountWithCurrency.OSAmount, OSCostAmountWithCurrency.Currency) + " " + OSCostAmountWithCurrency.Currency;
				}
				return result;
			}
		}

		#endregion

		#region CostExchangeRate

		public ZDecimal CostExchangeRate
		{
			get { return Charge != null ? Charge.OSCostExRate : ZDecimal.Zero; }
		}

		#endregion

		#region CostOSAmount

		public ZDecimal CostOSAmount
		{
			get { return Charge != null ? Charge.OSCostAmt : ZDecimal.Zero; }
		}

		#endregion

		#region CostCurrency

		public ZString CostCurrency
		{
			get { return Charge != null && Charge.OSCostCurrency != null ? Charge.OSCostCurrency.Code : ZString.Empty; }
		}

		#endregion

		#region HeaderOrganisation

		public DocOrganisation HeaderOrganisation
		{
			get { return fHeaderOrganisation; }
			set { fHeaderOrganisation = value; }
		}

		#endregion

		#region HeaderCurrency

		public DocCurrency HeaderCurrency
		{
			get { return fHeaderCurrency; }
			set { fHeaderCurrency = value; }
		}

		#endregion

		#region LocalAmountAndTax

		public ZDecimal LocalAmountAndTax
		{
			get { return LineAmount + GSTVAT; }
		}

		#endregion

		#region Charge

		public DocJobInvoicingJobCharge Charge
		{
			get
			{
				if (fCharge == null)
				{
					var tempCharge = Factory.LoadTop1<Charge>(new ZQuery(JobChargeSchema.JR_AL_ARLine, Line.PK));
					fCharge = DocJobInvoicingJobCharge.New(tempCharge, Factory);
				}
				return fCharge;
			}
		}
		DocJobInvoicingJobCharge fCharge;

		Charge WrappedCharge
		{
			get
			{
				if (fWrappedCharge == null)
				{
					fWrappedCharge = (Charge)Charge?.WrappedObject;
				}
				return fWrappedCharge;
			}
		}
		Charge fWrappedCharge;

		#endregion

		#region Sub-Totalling

		public ZBool IsSubTotalLine
		{
			get { return false; }
		}

		public ZBool IsSpacerLine
		{
			get { return false; }
		}

		public ZBool IsCommentLine
		{
			get
			{
				return (ChargeCode != null && ChargeCode.ChargeType == "CMT");
			}
		}

		public ZBool IsRollUpLine
		{
			get { return false; }
		}

		public ZBool HasBeenSubTotalled
		{
			get { return fHasBeenSubTotalled; }
			set { fHasBeenSubTotalled = value; }
		}

		ZBool fHasBeenSubTotalled;

		public ZDecimal OSAmountForTotal { get { return IsCommentLine ? ZDecimal.Zero : OSAmount; } }

		public ZDecimal OSExTaxAmountForTotal { get { return IsCommentLine ? ZDecimal.Zero : OSExTaxAmount; } }

		public ZDecimal LineAmountForTotal { get { return IsCommentLine ? ZDecimal.Zero : LineAmount; } }

		public ZDecimal GSTVATForTotal { get { return IsCommentLine ? ZDecimal.Zero : GSTVAT; } }

		public ZDecimal LocalAmountAndTaxForTotal { get { return IsCommentLine ? ZDecimal.Zero : LocalAmountAndTax; } }

		public ZString TaxAmountDisplayForTotal { get { return IsCommentLine ? ZString.Empty : TaxAmountDisplay; } }

		public ZString OSTaxDisplayForTotal { get { return IsCommentLine ? ZString.Empty : OSTaxDisplay; } }

		public ZString TaxGroupCode => Line.VATClass?.A9_TaxGroupCode ?? TaxRate?.AccTaxRate.DefaultVatClass?.A9_TaxGroupCode ?? ZString.Empty;

		#endregion

		#region Implementation

		protected DocCurrency fHeaderCurrency;
		protected DocOrganisation fHeaderOrganisation;

		internal InvoicingLineBase Line
		{
			get { return (InvoicingLineBase)WrappedObject; }
		}

		#endregion

		#region SubInvoiceRef

		public ZString SubInvoiceRef
		{
			get
			{
				if (Line != null & Line.TransactionHeader != null)
				{
					return Line.TransactionHeader.AH_TransactionType + " " + Line.TransactionHeader.TransactionNumberPrefixed;
				}
				else
				{
					return "";
				}
			}
		}

		#endregion

		#region Shippers References

		ZString fShippersReference;
		public ZString ShippersReference
		{
			get { return GetShippersReference(); }
		}

		protected ZString GetShippersReference()
		{
			if (fShippersReference.IsEmpty)
			{
				if (IsForwardingJob)
				{
					ForwardingShipment decShipment = Factory.Load<ForwardingShipment>(Line.Job.JH_ParentID);
					if (decShipment != null)
					{
						fShippersReference = decShipment.JS_BookingReference;
					}
				}
			}
			return fShippersReference;
		}

		#endregion

		#region Other References

		ZString fOtherReference;
		public ZString OtherReference
		{
			get { return GetOtherReference(true); }
		}

		protected ZString GetOtherReference(ZBool addContainerDetails)
		{
			if (fOtherReference.IsEmpty)
			{
				if (IsLoadListJob)
				{
					fOtherReference = GetContainerNumber(LoadList.Containers);
					if (fOtherReference.IsEmpty && (!LoadList.BookingReference.IsEmpty || !LoadList.MasterBillNum.IsEmpty))
					{
						fOtherReference = LoadList.BookingReference + " / " + LoadList.MasterBillNum;
					}
				}
				else if (IsCFSShipmentJob && Shipment != null)
				{
					fOtherReference = GetContainerNumber(Shipment.Containers);
					if (fOtherReference.IsEmpty && (!Shipment.BookingReference.IsEmpty || !Shipment.MasterBillNum.IsEmpty))
					{
						fOtherReference = Shipment.BookingReference + " / " + Shipment.MasterBillNum;
					}
				}
				else if (IsLocalCartage)
				{
					fOtherReference = "";
					if (LocalTransport != null)
					{
						fOtherReference = LocalTransport.OrderReferenceNumber;
						if (addContainerDetails)
						{
							if (LocalTransport.IsContainerised && LocalTransport.HasContainers)
							{
								fOtherReference += fOtherReference.IsEmpty ? "" : ", ";
								fOtherReference += GetContainerNumber(LocalTransport.Containers);
							}
						}
					}
				}
				else if (IsCustomJob)
				{
					fOtherReference = "";
					BaseJobDeclaration declaration = Factory.Load(typeof(BaseJobDeclaration), JobHeader.ParentID) as BaseJobDeclaration;
					if (declaration != null)
					{
						fOtherReference = declaration.JE_OwnerRef;
						if (!declaration.OrderNumbers.IsEmpty)
						{
							fOtherReference += "," + declaration.OrderNumbers;
						}
						if (addContainerDetails)
						{
							foreach (BaseCusContainer container in declaration.CusContainers)
							{
								if (container.IsFullContainer)
								{
									fOtherReference += "," + container.CO_ContainerNumber + " ";
								}
							}
						}
					}
				}
				else if (IsForwardingJob && Shipment != null)
				{
					ForwardingShipment decShipment = Shipment.WrappedObject as ForwardingShipment;

					if (decShipment != null)
					{
						if (!decShipment.DocsAndCartage.JP_OrderItemsAsString.IsEmpty)
						{
							fOtherReference = decShipment.DocsAndCartage.JP_OrderItemsAsString;
						}
						else
						{
							foreach (Order order in decShipment.AttachedOrders)
							{
								fOtherReference += order.JD_OrderNumber + " ";
							}
						}
					}
					if (Shipment.DeclarationForShipmentBranch != null)
					{
						fOtherReference += Shipment.DeclarationForShipmentBranch.OwnerRef;
					}

					if ((Shipment.PackingMode == Constants.ContainerModes.FCL ||
						(Shipment.PackingMode == Constants.ContainerModes.BuyersConsol && decShipment.IsBuyersConsolLead)) &&
						addContainerDetails)
					{
						fOtherReference += "," + Shipment.ContainerNumbers;
					}
				}
			}
			return fOtherReference;
		}

		ZString GetContainerNumber(DocContainerCollection containers)
		{
			ZString containerNo = "";

			foreach (IDocContainer container in containers)
			{
				containerNo += container.ContainerNumber + ":" + container.ContainerType + " ";
			}

			return containerNo;
		}

		ZString GetContainerNumber(DocCommonContainerCollection containers)
		{
			ZString containerNo = "";

			foreach (DocCommonContainer container in containers)
			{
				containerNo += container.ContainerNumber + ":" + container.ContainerType + " ";
			}

			return containerNo;
		}

		ZString GetContainerNumber(IDocContainerCollection containers)
		{
			ZString containerNo = "";

			foreach (IDocContainer container in containers)
			{
				containerNo += container.ContainerNumber + ":" + container.ContainerType + " ";
			}

			return containerNo;
		}

		#endregion

		#region Client Reference

		ZString fClientReference;
		public ZString ClientReference
		{
			get
			{
				if (fClientReference.IsEmpty)
				{
					if (IsLoadListJob && LoadList != null)
					{
						fClientReference = LoadList.AgentsReference;
					}
					else if (Shipment != null)
					{
						fClientReference = Shipment.ConsolReference;
					}
				}

				return fClientReference;
			}
		}

		#endregion

		#region ContainerNumbers

		public ZString ContainerNumbers
		{
			get
			{
				if (fContainerNumbers.IsEmpty)
				{
					if (IsLoadListJob)
					{
						fContainerNumbers = GetContainerNumber(LoadList.Containers);
					}
					else if (IsForwardingJob)
					{
						fContainerNumbers = GetContainerNumber(Shipment.Containers);
					}
					else if (IsLocalCartage)
					{
						fContainerNumbers = GetContainerNumber(LocalTransport.Containers);
					}
					else if (IsCustomJob)
					{
						if (Declaration != null)
						{
							fContainerNumbers = Declaration.ContainerNumbers;
						}
					}
					else if (IsContainerDetention)
					{
						fContainerNumbers = string.Join(", ", OperationsJob.Containers.Cast<ContainerWrapper>().Select(x => x.ContainerNo));
					}
				}
				return fContainerNumbers;
			}
		}
		ZString fContainerNumbers;

		#endregion

		#region Customs

		public DocBaseJobDeclaration Declaration
		{
			get
			{
				DocBaseJobDeclaration result = null;
				if (JobHeader != null && JobHeader.ParentTableCode == "JE")
				{
					BaseJobDeclaration declaration = Factory.Load(typeof(BaseJobDeclaration), JobHeader.ParentID) as BaseJobDeclaration;
					if (declaration != null)
					{
						result = DocBaseJobDeclaration.New(declaration, Factory);
					}
				}

				return result;
			}
		}

		public DocBaseJobDeclaration Customs
		{
			get
			{
				DocBaseJobDeclaration result = null;

				if (IsCustomJob)
				{
					string sql = @"SELECT RN_Code as CountryCode ";
					sql += @"FROM dbo.JobDeclaration  INNER JOIN ";
					sql += @"GlbBranch  ON JE_GB = GB_PK INNER JOIN ";
					sql += @"GlbCompany  ON GB_GC = GC_PK INNER JOIN ";
					sql += @"RefCountry  ON GC_RN_NKCountryCode = RN_Code ";
					sql += @"WHERE JE_DeclarationReference = @DeclarationReference";

					ZSqlParameterCollection @params = new ZSqlParameterCollection();
					@params.Add("@DeclarationReference", StripConsolidatedInvoiceRefOfEndChars(), JobDeclarationSchema.JE_DeclarationReference);
					DynamicBusinessObjectCollection dynamicDeclaration = new DynamicBusinessObjectCollection(Factory);
					dynamicDeclaration.Load(sql, @params);

					if (dynamicDeclaration.Count >= 1)
					{
						ZString countryCode = new ZString(dynamicDeclaration[0]["CountryCode"]);
						if (countryCode == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
						{
							ZQuery loadDeclarationFilter = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, StripConsolidatedInvoiceRefOfEndChars());
							var declaration = Factory.LoadTop1<BaseJobDeclaration>(loadDeclarationFilter);

							if (declaration != null)
							{
								result = DocBaseJobDeclaration.New(declaration, Factory);
							}
						}
					}
				}

				return result;
			}
		}

		protected ZString StripConsolidatedInvoiceRefOfEndChars()
		{
			ZString result = Line.TransactionHeader.AH_ConsolidatedInvoiceRef;
			ZString[] splitConsolidatedInvoiceRef = result.Split('/');

			if (splitConsolidatedInvoiceRef.Length > 1)
			{
				result = splitConsolidatedInvoiceRef[0];
			}

			return result.Trim();
		}

		#endregion

		OperationsJobHelper OpJobHelper
		{
			get
			{
				if (jobHelper == null)
				{
					jobHelper = new OperationsJobHelper(JobHeader, Shipment, Charge, Factory);
				}
				return jobHelper;
			}
		}
		OperationsJobHelper jobHelper;

		#region LoadList

		public DocLoadListConsol LoadList
		{
			get
			{
				DocLoadListConsol result = null;
				if (IsLoadListJob)
				{
					var loadListConsol = Factory.Load<CFSLoadListConsol>(Line.Job.JH_ParentID);
					if (loadListConsol != null)
					{
						result = DocLoadListConsol.New(loadListConsol, Factory);
					}
				}
				return result;
			}
		}

		#endregion

		#region LocalTransport

		public DocCommonCartage LocalTransport
		{
			get
			{
				DocCommonCartage result = null;

				if (this.JobHeader != null)
				{
					if (this.JobHeader.ParentTableCode == "JJ")
					{
						CommonCartage cartage = Factory.Load(typeof(CommonCartage), this.JobHeader.ParentID) as CommonCartage;
						if (cartage != null)
						{
							result = DocCommonCartage.New(cartage, Factory);
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region IsLoadListJob

		public ZBool IsLoadListJob
		{
			get
			{
				return (ZBool)(JobHeader != null && JobHeader.ParentTableCode == JobConsolSchema.Constants.Prefix);
			}
		}

		#endregion

		#region IsCFSShipmentJob

		public ZBool IsCFSShipmentJob
		{
			get
			{
				return OpJobHelper.IsCFSShipmentJob;
			}
		}

		#endregion

		#region IsCustomJob

		public ZBool IsCustomJob
		{
			get
			{
				return OpJobHelper.IsCustomJob;
			}
		}

		#endregion

		#region IsNCTSJob

		public ZBool IsNCTSJob
		{
			get
			{
				return OpJobHelper.IsNCTSJob;
			}
		}

		#endregion

		#region IsForwardingJob

		public ZBool IsForwardingJob
		{
			get
			{
				return OpJobHelper.IsForwardingJob;
			}
		}

		#endregion

		#region IsAgencyShippingJob

		public ZBool IsAgencyShippingJob
		{
			get
			{
				return OpJobHelper.IsAgencyShipment;
			}
		}

		#endregion

		#region IsLocalCartage

		public ZBool IsLocalCartage
		{
			get
			{
				return OpJobHelper.IsLocalCartage;
			}
		}

		#endregion

		#region IsISFJob

		public ZBool IsISFJob
		{
			get
			{
				return OpJobHelper.IsISFJob;
			}
		}

		#endregion

		#region IsConsignmentJob

		public ZBool IsConsignmentJob
		{
			get
			{
				return OpJobHelper.IsConsignmentJob;
			}
		}

		#endregion

		#region IsSundryCharges

		public ZBool IsSundryCharges
		{
			get
			{
				return OpJobHelper.IsSundryCharges;
			}
		}

		#endregion

		#region IsVoyageAccounting

		public ZBool IsVoyageAccounting
		{
			get
			{
				return OpJobHelper.IsVoyageAccounting;
			}
		}

		#endregion

		#region IsContainerDetention

		public ZBool IsContainerDetention
		{
			get
			{
				return OpJobHelper.IsContainerDetention;
			}
		}

		#endregion

		#region IsWarehouseJob

		public ZBool IsWarehouseJob
		{
			get { return OpJobHelper.IsWarehouseJob; }
		}

		#endregion

		#region LayoutWhenPrintedInPeriodicInvoice
		public ZString LayoutWhenPrintedInPeriodicInvoice
		{
			get
			{
				ZString result = InvoiceTypeLayoutList.Codes.CHG;
				if (InvoiceType != null)
				{
					result = InvoiceType.PI_Type;
				}
				return result;
			}
		}
		#endregion

		#region SecondaryLayoutWhenPrintedInPeriodicInvoice
		public ZString SecondaryLayoutWhenPrintedInPeriodicInvoice
		{
			get
			{
				ZString result = InvoiceTypeLayoutList.Codes.CHG;
				if (InvoiceType != null)
				{
					result = InvoiceType.PI_SecondaryType;
				}
				return result;
			}
		}
		#endregion

		#region OSIntegratedGST
		public ZDecimal OSIntegratedGSTAmount
		{
			get
			{
				if (Line.InvoiceBase != null
					&& Line.TaxRate != null
					&& Line.TaxRate.AT_Type == AccTaxRate.Types.IntegratedGST
					&& Line.TaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India)
				{
					return (Line.InvoiceBase.AH_RX_NKTransactionCurrency == Line.InvoiceBase.AH_Calc_LocalRXCode ? Line.AL_LocalTaxAmount : Line.AL_OSTaxAmount) * AmountMultiplier;
				}

				return ZDecimal.Zero;
			}
		}

		#endregion

		#region OSCentreGST
		public ZDecimal OSCentreGSTAmount
		{
			get
			{
				if (Line.InvoiceBase != null
					&& Line.TaxRate != null
					&& Line.TaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.StateGST
					&& Line.TaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India)
				{
					return (Line.InvoiceBase.AH_RX_NKTransactionCurrency == Line.InvoiceBase.AH_Calc_LocalRXCode ? Line.AL_LocalTaxAmount - Line.AL_LocalExtraTaxAmount : Line.AL_OSTaxAmount - Line.AL_OSExtraTaxAmount) * AmountMultiplier;
				}

				return ZDecimal.Zero;
			}
		}

		#endregion

		#region OSStateGST
		public ZDecimal OSStateGSTAmount
		{
			get
			{
				if (Line.InvoiceBase != null
					&& Line.TaxRate != null
					&& Line.TaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.StateGST
					&& Line.TaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India)
				{
					return (Line.InvoiceBase.AH_RX_NKTransactionCurrency == Line.InvoiceBase.AH_Calc_LocalRXCode ? Line.AL_LocalExtraTaxAmount : Line.AL_OSExtraTaxAmount) * AmountMultiplier;
				}

				return ZDecimal.Zero;
			}
		}

		internal ZBool HasIndiaIntegratedOrStateGST
		{
			get
			{
				if (Line.InvoiceBase != null
					&& Line.TaxRate != null
					&& Line.TaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India
					&& (Line.TaxRate.AT_Type == AccTaxRate.Types.IntegratedGST
						|| Line.TaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.StateGST))
				{
					return true;
				}

				return false;
			}
		}

		#endregion

		ZString GetAsterisks()
		{
			if (Line.AL_A9_VATClass.IsValid && Invoice.TaxMessagesToAsterisksMapping.ContainsKey(Line.AL_A9_VATClass))
			{
				var mapping = Invoice.TaxMessagesToAsterisksMapping[Line.AL_A9_VATClass];
				if (mapping != null)
				{
					Asterisks = " " + mapping.Item1;
				}
			}
			return Asterisks;
		}
		ZString Asterisks = ZString.Empty;

		ZString GetNumber()
		{
			if (Line.AL_A9_VATClass.IsValid && Invoice.TaxMessagesToAsterisksMapping.ContainsKey(Line.AL_A9_VATClass))
			{
				var mapping = Invoice.TaxMessagesToAsterisksMapping[Line.AL_A9_VATClass];
				if (mapping != null)
				{
					AsterisksAsNumber = " " + mapping.Item4.ToString() + ".";
				}
			}
			return AsterisksAsNumber;
		}
		ZString AsterisksAsNumber = ZString.Empty;

		bool IDocLine.PreventGrouping => PreventGrouping;

		#region ISortableDocLine Members

		int ISortableDocLine.OrgLevelSortOrder
		{
			get
			{
				int result = 0;
				DocOrganisation org = Organisation ?? Invoice.Organisation;
				if (org != null)
				{
					foreach (AccClientInvoiceOrder invoiceOrder in org.InvoiceOrders)
					{
						if ((invoiceOrder.AI_InvoiceType == Invoice.TransactionCategory || invoiceOrder.AI_InvoiceType == "ALL" || invoiceOrder.AI_InvoiceType.IsEmpty) &&
							ChargeCode != null && invoiceOrder.AI_AC == ChargeCode.ChargeCodePK)
						{
							result = invoiceOrder.AI_PrintOrder;
							break;
						}
					}
				}
				return result;
			}
		}

		int ISortableDocLine.ChargePrintSeqSortOrder
		{
			get { return ChargeCode != null ? (int)ChargeCode.Sequence : 0; }
		}

		int ISortableDocLine.UserEnteredSortOrder
		{
			get { return Sequence; }
		}

		string ISortableDocLine.AlphabeticalSortOrder
		{
			get { return ChargeCode != null ? ChargeCode.Code : (GLAccount != null ? GLAccount.AccountNumber : ZString.Empty); }
		}

		#endregion

		#region Fixed Place of Supply

		public ZString FixedPlaceOfSupplyLabel
		{
			get
			{
				var result = ZString.Empty;

				if (Line.AL_PlaceOfSupplyType == PlaceOfSupplyTypes.State.Code)
				{
					result = Res.GetString("4E19B55C-2C6A-4389-B0AB-A2EA587493CF", "State of Supply");
				}
				else if (Line.AL_PlaceOfSupplyType == PlaceOfSupplyTypes.PredefinedRule.Code)
				{
					result = Res.GetString("CA739D1F-7533-4C69-A546-E417AB325189", "Place of Supply");
				}
				return result;
			}
		}

		public ZString FixedPlaceOfSupply
		{
			get
			{
				var result = ZString.Empty;
				if (Line.AL_PlaceOfSupplyType == PlaceOfSupplyTypes.State.Code)
				{
					result = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(Line.Company).GetDescriptionFromCode(Line.AL_PlaceOfSupply) ?? ZString.Empty;
				}
				else if (Line.AL_PlaceOfSupplyType == PlaceOfSupplyTypes.PredefinedRule.Code)
				{
					if (Line.AL_PlaceOfSupply == PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry)
					{
						result = Res.GetString("96F33FA8-7FEC-49FD-8EA1-B5BE51A78F93", "Foreign Country/Region");
					}
					else if (Line.AL_PlaceOfSupply == PlaceOfSupplyListProvider.Codes.OtherTerritories)
					{
						result = Res.GetString("c42498e4-25cf-4c10-b7e1-bf31ade287cf", "Other Territories");
					}
				}
				return result;
			}
		}

		#endregion

		#region Print Layout
		OrgInvoiceType InvoiceType
		{
			get
			{
				if (invoiceType == null)
				{
					var debtor = Line.InvoiceBase.Header;
					var job = Line.InvoicingJob;

					if (debtor != null && debtor.CompanyData != null)
					{
						Dictionary<ZString, OrgInvoiceType> invoiceTypes;

						if (job != null)
						{
							try
							{
								invoiceTypes = debtor.CompanyData.GetApplicableInvoiceTypes(job.TransportMode, job.ServiceDirection, job.ServiceLevel, true, job.JobType.Code);
							}
							catch (NullReferenceException) when (job.PlugInData?.InvoicingSupporter == null)
							{
								throw new DataProviderException(Res.GetString("75b7e7e4-0656-4476-8d8b-5dd7d2aba6d3", @"Invoice number {0} cannot be printed because a parent is not available for the {1} job.
You could try to retrieve an original printed invoice document from the invoice’s eDocs tab.
Use the 'Missing/Invalid Job Parent' filter in the Job Management module to list all jobs without a valid parent.", Invoice.InvoiceNumber, job.JH_JobNum));
							}
						}
						else
						{
							invoiceTypes = debtor.CompanyData.GetApplicableInvoiceTypes(ZString.Empty, ZString.Empty, ZString.Empty, true, InvoiceTypeModuleList.Codes.MSC);
						}

						if (invoiceTypes != null && invoiceTypes.Count > 0)
						{
							invoiceType = invoiceTypes.Values.First();
						}
					}
				}
				return invoiceType;
			}
		}
		OrgInvoiceType invoiceType;
		#endregion

		#region Government Reporting

		public ZString GovernmentReportingCode => Line.AL_GovtChargeCode;

		public ZString GovernmentReportingCodeHeading
		{
			get
			{
				if (!AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
				{
					return ZString.Empty;
				}

				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.India)
				{
					return IsGoods ? "HSN" : "SAC";
				}
				else
				{
					return IsGoods ? GoodServiceTypes.Codes.GDS : GoodServiceTypes.Codes.SRV;
				}
			}
		}

		bool IsGoods => Line.ChargeCode != null && Line.ChargeCode.AC_GoodsServiceType == GoodServiceTypes.Codes.GDS;

		#endregion

		#region Quantity

		public ZString Quantity =>
			!LineDescription.IsEmpty && !IsCommentLine && Invoice.PrintQuantity ? "1" : string.Empty;

		#endregion

		#region DisplayTaxGroupCode

		public ZBool DisplayTaxGroupCode =>
			GlbCompany.CurrentCompany.IsInTaxCoreSupportedCountry() && (Invoice?.Invoice?.IsApprovedByGovt ?? false);

		#endregion
	}
}

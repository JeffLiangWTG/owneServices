using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public abstract class DocBaseJobComInvoiceLine : DocBaseWrapper, Integration.DocumentWrappers.IDocBaseJobComInvoiceLine
	{
		#region Static New with Type Decider Built In
		public static DocBaseJobComInvoiceLine New(BaseJobComInvoiceLine invoiceLine, BusinessObjectFactory factoryToWrap)
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia)
			{
				return AU.DocJobComInvoiceLine.New((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)invoiceLine, factoryToWrap);
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand)
			{
				return NZ.DocJobComInvoiceLine.New((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)invoiceLine, factoryToWrap);
			}
#if DEBUG
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes._TemplateCountryName_)
			{
				return (DocBaseJobComInvoiceLine)ObjectFactory.Get<Integration.Customs._CustomsTemplate_.IDocumentWrapperProvider>().NewDocJobComInvoiceLine((Integration.Customs._CustomsTemplate_.IJobComInvoiceLine)invoiceLine, factoryToWrap);
			}
#endif
			else
			{
				return General.DocJobComInvoiceLine.New(invoiceLine, factoryToWrap);
			}
		}
		#endregion

		protected DocBaseJobComInvoiceLine(BaseJobComInvoiceLine baseJobComInvoiceLine, BusinessObjectFactory factoryToWrap)
			: base(baseJobComInvoiceLine, factoryToWrap)
		{
		}

		public override string ToString()
		{
			return OrderNumber;
		}

		#region Abstract

		protected abstract DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(BaseJobComInvoiceHeader invoiceToWrap);
		protected abstract DocBaseCusEntryLine CreateCusEntryLine(CusEntryLine entryLineToWrap);

		#endregion

		#region Virtual

		public virtual ZBool Texco
		{
			get { return ZBool.False; }
		}

		public virtual ZBool Drawback
		{
			get { return ZBool.False; }
		}

		public virtual ZString PermitNumber
		{
			get { return ZString.Empty; }
		}

		public virtual ZString TempImportNum
		{
			get { return ZString.Empty; }
		}

		public virtual ZString PermitNumber1
		{
			get { return ZString.Empty; }
		}

		public virtual ZString PermitNumber2
		{
			get { return ZString.Empty; }
		}

		public virtual ZString PermitNumber3
		{
			get { return ZString.Empty; }
		}

		public virtual ZBool MotorVehiclePlan
		{
			get { return ZBool.False; }
		}

		public ZString TariffInformation
		{
			get { return BaseJobComInvoiceLine.ClassificationDetailsForGenericWrapper; }
		}

		public virtual ZDecimal DutyAmountForInvoiceReport
		{
			get { return DutyAmount; }
		}

		public virtual ZDecimal EffectiveDuty
		{
			get { return 0M; }
		}

		protected virtual ZDecimal VOTICore
		{
			get { return 0M; }
		}

		public ZDecimal VOTI
		{
			get { return VOTICore; }
		}

		#endregion

		#region ZString Fields

		public ZString CustomsValueCurrencyName
		{
			get { return BaseJobComInvoiceLine.InvoiceHeader.Invoice_Currency.RX_UnitNameMultilingual; }
		}

		public ZString Stat1Unit
		{
			get { return BaseJobComInvoiceLine.JI_CustomsUnitQty; }
		}

		public ZString CountryOfOriginCode
		{
			get { return CountryOfOriginCodeCore; }
		}

		public ZString FormattedInvoiceQuantity
		{
			get { return FormatNumberToMinDecimals(InvoiceQuantity, 2); }
		}

		public ZString FormattedLinePrice
		{
			get { return FormatNumberToMinDecimals(LinePrice, 2); }
		}

		public ZString FormattedUnitPrice
		{
			get { return FormatNumberToMinDecimals(UnitPrice, 2); }
		}

		public ZString CustomsQty
		{
			get
			{
				ZString result = ZString.Empty;
				if (CustomsQuantity > 0)
				{
					result = FormatNumber(CustomsQuantity, 2);
				}
				return result;
			}
		}

		public ZString InvoiceQty
		{
			get
			{
				ZString result = ZString.Empty;
				if (InvoiceQuantity > 0)
				{
					result = FormatNumber(InvoiceQuantity, 2);
				}
				return result;
			}
		}

		public ZString BondedWarehouseQty
		{
			get
			{
				ZString result = ZString.Empty;
				if (BondedWarehouseQuantity > 0)
				{
					result = FormatNumber(BondedWarehouseQuantity, 2);
				}
				return result;
			}
		}

		public ZString AmountAsString
		{
			get { return LinePrice.ToString(2) + "  "; }
		}

		public ZString UnitPriceAsString
		{
			get { return UnitPrice.ToString(3) + "  "; }
		}

		public ZString Invoice
		{
			get { return BaseJobComInvoiceLine.JI_Calc_Invoice; }
		}

		public ZString AddInfo
		{
			get { return BaseJobComInvoiceLine.JI_AddInfo; }
		}

		public ZString ConcessionOrder
		{
			get { return BaseJobComInvoiceLine.JI_ConcessionOrder; }
		}

		public ZString CustomAttrib1
		{
			get { return BaseJobComInvoiceLine.JI_CustomAttrib1; }
		}

		public ZString CustomAttrib2
		{
			get { return BaseJobComInvoiceLine.JI_CustomAttrib2; }
		}

		public ZString CustomAttrib3
		{
			get { return BaseJobComInvoiceLine.JI_CustomAttrib3; }
		}

		public ZString CustomAttrib4
		{
			get { return BaseJobComInvoiceLine.JI_CustomAttrib4; }
		}

		public ZString CustomsUnitQty
		{
			get { return BaseJobComInvoiceLine.JI_CustomsUnitQty; }
		}

		public ZString CustomsQtyOne
		{
			get { return BaseJobComInvoiceLine.JI_CustomAttrib1; }
		}

		public ZString Description
		{
			get { return BaseJobComInvoiceLine.JI_Description; }
		}

		public ZString OrderNumber
		{
			get { return BaseJobComInvoiceLine.JI_OrderNumber; }
		}

		public ZString MergedLineNumber
		{
			get { return BaseJobComInvoiceLine.MergedLineNumber; }
		}

		public ZString PartAttrib1
		{
			get { return BaseJobComInvoiceLine.JI_PartAttrib1; }
		}

		public ZString PartAttrib2
		{
			get { return BaseJobComInvoiceLine.JI_PartAttrib2; }
		}

		public ZString PartAttrib3
		{
			get { return BaseJobComInvoiceLine.JI_PartAttrib3; }
		}

		public ZString SerialNumber
		{
			get { return BaseJobComInvoiceLine.JI_SerialNumber; }
		}

		public ZString PartNo
		{
			get { return BaseJobComInvoiceLine.JI_PartNo; }
		}

		public ZString Tariff
		{
			get { return BaseJobComInvoiceLine.JI_Tariff; }
		}

		public ZString TariffLookup
		{
			get { return BaseJobComInvoiceLine.Classification != null ? BaseJobComInvoiceLine.Classification.CC_Description : ZString.Empty; }
		}

		public ZString TariffLookupCode
		{
			get { return BaseJobComInvoiceLine.Classification != null ? BaseJobComInvoiceLine.Classification.CC_LookupCode : ZString.Empty; }
		}

		public ZString VolumeUQ
		{
			get { return BaseJobComInvoiceLine.JI_VolumeUQ; }
		}

		public ZString WeightUQ
		{
			get { return BaseJobComInvoiceLine.JI_WeightUQ; }
		}

		public ZString InvoiceUQ
		{
			get { return BaseJobComInvoiceLine.JI_InvoiceUQ; }
		}

		public ZString BondedWarehouseUQ
		{
			get { return BaseJobComInvoiceLine.JI_BondedWhsUnitQty; }
		}

		public ZString OriginCode
		{
			get { return CountryOfOriginCodeCore; }
		}

		protected virtual ZString CountryOfOriginCodeCore
		{
			get { return BaseJobComInvoiceLine.JI_CountryOfOrigin; }
		}

		public ZString LinePriceCurrencyCode
		{
			get
			{
				return LinePriceCurr?.Code ?? ZString.Empty;
			}
		}

		public virtual ZString MergedLineNo
		{
			get { return BaseJobComInvoiceLine.JI_Calc_MergedLineNumber; }
		}

		public virtual ZShort MergedNumericLineNo
		{
			get
			{
				ZShort result;
				var b3LineNo = BaseJobComInvoiceLine.JI_Calc_MergedLineNumber;
				return ZShort.TryParse(b3LineNo, out result) ? result : ZShort.Zero;
			}
		}

		public ZString RefCountryCode
		{
			get { return RefCountryCodeCore; }
		}

		protected virtual ZString RefCountryCodeCore
		{
			get { return BaseJobComInvoiceLine.EffectiveCountryOfOrigin; }
		}

		public ZString ClassificationDetails
		{
			get { return ClassificationDetailsCore; }
		}

		protected virtual ZString ClassificationDetailsCore
		{
			get { return BaseJobComInvoiceLine.ClassificationDetailsForGenericWrapper; }
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal Stat1Qty
		{
			get { return BaseJobComInvoiceLine.JI_CustomsQuantity; }
		}

		public ZDecimal FOB
		{
			get { return BaseJobComInvoiceLine.JI_Calc_FOB; }
		}

		public ZDecimal FOBInLocalCurrency
		{
			get { return BaseJobComInvoiceLine.JI_Calc_FOB_InLocalCurrency; }
		}

		public ZDecimal CIF
		{
			get { return BaseJobComInvoiceLine.JI_Calc_CIF; }
		}

		public ZDecimal InsuranceInInvoiceCurr
		{
			get { return BaseJobComInvoiceLine.JI_Calc_InsuranceInInvoiceCurr; }
		}

		public ZDecimal FreightInInvoiceCurr
		{
			get { return BaseJobComInvoiceLine.JI_Calc_FreightInInvoiceCurr; }
		}

		public ZDecimal LinesTotal
		{
			get { return BaseJobComInvoiceLine.JI_Calc_LinesTotal; }
		}

		public ZDecimal LinesEntered
		{
			get { return BaseJobComInvoiceLine.JI_Calc_LinesEntered; }
		}

		public ZDecimal Balance
		{
			get { return BaseJobComInvoiceLine.JI_Calc_Balance; }
		}

		public ZDecimal DutyAmount
		{
			get { return BaseJobComInvoiceLine.JI_Calc_DutyAmount; }
		}

		public ZDecimal GSTVATAmount
		{
			get { return BaseJobComInvoiceLine.JI_Calc_GSTVATAmount; }
		}

		public ZDecimal UnitPrice
		{
			get { return BaseJobComInvoiceLine.UnitPrice; }
		}

		public ZDecimal UnitPriceInLocalCurrency
		{
			get { return UnitPriceInLocalCurrencyCore; }
		}

		protected virtual ZDecimal UnitPriceInLocalCurrencyCore
		{
			get { return BaseJobComInvoiceLine.UnitPriceInLocalCurrency; }
		}

		public ZDecimal CustomDecimal1
		{
			get { return BaseJobComInvoiceLine.JI_CustomDecimal1; }
		}

		public ZDecimal CustomDecimal2
		{
			get { return BaseJobComInvoiceLine.JI_CustomDecimal2; }
		}

		public ZDecimal CustomDecimal3
		{
			get { return BaseJobComInvoiceLine.JI_CustomDecimal3; }
		}

		public ZDecimal CustomsQuantity
		{
			get { return BaseJobComInvoiceLine.JI_CustomsQuantity; }
		}

		public ZDecimal LinePrice
		{
			get { return BaseJobComInvoiceLine.JI_LinePrice; }
		}

		public ZDecimal LinePriceInLocalCurrency
		{
			get { return LinePriceInLocalCurrencyCore; }
		}

		protected virtual ZDecimal LinePriceInLocalCurrencyCore
		{
			get { return BaseJobComInvoiceLine.JI_LinePriceInLocalCurrency; }
		}

		public ZDecimal Volume
		{
			get { return BaseJobComInvoiceLine.JI_Volume; }
		}

		public ZDecimal Weight
		{
			get { return BaseJobComInvoiceLine.JI_Weight; }
		}

		public ZDecimal InvoiceQuantity
		{
			get { return BaseJobComInvoiceLine.JI_InvoiceQuantity; }
		}

		public ZDecimal BondedWarehouseQuantity => BaseJobComInvoiceLine.JI_BondedWhsQuantity;

		public ZDecimal GSTRate
		{
			get { return BaseJobComInvoiceLine.GSTRate; }
		}

		public ZDecimal DutyPercent
		{
			get { return BaseJobComInvoiceLine.CusEntryLine == null ? new ZDecimal(0m) : BaseJobComInvoiceLine.CusEntryLine.CL_DutyPercent; }
		}

		public ZString DutyRateDescription
		{
			get { return BaseJobComInvoiceLine.CusEntryLine == null ? ZString.Empty : BaseJobComInvoiceLine.CusEntryLine.DutyRateDescription; }
		}

		public ZDecimal FlatDutyPortion
		{
			get { return FlatDutyPortionCore; }
		}

		protected virtual ZDecimal FlatDutyPortionCore
		{
			get { return 0m; }
		}

		public ZDecimal CustomsValue
		{
			get { return BaseJobComInvoiceLine.JI_CustomsValue; }
		}

		#endregion

		#region ZDateTime Fields

		public ZDateTime CustomDate1
		{
			get { return BaseJobComInvoiceLine.JI_CustomDate1; }
		}

		public ZDateTime CustomDate2
		{
			get { return BaseJobComInvoiceLine.JI_CustomDate2; }
		}

		public ZDateTime CustomDate3
		{
			get { return BaseJobComInvoiceLine.JI_CustomDate3; }
		}

		#endregion

		#region Wrapper Fields

		public DocBaseJobComInvoiceHeader BaseComInvoiceHeader
		{
			get { return InvoiceHeaderInternal; }
		}

		public DocCurrency LocalCurr
		{
			get { return DocCurrency.New(GlbCompany.CurrentCompany.LocalCurrency, Factory); }
		}

		public DocBaseCusEntryLine RateEntryLine
		{
			get { return CusEntryLineInternal; }
		}

		public DocCurrency LinePriceCurr
		{
			get { return DocCurrency.New(BaseJobComInvoiceLine.LinePriceRefCurrency, Factory); }
		}

		public DocOrderLine OrderLine
		{
			get { return BaseJobComInvoiceLine.JI_JO.IsValid ? DocOrderLine.New(BaseJobComInvoiceLine.Factory, BaseJobComInvoiceLine.JI_JO) : null; }
		}

		public DocOrgSupplierPart SupplierPart
		{
			get { return DocOrgSupplierPart.New(BaseJobComInvoiceLine.SupplierPart, Factory); }
		}

		public DocCountry Origin
		{
			get { return CountryOfOriginCore; }
		}

		public DocCountry CountryOfOrigin
		{
			get { return CountryOfOriginCore; }
		}

		protected virtual DocCountry CountryOfOriginCore
		{
			get { return DocCountry.New(BaseJobComInvoiceLine.CountryOfOrigin, Factory); }
		}

		#endregion

		#region ZBool Fields

		public ZBool CustomFlag1
		{
			get { return BaseJobComInvoiceLine.JI_CustomFlag1; }
		}

		public ZBool CustomFlag2
		{
			get { return BaseJobComInvoiceLine.JI_CustomFlag2; }
		}

		public ZBool CustomFlag3
		{
			get { return BaseJobComInvoiceLine.JI_CustomFlag3; }
		}

		#endregion

		#region ZShort Fields

		public ZShort LineNo
		{
			get { return BaseJobComInvoiceLine.JI_LineNo; }
		}

		public ZShort ParentLine
		{
			get { return BaseJobComInvoiceLine.JI_ParentLine; }
		}

		#endregion

		#region Implementation

		public DocBaseJobComInvoiceHeader BaseInvoiceHader
		{
			get { return InvoiceHeaderInternal; }
		}

		protected DocBaseJobComInvoiceHeader InvoiceHeaderInternal
		{
			get { return CreateJobComInvoiceHeader(BaseJobComInvoiceLine.InvoiceHeader); }
		}

		protected DocBaseCusEntryLine CusEntryLineInternal
		{
			get { return CreateCusEntryLine(BaseJobComInvoiceLine.CusEntryLine); }
		}

		BaseJobComInvoiceLine BaseJobComInvoiceLine
		{
			get { return (BaseJobComInvoiceLine)WrappedObject; }
		}

		/// <summary>
		/// This method returns number formatted to string to at least the specified decimal places.
		/// If the number has more than the specified decimal places it will return it as it is.
		/// Eg. 1.23456 with MinDecimalPlaces 2 will be returned as it is
		/// 1.234000 with MinDecimalPlaces 2 will be returned as 1.234
		/// 1.2 with MinDecimalPlaces 2 will be returned as 1.20
		/// 1 with MinDecimalPlaces 2 will be returned as 1.00
		/// </summary>
		/// <param name="value">The number you want to format</param>
		/// <param name="minDecimalPlaces">The number you want to format</param>
		protected ZString FormatNumberToMinDecimals(ZDecimal value, ZInt minDecimalPlaces)
		{
			ZString result = value.ToString();

			if (result.Contains(DecimalSeparator))
			{
				result = result.TrimEnd('0');
				if (result.EndsWith(DecimalSeparator))
				{
					result = value.ToString(2);
				}
				else
				{
					ZString decimals = result.Split(DecimalSeparator.ToCharArray())[1];
					ZString number = result.Split(DecimalSeparator.ToCharArray())[0];
					result = number + DecimalSeparator + decimals.PadRight(2, '0');
				}
			}
			else
			{
				result = value.ToString(2);
			}

			return result;
		}

		#endregion
	}
}

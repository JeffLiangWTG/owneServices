using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("LineNo")]
	public class CommercialInvoiceLineWrapper : GenericWrapper
	{
		public CommercialInvoiceLineWrapper(BaseJobComInvoiceLine invoiceLine, BusinessObjectFactory factory)
			: base(invoiceLine, factory)
		{
			InvoiceLineBO = invoiceLine ?? factory.GetNull<BaseJobComInvoiceLine>();
		}

		public OrgAddress ExporterAddress => InvoiceLineBO.ExporterAddress;
		public OrgAddress ManufacturerAddress => InvoiceLineBO.ManufacturerAddress;
		public OrgAddress ConsigneeAddress => InvoiceLineBO.ConsigneeAddressForDocument;

		public CommercialInvoiceWrapper Invoice
		{
			get { return InvoiceLineBO == null ? null : new CommercialInvoiceWrapper(InvoiceLineBO.InvoiceHeader, Factory); }
		}

		public CountryWrapper CountryOfOrigin
		{
			get { return new CountryWrapper(InvoiceLineBO.EffectiveCountryOfOrigin, Factory); }
		}

		public MoneyWrapper LinePrice
		{
			get { return new MoneyWrapper(GetValueWithCurrencyFromHeader(InvoiceLineBO.JI_LinePrice), Factory); }
		}

		public virtual MoneyWrapper UnitPrice
		{
			get { return new MoneyWrapper(GetValueWithCurrencyFromHeader(InvoiceLineBO.UnitPrice), Factory); }
		}

		public ValueAndUnitWrapper CustomsQuantity
		{
			get
			{
				var decimalPlaces = InvoiceLineBO.InvoiceHeader != null ? InvoiceLineBO.InvoiceHeader.GetInvoiceLineMaxDecimalPlaces(JobComInvoiceLineSchema.JI_CustomsQuantity.Name) : 3;
				return new ValueAndUnitWrapper(InvoiceLineBO.JI_CustomsQuantity, InvoiceLineBO.JI_CustomsUnitQty, decimalPlaces, InvoiceLineBO.Lookups.CustomsUQList, Factory);
			}
		}

		public ValueAndUnitWrapper LineQuantity
		{
			get
			{
				var decimalPlaces = InvoiceLineBO.InvoiceHeader != null ? InvoiceLineBO.InvoiceHeader.GetInvoiceLineMaxDecimalPlaces(JobComInvoiceLineSchema.JI_InvoiceQuantity.Name) : 2;
				return new ValueAndUnitWrapper(InvoiceLineBO.JI_InvoiceQuantity, InvoiceLineBO.JI_InvoiceUQ, decimalPlaces, InvoiceLineBO.Lookups.InvoiceUQList, Factory);
			}
		}

		public ValueAndUnitWrapper Volume
		{
			get
			{
				var decimalPlaces = InvoiceLineBO.InvoiceHeader != null ? InvoiceLineBO.InvoiceHeader.GetInvoiceLineMaxDecimalPlaces(JobComInvoiceLineSchema.JI_Volume.Name) : 3;
				return new ValueAndUnitWrapper(InvoiceLineBO.JI_Volume, InvoiceLineBO.JI_VolumeUQ, decimalPlaces, InvoiceLineBO.Lookups.VolumeUQList, Factory);
			}
		}

		public ValueAndUnitWrapper Weight
		{
			get
			{
				var decimalPlaces = InvoiceLineBO.InvoiceHeader != null ? InvoiceLineBO.InvoiceHeader.GetInvoiceLineMaxDecimalPlaces(JobComInvoiceLineSchema.JI_Weight.Name) : 1;
				return new ValueAndUnitWrapper(InvoiceLineBO.JI_Weight, InvoiceLineBO.JI_WeightUQ, decimalPlaces, InvoiceLineBO.Lookups.WeightUQList, Factory);
			}
		}

		public ValueAndUnitWrapper NetWeight
		{
			get
			{
				var decimalPlaces = InvoiceLineBO.InvoiceHeader != null ? InvoiceLineBO.InvoiceHeader.GetInvoiceLineMaxDecimalPlaces(JobComInvoiceLineSchema.JI_NetWeight.Name) : 1;
				return new ValueAndUnitWrapper(InvoiceLineBO.JI_NetWeight, InvoiceLineBO.JI_NetWeightUQ, decimalPlaces, InvoiceLineBO.Lookups.WeightUQList, Factory);
			}
		}

		public ZString ConcessionCode
		{
			get { return InvoiceLineBO.JI_ConcessionOrder; }
		}

		public ZString Description
		{
			get
			{
				var result = new ZStringBuilder();
				result.AppendIfNotEmpty(InvoiceLineBO.JI_Description);
				result.AppendIfNotEmpty(InvoiceLineBO.JI_NDescription);
				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString FormattedDescription
		{
			get
			{
				if (InvoiceLineBO.IsExport)
				{
					return Description;
				}
				else
				{
					var result = new ZStringBuilder();
					result.AppendIfNotEmpty(Description);
					if (!InvoiceLineBO.EffectiveCountryOfOrigin.IsEmpty)
					{
						result.Append(Res.GetString("4A0DBB82-D6D6-4E1D-9CA6-83E19FA91DA2", "C/O:{0}", InvoiceLineBO.EffectiveCountryOfOrigin));
					}
					if (!InvoiceLineBO.JI_Tariff.IsEmpty)
					{
						result.Append(Res.GetString("f5999295-86a3-4b40-bb82-301674f9f143", "HS:{0}", InvoiceLineBO.JI_Tariff));
					}
					return result.ToStringWithDelimiterBetweenAppends(";");
				}
			}
		}

		public ZInt LineNo
		{
			get { return InvoiceLineBO.JI_LineNo; }
		}

		public ZString LookupCode
		{
			get
			{
				BaseCusClassification classification = InvoiceLineBO.Classification;
				return classification == null ? ZString.Empty : classification.CC_LookupCode;
			}
		}

		public ZString TariffCode
		{
			get { return InvoiceLineBO.JI_Tariff; }
		}

		public ZString ClassificationDetails
		{
			get { return InvoiceLineBO.ClassificationDetailsForGenericWrapper; }
		}

		public ZString PartNumber
		{
			get { return InvoiceLineBO.JI_PartNo; }
		}

		public ZString OrderNumber
		{
			get { return InvoiceLineBO.JI_OrderNumber; }
		}

		public ZDecimal DutyAmount
		{
			get { return InvoiceLineBO.JI_Calc_DutyAmount; }
		}

		public ZString DutyAmountsAsString
		{
			get { return InvoiceLineBO.DutyAmountsAsString; }
		}

		public ZString DutyRateDescription
		{
			get { return InvoiceLineBO.CusEntryLine == null ? ZString.Empty : InvoiceLineBO.CusEntryLine.DutyRateDescription; }
		}

		public ZString ProvProgTariff
		{
			get { return InvoiceLineBO.ProvProgTariff; }
		}

		public ZString ProvProgDutyRate
		{
			get { return InvoiceLineBO.ProvProgDutyRate; }
		}

		public ZString ProvProgCustomsQty
		{
			get { return InvoiceLineBO.ProvProgCustomsQty; }
		}

		public ZString ProvProgTariff1
		{
			get { return InvoiceLineBO.ProvProgTariff1; }
		}

		public ZString ProvProgDutyRate1
		{
			get { return InvoiceLineBO.ProvProgDutyRate1; }
		}

		public ZString ProvProgCustomsQty1
		{
			get { return InvoiceLineBO.ProvProgCustomsQty1; }
		}

		public ZString ProvProgTariff2
		{
			get { return InvoiceLineBO.ProvProgTariff2; }
		}

		public ZString ProvProgDutyRate2
		{
			get { return InvoiceLineBO.ProvProgDutyRate2; }
		}

		public ZString ProvProgCustomsQty2
		{
			get { return InvoiceLineBO.ProvProgCustomsQty2; }
		}

		public ZString ProvProgTariff3
		{
			get { return InvoiceLineBO.ProvProgTariff3; }
		}

		public ZString ProvProgDutyRate3
		{
			get { return InvoiceLineBO.ProvProgDutyRate3; }
		}

		public ZString ProvProgCustomsQty3
		{
			get { return InvoiceLineBO.ProvProgCustomsQty3; }
		}

		public ZString ProvProgTariff4
		{
			get { return InvoiceLineBO.ProvProgTariff4; }
		}

		public ZString ProvProgDutyRate4
		{
			get { return InvoiceLineBO.ProvProgDutyRate4; }
		}

		public ZString ProvProgCustomsQty4
		{
			get { return InvoiceLineBO.ProvProgCustomsQty4; }
		}

		public ZString ProvProgTariff5
		{
			get { return InvoiceLineBO.ProvProgTariff5; }
		}

		public ZString ProvProgDutyRate5
		{
			get { return InvoiceLineBO.ProvProgDutyRate5; }
		}

		public ZString ProvProgCustomsQty5
		{
			get { return InvoiceLineBO.ProvProgCustomsQty5; }
		}

		public ZString MergedLineNumber
		{
			get { return InvoiceLineBO.MergedLineNumber; }
		}

		public ZDecimal GSTRate
		{
			get { return InvoiceLineBO.GSTRate; }
		}

		public ZString RefCountryCode
		{
			get { return InvoiceLineBO.EffectiveCountryOfOrigin; }
		}

		#region Custom Attributes

		public ZString CustomAttrib1
		{
			get { return InvoiceLineBO.JI_CustomAttrib1; }
		}

		public ZString CustomAttrib2
		{
			get { return InvoiceLineBO.JI_CustomAttrib2; }
		}

		public ZString CustomAttrib3
		{
			get { return InvoiceLineBO.JI_CustomAttrib3; }
		}

		public ZString CustomAttrib4
		{
			get { return InvoiceLineBO.JI_CustomAttrib4; }
		}

		public ZString CustomAttrib5
		{
			get { return InvoiceLineBO.JI_CustomAttrib5; }
		}

		public ZString CustomAttrib6
		{
			get { return InvoiceLineBO.JI_CustomAttrib6; }
		}

		#endregion

		#region Implementation
		readonly BaseJobComInvoiceLine InvoiceLineBO;

		protected Money GetValueWithCurrencyFromHeader(ZDecimal value)
		{
			BaseJobComInvoiceHeader invoiceHeader = InvoiceLineBO.InvoiceHeader;
			RefCurrency currency = invoiceHeader == null ? null : invoiceHeader.Invoice_Currency;
			return new Money(value, currency ?? GlbCompany.CurrentCompany.Country.LocalCurrency);
		}
		#endregion
	}
}

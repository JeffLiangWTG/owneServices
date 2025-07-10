using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;

namespace Enterprise.Customs.IN.Business;

public class NonStandardExchangeRate : CusSupportingInfo
{
	public NonStandardExchangeRate(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : CusSupportingInfo.Schema
	{
		public new const int CSI_DescriptionMaxLength = 35;
		public new const int CSI_ReferenceNumberMaxLength = 20;
	}

	[ReadOnly(true)]
	[ResourceStringData("Enterprise.Customs.IN.Business.NonStandardExchangeRate|CSI_RN_NKCountryCode", Caption = "Currency")]
	public override ZString CSI_RX_NKCurrency
	{
		get => base.CSI_RX_NKCurrency;
		set
		{
			base.CSI_RX_NKCurrency = value;
			InvoicesWithPresentCurrency?.ForEach(x => x.MarkAsNeedingValidation());
		}
	}

	[MaxLength(Schema.CSI_DescriptionMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.NonStandardExchangeRate|CSI_Description", Caption = "Bank Name")]
	public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.NonStandardExchangeRate|CSI_EffectiveDate", Caption = "Effective Date")]
	public override ZDateTimeOffset CSI_EffectiveDate { get => base.CSI_EffectiveDate; set => base.CSI_EffectiveDate = value; }

	[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.NonStandardExchangeRate|CSI_ReferenceNumber", Caption = "Certificate Number")]
	public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.NonStandardExchangeRate|CSI_DateOfIssue", Caption = "Certificate Date")]
	public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }

	[DecimalPlaces(2)]
	[ResourceStringData("Enterprise.Customs.IN.Business.NonStandardExchangeRate|CSI_Quantity", Caption = "Unit in \u20b9")]
	public override ZDecimal CSI_Quantity
	{
		get => base.CSI_Quantity;
		set
		{
			base.CSI_Quantity = value;
			if (!IsCopying)
			{
				if (CSI_Quantity < 1)
				{
					CSI_Value = ZDecimal.Zero;
				}
				RefreshExchangeRatePrintInfo();
			}
		}
	}

	[DecimalPlaces(4)]
	[ReadOnlyMember(nameof(CSI_Value_ReadOnly))]
	[ResourceStringData("Enterprise.Customs.IN.Business.NonStandardExchangeRate|CSI_Value", Caption = "Exchange Rate")]
	public override ZDecimal CSI_Value
	{
		get => base.CSI_Value;
		set
		{
			base.CSI_Value = value;
			if (!IsCopying)
			{
				InvoicesWithPresentCurrency?.ForEach(x =>
				{
					x.SetExchangeRateIfNotUserEntered();
					x.MarkAsNeedingValidation();
				});
				RefreshExchangeRatePrintInfo();
			}
		}
	}

	public ZBool CSI_Value_ReadOnly => CSI_Quantity == 0;

	void RefreshExchangeRatePrintInfo()
	{
		if (CSI_Value.IsEmpty || CSI_Quantity.IsEmpty)
		{
			CSI_AdditionalDescription = ZString.Empty;
		}
		else
		{
			CSI_AdditionalDescription = $"{CSI_Quantity:#.####} {CSI_RX_NKCurrency} = {CSI_Value * CSI_Quantity:#.####} {Core.Constants.CurrencyCodes.India}";
		}
	}

	[MaxLength(50)]
	[ReadOnlyMember(nameof(CSI_AdditionalDescription_ReadOnly))]
	[ResourceStringData("Enterprise.Customs.IN.Business.NonStandardExchangeRate|CSI_AdditionalDescription", Caption = "Ex. Rate Print Info.")]
	public override ZString CSI_AdditionalDescription { get => base.CSI_AdditionalDescription; set => base.CSI_AdditionalDescription = value; }

	public ZBool CSI_AdditionalDescription_ReadOnly => CSI_Quantity < 1;

	public new JobDeclaration Parent => (JobDeclaration)base.Parent;

	public new NonStandardExchangeRateValidation Validation => (NonStandardExchangeRateValidation)base.Validation;

	protected override CusSupportingInfoValidation GetNewValidation() => new NonStandardExchangeRateValidation(this);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CSI_UnitOfQuantity = Core.Constants.CurrencyCodes.India;
		CSI_RN_NKCountryCode = Core.Constants.CountryCodes.India;
		CSI_Type = CusSupportingInfoTypeList.Codes.NonStandardCurrency;
		CSI_Quantity = 1;
	}

	List<JobComInvoiceHeader> InvoicesWithPresentCurrency => Parent?.Invoices.Cast<JobComInvoiceHeader>().Where(x => x.JZ_RX_NKInvoice_Currency == CSI_RX_NKCurrency).ToList();
}

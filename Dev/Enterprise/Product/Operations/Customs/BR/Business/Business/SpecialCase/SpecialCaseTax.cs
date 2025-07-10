using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class SpecialCaseTax : NonPersistentBusinessObject
	{
		public SpecialCaseTax(JobComInvoiceLineTax invoiceLineTax, QuantityPerUnitInfo quantityPerUnit, LegalActInfo legalAct)
			: base(invoiceLineTax.Factory)
		{
			InvoiceLineTax = Argument.NotNull(invoiceLineTax, nameof(invoiceLineTax));
			QuantityPerUnit = quantityPerUnit;
			LegalAct = legalAct;

			InvoiceLine = invoiceLineTax.InvoiceLine;
		}

		internal readonly JobComInvoiceLine InvoiceLine;
		internal readonly JobComInvoiceLineTax InvoiceLineTax;

		internal LegalActInfo LegalAct { get; private set; }
		internal QuantityPerUnitInfo QuantityPerUnit { get; private set; }

		#region JobComInvoiceLineTax

		[MaxLength(5)]
		[List(nameof(Lookups) + "." + nameof(SpecialCaseTaxLookups.TaxGroupList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.SpecialCaseTax|TaxGroup", Caption = "Group")]
		public ZString TaxGroup
		{
			get => InvoiceLineTax.JLT_Type;
			set
			{
				CheckMaximumLength(TaxGroupInfo, value);
				var oldValue = TaxGroup;
				InvoiceLineTax.JLT_Type = value;
				var taxGroup = TaxGroup;

				if (!IsCopying && oldValue != taxGroup)
				{
					if (QuantityPerUnit != null)
					{
						QuantityPerUnit.CSI_SubType = taxGroup;
					}

					if (InvoiceLine.IsImportSiscomex)
					{
						if (taxGroup == Constants.RateCodes.Antidumping)
						{
							LegalAct = InvoiceLine.LegalActInfos.AddNew(AdditionalTaxTypeList.Codes.Antidumping);
						}
						else if (LegalAct != null)
						{
							LegalAct.Delete();
							LegalAct = null;
						}
					}
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateTaxGroup();
				}
				TaxGroupInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TaxGroupInfo => GetZPropertyInfo(nameof(TaxGroup));

		[ResourceStringData("Enterprise.Customs.BR.Business.SpecialCaseTax|TaxGroupDescription", Caption = "Group Description")]
		public ZString TaxGroupDescription => Lookups.TaxGroupList.GetDescriptionFromCode(TaxGroup);

		[MaxLength(JobComInvoiceLineTax.Schema.JLT_MethodOfCalculationMaxLength)]
		[List(nameof(Lookups) + "." + nameof(SpecialCaseTaxLookups.TaxTypeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.SpecialCaseTax|TaxType", Caption = "Type")]
		public ZString TaxType
		{
			get => InvoiceLineTax.JLT_MethodOfCalculation;
			set
			{
				CheckMaximumLength(TaxTypeInfo, value);

				var oldValue = TaxType;
				InvoiceLineTax.JLT_MethodOfCalculation = value;
				var taxType = TaxType;

				if (!IsCopying && oldValue != taxType)
				{
					if (taxType == SpecialCaseTaxTypeList.Codes.QuantityPerUnit)
					{
						QuantityPerUnit = InvoiceLine.QuantityPerUnitInfos.AddNew(TaxGroup);
					}
					else if (QuantityPerUnit != null)
					{
						QuantityPerUnit.Delete();
						QuantityPerUnit = null;
					}
					RateOrUnitValue = 0;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateTaxType();
				}
				TaxTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TaxTypeInfo => GetZPropertyInfo(nameof(TaxType));

		[DecimalPlaces(nameof(RateOrUnitValueDecimalPlaces))]
		[ResourceStringData("Enterprise.Customs.BR.Business.SpecialCaseTax|RateOrUnitValue", Caption = "Ad Valorem (%), Reduced Rate or Unit Value")]
		public ZDecimal RateOrUnitValue
		{
			get => InvoiceLineTax.JLT_Rate;
			set
			{
				InvoiceLineTax.JLT_Rate = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateRateOrUnitValue();
				}
				RateOrUnitValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo RateOrUnitValueInfo => GetZPropertyInfo(nameof(RateOrUnitValue));

		int RateOrUnitValueDecimalPlaces => TaxType == SpecialCaseTaxTypeList.Codes.QuantityPerUnit ? 5 : 2;

		#endregion

		#region QuantityPerUnitInfo

		[MaxLength(QuantityPerUnitInfo.Schema.CSI_RX_NKCurrencyMaxLength)]
		[List(nameof(InvoiceLineLookups) + "." + nameof(JobComInvoiceLineLookups.Currencies))]
		[ResourceStringData("Enterprise.Customs.BR.Business.SpecialCaseTax|Currency", Caption = "Currency")]
		[ReadOnlyMember(nameof(QuantityPerUnitReadOnly))]
		public ZString CurrencyCode
		{
			get => QuantityPerUnit?.CSI_RX_NKCurrency ?? ZString.Empty;
			set
			{
				if (QuantityPerUnit != null)
				{
					QuantityPerUnit.CSI_RX_NKCurrency = value;
				}
				CurrencyCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CurrencyCodeInfo => QuantityPerUnit == null ? GetZPropertyInfo(nameof(CurrencyCode)) : GetWrappedZPropertyInfo(nameof(CurrencyCode), x => QuantityPerUnit.CSI_RX_NKCurrencyInfo);

		public RefCurrency Currency => Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, CurrencyCode);

		[MaxLength(QuantityPerUnitInfo.Schema.CSI_UnitOfQuantityMaxLength)]
		[List(nameof(InvoiceLineLookups) + "." + nameof(JobComInvoiceLineLookups.InvoiceUQList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.SpecialCaseTax|UnitOfMeasure", Caption = "Unit Of Measure")]
		[ReadOnlyMember(nameof(QuantityPerUnitReadOnly))]
		public ZString UnitOfMeasure
		{
			get => QuantityPerUnit?.CSI_UnitOfQuantity ?? ZString.Empty;
			set
			{
				if (QuantityPerUnit != null)
				{
					QuantityPerUnit.CSI_UnitOfQuantity = value;
				}

				if (!IsValidationSuspended)
				{
					QuantityPerUnit.Validation.ValidateCSI_UnitOfQuantity();
				}
				UnitOfMeasureInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo UnitOfMeasureInfo => QuantityPerUnit == null ? GetZPropertyInfo(nameof(UnitOfMeasure)) : GetWrappedZPropertyInfo(nameof(UnitOfMeasure), x => QuantityPerUnit.CSI_UnitOfQuantityInfo);

		[MaxLength(9)]
		[DecimalPlaces(0)]
		[ResourceStringData("Enterprise.Customs.BR.Business.SpecialCaseTax|Quantity", Caption = "Quantity")]
		[ReadOnlyMember(nameof(QuantityPerUnitReadOnly))]
		public ZDecimal Quantity
		{
			get => QuantityPerUnit?.CSI_Quantity ?? ZDecimal.Zero;
			set
			{
				if (QuantityPerUnit != null)
				{
					QuantityPerUnit.CSI_Quantity = value;
				}
				QuantityInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo QuantityInfo => QuantityPerUnit == null ? GetZPropertyInfo(nameof(Quantity)) : GetWrappedZPropertyInfo(nameof(Quantity), x => QuantityPerUnit.CSI_QuantityInfo);

		public bool QuantityPerUnitReadOnly => TaxType != SpecialCaseTaxTypeList.Codes.QuantityPerUnit;

		#endregion

		#region LegalActInfo

		[MaxLength(LegalActInfo.Schema.LegalActTypeMaxLength)]
		[List(nameof(InvoiceLineLookups) + "." + nameof(JobComInvoiceLineLookups.ExTariffLegalActList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.SpecialCaseTax|LegalActType", Caption = "Legal Act")]
		[ReadOnlyMember(nameof(LegalActReadOnly))]
		public ZString LegalActType
		{
			get => LegalAct?.CSI_Code ?? ZString.Empty;
			set
			{
				if (LegalAct != null)
				{
					LegalAct.CSI_Code = value;
				}
				LegalActTypeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo LegalActTypeInfo => LegalAct == null ? GetZPropertyInfo(nameof(LegalActType)) : GetWrappedZPropertyInfo(nameof(LegalActType), x => LegalAct.CSI_CodeInfo);

		[MaxLength(LegalActInfo.Schema.LegalActIssuingBodyMaxLength)]
		[List(nameof(InvoiceLineLookups) + "." + nameof(JobComInvoiceLineLookups.LegalActIssuingAuthorityList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.SpecialCaseTax|LegalActIssuingBody", Caption = "Issuing Body")]
		[ReadOnlyMember(nameof(LegalActReadOnly))]
		public ZString LegalActIssuingBody
		{
			get => LegalAct?.CSI_IssuerType ?? ZString.Empty;
			set
			{
				if (LegalAct != null)
				{
					LegalAct.CSI_IssuerType = value;
				}
				LegalActIssuingBodyInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo LegalActIssuingBodyInfo => LegalAct == null ? GetZPropertyInfo(nameof(LegalActIssuingBody)) : GetWrappedZPropertyInfo(nameof(LegalActIssuingBody), x => LegalAct.CSI_IssuerTypeInfo);

		[MaxLength(LegalActInfo.Schema.LegalActNumberMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.SpecialCaseTax|LegalActNumber", Caption = "Act Number")]
		[ReadOnlyMember(nameof(LegalActReadOnly))]
		public ZString LegalActNumber
		{
			get => LegalAct?.CSI_ReferenceNumber ?? ZString.Empty;
			set
			{
				if (LegalAct != null)
				{
					LegalAct.CSI_ReferenceNumber = value;
				}
				LegalActNumberInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo LegalActNumberInfo => LegalAct == null ? GetZPropertyInfo(nameof(LegalActNumber)) : GetWrappedZPropertyInfo(nameof(LegalActNumber), x => LegalAct.CSI_ReferenceNumberInfo);

		[MaxLength(LegalActInfo.Schema.LegalActYearMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.SpecialCaseTax|LegalActYear", Caption = "Year")]
		[ReadOnlyMember(nameof(LegalActReadOnly))]
		public ZString LegalActYear
		{
			get => LegalAct?.CSI_YearOfIssue ?? ZString.Empty;
			set
			{
				if (LegalAct != null)
				{
					LegalAct.CSI_YearOfIssue = value;
				}
				LegalActYearInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LegalActYearInfo => LegalAct == null ? GetZPropertyInfo(nameof(LegalActYear)) : GetWrappedZPropertyInfo(nameof(LegalActYear), x => LegalAct.CSI_YearOfIssueInfo);

		public bool LegalActReadOnly => TaxGroup != Constants.RateCodes.Antidumping;

		public JobComInvoiceLineLookups InvoiceLineLookups => InvoiceLine.Lookups;

		#endregion

		#region UnitOfMeasureDescInPortugueseBrazil

		public ZString UnitOfMeasureDescInPortugueseBrazil => Factory.GetInvoiceUQDescriptions(InvoiceLineLookups.InvoiceUQList, UnitOfMeasure).DescInPortugueseBrazil;

		#endregion

		#region Implementation

		public SpecialCaseTaxLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = new SpecialCaseTaxLookups(this);
				}
				return fLookups;
			}
		}

		SpecialCaseTaxLookups fLookups;

		public SpecialCaseTaxValidation Validation
		{
			get { return new SpecialCaseTaxValidation(this); }
		}

		public override void Delete()
		{
			InvoiceLineTax.Delete();
			LegalAct?.Delete();
			QuantityPerUnit?.Delete();
			base.Delete();
		}

		#endregion
	}
}

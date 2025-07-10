using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;
using TariffFormatter = Enterprise.Customs.Business.TariffFormatter;

namespace Enterprise.Customs.JP.Business
{
	public partial class JobComInvoiceLine : AutoJPJobComInvoiceLine, ISupportMultipleResourceStringData, ICusOtherLawReferenceParent
	{
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static string DefaultPreference => "WK";

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JI_Calc_Preference = DefaultPreference;
		}

		[CodeAlive("Advanced Ruling static fields are used within JobComInvoiceLine")]
		public new class Schema : AutoJPJobComInvoiceLine.Schema
		{
			public const string EntryInstructionDescription = nameof(EntryInstructionDescription);

			public int AdvanceRulingOnClassification_MaxLength => 9;
			public int AdvanceRulingOnOrigin_MaxLength => 7;

			public const int JI_Calc_Preference_MaxLength = 2;
			public const int JI_Calc_OriginCertifier_MaxLength = 1;
			public const int JI_Calc_CertificateOfOriginCertifier_MaxLength = 1;
		}

		public static class TradeControlOrderAppendixCode
		{
			public const string _10101 = "10101";
		}

		public static class TradeControlOrderAppendixTableDescriptionConstants
		{
			public const string AppendixTable1 = "別表第１";
			public const string AppendixTable2 = "別表第２";
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.DutyReductionExemptionRefundCodeCollection))]
		[ResourceStringData("Enterprise.Customs.JP.Business.JobComInvoiceLine|JI_DutyReductionExemptionRefundCode|IsImport", ShortCaption = "Duty Red.", Caption = "Duty Reduction Code", FullDescription = "Duty Reduction or Exemption Code", MultipleKey = IsImportCaptionKey)]
		[ResourceStringData("Enterprise.Customs.JP.Business.JobComInvoiceLine|JI_DutyReductionExemptionRefundCode|IsExport", ShortCaption = "Duty Ref.", Caption = "Duty Refund Code", FullDescription = "Duty Reduction, Exemption or Refund Code", MultipleKey = IsExportCaptionKey)]
		public override ZString JI_DutyReductionExemptionRefundCode { get => base.JI_DutyReductionExemptionRefundCode; set => base.JI_DutyReductionExemptionRefundCode = value; }

		[ResourceStringData("JP.Business.JobComInvoiceLine|JI_DutyReductionAmount", Caption = "Duty Reduction Amount", ShortCaption = "Duty Red. Amt.")]
		public override ZDecimal JI_DutyReductionAmount { get => base.JI_DutyReductionAmount; set => base.JI_DutyReductionAmount = value; }

		[ResourceStringData("JP.Business.JobComInvoiceLine|DomesticConsumptionTaxExemptionType", Caption = "Domestic Consumption Tax Exemption Type")]
		public ZString DomesticConsumptionTaxExemptionType
		{
			get
			{
				if (string.IsNullOrEmpty(JI_DomesticConsumptionTaxExemptionCode))
				{
					return string.Empty;
				}
				else
				{
					return JI_DomesticConsumptionTaxExemptionIsPartial ? "P" : "A";
				}
			}
		}

		protected override ZString GetTariffDescription(ZString tariffCode)
		{
			var tariffLength = JI_Tariff.Length;
			if (tariffLength == 4 || tariffLength == 6)
			{
				return RefCusNomenclatureGroup?.ZZ5_Description ?? ZString.Empty;
			}
			else if (tariffLength == 9)
			{
				return UniversalTariff?.ZZ1_Description ?? ZString.Empty;
			}
			else
			{
				return string.Empty;
			}
		}

		public override ZString UniversalTariffType => IsImport ? Universal.Constants.TariffTypes.Import : Universal.Constants.TariffTypes.Export;

		[ChildEditable(true)]
		public CusOtherLawReferenceCollection<CusOtherLawReferenceForInvoiceLines> OtherLaws
		{
			get
			{
				if (cusOtherLawReferences == null)
				{
					cusOtherLawReferences = new CusOtherLawReferenceCollection<CusOtherLawReferenceForInvoiceLines>(this);
					cusOtherLawReferences.Load();
					RegisterEditableChildObject(cusOtherLawReferences);
				}
				return cusOtherLawReferences;
			}
		}
		CusOtherLawReferenceCollection<CusOtherLawReferenceForInvoiceLines> cusOtherLawReferences;

		public RefCusNomenclatureGroup RefCusNomenclatureGroup
		{
			get
			{
				var effectiveDate = EffectiveDateForDutyRate;
				var query = new ZQuery(RefCusNomenclatureGroupSchema.ZZ5_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Japan);
				query.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_Value, JI_Tariff);
				query.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
				query.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);

				return Factory.LoadTop1<RefCusNomenclatureGroup>(query);
			}
		}

		public override ZString JI_Tariff
		{
			get => base.JI_Tariff;
			set
			{
				var oldValue = base.JI_Tariff;
				if (oldValue != value)
				{
					base.JI_Tariff = value;
					if (!IsCopying && JI_NACCSCode.IsEmpty && ShouldDefaultNACCSCode)
					{
						JI_NACCSCode = CalculatedNACCSCode;
					}
				}
			}
		}

		bool ShouldDefaultNACCSCode => !(IsExport && (ValueTypeList.Codes.S.Equals(EntryInstruction?.CEI_ValueType) || (ValueTypeList.Codes.L.Equals(EntryInstruction?.CEI_ValueType) && JPExportDeclarationTypeList.Codes.G.Equals(EntryInstruction?.CEI_Style))));

		public ZString CalculatedNACCSCode
		{
			get
			{
				if (JI_Tariff.Length == 9)
				{
					var isSuccess = ZInt.TryParse(JI_Tariff, out var naccsCode);
					if (isSuccess)
					{
						return (naccsCode % 7).ToString();
					}
				}
				return ZString.Empty;
			}
		}

		[MaxLength(nameof(JI_TradeControlOrderCode_Length))]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.TradeControlOrderAppendixList))]
		[ResourceStringData("JP.Business.JobComInvoiceLine|JI_TradeControlOrderAppendix", Caption = "Trade Control Order Appendix", MediumCaption = "Trade Ctrl. Order Appx.", ShortCaption = "Trade Ctrl. Ord.", FullDescription = "Trade Control Order Appendix Code")]
		public override ZString JI_TradeControlOrderAppendix { get => base.JI_TradeControlOrderAppendix; set => base.JI_TradeControlOrderAppendix = value; }

		public ZString TradeControlOrderAppendixDescription => Lookups.TradeControlOrderAppendixList.GetDescriptionFromCode(JI_TradeControlOrderAppendix);

		[MaxLength(40)]
		public override ZString JI_Description { get => base.JI_Description; set => base.JI_Description = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.FEFTAArticle48List))]
		[ResourceStringData("JP.Business.JobComInvoiceLine|JI_FEFTAArticle48", Caption = "FEFTA Article 48", ShortCaption = "FEFTA", FullDescription = "Foreign Exchange and Foreign Trade Act (FEFTA) Article 48")]
		public override ZString JI_FEFTAArticle48 { get => base.JI_FEFTAArticle48; set => base.JI_FEFTAArticle48 = value; }

		[ResourceStringData("JP.Business.JobComInvoiceLine|FEFTAArticle48Description", Caption = "Description")]
		public ZString FEFTAArticle48Description => Lookups.FEFTAArticle48List.GetDescriptionFromCode(JI_FEFTAArticle48);

		[ResourceStringData("JP.Business.JobComInvoiceLine|JI_BondedDate", Caption = "Import for Storage (IS) Date", ShortCaption = "IS Date", MediumCaption = "Import for Storage Date", FullDescription = "The date for Import for Storage (IS). When there are multiple Import for Storage (IS) applications, the date of the first approved application should be used.")]
		public override ZDateTime JI_BondedDate
		{
			get => base.JI_BondedDate;
			set
			{
				if (base.JI_BondedDate != value)
				{
					base.JI_BondedDate = value;

					if (!IsCopying)
					{
						Declaration?.MarkAsNeedingValidation();
					}
				}
			}
		}

		[MaxLength(nameof(JI_TradeControlOrderCode_Length))]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ConcessionOrderList))]
		[ResourceStringData("JP.Business.JobComInvoiceLine|JI_ConcessionOrder", Caption = "Quota")]
		public override ZString JI_ConcessionOrder { get => base.JI_ConcessionOrder; set => base.JI_ConcessionOrder = value; }

		[ReadOnlyMember(nameof(JI_NACCSCode_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.NACCSCodeList))]
		[ResourceStringData("JP.Business.JobComInvoiceLine|JI_NACCSCode", Caption = "NACCS Code")]
		public override ZString JI_NACCSCode { get => base.JI_NACCSCode; set => base.JI_NACCSCode = value; }

		bool JI_NACCSCode_ReadOnly => IsImport && JPImportDeclarationTypeList.Codes.Y.Equals(EntryInstruction?.CEI_Style);

		[DecimalPlaces(2)]
		[DecimalPrecision(9)]
		[ResourceStringData("JP.Business.JobComInvoiceLine|JI_CustomsQuantity", Caption = "Customs Quantity 1", MediumCaption = "Customs Qty 1", ShortCaption = "Qty 1")]
		public override ZDecimal JI_CustomsQuantity { get => base.JI_CustomsQuantity; set => base.JI_CustomsQuantity = value; }

		[MaxLength(4)]
		[ResourceStringData("JP.Business.JobComInvoiceLine|JI_CustomsUnitQty", Caption = "Customs Quantity Unit 1", MediumCaption = "Customs Unit 1", ShortCaption = "Unit 1")]
		public override ZString JI_CustomsUnitQty { get => base.JI_CustomsUnitQty; set => base.JI_CustomsUnitQty = value; }

		[DecimalPlaces(2)]
		[DecimalPrecision(9)]
		[ResourceStringData("JP.Business.JobComInvoiceLine|JI_CustomsSecondQuantity", Caption = "Customs Quantity 2", MediumCaption = "Customs Qty 2", ShortCaption = "Qty 2")]
		public override ZDecimal JI_CustomsSecondQuantity { get => base.JI_CustomsSecondQuantity; set => base.JI_CustomsSecondQuantity = value; }

		[ResourceStringData("JP.Business.JobComInvoiceLine|JI_CustomsSecondUnitQty", Caption = "Customs Quantity Unit 2", MediumCaption = "Customs Unit 2", ShortCaption = "Unit 2")]
		public override ZString JI_CustomsSecondUnitQty { get => base.JI_CustomsSecondUnitQty; set => base.JI_CustomsSecondUnitQty = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ConsumptionTaxExemptionIDList))]
		[ResourceStringData("JP.Business.JobComInvoiceLine|JI_DomesticConsumptionTaxExemptionCode", Caption = "Consumption Tax Exemption")]
		public override ZString JI_DomesticConsumptionTaxExemptionCode { get => base.JI_DomesticConsumptionTaxExemptionCode; set => base.JI_DomesticConsumptionTaxExemptionCode = value; }

		int JI_TradeControlOrderCode_Length
		{
			get
			{
				if (IsExport)
				{
					return 5;
				}
				else if (IsImport)
				{
					return 4;
				}
				else
				{
					return Schema.JI_ConcessionOrderMaxLength;
				}
			}
		}

		[MaxLength(nameof(Schema.AdvanceRulingOnClassification_MaxLength))]
		[ResourceStringData("JP.Business.JobComInvoiceLine|JI_AdvanceRulingOnClassification", Caption = "Advance Ruling on Classification")]
		public override ZString JI_AdvanceRulingOnClassification { get => base.JI_AdvanceRulingOnClassification; set => base.JI_AdvanceRulingOnClassification = value; }

		[MaxLength(nameof(Schema.AdvanceRulingOnOrigin_MaxLength))]
		[ResourceStringData("JP.Business.JobComInvoiceLine|JI_AdvanceRulingOnOrigin", Caption = "Advance Ruling on Origin")]
		public override ZString JI_AdvanceRulingOnOrigin { get => base.JI_AdvanceRulingOnOrigin; set => base.JI_AdvanceRulingOnOrigin = value; }

		[MaxLength(Schema.JI_Calc_Preference_MaxLength)]
		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PreferenceList))]
		[ResourceStringData("JP.Business.JobComInvoiceLine|JI_Calc_Preference", ShortCaption = "C/O Type", Caption = "Certificate of Origin Type")]
		public ZString JI_Calc_Preference
		{
			get => JI_PrimaryPreference.SubstringSafe(0, 2).Replace("_", string.Empty);
			set
			{
				var oldValue = JI_Calc_Preference;
				if (!value.Equals(oldValue))
				{
					SetPreference(value, JI_Calc_OriginCertifier, JI_Calc_CertificateOfOriginCertifier);
					if (!IsValidationSuspended)
					{
						Validation.ValidateJI_Calc_Preference();
					}
					JI_Calc_PreferenceInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo JI_Calc_PreferenceInfo => GetZPropertyInfo(nameof(JI_Calc_Preference));

		[MaxLength(Schema.JI_Calc_OriginCertifier_MaxLength)]
		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.OriginCertifierList))]
		[ResourceStringData("JP.Business.JobComInvoiceLine|JI_Calc_OriginCertifier", Caption = "Certificate of Origin Type Third Character")]
		public ZString JI_Calc_OriginCertifier
		{
			get => JI_PrimaryPreference.SubstringSafe(2, 1).Replace("_", string.Empty);
			set
			{
				var oldValue = JI_Calc_OriginCertifier;
				if (!value.Equals(oldValue))
				{
					SetPreference(JI_Calc_Preference, value, JI_Calc_CertificateOfOriginCertifier);
					if (!IsValidationSuspended)
					{
						Validation.ValidateJI_Calc_OriginCertifier();
					}
					JI_Calc_OriginCertifierInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo JI_Calc_OriginCertifierInfo => GetZPropertyInfo(nameof(JI_Calc_OriginCertifier));

		[MaxLength(Schema.JI_Calc_CertificateOfOriginCertifier_MaxLength)]
		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CertificateOfOriginCertifierList))]
		[ResourceStringData("JP.Business.JobComInvoiceLine|JI_Calc_CertificateOfOriginCertifier", Caption = "Certificate of Origin Type Fourth Character")]
		public ZString JI_Calc_CertificateOfOriginCertifier
		{
			get => JI_PrimaryPreference.SubstringSafe(3, 1).Replace("_", string.Empty);
			set
			{
				var oldValue = JI_Calc_CertificateOfOriginCertifier;
				if (!value.Equals(oldValue))
				{
					SetPreference(JI_Calc_Preference, JI_Calc_OriginCertifier, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateJI_Calc_CertificateOfOriginCertifier();
					}
					JI_Calc_CertificateOfOriginCertifierInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo JI_Calc_CertificateOfOriginCertifierInfo => GetZPropertyInfo(nameof(JI_Calc_CertificateOfOriginCertifier));

		void SetPreference(ZString preference, ZString originCertifier, ZString certificateOfOriginCertifier)
		{
			JI_PrimaryPreference = string.Concat
			(
				preference.PadRight(Schema.JI_Calc_Preference_MaxLength, '_'),
				originCertifier.PadRight(Schema.JI_Calc_OriginCertifier_MaxLength, '_'),
				certificateOfOriginCertifier.PadRight(Schema.JI_Calc_CertificateOfOriginCertifier_MaxLength, '_')
			);
		}

		[MaxLength(nameof(CertificateOfOriginID_Length))]
		[ResourceStringData("JP.Business.JobComInvoiceLine|JI_Procedure", Caption = "Certificate of Origin ID", ShortCaption = "Cert. of Origin ID")]
		public override ZString JI_Procedure
		{
			get => base.JI_Procedure;
			set
			{
				if (IsImport && value.Length > CertificateOfOriginID_Length)
				{
					value = value.Left(CertificateOfOriginID_Length);
				}

				base.JI_Procedure = value;
			}
		}
		int CertificateOfOriginID_Length => IsImport ? 4 : Schema.JI_ProcedureMaxLength;

		[ResourceStringData("JP.Business.JobComInvoiceLine|DutyRateFormula", Caption = "Duty Rate Formula", ShortCaption = "Duty", MediumCaption = "Duty Rate", FullDescription = "Import Duty Rate Formula")]
		public ZString JI_DutyRateFormula => Factory.GetValue(ref dutyRateFormula, delegate
		{
			var applicableRates = ApplicableRates;
			return applicableRates.FirstOrDefault()?.ZZ2_RateFormulaDerivedFrom ?? Res.GetString("EA703EB0-3D12-43F3-A427-5FB2A674ED02", "{None Selected}");
		});

		[ResourceStringData("E69C9F93-448F-4DA6-9B3D-95A3DBF2BB07", Caption = "Export Control Number")]
		public ZString ExportControlNumber => EntryInstruction?.ExportControlNumber ?? ZString.Empty;

		CachedProperty<ZString> dutyRateFormula;

		[ReadOnlyMember(nameof(JI_StorageTypeReadOnly))]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.StorageTypeList))]
		[ResourceStringData("JP.Business.JobComInvoiceLine|JI_StorageType", Caption = "Storage Type")]
		public override ZString JI_StorageType
		{
			get { return base.JI_StorageType; }
			set { base.JI_StorageType = value; }
		}

		public ZBool JI_StorageTypeVisible => EntryInstruction?.IsStorageTypeVisible ?? ZBool.False;

		ZBool JI_StorageTypeReadOnly => !JI_StorageTypeVisible;

		public void UpdateStorageType(bool isStorageTypeVisible)
		{
			if (!isStorageTypeVisible)
			{
				JI_StorageType = string.Empty;
			}
		}

		public override ZGuid JI_CEI
		{
			get => base.JI_CEI;
			set
			{
				if (base.JI_CEI != value)
				{
					base.JI_CEI = value;
					InvoiceHeader?.MarkAsNeedingValidation();
				}
			}
		}

		protected override CurrencyConverter GetCurrencyConverter()
		{
			if (EntryInstruction != null && EntryInstruction.CEI_DateForDuty.IsValid)
			{
				return new RefCurrencyCurrencyConverter(Factory)
				{
					DateForRate = EntryInstruction.CEI_DateForDuty,
					RateType = ExchangeRateType.Customs
				};
			}
			else
			{
				return base.GetCurrencyConverter();
			}
		}

		[ResourceStringData("97FF6E69-F1DB-4204-BEF8-C5DD1C8686A7", Caption = "Entry Instr. Desc.", FullDescription = "Entry Instruction Description")]
		public ZString EntryInstructionDescription => EntryInstruction?.EntryInstructionDescription ?? ZString.Empty;

		[ResourceStringData("8BDF2B8F-1EDE-4A02-990C-0DC9593F970D", Caption = "Invoice Quantity", MediumCaption = "Inv. Qty", ShortCaption = "Qty")]
		public override ZDecimal JI_InvoiceQuantity { get => base.JI_InvoiceQuantity; set => base.JI_InvoiceQuantity = value; }

		[ResourceStringData("795D245C-43B5-4ED7-896B-E17344B616D7", Caption = "Invoice Quantity Unit", MediumCaption = "Inv. Qty Unit", ShortCaption = "UQ")]
		public override ZString JI_InvoiceUQ { get => base.JI_InvoiceUQ; set => base.JI_InvoiceUQ = value; }

		[ResourceStringData("4AC7B52B-8C2F-4931-8799-514EBD83261D", Caption = "Invoice Price", MediumCaption = "Inv. Price", ShortCaption = "Price", FullDescription = "The product of unit price and invoice quantity. This will be declared as the Basic Price Apportionment Coefficient in the message.")]
		[DecimalPlaces(nameof(LinePriceDecimalPlaces))]
		public override ZDecimal JI_LinePrice { get => base.JI_LinePrice; set => base.JI_LinePrice = value; }

		int LinePriceDecimalPlaces => JI_RX_NKLinePriceCurr == CurrencyCodes.Japan ? 0 : 2;

		[ResourceStringData("D8821B21-AAA1-45D1-ACCF-F558A65E76EF", Caption = "Invoice Unit Price", MediumCaption = "Inv. Unit Price", ShortCaption = "Unit Price")]
		public override ZDecimal UnitPrice { get => base.UnitPrice; set => base.UnitPrice = value; }

		IEnumerable<RateView> ApplicableRates => UniversalTariff?.GetApplicableRates(DutyRateSelectionCriteria) ?? Enumerable.Empty<RateView>();

		protected override IZZRateSelectionCriteria GetDutyRateSelectionCriteriaCore() => new RateSelectionCreteria(this, Universal.Constants.RateTypes.Duty, Universal.Constants.RateTypes.Duty);

		protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.Japan;

		protected override Type TypeOfPartUsedCore => typeof(OrgSupplierPart);

		protected override TariffFormatter TariffFormatter => new Common.TariffFormatter();

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobComInvoiceLineFetchStrategy(this);

		public override void UpdateDetailsFromPivotOnPartChangeCore()
		{
			base.UpdateDetailsFromPivotOnPartChangeCore();
			var pivot = (CusClassPartPivot)Pivot;
			if (pivot != null)
			{
				this.JI_CountryOfOrigin = pivot.CI_RN_NKCountryOfOrigin;
				this.JI_PrimaryPreference = pivot.CI_PrimaryPreference;
				this.JI_SecondaryPreference = pivot.CI_SecondaryPreference;
				this.JI_TradeControlOrderAppendix = pivot.CI_TradeControlOrderAppendix;
				this.JI_FEFTAArticle48 = pivot.CI_FEFTAArticle48;
				this.JI_StorageType = pivot.CI_StorageType;
				this.JI_AdvanceRulingOnClassification = pivot.CI_AdvanceRulingOnClassification;
				this.JI_AdvanceRulingOnOrigin = pivot.CI_AdvanceRulingOnOrigin;
				this.JI_DutyReductionExemptionRefundCode = pivot.CI_DutyReductionExemptionRefundCode;
				this.JI_DomesticConsumptionTaxExemptionCode = pivot.CI_DomesticConsumptionTaxExemptionCode;
				this.JI_DomesticConsumptionTaxExemptionIsPartial = pivot.CI_DomesticConsumptionTaxExemptionIsPartial;
				this.JI_DutyReductionAmount = pivot.CI_DutyReductionAmount;
			}
		}

		internal bool IsAppendixTable1 => TradeControlOrderAppendixDescription.StartsWith(TradeControlOrderAppendixTableDescriptionConstants.AppendixTable1);

		internal bool IsAppendixTable2 => TradeControlOrderAppendixDescription.StartsWith(TradeControlOrderAppendixTableDescriptionConstants.AppendixTable2);

		#region ISupportMultipleResourceStringData

		public IReadOnlyList<string> MultipleKeysToUse => IsImport ? [IsImportCaptionKey] : [IsExportCaptionKey];

		public const string IsImportCaptionKey = "E57D934E-80F1-4A64-B04B-121ED3534A42";

		public const string IsExportCaptionKey = "3E6E0AC9-6318-4EC7-A656-4F36D8C5A673";

		#endregion

		#region ICusOtherLawReferenceParent
		ZBool ICusOtherLawReferenceParent.IsOtherLawReferenceRequired => IsExport;

		public IEnumerable<ZString> GetTariffAttributesByKey(ZString key) => UniversalTariff?.GetAttributes(key).Select(x => x.ZZ3_Value) ?? Enumerable.Empty<ZString>();
		#endregion
	}
}

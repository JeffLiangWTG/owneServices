using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[SystemDefinedValues]
	public partial class JobComInvoiceLine : AutoKRJobComInvoiceLine,
		ICusCodeDataTypeSupporter,
		ICusSupportingInfoTypeSupporter,
		Integration.Customs.KR.IJobComInvoiceLine,
		ILineOrProduct,
		ISupportingDocumentParent,
		ISupportMultipleResourceStringData
	{
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoKRJobComInvoiceLine.Schema
		{
			public const string AdditionalInformationContent = "AdditionalInformationContent";

			public const int AdditionalInformationContentMaxLength = 600;
			public const int JI_FormattedTariff_MaxLength = 12;
			public new const int JI_TariffMaxLength = 10;
			public new const int JI_DescriptionMaxLength = 400;
			public new const int JI_LotNumberMaxLength = 50;
			public const int CertificateOfOriginNoMaxLength = 60;
			public const int CertificateOfOriginCriteriaCodeMaxLength = 2;
			public const int CertificateOfOriginIssuingCountryMaxLength = 2;
			public const int CertificateOfOriginAgencyNameMaxLength = 60;
			public const int CertificateOfOriginAreaNameMaxLength = 30;
			public const int CertificateOfOriginPersonNameMaxLength = 60;
			public const int CertificateOfOriginUnitOfQuantityMaxLength = 4;
			public const int CertificateOfOriginStatusMaxLength = 1;
			public const int CertificateOfOriginUQMaxLength = 4;
			public new const int JI_ModelMaxLength = 50;
			public const int JI_ModelImportAnd008MaxLength = 75;
			public new const int JI_BrandNameMaxLength = 30;
			public const int JI_BrandName008MaxLength = 50;
			public const int JI_NDescription008MaxLength = 200;
		}
		#region GenAddOn
		public static class GenAddOnColumnConstants
		{
			public const string KR_HighestGAApprovalSeqNo = "KR_HighestGAApprovalSeqNo";
			public const string KR_HighestVehicleSeqNo = "KR_HighestVehicleSeqNo";
		}
		#endregion

		#region GetTariffDescription - to be overridden once the Tariff is setup for a new country

		protected override ZString GetTariffDescription(ZString tariffCode) => ZString.Empty;

		#endregion

		[MaxLength(Schema.AdditionalInformationContentMaxLength)]
		[ResourceStringData("BF21CE3C-FA0F-4A39-AFA2-2AACEB1EC2A1", Caption = "Remark")]
		public ZString AdditionalInformationContent
		{
			get => AdditionalInformationContentNote.Text;
			set => AdditionalInformationContentNote.SetNoteText(this, AdditionalInformationContentInfo, value);
		}

		public ZPropertyInfo AdditionalInformationContentInfo => GetZPropertyInfo(Schema.AdditionalInformationContent);

		HiddenTextNote AdditionalInformationContentNote => additionalInformationContentNote ?? (additionalInformationContentNote = new HiddenTextNote(this, PredefinedNoteTypes.Instance.CustomsMessageRemarks.Description));
		HiddenTextNote additionalInformationContentNote;

		public override ZGuid JI_JZ
		{
			get => base.JI_JZ;
			set
			{
				var oldValue = JI_JZ;
				base.JI_JZ = value;
				if (!IsCopying && oldValue != JI_JZ && !IsValidationSuspended)
				{
					foreach (VehicleNumber vehicleNumber in VehicleNumbers)
					{
						vehicleNumber.MarkAsNeedingValidation();
					}
				}
			}
		}

		[ResourceStringData("024F1C54-EEE4-4AE8-B634-96D34A6C70BC", Caption = "Seq #", MultipleKey = KRJobMessageTypeList.Codes.PersonalItems)]
		public override ZShort JI_LineNo { get => base.JI_LineNo; set => base.JI_LineNo = value; }

		[ResourceStringData("DD91183E-A7D0-43DB-AB4C-58423E370130", Caption = "Goods Origin", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		public override ZString JI_CountryOfOrigin
		{
			get => base.JI_CountryOfOrigin;
			set
			{
				var oldValue = base.JI_CountryOfOrigin;
				base.JI_CountryOfOrigin = value;
				if (oldValue != JI_CountryOfOrigin)
				{
					SetDefaultDutyRateSelection();
				}
			}
		}
		[ResourceStringData("926E9BD6-6BE2-46E4-AD46-5DE5F8081165", Caption = "FTA Type")]
		[ResourceStringData("4B713FCC-4E87-4163-8AF3-0898DC9211F1", Caption = "Preference Code", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		[MaxLength(nameof(JI_PrimaryPreferenceMaxLength))]
		public override ZString JI_PrimaryPreference
		{
			get => base.JI_PrimaryPreference;
			set
			{
				var oldValue = JI_PrimaryPreference;
				base.JI_PrimaryPreference = value;

				if (oldValue != JI_PrimaryPreference)
				{
					JI_IsSpecificUseCode = IsSpecificUseCode();
					SetDefaultDutyRateSelection();
				}
			}
		}
		int JI_PrimaryPreferenceMaxLength => IsExport ? 3 : 6;

		[MaxLength(Schema.JI_TariffMaxLength)]
		public override ZString JI_Tariff
		{
			get => base.JI_Tariff;
			set
			{
				var oldValue = JI_Tariff;
				using (GetValidationSuspender())
				{
					base.JI_Tariff = value;
					if (oldValue != JI_Tariff)
					{
						JI_IsSpecificUseCode = IsSpecificUseCode();
						if (!IsCopying)
						{
							RenewGAApprovalDataCollectionByTariff();
							RenewHSExtensionCodeCollectionByTariff();
							Declaration?.RefreshInvoiceLinesEligibleForSimpleDrawback();
							RenewCustomsUQ();
						}
						SetDefaultDutyRateSelection();
					}
				}
				Validation.ValidateJI_Tariff();
			}
		}
		void SetDefaultDutyRateSelection()
		{
			if (Lookups.DutyRateSelectionList.Count == 1)
			{
				JI_DutyRateSelection = Lookups.DutyRateSelectionList[0].Code;
			}
			else
			{
				JI_DutyRateSelection = ZString.Empty;
			}
		}

		public void RenewHSExtensionCodeCollectionByTariff()
		{
			HSExtensionCodeCollection.UpdateMainAdditionalCode(UniversalTariff);
		}

		public void RenewGAApprovalDataCollectionByTariff()
		{
			GAApprovalDataCollection.UpdateExportConditions(UniversalTariff, EffectiveAssessmentDate);
		}

		public bool IsPreapprovalMandatory
		{
			get
			{
				var destCountry = Declaration?.JE_GoodsDestination ?? ZString.Empty;
				var tariffValid = UniversalTariff != null;

				if (!tariffValid || destCountry.IsEmpty)
				{
					return false;
				}

				return Factory.GetCachedValue("PreapprovalMandatory" + JI_Tariff + destCountry + EffectiveAssessmentDate.ToShortDateString(), () =>
				{
					var tradeGroups = GetTradeGroups();
					if (tradeGroups.Length == 0)
					{
						return false;
					}

					var parentNomenclatures = GetParentNomenclatures();

					return parentNomenclatures.Any(nomenclature =>
								nomenclature.Conditions.Any(condition =>
									HasValidSteelProductCondition(condition) && condition.ConditionValues.Any(conditionValue =>
										HasMatchingTradeGroup(conditionValue))));

					RefCusTradeGroup[] GetTradeGroups()
					{
						var tradeCountriesQuery = new ZDBOnlySubQuery(typeof(RefCusTradeGroupCountry), RefCusTradeGroupCountrySchema.ZZB_ZZA_TradeGroup);
						tradeCountriesQuery.AddToFilter(RefCusTradeGroupCountrySchema.ZZB_RN_NKTradeGroupCountryCode, destCountry)
							.AddToFilter(RefCusTradeGroupCountrySchema.ZZB_StartDate, SQLComparisonOperator.LessThanOrEqualTo, EffectiveAssessmentDate)
							.AddToFilter(RefCusTradeGroupCountrySchema.ZZB_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, EffectiveAssessmentDate);

						var tradeGroupQuery = new ZDBOnlyQuery(typeof(RefCusTradeGroup));
						tradeGroupQuery.AddToFilter(RefCusTradeGroupSchema.ZZA_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.KoreaSouth)
							.AddToFilter(RefCusTradeGroupSchema.ZZA_StartDate, SQLComparisonOperator.LessThanOrEqualTo, EffectiveAssessmentDate)
							.AddToFilter(RefCusTradeGroupSchema.ZZA_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, EffectiveAssessmentDate);
						tradeGroupQuery.AddSubQuery(tradeCountriesQuery, JoinCondition.And);

						return Factory.Load<RefCusTradeGroup>(tradeGroupQuery);
					}

					RefCusNomenclatureGroup[] GetParentNomenclatures()
					{
						var nomenclatureMaxLength = 9;
						var nomenclatureMinLength = 4;

						var nomenclatureQuery = new ZQuery(RefCusNomenclatureGroupSchema.ZZ5_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.KoreaSouth);
						nomenclatureQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_Value, Enumerable.Range(nomenclatureMinLength, nomenclatureMaxLength - nomenclatureMinLength + 1).Select(x => JI_Tariff.Substring(0, x)))
										.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_StartDate, SQLComparisonOperator.LessThanOrEqualTo, EffectiveAssessmentDate)
										.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, EffectiveAssessmentDate);

						return Factory.Load<RefCusNomenclatureGroup>(nomenclatureQuery);
					}

					bool HasValidSteelProductCondition(RefCusCondition condition)
					{
						return condition.ConditionType.Equals(Constants.ZZ.RefCusConditionType.SteelProduct)
							&& condition.ZX1_StartDate <= EffectiveAssessmentDate
							&& condition.ZX1_EndDate >= EffectiveAssessmentDate;
					}

					bool HasMatchingTradeGroup(RefCusConditionValue conditionValue)
					{
						return conditionValue.ValueType.Equals(Constants.ZZ.RefCusConditionValueType.TradeGroup)
							&& tradeGroups.Any(tradeGroup => tradeGroup.ZZA_TradeGroup.Equals(conditionValue.ZX3_Value));
					}
				});
			}
		}

		public override ZGuid JI_CL
		{
			get => base.JI_CL;
			set
			{
				var oldValue = JI_CL;
				base.JI_CL = value;
				if (!IsCopying && oldValue != JI_CL)
				{
					if (!IsValidationSuspended)
					{
						InvoiceHeader?.MarkAsNeedingValidation();
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.EntryInstructionList))]
		[ResourceStringData("439A0862-E013-4C9E-AFB5-9EE848E0AF33", Caption = "Entry Instruction")]
		[ResourceStringData("DF758570-5CA6-4E6D-8C6A-B31783BE94D8", Caption = "Entry Instruction", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		public override ZGuid JI_CEI { get => base.JI_CEI; set => base.JI_CEI = value; }

		[DecimalPlaces(DecimalPlacesConstants.Weight)]
		public override ZDecimal JI_NetWeight
		{
			get => base.JI_NetWeight;
			set
			{
				var oldValue = JI_NetWeight;
				base.JI_NetWeight = value;
				if (!IsCopying && oldValue != JI_NetWeight && !IsValidationSuspended)
				{
					InvoiceHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString JI_NetWeightUQ
		{
			get => base.JI_NetWeightUQ;
			set
			{
				var oldValue = JI_NetWeightUQ;
				base.JI_NetWeightUQ = value;
				if (!IsCopying && oldValue != JI_NetWeightUQ && !IsValidationSuspended)
				{
					InvoiceHeader?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("BD27FA78-9C1E-448D-8EFA-E1D804B8BE07", Caption = "Gross Weight", ShortCaption = "Gr. Weight")]
		[ResourceStringData("588505A8-0471-4614-A5CD-6963459EDE82", Caption = "Gross Weight", ShortCaption = "Gr. Weight", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		public override ZDecimal JI_Weight
		{
			get => base.JI_Weight;
			set
			{
				var oldValue = JI_Weight;
				base.JI_Weight = value;
				if (!IsCopying && oldValue != JI_Weight && !IsValidationSuspended)
				{
					InvoiceHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString JI_WeightUQ
		{
			get => base.JI_WeightUQ;
			set
			{
				var oldValue = JI_WeightUQ;
				base.JI_WeightUQ = value;
				if (!IsCopying && oldValue != JI_WeightUQ && !IsValidationSuspended)
				{
					InvoiceHeader?.MarkAsNeedingValidation();
				}
			}
		}

		protected override void SetTariffEtcDataFromProductsPivotCore(BaseCusClassPartPivot pivot)
		{
			base.SetTariffEtcDataFromProductsPivotCore(pivot);
			var cusClassPartPivot = (CusClassPartPivot)pivot;
			if (!string.IsNullOrEmpty(cusClassPartPivot.Part.OP_Brand))
			{
				JI_BrandName = cusClassPartPivot.Part.OP_Brand;
			}
			if (!string.IsNullOrEmpty(cusClassPartPivot.Part.OP_Model))
			{
				JI_Model = cusClassPartPivot.Part.OP_Model;
			}
			if (!cusClassPartPivot.KRClassification.CKR_Ingredient.IsEmpty)
			{
				JI_Ingredient = cusClassPartPivot.KRClassification.CKR_Ingredient;
			}
			if (!cusClassPartPivot.CI_RN_NKCountryOfOrigin.IsEmpty)
			{
				JI_CountryOfOrigin = cusClassPartPivot.CI_RN_NKCountryOfOrigin;
			}
			if (!cusClassPartPivot.KRClassification.CKR_COOLabelLocation.IsEmpty)
			{
				JI_COOLabelLocation = cusClassPartPivot.KRClassification.CKR_COOLabelLocation;
			}
			GAApprovalDataCollection.UpdateExportConditions((CusClassPartPivot)pivot);
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SecondaryPreferences))]
		[ResourceStringData("7F778445-A833-4876-8FEE-D0BE47B4BB8F", Caption = "Duty Reduction Code")]
		public override ZString JI_SecondaryPreference { get => base.JI_SecondaryPreference; set => base.JI_SecondaryPreference = value; }

		[MaxLength(Schema.JI_FormattedTariff_MaxLength)]
		[ResourceStringData("EAD26501-A4C7-4DB9-9EB7-0B80D09027EB", Caption = "Tariff")]
		public override ZString JI_FormattedTariff { get => base.JI_FormattedTariff; set => base.JI_FormattedTariff = value; }

		[ResourceStringData("9F69CE51-09AE-4F8B-B939-D00626564DC9", Caption = "UQ")]
		public override ZString JI_CustomsUnitQty { get => base.JI_CustomsUnitQty; set => base.JI_CustomsUnitQty = value; }

		[ReadOnly(true)]
		[ResourceStringData("6065D4A1-DC8D-4A75-AAF6-2A305A1D6318", Caption = "UQ")]
		public override ZString JI_CustomsSecondUnitQty { get => base.JI_CustomsSecondUnitQty; set => base.JI_CustomsSecondUnitQty = value; }

		[ReadOnly(true)]
		[ResourceStringData("13A4970C-9C99-4C08-8F81-EDB6832C4AD6", Caption = "UQ")]
		public override ZString JI_CustomsThirdUnitQty { get => base.JI_CustomsThirdUnitQty; set => base.JI_CustomsThirdUnitQty = value; }

		[ReadOnly(true)]
		[ResourceStringData("FF71174A-D946-4E92-A25B-A9E8C71725E9", Caption = "UQ")]
		public override ZString JI_CustomsFourthUnitQty { get => base.JI_CustomsFourthUnitQty; set => base.JI_CustomsFourthUnitQty = value; }

		int JI_ModelMaxLength => Declaration?.JE_MessageType.ToString() switch
		{
			KRJobMessageTypeList.Codes.Import => Schema.JI_ModelImportAnd008MaxLength,
			KRJobMessageTypeList.Codes.PersonalItems => Schema.JI_ModelImportAnd008MaxLength,
			_ => Schema.JI_ModelMaxLength
		};
		[MaxLength(nameof(JI_ModelMaxLength))]
		[ResourceStringData("2E5FBA8B-7BE6-4ABA-9FCE-EA9294939BFE", Caption = "Model/Trade Name")]
		[ResourceStringData("C75B938D-80B7-4302-8321-B3D8C2F955E9", Caption = "Model", MultipleKey = KRJobMessageTypeList.Codes.PersonalItems)]
		public override ZString JI_Model { get => base.JI_Model; set => base.JI_Model = value; }

		[MaxLength(nameof(JI_BrandNameMaxLength))]
		[ResourceStringData("9113832B-6492-4709-A892-6B096F6CD686", Caption = "Brand Name")]
		public override ZString JI_BrandName { get => base.JI_BrandName; set => base.JI_BrandName = value; }

		int JI_BrandNameMaxLength => Is008 ? Schema.JI_BrandName008MaxLength : Schema.JI_BrandNameMaxLength;

		[MaxLength(3)]
		[ResourceStringData("AB739950-3AEA-45D5-8AD6-9098F3DCB457", Caption = "Import Entry Line No.")]
		public override ZShort JI_PreviousEntryLineNumber { get => base.JI_PreviousEntryLineNumber; set => base.JI_PreviousEntryLineNumber = value; }

		[MaxLength(nameof(JI_PreviousEntryNumber_MaxLength))]
		[ResourceStringData("67AA1CBF-F519-4342-AF29-9833C64127CB", Caption = "Import Declaration No.")]
		[ResourceStringData("415077AE-92C7-4780-BECF-BB2EF859329E", Caption = "Previous Document No.", MultipleKey = KRJobMessageTypeList.Codes.LocalExport)]
		public override ZString JI_PreviousEntryNumber { get => base.JI_PreviousEntryNumber; set => base.JI_PreviousEntryNumber = value; }

		public int JI_PreviousEntryNumber_MaxLength => IsExport ? 16 : Schema.JI_PreviousEntryNumberMaxLength;

		[ResourceStringData("2742EDA5-EBC0-4511-B9FC-C21AD70A683F", Caption = "Customs Qty")]
		[ResourceStringData("C9008665-C958-4A08-9FA8-DA0680EA625E", Caption = "Month of Use", MultipleKey = KRJobMessageTypeList.Codes.PersonalItems)]
		[DecimalPlaces(nameof(CustomsQuantityDecimalPlace))]
		public override ZDecimal JI_CustomsQuantity { get => base.JI_CustomsQuantity; set => base.JI_CustomsQuantity = value; }
		protected int CustomsQuantityDecimalPlace
		{
			get
			{
				if (IsLocalExport)
				{
					return Constants.DecimalPlacesConstants.CustomsQuantityOrWeight;
				}
				else if (IsPersonalItemDeclaration)
				{
					return Constants.DecimalPlacesConstants.Month;
				}
				else
				{
					return Constants.DecimalPlacesConstants.CustomsQuantity;
				}
			}
		}

		[ReadOnlyMember(nameof(JI_CustomsSecondQuantity_ReadOnly))]
		[ResourceStringData("6EB138B4-46AF-428F-8047-650EDBD8CC2B", Caption = "Customs Qty 2")]
		[DecimalPlaces(nameof(CustomsQuantityDecimalPlace))]
		public override ZDecimal JI_CustomsSecondQuantity { get => base.JI_CustomsSecondQuantity; set => base.JI_CustomsSecondQuantity = value; }
		bool JI_CustomsSecondQuantity_ReadOnly => JI_CustomsSecondUnitQty.IsEmpty;

		[ReadOnlyMember(nameof(JI_CustomsThirdQuantity_ReadOnly))]
		[ResourceStringData("63ED74ED-AE16-4C96-9583-16034CA46BD2", Caption = "Customs Qty 3", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		[DecimalPlaces(nameof(CustomsQuantityDecimalPlace))]
		public override ZDecimal JI_CustomsThirdQuantity { get => base.JI_CustomsThirdQuantity; set => base.JI_CustomsThirdQuantity = value; }
		bool JI_CustomsThirdQuantity_ReadOnly => JI_CustomsThirdUnitQty.IsEmpty;

		[ReadOnlyMember(nameof(JI_CustomsFourthQuantity_ReadOnly))]
		[ResourceStringData("173DC7E5-AC6F-471B-9138-9258ED4B5575", Caption = "Customs Qty 4", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		[DecimalPlaces(nameof(CustomsQuantityDecimalPlace))]
		public override ZDecimal JI_CustomsFourthQuantity { get => base.JI_CustomsFourthQuantity; set => base.JI_CustomsFourthQuantity = value; }
		bool JI_CustomsFourthQuantity_ReadOnly => JI_CustomsFourthUnitQty.IsEmpty;

		[ResourceStringData("58C94222-7546-4B05-B4E6-978E628FBAD6", Caption = "C/O Used Quantity")]
		[DecimalPlaces(DecimalPlacesConstants.CustomsQuantityOrWeight)]
		public override ZDecimal JI_CustomsFifthQuantity { get => base.JI_CustomsFifthQuantity; set => base.JI_CustomsFifthQuantity = value; }

		[DecimalPlaces(nameof(InvoiceQuantityDecimalPlaces))]
		[ResourceStringData("17986A8A-C59F-44A5-88B8-5F63A4163253", Caption = "Quantity", MultipleKey = KRJobMessageTypeList.Codes.PersonalItems)]
		[ResourceStringData("E302702B-FB5F-46B7-B286-1E9399BF6D49", Caption = "Invoice Qty", MultipleKey = KRJobMessageTypeList.Codes.LocalExport)]
		[ResourceStringData("B2A07634-349F-4DA5-94EA-80839712E4C0", Caption = "Invoice Qty", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		public override ZDecimal JI_InvoiceQuantity { get => base.JI_InvoiceQuantity; set => base.JI_InvoiceQuantity = value; }
		int InvoiceQuantityDecimalPlaces
		{
			get
			{
				int result;
				if (IsLocalExport)
				{
					result = DecimalPlacesConstants.LocalExportInvoiceQuantity;
				}
				else if (Is008)
				{
					result = DecimalPlacesConstants.Qty;
				}
				else
				{
					result = DecimalPlacesConstants.InvoiceQuantity;
				}
				return result;
			}
		}

		[MaxLength(nameof(JI_DescriptionMaxLength))]
		[ResourceStringData("01A01BF2-9487-4A56-AAB7-0CD6B07FEB22", Caption = "Goods Description")]
		public override ZString JI_Description { get => base.JI_Description; set => base.JI_Description = value; }

		int JI_DescriptionMaxLength => (Declaration?.Is5SM ?? false) ? 90 : Schema.JI_DescriptionMaxLength;

		[ResourceStringData("419A60EA-68DC-4589-882F-91D23F063350", Caption = "Invoice Description", MultipleKey = KRJobMessageTypeList.Codes.PersonalItems)]
		[MaxLength(nameof(JI_NDescriptionMaxLength))]
		public override ZString JI_NDescription { get => base.JI_NDescription; set => base.JI_NDescription = value; }

		int JI_NDescriptionMaxLength => (Declaration?.IsPersonalItemDeclaration ?? false) ? Schema.JI_NDescription008MaxLength : Schema.JI_NDescriptionMaxLength;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.InvoiceUQList))]
		[ResourceStringData("BD0630BB-B638-4D92-8E17-21024A1760D8", Caption = "Item Code", MultipleKey = KRJobMessageTypeList.Codes.PersonalItems)]
		public override ZString JI_InvoiceUQ { get => base.JI_InvoiceUQ; set => base.JI_InvoiceUQ = value; }

		[ResourceStringData("81EBAFA7-1401-473A-AE02-29E0AD9EC176", Caption = "Material Line No")]
		public override ZShort JI_ParentLine { get => base.JI_ParentLine; set => base.JI_ParentLine = value; }
		protected override int UnitPriceDecimalPlaces => Constants.DecimalPlacesConstants.UnitPrice;

		[ResourceStringData("BC7C9E89-84CF-4EF5-85C0-96E38ADC6C06", Caption = "Line Currency")]
		public ZString LineCurrency => JI_RX_NKLinePriceCurr;

		public override ZDateTime EffectiveAssessmentDate
		{
			get
			{
				var result = CusEntryLine?.Header?.CusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
				return result.IsEmpty ? ZDateTime.Today : result;
			}
		}

		public ZShort KR_HighestGAApprovalSeqNo
		{
			get { return this.GetSystemDefinedValue<ZShort>(GenAddOnColumnConstants.KR_HighestGAApprovalSeqNo); }
			set
			{
				var oldValue = KR_HighestGAApprovalSeqNo;
				this.SetSystemDefinedValue(GenAddOnColumnConstants.KR_HighestGAApprovalSeqNo, value);
			}
		}

		public ZShort KR_HighestVehicleSeqNo
		{
			get { return this.GetSystemDefinedValue<ZShort>(GenAddOnColumnConstants.KR_HighestVehicleSeqNo); }
			set
			{
				var oldValue = KR_HighestVehicleSeqNo;
				this.SetSystemDefinedValue(GenAddOnColumnConstants.KR_HighestVehicleSeqNo, value);
			}
		}

		ZDateTime ILineOrProduct.DeclarationDate => EffectiveAssessmentDate;
		bool ILineOrProduct.IsExport => IsExport;
		bool ILineOrProduct.IsImport => IsImport;
		bool ILineOrProduct.IsIssueDateRelevant => true;
		bool ILineOrProduct.IsReferenceNumberRelevant => true;
		ZString ILineOrProduct.Tariff => JI_Tariff;
		GAApprovalCollection ILineOrProduct.GAApprovalDataCollection => GAApprovalDataCollection;
		HSExtensionCodeCollection ILineOrProduct.HSExtensionCodeCollection => HSExtensionCodeCollection;
		bool ILineOrProduct.IsValidationEnabled => true;
		TariffView ILineOrProduct.UniversalTariff => UniversalTariff;
		public ZBool IsSubjectTo5FN
		{
			get
			{
				var specificUseCodeDutyRatePermitNo = JI_SpecificUseCodeDutyRatePermitNo;
				var dutyReductionClassificationCode = DutyReductionClassificationCode;
				return specificUseCodeDutyRatePermitNo == ZString.Empty &&
					(dutyReductionClassificationCode == ImportDutyReductionClassificationList.Codes.DutyExemption ||
						 dutyReductionClassificationCode == ImportDutyReductionClassificationList.Codes.DutyReduction ||
						 dutyReductionClassificationCode == ImportDutyReductionClassificationList.Codes.InstallmentPayment ||
						 dutyReductionClassificationCode == ImportDutyReductionClassificationList.Codes.SpecificUseCodeOnly ||
						 dutyReductionClassificationCode == ImportDutyReductionClassificationList.Codes.SpecificUseCodeAndDutyReductionOrInstallment
					);
			}
		}

		protected override CurrencyConverter GetCurrencyConverter()
		{
			var declaration = Declaration;
			if (declaration != null && CusEntryLine != null)
			{
				return CusEntryLine.CurrencyConverter;
			}
			else
			{
				return base.GetCurrencyConverter();
			}
		}

		#region Related Objects

		public TariffView DutyReductionExemptionTariff
		{
			get
			{
				if (dutyReductionExemptionTariff == null || dutyReductionExemptionTariff.ZZ1_TariffCode != JI_SecondaryPreference)
				{
					dutyReductionExemptionTariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.TariffTypes.DutyReductionExemption, JI_SecondaryPreference, EffectiveAssessmentDate);
				}
				return dutyReductionExemptionTariff;
			}
		}
		TariffView dutyReductionExemptionTariff;
		#endregion

		#region Collections

		PreApproval PreApprovalData
		{
			get
			{
				if (preApprovalData?.IsDeleted ?? true)
				{
					preApprovalData = PreApprovalCollection.FirstOrDefault();
				}
				return preApprovalData;
			}
		}
		PreApproval preApprovalData;

		[ChildEditable(true)]
		public PreApprovalCollection PreApprovalCollection
		{
			get
			{
				if (preApprovalCollection == null)
				{
					preApprovalCollection = new PreApprovalCollection(this);
					preApprovalCollection.Load();
					RegisterEditableChildObject(preApprovalCollection);
				}
				return preApprovalCollection;
			}
		}
		PreApprovalCollection preApprovalCollection;

		[MaxLength(20)]
		[ResourceStringData("3F7EBE01-C8C0-4EB0-A6CB-F20875DAD95D", Caption = "Steel Approval No.")]
		public ZString PRA_ReferenceNumber
		{
			get => PreApprovalData?.CSI_ReferenceNumber ?? ZString.Empty;
			set
			{
				if (PreApprovalData == null)
				{
					PreApprovalCollection.AddNew();
				}
				PreApprovalData.CSI_ReferenceNumber = value;
				if (IsExport && !IsValidationSuspended)
				{
					((EXPJobComInvoiceLineValidation)Validation).ValidatePRA_ReferenceNumber();
				}
				PRA_ReferenceNumberInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo PRA_ReferenceNumberInfo => GetZPropertyInfo(nameof(PRA_ReferenceNumber));

		[ResourceStringData("69C84A85-0BCA-433C-80FB-9204A7735DC9", Caption = "Steel Approval Effective Date From")]
		public ZDateTime PRA_DateOfIssue
		{
			get => PreApprovalData?.CSI_DateOfIssue ?? ZDateTime.Empty;
			set
			{
				if (PreApprovalData == null)
				{
					PreApprovalCollection.AddNew();
				}
				PreApprovalData.CSI_DateOfIssue = value;
				if (IsExport && !IsValidationSuspended)
				{
					((EXPJobComInvoiceLineValidation)Validation).ValidatePRA_DateOfIssue();
				}
				PRA_DateOfIssueInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo PRA_DateOfIssueInfo => GetZPropertyInfo(nameof(PRA_DateOfIssue));

		[ResourceStringData("1446EC0D-7882-4F7A-829D-B3904C87E53E", Caption = "Steel Approval Effective Date To")]
		public ZDateTime PRA_DateOfExpiry
		{
			get => PreApprovalData?.CSI_DateOfExpiry ?? ZDate.Empty;
			set
			{
				if (PreApprovalData == null)
				{
					PreApprovalCollection.AddNew();
				}
				PreApprovalData.CSI_DateOfExpiry = value;
				if (IsExport && !IsValidationSuspended)
				{
					((EXPJobComInvoiceLineValidation)Validation).ValidatePRA_DateOfExpiry();
				}
				PRA_DateOfExpiryInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo PRA_DateOfExpiryInfo => GetZPropertyInfo(nameof(PRA_DateOfExpiry));

		[ChildEditable(true)]
		public GAApprovalCollection GAApprovalDataCollection
		{
			get
			{
				if (gaApprovalDataCollection == null)
				{
					gaApprovalDataCollection = new GAApprovalCollection(this);
					gaApprovalDataCollection.Load();
					RegisterEditableChildObject(gaApprovalDataCollection);
				}
				return gaApprovalDataCollection;
			}
		}
		GAApprovalCollection gaApprovalDataCollection;

		public CertificateOfOrigin CertificateOfOriginData
		{
			get
			{
				if (certificateOfOriginData?.IsDeleted ?? true)
				{
					certificateOfOriginData = CertificateOfOriginCollection.FirstOrDefault();
				}
				return certificateOfOriginData;
			}
		}
		CertificateOfOrigin certificateOfOriginData;

		public void CreateCertificateOfOriginDataIfRequired()
		{
			if (CertificateOfOriginData == null)
			{
				CertificateOfOriginCollection.AddNew();
			}
		}

		[ChildEditable(true)]
		CertificateOfOriginCollection CertificateOfOriginCollection
		{
			get
			{
				if (certificateOfOriginCollection == null)
				{
					certificateOfOriginCollection = new CertificateOfOriginCollection(this);
					certificateOfOriginCollection.Load();
					RegisterEditableChildObject(certificateOfOriginCollection);
				}
				return certificateOfOriginCollection;
			}
		}
		CertificateOfOriginCollection certificateOfOriginCollection;

		public void AddAllNotificationsFromCertificateOfOriginCSI_Code()
		{
			if (CertificateOfOriginData != null)
			{
				CertificateOfOriginIssueStatusInfo.AddAllNotificationsFrom(CertificateOfOriginData.CSI_CodeInfo);
			}
		}

		[ResourceStringData("BE10F930-700F-4668-8B2A-E20730F6DD3F", Caption = "C/O Product Type")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CertificateOfOriginProductTypeCodeList))]
		[MaxLength(1)]
		public ZString CertificateOfOriginProductType
		{
			get
			{
				return CertificateOfOriginData?.CSI_Code ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_Code = value;
				CertificateOfOriginProductTypeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginProductTypeInfo => GetZPropertyInfo(nameof(CertificateOfOriginProductType));

		[ResourceStringData("21CBB327-54EB-4CEC-A336-42FF5F969CBA", Caption = "C/O Issued")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CertificateOfOriginIssuedCodeList))]
		[MaxLength(1)]
		public ZString CertificateOfOriginIssueStatus
		{
			get
			{
				return CertificateOfOriginData?.CSI_Code ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_Code = value;

				if (IsExport && !IsValidationSuspended)
				{
					((EXPJobComInvoiceLineValidation)Validation).ValidateCertificateOfOriginIssueStatus();
				}
				CertificateOfOriginIssueStatusInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginIssueStatusInfo => GetZPropertyInfo(nameof(CertificateOfOriginIssueStatus));

		[ResourceStringData("048B10E0-7E58-4D1C-9EB6-668A57AF60D1", Caption = "C/O Determination Rule")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CountryOfOriginDeterminationRuleCodeList))]
		[MaxLength(1)]
		public ZString CriteriaForDeterminingCountryOfOrigin
		{
			get
			{
				return CertificateOfOriginData?.CSI_SubType ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_SubType = value;

				if (IsImport && !IsValidationSuspended)
				{
					((IMPJobComInvoiceLineValidation)Validation).ValidateCriteriaForDeterminingCountryOfOrigin();
				}
				CriteriaForDeterminingCountryOfOriginInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CriteriaForDeterminingCountryOfOriginInfo => GetZPropertyInfo(nameof(CriteriaForDeterminingCountryOfOrigin));

		[MaxLength(Schema.CertificateOfOriginNoMaxLength)]
		[ResourceStringData("F9A7CDDD-DE1F-4B23-9818-01E7B9B2B4EE", Caption = "Reference Number")]
		public ZString CertificateOfOriginNo
		{
			get
			{
				return CertificateOfOriginData?.CSI_ReferenceNumber ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_ReferenceNumber = value;

				if (IsImport && !IsValidationSuspended)
				{
					((IMPJobComInvoiceLineValidation)Validation).ValidateCertificateOfOriginNo();
				}
				CertificateOfOriginNoInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginNoInfo => GetZPropertyInfo(nameof(CertificateOfOriginNo));

		[ResourceStringData("4E7D07EF-B356-447A-91A9-84B9BBE759F6", Caption = "C/O Seq. No.")]
		public ZInt CertificateOfOriginLineNo
		{
			get
			{
				return CertificateOfOriginData?.CSI_LineNo ?? ZInt.Zero;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_LineNo = value;

				if (IsImport && !IsValidationSuspended)
				{
					((IMPJobComInvoiceLineValidation)Validation).ValidateCertificateOfOriginLineNo();
				}
				CertificateOfOriginLineNoInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginLineNoInfo => GetZPropertyInfo(nameof(CertificateOfOriginLineNo));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.InvoiceUQList))]
		[MaxLength(Schema.CertificateOfOriginUQMaxLength)]
		public ZString CertificateOfOriginUQ
		{
			get
			{
				return CertificateOfOriginData?.CSI_UnitOfQuantity ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_UnitOfQuantity = value;

				if (IsImport && !IsValidationSuspended)
				{
					((IMPJobComInvoiceLineValidation)Validation).ValidateCertificateOfOriginUQ();
				}
				CertificateOfOriginUQInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginUQInfo => GetZPropertyInfo(nameof(CertificateOfOriginUQ));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CountryOfOriginDeterminationRuleCodeList))]
		[MaxLength(Schema.CertificateOfOriginCriteriaCodeMaxLength)]
		[ResourceStringData("60B2D84E-7DB8-4ED1-A0ED-3ACE93E22705", Caption = "C/O Criteria Code")]
		public ZString CertificateOfOriginCriteriaCode
		{
			get
			{
				return CertificateOfOriginData?.CSI_Procedure ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_Procedure = value;

				if (IsImport && !IsValidationSuspended)
				{
					((IMPJobComInvoiceLineValidation)Validation).ValidateCertificateOfOriginCriteriaCode();
				}
				CertificateOfOriginCriteriaCodeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginCriteriaCodeInfo => GetZPropertyInfo(nameof(CertificateOfOriginCriteriaCode));
		public void AddAllNotificationsFromCertificateOfOriginCSI_Procedure()
		{
			if (CertificateOfOriginData != null)
			{
				CertificateOfOriginCriteriaCodeInfo.AddAllNotificationsFrom(CertificateOfOriginData.CSI_ProcedureInfo);
			}
		}

		[ResourceStringData("8E192CD6-2545-46E4-A6E7-EA6A6D4D7BCB", Caption = "C/O Issue Date")]
		public ZDateTime CertificateOfOriginIssueDate
		{
			get
			{
				return CertificateOfOriginData?.CSI_DateOfIssue ?? ZDateTime.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_DateOfIssue = value;

				if (IsImport && !IsValidationSuspended)
				{
					((IMPJobComInvoiceLineValidation)Validation).ValidateCertificateOfOriginIssueDate();
				}
				CertificateOfOriginIssueDateInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginIssueDateInfo => GetZPropertyInfo(nameof(CertificateOfOriginIssueDate));

		[MaxLength(Schema.CertificateOfOriginIssuingCountryMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CountryOfOrigins))]
		[ResourceStringData("3DF39EF9-D541-48D8-94AD-55D9C04A1BD8", Caption = "C/O Issuing Country")]
		public ZString CertificateOfOriginIssuingCountry
		{
			get
			{
				return CertificateOfOriginData?.CSI_RN_NKCountryCode ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_RN_NKCountryCode = value;

				if (IsImport && !IsValidationSuspended)
				{
					((IMPJobComInvoiceLineValidation)Validation).ValidateCertificateOfOriginIssuingCountry();
				}
				CertificateOfOriginIssuingCountryInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginIssuingCountryInfo => GetZPropertyInfo(nameof(CertificateOfOriginIssuingCountry));
		public void AddAllNotificationsFromCertificateOfOriginCSI_RN_NKCountryCode()
		{
			if (CertificateOfOriginData != null)
			{
				CertificateOfOriginIssuingCountryInfo.AddAllNotificationsFrom(CertificateOfOriginData.CSI_RN_NKCountryCodeInfo);
			}
		}

		[MaxLength(Schema.CertificateOfOriginAgencyNameMaxLength)]
		[ResourceStringData("6CC115C9-60D4-4050-A071-166C906B9676", Caption = "Issuing Agency Name")]
		public ZString CertificateOfOriginAgencyName
		{
			get
			{
				return CertificateOfOriginData?.CSI_Description ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CheckMaximumLength(CertificateOfOriginAgencyNameInfo, value);
				CertificateOfOriginData.CSI_Description = value;

				if (IsImport && !IsValidationSuspended)
				{
					((IMPJobComInvoiceLineValidation)Validation).ValidateCertificateOfOriginAgencyName();
				}
				CertificateOfOriginAgencyNameInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginAgencyNameInfo => GetZPropertyInfo(nameof(CertificateOfOriginAgencyName));

		[MaxLength(Schema.CertificateOfOriginAreaNameMaxLength)]
		[ResourceStringData("ACC3C9D1-8C90-41A7-A029-6C256D339A4F", Caption = "Issuing Area Name")]
		public ZString CertificateOfOriginAreaName
		{
			get
			{
				return CertificateOfOriginData?.CSI_AdditionalDescription ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_AdditionalDescription = value;

				if (IsImport && !IsValidationSuspended)
				{
					((IMPJobComInvoiceLineValidation)Validation).ValidateCertificateOfOriginAreaName();
				}
				CertificateOfOriginAreaNameInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginAreaNameInfo => GetZPropertyInfo(nameof(CertificateOfOriginAreaName));

		[MaxLength(Schema.CertificateOfOriginPersonNameMaxLength)]
		[ResourceStringData("DAA2794E-F0A3-41F1-AAE6-9843A6F2AADB", Caption = "Issuing Person Name")]
		public ZString CertificateOfOriginPersonName
		{
			get
			{
				return CertificateOfOriginData?.CSI_ReferenceNumber2 ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_ReferenceNumber2 = value;

				if (IsImport && !IsValidationSuspended)
				{
					((IMPJobComInvoiceLineValidation)Validation).ValidateCertificateOfOriginPersonName();
				}
				CertificateOfOriginPersonNameInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginPersonNameInfo => GetZPropertyInfo(nameof(CertificateOfOriginPersonName));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CertificateOfOriginSplitCodeList))]
		[MaxLength(Schema.CertificateOfOriginStatusMaxLength)]
		[ResourceStringData("CFEBB103-2128-4720-A158-AEBF4F0BB07C", Caption = "C/O Split Y/N")]
		public ZString CertificateOfOriginStatus
		{
			get
			{
				return CertificateOfOriginData?.CSI_Status ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_Status = value;

				if (IsImport && !IsValidationSuspended)
				{
					((IMPJobComInvoiceLineValidation)Validation).ValidateCertificateOfOriginStatus();
				}
				CertificateOfOriginStatusInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CertificateOfOriginStatusInfo => GetZPropertyInfo(nameof(CertificateOfOriginStatus));

		[ResourceStringData("{2A9A6F06-3C13-452D-ACBC-2B560CCAAD73}", Caption = "3rd Country")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.GoodsDestination))]
		public override ZString JI_RN_NKSecondCommercialInvoiceCountry
		{
			get => base.JI_RN_NKSecondCommercialInvoiceCountry;
			set => base.JI_RN_NKSecondCommercialInvoiceCountry = value;
		}

		[MaxLength(1)]
		[ResourceStringData("E506C276-060B-462C-9384-11A4788F51FB", Caption = "3rd Country Inv. Issued")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.YesNoList))]
		public ZString IssuedInThirdCountry => JI_RN_NKSecondCommercialInvoiceCountry.IsEmpty ? YesNoList.Codes.No : YesNoList.Codes.Yes;

		[ResourceStringData("B8E8ACAA-4961-4FE7-8968-B46A1FA3451B", Caption = "C/O Exporter Number")]
		public ZString CountryOfOriginExporterNumber
		{
			get
			{
				var result = ZString.Empty;
				var supplier = InvoiceHeader?.Supplier;
				if (supplier != null)
				{
					result = supplier.CustomsCodes?.Cast<OrgCusCode>()?.FirstOrDefault(x => x.OK_CodeType == IdentificationType.CertificateOfOriginExporterNumber)?.OK_CustomsRegNo ?? ZString.Empty;
				}
				return result;
			}
		}

		[MaxLength(1)]
		[ResourceStringData("93A4CFDD-FA7A-489D-8E62-5965E09116C8", Caption = "C/O Issuer Type")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CertifiticateOfOriginIssuedTypeList))]
		public ZString COOIssuerType
		{
			get
			{
				return CertificateOfOriginData?.CSI_IssuerType ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_IssuerType = value;
				COOIssuerTypeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo COOIssuerTypeInfo => GetZPropertyInfo(nameof(COOIssuerType));

		[DecimalPlaces(DecimalPlacesConstants.Weight)]
		[ResourceStringData("4BB3A144-9928-4DD4-BE86-C9D26B4EAA4D", Caption = "C/O Total Net Weight")]
		public ZDecimal COOTotalNetWeight
		{
			get
			{
				return CertificateOfOriginData?.CSI_Quantity2 ?? ZDecimal.Zero;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				CertificateOfOriginData.CSI_Quantity2 = value;
				COOTotalNetWeightInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo COOTotalNetWeightInfo => GetZPropertyInfo(nameof(COOTotalNetWeight));

		[MaxLength(3)]
		[ResourceStringData("B59FE5C1-70A4-403D-90BB-25E1A5C20E52", Caption = "UQ")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.WeightUQList))]
		public ZString COOTotalNetWeightUQ
		{
			get
			{
				return CertificateOfOriginData?.CSI_UnitOfQuantity2 ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				if (CertificateOfOriginData.CSI_UnitOfQuantity2 != value)
				{
					CheckMaximumLength(COOTotalNetWeightUQInfo, value);
					CertificateOfOriginData.CSI_UnitOfQuantity2 = value;
					COOTotalNetWeightUQInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo COOTotalNetWeightUQInfo => GetZPropertyInfo(nameof(COOTotalNetWeightUQ));

		[ResourceStringData("FEBF086B-D187-48F0-90E9-5B53D39417DD", Caption = "C/O Line No.")]
		public ZInt COOLineNumber
		{
			get => CertificateOfOriginData?.CSI_LineNo ?? ZInt.Zero;
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				if (CertificateOfOriginData.CSI_LineNo != value)
				{
					CertificateOfOriginData.CSI_LineNo = value;
					COOLineNumberInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo COOLineNumberInfo => GetZPropertyInfo(nameof(COOLineNumber));

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.InvoiceUnitQuantityCodeList))]
		[ResourceStringData("1458F758-2A34-42AD-9806-681C1839391B", Caption = "UQ")]
		public ZString COOUsedQuantityUQ
		{
			get
			{
				return CertificateOfOriginData?.CSI_UnitOfQuantity ?? ZString.Empty;
			}
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				if (CertificateOfOriginData.CSI_UnitOfQuantity != value)
				{
					CheckMaximumLength(COOUsedQuantityUQInfo, value);
					CertificateOfOriginData.CSI_UnitOfQuantity = value;
					COOUsedQuantityUQInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo COOUsedQuantityUQInfo => GetZPropertyInfo(nameof(COOUsedQuantityUQ));

		public ZDecimal CertificateOfOriginTotalNetWeightInKG
		{
			get => CertificateOfOriginData != null ? Core.Constants.Weight.ConvertSafe(CertificateOfOriginData.CSI_Quantity2, CertificateOfOriginData.CSI_UnitOfQuantity2, Core.Constants.Weight.Kilograms) : ZDecimal.Zero;
		}

		[ResourceStringData("3706E81F-F559-4280-8376-9BD26EBB8877", Caption = "Packages")]
		public override ZInt JI_NoOfPacks { get => base.JI_NoOfPacks; set => base.JI_NoOfPacks = value; }

		[ResourceStringData("A0FC41A0-09E1-4B65-AC86-E109F8643E5D", Caption = "C/O Split Order")]
		public ZInt COOSplitOrder
		{
			get => CertificateOfOriginData?.CSI_ItemNumber ?? ZInt.Zero;
			set
			{
				CreateCertificateOfOriginDataIfRequired();
				if (CertificateOfOriginData.CSI_ItemNumber != value)
				{
					CertificateOfOriginData.CSI_ItemNumber = value;
					COOSplitOrderInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo COOSplitOrderInfo => GetZPropertyInfo(nameof(COOSplitOrder));

		[ResourceStringData("CD352B8B-0B05-4CDA-9A1C-4C6D3D867884", Caption = "C/O Supporting Doc Type")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CountryOfOriginSupportingDocTypeCodeList))]
		public override ZString JI_COOSupportingDocType => base.JI_COOSupportingDocType;

		[ResourceStringData("44BAF913-94E4-4EAB-82ED-654DFB70C0D0", Caption = "UQ")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PackageKindCodeList))]
		[MaxLength(2)]
		public override ZString JI_PackType { get => base.JI_PackType; set => base.JI_PackType = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CourierCargoSelectivityIndicatorCodeList))]
		[ResourceStringData("2A73B64A-87B3-4FB9-A86B-DD8F068BF320", Caption = "Express delivery company C/S")]
		public override ZString JI_CourierCargoSelectivityIndicator { get => base.JI_CourierCargoSelectivityIndicator; set => base.JI_CourierCargoSelectivityIndicator = value; }

		[ResourceStringData("66A19A17-5713-4585-977C-DA1E077BDD09", Caption = "Customs Unit Price")]
		[DecimalPlaces(Constants.DecimalPlacesConstants.UnitPrice)]
		public ZDecimal CustomsUnitPrice => (JI_CustomsQuantity == ZDecimal.Zero) ? ZDecimal.Zero : (ZDecimal)(JI_LinePrice / JI_CustomsQuantity);
		[ResourceStringData("EE648951-2D6E-44EE-823F-1968FF942558", Caption = "Price (USD)", MultipleKey = KRJobMessageTypeList.Codes.PersonalItems)]
		[ResourceStringData("35490B50-0BB8-42EB-AB9A-5E5AE24037B3", Caption = "Price")]
		public override ZDecimal JI_LinePrice { get => base.JI_LinePrice; set => base.JI_LinePrice = value; }
		public ZPropertyInfo CustomsUnitPriceInfo
		{
			get { return GetZPropertyInfo(nameof(CustomsUnitPrice)); }
		}
		public ZBool HasHSRequiringInvQuantityInCustomsUQ => UniversalTariff?.Attributes.Cast<TariffAttributeView>().Any(x => x.ZZ3_Name == Constants.ZZ.TariffAttributes.InvoiceQuantityInCU1) ?? false;

		[ChildEditable(true)]
		public NonGADetailCollection NonGADetailCollection
		{
			get
			{
				if (nonGADetailCollection == null)
				{
					nonGADetailCollection = new NonGADetailCollection(this);
					nonGADetailCollection.Load();
					RegisterEditableChildObject(nonGADetailCollection);
				}
				return nonGADetailCollection;
			}
		}
		NonGADetailCollection nonGADetailCollection;

		[ChildEditable(true)]
		public PreviousExpDecLineCollection PreviousExpDecLineCollection
		{
			get
			{
				if (previousExpDecLineCollection == null)
				{
					previousExpDecLineCollection = new PreviousExpDecLineCollection(this);
					previousExpDecLineCollection.Load();
					RegisterEditableChildObject(previousExpDecLineCollection);
				}
				return previousExpDecLineCollection;
			}
		}
		PreviousExpDecLineCollection previousExpDecLineCollection;

		public override void Delete()
		{
			base.Delete();
			VehicleNumbers.RemoveAndDeleteAll();
			Declaration?.RefreshInvoiceLinesEligibleForSimpleDrawback();
		}

		[ChildEditable(true)]
		public VehicleNumberCollection VehicleNumbers
		{
			get
			{
				if (vehicleNumbers == null)
				{
					vehicleNumbers = new VehicleNumberCollection(this);
					vehicleNumbers.Load();
					RegisterEditableChildObject(vehicleNumbers);
				}
				return vehicleNumbers;
			}
		}
		VehicleNumberCollection vehicleNumbers;

		public void AddAllNotificationsFromSupportingDocumentCodeCSI_Code()
		{
			if (SupportingDocument != null)
			{
				SupportingDocumentCodeInfo.AddAllNotificationsFrom(SupportingDocument.CSI_CodeInfo);
			}
		}

		[ResourceStringData("0D538F43-F21E-49C7-BD62-903553AE5620", Caption = "Supporting Document Type")]
		[MaxLength(SupportingDocument.Schema.SUP_CodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.LocalExportDocumentTypeList))]
		public ZString SupportingDocumentCode
		{
			get
			{
				return SupportingDocument?.CSI_Code ?? ZString.Empty;
			}
			set
			{
				if (SupportingDocument == null)
				{
					SupportingDocumentCollection.AddNew();
				}
				SupportingDocument.CSI_Code = value;

				if (IsLocalExport && !IsValidationSuspended)
				{
					((LocalExportJobComInvoiceLineValidation)Validation).ValidateSupportingDocumentCode();
				}
				SupportingDocumentCodeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo SupportingDocumentCodeInfo => GetZPropertyInfo(nameof(SupportingDocumentCode));

		[ResourceStringData("FAC96470-C783-4381-A84B-10452D61E11A", Caption = "Supporting Document No.")]
		[MaxLength(SupportingDocument.Schema.SUP_ReferenceNumberMaxLength)]
		public ZString SupportingDocumentReferenceNumber
		{
			get
			{
				return SupportingDocument?.CSI_ReferenceNumber ?? ZString.Empty;
			}
			set
			{
				if (SupportingDocument == null)
				{
					SupportingDocumentCollection.AddNew();
				}
				SupportingDocument.CSI_ReferenceNumber = value;

				if (IsLocalExport && !IsValidationSuspended)
				{
					((LocalExportJobComInvoiceLineValidation)Validation).ValidateSupportingDocumentReferenceNumber();
				}
				SupportingDocumentReferenceNumberInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo SupportingDocumentReferenceNumberInfo => GetZPropertyInfo(nameof(SupportingDocumentReferenceNumber));

		[ChildEditable(true)]
		SupportingDocumentCollection SupportingDocumentCollection
		{
			get
			{
				if (supportingDocumentCollection == null)
				{
					supportingDocumentCollection = new SupportingDocumentCollection(this);
					supportingDocumentCollection.Load();
					RegisterEditableChildObject(supportingDocumentCollection);
				}
				return supportingDocumentCollection;
			}
		}
		SupportingDocumentCollection supportingDocumentCollection;
		SupportingDocument SupportingDocument => SupportingDocumentCollection.FirstOrDefault();

		public ZString AdditionalTariffCode
		{
			get
			{
				var result = HSExtensionCodeCollection.FirstOrDefault<HSExtensionCode>(x => x.ClassificationType == HsExtensionCodes.Description.Category && !x.CY_Code.IsEmpty)?.CY_Code ?? ZString.Empty;

				if (!result.IsEmpty)
				{
					HSExtensionCodeCollection
						.Where<HSExtensionCode>(x => x.ClassificationType != HsExtensionCodes.Description.Category && !x.CY_Code.IsEmpty).OrderBy(x => x.CY_Order)
						.ForEach(y => result += "-" + y.CY_Code);
				}

				return result;
			}
		}

		[ChildEditable(true)]
		public HSExtensionCodeCollection HSExtensionCodeCollection
		{
			get
			{
				if (hsExtensionCodeCollection == null)
				{
					hsExtensionCodeCollection = new HSExtensionCodeCollection(this);
					hsExtensionCodeCollection.Load();
					RegisterEditableChildObject(hsExtensionCodeCollection);
				}
				return hsExtensionCodeCollection;
			}
		}
		HSExtensionCodeCollection hsExtensionCodeCollection;

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.VehicleNumber, typeof(VehicleNumber));
			result.Add(CusCodeDataTypeList.Codes.ImmediateDelivery, typeof(ImmediateDelivery));
			result.Add(CusCodeDataTypeList.Codes.HsExtensionCode, typeof(HSExtensionCode));
			return result;
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		public IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type> {
				{ CusSupportingInfoTypeList.Codes.PreApproval, typeof(PreApproval) },
				{ CusSupportingInfoTypeList.Codes.GAApproval, typeof(GAApproval) },
				{ CusSupportingInfoTypeList.Codes.CertificateOfOrigin, typeof(CertificateOfOrigin) },
				{ CusSupportingInfoTypeList.Codes.NonGADetail, typeof(NonGADetail) },
				{ CusSupportingInfoTypeList.Codes.PreviousExpDecLine, typeof(PreviousExpDecLine) },
				{ CusSupportingInfoTypeList.Codes.SupportingDoc, typeof(SupportingDocument) }
			};
			return result;
		}

		#endregion
		ZBool IsSpecificUseCode()
		{
			var universalTariffConditionValue = UniversalTariff?.GetPostClearanceProcedureCondition(JI_PrimaryPreference) ?? ZString.Empty;
			return universalTariffConditionValue == Constants.ZZ.RefCusConditionValue.A || universalTariffConditionValue == Constants.ZZ.RefCusConditionValue.C;
		}

		protected override bool GetJI_CustomsQuantityReadOnly() => (IsExport || IsLocalExport) && JI_CustomsUnitQty.IsEmpty;
		protected override bool GetJI_CustomsUnitQtyInfoReadOnly() => true;

		protected override Customs.Business.TariffFormatter TariffFormatter => new TariffFormatter();

		protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.KoreaSouth;
		protected override Type TypeOfPartUsedCore => typeof(OrgSupplierPart);

		protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy() => new UniversalTariffCustomsUnitDefaultingStrategy<JobComInvoiceLine>();
		#region New Properties
		[ResourceStringData("B07BCAEC-42AB-4948-9718-0FF5E6D86827", Caption = "Duty Reduction Type")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.DutyReductionClassificationList))]
		public ZString DutyReductionClassificationCode
		{
			get
			{
				var result = ZString.Empty;
				ZString secondaryPreference = JI_SecondaryPreference;
				ZString installmentCode = JI_InstallmentCode;

				if (!installmentCode.IsEmpty && !secondaryPreference.IsEmpty)
				{
					result = ImportDutyReductionClassificationList.Codes.Invalid;
				}
				else if (JI_IsSpecificUseCode)
				{
					if (secondaryPreference.IsEmpty && installmentCode.IsEmpty)
					{
						result = ImportDutyReductionClassificationList.Codes.SpecificUseCodeOnly;
					}
					else if (secondaryPreference.IsEmpty && !installmentCode.IsEmpty ||
						!secondaryPreference.IsEmpty && installmentCode.IsEmpty)
					{
						result = ImportDutyReductionClassificationList.Codes.SpecificUseCodeAndDutyReductionOrInstallment;
					}
				}
				else
				{
					if (!secondaryPreference.IsEmpty && installmentCode.IsEmpty)
					{
						var dutyReductionExemptionTariff = DutyReductionExemptionTariff;
						if (dutyReductionExemptionTariff != null)
						{
							if (dutyReductionExemptionTariff.HasAttribute(Constants.ZZ.TariffAttributes.IsDutyExempt, Constants.YesNo.Yes))
							{
								result = ImportDutyReductionClassificationList.Codes.DutyExemption;
							}
							else
							{
								result = ImportDutyReductionClassificationList.Codes.DutyReduction;
							}
						}
					}
					else if (!installmentCode.IsEmpty && secondaryPreference.IsEmpty)
					{
						result = ImportDutyReductionClassificationList.Codes.InstallmentPayment;
					}
				}

				return result;
			}
		}

		[ResourceStringData("0037D9A8-92BD-4EB0-A9D4-A87BFCD9C848", Caption = "Duty Rate")]
		[DecimalPlaces(DecimalPlacesConstants.TaxRate)]
		public ZDecimal DutyRate => CusEntryLine?.DutyRate ?? ZDecimal.Zero;

		public ZString TaxClassification1
		{
			get
			{
				ZString result = ZString.Empty;
				if (DomesticTax != null)
				{
					var taxClassification = DomesticTaxClassification;
					switch (taxClassification)
					{
						case ChargeTypeList.Codes.LiquorTax:
							result = Constants.DomestictaxClassificationCode.LQT;
							break;
						case ChargeTypeList.Codes.TransportationTax:
							result = Constants.DomestictaxClassificationCode.TRT;
							break;
						default:
							result = JI_UseCode;
							break;
					}
				}
				return result;
			}
		}

		public ZString TaxClassification2
		{
			get
			{
				ZString result = ZString.Empty;
				if (DomesticTax != null)
				{
					if (JI_DomesticTaxExemptionCode.IsEmpty)
					{
						result = DomesticTaxPreference;
					}
					else
					{
						result = DomestictaxClassificationCode.Exempt;
					}
				}
				return result;
			}
		}

		[ResourceStringData("D046CE36-AAA9-4EB4-BBE4-82448E10AEEA", Caption = "Domestic Tax Type")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.DomesticTaxClassificationCodeList))]
		public ZString DomesticTaxType => TaxClassification1 + TaxClassification2;
		CusEntryLineFee DomesticTaxFee
		{
			get
			{
				CusEntryLineFee result = null;
				if (DomesticTax != null && CusEntryLine != null)
				{
					result = CusEntryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == DomesticTaxClassification);
				}
				return result;
			}
		}
		public ZDecimal DomesticTaxAmount => DomesticTaxFee?.CF_ChargeAmount ?? ZDecimal.Zero;

		[ResourceStringData("FB9CFCCE-5AEE-45C2-A82F-817E2D6847D7", Caption = "Domestic Tax Rate")]
		public ZDecimal DomesticTaxRate => DomesticTaxFee?.CF_Rate ?? ZDecimal.Zero;

		[ResourceStringData("A49DB404-F64E-4A18-B91E-760E710ECA25", Caption = "Duty Rate Type")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.DutyRateCodeList))]
		public ZString DutyRateCode
		{
			get
			{
				var result = ZString.Empty;

				if (UniversalDutyRate != null)
				{
					result = UniversalDutyRate.RateCode == Constants.ZZ.RateCodes.DutyAdValorem ? DutyRateCodeList.Codes._1 : DutyRateCodeList.Codes._3;
				}
				return result;
			}
		}

		public ZString DutyRateCodeDescription => Factory.GetCachedValue<DutyRateCodeList>().GetDescriptionFromCode(DutyRateCode);

		[ResourceStringData("DF61AC3D-F03E-4A9F-B495-C960F29A1115", Caption = "Duty Reduction Rate")]
		public ZDecimal DutyReductionRate
		{
			get
			{
				var result = ZDecimal.Zero;
				if (DutyReductionExemptionTariff != null)
				{
					var rate = DutyReductionExemptionTariff.Rates.SingleOrDefault();
					if (rate != null)
					{
						result = DutyCalculatorStrategy.GetRateNumeric(rate);
					}
				}

				return result;
			}
		}

		public ZString PreferenceCodeDescription => Preference?.ZZS_Description ?? string.Empty;

		CusRefPreferenceView Preference
		{
			get
			{
				if (preference == null)
				{
					var query = new ZDBOnlyQuery(typeof(CusRefPreferenceView));
					query.AddToFilter(CusRefPreferenceViewSchema.ZZS_Preference, JI_PrimaryPreference);
					query.AddToFilter(CusRefPreferenceViewSchema.ZZS_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.KoreaSouth);

					preference = Factory.LoadTop1<CusRefPreferenceView>(query);
				}
				return preference;
			}
		}
		CusRefPreferenceView preference;

		[ResourceStringData("11F18DCC-1485-4DF1-B184-77E964F48493", Caption = "Special Consumption Tax")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal JI_Calc_SpecialConsumptionTaxIncludingWHEstimate => (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.SpecialConsumptionTaxAmount).Amount;
		[ResourceStringData("D8489506-FF9C-415E-8885-12D4A2440B22", Caption = "Transportation Tax")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal JI_Calc_TransportationTaxIncludingWHEstimate => (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.TransportationTaxAmount).Amount;
		[ResourceStringData("53195FE3-9916-41C1-AE05-06BDDB83091E", Caption = "Liquor Tax")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal JI_Calc_LiquorTaxIncludingWHEstimate => (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.LiquorTaxAmount).Amount;
		[ResourceStringData("3CF0BB27-40FF-4637-8B8A-AF8F3193C60E", Caption = "Education Tax")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal JI_Calc_EducationTaxIncludingWHEstimate => (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.EducationTaxAmount).Amount;
		[ResourceStringData("C98295C3-050A-4B1D-B8E5-6D8621EA909C", Caption = "Agriculture Tax")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal JI_Calc_AgricultureTaxIncludingWHEstimate => (CusEntryLine == null) ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(CusEntryLine.AgricultureTaxAmount).Amount;

		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public override ZDecimal JI_Calc_GSTVATAmountIncludingWHEstimate => base.JI_Calc_GSTVATAmountIncludingWHEstimate;

		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public override ZDecimal JI_Calc_DutyAmountIncludingWHEstimate => base.JI_Calc_DutyAmountIncludingWHEstimate;

		#endregion

		public OrganizationDocWrapper Consignee => consignee ?? (consignee = new OrganizationDocWrapper(ConsigneeAddress));
		OrganizationDocWrapper consignee;

		int SerialNumberMaxLength => IsLocalExport ? 30 : 50;

		[ResourceStringData("F31453E4-0D80-4461-BE5A-09CA12EF1886", Caption = "Item ID")]
		[ResourceStringData("EF995AD6-A60C-493D-94ED-13001C1FEA2D", Caption = "Serial Number", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		[MaxLength(nameof(SerialNumberMaxLength))]
		public override ZString JI_SerialNumber
		{
			get => base.JI_SerialNumber;
			set => base.JI_SerialNumber = value;
		}

		#region AddInfo Field
		ZString[] SplitDomesticTaxCode => JI_DomesticTaxCode.Split("-");
		public ZString DomesticTaxPreference => SplitDomesticTaxCode.Length >= 2 ? SplitDomesticTaxCode[1] : ZString.Empty;
		public ZString DomesticTaxCode => SplitDomesticTaxCode.Length >= 1 ? SplitDomesticTaxCode[0] : ZString.Empty;

		void RenewCustomsUQ()
		{
			Dictionary<ZPropertyInfo, ZPropertyInfo> unitAndQuantityPairs = new()
			{
				{ JI_CustomsUnitQtyInfo, JI_CustomsQuantityInfo },
				{ JI_CustomsSecondUnitQtyInfo, JI_CustomsSecondQuantityInfo },
				{ JI_CustomsThirdUnitQtyInfo, JI_CustomsThirdQuantityInfo },
				{ JI_CustomsFourthUnitQtyInfo, JI_CustomsFourthQuantityInfo },
			};

			Dictionary<ZString, ZDecimal> oldCustomsQty = new();
			foreach (var oldPairs in unitAndQuantityPairs)
			{
				oldCustomsQty[(ZString)oldPairs.Key.Value] = (ZDecimal)oldPairs.Value.Value;
				SetUQAndQuantity(oldPairs.Key, oldPairs.Value, ZString.Empty);
			}

			List<ZString> currentUQs = new();
			if (UniversalTariff != null)
			{
				currentUQs.AddRange(UniversalTariff.UnitsOfMeasure.OrderBy(x => x.ZZ8_Type).Select(x => x.ZZ8_UOM));
			}

			if (DomesticTax != null)
			{
				currentUQs.AddRange(DomesticTax.UnitsOfMeasure.OrderBy(x => x.ZZ8_Type).Select(x => x.ZZ8_UOM));
			}

			if (DomesticTaxExemptionTariff != null)
			{
				currentUQs.AddRange(DomesticTaxExemptionTariff.UnitsOfMeasure.OrderBy(x => x.ZZ8_Type).Select(x => x.ZZ8_UOM));
			}

			var pair = unitAndQuantityPairs.GetEnumerator();
			foreach (var uq in currentUQs.Distinct())
			{
				while (pair.MoveNext() && !AssignUQIfEmpty(pair.Current.Key, pair.Current.Value, uq)) { }
			}

			bool AssignUQIfEmpty(ZPropertyInfo uqInfo, ZPropertyInfo qtyInfo, ZString uq)
			{
				if (uqInfo.Value.IsEmpty)
				{
					SetUQAndQuantity(uqInfo, qtyInfo, uq);
					return true;
				}
				return false;
			}

			void SetUQAndQuantity(ZPropertyInfo uqInfo, ZPropertyInfo qtyInfo, ZString uq)
			{
				uqInfo.Value = uq;
				oldCustomsQty.TryGetValue(uq, out var qty);
				qtyInfo.Value = qty;
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.InstallmentCodes))]
		[ResourceStringData("93D24848-A87E-4898-8F71-7942A280FB1D", Caption = "Instalment Code")]
		public override ZString JI_InstallmentCode
		{
			get => base.JI_InstallmentCode;
			set => base.JI_InstallmentCode = value;
		}

		[ResourceStringData("F04FBE19-3898-4B63-9D51-BA343B29CDC4", Caption = "Specific Use")]
		public override ZBool JI_IsSpecificUseCode
		{
			get => base.JI_IsSpecificUseCode;
			set => base.JI_IsSpecificUseCode = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.AdditionalDutyTypeCodeList))]
		[ResourceStringData("2A0AF54F-763E-49FE-9CE6-5ECABD74C43A", Caption = "Add. Duty Code")]
		public override ZString JI_AdditionalDutyType
		{
			get => base.JI_AdditionalDutyType;
			set => base.JI_AdditionalDutyType = value;
		}

		[ResourceStringData("FF68BCE4-C4B2-47DA-8912-C48B2627AB2A", Caption = "Add. Duty Rate")]
		public override ZDecimal JI_AdditionalDutyRate
		{
			get => base.JI_AdditionalDutyRate;
			set => base.JI_AdditionalDutyRate = value;
		}

		[ResourceStringData("51048B28-5C7E-4438-8156-8DB071BB7D78", Caption = "Specific Use Permit No")]
		public override ZString JI_SpecificUseCodeDutyRatePermitNo
		{
			get => base.JI_SpecificUseCodeDutyRatePermitNo;
			set => base.JI_SpecificUseCodeDutyRatePermitNo = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.DomesticTaxCodes))]
		[ResourceStringData("B8292DD0-F619-4D3D-97DD-6D438B64E3F3", Caption = "Domestic Tax Code")]
		public override ZString JI_DomesticTaxCode
		{
			get => base.JI_DomesticTaxCode;
			set
			{
				var oldValue = base.JI_DomesticTaxCode;
				base.JI_DomesticTaxCode = value;

				if (oldValue != JI_DomesticTaxCode && !IsCopying)
				{
					RenewCustomsUQ();
				}
			}
		}

		[ResourceStringData("4A73E2F9-8D03-4602-A462-FA716553F02E", Caption = "Covered By C/O Exporter System")]
		public override ZBool JI_CoveredByCOOExporter { get => base.JI_CoveredByCOOExporter; set => base.JI_CoveredByCOOExporter = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.TaxExemptionCodes))]
		[ResourceStringData("39D4D287-6986-47A5-9AC6-25B240763CFA", Caption = "Domestic Tax Exemption Code")]
		public override ZString JI_DomesticTaxExemptionCode
		{
			get => base.JI_DomesticTaxExemptionCode;
			set
			{
				var oldValue = JI_DomesticTaxExemptionCode;
				base.JI_DomesticTaxExemptionCode = value;
				if (oldValue != JI_DomesticTaxExemptionCode && !IsCopying)
				{
					RenewCustomsUQ();
				}
				if (!IsInstallationCostUsed)
				{
					JI_InstallationCost = ZDecimal.Zero;
				}
			}
		}

		[ResourceStringData("279F6CF1-4C4D-4FDF-BEBD-FC510548B155", Caption = "Installation Cost (KRW)")]
		[ReadOnlyMember(nameof(JI_InstallationCost_ReadOnly))]
		public override ZDecimal JI_InstallationCost
		{
			get => base.JI_InstallationCost;
			set => base.JI_InstallationCost = value;
		}

		bool JI_InstallationCost_ReadOnly => !IsInstallationCostUsed;

		bool IsInstallationCostUsed => (DomesticTaxExemptionTariff?.Attributes?.FirstOrDefault(x => x.ZZ3_Name == Constants.ZZ.TariffAttributeNames.InvolvesInstallationCost)?.ZZ3_Value ?? ZString.Empty) == YesNo.Yes;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.VATReductionCodes))]
		[ResourceStringData("63DD0440-C10E-4F89-A1D9-D3B2926FA7F6", Caption = "VAT Reduction Code")]
		[ResourceStringData("C17C5D46-D3AC-49F6-9CDC-F22CA0779C0E", Caption = "VAT Reduction Code", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		public override ZString JI_VATReductionCode
		{
			get => base.JI_VATReductionCode;
			set => base.JI_VATReductionCode = value;
		}

		[ResourceStringData("CC98A82A-0E14-433C-8DCB-A765AB5435EB", Caption = "VAT Rate Type")]
		[ResourceStringData("DBE322D6-C851-4542-B457-43F4E21F134D", Caption = "VAT Type", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		public override ZString JI_ZZF_NKTaxType
		{
			get => base.JI_ZZF_NKTaxType;
			set => base.JI_ZZF_NKTaxType = value;
		}

		[ResourceStringData("1A50A182-BBE4-412E-8F22-929401A0A627", Caption = "C/O Label Location")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CountryOfOriginLabelLocationCodeList))]
		[MaxLength(1)]
		public override ZString JI_COOLabelLocation
		{
			get => base.JI_COOLabelLocation;
			set => base.JI_COOLabelLocation = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CountryOfOriginLabelTypeCodeList))]
		[ResourceStringData("BFFF5615-941F-45F6-B028-63C84FA3E935", Caption = "C/O Label Type")]
		[ResourceStringData("0A54317B-FCB5-4753-9C98-088C7C2BCAFB", Caption = "C/O Label Type")]
		public override ZString JI_COOLabelType
		{
			get => base.JI_COOLabelType;
			set => base.JI_COOLabelType = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CountryOfOriginExemptionReasonCodeList))]
		[ResourceStringData("32CAD2BB-46BA-418C-A7A5-C8A7B72C87B8", Caption = "C/O Label Exemption Reason")]
		[ResourceStringData("8BC83427-092F-4482-9B0A-26249D52D4B6", Caption = "C/O Label Exemption Reason")]
		public override ZString JI_COOExemptionReason
		{
			get => base.JI_COOExemptionReason;
			set => base.JI_COOExemptionReason = value;
		}

		int IngredientMaxLength => IsLocalExport ? 15 : JobComInvoiceLine.Schema.JI_IngredientMaxLength;
		[ResourceStringData("7C4EBA75-0476-4378-A409-4A821A1925B7", Caption = "Ingredient")]
		[ResourceStringData("E60A342A-E08C-4181-9FB8-D903DFDF6055", Caption = "Material Code", MultipleKey = KRJobMessageTypeList.Codes.LocalExport)]
		[MaxLength(nameof(IngredientMaxLength))]
		public override ZString JI_Ingredient
		{
			get => base.JI_Ingredient;
			set => base.JI_Ingredient = value;
		}

		[ResourceStringData("B243D3E0-50F1-486A-86B3-E1E42AEA02E9", Caption = "Invoice Line No.")]
		[ResourceStringData("92D54E46-3C25-4B83-B1BB-C2144F357122", Caption = "Invoice Line No.", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		[ReadOnly(true)]
		public override ZShort JI_SequenceNumber
		{
			get => base.JI_SequenceNumber;
			set => base.JI_SequenceNumber = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SkipManifestReportingCodeList))]
		[ResourceStringData("6D0614B4-523B-4497-95F6-6C24D1FA6D13", Caption = "Is Empty Container", FullDescription = "Is Empty Container (Exempt from Manifest reporting)")]
		public override ZString JI_SkipManifestReport
		{
			get => base.JI_SkipManifestReport;
			set => base.JI_SkipManifestReport = value;
		}

		[MaxLength(Schema.JI_LotNumberMaxLength)]
		[ResourceStringData("8E6FEF5E-36A2-41BD-A957-C8F8235C9DE4", Caption = "Lot Number")]
		[ResourceStringData("F7BE0913-A436-445C-BCC7-5E6DD7C7169F", Caption = "Lot Number", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		public override ZString JI_LotNumber
		{
			get => base.JI_LotNumber;
			set => base.JI_LotNumber = value;
		}

		[ResourceStringData("F7985FFE-D4E7-4390-8088-026A7C4FCDA0", Caption = "Inbound Date")]
		public override ZDateTime JI_InboundDate
		{
			get => base.JI_InboundDate;
			set => base.JI_InboundDate = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.OriginalStateDocTypeList))]
		[ResourceStringData("DA6EB8DB-9D58-4767-86D6-0C091A283749", Caption = "Previous Document Type ")]
		public override ZString JI_OriginalStateDocType
		{
			get => base.JI_OriginalStateDocType;
			set => base.JI_OriginalStateDocType = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.BrandCodes))]
		[ResourceStringData("A7308387-0855-4D85-92BC-9136C933B71D", Caption = "Brand Code")]
		public override ZString JI_BrandCode { get => base.JI_BrandCode; set => base.JI_BrandCode = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SpecificUseProductTypeList))]
		[ResourceStringData("42F34943-C66E-4706-B76D-BD5EF9D35F28", Caption = "Product Type")]
		public override ZString JI_SpecificUseProductType { get => base.JI_SpecificUseProductType; set => base.JI_SpecificUseProductType = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ProductOrMaterialCodeList))]
		[ResourceStringData("58AFF4FA-01DB-49CF-8F09-B5211C617760", Caption = "Product Or Material")]
		[ResourceStringData("5CE50695-ACDF-403A-887C-40EC61AB5FEF", Caption = "Item Category", MultipleKey = KRJobMessageTypeList.Codes.PersonalItems)]
		public override ZString JI_ProductTypeCode
		{
			get => base.JI_ProductTypeCode;
			set => base.JI_ProductTypeCode = value;
		}

		[DecimalPlaces(Constants.DecimalPlacesConstants.DrawbackQuantity)]
		[ResourceStringData("9E610B11-F5BB-470F-92A4-1FB32F1D8BFB", Caption = "Drawback Qty")]
		public override ZDecimal JI_DrawbackQuantity { get => base.JI_DrawbackQuantity; set => base.JI_DrawbackQuantity = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.InvoiceUnitQuantityCodeList))]
		[ResourceStringData("77FB7C62-AF45-4070-B443-4AAA44D478FD", Caption = "UQ")]
		public override ZString JI_DrawbackUQ
		{
			get => base.JI_DrawbackUQ;
			set => base.JI_DrawbackUQ = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.OtherGovernmentAndAssociatedAgencyList))]
		[ResourceStringData("D5937B3A-5AA6-4965-9A37-19AAF3BCD22D", Caption = "PC Agency 1")]
		public override ZString JI_PostClearanceProcedureGA1
		{
			get => base.JI_PostClearanceProcedureGA1;
			set => base.JI_PostClearanceProcedureGA1 = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.OtherGovernmentAndAssociatedAgencyList))]
		[ResourceStringData("DDAD8296-93D4-4BEC-9D7C-7D04A2E738D8", Caption = "PC Agency 2")]
		public override ZString JI_PostClearanceProcedureGA2
		{
			get => base.JI_PostClearanceProcedureGA2;
			set => base.JI_PostClearanceProcedureGA2 = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.OtherGovernmentAndAssociatedAgencyList))]
		[ResourceStringData("41F8D2AA-45B3-468C-8575-C855E8409DB8", Caption = "PC Agency 3")]
		public override ZString JI_PostClearanceProcedureGA3
		{
			get => base.JI_PostClearanceProcedureGA3;
			set => base.JI_PostClearanceProcedureGA3 = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.MightRequireInspectionIndicatorCodeList))]
		[ResourceStringData("57DCB598-78BC-4246-BC92-338AB21E094D", Caption = "Declarant Require Inspection")]
		public override ZString JI_MightRequireInspection
		{
			get => base.JI_MightRequireInspection;
			set => base.JI_MightRequireInspection = value;
		}

		[MaxLength(3)]
		[ReadOnlyMember(nameof(IsDutySelectionReadOnly))]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.DutyRateSelectionList))]
		[ResourceStringData("102E5AAD-A65F-488D-8389-98C5B672C515", Caption = "MIN/MAX Duty")]
		public override ZString JI_DutyRateSelection
		{
			get => base.JI_DutyRateSelection;
			set => base.JI_DutyRateSelection = value;
		}

		bool IsDutySelectionReadOnly => Lookups.DutyRateSelectionList.Count <= 1;

		[ResourceStringData("3FB75306-1E86-429D-A3B0-8BD7EEF86665", Caption = "Use Code Description")]
		public override ZString JI_SpecificUseCodeDescription
		{
			get => base.JI_SpecificUseCodeDescription;
			set => base.JI_SpecificUseCodeDescription = value;
		}

		[ResourceStringData("5C2E45C8-E0A0-4F74-A04D-A49C63D3C8B3", Caption = "Customs Office")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsOfficeList))]
		public override ZString JI_JurisdictionalCusOffice
		{
			get => base.JI_JurisdictionalCusOffice;
			set => base.JI_JurisdictionalCusOffice = value;
		}

		[ResourceStringData("24654934-DDA8-4294-9426-C8C0DC8503DE", Caption = "Customs Office")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsOfficeList))]
		public override ZString JI_ScheduledReExportCustomsOffice
		{
			get => base.JI_ScheduledReExportCustomsOffice;
			set => base.JI_ScheduledReExportCustomsOffice = value;
		}

		[ResourceStringData("5118C6F0-35B0-4AAF-8C9C-12379FA25F3B", Caption = "Dest. Country")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.GoodsDestination))]
		public override ZString JI_RN_NKReExportDestinationCountry
		{
			get => base.JI_RN_NKReExportDestinationCountry;
			set => base.JI_RN_NKReExportDestinationCountry = value;
		}

		[ResourceStringData("5CEAD7D9-93EF-4696-A0B0-5AEAAB1939A1", Caption = "Estimate Date")]
		public override ZDateTime JI_ScheduledReExportDate
		{
			get => base.JI_ScheduledReExportDate;
			set => base.JI_ScheduledReExportDate = value;
		}

		[ResourceStringData("1FBB68A2-7D22-401C-BDAF-7A0FF230920B", Caption = "Post Clearance YN")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PostClearanceYNCodeList))]
		public override ZString JI_PCProcedure { get => base.JI_PCProcedure; set => base.JI_PCProcedure = value; }
		#endregion

		public IZZRateSelectionCriteria DutyReductionRateSelectionCriteria => (dutyReductionRateSelectionCriteria ?? (dutyReductionRateSelectionCriteria = new CachedProperty<IZZRateSelectionCriteria>(Factory, DutyReductionRateSelectionCriteriaCore))).Value;
		CachedProperty<IZZRateSelectionCriteria> dutyReductionRateSelectionCriteria;

		IZZRateSelectionCriteria DutyReductionRateSelectionCriteriaCore() => new RateSelectionCriteria<JobComInvoiceLine>(this, Constants.ZZ.TariffTypes.DutyReductionExemption, ZString.Empty);

		protected override ZString AdditionalCode => JI_DutyRateSelection;

		JobDeclaration ISupportingDocumentParent.Declaration => Declaration;
		IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => new string[] { Declaration?.JE_MessageType };

		public bool IsLocalExport => InvoiceHeader?.IsLocalExport ?? false;

		public bool IsD87 => InvoiceHeader?.IsD87 ?? false;
		public bool IsPersonalItemDeclaration => InvoiceHeader?.IsPersonalItemDeclaration ?? false;
		public bool Is5SM => InvoiceHeader?.Is5SM ?? false;
		public bool IsEligibleForSimpleDrawback
		{
			get
			{
				var result = false;
				var refCusCondition = UniversalTariff?.Conditions?.Where(x => x.ZX1_IsExport == true && x.ConditionType == Constants.ZZ.RefCusConditionType.SimpleDrawback && x.ZX1_StartDate <= EffectiveAssessmentDate && x.ZX1_EndDate >= EffectiveAssessmentDate);
				if (refCusCondition?.Count() > 0)
				{
					result = true;
				}
				return result;
			}
		}

		[ResourceStringData("D078EEAF-C494-4D09-9B7C-D2A4548F3CFA", Caption = "Domestic Tax Base Qty Or Price")]
		public ZDecimal DomesticTaxBaseQtyOrPrice
		{
			get
			{
				var result = ZDecimal.Zero;
				var unit = DomesticTaxBaseUnit;
				if (IsInstallationCostUsed)
				{
					result = JI_InstallationCost;
				}
				else
				{
					if (!unit.IsEmpty)
					{
						if (JI_CustomsUnitQty == unit)
						{
							result = JI_CustomsQuantity;
						}
						else if (JI_CustomsSecondUnitQty == unit)
						{
							result = JI_CustomsSecondQuantity;
						}
						else if (JI_CustomsThirdUnitQty == unit)
						{
							result = JI_CustomsThirdQuantity;
						}
						else if (JI_CustomsFourthUnitQty == unit)
						{
							result = JI_CustomsFourthQuantity;
						}
					}
				}

				return result;
			}
		}

		[ResourceStringData("8562F4B1-1BC4-4B60-BBE2-535736BABCCC", Caption = "UQ")]
		public ZString DomesticTaxBaseUnit
		{
			get
			{
				var result = ZString.Empty;

				if (!JI_DomesticTaxCode.IsEmpty && DomesticTaxClassification == ChargeTypeList.Codes.LiquorTax)
				{
					result = Constants.DomesticTaxBaseQtyOrPriceCode.UQs.AlcoholContent;
				}
				else
				{
					var uom = DomesticTax?.UnitsOfMeasure.FirstOrDefault(x => !x.ZZ8_UOM.IsEmpty)?.ZZ8_UOM ?? ZString.Empty;
					if (!uom.IsEmpty)
					{
						result = uom;
					}
					else if (JI_Tariff == Constants.DomesticTaxBaseQtyOrPriceCode.HSCodeInMinutes)
					{
						result = Constants.DomesticTaxBaseQtyOrPriceCode.UQs.Minutes;
					}
				}
				return result;
			}
		}

		[ResourceStringData("8FBA61CE-C3B5-4650-BB1F-1CEFE2CE36F5", Caption = "Education Tax Type")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.EducationTaxTypeCodeList))]
		public ZString EducationTaxExemptIndicator
		{
			get
			{
				var result = ZString.Empty;
				if (!JI_DomesticTaxCode.IsEmpty)
				{
					var taxRateByEDT = DomesticTax?.Rates?.FirstOrDefault(x => x.ZZ2_ZZR_RateTypeCode == ChargeTypeList.Codes.EducationTax);
					if (taxRateByEDT != null)
					{
						if (JI_DomesticTaxExemptionCode.IsEmpty)
						{
							result = EducationTaxTypeCodeList.Codes.A;
						}
						else
						{
							result = EducationTaxTypeCodeList.Codes.B;
						}
					}
				}
				return result;
			}
		}

		[ResourceStringData("31817643-033D-45D5-8A23-8F34AFC70C2F", Caption = "Agriculture Tax Type")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.AgricultureTaxTypeCodeList))]
		public ZString AgricultureTaxClassification
		{
			get
			{
				var result = ZString.Empty;
				if (!JI_SecondaryPreference.IsEmpty || !JI_DomesticTaxCode.IsEmpty)
				{
					var secondaryPreferenceTariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.TariffTypes.DutyReductionExemption, JI_SecondaryPreference, EffectiveAssessmentDate);
					var taxAttributeAApplies = secondaryPreferenceTariff?.Attributes.FirstOrDefault(x => x.ZZ3_Name == Constants.ZZ.TariffAttributes.AgricultureTaxAApplies);

					var taxAttributeBApplies = DomesticTax?.Attributes.FirstOrDefault(x => x.ZZ3_Name == Constants.ZZ.TariffAttributes.AgricultureTaxBApplies);

					if (taxAttributeAApplies == null && taxAttributeBApplies == null)
					{
						result = ZString.Empty;
					}
					else if (taxAttributeAApplies == null)
					{
						result = Constants.AgricultureTaxClassification.OnSpecialConsumptionTax;
					}
					else if (taxAttributeBApplies == null)
					{
						result = Constants.AgricultureTaxClassification.OnDutyExemptionReduction;
					}
					else
					{
						result = Constants.AgricultureTaxClassification.OnBoth;
					}
				}

				return result;
			}
		}

		public TariffView DomesticTaxExemptionTariff => new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.TariffTypes.DomesticTaxReductionExemption, JI_DomesticTaxExemptionCode, EffectiveAssessmentDate);
		public TariffView DomesticTax => new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.TariffTypes.DomesticTaxRate, JI_DomesticTaxCode, EffectiveAssessmentDate);

		public ZString DomesticTaxClassification => DomesticTax?.GetAttribute(Constants.ZZ.TariffAttributes.TaxClassification1)?.ZZ3_Value ?? ZString.Empty;

		[ChildEditable]
		public ImmediateDeliveryCollection ImmediateDeliveries
		{
			get
			{
				if (immediateDeliveries == null)
				{
					immediateDeliveries = new ImmediateDeliveryCollection(this);
					immediateDeliveries.Load();
					RegisterEditableChildObject(immediateDeliveries);
				}
				return immediateDeliveries;
			}
		}
		ImmediateDeliveryCollection immediateDeliveries;

		[ResourceStringData("731E9EEF-F09A-42D1-A314-9AB3F5A9A4EB", Caption = "Goods Location")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ConsigneeAddress))]
		public override ZGuid JI_OA_ConsigneeAddress
		{
			get => base.JI_OA_ConsigneeAddress;
			set => base.JI_OA_ConsigneeAddress = value;
		}

		[MaxLength(2)]
		[ResourceStringData("BC29666C-BB00-4FFD-94F2-85C78553AA65", Caption = "Group Number")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ReductionRateRegulationList))]
		public ZString DutyReductionGroupNumber
		{
			get
			{
				return GetDutyReductionNumber(0);
			}
			set
			{
				SetDutyReductionNumber(value, 0);
			}
		}

		[MaxLength(3)]
		[ResourceStringData("3C62F0BC-F3A3-46D5-BBC2-6589CCBB3744", Caption = "Seq. Number")]
		public ZString DutyReductionSeqNumber
		{
			get
			{
				return GetDutyReductionNumber(1);
			}
			set
			{
				CheckMaximumLength(DutyReductionSeqNumberInfo, value);
				SetDutyReductionNumber(value, 1);
				DutyReductionSeqNumberInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo DutyReductionSeqNumberInfo => GetZPropertyInfo(nameof(DutyReductionSeqNumber));

		[MaxLength(2)]
		[ResourceStringData("83ECBFE6-CB50-46F9-8935-40CBEDDF13D3", Caption = "Item Number")]
		public ZString DutyReductionItemNumber
		{
			get
			{
				return GetDutyReductionNumber(2);
			}
			set
			{
				CheckMaximumLength(DutyReductionItemNumberInfo, value);
				SetDutyReductionNumber(value, 2);
				DutyReductionItemNumberInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo DutyReductionItemNumberInfo => GetZPropertyInfo(nameof(DutyReductionItemNumber));

		ZString GetDutyReductionNumber(int index)
		{
			if (JI_DutyReductionRateRegulationCode.IsEmpty)
			{
				return ZString.Empty;
			}

			var splitNumbers = new List<ZString>();
			ZString[] dutyReductionNumberSplit = JI_DutyReductionRateRegulationCode.Split(Constants.ImportDutyReductionRateRegulationCodeSplitor);
			foreach (var splitNumber in dutyReductionNumberSplit)
			{
				splitNumbers.Add(splitNumber);
			}
			return splitNumbers[index];
		}

		void SetDutyReductionNumber(ZString value, int index)
		{
			var splitNumbers = JI_DutyReductionRateRegulationCode.Split(ImportDutyReductionRateRegulationCodeSplitor).ToList();
			if (splitNumbers.Count < 3)
			{
				splitNumbers.AddRange(Enumerable.Repeat(ZString.Empty, 3 - splitNumbers.Count));
			}
			splitNumbers[index] = value;
			JI_DutyReductionRateRegulationCode = string.Join(ImportDutyReductionRateRegulationCodeSplitor, splitNumbers);
		}

	protected override ZBool IsPreviousEntryNumberVisibleCore => true;
		public bool Is008 => InvoiceHeader?.Is008 ?? false;

		public bool IsFTAPreference
		{
			get
			{
				var result = false;
				var firstPreference = JI_PrimaryPreference.SubstringSafe(0, 1);
				if (firstPreference == Constants.ZZ.Preferences.AlphabetF)
				{
					if (JI_PrimaryPreference != Constants.ZZ.Preferences.GeneralInternationalPreference && JI_PrimaryPreference != Constants.ZZ.Preferences.GeneralInternationalPreference1)
					{
						result = true;
					}
				}
				return result;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobComInvoiceLineFetchStrategy(this);

		protected override string ChargeCodeForOverseasFreight => IsImport ? ImportChargeMethodCodeList.GetFreightCode(InvoiceHeader.JZ_ValuationCode) : base.ChargeCodeForOverseasFreight;
		protected override string ChargeCodeForOverseasInsurance => IsImport ? ImportChargeMethodCodeList.GetInsuranceCode(InvoiceHeader.JZ_ValuationCode) : base.ChargeCodeForOverseasInsurance;
	}
}

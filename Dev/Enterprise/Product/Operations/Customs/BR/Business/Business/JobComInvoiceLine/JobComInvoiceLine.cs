using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.BR.Business.Constants;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public partial class JobComInvoiceLine : AutoBRJobComInvoiceLine
		, ICusSupportingInfoTypeSupporter
		, IChargeApportionee
		, ICusCodeDataTypeSupporter
		, IDocAddresses
		, ICanDelete
		, ITariffDetachParent
		, IAdditionalTariffParent
		, ISupportMultipleResourceStringData
		, IICMSCalculationParameters
		, IICMSCalculationValues
		, IAttributeCusCodeDataParent
	{
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void MarkAsNeedingValidationCore()
		{
			base.MarkAsNeedingValidationCore();
			LPCOJobComInvLineRefsCollection.MarkAsNeedingValidation();
			TariffDetachs.MarkAsNeedingValidation();
		}

		#region Constants

		#region Schema

		public new class Schema : AutoBRJobComInvoiceLine.Schema
		{
			public const string ComplementaryDescription = nameof(JobComInvoiceLine.ComplementaryDescription);
			public const string FullGoodsDescription = nameof(JobComInvoiceLine.FullGoodsDescription);
			public const string LPCOConcatenated = nameof(JobComInvoiceLine.LPCOConcatenated);
			public const string TariffDetachConcatenated = nameof(JobComInvoiceLine.TariffDetachConcatenated);
			public const string NaladiNcca = nameof(JobComInvoiceLine.NaladiNcca);
			public const string NaladiHs = nameof(JobComInvoiceLine.NaladiHs);
			public const string MercosulForeignDeclarationType = nameof(JobComInvoiceLine.MercosulForeignDeclarationType);
			public const string ConsentingProcessConcatenated = nameof(JobComInvoiceLine.ConsentingProcessConcatenated);
			public const string DrawbackCANumber = nameof(JobComInvoiceLine.DrawbackCANumber);
			public const string DrawbackItemNumber = nameof(JobComInvoiceLine.DrawbackItemNumber);
			public const string DrawbackModality = nameof(JobComInvoiceLine.DrawbackModality);
			public const string ManufacturerDocOrgPK = nameof(JobComInvoiceLine.ManufacturerDocOrgPK);
			public const string ManufacturerDocAddressPK = nameof(JobComInvoiceLine.ManufacturerDocAddressPK);
			public const string ImportLicenseNumber = nameof(JobComInvoiceLine.ImportLicenseNumber);
			public const string ImportLicenseType = nameof(JobComInvoiceLine.ImportLicenseType);
			public const string ImportLicenseFeeType = nameof(JobComInvoiceLine.ImportLicenseFeeType);
			public const string ImportLicenseAuthorizationDate = nameof(JobComInvoiceLine.ImportLicenseAuthorizationDate);
			public const string DutyRateIsOverridden = nameof(JobComInvoiceLine.DutyRateIsOverridden);
			public const string DutyVigentRateValue = nameof(JobComInvoiceLine.DutyVigentRateValue);
			public const string FTAMarginRateValue = nameof(JobComInvoiceLine.FTAMarginRateValue);
			public const string FTADutyRateValue = nameof(JobComInvoiceLine.FTADutyRateValue);
			public const string ReductionMarginRateValue = nameof(JobComInvoiceLine.ReductionMarginRateValue);
			public const string ReductionDutyRateValue = nameof(JobComInvoiceLine.ReductionDutyRateValue);
			public const string ReducedDutyRateValue = nameof(JobComInvoiceLine.ReducedDutyRateValue);
			public const string DutyTaxRegime = nameof(JobComInvoiceLine.DutyTaxRegime);
			public const string DutyLegalBase = nameof(JobComInvoiceLine.DutyLegalBase);
			public const string IPIVigentRateValue = nameof(JobComInvoiceLine.IPIVigentRateValue);
			public const string PisRateIsOverridden = nameof(JobComInvoiceLine.PisRateIsOverridden);
			public const string PisVigentRateValue = nameof(JobComInvoiceLine.PisVigentRateValue);
			public const string CofinsRateIsOverridden = nameof(JobComInvoiceLine.CofinsRateIsOverridden);
			public const string CofinsVigentRateValue = nameof(JobComInvoiceLine.CofinsVigentRateValue);
			public const string PisCofinsTaxRegime = nameof(JobComInvoiceLine.PisCofinsTaxRegime);
			public const string PisCofinsLegalBase = nameof(JobComInvoiceLine.PisCofinsLegalBase);
			public const string ICMSTaxRegime = nameof(JobComInvoiceLine.ICMSTaxRegime);
			public const string ICMSLegalBase = nameof(JobComInvoiceLine.ICMSLegalBase);
			public const string IPIRateIsOverridden = nameof(JobComInvoiceLine.IPIRateIsOverridden);
			public const string IPITaxRegime = nameof(JobComInvoiceLine.IPITaxRegime);
			public const string IPITaxBenefitLegalActType = nameof(JobComInvoiceLine.IPITaxBenefitLegalActType);
			public const string IPITaxBenefitLegalActIssuingBody = nameof(JobComInvoiceLine.IPITaxBenefitLegalActIssuingBody);
			public const string IPITaxBenefitLegalActNumber = nameof(JobComInvoiceLine.IPITaxBenefitLegalActNumber);
			public const string IPITaxBenefitLegalActYear = nameof(JobComInvoiceLine.IPITaxBenefitLegalActYear);
			public const string AntidumpingDutyRate = nameof(JobComInvoiceLine.AntidumpingRateValue);
			public const string AntidumpingRateIsOverridden = nameof(JobComInvoiceLine.AntidumpingRateIsOverridden);
			public const string FMMBenefit = nameof(JobComInvoiceLine.FMMBenefit);
			public const string FMMBenefitDescription = nameof(JobComInvoiceLine.FMMBenefitDescription);
			public const string ICMSFCPRateValue = nameof(JobComInvoiceLine.ICMSFCPRateValue);
			public const string DuimpLegalBase = nameof(JobComInvoiceLine.DuimpLegalBase);
			public const string ManufacturerName = nameof(JobComInvoiceLine.ManufacturerName);
			public const int NaladiNccaMaxLength = 8;
			public const int NaladiHsMaxLength = 8;
			public const int MercosulForeignDeclarationTypeMaxLength = 5;
			public const int TaxRegimeMaxLength = 1;
			public const int LegalBaseMaxLength = 2;
			public const int TariffMaxLength = 10;
		}

		#endregion

		#endregion

		#region Implementation

		#region GetTariffDescription - to be overridden once the Tariff is setup for a new country

		protected override ZString GetTariffDescription(ZString tariffCode)
		{
			return UniversalTariff?.ZZ1_Description ?? ZString.Empty;
		}

		#endregion

		protected override bool GetJI_CustomsUnitQtyInfoReadOnly()
		{
			return true;
		}

		protected override bool GetJI_CustomsQuantityReadOnly()
		{
			return JI_CustomsUnitQty.IsEmpty || HasLinkedInvoiceLine;
		}

		protected override bool SupportInvoiceLineRefs => true;

		IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => new string[] { Declaration?.JE_MessageType };

		#region ComplementaryDescription

		[MaxLength(nameof(ComplementaryDescriptionMaxLength))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|ComplementaryDescriptionExport", ShortCaption = "Comp. Descr.", Caption = "Complementary Description", FullDescription = "If necessary complement the description of the goods in this field, in order to allow their correct identification and classification")]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|ComplementaryDescriptionImport", ShortCaption = "Complement", Caption = "Complement", FullDescription = "Additional product details", MultipleKey = BRJobMessageTypeList.Codes.Import)]
		public ZString ComplementaryDescription
		{
			get => ComplementaryDescriptionNote.Text;
			set
			{
				ComplementaryDescriptionNote.SetNoteText(this, ComplementaryDescriptionInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateComplementaryDescription();
				}
				ComplementaryDescriptionInfo.RefreshBinding();
			}
		}

		HiddenTextNote ComplementaryDescriptionNote => complementaryDescriptionNote ?? (complementaryDescriptionNote = new HiddenTextNote(this, PredefinedNoteTypes.Instance.BRComplementaryDescription.Description));
		HiddenTextNote complementaryDescriptionNote;

		int ComplementaryDescriptionMaxLength => IsImportOnly ? 4000 : 2000;

		public ZPropertyInfo ComplementaryDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ComplementaryDescription); }
		}

		#endregion

		#region FullGoodsDescription

		[ReadOnlyMember(nameof(HasLinkedInvoiceLine))]
		[MaxLength(nameof(FullGoodsDescriptionMaxLength))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|FullGoodsDescription", Caption = "Goods Description")]
		public ZString FullGoodsDescription
		{
			get => JI_Description + FullGoodsDescriptionNote.Text;
			set
			{
				value = value.TrimEndSpaceTab();
				CheckMaximumLength(FullGoodsDescriptionInfo, value);
				JI_Description = value.SubstringSafe(0, Schema.JI_DescriptionMaxLength);
				FullGoodsDescriptionNote.SetNoteText(this, FullGoodsDescriptionInfo, value.SubstringSafe(Schema.JI_DescriptionMaxLength));

				if (!IsValidationSuspended)
				{
					Validation.ValidateFullGoodsDescription();
				}
				FullGoodsDescriptionInfo.RefreshBinding();
			}
		}

		HiddenTextNote FullGoodsDescriptionNote => fullGoodsDescriptionNote ?? (fullGoodsDescriptionNote = new HiddenTextNote(this, PredefinedNoteTypes.Instance.DeclarationGoodsDescription.Description));
		HiddenTextNote fullGoodsDescriptionNote;

		public int FullGoodsDescriptionMaxLength => IsImportSiscomex || IsImportLicense ? 3900 : Schema.JI_DescriptionMaxLength;

		public ZPropertyInfo FullGoodsDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.FullGoodsDescription); }
		}

		#endregion

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_NFeNumber", ShortCaption = "NFE Key", Caption = "NF-e Key", FullDescription = "NF-e Key that has 44 characters")]
		public override ZString JI_NFeNumber { get => base.JI_NFeNumber; set => base.JI_NFeNumber = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_NFeItemNumber", ShortCaption = "NF-e Item Number", Caption = "NF-e Item Number")]
		public override ZString JI_NFeItemNumber { get => base.JI_NFeItemNumber; set => base.JI_NFeItemNumber = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_ExportJustificationInfo", ShortCaption = "Justification Export", Caption = "Justification", FullDescription = "Required if Fob / net weight in KG / quantity in the statistical measure) are outside the historical parameters observed by customs authorities")]
		public override ZString JI_ExportJustificationInfo { get => base.JI_ExportJustificationInfo; set => base.JI_ExportJustificationInfo = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_RN_NKCountryOfExport", ShortCaption = "Destination", Caption = "Country of Destination the goods")]
		public override ZString JI_RN_NKCountryOfExport
		{
			get => base.JI_RN_NKCountryOfExport;
			set => base.JI_RN_NKCountryOfExport = value;
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_NFeLinePrice", Caption = "NF-e Total Value")]
		public override ZDecimal JI_NFeLinePrice
		{
			get => base.JI_NFeLinePrice;
			set => base.JI_NFeLinePrice = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.Currencies))]
		public ZString JI_NFeLinePriceCurrency => Core.Constants.CurrencyCodes.Brazil;

		[ReadOnlyMember(nameof(JI_TemporaryAdmissionReasonReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_TemporaryAdmissionReason", Caption = "Reason for Temporary Admission")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.RefCusCodeListMATMPList))]
		public override ZString JI_TemporaryAdmissionReason { get => base.JI_TemporaryAdmissionReason; set => base.JI_TemporaryAdmissionReason = value; }

		ZBool JI_TemporaryAdmissionReasonReadOnly => !IsTemporaryAdmissionReasonApplicable;

		public bool IsTemporaryAdmissionReasonApplicable => IsImportSiscomex && DutyTaxRegime == TaxRegimeList.Codes.Suspension
			&& (Declaration.JE_MessageSubType == MessageSubTypeList.Codes._05 || Declaration.JE_MessageSubType == MessageSubTypeList.Codes._12);

		[ReadOnlyMember(nameof(JI_ComplementaryNote_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_ComplementaryNote", Caption = "TIPI Complementary Note")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ComplementaryNoteList))]
		public override ZString JI_ComplementaryNote { get => base.JI_ComplementaryNote; set => base.JI_ComplementaryNote = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_Calc_MergedLineNumber", Caption = "Merged Ln. #")]
		public override ZString JI_Calc_MergedLineNumber { get => base.JI_Calc_MergedLineNumber; }

		#region JI_Procedure

		[MaxLength(5)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_Procedure", ShortCaption = "First CPC", Caption = "First Procedure Code")]
		public override ZString JI_Procedure
		{
			get => base.JI_Procedure;
			set
			{
				var oldValue = JI_Procedure;
				base.JI_Procedure = value;
				if (!IsCopying && oldValue != JI_Procedure)
				{
					JI_TemporaryAdmissionReason = ZString.Empty;
				}
			}
		}

		public void UpdateJI_Procedure(bool isImportSiscomex)
		{
			if (isImportSiscomex)
			{
				JI_Procedure = Declaration.JE_MessageSubType.Left(2).PadRight(2) + (DutyTaxRegime.Left(1).PadRight(1) + DutyLegalBase);
			}
			else if (!JI_Procedure.IsEmpty)
			{
				JI_Procedure = ZString.Empty;
			}
		}

		#endregion

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_SecondCPC", ShortCaption = "Second CPC", Caption = "Second Procedure Code")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.Procedures))]
		public override ZString JI_SecondCPC { get => base.JI_SecondCPC; set => base.JI_SecondCPC = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_FourthCPC", ShortCaption = "Fourth CPC", Caption = "Fourth Procedure Code")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.Procedures))]
		public override ZString JI_FourthCPC { get => base.JI_FourthCPC; set => base.JI_FourthCPC = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_ThirdCPC", ShortCaption = "Third CPC", Caption = "Third Procedure Code")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.Procedures))]
		public override ZString JI_ThirdCPC { get => base.JI_ThirdCPC; set => base.JI_ThirdCPC = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_FinancedValue", Caption = "Financed Value", FullDescription = "When choosing one procedure code of financed export this field must be filled")]
		public override ZDecimal JI_FinancedValue { get => base.JI_FinancedValue; set => base.JI_FinancedValue = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_AgentCommission", Caption = "Agent Commission(%)")]
		public override ZDecimal JI_AgentCommissionPercentage { get => base.JI_AgentCommissionPercentage; set => base.JI_AgentCommissionPercentage = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_CargoPriority", ShortCaption = "Cargo Priority", Caption = "Cargo Priority", FullDescription = "Should be entered if there is a reason for the operation to have priority for boarding")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CargoPriorityList))]
		public override ZString JI_CargoPriority { get => base.JI_CargoPriority; set => base.JI_CargoPriority = value; }

		[ReadOnlyMember(nameof(IsImportLicenseGeneratedFromImportSiscomexLine))]
		public override ZGuid JI_CEI
		{
			get => base.JI_CEI;
			set
			{
				var oldValue = JI_CEI;
				base.JI_CEI = value;
				if (oldValue != JI_CEI)
				{
					Declaration?.MarkAsNeedingValidation();
					InvoiceHeader?.MarkAsNeedingValidation();
				}
			}
		}

		#region JI_Tariff

		[MaxLength(Schema.TariffMaxLength)]
		[ReadOnlyMember(nameof(HasLinkedInvoiceLine))]
		public override ZString JI_Tariff
		{
			get
			{
				return base.JI_Tariff;
			}
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_Tariff))
				{
					var oldValue = JI_Tariff;
					base.JI_Tariff = value;
					if (!IsCopying && oldValue != JI_Tariff)
					{
						ResetTaxDetailsDataIfNeeded();
						DuimpTaxRegimes.Rebuild();
						TaxRegimeAttributes.Rebuild();
						Attributes.Rebuild();
						NVECusCodeDataCollection.RebuildFromCharacteristics();
					}
				}
			}
		}

		public override ZString FormatTariffForSaving(ZString unformattedTariff) => unformattedTariff.KeepNumericCharacters();

		void ResetTaxDetailsDataIfNeeded()
		{
			if (IsImportSiscomex)
			{
				AdditionalTariffs.RemoveAndDeleteAll();
				IPIRateIsOverridden = false;
				PisRateIsOverridden = false;
				CofinsRateIsOverridden = false;
				IPITaxRegime = CountryOfOrigin != null && UniversalTariff != null && DefaultIPIVigentRateValue == null ? IPITaxRegimeList.Codes.NonTaxable : string.Empty;
				JI_PrimaryPreference = Constants.RatePreferenceType.Normal;
			}

			if (IsImportExcludingLicense)
			{
				SpecialCaseTaxes.RemoveAndDeleteAll();
			}
		}

		#endregion

		[ReadOnlyMember(nameof(HasLinkedInvoiceLine))]
		public override ZDecimal JI_InvoiceQuantity { get => base.JI_InvoiceQuantity; set => base.JI_InvoiceQuantity = value; }

		[ReadOnlyMember(nameof(HasLinkedInvoiceLine))]
		public override ZString JI_InvoiceUQ { get => base.JI_InvoiceUQ; set => base.JI_InvoiceUQ = value; }

		[ReadOnlyMember(nameof(HasLinkedInvoiceLine))]
		public override ZDecimal JI_LinePrice { get => base.JI_LinePrice; set => base.JI_LinePrice = value; }

		[ReadOnlyMember(nameof(HasLinkedInvoiceLine))]
		public override ZDecimal JI_NetWeight
		{
			get { return base.JI_NetWeight; }
			set
			{
				var oldValue = JI_NetWeight;
				base.JI_NetWeight = value;
				if (!IsCopying && oldValue != JI_NetWeight)
				{
					MarkApportionmentDirty(HasParentChargeDistributedByThisToMarkApportionmentDirty(ChargeDistributeByList.Codes.NetWeight));
				}
			}
		}

		[ReadOnlyMember(nameof(HasLinkedInvoiceLine))]
		public override ZString JI_NetWeightUQ
		{
			get { return base.JI_NetWeightUQ; }
			set
			{
				var oldValue = JI_NetWeightUQ;
				base.JI_NetWeightUQ = value;
				if (!IsCopying && oldValue != JI_NetWeightUQ)
				{
					MarkApportionmentDirty(HasParentChargeDistributedByThisToMarkApportionmentDirty(ChargeDistributeByList.Codes.NetWeight));
				}
			}
		}

		[MaxLength(6)]
		public override ZString JI_CustomsUnitQty { get => base.JI_CustomsUnitQty; set => base.JI_CustomsUnitQty = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_CustomsSecondQuantity", Caption = "Supp. Qty")]
		public override ZDecimal JI_CustomsSecondQuantity { get => base.JI_CustomsSecondQuantity; set => base.JI_CustomsSecondQuantity = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_CustomsThirdQuantity", Caption = "Capacity Qty")]
		public override ZDecimal JI_CustomsThirdQuantity { get => base.JI_CustomsThirdQuantity; set => base.JI_CustomsThirdQuantity = value; }

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ContainerTypeList))]
		public override ZString JI_CustomsSecondUnitQty { get => base.JI_CustomsSecondUnitQty; set => base.JI_CustomsSecondUnitQty = value; }

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CapacityUnitList))]
		public override ZString JI_CustomsThirdUnitQty { get => base.JI_CustomsThirdUnitQty; set => base.JI_CustomsThirdUnitQty = value; }

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobComInvoiceLineFetchStrategy(this);
		}

		public override void UpdateDetailsFromPivotOnPartChangeCore()
		{
			base.UpdateDetailsFromPivotOnPartChangeCore();
			var pivot = (CusClassPartPivot)Pivot;

			JI_CGC_Catalog = pivot?.CI_CGC_Catalog ?? ZGuid.Empty;

			if (IsImportOnly && JI_CGC_Catalog.IsEmpty)
			{
				var goodsCatalog = new CusGoodsCatalog.Loader(Factory).GetUniqueGoodsCatalogsByLocalPartNumber(JI_PartNo, Importer?.PK ?? ZGuid.Empty, GoodsCatalogTypeList.Codes.Import);
				if (goodsCatalog != null)
				{
					JI_CGC_Catalog = goodsCatalog.PK;
				}
			}

			if (pivot != null)
			{
				if (!pivot.ComplementaryDescription.IsEmpty)
				{
					ComplementaryDescription = pivot.ComplementaryDescription.Left(ComplementaryDescriptionMaxLength);
				}

				if (pivot.IsImportClassification && JI_Tariff == pivot.TariffNumber)
				{
					if (IsImportSiscomex || IsImportLicense)
					{
						foreach (var nve in pivot.NveCusCodeDataCollection)
						{
							NVECusCodeDataCollection.GetFirstElementHaving(nve.CY_Code)?.CopyValuesIfEntered(nve);
						}
						TariffDetachs.CloneFrom(pivot.TariffDetachs);
						TariffDetachConcatenatedInfo.RefreshBinding();
					}

					if (IsImportSiscomex)
					{
						foreach (AdditionalTariff addtionalTariff in pivot.AdditionalTariffs)
						{
							var additionalTariff = AdditionalTariffs.FindBySubject(addtionalTariff.LegalActSubject);
							if (additionalTariff != null)
							{
								AdditionalTariffs.RemoveAndDelete(additionalTariff);
							}

							CusLineTariffDetails.AddNew().CopyPersistentValuesFrom(addtionalTariff.TariffDetail);
							LegalActInfos.AddNew().CopyPersistentValuesFrom(addtionalTariff.LegalAct);
						}

						AdditionalTariffs.Rebuild();
					}
				}

				if (pivot.IsExportClassification && IsExport && JI_Tariff == pivot.CI_TariffNum)
				{
					foreach (AttributeCusCodeData attribute in pivot.Attributes)
					{
						Attributes.GetFirstElementHaving(attribute.CY_Code)?.CopyValuesIfEntered(attribute);
					}
				}
			}
		}

		protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy() => new UniversalTariffCustomsUnitDefaultingStrategy<JobComInvoiceLine>();

		bool ShouldReCalculateCustomsQty
		{
			get
			{
				var legalDocument = EntryInstruction?.CEI_LegalDocument ?? ZString.Empty;
				return !IsExport || (JI_NFeItemNumber.IsEmpty && JI_NFeNumber.IsEmpty && legalDocument != LegalDocumentList.Codes.ElectronicLogisticInvoice);
			}
		}

		protected override ZBool ShouldReCalculateCustomsQtyOnLineQuantityChange => ShouldReCalculateCustomsQty;

		public override void CalculateFromNetWeightToCustomsQty()
		{
			if (ShouldReCalculateCustomsQty)
			{
				base.CalculateFromNetWeightToCustomsQty();
			}
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			if (!IsInDatabase || HasChanges)
			{
				((IAddInfoManager)this).AddInfo.UpdateRelatedPropertyInfo();
			}
			return base.CloneInternal(args);
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var result = new List<string>(base.GetPropertiesToExcludeFromCloning());
			result.Add(JobComInvoiceLineSchema.Constants.JI_ParentID);
			result.Add(JobComInvoiceLineSchema.Constants.JI_ParentTableCode);
			return result;
		}

		#region LPCO

		[ChildEditable(true)]
		public LPCOJobComInvLineRefsCollection LPCOJobComInvLineRefsCollection
		{
			get
			{
				if (fLPCOJobComInvLineRefsCollection == null)
				{
					fLPCOJobComInvLineRefsCollection = new LPCOJobComInvLineRefsCollection(this);
					fLPCOJobComInvLineRefsCollection.Load();
					RegisterEditableChildObject(fLPCOJobComInvLineRefsCollection);
				}
				return fLPCOJobComInvLineRefsCollection;
			}
		}

		LPCOJobComInvLineRefsCollection fLPCOJobComInvLineRefsCollection;

		public ZString LPCOConcatenated
		{
			get
			{
				return string.Join(",", LPCOJobComInvLineRefsCollection.Cast<LPCOJobComInvLineRefs>().Where(x => !x.JG_ReferenceNumber.IsEmpty).Select(x => x.JG_ReferenceNumber).OrderBy(x => x));
			}
		}

		public ZPropertyInfo LPCOConcatenatedInfo
		{
			get { return GetZPropertyInfo(Schema.LPCOConcatenated); }
		}

		#endregion

		#region Tariff Detach

		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		[ChildEditable(true)]
		public TariffDetachCollection TariffDetachs
		{
			get
			{
				if (fTariffDetachs == null)
				{
					fTariffDetachs = new TariffDetachCollection(this);
					fTariffDetachs.Load();
					RegisterEditableChildObject(fTariffDetachs);
				}
				return fTariffDetachs;
			}
		}

		TariffDetachCollection fTariffDetachs;

		public ZString TariffDetachConcatenated => TariffDetachs.ConcatenatedCodes;

		public ZPropertyInfo TariffDetachConcatenatedInfo => GetZPropertyInfo(Schema.TariffDetachConcatenated);

		#endregion

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_IntendedTermDays", Caption = "Intended Term (in days)", FullDescription = "The Intended Term, in days, in which the goods will be out of the country.")]
		public override ZShort JI_IntendedTermDays { get => base.JI_IntendedTermDays; set => base.JI_IntendedTermDays = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_DigitalServiceDossier", Caption = "Digital Service dossiers", FullDescription = "The Digital Service Dossiers.")]
		public override ZString JI_DigitalServiceDossier { get => base.JI_DigitalServiceDossier; set => base.JI_DigitalServiceDossier = value; }

		[ReadOnlyMember(nameof(JI_ManufacturerIndicatorReadOnly))]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ManufacturerIndicatorList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_ManufacturerIndicator", Caption = "Manufacturer Indicator", ShortCaption = "Manufacturer Ind.")]
		public override ZString JI_ManufacturerIndicator
		{
			get => base.JI_ManufacturerIndicator;
			set
			{
				var oldValue = JI_ManufacturerIndicator;
				base.JI_ManufacturerIndicator = value;
				if (!IsCopying && oldValue != JI_ManufacturerIndicator)
				{
					DefaultManufacturerAddressIfNeeded();
					DefaultCountryOriginIfNeeded();
				}
			}
		}

		public bool JI_ManufacturerIndicatorReadOnly => HasLinkedInvoiceLine || (!InvoiceHeader?.JZ_OH_Supplier.IsValid ?? true);

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_ManufacturerAuthorityIdentifier", Caption = "Manufacturer Authority")]
		public override ZString JI_ManufacturerAuthorityIdentifier { get => base.JI_ManufacturerAuthorityIdentifier; set => base.JI_ManufacturerAuthorityIdentifier = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_ManufacturerAuthorityVersion", Caption = "Manufacturer Version")]
		public override ZString JI_ManufacturerAuthorityVersion { get => base.JI_ManufacturerAuthorityVersion; set => base.JI_ManufacturerAuthorityVersion = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|ManufacturerName", Caption = "Manufacturer Name")]
		public ZString ManufacturerName => (IsImportLicense ? ManufacturerDocAddress?.E2_CompanyNameTruncated : ManufacturerAddress?.EffectiveCompanyNameTruncated) ?? ZString.Empty;

		protected override ZDecimal GetBaseValueToApportionOnCore(CurrencyConverter currencyConverter, string distributeBy)
		{
			switch (distributeBy)
			{
				case ChargeDistributeByList.Codes.NetWeight:
					return Core.Constants.Weight.ContainsCode(JI_NetWeightUQ.ToUpper()) ? Core.Constants.Weight.Convert(JI_NetWeight, JI_NetWeightUQ.ToUpper(), Core.Constants.Weight.Kilograms) : 0m;

				case ChargeDistributeByList.Codes.FOB:
					return FOBValueForApportionment;

				default:
					return base.GetBaseValueToApportionOnCore(currencyConverter, distributeBy);
			}
		}

		public override ZDecimal JI_CustomsValue
		{
			get { return IsImport ? JI_Calc_CIF_InLocalCurrency : JI_Calc_FOB_InLocalCurrency; }
		}

		public override ZGuid JI_JZ
		{
			get => base.JI_JZ;
			set
			{
				var oldValue = JI_JZ;
				var oldSupplierAddress = InvoiceHeader?.JZ_OA_SupplierAddress;
				base.JI_JZ = value;
				var newSupplierAddress = InvoiceHeader?.JZ_OA_SupplierAddress;
				if (!IsCopying && oldValue != JI_JZ)
				{
					if (oldSupplierAddress.HasValue && newSupplierAddress.HasValue && oldSupplierAddress != newSupplierAddress && !JI_ManufacturerIndicator.IsEmpty)
					{
						JI_ManufacturerIndicator = ZString.Empty;
					}

					TariffDetachs.MarkAsNeedingValidation();
					LPCOJobComInvLineRefsCollection.MarkAsNeedingValidation();
					InvoiceHeader?.MarkAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(ManufacturerAddress_ReadOnly))]
		public override ZGuid JI_OA_ManufacturerAddress
		{
			get => base.JI_OA_ManufacturerAddress;
			set
			{
				var oldValue = JI_OA_ManufacturerAddress;
				base.JI_OA_ManufacturerAddress = value;
				if (!IsCopying && value != oldValue)
				{
					DefaultCountryOriginIfNeeded();
					PopulateValuesFromForeignOperator();
				}
			}
		}

		public bool IsManufacturerAddressApplicable => JI_ManufacturerIndicator == ManufacturerIndicatorList.Codes._2;

		public bool ManufacturerAddress_ReadOnly => !IsManufacturerAddressApplicable || HasLinkedInvoiceLine;

		void DefaultManufacturerAddressIfNeeded()
		{
			if (IsImport)
			{
				EffectiveManufacturerAddressPK = JI_ManufacturerIndicator == ManufacturerIndicatorList.Codes._1 ? InvoiceHeader.JZ_OA_SupplierAddress : ZGuid.Empty;
			}
		}

		public ZGuid EffectiveManufacturerAddressPK
		{
			get
			{
				return IsImportLicense ? ManufacturerDocAddressPK : JI_OA_ManufacturerAddress;
			}
			set
			{
				var orgPK = Factory.Load<OrgAddress>(value)?.Header?.PK ?? ZGuid.Empty;
				if (IsImportLicense)
				{
					ManufacturerDocOrgPK = orgPK;
					ManufacturerDocAddressPK = value;
				}
				else
				{
					JI_OA_ManufacturerAddress_ZAddress.OrgPK = orgPK;
					JI_OA_ManufacturerAddress = value;
				}
			}
		}

		[ReadOnlyMember(nameof(JI_CountryOfOriginReadOnly))]
		public override ZString JI_CountryOfOrigin
		{
			get => base.JI_CountryOfOrigin;
			set
			{
				var oldValue = JI_CountryOfOrigin;
				base.JI_CountryOfOrigin = value;
				if (!IsCopying && oldValue != JI_CountryOfOrigin)
				{
					ResetTaxDetailsDataIfNeeded();
					DuimpTaxRegimes.Rebuild();
					TaxRegimeAttributes.Rebuild();
				}
			}
		}

		public ZBool JI_CountryOfOriginReadOnly => (IsImport && JI_ManufacturerIndicator != ManufacturerIndicatorList.Codes._3) || HasLinkedInvoiceLine;

		void DefaultCountryOriginIfNeeded()
		{
			if (IsImport && JI_ManufacturerIndicator != ManufacturerIndicatorList.Codes._3)
			{
				var countryCode = IsImportLicense ? ManufacturerDocAddress.E2_RN_NKCountryCode : ManufacturerAddress?.OA_RN_NKCountryCode ?? ZString.Empty;
				if (JI_CountryOfOrigin != countryCode)
				{
					JI_CountryOfOrigin = countryCode;
				}
			}
		}

		#region Goods Conditions

		public bool UsedMaterialRegimeIsNationalization => IsImportLicense && JI_UsedMaterialRegime == UsedMaterialRegimeList.Codes.Nationalization;

		bool UsedMaterialRegimeRelatedFieldsReadOnly => !UsedMaterialRegimeIsNationalization;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.UsedMaterialRegimeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_UsedMaterialRegime", Caption = "Used Material Regime", ShortCaption = "Used Mat. Reg.", FullDescription = "The Regime that the Used Material fits in.")]
		public override ZString JI_UsedMaterialRegime
		{
			get => base.JI_UsedMaterialRegime;
			set
			{
				base.JI_UsedMaterialRegime = value;
				if (!IsCopying && !UsedMaterialRegimeIsNationalization)
				{
					JI_UsedMaterialOperationType = ZString.Empty;
					JI_UsedMaterialSerialNumber = ZString.Empty;
					JI_UsedMaterialManufactureYear = ZString.Empty;
					JI_BrandName = ZString.Empty;
					JI_Model = ZString.Empty;
				}
			}
		}

		[ReadOnlyMember(nameof(UsedMaterialRegimeRelatedFieldsReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_UsedMaterialOperationType", Caption = "Operation Type")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.GoodsConditionOperationTypeList))]
		public override ZString JI_UsedMaterialOperationType { get => base.JI_UsedMaterialOperationType; set => base.JI_UsedMaterialOperationType = value; }

		[ReadOnlyMember(nameof(UsedMaterialRegimeRelatedFieldsReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_UsedMaterialSerialNumber", Caption = "Serial Number")]
		public override ZString JI_UsedMaterialSerialNumber { get => base.JI_UsedMaterialSerialNumber; set => base.JI_UsedMaterialSerialNumber = value; }

		[ReadOnlyMember(nameof(UsedMaterialRegimeRelatedFieldsReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_UsedMaterialManufactureYear", Caption = "Year", FullDescription = "The Year of Manufacture.")]
		public override ZString JI_UsedMaterialManufactureYear { get => base.JI_UsedMaterialManufactureYear; set => base.JI_UsedMaterialManufactureYear = value; }

		[ReadOnlyMember(nameof(UsedMaterialRegimeRelatedFieldsReadOnly))]
		[MaxLength(JobComInvoiceLine.Schema.JI_BrandNameMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_BrandName", Caption = "Brand")]
		public override ZString JI_BrandName { get => base.JI_BrandName; set => base.JI_BrandName = value; }

		[ReadOnlyMember(nameof(UsedMaterialRegimeRelatedFieldsReadOnly))]
		[MaxLength(JobComInvoiceLine.Schema.JI_ModelMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_Model", Caption = "Model")]
		public override ZString JI_Model { get => base.JI_Model; set => base.JI_Model = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_GoodsCondition", Caption = "Goods Condition", FullDescription = "The Goods Condition Indicator.")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.GoodsConditionTypeList))]
		public override ZString JI_GoodsCondition { get => base.JI_GoodsCondition; set => base.JI_GoodsCondition = value; }

		#endregion

		#region GoodsAplication

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_GoodsApplication", Caption = "Goods Application", FullDescription = "The Goods Application.")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.GoodsApplicationTypeList))]
		public override ZString JI_GoodsApplication { get => base.JI_GoodsApplication; set => base.JI_GoodsApplication = value; }

		#endregion

		#endregion

		#region SuspentionDrawback

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public SuspensionDrawbackCollection SuspensionDrawbackCollection
		{
			get
			{
				if (fSuspensionDrawbackCollection == null)
				{
					fSuspensionDrawbackCollection = new SuspensionDrawbackCollection(this);
					fSuspensionDrawbackCollection.Load();
					RegisterEditableChildObject(fSuspensionDrawbackCollection);
				}
				return fSuspensionDrawbackCollection;
			}
		}

		SuspensionDrawbackCollection fSuspensionDrawbackCollection;

		#endregion

		#region PreviousDocument

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public PreviousDocumentCollection PreviousDocuments
		{
			get
			{
				if (fPreviousDocuments == null)
				{
					fPreviousDocuments = new PreviousDocumentCollection(this);
					fPreviousDocuments.Load();
					RegisterEditableChildObject(fPreviousDocuments);
				}
				return fPreviousDocuments;
			}
		}

		PreviousDocumentCollection fPreviousDocuments;

		public ZString PreviousDocumentConcatenated => string.Join(",", PreviousDocuments.Cast<PreviousDocument>().Select(x => $"{x.CSI_Code}|{x.CSI_ReferenceNumber}").OrderBy(x => x));

		#endregion

		#region ReferenceInvoiceManual

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public ReferenceInvoiceManualCollection ReferenceInvoiceManualCollection
		{
			get
			{
				if (fReferenceInvoiceManualCollection == null)
				{
					fReferenceInvoiceManualCollection = new ReferenceInvoiceManualCollection(this);
					fReferenceInvoiceManualCollection.Load();
					RegisterEditableChildObject(fReferenceInvoiceManualCollection);
				}
				return fReferenceInvoiceManualCollection;
			}
		}

		ReferenceInvoiceManualCollection fReferenceInvoiceManualCollection;

		#endregion

		#region ElectronicLogisticInvoice

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public ElectronicLogisticInvoiceCollection ElectronicLogisticInvoiceCollection
		{
			get
			{
				if (fElectronicLogisticInvoiceCollection == null)
				{
					fElectronicLogisticInvoiceCollection = new ElectronicLogisticInvoiceCollection(this);
					fElectronicLogisticInvoiceCollection.Load();
					RegisterEditableChildObject(fElectronicLogisticInvoiceCollection);
				}
				return fElectronicLogisticInvoiceCollection;
			}
		}

		ElectronicLogisticInvoiceCollection fElectronicLogisticInvoiceCollection;

		#endregion

		#region ComplementaryLogisticInvoice

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public ComplementaryLogisticInvoiceCollection ComplementaryLogisticInvoiceCollection
		{
			get
			{
				if (fComplementaryLogisticInvoiceCollection == null)
				{
					fComplementaryLogisticInvoiceCollection = new ComplementaryLogisticInvoiceCollection(this);
					fComplementaryLogisticInvoiceCollection.Load();
					RegisterEditableChildObject(fComplementaryLogisticInvoiceCollection);
				}
				return fComplementaryLogisticInvoiceCollection;
			}
		}

		ComplementaryLogisticInvoiceCollection fComplementaryLogisticInvoiceCollection;

		#endregion

		#region NVE

		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		[ChildEditable(true)]
		public NveCusCodeDataCollection NVECusCodeDataCollection
		{
			get
			{
				if (nveCollection == null)
				{
					nveCollection = new NveCusCodeDataCollection(this);
					nveCollection.Load();
					nveCollection.RebuildFromCharacteristics();
					RegisterEditableChildObject(nveCollection);
				}
				return nveCollection;
			}
		}

		NveCusCodeDataCollection nveCollection;

		public ZString NVEConcatenated
		{
			get
			{
				return string.Join(",", NVECusCodeDataCollection.Cast<NveCusCodeData>().Select(x => $"{x.CY_Order}|{x.CY_Code}|{x.CY_Data}").OrderBy(x => x));
			}
		}

		#endregion

		#region CertificateOfOrigin

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public CertificateOfOriginCollection CertificateOfOriginCollection
		{
			get
			{
				if (fCertificateOfOriginCollection == null)
				{
					fCertificateOfOriginCollection = new CertificateOfOriginCollection(this);
					fCertificateOfOriginCollection.Load();
					RegisterEditableChildObject(fCertificateOfOriginCollection);
				}
				return fCertificateOfOriginCollection;
			}
		}

		CertificateOfOriginCollection fCertificateOfOriginCollection;

		#endregion

		#region MercosulForeignExportDeclaration

		[MaxLength(Schema.MercosulForeignDeclarationTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CertificateTypeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|MercosulForeignDeclarationType", Caption = "Certificate Type")]
		public ZString MercosulForeignDeclarationType
		{
			get
			{
				var collection = MercosulForeignDeclarations.Cast<MercosulForeignDeclaration>();
				return collection.Any() && collection.AllSame(x => x.CSI_SubType) ? collection.First().CSI_SubType : ZString.Empty;
			}

			set
			{
				value = value.TrimEndSpaceTab();
				CheckMaximumLength(MercosulForeignDeclarationTypeInfo, value);

				if (!value.IsEmpty && !MercosulForeignDeclarations.Any())
				{
					MercosulForeignDeclarations.AddNew();
				}
				MercosulForeignDeclarations.Cast<MercosulForeignDeclaration>().ForEach(x => x.CSI_SubType = value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateMercosulForeignDeclarationType();
				}
				MercosulForeignDeclarationTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo MercosulForeignDeclarationTypeInfo
		{
			get { return GetZPropertyInfo(Schema.MercosulForeignDeclarationType); }
		}

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public MercosulForeignDeclarationCollection MercosulForeignDeclarations
		{
			get
			{
				if (fMercosulForeignDeclarations == null)
				{
					fMercosulForeignDeclarations = new MercosulForeignDeclarationCollection(this);
					fMercosulForeignDeclarations.Load();
					fMercosulForeignDeclarations.CountChanged += (o, e) => MercosulForeignDeclarationTypeInfo.RefreshBinding();
					RegisterEditableChildObject(fMercosulForeignDeclarations);
				}
				return fMercosulForeignDeclarations;
			}
		}

		MercosulForeignDeclarationCollection fMercosulForeignDeclarations;

		public ZString MercosulForeignDeclarationConcatenated => string.Join(",", MercosulForeignDeclarations.Cast<MercosulForeignDeclaration>().Select(x => $"{x.CSI_Description}|{x.CSI_ReferenceNumber2}|{x.CSI_ItemNumber}").OrderBy(x => x));

		#endregion

		#region ConsentingProcess

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public ConsentingProcessCollection ConsentingProcessCollection
		{
			get
			{
				if (fConsentingProcessCollection == null)
				{
					fConsentingProcessCollection = new ConsentingProcessCollection(this);
					fConsentingProcessCollection.Load();
					RegisterEditableChildObject(fConsentingProcessCollection);
				}
				return fConsentingProcessCollection;
			}
		}

		ConsentingProcessCollection fConsentingProcessCollection;

		public ZString ConsentingProcessConcatenated
		{
			get
			{
				return string.Join(",", ConsentingProcessCollection.Cast<ConsentingProcess>().Select(x => $"{x.CSI_ReferenceNumber}|{x.CSI_CustomsOffice}").OrderBy(x => x));
			}
		}

		#endregion

		#region Drawback for Import License

		public DrawbackImportLicense DrawbackImportLicense => DrawbackImportLicenseCollection.FirstOrDefault() ?? DrawbackImportLicenseCollection.AddNew();

		[ChildEditable(true)]
		public DrawbackImportLicenseCollection DrawbackImportLicenseCollection
		{
			get { return fTypeApprovalCertificateNumberCusSupportingCollection ?? (fTypeApprovalCertificateNumberCusSupportingCollection = GetDrawbackImportLicenseCollection()); }
		}
		DrawbackImportLicenseCollection fTypeApprovalCertificateNumberCusSupportingCollection;

		DrawbackImportLicenseCollection GetDrawbackImportLicenseCollection()
		{
			var result = new DrawbackImportLicenseCollection(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|DrawbackModality", ShortCaption = "Modality", Caption = "Modality (Drawback Modality)", FullDescription = "The Modality Drawback")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.DrawbackModalityList))]
		public ZString DrawbackModality
		{
			get => DrawbackImportLicense.CSI_Code;
			set
			{
				DrawbackImportLicense.CSI_Code = value;
				DrawbackModalityInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DrawbackModalityInfo => GetWrappedZPropertyInfo(Schema.DrawbackModality, x => DrawbackImportLicense.CSI_CodeInfo);

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|DrawbackCANumber", Caption = "Concession Act Number", ShortCaption = "CA Number", FullDescription = "The Concession Act Number")]
		public ZString DrawbackCANumber
		{
			get => DrawbackImportLicense.CSI_ReferenceNumber;
			set
			{
				DrawbackImportLicense.CSI_ReferenceNumber = value;
				DrawbackCANumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DrawbackCANumberInfo => GetWrappedZPropertyInfo(Schema.DrawbackCANumber, x => DrawbackImportLicense.CSI_ReferenceNumberInfo);

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|DrawbackItemNumber", ShortCaption = "Item Number", Caption = "Drawback Item Number", FullDescription = "The Concession Item Act Number")]
		public ZInt DrawbackItemNumber
		{
			get => DrawbackImportLicense.CSI_ItemNumber;
			set
			{
				DrawbackImportLicense.CSI_ItemNumber = value;
				DrawbackItemNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DrawbackItemNumberInfo => GetWrappedZPropertyInfo(Schema.DrawbackItemNumber, x => DrawbackImportLicense.CSI_ItemNumberInfo);

		#endregion

		#region Charge Type Concatenated

		public ZString ChargeTypesConcatenated => string.Join(",", Charges.Cast<JobComInvCharge>().Union(ApportionedCharges).Select(charge => charge.J7_ChargeType).Distinct().Where(x => !x.IsEmpty).OrderBy(x => x));

		#endregion

		#region Tax Regime & Legal Base

		[ChildEditable(true)]
		public TaxRegimeCollection TaxRegimeCollection
		{
			get { return fTaxRegimeCollection ?? (fTaxRegimeCollection = GetTaxRegimeCollection()); }
		}
		TaxRegimeCollection fTaxRegimeCollection;

		TaxRegimeCollection GetTaxRegimeCollection()
		{
			var result = new TaxRegimeCollection(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		#region Duty Tax Regime & Legal Base

		public TaxRegime DutyTaxRegimeSupportingInfo => TaxRegimeCollection.FindBySubject(TaxRegimeTypeList.Codes.Duty) ?? TaxRegimeCollection.AddNew(TaxRegimeTypeList.Codes.Duty);

		[ReadOnlyMember(nameof(HasLinkedInvoiceLine))]
		[MaxLength(JobComInvoiceLine.Schema.TaxRegimeMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|DutyTaxRegime", ShortCaption = "Tax Regime", Caption = "Duty Tax Regime")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.DutyTaxRegimeList))]
		public ZString DutyTaxRegime
		{
			get => DutyTaxRegimeSupportingInfo.CSI_Code;
			set
			{
				var oldValue = DutyTaxRegime;
				DutyTaxRegimeSupportingInfo.CSI_Code = value;
				if (!IsCopying && oldValue != DutyTaxRegime)
				{
					if (!DutyLegalBase.IsEmpty)
					{
						DutyLegalBase = ZString.Empty;
					}
					UpdateJI_Procedure(IsImportSiscomex);

					if (IPITaxRegime_ReadOnly)
					{
						IPITaxRegime = IPITaxRegimeList.Codes.NonTaxable;
						IPIRateIsOverridden = false;
					}
				}

				DutyTaxRegimeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DutyTaxRegimeInfo => GetWrappedZPropertyInfo(Schema.DutyTaxRegime, x => DutyTaxRegimeSupportingInfo.CSI_CodeInfo);

		[ReadOnlyMember(nameof(DutyLegalBase_ReadOnly))]
		[MaxLength(JobComInvoiceLine.Schema.LegalBaseMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|DutyLegalBase", ShortCaption = "Legal Base", Caption = "Duty Legal Base")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.DutyLegalBaseList))]
		public ZString DutyLegalBase
		{
			get => DutyTaxRegimeSupportingInfo.CSI_Procedure;
			set
			{
				var oldValue = DutyLegalBase;
				DutyTaxRegimeSupportingInfo.CSI_Procedure = value;
				if (!IsCopying && oldValue != DutyLegalBase)
				{
					UpdateJI_Procedure(IsImportSiscomex);
				}
				DutyLegalBaseInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DutyLegalBaseInfo => GetWrappedZPropertyInfo(Schema.DutyLegalBase, x => DutyTaxRegimeSupportingInfo.CSI_ProcedureInfo);

		bool DutyLegalBase_ReadOnly => (IsImportLicense && (DutyTaxRegime.IsEmpty || DutyTaxRegime == TaxRegimeList.Codes.FullCollection)
										|| IsImportSiscomex && (DutyTaxRegime == TaxRegimeList.Codes.FullCollection || DutyTaxRegime == TaxRegimeList.Codes.PaymentMade))
										|| HasLinkedInvoiceLine;

		#endregion

		#region IPI Tax Regime

		public TaxRegime IPITaxRegimeSupportingInfo => TaxRegimeCollection.FindBySubject(TaxRegimeTypeList.Codes.IPI) ?? TaxRegimeCollection.AddNew(TaxRegimeTypeList.Codes.IPI);

		bool IPILegalBase_ReadOnly => IsImportSiscomex && (IPITaxRegime == IPITaxRegimeList.Codes.FullCollection || IPITaxRegime == IPITaxRegimeList.Codes.NonTaxable);

		bool JI_ComplementaryNote_ReadOnly => IsImportSiscomex && IPITaxRegime == IPITaxRegimeList.Codes.NonTaxable;

		bool IPITaxRegime_ReadOnly => IsImportSiscomex && (DutyTaxRegime == TaxRegimeList.Codes.Immunity || DutyTaxRegime == TaxRegimeList.Codes.NoIncident);

		[ReadOnlyMember(nameof(IPITaxRegime_ReadOnly))]
		[MaxLength(Schema.TaxRegimeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.IPITaxRegimeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|IPITaxRegime", ShortCaption = "Tax Regime", Caption = "IPI Tax Regime", FullDescription = "The IPI Tax Regime")]
		public ZString IPITaxRegime
		{
			get => IPITaxRegimeSupportingInfo.CSI_Code;
			set
			{
				var oldValue = IPITaxRegime;
				IPITaxRegimeSupportingInfo.CSI_Code = value;

				if (!IsCopying && oldValue != IPITaxRegime)
				{
					IPITaxBenefitLegalAct.Delete();

					if (JI_ComplementaryNote_ReadOnly)
					{
						JI_ComplementaryNote = ZString.Empty;
					}

					if (IPITaxRegime == IPITaxRegimeList.Codes.Reduction || oldValue == IPITaxRegimeList.Codes.Reduction)
					{
						OverriddenIPITaxRate?.Delete();

						if (IPITaxRegime == IPITaxRegimeList.Codes.Reduction)
						{
							SpecialCaseTaxes.UpdateOrAddReductionRate(Constants.RateCodes.IPI);
						}
						else
						{
							SpecialCaseTaxes.DeleteReductionRate(Constants.RateCodes.IPI);
						}
					}
				}

				if (!IsValidationSuspended)
				{
					IPITaxBenefitLegalAct.Validation.ValidateAll();
					Validation.ValidateIPIRateIsOverridden();
				}

				IPITaxRegimeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IPITaxRegimeInfo => GetWrappedZPropertyInfo(Schema.IPITaxRegime, x => IPITaxRegimeSupportingInfo.CSI_CodeInfo);

		#endregion

		#region PisCofins Tax Regime & Legal Base

		public TaxRegime PisCofinsTaxRegimeSupportingInfo => TaxRegimeCollection.FindBySubject(TaxRegimeTypeList.Codes.PisCofins) ?? TaxRegimeCollection.AddNew(TaxRegimeTypeList.Codes.PisCofins);

		bool PisCofinsLegalBase_ReadOnly => IsImportSiscomex && (PisCofinsTaxRegime == TaxRegimeList.Codes.FullCollection || PisCofinsTaxRegime == TaxRegimeList.Codes.PaymentMade);

		[MaxLength(Schema.TaxRegimeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PisCofinsTaxRegimeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|PisCofinsTaxRegime", ShortCaption = "Tax Regime", Caption = "PIS/COFINS Tax Regime", FullDescription = "The PIS/COFINS Tax Regime")]
		public ZString PisCofinsTaxRegime
		{
			get => PisCofinsTaxRegimeSupportingInfo.CSI_Code;
			set
			{
				var oldValue = PisCofinsTaxRegime;
				PisCofinsTaxRegimeSupportingInfo.CSI_Code = value;

				if (!IsCopying && oldValue != PisCofinsTaxRegime)
				{
					if (!PisCofinsLegalBase.IsEmpty)
					{
						PisCofinsLegalBase = ZString.Empty;
					}
					if (PisCofinsIsOverriddenReadOnly)
					{
						CofinsRateIsOverridden = false;
						PisRateIsOverridden = false;
					}

					if (PisCofinsTaxRegime == TaxRegimeList.Codes.Reduction || oldValue == TaxRegimeList.Codes.Reduction)
					{
						OverriddenPisTaxRate?.Delete();
						OverriddenCofinsTaxRate?.Delete();

						if (PisCofinsTaxRegime == TaxRegimeList.Codes.Reduction)
						{
							SpecialCaseTaxes.UpdateOrAddReductionRate(Constants.RateCodes.PIS);
							SpecialCaseTaxes.UpdateOrAddReductionRate(Constants.RateCodes.Cofins);
						}
						else
						{
							SpecialCaseTaxes.DeleteReductionRate(Constants.RateCodes.PIS);
							SpecialCaseTaxes.DeleteReductionRate(Constants.RateCodes.Cofins);
						}
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidatePisRateIsOverridden();
					Validation.ValidateCofinsRateIsOverridden();
				}

				PisCofinsTaxRegimeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PisCofinsTaxRegimeInfo => GetWrappedZPropertyInfo(Schema.PisCofinsTaxRegime, x => PisCofinsTaxRegimeSupportingInfo.CSI_CodeInfo);

		[MaxLength(Schema.LegalBaseMaxLength)]
		[ReadOnlyMember(nameof(PisCofinsLegalBase_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PisCofinsLegalBaseList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|PisCofinsLegalBase", ShortCaption = "Legal Base", Caption = "PIS/COFINS Legal Base", FullDescription = "The PIS/COFINS Legal Base")]
		public ZString PisCofinsLegalBase
		{
			get => PisCofinsTaxRegimeSupportingInfo.CSI_Procedure;
			set
			{
				PisCofinsTaxRegimeSupportingInfo.CSI_Procedure = value;
				PisCofinsLegalBaseInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PisCofinsLegalBaseInfo => GetWrappedZPropertyInfo(Schema.PisCofinsLegalBase, x => PisCofinsTaxRegimeSupportingInfo.CSI_ProcedureInfo);

		#endregion

		#region ICMS Tax Regime & Legal Base

		public TaxRegime ICMSTaxRegimeSupportingInfo => TaxRegimeCollection.FindBySubject(TaxRegimeTypeList.Codes.ICMS) ?? TaxRegimeCollection.AddNew(TaxRegimeTypeList.Codes.ICMS);

		[MaxLength(Schema.TaxRegimeMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|ICMSTaxRegime", ShortCaption = "Tax Regime", Caption = "ICMS Tax Regime", FullDescription = "The ICMS Tax Regime")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ICMSTaxRegimeList))]
		public ZString ICMSTaxRegime
		{
			get => ICMSTaxRegimeSupportingInfo.CSI_Code;
			set
			{
				var oldValue = ICMSTaxRegimeSupportingInfo.CSI_Code;
				ICMSTaxRegimeSupportingInfo.CSI_Code = value;
				if (!IsCopying && ICMSTaxRegime != oldValue)
				{
					JI_ICMSBaseValueReductionPercentage = ZDecimal.Zero;
					JI_ICMSTotalAmountReductionPercentage = ZDecimal.Zero;
				}

				ICMSTaxRegimeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ICMSTaxRegimeInfo => GetWrappedZPropertyInfo(Schema.ICMSTaxRegime, x => ICMSTaxRegimeSupportingInfo.CSI_CodeInfo);

		[MaxLength(Schema.LegalBaseMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|ICMSLegalBase", ShortCaption = "Legal Base", Caption = "ICMS Legal Base", FullDescription = "The ICMS Legal Base")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ICMSLegalBaseList))]
		public ZString ICMSLegalBase
		{
			get => ICMSTaxRegimeSupportingInfo.CSI_Procedure;
			set
			{
				ICMSTaxRegimeSupportingInfo.CSI_Procedure = value;
				ICMSLegalBaseInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ICMSLegalBaseInfo => GetWrappedZPropertyInfo(Schema.ICMSLegalBase, x => ICMSTaxRegimeSupportingInfo.CSI_ProcedureInfo);

		public bool IsICMSFeeCalculationApplicable => ICMSTaxRegime == ICMSTaxRegimeList.Codes.FullCollection || ICMSTaxRegime == ICMSTaxRegimeList.Codes.Reduction;

		#endregion

		#region JI_ICMSRate

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_ICMSRate", ShortCaption = "Rate (%)", Caption = "ICMS Rate (%)", FullDescription = "The ICMS Rate.")]
		public override ZDecimal JI_ICMSRate { get => base.JI_ICMSRate; set => base.JI_ICMSRate = value; }

		#endregion

		#region JI_ICMSBaseValueReductionPercentage

		[DecimalPlaces(5)]
		[ReadOnlyMember(nameof(ICMSBaseValueReductionPercentageReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_ICMSBaseValueReductionPercentage", ShortCaption = "Reduction of (%)", Caption = "Base Amount reduction (%)", FullDescription = "The Percentage that will be applied to reduce the ICMS Base Amount.")]
		public override ZDecimal JI_ICMSBaseValueReductionPercentage
		{
			get => base.JI_ICMSBaseValueReductionPercentage;
			set
			{
				var oldValue = base.JI_ICMSBaseValueReductionPercentage;
				base.JI_ICMSBaseValueReductionPercentage = value;
				if (!IsCopying && JI_ICMSBaseValueReductionPercentage != oldValue)
				{
					if (JI_ICMSFormula_ReadOnly)
					{
						JI_ICMSFormula = ZString.Empty;
					}
				}
			}
		}

		internal bool ICMSBaseValueReductionPercentageReadOnly => ICMSTaxRegime != ICMSTaxRegimeList.Codes.Reduction;

		#endregion

		#region JI_ICMSTotalAmountReductionPercentage

		[ReadOnlyMember(nameof(ICMSTotalAmountReductionPercentageReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_ICMSTotalAmountReductionPercentage", Caption = "Total Amount Reduction (%)", FullDescription = "The Percentage that will be applied to reduce the ICMS Total Amount.")]
		public override ZDecimal JI_ICMSTotalAmountReductionPercentage => base.JI_ICMSTotalAmountReductionPercentage;

		internal bool ICMSTotalAmountReductionPercentageReadOnly => ICMSTaxRegime != ICMSTaxRegimeList.Codes.Reduction;

		#endregion

		#region JI_ICMSFormula

		[ReadOnlyMember(nameof(JI_ICMSFormula_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_ICMSFormula", ShortCaption = "ICMS Formula", Caption = "BC ICMS Formula")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ICMSFormulaList))]
		public override ZString JI_ICMSFormula { get => base.JI_ICMSFormula; set => base.JI_ICMSFormula = value; }

		public bool JI_ICMSFormula_ReadOnly => JI_ICMSBaseValueReductionPercentage.IsEmpty;

		#endregion

		#endregion

		#region Import License

		public ImportLicenseInfo ImportLicenseSupportingInfo => ImportLicenseInfos.FirstOrDefault() ?? ImportLicenseInfos.AddNew();

		[ChildEditable(true)]
		internal ImportLicenseInfoCollection ImportLicenseInfos
		{
			get { return fImportLicenseInfos ?? (fImportLicenseInfos = GetImportLicenseCollection()); }
		}
		ImportLicenseInfoCollection fImportLicenseInfos;

		ImportLicenseInfoCollection GetImportLicenseCollection()
		{
			var result = new ImportLicenseInfoCollection(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|ImportLicenseType", ShortCaption = "Type", Caption = "Import License Type", FullDescription = "The Import License Type.")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ImportLicenseTypeList))]
		[MaxLength(ImportLicenseInfo.Schema.CSI_CodeMaxLength)]
		public ZString ImportLicenseType
		{
			get => ImportLicenseSupportingInfo.CSI_Code;
			set
			{
				ImportLicenseSupportingInfo.CSI_Code = value;
				ImportLicenseTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ImportLicenseTypeInfo => GetWrappedZPropertyInfo(Schema.ImportLicenseType, x => ImportLicenseSupportingInfo.CSI_CodeInfo);

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|ImportLicenseAuthorizationDate", ShortCaption = "Concession Date", Caption = "Import License Concession Date", FullDescription = "The date in which the Import License was authorized by the Consenting Body.")]
		public ZDateTime ImportLicenseAuthorizationDate
		{
			get => ImportLicenseSupportingInfo.CSI_DateOfIssue;
			set
			{
				ImportLicenseSupportingInfo.CSI_DateOfIssue = value;
				ImportLicenseAuthorizationDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ImportLicenseAuthorizationDateInfo => GetWrappedZPropertyInfo(Schema.ImportLicenseAuthorizationDate, x => ImportLicenseSupportingInfo.CSI_DateOfIssueInfo);

		[MaxLength(ImportLicenseInfo.Schema.CSI_ReferenceNumberMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|ImportLicenseNumber", Caption = "Import License Number", ShortCaption = "Import License No")]
		public ZString ImportLicenseNumber
		{
			get => ImportLicenseSupportingInfo?.CSI_ReferenceNumber ?? ZString.Empty;
			set
			{
				var oldValue = ImportLicenseNumber;
				ImportLicenseSupportingInfo.CSI_ReferenceNumber = value;
				ImportLicenseNumberInfo.RefreshBinding();
				if (!IsCopying && oldValue != ImportLicenseNumber)
				{
					JI_RequiresImportLicense = !ImportLicenseNumber.IsEmpty;
					InvoiceHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public ZPropertyInfo ImportLicenseNumberInfo => GetWrappedZPropertyInfo(Schema.ImportLicenseNumber, x => ImportLicenseSupportingInfo.CSI_ReferenceNumberInfo);

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|ImportLicenseFeeType", Caption = "Import License Fee Type", ShortCaption = "Fee Type", FullDescription = "The Import License Fee Type.")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ImportLicenseFeeTypeList))]
		[MaxLength(ImportLicenseInfo.Schema.CSI_SubTypeMaxLength)]
		public ZString ImportLicenseFeeType
		{
			get => ImportLicenseSupportingInfo.CSI_SubType;
			set
			{
				ImportLicenseSupportingInfo.CSI_SubType = value;
				ImportLicenseFeeTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ImportLicenseFeeTypeInfo => GetWrappedZPropertyInfo(Schema.ImportLicenseFeeType, x => ImportLicenseSupportingInfo.CSI_SubTypeInfo);

		[ReadOnlyMember(nameof(JI_RequiresImportLicenseReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_RequiresImportLicense", Caption = "Requires Import License")]
		public override ZBool JI_RequiresImportLicense { get => base.JI_RequiresImportLicense; set => base.JI_RequiresImportLicense = value; }

		ZBool JI_RequiresImportLicenseReadOnly => !ImportLicenseNumber.IsEmpty;

		#endregion

		#region JI_SecondaryPreference

		[MaxLength(5)]
		[ReadOnlyMember(nameof(HasLinkedInvoiceLine))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_SecondaryPreference", Caption = "Tariff Agreement", ShortCaption = "Agreement", FullDescription = "The Tariff Agreement.")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.TariffAgreementList))]
		public override ZString JI_SecondaryPreference { get => base.JI_SecondaryPreference; set => base.JI_SecondaryPreference = value; }

		#endregion

		#region FMM Benefit

		public TaxRegime FMMTaxRegimeSupportingInfo => TaxRegimeCollection.FindBySubject(TaxRegimeTypeList.Codes.FMM) ?? TaxRegimeCollection.AddNew(TaxRegimeTypeList.Codes.FMM);

		[MaxLength(1)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|FMMBenefit", Caption = "FMM Benefit", FullDescription = "The FMM Benefit.")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.FMMBenefitList))]
		[ReadOnlyMember(nameof(FMMBenefit_ReadOnly))]
		public ZString FMMBenefit
		{
			get => FMMTaxRegimeSupportingInfo.CSI_Code;
			set
			{
				FMMTaxRegimeSupportingInfo.CSI_Code = value;
				FMMBenefitInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FMMBenefitInfo => GetWrappedZPropertyInfo(nameof(FMMBenefit), x => FMMTaxRegimeSupportingInfo.CSI_CodeInfo);

		ZBool FMMBenefit_ReadOnly => !(Declaration?.IsAFRMMApplicable ?? false);

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|FMMBenefitDescription", Caption = "FMM Benefit Description")]
		public ZString FMMBenefitDescription => Lookups.FMMBenefitList.GetDescriptionFromCode(FMMBenefit);

		#endregion

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type>
			{
				{ CusSupportingInfoTypeList.Codes.SuspensionDrawback, typeof(SuspensionDrawback) },
				{ CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(PreviousDocument) },
				{ CusSupportingInfoTypeList.Codes.ReferenceInvoiceManual, typeof(ReferenceInvoiceManual) },
				{ CusSupportingInfoTypeList.Codes.ElectronicLogisticInvoice, typeof(ElectronicLogisticInvoice) },
				{ CusSupportingInfoTypeList.Codes.ComplementaryLogisticInvoice, typeof(ComplementaryLogisticInvoice) },
				{ CusSupportingInfoTypeList.Codes.CertificateOfOrigin, typeof(CertificateOfOrigin) },
				{ CusSupportingInfoTypeList.Codes.MercosulForeignDeclaration, typeof(MercosulForeignDeclaration) },
				{ CusSupportingInfoTypeList.Codes.ConsentingProcess, typeof(ConsentingProcess) },
				{ CusSupportingInfoTypeList.Codes.Drawback, typeof(DrawbackImportLicense) },
				{ CusSupportingInfoTypeList.Codes.LegalAct, typeof(LegalActInfo) },
				{ CusSupportingInfoTypeList.Codes.TaxRegime, typeof(TaxRegime) },
				{ CusSupportingInfoTypeList.Codes.ImportLicense, typeof(ImportLicenseInfo) },
				{ CusSupportingInfoTypeList.Codes.QuantityPerUnit, typeof(QuantityPerUnitInfo) },
				{ CusSupportingInfoTypeList.Codes.Permit, typeof(Permit) },
				{ CusSupportingInfoTypeList.Codes.DuimpTaxRegime, typeof(DuimpTaxRegime) },
			};
			return result;
		}

		public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		protected override ZString GetProcedureCodeCore() => JI_Procedure;

		protected override (GetValueDelegate<RefCusProcedure> GetCusProcedureFunc, Func<string> GetKeyFunc) GetCusProcedureCore(ZString procedureCode)
		{
			RefCusProcedure GetProcedureFunc() => new RefCusProcedure.Loader(Factory).LoadTop1FromCodeAndCountry(procedureCode, ZString.Empty, CustomsCountryCode, ZDateTime.Today);
			string GetKeyFunc() => $"BRCusProcedure_{procedureCode}_{CustomsCountryCode}_{ZDateTime.Today.ToShortDateString()}";  // this key should be changed following the changes of LoadTop1FromCodeAndCountry's parameter above.
			return (GetProcedureFunc, GetKeyFunc);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ CusCodeDataTypeList.Codes.Attribute, typeof(AttributeCusCodeData) },
				{ CusCodeDataTypeList.Codes.TaxRegimeAttribute, typeof(AttributeCusCodeData) },
				{ CusCodeDataTypeList.Codes.NVE, typeof(NveCusCodeData) },
				{ CusCodeDataTypeList.Codes.TariffDetach, typeof(TariffDetach) }
			};
		}

		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		[ChildEditable(true)]
		public AttributeCusCodeDataCollection Attributes
		{
			get
			{
				if (attributes == null)
				{
					attributes = new AttributeCusCodeDataCollection(this, CusCodeDataTypeList.Codes.Attribute);
					attributes.Load();
					attributes.Rebuild();
					RegisterEditableChildObject(attributes);
				}
				return attributes;
			}
		}

		AttributeCusCodeDataCollection attributes;

		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		[ChildEditable(true)]
		public AttributeCusCodeDataCollection TaxRegimeAttributes
		{
			get
			{
				if (taxRegimeAttributes == null)
				{
					taxRegimeAttributes = new AttributeCusCodeDataCollection(this, CusCodeDataTypeList.Codes.TaxRegimeAttribute);
					taxRegimeAttributes.Load();
					taxRegimeAttributes.Rebuild();
					RegisterEditableChildObject(taxRegimeAttributes);
				}
				return taxRegimeAttributes;
			}
		}

		AttributeCusCodeDataCollection taxRegimeAttributes;

		public AttributeCusCodeDataCollection GetAttributes(string type)
		{
			return type switch
			{
				CusCodeDataTypeList.Codes.TaxRegimeAttribute => TaxRegimeAttributes,
				CusCodeDataTypeList.Codes.Attribute => Attributes,
				_ => null,
			};
		}

		#region NALADI/NCCA

		[MaxLength(Schema.NaladiNccaMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.NaladiNccaTariffList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|NaladiNcca", Caption = "NALADI/NCCA")]
		public ZString NaladiNcca
		{
			get
			{
				return LoadTariffDetailByType(Constants.TariffTypes.NCCA)?.BZ_Tariff ?? ZString.Empty;
			}
			set
			{
				if (value != NaladiNcca)
				{
					CheckMaximumLength(NaladiNccaInfo, value);
					UpdateOrAddTariffDetail(value, Constants.TariffTypes.NCCA);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateNaladiNcca();
				}
				NaladiNccaInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo NaladiNccaInfo
		{
			get { return GetZPropertyInfo(Schema.NaladiNcca); }
		}

		#endregion

		#region NALADI/HS

		[ReadOnlyMember(nameof(HasLinkedInvoiceLine))]
		[MaxLength(Schema.NaladiHsMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.NaladiHsTariffList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|NaladiHs", Caption = "NALADI/HS", FullDescription = "The Nomenclature of the Latin American Integration based on the Harmonized System.")]
		public ZString NaladiHs
		{
			get
			{
				return LoadTariffDetailByType(Constants.TariffTypes.NALADIHS)?.BZ_Tariff ?? ZString.Empty;
			}
			set
			{
				if (value != NaladiHs)
				{
					CheckMaximumLength(NaladiHsInfo, value);
					UpdateOrAddTariffDetail(value, Constants.TariffTypes.NALADIHS);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateNaladiHs();
				}
				NaladiHsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo NaladiHsInfo
		{
			get { return GetZPropertyInfo(Schema.NaladiHs); }
		}

		#endregion

		#region AdditionalTariffs

		[ChildEditable(true)]
		public AdditionalTariffCollection AdditionalTariffs
		{
			get
			{
				if (fAdditionalTariffs == null)
				{
					fAdditionalTariffs = new AdditionalTariffCollection(this);
					fAdditionalTariffs.Load();
					RegisterEditableChildObject(fAdditionalTariffs);
				}
				return fAdditionalTariffs;
			}
		}

		AdditionalTariffCollection fAdditionalTariffs;

		public bool AdditionalTariffsIsLoaded => fAdditionalTariffs != null;

		public ZString AdditionalTariffConcatenated
		{
			get
			{
				return string.Join(",", AdditionalTariffs.Cast<AdditionalTariff>().OrderBy(x => x.LegalActSubject)
					.Select(x => $"{x.LegalActSubject}|{x.ExNumber}|{x.TariffType}|{x.LegalActType}|{x.LegalActIssuingBody}|{x.LegalActNumber}|{x.LegalActYear}"));
			}
		}

		#endregion

		#region Special Cases

		[ChildEditable(true)]
		public SpecialCaseTaxCollection SpecialCaseTaxes
		{
			get
			{
				if (fSpecialCaseTaxes == null)
				{
					fSpecialCaseTaxes = new SpecialCaseTaxCollection(this);
					fSpecialCaseTaxes.Load();
					RegisterEditableChildObject(fSpecialCaseTaxes);
				}
				return fSpecialCaseTaxes;
			}
		}

		SpecialCaseTaxCollection fSpecialCaseTaxes;

		public bool SpecialCaseTaxesIsLoaded => fSpecialCaseTaxes != null;

		public SpecialCaseTax IPICalculateByUQRate => SpecialCaseTaxes.FindByRateCodeAndTaxType(Constants.RateCodes.IPI, SpecialCaseTaxTypeList.Codes.QuantityPerUnit);

		public SpecialCaseTax PISCalculateByUQRate => SpecialCaseTaxes.FindByRateCodeAndTaxType(Constants.RateCodes.PIS, SpecialCaseTaxTypeList.Codes.QuantityPerUnit);

		public SpecialCaseTax CofinsCalculateByUQRate => SpecialCaseTaxes.FindByRateCodeAndTaxType(Constants.RateCodes.Cofins, SpecialCaseTaxTypeList.Codes.QuantityPerUnit);

		public SpecialCaseTax AntidumpingCalculateByUQRate => SpecialCaseTaxes.FindByRateCodeAndTaxType(Constants.RateCodes.Antidumping, SpecialCaseTaxTypeList.Codes.QuantityPerUnit);

		#endregion

		#region CusLineTariffDetail

		protected override bool SupportsAdditionalTariffs => true;

		CusLineTariffDetail LoadTariffDetailByType(ZString tariffType)
		{
			return CusLineTariffDetails.Cast<CusLineTariffDetail>().FirstOrDefault(detail => detail.BZ_Type == tariffType && !detail.IsDeleted);
		}

		void UpdateOrAddTariffDetail(ZString tariffCode, ZString tariffType)
		{
			var tariffDetail = LoadTariffDetailByType(tariffType);

			if (tariffCode.IsEmpty)
			{
				tariffDetail?.Delete();
			}
			else
			{
				if (tariffDetail == null)
				{
					tariffDetail = CusLineTariffDetails.AddNew();
					tariffDetail.BZ_Type = tariffType;
				}

				tariffDetail.BZ_Tariff = tariffCode;
			}
		}

		CusLineTariffDetail ExDutyTariffDetail => AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.ExDutyTariff)?.TariffDetail;

		public TariffView ExDutyTariff
		{
			get
			{
				TariffView tariff = null;

				var tariffDetail = ExDutyTariffDetail;
				if (tariffDetail != null && !tariffDetail.BZ_Tariff.IsEmpty && !tariffDetail.BZ_Type.IsEmpty)
				{
					tariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff), tariffDetail.BZ_Type, tariffDetail.BZ_Tariff, EffectiveAssessmentDate);
				}
				return tariff;
			}
		}

		public RateView ExDutyTariffRate => ExDutyTariff?.GetApplicableRates(DutyRateSelectionCriteria).FirstOrDefault();

		public ZDecimal ExTariffDutyRateValue => Factory.GetCached(ref cachedExDutyTariff, GetExTariffDutyRateValue);
		CachedProperty<ZDecimal> cachedExDutyTariff;

		ZDecimal GetExTariffDutyRateValue()
		{
			var exDutyRate = ExDutyTariffRate;
			return exDutyRate == null ? NormalDutyRateValue : ZDecimal.ParseSafe(exDutyRate.ZZ2_RateFormulaDerivedFrom, ZDecimal.Zero);
		}

		#endregion

		#region Duty & Taxes

		#region Duty Rate

		protected override ZString UniversalTariffDutyRateCode => Constants.RateCodes.ImportDuty;

		#region DutyVigentRateValue

		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(DutyVigentRateValueReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|DutyVigentRateValue", Caption = "Ad Valorem Rate (%)")]
		public ZDecimal DutyVigentRateValue
		{
			get
			{
				if (JI_PrimaryPreference == Constants.RatePreferenceType.ExTariff)
				{
					return DutyRateIsOverridden ? OverriddenDutyRateValue : ExTariffDutyRateValue;
				}
				else
				{
					return NormalDutyRateValue;
				}
			}
			set
			{
				OverriddenDutyRateValue = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateDutyVigentRateValue();
				}

				DutyVigentRateValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DutyVigentRateValueInfo => GetZPropertyInfo(Schema.DutyVigentRateValue);

		public ZBool DutyVigentRateValueReadOnly => !(JI_PrimaryPreference == Constants.RatePreferenceType.ExTariff && DutyRateIsOverridden);

		#endregion

		#region FTAMarginRateValue

		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(FTAMarginRateValueReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|FTAMarginRateValue", Caption = "FTA (Margin)")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Baseline")]
		public ZDecimal FTAMarginRateValue
		{
			get
			{
				var dutyVigentRateValue = DutyVigentRateValue;
				if (JI_PrimaryPreference == RatePreferenceType.FreeTradeAgreement && dutyVigentRateValue > 0)
				{
					return !DutyRateIsOverridden ? 0m : Utilities.Round((1 - (FTADutyRateValue / dutyVigentRateValue)) * 100, 2);
				}
				return ZDecimal.Zero;
			}
			set
			{
				var dutyVigentRateValue = DutyVigentRateValue;
				FTADutyRateValue = dutyVigentRateValue - (dutyVigentRateValue * (value / 100));

				if (!IsValidationSuspended)
				{
					Validation.ValidateFTAMarginRateValue();
				}

				FTAMarginRateValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FTAMarginRateValueInfo => GetZPropertyInfo(Schema.FTAMarginRateValue);

		public ZBool FTAMarginRateValueReadOnly => !(JI_PrimaryPreference == RatePreferenceType.FreeTradeAgreement && DutyRateIsOverridden);

		#endregion

		#region FTADutyRateValue

		[ReadOnly(true)]
		[DecimalPlaces(2)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|FTADutyRateValue", Caption = "FTA Rate (%)")]
		public ZDecimal FTADutyRateValue
		{
			get => !FTAMarginRateValueReadOnly ? OverriddenDutyRateValue : ZDecimal.Zero;
			set
			{
				OverriddenDutyRateValue = value;
				FTADutyRateValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FTADutyRateValueInfo => GetZPropertyInfo(Schema.FTADutyRateValue);

		#endregion

		#region ReductionMarginRateValue

		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(ReductionMarginRateValueReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|ReductionMarginRateValue", Caption = "Reduction (Margin)")]
		public ZDecimal ReductionMarginRateValue
		{
			get
			{
				var dutyVigentRateValue = DutyVigentRateValue;
				if (JI_PrimaryPreference == RatePreferenceType.ReductionMargin && dutyVigentRateValue > 0)
				{
					return !DutyRateIsOverridden ? 0m : Utilities.Round((1 - (ReductionDutyRateValue / dutyVigentRateValue)) * 100, 2);
				}
				return ZDecimal.Zero;
			}
			set
			{
				var dutyVigentRateValue = DutyVigentRateValue;
				ReductionDutyRateValue = dutyVigentRateValue - (dutyVigentRateValue * (value / 100));

				if (!IsValidationSuspended)
				{
					Validation.ValidateReductionMarginRateValue();
				}

				ReductionMarginRateValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ReductionMarginRateValueInfo => GetZPropertyInfo(Schema.ReductionMarginRateValue);

		public ZBool ReductionMarginRateValueReadOnly => !(JI_PrimaryPreference == Constants.RatePreferenceType.ReductionMargin && DutyRateIsOverridden);

		#endregion

		#region ReductionDutyRateValue

		[ReadOnly(true)]
		[DecimalPlaces(2)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|ReductionDutyRateValue", Caption = "Reduction Rate (%)")]
		public ZDecimal ReductionDutyRateValue
		{
			get => !ReductionMarginRateValueReadOnly ? OverriddenDutyRateValue : ZDecimal.Zero;
			set
			{
				OverriddenDutyRateValue = value;
				ReductionDutyRateValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ReductionDutyRateValueInfo => GetZPropertyInfo(Schema.ReductionDutyRateValue);

		#endregion

		#region ReducedDutyRateValue

		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(ReducedDutyRateValueReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|ReducedDutyRateValue", Caption = "Reduced Rate")]
		public ZDecimal ReducedDutyRateValue
		{
			get => !ReducedDutyRateValueReadOnly ? OverriddenDutyRateValue : ZDecimal.Zero;
			set
			{
				OverriddenDutyRateValue = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateReducedDutyRateValue();
				}

				ReducedDutyRateValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ReducedDutyRateValueInfo => GetZPropertyInfo(Schema.ReducedDutyRateValue);

		public ZBool ReducedDutyRateValueReadOnly => !(JI_PrimaryPreference == Constants.RatePreferenceType.ReducedRate && DutyRateIsOverridden);

		#endregion

		#region AntidumpingRateValue

		public ZDecimal DefaultAntidumpingRateValue => Factory.GetCached(ref defaultAntidumpingRateValue, GetDefaultAntidumpingRate);
		CachedProperty<ZDecimal> defaultAntidumpingRateValue;

		ZDecimal GetDefaultAntidumpingRate() => ZDecimal.ParseSafe(AntidumpingRate?.ZZ2_RateFormulaDerivedFrom ?? ZString.Empty, ZDecimal.Zero);

		public RateView AntidumpingRate => UniversalTariff?.GetApplicableRate(AntidumpingRateSelectionCriteria);

		public IZZRateSelectionCriteria AntidumpingRateSelectionCriteria => Factory.GetCached(ref antidumpingRateSelectionCriteria, GetAntidumpingRateSelectionCriteria);
		CachedProperty<IZZRateSelectionCriteria> antidumpingRateSelectionCriteria;

		IZZRateSelectionCriteria GetAntidumpingRateSelectionCriteria() => new RateSelectionCriteria<BaseJobComInvoiceLine>(this, Constants.RateTypes.Antidumping, Constants.RateCodes.Antidumping);

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|AntidumpingRateValue", ShortCaption = "Ad Valorem Rate (%)", Caption = "Antidumping Ad Valorem Rate (%)", FullDescription = "The Antidumping Ad Valorem Rate.")]
		public ZDecimal AntidumpingRateValue => AntidumpingRateIsOverridden ? OverriddenAntidumpingTaxRate.JLT_Rate : DefaultAntidumpingRateValue;

		#region AntidumpingRateIsOverridden

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|AntidumpingRateIsOverridden", ShortCaption = "Override", Caption = "Antidumping Rate Override")]
		public ZBool AntidumpingRateIsOverridden => OverriddenAntidumpingTaxRate != null;

		JobComInvoiceLineTax OverriddenAntidumpingTaxRate => Taxes.FindByType(Constants.RateCodes.Antidumping);

		#endregion

		#endregion

		#region DutyRateIsOverridden

		string GetMethodOfCalculationCode(string ratePreferenceType)
		{
			return ratePreferenceType switch
			{
				Constants.RatePreferenceType.FreeTradeAgreement => SpecialCaseTaxTypeList.Codes.TariffAgreement,
				Constants.RatePreferenceType.ReductionMargin => SpecialCaseTaxTypeList.Codes.Reduction,
				Constants.RatePreferenceType.ReducedRate => SpecialCaseTaxTypeList.Codes.Reduced,
				_ => SpecialCaseTaxTypeList.Codes.AdValoremRate
			};
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(DutyRateIsOverriddenReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|DutyRateIsOverridden", ShortCaption = "Override", Caption = "Duty Rate Override")]
		public ZBool DutyRateIsOverridden
		{
			get
			{
				return !OverriddenDutyRate?.JLT_MethodOfCalculation.IsEmpty ?? false;
			}
			set
			{
				var oldValue = DutyRateIsOverridden;
				if (!IsCopying && oldValue != value)
				{
					if (value)
					{
						switch (JI_PrimaryPreference)
						{
							case Constants.RatePreferenceType.ExTariff:
								DutyVigentRateValue = ExTariffDutyRateValue;
								break;
							case Constants.RatePreferenceType.FreeTradeAgreement:
								FTAMarginRateValue = ZDecimal.Zero;
								break;
							case Constants.RatePreferenceType.ReductionMargin:
								ReductionMarginRateValue = 100m;
								break;
							case Constants.RatePreferenceType.ReducedRate:
								ReducedDutyRateValue = ZDecimal.Zero;
								break;
						}
						OverriddenDutyRate.JLT_MethodOfCalculation = GetMethodOfCalculationCode(JI_PrimaryPreference);
					}
					else if (OverriddenDutyRate != null)
					{
						OverriddenDutyRate.JLT_Rate = ZDecimal.Zero;
						OverriddenDutyRate.JLT_MethodOfCalculation = ZString.Empty;
					}

					AdditionalTariffs.Cast<AdditionalTariff>().ForEach(x => x.DefaultLegalActInformation());
				}

				DutyRateIsOverriddenInfo.RefreshBinding(value);
				ReductionMarginRateValueInfo.RefreshBinding();
				FTAMarginRateValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DutyRateIsOverriddenInfo => GetZPropertyInfo(Schema.DutyRateIsOverridden);

		ZBool DutyRateIsOverriddenReadOnly => JI_PrimaryPreference == ZString.Empty || JI_PrimaryPreference == Constants.RatePreferenceType.Normal;

		#endregion

		#region ICMSFCPRateValue

		[DecimalPlaces(2)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|ICMSFCPRateValue", ShortCaption = "% ICMS FCP", Caption = "% ICMS FCP", FullDescription = "ICMS ( % on the Fund to Combat Poverty ) ")]
		public ZDecimal ICMSFCPRateValue
		{
			get => ICMSFCPTaxRate?.JLT_Rate ?? ZDecimal.Zero;
			set
			{
				var oldValue = ICMSFCPRateValue;
				if (!IsCopying && oldValue != value)
				{
					if (value.IsEmpty)
					{
						ICMSFCPTaxRate?.Delete();
					}
					else
					{
						(ICMSFCPTaxRate ?? Taxes.AddNew(Constants.RateCodes.ICMSFCP, Constants.MethodOfCalculation.Percentage)).JLT_Rate = value;
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateICMSFCPRateValue();
				}

				ICMSFCPRateValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ICMSFCPRateValueInfo => GetZPropertyInfo(Schema.ICMSFCPRateValue);

		JobComInvoiceLineTax ICMSFCPTaxRate => Taxes.FindByType(Constants.RateCodes.ICMSFCP);

		#endregion

		#endregion

		#region IPI

		public IZZRateSelectionCriteria IPIVigentRateSelectionCriteria => Factory.GetCached(ref ipiVigentRateSelectionCriteria, GetIPIVigentRateSelectionCriteriaCore);
		CachedProperty<IZZRateSelectionCriteria> ipiVigentRateSelectionCriteria;

		IZZRateSelectionCriteria GetIPIVigentRateSelectionCriteriaCore() => new RateSelectionCriteria<BaseJobComInvoiceLine>(this, Constants.RateTypes.IPI, Constants.RateCodes.IPI);

		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(IPIVigentRateValueReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|IPIVigentRateValue", ShortCaption = "Ad Valorem Rate (%)", Caption = "IPI Ad Valorem Rate (%)")]
		public ZDecimal IPIVigentRateValue
		{
			get => IPIRateIsOverridden ? OverriddenIPITaxRate.JLT_Rate : DefaultIPIVigentRateValue ?? ZDecimal.Zero;
			set
			{
				(OverriddenIPITaxRate ?? Taxes.AddNew(Constants.RateCodes.IPI, SpecialCaseTaxTypeList.Codes.AdValoremRate)).JLT_Rate = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateIPIVigentRateValue();
				}

				IPIVigentRateValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IPIVigentRateValueInfo => GetZPropertyInfo(Schema.IPIVigentRateValue);

		public RateView IPIVigentRate => UniversalTariff?.GetApplicableRate(IPIVigentRateSelectionCriteria);

		public ZDecimal? DefaultIPIVigentRateValue => Factory.GetCached(ref defaultIPIVigentRateValue, GetDefaultIPIVigentRate);
		CachedProperty<ZDecimal?> defaultIPIVigentRateValue;

		ZDecimal? GetDefaultIPIVigentRate() => IPIVigentRate?.ZZ2_RateFormulaDerivedFrom.ParseToDecimal();

		public ZBool IPIVigentRateValueReadOnly => !IPIRateIsOverridden;

		[BusinessObjectTestExclude()]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|IPIRateIsOverridden", ShortCaption = "Override", Caption = "IPI Rate Override")]
		[ReadOnlyMember(nameof(IPIRateIsOverriddenReadOnly))]
		public ZBool IPIRateIsOverridden
		{
			get => OverriddenIPITaxRate != null;
			set
			{
				var oldValue = IPIRateIsOverridden;
				if (!IsCopying && oldValue != value)
				{
					if (value)
					{
						IPIVigentRateValue = DefaultIPIVigentRateValue ?? ZDecimal.Zero;

						var addTariff = AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.ExIPITariff);
						if (addTariff == null)
						{
							addTariff = AdditionalTariffs.AddNew();
							addTariff.LegalActSubject = AdditionalTaxTypeList.Codes.ExIPITariff;
							addTariff.TariffType = ChildTariffTypeList.Codes.IPI;
						}
					}
					else
					{
						OverriddenIPITaxRate?.Delete();
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateIPIRateIsOverridden();
				}

				IPIRateIsOverriddenInfo.RefreshBinding(value);
			}
		}

		public ZPropertyInfo IPIRateIsOverriddenInfo => GetZPropertyInfo(Schema.IPIRateIsOverridden);

		public bool IPIRateIsOverriddenReadOnly => IPITaxRegime_ReadOnly || IPITaxRegime == IPITaxRegimeList.Codes.Reduction;

		JobComInvoiceLineTax OverriddenIPITaxRate => Taxes.FindByTypeAndMethod(Constants.RateCodes.IPI, SpecialCaseTaxTypeList.Codes.AdValoremRate);

		#endregion

		#region PIS

		public bool PisCofinsIsOverriddenReadOnly => IsImportSiscomex && (PisCofinsTaxRegime == TaxRegimeList.Codes.Immunity || PisCofinsTaxRegime == TaxRegimeList.Codes.NoIncident || PisCofinsTaxRegime == TaxRegimeList.Codes.Reduction);

		public IZZRateSelectionCriteria PISVigentRateSelectionCriteria => Factory.GetCached(ref pisVigentRateSelectionCriteria, GetPISVigentRateSelectionCriteria);
		CachedProperty<IZZRateSelectionCriteria> pisVigentRateSelectionCriteria;

		IZZRateSelectionCriteria GetPISVigentRateSelectionCriteria() => new RateSelectionCriteria<BaseJobComInvoiceLine>(this, Constants.RateTypes.PIS, Constants.RateCodes.PIS);

		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(PisVigentRateValueReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|PisVigentRateValue", Caption = "Pis Ad Valorem Rate (%)")]
		public ZDecimal PisVigentRateValue
		{
			get => PisRateIsOverridden ? OverriddenPisTaxRate.JLT_Rate : DefaultPisVigentRateValue ?? ZDecimal.Zero;
			set
			{
				(OverriddenPisTaxRate ?? Taxes.AddNew(Constants.RateCodes.PIS, SpecialCaseTaxTypeList.Codes.AdValoremRate)).JLT_Rate = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidatePisVigentRateValue();
				}

				PisVigentRateValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PisVigentRateValueInfo => GetZPropertyInfo(Schema.PisVigentRateValue);

		public ZBool PisVigentRateValueReadOnly => !PisRateIsOverridden;

		public ZDecimal? DefaultPisVigentRateValue => Factory.GetCached(ref defaultPisVigentRateValue, GetDefaultPISVigentRate);
		CachedProperty<ZDecimal?> defaultPisVigentRateValue;

		ZDecimal? GetDefaultPISVigentRate() => PISVigentRate?.ZZ2_RateFormulaDerivedFrom.ParseToDecimal();

		public RateView PISVigentRate => UniversalTariff?.GetApplicableRate(PISVigentRateSelectionCriteria);

		#region PisRateIsOverridden

		[BusinessObjectTestExclude()]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|PisRateIsOverridden", ShortCaption = "Override", Caption = "Pis Rate Override")]
		[ReadOnlyMember(nameof(PisCofinsIsOverriddenReadOnly))]
		public ZBool PisRateIsOverridden
		{
			get => OverriddenPisTaxRate != null;
			set
			{
				var oldValue = PisRateIsOverridden;
				if (!IsCopying && oldValue != value)
				{
					if (value)
					{
						PisVigentRateValue = DefaultPisVigentRateValue ?? ZDecimal.Zero;
					}
					else
					{
						OverriddenPisTaxRate?.Delete();
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidatePisRateIsOverridden();
				}

				PisRateIsOverriddenInfo.RefreshBinding(value);
			}
		}

		public ZPropertyInfo PisRateIsOverriddenInfo => GetZPropertyInfo(Schema.PisRateIsOverridden);

		JobComInvoiceLineTax OverriddenPisTaxRate => Taxes.FindByTypeAndMethod(Constants.RateCodes.PIS, SpecialCaseTaxTypeList.Codes.AdValoremRate);

		#endregion

		#endregion

		#region Cofins

		public IZZRateSelectionCriteria CofinsVigentRateSelectionCriteria => Factory.GetCached(ref cofinsVigentRateSelectionCriteria, GetCofinsVigentRateSelectionCriteria);
		CachedProperty<IZZRateSelectionCriteria> cofinsVigentRateSelectionCriteria;

		IZZRateSelectionCriteria GetCofinsVigentRateSelectionCriteria() => new RateSelectionCriteria<BaseJobComInvoiceLine>(this, Constants.RateTypes.Cofins, Constants.RateCodes.Cofins);

		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(CofinsVigentRateValueReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|CofinsVigentRateValue", Caption = "Cofins Ad Valorem Rate (%)")]
		public ZDecimal CofinsVigentRateValue
		{
			get => CofinsRateIsOverridden ? OverriddenCofinsTaxRate.JLT_Rate : DefaultCofinsVigentRateValue ?? ZDecimal.Zero;
			set
			{
				(OverriddenCofinsTaxRate ?? Taxes.AddNew(Constants.RateCodes.Cofins, SpecialCaseTaxTypeList.Codes.AdValoremRate)).JLT_Rate = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateCofinsVigentRateValue();
				}

				CofinsVigentRateValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CofinsVigentRateValueInfo => GetZPropertyInfo(Schema.CofinsVigentRateValue);

		public ZBool CofinsVigentRateValueReadOnly => !CofinsRateIsOverridden;

		public ZDecimal? DefaultCofinsVigentRateValue => Factory.GetCached(ref defaultCofinsVigentRateValue, GetDefaultCofinsVigentRate);
		CachedProperty<ZDecimal?> defaultCofinsVigentRateValue;

		ZDecimal? GetDefaultCofinsVigentRate() => CofinsVigentRate?.ZZ2_RateFormulaDerivedFrom.ParseToDecimal();

		public RateView CofinsVigentRate => UniversalTariff?.GetApplicableRate(CofinsVigentRateSelectionCriteria);

		#region CofinsRateIsOverridden

		[BusinessObjectTestExclude()]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|CofinsRateIsOverridden", ShortCaption = "Override", Caption = "Cofins Rate Override")]
		[ReadOnlyMember(nameof(PisCofinsIsOverriddenReadOnly))]
		public ZBool CofinsRateIsOverridden
		{
			get => OverriddenCofinsTaxRate != null;
			set
			{
				var oldValue = CofinsRateIsOverridden;
				if (!IsCopying && oldValue != value)
				{
					if (value)
					{
						CofinsVigentRateValue = DefaultCofinsVigentRateValue ?? ZDecimal.Zero;
					}
					else
					{
						OverriddenCofinsTaxRate?.Delete();
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateCofinsRateIsOverridden();
				}

				CofinsRateIsOverriddenInfo.RefreshBinding(value);
			}
		}

		public ZPropertyInfo CofinsRateIsOverriddenInfo => GetZPropertyInfo(Schema.CofinsRateIsOverridden);

		JobComInvoiceLineTax OverriddenCofinsTaxRate => Taxes.FindByTypeAndMethod(Constants.RateCodes.Cofins, SpecialCaseTaxTypeList.Codes.AdValoremRate);

		#endregion

		#endregion

		#endregion

		#region Quantity Per Unit

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public QuantityPerUnitInfoCollection QuantityPerUnitInfos
		{
			get
			{
				if (fQuantityPerUnitInfos == null)
				{
					fQuantityPerUnitInfos = new QuantityPerUnitInfoCollection(this);
					fQuantityPerUnitInfos.Load();
					RegisterEditableChildObject(fQuantityPerUnitInfos);
				}
				return fQuantityPerUnitInfos;
			}
		}

		QuantityPerUnitInfoCollection fQuantityPerUnitInfos;

		#endregion

		#region Legal Act Collection

		[ChildEditable(true)]
		public LegalActInfoCollection LegalActInfos
		{
			get
			{
				if (fLegalActCollection == null)
				{
					fLegalActCollection = new LegalActInfoCollection(this);
					fLegalActCollection.Load();
					RegisterEditableChildObject(fLegalActCollection);
				}
				return fLegalActCollection;
			}
		}

		LegalActInfoCollection fLegalActCollection;

		#endregion

		#region IPI Tax Benefit Legal Act

		LegalActInfo IPITaxBenefitLegalAct => LegalActInfos.FindBySubject(AdditionalTaxTypeList.Codes.IPITaxBenefit) ?? LegalActInfos.AddNew(AdditionalTaxTypeList.Codes.IPITaxBenefit);

		[ReadOnlyMember(nameof(IPILegalBase_ReadOnly))]
		[MaxLength(LegalActInfo.Schema.LegalActTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ExTariffLegalActList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|IPITaxBenefitLegalActType", ShortCaption = "Legal Act", Caption = "IPI Legal Act")]
		public ZString IPITaxBenefitLegalActType
		{
			get => IPITaxBenefitLegalAct?.CSI_Code ?? ZString.Empty;
			set
			{
				IPITaxBenefitLegalAct.CSI_Code = value;
				IPITaxBenefitLegalActTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IPITaxBenefitLegalActTypeInfo => GetWrappedZPropertyInfo(Schema.IPITaxBenefitLegalActType, x => IPITaxBenefitLegalAct.CSI_CodeInfo);

		[ReadOnlyMember(nameof(IPILegalBase_ReadOnly))]
		[MaxLength(LegalActInfo.Schema.LegalActIssuingBodyMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.LegalActIssuingAuthorityList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|IPITaxBenefitLegalActIssuingBody", ShortCaption = "Issuing Body", Caption = "IPI Issuing Body")]
		public ZString IPITaxBenefitLegalActIssuingBody
		{
			get => IPITaxBenefitLegalAct?.CSI_IssuerType ?? ZString.Empty;
			set
			{
				IPITaxBenefitLegalAct.CSI_IssuerType = value;
				IPITaxBenefitLegalActIssuingBodyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IPITaxBenefitLegalActIssuingBodyInfo => GetWrappedZPropertyInfo(Schema.IPITaxBenefitLegalActIssuingBody, x => IPITaxBenefitLegalAct.CSI_IssuerTypeInfo);

		[ReadOnlyMember(nameof(IPILegalBase_ReadOnly))]
		[MaxLength(LegalActInfo.Schema.LegalActNumberMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|IPITaxBenefitLegalActNumber", ShortCaption = "Act Number", Caption = "IPI Act Number")]
		public ZString IPITaxBenefitLegalActNumber
		{
			get => IPITaxBenefitLegalAct?.CSI_ReferenceNumber ?? ZString.Empty;
			set
			{
				IPITaxBenefitLegalAct.CSI_ReferenceNumber = value;
				IPITaxBenefitLegalActNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IPITaxBenefitLegalActNumberInfo => GetWrappedZPropertyInfo(Schema.IPITaxBenefitLegalActNumber, x => IPITaxBenefitLegalAct.CSI_ReferenceNumberInfo);

		[ReadOnlyMember(nameof(IPILegalBase_ReadOnly))]
		[MaxLength(LegalActInfo.Schema.LegalActYearMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|IPITaxBenefitLegalActYear", ShortCaption = "Year", Caption = "IPI Year")]
		public ZString IPITaxBenefitLegalActYear
		{
			get => IPITaxBenefitLegalAct?.CSI_YearOfIssue ?? ZString.Empty;
			set
			{
				IPITaxBenefitLegalAct.CSI_YearOfIssue = value;
				IPITaxBenefitLegalActYearInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo IPITaxBenefitLegalActYearInfo => GetWrappedZPropertyInfo(Schema.IPITaxBenefitLegalActYear, x => IPITaxBenefitLegalAct.CSI_YearOfIssueInfo);

		#endregion

		#region Antidumping Legal Act

		LegalActInfo AntidumpingLegalAct => LegalActInfos.FindBySubject(AdditionalTaxTypeList.Codes.Antidumping);

		public ZString AntidumpingLegalActType => AntidumpingLegalAct?.CSI_Code ?? ZString.Empty;

		public ZString AntidumpingLegalActIssuingBody => AntidumpingLegalAct?.CSI_IssuerType ?? ZString.Empty;

		public ZString AntidumpingLegalActNumber => AntidumpingLegalAct?.CSI_ReferenceNumber ?? ZString.Empty;

		public ZString AntidumpingLegalActYear => AntidumpingLegalAct?.CSI_YearOfIssue ?? ZString.Empty;

		#endregion

		#region Goods Catalog

		public override ZGuid JI_CGC_Catalog
		{
			get => base.JI_CGC_Catalog;
			set
			{
				var oldValue = base.JI_CGC_Catalog;
				base.JI_CGC_Catalog = value;
				if (!IsCopying && oldValue != JI_CGC_Catalog)
				{
					var goodsCatalog = GoodsCatalog;
					JI_CatalogAuthorityIdentifier = goodsCatalog?.CGC_AuthorityIdentifier ?? ZString.Empty;
					JI_CatalogAuthorityVersion = goodsCatalog?.CGC_AuthorityVersion ?? ZString.Empty;
					if (goodsCatalog != null)
					{
						JI_Tariff = goodsCatalog.CGC_Tariff.Truncate(JI_TariffInfo.MaxLength);
					}
				}
			}
		}

		#region Authority Identifier

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_CatalogAuthorityIdentifier", Caption = "Catalog Authority", ShortCaption = "Catalog Authority", FullDescription = "Goods Catalog Authority Identifier")]
		public override ZString JI_CatalogAuthorityIdentifier
		{
			get => base.JI_CatalogAuthorityIdentifier;
			set => base.JI_CatalogAuthorityIdentifier = value;
		}

		#endregion

		#region Authority Version

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_CatalogAuthorityVersion", Caption = "Catalog Version", ShortCaption = "Catalog Version", FullDescription = "Goods Catalog Authority Version")]
		public override ZString JI_CatalogAuthorityVersion
		{
			get => base.JI_CatalogAuthorityVersion;
			set => base.JI_CatalogAuthorityVersion = value;
		}

		#endregion

		#endregion

		protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.Brazil;
		protected override Type TypeOfPartUsedCore => typeof(OrgSupplierPart);
		public new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

		public bool IsImportLicense => Declaration?.IsImportLicense ?? false;

		public bool IsImportSiscomex => Declaration?.IsImportSiscomex ?? false;

		public bool IsImportExcludingLicense => !IsImportLicense && IsImport;

		public bool IsImportOnly => InvoiceHeader?.IsImportOnly ?? false;

		public bool IsLPCO => Declaration?.IsLPCO ?? false;

		public bool IsAttachedToPersistentDeclaration => InvoiceHeader?.IsAttachedToPersistentDeclaration ?? false;

		public bool IsClonedFrom(CusEntryInstruction entryInstruction)
		{
			var sourceLine = ParentTariffLine;
			return sourceLine != null && sourceLine.JI_CEI == entryInstruction.PK;
		}

		public bool HasLinkedInvoiceLine => IsImport && JI_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix && JI_ParentID.IsValid;

		internal MergeKey LastMergeKeyForImportLicenseEntry { get; set; }

		protected override bool AllowParentTariffLineAndThisLineHavingDifferentHeader => true;

		public override ZString JI_ParentTableCode
		{
			get => base.JI_ParentTableCode;
			set
			{
				var oldValue = JI_ParentTableCode;
				base.JI_ParentTableCode = value;
				if (!IsCopying && oldValue != JI_ParentTableCode)
				{
					InvoiceHeader?.MarkAsNeedingValidation();
					TariffDetachs?.MarkAsNeedingValidation();
				}
			}
		}

		protected override void SetTariffEtcDataFromProductsPivotCore(BaseCusClassPartPivot pivot)
		{
			if (pivot.CI_CGC_Catalog.IsEmpty)
			{
				base.SetTariffEtcDataFromProductsPivotCore(pivot);
			}
		}

		#region Overseas Charges

		#region Overseas Freight

		public ZDecimal OverseasFreightInLocalCurrency
		{
			get { return InvoiceHeader != null ? ConvertToLocalAmountExact(JI_OverseasFreight).Amount : ZDecimal.Zero; }
		}

		public override Money JI_OverseasFreight
		{
			get
			{
				var result = Money.Empty;

				if (InvoiceHeader != null)
				{
					if (IsImport)
					{
						foreach (var charge in ImportCommonChargesProvider.OverseasFreightCharges)
						{
							result = CurrencyConverter.Add(result, GetCharge(charge));
						}
					}
					else
					{
						result = base.JI_OverseasFreight;
					}
				}
				return result;
			}
		}

		#endregion

		#region Overseas Insurance

		public ZDecimal OverseasInsuranceInLocalCurrency
		{
			get { return InvoiceHeader != null ? ConvertToLocalAmountExact(JI_OverseasInsurance).Amount : ZDecimal.Zero; }
		}

		#endregion

		#endregion

		#region Taxes

		[BusinessObjectTestExclude]
		[UniversalCopyCollectionEntity(JobComInvoiceLineTaxSchema.Constants.TableName, JobComInvoiceLineTaxSchema.Constants.JLT_JI)]
		[ChildEditable(true)]
		public JobComInvoiceLineTaxCollection Taxes
		{
			get
			{
				if (fTaxes == null)
				{
					fTaxes = new JobComInvoiceLineTaxCollection(this);
					fTaxes.Load();
					RegisterEditableChildObject(fTaxes);
				}
				return fTaxes;
			}
		}
		JobComInvoiceLineTaxCollection fTaxes;

		#region OverriddenDutyRate

		JobComInvoiceLineTax OverriddenDutyRate => Taxes.FindByType(Constants.RateCodes.ImportDuty);

		public ZDecimal OverriddenDutyRateValue
		{
			get => OverriddenDutyRate?.JLT_Rate ?? ZDecimal.Zero;
			set => (OverriddenDutyRate ?? Taxes.AddNew(Constants.RateCodes.ImportDuty, GetMethodOfCalculationCode(JI_PrimaryPreference))).JLT_Rate = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.RatePreferencesList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|JI_PrimaryPreference", Caption = "Rate Preference")]
		public override ZString JI_PrimaryPreference
		{
			get => base.JI_PrimaryPreference;
			set
			{
				var oldValue = JI_PrimaryPreference;
				base.JI_PrimaryPreference = value;
				if (!IsCopying && JI_PrimaryPreference != oldValue)
				{
					if (JI_PrimaryPreference == Constants.RatePreferenceType.Normal)
					{
						DutyRateIsOverridden = false;
					}
					if (IsImportSiscomex)
					{
						if (DutyRateIsOverridden)
						{
							OverriddenDutyRate.JLT_MethodOfCalculation = GetMethodOfCalculationCode(JI_PrimaryPreference);
						}
						switch (JI_PrimaryPreference)
						{
							case RatePreferenceType.ExTariff:
								if (AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.ExDutyTariff) == null)
								{
									AdditionalTariffs.AddNew().LegalActSubject = AdditionalTaxTypeList.Codes.ExDutyTariff;
								}
								break;
							case RatePreferenceType.FreeTradeAgreement:
								if (AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.TariffAgreement) == null)
								{
									AdditionalTariffs.AddNew().LegalActSubject = AdditionalTaxTypeList.Codes.TariffAgreement;
								}
								break;
						}

						switch (oldValue)
						{
							case RatePreferenceType.ExTariff:
								if (AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.ExDutyTariff) is var exTariff && exTariff != null)
								{
									AdditionalTariffs.RemoveAndDelete(exTariff);
								}
								break;
							case RatePreferenceType.FreeTradeAgreement:
								if (AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.TariffAgreement) is var tariffAgreement && tariffAgreement != null)
								{
									AdditionalTariffs.RemoveAndDelete(tariffAgreement);
								}
								break;
						}

						AdditionalTariffs.Cast<AdditionalTariff>().ForEach(x => x.DefaultLegalActInformation());
						AdditionalTariffs.RefreshBinding();
					}
				}
			}
		}

		public ZDecimal NormalDutyRateValue => Factory.GetCached(ref normalDutyRateValue, () => ZDecimal.ParseSafe(UniversalTariff?.GetApplicableRate(NormalDutyRateSelectionCriteria)?.ZZ2_RateFormulaDerivedFrom ?? ZString.Empty, ZDecimal.Zero));
		CachedProperty<ZDecimal> normalDutyRateValue;

		public IZZRateSelectionCriteria NormalDutyRateSelectionCriteria => Factory.GetCached(ref normalTariffDutyRateSelectionCriteria, GetNormalDutyRateSelectionCriteria);
		CachedProperty<IZZRateSelectionCriteria> normalTariffDutyRateSelectionCriteria;

		IZZRateSelectionCriteria GetNormalDutyRateSelectionCriteria() => new NormalRateSelectionCriteria(this, Universal.Constants.RateTypes.Duty, Constants.RateCodes.ImportDuty);

		internal class NormalRateSelectionCriteria : RateSelectionCriteria<JobComInvoiceLine>
		{
			public NormalRateSelectionCriteria(JobComInvoiceLine invoiceLine, ZString rateType, ZString rateCode) : base(invoiceLine, rateType, rateCode)
			{
			}

			protected override ZString GetPrimaryPreference(JobComInvoiceLine invoiceLine) => Constants.RatePreferenceType.Normal;
		}

		#endregion

		#endregion

		#region Manufacturer Doc Address

		public JobDocAddress ManufacturerDocAddress
		{
			get
			{
				if (fManufacturerDocAddress == null || fManufacturerDocAddress.IsDeleted)
				{
					fManufacturerDocAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Manufacturer);
				}
				fManufacturerDocAddress.ReadOnly = ManufacturerAddress_ReadOnly;
				fManufacturerDocAddress.DocAddressChanged += ManufactureAddress_DocAddressChanged;

				return fManufacturerDocAddress;
			}
		}

		JobDocAddress fManufacturerDocAddress;

		void ManufactureAddress_DocAddressChanged(object sender, EventArgs e)
		{
			DefaultCountryOriginIfNeeded();
			PopulateValuesFromForeignOperator();
		}

		#endregion

		#region Manufacturer Doc Address Organisation PK

		[ReadOnlyMember(nameof(ManufacturerAddress_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SupplierList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|ManufacturerDocOrgPK", Caption = "Manufacturer")]
		public ZGuid ManufacturerDocOrgPK
		{
			get => ManufacturerDocAddress.OrganisationPK;
			set
			{
				ManufacturerDocAddress.OrganisationPK = value;
				ManufacturerDocOrgPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ManufacturerDocOrgPKInfo => GetWrappedZPropertyInfo(Schema.ManufacturerDocOrgPK, x => ManufacturerDocAddress.OrganisationPKInfo);

		void PopulateValuesFromForeignOperator()
		{
			JI_ManufacturerAuthorityIdentifier = IsImportOnly && ForeignOperator != null ? ForeignOperator.BFR_AuthorityIdentifier : ZString.Empty;
			JI_ManufacturerAuthorityVersion = IsImportOnly && ForeignOperator != null ? ForeignOperator.BFR_AuthorityVersion : ZString.Empty;
		}

		#endregion

		#region Manufacturer Doc Address PK

		[ReadOnlyMember(nameof(ManufacturerAddress_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|ManufacturerDocAddressPK", Caption = "Manufacturer Address")]
		public ZGuid ManufacturerDocAddressPK
		{
			get => ManufacturerDocAddress.E2_OA_Address;
			set
			{
				ManufacturerDocAddress.E2_OA_Address = value;
				ManufacturerDocAddressPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ManufacturerDocAddressPKInfo => GetWrappedZPropertyInfo(Schema.ManufacturerDocAddressPK, x => ManufacturerDocAddress.E2_OA_AddressInfo);

		#endregion

		#region ManufacturerCode

		public ZString ManufacturerOrgCode => (IsImportLicense ? ManufacturerDocAddress?.Organisation : ManufacturerAddress?.Header)?.OH_Code ?? ZString.Empty;

		#endregion

		#region Calculated Amounts

		public ZDecimal JI_Calc_InvAmount => Factory.GetCached(ref cachedJI_Calc_InvAmount, GetJI_Calc_InvAmount);
		CachedProperty<ZDecimal> cachedJI_Calc_InvAmount;

		ZDecimal GetJI_Calc_InvAmount()
		{
			return JI_LinePrice + GetAmountOnCharges(charge => charge.J7_Calc_IsIncludedInInvoiceAmount && !charge.J7_IsIncludedInITOT, LinePriceRefCurrency);
		}

		public ZDecimal FOBValueForApportionment => Factory.GetCached(ref cachedFOBValueForApportionment, GetFOBValueForApportionment);
		CachedProperty<ZDecimal> cachedFOBValueForApportionment;

		ZDecimal GetFOBValueForApportionment()
		{
			var currency = LinePriceRefCurrency;
			return JI_LinePrice
				+ GetAmountOnCharges(charge => charge.J7_IsDutiable && !charge.J7_IsIncludedInITOT, currency)
				- GetAmountOnCharges(charge => charge.J7_IsIncludedInITOT, currency);
		}

		public ZDecimal JI_Calc_EICAmount => Factory.GetCached(ref cachedJI_Calc_EICAmount, () => GetAmountOnCharges(ImportCustomsChargeTypeList.Codes.OtherExpensesICMS, LocalCurrency));
		CachedProperty<ZDecimal> cachedJI_Calc_EICAmount;

		internal ZDecimal GetAmountOnCharges(string chargeType, RefCurrency currency) => GetAmountOnCharges(c => c.J7_ChargeType == chargeType, currency);

		internal ZDecimal GetAmountOnCharges(Func<JobComInvCharge, bool> predicate, RefCurrency currency, bool roundToDestinationCurrencyDecimals = true)
		{
			var result = 0m;

			if (currency != null)
			{
				result += GetAllCharges(predicate).Sum(charge => CurrencyConverter.ConvertExact(charge.Money, currency, roundToDestinationCurrencyDecimals: false).Amount * (charge.IsDiscount ? -1 : 1));

				if (roundToDestinationCurrencyDecimals)
				{
					result = Utilities.Round(result, currency.Decimals);
				}
			}
			return result;
		}

		internal IEnumerable<JobComInvCharge> GetAllCharges(Func<JobComInvCharge, bool> predicate) => Charges.Cast<JobComInvCharge>().Union(ApportionedCharges).Where(predicate);

		public ZDecimal JI_Calc_DutyBaseAmount => GetFeeApportionedFromCusEntryLine(ChargeTypesList.Codes.DTY, x => x.CF_BaseValueInfo);

		public ZDecimal JI_Calc_IPIBaseAmount => GetFeeApportionedFromCusEntryLine(RateTypes.IPI, x => x.CF_BaseValueInfo);

		public ZDecimal JI_Calc_IPIAmount => GetFeeApportionedFromCusEntryLine(RateTypes.IPI, x => x.CF_ChargeAmountInfo);

		public ZDecimal JI_Calc_PISBaseAmount => GetFeeApportionedFromCusEntryLine(RateTypes.PIS, x => x.CF_BaseValueInfo);

		public ZDecimal JI_Calc_PISAmount => GetFeeApportionedFromCusEntryLine(RateTypes.PIS, x => x.CF_ChargeAmountInfo);

		public ZDecimal JI_Calc_CofinsBaseAmount => GetFeeApportionedFromCusEntryLine(RateTypes.Cofins, x => x.CF_BaseValueInfo);

		public ZDecimal JI_Calc_CofinsAmount => GetFeeApportionedFromCusEntryLine(RateTypes.Cofins, x => x.CF_ChargeAmountInfo);

		public ZDecimal JI_Calc_ICMSBaseAmount => CalculatedICMSFCPFee.BaseAmount.GetValueOrDefault().Round(8);

		public ZDecimal JI_Calc_ICMSAmount => CalculatedICMSFCPFee.ICMSAmount.GetValueOrDefault().Round(8);

		public ZDecimal JI_Calc_FCPAmount => CalculatedICMSFCPFee.FCPAmount.GetValueOrDefault().Round(10);

		public ZDecimal JI_Calc_AntidumpingBaseAmount => GetFeeApportionedFromCusEntryLine(RateTypes.Antidumping, x => x.CF_BaseValueInfo);

		public ZDecimal JI_Calc_AntidumpingAmount => GetFeeApportionedFromCusEntryLine(RateTypes.Antidumping, x => x.CF_ChargeAmountInfo);

		public ZDecimal JI_Calc_AfrmmAmount => GetFeeApportionedFromCusEntryLineByNetWeight(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax, x => x.CF_ChargeAmountInfo);

		public ZDecimal JI_Calc_SiscomexUsageAmount => GetFeeApportionedFromCusEntryLine(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee, x => x.CF_ChargeAmountInfo);

		public ZDecimal JI_Calc_ImportLicenseFineAmount => GetFeeApportionedFromCusEntryLine(Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.NoDiscountCode, x => x.CF_ChargeAmountInfo) +
															GetFeeApportionedFromCusEntryLine(Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.FiftyPercentDiscountCode, x => x.CF_ChargeAmountInfo);

		ZDecimal GetFeeApportionedFromCusEntryLine(string feeType, Func<CusEntryLineFee, ZPropertyInfo> getValueInfo)
		{
			var fee = CusEntryLine?.Fees.GetElementWithThisCode(feeType) as CusEntryLineFee;
			var amount = fee != null && getValueInfo(fee) is ZPropertyInfoDecimal decimalInfo ? decimalInfo.Value : ZDecimal.Zero;
			return CusEntryLine == null ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLine(amount).Amount;
		}

		protected override ZDecimal ProportionOfCusEntryLine()
		{
			ZDecimal result = 1m;

			if (Declaration != null && !Declaration.IsMergeInProgress && CusEntryLine != null && CusEntryLine.InvoiceLines.Count > 1)
			{
				if (CusEntryLine.CL_CustomsValue != 0m)
				{
					result = JI_CustomsValue / CusEntryLine.CL_CustomsValue;
				}
			}

			return result;
		}

		ZDecimal GetFeeApportionedFromCusEntryLineByNetWeight(string feeType, Func<CusEntryLineFee, ZPropertyInfo> getValueInfo)
		{
			var fee = CusEntryLine?.Fees.GetElementWithThisCode(feeType) as CusEntryLineFee;
			var amount = fee != null && getValueInfo(fee) is ZPropertyInfoDecimal decimalInfo ? decimalInfo.Value : ZDecimal.Zero;
			return CusEntryLine == null ? new ZDecimal(0m) : GetAmountApportionedFromCusEntryLineByNetWeight(amount).Amount;
		}

		Money GetAmountApportionedFromCusEntryLineByNetWeight(ZDecimal cusEntryLineAmount)
		{
			ZDecimal amount = 0m;
			var amountIsValid = false;

			if (Declaration != null && !Declaration.IsMergeInProgress && CusEntryLine != null && cusEntryLineAmount.IsValid)
			{
				amount = cusEntryLineAmount * ProportionOfCusEntryLineByNetWeight;
				amountIsValid = true;
			}

			return new Money(amount, LocalCurrency, amountIsValid).Round(8);
		}

		ZDecimal ProportionOfCusEntryLineByNetWeight => Factory.GetValue(ref proportionOfCusEntryLineByNetWeight, () =>
		{
			ZDecimal result = 1m;

			if (Declaration != null && !Declaration.IsMergeInProgress && CusEntryLine != null && CusEntryLine.InvoiceLines.Count > 1)
			{
				if (CusEntryLine.EffectiveNetWeight.InKilogramsSafe != 0m)
				{
					result = NetWeightInKG / CusEntryLine.EffectiveNetWeight.InKilogramsSafe;
				}
			}

			return result;
		});
		CachedProperty<ZDecimal> proportionOfCusEntryLineByNetWeight;

		ICMSFCPFeeCalculator.ICMSFCPFee CalculatedICMSFCPFee => Factory.GetValue(ref cachedICMSFCPFee, () => new ICMSFCPFeeCalculator().Calculate(this, this));
		CachedProperty<ICMSFCPFeeCalculator.ICMSFCPFee> cachedICMSFCPFee;

		#endregion

		#region CanDelete

		internal static ZQuery GetChildInvoiceLinesQuery(string jobMessageType, params ZGuid[] invoiceLinePKs)
		{
			if (invoiceLinePKs?.Length > 0)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), JobDeclarationSchema.JE_ClusterKey, JobComInvoiceLineSchema.JI_ClusterKey);
				subQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, jobMessageType);
				var query = new ZDBOnlyQuery(typeof(JobComInvoiceLine));
				query.AddToFilter(JobComInvoiceLineSchema.JI_ParentID, invoiceLinePKs);
				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
			else
			{
				return ZQuery.NoResultQuery;
			}
		}

		internal static bool AttachedToImportLicenseLines(BusinessObjectFactory factory, params JobComInvoiceLine[] lines)
		{
			if (lines.Length > 0)
			{
				var query = new ZQuery(JobComInvoiceLineSchema.JI_ParentID, lines.Select(x => x.PK).ToArray());
				query.FetchOnlyFromLocalCache = lines.All(x => !x.IsInDatabase);
				return factory.Load<JobComInvoiceLine>(query).Any(x => x.IsImportLicense);
			}
			return false;
		}

		public bool AttachedToImportLicenseLine => Factory.GetCached(ref attachedToImportLicenseLine, () => AttachedToImportLicenseLines(Factory, this));
		CachedProperty<bool> attachedToImportLicenseLine;

		protected override bool CanDeleteCore => base.CanDeleteCore && !IsImportLicenseGeneratedFromImportSiscomexLine && !AttachedToImportLicenseLine;

		protected override MultilingualString ReasonForNotAbleToDeleteCore
		{
			get
			{
				if (IsImportLicenseGeneratedFromImportSiscomexLine)
				{
					return ResString.GetMultilingualString("55EA9AD6-54AD-4DFA-BCBB-4C832C8B0CF3", "The License Line can not be deleted, because it is attached to an Import Entry.");
				}
				if (AttachedToImportLicenseLine)
				{
					return ResString.GetMultilingualString("98316021-22B6-4FCE-8A08-F1E065F599CA", "The Invoice Line cannot be deleted, because there is Import License line reference it.");
				}
				return base.ReasonForNotAbleToDeleteCore;
			}
		}

		#endregion

		#region Attached Invoice Line

		public JobComInvoiceLine AttachedImportSiscomexLine => JI_ParentID.IsValid && ParentTariffLine is JobComInvoiceLine invoiceLine && invoiceLine.IsImportSiscomex ? invoiceLine : null;

		public JobComInvoiceLine AttachedImportLicenseLine => JI_ParentID.IsValid && ParentTariffLine is JobComInvoiceLine invoiceLine && invoiceLine.IsImportLicense ? invoiceLine : null;

		public bool IsImportSiscomexWithGeneratedImportLicenseLine => IsImportSiscomex && IsGenerated(AttachedImportLicenseLine);

		public bool IsImportLicenseGeneratedFromImportSiscomexLine => IsImportLicense && IsGenerated(AttachedImportSiscomexLine);

		bool IsGenerated(JobComInvoiceLine invoiceLine) => invoiceLine != null && JI_ParentID == invoiceLine.PK && invoiceLine.JI_ParentID == PK;

		#endregion

		#region Permit

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public PermitCollection Permits
		{
			get
			{
				if (fPermits == null)
				{
					fPermits = new PermitCollection(this);
					fPermits.Load();
					RegisterEditableChildObject(fPermits);
				}
				return fPermits;
			}
		}

		PermitCollection fPermits;

		#endregion

		#region DuimpTaxRegime

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public DuimpTaxRegimeCollection DuimpTaxRegimes
		{
			get
			{
				if (fDuimpTaxRegimes == null)
				{
					fDuimpTaxRegimes = new DuimpTaxRegimeCollection(this);
					fDuimpTaxRegimes.Load();
					RegisterEditableChildObject(fDuimpTaxRegimes);
				}
				return fDuimpTaxRegimes;
			}
		}
		DuimpTaxRegimeCollection fDuimpTaxRegimes;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.DuimpLegalBaseList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceLine|DuimpLegalBase", ShortCaption = "Legal Basis", Caption = "Legal Basis", FullDescription = "Select Legal Basis to include.")]
		public ZString DuimpLegalBase
		{
			get => duimpLegalBase;
			set
			{
				SetNonPersistentPropertyValue(DuimpLegalBaseInfo, ref duimpLegalBase, value);
				DuimpLegalBaseInfo.RefreshBinding();
			}
		}
		ZString duimpLegalBase;

		public ZPropertyInfo DuimpLegalBaseInfo => GetZPropertyInfo(Schema.DuimpLegalBase);

		public void AddDuimpTaxRegimes()
		{
			var profiles = GetRequiredTTProfiles(isMandatory: false).Where(w => w.LegalCode == DuimpLegalBase);
			profiles.ForEach(x => DuimpTaxRegimes.AddNew(x));
			DuimpLegalBase = ZString.Empty;

			TaxRegimeAttributes.Rebuild();
		}

		public void RemoveDuimpTaxRegimes(IEnumerable<DuimpTaxRegime> taxRegimes)
		{
			var legalCodes = taxRegimes.Where(w => !w.IsMandatory).Select(x => x.CSI_Procedure).ToArray();

			var removeList = DuimpTaxRegimes.Where(w => legalCodes.Contains(w.CSI_Procedure) && !w.IsDeleted).ToList();
			foreach (var taxRegimeToRemove in removeList)
			{
				taxRegimeToRemove.Delete();
			}
			TaxRegimeAttributes.Rebuild();
		}

		public TariffProfile[] GetRequiredTTProfiles(bool? isMandatory = null)
		{
			TariffProfile[] profiles = null;

			if (UniversalTariff is { } tariff && !JI_CountryOfOrigin.IsEmpty)
			{
				profiles = Declaration.GetTariffProfilesFromMessage(JI_Tariff, JI_CountryOfOrigin) ?? tariff?.GetTariffTTProfiles(JI_CountryOfOrigin, EffectiveAssessmentDate);
			}
			return profiles?.Where(p => !isMandatory.HasValue || p.IsMandatory == isMandatory).ToArray() ?? [];
		}

		#endregion

		public CusBRForeignOperator ForeignOperator
		{
			get
			{
				var ownerPK = Declaration?.JE_OH_Importer ?? ZGuid.Empty;
				return new CusBRForeignOperator.Loader(Factory).LoadByOwnerAndForeignOperator(ownerPK, ManufacturerOrgPK);
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (!IsDeleted && Declaration != null && (!Declaration.IsInDatabase || Declaration.JE_MessageTypeInfo.HasChanges))
			{
				if (!IsExport)
				{
					CertificateOfOriginCollection.RemoveAndDeleteAll();
					ComplementaryLogisticInvoiceCollection.RemoveAndDeleteAll();
					ElectronicLogisticInvoiceCollection.RemoveAndDeleteAll();
					ReferenceInvoiceManualCollection.RemoveAndDeleteAll();
					SuspensionDrawbackCollection.RemoveAndDeleteAll();
					LPCOJobComInvLineRefsCollection.RemoveAndDeleteAll();
				}
				if (!IsImportExcludingLicense)
				{
					JI_PrimaryPreference = ZString.Empty;
					MercosulForeignDeclarations.RemoveAndDeleteAll();
					ImportLicenseInfos.RemoveAndDeleteAll();
					TaxRegimeCollection.DeleteBySubject(TaxRegimeTypeList.Codes.ICMS);
					SpecialCaseTaxes.RemoveAndDeleteAll();
					Taxes.RemoveAndDeleteAll();
				}
				if (!IsImportSiscomex && !IsImportLicense)
				{
					JI_TemporaryAdmissionReason = ZString.Empty;
				}
				if (!IsImportLicense)
				{
					ConsentingProcessCollection.RemoveAndDeleteAll();
					DrawbackImportLicenseCollection.RemoveAndDeleteAll();
					JI_UsedMaterialRegime = ZString.Empty;
					JI_UsedMaterialSerialNumber = ZString.Empty;
					JI_UsedMaterialManufactureYear = ZString.Empty;
					JI_Model = ZString.Empty;
					JI_BrandName = ZString.Empty;
					DocAddresses.FindByDocAddressType(DocAddressType.Manufacturer)?.Delete();
				}
				if (!IsImportSiscomex)
				{
					AdditionalTariffs.RemoveAndDeleteAll();
					LegalActInfos.RemoveAndDeleteAll();
					TaxRegimeCollection.DeleteBySubject(TaxRegimeTypeList.Codes.IPI);
					TaxRegimeCollection.DeleteBySubject(TaxRegimeTypeList.Codes.PisCofins);
					TaxRegimeCollection.DeleteBySubject(TaxRegimeTypeList.Codes.FMM);
					JI_ComplementaryNote = ZString.Empty;
				}
				if (!IsImport)
				{
					NVECusCodeDataCollection.RemoveAndDeleteAll();
					TariffDetachs.RemoveAndDeleteAll();
					TaxRegimeCollection.DeleteBySubject(TaxRegimeTypeList.Codes.Duty);
				}
				if (!IsImportOnly)
				{
					Permits.RemoveAndDeleteAll();
					DuimpTaxRegimes.RemoveAndDeleteAll();
				}
				if (!IsExport && !IsImportExcludingLicense)
				{
					PreviousDocuments.RemoveAndDeleteAll();
				}

				if (!IsExport && !IsImportOnly)
				{
					ComplementaryDescription = ZString.Empty;
				}
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				Taxes.RemoveAndDeleteAll();
			}

			base.Delete();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (IsInDatabase)
			{
				ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JI_ParentID), ConcurrencyPolicy.Strict);
			}
		}

		[ChildEditable(true)]
		[BusinessObjectTestExclude]
		[UniversalCopyCollectionEntity(CusLineTariffDetailSchema.Constants.TableName, CusLineTariffDetailSchema.Constants.BZ_ParentID)]
		public new ICusLineTariffDetailCollection<CusLineTariffDetail> CusLineTariffDetails => (CusLineTariffDetailCollection<CusLineTariffDetail>)base.CusLineTariffDetails;

		protected override ICusLineTariffDetailCollection<Customs.Business.CusLineTariffDetail> GetCusLineTariffDetails() => new CusLineTariffDetailCollection<CusLineTariffDetail>(this);

		#region IDocAddress implementation

		public ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new JobComInvoiceLineJobDocAddressValidation(addressToValidate, this);
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress) => Environment.Env.Security.None;

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return null;
		}

		public void DocAddressChanged(JobDocAddress docAddress)
		{
		}

		public void OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		public void OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		public void AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		public void OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		public bool CanDeleteAddress(JobDocAddress docAddress) => false;

		public OrgHeaderCollection GetOrgHeaderList(DocAddressType addressType) => new OrgHeaderCollection(Factory);

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}

				return fDocAddresses;
			}
		}
		JobDocAddressDependentCollection fDocAddresses;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => new DocAddressType[] { DocAddressType.Manufacturer };

		ZString IDocAddresses.HumanReadableName => HumanReadableNameCore;

		#endregion

		#region InvoiceUQDescInPortugueseBrazil

		public ZString InvoiceUQDescInPortugueseBrazil => Factory.GetInvoiceUQDescriptions(Lookups.InvoiceUQList, JI_InvoiceUQ).DescInPortugueseBrazil;

		#endregion

		#region IICMSCalculationParameters

		ZDecimal IICMSCalculationParameters.ICMSRate => JI_ICMSRate;
		ZDecimal IICMSCalculationParameters.ICMSFCPRate => ICMSFCPRateValue;
		ZDecimal IICMSCalculationParameters.ICMSBaseValueReductionPercentage => JI_ICMSBaseValueReductionPercentage;
		ZDecimal IICMSCalculationParameters.ICMSTotalAmountReductionPercentage => JI_ICMSTotalAmountReductionPercentage;
		ZString IICMSCalculationParameters.ICMSFormula => JI_ICMSFormula;
		ZString IICMSCalculationParameters.ICMSTaxRegime => ICMSTaxRegime;

		#endregion

		#region IICMSCalculationValues

		ZDecimal IICMSCalculationValues.CustomsValue => JI_CustomsValue;
		ZDecimal IICMSCalculationValues.DutyAmount => JI_Calc_DutyAmount;
		ZDecimal IICMSCalculationValues.IPIAmount => JI_Calc_IPIAmount;
		ZDecimal IICMSCalculationValues.PISAmount => JI_Calc_PISAmount;
		ZDecimal IICMSCalculationValues.CofinsAmount => JI_Calc_CofinsAmount;
		ZDecimal IICMSCalculationValues.SiscomexUsageAmount => JI_Calc_SiscomexUsageAmount;
		ZDecimal IICMSCalculationValues.AfrmmTaxAmount => JI_Calc_AfrmmAmount;
		ZDecimal IICMSCalculationValues.AntidumpingAmount => JI_Calc_AntidumpingAmount;
		ZDecimal IICMSCalculationValues.EICAmount => JI_Calc_EICAmount;

		#endregion
	}
}

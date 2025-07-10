using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.IE.Business.Constants;
using static Enterprise.Customs.IE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class JobComInvoiceLine : AutoJobComInvoiceLine
		, Integration.Customs.IE.IJobComInvoiceLine
	{
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool HasAdditionalProcedureCodeConcessionF15 => JI_Calc_AdditionalProcedureCodes.Contains(UniversalReferenceConstants.ProcedureCodes.Concession.F15);

		public bool HasChargeType(ZString chargeType)
		{
			return Charges.Any(charge => charge.J7_ChargeType == chargeType) || ApportionedCharges.Any(apportionedCharges => apportionedCharges.J7_ChargeType == chargeType);
		}

		[ChildEditable(true)]
		public new IJobComInvChargeCollection<InvoiceLineCharge> Charges => (IJobComInvChargeCollection<InvoiceLineCharge>)base.Charges;

		[ChildEditable(true)]
		public new IJobComInvApportionedChargeCollection<InvoiceLineApportionCharge> ApportionedCharges => (IJobComInvApportionedChargeCollection<InvoiceLineApportionCharge>)base.ApportionedCharges;

		protected override IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> CreateNewInvoiceLineApportionedChargeCollection()
		{
			return new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>(this);
		}

		protected override IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection()
		{
			return new InvoiceLineChargeCollection<InvoiceLineCharge>(this);
		}

		public const int JI_DescriptionMaxLength_TransitionPeriodAES30 = 280;
		public const int JI_DescriptionMaxLength_AISUCC5 = 512;

		[ResourceStringData("{239139FB-CDFB-440E-B13D-C31CBB23A74E}", Caption = "Is Main Pack")]
		public override ZBool ZG_IsMainPack { get => base.ZG_IsMainPack; set => base.ZG_IsMainPack = value; }

		[ResourceStringData("E9010DE9-CAEA-4D9E-9DB4-D0F191329840", Caption = "Net Weight in KG", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		[ResourceStringData("DE98CB30-80AA-4B52-8AD7-E72C8A093729", ShortCaption = "[6/1] Net Mass", MediumCaption = "[6/1] Net Mass (kg)", FullDescription = "[6/1] Net Mass (kg)", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZDecimal JI_CustomsQuantity
		{
			get => base.JI_CustomsQuantity;
			set => base.JI_CustomsQuantity = value;
		}

		public override ZGuid JI_JZ
		{
			get => base.JI_JZ;
			set
			{
				var oldValue = JI_JZ;
				base.JI_JZ = value;
				if (!IsCopying && oldValue != JI_JZ)
				{
					InvoiceHeader?.MarkAsNeedingValidation();
					MarkChargesAsNeedingValidation();
				}
			}
		}

		public void MarkChargesAsNeedingValidation()
		{
			Charges.MarkAsNeedingValidation();
			ApportionedCharges.MarkAsNeedingValidation();
		}

		protected override ZBool ShouldCheckMissingPreviousDocumentsCore => !(InvoiceHeader?.JobDeclaration?.IsReExport ?? false) && base.ShouldCheckMissingPreviousDocumentsCore;
		[ResourceStringData("952f4e6e-8a33-4890-8cec-57d1c2c3d208", Caption = "Valuation Method")]
		[ResourceStringData("FBD2742E-46B3-4F5F-99D0-4A695FF9AD73", ShortCaption = "Val. Method", MediumCaption = "[4/16] Val. Method", Caption = "[4/16] Valuation Method", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString JI_ValuationCode
		{
			get => base.JI_ValuationCode;
			set => base.JI_ValuationCode = value;
		}

		[ResourceStringData("AF9A5DF6-1209-4991-AEBB-F7C2B7BAF664", Caption = "Price")]
		[ResourceStringData("9A52ABFB-9DE9-4DFC-BE55-A2B0430EE7CF", ShortCaption = "Item Price", MediumCaption = "[4/14] Item Price", Caption = "[4/14] Item Price / Amount", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZDecimal JI_LinePrice
		{
			get => base.JI_LinePrice;
			set => base.JI_LinePrice = value;
		}

		[ResourceStringData("2A77EFEA-9AA2-4955-AE28-B8A107337DF0", Caption = "Preference")]
		[ResourceStringData("19633536-A66A-45FE-A278-900DAFA4A8BB", ShortCaption = "Pref.", MediumCaption = "[4/17] Pref.", Caption = "[4/17] Preference", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString JI_PrimaryPreference
		{
			get => base.JI_PrimaryPreference;
			set => base.JI_PrimaryPreference = value;
		}

		public ZString PreferenceCode => JI_PrimaryPreference.Left(1);

		[ResourceStringData("F78017C9-988F-42E9-8A53-BE8F39B31531", ShortCaption = "[5/15] Orig. Ctry.", MediumCaption = "[5/15] Orig. Country", Caption = "[5/15] Origin Country", FullDescription = "[5/15] Country of Origin code", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString JI_CountryOfOrigin
		{
			get => GetDeclarationEffectiveValueToReturnIfNeeded(base.JI_CountryOfOrigin, Schema.JI_CountryOfOrigin, JobDeclarationSchema.Constants.JE_GoodsOrigin);
			set => base.JI_CountryOfOrigin = GetEffectiveValueToSetCompareToDeclaration(value, JobDeclarationSchema.Constants.JE_GoodsOrigin);
		}

		[ResourceStringData("99BF6393-8218-4CC2-AE0C-F31465E5ADD1", Caption = "Gross Weight", FullDescription = "[18 04 001 000] Gross Mass (KG)")]
		[ResourceStringData("CC115FEF-94EE-49A8-8C4E-85AEF1AD3516", ShortCaption = "[6/5] Gross Mass", MediumCaption = "[6/5] Gross Mass (kg)", Caption = "[6/5] Gross Mass (kg)", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZDecimal JI_Weight
		{
			get => base.JI_Weight;
			set
			{
				var oldValue = JI_Weight;
				base.JI_Weight = value;

				if (!IsCopying && oldValue != JI_Weight && !IsValidationSuspended)
				{
					Validation.ValidateJI_CustomsQuantity();
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
					Validation.ValidateJI_CustomsQuantity();
				}
			}
		}

		[ResourceStringData("D876C20F-0135-4A8C-A218-39DB368FB5F0", ShortCaption = "Suppl. Units", Caption = "Supplementary Units", FullDescription = "[18 02 001 000] Supplementary Units")]
		[ResourceStringData("5BD33442-AEF6-4329-AB3B-C553EC49014A", ShortCaption = "[6/2] Suppl. Units", MediumCaption = "[6/2] Suppl. Units", Caption = "[6/2] Supplementary Units", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZDecimal JI_CustomsSecondQuantity
		{
			get => base.JI_CustomsSecondQuantity;
			set => base.JI_CustomsSecondQuantity = value;
		}

		[ResourceStringData("E430850A-8190-4807-9D7F-29546CFE9286", ShortCaption = "Goods Desc.", Caption = "Goods Description", FullDescription = "[18 05 001 000] Description of Goods")]
		[ResourceStringData("8D0F16F9-373B-4DDD-B07A-F9BB63F191A5", ShortCaption = "Goods Desc.", MediumCaption = "[6/8] Goods Desc.", Caption = "[6/8] Goods Description", FullDescription = "[6/8] Description of Goods", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString JI_Description
		{
			get => base.JI_Description;
			set => base.JI_Description = value;
		}

		[ResourceStringData("623443A7-D5A1-4409-8EEE-EE36ED04EBBF", Caption = "Tariff", FullDescription = "[18 09 056 000] Harmonized system sub-heading code and [18 09 057 000] Combined nomenclature code")]
		[ResourceStringData("DAF527AE-725F-4C24-B009-CB34A7762EF2", ShortCaption = "[6/14] Commodity", MediumCaption = "[6/14 & 6/15] Commodity & TARIC", Caption = "[6/14 & 6/15] Commodity Code – Combined Nom. & TARIC", FullDescription = "[6/14 & 6/15] Commodity Code – Combined Nomenclature Code & TARIC Code", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString JI_Tariff
		{
			get => base.JI_Tariff;
			set
			{
				var oldValue = JI_Tariff;
				base.JI_Tariff = value;
				if (!IsCopying && oldValue != JI_Tariff)
				{
					RefreshCusLineTariffDetails(UniversalTariff);
					CusLineTariffDetails.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("2C506D3D-E0A4-4398-97DF-7FD1FCBEA076", ShortCaption = "[6/16] Add. Code 1", MediumCaption = "[6/16] First Add. Code", Caption = "[6/16] Commodity Code – First TARIC additional code", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString JI_SupplementaryCode1
		{
			get => base.JI_SupplementaryCode1;
			set => base.JI_SupplementaryCode1 = value;
		}

		[ResourceStringData("6D69CEA0-8AAC-4FFA-83A8-315821C7E98A", ShortCaption = "[6/16] Add. Code 2", MediumCaption = "[6/16] Second Add. Code", Caption = "[6/16] Commodity Code – Second TARIC additional code", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString JI_SupplementaryCode2
		{
			get => base.JI_SupplementaryCode2;
			set => base.JI_SupplementaryCode2 = value;
		}

		[ResourceStringData("DA93C3B2-7664-44A1-89A1-F22BABEB0538", ShortCaption = "[6/16] Fur. Codes", MediumCaption = "[6/16] Further Add. Codes", Caption = "[6/16] Commodity Code – Further TARIC additional codes", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString JI_AdditionalSupplements => AdditionalSupplementaryCodes.AsString;

		[ResourceStringData("585C8A10-C425-485C-B8F9-8A9B41D6FC51", ShortCaption = "[6/17] VAT", MediumCaption = "[6/17] VAT", Caption = "[6/17] Commodity Code – National additional code(s)", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString JI_ZZF_NKTaxType
		{
			get => base.JI_ZZF_NKTaxType;
			set => base.JI_ZZF_NKTaxType = value;
		}

		void RefreshCusLineTariffDetails(TariffView tariffView)
		{
			CusLineTariffDetails.RemoveAndDeleteAll();
			if (tariffView != null)
			{
				var exciseTaxs = tariffView.ChildTariffs.Find(GetExciseTaxRelationQuery());
				foreach (var exciseTax in exciseTaxs)
				{
					var cusLineTariffDetail = CusLineTariffDetails.AddNew();
					cusLineTariffDetail.BZ_Type = exciseTax.RelatedTariffFromTypeCode;
					cusLineTariffDetail.BZ_Tariff = exciseTax.RelatedTariffFromCode;
				}
			}
		}

		ZQuery GetExciseTaxRelationQuery()
		{
			var query = new ZDBOnlyQuery(typeof(TariffRelationshipView));
			query.AddToFilter(TariffRelationshipViewSchema.ZZH_ZZZ_RelatedTariffDataGrouping, DefaultDataGroupingForTariffsCore);
			var tariffTypeSubQuery = new ZDBOnlySubQuery(typeof(RefCusTariffType), RefCusTariffTypeSchema.PK);
			tariffTypeSubQuery.AddToFilter(RefCusTariffTypeSchema.ZZI_TariffType, new[] { TaxOrFeeType.ExciseTaxes.MineralOilTax, TaxOrFeeType.ExciseTaxes.TobaccoProductsTax, TaxOrFeeType.ExciseTaxes.AlcoholProductsTax, TaxOrFeeType.ExciseTaxes.MineralOilTaxCarbon, });
			query.AddSubQuery(TariffRelationshipViewSchema.ZZH_ZZI_RelatedTariffType, tariffTypeSubQuery, JoinCondition.And);

			return query;
		}

		[ResourceStringData("41C67E90-B5DC-425B-9D80-EC2F31FF95A8", Caption = "Tariff")]
		[ResourceStringData("DAF527AE-725F-4C24-B009-CB34A7762EF2", ShortCaption = "[6/14] Commodity", MediumCaption = "[6/14 & 6/15] Commodity & TARIC", Caption = "[6/14 & 6/15] Commodity Code – Combined Nom. & TARIC", FullDescription = "[6/14 & 6/15] Commodity Code – Combined Nomenclature Code & TARIC Code", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString JI_FormattedTariff
		{
			get => base.JI_FormattedTariff;
			set => base.JI_FormattedTariff = value;
		}

		[ResourceStringData(
			key: "IE.JobComInvoiceLine.JI_Procedure|UCC5",
			ShortCaption = "Proc. Code",
			MediumCaption = "[1/10 & 1/11] Proc. Code",
			Caption = "[1/10 & 1/11] Procedure Code",
			MultipleKey = JobDeclaration.CaptionKeyImportUCC5
		)]
		[ResourceStringData("1A58799D-214C-4576-AE71-2170E1101D93", Caption = "Procedure Code", ShortCaption = "CPC")]
		public override ZString JI_Procedure
		{
			get => base.JI_Procedure;
			set
			{
				var oldValue = JI_Procedure;
				base.JI_Procedure = value;
				if (!IsCopying && oldValue != JI_Procedure)
				{
					InvoiceHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public bool IsProcedureCodeStartsWith76Or77 => JI_Procedure.StartsWith(UniversalReferenceConstants.ProcedureCodes.ProcedureCode._76) || JI_Procedure.StartsWith(UniversalReferenceConstants.ProcedureCodes.ProcedureCode._77);

		[ResourceStringData(
			key: "IE.JobComInvoiceLine.JI_FormattedProcedure|UCC5",
			ShortCaption = "[1/10]Proc. Code",
			MediumCaption = "[1/10 & 1/11] Proc. Code",
			Caption = "[1/10 & 1/11] Procedure Code",
			MultipleKey = JobDeclaration.CaptionKeyImportUCC5
		)]
		[ResourceStringData("9756EA4D-B892-45F8-9410-1F4134767A7D", Caption = "Procedure Code", ShortCaption = "CPC")]
		public override ZString JI_FormattedProcedure
		{
			get => JI_Procedure;
			set => JI_Procedure = value;
		}

		[ResourceStringData("IE.JobComInvoiceLine.JI_ConcessionOrder|UCC5", ShortCaption = "[8/1] Quota", MediumCaption = "[8/1] Quota", Caption = "[8/1] Quota order number", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		[ResourceStringData("BA602FA5-72E8-4ACB-BEAC-CD55F5ED99AE", ShortCaption = "Quota", Caption = "Quota Order Number", FullDescription = "Quota. Box 39. The Quota Order Number of the quota against which a claim for relief from Customs Duty is to be applied.")]
		public override ZString JI_ConcessionOrder
		{
			get => base.JI_ConcessionOrder;
			set => base.JI_ConcessionOrder = value;
		}

		[ResourceStringData(
			key: "IE.JobComInvoiceLine.AdditionalProcedureCodesAsString|UCC5",
			ShortCaption = "[1/11]Add. Proc.",
			MediumCaption = "[1/11] Add. Procedure",
			Caption = "[1/11] Additional Procedure",
			MultipleKey = JobDeclaration.CaptionKeyImportUCC5
		)]
		public override ZString AdditionalProcedureCodesAsString => base.AdditionalProcedureCodesAsString;

		[ResourceStringData("00EB3062-6A8C-4EAC-8316-DD4F046FB893", Caption = "Third Qty", ShortCaption = "3rd Qty")]
		public override ZDecimal JI_CustomsThirdQuantity
		{
			get => base.JI_CustomsThirdQuantity;
			set => base.JI_CustomsThirdQuantity = value;
		}

		[ResourceStringData("611D46CA-FCA8-4A22-9575-6E597AE3854E", Caption = "Region of Destination", ShortCaption = "Dest. Region")]
		public override ZString ZG_RegionOfDestination
		{
			get => base.ZG_RegionOfDestination;
			set => base.ZG_RegionOfDestination = value;
		}

		[ResourceStringData("E1ACDE43-C45D-4A40-AC0D-426534246D7D", Caption = "Consignor")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ConsignorList))]
		public override ZGuid JI_OA_ExporterAddress
		{
			get => base.JI_OA_ExporterAddress;
			set => base.JI_OA_ExporterAddress = value;
		}

		protected override ZAddress GetNewJI_OA_ExporterAddress_ZAddress()
		{
			var result = base.GetNewJI_OA_ExporterAddress_ZAddress();
			result.GetDefaultAddress = new ZAddress.DefaultAddressHandler(GetDefaultAddress);
			return result;
		}

		protected override ZAddress GetNewJI_OA_ConsigneeAddress_ZAddress()
		{
			var result = base.GetNewJI_OA_ConsigneeAddress_ZAddress();
			result.GetDefaultAddress = new ZAddress.DefaultAddressHandler(GetDefaultAddress);
			return result;
		}

		public override ZString JI_RN_NKCountryOfExport
		{
			get => GetDeclarationEffectiveValueToReturnIfNeeded(base.JI_RN_NKCountryOfExport, Schema.JI_RN_NKCountryOfExport, JobDeclaration.Schema.CountryOfExport);
			set => base.JI_RN_NKCountryOfExport = GetEffectiveValueToSetCompareToDeclaration(value, JobDeclaration.Schema.CountryOfExport);
		}

		ZString DeclarationType => EntryInstruction?.CEI_Style ?? ZString.Empty;

		public ZString RequestedPreviousProcedure => JI_Procedure.Left(4);

		public bool IsCustomsWarehousingProcedure76Or77 => JI_Calc_RequestedProcedure == ProcedureCodes.ProcedureCode._76 || JI_Calc_RequestedProcedure == ProcedureCodes.ProcedureCode._77;

		public bool IsProcedureCode44 => JI_Calc_RequestedProcedure == ProcedureCodes.ProcedureCode._44;

		public bool IsProcedureCode53 => JI_Calc_RequestedProcedure == ProcedureCodes.ProcedureCode._53;

		public bool IsInwardProcessingProcedure51 => JI_Calc_RequestedProcedure == ProcedureCodes.ProcedureCode._51;

		CachedProperty<bool> isSecuritiesForEndUse;
		public bool IsSecuritiesForEndUse => Factory.GetValue(ref isSecuritiesForEndUse, () => DeclarationType.ToString() switch
		{
			ImportDeclarationTypeList.Codes.H1 => IsProcedureCode44,
			ImportDeclarationTypeList.Codes.H3 => IsProcedureCode53,
			ImportDeclarationTypeList.Codes.H4 => IsInwardProcessingProcedure51,
			_ => false
		});

		public bool IsPlacesOfUsageAndDetailsOfPlannedActivitiesRequired => Factory.GetValue(
			ref isPlacesOfUsageAndDetailsOfPlannedActivitiesRequired,
			() => IsSecuritiesForEndUse && AdditionalInfos.HasAuthorisationForSpecialProcedure()
		);
		CachedProperty<bool> isPlacesOfUsageAndDetailsOfPlannedActivitiesRequired;

		public override ZInt MaxNumberOfAdditionalProcedureCode => 99;

		public override int MaxSupportingDocuments => IsExport ? 99 : base.MaxSupportingDocuments;

		public override ZString SupportingDocumentsValidationMessage => IsExport
			? Res.GetString("5D0F1460-E665-43D2-BCB0-2C07F55414A1",
				"The maximum number of supporting documents allowed is 99.")
			: base.SupportingDocumentsValidationMessage;

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

		public new JobComInvoiceLineLookups Lookups => (JobComInvoiceLineLookups)base.Lookups;

		public new JobComInvoiceLineValidation Validation => (JobComInvoiceLineValidation)base.Validation;

		public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		public override List<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> EffectiveSupportingDocuments() => IsExport ? SupportingDocuments.Cast<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument>().ToList() : base.EffectiveSupportingDocuments();

		public bool HasMutuallyExclusiveSupportingDocument => Factory.GetValue(ref hasMutuallyExclusiveSupportingDocument, () => SupportingDocumentCollectionExtensions.HasMutuallyExclusiveSupportingDocument(SupportingDocuments));
		CachedProperty<bool> hasMutuallyExclusiveSupportingDocument;

		protected override bool IsLookupsCachedInBase => false;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobComInvoiceLineFetchStrategy(this);

		protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups() => new JobComInvoiceLineLookups(this);

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
		{
			Customs.Business.JobComInvoiceLineValidation result;
			if (InvoiceHeader is JobComInvoiceHeader invoice)
			{
				if (invoice.PersistentDeclaration is JobDeclaration declaration)
				{
					switch (declaration.JE_MessageType.ToUpperInvariant())
					{
						case IEJobMessageTypeList.Codes.ReExport:
							result = new ReExportJobComInvoiceLineValidation(this);
							break;
						case IEJobMessageTypeList.Codes.ExitSummary:
							result = new ExitSummaryJobComInvoiceLineValidation(this);
							break;
						case IEJobMessageTypeList.Codes.Export:
							result = new ExportJobComInvoiceLineValidation(this);
							break;
						case IEJobMessageTypeList.Codes.Import:
							result = new ImportJobComInvoiceLineValidation(this);
							break;
						default:
							result = new JobComInvoiceLineValidation(this);
							break;
					}
				}
				else if (invoice.JZ_StandAloneInvoiceDirection.EqualsIgnoringCase(IEJobMessageTypeList.Codes.Export))
				{
					result = new ExportJobComInvoiceLineValidation(this);
				}
				else
				{
					result = new JobComInvoiceLineValidation(this);
				}
			}
			else
			{
				result = new JobComInvoiceLineValidation(this);
			}
			return result;
		}

		protected override ICustomsValuationCalculator GetValuationCalculatorCore() => new IeCustomsValuationCalculator(this);

		protected override EU.Business.Declaration.AddInfoJobComInvoiceLine GetNewAddInfo() => new AddInfoJobComInvoiceLine(JI_AddInfoInfo);

		public new AddInfoJobComInvoiceLineLookups AddInfoLookups => (AddInfoJobComInvoiceLineLookups)base.AddInfoLookups;
		public new AddInfoJobComInvoiceLineValidation AddInfoValidation => (AddInfoJobComInvoiceLineValidation)base.AddInfoValidation;

		public new ICusFiscalReferenceCollection<CusFiscalReference> FiscalReferences => (ICusFiscalReferenceCollection<CusFiscalReference>)base.FiscalReferences;

		protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		protected override IEnumerable<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo> AdditionalDocumentsIncludingInherited => base.AdditionalDocumentsIncludingInherited.Union(EntryInstruction?.AdditionalInfos.Cast<AdditionalInfo>() ?? Enumerable.Empty<AdditionalInfo>());

		protected override ICusFiscalReferenceCollection<EU.Business.Declaration.CusFiscalReference> GetNewFiscalReferenceCollection() => new CusFiscalReferenceCollection<CusFiscalReference>(this);

		protected override Type FiscalReferenceType => typeof(CusFiscalReference);

		protected override ResourceStringData SupplementaryCodeCaptionCore => Res.GetData("C42C090D-D340-49E4-9349-03AED5D0BE9F", "TARIC Additional Code");

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			return result;
		}

		protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.Ireland;

		protected override Type TypeOfPartUsedCore => typeof(MasterFiles.OrgSupplierPart);

		internal ISet<ZString> JI_Calc_AdditionalProcedureCodes => Factory.GetValue(ref additionalProcedureCodesCached, () => AdditionalProcedureCodes.Cast<EU.Business.AdditionalProcedureCode>().Select(x => x.CY_Code.Right(3).ToUpperInvariant()).ToHashSet());
		CachedProperty<ISet<ZString>> additionalProcedureCodesCached;

		internal ISet<ZString> AdditionalProcedureCodesIncludingConcession => Factory.GetValue(ref additionalProcedureCodesIncludingConcessionCached,
			() =>
			{
				var result = new HashSet<ZString>(JI_Calc_AdditionalProcedureCodes);
				var concession = JI_Calc_Concession.ToUpperInvariant();
				if (!concession.IsEmpty)
				{
					result.Add(concession);
				}
				return result;
			});
		CachedProperty<ISet<ZString>> additionalProcedureCodesIncludingConcessionCached;

		bool HasMRNPreviousDocuments => Factory.GetValue(ref hasMRNPreviousDocumentsCached, () => PreviousDocuments.HasMRNPreviousDocuments());
		CachedProperty<bool> hasMRNPreviousDocumentsCached;

		public bool HasMRNPreviousDocumentsIncludingEntryInstructionAndInvoiceHeader => Factory.GetValue(ref hasMRNPreviousDocumentsIncludingEntryInstructionAndInvoiceHeaderCached, () =>
		{
			var entryInstructionMRN = EntryInstruction?.PreviousDocuments.HasMRNPreviousDocuments() ?? false;
			var invoiceMRN = InvoiceHeader?.PreviousDocuments.HasMRNPreviousDocuments() ?? false;
			return entryInstructionMRN || invoiceMRN || HasMRNPreviousDocuments;
		});
		CachedProperty<bool> hasMRNPreviousDocumentsIncludingEntryInstructionAndInvoiceHeaderCached;

		public bool IsBR2001PreviousProcedureCodeUsed => Factory.GetValue(ref isBR2001PreviousProcedureCodeUsedCached, () =>
		{
			switch (JI_Calc_PreviousProcedure)
			{
				case PreviousProcedureCodes.Codes._07:
				case PreviousProcedureCodes.Codes._41:
				case PreviousProcedureCodes.Codes._43:
				case PreviousProcedureCodes.Codes._45:
				case PreviousProcedureCodes.Codes._51:
				case PreviousProcedureCodes.Codes._53:
				case PreviousProcedureCodes.Codes._54:
				case PreviousProcedureCodes.Codes._71:
				case PreviousProcedureCodes.Codes._76:
				case PreviousProcedureCodes.Codes._77:
				case PreviousProcedureCodes.Codes._78:
				case PreviousProcedureCodes.Codes._10:
				case PreviousProcedureCodes.Codes._11:
				case PreviousProcedureCodes.Codes._21:
				case PreviousProcedureCodes.Codes._22:
				case PreviousProcedureCodes.Codes._23:
					return true;
				default:
					return false;
			}
		});
		CachedProperty<bool> isBR2001PreviousProcedureCodeUsedCached;

		#region Package links & Pack Details

		internal PackageType OverallPackageType => Factory.GetValue(ref overallPackageTypeCached, () =>
		{
			var overallPackageType = PackageType.None;
			foreach (InvoiceLinePackagePivot packagePivot in PackagesPivot)
			{
				if (packagePivot.Package is Package package)
				{
					var packageType = package.PackageType;
					if (packageType != overallPackageType)
					{
						if (overallPackageType == PackageType.None)
						{
							overallPackageType = packageType;
						}
						else
						{
							overallPackageType = PackageType.Mixed;
							break;
						}
					}
				}
			}
			return overallPackageType;
		});
		CachedProperty<PackageType> overallPackageTypeCached;

		public new ICusLineTariffDetailCollection<CusLineTariffDetail> CusLineTariffDetails => (CusLineTariffDetailCollection)base.CusLineTariffDetails;

		protected override ICusLineTariffDetailCollection<Customs.Business.CusLineTariffDetail> GetCusLineTariffDetails() => new CusLineTariffDetailCollection(this);

		protected override ZBool IsSupportEmptyPackType(BasePackage package) => !(IsExport && package is Package localPackage && localPackage.IsBreakBulk);

		protected override Customs.Business.InvoiceLinePackageValidation GetNewLinkPackValidationCore(BaseCusLinkPackage linkPackage) => IsExport ? new ExportInvoiceLinePackageValidation(linkPackage, this) : new InvoiceLinePackageValidation(linkPackage, this);

		protected override BaseCusLinkPackageCollection PackagesForInvoiceLinesCore() => new InvoiceLineCusLinkPackageCollection(this);

		#endregion

		ZGuid GetDefaultAddress(IOrgHeader orgHeader)
		{
			ZGuid result = ZGuid.Empty;
			if (orgHeader is OrgHeader organisation)
			{
				result = organisation.MainAddress.PK;
			}
			return result;
		}

		#region Overridden EU Properties

		protected override CodeDescriptionPairList AdditionalProcedureCodeListCore()
		{
			if (IsImport)
			{
				var cpcList = Lookups.CPCList;
				var cei_StyleInUpperCase = EntryInstruction?.CEI_Style.ToUpper() ?? ZString.Empty;
				var first4CharactersOfJI_Procedure = RequestedPreviousProcedure;
				return GetCachedAdditionalProcedureCodeList(
					GetKey(Lookups.CPCList.CompleteFilter.GetHashKey().ToString(), first4CharactersOfJI_Procedure, cei_StyleInUpperCase),
					cdpl =>
					{
						foreach (var additionalProcedure in cpcList.Where(p =>
						(first4CharactersOfJI_Procedure.IsEmpty || p.ZZ6_ProcedureCode + p.ZZ6_PreviousProcedureCode == first4CharactersOfJI_Procedure) &&
						(cei_StyleInUpperCase.IsEmpty || p.ZZ6_Group.Contains(cei_StyleInUpperCase.ToUpper()))))
						{
							AddAdditionalProcedureCodeDescription(cdpl, CustomsCountryCodeCore, additionalProcedure);
						}
					});
			}
			else
			{
				return base.AdditionalProcedureCodeListCore();
			}
		}

		public override bool NeedsCustomsQuantity
		{
			get
			{
				var result = base.NeedsCustomsQuantity;

				if (result && Declaration is JobDeclaration declaration)
				{
					result = !(declaration.IsExitSummary || declaration.IsReExport || (declaration.IsUCC5 && EntryInstruction is CusEntryInstruction entryInstruction && (entryInstruction.IsH2 || entryInstruction.IsH3)));
				}

				return result;
			}
		}

		protected override ZString EffectiveCountryOfOriginCore
		{
			get
			{
				var result = base.EffectiveCountryOfOriginCore;
				if (IsImport && !ZG_CountryOfSupply.IsEmpty)
				{
					result = ZG_CountryOfSupply;
				}
				return result;
			}
		}

		[ResourceStringData("{97BB3314-4C4C-4C84-916C-2DDE681601F5}", Caption = "Destination", ShortCaption = "Dest.")]
		[ResourceStringData("0DA0BB42-B31F-41C5-A7BA-8B2EA5EE428D", ShortCaption = "Dest. Country", MediumCaption = "[5/8] Dest. Country", Caption = "[5/8] Destination Country", FullDescription = "[5/8] Country of Destination Code", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString ZG_CountryOfDestination
		{
			get => GetDeclarationEffectiveValueToReturnIfNeeded(base.ZG_CountryOfDestination, Schema.ZG_CountryOfDestination, JobDeclarationSchema.Constants.JE_GoodsDestination);
			set => base.ZG_CountryOfDestination = GetEffectiveValueToSetCompareToDeclaration(value, JobDeclarationSchema.Constants.JE_GoodsDestination);
		}

		[ResourceStringData("1B3E2CE5-F5B0-428A-9CDF-C72A92EAA3C7", ShortCaption = "Dispatch Ctry.", Caption = "Country of Dispatch")]
		[ResourceStringData("AD0C34DD-14D9-45A8-BAC5-51B8EAEB1727", ShortCaption = "[5/14] Disp. Ctry.", MediumCaption = "[5/14] Disp. Country", Caption = "[5/14] Dispatch Country", FullDescription = "[5/14] Country of Dispatch/ Export", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString ZG_CountryOfDispatch
		{
			get => GetDeclarationEffectiveValueToReturnIfNeeded(base.ZG_CountryOfDispatch, Schema.ZG_CountryOfDispatch, JobDeclaration.Schema.CountryOfExport);
			set => base.ZG_CountryOfDispatch = GetEffectiveValueToSetCompareToDeclaration(value, JobDeclaration.Schema.CountryOfExport);
		}

		[ResourceStringData("0A59865C-8456-440D-B3CF-FC5F22826C22", Caption = "Pref. Orig.", ShortCaption = "Origin", FullDescription = "Country of (Preferential) Origin")]
		[ResourceStringData("36C5F221-B53A-46BA-A3FF-0C23DE31248F", ShortCaption = "Pref. Orig.", MediumCaption = "[5/16] Pref. Orig.", Caption = "[5/16] Preferential Orig. Country", FullDescription = "[5/16] Country of preferential origin code", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString ZG_CountryOfSupply { get => base.ZG_CountryOfSupply; set => base.ZG_CountryOfSupply = value; }

		[ResourceStringData("A936CE2C-475C-4860-8D75-4900CD25184A", Caption = "CUS Code", FullDescription = "[18 08 001 000] CUS Code")]
		[ResourceStringData("D3D444C4-F900-40DE-9E0C-276F66D13194", ShortCaption = "[6/13] CUS code", MediumCaption = "[6/13] CUS code", Caption = "[6/13] CUS code", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString ZG_CusNumber { get => base.ZG_CusNumber; set => base.ZG_CusNumber = value; }

		#endregion

		public IEnumerable<ZString> C100SupportingDocReferences => Factory.GetValue(ref c100SupportingDocReferencesCached, () => SupportingDocuments.Cast<SupportingDocument>().Where(x => x.CSI_Code == SupportingDocumentCodes._C100 && !x.CSI_ReferenceNumber.IsEmpty).Select(r => r.CSI_ReferenceNumber));
		CachedProperty<IEnumerable<ZString>> c100SupportingDocReferencesCached;

		public bool IsBR8011PreferenceUsed()
		{
			var preferenceCode = PreferenceCode;
			return preferenceCode == Constants.PreferenceCode.Code1
					|| preferenceCode == Constants.PreferenceCode.Code2
					|| preferenceCode == Constants.PreferenceCode.Code3
					|| preferenceCode == Constants.PreferenceCode.Code4
					|| preferenceCode == Constants.PreferenceCode.Code5;
		}

		public string GetBR8011Key()
		{
			var targetCountry = JI_PrimaryPreference.StartsWith(Constants.PreferenceCode.Code1) ? JI_CountryOfOrigin : ZG_CountryOfSupply;
			return GetKey(targetCountry, JI_Tariff, JI_ConcessionOrder);
		}

		protected override bool SupportsAdditionalTariffs => true;

		protected override IRefCountry CountryOfOriginFallbackCore => JI_CountryOfOrigin.IsEmpty ? null : new DataTransferCountryInfo(JI_CountryOfOrigin, ((ZZRefCusCodeListCombinedCollection)Lookups.CountryOfOrigins).Cast<ZZRefCusCodeListCombined>().FirstOrDefault(x => x.ZZD_Code == JI_CountryOfOrigin)?.ZZD_Description ?? null);
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using static Enterprise.Customs.IT.Business.SADConstants;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusEntryLine : AutoCusEntryLine, Integration.Customs.IT.ICusEntryLine, IPackageProvider, IMergedPreviousDocumentsProvider, INBWrappableBusinessObject, ICanBeImportOrExport
{
	public CusEntryLine(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
	public new class Schema : EU.Business.Declaration.CusEntryLine.Schema
	{
		public const string ZG_AdjustmentAmount = nameof(CusEntryLine.ZG_AdjustmentAmount);
		public const string ZG_LinesValue = nameof(CusEntryLine.ZG_LinesValue);
		public const string LinesValueInInvoiceCurrency = nameof(CusEntryLine.LinesValueInInvoiceCurrency);
		public const string PackageType = nameof(CusEntryLine.PackageType);
		public const string SteelType = nameof(CusEntryLine.SteelType);
		public const string ReleaseCode = nameof(CusEntryLine.ReleaseCode);
		public const string ReleaseDate = nameof(CusEntryLine.ReleaseDate);
		public const string EffectiveGrossWeightKg = nameof(CusEntryLine.EffectiveGrossWeightKg);
		public const string EffectiveNetWeightKg = nameof(CusEntryLine.EffectiveNetWeightKg);
		public const string EffectiveCustomsWeightKg = nameof(CusEntryLine.EffectiveCustomsWeightKg);
		public const string EffectiveSupplementaryQuantity = nameof(CusEntryLine.EffectiveSupplementaryQuantity);
		public const string NumberOfPackages = nameof(CusEntryLine.NumberOfPackages);
		public const string ValuationMethod = nameof(CusEntryLine.ValuationMethod);
		public const string QuotaOrderNumber = nameof(CusEntryLine.QuotaOrderNumber);
		public const int EffectiveGrossWeightKgDecimalPlaces = 6;
		public const int EffectiveNetWeightKgDecimalPlaces = 6;
		public const int EffectiveCustomsWeightKgDecimalPlaces = 6;
		public const int EffectiveSupplementaryQuantityDecimalPlaces = 6;
	}

	public new JobComInvoiceLine RandomLine => (JobComInvoiceLine)base.RandomLine;

	protected override Customs.Business.CusEntryLineValidation GetNewValidation() => new CusEntryLineValidation(this);

	public new JobComInvoiceLine FirstLine => (JobComInvoiceLine)base.FirstLine;

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public new CusEntryHeader Header => (CusEntryHeader)base.Header;

	public new CusEntryLineFeeCollection Fees => (CusEntryLineFeeCollection)base.Fees;

	public override ZString CL_CustomsPostedStatus
	{
		get => base.CL_CustomsPostedStatus;
		set
		{
			var oldValue = CL_CustomsPostedStatus;
			base.CL_CustomsPostedStatus = value;
			if (!IsCopying && oldValue != CL_CustomsPostedStatus && !IsValidationSuspended)
			{
				Header?.MarkAsNeedingValidation();
			}
		}
	}

	[ReadOnly(true)]
	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryLine|ZG_LinesValue", Caption = "Lines Value in EUR")]
	public override ZDecimal ZG_LinesValue
	{
		get => base.ZG_LinesValue;
		set => base.ZG_LinesValue = value;
	}

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryLine|LinesValueInInvoiceCurrency", Caption = "Lines Value")]
	public ZDecimal LinesValueInInvoiceCurrency => TotalLinePrice.Amount;

	[ReadOnly(true)]
	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryLine|CL_AdjustmentAmount", Caption = "Adjustment Amount")]
	public override ZDecimal ZG_AdjustmentAmount
	{
		get => base.ZG_AdjustmentAmount;
		set => base.ZG_AdjustmentAmount = value;
	}

	public override void ResetTotalsAndCachedValues()
	{
		base.ResetTotalsAndCachedValues();
		ZG_LinesValue = ZDecimal.Zero;
		ZG_AdjustmentAmount = ZDecimal.Zero;
	}

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryLine|NumberOfPackages", Caption = "Packages")]
	public ZInt NumberOfPackages => PackagingDetails.Sum(x => x.CHC_NumberOfPacks);

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryLine|PackageType", Caption = "Package Type")]
	public ZString PackageType => PackagingDetails.FirstOrDefault()?.Package?.CW_PackType ?? ZString.Empty;

	public ZString MarksAndNumbers => GetMarksAndNumbers().MarksAndNumbers;

	public ZString LineNotes => ZString.Join(".", GetNoteSegments().Where(x => !x.IsEmpty).ToArray());

	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryLine|SteelType", Caption = "Steel Type")]
	public ZString SteelType => RandomLine.ZG_SteelType;

	public IEnumerable<ZString> AdditionalCodes
	{
		get
		{
			var supplementaryCodes = InvoiceLinesAsEnumerable.SelectMany(x => x.SupplementaryCodes).Cast<SupplementaryCode>().Select(x => x?.CY_Code ?? ZString.Empty);
			return supplementaryCodes.Where(x => !x.IsEmpty).Distinct();
		}
	}

	public ZString NationalProcedureCode
	{
		get
		{
			const int nationalProcedureStart = 4;
			const int nationalProcedureLength = 3;

			return ProcedureCode.SubstringSafe(nationalProcedureStart, nationalProcedureLength).Trim();
		}
	}

	public ZDecimal? SupplementaryUnit
	{
		get
		{
			var supplementaryUnit = InvoiceLinesAsEnumerable.Sum(x => x.JI_CustomsSecondQuantity);
			return supplementaryUnit > 0 ? supplementaryUnit : null;
		}
	}

	[ResourceStringData("71537114-7EDD-477B-996F-86B0C4829816", Caption = "Release Code")]
	public ZString ReleaseCode => EntryNumbersProvider.ReleaseInfo?.CE_EntryNum ?? ZString.Empty;

	[ResourceStringData("0402EA6F-BA03-41BB-BC62-5D99DAEC0CD9", Caption = "Release Date")]
	public ZDate ReleaseDate => EntryNumbersProvider.ReleaseInfo?.CE_IssueDate.Date ?? ZDate.Empty;

	public IEnumerable<MergedPreviousDocument> MergedPreviousDocuments => Declaration.ApportionedDocumentCollection[PK.ToString()];

	public GroupedPreviousDocumentCollection GroupedPreviousDocuments => groupedPreviousDocuments ?? (groupedPreviousDocuments = GroupedPreviousDocumentCollection.LoadNew(this, Factory));
	GroupedPreviousDocumentCollection groupedPreviousDocuments;

	public ICusEntryLineAttachmentPrintingSupporter AttachmentPrintingSupporter
	{
		get
		{
			if (attachmentPrintingSupporter == null)
			{
				attachmentPrintingSupporter = new CachedProperty<ICusEntryLineAttachmentPrintingSupporter>(Factory, () => GetNewAttachmentPrintingSupporter());
			}
			return attachmentPrintingSupporter.Value;
		}
	}
	CachedProperty<ICusEntryLineAttachmentPrintingSupporter> attachmentPrintingSupporter;

	ICusEntryLineAttachmentPrintingSupporter GetNewAttachmentPrintingSupporter()
	{
		if (UseEadAttachmentPrintingSupporter)
		{
			return new CusEntryLineEadAttachmentPrintingSupporter(this);
		}

		return new CusEntryLineSadAttachmentPrintingSupporter(this);
	}

	public ZBool UseEadAttachmentPrintingSupporter
	{
		get => useEadAttachmentPrintingSupporter;
		set
		{
			if (value != useEadAttachmentPrintingSupporter)
			{
				attachmentPrintingSupporter = null;
				useEadAttachmentPrintingSupporter = value;
			}
		}
	}
	ZBool useEadAttachmentPrintingSupporter;

	public IEnumerable<ZString> Remarks => Factory.GetValue(ref remarksCached, () => InvoiceLinesAsEnumerable.Where(x => !x.EffectiveRemarks.IsEmpty).Select(x => x.EffectiveRemarks));
	CachedProperty<IEnumerable<ZString>> remarksCached;

	protected override object GetAdditionalInfoDistinctKey(EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo ai)
	{
		return new
		{
			ai.CSI_Code,
			ai.CSI_ReferenceNumber,
			ai.CSI_Description
		};
	}

	public ZBool PreferenceCodeStartWith2 => PreferenceCode.StartsWith("2");

	public JobComInvoiceLine GetFirstInvoiceLineOrderedByInvoiceNumberAndLineNo() => InvoiceLinesAsEnumerable.OrderBy(x => x.InvoiceHeader.JZ_InvoiceNumber).ThenBy(x => x.JI_LineNo).First();

	public ZBool IsCustomsInEuroValueLessOrEqualThanCustomsValueInEuroTresholdForOriginDeclaration => CustomsValue.Amount <= UniversalReferenceConstants.CustomsValueInEuroTresholdForOriginDeclaration;

	public ZBool IsDefaultLogicForSupportingDocumentC100Applicable
	{
		get
		{
			if (isDefaultLogicForSupportingDocumentC100Applicable == null)
			{
				isDefaultLogicForSupportingDocumentC100Applicable = new CachedProperty<ZBool>(Factory, () =>
				{
					return Declaration != null
					&& Declaration.IsImport
					&& !SupplierRexCode.IsEmpty
					&& PreferenceCodeStartWith2
					&& Header.StatusAllowsSending;
				});
			}
			return isDefaultLogicForSupportingDocumentC100Applicable.Value;
		}
	}
	CachedProperty<ZBool> isDefaultLogicForSupportingDocumentC100Applicable;

	public ZBool HasSupportingDocumentC100WithRexCodeDifferentFromSupplier
	{
		get
		{
			if (hasSupportingDocumentC100WithRexCodeDifferentFromSupplier == null)
			{
				hasSupportingDocumentC100WithRexCodeDifferentFromSupplier = new CachedProperty<ZBool>(Factory, () => !SupplierRexCode.IsEmpty && SupportingDocumentsC100.Any(x => x.CSI_ReferenceNumber != SupplierRexCode));
			}
			return hasSupportingDocumentC100WithRexCodeDifferentFromSupplier.Value;
		}
	}
	CachedProperty<ZBool> hasSupportingDocumentC100WithRexCodeDifferentFromSupplier;

	public void ApplyDefaultLogicForSupportingDocumentsC100IfApplicable()
	{
		if (IsDefaultLogicForSupportingDocumentC100Applicable)
		{
			if (!SupportingDocumentsC100.Any() && InvoiceLines.Any())
			{
				var firstInvoiceLineOrdered = GetFirstInvoiceLineOrderedByInvoiceNumberAndLineNo();
				var rexCode = firstInvoiceLineOrdered.SupportingDocuments.AddNew();
				rexCode.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.C100;
				rexCode.CSI_ReferenceNumber = SupplierRexCode;
			}
			else if (HasSupportingDocumentC100WithRexCodeDifferentFromSupplier && (Declaration?.ShouldOverrideC100SupportingDocumentRexCode ?? false))
			{
				SupportingDocumentsC100.ToList().ForEach(x => x.CSI_ReferenceNumber = SupplierRexCode);
				Declaration.ShouldOverrideC100SupportingDocumentRexCode = false;
			}
		}
	}

	public ZDecimal ExtraEUFreightChargesAmount => InvoiceLinesAsEnumerable.Sum(x => x.JI_Calc_ExtraEUFreightChargesAmount);

	public ZDecimal EUFreightChargesAmount => InvoiceLinesAsEnumerable.Sum(x => x.JI_Calc_EUFreightChargesAmount);

	public ZDecimal DomesticFreightChargesAmount => InvoiceLinesAsEnumerable.Sum(x => x.JI_Calc_DomesticFreightChargesAmount);

	public ZBool RequiresVATExemption
	{
		get
		{
			if (requiresVATExemption == null)
			{
				requiresVATExemption = new CachedProperty<ZBool>(Factory, () =>
				{
					return Declaration != null && SupportingDocuments.Cast<SupportingDocument>().GetDeclarationOfIntentSupportingDocuments().Any();
				});
			}
			return requiresVATExemption.Value;
		}
	}
	CachedProperty<ZBool> requiresVATExemption;

	protected override Type GetCusEntryHeaderType() => typeof(CusEntryHeader);

	protected override EU.Business.Declaration.AddInfoCusEntryLine GetNewAddInfo() => new AddInfoCusEntryLine(CL_AddInfoInfo);

	protected override Customs.Business.ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection() => new CusEntryLineFeeCollection(this, Factory);

	protected override IEnumerable<IDocSADHLineTaxBoxSupporter> GetTaxBoxSupporterListCore() => Fees
		.Cast<CusEntryLineFee>()
		.Where(x => x.CF_RateOverrideReasonCode != RateOverrideReasonList.Codes.Exclude)
		.InCustomsCompliantOrder();

	protected override ZString DescriptionInternal => GoodsDescription.IsEmpty ? GetFirstNotEmptyRelatedDescription() : GoodsDescription;

	protected override bool EffectiveGrossWeightIsApplicableCore => true;

	[DecimalPlaces(Schema.EffectiveGrossWeightKgDecimalPlaces)]
	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryLine|EffectiveGrossWeightKg", Caption = "Gross Weight (KG)")]
	public ZDecimal EffectiveGrossWeightKg => EffectiveGrossWeight.InKilogramsSafe;

	[DecimalPlaces(Schema.EffectiveNetWeightKgDecimalPlaces)]
	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryLine|EffectiveNetWeightKg", Caption = "Net Weight (KG)")]
	public ZDecimal EffectiveNetWeightKg => EffectiveNetWeight.InKilogramsSafe;

	[DecimalPlaces(Schema.EffectiveCustomsWeightKgDecimalPlaces)]
	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryLine|EffectiveCustomsWeightKg", Caption = "Customs Qty (KG)")]
	public ZDecimal EffectiveCustomsWeightKg => EffectiveCustomsWeight.InKilogramsSafe;

	[DecimalPlaces(Schema.EffectiveSupplementaryQuantityDecimalPlaces)]
	[ResourceStringData("Enterprise.Customs.IT.Business.Declaration.CusEntryLine|EffectiveSupplementaryQuantity", Caption = "Suppl. Qty")]
	public ZDecimal EffectiveSupplementaryQuantity => base.SupplementaryQuantity;

	internal void ResetGroupedPreviousDocuments()
	{
		groupedPreviousDocuments = null;
	}

	IEnumerable<JobComInvoiceLine> InvoiceLinesAsEnumerable => InvoiceLines.Cast<JobComInvoiceLine>();

	IEnumerable<ZString> GetNoteSegments()
	{
		yield return GetMarksAndNumbers().MarksAndNumbersOnNotes;
		yield return GetAdditionalInfo();
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant seeNotes")]
	(ZString MarksAndNumbers, ZString MarksAndNumbersOnNotes) GetMarksAndNumbers()
	{
		const string seeNotes = "... see notes";
		const string separator = ";";

		var marksAndNumbersList = PackagingDetails.Select(x => x.Package?.CW_MarksAndNos.Trim() ?? ZString.Empty).Where(x => !x.IsEmpty).Distinct().ToArray();
		if (marksAndNumbersList.Sum(x => x.Length) + marksAndNumbersList.Length - 1 <= CustomsFieldMaxLength.EntryLine.MarksAndNumbers)
		{
			return (ZString.Join(separator, marksAndNumbersList), ZString.Empty);
		}
		else
		{
			var marksAndNumbers = ZString.Empty;
			ZInt i = 0;
			foreach (var marksAndNumberItem in marksAndNumbersList)
			{
				if (marksAndNumbers.Length + separator.Length + marksAndNumberItem.Length + seeNotes.Length <= CustomsFieldMaxLength.EntryLine.MarksAndNumbers)
				{
					marksAndNumbers += (i == 0 ? ZString.Empty : (ZString)separator) + marksAndNumberItem;
				}
				else
				{
					marksAndNumbers += seeNotes;
					break;
				}
				i++;
			}
			var marksAndNumbersOnNotes = FormattableString.Invariant($"Other Marks: {ZString.Join(separator, marksAndNumbersList.Skip(i).ToArray())}");
			return (marksAndNumbers, marksAndNumbersOnNotes);
		}
	}

	ZString GetFirstNotEmptyRelatedDescription()
	{
		return InvoiceLines.Where(x => !x.JI_Description.IsEmpty).OrderBy(x => x.JI_Description).FirstOrDefault()?.JI_Description ?? ZString.Empty;
	}

	ZString GetAdditionalInfo()
	{
		const string pipeSeparator = "|";
		var additionalInfoSegments = InvoiceLinesAsEnumerable.OrderBy(x => x.JI_LineNo)
			.Select(x => x.Remarks)
			.Where(x => !x.IsEmpty)
			.Distinct();
		return ZString.Join(pipeSeparator, additionalInfoSegments.ToArray());
	}

	CusEntryLineEntryNumbersProvider EntryNumbersProvider => entryNumbersProvider ?? (entryNumbersProvider = GetNewCusEntryLineEntryNumbersProvider());
	CusEntryLineEntryNumbersProvider entryNumbersProvider;

	CusEntryLineEntryNumbersProvider GetNewCusEntryLineEntryNumbersProvider() => new CusEntryLineEntryNumbersProvider(this);

	ZString SupplierRexCode => Declaration?.SupplierRexCode ?? ZString.Empty;

	IEnumerable<SupportingDocument> SupportingDocumentsC100 => InvoiceLinesAsEnumerable.SelectMany(x => x.ActualSupportingDocumentCodeCollection).Distinct().Where(x => x.CSI_Code == UniversalReferenceConstants.SupportingDocumentTypes.C100);

	#region IPackageProvider Members

	ZInt IPackageProvider.NumberOfPackages => NumberOfPackages;

	ZString IPackageProvider.PackageType => PackageType;

	ZString IPackageProvider.MarksAndNumbers => MarksAndNumbers;

	#endregion

	#region IMergedPreviousDocumentsProvider Members

	ZString IMergedPreviousDocumentsProvider.NBStatus => ZG_NBStatus;

	IEnumerable<IMergedPreviousDocument> IMergedPreviousDocumentsProvider.MergedPreviousDocuments => MergedPreviousDocuments.Cast<IMergedPreviousDocument>();

	ZInt IMergedPreviousDocumentsProvider.LineNumber => CL_LineNumber;

	ZBool IMergedPreviousDocumentsProvider.IsExport => Declaration.IsExport;

	#endregion

	#region ICanBeImportOrExport

	ZBool ICanBeImportOrExport.IsExport => Declaration?.IsExport ?? ZBool.False;

	#endregion

	#region INBWrappableBusinessObject Members

	ZInt INBWrappableBusinessObject.LineNumber => CL_LineNumber;

	ZBool INBWrappableBusinessObject.IsImport => Declaration.IsImport;

	ZBool INBWrappableBusinessObject.IsExport => Declaration.IsExport;

	RegCusEntryNumberWrapper INBWrappableBusinessObject.EntryNumberWrapper => Header.EntryNumbersProvider?.RegistrationInfoWrapper;

	IEnumerable<GroupedPreviousDocument> INBWrappableBusinessObject.NBGroupedPreviousDocuments => new TypedEnumerable<GroupedPreviousDocument>(GroupedPreviousDocuments);

	#endregion

	public MergedPreviousDocument GetUnloadingDataPreviousProcedureDocument()
	{
		if (!MergedPreviousDocuments.Skip(1).Any())
		{
			return MergedPreviousDocuments.SingleOrDefault(x => x.IsPreviousProcedureDocument);
		}
		else if (!MergedPreviousDocuments.Skip(2).Any() && MergedPreviousDocuments.Any(x => x.IsSummaryDeclarationDocument))
		{
			return MergedPreviousDocuments.SingleOrDefault(x => x.IsPreviousProcedureDocument);
		}

		return null;
	}

	public ZString NatureOfTransaction => RandomLine?.InvoiceHeader?.JZ_ValuationCode ?? ZString.Empty;
}

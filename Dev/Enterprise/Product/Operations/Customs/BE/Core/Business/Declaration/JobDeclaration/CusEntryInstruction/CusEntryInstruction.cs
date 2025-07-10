using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Declaration;

public class CusEntryInstruction : AutoCusEntryInstruction, Integration.Customs.BE.ICusEntryInstruction
{
	public CusEntryInstruction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.CusEntryInstruction.Schema
	{
		public const string ZG_ManualDeclaration = nameof(CusEntryInstruction.ZG_ManualDeclaration);
	}

	public bool ZG_ManualDeclaration_ReadOnly => EntryHeader?.IsWaitingForResponseOrHasBeenLodgedAtCustoms ?? false;

	public new CusEntryHeader EntryHeader => EntryHeaderCore;

	protected virtual CusEntryHeader EntryHeaderCore => base.EntryHeader as CusEntryHeader;

	public ZBool IsImport => Factory.GetValue(ref isImport, () => JobDeclaration?.IsImport ?? false);
	CachedProperty<ZBool> isImport;

	public ZBool IsExport => Factory.GetValue(ref isExport, () => JobDeclaration?.IsExport ?? false);
	CachedProperty<ZBool> isExport;

	public ZBool IsReExport => Factory.GetValue(ref isReExport, () => ((JobDeclaration)JobDeclaration)?.IsReExport ?? false);
	CachedProperty<ZBool> isReExport;

	public ZBool IsExitSummary => Factory.GetValue(ref isExitSummary, () => ((JobDeclaration)JobDeclaration)?.IsExitSummary ?? false);
	CachedProperty<ZBool> isExitSummary;

	protected override Customs.Business.CusEntryInstructionLookups GetNewLookups()
	{
		Customs.Business.CusEntryInstructionLookups result;
		if (IsImport)
		{
			result = new ImportCusEntryInstructionLookups(this);
		}
		else if (IsExport)
		{
			result = new ExportCusEntryInstructionLookups(this);
		}
		else
		{
			result = new CusEntryInstructionLookups(this);
		}

		return result;
	}

	protected override Customs.Business.CusEntryInstructionValidation GetNewValidation()
	{
		CusEntryInstructionValidation result;
		if (IsImport)
		{
			result = new ImportCusEntryInstructionValidation(this);
		}
		else if (IsExport)
		{
			result = new ExportCusEntryInstructionValidation(this);
		}
		else
		{
			result = new CusEntryInstructionValidation(this);
		}

		return result;
	}

	#region Guarantees

	[MaxLength(5)]
	[List(nameof(Guarantee) + "." + nameof(GuaranteeForEntryInstruction.Lookups) + "." + nameof(GuaranteeForEntryInstructionLookups.BondTypeList))]
	public ZString GuaranteeType
	{
		get => Guarantee.PW_BondType;
		set
		{
			Guarantee.PW_BondType = value;
			GuaranteeTypeInfo.RefreshBinding();
		}
	}
	public ZPropertyInfo GuaranteeTypeInfo => GetWrappedZPropertyInfo(nameof(GuaranteeType), x => Guarantee.PW_BondTypeInfo);

	[MaxLength(35)]
	public ZString GuaranteeReferenceNumber
	{
		get => Guarantee.PW_BondNumber;
		set
		{
			Guarantee.PW_BondNumber = value;
			GuaranteeReferenceNumberInfo.RefreshBinding();
		}
	}
	public ZPropertyInfo GuaranteeReferenceNumberInfo => GetWrappedZPropertyInfo(nameof(GuaranteeReferenceNumber), x => Guarantee.PW_BondNumberInfo);

	[MaxLength(35)]
	[List(nameof(Guarantee) + "." + nameof(GuaranteeForEntryInstruction.Lookups) + "." + nameof(GuaranteeForEntryInstructionLookups.FacilityCollection))]
	public ZString GuaranteeOffice
	{
		get => Guarantee.PW_HolderIdentification;
		set
		{
			Guarantee.PW_HolderIdentification = value;
			GuaranteeOfficeInfo.RefreshBinding();
		}
	}
	public ZPropertyInfo GuaranteeOfficeInfo => GetWrappedZPropertyInfo(nameof(GuaranteeOffice), x => Guarantee.PW_HolderIdentificationInfo);

	public ZDateTime GuaranteeDate
	{
		get => Guarantee.PW_BondEffectiveDate;
		set
		{
			Guarantee.PW_BondEffectiveDate = value;
			GuaranteeDateInfo.RefreshBinding();
		}
	}
	public ZPropertyInfo GuaranteeDateInfo => GetWrappedZPropertyInfo(nameof(GuaranteeDate), x => Guarantee.PW_BondEffectiveDateInfo);

	public GuaranteeForEntryInstruction Guarantee
	{
		get
		{
			if (Guarantees.Count == 0)
			{
				Guarantees.AddNew();
			}
			return Guarantees[0];
		}
	}

	public new GuaranteeForEntryInstructionCollection Guarantees => (GuaranteeForEntryInstructionCollection)base.Guarantees;

	protected override EU.Business.Declaration.GuaranteeForEntryInstructionCollection GetNewGuaranteeForEntryInstructionCollection()
	{
		return new GuaranteeForEntryInstructionCollection(this);
	}

	#endregion

	[MaxLength(3)]
	public override ZString CEI_Style
	{
		get => base.CEI_Style;
		set
		{
			var hasChanged = CEI_Style != value;
			base.CEI_Style = value;

			if (hasChanged && !IsCopying)
			{
				var style = CEI_Style;
				DefaultFiscalReferenceForEntryInstructionStyle(this, style, FiscalReferenceCodeList.Codes.FR3_TaxRepresentative);
				InvoiceLines.Cast<JobComInvoiceLine>().ToList().ForEach(i => DefaultFiscalReferenceForEntryInstructionStyle(i, style, FiscalReferenceCodeList.Codes.FR2_Customer));
			}
		}
	}

	[ResourceStringData("bf48208d-ff09-4b21-9e7b-d13f7a11decp", Caption = "[UCC 1/2] Sub Style")]
	[ResourceStringData("3F6FBF18-130D-409C-BAE1-66EF5AAEF9F9", Caption = "[UCC 1/2] Sub Style", FullDescription = "[11 02 001 000] Additional Declaration Type", MultipleKey = BE.Business.Declaration.JobDeclaration.CaptionKeyImportUCC6)]
	[MaxLength(3)]
	public override ZString CEI_SubStyle { get => base.CEI_SubStyle; set => base.CEI_SubStyle = value; }

	[ResourceStringData("FEFFFC45-96A1-465F-8C93-FDBA80B5CE5B", Caption = "Location of Goods", FullDescription = "[UCC 5/23] Location of Goods")]
	public override ZString GoodsLocationDescription { get => base.GoodsLocationDescription; }

	public override ZBool ZG_ManualDeclaration
	{
		get => base.ZG_ManualDeclaration;
		set
		{
			base.ZG_ManualDeclaration = value;
			EntryHeader?.SetAllEntryLinesReadOnly();
		}
	}

	[ResourceStringData("941BF923-026C-48B0-BF8D-87CA610019A1", Caption = "To Warehouse")]
	public override ZGuid CEI_OA_Warehouse2 { get => base.CEI_OA_Warehouse2; set => base.CEI_OA_Warehouse2 = value; }

	protected override EU.Business.CusGoodsLocation GetGoodsLocation() => (CusGoodsLocation)base.GetGoodsLocation();

	public void DefaultFiscalReferenceForEntryInstructionStyle(BusinessObject businessObject, string style, string type)
	{
		if (businessObject != null)
		{
			if (style == ImportCusEntryInstructionsDeclarationTypeList.Codes.FreeCirculation ||
				style == ImportCusEntryInstructionsDeclarationTypeList.Codes.ImportSimplified ||
				style == ImportCusEntryInstructionsDeclarationTypeList.Codes.ProbablyNoControlRequired)
			{
				if (businessObject is JobComInvoiceLine invoiceLine && ((invoiceLine.JI_Procedure.IsEmpty ? ZString.Empty : invoiceLine.JI_Procedure).StartsWith(CustomsProcedureCodeList.ProcedureCode._42) || (invoiceLine.JI_Procedure.IsEmpty ? ZString.Empty : invoiceLine.JI_Procedure).StartsWith(CustomsProcedureCodeList.ProcedureCode._63)))
				{
					FiscalRepresentativeDefaulter.DefaultFiscalReferences(invoiceLine, type);
				}
				else if (businessObject is CusEntryInstruction entryInstruction)
				{
					FiscalRepresentativeDefaulter.DefaultFiscalReferences(entryInstruction);
				}
				else
				{
					FiscalRepresentativeDefaulter.RemoveFiscalReference(businessObject, type);
				}
			}
			else
			{
				FiscalRepresentativeDefaulter.RemoveFiscalReference(businessObject, type);
			}
		}
	}

	public bool IsImportDeclarationType()
	{
		var style = CEI_Style;
		return style == ImportCusEntryInstructionsDeclarationTypeList.Codes.FreeCirculation
				|| style == ImportCusEntryInstructionsDeclarationTypeList.Codes.CustomsWarehousing
				|| style == ImportCusEntryInstructionsDeclarationTypeList.Codes.TemporaryAdmission
				|| style == ImportCusEntryInstructionsDeclarationTypeList.Codes.InwardProcessing
				|| style == ImportCusEntryInstructionsDeclarationTypeList.Codes.IntroductionOfGoods
				|| style == ImportCusEntryInstructionsDeclarationTypeList.Codes.ImportSimplified;
	}

	public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

	protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

	public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
	{
		var result = base.GetCusSupportingInfoTypesCore();
		result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
		result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		return result;
	}

	public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);
}

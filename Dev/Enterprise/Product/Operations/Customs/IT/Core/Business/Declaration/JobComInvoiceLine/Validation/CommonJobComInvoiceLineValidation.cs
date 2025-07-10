using System;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CommonJobComInvoiceLineValidation : EU.Business.Declaration.JobComInvoiceLineValidation
{
	public CommonJobComInvoiceLineValidation(JobComInvoiceLine parent) : base(parent)
	{
	}

	public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;
	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected CusEntryLine EntryLine => Parent.CusEntryLine;
	protected ZBool IsMergeDone => (Declaration?.IsMergeDone ?? ZBool.False) && EntryLine != null;
	protected ZBool IsFirstInvoiceLineOfRelatedEntryLine => (EntryLine?.GetFirstInvoiceLineOrderedByInvoiceNumberAndLineNo()?.PK ?? ZGuid.Empty) == Parent.PK;

	protected override bool HasValidPackagePivots => Parent.PackagesPivot.Cast<InvoiceLinePackagePivot>().Any(p => p.CHC_JI.IsValid);

	public override void ValidateAll()
	{
		base.ValidateAll();

		ValidateRemarks();
		Parent.SupportingDocumentsManager.ValidateCustomsDecisionsSupportingDocument();
		ValidatePreviousDocumentsCount();
	}

	protected override void CheckJI_Procedure()
	{
		base.CheckJI_Procedure();

		var parent = Parent;
		CheckJI_ProcedureStartingWithRelatedEntryInstructionProcedure(parent);
		CheckJI_ProcedureIfTemporaryProcedureLimitDateIsRequired(parent);
		CheckJI_ProcedureIfMandatorySupportingDocumentsExists(parent);
	}

	protected override void CheckJI_CEI()
	{
		base.CheckJI_CEI();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CEIInfo);
	}

	protected override void CheckJI_CustomsSecondUnitQty()
	{
		var unitOfMeasure = Parent.JI_CustomsSecondUnitQty;
		var customsSecondQuantityUnitQtyInfo = Parent.JI_CustomsSecondUnitQtyInfo;
		var unitsOfMeasureForTariff = Parent.Lookups.SupplementaryQuantityUOMs;
		var unitsOfMeasureForTariffCount = unitsOfMeasureForTariff.Count();
		if (unitsOfMeasureForTariffCount > 0)
		{
			if (unitOfMeasure.IsEmpty)
			{
				customsSecondQuantityUnitQtyInfo.AddMessageError(ValidationCaptions.InvoiceLine.TheSelectedTariffRequiresSupplementaryQuantityUOM);
			}
			else if (!unitsOfMeasureForTariff.Any(x => x == unitOfMeasure))
			{
				customsSecondQuantityUnitQtyInfo.AddMessageError(ValidationCaptions.InvoiceLine.TheSelectedSupplementaryQuantityUOMIsNotInTheList(unitsOfMeasureForTariff.OrderBy(x => x)));
			}
		}
		else if (Parent.UniversalTariff != null && !unitOfMeasure.IsEmpty)
		{
			customsSecondQuantityUnitQtyInfo.AddMessageError(ValidationCaptions.InvoiceLine.TheSelectedTariffHasNoSupplementaryQuantityUOM);
		}

		if (!Parent.JI_CustomsSecondQuantity.IsEmpty && unitOfMeasure.IsEmpty)
		{
			customsSecondQuantityUnitQtyInfo.AddError(ValidationCaptions.InvoiceLine.YouHaveEnteredSupplementaryQuantityWithoutUOM);
		}
	}

	protected override void CheckJI_CustomsThirdUnitQty()
	{
		base.CheckJI_CustomsThirdUnitQty();

		var customsThirdUnitQty = Parent.JI_CustomsThirdUnitQty;
		var customsThirdUnitQtyInfo = Parent.JI_CustomsThirdUnitQtyInfo;
		var suggestedCustomsThirdUnit = Parent.SuggestedCustomsThirdQuantityUOM;

		if (!Parent.JI_CustomsThirdQuantity.IsEmpty)
		{
			if (customsThirdUnitQty.IsEmpty)
			{
				customsThirdUnitQtyInfo.AddMessageError(ValidationCaptions.InvoiceLine.ThirdQuantityUOMIsRequire);
			}
			else if (suggestedCustomsThirdUnit != customsThirdUnitQty)
			{
				customsThirdUnitQtyInfo.AddMessageError(ValidationCaptions.InvoiceLine.TheSelectedThirdQuantityUOMIsNotInTheList(suggestedCustomsThirdUnit));
			}
		}
	}

	protected override void CheckJI_CustomsThirdQuantity()
	{
		base.CheckJI_CustomsThirdQuantity();

		var customsThirdQuantity = Parent.JI_CustomsThirdQuantity;
		var tariffRequiresCustomsThirdQuantity = !Parent.SuggestedCustomsThirdQuantityUOM.IsEmpty;
		var customsThirdQuantityInfo = Parent.JI_CustomsThirdQuantityInfo;

		if (tariffRequiresCustomsThirdQuantity && customsThirdQuantity.IsEmpty)
		{
			customsThirdQuantityInfo.AddMessageError(ValidationCaptions.InvoiceLine.TheSelectedTariffRequiresThirdQuantity);
		}
		else if (!tariffRequiresCustomsThirdQuantity && !customsThirdQuantity.IsEmpty)
		{
			customsThirdQuantityInfo.AddMessageError(ValidationCaptions.InvoiceLine.TheSelectedTariffHasNoThirdQuantity);
		}
	}

	protected virtual void CheckCustomsQuantityAndNetWeightEquality()
	{
		if (Parent.CustomsFirstQuantityInKG != Parent.NetWeightInKG)
		{
			Parent.JI_CustomsQuantityInfo.AddWarning(ValidationCaptions.InvoiceLine.CustomsQtyIsUsuallyEqualNetWeight);
		}
	}

	protected override void CheckJI_Weight()
	{
		base.CheckJI_Weight();
		CheckJI_WeightMandatoryValidation();
	}

	protected virtual void CheckJI_WeightMandatoryValidation()
	{
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_WeightInfo);
	}

	protected override void CheckJI_CustomsQuantity()
	{
		base.CheckJI_CustomsQuantity();
		var customsQuantityInfo = Parent.JI_CustomsQuantityInfo;
		MandatoryValidation.MessageErrorIfNotEntered(customsQuantityInfo);

		CheckCustomsQuantityAndNetWeightEquality();
	}

	protected override void CheckJI_Description()
	{
		base.CheckJI_Description();
		var maxLength = GetDescriptionMaxLengthBasedOnDeclarationType();
		if (maxLength.HasValue && Parent.JI_Description.Length > maxLength.Value)
		{
			Parent.JI_DescriptionInfo.AddWarning(ValidationCaptions.Shared.GetFieldExceedsCustomsMaxLengthExceesWillBeTruncateCaption(Parent.JI_DescriptionInfo, maxLength.Value));
		}
	}

	protected virtual ZInt? GetDescriptionMaxLengthBasedOnDeclarationType() => null;

	public void ValidateRemarks()
	{
		ValidateCalculatedProperty(Parent.RemarksInfo);
	}

	protected void CheckRemarks()
	{
		var entryLine = Parent.CusEntryLine;
		if (entryLine != null && entryLine.LineNotes.Length > SADConstants.CustomsFieldMaxLength.EntryLine.Notes)
		{
			Parent.RemarksInfo.AddWarning(ValidationCaptions.InvoiceLine.TotalLengthOfRemarksExceedsTheMaximumAllowedLengthInTheMessage(SADConstants.CustomsFieldMaxLength.EntryLine.Notes));
		}
	}

	protected override void CheckJI_WeightUQ()
	{
		base.CheckJI_WeightUQ();
		ListValidation.MessageErrorIfInvalidCode(Parent.JI_WeightUQInfo);
	}

	protected override void CheckJI_NetWeightUQ()
	{
		base.CheckJI_NetWeightUQ();
		ListValidation.MessageErrorIfInvalidCode(Parent.JI_NetWeightUQInfo);
	}

	protected override void CheckJI_PartNo()
	{
		base.CheckJI_PartNo();
		CheckPartNoForBondedWarehouse(Parent);
	}

	#region Implementation

	void ValidatePreviousDocumentsCount()
	{
		var parent = Parent;
		var cusEntryLine = parent.CusEntryLine;
		if (!parent.Declaration.IsImport || cusEntryLine is null)
		{
			return;
		}

		var errorMessage = ValidationCaptions.InvoiceLine.TheTotalNumberOfPreviousDocumentsForThisInvoiceLineExceedsTheMaximumAllowed(parent.CusEntryLine.CL_LineNumber, MaximumDistinctPreviousDocumentsForImport);
		parent.ClearRowNotificationsContaining(errorMessage);

		var distinctPreviousDocuments = cusEntryLine
			.InvoiceLines.Cast<JobComInvoiceLine>()
			.SelectMany(invoiceLine => invoiceLine.PreviousDocuments.Cast<PreviousDocument>())
			.GroupBy(previousDocument => PreviousDocumentHelper.GetImportWrapperGroupKey(previousDocument))
			.Count();

		if (distinctPreviousDocuments > MaximumDistinctPreviousDocumentsForImport)
		{
			parent.AddRowMessageError(errorMessage);
		}
	}

	void CheckJI_ProcedureStartingWithRelatedEntryInstructionProcedure(JobComInvoiceLine parent)
	{
		var procedureCode = parent.EntryInstruction?.CEI_Procedure ?? ZString.Empty;
		var invoiceLineProcedure = parent.JI_Procedure;
		if (!invoiceLineProcedure.IsEmpty && !invoiceLineProcedure.StartsWith(procedureCode, StringComparison.OrdinalIgnoreCase))
		{
			parent.JI_ProcedureInfo.AddMessageError(ValidationCaptions.InvoiceLine.CpcInvalidAgainstRelatedEntryInstructionExpectedStartingWith(procedureCode));
		}
	}

	void CheckJI_ProcedureIfTemporaryProcedureLimitDateIsRequired(JobComInvoiceLine parent)
	{
		var entryInstruction = parent.EntryInstruction;
		if (entryInstruction != null && parent.IsTemporaryProcedure && entryInstruction.ZG_TempProcLimitDate.IsEmpty && entryInstruction.TempProcLimitDateRequired)
		{
			parent.JI_ProcedureInfo.AddMessageError(ValidationCaptions.InvoiceLine.TemporaryProcedureLimitDateIsRequiredWhenCpcStartsWith2Or5);
		}
	}

	void CheckJI_ProcedureIfMandatorySupportingDocumentsExists(JobComInvoiceLine parent)
	{
		var entryLine = parent.CusEntryLine;
		if (entryLine is null)
		{
			return;
		}

		var supportingDocuments = entryLine.SupportingDocuments.Cast<SupportingDocument>();
		foreach (var (procedureCodes, mandatorySupportingDocument) in procedureCodesAndMandatorySupportingDocuments)
		{
			if (parent.JI_FormattedProcedure.In(procedureCodes) && !supportingDocuments.Any(x => x.CSI_Code == mandatorySupportingDocument))
			{
				parent.JI_ProcedureInfo.AddMessageError(ValidationCaptions.InvoiceLine.GetSupportingDocumentMustPresentForProcedureCodeCaption(mandatorySupportingDocument));
			}
		}
	}

	void CheckPartNoForBondedWarehouse(JobComInvoiceLine parent)
	{
		var entryInstruction = parent.EntryInstruction;
		if (entryInstruction is null
			|| !parent.JI_PartNo.IsEmpty)
		{
			return;
		}

		if ((parent.HasOutOfRegimeProcedure && IsBondedWarehouseEnabled(entryInstruction.Warehouse))
			|| (parent.HasIntoRegimeProcedure && IsBondedWarehouseEnabled(entryInstruction.Warehouse2)))
		{
			parent.JI_PartNoInfo.AddMessageError(ValidationCaptions.InvoiceLine.YouHaveNotEnteredProductCodeInventoryCannotBeUpdated);
		}

		bool IsBondedWarehouseEnabled(MasterFiles.Business.OrgAddress warehouse) => warehouse?.GetWhsWarehouse()?.IsWarehouseBondEnabled ?? false;
	}

	readonly ImmutableArray<(ZString[] ProcedureCodes, ZString SupportingDocument)> procedureCodesAndMandatorySupportingDocuments = new (ZString[] ProcedureCodes, ZString SupportingDocument)[]
	{
		(
			new ZString[]
			{
				RefCusProcedureCodes.SpecialUse.NoPreviousProcedure4400,
				RefCusProcedureCodes.SpecialUse.InwardProcedure4451,
				RefCusProcedureCodes.SpecialUse.TemporaryImport4453,
				RefCusProcedureCodes.SpecialUse.Storage4471,
			}
			, SupportingDocumentTypes.N990
		),
		(
			new ZString[]
			{
				RefCusProcedureCodes.ReimportWarehouseDutiesExempt6171F01,
			}
			, SupportingDocumentTypes.ExportDeclarationMRNtoUnload32YY
		),
	}.ToImmutableArray();

	const int MaximumDistinctPreviousDocumentsForImport = 99;

	#endregion
}

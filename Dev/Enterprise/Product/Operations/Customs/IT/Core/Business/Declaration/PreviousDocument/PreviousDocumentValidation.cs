using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.IT.Business.UniversalReference;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Declaration;

public class PreviousDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentValidation
{
	public PreviousDocumentValidation(PreviousDocument parent) : base(parent)
	{
	}

	protected new PreviousDocument Parent => (PreviousDocument)base.Parent;

	public override void ValidateAll()
	{
		Parent.ClearRowNotifications();
		base.ValidateAll();

		ValidatePreviousDocumentsForReimportProcedure();
	}

	protected override void CheckCSI_Procedure()
	{
		base.CheckCSI_Procedure();
		PreviousDocumentValidationHelper.CheckCSI_Procedure(Parent);
		new PreviousDocumentProcedureValidator(Parent).CheckCSI_Procedure();
	}

	protected override void CheckCSI_Code()
	{
		PreviousDocumentValidationHelper.CheckCSI_Code(Parent);
	}

	protected override void CheckCSI_SubType()
	{
		PreviousDocumentValidationHelper.CheckCSI_SubType(Parent);
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		var referenceNumberValidator = GetReferenceNumberValidator();
		referenceNumberValidator.CheckReferenceNumber();
	}

	protected virtual IPreviousDocumentReferenceNumberValidator GetReferenceNumberValidator()
		=> new PreviousDocumentReferenceNumberValidator(Parent, GetSettings(Parent));

	protected PreviousDocumentFieldsInfo GetSettings(PreviousDocument parent)
	{
		var combinationsProvider = new PreviousDocumentCombinationsProvider(parent);
		var settingsProvider = new PreviousDocumentSettingsProvider(combinationsProvider);
		return settingsProvider.GetSettings();
	}

	protected override void CheckCSI_ReferenceNumber2()
	{
		base.CheckCSI_ReferenceNumber2();
		PreviousDocumentValidationHelper.CheckCSI_ReferenceNumber2(Parent);
	}

	protected override void CheckCSI_DateOfIssue()
	{
		base.CheckCSI_DateOfIssue();
		PreviousDocumentValidationHelper.CheckCSI_DateOfIssue(Parent);
	}

	protected override void CheckCSI_LineNo()
	{
		base.CheckCSI_LineNo();

		var lineNoInfo = Parent.CSI_LineNoInfo;
		if (Parent.IsPreviousProcedureDocument)
		{
			if (Parent.CSI_LineNo.IsEmpty && Parent.CSI_Tariff.IsEmpty)
			{
				lineNoInfo.AddWarning(ValidationCaptions.PreviousDocument.ForRpDocumentsLineNoAndTariffAreEmpty);
			}
		}
		else
		{
			CheckLineNoMessageErrorIfIsEntered(lineNoInfo);
		}
	}

	protected virtual void CheckLineNoMessageErrorIfIsEntered(ZPropertyInfo lineNoInfo)
	{
		MandatoryValidation.MessageErrorIfIsEntered(lineNoInfo);
	}

	protected override void CheckCSI_Status()
	{
		base.CheckCSI_Status();
		PreviousDocumentValidationHelper.CheckCSI_Status(Parent);
	}

	protected override void CheckCSI_CustomsOffice()
	{
		base.CheckCSI_CustomsOffice();
		PreviousDocumentValidationHelper.CheckCSI_CustomsOffice(Parent);
	}

	protected override void CheckCSI_UnitOfQuantity()
	{
		base.CheckCSI_UnitOfQuantity();
		var unitOfQuantityInfo = Parent.CSI_UnitOfQuantityInfo;
		if (!unitOfQuantityInfo.ReadOnly)
		{
			MandatoryValidation.MessageErrorIfNotEntered(unitOfQuantityInfo);
			ListValidation.MessageErrorIfInvalidCode(unitOfQuantityInfo);
		}
	}

	protected override void CheckCSI_UnitOfQuantity2()
	{
		base.CheckCSI_UnitOfQuantity2();
		ListValidation.MessageErrorIfInvalidCode(Parent.CSI_UnitOfQuantity2Info);
	}

	protected override void CheckCSI_UnitOfQuantity3()
	{
		base.CheckCSI_UnitOfQuantity3();
		var unitOfQuantityInfo = Parent.CSI_UnitOfQuantity3Info;
		if (!unitOfQuantityInfo.ReadOnly)
		{
			MandatoryValidation.MessageErrorIfNotEntered(unitOfQuantityInfo);
			ListValidation.MessageErrorIfInvalidCode(unitOfQuantityInfo);
		}
	}

	protected override void CheckCSI_Tariff()
	{
		base.CheckCSI_Tariff();
		if (Parent.IsPreviousProcedureDocument && Parent.CSI_Tariff.IsEmpty && Parent.CSI_LineNo.IsEmpty)
		{
			Parent.CSI_TariffInfo.AddWarning(ValidationCaptions.PreviousDocument.ForRpDocumentsLineNoAndTariffAreEmpty);
		}
	}

	protected override void CheckCSI_Quantity()
	{
		base.CheckCSI_Quantity();
		var parent = Parent;
		var propertyInfo = parent.CSI_QuantityInfo;
		if (parent.IsPreviousProcedureDocument)
		{
			MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			CheckPendingMergeOrRunValidationAction(propertyInfo, () =>
			{
				CheckPreviousDocumentQuantityAgainstEntryLineQuantitiesWhereItHasBeenApportioned(
					  entryLine => RelatedProcedureMatchesConditions(entryLine)
					, entryLine => entryLine.EffectiveNetWeight.InKilogramsSafe
					, mergedPreviousDocument => mergedPreviousDocument.IsPreviousProcedureDocument
					, mergedPreviousDocument => mergedPreviousDocument.NetMass
					, propertyInfo
				);
			});
		}

		AddWarningIfQuantityCouldBeRounded(parent.EffectiveNetMass, propertyInfo);

		ValidateQuantityTotalDigits(parent, propertyInfo);

		bool RelatedProcedureMatchesConditions(CusEntryLine entryLine)
		{
			var procedure = entryLine.RandomLine.CusProcedure;

			return procedure != null
			&& !procedure.IsReimportProcedure()
			&& !procedure.HasEmptyPreviousProcedure()
			&& !procedure.IsOutOfInwardProcessing();
		}
	}

	protected override void CheckCSI_PackQty()
	{
		base.CheckCSI_PackQty();
		var targetPropertyInfo = Parent.CSI_PackQtyInfo;
		if (IsPreviousProcedureDocumentAndExistM2LinesWithoutSummaryDeclarationDocument)
		{
			CheckPendingMergeOrRunValidationAction(targetPropertyInfo, () =>
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
			});
		}
		else if (IsSummaryDeclarationDocumentAndExistM2Lines)
		{
			CheckPendingMergeOrRunValidationAction(targetPropertyInfo, () =>
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
				CheckPreviousDocumentQuantityAgainstEntryLineQuantitiesWhereItHasBeenApportioned(
					  entryLine => entryLine.RandomLine.CusProcedure?.HasEmptyPreviousProcedure() ?? ZBool.False
					, entryLine => entryLine.NumberOfPackages
					, mergedPreviousDocument => mergedPreviousDocument.IsSummaryDeclarationDocument
					, mergedPreviousDocument => mergedPreviousDocument.PackageQuantity
					, targetPropertyInfo
				);
			});
		}
	}

	protected override void CheckCSI_Quantity3()
	{
		base.CheckCSI_Quantity3();
		var targetPropertyInfo = Parent.CSI_Quantity3Info;
		if (IsPreviousProcedureDocumentAndExistM2LinesWithoutSummaryDeclarationDocument)
		{
			CheckPendingMergeOrRunValidationAction(targetPropertyInfo, () =>
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
				if (Parent.CSI_Quantity3 < Parent.CSI_Quantity)
				{
					targetPropertyInfo.AddMessageError(ValidationCaptions.PreviousDocument.GrossMassMustBeGreaterThanOrEqualToNetMass);
				}
			});
		}
		else if (IsSummaryDeclarationDocumentAndExistM2Lines)
		{
			CheckPendingMergeOrRunValidationAction(targetPropertyInfo, () =>
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
				CheckPreviousDocumentQuantityAgainstEntryLineQuantitiesWhereItHasBeenApportioned(
					  entryLine => entryLine.RandomLine.CusProcedure?.HasEmptyPreviousProcedure() ?? ZBool.False
					, entryLine => entryLine.EffectiveGrossWeight.InKilogramsSafe
					, mergedPreviousDocument => mergedPreviousDocument.IsSummaryDeclarationDocument
					, mergedPreviousDocument => mergedPreviousDocument.GrossMass
					, targetPropertyInfo
				);
			});
		}

		AddWarningIfQuantityCouldBeRounded(Parent.EffectiveGrossMass, targetPropertyInfo);
	}

	void AddWarningIfQuantityCouldBeRounded(ZWeight weight, ZPropertyInfo propertyInfo)
	{
		const int nonUcc6KGMaxLengthDecimalPartSupported = 5;
		const int ucc6KGMaxLengthDecimalPartSupported = 6;
		const string nonUcc6KGDecimalFormat = "#0.00000";
		const string ucc6KGDecimalFormat = "#0.000000";

		var isUCC6AndIsImport = Parent.Declaration?.IsUCC6AndIsImport ?? false;
		var maxLengthDecimalPartSupportedForKG = isUCC6AndIsImport ? ucc6KGMaxLengthDecimalPartSupported : nonUcc6KGMaxLengthDecimalPartSupported;
		var weightInKilograms = isUCC6AndIsImport ? weight.InUnroundedKilogramsSafe : weight.InKilogramsSafe;
		var actualDecimalPartLength = weightInKilograms.ToString(string.Empty, Culture.Invariant).Split(new char[] { '.' }).Last().Length;
		if (actualDecimalPartLength > maxLengthDecimalPartSupportedForKG)
		{
			var weightCodeDescriptionPairList = Parent.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

			var quanityCouldBeRoundedMessage = isUCC6AndIsImport
				? ValidationCaptions.PreviousDocument.GetQuantityCouldBeRoundedMessageForUCC6Import(weightCodeDescriptionPairList.GetDescriptionFromCode(weight.Unit), weight.Amount, weight.InKilogramsSafe.ToString(ucc6KGDecimalFormat, Culture.Invariant))
				: ValidationCaptions.PreviousDocument.GetQuantityCouldBeRoundedMessage(weightCodeDescriptionPairList.GetDescriptionFromCode(weight.Unit), actualDecimalPartLength - maxLengthDecimalPartSupportedForKG, weight.Amount, weight.InKilogramsSafe.ToString(nonUcc6KGDecimalFormat, Culture.Invariant));

			propertyInfo.AddWarning(quanityCouldBeRoundedMessage);
		}
	}

	void CheckPreviousDocumentQuantityAgainstEntryLineQuantitiesWhereItHasBeenApportioned(Func<CusEntryLine, ZBool> entryLineProcedureCodeCondition, Func<CusEntryLine, decimal> entryLineFieldWhichShouldMatchWithDocumentsFieldSum, Func<MergedPreviousDocument, bool> previousDocumentCategoryCondition, Func<MergedPreviousDocument, decimal> previousDocumentFieldWhichSumShouldMatch, ZPropertyInfo targetValidationPropertyInfo)
	{
		if (EntryLinesWhereDocumentHasBeenApportioned.Any((entryLine) => entryLineProcedureCodeCondition.Invoke(entryLine) && entryLineFieldWhichShouldMatchWithDocumentsFieldSum.Invoke(entryLine) != entryLine.MergedPreviousDocuments.Where(previousDocumentCategoryCondition).Sum(previousDocumentFieldWhichSumShouldMatch)))
		{
			targetValidationPropertyInfo.AddMessageError(ValidationCaptions.PreviousDocument.TheQuantitiesOfDocumentsDoNotMatchWithQuantitiesOfEntryLine);
		}
	}

	void CheckPendingMergeOrRunValidationAction(ZPropertyInfo targetValidationPropertyInfo, Action actionToRunIfNoPendingMerge)
	{
		if (Parent.Declaration?.MergeManager?.RequiresMerge ?? false)
		{
			targetValidationPropertyInfo.AddWarning(ValidationCaptions.PreviousDocument.ApportionmentIsPendingPleaseGenerateEntriesAndSaveOrValidateAll);
		}
		else
		{
			actionToRunIfNoPendingMerge();
		}
	}

	protected override void CheckCSI_Quantity2()
	{
		base.CheckCSI_Quantity2();
		Parent.SupplementaryQuantityHandler.ValidateQuantity2();
	}

	void ValidatePreviousDocumentsForReimportProcedure()
	{
		var importExportParent = Parent.ImportExportParent;

		switch (importExportParent.Level)
		{
			case EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header:
				var declaration = importExportParent as JobDeclaration;
				ValidatePreviousDocumentsIfDeclarationHasReimportProcedure(declaration, docParent => docParent, ValidateJobLevelPreviousDocuments);
				break;

			case EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Both:
				var invoiceHeader = importExportParent as JobComInvoiceHeader;
				ValidatePreviousDocumentsIfDeclarationHasReimportProcedure(invoiceHeader, docParent => docParent?.JobDeclaration, ValidateInvoiceLevelPreviousDocuments);
				break;

			case EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item:
				var invoiceLine = importExportParent as JobComInvoiceLine;
				ValidatePreviousDocumentsIfDeclarationHasReimportProcedure(invoiceLine, docParent => docParent?.Declaration, ValidateInvoiceLineLevelPreviousDocuments);
				break;
		}
	}

	void ValidatePreviousDocumentsIfDeclarationHasReimportProcedure<T>(T docParent, Func<T, JobDeclaration> getDeclaration, Action<T> validatePreviousDocuments) where T : ICanBeImportOrExport
	{
		var declaration = getDeclaration(docParent);
		var declarationHasReimportProcedure = declaration?.CustomsEntryInstructions?.Cast<CusEntryInstruction>()?.Any(x => x.HasReimportProcedure) ?? false;

		if (declarationHasReimportProcedure)
		{
			validatePreviousDocuments(docParent);
		}
	}

	void ValidateInvoiceLineLevelPreviousDocuments(JobComInvoiceLine invoiceLine)
	{
		Argument.NotNull(invoiceLine, "invoiceLine");

		var previousDocuments = invoiceLine.PreviousDocuments.Cast<PreviousDocument>();
		var declarationPreviousDocuments = invoiceLine.Declaration.PreviousDocuments.Cast<PreviousDocument>();
		var invoicePreviousDocuments = invoiceLine.InvoiceHeader.PreviousDocuments.Cast<PreviousDocument>();
		var declarationAndInvoicePreviousDocuments = declarationPreviousDocuments.Union(invoicePreviousDocuments);
		ValidatePreviousDocumentAgainstSameLevelOrParentLevelPreviousDocuments(previousDocuments, ValidationCaptions.PreviousDocument.ForReimportProceduresYouCanHaveJustOneSummaryDeclarationDocumentPerInvoiceLine, declarationAndInvoicePreviousDocuments);
	}

	void ValidateInvoiceLevelPreviousDocuments(JobComInvoiceHeader invoiceHeader)
	{
		Argument.NotNull(invoiceHeader, "invoiceHeader");

		var previousDocuments = invoiceHeader.PreviousDocuments.Cast<PreviousDocument>();
		var declarationPreviousDocuments = invoiceHeader.JobDeclaration.PreviousDocuments.Cast<PreviousDocument>();
		ValidatePreviousDocumentAgainstSameLevelOrParentLevelPreviousDocuments(previousDocuments, ValidationCaptions.PreviousDocument.ForReimportProceduresYouCanHaveJustOneSummaryDeclarationDocumentPerInvoice, declarationPreviousDocuments);
	}

	void ValidateJobLevelPreviousDocuments(JobDeclaration declaration)
	{
		Argument.NotNull(declaration, "declaration");

		var previousDocuments = declaration.PreviousDocuments.Cast<PreviousDocument>();
		ValidatePreviousDocumentAgainstSameLevelOrParentLevelPreviousDocuments(previousDocuments, ValidationCaptions.PreviousDocument.ForReimportProceduresYouCanHaveJustOneSummaryDeclarationDocumentPerJob, Enumerable.Empty<PreviousDocument>());
	}

	void ValidatePreviousDocumentAgainstSameLevelOrParentLevelPreviousDocuments(IEnumerable<PreviousDocument> currentLevelDocuments, ZString currentLevelMessage, IEnumerable<PreviousDocument> parentLevelDocuments)
	{
		var previousDocumentsFromCurrentLevel = currentLevelDocuments.Where(x => x.IsSummaryDeclarationDocument);
		var previousDocumentsFromParentLevel = parentLevelDocuments.Where(x => x.IsSummaryDeclarationDocument);

		if (previousDocumentsFromCurrentLevel.Skip(1).Any())
		{
			Parent.AddRowWarning(currentLevelMessage);
		}
		if (previousDocumentsFromCurrentLevel.Any() && previousDocumentsFromParentLevel.Any())
		{
			Parent.AddRowWarning(ValidationCaptions.PreviousDocument.ForReimportProceduresYouCanHaveJustOneSummaryDeclarationDocumentPerJobAndInvoiceAndLine);
		}
	}

	void ValidateQuantityTotalDigits(PreviousDocument previousDocument, ZPropertyInfo propertyInfo)
	{
		var declaration = previousDocument.Declaration;
		var parentTableCode = previousDocument.CSI_ParentTableCode;
		var isImportAndParentIsEntryInstruction = (declaration?.IsImport ?? false) && parentTableCode == CusEntryInstructionSchema.Constants.Prefix;
		var parentIsJobComInvoiceLineOrHeaderAndDeclarationIsUCC6 = (parentTableCode == JobComInvoiceHeaderSchema.Constants.Prefix || previousDocument.ParentIsJobComInvoiceLine) && (declaration?.IsUCC6 ?? false);

		if ((parentIsJobComInvoiceLineOrHeaderAndDeclarationIsUCC6 || isImportAndParentIsEntryInstruction)
			&& previousDocument.CSI_Quantity.GetNumberOfSignificantDigits() > Ucc6XmlConstants.PreviousDocument.QuantityMaxLength)
		{
			propertyInfo.AddMessageError(ValidationCaptions.PreviousDocument.PreviousDocumentsQuantityAllowedNumberOfDigits(Ucc6XmlConstants.PreviousDocument.QuantityMaxLength));
		}
	}

	IEnumerable<CusEntryLine> EntryLinesWhereDocumentHasBeenApportioned => entryLinesWhereDocumentHasBeenApportioned ?? (entryLinesWhereDocumentHasBeenApportioned = GetEntryLinesWhereDocumentHasBeenApportioned());
	IEnumerable<CusEntryLine> entryLinesWhereDocumentHasBeenApportioned;

	IEnumerable<CusEntryLine> GetEntryLinesWhereDocumentHasBeenApportioned()
	{
		var declarationEntryLines = Parent.Declaration?.CustomsEntryHeaders.Cast<CusEntryHeader>().SelectMany(x => x.MergedLines) ?? Array.Empty<CusEntryLine>();
		return declarationEntryLines.Where(entryLine => entryLine.MergedPreviousDocuments.Any(previousDocument => previousDocument.Key == MergedPreviousDocument.FromPreviousDocument(Parent).Key)).ToArray();
	}

	bool IsPreviousProcedureDocumentAndExistM2LinesWithoutSummaryDeclarationDocument
	{
		get
		{
			if (!isPreviousProcedureDocumentAndExistM2LinesWithoutSummaryDeclarationDocument.HasValue)
			{
				isPreviousProcedureDocumentAndExistM2LinesWithoutSummaryDeclarationDocument = Parent.IsPreviousProcedureDocument && EntryLinesWhereDocumentHasBeenApportioned.Any(x => x.GroupedPreviousDocuments.Any() && !x.GroupedPreviousDocuments.ContainsSummaryDeclarationDocument);
			}
			return isPreviousProcedureDocumentAndExistM2LinesWithoutSummaryDeclarationDocument.Value;
		}
	}

	bool? isPreviousProcedureDocumentAndExistM2LinesWithoutSummaryDeclarationDocument;

	bool IsSummaryDeclarationDocumentAndExistM2Lines
	{
		get
		{
			if (!isSummaryDeclarationDocumentAndExistM2Lines.HasValue)
			{
				isSummaryDeclarationDocumentAndExistM2Lines = Parent.IsSummaryDeclarationDocument && EntryLinesWhereDocumentHasBeenApportioned.Any(x => x.GroupedPreviousDocuments.Any());
			}
			return isSummaryDeclarationDocumentAndExistM2Lines.Value;
		}
	}

	bool? isSummaryDeclarationDocumentAndExistM2Lines;
}

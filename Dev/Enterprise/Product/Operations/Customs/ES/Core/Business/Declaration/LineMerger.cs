using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.ESConstants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.Declaration;

public class LineMerger : EU.Business.Declaration.LineMerger
{
	public LineMerger(JobDeclaration declaration)
		: base(declaration)
	{
	}

	new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override Customs.Business.EntryCreationStrategy[] GetEntryCreationStrategies()
	{
		var declaration = Declaration;
		var result = declaration.CreateEntryCreationStrategy();
		return new Customs.Business.EntryCreationStrategy[] { result };
	}

	protected override IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => new DutyCalculatorStrategy(Declaration);

	protected override void OnMerging()
	{
		if (Declaration.IsImport && Declaration.CustomsEntryInstructions.Any<CusEntryInstruction>(x => x.IsH2))
		{
			AddSupportingDocumentsForH2(SupportingDocumentType.CentralizedClearance);
			AddSupportingDocumentsForH2(SupportingDocumentType.EntryOfDataInDeclarantsRecords);
			AddSupportingDocumentsForH2(SupportingDocumentType.WHLOCAuthorisation);
		}

		base.OnMerging();
		foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
		{
			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				entryLine.ResetReadOnlyPreviousDocuments();
				entryLine.ResetReadOnlyAdditionalInfos();
			}
		}
	}

	protected override void OnMerged()
	{
		base.OnMerged();

		SetUCC6Version();
		SetPOUSVersion();

		if (Declaration.IsImport)
		{
			CalculateAidAmountREA();
			var populateGuaranteesHelper = new PopulateGuaranteesHelper();
			populateGuaranteesHelper.PopulateGuaranteesIfApplicable(Declaration);

			ResetLIQSupportingDocuments(SupportingDocumentType.VATAdditionsCode, GetAmountFor7002Doc);
			ResetLIQSupportingDocuments(SupportingDocumentType.REARebate, GetAmountFor7003Doc);
			ResetLIQ9015SupportingDocuments();
			ResetLIQ5018SupportingDocuments();
			ResetLIQ7015SupportingDocuments();

			ResetH1SSupportingDocuments(SupportingDocumentType.AmountToBeGuaranteedAEAT, Res.GetString("F14F080A-5C1A-4536-BF0F-78D7F03758A8", "Amount to be guaranteed (AEAT)"), GetAmountToBeGuaranteedAEAT);

			ResetH1SSupportingDocuments(SupportingDocumentType.AmountToBeGuaranteedATC, Res.GetString("B8BEC9A7-B44E-4E04-89BF-FE4A4B6C9DC1", "Amount to be guaranteed (ATC)"), GetAmountToBeGuaranteedATC, canaryIslandsOnly: true);
		}

		if (Declaration.IsExport)
		{
			AddT2LSupportingDocuments();
			AddT2LFSupportingDocuments();
		}

		PopulateLocationOfGoodsIfNeeded();

		foreach (CusEntryInstruction entryInstruction in Declaration.CustomsEntryInstructions)
		{
			entryInstruction.Validation.AddNotAllowDeleteEntryLinesErrorForPDIOnMerged();
		}
	}

	protected override void PerformCountrySpecificOperationAfterMergeAfterCalculateDuty()
	{
		base.PerformCountrySpecificOperationAfterMergeAfterCalculateDuty();

		UOMDefaulter.DefaultUOMIfApplicable(Declaration);
	}

	void CalculateAidAmountREA()
	{
		foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
		{
			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				entryLine.InvoiceLines.ForEach(x => CalculateREAForInvoiceLine((JobComInvoiceLine)x));
			}
		}
	}

	void CalculateREAForInvoiceLine(JobComInvoiceLine invoiceLine)
	{
		var reaCharges = invoiceLine.Charges.GetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation);
		if (reaCharges != null)
		{
			reaCharges.ForEach(x => x.Delete());
		}

		var readProductCode = invoiceLine.ZG_REAProductCode;

		if (!readProductCode.IsEmpty && invoiceLine.JI_PrimaryPreference == UniversalReferenceConstants.PrimaryPreferenceCode.REA && invoiceLine.UniversalTariff is TariffView tariff)
		{
			var criteria = new SpecificRateSelectionCriteria(invoiceLine.EffectiveCountryOfOrigin, Core.Constants.CountryCodes.Spain, ZString.Empty, ZString.Empty, new HashSet<ZString>() { readProductCode }, invoiceLine.EffectiveDateForDutyRate, UniversalReferenceConstants.RateTypeList.REA, invoiceLine.REARateCode);
			var refCusRate = tariff.GetApplicableRate(criteria);
			if (refCusRate != null)
			{
				var rateCalc = new REAUniversalRateCalcData(invoiceLine);
				rateCalc.CustomsValueFormula = refCusRate.ZZ2_RateFormula;
				var valueForDuty = rateCalc.ValueForDuty;

				if (valueForDuty > 0)
				{
					var newCharge = invoiceLine.Charges.AddNew(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, new ZDecimal(valueForDuty));
				}
			}
		}
	}

	void ResetLIQSupportingDocuments(string docType, Func<CusEntryLine, decimal> getAmountForDoc)
	{
		DeleteExistingTypeNonEntrySupportingDocuments(docType);

		foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
		{
			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				DeletePreviouslySentNotAcceptedTypeSupportingDocuments(entryLine, docType);

				var amount = getAmountForDoc(entryLine);

				if (amount > 0)
				{
					AddNewLIQSupportingDocumentToEntryLine(entryLine, docType, Utilities.FormatNumberNational(amount, 2), amount: amount);
				}

				entryLine.ReadOnlySupportingDocuments.LoadNew();
			}
		}
	}

	void DeleteExistingTypeNonEntrySupportingDocuments(string docType)
	{
		DeleteExistingJobDeclarationSupportingDocuments(docType);
		DeleteExistingEntryInstructionsSupportingDocuments(docType);
		DeleteExistingInvoiceHeadersSupportingDocuments(docType);
		DeleteExistingInvoiceLinesSupportingDocuments(docType);
	}

	void DeleteExistingJobDeclarationSupportingDocuments(string docType)
	{
		var declarationSupDocsToRemove = Declaration.SupportingDocuments.Find(x => x.CSI_Code == docType).ToList();
		foreach (var docu in declarationSupDocsToRemove)
		{
			Declaration.SupportingDocuments.RemoveAndDelete(docu);
		}
	}

	void DeleteExistingEntryInstructionsSupportingDocuments(string docType)
	{
		foreach (CusEntryInstruction entryInstruction in Declaration.CustomsEntryInstructions)
		{
			var entryInstructionSupDocsToRemove = entryInstruction.SupportingDocuments.Find(x => x.CSI_Code == docType).ToList();
			foreach (var docu in entryInstructionSupDocsToRemove)
			{
				entryInstruction.SupportingDocuments.RemoveAndDelete(docu);
			}
		}
	}

	void DeleteExistingInvoiceHeadersSupportingDocuments(string docType)
	{
		foreach (JobComInvoiceHeader invoice in Declaration.Invoices)
		{
			var invoiceSupDocsToRemove = invoice.SupportingDocuments.Find(x => x.CSI_Code == docType).ToList();
			foreach (var docu in invoiceSupDocsToRemove)
			{
				invoice.SupportingDocuments.RemoveAndDelete(docu);
			}
		}
	}

	void DeleteExistingInvoiceLinesSupportingDocuments(string docType)
	{
		foreach (JobComInvoiceHeader invoice in Declaration.Invoices)
		{
			foreach (JobComInvoiceLine invoiceLine in invoice.InvoiceLines)
			{
				var invoiceLineSupDocsToRemove = invoiceLine.SupportingDocuments.Find(x => x.CSI_Code == docType).ToList();
				foreach (var docu in invoiceLineSupDocsToRemove)
				{
					invoiceLine.SupportingDocuments.RemoveAndDelete(docu);
				}
			}
		}
	}

	static decimal GetAmountFor7002Doc(CusEntryLine entryLine) => entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_VAT_Additions);

	static decimal GetAmountFor7003Doc(CusEntryLine entryLine) => entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.ValuationCalculator.GetREAChargeAmount());

	decimal GetAmountToBeGuaranteedAEAT(CusEntryLine entryLine) => entryLine.Fees.Cast<CusEntryLineFee>().Where(fee => !IsChargeTypeATC(fee)).Sum(fee => (decimal)fee.CF_ChargeAmount);

	decimal GetAmountToBeGuaranteedATC(CusEntryLine entryLine) => entryLine.Fees.Cast<CusEntryLineFee>().Where(IsChargeTypeATC).Sum(fee => (decimal)fee.CF_ChargeAmount);

	bool IsChargeTypeATC(CusEntryLineFee fee)
	{
		var taxClass = fee.GetTaxClass(Declaration.DestinationStateIsCanaryIsland);

		return taxClass.StartsWith("3") || taxClass.StartsWith("4");
	}

	void ResetLIQ9015SupportingDocuments()
	{
		var docType = SupportingDocumentType.VATReductionCode;

		foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
		{
			var doc9015IsNeeded = entryHeader.Is9015SupportingDocumentNeeded();

			if (doc9015IsNeeded)
			{
				entryHeader.Remove9015Documents();
			}

			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				DeletePreviouslySentNotAcceptedTypeSupportingDocuments(entryLine, docType);

				if (!EntryLineHasAcceptedSupportingDocument(entryLine, docType) && doc9015IsNeeded && entryLine.CL_LineNumber == 1)
				{
					var declarantPK = Declaration.Declarant?.OA_OH ?? ZGuid.Empty;
					var authorisationForDeclarant = Declaration.LoadAEOCusGuaranteeHeaderFromReference(declarantPK);

					AddNewLIQSupportingDocumentToEntryLine(entryLine, docType, authorisationForDeclarant.CPH_Number);
				}

				entryLine.ReadOnlySupportingDocuments.LoadNew();
			}
		}
	}

	void ResetLIQ5018SupportingDocuments()
	{
		var docType = SupportingDocumentType.WHLOCAuthorisation;

		foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
		{
			DeleteExistingJobDeclarationSupportingDocuments(docType);
			DeleteExistingInvoiceHeadersSupportingDocuments(docType);
			DeleteExistingInvoiceLinesSupportingDocuments(docType);

			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				DeletePreviouslySentNotAcceptedTypeSupportingDocuments(entryLine, docType);

				var amount = entryLine.ThirdQuantity;
				var unit = entryLine.ThirdUQ;
				var unitIsCorrect = !unit.IsEmpty && unit != ESConstants.UOM.PK && unit != ESConstants.UOM.GF;
				var reference = entryLine.RandomLine?.EntryInstruction?.SupportingDocuments?.Cast<SupportingDocument>().FirstOrDefault(x => x.CSI_Code == docType)?.CSI_ReferenceNumber ?? ZString.Empty;

				if (!EntryLineHasAcceptedSupportingDocument(entryLine, docType) && !amount.IsEmpty && unitIsCorrect && !reference.IsEmpty)
				{
					AddNewLIQSupportingDocumentToEntryLine(entryLine, docType, reference, amount, unit);
				}

				entryLine.ReadOnlySupportingDocuments.LoadNew();
			}
		}
	}

	void ResetLIQ7015SupportingDocuments()
	{
		var docType = SupportingDocumentType.FluorinatedGases;

		foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
		{
			DeleteExistingJobDeclarationSupportingDocuments(docType);
			DeleteExistingInvoiceHeadersSupportingDocuments(docType);
			DeleteExistingInvoiceLinesSupportingDocuments(docType);

			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				DeletePreviouslySentNotAcceptedTypeSupportingDocuments(entryLine, docType);

				var pca = entryLine.RandomLine?.ZG_GlobalWarmingPotential ?? ZDecimal.Zero;
				if (entryLine.RandomLine.IsPCAApplicable && !pca.IsEmpty)
				{
					var reference = pca.ToString(2, useCommas: true);
					var (quantity, quantityUnit) = Declaration.IsUCC6 ? (pca, "NAR") : (0, string.Empty);

					AddNewLIQSupportingDocumentToEntryLine(entryLine, docType, reference, quantity, quantityUnit);
				}

				entryLine.ReadOnlySupportingDocuments.LoadNew();
			}
		}
	}

	void ResetH1SSupportingDocuments(string docType, string reference, Func<CusEntryLine, decimal> getValueAmount, bool canaryIslandsOnly = false)
	{
		foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
		{
			var supportingDocumentRequired = (entryHeader.EntryInstruction is not null) && !entryHeader.EntryInstruction.IsH2 && entryHeader.EntryInstruction.IsSubStyleBOrC && entryHeader.IsUCC6;
			if (canaryIslandsOnly)
			{
				supportingDocumentRequired = supportingDocumentRequired && Declaration.DestinationStateIsCanaryIsland;
			}

			entryHeader.RemoveSupportingDocuments(docType);

			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				DeletePreviouslySentNotAcceptedTypeSupportingDocuments(entryLine, docType);

				if (supportingDocumentRequired)
				{
					var valueAmount = getValueAmount(entryLine);

					AddNewH1SSupportingDocumentToEntryLine(entryLine, docType, reference, valueAmount);
				}

				entryLine.ReadOnlySupportingDocuments.LoadNew();
			}
		}
	}

	ZBool EntryLineHasAcceptedSupportingDocument(CusEntryLine entryLine, string docType) => entryLine.GetPreviouslySentSupportingDocuments().Any(x => x.CSI_Code == docType && x.CSI_Status == DocumentStatus.Accepted);

	void DeletePreviouslySentNotAcceptedTypeSupportingDocuments(CusEntryLine entryLine, ZString docType) =>
		entryLine.GetPreviouslySentSupportingDocuments().Where(x => x.CSI_Code == docType && x.CSI_Status != DocumentStatus.Accepted).ForEach(x => x.Delete());

	void AddNewLIQSupportingDocumentToEntryLine(CusEntryLine entryLine, string docType, string reference, decimal quantity = 0m, string qtyUnit = "", decimal amount = 0m)
	{
		var document = AddNewCommonSupportingDocumentToEntryLine(entryLine, docType, SupportingDocumentSubType.LIQ, reference);

		if (quantity > ZDecimal.Zero)
		{
			document.CSI_Quantity = quantity;
		}

		if (!string.IsNullOrEmpty(qtyUnit))
		{
			document.CSI_UnitOfQuantity = qtyUnit;
		}

		if (entryLine.Header.IsUCC6 && amount > ZDecimal.Zero)
		{
			document.CSI_Value = amount;
			document.CSI_RX_NKCurrency = Core.Constants.CurrencyCodes.Spain;
		}
	}

	void AddNewH1SSupportingDocumentToEntryLine(CusEntryLine entryLine, string docType, string reference, decimal valueAmount)
	{
		var document = AddNewCommonSupportingDocumentToEntryLine(entryLine, docType, SupportingDocumentSubType.H1S, reference);
		document.CSI_Value = valueAmount;
		document.CSI_RX_NKCurrency = Core.Constants.CurrencyCodes.Spain;
	}

	SupportingDocument AddNewCommonSupportingDocumentToEntryLine(CusEntryLine entryLine, string docType, string docSubType, string reference)
	{
		var document = entryLine.Factory.New<SupportingDocument>();
		document.CSI_Code = docType;
		document.CSI_SubType = docSubType;
		document.CSI_ReferenceNumber = reference;
		document.CSI_ParentID = entryLine.PK;
		document.CSI_ParentTableCode = entryLine.TablePrefix;
		document.CSI_Status = ZString.Empty;
		document.CSI_DataModel = entryLine.CountryCode;
		return document;
	}

	void SetUCC6Version()
	{
		foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
		{
			if ((Declaration.IsExport || (Declaration.IsImport && !entryHeader.IsH2Style)) && (entryHeader.CH_EntryStatus.IsEmpty && !entryHeader.IsWaitingForResponse))
			{
				if (Declaration.IsUCC6 && entryHeader.EntryInstruction != null && entryHeader.EntryInstruction.IsSubStyleAOrBOrCOrXOrYOrZ)
				{
					entryHeader.ZG_UCC6Version = UCC6VersionCodes.UCC6;
				}
				else
				{
					entryHeader.ZG_UCC6Version = UCC6VersionCodes.NoUCC6;
				}
			}
		}
	}

	void SetPOUSVersion()
	{
		foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
		{
			if (entryHeader.CH_EntryStatus.IsEmpty && !entryHeader.IsWaitingForResponse)
			{
				if (MessageVersionRegistryProvider.IsT2LAndAnyVersionPOUS() && entryHeader.EntryInstruction != null && (entryHeader.EntryInstruction.IsT2C || entryHeader.EntryInstruction.IsT2L))
				{
					entryHeader.ZG_POUSVersion = MessageVersionRegistryProvider.IsT2LVersionPOUS() ? POUSVersionCodes.POUS : POUSVersionCodes.POUS2;
				}
				else
				{
					entryHeader.ZG_POUSVersion = POUSVersionCodes.NoPOUS;
				}
			}
		}
	}

	void AddT2LSupportingDocuments()
	{
		AddT2LOrT2LFSupportingDocuments(SupportingDocumentType.T2L, ExportCommunityTransitStatusList.Codes.T2L);
	}

	void AddT2LFSupportingDocuments()
	{
		AddT2LOrT2LFSupportingDocuments(SupportingDocumentType.T2LF, ExportCommunityTransitStatusList.Codes.T2LF);
	}

	void AddT2LOrT2LFSupportingDocuments(ZString code, ZString referenceAndCTStatus)
	{
		if (DeclarationHasExpectedCTStatus(Declaration, referenceAndCTStatus))
		{
			Declaration.ActiveEntryHeaders.Where(header => EntryIsExportUcc6HasNoMRN((CusEntryHeader)header)).ForEach(header => AddT2LOrT2LFSupportingDocumentToEntryHeader((CusEntryHeader)header, code, referenceAndCTStatus));
		}
		else
		{
			Declaration.ActiveEntryHeaders.ForEach(header => ((CusEntryHeader)header).MergedLines.ForEach(line => DeletePreviouslySentNotAcceptedTypeSupportingDocuments(line, code)));
		}
	}

	void AddT2LOrT2LFSupportingDocumentToEntryHeader(CusEntryHeader entryHeader, ZString code, ZString referenceAndCTStatus)
	{
		if (!entryHeader.MergedLines.Any(x => EntryLineHasExpectedDoc(x, code)))
		{
			if (Declaration.IsTransitionPeriodAES30)
			{
				AddNewLIQSupportingDocumentToEntryLine(entryHeader.MergedLines.FirstOrDefault(l => l.CL_LineNumber == 1), code, referenceAndCTStatus);
			}
			else
			{
				entryHeader.EntryInstruction.AddNewSupportingDocument(code, referenceAndCTStatus);
			}
		}

		entryHeader.MergedLines.ForEach(x => x.ReadOnlySupportingDocuments.LoadNew());
	}

	void AddSupportingDocumentsForH2(string docType)
	{
		foreach (CusEntryInstruction entryInstruction in Declaration.CustomsEntryInstructions)
		{
			if (!entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == docType))
			{
				if (entryInstruction.IsH2)
				{
					AddSupportingDocumentToEntryInstructionForH2(entryInstruction, docType);
				}
			}
		}
	}

	void AddSupportingDocumentToEntryInstructionForH2(CusEntryInstruction entryInstruction, string docType)
	{
		if (docType == SupportingDocumentType.WHLOCAuthorisation)
		{
			var authorisationRule = entryInstruction.GetAutorisationRuleFor5018Doc();
			if (authorisationRule != null)
			{
				entryInstruction.AddNewSupportingDocument(SupportingDocumentType.WHLOCAuthorisation, authorisationRule.CPR_ValueFrom);
			}
		}
		else
		{
			var authorisation = entryInstruction.GetAuthorisationHeaderForDocs(docType);
			if (authorisation != null)
			{
				entryInstruction.AddNewSupportingDocument(docType, authorisation.CPH_Number);
			}
		}
	}

	ZBool DeclarationHasExpectedCTStatus(JobDeclaration declaration, ZString expectedCTStatus) => declaration.ZG_CTStatusID == expectedCTStatus;

	ZBool EntryIsExportUcc6HasNoMRN(CusEntryHeader entryHeader) => entryHeader.IsExportUCC6 && entryHeader.MovementReferenceNumber.IsEmpty;

	ZBool EntryLineHasExpectedDoc(CusEntryLine entryLine, ZString expectedCode) => entryLine.ReadOnlySupportingDocuments.Any(x => ((ReadOnlySupportingDocument)x).CSI_Code == expectedCode);

	void PopulateLocationOfGoodsIfNeeded()
	{
		var declarationLocation = Declaration.JE_LocationOfGoods;
		if (declarationLocation.StartsWith(Core.Constants.CountryCodes.Spain))
		{
			foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
			{
				var instruction = entryHeader.EntryInstruction;
				if (instruction?.GoodsLocationDescription.IsEmpty ?? false)
				{
					var goodsLocation = instruction.GoodsLocation;
					goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
					goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
					goodsLocation.Address.AuthorisationNumber = declarationLocation.Left(CusGoodsLocationAddress.Schema.E2_GovRegNumMaxLength);
				}
			}
		}
	}
}

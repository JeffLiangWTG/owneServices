using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public interface IDeclarationOfIntentRefresher
{
	ZBool ShouldAskToOverwrite();
	void OverwritePlaceholderSupportingDocumentValues();
}

public class DeclarationOfIntentRefresher : IDeclarationOfIntentRefresher
{
	public DeclarationOfIntentRefresher(JobDeclaration declaration)
	{
		Declaration = Argument.NotNull(declaration, nameof(declaration));
	}

	public void DefaultZG_UseDeclarationOfIntent()
	{
		foreach (CusEntryInstruction entryInstruction in EntryInstructions)
		{
			DefaultZG_UseDeclarationOfIntent(entryInstruction);
		}
	}

	public void DefaultZG_UseDeclarationOfIntent(CusEntryInstruction entryInstruction)
	{
		Argument.NotNull(entryInstruction, nameof(entryInstruction));

		var useDeclarationOfIntent = ZBool.False;
		if (Declaration.IsImport)
		{
			var useRule = DOIAuthorisation?.CusAuthorisationRules.SingleOrDefault(x => x.CPR_RuleCode == ITCusAuthorisationRuleTypeList.Codes.Use);
			useDeclarationOfIntent = (useRule?.CPR_ValueFrom ?? ZString.Empty) == CusAuthorisationRuleUseValueList.Codes.Always;
		}
		entryInstruction.ZG_UseDeclarationOfIntent = useDeclarationOfIntent;
	}

	public void DefaultSupportingDocument01DI()
	{
		foreach (CusEntryInstruction entryInstruction in EntryInstructions)
		{
			DefaultSupportingDocument01DI(entryInstruction);
		}
	}

	public void DefaultSupportingDocument01DI(CusEntryInstruction entryInstruction)
	{
		Argument.NotNull(entryInstruction, nameof(entryInstruction));

		var shouldDefaultSupportingDocument01DI = ShouldDefaultSupportingDocument01DI(entryInstruction);

		foreach (var invoice in entryInstruction.Invoices)
		{
			RemoveAllSupportingDocuments01DI(invoice);

			if (shouldDefaultSupportingDocument01DI)
			{
				AddSystemGeneratedSupportingDocument01DI(invoice);
			}
		}
	}

	public ZBool ShouldAskToOverwrite()
	{
		return Declaration.IsImport
			&& GetDOIAuthorisationNumberValueObject().IsPlaceholder
			&& GetEntryInstructionAndDOISupportingDocumentsWrappers().Any();
	}

	public void OverwritePlaceholderSupportingDocumentValues()
	{
		var entryInstructionWithDOIWrappers = GetEntryInstructionAndDOISupportingDocumentsWrappers();
		foreach (var wrapper in entryInstructionWithDOIWrappers)
		{
			var applicableDOI = wrapper.ValidDOISupportingDocument;
			if (applicableDOI != null)
			{
				foreach (var documentToUpdate in wrapper.DOISupportingDocuments.Where(x => x.CSI_ReferenceNumber == Placeholder))
				{
					UpdateSupportingDocument(documentToUpdate, applicableDOI.CSI_ReferenceNumber, applicableDOI.CSI_RN_NKCountryCode, applicableDOI.CSI_DateOfIssue);
				}
			}
		}
	}

	#region Implementation

	JobDeclaration Declaration { get; }
	CusEntryInstructionCollection EntryInstructions => Declaration.CustomsEntryInstructions;
	Customs.Business.CusAuthorisationHeader DOIAuthorisation => Declaration.DeclarationOfIntentAuthorisation;

	bool ShouldDefaultSupportingDocument01DI(CusEntryInstruction entryInstruction)
	{
		return Declaration.IsImport
			&& DOIAuthorisation != null
			&& entryInstruction.ZG_UseDeclarationOfIntent;
	}

	void AddSystemGeneratedSupportingDocument01DI(JobComInvoiceHeader invoice)
	{
		var doiValueObject = GetDOIAuthorisationNumberValueObject();
		var systemGenerated01DI = invoice.SupportingDocuments.AddNew();
		UpdateSupportingDocument(systemGenerated01DI, doiValueObject.DOINumber, DOIAuthorisation.PermitHolder?.CountryCode ?? ZString.Empty, doiValueObject.IssueDate);
	}

	void UpdateSupportingDocument(SupportingDocument supportingDocument, ZString doiNumber, ZString countryCode, ZDateTime issueDate)
	{
		supportingDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent;
		supportingDocument.CSI_ReferenceNumber = doiNumber;
		supportingDocument.CSI_RN_NKCountryCode = countryCode;
		supportingDocument.CSI_DateOfIssue = issueDate;
	}

	DeclarationOfIntentValueObject GetDOIAuthorisationNumberValueObject() => new DeclarationOfIntentValueObject(DOIAuthorisation?.CPH_Number ?? ZString.Empty);

	void RemoveAllSupportingDocuments01DI(JobComInvoiceHeader invoiceHeader)
	{
		var supportingDocuments01DI = invoiceHeader.SupportingDocuments.GetDeclarationOfIntentSupportingDocuments().ToArray();
		foreach (var supportingDocument in supportingDocuments01DI)
		{
			invoiceHeader.SupportingDocuments.RemoveAndDelete(supportingDocument);
		}
	}

	IEnumerable<EntryInstructionAndDOISupportingDocumentsWrapper> GetEntryInstructionAndDOISupportingDocumentsWrappers()
	{
		return EntryInstructions.Cast<CusEntryInstruction>()
			.Where(entryInstruction => entryInstruction.ZG_UseDeclarationOfIntent)
			.Select(entryInstruction => new EntryInstructionAndDOISupportingDocumentsWrapper(entryInstruction))
			.Where(wrapper => wrapper.ContainsTwoDistinctDOINumbersAndOneIsPlaceholder());
	}

	class EntryInstructionAndDOISupportingDocumentsWrapper
	{
		public EntryInstructionAndDOISupportingDocumentsWrapper(CusEntryInstruction entryInstruction)
		{
			EntryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
		}

		public CusEntryInstruction EntryInstruction { get; }

		public SupportingDocument[] DOISupportingDocuments => doiSupportingDocuments ?? (doiSupportingDocuments = LoadSupportingDocuments());
		SupportingDocument[] doiSupportingDocuments;

		SupportingDocument[] LoadSupportingDocuments() => EntryInstruction.Invoices.SelectMany(x => x.SupportingDocuments.GetDeclarationOfIntentSupportingDocuments()).ToArray();

		public SupportingDocument ValidDOISupportingDocument => validDOISupportingDocument ?? (validDOISupportingDocument = LoadValidSupportingDocument());
		SupportingDocument validDOISupportingDocument;

		SupportingDocument LoadValidSupportingDocument() => DOISupportingDocuments.FirstOrDefault(x => x.CSI_ReferenceNumber != Placeholder);

		public ZBool ContainsTwoDistinctDOINumbersAndOneIsPlaceholder()
		{
			var doiNumbers = DOISupportingDocuments.Select(x => x.CSI_ReferenceNumber).Distinct().ToArray();
			return doiNumbers.Length == 2 && doiNumbers.Any(x => x == Placeholder);
		}
	}

	const string Placeholder = "X";

	#endregion
}

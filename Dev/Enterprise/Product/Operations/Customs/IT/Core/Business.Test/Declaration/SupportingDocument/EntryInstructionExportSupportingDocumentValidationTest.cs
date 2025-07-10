using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.Testing.Declaration.SupportingDocument;

sealed class EntryInstructionExportSupportingDocumentValidationTest : TestCaseWithFactory
{
	public void TestValidateIfSupportingDocumentsAreAllowed()
	{
		const string expectedMessageError = "[E1301] During the transition period, which is active now, Supporting documents at header level must not be used";

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			using (TemporallySetTransitionPeriod(isActive: true))
			{
				var supportingDocument = entryInstruction.SupportingDocuments.AddNew();
				supportingDocument.CSI_Code = "1001";
				var validation = CreateValidation(supportingDocument);
				validation.ValidateIfSupportingDocumentsAreAllowed();
				AssertHasRowMessageError("[UCC6] When in Transition Period Header Level Supporting Document is present", supportingDocument, expectedMessageError);
			}

			SetupDataUnderNoTransitionPeriodAndAssertNoMessageError("UCC6");
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: false))
		{
			SetupDataUnderNoTransitionPeriodAndAssertNoMessageError("Non-UCC6");
		}

		void SetupDataUnderNoTransitionPeriodAndAssertNoMessageError(string preconditionMessage)
		{
			using (TemporallySetTransitionPeriod(isActive: false))
			{
				var supportingDocument = entryInstruction.SupportingDocuments.AddNew();
				var validation = CreateValidation(supportingDocument);
				validation.ValidateIfSupportingDocumentsAreAllowed();
				AssertNoRowMessageError($"{preconditionMessage} When Not in Transition Period and Header Level Supporting document is present", supportingDocument, expectedMessageError);
			}
		}
	}

	public void TestValidateSupportingDocumentsCount()
	{
		const string expectedMessageError = "You have entered more than 99 Supporting Documents";
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			CombineAssertions("When UCC6", () =>
			{
				ClearAndAddSupportingDocuments(100);
				var supportingDocument = GetFirstSupportingDocument();
				CreateValidation(supportingDocument).ValidateSupportingDocumentsCount();
				AssertHasRowMessageError("When EntryInstruction has more than 100 Supporting Documents", supportingDocument, expectedMessageError);

				ClearAndAddSupportingDocuments(99);
				supportingDocument = GetFirstSupportingDocument();
				CreateValidation(supportingDocument).ValidateSupportingDocumentsCount();
				AssertNoRowMessageError("When EntryInstruction has exactly 99 Supporting Documents", supportingDocument, expectedMessageError);

				ClearAndAddSupportingDocuments(98);
				CreateValidation(supportingDocument).ValidateSupportingDocumentsCount();
				AssertNoRowMessageError("When EntryInstruction has less than 99 (Exact 98) Supporting Documents", supportingDocument, expectedMessageError);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: false))
		{
			CombineAssertions("When Non-UCC6", () =>
			{
				ClearAndAddSupportingDocuments(100);
				var supportingDocument = GetFirstSupportingDocument();
				CreateValidation(supportingDocument).ValidateSupportingDocumentsCount();
				AssertNoRowMessageError("When EntryInstruction has more than 100 Supporting Documents", supportingDocument, expectedMessageError);
			});
		}

		Business.Declaration.SupportingDocument GetFirstSupportingDocument() => entryInstruction.SupportingDocuments[0];
	}

	public void TestValidateSupportingDocumentCount_MessageIsNotShownWhenDocumentsAreRemovedAndCountIsLessThan99()
	{
		const string expectedMessageError = "You have entered more than 99 Supporting Documents";
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			ClearAndAddSupportingDocuments(100);
			var supportingDocument = entryInstruction.SupportingDocuments[0];
			var validator = CreateValidation(supportingDocument);
			validator.ValidateAll();
			AssertHasRowMessageError("When EntryInstruction has more than 100 Supporting Documents", supportingDocument, expectedMessageError);

			entryInstruction.SupportingDocuments.Remove(entryInstruction.SupportingDocuments[99].PK);
			entryInstruction.SupportingDocuments.Remove(entryInstruction.SupportingDocuments[98].PK);
			entryInstruction.SupportingDocuments.Remove(entryInstruction.SupportingDocuments[97].PK);
			entryInstruction.SupportingDocuments.Remove(entryInstruction.SupportingDocuments[96].PK);
			entryInstruction.SupportingDocuments.Remove(entryInstruction.SupportingDocuments[95].PK);

			validator.ValidateAll();
			AssertNoRowMessageError("When EntryInstruction has less than 99 Supporting Documents", supportingDocument, expectedMessageError);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	CusEntryHeader entryHeader;

	#region Implementation

	IDisposable TemporallySetTransitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isActive);

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isActive)
		=> ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isActive);

	void ClearAndAddSupportingDocuments(int requiredNumberOfDocuments)
	{
		entryInstruction.SupportingDocuments.RemoveAndDeleteAll();

		for (int i = 1; i <= requiredNumberOfDocuments; i++)
		{
			var doc = entryInstruction.SupportingDocuments.AddNew();
			doc.CSI_Code = i.ToString();
			doc.CSI_SubType = "Y";
			doc.CSI_ReferenceNumber = $"CSI_ReferenceNumber-{i}";
		}
	}

	EntryInstructionExportSupportingDocumentValidation CreateValidation(Business.Declaration.SupportingDocument document) => new EntryInstructionExportSupportingDocumentValidation(document);

	#endregion
}

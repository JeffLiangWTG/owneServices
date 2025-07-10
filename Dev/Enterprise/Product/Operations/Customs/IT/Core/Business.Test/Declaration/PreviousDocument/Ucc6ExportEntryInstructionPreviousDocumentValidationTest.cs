using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportEntryInstructionPreviousDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_Procedure()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			previousDocument.Validation.ValidateCSI_Procedure();
			AssertNoMessageErrors(previousDocument.CSI_ProcedureInfo);
		}
	}

	public void TestCheckCSI_SubType()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			previousDocument.Validation.ValidateCSI_SubType();
			AssertNoMessageErrors(previousDocument.CSI_SubTypeInfo);
		}
	}

	public void TestCheckCSI_UnitOfQuantity()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			previousDocument.CSI_UnitOfQuantity = ZString.Empty;
			AssertNoMessageErrors(previousDocument.CSI_UnitOfQuantityInfo);
		}
	}

	public void TestCheckCSI_UnitOfQuantity3()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			previousDocument.CSI_UnitOfQuantity3 = ZString.Empty;
			AssertNoMessageErrors(previousDocument.CSI_UnitOfQuantity3Info);
		}
	}

	public void TestGetReferenceNumberValidator()
	{
		var validation = new Ucc6ExportEntryInstructionPreviousDocumentValidationForTest(previousDocument);
		AssertType<Ucc6ExportEntryInstructionPreviousDocumentReferenceNumberValidator>(validation.GetReferenceNumberValidatorExposed());
	}

	public void TestValidatePreviousDocumentPresenceWithEntrySubStyleRuleB1905()
	{
		const string expectedMessage = "[B1905] For Sub Style different from X and Y no Previous Documents must be present in Entry Instructions.";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			using (TemporarilySetTransitionPeriod(isActive: true))
			{
				CombineAssertions("When Ucc6 and AESTP ON", () =>
				{
					entryInstruction.CEI_SubStyle = "A";
					previousDocument.Validation.ValidateAll();
					AssertHasRowMessageError("For SubStyle = A, at Previous document", previousDocument, expectedMessage);

					entryInstruction.CEI_SubStyle = "D";
					previousDocument.Validation.ValidateAll();
					AssertHasRowMessageError("For SubStyle = D and, at Previous document", previousDocument, expectedMessage);

					entryInstruction.CEI_SubStyle = "X";
					previousDocument.Validation.ValidateAll();
					AssertNoRowMessageError("For SubStyle = X and, at Previous document", previousDocument, expectedMessage);

					entryInstruction.CEI_SubStyle = "Y";
					previousDocument.Validation.ValidateAll();
					AssertNoRowMessageError("For SubStyle = Y and, at Previous document", previousDocument, expectedMessage);

					entryInstruction.CEI_SubStyle = "Z";
					previousDocument.Validation.ValidateAll();
					AssertHasRowMessageError("For SubStyle = Z and, at Previous document", previousDocument, expectedMessage);
				});
			}

			using (TemporarilySetTransitionPeriod(isActive: false))
			{
				CombineAssertions("When Ucc6 and AESTP OFF", () =>
				{
					entryInstruction.CEI_SubStyle = "A";
					previousDocument.Validation.ValidateAll();
					AssertNoRowMessageError("For SubStyle = A, at Previous document", previousDocument, expectedMessage);

					entryInstruction.CEI_SubStyle = "D";
					previousDocument.Validation.ValidateAll();
					AssertNoRowMessageError("For SubStyle = D and, at Previous document", previousDocument, expectedMessage);

					entryInstruction.CEI_SubStyle = "Z";
					previousDocument.Validation.ValidateAll();
					AssertNoRowMessageError("For SubStyle = Z and, at Previous document", previousDocument, expectedMessage);
				});
			}
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		previousDocument = entryInstruction.PreviousDocuments.AddNew();
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	PreviousDocument previousDocument;

	IDisposable TemporarilySetTransitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isActive);

	class Ucc6ExportEntryInstructionPreviousDocumentValidationForTest : Ucc6ExportEntryInstructionPreviousDocumentValidation
	{
		public Ucc6ExportEntryInstructionPreviousDocumentValidationForTest(PreviousDocument parent) : base(parent)
		{
		}

		public IPreviousDocumentReferenceNumberValidator GetReferenceNumberValidatorExposed() => base.GetReferenceNumberValidator();
	}
}

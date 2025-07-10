using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportEntryInstructionAdditionalInfoValidationTest : TestCaseWithFactory
{
	public void TestRuleE1301()
	{
		const string expectedMessageError = "[E1301] during the transition period, which is active now, Additional Documents at message header level must not be used";
		using (TemporaryClearDeclarationConfigurationAndSetUcc6(isActive: true))
		{
			using (TemporarySetTransitionPeriod(isActive: true))
			{
				var additionalInfo = entryInstruction.AdditionalInfos.AddNew();
				ValidateRuleE1301(additionalInfo);
				AssertHasRowMessageError("When Transition Period is ON", additionalInfo, expectedMessageError);
			}

			using (TemporarySetTransitionPeriod(isActive: false))
			{
				var additionalInfo = entryInstruction.AdditionalInfos.AddNew();
				ValidateRuleE1301(additionalInfo);
				AssertNoRowMessageError("When Transition Period is Off", additionalInfo, expectedMessageError);
			}
		}

		void ValidateRuleE1301(AdditionalInfo additionalInfo)
		{
			new Ucc6ExportEntryInstructionAdditionalInfoValidation(additionalInfo).ValidateRuleE1301();
		}
	}

	public void TestValidateMoreThan99DocumentsAreNotAllowedPerType()
	{
		const string expectedMessageError = "You have entered more than 99 Additional Documents with Type";
		using (TemporaryClearDeclarationConfigurationAndSetUcc6(isActive: true))
		{
			SetupDataAndAssertDocumentCountForAdditionalDocuments("INF");
			SetupDataAndAssertDocumentCountForAdditionalDocuments("REF");
			SetupDataAndAssertDocumentCountForAdditionalDocuments("TRA");

			ClearAndAddDocumentsToEntryInstruction("XYZ", 100);
			var document = GetFirstDocument();
			ValidateDocumentCount(document);
			AssertNoRowMessageErrorContaining(document, expectedMessageError);
		}

		void SetupDataAndAssertDocumentCountForAdditionalDocuments(string documentType)
		{
			CombineAssertions($"When DocumentType is {documentType}", () =>
			{
				ClearAndAddDocumentsToEntryInstruction(documentType, 100);
				var document = GetFirstDocument();
				ValidateDocumentCount(document);
				AssertHasRowMessageErrorContaining(document, expectedMessageError);

				ClearAndAddDocumentsToEntryInstruction(documentType, 97);
				document = GetFirstDocument();
				ValidateDocumentCount(document);
				AssertNoRowMessageErrorContaining(document, expectedMessageError);

				ClearAndAddDocumentsToEntryInstruction(documentType, 99);
				document = GetFirstDocument();
				entryInstruction.AdditionalInfos.AddNew().CSI_SubType = "XYZ";
				ValidateDocumentCount(document);
				AssertNoRowMessageErrorContaining(document, expectedMessageError);
			});
		}

		void ValidateDocumentCount(AdditionalInfo document)
			=> new Ucc6ExportEntryInstructionAdditionalInfoValidation(document).ValidateMoreThan99DocumentsAreNotAllowedPerType();

		AdditionalInfo GetFirstDocument() => entryInstruction.AdditionalInfos.Cast<AdditionalInfo>().First();

		void ClearAndAddDocumentsToEntryInstruction(string documentType, int numberOfDocuments)
		{
			entryInstruction.AdditionalInfos.RemoveAndDeleteAll();
			for (int i = 0; i < numberOfDocuments; i++)
			{
				var document = entryInstruction.AdditionalInfos.AddNew();
				var counterString = i.ToString();
				document.CSI_SubType = documentType;
				document.CSI_Code = counterString;
				document.CSI_ReferenceNumber = "REFERENCE" + counterString;
			}
		}
	}

	public void TestCheckCSI_SubType()
	{
		declaration.MessageVersion = "XML";
		var additionalInfo = entryInstruction.AdditionalInfos.AddNew();

		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(additionalInfo.CSI_SubTypeInfo, invalidCodes: new ZString[] { "X" }, validCodes: new ZString[] { "INF", "REF" });
	}

	public void TestCSI_ReferenceNumber_MandatoryValidation()
	{
		AdditionalInfoTestHelper.SetUpRefCusCodesForAttributeName(Factory);

		using (TemporaryClearDeclarationConfigurationAndSetUcc6(isActive: true))
		{
			var additionalInfo = entryInstruction.AdditionalInfos.AddNew();
			CombineAssertions(() =>
			{
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				additionalInfo.CSI_Code = "AB01C";
				additionalInfo.CSI_ReferenceNumber = ZString.Empty;
				AssertHasMessageErrorContaining("CSI_ReferenceNumber is empty, Kind: REF, Code requires CSI_ReferenceNumber", additionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				additionalInfo.CSI_ReferenceNumber = "12345";
				AssertNoMessageErrorContaining("CSI_ReferenceNumber is filled, Kind: REF, Code requires CSI_ReferenceNumber", additionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				additionalInfo.CSI_Code = "XY01Z";
				additionalInfo.CSI_ReferenceNumber = ZString.Empty;
				AssertNoMessageErrorContaining("CSI_ReferenceNumber is empty, Kind: REF, Code does not require CSI_ReferenceNumber", additionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalInfo.CSI_Code = "AB01C";
				additionalInfo.Validation.ValidateCSI_ReferenceNumber();
				AssertHasMessageErrorContaining("CSI_ReferenceNumber is empty, Kind: TRA, Code does not matter", additionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalInfo.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageErrorContaining("CSI_ReferenceNumber empty, Kind: INF, Code requires CSI_ReferenceNumber", additionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
	}

	CusEntryInstruction entryInstruction;
	JobDeclaration declaration;

	IDisposable TemporarySetTransitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, isActive);

	IDisposable TemporaryClearDeclarationConfigurationAndSetUcc6(bool isActive)
		=> ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isActive);
}

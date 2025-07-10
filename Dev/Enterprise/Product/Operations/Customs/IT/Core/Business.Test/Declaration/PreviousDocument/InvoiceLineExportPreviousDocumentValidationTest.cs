using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class InvoiceLineExportPreviousDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestConstructor()
	{
		var document = Factory.New<PreviousDocument>();
		AssertExceptionThrown<ArgumentNullException>(() => new InvoiceLineExportPreviousDocumentValidation(document));

		var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
		var invoiceLineLevelDocument = jobComInvoiceLine.PreviousDocuments.AddNew();
		AssertNoExceptionThrown("When Document's parent is an invoice line", () => new InvoiceLineExportPreviousDocumentValidation(invoiceLineLevelDocument));

		var declarationLevelPreviousDocument = declaration.PreviousDocuments.AddNew();
		AssertExceptionThrown<ArgumentNullException>(() => new InvoiceLineExportPreviousDocumentValidation(declarationLevelPreviousDocument));
	}

	public void TestCheckCSI_PackageType_ForNumberOfPackages()
	{
		const string expectedMessageError = "When Number of Package is provided at Invoice Line's Previous Document, then Package Type is mandatory";
		var packageTypePropertyInfo = previousDocument.CSI_PackTypeInfo;

		RunTestUnderUcc6Configuration(() =>
		{
			previousDocument.CSI_PackQty = 1;
			previousDocument.CSI_PackType = ZString.Empty;
			previousDocument.Validation.ValidateCSI_PackType();
			AssertHasMessageErrorContaining("When Package Type is empty and Quantity is 1", packageTypePropertyInfo, expectedMessageError);

			previousDocument.CSI_PackType = "2C";
			previousDocument.Validation.ValidateCSI_PackType();
			AssertNoMessageErrorContaining("When Package Type is not empty", packageTypePropertyInfo, expectedMessageError);

			previousDocument.CSI_PackQty = 0;
			previousDocument.CSI_PackType = ZString.Empty;
			previousDocument.Validation.ValidateCSI_PackType();
			AssertNoMessageErrorContaining("When Package Type is empty and Quantity is 0", packageTypePropertyInfo, expectedMessageError);
		});

		RunTestUnderNonUcc6Configuration(() =>
		{
			previousDocument.CSI_PackQty = 1;
			previousDocument.CSI_PackType = ZString.Empty;
			previousDocument.Validation.ValidateCSI_PackType();
			AssertNoMessageErrorContaining("When Package Type is empty and Quantity is 1", packageTypePropertyInfo, expectedMessageError);
		});
	}

	public void TestCheckCSI_PackageType_RequiredForNonBulkType()
	{
		const string expectedMessageError = "Package Type (non BULK) is required at Invoice Lines only if Number of Package is provided";
		var packageTypePropertyInfo = previousDocument.CSI_PackTypeInfo;
		SetUpRefData();

		RunTestUnderUcc6Configuration(() =>
		{
			previousDocument.CSI_PackType = "AB";
			previousDocument.CSI_PackQty = 0;
			previousDocument.Validation.ValidateCSI_PackType();
			AssertHasMessageErrorContaining("When Package Type is AB (Non-Bulk) and Quantity is 0", packageTypePropertyInfo, expectedMessageError);

			previousDocument.CSI_PackQty = 1;
			previousDocument.Validation.ValidateCSI_PackType();
			AssertNoMessageErrorContaining("When Package Type is AB (Non-Bulk) and Quantity is 1", packageTypePropertyInfo, expectedMessageError);

			previousDocument.CSI_PackType = "VQ";
			previousDocument.CSI_PackQty = 0;
			previousDocument.Validation.ValidateCSI_PackType();
			AssertNoMessageErrorContaining("When Package Type is VQ (Bulk) and Quantity is 0", packageTypePropertyInfo, expectedMessageError);
		});

		RunTestUnderNonUcc6Configuration(() =>
		{
			previousDocument.CSI_PackType = "AB";
			previousDocument.CSI_PackQty = 0;
			previousDocument.Validation.ValidateCSI_PackType();
			AssertNoMessageErrorContaining("When Package Type is AB (Non-Bulk) and Quantity is 0", packageTypePropertyInfo, expectedMessageError);
		});
	}

	public void TestCSI_UnitOfQuantity_WhenQuantityIsZeroAndUOMIsFilled()
	{
		const string expectedMessageError = "Previous Document's Measurement Unit is required at Invoice Lines only if quantity is provided.";
		var unitOfQuantityPropertyInfo = previousDocument.CSI_UnitOfQuantityInfo;

		RunTestUnderUcc6Configuration(() =>
		{
			previousDocument.CSI_Quantity = 0;
			previousDocument.CSI_UnitOfQuantity = "KG";
			previousDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertHasMessageErrorContaining("When Quantity is 0 and UOM is KG", unitOfQuantityPropertyInfo, expectedMessageError);

			previousDocument.CSI_UnitOfQuantity = ZString.Empty;
			previousDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertNoMessageErrorContaining("When Quantity is 0 and UOM is Empty", unitOfQuantityPropertyInfo, expectedMessageError);

			previousDocument.CSI_Quantity = 12;
			previousDocument.CSI_UnitOfQuantity = ZString.Empty;
			previousDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertNoMessageErrorContaining("When Quantity is 12 and UOM is Empty", unitOfQuantityPropertyInfo, expectedMessageError);
		});

		RunTestUnderNonUcc6Configuration(() =>
		{
			previousDocument.CSI_Quantity = 0;
			previousDocument.CSI_UnitOfQuantity = "KG";
			previousDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertNoMessageErrorContaining("When Quantity is 0 and UOM is KG", unitOfQuantityPropertyInfo, expectedMessageError);
		});
	}

	public void TestCSI_UnitOfQuantity_WhenQuantityIsFilledAndUOMIsEmpty()
	{
		const string expectedMessageError = "When Quantity is provided at Invoice Line's Previous Document, then Measurement Unit & Qualifier is mandatory";
		var unitOfQuantityPropertyInfo = previousDocument.CSI_UnitOfQuantityInfo;

		RunTestUnderUcc6Configuration(() =>
		{
			previousDocument.CSI_Quantity = 10;
			previousDocument.CSI_UnitOfQuantity = ZString.Empty;
			previousDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertHasMessageErrorContaining("When Quantity is 10 and UOM is Empty", unitOfQuantityPropertyInfo, expectedMessageError);

			previousDocument.CSI_UnitOfQuantity = "KG";
			previousDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertNoMessageErrorContaining("When Quantity is 0 and UOM is KG", unitOfQuantityPropertyInfo, expectedMessageError);

			previousDocument.CSI_Quantity = 0;
			previousDocument.CSI_UnitOfQuantity = ZString.Empty;
			previousDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertNoMessageErrorContaining("When Quantity is 0 and UOM is Empty", unitOfQuantityPropertyInfo, expectedMessageError);
		});

		RunTestUnderNonUcc6Configuration(() =>
		{
			previousDocument.CSI_Quantity = 10;
			previousDocument.CSI_UnitOfQuantity = ZString.Empty;
			previousDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertNoMessageErrorContaining("When Quantity is 10 and UOM is Empty", unitOfQuantityPropertyInfo, expectedMessageError);
		});
	}

	public void TestCSI_UnitOfQuantity_InvalidCodes()
	{
		SetUpRefData();

		RunTestUnderUcc6Configuration(() =>
		{
			previousDocument.CSI_UnitOfQuantity = "X";
			previousDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertHasMessageErrorContaining(previousDocument.CSI_UnitOfQuantityInfo, ListValidation.InvalidCodeMessageError);

			previousDocument.CSI_UnitOfQuantity = "KGMG";
			previousDocument.Validation.ValidateCSI_UnitOfQuantity();
			AssertNoMessageErrorContaining(previousDocument.CSI_UnitOfQuantityInfo, ListValidation.InvalidCodeMessageError);
		});
	}

	public void TestCSI_ItemNumber()
	{
		const string expectedMessageError = "When Previous Document is in either C651 or C658, then Previous Document Item No. is required";
		var itemNumberPropertyInfo = previousDocument.CSI_ItemNumberInfo;

		RunTestUnderUcc6Configuration(() =>
		{
			previousDocument.CSI_Code = "C651";
			previousDocument.CSI_ItemNumber = 0;
			previousDocument.Validation.ValidateCSI_ItemNumber();
			AssertHasMessageErrorContaining("When CSI_Code=C651 and Item Number 0", itemNumberPropertyInfo, expectedMessageError);

			previousDocument.CSI_ItemNumber = 11;
			previousDocument.Validation.ValidateCSI_ItemNumber();
			AssertNoMessageErrorContaining("When CSI_Code=C651 and Item Number 11", itemNumberPropertyInfo, expectedMessageError);

			previousDocument.CSI_Code = "C658";
			previousDocument.CSI_ItemNumber = 0;
			previousDocument.Validation.ValidateCSI_ItemNumber();
			AssertHasMessageErrorContaining("When CSI_Code=C658 and Item Number 0", itemNumberPropertyInfo, expectedMessageError);

			previousDocument.CSI_Code = "ABC";
			previousDocument.Validation.ValidateCSI_ItemNumber();
			AssertNoMessageErrorContaining("When CSI_Code=ABC and Item Number 0", itemNumberPropertyInfo, expectedMessageError);
		});
	}

	public void TestCSI_ReferenceNumber()
	{
		const string expectedMessageError = "Reference Number of Previous Document can have up to 35 alpha numeric characters";
		var referenceNumberPropertyInfo = previousDocument.CSI_ReferenceNumberInfo;

		using (TemporallySetTransitionPeriod(true))
		{
			RunTestUnderUcc6Configuration(() =>
			{
				previousDocument.CSI_ReferenceNumber = GenerateReferenceNumberString(38);
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertHasMessageErrorContaining("When In Transition Period and ReferenceNumber Length=38", referenceNumberPropertyInfo, expectedMessageError);

				previousDocument.CSI_ReferenceNumber = GenerateReferenceNumberString(34);
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageErrorContaining("When In Transition Period and ReferenceNumber Length=34", referenceNumberPropertyInfo, expectedMessageError);

				previousDocument.CSI_ReferenceNumber = GenerateReferenceNumberString(35);
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageErrorContaining("When In Transition Period and ReferenceNumber Length=35", referenceNumberPropertyInfo, expectedMessageError);
			});

			RunTestUnderNonUcc6Configuration(() =>
			{
				previousDocument.CSI_ReferenceNumber = GenerateReferenceNumberString(38);
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageErrorContaining("When In Transition Period and ReferenceNumber Length=38", referenceNumberPropertyInfo, expectedMessageError);
			});
		}

		RunTestUnderNonUcc6Configuration(() =>
		{
			previousDocument.CSI_ReferenceNumber = GenerateReferenceNumberString(38);
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageErrorContaining("When No Transition Period and ReferenceNumber Length=38", referenceNumberPropertyInfo, expectedMessageError);
		});

		string GenerateReferenceNumberString(int requiredLength) => "Test1".PadLeft(requiredLength, 'A');
	}

	public void TestValidateNumberOfPreviousDocuments_InTransitionPeriod()
	{
		const string expectedMessageError = "During the transition period, which is active now, the maximum number of previous documents allowed is 9";
		using (TemporallySetTransitionPeriod(true))
		{
			RunTestUnderUcc6Configuration(() =>
			{
				ClearAndAddPreviousDocuments(10);
				var (validation, document) = CreateNewValidation();
				validation.ValidateNumberOfPreviousDocumentsAllowed();
				AssertHasRowMessageErrorContaining(document, expectedMessageError);

				ClearAndAddPreviousDocuments(9);
				(validation, document) = CreateNewValidation();
				validation.ValidateNumberOfPreviousDocumentsAllowed();
				AssertNoRowMessageErrorContaining(document, expectedMessageError);
			});

			RunTestUnderNonUcc6Configuration(() =>
			{
				ClearAndAddPreviousDocuments(12);
				var (validation, document) = CreateNewValidation();
				validation.ValidateNumberOfPreviousDocumentsAllowed();
				AssertNoRowMessageErrorContaining(document, expectedMessageError);
			});
		}
	}

	public void TestValidateNumberOfPreviousDocuments_InNonTransitionPeriod()
	{
		const string expectedMessageError = "The maximum number of previous documents allowed is 99";

		RunTestUnderUcc6Configuration(() =>
		{
			ClearAndAddPreviousDocuments(100);
			var (validation, document) = CreateNewValidation();
			validation.ValidateNumberOfPreviousDocumentsAllowed();
			AssertHasRowMessageErrorContaining(document, expectedMessageError);

			ClearAndAddPreviousDocuments(99);
			(validation, document) = CreateNewValidation();
			validation.ValidateNumberOfPreviousDocumentsAllowed();
			AssertNoRowMessageErrorContaining(document, expectedMessageError);
		});

		RunTestUnderNonUcc6Configuration(() =>
		{
			ClearAndAddPreviousDocuments(100);
			var (validation, document) = CreateNewValidation();
			validation.ValidateNumberOfPreviousDocumentsAllowed();
			AssertNoRowMessageErrorContaining(document, expectedMessageError);
		});
	}

	public void TestCSI_ProcedureNoValidationForUcc6Export()
	{
		var targetPropertyInfo = previousDocument.CSI_ProcedureInfo;
		previousDocument.CSI_Procedure = ZString.Empty;

		RunTestUnderUcc6Configuration(() =>
		{
			previousDocument.Validation.ValidateCSI_Procedure();
			AssertNoMessageErrorContaining("When CSI_Procedure is empty", targetPropertyInfo, MandatoryValidation.YouHaveNotEntered);
		});

		RunTestUnderNonUcc6Configuration(() =>
		{
			previousDocument.Validation.ValidateCSI_Procedure();
			AssertHasMessageErrorContaining("When CSI_Procedure is empty", targetPropertyInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCSI_SubTypeNoValidationForUcc6Export()
	{
		var targetPropertyInfo = previousDocument.CSI_SubTypeInfo;
		previousDocument.CSI_SubType = ZString.Empty;

		RunTestUnderUcc6Configuration(() =>
		{
			previousDocument.Validation.ValidateCSI_SubType();
			AssertNoMessageErrorContaining("When CSI_SubType is empty", targetPropertyInfo, MandatoryValidation.YouHaveNotEntered);
		});

		RunTestUnderNonUcc6Configuration(() =>
		{
			previousDocument.Validation.ValidateCSI_SubType();
			AssertHasMessageErrorContaining("When CSI_SubType is empty", targetPropertyInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCSI_UnitOfQuantity3NoValidationForUcc6Export()
	{
		var targetPropertyInfo = previousDocument.CSI_UnitOfQuantity3Info;
		previousDocument.CSI_UnitOfQuantity3 = ZString.Empty;

		RunTestUnderUcc6Configuration(() =>
		{
			previousDocument.Validation.ValidateCSI_UnitOfQuantity3();
			AssertNoMessageErrorContaining("When CSI_UnitOfQuantity3 is empty", targetPropertyInfo, MandatoryValidation.YouHaveNotEntered);
		});

		RunTestUnderNonUcc6Configuration(() =>
		{
			previousDocument.Validation.ValidateCSI_UnitOfQuantity3();
			AssertHasMessageErrorContaining("When CSI_UnitOfQuantity3 is empty", targetPropertyInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCSI_ReferenceNumber_ValidateFormat()
	{
		const string invalidFormatMessageError = "Number entered is in an invalid format.";
		var targetPropertyInfo = previousDocument.CSI_ReferenceNumberInfo;
		previousDocument.CSI_Code = "271";
		previousDocument.CSI_ReferenceNumber = "IT-2023-123";

		RunTestUnderUcc6Configuration(() =>
		{
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageErrorContaining("When CSI_ReferenceNumber is AA23D", targetPropertyInfo, invalidFormatMessageError);
		});

		RunTestUnderNonUcc6Configuration(() =>
		{
			previousDocument.CSI_Procedure = "AWB";
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageErrorContaining("When CSI_ReferenceNumber is AA23D", targetPropertyInfo, invalidFormatMessageError);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew();

		var invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_CL = entryLine.PK;

		previousDocument = invoiceLine.PreviousDocuments.AddNew();
		previousDocument.CSI_Code = "454";
		previousDocument.CSI_SubType = "Y";
		previousDocument.CSI_ReferenceNumber = "PREVDOC INVLINE";
	}

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	PreviousDocument previousDocument;

	(InvoiceLineExportPreviousDocumentValidation, PreviousDocument) CreateNewValidation()
	{
		var document = invoiceLine.PreviousDocuments.First() as PreviousDocument;
		var validation = new InvoiceLineExportPreviousDocumentValidation(document);
		return (validation, document);
	}

	IDisposable TemporallySetTransitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isActive);

	void RunTestUnderUcc6Configuration(Action testScenario)
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions($"When Declaration Type: {declaration.JE_MessageType}, UCC6", () => testScenario());
		}
	}

	void RunTestUnderNonUcc6Configuration(Action testScenario)
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			CombineAssertions($"When Declaration Type: {declaration.JE_MessageType}, Non-UCC6", () => testScenario());
		}
	}

	void AddPreviousDocuments(int requiredNumberOfDocuments)
	{
		for (int i = 1; i <= requiredNumberOfDocuments; i++)
		{
			var doc = invoiceLine.PreviousDocuments.AddNew();
			doc.CSI_Code = i.ToString();
			doc.CSI_SubType = "Y";
			doc.CSI_ReferenceNumber = $"INVLineRefNum-{i}";
		}
	}

	void ClearPreviousDocuments() => invoiceLine.PreviousDocuments.RemoveAndDeleteAll();

	void ClearAndAddPreviousDocuments(int requiredNumberOfDocuments)
	{
		ClearPreviousDocuments();
		AddPreviousDocuments(requiredNumberOfDocuments);
		declaration.ResetApportionedPreviousDocuments();
	}

	void SetUpRefData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package Types");
		helper.CreateCusCodeListWithAttribute(RefDataGroupingCodes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.UnitedNationsRecommendations);

		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGMG", "Kilogram Gross", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);

		Factory.Save();
	}
}

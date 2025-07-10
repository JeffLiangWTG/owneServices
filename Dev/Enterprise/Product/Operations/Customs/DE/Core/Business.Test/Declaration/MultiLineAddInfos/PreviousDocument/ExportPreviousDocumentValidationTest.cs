using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants.RefCusCodeList.Codes;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class ExportPreviousDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code_InvoiceHeader_Mandatory()
		{
			var previousDocument = invoiceHeader.PreviousDocuments.AddNew();
			Customs.Business.Testing.ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_CodeInfo);
		}

		public void TestCheckCSI_Code_InvoiceLine_Mandatory()
		{
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			Customs.Business.Testing.ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_CodeInfo);
		}

		public void TestCheckCSI_Code_Duplicated_InvoiceLine()
		{
			AssertCheckCSI_Code_Duplicated(invoiceLine.PreviousDocuments);
		}

		public void TestCheckCSI_Code_Duplicated_CusClassPartPivot()
		{
			var (pivot, _) = PreviousDocumentTest.CreatePreviousDocumentWithPivotParent(Factory);
			AssertCheckCSI_Code_Duplicated(pivot.PreviousDocuments);
		}

		public void TestCheckCSI_Code_InvoiceHeader_ListValidation()
		{
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header, UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference, UniversalReferenceConstants.RefCusCodeListAttributes.Value.No, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);

			var previousDocument = invoiceHeader.PreviousDocuments.AddNew();
			Customs.Business.Testing.ValidationTestHelper.AssertInvalidCodeMessageError(previousDocument.CSI_CodeInfo, "invalid", "DE01");
			Customs.Business.Testing.ValidationTestHelper.AssertInvalidCodeMessageError(previousDocument.CSI_CodeInfo, "de01", "DE01");
		}

		public void TestCheckCSI_Code_InvoiceLine_ListValidation()
		{
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference, UniversalReferenceConstants.RefCusCodeListAttributes.Value.No, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);

			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			Customs.Business.Testing.ValidationTestHelper.AssertInvalidCodeMessageError(previousDocument.CSI_CodeInfo, "invalid", "DE01");
		}

		public void TestCheckCSI_ReferenceNumber_InvoiceHeader_Mandatory()
		{
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header, UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference, UniversalReferenceConstants.RefCusCodeListAttributes.Value.No, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);

			var previousDocument = invoiceHeader.PreviousDocuments.AddNew();
			CombineAssertions(() =>
			{
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageErrorContaining("CSI_Code is empty", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				previousDocument.CSI_Code = "DE01";
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageErrorContaining("Has attribute 'Reference' but value is not 'R'", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				previousDocument.CSI_Code = "DE02";
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertHasMessageErrorContaining("Has attribute 'Reference' and value is 'R'", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCSI_ReferenceNumber_InvoiceLine_Mandatory()
		{
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference, UniversalReferenceConstants.RefCusCodeListAttributes.Value.No, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);

			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			CombineAssertions(() =>
			{
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageErrorContaining("CSI_Code is empty", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				previousDocument.CSI_Code = "DE01";
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageErrorContaining("Has attribute 'Reference' but value is not 'R'", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				previousDocument.CSI_Code = "DE02";
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertHasMessageErrorContaining("Has attribute 'Reference' and value is 'R'", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCSI_ReferenceNumber_Length_InvoiceLine()
		{
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			AssertCheckCSI_ReferenceNumber_Length(previousDocument);
		}

		public void TestCheckCSI_ReferenceNumber_Length_CusClassPartPivot()
		{
			var (_, previousDocument) = PreviousDocumentTest.CreatePreviousDocumentWithPivotParent(Factory);
			AssertCheckCSI_ReferenceNumber_Length(previousDocument);
		}

		public void TestCheckCSI_ReferenceNumber_InvoiceLine_N830()
		{
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			var info = previousDocument.CSI_ReferenceNumberInfo;

			CombineAssertions(() =>
			{
				previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N830;
				TestHelper.AssertMRNFormatValidated("When CSI_Code == N830", info);

				previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.D019;
				TestHelper.AssertMRNFormatNotValidated("When CSI_Code != N830", info);
			});
		}

		public void TestCheckCSI_ReferenceNumber_InvoiceHeader_N830()
		{
			var previousDocument = invoiceHeader.PreviousDocuments.AddNew();
			var info = previousDocument.CSI_ReferenceNumberInfo;

			CombineAssertions(() =>
			{
				previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N830;
				TestHelper.AssertMRNFormatValidated("When CSI_Code == N830", info);

				previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.D019;
				TestHelper.AssertMRNFormatNotValidated("When CSI_Code != N830", info);
			});
		}

		public void TestCheckCSI_ItemNumber_Mandatory_InvoiceLine()
		{
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			AssertCheckCSI_ItemNumber_Mandatory(previousDocument);
		}

		public void TestCheckCSI_ItemNumber_Mandatory_CusClassPartPivot()
		{
			var (_, previousDocument) = PreviousDocumentTest.CreatePreviousDocumentWithPivotParent(Factory);
			AssertCheckCSI_ItemNumber_Mandatory(previousDocument);
		}

		public void TestCheckCSI_UnitOfQuantity_Mandatory_InvoiceLine()
		{
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			AssertCheckCSI_UnitOfQuantity_Mandatory(previousDocument);
		}

		public void TestCheckCSI_UnitOfQuantity_Mandatory_CusClassPartPivot()
		{
			var (_, previousDocument) = PreviousDocumentTest.CreatePreviousDocumentWithPivotParent(Factory);
			AssertCheckCSI_UnitOfQuantity_Mandatory(previousDocument);
		}

		public void TestCheckCSI_UnitOfQuantity_InvoiceLine_ListValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC40E, "DC40E", Core.Constants.CountryCodes.Germany);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, "DC40E", "C651", "C651", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit, "MeasurementUnit");
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CUSUQ", Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "DTN", "DTN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit, "MeasurementUnit",
				UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC40E, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ);
			Factory.Save();

			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = "C651";
			Customs.Business.Testing.ValidationTestHelper.AssertInvalidCodeMessageError(previousDocument.CSI_UnitOfQuantityInfo, "XX", "DTN");
		}

		public void TestCheckCSI_Description_Mandatory_InvoiceLine()
		{
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			AssertCheckCSI_Description_Mandatory(previousDocument);
		}

		public void TestCheckCSI_Description_Mandatory_CusClassPartPivot()
		{
			var (_, previousDocument) = PreviousDocumentTest.CreatePreviousDocumentWithPivotParent(Factory);
			AssertCheckCSI_Description_Mandatory(previousDocument);
		}

		public void TestCheckCSI_Quantity_InvoiceLine_Mandatory()
		{
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit, UniversalReferenceConstants.RefCusCodeListAttributes.Value.No, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);

			var message = "Please enter a Quantity between 0.00001 and 99999999999.99999.";

			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining("CSI_Code is empty", previousDocument.CSI_QuantityInfo, message);

				previousDocument.CSI_Code = "DE02";
				previousDocument.Validation.ValidateCSI_Quantity();
				AssertHasMessageErrorContaining("Quantity is mandatory", previousDocument.CSI_QuantityInfo, message);

				previousDocument.CSI_Code = "DE01";
				previousDocument.Validation.ValidateCSI_Quantity();
				AssertNoMessageErrorContaining("Quantity is optional and Unit of Measure is empty", previousDocument.CSI_QuantityInfo, message);

				previousDocument.CSI_UnitOfQuantity = "XX";
				previousDocument.Validation.ValidateCSI_Quantity();
				AssertHasMessageErrorContaining("Quantity is optional and Unit of Measure isn't empty", previousDocument.CSI_QuantityInfo, message);
			});
		}

		public void TestCheckCSI_QuantityIsValidZDecimal_InvoiceLine()
		{
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			AssertCheckCSI_QuantityIsValidZDecimal(previousDocument);
		}

		public void TestCheckCSI_QuantityIsValidZDecimal_CusClassPartPivot()
		{
			var (_, previousDocument) = PreviousDocumentTest.CreatePreviousDocumentWithPivotParent(Factory);
			AssertCheckCSI_QuantityIsValidZDecimal(previousDocument);
		}

		public void TestCheckCSI_Quantity_Negative()
		{
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			CombineAssertions(() =>
			{
				previousDocument.CSI_Quantity = -1;
				AssertHasErrorContaining("Quantity is negative", previousDocument.CSI_QuantityInfo, MandatoryValidation.ValueCannotBeNegative);

				previousDocument.CSI_Quantity = 0;
				AssertNoErrorContaining("Quantity isn't negative", previousDocument.CSI_QuantityInfo, MandatoryValidation.ValueCannotBeNegative);
			});
		}

		public void TestCheckCSI_Quantity_InvoiceLine_Integer()
		{
			var message = "Quantity should be an integer value.";
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			CombineAssertions(() =>
			{
				previousDocument.CSI_Quantity = 0.1;
				previousDocument.Validation.ValidateCSI_Quantity();
				AssertNoMessageErrorContaining("CSI_UnitOfQuantity is empty", previousDocument.CSI_QuantityInfo, message);

				previousDocument.CSI_UnitOfQuantity = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems;
				previousDocument.Validation.ValidateCSI_Quantity();
				AssertHasMessageErrorContaining("CSI_UnitOfQuantity is 'NAR'", previousDocument.CSI_QuantityInfo, message);

				previousDocument.CSI_UnitOfQuantity = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfCells;
				previousDocument.Validation.ValidateCSI_Quantity();
				AssertHasMessageErrorContaining("CSI_UnitOfQuantity is 'NCL'", previousDocument.CSI_QuantityInfo, message);

				previousDocument.CSI_UnitOfQuantity = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfPairs;
				previousDocument.Validation.ValidateCSI_Quantity();
				AssertHasMessageErrorContaining("CSI_UnitOfQuantity is 'NPR'", previousDocument.CSI_QuantityInfo, message);
			});
		}

		public void TestCheckCSI_Code_N955_NotAllowed()
		{
			const string messageError = "For the selected Type (Time + Procedure) you may not enter the Previous Document Type N955.";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			var info = previousDocument.CSI_CodeInfo;

			CombineAssertions(() =>
			{
				entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._13;
				previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N830;
				AssertNoMessageError("CEI_SubStyle = 13, CSI_Code != N955", info, messageError);

				previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N955;
				AssertHasMessageError("CEI_SubStyle = 13, CSI_Code = N955", info, messageError);

				entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._12;
				previousDocument.Validation.ValidateCSI_Code();
				AssertNoMessageError("CEI_SubStyle != 13, CSI_Code = N955", info, messageError);
			});
		}

		public void TestCheckCSI_Code_N830_NotAllowed_CEI_SubStyle11() => AssertCSI_CodeN830NotAllowed(ExportDeclarationTypeTimeList.Codes._11);

		public void TestCheckCSI_Code_N830_NotAllowed_CEI_SubStyle12() => AssertCSI_CodeN830NotAllowed(ExportDeclarationTypeTimeList.Codes._12);

		public void TestCheckCSI_Code_CombinationOfMutuallyExclusive_NotAllowed_InvoiceLine()
		{
			AssertCheckCSI_Code_CombinationOfMutuallyExclusive_NotAllowed(invoiceLine.PreviousDocuments);
		}

		public void TestCheckCSI_Code_CombinationOfMutuallyExclusive_NotAllowed_CusClassPartPivot()
		{
			var (pivot, _) = PreviousDocumentTest.CreatePreviousDocumentWithPivotParent(Factory);
			AssertCheckCSI_Code_CombinationOfMutuallyExclusive_NotAllowed(pivot.PreviousDocuments);
		}

		void AssertCSI_CodeN830NotAllowed(string cei_SubStyle)
		{
			const string messageError = "For the selected Type (Time + Procedure) you may not enter the Previous Document Type N830.";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			var info = previousDocument.CSI_CodeInfo;

			CombineAssertions(() =>
			{
				entryInstruction.CEI_SubStyle = cei_SubStyle;
				previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N955;
				AssertNoMessageError($"CEI_SubStyle = {cei_SubStyle}, CSI_Code != N830", info, messageError);

				previousDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N830;
				AssertHasMessageError("CEI_SubStyle = {cei_SubStyle}, CSI_Code = N830", info, messageError);

				entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._13;
				previousDocument.Validation.ValidateCSI_Code();
				AssertNoMessageError("CEI_SubStyle = 13, CSI_Code = N830", info, messageError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;

		void AssertCheckCSI_Code_Duplicated(PreviousDocumentCollection previousDocuments)
		{
			CombineAssertions(() =>
			{
				AssertForCode(Code_C651, "A Previous Document of Type C651 has already been entered.");
				AssertForCode(Code_C658, "A Previous Document of Type C658 has already been entered.");
			});

			void AssertForCode(string code, string message)
			{
				var previousDocument = previousDocuments.AddNew();
				previousDocument.CSI_Code = code;
				var previousDocument2 = previousDocuments.AddNew();
				previousDocument2.CSI_Code = code;
				AssertHasMessageError($"For {code}: {code} type", previousDocument2.CSI_CodeInfo, message);
				previousDocument2.CSI_Code = "XX";
				AssertNoMessageError($"For {code}: Other type", previousDocument2.CSI_CodeInfo, message);
			}
		}

		void AssertCheckCSI_Code_CombinationOfMutuallyExclusive_NotAllowed(PreviousDocumentCollection previousDocuments)
		{
			var previousDocument = previousDocuments.AddNew();
			var previousDocument2 = previousDocuments.AddNew();
			var info2 = previousDocument2.CSI_CodeInfo;

			CombineAssertions(() =>
			{
				AssertForCodes(Code_9ZZX, Code_9ZZY, "The Previous Document Types 9ZZX and 9ZZY are mutually exclusive.");
				AssertForCodes(Code_C651, Code_C658, "The Previous Document Types C651 and C658 are mutually exclusive.");
			});

			void AssertForCodes(ZString code1, ZString code2, string message)
			{
				previousDocument.CSI_Code = code1;
				previousDocument2.CSI_Code = "NONE";
				AssertNoMessageError($"{code1} - NONE", info2, message);

				previousDocument2.CSI_Code = code2;
				AssertHasMessageError($"{code1} - {code2}", info2, message);
			}
		}

		void AssertCheckCSI_ReferenceNumber_Length(PreviousDocument previousDocument)
		{
			var message = "The C651 Reference must have 21 characters.";
			CombineAssertions(() =>
			{
				previousDocument.CSI_Code = Code_C651;
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageErrorContaining("empty", previousDocument.CSI_ReferenceNumberInfo, message);

				previousDocument.CSI_ReferenceNumber = "1234";
				AssertHasMessageErrorContaining("less charaters than 21", previousDocument.CSI_ReferenceNumberInfo, message);

				previousDocument.CSI_ReferenceNumber = "1234567890123456789012";
				AssertHasMessageErrorContaining("more charaters than 21", previousDocument.CSI_ReferenceNumberInfo, message);

				previousDocument.CSI_ReferenceNumber = "123456789012345678901";
				AssertNoMessageErrorContaining("21 charaters", previousDocument.CSI_ReferenceNumberInfo, message);
			});
		}

		void AssertCheckCSI_ItemNumber_Mandatory(PreviousDocument previousDocument)
		{
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, UniversalReferenceConstants.RefCusCodeListAttributes.Name.ItemNumber, UniversalReferenceConstants.RefCusCodeListAttributes.Value.No, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);

			CombineAssertions(() =>
			{
				previousDocument.Validation.ValidateCSI_ItemNumber();
				AssertNoMessageErrorContaining("CSI_Code is empty", previousDocument.CSI_ItemNumberInfo, MandatoryValidation.YouHaveNotEntered);

				previousDocument.CSI_Code = "DE01";
				previousDocument.Validation.ValidateCSI_ItemNumber();
				AssertNoMessageErrorContaining("Has attribute 'ItemNumber' but value isn't 'R'", previousDocument.CSI_ItemNumberInfo, MandatoryValidation.YouHaveNotEntered);

				previousDocument.CSI_Code = "DE02";
				previousDocument.Validation.ValidateCSI_ItemNumber();
				AssertHasMessageErrorContaining("Has attribute 'ItemNumber' and value is 'R'", previousDocument.CSI_ItemNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		void AssertCheckCSI_UnitOfQuantity_Mandatory(PreviousDocument previousDocument)
		{
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit, UniversalReferenceConstants.RefCusCodeListAttributes.Value.No, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);

			CombineAssertions(() =>
			{
				previousDocument.Validation.ValidateCSI_UnitOfQuantity();
				AssertNoMessageErrorContaining("CSI_Code is empty", previousDocument.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEntered);

				previousDocument.CSI_Code = "DE01";
				previousDocument.Validation.ValidateCSI_UnitOfQuantity();
				AssertNoMessageErrorContaining("Has attribute 'MeasurementUnit' but value isn't 'R'", previousDocument.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEntered);

				previousDocument.CSI_Code = "DE02";
				previousDocument.Validation.ValidateCSI_UnitOfQuantity();
				AssertHasMessageErrorContaining("Has attribute 'MeasurementUnit' and value is 'R'", previousDocument.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		void AssertCheckCSI_QuantityIsValidZDecimal(PreviousDocument previousDocument)
		{
			var message = "the maximum value allowed for Quantity is 99,999,999,999.99999.";

			CombineAssertions(() =>
			{
				previousDocument.CSI_Quantity = 100000000000M;
				AssertHasErrorContaining("Has error", previousDocument.CSI_QuantityInfo, message);

				previousDocument.CSI_Quantity = 99999999999M;
				AssertNoErrorContaining("No error", previousDocument.CSI_QuantityInfo, message);
			});
		}

		void AssertCheckCSI_Description_Mandatory(PreviousDocument previousDocument)
		{
			TestHelper.CreateDC40CodeTypeWithCusCode(Factory, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, UniversalReferenceConstants.RefCusCodeListAttributes.Name.Complement, UniversalReferenceConstants.RefCusCodeListAttributes.Value.No, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);

			CombineAssertions(() =>
			{
				previousDocument.Validation.ValidateCSI_Description();
				AssertNoMessageErrorContaining("CSI_Code is empty", previousDocument.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

				previousDocument.CSI_Code = "DE01";
				previousDocument.Validation.ValidateCSI_Description();
				AssertNoMessageErrorContaining("Has attribute 'Complement' but value isn't 'Y'", previousDocument.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

				previousDocument.CSI_Code = "DE02";
				previousDocument.Validation.ValidateCSI_Description();
				AssertHasMessageErrorContaining("Has attribute 'Complement' and value is 'Y'", previousDocument.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}
}

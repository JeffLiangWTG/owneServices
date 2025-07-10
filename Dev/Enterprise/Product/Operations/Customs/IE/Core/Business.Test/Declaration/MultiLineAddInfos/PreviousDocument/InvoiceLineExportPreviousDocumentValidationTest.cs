using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using RefCusCodeListTypesCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(InvoiceLineExportPreviousDocumentValidation))]
	sealed class InvoiceLineExportPreviousDocumentValidationTest : PreviousDocumentValidationAbstractTest
	{
		public void TestCheckPreviousDocumentCount()
		{
			var previousDocuments = InvoiceLine.PreviousDocuments;
			for (int i = 0; i < 9; i++)
			{
				previousDocuments.AddNew();
			}

			AssertEquals("Precondition", 10, previousDocuments.Count);

			var rowMessageError = "During the transition period, which is active now, the maximum number of previous documents allowed is 9.";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
			{
				previousDocument.Validation.ValidateAll();
				AssertHasRowMessageError("When ShouldCheckForTransitionPeriod is true and count > 9", previousDocument, rowMessageError);

				previousDocuments.RemoveAndDelete(previousDocuments[9]);
				previousDocument.Validation.ValidateAll();
				AssertNoRowMessageError("When count <= 9", previousDocument, rowMessageError);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, false))
			{
				previousDocuments.AddNew();
				AssertEquals("Precondition", 10, previousDocuments.Count);
				AssertNoRowMessageError("When post transition period & count > 9", previousDocument, rowMessageError);

				rowMessageError = "During post transition period, the maximum number of previous documents allowed is 99 per entry line";
				AssertNoRowMessageError("When post transition period & count <= 99", previousDocument, rowMessageError);

				for (int i = 0; i < 99; i++)
				{
					previousDocuments.AddNew();
				}
				previousDocument.Validation.ValidateAll();

				AssertHasRowMessageError("When ShouldCheckForTransitionPeriod is true and count > 99", previousDocument, rowMessageError);
			}
		}

		public void TestCheckCSI_PackQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package Types");
			helper.CreateCusCodeListWithAttribute(RefDataGroupingCodes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.UnitedNationsRecommendations);
			Factory.Save();

			const string expectedMessageError = "Number of Packages cannot be less than or equal to 0 when Type of Packages is entered.";
			var targetInfo = previousDocument.CSI_PackQtyInfo;

			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertValueCannotBeNegativeMessageError(targetInfo);

				previousDocument.CSI_PackType = "AB";
				previousDocument.CSI_PackQty = 0;
				previousDocument.Validation.ValidateCSI_PackQty();
				AssertHasMessageErrorContaining("When Package Type is AB (Non-Bulk) and Quantity is 0", targetInfo, expectedMessageError);

				previousDocument.CSI_PackQty = 1;
				previousDocument.Validation.ValidateCSI_PackQty();
				AssertNoMessageErrorContaining("When Package Type is AB (Non-Bulk) and Quantity is 1", targetInfo, expectedMessageError);

				previousDocument.CSI_PackType = "VQ";
				previousDocument.CSI_PackQty = 0;
				previousDocument.Validation.ValidateCSI_PackQty();
				AssertNoMessageErrorContaining("When Package Type is VQ (Bulk) and Quantity is 0", targetInfo, expectedMessageError);
			});
		}

		public void TestCheckCSI_PackType()
		{
			var dataHelper = new UniversalReferenceTestDataHelper(Factory);
			dataHelper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.UnitedNationsRecommendations);
			dataHelper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypesCodes.UnitedNationsPackageTypes, "United Nations Package Types");
			dataHelper.CreateNewOrGetExistingCusCodeList(RefDataGroupingCodes.UnitedNationsRecommendations, RefCusCodeListTypesCodes.UnitedNationsPackageTypes, "PB", "United Nations Package Type B", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var targetInfo = previousDocument.CSI_PackTypeInfo;

			CombineAssertions(() =>
			{
				previousDocument.CSI_PackQty = 0;
				previousDocument.CSI_PackType = ZString.Empty;
				AssertNoMessageErrors(targetInfo);

				previousDocument.CSI_PackQty = 123;
				previousDocument.Validation.ValidateCSI_PackType();
				AssertHasMessageError(targetInfo, "Type of Packages cannot be empty when Number of Packages is greater than 0.");

				previousDocument.CSI_PackQty = 123;
				previousDocument.CSI_PackType = "PB";
				AssertNoMessageErrors(targetInfo);

				previousDocument.CSI_PackQty = 123;
				previousDocument.CSI_PackType = "XX";
				AssertHasMessageError("ListValidation of CSI_UnitOfQuantity for InvoiceLine PreviousDocument", targetInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckCSI_Quantity()
		{
			var targetInfo = previousDocument.CSI_QuantityInfo;

			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertValueCannotBeNegativeMessageError(targetInfo);

				previousDocument.CSI_Quantity = 0;
				previousDocument.CSI_UnitOfQuantity = ZString.Empty;
				AssertNoMessageErrors(targetInfo);

				previousDocument.CSI_Quantity = 0;
				previousDocument.CSI_UnitOfQuantity = "UQB";
				previousDocument.Validation.ValidateCSI_Quantity();
				AssertHasMessageError(targetInfo, "Quantity cannot be less than or equal to 0 when Unit of Quantity is entered.");

				previousDocument.CSI_Quantity = 123;
				previousDocument.CSI_UnitOfQuantity = "UQB";
				AssertNoMessageErrors(targetInfo);
			});
		}

		public void TestCheckCSI_UnitOfQuantity()
		{
			var dataHelper = new UniversalReferenceTestDataHelper(Factory);
			dataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland);
			dataHelper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypesCodes.SupportingDocumentsUnitOfMeasure, "Supporting Documents Unit of Measure");
			dataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, RefCusCodeListTypesCodes.SupportingDocumentsUnitOfMeasure, "UQB", "Supporting Documents Unit of Measure B", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var targetInfo = previousDocument.CSI_UnitOfQuantityInfo;

			CombineAssertions(() =>
			{
				previousDocument.CSI_Quantity = 0;
				previousDocument.CSI_UnitOfQuantity = ZString.Empty;
				AssertNoMessageErrors(targetInfo);

				previousDocument.CSI_Quantity = 123;
				previousDocument.Validation.ValidateCSI_UnitOfQuantity();
				AssertHasMessageError(targetInfo, "Unit of Quantity cannot be empty when Quantity is greater than 0.");

				previousDocument.CSI_Quantity = 10;
				previousDocument.CSI_UnitOfQuantity = "UQB";
				AssertNoMessageErrors(targetInfo);
			});
		}

		protected override PreviousDocument SetupPreviousDocument() => jobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew().PreviousDocuments.AddNew();

		protected override string MessageType => IEJobMessageTypeList.Codes.Export;
		JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)previousDocument.Parent;
	}
}

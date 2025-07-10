using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class SupportingDocumentValidationTest : TestCaseWithFactory
	{
		public void TestCheckCSI_DateOfExpiry_Export_InvoiceLine_CannotBeEarlierThanIssuingDate()
		{
			const string messageEntryDateIssuingDateCheck = "The Date of Validity cannot be earlier than the Issuing Date.";

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			SupportingDocumentTestHelper.CreateRefCusCodeWithAttributeToEnsurePropertyEditable(Factory, RefCusCodeListAttributes.Name.ValidityDate, "EXP1", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection);
			supportingDocumentForLine.CSI_FullType = "EXP1";

			CombineAssertions(() =>
			{
				supportingDocumentForLine.CSI_DateOfIssue = new ZDateTime(2022, 10, 14);
				supportingDocumentForLine.CSI_DateOfExpiry = supportingDocumentForLine.CSI_DateOfIssue;
				AssertNoMessageError("CSI_DateOfExpiry same as CSI_DateOfIssue", supportingDocumentForLine.CSI_DateOfExpiryInfo, messageEntryDateIssuingDateCheck);

				supportingDocumentForLine.CSI_DateOfExpiry = supportingDocumentForLine.CSI_DateOfIssue.AddDays(1);
				AssertNoMessageError("CSI_DateOfExpiry after CSI_DateOfIssue", supportingDocumentForLine.CSI_DateOfExpiryInfo, messageEntryDateIssuingDateCheck);

				supportingDocumentForLine.CSI_DateOfExpiry = supportingDocumentForLine.CSI_DateOfIssue.AddDays(-1);
				AssertHasMessageError("CSI_DateOfExpiry before CSI_DateOfIssue", supportingDocumentForLine.CSI_DateOfExpiryInfo, messageEntryDateIssuingDateCheck);

				dec.JE_MessageType = MessageTypeList.Codes.Import;
				supportingDocumentForLine.Validation.ValidateCSI_DateOfExpiry();
				AssertNoMessageError("Import declaration", supportingDocumentForLine.CSI_DateOfExpiryInfo, messageEntryDateIssuingDateCheck);
			});
		}

		public void TestCheckCSI_DateOfExpiry_Export_InvoiceHeader_Mandatory()
		{
			SupportingDocumentValidationTestHelper.AssertRefCusCodePropertyMandatoryForExport(supportingDocumentForHeader, x => x.CSI_DateOfExpiryInfo, RefCusCodeListAttributes.Name.ValidityDate, dec);
		}

		public void TestCheckCSI_DateOfExpiry_Export_InvoiceLine_Mandatory()
		{
			SupportingDocumentValidationTestHelper.AssertRefCusCodePropertyMandatoryForExport(supportingDocumentForLine, x => x.CSI_DateOfExpiryInfo, RefCusCodeListAttributes.Name.ValidityDate, dec);
		}

		public void TestCheckCSI_DateOfExpiry_Import_InvoiceHeader_Optional()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(supportingDocumentForHeader.CSI_DateOfExpiryInfo);
		}

		public void TestCheckCSI_DateOfExpiry_Import_InvoiceLine_Optional()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(supportingDocumentForLine.CSI_DateOfExpiryInfo);
		}

		[TestDate(2022, 05, 16)]
		public void TestCheckCSI_DateOfExpiryIsValidZDateTimeRange_FutureYears()
		{
			const string message = @"The date '17-May-2027' is more than 5 years from now.";
			SupportingDocumentTestHelper.CreateRefCusCodeWithAttributeToEnsurePropertyEditable(Factory, RefCusCodeListAttributes.Name.ValidityDate, "EXP1", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection);

			CombineAssertions(() =>
			{
				dec.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
				supportingDocumentForHeader.CSI_FullType = "EXP1";
				supportingDocumentForHeader.CSI_DateOfExpiry = new ZDateTime(2027, 05, 17);
				AssertHasWarning("Export InvoiceHeader", supportingDocumentForHeader.CSI_DateOfExpiryInfo, message);

				dec.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				supportingDocumentForHeader.Validation.ValidateCSI_DateOfExpiry();
				AssertNoWarning("Import InvoiceHeader", supportingDocumentForHeader.CSI_DateOfExpiryInfo, message);

				supportingDocumentForLine.CSI_FullType = "EXP1";
				supportingDocumentForLine.CSI_DateOfExpiry = new ZDateTime(2027, 05, 17);
				AssertNoWarning("Import InvoiceLine", supportingDocumentForLine.CSI_DateOfExpiryInfo, message);

				dec.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
				supportingDocumentForLine.Validation.ValidateCSI_DateOfExpiry();
				AssertHasWarning("Export InvoiceLine", supportingDocumentForLine.CSI_DateOfExpiryInfo, message);

				supportingDocumentForLine.CSI_DateOfExpiry = ZDateTime.MaxSmallDateTimeValue;
				AssertHasWarning("MaximumFutureYears", supportingDocumentForLine.CSI_DateOfExpiryInfo, @"The date '06-Jun-2079' is more than 5 years from now.");
			});
		}

		public void TestCheckCSI_DateOfExpiry_Export_InvoiceHeader_CannotBeEarlierThanIssuingDate()
		{
			const string messageEntryDateIssuingDateCheck = "The Date of Validity cannot be earlier than the Issuing Date.";
			SupportingDocumentTestHelper.CreateRefCusCodeWithAttributeToEnsurePropertyEditable(Factory, RefCusCodeListAttributes.Name.ValidityDate, "EXP1", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection);

			dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			supportingDocumentForHeader.CSI_FullType = "EXP1";
			CombineAssertions(() =>
			{
				supportingDocumentForHeader.CSI_DateOfIssue = new ZDateTime(2022, 10, 14);
				supportingDocumentForHeader.CSI_DateOfExpiry = supportingDocumentForHeader.CSI_DateOfIssue;
				AssertNoMessageError("CSI_DateOfExpiry same as CSI_DateOfIssue", supportingDocumentForHeader.CSI_DateOfExpiryInfo, messageEntryDateIssuingDateCheck);

				supportingDocumentForHeader.CSI_DateOfExpiry = supportingDocumentForHeader.CSI_DateOfIssue.AddDays(1);
				AssertNoMessageError("CSI_DateOfExpiry after CSI_DateOfIssue", supportingDocumentForHeader.CSI_DateOfExpiryInfo, messageEntryDateIssuingDateCheck);

				supportingDocumentForHeader.CSI_DateOfExpiry = supportingDocumentForHeader.CSI_DateOfIssue.AddDays(-1);
				AssertHasMessageError("CSI_DateOfExpiry before CSI_DateOfIssue", supportingDocumentForHeader.CSI_DateOfExpiryInfo, messageEntryDateIssuingDateCheck);

				dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				supportingDocumentForHeader.Validation.ValidateCSI_DateOfExpiry();
				AssertNoMessageError("Import declaration", supportingDocumentForHeader.CSI_DateOfExpiryInfo, messageEntryDateIssuingDateCheck);
			});
		}

		public void TestCheckCSI_ReferenceNumber_Export_InvoiceLine_X001()
		{
			const string X001ReferenceLimitation = "For type X001 Reference must not have more than 18 characters.";

			dec.JE_MessageType = MessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				supportingDocumentForLine.CSI_Code = "X001";
				supportingDocumentForLine.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageError("CSI_ReferenceNumber empty", supportingDocumentForLine.CSI_ReferenceNumberInfo, X001ReferenceLimitation);

				supportingDocumentForLine.CSI_ReferenceNumber = "123456789012345678";
				AssertNoMessageError("CSI_ReferenceNumber 18 chars", supportingDocumentForLine.CSI_ReferenceNumberInfo, X001ReferenceLimitation);

				supportingDocumentForLine.CSI_ReferenceNumber = "1234567890123456789";
				AssertHasMessageError("CSI_ReferenceNumber > 18 chars", supportingDocumentForLine.CSI_ReferenceNumberInfo, X001ReferenceLimitation);

				dec.JE_MessageType = MessageTypeList.Codes.Import;
				supportingDocumentForLine.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageError("Import declaration", supportingDocumentForLine.CSI_ReferenceNumberInfo, X001ReferenceLimitation);
			});
		}

		public void TestCheckCSI_ReferenceNumber_Export_InvoiceHeader_Mandatory()
		{
			SupportingDocumentValidationTestHelper.AssertRefCusCodePropertyMandatoryForExport(supportingDocumentForHeader, x => x.CSI_ReferenceNumberInfo, RefCusCodeListAttributes.Name.Reference, dec, x => x.Validation.ValidateCSI_ReferenceNumber());
		}

		public void TestCheckCSI_ReferenceNumber_Export_InvoiceLine_Mandatory()
		{
			SupportingDocumentValidationTestHelper.AssertRefCusCodePropertyMandatoryForExport(supportingDocumentForLine, x => x.CSI_ReferenceNumberInfo, RefCusCodeListAttributes.Name.Reference, dec, x => x.Validation.ValidateCSI_ReferenceNumber());
		}

		public void TestCheckCSI_DateOfIssue_Import_InvoiceHeader_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(supportingDocumentForHeader.CSI_DateOfIssueInfo);
		}

		public void TestCheckCSI_DateOfIssue_Import_InvoiceLine_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(supportingDocumentForLine.CSI_DateOfIssueInfo);
		}

		public void TestCheckCSI_DateOfIssue_Export_InvoiceHeader_Mandatory()
		{
			SupportingDocumentValidationTestHelper.AssertRefCusCodePropertyMandatoryForExport(supportingDocumentForHeader, x => x.CSI_DateOfIssueInfo, RefCusCodeListAttributes.Name.IssuingDate, dec);
		}

		public void TestCheckCSI_DateOfIssue_Export_InvoiceLine_Mandatory()
		{
			SupportingDocumentValidationTestHelper.AssertRefCusCodePropertyMandatoryForExport(supportingDocumentForLine, x => x.CSI_DateOfIssueInfo, RefCusCodeListAttributes.Name.IssuingDate, dec);
		}

		public void TestCheckCSI_DateOfIssue_Export_InvoiceHeader_InTheFuture()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			SupportingDocumentTestHelper.CreateRefCusCodeWithAttributeToEnsurePropertyEditable(Factory, RefCusCodeListAttributes.Name.IssuingDate, "EXP1", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection);
			supportingDocumentForHeader.CSI_FullType = "EXP1";
			AssertFutureDateMessageError(supportingDocumentForHeader.CSI_DateOfIssueInfo);
		}

		public void TestCheckCSI_DateOfIssue_Export_InvoiceLine_InTheFuture()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			SupportingDocumentTestHelper.CreateRefCusCodeWithAttributeToEnsurePropertyEditable(Factory, RefCusCodeListAttributes.Name.IssuingDate, "EXP1", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection);
			supportingDocumentForLine.CSI_FullType = "EXP1";
			AssertFutureDateMessageError(supportingDocumentForLine.CSI_DateOfIssueInfo);
		}

		public void TestCheckCSI_DateOfIssue_Import_InvoiceHeader_InTheFuture()
		{
			AssertFutureDateMessageError(supportingDocumentForHeader.CSI_DateOfIssueInfo);
		}

		public void TestCheckCSI_DateOfIssue_Import_InvoiceLine_InTheFuture()
		{
			AssertFutureDateMessageError(supportingDocumentForLine.CSI_DateOfIssueInfo);
		}

		[TestDate(2022, 05, 16)]
		public void TestCheckCSI_DateOfIssueIsValidZDateTimeRange_PastYears()
		{
			const string message = @"The date '15-May-2012' is more than 10 years old.";
			SupportingDocumentTestHelper.CreateRefCusCodeWithAttributeToEnsurePropertyEditable(Factory, RefCusCodeListAttributes.Name.IssuingDate, "EXP1", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection);

			CombineAssertions(() =>
			{
				dec.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
				supportingDocumentForHeader.CSI_FullType = "EXP1";
				supportingDocumentForHeader.CSI_DateOfIssue = new ZDateTime(2012, 05, 15);
				AssertHasWarning("Export InvoiceHeader", supportingDocumentForHeader.CSI_DateOfIssueInfo, message);

				dec.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				supportingDocumentForHeader.Validation.ValidateCSI_DateOfIssue();
				AssertNoWarning("Import InvoiceHeader", supportingDocumentForHeader.CSI_DateOfIssueInfo, message);

				supportingDocumentForLine.CSI_FullType = "EXP1";
				supportingDocumentForLine.CSI_DateOfIssue = new ZDateTime(2012, 05, 15);
				AssertNoWarning("Import InvoiceLine", supportingDocumentForLine.CSI_DateOfIssueInfo, message);

				dec.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
				supportingDocumentForLine.Validation.ValidateCSI_DateOfIssue();
				AssertHasWarning("Export InvoiceLine", supportingDocumentForLine.CSI_DateOfIssueInfo, message);

				supportingDocumentForLine.CSI_DateOfIssue = ZDateTime.MinSmallDateTimeValue;
				AssertHasWarning("MaximumPastYears", supportingDocumentForLine.CSI_DateOfIssueInfo, @"The date '01-Jan-1900' is more than 10 years old.");
			});
		}

		public void TestCheckCSI_Status_List_InvoiceLine()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(supportingDocumentForLine.CSI_StatusInfo, "X", AvailabilityList.Codes.J);
		}

		public void TestCheckCSI_Status_Import_InvoiceLine_Mandatory()
		{
			_ = new SupportingDocumentTestHelper(Factory);

			CombineAssertions(() =>
			{
				supportingDocumentForLine.Validation.ValidateCSI_Status();
				AssertHasMessageErrorContaining("No Division", supportingDocumentForLine.CSI_StatusInfo, MandatoryValidation.YouHaveNotEntered);
				supportingDocumentForLine.CSI_Code = "9005";
				supportingDocumentForLine.Validation.ValidateCSI_Status();
				AssertNoMessageErrorContaining("Exemption Division", supportingDocumentForLine.CSI_StatusInfo, MandatoryValidation.YouHaveNotEntered);
				supportingDocumentForLine.CSI_Code = "9001";
				supportingDocumentForLine.Validation.ValidateCSI_Status();
				AssertHasMessageErrorContaining("None Exemption Division", supportingDocumentForLine.CSI_StatusInfo, MandatoryValidation.YouHaveNotEntered);
				supportingDocumentForLine.CSI_Status = AvailabilityList.Codes.J;
				AssertNoMessageErrorContaining("Status Entered", supportingDocumentForLine.CSI_StatusInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCSI_Validation_Export_InvoiceLine()
		{
			_ = new SupportingDocumentTestHelper(Factory);

			dec.JE_MessageType = MessageTypeList.Codes.Export;

			supportingDocumentForLine.CSI_FullType = "3LLA231";
			supportingDocumentForLine.CSI_ReferenceNumber = "";
			supportingDocumentForLine.CSI_Description = "";
			supportingDocumentForLine.CSI_ReferenceNumber2 = "";
			supportingDocumentForLine.CSI_DateOfIssue = ZDateTime.Empty;
			supportingDocumentForLine.CSI_DateOfExpiry = ZDateTime.Empty;
			supportingDocumentForLine.CSI_Value = ZDecimal.Zero;
			supportingDocumentForLine.CSI_UnitOfQuantity = "";

			supportingDocumentForLine.Validation.ValidateCSI_ReferenceNumber();
			supportingDocumentForLine.Validation.ValidateCSI_Description();
			supportingDocumentForLine.Validation.ValidateCSI_ReferenceNumber2();
			supportingDocumentForLine.Validation.ValidateCSI_DateOfIssue();
			supportingDocumentForLine.Validation.ValidateCSI_DateOfExpiry();
			supportingDocumentForLine.Validation.ValidateCSI_Value();
			supportingDocumentForLine.Validation.ValidateCSI_UnitOfQuantity();

			AssertHasMessageError(supportingDocumentForLine.CSI_ReferenceNumberInfo, "You have not entered a Reference."); // Reference-Y
			AssertNoNotifications(supportingDocumentForLine.CSI_DescriptionInfo); // Complement-doesn't exist
			AssertNoNotifications(supportingDocumentForLine.CSI_ReferenceNumber2Info); // Detail-N
			AssertHasMessageError(supportingDocumentForLine.CSI_DateOfIssueInfo, "You have not entered a Date of Issue."); // IssuingDate-Y
			AssertHasMessageError(supportingDocumentForLine.CSI_DateOfExpiryInfo, "You have not entered a Date of Expiry."); // ExpiryDate-Y
			AssertNoNotifications(supportingDocumentForLine.CSI_ValueInfo); // Value-doesn't exist
			AssertHasMessageErrorContaining("CSI_UnitOfQuantityInfo", supportingDocumentForLine.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEntered); // ComplementaryUnit-Y

			supportingDocumentForLine.CSI_ReferenceNumber = "Reference";
			supportingDocumentForLine.CSI_Description = "Complement";
			supportingDocumentForLine.CSI_ReferenceNumber2 = "LicenseDetail";
			supportingDocumentForLine.CSI_DateOfIssue = ZDateTime.Today;
			supportingDocumentForLine.CSI_DateOfExpiry = ZDateTime.Today;
			supportingDocumentForLine.CSI_Value = 3;
			supportingDocumentForLine.CSI_UnitOfQuantity = SupportingDocumentsCustomsUQList.Codes.Gramm;

			supportingDocumentForLine.Validation.ValidateCSI_ReferenceNumber();
			supportingDocumentForLine.Validation.ValidateCSI_Description();
			supportingDocumentForLine.Validation.ValidateCSI_ReferenceNumber2();
			supportingDocumentForLine.Validation.ValidateCSI_DateOfIssue();
			supportingDocumentForLine.Validation.ValidateCSI_DateOfExpiry();
			supportingDocumentForLine.Validation.ValidateCSI_Value();
			supportingDocumentForLine.Validation.ValidateCSI_UnitOfQuantity();

			CombineAssertions(() =>
			{
				AssertNoMessageError(supportingDocumentForLine.CSI_ReferenceNumberInfo, "You have not entered a Reference."); // Reference-Y
				AssertNoNotifications(supportingDocumentForLine.CSI_DescriptionInfo); // Complement-doesn't exist
				AssertNoNotifications(supportingDocumentForLine.CSI_ReferenceNumber2Info); // Detail-N
				AssertNoMessageError(supportingDocumentForLine.CSI_DateOfIssueInfo, "You have not entered a Date of Issue."); // IssuingDate-Y
				AssertNoMessageError(supportingDocumentForLine.CSI_DateOfExpiryInfo, "You have not entered a Date of Expiry."); // ExpiryDate-Y
				AssertNoNotifications(supportingDocumentForLine.CSI_ValueInfo); // Value-doesn't exist
				AssertNoNotifications(supportingDocumentForLine.CSI_UnitOfQuantityInfo); // Unit-doesn't exist
			});
		}

		public void TestCheckCSI_Validation_Export_InvoiceLine_Required()
		{
			var supportingDocumentTestHelper = new SupportingDocumentTestHelper(Factory);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusCodeListPK = supportingDocumentTestHelper.FullType_3LLA231_Attribute_Reference.ZZE_ZZD_CodeList;

			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCodeListPK, RefCusCodeListAttributes.Name.Complement, RefCusCodeListAttributes.Value.Yes);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCodeListPK, RefCusCodeListAttributes.Name.Value, RefCusCodeListAttributes.Value.Yes);
			supportingDocumentTestHelper.FullType_3LLA231_Attribute_Reference.ZZE_Value = RefCusCodeListAttributes.Value.Yes;
			supportingDocumentTestHelper.FullType_3LLA231_Attribute_Detail.ZZE_Value = RefCusCodeListAttributes.Value.Yes;
			supportingDocumentTestHelper.FullType_3LLA231_Attribute_IssuingDate.ZZE_Value = RefCusCodeListAttributes.Value.Yes;
			supportingDocumentTestHelper.FullType_3LLA231_Attribute_ValidityDate.ZZE_Value = RefCusCodeListAttributes.Value.Yes;
			Factory.Save();

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			supportingDocumentForLine.CSI_FullType = "3LLA231";
			supportingDocumentForLine.CSI_ReferenceNumber = "";
			supportingDocumentForLine.CSI_Description = "";
			supportingDocumentForLine.CSI_ReferenceNumber2 = "";
			supportingDocumentForLine.CSI_DateOfIssue = ZDateTime.Empty;
			supportingDocumentForLine.CSI_DateOfExpiry = ZDateTime.Empty;
			supportingDocumentForLine.CSI_Value = ZDecimal.Zero;
			supportingDocumentForLine.CSI_UnitOfQuantity = "";

			supportingDocumentForLine.Validation.ValidateCSI_ReferenceNumber();
			supportingDocumentForLine.Validation.ValidateCSI_Description();
			supportingDocumentForLine.Validation.ValidateCSI_ReferenceNumber2();
			supportingDocumentForLine.Validation.ValidateCSI_DateOfIssue();
			supportingDocumentForLine.Validation.ValidateCSI_DateOfExpiry();
			supportingDocumentForLine.Validation.ValidateCSI_Value();
			supportingDocumentForLine.Validation.ValidateCSI_UnitOfQuantity();

			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("CSI_ReferenceNumberInfo", supportingDocumentForLine.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining("CSI_DescriptionInfo", supportingDocumentForLine.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining("CSI_ReferenceNumber2Info", supportingDocumentForLine.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining("CSI_DateOfIssueInfo", supportingDocumentForLine.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining("CSI_DateOfExpiryInfo", supportingDocumentForLine.CSI_DateOfExpiryInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasWarningContaining("CSI_ValueInfo", supportingDocumentForLine.CSI_ValueInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining("CSI_UnitOfQuantityInfo", supportingDocumentForLine.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEntered);
			});
			SupportingDocumentValidationTestHelper.AssertRefCusCodePropertyMandatoryForExport(supportingDocumentForLine, x => x.CSI_UnitOfQuantityInfo, RefCusCodeListAttributes.Name.ComplementaryUnit, dec);
		}

		public void TestCheckCSI_Validation_Export_InvoiceLine_NotRequired()
		{
			var supportingDocumentTestHelper = new SupportingDocumentTestHelper(Factory);

			supportingDocumentTestHelper.FullType_3LLA231_Attribute_Reference.Delete();
			supportingDocumentTestHelper.FullType_3LLA231_Attribute_Detail.Delete();
			supportingDocumentTestHelper.FullType_3LLA231_Attribute_IssuingDate.Delete();
			supportingDocumentTestHelper.FullType_3LLA231_Attribute_ValidityDate.Delete();
			Factory.Save();

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			supportingDocumentForLine.CSI_FullType = "3LLA231";
			supportingDocumentForLine.CSI_ReferenceNumber = "Reference";
			supportingDocumentForLine.CSI_Description = "Complement";
			supportingDocumentForLine.CSI_ReferenceNumber2 = "LicenseDetail";
			supportingDocumentForLine.CSI_DateOfIssue = ZDateTime.Today;
			supportingDocumentForLine.CSI_DateOfExpiry = ZDateTime.Today;
			supportingDocumentForLine.CSI_Value = 3;
			supportingDocumentForLine.CSI_UnitOfQuantity = SupportingDocumentsCustomsUQList.Codes.Gramm;

			supportingDocumentForLine.Validation.ValidateCSI_ReferenceNumber();
			supportingDocumentForLine.Validation.ValidateCSI_Description();
			supportingDocumentForLine.Validation.ValidateCSI_ReferenceNumber2();
			supportingDocumentForLine.Validation.ValidateCSI_DateOfIssue();
			supportingDocumentForLine.Validation.ValidateCSI_DateOfExpiry();
			supportingDocumentForLine.Validation.ValidateCSI_Value();
			supportingDocumentForLine.Validation.ValidateCSI_UnitOfQuantity();

			CombineAssertions(() =>
			{
				AssertNoNotifications("CSI_DescriptionInfo", supportingDocumentForLine.CSI_DescriptionInfo);
				AssertNoNotifications("CSI_ReferenceNumber2Info", supportingDocumentForLine.CSI_ReferenceNumber2Info);
				AssertNoNotifications("CSI_DateOfIssueInfo", supportingDocumentForLine.CSI_DateOfIssueInfo);
				AssertNoNotifications("CSI_DateOfExpiryInfo", supportingDocumentForLine.CSI_DateOfExpiryInfo);
				AssertNoNotifications("CSI_ValueInfo", supportingDocumentForLine.CSI_ValueInfo);
				AssertNoNotifications("CSI_UnitOfQuantityInfo", supportingDocumentForLine.CSI_UnitOfQuantityInfo);
			});
		}

		public void TestCheckCSI_Validation_Export_InvoiceLine_Optional()
		{
			var supportingDocumentTestHelper = new SupportingDocumentTestHelper(Factory);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusCodeListPK = supportingDocumentTestHelper.FullType_3LLA231_Attribute_Reference.ZZE_ZZD_CodeList;

			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCodeListPK, RefCusCodeListAttributes.Name.Complement, RefCusCodeListAttributes.Value.No);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCodeListPK, RefCusCodeListAttributes.Name.Value, RefCusCodeListAttributes.Value.No);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCodeListPK, RefCusCodeListAttributes.Name.Unit, RefCusCodeListAttributes.Value.No);
			supportingDocumentTestHelper.FullType_3LLA231_Attribute_Reference.ZZE_Value = RefCusCodeListAttributes.Value.No;
			supportingDocumentTestHelper.FullType_3LLA231_Attribute_Detail.ZZE_Value = RefCusCodeListAttributes.Value.No;
			supportingDocumentTestHelper.FullType_3LLA231_Attribute_IssuingDate.ZZE_Value = RefCusCodeListAttributes.Value.No;
			supportingDocumentTestHelper.FullType_3LLA231_Attribute_ValidityDate.ZZE_Value = RefCusCodeListAttributes.Value.No;
			Factory.Save();

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			supportingDocumentForLine.CSI_FullType = "3LLA231";
			supportingDocumentForLine.CSI_ReferenceNumber = "Reference";
			supportingDocumentForLine.CSI_Description = "Complement";
			supportingDocumentForLine.CSI_ReferenceNumber2 = "LicenseDetail";
			supportingDocumentForLine.CSI_DateOfIssue = ZDateTime.Today;
			supportingDocumentForLine.CSI_DateOfExpiry = ZDateTime.Today;
			supportingDocumentForLine.CSI_Value = 3;
			supportingDocumentForLine.CSI_UnitOfQuantity = SupportingDocumentsCustomsUQList.Codes.Gramm;

			supportingDocumentForLine.Validation.ValidateCSI_ReferenceNumber();
			supportingDocumentForLine.Validation.ValidateCSI_Description();
			supportingDocumentForLine.Validation.ValidateCSI_ReferenceNumber2();
			supportingDocumentForLine.Validation.ValidateCSI_DateOfIssue();
			supportingDocumentForLine.Validation.ValidateCSI_DateOfExpiry();
			supportingDocumentForLine.Validation.ValidateCSI_Value();
			supportingDocumentForLine.Validation.ValidateCSI_UnitOfQuantity();

			CombineAssertions(() =>
			{
				AssertNoNotifications("CSI_DescriptionInfo", supportingDocumentForLine.CSI_DescriptionInfo);
				AssertNoNotifications("CSI_ReferenceNumber2Info", supportingDocumentForLine.CSI_ReferenceNumber2Info);
				AssertNoNotifications("CSI_DateOfIssueInfo", supportingDocumentForLine.CSI_DateOfIssueInfo);
				AssertNoNotifications("CSI_DateOfExpiryInfo", supportingDocumentForLine.CSI_DateOfExpiryInfo);
				AssertNoNotifications("CSI_ValueInfo", supportingDocumentForLine.CSI_ValueInfo);
				AssertNoNotifications("CSI_UnitOfQuantityInfo", supportingDocumentForLine.CSI_UnitOfQuantityInfo);
			});
		}

		public void TestCheckCSI_Quantity_Import_InvoiceLine()
		{
			const string QuantityNotEntered = "You have not entered a Quantity.";
			const string QuantityInteger = "Quantity should be an integer value.";
			const string NumberTooLarge = "The number 1,234,567,890 is too large, the maximum value allowed for Quantity is 999,999,999.999.";

			_ = new SupportingDocumentTestHelper(Factory);

			CombineAssertions(() =>
			{
				supportingDocumentForLine.CSI_Code = "9002";
				AssertNoMessageError(supportingDocumentForLine.CSI_QuantityInfo, QuantityInteger);
				supportingDocumentForLine.CSI_UnitOfQuantity = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems;
				supportingDocumentForLine.CSI_Quantity = 1.1m;
				AssertHasMessageError(supportingDocumentForLine.CSI_QuantityInfo, QuantityInteger);
				AssertNoMessageError(supportingDocumentForLine.CSI_QuantityInfo, QuantityNotEntered);

				supportingDocumentForLine.CSI_Quantity = 0m;
				AssertHasMessageError(supportingDocumentForLine.CSI_QuantityInfo, QuantityNotEntered);

				supportingDocumentForLine.CSI_Code = "2AAA";
				supportingDocumentForLine.Validation.ValidateCSI_Quantity();
				AssertNoMessageError(supportingDocumentForLine.CSI_QuantityInfo, QuantityNotEntered);

				supportingDocumentForLine.CSI_Quantity = 1234567890m;
				AssertHasError(supportingDocumentForLine.CSI_QuantityInfo, NumberTooLarge);

				supportingDocumentForLine.CSI_Quantity = 123456789m;
				AssertNoError(supportingDocumentForLine.CSI_QuantityInfo, NumberTooLarge);
			});
		}

		public void TestCheckCSI_Quantity_Export_InvoiceLine()
		{
			const string NumberTooLarge = "The number 12,345,678,901 is too large, the maximum value allowed for Quantity is 999,999,999.9999.";

			_ = new SupportingDocumentTestHelper(Factory);

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			SupportingDocumentTestHelper.CreateRefCusCodeWithAttributeToEnsurePropertyEditable(Factory, RefCusCodeListAttributes.Name.ComplementaryUnit, "EXP1", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection);

			SupportingDocumentValidationTestHelper.AssertRefCusCodePropertyMandatoryForExport(supportingDocumentForLine, x => x.CSI_QuantityInfo, RefCusCodeListAttributes.Name.MeasurementUnit, dec);
			SupportingDocumentValidationTestHelper.AssertRefCusCodePropertyMandatoryForExport(supportingDocumentForLine, x => x.CSI_QuantityInfo, RefCusCodeListAttributes.Name.ComplementaryUnit, dec);
			CombineAssertions(() =>
			{
				supportingDocumentForLine.CSI_Quantity = 12345678901m;
				AssertHasError(supportingDocumentForLine.CSI_QuantityInfo, NumberTooLarge);

				supportingDocumentForLine.CSI_Quantity = 1234567890m;
				AssertNoError(supportingDocumentForLine.CSI_QuantityInfo, NumberTooLarge);

				supportingDocumentForLine.CSI_FullType = "3LLA231";
				supportingDocumentForLine.CSI_UnitOfQuantity = SupportingDocumentsCustomsUQList.Codes.Gramm;
				supportingDocumentForLine.CSI_Quantity = 0;
				AssertHasMessageErrorContaining(supportingDocumentForLine.CSI_QuantityInfo, MandatoryValidation.YouHaveNotEntered);

				foreach (var csiUnitOfQuantity in new[] { SupportingDocumentsCustomsUQList.Codes.Diverse, SupportingDocumentsCustomsUQList.Codes.lautAnlage })
				{
					supportingDocumentForLine.CSI_UnitOfQuantity = csiUnitOfQuantity;
					supportingDocumentForLine.Validation.ValidateCSI_Quantity();
					AssertNoMessageErrorContaining(supportingDocumentForLine.CSI_QuantityInfo, MandatoryValidation.YouHaveNotEntered);
				}
			});
		}

		public void TestCheckCSI_Quantity_Export_InvoiceLine_MustBeInteger()
		{
			const string message = "Quantity should be an integer value.";

			dec.JE_MessageType = MessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				foreach (var unitOfQuantity2 in new[]
						 {
							 Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems,
							 Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfCells,
							 Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfPairs,
							 Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItemsPerFlask
						 })
				{
					supportingDocumentForLine.CSI_UnitOfQuantity2 = unitOfQuantity2;
					supportingDocumentForLine.CSI_Quantity = 1.1m;
					AssertHasMessageError($"CSI_UnitOfQuantity2 = '{unitOfQuantity2}', CSI_Quantity2 decimal", supportingDocumentForLine.CSI_QuantityInfo, message);

					supportingDocumentForLine.CSI_Quantity = 1;
					AssertNoMessageError($"CSI_UnitOfQuantity2 = '{unitOfQuantity2}', CSI_Quantity2 int", supportingDocumentForLine.CSI_QuantityInfo, message);
				}
			});
		}

		public void TestCheckCSI_UnitOfQuantity_Import_InvoiceLine()
		{
			supportingDocumentForLine.CSI_Code = "9002";
			supportingDocumentForLine.CSI_Quantity = 1.1m;
			supportingDocumentForLine.Validation.ValidateCSI_UnitOfQuantity();
			AssertHasMessageError(supportingDocumentForLine.CSI_UnitOfQuantityInfo, "You have not entered a Unit of Measure.");

			supportingDocumentForLine.CSI_UnitOfQuantity = "Test";
			AssertNoMessageError(supportingDocumentForLine.CSI_UnitOfQuantityInfo, "You have not entered a Unit of Measure.");

			supportingDocumentForLine.CSI_UnitOfQuantity = SupportingDocumentsCustomsUQList.Codes.Diverse;
			AssertNoMessageError(supportingDocumentForLine.CSI_UnitOfQuantityInfo, "Only the units of measurement \"l\" and \"St\" are allowed for this document type.");

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			supportingDocumentForLine.CSI_Code = "X001";
			supportingDocumentForLine.CSI_UnitOfQuantity = SupportingDocumentsCustomsUQList.Codes.Diverse;
			AssertHasMessageError(supportingDocumentForLine.CSI_UnitOfQuantityInfo, "Only the units of measurement \"l\" and \"St\" are allowed for this document type.");
		}

		public void TestCheckCSI_UnitOfQuantity_Export_CaseSensitiveListValidation()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			ValidationTestHelper.AssertInvalidCodeMessageError(supportingDocumentForLine.CSI_UnitOfQuantityInfo, "KG", "kg");
			ValidationTestHelper.AssertInvalidCodeMessageError(supportingDocumentForHeader.CSI_UnitOfQuantityInfo, "KG", "kg");
		}

		public void TestCheckCSI_UnitOfQuantity_Export_InvoiceHeader()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Export;

			supportingDocumentForLine.CSI_Code = "9002";
			supportingDocumentForLine.CSI_Quantity = 1.1m;
			supportingDocumentForLine.CSI_UnitOfQuantity = "Test";

			supportingDocumentForHeader.CSI_Code = "X001";
			supportingDocumentForHeader.CSI_UnitOfQuantity = SupportingDocumentsCustomsUQList.Codes.Diverse;
			AssertNoMessageError(supportingDocumentForHeader.CSI_UnitOfQuantityInfo, "Only the units of measurement \"l\" and \"St\" are allowed for this document type.");

			supportingDocumentForLine.CSI_Code = "X001";
			supportingDocumentForLine.CSI_UnitOfQuantity = SupportingDocumentsCustomsUQList.Codes.Diverse;
			AssertHasMessageError(supportingDocumentForLine.CSI_UnitOfQuantityInfo, "Only the units of measurement \"l\" and \"St\" are allowed for this document type.");
		}

		public void TestCheckCSI_UnitOfQuantity_Export_InvoiceLine()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Export;

			supportingDocumentForLine.CSI_Code = "9002";
			supportingDocumentForLine.CSI_Quantity = 1.1m;
			supportingDocumentForLine.CSI_UnitOfQuantity = "Test";

			CombineAssertions(() =>
			{
				supportingDocumentForLine.Validation.ValidateCSI_UnitOfQuantity();
				AssertHasMessageErrorContaining(supportingDocumentForLine.CSI_UnitOfQuantityInfo, ListValidation.InvalidCodeMessageError);

				supportingDocumentForLine.Validation.ValidateCSI_UnitOfQuantity();
				AssertNoMessageError(supportingDocumentForLine.CSI_UnitOfQuantityInfo, "Only the units of measurement \"l\" and \"St\" are allowed for this document type.");

				supportingDocumentForLine.CSI_Code = "X001";
				supportingDocumentForLine.CSI_UnitOfQuantity = SupportingDocumentsCustomsUQList.Codes.Diverse;
				AssertHasMessageError(supportingDocumentForLine.CSI_UnitOfQuantityInfo, "Only the units of measurement \"l\" and \"St\" are allowed for this document type.");

				supportingDocumentForLine.CSI_UnitOfQuantity = SupportingDocumentsCustomsUQList.Codes.Liter;
				AssertNoMessageError(supportingDocumentForLine.CSI_UnitOfQuantityInfo, "Only the units of measurement \"l\" and \"St\" are allowed for this document type.");

				supportingDocumentForLine.CSI_UnitOfQuantity = "";
				AssertNoMessageError(supportingDocumentForLine.CSI_UnitOfQuantityInfo, "Only the units of measurement \"l\" and \"St\" are allowed for this document type.");
			});
		}

		public void TestCheckCSI_FullType_Export_CaseSensitiveListValidation()
		{
			_ = new SupportingDocumentTestHelper(Factory);

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			ValidationTestHelper.AssertInvalidCodeMessageError(supportingDocumentForLine.CSI_FullTypeInfo, "3LLa231", "3LLA231");
			ValidationTestHelper.AssertInvalidCodeMessageError(supportingDocumentForHeader.CSI_FullTypeInfo, "3LLa232", "3LLA232");
		}

		public void TestCheckCSI_FullType_Export_InvoiceLine()
		{
			_ = new SupportingDocumentTestHelper(Factory);

			dec.JE_MessageType = MessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				supportingDocumentForLine.Validation.ValidateCSI_FullType();
				AssertHasMessageErrorContaining(supportingDocumentForLine.CSI_FullTypeInfo, MandatoryValidation.YouHaveNotEntered);
				supportingDocumentForLine.CSI_FullType = "1234";
				AssertHasMessageError(supportingDocumentForLine.CSI_FullTypeInfo, "The code you have selected is not in the list.");
				supportingDocumentForLine.CSI_FullType = "3LLA231";
				AssertNoMessageErrors(supportingDocumentForLine.CSI_FullTypeInfo);

				supportingDocumentForLine.CSI_FullType = string.Empty;
				supportingDocumentForLine.CSI_Code = "3LLA";
				AssertEquals("3LLA", supportingDocumentForLine.CSI_FullType);

				supportingDocumentForLine.CSI_FullType = string.Empty;
				supportingDocumentForLine.CSI_SubType = "231";
				AssertEquals("    231", supportingDocumentForLine.CSI_FullType);

				supportingDocumentForLine.CSI_FullType = string.Empty;
				supportingDocumentForLine.CSI_Code = "3LLA";
				supportingDocumentForLine.CSI_SubType = "231";
				AssertEquals("3LLA231", supportingDocumentForLine.CSI_FullType);

				supportingDocumentForLine.CSI_ReferenceNumber = "JASON";
				supportingDocumentForLine.Validation.ValidateCSI_FullType();
				AssertNoMessageErrorContaining(supportingDocumentForLine.CSI_FullTypeInfo, "already exists");

				var supportingDocument2 = invoiceLine.SupportingDocuments.AddNew();
				supportingDocument2.CSI_ReferenceNumber = "JASON";
				supportingDocument2.CSI_FullType = "3LLA231";
				AssertHasMessageErrorContaining(supportingDocument2.CSI_FullTypeInfo, "already exists");
			});
		}

		public void TestCheckCSI_FullType_Export_InvoiceLine_RelatedToSupplier()
		{
			const string errorMsg = "Supplier must have a Registration Number of Type 'EOR' stored in Organizations Registration Numbers/Codes if an Export License is used.";
			Enterprise.Customs.DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction = dec.CustomsEntryInstructions.AddNew();
			entryInstruction.ZG_PartyConstellation = "1111";
			invoiceLine.JI_CEI = entryInstruction.PK;
			var org = Factory.New<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, "Z123456789", Core.Constants.CountryCodes.Italy);
			invoiceHeader.JZ_OH_Supplier = org.PK;

			CombineAssertions(() =>
			{
				supportingDocumentForLine.CSI_FullType = "3LLA231";
				AssertHasMessageErrorContaining(supportingDocumentForLine.CSI_FullTypeInfo, errorMsg);

				supportingDocumentForLine.CSI_FullType = "111111";
				supportingDocumentForLine.Validation.ValidateCSI_FullType();
				AssertNoMessageErrorContaining(supportingDocumentForLine.CSI_FullTypeInfo, errorMsg);

				supportingDocumentForLine.CSI_FullType = "3LLA231";
				cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				supportingDocumentForLine.Validation.ValidateCSI_FullType();
				AssertNoMessageErrorContaining(supportingDocumentForLine.CSI_FullTypeInfo, errorMsg);

				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
				entryInstruction.ZG_PartyConstellation = "0111";
				supportingDocumentForLine.Validation.ValidateCSI_FullType();
				AssertNoMessageErrorContaining(supportingDocumentForLine.CSI_FullTypeInfo, errorMsg);
			});
		}

		public void TestCheckCSI_FullType_Export_InvoiceLine_C034()
		{
			const string message = "Supporting Document 'C034' requires an Additional Reference of Type 'Y015'.";
			dec.JE_MessageType = MessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				supportingDocumentForLine.CSI_FullType = "3LLA231";
				AssertNoMessageError("CSI_FullType <> 'C034'", supportingDocumentForLine.CSI_FullTypeInfo, message);

				supportingDocumentForLine.CSI_FullType = SupportingDocumentTypes.C034;
				AssertHasMessageError("CSI_FullType = 'C034'", supportingDocumentForLine.CSI_FullTypeInfo, message);

				var addInfo = invoiceLine.AdditionalInfos.AddNew();
				addInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
				addInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.Code_Y015;
				supportingDocumentForLine.Validation.ValidateCSI_FullType();
				AssertHasMessageError("AddInfo type <> 'REF'", supportingDocumentForLine.CSI_FullTypeInfo, message);

				addInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
				addInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.Code_C651;
				supportingDocumentForLine.Validation.ValidateCSI_FullType();
				AssertHasMessageError("AddInfo value <> 'Y015'", supportingDocumentForLine.CSI_FullTypeInfo, message);

				addInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.Code_Y015;
				supportingDocumentForLine.Validation.ValidateCSI_FullType();
				AssertNoMessageError("Has AddInfo 'REF' with value 'Y015'", supportingDocumentForLine.CSI_FullTypeInfo, message);
			});
		}

		public void TestCheckCSI_Code_Import_InvoiceLine()
		{
			_ = new SupportingDocumentTestHelper(Factory);

			CombineAssertions(() =>
			{
				supportingDocumentForLine.Validation.ValidateCSI_Code();
				AssertHasMessageErrorContaining(supportingDocumentForLine.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
				supportingDocumentForLine.CSI_Code = "Test";
				AssertHasMessageError(supportingDocumentForLine.CSI_CodeInfo, "The code you have selected is not in the list.");
				supportingDocumentForLine.CSI_Code = "9002";
				AssertNoMessageErrors(supportingDocumentForLine.CSI_CodeInfo);

				supportingDocumentForLine.CSI_ReferenceNumber = "JASON";
				AssertNoMessageErrorContaining(supportingDocumentForLine.CSI_CodeInfo, "already exists");

				var supportingDocument2 = invoiceLine.SupportingDocuments.AddNew();
				supportingDocument2.CSI_ReferenceNumber = "JASON";
				supportingDocument2.CSI_Code = "9002";
				AssertHasMessageErrorContaining(supportingDocument2.CSI_CodeInfo, "already exists");

				dec.JE_MessageType = MessageTypeList.Codes.Export;
				supportingDocumentForLine.Validation.ValidateCSI_Code();
				AssertNoMessageErrorContaining(supportingDocumentForLine.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
				supportingDocumentForLine.CSI_Code = "Test";
				AssertNoMessageError(supportingDocumentForLine.CSI_CodeInfo, "The code you have selected is not in the list.");
			});
		}

		public void TestCheckCSI_Value_Export_InvoiceLine_TransitionPeriodAES30()
		{
			const string message = "Amount should be an integer value.";
			const string rangeMessage = "the maximum value allowed for Value is 999,999,999.";
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			var entryInstruction = dec.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "000400";
			invoiceLine.JI_CEI = entryInstruction.PK;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				CombineAssertions(() =>
				{
					supportingDocumentForLine.CSI_Value = 1234567890m;
					AssertNoErrorContaining("When Style4thDigitIs4 more then 9 digits is allowed", supportingDocumentForLine.CSI_ValueInfo, rangeMessage);
					supportingDocumentForLine.CSI_Value = 1.11m;
					AssertNoMessageError("When Style4thDigitIs4, decimals are allowed", supportingDocumentForLine.CSI_ValueInfo, message);
					entryInstruction.CEI_Style = "000300";
					supportingDocumentForLine.Validation.ValidateCSI_Value();
					AssertHasMessageError("When Style4thDigit Is Not 4, decimals are not allowed", supportingDocumentForLine.CSI_ValueInfo, message);
					supportingDocumentForLine.CSI_Value = 1.00m;
					AssertNoMessageError("Integers in the form of a decimal are allowed", supportingDocumentForLine.CSI_ValueInfo, message);
					supportingDocumentForLine.CSI_Value = 1;
					AssertNoMessageError("Integers are allowed", supportingDocumentForLine.CSI_ValueInfo, message);
					supportingDocumentForLine.CSI_Value = 1234567890m;
					AssertHasErrorContaining("Limit Amount to 9 digits when Style4thDigit Is Not 4", supportingDocumentForLine.CSI_ValueInfo, rangeMessage);
				});
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				CombineAssertions(() =>
				{
					supportingDocumentForLine.CSI_Value = 1.11m;
					AssertNoMessageError("Outside Transition Period, decimals are allowed", supportingDocumentForLine.CSI_ValueInfo, message);
					supportingDocumentForLine.CSI_Value = 1234567890m;
					AssertNoErrorContaining("utside Transition Period more then 9 digits is allowed", supportingDocumentForLine.CSI_ValueInfo, rangeMessage);
				});
			}
		}

		public void TestCheckCSI_SubType_Import_InvoiceHeader()
		{
			supportingDocumentForHeader.Validation.ValidateCSI_SubType();
			AssertNoMessageErrors(supportingDocumentForHeader.CSI_SubTypeInfo);
		}

		public void TestCheckCSI_SubType_Export_InvoiceHeader()
		{
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			supportingDocumentForHeader.Validation.ValidateCSI_SubType();
			AssertNoMessageErrors(supportingDocumentForHeader.CSI_SubTypeInfo);
		}

		public void TestCheckCSI_Code_Import_InvoiceHeader_RequiringEndUseAuthorization()
		{
			const string errorMessage = "The Declarant must have an End Use Authorization (EUS) for this declaration.";
			var orgHeader = dec.DeclarantAddress.Header;
			orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, "DEACE111");

			CombineAssertions(() =>
			{
				supportingDocumentForHeader.CSI_Code = SupportingDocumentTypes.N990;
				AssertNoMessageError("Supporting doc for InvoiceHeader", supportingDocumentForHeader.CSI_CodeInfo, errorMessage);

				supportingDocumentForLine.CSI_Code = "9002";
				AssertNoMessageError("9002, No EUS Authorisation", supportingDocumentForLine.CSI_CodeInfo, errorMessage);

				foreach (var type in CusAuthorizationHelper.SupportingDocumentTypesRequiringEndOfUseAuthorisation)
				{
					supportingDocumentForLine.CSI_Code = type;
					AssertHasMessageError($"{type}, No EUS Authorisation", supportingDocumentForLine.CSI_CodeInfo, errorMessage);
				}

				orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EndUse, "DEEUS123");

				foreach (var type in CusAuthorizationHelper.SupportingDocumentTypesRequiringEndOfUseAuthorisation)
				{
					supportingDocumentForLine.CSI_Code = type;
					AssertNoMessageError($"{type}, has EUS Authorisation", supportingDocumentForLine.CSI_CodeInfo, errorMessage);
				}

				dec.JE_MessageType = MessageTypeList.Codes.Export;
				supportingDocumentForLine.CSI_Code = SupportingDocumentTypes.N990;
				AssertNoMessageError("Export declaration", supportingDocumentForLine.CSI_CodeInfo, errorMessage);
			});
		}

		public void TestCheckCSI_Code_Import_InvoiceLine_C626Requires9DFC()
		{
			const string message = "Code 'C626' requires also Code '9DFC' to be entered.";

			CombineAssertions(() =>
			{
				supportingDocumentForLine.CSI_Code = SupportingDocumentTypes.C626;
				AssertHasMessageError("No 9DFC", supportingDocumentForLine.CSI_CodeInfo, message);

				var doc = invoiceLine.SupportingDocuments.AddNew();
				doc.CSI_Code = SupportingDocumentTypes._9DFC;
				supportingDocumentForLine.Validation.ValidateCSI_Code();
				AssertNoMessageError("Has 9DFC", supportingDocumentForLine.CSI_CodeInfo, message);

				supportingDocumentForHeader.CSI_Code = SupportingDocumentTypes.C626;
				AssertNoMessageError("InvoiceHeader not validated", supportingDocumentForHeader.CSI_CodeInfo, message);
			});
		}

		public void TestCheckCSI_Code_Import_InvoiceLine_C627Requires9DFD()
		{
			const string message = "Code 'C627' requires also Code '9DFD' to be entered.";

			CombineAssertions(() =>
			{
				supportingDocumentForLine.CSI_Code = SupportingDocumentTypes.C627;
				AssertHasMessageError("No 9DFD", supportingDocumentForLine.CSI_CodeInfo, message);

				var doc = invoiceLine.SupportingDocuments.AddNew();
				doc.CSI_Code = SupportingDocumentTypes._9DFD;
				supportingDocumentForLine.Validation.ValidateCSI_Code();
				AssertNoMessageError("Has 9DFD", supportingDocumentForLine.CSI_CodeInfo, message);

				supportingDocumentForHeader.CSI_Code = SupportingDocumentTypes.C627;
				AssertNoMessageError("InvoiceHeader not validated", supportingDocumentForHeader.CSI_CodeInfo, message);
			});
		}

		public void TestCheckCSI_UnitOfQuantity2_Export_InvoiceLine()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "DC44E", Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributes.Name.MeasurementUnit, "MeasurementUnit",
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, Core.Constants.CountryCodes.Germany);

			var codeWithoutAttributeUOM = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, "DC44E", "EXP1", "EXP1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeWithoutAttributeUOM.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			var codeWithAttributeUOM = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, "DC44E", "3LLA231", "3LLA231", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeWithAttributeUOM.PK, RefCusCodeListAttributes.Name.MeasurementUnit, "MeasurementUnit");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeWithAttributeUOM.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CustomsUQ");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "MIL", "Kilograms", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KLT", "Kilograms", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGM", "Kilograms", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			supportingDocumentForLine.CSI_FullType = "3LLA231";
			ValidationTestHelper.AssertInvalidCodeMessageError(supportingDocumentForLine.CSI_UnitOfQuantity2Info, "XXX", "KLT");
			SupportingDocumentValidationTestHelper.AssertRefCusCodePropertyMandatoryForExport(supportingDocumentForLine, x => x.CSI_UnitOfQuantity2Info, RefCusCodeListAttributes.Name.MeasurementUnit, dec);
		}

		public void TestCheckCSI_UnitOfQuantity2_Import_InvoiceLine()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(supportingDocumentForLine.CSI_UnitOfQuantity2Info);
		}

		public void TestCheckCSI_RX_NKCurrency_Export_InvoiceLine()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CURRE, "Currency for Export");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CURRE, "TRY", "Türkische Lire", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			ValidationTestHelper.AssertInvalidCodeMessageError(supportingDocumentForLine.CSI_RX_NKCurrencyInfo, "AUD", "TRY", ListValidation.InvalidCodeMessage.ToString());
			SupportingDocumentValidationTestHelper.AssertRefCusCodePropertyMandatoryForExport(supportingDocumentForLine, x => x.CSI_RX_NKCurrencyInfo, RefCusCodeListAttributes.Name.Value, dec);
		}

		public void TestCheckCSI_RX_NKCurrency_Import_InvoiceLine()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(supportingDocumentForLine.CSI_RX_NKCurrencyInfo, "XYZ", "AUD");
			ValidationTestHelper.AssertFieldIsNotMandatory(supportingDocumentForLine.CSI_RX_NKCurrencyInfo);
		}

		public void TestCheckCSI_AdditionalDescription_Export_InvoiceHeader_Mandatory()
		{
			SupportingDocumentValidationTestHelper.AssertRefCusCodePropertyMandatoryForExport(supportingDocumentForHeader, x => x.CSI_AdditionalDescriptionInfo, RefCusCodeListAttributes.Name.Authority, dec);
		}

		public void TestCheckCSI_AdditionalDescription_Export_InvoiceLine_Mandatory()
		{
			SupportingDocumentValidationTestHelper.AssertRefCusCodePropertyMandatoryForExport(supportingDocumentForLine, x => x.CSI_AdditionalDescriptionInfo, RefCusCodeListAttributes.Name.Authority, dec);
		}

		public void TestCheckCSI_AdditionalDescription_Import_InvoiceHeader_Optional()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(supportingDocumentForHeader.CSI_AdditionalDescriptionInfo);
		}

		public void TestCheckCSI_AdditionalDescription_Import_InvoiceLine_Optional()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(supportingDocumentForLine.CSI_AdditionalDescriptionInfo);
		}

		public void TestCheckCSI_ItemNumber_Export_InvoiceHeader_Mandatory()
		{
			SupportingDocumentValidationTestHelper.AssertRefCusCodePropertyMandatoryForExport(supportingDocumentForHeader, x => x.CSI_ItemNumberInfo, RefCusCodeListAttributes.Name.ItemNumber, dec);
		}

		public void TestCheckCSI_ItemNumber_Export_InvoiceLine_Mandatory()
		{
			SupportingDocumentValidationTestHelper.AssertRefCusCodePropertyMandatoryForExport(supportingDocumentForLine, x => x.CSI_ItemNumberInfo, RefCusCodeListAttributes.Name.ItemNumber, dec);
		}

		public void TestCheckCSI_ItemNumber_Import_InvoiceHeader_Optional()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(supportingDocumentForHeader.CSI_ItemNumberInfo);
		}

		public void TestCheckCSI_ItemNumber_Import_InvoiceLine_Optional()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(supportingDocumentForLine.CSI_ItemNumberInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			dec = Factory.New<JobDeclaration>();
			invoiceHeader = dec.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			supportingDocumentForHeader = invoiceHeader.SupportingDocuments.AddNew();
			supportingDocumentForLine = invoiceLine.SupportingDocuments.AddNew();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			dec.JE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
		}

		JobDeclaration dec;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		SupportingDocument supportingDocumentForHeader;
		SupportingDocument supportingDocumentForLine;

		void AssertFutureDateMessageError(ZPropertyInfo propertyInfo)
		{
			const string messageError = "The Date of Issue can't be in the future.";

			CombineAssertions(() =>
			{
				propertyInfo.Value = ZDateTime.Empty;
				AssertNoMessageError("No Date of Issue", propertyInfo, messageError);
				propertyInfo.Value = ZDateTime.Today.AddDays(1);
				AssertHasMessageError("Future", propertyInfo, messageError);
				propertyInfo.Value = ZDateTime.Today;
				AssertNoMessageError("Today", propertyInfo, messageError);
			});
		}
	}

	sealed class SupportingDocumentValidationTestHelper : ValidationTestHelper
	{
		public static void AssertRefCusCodePropertyMandatoryForExport(SupportingDocument supportingDocument, Func<SupportingDocument, ZPropertyInfo> getPropertyInfo, string attributeName, JobDeclaration declaration, Action<SupportingDocument> getValidation = null)
		{
			var factory = supportingDocument.Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "DC44E Desc.");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(attributeName, attributeName, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, Core.Constants.CountryCodes.Germany);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "EXP1", "Has attribute but value is not 'Y'", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, attributeName, RefCusCodeListAttributes.Value.No);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "EXP2", "Has attribute with value 'Y'", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, attributeName, RefCusCodeListAttributes.Value.Yes);
			factory.Save();

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var propertyInfo = getPropertyInfo(supportingDocument);
			var propertyName = propertyInfo.Name;

			AssertionWithHtml.CombineAssertions(() =>
			{
				supportingDocument.CSI_FullType = "EXP1";
				SetEmptyValueAndValidateIfNecessary();
				TestCaseWithFactory.AssertNoMessageErrorContaining($"RefCusCode doesn't have attribute '{attributeName}' = 'Y' -> {propertyName} optional", propertyInfo, MandatoryValidation.YouHaveNotEntered);

				supportingDocument.CSI_FullType = "EXP2";
				SetEmptyValueAndValidateIfNecessary();
				TestCaseWithFactory.AssertHasMessageErrorContaining($"RefCusCode has attribute '{attributeName}' = 'Y' -> {propertyName} mandatory", propertyInfo, MandatoryValidation.YouHaveNotEntered);

				supportingDocument.CSI_FullType = ZString.Empty;
				SetEmptyValueAndValidateIfNecessary();
				TestCaseWithFactory.AssertNoMessageErrorContaining($"RefCusCode null -> {propertyName} optional", propertyInfo, MandatoryValidation.YouHaveNotEntered);
			});

			void SetEmptyValueAndValidateIfNecessary()
			{
				SetEmptyValue(propertyInfo);
				getValidation?.Invoke(supportingDocument);
			}
		}
	}
}

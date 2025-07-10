using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	class TaxInvoiceValidationTest  : TestCaseWithFactory
	{
		public void TestValidateTaxInvoice_EmptyBusinessTypeCodeAndInvoiceeID()
		{
			var errorMessage = "Invoice Debtor does not have a valid Korea Government VAT Code ('VAT'), Citizen Registration Number ('01') or Foreigner Registration Number ('03'). Please specify a value then re-queue the invoice.";

			AssertHavingErrorMessageOfAdditionalInfo("Should not have error when InvoiceeBusinessTypeCode and InvoiceeID are not empty.", false, errorMessage, new AdditionalInfo
			{
				InvoiceeBusinessTypeCode = ReadyKoreaConstants.InvoiceePartyBusinessTypeCode.RegistrationNumberOfBusinessOperator,
				InvoiceeID = "1234567890"
			});

			AssertHavingErrorMessageOfAdditionalInfo("Should not have error when only InvoiceeBusinessTypeCode is empty.", false, errorMessage, new AdditionalInfo
			{
				InvoiceeBusinessTypeCode = ZString.Empty,
				InvoiceeID = "1234567890"
			});

			AssertHavingErrorMessageOfAdditionalInfo("Should not have error when only InvoiceeID is empty.", false, errorMessage, new AdditionalInfo
			{
				InvoiceeBusinessTypeCode = ReadyKoreaConstants.InvoiceePartyBusinessTypeCode.RegistrationNumberOfBusinessOperator,
				InvoiceeID = ZString.Empty
			});

			AssertHavingErrorMessageOfAdditionalInfo("Should have error when InvoiceeBusinessTypeCode and InvoiceeID are both empty.", true, errorMessage, new AdditionalInfo
			{
				InvoiceeBusinessTypeCode = ZString.Empty,
				InvoiceeID = ZString.Empty
			});
		}

		public void TestValidateTaxInvoice_EmptyFullTypeCode()
		{
			var errorMessage = "Invalid Tax Invoice Document's Type Code. Please raise eRequest for assistance.";

			AssertHavingErrorMessageOfAdditionalInfo("Should not have error when FullTypeCode is not empty.", false, errorMessage, new AdditionalInfo
			{
				FullTypeCode = "0101"
			});

			AssertHavingErrorMessageOfAdditionalInfo("Should have error when FullTypeCode is empty.", true, errorMessage, new AdditionalInfo
			{
				FullTypeCode = ZString.Empty
			});
		}

		public void TestValidateTaxInvoice_InvalidAmendStatusCode()
		{
			var errorMessage = "The 'AmendStatusCode' element is invalid. Your value is 'AA'. The expected value is '01', '02', '03', '04', '05' or '06'.";

			AssertErrorMessageOfAmendStatusCode("Should have error when AmendStatusCode is invalid and FullTypeCode is started with '02' or '04'.", "AA", "0201", true, errorMessage);
			AssertErrorMessageOfAmendStatusCode("Should have error when AmendStatusCode is invalid and FullTypeCode is started with '02' or '04'.", "AA", "0401", true, errorMessage);
			AssertErrorMessageOfAmendStatusCode("Should not have error when AmendStatusCode is invalid and FullTypeCode is not started with '02' or '04'.", "AA", "0101", false, errorMessage);
			AssertErrorMessageOfAmendStatusCode("Should not have error when AmendStatusCode is valid and FullTypeCode is started with '02' or '04'.", "01", "0201", false, errorMessage);
			AssertErrorMessageOfAmendStatusCode("Should not have error when AmendStatusCode is valid and FullTypeCode is started with '02' or '04'.", "01", "0401", false, errorMessage);
			AssertErrorMessageOfAmendStatusCode("Should not have error when AmendStatusCode is valid and FullTypeCode is not started with '02' or '04'.", "01", "0101", false, errorMessage);
		}

		public void TestValidateTaxInvoice_MissingOriginalIssueID()
		{
			var errorMessage = "OriginalIssueID is mandatory for amendment transaction.";

			AssertErrorMessageOfOriginalIssueID("Should have error when OriginalIssueID is empty and FullTypeCode is started with '02' or '04'.", string.Empty, "0201", true, errorMessage);
			AssertErrorMessageOfOriginalIssueID("Should have error when OriginalIssueID is empty and FullTypeCode is started with '02' or '04'.", string.Empty, "0401", true, errorMessage);
			AssertErrorMessageOfOriginalIssueID("Should not have error when OriginalIssueID is empty and FullTypeCode is not started with '02' or '04'.", string.Empty, "0101", false, errorMessage);
			AssertErrorMessageOfOriginalIssueID("Should not have error when OriginalIssueID is not empty and FullTypeCode is started with '02' or '04'.", "0123456789", "0201", false, errorMessage);
			AssertErrorMessageOfOriginalIssueID("Should not have error when OriginalIssueID is not empty and FullTypeCode is started with '02' or '04'.", "0123456789", "0401", false, errorMessage);
			AssertErrorMessageOfOriginalIssueID("Should not have error when OriginalIssueID is not empty and FullTypeCode is not started with '02' or '04'.", "0123456789", "0101", false, errorMessage);
		}

		public void TestValidateTaxInvoice_MissingAmendStatusCode()
		{
			var errorMessage = "AmendStatusCode is mandatory for amendment transaction.";

			AssertErrorMessageOfAmendStatusCode("Should have error when AmendStatusCode is empty and FullTypeCode is started with '02' or '04'.", string.Empty, "0201", true, errorMessage);
			AssertErrorMessageOfAmendStatusCode("Should have error when AmendStatusCode is empty and FullTypeCode is started with '02' or '04'.", string.Empty, "0401", true, errorMessage);
			AssertErrorMessageOfAmendStatusCode("Should not have error when AmendStatusCode is empty and FullTypeCode is not started with '02' or '04'.", string.Empty, "0101", false, errorMessage);
			AssertErrorMessageOfAmendStatusCode("Should not have error when AmendStatusCode is not empty and FullTypeCode is started with '02' or '04'.", "01", "0201", false, errorMessage);
			AssertErrorMessageOfAmendStatusCode("Should not have error when AmendStatusCode is not empty and FullTypeCode is started with '02' or '04'.", "01", "0401", false, errorMessage);
			AssertErrorMessageOfAmendStatusCode("Should not have error when AmendStatusCode is not empty and FullTypeCode is not started with '02' or '04'.", "01", "0101", false, errorMessage);
		}

		void AssertErrorMessageOfAmendStatusCode(string comment, string amendStatusCode, string fullTypeCode, bool isExpectingError, string errorMessage)
		{
			var additionalInfo = new AdditionalInfo
			{
				AmendStatusCode = amendStatusCode,
				FullTypeCode = fullTypeCode
			};

			AssertHavingErrorMessageOfAdditionalInfo(comment, isExpectingError, errorMessage, additionalInfo);
		}

		void AssertErrorMessageOfOriginalIssueID(string comment, string originalIssueID, string fullTypeCode, bool isExpectingError, string errorMessage)
		{
			var additionalInfo = new AdditionalInfo
			{
				OriginalIssueID = originalIssueID,
				FullTypeCode = fullTypeCode
			};

			AssertHavingErrorMessageOfAdditionalInfo(comment, isExpectingError, errorMessage, additionalInfo);
		}

		void AssertHavingErrorMessageOfAdditionalInfo(string comment, bool isExpectingError, string errorMessage, AdditionalInfo additionalInfo)
		{
			if (isExpectingError)
			{
				AssertContains(comment, errorMessage, ValidateTaxInvoice(additionalInfo, GenValidTransactionInfo()).ToString());
			}
			else
			{
				AssertNotContains(comment, errorMessage, ValidateTaxInvoice(additionalInfo, GenValidTransactionInfo()).ToString());
			}
		}

		public void TestValidateTaxInvoice_InvoicerNameTextLength()
		{
			var errorMessage1 = "Company Name of the Current Login Company's organization proxy cannot left as empty.";
			var errorMessage2 = "Company Name of the Current Login Company's organization proxy exceeded the maximum length of 200 characters.";

			var case1 = ValidateTaxInvoice(new AdditionalInfo());
			AssertContains(errorMessage1, case1.ToString());

			var case2 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoicerNameText = "0123456789"
			});
			AssertNotContains(errorMessage1, case2.ToString());
			AssertNotContains(errorMessage2, case2.ToString());

			var case3 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoicerNameText = "012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"
			});
			AssertContains(errorMessage2, case3.ToString());
		}

		public void TestValidateTaxInvoice_InvoicerSpecifiedAddressLineOneTextLength()
		{
			var errorMessage = "Company Address of the Current Login Company's organization proxy exceeded the maximum length of 300 characters.";

			var case1 = ValidateTaxInvoice(new AdditionalInfo());
			AssertNotContains(errorMessage, case1.ToString());

			var case2 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoicerSpecifiedAddressLineOneText = "01234567890123456789"
			});
			AssertNotContains(errorMessage, case2.ToString());

			var case3 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoicerSpecifiedAddressLineOneText = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"
			});
			AssertContains(errorMessage, case3.ToString());
		}

		public void TestValidateTaxInvoice_InvoicerTypeCodeLength()
		{
			var errorMessage = "Korea Business Principal Activity Type (KBT) recorded against the Current Login Company's organization proxy exceeded the maximum length of 100 characters.";

			var case1 = ValidateTaxInvoice(new AdditionalInfo());
			AssertNotContains(errorMessage, case1.ToString());

			var case2 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoicerTypeCode = "01234567890123456789"
			});
			AssertNotContains(errorMessage, case2.ToString());

			var case3 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoicerTypeCode = "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"
			});
			AssertContains(errorMessage, case3.ToString());
		}

		public void TestValidateTaxInvoice_InvoicerClassificationCodeLength()
		{
			var errorMessage = "Korea Business Principal Industry Category (KBC) recorded against the Current Login Company's organization proxy exceeded the maximum length of 100 characters.";

			var case1 = ValidateTaxInvoice(new AdditionalInfo());
			AssertNotContains(errorMessage, case1.ToString());

			var case2 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoicerClassificationCode = "01234567890123456789"
			});
			AssertNotContains(errorMessage, case2.ToString());

			var case3 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoicerClassificationCode = "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"
			});
			AssertContains(errorMessage, case3.ToString());
		}

		public void TestValidateTaxInvoice_InvoicerDefinedContactPersonNameLength()
		{
			var errorMessage = "Contact Staff Name of the Current Login Company exceeded the maximum length of 100 characters.";

			var case1 = ValidateTaxInvoice(new AdditionalInfo());
			AssertNotContains(errorMessage, case1.ToString());

			var case2 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoicerDefinedContactPersonName = "01234567890123456789"
			});
			AssertNotContains(errorMessage, case2.ToString());

			var case3 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoicerDefinedContactPersonName = "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"
			});
			AssertContains(errorMessage, case3.ToString());
		}

		public void TestValidateTaxInvoice_InvoicerDefinedContactURICommunicationLength()
		{
			var errorMessage = "Contact Staff Email Address of the Current Login Company exceeded the maximum length of 100 characters.";

			var case1 = ValidateTaxInvoice(new AdditionalInfo());
			AssertNotContains(errorMessage, case1.ToString());

			var case2 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoicerDefinedContactURICommunication = "01234567890123456789"
			});
			AssertNotContains(errorMessage, case2.ToString());

			var case3 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoicerDefinedContactURICommunication = "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"
			});
			AssertContains(errorMessage, case3.ToString());
		}

		public void TestValidateTaxInvoice_InvoiceeNameTextLength()
		{
			var errorMessage1 = "Company Name of the Debtor Organization cannot be empty.";
			var errorMessage2 = "Company Name of the Debtor Organization exceeded the maximum length of 200 characters.";

			var case1 = ValidateTaxInvoice(new AdditionalInfo());
			AssertContains(errorMessage1, case1.ToString());

			var case2 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeNameText = "0123456789"
			});
			AssertNotContains(errorMessage1, case2.ToString());
			AssertNotContains(errorMessage2, case2.ToString());

			var case3 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeNameText = "012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"
			});
			AssertContains(errorMessage2, case3.ToString());
		}

		public void TestValidateTaxInvoice_InvoiceeSpecifiedAddressLineOneTextLength()
		{
			var errorMessage = "Company Address of the Debtor Organization exceeded the maximum length of 300 characters.";

			var case1 = ValidateTaxInvoice(new AdditionalInfo());
			AssertNotContains(errorMessage, case1.ToString());

			var case2 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeSpecifiedAddressLineOneText = "01234567890123456789"
			});
			AssertNotContains(errorMessage, case2.ToString());

			var case3 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeSpecifiedAddressLineOneText = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"
			});
			AssertContains(errorMessage, case3.ToString());
		}

		public void TestValidateTaxInvoice_InvoiceeTypeCodeLength()
		{
			var errorMessage = "Korea Business Principal Activity Type (KBT) recorded against the Debtor Organization exceeded the maximum length of 100 characters.";

			var case1 = ValidateTaxInvoice(new AdditionalInfo());
			AssertNotContains(errorMessage, case1.ToString());

			var case2 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeTypeCode = "01234567890123456789"
			});
			AssertNotContains(errorMessage, case2.ToString());

			var case3 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeTypeCode = "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"
			});
			AssertContains(errorMessage, case3.ToString());
		}

		public void TestValidateTaxInvoice_InvoiceeClassificationCodeLength()
		{
			var errorMessage = "Korea Business Principal Industry Category (KBC) recorded against the Debtor Organization exceeded the maximum length of 100 characters.";

			var case1 = ValidateTaxInvoice(new AdditionalInfo());
			AssertNotContains(errorMessage, case1.ToString());

			var case2 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeClassificationCode = "01234567890123456789"
			});
			AssertNotContains(errorMessage, case2.ToString());

			var case3 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeClassificationCode = "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"
			});
			AssertContains(errorMessage, case3.ToString());
		}

		public void TestValidateTaxInvoice_InvoiceePrimaryDefinedContactPersonNameLength()
		{
			var errorMessage = "Contact Person Name of the Debtor Organization exceeded the maximum length of 100 characters.";

			var case1 = ValidateTaxInvoice(new AdditionalInfo());
			AssertNotContains("No chars.", errorMessage, case1.ToString());

			var case2 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceePrimaryDefinedContactPersonName = "01234567890123456789"
			});
			AssertNotContains("20 chars.", errorMessage, case2.ToString());

			var case3 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceePrimaryDefinedContactPersonName = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789"
			});
			AssertNotContains("100 chars.", errorMessage, case3.ToString());

			var case4 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceePrimaryDefinedContactPersonName = "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"
			});
			AssertContains("101 chars. Exceed length limit.", errorMessage, case4.ToString());
		}

		public void TestValidateTaxInvoice_InvoiceePrimaryDefinedContactURICommunicationLength()
		{
			var errorMessage = "Contact Person Email Address of the Debtor Organization exceeded the maximum length of 100 characters.";

			var case1 = ValidateTaxInvoice(new AdditionalInfo());
			AssertNotContains("No chars.", errorMessage, case1.ToString());

			var case2 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceePrimaryDefinedContactURICommunication = "01234567890123456789"
			});
			AssertNotContains("20 chars.", errorMessage, case2.ToString());

			var case3 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceePrimaryDefinedContactURICommunication = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789"
			});
			AssertNotContains("100 chars.", errorMessage, case3.ToString());

			var case4 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceePrimaryDefinedContactURICommunication = "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"
			});
			AssertContains("101 chars. Exceed length limit.", errorMessage, case4.ToString());
		}

		public void TestValidateTaxInvoice_InvoiceeSecondaryDefinedContactPersonNameLength()
		{
			var errorMessage = "Contact Person Name of the Debtor Organization exceeded the maximum length of 100 characters.";

			var case1 = ValidateTaxInvoice(new AdditionalInfo());
			AssertNotContains("No chars.", errorMessage, case1.ToString());

			var case2 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeSecondaryDefinedContactPersonName = "01234567890123456789"
			});
			AssertNotContains("20 chars.", errorMessage, case2.ToString());

			var case3 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeSecondaryDefinedContactPersonName = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789"
			});
			AssertNotContains("100 chars.", errorMessage, case3.ToString());

			var case4 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeSecondaryDefinedContactPersonName = "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"
			});
			AssertContains("101 chars. Exceed length limit.", errorMessage, case4.ToString());
		}

		public void TestValidateTaxInvoice_InvoiceeSecondaryDefinedContactURICommunicationLength()
		{
			var errorMessage = "Contact Person Email Address of the Debtor Organization exceeded the maximum length of 100 characters.";

			var case1 = ValidateTaxInvoice(new AdditionalInfo());
			AssertNotContains("No chars.", errorMessage, case1.ToString());

			var case2 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeSecondaryDefinedContactURICommunication = "01234567890123456789"
			});
			AssertNotContains("20 chars.", errorMessage, case2.ToString());

			var case3 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeSecondaryDefinedContactURICommunication = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789"
			});
			AssertNotContains("100 chars.", errorMessage, case3.ToString());

			var case4 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeSecondaryDefinedContactURICommunication = "01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"
			});
			AssertContains("101 chars. Exceed length limit.", errorMessage, case4.ToString());
		}

		public void TestValidateTaxInvoice_InvoiceeIDLength_BusinessOperator()
		{
			var errorMessage = "Invalid KR VAT registration number recorded against the Debtor Organization. It should be in format 'NNNNNNNNNN'.";

			var logger_ErrorCase1 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeBusinessTypeCode = ReadyKoreaConstants.InvoiceePartyBusinessTypeCode.RegistrationNumberOfBusinessOperator
			});
			AssertContains(errorMessage, logger_ErrorCase1.ToString());

			var logger_ErrorCase2 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeBusinessTypeCode = ReadyKoreaConstants.InvoiceePartyBusinessTypeCode.RegistrationNumberOfBusinessOperator,
				InvoiceeID = "01234567890"
			});
			AssertContains(errorMessage, logger_ErrorCase2.ToString());

			var logger_NoErrorCase = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeBusinessTypeCode = ReadyKoreaConstants.InvoiceePartyBusinessTypeCode.RegistrationNumberOfBusinessOperator,
				InvoiceeID = "0123456789"
			});
			AssertNotContains(errorMessage, logger_NoErrorCase.ToString());
		}

		public void TestValidateTaxInvoice_InvoiceeIDLength_Resident()
		{
			var errorMessage = "Invalid KR Citizen Registration Number (Code:01) of the Debtor Organization. It should be in format 'NNNNNNNNNNNNN'.";

			var logger_ErrorCase1 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeBusinessTypeCode = ReadyKoreaConstants.InvoiceePartyBusinessTypeCode.RegistrationNumberOfResident,
			});
			AssertContains(errorMessage, logger_ErrorCase1.ToString());

			var logger_ErrorCase2 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeBusinessTypeCode = ReadyKoreaConstants.InvoiceePartyBusinessTypeCode.RegistrationNumberOfResident,
				InvoiceeID = "01234567890123"
			});
			AssertContains(errorMessage, logger_ErrorCase2.ToString());

			var logger_NoErrorCase = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeBusinessTypeCode = ReadyKoreaConstants.InvoiceePartyBusinessTypeCode.RegistrationNumberOfResident,
				InvoiceeID = "0123456789012"
			});
			AssertNotContains(errorMessage, logger_NoErrorCase.ToString());
		}

		public void TestValidateTaxInvoice_InvoicerIDLength()
		{
			var errorMessage = "Invalid KR VAT registration number recorded against the Company Organization Proxy. It should be in format 'NNNNNNNNNN'.";

			var logger_ErrorCase1 = ValidateTaxInvoice(new AdditionalInfo());
			AssertContains(errorMessage, logger_ErrorCase1.ToString());

			var logger_ErrorCase2 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoicerID = "01234567890"
			});
			AssertContains(errorMessage, logger_ErrorCase2.ToString());

			var logger_NoErrorCase = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoicerID = "0123456789"
			});
			AssertNotContains(errorMessage, logger_NoErrorCase.ToString());
		}

		public void TestValidateTaxInvoice_InvoicerTaxRegistrationIDLength()
		{
			var errorMessage = "Invalid KR Office ID (Code:08) recorded against the Company Organization Proxy. It should be in format 'NNNN'.";

			var case1 = ValidateTaxInvoice(new AdditionalInfo());
			AssertNotContains(errorMessage, case1.ToString());

			var case2 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoicerTaxRegistrationID = "01"
			});
			AssertContains(errorMessage, case2.ToString());

			var case3 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoicerTaxRegistrationID = "0123"
			});
			AssertNotContains(errorMessage, case3.ToString());
		}

		public void TestValidateTaxInvoice_InvoiceeTaxRegistrationIDLength()
		{
			var errorMessage = "Invalid KR Office ID (Code:08) recorded against the Debtor Organization. It should be in format 'NNNN'.";

			var case1 = ValidateTaxInvoice(new AdditionalInfo());
			AssertNotContains(errorMessage, case1.ToString());

			var case2 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeTaxRegistrationID = "01"
			});
			AssertContains(errorMessage, case2.ToString());

			var case3 = ValidateTaxInvoice(new AdditionalInfo
			{
				InvoiceeTaxRegistrationID = "0123"
			});
			AssertNotContains(errorMessage, case3.ToString());
		}

		public void TestValidateEmptySpecifiedPersonName()
		{
			var expectedMsgForInvoicee = "Please add a contact against the Debtor Organization with 'KRC - Korea Company Representative' allocated contact type.";
			var expectedMsgForInvoicer = "Please add a contact against the Company Organization Proxy with 'KRC - Korea Company Representative' allocated contact type.";

			Logger logger;
			var additionalInfo = new AdditionalInfo();

			logger = ValidateTaxInvoice(additionalInfo);
			AssertContains(expectedMsgForInvoicee, logger.ToString());
			AssertContains(expectedMsgForInvoicer, logger.ToString());

			additionalInfo.InvoiceeSpecifiedPersonNameText = "Test1";
			logger = ValidateTaxInvoice(additionalInfo);
			AssertNotContains(expectedMsgForInvoicee, logger.ToString());
			AssertContains(expectedMsgForInvoicer, logger.ToString());

			additionalInfo.InvoiceeSpecifiedPersonNameText = string.Empty;
			additionalInfo.InvoicerSpecifiedPersonNameText = "Test2";
			logger = ValidateTaxInvoice(additionalInfo);
			AssertContains(expectedMsgForInvoicee, logger.ToString());
			AssertNotContains(expectedMsgForInvoicer, logger.ToString());

			additionalInfo.InvoiceeSpecifiedPersonNameText = "Test1";
			additionalInfo.InvoicerSpecifiedPersonNameText = "Test2";
			logger = ValidateTaxInvoice(additionalInfo);
			AssertNotContains(expectedMsgForInvoicee, logger.ToString());
			AssertNotContains(expectedMsgForInvoicer, logger.ToString());
		}

		[TestDate(2022, 11, 11)]
		public void TestValidateTaxInvoice_TransactionDateIsNotFutureDate()
		{
			var errorMessageInvoiceDateIsNotFutureDate = "The invoice date cannot be a future date.";

			var additionalInfo = new AdditionalInfo();
			var transactionInfo1 = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionDate = new ZDateTime(2022, 03, 04),
			};
			var loggerCase1 = ValidateTaxInvoice(additionalInfo, transactionInfo1);
			AssertNotContains("InvoiceDate < Today", errorMessageInvoiceDateIsNotFutureDate, loggerCase1.ToString());

			var transactionInfo2 = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionDate = new ZDateTime(2022, 11, 11),
			};
			var loggerCase2 = ValidateTaxInvoice(additionalInfo, transactionInfo2);
			AssertNotContains("InvoiceDate = Today", errorMessageInvoiceDateIsNotFutureDate, loggerCase2.ToString());

			var transactionInfo3 = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionDate = new ZDateTime(2022, 11, 12),
			};
			var loggerCase3 = ValidateTaxInvoice(additionalInfo, transactionInfo3);
			AssertContains("InvoiceDate > Today", errorMessageInvoiceDateIsNotFutureDate, loggerCase3.ToString());
		}

		public void TestValidateTaxInvoice_TransactionDateMonthIsSameAsReverseDateMonth()
		{
			var errorMessageInvoiceDateMonthIsSameAsReverseDateMonth = "The month of the invoice date and the revenue recognition date must be the same.";

			var additionalInfo1 = new AdditionalInfo
			{
				Lines = new List<TaxInvoiceTradeLineItem>()
				{
					new TaxInvoiceTradeLineItem()
					{
						ReverseDate = new ZDateTime(2022,03,04)
					},
					new TaxInvoiceTradeLineItem()
					{
						ReverseDate = new ZDateTime(2022,03,05)
					},
					new TaxInvoiceTradeLineItem()
					{
						ReverseDate = new ZDateTime(2022,03,06)
					},
				}
			};
			var transactionInfo1 = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionDate = new ZDateTime(2022, 03, 04),
			};
			var loggerCase1 = ValidateTaxInvoice(additionalInfo1, transactionInfo1);
			AssertNotContains("All in the same month", errorMessageInvoiceDateMonthIsSameAsReverseDateMonth, loggerCase1.ToString());

			var transactionInfo2 = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionDate = new ZDateTime(2022, 06, 04),
			};
			var loggerCase2 = ValidateTaxInvoice(additionalInfo1, transactionInfo2);
			AssertContains("All not in the same month", errorMessageInvoiceDateMonthIsSameAsReverseDateMonth, loggerCase2.ToString());

			var additionalInfo2 = new AdditionalInfo
			{
				Lines = new List<TaxInvoiceTradeLineItem>()
				{
					new TaxInvoiceTradeLineItem()
					{
						ReverseDate = new ZDateTime(2022,03,04)
					},
					new TaxInvoiceTradeLineItem()
					{
						ReverseDate = new ZDateTime(2022,04,05)
					},
					new TaxInvoiceTradeLineItem()
					{
						ReverseDate = new ZDateTime(2022,05,06)
					},
				}
			};

			var loggerCase3 = ValidateTaxInvoice(additionalInfo2, transactionInfo1);
			AssertContains("Exist one line in the same month", errorMessageInvoiceDateMonthIsSameAsReverseDateMonth, loggerCase3.ToString());
		}

		public void TestValidateTaxInvoice_ReverseDateMonthIsNotEmpty()
		{
			var errorMessageInvoiceDateMonthIsSameAsReverseDateMonth = "The month of the invoice date and the revenue recognition date must be the same.";

			var additionalInfo1 = new AdditionalInfo
			{
				Lines = new List<TaxInvoiceTradeLineItem>()
				{
					new TaxInvoiceTradeLineItem()
					{
						ReverseDate = ZDateTime.Empty
					},
					new TaxInvoiceTradeLineItem()
					{
						ReverseDate = ZDateTime.Empty
					}
				}
			};
			var transactionInfo1 = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionDate = new ZDateTime(2022, 06, 04),
			};
			var loggerCase1 = ValidateTaxInvoice(additionalInfo1, transactionInfo1);
			AssertContains("All month is empty", errorMessageInvoiceDateMonthIsSameAsReverseDateMonth, loggerCase1.ToString());

			var additionalInfo2 = new AdditionalInfo
			{
				Lines = new List<TaxInvoiceTradeLineItem>()
				{
					new TaxInvoiceTradeLineItem()
					{
						ReverseDate = ZDateTime.Empty
					},
					new TaxInvoiceTradeLineItem()
					{
						ReverseDate = new ZDateTime(2022,06,04)
					},
				}
			};
			var transactionInfo2 = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionDate = new ZDateTime(2022, 06, 03),
			};
			var loggerCase2 = ValidateTaxInvoice(additionalInfo2, transactionInfo2);
			AssertContains("Exist one line, month is empty", errorMessageInvoiceDateMonthIsSameAsReverseDateMonth, loggerCase2.ToString());

			var additionalInfo_amendingTransaction = new AdditionalInfo
			{
				OriginalIssueID = "AAA",
				Lines = new List<TaxInvoiceTradeLineItem>()
				{
					new TaxInvoiceTradeLineItem()
					{
						ReverseDate = ZDateTime.Empty
					},
					new TaxInvoiceTradeLineItem()
					{
						ReverseDate = new ZDateTime(2022,06,04)
					},
				}
			};
			var transactionInfo_amendingTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionDate = new ZDateTime(2022, 06, 03),
			};
			var loggerCase_amendingTransaction = ValidateTaxInvoice(additionalInfo_amendingTransaction, transactionInfo_amendingTransaction);
			AssertNotContains("Should not run this validation when it is amending transaction.", errorMessageInvoiceDateMonthIsSameAsReverseDateMonth, loggerCase_amendingTransaction.ToString());
		}

		TransactionInfo GenValidTransactionInfo()
		{
			return new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionDate = new ZDateTime(2022, 03, 04),
			};
		}

		Logger ValidateTaxInvoice(AdditionalInfo additionalInfo)
		{
			return ValidateTaxInvoice(additionalInfo, GenValidTransactionInfo());
		}

		Logger ValidateTaxInvoice(AdditionalInfo additionalInfo, TransactionInfo transactionInfo)
		{
			var logger = new Logger();
			var validator = new TaxInvoiceValidation();
			validator.ValidateTaxInvoice(logger, additionalInfo, transactionInfo);
			return logger;
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ImportLicenseInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var importLicense = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().ImportLicenseSupportingInfo;

			AssertNoErrors(importLicense.CSI_CodeInfo);

			importLicense.CSI_ReferenceNumber = "1234";

			AssertHasMessageErrorContaining(importLicense.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

			importLicense.CSI_Code = "3";
			AssertHasMessageError(importLicense.CSI_CodeInfo, ListValidation.InvalidCodeMessageError.ToString());
			importLicense.CSI_Code = ImportLicenseType.Codes.PostBoarding;
			AssertNoMessageErrors(importLicense.CSI_CodeInfo);
		}

		public void TestCheckCSI_DateOfIssue()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var importLicense = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().ImportLicenseSupportingInfo;

			AssertNoErrors(importLicense.CSI_DateOfIssueInfo);

			importLicense.CSI_ReferenceNumber = "1234";
			importLicense.CSI_DateOfIssue = new ZDateTime(1899, 12, 31);
			AssertHasError("earlier than '1900-01-01", importLicense.CSI_DateOfIssueInfo, TypeValidation.ErrorForSmallDateTimePast(importLicense.CSI_DateOfIssue));
			importLicense.CSI_DateOfIssue = new ZDateTime(1900, 01, 01);
			AssertNoErrors(importLicense.CSI_DateOfIssueInfo);
			importLicense.CSI_DateOfIssue = new ZDateTime(2079, 06, 07);
			AssertHasError("later than '2079-06-06'", importLicense.CSI_DateOfIssueInfo, TypeValidation.ErrorForSmallDateTimeFuture(importLicense.CSI_DateOfIssue));
			importLicense.CSI_DateOfIssue = new ZDateTime(2079, 06, 06);
			AssertNoErrors(importLicense.CSI_DateOfIssueInfo);

			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(importLicense.CSI_DateOfIssueInfo, importLicense.CSI_ReferenceNumberInfo);
		}

		public void TestCheckCSI_SubType()
		{
			ReferenceTestDataHelper.CreateReferenceDataForILFFeeTypeList(Factory);
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var importLicense = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().ImportLicenseSupportingInfo;

			AssertNoMessageErrors(importLicense.CSI_SubTypeInfo);

			importLicense.CSI_ReferenceNumber = "1234";
			importLicense.CSI_SubType = "XXX";
			AssertHasMessageError(importLicense.CSI_SubTypeInfo, ListValidation.InvalidCodeMessageError.ToString());

			importLicense.CSI_SubType = "F1ND";
			AssertNoMessageErrors(importLicense.CSI_SubTypeInfo);
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var messageTypeCodes = declaration.Lookups.MessageTypeList.GetAllCodes();
			AssertHasErrorNotification(BRJobMessageTypeList.Codes.ImportSiscomex, true);
			AssertNoErrorNotification(BRJobMessageTypeList.Codes.Import, true);
			AssertNoErrorNotification(BRJobMessageTypeList.Codes.ImportLicense, true);
			AssertNoErrorNotification(BRJobMessageTypeList.Codes.Export, true);
			AssertNoErrorNotification(BRJobMessageTypeList.Codes.LPCO, true);

			declaration.MakeNonPersistent();
			messageTypeCodes = invoice.Lookups.MessageTypes.GetAllCodes();
			AssertHasErrorNotification(BRJobMessageTypeList.Codes.Import, false);
			AssertNoErrorNotification(BRJobMessageTypeList.Codes.Export, false);

			void AssertHasErrorNotification(string messageTypeCode, bool isPersistent)
			{
				if (isPersistent)
				{
					declaration.JE_MessageType = messageTypeCode;
				}
				else
				{
					invoice.JZ_MessageType = messageTypeCode;
				}

				CombineAssertions($"Error Notification when {messageTypeCode}", () =>
				{
					invoiceLine.ImportLicenseNumber = ZString.Empty;
					AssertNoError(invoiceLine.ImportLicenseNumberInfo, "Import License Number must only contain numeric characters.");
					AssertNoError(invoiceLine.ImportLicenseNumberInfo, "Import License Number has less than 10 characters.");

					invoiceLine.ImportLicenseNumber = "0123456789";
					AssertNoError(invoiceLine.ImportLicenseNumberInfo, "Import License Number must only contain numeric characters.");
					AssertNoError(invoiceLine.ImportLicenseNumberInfo, "Import License Number has less than 10 characters.");

					invoiceLine.ImportLicenseNumber = "T123456789";
					AssertHasError(invoiceLine.ImportLicenseNumberInfo, "Import License Number must only contain numeric characters.");
					AssertNoError(invoiceLine.ImportLicenseNumberInfo, "Import License Number has less than 10 characters.");

					invoiceLine.ImportLicenseNumber = "0123456";
					AssertNoError(invoiceLine.ImportLicenseNumberInfo, "Import License Number must only contain numeric characters.");
					AssertHasError(invoiceLine.ImportLicenseNumberInfo, "Import License Number has less than 10 characters.");

					invoiceLine.ImportLicenseNumber = "T123456";
					AssertHasError(invoiceLine.ImportLicenseNumberInfo, "Import License Number must only contain numeric characters.");
					AssertHasError(invoiceLine.ImportLicenseNumberInfo, "Import License Number has less than 10 characters.");
				});
			}

			void AssertNoErrorNotification(string messageTypeCode, bool isPersistent)
			{
				if (isPersistent)
				{
					declaration.JE_MessageType = messageTypeCode;
				}
				else
				{
					invoice.JZ_MessageType = messageTypeCode;
				}
				CombineAssertions($"Error Notification when {messageTypeCode}", () =>
				{
					invoiceLine.ImportLicenseNumber = "T123456789";
					AssertNoError(invoiceLine.ImportLicenseNumberInfo, "Import License Number must only contain numeric characters.");
					invoiceLine.ImportLicenseNumber = "0123456";
					AssertNoError(invoiceLine.ImportLicenseNumberInfo, "Import License Number has less than 10 characters.");
					invoiceLine.ImportLicenseNumber = "T123456";
					AssertNoError(invoiceLine.ImportLicenseNumberInfo, "Import License Number must only contain numeric characters.");
					AssertNoError(invoiceLine.ImportLicenseNumberInfo, "Import License Number has less than 10 characters.");
				});
			}
		}
	}
}

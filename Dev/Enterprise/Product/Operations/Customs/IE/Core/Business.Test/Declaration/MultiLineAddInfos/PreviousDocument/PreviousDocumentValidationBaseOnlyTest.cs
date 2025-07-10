using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(CommonPreviousDocumentValidation))]
	sealed class PreviousDocumentValidationBaseOnlyTest : PreviousDocumentValidationAbstractTest
	{
		public void TestCheckCSI_ReferenceNumberWhenNotEntered()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_ReferenceNumberInfo);
		}

		[TestDate(2023, 11, 15)]
		public void TestCheckCSI_ReferenceNumberDateFormatAndCannotBeFutureDate()
		{
			const string message = "[BR2017] Please enter a valid Reference Number in the format of 'yyyyMMdd' + 'EIDR Reference' with no space in between the two components. The entered date must not be a future date.";
			CombineAssertions("Validations for date format and future date check", () =>
			{
				previousDocument.CSI_Code = PreviousDocumentTypeList.Codes.ReferenceDateOfEntryInTheDeclarantRecords;

				previousDocument.CSI_ReferenceNumber = "20231115REFNUMBER";
				AssertNoMessageError("No future date and correct format", previousDocument.CSI_ReferenceNumberInfo, message);

				previousDocument.CSI_ReferenceNumber = "20231115 REFNUMBER";
				AssertHasMessageError("No future date but wrong format because of space", previousDocument.CSI_ReferenceNumberInfo, message);

				previousDocument.CSI_ReferenceNumber = "X0231115REFNUMBER";
				AssertHasMessageError("No future date and correct format", previousDocument.CSI_ReferenceNumberInfo, message);

				previousDocument.CSI_ReferenceNumber = "20241212 REFNUMBER";
				AssertHasMessageError("There is future date and wrong format because of space", previousDocument.CSI_ReferenceNumberInfo, message);

				previousDocument.CSI_ReferenceNumber = "20241212REFNUMBER";
				AssertHasMessageError("There is future date and correct format with no space", previousDocument.CSI_ReferenceNumberInfo, message);
			});
		}

		public void TestCheckCSI_DateOfIssue_CannotBePastDate()
		{
			const string message = "Date of Issue Cannot be earlier than Today.";
			CombineAssertions(() =>
			{
				validation.ValidateCSI_DateOfIssue();
				AssertNoMessageError("Default", previousDocument.CSI_DateOfIssueInfo, message);

				previousDocument.CSI_Code = PreviousDocumentTypeList.Codes.ReferenceDateOfEntryInTheDeclarantRecords;
				previousDocument.CSI_DateOfIssue = ZDateTime.Today.AddDays(-1);
				AssertHasMessageError("Past Date", previousDocument.CSI_DateOfIssueInfo, message);
				previousDocument.CSI_DateOfIssue = ZDateTime.Today;
				AssertNoMessageError("Not Past Date", previousDocument.CSI_DateOfIssueInfo, message);
			});
		}

		public void TestCheckCSI_SubType()
		{
			CombineAssertions(() =>
			{
				previousDocument.Validation.ValidateCSI_SubType();
				AssertNoNotifications("CSI_SubType is not used in IE, hence no Mandatory validation required.", previousDocument.CSI_SubTypeInfo);

				previousDocument.CSI_SubType = "^_^";
				AssertNoNotifications("CSI_SubType is not used in IE, hence no List validation required.", previousDocument.CSI_SubTypeInfo);
			});
		}

		#region CheckCSI_PackType

		public void TestCheckCSI_PackType_InvoiceHeaders_NoCheck()
		{
			var declaration = Factory.New<JobDeclaration>();
			var testPreviousDocument = declaration.Invoices.AddNew().PreviousDocuments.AddNew();
			testPreviousDocument.CSI_PackType = "XX";

			AssertNoMessageErrors("No check of CSI_PackType for InvoiceHeader", testPreviousDocument.CSI_PackTypeInfo);
		}

		#endregion

		#region CheckCSI_UnitOfQuantity

		public void TestCheckCSI_UnitOfQuantity_InvoiceHeaders_NoCheck()
		{
			var declaration = Factory.New<JobDeclaration>();
			var testPreviousDocument = declaration.Invoices.AddNew().PreviousDocuments.AddNew();
			testPreviousDocument.CSI_UnitOfQuantity = "XXX";

			AssertNoMessageErrors("No check of CSI_UnitOfQuantity for InvoiceHeader", testPreviousDocument.CSI_UnitOfQuantityInfo);
		}

		#endregion

		protected override string MessageType => IEJobMessageTypeList.Codes.MiscellaneousCustoms;
		protected override PreviousDocument SetupPreviousDocument() => jobDeclaration.PreviousDocuments.AddNew();
	}
}

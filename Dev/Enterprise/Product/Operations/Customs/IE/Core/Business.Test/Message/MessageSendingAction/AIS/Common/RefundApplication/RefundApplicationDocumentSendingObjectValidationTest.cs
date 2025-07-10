using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class RefundApplicationDocumentSendingObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckDocumentType()
		{
			parent.ShouldSend = ZBool.False;
			sendingObject.DocumentType = ZString.Empty;
			sendingObject.Validation.ValidateDocumentType();
			AssertNoErrors("No error if parent is not selected for sending.", sendingObject.DocumentTypeInfo);

			parent.ShouldSend = ZBool.True;
			sendingObject.Validation.ValidateDocumentType();
			AssertHasErrorContaining("DocumentType should have a validation.", sendingObject.DocumentTypeInfo, MandatoryValidation.MustBeEntered);

			sendingObject.DocumentType = "9002";
			sendingObject.Validation.ValidateDocumentType();
			AssertNoErrors("DocumentType passed.", sendingObject.DocumentTypeInfo);
		}

		public void TestCheckDocumentTypeIsWesternEuropean()
		{
			parent.ShouldSend = ZBool.False;
			sendingObject.DocumentType = "么";
			sendingObject.Validation.ValidateDocumentType();
			AssertNoErrors("No error if parent is not selected for sending.", sendingObject.DocumentTypeInfo);

			parent.ShouldSend = ZBool.True;
			var errorMessage = EnglishCharactersValidation.GetNotificationMessage(sendingObject.DocumentTypeInfo);
			sendingObject.Validation.ValidateDocumentType();
			AssertHasError("DocumentType should have a validation.", sendingObject.DocumentTypeInfo, errorMessage);

			sendingObject.DocumentType = "9";
			sendingObject.Validation.ValidateDocumentType();
			AssertNoErrors("DocumentType passed.", sendingObject.DocumentTypeInfo);
		}

		public void TestCheckDocumentDateIsValidZDateTime()
		{
			parent.ShouldSend = ZBool.False;
			sendingObject.DocumentDate = ZDateTime.Invalid;
			sendingObject.Validation.ValidateDocumentDate();
			AssertNoErrors("No error if parent is not selected for sending.", sendingObject.DocumentDateInfo);

			parent.ShouldSend = ZBool.True;
			string invalidOrgErrorMessage = string.Format(TypeValidation.InvalidTypeMessage, TypeValidation.GetHumanReadablePropertyName(sendingObject.DocumentDateInfo));
			sendingObject.Validation.ValidateDocumentDate();
			AssertHasError("DocumentDate should have a validation.", sendingObject.DocumentDateInfo, invalidOrgErrorMessage);

			sendingObject.DocumentDate = ZDateTime.Today;
			sendingObject.Validation.ValidateDocumentDate();
			AssertNoErrors("DocumentDate passed.", sendingObject.DocumentDateInfo);
		}

		public void TestCheckDocumentDateIsValidZDateTimeRange()
		{
			parent.ShouldSend = ZBool.False;
			var pastYearsLimit = TypeValidationLimits.Default.PastYearsBeforeError;
			var date = ZDateTime.Today.AddYears(-(pastYearsLimit + 1));
			sendingObject.DocumentDate = date;
			sendingObject.Validation.ValidateDocumentDate();
			AssertNoErrors("No error if parent is not selected for sending.", sendingObject.DocumentDateInfo);

			parent.ShouldSend = ZBool.True;
			var errorMessage = DateRangeValidation.ErrorForPastYear(date.ToString("dd-MMM-yyyy"), pastYearsLimit);
			sendingObject.Validation.ValidateDocumentDate();
			AssertHasError("DocumentDate should have a validation.", sendingObject.DocumentDateInfo, errorMessage);

			sendingObject.DocumentDate = ZDateTime.Today;
			sendingObject.Validation.ValidateDocumentDate();
			AssertNoErrors("DocumentDate passed.", sendingObject.DocumentDateInfo);
		}

		public void TestCheckDocumentIdentifierIsWesternEuropean()
		{
			parent.ShouldSend = ZBool.False;
			sendingObject.DocumentIdentifier = "么";
			sendingObject.Validation.ValidateDocumentIdentifier();
			AssertNoErrors("No error if parent is not selected for sending.", sendingObject.DocumentIdentifierInfo);

			parent.ShouldSend = ZBool.True;
			var errorMessage = EnglishCharactersValidation.GetNotificationMessage(sendingObject.DocumentIdentifierInfo);
			sendingObject.Validation.ValidateDocumentIdentifier();
			AssertHasError("DocumentIdentifier should have a validation.", sendingObject.DocumentIdentifierInfo, errorMessage);

			sendingObject.DocumentIdentifier = "9";
			sendingObject.Validation.ValidateDocumentIdentifier();
			AssertNoErrors("DocumentIdentifier passed.", sendingObject.DocumentIdentifierInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var (entryHeaderWrapper, _) = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			entryHeader = entryHeaderWrapper.EntryHeader;
			parent = new RefundApplicationMessageSendingAction(entryHeader);
			sendingObject = new RefundApplicationDocumentSendingObject(entryHeader, parent);
		}
		CusEntryHeader entryHeader;
		RefundApplicationDocumentSendingObject sendingObject;
		RefundApplicationMessageSendingAction parent;
	}
}

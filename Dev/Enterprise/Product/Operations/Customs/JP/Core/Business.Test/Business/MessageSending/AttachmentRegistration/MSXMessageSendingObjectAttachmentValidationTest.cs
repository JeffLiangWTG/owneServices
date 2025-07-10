using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.JP.Business.Testing
{
	sealed class MSXMessageSendingObjectAttachmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckFile()
		{
			const string invalidFileErrorMessage = "Enter a valid File.";
			const string fileNameShouleNotBeDuplicatedErrorMessage = "Every file name must be unique. If the same file name already exists, please choose a different one.";
			const string fileExtensionErrorMessage = "Only TXT, DOC, DOCX, PPT, PPTX, XML, HTM, HTML, RTF, JTD, XLS, XLSX, CSV, JPEG, JPE, JPG, TIF, TIFF, BMP, GIF, PNG, PDF, JET are allowed.";
			const string fileNameTooLongErrorMessage = "Max length exceeded. Please enter a shorter file name.";
			const string tooManyPeriodsErrorMessage = "Period is only allowed in file extension.";
			const string unspportedCharactersErrorMessage1 = "There are unsupported characters in filename, please change the filename.";
			const string unsupportedCharactersErrorMessage2 = "The following characters are not allowed: ";
			var declaration = Factory.New<JobDeclaration>();
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "invoice.pdf", "CIV");
			var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "invoice2.pdf", "CIV");
			var eDoc3 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "test.mp4", "CIV");
			var eDoc4 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "12345678901234567890123456789012345678901234567.txt", "CIV");
			var eDoc5 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "test.doc.pdf", "CIV");
			var eDoc6 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "\u7EAA.pdf", "CIV");
			var eDoc7 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "t-E_s t!.pdf", "CIV");
			var header = declaration.CustomsEntryHeaders.AddNew();
			var msxMessageSendingObject = new MSXMessageSendingObject(header);
			var attachment1 = msxMessageSendingObject.Attachments.AddNew();
			var attachment2 = msxMessageSendingObject.Attachments.AddNew();

			attachment1.File = ZGuid.Invalid;
			AssertHasError(attachment1.FileInfo, invalidFileErrorMessage);

			attachment1.File = eDoc1.UniqueKey;
			AssertNoError(attachment1.FileInfo, invalidFileErrorMessage);
			AssertNoError(attachment1.FileInfo, fileNameShouleNotBeDuplicatedErrorMessage);
			attachment2.File = eDoc1.UniqueKey;
			attachment1.Validation.ValidateFile();
			AssertHasError(attachment1.FileInfo, fileNameShouleNotBeDuplicatedErrorMessage);

			attachment2.File = eDoc3.UniqueKey;
			AssertHasError(attachment2.FileInfo, fileExtensionErrorMessage);
			attachment2.File = eDoc2.UniqueKey;
			AssertNoError(attachment2.FileInfo, fileExtensionErrorMessage);

			attachment2.File = eDoc4.UniqueKey;
			AssertHasError(attachment2.FileInfo, fileNameTooLongErrorMessage);

			attachment2.File = eDoc5.UniqueKey;
			AssertHasError(attachment2.FileInfo, tooManyPeriodsErrorMessage);

			attachment2.File = eDoc6.UniqueKey;
			AssertHasError(attachment2.FileInfo, unspportedCharactersErrorMessage1);

			attachment2.File = eDoc7.UniqueKey;
			AssertHasError(attachment2.FileInfo, unsupportedCharactersErrorMessage2 + "!");
		}

		public void TestCheckFileSize()
		{
			const string fileTooLargeErrorMessage = "The maximum file size allowed is 10 MB.";
			var declaration = Factory.New<JobDeclaration>();
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1024 * 1024 * 10], "Invoice.pdf", "CIV");
			var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1024 * 1024 * 10 + 1], "Invoice2.pdf", "CIV");
			var header = declaration.CustomsEntryHeaders.AddNew();
			var msxMessageSendingObject = new MSXMessageSendingObject(header);
			var attachment = msxMessageSendingObject.Attachments.AddNew();
			attachment.File = eDoc1.UniqueKey;

			AssertNoError(attachment.FileSizeInfo, fileTooLargeErrorMessage);

			attachment.File = eDoc2.UniqueKey;
			AssertHasError(attachment.FileSizeInfo, fileTooLargeErrorMessage);
		}

		public void TestCheckType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			var msxMessageSendingObject = new MSXMessageSendingObject(header);
			var attachment = msxMessageSendingObject.Attachments.AddNew();

			attachment.Validation.ValidateType();
			AssertHasError(attachment.TypeInfo, MandatoryValidation.MustBeEnteredMessage(attachment.TypeInfo.HumanReadableName));

			attachment.Type = "OR";
			AssertNoError(attachment.TypeInfo, MandatoryValidation.MustBeEnteredMessage(attachment.TypeInfo.HumanReadableName));

			attachment.Type = "OS";
			AssertHasError(attachment.TypeInfo, ListValidation.InvalidCodeError + attachment.TypeInfo.HumanReadableName + ".");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingCusCodeType("JPDOC", "JPDOC", Core.Constants.CountryCodes.Japan);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, "JPDOC", "OR", startDate, endDate);
			Factory.Save();
		}
	}
}

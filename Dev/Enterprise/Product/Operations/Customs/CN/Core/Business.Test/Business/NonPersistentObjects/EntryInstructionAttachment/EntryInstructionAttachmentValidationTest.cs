using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class EntryInstructionAttachmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAttachmentType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var attachment = instruction.Attachments.AddNew();
			var info = attachment.AttachmentTypeInfo;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			attachment.Validation.ValidateAll();
			AssertHasError(info, MandatoryValidation.MustBeEnteredMessage("Attachment Type"));
			attachment.AttachmentType = "abc";
			AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
			attachment.AttachmentType = CSDDocTypeList.Codes._00000001;
			AssertNoMessageErrors(info);
			var attachment1 = instruction.Attachments.AddNew();
			var info1 = attachment1.AttachmentTypeInfo;
			attachment1.AttachmentType = CSDDocTypeList.Codes._00000001;
			AssertHasMessageErrorContaining(info1, "The type is duplicated.");
			attachment1.AttachmentType = CSDDocTypeList.Codes._00000002;
			AssertNoMessageErrors(info1);
		}

		public void TestCheckLinkToInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var attachment = instruction.Attachments.AddNew();
			attachment.AttachmentType = CSDDocTypeList.Codes._80000001;

			var targetMessage = "Invoice Lines should be linked to this Attachment.";
			var targetInfo = attachment.AttachmentTypeInfo;

			AssertHasMessageError("AttachmentType 80000001 requires an InvoiceLine Attachment linked.", targetInfo, targetMessage);

			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.CargoAttributes.AddNew(CargoAttributeList.Codes._31);
			((AttachmentInvoiceLineLink)invoiceLine.AttachmentLinks.First()).IsLinked = true;

			attachment.Validation.ValidateAttachmentType();
			AssertNoMessageError("AttachmentType 80000001 requires an InvoiceLine Attachment linked(validation pass).", targetInfo, targetMessage);
		}

		public void TestCheckEDoc()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var attachment = instruction.Attachments.AddNew();
			var info = attachment.EDocInfo;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			attachment.Validation.ValidateEDoc();
			AssertHasErrorContaining(info, "Please enter an eDoc.");
			attachment.EDoc = ZGuid.Missing;
			AssertHasErrorContaining(info, "The selected eDoc is no longer valid. Please choose a new eDoc from the list.");
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "test1.pdf", Core.Constants.FileFormats.PDF);
			var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[(4 * 1024 * 1024) + 1], "test2.pdf", Core.Constants.FileFormats.PDF);
			attachment.EDoc = eDoc2.UniqueKey;
			AssertHasMessageErrorContaining(info, "The selected eDoc is larger than 4M.");
			AssertNoErrors(info);
			attachment.EDoc = eDoc1.UniqueKey;
			AssertNoMessageErrors(info);
		}

		public void TestCheckAttachmentNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var attachment = instruction.Attachments.AddNew();
			var info = attachment.AttachmentNumberInfo;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

			var shouldBe17Digit = "The attachment number should be 17 digits.";
			attachment.AttachmentType = CSDDocTypeList.Codes._10000001;
			attachment.AttachmentNumber = "1234567890";
			AssertHasMessageErrorContaining(info, shouldBe17Digit);
			attachment.AttachmentNumber = "ab1234567890";
			AssertHasMessageErrorContaining(info, shouldBe17Digit);
			attachment.AttachmentNumber = ZString.Empty;
			AssertHasMessageErrorContaining(info, shouldBe17Digit);
			attachment.AttachmentNumber = "12345678901234567";
			AssertNoMessageErrors(info);

			var shouldBe12Digit = "The attachment number should be 12 digits.";
			attachment.AttachmentType = CSDDocTypeList.Codes._10000002;
			attachment.AttachmentNumber = "1234567890";
			AssertHasMessageErrorContaining(info, shouldBe12Digit);
			attachment.AttachmentNumber = "ab1234567890";
			AssertHasMessageErrorContaining(info, shouldBe12Digit);
			attachment.AttachmentNumber = ZString.Empty;
			AssertHasMessageErrorContaining(info, shouldBe12Digit);
			attachment.AttachmentNumber = "123456789012";
			AssertNoMessageErrors(info);

			var mustBe13Digit = "The attachment number should be 13 digits.";
			attachment.AttachmentType = CSDDocTypeList.Codes._10000003;
			attachment.AttachmentNumber = "1234567890";
			AssertHasMessageErrorContaining(info, mustBe13Digit);
			attachment.AttachmentNumber = "abc1234567890";
			AssertHasMessageErrorContaining(info, mustBe13Digit);
			attachment.AttachmentNumber = ZString.Empty;
			AssertHasMessageErrorContaining(info, mustBe13Digit);
			attachment.AttachmentNumber = "1234567890123";
			AssertNoMessageErrors(info);
		}

		public void TestValidationModeProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var attachment = instruction.Attachments.AddNew();
			ValidationExtensionsTest.AssertValidationModeProvider(declaration, attachment.Validation.ValidationModeProvider);

			instruction = Factory.New<CusEntryInstruction>();
			attachment = instruction.Attachments.AddNew();
			AssertNull(attachment.Validation.ValidationModeProvider);
		}
	}
}

using System;

namespace Enterprise.Customs.CH.Business.Testing;

internal class SupportingDocSendingObjectValidationTest : Customs.Business.Testing.SupportingDocSendingObjectValidationTest
{
	public void TestCheckEDoc_FileExtensions()
	{
		var allowedFileExtensions = new string[] { "txt", "pdf" };
		using (CHCustomsDataRegistry.Instance.AllowedFileExtensions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, allowedFileExtensions))
		{
			var pdfDoc = Declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var xlsDoc = Declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.xls", "CIV");
			var noextDoc = Declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice", "CIV");
			SupportingDocSendingObject sendingObject = new SupportingDocSendingObject(Declaration);

			CombineAssertions(() =>
			{
				sendingObject.EDoc = xlsDoc.UniqueKey;
				AssertHasError("Invalid ext", sendingObject.EDocInfo, ValidationMessages.SupportingDocSendingObject.InvalidFileExtension(allowedFileExtensions));

				sendingObject.EDoc = pdfDoc.UniqueKey;
				AssertNoError("Valid ext", sendingObject.EDocInfo, ValidationMessages.SupportingDocSendingObject.InvalidFileExtension(allowedFileExtensions));

				sendingObject.EDoc = noextDoc.UniqueKey;
				AssertHasError("No ext", sendingObject.EDocInfo, ValidationMessages.SupportingDocSendingObject.InvalidFileExtension(allowedFileExtensions));
			});
		}
	}

	public override void TestCheckEDocFileSizeInMB()
	{
		using (CHCustomsDataRegistry.Instance.MaximumFileSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
		{
			var smallDoc = Declaration.DocManagerInfo.AddFileOrDocument(new byte[1024 * 1024], "Invoice1.pdf", "CIV");
			var largeDoc = Declaration.DocManagerInfo.AddFileOrDocument(new byte[1024 * 1024 + 1], "Invoice2.pdf", "CIV");
			SupportingDocSendingObject sendingObject = new SupportingDocSendingObject(Declaration);

			CombineAssertions(() =>
			{
				sendingObject.EDoc = largeDoc.UniqueKey;
				AssertHasError("Too large doc", sendingObject.EDocFileSizeInMBInfo, ValidationMessages.SupportingDocSendingObject.MaxAllowedFileSizeExceeded(1));

				sendingObject.EDoc = smallDoc.UniqueKey;
				AssertNoError("No too large doc", sendingObject.EDocFileSizeInMBInfo, ValidationMessages.SupportingDocSendingObject.MaxAllowedFileSizeExceeded(1));
			});
		}
	}

	JobDeclaration Declaration => declaration ?? (declaration = Factory.NewWithValidTestData<JobDeclaration>());
	JobDeclaration declaration;
}

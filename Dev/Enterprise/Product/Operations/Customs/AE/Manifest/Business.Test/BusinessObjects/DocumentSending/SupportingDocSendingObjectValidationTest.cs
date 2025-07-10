using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class SupportingDocSendingObjectValidationTest : TestCaseWithFactory
{
	public void TestCheckDocumentType() => CombineAssertions(() =>
	{
		var errMsg = "The file type is not supported. Please convert document to one of the supported types - jpeg, jpg, png, pdf.";
		sendingObject.DocumentType = ".txt";
		AssertHasErrorContaining(sendingObject.DocumentTypeInfo, errMsg);
		sendingObject.DocumentType = ".pdf";
		AssertNoErrorContaining(sendingObject.DocumentTypeInfo, errMsg);
	});

	public void TestCheckEDocFileSizeInMB() => CombineAssertions(() =>
	{
		var eDocNormal = manifest.DocManagerInfo.AddFileOrDocument(new byte[sendingObject.Validation.MaxEDocFileSizeInBytes], "Normal.pdf", "CIV");
		sendingObject.EDoc = eDocNormal.UniqueKey;
		sendingObject.Validation.ValidateEDocFileSizeInMB();
		AssertNoError(sendingObject.EDocFileSizeInMBInfo, sendingObject.Validation.EDocTooLargeError);

		var eDocTooLarge = manifest.DocManagerInfo.AddFileOrDocument(new byte[sendingObject.Validation.MaxEDocFileSizeInBytes + 1], "TooLarge.pdf", "CIV");
		sendingObject.EDoc = eDocTooLarge.UniqueKey;
		sendingObject.Validation.ValidateEDocFileSizeInMB();
		AssertHasError(sendingObject.EDocFileSizeInMBInfo, sendingObject.Validation.EDocTooLargeError);
	});

	public void TestCheckCUSRES() => ValidationTestHelper.AssertErrorIfNotEntered(sendingObject.CUSRESInfo);

	public void TestCheckCUSCAR() => ValidationTestHelper.AssertErrorIfNotEntered(sendingObject.CUSCARInfo);

	protected override void SetUp()
	{
		base.SetUp();
		manifest = Factory.New<AsycudaManifestHeader>();
		sendingObject = (SupportingDocSendingObject)manifest.GetSupportingDocSendingObject();
	}
	SupportingDocSendingObject sendingObject;
	AsycudaManifestHeader manifest;
}


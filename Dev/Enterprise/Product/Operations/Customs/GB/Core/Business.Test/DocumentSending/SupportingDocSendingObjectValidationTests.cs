using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.DocumentSending;

namespace Enterprise.Customs.GB.Business.Testing.DocumentSending
{
	public class SupportingDocSendingObjectValidationTests : Customs.Business.Testing.SupportingDocSendingObjectValidationTest
	{
		public override void TestCheckDocumentType()
		{
			var dec = Factory.New<JobDeclaration>();
			var sendingObject = SupportingDocSendingObject.New(dec);
			sendingObject.DocumentType = ZString.Empty;

			AssertHasErrorContaining(sendingObject.DocumentTypeInfo, MandatoryValidation.MustBeEntered);

			sendingObject.DocumentType = "XXX";
			AssertNoNotifications(sendingObject.DocumentTypeInfo);
		}

		public void TestInvalidFileNameCharsForSending()
		{
			var dec = Factory.New<JobDeclaration>();
			var sendingObject = SupportingDocSendingObject.New(dec);
			AssertContainsExactElementsInAnyOrder("Invalid chars", new char[] { '{', '}' }, sendingObject.Validation.InvalidFileNameCharsForSending);
		}

		public void TestCheckEDocFileName()
		{
			var dec = Factory.New<JobDeclaration>();
			var sendingObject = SupportingDocSendingObject.New(dec);

			string expectedError = "The filename contains characters that are not acceptable. On the eDocs tab, please rename the file so that its name does not contain any of the following characters, the retry the operation. Forbidden characters: { }";

			CombineAssertions(() =>
			{
				var eDoc = dec.DocManagerInfo.AddFileOrDocument(new byte[1], "Test {.pdf", "CIV");
				sendingObject.EDoc = eDoc.UniqueKey;
				sendingObject.Validation.ValidateEDoc();
				AssertHasErrorContaining(sendingObject.EDocInfo, expectedError);

				eDoc = dec.DocManagerInfo.AddFileOrDocument(new byte[1], "} Test.pdf", "CIV");
				sendingObject.EDoc = eDoc.UniqueKey;
				sendingObject.Validation.ValidateEDoc();
				AssertHasErrorContaining(sendingObject.EDocInfo, expectedError);

				eDoc = dec.DocManagerInfo.AddFileOrDocument(new byte[1], "Test.pdf", "CIV");
				sendingObject.EDoc = eDoc.UniqueKey;
				sendingObject.Validation.ValidateEDoc();
				AssertNoError(sendingObject.EDocInfo, expectedError);
			});
		}
	}
}

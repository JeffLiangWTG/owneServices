using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(CusStorageDocPivotValidation))]
	sealed class CusStorageDocPivotValidationTest : Customs.Business.Testing.CusStorageDocPivotValidationTest
	{
		public void TestCheckCSD_DocType()
		{
			var message = "Should have error when its parent is 'Request Header' and the document type is not PDF or TIF.";
			var error = "Please choose a document which file type is PDF, JPG or JPEG.";

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var requestHeader = manifestHeader.RequestHeaders.AddNew();
			requestHeader.EUS_Identifier = "XXX";
			requestHeader.EUS_Type = "YYY";

			var path = "Enterprise.Customs.EU.Manifest.ICS2.Business.Test.Message.MessageProcessors.Outgoing.TestFiles";
			var reader = new Customs.Business.Testing.TestFileReader(typeof(CusStorageDocPivotValidationTest));

			var eDoc1 = manifestHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var eDoc2 = manifestHeader.DocManagerInfo.AddFileOrDocument(reader.GetEmbeddedFileData(path, "TestImage.jpg"), "Invoice1.jpg", "CIV", overwriteExistingFileIfNotImageFile: true);
			var eDoc3 = manifestHeader.DocManagerInfo.AddFileOrDocument(reader.GetEmbeddedFileData(path, "TestImage.jpeg"), "Invoice2.jpeg", "CIV", overwriteExistingFileIfNotImageFile: true);
			var eDoc4 = manifestHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice3.xml", "CIV");

			AssertDocumentType(false, requestHeader, eDoc1.UniqueKey);
			AssertDocumentType(false, requestHeader, eDoc2.UniqueKey);
			AssertDocumentType(false, requestHeader, eDoc3.UniqueKey);
			AssertDocumentType(false, bill, eDoc4.UniqueKey);
			AssertDocumentType(true, requestHeader, eDoc4.UniqueKey);

			Factory.Save();

			void AssertDocumentType(bool hasError, BusinessObject parent, ZGuid docPk)
			{
				var pivot = GetNewPivot();
				pivot.Parent = parent;
				pivot.CSD_StorageDocReference = docPk;

				if (hasError)
				{
					AssertHasMessageError(message, pivot.CSD_DocTypeInfo, error);
				}
				else
				{
					AssertNoMessageError(message, pivot.CSD_DocTypeInfo, error);
				}
			}
		}

		protected override BaseCusStorageDocPivot GetNewPivot()
		{
			return base.Factory.New<CusStorageDocPivot>();
		}
	}
}

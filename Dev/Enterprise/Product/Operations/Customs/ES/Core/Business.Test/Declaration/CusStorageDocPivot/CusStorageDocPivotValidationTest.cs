using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Testing
{
	class CusStorageDocPivotValidationTest : Customs.Business.Testing.CusStorageDocPivotValidationTest
	{
		public void TestCheckCSD_StorageDocReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var pivot = entryHeader.EDocPivotCollection.AddNew();
			pivot.CSD_StorageDocReference = ZGuid.Empty;
			AssertHasErrorContaining(pivot.CSD_StorageDocReferenceInfo, MandatoryValidation.MustBeEntered);
			pivot.CSD_StorageDocReference = ZGuid.Invalid;
			AssertHasErrorContaining(pivot.CSD_StorageDocReferenceInfo, ListValidation.InvalidCodeError);
			pivot.CSD_StorageDocReference = eDoc1.UniqueKey;
			AssertNoNotifications(pivot.CSD_StorageDocReferenceInfo);
		}

		public void TestCheckCSD_CSD_DocType()
		{
			var message = "This document has an invalid extension. Allowed extensions are";
			var declaration = Factory.New<JobDeclaration>();
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.xxx", "CIV");
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var pivot = entryHeader.EDocPivotCollection.AddNew();
			pivot.CSD_DocType = "xxx";
			AssertHasRowErrorContaining(pivot, message);

			var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.PDF", "CIV");
			var pivot2 = entryHeader.EDocPivotCollection.AddNew();
			pivot2.CSD_DocType = "PDF";
			AssertNoRowErrorContaining(pivot2, message);
		}

		protected override BaseCusStorageDocPivot GetNewPivot() => Factory.New<CusStorageDocPivot>();

		protected override string DocTypeStorageDuplicatingMessage => "An eDoc can only be declared once per entry";
	}
}

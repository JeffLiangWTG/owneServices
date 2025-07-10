using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ColsCusStorageDocPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSD_StorageDocReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice1.pdf", "CIV");
			var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice2.pdf", "CIV");
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			var pivot1 = colsHeader.EDocPivotCollection.AddNew();

			pivot1.CSD_StorageDocReference = ZGuid.Empty;
			AssertHasErrorContaining(pivot1.CSD_StorageDocReferenceInfo, MandatoryValidation.MustBeEntered);
			pivot1.CSD_StorageDocReference = ZGuid.NewZGuid();
			AssertHasErrorContaining(pivot1.CSD_StorageDocReferenceInfo, ListValidation.InvalidCodeError);
			pivot1.CSD_StorageDocReference = eDoc1.UniqueKey;
			AssertNoNotifications(pivot1.CSD_StorageDocReferenceInfo);

			var pivot2 = colsHeader.EDocPivotCollection.AddNew();
			pivot2.CSD_StorageDocReference = eDoc1.UniqueKey;
			AssertHasError(pivot2.CSD_StorageDocReferenceInfo, "EDoc should be unique.");

			pivot2.CSD_StorageDocReference = eDoc2.UniqueKey;
			AssertNoNotifications(pivot2.CSD_StorageDocReferenceInfo);
		}

		[TestDate(2019, 3, 6)]
		public void TestCheckCSD_DocType()
		{
			var date1 = new ZDateTime(2019, 3, 1);
			var date2 = new ZDateTime(2019, 3, 31);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			const string colstyp = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AUCOLSAttachmentType;
			helper.CreateNewOrGetExistingCusCodeType(colstyp, "colstyp Desc.");
			helper.CreateNewOrGetExistingCusCodeList("AU", colstyp, "CT1", "1", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", colstyp, "CT2", "2", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", colstyp, "CT3", "1", date1, date2);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			var pivot = colsHeader.EDocPivotCollection.AddNew();
			pivot.CSD_DocType = ZString.Empty;
			AssertHasErrorContaining(pivot.CSD_DocTypeInfo, MandatoryValidation.MustBeEntered);
			pivot.CSD_DocType = "XXX";
			AssertHasMessageErrorContaining(pivot.CSD_DocTypeInfo, ListValidation.InvalidCodeMessageError);
			pivot.CSD_DocType = "CT2";
			AssertNoNotifications(pivot.CSD_DocTypeInfo);
		}
	}
}

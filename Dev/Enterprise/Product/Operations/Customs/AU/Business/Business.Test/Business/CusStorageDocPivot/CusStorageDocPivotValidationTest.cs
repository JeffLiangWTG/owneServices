using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusStorageDocPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSD_StorageDocReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var invoice = declaration.Invoices.AddNew();
			var pivot = invoice.EDocPivotCollection.AddNew();
			pivot.CSD_StorageDocReference = ZGuid.Empty;
			AssertHasErrorContaining(pivot.CSD_StorageDocReferenceInfo, MandatoryValidation.MustBeEntered);
			pivot.CSD_StorageDocReference = ZGuid.NewZGuid();
			AssertHasErrorContaining(pivot.CSD_StorageDocReferenceInfo, ListValidation.InvalidCodeError);
			pivot.CSD_StorageDocReference = eDoc1.UniqueKey;
			AssertNoNotifications(pivot.CSD_StorageDocReferenceInfo);
		}

		[TestDate(2019, 3, 6)]
		public void TestCheckCSD_DocType()
		{
			var date1 = new ZDateTime(2019, 3, 1);
			var date2 = new ZDateTime(2019, 3, 31);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			const string natyp = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSAttachmentType;
			helper.CreateNewOrGetExistingCusCodeType(natyp, "NATYP Desc.");
			helper.CreateNewOrGetExistingCusCodeList("AU", natyp, "TY1", "1", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", natyp, "TY2", "2", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", natyp, "TY3", "1", date1, date2);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var pivot = invoice.EDocPivotCollection.AddNew();
			pivot.CSD_DocType = ZString.Empty;
			AssertHasErrorContaining(pivot.CSD_DocTypeInfo, MandatoryValidation.MustBeEntered);
			pivot.CSD_DocType = "XXX";
			AssertHasMessageErrorContaining(pivot.CSD_DocTypeInfo, ListValidation.InvalidCodeMessageError);
			pivot.CSD_DocType = "TY2";
			AssertNoNotifications(pivot.CSD_DocTypeInfo);
		}

		public void TestCheckCSD_Description()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var pivot = invoice.EDocPivotCollection.AddNew();
			pivot.CSD_Description = ZString.Empty;
			AssertHasMessageErrorContaining(pivot.CSD_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			pivot.CSD_Description = "XXX";
			AssertNoNotifications(pivot.CSD_DescriptionInfo);
		}
	}
}

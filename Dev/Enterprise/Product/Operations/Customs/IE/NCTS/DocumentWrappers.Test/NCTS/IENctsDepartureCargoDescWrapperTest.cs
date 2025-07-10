using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.DocumentWrappers.Testing
{
	[TestedType(typeof(IENctsDepartureCargoDescWrapper))]
	[MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Ireland)]
	sealed class IENctsDepartureCargoDescWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBox32ItemNumber()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			var item = bill.GoodsItems.AddNew();
			item.BY_LineNo = 1;
			item.BY_DeclarationGoodsItemNumber = 10111;
			var wrapper = IENctsDepartureCargoDescWrapper.New(item, Factory);
			AssertEquals("BOX32ITEM should return BY_LineNo / BY_DeclarationGoodsItemNumber", "1 / 10111", wrapper.BOX32ITEM);
		}

		public void TestGetSupportingDocumentsFormatted_Phase5()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var bill = header.Bills.AddNew();
			var billDocument = bill.SupportingDocuments.AddNew();
			billDocument.CSI_Code = "420";
			billDocument.CSI_ReferenceNumber = "123";

			var item = bill.GoodsItems.AddNew();
			var supportingDocument1 = item.SupportingDocuments.AddNew();
			supportingDocument1.CSI_LineNo = 1;
			supportingDocument1.CSI_Code = "9100";
			supportingDocument1.CSI_ReferenceNumber = "3278923";
			supportingDocument1.CSI_ItemNumber = 1;
			supportingDocument1.CSI_ReferenceNumber2 = "Test";
			var supportingDocument2 = item.SupportingDocuments.AddNew();
			supportingDocument2.CSI_LineNo = 2;
			supportingDocument2.CSI_Code = "9102";
			supportingDocument2.CSI_ReferenceNumber = "3278924";
			supportingDocument2.CSI_ItemNumber = 2;
			supportingDocument2.CSI_ReferenceNumber2 = "Test2";

			var wrapper = IENctsDepartureCargoDescWrapper.New(item, Factory);
			CombineAssertions(() =>
			{
				AssertEquals("BOX44 should get CSI_Code-CSI_ReferenceNumber from item.SupportingDocuments", "420-123; 9100-3278923; 9102-3278924", wrapper.BOX44);
				AssertEquals("BOX441DOCSANDCERTS should get same value as BOX44", "420-123; 9100-3278923; 9102-3278924", wrapper.BOX441DOCSANDCERTS);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return IENctsDepartureCargoDescWrapper.New(Factory.New<NctsDepartureCargoDesc>(), Factory);
		}
	}
}

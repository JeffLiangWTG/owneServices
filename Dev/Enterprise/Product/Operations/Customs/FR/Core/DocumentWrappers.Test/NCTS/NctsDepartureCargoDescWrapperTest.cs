using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.NCTS;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.NCTS.Testing;

[TestedType(typeof(NctsDepartureCargoDescWrapper))]
sealed class NctsDepartureCargoDescWrapperTest : NonPersistentBusinessObjectTestCase
{
	public void TestBox40Documents_Phase4()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var item = header.MovementHeader.GoodsItems.AddNew();

		var documentOnFirstLine = item.PreviousDocuments.AddNew();
		documentOnFirstLine.CSI_Procedure = "7";
		documentOnFirstLine.CSI_SubType = Enterprise.Customs.EU.Business.PreviousDocumentClassList.Codes.PreviousDocument;
		documentOnFirstLine.CSI_Code = "380";
		documentOnFirstLine.CSI_ReferenceNumber = "34217890";
		documentOnFirstLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 2);
		documentOnFirstLine.CSI_Status = "G";
		documentOnFirstLine.CSI_CustomsOffice = "IT279100";
		documentOnFirstLine.CSI_LineNo = 1;

		var documentOnSecondLine = item.PreviousDocuments.AddNew();
		documentOnSecondLine.CSI_Procedure = "7";
		documentOnSecondLine.CSI_SubType = "Y";
		documentOnSecondLine.CSI_Code = "CLE";
		documentOnSecondLine.CSI_ReferenceNumber2 = "20070701";
		documentOnSecondLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 3);
		documentOnSecondLine.CSI_Status = "G";
		documentOnSecondLine.CSI_CustomsOffice = "IT279100";
		documentOnSecondLine.CSI_LineNo = 1;

		var wrapper = NctsDepartureCargoDescWrapper.New(item, Factory);

		AssertEquals(nameof(NctsDepartureCargoDescWrapper.BOX40DOCUMENTS), "Z-380-7-34217890 G-02/01/2000-IT279100-1; Y-CLE-7-20070701 G-03/01/2000-IT279100-1", wrapper.BOX40DOCUMENTS);
	}

	public void TestBox40Documents_Phase5()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

		var bill = header.Bills.AddNew();
		var item = bill.GoodsItems.AddNew();

		var billDocument = bill.PreviousDocuments.AddNew();
		billDocument.CSI_Procedure = "7";
		billDocument.CSI_SubType = "Z";
		billDocument.CSI_Code = "380";
		billDocument.CSI_ReferenceNumber = "34217890";
		billDocument.CSI_DateOfIssue = new ZDateTime(2000, 1, 2);
		billDocument.CSI_Status = "G";
		billDocument.CSI_CustomsOffice = "IT279100";
		billDocument.CSI_LineNo = 1;

		var itemDocument = item.PreviousDocuments.AddNew();
		itemDocument.CSI_Procedure = "7";
		itemDocument.CSI_SubType = "Y";
		itemDocument.CSI_Code = "CLE";
		itemDocument.CSI_ReferenceNumber = "20070701";
		itemDocument.CSI_DateOfIssue = new ZDateTime(2000, 1, 3);
		itemDocument.CSI_Status = "G";
		itemDocument.CSI_CustomsOffice = "IT279100";
		itemDocument.CSI_LineNo = 1;

		var wrapper = NctsDepartureCargoDescWrapper.New(item, Factory);

		AssertEquals(nameof(NctsDepartureCargoDescWrapper.BOX40DOCUMENTS), "380-34217890; CLE-20070701", wrapper.BOX40DOCUMENTS);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return NctsDepartureCargoDescWrapper.New(Factory.New<NctsDepartureCargoDesc>(), Factory);
	}
}

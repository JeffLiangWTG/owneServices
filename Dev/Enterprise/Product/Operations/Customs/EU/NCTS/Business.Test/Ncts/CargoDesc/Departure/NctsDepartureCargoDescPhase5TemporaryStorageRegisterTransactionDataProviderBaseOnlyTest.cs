using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class NctsDepartureCargoDescPhase5TemporaryStorageRegisterTransactionDataProviderBaseOnlyTest : TestCaseWithFactory
{
	public void TestGetTemporaryStorageRegisterTransactionData()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();

		nctsHeader.BH_JobReference = "NCT0000001";
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRN001";

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_PreviousReference = "PR001";

		var regLine1 = regHeader.CusTempStorageRegLines.AddNew();
		regLine1.SRL_LineNumber = 1;

		var regLine2 = regHeader.CusTempStorageRegLines.AddNew();
		regLine2.SRL_LineNumber = 2;

		_ = CreatePreviousDocument(10, 20m, NCTS5PreviousDocumentTypeList.Codes.N337, "PR001", 1);
		_ = CreatePreviousDocument(20, 30m, NCTS5PreviousDocumentTypeList.Codes.N337, "PR001", 1);
		_ = CreatePreviousDocument(30, 40m, NCTS5PreviousDocumentTypeList.Codes.N337, "PR001", 2);
		_ = CreatePreviousDocument(40, 50m, NCTS5PreviousDocumentTypeList.Codes.N337, "PR001", 3);
		_ = CreatePreviousDocument(60, 70m, NCTS5PreviousDocumentTypeList.Codes.N337, "PR002", 1);
		_ = CreatePreviousDocument(70, 80m, NCTS5PreviousDocumentTypeList.Codes.N355, "PR001", 1);

		var transactionData = goodsItem.TemporaryStorageRegisterTransactionDataProvider.GetTemporaryStorageRegisterTransactionData().ToList();
		AssertEquals("No Transaction Data should be created for PreviousDocument4(invalid item number), PreviousDocument5(invalid reference number) and PreviousDocument6(invalid code)", 3, transactionData.Count);

		AssertTransactionData(transactionData[0], "Transaction 1", 1, -20m, -10);
		AssertTransactionData(transactionData[1], "Transaction 2", 1, -30m, -20);
		AssertTransactionData(transactionData[2], "Transaction 3", 2, -40m, -30);

		void AssertTransactionData(TemporaryStorageRegisterTransactionData transactionData, string message, ZInt registerLineNo, ZDecimal grossMass, ZInt packageQuantity)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("PreviousRegisterHeader", regHeader, transactionData.PreviousRegisterHeader);
				AssertEquals("RegisterLineNo", registerLineNo, transactionData.RegisterLineNo);
				AssertEquals("GrossMass", grossMass, transactionData.GrossMass);
				AssertEquals("PackageQuantity", packageQuantity, transactionData.PackageQuantity);
				AssertEquals("CustomsReferenceNumber", "NCT0000001", transactionData.CustomsReferenceNumber);
				AssertEquals("ReferenceType", "DEC", transactionData.ReferenceType);
				AssertEquals("InternalReferenceNumber", "MRN001", transactionData.InternalReferenceNumber);
				AssertEquals("InternalReferenceType", "MRN", transactionData.InternalReferenceType);
			});
		}

		NctsPreviousDocument CreatePreviousDocument(ZInt packQty, ZDecimal quantity, ZString code, ZString referenceNumber, ZInt itemNumber)
		{
			var previousDocument = goodsItem.PreviousDocuments.AddNew();
			previousDocument.CSI_PackQty = packQty;
			previousDocument.CSI_Quantity = quantity;
			previousDocument.CSI_Code = code;
			previousDocument.CSI_ReferenceNumber = referenceNumber;
			previousDocument.CSI_ItemNumber = itemNumber;
			return previousDocument;
		}
	}
}

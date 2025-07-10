using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.CusTempStorage;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing;

[TestedType(typeof(NctsDepartureCargoDescPhase5TemporaryStorageRegisterTransactionDataProvider))]
sealed class NctsDepartureCargoDescPhase5TemporaryStorageRegisterTransactionDataProviderTest : TestCaseWithFactory
{
	public void TestGetTemporaryStorageRegisterTransactionData()
	{
		var register = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
		register.SRH_Reference = "DDT1";

		var ist = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
		ist.SJH_JobReference = "FRJ_IST1";
		ist.DDTNumber = "DDT1";

		var regline1 = register.CusTempStorageRegLines.AddNew();
		regline1.FillWithValidTestData();
		regline1.SRL_LineNumber = 1;
		regline1.SRL_PackageType = "UNT";

		nctsHeader.MovementHeader.BM_PaperlessInbondNum = "NCT0000002";
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "NCTS0001";

		var doc = goodsItem.PreviousDocuments.AddNew();
		doc.CSI_Quantity = 20m;
		doc.CSI_Quantity2 = 40m;
		doc.CSI_UnitOfQuantity2 = "UNT";
		doc.CSI_Code = PreviousDocumentCodeList.Codes._337;
		doc.CSI_ReferenceNumber = "FRJ_IST999";
		doc.CSI_ItemNumber = 999;

		var provider = goodsItem.TemporaryStorageRegisterTransactionDataProvider;
		var transactionData = provider.GetTemporaryStorageRegisterTransactionData().ToList();
		AssertEquals("No TransactionData when CSI_ReferenceNumber does not exist", 0, transactionData.Count);

		doc.CSI_ReferenceNumber = "FRJ_IST1";
		transactionData = provider.GetTemporaryStorageRegisterTransactionData().ToList();
		AssertEquals("No TransactionData when CSI_Code is not N337", 0, transactionData.Count);

		doc.CSI_Code = FRConstants.PreviousDocuments.N337;
		transactionData = provider.GetTemporaryStorageRegisterTransactionData().ToList();
		AssertEquals("No TransactionData when CSI_ItemNumber is not equal to SRL_LineNumber", 0, transactionData.Count);

		doc.CSI_ItemNumber = 1;
		doc.CSI_UnitOfQuantity2 = "UNT2";
		transactionData = provider.GetTemporaryStorageRegisterTransactionData().ToList();
		AssertEquals("No TransactionData when CSI_UnitOfQuantity2 is not equal to SRL_PackageType", 0, transactionData.Count);

		doc.CSI_UnitOfQuantity2 = "UNT";
		transactionData = provider.GetTemporaryStorageRegisterTransactionData().ToList();
		var transaction = transactionData.Single();
		CombineAssertions("Transaction values", () =>
		{
			AssertEquals("PreviousRegisterHeader", register, transaction.PreviousRegisterHeader);
			AssertEquals("CustomsReferenceNumber", "NCTS0001", transaction.CustomsReferenceNumber);
			AssertEquals("InternalReferenceNumber", "NCT0000002", transaction.InternalReferenceNumber);
			AssertEquals("PackageQuantity", 40, transaction.PackageQuantity);
			AssertEquals("ReferenceType", TempStorageTransactionRefTypeList.Codes.NctsHeader, transaction.ReferenceType);
			AssertEquals("GrossMass", 20m, transaction.GrossMass);
			AssertEquals("Comments", "", transaction.Comments);
			AssertEquals("RegisterLineNo", 1, transaction.RegisterLineNo);
		});
	}

	public void TestGetTemporaryStorageRegisterTransactionDataWithMultiplePreviousDocuments()
	{
		var register1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
		register1.SRH_Reference = "DDT1";

		var ist1 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
		ist1.SJH_JobReference = "FRJ_IST1";
		ist1.DDTNumber = "DDT1";

		var regline1 = register1.CusTempStorageRegLines.AddNew();
		regline1.FillWithValidTestData();
		regline1.SRL_LineNumber = 1;
		regline1.SRL_PackageType = "UNT";

		var regline2 = register1.CusTempStorageRegLines.AddNew();
		regline2.FillWithValidTestData();
		regline2.SRL_LineNumber = 2;
		regline2.SRL_PackageType = "CT";

		var register2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
		register2.SRH_Reference = "DDT2";

		var ist2 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
		ist2.SJH_JobReference = "FRJ_IST2";
		ist2.DDTNumber = "DDT2";

		var regline3 = register2.CusTempStorageRegLines.AddNew();
		regline3.FillWithValidTestData();
		regline3.SRL_LineNumber = 1;
		regline3.SRL_PackageType = "RL";

		nctsHeader.MovementHeader.BM_PaperlessInbondNum = "NCT0000002";
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "NCTS0001";

		var pd1 = goodsItem.PreviousDocuments.AddNew();
		pd1.CSI_Quantity = 20m;
		pd1.CSI_Quantity2 = 40m;
		pd1.CSI_UnitOfQuantity2 = "UNT";
		pd1.CSI_Code = FRConstants.PreviousDocuments.N337;
		pd1.CSI_ReferenceNumber = "FRJ_IST1";
		pd1.CSI_ItemNumber = 1;

		var pd2 = goodsItem.PreviousDocuments.AddNew();
		pd2.CSI_Quantity = 20m;
		pd2.CSI_Quantity2 = 40m;
		pd2.CSI_UnitOfQuantity2 = "UNT";
		pd2.CSI_Code = FRConstants.PreviousDocuments.N337;
		pd2.CSI_ReferenceNumber = "FRJ_IST1";
		pd2.CSI_ItemNumber = 1;

		var pd3 = goodsItem.PreviousDocuments.AddNew();
		pd3.CSI_Quantity = 20m;
		pd3.CSI_Quantity2 = 40m;
		pd3.CSI_UnitOfQuantity2 = "CT";
		pd3.CSI_Code = FRConstants.PreviousDocuments.N337;
		pd3.CSI_ReferenceNumber = "FRJ_IST1";
		pd3.CSI_ItemNumber = 2;

		var pd4 = goodsItem.PreviousDocuments.AddNew();
		pd4.CSI_Quantity = 30m;
		pd4.CSI_Quantity2 = 50m;
		pd4.CSI_UnitOfQuantity2 = "RL";
		pd4.CSI_Code = FRConstants.PreviousDocuments.N337;
		pd4.CSI_ReferenceNumber = "FRJ_IST2";
		pd4.CSI_ItemNumber = 1;

		var transactionData = goodsItem.TemporaryStorageRegisterTransactionDataProvider.GetTemporaryStorageRegisterTransactionData().ToList();
		AssertEquals("3 transactions should be created grouped by CSI_ReferenceNumber and CSI_ItemNumber", 3, transactionData.Count);

		var transaction1 = transactionData.Single(x => x.PreviousRegisterHeader == register1 && x.RegisterLineNo == pd1.CSI_ItemNumber);
		CombineAssertions("Transaction 1 values", () =>
		{
			AssertEquals("PreviousRegisterHeader", register1, transaction1.PreviousRegisterHeader);
			AssertEquals("CustomsReferenceNumber", "NCTS0001", transaction1.CustomsReferenceNumber);
			AssertEquals("InternalReferenceNumber", "NCT0000002", transaction1.InternalReferenceNumber);
			AssertEquals("PackageQuantity", 80, transaction1.PackageQuantity);
			AssertEquals("ReferenceType", TempStorageTransactionRefTypeList.Codes.NctsHeader, transaction1.ReferenceType);
			AssertEquals("GrossMass", 40m, transaction1.GrossMass);
			AssertEquals("Comments", "", transaction1.Comments);
			AssertEquals("RegisterLineNo", 1, transaction1.RegisterLineNo);
		});

		var transaction2 = transactionData.Single(x => x.RegisterLineNo == pd3.CSI_ItemNumber);
		CombineAssertions("Transaction 2 values", () =>
		{
			AssertEquals("PreviousRegisterHeader", register1, transaction2.PreviousRegisterHeader);
			AssertEquals("CustomsReferenceNumber", "NCTS0001", transaction2.CustomsReferenceNumber);
			AssertEquals("InternalReferenceNumber", "NCT0000002", transaction2.InternalReferenceNumber);
			AssertEquals("PackageQuantity", 40, transaction2.PackageQuantity);
			AssertEquals("ReferenceType", TempStorageTransactionRefTypeList.Codes.NctsHeader, transaction2.ReferenceType);
			AssertEquals("GrossMass", 20m, transaction2.GrossMass);
			AssertEquals("Comments", "", transaction2.Comments);
			AssertEquals("RegisterLineNo", 2, transaction2.RegisterLineNo);
		});

		var transaction3 = transactionData.Single(x => x.PreviousRegisterHeader == register2 && x.RegisterLineNo == pd4.CSI_ItemNumber);
		CombineAssertions("Transaction 3 values", () =>
		{
			AssertEquals("PreviousRegisterHeader", register2, transaction3.PreviousRegisterHeader);
			AssertEquals("CustomsReferenceNumber", "NCTS0001", transaction3.CustomsReferenceNumber);
			AssertEquals("InternalReferenceNumber", "NCT0000002", transaction3.InternalReferenceNumber);
			AssertEquals("PackageQuantity", 50, transaction3.PackageQuantity);
			AssertEquals("ReferenceType", TempStorageTransactionRefTypeList.Codes.NctsHeader, transaction3.ReferenceType);
			AssertEquals("GrossMass", 30m, transaction3.GrossMass);
			AssertEquals("Comments", "", transaction3.Comments);
			AssertEquals("RegisterLineNo", 1, transaction3.RegisterLineNo);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		Factory.Save();
	}

	NctsHeader nctsHeader;
	NctsDepartureCargoDesc goodsItem;
}

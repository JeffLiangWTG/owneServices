using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.CusTempStorage;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing;

[TestedType(typeof(NctsDepartureCargoDescPhase4TemporaryStorageRegisterTransactionDataProvider))]
sealed class NctsDepartureCargoDescPhase4TemporaryStorageRegisterTransactionDataProviderTest : TestCaseWithFactory
{
	public void TestGetTemporaryStorageRegisterTransactionData()
	{
		var register = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
		register.SRH_Reference = "DDT1";

		var ist = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
		ist.SJH_JobReference = "FRJ_IST1";
		ist.DDTNumber = "DDT1";

		nctsHeader.BH_JobReference = "NCT0000002";
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "NCTS0001";

		goodsItem.BY_GrossWeight = 20m;
		var package1 = goodsItem.Packages.AddNew();
		package1.B5_UnitCount = 40;
		var doc = goodsItem.PreviousDocuments.AddNew();
		doc.CSI_Code = PreviousDocumentCodeList.Codes._337;
		doc.CSI_ReferenceNumber = "FRJ_IST1";
		doc.CSI_LineNo = 1;

		var transactionData1 = goodsItem.TemporaryStorageRegisterTransactionDataProvider.GetTemporaryStorageRegisterTransactionData();
		var transaction1 = transactionData1.Single();
		CombineAssertions("Transaction values", () =>
		{
			AssertEquals("PreviousRegisterHeader", register, transaction1.PreviousRegisterHeader);
			AssertEquals("CustomsReferenceNumber", "NCTS0001", transaction1.CustomsReferenceNumber);
			AssertEquals("InternalReferenceNumber", "NCT0000002", transaction1.InternalReferenceNumber);
			AssertEquals("PackageQuantity", 40, transaction1.PackageQuantity);
			AssertEquals("ReferenceType", TempStorageTransactionRefTypeList.Codes.NctsHeader, transaction1.ReferenceType);
			AssertEquals("GrossMass", 20m, transaction1.GrossMass);
			AssertEquals("Comments", "", transaction1.Comments);
			AssertEquals("RegisterLineNo", 1, transaction1.RegisterLineNo);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		Factory.Save();
	}

	NctsHeader nctsHeader;
	NctsDepartureCargoDesc goodsItem;
}

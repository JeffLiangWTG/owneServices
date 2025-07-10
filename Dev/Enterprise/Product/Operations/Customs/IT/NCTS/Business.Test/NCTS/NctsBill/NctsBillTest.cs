using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsBill))]
sealed class NctsBillTest : EnterpriseBusinessObjectTestCase
{
	public void TestB0_BillStatus_CaptionAndDescription()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var nctsBill = nctsHeader.Bills.AddNew();

		AssertEntity<NctsBill>()
			.HasProperty(x => x.B0_BillStatus)
			.WithCaption("Status")
			.WithFullDescription("House Consignment Status");
	}

	public void TestSetAsCustomsDeletionRequest()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var nctsBill = nctsHeader.Bills.AddNew();

		nctsBill.B0_BillStatus = ZString.Empty;

		nctsBill.SetAsCustomsDeletionRequest();

		AssertEquals("status", NctsDeletionStatusList.Codes.DeletionRequest, nctsBill.B0_BillStatus);
	}

	public void TestClearCustomsDeletionStatus()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var nctsBill = nctsHeader.Bills.AddNew();

		nctsBill.B0_BillStatus = NctsDeletionStatusList.Codes.Deleted;

		nctsBill.ClearCustomsDeletionStatus();

		AssertEquals("status", ZString.Empty, nctsBill.B0_BillStatus);
	}

	public void TestIsCustomsStatusDeleted()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var nctsBill = nctsHeader.Bills.AddNew();

		nctsBill.B0_BillStatus = NctsDeletionStatusList.Codes.Deleted;

		AssertEquals("status", true, nctsBill.IsCustomsStatusDeleted);

		nctsBill.B0_BillStatus = NctsDeletionStatusList.Codes.DeletionRequest;

		AssertEquals("status", false, nctsBill.IsCustomsStatusDeleted);
	}

	public void TestIsCustomsStatusDeletionRequested()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var nctsBill = nctsHeader.Bills.AddNew();

		nctsBill.B0_BillStatus = NctsDeletionStatusList.Codes.DeletionRequest;

		AssertEquals("status", true, nctsBill.IsCustomsStatusDeletionRequested);

		nctsBill.B0_BillStatus = NctsDeletionStatusList.Codes.Deleted;

		AssertEquals("status", false, nctsBill.IsCustomsStatusDeletionRequested);
	}

	public void TestB0_BillStatusReadOnly()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		var nctsDepartureMovementHeader = nctsHeader.MovementHeader;
		var nctsBill = nctsHeader.Bills.AddNew();

		AssertEquals("status.ReadOnly", true, nctsBill.B0_BillStatusInfo.ReadOnly);
	}

	public void TestSupportingDocumentsType()
	{
		var nctsBill = Factory.New<NctsBill>();
		AssertType<NctsSupportingDocumentCollection<NctsSupportingDocument>>("SupportingDocuments Type", nctsBill.SupportingDocuments);
	}

	public void TestTransportTypeAtDepartureParsed()
	{
		var nctsBill = CreatePhase5NctsBill();
		var header = nctsBill.Header;
		var moveHeader = header.MovementHeader;
		var borderTransportMeans = nctsBill.DepartureTransportInfos.FirstOrDefault() ?? nctsBill.DepartureTransportInfos.AddNew();
		borderTransportMeans.TPM_TypeOfIdentification = string.Empty;

		AssertEquals("Empty when TPM_TypeOfIdentification empty.", 0, nctsBill.TransportTypeAtDepartureParsed);

		borderTransportMeans.TPM_TypeOfIdentification = "X";
		AssertEquals("Empty when TPM_TypeOfIdentification invalid.", 0, nctsBill.TransportTypeAtDepartureParsed);

		borderTransportMeans.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._10;
		AssertEquals("Convert from TPM_TypeOfIdentification.", 10, nctsBill.TransportTypeAtDepartureParsed);
	}

	public void TestGoodsItemsType()
	{
		var nctsBill = Factory.New<NctsBill>();
		AssertType<NctsDepartureCargoDescCollection>("GoodsItems Type", nctsBill.GoodsItems);
	}

	public void TestPreviousDocumentsType()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var nctsBill = nctsHeader.Bills.AddNew();
		AssertType<CommonPreviousDocumentCollection<CommonPreviousDocument>>("PreviousDocuments Type", nctsBill.PreviousDocuments);
	}

	public void TestCanDelete()
	{
		var nctsBill = CreatePhase5NctsBill();
		var header = nctsBill.Header;
		var moveHeader = header.MovementHeader;

		CombineAssertions(() =>
		{
			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			AssertEquals("In declaration state", true, nctsBill.CanDelete);

			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Amendment;
			AssertEquals("In amendment state", false, nctsBill.CanDelete);
		});
	}

	public void TestReasonForNotAbleToDelete()
	{
		const string expectedMessage = "It is not possible to delete a House Consignment in Amendment phase. You can request a deletion of House Consignment setting its Status to DLR (deletion request).";
		var nctsBill = CreatePhase5NctsBill();
		var header = nctsBill.Header;
		var moveHeader = header.MovementHeader;

		moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Amendment;
		AssertEquals(expectedMessage, nctsBill.ReasonForNotAbleToDelete);

		moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
		AssertNotEquals(expectedMessage, nctsBill.ReasonForNotAbleToDelete);
	}

	protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest) => new LightValidationTesterExcludingJobDocAddress(bizObjToTest);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateBill(factory);

	protected override BusinessObject GetNewBusinessObject() => CreateBill(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => CreateBill(Factory);

	NctsBill CreateBill(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.NewDepartureNctsHeader();
		return nctsHeader.Bills.AddNew();
	}

	NctsBill CreatePhase5NctsBill()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		return nctsHeader.Bills.AddNew();
	}

	class LightValidationTesterExcludingJobDocAddress(BusinessObject bo) : LightValidationTester(bo)
	{
		protected override bool ShouldTestProperty(ZPropertyInfo info) =>
			info.BizObj is not MasterFiles.Business.JobDocAddress { DocAddressType: DocAddressType.ConsigneeAddress }
			&& base.ShouldTestProperty(info);
	}
}

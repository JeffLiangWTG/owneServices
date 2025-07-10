using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

[TestedType(typeof(ArrivalCusTransportMeans))]
sealed class ArrivalCusTransportMeansTest : EnterpriseBusinessObjectTestCase
{
	public void TestTPM_SequenceNumber()
	{
		NCTSTestHelper.AssertCaptions(arrivalCusTransportMeans.TPM_SequenceNumberInfo, "Sequence Number", "Sequence No.", "Seq.No.");
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => arrivalCusTransportMeans;

	protected override BusinessObject GetNewBusinessObject() => arrivalCusTransportMeans;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => arrivalCusTransportMeans;

	protected override void SetUp()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var nctsBill = nctsHeader.Bills.AddNew();
		arrivalCusTransportMeans = nctsBill.ArrivalTransportInfos.AddNew();
	}
	ArrivalCusTransportMeans arrivalCusTransportMeans;
}

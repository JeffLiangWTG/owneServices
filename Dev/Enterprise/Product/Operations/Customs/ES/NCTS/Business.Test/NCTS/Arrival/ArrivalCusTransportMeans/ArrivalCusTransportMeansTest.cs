using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(ArrivalCusTransportMeans))]
	sealed class ArrivalCusTransportMeansTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTPM_TypeOfIdentification_ReadOnly_DIF()
		{
			arrivalCusTransportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;
			AssertEquals(false, arrivalCusTransportMeans.TPM_TypeOfIdentificationInfo.ReadOnly);
		}

		public void TestTPM_IdentificationNumber_ReadOnly_DIF()
		{
			arrivalCusTransportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;
			AssertEquals(false, arrivalCusTransportMeans.TPM_IdentificationNumberInfo.ReadOnly);
		}

		public void TestTPM_RN_NKTransportNationality_ReadOnly_DIF()
		{
			arrivalCusTransportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;
			AssertEquals(false, arrivalCusTransportMeans.TPM_RN_NKTransportNationalityInfo.ReadOnly);
		}

		public void TestLookups()
		{
			AssertType<ArrivalCusTransportMeansLookups>(arrivalCusTransportMeans.Lookups);
		}

		public void TestArrivalCusTransportMeansForMovementHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
			var arrivalCusTransportMeans = arrivalMovementHeader.ArrivalTransportInfos.AddNew();
			AssertType<ArrivalCusTransportMeans>(arrivalCusTransportMeans);
		}

		public void TestArrivalCusTransportMeansForBill()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var nctsBill = nctsHeader.Bills.AddNew();
			var arrivalCusTransportMeans = nctsBill.ArrivalTransportInfos.AddNew();
			AssertType<ArrivalCusTransportMeans>(arrivalCusTransportMeans);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => arrivalCusTransportMeans;

		protected override BusinessObject GetNewBusinessObject() => arrivalCusTransportMeans;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => arrivalCusTransportMeans;

		protected override void SetUp()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
			arrivalCusTransportMeans = arrivalMovementHeader.ArrivalTransportInfos.AddNew();
			arrivalMovementHeader.BM_NoChangesToReport = false;
		}
		ArrivalCusTransportMeans arrivalCusTransportMeans;
	}
}

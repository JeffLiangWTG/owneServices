using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
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

		public void TestTPM_TypeOfIdentiFication_IdentificationNumber_RN_NK_TransportNationality_IsSet()
		{
			arrivalCusTransportMeans.TPM_TypeOfIdentification = "10";
			arrivalCusTransportMeans.TPM_IdentificationNumber = "123456";
			arrivalCusTransportMeans.TPM_RN_NKTransportNationality = "DE";
			arrivalCusTransportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;
			AssertEquals(ZString.Empty,arrivalCusTransportMeans.TPM_RN_NKTransportNationality);
			AssertEquals(ZString.Empty, arrivalCusTransportMeans.TPM_TypeOfIdentification);
			AssertEquals(ZString.Empty, arrivalCusTransportMeans.TPM_IdentificationNumber);
		}

		public void TestLookups()
		{
			AssertType<ArrivalCusTransportMeansLookups>(arrivalCusTransportMeans.Lookups);
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
}

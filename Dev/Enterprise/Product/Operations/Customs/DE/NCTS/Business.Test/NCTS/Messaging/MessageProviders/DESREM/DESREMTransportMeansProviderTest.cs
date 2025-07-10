using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class DESREMTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<DESREMTransportMeansProvider>
	{
		public void TestConstructor_NullArgument()
		{
			AssertNull(DESREMTransportMeansProvider.NewOrNull(null));
		}

		public void TestSequenceNumber()
		{
			transportMeans.TPM_SequenceNumber = 2;
			AssertEquals(2, Provider.SequenceNumber);
		}

		public void TestTypeOfIdentification_NEW()
		{
			transportMeans.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._31;
			transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("31", Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentification_DIF()
		{
			transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;
			transportMeans.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._31;
			AssertEquals("31", Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentification_MIS()
		{
			transportMeans.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._31;
			transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
			AssertNull(Provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber_NEW()
		{
			transportMeans.TPM_IdentificationNumber = "1234";
			transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("1234", Provider.IdentificationNumber);
		}

		public void TestIdentificationNumber_DIF()
		{
			transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;
			transportMeans.TPM_IdentificationNumber = "1234";
			AssertEquals("1234", Provider.IdentificationNumber);
		}

		public void TestIdentificationNumber_MIS()
		{
			transportMeans.TPM_IdentificationNumber = "1234";
			transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
			AssertNull(Provider.IdentificationNumber);
		}

		public void TestNationality_NEW()
		{
			transportMeans.TPM_RN_NKTransportNationality = "DE";
			transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("DE", Provider.Nationality);
		}

		public void TestNationality_DIF()
		{
			transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;
			transportMeans.TPM_RN_NKTransportNationality = "DE";
			AssertEquals("DE", Provider.Nationality);
		}

		public void TestNationality_MIS()
		{
			transportMeans.TPM_RN_NKTransportNationality = "DE";
			transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
			AssertNull(Provider.Nationality);
		}

		protected override DESREMTransportMeansProvider GetProvider() => DESREMTransportMeansProvider.NewOrNull(transportMeans);

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			transportMeans = header.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();
		}
		ArrivalCusTransportMeans transportMeans;

		new IDESREMTransportMeans Provider => base.Provider;
	}
}

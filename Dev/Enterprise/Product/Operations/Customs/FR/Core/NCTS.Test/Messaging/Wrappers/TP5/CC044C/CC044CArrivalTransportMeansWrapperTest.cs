using System;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing;

[TestedType(typeof(CC044CArrivalTransportMeansWrapper))]
sealed class CC044CArrivalTransportMeansWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC044CArrivalTransportMeansWrapper>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new CC044CArrivalTransportMeansWrapper(null));

	public void TestTypeOfIdentification()
	{
		transportMeans.TPM_TransportState = "XXX";
		transportMeans.TPM_TypeOfIdentification = "3";
		AssertNullOrEmpty("TypeOfIdentification should be null when TPM_TransportState is not New", Provider.TypeOfIdentification);
	}

	public void TestIdentificationNumber_NEW()
	{
		transportMeans.TPM_TransportState = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
		transportMeans.TPM_IdentificationNumber = "DepTranMeansID";
		AssertEquals("IdentificationNumber should be equal to TPM_IdentificationNumber when TPM_TransportState is New", "DepTranMeansID", Provider.IdentificationNumber);
	}

	public void TestIdentificationNumber()
	{
		transportMeans.TPM_TransportState = "XXX";
		transportMeans.TPM_IdentificationNumber = "DepTranMeansID";
		AssertNullOrEmpty("IdentificationNumber should be null when TPM_TransportState is not New", Provider.IdentificationNumber);
	}

	public void TestTypeOfIdentification_NEW()
	{
		transportMeans.TPM_TransportState = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
		transportMeans.TPM_TypeOfIdentification = "3";
		AssertEquals("TypeOfIdentification should be equal to TPM_TypeOfIdentification when TPM_TransportState is New", "3", Provider.TypeOfIdentification);
	}

	public void TestNationality()
	{
		transportMeans.TPM_TransportState = "XXX";
		transportMeans.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.France;
		AssertNullOrEmpty("Nationality should be null when TPM_TransportState is not New", Provider.Nationality);
	}

	public void TestNationality_NEW()
	{
		transportMeans.TPM_TransportState = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
		transportMeans.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.France;
		AssertEquals("Nationality should be equal to TPM_RN_NKTransportNationality when TPM_TransportState is New", Core.Constants.CountryCodes.France, Provider.Nationality);
	}

	protected override CC044CArrivalTransportMeansWrapper GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		transportMeans = Factory.New<ArrivalCusTransportMeans>();
		provider = new CC044CArrivalTransportMeansWrapper(transportMeans);
	}

	CC044CArrivalTransportMeansWrapper provider;
	ArrivalCusTransportMeans transportMeans;
}

using System;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class ArrivalMeansOfTransportWrapperTest : TestCaseWithFactory
{
	public void TestNewOrNull()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => ArrivalMeansOfTransportWrapper.NewOrNull(null));
		AssertNull(nameof(ArrivalMeansOfTransportWrapper.NewOrNull), ArrivalMeansOfTransportWrapper.NewOrNull(declaration));
	}

	public void TestIdentificationNumber()
	{
		declaration.ZG_Box18TransportID = "IDENTIFICATION NUMBER";
		var arrivalMeansOfTransport = GetArrivalMeansOfTransport();
		AssertEquals(nameof(IArrivalMeansOfTransport.IdentificationNumber), "IDENTIFICATION NUMBER", arrivalMeansOfTransport.IdentificationNumber);
	}

	public void TestTransportMode()
	{
		declaration.JE_TransportMeans = "10";
		var arrivalMeansOfTransport = GetArrivalMeansOfTransport();
		AssertEquals(nameof(IArrivalMeansOfTransport.TransportMode), 10, arrivalMeansOfTransport.TransportMode);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
	}

	JobDeclaration declaration;

	IArrivalMeansOfTransport GetArrivalMeansOfTransport() => ArrivalMeansOfTransportWrapper.NewOrNull(declaration);
}

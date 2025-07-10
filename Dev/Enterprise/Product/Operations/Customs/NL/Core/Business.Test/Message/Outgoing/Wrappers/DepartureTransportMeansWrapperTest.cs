using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

class DepartureTransportMeansWrapperTest : DataProviderTestCase<DepartureTransportMeansWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => DepartureTransportMeansWrapper.New(null, 0, ZString.Empty, ZString.Empty, ZString.Empty));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals("SequenceNumeric", 1, wrapper.SequenceNumeric);
	}

	public void TestIdentificationTypeCode()
	{
		AssertEquals("IdentificationTypeCode", "10", wrapper.IdentificationTypeCode);
	}

	public void TestId()
	{
		AssertEquals("IdentificationNumber", "1312", wrapper.Id);
	}

	public void TestRegistrationNationalityCode()
	{
		AssertEquals("RegistrationNationalityCode", Core.Constants.CountryCodes.Netherlands, wrapper.RegistrationNationalityCode);
	}

	public void TestNationality()
	{
		AssertEquals("Nationality", string.Empty, wrapper.Nationality);
	}

	public void TestModeCode()
	{
		declaration.JE_TransportModeInland = "ROA";
		AssertEquals("ModeCode", ModeOfTransportCodeList.Descriptions._ROA.ToString(), wrapper.ModeCode);
	}

	public void TestEmptyPropertyObjectCreation()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_TransportMeans = "10";
		declaration.JE_TransportIDInland = "1312";
		declaration.JE_RN_NKTransportNationalityInland = "NL";
		declaration.JE_TransportModeInland = "ROA";
		var wrapper = DepartureTransportMeansWrapper.New(declaration, 1, declaration.JE_TransportIDInland, declaration.JE_TransportMeans, declaration.JE_RN_NKTransportNationalityInland);
		AssertNotNull("Object not null", wrapper);

		declaration.JE_TransportMeans = string.Empty;
		wrapper = DepartureTransportMeansWrapper.New(declaration, 1, declaration.JE_TransportIDInland, declaration.JE_TransportMeans, declaration.JE_RN_NKTransportNationalityInland);
		AssertNotNull("Object not null even if partially mapped", wrapper);

		declaration.JE_TransportMeans = string.Empty;
		declaration.JE_TransportIDInland = string.Empty;
		declaration.JE_RN_NKTransportNationalityInland = string.Empty;
		declaration.JE_TransportModeInland = string.Empty;
		wrapper = DepartureTransportMeansWrapper.New(declaration, 1, declaration.JE_TransportIDInland, declaration.JE_TransportMeans, declaration.JE_RN_NKTransportNationalityInland);
		AssertNull("Object null as all properties not initialised with data", wrapper);
	}

	protected override DepartureTransportMeansWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_TransportMeans = "10";
		declaration.JE_TransportIDInland = "1312";
		declaration.JE_RN_NKTransportNationalityInland = "NL";
		wrapper = DepartureTransportMeansWrapper.New(declaration, 1, declaration.JE_TransportIDInland, declaration.JE_TransportMeans, declaration.JE_RN_NKTransportNationalityInland);
	}
	DepartureTransportMeansWrapper wrapper;
	JobDeclaration declaration;
}

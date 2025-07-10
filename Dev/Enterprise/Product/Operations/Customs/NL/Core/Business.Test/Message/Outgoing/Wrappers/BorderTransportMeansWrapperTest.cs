using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class BorderTransportMeansWrapperTest : DataProviderTestCase<BorderTransportMeansWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new BorderTransportMeansWrapper(null));
	}

	public void TestRegistrationNationalityCode()
	{
		AssertEquals(Core.Constants.CountryCodes.Netherlands, wrapper.RegistrationNationalityCode);
	}

	public void TestModeCode()
	{
		AssertEquals("ModeCode", ModeOfTransportCodeList.Descriptions._ROA.ToString(), wrapper.ModeCode);
	}

	public void TestId()
	{
		AssertEquals("ID", "VOYAGER", wrapper.Id);
	}

	public void TestIdentificationType()
	{
		AssertEquals("IdentificationType", "81", wrapper.IdentificationType);
	}

	protected override BorderTransportMeansWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		var decl = Factory.New<JobDeclaration>();
		decl.JE_TransportMode = "ROA";
		decl.JE_VesselName = "VOYAGER";
		decl.ZG_BorderTransportMeans = "81";
		decl.JE_RN_NKTransportNationality = "NL";
		wrapper = new BorderTransportMeansWrapper(decl);
	}
	BorderTransportMeansWrapper wrapper;
}

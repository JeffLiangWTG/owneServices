using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.ES.Business.Testing;

class TransportMediumInfoCommonWrapperTest : WrapperHelperTest<TransportMediumInfoCommonWrapper>
{
	public void TestTransportMode()
	{
		wrapper = new TransportMediumInfoCommonWrapper(NctsTransportData1.Mode, ZString.Empty, ZString.Empty, declaration);
		AssertEquals("Expected filled TransportMode.", NctsTransportData1.Mode, wrapper.TransportMode);

		wrapper = new TransportMediumInfoCommonWrapper(NctsTransportData1.Mode, ZString.Empty, ZString.Empty);
		AssertEquals("Expected filled TransportMode when declaration is null.", NctsTransportData1.Mode, wrapper.TransportMode);
	}

	public void TestTransportId()
	{
		CombineAssertions(() =>
		{
			wrapper = new TransportMediumInfoCommonWrapper(ZString.Empty, NctsTransportData1.Id, ZString.Empty, declaration);
			AssertEquals("Expected filled TransportId when id is one word", NctsTransportData1.Id, wrapper.TransportId);

			wrapper = new TransportMediumInfoCommonWrapper(ZString.Empty, NctsTransportData1.Id, ZString.Empty);
			AssertEquals("Expected filled TransportId when id is one word and declaration is null.", NctsTransportData1.Id, wrapper.TransportId);
		});
	}

	public void TestTransportNationality()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", false);
		helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "RS", "XS", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
		helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "MQ", "FR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
		Factory.Save();

		CombineAssertions(() =>
		{
			wrapper = new TransportMediumInfoCommonWrapper(ZString.Empty, ZString.Empty, "RS", declaration);
			AssertEquals("Expected filled Country with Default Territory (XS)", "XS", wrapper.TransportNationality);

			wrapper = new TransportMediumInfoCommonWrapper(ZString.Empty, ZString.Empty, "ES", declaration);
			AssertEquals("Expected filled Country with given code since there is no Default Territory", "ES", wrapper.TransportNationality);

			wrapper = new TransportMediumInfoCommonWrapper(ZString.Empty, ZString.Empty, "MQ", declaration);
			AssertEquals("Expected filled Country with Default Territory (FR)", "FR", wrapper.TransportNationality);
		});

		CombineAssertions("TransportNationality should be same as provided if declaration is null.", () =>
		{
			wrapper = new TransportMediumInfoCommonWrapper(ZString.Empty, ZString.Empty, "RS");
			AssertEquals("TransportNationality should return RS as declaration is not provieded.", "RS", wrapper.TransportNationality);

			wrapper = new TransportMediumInfoCommonWrapper(ZString.Empty, ZString.Empty, "ES");
			AssertEquals("Expected filled Country with given code since there is no Default Territory or declaration is null", "ES", wrapper.TransportNationality);

			wrapper = new TransportMediumInfoCommonWrapper(ZString.Empty, ZString.Empty, "MQ");
			AssertEquals("TransportNationality should return MQ as declaration is not provieded.", "MQ", wrapper.TransportNationality);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		wrapper = new TransportMediumInfoCommonWrapper(ZString.Empty, ZString.Empty, ZString.Empty, declaration);
	}

	TransportMediumInfoCommonWrapper wrapper;
	JobDeclaration declaration;

	protected override TransportMediumInfoCommonWrapper GetProvider() => wrapper;
}

using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing;

public class CommonDepartureTransportMeansWrapperTest : WrapperHelperTest<CommonDepartureTransportMeansWrapper>
{
	public void TestSequenceNumber()
	{
		AssertEquals("Expected filled SequenceNumber", "1", wrapper.SequenceNumber);
	}

	public void TestDeclarationIsOptional()
	{
		wrapper = new CommonDepartureTransportMeansWrapper("Mode", "ID", "Nationality", 1);

		CombineAssertions("Expected to work with no declaration provided.", () =>
		{
			AssertEquals("Expected filled SequenceNumber.", "1", wrapper.SequenceNumber);
			AssertEquals("Expected TransportMode filled correctly", "Mode", wrapper.TransportMode);
			AssertEquals("Expected TransportId filled correctly", "ID", wrapper.TransportId);
			AssertEquals("Expected TransportNationality filled correctly", "Nationality", wrapper.TransportNationality);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		wrapper = new CommonDepartureTransportMeansWrapper(ZString.Empty, ZString.Empty, ZString.Empty, 1, declaration);
	}

	CommonDepartureTransportMeansWrapper wrapper;

	protected override CommonDepartureTransportMeansWrapper GetProvider() => wrapper;
}

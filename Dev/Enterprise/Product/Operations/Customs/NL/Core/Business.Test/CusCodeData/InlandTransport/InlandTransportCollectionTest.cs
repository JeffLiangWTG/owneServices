using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(InlandTransportCollection))]
sealed class InlandTransportCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestMaxCount()
	{
		AssertEquals(1, declaration.InlandTransports.MaxCount);
	}

	public void TestMaxCount_Road()
	{
		declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
		AssertEquals(3, declaration.InlandTransports.MaxCount);
	}

	public void TestMaxCount_OtherInlandTransport()
	{
		declaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
		AssertEquals(1, declaration.InlandTransports.MaxCount);
	}

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var parent = Factory.New<JobDeclaration>();
		return new InlandTransportCollection(parent);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;
}

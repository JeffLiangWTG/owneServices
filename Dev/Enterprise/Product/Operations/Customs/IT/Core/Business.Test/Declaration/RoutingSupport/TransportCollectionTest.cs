using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(TransportCollection))]
sealed class TransportCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestDefaultingLegOrderForNewElements()
	{
		var declaration = Factory.New<JobDeclaration>();
		var transportCollection = new TransportCollection(declaration);

		var transport1 = transportCollection.AddNew();
		AssertEquals("First Transport Leg Order", (byte)1, transport1.JW_LegOrder);

		var transport2 = transportCollection.AddNew();
		AssertEquals("Second Transport Leg Order", (byte)2, transport2.JW_LegOrder);

		transportCollection.AddNew().JW_LegOrder = 9;
		var transportX = transportCollection.AddNew();
		AssertEquals("Last Transport Leg Order", (byte)10, transportX.JW_LegOrder);

		transport2.JW_LegOrder = 255;
		AssertEquals("When existing leg has reached the maximum leg order value (255), LegOrder for a new one", (byte)255, transportCollection.AddNew().JW_LegOrder);
	}

	protected override BusinessObjectCollection GetCollectionToTest() => Factory.New<JobDeclaration>().Transports;
}


using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CHEDIMessageCollection))]
public class CHEDIMessageCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestAdditionalFilter()
	{
		var master = Factory.New<DummyBusinessObject>();
		var message1 = Factory.New<CHEDIMessage>();
		message1.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CHCustomsEdec;
		message1.EM_LinkedObject = master;
		var message2 = Factory.New<CHEDIMessage>();
		message2.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CHCustomsPassar;
		message2.EM_LinkedObject = master;
		var message3 = Factory.New<CHEDIMessage>();
		message3.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CHCustomsCharteraOutput;
		message3.EM_LinkedObject = master;
		var otherMessage = Factory.New<CHEDIMessage>();
		otherMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.UniversalDataMessaging;
		otherMessage.EM_LinkedObject = master;

		var collection = new CHEDIMessageCollection(master);
		collection.Load();
		AssertContainsExactElementsInAnyOrder(new[] { message1, message2, message3 }, collection);
	}

	protected override BusinessObjectCollection GetCollectionToTest() => GetNewMessageCollection();

	CHEDIMessageCollection GetNewMessageCollection() => new CHEDIMessageCollection(Factory.New<DummyBusinessObject>());
}

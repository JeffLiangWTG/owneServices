using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(MessageManagerFactory))]
sealed class MessageManagerFactoryTest : TestCaseWithFactory
{
	public void TestCreateNew_Null() => AssertNull(MessageManagerFactory.CreateNew(null));

	public void TestCreateNewDeparture() => CombineAssertions(() =>
	{
		var nctsDepartuerHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsDepartuerHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var departureSendingObject = new NctsHeaderDepartureMessageSendingObject(nctsDepartuerHeader);

		departureSendingObject.MessageType = PassarMessageTypeList.Codes.NT013;
		AssertTypeForSendingObject<NT013MessageManager>(departureSendingObject, $"MessageType={departureSendingObject.MessageType}");
		departureSendingObject.MessageType = PassarMessageTypeList.Codes.NT014;
		AssertTypeForSendingObject<NT014MessageManager>(departureSendingObject, $"MessageType={departureSendingObject.MessageType}");
		departureSendingObject.MessageType = PassarMessageTypeList.Codes.NT015;
		AssertTypeForSendingObject<NT015MessageManager>(departureSendingObject, $"MessageType={departureSendingObject.MessageType}");
		departureSendingObject.MessageType = PassarMessageTypeList.Codes.NT141;
		AssertTypeForSendingObject<NT141MessageManager>(departureSendingObject, $"MessageType={departureSendingObject.MessageType}");
		departureSendingObject.MessageType = PassarMessageTypeList.Codes.NT513;
		AssertTypeForSendingObject<NT513MessageManager>(departureSendingObject, $"MessageType={departureSendingObject.MessageType}");
		departureSendingObject.MessageType = PassarMessageTypeList.Codes.NT515;
		AssertTypeForSendingObject<NT515MessageManager>(departureSendingObject, $"MessageType={departureSendingObject.MessageType}");
		departureSendingObject.MessageType = PassarMessageTypeList.Codes.NC016;
		AssertTypeForSendingObject<NC016MessageManager>(departureSendingObject, $"MessageType={departureSendingObject.MessageType}");
		departureSendingObject.MessageType = PassarMessageTypeList.Codes.NC123;
		AssertTypeForSendingObject<NC123MessageManager>(departureSendingObject, $"MessageType={departureSendingObject.MessageType}");
	});

	public void TestCreateNewArrival() => CombineAssertions(() =>
	{
		var nctsArrivalHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsArrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var arrivalSendingObject = new NctsHeaderArrivalMessageSendingObject(nctsArrivalHeader);

		arrivalSendingObject.MessageType = PassarMessageTypeList.Codes.NT007;
		AssertTypeForSendingObject<NT007MessageManager>(arrivalSendingObject, $"MessageType={arrivalSendingObject.MessageType}");
		arrivalSendingObject.MessageType = PassarMessageTypeList.Codes.NT044;
		AssertTypeForSendingObject<NT044MessageManager>(arrivalSendingObject, $"MessageType={arrivalSendingObject.MessageType}");
	});

	public void TestCreateNewCharteraDocumentSearch()
	{
		var sendingObject = new CharteraOutputDocumentSearchSendingObject(Factory);
		AssertTypeForSendingObject<CharteraOutputDocumentSearchRequestMessageManager>(sendingObject);
	}

	void AssertTypeForSendingObject<T>(IMessageSendingObject sendingObject, string assertionMessage = "") => AssertType<T>($"Type={sendingObject.GetType().Name} {assertionMessage}", MessageManagerFactory.CreateNew(sendingObject));
}

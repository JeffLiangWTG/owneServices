using System.Linq;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC556C;
using CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC917C;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class InboundProviderHelperTest : TestCaseWithFactory
{
	public void TestNameSpacesIgnored()
	{
		var messageInvalid = Factory.New<BEMessage>();
		messageInvalid.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		messageInvalid.EM_MessageText = embeddedResourceRetriever.GetString("Enterprise.Customs.BE.Business.Testing.Messaging.Incoming.MessageProviders.TestFiles.InvalidNameSpaceMessage.txt");

		AssertNoExceptionThrown(() => messageInvalid.GetCachedInboundProvider<Cc917CType, CC917CDataProvider>());
	}

	public void TestXSDValidation()
	{
		var messageValid = Factory.New<BEMessage>();
		var messageInvalid = Factory.New<BEMessage>();
		messageValid.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		messageInvalid.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		messageValid.EM_MessageText = embeddedResourceRetriever.GetString("Enterprise.Customs.BE.Business.Testing.Messaging.Incoming.MessageProviders.TestFiles.ValidXSDValidationMessage.txt");
		messageInvalid.EM_MessageText = embeddedResourceRetriever.GetString("Enterprise.Customs.BE.Business.Testing.Messaging.Incoming.MessageProviders.TestFiles.InvalidXSDValidationMessage.txt");

		AssertNoExceptionThrown("Should NOT have Exception even though it has an invalid message type.", () => messageInvalid.GetCachedInboundProvider<Cc556CType, CC556CDataProvider>());
	}

	public void TestGetCachedInboundProvider_MessageIsNull()
	{
		AssertNull(((BEMessage)null).GetCachedInboundProvider<Cc556CType, CC556CDataProvider>());
	}

	public void TestGetCachedInboundProvider_MessageIsNotReceive()
	{
		AssertNull(Factory.New<BEMessage>().GetCachedInboundProvider<Cc556CType, CC556CDataProvider>());
	}

	public void TestGetCachedInboundProvider_Valid()
	{
		var message = Factory.New<BEMessage>();
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message.EM_MessageText = embeddedResourceRetriever.GetString("Enterprise.Customs.BE.Business.Testing.Messaging.Incoming.MessageProviders.TestFiles.ValidXSDValidationMessage.txt");
		var cachedInboundProvider = message.GetCachedInboundProvider<Cc556CType, CC556CDataProvider>();

		CombineAssertions("The value in the provider should be equal to the value we set in the XML.", () =>
		{
			AssertEquals(typeof(CC556CDataProvider), cachedInboundProvider.GetType());
			AssertEquals("22045281480600000001", cachedInboundProvider.LRN);
			AssertEquals("22BEE00000000012J1", cachedInboundProvider.MRN);
			AssertEquals("515", cachedInboundProvider.BusinessRejectionType);
			AssertEquals(new ZDateTime(2022, 4, 1, 12, 34, 56), cachedInboundProvider.RejectionDateAndTime);
			AssertEquals("4", cachedInboundProvider.RejectionCode);
			AssertEquals("invalid", cachedInboundProvider.RejectionReason);
			AssertEquals("cc515c.DepartureTransportMeans(2).typeOfIdentification", cachedInboundProvider.FunctionalErrorList.First().ErrorPointer);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		embeddedResourceRetriever = new EmbeddedResourceRetriever();
	}

	EmbeddedResourceRetriever embeddedResourceRetriever;
}

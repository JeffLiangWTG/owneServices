using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSCancelDeclarationEDIMessage))]
	class CDSCancelDeclarationEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSCancelDeclarationEDIMessage>();
			AssertEquals(CDSEDIMessageTypeList.Codes.CancelDeclaration, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
		}

		public void TestMessageDataObject()
		{
			var message = Factory.New<CDSCancelDeclarationEDIMessage>();
			message.EM_MessageText = new MetaData().SetDeclaration(new CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration()).Serialize();
			var messageDataObject = message.MessageDataObject;
			AssertNotNull("Declaration", messageDataObject.GetDeclaration());
		}
	}
}

using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSNewDeclarationEDIMessage))]
	class CDSNewDeclarationEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSNewDeclarationEDIMessage>();
			AssertEquals(CDSEDIMessageTypeList.Codes.NewDeclaration, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
		}

		public void TestMessageDataObject()
		{
			var message = Factory.New<CDSNewDeclarationEDIMessage>();
			message.EM_MessageText = CDSNewDeclarationEDIMessage.Serialize(new MetaData().SetDeclaration(new CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration()));
			var messageDataObject = message.MessageDataObject;
			AssertNotNull("Declaration", messageDataObject.GetDeclaration());
		}
	}
}

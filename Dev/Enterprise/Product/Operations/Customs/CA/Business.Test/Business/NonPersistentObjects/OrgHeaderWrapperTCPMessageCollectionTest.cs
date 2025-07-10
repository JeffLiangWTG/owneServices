using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(OrgHeaderWrapperTCPMessageCollection))]
	sealed class OrgHeaderWrapperTCPMessageCollectionTest : Enterprise.Messaging.Business.EDIMessageCollectionTest
	{
		public void TestMaster()
		{
			var org = Factory.New<OrgHeader>();
			var wrapper = OrgHeaderTCPMessageWrapper.New(org);
			AssertEquals("Master", org, wrapper.Messages.Master);
		}

		public void TestCreateRelationshipFilter()
		{
			var org = Factory.New<OrgHeader>();

			var orgMessage = Factory.New<EDIMessage>();
			orgMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			orgMessage.EM_MessageType = MessageTypeList.Codes.TradeChainPartner;
			orgMessage.EM_LinkedObject = org;
			orgMessage.EM_MessageText = "message1";

			orgMessage = Factory.New<EDIMessage>();
			orgMessage.EM_MessageType = MessageTypeList.Codes.TradeChainPartner;
			orgMessage.EM_LinkedObject = org;
			orgMessage.EM_MessageText = "message2";

			orgMessage = Factory.New<EDIMessage>();
			orgMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			orgMessage.EM_LinkedObject = org;
			orgMessage.EM_MessageText = "message3";

			orgMessage = Factory.New<EDIMessage>();
			orgMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			orgMessage.EM_MessageType = MessageTypeList.Codes.TradeChainPartner;
			orgMessage.EM_MessageText = "message4";

			var wrapper = OrgHeaderTCPMessageWrapper.New(org);
			AssertNotNull(wrapper.Messages);
			AssertEquals(1, wrapper.Messages.Count);
			AssertEquals("Message Text", "message1", wrapper.Messages[0].EM_MessageText);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgHeaderWrapperTCPMessageCollection(Organisation);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<EDIMessage>();
		}

		OrgHeader Organisation
		{
			get { return org ?? (org = Factory.New<OrgHeader>()); }
		}
		OrgHeader org;
	}
}

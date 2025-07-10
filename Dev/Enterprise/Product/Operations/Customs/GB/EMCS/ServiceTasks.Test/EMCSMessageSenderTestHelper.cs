using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.EMCS.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.EMCS.ServiceTasks.Testing
{
	public static class EMCSMessageSenderTestHelper
	{
		public static EMCSJobDeclaration CreateDeclaration(BusinessObjectFactory factory, Guid branchPK, bool emptyProfile = false)
		{
			var declaration = factory.NewWithValidTestData<EMCSJobDeclaration>();
			declaration.JE_DeclarationReference = "B10000001";
			declaration.JE_CustomsProfile = emptyProfile ? string.Empty : "HYECMT.123456789.EMC";
			declaration.JE_GB = branchPK;
			return declaration;
		}

		public static EMCSOutboundEDIMessage CreateAndPopulateMessage(BusinessObjectFactory factory, EMCSJobDeclaration declaration, ZString messageType)
		{
			var message = factory.New<EMCSOutboundEDIMessage>();
			message.MessageNumberStrategy = new GbMessageNumberStrategy(factory, EDIMessage.ApplicationCodes.GbCustomsEMCS);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsEMCS;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageText = $"<Test {messageType}/>";
			message.EM_MessageType = messageType;
			message.EM_MessageNum = "1";
			message.EM_MessageOwner = "ABC";
			message.EM_LinkedObject = declaration;
			message.EM_GB = declaration.JE_GB;
			return message;
		}
	}
}

using CargoWise.EntityFramework;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class CcsukInventoryMessageBuilder : IMessageBuilder
	{
		public CcsukInventoryMessageBuilder(BusinessObject bizO, CcsukTransmissionMessageFunction how)
		{
			this.bizO = bizO;
			this.cusAwb = bizO as ICcsukCusAwb;
			this.how = how;
			errorCollector = new ErrorCollector();
		}

		public IMessageBuilderResult PopulateMessages()
		{
			var result = new MessageBuilderResult();
			IBuilderResult builderResult = PopulateMessage();
			if (builderResult != null)
			{
				result.AddBuilderResult(builderResult);
			}
			return result;
		}

		IBuilderResult PopulateMessage()
		{
			var generator = CusAwbToInventoryMessageGenerator.New(how, cusAwb, errorCollector);
			var messageText = generator.MakeMessageText();
			var result = new BuilderResult(bizO, errorCollector.GetErrors(), AfterFullSuccess);
			result.Message = generator.GetNewMessage();
			result.Message.MessageNumberStrategy = new GbMessageNumberStrategy(generator.Factory, ApplicationCodeList.Codes.GbCcsuk);
			result.Message.EM_MessageText = messageText;
			result.Message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			result.Message.EM_MessageType = how.MessageType;
			result.Message.EM_MessageSubType = how.MessageSubType;
			result.Message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			result.Message.EM_MessageInterpretation = generator.MessageInterpretation;
			result.Message.EM_MessageOwner = generator.SenderPima; // sender's PIMA
			result.Message.EM_ApplicationReference = generator.RecipientPima;  // recipient is based on type of message being generated
			result.Message.Saving += message_Saving;
			result.Message.Saved += message_Saved;

			return result;
		}

		public virtual void AfterFullSuccess(IBuilderResult builderResult)
		{
			// This puts the messag'es PK into SYS-CAR.  Compare this to Chief messaging, where we put the Entry's PK in.  
			// The latter is not satisfacotry because from an entry we then have to find the last message, which is a PITA.
			// If we find the message from the SYS-CAR, finding the bizO owner is trivial. 
			builderResult.Message.EM_MessageText = GbTransmissionMessageGenerator.PutBizoPkIntoSysCarPlaceholder(builderResult.Message.EM_MessageText, builderResult.Message);
		}

		void message_Saved(EDIMessage message, bool saveSucceeded)
		{ }

		protected virtual void message_Saving(EDIMessage message)
		{ }

		readonly ErrorCollector errorCollector;
		readonly ICcsukCusAwb cusAwb;
		readonly BusinessObject bizO;
		readonly CcsukTransmissionMessageFunction how;
	}
}

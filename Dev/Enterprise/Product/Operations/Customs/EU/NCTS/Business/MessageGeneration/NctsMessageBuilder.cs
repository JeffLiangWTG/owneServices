using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.EU.NCTS.Business.MessageGeneration
{
	public class NctsMessageBuilder : IMessageBuilder
	{
		public NctsMessageBuilder(NctsHeader nctsHeader, IMessageGenerator<NctsHeader> generator)
		{
			this.generator = generator;
			this.nctsHeader = nctsHeader;
		}

		#region IMessageBuilder Members

		public IMessageBuilderResult PopulateMessages()
		{
			var result = new MessageBuilderResult();
			var builderResult = PopulateMessage(nctsHeader);
			if (builderResult != null)
			{
				result.AddBuilderResult(builderResult);
			}
			return result;
		}

		#endregion

		#region Implementation

		IBuilderResult PopulateMessage(NctsHeader nctsHeader)
		{
			var result = generator.Generate(nctsHeader);
			if (result != null && result.Message != null)
			{
				result.Message.Saving += message_Saving;
				result.Message.Saved += message_Saved;
			}
			return result;
		}

		void message_Saved(EDIMessage message, bool saveSucceeded)
		{
			//TODO
		}

		void message_Saving(EDIMessage message)
		{
			generator.PutReferenceNumberIntoMessageFromPlaceholder(message, message.EM_MessageText, nctsHeader);
			message.EM_MessageInterpretation = generator.MakePrettyForInterpretation(message);
		}

		#endregion

		readonly NctsHeader nctsHeader;
		readonly IMessageGenerator<NctsHeader> generator;
	}
}

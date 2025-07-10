using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Messaging.Business.MessageBuilders;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public class CADMessageBuilder : IMessageBuilder
	{
		public CADMessageBuilder(IEDIFACTMessageAttachee dataWrapper, ZString messageSubType)
		{
			this.dataWrapper = dataWrapper;
			if (dataWrapper is CADMessageWrapper wrapper)
			{
				this.cadDocumentMetaData = wrapper.CADDocumentMetaData;
			}
			this.messageSubType = messageSubType;
		}

		readonly IEDIFACTMessageAttachee dataWrapper;
		readonly ICADMessageMetaData cadDocumentMetaData;
		readonly ZString messageSubType;

		#region IMessageBuilder

		IMessageBuilderResult IMessageBuilder.PopulateMessages()
		{
			var messageBuilderResult = new MessageBuilderResult();
			var builderResult = new BuilderResult(null, System.Array.Empty<string>(), null);
			var message = dataWrapper.Factory.New<CADMessage>();
			message.EM_MessageSubType = messageSubType;
			message.EM_MessageText = CargoWise.Customs.CA.MessageContracts.CAD.CADMessageBuilder.SerializeToMessageString(cadDocumentMetaData);
			builderResult.Message = message;
			messageBuilderResult.AddBuilderResult(builderResult);
			return messageBuilderResult;
		}

		#endregion
	}
}

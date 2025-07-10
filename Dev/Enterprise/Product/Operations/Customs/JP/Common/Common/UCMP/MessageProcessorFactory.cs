using CargoWise.Application;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.JP;

namespace Enterprise.Customs.JP.Common;

public sealed class MessageProcessorFactory
{
	public static NACCSMessageProcessor GetMessageProcessor(Messaging.Business.EDIMessage message, LoggingInformation logger)
	{
		if (message.EM_MessageType == JPMessageTypes.Codes.XER)
		{
			return new ErrorMessageProcessor(logger);
		}
		else
		{
			switch (message.EM_LinkTable)
			{
				case CusEntryHeaderSchema.Constants.TableName:
					return (NACCSMessageProcessor)ObjectFactory.Get<IDeclarationMessageProcessor>($"JP.{nameof(IDeclarationMessageProcessor)}", logger);

				case AsycudaManifestHeaderSchema.Constants.TableName:
					return (NACCSMessageProcessor)ObjectFactory.Get<IManifestHeaderMessageProcessor>($"JP.{nameof(IManifestHeaderMessageProcessor)}", logger);
			}
		}

		logger.LogWarning($"Can't find a valid message processor for the message - {message.EM_MessageNum}.");
		return null;
	}
}

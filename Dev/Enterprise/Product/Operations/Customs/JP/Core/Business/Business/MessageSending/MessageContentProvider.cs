using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Business;

public sealed class MessageContentProvider : IMessageContentProvider
{
	public MessageContentProvider(BusinessObjectFactory factory, JobDeclarationMessageSendingObject sendingObject)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		SendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
	}

	readonly BusinessObjectFactory factory;

	public JobDeclarationMessageSendingObject SendingObject { get; }

	BusinessObjectFactory IMessageContentProvider.Factory => factory;

	ZString IMessageContentProvider.ProcedureCode => SendingObject.GetProcedureCode();

	byte[] IMessageContentProvider.GetMessageData() => NACCSMessageBuilder.BuildNACCSMessage(SendingObject);
}

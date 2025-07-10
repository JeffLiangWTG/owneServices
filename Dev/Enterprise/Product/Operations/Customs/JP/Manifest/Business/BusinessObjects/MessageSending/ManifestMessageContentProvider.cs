using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Manifest.Business;

sealed class ManifestMessageContentProvider : IMessageContentProvider
{
	public ManifestMessageContentProvider(BusinessObjectFactory factory, IEnumerable<ManifestMessageSendingObject> sendingObjects)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		SendingObjects = Argument.NotNull(sendingObjects, nameof(sendingObjects));
	}

	readonly BusinessObjectFactory factory;

	public IEnumerable<ManifestMessageSendingObject> SendingObjects { get; }

	BusinessObjectFactory IMessageContentProvider.Factory => factory;

	ZString IMessageContentProvider.ProcedureCode => SendingObjects.FirstOrDefault()?.MessageType ?? ZString.Empty;

	byte[] IMessageContentProvider.GetMessageData() => ManifestNACCSMessageBuilder.BuildNACCSMessage(SendingObjects);
}

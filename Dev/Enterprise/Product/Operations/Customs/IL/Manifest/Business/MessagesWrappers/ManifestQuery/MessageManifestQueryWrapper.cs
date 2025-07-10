using System;
using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_820;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	sealed class MessageManifestQueryWrapper : IMessageManifestQuery
	{
		MessageManifestQueryWrapper(AsycudaManifestQueryMessageSendingObject messageSendingObject)
		{
			this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		}

		internal static IMessageManifestQuery NewOrNull(AsycudaManifestQueryMessageSendingObject messageSendingObject)
			=> messageSendingObject == null ? null : new MessageManifestQueryWrapper(messageSendingObject);

		public ICargoIdentifier CargoIdentifier => ManifestQueryCargoIdentifierWrapper.NewOrNull(messageSendingObject);

		public string ContainerNumber => null;

		public string DocumentNumber => null;

		public string EntrySiteId => null;

		public string ExitSiteId => null;

		public DateTime? FromEntryExitDateTime => null;

		public string ReferenceNum => null;

		public int? ReferenceType => null;

		public IRequestContentHeader RequestContentHeader => RequestContentHeaderWrapper.New();

		public DateTime? ToEntryExitDateTime => null;

		readonly AsycudaManifestQueryMessageSendingObject messageSendingObject;
	}
}

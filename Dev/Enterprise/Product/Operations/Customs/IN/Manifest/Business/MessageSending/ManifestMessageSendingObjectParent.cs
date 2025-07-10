using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.IN.Manifest.Business;

public class ManifestMessageSendingObjectParent : BaseMessageSendingObjectParent<ManifestMessageSendingObject>, IMessageSendingObjectParent
{
	public ManifestMessageSendingObjectParent(AsycudaManifestHeader asycudaManifestHeader) : base(asycudaManifestHeader?.Factory)
	{
		ManifestHeader = Argument.NotNull(asycudaManifestHeader, nameof(asycudaManifestHeader));
	}

	public readonly AsycudaManifestHeader ManifestHeader;

	public override BusinessObject TopLevelBusinessObject => ManifestHeader;

	public override Security.SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.GlobalManifestSendWithMessageErrors;

	public EDIMessage[] SendAndSaveMessages(MessageSendingContext context)
	{
		var messageTypeKey = CreateMessageTypeKey(ManifestHeader.AMA_ManifestType, ManifestHeader.AMA_TransportMode);
		return ManifestMessageSenderProviders.TryGetValue(messageTypeKey, out var sender) ? this.SendAndSaveMessages(sender, context) : Array.Empty<EDIMessage>();
	}

	public ZString ValidateBeforeSend() => ManifestHeader.AMA_CustomsOffice.IsEmpty ? Res.GetString("B4501663-363F-4588-BEB6-BA7C160F880F", "Please enter a Customs Office to proceed with CGM message sending.") : ZString.Empty;

	protected override NonPersistentBusinessObjectCollection<ManifestMessageSendingObject> GetSendingObjectsCollectionCore()
	{
		return new ManifestMessageSendingObjectCollection(Factory)
		{
			new ManifestMessageSendingObject(ManifestHeader)
		};
	}

	Dictionary<string, Func<ManifestMessageSendingObject, IMessageSender>> ManifestMessageSenderProviders => new ()
	{
		{ CreateMessageTypeKey(INManifestTypes.Codes.CGM, Core.Constants.TransportModes.Sea), x => new SeaCgmCMCHI21MessageSender(x) },
		{ CreateMessageTypeKey(INManifestTypes.Codes.CGM, Core.Constants.TransportModes.Air), x => new AirCgmCMCHI01MessageSender(x) }
	};

	string CreateMessageTypeKey(ZString manifestType, ZString transportMode)
	{
		return string.Concat(manifestType, transportMode);
	}
}

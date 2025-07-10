using System.ComponentModel;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaManifestMessageSendingObject : AutoAsycudaManifestMessageSendingObject
	{
		public AsycudaManifestMessageSendingObject(AsycudaManifestHeader header)
			: base(header.Factory)
		{
			Header = Argument.NotNull(header, nameof(header));
		}

		public readonly AsycudaManifestHeader Header;

		public AsycudaManifestMessageSendingObjectLookups Lookups => lookups ??= new AsycudaManifestMessageSendingObjectLookups(this);
		AsycudaManifestMessageSendingObjectLookups lookups;

		[ReadOnly(true)]
		public override ZString MessageType { get => base.MessageType; set => base.MessageType = value; }

		[ReadOnly(true)]
		public override ZString MessageSubType { get => base.MessageSubType; set => base.MessageSubType = value; }

		public override ZString MessageSubTypeDescription => Lookups.MessageSubTypes.GetDescriptionFromCode(MessageSubType);
	}
}
